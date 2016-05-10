using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Processor
{
    public enum InterruptType
    {
        None,
        NonMaskable, // NMI
        Request      // IRQ
    }
}
