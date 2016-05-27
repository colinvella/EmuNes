using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NesCore.Processor;
using NesCore.Utility;

namespace NesCoreTest
{
    [TestClass]
    public class ProcessorTest
    {
        public ProcessorTest()
        {
            processor = new Mos6502();
            processor.ReadByte = ReadByte;
            processor.WriteByte = WriteByte;
            assembler = new Assembler(processor);
        }

        [TestMethod]
        public void TestStack()
        {
            ResetSystem();
            processor.PushByte(0x69);
            Assert.IsTrue(processor.State.StackPointer == 0xFC, "SP did not update correctly (8bit)");
            Assert.IsTrue(processor.PullByte() == 0x69, "push and pull error (8bit)");

            processor.PushWord(0x1234);
            Assert.IsTrue(processor.State.StackPointer == 0xFB, "SP did not update correctly (16bit)");
            Assert.IsTrue(processor.PullWord() == 0x1234, "push and pull error (16bit)");
        }

        [TestMethod]
        public void TestInstructionBrk()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BRK");
            Assert.IsTrue(ReadByte(0x1000) == 0x00, "BRK instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.WriteWord(Mos6502.IrqVector, 0x2030);
            processor.State.InterruptDisableFlag = false;
            byte statusFlags = processor.State.Flags;

            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x2030, "PC not set");
            Assert.IsTrue(processor.State.InterruptDisableFlag, "Interrupt Disable flag not set");
            Assert.IsTrue(processor.PullByte() == (byte)(statusFlags | 0x10), "status flags not preserved");
            Assert.IsTrue(processor.PullWord() == 0x1001, "PC not pushed on stack prior to BRK");
        }

