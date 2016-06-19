using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES.Settings
{
    [Serializable]
    public class ControllerSettings
    {
        public byte Port { get; set; }
    }
}
