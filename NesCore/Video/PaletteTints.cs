using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Video
{
    public class PaletteTints
    {
        public PaletteTints()
        {
            paletteTints = new Palette[8];
            for (byte tint = 0; tint < 8; tint++)
                paletteTints[tint] = new Palette(tint);
        }

        public Palette this[byte tint]
        {
            get
            {
                tint &= 0x07;
                return paletteTints[tint];
            }
        }

        private Palette[] paletteTints;
    }
}
