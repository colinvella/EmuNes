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
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filePropertiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileRecentFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameRunMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameResetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameStopMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewScreenSizeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewScreenSizeX1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewScreenSizeX2MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewScreenSizeX3MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewScreenSizeX4MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewScreenSizeSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.viewScreenSizeFullScreenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewTvAspectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewScreenFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewScreenFilterNoneMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewScreenFilterRasterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewScreenFilterLcdMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMotionBlurMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsInputMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diagnosticsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diagnosticsCodeDisassemblyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoPanel = new System.Windows.Forms.Panel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.emulatorStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.frameRateStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.iconTimer = new System.Windows.Forms.Timer(this.components);
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mainMenuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(146, 6);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(146, 6);
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.gameMenuItem,
            this.viewMenuItem,
            this.toolStripMenuItem1,
            this.optionsMenuItem,
            this.diagnosticsMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.mainMenuStrip.Size = new System.Drawing.Size(512, 24);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "Main Menu Strip";
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileOpenMenuItem,
            this.filePropertiesMenuItem,
            toolStripSeparator1,
            this.fileRecentFilesMenuItem,
            toolStripSeparator2,
            this.fileExitMenuItem});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileMenuItem.Text = "&File";
            // 
            // fileOpenMenuItem
            // 
            this.fileOpenMenuItem.Image = global::EmuNES.Properties.Resources.FileOpen;
            this.fileOpenMenuItem.Name = "fileOpenMenuItem";
            this.fileOpenMenuItem.Size = new System.Drawing.Size(149, 26);
            this.fileOpenMenuItem.Text = "&Open";
            this.fileOpenMenuItem.Click += new System.EventHandler(this.OnFileOpen);
            // 
            // filePropertiesMenuItem
            // 
            this.filePropertiesMenuItem.Enabled = false;
            this.filePropertiesMenuItem.Image = global::EmuNES.Properties.Resources.FileProperties;
            this.filePropertiesMenuItem.Name = "filePropertiesMenuItem";
            this.filePropertiesMenuItem.Size = new System.Drawing.Size(149, 26);
            this.filePropertiesMenuItem.Text = "&Properties...";
            this.filePropertiesMenuItem.Click += new System.EventHandler(this.OnFileProperties);
            // 
            // fileRecentFilesMenuItem
            // 
            this.fileRecentFilesMenuItem.Image = global::EmuNES.Properties.Resources.FileRecent;
            this.fileRecentFilesMenuItem.Name = "fileRecentFilesMenuItem";
            this.fileRecentFilesMenuItem.Size = new System.Drawing.Size(149, 26);
            this.fileRecentFilesMenuItem.Text = "Recent Files...";
            // 
            // fileExitMenuItem
            // 
            this.fileExitMenuItem.Image = global::EmuNES.Properties.Resources.FileExit;
            this.fileExitMenuItem.Name = "fileExitMenuItem";
            this.fileExitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.fileExitMenuItem.Size = new System.Drawing.Size(149, 26);
            this.fileExitMenuItem.Text = "Exit";
            this.fileExitMenuItem.Click += new System.EventHandler(this.OnFileExit);
            // 
            // gameMenuItem
            // 
            this.gameMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameRunMenuItem,
            this.gameResetMenuItem,
            this.gameStopMenuItem});
            this.gameMenuItem.Enabled = false;
            this.gameMenuItem.Name = "gameMenuItem";
            this.gameMenuItem.Size = new System.Drawing.Size(50, 20);
            this.gameMenuItem.Text = "&Game";
            // 
            // gameRunMenuItem
            // 
            this.gameRunMenuItem.Image = global::EmuNES.Properties.Resources.GameRun;
            this.gameRunMenuItem.Name = "gameRunMenuItem";
            this.gameRunMenuItem.Size = new System.Drawing.Size(143, 26);
            this.gameRunMenuItem.Text = "&Run";
            this.gameRunMenuItem.Click += new System.EventHandler(this.OnGameRunPause);
            // 
            // gameResetMenuItem
            // 
            this.gameResetMenuItem.Image = global::EmuNES.Properties.Resources.GameReset;
            this.gameResetMenuItem.Name = "gameResetMenuItem";
            this.gameResetMenuItem.Size = new System.Drawing.Size(143, 26);
            this.gameResetMenuItem.Text = "Reset";
            this.gameResetMenuItem.Click += new System.EventHandler(this.OnGameReset);
            // 
            // gameStopMenuItem
            // 
            this.gameStopMenuItem.Image = global::EmuNES.Properties.Resources.GameStop;
            this.gameStopMenuItem.Name = "gameStopMenuItem";
            this.gameStopMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.gameStopMenuItem.Size = new System.Drawing.Size(143, 26);
            this.gameStopMenuItem.Text = "Stop";
            this.gameStopMenuItem.Click += new System.EventHandler(this.OnGameStop);
            // 
            // viewMenuItem
            // 
            this.viewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewScreenSizeMenuItem,
            this.viewTvAspectMenuItem,
            this.viewScreenFilterMenuItem,
            this.viewMotionBlurMenuItem});
            this.viewMenuItem.Name = "viewMenuItem";
            this.viewMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewMenuItem.Text = "&View";
            // 
            // viewScreenSizeMenuItem
            // 
            this.viewScreenSizeMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewScreenSizeX1MenuItem,
            this.viewScreenSizeX2MenuItem,
            this.viewScreenSizeX3MenuItem,
            this.viewScreenSizeX4MenuItem,
            this.viewScreenSizeSeparator,
            this.viewScreenSizeFullScreenMenuItem});
            this.viewScreenSizeMenuItem.Image = global::EmuNES.Properties.Resources.ViewScreenSize;
            this.viewScreenSizeMenuItem.Name = "viewScreenSizeMenuItem";
            this.viewScreenSizeMenuItem.Size = new System.Drawing.Size(182, 26);
            this.viewScreenSizeMenuItem.Text = "Screen &Size";
            // 
            // viewScreenSizeX1MenuItem
            // 
            this.viewScreenSizeX1MenuItem.Name = "viewScreenSizeX1MenuItem";
            this.viewScreenSizeX1MenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D1)));
            this.viewScreenSizeX1MenuItem.Size = new System.Drawing.Size(171, 26);
            this.viewScreenSizeX1MenuItem.Text = "×1";
            this.viewScreenSizeX1MenuItem.Click += new System.EventHandler(this.OnViewScreenSizeX1);
            // 
            // viewScreenSizeX2MenuItem
            // 
            this.viewScreenSizeX2MenuItem.Name = "viewScreenSizeX2MenuItem";
            this.viewScreenSizeX2MenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D2)));
            this.viewScreenSizeX2MenuItem.Size = new System.Drawing.Size(171, 26);
            this.viewScreenSizeX2MenuItem.Text = "×2";
            this.viewScreenSizeX2MenuItem.Click += new System.EventHandler(this.OnViewScreenSizeX2);
            // 
            // viewScreenSizeX3MenuItem
            // 
            this.viewScreenSizeX3MenuItem.Name = "viewScreenSizeX3MenuItem";
            this.viewScreenSizeX3MenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D3)));
            this.viewScreenSizeX3MenuItem.Size = new System.Drawing.Size(171, 26);
            this.viewScreenSizeX3MenuItem.Text = "×3";
            this.viewScreenSizeX3MenuItem.Click += new System.EventHandler(this.OnViewScreenSizeX3);
            // 
            // viewScreenSizeX4MenuItem
            // 
            this.viewScreenSizeX4MenuItem.Name = "viewScreenSizeX4MenuItem";
            this.viewScreenSizeX4MenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D4)));
            this.viewScreenSizeX4MenuItem.Size = new System.Drawing.Size(171, 26);
            this.viewScreenSizeX4MenuItem.Text = "×4";
            this.viewScreenSizeX4MenuItem.Click += new System.EventHandler(this.OnViewScreenSizeX4);
            // 
            // viewScreenSizeSeparator
            // 
            this.viewScreenSizeSeparator.Name = "viewScreenSizeSeparator";
            this.viewScreenSizeSeparator.Size = new System.Drawing.Size(168, 6);
            // 
            // viewScreenSizeFullScreenMenuItem
            // 
            this.viewScreenSizeFullScreenMenuItem.Image = global::EmuNES.Properties.Resources.ViewScreenSizeFull;
            this.viewScreenSizeFullScreenMenuItem.Name = "viewScreenSizeFullScreenMenuItem";
            this.viewScreenSizeFullScreenMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.E)));
            this.viewScreenSizeFullScreenMenuItem.Size = new System.Drawing.Size(171, 26);
            this.viewScreenSizeFullScreenMenuItem.Text = "Full Screen";
            this.viewScreenSizeFullScreenMenuItem.Click += new System.EventHandler(this.OnViewScreenSizeFullScreen);
            // 
            // viewTvAspectMenuItem
            // 
            this.viewTvAspectMenuItem.Image = global::EmuNES.Properties.Resources.ViewTvAspect;
            this.viewTvAspectMenuItem.Name = "viewTvAspectMenuItem";
            this.viewTvAspectMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.viewTvAspectMenuItem.Size = new System.Drawing.Size(182, 26);
            this.viewTvAspectMenuItem.Text = "&TV Aspect";
            this.viewTvAspectMenuItem.Click += new System.EventHandler(this.OnViewTvAspect);
            // 
            // viewScreenFilterMenuItem
            // 
            this.viewScreenFilterMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewScreenFilterNoneMenuItem,
            this.viewScreenFilterRasterMenuItem,
            this.viewScreenFilterLcdMenuItem});
            this.viewScreenFilterMenuItem.Image = global::EmuNES.Properties.Resources.ViewScreenFilter;
            this.viewScreenFilterMenuItem.Name = "viewScreenFilterMenuItem";
            this.viewScreenFilterMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.viewScreenFilterMenuItem.Size = new System.Drawing.Size(182, 26);
            this.viewScreenFilterMenuItem.Text = "Screen &Filter";
            this.viewScreenFilterMenuItem.Click += new System.EventHandler(this.OnViewScreenFilter);
            // 
            // viewScreenFilterNoneMenuItem
            // 
            this.viewScreenFilterNoneMenuItem.Name = "viewScreenFilterNoneMenuItem";
            this.viewScreenFilterNoneMenuItem.Size = new System.Drawing.Size(106, 22);
            this.viewScreenFilterNoneMenuItem.Text = "None";
            this.viewScreenFilterNoneMenuItem.Click += new System.EventHandler(this.OnVewScreenFilterItem);
            // 
            // viewScreenFilterRasterMenuItem
            // 
            this.viewScreenFilterRasterMenuItem.Name = "viewScreenFilterRasterMenuItem";
            this.viewScreenFilterRasterMenuItem.Size = new System.Drawing.Size(106, 22);
            this.viewScreenFilterRasterMenuItem.Text = "Raster";
            this.viewScreenFilterRasterMenuItem.Click += new System.EventHandler(this.OnVewScreenFilterItem);
            // 
            // viewScreenFilterLcdMenuItem
            // 
            this.viewScreenFilterLcdMenuItem.Name = "viewScreenFilterLcdMenuItem";
            this.viewScreenFilterLcdMenuItem.Size = new System.Drawing.Size(106, 22);
            this.viewScreenFilterLcdMenuItem.Text = "LCD";
            this.viewScreenFilterLcdMenuItem.Click += new System.EventHandler(this.OnVewScreenFilterItem);
            // 
            // viewMotionBlurMenuItem
            // 
            this.viewMotionBlurMenuItem.Image = global::EmuNES.Properties.Resources.ViewMotionBlur;
            this.viewMotionBlurMenuItem.Name = "viewMotionBlurMenuItem";
            this.viewMotionBlurMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.viewMotionBlurMenuItem.Size = new System.Drawing.Size(182, 26);
            this.viewMotionBlurMenuItem.Text = "Motion &Blur";
            this.viewMotionBlurMenuItem.Click += new System.EventHandler(this.OnViewMotionBlur);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(12, 20);
            // 
            // optionsMenuItem
            // 
            this.optionsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsInputMenuItem});
            this.optionsMenuItem.Name = "optionsMenuItem";
            this.optionsMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsMenuItem.Text = "&Options";
            // 
            // optionsInputMenuItem
            // 
            this.optionsInputMenuItem.Image = global::EmuNES.Properties.Resources.OptionsInput;
            this.optionsInputMenuItem.Name = "optionsInputMenuItem";
            this.optionsInputMenuItem.Size = new System.Drawing.Size(115, 26);
            this.optionsInputMenuItem.Text = "&Input...";
            this.optionsInputMenuItem.Click += new System.EventHandler(this.OnOptionsInput);
            // 
            // diagnosticsMenuItem
            // 
            this.diagnosticsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.diagnosticsCodeDisassemblyMenuItem});
            this.diagnosticsMenuItem.Name = "diagnosticsMenuItem";
            this.diagnosticsMenuItem.Size = new System.Drawing.Size(80, 20);
            this.diagnosticsMenuItem.Text = "&Diagnostics";
            // 
            // diagnosticsCodeDisassemblyMenuItem
            // 
            this.diagnosticsCodeDisassemblyMenuItem.Name = "diagnosticsCodeDisassemblyMenuItem";
            this.diagnosticsCodeDisassemblyMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F12)));
            this.diagnosticsCodeDisassemblyMenuItem.Size = new System.Drawing.Size(222, 22);
            this.diagnosticsCodeDisassemblyMenuItem.Text = "&Code Disassembly";
            this.diagnosticsCodeDisassemblyMenuItem.Click += new System.EventHandler(this.OnDiagnosticsCodeDisassembly);
            // 
            // videoPanel
            // 
            this.videoPanel.BackColor = System.Drawing.Color.Black;
            this.videoPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.videoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoPanel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.videoPanel.Location = new System.Drawing.Point(0, 24);
            this.videoPanel.Margin = new System.Windows.Forms.Padding(0);
            this.videoPanel.Name = "videoPanel";
            this.videoPanel.Size = new System.Drawing.Size(512, 462);
            this.videoPanel.TabIndex = 1;
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.emulatorStatusLabel,
            this.frameRateStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 486);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(512, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // emulatorStatusLabel
            // 
            this.emulatorStatusLabel.Name = "emulatorStatusLabel";
            this.emulatorStatusLabel.Size = new System.Drawing.Size(462, 17);
            this.emulatorStatusLabel.Spring = true;
            this.emulatorStatusLabel.Text = "Status";
            this.emulatorStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frameRateStatusLabel
            // 
            this.frameRateStatusLabel.Name = "frameRateStatusLabel";
            this.frameRateStatusLabel.Size = new System.Drawing.Size(35, 17);
            this.frameRateStatusLabel.Text = "0 FPS";
            this.frameRateStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // iconTimer
            // 
            this.iconTimer.Enabled = true;
            this.iconTimer.Interval = 250;
            this.iconTimer.Tick += new System.EventHandler(this.OnIconTick);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(512, 508);
            this.Controls.Add(this.videoPanel);
            this.Controls.Add(this.mainMenuStrip);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenuStrip;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EmuNES";
            this.Deactivate += new System.EventHandler(this.OnFormDeactivate);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem gameMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameRunMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameResetMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel emulatorStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel frameRateStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem gameStopMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewScreenSizeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewScreenSizeX1MenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewScreenSizeX2MenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewScreenSizeX3MenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewScreenSizeX4MenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewTvAspectMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewScreenFilterMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMotionBlurMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filePropertiesMenuItem;
        private System.Windows.Forms.ToolStripSeparator viewScreenSizeSeparator;
        private System.Windows.Forms.ToolStripMenuItem viewScreenSizeFullScreenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileRecentFilesMenuItem;
        private System.Windows.Forms.Timer iconTimer;
        private System.Windows.Forms.ToolStripMenuItem viewScreenFilterNoneMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewScreenFilterRasterMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewScreenFilterLcdMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem optionsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsInputMenuItem;
        private System.Windows.Forms.ToolStripMenuItem diagnosticsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem diagnosticsCodeDisassemblyMenuItem;
    }
}

