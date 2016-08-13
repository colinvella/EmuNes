using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Input
{
    public class Joypad: Controller
    {
        public Joypad()
        {
            buttonPressed = new ButtonPressed[8];
            buttonIndex = 0x00;
            A = B = Start = Select = Up = Down = Left = Right = () => false;
        }

        public ButtonPressed A { get { return buttonPressed[0]; } set { buttonPressed[0] = value; } }
        public ButtonPressed B { get { return buttonPressed[1]; } set { buttonPressed[1] = value; } }
        public ButtonPressed Select { get { return buttonPressed[2]; } set { buttonPressed[2] = value; } }
        public ButtonPressed Start { get { return buttonPressed[3]; } set { buttonPressed[3] = value; } }
        public ButtonPressed Up { get { return buttonPressed[4]; } set { buttonPressed[4] = value; } }
        public ButtonPressed Down { get { return buttonPressed[5]; } set { buttonPressed[5] = value; } }
        public ButtonPressed Left { get { return buttonPressed[6]; } set { buttonPressed[6] = value; } }
        public ButtonPressed Right { get { return buttonPressed[7]; } set { buttonPressed[7] = value; } }

        public bool IsSerial { get { return true; } }

        public void Strobe()
        {
            buttonIndex = 0x00;
        }

        public bool ReadSerial()
        {
            bool value = false;

            if (buttonIndex < 8 && buttonPressed[buttonIndex]())
                value = true;
            ++buttonIndex;

            return value;
        }

        public byte PortValue { get { return 0x00; } }

        private ButtonPressed[] buttonPressed;
        private byte buttonIndex;
    }
}
