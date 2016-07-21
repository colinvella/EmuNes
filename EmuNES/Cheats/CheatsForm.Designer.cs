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
            this.cheatsGroupBox = new System.Windows.Forms.GroupBox();
            this.cheatsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.cheatsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cheatNewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatEditMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatDeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.cheatActivateAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cheatDeactivateAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cheatsGroupBox.SuspendLayout();
            this.cheatsContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // cheatsGroupBox
            // 
            this.cheatsGroupBox.Controls.Add(this.cheatsCheckedListBox);
            this.cheatsGroupBox.Location = new System.Drawing.Point(12, 12);
            this.cheatsGroupBox.Name = "cheatsGroupBox";
            this.cheatsGroupBox.Size = new System.Drawing.Size(360, 173);
            this.cheatsGroupBox.TabIndex = 0;
            this.cheatsGroupBox.TabStop = false;
            this.cheatsGroupBox.Text = "Cheats";
            // 
            // cheatsCheckedListBox
            // 
            this.cheatsCheckedListBox.ContextMenuStrip = this.cheatsContextMenuStrip;
            this.cheatsCheckedListBox.FormattingEnabled = true;
            this.cheatsCheckedListBox.Location = new System.Drawing.Point(6, 13);
            this.cheatsCheckedListBox.Name = "cheatsCheckedListBox";
            this.cheatsCheckedListBox.Size = new System.Drawing.Size(348, 154);
            this.cheatsCheckedListBox.TabIndex = 0;
            this.cheatsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OnCheatItemCheck);
            this.cheatsCheckedListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnCheatsListMouseDown);
            // 
            // cheatsContextMenuStrip
            // 
            this.cheatsContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cheatNewMenuItem,
            this.cheatEditMenuItem,
            toolStripSeparator1,
            this.cheatActivateAllMenuItem,
            this.cheatDeactivateAllMenuItem,
            toolStripSeparator2,
            this.cheatDeleteMenuItem});
            this.cheatsContextMenuStrip.Name = "cheatsContextMenuStrip";
            this.cheatsContextMenuStrip.Size = new System.Drawing.Size(153, 148);
            this.cheatsContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.OnCheatContextMenuOpening);
            // 
            // cheatNewMenuItem
            // 
            this.cheatNewMenuItem.Name = "cheatNewMenuItem";
            this.cheatNewMenuItem.Size = new System.Drawing.Size(152, 22);
            this.cheatNewMenuItem.Text = "&New...";
            this.cheatNewMenuItem.Click += new System.EventHandler(this.OnCheatNew);
            // 
            // cheatEditMenuItem
            // 
            this.cheatEditMenuItem.Name = "cheatEditMenuItem";
            this.cheatEditMenuItem.Size = new System.Drawing.Size(152, 22);
            this.cheatEditMenuItem.Text = "&Edit...";
            this.cheatEditMenuItem.Click += new System.EventHandler(this.OnCheatEdit);
            // 
            // cheatDeleteMenuItem
            // 
            this.cheatDeleteMenuItem.Name = "cheatDeleteMenuItem";
            this.cheatDeleteMenuItem.Size = new System.Drawing.Size(152, 22);
            this.cheatDeleteMenuItem.Text = "Delete...";
            this.cheatDeleteMenuItem.Click += new System.EventHandler(this.OnCheatDelete);
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point(12, 191);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(360, 129);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Cheat Finder";
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(297, 326);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "&Close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // cheatActivateAllMenuItem
            // 
            this.cheatActivateAllMenuItem.Name = "cheatActivateAllMenuItem";
            this.cheatActivateAllMenuItem.Size = new System.Drawing.Size(152, 22);
            this.cheatActivateAllMenuItem.Text = "Activate All";
            this.cheatActivateAllMenuItem.Click += new System.EventHandler(this.OnCheatActivateAll);
            // 
            // cheatDeactivateAllMenuItem
            // 
            this.cheatDeactivateAllMenuItem.Name = "cheatDeactivateAllMenuItem";
            this.cheatDeactivateAllMenuItem.Size = new System.Drawing.Size(152, 22);
            this.cheatDeactivateAllMenuItem.Text = "Deactivate All";
            this.cheatDeactivateAllMenuItem.Click += new System.EventHandler(this.OnCheatDeactivateAll);
            // 
            // CheatsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(384, 361);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.cheatsGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheatsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cheats";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.cheatsGroupBox.ResumeLayout(false);
            this.cheatsContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox cheatsGroupBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.CheckedListBox cheatsCheckedListBox;
        private System.Windows.Forms.ContextMenuStrip cheatsContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem cheatEditMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatNewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatDeleteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatActivateAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cheatDeactivateAllMenuItem;
    }
}