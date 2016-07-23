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
            this.components = new System.ComponentModel.Container();
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
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createCheatMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeButton = new System.Windows.Forms.Button();
            this.resultsGroupBox = new System.Windows.Forms.GroupBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.filterGroupBox.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.resultsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // valueMatchRadioButton
            // 
            this.valueMatchRadioButton.AutoSize = true;
            this.valueMatchRadioButton.Location = new System.Drawing.Point(8, 23);
            this.valueMatchRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.valueMatchRadioButton.Name = "valueMatchRadioButton";
            this.valueMatchRadioButton.Size = new System.Drawing.Size(132, 21);
            this.valueMatchRadioButton.TabIndex = 0;
            this.valueMatchRadioButton.TabStop = true;
            this.valueMatchRadioButton.Text = "Current value is ";
            this.valueMatchRadioButton.UseVisualStyleBackColor = true;
            this.valueMatchRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // currentValueOnlyTextBox
            // 
            this.currentValueOnlyTextBox.Enabled = false;
            this.currentValueOnlyTextBox.Location = new System.Drawing.Point(140, 22);
            this.currentValueOnlyTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.currentValueOnlyTextBox.MaxLength = 2;
            this.currentValueOnlyTextBox.Name = "currentValueOnlyTextBox";
            this.currentValueOnlyTextBox.Size = new System.Drawing.Size(31, 22);
            this.currentValueOnlyTextBox.TabIndex = 1;
            this.currentValueOnlyTextBox.Text = "00";
            this.currentValueOnlyTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidatingByteTextBox);
            this.currentValueOnlyTextBox.Validated += new System.EventHandler(this.OnValidatedByteTextBox);
            // 
            // currentAndPreviousMatchRadioButton
            // 
            this.currentAndPreviousMatchRadioButton.AutoSize = true;
            this.currentAndPreviousMatchRadioButton.Location = new System.Drawing.Point(8, 55);
            this.currentAndPreviousMatchRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.currentAndPreviousMatchRadioButton.Name = "currentAndPreviousMatchRadioButton";
            this.currentAndPreviousMatchRadioButton.Size = new System.Drawing.Size(132, 21);
            this.currentAndPreviousMatchRadioButton.TabIndex = 2;
            this.currentAndPreviousMatchRadioButton.TabStop = true;
            this.currentAndPreviousMatchRadioButton.Text = "Current value is ";
            this.currentAndPreviousMatchRadioButton.UseVisualStyleBackColor = true;
            this.currentAndPreviousMatchRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // currentValueTextBox
            // 
            this.currentValueTextBox.Enabled = false;
            this.currentValueTextBox.Location = new System.Drawing.Point(140, 54);
            this.currentValueTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.currentValueTextBox.MaxLength = 2;
            this.currentValueTextBox.Name = "currentValueTextBox";
            this.currentValueTextBox.Size = new System.Drawing.Size(31, 22);
            this.currentValueTextBox.TabIndex = 3;
            this.currentValueTextBox.Text = "00";
            this.currentValueTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidatingByteTextBox);
            this.currentValueTextBox.Validated += new System.EventHandler(this.OnValidatedByteTextBox);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(173, 57);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "and previous value is";
            // 
            // previousValueTextBox
            // 
            this.previousValueTextBox.Enabled = false;
            this.previousValueTextBox.Location = new System.Drawing.Point(317, 54);
            this.previousValueTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.previousValueTextBox.MaxLength = 2;
            this.previousValueTextBox.Name = "previousValueTextBox";
            this.previousValueTextBox.Size = new System.Drawing.Size(31, 22);
            this.previousValueTextBox.TabIndex = 5;
            this.previousValueTextBox.Text = "00";
            this.previousValueTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidatingByteTextBox);
            this.previousValueTextBox.Validated += new System.EventHandler(this.OnValidatedByteTextBox);
            // 
            // increasedByRadioButton
            // 
            this.increasedByRadioButton.AutoSize = true;
            this.increasedByRadioButton.Location = new System.Drawing.Point(8, 87);
            this.increasedByRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.increasedByRadioButton.Name = "increasedByRadioButton";
            this.increasedByRadioButton.Size = new System.Drawing.Size(150, 21);
            this.increasedByRadioButton.TabIndex = 6;
            this.increasedByRadioButton.TabStop = true;
            this.increasedByRadioButton.Text = "Value increased by";
            this.increasedByRadioButton.UseVisualStyleBackColor = true;
            this.increasedByRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // incrementTextBox
            // 
            this.incrementTextBox.Enabled = false;
            this.incrementTextBox.Location = new System.Drawing.Point(164, 86);
            this.incrementTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.incrementTextBox.MaxLength = 2;
            this.incrementTextBox.Name = "incrementTextBox";
            this.incrementTextBox.Size = new System.Drawing.Size(31, 22);
            this.incrementTextBox.TabIndex = 7;
            this.incrementTextBox.Text = "01";
            this.incrementTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidatingByteTextBox);
            this.incrementTextBox.Validated += new System.EventHandler(this.OnValidatedByteTextBox);
            // 
            // decrementTextBox
            // 
            this.decrementTextBox.Enabled = false;
            this.decrementTextBox.Location = new System.Drawing.Point(164, 118);
            this.decrementTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.decrementTextBox.MaxLength = 2;
            this.decrementTextBox.Name = "decrementTextBox";
            this.decrementTextBox.Size = new System.Drawing.Size(31, 22);
            this.decrementTextBox.TabIndex = 9;
            this.decrementTextBox.Text = "01";
            this.decrementTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidatingByteTextBox);
            this.decrementTextBox.Validated += new System.EventHandler(this.OnValidatedByteTextBox);
            // 
            // decreasedByRadioButton
            // 
            this.decreasedByRadioButton.AutoSize = true;
            this.decreasedByRadioButton.Location = new System.Drawing.Point(8, 119);
            this.decreasedByRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.decreasedByRadioButton.Name = "decreasedByRadioButton";
            this.decreasedByRadioButton.Size = new System.Drawing.Size(155, 21);
            this.decreasedByRadioButton.TabIndex = 8;
            this.decreasedByRadioButton.TabStop = true;
            this.decreasedByRadioButton.Text = "Value decreased by";
            this.decreasedByRadioButton.UseVisualStyleBackColor = true;
            this.decreasedByRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // decreasedRadioButton
            // 
            this.decreasedRadioButton.AutoSize = true;
            this.decreasedRadioButton.Location = new System.Drawing.Point(8, 183);
            this.decreasedRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.decreasedRadioButton.Name = "decreasedRadioButton";
            this.decreasedRadioButton.Size = new System.Drawing.Size(136, 21);
            this.decreasedRadioButton.TabIndex = 12;
            this.decreasedRadioButton.TabStop = true;
            this.decreasedRadioButton.Text = "Value decreased";
            this.decreasedRadioButton.UseVisualStyleBackColor = true;
            this.decreasedRadioButton.CheckedChanged += new System.EventHandler(this.OnFilterOptionChanged);
            // 
            // increasedRadioButton
            // 
            this.increasedRadioButton.AutoSize = true;
            this.increasedRadioButton.Location = new System.Drawing.Point(8, 151);
            this.increasedRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.increasedRadioButton.Name = "increasedRadioButton";
            this.increasedRadioButton.Size = new System.Drawing.Size(131, 21);
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
            this.filterGroupBox.Location = new System.Drawing.Point(16, 14);
            this.filterGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.filterGroupBox.Name = "filterGroupBox";
            this.filterGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.filterGroupBox.Size = new System.Drawing.Size(356, 246);
            this.filterGroupBox.TabIndex = 13;
            this.filterGroupBox.TabStop = false;
            this.filterGroupBox.Text = "Search Filter";
            // 
            // applyButton
            // 
            this.applyButton.Enabled = false;
            this.applyButton.Location = new System.Drawing.Point(248, 211);
            this.applyButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 28);
            this.applyButton.TabIndex = 14;
            this.applyButton.Text = "&Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.OnSearchApply);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(143, 211);
            this.resetButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(100, 28);
            this.resetButton.TabIndex = 13;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.OnSearchReset);
            // 
            // resultListBox
            // 
            this.resultListBox.ContextMenuStrip = this.contextMenuStrip;
            this.resultListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultListBox.Font = new System.Drawing.Font("Consolas", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resultListBox.FormattingEnabled = true;
            this.resultListBox.ItemHeight = 20;
            this.resultListBox.Location = new System.Drawing.Point(3, 17);
            this.resultListBox.Margin = new System.Windows.Forms.Padding(4);
            this.resultListBox.Name = "resultListBox";
            this.resultListBox.Size = new System.Drawing.Size(144, 227);
            this.resultListBox.TabIndex = 0;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createCheatMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(182, 58);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.OnOpeningContextMenu);
            // 
            // createCheatMenuItem
            // 
            this.createCheatMenuItem.Image = global::SharpNes.Properties.Resources.OptionsCheats;
            this.createCheatMenuItem.Name = "createCheatMenuItem";
            this.createCheatMenuItem.Size = new System.Drawing.Size(181, 26);
            this.createCheatMenuItem.Text = "Create Cheat...";
            this.createCheatMenuItem.Click += new System.EventHandler(this.OnCreateCheatMenuItem);
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(428, 269);
            this.closeButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 28);
            this.closeButton.TabIndex = 14;
            this.closeButton.Text = "&Continue";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.OnFormContinue);
            // 
            // resultsGroupBox
            // 
            this.resultsGroupBox.Controls.Add(this.resultListBox);
            this.resultsGroupBox.Location = new System.Drawing.Point(378, 14);
            this.resultsGroupBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.resultsGroupBox.Name = "resultsGroupBox";
            this.resultsGroupBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.resultsGroupBox.Size = new System.Drawing.Size(150, 246);
            this.resultsGroupBox.TabIndex = 15;
            this.resultsGroupBox.TabStop = false;
            this.resultsGroupBox.Text = "Results";
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // CheatFinderForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 306);
            this.Controls.Add(this.resultsGroupBox);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.filterGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheatFinderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cheat Finder";
            this.filterGroupBox.ResumeLayout(false);
            this.filterGroupBox.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.resultsGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
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
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem createCheatMenuItem;
    }
}