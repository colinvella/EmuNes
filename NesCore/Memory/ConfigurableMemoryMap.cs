using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Memory
{
    public class ConfigurableMemoryMap : IMemoryMap
    {
        public const int Size = ushort.MaxValue + 1;

        public delegate byte ReadMemoryHandler(ushort address);
        public delegate void WriteMemoryHandler(ushort address, byte value);

        public ConfigurableMemoryMap()
        {
            readMemoryHandlers = new ReadMemoryHandler[Size];
            writeMemoryHandlers = new WriteMemoryHandler[Size];
            memory = new byte[Size];

            ResetConfiguration();
        }

        public void ResetConfiguration()
        {
            for (int handlerIndex = 0; handlerIndex < Size; handlerIndex++)
            {
                readMemoryHandlers[handlerIndex] = (address) => { return memory[address]; };
                writeMemoryHandlers[handlerIndex] = (address, value) => { memory[address] = value; };
            }
        }

        /// <summary>
        /// Initialises all the memory locations to zeroes
        /// </summary>
        public void Wipe()
        {
            for (int address = 0; address < Size; address++)
                this[(ushort)address] = 0;
        }

        /// <summary>
        /// Reads or writes a byte value at the given address 
        /// </summary>
        /// <param name="address">Address from where to read or where to write byte value</param>
        /// <returns></returns>
        public byte this[ushort address]
        {
            get { return readMemoryHandlers[address](address); }
            set { writeMemoryHandlers[address](address, value);  }
        }

        /// <summary>
        /// Configure memory mirroring for a given address range
        /// </summary>
        /// <param name="baseAddress">base address of the memory segment to be mirrored</param>
        /// <param name="mirroredSegmentLength">length of the memory to be mirrored, starting at the base address</param>
        /// <param name="range">length of the memory range to be mirrored (a multiple of the mirrored segment length)</param>
        public void ConfigureAddressMirroring(ushort baseAddress, ushort mirroredSegmentLength, ushort range)
        {
            if (baseAddress + range > Size)
                throw new ArgumentException("Mirrored address range exceeds memory limit", "range");
            if (range % mirroredSegmentLength != 0)
                throw new ArgumentException("The range is not divisible by the mirrored segment length", "mirroredSegmentLength");

            int endAddressExclusive = baseAddress + range;
            for (int handlerIndex = baseAddress + mirroredSegmentLength; handlerIndex < endAddressExclusive; handlerIndex++)
            {
                readMemoryHandlers[handlerIndex] = (address) =>
                {
                    ushort mirroredAddress = (ushort)(baseAddress + (address - baseAddress) % mirroredSegmentLength);
                    return this[mirroredAddress]; 
                };

                writeMemoryHandlers[handlerIndex] = (address, value) =>
                {
                    ushort mirroredAddress = (ushort)(baseAddress + (address - baseAddress) % mirroredSegmentLength);
                    this[mirroredAddress] = value;
                };
            }
        }

        /// <summary>
        /// Configure memory read and write handlers for a given memory address range
        /// </summary>
        /// <param name="startAddress">starting address to configure</param>
        /// <param name="length">length of the address range to configure</param>
        /// <param name="readMemoryHandler">delegate to handle memory reads at the given address range</param>
        /// <param name="writeMemoryHandler">delegate to handle memory writes at the given address range</param>
        public void ConfigureMemoryAccessRange(ushort startAddress, ushort length, ReadMemoryHandler readMemoryHandler, WriteMemoryHandler writeMemoryHandler)
        {
            int endAddressExclusive = startAddress + length;

            if (endAddressExclusive > Size)
                throw new ArgumentOutOfRangeException("length");

            for (ushort address = startAddress; address < endAddressExclusive; address++)
            {
                readMemoryHandlers[address] = readMemoryHandler;
                writeMemoryHandlers[address] = writeMemoryHandler;
            }
        }

        /// <summary>
        /// Configure a memory read handler for a given memory address range
        /// </summary>
        /// <param name="startAddress">starting address to configure</param>
        /// <param name="length">length of the address range to configure</param>
        /// <param name="readMemoryHandler">delegate to handle memory reads at the given address range</param>
        public void ConfigureMemoryReadRange(ushort startAddress, ushort length, ReadMemoryHandler readMemoryHandler)
        {
            int endAddressExclusive = startAddress + length;

            if (endAddressExclusive > Size)
                throw new ArgumentOutOfRangeException("length");

            for (ushort address = startAddress; address < endAddressExclusive; address++)
                readMemoryHandlers[address] = readMemoryHandler;
        }

        /// <summary>
        /// Configure a memory write handler for a given memory address range
        /// </summary>
        /// <param name="startAddress">starting address to configure</param>
        /// <param name="length">length of the address range to configure</param>
        /// <param name="writeMemoryHandler">delegate to handle memory writes at the given address range</param>
        public void ConfigureMemoryWriteRange(ushort startAddress, ushort length, WriteMemoryHandler writeMemoryHandler)
        {
            int endAddressExclusive = startAddress + length;

            if (endAddressExclusive > Size)
                throw new ArgumentOutOfRangeException("length");

            for (ushort address = startAddress; address < endAddressExclusive; address++)
                writeMemoryHandlers[address] = writeMemoryHandler;
        }

        /// <summary>
        /// Configure memory read and write handlers for a given memory address
        /// </summary>
        /// <param name="address">Memory address to associate to the handlers</param>
        /// <param name="readMemoryHandler">delegate to handle memory reads at the given address</param>
        /// <param name="writeMemoryHandler">delegate to handle memory writes at the given address</param>
        public void ConfigureMemoryAccess(ushort address, ReadMemoryHandler readMemoryHandler, WriteMemoryHandler writeMemoryHandler)
        {
            readMemoryHandlers[address] = readMemoryHandler;
            writeMemoryHandlers[address] = writeMemoryHandler;
        }

        /// <summary>
        /// Configure a memory read handler for a given memory address
        /// </summary>
        /// <param name="address">Memory address to associate to the read handler</param>
        /// <param name="readMemoryHandler">delegate to handle memory reads at the given address</param>
        public void ConfigureMemoryRead(ushort address, ReadMemoryHandler readMemoryHandler)
        {
            readMemoryHandlers[address] = readMemoryHandler;
        }

        /// <summary>
        /// Configure a memory write handler for a given memory address
        /// </summary>
        /// <param name="address">Memory address to associate to the write handler</param>
        /// <param name="writeMemoryHandler">delegate to handle memory writes at the given address</param>
        public void ConfigureMemoryWrite(ushort address, WriteMemoryHandler writeMemoryHandler)
        {
            writeMemoryHandlers[address] = writeMemoryHandler;
        }

        private byte[] memory;
        private ReadMemoryHandler[] readMemoryHandlers;
        private WriteMemoryHandler[] writeMemoryHandlers;
    }
}
