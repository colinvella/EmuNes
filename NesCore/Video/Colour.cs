using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Video
{
    public struct Colour
    {
        public Colour(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public Colour(ulong colourValue)
        {
            Red = (byte)(colourValue >> 16);
            Green = (byte)(colourValue >> 8);
            Blue = (byte)colourValue;
        }

        /// <summary>
        /// Creates tinted copy of the colour
        /// </summary>
        /// <param name="red">de-emphasise green and blue</param>
        /// <param name="green">de-emphasise red and blue</param>
        /// <param name="blue">de-emphasise red and green</param>
        /// <returns></returns>
        public Colour Emphasise(bool red, bool green, bool blue)
        {
            byte newRed = Red;
            byte newGreen = Green;
            byte newBlue = Blue;
            if (red)
            {
                newGreen = (byte)(newGreen * 0.8);
                newBlue = (byte)(newBlue * 0.8);
            }
            if (green)
            {
                newRed = (byte)(newRed * 0.8);
                newBlue = (byte)(newBlue * 0.8);
            }
            if (blue)
            {
                newRed = (byte)(newRed * 0.8);
                newGreen = (byte)(newGreen * 0.8);
            }

            return new Colour(newRed, newGreen, newBlue);
        }

        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }
    }
}
