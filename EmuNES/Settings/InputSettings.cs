using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes.Settings
{
    [Serializable]
    public class InputSettings
    {
        public InputSettings()
        {
            controllerMap = null;
            Joypads = new List<JoypadSettings>();
            Zappers = new List<ZapperSettings>();
        }

        public void BuildDefaultSettings()
        {
            Joypads.Clear();
            JoypadSettings joypadSettings = new JoypadSettings();
            joypadSettings.Port = 1;
            joypadSettings.Start = joypadSettings.EncodeKeyboardMapping(Keys.Enter);
            joypadSettings.Select = joypadSettings.EncodeKeyboardMapping(Keys.Tab);
            joypadSettings.A = joypadSettings.EncodeKeyboardMapping(Keys.Z);
            joypadSettings.B = joypadSettings.EncodeKeyboardMapping(Keys.X);
            joypadSettings.Up = joypadSettings.EncodeKeyboardMapping(Keys.Up);
            joypadSettings.Down = joypadSettings.EncodeKeyboardMapping(Keys.Down);
            joypadSettings.Left = joypadSettings.EncodeKeyboardMapping(Keys.Left);
            joypadSettings.Right = joypadSettings.EncodeKeyboardMapping(Keys.Right);

            Joypads.Add(joypadSettings);

            ZapperSettings zapperSettings = new ZapperSettings();
            zapperSettings.Port = 2;
            zapperSettings.Trigger = "mouse:left";
            zapperSettings.LightSense = "mouse:cursor";

            Zappers.Add(zapperSettings);
        }

        public ControllerSettings GetControllerForPort(byte port)
        {
            foreach (JoypadSettings joypadSettings in Joypads)
                if (joypadSettings.Port == port)
                    return joypadSettings;

            // TODO: check for zappers etc. 

            return null;
        }

        public ControllerSettings this[byte port]
        {
            get
            {
                EnsureMapInitialised();
                ControllerSettings controllerSettings = null;
                controllerMap.TryGetValue(port, out controllerSettings);
                return controllerSettings;
            }
            set
            {
                EnsureMapInitialised();
                controllerMap[port] = value;
                UpdateControllerLists();
            }
        }

        public List<JoypadSettings> Joypads { get; private set; }

        public List<ZapperSettings> Zappers { get; private set; }

        public InputSettings Duplicate()
        {
            InputSettings copy = new InputSettings();
            foreach (JoypadSettings joypad in Joypads)
                copy.Joypads.Add(joypad.Duplicate());
            foreach (ZapperSettings zapper in Zappers)
                copy.Zappers.Add(zapper.Duplicate());
            return copy;
        }

        private void EnsureMapInitialised()
        {
            if (controllerMap == null)
            {
                controllerMap = new Dictionary<byte, ControllerSettings>();

                foreach (JoypadSettings joypadSettings in Joypads)
                    controllerMap[joypadSettings.Port] = joypadSettings;

                foreach (ZapperSettings zapperSettings in Zappers)
                    controllerMap[zapperSettings.Port] = zapperSettings;
            }
        }

        private void UpdateControllerLists()
        {
            Joypads.Clear();
            Zappers.Clear();

            foreach (ControllerSettings controllerSettings in controllerMap.Values)
            {
                if (controllerSettings is JoypadSettings)
                    Joypads.Add((JoypadSettings)controllerSettings);

                if (controllerSettings is ZapperSettings)
                    Zappers.Add((ZapperSettings)controllerSettings);
            }
        }

        private Dictionary<byte, ControllerSettings> controllerMap;
    }
}
