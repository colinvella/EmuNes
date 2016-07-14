using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapJalecoSs88006 : CartridgeMap
    {
        public CartridgeMapJalecoSs88006(Cartridge cartridge)
            : base(cartridge)
        {
            programRomBank = new int[4];
            characterRomBank = new int[8];

            int programBankCount = cartridge.ProgramRom.Count / 0x2000;

            programRomBank[0] = 0;
            programRomBank[1] = 1;
            programRomBank[2] = programBankCount - 2;
            programRomBank[3] = programBankCount - 1;
        }

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
                else if (address >= 0x8000)
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

                    int oldProgramBank = programRomBank[programBankIndex];

                    if (offset1000 % 2 == 0)
                        programRomBank[programBankIndex] = SetLowerNybble(programRomBank[programBankIndex], value);
                    else
                        programRomBank[programBankIndex] = SetHigherNybble(programRomBank[programBankIndex], value);


                    if (programRomBank[programBankIndex] != oldProgramBank)
                    {
                        Debug.WriteLine("Program Bank " + programBankIndex + " (" + Hex.Format(address) + ") = " + programRomBank[programBankIndex]);
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

                    int oldCharacterBank = characterRomBank[characterBankIndex];

                    if (offset1000 % 2 == 0)
                        characterRomBank[characterBankIndex] = SetLowerNybble(characterRomBank[characterBankIndex], value);
                    else
                        characterRomBank[characterBankIndex] = SetHigherNybble(characterRomBank[characterBankIndex], value);

                    if (characterRomBank[characterBankIndex] != oldCharacterBank)
                    {
                        Debug.WriteLine("Character Bank " + characterBankIndex + " (" + Hex.Format(address) + ") = " + characterRomBank[characterBankIndex]);
                        CharacterBankSwitch?.Invoke((ushort)(characterBankIndex * 0x400), 0x400);
                    }
                }
                else if (address == 0xE000)
                {
                    // bits 0..3 of IRQ counter reload
                    irqReload &= 0xFFF0;
                    irqReload |= (ushort)(value & 0x0F);
                }
                else if (address == 0xE001)
                {
                    // bits 4..7 of IRQ counter reload
                    irqReload &= 0xFF0F;
                    irqReload |= (ushort)((value & 0x0F) << 4);
                }
                else if (address == 0xE002)
                {
                    // bits 8..11 of IRQ counter reload
                    irqReload &= 0xF0FF;
                    irqReload |= (ushort)((value & 0x0F) << 8);
                }
                else if (address == 0xE003)
                {
                    // bits 12..15 of IRQ counter reload
                    irqReload &= 0x0FFF;
                    irqReload |= (ushort)((value & 0x0F) << 12);
                }
                else if (address == 0xF000)
                {
                    irqCounter = irqReload;
                    Debug.WriteLine("IRQ Reloaded to " + irqReload);
                    CancelInterruptRequest?.Invoke();
                    irqPrimed = false;
                }
                else if (address == 0xF001)
                {
                    irqCounterEnabled = (value & 0x01) != 0;

                    int counterBits = (value >> 1) & 0x07;

                    switch (counterBits)
                    {
                        case 0: irqFixedMask = 0x0000; break;
                        case 1: irqFixedMask = 0xF000; break;
                        case 2:
                        case 3: irqFixedMask = 0xFF00; break;
                        default: irqFixedMask = 0xFFF0; break;
                    }
                    irqCounterMask = 0xFFFF - irqFixedMask; // complement

                    CancelInterruptRequest?.Invoke();
                    irqPrimed = false;
                }
                else if (address == 0xF002)
                {
                    // mirror mode
                    switch (value & 0x03)
                    {
                        case 0x00: MirrorMode = MirrorMode.Horizontal; break;
                        case 0x01: MirrorMode = MirrorMode.Vertical; break;
                        case 0x02: MirrorMode = MirrorMode.Single0; break;
                        case 0x03: MirrorMode = MirrorMode.Single1; break;
                    }
                }
            }
        }

        public override string Name { get { return "Jaleco SS88006"; } }

        public override void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            cpuClock++;
            cpuClock %= 3;

            // using priming variable synchronises IRQ timing and prevents jitter
            if (irqPrimed && cycle == 0)
            {
                TriggerInterruptRequest?.Invoke();
                irqPrimed = false;
            }

            if (!irqCounterEnabled || cpuClock != 0)
                return;

            ushort fixedBits = (ushort)(irqCounter & irqFixedMask);

            ushort counterbits = (ushort)(irqCounter & irqCounterMask);

            if (counterbits == 0)
                irqPrimed = true;

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

        private int[] programRomBank;
        private int[] characterRomBank;
        private ushort cpuClock;
        private bool irqCounterEnabled;
        private ushort irqReload;
        private int irqFixedMask;
        private int irqCounterMask;
        private ushort irqCounter;
        private bool irqPrimed;
    }
}
