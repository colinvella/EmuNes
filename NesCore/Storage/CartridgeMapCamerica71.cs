using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    class CartridgeMapCamerica71 : CartridgeMapUxRom
    {
        public CartridgeMapCamerica71(Cartridge cartridge)
            : base(cartridge)
        {
        }

        public override string Name { get { return "Camerica71"; } }
    }
}
