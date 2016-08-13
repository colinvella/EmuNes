using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Input
{
    public class ControllerMultiplexor
    {
        public ControllerMultiplexor()
        {
        }

        public byte Port
        {
            get
            {
                byte value = 0x40;

                if (Primary != null)
                {
                    if (Primary.IsSerial && Primary.ReadSerial())
                        value |= 0x01;

                    if (!Primary.IsSerial) // such as zapper Nes/Famicon version
                        value |= Primary.PortValue;
                }

                if (Secondary != null && Secondary.IsSerial && Secondary.ReadSerial())
                    value |= 0x02;

                return value;
            }
            set
            {
                if ((value & 0x01) == 0x01)
                {
                    if (Primary != null)
                        Primary.Strobe();
                    if (Secondary != null)
                        Secondary.Strobe();
                }
            }
        }

        public Controller Primary { get; set; }
        public Controller Secondary { get; set; }
    }
}
