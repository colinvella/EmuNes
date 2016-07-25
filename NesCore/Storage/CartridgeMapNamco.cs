using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapNamco : CartridgeMap
    {
        public enum Variant
        {
            Namco_129 = 1,
            Namco_163 = 2,
            Namco_175 = 4,
            Namco_340 = 8
        }

        public CartridgeMapNamco(Cartridge cartridge, Variant variants)
            : base(cartridge)
        {
            this.variants = variants;

            // build name dynamically from variants
            // and determine variant characteristics
            isNamco129Or163 = false;
            isNamco129or163or175 = false;
            isNamco175 = false;
            isNamco340 = false;
            List<String> variantNames = new List<string>();
            foreach (Variant variant in Enum.GetValues(typeof(Variant)))
            {
                if ((variants & variant) != 0)
                {
                    variantNames.Add(variant.ToString().Replace("_", " "));
                    switch (variant)
                    {
                        case Variant.Namco_129:
                        case Variant.Namco_163:
                            isNamco129Or163 = true;
                            isNamco129or163or175 = true;
                            break;
                        case Variant.Namco_175:
                            isNamco129or163or175 = true;
                            isNamco175 = true;
                            break;
                        case Variant.Namco_340:
                            isNamco340 = true;
                            break;

                    }
                }
            }
            this.mapperName = string.Join(" / ", variantNames);

            programRam = new byte[0x2000];

            programRomBank = new int[4];

            characterBank = new int[8];

            nameTableBank = new int[4];
            
            ramWriteEnableSection = new bool[4];

            programBankCount = cartridge.ProgramRom.Count / 0x2000;
            characterBankCount = cartridge.CharacterRom.Count() / 0x400;

            programRomBank[3] = programBankCount - 1;

            for (int index = 0; index < 8; index++)
                characterBank[index] = index % characterBankCount;

            characterRam = new byte[0x4000];

            if (isNamco129Or163)
            {
                soundChip = new Namco163SoundChip();
            }
        }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bankIndex = address / 0x400;
                    int bankOffset = address % 0x400;
                    bool ramMode = characterBank[bankIndex] >= 0xE0;
                    if (address < 0x1000)
                        ramMode = ramMode && characterRamEnabledLow;
                    else
                        ramMode = ramMode && characterRamEnabledHigh;

                    int flatAddress = characterBank[bankIndex] * 0x400 + bankOffset;
                    if (ramMode)
                    {
                        flatAddress %= characterRam.Length;
                        return characterRam[flatAddress];
                    }
                    else
                    {
                        flatAddress %= Cartridge.CharacterRom.Length;
                        return Cartridge.CharacterRom[flatAddress];
                    }
                }
                else if (address >= 0x5000 && address < 0x5800)
                {
                    AcknowledgeInterrupt();
                    return (byte)(irqCounter & 0xff);
                }
                else if (address >= 0x5800 && address < 0x6000)
                {
                    AcknowledgeInterrupt();
                    return (byte)(((irqCounter >> 8) & 0x7f) | (irqEnabled ? 0x80 : 0));
                }
                else if (address >= 0x6000 && address < 0x8000)
                {
                    if (isNamco129or163or175)
                        return programRam[address - 0x6000];
                    else
                        return (byte)(address >> 8); // open bus
                }
                else if (address >= 0x8000)
                {
                    int bankIndex = (address - 0x8000) / 0x2000;
                    int bankOffset = address % 0x2000;
                    return Cartridge.ProgramRom[programRomBank[bankIndex] * 0x2000 + bankOffset];
                }
                else
                    return (byte)(address >> 8); // open bus for anything unspecified
            }

            set
            {
                if (address < 0x2000)
                {
                    int bankIndex = address / 0x400;
                    int bankOffset = address % 0x400;
                    bool ramMode = characterBank[bankIndex] >= 0xE0;
                    if (address < 0x1000)
                        ramMode = ramMode && characterRamEnabledLow;
                    else
                        ramMode = ramMode && characterRamEnabledHigh;

                    int flatAddress = characterBank[bankIndex] * 0x400 + bankOffset;
                    if (ramMode)
                    {
                        flatAddress %= characterRam.Length;
                        characterRam[flatAddress] = value;
                    }
                    else
                    {
                        flatAddress %= Cartridge.CharacterRom.Length;
                        Cartridge.CharacterRom[flatAddress] = value;
                    }
                }
                else if (address >= 0x4800 && address < 0x5000)
                {
                    if (isNamco129Or163)
                    {
                        // sound data port
                        soundChip.DataPort = value;
                        //Debug.WriteLine("Sound Data Port (" + Hex.Format(address) + ") = " + Hex.Format(value));
                    }
                }
                else if (address >= 0x5000 && address < 0x5800)
                {
                    if (isNamco129Or163)
                    {
                        // irq low 8 bits
                        irqCounter &= 0x7F00;
                        irqCounter |= value;

                        AcknowledgeInterrupt();

                        Debug.WriteLine("IRQ Counter High (" + Hex.Format(address) + ") = " + Hex.Format(value) + " (IRQ Counter = " + Hex.Format(irqCounter) + ")");
                    }
                }
                else if (address >= 0x5800 && address < 0x6000)
                {
                    if (isNamco129Or163)
                    {
                        // irq enable and high 7 bits
                        irqCounter &= 0x00FF;
                        irqCounter |= (ushort)((value & 0x7F) << 8);
                        irqEnabled = (value & 0x80) != 0;

                        AcknowledgeInterrupt();

                        Debug.WriteLine("IRQ Counter Low (" + Hex.Format(address) + ") = " + Hex.Format(value) + " (IRQ Counter = " + Hex.Format(irqCounter) + ")");
                    }
                }
                else if (address >= 0x6000 && address < 0x8000)
                {
                    // check if ram writes enabled at the general level
                    if (!ramWriteEnable)
                        return;

                    // check if ram writes enabled for the given 2K section
                    int ramOffset = address - 0x6000;
                    if (!ramWriteEnableSection[ramOffset / 0x800])
                        return;

                    programRam[address - 0x6000] = value;
                }
                else if (address >= 0x8000 && address < 0xC000)
                {
                    int bankIndex = (address - 0x8000) / 0x800;
                    characterBank[bankIndex] = value;
                }
                else if (address >= 0xC000 && address < 0xE000)
                {
                    if (isNamco129Or163)
                    {
                        int bankIndex = (address - 0xC000) / 0x800;
                        nameTableBank[bankIndex] = value;
                        //Debug.WriteLine("CHR RAM name table register set");
                    }
                    if (isNamco175 && address < 0xC800)
                    {
                        // Namco 175 has a single RAM enable flag (Namco 129, 163 ram sections ignored)
                        bool enableRam = (value & 0x01) != 0;
                        ramWriteEnable = enableRam;
                        for (int index = 0; index < 4; index++)
                            ramWriteEnableSection[index] = enableRam;
                    }
                }
                else if (address >= 0xE000 && address < 0xE800)
                {
                    // MMPP PPPP
                    // |||| ||||
                    // ||++-++++- Select 8KB page of PRG-ROM at $8000
                    // |+-------- Namco 129, 163 only: Disable sound if set
                    // ++-------- Namco 340 only: Select mirroring
                    programRomBank[0] = (value & 0x3F) % programBankCount;
                    if (isNamco129Or163)
                    {
                        soundChip.SoundEnable = (value & 0x40) == 0;
                    }

                    if (isNamco340)
                    {
                        // Namco 340 supports dynamic mirror mode change
                        switch (value >> 6)
                        {
                            case 0: MirrorMode = MirrorMode.Single0; break;
                            case 1: MirrorMode = MirrorMode.Vertical; break;
                            case 2: MirrorMode = MirrorMode.Horizontal; break;
                            case 3: MirrorMode = MirrorMode.Single1; break;
                        }
                    }

                    Debug.WriteLine("Program bank $8000 - $9FFF (" + Hex.Format(address) + ") = " + Hex.Format((byte)programRomBank[0]));
                }
                else if (address >= 0xE800 && address < 0xF000)
                {
                    // HLPP PPPP
                    // |||| ||||
                    // ||++-++++-Select 8KB page of PRG-ROM at $A000
                    // |+--------Disable CHR - RAM at $0000 -$0FFF(Namco 129, 163 only)
                    // |           0: Pages $E0 -$FF use NT RAM as CHR - RAM
                    // |           1: Pages $E0 -$FF are the last $20 banks of CHR - ROM
                    // +---------Disable CHR - RAM at $1000 -$1FFF(Namco 129, 163 only)
                    //             0: Pages $E0 -$FF use NT RAM as CHR - RAM
                    //             1: Pages $E0 -$FF are the last $20 banks of CHR - ROM
                    programRomBank[1] = (value & 0x3F) % programBankCount;
                    characterRamEnabledLow = (value & 0x40) != 0;
                    characterRamEnabledHigh = (value & 0x80) != 0;

                    Debug.WriteLine("Program bank $A000 - $BFFF (" + Hex.Format(address) + ") = " + Hex.Format((byte)programRomBank[1]));
                }
                else if (address >= 0xF000 && address < 0xF800)
                {
                    // ..PP PPPP
                    //   || ||||
                    //   ++-++++- Select 8KB page of PRG-ROM at $C000
                    programRomBank[2] = (value & 0x3F) % programBankCount;

                    Debug.WriteLine("Program bank $C000 - $DFFF (" + Hex.Format(address) + ") = " + Hex.Format((byte)programRomBank[2]));
                }
                else if (address >= 0xF800)
                {
                    if (isNamco129Or163)
                    {
                        // KKKK DCBA
                        // |||| ||||
                        // |||| ||| +- 1: Write - protect 2kB window of external RAM from $6000 -$67FF(0: write enable)
                        // |||| || +-- 1: Write - protect 2kB window of external RAM from $6800 -$6FFF(0: write enable)
                        // |||| | +--- 1: Write - protect 2kB window of external RAM from $7000 -$77FF(0: write enable)
                        // |||| +----- 1: Write - protect 2kB window of external RAM from $7800 -$7FFF(0: write enable)
                        // ++++------- Additionally the upper nybble must be equal to b0100 to enable writes
                        ramWriteEnable = (value >> 4) == 0x04;
                        ramWriteEnableSection[0] = (value & 0x01) != 0;
                        ramWriteEnableSection[1] = (value & 0x02) != 0;
                        ramWriteEnableSection[2] = (value & 0x04) != 0;
                        ramWriteEnableSection[3] = (value & 0x08) != 0;

                        // also Audio Chip address port (auto increment and 7bit internal address)
                        soundChip.AddressPort = value;
                    }
                }
            }
        }

        public override string Name { get { return mapperName; } }

        public override byte ReadNameTableByte(ushort address)
        {
            if (isNamco129Or163)
            {
                int nameTableIndex = (address - 0x2000) / 0x400;
                int bankOffset = address % 0x400;
                int selectedBank = nameTableBank[nameTableIndex];
                if (selectedBank < 0xE0)
                    return Cartridge.CharacterRom[selectedBank * 0x400 + bankOffset];
                else
                    return base.ReadNameTableByte((ushort)((selectedBank % 2) * 0x400 + bankOffset));
            }
            else
                return base.ReadNameTableByte(address);
        }

        public override void WriteNameTableByte(ushort address, byte value)
        {
            if (isNamco129Or163)
            {
                int nameTableIndex = (address - 0x2000) / 0x400;
                int bankOffset = address % 0x400;
                int selectedBank = nameTableBank[nameTableIndex];
                if (selectedBank < 0xE0)
                    Cartridge.CharacterRom[selectedBank * 0x400 + bankOffset] = value; // should allow or not?
                else
                    base.WriteNameTableByte((ushort)((selectedBank % 2) * 0x400 + bankOffset), value);
            }
            else
                base.WriteNameTableByte(address, value);
        }

        public override void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            cpuClock++;
            cpuClock %= 3;

            if (cpuClock != 0)
                return;

            if (irqCounter < 0x7FFF)
                ++irqCounter;
            else if (irqEnabled)
            {
                if (!irqTriggered)
                {
                    TriggerInterruptRequest?.Invoke();
                    irqTriggered = true;
                }
            }

            if (isNamco129Or163)
                soundChip.Update(1);
            /*
            int output = soundChip.Output;
            Debug.WriteLineIf(output != 0, "Sound Output = " + output);
            WriteAudioSample(output * 0.1f);
            */
        }

        private void AcknowledgeInterrupt()
        {
            if (irqTriggered)
            {
                CancelInterruptRequest?.Invoke();
                irqTriggered = false;
            }
        }

        private Variant variants;
        private bool isNamco129Or163;
        private bool isNamco175;
        private bool isNamco129or163or175;
        private bool isNamco340;
        private string mapperName;

        private byte[] programRam;
        private bool ramWriteEnable;
        private bool[] ramWriteEnableSection;

        private int programBankCount;
        private int[] programRomBank;

        private int characterBankCount;
        private int[] characterBank;
        private bool characterRamEnabledLow;
        private bool characterRamEnabledHigh;
        private byte[] characterRam;

        private bool irqEnabled;
        private ushort irqCounter;
        private bool irqTriggered;
        private byte cpuClock;

        private int[] nameTableBank;

        private Namco163SoundChip soundChip;


    }

}
