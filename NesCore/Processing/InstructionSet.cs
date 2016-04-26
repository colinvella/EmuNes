using NesCore.Addressing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Processing
{
    public class InstructionSet: IEnumerable
    {
        public InstructionSet(Processor processor)
        {
            Processor = processor;
            Memory = processor.Console.Memory;

            Initialise();
        }

        public Processor Processor { get; private set; }
        public Memory Memory { get; private set; }

        public IEnumerator GetEnumerator()
        {
            return instructions.GetEnumerator();
        }

        public Instruction this[byte opCode]
        {
            get { return instructions[opCode]; }
        }

        private void Initialise()
        {
            // general instruction operations

            // general illegal opcode
            Execute IllegalOpCode = (address, mode) => { };

            // no operation
            Execute NoOperation = (address, mode) => { };

            // ORA - logical inclusive OR
            Execute LogicalInclusiveOr = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator |= Memory.Read(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // ASL - arithmetic shift left
            Execute ArithmeticShiftLeft = (address, mode) =>
            {
                State state = Processor.State;
                if (mode == AddressingMode.Accumulator)
                {
                    // carry if highest bit is 1
                    state.CarryFlag = ((state.Accumulator >> 7) & 1) != 0;
                    // shift left
                    state.Accumulator <<= 1;
                    // update zero and negative flags
                    SetZeroAndNegativeFlags(state.Accumulator);
                }
                else
                {
                    // read value from address
                    byte value = Memory.Read(address);
                    // carry if highest bit is 1
                    state.CarryFlag = ((value >> 7) & 1) != 0;
                    // shift left
                    value <<= 1;
                    // write shifted value back to memory
                    Memory.Write(address, value);
                    // update zero and negative flags
                    SetZeroAndNegativeFlags(value);
                }
            };

            // PHP - push processor status
            Execute PushProcessorStatus = (address, mode) =>
            {
                Processor.Push((byte)(Processor.State.Flags | State.BreakCommandMask));
            };

            // BPL - branch on plus
            Execute BranchOnPlus = (address, mode) =>
            {
                State state = Processor.State;

                // no branching if zero
                if (state.ZeroFlag)
                    return;

                // set program counter to the given address operand
                state.ProgramCounter = address;
                // add extra cycles as necessary
                AddBranchCycles(address, mode);
            };

            // CLC - clear carry
            Execute ClearCarryFlag = (address, mode) =>
            {
                Processor.State.CarryFlag = false;
            };

            // JSR - jump to subroutine
            Execute JumpToSubroutine = (address, mode) =>
            {
                State state = Processor.State;
                Processor.Push16((UInt16)(state.ProgramCounter - 1));
                state.ProgramCounter = address;
            };

            // AND - logical And
            Execute LogicalAnd = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator &= Memory.Read(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // BIT - bit test
            Execute BitTest = (address, mode) =>
            {
                State state = Processor.State;
                byte value = Memory.Read(address);
                state.OverflowFlag = (value & 0x40) != 0; // bit 6
                state.NegativeFlag = (value & 0x80) != 0; // bit 7
                state.ZeroFlag = (value & state.Accumulator) != 0;
            };

            // ROL - rotate left
            Execute RotateLeft = (address, mode) =>
            {
                State state = Processor.State;
                bool carryFlag = state.CarryFlag;

                if (mode == AddressingMode.Accumulator)
                {
                    // work on accumulator value
                    state.CarryFlag = (state.Accumulator & 0x80) != 0;
                    state.Accumulator = (byte)(state.Accumulator << 1);
                    if (carryFlag)
                        state.Accumulator |= 1;
                    SetZeroAndNegativeFlags(state.Accumulator);
                }
                else
                {
                    // work on value retrieved from memory
                    byte value = Memory.Read(address);
                    state.CarryFlag = (value & 0x80) != 0;
                    value = (byte)(value << 1);
                    if (carryFlag)
                        value |= 1;
                    Memory.Write(address, value);
                    SetZeroAndNegativeFlags(value);
                }
            };

            // ROR - rotate right
            Execute RotateRight = (address, mode) =>
            {
                State state = Processor.State;
                bool carryFlag = state.CarryFlag;

                if (mode == AddressingMode.Accumulator)
                {
                    // work on accumulator value
                    state.CarryFlag = (state.Accumulator & 0x01) != 0;
                    state.Accumulator = (byte)(state.Accumulator >> 1);
                    if (carryFlag)
                        state.Accumulator |= 0x80;
                    SetZeroAndNegativeFlags(state.Accumulator);
                }
                else
                {
                    // work on value retrieved from memory
                    byte value = Memory.Read(address);
                    state.CarryFlag = (value & 0x01) != 0;
                    value = (byte)(value >> 1);
                    if (carryFlag)
                        value |= 0x80;
                    Memory.Write(address, value);
                    SetZeroAndNegativeFlags(value);
                }
            };

            // PLP - pull processor status
            Execute PullProcessorStatus = (address, mode) =>
            {
                State state = Processor.State;
                state.Flags = Processor.Pull();
                state.BreakCommandFlag = false; // & 0xEF
                state.UnusedFlag = true; // | 0x20
            };

            // BMI - branch on minus
            Execute BranchOnMinus = (address, mode) =>
            {
                State state = Processor.State;

                // no branching if zero or positive
                if (!state.NegativeFlag)
                    return;

                // set program counter to the given address operand
                state.ProgramCounter = address;

                // add extra cycles as necessary
                AddBranchCycles(address, mode);
            };

            // SEC - set carry flag
            Execute SetCarryFlag = (address, mode) =>
            {
                Processor.State.CarryFlag = true;
            };

            // RTI - Return from interrupt
            Execute ReturnFromInterrupt = (address, mode) =>
            {
                State state = Processor.State;
                state.Flags = Processor.Pull();
                state.BreakCommandFlag = false; // & 0xEF
                state.UnusedFlag = true; // | 0x20

                state.ProgramCounter = Processor.Pull16();
            };

            // EOR - logical exlusive OR
            Execute LogicalExclusiveOr = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator ^= Memory.Read(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // LSR - logical shift right
            Execute LogicalShiftRight = (address, mode) =>
            {
                State state = Processor.State;
                if (mode == AddressingMode.Accumulator)
                {
                    state.CarryFlag = (state.Accumulator & 0x01) != 0;
                    state.Accumulator >>= 1;
                    SetZeroAndNegativeFlags(state.Accumulator);
                }
                else
                {
                    byte value = Memory.Read(address);
                    state.CarryFlag = (value & 0x01) != 0;
                    value >>= 1;
                    Memory.Write(address, value);
                    SetZeroAndNegativeFlags(value);
                }
            };

            // PHA - push accumulator
            Execute PushAccumulator = (address, mode) =>
            {
                Processor.Push(Processor.State.Accumulator);
            };

            // PLA - Pull Accumulator
            Execute PullAccumulator = (address, mode) =>
            {
                Processor.State.Accumulator = Processor.Pull();
            };


            // JMP - jump
            Execute Jump = (address, mode) =>
            {
                Processor.State.ProgramCounter = address;
            };

            // BVC - branch if overflow clear
            Execute BranchIfOverflowClear = (address, mode) =>
            {
                State state = Processor.State;
                if (state.OverflowFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address, mode);
            };

            // CLI - clear interrupt disable flag
            Execute ClearInterruptDisableFlag = (address, mode) =>
            {
                Processor.State.InterruptDisableFlag = false;
            };

            // RTS - return from subroutine
            Execute ReturnFromSubroutine = (address, mode) =>
            {
                State state = Processor.State;
                state.ProgramCounter = Processor.Pull16();
                ++state.ProgramCounter;
            };

            // ADC - add with carry
            Execute AddWithCarry = (address, mode) =>
            {
                State state = Processor.State;
                byte oldAccumulatorValue = state.Accumulator;
                byte operandValue = Memory.Read(address);
                byte carryValue = state.CarryFlag ? (byte)1 : (byte)0;
                state.Accumulator = (byte)(oldAccumulatorValue + operandValue + carryValue);
                SetZeroAndNegativeFlags(state.Accumulator);
                state.CarryFlag = oldAccumulatorValue + operandValue + carryValue > 0xFF;
                state.OverflowFlag = ((oldAccumulatorValue ^ operandValue) & 0x80) == 0
                    && ((oldAccumulatorValue ^ state.Accumulator) & 0x80) != 0;
            };




            // SEI - set interrupt disable flag
            Execute SetInterruptDisableFlag = (address, mode) =>
            {
                Processor.State.InterruptDisableFlag = true;
            };

            // BRK - break (force interrupt)
            Execute Break = (address, mode) =>
            {
                Processor.Push16(Processor.State.ProgramCounter);
                PushProcessorStatus(address, mode);
                SetInterruptDisableFlag(address, mode);
                Processor.State.ProgramCounter = Memory.Read16(Memory.IrqVector);
            };

            // Op Codes

            // 0x00 - 0x0F
            instructions[0x00] = new Instruction("BRK", AddressingMode.Implied, 7, Break);
            instructions[0x01] = new Instruction("ORA", AddressingMode.IndexedIndirect, 6, LogicalInclusiveOr);
            instructions[0x02] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x03] = new Instruction("SLO", AddressingMode.IndexedIndirect, 8, IllegalOpCode);
            instructions[0x04] = new Instruction("NOP", AddressingMode.ZeroPage, 3, IllegalOpCode);
            instructions[0x05] = new Instruction("ORA", AddressingMode.ZeroPage, 3, LogicalInclusiveOr);
            instructions[0x06] = new Instruction("ASL", AddressingMode.ZeroPage, 5, ArithmeticShiftLeft);
            instructions[0x07] = new Instruction("SLO", AddressingMode.ZeroPage, 5, IllegalOpCode);
            instructions[0x08] = new Instruction("PHP", AddressingMode.Implied, 3, PushProcessorStatus);
            instructions[0x09] = new Instruction("ORA", AddressingMode.Immediate, 2, LogicalInclusiveOr);
            instructions[0x0A] = new Instruction("ASL", AddressingMode.Accumulator, 2, ArithmeticShiftLeft);
            instructions[0x0B] = new Instruction("ANC", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0x0C] = new Instruction("NOP", AddressingMode.Absolute, 4, IllegalOpCode);
            instructions[0x0D] = new Instruction("ORA", AddressingMode.Absolute, 4, LogicalInclusiveOr);
            instructions[0x0E] = new Instruction("ASL", AddressingMode.Absolute, 6, ArithmeticShiftLeft);
            instructions[0x0F] = new Instruction("SLO", AddressingMode.Absolute, 6, IllegalOpCode);

            // 0x10 - 0x1F
            instructions[0x10] = new Instruction("BPL", AddressingMode.Relative, 2, BranchOnPlus);
            instructions[0x11] = new Instruction("ORA", AddressingMode.IndirectIndexed, 5, LogicalInclusiveOr);
            instructions[0x12] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x13] = new Instruction("SLO", AddressingMode.IndirectIndexed, 8, IllegalOpCode);
            instructions[0x14] = new Instruction("NOP", AddressingMode.ZeroPageX, 4, IllegalOpCode);
            instructions[0x15] = new Instruction("ORA", AddressingMode.ZeroPageX, 4, LogicalInclusiveOr);
            instructions[0x16] = new Instruction("ASL", AddressingMode.ZeroPageX, 6, ArithmeticShiftLeft);
            instructions[0x17] = new Instruction("SLO", AddressingMode.ZeroPageX, 6, IllegalOpCode);
            instructions[0x18] = new Instruction("CLC", AddressingMode.Implied, 2, ClearCarryFlag);
            instructions[0x19] = new Instruction("ORA", AddressingMode.AbsoluteY, 4, LogicalInclusiveOr);
            instructions[0x1A] = new Instruction("NOP", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x1B] = new Instruction("SLO", AddressingMode.AbsoluteY, 7, IllegalOpCode);
            instructions[0x1C] = new Instruction("NOP", AddressingMode.AbsoluteX, 4, IllegalOpCode);
            instructions[0x1D] = new Instruction("ORA", AddressingMode.AbsoluteX, 4, LogicalInclusiveOr);
            instructions[0x1E] = new Instruction("ASL", AddressingMode.AbsoluteX, 7, ArithmeticShiftLeft);
            instructions[0x1F] = new Instruction("SLO", AddressingMode.AbsoluteX, 7, IllegalOpCode);

            // 0x20 - 0x2F
            instructions[0x20] = new Instruction("JSR", AddressingMode.Absolute, 6, JumpToSubroutine);
            instructions[0x21] = new Instruction("AND", AddressingMode.IndexedIndirect, 6, LogicalAnd);
            instructions[0x22] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x23] = new Instruction("RLA", AddressingMode.IndexedIndirect, 8, IllegalOpCode);
            instructions[0x24] = new Instruction("BIT", AddressingMode.ZeroPage, 3, BitTest);
            instructions[0x25] = new Instruction("AND", AddressingMode.ZeroPage, 3, LogicalAnd);
            instructions[0x26] = new Instruction("ROL", AddressingMode.ZeroPage, 5, RotateLeft);
            instructions[0x27] = new Instruction("RLA", AddressingMode.ZeroPage, 5, IllegalOpCode);
            instructions[0x28] = new Instruction("PLP", AddressingMode.Implied, 4, PullProcessorStatus);
            instructions[0x29] = new Instruction("AND", AddressingMode.Immediate, 2, LogicalAnd);
            instructions[0x2A] = new Instruction("ROL", AddressingMode.Accumulator, 2, RotateLeft);
            instructions[0x2B] = new Instruction("ANC", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0x2C] = new Instruction("BIT", AddressingMode.Absolute, 4, BitTest);
            instructions[0x2D] = new Instruction("AND", AddressingMode.Absolute, 4, LogicalAnd);
            instructions[0x2E] = new Instruction("ROL", AddressingMode.Absolute, 6, RotateLeft);
            instructions[0x2F] = new Instruction("RLA", AddressingMode.Absolute, 6, IllegalOpCode);

            // 0x30 - 0x3F
            instructions[0x30] = new Instruction("BMI", AddressingMode.Relative, 2, BranchOnMinus);
            instructions[0x31] = new Instruction("AND", AddressingMode.IndirectIndexed, 5, LogicalAnd);
            instructions[0x32] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x33] = new Instruction("RLA", AddressingMode.IndirectIndexed, 8, IllegalOpCode);
            instructions[0x34] = new Instruction("NOP", AddressingMode.ZeroPageX, 4, IllegalOpCode);
            instructions[0x35] = new Instruction("AND", AddressingMode.ZeroPageX, 4, LogicalAnd);
            instructions[0x36] = new Instruction("ROL", AddressingMode.ZeroPageX, 6, RotateLeft);
            instructions[0x37] = new Instruction("RLA", AddressingMode.ZeroPageX, 6, IllegalOpCode);
            instructions[0x38] = new Instruction("SEC", AddressingMode.Implied, 2, SetCarryFlag);
            instructions[0x39] = new Instruction("AND", AddressingMode.AbsoluteY, 4, LogicalAnd);
            instructions[0x3A] = new Instruction("NOP", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x3B] = new Instruction("RLA", AddressingMode.AbsoluteY, 7, IllegalOpCode);
            instructions[0x3C] = new Instruction("NOP", AddressingMode.AbsoluteX, 3, IllegalOpCode);
            instructions[0x3D] = new Instruction("AND", AddressingMode.AbsoluteX, 4, LogicalAnd);
            instructions[0x3E] = new Instruction("ROL", AddressingMode.AbsoluteX, 7, RotateLeft);
            instructions[0x3F] = new Instruction("RLA", AddressingMode.AbsoluteX, 7, IllegalOpCode);

            // 0x40 - 0x4F
            instructions[0x40] = new Instruction("RTI", AddressingMode.Implied, 6, ReturnFromInterrupt);
            instructions[0x41] = new Instruction("EOR", AddressingMode.IndexedIndirect, 6, LogicalExclusiveOr);
            instructions[0x42] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x43] = new Instruction("KIL", AddressingMode.IndexedIndirect, 8, IllegalOpCode);
            instructions[0x44] = new Instruction("NOP", AddressingMode.ZeroPage, 3, IllegalOpCode);
            instructions[0x45] = new Instruction("EOR", AddressingMode.ZeroPage, 3, LogicalExclusiveOr);
            instructions[0x46] = new Instruction("LSR", AddressingMode.ZeroPage, 5, LogicalShiftRight);
            instructions[0x47] = new Instruction("SRE", AddressingMode.ZeroPage, 5, IllegalOpCode);
            instructions[0x48] = new Instruction("PHA", AddressingMode.Implied, 3, PushAccumulator);
            instructions[0x49] = new Instruction("EOR", AddressingMode.Immediate, 2, LogicalExclusiveOr);
            instructions[0x4A] = new Instruction("LSR", AddressingMode.Accumulator, 2, LogicalShiftRight);
            instructions[0x4B] = new Instruction("LSR", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0x4C] = new Instruction("JMP", AddressingMode.Absolute, 3, Jump);
            instructions[0x4D] = new Instruction("EOR", AddressingMode.Absolute, 4, LogicalExclusiveOr);
            instructions[0x4E] = new Instruction("LSR", AddressingMode.Absolute, 6, LogicalShiftRight);
            instructions[0x4F] = new Instruction("SRE", AddressingMode.Absolute, 6, IllegalOpCode);

            // 0x50 - 0x5F
            instructions[0x50] = new Instruction("BVC", AddressingMode.Relative, 2, BranchIfOverflowClear);
            instructions[0x51] = new Instruction("EOR", AddressingMode.IndirectIndexed, 5, LogicalExclusiveOr);
            instructions[0x52] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x53] = new Instruction("SRE", AddressingMode.IndirectIndexed, 8, IllegalOpCode);
            instructions[0x54] = new Instruction("NOP", AddressingMode.ZeroPageX, 4, IllegalOpCode);
            instructions[0x55] = new Instruction("EOR", AddressingMode.ZeroPageX, 4, LogicalExclusiveOr);
            instructions[0x56] = new Instruction("LSR", AddressingMode.ZeroPageX, 6, LogicalShiftRight);
            instructions[0x57] = new Instruction("SRE", AddressingMode.ZeroPageX, 6, IllegalOpCode);
            instructions[0x58] = new Instruction("CLI", AddressingMode.Implied, 2, ClearInterruptDisableFlag);
            instructions[0x59] = new Instruction("EOR", AddressingMode.AbsoluteY, 4, LogicalExclusiveOr);
            instructions[0x5A] = new Instruction("NOP", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x5B] = new Instruction("SRE", AddressingMode.AbsoluteY, 7, IllegalOpCode);
            instructions[0x5C] = new Instruction("NOP", AddressingMode.AbsoluteX, 4, IllegalOpCode);
            instructions[0x5D] = new Instruction("EOR", AddressingMode.AbsoluteX, 4, LogicalExclusiveOr);
            instructions[0x5E] = new Instruction("LSR", AddressingMode.AbsoluteX, 7, LogicalShiftRight);
            instructions[0x5F] = new Instruction("SRE", AddressingMode.AbsoluteX, 7, IllegalOpCode);

            // 0x60 - 0x6F
            instructions[0x60] = new Instruction("RTS", AddressingMode.Implied, 6, ReturnFromSubroutine);
            instructions[0x61] = new Instruction("ADC", AddressingMode.IndexedIndirect, 6, AddWithCarry);
            instructions[0x62] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x63] = new Instruction("RRA", AddressingMode.IndexedIndirect, 8, IllegalOpCode);
            instructions[0x64] = new Instruction("NOP", AddressingMode.ZeroPage, 3, IllegalOpCode);
            instructions[0x65] = new Instruction("ADC", AddressingMode.ZeroPage, 3, AddWithCarry);
            instructions[0x66] = new Instruction("ROR", AddressingMode.ZeroPage, 5, RotateRight);
            instructions[0x67] = new Instruction("RRA", AddressingMode.ZeroPage, 5, IllegalOpCode);
            instructions[0x68] = new Instruction("PLA", AddressingMode.Implied, 4, PullAccumulator);
            instructions[0x69] = new Instruction("ADC", AddressingMode.Immediate, 2, AddWithCarry);
            instructions[0x6A] = new Instruction("ROR", AddressingMode.Accumulator, 2, RotateRight);
            instructions[0x6B] = new Instruction("ARR", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0x6C] = new Instruction("JMP", AddressingMode.Indirect, 5, Jump);
            instructions[0x6D] = new Instruction("ADC", AddressingMode.Absolute, 4, AddWithCarry);
            instructions[0x6E] = new Instruction("ROR", AddressingMode.Absolute, 6, RotateRight);
            instructions[0x6F] = new Instruction("RRA", AddressingMode.Absolute, 6, IllegalOpCode);
            
            // 0x70 - 0x7F

            instructions[0x72] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);


            instructions[0x78] = new Instruction("SEI", AddressingMode.Implied, 2, SetInterruptDisableFlag);

            // 0x80 - 0x8F

            // 0x90 - 0x9F

            instructions[0x92] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);


            // 0xA0 - 0xAF
            
            // 0xB0 - 0xBF

            instructions[0xB2] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);


            // 0xC0 - 0xCF


            // 0xD0 - 0xDF

            instructions[0xD2] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);


            // 0xE0 - 0xEF

            instructions[0xEA] = new Instruction("NOP", AddressingMode.Implied, 2, NoOperation);


            // 0xF0 - 0xFF

            instructions[0xF2] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);

        }

        // private helper functions

        // sets zero and negative flags for the given byte value
        private void SetZeroAndNegativeFlags(byte value)
        {
            State state = Processor.State;
            state.ZeroFlag = value != 0;
            state.NegativeFlag = (value & 0x80) != 0;
        }

        // adds additional branch cycles
        private void AddBranchCycles(UInt16 address, AddressingMode mode)
        {
            State state = Processor.State;
            // at least one cycle required for branching
            state.Cycles++;

            // one more cycle is needed when crossing pages
            if (Memory.PagesDiffer(state.ProgramCounter, address))
                state.Cycles++;
        }

        // instruction opcode map
        private Instruction[] instructions = new Instruction[256];

    }
}
