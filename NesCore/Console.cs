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
using NesCore.Audio;

namespace NesCore
{
    public class Console
    {
        public Console()
        {
            // create main components
            Processor = new Mos6502();
            Video = new RicohRP2C0X();
            Audio = new Apu();
            ControllerMultiplexor1 = new ControllerMultiplexor();
            ControllerMultiplexor2 = new ControllerMultiplexor();
            Memory = new ConfigurableMemoryMap();

            allocatedCycles = 0;

            // configure hardware and connect components
            ConfigureMemoryMap();

            // connect processor to memory
            Processor.ReadByte = (ushort address) => { return Memory[address]; };
            Processor.WriteByte = (ushort address, byte value) => { Memory[address] = value; };

            // wire NMI between video and processor
            Video.TriggerNonMaskableInterupt = () => Processor.TriggerNonMaskableInterrupt();

            // connect video to memory for DMA operations
            Video.ReadByte = (ushort address) => { return Memory[address]; };

            // connect APU DMC to memory
            Audio.Dmc.ReadMemorySample = (address) =>
            {
                Processor.State.StallCycles += 4;
                return Memory[address];
            };

            // wire IRQ between audio and processor
            Audio.TriggerInterruptRequest = () => Processor.TriggerInterruptRequest();
            Audio.Dmc.TriggerInterruptRequest = () => Processor.TriggerInterruptRequest();

            // connect default first controller
            ConnectController(1, new Joypad());
        }

        public ConfigurableMemoryMap Memory { get; private set; }
        public Mos6502 Processor { get; private set; }
        public RicohRP2C0X Video { get; private set; }
        public Apu Audio { get; private set; }
        public ControllerMultiplexor ControllerMultiplexor1 { get; private set; }
        public ControllerMultiplexor ControllerMultiplexor2 { get; private set; }
        public Cartridge Cartridge { get; private set; }

        /// <summary>
        /// Connects a controller to the given port
        /// </summary>
        /// <param name="portNumber">Port number between 1 and 4</param>
        /// <param name="controller">Controller to connect</param>
        public void ConnectController(byte portNumber, Controller controller)
        {
            if (portNumber == 0 || portNumber > 3)
                throw new ArgumentOutOfRangeException("portNumber", "Port number must be between 1 and 4");

            switch (portNumber)
            {
                case 1: ControllerMultiplexor1.Primary = controller; break;
                case 2: ControllerMultiplexor2.Primary = controller; break;
                case 3: ControllerMultiplexor1.Secondary = controller; break;
                case 4: ControllerMultiplexor2.Secondary = controller; break;
            }
        }

        /// <summary>
        /// Maps the cartridge into the address range $6000-$FFFF
        /// </summary>
        /// <param name="cartridge"></param>
        public void LoadCartridge(Cartridge cartridge)
        {
            // connect $4100 upwards to main memory
            Memory.ConfigureMemoryAccessRange(0x4100, 0x10000 - 0x4100,
                (address) => cartridge.Map[address],
                (address, value) => cartridge.Map[address] = value);

            // connect $0000-$1FFF to ROM CHR
            Video.Memory.ConfigureMemoryAccessRange(0x0000, 0x2000,
                (address) => cartridge.Map[address],
                (address, value) => cartridge.Map[address] = value);

            // name table mirroring mode determined from cartridge rom
            // first time round on loading cartridge (for simple mappers)
            Video.ConfigureNameTableMirroringMode(cartridge.MirrorMode);

            // wired to mirror mode changes (for more complex mappers like MMC1)
            cartridge.Map.MirrorModeChanged = Video.ConfigureNameTableMirroringMode;

            // wire IRQ triggering for MMC3, MMC5 etc.
            cartridge.Map.TriggerInterruptRequest = Processor.TriggerInterruptRequest;
            cartridge.Map.CancelInterruptRequest = Processor.CancelInterruptRequest;

            // write sprite size change for MMC5
            Video.SpriteSizeChanged = (SpriteSize spriteSize) => cartridge.Map.SpriteSize = spriteSize;

            // wire sprite and background evaluation hooks for MMC5
            Video.EvaluatingSpriteData = () =>
                cartridge.Map.AccessingSpriteCharacters = true;
            Video.EvaluatingBackgroundData = () =>
                cartridge.Map.AccessingSpriteCharacters = false;

            // wire extended name table C
            Video.ReadNameTableC = cartridge.Map.ReadNameTableC;
            Video.WriteNameTableC = cartridge.Map.WriteNameTableC;

            // wire extended name table D
            Video.ReadNameTableD = cartridge.Map.ReadNameTableD;
            Video.WriteNameTableD = cartridge.Map.WriteNameTableD;

            // wire enhance nametable tile and attributes (e.g. for MMC5 ExRam Mode 1)
            Video.EnhanceTileByte = cartridge.Map.EnhanceTileIndex;
            Video.EnhanceTileAttributes = cartridge.Map.EnhanceTileAttributes;

            Cartridge = cartridge;
        }

        /// <summary>
        /// Resets the console
        /// </summary>
        public void Reset()
        {
            Processor.Reset();
            Video.Reset();

            Memory[0x4017] = Memory[0x4015] = 0x00;
            for (ushort registerAddress = 0x4000; registerAddress <= 0x400f; registerAddress++)
                Memory[registerAddress] = 0x00;
        }

