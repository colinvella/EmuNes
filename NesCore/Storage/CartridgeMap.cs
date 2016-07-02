using NesCore.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public abstract class CartridgeMap
    {
        public delegate void MirrorModeChangedHandler(MirrorMode mirrorMode);

        public delegate void BankSwitchHandler(ushort address, ushort size);

        public abstract string Name { get;  }

        public MirrorModeChangedHandler MirrorModeChanged { get; set; }

        public virtual Action TriggerInterruptRequest { get; set; }

        public virtual Action CancelInterruptRequest { get; set; }

        public BankSwitchHandler ProgramBankSwitch { get; set; }

        public BankSwitchHandler CharacterBankSwitch { get; set; }

        public abstract byte this[ushort address] { get; set; }

        public virtual void StepVideo(int scanLine, int cycle, bool showBackground, bool showSprites) { }

        public SpriteSize SpriteSize { get; set; }

        public bool AccessingSpriteCharacters { get; set; }

        public virtual byte ReadNameTableC(ushort address, byte defaultValue) { return defaultValue; }

        public virtual void WriteNameTableC(ushort address, byte value) { }

        public virtual byte ReadNameTableD(ushort address, byte defaultValue) { return defaultValue; }

        public virtual void WriteNameTableD(ushort address, byte value) { }

        public virtual byte EnhanceTileAttributes(ushort address, byte defaultTileAttriutes) { return defaultTileAttriutes; }
    }
}
