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
        public CheatsForm(CheatSystem cheatSystem)
        {
            InitializeComponent();

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
            selectedCheat.Active = itemCheckEventArgs.NewValue == CheckState.Checked;
        }

        private void OnCheatContextMenuOpening(object sender, CancelEventArgs cancelEventArgs)
        {
            bool cheatSelected = cheatsCheckedListBox.SelectedIndex >= 0;
            cheatEditMenuItem.Enabled = cheatDeleteMenuItem.Enabled = cheatSelected;
            cheatNewMenuItem.Enabled = !cheatSelected;
        }

        private void OnCheatNew(object sender, EventArgs eventArgs)
        {
            Cheat newCheat = new Cheat();
            CheatDetailsForm cheatDetailsForm = new CheatDetailsForm(newCheat, true);
            cheatDetailsForm.ShowDialog();

            UpdateCheatListBox();
        }

        private void OnCheatEdit(object sender, EventArgs eventArgs)
        {
            CheatDetailsForm cheatDetailsForm = new CheatDetailsForm(selectedCheat, false);
            cheatDetailsForm.ShowDialog();
        }

        private void OnCheatDelete(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,
                "Delete cheat '" + selectedCheat.Description + "'?", "Delete Cheat",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            cheatSystem.RemoveCheat(selectedCheat.Address);
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

        private CheatSystem cheatSystem;
        private Cheat selectedCheat;

    }
}
