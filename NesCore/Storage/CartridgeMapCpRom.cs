using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapCpRom : CartridgeMap
    {
        public CartridgeMapCpRom(Cartridge cartridge)
            : base(cartridge)
        {
            characterBank = 0;
            // CHR bank 2 and 3
            characterRam = new byte[0x2000];
        }

        public override string Name { get { return "CPROM"; } }
        
        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x1000)
                    return Cartridge.CharacterRom[address];

                if (address < 0x2000)
                {
                    int index = address & 0xFFF;
                    if (characterBank < 2)
                        return Cartridge.CharacterRom[characterBank * 0x1000 + index];
                    else
                        return characterRam[(characterBank - 2) * 0x1000 + index];
                }

                if (address >= 0x8000)
                    return Cartridge.ProgramRom[address - 0x8000];

                throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x1000)
                    Cartridge.CharacterRom[address] = value;
                else if (address < 0x2000)
                {
                    int index = address & 0xFFF;
                    if (characterBank < 2)
                        Cartridge.CharacterRom[characterBank * 0x1000 + index] = value;
                    else
                        characterRam[(characterBank - 2) * 0x1000 + index] = value;
                }
                else if (address >= 0x8000)
                {
                    characterBank = value & 0x03;

                    // invalidate address regions
                    CharacterBankSwitch?.Invoke(0x0000, 0x1000);
                }
                else
                    throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));
            }
        }

        private int characterBank;
        private byte[] characterRam;
    }
}
