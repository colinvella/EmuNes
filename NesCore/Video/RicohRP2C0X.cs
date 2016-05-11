using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Video
{
    public class RicohRP2C0X
    {
        public RicohRP2C0X()
        {
            Registers = new Registers();
        }

        public Registers Registers { get; private set; }


    }
}
