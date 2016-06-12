using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES.Input
{
    delegate void GameControllerEventHandler(object sender, GameControllerEventArgs gameControllerEventArgs);

    class GameControllerEventArgs
    {
        public GameControllerEventArgs(byte id, Button button)
        {
            Id = id;
            Button = button;
        }

        public byte Id { get; private set;}
        public Button Button { get; private set; }
    }
}
