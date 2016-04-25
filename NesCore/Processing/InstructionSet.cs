using NesCore.Addressing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Processing
{
    public class InstructionSet
    {
        public InstructionSet(Processor processor)
        {
            Processor = processor;
            Memory = processor.Console.Memory;

            Initialise();
        }

        public Processor Processor { get; private set; }
        public Memory Memory { get; private set; }

        public Instruction this[byte opCode]
        {
            get { return instructions[opCode]; }
        }

        private void Initialise()
        {
            // general instruction operations

            // general no operation
            Execute DoNothing = (address, mode) => { };

            // ORA - logical inclusive OR
            Execute Ora = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator |= Memory.Read(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // ASL - arithmetic shift left
            Execute Asl = (address, mode) =>
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
            Execute Php = (address, mode) =>
            {
                Processor.Push((byte)(Processor.State.Flags | State.BreakCommandMask));
            };

            // BPL - branch on plus
            Execute Bpl = (address, mode) =>
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
            Execute Clc = (address, mode) =>
            {
                Processor.State.CarryFlag = false;
            };

            // JSR - jump to subroutine
            Execute Jsr = (address, mode) =>
            {
                State state = Processor.State;
                Processor.Push16((UInt16)(state.ProgramCounter - 1));
                state.ProgramCounter = address;
            };

            // AND - logical And
            Execute And = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator &= Memory.Read(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };

            // BIT - bit test
            Execute Bit = (address, mode) =>
            {
                State state = Processor.State;
                byte value = Memory.Read(address);
                state.OverflowFlag = (value & 0x40) != 0; // bit 6
                state.NegativeFlag = (value & 0x80) != 0; // bit 7
                state.ZeroFlag = (value & state.Accumulator) != 0;
            };

            // ROL - rotate left
            Execute Rol = (address, mode) =>
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
                    Memory.Write(address, value);
                    SetZeroAndNegativeFlags(value);          
                }
            };

            // PLP - pull processor status
            Execute Plp = (address, mode) =>
            {
                State state = Processor.State;
                state.Flags = Processor.Pull();
                state.BreakCommandFlag = false; // & 0xEF
                state.UnusedFlag = true; // | 0x20
            };

            // BMI - branch on minus
            Execute Bmi = (address, mode) =>
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
            Execute Sec = (address, mode) =>
            {
                Processor.State.CarryFlag = true;
            };

            // RTI - Return from interrupt
            Execute Rti = (address, mode) =>
            {
                State state = Processor.State;
                state.Flags = Processor.Pull();
                state.BreakCommandFlag = false; // & 0xEF
                state.UnusedFlag = true; // | 0x20

                state.ProgramCounter = Processor.Pull16();
            };

            // EOR - logical exlusive OR
            Execute Eor = (address, mode) =>
            {
                State state = Processor.State;
                state.Accumulator ^= Memory.Read(address);
                SetZeroAndNegativeFlags(state.Accumulator);
            };




            // SEI - set interrupt disable
            Execute Sei = (address, mode) =>
            {
                Processor.State.InterruptDisableFlag = true;
            };

            // Op Codes

            // BRK - force interrupt
            instructions[0x00] = new Instruction("BRK", AddressingMode.Implied, 1, 0,
                (address, mode) =>
                {
                    Processor.Push16(Processor.State.ProgramCounter);
                    Php(address, mode);
                    Sei(address, mode);
                    Processor.State.ProgramCounter = Memory.Read16(Memory.IrqVector);
                });

            // 0x00 - 0x07

            // ORA - logical inclusive OR - indexed indirect mode
            instructions[0x01] = new Instruction("ORA", AddressingMode.IndexedIndirect, 2, 6, Ora);

            // KIL - illegal opcode
            instructions[0x02] = new Instruction("KIL", AddressingMode.Implied, 0, 2, DoNothing);

            // SLO - illegal opcode
            instructions[0x03] = new Instruction("SLO", AddressingMode.IndexedIndirect, 0, 8, DoNothing);

            // NOP - no operation
            instructions[0x04] = new Instruction("NOP", AddressingMode.ZeroPage, 2, 3, DoNothing);

            // ORA - logical inclusive ore - zero page mode
            instructions[0x05] = new Instruction("ORA", AddressingMode.ZeroPage, 2, 3, Ora);

            // ASL - arithmetic shift left - zero page mode
            instructions[0x06] = new Instruction("ASL", AddressingMode.ZeroPage, 2, 5, Asl);

            // SLO - illegal opcode
            instructions[0x07] = new Instruction("SLO", AddressingMode.ZeroPage, 0, 5, DoNothing);

            // 0x08 - 0x0F

            // PHP - push processor status
            instructions[0x08] = new Instruction("PHP", AddressingMode.Implied, 1, 3, Php);

            // ORA - logical inclusive OR - immediate mode
            instructions[0x09] = new Instruction("ORA", AddressingMode.Immediate, 2, 2, Ora);

            // ASL - arithmetic shift left - accumulator mode
            instructions[0x0A] = new Instruction("ASL", AddressingMode.Accumulator, 1, 2, Asl);

            // ANC - illegal opcode
            instructions[0x0B] = new Instruction("ANC", AddressingMode.Immediate, 0, 2, DoNothing);

            // NOP - no operation
            instructions[0x0C] = new Instruction("NOP", AddressingMode.Absolute, 3, 4, DoNothing);

            // ORA - logical inclusive OR - absolute mode
            instructions[0x0D] = new Instruction("ORA", AddressingMode.Absolute, 3, 4, Ora);

            // ASL - arithmetic shift left - absolute mode
            instructions[0x0E] = new Instruction("ASL", AddressingMode.Absolute, 3, 6, Asl);

            // SLO - illegal opcode
            instructions[0x0F] = new Instruction("SLO", AddressingMode.Absolute, 0, 6, DoNothing);

            // 0x10 - 0x17

            // BPL - branch on plus
            instructions[0x10] = new Instruction("BPL", AddressingMode.Relative, 2, 2, 1, Bpl);

            // ORA - logical inclusive OR - indirect indexed
            instructions[0x11] = new Instruction("ORA", AddressingMode.IndirectIndexed, 2, 5, 1, Ora);

            // KIL - illegal opcode
            instructions[0x12] = new Instruction("KIL", AddressingMode.Implied, 0, 2, DoNothing);

            // SLO - illegal opcode
            instructions[0x13] = new Instruction("SLO", AddressingMode.IndirectIndexed, 0, 8, DoNothing);

            // NOP - no operation
            instructions[0x14] = new Instruction("NOP", AddressingMode.ZeroPageX, 2, 4, DoNothing);

            // ORA - logical inclusive OR - zero page x mode
            instructions[0x15] = new Instruction("ORA", AddressingMode.ZeroPageX, 2, 4, Ora);

            // ASL - arithmetic shift left - zero page x mode 
            instructions[0x16] = new Instruction("ASL", AddressingMode.ZeroPageX, 2, 6, Asl);

            // SLO - illegal opcode
            instructions[0x17] = new Instruction("SLO", AddressingMode.ZeroPageX, 0, 6, DoNothing);

            // 0x18 - 0x1F

            // CLC - clear carry flag - implied mode
            instructions[0x18] = new Instruction("CLC", AddressingMode.Implied, 1, 2, Clc);

            // ORA - logical inclusive OR - absolute y mode
            instructions[0x19] = new Instruction("ORA", AddressingMode.AbsoluteY, 3, 4, 1, Ora);

            // NOP - no operation - absolute y mode
            instructions[0x1A] = new Instruction("NOP", AddressingMode.AbsoluteY, 1, 2, DoNothing);

            // SLO - illegal opcode - absolute y mode
            instructions[0x1B] = new Instruction("SLO", AddressingMode.AbsoluteY, 0, 7, DoNothing);

            // NOP - no operation - absolute x mode
            instructions[0x1C] = new Instruction("NOP", AddressingMode.AbsoluteX, 3, 4, DoNothing);

            // ORA - logical inclusive OR - absolute x mode
            instructions[0x1D] = new Instruction("ORA", AddressingMode.AbsoluteX, 3, 4, 1, Ora);

            // ASL - arithmetic shift left - absolue x mode 
            instructions[0x1E] = new Instruction("ASL", AddressingMode.AbsoluteX, 3, 7, Asl);

            // SLO - illegal opcode - absolute x mode
            instructions[0x1F] = new Instruction("SLO", AddressingMode.AbsoluteX, 0, 7, DoNothing);

            // 0x20 - 0x27

            // JSR - jump to subroutine - absolute mode
            instructions[0x20] = new Instruction("JSR", AddressingMode.Absolute, 3, 6, Jsr);

            // AND - logical and - absolute mode
            instructions[0x21] = new Instruction("AND", AddressingMode.IndexedIndirect, 2, 6, And);

            // KIL - illegal opcode
            instructions[0x22] = new Instruction("KIL", AddressingMode.Implied, 0, 2, DoNothing);

            // RLA - illegal opcode - indexed indirect
            instructions[0x23] = new Instruction("RLA", AddressingMode.IndexedIndirect, 0, 8, DoNothing);

            // BIT - bit test - zero page mode
            instructions[0x24] = new Instruction("BIT", AddressingMode.ZeroPage, 2, 3, Bit);

            // AND - logical and - zero page mode
            instructions[0x25] = new Instruction("AND", AddressingMode.ZeroPage, 2, 3, And);

            // ROL - rotate left - zero page mode
            instructions[0x26] = new Instruction("ROL", AddressingMode.ZeroPage, 2, 5, Rol);

            // RLA - illegal opcode - zero page mode
            instructions[0x27] = new Instruction("RLA", AddressingMode.ZeroPage, 0, 5, DoNothing);

            // 0x28 - 0x2F

            // PLP - pull processor status - zero page
            instructions[0x28] = new Instruction("PLP", AddressingMode.Implied, 1, 4, Plp);

            // AND - logical and - immediate mode
            instructions[0x29] = new Instruction("AND", AddressingMode.Immediate, 2, 2, And);

            // ROL - rotate left - accumulator mode
            instructions[0x2A] = new Instruction("ROL", AddressingMode.Accumulator, 1, 2, Rol);

            // ANC - illegal opcode
            instructions[0x2B] = new Instruction("ANC", AddressingMode.Immediate, 0, 2, DoNothing);

            // BIT - bit test - absolute mode
            instructions[0x2C] = new Instruction("BIT", AddressingMode.Absolute, 3, 4, Bit);

            // AND - logical and - absolute mode
            instructions[0x2D] = new Instruction("AND", AddressingMode.Absolute, 3, 4, And);

            // ROL - rotate left - absolute mode
            instructions[0x2E] = new Instruction("ROL", AddressingMode.Absolute, 3, 6, Rol);

            // RLA - illegal opcode - absolute mode
            instructions[0x2F] = new Instruction("RLA", AddressingMode.Absolute, 0, 6, DoNothing);

            // 0x30 - 0x37

            // BMI - branch on minus - relative mode
            instructions[0x30] = new Instruction("BMI", AddressingMode.Relative, 2, 2, 1, Bmi);

            // AND - logical and - indirect indexed mode
            instructions[0x31] = new Instruction("AND", AddressingMode.IndirectIndexed, 2, 5, 1, And);

            // KIL - illegal opcode
            instructions[0x32] = new Instruction("KIL", AddressingMode.Implied, 0, 2, DoNothing);

            // RLA - illegal opcode - indirect indexed mode
            instructions[0x33] = new Instruction("RLA", AddressingMode.IndirectIndexed, 0, 8, DoNothing);

            // NOP - no operation - zero page x mode
            instructions[0x34] = new Instruction("NOP", AddressingMode.ZeroPageX, 2, 4, DoNothing);

            // AND - logical and - zero page x mode
            instructions[0x35] = new Instruction("AND", AddressingMode.ZeroPageX, 2, 4, And);

            // ROL - rotate left - zero page x mode
            instructions[0x36] = new Instruction("ROL", AddressingMode.ZeroPageX, 2, 6, Rol);

            // RLA - illegal opcode - zero page x mode
            instructions[0x37] = new Instruction("RLA", AddressingMode.ZeroPageX, 0, 6, DoNothing);

            // 0x38 - 0x3F

            // SEC - set carry flag - implied mode
            instructions[0x38] = new Instruction("SEC", AddressingMode.Implied, 1, 2, Sec);

            // AND - logical and - absolute y mode
            instructions[0x39] = new Instruction("AND", AddressingMode.AbsoluteY, 3, 4, 1, And);

            // NOP - no operation - absolute y mode
            instructions[0x3A] = new Instruction("NOP", AddressingMode.AbsoluteY, 1, 2, DoNothing);

            // RLA - illegal opcode - absolute y mode
            instructions[0x3B] = new Instruction("RLA", AddressingMode.AbsoluteY, 0, 7, DoNothing);

            // NOP - no operation - absolute x mode
            instructions[0x3C] = new Instruction("NOP", AddressingMode.AbsoluteX, 1, 3, DoNothing);

            // AND - logical and - absolute x mode
            instructions[0x3D] = new Instruction("AND", AddressingMode.AbsoluteX, 3, 4, 1, And);

            // ROL - rotate left - absolute x mode
            instructions[0x3E] = new Instruction("ROL", AddressingMode.AbsoluteX, 3, 7, Rol);

            // RLA - illegal opcode - absolute x mode
            instructions[0x3F] = new Instruction("RLA", AddressingMode.AbsoluteX, 0, 7, DoNothing);

            // 0x40 - 0x47

            // RTI - return from interrupt - implied mode
            instructions[0x40] = new Instruction("RTI", AddressingMode.Implied, 1, 6, Rti);

            // EOR - logical exclusive or - indexed indirect mode
            instructions[0x41] = new Instruction("EOR", AddressingMode.IndexedIndirect, 2, 6, Rti);

            // KIL - illegal opcode
            instructions[0x42] = new Instruction("KIL", AddressingMode.Implied, 0, 2, DoNothing);

            // SRE - illegal opcode - indexed indirect mode
            instructions[0x43] = new Instruction("KIL", AddressingMode.IndexedIndirect, 0, 8, DoNothing);

            // NOP - no operation - zero page mode
            instructions[0x44] = new Instruction("NOP", AddressingMode.ZeroPage, 2, 3, DoNothing);


            // 0x78 - 0x80

            // SEI - set interrupt disable
            instructions[0x78] = new Instruction("SEI", AddressingMode.Implied, 1, 0, Sei);
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
