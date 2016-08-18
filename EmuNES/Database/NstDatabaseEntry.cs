using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Database
{
    class NstDatabaseEntry
    {
        public NstDatabaseEntry(string crc, byte mapperId, IReadOnlyCollection<Peripheral> peripherals)
        {
            Crc = crc;
            MapperId = mapperId;
            Peripherals = new List<Peripheral>(peripherals);
        }

        public string Crc { get; private set; }
        public byte MapperId { get; private set; }
        public IReadOnlyCollection<Peripheral> Peripherals { get; private set; }
    }
}
