using NesCore.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmuNES.Input
{
    public partial class QuickConfigurationForm : Form
    {
        public QuickConfigurationForm(Joypad joypad, KeyboardState keyboardState)
        {
            InitializeComponent();

            this.joypad = joypad;
            this.joypadConfigState = JoypadConfigState.Start;
            this.keyboardState = keyboardState;
            this.configurationLabel.Text = "Press Start";
        }

        private KeyboardState keyboardState;
        private JoypadConfigState joypadConfigState;
        private Joypad joypad;

        private enum JoypadConfigState
        {
            Start,
            Select,
            A,
            B,
            Up,
            Down,
            Left,
            Right,
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventargs)
        {
            switch (joypadConfigState)
            {
                case JoypadConfigState.Start:
                    joypad.Start = () => keyboardState[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Select:
                    joypad.Select = () => keyboardState[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.A:
                    joypad.A = () => keyboardState[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.B:
                    joypad.B = () => keyboardState[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Up:
                    joypad.Up = () => keyboardState[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Down:
                    joypad.Down = () => keyboardState[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Left:
                    joypad.Left = () => keyboardState[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Right:
                    joypad.Right = () => keyboardState[keyEventargs.KeyCode];
                    Close();
                    break;
            }
        }
    }
}
