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
    public partial class CheatDetailsForm : Form
    {
        public CheatDetailsForm(Cheat cheat, bool isNew)
        {
            InitializeComponent();

            this.cheat = cheat;
            this.isNew = isNew;
        }

        private void OnFormLoad(object sender, EventArgs eventArgs)
        {
            if (isNew)
            {
                this.Text = "New Cheat";
            }
            else
            {
                this.Text = "Edit Cheat";
                this.addressTextBox.Text = cheat.Address.ToString("X4");
                this.addressTextBox.ReadOnly = true;
                this.valueTextBox.Text = cheat.Value.ToString("X2");
                this.compareTextBox.Text = cheat.CompareValue.ToString("X2");
                this.compareRequiredCheckBox.Checked = this.compareTextBox.Enabled = cheat.NeedsComparison;
                this.descriptionTextBox.Text = cheat.Description;
            }
        }

        private Cheat cheat;
        private bool isNew;
    }
}
