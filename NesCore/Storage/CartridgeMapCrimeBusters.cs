using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapCrimeBusters : CartridgeMap
    {
        public CartridgeMapCrimeBusters(Cartridge cartridge)
            : base(cartridge)
        {
            programBankCount = cartridge.ProgramRom.Count / 0x8000;
            programBank = 0;
            characterBank = 0;
        }

        public override string Name { get { return "Crime Busters"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    return Cartridge.CharacterRom[characterBank * 0x2000 + address];
                }
                else if (address >= 0x8000)
                {
                    return Cartridge.ProgramRom[programBank * 0x8000 + address % 0x8000];
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected read from address: " + Hex.Format(address));
                    return (byte)(address >> 8); // open bus
                }
            }

            set
            {
                if (address >= 0x7000 && address < 0x8000)
                {
                    // .... CCPP

                    int oldProgramBank = programBank;
                    programBank = value & 0x03;

                    // invalidate address region
                    if (programBank != oldProgramBank)
                        ProgramBankSwitch?.Invoke(0x8000, 0x8000);

                    int oldCharacterBank = characterBank;
                    characterBank = (value >> 2) & 0x03;

                    // invalidate address region
                    if (characterBank != oldCharacterBank)
                        CharacterBankSwitch?.Invoke(0x0000, 0x2000);
                }
                else
                {
                    Debug.WriteLine(Name + ": unexpected write of value " + Hex.Format(value) + " at address: " + Hex.Format(address));
                }
            }
        }

        private int programBankCount;
        private int programBank;
        private int characterBank;
    }
}
