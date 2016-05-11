using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NesCore.Processor;
using NesCore.Memory;

namespace NesCore
{
    public class Console
    {
        public Console()
        {
            Processor = new Mos6502();

            Memory = CreateConfiguredMemoryMap();

            // connect processor to memory
            Processor.ReadByte = (ushort address) => { return Memory[address]; };
            Processor.WriteByte = (ushort address, byte value) => { Memory[address] = value; };
        }

        public Mos6502 Processor { get; private set; }
        public IMemoryMap Memory { get; private set; }

        private IMemoryMap CreateConfiguredMemoryMap()
        {
            ConfigurableMemoryMap memory = new ConfigurableMemoryMap();

            // work ram mirrored 4 times (4 x $800 segments)
            memory.ConfigureAddressMirroring(0x000, 0x0800, 0x2000);

            // $2000-$2007 PPU registers (TODO)
            memory.ConfigureMemoryRead(0x2000, (address) => { return 0; } );
            memory.ConfigureMemoryWrite(0x2000, (address, value) => { } );

            // 8 PPU registers mirrered all the way up to $3FFF
            memory.ConfigureAddressMirroring(0x2000, 0x0008, 0x2000);

            // $4000-$401F APU and I/O reSgisters (TODO)

            // $4020-$FFFF Cartridge space: PRG ROM, PRG RAM, and mapper registers (TODO)

            return memory;
        }
    }
}
