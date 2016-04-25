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

        public Instruction(String name, AddressingMode addressingMode, byte cycles, Execute execute)
            : this(name, addressingMode, 0, cycles, 0, execute)
        {
            switch (addressingMode)
            {
                case AddressingMode.Implied:
                case AddressingMode.Accumulator:
                    Size = 1; break;
                case AddressingMode.Immediate:
                case AddressingMode.Relative:
                case AddressingMode.ZeroPage:
                case AddressingMode.ZeroPageX:
                case AddressingMode.ZeroPageY:
                    Size = 2; break;
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteX:
                case AddressingMode.AbsoluteY:
                case AddressingMode.IndexedIndirect:
                case AddressingMode.Indirect:
                case AddressingMode.IndirectIndexed:
                    Size = 3; break;
                default:
                    throw new ArgumentException("addressingMode");
            }
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
