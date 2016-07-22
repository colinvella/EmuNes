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
            this.applyButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.resultListBox = new System.Windows.Forms.ListBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.resultsGroupBox = new System.Windows.Forms.GroupBox();
            this.filterGroupBox.SuspendLayout();
            this.resultsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // valueMatchRadioButton
            // 
            this.valueMatchRadioButton.AutoSize = true;
            this.valueMatchRadioButton.Location = new System.Drawing.Point(9, 29);
            this.valueMatchRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.valueMatchRadioButton.Name = "valueMatchRadioButton";
            this.valueMatchRadioButton.Size = new System.Drawing.Size(153, 24);
            this.valueMatchRadioButton.TabIndex = 0;
            this.valueMatchRadioButton.TabStop = true;
            this.valueMatchRadioButton.Text = "Current value is ";
            this.valueMatchRadioButton.UseVisualStyleBackColor = true;
            this.valueMatchRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // currentValueOnlyTextBox
            // 
            this.currentValueOnlyTextBox.Enabled = false;
            this.currentValueOnlyTextBox.Location = new System.Drawing.Point(158, 28);
            this.currentValueOnlyTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.currentValueOnlyTextBox.MaxLength = 2;
            this.currentValueOnlyTextBox.Name = "currentValueOnlyTextBox";
            this.currentValueOnlyTextBox.Size = new System.Drawing.Size(34, 26);
            this.currentValueOnlyTextBox.TabIndex = 1;
            // 
            // currentAndPreviousMatchRadioButton
            // 
            this.currentAndPreviousMatchRadioButton.AutoSize = true;
            this.currentAndPreviousMatchRadioButton.Location = new System.Drawing.Point(9, 69);
            this.currentAndPreviousMatchRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.currentAndPreviousMatchRadioButton.Name = "currentAndPreviousMatchRadioButton";
            this.currentAndPreviousMatchRadioButton.Size = new System.Drawing.Size(153, 24);
            this.currentAndPreviousMatchRadioButton.TabIndex = 2;
            this.currentAndPreviousMatchRadioButton.TabStop = true;
            this.currentAndPreviousMatchRadioButton.Text = "Current value is ";
            this.currentAndPreviousMatchRadioButton.UseVisualStyleBackColor = true;
            this.currentAndPreviousMatchRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // currentValueTextBox
            // 
            this.currentValueTextBox.Enabled = false;
            this.currentValueTextBox.Location = new System.Drawing.Point(158, 68);
            this.currentValueTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.currentValueTextBox.MaxLength = 2;
            this.currentValueTextBox.Name = "currentValueTextBox";
            this.currentValueTextBox.Size = new System.Drawing.Size(34, 26);
            this.currentValueTextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(196, 72);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "and previous value is";
            // 
            // previousValueTextBox
            // 
            this.previousValueTextBox.Enabled = false;
            this.previousValueTextBox.Location = new System.Drawing.Point(357, 68);
            this.previousValueTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.previousValueTextBox.MaxLength = 2;
            this.previousValueTextBox.Name = "previousValueTextBox";
            this.previousValueTextBox.Size = new System.Drawing.Size(34, 26);
            this.previousValueTextBox.TabIndex = 5;
            // 
            // increasedByRadioButton
            // 
            this.increasedByRadioButton.AutoSize = true;
            this.increasedByRadioButton.Location = new System.Drawing.Point(9, 109);
            this.increasedByRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.increasedByRadioButton.Name = "increasedByRadioButton";
            this.increasedByRadioButton.Size = new System.Drawing.Size(172, 24);
            this.increasedByRadioButton.TabIndex = 6;
            this.increasedByRadioButton.TabStop = true;
            this.increasedByRadioButton.Text = "Value increased by";
            this.increasedByRadioButton.UseVisualStyleBackColor = true;
            this.increasedByRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // incrementTextBox
            // 
            this.incrementTextBox.Enabled = false;
            this.incrementTextBox.Location = new System.Drawing.Point(184, 108);
            this.incrementTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.incrementTextBox.MaxLength = 2;
            this.incrementTextBox.Name = "incrementTextBox";
            this.incrementTextBox.Size = new System.Drawing.Size(34, 26);
            this.incrementTextBox.TabIndex = 7;
            // 
            // decrementTextBox
            // 
            this.decrementTextBox.Enabled = false;
            this.decrementTextBox.Location = new System.Drawing.Point(184, 148);
            this.decrementTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.decrementTextBox.MaxLength = 2;
            this.decrementTextBox.Name = "decrementTextBox";
            this.decrementTextBox.Size = new System.Drawing.Size(34, 26);
            this.decrementTextBox.TabIndex = 9;
            // 
            // decreasedByRadioButton
            // 
            this.decreasedByRadioButton.AutoSize = true;
            this.decreasedByRadioButton.Location = new System.Drawing.Point(9, 149);
            this.decreasedByRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.decreasedByRadioButton.Name = "decreasedByRadioButton";
            this.decreasedByRadioButton.Size = new System.Drawing.Size(177, 24);
            this.decreasedByRadioButton.TabIndex = 8;
            this.decreasedByRadioButton.TabStop = true;
            this.decreasedByRadioButton.Text = "Value decreased by";
            this.decreasedByRadioButton.UseVisualStyleBackColor = true;
            this.decreasedByRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // decreasedRadioButton
            // 
            this.decreasedRadioButton.AutoSize = true;
            this.decreasedRadioButton.Location = new System.Drawing.Point(9, 229);
            this.decreasedRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.decreasedRadioButton.Name = "decreasedRadioButton";
            this.decreasedRadioButton.Size = new System.Drawing.Size(155, 24);
            this.decreasedRadioButton.TabIndex = 12;
            this.decreasedRadioButton.TabStop = true;
            this.decreasedRadioButton.Text = "Value decreased";
            this.decreasedRadioButton.UseVisualStyleBackColor = true;
            this.decreasedRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // increasedRadioButton
            // 
            this.increasedRadioButton.AutoSize = true;
            this.increasedRadioButton.Location = new System.Drawing.Point(9, 189);
            this.increasedRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.increasedRadioButton.Name = "increasedRadioButton";
            this.increasedRadioButton.Size = new System.Drawing.Size(150, 24);
            this.increasedRadioButton.TabIndex = 10;
            this.increasedRadioButton.TabStop = true;
            this.increasedRadioButton.Text = "Value increased";
            this.increasedRadioButton.UseVisualStyleBackColor = true;
            this.increasedRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // filterGroupBox
            // 
            this.filterGroupBox.Controls.Add(this.applyButton);
            this.filterGroupBox.Controls.Add(this.resetButton);
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
            this.filterGroupBox.Location = new System.Drawing.Point(18, 18);
            this.filterGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.filterGroupBox.Name = "filterGroupBox";
            this.filterGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.filterGroupBox.Size = new System.Drawing.Size(400, 307);
            this.filterGroupBox.TabIndex = 13;
            this.filterGroupBox.TabStop = false;
            this.filterGroupBox.Text = "Search Filter";
            // 
            // applyButton
            // 
            this.applyButton.Enabled = false;
            this.applyButton.Location = new System.Drawing.Point(279, 264);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(112, 35);
            this.applyButton.TabIndex = 14;
            this.applyButton.Text = "&Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.OnSearchApply);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(161, 264);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(112, 35);
            this.resetButton.TabIndex = 13;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.OnSearchReset);
            // 
            // resultListBox
            // 
            this.resultListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultListBox.Font = new System.Drawing.Font("Consolas", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resultListBox.FormattingEnabled = true;
            this.resultListBox.ItemHeight = 20;
            this.resultListBox.Location = new System.Drawing.Point(3, 22);
            this.resultListBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.resultListBox.Name = "resultListBox";
            this.resultListBox.Size = new System.Drawing.Size(163, 282);
            this.resultListBox.TabIndex = 0;
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(482, 336);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(112, 35);
            this.closeButton.TabIndex = 14;
            this.closeButton.Text = "&Continue";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.OnFormContinue);
            // 
            // resultsGroupBox
            // 
            this.resultsGroupBox.Controls.Add(this.resultListBox);
            this.resultsGroupBox.Location = new System.Drawing.Point(425, 18);
            this.resultsGroupBox.Name = "resultsGroupBox";
            this.resultsGroupBox.Size = new System.Drawing.Size(169, 307);
            this.resultsGroupBox.TabIndex = 15;
            this.resultsGroupBox.TabStop = false;
            this.resultsGroupBox.Text = "Results";
            // 
            // CheatFinderForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 383);
            this.Controls.Add(this.resultsGroupBox);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.filterGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheatFinderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cheat Finder";
            this.filterGroupBox.ResumeLayout(false);
            this.filterGroupBox.PerformLayout();
            this.resultsGroupBox.ResumeLayout(false);
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
        private System.Windows.Forms.ListBox resultListBox;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.GroupBox resultsGroupBox;
    }
}