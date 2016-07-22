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
        }

        private void OnFilterOptionChanged(object sender, EventArgs eventArgs)
        {
            currentValueOnlyTextBox.Enabled = valueMatchRadioButton.Checked;
            currentValueTextBox.Enabled = previousValueTextBox.Enabled = currentAndPreviousMatchRadioButton.Checked;
            incrementTextBox.Enabled = increasedByRadioButton.Checked;
            decrementTextBox.Enabled = decreasedByRadioButton.Checked;
        }
    }
}
