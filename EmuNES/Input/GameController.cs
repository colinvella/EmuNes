using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES.Input
{
    class GameController
    {      
        public GameController(byte id, int deviceId)
        {
            Id = id;
            this.deviceId = deviceId;
            joyInfoEx = new WindowsMultiMedia.JOYINFOEX();
            joyInfoEx.dwSize = Marshal.SizeOf(joyInfoEx);
            joyInfoEx.dwFlags = WindowsMultiMedia.JOY_RETURNALL;

            oldButtonState = new bool[32];
            buttonState = new bool[32];
        }

        public byte Id { get; private set; }

        public bool Left { get; private set; }
        public bool Right { get; private set; }
        public bool Up { get; private set; }
        public bool Down { get; private set; }

        public event GameControllerEventHandler ButtonPressed;

        public IReadOnlyList<bool> Buttons { get { return buttonState; } }

        public void UpdateState()
        {
            int joyX = 0;
            int joyY = 0;
            int joyButtons = 0;
   
            int result = WindowsMultiMedia.joyGetPosEx(deviceId, ref joyInfoEx);
            if (result == 0)
            {
                joyX = joyInfoEx.dwXpos;
                joyY = joyInfoEx.dwYpos;
                joyButtons = joyInfoEx.dwButtons;
            }

            // detect button changes for event dispatch
            bool oldLeft = Left;
            bool oldRight = Right;
            bool oldUp = Up;
            bool oldDown = Down;

            Left = joyX < JoyCentreMinX;
            Right = joyX > JoyCentreMaxX;
            Up = joyY < JoyCentreMinY;
            Down = joyY > JoyCentreMaxY;

            Array.Copy(Buttons.ToArray(), oldButtonState, oldButtonState.Length);
            for (int bitIndex = 0; bitIndex < 32; bitIndex++)
            {
                buttonState[bitIndex] = (joyButtons & 1) == 1;
                joyButtons >>= 1;
            }

            // if event wired, fire it on any presses
            if (ButtonPressed != null)
            {
                if (!oldLeft && Left)
                    ButtonPressed(this, new GameControllerEventArgs(Id, Button.Left));
                if (!oldRight && Right)
                    ButtonPressed(this, new GameControllerEventArgs(Id, Button.Right));
                if (!oldUp && Up)
                    ButtonPressed(this, new GameControllerEventArgs(Id, Button.Up));
                if (!oldDown && Down)
                    ButtonPressed(this, new GameControllerEventArgs(Id, Button.Down));

                for (int buttonIndex = 0; buttonIndex < 32; buttonIndex++)
                    if (!oldButtonState[buttonIndex] && buttonState[buttonIndex])
                        ButtonPressed(this, new GameControllerEventArgs(Id, Button.Button0 + buttonIndex));
            }
        }

        private int deviceId;
        private WindowsMultiMedia.JOYINFOEX joyInfoEx;
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
