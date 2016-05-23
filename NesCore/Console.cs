using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NesCore.Processor;
using NesCore.Video;
using NesCore.Memory;
using NesCore.Input;

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
        public RicohRP2C0X Video { get; private set; }
        public ConfigurableMemoryMap Memory { get; private set; }

        public void ConnectControllerOne(Controller controller)
        {
            ConnectController(0x4016, controller);
        }

        public void ConnectControllerTwo(Controller controller)
        {
            ConnectController(0x4017, controller);
        }

        private ConfigurableMemoryMap CreateConfiguredMemoryMap()
        {
            ConfigurableMemoryMap memory = new ConfigurableMemoryMap(MemorySize);

            // work ram mirrored 4 times (4 x $800 segments)
            memory.ConfigureAddressMirroring(0x000, 0x0800, 0x2000);

            // $2000-$2007 PPU registers

            // $2000 PPUCTRL
            memory.ConfigureMemoryRead(0x2000, (address) => Video.Control);
            memory.ConfigureMemoryWrite(0x2000, (address, value) => Video.Control = value);

            // $2001 PPUMASK
            memory.ConfigureMemoryRead(0x2001, (address) => Video.Mask);
            memory.ConfigureMemoryWrite(0x2001, (address, value) => Video.Mask = value);

            // $2002 PPUSTATUS
            memory.ConfigureMemoryRead(0x2002, (address) => Video.Status);
            memory.ConfigureMemoryWrite(0x2002, (address, value) => Video.Status = value);

            // $2003 OAMADDR
            memory.ConfigureMemoryRead(0x2003, (address) => Video.ObjectAttributeMemoryAddress);
            memory.ConfigureMemoryWrite(0x2003, (address, value) => Video.ObjectAttributeMemoryAddress = value);

            // $2004 OAMDATA
            memory.ConfigureMemoryRead(0x2004, (address) => Video.ObjectAttributeMemoryData);
            memory.ConfigureMemoryWrite(0x2004, (address, value) => Video.ObjectAttributeMemoryData = value);

            // $2005 PPUSCROLL
            memory.ConfigureMemoryRead(0x2005, (address) => Video.Scroll);
            memory.ConfigureMemoryWrite(0x2005, (address, value) => { Video.Scroll = value; });

            // $2006 PPUADDR
            memory.ConfigureMemoryRead(0x2006, (address) => Video.Address);
            memory.ConfigureMemoryWrite(0x2006, (address, value) => Video.Address = value);

            // $2007 PPUDATA
            memory.ConfigureMemoryRead(0x2007, (address) => Video.Data);
            memory.ConfigureMemoryWrite(0x2007, (address, value) => Video.Data = value);

            // 8 PPU registers mirrered all the way up to $3FFF
            memory.ConfigureAddressMirroring(0x2000, 0x0008, 0x2000);

            // $4000-$401F APU and I/O registers (TODO)

            // $4014 OAMDMA
            memory.ConfigureMemoryRead(0x4014, (address) => Video.ObjectAttributeDirectMemoryAccess);
            memory.ConfigureMemoryWrite(0x4014, (address, value) =>
            {
                Video.ObjectAttributeDirectMemoryAccess = value;

                // DMA causes processor to stall for 513 or 514 cycles
                Processor.State.StallCycles += 513;
                if (Processor.State.Cycles % 2 == 1)
                    ++Processor.State.StallCycles;
            });

            // $4020-$FFFF Cartridge space: PRG ROM, PRG RAM, and mapper registers (TODO)

            return memory;
        }

        private void ConnectController(ushort portAddress, Controller controller)
        {
            if (portAddress != 0x4016 && portAddress != 0x4017)
                throw new ArgumentOutOfRangeException("portAddress", portAddress, "Allowed port addresses are $4016 ans $4017");
            Memory.ConfigureMemoryRead(portAddress, (address) => controller.Port);
            Memory.ConfigureMemoryWrite(portAddress, (address, value) => controller.Port = value);
        }

        private const uint MemorySize = ushort.MaxValue + 1;
    }
}
