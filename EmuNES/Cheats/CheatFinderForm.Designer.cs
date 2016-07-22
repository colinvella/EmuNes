namespace SharpNes.Cheats
{
    partial class CheatFinderForm
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
            this.valueMatchRadioButton = new System.Windows.Forms.RadioButton();
            this.currentValueOnlyTextBox = new System.Windows.Forms.TextBox();
            this.currentAndPreviousMatchRadioButton = new System.Windows.Forms.RadioButton();
            this.currentValueTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.previousValueTextBox = new System.Windows.Forms.TextBox();
            this.increasedByRadioButton = new System.Windows.Forms.RadioButton();
            this.incrementTextBox = new System.Windows.Forms.TextBox();
            this.decrementTextBox = new System.Windows.Forms.TextBox();
            this.decreasedByRadioButton = new System.Windows.Forms.RadioButton();
            this.decreasedRadioButton = new System.Windows.Forms.RadioButton();
            this.increasedRadioButton = new System.Windows.Forms.RadioButton();
            this.filterGroupBox = new System.Windows.Forms.GroupBox();
            this.resultRistBox = new System.Windows.Forms.ListBox();
            this.filterGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // valueMatchRadioButton
            // 
            this.valueMatchRadioButton.AutoSize = true;
            this.valueMatchRadioButton.Location = new System.Drawing.Point(6, 19);
            this.valueMatchRadioButton.Name = "valueMatchRadioButton";
            this.valueMatchRadioButton.Size = new System.Drawing.Size(101, 17);
            this.valueMatchRadioButton.TabIndex = 0;
            this.valueMatchRadioButton.TabStop = true;
            this.valueMatchRadioButton.Text = "Current value is ";
            this.valueMatchRadioButton.UseVisualStyleBackColor = true;
            this.valueMatchRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // currentValueOnlyTextBox
            // 
            this.currentValueOnlyTextBox.Enabled = false;
            this.currentValueOnlyTextBox.Location = new System.Drawing.Point(105, 18);
            this.currentValueOnlyTextBox.MaxLength = 2;
            this.currentValueOnlyTextBox.Name = "currentValueOnlyTextBox";
            this.currentValueOnlyTextBox.Size = new System.Drawing.Size(24, 20);
            this.currentValueOnlyTextBox.TabIndex = 1;
            // 
            // currentAndPreviousMatchRadioButton
            // 
            this.currentAndPreviousMatchRadioButton.AutoSize = true;
            this.currentAndPreviousMatchRadioButton.Location = new System.Drawing.Point(6, 45);
            this.currentAndPreviousMatchRadioButton.Name = "currentAndPreviousMatchRadioButton";
            this.currentAndPreviousMatchRadioButton.Size = new System.Drawing.Size(101, 17);
            this.currentAndPreviousMatchRadioButton.TabIndex = 2;
            this.currentAndPreviousMatchRadioButton.TabStop = true;
            this.currentAndPreviousMatchRadioButton.Text = "Current value is ";
            this.currentAndPreviousMatchRadioButton.UseVisualStyleBackColor = true;
            this.currentAndPreviousMatchRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // currentValueTextBox
            // 
            this.currentValueTextBox.Enabled = false;
            this.currentValueTextBox.Location = new System.Drawing.Point(105, 44);
            this.currentValueTextBox.MaxLength = 2;
            this.currentValueTextBox.Name = "currentValueTextBox";
            this.currentValueTextBox.Size = new System.Drawing.Size(24, 20);
            this.currentValueTextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(131, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "and previous value is";
            // 
            // previousValueTextBox
            // 
            this.previousValueTextBox.Enabled = false;
            this.previousValueTextBox.Location = new System.Drawing.Point(238, 44);
            this.previousValueTextBox.MaxLength = 2;
            this.previousValueTextBox.Name = "previousValueTextBox";
            this.previousValueTextBox.Size = new System.Drawing.Size(24, 20);
            this.previousValueTextBox.TabIndex = 5;
            // 
            // increasedByRadioButton
            // 
            this.increasedByRadioButton.AutoSize = true;
            this.increasedByRadioButton.Location = new System.Drawing.Point(6, 71);
            this.increasedByRadioButton.Name = "increasedByRadioButton";
            this.increasedByRadioButton.Size = new System.Drawing.Size(115, 17);
            this.increasedByRadioButton.TabIndex = 6;
            this.increasedByRadioButton.TabStop = true;
            this.increasedByRadioButton.Text = "Value increased by";
            this.increasedByRadioButton.UseVisualStyleBackColor = true;
            this.increasedByRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // incrementTextBox
            // 
            this.incrementTextBox.Enabled = false;
            this.incrementTextBox.Location = new System.Drawing.Point(123, 70);
            this.incrementTextBox.MaxLength = 2;
            this.incrementTextBox.Name = "incrementTextBox";
            this.incrementTextBox.Size = new System.Drawing.Size(24, 20);
            this.incrementTextBox.TabIndex = 7;
            // 
            // decrementTextBox
            // 
            this.decrementTextBox.Enabled = false;
            this.decrementTextBox.Location = new System.Drawing.Point(123, 96);
            this.decrementTextBox.MaxLength = 2;
            this.decrementTextBox.Name = "decrementTextBox";
            this.decrementTextBox.Size = new System.Drawing.Size(24, 20);
            this.decrementTextBox.TabIndex = 9;
            // 
            // decreasedByRadioButton
            // 
            this.decreasedByRadioButton.AutoSize = true;
            this.decreasedByRadioButton.Location = new System.Drawing.Point(6, 97);
            this.decreasedByRadioButton.Name = "decreasedByRadioButton";
            this.decreasedByRadioButton.Size = new System.Drawing.Size(119, 17);
            this.decreasedByRadioButton.TabIndex = 8;
            this.decreasedByRadioButton.TabStop = true;
            this.decreasedByRadioButton.Text = "Value decreased by";
            this.decreasedByRadioButton.UseVisualStyleBackColor = true;
            this.decreasedByRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // decreasedRadioButton
            // 
            this.decreasedRadioButton.AutoSize = true;
            this.decreasedRadioButton.Location = new System.Drawing.Point(6, 149);
            this.decreasedRadioButton.Name = "decreasedRadioButton";
            this.decreasedRadioButton.Size = new System.Drawing.Size(105, 17);
            this.decreasedRadioButton.TabIndex = 12;
            this.decreasedRadioButton.TabStop = true;
            this.decreasedRadioButton.Text = "Value decreased";
            this.decreasedRadioButton.UseVisualStyleBackColor = true;
            this.decreasedRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // increasedRadioButton
            // 
            this.increasedRadioButton.AutoSize = true;
            this.increasedRadioButton.Location = new System.Drawing.Point(6, 123);
            this.increasedRadioButton.Name = "increasedRadioButton";
            this.increasedRadioButton.Size = new System.Drawing.Size(101, 17);
            this.increasedRadioButton.TabIndex = 10;
            this.increasedRadioButton.TabStop = true;
            this.increasedRadioButton.Text = "Value increased";
            this.increasedRadioButton.UseVisualStyleBackColor = true;
            this.increasedRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // filterGroupBox
            // 
            this.filterGroupBox.Controls.Add(this.currentValueTextBox);
            this.filterGroupBox.Controls.Add(this.currentValueOnlyTextBox);
            this.filterGroupBox.Controls.Add(this.valueMatchRadioButton);
            this.filterGroupBox.Controls.Add(this.label1);
            this.filterGroupBox.Controls.Add(this.decreasedRadioButton);
            this.filterGroupBox.Controls.Add(this.increasedRadioButton);
            this.filterGroupBox.Controls.Add(this.currentAndPreviousMatchRadioButton);
            this.filterGroupBox.Controls.Add(this.decrementTextBox);
            this.filterGroupBox.Controls.Add(this.decreasedByRadioButton);
            this.filterGroupBox.Controls.Add(this.previousValueTextBox);
            this.filterGroupBox.Controls.Add(this.incrementTextBox);
            this.filterGroupBox.Controls.Add(this.increasedByRadioButton);
            this.filterGroupBox.Location = new System.Drawing.Point(12, 12);
            this.filterGroupBox.Name = "filterGroupBox";
            this.filterGroupBox.Size = new System.Drawing.Size(294, 176);
            this.filterGroupBox.TabIndex = 13;
            this.filterGroupBox.TabStop = false;
            this.filterGroupBox.Text = "Search Filter";
            // 
            // resultRistBox
            // 
            this.resultRistBox.FormattingEnabled = true;
            this.resultRistBox.Location = new System.Drawing.Point(312, 18);
            this.resultRistBox.Name = "resultRistBox";
            this.resultRistBox.Size = new System.Drawing.Size(76, 173);
            this.resultRistBox.TabIndex = 0;
            // 
            // CheatFinderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 261);
            this.Controls.Add(this.resultRistBox);
            this.Controls.Add(this.filterGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheatFinderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cheat Finder";
            this.filterGroupBox.ResumeLayout(false);
            this.filterGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton valueMatchRadioButton;
        private System.Windows.Forms.TextBox currentValueOnlyTextBox;
        private System.Windows.Forms.RadioButton currentAndPreviousMatchRadioButton;
        private System.Windows.Forms.TextBox currentValueTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox previousValueTextBox;
        private System.Windows.Forms.RadioButton increasedByRadioButton;
        private System.Windows.Forms.TextBox incrementTextBox;
        private System.Windows.Forms.TextBox decrementTextBox;
        private System.Windows.Forms.RadioButton decreasedByRadioButton;
        private System.Windows.Forms.RadioButton decreasedRadioButton;
        private System.Windows.Forms.RadioButton increasedRadioButton;
        private System.Windows.Forms.GroupBox filterGroupBox;
        private System.Windows.Forms.ListBox resultRistBox;
    }
}