using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapBandaiFcg : CartridgeMap
    {
        public CartridgeMapBandaiFcg(Cartridge cartridge)
        {
            Cartridge = cartridge;
            programBankCount = cartridge.ProgramRom.Count / 0x4000;
            programBank = 0;
            lastProgramBankBase = (programBankCount - 1) * 0x4000;
            characterBank = new int[8];
            mirrorMode = cartridge.MirrorMode;
        }

        public override string Name { get { return "Bandai FCG"; } }

        public Cartridge Cartridge { get; private set; }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    int bankindex = address / 0x400;
                    int bankOffset = address % 0x400;
                    return Cartridge.CharacterRom[characterBank[bankindex] * 0x400 + bankOffset];
                }
                
                if (address >= 0x6000 && address < 0x7FFF)
                {
                    // due to FCG board variants, simplest to just return 0 (EEPROM not emulated)
                    return 0x00;
                }   
                 
                if (address >= 0x8000 && address < 0xC000)
                {
                    int bankOffset = address % 0x4000;
                    return Cartridge.ProgramRom[programBank * 0x4000 + bankOffset];
                }

                if (address >= 0xC000)
                {
                    // fixed at last bank
                    int lastBankOffset = address % 0x4000;
                    return Cartridge.ProgramRom[lastProgramBankBase + lastBankOffset];
                }

                //if (address >= 0x6000)
                //    return Cartridge.SaveRam[(ushort)(address - 0x6000)];

                //throw new Exception("Unhandled " + Name + " mapper read at address: " + Hex.Format(address));
                // return open bus?
                return (byte)(address >> 8);
            }

            set
            {
                if (address < 0x2000)
                {
                    // CHR bank switches for 8 0x400 ranges
                    int bankindex = address / 0x400;
                    int bankOffset = address % 0x400;
                    Cartridge.CharacterRom[characterBank[bankindex] * 0x400 + bankOffset] = value;
                }
                else if (address >= 0x6000)
                {
                    // NOTE: FCG variants vary register base between $6000 and $8000
                    // it is sufficient to mirror these bases to get most games to work
                    int registerAddress = address % 0x10;
                    if (registerAddress < 0x08)
                    {
                        // CHR bank switch
                        int oldCharacterBank = characterBank[registerAddress];
                        characterBank[registerAddress] = value;
                        if (value != oldCharacterBank)
                            CharacterBankSwitch?.Invoke((ushort)(registerAddress * 0x400), 0x400);
                    }
                    else if (registerAddress == 0x08)
                    {
                        // program bank switch
                        int oldProgramBank = programBank;
                        programBank = value & 0x0F;
                        programBank %= programBankCount; // probably not needed, but anyhow

                        // invalidate address region
                        if (programBank != oldProgramBank)
                            ProgramBankSwitch?.Invoke(0x8000, 0x4000);
                    }
                    else if (registerAddress == 0x09)
                    {
                        // mirroring mode
                        MirrorMode newMirrorMode = mirrorMode;
                        switch (value & 0x03)
                        {
                            case 0: newMirrorMode = MirrorMode.Vertical; break;
                            case 1: newMirrorMode = MirrorMode.Horizontal; break;
                            case 2: newMirrorMode = MirrorMode.Single0; break;
                            case 3: newMirrorMode = MirrorMode.Single1; break;
                        }

                        if (newMirrorMode != mirrorMode)
                        {
                            mirrorMode = newMirrorMode;
                            MirrorModeChanged?.Invoke(mirrorMode);
                        }
                    }
                    else if (registerAddress == 0x0A)
                    {
                        irqEnabled = (value & 0x01) != 0;
                        CancelInterruptRequest?.Invoke();
                    }
                    else if (registerAddress == 0x0B)
                    {
                        irqCounter &= 0xFF00;
                        irqCounter |= value;
                    }
                    else if (registerAddress == 0x0C)
                    {
                        irqCounter &= 0x00FF;
                        irqCounter |= (ushort)(value << 8);
                    }
                    // TODO: D: eeprom/PRG ram enable
                    // TODO: variants
                }
                else
                {
                    throw new Exception("Unhandled " + Name + " mapper write at address: " + Hex.Format(address));
                    //ignore writes?
                }
            }
        }

        public override void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites)
        {
            cpuClock++;
            cpuClock %= 3;

            if (irqEnabled && cpuClock == 0)
            {
                --irqCounter;
                if (irqCounter == 0)
                    TriggerInterruptRequest();
            }
        }

        private int programBankCount;
        private int[] characterBank;
        private int programBank;
        private int lastProgramBankBase;
        private MirrorMode mirrorMode;
        private bool irqEnabled;
        private ushort irqCounter;
        private ushort cpuClock;
    }
}
