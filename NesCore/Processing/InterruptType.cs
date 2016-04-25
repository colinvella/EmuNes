using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Processing
{
    public enum InterruptType
    {
        None,
        NonMaskable, // NMI
        Request      // IRQ
    }
}
