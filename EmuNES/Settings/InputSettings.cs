using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES.Settings
{
    [Serializable]
    public class InputSettings
    {
        public InputSettings()
        {
            Joypads = new List<JoypadSettings>();
        }

        public List<JoypadSettings> Joypads { get; private set; }

        public InputSettings Duplicate()
        {
            InputSettings copy = new InputSettings();
            foreach (JoypadSettings joypad in Joypads)
                copy.Joypads.Add(joypad.Duplicate());
            return copy;
        }
    }
}
