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
        public enum Variant
        {
            Bandai_FCG_1_and_2 = 1,
            LZ93D50 = 2,
            LZ93D50_with_24C01 = 4,
            LZ93D50_with_24C02 = 8,
            LZ93D50_with_SRAM = 16,
            Datach_Joint_Rom_System = 32
        }

        public CartridgeMapBandaiFcg(Cartridge cartridge, Variant variants)
        {
            Cartridge = cartridge;

            this.variants = variants;

            // build name dynamically from variants
            // and determine variant characteristics
            outerProgramBankSupported = false;
            characterBanksSupported = true;
            registerBase = 0x6000; // mapper 16 lumps FCG1/2, LZ93D50 and LZ93D50with24C02 together, so assume $6000 to catch all register cases
            List<String> variantNames = new List<string>();
            foreach (Variant variant in Enum.GetValues(typeof(Variant)))
            {
                if ((variants & variant) != 0)
                {
                    variantNames.Add(variant.ToString().Replace("_", " "));
                    if (variant == Variant.Datach_Joint_Rom_System)
                    {
                        characterBanksSupported = false;
                        registerBase = 0x8000; // Datach has registers based at $8000
                    }
                    if (variant == Variant.LZ93D50_with_SRAM)
                    {
                        outerProgramBankSupported = true;
                        characterBanksSupported = false;
                        registerBase = 0x8000; // SRAM variant has registers based at $8000 so that SRAM us based in $6000-$7FFF
                    }
                }
            }
            this.variantName = string.Join(" / ", variantNames);
            this.saveRamSupported = this.variants == Variant.LZ93D50_with_SRAM;

            programBankCount = cartridge.ProgramRom.Count / 0x4000;
            programBank = 0;
            lastProgramBankBase = (programBankCount - 1) * 0x4000;
            characterBank = new int[8];
            mirrorMode = cartridge.MirrorMode;
        }

        public override string Name
        {
            get
            {
                return this.variantName;
            }
        }

        public Cartridge Cartridge { get; private set; }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    if (characterBanksSupported)
                    {
                        // get data from corresponding 1K bank
                        int bankindex = address / 0x400;
                        int bankOffset = address % 0x400;
                        return Cartridge.CharacterRom[characterBank[bankindex] * 0x400 + bankOffset];
                    }
                    else
                    {
                        // for Datach Joint ROM System - treat as flat CHR RAM
                        return Cartridge.CharacterRom[address];
                    }
                }
                
                if (address >= 0x6000 && address < 0x7FFF)
                {
                    // return SRAM or open bus depending on variant
                    return saveRamSupported ? Cartridge.SaveRam[(ushort)(address - 0x6000)] : (byte)(address >> 8);
                }   
                 
                if (address >= 0x8000 && address < 0xC000)
                {
                    int bankOffset = address % 0x4000;
                    return Cartridge.ProgramRom[outerProgramBank + programBank * 0x4000 + bankOffset];
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
                    if (characterBanksSupported)
                    {
                        // CHR bank switches for 8 0x400 ranges
                        // set CHR bank for corresponding 0x400 range
                        int bankindex = address / 0x400;
                        int bankOffset = address % 0x400;
                        Cartridge.CharacterRom[characterBank[bankindex] * 0x400 + bankOffset] = value;
                    }
                    else
                    {
                        // for Datach Joint ROM System - treat as flat CHR RAM
                        Cartridge.CharacterRom[address] = value;
                    }
                }
                else if (saveRamSupported && address >= 0x6000 && address < 0x8000)
                {
                    Cartridge.SaveRam[(ushort)(address - 0x6000)] = value;
                }
                else if (address >= registerBase)
                {
                    // NOTE: FCG variants vary register base between $6000 and $8000
                    // variants lumped under mapper 16 require mirroring these bases to get most games to work
                    int registerAddress = address % 0x10;
                    if (outerProgramBankSupported && registerAddress < 0x04)
                    {
                        // outer program bank (mapper 153 only)
                        if ((value & 0x01) != 0)
                            outerProgramBank = 0x40000;
                        else
                            outerProgramBank = 0;
                    }
                    else if (characterBanksSupported && registerAddress < 0x08)
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

        private Variant variants;
        private string variantName;

        private bool outerProgramBankSupported;
        private bool characterBanksSupported;
        private bool saveRamSupported;
        private ushort registerBase;

        private int programBankCount;
        private int outerProgramBank;
        private int[] characterBank;
        private int programBank;
        private int lastProgramBankBase;
        private MirrorMode mirrorMode;
        private bool irqEnabled;
        private ushort irqCounter;
        private ushort cpuClock;
    }
}
