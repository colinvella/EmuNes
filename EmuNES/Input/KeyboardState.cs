using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes.Input
{
    public class KeyboardState
    {
        public KeyboardState()
        {
            keyPressed = new Dictionary<Keys, bool>();
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
                keyPressed[key] = false;
        }

        public bool this[Keys key]
        {
            get { return keyPressed[key]; }
            set { keyPressed[key] = value; }
        }

        private Dictionary<Keys, bool> keyPressed;
    }
}
