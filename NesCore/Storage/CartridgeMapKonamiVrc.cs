using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    abstract class CartridgeMapKonamiVrc : CartridgeMap
    {
        public CartridgeMapKonamiVrc(Cartridge cartridge) : base(cartridge)
        {
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

        protected void WriteIrqReloadValueLowNybble(byte value)
        {
            irqReloadValue &= 0xF0;
            irqReloadValue |= (byte)(value & 0x0F);
        }

        protected void WriteIrqReloadValueHighNybble(byte value)
        {
            irqReloadValue &= 0x0F;
            irqReloadValue |= (byte)((value & 0x0F) << 4);
        }

        protected void WriteIrqControl(byte value)
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

        protected void WriteIrqAcknowledge()
        {
            if (irqTriggered)
                CancelInterruptRequest?.Invoke();
            irqEnable = irqEnableOnAcknowledge;
            irqTriggered = false;
        }

        private byte cpuClock;
        private byte irqCounter;
        private byte irqReloadValue;
        private IrqCountMode irqCountMode;
        private bool irqEnable;
        private bool irqEnableOnAcknowledge;
        private int irqPrescaler;
        private bool irqTriggered;

        private enum IrqCountMode
        {
            Scanline,
            Cpu
        }
    }
}
