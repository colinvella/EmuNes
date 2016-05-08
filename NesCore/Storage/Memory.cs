using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public class Memory: SystemBus
    {
        public Memory()
        {
        }

        public byte Read(ushort address) { return 0; }
        public void Write(ushort address, byte value) { }


        // test
    }
}
