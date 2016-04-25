using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Addressing
{
    public class Memory
    {
        public const UInt16 ResetVector = 0xFFFC;
        public const UInt16 IrqVector = 0xFFFE;

        public Memory(Console console)
        {
            Console = console;
        }

        public Console Console { get; private set; }

        // returns true if address pages differ (differ by high byte)
        public bool PagesDiffer(UInt16 addressOne, UInt16 addressTwo)
        {
            return (addressOne & 0xFF00) != (addressTwo & 0xFF00);
        }

        public byte Read(UInt16 address) { return 0; }
        public void Write(UInt16 address, byte value) { }

        public UInt16 Read16(UInt16 address) { return 0; }

        public UInt16 Read16Bug(UInt16 address) { return 0; }

    }
}
