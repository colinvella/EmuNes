using NesCore.Input;
using NesCore.Storage;
using NesCore.Video;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

            ConfigureVideoBuffer();
            ConfigureDefaultController();

            gameState = GameState.Stopped;

            bitmapBuffer = new FastBitmap(256, 240);
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

        private void UpdateGameMenuItems()
        {
            string runPauseText = null;
            switch (gameState)
            {
                case GameState.Stopped: runPauseText = "&Run"; break;
                case GameState.Running: runPauseText = "&Pause"; break;
                case GameState.Paused: runPauseText = "&Resume"; break;
            }
            gameRunMenuItem.Text = runPauseText;
            gameStopMenuItem.Enabled = gameState != GameState.Stopped;
        }

        private void ConfigureVideoBuffer()
        {
            Console.Video.WritePixel = (x, y, colour) =>
            {
                int offset = (y * 256 + x) * 4;
                bitmapBuffer.Bits[offset++] = colour.Blue;
                bitmapBuffer.Bits[offset++] = colour.Green;
                bitmapBuffer.Bits[offset++] = colour.Red;
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

            DateTime currentTickDateTime = DateTime.Now;
            double tickDelta = (currentTickDateTime - gameTickDateTime).TotalSeconds;
            gameTickDateTime = currentTickDateTime;

            Console.Run(tickDelta);
        }

        private void OnVideoPanelPaint(object sender, PaintEventArgs paintEventArgs)
        {
            Graphics graphics = paintEventArgs.Graphics;
            switch (gameState)
            {
                case GameState.Stopped:
                    graphics.DrawImage(Properties.Resources.Background, 0, 0, 512, 480);
                    break;
                case GameState.Paused:
                    graphics.DrawImage(Properties.Resources.Background, 0, 0, 512, 480);
                    graphics.DrawImage(bitmapBuffer.Bitmap, 0, 240, 256, 240);
                    break;
                case GameState.Running:
                    graphics.DrawImage(bitmapBuffer.Bitmap, 0, 0, 512, 480);
                    break;

            }
            //graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //graphics.DrawImage(Properties.Resources.Filter, 0, 0, 512, 480);
        }

        public NesCore.Console Console { get; private set; }

        private Dictionary<Keys, bool> keyPressed;
        private FastBitmap bitmapBuffer;
        private GameState gameState;
        private DateTime gameTickDateTime;
        private DateTime frameDateTime;
        private double averageDeltaTime;
    }
}
