using EmuNES.Input;
using NesCore.Input;
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
    public class JoypadSettings
    {
        public JoypadSettings() { }

        public byte Port { get; set; }
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

        public string EncodeKeyboardMapping(Keys key)
        {
            return "key:" + key;
        }

        public string EncodeJoystickMapping(byte controllerId, EmuNES.Input.Button button)
        {
            return "joy:" + controllerId + ":" + button;
        }

        public ButtonPressed DecodeMapping(string mapping,
            KeyboardState keyboardState,
            GameControllerManager gameCopntrollerManager)
        {
            if (mapping.StartsWith("key:"))
                return DencodeKeyboardMapping(mapping, keyboardState);
            if (mapping.StartsWith("joy:"))
                return DencodeJoystickMapping(mapping, gameCopntrollerManager);

            return () => false;
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

        private ButtonPressed DencodeKeyboardMapping(string mapping, KeyboardState keyboardState)
        {
            Keys key = (Keys)Enum.Parse(typeof(Keys), mapping.Replace("key:", ""));
            return () => keyboardState[key];
        }

        private ButtonPressed DencodeJoystickMapping(string mapping, GameControllerManager gameControllerManager)
        {
            string[] tokens = mapping.Trim().Split(new char[] { ':' });
            byte controllerId = byte.Parse(tokens[1]);
            EmuNES.Input.Button button = (EmuNES.Input.Button)Enum.Parse(typeof(EmuNES.Input.Button), tokens[2]);
            GameController gameController = gameControllerManager[controllerId];
            return () => gameController[button];
        }
    }
}
