using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Utility
{
    public class Hex
    {
        public static string Format(byte value)
        {
            return "$" + value.ToString("X2");
        }

        public static string Format(ushort value)
        {
            return "$" + value.ToString("X4");
        }

        public static string Format(uint value)
        {
            return "$" + value.ToString("X8");
        }

        public static string Format(ulong value)
        {
            return "$" + value.ToString("X16");
        }
    }
}
