using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapMmc5 : CartridgeMap
    {
        public CartridgeMapMmc5(Cartridge cartridge)
        {
            Cartridge = cartridge;

            // 64K ram total (switchable 8 banks of 8K)
            programRam = new byte[0x10000];

            // 1K expansion ram
            extendedRam = new byte[0x400];

            programBankMode = 3;
            SetCharacterBankMode(0);
            programRomBank = (byte)(Cartridge.ProgramRom.Count / 0x2000 - 1);

            characterBanks = new ushort[12];
        }

        public Cartridge Cartridge { get; private set; }

        public override string Name { get { return "MMC5"; } }

        public override byte this[ushort address]
        {
            get
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
                    byte value = 0x00;
                    if (irqPending)
                        value |= 0x80;
                    if (ppuRendering)
                        value |= 0x40;
                    irqPending = false;
                    return value;
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
                throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
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
                    return;
                }
                if (address == 0x5101)
                {
                    SetCharacterBankMode((byte)(value & 0x03));
                    return;
                }
                if (address == 0x5102)
                {
                    programRamProtect1 = value == 2;
                    programRamProtect = programRamProtect1 && programRamProtect2;
                    return;
                }
                if (address == 0x5103)
                {
                    programRamProtect2 = value == 1;
                    programRamProtect = programRamProtect1 && programRamProtect2;
                    return;
                }
                if (address == 0x5104)
                {
                    extendedRamMode = (byte)(value & 0x03);
                    return;
                }
                if (address == 0x5105)
                {
                    // DD CC BB AA

                    MirrorMode mirrorMode = MirrorMode.Single0;

                    switch (value)
                    {
                        case 0x50: mirrorMode = MirrorMode.Horizontal;        break; // 01 01 00 00
                        case 0x44: mirrorMode = MirrorMode.Vertical;          break; // 01 00 01 00
                        case 0x00: mirrorMode = MirrorMode.Single0;           break; // 00 00 00 00
                        case 0x55: mirrorMode = MirrorMode.Single1;           break; // 01 01 01 01
                        case 0xAA: mirrorMode = MirrorMode.ExRam;             break; // 10 10 10 10
                        case 0xFF: mirrorMode = MirrorMode.FillMode;          break; // 11 11 11 11
                        case 0x14: mirrorMode = MirrorMode.Diagonal;          break; // 00 01 01 00
                        case 0x54: mirrorMode = MirrorMode.LShaped;           break; // 01 01 01 00
                        case 0xA4: mirrorMode = MirrorMode.Horizontal3Screen; break; // 10 10 01 00
                        case 0x98: mirrorMode = MirrorMode.Vertical3Screen;   break; // 10 01 10 00
                        case 0x94: mirrorMode = MirrorMode.Diagonal3Screen;   break; // 10 01 01 00
                        case 0xE4: mirrorMode = MirrorMode.PseudoFour;        break; // 11 10 01 00
                        default: // unimplemented
                            Cartridge.MirrorMode = MirrorMode.Single0;
                            break;
                    }

                    if (Cartridge.MirrorMode != mirrorMode)
                    {
                        Cartridge.MirrorMode = mirrorMode;
                        Cartridge.MirrorModeChanged?.Invoke();
                    }

                    return;
                }
                if (address == 0x5106)
                {
                    fillModeTile = value;
                    return;
                }
                if (address == 0x5107)
                {
                    fillModeAttributes = (byte)(value & 0x03);
                    return;
                }
                if (address == 0x5113)
                {
                    //---- -CBB : C: chip, BB: bank within chip (CBB can be treated as 8 banks) 
                    programRamBank = (byte)(value & 0x07);
                    return;
                }
                if (address == 0x5114)
                {
                    //RBBB BBBB : R - ROM mode, BBBBBBB - bank number 
                    romMode0 = (value & 0x80) != 0;
                    programBank0 = (byte)(value & 0x7F);
                    return;
                }
                if (address == 0x5115)
                {
                    //RBBB BBBB : R - ROM mode, BBBBBBB - bank number 
                    romMode1 = (value & 0x80) != 0;
                    programBank1 = (byte)(value & 0x7F);
                    return;
                }
                if (address == 0x5116)
                {
                    //RBBB BBBB : R - ROM mode, BBBBBBB - bank number 
                    romMode2 = (value & 0x80) != 0;
                    programBank2 = (byte)(value & 0x7F);
                    return;
                }
                if (address == 0x5117)
                {
                    //-BBB BBBB : BBBBBBB - bank number 
                    programRomBank = (byte)(value & 0x7F);
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
                    return;
                }

                if (address == 0x5130)
                {
                    // upper 2 bits (bit 8, 9) for character bank selection (all banks)
                    characterBankUpper = value;
                    characterBankUpper &= 0x03;
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
                    irqScanline = value;
                    return;
                }

                if (address == 0x5204)
                {
                    irqEnabled = (value & 0x80) != 0;
                    return;
                }

                if (address == 0x5205)
                {
                    factor1 = value;
                    EvaluateProduct();
                    return;
                }
                if (address == 0x5206)
                {
                    factor2 = value;
                    EvaluateProduct();
                    return;
                }

                if (address >= 0x5C00 && address < 0x6000)
                {
                    // expansion ram - all modes
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

            if (!ppuRendering)
                irqCounter = 0;

            if (cycle != 0)
                return;

            if (scanLine == 0)
            {
                irqPending = false;
                irqCounter = 0;
            }
            else if (scanLine > 0)
                ++irqCounter;

            if (irqCounter == irqScanline)
            {
                irqPending = true;
                if (irqEnabled)
                    TriggerInterruptRequest?.Invoke();
            }

            if (scanLine > 239)
            {
                irqPending = false;
            }
        }

        public override byte ReadNameTableC(ushort address)
        {
            return extendedRam[address];
        }

        public override void WriteNameTableC(ushort address, byte value)
        {
            // NOTE: not sure if need to implement ex ram mode logic here as well
            extendedRam[address] = value;
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
        bool irqPending;
        byte irqCounter;
        byte irqScanline;

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
