using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NesCore.Processing;
using NesCore.Utility;

namespace NesCoreTest
{
    [TestClass]
    public class ProcessorTest: SystemBusTest
    {
        public ProcessorTest()
        {
            processor = new Processor(this);
            assembler = new Assembler(processor);
        }

        [TestMethod]
        public void TestStack()
        {
            ResetSystem();
            processor.Push(0x69);
            Assert.IsTrue(processor.State.StackPointer == 0xFC, "SP did not update correctly (8bit)");
            Assert.IsTrue(processor.Pull() == 0x69, "push and pull error (8bit)");

            processor.Push16(0x1234);
            Assert.IsTrue(processor.State.StackPointer == 0xFB, "SP did not update correctly (16bit)");
            Assert.IsTrue(processor.Pull16() == 0x1234, "push and pull error (16bit)");
        }

        [TestMethod]
        public void TestInstructionBrk()
        {
            // assembler test
            ResetSystem();
            Write(0x1000, 0xDF);
            assembler.GenerateProgram(0x1000,
                @"BRK");
            Assert.IsTrue(Read(0x1000) == 0x00, "BRK instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.Write16(Processor.IrqVector, 0x2030);
            processor.State.InterruptDisableFlag = false;
            byte statusFlags = processor.State.Flags;

            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x2030, "PC not set");
            Assert.IsTrue(processor.State.InterruptDisableFlag, "Interrupt Disable flag not set");
            Assert.IsTrue(processor.Pull() == (byte)(statusFlags | 0x10), "status flags not preserved");
            Assert.IsTrue(processor.Pull16() == 0x1001, "PC not pushed on stack prior to BRK");

        }

        [TestMethod]
        public void TestImpliedInstructions()
        {
            ResetSystem();
            processor.Write16(Processor.IrqVector, 0x2030);
            assembler.GenerateProgram(0x1000,
                @"BRK");

            WipeMemory();
            assembler.GenerateProgram(0x1000,
                @"NOP ;no operation
                  TAX ;transfer A to X");

            Assert.IsTrue(Read(0x1000) == 0xEA);
            Assert.IsTrue(Read(0x1001) == 0xAA);
        }

        [TestMethod]
        public void TestImmediateInstructions()
        {
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDX #$10 ;load value 16 in x register
                  INX      ;increment x register
                  DEX      ;decrement x register
                  DEX      ;decrement x register");

            Assert.IsTrue(Read(0x1000) == 0xA2); //LDX
            Assert.IsTrue(Read(0x1001) == 0x10); //#$10
            Assert.IsTrue(Read(0x1002) == 0xE8); //INX
            Assert.IsTrue(Read(0x1003) == 0xCA); //DEX
            Assert.IsTrue(Read(0x1004) == 0xCA); //DEX

            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstructions(4);

            Assert.IsTrue(processor.State.RegisterX == 0x0F);
        }

        private void ResetSystem()
        {
            WipeMemory();
            processor.Reset();
        }

        private Processor processor;
        private Assembler assembler;
    }
}
