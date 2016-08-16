using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Database
{
    class NstDatabaseEntry
    {
        public NstDatabaseEntry(string crc, byte mapperId)
        {
            Crc = crc;
            MapperId = mapperId;
        }

        public string Crc { get; private set; }
        public byte MapperId { get; private set; }
    }
}
