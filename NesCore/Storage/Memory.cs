using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public class Memory
    {
        public Memory(Console console)
        {
            Console = console;
        }

        public Console Console { get; private set; }

        public byte Read(ushort address) { return 0; }
        public void Write(ushort address, byte value) { }


        // test
    }
}
