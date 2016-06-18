using EmuNES.Settings;
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
            Keys keyCoke = keyEventargs.KeyCode;
            switch (joypadConfigState)
            {
                case JoypadConfigState.Start:
                    joypadSettings.Start = joypadSettings.EncodeKeyboardMapping(keyCoke);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Select:
                    joypadSettings.Select = joypadSettings.EncodeKeyboardMapping(keyCoke);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.A:
                    joypadSettings.A = joypadSettings.EncodeKeyboardMapping(keyCoke);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.B:
                    joypadSettings.B = joypadSettings.EncodeKeyboardMapping(keyCoke);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Up:
                    joypadSettings.Up = joypadSettings.EncodeKeyboardMapping(keyCoke);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Down:
                    joypadSettings.Down = joypadSettings.EncodeKeyboardMapping(keyCoke);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Left:
                    joypadSettings.Left = joypadSettings.EncodeKeyboardMapping(keyCoke);
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Right:
                    joypadSettings.Right = joypadSettings.EncodeKeyboardMapping(keyCoke);

                    foreach (GameController gameController in gameControllerManager.Controllers)
                        gameController.ButtonPressed -= OnControllerButtonPressed;

                    Close();
                    break;
            }
        }

        private void OnControllerButtonPressed(object sender, GameControllerEventArgs gameControllerEventArgs)
        {
            /*
            GameController gameController = (GameController)sender;
            switch (joypadConfigState)
            {
                case JoypadConfigState.Start:
                    joypad.Start = () => gameController[gameControllerEventArgs.Button];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Select:
                    joypad.Select = () => gameController[gameControllerEventArgs.Button];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.A:
                    joypad.A = () => gameController[gameControllerEventArgs.Button];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.B:
                    joypad.B = () => gameController[gameControllerEventArgs.Button];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Up:
                    joypad.Up = () => gameController[gameControllerEventArgs.Button];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Down:
                    joypad.Down = () => gameController[gameControllerEventArgs.Button];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Left:
                    joypad.Left = () => gameController[gameControllerEventArgs.Button];
                    ++joypadConfigState;
                    configurationLabel.Text = "Press " + joypadConfigState;
                    break;
                case JoypadConfigState.Right:
                    joypad.Right = () => gameController[gameControllerEventArgs.Button];

                    foreach (GameController gameController2 in gameControllerManager.Controllers)
                        gameController2.ButtonPressed -= OnControllerButtonPressed;

                    Close();
                    break;
            }*/
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
