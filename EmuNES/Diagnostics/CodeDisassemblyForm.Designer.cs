namespace SharpNes.Diagnostics
{
    partial class CodeDisassemblyForm
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
            this.disassemblyTimer = new System.Windows.Forms.Timer(this.components);
            this.disassemblyRichTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // disassemblyTimer
            // 
            this.disassemblyTimer.Enabled = true;
            this.disassemblyTimer.Tick += new System.EventHandler(this.OnDisassemblyTick);
            // 
            // disassemblyRichTextBox
            // 
            this.disassemblyRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.disassemblyRichTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.disassemblyRichTextBox.Location = new System.Drawing.Point(0, 0);
            this.disassemblyRichTextBox.Name = "disassemblyRichTextBox";
            this.disassemblyRichTextBox.ReadOnly = true;
            this.disassemblyRichTextBox.Size = new System.Drawing.Size(436, 327);
            this.disassemblyRichTextBox.TabIndex = 0;
            this.disassemblyRichTextBox.Text = "";
            this.disassemblyRichTextBox.WordWrap = false;
            // 
            // CodeDisassemblyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 327);
            this.ControlBox = false;
            this.Controls.Add(this.disassemblyRichTextBox);
            this.Name = "CodeDisassemblyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Code Disassembly";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer disassemblyTimer;
        private System.Windows.Forms.RichTextBox disassemblyRichTextBox;
    }
}