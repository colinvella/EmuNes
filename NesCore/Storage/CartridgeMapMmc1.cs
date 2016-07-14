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
            : base(cartridge)
        {
            programBankOffsets = new int[2];
            characterBankOffsets = new int[2];

            outerProgramRomBankSupported = cartridge.ProgramRom.Count > 0x40000;
            programRamBanksSupported = cartridge.CharacterRom.Count() == 0x2000;
            if (programRamBanksSupported)
                programRam = new byte[0x2000 * 0x04];

            shiftRegister = 0x10;
            programBankOffsets[1] = GetProgramBankOffset(-1);
        }

        public override string Name { get { return "MMC1"; } }

        public override byte this[ushort address]
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
                    return Cartridge.ProgramRom[outerProgramRomBank + programBankOffsets[bank] + offset];
                }
                else if (address >= 0x6000)
                {
                    ushort offset = (ushort)(address - 0x6000);
                    if (programRamBanksSupported)
                        return programRam[programRamBank * 0x2000 + offset];
                    else
                        return Cartridge.SaveRam[offset];
                }
                else
                    throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x2000)
                {
                    int bank = address / 0x1000;
                    int offset = address % 0x1000;
                    Cartridge.CharacterRom[characterBankOffsets[bank] + offset] = value;

                    CharacterBankSwitch?.Invoke((ushort)characterBankOffsets[bank], 0x1000);
                }
                else if (address >= 0x8000)
                {
                    LoadRegister(address, value);
                }
                else if (address >= 0x6000)
                {
                    ushort offset = (ushort)(address - 0x6000);
                    if (programRamBanksSupported)
                        programRam[programRamBank * 0x2000 + offset] = value;
                    else
                        Cartridge.SaveRam[offset] = value;
                }
            }
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

            UpdateOffsets();

            // mirror mode
            switch (value & 3)
            {
                case 0: MirrorMode = MirrorMode.Single0; break;
                case 1: MirrorMode = MirrorMode.Single1; break;
                case 2: MirrorMode = MirrorMode.Vertical; break;
                case 3: MirrorMode = MirrorMode.Horizontal; break;
            }

        }

        // CHR bank 0 (internal, $A000-$BFFF)
        private void WriteCharacterBank0(byte value)
        {
            characterBank0 = value;
            UpdateOffsets();

            CharacterBankSwitch?.Invoke(0x0000, 0x1000);

            // both chr modes
            HandleSxRomVariants(value);
        }

        // CHR bank 1 (internal, $C000-$DFFF)
        private void WriteCharacterBank1(byte value)
        {
            characterBank1 = value;
            UpdateOffsets();

            CharacterBankSwitch?.Invoke(0x1000, 0x1000);

            // ignored in 8k CHAR mode
            if (characterBankMode == 1)
                HandleSxRomVariants(value);
        }

        private void HandleSxRomVariants(byte value)
        {
            // try to determine if SUROM variant
            if (outerProgramRomBankSupported)
            {
                // outer bank is determined by bit 4 of char bank register 0 / 1
                outerProgramRomBank = (value & 0x10) != 0 ? 0x40000 : 0x0000;
            }

            // switchable RAM bank at $6000-$7FFF controlled by bits 3 and 2 (4 banks)
            if (programRamBanksSupported)
            {
                programRamBank = (value & 0x0C) >> 2;
            }
        }

        // PRG bank (internal, $E000-$FFFF)
        private void WriteProgramBank(byte value)
        {
            programBank = (byte)(value & 0x0F);
            UpdateOffsets();

            ProgramBankSwitch?.Invoke(0xE000, 0x2000);

            // TODO: ram chip enable
        }

        private int GetProgramBankOffset(int index)
        {
            if (index >= 0x80)
                index -= 0x100;

            int outerBankSize = Math.Min(0x40000, Cartridge.ProgramRom.Count);

            index %= (outerBankSize / 0x4000);
            int offset = index * 0x4000;
            if (offset < 0)
                offset += outerBankSize;
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
                    // emulate 16K switchable bank
                    programBankOffsets[0] = GetProgramBankOffset(programBank & 0xFE);
                    programBankOffsets[1] = GetProgramBankOffset(programBank | 0x01);
                    break;
                case 2:
                    // range $8000-$BFFF fixed to first bank, $C000-$FFFF switchable
                    programBankOffsets[0] = 0;
                    programBankOffsets[1] = GetProgramBankOffset(programBank);
                    break;
                case 3:
                    // range $8000-$BFFF switchable, $C000-$FFFF fixed to last bank
                    programBankOffsets[0] = GetProgramBankOffset(programBank);
                    programBankOffsets[1] = GetProgramBankOffset(-1);
                    break;
            }

            switch (characterBankMode)
            {
                case 0:
                    // emulate 8K switchable bank
                    characterBankOffsets[0] = GetCharacterBankOffset(characterBank0 & 0xFE);
                    characterBankOffsets[1] = GetCharacterBankOffset(characterBank0 | 0x01);
                    break;
                case 1:
                    // emulate individual 4K banks
                    characterBankOffsets[0] = GetCharacterBankOffset(characterBank0);
                    characterBankOffsets[1] = GetCharacterBankOffset(characterBank1);
                    break;
            }
        }

        private bool outerProgramRomBankSupported;
        private bool programRamBanksSupported;
        private byte shiftRegister;
        private byte control;
        private byte programBankMode;
        private byte characterBankMode;
        private byte programBank;
        private byte characterBank0;
        private byte characterBank1;
        private int[] programBankOffsets;
        private int[] characterBankOffsets;
        private int outerProgramRomBank;
        private int programRamBank;
        private byte[] programRam;
    }
}
