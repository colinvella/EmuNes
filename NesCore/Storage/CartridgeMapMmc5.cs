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

            programBankMode = 0;
        }

        public Cartridge Cartridge { get; private set; }

        public override string Name { get { return "MMC5"; } }

        public override byte this[ushort address]
        {
            get
            {
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
                                return Cartridge.ProgramRom[programBank0 * 0x8000 + offset];
                            }
                        case 1:
                            if (address < 0xC000)
                            {
                                // PRG mode 1 - first 16k switchable ROM/RAM bank
                                int offset = address % 0x4000;
                                return Cartridge.ProgramRom[programBank1 * 0x4000 + offset];
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
                                return Cartridge.ProgramRom[programBank1 * 0x4000 + offset];
                            }
                            else if (address < 0xE000)
                            {
                                // PRG mode 2 - first 8k switchable ROM/RAM bank
                                int offset = address % 0x2000;
                                return Cartridge.ProgramRom[programBank2 * 0x2000 + offset];
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
                                return Cartridge.ProgramRom[programBank0 * 0x2000 + offset];
                            }
                            else if (address < 0xC000)
                            {
                                // PRG mode 3 - second 8k switchable ROM/RAM bank
                                int offset = address % 0x2000;
                                return Cartridge.ProgramRom[programBank1 * 0x2000 + offset];
                            }
                            else if (address < 0xE000 )
                            {
                                // PRG mode 3 - third 8k switchable ROM/RAM bank
                                int offset = address % 0x2000;
                                return Cartridge.ProgramRom[programBank2 * 0x2000 + offset];
                            }
                            else // if (address >= 0xE000)
                            {
                                // PRG mode 3 - fourth 8k switchable ROM/RAM bank
                                int offset = address % 0x2000;
                                return Cartridge.ProgramRom[programRomBank * 0x2000 + offset];
                            }
                        default:
                            throw new Exception("MMC5 Invalid program bank mode");
                    }
                }

                // invalid / unhandled addresses
                throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));
            }

            set
            {
                // registers
                if (address == 0x5100)
                {
                    programBankMode = (byte)(value & 0x03);
                    return;
                }
                if (address == 0x5101)
                {
                    throw new NotImplementedException("CHR mode");
                }
                if (address == 0x5102)
                {
                    throw new NotImplementedException("PRG ram protect 1");
                }
                if (address == 0x5103)
                {
                    throw new NotImplementedException("PRG ram protect 2");
                }
                if (address == 0x5104)
                {
                    extendedRamMode = (byte)(value & 0x03);
                    return;
                }
                if (address == 0x5105)
                {
                    throw new NotImplementedException("name table mapping");
                }
                if (address == 0x5106)
                {
                    throw new NotImplementedException("fill mode tile");
                }
                if (address == 0x5107)
                {
                    throw new NotImplementedException("fill mode colour");
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
                    readOnly0 = (value & 0x80) != 0;
                    programBank0 = (byte)(value & 0x7F);
                    return;
                }
                if (address == 0x5115)
                {
                    //RBBB BBBB : R - ROM mode, BBBBBBB - bank number 
                    readOnly1 = (value & 0x80) != 0;
                    programBank1 = (byte)(value & 0x7F);
                    return;
                }
                if (address == 0x5116)
                {
                    //RBBB BBBB : R - ROM mode, BBBBBBB - bank number 
                    readOnly2 = (value & 0x80) != 0;
                    programBank2 = (byte)(value & 0x7F);
                    return;
                }
                if (address == 0x5117)
                {
                    //-BBB BBBB : BBBBBBB - bank number 
                    programRomBank = (byte)(value & 0x7F);
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
                throw new NotImplementedException();
            }
        }

        public override void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            ppuRendering = scanLine >= 0 && scanLine < 240;
        }

        private void EvaluateProduct()
        {
            int product = factor1 * factor2;
            productLow = (byte)product;
            productHigh = (byte)(product >> 8);
        }

        private byte[] programRam;
        private byte[] extendedRam;

        private byte programBankMode;
        private byte extendedRamMode;

        private byte programRamBank;

        private byte programBank0;
        private bool readOnly0;

        private byte programBank1;
        private bool readOnly1;

        private byte programBank2;
        private bool readOnly2;

        private byte programRomBank;

        private bool ppuRendering;

        private byte factor1;
        private byte factor2;
        private byte productLow;
        private byte productHigh;
    }
}
