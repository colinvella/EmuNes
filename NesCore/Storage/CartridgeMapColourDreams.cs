using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapColourDreams : CartridgeMap
    {
        public CartridgeMapColourDreams(Cartridge cartridge)
            : base(cartridge)
        {
            int programBankCount = cartridge.ProgramRom.Count / 0x4000;
            characterBank = 0;
            programBank = 0;
        }

        public override string Name { get { return "Color Dreams"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                    return Cartridge.CharacterRom[characterBank * 0x2000 + address];

                if (address >= 0x8000)
                    return Cartridge.ProgramRom[programBank * 0x8000 + address - 0x8000];

                throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address >= 0x8000)
                {
                    int oldProgramBank = programBank;

                    // CCCCLLPP
                    // CCCC - CHR bank, LL - CIC chip lockout defeat, PP - PRG bank
                    programBank = value & 0x3;
                    characterBank = (value >> 4) & 0xF;

                    // invalidate address regions
                    CharacterBankSwitch?.Invoke(0x0000, 0x2000);
                    if (programBank != oldProgramBank)
                        ProgramBankSwitch?.Invoke(0x8000, 0x8000);
                }
                else
                    throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));
            }
        }

        private int characterBank;
        private int programBank;
    }
}
