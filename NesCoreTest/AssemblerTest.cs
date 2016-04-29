using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NesCore.Processing;
using NesCore.Utility;

namespace NesCoreTest
{
    [TestClass]
    public class AssemblerTest: SystemBusTest
    {
        public AssemblerTest()
        {
            processor = new Processor(this);
            assembler = new Assembler(processor);
        }

        [TestMethod]
        public void TestImpliedInstructions()
        {
            assembler.GenerateProgram(0x1000,
                @"NOP ;no operation
                  TAX ;transfer A to X");

            Assert.IsTrue(Read(0x1000) == 0xEA);
            Assert.IsTrue(Read(0x1001) == 0xAA);
        }

        [TestMethod]
        public void TestImmediateInstructions()
        {
            assembler.GenerateProgram(0x1000,
                @"LDX #$10 ;load value 16 in x register
                  INX      ;increment x register");

            Assert.IsTrue(Read(0x1000) == 0xA2);
            Assert.IsTrue(Read(0x1001) == 0x10);
            Assert.IsTrue(Read(0x1002) == 0xE8);
        }

        private Processor processor;
        private Assembler assembler;
    }
}
