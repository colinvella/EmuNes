using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio
{
    public class PulseGenerator: WaveGenerator
    {
        public byte Control
        {
            set
            {
                DutyMode = (byte)((value >> 6) & 3);
                LengthEnabled = ((value >> 5) & 1) == 0;
                EnvelopeLoop = ((value >> 5) & 1) == 1;
                EnvelopeEnabled = ((value >> 4) & 1) == 0;
                EnvelopePeriod = ConstantVolume = (byte)(value & 15);
                EnvelopeStart = true;
            }
        }

        public byte Sweep
        {
            set
            {
                SweepEnabled = ((value >> 7) & 1) == 1;
                SweepPeriod = (byte)((value >> 4) & 7);
                SweepNegate = ((value >> 3) & 1) == 1;
                SweepShift = (byte)(value & 7);
                SweepReload = true;
            }
        }

        public byte TimerLow
        {
            set
            {
                TimerPeriod &= 0xFF00;
                TimerPeriod |= value;
            }
        }

        public byte TimerHigh
        {
            set
            {
                LengthValue = lengthTable[value >> 3];
                TimerPeriod = (ushort)((TimerPeriod & 0x00FF) | ((value & 7) << 8));
                EnvelopeStart = true;
                DutyValue = 0;
            }
        }

        public bool Enabled { get; private set; }

        public byte Channel { get; private set; }

        public bool LengthEnabled { get; private set; }
        public byte LengthValue { get; private set; }

        public ushort TimerPeriod { get; private set; }
        public ushort TimerValue { get; private set; }

        public byte DutyMode { get; private set; }
        public byte DutyValue { get; private set; }

        public bool SweepReload { get; private set; }
        public bool SweepEnabled { get; private set; }
        public bool SweepNegate { get; private set; }
        public byte SweepShift { get; private set; }
        public byte SweepPeriod { get; private set; }
        public byte SweepValue { get; private set; }

        public bool EnvelopeEnabled { get; private set; }
        public bool EnvelopeLoop { get; private set; }
        public bool EnvelopeStart { get; private set; }
        public byte EnvelopePeriod { get; private set; }
        public byte EnvelopeValue { get; private set; }
        public byte EnvelopeVolume { get; private set; }
        public byte ConstantVolume { get; private set; }

        void SaveState(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Enabled);

            binaryWriter.Write(Channel);

            binaryWriter.Write(LengthEnabled);
            binaryWriter.Write(LengthValue);

            binaryWriter.Write(TimerPeriod);
            binaryWriter.Write(TimerValue);

            binaryWriter.Write(DutyMode);
            binaryWriter.Write(DutyValue);

            binaryWriter.Write(SweepReload);
            binaryWriter.Write(SweepEnabled);
            binaryWriter.Write(SweepNegate);
            binaryWriter.Write(SweepShift);
            binaryWriter.Write(SweepPeriod);
            binaryWriter.Write(SweepValue);

            binaryWriter.Write(EnvelopeEnabled);
            binaryWriter.Write(EnvelopeLoop);
            binaryWriter.Write(EnvelopeStart);
            binaryWriter.Write(EnvelopePeriod);
            binaryWriter.Write(EnvelopeValue);
            binaryWriter.Write(EnvelopeVolume);
            binaryWriter.Write(ConstantVolume);
        }

        void LoadState(BinaryReader binaryReader)
        {
            Enabled = binaryReader.ReadBoolean();

            Channel = binaryReader.ReadByte();

            LengthEnabled = binaryReader.ReadBoolean();
            LengthValue = binaryReader.ReadByte();

            TimerPeriod = binaryReader.ReadUInt16();
            TimerValue = binaryReader.ReadUInt16();

            DutyMode = binaryReader.ReadByte();
            DutyValue = binaryReader.ReadByte();

            SweepReload = binaryReader.ReadBoolean();
            SweepEnabled = binaryReader.ReadBoolean();
            SweepNegate = binaryReader.ReadBoolean();
            SweepShift = binaryReader.ReadByte();
            SweepPeriod = binaryReader.ReadByte();
            SweepValue = binaryReader.ReadByte();

            EnvelopeEnabled = binaryReader.ReadBoolean();
            EnvelopeLoop = binaryReader.ReadBoolean();
            EnvelopeStart = binaryReader.ReadBoolean();
            EnvelopePeriod = binaryReader.ReadByte();
            EnvelopeValue = binaryReader.ReadByte();
            EnvelopeVolume = binaryReader.ReadByte();
            ConstantVolume = binaryReader.ReadByte();
        }

    }
}
