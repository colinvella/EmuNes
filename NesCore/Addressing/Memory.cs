using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Addressing
{
    public class Memory
    {
        public Memory(Console console)
        {
            Console = console;
        }

        public Console Console { get; private set; }

        public byte Read(UInt16 address) { return 0; }
        public void Write(UInt16 address, byte value) { }


        // test
    }
}
