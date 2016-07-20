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

        private void UpdateCheatListBox()
        {
            cheatsCheckedListBox.Items.Clear();
            foreach (Cheat cheat in cheatSystem.Cheats)
            {
                cheatsCheckedListBox.Items.Add(cheat, cheat.Active ? CheckState.Checked : CheckState.Unchecked);
            }
        }

        private void OnCheatItemCheck(object sender, ItemCheckEventArgs itemCheckEventArgs)
        {
            Cheat cheat = (Cheat)cheatsCheckedListBox.Items[itemCheckEventArgs.Index];
            cheat.Active = itemCheckEventArgs.NewValue == CheckState.Checked;
        }

        private CheatSystem cheatSystem;
    }
}
