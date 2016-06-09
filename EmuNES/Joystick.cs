using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES
{
    class Joystick
    {      
        public enum Button
        {
            None,
            Left,
            Right,
            Up,
            Down,
            Button0,
            Button1,
            Button2,
            Button3,
            Button4,
            Button5,
            Button6,
            Button7,
            Button8,
            Button9,
            Button10,
            Button11,
            Button12,
            Button13,
            Button14,
            Button15,
            Button16,
            Button17,
            Button18,
            Button19,
            Button20,
            Button21,
            Button22,
            Button23,
            Button24,
            Button25,
            Button26,
            Button27,
            Button28,
            Button29,
            Button30,
            Button31
        }

        public delegate void ButtonPressedHandler(Button button);

        public Joystick()
        {
            joyInfo = new JOYINFO();
            joyInfoEx = new JOYINFOEX();
            joyEx = false;
            joyId = 0;

            oldButtonState = new bool[32];
            buttonState = new bool[32];
            minX = minY = int.MaxValue;
            maxX = maxY = int.MinValue;

            int joystickCount = joyGetNumDevs();
            for (Int32 joystickIndex = 0; joystickIndex < joystickCount; joystickIndex++)
            {
                if (joyGetPos(joystickIndex, ref joyInfo) == 0)
                {
                    joyId = joystickIndex;
                    joyEx = false;
                }
                if (joyGetPosEx(joystickIndex, ref joyInfoEx) == 0)
                {
                    joyId = joystickIndex;
                    joyEx = true;
                }
            }
        }

        public bool Left { get; private set; }
        public bool Right { get; private set; }
        public bool Up { get; private set; }
        public bool Down { get; private set; }

        public ButtonPressedHandler ButtonPressed { get; set; }

        public IReadOnlyList<bool> Buttons { get { return buttonState; } }

        public void UpdateState()
        {
            Int32 result, i = joyId;

            int joyX = 0;
            int joyY = 0;
            int joyButtons = 0;

            if (joyEx)
            {
                result = joyGetPosEx(joyId, ref joyInfoEx);
                if (result != 0) return;

                joyX = joyInfoEx.dwXpos;
                joyY = joyInfoEx.dwYpos;
                joyButtons = joyInfoEx.dwButtons;
            }
            else
            {
                result = joyGetPos(i, ref joyInfo);
                if (result != 0) return;

                joyX = joyInfo.wXpos;
                joyY = joyInfo.wYpos;
                joyButtons = joyInfo.wButtons;
            }

            // auto calibrate
            minX = Math.Min(minX, joyInfoEx.dwXpos);
            minY = Math.Min(minY, joyInfoEx.dwYpos);
            maxX = Math.Max(maxX, joyInfoEx.dwXpos);
            maxY = Math.Max(maxY, joyInfoEx.dwYpos);

            bool oldLeft = Left;
            bool oldRight = Right;
            bool oldUp = Up;
            bool oldDown = Down;

            Left = joyX < minX / 2;
            Right = joyX > maxX / 2;
            Up = joyY < minY / 2;
            Down = joyY > maxY / 2;

            Array.Copy(Buttons.ToArray(), oldButtonState, oldButtonState.Length);
            for (int bitIndex = 0; bitIndex < 32; bitIndex++)
            {
                buttonState[bitIndex] = (joyButtons & 1) == 1;
                joyButtons >>= 1;
            }

            if (ButtonPressed != null)
            {
                if (!oldLeft && Left)
                    ButtonPressed(Button.Left);
                if (!oldRight && Right)
                    ButtonPressed(Button.Left);
                if (!oldUp && Up)
                    ButtonPressed(Button.Up);
                if (!oldDown && Down)
                    ButtonPressed(Button.Down);

                for (int buttonIndex = 0; buttonIndex < 32; buttonIndex++)
                    if (!oldButtonState[buttonIndex] && buttonState[buttonIndex])
                        ButtonPressed(Button.Button0 + buttonIndex);
            }
        }

        private JOYINFO joyInfo;
        private JOYINFOEX joyInfoEx;
        private Boolean joyEx;
        private Int32 joyId;

        private bool[] buttonState;
        private bool[] oldButtonState;
        private int minX;
        private int maxX;
        private int minY;
        private int maxY;

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

        [StructLayout(LayoutKind.Sequential)]
        private struct JOYINFO
        {
            public Int32 wXpos; // Current X-coordinate.
            public Int32 wYpos; // Current Y-coordinate.
            public Int32 wZpos; // Current Z-coordinate.
            public Int32 wButtons; // Current state of joystick buttons.
        }

        private const String WINMM_NATIVE_LIBRARY = "winmm.dll";
        private const CallingConvention CALLING_CONVENTION = CallingConvention.StdCall;

        [DllImport(WINMM_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        private static extern Int32 joyGetNumDevs();

        [DllImport(WINMM_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        private static extern Int32 joyGetPos(Int32 uJoyId, ref JOYINFO pJoyInfo);

        [DllImport(WINMM_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        private static extern Int32 joyGetPosEx(Int32 uJoyID, ref JOYINFOEX pji);
    }
}
