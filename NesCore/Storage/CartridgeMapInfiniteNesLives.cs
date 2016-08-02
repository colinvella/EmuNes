using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapInfiniteNesLives : CartridgeMap
    {
        public CartridgeMapInfiniteNesLives(Cartridge cartridge) : base(cartridge)
        {
            programBankCount = Cartridge.ProgramRom.Count / 0x1000;
            programBank = new int[8];
            programBank[7] = 0xFF;
        }

        public override string Name { get { return "Infinite NES Lives"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    return Cartridge.CharacterRom[address];
                }
                else if (address >= 0x8000)
                {
                    int programBankIndex = (address - 0x8000) / 0x1000;
                    int bankOffset = address % 0x1000;
                    int selectedBank = programBank[programBankIndex] % programBankCount;
                    return Cartridge.ProgramRom[selectedBank * 0x1000 + bankOffset];
                }
                else
                    return (byte)(address >> 8); // open bus
            }

            set
            {
                if (address < 0x2000)
                {
                    Cartridge.CharacterRom[address] = value;
                }
                else if (address >= 0x5000 && address < 0x6000)
                {
                    programBank[address & 0x0007] = value;
                }
            }
        }

        private int programBankCount;
        private int[] programBank;

    }
}
