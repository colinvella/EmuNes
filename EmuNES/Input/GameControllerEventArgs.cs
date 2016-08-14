using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Input
{
    public delegate void GameControllerEventHandler(object sender, GameControllerEventArgs gameControllerEventArgs);

    public class GameControllerEventArgs
    {
        public GameControllerEventArgs(byte id, GameController.Button button)
        {
            Id = id;
            Button = button;
        }

        public byte Id { get; private set;}
        public GameController.Button Button { get; private set; }
    }
}
