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

        public string GameGenieCode
        {
            get
            {
                byte[] nybbles = new byte[NeedsComparison ? 8 : 6];

                // reverse of
                // address = 0x8000 + ((n3 & 7) << 12) | ((n5 & 7) << 8) | ((n4 & 8) << 8) | ((n2 & 7) << 4) | ((n1 & 8) << 4) | (n4 & 7) | (n3 & 8);

                // common to both 6-letter and 8-letter codes
                nybbles[0] = (byte)(((Value   >> 4) & 8) | ((Value   >>  0) & 7));
                nybbles[1] = (byte)(((Address >> 4) & 8) | ((Value   >>  4) & 7));

                nybbles[2] = (byte)(((Address >> 4) & 7));
                nybbles[3] = (byte)(((Address >> 0) & 8) | ((Address >> 12) & 7));
                nybbles[4] = (byte)(((Address >> 8) & 8) | ((Address >>  0) & 7));

                if (NeedsComparison)
                {
                    nybbles[2] |= 8; // set 3rd nybble bit 7 to make it behave nicely in real Game Genie
 
                    // reverse of
                    // data = ((n1 & 7) << 4) | ((n0 & 8) << 4) | (n0 & 7) | (n7 & 8);
                    // compare = ((n7 & 7) << 4) | ((n6 & 8) << 4) | (n6 & 7) | (n5 & 8);
                    nybbles[5] = (byte)(((CompareValue >> 0) & 8) | ((Address      >> 8) & 7));
                    nybbles[6] = (byte)(((CompareValue >> 4) & 8) | ((CompareValue >> 0) & 7));
                    nybbles[7] = (byte)(((Value        >> 0) & 8) | ((CompareValue >> 4) & 7));
                }
                else // 6 letter code
                {
                    // reverse of
                    // data = ((n1 & 7) << 4) | ((n0 & 8) << 4) | (n0 & 7) | (n5 & 8);
                    nybbles[5] = (byte)(((Value   >> 0) & 8) | ((Address >> 8) & 7));
                }

                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte nybble in nybbles)
                    stringBuilder.Append(gameGenieNybbleToChar[nybble]);
                
                return stringBuilder.ToString();
            }
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
                if (!gameGenieCharToNybble.ContainsKey(ch))
                    return false;

            return true;
        }

        public static Cheat ParseGameGenieCode(string gameGenieCode)
        {
            if (!IsValidGameGenieCode(gameGenieCode))
                return null;

            byte[] nybbles = new byte[gameGenieCode.Length];
            for (int index = 0; index < gameGenieCode.Length; index++)
                nybbles[index] = gameGenieCharToNybble[gameGenieCode[index]];

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

        private static char[] gameGenieNybbleToChar;
        private static Dictionary<char, byte> gameGenieCharToNybble;

        static Cheat()
        {
            gameGenieNybbleToChar = new char[]
                { 'A', 'P', 'Z', 'L', 'G', 'I', 'T', 'Y',
                  'E', 'O', 'X', 'U', 'K', 'S', 'V', 'N' };
            gameGenieCharToNybble = new Dictionary<char, byte>();

            for (byte nybble = 0; nybble < gameGenieNybbleToChar.Length; nybble++)
                gameGenieCharToNybble[gameGenieNybbleToChar[nybble]] = nybble;
        }
    }
}
