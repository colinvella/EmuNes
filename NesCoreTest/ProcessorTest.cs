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
        public void TestAdcAbZeroes()
        {
            Processor processor = new Processor(this);
            State state = processor.State;

            state.Accumulator = 0;
            state.ProgramCounter = 0;
            Write(0, 0x6D); // ADC/AB
            Write(1, 0x00);
            Write(2, 0xC0);
            Write(0xC000, 0);

            ulong cycles = processor.ExecuteInstruction();

            Assert.IsTrue(state.ProgramCounter == 3);
            Assert.IsTrue(!state.CarryFlag);
            Assert.IsTrue(!state.NegativeFlag);
            Assert.IsTrue(state.ZeroFlag);
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
