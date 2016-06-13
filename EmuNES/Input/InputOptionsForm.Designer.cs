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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.controllerOneTabPage = new System.Windows.Forms.TabPage();
            this.controllerTwoTabPage = new System.Windows.Forms.TabPage();
            this.controllerThreeTabPage = new System.Windows.Forms.TabPage();
            this.controllerFourTabPage = new System.Windows.Forms.TabPage();
            this.configureOneButton = new System.Windows.Forms.Button();
            this.configureTextbox = new System.Windows.Forms.TextBox();
            this.tabControl.SuspendLayout();
            this.controllerOneTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(436, 359);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(64, 32);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(506, 359);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(64, 32);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl.Controls.Add(this.controllerOneTabPage);
            this.tabControl.Controls.Add(this.controllerTwoTabPage);
            this.tabControl.Controls.Add(this.controllerThreeTabPage);
            this.tabControl.Controls.Add(this.controllerFourTabPage);
            this.tabControl.ItemSize = new System.Drawing.Size(97, 32);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(558, 341);
            this.tabControl.TabIndex = 2;
            // 
            // controllerOneTabPage
            // 
            this.controllerOneTabPage.Controls.Add(this.configureTextbox);
            this.controllerOneTabPage.Controls.Add(this.configureOneButton);
            this.controllerOneTabPage.Location = new System.Drawing.Point(4, 36);
            this.controllerOneTabPage.Name = "controllerOneTabPage";
            this.controllerOneTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.controllerOneTabPage.Size = new System.Drawing.Size(550, 301);
            this.controllerOneTabPage.TabIndex = 0;
            this.controllerOneTabPage.Text = "Controller One";
            // 
            // controllerTwoTabPage
            // 
            this.controllerTwoTabPage.Location = new System.Drawing.Point(4, 36);
            this.controllerTwoTabPage.Name = "controllerTwoTabPage";
            this.controllerTwoTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.controllerTwoTabPage.Size = new System.Drawing.Size(550, 301);
            this.controllerTwoTabPage.TabIndex = 1;
            this.controllerTwoTabPage.Text = "Controller Two";
            // 
            // controllerThreeTabPage
            // 
            this.controllerThreeTabPage.Location = new System.Drawing.Point(4, 28);
            this.controllerThreeTabPage.Name = "controllerThreeTabPage";
            this.controllerThreeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.controllerThreeTabPage.Size = new System.Drawing.Size(550, 318);
            this.controllerThreeTabPage.TabIndex = 2;
            this.controllerThreeTabPage.Text = "Controller Three";
            this.controllerThreeTabPage.UseVisualStyleBackColor = true;
            // 
            // controllerFourTabPage
            // 
            this.controllerFourTabPage.Location = new System.Drawing.Point(4, 28);
            this.controllerFourTabPage.Name = "controllerFourTabPage";
            this.controllerFourTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.controllerFourTabPage.Size = new System.Drawing.Size(550, 318);
            this.controllerFourTabPage.TabIndex = 3;
            this.controllerFourTabPage.Text = "Controller 4";
            this.controllerFourTabPage.UseVisualStyleBackColor = true;
            // 
            // configureOneButton
            // 
            this.configureOneButton.Location = new System.Drawing.Point(7, 7);
            this.configureOneButton.Name = "configureOneButton";
            this.configureOneButton.Size = new System.Drawing.Size(96, 32);
            this.configureOneButton.TabIndex = 0;
            this.configureOneButton.Text = "&Configure...";
            this.configureOneButton.UseVisualStyleBackColor = true;
            this.configureOneButton.Click += new System.EventHandler(this.OnConfigureController);
            this.configureOneButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            // 
            // configureTextbox
            // 
            this.configureTextbox.Location = new System.Drawing.Point(6, 45);
            this.configureTextbox.Name = "configureTextbox";
            this.configureTextbox.ReadOnly = true;
            this.configureTextbox.Size = new System.Drawing.Size(100, 22);
            this.configureTextbox.TabIndex = 2;
            // 
            // InputOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 403);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputOptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Input Options";
            this.tabControl.ResumeLayout(false);
            this.controllerOneTabPage.ResumeLayout(false);
            this.controllerOneTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage controllerOneTabPage;
        private System.Windows.Forms.TabPage controllerTwoTabPage;
        private System.Windows.Forms.TabPage controllerThreeTabPage;
        private System.Windows.Forms.TabPage controllerFourTabPage;
        private System.Windows.Forms.Button configureOneButton;
        private System.Windows.Forms.TextBox configureTextbox;
    }
}