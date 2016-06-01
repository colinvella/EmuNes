using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    public abstract class ProceduralGenerator: WaveGenerator
    {
        public bool LengthEnabled { get; protected set; }
        public byte LengthValue { get; set; }

        public ushort TimerPeriod { get; protected set; }
        public ushort TimerValue { get; protected set; }

        public void StepLength()
        {
            if (LengthEnabled && LengthValue > 0)
                --LengthValue;
        }

        public override void SaveState(BinaryWriter binaryWriter)
        {
            base.SaveState(binaryWriter);

            binaryWriter.Write(LengthEnabled);
            binaryWriter.Write(LengthValue);

            binaryWriter.Write(TimerPeriod);
            binaryWriter.Write(TimerValue);

        }

        public override void LoadState(BinaryReader binaryReader)
        {
            base.LoadState(binaryReader);

            LengthEnabled = binaryReader.ReadBoolean();
            LengthValue = binaryReader.ReadByte();

            TimerPeriod = binaryReader.ReadUInt16();
            TimerValue = binaryReader.ReadUInt16();
        }

    }
}
