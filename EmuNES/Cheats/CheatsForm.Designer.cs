namespace SharpNes.Cheats
{
    partial class CheatsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheatsForm));
            this.cheatsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cheatNewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatNewManualEntryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatNewGameGenieMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatEditMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatActivateAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatDeactivateAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatGenerateGameGenieCodeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatDeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeButton = new System.Windows.Forms.Button();
            this.cheatsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.findButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.importButton = new System.Windows.Forms.Button();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cheatsContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(229, 6);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(229, 6);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(229, 6);
            // 
            // cheatsContextMenuStrip
            // 
            this.cheatsContextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cheatsContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cheatNewMenuItem,
            this.cheatEditMenuItem,
            toolStripSeparator1,
            this.cheatActivateAllMenuItem,
            this.cheatDeactivateAllMenuItem,
            toolStripSeparator2,
            this.cheatGenerateGameGenieCodeMenuItem,
            toolStripSeparator3,
            this.cheatDeleteMenuItem});
            this.cheatsContextMenuStrip.Name = "cheatsContextMenuStrip";
            this.cheatsContextMenuStrip.Size = new System.Drawing.Size(233, 178);
            this.cheatsContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.OnCheatContextMenuOpening);
            // 
            // cheatNewMenuItem
            // 
            this.cheatNewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cheatNewManualEntryMenuItem,
            this.cheatNewGameGenieMenuItem});
            this.cheatNewMenuItem.Name = "cheatNewMenuItem";
            this.cheatNewMenuItem.Size = new System.Drawing.Size(232, 26);
            this.cheatNewMenuItem.Text = "&New...";
            // 
            // cheatNewManualEntryMenuItem
            // 
            this.cheatNewManualEntryMenuItem.Name = "cheatNewManualEntryMenuItem";
            this.cheatNewManualEntryMenuItem.Size = new System.Drawing.Size(182, 26);
            this.cheatNewManualEntryMenuItem.Text = "Manual Entry...";
            this.cheatNewManualEntryMenuItem.Click += new System.EventHandler(this.OnCheatNewManualEntry);
            // 
            // cheatNewGameGenieMenuItem
            // 
            this.cheatNewGameGenieMenuItem.Image = global::SharpNes.Properties.Resources.OptionsCheatsGameGenie;
            this.cheatNewGameGenieMenuItem.Name = "cheatNewGameGenieMenuItem";
            this.cheatNewGameGenieMenuItem.Size = new System.Drawing.Size(182, 26);
            this.cheatNewGameGenieMenuItem.Text = "Game Genie Code...";
            this.cheatNewGameGenieMenuItem.Click += new System.EventHandler(this.OnCheatNewGameGenieCode);
            // 
            // cheatEditMenuItem
            // 
            this.cheatEditMenuItem.Name = "cheatEditMenuItem";
            this.cheatEditMenuItem.Size = new System.Drawing.Size(232, 26);
            this.cheatEditMenuItem.Text = "&Edit...";
            this.cheatEditMenuItem.Click += new System.EventHandler(this.OnCheatEdit);
            // 
            // cheatActivateAllMenuItem
            // 
            this.cheatActivateAllMenuItem.Name = "cheatActivateAllMenuItem";
            this.cheatActivateAllMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.cheatActivateAllMenuItem.Size = new System.Drawing.Size(232, 26);
            this.cheatActivateAllMenuItem.Text = "Activate All";
            this.cheatActivateAllMenuItem.Click += new System.EventHandler(this.OnCheatActivateAll);
            // 
            // cheatDeactivateAllMenuItem
            // 
            this.cheatDeactivateAllMenuItem.Name = "cheatDeactivateAllMenuItem";
            this.cheatDeactivateAllMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Space)));
            this.cheatDeactivateAllMenuItem.Size = new System.Drawing.Size(232, 26);
            this.cheatDeactivateAllMenuItem.Text = "Deactivate All";
            this.cheatDeactivateAllMenuItem.Click += new System.EventHandler(this.OnCheatDeactivateAll);
            // 
            // cheatGenerateGameGenieCodeMenuItem
            // 
            this.cheatGenerateGameGenieCodeMenuItem.Image = global::SharpNes.Properties.Resources.OptionsCheatsGameGenie;
            this.cheatGenerateGameGenieCodeMenuItem.Name = "cheatGenerateGameGenieCodeMenuItem";
            this.cheatGenerateGameGenieCodeMenuItem.Size = new System.Drawing.Size(232, 26);
            this.cheatGenerateGameGenieCodeMenuItem.Text = "Generate Game &Genie Code...";
            this.cheatGenerateGameGenieCodeMenuItem.Click += new System.EventHandler(this.OnGenerateGameGenieCode);
            // 
            // cheatDeleteMenuItem
            // 
            this.cheatDeleteMenuItem.Name = "cheatDeleteMenuItem";
            this.cheatDeleteMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.cheatDeleteMenuItem.Size = new System.Drawing.Size(232, 26);
            this.cheatDeleteMenuItem.Text = "Delete...";
            this.cheatDeleteMenuItem.Click += new System.EventHandler(this.OnCheatDelete);
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(297, 226);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "&Close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // cheatsCheckedListBox
            // 
            this.cheatsCheckedListBox.CheckOnClick = true;
            this.cheatsCheckedListBox.ContextMenuStrip = this.cheatsContextMenuStrip;
            this.cheatsCheckedListBox.FormattingEnabled = true;
            this.cheatsCheckedListBox.Location = new System.Drawing.Point(12, 12);
            this.cheatsCheckedListBox.Name = "cheatsCheckedListBox";
            this.cheatsCheckedListBox.Size = new System.Drawing.Size(360, 199);
            this.cheatsCheckedListBox.TabIndex = 0;
            this.cheatsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OnCheatItemCheck);
            this.cheatsCheckedListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnCheatsListMouseDown);
            // 
            // findButton
            // 
            this.findButton.Location = new System.Drawing.Point(216, 226);
            this.findButton.Name = "findButton";
            this.findButton.Size = new System.Drawing.Size(75, 23);
            this.findButton.TabIndex = 3;
            this.findButton.Text = "&Find...";
            this.findButton.UseVisualStyleBackColor = true;
            this.findButton.Click += new System.EventHandler(this.OnCheatFind);
            // 
            // exportButton
            // 
            this.exportButton.Location = new System.Drawing.Point(135, 226);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 2;
            this.exportButton.Text = "&Export...";
            this.exportButton.UseVisualStyleBackColor = true;
            // 
            // importButton
            // 
            this.importButton.Location = new System.Drawing.Point(54, 226);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(75, 23);
            this.importButton.TabIndex = 1;
            this.importButton.Text = "&Import...";
            this.importButton.UseVisualStyleBackColor = true;
            // 
            // CheatsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(384, 261);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.findButton);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.cheatsCheckedListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheatsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cheats";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.cheatsContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.ContextMenuStrip cheatsContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem cheatEditMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatNewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatDeleteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatActivateAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatDeactivateAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatNewManualEntryMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatNewGameGenieMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatGenerateGameGenieCodeMenuItem;
        private System.Windows.Forms.CheckedListBox cheatsCheckedListBox;
        private System.Windows.Forms.Button findButton;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button importButton;
    }
}