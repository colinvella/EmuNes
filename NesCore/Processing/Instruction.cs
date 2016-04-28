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
        public Instruction(byte code, String name, AddressingMode addressingMode, byte cycles, Execute execute)
        {
            Code = code;
            Name = name;
            AddressingMode = addressingMode;
            Cycles = cycles;
            Exceute = execute;

            switch (addressingMode)
            {
                // implied and accumulator modes do not require operands
                case AddressingMode.Implied:
                case AddressingMode.Accumulator:
                    Size = 1; break;
                // immediate, indexed indirect, relative and zero page variants only require a byte operand
                case AddressingMode.Immediate:
                case AddressingMode.IndexedIndirect:
                case AddressingMode.Relative:
                case AddressingMode.ZeroPage:
                case AddressingMode.ZeroPageX:
                case AddressingMode.ZeroPageY:
                    Size = 2; break;
                // absolute and indirect variants require a word operand
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteX:
                case AddressingMode.AbsoluteY:
                case AddressingMode.Indirect:
                case AddressingMode.IndirectIndexed:
                    Size = 3; break;
                default:
                    throw new ArgumentException("addressingMode");
            }
        }

        public override string ToString()
        {
            return Name + ": Mode: " + AddressingMode + ", Size: " + Size + "b, Cycles: " + Cycles;
        }

        public byte Code { get; private set; }
        public string Name { get; private set; }
        public AddressingMode AddressingMode { get; private set; }
        public byte Size { get; private set; }
        public byte Cycles { get; private set; }
        public readonly Execute Exceute;
    }
}
