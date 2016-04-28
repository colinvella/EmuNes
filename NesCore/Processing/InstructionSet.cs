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
            SystemBus = processor.SystemBus;

            Initialise();
        }

        public Processor Processor { get; private set; }

        // some instructions exposed
        public Execute PushProcessorStatus { get; private set; }

        private SystemBus SystemBus { get; set; }

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

            // move instructions

            // TAX - transfer accumulator to x
            Execute TransferAccumulatorToX = (address, mode) =>
            {
                State state = Processor.State;
                state.RegisterX = state.Accumulator;
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // TAY - transfer accumulator to y
            Execute TransferAccumulatorToY = (address, mode) =>
            {
                State state = Processor.State;
                state.RegisterY = state.Accumulator;
                SetZeroAndNegativeFlags(state.RegisterY);
            };

            // TSX - transfer stack pointer to x
            Execute TransferStackPointerToX = (address, mode) =>
            {
                State state = Processor.State;
                state.RegisterX = state.StackPointer;
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // TXA - transfer x to accumulator
            Execute TransferXToAccumulator = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator = state.RegisterX;
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // TYA - transfer y to accumulator
            Execute TransferYToAccumulator = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator = state.RegisterY;
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // TXS - Transfer x to stack pointer
            Execute TransferXToStackPointer = (address, mode) =>
            {
                State state = Processor.State;
                state.StackPointer = state.RegisterX;
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // load instructions

            // LDA - load accumulator
            Execute LoadAccumulator = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator = SystemBus.Read(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // LDX - load register x
            Execute LoadRegisterX = (address, mode) =>
            {
                State state = Processor.State;
                state.RegisterX = SystemBus.Read(address);
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // LDY - load register y
            Execute LoadRegisterY = (address, mode) =>
            {
                State state = Processor.State;
                state.RegisterY = SystemBus.Read(address);
                SetZeroAndNegativeFlags(state.RegisterY);
            };

            // store instructions

            // STA - store accumulator
            Execute StoreAccumulator = (address, mode) =>
            {
                SystemBus.Write(address, Processor.State.Accumulator);
            };

            // STX - store register x
            Execute StoreRegisterX = (address, mode) =>
            {
                SystemBus.Write(address, Processor.State.RegisterX);
            };

            // STY - store register y
            Execute StoreRegisterY = (address, mode) =>
            {
                SystemBus.Write(address, Processor.State.RegisterY);
            };

            // logical instructions 

            // AND - logical And
            Execute LogicalAnd = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator &= SystemBus.Read(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // ORA - logical inclusive OR
            Execute LogicalInclusiveOr = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator |= SystemBus.Read(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // EOR - logical exlusive OR
            Execute LogicalExclusiveOr = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator ^= SystemBus.Read(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // BIT - bit test
            Execute BitTest = (address, mode) =>
            {
                State state = Processor.State;
                byte value = SystemBus.Read(address);
                state.OverflowFlag = (value & 0x40) != 0; // bit 6
                state.NegativeFlag = (value & 0x80) != 0; // bit 7
                state.ZeroFlag = (value & state.Accumulator) != 0;
            };

            // shift instructions

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
                    byte value = SystemBus.Read(address);
                    // carry if highest bit is 1
                    state.CarryFlag = ((value >> 7) & 1) != 0;
                    // shift left
                    value <<= 1;
                    // write shifted value back to memory
                    SystemBus.Write(address, value);
                    // update zero and negative flags
                    SetZeroAndNegativeFlags(value);
                }
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
                    byte value = SystemBus.Read(address);
                    state.CarryFlag = (value & 0x80) != 0;
                    value = (byte)(value << 1);
                    if (carryFlag)
                        value |= 1;
                    SystemBus.Write(address, value);
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
                    byte value = SystemBus.Read(address);
                    state.CarryFlag = (value & 0x01) != 0;
                    value = (byte)(value >> 1);
                    if (carryFlag)
                        value |= 0x80;
                    SystemBus.Write(address, value);
                    SetZeroAndNegativeFlags(value);
                }
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
                    byte value = SystemBus.Read(address);
                    state.CarryFlag = (value & 0x01) != 0;
                    value >>= 1;
                    SystemBus.Write(address, value);
                    SetZeroAndNegativeFlags(value);
                }
            };

            // compare instructions

            // CMP - compare accumulator
            Execute CompareAccumulator = (address, mode) =>
            {
                byte value = SystemBus.Read(address);
                CompareValues(Processor.State.Accumulator, value);
            };

            // CPX - compare x register
            Execute CompareRegisterX = (address, mode) =>
            {
                byte value = SystemBus.Read(address);
                CompareValues(Processor.State.RegisterX, value);
            };

            // CPY - compare y register
            Execute CompareRegisterY = (address, mode) =>
            {
                byte value = SystemBus.Read(address);
                CompareValues(Processor.State.RegisterY, value);
            };

            // branch instructions

            // BEQ - branch if  equal
            Execute BranchIfEqual = (address, mode) =>
            {
                State state = Processor.State;
                if (!state.ZeroFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address, mode);
            };

            // BNE - branch if not equal
            Execute BranchIfNotEqual = (address, mode) =>
            {
                State state = Processor.State;
                if (state.ZeroFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address, mode);
            };

            // BPL - branch if plus
            Execute BranchIfPlus = (address, mode) =>
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

            // BMI - branch if minus
            Execute BranchIfMinus = (address, mode) =>
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

            // BCC - branch if carry clear
            Execute BranchIfCarryClear = (address, mode) =>
            {
                State state = Processor.State;
                if (state.CarryFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address, mode);
            };

            // BCS - branch if carry set
            Execute BranchIfCarrySet = (address, mode) =>
            {
                State state = Processor.State;
                if (!state.CarryFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address, mode);
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

            // BVS - branch if overflow set
            Execute BranchIfOverflowSet = (address, mode) =>
            {
                State state = Processor.State;
                if (!state.OverflowFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address, mode);
            };

            // JSR - jump to subroutine
            Execute JumpToSubroutine = (address, mode) =>
            {
                State state = Processor.State;
                Processor.Push16((UInt16)(state.ProgramCounter - 1));
                state.ProgramCounter = address;
            };

            // RTS - return from subroutine
            Execute ReturnFromSubroutine = (address, mode) =>
            {
                State state = Processor.State;
                state.ProgramCounter = Processor.Pull16();
                ++state.ProgramCounter;
            };

            // stack instructions

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

            // PHP - push processor status
            PushProcessorStatus = (address, mode) =>
            {
                Processor.Push((byte)(Processor.State.Flags | State.BreakCommandMask));
            };

            // PLP - pull processor status
            Execute PullProcessorStatus = (address, mode) =>
            {
                State state = Processor.State;
                state.Flags = Processor.Pull();
                state.BreakCommandFlag = false; // & 0xEF
                state.UnusedFlag = true; // | 0x20
            };

            // clear / set flag instructions

            // SEC - set carry flag
            Execute SetCarryFlag = (address, mode) =>
            {
                Processor.State.CarryFlag = true;
            };

            // CLC - clear carry flag
            Execute ClearCarryFlag = (address, mode) =>
            {
                Processor.State.CarryFlag = false;
            };

            // SED - set decimal mode flag
            Execute SetDecimalModeFlag = (address, mode) =>
            {
                Processor.State.DecimalModeFlag = true;
            };

            // CLD - clear decimal mode flag
            Execute ClearDecimalModeFlag = (address, mode) =>
            {
                Processor.State.DecimalModeFlag = false;
            };

            // CLV - clear overflow flag
            Execute ClearOverflowFlag = (address, mode) =>
            {
                Processor.State.OverflowFlag = false;
            };

            // SEI - set interrupt disable flag
            Execute SetInterruptDisableFlag = (address, mode) =>
            {
                Processor.State.InterruptDisableFlag = true;
            };

            // CLI - clear interrupt disable flag
            Execute ClearInterruptDisableFlag = (address, mode) =>
            {
                Processor.State.InterruptDisableFlag = false;
            };

            // JMP - jump
            Execute Jump = (address, mode) =>
            {
                Processor.State.ProgramCounter = address;
            };

            // arithmetic instructions

            // INX - increment register x
            Execute IncrementRegisterX = (address, mode) =>
            {
                State state = Processor.State;
                ++state.RegisterX;
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // INY - increment register y
            Execute IncrementRegisterY = (address, mode) =>
            {
                State state = Processor.State;
                ++state.RegisterY;
                SetZeroAndNegativeFlags(state.RegisterY);
            };

            // INC - increment memory
            Execute IncrementMemory = (address, mode) =>
            {
                byte value = SystemBus.Read(address);
                ++value;
                SystemBus.Write(address, value);
                SetZeroAndNegativeFlags(value);
            };

            // DEC - decrement memory
            Execute DecrementMemory = (address, mode) =>
            {
                byte value = SystemBus.Read(address);
                --value;
                SystemBus.Write(address, value);
                SetZeroAndNegativeFlags(value);
            };

            // DEX - Decrement X Register
            Execute DecrementRegisterX = (address, mode) =>
            {
                State state = Processor.State;
                --state.RegisterX;
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // DEY - Decrement Y Register
            Execute DecrementRegisterY = (address, mode) =>
            {
                State state = Processor.State;
                --state.RegisterY;
                SetZeroAndNegativeFlags(state.RegisterY);
            };

            // ADC - add with carry
            Execute AddWithCarry = (address, mode) =>
            {
                State state = Processor.State;
                byte oldAccumulatorValue = state.Accumulator;
                byte operandValue = SystemBus.Read(address);
                byte carryValue = state.CarryFlag ? (byte)1 : (byte)0;
                state.Accumulator = (byte)(oldAccumulatorValue + operandValue + carryValue);
                SetZeroAndNegativeFlags(state.Accumulator);
                state.CarryFlag = oldAccumulatorValue + operandValue + carryValue > 0xFF;
                state.OverflowFlag = ((oldAccumulatorValue ^ operandValue) & 0x80) == 0
                    && ((oldAccumulatorValue ^ state.Accumulator) & 0x80) != 0;
            };


            // SBC - subtract with carry
            Execute SubtractWithCarry = (address, mode) =>
            {
                State state = Processor.State;
                byte oldAccumulatorValue = state.Accumulator;
                byte operandValue = SystemBus.Read(address);
                byte carryValue = state.CarryFlag ? (byte)1 : (byte)0;
                int result = oldAccumulatorValue - operandValue - 1 + carryValue;
                state.Accumulator = (byte)result;
                SetZeroAndNegativeFlags(state.Accumulator);
                state.CarryFlag = result >= 0;
                state.OverflowFlag = ((oldAccumulatorValue ^ operandValue) & 0x80) != 0
                    && ((oldAccumulatorValue ^ state.Accumulator) & 0x80) != 0;
            };

            // interrupt instructions

            // BRK - break (force interrupt)
            Execute Break = (address, mode) =>
            {
                Processor.Push16(Processor.State.ProgramCounter);
                PushProcessorStatus(address, mode);
                SetInterruptDisableFlag(address, mode);
                Processor.State.ProgramCounter = Processor.Read16(Processor.IrqVector);
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
            instructions[0x10] = new Instruction("BPL", AddressingMode.Relative, 2, BranchIfPlus);
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
            instructions[0x30] = new Instruction("BMI", AddressingMode.Relative, 2, BranchIfMinus);
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
            instructions[0x70] = new Instruction("BVS", AddressingMode.Relative, 2, BranchIfOverflowClear);
            instructions[0x71] = new Instruction("ADC", AddressingMode.IndirectIndexed, 5, AddWithCarry);
            instructions[0x72] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x73] = new Instruction("RRA", AddressingMode.IndirectIndexed, 8, IllegalOpCode);
            instructions[0x74] = new Instruction("NOP", AddressingMode.ZeroPageX, 4, IllegalOpCode);
            instructions[0x75] = new Instruction("ADC", AddressingMode.ZeroPageX, 4, AddWithCarry);
            instructions[0x76] = new Instruction("ROR", AddressingMode.ZeroPageX, 6, RotateRight);
            instructions[0x77] = new Instruction("RRA", AddressingMode.ZeroPageX, 6, IllegalOpCode);
            instructions[0x78] = new Instruction("SEI", AddressingMode.Implied, 2, SetInterruptDisableFlag);
            instructions[0x79] = new Instruction("ADC", AddressingMode.AbsoluteY, 4, AddWithCarry);
            instructions[0x7A] = new Instruction("NOP", AddressingMode.Absolute, 2, IllegalOpCode);
            instructions[0x7B] = new Instruction("RRA", AddressingMode.AbsoluteY, 7, IllegalOpCode);
            instructions[0x7C] = new Instruction("NOP", AddressingMode.AbsoluteX, 4, IllegalOpCode);
            instructions[0x7D] = new Instruction("ADC", AddressingMode.AbsoluteX, 4, AddWithCarry);
            instructions[0x7E] = new Instruction("ROR", AddressingMode.AbsoluteX, 7, RotateRight);
            instructions[0x7F] = new Instruction("RRA", AddressingMode.AbsoluteX, 7, IllegalOpCode);

            // 0x80 - 0x8F
            instructions[0x80] = new Instruction("NOP", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0x81] = new Instruction("STA", AddressingMode.IndexedIndirect, 6, StoreAccumulator);
            instructions[0x82] = new Instruction("NOP", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0x83] = new Instruction("SAX", AddressingMode.IndexedIndirect, 6, IllegalOpCode);
            instructions[0x84] = new Instruction("STY", AddressingMode.ZeroPage, 3, StoreRegisterY);
            instructions[0x85] = new Instruction("STA", AddressingMode.ZeroPage, 3, StoreAccumulator);
            instructions[0x86] = new Instruction("STX", AddressingMode.ZeroPage, 3, StoreRegisterX);
            instructions[0x87] = new Instruction("SAX", AddressingMode.ZeroPage, 3, IllegalOpCode);
            instructions[0x88] = new Instruction("DEY", AddressingMode.Implied, 2, DecrementRegisterY);
            instructions[0x89] = new Instruction("NOP", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0x8A] = new Instruction("TXA", AddressingMode.Implied, 2, TransferXToAccumulator);
            instructions[0x8B] = new Instruction("XAA", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0x8C] = new Instruction("STY", AddressingMode.Absolute, 4, StoreRegisterY);
            instructions[0x8D] = new Instruction("STA", AddressingMode.Absolute, 5, StoreAccumulator);
            instructions[0x8E] = new Instruction("STX", AddressingMode.Absolute, 4, StoreRegisterX);
            instructions[0x8F] = new Instruction("SAX", AddressingMode.Absolute, 4, IllegalOpCode);

            // 0x90 - 0x9F
            instructions[0x90] = new Instruction("BCC", AddressingMode.Relative, 2, BranchIfCarryClear);
            instructions[0x91] = new Instruction("STA", AddressingMode.IndirectIndexed, 6, StoreAccumulator);
            instructions[0x92] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0x93] = new Instruction("AHX", AddressingMode.IndirectIndexed, 6, IllegalOpCode);
            instructions[0x94] = new Instruction("STY", AddressingMode.ZeroPageX, 4, StoreRegisterY);
            instructions[0x95] = new Instruction("STA", AddressingMode.ZeroPageX, 4, StoreAccumulator);
            instructions[0x96] = new Instruction("STX", AddressingMode.ZeroPageY, 4, StoreRegisterX);
            instructions[0x97] = new Instruction("SAX", AddressingMode.ZeroPageY, 4, IllegalOpCode);
            instructions[0x98] = new Instruction("TYA", AddressingMode.Implied, 2, TransferYToAccumulator);
            instructions[0x99] = new Instruction("STA", AddressingMode.AbsoluteY, 5, StoreAccumulator);
            instructions[0x9A] = new Instruction("TXS", AddressingMode.Implied, 2, TransferXToStackPointer);
            instructions[0x9B] = new Instruction("TAS", AddressingMode.AbsoluteY, 5, IllegalOpCode);
            instructions[0x9C] = new Instruction("SHY", AddressingMode.AbsoluteX, 5, IllegalOpCode);
            instructions[0x9D] = new Instruction("STA", AddressingMode.AbsoluteX, 5, StoreAccumulator);
            instructions[0x9E] = new Instruction("SHX", AddressingMode.AbsoluteY, 5, IllegalOpCode);
            instructions[0x9F] = new Instruction("AHX", AddressingMode.AbsoluteY, 5, IllegalOpCode);

            // 0xA0 - 0xAF
            instructions[0xA0] = new Instruction("LDY", AddressingMode.Immediate, 2, LoadRegisterY);
            instructions[0xA1] = new Instruction("LDA", AddressingMode.IndexedIndirect, 6, LoadAccumulator);
            instructions[0xA2] = new Instruction("LDX", AddressingMode.Immediate, 2, LoadRegisterX);
            instructions[0xA3] = new Instruction("LAX", AddressingMode.IndexedIndirect, 6, IllegalOpCode);
            instructions[0xA4] = new Instruction("LDY", AddressingMode.ZeroPage, 3, LoadRegisterY);
            instructions[0xA5] = new Instruction("LDA", AddressingMode.ZeroPage, 3, LoadAccumulator);
            instructions[0xA6] = new Instruction("LDX", AddressingMode.ZeroPage, 3, LoadRegisterX);
            instructions[0xA7] = new Instruction("LAX", AddressingMode.ZeroPage, 3, IllegalOpCode);
            instructions[0xA8] = new Instruction("TAY", AddressingMode.Implied, 2, TransferAccumulatorToY);
            instructions[0xA9] = new Instruction("LDA", AddressingMode.Immediate, 2, LoadAccumulator);
            instructions[0xAA] = new Instruction("TAX", AddressingMode.Implied, 2, TransferAccumulatorToX);
            instructions[0xAB] = new Instruction("LAX", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0xAC] = new Instruction("LDY", AddressingMode.Absolute, 4, LoadRegisterY);
            instructions[0xAD] = new Instruction("LDA", AddressingMode.Absolute, 4, LoadAccumulator);
            instructions[0xAE] = new Instruction("LDX", AddressingMode.Absolute, 4, LoadRegisterX);
            instructions[0xAF] = new Instruction("LAX", AddressingMode.Absolute, 4, IllegalOpCode);

            // 0xB0 - 0xBF
            instructions[0xB0] = new Instruction("BCS", AddressingMode.Relative, 2, BranchIfCarrySet);
            instructions[0xB1] = new Instruction("LDA", AddressingMode.IndirectIndexed, 5, LoadAccumulator);
            instructions[0xB2] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0xB3] = new Instruction("LAX", AddressingMode.IndirectIndexed, 5, IllegalOpCode);
            instructions[0xB4] = new Instruction("LDY", AddressingMode.ZeroPageX, 4, LoadRegisterY);
            instructions[0xB5] = new Instruction("LDA", AddressingMode.ZeroPageX, 4, LoadAccumulator);
            instructions[0xB6] = new Instruction("LDX", AddressingMode.ZeroPageY, 4, LoadRegisterX);
            instructions[0xB7] = new Instruction("LAX", AddressingMode.ZeroPageY, 4, IllegalOpCode);
            instructions[0xB8] = new Instruction("CLV", AddressingMode.Implied, 2, ClearOverflowFlag);
            instructions[0xB9] = new Instruction("LDA", AddressingMode.AbsoluteY, 4, LoadAccumulator);
            instructions[0xBA] = new Instruction("TSX", AddressingMode.Implied, 2, TransferStackPointerToX);
            instructions[0xBB] = new Instruction("LAS", AddressingMode.AbsoluteY, 4, IllegalOpCode);
            instructions[0xBC] = new Instruction("LDY", AddressingMode.AbsoluteX, 4, LoadRegisterY);
            instructions[0xBD] = new Instruction("LDA", AddressingMode.AbsoluteX, 4, LoadAccumulator);
            instructions[0xBE] = new Instruction("LDX", AddressingMode.AbsoluteY, 4, LoadRegisterX);
            instructions[0xBF] = new Instruction("LAX", AddressingMode.AbsoluteY, 4, IllegalOpCode);

            // 0xC0 - 0xCF
            instructions[0xC0] = new Instruction("CPY", AddressingMode.Immediate, 2, CompareRegisterY);
            instructions[0xC1] = new Instruction("CMP", AddressingMode.IndexedIndirect, 6, CompareAccumulator);
            instructions[0xC2] = new Instruction("NOP", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0xC3] = new Instruction("DCP", AddressingMode.IndexedIndirect, 8, IllegalOpCode);
            instructions[0xC4] = new Instruction("CPY", AddressingMode.ZeroPage, 3, CompareRegisterY);
            instructions[0xC5] = new Instruction("CMP", AddressingMode.ZeroPage, 3, CompareAccumulator);
            instructions[0xC6] = new Instruction("DEC", AddressingMode.ZeroPage, 5, DecrementMemory);
            instructions[0xC7] = new Instruction("DCP", AddressingMode.ZeroPage, 5, IllegalOpCode);
            instructions[0xC8] = new Instruction("INY", AddressingMode.Implied, 2, IncrementRegisterY);
            instructions[0xC9] = new Instruction("CMP", AddressingMode.Immediate, 2, CompareAccumulator);
            instructions[0xCA] = new Instruction("DEX", AddressingMode.Implied, 2, DecrementRegisterX);
            instructions[0xCB] = new Instruction("AXS", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0xCC] = new Instruction("CPY", AddressingMode.Absolute, 4, CompareRegisterY);
            instructions[0xCD] = new Instruction("CMP", AddressingMode.Absolute, 4, CompareAccumulator);
            instructions[0xCE] = new Instruction("DEC", AddressingMode.Absolute, 6, DecrementMemory);
            instructions[0xCF] = new Instruction("DCP", AddressingMode.Absolute, 6, IllegalOpCode);

            // 0xD0 - 0xDF
            instructions[0xD0] = new Instruction("BNE", AddressingMode.Relative, 2, BranchIfNotEqual);
            instructions[0xD1] = new Instruction("CMP", AddressingMode.IndirectIndexed, 5, CompareAccumulator);
            instructions[0xD2] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0xD3] = new Instruction("DCP", AddressingMode.IndirectIndexed, 8, IllegalOpCode);
            instructions[0xD4] = new Instruction("NOP", AddressingMode.ZeroPageX, 4, IllegalOpCode);
            instructions[0xD5] = new Instruction("CMP", AddressingMode.ZeroPageX, 4, CompareAccumulator);
            instructions[0xD6] = new Instruction("DEC", AddressingMode.ZeroPageX, 6, DecrementMemory);
            instructions[0xD7] = new Instruction("DCP", AddressingMode.ZeroPageX, 6, IllegalOpCode);
            instructions[0xD8] = new Instruction("CLD", AddressingMode.Implied, 2, ClearDecimalModeFlag);
            instructions[0xD9] = new Instruction("CMP", AddressingMode.AbsoluteY, 4, CompareAccumulator);
            instructions[0xDA] = new Instruction("NOP", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0xDB] = new Instruction("DCP", AddressingMode.AbsoluteY, 7, IllegalOpCode);
            instructions[0xDC] = new Instruction("NOP", AddressingMode.AbsoluteX, 4, IllegalOpCode);
            instructions[0xDD] = new Instruction("CMP", AddressingMode.AbsoluteX, 4, CompareAccumulator);
            instructions[0xDE] = new Instruction("DEC", AddressingMode.AbsoluteX, 7, DecrementMemory);
            instructions[0xDF] = new Instruction("DCP", AddressingMode.AbsoluteX, 7, IllegalOpCode);

            // 0xE0 - 0xEF
            instructions[0xE0] = new Instruction("CPX", AddressingMode.Immediate, 2, CompareRegisterX);
            instructions[0xE1] = new Instruction("SBC", AddressingMode.IndexedIndirect, 6, SubtractWithCarry);
            instructions[0xE2] = new Instruction("NOP", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0xE3] = new Instruction("ISC", AddressingMode.IndexedIndirect, 8, IllegalOpCode);
            instructions[0xE4] = new Instruction("CPX", AddressingMode.ZeroPage, 3, CompareRegisterX);
            instructions[0xE5] = new Instruction("SBC", AddressingMode.ZeroPage, 3, SubtractWithCarry);
            instructions[0xE6] = new Instruction("INC", AddressingMode.ZeroPage, 5, IncrementMemory);
            instructions[0xE7] = new Instruction("ISC", AddressingMode.ZeroPage, 5, IllegalOpCode);
            instructions[0xE8] = new Instruction("INX", AddressingMode.Implied, 2, IncrementRegisterX);
            instructions[0xE9] = new Instruction("SBC", AddressingMode.Immediate, 2, SubtractWithCarry);
            instructions[0xEA] = new Instruction("NOP", AddressingMode.Implied, 2, NoOperation); // legal NOP
            instructions[0xEB] = new Instruction("SBC", AddressingMode.Immediate, 2, IllegalOpCode);
            instructions[0xEC] = new Instruction("CPX", AddressingMode.Absolute, 4, CompareRegisterX);
            instructions[0xED] = new Instruction("SBC", AddressingMode.Absolute, 4, SubtractWithCarry);
            instructions[0xEE] = new Instruction("INC", AddressingMode.Absolute, 6, IncrementMemory);
            instructions[0xEF] = new Instruction("ISC", AddressingMode.Absolute, 6, IllegalOpCode);

            // 0xF0 - 0xFF
            instructions[0xF0] = new Instruction("BEQ", AddressingMode.Relative, 2, BranchIfEqual);
            instructions[0xF1] = new Instruction("SBC", AddressingMode.IndirectIndexed, 5, SubtractWithCarry);
            instructions[0xF2] = new Instruction("KIL", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0xF3] = new Instruction("ISC", AddressingMode.IndirectIndexed, 8, IllegalOpCode);
            instructions[0xF4] = new Instruction("NOP", AddressingMode.ZeroPageX, 4, IllegalOpCode);
            instructions[0xF5] = new Instruction("SBC", AddressingMode.ZeroPageX, 4, SubtractWithCarry);
            instructions[0xF6] = new Instruction("INC", AddressingMode.ZeroPageX, 6, IncrementMemory);
            instructions[0xF7] = new Instruction("ISC", AddressingMode.ZeroPageX, 6, IllegalOpCode);
            instructions[0xF8] = new Instruction("SED", AddressingMode.Implied, 2, SetDecimalModeFlag);
            instructions[0xF9] = new Instruction("SBC", AddressingMode.AbsoluteY, 4, SubtractWithCarry);
            instructions[0xFA] = new Instruction("NOP", AddressingMode.Implied, 2, IllegalOpCode);
            instructions[0xFB] = new Instruction("ISC", AddressingMode.AbsoluteY, 7, IllegalOpCode);
            instructions[0xFC] = new Instruction("NOP", AddressingMode.AbsoluteX, 4, IllegalOpCode);
            instructions[0xFD] = new Instruction("SBC", AddressingMode.AbsoluteX, 4, SubtractWithCarry);
            instructions[0xFE] = new Instruction("INC", AddressingMode.AbsoluteX, 7, IncrementMemory);
            instructions[0xFF] = new Instruction("ISC", AddressingMode.AbsoluteX, 7, IllegalOpCode);
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
            if (Processor.PagesDiffer(state.ProgramCounter, address))
                state.Cycles++;
        }

        // compares values and set the appropriate flags
        private void CompareValues(byte valueOne, byte valueTwo)
        {
            SetZeroAndNegativeFlags((byte)(valueOne - valueTwo));
            Processor.State.CarryFlag = valueOne >= valueTwo;
        }

        // instruction opcode map
        private Instruction[] instructions = new Instruction[256];

    }
}
