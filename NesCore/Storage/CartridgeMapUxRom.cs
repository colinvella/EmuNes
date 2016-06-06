using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapUxRom : CartridgeMapNRom
    {
        public CartridgeMapUxRom(Cartridge cartridge)
            : base(cartridge)
        {
        }

        public override string Name { get { return "UXROM"; } }
    }
}
