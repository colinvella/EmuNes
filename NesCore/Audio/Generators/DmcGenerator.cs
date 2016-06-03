using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    public class DmcGenerator : WaveGenerator
    {
        public delegate byte ReadMemorySampleHandler(ushort address);

        public override byte Control
        {
            set
            {
                InterruptRequestEnabled = (value & 0x80) == 0x80;
                loop = (value & 0x40) == 0x40;
                tickPeriod = dmcTable[value & 0x0F];
            }
        }

        public override byte Output
        {
            get
            {
                return sampleValue;
            }
        }

        public byte SampleValue
        {
            set
            {
                sampleValue = (byte)(value & 0x7F);
            }
        }

        public ushort SampleAddress
        {
            set
            {
                sampleAddress = (ushort)(0xC000 | (value << 6));
            }
        }

        public ushort SampleLength
        {
            set
            {
                sampleLength = (ushort)((value << 4) | 1);
            }
        }

        public ushort CurrentLength { get; set; }

        public bool InterruptRequestEnabled { get; private set; }

        public ReadMemorySampleHandler ReadMemorySample { get; set; }

        /// <summary>
        /// Handler for triggering interrupt requests
        /// </summary>
        public Action TriggerInterruptRequest { get; set; }

        public override void StepTimer()
        {
            if (!Enabled)
                return;

            StepReader();

            if (tickValue == 0)
            {
                tickValue = tickPeriod;
                StepShifter();
            }
            else
            {
                --tickValue;
            }
        }

        public void Restart()
        {
            currentAddress = sampleAddress;
            CurrentLength = sampleLength;
        }

        public override void SaveState(BinaryWriter binaryWriter)
        {
            base.SaveState(binaryWriter);

            binaryWriter.Write(sampleValue);

            binaryWriter.Write(sampleAddress);
            binaryWriter.Write(sampleLength);

            binaryWriter.Write(currentAddress);
            binaryWriter.Write(CurrentLength);

            binaryWriter.Write(shiftRegister);
            binaryWriter.Write(bitCount);

            binaryWriter.Write(tickPeriod);
            binaryWriter.Write(tickValue);

            binaryWriter.Write(loop);

            binaryWriter.Write(InterruptRequestEnabled);
        }

        public override void LoadState(BinaryReader binaryReader)
        {
            base.LoadState(binaryReader);

            sampleValue = binaryReader.ReadByte();

            sampleAddress = binaryReader.ReadUInt16();
            sampleLength = binaryReader.ReadUInt16();

            currentAddress = binaryReader.ReadUInt16();
            CurrentLength = binaryReader.ReadUInt16();

            shiftRegister = binaryReader.ReadByte();
            bitCount = binaryReader.ReadByte();

            tickPeriod = binaryReader.ReadByte();
            tickValue = binaryReader.ReadByte();

            loop = binaryReader.ReadBoolean();

            InterruptRequestEnabled = binaryReader.ReadBoolean();
        }

        private void StepReader()
        {
            if (CurrentLength > 0 && bitCount == 0)
            {
                // cpu should stall 4 cycles here
                if (ReadMemorySample != null)
                   shiftRegister = ReadMemorySample(currentAddress);

                bitCount = 8;

                ++currentAddress;
                if (currentAddress == 0)
                    currentAddress = 0x8000;

                --CurrentLength;

                if (CurrentLength == 0)
                {
                    if (loop)
                        Restart();
                    else
                    {
                        if (InterruptRequestEnabled)
                            TriggerInterruptRequest();

                    }

                }
            }
        }

        private void StepShifter()
        {
            if (bitCount == 0)
                return;

            if ((shiftRegister & 1) == 1)
            {
                if (sampleValue <= 125)
                    sampleValue += 2;
            }
            else
            {
                if (sampleValue >= 2)
                    sampleValue -= 2;
            }

            shiftRegister >>= 1;

            --bitCount;        
        }

        private byte sampleValue;

        private ushort sampleAddress;
        private ushort sampleLength;

        private ushort currentAddress;

        private byte shiftRegister;
        private byte bitCount;

        private byte tickPeriod;
        private byte tickValue;

        private bool loop;

        private static readonly byte[] dmcTable = {
            214, 190, 170, 160, 143, 127, 113, 107, 95, 80, 71, 64, 53, 42, 36, 27,
        };
    }
}
