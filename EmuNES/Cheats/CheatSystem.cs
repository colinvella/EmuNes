using NesCore.Memory;
using NesCore.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes.Cheats
{
    class CheatSystem
    {
        public CheatSystem()
        {
            cheats = new Dictionary<ushort, Cheat>();
        }

        public IEnumerable<Cheat> Cheats { get { return cheats.Values; } }

        public void Clear(Mos6502 processor)
        {
            UnpatchCheats(processor);
            cheats.Clear();
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
        }

        public void PatchCheats(Mos6502 processor)
        {
            if (oldProcessorReadByte != null)
                UnpatchCheats(processor);

            if (cheats.Count == 0)
                return;

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
                    return cheat.CompareValue;
                else
                    return unpatchedValue;
            };
        }

        public void UnpatchCheats(Mos6502 processor)
        {
            if (oldProcessorReadByte == null)
                return;

            processor.ReadByte = oldProcessorReadByte;

            oldProcessorReadByte = null;
        }

        private Dictionary<ushort, Cheat> cheats;
        private ReadByte oldProcessorReadByte;
    }
}
