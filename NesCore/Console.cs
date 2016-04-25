using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NesCore.Processing;
using NesCore.Addressing;

namespace NesCore
{
    public class Console
    {
        public Console()
        {
            Processor = new Processor(this);
            Memory = new Memory(this);
        }

        public Processor Processor { get; private set; }
        public Memory Memory { get; private set; }
    }
}
