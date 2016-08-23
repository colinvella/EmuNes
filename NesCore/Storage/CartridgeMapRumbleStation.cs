using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapRumbleStation : CartridgeMap
    {
        public CartridgeMapRumbleStation(Cartridge cartridge)
            : base(cartridge)
        {
            programBank = 0;
            characterBank = 0;
        }

        public override string Name { get { return "Rumble Station (Color Dreams Multicart)"; } }

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
                if (address >= 0x6000 && address < 0x8000)
                {
                    // $6000-7FFF:  [CCCC PPPP]   High CHR, PRG bits
                    int oldProgramBank = programBank;
                    int oldCharacterBank = characterBank;

                    programBank &= 0x01; // clear bit1 onwards (preserve PRG low bit)
                    programBank |= (value & 0x0F) << 1; // set bit1-bit4 PRG high bits

                    characterBank &= 0x07; // clear bit3 onwards (priserve CHR low bits)
                    characterBank |= ((value >> 4) << 3); // set bit3-bit6 CHR high bits)

                    // invalidate address regions
                    if (programBank != oldProgramBank)
                        ProgramBankSwitch?.Invoke(0x8000, 0x8000);
                    if (characterBank != oldCharacterBank)
                        CharacterBankSwitch?.Invoke(0x0000, 0x2000);
                }
                if (address >= 0x8000)
                {
                    //  $8000-FFFF:  [.CCC ...P]   Low CHR, PRG bits

                    int oldProgramBank = programBank;
                    int oldCharacterBank = characterBank;

                    programBank &= 0xFE; // clear bit 0
                    programBank |= value & 0x01; // set PRG low bit

                    characterBank &= 0xF8; // clear bits 0 - 2
                    characterBank |= (value >> 4) & 0x07; // set CHR low bits

                    // invalidate address regions
                    if (programBank != oldProgramBank)
                        ProgramBankSwitch?.Invoke(0x8000, 0x8000);
                    if (characterBank != oldCharacterBank)
                        CharacterBankSwitch?.Invoke(0x0000, 0x2000);
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
                }
            }
        }

        public override void Reset()
        {
            this[0x6000] = 0x00;
        }

        private int programBank;
        private int characterBank;
    }
}
