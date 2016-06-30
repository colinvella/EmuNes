namespace SharpNes.Input
{
    partial class JoypadConfigurationForm
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
            this.configurationLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // configurationLabel
            // 
            this.configurationLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.configurationLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configurationLabel.Location = new System.Drawing.Point(0, 0);
            this.configurationLabel.Margin = new System.Windows.Forms.Padding(0);
            this.configurationLabel.Name = "configurationLabel";
            this.configurationLabel.Size = new System.Drawing.Size(184, 61);
            this.configurationLabel.TabIndex = 0;
            this.configurationLabel.Text = "Quick Configuration";
            this.configurationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // QuickConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(184, 61);
            this.Controls.Add(this.configurationLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "QuickConfigurationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ControllerQuickConfigurationForm";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label configurationLabel;
    }
}