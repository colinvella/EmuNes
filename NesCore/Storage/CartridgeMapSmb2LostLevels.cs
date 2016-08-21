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
                    return Cartridge.ProgramRom[programBank9000 * 0x2000 + address % 0x2000];
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
                if (address == 0x9000)
                {
                    programBank9000 = value;
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
                }
            }
        }

        int programBank9000;
    }
}
