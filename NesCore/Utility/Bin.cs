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

        public static string Format(ushort value)
        {
            return "%" + Convert.ToString(value, 2).PadLeft(16, '0');
        }

        public const byte Bit0 = 0x01;
        public const byte Bit1 = 0x02;
        public const byte Bit2 = 0x04;
        public const byte Bit3 = 0x08;
        public const byte Bit4 = 0x10;
        public const byte Bit5 = 0x20;
        public const byte Bit6 = 0x40;
        public const byte Bit7 = 0x80;

        public const byte B00000001 = 0x01;
        public const byte B00000011 = 0x03;
        public const byte B00000111 = 0x07;
        public const byte B00001111 = 0x0F;
        public const byte B00011111 = 0x1F;
        public const byte B00111111 = 0x3F;
        public const byte B01111111 = 0x7F;
        public const byte B11111111 = 0xFF;

        public const byte Nybble0 = 0x0F;
        public const byte Nybble1 = 0xF0;
    }
}
