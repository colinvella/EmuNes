using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore
{
    public interface SystemBus
    {
        byte Read(ushort address);
        void Write(ushort address, byte value);
    }
}
