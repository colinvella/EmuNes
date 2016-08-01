using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapHomeBrew29 : CartridgeMap
    {
        public CartridgeMapHomeBrew29(Cartridge cartridge) : base(cartridge)
        {
            MirrorMode = MirrorMode.Vertical;
            programLastAddress16k = Cartridge.ProgramRom.Count - 0x4000;
        }

        public override string Name { get { return "HomeBrew Mapper 29"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    return Cartridge.CharacterRom[characterBank * 0x2000 + address];
                }
                else if (address >= 0x8000 && address < 0xC000)
                {
                    return Cartridge.ProgramRom[programBank * 0x4000 + address % 0x4000];
                }
                else if (address >= 0xC000)
                {
                    return Cartridge.ProgramRom[programLastAddress16k + address % 0x4000];
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected read from address " + Hex.Format(address));
                    return (byte)(address >> 8);
                }
            }

            set
            {
                if (address > 0x8000)
                {
                    int oldProgramBank = programBank;
                    programBank = (value >> 2) & 0x07;
                    if (programBank != oldProgramBank)
                        ProgramBankSwitch?.Invoke(0x8000, 0x4000);
                    
                    int oldCharacterBank = characterBank;
                    characterBank = value & 0x03;
                    if (characterBank != oldCharacterBank)
                        CharacterBankSwitch?.Invoke(0x0000, 0x2000);
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " to address " + Hex.Format(address));
                }
            }
        }

        private int programLastAddress16k;
        private int programBank;
        private int characterBank;
    }
}
