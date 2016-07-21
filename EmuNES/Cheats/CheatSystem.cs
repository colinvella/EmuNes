using NesCore.Memory;
using NesCore.Processor;
using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes.Cheats
{
    public class CheatSystem
    {
        public CheatSystem(Mos6502 processor)
        {
            this.processor = processor;
            cheats = new Dictionary<ushort, Cheat>();

            PatchCheats();
        }

        public IEnumerable<ushort> PatchedAddresses { get { return cheats.Keys; } }

        public IEnumerable<Cheat> Cheats { get { return cheats.Values; } }

        public void Clear()
        {
            cheats.Clear();
            UnpatchCheats();
        }

        public bool AddCheat(Cheat cheat)
        {
            if (cheats.ContainsKey(cheat.Address))
            {
                MessageBox.Show("A cheat is already patched at address: " + Hex.Format(cheat.Address), "Add Cheat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            cheats[cheat.Address] = cheat;
            PatchCheats();
            return true;
        }

        public bool AddCheat(string gameGenieCode)
        {
            gameGenieCode = gameGenieCode.Trim();

            if (!IsValidGameGenieCode(gameGenieCode))
            {
                MessageBox.Show("Invalid Game Genie code", "Add Game Genie Code", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            Cheat gameGenieCheat = DecodeGameGenieCode(gameGenieCode);

            if (cheats.ContainsKey(gameGenieCheat.Address))
            {
                MessageBox.Show("A cheat is already patched at address: " + Hex.Format(gameGenieCheat.Address), "Add Game Genie Code", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            cheats[gameGenieCheat.Address] = gameGenieCheat;
            PatchCheats();
            return true;
        }

        public bool RemoveCheat(ushort address)
        {
            if (!cheats.ContainsKey(address))
            {
                MessageBox.Show("No cheat patched at address: " + Hex.Format(address), "Remove Cheat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            cheats.Remove(address);

            if (cheats.Count == 0)
                UnpatchCheats();

            return true;
        }

        public void Load(string filename)
        {
            string[] lines = null;
            try
            {
                lines = File.ReadAllLines(filename);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Unable to load cheat file. Reason: " + exception.Message,
                    "Load Cheat File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            cheats.Clear();
            foreach (string line in lines)
            {
                try
                {
                    string trimmedLine = line.Trim();
                    if (trimmedLine == ""
                        || trimmedLine.StartsWith("//")
                        || trimmedLine.StartsWith("#"))
                        continue;

                    Cheat cheat = Cheat.Parse(trimmedLine);
                    cheats[cheat.Address] = cheat;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(
                        "Unable to parse line:\r" + line + "\rReason: " + exception.Message,
                        "Load Cheat File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (cheats.Count > 0)
                PatchCheats();
        }

        public void PatchCheats()
        {
            if (oldProcessorReadByte != null)
                UnpatchCheats();

            this.oldProcessorReadByte = processor.ReadByte;

            processor.ReadByte = (address) =>
            {
                // check for patched cheat at givne address
                Cheat cheat = null;
                cheats.TryGetValue(address, out cheat);

                // if no cheat, just go through
                if (cheat == null)
                    return oldProcessorReadByte(address);

                // if cheat not active, just go through
                if (!cheat.Active)
                    return oldProcessorReadByte(address);

                // if cheat is substitute only, apply substitution
                if (!cheat.NeedsComparison)
                    return cheat.Value;

                // otherwise, compare unpatched value before applying
                byte unpatchedValue = oldProcessorReadByte(address);
                if (cheat.CompareValue == unpatchedValue)
                    return cheat.Value;
                else
                    return unpatchedValue;
            };
        }

        public void UnpatchCheats()
        {
            if (oldProcessorReadByte == null)
                return;

            processor.ReadByte = oldProcessorReadByte;

            oldProcessorReadByte = null;
        }

        public bool IsValidGameGenieCode(string gameGenieCode)
        {
            if (gameGenieCode.Length != 6 && gameGenieCode.Length != 8)
                return false;

            foreach (char ch in gameGenieCode)
                if (!gameGenieCharacterMap.ContainsKey(ch))
                    return false;

            return true;
        }

        private Cheat DecodeGameGenieCode(string gameGenieCode)
        {
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

        private ushort DecodeGameGenieCodeAddress(byte[] nybbles)
        {
            return (ushort)(0x8000
                + ((nybbles[3] & 7) << 12) | ((nybbles[5] & 7) << 8) | ((nybbles[4] & 8) << 8)
                | ((nybbles[2] & 7) << 4) | ((nybbles[1] & 8) << 4) | (nybbles[4] & 7) | (nybbles[3] & 8));
        }

        private Mos6502 processor;
        private Dictionary<ushort, Cheat> cheats;
        private ReadByte oldProcessorReadByte;

        private static Dictionary<char, byte> gameGenieCharacterMap;

        static CheatSystem()
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