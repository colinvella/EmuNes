using NesCore.Memory;
using NesCore.Utility;
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
            this.cheatSystem = cheatSystem;

            this.searchType = -1;

            if (currentValues.Count == 0 && previousValues.Count == 0)
                ResetValueMaps();

            UpdateValueMaps();
            PopulateResultsList();
        }

        private void OnFilterOptionChanged(object sender, EventArgs eventArgs)
        {
            currentValueOnlyTextBox.Enabled = valueMatchRadioButton.Checked;
            currentValueTextBox.Enabled = previousValueTextBox.Enabled = currentAndPreviousMatchRadioButton.Checked;
            incrementTextBox.Enabled = increasedByRadioButton.Checked;
            decrementTextBox.Enabled = decreasedByRadioButton.Checked;

            if (valueMatchRadioButton.Checked)
                searchType = 0;
            else if (currentAndPreviousMatchRadioButton.Checked)
                searchType = 1;
            else if (increasedByRadioButton.Checked)
                searchType = 2;
            else if (decreasedByRadioButton.Checked)
                searchType = 3;
            else if (increasedRadioButton.Checked)
                searchType = 4;
            else if (decreasedRadioButton.Checked)
                searchType = 5;
            else
                searchType = -1;

            applyButton.Enabled = true;
        }

        private void OnSearchReset(object sender, EventArgs eventArgs)
        {
            ResetValueMaps();
            valueMatchRadioButton.Checked = currentAndPreviousMatchRadioButton.Checked
                = increasedByRadioButton.Checked = decreasedByRadioButton.Checked
                = increasedRadioButton.Checked = decreasedRadioButton.Checked 
                = applyButton.Enabled = false;
            OnFilterOptionChanged(this, eventArgs);
            PopulateResultsList();
        }

        private void OnSearchApply(object sender, EventArgs eventArgs)
        {
            switch (searchType)
            {
                case 0:
                    SearchCurrentValueOnly(
                        Convert.ToByte(currentValueOnlyTextBox.Text, 16));
                    break;
                case 1:
                    SearchCurrentAndPreviousValues(
                        Convert.ToByte(currentValueTextBox.Text, 16),
                        Convert.ToByte(previousValueTextBox.Text, 16));
                    break;
                case 2:
                    SearchValueIncreasedBy(Convert.ToByte(incrementTextBox.Text, 16));
                    break;
                case 3:
                    SearchValueDecreasedBy(Convert.ToByte(decrementTextBox.Text, 16));
                    break;
                case 4:
                    SearchValueIncreased();
                    break;
                case 5:
                    SearchValueDecreased();
                    break;
            }
            if (searchType >= 0)
            {
                valuesRestricted = true;
                PopulateResultsList();
            }
        }

        private void OnFormContinue(object sender, EventArgs e)
        {
            if (valuesRestricted)
            {
                Dictionary<ushort, byte> restrictedCurrentValues = new Dictionary<ushort, byte>();
                Dictionary<ushort, byte> restrictedPreviousValues = new Dictionary<ushort, byte>();

                foreach (ushort address in searchResults)
                {
                    restrictedCurrentValues[address] = currentValues[address];
                    restrictedPreviousValues[address] = previousValues[address];
                }

                currentValues = restrictedCurrentValues;
                previousValues = restrictedPreviousValues;
            }

            Close();
        }

        private void OnValidatingByteTextBox(object sender, CancelEventArgs cancelEventArgs)
        {
            TextBox textBox = (TextBox)sender;
            try
            {
                string byteText = textBox.Text.Trim();
                if (byteText.Length > 0)
                    Convert.ToByte(byteText, 16);
                else textBox.Text = "00";
            }
            catch (Exception)
            {
                errorProvider.SetError(textBox, "Invalid hex byte value");
                cancelEventArgs.Cancel = true;
            }
        }

        private void OnValidatedByteTextBox(object sender, EventArgs eventArgs)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Text = textBox.Text.ToUpper().PadLeft(2, '0');
            errorProvider.SetError(textBox, "");
        }

        private void OnOpeningContextMenu(object sender, CancelEventArgs cancelEventArgs)
        {
            createCheatMenuItem.Enabled = resultListBox.SelectedIndex >= 0;
        }

        private void OnCreateCheatMenuItem(object sender, EventArgs eventArgs)
        {
            if (resultListBox.SelectedIndex < 0)
                return;

            Cheat newCheat = new Cheat();
            newCheat.Address = Convert.ToUInt16(
                resultListBox.SelectedItem.ToString().Substring(0, 4), 16);
            CheatDetailsForm cheatDetailsForm = new CheatDetailsForm(newCheat, false);

            if (cheatDetailsForm.ShowDialog() == DialogResult.Cancel)
                return;

            cheatSystem.AddCheat(newCheat);
        }

        private void PopulateResultsList()
        {
            resultListBox.BeginUpdate();
            resultListBox.Items.Clear();
            int count = 0; ;
            foreach (ushort address in searchResults)
            {
                resultListBox.Items.Add(address.ToString("X4") + ": " + currentValues[address].ToString("X2"));
                if (++count == 256)
                {
                    resultListBox.Items.Add("...");
                    break;
                }
            }
            resultListBox.EndUpdate();
        }

        private void ResetValueMaps()
        {
            searchResults.Clear();
            currentValues.Clear();
            previousValues.Clear();
            for (int address = 0; address <= ushort.MaxValue; address++)
            {
                currentValues[(ushort)address] = previousValues[(ushort)address] = memoryMap[(ushort)address];
                searchResults.Add((ushort)address);
            }
        }

        private void UpdateValueMaps()
        {
            previousValues = currentValues;

            currentValues = new Dictionary<ushort, byte>();
            foreach (ushort address in previousValues.Keys)
                currentValues[(ushort)address] = memoryMap[(ushort)address];
        }

        private void SearchCurrentValueOnly(byte value)
        {
            searchResults.Clear();
            foreach (ushort address in currentValues.Keys)
                if (currentValues[address] == value)
                    searchResults.Add(address);
        }

        private void SearchCurrentAndPreviousValues(byte currentValue, byte previousValue)
        {
            searchResults.Clear();
            foreach (ushort address in currentValues.Keys)
                if (currentValues[address] == currentValue
                    && previousValues[address] == previousValue)
                    searchResults.Add(address);
        }

        private void SearchValueIncreasedBy(byte increase)
        {
            searchResults.Clear();
            foreach (ushort address in currentValues.Keys)
                if (currentValues[address] == (byte)(previousValues[address] + increase))
                    searchResults.Add(address);
        }

        private void SearchValueDecreasedBy(byte decrease)
        {
            searchResults.Clear();
            foreach (ushort address in currentValues.Keys)
                if (currentValues[address] == (byte)(previousValues[address] - decrease))
                    searchResults.Add(address);
        }

        private void SearchValueIncreased()
        {
            searchResults.Clear();
            foreach (ushort address in currentValues.Keys)
                if (currentValues[address] > previousValues[address])
                    searchResults.Add(address);
        }

        private void SearchValueDecreased()
        {
            searchResults.Clear();
            foreach (ushort address in currentValues.Keys)
                if (currentValues[address] < previousValues[address])
                    searchResults.Add(address);
        }

        private CheatSystem cheatSystem;
        private MemoryMap memoryMap;
        private int searchType;
        private bool valuesRestricted;

        private static Dictionary<ushort, byte> currentValues= new Dictionary<ushort, byte>();
        private static Dictionary<ushort, byte> previousValues = new Dictionary<ushort, byte>();
        private static HashSet<ushort> searchResults = new HashSet<ushort>();
    }
}
