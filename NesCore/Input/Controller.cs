using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Input
{
    public delegate bool ButtonPressed();

    public interface Controller
    {
        bool IsSerial { get; }
        void Strobe();
        bool ReadSerial();
        byte PortValue { get; }
    }
}
