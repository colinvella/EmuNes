using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapTaitoTc0190 : CartridgeMap
    {
        public CartridgeMapTaitoTc0190(Cartridge cartridge) : base(cartridge)
        {
            programBankCount = Cartridge.ProgramRom.Count / 0x2000;
            programBankLast16kAddress = Cartridge.ProgramRom.Count - 0x4000;

            characterBankCount2k = Cartridge.CharacterRom.Length / 0x800;
            characterBankCount1k = characterBankCount2k * 2;
            characterBank2k = new int[2];
            characterBank1k = new int[4];
        }

        public override string Name { get { return "Taito TC0190"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x1000)
                {
                    int bankIndex = address / 0x800;
                    int bankOffset = address % 0x800;
                    return Cartridge.CharacterRom[characterBank2k[bankIndex] * 0x800 + bankOffset];
                }
                else if (address < 0x2000)
                {
                    int bankIndex = (address - 0x1000) / 0x400;
                    int bankOffset = address % 0x400;
                    return Cartridge.CharacterRom[characterBank1k[bankIndex] * 0x400 + bankOffset];
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
                    return Cartridge.ProgramRom[programBankLast16kAddress + address % 0x4000];
                }
                else
                    return (byte)(address >> 8); // open bus
            }

            set
            {
                if (address == 0x8000)
                {
                    //.MPP PPPP
                    programBank0 = value & 0x3F;
                    programBank0 %= programBankCount;
                    MirrorMode = ((value >> 6) & 0x01) == 1 ? MirrorMode.Horizontal : MirrorMode.Vertical;
                }
                else if (address == 0x8001)
                {
                    //..PP PPPP
                    programBank1 = value & 0x3F;
                    programBank1 %= programBankCount;
                }
                else if (address == 0x8002 || address == 0x8003)
                {
                    characterBank2k[address - 0x8002] = value % characterBankCount2k;
                }
                else if (address >= 0xA000 && address < 0xA004 )
                {
                    characterBank1k[address - 0xA000] = value % characterBankCount1k;
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
                }
            }
        }

        private int programBankCount;
        private int programBank0;
        private int programBank1;
        private int programBankLast16kAddress;

        private int characterBankCount2k;
        private int characterBankCount1k;
        private int[] characterBank2k;
        private int[] characterBank1k;
    }
}
