using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapMapper58 : CartridgeMap
    {
        public CartridgeMapMapper58(Cartridge cartridge) : base(cartridge)
        {
        }

        public override string Name { get { return "Game Star 68-in-1, Study and Game 32-in-1"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    return Cartridge.CharacterRom[characterBank * 0x2000 + address];
                }
                else if (address >= 0x8000)
                {
                    int bankSize = programMode == 0 ? 0x8000 : 0x4000;
                    int bankOffset = address % bankSize;
                    int selectedBank = programMode == 0 ? (programBank >> 1) : programBank;
                    int flataddress = selectedBank * bankSize + bankOffset;
                    return Cartridge.ProgramRom[flataddress];
                }
                else
                {
                    return base[address];
                }
            }

            set
            {
                if (address >= 0x8000)
                {
                    // $8000 - FFFF:  A~[.... .... MOCC CPPP]
                    // P = PRG page select
                    // C = CHR page select (8k @ $0000)
                    // O = PRG Mode
                    // M = Mirroring(0 = Vert, 1 = Horz)
                    programBank = address & Bin.B00000111;
                    characterBank = (address >> 3) & Bin.B00000111;
                    programMode = (address >> 6) & Bin.Bit0;
                    MirrorMode = (address & Bin.Bit7) != 0 ? MirrorMode.Horizontal : MirrorMode.Vertical;

                    Debug.WriteLine(" ........MOCCCPPP");
                    Debug.WriteLine(Bin.Format(address));
                }
                else
                {
                    base[address] = value;
                }
            }
        }

        private int programMode;
        private int programBank;
        private int characterBank;
    }
}
