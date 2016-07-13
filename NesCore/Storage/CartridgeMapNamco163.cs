using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapNamco163 : CartridgeMap
    {
        public CartridgeMapNamco163(Cartridge cartridge)
        {
            Cartridge = cartridge;

            programRam = new byte[0x2000];

            programBank = new int[4];

            int programBankCount = cartridge.ProgramRom.Count / 0x2000;

            programBank[3] = programBankCount - 1;
        }

        private Cartridge Cartridge { get; set; }

        public override byte this[ushort address]
        {
            get
            {
                if (address >= 0x6000 && address < 0x8000)
                {
                    return programRam[address - 0x6000];
                }
                if (address >= 0x8000)
                {
                    int bankIndex = (address - 0x8000) / 0x2000;
                    int bankOffset = address % 0x2000;
                    return Cartridge.ProgramRom[programBank[bankIndex] * 0x2000 + bankOffset];
                }
                else
                    return (byte)(address >> 8); // open bus for anything unspecified
            }

            set
            {
                if (address >= 0x4800 && address < 0x5000)
                {
                    // sound data port
                }
                else if (address >= 0x5000 && address < 0x5800)
                {
                    // irq low 8 bits
                    irqCounter &= 0x7F00;
                    irqCounter |= value;
                }
                else if (address >= 0x5800 && address < 0x6000)
                {
                    // irq enable and high 7 bits
                    irqCounter &= 0x00FF;
                    irqCounter |= (ushort)((value & 0x7F) << 8);
                    irqEnabled = (value & 0x80) != 0;
                }
                else if (address >= 0x6000 && address < 0x8000)
                {
                    programRam[address - 0x6000] = value;
                }
                else if (address >= 0xE000 && address < 0xE800)
                {
                    // MMPP PPPP
                    // |||| ||||
                    // ||++-++++-Select 8KB page of PRG-ROM at $8000
                    // |+--------Namco 129, 163 only: Disable sound if set
                    programBank[0] = value & 0x3F;
                }
                else if (address >= 0xE800 && address < 0xF000)
                {
                    // HLPP PPPP
                    // |||| ||||
                    // ||++-++++-Select 8KB page of PRG-ROM at $A000
                    // |+--------Disable CHR - RAM at $0000 -$0FFF(Namco 129, 163 only)
                    // |           0: Pages $E0 -$FF use NT RAM as CHR - RAM
                    // |           1: Pages $E0 -$FF are the last $20 banks of CHR - ROM
                    // +---------Disable CHR - RAM at $1000 -$1FFF(Namco 129, 163 only)
                    //             0: Pages $E0 -$FF use NT RAM as CHR - RAM
                    //             1: Pages $E0 -$FF are the last $20 banks of CHR - ROM
                    programBank[1] = value & 0x3F;
                }
                else if (address >= 0xF000 && address < 0xF800)
                {
                    // ..PP PPPP
                    //   || ||||
                    //   ++-++++- Select 8KB page of PRG-ROM at $C000
                    programBank[2] = value & 0x3F;
                }
            }
        }

        public override string Name { get { return "Namco 163"; } }

        private byte[] programRam;
        private int[] programBank;
        private bool irqEnabled;
        private ushort irqCounter;
    }

}
