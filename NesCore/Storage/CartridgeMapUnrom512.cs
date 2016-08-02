using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapUnrom512 : CartridgeMap
    {
        public CartridgeMapUnrom512(Cartridge cartridge) : base(cartridge)
        {
            // note - self flashing functionality not implemented
            programLastAddress16k = Cartridge.ProgramRom.Count - 0x4000;

            flashMemory = new FlashMemory(Cartridge.ProgramRom.ToArray());
        }

        public override string Name { get { return "UNROM 512"; } }

        public override byte this[ushort address]
        {
            get
            {
                if (address < 0x2000)
                {
                    return Cartridge.CharacterRom[characterBank * 0x2000 + address];
                }
                else if (address >= 0x8000 && address < 0xC000)
                {
                    return flashMemory[(uint)(programBank * 0x4000 + address % 0x4000)];
                }
                else if (address >= 0xC000)
                {
                    return flashMemory[(uint)(programLastAddress16k + address % 0x4000)];
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected read from address " + Hex.Format(address));
                    return (byte)(address >> 8);
                }
            }

            set
            {
                flashMemory[address] = value;

                if (address > 0x8000)
                {
                    // MCCP PPPP
                    MirrorMode = (value & 0x80) != 0 ? MirrorMode.Single1 : MirrorMode.Single0;

                    int oldProgramBank = programBank;
                    programBank = value & 0x1F;
                    if (programBank != oldProgramBank)
                        ProgramBankSwitch?.Invoke(0x8000, 0x4000);
                    
                    int oldCharacterBank = characterBank;
                    characterBank = (value >> 5) & 0x03;
                    if (characterBank != oldCharacterBank)
                        CharacterBankSwitch?.Invoke(0x0000, 0x2000);
                }
                else
                {
                    Debug.WriteLine(Name + ": Unexpected write of value " + Hex.Format(value) + " to address " + Hex.Format(address));
                }
            }
        }

        private FlashMemory flashMemory;
        private int programLastAddress16k;
        private int programBank;
        private int characterBank;

        private class FlashMemory
        {
            // implementation of SST39SF040 flash CMOS - may extract as separate class in the future
            public FlashMemory(byte[] memory)
            {
                this.memory = memory.ToArray();
            }

            public byte this[uint address]
            {
                get  { return memory[address % memory.Length]; }

                set
                {
                    address = (uint)(address % memory.Length);

                    // process code sequence to flash 1K sector
                    if (flashSectorAddressSequence[flashSectorState] == address && flashSectorValueSequence[flashSectorState] == value)
                    {
                        ++flashSectorState;
                    }
                    else if (flashSectorAddressSequence[flashSectorState] == address && flashSectorValueSequence[flashSectorState] == 0xFF)
                    {
                        flashBank = value & 0x1F;
                        ++flashSectorState;
                    }
                    else if (flashSectorAddressSequence[flashSectorState] == 0xFFFF && flashSectorValueSequence[flashSectorState] == value)
                    {
                        flashSectorState = 0;
                        if (address == 0x8000 || address == 0x9000 || address == 0xA000 || address == 0xB000)
                        {
                            int sectorStart = (int)(flashBank * 0x4000 + address - 0x8000);
                            int sectorEnd = sectorStart + 0x1000;
                            for (int index = sectorStart; index < sectorEnd; index++)
                                memory[index] = 0xFF;
                        }
                    }

                    // process code sequence to write a byte to flash memory
                    if (writeByteAddressSequence[writeByteState] == address && writeByteValueSequence[writeByteState] == value)
                    {
                        ++writeByteState;
                    }
                    else if (writeByteAddressSequence[writeByteState] == address && writeByteValueSequence[writeByteState] == 0xFF)
                    {
                        flashBank = value & 0x1F;
                        ++writeByteState;
                    }
                    else if (writeByteAddressSequence[writeByteState] == 0xFFFF && writeByteValueSequence[writeByteState] == 0xFF)
                    {
                        writeByteState = 0;
                        if (address >= 0x8000 && address < 0xC000)
                            memory[flashBank * 0x4000 + address % 0x4000] = value;
                          
                    }
                }
            }

            private byte[] memory;

            private ushort[] flashSectorAddressSequence = { 0xC000, 0x9555, 0xC000, 0xAAAA, 0xC000, 0x9555, 0xC000, 0x9555, 0xC000, 0xAAAA, 0xC000, 0xFFFF };
            private byte[] flashSectorValueSequence = { 0x01, 0xAA, 0x00, 0x55, 0x01, 0x80, 0x01, 0xAA, 0x00, 0x55, 0xFF, 0x30 };

            private ushort[] writeByteAddressSequence = { 0xC000, 0x9555, 0xC000, 0xAAAA, 0xC000, 0x9555, 0xC000, 0xFFFF };
            private byte[] writeByteValueSequence = { 0x01, 0xAA, 0x00, 0x55, 0x01, 0xA0, 0xFF, 0xFF };

            private int flashSectorState;
            private int writeByteState;

            private int flashBank;

        }
    }
}
