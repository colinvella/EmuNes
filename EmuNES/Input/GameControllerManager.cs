using SharpNes.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Input
{
    public class GameControllerManager
    {
        public GameControllerManager()
        {
            gameControllers = new List<GameController>();
            int joystickCount =  WindowsMultiMedia.GetJoystickDeviceCount();
            byte controllerId = 0;

            WindowsMultiMedia.JOYINFOEX joyInfoEx = new WindowsMultiMedia.JOYINFOEX();
            joyInfoEx.dwSize = Marshal.SizeOf(joyInfoEx);
            joyInfoEx.dwFlags = WindowsMultiMedia.JOY_RETURNALL;

            for (Int32 deviceId = 0; deviceId < joystickCount; deviceId++)
            {
                if (WindowsMultiMedia.GetJoystickState(deviceId, ref joyInfoEx) == WindowsMultiMedia.JOYERR_NOERROR)
                    gameControllers.Add(new GameController(controllerId++, deviceId));
            }
        }

        public byte Count { get { return (byte)gameControllers.Count; } }

        public IEnumerable<GameController> Controllers { get { return gameControllers; } }

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
    }
}
