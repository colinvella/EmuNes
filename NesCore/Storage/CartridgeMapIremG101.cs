using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapIremG101 : CartridgeMap
    {
        public CartridgeMapIremG101(Cartridge cartridge) : base(cartridge)
        {
            programBankCount = cartridge.ProgramRom.Count / 0x2000;
            characterBankCount = cartridge.CharacterRom.Length / 0x400;
            characterBank = new int[8];
        }

        public override string Name { get { return "Irem G101"; } }

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
                else if (address >= 0x8000 && address < 0xA000)
                {
                    int selectedBank = programBankMode == 0 ? programBank0 : programBankCount - 2;
                    return Cartridge.ProgramRom[selectedBank * 0x2000 + address % 0x2000];
                }
                else if (address >= 0xA000 && address < 0xC000)
                {
                    return Cartridge.ProgramRom[programBank1 * 0x2000 + address % 0x2000];
                }
                else if (address >= 0xC000 && address < 0xE000)
                {
                    int selectedBank = programBankMode == 0 ? programBankCount - 2 : programBank0;
                    return Cartridge.ProgramRom[selectedBank * 0x2000 + address % 0x2000];
                }
                else if (address >= 0xE000)
                {
                    int selectedBank = programBankCount - 1;
                    return Cartridge.ProgramRom[selectedBank * 0x2000 + address % 0x2000];
                }
                return (byte)(address >> 8); // open bus
            }

            set
            {
                if (address >= 0x8000 && address < 0x8008)
                {
                    programBank0 = value & 0x1F;
                    programBank0 %= programBankCount;
                }
                else if (address >= 0x9000 && address < 0x9008)
                {
                    // PRG mode mapper controlled mirroring for all games except Major League
                    if (Cartridge.Crc != 0x243A8735)
                    {
                        programBankMode = (value >> 1) & 0x01;
                        MirrorMode = (value & 0x01) == 1 ? MirrorMode.Horizontal : MirrorMode.Vertical;
                    }
                }
                else if (address >= 0xA000 && address < 0xA008)
                {
                    programBank1 = value & 0x1F;
                    programBank1 %= programBankCount;
                }
                else if (address >= 0xB000 && address < 0xB008)
                {
                    characterBank[address - 0xB000] = value % characterBankCount;
                }

            }
        }

        int programBankCount;
        int programBankMode;
        int programBank0;
        int programBank1;

        int characterBankCount;
        int[] characterBank;
    }
}
