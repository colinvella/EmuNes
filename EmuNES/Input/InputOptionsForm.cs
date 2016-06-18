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
using EmuNES.Settings;

namespace EmuNES.Input
{
    public partial class InputOptionsForm : Form
    {
        public InputOptionsForm(
            NesCore.Console console,
            KeyboardState keyboardState,
            GameControllerManager gameControllerManager)
        {
            InitializeComponent();

            this.console = console;
            this.keyboardState = keyboardState;
            this.gameControllerManager = gameControllerManager;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            inputSettings = Properties.Settings.Default.InputSettings.Duplicate();

            controllerIdComboBox.SelectedIndex = controllerTypeComboBox.SelectedIndex = 0;
        }

        private void OnConfigureController(object sender, EventArgs eventArgs)
        {
            JoypadSettings joypadSettings = inputSettings.Joypads[controllerIdComboBox.SelectedIndex];

            JoypadConfigurationForm quickConfigurationForm 
                = new JoypadConfigurationForm(joypadSettings, keyboardState, gameControllerManager);
            quickConfigurationForm.ShowDialog();
        }

        private void OnOk(object sender, EventArgs eventArgs)
        {
            Properties.Settings.Default.InputSettings = inputSettings;
            Properties.Settings.Default.Save();

            foreach (JoypadSettings joypadSettings in inputSettings.Joypads)
                console.ConnectController(joypadSettings.Port,
                    joypadSettings.ConfigureJoypad(keyboardState, gameControllerManager));

            DialogResult = DialogResult.OK;
        }

        private InputSettings inputSettings;
        private NesCore.Console console;
        private KeyboardState keyboardState;
        private GameControllerManager gameControllerManager;
    }
}
