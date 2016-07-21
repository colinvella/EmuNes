using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{

    class Namco163SoundChip
    {
        public Namco163SoundChip()
        {
            memory = new byte[0x80];

            soundChannels = new SoundChannel[8];
            for (int channelIndex = 0; channelIndex < MaxChannels; channelIndex++)
                soundChannels[channelIndex] = new SoundChannel(memory, 0x40 + channelIndex * 0x08);
        }

        public bool SoundEnable { get; set; }

        public byte AddressPort
        {
            set
            {
                autoIncrement = (value & 0x80) != 0;
                address = (byte)(value & 0x7F);

                Debug.WriteLine("Namco 163 Sound Chip Address = " + Hex.Format(address));
                Debug.WriteLine("Namco 163 Sound AutoIncrement = " + autoIncrement);
            }
        }

        public byte DataPort
        {
            get
            {
                byte value = memory[address];
                ProcessAddress();
                return value;
            }
            set
            {
                memory[address] = value;
                if (address == 0x7F)
                {
                    int enabledChannels = (value >> 4) & 0x07;
                    startChannel = currentChannel = MaxChannels - enabledChannels - 1;
                }
                Debug.WriteLine("Namco 163 Sound Chip[" + Hex.Format(address) + "] = " + Hex.Format(value));
                ProcessAddress();
            }
        }

        public void Update(int cpuCycles)
        {
            availableCycles += cpuCycles;

            while (availableCycles >= 15)
            {
                SoundChannel soundChannel = soundChannels[currentChannel];
                int output = soundChannel.Output;

                //Debug.WriteLineIf(output != 0, "Namco 163 SoundChip Output = " + output);


                currentChannel++;
                if (currentChannel >= MaxChannels)
                    currentChannel = startChannel;

                availableCycles -= 15;
            }
        }

        public int Output
        {
            get
            {
                int sample = 0;
                for (int channelIndex = startChannel; channelIndex < MaxChannels; ++channelIndex)
                {
                    sample += soundChannels[channelIndex].Output * 16;
                }
                //this low pass filter is here to reduce noise in games using 8 channels 
                //while still letting me output 1 after the other like the real chip does 
                sample += lowPassAccumulator;
                lowPassAccumulator -= sample / 16;            
                return lowPassAccumulator;
            }
        }

        private void ProcessAddress()
        {
            if (!autoIncrement)
                return;
            ++address;
            address &= 0x7F;
        }

        private SoundChannel[] soundChannels;

        private byte[] memory;
        private bool autoIncrement;
        private byte address;

        private int startChannel;
        private int currentChannel;
        private int availableCycles;

        private int lowPassAccumulator;

        public const int MaxChannels = 8;

        private class SoundChannel
        {
            public SoundChannel(byte[] memory, int baseAddress)
            {
                this.channelIndex = (baseAddress - 0x40) / 0x08;
                this.memory = memory;
                this.baseAddress = baseAddress;
            }

            public byte LowFrequency { get { return memory[baseAddress]; } }

            public byte LowPhase
            {
                get { return memory[baseAddress + 1]; }
                set { memory[baseAddress + 1] = value; }
            }

            public byte MidFrequency { get { return memory[baseAddress + 2]; } }

            public byte MidPhase {
                get { return memory[baseAddress + 3]; }
                set { memory[baseAddress + 3] = value; }
            }

            public byte HighFrequency { get { return (byte)(memory[baseAddress + 4] & 0x03); } }

            public byte WaveLength { get { return (byte)(memory[baseAddress + 4] >> 2); } }

            public byte HighPhase {
                get { return memory[baseAddress + 5]; }
                set { memory[baseAddress + 5] = value; }
            }

            public byte WaveAddress { get { return memory[baseAddress + 6]; } }

            public byte Volume { get { return (byte)(memory[baseAddress + 7] & 0x0F); } }

            public uint Phase
            {
                get { return (uint)((HighPhase << 16) | (MidPhase << 8) | LowPhase); }
                set {
                    LowPhase = (byte)value;
                    MidPhase = (byte)(value >> 8);
                    HighPhase = (byte)(value >> 16);
                }
            }

            public uint Frequency
            {
                get { return (uint)((HighFrequency << 16) | (MidFrequency << 8) | LowFrequency); }
            }

            public byte Length
            {
                get { return (byte)((64 - WaveLength) * 4); }
            }

            public int Output
            {
                get
                {
                    uint phase = Phase;
                    int offset = WaveAddress;
                    if (Length != 0)
                        phase += (uint)((phase + Frequency) % (Length << 16));
                    int sampleIndex = (int)(((phase >> 16) + offset) & 0xFF);
                    int output = (GetSample(sampleIndex) - 8) * Volume;
                    Phase = phase;
                    return output;
                }
            }

            private byte GetSample(int index)
            {
                index &= 0x7F;
                int nybbleShift = (index & 0x01) * 4;
                return (byte)((memory[index / 2] >> nybbleShift) & 0x0F);
            }

            private int channelIndex;
            private byte[] memory;
            private int baseAddress;
        }
    }

}
