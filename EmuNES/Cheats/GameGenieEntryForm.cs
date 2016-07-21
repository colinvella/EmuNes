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
    public partial class GameGenieEntryForm : Form
    {
        public GameGenieEntryForm(CheatSystem cheatSystem)
        {
            InitializeComponent();

            this.cheatSystem = cheatSystem;
        }

        private void OnValidatingGameGenieCode(object sender, CancelEventArgs cancelEventArgs)
        {
            string gameGenieCode = gameGenieCodeTextBox.Text.Trim().ToUpper();
            if (gameGenieCode.Length > 0 && !Cheat.IsValidGameGenieCode(gameGenieCode))
            {
                errorProvider.SetError(gameGenieCodeTextBox, "Invalid Game Genie code");
                cancelEventArgs.Cancel = true;
            }
        }

        private void OnValidatedGameGenieCode(object sender, EventArgs eventArgs)
        {
            gameGenieCodeTextBox.Text = gameGenieCodeTextBox.Text.ToUpper();
            errorProvider.SetError(gameGenieCodeTextBox, "");
        }

        private void OnFormOk(object sender, EventArgs eventArgs)
        {
            string gameGenieCode = gameGenieCodeTextBox.Text;
            cheatSystem.AddCheat(gameGenieCode);
            Close();
        }

        private CheatSystem cheatSystem;
    }
}
