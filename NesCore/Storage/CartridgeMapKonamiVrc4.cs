using System;
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
            registerOffsets = new byte[8];
            programModeRegisterAddresses = new ushort[4];
            programBank0RegisterAddresses = new ushort[8];
            mirroringRegisterAddresses = new ushort[3];

            registerOffsets[0] = 0x00;
            registerOffsets[4] = 0x00;
            switch (variant)
            {
                case Variant.Vrc4RevAorC:
                    mapperName = "Konami VRC4a/VRC4c";
                    // program regeister 0 addresses for VRC4a and VRC4c
                    registerOffsets[1] = 0x02;
                    registerOffsets[2] = 0x04;
                    registerOffsets[3] = 0x06;

                    registerOffsets[5] = 0x40;
                    registerOffsets[6] = 0x80;
                    registerOffsets[7] = 0xC0;
                    break;
                case Variant.Vrc4RevBorD:
                    mapperName = "Konami VRC4b/VRC4d";
                    // program regeister 0 addresses for VRC4b and VRC4d
                    registerOffsets[1] = 0x02;
                    registerOffsets[2] = 0x01;
                    registerOffsets[3] = 0x03;

                    registerOffsets[5] = 0x08;
                    registerOffsets[6] = 0x04;
                    registerOffsets[7] = 0x0C;
                    break;
                case Variant.Vrc4RevEorF:
                    mapperName = "Konami VRC4e/VRC4f";
                    // program regeister 0 addresses for VRC4b and VRC4d
                    registerOffsets[1] = 0x04;
                    registerOffsets[2] = 0x08;
                    registerOffsets[3] = 0x0C;

                    registerOffsets[5] = 0x01;
                    registerOffsets[6] = 0x02;
                    registerOffsets[7] = 0x03;
                    break;
            }

            for (int index = 0; index < 8; index++)
            {
                programBank0RegisterAddresses[index] = (ushort)(0x8000 + registerOffsets[index]);
            }
            mirroringRegisterAddresses[0] = 0x9000;
            mirroringRegisterAddresses[1] = (ushort)(0x9000 + registerOffsets[1]);
            mirroringRegisterAddresses[1] = (ushort)(0x9000 + registerOffsets[5]);

            programModeRegisterAddresses[0] = (ushort)(0x9000 + registerOffsets[2]);
            programModeRegisterAddresses[1] = (ushort)(0x9000 + registerOffsets[3]);
            programModeRegisterAddresses[2] = (ushort)(0x9000 + registerOffsets[6]);
            programModeRegisterAddresses[3] = (ushort)(0x9000 + registerOffsets[7]);

            programBankCount = cartridge.ProgramRom.Count / 0x2000;
        }

        public override string Name { get { return mapperName; } }

        public override byte this[ushort address]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                if (programModeRegisterAddresses.Contains(address))
                {
                    programMode = (ProgramMode)((value >> 1) & 0x01);
                }
                else if (programBank0RegisterAddresses.Contains(address))
                {
                    programBankIndex0 = value % programBankCount;
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
            }
        }

        private Variant variant;
        private string mapperName;

        private byte[] registerOffsets;
        private ushort[] programModeRegisterAddresses;
        private ushort[] programBank0RegisterAddresses;
        private ushort[] mirroringRegisterAddresses;

        private ProgramMode programMode;
        private int programBankCount;
        private int programBankIndex0;

        private enum ProgramMode
        {
            Mode0,
            Mode1
        }

    }
}
