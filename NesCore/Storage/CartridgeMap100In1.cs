using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMap100In1 : CartridgeMap
    {
        public CartridgeMap100In1(Cartridge cartridge)
        {
            this.Cartridge = cartridge;
            bankMode = 2;
        }

        public Cartridge Cartridge { get; private set; }

        public byte this[ushort address]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                if (address >= 0x8000)
                {
                    bankMode = address & 0x03;
                    programRomBank = value & 0x3f;

                    byte mirrorMode = (value & 0x40) != 0 ? Cartridge.MirrorHorizontal : Cartridge.MirrorVertical;
                    if (Cartridge.MirrorMode != mirrorMode)
                    {
                        Cartridge.MirrorMode = mirrorMode;
                        Cartridge.MirrorModeChanged?.Invoke();
                    }
                    return;
                }
                throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
            }
        }

        public string Name
        {
            get { return "100-in-1"; }
        }

        public Action TriggerInterruptRequest
        {
            get { return null; }
            set { }
        }

        public void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites) { }

        private int bankMode;
        private int programRomBank;
        private int subBank;
    }
}
