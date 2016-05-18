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
            buttonPressed = new bool[8];
            buttonIndex = 0x00;
            A = B = Start = Select = Up = Down = Left = Right = () => false;
        }

        public ButtonPressed A { get; set; }
        public ButtonPressed B { get; set; }
        public ButtonPressed Start { get; set; }
        public ButtonPressed Select { get; set; }
        public ButtonPressed Up { get; set; }
        public ButtonPressed Down { get; set; }
        public ButtonPressed Left { get; set; }
        public ButtonPressed Right { get; set; }

        public byte Port
        {
            get
            {
                byte value = 0x00;

                if (buttonIndex < 8 && buttonPressed[buttonIndex])
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

        public void Step()
        {                              
            buttonPressed[0] = A();
            buttonPressed[1] = B();
            buttonPressed[2] = Start();
            buttonPressed[3] = Select();
            buttonPressed[4] = Up();
            buttonPressed[5] = Down();
            buttonPressed[6] = Left();
            buttonPressed[7] = Right();
        }


        private bool[] buttonPressed;
        private byte buttonIndex;
        private byte strobe;
    }
}
