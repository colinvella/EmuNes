using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES.Diagnostics
{
    class DisassemblyLine
    {
        
        public string Label { get; set; }

        public string Address { get; set; }

        [DisplayName("M Code")]
        public string MachineCode { get; set; }

        public string Source { get; set; }

        public string Remarks { get; set; }
    }
}
