using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Input
{
    public class MouseState
    {
        public MouseState()
        {
        }

        public Point Position { get; set; }
        public bool SensePixel { get; set; }
        public bool LeftButtonPressed { get; set; }
    }
}
