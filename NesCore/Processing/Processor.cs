using NesCore.Addressing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Processing
{
    public class Processor
    {
        public const int Frequency = 1789773;
        public const byte StackBase = 0xFD;

        public Processor(Console console)
        {
            Console = console;
            State = new State();
            InstructionSet = new InstructionSet(this);
        }

        public Console Console { get; private set; }
        public State State { get; private set; }
        public InstructionSet InstructionSet { get; private set; }

        // resets the processor state
        public void Reset()
        {
            State.ProgramCounter = Memory.ResetVector;
            State.StackPointer = StackBase;
            State.InterruptDisableFlag = true;
        }

        public UInt64 ExecuteNextInstruction()
        {
            // consume 1 cycle and do nothing if there are pending stall cycles
            if (State.StallCycles > 0)
            {
                --State.StallCycles;
                return 1;
            }

            // handle any NMI or IRQ and clear interrupt
            if (State.InterruptType != InterruptType.None)
            {
                if (State.InterruptType == InterruptType.NonMaskable)
                    HandleNMI();
                else
                    HandleIRQ();
                State.InterruptType = InterruptType.None;
            }

            // keep ttrack of current cycle
            UInt64 cycles = State.Cycles;

            // read next op code
            Memory memory = Console.Memory;
            byte opCode = memory.Read(State.ProgramCounter);

            // get corresponding instruction
            Instruction instruction = InstructionSet[opCode];

            // determine address operand and if a page is crossed
            bool pageCrossed = false;
            UInt16 address = GetAddressOperand(instruction.AddressingMode, out pageCrossed);

            // advance program counter by instruction size
            State.ProgramCounter += instruction.Size;

            // advance cycle count by instruction cycle duration
            // note: this takes care of fixed durations; variable durations e.g. for brancing are computed within the instruction
            State.Cycles += instruction.Cycles;

            // consume an extra cycle if a page is crossed during addressing
            if (pageCrossed)
                ++State.Cycles;

            // execute the instruction
            instruction.Exceute(address, instruction.AddressingMode);

            // determine and return cycles consumed by this instruction
            UInt64 consumedCycles = State.Cycles - cycles;
            return consumedCycles;
        }
        
        // push byte onto stack
        public void Push(byte value)
        {
            // stack is located in address range 0x100 to 0x1FF and initially set to 0x1FF
            Console.Memory.Write((UInt16)(0x100 | State.StackPointer), value);
            State.StackPointer--;
        }

        // push word onto stack
        public void Push16(UInt16 value)
        {
            // push hi, then lo
            Push((byte)(value >> 8));
            Push((byte)(value & 0xFF));
        }

        // pull byte from stack
        public byte Pull()
        {
            State.StackPointer++;
            return Console.Memory.Read((UInt16)(0x100 | State.StackPointer));
        }

        // pull word from stack
        public UInt16 Pull16()
        {
            // pull lo, then hi, then combine into word
            byte lo = Pull();
            byte hi = Pull();
            return (UInt16)((hi << 8) | lo);
        }

        public void HandleNMI()
        {
        }

        public void HandleIRQ()
        {
        }

        private UInt16 GetAddressOperand(AddressingMode addressingMode, out bool pageCrossed)
        {
            Memory memory = Console.Memory;

            UInt16 address = 0;
            pageCrossed = false;
            // immediate address is word following op code
            UInt16 immediateAddress = (UInt16)(State.ProgramCounter + 1);

            switch (addressingMode)
            {
                case AddressingMode.Absolute:
                    // absolute address is the word located at immediate address
                    address = memory.Read16(immediateAddress);
                    break;
                case AddressingMode.AbsoluteX:
                    // absolute x address is the absolute address offset by the X register
                    address = (UInt16)(memory.Read16(immediateAddress) + State.RegisterX);
                    pageCrossed = memory.PagesDiffer((UInt16)(address - State.RegisterX), address);
                    break;
                case AddressingMode.AbsoluteY:
                    // absolute y address is the absolute address offset by the Y register
                    address = (UInt16)(memory.Read16(immediateAddress) + State.RegisterY);
                    pageCrossed = memory.PagesDiffer((UInt16)(address - State.RegisterY), address);
                    break;
                case AddressingMode.Accumulator:
                    // address n/a
                    break;
                case AddressingMode.Immediate:
                    // address is immediate address following op code
                    address = immediateAddress;
                    break;
                case AddressingMode.Implied:
                    // address n/a
                    break;
                case AddressingMode.IndexedIndirect:
                    // indexed indirect is address located at the x register, offset by the byte immediately following the op code
                    address = memory.Read16Bug((UInt16)(memory.Read(immediateAddress) + State.RegisterX));
                    break;
                case AddressingMode.Indirect:
                    // indirect address is the address located at the absolute address (with 6502 addressing bug)
                    address = memory.Read16Bug(memory.Read16(immediateAddress));
                    break;
                case AddressingMode.IndirectIndexed:
                    // indirect indexed address is the address located at the byte address immediately after the op code, then offset by the Y register
                    address = (UInt16)(memory.Read16Bug((memory.Read(immediateAddress))) + State.RegisterY);
                    pageCrossed = memory.PagesDiffer((UInt16)(address - State.RegisterY), address);
                    break;
                case AddressingMode.Relative:
                    // address is relative signed byte offset following op code, applied at the end of the instruction
                    byte offset = memory.Read(immediateAddress);
                    address = (UInt16)(State.ProgramCounter + 2 + offset);
                    if (offset >= 0x80)
                        address -= 0x100;
                    break;
                case AddressingMode.ZeroPage:
                    // address is absolute byte address within 0th page
                    address = memory.Read(immediateAddress);
                    break;
                case AddressingMode.ZeroPageX:
                    // address is absolute byte address within 0th page, offset by x register
                    address = (UInt16)(memory.Read(immediateAddress) + State.RegisterX);
                    break;
                case AddressingMode.ZeroPageY:
                    // address is absolute byte address within 0th page, offset by y register
                    address = (UInt16)(memory.Read(immediateAddress) + State.RegisterY);
                    break;
            }

            return address;
        }


    }
}
