using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Utility
{
    public class Bin
    {
        public static string Format(byte value)
        {
            return "%" + Convert.ToString(value, 2).PadLeft(8, '0');
        }

        public const byte Bit0 = 0x01;
        public const byte Bit1 = 0x02;
        public const byte Bit2 = 0x04;
        public const byte Bit3 = 0x08;
        public const byte Bit4 = 0x10;
        public const byte Bit5 = 0x20;
        public const byte Bit6 = 0x40;
        public const byte Bit7 = 0x80;
    }
}
