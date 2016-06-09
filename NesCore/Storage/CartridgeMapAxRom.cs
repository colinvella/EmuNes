using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapAxRom : CartridgeMap
    {
        public CartridgeMapAxRom(Cartridge cartridge)
        {
            Cartridge = cartridge;
            programBank = 0;
        }

        public virtual string Name { get { return "AXROM"; } }

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
                    return Cartridge.CharacterRom[address];

                if (address >= 0x8000)
                    return Cartridge.ProgramRom[programBank * 0x8000 + address - 0x8000];

                if (address >= 0x6000)
                    return Cartridge.SaveRam[address - 0x6000];

                throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x2000)
                    Cartridge.CharacterRom[address] = value;
                else if (address >= 0x8000)
                {
                    // ---M-PPP
                    programBank = value & 7;
                    Cartridge.MirrorMode = (value & 0x10) == 0x10
                        ? MirrorMode.Single1 : MirrorMode.Single0;
                }
                else if (address >= 0x6000)
                    Cartridge.SaveRam[address - 0x6000] = value;
                else
                    throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));
            }
        }

        public void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
        }

        private int programBank;
    }
}
