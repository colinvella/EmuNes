namespace SharpNes.Input
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
            this.configureJoypadButton = new System.Windows.Forms.Button();
            this.controllerIdComboBox = new System.Windows.Forms.ComboBox();
            this.configureZapperButton = new System.Windows.Forms.Button();
            this.mappingsPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(236, 310);
            this.okButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(64, 32);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OnOk);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(306, 310);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(64, 32);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // configureJoypadButton
            // 
            this.configureJoypadButton.Location = new System.Drawing.Point(12, 43);
            this.configureJoypadButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.configureJoypadButton.Name = "configureJoypadButton";
            this.configureJoypadButton.Size = new System.Drawing.Size(145, 32);
            this.configureJoypadButton.TabIndex = 0;
            this.configureJoypadButton.Text = "Configure &Joypad...";
            this.configureJoypadButton.UseVisualStyleBackColor = true;
            this.configureJoypadButton.Click += new System.EventHandler(this.OnConfigureJoypad);
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
            this.controllerIdComboBox.Location = new System.Drawing.Point(13, 13);
            this.controllerIdComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.controllerIdComboBox.Name = "controllerIdComboBox";
            this.controllerIdComboBox.Size = new System.Drawing.Size(144, 24);
            this.controllerIdComboBox.TabIndex = 9;
            this.controllerIdComboBox.SelectedIndexChanged += new System.EventHandler(this.OnPortChanged);
            // 
            // configureZapperButton
            // 
            this.configureZapperButton.Location = new System.Drawing.Point(12, 79);
            this.configureZapperButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.configureZapperButton.Name = "configureZapperButton";
            this.configureZapperButton.Size = new System.Drawing.Size(145, 32);
            this.configureZapperButton.TabIndex = 10;
            this.configureZapperButton.Text = "Configure &Zapper...";
            this.configureZapperButton.UseVisualStyleBackColor = true;
            this.configureZapperButton.Click += new System.EventHandler(this.OnConfigureZapper);
            // 
            // mappingsPropertyGrid
            // 
            this.mappingsPropertyGrid.HelpVisible = false;
            this.mappingsPropertyGrid.Location = new System.Drawing.Point(164, 13);
            this.mappingsPropertyGrid.Name = "mappingsPropertyGrid";
            this.mappingsPropertyGrid.Size = new System.Drawing.Size(206, 292);
            this.mappingsPropertyGrid.TabIndex = 11;
            // 
            // InputOptionsForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(382, 353);
            this.Controls.Add(this.mappingsPropertyGrid);
            this.Controls.Add(this.configureZapperButton);
            this.Controls.Add(this.controllerIdComboBox);
            this.Controls.Add(this.configureJoypadButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputOptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Input Options";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button configureJoypadButton;
        private System.Windows.Forms.ComboBox controllerIdComboBox;
        private System.Windows.Forms.Button configureZapperButton;
        private System.Windows.Forms.PropertyGrid mappingsPropertyGrid;
    }
}