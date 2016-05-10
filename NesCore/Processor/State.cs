using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Processor
{
    public class State
    {
        public UInt64 Cycles { get; set; }
        public ushort StallCycles { get; set; }
        public ushort ProgramCounter { get; set; }
        public byte StackPointer { get; set; }
        public byte Accumulator { get; set; }
        public byte RegisterX { get; set; }
        public byte RegisterY { get; set; }

        public byte Flags { get; set; }

        public bool CarryFlag            { get { return (Flags & CarryMask) != 0; }   set { SetFlag(0, value); } }
        public bool ZeroFlag             { get { return (Flags & ZeroMask) != 0; }   set { SetFlag(1, value); } }
        public bool InterruptDisableFlag { get { return (Flags & InterruptDisableMask) != 0; }   set { SetFlag(2, value); } }
        public bool DecimalModeFlag      { get { return (Flags & DecimalModeMask) != 0; }   set { SetFlag(3, value); } }
        public bool BreakCommandFlag     { get { return (Flags & BreakCommandMask) != 0; }  set { SetFlag(4, value); } }
        public bool UnusedFlag           { get { return (Flags & UnusedMask) != 0; }  set { SetFlag(5, value); } }
        public bool OverflowFlag         { get { return (Flags & OverflowMask) != 0; }  set { SetFlag(6, value); } }
        public bool NegativeFlag         { get { return (Flags & NegativeMask) != 0; } set { SetFlag(7, value); } }

        public InterruptType InterruptType { get; set; }

        public const byte CarryMask = 1;
        public const byte ZeroMask = 2;
        public const byte InterruptDisableMask = 4;
        public const byte DecimalModeMask = 8;
        public const byte BreakCommandMask = 16;
        public const byte UnusedMask = 32;
        public const byte OverflowMask = 64;
        public const byte NegativeMask = 128;

        public override string ToString()
        {
            return "PC: " + Hex.Format(ProgramCounter)
                + " SP: " + Hex.Format(StackPointer)
                + " A: " + Hex.Format(Accumulator)
                + " X: " + Hex.Format(RegisterX)
                + " Y: " + Hex.Format(RegisterY)
                + " NVuBDIZC: " + Bin.Format(Flags);
        }

        private void SetFlag(int index, bool enabled)
        {
            byte mask = (byte)(1 << index);
            if (enabled)
                Flags |= mask;
            else
                Flags = (byte)(Flags & ~mask);
        }
    }
}
