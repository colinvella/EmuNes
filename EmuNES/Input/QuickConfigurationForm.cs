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
        public QuickConfigurationForm(Joypad joypad, Dictionary<Keys, bool> keyPressed)
        {
            InitializeComponent();

            this.joypad = joypad;
            this.joypadConfigState = JoypadConfigState.Start;
            this.keyPressed = keyPressed;
            this.configurationLabel.Text = "Press Start";
        }

        private Dictionary<Keys, bool> keyPressed;
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
                    joypad.Start = () => keyPressed[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Select:
                    joypad.Select = () => keyPressed[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.A:
                    joypad.A = () => keyPressed[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.B:
                    joypad.B = () => keyPressed[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Up:
                    joypad.Up = () => keyPressed[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Down:
                    joypad.Down = () => keyPressed[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Left:
                    joypad.Left = () => keyPressed[keyEventargs.KeyCode];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Right:
                    joypad.Right = () => keyPressed[keyEventargs.KeyCode];
                    Close();
                    break;
            }
        }
    }
}
