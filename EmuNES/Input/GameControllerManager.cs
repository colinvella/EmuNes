using EmuNES.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES.Input
{
    class GameControllerManager
    {
        public GameControllerManager()
        {
            gameControllers = new List<GameController>();
            int joystickCount = joyGetNumDevs();
            for (Int32 joystickIndex = 0; joystickIndex < joystickCount; joystickIndex++)
            {
                gameControllers.Add(new GameController((byte)joystickIndex));
            }
        }

        public GameController this[ushort gameControllerIndex]
        {
            get
            {
                if (gameControllerIndex >= gameControllers.Count)
                    throw new IndexOutOfRangeException("gameControllerIndex");
                return gameControllers[gameControllerIndex];
            }
        }

        public void UpdateState()
        {
            gameControllers.ForEach((gameController) => gameController.UpdateState());
        }

        private List<GameController> gameControllers;

        private const String WINMM_NATIVE_LIBRARY = "winmm.dll";
        private const CallingConvention CALLING_CONVENTION = CallingConvention.StdCall;

        [DllImport(WINMM_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        private static extern Int32 joyGetNumDevs();
    }
}
