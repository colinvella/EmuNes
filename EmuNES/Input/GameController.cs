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
        public GameController(byte id)
        {
            Id = id;
            joyInfoEx = new JOYINFOEX();
            joyInfoEx.dwSize = Marshal.SizeOf(joyInfoEx);
            joyInfoEx.dwFlags = JOY_RETURNALL;

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
   
            int result = joyGetPosEx(Id, ref joyInfoEx);
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

        private JOYINFOEX joyInfoEx;

        private bool[] buttonState;
        private bool[] oldButtonState;

        [StructLayout(LayoutKind.Sequential)]
        private struct JOYINFOEX
        {
            public Int32 dwSize; // Size, in bytes, of this structure.
            public Int32 dwFlags; // Flags indicating the valid information returned in this structure.
            public Int32 dwXpos; // Current X-coordinate.
            public Int32 dwYpos; // Current Y-coordinate.
            public Int32 dwZpos; // Current Z-coordinate.
            public Int32 dwRpos; // Current position of the rudder or fourth joystick axis.
            public Int32 dwUpos; // Current fifth axis position.
            public Int32 dwVpos; // Current sixth axis position.
            public Int32 dwButtons; // Current state of the 32 joystick buttons (bits)
            public Int32 dwButtonNumber; // Current button number that is pressed.
            public Int32 dwPOV; // Current position of the point-of-view control (0..35,900, deg*100)
            public Int32 dwReserved1; // Reserved; do not use.
            public Int32 dwReserved2; // Reserved; do not use.
        }

        private const int JoyMinX = 0;
        private const int JoyMinY = 0;
        private const int JoyMaxX = ushort.MaxValue;
        private const int JoyMaxY = JoyMaxX;
        private const int JoyCentreMinX = ushort.MaxValue / 4;
        private const int JoyCentreMinY = JoyCentreMinX;
        private const int JoyCentreMaxX = ushort.MaxValue * 3 / 4;
        private const int JoyCentreMaxY = JoyCentreMaxX;

        private const String WINMM_NATIVE_LIBRARY = "winmm.dll";
        private const CallingConvention CALLING_CONVENTION = CallingConvention.StdCall;

        [DllImport(WINMM_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        private static extern Int32 joyGetPosEx(Int32 uJoyID, ref JOYINFOEX pji);

        private const int JOY_RETURNBUTTONS = 0x80;
        private const int JOY_RETURNY = 0x2;
        private const int JOY_RETURNX = 0x1;
        private const int JOY_RETURNPOV = 0x40;
        private const int JOY_RETURNR = 0x8;
        private const int JOY_RETURNU = 0x10;
        private const int JOY_RETURNV = 0x20;
        private const int JOY_RETURNZ = 0x4;
        private const int JOY_RETURNALL = JOY_RETURNX | JOY_RETURNY | JOY_RETURNZ | JOY_RETURNR | JOY_RETURNU | JOY_RETURNV | JOY_RETURNPOV | JOY_RETURNBUTTONS;
    }
}
