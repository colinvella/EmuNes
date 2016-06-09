using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMap100In1 : CartridgeMap
    {
        public CartridgeMap100In1(Cartridge cartridge)
        {
            this.Cartridge = cartridge;
            bankMode = 0;
            programRam = new byte[0x2000];
        }

        public Cartridge Cartridge { get; private set; }

        public byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                    return Cartridge.CharacterRom[address];

                if (address >= 0x6000 && address < 0x8000)
                    return programRam[address - 0x6000];

                int index = 0;
                switch (bankMode)
                {
                    case 0:
                        index = address & 0x3FFF;
                        if (address >= 0x8000 && address < 0xC000)
                        {
                            return Cartridge.ProgramRom[programRomBank * 0x4000 + index];
                        }
                        else if (address >= 0xC000)
                        {
                            return Cartridge.ProgramRom[(programRomBank | 1) * 0x4000 + index];
                        }
                        break;
                    case 1:
                        index = address & 0x3FFF;
                        if (address >= 0x8000 && address < 0xC000)
                        {
                            return Cartridge.ProgramRom[programRomBank * 0x4000 + index];
                        }
                        else if (address >= 0xC000)
                        {
                            // last bank
                            return Cartridge.ProgramRom[Cartridge.ProgramRom.Count - 0x4000 + index];
                        }
                        break;
                    case 2:
                        // 8k banks
                        index = address & 0x1FFF;
                        if (address >= 0x8000)
                        {
                            return Cartridge.ProgramRom[programRomBank * 0x4000 + subBank * 0x2000 + index];
                        }
                        break;
                    case 3:
                        // 16k banks (mirrored)
                        index = address & 0x3FFF;
                        if (address >= 0x8000)
                        {
                            return Cartridge.ProgramRom[programRomBank * 0x4000 + index];
                        }
                        break;
                }

                throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x2000)
                {
                    Cartridge.CharacterRom[address] = value;
                    return;
                }

                if (address >= 0x6000 && address < 0x8000)
                {
                    programRam[address - 0x6000] = value;
                    return;
                }

                if (address >= 0x8000)
                {
                    bankMode = address & 0x03;
                    programRomBank = value & 0x3f;
                    subBank = value >> 7;

                    byte mirrorMode = (value & 0x40) != 0 ? Cartridge.MirrorHorizontal : Cartridge.MirrorVertical;
                    if (Cartridge.MirrorMode != mirrorMode)
                    {
                        Cartridge.MirrorMode = mirrorMode;
                        Cartridge.MirrorModeChanged?.Invoke();
                    }
                    return;
                }

                throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));
            }
        }

        public string Name
        {
            get { return "100-in-1"; }
        }

        public Action TriggerInterruptRequest
        {
            get { return null; }
            set { }
        }

        public void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites) { }

        private int bankMode;
        private int programRomBank;
        private int subBank;
        private byte[] programRam;
    }
}
