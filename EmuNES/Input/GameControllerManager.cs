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
            int joystickCount =  WindowsMultiMedia.joyGetNumDevs();
            byte controllerId = 0;
            WindowsMultiMedia.JOYINFOEX joyInfoEx = new WindowsMultiMedia.JOYINFOEX();
            for (Int32 deviceId = 0; deviceId < joystickCount; deviceId++)
            {
                if (WindowsMultiMedia.joyGetPosEx(deviceId, ref joyInfoEx) == 0)
                    gameControllers.Add(new GameController(controllerId++, deviceId));
            }
        }

        public byte Count { get { return (byte)gameControllers.Count; } }

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
