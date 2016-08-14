using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Input
{
    public class GameController
    {
        public enum Button
        {
            Left, Right, Up, Down,
            Button0, Button1, Button2, Button3, Button4, Button5, Button6, Button7,
            Button8, Button9, Button10, Button11, Button12, Button13, Button14, Button15,
            Button16, Button17, Button18, Button19, Button20, Button21, Button22, Button23,
            Button24, Button25, Button26, Button27, Button28, Button29, Button30, Button31
        }

        public GameController(byte id, int deviceId)
        {
            Id = id;
            this.deviceId = deviceId;
            joyInfoEx = new WindowsMultiMedia.JOYINFOEX();
            joyInfoEx.dwSize = Marshal.SizeOf(joyInfoEx);
            joyInfoEx.dwFlags = WindowsMultiMedia.JOY_RETURNALL;

            buttonState = new bool[36];
            oldButtonState = new bool[36];

            fireButtonState = new bool[32];
        }

        public byte Id { get; private set; }

        public bool Left { get; private set; }
        public bool Right { get; private set; }
        public bool Up { get; private set; }
        public bool Down { get; private set; }

        public IReadOnlyList<bool> FireButtons { get { return fireButtonState; } }

        public bool this[Button button] { get { return buttonState[(int)button]; } }

        public event GameControllerEventHandler ButtonPressed;
        public event GameControllerEventHandler ButtonReleased;

        public void UpdateState()
        {
            int joyX = 0;
            int joyY = 0;
            int joyButtons = 0;
   
            if (WindowsMultiMedia.GetJoystickState(deviceId, ref joyInfoEx) == WindowsMultiMedia.JOYERR_NOERROR)
            {
                joyX = joyInfoEx.dwXpos;
                joyY = joyInfoEx.dwYpos;
                joyButtons = joyInfoEx.dwButtons;
            }

            // detect button changes for event dispatch
            Left = joyX < JoyCentreMinX;
            Right = joyX > JoyCentreMaxX;
            Up = joyY < JoyCentreMinY;
            Down = joyY > JoyCentreMaxY;

            buttonState[(int)Button.Left] = Left;
            buttonState[(int)Button.Right] = Right;
            buttonState[(int)Button.Up] = Up;
            buttonState[(int)Button.Down] = Down;


            for (int bitIndex = 0; bitIndex < 32; bitIndex++)
            {
                buttonState[4 + bitIndex] = (joyButtons & 1) == 1;
                joyButtons >>= 1;
            }

            // copy fire button part of button state into fire button state array
            Array.Copy(buttonState, 4, fireButtonState, 0, fireButtonState.Length);

            // if ButtonPressed event wired, fire it on any presses
            if (ButtonPressed != null)
            {
                for (int index = 0; index < buttonState.Length; index++)
                    if (buttonState[index] && !oldButtonState[index])
                        ButtonPressed(this, new GameControllerEventArgs(Id, (GameController.Button)index));
            }

            // if ButtonReleased event wired, fire it on any input releases
            if (ButtonReleased != null)
            {
                for (int index = 0; index < buttonState.Length; index++)
                    if (!buttonState[index] && oldButtonState[index])
                        ButtonReleased(this, new GameControllerEventArgs(Id, (GameController.Button)index));
            }

            // copy button states to detect changes in next iteration
            Array.Copy(buttonState, oldButtonState, buttonState.Length);
        }

        private int deviceId;
        private WindowsMultiMedia.JOYINFOEX joyInfoEx;
        private bool[] fireButtonState;
        private bool[] buttonState;
        private bool[] oldButtonState;

        private const int JoyMinX = 0;
        private const int JoyMinY = 0;
        private const int JoyMaxX = ushort.MaxValue;
        private const int JoyMaxY = JoyMaxX;
        private const int JoyCentreMinX = ushort.MaxValue / 4;
        private const int JoyCentreMinY = JoyCentreMinX;
        private const int JoyCentreMaxX = ushort.MaxValue * 3 / 4;
        private const int JoyCentreMaxY = JoyCentreMaxX;
    }
}
