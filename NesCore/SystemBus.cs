using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore
{
    public interface SystemBus
    {
        byte Read(UInt16 address);
        void Write(UInt16 address, byte value);
    }
}
