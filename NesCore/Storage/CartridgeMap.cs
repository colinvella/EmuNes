using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public interface CartridgeMap
    {
        string Name { get;  }

        byte this[ushort address] { get; set; }
    }
}
