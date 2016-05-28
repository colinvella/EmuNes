namespace EmuNES
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameRunMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gamePauseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameResetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoPanel = new System.Windows.Forms.Panel();
            this.gameTimer = new System.Windows.Forms.Timer(this.components);
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.emulatorStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.frameRateStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainMenuStrip.SuspendLayout();
            this.videoPanel.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.gameMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.mainMenuStrip.Size = new System.Drawing.Size(683, 28);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "Main Menu Strip";
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileOpenMenuItem,
            this.fileExitMenuItem});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileMenuItem.Text = "&File";
            // 
            // fileOpenMenuItem
            // 
            this.fileOpenMenuItem.Name = "fileOpenMenuItem";
            this.fileOpenMenuItem.Size = new System.Drawing.Size(161, 26);
            this.fileOpenMenuItem.Text = "&Open";
            this.fileOpenMenuItem.Click += new System.EventHandler(this.OnFileOpen);
            // 
            // fileExitMenuItem
            // 
            this.fileExitMenuItem.Name = "fileExitMenuItem";
            this.fileExitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.fileExitMenuItem.Size = new System.Drawing.Size(161, 26);
            this.fileExitMenuItem.Text = "Exit";
            this.fileExitMenuItem.Click += new System.EventHandler(this.OnFileExit);
            // 
            // gameMenuItem
            // 
            this.gameMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameRunMenuItem,
            this.gamePauseMenuItem,
            this.gameResetMenuItem});
            this.gameMenuItem.Name = "gameMenuItem";
            this.gameMenuItem.Size = new System.Drawing.Size(60, 24);
            this.gameMenuItem.Text = "&Game";
            // 
            // gameRunMenuItem
            // 
            this.gameRunMenuItem.Enabled = false;
            this.gameRunMenuItem.Name = "gameRunMenuItem";
            this.gameRunMenuItem.Size = new System.Drawing.Size(121, 26);
            this.gameRunMenuItem.Text = "&Run";
            this.gameRunMenuItem.Click += new System.EventHandler(this.OnGameRun);
            // 
            // gamePauseMenuItem
            // 
            this.gamePauseMenuItem.Enabled = false;
            this.gamePauseMenuItem.Name = "gamePauseMenuItem";
            this.gamePauseMenuItem.Size = new System.Drawing.Size(121, 26);
            this.gamePauseMenuItem.Text = "&Pause";
            this.gamePauseMenuItem.Click += new System.EventHandler(this.OnGamePause);
            // 
            // gameResetMenuItem
            // 
            this.gameResetMenuItem.Enabled = false;
            this.gameResetMenuItem.Name = "gameResetMenuItem";
            this.gameResetMenuItem.Size = new System.Drawing.Size(121, 26);
            this.gameResetMenuItem.Text = "Reset";
            this.gameResetMenuItem.Click += new System.EventHandler(this.OnGameReset);
            // 
            // videoPanel
            // 
            this.videoPanel.Controls.Add(this.statusStrip);
            this.videoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoPanel.Location = new System.Drawing.Point(0, 28);
            this.videoPanel.Margin = new System.Windows.Forms.Padding(0);
            this.videoPanel.Name = "videoPanel";
            this.videoPanel.Size = new System.Drawing.Size(683, 617);
            this.videoPanel.TabIndex = 1;
            // 
            // gameTimer
            // 
            this.gameTimer.Interval = 20;
            this.gameTimer.Tick += new System.EventHandler(this.OnGameTick);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.emulatorStatusLabel,
            this.frameRateStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 592);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(683, 25);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // emulatorStatusLabel
            // 
            this.emulatorStatusLabel.Name = "emulatorStatusLabel";
            this.emulatorStatusLabel.Size = new System.Drawing.Size(624, 20);
            this.emulatorStatusLabel.Spring = true;
            this.emulatorStatusLabel.Text = "Status";
            this.emulatorStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frameRateStatusLabel
            // 
            this.frameRateStatusLabel.Name = "frameRateStatusLabel";
            this.frameRateStatusLabel.Size = new System.Drawing.Size(44, 20);
            this.frameRateStatusLabel.Text = "0 FPS";
            this.frameRateStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 645);
            this.Controls.Add(this.videoPanel);
            this.Controls.Add(this.mainMenuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenuStrip;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EmuNES";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.videoPanel.ResumeLayout(false);
            this.videoPanel.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileOpenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileExitMenuItem;
        private System.Windows.Forms.Panel videoPanel;
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.ToolStripMenuItem gameMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameRunMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gamePauseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameResetMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel emulatorStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel frameRateStatusLabel;
    }
}

