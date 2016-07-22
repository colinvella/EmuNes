using NesCore.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes.Cheats
{
    public partial class CheatFinderForm : Form
    {
        public CheatFinderForm(MemoryMap memoryMap, CheatSystem cheatSystem)
        {
            InitializeComponent();

            this.memoryMap = memoryMap;

            if (currentValues.Count == 0 && previousValues.Count == 0)
                ResetSearch();

            UpdateValueMaps();
        }

        private void OnFilterOptionChanged(object sender, EventArgs eventArgs)
        {
            currentValueOnlyTextBox.Enabled = valueMatchRadioButton.Checked;
            currentValueTextBox.Enabled = previousValueTextBox.Enabled = currentAndPreviousMatchRadioButton.Checked;
            incrementTextBox.Enabled = increasedByRadioButton.Checked;
            decrementTextBox.Enabled = decreasedByRadioButton.Checked;
        }

        private void ResetSearch()
        {
            currentValues.Clear();
            previousValues.Clear();
            for (int address = 0; address <= ushort.MaxValue; address++)
            {
                currentValues[(ushort)address] = previousValues[(ushort)address] = memoryMap[(ushort)address];
            }
        }

        private void UpdateValueMaps()
        {
            previousValues = currentValues;

            currentValues = new Dictionary<ushort, byte>();
            foreach (ushort address in previousValues.Keys)
                currentValues[(ushort)address] = memoryMap[(ushort)address];           
        }

        private MemoryMap memoryMap;
        private static Dictionary<ushort, byte> currentValues= new Dictionary<ushort, byte>();
        private static Dictionary<ushort, byte> previousValues = new Dictionary<ushort, byte>();
    }
}
