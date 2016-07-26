﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapKonamiVrc4 : CartridgeMap
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

            characterBank = new int[8];

            irqReloadLowAddress = 0xF000;

            switch (variant)
            {
                case Variant.Vrc4RevAorC:
                    mapperName = "Konami VRC4a/VRC4c";

                    programBank0RegisterAddresses = new ushort[] { 0x8000, 0x8002, 0x8004, 0x8006, 0x8040, 0x8080, 0x80C0 };
                    programBank1RegisterAddresses = new ushort[] { 0xA000, 0xA002, 0xA004, 0xA006, 0xA040, 0xA080, 0xA0C0 };
                    mirroringRegisterAddresses = new ushort[] { 0x9000, 0x9002, 0x9040 };
                    programModeRegisterAddresses = new ushort[] { 0x9004, 0x9006, 0x90C0 };

                    characterBankRegisterLowAddresses[0] = new ushort[] { 0xB000 };
                    characterBankRegisterHighAddresses[0] = new ushort[] { 0xB002, 0xB040 };
                    characterBankRegisterLowAddresses[1] = new ushort[] { 0xB004, 0x8080 };
                    characterBankRegisterHighAddresses[1] = new ushort[] { 0xB006, 0xB0C0 };

                    characterBankRegisterLowAddresses[2] = new ushort[] { 0xC000 };
                    characterBankRegisterHighAddresses[2] = new ushort[] { 0xC002, 0xC040 };
                    characterBankRegisterLowAddresses[3] = new ushort[] { 0xC004, 0xC080 };
                    characterBankRegisterHighAddresses[3] = new ushort[] { 0xC006, 0xC0C0 };

                    characterBankRegisterLowAddresses[4] = new ushort[] { 0xD000 };
                    characterBankRegisterHighAddresses[4] = new ushort[] { 0xD002, 0xD040 };
                    characterBankRegisterLowAddresses[5] = new ushort[] { 0xD004, 0xD080 };
                    characterBankRegisterHighAddresses[5] = new ushort[] { 0xD006, 0xD0C0 };

                    characterBankRegisterLowAddresses[6] = new ushort[] { 0xE000 };
                    characterBankRegisterHighAddresses[6] = new ushort[] { 0xE002, 0xE040 };
                    characterBankRegisterLowAddresses[7] = new ushort[] { 0xE004, 0xE080 };
                    characterBankRegisterHighAddresses[7] = new ushort[] { 0xE006, 0xE0C0 };

                    irqReloadHighAddresses = new ushort[] { 0xF002, 0xF040 };
                    irqControlAddresses = new ushort[] { 0xF004, 0xF080 };
                    irqAcknowledgeAddresses = new ushort[] { 0xF006, 0xF0C0 };

                    break;
                case Variant.Vrc4RevBorD:
                    mapperName = "Konami VRC4b/VRC4d";
                    break;
                case Variant.Vrc4RevEorF:
                    mapperName = "Konami VRC4e/VRC4f";
                    break;
            }

            programBankCount = cartridge.ProgramRom.Count / 0x2000;
            characterBankCount = cartridge.CharacterRom.Length / 0x400;

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
                    return Cartridge.CharacterRom[characterBank[bankIndex] + bankOffset];
                }
                else
                    return (byte)(address >> 8); // open buss
            }

            set
            {
                if (programModeRegisterAddresses.Contains(address))
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
                    for (int characterBankIndex = 0; characterBankIndex < 8; characterBankIndex++)
                    {
                        if (characterBankRegisterLowAddresses[characterBankIndex].Contains(address))
                        {
                            characterBank[characterBankIndex] &= 0x1F0;
                            characterBank[characterBankIndex] |= (value & 0x0F);
                            break;
                        }
                        else if (characterBankRegisterHighAddresses[characterBankIndex].Contains(address))
                        {
                            characterBank[characterBankIndex] &= 0x00F;
                            characterBank[characterBankIndex] |= ((value & 0x1F) << 4);
                            break;
                        }
                    }
                }
                else if (address == irqReloadLowAddress)
                {
                    irqReloadValue &= 0xF0;
                    irqReloadValue |= (byte)(value & 0x0F);
                }
                else if (irqReloadHighAddresses.Contains(address))
                {
                    irqReloadValue &= 0x0F;
                    irqReloadValue |= (byte)((value & 0x0F) << 4);
                }
                else if (irqControlAddresses.Contains(address))
                {

                }
                else if (irqAcknowledgeAddresses.Contains(address))
                {

                }
            }
        }

        private Variant variant;
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

        private int characterBankCount;
        private int[] characterBank;

        private byte irqReloadValue;

        private enum ProgramMode
        {
            Mode0,
            Mode1
        }

    }
}
