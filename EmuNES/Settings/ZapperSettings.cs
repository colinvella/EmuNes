using SharpNes.Input;
using NesCore.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes.Settings
{
    [Serializable]
    public class ZapperSettings: ControllerSettings
    {
        public ZapperSettings() { }

        [Category("Zapper")] public string Trigger { get; set; }
        [Category("Zapper")] public string LightSense { get; set; }

        public ZapperSettings Duplicate()
        {
            ZapperSettings copy = new ZapperSettings();
            copy.Port = Port;
            copy.Trigger = Trigger;
            copy.LightSense = LightSense;
            return copy;
        }

        public Zapper ConfigureZapper(MouseState mouseState)
        {
            Zapper zapper = new Zapper();
            zapper.Trigger = () =>
            {
                return mouseState.LeftButtonPressed;
            };

            zapper.LightSense = () =>
            {
                return mouseState.SensePixel;
            };
            return zapper;
        }
    }
}
