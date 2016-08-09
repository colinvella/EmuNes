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
                this.addressTextBox.ReadOnly = false;
                this.compareTextBox.Enabled = false;
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

        private void OnValidatingAddress(object sender, CancelEventArgs cancelEventArgs)
        {
            try
            {
                Convert.ToUInt16(addressTextBox.Text, 16);
            }
            catch (Exception)
            {
                cancelEventArgs.Cancel = true;
                errorProvider.SetError(addressTextBox, "Address value must be in 4 hex-digit format");
            }
        }

        private void OnCompareRequiredCheckedChanged(object sender, EventArgs eventArgs)
        {
            compareTextBox.Enabled = compareRequiredCheckBox.Checked;
        }

        private void OnValidatedAddress(object sender, EventArgs eventArgs)
        {
            addressTextBox.Text = addressTextBox.Text.ToUpper();
            errorProvider.SetError(addressTextBox, "");
        }

        private void OnValidatingByteValue(object sender, CancelEventArgs cancelEventArgs)
        {
            TextBox textBox = (TextBox)sender;
            if (!textBox.Enabled)
                return;

            try
            {
                Convert.ToByte(textBox.Text, 16);
            }
            catch (Exception)
            {
                cancelEventArgs.Cancel = true;
                errorProvider.SetError(textBox, "Byte value must be in 2 hex-digit format");
            }
        }

        private void OnValidatedByteValue(object sender, EventArgs eventArgs)
        {
            TextBox textBox = (TextBox)sender;
            if (!textBox.Enabled)
                return;

            textBox.Text = textBox.Text.ToUpper();
            errorProvider.SetError(textBox, "");
        }

        private void OnValidatingDescription(object sender, CancelEventArgs cancelEventArgs)
        {
            descriptionTextBox.Text = descriptionTextBox.Text.Trim();
            if (descriptionTextBox.Text.Length == 0)
            {
                cancelEventArgs.Cancel = true;
                errorProvider.SetError(descriptionTextBox, "No description specified");
            }
        }

        private void OnValidatedDescription(object sender, EventArgs eventargs)
        {
            descriptionTextBox.Text = descriptionTextBox.Text.Trim();
        }

        private void OnButtonOk(object sender, EventArgs eventArgs)
        {
            /*
            if (!addressTextBox.Focus() || !valueTextBox.Focus() || !(compareRequiredCheckBox.Enabled && compareRequiredCheckBox.Focus()))
            {
                DialogResult = DialogResult.None;
                return;
            }*/
            if (!ValidateChildren())
            {
                DialogResult = DialogResult.None;
                return;
            }

            cheat.Address = Convert.ToUInt16(addressTextBox.Text, 16);
            cheat.Value = Convert.ToByte(valueTextBox.Text, 16);
            if (compareRequiredCheckBox.Checked)
                cheat.CompareValue = Convert.ToByte(compareTextBox.Text, 16);
            else
                cheat.CompareValue = 0;
            cheat.NeedsComparison = compareRequiredCheckBox.Checked;
            cheat.Description = descriptionTextBox.Text;
            this.Close();
        }

        private Cheat cheat;
        private bool isNew;
    }
}
