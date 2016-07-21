using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Cheats
{
    public class Cheat
    {
        public bool Active { get; set; }
        public bool NeedsComparison { get; set; }
        public ushort Address { get; set; }
        public byte Value { get; set; }
        public byte CompareValue { get; set; }
        public string Description
        {
            get { return description; }            
            set { description = value.Trim().Replace(":", ""); }
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool longFormat)
        {
            if (!longFormat)
                return description;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(NeedsComparison ? "SC:" : "S:");
            stringBuilder.Append(Address.ToString("X4"));
            stringBuilder.Append(":");
            stringBuilder.Append(Value.ToString("X2"));
            stringBuilder.Append(":");
            if (NeedsComparison)
            {
                stringBuilder.Append(CompareValue.ToString("X2"));
                stringBuilder.Append(":");
            }
            stringBuilder.Append(Description);
            return stringBuilder.ToString();
        }

        public static Cheat Parse(string line)
        {
            string[] tokens = line.Split(new char[] { ':' });
            if (tokens.Length < 4 || tokens.Length > 5)
                throw new FormatException(
                    "Cheat must consist of 4 or 5 elements separated by a semicolom");

            for (int index = 0; index < tokens.Length; index++)
                tokens[index] = tokens[index].Trim();

            Cheat cheat = new Cheat();

            string prefix = tokens[0].ToUpper();
            if (prefix != "S" && prefix != "SC")
                throw new FormatException("Unsupported cheat prefix: " + prefix);
            cheat.NeedsComparison = prefix == "SC";

            try
            {
                ushort address = Convert.ToUInt16(tokens[1], 16);
                cheat.Address = address;
            }
            catch (Exception)
            {
                throw new FormatException("Invalid address: " + tokens[1]);
            }

            try
            {
                byte value = Convert.ToByte(tokens[2], 16);
                cheat.Value = value;
            }
            catch (Exception)
            {
                throw new FormatException("Invalid cheat value: " + tokens[2]);
            }

            if (cheat.NeedsComparison)
            {
                try
                {
                    byte compareValue = Convert.ToByte(tokens[3], 16);
                    cheat.CompareValue = compareValue;

                    cheat.Description = tokens[4];
                }
                catch (Exception)
                {
                    throw new FormatException("Invalid compare value: " + tokens[3]);
                }
            }
            else
            {
                cheat.Description = tokens[3];
            }

            return cheat;
        }

        public static bool IsValidGameGenieCode(string gameGenieCode)
        {
            if (gameGenieCode.Length != 6 && gameGenieCode.Length != 8)
                return false;

            foreach (char ch in gameGenieCode)
                if (!gameGenieCharacterMap.ContainsKey(ch))
                    return false;

            return true;
        }

        public static Cheat ParseGameGenieCode(string gameGenieCode)
        {
            if (!IsValidGameGenieCode(gameGenieCode))
                return null;

            byte[] nybbles = new byte[gameGenieCode.Length];
            for (int index = 0; index < gameGenieCode.Length; index++)
                nybbles[index] = gameGenieCharacterMap[gameGenieCode[index]];

            Cheat newCheat = new Cheat();
            newCheat.Address = DecodeGameGenieCodeAddress(nybbles);

            if (gameGenieCode.Length == 6)
            {
                newCheat.Value = (byte)(((nybbles[1] & 7) << 4) | ((nybbles[0] & 8) << 4) | (nybbles[0] & 7) | (nybbles[5] & 8));
            }
            else // 8
            {
                newCheat.Value = (byte)(((nybbles[1] & 7) << 4) | ((nybbles[0] & 8) << 4) | (nybbles[0] & 7) | (nybbles[7] & 8));
                newCheat.CompareValue = (byte)(((nybbles[7] & 7) << 4) | ((nybbles[6] & 8) << 4) | (nybbles[6] & 7) | (nybbles[5] & 8));
                newCheat.NeedsComparison = true;
            }

            newCheat.Description = "Game Genie Code: " + gameGenieCode;

            return newCheat;
        }

        private static ushort DecodeGameGenieCodeAddress(byte[] nybbles)
        {
            return (ushort)(0x8000
                + ((nybbles[3] & 7) << 12) | ((nybbles[5] & 7) << 8) | ((nybbles[4] & 8) << 8)
                | ((nybbles[2] & 7) << 4) | ((nybbles[1] & 8) << 4) | (nybbles[4] & 7) | (nybbles[3] & 8));
        }

        private string description;

        private static Dictionary<char, byte> gameGenieCharacterMap;

        static Cheat()
        {
            gameGenieCharacterMap = new Dictionary<char, byte>();
            gameGenieCharacterMap['A'] = 0x0;
            gameGenieCharacterMap['P'] = 0x1;
            gameGenieCharacterMap['Z'] = 0x2;
            gameGenieCharacterMap['L'] = 0x3;
            gameGenieCharacterMap['G'] = 0x4;
            gameGenieCharacterMap['I'] = 0x5;
            gameGenieCharacterMap['T'] = 0x6;
            gameGenieCharacterMap['Y'] = 0x7;
            gameGenieCharacterMap['E'] = 0x8;
            gameGenieCharacterMap['O'] = 0x9;
            gameGenieCharacterMap['X'] = 0xA;
            gameGenieCharacterMap['U'] = 0xB;
            gameGenieCharacterMap['K'] = 0xC;
            gameGenieCharacterMap['S'] = 0xD;
            gameGenieCharacterMap['V'] = 0xE;
            gameGenieCharacterMap['N'] = 0xF;
        }
    }
}
