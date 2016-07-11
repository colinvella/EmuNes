using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapJalecoSs88006 : CartridgeMap
    {
        public CartridgeMapJalecoSs88006(Cartridge cartridge)
        {
            this.Cartridge = cartridge;

            programRomBank = new int[4];
            characterRomBank = new int[8];

            programRomBank[0] = programRomBank[1] = programRomBank[2] = 0x000;
            programRomBank[3] = (cartridge.ProgramRom.Count / 0x2000) - 1;
        }

        public Cartridge Cartridge { get; private set; }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bank = address / 0x400;
                    int offset = address % 0x400;
                    return Cartridge.CharacterRom[characterRomBank[bank] * 0x400 + offset];
                }
                else if (address > 0x8000)
                {
                    int bank = (address - 0x8000) / 0x2000;
                    int offset = address % 0x2000;
                    return Cartridge.ProgramRom[programRomBank[bank] * 0x2000 + offset];
                }
                else
                    return (byte)(address >> 8); // assuming open bus
            }

            set
            {
                if (address >= 0x8000 && address < 0xA000)
                {
                    int offset1000 = address % 0x1000;
                    if (offset1000 >= 4)
                        return;

                    int bankIndex1000 = (address - 0x8000) / 0x1000;

                    int programBankIndex = bankIndex1000 * 2 + offset1000 / 2;

                    // last bank must remain fixed
                    if (programBankIndex > 2)
                        return;

                    if (offset1000 % 2 == 0)
                        programRomBank[programBankIndex] = SetLowerNybble(programRomBank[programBankIndex], value);
                    else
                        programRomBank[programBankIndex] = SetHigherNybble(programRomBank[programBankIndex], value);
                }
                else if (address >= 0xA000 && address < 0xE000)
                {
                    int offset1000 = address % 0x1000;
                    if (offset1000 >= 4)
                        return;
                    int bankIndex1000 = (address - 0xA000) / 0x1000;

                    int characterBankIndex = bankIndex1000 * 2 + offset1000 / 2;

                    if (offset1000 % 2 == 0)
                        characterRomBank[characterBankIndex] = SetLowerNybble(characterRomBank[characterBankIndex], value);
                    else
                        characterRomBank[characterBankIndex] = SetHigherNybble(characterRomBank[characterBankIndex], value);
                }
                else if (address == 0xF000)
                {
                    throw new NotImplementedException("$F000");
                }
                else if (address == 0xF001)
                {
                    throw new NotImplementedException("$F001");
                }
            }
        }

        public override string Name { get { return "Jaleco SS88006"; } }

        private int SetLowerNybble(int currentValue, byte nybble)
        {
            currentValue &= 0xF0;
            currentValue |= (nybble & 0x0F);
            return currentValue;
        }

        private int SetHigherNybble(int currentValue, byte nybble)
        {
            currentValue &= 0x0F;
            currentValue |= (nybble & 0x0F) << 4;
            return currentValue;
        }

        private int[] programRomBank;

        private int[] characterRomBank;
    }
}
