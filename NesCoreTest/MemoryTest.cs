using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NesCore;
using NesCore.Addressing;

namespace NesCoreTest
{
    [TestClass]
    public class MemoryTest
    {
        [TestMethod]
        public void ByteWrite()
        {
            NesCore.Console console = new NesCore.Console();
            Memory memory = console.Memory;
            memory.Write(0x1234, 0x10);
            Assert.AreEqual(memory.Read(0x1234),  0x10, "Byte Write Test Failed");
        }
    }
}
