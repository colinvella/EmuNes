using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapGxRom : CartridgeMap
    {
        public CartridgeMapGxRom(Cartridge cartridge)
        {
            Cartridge = cartridge;
            programBank = 0;
            characterBank = 0;
        }

        public virtual string Name { get { return "GxROM"; } }

        public Cartridge Cartridge { get; private set; }

        public Action TriggerInterruptRequest
        {
            get { return null; }
            set { }
        }

        public byte this[ushort address]
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
                if (address < 0x2000)
                    Cartridge.CharacterRom[characterBank * 0x2000 + address] = value;
                else if (address >= 0x8000)
                {
                    // --PP--CC
                    programBank = (value >> 4) & 7;
                    characterBank = value & 7;
                }
                else if (address >= 0x6000)
                    Cartridge.SaveRam[(ushort)(address - 0x6000)] = value;
                else
                    throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));
            }
        }

        public void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites) { }

        private int programBank;
        private int characterBank;
    }
}
