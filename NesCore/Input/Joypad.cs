using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Input
{
    public delegate bool ButtonPressed();

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
        public ButtonPressed Start { get { return buttonPressed[2]; } set { buttonPressed[2] = value; } }
        public ButtonPressed Select { get { return buttonPressed[3]; } set { buttonPressed[3] = value; } }
        public ButtonPressed Up { get { return buttonPressed[4]; } set { buttonPressed[4] = value; } }
        public ButtonPressed Down { get { return buttonPressed[5]; } set { buttonPressed[5] = value; } }
        public ButtonPressed Left { get { return buttonPressed[6]; } set { buttonPressed[6] = value; } }
        public ButtonPressed Right { get { return buttonPressed[7]; } set { buttonPressed[7] = value; } }

        public byte Port
        {
            get
            {
                byte value = 0x00;

                if (buttonIndex < 8 && buttonPressed[buttonIndex]())
                    value = 0x01;
                ++buttonIndex;

                if ((strobe & 0x01) == 0x01)
                    buttonIndex = 0x00;

                return value;
            }
            set
            {
                strobe = value;
                if ((strobe & 0x01) == 0x01)
                    buttonIndex = 0x00;
            }
        }

        private ButtonPressed[] buttonPressed;
        private byte buttonIndex;
        private byte strobe;
    }
}
