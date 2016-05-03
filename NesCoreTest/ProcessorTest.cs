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
        public void TestInstructionOraIzx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ORA ($10,X) ; OR accumulator with contents of address $2000 contained at address [$90, $91] computed by $10 offset from X ($80)");
            Assert.IsTrue(Read(0x1000) == 0x01, "ORA/IZX instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "Indexed indirect 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.Write16(0x0090, 0x2000);
            Write(0x2000, 0x0F);
            processor.State.Accumulator = 0xF0;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xFF, "Value $FF expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionOraZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ORA $10 ; OR accumulator with contents of address $0010");
            Assert.IsTrue(Read(0x1000) == 0x05, "ORA/ZP instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZP operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            Write(0x0010, 0x0F);
            processor.State.Accumulator = 0xF0;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xFF, "Value $FF expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionAslZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @";8 shifts
                  ASL $10 ; ASL contents of address $0010
                  ASL $10 ; ASL contents of address $0010
                  ASL $10 ; ASL contents of address $0010
                  ASL $10 ; ASL contents of address $0010
                  ASL $10 ; ASL contents of address $0010
                  ASL $10 ; ASL contents of address $0010
                  ASL $10 ; ASL contents of address $0010
                  ASL $10 ; ASL contents of address $0010");
            Assert.IsTrue(Read(0x1000) == 0x06, "ASL/ZP instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZP operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            Write(0x0010, 0x01);
 
            // execute first 7 ASL
            processor.ExecuteInstructions(7);

            Assert.IsTrue(Read(0x0010) == 0x80, "Value $80 expected at location $0010");
            Assert.IsTrue(!processor.State.CarryFlag, "Carry should not be set");

            // execute last ASL
            processor.ExecuteInstruction();
            Assert.IsTrue(Read(0x0010) == 0x00, "Value $00 expected at location $0010");
            Assert.IsTrue(processor.State.CarryFlag, "Carry should be set");
        }

        [TestMethod]
        public void TestInstructionPhp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"PHP");
            Assert.IsTrue(Read(0x1000) == 0x08, "PHP instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            byte statusFlags = processor.State.Flags;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.Pull() == (byte)(statusFlags | 0x10), "status flags not preserved");
        }

        [TestMethod]
        public void TestInstructionOraImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ORA #$0F; OR value $0F with accumulator");
            Assert.IsTrue(Read(0x1000) == 0x09, "ORA/IMM instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x0F, "Immediate value $0F not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0xF0;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xFF, "Value $FF expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionAslAcc()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @";8 shifts
                  ASL A;ASL contents of accumulator
                  ASL A;ASL contents of accumulator
                  ASL A;ASL contents of accumulator
                  ASL A;ASL contents of accumulator
                  ASL A;ASL contents of accumulator
                  ASL A;ASL contents of accumulator
                  ASL A;ASL contents of accumulator
                  ASL A;ASL contents of accumulator");
            Assert.IsTrue(Read(0x1000) == 0x0A, "ASL/Acc instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;

            // execute first 7 ASL
            processor.ExecuteInstructions(7);

            Assert.IsTrue(processor.State.Accumulator == 0x80, "Value $80 expected in accumulator");
            Assert.IsTrue(!processor.State.CarryFlag, "Carry should not be set");

            // execute last ASL
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.Accumulator == 0x00, "Value $00 expected in accumulator");
            Assert.IsTrue(processor.State.CarryFlag, "Carry should be set");
        }

        [TestMethod]
        public void TestInstructionOraAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ORA $2000; OR contents at address $2000 with accumulator");
            Assert.IsTrue(Read(0x1000) == 0x0D, "ORA/ABS instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "Operand should be $2000");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0xF0;
            processor.Write16(0x2000, 0x0F);

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xFF, "Value $FF expected in Accumulator");
        }


        [TestMethod]
        public void TestInstructionAslAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ASL $2000 ;ASL contents of address $2000");
            Assert.IsTrue(Read(0x1000) == 0x0E, "ASL/Abs instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            Write(0x2000, 0x04);
            processor.ExecuteInstruction();
            Assert.IsTrue(Read(0x2000) == 0x08, "Value $08 expected at address $2000");
        }

        [TestMethod]
        public void TestInstructionBpl()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDA #$01 ;load accumulator with positive value
                  BPL  $40 ;branch to $40 bytes ahead of instruction");
            Assert.IsTrue(Read(0x1000) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x01, "immediate operand $01 expected");
            Assert.IsTrue(Read(0x1002) == 0x10, "BPL instruction not assembled");
            Assert.IsTrue(Read(0x1003) == 0x40, "Relative branch offset $40 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstructions(2);
            Assert.IsTrue(processor.State.ProgramCounter == 0x1044, "Value $1044 expected in PC");
        }

        [TestMethod]
        public void TestInstructionOraIzy()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ORA ($10),Y ; OR accumulator with contents of zero page offset $10 (absolute $0010), offset by contents of Y register ($30)");
            Assert.IsTrue(Read(0x1000) == 0x11, "ORA/IZY instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "Indexed indirect 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.Write16(0x0010, 0x2000); // ($10) = ($0010) = $2000
            Write(0x2030, 0x0F); // ($10),Y = $2000 + $30 = $2030
            processor.State.Accumulator = 0xF0;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xFF, "Value $FF expected in Accumulator");
        }





        [TestMethod]
        public void TestLabelDeclarations()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"Start:
                    LDA #$01    ;load accumulator with $01 (positive)
                    BPL Middle  ;branch to Middle label
                    NOP         ;do nothing
                  Middle:
                    LDX #$02    ;load X wit $02 (positive)
                    BPL Start   ;branch to start if positive");
            Assert.IsTrue(Read(0x1000) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x01, "immediate operand $01 expected");
            Assert.IsTrue(Read(0x1002) == 0x10, "BPL instruction not assembled");
            Assert.IsTrue(Read(0x1003) == 0x01, "Computed relative branch offset $01 expected (to skip NOP)");
            Assert.IsTrue(Read(0x1004) == 0xEA, "NOP instruction not assembled");
            Assert.IsTrue(Read(0x1005) == 0xA2, "LDX/Imm instruction not assembled");
            Assert.IsTrue(Read(0x1006) == 0x02, "immediate operand $02 expected");
            Assert.IsTrue(Read(0x1007) == 0x10, "BPL instruction not assembled");
            Assert.IsTrue(Read(0x1008) == 0xF7, "Computed relative byte offset $F7 (-9) expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstructions(4);
            Assert.IsTrue(processor.State.ProgramCounter == 0x1000, "Value $1000 expected in PC (branch back to Start)");
            Assert.IsTrue(processor.State.Accumulator == 0x01, "value $01 expected in A");
            Assert.IsTrue(processor.State.RegisterX == 0x02, "value $02 expected in X");
        }



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
