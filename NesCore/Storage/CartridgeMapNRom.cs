using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapNRom : CartridgeMap
    {
        public CartridgeMapNRom(Cartridge cartridge)
        {
            Cartridge = cartridge;
            programBankCount = cartridge.ProgramRom.Count / 0x4000;
            programBank1 = 0;
            programBank2 = programBankCount - 1;
        }

        public override string Name { get { return "NROM"; } }

        public Cartridge Cartridge { get; private set; }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                    return Cartridge.CharacterRom[address];

                if (address >= 0xC000)
                {
                    int index = programBank2 * 0x4000 + address - 0xC000;
                    return Cartridge.ProgramRom[index];
                }

                if (address >= 0x8000)
                {
                    int index = programBank1 * 0x4000 + address - 0x8000;
                    return Cartridge.ProgramRom[index];
                }

                if (address >= 0x6000)
                    return Cartridge.SaveRam[(ushort)(address - 0x6000)];

                throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x2000)
                {
                    Cartridge.CharacterRom[address] = value;
                }
                else if (address >= 0x8000)
                {
                    programBank1 = value % programBankCount;

                    // invalidate address region
                    ProgramBankSwitch?.Invoke(0x8000, 0x4000);
                }
                else if (address >= 0x6000)
                { 
                    Cartridge.SaveRam[(ushort)(address - 0x6000)] = value;
                }
                else
                    throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));
            }
        }

        private int programBankCount;
        private int programBank1;
        private int programBank2;
    }
}
