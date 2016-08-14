using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Input
{
    public class Zapper: Controller
    {
        public Zapper()
        {
            LightSense = Trigger = () => false;
        }

        public ButtonPressed LightSense { get; set; }
        public ButtonPressed Trigger { get; set; }

        public bool IsSerial { get { return false; } }

        public void Strobe()
        {
        }

        public bool ReadSerial()
        {
            return false;
        }

        public byte PortValue
        {
            get
            {
                // ...T L...
                byte value = 0x00;
                if (!LightSense())
                    value |= 0x08;
                if (Trigger())
                    value |= 0x10;
                return value;
            }
        }
    }
}
