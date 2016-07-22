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
    public partial class CheatsForm : Form
    {
        public CheatsForm(MemoryMap memoryMap, CheatSystem cheatSystem)
        {
            InitializeComponent();

            this.memoryMap = memoryMap;
            this.cheatSystem = cheatSystem;
        }

        private void OnFormLoad(object sender, EventArgs eventArgs)
        {
            UpdateCheatListBox();
        }

        private void OnCheatsListMouseDown(object sender, MouseEventArgs e)
        {
            cheatsCheckedListBox.SelectedIndex = cheatsCheckedListBox.IndexFromPoint(e.Location);
            selectedCheat = (Cheat)cheatsCheckedListBox.SelectedItem;
        }

        private void OnCheatItemCheck(object sender, ItemCheckEventArgs itemCheckEventArgs)
        {
            if (selectedCheat == null)
                return;
            selectedCheat.Active = itemCheckEventArgs.NewValue == CheckState.Checked;
        }

        private void OnCheatContextMenuOpening(object sender, CancelEventArgs cancelEventArgs)
        {
            bool cheatSelected = cheatsCheckedListBox.SelectedIndex >= 0;
            cheatEditMenuItem.Enabled = cheatDeleteMenuItem.Enabled
                = cheatGenerateGameGenieCodeMenuItem.Enabled = cheatSelected;
            cheatActivateAllMenuItem.Enabled = cheatDeactivateAllMenuItem.Enabled = cheatSystem.Cheats.Count() > 0;
        }

        private void OnCheatNewManualEntry(object sender, EventArgs eventArgs)
        {
            Cheat newCheat = new Cheat();
            CheatDetailsForm cheatDetailsForm = new CheatDetailsForm(newCheat, true);
            if (cheatDetailsForm.ShowDialog() == DialogResult.Cancel)
                return;

            cheatSystem.AddCheat(newCheat);

            UpdateCheatListBox();
        }

        private void OnCheatNewGameGenieCode(object sender, EventArgs eventArgs)
        {
            GameGenieEntryForm gameGenieEntryForm = new GameGenieEntryForm(cheatSystem);
            if (gameGenieEntryForm.ShowDialog() == DialogResult.Cancel)
                return;
            UpdateCheatListBox();
        }

        private void OnCheatEdit(object sender, EventArgs eventArgs)
        {
            CheatDetailsForm cheatDetailsForm = new CheatDetailsForm(selectedCheat, false);
            if (cheatDetailsForm.ShowDialog() == DialogResult.Cancel)
                return;
            UpdateCheatListBox();
        }

        private void OnCheatActivateAll(object sender, EventArgs eventArgs)
        {
            foreach (Cheat cheat in cheatSystem.Cheats)
                cheat.Active = true;
            UpdateCheatListBox();
        }

        private void OnCheatDeactivateAll(object sender, EventArgs eventArgs)
        {
            foreach (Cheat cheat in cheatSystem.Cheats)
                cheat.Active = false;
            UpdateCheatListBox();
        }

        private void OnGenerateGameGenieCode(object sender, EventArgs eventArgs)
        {
            if (selectedCheat == null)
                return;

            string message = selectedCheat.Address >= 0x8000
                ? selectedCheat.GameGenieCode
                : "Cannot generate codes for addresses in the lower 32K region";

            MessageBox.Show(this, message, "Game Genie Code",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnCheatDelete(object sender, EventArgs eventArgs)
        {
            if (selectedCheat == null)
                return;

            if (MessageBox.Show(this,
                "Delete cheat '" + selectedCheat.Description + "'?", "Delete Cheat",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            cheatSystem.RemoveCheat(selectedCheat.Address);
            UpdateCheatListBox();
        }

        private void OnCheatFind(object sender, EventArgs eventArgs)
        {
            CheatFinderForm cheatFinderForm = new CheatFinderForm(memoryMap, cheatSystem);
            cheatFinderForm.ShowDialog(this);
            UpdateCheatListBox();
        }

        private void UpdateCheatListBox()
        {
            cheatsCheckedListBox.Items.Clear();
            foreach (Cheat cheat in cheatSystem.Cheats)
            {
                cheatsCheckedListBox.Items.Add(cheat, cheat.Active ? CheckState.Checked : CheckState.Unchecked);
            }
        }

        private MemoryMap memoryMap;
        private CheatSystem cheatSystem;
        private Cheat selectedCheat;
    }
}
