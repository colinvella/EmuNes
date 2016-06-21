using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    public class TriangleGenerator : ProceduralGenerator
    {
        public byte DutyValue { get; private set; }

        public byte CounterPeriod { get; private set; }
        public byte CounterValue { get; private set; }
        public bool CounterReload { get; private set; }

        public override byte Control
        {
            set
            {
                LengthEnabled = ((value >> 5) & 1) == 0;
                CounterPeriod = (byte)(value & 0x0F);
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

                if (CounterValue == 0)
                    return 0;

                return triangleTable[DutyValue];
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
                TimerValue = TimerPeriod;
                CounterReload = true;
            }
        }

        public override void StepTimer()
        {
            if (TimerValue == 0)
            {
                TimerValue = TimerPeriod;
                if (LengthValue > 0 && CounterValue > 0)
                {
                    ++DutyValue;
                    DutyValue %= 32;
                }
            }
            else
            {
                --TimerValue;
            }
        }

        public void StepCounter()
        {
            if (CounterReload)
                CounterValue = CounterPeriod;
            else if (CounterValue > 0)
                --CounterValue;

            if (LengthEnabled)
                CounterReload = false;
        }

        public override void SaveState(BinaryWriter binaryWriter)
        {
            base.SaveState(binaryWriter);

            binaryWriter.Write(DutyValue);

            binaryWriter.Write(CounterPeriod);
            binaryWriter.Write(CounterValue);
            binaryWriter.Write(CounterReload);
        }

        public override void LoadState(BinaryReader binaryReader)
        {
            base.LoadState(binaryReader);

            DutyValue = binaryReader.ReadByte();

            CounterPeriod = binaryReader.ReadByte();
            CounterValue = binaryReader.ReadByte();
            CounterReload = binaryReader.ReadBoolean();
        }

        private static readonly byte[] triangleTable = {
            15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0,
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
        };
    }
}
