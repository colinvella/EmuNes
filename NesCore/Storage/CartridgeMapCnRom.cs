using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapCnRom : CartridgeMap
    {
        public CartridgeMapCnRom(Cartridge cartridge)
        {
            Cartridge = cartridge;
            int programBankCount = cartridge.ProgramRom.Count / 0x4000;
            characterBank = 0;
            programBank1 = 0;
            programBank2 = programBankCount - 1;
        }

        public override string Name { get { return "CNROM"; } }

        public Cartridge Cartridge { get; private set; }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                    return Cartridge.CharacterRom[characterBank * 0x2000 + address];

                if (address >= 0xC000)
                    return Cartridge.ProgramRom[programBank2 * 0x4000 + address - 0xC000];

                if (address >= 0x8000)
                    return Cartridge.ProgramRom[programBank1 * 0x4000 + address - 0x8000];

                if (address >= 0x6000)
                    return Cartridge.SaveRam[(ushort)(address - 0x6000)];

                throw new Exception("Unhandled CNROM mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x2000)
                    Cartridge.CharacterRom[characterBank * 0x2000 + address] = value;
                else if (address >= 0x8000)
                    characterBank = value % 3;
                else if (address >= 0x6000)
                    Cartridge.SaveRam[(ushort)(address - 0x6000)] = value;
                else
                    throw new Exception("Unhandled CNROM mapper write at address: " + Hex.Format(address));
            }
        }

        private int characterBank;
        private int programBank1;
        private int programBank2;
    }
}
