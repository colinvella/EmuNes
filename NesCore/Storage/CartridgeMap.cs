using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public abstract class CartridgeMap
    {
        public abstract string Name { get;  }

        public virtual Action TriggerInterruptRequest { get; set; }

        public abstract byte this[ushort address] { get; set; }

        public virtual void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites) { }
    }
}
