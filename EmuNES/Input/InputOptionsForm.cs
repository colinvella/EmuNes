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
using SharpNes.Settings;

namespace SharpNes.Input
{
    public partial class InputOptionsForm : Form
    {
        public InputOptionsForm(
            NesCore.Console console,
            KeyboardState keyboardState,
            MouseState mouseState,
            GameControllerManager gameControllerManager)
        {
            InitializeComponent();

            this.console = console;
            this.keyboardState = keyboardState;
            this.mouseState = mouseState;
            this.gameControllerManager = gameControllerManager;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            inputSettings = Properties.Settings.Default.InputSettings.Duplicate();

            controllerIdComboBox.SelectedIndex = 0;
        }

        private void OnPortChanged(object sender, EventArgs eventArgs)
        {
            byte port = (byte)(controllerIdComboBox.SelectedIndex + 1);
            ControllerSettings controllerSettimgs = inputSettings[port];

            if (controllerSettimgs != null)
                TypeDescriptor.AddAttributes(controllerSettimgs, new Attribute[] { new ReadOnlyAttribute(true) });
            mappingsPropertyGrid.SelectedObject = controllerSettimgs;
        }

        private void OnConfigureJoypad(object sender, EventArgs eventArgs)
        {
            byte port = (byte)(controllerIdComboBox.SelectedIndex + 1);
            JoypadSettings joypadSettings = new JoypadSettings();
            joypadSettings.Port = port;

            JoypadConfigurationForm quickConfigurationForm 
                = new JoypadConfigurationForm(joypadSettings, keyboardState, gameControllerManager);
            quickConfigurationForm.ShowDialog();

            inputSettings[port] = joypadSettings;

            OnPortChanged(sender, eventArgs);
        }

        private void OnConfigureZapper(object sender, EventArgs eventArgs)
        {
            byte port = (byte)(controllerIdComboBox.SelectedIndex + 1);
            ZapperSettings zapperSettings = new ZapperSettings();
            zapperSettings.Port = port;

            // inputs hardwired for now
            zapperSettings.Trigger = "mouse:left";
            zapperSettings.LightSense = "mouse:cursor";

            inputSettings[port] = zapperSettings;

            OnPortChanged(sender, eventArgs);
        }

        private void OnOk(object sender, EventArgs eventArgs)
        {
            Properties.Settings.Default.InputSettings = inputSettings;
            Properties.Settings.Default.Save();

            foreach (JoypadSettings joypadSettings in inputSettings.Joypads)
                console.ConnectController(joypadSettings.Port,
                    joypadSettings.ConfigureJoypad(keyboardState, gameControllerManager));

            foreach (ZapperSettings zapperSettings in inputSettings.Zappers)
                console.ConnectController(zapperSettings.Port,
                    zapperSettings.ConfigureZapper(mouseState));

            DialogResult = DialogResult.OK;
        }

        private InputSettings inputSettings;
        private NesCore.Console console;
        private KeyboardState keyboardState;
        private MouseState mouseState;
        private GameControllerManager gameControllerManager;
    }
}
