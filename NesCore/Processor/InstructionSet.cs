using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Processor
{
    public class InstructionSet: IEnumerable
    {
        public InstructionSet(Mos6502 processor)
        {
            Processor = processor;

            Initialise();
        }

        public Mos6502 Processor { get; private set; }

        // some instructions exposed
        public Execute PushProcessorStatus { get; private set; }

        public Instruction FindBy(string opName, AddressingMode addressingMode)
        {
            opName = opName.ToUpper();
            foreach (Instruction instruction in instructions)
            {
                if (instruction.Name == opName && instruction.AddressingMode == addressingMode)
                    return instruction;
            }
            return null;
        }

        public IEnumerable<Instruction> GetInstructionVariants(string opName)
        {
            if (instructionVariants.ContainsKey(opName))
                return instructionVariants[opName];
            else
                return null;
        }

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
            // addressing mode fetches

            Fetch FetchNone = (ushort operandAddress, out bool pageCrossed) =>
            {
                pageCrossed = false;
                return 0x0000;
            };

            Fetch FetchAbsolute = (ushort operandAddress, out bool pageCrossed) =>
            {
                pageCrossed = false;
                return Processor.ReadWord(operandAddress);
            };

            Fetch FetchAbsoluteX = (ushort operandAddress, out bool pageCrossed) =>
            {
                ushort absoluteAddress = Processor.ReadWord(operandAddress);
                ushort valueAddress = (ushort)(absoluteAddress + Processor.State.RegisterX);
                pageCrossed = Processor.PagesDiffer(absoluteAddress, valueAddress);
                return valueAddress;
            };

            Fetch FetchAbsoluteY = (ushort operandAddress, out bool pageCrossed) =>
            {
                ushort absoluteAddress = Processor.ReadWord(operandAddress);
                ushort valueAddress = (ushort)(absoluteAddress + Processor.State.RegisterY);
                pageCrossed = Processor.PagesDiffer(absoluteAddress, valueAddress);
                return valueAddress;
            };

            Fetch FetchImmediate = (ushort operandAddress, out bool pageCrossed) =>
            {
                pageCrossed = false;
                return operandAddress;
            };

            Fetch FetchIndexedIndirect = (ushort operandAddress, out bool pageCrossed) =>
            {
                pageCrossed = false;
                // indexed indirect is address located at the x register, offset by the byte immediately following the op code
                return Processor.ReadWordWrap((ushort)(byte)(Processor.ReadByte(operandAddress) + Processor.State.RegisterX));
            };


            Fetch FetchIndirect = (ushort operandAddress, out bool pageCrossed) =>
            {
                pageCrossed = false;
                // indirect address is the address located at the absolute address (with 6502 addressing bug)
                return Processor.ReadWordWrap(Processor.ReadWord(operandAddress));
            };

            Fetch FetchIndirectIndexed = (ushort operandAddress, out bool pageCrossed) =>
            {
                // indirect indexed address is the address located at the byte address immediately after the op code, then offset by the Y register
                ushort address = (ushort)(Processor.ReadWordWrap((Processor.ReadByte(operandAddress))) + Processor.State.RegisterY);
                pageCrossed = Processor.PagesDiffer((ushort)(address - Processor.State.RegisterY), address);
                return address;
            };

            Fetch FetchRelative = (ushort operandAddress, out bool pageCrossed) =>
            {
                // address is relative signed byte offset following op code, applied at the end of the instruction
                pageCrossed = false;
                byte offset = Processor.ReadByte(operandAddress);
                ushort valueAddress = (ushort)(operandAddress + 1 + offset);
                if (offset >= 0x80)
                    valueAddress -= 0x100;
                return valueAddress;
            };

            Fetch FetchZeroPage = (ushort operandAddress, out bool pageCrossed) =>
            {
                pageCrossed = false;
                return Processor.ReadByte(operandAddress);
            };

            Fetch FetchZeroPageX = (ushort operandAddress, out bool pageCrossed) =>
            {
                pageCrossed = false;
                return (ushort)(byte)(Processor.ReadByte(operandAddress) + Processor.State.RegisterX);
            };

            Fetch FetchZeroPageY = (ushort operandAddress, out bool pageCrossed) =>
            {
                pageCrossed = false;
                return (ushort)(byte)(Processor.ReadByte(operandAddress) + Processor.State.RegisterY);
            };

            // general instruction operations (documented)

            // instructor that locks up processor
            Execute Lockup = (address) =>
            {
                if (Processor.Lockup != null)
                    Processor.Lockup();
            };

            // no operation
            Execute NoOperation = (address) => { };

            // move instructions

            // TAX - transfer accumulator to x
            Execute TransferAccumulatorToX = (address) =>
            {
                State state = Processor.State;
                state.RegisterX = state.Accumulator;
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // TAY - transfer accumulator to y
            Execute TransferAccumulatorToY = (address) =>
            {
                State state = Processor.State;
                state.RegisterY = state.Accumulator;
                SetZeroAndNegativeFlags(state.RegisterY);
            };

            // TSX - transfer stack pointer to x
            Execute TransferStackPointerToX = (address) =>
            {
                State state = Processor.State;
                state.RegisterX = state.StackPointer;
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // TXA - transfer x to accumulator
            Execute TransferXToAccumulator = (address) =>
            {
                State state = Processor.State;
                state.Accumulator = state.RegisterX;
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // TYA - transfer y to accumulator
            Execute TransferYToAccumulator = (address) =>
            {
                State state = Processor.State;
                state.Accumulator = state.RegisterY;
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // TXS - Transfer x to stack pointer
            Execute TransferXToStackPointer = (address) =>
            {
                State state = Processor.State;
                state.StackPointer = state.RegisterX;
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // load instructions

            // LDA - load accumulator
            Execute LoadAccumulator = (address) =>
            {
                State state = Processor.State;
                state.Accumulator = Processor.ReadByte(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // LDX - load register x
            Execute LoadRegisterX = (address) =>
            {
                State state = Processor.State;
                state.RegisterX = Processor.ReadByte(address);
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // LDY - load register y
            Execute LoadRegisterY = (address) =>
            {
                State state = Processor.State;
                state.RegisterY = Processor.ReadByte(address);
                SetZeroAndNegativeFlags(state.RegisterY);
            };

            // store instructions

            // STA - store accumulator
            Execute StoreAccumulator = (address) =>
            {
                Processor.WriteByte(address, Processor.State.Accumulator);
            };

            // STX - store register x
            Execute StoreRegisterX = (address) =>
            {
                Processor.WriteByte(address, Processor.State.RegisterX);
            };

            // STY - store register y
            Execute StoreRegisterY = (address) =>
            {
                Processor.WriteByte(address, Processor.State.RegisterY);
            };

            // logical instructions 

            // AND - logical And
            Execute LogicalAnd = (address) =>
            {
                State state = Processor.State;
                state.Accumulator &= Processor.ReadByte(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // ORA - logical inclusive OR
            Execute LogicalInclusiveOr = (address) =>
            {
                State state = Processor.State;
                state.Accumulator |= Processor.ReadByte(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // EOR - logical exlusive OR
            Execute LogicalExclusiveOr = (address) =>
            {
                State state = Processor.State;
                state.Accumulator ^= Processor.ReadByte(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // BIT - bit test
            Execute BitTest = (address) =>
            {
                State state = Processor.State;
                byte value = Processor.ReadByte(address);
                state.OverflowFlag = (value & 0x40) != 0; // bit 6
                state.NegativeFlag = (value & 0x80) != 0; // bit 7
                state.ZeroFlag = (value & state.Accumulator) == 0;
            };

            // shift instructions

            // ASL - arithmetic shift left - accumulator version
            Execute ArithmeticShiftLeftAccumulator = (address) =>
            {
                State state = Processor.State;
                // carry if highest bit is 1
                state.CarryFlag = ((state.Accumulator >> 7) & 1) != 0;
                // shift left
                state.Accumulator <<= 1;
                // update zero and negative flags
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // ASL - arithmetic shift left - memory version
            Execute ArithmeticShiftLeftMemory = (address) =>
            {
                State state = Processor.State;
                // read value from address
                byte value = Processor.ReadByte(address);
                // carry if highest bit is 1
                state.CarryFlag = ((value >> 7) & 1) != 0;
                // shift left
                value <<= 1;
                // write shifted value back to memory
                Processor.WriteByte(address, value);
                // update zero and negative flags
                SetZeroAndNegativeFlags(value);
            };

            // ROL - rotate left (shift accumulator left, bit 7 to carry, and carry to bit 0)
            Execute RotateLeftAccumulator = (address) =>
            {
                State state = Processor.State;
                bool carryFlag = state.CarryFlag;
                state.CarryFlag = (state.Accumulator & 0x80) != 0;
                state.Accumulator = (byte)(state.Accumulator << 1);
                if (carryFlag)
                    state.Accumulator |= 1;
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // ROL - rotate left (shift memory value left, bit 7 to carry, and carry to bit 0)
            Execute RotateLeftMemory = (address) =>
            {
                State state = Processor.State;
                bool carryFlag = state.CarryFlag;
                byte value = Processor.ReadByte(address);
                state.CarryFlag = (value & 0x80) != 0;
                value = (byte)(value << 1);
                if (carryFlag)
                    value |= 1;
                Processor.WriteByte(address, value);
                SetZeroAndNegativeFlags(value);
            };

            // ROR - rotate right (shift accumulator right, bit 0 to carry and carry to bit 7)
            Execute RotateRightAccumulator = (address) =>
            {
                State state = Processor.State;
                bool carryFlag = state.CarryFlag;
                state.CarryFlag = (state.Accumulator & 0x01) != 0;
                state.Accumulator = (byte)(state.Accumulator >> 1);
                if (carryFlag)
                    state.Accumulator |= 0x80;
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // ROR - rotate right (shift memory value right, bit 0 to carry and carry to bit 7)
            Execute RotateRightMemory = (address) =>
            {
                State state = Processor.State;
                bool carryFlag = state.CarryFlag;
                byte value = Processor.ReadByte(address);
                state.CarryFlag = (value & 0x01) != 0;
                value = (byte)(value >> 1);
                if (carryFlag)
                    value |= 0x80;
                Processor.WriteByte(address, value);
                SetZeroAndNegativeFlags(value);
          
            };

            // LSR - logical shift right - accumulator version
            Execute LogicalShiftRightAccumulator = (address) =>
            {
                State state = Processor.State;
                state.CarryFlag = (state.Accumulator & 0x01) != 0;
                state.Accumulator >>= 1;
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // LSR - logical shift right - memory version
            Execute LogicalShiftRightMemory = (address) =>
            {
                State state = Processor.State;
                byte value = Processor.ReadByte(address);
                state.CarryFlag = (value & 0x01) != 0;
                value >>= 1;
                Processor.WriteByte(address, value);
                SetZeroAndNegativeFlags(value);
            };

            // compare instructions

            // CMP - compare accumulator
            Execute CompareAccumulator = (address) =>
            {
                byte value = Processor.ReadByte(address);
                CompareValues(Processor.State.Accumulator, value);
            };

            // CPX - compare x register
            Execute CompareRegisterX = (address) =>
            {
                byte value = Processor.ReadByte(address);
                CompareValues(Processor.State.RegisterX, value);
            };

            // CPY - compare y register
            Execute CompareRegisterY = (address) =>
            {
                byte value = Processor.ReadByte(address);
                CompareValues(Processor.State.RegisterY, value);
            };

            // branch instructions

            // BEQ - branch if  equal
            Execute BranchIfEqual = (address) =>
            {
                State state = Processor.State;
                if (!state.ZeroFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address);
            };

            // BNE - branch if not equal
            Execute BranchIfNotEqual = (address) =>
            {
                State state = Processor.State;
                if (state.ZeroFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address);
            };

            // BPL - branch if plus
            Execute BranchIfPlus = (address) =>
            {
                State state = Processor.State;

                // no branching if zero
                if (state.NegativeFlag)
                    return;

                // set program counter to the given address operand
                state.ProgramCounter = address;
                // add extra cycles as necessary
                AddBranchCycles(address);
            };

            // BMI - branch if minus
            Execute BranchIfMinus = (address) =>
            {
                State state = Processor.State;

                // no branching if zero or positive
                if (!state.NegativeFlag)
                    return;

                // set program counter to the given address operand
                state.ProgramCounter = address;

                // add extra cycles as necessary
                AddBranchCycles(address);
            };

            // BCC - branch if carry clear
            Execute BranchIfCarryClear = (address) =>
            {
                State state = Processor.State;
                if (state.CarryFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address);
            };

            // BCS - branch if carry set
            Execute BranchIfCarrySet = (address) =>
            {
                State state = Processor.State;
                if (!state.CarryFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address);
            };

            // BVC - branch if overflow clear
            Execute BranchIfOverflowClear = (address) =>
            {
                State state = Processor.State;
                if (state.OverflowFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address);
            };

            // BVS - branch if overflow set
            Execute BranchIfOverflowSet = (address) =>
            {
                State state = Processor.State;
                if (!state.OverflowFlag)
                    return;
                state.ProgramCounter = address;
                AddBranchCycles(address);
            };

            // JSR - jump to subroutine
            Execute JumpToSubroutine = (address) =>
            {
                State state = Processor.State;
                Processor.PushWord((ushort)(state.ProgramCounter - 1));
                state.ProgramCounter = address;
            };

            // RTS - return from subroutine
            Execute ReturnFromSubroutine = (address) =>
            {
                State state = Processor.State;
                state.ProgramCounter = Processor.PullWord();
                ++state.ProgramCounter;
            };

            // stack instructions

            // PHA - push accumulator
            Execute PushAccumulator = (address) =>
            {
                Processor.PushByte(Processor.State.Accumulator);
            };

            // PLA - Pull Accumulator
            Execute PullAccumulator = (address) =>
            {
                Processor.State.Accumulator = Processor.PullByte();
                SetZeroAndNegativeFlags(Processor.State.Accumulator);
            };

            // PHP - push processor status
            PushProcessorStatus = (address) =>
            {
                Processor.PushByte((byte)(Processor.State.Flags | State.BreakCommandMask));
            };

            // PLP - pull processor status
            Execute PullProcessorStatus = (address) =>
            {
                State state = Processor.State;
                state.Flags = Processor.PullByte();
                state.BreakCommandFlag = false; // & 0xEF
                state.UnusedFlag = true; // | 0x20
            };

            // clear / set flag instructions

            // SEC - set carry flag
            Execute SetCarryFlag = (address) =>
            {
                Processor.State.CarryFlag = true;
            };

            // CLC - clear carry flag
            Execute ClearCarryFlag = (address) =>
            {
                Processor.State.CarryFlag = false;
            };

            // SED - set decimal mode flag
            Execute SetDecimalModeFlag = (address) =>
            {
                Processor.State.DecimalModeFlag = true;
            };

            // CLD - clear decimal mode flag
            Execute ClearDecimalModeFlag = (address) =>
            {
                Processor.State.DecimalModeFlag = false;
            };

            // CLV - clear overflow flag
            Execute ClearOverflowFlag = (address) =>
            {
                Processor.State.OverflowFlag = false;
            };

            // SEI - set interrupt disable flag
            Execute SetInterruptDisableFlag = (address) =>
            {
                Processor.State.InterruptDisableFlag = true;
            };

            // CLI - clear interrupt disable flag
            Execute ClearInterruptDisableFlag = (address) =>
            {
                Processor.State.InterruptDisableFlag = false;
            };

            // JMP - jump
            Execute Jump = (address) =>
            {
                Processor.State.ProgramCounter = address;
            };

            // arithmetic instructions

            // INX - increment register x
            Execute IncrementRegisterX = (address) =>
            {
                State state = Processor.State;
                ++state.RegisterX;
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // INY - increment register y
            Execute IncrementRegisterY = (address) =>
            {
                State state = Processor.State;
                ++state.RegisterY;
                SetZeroAndNegativeFlags(state.RegisterY);
            };

            // INC - increment memory
            Execute IncrementMemory = (address) =>
            {
                byte value = Processor.ReadByte(address);
                ++value;
                Processor.WriteByte(address, value);
                SetZeroAndNegativeFlags(value);
            };

            // DEC - decrement memory
            Execute DecrementMemory = (address) =>
            {
                byte value = Processor.ReadByte(address);
                --value;
                Processor.WriteByte(address, value);
                SetZeroAndNegativeFlags(value);
            };

            // DEX - Decrement X Register
            Execute DecrementRegisterX = (addressode) =>
            {
                State state = Processor.State;
                --state.RegisterX;
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // DEY - Decrement Y Register
            Execute DecrementRegisterY = (address) =>
            {
                State state = Processor.State;
                --state.RegisterY;
                SetZeroAndNegativeFlags(state.RegisterY);
            };

            // ADC - add with carry
            Execute AddWithCarry = (address) =>
            {
                State state = Processor.State;
                byte oldAccumulatorValue = state.Accumulator;
                byte operandValue = Processor.ReadByte(address);
                byte carryValue = state.CarryFlag ? (byte)1 : (byte)0;
                state.Accumulator = (byte)(oldAccumulatorValue + operandValue + carryValue);
                SetZeroAndNegativeFlags(state.Accumulator);
                state.CarryFlag = oldAccumulatorValue + operandValue + carryValue > 0xFF;
                state.OverflowFlag = ((oldAccumulatorValue ^ operandValue) & 0x80) == 0
                    && ((oldAccumulatorValue ^ state.Accumulator) & 0x80) != 0;
            };

            // SBC - subtract with carry
            Execute SubtractWithCarry = (address) =>
            {
                State state = Processor.State;
                byte oldAccumulatorValue = state.Accumulator;
                byte operandValue = Processor.ReadByte(address);
                byte carryValue = state.CarryFlag ? (byte)1 : (byte)0;
                ushort result = (ushort)(oldAccumulatorValue - operandValue - 1 + carryValue);
                state.Accumulator = (byte)result;
                SetZeroAndNegativeFlags(state.Accumulator);
                state.CarryFlag = result < 0x100;
                state.OverflowFlag = ((oldAccumulatorValue ^ operandValue) & 0x80) != 0
                    && ((oldAccumulatorValue ^ state.Accumulator) & 0x80) != 0;
            };

            // interrupt instructions

            // BRK - break (force interrupt)
            Execute Break = (address) =>
            {
                Processor.PushWord(Processor.State.ProgramCounter);
                PushProcessorStatus(address);
                SetInterruptDisableFlag(address);
                Processor.State.ProgramCounter = Processor.ReadWord(Mos6502.IrqVector);
            };

            // RTI - Return from interrupt
            Execute ReturnFromInterrupt = (address) =>
            {
                State state = Processor.State;
                state.Flags = Processor.PullByte();
                state.BreakCommandFlag = false; // & 0xEF
                state.UnusedFlag = true; // | 0x20

                state.ProgramCounter = Processor.PullWord();
            };

            // general instruction operations (undocumented)

            // SAX - AND X and A and store result in memory
            Execute LogicalAndX = (address) =>
            {
                State state = Processor.State;
                byte result = (byte)(state.RegisterX & state.Accumulator);
                Processor.WriteByte(address, result);
            };

            // LAX - load both X and A from memory
            Execute LoadAandX = (address) =>
            {
                State state = Processor.State;
                byte value = Processor.ReadByte(address);
                state.Accumulator = state.RegisterX = value;
                SetZeroAndNegativeFlags(value);
            };

            // SHX - AND X register with high byte of the target address of the argument + 1
            // and store result in memory.
            Execute UndocumentedShx = (address) =>
            {
                State state = Processor.State;
                ushort value = Processor.ReadWord(address);
                byte highByte = (byte)(value >> 8);
                highByte &= state.RegisterX;
                ++highByte;
                Processor.WriteByte(address, highByte);
            };

            // SHY - AND Y register with high byte of the target address of the argument + 1
            // and store result in memory.
            Execute UndocumentedShy = (address) =>
            {
                State state = Processor.State;
                ushort value = Processor.ReadWord(address);
                byte highByte = (byte)(value >> 8);
                highByte &= state.RegisterY;
                ++highByte;
                Processor.WriteByte(address, highByte);
            };

            // ARR - AND and ROR
            Execute AndAndRorateRight = (address) =>
            {
                LogicalAnd(address);
                RotateRightAccumulator(address);
            };

            // DCP - Decrement memory without borrow
            Execute DecrementMemoryWithoutBorrow = (address) =>
            {
                DecrementMemory(address);
                CompareAccumulator(address);
            };

            // ISC - increment memory, then subtract memory from accu-mulator (with borrow)
            Execute IncrementThenSubtract = (address) =>
            {
                IncrementMemory(address);
                SubtractWithCarry(address);
            };

            // SLO - arithmetic shift left then OR
            Execute ArithmeticShiftLeftThenOr = (address) =>
            {
                ArithmeticShiftLeftMemory(address);
                LogicalInclusiveOr(address);
            };

            // SRE - logical shift right then XOR
            Execute LogicalShiftRightThenXor = (address) =>
            {
                LogicalShiftRightMemory(address);
                LogicalExclusiveOr(address);
            };

            // RLA - Rotate left then And
            Execute RotateLeftThenAnd = (address) =>
            {
                RotateLeftMemory(address);
                LogicalAnd(address);
            };

            // RRA - Rotate right then Add
            Execute RotateRightThenAdd = (address) =>
            {
                RotateRightMemory(address);
                AddWithCarry(address);
            };

            // ANC - and with carry
            Execute AndWithCarry = (address) =>
            {
                State state = Processor.State;
                state.Accumulator &= Processor.ReadByte(address);
                SetZeroAndNegativeFlags(state.Accumulator);
                state.CarryFlag = (state.Accumulator & 0x80) != 0;
            };

            // ALR - AND then shift right
            Execute AndThenShiftRight = (address) =>
            {
                State state = Processor.State;
                state.Accumulator &= Processor.ReadByte(address);
                LogicalShiftRightMemory(address);
            };

            // XAA - undocumented A = (A | #$EE) & X & #byte
            // very vague documentation found
            Execute UndocumentedXaa = (address) =>
            {
                State state = Processor.State;
                state.Accumulator |= 0xEE;
                state.Accumulator &= state.RegisterX;
                state.Accumulator &= Processor.ReadByte(address);
            };

            // AXH - And X and A and 0x07 and store result
            Execute UndocumentedAxa = (address) =>
            {
                State state = Processor.State;
                byte result = state.RegisterX;
                result &= state.Accumulator;
                result &= 0x07;
                Processor.WriteByte(address, result);
            };

            // TAS - AND X with A, store result in SP, AND SP with the high byte of word at address + 1. store result
            // note: probably incorrectly implemented - but this opcode is practically unused
            Execute UndocumentedTas = (address) =>
            {
                State state = Processor.State;
                state.StackPointer = state.RegisterX;
                state.StackPointer &= state.Accumulator;

                ushort value = Processor.ReadWord((ushort)(address + 1));
                byte result = state.StackPointer;
                result &= (byte)(value >> 8); ;
                Processor.WriteByte(address, result);
            };

            // ATX - AND byte with A, then transfer A to X
            Execute LogicalAndThenTransferToX = (address) =>
            {
                LogicalAnd(address);
                TransferAccumulatorToX(address);
            };

            // LAS - AND memory with SP, store in SP, X, A
            Execute UndocumentedLas = (address) =>
            {
                State state = Processor.State;
                byte value = Processor.ReadByte(address);
                state.StackPointer &= value;
                state.RegisterX = state.Accumulator = state.StackPointer;
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // AXS - AND X with A, store in X, subtract byte from X (without borrow)
            // probably not correctly implemented - but not used
            Execute UndocumentedAxs = (address) =>
            {
                State state = Processor.State;
                state.RegisterX &= state.Accumulator;
                byte value = Processor.ReadByte(address);
                state.RegisterX -= value;
                SetZeroAndNegativeFlags(state.RegisterX);
            };

            // DOP - double NOP
            Execute DoubleNop = (address) => { };

            // TOP - tripple NOP
            Execute TrippleNop = (address) => { };

            // Op Codes

            // 0x00 - 0x0F
            instructions[0x00] = new Instruction(0x00, "BRK", AddressingMode.Implied, 7, FetchNone, Break);
            instructions[0x01] = new Instruction(0x01, "ORA", AddressingMode.IndexedIndirect, 6, FetchIndexedIndirect,LogicalInclusiveOr);
            instructions[0x02] = new Instruction(0x02, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0x03] = new Instruction(0x03, "SLO", AddressingMode.IndexedIndirect, 8, FetchIndexedIndirect, ArithmeticShiftLeftThenOr);
            instructions[0x04] = new Instruction(0x04, "DOP", AddressingMode.ZeroPage, 3, FetchZeroPage, DoubleNop);
            instructions[0x05] = new Instruction(0x05, "ORA", AddressingMode.ZeroPage, 3, FetchZeroPage, LogicalInclusiveOr);
            instructions[0x06] = new Instruction(0x06, "ASL", AddressingMode.ZeroPage, 5, FetchZeroPage, ArithmeticShiftLeftMemory);
            instructions[0x07] = new Instruction(0x07, "SLO", AddressingMode.ZeroPage, 5, FetchZeroPage, ArithmeticShiftLeftThenOr);
            instructions[0x08] = new Instruction(0x08, "PHP", AddressingMode.Implied, 3, FetchNone, PushProcessorStatus);
            instructions[0x09] = new Instruction(0x09, "ORA", AddressingMode.Immediate, 2, FetchImmediate, LogicalInclusiveOr);
            instructions[0x0A] = new Instruction(0x0A, "ASL", AddressingMode.Accumulator, 2, FetchNone, ArithmeticShiftLeftAccumulator);
            instructions[0x0B] = new Instruction(0x0B, "ANC", AddressingMode.Immediate, 2, FetchImmediate, AndWithCarry);
            instructions[0x0C] = new Instruction(0x0C, "TOP", AddressingMode.Absolute, 4, FetchAbsolute, TrippleNop);
            instructions[0x0D] = new Instruction(0x0D, "ORA", AddressingMode.Absolute, 4, FetchAbsolute, LogicalInclusiveOr);
            instructions[0x0E] = new Instruction(0x0E, "ASL", AddressingMode.Absolute, 6, FetchAbsolute, ArithmeticShiftLeftMemory);
            instructions[0x0F] = new Instruction(0x0F, "SLO", AddressingMode.Absolute, 6, FetchAbsolute, ArithmeticShiftLeftThenOr);

            // 0x10 - 0x1F
            instructions[0x10] = new Instruction(0x10, "BPL", AddressingMode.Relative, 2, FetchRelative, BranchIfPlus);
            instructions[0x11] = new Instruction(0x11, "ORA", AddressingMode.IndirectIndexed, 5, FetchIndirectIndexed, LogicalInclusiveOr);
            instructions[0x12] = new Instruction(0x12, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0x13] = new Instruction(0x13, "SLO", AddressingMode.IndirectIndexed, 8, FetchIndirectIndexed, ArithmeticShiftLeftThenOr);
            instructions[0x14] = new Instruction(0x14, "DOP", AddressingMode.ZeroPageX, 4, FetchZeroPageX, DoubleNop);
            instructions[0x15] = new Instruction(0x15, "ORA", AddressingMode.ZeroPageX, 4, FetchZeroPageX, LogicalInclusiveOr);
            instructions[0x16] = new Instruction(0x16, "ASL", AddressingMode.ZeroPageX, 6, FetchZeroPageX, ArithmeticShiftLeftMemory);
            instructions[0x17] = new Instruction(0x17, "SLO", AddressingMode.ZeroPageX, 6, FetchZeroPageX, ArithmeticShiftLeftThenOr);
            instructions[0x18] = new Instruction(0x18, "CLC", AddressingMode.Implied, 2, FetchNone, ClearCarryFlag);
            instructions[0x19] = new Instruction(0x19, "ORA", AddressingMode.AbsoluteY, 4, FetchAbsoluteY, LogicalInclusiveOr);
            instructions[0x1A] = new Instruction(0x1A, "NOPu", AddressingMode.Implied, 2, FetchNone, NoOperation);
            instructions[0x1B] = new Instruction(0x1B, "SLO", AddressingMode.AbsoluteY, 7, FetchAbsoluteY, ArithmeticShiftLeftThenOr);
            instructions[0x1C] = new Instruction(0x1C, "TOP", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, TrippleNop);
            instructions[0x1D] = new Instruction(0x1D, "ORA", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, LogicalInclusiveOr);
            instructions[0x1E] = new Instruction(0x1E, "ASL", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, ArithmeticShiftLeftMemory);
            instructions[0x1F] = new Instruction(0x1F, "SLO", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, ArithmeticShiftLeftThenOr);

            // 0x20 - 0x2F
            instructions[0x20] = new Instruction(0x20, "JSR", AddressingMode.Absolute, 6, FetchAbsolute, JumpToSubroutine);
            instructions[0x21] = new Instruction(0x21, "AND", AddressingMode.IndexedIndirect, 6, FetchIndexedIndirect, LogicalAnd);
            instructions[0x22] = new Instruction(0x22, "KIL", AddressingMode.Implied, 2, FetchNone,  Lockup);
            instructions[0x23] = new Instruction(0x23, "RLA", AddressingMode.IndexedIndirect, 8, FetchIndexedIndirect, RotateLeftThenAnd);
            instructions[0x24] = new Instruction(0x24, "BIT", AddressingMode.ZeroPage, 3, FetchZeroPage, BitTest);
            instructions[0x25] = new Instruction(0x25, "AND", AddressingMode.ZeroPage, 3, FetchZeroPage, LogicalAnd);
            instructions[0x26] = new Instruction(0x26, "ROL", AddressingMode.ZeroPage, 5, FetchZeroPage, RotateLeftMemory);
            instructions[0x27] = new Instruction(0x27, "RLA", AddressingMode.ZeroPage, 5, FetchZeroPage, RotateLeftThenAnd);
            instructions[0x28] = new Instruction(0x28, "PLP", AddressingMode.Implied, 4, FetchNone, PullProcessorStatus);
            instructions[0x29] = new Instruction(0x29, "AND", AddressingMode.Immediate, 2, FetchImmediate, LogicalAnd);
            instructions[0x2A] = new Instruction(0x2A, "ROL", AddressingMode.Accumulator, 2, FetchNone, RotateLeftAccumulator);
            instructions[0x2B] = new Instruction(0x2B, "ANC", AddressingMode.Immediate, 2, FetchImmediate, AndWithCarry);
            instructions[0x2C] = new Instruction(0x2C, "BIT", AddressingMode.Absolute, 4, FetchAbsolute, BitTest);
            instructions[0x2D] = new Instruction(0x2D, "AND", AddressingMode.Absolute, 4, FetchAbsolute, LogicalAnd);
            instructions[0x2E] = new Instruction(0x2E, "ROL", AddressingMode.Absolute, 6, FetchAbsolute, RotateLeftMemory);
            instructions[0x2F] = new Instruction(0x2F, "RLA", AddressingMode.Absolute, 6, FetchAbsolute, RotateLeftThenAnd);

            // 0x30 - 0x3F
            instructions[0x30] = new Instruction(0x30, "BMI", AddressingMode.Relative, 2, FetchRelative, BranchIfMinus);
            instructions[0x31] = new Instruction(0x31, "AND", AddressingMode.IndirectIndexed, 5, FetchIndirectIndexed, LogicalAnd);
            instructions[0x32] = new Instruction(0x32, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0x33] = new Instruction(0x33, "RLA", AddressingMode.IndirectIndexed, 8, FetchIndirectIndexed, RotateLeftThenAnd);
            instructions[0x34] = new Instruction(0x34, "DOP", AddressingMode.ZeroPageX, 4, FetchZeroPageX, DoubleNop);
            instructions[0x35] = new Instruction(0x35, "AND", AddressingMode.ZeroPageX, 4, FetchZeroPageX, LogicalAnd);
            instructions[0x36] = new Instruction(0x36, "ROL", AddressingMode.ZeroPageX, 6, FetchZeroPageX, RotateLeftMemory);
            instructions[0x37] = new Instruction(0x37, "RLA", AddressingMode.ZeroPageX, 6, FetchZeroPageX, RotateLeftThenAnd);
            instructions[0x38] = new Instruction(0x38, "SEC", AddressingMode.Implied, 2, FetchNone, SetCarryFlag);
            instructions[0x39] = new Instruction(0x39, "AND", AddressingMode.AbsoluteY, 4, FetchAbsoluteY, LogicalAnd);
            instructions[0x3A] = new Instruction(0x3A, "NOPu", AddressingMode.Implied, 2, FetchNone, NoOperation);
            instructions[0x3B] = new Instruction(0x3B, "RLA", AddressingMode.AbsoluteY, 7, FetchAbsoluteY, RotateLeftThenAnd);
            instructions[0x3C] = new Instruction(0x3C, "TOP", AddressingMode.AbsoluteX, 3, FetchAbsoluteX, TrippleNop);
            instructions[0x3D] = new Instruction(0x3D, "AND", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, LogicalAnd);
            instructions[0x3E] = new Instruction(0x3E, "ROL", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, RotateLeftMemory);
            instructions[0x3F] = new Instruction(0x3F, "RLA", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, RotateLeftThenAnd);

            // 0x40 - 0x4F
            instructions[0x40] = new Instruction(0x40, "RTI", AddressingMode.Implied, 6, FetchNone, ReturnFromInterrupt);
            instructions[0x41] = new Instruction(0x41, "EOR", AddressingMode.IndexedIndirect, 6, FetchIndexedIndirect, LogicalExclusiveOr);
            instructions[0x42] = new Instruction(0x42, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0x43] = new Instruction(0x43, "SRE", AddressingMode.IndexedIndirect, 8, FetchIndexedIndirect, LogicalShiftRightThenXor);
            instructions[0x44] = new Instruction(0x44, "DOP", AddressingMode.ZeroPage, 3, FetchZeroPage, DoubleNop);
            instructions[0x45] = new Instruction(0x45, "EOR", AddressingMode.ZeroPage, 3, FetchZeroPage, LogicalExclusiveOr);
            instructions[0x46] = new Instruction(0x46, "LSR", AddressingMode.ZeroPage, 5, FetchZeroPage, LogicalShiftRightMemory);
            instructions[0x47] = new Instruction(0x47, "SRE", AddressingMode.ZeroPage, 5, FetchZeroPage, LogicalShiftRightThenXor);
            instructions[0x48] = new Instruction(0x48, "PHA", AddressingMode.Implied, 3, FetchNone, PushAccumulator);
            instructions[0x49] = new Instruction(0x49, "EOR", AddressingMode.Immediate, 2, FetchImmediate, LogicalExclusiveOr);
            instructions[0x4A] = new Instruction(0x4A, "LSR", AddressingMode.Accumulator, 2, FetchNone, LogicalShiftRightAccumulator);
            instructions[0x4B] = new Instruction(0x4B, "ALR", AddressingMode.Immediate, 2, FetchImmediate, AndThenShiftRight);
            instructions[0x4C] = new Instruction(0x4C, "JMP", AddressingMode.Absolute, 3, FetchAbsolute, Jump);
            instructions[0x4D] = new Instruction(0x4D, "EOR", AddressingMode.Absolute, 4, FetchAbsolute, LogicalExclusiveOr);
            instructions[0x4E] = new Instruction(0x4E, "LSR", AddressingMode.Absolute, 6, FetchAbsolute, LogicalShiftRightMemory);
            instructions[0x4F] = new Instruction(0x4F, "SRE", AddressingMode.Absolute, 6, FetchAbsolute, LogicalShiftRightThenXor);

            // 0x50 - 0x5F
            instructions[0x50] = new Instruction(0x50, "BVC", AddressingMode.Relative, 2, FetchRelative, BranchIfOverflowClear);
            instructions[0x51] = new Instruction(0x51, "EOR", AddressingMode.IndirectIndexed, 5, FetchIndirectIndexed, LogicalExclusiveOr);
            instructions[0x52] = new Instruction(0x52, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0x53] = new Instruction(0x53, "SRE", AddressingMode.IndirectIndexed, 8, FetchIndirectIndexed, LogicalShiftRightThenXor);
            instructions[0x54] = new Instruction(0x54, "DOP", AddressingMode.ZeroPageX, 4, FetchZeroPageX, DoubleNop);
            instructions[0x55] = new Instruction(0x55, "EOR", AddressingMode.ZeroPageX, 4, FetchZeroPageX, LogicalExclusiveOr);
            instructions[0x56] = new Instruction(0x56, "LSR", AddressingMode.ZeroPageX, 6, FetchZeroPageX, LogicalShiftRightMemory);
            instructions[0x57] = new Instruction(0x57, "SRE", AddressingMode.ZeroPageX, 6, FetchZeroPageX, LogicalShiftRightThenXor);
            instructions[0x58] = new Instruction(0x58, "CLI", AddressingMode.Implied, 2, FetchNone, ClearInterruptDisableFlag);
            instructions[0x59] = new Instruction(0x59, "EOR", AddressingMode.AbsoluteY, 4, FetchAbsoluteY, LogicalExclusiveOr);
            instructions[0x5A] = new Instruction(0x5A, "NOPu", AddressingMode.Implied, 2, FetchNone, NoOperation);
            instructions[0x5B] = new Instruction(0x5B, "SRE", AddressingMode.AbsoluteY, 7, FetchAbsoluteY, LogicalShiftRightThenXor);
            instructions[0x5C] = new Instruction(0x5C, "TOP", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, TrippleNop);
            instructions[0x5D] = new Instruction(0x5D, "EOR", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, LogicalExclusiveOr);
            instructions[0x5E] = new Instruction(0x5E, "LSR", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, LogicalShiftRightMemory);
            instructions[0x5F] = new Instruction(0x5F, "SRE", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, LogicalShiftRightThenXor);

            // 0x60 - 0x6F
            instructions[0x60] = new Instruction(0x60, "RTS", AddressingMode.Implied, 6, FetchNone, ReturnFromSubroutine);
            instructions[0x61] = new Instruction(0x61, "ADC", AddressingMode.IndexedIndirect, 6, FetchIndexedIndirect, AddWithCarry);
            instructions[0x62] = new Instruction(0x62, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0x63] = new Instruction(0x63, "RRA", AddressingMode.IndexedIndirect, 8, FetchIndexedIndirect, RotateRightThenAdd);
            instructions[0x64] = new Instruction(0x64, "DOP", AddressingMode.ZeroPage, 3, FetchZeroPage, DoubleNop);
            instructions[0x65] = new Instruction(0x65, "ADC", AddressingMode.ZeroPage, 3, FetchZeroPage, AddWithCarry);
            instructions[0x66] = new Instruction(0x66, "ROR", AddressingMode.ZeroPage, 5, FetchZeroPage, RotateRightMemory);
            instructions[0x67] = new Instruction(0x67, "RRA", AddressingMode.ZeroPage, 5, FetchZeroPage, RotateRightThenAdd);
            instructions[0x68] = new Instruction(0x68, "PLA", AddressingMode.Implied, 4, FetchNone, PullAccumulator);
            instructions[0x69] = new Instruction(0x69, "ADC", AddressingMode.Immediate, 2, FetchImmediate, AddWithCarry);
            instructions[0x6A] = new Instruction(0x6A, "ROR", AddressingMode.Accumulator, 2, FetchNone, RotateRightAccumulator);
            instructions[0x6B] = new Instruction(0x6B, "ARR", AddressingMode.Immediate, 2, FetchImmediate, AndAndRorateRight);
            instructions[0x6C] = new Instruction(0x6C, "JMP", AddressingMode.Indirect, 5, FetchIndirect, Jump);
            instructions[0x6D] = new Instruction(0x6D, "ADC", AddressingMode.Absolute, 4, FetchAbsolute, AddWithCarry);
            instructions[0x6E] = new Instruction(0x6E, "ROR", AddressingMode.Absolute, 6, FetchAbsolute, RotateRightMemory);
            instructions[0x6F] = new Instruction(0x6F, "RRA", AddressingMode.Absolute, 6, FetchAbsolute, RotateRightThenAdd);

            // 0x70 - 0x7F
            instructions[0x70] = new Instruction(0x70, "BVS", AddressingMode.Relative, 2, FetchRelative, BranchIfOverflowSet);
            instructions[0x71] = new Instruction(0x71, "ADC", AddressingMode.IndirectIndexed, 5, FetchIndirectIndexed, AddWithCarry);
            instructions[0x72] = new Instruction(0x72, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0x73] = new Instruction(0x73, "RRA", AddressingMode.IndirectIndexed, 8, FetchIndirectIndexed, RotateRightThenAdd);
            instructions[0x74] = new Instruction(0x74, "DOP", AddressingMode.ZeroPageX, 4, FetchZeroPageX, DoubleNop);
            instructions[0x75] = new Instruction(0x75, "ADC", AddressingMode.ZeroPageX, 4, FetchZeroPageX, AddWithCarry);
            instructions[0x76] = new Instruction(0x76, "ROR", AddressingMode.ZeroPageX, 6, FetchZeroPageX, RotateRightMemory);
            instructions[0x77] = new Instruction(0x77, "RRA", AddressingMode.ZeroPageX, 6, FetchZeroPageX, RotateRightThenAdd);
            instructions[0x78] = new Instruction(0x78, "SEI", AddressingMode.Implied, 2, FetchNone, SetInterruptDisableFlag);
            instructions[0x79] = new Instruction(0x79, "ADC", AddressingMode.AbsoluteY, 4, FetchAbsoluteY, AddWithCarry);
            instructions[0x7A] = new Instruction(0x7A, "NOPu", AddressingMode.Absolute, 2, FetchAbsolute, NoOperation);
            instructions[0x7B] = new Instruction(0x7B, "RRA", AddressingMode.AbsoluteY, 7, FetchAbsoluteY, RotateRightThenAdd);
            instructions[0x7C] = new Instruction(0x7C, "TOP", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, TrippleNop);
            instructions[0x7D] = new Instruction(0x7D, "ADC", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, AddWithCarry);
            instructions[0x7E] = new Instruction(0x7E, "ROR", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, RotateRightMemory);
            instructions[0x7F] = new Instruction(0x7F, "RRA", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, RotateRightThenAdd);

            // 0x80 - 0x8F
            instructions[0x80] = new Instruction(0x80, "DOP", AddressingMode.Immediate, 2, FetchImmediate, DoubleNop);
            instructions[0x81] = new Instruction(0x81, "STA", AddressingMode.IndexedIndirect, 6, FetchIndexedIndirect, StoreAccumulator);
            instructions[0x82] = new Instruction(0x82, "DOP", AddressingMode.Immediate, 2, FetchImmediate, DoubleNop);
            instructions[0x83] = new Instruction(0x83, "SAX", AddressingMode.IndexedIndirect, 6, FetchIndexedIndirect, LogicalAndX);
            instructions[0x84] = new Instruction(0x84, "STY", AddressingMode.ZeroPage, 3, FetchZeroPage, StoreRegisterY);
            instructions[0x85] = new Instruction(0x85, "STA", AddressingMode.ZeroPage, 3, FetchZeroPage, StoreAccumulator);
            instructions[0x86] = new Instruction(0x86, "STX", AddressingMode.ZeroPage, 3, FetchZeroPage, StoreRegisterX);
            instructions[0x87] = new Instruction(0x87, "SAX", AddressingMode.ZeroPage, 3, FetchZeroPage, LogicalAndX);
            instructions[0x88] = new Instruction(0x88, "DEY", AddressingMode.Implied, 2, FetchNone, DecrementRegisterY);
            instructions[0x89] = new Instruction(0x89, "DOP", AddressingMode.Immediate, 2, FetchImmediate, DoubleNop);
            instructions[0x8A] = new Instruction(0x8A, "TXA", AddressingMode.Implied, 2, FetchNone, TransferXToAccumulator);
            instructions[0x8B] = new Instruction(0x8B, "XAA", AddressingMode.Immediate, 2, FetchImmediate, UndocumentedXaa);
            instructions[0x8C] = new Instruction(0x8C, "STY", AddressingMode.Absolute, 4, FetchAbsolute, StoreRegisterY);
            instructions[0x8D] = new Instruction(0x8D, "STA", AddressingMode.Absolute, 5, FetchAbsolute, StoreAccumulator);
            instructions[0x8E] = new Instruction(0x8E, "STX", AddressingMode.Absolute, 4, FetchAbsolute, StoreRegisterX);
            instructions[0x8F] = new Instruction(0x8F, "SAX", AddressingMode.Absolute, 4, FetchAbsolute, LogicalAndX);

            // 0x90 - 0x9F
            instructions[0x90] = new Instruction(0x90, "BCC", AddressingMode.Relative, 2, FetchRelative, BranchIfCarryClear);
            instructions[0x91] = new Instruction(0x91, "STA", AddressingMode.IndirectIndexed, 6, FetchIndirectIndexed, StoreAccumulator);
            instructions[0x92] = new Instruction(0x92, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0x93] = new Instruction(0x93, "AXA", AddressingMode.IndirectIndexed, 6, FetchIndirectIndexed, UndocumentedAxa);
            instructions[0x94] = new Instruction(0x94, "STY", AddressingMode.ZeroPageX, 4, FetchZeroPageX, StoreRegisterY);
            instructions[0x95] = new Instruction(0x95, "STA", AddressingMode.ZeroPageX, 4, FetchZeroPageX, StoreAccumulator);
            instructions[0x96] = new Instruction(0x96, "STX", AddressingMode.ZeroPageY, 4, FetchZeroPageY, StoreRegisterX);
            instructions[0x97] = new Instruction(0x97, "SAX", AddressingMode.ZeroPageY, 4, FetchZeroPageY, LogicalAndX);
            instructions[0x98] = new Instruction(0x98, "TYA", AddressingMode.Implied, 2, FetchNone, TransferYToAccumulator);
            instructions[0x99] = new Instruction(0x99, "STA", AddressingMode.AbsoluteY, 5, FetchAbsoluteY, StoreAccumulator);
            instructions[0x9A] = new Instruction(0x9A, "TXS", AddressingMode.Implied, 2, FetchNone, TransferXToStackPointer);
            instructions[0x9B] = new Instruction(0x9B, "TAS", AddressingMode.AbsoluteY, 5, FetchAbsoluteY, UndocumentedTas);
            instructions[0x9C] = new Instruction(0x9C, "SHY", AddressingMode.AbsoluteX, 5, FetchAbsoluteX, UndocumentedShy);
            instructions[0x9D] = new Instruction(0x9D, "STA", AddressingMode.AbsoluteX, 5, FetchAbsoluteX, StoreAccumulator);
            instructions[0x9E] = new Instruction(0x9E, "SHX", AddressingMode.AbsoluteY, 5, FetchAbsoluteY, UndocumentedShx);
            instructions[0x9F] = new Instruction(0x9F, "AXA", AddressingMode.AbsoluteY, 5, FetchAbsoluteY, UndocumentedAxa);

            // 0xA0 - 0xAF
            instructions[0xA0] = new Instruction(0xA0, "LDY", AddressingMode.Immediate, 2, FetchImmediate, LoadRegisterY);
            instructions[0xA1] = new Instruction(0xA1, "LDA", AddressingMode.IndexedIndirect, 6, FetchIndexedIndirect, LoadAccumulator);
            instructions[0xA2] = new Instruction(0xA2, "LDX", AddressingMode.Immediate, 2, FetchImmediate, LoadRegisterX);
            instructions[0xA3] = new Instruction(0xA3, "LAX", AddressingMode.IndexedIndirect, 6, FetchIndexedIndirect, LoadAandX);
            instructions[0xA4] = new Instruction(0xA4, "LDY", AddressingMode.ZeroPage, 3, FetchZeroPage, LoadRegisterY);
            instructions[0xA5] = new Instruction(0xA5, "LDA", AddressingMode.ZeroPage, 3, FetchZeroPage, LoadAccumulator);
            instructions[0xA6] = new Instruction(0xA6, "LDX", AddressingMode.ZeroPage, 3, FetchZeroPage, LoadRegisterX);
            instructions[0xA7] = new Instruction(0xA7, "LAX", AddressingMode.ZeroPage, 3, FetchZeroPage, LoadAandX);
            instructions[0xA8] = new Instruction(0xA8, "TAY", AddressingMode.Implied, 2, FetchNone, TransferAccumulatorToY);
            instructions[0xA9] = new Instruction(0xA9, "LDA", AddressingMode.Immediate, 2, FetchImmediate, LoadAccumulator);
            instructions[0xAA] = new Instruction(0xAA, "TAX", AddressingMode.Implied, 2, FetchNone, TransferAccumulatorToX);
            instructions[0xAB] = new Instruction(0xAB, "ATX", AddressingMode.Immediate, 2, FetchImmediate, LogicalAndThenTransferToX);
            instructions[0xAC] = new Instruction(0xAC, "LDY", AddressingMode.Absolute, 4, FetchAbsolute, LoadRegisterY);
            instructions[0xAD] = new Instruction(0xAD, "LDA", AddressingMode.Absolute, 4, FetchAbsolute, LoadAccumulator);
            instructions[0xAE] = new Instruction(0xAE, "LDX", AddressingMode.Absolute, 4, FetchAbsolute, LoadRegisterX);
            instructions[0xAF] = new Instruction(0xAF, "LAX", AddressingMode.Absolute, 4, FetchAbsolute, LoadAandX);

            // 0xB0 - 0xBF
            instructions[0xB0] = new Instruction(0xB0, "BCS", AddressingMode.Relative, 2, FetchRelative, BranchIfCarrySet);
            instructions[0xB1] = new Instruction(0xB1, "LDA", AddressingMode.IndirectIndexed, 5, FetchIndirectIndexed, LoadAccumulator);
            instructions[0xB2] = new Instruction(0xB2, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0xB3] = new Instruction(0xB3, "LAX", AddressingMode.IndirectIndexed, 5, FetchIndirectIndexed, LoadAandX);
            instructions[0xB4] = new Instruction(0xB4, "LDY", AddressingMode.ZeroPageX, 4, FetchZeroPageX, LoadRegisterY);
            instructions[0xB5] = new Instruction(0xB5, "LDA", AddressingMode.ZeroPageX, 4, FetchZeroPageX, LoadAccumulator);
            instructions[0xB6] = new Instruction(0xB6, "LDX", AddressingMode.ZeroPageY, 4, FetchZeroPageY, LoadRegisterX);
            instructions[0xB7] = new Instruction(0xB7, "LAX", AddressingMode.ZeroPageY, 4, FetchZeroPageY, LoadAandX);
            instructions[0xB8] = new Instruction(0xB8, "CLV", AddressingMode.Implied, 2, FetchNone, ClearOverflowFlag);
            instructions[0xB9] = new Instruction(0xB9, "LDA", AddressingMode.AbsoluteY, 4, FetchAbsoluteY, LoadAccumulator);
            instructions[0xBA] = new Instruction(0xBA, "TSX", AddressingMode.Implied, 2, FetchNone, TransferStackPointerToX);
            instructions[0xBB] = new Instruction(0xBB, "LAS", AddressingMode.AbsoluteY, 4, FetchAbsoluteY, UndocumentedLas);
            instructions[0xBC] = new Instruction(0xBC, "LDY", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, LoadRegisterY);
            instructions[0xBD] = new Instruction(0xBD, "LDA", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, LoadAccumulator);
            instructions[0xBE] = new Instruction(0xBE, "LDX", AddressingMode.AbsoluteY, 4, FetchAbsoluteY, LoadRegisterX);
            instructions[0xBF] = new Instruction(0xBF, "LAX", AddressingMode.AbsoluteY, 4, FetchAbsoluteY, LoadAandX);

            // 0xC0 - 0xCF
            instructions[0xC0] = new Instruction(0xC0, "CPY", AddressingMode.Immediate, 2, FetchImmediate, CompareRegisterY);
            instructions[0xC1] = new Instruction(0xC1, "CMP", AddressingMode.IndexedIndirect, 6, FetchIndexedIndirect, CompareAccumulator);
            instructions[0xC2] = new Instruction(0xC2, "DOP", AddressingMode.Immediate, 2, FetchImmediate, DoubleNop);
            instructions[0xC3] = new Instruction(0xC3, "DCP", AddressingMode.IndexedIndirect, 8, FetchIndexedIndirect, DecrementMemoryWithoutBorrow);
            instructions[0xC4] = new Instruction(0xC4, "CPY", AddressingMode.ZeroPage, 3, FetchZeroPage, CompareRegisterY);
            instructions[0xC5] = new Instruction(0xC5, "CMP", AddressingMode.ZeroPage, 3, FetchZeroPage, CompareAccumulator);
            instructions[0xC6] = new Instruction(0xC6, "DEC", AddressingMode.ZeroPage, 5, FetchZeroPage, DecrementMemory);
            instructions[0xC7] = new Instruction(0xC7, "DCP", AddressingMode.ZeroPage, 5, FetchZeroPage, DecrementMemoryWithoutBorrow);
            instructions[0xC8] = new Instruction(0xC8, "INY", AddressingMode.Implied, 2, FetchNone, IncrementRegisterY);
            instructions[0xC9] = new Instruction(0xC9, "CMP", AddressingMode.Immediate, 2, FetchImmediate, CompareAccumulator);
            instructions[0xCA] = new Instruction(0xCA, "DEX", AddressingMode.Implied, 2, FetchNone, DecrementRegisterX);
            instructions[0xCB] = new Instruction(0xCB, "AXS", AddressingMode.Immediate, 2, FetchImmediate, UndocumentedAxs);
            instructions[0xCC] = new Instruction(0xCC, "CPY", AddressingMode.Absolute, 4, FetchAbsolute, CompareRegisterY);
            instructions[0xCD] = new Instruction(0xCD, "CMP", AddressingMode.Absolute, 4, FetchAbsolute, CompareAccumulator);
            instructions[0xCE] = new Instruction(0xCE, "DEC", AddressingMode.Absolute, 6, FetchAbsolute, DecrementMemory);
            instructions[0xCF] = new Instruction(0xCF, "DCP", AddressingMode.Absolute, 6, FetchAbsolute, DecrementMemoryWithoutBorrow);

            // 0xD0 - 0xDF
            instructions[0xD0] = new Instruction(0xD0, "BNE", AddressingMode.Relative, 2, FetchRelative, BranchIfNotEqual);
            instructions[0xD1] = new Instruction(0xD1, "CMP", AddressingMode.IndirectIndexed, 5, FetchIndirectIndexed, CompareAccumulator);
            instructions[0xD2] = new Instruction(0xD2, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0xD3] = new Instruction(0xD3, "DCP", AddressingMode.IndirectIndexed, 8, FetchIndirectIndexed, DecrementMemoryWithoutBorrow);
            instructions[0xD4] = new Instruction(0xD4, "DOP", AddressingMode.ZeroPageX, 4, FetchZeroPageX, DoubleNop);
            instructions[0xD5] = new Instruction(0xD5, "CMP", AddressingMode.ZeroPageX, 4, FetchZeroPageX, CompareAccumulator);
            instructions[0xD6] = new Instruction(0xD6, "DEC", AddressingMode.ZeroPageX, 6, FetchZeroPageX, DecrementMemory);
            instructions[0xD7] = new Instruction(0xD7, "DCP", AddressingMode.ZeroPageX, 6, FetchZeroPageX, DecrementMemoryWithoutBorrow);
            instructions[0xD8] = new Instruction(0xD8, "CLD", AddressingMode.Implied, 2, FetchNone, ClearDecimalModeFlag);
            instructions[0xD9] = new Instruction(0xD9, "CMP", AddressingMode.AbsoluteY, 4, FetchAbsoluteY, CompareAccumulator);
            instructions[0xDA] = new Instruction(0xDA, "NOPu", AddressingMode.Implied, 2, FetchNone, NoOperation);
            instructions[0xDB] = new Instruction(0xDB, "DCP", AddressingMode.AbsoluteY, 7, FetchAbsoluteY, DecrementMemoryWithoutBorrow);
            instructions[0xDC] = new Instruction(0xDC, "TOP", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, TrippleNop);
            instructions[0xDD] = new Instruction(0xDD, "CMP", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, CompareAccumulator);
            instructions[0xDE] = new Instruction(0xDE, "DEC", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, DecrementMemory);
            instructions[0xDF] = new Instruction(0xDF, "DCP", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, DecrementMemoryWithoutBorrow);

            // 0xE0 - 0xEF
            instructions[0xE0] = new Instruction(0xE0, "CPX", AddressingMode.Immediate, 2, FetchImmediate, CompareRegisterX);
            instructions[0xE1] = new Instruction(0xE1, "SBC", AddressingMode.IndexedIndirect, 6, FetchIndexedIndirect, SubtractWithCarry);
            instructions[0xE2] = new Instruction(0xE2, "DOP", AddressingMode.Immediate, 2, FetchImmediate, DoubleNop);
            instructions[0xE3] = new Instruction(0xE3, "ISC", AddressingMode.IndexedIndirect, 8, FetchIndexedIndirect, IncrementThenSubtract);
            instructions[0xE4] = new Instruction(0xE4, "CPX", AddressingMode.ZeroPage, 3, FetchZeroPage, CompareRegisterX);
            instructions[0xE5] = new Instruction(0xE5, "SBC", AddressingMode.ZeroPage, 3, FetchZeroPage, SubtractWithCarry);
            instructions[0xE6] = new Instruction(0xE6, "INC", AddressingMode.ZeroPage, 5, FetchZeroPage, IncrementMemory);
            instructions[0xE7] = new Instruction(0xE7, "ISC", AddressingMode.ZeroPage, 5, FetchZeroPage, IncrementThenSubtract);
            instructions[0xE8] = new Instruction(0xE8, "INX", AddressingMode.Implied, 2, FetchNone, IncrementRegisterX);
            instructions[0xE9] = new Instruction(0xE9, "SBC", AddressingMode.Immediate, 2, FetchImmediate, SubtractWithCarry);
            instructions[0xEA] = new Instruction(0xEA, "NOP", AddressingMode.Implied, 2, FetchNone, NoOperation); // legal NOP
            instructions[0xEB] = new Instruction(0xEB, "SBCu", AddressingMode.Immediate, 2, FetchImmediate, SubtractWithCarry); // undocumented, but like $E9
            instructions[0xEC] = new Instruction(0xEC, "CPX", AddressingMode.Absolute, 4, FetchAbsolute, CompareRegisterX);
            instructions[0xED] = new Instruction(0xED, "SBC", AddressingMode.Absolute, 4, FetchAbsolute, SubtractWithCarry);
            instructions[0xEE] = new Instruction(0xEE, "INC", AddressingMode.Absolute, 6, FetchAbsolute, IncrementMemory);
            instructions[0xEF] = new Instruction(0xEF, "ISC", AddressingMode.Absolute, 6, FetchAbsolute, IncrementThenSubtract);

            // 0xF0 - 0xFF
            instructions[0xF0] = new Instruction(0xF0, "BEQ", AddressingMode.Relative, 2, FetchRelative, BranchIfEqual);
            instructions[0xF1] = new Instruction(0xF1, "SBC", AddressingMode.IndirectIndexed, 5, FetchIndirectIndexed, SubtractWithCarry);
            instructions[0xF2] = new Instruction(0xF2, "KIL", AddressingMode.Implied, 2, FetchNone, Lockup);
            instructions[0xF3] = new Instruction(0xF3, "ISC", AddressingMode.IndirectIndexed, 8, FetchIndirectIndexed, IncrementThenSubtract);
            instructions[0xF4] = new Instruction(0xF4, "DOP", AddressingMode.ZeroPageX, 4, FetchZeroPageX, DoubleNop);
            instructions[0xF5] = new Instruction(0xF5, "SBC", AddressingMode.ZeroPageX, 4, FetchZeroPageX, SubtractWithCarry);
            instructions[0xF6] = new Instruction(0xF6, "INC", AddressingMode.ZeroPageX, 6, FetchZeroPageX, IncrementMemory);
            instructions[0xF7] = new Instruction(0xF7, "ISC", AddressingMode.ZeroPageX, 6, FetchZeroPageX, IncrementThenSubtract);
            instructions[0xF8] = new Instruction(0xF8, "SED", AddressingMode.Implied, 2, FetchNone, SetDecimalModeFlag);
            instructions[0xF9] = new Instruction(0xF9, "SBC", AddressingMode.AbsoluteY, 4, FetchAbsoluteY, SubtractWithCarry);
            instructions[0xFA] = new Instruction(0xFA, "NOPu", AddressingMode.Implied, 2, FetchNone, NoOperation);
            instructions[0xFB] = new Instruction(0xFB, "ISC", AddressingMode.AbsoluteY, 7, FetchAbsoluteY, IncrementThenSubtract);
            instructions[0xFC] = new Instruction(0xFC, "TOP", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, TrippleNop);
            instructions[0xFD] = new Instruction(0xFD, "SBC", AddressingMode.AbsoluteX, 4, FetchAbsoluteX, SubtractWithCarry);
            instructions[0xFE] = new Instruction(0xFE, "INC", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, IncrementMemory);
            instructions[0xFF] = new Instruction(0xFF, "ISC", AddressingMode.AbsoluteX, 7, FetchAbsoluteX, IncrementThenSubtract);

            // set up instruction variants
            foreach (Instruction instruction in instructions)
            {
                string opName = instruction.Name;
                List<Instruction> group = null;
                if (instructionVariants.ContainsKey(opName))
                    group = instructionVariants[opName];
                else
                {
                    group = new List<Instruction>();
                    instructionVariants[opName] = group;
                }

                group.Add(instruction);
            }
        }

        // private helper functions

        // sets zero and negative flags for the given byte value
        private void SetZeroAndNegativeFlags(byte value)
        {
            State state = Processor.State;
            state.ZeroFlag = value == 0;
            state.NegativeFlag = (value & 0x80) != 0;
        }

        // adds additional branch cycles
        private void AddBranchCycles(ushort address)
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

        // instruction variants
        private Dictionary<string, List<Instruction>> instructionVariants = new Dictionary<string, List<Instruction>>();

    }
}
