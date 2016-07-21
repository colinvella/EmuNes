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
            gameGenieCode = gameGenieCode.Trim().ToUpper();

            if (!Cheat.IsValidGameGenieCode(gameGenieCode))
            {
                MessageBox.Show("Invalid Game Genie code", "Add Game Genie Code", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            Cheat gameGenieCheat = Cheat.ParseGameGenieCode(gameGenieCode);

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

        private Mos6502 processor;
        private Dictionary<ushort, Cheat> cheats;
        private ReadByte oldProcessorReadByte;
    }
}