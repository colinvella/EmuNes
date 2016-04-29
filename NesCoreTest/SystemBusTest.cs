using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NesCore;

namespace NesCoreTest
{
    [TestClass]
    public abstract class SystemBusTest: SystemBus
    {
        public SystemBusTest()
        {
            WipeMemory();
        }
        public byte Read(ushort address)
        {
            return memory[address];
        }

        public void Write(ushort address, byte value)
        {
            memory[address] = value;
        }

        public void WipeMemory()
        {
            memory = new byte[UInt16.MaxValue];
        }

        private byte[] memory;
    }
}
