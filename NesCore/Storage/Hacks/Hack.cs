using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage.Hacks
{
    abstract class Hack
    {
        public abstract byte Read(ushort address, byte originalValue);
    }
}
