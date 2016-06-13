using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NesCore;
using NesCore.Input;

namespace EmuNES.Input
{
    public partial class InputOptionsForm : Form
    {
        public InputOptionsForm(NesCore.Console console, Dictionary<Keys, bool> keyPressed)
        {
            InitializeComponent();

            this.console = console;
            this.keyPressed = keyPressed;
            this.newJoypad = null;
        }

        private void OnConfigureController(object sender, EventArgs eventArgs)
        {
            if (joypadConfigState == JoypadConfigState.Ready)
            {
                joypadConfigState = JoypadConfigState.Start;
                configureOneButton.Text = "&Cancel";
                configureTextbox.Text = "Press " + joypadConfigState;
                configureTextbox.Focus();
                this.newJoypad = new Joypad();
            }
            else
            {
                joypadConfigState = JoypadConfigState.Ready;
                configureOneButton.Text = "&Configure...";
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            switch (joypadConfigState)
            {
                case JoypadConfigState.Start:
                    newJoypad.Start = () => keyPressed[keyEventArgs.KeyCode];
                    ++joypadConfigState;
                    configureTextbox.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Select:
                    newJoypad.Select = () => keyPressed[keyEventArgs.KeyCode];
                    ++joypadConfigState;
                    configureTextbox.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.A:
                    newJoypad.A = () => keyPressed[keyEventArgs.KeyCode];
                    ++joypadConfigState;
                    configureTextbox.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.B:
                    newJoypad.B = () => keyPressed[keyEventArgs.KeyCode];
                    ++joypadConfigState;
                    configureTextbox.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Up:
                    newJoypad.Up = () => keyPressed[keyEventArgs.KeyCode];
                    ++joypadConfigState;
                    configureTextbox.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Down:
                    newJoypad.Down = () => keyPressed[keyEventArgs.KeyCode];
                    ++joypadConfigState;
                    configureTextbox.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Left:
                    newJoypad.Left = () => keyPressed[keyEventArgs.KeyCode];
                    ++joypadConfigState;
                    configureTextbox.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Right:
                    newJoypad.Right = () => keyPressed[keyEventArgs.KeyCode];
                    console.ConnectController((byte)(tabControl.SelectedIndex + 1), newJoypad);
                    newJoypad = null;
                    joypadConfigState = JoypadConfigState.Ready;
                    configureOneButton.Text = "&Configure";
                    configureTextbox.Text = "Done";
                    break;
            }
        }

        private NesCore.Console console;
        private Dictionary<Keys, bool> keyPressed;
        private JoypadConfigState joypadConfigState = JoypadConfigState.Ready;
        private Joypad newJoypad;

        private enum  JoypadConfigState
        {
            Ready,
            Start,
            Select,
            A,
            B,
            Up,
            Down,
            Left,
            Right,
        }

    }
}
