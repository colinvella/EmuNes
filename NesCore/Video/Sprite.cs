using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Video
{
    public struct Sprite
    {
        public uint Pattern { get; set; }
        public byte PositionX { get; set; }
        public byte Priority { get; set; }
        public byte Index { get; set; }

        public void SaveState(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Pattern);
            binaryWriter.Write(PositionX);
            binaryWriter.Write(Priority);
            binaryWriter.Write(Index);           
        }

        public void LoadState(BinaryReader binaryReader)
        {
            Pattern = binaryReader.ReadUInt16();
            PositionX = binaryReader.ReadByte();
            Priority = binaryReader.ReadByte();
            Index = binaryReader.ReadByte();
        }
    }
}
