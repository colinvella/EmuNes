﻿namespace SharpNes.Cheats
{
    partial class CheatDetailsForm
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.valueTextBox = new System.Windows.Forms.TextBox();
            this.compareTextBox = new System.Windows.Forms.TextBox();
            this.compareRequiredCheckBox = new System.Windows.Forms.CheckBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(38, 23);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(71, 20);
            label1.TabIndex = 0;
            label1.Text = "Address";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(54, 63);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(51, 20);
            label2.TabIndex = 2;
            label2.Text = "Value";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(32, 103);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(77, 20);
            label3.TabIndex = 4;
            label3.Text = "Compare";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(15, 143);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(95, 20);
            label4.TabIndex = 7;
            label4.Text = "Description";
            // 
            // addressTextBox
            // 
            this.addressTextBox.Location = new System.Drawing.Point(114, 18);
            this.addressTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addressTextBox.MaxLength = 4;
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(70, 26);
            this.addressTextBox.TabIndex = 1;
            this.addressTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidatingAddress);
            this.addressTextBox.Validated += new System.EventHandler(this.OnValidatedAddress);
            // 
            // valueTextBox
            // 
            this.valueTextBox.Location = new System.Drawing.Point(114, 58);
            this.valueTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.valueTextBox.MaxLength = 2;
            this.valueTextBox.Name = "valueTextBox";
            this.valueTextBox.Size = new System.Drawing.Size(34, 26);
            this.valueTextBox.TabIndex = 3;
            this.valueTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidatingByteValue);
            this.valueTextBox.Validated += new System.EventHandler(this.OnValidatedByteValue);
            // 
            // compareTextBox
            // 
            this.compareTextBox.Location = new System.Drawing.Point(114, 98);
            this.compareTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.compareTextBox.MaxLength = 2;
            this.compareTextBox.Name = "compareTextBox";
            this.compareTextBox.Size = new System.Drawing.Size(34, 26);
            this.compareTextBox.TabIndex = 5;
            this.compareTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.OnValidatingByteValue);
            this.compareTextBox.Validated += new System.EventHandler(this.OnValidatedByteValue);
            // 
            // compareRequiredCheckBox
            // 
            this.compareRequiredCheckBox.AutoSize = true;
            this.compareRequiredCheckBox.Location = new System.Drawing.Point(159, 102);
            this.compareRequiredCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.compareRequiredCheckBox.Name = "compareRequiredCheckBox";
            this.compareRequiredCheckBox.Size = new System.Drawing.Size(171, 24);
            this.compareRequiredCheckBox.TabIndex = 6;
            this.compareRequiredCheckBox.Text = "Compare Required";
            this.compareRequiredCheckBox.UseVisualStyleBackColor = true;
            this.compareRequiredCheckBox.CheckedChanged += new System.EventHandler(this.OnCompareRequiredCheckedChanged);
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Location = new System.Drawing.Point(114, 138);
            this.descriptionTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(292, 26);
            this.descriptionTextBox.TabIndex = 8;
            this.descriptionTextBox.Validated += new System.EventHandler(this.OnValidatedDescription);
            // 
            // cancelButton
            // 
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(296, 178);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(112, 35);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(174, 178);
            this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(112, 35);
            this.okButton.TabIndex = 10;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OnButtonOk);
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // CheatDetailsForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(426, 231);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(label4);
            this.Controls.Add(this.compareRequiredCheckBox);
            this.Controls.Add(this.compareTextBox);
            this.Controls.Add(label3);
            this.Controls.Add(this.valueTextBox);
            this.Controls.Add(label2);
            this.Controls.Add(this.addressTextBox);
            this.Controls.Add(label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheatDetailsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Cheat";
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.TextBox valueTextBox;
        private System.Windows.Forms.TextBox compareTextBox;
        private System.Windows.Forms.CheckBox compareRequiredCheckBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}