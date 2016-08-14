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

        public ZapperSettings Duplicate()
        {
            ZapperSettings copy = new ZapperSettings();
            copy.Port = Port;
            copy.Trigger = Trigger;
            return copy;
        }

        public Zapper ConfigureZapper(KeyboardState keyboardState, GameControllerManager gameControllerManager)
        {
            Zapper zapper = new Zapper();
            zapper.Trigger = DecodeMapping(Trigger, keyboardState, gameControllerManager);
            return zapper;
        }
    }
}
