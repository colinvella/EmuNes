using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapSuperHiK4in1: CartridgeMapMmc3
    {
        public CartridgeMapSuperHiK4in1(Cartridge cartridge) : base(cartridge, false)
        {
        }

        public override string Name { get { return "Super HiK 4-in-1"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bankIndex = address / 0x0400;
                    int bankOffset = address % 0x0400;

                    int flatAddress = outerBlock * 0x20000
                        + (characterBankOffsets[bankIndex] % 0x20000) + bankOffset;

                    return Cartridge.CharacterRom[flatAddress];
                }
                else if (address >= 0x8000)
                {
                    int flatAddress = outerBlock * 0x20000; // 128k block
                    if (programModeNormal)
                    {
                        address -= 0x8000;
                        int bankIndex = address / 0x2000;
                        int bankOffset = address % 0x2000;

                        flatAddress += (programBankOffsets[bankIndex] % 0x20000) + bankOffset;
                    }
                    else
                    {
                        flatAddress += programBank * 0x8000 + address % 0x8000; // 32k banks
                    }

                    return Cartridge.ProgramRom[flatAddress];
                }
                else
                {
                    return base[address];
                }
            }

            set
            {
                if (address >= 0x6000 && address < 0x8000)
                {
                    // $6000-7FFF:  [BBPP ...O]  Multicart reg
                    // B = Block
                    // P = 32k PRG Reg
                    // O = PRG Mode(0 = 32k mode)
                    outerBlock = value >> 6;
                    programBank = (value >> 4) & 0x03;
                    programModeNormal = (value & 0x01) != 0;

                    ProgramBankSwitch?.Invoke(0x8000, 0x8000);
                }
                else
                {
                    base[address] = value;
                }
            }
        }

        public override void Reset()
        {
            this[0x6000] = 0x00;
        }
        private int outerBlock;
        private int programBank;
        private bool programModeNormal;
    }
}
