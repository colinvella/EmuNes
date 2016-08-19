using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapMultiMmc3 : CartridgeMap
    {
        public CartridgeMapMultiMmc3(Cartridge cartridge) : base(cartridge)
        {
            outerProgramBankOffset = new int[] { 0x00000, 0x00000, 0x00000, 0x10000, 0x20000, 0x20000, 0x20000, 0x30000 };
            outerProgramBankLength = new int[] { 0x10000, 0x10000, 0x10000, 0x10000, 0x20000, 0x20000, 0x20000, 0x10000 };

            outerCharacterBankOffset = new int[] { 0x00000, 0x00000, 0x00000, 0x00000, 0x20000, 0x20000, 0x20000, 0x20000 };

            innerCartridge = new Cartridge[8];
            for (int index = 0; index < 8; index++)
            {
                byte[] programRom = Cartridge.ProgramRom.Skip(outerProgramBankOffset[index]).Take(outerProgramBankLength[index]).ToArray();
                byte[] characterRom = Cartridge.CharacterRom.Skip(outerCharacterBankOffset[index]).Take(0x20000).ToArray();
                innerCartridge[index] = new Cartridge(programRom, characterRom, 4, MirrorMode);
            }

            outerBank = 0;
        }

        public override string Name { get { return "MMC3 Multicart"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address >= 0x6000 && address < 0x8000)
                {
                    return (byte)(address >> 8); // open bus
                }
                else
                {
                    return innerCartridge[outerBank].Map[address];
                }
            }

            set
            {
                if (address >= 0x6000 && address < 0x8000)
                {
                    outerBank = (byte)(value % 8);
                }
                else
                    innerCartridge[outerBank].Map[address] = value;
            }
        }

        private byte outerBank;
        private Cartridge[] innerCartridge;
        private int[] outerProgramBankOffset;
        private int[] outerProgramBankLength;
        private int[] outerCharacterBankOffset;
    }
}
