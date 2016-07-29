using System;
using System.Collections.Generic;
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
            programBank8k = programBankCount16K * 2;
            programBankLastBankAddress = cartridge.ProgramRom.Count - 0x2000;
        }

        public override string Name { get { return mapperName; } }

        public override byte this[ushort address]
        {
            get
            {
                return (byte)(address >> 8); // open bus
            }

            set
            {
                address &= 0xF003;
                byte addressHighNybble = (byte)(address >> 12);
                byte addressLowBits = (byte)(address & 0x03);
                if (variant == Variant.Vrc6b)
                {
                    // for Rev B, swap A0 and A1
                    if (addressLowBits == 1)
                        addressLowBits = 2;
                    else if (addressLowBits == 2)
                        addressLowBits = 1;
                }

                if (addressHighNybble == 0x8)
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
                }
                else if (addressHighNybble == 0xC)
                {
                    programBank8k = value & 0x1F;
                    programBank8k %= programBankCount8K; // paranoia
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

        private Variant variant;
        private string mapperName;

        private int programBankCount16K;
        private int programBankCount8K;
        private int programBankLastBankAddress;
        private int programBank16k;
        private int programBank8k;

        private bool programRamEnabled;
    }
}
