using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NesCore.Processing;
using NesCore.Storage;

namespace NesCore
{
    public class Console
    {
        public Console()
        {
            Processor = new Processor();
            Memory = new Memory();
        }

        public Processor Processor { get; private set; }
        public Memory Memory { get; private set; }
    }
}
