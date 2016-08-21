using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapSmb2LostLevels : CartridgeMap
    {
        public CartridgeMapSmb2LostLevels(Cartridge cartridge)
            : base(cartridge)
        {
        }

        public override string Name { get { return "SMB2j Lost Levels"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    return Cartridge.CharacterRom[address];
                }
                else if (address >= 0x6000 && address < 0x8000)
                {
                    return Cartridge.ProgramRom[6 * 0x2000 + address % 0x2000];
                }
                else if (address >= 0x8000 && address < 0xA000)
                {
                    return Cartridge.ProgramRom[4 * 0x2000 + address % 0x2000];
                }
                else if (address >= 0xA000 && address < 0xC000)
                {
                    return Cartridge.ProgramRom[5 * 0x2000 + address % 0x2000];
                }
                else if (address >= 0xC000 && address < 0xE000)
                {
                    return Cartridge.ProgramRom[programBank * 0x2000 + address % 0x2000];
                }
                else if (address >= 0xE000)
                {
                    return Cartridge.ProgramRom[7 * 0x2000 + address % 0x2000];
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected read from address " + Hex.Format(address));
                    return (byte)(address >> 8); // open bus
                }
            }

            set
            {
                if (address >= 0x8000 && address < 0xA000)
                {
                    irqEnabled = false;
                    irqCounter = 0;
                }
                else if (address >= 0xA000 && address < 0xC000)
                {
                    irqEnabled = true;
                }
                else if (address >= 0xE000)
                {
                    programBank = value % 8;
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
                if (irqCounter >= 4096)
                    TriggerInterruptRequest?.Invoke();
            }
        }

        private int programBank;
        private bool irqEnabled;
        private int irqCounter;
        private int cpuClock;
    }
}
