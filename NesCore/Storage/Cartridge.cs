using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public class Cartridge
    {
        public delegate byte DetermineMapperIdHandler(uint romCrc, byte romMapperId);

        public Cartridge(BinaryReader romBinaryReader)
        {
            byte[] romBody = LoadFromBinaryReader(romBinaryReader);

            SaveRam = new SaveRam();

            // compute CRC
            Crc32 crc32 = new Crc32();
            Crc = crc32.ComputeChecksum(romBody.ToArray());

            if (DetermineMapperId != null)
                MapperId = DetermineMapperId(Crc, MapperId);

            DetermineCartridgeMap();
            Debug.WriteLine(ToString());
        }

        public Cartridge(byte[] programRom, byte[] characterRom, byte mapperId, MirrorMode mirrorMode)
        {
            ProgramRom = programRom.ToArray();
            CharacterRom = characterRom.ToArray();
            MapperId = mapperId;
            MirrorMode = mirrorMode;

            SaveRam = new SaveRam();

            // compute CRC
            byte[] romBody = new byte[programRom.Length + characterRom.Length];
            Array.Copy(programRom, romBody, programRom.Length);
            Array.Copy(characterRom, 0, romBody, programRom.Length, characterRom.Length);
            Crc32 crc32 = new Crc32();
            Crc = crc32.ComputeChecksum(romBody.ToArray());

            if (DetermineMapperId != null)
                MapperId = DetermineMapperId(Crc, MapperId);

            DetermineCartridgeMap();
            Debug.WriteLine(ToString());
        }

        public static DetermineMapperIdHandler DetermineMapperId { get; set; }

        public IReadOnlyList<byte> ProgramRom { get; private set; }
        public byte[] CharacterRom { get; private set; }
        public SaveRam SaveRam { get; }
        public byte MapperId { get; private set; }
        public MirrorMode MirrorMode { get; private set; }
        public bool BatteryPresent { get; private set; }
        public uint Crc { get; private set; }

        public CartridgeMap Map { get; private set; }

        public override string ToString()
        {
            return "PRG ROM Size: " + KiloBytes.Format(ProgramRom.Count)
                + ", CHR ROM Size: " + KiloBytes.Format(CharacterRom.Length)
                + ", Mapper ID: " + MapperId
                + ", Mirror Mode: " + MirrorMode + " (" + (byte)MirrorMode + ")"
                + ", Battery: " + (BatteryPresent ? "Yes" : "No");
        }

        private byte[] LoadFromBinaryReader(BinaryReader romBinaryReader)
        {
            List<byte> romBody = new List<byte>();

            uint magicNumber = romBinaryReader.ReadUInt32();

            if (magicNumber != InesMagicNumber)
                throw new InvalidDataException("INES Magic Number mismatch");

            // read header
            byte programBankCount = romBinaryReader.ReadByte();
            byte characterBankCount = romBinaryReader.ReadByte();
            byte controlBits1 = romBinaryReader.ReadByte();
            byte controlBits2 = romBinaryReader.ReadByte();
            byte programRamSize = romBinaryReader.ReadByte();
            romBinaryReader.ReadBytes(7); // unused 7 bytes

            // determine mapper id from control bits
            int mapperIdLowerNybble = controlBits1 >> 4;
            int mapperIdHigherNybble = controlBits2 >> 4;
            MapperId = (byte)((mapperIdHigherNybble << 4) | mapperIdLowerNybble);

            // determine mirroring mode
            int mirrorLowBit = controlBits1 & 1;
            int mirrorHighBit = (controlBits1 >> 3) & 1;

            byte mirrorModeByte = (byte)((mirrorHighBit << 1) | mirrorLowBit);
            switch (mirrorModeByte)
            {
                case 0: MirrorMode = MirrorMode.Horizontal; break;
                case 1: MirrorMode = MirrorMode.Vertical; break;
                case 2: MirrorMode = MirrorMode.Single0; break;
                case 3: MirrorMode = MirrorMode.Single1; break;
            }

            // battery-backed RAM
            BatteryPresent = (controlBits1 & 0x2) != 0;

            // read trainer if present (unused)
            if ((controlBits1 & 0x04) == 0x04)
            {
                byte[] trainer = romBinaryReader.ReadBytes(512);
                romBody.AddRange(trainer);
            }

            // read prg-rom bank(s)
            byte[] programData = romBinaryReader.ReadBytes(programBankCount * 0x4000);
            ProgramRom = new List<byte>(programData);
            romBody.AddRange(programData);

            // read chr-rom bank(s)
            if (characterBankCount == 0)
            {
                CharacterRom = new byte[0x2000]; // at least one default empty bank if there are none
            }
            else
            {
                CharacterRom = romBinaryReader.ReadBytes(characterBankCount * 0x2000);
                romBody.AddRange(CharacterRom);
            }

            return romBody.ToArray();
        }

        private void DetermineCartridgeMap()
        {
            // instantiate appropriate mapper
            switch (MapperId)
            {
                case 0: Map = new CartridgeMapNRom(this); break;
                case 1: Map = new CartridgeMapMmc1(this); break;
                case 2: Map = new CartridgeMapUxRom(this); break;
                case 3: Map = new CartridgeMapCnRom(this); break;
                case 4: Map = new CartridgeMapMmc3(this); break;
                case 5: Map = new CartridgeMapMmc5(this); break;
                case 7: Map = new CartridgeMapAxRom(this); break;
                case 9: Map = new CartridgeMapMmc2(this); break;
                case 10: Map = new CartridgeMapMmc4(this); break;
                case 11: Map = new CartridgeMapColourDreams(this); break;
                case 12: Map = new CartridgeMapMmc3(this, true); break;
                case 13: Map = new CartridgeMapCpRom(this); break;
                case 15: Map = new CartridgeMap100In1(this); break;
                case 16: Map = new CartridgeMapBandaiFcg(this, CartridgeMapBandaiFcg.Variant.Bandai_FCG_1_and_2 | CartridgeMapBandaiFcg.Variant.LZ93D50); break;
                case 18: Map = new CartridgeMapJalecoSs88006(this); break;
                case 19: Map = new CartridgeMapNamco(this, CartridgeMapNamco.Variant.Namco_129 | CartridgeMapNamco.Variant.Namco_163); break;
                case 21: Map = new CartridgeMapKonamiVrc4(this, CartridgeMapKonamiVrc4.Variant.Vrc4RevAorC); break;
                case 22: Map = new CartridgeMapKonamiVrc2(this, CartridgeMapKonamiVrc2.Variant.Vrc2a); break;
                case 23: Map = new CartridgeMapKonamiVrc2(this, CartridgeMapKonamiVrc2.Variant.Vrc2b); break;
                case 24: Map = new CartridgeMapKonamiVrc6(this, CartridgeMapKonamiVrc6.Variant.Vrc6a); break;
                case 25: Map = new CartridgeMapKonamiVrc4(this, CartridgeMapKonamiVrc4.Variant.Vrc4RevBorD); break;
                case 26: Map = new CartridgeMapKonamiVrc6(this, CartridgeMapKonamiVrc6.Variant.Vrc6b); break;
                case 27: Map = new CartridgeMapKonamiVrc4(this, CartridgeMapKonamiVrc4.Variant.Vrc4RevEorF); break; // CHR broken
                case 29: Map = new CartridgeMapHomeBrew29(this); break;
                case 30: Map = new CartridgeMapUnrom512(this); break;
                case 31: Map = new CartridgeMapInfiniteNesLives(this); break;
                case 32: Map = new CartridgeMapIremG101(this); break;
                case 33: Map = new CartridgeMapTaitoTc0190(this); break;
                case 34: Map = new CartridgeMapBnRomNina001(this); break;
                case 36: Map = new CartridgeMapMicroGenius36(this); break;
                case 37: Map = new CartridgeMapMultiMmc3(this); break;
                case 38: Map = new CartridgeMapCrimeBusters(this); break;
                case 40: Map = new CartridgeMapSmb2LostLevels(this); break;
                case 41: Map = new CartridgeMapCaltron6in1(this); break;
                case 42: Map = new CartridgeMapFdsHacks(this); break;
                case 46: Map = new CartridgeMapRumbleStation(this); break;
                case 48: Map = new CartridgeMapTaitoTc0690(this); break;
                case 49: Map = new CartridgeMapSuperHiK4in1(this); break;
                case 66: Map = new CartridgeMapGxRom(this); break;
                case 71: Map = new CartridgeMapCamerica71(this); break;
                case 153: Map = new CartridgeMapBandaiFcg(this, CartridgeMapBandaiFcg.Variant.LZ93D50_with_SRAM); break;
                case 157: Map = new CartridgeMapBandaiFcg(this, CartridgeMapBandaiFcg.Variant.Datach_Joint_Rom_System); break;
                case 201: Map = new CartridgeMapMapper201(this); break;
                case 210: Map = new CartridgeMapNamco(this, CartridgeMapNamco.Variant.Namco_175 | CartridgeMapNamco.Variant.Namco_340); break;
                default:
                    throw new NotSupportedException(
                        "Mapper ID " + MapperId + " not supported");
            }
        }

        private const uint InesMagicNumber = 0x1a53454e;
    }

   
}
