using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NesCore.Processor;
using NesCore.Memory;

namespace NesCore
{
    public class Console
    {
        public Console()
        {
            Processor = new Mos6502();
            //Memory = new Memory();
        }

        public Mos6502 Processor { get; private set; }
        //public Memory Memory { get; private set; }
    }
}
