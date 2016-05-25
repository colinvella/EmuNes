using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public class Cartridge
    {
        public Cartridge(BinaryReader romBinaryReader)
        {
            SaveRam = new byte[0x2000];

            uint magicNumber = romBinaryReader.ReadUInt32();

            if (magicNumber != InesMagicNumber)
                throw new InvalidDataException("INES Magic Number mismatch");

            // read header
            byte programBankCount = romBinaryReader.ReadByte();
            byte characterBankCount = romBinaryReader.ReadByte();
            byte controlBits1 = romBinaryReader.ReadByte();
            byte controlBits2 = romBinaryReader.ReadByte();
            byte programRamSize = romBinaryReader.ReadByte();
            romBinaryReader.ReadByte(); // unused byte

            // determine mapper type from control bits
            int mapperTypeLowerNybble = controlBits1 >> 4;
            int mapperTypeHigherNybble = controlBits2 >> 4;
            MapperType = (byte)((mapperTypeHigherNybble << 4) | mapperTypeLowerNybble);

            // determine mirroring mode
            int mirrorLowBit = controlBits1 & 1;
            int mirrorHighBit = (controlBits1 >> 3) & 1;
            MirrorMode = (byte)((mirrorHighBit << 1) | mirrorLowBit);

            // battery-backed RAM
            BatteryPresent = (controlBits1 & 0x2) != 0;

            // read trainer if present (unused)
            if ((controlBits1 & 0x04) == 0x04)
            {
                byte[] trainer = romBinaryReader.ReadBytes(512);
            }

            // read prg-rom bank(s)
            byte[] programData = romBinaryReader.ReadBytes(programBankCount * 0x4000);
            ProgramRom = new List<byte>(programData);

            // read chr-rom bank(s)
            byte[] characterData = characterBankCount == 0
                ? new byte[0x2000] // at least one default empty bank if there are none
                : romBinaryReader.ReadBytes(characterBankCount * 0x2000);
            CharacterRom = new List<byte>(characterData);

            // instantiate appropriate mapper
            switch (MapperType)
            {
                case 0: Map = new CartridgeMapNRom(this); break;
                case 1: Map = new CartridgeMapMmc1(this); break;
                default: throw new NotSupportedException("Mapper Type " + Utility.Hex.Format(MapperType) + " not supported");
            }
        }

        public IReadOnlyList<byte> ProgramRom { get; private set; }
        public IReadOnlyList<byte> CharacterRom { get; private set; }
        public byte[] SaveRam { get; private set; }
        public byte MapperType { get; private set; }
        public byte MirrorMode { get; set; }
        public bool BatteryPresent { get; private set; }

        public CartridgeMap Map { get; private set; }

        public override string ToString()
        {
            return "PRG: " + Hex.Format((uint)ProgramRom.Count)
                + "b, CHR: " + Hex.Format((uint)CharacterRom.Count)
                + "b, Mapper Type: " + Hex.Format(MapperType)
                + ", Mirror Mode:" + Hex.Format(MirrorMode)
                + ", Battery: " + (BatteryPresent ? "Yes" : "No");
        }

        private const uint InesMagicNumber = 0x1a53454e;
    }

   
}
