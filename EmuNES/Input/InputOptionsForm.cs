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
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            controllerIdComboBox.SelectedIndex = controllerTypeComboBox.SelectedIndex = 0;
        }


        private void OnConfigureController(object sender, EventArgs eventArgs)
        {
            Joypad joypad = new Joypad();
            QuickConfigurationForm quickConfigurationForm = new QuickConfigurationForm(joypad, keyPressed);
            quickConfigurationForm.ShowDialog();

            this.console.ConnectController((byte)(controllerIdComboBox.SelectedIndex + 1), joypad);
        }

        private NesCore.Console console;
        private Dictionary<Keys, bool> keyPressed;

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
