using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapMultiMmc3 : CartridgeMap
    {
        public CartridgeMapMultiMmc3(Cartridge cartridge)
            : base(cartridge)
        {
            registers = new byte[8];
            programBankOffsets = new int[4];
            characterBankOffsets = new int[8];

            SelectOuterBank(0);

            programBankOffsets[0] = GetProgramBankOffset(0);
            programBankOffsets[1] = GetProgramBankOffset(1);
            programBankOffsets[2] = GetProgramBankOffset(-2);
            programBankOffsets[3] = GetProgramBankOffset(-1);
        }

        public override string Name { get { return "MMC3 Multicart"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bank = address / 0x0400;
                    int offset = address % 0x0400;

                    int flatAddress = innerCharacterBankOffset + characterBankOffsets[bank] + offset;

                    return Cartridge.CharacterRom[flatAddress];
                }
                else if (address >= 0x8000)
                {
                    address -= 0x8000;
                    int bank = address / 0x2000;
                    int offset = address % 0x2000;

                    return Cartridge.ProgramRom[innerProgramBankOffset + programBankOffsets[bank] + offset];
                }
                else if (address >= 0x6000)
                    return (byte)(address >> 8); // open bus (no save ram)
                else
                {
                    Debug.WriteLine(Name + ": Unexpected read from address " + Hex.Format(address));
                    return (byte)(address >> 8); // return open bus
                }
            }

            set
            {
                if (address < 0x2000)
                {
                    int bank = address / 0x0400;
                    int offset = address % 0x0400;

                    int flatAddress = characterBankOffsets[bank] + offset;

                    Cartridge.CharacterRom[innerCharacterBankOffset + flatAddress] = value;
                }
                else if (address >= 0x8000)
                    WriteRegister(address, value);
                else if (address >= 0x6000)
                {
                    SelectOuterBank(value);
                    UpdateOffsets();
                }
                else
                {
                    // Somari homebrew ROM writes to $4100 as part mapper 116 to switch between VRC2/MMC3/MMc1
                    Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " at address " + Hex.Format(address));
                }
            }
        }

        public override void Reset()
        {
            SelectOuterBank(0);

            programBankOffsets[0] = GetProgramBankOffset(0);
            programBankOffsets[1] = GetProgramBankOffset(1);
            programBankOffsets[2] = GetProgramBankOffset(-2);
            programBankOffsets[3] = GetProgramBankOffset(-1);

        }

        public override void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            if (cycle != 260)
                return;

            if (scanLine > 239 && scanLine < 261)
                return;

            if (!showBackground && !showSprites)
                return;

            HandleScanLine();
        }

        private void HandleScanLine()
        {
            if (irqCounter == 0)
                irqCounter = irqReload;
            else
            {
                --irqCounter;
                if (irqCounter == 0 && irqEnable)
                    TriggerInterruptRequest?.Invoke();
            }
        }

        private void WriteRegister(ushort address, byte value)
        {
            if (address <= 0x9FFF && address % 2 == 0)
                WriteBankSelect(value);
            else if (address <= 0x9FFF && address % 2 == 1)
                WriteBankData(value);
            else if (address <= 0xBFFF && address % 2 == 0)
                WriteMirror(value);
            else if (address <= 0xBFFF && address % 2 == 1)
                WriteProtect(value);
            else if (address <= 0xDFFF && address % 2 == 0)
                WriteIRQLatch(value);
            else if (address <= 0xDFFF && address % 2 == 1)
                WriteIRQReload(value);
            else if (address <= 0xFFFF && address % 2 == 0)
                WriteIRQDisable(value);
            else if (address <= 0xFFFF && address % 2 == 1)
                WriteIRQEnable(value);
        }

        private void WriteBankSelect(byte value)
        {
            programBankMode = (byte)((value >> 6) & 1);
            characterBankMode = (byte)((value >> 7) & 1);
            registerIndex = (byte)(value & 7);
            UpdateOffsets();

            // invalidate address regions
            CharacterBankSwitch?.Invoke(0x0000, 0x2000);
            ProgramBankSwitch?.Invoke(0x8000, 0x8000);
        }

        private void WriteBankData(byte value)
        {
            registers[registerIndex] = value;
            UpdateOffsets();
        }

        private void WriteMirror(byte value)
        {
            MirrorMode = ((value & 1) == 0) ? MirrorMode.Vertical : MirrorMode.Horizontal;
        }

        private void WriteProtect(byte value)
        {
        }

        private void WriteIRQLatch(byte value)
        {
            irqReload = value;
        }

        private void WriteIRQReload(byte value)
        {
            irqCounter = 0;
        }

        private void WriteIRQDisable(byte value)
        {
            irqEnable = false;
        }

        private void WriteIRQEnable(byte value)
        {
            irqEnable = true;
        }

        private int GetProgramBankOffset(int index)
        {
            if (index >= 0x80)
                index -= 0x100;

            index %= innerProgramBankLength / 0x2000;
            int offset = index * 0x2000;
            if (offset < 0)
                offset += innerProgramBankLength;

            return offset;
        }

        private int GetCharacterBankOffset(int index)
        {
            if (index >= 0x80)
                index -= 0x100;

            index %= innerCharacterBankLength / 0x0400;

            int offset = index * 0x0400;
            if (offset < 0)
                offset += innerCharacterBankLength;

            return offset;
        }

        private void UpdateOffsets()
        {
            if (programBankMode == 0)
            {
                programBankOffsets[0] = GetProgramBankOffset(registers[6]);
                programBankOffsets[1] = GetProgramBankOffset(registers[7]);
                programBankOffsets[2] = GetProgramBankOffset(-2);
                programBankOffsets[3] = GetProgramBankOffset(-1);
            }
            else // == 1
            {
                programBankOffsets[0] = GetProgramBankOffset(-2);
                programBankOffsets[1] = GetProgramBankOffset(registers[7]);
                programBankOffsets[2] = GetProgramBankOffset(registers[6]);
                programBankOffsets[3] = GetProgramBankOffset(-1);
            }

            if (characterBankMode == 0)
            {
                characterBankOffsets[0] = GetCharacterBankOffset(registers[0] & 0xFE);
                characterBankOffsets[1] = GetCharacterBankOffset(registers[0] | 0x01);
                characterBankOffsets[2] = GetCharacterBankOffset(registers[1] & 0xFE);
                characterBankOffsets[3] = GetCharacterBankOffset(registers[1] | 0x01);
                characterBankOffsets[4] = GetCharacterBankOffset(registers[2]);
                characterBankOffsets[5] = GetCharacterBankOffset(registers[3]);
                characterBankOffsets[6] = GetCharacterBankOffset(registers[4]);
                characterBankOffsets[7] = GetCharacterBankOffset(registers[5]);
            }
            else // == 1
            {
                characterBankOffsets[0] = GetCharacterBankOffset(registers[2]);
                characterBankOffsets[1] = GetCharacterBankOffset(registers[3]);
                characterBankOffsets[2] = GetCharacterBankOffset(registers[4]);
                characterBankOffsets[3] = GetCharacterBankOffset(registers[5]);
                characterBankOffsets[4] = GetCharacterBankOffset(registers[0] & 0xFE);
                characterBankOffsets[5] = GetCharacterBankOffset(registers[0] | 0x01);
                characterBankOffsets[6] = GetCharacterBankOffset(registers[1] & 0xFE);
                characterBankOffsets[7] = GetCharacterBankOffset(registers[1] | 0x01);
            }
        }

        private void SelectOuterBank(int outerBank)
        {
            outerBank %= 8;
            switch (outerBank)
            {
                case 0: case 1: case 2: innerProgramBankOffset = 0x00000; break;
                case 3: innerProgramBankOffset = 0x10000; break;
                case 4: case 5: case 6: innerProgramBankOffset = 0x20000; break;
                case 7: innerProgramBankOffset = 0x30000; break;
            }

            switch(outerBank)
            {
                case 4: case 5: case 6: innerProgramBankLength = 0x20000; break;
                default: innerProgramBankLength = 0x10000; break;
            }

            innerCharacterBankOffset = outerBank < 4 ? 0x00000 : 0x20000;

            innerCharacterBankLength = 0x20000;
        }

        private byte registerIndex;
        private byte[] registers;
        private byte programBankMode;
        private byte characterBankMode;
        private int[] programBankOffsets;
        private int[] characterBankOffsets;
        private byte irqReload;
        private byte irqCounter;
        private bool irqEnable;

        private int innerProgramBankOffset;
        private int innerProgramBankLength;
        private int innerCharacterBankOffset;
        private int innerCharacterBankLength;
    }
}
