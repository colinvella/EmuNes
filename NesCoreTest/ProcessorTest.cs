﻿using System;
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
                @"LDA #$FF ;load accumulator with negative value
                  BPL  $30 ;branch on plus to $30 bytes ahead of instruction
                  LDA #$01 ;load accumulator with positive value
                  BPL  $40 ;branch on plus to $40 bytes ahead of instruction");
            Assert.IsTrue(Read(0x1000) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0xFF, "immediate operand $FF expected");
            Assert.IsTrue(Read(0x1002) == 0x10, "BPL instruction not assembled");
            Assert.IsTrue(Read(0x1003) == 0x30, "Relative branch offset $30 expected");
            Assert.IsTrue(Read(0x1004) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(Read(0x1005) == 0x01, "immediate operand $01 expected");
            Assert.IsTrue(Read(0x1006) == 0x10, "BPL instruction not assembled");
            Assert.IsTrue(Read(0x1007) == 0x40, "Relative branch offset $40 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstructions(4);
            Assert.IsTrue(processor.State.ProgramCounter == 0x1048, "Value $1048 expected in PC");
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
        public void TestInstructionOraZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ORA $10,X ; OR accumulator with contents of address $0010 + X");
            Assert.IsTrue(Read(0x1000) == 0x15, "ORA/ZPX instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZPX operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            Write(0x0040, 0x0F);
            processor.State.Accumulator = 0xF0;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xFF, "Value $FF expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionAslZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ASL $20, X ;ASL contents of address $20 + X = $0020 + $10 = $0030");
            Assert.IsTrue(Read(0x1000) == 0x16, "ASL/Zpx instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x10;
            Write(0x0030, 0x04);
            processor.ExecuteInstruction();
            Assert.IsTrue(Read(0x0030) == 0x08, "Value $08 expected at address $0030");
        }

        [TestMethod]
        public void TestInstructionClc()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CLC ;clear carry flag");
            Assert.IsTrue(Read(0x1000) == 0x18, "CLC instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = true;
            processor.ExecuteInstruction();
            Assert.IsTrue(!processor.State.CarryFlag, "Carry flag expected to be False");
        }

        [TestMethod]
        public void TestInstructionOraAby()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ORA $2000,Y ; OR accumulator with contents of address $2000 + Y = $2000 + $30 = $2030");
            Assert.IsTrue(Read(0x1000) == 0x19, "ORA/ABY instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "ABY operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            Write(0x2030, 0x0F);
            processor.State.Accumulator = 0xF0;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xFF, "Value $FF expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionOraAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ORA $2000,X ; OR accumulator with contents of address $2000 + X = $2000 + $30 = $2030");
            Assert.IsTrue(Read(0x1000) == 0x1D, "ORA/ABX instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "ABX operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            Write(0x2030, 0x0F);
            processor.State.Accumulator = 0xF0;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xFF, "Value $FF expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionAslAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ASL $2000, X ;ASL contents of address $2000 + X = $2000 + $30 = $2030");
            Assert.IsTrue(Read(0x1000) == 0x1E, "ASL/Abx instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "ABX operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            Write(0x2030, 0x04);
            processor.ExecuteInstruction();
            Assert.IsTrue(Read(0x2030) == 0x08, "Value $08 expected at address $2030");
        }

        [TestMethod]
        public void TestInstructionJsrAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"JSR $2000 ;jump to subroutine at absolute $2000");
            Assert.IsTrue(Read(0x1000) == 0x20, "JSR/abs instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "ABS operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            Write(0x0030, 0x04);
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x2000, "PC expected to be $2000");
            Assert.IsTrue(processor.Pull16() == 0x1002, "Top of stack expected to hold address of instruction after JSR - 1");
        }

        [TestMethod]
        public void TestInstructionAndIzx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"AND ($10,X) ; AND accumulator with contents of address $2000 contained at address [$90, $91] computed by $10 offset from X ($80)");
            Assert.IsTrue(Read(0x1000) == 0x21, "AND/IZX instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "Izx 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.Write16(0x0090, 0x2000);
            Write(0x2000, 0xF0);
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xF0, "Value $F0 expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionBitZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BIT $10 ; bit test the contents of address $0010");
            Assert.IsTrue(Read(0x1000) == 0x24, "BIT/ZP instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZP operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = true;
            processor.State.NegativeFlag = false;
            processor.State.OverflowFlag = false;
            Write(0x0010, 0xC0); // $C0 = b11000000

            processor.ExecuteInstruction();

            Assert.IsTrue(!processor.State.ZeroFlag, "Zero flag expected to be clear");
            Assert.IsTrue(processor.State.NegativeFlag, "Negative (S) flag expected to be clear");
            Assert.IsTrue(processor.State.OverflowFlag, "Overflow flag expected to be clear");
        }

        [TestMethod]
        public void TestInstructionAndZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"AND $10 ; AND accumulator with contents of address $0010");
            Assert.IsTrue(Read(0x1000) == 0x25, "AND/ZP instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZP operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            Write(0x0010, 0x0F);
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x0F, "Value $0F expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionRolZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ROL $10 ; ROL contents of address $0010");
            Assert.IsTrue(Read(0x1000) == 0x26, "ROL/ZP instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZP operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            Write(0x0010, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(Read(0x0010) == 0x02, "Value $02 expected at address 0x0010");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 7)");
        }

        [TestMethod]
        public void TestInstructionPlp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"PHP ;push processor status
                  CLC ;clear carry
                  PLP ;pull processor status");
            Assert.IsTrue(Read(0x1000) == 0x08, "PHP instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = true;

            processor.ExecuteInstructions(2);
            Assert.IsTrue(!processor.State.CarryFlag, "carry flag should be clear");

            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.CarryFlag, "carry flag should be restored to set");
        }

        [TestMethod]
        public void TestInstructionAndImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"AND #$F0 ; AND accumulator with value $F0)");
            Assert.IsTrue(Read(0x1000) == 0x29, "AND/IMM instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0xF0, "Imm $F0 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xF0, "Value $F0 expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionRolAcc()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ROL A ;ROL contents of accumulator");
            Assert.IsTrue(Read(0x1000) == 0x2A, "ROL/Acc instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            processor.State.Accumulator = 0x81;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x02, "Value $02 expected in accumulator");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 7)");
        }

        [TestMethod]
        public void TestInstructionBitAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BIT $2000 ; bit test the contents of address $2000");
            Assert.IsTrue(Read(0x1000) == 0x2C, "BIT/Abs instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = true;
            processor.State.NegativeFlag = false;
            processor.State.OverflowFlag = false;
            Write(0x2000, 0xC0); // $C0 = b11000000

            processor.ExecuteInstruction();

            Assert.IsTrue(!processor.State.ZeroFlag, "Zero flag expected to be clear");
            Assert.IsTrue(processor.State.NegativeFlag, "Negative (S) flag expected to be clear");
            Assert.IsTrue(processor.State.OverflowFlag, "Overflow flag expected to be clear");
        }

        [TestMethod]
        public void TestInstructionAndAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"AND $2000 ; AND accumulator with contents of address $2000");
            Assert.IsTrue(Read(0x1000) == 0x2D, "AND/Abs instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            Write(0x2000, 0x0F);
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x0F, "Value $0F expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionRolAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ROL $2000 ; ROL contents of address $2000");
            Assert.IsTrue(Read(0x1000) == 0x2E, "ROL/Abs instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            Write(0x2000, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(Read(0x2000) == 0x02, "Value $02 expected at address $2000");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 7)");
        }

        [TestMethod]
        public void TestInstructionBmi()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDA #$01 ;load accumulator with positive value
                  BMI  $30 ;branch on minus to $30 bytes ahead of instruction
                  LDA #$FF ;load accumulator with negative value
                  BMI  $40 ;branch on minus to $40 bytes ahead of instruction");
            Assert.IsTrue(Read(0x1000) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x01, "immediate operand $01 expected");
            Assert.IsTrue(Read(0x1002) == 0x30, "BMI instruction not assembled");
            Assert.IsTrue(Read(0x1003) == 0x30, "Relative branch offset $30 expected");
            Assert.IsTrue(Read(0x1004) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(Read(0x1005) == 0xFF, "immediate operand $FF expected");
            Assert.IsTrue(Read(0x1006) == 0x30, "BMI instruction not assembled");
            Assert.IsTrue(Read(0x1007) == 0x40, "Relative branch offset $40 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstructions(4);
            Assert.IsTrue(processor.State.ProgramCounter == 0x1048, "Value $1048 expected in PC");
        }

        [TestMethod]
        public void TestInstructionAndIzy()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"AND ($10),Y ; AND accumulator with contents of zero page offset $10 (absolute $0010), offset by contents of Y register ($30)");
            Assert.IsTrue(Read(0x1000) == 0x31, "AND/IZY instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "Indexed indirect 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.Write16(0x0010, 0x2000); // ($10) = ($0010) = $2000
            Write(0x2030, 0x0F); // ($10),Y = $2000 + $30 = $2030
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x0F, "Value $0F expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionAndZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"AND $10,X ; AND accumulator with contents of address $0010 + X");
            Assert.IsTrue(Read(0x1000) == 0x35, "AND/ZPX instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZPX operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            Write(0x0040, 0x0F);
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x0F, "Value $0F expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionRolZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ROL $10,X ; ROL contents of address $0010 offset by X");
            Assert.IsTrue(Read(0x1000) == 0x36, "ROL/ZPX instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZPX operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x20;
            processor.State.CarryFlag = false;
            Write(0x0030, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(Read(0x0030) == 0x02, "Value $02 expected at address $0030 = ($10,X), X = $20");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 7)");
        }

        [TestMethod]
        public void TestInstructionSec()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SEC ;set carry flag");
            Assert.IsTrue(Read(0x1000) == 0x38, "SEC instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set");
        }

        [TestMethod]
        public void TestInstructionAndAby()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"AND $2000,Y ; AND accumulator with contents of address $2000 + Y");
            Assert.IsTrue(Read(0x1000) == 0x39, "AND/ABY instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "ABY operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            Write(0x2030, 0x0F);
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x0F, "Value $0F expected at address $2030");
        }

        [TestMethod]
        public void TestInstructionAndAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"AND $2000,X ; AND accumulator with contents of address $2000 + X");
            Assert.IsTrue(Read(0x1000) == 0x3D, "AND/ABX instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "ABX operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            Write(0x2030, 0x0F);
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x0F, "Value $0F expected at address $2030");
        }

        [TestMethod]
        public void TestInstructionRolAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ROL $2000,X ; ROL contents of address $2000 offset by X");
            Assert.IsTrue(Read(0x1000) == 0x3E, "ROL/ABX instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "ABX operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            processor.State.CarryFlag = false;
            Write(0x2030, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(Read(0x2030) == 0x02, "Value $02 expected at address $2030 = $2000,X where X = $30");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 7)");
        }

        [TestMethod]
        public void TestInstructionRti()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"RTI ;return from interrup handler");
            Assert.IsTrue(Read(0x1000) == 0x40, "RTI instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.Push16(0x2000); // dummy PC value prior to break
            processor.Push(0x00); // dummy processor state
            processor.State.BreakCommandFlag = true; // break flag set as within interrupt handler

            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x2000, "PC not restored to $2000");
            Assert.IsTrue(!processor.State.BreakCommandFlag, "break command flag not cleared");
        }

        [TestMethod]
        public void TestInstructionEorIzx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"EOR ($10,X) ; EOR accumulator with contents of address $2000 contained at address [$90, $91] computed by $10 offset from X ($80)");
            Assert.IsTrue(Read(0x1000) == 0x41, "EOR/IZX instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "Izx $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.Write16(0x0090, 0x2000);
            Write(0x2000, 0xFF);
            processor.State.Accumulator = 0xF0;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x0F, "Value $0F expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionEorZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"EOR $10 ; EOR accumulator with contents of address $0010");
            Assert.IsTrue(Read(0x1000) == 0x45, "EOR/ZP instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            Write(0x0010, 0x0F);
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xF0, "Value $F0 expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionLsrZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LSR $10 ; LSR contents of address $0010");
            Assert.IsTrue(Read(0x1000) == 0x46, "LSR/ZP instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            Write(0x0010, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(Read(0x0010) == 0x40, "Value $40 expected at address 0x0010");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionPha()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"PHA      ;push A to stack");
            Assert.IsTrue(Read(0x1000) == 0x48, "PHA instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;

            processor.ExecuteInstruction();
            Assert.IsTrue(processor.Pull() == 0x01, "Top of stack expected to contain $01");
        }

        [TestMethod]
        public void TestInstructionEorImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"EOR #$FF; EOR value $FF with accumulator");
            Assert.IsTrue(Read(0x1000) == 0x49, "EOR/IMM instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0xFF, "Immediate value $FF not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0xF0;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x0F, "Value $0F expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionLsrAcc()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LSR A ;LSR contents of A");
            Assert.IsTrue(Read(0x1000) == 0x4A, "LSR/Acc instruction not assembled");
         
            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            processor.State.Accumulator = 0x81;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x40, "Value $40 expected in A");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionJmpAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"JMP $2000 ;jump to address $2000");
            Assert.IsTrue(Read(0x1000) == 0x4C, "JMP/Abs instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.ProgramCounter == 0x2000, "PC expected to point to $2000");
        }

        [TestMethod]
        public void TestInstructionEorAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"EOR $2000; EOR contents of address $2000 with accumulator");
            Assert.IsTrue(Read(0x1000) == 0x4D, "EOR/Abs instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0xF0;
            Write(0x2000, 0xFF);

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x0F, "Value $0F expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionLsrAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LSR $2000 ; LSR contents of address $2000");
            Assert.IsTrue(Read(0x1000) == 0x4E, "LSR/Abs instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            Write(0x2000, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(Read(0x2000) == 0x40, "Value $40 expected at address $2000");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionBvc()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BVC $40 ; branch if overflow clear to relative offset $40 ($1042)");
            Assert.IsTrue(Read(0x1000) == 0x50, "BVC instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x40, "Relative operand $40 expected");

            // execution test (overflow clear)
            processor.State.ProgramCounter = 0x1000;
            processor.State.OverflowFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1042, "PC expected to point to address $1042");

            // execution test (overflow set)
            processor.State.ProgramCounter = 0x1000;
            processor.State.OverflowFlag = true;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1002, "PC expected to point to address $1002");
        }

        [TestMethod]
        public void TestInstructionEorIzy()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"EOR ($10),Y ; EOR accumulator with contents of zero page offset $10 (absolute $0010), offset by contents of Y register ($30)");
            Assert.IsTrue(Read(0x1000) == 0x51, "EOR/IZY instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "Indexed indirect $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.Write16(0x0010, 0x2000); // ($10) = ($0010) = $2000
            Write(0x2030, 0x0F); // ($10),Y = $2000 + $30 = $2030
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xF0, "Value $F0 expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionEorZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"EOR $10,X ; EOR accumulator with contents of address $0010 + X");
            Assert.IsTrue(Read(0x1000) == 0x55, "EOR/ZPX instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZPX operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            Write(0x0040, 0x0F);
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xF0, "Value $F0 expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionLsrZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LSR $10,X ;LSR contents of address $0010 + X");
            Assert.IsTrue(Read(0x1000) == 0x56, "LSR/ZPX instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            Write(0x0040, 0x81);
            processor.State.CarryFlag = false;

            processor.ExecuteInstruction();

            Assert.IsTrue(Read(0x0040) == 0x40, "Value $40 expected at address 0x0040");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionCli()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CLI ;clear interrup disable flag");
            Assert.IsTrue(Read(0x1000) == 0x58, "CLI instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.InterruptDisableFlag = true;

            processor.ExecuteInstruction();

            Assert.IsTrue(!processor.State.InterruptDisableFlag, "Interrup disable flag expected to be clear");
        }

        [TestMethod]
        public void TestInstructionEorAby()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"EOR $2000,Y ; EOR accumulator with contents of address $2000 + Y");
            Assert.IsTrue(Read(0x1000) == 0x59, "EOR/ABY instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "ABY operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            Write(0x2030, 0x0F);
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xF0, "Value $F0 expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionEorAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"EOR $2000,X ; EOR accumulator with contents of address $2000 + X");
            Assert.IsTrue(Read(0x1000) == 0x5D, "EOR/ABX instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "ABX operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            Write(0x2030, 0x0F);
            processor.State.Accumulator = 0xFF;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0xF0, "Value $F0 expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionLsrAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LSR $2000,X ;LSR contents of address $2000 + X");
            Assert.IsTrue(Read(0x1000) == 0x5E, "LSR/ABX instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "ABX operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            Write(0x2030, 0x81);
            processor.State.CarryFlag = false;

            processor.ExecuteInstruction();

            Assert.IsTrue(Read(0x2030) == 0x40, "Value $40 expected at address 0x0040");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionRts()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"RTS ;return from subroutine");
            Assert.IsTrue(Read(0x1000) == 0x60, "RTS instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.Push16(0x2000 - 0x1); // dummy PC value prior to JSR

            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x2000, "PC not restored to $2000");
        }

        [TestMethod]
        public void TestInstructionAdcIzx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ADC ($10,X) ;ADC to the accumulator the contents of address $2000 contained at address [$90, $91] computed by $10 offset from X ($80)
                  ADC #$80    ;ADC another $80 to cause carry
                  ADC #$01    ;ADC another $01 for no carry");
            Assert.IsTrue(Read(0x1000) == 0x61, "ADC/IZX instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "Indexed indirect 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.Write16(0x0090, 0x2000);
            Write(0x2000, 0x01);
            processor.State.Accumulator = 0x7F;

            RunAdcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionAdcZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ADC $10  ;ADC to the accumulator the contents of address $0010
                  ADC #$80 ;ADC another $80 to cause carry
                  ADC #$01 ;ADC another $01 for no carry");
            Assert.IsTrue(Read(0x1000) == 0x65, "ADC/ZP instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.Write16(0x0010, 0x01);
            processor.State.Accumulator = 0x7F;

            RunAdcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionRorZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ROR $10 ; ROR contents of address $0010");
            Assert.IsTrue(Read(0x1000) == 0x66, "ROR/ZP instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            Write(0x0010, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(Read(0x0010) == 0x40, "Value $40 expected at address 0x0010");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionPla()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"PLA ;pull value from stack into A");
            Assert.IsTrue(Read(0x1000) == 0x68, "PLA instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.Push(0x01);
            processor.State.Accumulator = 0x00;

            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.Accumulator == 0x01, "A expected to contain $01 from stack");
        }

        [TestMethod]
        public void TestInstructionAdcImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ADC #$01 ;ADC to the accumulator the value $01
                  ADC #$80 ;ADC another $80 to cause carry
                  ADC #$01 ;ADC another $01 for no carry");
            Assert.IsTrue(Read(0x1000) == 0x69, "ADC/Imm instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x01, "Imm operand $01 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x7F;

            RunAdcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionRorAcc()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ROR A ;ROR contents of A");
            Assert.IsTrue(Read(0x1000) == 0x6A, "ROR/Acc instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            processor.State.Accumulator = 0x81;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator== 0x40, "Value $40 expected in A");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionJmpInd()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"JMP ($2000) ;jump to address value contained at address $2000");
            Assert.IsTrue(Read(0x1000) == 0x6C, "JMP/Abs instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "Ind operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.Write16(0x2000, 0x3000);

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.ProgramCounter == 0x3000, "PC expected to point to $3000");
        }

        [TestMethod]
        public void TestInstructionAdcAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ADC $2000 ;ADC to the accumulator the contents of address $2000
                  ADC #$80  ;ADC another $80 to cause carry
                  ADC #$01  ;ADC another $01 for no carry");
            Assert.IsTrue(Read(0x1000) == 0x6D, "ADC/Abs instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.Write16(0x2000, 0x01);
            processor.State.Accumulator = 0x7F;

            RunAdcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionRorAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ROR $2000 ; ROR contents of address $2000");
            Assert.IsTrue(Read(0x1000) == 0x6E, "ROR/Abs instruction not assembled");
            Assert.IsTrue(processor.Read16(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            Write(0x2000, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(Read(0x2000) == 0x40, "Value $40 expected at address $2000");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionBvs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BVS $40 ; branch if overflow set to relative offset $40 ($1042)");
            Assert.IsTrue(Read(0x1000) == 0x70, "BVS instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x40, "Relative operand $40 expected");

            // execution test (overflow set)
            processor.State.ProgramCounter = 0x1000;
            processor.State.OverflowFlag = true;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1042, "PC expected to point to address $1042");

            // execution test (overflow clear)
            processor.State.ProgramCounter = 0x1000;
            processor.State.OverflowFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1002, "PC expected to point to address $1002");
        }

        [TestMethod]
        public void TestInstructionAdcIzy()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ADC ($10),Y ;ADC to accumulator, the contents of zero page offset $10 (absolute $0010), offset by contents of Y register ($30)
                  ADC #$80    ;ADC another $80 to cause carry
                  ADC #$01    ;ADC another $01 for no carry");
            Assert.IsTrue(Read(0x1000) == 0x71, "ADC/IZY instruction not assembled");
            Assert.IsTrue(Read(0x1001) == 0x10, "Indexed indirect 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.Write16(0x0010, 0x2000); // ($10) = ($0010) = $2000
            Write(0x2030, 0x01); // ($10),Y = $2000 + $30 = $2030
            processor.State.Accumulator = 0x7F;

            RunAdcExecutionTest();
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

        [TestMethod]
        public void TestRawPerformance()
        {
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"Start:
                    LDA #$01  ;load positive value in accumulator
                    BPL Start ;branch to start if positive");
            processor.State.ProgramCounter = 0x1000;

            double testDuration = 1.0;
            ulong cycles = 0;
            DateTime dateTimeStart = DateTime.Now;
            while ((DateTime.Now - dateTimeStart).TotalSeconds < testDuration)
            {
                cycles += processor.ExecuteInstruction();
            }
            ulong cyclesPerSecond = (ulong)(cycles / testDuration);
            Console.WriteLine("Cycles per second: " + cyclesPerSecond);

            Assert.IsTrue(cyclesPerSecond > Processor.Frequency, "Processor running t0o slowly");
        }

        private void ResetSystem()
        {
            WipeMemory();
            processor.Reset();
        }

        // common execution test for all ADC variants
        private void RunAdcExecutionTest()
        {
            // add $01 to $7F to trigger sign overflow
            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x80, "Value $80 expected in Accumulator");
            Assert.IsTrue(processor.State.OverflowFlag, "Overflow flag expected to be set");
            Assert.IsTrue(!processor.State.CarryFlag, "Carry flag expected to be clear");

            // add $80 to $80 to trigger carry
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.OverflowFlag, "Overflow flag expected to be set");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set");

            // add $01 to $00
            processor.ExecuteInstruction();
            Assert.IsTrue(!processor.State.OverflowFlag, "Overflow flag expected to be clear");
            Assert.IsTrue(!processor.State.CarryFlag, "Carry flag expected to be clear");
        }

        private Processor processor;
        private Assembler assembler;
    }
}
