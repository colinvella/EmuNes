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
                    return Cartridge.CharacterRom[
                        characterBankOuter * 0x8000
                        + characterBankInner * 0x2000 
                        + address % 0x2000];
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
                    // 0110 0... ..MC CEPP (address line)
                    programBank = address & 0x07; // EPP (enabled and higher banks selected at the same time)
                    characterBankInnerEnabled = (address & Bin.Bit2) != 0;
                    characterBankOuter = (address >> 3) & 0x03;
                    MirrorMode = (address & Bin.Bit5) != 0 ? MirrorMode.Horizontal : MirrorMode.Vertical;
                }
                else if (address >= 0x8000)
                {
                    // ..ZZ..cc (data line)
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
