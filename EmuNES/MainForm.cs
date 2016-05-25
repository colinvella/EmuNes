using NesCore.Input;
using NesCore.Storage;
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

            keyPressed = new Dictionary<Keys, bool>();

            Console = new NesCore.Console();

            ConfigureDefaultController();
        }

        private void OnApplicationClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            cancelEventArgs.Cancel = MessageBox.Show(
                this, "Are you sure?", "Exit " + Application.ProductName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No;
        }

        private void OnGameOpen(object sender, EventArgs e)
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

        private void OnGameExit(object sender, EventArgs e)
        {
            Application.Exit();
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

        public NesCore.Console Console { get; private set; }

        private Dictionary<Keys, bool> keyPressed;

    }
}
