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

        public void ConfigureMemoryRead(ushort address, ReadMemoryHandler readMemoryHandler)
        {
            readMemoryHandlers[address] = readMemoryHandler;
        }

        public void ConfigureMemoryWrite(ushort address, WriteMemoryHandler writeMemoryHandler)
        {
            writeMemoryHandlers[address] = writeMemoryHandler;
        }

        private byte[] memory;
        private ReadMemoryHandler[] readMemoryHandlers;
        private WriteMemoryHandler[] writeMemoryHandlers;
    }
}
