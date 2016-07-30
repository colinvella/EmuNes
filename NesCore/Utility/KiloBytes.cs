using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Utility
{
    public class KiloBytes
    {
        public static string Format(int bytes)
        {
            return ((bytes + 512) / 1024) + "Kb";
        }
    }
}
