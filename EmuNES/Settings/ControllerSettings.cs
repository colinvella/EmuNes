using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES.Settings
{
    [Serializable]
    public class ControllerSettings
    {
        [Browsable(false)]
        public byte Port { get; set; }
    }
}
