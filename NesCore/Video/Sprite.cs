using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Video
{
    public struct Sprite
    {
        public uint Pattern { get; set; }
        public byte Position { get; set; }
        public byte Priority { get; set; }
        public byte Index { get; set; }
    }
}
