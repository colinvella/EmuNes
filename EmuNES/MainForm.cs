using EmuNES.Audio;
using EmuNES.Diagnostics;
using EmuNES.Input;
using NAudio.Wave;
using NesCore.Input;
using NesCore.Storage;
using NesCore.Video;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmuNES
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.FormClosing += OnApplicationClosing;
            videoPanel.Paint += OnVideoPanelPaint;

            // enabled DoubleBuffered property (protected)
            System.Reflection.PropertyInfo aProp =
               typeof(System.Windows.Forms.Control).GetProperty(
                 "DoubleBuffered",
                 System.Reflection.BindingFlags.NonPublic |
                 System.Reflection.BindingFlags.Instance);
            aProp.SetValue(videoPanel, true, null);

            // map for key states to help controller mapping
            this.keyboardState = new KeyboardState();

            // set up joystick object before console
            this.gameControllerManager = new GameControllerManager();

            Console = new NesCore.Console();

            ConfigureVideo();
            ConfigureAudio();
            ConfigureDefaultController();

            gameState = GameState.Stopped;

            bitmapBuffer = new FastBitmap(256, 240);

            waveOut = new WaveOut();
            waveOut.DesiredLatency = 100;

            apuAudioProvider = new ApuAudioProvider();
            waveOut.Init(apuAudioProvider);
        }

        private void OnFormLoad(object sender, EventArgs eventArgs)
        {
            recentFileManager = new RecentFileManager(
                fileRecentFilesMenuItem,
                LoadRecentRom, null,
                Properties.Resources.FileOpen);

            applicationMargin = new Size(Width - videoPanel.ClientSize.Width, Height - videoPanel.ClientSize.Height);
            SetScreen(1, true);
            SetScreenFilter(ScreenFilter.None);
            SetMotionBlur(false);
            viewScreenSizeFullScreenMenuItem.ShortcutKeys = Keys.Alt | Keys.Enter;

            // make space for checkboxes (disabled due to icons)
            ((ToolStripDropDownMenu)viewMenuItem.DropDown).ShowCheckMargin = true;

            codeDisassemblyForm = new CodeDisassemblyForm(Console);

            Application.Idle += TickWhileIdle;

#if DEBUG
            gameTimer.Interval = 20;
#else
            gameTimer.Interval = 1;
#endif
        }

        private void OnApplicationClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            cancelEventArgs.Cancel = MessageBox.Show(
                this, "Are you sure?", "Exit " + Application.ProductName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No;

            if (cancelEventArgs.Cancel)
                return;

            // save previous SRAM if applicable
            if (this.cartridge != null && this.cartridge.SaveRam.Modified)
                StoreSaveRam();
        }

        private void OnFileOpen(object sender, EventArgs eventArgs)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Application.ExecutablePath;
            openFileDialog.Filter = "NES ROM files (*.nes, *.zip)|*.nes;*.zip|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "Open Game ROM";
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            LoadCartridgeRom(openFileDialog.FileName);
        }

        private void OnFileProperties(object sender, EventArgs eventArgs)
        {
            if (cartridge == null)
                return;

            string cartridgeProperties
                = "Program Rom Size: " + cartridge.ProgramRom.Count + "k\r\n"
                + "Character rom Size: " + cartridge.CharacterRom.Length + "k\r\n"
                + "Mapper Type: " + cartridge.MapperType + " (" + cartridge.Map.Name + ")\r\n"
                + "Mirroring Mode: " + cartridge.MirrorMode + "\r\n"
                + "Battery Present: " + (cartridge.BatteryPresent ? "Yes" : "No");

            MessageBox.Show(this, cartridgeProperties, this.cartridgeRomFilename + " Properties", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnFileExit(object sender, EventArgs eventArgs)
        {
            Application.Exit();
        }

        private void OnGameRunPause(object sender, EventArgs eventArgs)
        {
            switch (gameState)
            {
                case GameState.Stopped:
                    // run
                    Console.Reset();
                    gameTickDateTime = DateTime.Now;
                    averageDeltaTime = 1.0 / 60.0;
                    gameState = GameState.Running;
                    waveOut.Play();
                    break;
                case GameState.Running:
                    // pause
                    waveOut.Pause();
                    gameState = GameState.Paused;
                    break;
                case GameState.Paused:
                    // resume;
                    gameTickDateTime = DateTime.Now;
                    averageDeltaTime = 1.0 / 60.0;
                    gameState = GameState.Running;
                    waveOut.Resume();
                    break;
            }

            videoPanel.Invalidate();
            UpdateGameMenuItems();
        }

        private void OnGameReset(object sender, EventArgs eventArgs)
        {
            waveOut.Stop();
            waveOut.Play();
            OnGameStop(sender, eventArgs);
            Console.Reset();
            OnGameRunPause(sender, eventArgs);
        }

        private void OnGameStop(object sender, EventArgs eventArgs)
        {
            if (gameState == GameState.Stopped)
                return;

            videoPanel.Invalidate();
            gameState = GameState.Stopped;
            waveOut.Stop();

            // save previous SRAM if applicable
            if (this.cartridge.SaveRam.Modified)
                StoreSaveRam();

            UpdateGameMenuItems();
        }

        private void OnViewScreenSizeX1(object sender, EventArgs eventArgs)
        {
            SetScreenSize(1);
        }

        private void OnViewScreenSizeX2(object sender, EventArgs eventArgs)
        {
            SetScreenSize(2);
        }

        private void OnViewScreenSizeX3(object sender, EventArgs eventArgse)
        {
            SetScreenSize(3);
        }

        private void OnViewScreenSizeX4(object sender, EventArgs eventArgs)
        {
            SetScreenSize(4);
        }

        private void OnViewScreenSizeFullScreen(object sender, EventArgs e)
        {
            SetFullScreen(!fullScreen);
        }

        private void OnViewTvAspect(object sender, EventArgs eventArgs)
        {
            SetTvAspect(!tvAspect);
        }

        private void OnViewScreenFilter(object sender, EventArgs e)
        {
            int filterCount = Enum.GetValues(typeof(ScreenFilter)).Length;
            screenFilter = (ScreenFilter)(((int)screenFilter + 1) % filterCount);
            SetScreenFilter(screenFilter);
        }

        private void OnVewScreenFilterItem(object sender, EventArgs eventArgs)
        {
            int filterIndex = 0;
            foreach (ToolStripMenuItem filterMenuItem in viewScreenFilterMenuItem.DropDownItems)
            {
                if (filterMenuItem == sender)
                {
                    SetScreenFilter((ScreenFilter)filterIndex);
                    return;
                }
                ++filterIndex;
            }
        }

        private void OnViewMotionBlur(object sender, EventArgs eventArgs)
        {
            SetMotionBlur(!motionBlur);
        }

        private void OnOptionsInput(object sender, EventArgs eventArgs)
        {
            InputOptionsForm inputOptionsForm = new InputOptionsForm(Console, keyboardState);
            inputOptionsForm.ShowDialog(this);
        }

        private void OnDiagnosticsCodeDisassembly(object sender, EventArgs eventArgs)
        {
            traceEnabled = !traceEnabled;
            diagnosticsCodeDisassemblyMenuItem.Checked = traceEnabled;

            if (traceEnabled)
            {
                codeDisassemblyForm.Hide();
                Console.Processor.Trace = null;
                if (Console.Cartridge != null)
                {
                    Console.Cartridge.Map.ProgramBankSwitch = null;
                }
            }
            else
            {
                codeDisassemblyForm.Show(this);
                Console.Processor.Trace = () => codeDisassemblyForm.Trace();
                if (Console.Cartridge != null)
                {
                    Console.Cartridge.Map.ProgramBankSwitch
                        = (address, size) => codeDisassemblyForm.InvalidateMemoryRange(address, size);
                }
            }
            diagnosticsCodeDisassemblyMenuItem.Checked = !diagnosticsCodeDisassemblyMenuItem.Checked;
            this.Focus();
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            keyboardState[keyEventArgs.KeyCode] = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            keyboardState[keyEventArgs.KeyCode] = false;

            // custom shurtcut code as menu item has dropdown items
            if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.F)
                OnViewScreenFilter(this, EventArgs.Empty);
        }

        void TickWhileIdle(object sender, EventArgs eventArgs)
        {
            WindowsUser32.Message message;

            while (!WindowsUser32.PeekMessage(out message, IntPtr.Zero, 0, 0, 0))
            {
                OnGameTick(sender, eventArgs);
            }
        }

        private void OnGameTick(object sender, EventArgs eventArgs)
        {
            gameControllerManager.UpdateState();

            if (gameState != GameState.Running)
                return;

#if DEBUG
            Console.Run(0.020);
#else
            DateTime currentTickDateTime = DateTime.Now;
            double tickDelta = (currentTickDateTime - gameTickDateTime).TotalSeconds;
            gameTickDateTime = currentTickDateTime;

            Console.Run(tickDelta);
#endif
        }

        private void OnIconTick(object sender, EventArgs eventArgs)
        {
            if (gameState != GameState.Running)
            {
                if (this.Icon != Properties.Resources.Nes)
                    this.Icon = Properties.Resources.Nes;
                return;
            }

            // update application icon with running game
            if (gameIcon != null)
                DestroyIcon(gameIcon.Handle);

            Bitmap bitmapIcon = ResizeImage(bitmapBuffer.Bitmap, 32, 32);
            gameIcon = Icon.FromHandle(bitmapIcon.GetHicon());
            bitmapIcon.Dispose();
            this.Icon = gameIcon;
        }

        private void OnVideoPanelPaint(object sender, PaintEventArgs paintEventArgs)
        {
            Graphics graphics = paintEventArgs.Graphics;
            int leftCenteredMargin = (videoPanel.Width - bufferSize.Width) / 2;

            graphics.InterpolationMode = InterpolationMode.Low;
            switch (gameState)
            {
                case GameState.Stopped:
                    graphics.InterpolationMode = InterpolationMode.Low;
                    graphics.DrawImage(Properties.Resources.Background, 0, 0, videoPanel.Width, videoPanel.Height);
                    break;
                case GameState.Paused:
                    graphics.DrawImage(bitmapBuffer.Bitmap, leftCenteredMargin, 0, bufferSize.Width, bufferSize.Height);
                    graphics.FillRectangle(new SolidBrush(Color.FromArgb(192, Color.Black)), leftCenteredMargin, 0, bufferSize.Width, bufferSize.Height);
                    DrawCenteredText(graphics, "Paused");
                    break;
                case GameState.Running:
                    if (screenFilter != ScreenFilter.None)
                    {
                        graphics.DrawImage(bitmapBuffer.Bitmap, leftCenteredMargin, 0, bufferSize.Width, bufferSize.Height);
                        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                        graphics.DrawImage(resizedScreenFilter, leftCenteredMargin, 0);
                    }
                    else
                    {
                        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                        graphics.DrawImage(bitmapBuffer.Bitmap, leftCenteredMargin, 0, bufferSize.Width, bufferSize.Height);
                    }

                    break;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        private void DrawCenteredText(Graphics graphics, string text)
        {
            if (resizedCaptionFont == null)
                resizedCaptionFont = new Font(this.Font.FontFamily, videoPanel.Height / 20, GraphicsUnit.Pixel);

            SizeF textSize = graphics.MeasureString(text, resizedCaptionFont);
            float textX = (videoPanel.Width - textSize.Width) / 2;
            float textY = (videoPanel.Height - videoPanel.Height / 10) / 2;

            // back outline
            graphics.DrawString(text, resizedCaptionFont, Brushes.Black, textX - 1, textY);
            graphics.DrawString(text, resizedCaptionFont, Brushes.Black, textX + 1, textY);
            graphics.DrawString(text, resizedCaptionFont, Brushes.Black, textX, textY - 1);
            graphics.DrawString(text, resizedCaptionFont, Brushes.Black, textX, textY + 1);

            graphics.DrawString(text, resizedCaptionFont, Brushes.White, textX, textY);
        }

        private void UpdateGameMenuItems()
        {
            switch (gameState)
            {
                case GameState.Stopped:
                    gameRunMenuItem.Text = "&Run";
                    gameRunMenuItem.Image = Properties.Resources.GameRun;
                    gameRunMenuItem.ShortcutKeys = Keys.Control | Keys.R;
                    break;
                case GameState.Running:
                    gameRunMenuItem.Text = "&Pause";
                    gameRunMenuItem.Image = Properties.Resources.GamePause;
                    gameRunMenuItem.ShortcutKeys = Keys.Control | Keys.P;
                    break;
                case GameState.Paused:
                    gameRunMenuItem.Text = "&Resume";
                    gameRunMenuItem.Image = Properties.Resources.GameRun;
                    gameRunMenuItem.ShortcutKeys = Keys.Control | Keys.R;
                    break;
            }
            gameStopMenuItem.Enabled = gameState != GameState.Stopped;
        }

        private void ConfigureVideo()
        {
            Console.Video.WritePixel = (x, y, colour) =>
            {
                int offset = (y * 256 + x) * 4;

                if (motionBlur)
                {
                    byte oldBlue = bitmapBuffer.Bits[offset];
                    if (colour.Blue < oldBlue)
                        bitmapBuffer.Bits[offset++] = (byte)(oldBlue * 0.8);
                    else
                        bitmapBuffer.Bits[offset++] = colour.Blue;

                    byte oldGreen = bitmapBuffer.Bits[offset];
                    if (colour.Green < oldGreen)
                        bitmapBuffer.Bits[offset++] = (byte)(oldGreen * 0.8);
                    else
                        bitmapBuffer.Bits[offset++] = colour.Green;

                    byte oldRed = bitmapBuffer.Bits[offset];
                    if (colour.Red < oldRed)
                        bitmapBuffer.Bits[offset] = (byte)(oldRed * 0.8);
                    else
                        bitmapBuffer.Bits[offset] = colour.Red;
                }
                else
                {
                    bitmapBuffer.Bits[offset++] = colour.Blue;
                    bitmapBuffer.Bits[offset++] = colour.Green;
                    bitmapBuffer.Bits[offset] = colour.Red;
                }
            };

            Console.Video.ShowFrame = () =>
            {
                videoPanel.Invalidate();

                // frame rate
                DateTime currentDateTime = DateTime.Now;
                double currentDeltaTime = (currentDateTime - frameDateTime).TotalSeconds;
                frameDateTime = currentDateTime;
                averageDeltaTime = averageDeltaTime * 0.9 + currentDeltaTime * 0.1;
                int frameRate = (int)(1.0 / averageDeltaTime);
                frameRateStatusLabel.Text = frameRate + " FPS";
            };
        }

        private void ConfigureAudio()
        {
            Console.Audio.SampleRate = 44100;

            float[] outputBuffer = new float[256];
            int writeIndex = 0;

            Console.Audio.WriteSample = (sampleValue) =>
            {
                // fill buffer
                outputBuffer[writeIndex++] = sampleValue;
                writeIndex %= outputBuffer.Length;

                // when buffer full, send to wave provider
                if (writeIndex == 0)
                    apuAudioProvider.Queue(outputBuffer);
            };
        }

        private void ConfigureDefaultController()
        {
            Joypad joypad = new Joypad();

            if (gameControllerManager.Count > 0)
            {
                // temporary - if there is a controller, accept both keyboard and joypad for controller 1
                GameController gameController = gameControllerManager[0];

                joypad.Start = () => keyboardState[Keys.Enter] || gameController.Buttons[9];
                joypad.Select = () => keyboardState[Keys.Tab] || gameController.Buttons[8];
                joypad.A = () => keyboardState[Keys.Z] || gameController.Buttons[1];
                joypad.B = () => keyboardState[Keys.X] || gameController.Buttons[2];
                joypad.Up = () => keyboardState[Keys.Up] || gameController.Up;
                joypad.Down = () => keyboardState[Keys.Down] || gameController.Down;
                joypad.Left = () => keyboardState[Keys.Left] || gameController.Left;
                joypad.Right = () => keyboardState[Keys.Right] || gameController.Right;
            }
            else
            {
                // temporary - otherwise, just keyboard input
                joypad.Start = () => keyboardState[Keys.Enter];
                joypad.Select = () => keyboardState[Keys.Tab];
                joypad.A = () => keyboardState[Keys.Z];
                joypad.B = () => keyboardState[Keys.X];
                joypad.Up = () => keyboardState[Keys.Up];
                joypad.Down = () => keyboardState[Keys.Down];
                joypad.Left = () => keyboardState[Keys.Left];
                joypad.Right = () => keyboardState[Keys.Right];
            }

            Console.ConnectController(1, joypad);
        }

        private void LoadRecentRom(object sender, EventArgs eventArgs)
        {
            string cartridgeRomPath = ((ToolStripMenuItem)sender).Text;
            if (!LoadCartridgeRom(cartridgeRomPath))
                recentFileManager.RemoveRecentFile(cartridgeRomPath);
        }

        private bool LoadCartridgeRom(string cartridgeRomPath)
        {
            try
            {
                // save previous SRAM if applicable
                if (this.cartridge != null && this.cartridge.SaveRam.Modified)
                    StoreSaveRam();

                Stream cartridgeRomStream = GetCartridgeRomStream(cartridgeRomPath);

                BinaryReader romBinaryReader = new BinaryReader(cartridgeRomStream);
                Cartridge newCartridge = new Cartridge(romBinaryReader);
                romBinaryReader.Close();

                // load SRAM if file exists
                this.cartridgeSaveRamFilename = cartridgeRomPath.Replace(".nes", ".sram").Replace(".zip", ".sram");
                if (File.Exists(this.cartridgeSaveRamFilename))
                {
                    BinaryReader saveRamBinaryReader = new BinaryReader(new FileStream(this.cartridgeSaveRamFilename, FileMode.Open));
                    newCartridge.SaveRam.Load(saveRamBinaryReader);
                    saveRamBinaryReader.Close();
                    emulatorStatusLabel.Text = "SaveRam file detected and loaded";
                }

                Console.LoadCartridge(newCartridge);

                this.cartridge = newCartridge;

                this.cartridgeRomFilename = Path.GetFileNameWithoutExtension(cartridgeRomPath);

                this.Text = cartridgeRomFilename + " - " + Application.ProductName;

                filePropertiesMenuItem.Enabled = true;
                gameMenuItem.Enabled = true;

                if (traceEnabled)
                {
                    Console.Cartridge.Map.ProgramBankSwitch = 
                        (address, size) => codeDisassemblyForm.InvalidateMemoryRange(address, size);
                }

                OnGameStop(this, EventArgs.Empty);
                OnGameRunPause(this, EventArgs.Empty);

                recentFileManager.AddRecentFile(cartridgeRomPath);

                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, "Unable to load cartridge rom. Reason: " + exception.Message, "Open Game ROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void StoreSaveRam()
        {
            BinaryWriter saveRamBinaryWriter = new BinaryWriter(new FileStream(this.cartridgeSaveRamFilename, FileMode.OpenOrCreate, FileAccess.Write));
            this.cartridge.SaveRam.Save(saveRamBinaryWriter);
            saveRamBinaryWriter.Close();
            emulatorStatusLabel.Text = "SaveRam saved to disk";

        }

        /// <summary>
        /// Tries to get a stream to a cartridge rom file using the given path.
        /// If the file is a ZIP archive, the a stream to the first NES file is
        /// returned.
        /// </summary>
        /// <param name="cartridgeRomPath">Path to NES or ZIP file</param>
        /// <returns></returns>
        private Stream GetCartridgeRomStream(string cartridgeRomPath)
        {
            Stream cartridgeRomStream = null;

            if (Path.GetExtension(cartridgeRomPath).ToLower() == ".zip")
            {
                Stream zipStream = new FileStream(cartridgeRomPath, FileMode.Open);
                ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);

                // find first nes file
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    if (entry.FullName.EndsWith(".nes", StringComparison.OrdinalIgnoreCase))
                    {
                        cartridgeRomStream = entry.Open();
                        break;
                    }
                }
            }

            if (cartridgeRomStream == null)
                cartridgeRomStream = new FileStream(cartridgeRomPath, FileMode.Open);

            return cartridgeRomStream;
        }

        private void SetScreenSize(byte newScreenSize)
        {
            SetScreen(newScreenSize, tvAspect);
        }

        private void SetTvAspect(bool newTvAspect)
        {
            SetScreen(screenSize, newTvAspect);
        }

        private void SetScreen(byte newScreenSize, bool newTvAspect)
        {
            if (fullScreen)
                SetFullScreen(false);

            resizedCaptionFont = null;

            screenSize = newScreenSize;
            tvAspect = newTvAspect;

            bufferSize.Width = tvAspect ? 282 * screenSize : 256 * screenSize;
            bufferSize.Height = 240 * screenSize;

            Width = bufferSize.Width + applicationMargin.Width;
            Height = bufferSize.Height + applicationMargin.Height;

            // create resized version of raster filter to optimise rendering
            UpdateResizedScreenFilter();

            viewScreenSizeX1MenuItem.Checked = newScreenSize == 1;
            viewScreenSizeX2MenuItem.Checked = newScreenSize == 2;
            viewScreenSizeX3MenuItem.Checked = newScreenSize == 3;
            viewScreenSizeX4MenuItem.Checked = newScreenSize == 4;
            viewTvAspectMenuItem.Checked = tvAspect;
            videoPanel.Invalidate();
        }

        private void SetFullScreen(bool fullScreen)
        {
            if (this.fullScreen == fullScreen)
                return;

            resizedCaptionFont = null;

            this.fullScreen = fullScreen;

            if (fullScreen)
            {
                Cursor.Hide();

                this.windowModePosition = new Point(this.Left, this.Top);
                this.FormBorderStyle = FormBorderStyle.None;
                this.MainMenuStrip.Hide();
                this.statusStrip.Hide();
                this.Left = this.Top = 0;
                int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
                int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

                this.Width = screenWidth;
                this.Height = screenHeight;
                bufferSize.Width = screenHeight * 282 / 256;
                bufferSize.Height = screenHeight + 1;

                UpdateResizedScreenFilter();
            }
            else
            {
                Cursor.Show();

                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.MainMenuStrip.Show();
                this.statusStrip.Show();
                SetScreen(screenSize, tvAspect);
                this.Left = windowModePosition.X;
                this.Top = windowModePosition.Y;
            }
        }

        private void UpdateResizedScreenFilter()
        {
            Image sourceFilter = null;
            switch (screenFilter)
            {
                case ScreenFilter.Raster: sourceFilter = Properties.Resources.FilterRaster; break;
                case ScreenFilter.Lcd: sourceFilter = Properties.Resources.FilterLcd; break;
            }

            if (sourceFilter != null)
                this.resizedScreenFilter = ResizeImage(sourceFilter,
                    bufferSize.Width, bufferSize.Height);
        }

        private void SetScreenFilter(ScreenFilter newScreenFilter)
        {
            screenFilter = newScreenFilter;
            UpdateResizedScreenFilter();

            viewScreenFilterNoneMenuItem.Checked = screenFilter == ScreenFilter.None;
            viewScreenFilterRasterMenuItem.Checked = screenFilter == ScreenFilter.Raster;
            viewScreenFilterLcdMenuItem.Checked = screenFilter == ScreenFilter.Lcd;
        }

        private void SetMotionBlur(bool newMotionBlur)
        {
            motionBlur = newMotionBlur;
            viewMotionBlurMenuItem.Checked = motionBlur;
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.Low;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public NesCore.Console Console { get; private set; }

        private Cartridge cartridge;
        private string cartridgeRomFilename;
        private string cartridgeSaveRamFilename;
        private RecentFileManager recentFileManager;

        private FastBitmap bitmapBuffer;
        private GameState gameState;
        private DateTime gameTickDateTime;
        private Icon gameIcon;

        private Image resizedScreenFilter;
        private Font resizedCaptionFont;

        // frame rate handling
        private DateTime frameDateTime;
        private double averageDeltaTime;

        // view size
        private Size applicationMargin;
        private Size bufferSize;
        private byte screenSize;
        private bool fullScreen;
        private Point windowModePosition;
        private bool tvAspect;
        private ScreenFilter screenFilter;
        private bool motionBlur;

        // audio system
        private WaveOut waveOut;
        private ApuAudioProvider apuAudioProvider;

        // input system
        private KeyboardState keyboardState;
        private GameControllerManager gameControllerManager;

        // debug
        private CodeDisassemblyForm codeDisassemblyForm;
        private bool traceEnabled;
    }
}
