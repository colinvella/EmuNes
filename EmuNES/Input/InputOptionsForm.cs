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
        public InputOptionsForm(NesCore.Console console, KeyboardState keyboardState)
        {
            InitializeComponent();

            this.console = console;
            this.controllers = new Controller[4];
            this.keyboardState = keyboardState;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            controllerIdComboBox.SelectedIndex = controllerTypeComboBox.SelectedIndex = 0;
        }

        private void OnConfigureController(object sender, EventArgs eventArgs)
        {
            Joypad joypad = new Joypad();
            QuickConfigurationForm quickConfigurationForm = new QuickConfigurationForm(joypad, keyboardState);
            quickConfigurationForm.ShowDialog();
            controllers[controllerTypeComboBox.SelectedIndex] = joypad;
        }

        private void OnOk(object sender, EventArgs eventArgs)
        {
            for (int controllerIndex = 0; controllerIndex < 4; controllerIndex++)
            {
                Controller controller = controllers[controllerIndex];
                if (controller != null)
                    this.console.ConnectController((byte)(controllerIndex + 1), controller);
            }
            DialogResult = DialogResult.OK;
        }

        private NesCore.Console console;
        private KeyboardState keyboardState;

        private Controller[] controllers;
    }
}
