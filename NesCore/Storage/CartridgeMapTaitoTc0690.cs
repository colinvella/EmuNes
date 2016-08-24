using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapTaitoTc0690 : CartridgeMapTaitoTc0190
    {
        public CartridgeMapTaitoTc0690(Cartridge cartridge) : base(cartridge)
        {
        }

        public override string Name { get { return "Taito TC0690"; } }
    }
}