        [TestMethod]
        public void TestInstructionOraIzx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ORA ($10,X) ; OR accumulator with contents of address $2000 contained at address [$90, $91] computed by $10 offset from X ($80)");
            Assert.IsTrue(ReadByte(0x1000) == 0x01, "ORA/IZX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Indexed indirect $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.WriteWord(0x0090, 0x2000);
            WriteByte(0x2000, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x05, "ORA/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            WriteByte(0x0010, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x06, "ASL/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            WriteByte(0x0010, 0x01);
 
            // execute first 7 ASL
            processor.ExecuteInstructions(7);

            Assert.IsTrue(ReadByte(0x0010) == 0x80, "Value $80 expected at location $0010");
            Assert.IsTrue(!processor.State.CarryFlag, "Carry should not be set");

            // execute last ASL
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x0010) == 0x00, "Value $00 expected at location $0010");
            Assert.IsTrue(processor.State.CarryFlag, "Carry should be set");
        }

        [TestMethod]
        public void TestInstructionPhp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"PHP");
            Assert.IsTrue(ReadByte(0x1000) == 0x08, "PHP instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            byte statusFlags = processor.State.Flags;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.PullByte() == (byte)(statusFlags | 0x10), "status flags not preserved");
        }

        [TestMethod]
        public void TestInstructionOraImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ORA #$0F; OR value $0F with accumulator");
            Assert.IsTrue(ReadByte(0x1000) == 0x09, "ORA/IMM instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x0F, "Immediate value $0F not written");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x0A, "ASL/Acc instruction not assembled");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x0D, "ORA/ABS instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Operand should be $2000");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0xF0;
            processor.WriteWord(0x2000, 0x0F);

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
            Assert.IsTrue(ReadByte(0x1000) == 0x0E, "ASL/Abs instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            WriteByte(0x2000, 0x04);
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x2000) == 0x08, "Value $08 expected at address $2000");
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
            Assert.IsTrue(ReadByte(0x1000) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0xFF, "immediate operand $FF expected");
            Assert.IsTrue(ReadByte(0x1002) == 0x10, "BPL instruction not assembled");
            Assert.IsTrue(ReadByte(0x1003) == 0x30, "Relative branch offset $30 expected");
            Assert.IsTrue(ReadByte(0x1004) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1005) == 0x01, "immediate operand $01 expected");
            Assert.IsTrue(ReadByte(0x1006) == 0x10, "BPL instruction not assembled");
            Assert.IsTrue(ReadByte(0x1007) == 0x40, "Relative branch offset $40 expected");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x11, "ORA/IZY instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Indexed indirect $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.WriteWord(0x0010, 0x2000); // ($10) = ($0010) = $2000
            WriteByte(0x2030, 0x0F); // ($10),Y = $2000 + $30 = $2030
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
            Assert.IsTrue(ReadByte(0x1000) == 0x15, "ORA/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x0040, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x16, "ASL/Zpx instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x10;
            WriteByte(0x0030, 0x04);
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x0030) == 0x08, "Value $08 expected at address $0030");
        }

        [TestMethod]
        public void TestInstructionClc()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CLC ;clear carry flag");
            Assert.IsTrue(ReadByte(0x1000) == 0x18, "CLC instruction not assembled");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x19, "ORA/ABY instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABY operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            WriteByte(0x2030, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x1D, "ORA/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x2030, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x1E, "ASL/Abx instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x2030, 0x04);
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x2030) == 0x08, "Value $08 expected at address $2030");
        }

        [TestMethod]
        public void TestInstructionJsrAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"JSR $2000 ;jump to subroutine at absolute $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0x20, "JSR/abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABS operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            WriteByte(0x0030, 0x04);
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x2000, "PC expected to be $2000");
            Assert.IsTrue(processor.PullWord() == 0x1002, "Top of stack expected to hold address of instruction after JSR - 1");
        }

        [TestMethod]
        public void TestInstructionAndIzx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"AND ($10,X) ; AND accumulator with contents of address $2000 contained at address [$90, $91] computed by $10 offset from X ($80)");
            Assert.IsTrue(ReadByte(0x1000) == 0x21, "AND/IZX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Izx 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.WriteWord(0x0090, 0x2000);
            WriteByte(0x2000, 0xF0);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x24, "BIT/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = true;
            processor.State.NegativeFlag = false;
            processor.State.OverflowFlag = false;
            processor.State.Accumulator = 0xFF;
            WriteByte(0x0010, 0xC0); // $C0 = b11000000

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
            Assert.IsTrue(ReadByte(0x1000) == 0x25, "AND/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            WriteByte(0x0010, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x26, "ROL/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            WriteByte(0x0010, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0010) == 0x02, "Value $02 expected at address 0x0010");
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
            Assert.IsTrue(ReadByte(0x1000) == 0x08, "PHP instruction not assembled");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x29, "AND/IMM instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0xF0, "Imm $F0 not written");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x2A, "ROL/Acc instruction not assembled");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x2C, "BIT/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = true;
            processor.State.NegativeFlag = false;
            processor.State.OverflowFlag = false;
            processor.State.Accumulator = 0xFF;
            WriteByte(0x2000, 0xC0); // $C0 = b11000000

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
            Assert.IsTrue(ReadByte(0x1000) == 0x2D, "AND/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            WriteByte(0x2000, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x2E, "ROL/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            WriteByte(0x2000, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2000) == 0x02, "Value $02 expected at address $2000");
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
            Assert.IsTrue(ReadByte(0x1000) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x01, "immediate operand $01 expected");
            Assert.IsTrue(ReadByte(0x1002) == 0x30, "BMI instruction not assembled");
            Assert.IsTrue(ReadByte(0x1003) == 0x30, "Relative branch offset $30 expected");
            Assert.IsTrue(ReadByte(0x1004) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1005) == 0xFF, "immediate operand $FF expected");
            Assert.IsTrue(ReadByte(0x1006) == 0x30, "BMI instruction not assembled");
            Assert.IsTrue(ReadByte(0x1007) == 0x40, "Relative branch offset $40 expected");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x31, "AND/IZY instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Indexed indirect 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.WriteWord(0x0010, 0x2000); // ($10) = ($0010) = $2000
            WriteByte(0x2030, 0x0F); // ($10),Y = $2000 + $30 = $2030
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
            Assert.IsTrue(ReadByte(0x1000) == 0x35, "AND/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x0040, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x36, "ROL/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x20;
            processor.State.CarryFlag = false;
            WriteByte(0x0030, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0030) == 0x02, "Value $02 expected at address $0030 = ($10,X), X = $20");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 7)");
        }

        [TestMethod]
        public void TestInstructionSec()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SEC ;set carry flag");
            Assert.IsTrue(ReadByte(0x1000) == 0x38, "SEC instruction not assembled");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x39, "AND/ABY instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABY operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            WriteByte(0x2030, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x3D, "AND/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x2030, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x3E, "ROL/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            processor.State.CarryFlag = false;
            WriteByte(0x2030, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2030) == 0x02, "Value $02 expected at address $2030 = $2000,X where X = $30");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 7)");
        }

        [TestMethod]
        public void TestInstructionRti()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"RTI ;return from interrup handler");
            Assert.IsTrue(ReadByte(0x1000) == 0x40, "RTI instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.PushWord(0x2000); // dummy PC value prior to break
            processor.PushByte(0x00); // dummy processor state
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
            Assert.IsTrue(ReadByte(0x1000) == 0x41, "EOR/IZX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Izx $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.WriteWord(0x0090, 0x2000);
            WriteByte(0x2000, 0xFF);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x45, "EOR/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            WriteByte(0x0010, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x46, "LSR/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            WriteByte(0x0010, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0010) == 0x40, "Value $40 expected at address 0x0010");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionPha()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"PHA      ;push A to stack");
            Assert.IsTrue(ReadByte(0x1000) == 0x48, "PHA instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;

            processor.ExecuteInstruction();
            Assert.IsTrue(processor.PullByte() == 0x01, "Top of stack expected to contain $01");
        }

        [TestMethod]
        public void TestInstructionEorImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"EOR #$FF; EOR value $FF with accumulator");
            Assert.IsTrue(ReadByte(0x1000) == 0x49, "EOR/IMM instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0xFF, "Immediate value $FF not written");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x4A, "LSR/Acc instruction not assembled");
         
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
            Assert.IsTrue(ReadByte(0x1000) == 0x4C, "JMP/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x4D, "EOR/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0xF0;
            WriteByte(0x2000, 0xFF);

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
            Assert.IsTrue(ReadByte(0x1000) == 0x4E, "LSR/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            WriteByte(0x2000, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2000) == 0x40, "Value $40 expected at address $2000");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionBvc()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BVC $40 ; branch if overflow clear to relative offset $40 ($1042)");
            Assert.IsTrue(ReadByte(0x1000) == 0x50, "BVC instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x40, "Relative operand $40 expected");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x51, "EOR/IZY instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Indexed indirect $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.WriteWord(0x0010, 0x2000); // ($10) = ($0010) = $2000
            WriteByte(0x2030, 0x0F); // ($10),Y = $2000 + $30 = $2030
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
            Assert.IsTrue(ReadByte(0x1000) == 0x55, "EOR/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x0040, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x56, "LSR/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x0040, 0x81);
            processor.State.CarryFlag = false;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0040) == 0x40, "Value $40 expected at address 0x0040");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionCli()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CLI ;clear interrup disable flag");
            Assert.IsTrue(ReadByte(0x1000) == 0x58, "CLI instruction not assembled");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x59, "EOR/ABY instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABY operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            WriteByte(0x2030, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x5D, "EOR/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x2030, 0x0F);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x5E, "LSR/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x2030, 0x81);
            processor.State.CarryFlag = false;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2030) == 0x40, "Value $40 expected at address 0x0040");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionRts()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"RTS ;return from subroutine");
            Assert.IsTrue(ReadByte(0x1000) == 0x60, "RTS instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.PushWord(0x2000 - 0x1); // dummy PC value prior to JSR

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
            Assert.IsTrue(ReadByte(0x1000) == 0x61, "ADC/IZX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Indexed indirect 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.WriteWord(0x0090, 0x2000);
            WriteByte(0x2000, 0x01);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x65, "ADC/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.WriteWord(0x0010, 0x01);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x66, "ROR/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            WriteByte(0x0010, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0010) == 0x40, "Value $40 expected at address 0x0010");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionPla()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"PLA ;pull value from stack into A");
            Assert.IsTrue(ReadByte(0x1000) == 0x68, "PLA instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.PushByte(0x01);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x69, "ADC/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x01, "Imm operand $01 not written");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x6A, "ROR/Acc instruction not assembled");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x6C, "JMP/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Ind operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.WriteWord(0x2000, 0x3000);

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
            Assert.IsTrue(ReadByte(0x1000) == 0x6D, "ADC/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.WriteWord(0x2000, 0x01);
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
            Assert.IsTrue(ReadByte(0x1000) == 0x6E, "ROR/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            WriteByte(0x2000, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2000) == 0x40, "Value $40 expected at address $2000");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionBvs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BVS $40 ; branch if overflow set to relative offset $40 ($1042)");
            Assert.IsTrue(ReadByte(0x1000) == 0x70, "BVS instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x40, "Relative operand $40 expected");

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
            Assert.IsTrue(ReadByte(0x1000) == 0x71, "ADC/IZY instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Indexed indirect 0x10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.WriteWord(0x0010, 0x2000); // ($10) = ($0010) = $2000
            WriteByte(0x2030, 0x01); // ($10),Y = $2000 + $30 = $2030
            processor.State.Accumulator = 0x7F;

            RunAdcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionAdcZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ADC $10,X ;ADC to the accumulator the contents of address $0010 + X
                  ADC #$80  ;ADC another $80 to cause carry
                  ADC #$01  ;ADC another $01 for no carry");
            Assert.IsTrue(ReadByte(0x1000) == 0x75, "ADC/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            processor.WriteWord(0x0040, 0x01);
            processor.State.Accumulator = 0x7F;

            RunAdcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionRorZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ROR $10,X ; ROR contents of address $0010 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0x76, "ROR/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            processor.State.CarryFlag = false;
            WriteByte(0x0040, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0040) == 0x40, "Value $40 expected at address 0x0040");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionSei()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SEI ;set interrup disable flag");
            Assert.IsTrue(ReadByte(0x1000) == 0x78, "SEI instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.InterruptDisableFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.InterruptDisableFlag, "Interrupt disable flag expected to be set");
        }

        [TestMethod]
        public void TestInstructionAdcAby()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ADC $2000,Y ;ADC to the accumulator the contents of address $2000 + Y
                  ADC #$80    ;ADC another $80 to cause carry
                  ADC #$01    ;ADC another $01 for no carry");
            Assert.IsTrue(ReadByte(0x1000) == 0x79, "ADC/Aby instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABY operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            processor.WriteWord(0x2030, 0x01);
            processor.State.Accumulator = 0x7F;

            RunAdcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionAdcAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ADC $2000,X ;ADC to the accumulator the contents of address $2000 + X
                  ADC #$80    ;ADC another $80 to cause carry
                  ADC #$01    ;ADC another $01 for no carry");
            Assert.IsTrue(ReadByte(0x1000) == 0x7D, "ADC/Abx instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            processor.WriteWord(0x2030, 0x01);
            processor.State.Accumulator = 0x7F;

            RunAdcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionRorAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"ROR $2000,X ; ROR contents of address $2000 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0x7E, "ROR/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            processor.State.CarryFlag = false;
            WriteByte(0x2030, 0x81);

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2030) == 0x40, "Value $40 expected at address 0x2030");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set (shifted from bit 0)");
        }

        [TestMethod]
        public void TestInstructionStaIzx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STA ($10,X) ; Store accumulator value to address $2000 contained at address [$90, $91] computed by $10 offset from X ($80)");
            Assert.IsTrue(ReadByte(0x1000) == 0x81, "STA/IZX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Indexed indirect $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.WriteWord(0x0090, 0x2000);
            WriteByte(0x2000, 0x00);
            processor.State.Accumulator = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.ReadWord(0x2000) == 0x01, "Value $01 expected at address $2000");
        }

        [TestMethod]
        public void TestInstructionStyZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STY $10 ;store contents of register Y in address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0x84, "STY/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0010) == 0x01, "Value $01 expected in address $0010");
        }

        [TestMethod]
        public void TestInstructionStaZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STA $10 ;store contents of A in address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0x85, "STA instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0010) == 0x01, "Value $01 expected in address $0010");
        }

        [TestMethod]
        public void TestInstructionStxZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STX $10 ;store contents of register X in address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0x86, "STX/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0010) == 0x01, "Value $01 expected in address $0010");
        }
        
        [TestMethod]
        public void TestInstructionDey()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"DEY ;decrement register Y");
            Assert.IsTrue(ReadByte(0x1000) == 0x88, "DEY instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x10;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterY == 0x0F, "Value $0F expected in register Y");
        }

        [TestMethod]
        public void TestInstructionTxa()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"TXA ;transfer the contents of X to A");
            Assert.IsTrue(ReadByte(0x1000) == 0x8A, "TXA instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x00;
            processor.State.RegisterX = 0x10;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x10, "Value $10 expected in A");
        }

        [TestMethod]
        public void TestInstructionStyAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STY $2000 ;store value in register Y to address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0x8C, "STY/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2000) == 0x01, "Value $01 expected in address $2000");
        }

        [TestMethod]
        public void TestInstructionStaAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STA $2000 ;store value in A to address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0x8D, "STA/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2000) == 0x01, "Value $01 expected in address $2000");
        }

        [TestMethod]
        public void TestInstructionStxAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STX $2000 ;store value in register X to address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0x8E, "STX/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2000) == 0x01, "Value $01 expected in address $2000");
        }

        [TestMethod]
        public void TestInstructionBcc()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BCC $40 ; branch if carry clear to relative offset $40 ($1042)");
            Assert.IsTrue(ReadByte(0x1000) == 0x90, "BCC instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x40, "Relative operand $40 expected");

            // execution test (carry clear)
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1042, "PC expected to point to address $1042");

            // execution test (carry set)
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = true;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1002, "PC expected to point to address $1002");
        }

        [TestMethod]
        public void TestInstructionStaIzy()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STA ($10),Y ; Store value of accumulator to zero page offset $10 (absolute $0010), offset by contents of Y register ($30)");
            Assert.IsTrue(ReadByte(0x1000) == 0x91, "STA/IZY instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Indexed indirect $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.WriteWord(0x0010, 0x2000); // ($10) = ($0010) = $2000
            processor.State.Accumulator = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2030) == 0x01, "Value $01 expected at address $2030");
        }

        [TestMethod]
        public void TestInstructionStyZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STY $10,X ;store contents of register Y in address $0010 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0x94, "STY/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            processor.State.RegisterY = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0040) == 0x01, "Value $01 expected in address $0040");
        }

        [TestMethod]
        public void TestInstructionStaZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STA $10,X ;store contents of A in address $0010 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0x95, "STA/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            processor.State.Accumulator = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0040) == 0x01, "Value $01 expected in address $0040");
        }

        [TestMethod]
        public void TestInstructionStxZpy()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STX $10,Y ;store contents of register X in address $0010 + Y");
            Assert.IsTrue(ReadByte(0x1000) == 0x96, "STX/ZPY instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPY operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            processor.State.RegisterX = 0x01;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x0040) == 0x01, "Value $01 expected in address $0040");
        }

        [TestMethod]
        public void TestInstructionTya()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"TYA ;transfer the contents of Y to A");
            Assert.IsTrue(ReadByte(0x1000) == 0x98, "TYA instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x00;
            processor.State.RegisterY = 0x10;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x10, "Value $10 expected in A");
        }

        [TestMethod]
        public void TestInstructionStaAby()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STA $2000,Y ;store value in A to address $2000 + Y");
            Assert.IsTrue(ReadByte(0x1000) == 0x99, "STA/Aby instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Aby operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;
            processor.State.RegisterY = 0x30;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2030) == 0x01, "Value $01 expected in address $2000");
        }

        [TestMethod]
        public void TestInstructionTxs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"TXS ;transfer the contents of X to the stack pointer");
            Assert.IsTrue(ReadByte(0x1000) == 0x9A, "TXS instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.StackPointer = 0x00;
            processor.State.RegisterX = 0x80;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.StackPointer == 0x80, "Value $80 expected in SP");
        }

        [TestMethod]
        public void TestInstructionStaAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"STA $2000,X ;store value in A to address $2000 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0x9D, "STA/Abx instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abx operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;
            processor.State.RegisterX = 0x30;

            processor.ExecuteInstruction();

            Assert.IsTrue(ReadByte(0x2030) == 0x01, "Value $01 expected in address $2000");
        }

        [TestMethod]
        public void TestInstructionLdyImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDY #$01 ;load value $01 in register Y");
            Assert.IsTrue(ReadByte(0x1000) == 0xA0, "LDY/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x01, "Imm operand $01 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterY == 0x01, "Value $01 expected in register Y");
        }

        [TestMethod]
        public void TestInstructionLdaIzx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDA ($10,X) ;load into the accumulator the contents of address $2000 contained at address [$90, $91] computed by $10 offset from X ($80)");
            Assert.IsTrue(ReadByte(0x1000) == 0xA1, "LDA/IZX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Indexed indirect $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.WriteWord(0x0090, 0x2000);
            WriteByte(0x2000, 0x01);
            processor.State.Accumulator = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x01, "Value $01 expected in Accumulator");
        }

        [TestMethod]
        public void TestInstructionLdxImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDX #$01 ;load value $01 in register X");
            Assert.IsTrue(ReadByte(0x1000) == 0xA2, "LDX/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x01, "Imm operand $01 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterX == 0x01, "Value $01 expected in register X");
        }

        [TestMethod]
        public void TestInstructionLdyZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDY $10 ; Load into register Y the contents of address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0xA4, "LDY/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            WriteByte(0x0010, 0x01);
            processor.State.RegisterY = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterY == 0x01, "Value $01 expected in register Y");
        }

        [TestMethod]
        public void TestInstructionLdaZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDA $10 ; Load into accumulator the contents of address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0xA5, "LDA/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            WriteByte(0x0010, 0x01);
            processor.State.Accumulator = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x01, "Value $01 expected in accumulator");
        }

        [TestMethod]
        public void TestInstructionLdxZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDX $10 ; Load into register X the contents of address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0xA6, "LDX/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            WriteByte(0x0010, 0x01);
            processor.State.RegisterX = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterX == 0x01, "Value $01 expected in register X");
        }

        [TestMethod]
        public void TestInstructionTay()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"TAY ;transfer the contents of A to Y");
            Assert.IsTrue(ReadByte(0x1000) == 0xA8, "TAY instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x10;
            processor.State.RegisterY = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterY == 0x10, "Value $10 expected in Y");
        }

        [TestMethod]
        public void TestInstructionLdaImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDA #$01 ;load value $01 in accumulator");
            Assert.IsTrue(ReadByte(0x1000) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x01, "Imm operand $01 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x01, "Value $01 expected in accumulator");
        }

        [TestMethod]
        public void TestInstructionTax()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"TAX ;transfer the contents of A to X");
            Assert.IsTrue(ReadByte(0x1000) == 0xAA, "TAX instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x10;
            processor.State.RegisterX = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterX == 0x10, "Value $10 expected in X");
        }

        [TestMethod]
        public void TestInstructionLdyAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDY $2000; Load into Y the contents at address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0xAC, "LDY/ABS instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x00;
            processor.WriteWord(0x2000, 0x01);

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterY == 0x01, "Value $01 expected in Y");
        }

        [TestMethod]
        public void TestInstructionLdaAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDA $2000; Load into A the contents at address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0xAD, "LDA/ABS instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x00;
            processor.WriteWord(0x2000, 0x01);

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x01, "Value $01 expected in A");
        }

        [TestMethod]
        public void TestInstructionLdxAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDX $2000; Load into X the contents at address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0xAE, "LDX/ABS instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x00;
            processor.WriteWord(0x2000, 0x01);

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterX == 0x01, "Value $01 expected in X");
        }

        [TestMethod]
        public void TestInstructionBcs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BCS $40 ; branch if carry set to relative offset $40 ($1042)");
            Assert.IsTrue(ReadByte(0x1000) == 0xB0, "BCS instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x40, "Relative operand $40 expected");

            // execution test (carry set)
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = true;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1042, "PC expected to point to address $1042");

            // execution test (carry clear)
            processor.State.ProgramCounter = 0x1000;
            processor.State.CarryFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1002, "PC expected to point to address $1002");
        }

        [TestMethod]
        public void TestInstructionLdaIzy()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDA ($10),Y ; Load into accumulator the value at zero page offset $10 (absolute $0010), offset by contents of Y register ($30)");
            Assert.IsTrue(ReadByte(0x1000) == 0xB1, "LDA/IZY instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Indexed indirect $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.WriteWord(0x0010, 0x2000); // ($10) = ($0010) = $2000
            WriteByte(0x2030, 0x01);
            processor.State.Accumulator = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x01, "Value $01 expected in accumulator");
        }

        [TestMethod]
        public void TestInstructionLdyZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDY $10,X ; Load into register Y the contents of address $0010 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0xB4, "LDY/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x0040, 0x01);
            processor.State.RegisterY = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterY == 0x01, "Value $01 expected in register Y");
        }

        [TestMethod]
        public void TestInstructionLdaZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDA $10,X ; Load into accumulator the contents of address $0010 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0xB5, "LDA/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x0040, 0x01);
            processor.State.Accumulator = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x01, "Value $01 expected in accumulator");
        }

        [TestMethod]
        public void TestInstructionLdxZpy()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDX $10,Y ; Load into register X the contents of address $0010 + Y");
            Assert.IsTrue(ReadByte(0x1000) == 0xB6, "LDX/ZPY instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPY operand $10 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            WriteByte(0x0040, 0x01);
            processor.State.RegisterX = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterX == 0x01, "Value $01 expected in register X");
        }

        [TestMethod]
        public void TestInstructionClv()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CLV ;clear overflow flag");
            Assert.IsTrue(ReadByte(0x1000) == 0xB8, "CLV instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.OverflowFlag = true;

            processor.ExecuteInstruction();

            Assert.IsTrue(!processor.State.OverflowFlag, "Overflow flag expected to be clear");
        }

        [TestMethod]
        public void TestInstructionLdaAby()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDA $2000,Y ; Load into accumulator the contents of address $2000 + Y");
            Assert.IsTrue(ReadByte(0x1000) == 0xB9, "LDA/ABY instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABY operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            WriteByte(0x2030, 0x01);
            processor.State.Accumulator = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x01, "Value $01 expected in accumulator");
        }

        [TestMethod]
        public void TestInstructionTsx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"TSX ;transfer the contents of SP to X");
            Assert.IsTrue(ReadByte(0x1000) == 0xBA, "TSX instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.StackPointer = 0x10;
            processor.State.RegisterX = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterX == 0x10, "Value $10 expected in X");
        }

        [TestMethod]
        public void TestInstructionLdyAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDY $2000,X ; Load into register Y the contents of address $2000 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0xBC, "LDY/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x2030, 0x01);
            processor.State.RegisterY = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterY == 0x01, "Value $01 expected in register Y");
        }

        [TestMethod]
        public void TestInstructionLdaAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDA $2000,X ; Load into accumulator the contents of address $2000 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0xBD, "LDA/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x30;
            WriteByte(0x2030, 0x01);
            processor.State.Accumulator = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x01, "Value $01 expected in accumulator");
        }

        [TestMethod]
        public void TestInstructionLdxAby()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"LDX $2000,Y ; Load into register X the contents of address $2000 + Y");
            Assert.IsTrue(ReadByte(0x1000) == 0xBE, "LDX/ABY instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABY operand $2000 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;
            WriteByte(0x2030, 0x01);
            processor.State.RegisterX = 0x00;

            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.RegisterX == 0x01, "Value $01 expected in register X");
        }

        [TestMethod]
        public void TestInstructionCpyImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CPY #$10 ;compare register Y with value $10");
            Assert.IsTrue(ReadByte(0x1000) == 0xC0, "CPY/IMM instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Imm operand $10 expected");

            // execution test (equal)
            processor.State.RegisterY = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.RegisterY = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.RegisterY = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionCmpIzx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CMP ($10,X) ;compare accumulator with contents of address $2000 contained at address [$90, $91] computed by $10 offset from X ($80)");
            Assert.IsTrue(ReadByte(0x1000) == 0xC1, "CMP/IZX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Izx 0x10 not written");

            // execution test
            processor.State.RegisterX = 0x80;
            processor.WriteWord(0x0090, 0x2000);
            WriteByte(0x2000, 0x10);

            // execution test (equal)
            processor.State.Accumulator = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.Accumulator = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.Accumulator = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionCpyZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CPY $10 ;compare register Y with value at address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0xC4, "CPY/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 expected");

            WriteByte(0x0010, 0x10);

            // execution test (equal)
            processor.State.RegisterY = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.RegisterY = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.RegisterY = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionCmpZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CMP $10 ;compare accumulator with value at address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0xC5, "CMP/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 expected");

            WriteByte(0x0010, 0x10);

            // execution test (equal)
            processor.State.Accumulator = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.Accumulator = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.Accumulator = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionDecZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"DEC $10 ;decrement value stored at address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0xC6, "DEC/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 expected");

            WriteByte(0x0010, 0x10);
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x0010) == 0x0F, "Value in $0010 expected to be $0F");
        }

        [TestMethod]
        public void TestInstructionIny()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"INY ;increment register Y");
            Assert.IsTrue(ReadByte(0x1000) == 0xC8, "INY instruction not assembled");

            processor.State.RegisterY = 0x10;
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.RegisterY == 0x11, "$11 expected in register Y");
        }

        [TestMethod]
        public void TestInstructionCmpImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CMP #$10 ;compare accumulator with value $10");
            Assert.IsTrue(ReadByte(0x1000) == 0xC9, "CMP/IMM instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Imm operand $10 expected");

            // execution test (equal)
            processor.State.Accumulator = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.Accumulator = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.Accumulator = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionDex()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"DEX ;decrement register X");
            Assert.IsTrue(ReadByte(0x1000) == 0xCA, "DEX instruction not assembled");

            processor.State.RegisterX = 0x10;
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.RegisterX == 0x0F, "$0F expected in register X");
        }

        [TestMethod]
        public void TestInstructionCpyAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CPY $2000 ;compare register Y with value at address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0xCC, "CPY/ABS instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABS operand $2000 expected");

            WriteByte(0x2000, 0x10);

            // execution test (equal)
            processor.State.RegisterY = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.RegisterY = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.RegisterY = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionCmpAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CMP $2000 ;compare accumulator with value at address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0xCD, "CMP/ABS instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABS operand $2000 expected");

            WriteByte(0x2000, 0x10);

            // execution test (equal)
            processor.State.Accumulator = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.Accumulator = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.Accumulator = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionDecAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"DEC $2000 ;decrement value stored at address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0xCE, "DEC/ABS instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABS operand $2000 expected");

            WriteByte(0x2000, 0x10);
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x2000) == 0x0F, "Value in $2000 expected to be $0F");
        }

        [TestMethod]
        public void TestInstructionBne()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BNE $40 ; branch if not equal to relative offset $40 ($1042)");
            Assert.IsTrue(ReadByte(0x1000) == 0xD0, "BNE instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x40, "Relative operand $40 expected");

            // execution test (not equal: zero flag clear - do branch)
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1042, "PC expected to point to address $1042");

            // execution test (equal: zero flag set - don't branch)
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = true;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1002, "PC expected to point to address $1002");
        }

        [TestMethod]
        public void TestInstructionCmpIzy()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CMP ($10),Y ; Compare accumulator with contents of zero page offset $10 (absolute $0010), offset by contents of Y register ($30)");
            Assert.IsTrue(ReadByte(0x1000) == 0xD1, "CMP/IZY instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "IZY operand $10 not written");

            processor.State.RegisterY = 0x30;  // Y = $30
            processor.WriteWord(0x0010, 0x2000); // ($10) = ($0010) = $2000
            WriteByte(0x2030, 0x10); // ($10),Y = $2000 + $30 = $2030

            // execution test (equal)
            processor.State.Accumulator = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.Accumulator = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.Accumulator = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionCmpZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CMP $10,X ;compare accumulator with value at address $0010 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0xD5, "CMP/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 expected");

            processor.State.RegisterX = 0x30;
            WriteByte(0x0040, 0x10);

            // execution test (equal)
            processor.State.Accumulator = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.Accumulator = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.Accumulator = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionDecZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"DEC $10,X ;decrement value stored at address $0010 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0xD6, "DEC/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 expected");

            processor.State.RegisterX = 0x30;
            WriteByte(0x0040, 0x10);
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x0040) == 0x0F, "Value in $0040 expected to be $0F");
        }

        [TestMethod]
        public void TestInstructionCld()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CLD ;clear decimal mode flag");
            Assert.IsTrue(ReadByte(0x1000) == 0xD8, "CLD instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.DecimalModeFlag = true;

            processor.ExecuteInstruction();

            Assert.IsTrue(!processor.State.DecimalModeFlag, "Decimal mode flag expected to be clear");
        }

        [TestMethod]
        public void TestInstructionCmpAby()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CMP $2000,Y ;compare accumulator with value at address $2000 + Y");
            Assert.IsTrue(ReadByte(0x1000) == 0xD9, "CMP/ABY instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABY operand $2000 expected");

            processor.State.RegisterY = 0x30;
            WriteByte(0x2030, 0x10);

            // execution test (equal)
            processor.State.Accumulator = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.Accumulator = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.Accumulator = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionCmpAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CMP $2000,X ;compare accumulator with value at address $2000 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0xDD, "CMP/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 expected");

            processor.State.RegisterX = 0x30;
            WriteByte(0x2030, 0x10);

            // execution test (equal)
            processor.State.Accumulator = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.Accumulator = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.Accumulator = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionDecAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"DEC $2000,X ;decrement value stored at address $2000 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0xDE, "DEC/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 expected");

            processor.State.RegisterX = 0x30;
            WriteByte(0x2030, 0x10);
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x2030) == 0x0F, "Value in $2030 expected to be $0F");
        }

        [TestMethod]
        public void TestInstructionCpxImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CPX #$10 ;compare register X with value $10");
            Assert.IsTrue(ReadByte(0x1000) == 0xE0, "CPX/IMM instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "Imm operand $10 expected");

            // execution test (equal)
            processor.State.RegisterX = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.RegisterX = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.RegisterX = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionSbcIzx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SBC ($10,X) ;SBC from the accumulator the contents of address $2000 contained at address [$90, $91] computed by $10 offset from X ($80)
                  SBC #$80    ;SBC another $80 to cause carry ($FF - $80 = $7E <-> -1 - (-127) = 126)
                  SBC #$FE    ;SBC another $FE for no carry ($7E - $FE = $80 <-> 126 - (-2) = 128) - overflow");
            Assert.IsTrue(ReadByte(0x1000) == 0xE1, "SBC/IZX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "IZX operand $10 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterX = 0x80;
            processor.WriteWord(0x0090, 0x2000);
            WriteByte(0x2000, 0x01);
            processor.State.Accumulator = 0x01;

            RunSbcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionCpxZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CPX $10 ;compare register X with value at address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0xE4, "CPX/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 expected");

            WriteByte(0x0010, 0x10);

            // execution test (equal)
            processor.State.RegisterX = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.RegisterX = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.RegisterX = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionSbcZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SBC $10  ;SBC from the accumulator the value at address $0010 (and borrow - Carry clear)
                  SBC #$80 ;SBC another $80 to cause carry ($FF - $80 = $7E <-> -1 - (-127) = 126)
                  SBC #$FE ;SBC another $FE for no carry ($7E - $FE = $80 <-> 126 - (-2) = 128) - overflow");
            Assert.IsTrue(ReadByte(0x1000) == 0xE5, "SBC/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;
            processor.WriteWord(0x0010, 0x01);

            RunSbcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionIncZp()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"INC $10 ;Increment value stored at address $0010");
            Assert.IsTrue(ReadByte(0x1000) == 0xE6, "INC/ZP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZP operand $10 expected");

            WriteByte(0x0010, 0x10);
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x0010) == 0x11, "Value in $0010 expected to be $11");
        }

        [TestMethod]
        public void TestInstructionInx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"INX ;increment register X");
            Assert.IsTrue(ReadByte(0x1000) == 0xE8, "INX instruction not assembled");

            processor.State.RegisterX = 0x10;
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.RegisterX == 0x11, "$11 expected in register X");
        }

        [TestMethod]
        public void TestInstructionSbcImm()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SBC #$01 ;SBC from the accumulator the value $01 (and borrow - Carry clear)
                  SBC #$80 ;SBC another $80 to cause carry ($FF - $80 = $7E <-> -1 - (-127) = 126)
                  SBC #$FE ;SBC another $FE for no carry ($7E - $FE = $80 <-> 126 - (-2) = 128) - overflow");
            Assert.IsTrue(ReadByte(0x1000) == 0xE9, "SBC/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x01, "Imm operand $01 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;

            RunSbcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionNop()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"NOP ;no operation");
            Assert.IsTrue(ReadByte(0x1000) == 0xEA, "NOP instruction not assembled");

            byte flags = processor.State.Flags;
            byte accumulator = processor.State.Accumulator;
            byte registerX = processor.State.RegisterX;
            byte registerY = processor.State.RegisterY;

            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.Flags == flags, "Status flags not expected to change");
            Assert.IsTrue(processor.State.Accumulator == accumulator, "Accumulator not expected to change");
            Assert.IsTrue(processor.State.RegisterX == registerX, "Register X not expected to change");
            Assert.IsTrue(processor.State.RegisterY == registerY, "Register Y not expected to change");
        }

        [TestMethod]
        public void TestInstructionCpxAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"CPX $2000 ;compare register X with value at address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0xEC, "CPX/ABS instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABS operand $2000 expected");

            WriteByte(0x2000, 0x10);

            // execution test (equal)
            processor.State.RegisterX = 0x10;
            RunCompareEqualTest();

            // execution test (less)
            processor.State.RegisterX = 0x0F;
            RunCompareLessTest();

            // execution test (greater)
            processor.State.RegisterX = 0x11;
            RunCompareGreaterTest();
        }

        [TestMethod]
        public void TestInstructionSbcAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SBC $2000 ;SBC from the accumulator the value at address $2000 (and borrow - Carry clear)
                  SBC #$80  ;SBC another $80 to cause carry ($FF - $80 = $7E <-> -1 - (-127) = 126)
                  SBC #$FE  ;SBC another $FE for no carry ($7E - $FE = $80 <-> 126 - (-2) = 128) - overflow");
            Assert.IsTrue(ReadByte(0x1000) == 0xED, "SBC/Abs instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abs operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;
            processor.WriteWord(0x2000, 0x01);

            RunSbcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionIncAbs()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"INC $2000 ;increment value stored at address $2000");
            Assert.IsTrue(ReadByte(0x1000) == 0xEE, "INC/ABS instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABS operand $2000 expected");

            WriteByte(0x2000, 0x10);
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x2000) == 0x11, "Value in $2000 expected to be $11");
        }

        [TestMethod]
        public void TestInstructionBeq()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"BEQ $40 ; branch if equal to relative offset $40 ($1042)");
            Assert.IsTrue(ReadByte(0x1000) == 0xF0, "BEQ instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x40, "Relative operand $40 expected");

            // execution test (equal: zero flag set - do branch)
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = true;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1042, "PC expected to point to address $1042");

            // execution test (not equal: zero flag clear - don't branch)
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ProgramCounter == 0x1002, "PC expected to point to address $1002");
        }

        [TestMethod]
        public void TestInstructionSbcIzy()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SBC ($10),Y ;SBC from accumulator, the contents of zero page offset $10 (absolute $0010), offset by contents of Y register ($30)
                  SBC #$80    ;SBC another $80 to cause carry ($FF - $80 = $7E <-> -1 - (-127) = 126)
                  SBC #$FE    ;SBC another $FE for no carry ($7E - $FE = $80 <-> 126 - (-2) = 128) - overflow");
            Assert.IsTrue(ReadByte(0x1000) == 0xF1, "SBC/IZY instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "IZY operand $10 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.RegisterY = 0x30;  // Y = $30
            processor.WriteWord(0x0010, 0x2000); // ($10) = ($0010) = $2000
            WriteByte(0x2030, 0x01); // ($10),Y = $2000 + $30 = $2030
            processor.State.Accumulator = 0x01;

            RunSbcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionSbcZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SBC $10,X ;SBC from the accumulator the value at address $0010+X (and borrow - Carry clear)
                  SBC #$80  ;SBC another $80 to cause carry ($FF - $80 = $7E <-> -1 - (-127) = 126)
                  SBC #$FE  ;SBC another $FE for no carry ($7E - $FE = $80 <-> 126 - (-2) = 128) - overflow");
            Assert.IsTrue(ReadByte(0x1000) == 0xF5, "SBC/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;
            processor.State.RegisterX = 0x30;
            processor.WriteWord(0x0040, 0x01);

            RunSbcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionIncZpx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"INC $10,X ;Increment value stored at address $0010 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0xF6, "INC/ZPX instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x10, "ZPX operand $10 expected");

            processor.State.RegisterX = 0x30;
            WriteByte(0x0040, 0x10);
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x0040) == 0x11, "Value in $0040 expected to be $11");
        }

        [TestMethod]
        public void TestInstructionSed()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SED ;set decimal mode flag");
            Assert.IsTrue(ReadByte(0x1000) == 0xF8, "SED instruction not assembled");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.DecimalModeFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.DecimalModeFlag, "Decimal mode flag expected to be set");
        }

        [TestMethod]
        public void TestInstructionSbcAby()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SBC $2000,Y ;SBC from the accumulator the value at address $2000+Y (and borrow - Carry clear)
                  SBC #$80    ;SBC another $80 to cause carry ($FF - $80 = $7E <-> -1 - (-127) = 126)
                  SBC #$FE    ;SBC another $FE for no carry ($7E - $FE = $80 <-> 126 - (-2) = 128) - overflow");
            Assert.IsTrue(ReadByte(0x1000) == 0xF9, "SBC/Aby instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Aby operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;
            processor.State.RegisterY = 0x30;
            processor.WriteWord(0x2030, 0x01);

            RunSbcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionSbcAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"SBC $2000,X ;SBC from the accumulator the value at address $2000+X (and borrow - Carry clear)
                  SBC #$80    ;SBC another $80 to cause carry ($FF - $80 = $7E <-> -1 - (-127) = 126)
                  SBC #$FE    ;SBC another $FE for no carry ($7E - $FE = $80 <-> 126 - (-2) = 128) - overflow");
            Assert.IsTrue(ReadByte(0x1000) == 0xFD, "SBC/Abx instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "Abx operand $2000 not written");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.State.Accumulator = 0x01;
            processor.State.RegisterX = 0x30;
            processor.WriteWord(0x2030, 0x01);

            RunSbcExecutionTest();
        }

        [TestMethod]
        public void TestInstructionIncAbx()
        {
            // assembler test
            ResetSystem();
            assembler.GenerateProgram(0x1000,
                @"INC $2000,X ;increment value stored at address $2000 + X");
            Assert.IsTrue(ReadByte(0x1000) == 0xFE, "DEC/ABX instruction not assembled");
            Assert.IsTrue(processor.ReadWord(0x1001) == 0x2000, "ABX operand $2000 expected");

            processor.State.RegisterX = 0x30;
            WriteByte(0x2030, 0x10);
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstruction();
            Assert.IsTrue(ReadByte(0x2030) == 0x11, "Value in $2030 expected to be $11");
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
            Assert.IsTrue(ReadByte(0x1000) == 0xA9, "LDA/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1001) == 0x01, "immediate operand $01 expected");
            Assert.IsTrue(ReadByte(0x1002) == 0x10, "BPL instruction not assembled");
            Assert.IsTrue(ReadByte(0x1003) == 0x01, "Computed relative branch offset $01 expected (to skip NOP)");
            Assert.IsTrue(ReadByte(0x1004) == 0xEA, "NOP instruction not assembled");
            Assert.IsTrue(ReadByte(0x1005) == 0xA2, "LDX/Imm instruction not assembled");
            Assert.IsTrue(ReadByte(0x1006) == 0x02, "immediate operand $02 expected");
            Assert.IsTrue(ReadByte(0x1007) == 0x10, "BPL instruction not assembled");
            Assert.IsTrue(ReadByte(0x1008) == 0xF7, "Computed relative byte offset $F7 (-9) expected");

            // execution test
            processor.State.ProgramCounter = 0x1000;
            processor.ExecuteInstructions(4);
            Assert.IsTrue(processor.State.ProgramCounter == 0x1000, "Value $1000 expected in PC (branch back to Start)");
            Assert.IsTrue(processor.State.Accumulator == 0x01, "value $01 expected in A");
            Assert.IsTrue(processor.State.RegisterX == 0x02, "value $02 expected in X");
        }

        [TestMethod]
        public void TestRawProcessorPerformance()
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

            Assert.IsTrue(cyclesPerSecond > Mos6502.Frequency, "Processor running t0o slowly");
        }

        private void ResetSystem()
        {
            WipeMemory();
            processor.Reset();
        }

        private void WipeMemory()
        {
            memory = new byte[ushort.MaxValue + 1];
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
            Assert.IsTrue(processor.State.Accumulator == 0x00, "Value $00 expected in Accumulator");
            Assert.IsTrue(processor.State.OverflowFlag, "Overflow flag expected to be set");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set");

            // add $01 to $00 (carry set)
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.Accumulator == 0x02, "Value $02 expected in Accumulator");
            Assert.IsTrue(!processor.State.OverflowFlag, "Overflow flag expected to be clear");
            Assert.IsTrue(!processor.State.CarryFlag, "Carry flag expected to be clear");
        }

        // common execution test for all SBC variants
        private void RunSbcExecutionTest()
        {
            // subtract $01 from $01
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.Accumulator == 0xFF, "Value $FF expected in Accumulator (carry clear => borrow");
            Assert.IsTrue(!processor.State.OverflowFlag, "Overflow flag expected to be clear");
            Assert.IsTrue(!processor.State.CarryFlag, "Carry flag expected to be clear");

            // subtract $80 from $FF to trigger carry (-1 - (-127) = 126)
            processor.ExecuteInstruction();
            Assert.IsTrue(!processor.State.OverflowFlag, "Overflow flag expected to be clear");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set");

            // subtract $FD from $7E (with Carry - no borrow) to trigger sign overflow (126 - (-2) = 128)
            processor.ExecuteInstruction();

            Assert.IsTrue(processor.State.Accumulator == 0x80, "Value $80 expected in Accumulator");
            Assert.IsTrue(processor.State.OverflowFlag, "Overflow flag expected to be set");
            Assert.IsTrue(!processor.State.CarryFlag, "Carry flag expected to be clear");
        }

        // general comparision test (equal)
        private void RunCompareEqualTest()
        {
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = false;
            processor.State.NegativeFlag = true;
            processor.State.CarryFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(processor.State.ZeroFlag, "Zero flag expected to be set");
            Assert.IsTrue(!processor.State.NegativeFlag, "Negative flag expected to be clear");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set");
        }

        // general comparision test (less)
        private void RunCompareLessTest()
        {
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = true;
            processor.State.NegativeFlag = false;
            processor.State.CarryFlag = true;
            processor.ExecuteInstruction();
            Assert.IsTrue(!processor.State.ZeroFlag, "Zero flag expected to be clear");
            Assert.IsTrue(processor.State.NegativeFlag, "Negative flag expected to be set");
            Assert.IsTrue(!processor.State.CarryFlag, "Carry flag expected to be clear");
        }

        // general comparision test (greater)
        private void RunCompareGreaterTest()
        {
            processor.State.ProgramCounter = 0x1000;
            processor.State.ZeroFlag = true;
            processor.State.NegativeFlag = false;
            processor.State.CarryFlag = false;
            processor.ExecuteInstruction();
            Assert.IsTrue(!processor.State.ZeroFlag, "Zero flag expected to be clear");
            Assert.IsTrue(!processor.State.NegativeFlag, "Negative flag expected to be clear");
            Assert.IsTrue(processor.State.CarryFlag, "Carry flag expected to be set");
        }

        private byte ReadByte(ushort address)
        {
            return memory[address];
        }

        private void WriteByte(ushort address, byte value)
        {
            memory[address] = value;
        }

        private Mos6502 processor;
        private Assembler assembler;

        private byte[] memory;
    }
}
