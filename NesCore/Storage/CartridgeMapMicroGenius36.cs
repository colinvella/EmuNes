using NesCore.Storage.Hacks;
using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapMicroGenius36 : CartridgeMap
    {
        public CartridgeMapMicroGenius36(Cartridge cartridge) : base(cartridge)
        {
            programBankCount = Cartridge.ProgramRom.Count / 0x8000;
            characterBankCount = Cartridge.CharacterRom.Length / 0x2000;

            programBank = programBankCount - 1;

            if (Cartridge.Crc == 0x143DF524)
                strikeWolfHack = new PpuStatusSpinHack();
        }

        public override string Name { get { return "Micro Genius TXC"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    return Cartridge.CharacterRom[characterBank * 0x2000 + address];
                }
                else if (address >= 0x4100 && address < 0x4104)
                {
                    byte value = (byte)(programBankRR << 4);
                    value |= 0x41;
                    return value;
                }
                else if (address >= 0x8000)
                {
                    byte value = Cartridge.ProgramRom[programBank * 0x8000 + address % 0x8000];

                    if (Cartridge.Crc == 0x143DF524)
                        value = strikeWolfHack.Read(address, value);

                    return value;
                }
                else
                    return (byte)(address >> 8);
            }

            set
            {
                /*
                     Mask: $E103
                     read $4100-4103: [xxRR xxxx]
                                       |||| ||||
                                       ||++------ reads show the internal state.
                                       ++---++++- open bus
                     write $4100: when M=0, copy PP to RR. When M=1, RR=RR+1
                     write $4101: no visible effect
                     write $4102: [..PP ....] - Request 32 KiB PRG bank
                     write $4103: [...M ....] - PRG banking mode (0: copy, 1: increment)
                     write $8000-$FFFF: copy RR to PRG banking pins
                 */
                if (address == 0x4100)
                {
                    if (programBankMode == 0)
                        programBankRR = programBankPP;
                    else
                    {
                        ++programBankRR;
                        programBankRR &= 0x03;
                    }
                }
                else if (address == 0x4102)
                {
                    programBankPP = (value >> 4) & 0x03;
                }
                else if (address == 0x4103)
                {
                    programBankMode = (value >> 4) & 0x01;
                }
                else if (address == 0x4200)
                {
                    characterBank = value & 0x0F;
                    characterBank %= characterBankCount;
                }
                else if (address >= 0x8000)
                {
                    programBank = programBankRR;
                    programBank %= programBankCount;
                }
                Debug.WriteLine(Hex.Format(address) + " = " + Hex.Format(value));
            }
        }

        private int programBankCount;
        private int programBankMode;
        private int programBankRR;
        private int programBankPP;
        private int programBank;
        private int characterBankCount;
        private int characterBank;

        // Strike Wolf hack
        private Hack strikeWolfHack;
    }
}
