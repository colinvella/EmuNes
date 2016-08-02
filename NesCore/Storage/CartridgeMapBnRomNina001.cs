using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapBnRomNina001 : CartridgeMap
    {
        public CartridgeMapBnRomNina001(Cartridge cartridge)
            : base(cartridge)
        {
            board = Cartridge.CharacterRom.Length > 0x2000 ? Board.Nina001 : Board.BnRom;

            programBankCount = Cartridge.ProgramRom.Count / 0x8000;
            programBank = programBankCount - 1;

            if (board == Board.BnRom)
            {
                mapperName = "BNROM";
            }
            else //if (board == Board.Nina001)
            {
                mapperName = "NINA-001";
                characterBank = new int[2];
                programRam = new byte[0x2000];
            }
        }

        public override string Name { get { return mapperName; } }

        public override byte this[ushort address]
        {
            get
            {
                if (board == Board.BnRom)
                {
                    if (address < 0x2000)
                    {
                        return Cartridge.CharacterRom[address];
                    }
                    else if (address >= 0x8000)
                    {
                        return Cartridge.ProgramRom[programBank * 0x8000 + address % 0x8000];
                    }
                    else
                        return (byte)(address >> 8); // open bus
                }
                else // NINA-001
                {
                    if (address < 0x2000)
                    {
                        int bankIndex = address / 0x1000;
                        int bankOffset = address % 0x1000;
                        return Cartridge.CharacterRom[characterBank[bankIndex] * 0x1000 + bankOffset];
                    }
                    else if (address >= 0x6000 && address < 0x8000)
                    {
                        return programRam[address % 0x2000];
                    }
                    else if (address >= 0x8000)
                    {
                        return Cartridge.ProgramRom[programBank * 0x8000 + address % 0x8000];
                    }
                    else
                        return (byte)(address >> 8); // open bus
                }
            }

            set
            {
                if (board == Board.BnRom)
                {
                    if (address < 0x2000)
                    {
                        Cartridge.CharacterRom[address] = value;
                    }
                    else if (address >= 0x8000)
                    {
                        programBank = value & 0x3;
                    }
                    else
                    {
                        Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
                    }
                }
                else // NINA-001
                {
                    if (address >= 0x6000 && address < 0x7FFD)
                    {
                        programRam[address % 0x2000] = value;
                    }
                    if (address == 0x7FFD)
                    {
                        programBank = value & 0x01;
                    }
                    else if (address == 0x7FFE || address == 0x7FFF)
                    {
                        characterBank[address - 0x7FFE] = value & 0x0F;
                    }
                    else
                    {
                        Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
                    }
                }
            }
        }

        private Board board;
        private string mapperName;

        private int programBankCount;
        private int programBank;

        private int[] characterBank;

        private byte[] programRam;

        private enum Board
        {
            BnRom,
            Nina001
        }
    }
}
