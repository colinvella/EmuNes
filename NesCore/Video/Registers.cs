using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Video
{
    public class Registers
    {
        /// <summary>
        /// PPUCTRL $2000
        /// </summary>
        public byte Control { get; set; }

        /// <summary>
        /// PPUMASK $2001
        /// </summary>
        public byte Mask { get; set; }

        /// <summary>
        /// PPUSTATUS $2002
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// OAMADDR $2003
        /// </summary>
        public byte ObjectAttributeMemoryAddress { get; set; }

        /// <summary>
        /// OAMDATA $2004
        /// </summary>
        public byte ObjectAttributeMemoryData { get; set; }

        /// <summary>
        /// PPUSCROLL $2005
        /// </summary>
        public byte Scroll { get; set; } // TODO: write two times X, Y

        /// <summary>
        /// PPUADDR $2006
        /// </summary>
        public byte Address { get; set; } // TODO: write two times Hi, Lo

        /// <summary>
        /// PPUDATA $2007
        /// </summary>
        public byte Data { get; set; }

        /// <summary>
        /// OAMDMA $4014
        /// </summary>
        public byte DirectMemoryAccess { get; set; }

    }
}
