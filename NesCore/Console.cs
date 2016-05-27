using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NesCore.Processor;
using NesCore.Video;
using NesCore.Memory;
using NesCore.Input;
using NesCore.Storage;

namespace NesCore
{
    public class Console
    {
        public Console()
        {
            // create main components
            Processor = new Mos6502();
            Video = new RicohRP2C0X();
            Memory = new ConfigurableMemoryMap();

            // configure hardware and connect components
            ConfigureMemoryMap();

            // connect processor it to memory
            Processor.ReadByte = (ushort address) => { return Memory[address]; };
            Processor.WriteByte = (ushort address, byte value) => { Memory[address] = value; };

            // wire NMI between video and processor
            Video.TriggerNonMaskableInterupt = () => Processor.TriggerNonMaskableInterrupt();

            // connect video to memory for DMA operations
            Video.ReadByte = (ushort address) => { return Memory[address]; };

            // connect default first controller
            ConnectControllerOne(new Joypad());
        }

        public Mos6502 Processor { get; private set; }
        public RicohRP2C0X Video { get; private set; }
        public ConfigurableMemoryMap Memory { get; private set; }

        /// <summary>
        /// Connect a controller to the first port
        /// </summary>
        /// <param name="controller"></param>
        public void ConnectControllerOne(Controller controller)
        {
            ConnectController(0x4016, controller);
        }

        /// <summary>
        /// Connect a controller to the second port
        /// </summary>
        /// <param name="controller"></param>
        public void ConnectControllerTwo(Controller controller)
        {
            ConnectController(0x4017, controller);
        }

        /// <summary>
        /// Maps the cartridge into the address range $6000-$FFFF
        /// </summary>
        /// <param name="cartridge"></param>
        public void LoadCartridge(Cartridge cartridge)
        {
            // connect $6000 upwards to main memory
            Memory.ConfigureMemoryAccessRange(0x6000, 0xA000,
                (address) => cartridge.Map[address],
                (address, value) => cartridge.Map[address] = value);

            // connect $0000-$1FFF to ROM CHR
            Video.Memory.ConfigureMemoryAccessRange(0x0000, 0x2000,
                (address) => cartridge.Map[address],
                (address, value) => cartridge.Map[address] = value);

            // name table mirroring mode determined from cartridge rom
            Video.ConfigureNameTableMirroringMode(cartridge.MirrorMode);
        }

        /// <summary>
        /// Resets the console
        /// </summary>
        public void Reset()
        {
            Processor.Reset();
            Video.Reset();
        }

        /// <summary>
        /// Steps through one CPU instruction and corresponding video and audio cycles
        /// </summary>
        /// <returns>cycles consumed</returns>
        public ulong Step()
        {
            ulong cpuCycles = Processor.ExecuteInstruction();
            ulong ppuCycles = cpuCycles * 3;
	        for (ulong i = 0; i < ppuCycles; i++)
            {
                Video.Step();
		        // cartridge map step goes here
	        }
	        for (ulong i = 0; i < cpuCycles; i++)
            {
                // audio step goes here
	        }
            return cpuCycles;
        }

        /// <summary>
        /// Execute for the given time frame
        /// </summary>
        /// <param name="deltaTime">Time frame for which to execute</param>
        /// <returns></returns>
        public ulong Run(double deltaTime)
        {
            long pendingCycles = (long)(deltaTime * Mos6502.Frequency);

            ulong consumedCycles = 0;
            while (pendingCycles > 0)
            {
                ulong stepCycles = Step();
                pendingCycles -= (long)stepCycles;
                consumedCycles += stepCycles;
            }

            return consumedCycles;
        }

        private void ConfigureMemoryMap()
        {
            // work ram mirrored 4 times (4 x $800 segments)
            Memory.ConfigureAddressMirroring(0x000, 0x0800, 0x2000);

            // $2000-$2007 PPU registers

            // $2000 PPUCTRL
            Memory.ConfigureMemoryRead(0x2000, (address) => Video.Control);
            Memory.ConfigureMemoryWrite(0x2000, (address, value) => Video.Control = value);

            // $2001 PPUMASK
            Memory.ConfigureMemoryRead(0x2001, (address) => Video.Mask);
            Memory.ConfigureMemoryWrite(0x2001, (address, value) => Video.Mask = value);

            // $2002 PPUSTATUS
            Memory.ConfigureMemoryRead(0x2002, (address) => Video.Status);
            Memory.ConfigureMemoryWrite(0x2002, (address, value) => Video.Status = value);

            // $2003 OAMADDR
            Memory.ConfigureMemoryRead(0x2003, (address) => Video.ObjectAttributeMemoryAddress);
            Memory.ConfigureMemoryWrite(0x2003, (address, value) => Video.ObjectAttributeMemoryAddress = value);

            // $2004 OAMDATA
            Memory.ConfigureMemoryRead(0x2004, (address) => Video.ObjectAttributeMemoryData);
            Memory.ConfigureMemoryWrite(0x2004, (address, value) => Video.ObjectAttributeMemoryData = value);

            // $2005 PPUSCROLL
            Memory.ConfigureMemoryRead(0x2005, (address) => Video.Scroll);
            Memory.ConfigureMemoryWrite(0x2005, (address, value) => { Video.Scroll = value; });

            // $2006 PPUADDR
            Memory.ConfigureMemoryRead(0x2006, (address) => Video.Address);
            Memory.ConfigureMemoryWrite(0x2006, (address, value) => Video.Address = value);

            // $2007 PPUDATA
            Memory.ConfigureMemoryRead(0x2007, (address) => Video.Data);
            Memory.ConfigureMemoryWrite(0x2007, (address, value) => Video.Data = value);

            // 8 PPU registers mirrered all the way up to $3FFF
            Memory.ConfigureAddressMirroring(0x2000, 0x0008, 0x2000);

            // $4000-$401F APU and I/O registers (TODO)

            // $4014 OAMDMA
            Memory.ConfigureMemoryRead(0x4014, (address) => Video.ObjectAttributeDirectMemoryAccess);
            Memory.ConfigureMemoryWrite(0x4014, (address, value) =>
            {
                Video.ObjectAttributeDirectMemoryAccess = value;

                // DMA causes processor to stall for 513 or 514 cycles
                Processor.State.StallCycles += 513;
                if (Processor.State.Cycles % 2 == 1)
                    ++Processor.State.StallCycles;
            });

            // $4020-$FFFF Cartridge space: PRG ROM, PRG RAM, and mapper registers (TODO)
        }

        private void ConnectController(ushort portAddress, Controller controller)
        {
            if (portAddress != 0x4016 && portAddress != 0x4017)
                throw new ArgumentOutOfRangeException("portAddress", portAddress, "Allowed port addresses are $4016 ans $4017");
            Memory.ConfigureMemoryRead(portAddress, (address) => controller.Port);
            Memory.ConfigureMemoryWrite(portAddress, (address, value) => controller.Port = value);
        }

        private ushort MirrorAddress(byte mode, ushort address)
        {
            address = (ushort)((address - 0x2000) % 0x1000);
            int table = address / 0x0400;
            int offset = address % 0x0400;
            return (ushort)(0x2000 + mirrorLookup[mode][table] * 0x0400 + offset);
        }

        private const uint MemorySize = ushort.MaxValue + 1;

        private static readonly ushort[][] mirrorLookup = {
            new ushort[]{0, 0, 1, 1},
            new ushort[]{0, 1, 0, 1},
            new ushort[]{0, 0, 0, 0},
            new ushort[]{1, 1, 1, 1},
            new ushort[]{0, 1, 2, 3}
        };
    }
}
