using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapMapper201 : CartridgeMap
    {
        public CartridgeMapMapper201(Cartridge cartridge) : base(cartridge)
        {
        }

        public override string Name { get { return "Multicart 8 in 1/21 in 1"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int flatAddress = romBank * 0x2000 + address;
                    flatAddress %= Cartridge.CharacterRom.Length;
                    return Cartridge.CharacterRom[flatAddress];
                }
                else if (address >= 0x8000)
                {
                    int flatAddress = romBank * 0x8000 + address % 0x8000;
                    flatAddress %= Cartridge.ProgramRom.Count;
                    return Cartridge.ProgramRom[flatAddress];
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected read from address " + Hex.Format(address));
                    return (byte)(address >> 8); // return open bus
                }
            }

            set
            {
                if (address >= 0x8000)
                {
                    romBank = value;
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
                }
            }
        }

        public override void Reset()
        {
            romBank = 0;
        }

        private int romBank;
    }
}
