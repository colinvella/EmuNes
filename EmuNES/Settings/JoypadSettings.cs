using SharpNes.Input;
using NesCore.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes.Settings
{
    [Serializable]
    public class JoypadSettings: ControllerSettings
    {
        public JoypadSettings() { }

        [Category("Joypad")] public string Start { get; set; }
        [Category("Joypad")] public string Select { get; set; }
        [Category("Joypad")] public string A { get; set; }
        [Category("Joypad")] public string B { get; set; }
        [Category("Joypad")] public string Left { get; set; }
        [Category("Joypad")] public string Right { get; set; }
        [Category("Joypad")] public string Up { get; set; }
        [Category("Joypad")] public string Down { get; set; }

        public JoypadSettings Duplicate()
        {
            JoypadSettings copy = new JoypadSettings();
            copy.Port = Port;
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

        public Joypad ConfigureJoypad(KeyboardState keyboardState, GameControllerManager gameControllerManager)
        {
            Joypad joypad = new Joypad();
            joypad.Start = DecodeMapping(Start, keyboardState, gameControllerManager);
            joypad.Select = DecodeMapping(Select, keyboardState, gameControllerManager);
            joypad.A = DecodeMapping(A, keyboardState, gameControllerManager);
            joypad.B = DecodeMapping(B, keyboardState, gameControllerManager);
            joypad.Up = DecodeMapping(Up, keyboardState, gameControllerManager);
            joypad.Down = DecodeMapping(Down, keyboardState, gameControllerManager);
            joypad.Left = DecodeMapping(Left, keyboardState, gameControllerManager);
            joypad.Right = DecodeMapping(Right, keyboardState, gameControllerManager);
            return joypad;
        }
    }
}
