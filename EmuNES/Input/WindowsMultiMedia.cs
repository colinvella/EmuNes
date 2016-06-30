using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Input
{
    class WindowsMultiMedia
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct JOYCAPS
        {
            public UInt16 wMid;
            public UInt16 wPid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public Int32 wXmin;
            public Int32 wXmax;
            public Int32 wYmin;
            public Int32 wYmax;
            public Int32 wZmin;
            public Int32 wZmax;
            public Int32 wNumButtons;
            public Int32 wPeriodMin;
            public Int32 wPeriodMax;
            public Int32 wRmin;
            public Int32 wRmax;
            public Int32 wUmin;
            public Int32 wUmax;
            public Int32 wVmin;
            public Int32 wVmax;
            public Int32 wCaps;
            public Int32 wMaxAxes;
            public Int32 wNumAxes;
            public Int32 wMaxButtons;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szRegKey;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szOEMVxD;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JOYINFOEX
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

        [DllImport(WINMM_NATIVE_LIBRARY, EntryPoint = "joyGetNumDevs", CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public static extern Int32 GetJoystickDeviceCount();

        [DllImport(WINMM_NATIVE_LIBRARY, EntryPoint = "joyGetDevCaps", CallingConvention = CALLING_CONVENTION)]
        private static extern uint GetJoystickDeviceCapabilities(uint id, out JOYCAPS caps, int cbjc);

        [DllImport(WINMM_NATIVE_LIBRARY, EntryPoint = "joyGetPosEx", CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public static extern Int32 GetJoystickState(Int32 uJoyID, ref JOYINFOEX pji);

        public const int JOYERR_NOERROR = 0;
        public const int JOY_RETURNBUTTONS = 0x80;
        public const int JOY_RETURNY = 0x2;
        public const int JOY_RETURNX = 0x1;
        public const int JOY_RETURNPOV = 0x40;
        public const int JOY_RETURNR = 0x8;
        public const int JOY_RETURNU = 0x10;
        public const int JOY_RETURNV = 0x20;
        public const int JOY_RETURNZ = 0x4;
        public const int JOY_RETURNALL = JOY_RETURNX | JOY_RETURNY | JOY_RETURNZ | JOY_RETURNR | JOY_RETURNU | JOY_RETURNV | JOY_RETURNPOV | JOY_RETURNBUTTONS;

        private const String WINMM_NATIVE_LIBRARY = "winmm.dll";
        private const CallingConvention CALLING_CONVENTION = CallingConvention.StdCall;
    }
}
