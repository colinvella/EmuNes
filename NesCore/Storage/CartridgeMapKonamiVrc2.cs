using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapKonamiVrc2 : CartridgeMap
    {
        public enum Variant
        {
            Vrc2a,
            Vrc2b
        }

        public CartridgeMapKonamiVrc2(Cartridge cartridge, Variant variant) : base(cartridge)
        {
            this.variant = variant;
            mapperName = variant == Variant.Vrc2a ? "Konami VRC2 Rev A" : "Konami VRC2 Rev B";

            programRam = new byte[0x2000];

            programBankCount = Cartridge.ProgramRom.Count / 0x2000;
            programLastTwoBanksAddress = (programBankCount - 2) * 0x2000;

            characterBankCount = Cartridge.CharacterRom.Length / 0x400;
            characterBank = new int[8];
        }

        public override string Name { get { return mapperName; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bankIndex = address / 0x400;
                    int bankOffset = address % 0x400;
                    return Cartridge.CharacterRom[characterBank[bankIndex] * 0x400 + bankOffset];
                }
                else if (address >= 0x6000 && address < 0x8000)
                {
                    return programRam[address % 0x2000];
                }
                else if (address >= 0x8000 && address < 0xA000)
                {
                    return Cartridge.ProgramRom[programBank0 * 0x2000 + address % 0x2000];
                }
                else if (address >= 0xA000 && address < 0xC000)
                {
                    return Cartridge.ProgramRom[programBank1 * 0x2000 + address % 0x2000];
                }
                else if (address >= 0xC000)
                {
                    return Cartridge.ProgramRom[programLastTwoBanksAddress + address % 0x4000];
                }
                else
                    return (byte)(address >> 8); // open bus
            }

            set
            {
                if (address >= 0x6000 && address < 0x8000)
                {
                    programRam[address % 0x2000] = value;
                }
                else if (address >= 0x8000 && address < 0x8004)
                {
                    programBank0 = value & 0x1F;
                    programBank0 %= programBankCount;
                }
                else if (address >= 0x9000 && address < 0x8009)
                {
                    switch (value % 0x01)
                    {
                        case 0: MirrorMode = MirrorMode.Vertical; break;
                        case 1: MirrorMode = MirrorMode.Horizontal; break;
                    }
                }
                else if (address >= 0xA000 && address < 0xA004)
                {
                    programBank1 = value & 0x1F;
                    programBank1 %= programBankCount;
                }
                else
                    Debug.WriteLine("VRC2: Unknown write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
            }
        }

        private Variant variant;
        private string mapperName;

        private byte[] programRam;

        private int programBankCount;
        private int programBank0;
        private int programBank1;
        private int programLastTwoBanksAddress;

        private int characterBankCount;
        private int[] characterBank;
    }
}
