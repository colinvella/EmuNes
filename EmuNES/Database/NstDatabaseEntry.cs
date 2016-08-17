using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Database
{
    class NstDatabaseEntry
    {
        public NstDatabaseEntry(string crc, byte mapperId, string inputDevice)
        {
            Crc = crc;
            MapperId = mapperId;
            InputDevice = inputDevice;
        }

        public string Crc { get; private set; }
        public byte MapperId { get; private set; }
        public string InputDevice { get; private set; }
    }
}
