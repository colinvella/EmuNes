using NesCore.Memory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Cheats
{
    class Cheat
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

        public void Apply(MemoryMap memoryMap)
        {
            if (NeedsComparison && memoryMap[Address] != CompareValue)
                return;

            memoryMap[Address] = Value;
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

        private string description;
    }
}
