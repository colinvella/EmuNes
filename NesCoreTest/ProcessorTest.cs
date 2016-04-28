using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NesCore;
using NesCore.Processing;

namespace NesCoreTest
{
    [TestClass]
    public class ProcessorTest
        : SystemBus
    {
        [TestMethod]
        public void TestMethod1()
        {
            Processor processor = new Processor(this);
        }

        public byte Read(ushort address)
        {
            return memory[address];
        }

        public void Write(ushort address, byte value)
        {
            memory[address] = value;
        }

        private byte[] memory = new byte[UInt16.MaxValue];
    }
}
