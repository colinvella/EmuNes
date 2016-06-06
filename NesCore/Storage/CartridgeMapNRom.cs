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
            prgBanks = cartridge.ProgramRom.Count / 0x4000;
            prgBank1 = 0;
            prgBank2 = prgBanks - 1;
        }

        public virtual string Name { get { return "NROM"; } }

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

                if (address >= 0xC000)
                {
                    int index = prgBank2 * 0x4000 + address - 0xC000;
                    return Cartridge.ProgramRom[index];
                }

                if (address >= 0x8000)
                {
                    int index = prgBank1 * 0x4000 + address - 0x8000;
                    return Cartridge.ProgramRom[index];
                }

                if (address >= 0x6000)
                    return Cartridge.SaveRam[address - 0x6000];

                throw new Exception("Unhandled NROM mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x2000)
                {
                    Cartridge.CharacterRom[address] = value;
                }
                else if (address >= 0x8000)
                {
                    prgBank1 = value % prgBanks;
                }
                else if (address >= 0x6000)
                { 
                    Cartridge.SaveRam[address - 0x6000] = value;
                }
                else
                    throw new Exception("Unhandled NROM mapper read at address: " + Hex.Format(address));
            }
        }

        public void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
        }

        private int prgBanks;
        private int prgBank1;
        private int prgBank2;
    }
}
