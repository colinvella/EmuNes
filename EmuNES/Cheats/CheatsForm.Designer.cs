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
            this.cheatsGroupBox = new System.Windows.Forms.GroupBox();
            this.cheatsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.cheatsGroupBox.SuspendLayout();
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
            this.cheatsCheckedListBox.FormattingEnabled = true;
            this.cheatsCheckedListBox.Location = new System.Drawing.Point(6, 13);
            this.cheatsCheckedListBox.Name = "cheatsCheckedListBox";
            this.cheatsCheckedListBox.Size = new System.Drawing.Size(348, 154);
            this.cheatsCheckedListBox.TabIndex = 0;
            this.cheatsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OnCheatItemCheck);
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox cheatsGroupBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.CheckedListBox cheatsCheckedListBox;
    }
}