using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapNamco163 : CartridgeMap
    {
        public CartridgeMapNamco163(Cartridge cartridge)
            : base(cartridge)
        {
            programRam = new byte[0x2000];

            programRomBank = new int[4];

            characterBank = new int[8];
            
            ramWriteEnableSection = new bool[4];

            programBankCount = cartridge.ProgramRom.Count / 0x2000;
            characterBankCount = cartridge.CharacterRom.Count() / 0x400;

            programRomBank[3] = programBankCount - 1;

            for (int index = 0; index < 8; index++)
                characterBank[index] = index % characterBankCount;

            characterRam = new byte[0x4000];
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
                    return programRam[address - 0x6000];
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
                    // sound data port
                    Debug.WriteLine("Sound Data Port (" + Hex.Format(address) + ") = " + Hex.Format(value));
                }
                else if (address >= 0x5000 && address < 0x5800)
                {
                    // irq low 8 bits
                    irqCounter &= 0x7F00;
                    irqCounter |= value;

                    AcknowledgeInterrupt();

                    Debug.WriteLine("IRQ Counter High (" + Hex.Format(address) + ") = " + Hex.Format(value) + " (IRQ Counter = " + Hex.Format(irqCounter) + ")");
                }
                else if (address >= 0x5800 && address < 0x6000)
                {
                    // irq enable and high 7 bits
                    irqCounter &= 0x00FF;
                    irqCounter |= (ushort)((value & 0x7F) << 8);
                    irqEnabled = (value & 0x80) != 0;

                    AcknowledgeInterrupt();

                    Debug.WriteLine("IRQ Counter Low (" + Hex.Format(address) + ") = " + Hex.Format(value) + " (IRQ Counter = " + Hex.Format(irqCounter) + ")");
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
                    int bankIndex = (address - 0xC000) / 0x800;
                    characterBank[bankIndex] = value;
                    Debug.WriteLine("CHR RAM name table register set");
                }
                else if (address >= 0xE000 && address < 0xE800)
                {
                    // MMPP PPPP
                    // |||| ||||
                    // ||++-++++- Select 8KB page of PRG-ROM at $8000
                    // |+-------- Namco 129, 163 only: Disable sound if set
                    // ++-------- Namco 340 only: Select mirroring
                    programRomBank[0] = (value & 0x3F) % programBankCount;
                    soundEnabled = (value & 0x40) == 0;

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
                }
            }
        }

        public override string Name { get { return "Namco 163"; } }

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
        }

        private void AcknowledgeInterrupt()
        {
            if (irqTriggered)
            {
                CancelInterruptRequest?.Invoke();
                irqTriggered = false;
            }
        }

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

        private bool soundEnabled;


    }

}
