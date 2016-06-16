using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public abstract class CartridgeMap
    {
        public delegate void BankSwitchHandler(ushort address, ushort size);

        public abstract string Name { get;  }

        public virtual Action TriggerInterruptRequest { get; set; }

        public BankSwitchHandler ProgramBankSwitch { get; set; }

        public BankSwitchHandler CharacterBankSwitch { get; set; }

        public abstract byte this[ushort address] { get; set; }

        public virtual void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites) { }
    }
}
