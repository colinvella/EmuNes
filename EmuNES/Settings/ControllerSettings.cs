using NesCore.Input;
using SharpNes.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes.Settings
{
    [Serializable]
    public class ControllerSettings
    {
        [Browsable(false)]
        public byte Port { get; set; }

        public string EncodeKeyboardMapping(Keys key)
        {
            return "key:" + key;
        }

        public string EncodeJoystickMapping(byte controllerId, SharpNes.Input.GameController.Button button)
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

        private ButtonPressed DencodeKeyboardMapping(string mapping, KeyboardState keyboardState)
        {
            Keys key = (Keys)Enum.Parse(typeof(Keys), mapping.Replace("key:", ""));
            return () => keyboardState[key];
        }

        private ButtonPressed DencodeJoystickMapping(string mapping, GameControllerManager gameControllerManager)
        {
            string[] tokens = mapping.Trim().Split(new char[] { ':' });
            byte controllerId = byte.Parse(tokens[1]);
            GameController.Button button = (GameController.Button)Enum.Parse(typeof(GameController.Button), tokens[2]);
            if (controllerId < gameControllerManager.Count)
            {
                GameController gameController = gameControllerManager[controllerId];
                return () => gameController[button];
            }
            else
            {
                // may have been unplugged
                return () => false;
            }
        }
    }
}
