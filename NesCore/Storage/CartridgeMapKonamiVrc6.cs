using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapKonamiVrc6 : CartridgeMapKonamiVrc
    {
        public enum Variant
        {
            Vrc6a,
            Vrc6b
        }

        public CartridgeMapKonamiVrc6(Cartridge cartridge, Variant variant) : base(cartridge)
        {
            this.variant = variant;
            if (variant == Variant.Vrc6a)
                mapperName = "VRC6 Rev A";
            else
                mapperName = "VRC6 Rev B";

            programBankCount16K = cartridge.ProgramRom.Count / 0x4000;
            programBankCount8K = programBankCount16K * 2;
            programBank8kLastAddress = cartridge.ProgramRom.Count - 0x2000;

            characterBankCount = cartridge.CharacterRom.Length / 0x400;
            characterBank = new int[8];

            nameTableBankIndex = new int[4];
            nameTableBankIndex[0] = 6;
            nameTableBankIndex[1] = 6;
            nameTableBankIndex[2] = 7;
            nameTableBankIndex[3] = 7;
        }

        public override string Name { get { return mapperName; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bankIndex = address / 0x400;
                    if (characterBankMode == 1)
                        bankIndex /= 2;
                    else if (characterBankMode >= 2)
                    {
                        if (bankIndex >= 4)
                        {
                            bankIndex -= 4;
                            bankIndex /= 2;
                            bankIndex += 4;
                        }
                    }

                    int bankOffset = address % 0x400;

                    return Cartridge.CharacterRom[characterBank[bankIndex] * 0x400 + bankOffset];
                }
                else if (address >= 0x6000 && address < 0x8000)
                {
                    if (programRamEnabled)
                        return Cartridge.SaveRam[(ushort)(address % 0x2000)];
                    else
                        return (byte)(address >> 8); // if no ram, open bus?
                }
                else if (address >= 0x8000 && address < 0xC000)
                {
                    int bankOffset = address % 0x4000;
                    return Cartridge.ProgramRom[programBank16k * 0x4000 + bankOffset];
                }
                else if (address >= 0xC000 && address < 0xE000)
                {
                    int bankOffset = address % 0x2000;
                    return Cartridge.ProgramRom[programBank8k * 0x2000 + bankOffset];
                }
                else if (address >= 0xE000)
                {
                    int bankOffset = address % 0x2000;
                    return Cartridge.ProgramRom[programBank8kLastAddress + bankOffset];
                }
                else
                {
                    Debug.WriteLine("Open bus read at address " + Hex.Format(address));
                    return (byte)(address >> 8); // open bus
                }
            }

            set
            {
                Debug.WriteLine(variant + ": [" + Hex.Format(address) + "] = " + Hex.Format(value));

                byte addressHighNybble = (byte)(address >> 12);
                byte addressLowBits = (byte)(address & 0x03);
                if (variant == Variant.Vrc6b)
                {
                    // for Rev B, swap around A0 and A1
                    if (addressLowBits == 1)
                        addressLowBits = 2;
                    else if (addressLowBits == 2)
                        addressLowBits = 1;
                }

                if (address >= 0x6000 && address < 0x8000)
                {
                    if (programRamEnabled)
                        Cartridge.SaveRam[(ushort)(address % 0x2000)] = value;
                }
                else if (addressHighNybble == 0x8)
                {
                    programBank16k = value & 0x0F;
                    programBank16k %= programBankCount16K; // paranoia
                }
                else if (addressHighNybble == 0x9)
                {
                    // sound - pulse 1
                }
                else if (addressHighNybble == 0xA)
                {
                    // sound - pulse 2
                }
                else if (address >= 0xB000 && address < 0xB003)
                {
                    // sound - triangle
                }
                else if (address == 0xB003)
                {
                    // controls
                    programRamEnabled = (value & 0x80) != 0;
                    nameTableSource = (NameTableSource)((value >> 4) % 0x01);
                    characterBankMode = value & 0x03;
                    characterBankPassThrough = (value & 0x20) != 0;

                    switch ((value >> 2) & 0x03)
                    {
                        case 0: MirrorMode = MirrorMode.Vertical; break;
                        case 1: MirrorMode = MirrorMode.Horizontal; break;
                        case 2: MirrorMode = MirrorMode.Single0; break;
                        case 3: MirrorMode = MirrorMode.Single1; break;
                    }

                    switch (value & 0x07)
                    {
                        case 0:
                        case 6:
                        case 7:
                            nameTableBankIndex[0] = 6;
                            nameTableBankIndex[1] = 6;
                            nameTableBankIndex[2] = 7;
                            nameTableBankIndex[3] = 7;
                            break;
                        case 1:
                        case 5:
                            nameTableBankIndex[0] = 4;
                            nameTableBankIndex[1] = 5;
                            nameTableBankIndex[2] = 6;
                            nameTableBankIndex[3] = 7;
                            break;
                        case 2:
                        case 3:
                        case 4:
                            nameTableBankIndex[0] = 6;
                            nameTableBankIndex[1] = 7;
                            nameTableBankIndex[2] = 6;
                            nameTableBankIndex[3] = 7;
                            break;
                    }
                }
                else if (addressHighNybble == 0xC)
                {
                    programBank8k = value & 0x1F;
                    programBank8k %= programBankCount8K; // paranoia
                }
                else if (addressHighNybble == 0xD || addressHighNybble == 0xE)
                {
                    int bankIndex = (addressHighNybble - 0xD) * 4;
                    bankIndex += addressLowBits;
                    characterBank[bankIndex] = value % characterBankCount;
                }
                else if (addressHighNybble == 0xF)
                {
                    switch (addressLowBits)
                    {
                        case 0:
                            WriteIrqReloadValue(value);
                            break;
                        case 1:
                            WriteIrqControl(value);
                            break;
                        case 2:
                            WriteIrqAcknowledge();
                            break;
                    }
                }
            }
        }

        public override byte ReadNameTableByte(ushort address)
        {
            if (nameTableSource == NameTableSource.CiRam)
                return base.ReadNameTableByte(address);

            int bankIndex = (address % 0x1000) / 0x400;
            int characterBankIndex = nameTableBankIndex[bankIndex];

            int bankOffset = address % 0x400;

            return Cartridge.CharacterRom[characterBank[characterBankIndex] * 0x400 + bankOffset];
        }

        public override void WriteNameTableByte(ushort address, byte value)
        {
            if (nameTableSource == NameTableSource.CiRam)
                base.WriteNameTableByte(address, value);
        }

        private Variant variant;
        private string mapperName;

        private int programBankCount16K;
        private int programBankCount8K;
        private int programBank8kLastAddress;
        private int programBank16k;
        private int programBank8k;

        private int characterBankMode;
        private int characterBankCount;
        private int[] characterBank;
        private bool characterBankPassThrough;

        private NameTableSource nameTableSource;
        private int[] nameTableBankIndex;

        private bool programRamEnabled;

        private enum NameTableSource
        {
            CiRam,
            ChrRom
        }
    }
}
