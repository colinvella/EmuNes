using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmuNES.Settings
{
    [Serializable]
    public class InputSettings
    {
        public InputSettings()
        {
            Joypads = new List<JoypadSettings>();
        }

        public void BuildDefaultSettings()
        {
            Joypads.Clear();
            JoypadSettings joypadSettings = new JoypadSettings();
            joypadSettings.Port = 1;
            joypadSettings.Start = joypadSettings.EncodeKeyboardMapping(Keys.Enter);
            joypadSettings.Select = joypadSettings.EncodeKeyboardMapping(Keys.Tab);
            joypadSettings.A = joypadSettings.EncodeKeyboardMapping(Keys.Z);
            joypadSettings.B = joypadSettings.EncodeKeyboardMapping(Keys.X);
            joypadSettings.Up = joypadSettings.EncodeKeyboardMapping(Keys.Up);
            joypadSettings.Down = joypadSettings.EncodeKeyboardMapping(Keys.Down);
            joypadSettings.Left = joypadSettings.EncodeKeyboardMapping(Keys.Left);
            joypadSettings.Right = joypadSettings.EncodeKeyboardMapping(Keys.Right);
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
