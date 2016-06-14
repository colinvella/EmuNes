namespace EmuNES.Input
{
    partial class InputOptionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputOptionsForm));
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.configureTextbox = new System.Windows.Forms.TextBox();
            this.configureButton = new System.Windows.Forms.Button();
            this.controllerIdComboBox = new System.Windows.Forms.ComboBox();
            this.controllerTypeComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(327, 292);
            this.okButton.Margin = new System.Windows.Forms.Padding(2);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(48, 26);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(380, 292);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(48, 26);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // configureTextbox
            // 
            this.configureTextbox.Location = new System.Drawing.Point(12, 111);
            this.configureTextbox.Margin = new System.Windows.Forms.Padding(2);
            this.configureTextbox.Name = "configureTextbox";
            this.configureTextbox.ReadOnly = true;
            this.configureTextbox.Size = new System.Drawing.Size(76, 20);
            this.configureTextbox.TabIndex = 2;
            this.configureTextbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            // 
            // configureButton
            // 
            this.configureButton.Location = new System.Drawing.Point(12, 58);
            this.configureButton.Margin = new System.Windows.Forms.Padding(2);
            this.configureButton.Name = "configureButton";
            this.configureButton.Size = new System.Drawing.Size(72, 26);
            this.configureButton.TabIndex = 0;
            this.configureButton.Text = "&Configure...";
            this.configureButton.UseVisualStyleBackColor = true;
            this.configureButton.Click += new System.EventHandler(this.OnConfigureController);
            // 
            // controllerIdComboBox
            // 
            this.controllerIdComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.controllerIdComboBox.FormattingEnabled = true;
            this.controllerIdComboBox.Items.AddRange(new object[] {
            "Controller 1",
            "Controller 2",
            "Controller 3",
            "Controller 4"});
            this.controllerIdComboBox.Location = new System.Drawing.Point(12, 12);
            this.controllerIdComboBox.Name = "controllerIdComboBox";
            this.controllerIdComboBox.Size = new System.Drawing.Size(96, 21);
            this.controllerIdComboBox.TabIndex = 9;
            // 
            // controllerTypeComboBox
            // 
            this.controllerTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.controllerTypeComboBox.FormattingEnabled = true;
            this.controllerTypeComboBox.Items.AddRange(new object[] {
            "Joypad",
            "Zapper"});
            this.controllerTypeComboBox.Location = new System.Drawing.Point(114, 12);
            this.controllerTypeComboBox.Name = "controllerTypeComboBox";
            this.controllerTypeComboBox.Size = new System.Drawing.Size(96, 21);
            this.controllerTypeComboBox.TabIndex = 10;
            // 
            // InputOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 327);
            this.Controls.Add(this.controllerTypeComboBox);
            this.Controls.Add(this.controllerIdComboBox);
            this.Controls.Add(this.configureTextbox);
            this.Controls.Add(this.configureButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputOptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Input Options";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button configureButton;
        private System.Windows.Forms.TextBox configureTextbox;
        private System.Windows.Forms.ComboBox controllerIdComboBox;
        private System.Windows.Forms.ComboBox controllerTypeComboBox;
    }
}