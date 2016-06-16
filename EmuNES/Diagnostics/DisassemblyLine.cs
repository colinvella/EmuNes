using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES.Diagnostics
{
    class DisassemblyLine
    {
        public string Label { get; set; }

        public string Address { get; set; }

        public string Instruction { get; set; }
    }
}
