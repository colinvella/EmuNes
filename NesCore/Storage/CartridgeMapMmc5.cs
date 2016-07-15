using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapMmc5 : CartridgeMap
    {
        public CartridgeMapMmc5(Cartridge cartridge)
            : base(cartridge)
        {
            // 64K ram total (switchable 8 banks of 8K)
            programRam = new byte[0x10000];

            // 1K expansion ram
            extendedRam = new byte[0x400];

            programBankMode = 3;
            SetCharacterBankMode(0);
            programRomBank = (byte)(Cartridge.ProgramRom.Count / 0x2000 - 1);

            characterBanks = new ushort[12];
        }

        public override string Name { get { return "MMC5"; } }

        public override byte this[ushort address]
        {
            get
            {
                int bankOffset = address % characterBankSize;

                // character ROM
                if (address < 0x2000)
                {
                    // handle extended mode - bank registers from extended ram page
                    if (extendedRamMode == 1 && !AccessingSpriteCharacters)
                    {
                        bankOffset = address % 0x1000;
                        
                        int extendedCharacterBank = extendedRam[lastTileIndex];
                        extendedCharacterBank &= 0x3F;
                        extendedCharacterBank |= (characterBankUpper >> 2);
                        return Cartridge.CharacterRom[extendedCharacterBank * 0x1000 + bankOffset];
                    }


                    // handles all CHR modes

                    // bank range CHR modes 8K: [0], 4K: [0, 1] 2K: [0..3], 1k: [0..7]
                    int bankRange = address / characterBankSize;

                    // determine stride between bank registers, given CHR mode: 8K: 8, 4K: 4, 2K: 2, 1K: 1
                    int indexStride = characterBankSize / 0x0400;

                    // determine bank index: 8K: [7], 4k: [3, 7], 2K: [1, 3, 5, 7], 8K: [0..7]
                    int bankIndex = (bankRange + 1) * indexStride - 1;

                    // if sprite mode is 8x16 and chr access if for background, use upper
                    // bank switching register indexes  8K: [11], 4k: [11], 2K: [9, 11], 8K: [8..11]
                    // or
                    // sprite mode is 8x8 and last register banks written are upper
                    if ((SpriteSize == Video.SpriteSize.Size8x16 && !AccessingSpriteCharacters)
                        || (SpriteSize == Video.SpriteSize.Size8x8 && characterBankLastWrittenUpper))
                    {
                        bankIndex %= 4;
                        bankIndex += 8;
                    }

                    int characterBank = characterBanks[bankIndex];
                    int addressBase = characterBank * characterBankSize;
                    return Cartridge.CharacterRom[addressBase + bankOffset];

                }

                if (address >= 0x5000 && address <= 0x5007)
                {
                    // TODO: pulse generators 1 and 2
                    return 0x00;
                }
                if (address == 0x5010 || address == 0x5011 || address == 0x5015)
                {
                    // TODO: irq, pcm, status
                    return 0x00;
                }

                // read-only IRQ counter - return open bus?
                if (address == 0x5203)
                    return 0x52;

                if (address == 0x5204)
                {
                    byte result = 0;
                    if (irqPending)
                        result |= 0x80;
                    if (inFrame)
                        result |= 0x40;
                    irqPending = false;
                    CancelInterruptRequest?.Invoke();
                    return result;
                }

                if (address == 0x5205)
                    return productLow;

                if (address == 0x5206)
                    return productHigh;

                if (address >= 0x5C00 && address < 0x6000)
                {
                    // expansion ram - all modes
                    switch (extendedRamMode)
                    {
                        case 0:
                        case 1:
                            // expansion ram mode 0/1 - returns open bus
                            return (byte)(address >> 8);
                        case 2:
                            // expansion ram mode 2 - 1K r/w memory
                        case 3:
                            // expansion ram mode 3 - 1K ROM
                            return extendedRam[address % 0x400];
                        default:
                            throw new Exception("MMC5 Invalid expansion ram mode");
                    }
                }

                if (address >= 0x6000 && address < 0x8000)
                {
                    // all bank modes - 8K switchable RAM bank
                    int offset = address % 0x2000;
                    return programRam[programRamBank * 0x2000 + offset];
                }

                if (address >= 0x8000)
                {
                    // program banks for all modes
                    switch (programBankMode)
                    {
                        case 0:
                            {
                                // PRG mode 0 - single 32k switchable ROM bank
                                int offset = address % 0x8000;
                                return Cartridge.ProgramRom[(programRomBank >> 2) * 0x8000 + offset];
                            }
                        case 1:
                            if (address < 0xC000)
                            {
                                // PRG mode 1 - first 16k switchable ROM/RAM bank
                                int offset = address % 0x4000;
                                int flatAddress = (programBank1 >> 1) * 0x4000 + offset;
                                if (romMode1)
                                    return Cartridge.ProgramRom[flatAddress];
                                else
                                    return programRam[flatAddress];
                            }
                            else // if (address >= 0xC000)
                            {
                                // PRG mode 1 - second 16k switchable ROM bank
                                int offset = address % 0x4000;
                                return Cartridge.ProgramRom[programRomBank * 0x4000 + offset];
                            }
                        case 2:
                            if (address < 0xC000)
                            {
                                // PRG mode 2 - 16k switchable ROM/RAM bank
                                int offset = address % 0x4000;
                                int flatAddress = (programBank1 >> 1) * 0x4000 + offset;
                                if (romMode1)
                                    return Cartridge.ProgramRom[flatAddress];
                                else
                                    return programRam[flatAddress];
                            }
                            else if (address < 0xE000)
                            {
                                // PRG mode 2 - first 8k switchable ROM/RAM bank
                                int offset = address % 0x2000;
                                int flatAddress = programBank2 * 0x2000 + offset;
                                if (romMode2)
                                    return Cartridge.ProgramRom[flatAddress];
                                else
                                    return programRam[flatAddress];
                            }
                            else // if (address >= 0xE000 )
                            {
                                // PRG mode 2 - second 8k switchable ROM bank
                                int offset = address % 0x2000;
                                return Cartridge.ProgramRom[programRomBank * 0x2000 + offset];
                            }
                        case 3:
                            if (address < 0xA000)
                            {
                                // PRG mode 3 - first 8k switchable ROM/RAM bank
                                int offset = address % 0x2000;
                                int flatAddress = programBank0 * 0x2000 + offset;
                                if (romMode0)
                                    return Cartridge.ProgramRom[flatAddress];
                                else
                                    return programRam[flatAddress];
                            }
                            else if (address < 0xC000)
                            {
                                // PRG mode 3 - second 8k switchable ROM/RAM bank
                                int offset = address % 0x2000;
                                int flatAddress = programBank1 * 0x2000 + offset;
                                if (romMode1)
                                    return Cartridge.ProgramRom[flatAddress];
                                else
                                    return programRam[flatAddress];
                            }
                            else if (address < 0xE000 )
                            {
                                // PRG mode 3 - third 8k switchable ROM/RAM bank
                                int offset = address % 0x2000;
                                int flatAddress = programBank2 * 0x2000 + offset;
                                if (romMode2)
                                    return Cartridge.ProgramRom[flatAddress];
                                else
                                    return programRam[flatAddress];
                            }
                            else // if (address >= 0xE000)
                            {
                                // PRG mode 3 - fourth 8k switchable ROM bank
                                int offset = address % 0x2000;
                                return Cartridge.ProgramRom[programRomBank * 0x2000 + offset];
                            }
                        default:
                            throw new Exception("MMC5 Invalid program bank mode");
                    }
                }

                // invalid / unhandled addresses
                //throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
                // return open bus for unhandled read
                return (byte)(address >> 8);
            }

            set
            {
                // character ROM
                if (address < 0x2000)
                {
                    // handles all CHR modes

                    // bank range CHR modes 8K: [0], 4K: [0, 1] 2K: [0..3], 1k: [0..7]
                    int bankRange = address / characterBankSize;

                    // determine stride between bank registers, given CHR mode: 8K: 8, 4K: 4, 2K: 2, 1K: 1
                    int indexStride = characterBankSize / 0x0400;

                    // determine bank index: 8K: [7], 4k: [3, 7], 2K: [1, 3, 5, 7], 8K: [0..7]
                    int bankIndex = (bankRange + 1) * indexStride - 1;

                    // if sprite mode is 8x16 and chr access if for background, use upper
                    // bank switching register indexes  8K: [11], 4k: [11], 2K: [9, 11], 8K: [8..11]
                    // or
                    // sprite mode is 8x8 and last register banks written are upper
                    if ((SpriteSize == Video.SpriteSize.Size8x16 && !AccessingSpriteCharacters)
                        || (SpriteSize == Video.SpriteSize.Size8x8 && characterBankLastWrittenUpper))
                    {
                        bankIndex %= 4;
                        bankIndex += 8;
                    }

                    int characterBank = characterBanks[bankIndex];
                    int addressBase = characterBank * characterBankSize;
                    int bankOffset = address % characterBankSize;
                    Cartridge.CharacterRom[addressBase + bankOffset] = value;
                    return;
                }

                // registers
                if (address >= 0x5000 && address <= 0x5007)
                {
                    // TODO: pulse generators 1 and 2
                    return;
                }
                if (address == 0x5010 || address == 0x5011 || address == 0x5015)
                {
                    // TODO: irq, pcm, status
                    return;
                }

                if (address == 0x5100)
                {
                    programBankMode = (byte)(value & 0x03);
                    Debug.WriteLine("MMC5 PRG Bank Mode ($5100) = " + Hex.Format(programBankMode));
                    return;
                }
                if (address == 0x5101)
                {
                    SetCharacterBankMode((byte)(value & 0x03));

                    Debug.WriteLine("MMC5 CHR Bank Mode ($5101) = " + Hex.Format(characterBankMode) + " (" + characterBankCount + " " + characterBankSize + "b Banks)");
                    return;
                }
                if (address == 0x5102)
                {
                    programRamProtect1 = value == 2;
                    programRamProtect = programRamProtect1 && programRamProtect2;
                    Debug.WriteLine("MMC5 Ram Protect 1 ($5103) = " + (programRamProtect1 ? "Enabled" : "Disabled") + " (P1 && P2 = " + programRamProtect + ")");
                    return;
                }
                if (address == 0x5103)
                {
                    programRamProtect2 = value == 1;
                    programRamProtect = programRamProtect1 && programRamProtect2;
                    Debug.WriteLine("MMC5 Ram Protect 2 ($5103) = " + (programRamProtect2 ? "Enabled" : "Disabled") + " (P1 && P2 = " + programRamProtect + ")");
                    return;
                }
                if (address == 0x5104)
                {
                    extendedRamMode = (byte)(value & 0x03);
                    Debug.WriteLine("MMC5 ExRamMode ($5104) = " + Hex.Format((byte)extendedRamMode));
                    return;
                }
                if (address == 0x5105)
                {
                    // Mirror mode - DD CC BB AA
                    MirrorMode = (MirrorMode)value;
                    Debug.WriteLine("MMC5 Mirror Mode ($5105) = " + MirrorMode + " (" + Hex.Format((byte)MirrorMode) + ") (" + Bin.Format((byte)MirrorMode) + ")");

                    return;
                }
                if (address == 0x5106)
                {
                    fillModeTile = value;
                    Debug.WriteLine("MMC5 Fill Mode Tile ($5106) = " + Hex.Format(fillModeTile));
                    return;
                }
                if (address == 0x5107)
                {
                    int attrbibutes = value & 0x03;
                    // replicate bits 0,1 to rest of byte ------AB -> ABABABAB
                    fillModeAttributes = (byte)(attrbibutes | (attrbibutes << 2) | (attrbibutes << 4) | (attrbibutes << 6));
                    Debug.WriteLine("MMC5 Fill Mode Attributes ($5107) = " + Hex.Format((byte)attrbibutes));
                    return;
                }
                if (address == 0x5113)
                {
                    //---- -CBB : C: chip, BB: bank within chip (CBB can be treated as 8 banks) 
                    programRamBank = (byte)(value & 0x07);
                    Debug.WriteLine("MMC5 PRG RAM Bank ($5113) = " + Hex.Format(programRamBank));
                    return;
                }
                if (address == 0x5114)
                {
                    //RBBB BBBB : R - ROM mode, BBBBBBB - bank number 
                    romMode0 = (value & 0x80) != 0;
                    programBank0 = (byte)(value & 0x7F);
                    Debug.WriteLine("MMC5 PRG ROM/RAM Bank 0 ($5114) = " + Hex.Format(programBank0) + " in " + (romMode0 ? "ROM" : "RAM") + " mode");
                    return;
                }
                if (address == 0x5115)
                {
                    //RBBB BBBB : R - ROM mode, BBBBBBB - bank number 
                    romMode1 = (value & 0x80) != 0;
                    programBank1 = (byte)(value & 0x7F);
                    Debug.WriteLine("MMC5 PRG ROM/RAM Bank 1 ($5115) = " + Hex.Format(programBank1) + " in " + (romMode1 ? "ROM" : "RAM") + " mode");
                    return;
                }
                if (address == 0x5116)
                {
                    //RBBB BBBB : R - ROM mode, BBBBBBB - bank number 
                    romMode2 = (value & 0x80) != 0;
                    programBank2 = (byte)(value & 0x7F);
                    Debug.WriteLine("MMC5 PRG ROM/RAM Bank 2 ($5116) = " + Hex.Format(programBank2) + " in " + (romMode2 ? "ROM" : "RAM") + " mode");
                    return;
                }
                if (address == 0x5117)
                {
                    //-BBB BBBB : BBBBBBB - bank number 
                    programRomBank = (byte)(value & 0x7F);
                    Debug.WriteLine("MMC5 PRG ROM Bank Register ($5117) = " + Hex.Format(programRomBank));
                    return;
                }

                if (address >= 0x5120 && address <= 0x512B)
                {
                    if (characterBankCount == 0)
                        return;

                    characterBankLastWrittenUpper = address > 0x5127;

                    ushort characterBank = 0;
                    // merge low bits
                    characterBank |= value;
                    // merge high bits
                    characterBank |= characterBankUpper;
                    // ensure within available banks
                    characterBank %= characterBankCount;
                    // assign to corresponding bank switch
                    characterBanks[address - 0x5120] = characterBank;

                    Debug.WriteLine("MMC5 CHR Bank Register (" + Hex.Format(address) + ") = " + Hex.Format(value) + " (" + Hex.Format(characterBank) + ")");

                    return;
                }

                if (address == 0x5130)
                {
                    // upper 2 bits (bit 8, 9) for character bank selection (all banks)
                    characterBankUpper = value;
                    characterBankUpper &= 0x03;
                    Debug.WriteLine("MMC5 Upper CHR Bits ($5130) = " + Bin.Format((byte)characterBankUpper));
                    characterBankUpper <<= 8;
                    return;
                }

                if (address == 0x5200)
                {
                    //ES-W WWWW
                    verticalSplitModeEnabled = (value & 0x80) != 0;
                    verticalSplitSide = (VerticalSplitSide)((value >> 6) & 0x01);
                    verticalSplitStartStopTile = (byte)(value & 0x1F);
                    return;
                }

                if (address == 0x5203)
                {
                    irqLatch = value;
                    Debug.WriteLine("MMC5 IRQ Counter($5203) = " + irqLatch);
                    return;
                }

                if (address == 0x5204)
                {
                    irqEnabled = (value & 0x80) != 0;
                    Debug.WriteLine("MMC5 IRQ ($5204) = " + (irqEnabled ? "Enabled" : "Disabled"));
                    return;
                }

                if (address == 0x5205)
                {
                    factor1 = value;
                    EvaluateProduct();
                    Debug.WriteLine("MMC5 Mul Factor 1 ($5205) = " + Hex.Format(factor1) + " (Result High: " + Hex.Format(productHigh) + ", Low: " + Hex.Format(productLow) + ")");
                    return;
                }
                if (address == 0x5206)
                {
                    factor2 = value;
                    EvaluateProduct();
                    Debug.WriteLine("MMC5 Mul Factor 2 ($5205) = " + Hex.Format(factor1) + " (Result High: " + Hex.Format(productHigh) + ", Low: " + Hex.Format(productLow) + ")");
                    return;
                }

                if (address >= 0x5C00 && address < 0x6000)
                {
                    // expansion ram - all modes
                    Debug.WriteLine("MMC5 Write to Extended RAM (" + Hex.Format(address) + ") = " + Hex.Format(value));

                    switch (extendedRamMode)
                    {
                        case 0:
                        case 1:
                            // expansion ram mode 0/1 - writes allowed when ppu rendering, otherwise zero written
                            extendedRam[address % 0x400] = ppuRendering ? value : (byte)0;
                            return;
                        case 2:
                            // expansion ram mode 2 - 1K r/w memory
                            extendedRam[address % 0x400] = value;
                            return;
                        case 3:
                            // expansion ram mode 3 - 1K ROM (read only - do nothing?)
                            return;
                        default:
                            throw new Exception("MMC5 Invalid expansion ram mode");
                    }
                }

                if (address >= 0x6000 && address < 0x8000)
                {
                    // all bank modes - 8K switchable RAM bank
                    int offset = address % 0x2000;
                    programRam[programRamBank * 0x2000 + offset] = value;
                    return;
                }

                if (address >= 0x8000)
                {
                    // program banks for all modes
                    switch (programBankMode)
                    {
                        case 0:
                            {
                                // PRG mode 0 - single 32k switchable ROM bank
                                return;
                            }
                        case 1:
                            if (address < 0xC000)
                            {
                                // PRG mode 1 - first 16k switchable ROM/RAM bank
                                if (!romMode1 && !programRamProtect)
                                {
                                    int offset = address % 0x4000;
                                    programRam[(programBank1 >> 1) * 0x4000 + offset] = value;
                                }
                                return;
                            }
                            else // if (address >= 0xC000)
                            {
                                // PRG mode 1 - second 16k switchable ROM bank
                                return;
                            }
                        case 2:
                            if (address < 0xC000)
                            {
                                // PRG mode 2 - 16k switchable ROM/RAM bank
                                if (!romMode1 && !programRamProtect)
                                {
                                    int offset = address % 0x4000;
                                    programRam[(programBank1 >> 1) * 0x4000 + offset] = value;
                                }
                                return;
                            }
                            else if (address < 0xE000)
                            {
                                // PRG mode 2 - first 8k switchable ROM/RAM bank
                                if (!romMode2 && !programRamProtect)
                                {
                                    int offset = address % 0x2000;
                                    programRam[programBank2 * 0x2000 + offset] = value;
                                }
                                return;
                            }
                            else // if (address >= 0xE000 )
                            {
                                // PRG mode 2 - second 8k switchable ROM bank
                                return;
                            }
                        case 3:
                            if (address < 0xA000)
                            {
                                // PRG mode 3 - first 8k switchable ROM/RAM bank
                                if (!romMode0 && !programRamProtect)
                                {
                                    int offset = address % 0x2000;
                                    programRam[programBank0 * 0x2000 + offset] = value;
                                }
                                return;
                            }
                            else if (address < 0xC000)
                            {
                                // PRG mode 3 - second 8k switchable ROM/RAM bank
                                if (!romMode1 && !programRamProtect)
                                {
                                    int offset = address % 0x2000;
                                    programRam[programBank1 * 0x2000 + offset] = value;
                                }
                                return;
                            }
                            else if (address < 0xE000)
                            {
                                // PRG mode 3 - third 8k switchable ROM/RAM bank
                                if (!romMode2 && !programRamProtect)
                                {
                                    int offset = address % 0x2000;
                                    programRam[programBank2 * 0x2000 + offset] = value;
                                }
                                return;
                            }
                            else // if (address >= 0xE000)
                            {
                                // PRG mode 3 - fourth 8k switchable ROM bank
                                return;
                            }
                        default:
                            throw new Exception("MMC5 Invalid program bank mode");
                    }
                }

                // invalid / unhandled addresses
                throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));

            }
        }

        public override void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            ppuRendering = scanLine >= 0 && scanLine < 240 && (showBackground || showSprites);

            if (cycle != 0)
                return;

            if (ppuRendering)
            {
                if (!inFrame)
                {
                    inFrame = true;
                    irqCounter = 0;
                    CancelInterruptRequest?.Invoke();
                }
                else
                {
                    ++irqCounter;
                    if (irqCounter == irqLatch)
                    {
                        irqPending = true;
                        if (irqEnabled)
                            TriggerInterruptRequest?.Invoke();
                    }
                }
            }
            else
            {
                inFrame = false;
            }
        }

        public override byte ReadNameTableByte(ushort address)
        {
            ushort mirroredAddress = MirrorAddress(MirrorMode, address);

            int nameTableOffset = address % 0x400;
            if (nameTableOffset < 0x3C0)
                lastTileIndex = nameTableOffset;

            //mirroredAddress %= 0x800;
            if (mirroredAddress < 0x800) // normal name tables A and B
                return nameTableRam[mirroredAddress];
            else if (mirroredAddress < 0xC00) // name table C - depends on ex mode
            {
                switch (extendedRamMode)
                {
                    case 0: return extendedRam[nameTableOffset];
                    case 1:
                        // AACC - CCCC - exram byte
                        // ---- - --UU - $5130 upper 
                        if (nameTableOffset < 0x3C0) // nametable tile
                        {
                            // tile = UUCC CCCC
                            byte value = extendedRam[nameTableOffset];
                            value &= 0x3F;
                            value |= (byte)(characterBankUpper >> 6);
                            return value;
                        }
                        else // tile attribute
                        {
                            // attribute = AA
                            byte value = extendedRam[nameTableOffset];
                            value >>= 6;
                            return value;
                        }
                    default: // 2, 3
                        return 0x00;
                }
            }
            else // name table D - fill mode
            {
                return nameTableOffset < 0x3C0 ? fillModeTile : fillModeAttributes;
            }
        }

        public override void WriteNameTableByte(ushort address, byte value)
        {
            ushort mirroredAddress = MirrorAddress(MirrorMode, address);

            if (mirroredAddress < 0x800) // normal name tables A and B
                nameTableRam[mirroredAddress] = value;
            else if (mirroredAddress < 0xC00) // name table C
            {
                // NOTE: not sure if need to implement ex ram mode logic here as well
                extendedRam[address % 0x400] = value;
            }
            else // name table D
            {
                // do nothing - fill mode is read only and set via registers
            }
        }

        public override byte EnhanceTileAttributes(ushort nameTableAddress, byte defaultTileAttributes)
        {
            //return defaultTileAttributes;

            if (extendedRamMode != 1)
                return defaultTileAttributes;

            byte attributes = extendedRam[nameTableAddress % 0x400];
            attributes >>= 6;
            attributes <<= 2;
            return attributes;
        }

        private void SetCharacterBankMode(byte newCharacterBankMode)
        {
            characterBankMode = (byte)(newCharacterBankMode & 0x03);

            // compute character bank count and size depending on mode
            characterBankSize = (ushort)(0x0400 * Math.Pow(2,(3 - characterBankMode)));
            characterBankCount = (ushort)(Cartridge.CharacterRom.Length / characterBankSize);
        }

        private void EvaluateProduct()
        {
            int product = factor1 * factor2;
            productLow = (byte)product;
            productHigh = (byte)(product >> 8);
        }

        // program ram
        private byte[] programRam;

        // program bank mode and switching
        private byte programBankMode;

        private byte programRamBank;
        private bool programRamProtect1;
        private bool programRamProtect2;
        private bool programRamProtect;

        private byte programBank0;
        private bool romMode0;

        private byte programBank1;
        private bool romMode1;

        private byte programBank2;
        private bool romMode2;

        private byte programRomBank;

        // extended ram
        private byte[] extendedRam;
        private byte extendedRamMode;
        private bool ppuRendering;
        private int lastTileIndex;

        // character bank mode and switching
        private byte characterBankMode;
        private ushort characterBankSize;
        private ushort characterBankCount;
        private ushort[] characterBanks;
        private ushort characterBankUpper;
        private bool characterBankLastWrittenUpper;

        // fill mode
        byte fillModeTile;
        byte fillModeAttributes;

        // IRQ
        bool irqEnabled;
        byte irqCounter;
        byte irqLatch;
        bool irqPending;
        bool inFrame;

        // vertical split mode
        bool verticalSplitModeEnabled;
        VerticalSplitSide verticalSplitSide;
        byte verticalSplitStartStopTile;

        // multiplication
        private byte factor1;
        private byte factor2;
        private byte productLow;
        private byte productHigh;

        private enum VerticalSplitSide
        {
            Left,
            Right
        }
    }
}
