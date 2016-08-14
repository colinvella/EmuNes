using SharpNes.Settings;
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

namespace SharpNes.Input
{
    public partial class JoypadConfigurationForm : Form
    {
        public JoypadConfigurationForm(JoypadSettings joypadSettings,
            KeyboardState keyboardState,
            GameControllerManager gameControllerManager)
        {
            InitializeComponent();

            this.joypadSettings = joypadSettings;
            this.joypadConfigState = JoypadConfigState.Start;
            this.keyboardState = keyboardState;
            this.gameControllerManager = gameControllerManager;
            this.configurationLabel.Text = "Press Start";

            foreach (GameController gameController in gameControllerManager.Controllers)
                gameController.ButtonPressed += OnControllerButtonPressed;
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventargs)
        {
            Keys keyCode = keyEventargs.KeyCode;
            switch (joypadConfigState)
            {
                case JoypadConfigState.Start:
                    joypadSettings.Start = joypadSettings.EncodeKeyboardMapping(keyCode);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Select:
                    joypadSettings.Select = joypadSettings.EncodeKeyboardMapping(keyCode);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.A:
                    joypadSettings.A = joypadSettings.EncodeKeyboardMapping(keyCode);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.B:
                    joypadSettings.B = joypadSettings.EncodeKeyboardMapping(keyCode);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Up:
                    joypadSettings.Up = joypadSettings.EncodeKeyboardMapping(keyCode);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Down:
                    joypadSettings.Down = joypadSettings.EncodeKeyboardMapping(keyCode);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Left:
                    joypadSettings.Left = joypadSettings.EncodeKeyboardMapping(keyCode);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Right:
                    joypadSettings.Right = joypadSettings.EncodeKeyboardMapping(keyCode);

                    foreach (GameController gameController in gameControllerManager.Controllers)
                        gameController.ButtonPressed -= OnControllerButtonPressed;

                    Close();
                    break;
            }
        }

        private void OnControllerButtonPressed(object sender, GameControllerEventArgs gameControllerEventArgs)
        {
            GameController.Button button = gameControllerEventArgs.Button;
            GameController gameController = (GameController)sender;
            switch (joypadConfigState)
            {
                case JoypadConfigState.Start:
                    joypadSettings.Start = joypadSettings.EncodeJoystickMapping(gameController.Id, button);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Select:
                    joypadSettings.Select = joypadSettings.EncodeJoystickMapping(gameController.Id, button);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.A:
                    joypadSettings.A = joypadSettings.EncodeJoystickMapping(gameController.Id, button);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.B:
                    joypadSettings.B = joypadSettings.EncodeJoystickMapping(gameController.Id, button);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Up:
                    joypadSettings.Up = joypadSettings.EncodeJoystickMapping(gameController.Id, button);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Down:
                    joypadSettings.Down = joypadSettings.EncodeJoystickMapping(gameController.Id, button);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Left:
                    joypadSettings.Left = joypadSettings.EncodeJoystickMapping(gameController.Id, button);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Right:
                    joypadSettings.Right = joypadSettings.EncodeJoystickMapping(gameController.Id, button);

                    foreach (GameController gameController2 in gameControllerManager.Controllers)
                        gameController2.ButtonPressed -= OnControllerButtonPressed;

                    Close();
                    break;
            }
        }

        private JoypadSettings joypadSettings;
        private KeyboardState keyboardState;
        private GameControllerManager gameControllerManager;
        private JoypadConfigState joypadConfigState;

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

    }
}
