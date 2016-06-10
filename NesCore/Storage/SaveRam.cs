using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public class SaveRam
    {
        public SaveRam()
        {
            saveRam = new byte[0x2000];
        }

        public bool Modified { get; private set; }

        public byte this[ushort address]
        {
            get { return saveRam[address % 0x2000]; }
            set
            {
                saveRam[address % 0x2000] = value;
                Modified = true;
            }
        }

        public void Load(BinaryReader binaryReader)
        {
            saveRam = binaryReader.ReadBytes(0X2000);
        }

        public void Save(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(saveRam);
        }

        private byte[] saveRam;
    }
}
