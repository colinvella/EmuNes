using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage
{
    public class Cartridge
    {
        public IReadOnlyList<byte> ProgramBanks
        {
            get { return programBanks; }
            private set
            {
                programBanks = value;
            }
        }

        private IReadOnlyList<byte> programBanks;
    }
}
