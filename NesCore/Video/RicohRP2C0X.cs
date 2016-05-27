using NesCore.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Video
{
    public class RicohRP2C0X
    {
        /// <summary>
        /// delegate for reading a byte from a given address within main memory
        /// </summary>
        /// <param name="address">16bit main memory address of the byte to read</param>
        /// <returns></returns>
        public delegate byte ReadByteHandler(ushort address);

        // delegate for writing pixel in a frame buffer implementation
        public delegate void WritePixelHandler(byte x, byte y, Colour colour);

        /// <summary>
        /// Constructs a new PPU
        /// </summary>
        public RicohRP2C0X()
        {
            Memory = new ConfigurableMemoryMap(0x10000);
            paletteTints = new PaletteTints();

            // VRM is actually $4000, but is configured to wrap around whole addressable space
            Memory.ConfigureAddressMirroring(0x000, 0x4000, 0x10000);

            // $0000 - $1FFF mapped to CHR when cartridge loaded

            // $2000 - $3EFF mirroring configured when cartridge loaded

            // $3F00 - $3FFF - palette
            Memory.ConfigureMemoryAccessRange(0x3F00, 0x0100,
                (address) => ReadPalette((ushort)(address % 0x20)),
                (address, value) => WritePalette((ushort)(address % 0x20), value));

            Reset();
        }

        public ConfigurableMemoryMap Memory { get; private set; }

        // implementation hooks

        public Action TriggerNonMaskableInterupt { get; set; }

        /// <summary>
        /// Main memory read hook
        /// </summary>
        public ReadByteHandler ReadByte { get; set; }

        /// <summary>
        /// Pixel computation hoook
        /// </summary>
        public WritePixelHandler WritePixel { get; set; }

        /// <summary>
        /// Frame presentation hook
        /// </summary>
        public Action ShowFrame { get; set; }

        /// <summary>
        /// Control register ($2000 PPUCTRL)
        /// </summary>
        public byte Control
        {
            get
            {
                return registerLatch;
            }
            set
            {
                registerLatch = value;
                nameTable = (byte)(value & 0x03);
                vramIncrement = (value & 0x04) != 0 ? VramIncrement.Down : VramIncrement.Across;
                spritePatternTable = (byte)((value & 0x08) != 0 ? 1 : 0);
                backgroundPatternTableAddress = (ushort)((value & 0x10) != 0 ? 0x1000 : 0x0000);
                spriteSize = (value & 0x20) != 0 ? SpriteSize.Size8x16 : SpriteSize.Size8x8;
                masterSlave = (value & 0x40) != 0;
                nmiOutput = (value & 0x80) != 0;

                NmiChange();
                // t: ....BA.. ........ = d: ......BA
                tempAddress = (ushort)((tempAddress & 0xF3FF) | ((value & 0x03) << 10));
            }
        }

        /// <summary>
        /// Mask register ($2001 PPUMASK(
        /// </summary>
        public byte Mask
        {
            get
            {
                return registerLatch;
            }
            set
            {
                registerLatch = value;
                grayscale = (value & 0x01) != 0;
                showLeftBackground = (value & 0x02) != 0;
                showLeftSprites = (value & 0x04) != 0;
                showBackground = (value & 0x08) != 0;
                showSprites = (value & 0x10) != 0;
                tint = (byte)(value >> 5);
            }
        }

        /// <summary>
        /// Status register ($2002 PPUSTATUS)
        /// </summary>
        public byte Status
        {
            get
            {
                byte result = (byte)(registerLatch & 0x1F);
                if (spriteOverflow)
                    result |= 0x20;
                if (spriteZeroHit)
                    result |= 0x40;
                if (nmiOccurred)
                    result |= 0x80;

                nmiOccurred = false;
                NmiChange();
                writeToggle = WriteToggle.First;
                return result;
            }
            set
            {
                registerLatch = value;
            }
        }

        /// <summary>
        /// Object attribute memory address ($2003 OAMADDR)
        /// </summary>
        public byte ObjectAttributeMemoryAddress
        {
            get
            {
                return registerLatch;
            }
            set
            {
                registerLatch = value;
                oamAddress = value;
            }
        }

        /// <summary>
        /// Object attribute memory data ($2004 OAMDATA).
        /// Reads from current OAM address, writes cause OAM address to advance
        /// </summary>
        public byte ObjectAttributeMemoryData
        {
            get
            {
                return oamData[oamAddress];
            }
            set
            {
                registerLatch = value;
                oamData[oamAddress++] = value;
            }
        }

        /// <summary>
        /// Scrolling position register ($2005 PPUSCROLL)
        /// Accepts two sequential writes
        /// </summary>
        public byte Scroll
        {
            get
            {
                return registerLatch;
            }
            set
            {
                registerLatch = value;
                if (writeToggle == WriteToggle.First)
                {
                    // t: ........ ...HGFED = d: HGFED...
                    // x:               CBA = d: .....CBA
                    tempAddress = (ushort)((tempAddress & 0xFFE0) | (value >> 3));
                    scrollX = (byte)(value & 0x07);
                    writeToggle = WriteToggle.Second;
                }
                else
                {
                    // t: .CBA..HG FED..... = d: HGFEDCBA
                    tempAddress = (ushort)((tempAddress & 0x8FFF) | ((value & 0x07) << 12));
                    tempAddress = (ushort)((tempAddress & 0xFC1F) | ((value & 0xF8) << 2));
                    writeToggle = WriteToggle.First;
                }
            }
        }

        /// <summary>
        /// Address register ($2006 PPUADDR)
        /// Two writes required in high-low byte order
        /// </summary>
        public byte Address
        {
            get
            {
                return registerLatch;
            }
            set
            {
                registerLatch = value;
                if (writeToggle == WriteToggle.First)
                {
                    // write high address byte
                    // t: ..FEDCBA ........ = d: ..FEDCBA
                    // t: .X...... ........ = 0
                    tempAddress = (ushort)((tempAddress & 0x80FF) | ((value & 0x3F) << 8));
                    writeToggle = WriteToggle.Second;
                }
                else
                {
                    // write low address byte
                    // t: ........ HGFEDCBA = d: HGFEDCBA
                    // v                    = t
                    tempAddress = (ushort)((tempAddress & 0xFF00) | value);
                    vramAddress = tempAddress;
                    writeToggle = WriteToggle.First;
                }
            }
        }

        // $2007: PPUDATA (read)
        /// <summary>
        /// Data register ($2007 PPUDATA)
        /// </summary>
        public byte Data
        {
            get
            {
                byte value = Memory[vramAddress];
                // emulate buffered reads
                if (vramAddress % 0x4000 < 0x3F00)
                {
                    byte buffered = bufferedData;
                    bufferedData = value;
                    value = buffered;
                }
                else
                {
                    bufferedData = Memory[(ushort)(vramAddress - 0x1000)];
                }

                // increment address
                if (vramIncrement == VramIncrement.Down)
                    vramAddress += 0x20;
                else
                    vramAddress += 0x01;

                return value;
            }
            set
            {
                registerLatch = value;
                Memory[vramAddress]  = value;

                // increment address
                if (vramIncrement == VramIncrement.Down)
                    vramAddress += 0x20;
                else
                    vramAddress += 0x01;
            }
        }

        /// <summary>
        /// Direct memory access write for object attribute memory ($4014 OAMDMA)
        /// Write-only high byte (page) address for data to copy into the OAM
        /// </summary>
        public byte ObjectAttributeDirectMemoryAccess
        {
            get
            {
                return registerLatch;
            }
            set
            {
                registerLatch = value;

                ushort address = (ushort)(value << 8);

                for (int i = 0; i < 256; i++)
                    oamData[oamAddress++] = ReadByte(address++);
            }
        }

        /// <summary>
        /// Configures name table data mirroring mode
        /// </summary>
        /// <param name="mirrorMode">mirroring mode from cartridge ROM</param>
        public void ConfigureNameTableMirroringMode(byte mirrorMode)
        {
            // $2000-$3EFF
            Memory.ConfigureMemoryAccessRange(0x2000, 0x1F00,
                (address) => nameTableData[MirrorAddress(mirrorMode, address) % 2048],
                (address, value) => nameTableData[MirrorAddress(mirrorMode, address) % 2048] = value);
        }

        /// <summary>
        /// resets the PPU
        /// </summary>
        public void Reset()
        {
            cycle = 340;
            scanLine = 240;

            Control = 0x00;
            Mask = 0x00;
            ObjectAttributeMemoryAddress = 0x00;

            Memory.Wipe(0x2000, 0x1000);
        }

        // Step executes a single PPU cycle
        public void Step()
        {
            Tick();

            bool renderingEnabled = showBackground || showSprites;
            bool preLine = scanLine == 261;
            bool visibleLine = scanLine < 240;
            // postLine := ppu.ScanLine == 240
            bool renderLine = preLine || visibleLine;

            bool preFetchCycle = cycle >= 321 && cycle <= 336;
            bool visibleCycle = cycle >= 1 && cycle <= 256;
            bool fetchCycle = preFetchCycle || visibleCycle;

            // background logic
            if (renderingEnabled)
            {
                if (visibleLine && visibleCycle)
                    RenderPixel();

                if (renderLine && fetchCycle)
                {
                    tileData <<= 4;

                    switch (cycle % 8)
                    {
                        case 1:
                            FetchNameTableByte();
                            break;
                        case 3:
                            FetchAttributeTableByte();
                            break;
                        case 5:
                            FetchLowTileByte();
                            break;
                        case 7:
                            FetchHighTileByte();
                            break;
                        case 0:
                            StoreTileData();
                            break;
                    }
                }

                if (preLine && cycle >= 280 && cycle <= 304)
                    CopyY();

                if (renderLine)
                {
                    if (fetchCycle && cycle % 8 == 0)
                        IncrementX();

                    if (cycle == 256)
                        IncrementY();

                    if (cycle == 257)
                        CopyX();
                }
            }

            // sprite logic
            if (renderingEnabled)
            {
                if (cycle == 257)
                {
                    if (visibleLine)
                        EvaluateSprites();
                    else
                        spriteCount = 0;
                }
            }

            // vblank logic
            if (scanLine == 241 && cycle == 1)
                SetVerticalBlank();

            if (preLine && cycle == 1)
            {
                ClearVerticalBlank();
                spriteZeroHit = false;
                spriteOverflow = false;
            }
        }

        /// <summary>
        /// Saves the state of the PPU
        /// </summary>
        /// <param name="streamWriter"></param>
        public void Save(StreamWriter streamWriter)
        {
            streamWriter.Write(cycle);
            streamWriter.Write(scanLine);
            streamWriter.Write(paletteData);
            streamWriter.Write(nameTableData);
            streamWriter.Write(oamData);
            streamWriter.Write(vramAddress);
            streamWriter.Write(tempAddress);
            streamWriter.Write(scrollX);
            streamWriter.Write(writeToggle);
            streamWriter.Write(evenFrame);
            streamWriter.Write(nmiOccurred);
            streamWriter.Write(nmiOutput);
            streamWriter.Write(nmiPrevious);
            streamWriter.Write(nmiDelay);
            streamWriter.Write(nameTableByte);
            streamWriter.Write(attributeTableByte);
            streamWriter.Write(lowTileByte);
            streamWriter.Write(highTileByte);
            streamWriter.Write(tileData);
            streamWriter.Write(spriteCount);
            streamWriter.Write(spritePatterns);
            streamWriter.Write(spritePositions);
            streamWriter.Write(spritePriorities);
            streamWriter.Write(spriteIndexes);
            streamWriter.Write(nameTable);
            streamWriter.Write(vramIncrement);
            streamWriter.Write(spritePatternTable);
            streamWriter.Write(backgroundPatternTableAddress);
            streamWriter.Write(spriteSize);
            streamWriter.Write(masterSlave);
            streamWriter.Write(grayscale);
            streamWriter.Write(showLeftBackground);
            streamWriter.Write(showLeftSprites);
            streamWriter.Write(showBackground);
            streamWriter.Write(showSprites);
            streamWriter.Write(tint);
            streamWriter.Write(spriteZeroHit);
            streamWriter.Write(spriteOverflow);
	        streamWriter.Write(oamAddress);
	        streamWriter.Write(bufferedData);
        }

        /// <summary>
        /// Restores the state of the PPU
        /// </summary>
        /// <param name="streamReader"></param>
        public void Load(StreamReader streamReader)
        {
            throw new NotImplementedException();
        }

        private byte ReadPalette(ushort address)
        {
            if (address >= 16 && address % 4 == 0)
                address -= 16;
            return paletteData[address];
        }

        private void WritePalette(ushort address, byte value)
        {
            if (address >= 16 && address % 4 == 0)
                address -= 16;
            paletteData[address] = value;
        }
        
        // NTSC Timing Helper Functions
        private void IncrementX()
        {
            // increment hori(v)
            // if coarse X == 31
            if ((vramAddress & 0x001F) == 0x1F)
            {
                // coarse X = 0
                vramAddress &= 0xFFE0;
                // switch horizontal nametable
                vramAddress ^= 0x0400;
            }
            else
            {
                // increment coarse X
                ++vramAddress;
            }
        }

        private void IncrementY()
        {
            // increment vert(v)
            // if fine Y < 7
            if ((vramAddress & 0x7000) != 0x7000)
            {
                // increment fine Y
                vramAddress += 0x1000;
            }
            else
            {
                // fine Y = 0
                vramAddress &= 0x8FFF;
                // let y = coarse Y
                int y = (vramAddress & 0x03E0) >> 5;
                      
                if (y == 29)
                {
                    // coarse Y = 0
                    y = 0;
                    // switch vertical nametable
                    vramAddress ^= 0x0800;
                }
                else if (y == 31)
                {
                    // coarse Y = 0, nametable not switched
                    y = 0;
                }
                else
                {
                    // increment coarse Y
                    ++y;
                }

                // put coarse Y back into v
                vramAddress = (ushort)((vramAddress & 0xFC1F) | (y << 5));
            }
        }

        private void CopyX()
        {
            // hori(v) = hori(t)
            // v: .....F.. ...EDCBA = t: .....F.. ...EDCBA
            vramAddress = (ushort)((vramAddress & 0xFBE0) | (tempAddress & 0x041F));
        }

        private void CopyY()
        {
            // vert(v) = vert(t)
            // v: .IHGF.ED CBA..... = t: .IHGF.ED CBA.....
            vramAddress = (ushort)((vramAddress & 0x841F) | (tempAddress & 0x7BE0));
        }

        private void NmiChange()
        {
            bool nmi = nmiOutput && nmiOccurred;

            if (nmi && !nmiPrevious)
            {
                // TODO: this fixes some games but the delay shouldn't have to be so
                // long, so the timings are off somewhere
                nmiDelay = 15;
            }
            nmiPrevious = nmi;
        }

        private void SetVerticalBlank()
        {
            //ppu.front, ppu.back = ppu.back, ppu.front

            nmiOccurred = true;
            NmiChange();

            // call hook to present frame on vblank
            ShowFrame();
        }

        private void ClearVerticalBlank()
        {
            nmiOccurred = false;
            NmiChange();
        }

        private void FetchNameTableByte()
        {
            ushort address = (ushort)(0x2000 | (vramAddress & 0x0FFF));
            nameTableByte = Memory[address];
        }

        private void FetchAttributeTableByte()
        {
            ushort address = (ushort)(0x23C0 | (vramAddress & 0x0C00) | ((vramAddress >> 4) & 0x38) | ((vramAddress >> 2) & 0x07));
            int shift = ((vramAddress >> 4) & 4) | (vramAddress & 2);
            attributeTableByte = (byte)(((Memory[address] >> shift) & 3) << 2);
        }

        private void FetchLowTileByte()
        {
            int fineY = (vramAddress >> 12) & 7;
            ushort address = (ushort)(backgroundPatternTableAddress + nameTableByte * 16 + fineY);
            lowTileByte = Memory[address];
        }

        private void FetchHighTileByte()
        {
            int fineY = (vramAddress >> 12) & 7;
            ushort address = (ushort)(backgroundPatternTableAddress + nameTableByte * 16 + fineY + 8);
            highTileByte = Memory[address];
        }

        private void StoreTileData()
        {
            uint data = 0;

            for (int i = 0; i < 8; i++)
            {
                int p1 = (lowTileByte & 0x80) >> 7;
                int p2 = (highTileByte & 0x80) >> 6;

                lowTileByte <<= 1;
                highTileByte <<= 1;
                data <<= 4;

                data |= (uint)(attributeTableByte | p1 | p2);

            }

            tileData |= (ulong)(data);
        }

        private uint FetchTileData()
        {
            return (uint)(tileData >> 32);
        }

        private byte GetBackgroundPixel()
        {
	        if (!showBackground)
                return 0;

            uint data = FetchTileData() >> ((7 - scrollX) * 4);
            return (byte)(data & 0x0F);
        }

        private byte GetSpritePixel(out byte spriteIndex)
        {
            spriteIndex = 0;
            if (!showSprites)
                return 0;

	        for (int i = 0; i < spriteCount; i++)
            {
                int offset = (cycle - 1) - spritePositions[i];
                if (offset < 0 || offset > 7)
                    continue;

                offset = 7 - offset;
                byte colour = (byte)((spritePatterns[i] >> (offset * 4)) & 0x0F);
                if (colour % 4 == 0)
                    continue;

                spriteIndex = (byte)i;
		        return colour;
            }

            spriteIndex = 0;
            return 0;
        }

        private void RenderPixel()
        {
            byte x = (byte)(cycle - 1);
            byte y = (byte)scanLine;

            byte backgroundPixel = GetBackgroundPixel();

            byte spriteIndex = 0;
            byte spritePixel = GetSpritePixel(out spriteIndex);

            if (x < 8 && backgroundPatternTableAddress == 0x0000)
                backgroundPixel = 0;

            if (x < 8 && !showLeftSprites)
                spritePixel = 0;

            bool opaqueBackground = backgroundPixel % 4 != 0;

            bool opaqueSprite = spritePixel % 4 != 0;

            byte colourIndex = 0;

            if (opaqueBackground)
            {
                // opaque background pixel
                if (opaqueSprite)
                {
                    // opaque background and sprite pixels

                    // check sprite 0 hit
                    if (spriteIndexes[spriteIndex] == 0 && x < 255)
                        spriteZeroHit = true;

                    // determine if sprite or backgroubnd pixel prevails
                    if (spritePriorities[spriteIndex] == 0)
                        colourIndex = (byte)(spritePixel | 0x10);
                    else
                        colourIndex = backgroundPixel;
                }
                else
                {
                    // opaque background and transparent sprite pixel
                    colourIndex = backgroundPixel;
                }
            }
            else
            {
                // transparent backgorund
                if (opaqueSprite)
                {
                    // transparent background and opaque sprite pixels
                    colourIndex = (byte)(spritePixel | 0x10);
                }
                else
                {
                    // transparent background and sprite pixels
                    colourIndex = 0;
                }
            }

            // get palette index from colour index
            byte paletteIndex = ReadPalette((ushort)(colourIndex % 64));

            // check if grayscale bit applies
            // note that palette index encodes luminance (4 levels - high nibble 0000-0011) and hue (low nibble 0001-1100, 0000 lighter gray, 1101 darker gray)
            // therefore grayscale can be obtained by masking out low nibble, mapping $00-0F to $00, $10-$1F to $10, etc.
            if (grayscale)
                paletteIndex &= 0xF0;

            // get colour
            Colour colour = paletteTints[tint][paletteIndex];

            // apply tinting if needed
            //if (redTint || greenTint || blueTint)
            //    colour = colour.Emphasise(redTint, greenTint, blueTint);

            // hook to write pixel
            WritePixel(x, y, colour);
        }

        private uint FetchSpritePattern(int spriteIndex, int row)
        {
            byte tile = oamData[spriteIndex * 4 + 1];
            byte attributes = oamData[spriteIndex * 4 + 2];
            ushort address = 0;
            if (spriteSize == SpriteSize.Size8x8)
            {
                if ((attributes & 0x80) == 0x80)
                    row = 7 - row;

                address = (ushort)(0x1000 * spritePatternTable + tile * 16 + row);
            }
            else // 8x16
            {
		        if ((attributes & 0x80) == 0x80)
                    row = 15 - row;

                int table = tile & 1;

                tile &= 0xFE;
		        if (row > 7)
                {
                    ++tile;
                    row -= 8;
		        }
                address = (ushort)(0x1000 * table + tile * 16 + row);
            }

            byte a = (byte)((attributes & 3) << 2);
            lowTileByte = Memory[address];
            highTileByte = Memory[(ushort)(address + 8)];

            uint data = 0;

            byte p1 = 0, p2 = 0;
            for (int spriteRow = 0; spriteRow < 8; spriteRow++)
            {
                if ((attributes & 0x40) == 0x40)
                {
                    p1 = (byte)((lowTileByte & 1) << 0);
                    p2 = (byte)((highTileByte & 1) << 1);
                    lowTileByte >>= 1;
                    highTileByte >>= 1;
                }
                else
                {
                    p1 = (byte)((lowTileByte & 0x80) >> 7);
                    p2 = (byte)((highTileByte & 0x80) >> 6);
                    lowTileByte <<= 1;
                    highTileByte <<= 1;
                }
                data <<= 4;
                data |= (uint)(a | p1 | p2);
            }
            return data;
        }

        private void EvaluateSprites()
        {
            int spriteHeight = spriteSize == SpriteSize.Size8x16 ? 16 : 8;

            int count = 0;
            for (int index = 0; index < 64; index++)
            {
                byte spriteY = oamData[index * 4 + 0];
                byte spriteAttributes = oamData[index * 4 + 2];
                byte spriteX = oamData[index * 4 + 3];

                int row = scanLine - spriteY;

                if (row < 0 || row >= spriteHeight)
                    continue;

                if (count < 8)
                {
                    spritePatterns[count] = FetchSpritePattern(index, row);
                    spritePositions[count] = spriteX;
                    spritePriorities[count] = (byte)((spriteAttributes >> 5) & 1);
                    spriteIndexes[count] = (byte)index;
                }
                ++count;
            }

            if (count > 8)
            {
                count = 8;
                spriteOverflow = true;
            }
            spriteCount = count;
        }

        // tick updates Cycle, ScanLine and Frame counters
        private void Tick()
        {
            if (nmiDelay > 0)
            {
                --nmiDelay;
        
                if (nmiDelay == 0 && nmiOutput && nmiOccurred)
                {
                    TriggerNonMaskableInterupt();
                }
            }

            if (showBackground || showSprites)
            {
                if (evenFrame && scanLine == 261 && cycle == 339)
                {
                    cycle = 0;
                    scanLine = 0;
                    evenFrame = false;
                    return;
                }
            }
            ++cycle;

            if (cycle > 340)
            {
                cycle = 0;
                ++scanLine;

                if (scanLine > 261)
                {
                    scanLine = 0;
                    evenFrame = !evenFrame;
                }
            }
        }

        private ushort MirrorAddress(byte mode, ushort address)
        {
            address = (ushort)((address - 0x2000) % 0x1000);
            int table = address / 0x0400;
            int offset = address % 0x0400;
            return (ushort)(0x2000 + mirrorLookup[mode][table] * 0x0400 + offset);
        }

        private int cycle; // 0-340
        private int scanLine; // 0-261, 0-239=visible, 240=post, 241-260=vblank, 261=pre

        // storage variables
        private byte[] paletteData = new byte[32];
        private byte[] nameTableData = new byte[2048];
        private byte[] oamData = new byte[256];

        // PPU registers
        private ushort vramAddress;         // current vram address (15 bit)
        private ushort tempAddress;         // temporary vram address (15 bit)
        private byte scrollX;               // fine x scroll (3 bit)
        private WriteToggle writeToggle;    // write toggle (1 bit)
        private bool evenFrame;             // even/odd frame
        private byte registerLatch;         // status register
        
        // NMI flags
        private bool nmiOccurred;
        private bool nmiOutput;
        private bool nmiPrevious;
        private byte nmiDelay;

        // background temporary variables
        private byte nameTableByte;
        private byte attributeTableByte;
        private byte lowTileByte;
        private byte highTileByte;
        private ulong tileData;

        // sprite temporary variables
        private int spriteCount;
        private uint[] spritePatterns = new uint[8];
        private byte[] spritePositions = new byte[8];
        private byte[] spritePriorities = new byte[8];
        private byte[] spriteIndexes = new byte[8];

        // $2000 PPUCTRL
        private byte nameTable;                         // 0: $2000; 1: $2400; 2: $2800; 3: $2C00
        private VramIncrement vramIncrement;            // Across: add 1; Down: add 32
        private byte spritePatternTable;                // 0: $0000; 1: $1000; ignored in 8x16 mode
        private ushort backgroundPatternTableAddress;   // 0: $0000; 1: $1000
        private SpriteSize spriteSize;                  // 8x8 or 8x16 pixels
        private bool masterSlave;                       // false: read EXT; true: write EXT

        // $2001 PPUMASK
        private bool grayscale;             // true: gray scale mode; false: colour mode
        private bool showLeftBackground;    // false: hide;   true: show
        private bool showLeftSprites;       // false: hide;   true: show
        private bool showBackground;        // false: hide;   true: show
        private bool showSprites;           // false: hide;   true: show
        private byte tint;                  // 000 - 111 binary showing colour emphasis in BGR order 

        // $2002 PPUSTATUS
        private bool spriteZeroHit;
        private bool spriteOverflow;

        // $2003 OAMADDR
        private byte oamAddress;

        // $2007 PPUDATA
        private byte bufferedData; // for buffered reads

        private PaletteTints paletteTints;

        private static readonly ushort[][] mirrorLookup = {
            new ushort[]{0, 0, 1, 1},
            new ushort[]{0, 1, 0, 1},
            new ushort[]{0, 0, 0, 0},
            new ushort[]{1, 1, 1, 1},
            new ushort[]{0, 1, 2, 3}
        };

        private enum WriteToggle
        {
            First,
            Second
        }

        private enum SpriteSize
        {
            Size8x8,
            Size8x16
        }

        private enum VramIncrement
        {
            Across,
            Down
        }
    }

}
