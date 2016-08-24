using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapTaitoTc0690 : CartridgeMapTaitoTc0190
    {
        public CartridgeMapTaitoTc0690(Cartridge cartridge) : base(cartridge)
        {
        }

        public override string Name { get { return "Taito TC0690"; } }

        public override byte this[ushort address]
        {
            set
            {

                if (address == 0x8000)
                {
                    // ignore mirror mode set by register $8000 for TC0190
                    MirrorMode oldMirrorMode = MirrorMode;
                    base[address] = value;
                    MirrorMode = oldMirrorMode;
                }
                else if (address == 0xC000)
                {
                    // irq latch - reload value
                    irqReload = value;
                    irqReload ^= 0xFF;
                }
                else if (address == 0xC001)
                {
                    // irq reload - set irq counter to reload value
                    irqReloadPrimed = true;
                }
                else if (address == 0xC002)
                {
                    // enable IRQ
                    irqEnabled = true;
                }
                else if (address == 0xC003)
                {
                    // disable IRQ
                    irqEnabled = false;
                    CancelInterruptRequest?.Invoke();
                }
                else if (address == 0xE000)
                {
                    // $E000: [.M.. ....]   Mirroring: 0 = Vert, 1 = Horz
                    MirrorMode = (value & Bin.Bit6) != 0 ? MirrorMode.Horizontal : MirrorMode.Vertical;
                }
                else
                {
                    base[address] = value;
                }

            }
        }

        public override void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            if (cycle != 332)
                return;

            if (irqReloadPrimed)
            {
                irqCounter = irqReload;
                irqReloadPrimed = false;
            }

            if (scanLine > 239 && scanLine < 261)
                return;

            if (!showBackground && !showSprites)
                return;

            if (irqCounter == 0)
            {
                irqCounter = irqReload;
            }
            else
            {
                --irqCounter;
                if (irqCounter == 0 && irqEnabled)
                    TriggerInterruptRequest?.Invoke();
            }
        }

        private byte irqCounter;
        private bool irqReloadPrimed;
        private byte irqReload;
        private bool irqEnabled;
    }
}
