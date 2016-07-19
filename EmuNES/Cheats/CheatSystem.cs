using NesCore.Memory;
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
            Cheats = new List<Cheat>();
        }

        public List<Cheat> Cheats { get; private set; }

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

            Cheats.Clear();

            foreach (string line in lines)
            {
                try
                {
                    Cheat cheat = Cheat.Parse(line);
                    Cheats.Add(cheat);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(
                        "Unable to parse line:\r" + line + "\rReason: " + exception.Message,
                        "Load Cheat File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void ApplyCheats(MemoryMap memoryMap)
        {
            foreach (Cheat cheat in Cheats)
                if (cheat.Active)
                    cheat.Apply(memoryMap);
        }
    }
}
