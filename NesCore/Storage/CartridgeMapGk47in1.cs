using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapGk47in1 : CartridgeMap
    {
        public CartridgeMapGk47in1(Cartridge cartridge) : base(cartridge)
        {
        }

        public override string Name { get { return "GK 4-in-1"; } }

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
                    int bankSize = programMode == 0 ? 0x4000 : 0x8000;
                    int bankOffset = address % bankSize;
                    int selectedBank = programMode == 0 ? programBank : (programBank >> 1);
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
                if ((address & 0x8800) == 0x8000)
                {
                    // $8000:  [.H.. .AAA]
                    // H = High bit of CHR reg (bit 4)
                    // A = Low 3 bits of CHR Reg (OR with 'B' bits)
                    characterBankH = (value & Bin.Bit6) >> 3;
                    characterBankA = value & Bin.B00000111;
                    characterBank = characterBankH | characterBankA | characterBankB;
                }
                else if ((address & 0x8800) == 0x8800)
                {
                    // $8800:  [PPPO MBBB]
                    // P = PRG Reg
                    // O = PRG Mode
                    // M = Mirroring (0=Vert, 1=Horz)
                    // B = Low 3 bits of CHR Reg (OR with 'A' bits)
                    programBank = value >> 5;
                    programMode = (value & Bin.Bit4) >> 4;
                    MirrorMode = (value & Bin.Bit3) != 0 ? MirrorMode.Horizontal : MirrorMode.Vertical;
                    characterBankB = value & Bin.B00000111;
                    characterBank = characterBankH | characterBankA | characterBankB;
                }
                else
                {
                    base[address] = value;
                }
            }
        }

        private int programMode;
        private int programBank;
        private int characterBankH;
        private int characterBankA;
        private int characterBankB;
        private int characterBank;
    }
}
