using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    public class NoiseGenerator : ProceduralGenerator
    {
        public NoiseGenerator()
        {
            ShiftRegister = 1;
        }

        public override byte Control
        {
            set
            {
                LengthEnabled = ((value >> 5) & 1) == 0;
                EnvelopeLoop = !LengthEnabled;
                EnvelopeEnabled = ((value >> 4) & 1) == 0;
                EnvelopePeriod = ConstantVolume = (byte)(value & 15);
            }
        }

        public override byte Output
        {
            get
            {
                if (!Enabled)
                    return 0;

                if (LengthValue == 0)
                    return 0;

                if ((ShiftRegister & 1) == 1)
                    return 0;

                return EnvelopeEnabled ? EnvelopeVolume : ConstantVolume;
            }
        }

        public byte ModeAndPeriod
        {
            set
            {
                Mode = (value & 0x80) == 0x80;
                TimerPeriod = noiseTable[value & 0x0F];
            }
        }

        public byte Length
        {
            set
            {
                if (!Enabled) return;

                LengthValue = lengthTable[value >> 3];
                EnvelopeStart = true;
            }
        }

        public bool Mode { get; private set; }

        public ushort ShiftRegister { get; private set; }

        public bool EnvelopeEnabled { get; private set; }
        public bool EnvelopeLoop { get; private set; }
        public bool EnvelopeStart { get; private set; }
        public byte EnvelopePeriod { get; private set; }
        public byte EnvelopeValue { get; private set; }
        public byte EnvelopeVolume { get; private set; }

        public byte ConstantVolume { get; private set; }

        public override void StepTimer()
        {
            if (TimerValue == 0)
            {
                TimerValue = TimerPeriod;
                byte shift = Mode ? (byte)6 : (byte)1;

                byte b1 = (byte)(ShiftRegister & 1);
                byte b2 = (byte)((ShiftRegister >> shift) & 1);

                ShiftRegister >>= 1;
                ShiftRegister |= (ushort)((b1 ^ b2) << 14);
            }
            else
            {
                --TimerValue;
            }
        }

        public void StepEnvelope()
        {
            if (EnvelopeStart)
            {
                EnvelopeVolume = 15;
                EnvelopeValue = EnvelopePeriod;
                EnvelopeStart = false;
            }
            else if (EnvelopeValue > 0)
            {
                --EnvelopeValue;
            }
            else
            {
                if (EnvelopeVolume > 0)
                {
                    --EnvelopeVolume;
                }
                else if (EnvelopeLoop)
                {
                    EnvelopeVolume = 15;
                }
                EnvelopeValue = EnvelopePeriod;
            }
        }

        public override void LoadState(BinaryReader binaryReader)
        {
            base.LoadState(binaryReader);

            Mode = binaryReader.ReadBoolean();

            ShiftRegister = binaryReader.ReadUInt16();

            EnvelopeEnabled = binaryReader.ReadBoolean();
            EnvelopeLoop = binaryReader.ReadBoolean();
            EnvelopeStart = binaryReader.ReadBoolean();
            EnvelopePeriod = binaryReader.ReadByte();
            EnvelopeValue = binaryReader.ReadByte();
            EnvelopeVolume = binaryReader.ReadByte();

            ConstantVolume = binaryReader.ReadByte();
        }

        public override void SaveState(BinaryWriter binaryWriter)
        {
            base.SaveState(binaryWriter);
            binaryWriter.Write(Mode);

            binaryWriter.Write(ShiftRegister);

            binaryWriter.Write(EnvelopeEnabled);
            binaryWriter.Write(EnvelopeLoop);
            binaryWriter.Write(EnvelopeStart);
            binaryWriter.Write(EnvelopePeriod);
            binaryWriter.Write(EnvelopeValue);
            binaryWriter.Write(EnvelopeVolume);

            binaryWriter.Write(ConstantVolume);
        }

        private static readonly ushort[] noiseTable = {
            4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068,
        };
    }
}
