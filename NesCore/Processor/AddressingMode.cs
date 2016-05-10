using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Processor
{
    public enum AddressingMode
    {
        Undefined,
        Absolute,
        AbsoluteX,
        AbsoluteY,
        Accumulator,
        Immediate,
        Implied,
        IndexedIndirect,
        Indirect,
        IndirectIndexed,
        Relative,
        ZeroPage,
        ZeroPageX,
        ZeroPageY
    }
}
