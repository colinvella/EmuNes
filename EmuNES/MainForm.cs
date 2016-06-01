using NesCore.Input;
using NesCore.Storage;
using NesCore.Video;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
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
        }

        private void OnFormLoad(object sender, EventArgs eventArgs)
        {
            applicationMargin = new Size(Width - videoPanel.Width, Height - videoPanel.Height);
            SetScreenSizeAndAspect(1, true);
            SetRasterEffect(false);
            SetMotionBlur(false);

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
            openFileDialog.Filter = "NES ROM files (*.nes)|*.nes|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "Open Game ROM";
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            LoadCartridgeRom(openFileDialog.FileName);
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
                    gameTimer.Enabled = true;
                    break;
                case GameState.Running:
                    // pause
                    gameTimer.Enabled = false;
                    gameState = GameState.Paused;
                    break;
                case GameState.Paused:
                    // resume;
                    gameTickDateTime = DateTime.Now;
                    averageDeltaTime = 1.0 / 60.0;
                    gameState = GameState.Running;
                    gameTimer.Enabled = true;
                    break;
            }

            videoPanel.Invalidate();
            UpdateGameMenuItems();
        }

        private void OnGameReset(object sender, EventArgs eventArgs)
        {
            OnGameStop(sender, eventArgs);
            Console.Reset();
            OnGameRunPause(sender, eventArgs);
        }

        private void OnGameStop(object sender, EventArgs eventArgs)
        {
            if (gameState == GameState.Stopped)
                return;

            gameTimer.Enabled = false;
            videoPanel.Invalidate();
            gameState = GameState.Stopped;

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

        private void OnVideoPanelPaint(object sender, PaintEventArgs paintEventArgs)
        {
            Graphics graphics = paintEventArgs.Graphics;

            switch (gameState)
            {
                case GameState.Stopped:
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(Properties.Resources.Background, 0, 0, videoPanel.Width, videoPanel.Height);
                    break;
                case GameState.Paused:
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(Properties.Resources.Background, 0, 0, videoPanel.Width, videoPanel.Height);
                    graphics.DrawImage(bitmapBuffer.Bitmap, 0, bufferSize.Height / 2, bufferSize.Width / 2, bufferSize.Height / 2);
                    break;
                case GameState.Running:
                    if (rasterEffect)
                    {
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(bitmapBuffer.Bitmap, 0, 0, bufferSize.Width, bufferSize.Height);
                        graphics.DrawImage(Properties.Resources.Filter, 0, 0, bufferSize.Width, bufferSize.Height);
                    }
                    else
                    {
                        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                        graphics.DrawImage(bitmapBuffer.Bitmap, 0, 0, bufferSize.Width, bufferSize.Height);
                    }
                    break;
            }
        }

        private void UpdateGameMenuItems()
        {
            switch (gameState)
            {
                case GameState.Stopped:
                    gameRunMenuItem.Text = "&Run";
                    gameRunMenuItem.Image = Properties.Resources.GameRun;
                    break;
                case GameState.Running:
                    gameRunMenuItem.Text = "&Pause";
                    gameRunMenuItem.Image = Properties.Resources.GamePause;
                    break;
                case GameState.Paused:
                    gameRunMenuItem.Text = "&Resume";
                    gameRunMenuItem.Image = Properties.Resources.GameRun;
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
                    bitmapBuffer.Bits[offset] = (byte)(bitmapBuffer.Bits[offset++] * 3 / 4 + colour.Blue * 1 / 4);
                    bitmapBuffer.Bits[offset] = (byte)(bitmapBuffer.Bits[offset++] * 3 / 4 + colour.Green * 1 / 4);
                    bitmapBuffer.Bits[offset] = (byte)(bitmapBuffer.Bits[offset++] * 3 / 4 + colour.Red * 1 / 4);
                }
                else
                {
                    bitmapBuffer.Bits[offset++] = colour.Blue;
                    bitmapBuffer.Bits[offset++] = colour.Green;
                    bitmapBuffer.Bits[offset++] = colour.Red;
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

            BinaryWriter bw = new BinaryWriter(new FileStream(Application.StartupPath + "\\audio.bin", FileMode.OpenOrCreate, FileAccess.Write));
            int i = 0;
            Console.Audio.WriteSample = (sampleValue) =>
            {
                // TODO: actual audio here
                bw.Write(sampleValue);
                bw.Flush();
                System.Console.WriteLine(i + ": " + sampleValue);
                ++i;
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

        private void LoadCartridgeRom(string cartridgeRomPath)
        {
            try
            {
                BinaryReader romBinaryReader = new BinaryReader(new FileStream(cartridgeRomPath, FileMode.Open));
                Cartridge cartridge = new Cartridge(romBinaryReader);
                romBinaryReader.Close();
                Console.LoadCartridge(cartridge);

                string cartridgeRomFilename = Path.GetFileNameWithoutExtension(cartridgeRomPath);
                this.Text = cartridgeRomFilename + " - " + Application.ProductName;

                gameMenuItem.Enabled = true;
                OnGameStop(this, EventArgs.Empty);
                OnGameRunPause(this, EventArgs.Empty);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, "Unable to load cartridge rom. Reason: " + exception.Message, "Open Game ROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetScreenSize(byte newScreenSize)
        {
            SetScreenSizeAndAspect(newScreenSize, tvAspect);
        }

        private void SetTvAspect(bool newTvAspect)
        {
            SetScreenSizeAndAspect(screenSize, newTvAspect);
        }

        private void SetScreenSizeAndAspect(byte newScreenSize, bool newTvAspect)
        {
            screenSize = newScreenSize;
            tvAspect = newTvAspect;

            bufferSize.Width = tvAspect ? 282 * screenSize : 256 * screenSize;
            bufferSize.Height = 240 * screenSize;

            Width = bufferSize.Width + applicationMargin.Width;
            Height = bufferSize.Height + applicationMargin.Height;

            viewScreenSizeX1MenuItem.Checked = newScreenSize == 1;
            viewScreenSizeX2MenuItem.Checked = newScreenSize == 2;
            viewScreenSizeX3MenuItem.Checked = newScreenSize == 3;
            viewScreenSizeX4MenuItem.Checked = newScreenSize == 4;
            viewTvAspectMenuItem.Checked = tvAspect;
            videoPanel.Invalidate();
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


        public NesCore.Console Console { get; private set; }

        private Dictionary<Keys, bool> keyPressed;
        private FastBitmap bitmapBuffer;
        private GameState gameState;
        private DateTime gameTickDateTime;

        // frame rate handling
        private DateTime frameDateTime;
        private double averageDeltaTime;

        // view size
        private Size applicationMargin;
        private Size bufferSize;
        private byte screenSize;
        private bool tvAspect;
        private bool rasterEffect;
        private bool motionBlur;

    }
}
