using NesCore.Storage.Hacks;
using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapAxRom : CartridgeMap
    {
        public CartridgeMapAxRom(Cartridge cartridge)
            : base(cartridge)
        {
            programBank = 0;

            if (Cartridge.Crc == 0x279710DC)
                battleToadsHack = new PpuStatusSpinHack();
        }

        public override string Name { get { return "AxROM"; } }


        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                    return Cartridge.CharacterRom[address];

                if (address >= 0x8000)
                {
                    byte value = Cartridge.ProgramRom[programBank * 0x8000 + address - 0x8000];

                    // apply ppu spin loop hack to Battletoads ROM
                    if (Cartridge.Crc == 0x279710DC)
                        value = battleToadsHack.Read(address, value);

                    return value;
                }
                if (address >= 0x6000)
                    return Cartridge.SaveRam[(ushort)(address - 0x6000)];

                Debug.WriteLine(Name + ": Unexpected read from address " + Hex.Format(address));
                return (byte)(address >> 8); // open bus
            }

            set
            {
                if (address < 0x2000)
                    Cartridge.CharacterRom[address] = value;
                else if (address >= 0x8000)
                {
                    int oldProgramBank = programBank;

                    // ---M-PPP
                    programBank = value & 7;

                    // mirror mode
                    MirrorMode = (value & 0x10) == 0x10 ? MirrorMode.Single1 : MirrorMode.Single0;

                    // invalidate address region
                    if (programBank != oldProgramBank)
                        ProgramBankSwitch?.Invoke(0x8000, 0x8000);
                }
                else if (address >= 0x6000)
                    Cartridge.SaveRam[(ushort)(address - 0x6000)] = value;
                else
                {
                    Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
                }
            }
        }

        private int programBank;
        private Hack battleToadsHack;
    }
}
