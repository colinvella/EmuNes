using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapKonamiVrc4 : CartridgeMap
    {
        public enum Variant
        {
            Vrc4RevAorC,
            Vrc4RevBorD,
            Vrc4RevEorF
        }

        public CartridgeMapKonamiVrc4(Cartridge cartridge, Variant variant)
            : base(cartridge)
        {
            characterBankRegisterLowAddresses = new ushort[8][];
            characterBankRegisterHighAddresses = new ushort[8][];

            characterBank = new int[8];

            // IRQ reload address is common to all variants
            irqReloadLowAddress = 0xF000;

            switch (variant)
            {
                case Variant.Vrc4RevAorC:
                    mapperName = "Konami VRC4a/VRC4c";

                    programBank0RegisterAddresses = new ushort[] { 0x8000, 0x8002, 0x8004, 0x8006, 0x8040, 0x8080, 0x80C0 };

                    mirroringRegisterAddresses = new ushort[] { 0x9000, 0x9002, 0x9040 };
                    programModeRegisterAddresses = new ushort[] { 0x9004, 0x9006, 0x90C0 };

                    // define CHR regs 0 and 1 - the rest can be computed
                    characterBankRegisterLowAddresses[0] = new ushort[] { 0xB000 };
                    characterBankRegisterHighAddresses[0] = new ushort[] { 0xB002, 0xB040 };
                    characterBankRegisterLowAddresses[1] = new ushort[] { 0xB004, 0x8080 };
                    characterBankRegisterHighAddresses[1] = new ushort[] { 0xB006, 0xB0C0 };

                    irqReloadHighAddresses = new ushort[] { 0xF002, 0xF040 };
                    irqControlAddresses = new ushort[] { 0xF004, 0xF080 };
                    irqAcknowledgeAddresses = new ushort[] { 0xF006, 0xF0C0 };

                    break;
                case Variant.Vrc4RevBorD:
                    mapperName = "Konami VRC4b/VRC4d";
                    break;
                case Variant.Vrc4RevEorF:
                    mapperName = "Konami VRC4e/VRC4f";
                    break;
            }

            // PRG bank 1 registers can be computed from bank 0 registers
            programBank1RegisterAddresses = programBank0RegisterAddresses.Select((a) => (ushort)(a + 0x2000)).ToArray();

            // compute even CHR low and high register addresses 2, 4, 6 from 0
            for (int index = 2; index < 8; index += 2)
            {
                characterBankRegisterLowAddresses[index]
                    = characterBankRegisterLowAddresses[0].Select((a) => (ushort)(a + 0x1000 * (index / 2))).ToArray();

                characterBankRegisterHighAddresses[index]
                    = characterBankRegisterHighAddresses[0].Select((a) => (ushort)(a + 0x1000 * (index / 2))).ToArray();
            }

            // compute odd CHR low and high register addresses 3, 5, 7 from 1
            for (int index = 3; index < 8; index += 2)
            {
                characterBankRegisterLowAddresses[index]
                    = characterBankRegisterLowAddresses[1].Select((a) => (ushort)(a + 0x1000 * (index / 2))).ToArray();

                characterBankRegisterHighAddresses[index]
                    = characterBankRegisterHighAddresses[1].Select((a) => (ushort)(a + 0x1000 * (index / 2))).ToArray();
            }

            programBankCount = cartridge.ProgramRom.Count / 0x2000;
            programBankLastAddress = (programBankCount - 1) * 0x2000;
            programBankNextToLastAddress = programBankLastAddress - 0x2000;

            characterBankCount = cartridge.CharacterRom.Length / 0x400;
        }

        public override string Name { get { return mapperName; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bankIndex = address / 0x400;
                    int bankOffset = address % 0x400;
                    return Cartridge.CharacterRom[characterBank[bankIndex] * 0x400 + bankOffset];
                }
                else if (address >= 0x8000 && address < 0xA000)
                {
                    int bankOffset = address % 0x2000;
                    if (programMode == ProgramMode.Mode0)
                        return Cartridge.ProgramRom[programBank0 * 0x2000 + bankOffset];
                    else
                        return Cartridge.ProgramRom[programBankNextToLastAddress + bankOffset];
                }
                else if (address >= 0xA000 && address < 0xC000)
                {
                    int bankOffset = address % 0x2000;
                    return Cartridge.ProgramRom[programBank1 * 0x2000 + bankOffset];
                }
                else if (address >= 0xC000 && address < 0xE000)
                {
                    int bankOffset = address % 0x2000;
                    if (programMode == ProgramMode.Mode0)
                        return Cartridge.ProgramRom[programBankNextToLastAddress + bankOffset];
                    else
                        return Cartridge.ProgramRom[programBank0 * 0x2000 + bankOffset];
                }
                else if (address >= 0xE000)
                {
                    int bankOffset = address % 0x2000;
                    return Cartridge.ProgramRom[programBankLastAddress + bankOffset];
                }
                else
                    return (byte)(address >> 8); // open buss
            }

            set
            {
                if (programModeRegisterAddresses.Contains(address))
                {
                    programMode = (ProgramMode)((value >> 1) & 0x01);
                }
                else if (programBank0RegisterAddresses.Contains(address))
                {
                    programBank0 = value % programBankCount;
                }
                else if (programBank1RegisterAddresses.Contains(address))
                {
                    programBank1 = value % programBankCount;
                }
                else if (mirroringRegisterAddresses.Contains(address))
                {
                    switch (value % 0x03)
                    {
                        case 0: MirrorMode = MirrorMode.Vertical; break;
                        case 1: MirrorMode = MirrorMode.Horizontal; break;
                        case 2: MirrorMode = MirrorMode.Single0; break;
                        case 3: MirrorMode = MirrorMode.Single1; break;
                    }
                }
                else if (address >= 0xB000 && address < 0xF000)
                {
                    //Debug.WriteLine("CHR Register " + Hex.Format(address) + " = " + Hex.Format(value));
                    for (int characterBankIndex = 0; characterBankIndex < 8; characterBankIndex++)
                    {
                        if (characterBankRegisterLowAddresses[characterBankIndex].Contains(address))
                        {
                            characterBank[characterBankIndex] &= 0x1F0;
                            characterBank[characterBankIndex] |= (value & 0x0F);
                            characterBank[characterBankIndex] %= characterBankCount;
                            //Debug.WriteLine("CHR bank [" + characterBankIndex + "] (" + Hex.Format((ushort)(characterBankIndex * 0x400)) + ") = " + Hex.Format((ushort)characterBank[characterBankIndex]));
                            break;
                        }
                        else if (characterBankRegisterHighAddresses[characterBankIndex].Contains(address))
                        {
                            characterBank[characterBankIndex] &= 0x00F;
                            characterBank[characterBankIndex] |= ((value & 0x1F) << 4);
                            characterBank[characterBankIndex] %= characterBankCount;
                            //Debug.WriteLine("CHR bank [" + characterBankIndex + "] (" + Hex.Format((ushort)(characterBankIndex * 0x400)) + ") = " + Hex.Format((ushort)characterBank[characterBankIndex]));
                            break;
                        }
                    }
                }
                else if (address == irqReloadLowAddress)
                {
                    irqReloadValue &= 0xF0;
                    irqReloadValue |= (byte)(value & 0x0F);
                }
                else if (irqReloadHighAddresses.Contains(address))
                {
                    irqReloadValue &= 0x0F;
                    irqReloadValue |= (byte)((value & 0x0F) << 4);
                }
                else if (irqControlAddresses.Contains(address))
                {
                    irqCountMode = (IrqCountMode)((value >> 2) & 0x01);
                    irqEnable = (value & 0x02) != 0;
                    irqEnableOnAcknowledge = (value & 0x01) != 0;
                    irqTriggered = false;
                    if (irqEnable)
                    {
                        irqPrescaler = 341;
                        irqCounter = irqReloadValue;
                    }
                    Debug.WriteLine("IRQ Count Mode = " + irqCountMode);
                }
                else if (irqAcknowledgeAddresses.Contains(address))
                {
                    if (irqTriggered)
                        CancelInterruptRequest?.Invoke();
                    irqEnable = irqEnableOnAcknowledge;
                    irqTriggered = false;
                }
                else
                {
                    Debug.WriteLine("VRC4 unknown write at " + Hex.Format(address) + " with value " + Hex.Format(value));
                }
            }
        }

        public override void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            cpuClock++;
            cpuClock %= 3;

            if (cpuClock != 0)
                return;

            if (!irqEnable)
                return;

            if (irqCountMode == IrqCountMode.Scanline)
            {
                irqPrescaler -= 3;
                if (irqPrescaler <= 0)
                {
                    UpdateIrqCounter();
                    irqPrescaler += 341;
                }
            }
            else
            {
                UpdateIrqCounter();
            }

        }

        private void UpdateIrqCounter()
        {
            if (irqCounter == 0xFF)
            {
                irqCounter = irqReloadValue;
                Debug.WriteLine("IRQ counter reloaded to " + irqReloadValue);
                if (!irqTriggered)
                {
                    TriggerInterruptRequest?.Invoke();
                    irqTriggered = true;
                }
            }
            else
                ++irqCounter;
        }

        private string mapperName;

        private ushort[] programModeRegisterAddresses;
        private ushort[] programBank0RegisterAddresses;
        private ushort[] programBank1RegisterAddresses;
        private ushort[] mirroringRegisterAddresses;

        private ushort[][] characterBankRegisterLowAddresses;
        private ushort[][] characterBankRegisterHighAddresses;

        private ushort irqReloadLowAddress;
        private ushort[] irqReloadHighAddresses;
        private ushort[] irqControlAddresses;
        private ushort[] irqAcknowledgeAddresses;

        private ProgramMode programMode;
        private int programBankCount;
        private int programBank0;
        private int programBank1;
        private int programBankLastAddress;
        private int programBankNextToLastAddress;

        private int characterBankCount;
        private int[] characterBank;

        private byte cpuClock;
        private byte irqCounter;
        private byte irqReloadValue;
        private IrqCountMode irqCountMode;
        private bool irqEnable;
        private bool irqEnableOnAcknowledge;
        private int irqPrescaler;
        private bool irqTriggered;

        private enum ProgramMode
        {
            Mode0,
            Mode1
        }

        private enum IrqCountMode
        {
            Scanline,
            Cpu
        }

    }
}
