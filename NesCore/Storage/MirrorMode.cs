using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public enum MirrorMode
    {
        Single0        = 0x00,
        Diagonal       = 0x14,
        Vertical       = 0x44,
        Horizontal     = 0x50,
        LShaped        = 0x54,
        Single1        = 0x55,
        Diagonal3      = 0x94,
        Vertical3      = 0x98,
        Horizontal3    = 0xA4,
        SingleExRam    = 0xAA,
        Pseudo4        = 0xE4,
        SingleFillMode = 0xFF
    }
}