        /// <summary>
        /// Steps through one CPU instruction and corresponding video and audio cycles
        /// </summary>
        /// <returns>cycles consumed</returns>
        public ulong Step()
        {
            // cpu emulation
            ulong cpuCycles = Processor.ExecuteInstruction();

            // vidoe emulation
            ulong ppuCycles = cpuCycles * 3;
	        for (ulong index = 0; index < ppuCycles; index++)
            {
                Video.Step();
                Cartridge.Map.StepVideo(Video.ScanLine, Video.Cycle, Video.ShowBackground, Video.ShowSprites);
	        }

            // audio emulation
	        for (ulong index = 0; index < cpuCycles; index++)
                Audio.Step();

            return cpuCycles;
        }

        /// <summary>
        /// Execute for the given time frame
        /// </summary>
        /// <param name="deltaTime">Time frame for which to execute</param>
        /// <returns></returns>
        public ulong Run(double deltaTime)
        {
            allocatedCycles += (long)(deltaTime * Mos6502.Frequency);

            ulong consumedCycles = 0;
            while (allocatedCycles > 0)
            {
                ulong stepCycles = Step();
                allocatedCycles -= (long)stepCycles;
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

            // APU Pulse 1

            // $4000 APU Pulse 1 Control
            Memory.ConfigureMemoryWrite(0x4000, (address, value) => Audio.Pulse1.Control = value);

            // $4001 APU Pulse 1 Sweep
            Memory.ConfigureMemoryWrite(0x4001, (address, value) => Audio.Pulse1.Sweep = value);

            // $4002 APU Pulse 1 Timer Low
            Memory.ConfigureMemoryWrite(0x4002, (address, value) => Audio.Pulse1.TimerLow = value);

            // $4003 APU Pulse 1 Timer High
            Memory.ConfigureMemoryWrite(0x4003, (address, value) => Audio.Pulse1.TimerHigh = value);

            // APU Pulse 2

            // $4004 APU Pulse 2 Control
            Memory.ConfigureMemoryWrite(0x4004, (address, value) => Audio.Pulse2.Control = value);

            // $4005 APU Pulse 2 Sweep
            Memory.ConfigureMemoryWrite(0x4005, (address, value) => Audio.Pulse2.Sweep = value);

            // $4006 APU Pulse 2 Timer Low
            Memory.ConfigureMemoryWrite(0x4006, (address, value) => Audio.Pulse2.TimerLow = value);

            // $4007 APU Pulse 2 Timer High
            Memory.ConfigureMemoryWrite(0x4007, (address, value) => Audio.Pulse2.TimerHigh = value);

            // APU Triangle

            // $4008 APU Triangle Control
            Memory.ConfigureMemoryWrite(0x4008, (address, value) => Audio.Triangle.Control = value);

            // $400A APU Triangle Timer Low
            Memory.ConfigureMemoryWrite(0x400A, (address, value) => Audio.Triangle.TimerLow = value);

            // $400B APU Triangle Timer High
            Memory.ConfigureMemoryWrite(0x400B, (address, value) => Audio.Triangle.TimerHigh = value);

            // APU Noise

            // $400C APU Noise Control
            Memory.ConfigureMemoryWrite(0x400C, (address, value) => Audio.Noise.Control = value);

            // $400E APU Noise Period
            Memory.ConfigureMemoryWrite(0x400E, (address, value) => Audio.Noise.ModeAndPeriod = value);

            // $400F APU Noise Length
            Memory.ConfigureMemoryWrite(0x400F, (address, value) => Audio.Noise.Length = value);

            // APU DMC

            // $4010 APU DMC Control
            Memory.ConfigureMemoryWrite(0x4010, (address, value) => Audio.Dmc.Control = value);

            // $4011 APU DMC Direct Load
            Memory.ConfigureMemoryWrite(0x4011, (address, value) => Audio.Dmc.SampleValue = value);

            // $4012 APU DMC Address
            Memory.ConfigureMemoryWrite(0x4012, (address, value) => Audio.Dmc.SampleAddress = value);

            // $4013 APU DMC Length
            Memory.ConfigureMemoryWrite(0x4013, (address, value) => Audio.Dmc.SampleLength = value);

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

            // $4015 APU Control/Status
            Memory.ConfigureMemoryAccess(0x4015, (address) => Audio.Status, (address, value) => Audio.Control = value);

            // $4016 controller read (joypad 1 and 3)
            Memory.ConfigureMemoryRead(0x4016, (address) => ControllerMultiplexor1.Port);

            // $4016 controller write (strobes all controllers)
            Memory.ConfigureMemoryWrite(0x4016, (address, value) => { ControllerMultiplexor1.Port = value; ControllerMultiplexor2.Port = value; });

            // $4017 controller read (joypad 2 and 4)
            Memory.ConfigureMemoryRead(0x4017, (address) => ControllerMultiplexor2.Port);

            // $4017 APU Frame Counter (write only)
            Memory.ConfigureMemoryWrite(0x4017, (address, value) => Audio.FrameCounter = value);

            // $4020-$FFFF Cartridge space: PRG ROM, PRG RAM, and mapper registers - done when cartridge loaded
        }

        private long allocatedCycles;

        private const uint MemorySize = ushort.MaxValue + 1;
    }
}
