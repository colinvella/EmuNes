using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapKonamiVrc4 : CartridgeMapKonamiVrc
    {
        public enum Variant
        {
            Vrc4RevAorC,
            Vrc4RevBorD,
            Vrc4RevEorF
        }

        public CartridgeMapKonamiVrc4(Cartridge cartridge, Variant variant)
            : base(cartridge)
        {
            characterBankRegisterLowAddresses = new ushort[8][];
            characterBankRegisterHighAddresses = new ushort[8][];

            characterBank = new int[8];

            // IRQ reload address is common to all variants
            irqReloadLowAddress = 0xF000;

            switch (variant)
            {
                case Variant.Vrc4RevAorC:
                    mapperName = "Konami VRC4 rev A/C";

                    // define PRG reg 0 (reg 1 addresses can be computed)
                    programBank0RegisterAddresses = new ushort[] { 0x8000, 0x8002, 0x8004, 0x8006, 0x8040, 0x8080, 0x80C0 };

                    mirroringRegisterAddresses = new ushort[] { 0x9000, 0x9002, 0x9040 };
                    programModeRegisterAddresses = new ushort[] { 0x9004, 0x9006, 0x90C0 };

                    // define CHR regs 0 and 1 - the rest can be computed
                    characterBankRegisterLowAddresses[0] = new ushort[] { 0xB000 };
                    characterBankRegisterHighAddresses[0] = new ushort[] { 0xB002, 0xB040 };
                    characterBankRegisterLowAddresses[1] = new ushort[] { 0xB004, 0x8080 };
                    characterBankRegisterHighAddresses[1] = new ushort[] { 0xB006, 0xB0C0 };

                    // IRQ registers
                    irqReloadHighAddresses = new ushort[] { 0xF002, 0xF040 };
                    irqControlAddresses = new ushort[] { 0xF004, 0xF080 };
                    irqAcknowledgeAddresses = new ushort[] { 0xF006, 0xF0C0 };

                    break;
                case Variant.Vrc4RevBorD:
                    mapperName = "Konami VRC4 rev B / D";
                    
                    // define PRG reg 0 (reg 1 addresses can be computed)
                    programBank0RegisterAddresses = new ushort[] { 0x8000, 0x8002, 0x8001, 0x8003, 0x8008, 0x8004, 0x800C };

                    mirroringRegisterAddresses = new ushort[] { 0x9000, 0x9002, 0x9008 };
                    programModeRegisterAddresses = new ushort[] { 0x9001, 0x9003, 0x900C };

                    // define CHR regs 0 and 1 - the rest can be computed
                    characterBankRegisterLowAddresses[0] = new ushort[] { 0xB000 };
                    characterBankRegisterHighAddresses[0] = new ushort[] { 0xB002, 0xB008 };
                    characterBankRegisterLowAddresses[1] = new ushort[] { 0xB001, 0x8004 };
                    characterBankRegisterHighAddresses[1] = new ushort[] { 0xB003, 0xB00C };

                    // IRQ registers
                    irqReloadHighAddresses = new ushort[] { 0xF002, 0xF008 };
                    irqControlAddresses = new ushort[] { 0xF001, 0xF004 };
                    irqAcknowledgeAddresses = new ushort[] { 0xF003, 0xF00C };

                    break;
                case Variant.Vrc4RevEorF:
                    mapperName = "Konami VRC4 rev E / F";

                    // define PRG reg 0 (reg 1 addresses can be computed)
                    programBank0RegisterAddresses = new ushort[] { 0x8000, 0x8004, 0x8008, 0x800C, 0x8001, 0x8002, 0x8003 };

                    mirroringRegisterAddresses = new ushort[] { 0x9000, 0x9004, 0x9001 };
                    programModeRegisterAddresses = new ushort[] { 0x9008, 0x900C, 0x9003 };

                    // define CHR regs 0 and 1 - the rest can be computed
                    characterBankRegisterLowAddresses[0] = new ushort[] { 0xB000 };
                    characterBankRegisterHighAddresses[0] = new ushort[] { 0xB004, 0xB001 };
                    characterBankRegisterLowAddresses[1] = new ushort[] { 0xB008, 0x8002 };
                    characterBankRegisterHighAddresses[1] = new ushort[] { 0xB00C, 0xB003 };

                    // IRQ registers
                    irqReloadHighAddresses = new ushort[] { 0xF004, 0xF001 };
                    irqControlAddresses = new ushort[] { 0xF008, 0xF002 };
                    irqAcknowledgeAddresses = new ushort[] { 0xF00C, 0xF003 };

                    break;
            }

            // PRG bank 1 registers can be computed from bank 0 registers
            programBank1RegisterAddresses = programBank0RegisterAddresses.Select((a) => (ushort)(a + 0x2000)).ToArray();

            // compute CHR low and high register addresses 2 - 7 from 0 or 1 depending on evenness
            for (int index = 2; index < 8; index++)
            {
                characterBankRegisterLowAddresses[index]
                    = characterBankRegisterLowAddresses[index % 2].Select((a) => (ushort)(a + 0x1000 * (index / 2))).ToArray();

                characterBankRegisterHighAddresses[index]
                    = characterBankRegisterHighAddresses[index % 2].Select((a) => (ushort)(a + 0x1000 * (index / 2))).ToArray();
            }

            // program bank count and flat addresses to last and penultimate banks
            programBankCount = cartridge.ProgramRom.Count / 0x2000;
            programBankLastAddress = (programBankCount - 1) * 0x2000;
            programBankNextToLastAddress = programBankLastAddress - 0x2000;

            // CHR bank count for bank index modulation
            characterBankCount = cartridge.CharacterRom.Length / 0x400;

            programRam = new byte[0x2000];
        }

        public override string Name { get { return mapperName; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bankIndex = address / 0x400;
                    int bankOffset = address % 0x400;
                    return Cartridge.CharacterRom[characterBank[bankIndex] * 0x400 + bankOffset];
                }
                else if (address >= 0x6000 && address < 0x8000)
                {
                    // note: PRG RAM needed due to VRC4 rev B/D and VRC2 mapper mixup - makes Ganbare Goemon Gaiden 2 work
                    return programRam[address % 0x2000];
                }
                else if (address >= 0x8000 && address < 0xA000)
                {
                    int bankOffset = address % 0x2000;
                    if (programMode == ProgramMode.Mode0)
                        return Cartridge.ProgramRom[programBank0 * 0x2000 + bankOffset];
                    else
                        return Cartridge.ProgramRom[programBankNextToLastAddress + bankOffset];
                }
                else if (address >= 0xA000 && address < 0xC000)
                {
                    int bankOffset = address % 0x2000;
                    return Cartridge.ProgramRom[programBank1 * 0x2000 + bankOffset];
                }
                else if (address >= 0xC000 && address < 0xE000)
                {
                    int bankOffset = address % 0x2000;
                    if (programMode == ProgramMode.Mode0)
                        return Cartridge.ProgramRom[programBankNextToLastAddress + bankOffset];
                    else
                        return Cartridge.ProgramRom[programBank0 * 0x2000 + bankOffset];
                }
                else if (address >= 0xE000)
                {
                    int bankOffset = address % 0x2000;
                    return Cartridge.ProgramRom[programBankLastAddress + bankOffset];
                }
                else
                    return (byte)(address >> 8); // open buss
            }

            set
            {
                if (address >= 0x6000 && address < 0x8000)
                {
                    programRam[address % 0x2000] = value;
                }
                else if (programModeRegisterAddresses.Contains(address))
                {
                    programMode = (ProgramMode)((value >> 1) & 0x01);
                }
                else if (programBank0RegisterAddresses.Contains(address))
                {
                    programBank0 = value % programBankCount;
                }
                else if (programBank1RegisterAddresses.Contains(address))
                {
                    programBank1 = value % programBankCount;
                }
                else if (mirroringRegisterAddresses.Contains(address))
                {
                    switch (value % 0x03)
                    {
                        case 0: MirrorMode = MirrorMode.Vertical; break;
                        case 1: MirrorMode = MirrorMode.Horizontal; break;
                        case 2: MirrorMode = MirrorMode.Single0; break;
                        case 3: MirrorMode = MirrorMode.Single1; break;
                    }
                }
                else if (address >= 0xB000 && address < 0xF000)
                {
                    //Debug.WriteLine("CHR Register " + Hex.Format(address) + " = " + Hex.Format(value));
                    for (int characterBankIndex = 0; characterBankIndex < 8; characterBankIndex++)
                    {
                        if (characterBankRegisterLowAddresses[characterBankIndex].Contains(address))
                        {
                            characterBank[characterBankIndex] &= 0x1F0;
                            characterBank[characterBankIndex] |= (value & 0x0F);
                            characterBank[characterBankIndex] %= characterBankCount;
                            //Debug.WriteLine("CHR bank [" + characterBankIndex + "] (" + Hex.Format((ushort)(characterBankIndex * 0x400)) + ") = " + Hex.Format((ushort)characterBank[characterBankIndex]));
                            break;
                        }
                        else if (characterBankRegisterHighAddresses[characterBankIndex].Contains(address))
                        {
                            characterBank[characterBankIndex] &= 0x00F;
                            characterBank[characterBankIndex] |= ((value & 0x1F) << 4);
                            characterBank[characterBankIndex] %= characterBankCount;
                            //Debug.WriteLine("CHR bank [" + characterBankIndex + "] (" + Hex.Format((ushort)(characterBankIndex * 0x400)) + ") = " + Hex.Format((ushort)characterBank[characterBankIndex]));
                            break;
                        }
                    }
                }
                else if (address == irqReloadLowAddress)
                {
                    WriteIrqReloadValueLowNybble(value);
                }
                else if (irqReloadHighAddresses.Contains(address))
                {
                    WriteIrqReloadValueHighNybble(value);
                }
                else if (irqControlAddresses.Contains(address))
                {
                    WriteIrqControl(value);
                }
                else if (irqAcknowledgeAddresses.Contains(address))
                {
                    WriteIrqAcknowledge();
                }
                else
                {
                    Debug.WriteLine("VRC4 unknown write at " + Hex.Format(address) + " with value " + Hex.Format(value));
                }
            }
        }

        private string mapperName;

        private ushort[] programModeRegisterAddresses;
        private ushort[] programBank0RegisterAddresses;
        private ushort[] programBank1RegisterAddresses;
        private ushort[] mirroringRegisterAddresses;

        private ushort[][] characterBankRegisterLowAddresses;
        private ushort[][] characterBankRegisterHighAddresses;

        private ushort irqReloadLowAddress;
        private ushort[] irqReloadHighAddresses;
        private ushort[] irqControlAddresses;
        private ushort[] irqAcknowledgeAddresses;

        private ProgramMode programMode;
        private int programBankCount;
        private int programBank0;
        private int programBank1;
        private int programBankLastAddress;
        private int programBankNextToLastAddress;

        private int characterBankCount;
        private int[] characterBank;

        private byte[] programRam;

        private enum ProgramMode
        {
            Mode0,
            Mode1
        }
    }
}
