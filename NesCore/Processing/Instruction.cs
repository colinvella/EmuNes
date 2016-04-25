using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Processing
{
    public delegate void Execute(UInt16 address, AddressingMode mode);

    public class Instruction
    {
        public Instruction(String name, AddressingMode addressingMode, byte size, byte cycles, byte pageCycles, Execute execute)
        {
            Name = name;
            AddressingMode = addressingMode;
            Size = size;
            Cycles = cycles;
            PageCycles = pageCycles;
            Exceute = execute;
        }

        public Instruction(String name, AddressingMode addressingMode, byte size, byte cycles, Execute execute)
            : this(name, addressingMode, size, cycles, 0, execute)
        {
        }

        public override string ToString()
        {
            return Name + ": Mode: " + AddressingMode + ", Size: " + Size + "b, Cycles: " + Cycles + "/" + PageCycles;
        }

        public string Name { get; private set; }
        public AddressingMode AddressingMode { get; private set; }
        public byte Size { get; private set; }
        public byte Cycles { get; private set; }
        public byte PageCycles { get; private set; }
        public readonly Execute Exceute;
    }
}
