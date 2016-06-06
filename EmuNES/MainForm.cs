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
            keyPressed = new Dictionary<Keys, bool>();
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
                keyPressed[key] = false;

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
            recentFileManager = new RecentFileManager(fileRecentFilesMenuItem, LoadRecentRom);

            applicationMargin = new Size(Width - videoPanel.ClientSize.Width, Height - videoPanel.ClientSize.Height);
            SetScreen(1, true);
            SetRasterEffect(false);
            SetMotionBlur(false);
            viewScreenSizeFullScreenMenuItem.ShortcutKeys = Keys.Alt | Keys.Enter;

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
            switch(gameState)
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

        private void OnViewRasterEffect(object sender, EventArgs e)
        {
            SetRasterEffect(!rasterEffect);
        }

        private void OnViewMotionBlur(object sender, EventArgs e)
        {
            SetMotionBlur(!motionBlur);
        }


        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            keyPressed[keyEventArgs.KeyCode] = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            keyPressed[keyEventArgs.KeyCode] = false;
        }

        private void OnGameTick(object sender, EventArgs eventArgs)
        {
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
                    if (rasterEffect)
                    {
                        graphics.DrawImage(bitmapBuffer.Bitmap, leftCenteredMargin, 0, bufferSize.Width, bufferSize.Height);
                        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                        graphics.DrawImage(resizedRasterFilter, leftCenteredMargin, 0);
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

            float[] outputBuffer = new float[512];
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
            joypad.Start = () => keyPressed[Keys.Enter];
            joypad.Select = () => keyPressed[Keys.Tab];
            joypad.A = () => keyPressed[Keys.Z];
            joypad.B = () => keyPressed[Keys.X];
            joypad.Up = () => keyPressed[Keys.Up];
            joypad.Down = () => keyPressed[Keys.Down];
            joypad.Left = () => keyPressed[Keys.Left];
            joypad.Right = () => keyPressed[Keys.Right];

            Console.ConnectControllerOne(joypad);
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
                Stream cartridgeRomStream = GetCartridgeRomStream(cartridgeRomPath);

                BinaryReader romBinaryReader = new BinaryReader(cartridgeRomStream);
                this.cartridge = new Cartridge(romBinaryReader);
                romBinaryReader.Close();
                Console.LoadCartridge(cartridge);

                this.cartridgeRomFilename = Path.GetFileNameWithoutExtension(cartridgeRomPath);
                this.Text = cartridgeRomFilename + " - " + Application.ProductName;

                filePropertiesMenuItem.Enabled = true;
                gameMenuItem.Enabled = true;

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
            resizedRasterFilter = ResizeImage(Properties.Resources.RasterFilter, bufferSize.Width, bufferSize.Height);

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
                this.resizedRasterFilter = ResizeImage(Properties.Resources.RasterFilter,
                    bufferSize.Width, bufferSize.Height);
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.MainMenuStrip.Show();
                this.statusStrip.Show();
                SetScreen(screenSize, tvAspect);
                this.Left = windowModePosition.X;
                this.Top = windowModePosition.Y;
            }
        }
        
        private void SetRasterEffect(bool newRasterEffect)
        {
            rasterEffect = newRasterEffect;
            rasterEffectMenuItem.Checked = rasterEffect;
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
        private RecentFileManager recentFileManager;

        private Dictionary<Keys, bool> keyPressed;
        private FastBitmap bitmapBuffer;
        private GameState gameState;
        private DateTime gameTickDateTime;
        private Icon gameIcon;

        private Image resizedRasterFilter;
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
        private bool rasterEffect;
        private bool motionBlur;

        // audio system
        private WaveOut waveOut;
        private ApuAudioProvider apuAudioProvider;
    }
}
