using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapMmc2 : CartridgeMap
    {
        public CartridgeMapMmc2(Cartridge cartridge)
        {
            Cartridge = cartridge;
            programBank = 0;
            programBankLast3 = Cartridge.ProgramRom.Count - 0x2000 * 3;
            characterBank1 = 0;
            characterBank2 = 0;
            latch0 = 0xFD;
            latch1 = 0xFE;
        }

        public virtual string Name { get { return "MMC2"; } }

        public Cartridge Cartridge { get; private set; }

        public Action TriggerInterruptRequest
        {
            get { return null; }
            set { }
        }

        public byte this[ushort address]
        {
            get
            {
                // first 2 switchable 4K PPU banks
                if (address < 0x1000)
                {
                    byte value = Cartridge.CharacterRom[characterBank1 * 0x1000 + address];

                    // chr bank 1 switching by reading specific addresses
                    if (address == 0x0FD8)
                        latch0 = 0xFD;
                    else if (address == 0x0FE8)
                        latch0 = 0xFE;

                    return value;
                }

                // second 2 switchable 4K PPU banks
                if (address < 0x2000)
                {
                    ushort offset = (ushort)(address - 0x1000);
                    byte value = Cartridge.CharacterRom[0x2000 + characterBank2 * 0x1000 + offset];

                    // chr bank 2 switching by reading specific address ranges
                    if (address >= 0x1FD8 && address <= 0x1FDF)
                        latch1 = 0xFD;
                    else if (address >= 0x1FE8 && address <= 0x1FEF)
                        latch1 = 0xFE;

                    // REF: https://github.com/nwidger/nintengo/blob/master/nes/mmc2.go

                    return value;
                }

                if (address >= 0x6000  && address < 0x8000)
                {
                    // first 8K PRG bank
                    address -= 0x6000;
                    return Cartridge.ProgramRom[address];
                }

                if (address >= 0x8000 && address < 0xA000)
                {
                    address -= 0x8000;
                    return Cartridge.ProgramRom[programBank * 0x2000 + address];
                }

                if (address >= 0xA000)
                {
                    address -= 0xA000;
                    return Cartridge.ProgramRom[programBankLast3 + address];
                }

                throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x1000)
                {
                    // first 2 switchable 4K PPU banks
                    Cartridge.CharacterRom[characterBank1 * 0x1000 + address] = value;
                    return;
                }

                if (address < 0x2000)
                {
                    // second 2 switchable 4K PPU banks
                    address -= 0x1000;
                    Cartridge.CharacterRom[0x2000 + characterBank2 * 0x1000 + address] = value;
                    return;
                }

                if (address >= 0xA000 && address < 0xB000)
                {
                    programBank = value & 0x0F;
                    return;
                }

                if (address >= 0xB000 && address < 0xC000)
                {
                    characterBank1 = value & 0x1F;
                    return;
                }

                if (address >= 0xF000)
                {
                    Cartridge.MirrorMode = ((value & 1) == 1) ? Cartridge.MirrorHorizontal : Cartridge.MirrorVertical;
                    Cartridge.MirrorModeChanged?.Invoke();
                    return;
                }

                throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));
            }
        }

        public void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
        }

        private int programBank;
        private int programBankLast3;
        private int characterBank1;
        private int characterBank2;
        private byte latch0;
        private byte latch1;
    }
}
