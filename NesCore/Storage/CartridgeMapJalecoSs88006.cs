using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapJalecoSs88006 : CartridgeMap
    {
        public CartridgeMapJalecoSs88006(Cartridge cartridge)
        {
            this.Cartridge = cartridge;

            programRomBankLatch = new int[4];
            programRomBank = new int[4];

            characterRomBankLatch = new int[8];
            characterRomBank = new int[8];

            programRomBank[0] = programRomBank[1] = programRomBank[2] = 0x000;
            programRomBank[3] = (cartridge.ProgramRom.Count / 0x2000) - 1;

            this.mirrorMode = cartridge.MirrorMode;
        }

        public Cartridge Cartridge { get; private set; }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bank = address / 0x400;
                    int offset = address % 0x400;
                    return Cartridge.CharacterRom[characterRomBank[bank] * 0x400 + offset];
                }
                else if (address > 0x8000)
                {
                    int bank = (address - 0x8000) / 0x2000;
                    int offset = address % 0x2000;
                    return Cartridge.ProgramRom[programRomBank[bank] * 0x2000 + offset];
                }
                else
                    return (byte)(address >> 8); // assuming open bus
            }

            set
            {
                if (address >= 0x8000 && address < 0xA000)
                {
                    int offset1000 = address % 0x1000;
                    if (offset1000 >= 4)
                        return;

                    int bankIndex1000 = (address - 0x8000) / 0x1000;

                    int programBankIndex = bankIndex1000 * 2 + offset1000 / 2;

                    // last bank must remain fixed
                    if (programBankIndex > 2)
                        return;


                    if (offset1000 % 2 == 0)
                        programRomBankLatch[programBankIndex] = value & 0x0F;
                    else
                    {
                        int oldProgramBank = programRomBank[programBankIndex];

                        programRomBank[programBankIndex] = SetHigherNybble(programRomBankLatch[programBankIndex], value);

                        if (programRomBank[programBankIndex] != oldProgramBank)
                            ProgramBankSwitch?.Invoke((ushort)(0x8000 + programBankIndex * 0x2000), 0x2000);
                    }
                }
                else if (address >= 0xA000 && address < 0xE000)
                {
                    int offset1000 = address % 0x1000;
                    if (offset1000 >= 4)
                        return;
                    int bankIndex1000 = (address - 0xA000) / 0x1000;

                    int characterBankIndex = bankIndex1000 * 2 + offset1000 / 2;

                    if (offset1000 % 2 == 0)
                        characterRomBankLatch[characterBankIndex] = value & 0x0F;
                    else
                    {
                        int oldCharacterBank = characterRomBank[characterBankIndex];

                        characterRomBank[characterBankIndex] = SetHigherNybble(characterRomBankLatch[characterBankIndex], value);

                        if (characterRomBank[characterBankIndex] != oldCharacterBank)
                            CharacterBankSwitch?.Invoke((ushort)(characterBankIndex * 0x400), 0x400);
                    }
                }
                else if (address == 0xE000)
                {
                    // bits 0..3 of IRQ counter reload
                    irqReload &= 0xFFF0;
                    irqReload |= (byte)(value & 0x0F);
                }
                else if (address == 0xE001)
                {
                    // bits 4..7 of IRQ counter reload
                    irqReload &= 0xFF0F;
                    irqReload |= (byte)((value & 0x0F) << 4);
                }
                else if (address == 0xE002)
                {
                    // bits 8..11 of IRQ counter reload
                    irqReload &= 0xF0FF;
                    irqReload |= (byte)((value & 0x0F) << 8);
                }
                else if (address == 0xE003)
                {
                    // bits 12..15 of IRQ counter reload
                    irqReload &= 0x0FFF;
                    irqReload |= (byte)((value & 0x0F) << 12);
                }
                else if (address == 0xF000)
                {
                    irqCounter = irqReload;
                    CancelInterruptRequest?.Invoke();
                }
                else if (address == 0xF001)
                {
                    irqCounterEnabled = (value & 0x01) != 0;

                    int counterBits = (value >> 1) & 0x07;

                    switch (value)
                    {
                        case 0: irqFixedMask = 0x0000; break;
                        case 1: irqFixedMask = 0xF000; break;
                        case 2:
                        case 3: irqFixedMask = 0xFF00; break;
                        default: irqFixedMask = 0xFFF0; break;
                    }
                    irqCounterMask = 0xFFFF - irqFixedMask; // complement

                    CancelInterruptRequest?.Invoke();
                }
                else if (address == 0xF002)
                {
                    MirrorMode newMirrorMode = mirrorMode;
                    switch (value & 0x03)
                    {
                        case 0x00: newMirrorMode = MirrorMode.Horizontal; break;
                        case 0x01: newMirrorMode = MirrorMode.Vertical; break;
                        case 0x02: newMirrorMode = MirrorMode.Single0; break;
                        case 0x03: newMirrorMode = MirrorMode.Single1; break;
                    }
                    if (newMirrorMode != mirrorMode)
                    {
                        mirrorMode = newMirrorMode;
                        MirrorModeChanged?.Invoke(mirrorMode);
                    }
                }
            }
        }

        public override string Name { get { return "Jaleco SS88006"; } }

        public override void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            cpuClock++;
            cpuClock %= 3;

            if (!irqCounterEnabled || cpuClock != 0)
                return;

            ushort fixedBits = (ushort)(irqCounter & irqFixedMask);

            ushort counterbits = (ushort)(irqCounter & irqCounterMask);

            if (counterbits == 0)
                TriggerInterruptRequest?.Invoke();

            --counterbits;

            irqCounter = (ushort)(fixedBits | (counterbits & irqCounterMask));

        }
        private int SetLowerNybble(int currentValue, byte nybble)
        {
            currentValue &= 0xF0;
            currentValue |= (nybble & 0x0F);
            return currentValue;
        }

        private int SetHigherNybble(int currentValue, byte nybble)
        {
            currentValue &= 0x0F;
            currentValue |= (nybble & 0x0F) << 4;
            return currentValue;
        }

        private int[] programRomBankLatch;
        private int[] programRomBank;
        private int[] characterRomBankLatch;
        private int[] characterRomBank;
        private ushort cpuClock;
        private bool irqCounterEnabled;
        private ushort irqReload;
        private int irqFixedMask;
        private int irqCounterMask;
        private ushort irqCounter;
        private MirrorMode mirrorMode;
    }
}
