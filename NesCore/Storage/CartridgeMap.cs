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

        Action TriggerInterruptRequest { get; set; }

        byte this[ushort address] { get; set; }

        void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites);
    }
}
