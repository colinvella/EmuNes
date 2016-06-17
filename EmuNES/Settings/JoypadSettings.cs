using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES.Settings
{
    [Serializable]
    public class JoypadSettings
    {
        public JoypadSettings() { }

        public byte Id { get; set; }
        public string Start { get; set; }
        public string Select { get; set; }
        public string A { get; set; }
        public string B { get; set; }
        public string Left { get; set; }
        public string Right { get; set; }
        public string Up { get; set; }
        public string Down { get; set; }

        public JoypadSettings Duplicate()
        {
            JoypadSettings copy = new JoypadSettings();
            copy.Id = Id;
            copy.Start = Start;
            copy.Select = Select;
            copy.A = A;
            copy.B = B;
            copy.Left = Left;
            copy.Right = Right;
            copy.Up = Up;
            copy.Down = Down;
            return copy;
        }
    }
}
