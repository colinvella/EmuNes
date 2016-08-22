using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapFdsHacks : CartridgeMap
    {
        public CartridgeMapFdsHacks(Cartridge cartridge) : base(cartridge)
        {
        }

        public override string Name { get { return "FDS Hacks"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    return Cartridge.CharacterRom[characterBank * 0x2000 + address];
                }
                else if (address >= 0x6000 && address < 0x8000)
                {
                    return Cartridge.ProgramRom[programBank * 0x2000 + address % 0x2000];
                }
                else if (address >= 0x8000)
                {
                    return Cartridge.ProgramRom[Cartridge.ProgramRom.Count - 0x8000 + address % 0x8000];
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected read from address " + Hex.Format(address));
                    return (byte)(address >> 8); // return open bus
                }
            }

            set
            {
                if (address < 0x2000)
                {
                    Cartridge.CharacterRom[characterBank * 0x2000 + address] = value;
                }
                else if (address == 0x8000)
                {
                    // .... CCCC
                    characterBank = value & 0x0F;
                }
                else if (address >= 0xE000)
                {
                    switch (address % 4)
                    {
                        case 0:
                            // .... PPPP
                            programBank = value & 0x0F;
                            break;
                        case 1:
                            // .... M...
                            MirrorMode = (value & Bin.Bit3) != 0 ? MirrorMode.Horizontal : MirrorMode.Vertical;
                            break;
                        case 2:
                            // .... ..E.
                            irqEnabled = (value & Bin.Bit1) != 0;
                            if (!irqEnabled)
                                irqCounter = 0;
                            break;
                    }

                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
                }
            }
        }

        public override void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            ++cpuClock;
            cpuClock %= 3;

            if (cpuClock != 0)
                return;

            if (irqEnabled)
            {
                ++irqCounter;
                if (irqCounter >= 0x6000)
                    TriggerInterruptRequest?.Invoke();
            }
        }

        private int programBank;
        private int characterBank;

        private int cpuClock;
        private bool irqEnabled;
        private int irqCounter;
    }
}
