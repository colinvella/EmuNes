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

            keyPressed = new Dictionary<Keys, bool>();

            Console = new NesCore.Console();

            ConfigureVideoBuffer();
            ConfigureDefaultController();

            gameIsRunning = false;

            bitmapBuffer = new Bitmap(256, 240);
            bitmapBuffer.SetPixel(100, 100, Color.Green);
        }

        private void OnApplicationClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            cancelEventArgs.Cancel = MessageBox.Show(
                this, "Are you sure?", "Exit " + Application.ProductName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No;
        }

        private void OnFileOpen(object sender, EventArgs e)
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

        private void OnFileExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ConfigureVideoBuffer()
        {
            Console.Video.WritePixel = (x, y, paletteIndex) =>
            {
                Colour gameColour = palette[paletteIndex];
                bitmapBuffer.SetPixel(x, y, Color.FromArgb(gameColour.Red, gameColour.Green, gameColour.Blue));
            };

            Console.Video.PresentFrame = () => videoPanel.Invalidate();
        }

        private void ConfigureDefaultController()
        {
            Joypad joypad = new Joypad();
            joypad.Start = () => keyPressed[Keys.F1];
            joypad.Select = () => keyPressed[Keys.F2];
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
                MessageBox.Show(this, cartridge.ToString(), "Open Game ROM", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ResetGame();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, "Unable to load cartridge rom. Reason: " + exception.Message, "Open Game ROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RunGame()
        {
            if (gameIsRunning)
                return;

            gameIsRunning = true;
            gameTimer.Enabled = true;
        }

        private void PauseGame()
        {
            if (!gameIsRunning)
                return;

            gameTimer.Enabled = false;
            gameIsRunning = false;
        }

        private void ResetGame()
        {
            PauseGame();
            Console.Reset();
            RunGame();
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
            if (!gameIsRunning)
                return;

            Console.Run(0.020);
        }

        private void OnVideoPanelPaint(object sender, PaintEventArgs paintEventArgs)
        {
            Graphics graphics = paintEventArgs.Graphics;
            graphics.DrawImage(bitmapBuffer, 0, 0, 512, 480);
        }

        public NesCore.Console Console { get; private set; }

        private Dictionary<Keys, bool> keyPressed;
        private Bitmap bitmapBuffer;
        private bool gameIsRunning;

        // temp
        private Palette palette = new Palette();
    }
}
