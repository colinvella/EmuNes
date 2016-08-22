using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapCaltron6in1 : CartridgeMap
    {
        public CartridgeMapCaltron6in1(Cartridge cartridge) : base(cartridge)
        {
        }

        public override string Name { get { return "Caltron 6-in-1"; } }


        public override byte this[ushort address]
        {
            get
            {
                Debug.WriteLine(Name + ": Unexpected read from address " + Hex.Format(address));
                return (byte)(address >> 8); // return open bus
            }

            set
            {
                if (address >= 0x6000 && address < 0x6800)
                {
                    // ..MCCEPP
                    programBank = value & 0x03;
                    characterBankInnerEnabled = (value & Bin.Bit2) != 0;
                    characterBankOuter = (value >> 3) & 0x03;
                    MirrorMode = (value & Bin.Bit5) != 0 ? MirrorMode.Horizontal : MirrorMode.Vertical;
                }
                if (address >= 0x8000)
                {
                    // ..ZZ..cc
                    if (characterBankInnerEnabled)
                    {
                        characterBankInner = value & 0x03;
                    }
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
                }
            }
        }

        private int programBank;
        private int characterBankOuter;
        private int characterBankInner;
        private bool characterBankInnerEnabled;
    }
}
