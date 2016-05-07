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
            return Convert.ToString(value, 2).PadLeft(8, '0');
        }
    }
}
