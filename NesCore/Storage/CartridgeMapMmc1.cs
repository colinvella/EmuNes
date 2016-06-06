using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapMmc1 : CartridgeMap
    {
        public CartridgeMapMmc1(Cartridge cartridge)
        {
            Cartridge = cartridge;
            programBankOffsets = new int[2];
            characterBankOffsets = new int[2];

            shiftRegister = 0x10;
            programBankOffsets[1] = GetProgramBankOffset(-1);
        }

        public string Name { get { return "MMC1"; } }

        public Cartridge Cartridge { get; private set; }

        public Action TriggerInterruptRequest
        {
            get { return null; }
            set { }
        }

        public byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bank = address / 0x1000;
                    int offset = address % 0x1000;
                    return Cartridge.CharacterRom[characterBankOffsets[bank] + offset];
                }
                else if (address >= 0x8000)
                {
                    address -= 0x8000;
                    int bank = address / 0x4000;
                    int offset = address % 0x4000;
                    return Cartridge.ProgramRom[programBankOffsets[bank] + offset];
                }
                else if (address >= 0x6000)
                    return Cartridge.SaveRam[address - 0x6000];
                else
                    throw new Exception("Unhandled MMC1 mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x2000)
                {
                    int bank = address / 0x1000;
                    int offset = address % 0x1000;
                    Cartridge.CharacterRom[characterBankOffsets[bank] + offset] = value;
                }
                else if (address >= 0x8000)
                {
                    LoadRegister(address, value);
                }
                else if (address >= 0x6000)
                {
                    Cartridge.SaveRam[address - 0x6000] = value;
                }
            }
        }

        public void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
        }

        private void LoadRegister(ushort address, byte value)
        {
            if ((value & 0x80) == 0x80)
            {
                shiftRegister = 0x10;
                WriteControl((byte)(control | 0x0C));
            }
            else
            {
                bool complete = (shiftRegister & 1) == 1;
                shiftRegister >>= 1;
                shiftRegister |= (byte)((value & 1) << 4);

                if (complete)
                {
                    WriteRegister(address, shiftRegister);
                    shiftRegister = 0x10;
                }
            }
        }

        private void WriteRegister(ushort address, byte value)
        {
            if (address <= 0x9FFF)
                WriteControl(value);
            else if (address <= 0xBFFF)
                WriteCharacterBank0(value);
            else if (address <= 0xDFFF)
                WriteCharacterBank1(value);
            else if (address <= 0xFFFF)
                WriteProgramBank(value);
        }

        // Control (internal, $8000-$9FFF)
        private void WriteControl(byte value)
        {
            control = value;
            characterBankMode = (byte)((value >> 4) & 1);
            programBankMode = (byte)((value >> 2) & 3);
            int mirror = value & 3;

            switch (mirror)
            {
                case 0: Cartridge.MirrorMode = Cartridge.MirrorSingle0; break;
                case 1: Cartridge.MirrorMode = Cartridge.MirrorSingle1; break;
                case 2: Cartridge.MirrorMode = Cartridge.MirrorVertical; break;
                case 3: Cartridge.MirrorMode = Cartridge.MirrorHorizontal; break;
            }
            UpdateOffsets();

            // call mirrir mode change hook
            Cartridge.MirrorModeChanged?.Invoke();
        }

        // CHR bank 0 (internal, $A000-$BFFF)
        private void WriteCharacterBank0(byte value)
        {
            characterBank0 = value;
            UpdateOffsets();
        }

        // CHR bank 1 (internal, $C000-$DFFF)
        private void WriteCharacterBank1(byte value)
        {
            characterBank1 = value;
            UpdateOffsets();
        }

        // PRG bank (internal, $E000-$FFFF)
        private void WriteProgramBank(byte value)
        {
            programBank = (byte)(value & 0x0F);
            UpdateOffsets();
        }

        private int GetProgramBankOffset(int index)
        {
            if (index >= 0x80)
                index -= 0x100;

            index %= (Cartridge.ProgramRom.Count / 0x4000);
            int offset = index * 0x4000;
            if (offset < 0)
                offset += Cartridge.ProgramRom.Count;
            return offset;
        }

        private int GetCharacterBankOffset(int index)
        {
            if (index >= 0x80)
                index -= 0x100;

            index %= Cartridge.CharacterRom.Length / 0x1000;
            int offset = index * 0x1000;
            if (offset < 0)
                offset += Cartridge.CharacterRom.Length;
            return offset;
        }

        // PRG ROM bank mode (0, 1: switch 32 KB at $8000, ignoring low bit of bank number;
        //                    2: fix first bank at $8000 and switch 16 KB bank at $C000;
        //                    3: fix last bank at $C000 and switch 16 KB bank at $8000)
        // CHR ROM bank mode (0: switch 8 KB at a time; 1: switch two separate 4 KB banks)
        private void UpdateOffsets()
        {
            switch (programBankMode)
            {
                case 0:
                case 1:
                    programBankOffsets[0] = GetProgramBankOffset(programBank & 0xFE);
                    programBankOffsets[1] = GetProgramBankOffset(programBank | 0x01);
                    break;
                case 2:
                    programBankOffsets[0] = 0;
                    programBankOffsets[1] = GetProgramBankOffset(programBank);
                    break;
                case 3:
                    programBankOffsets[0] = GetProgramBankOffset(programBank);
                    programBankOffsets[1] = GetProgramBankOffset(-1);
                    break;
            }

            switch (characterBankMode)
            {
                case 0:
                    characterBankOffsets[0] = GetCharacterBankOffset(characterBank0 & 0xFE);
                    characterBankOffsets[1] = GetCharacterBankOffset(characterBank0 | 0x01);
                    break;
                case 1:
                    characterBankOffsets[0] = GetCharacterBankOffset(characterBank0);
                    characterBankOffsets[1] = GetCharacterBankOffset(characterBank1);
                    break;
            }
        }

        private byte shiftRegister;
        private byte control;
        private byte programBankMode;
        private byte characterBankMode;
        private byte programBank;
        private byte characterBank0;
        private byte characterBank1;
        private int[] programBankOffsets;
        private int[] characterBankOffsets;
    }
}
