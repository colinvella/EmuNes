using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES.Diagnostics
{
    struct DisassemblyLine
    {
        public string Address { get; set; }

        public byte OpCode { get; set; }

        public ushort Operand { get; set; }

        public string Instruction { get; set; }
    }
}
