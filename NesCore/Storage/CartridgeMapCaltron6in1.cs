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
                if (address < 0x2000)
                {
                    int flatAddress = characterBankOuter * 0x8000;
                    int bankOffset = address % 0x8000;
                    if (characterBankInnerEnabled)
                    {
                        flatAddress += characterBankInner * 0x2000;
                        bankOffset = address % 0x2000;
                    }
                    flatAddress += bankOffset;
                    return Cartridge.CharacterRom[flatAddress];
                }
                else if (address >= 0x8000)
                {
                    return Cartridge.ProgramRom[programBank * 0x8000 + address % 0x8000];
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected read from address " + Hex.Format(address));
                    return (byte)(address >> 8); // return open bus
                }
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

        public override void Reset()
        {
            programBank = characterBankOuter = characterBankInner = 0;
            characterBankInnerEnabled = false;
            MirrorMode = MirrorMode.Vertical;
        }

        private int programBank;
        private int characterBankOuter;
        private int characterBankInner;
        private bool characterBankInnerEnabled;
    }
}
