using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    public abstract class WaveGenerator
    {
        public bool Enabled { get; set; }

        public abstract byte Control { set; }

        public abstract byte Output { get; }

        public abstract void StepTimer();

        public virtual void SaveState(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Enabled);
        }

        public virtual void LoadState(BinaryReader binaryReader)
        {
            Enabled = binaryReader.ReadBoolean();
        }

        protected static readonly byte[] lengthTable = {
            10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
            12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
        };

        protected static readonly byte[][] dutyTable = {
	        new byte[]{0, 1, 0, 0, 0, 0, 0, 0},
            new byte[]{0, 1, 1, 0, 0, 0, 0, 0},
            new byte[]{0, 1, 1, 1, 1, 0, 0, 0},
            new byte[]{1, 0, 0, 1, 1, 1, 1, 1},
        };
    }
}
