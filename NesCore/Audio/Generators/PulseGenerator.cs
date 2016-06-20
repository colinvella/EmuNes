using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    public class PulseGenerator: ProceduralGenerator
    {
        public PulseGenerator(byte channel)
        {
            if (channel != 1 && channel != 2)
                throw new ArgumentOutOfRangeException("channel", "channel should have value 1 or 2");

            this.Channel = channel;
        }

        public override byte Control
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

        public override byte Output
        {
            get
            {
                if (!Enabled)
                    return 0;

                if (LengthValue == 0)
                    return 0;

                if (dutyTable[DutyMode][DutyValue] == 0)
                    return 0;

                if (TimerPeriod < 8 || TimerPeriod > 0x7FF)
                    return 0;

                //if (!SweepNegate && TimerPeriod + (TimerPeriod >> SweepShift) > 0x7FF)
                //    return 0;

                return EnvelopeEnabled ? EnvelopeVolume : ConstantVolume;
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
                if (!Enabled) return;

                LengthValue = lengthTable[value >> 3];
                TimerPeriod = (ushort)((TimerPeriod & 0x00FF) | ((value & 7) << 8));
                EnvelopeStart = true;
                DutyValue = 0;
            }
        }

        public byte Channel { get; private set; }

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

        public override void StepTimer()
        {
            if (TimerValue == 0)
            {
                TimerValue = TimerPeriod;
                ++DutyValue;
                DutyValue %= 8;
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

        public void StepSweep()
        {
            if (SweepReload)
            {
                if (SweepEnabled && SweepValue == 0)
                    ApplySweep();

                SweepValue = SweepPeriod;
                SweepReload = false;
            }
            else if (SweepValue > 0)
            {
                --SweepValue;
            }
            else
            {
                if (SweepEnabled)
                    ApplySweep();

                SweepValue = SweepPeriod;
            }
        }

        public override void SaveState(BinaryWriter binaryWriter)
        {
            base.SaveState(binaryWriter);

            binaryWriter.Write(Channel);

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

        public override void LoadState(BinaryReader binaryReader)
        {
            base.LoadState(binaryReader);

            Channel = binaryReader.ReadByte();

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

        private void ApplySweep()
        {
            ushort delta = (ushort)(TimerPeriod >> SweepShift);

            if (SweepNegate)
            {
                TimerPeriod -= delta;

                if (Channel == 1)
                    --TimerPeriod;
            }
            else
            {
                TimerPeriod += delta;
            }
        }
    }
}
