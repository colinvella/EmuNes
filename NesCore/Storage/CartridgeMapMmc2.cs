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
            programBankCount = (byte)(Cartridge.ProgramRom.Count / 0x2000);
            programBank = 0;
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
                if (address < 0x2000)
                {
                    SetCharacterBanks();

                    byte value = 0;
                    ushort index = (ushort)(address & 0x0FFF);

                    if (address < 0x1000)
                        value = Cartridge.CharacterRom[selectedCharacterBank0 * 0x1000 + index];
                    else // 0x1000 - 0x1FFF
                        value = Cartridge.CharacterRom[selectedCharacterBank1 * 0x1000 + index];
                    
                    if (address == 0x0FD8)
                        latch0 = 0xFD;
                    else if (address == 0x0FE8)
                        latch0 = 0xFE;

                    if (address >= 0x1FD8 && address <= 0x1FDF)
                        latch1 = 0xFD;
                    else if (address >= 0x1FE8 && address <= 0x1FEF)
                        latch1 = 0xFE;

                    return value;
                }

                if (address >= 0x8000)
                {
                    int index = address & 0x1FFF;

                    if (address < 0xA000)
                        return Cartridge.ProgramRom[programBank * 0x2000 + index];
                    else if (address < 0xC000)
                        return Cartridge.ProgramRom[(programBankCount - 3) * 0x2000 + index];
                    else if (address < 0xE000)
                        return Cartridge.ProgramRom[(programBankCount - 2) * 0x2000 + index];
                    else // 0xE000 - 0xFFFF
                        return Cartridge.ProgramRom[(programBankCount - 1) * 0x2000 + index];
                }

                throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
            }

            set
            {
                if (address < 0x2000)
                {
                    int index = address & 0x0FFF;
                    SetCharacterBanks();

                    if (address < 0x1000)
                        Cartridge.CharacterRom[selectedCharacterBank0 * 0x1000 + index] = value;
                    else
                        Cartridge.CharacterRom[selectedCharacterBank1 * 0x1000 + index] = value;

                    return;
                }

                if (address >= 0x8000)
                {
                    if (address < 0xA000)
                    {
                        int index = address & 0x1FFF;
                        throw new NotImplementedException("MMC2 write to $8000 - $9FFF");
                    }
                    else if (address < 0xB000)
                        programBank = (byte)(value & 0x0F);
                    else if (address < 0xC000)
                        characterBank0 = (byte)(value & 0x1F);
                    else if (address < 0xD000)
                        characterBank1 = (byte)(value & 0x1F);
                    else if (address < 0xE000)
                        characterBank2 = (byte)(value & 0x1F);
                    else if (address < 0xF000)
                        characterBank3 = (byte)(value & 0x1F);
                    else //address >= 0xF000
                    {
                        Cartridge.MirrorMode = ((value & 1) == 1) ? MirrorMode.Horizontal : MirrorMode.Vertical;
                        Cartridge.MirrorModeChanged?.Invoke();
                    }
                    return;
                }

                throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));
            }
        }

        public void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
        }

        private void SetCharacterBanks()
        {
            selectedCharacterBank0 = latch0 == 0xFD ? characterBank0 : characterBank1;
            selectedCharacterBank1 = latch1 == 0xFD ? characterBank2 : characterBank3;
    	}

        private byte programBankCount;
        private byte programBank;
        private byte characterBank0;
        private byte characterBank1;
        private byte characterBank2;
        private byte characterBank3;
        private byte selectedCharacterBank0;
        private byte selectedCharacterBank1;
        private byte latch0;
        private byte latch1;
    }
}
