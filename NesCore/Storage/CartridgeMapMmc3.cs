using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public class CartridgeMapMmc3 : CartridgeMap
    {
        public CartridgeMapMmc3(Cartridge cartridge)
        {
            Cartridge = cartridge;

            registers = new byte[8];
            programBankOffsets = new int[4];
            characterBankOffsets = new int[8];

            programBankOffsets[0] = GetProgramBankOffset(0);
            programBankOffsets[1] = GetProgramBankOffset(1);
            programBankOffsets[2] = GetProgramBankOffset(-2);
            programBankOffsets[3] = GetProgramBankOffset(-1);
        }

        public Cartridge Cartridge { get; private set; }

        public string Name { get { return "MMC3"; } }

        public Action TriggerInterruptRequest
        {
            get { return triggerInterruptRequest; }
            set { triggerInterruptRequest = value; }
        }

        public byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bank = address / 0x0400;
                    int offset = address % 0x0400;
                    return Cartridge.CharacterRom[characterBankOffsets[bank] + offset];
                }
                else if (address >= 0x8000)
                {
                    address -= 0x8000;
                    int bank = address / 0x2000;
                    int offset = address % 0x2000;
                    return Cartridge.ProgramRom[programBankOffsets[bank] + offset];
                }
                else if (address >= 0x6000)
                    return Cartridge.SaveRam[address - 0x6000];
                else
                    throw new Exception("Unhandled MMC3 mapper write at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x2000)
                {
                    int bank = address / 0x0400;
                    int offset = address % 0x0400;
                    Cartridge.CharacterRom[characterBankOffsets[bank] + offset] = value;
                }
                else if (address >= 0x8000)
                    WriteRegister(address, value);
                else if (address >= 0x6000)
                    Cartridge.SaveRam[address - 0x6000] = value;
                else
                    throw new Exception("Unhandled MMC3 mapper write at address: " + Hex.Format(address));
            }
        }

        public void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            if (cycle != 260)
                return;

            if (scanLine > 239 && scanLine < 261)
                return;

            if (!showBackground && !showSprites)
                return;

            HandleScanLine();
        }

        public void SaveState(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(registerIndex);
            binaryWriter.Write(registers);
            binaryWriter.Write(programBankMode);
            binaryWriter.Write(characterBankMode);
            for (int index = 0; index < 4; index++)
                binaryWriter.Write(programBankOffsets[index]);
            for (int index = 0; index < 8; index++)
                binaryWriter.Write(characterBankOffsets[index]);
            binaryWriter.Write(reload);
            binaryWriter.Write(counter);
            binaryWriter.Write(irqEnable);
        }

        public void LoadState(BinaryReader binaryReader)
        {
            registerIndex = binaryReader.ReadByte();
            registers = binaryReader.ReadBytes(8);
            programBankMode = binaryReader.ReadByte();
            characterBankMode = binaryReader.ReadByte();
            for (int index = 0; index < 4; index++)
                programBankOffsets[index] = binaryReader.ReadInt32();
            for (int index = 0; index < 8; index++)
                characterBankOffsets[index] = binaryReader.ReadInt32();
            reload = binaryReader.ReadByte();
            counter = binaryReader.ReadByte();
            irqEnable = binaryReader.ReadBoolean();
        }

        private void HandleScanLine()
        {
            if (counter == 0)
                counter = reload;
            else
            {
                --counter;
                if (counter == 0 && irqEnable)
                    triggerInterruptRequest?.Invoke();
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
        }

        private void WriteBankData(byte value)
        {
            registers[registerIndex] = value;
            UpdateOffsets();
        }

        private void WriteMirror(byte value)
        {
            Cartridge.MirrorMode = ((value & 1) == 0) ? Cartridge.MirrorVertical : Cartridge.MirrorHorizontal;
        }

        private void WriteProtect(byte value)
        {
        }

        private void WriteIRQLatch(byte value)
        {
            reload = value;
        }

        private void WriteIRQReload(byte value)
        {
            counter = 0;
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

            index %= Cartridge.ProgramRom.Count / 0x2000;
            int offset = index * 0x2000;
            if (offset < 0)
                offset += Cartridge.ProgramRom.Count;

            return offset;
        }

        private int GetCharacterBankOffset(int index)
        {
            if (index >= 0x80)
                index -= 0x100;

            index %= Cartridge.CharacterRom.Length / 0x0400;
            int offset = index * 0x0400;
            if (offset < 0)
                offset += Cartridge.CharacterRom.Length;

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

        private byte registerIndex;
        private byte[] registers;
        private byte programBankMode;
        private byte characterBankMode;
        private int[] programBankOffsets;
        private int[] characterBankOffsets;
        private byte reload;
        private byte counter;
        private bool irqEnable;

        private Action triggerInterruptRequest;
    }
}
