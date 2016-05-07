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

        public static string Format(UInt16 value)
        {
            return "$" + value.ToString("X4");
        }
    }
}
