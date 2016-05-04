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

        public const UInt16 NmiVector = 0xFFFA;
        public const UInt16 ResetVector = 0xFFFC;
        public const UInt16 IrqVector = 0xFFFE;


        public Processor(SystemBus systemBus)
        {
            SystemBus = systemBus;
            State = new State();
            InstructionSet = new InstructionSet(this);
        }

        public SystemBus SystemBus { get; private set; }
        public State State { get; private set; }
        public InstructionSet InstructionSet { get; private set; }

        // resets the processor state
        public void Reset()
        {
            State.ProgramCounter = ResetVector;
            State.StackPointer = StackBase;
            State.InterruptDisableFlag = true;
        }

        public UInt64 ExecuteInstructions(UInt16 count)
        {
            UInt64 consumedCycles = 0;
            while (count-- > 0)
                consumedCycles += ExecuteInstruction();
            return consumedCycles;
        }

        public UInt64 ExecuteUntilBreak()
        {
            UInt64 consumedCycles = 0;
            while (true)
            {
                bool breakReached = SystemBus.Read(State.ProgramCounter) == 0x00;
                consumedCycles += ExecuteInstruction();
                if (breakReached)
                    break;
            }
            return consumedCycles;
        }

        public UInt64 ExecuteInstruction()
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
                    HandleInterrupt(NmiVector);
                else
                    HandleInterrupt(IrqVector);
                State.InterruptType = InterruptType.None;
            }

            // keep track of current cycle
            UInt64 cycles = State.Cycles;

            // read next op code
            byte opCode = SystemBus.Read(State.ProgramCounter);

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
            instruction.Exceute(address);

            // determine and return cycles consumed by this instruction
            UInt64 consumedCycles = State.Cycles - cycles;
            return consumedCycles;
        }
        
        // push byte onto stack
        public void Push(byte value)
        {
            // stack is located in address range 0x100 to 0x1FF and initially set to 0x1FF
            SystemBus.Write((UInt16)(0x100 | State.StackPointer), value);
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
            return SystemBus.Read((UInt16)(0x100 | State.StackPointer));
        }

        // pull word from stack
        public UInt16 Pull16()
        {
            // pull lo, then hi, then combine into word
            byte lo = Pull();
            byte hi = Pull();
            return (UInt16)((hi << 8) | lo);
        }

        public void HandleInterrupt(UInt16 interruptVector)
        {
            Push16(State.ProgramCounter);
            InstructionSet.PushProcessorStatus(0x0000);
            State.ProgramCounter = Read16(interruptVector);
            State.InterruptDisableFlag = true;
            State.Cycles += 7;
        }
        
        private UInt16 GetAddressOperand(AddressingMode addressingMode, out bool pageCrossed)
        {
            UInt16 address = 0;
            pageCrossed = false;
            // immediate address is word address after op code
            UInt16 immediateAddress = (UInt16)(State.ProgramCounter + 1);

            switch (addressingMode)
            {
                case AddressingMode.Absolute:
                    // absolute address is the word located at immediate address
                    address = Read16(immediateAddress);
                    break;
                case AddressingMode.AbsoluteX:
                    // absolute x address is the absolute address offset by the X register
                    address = (UInt16)(Read16(immediateAddress) + State.RegisterX);
                    pageCrossed = PagesDiffer((UInt16)(address - State.RegisterX), address);
                    break;
                case AddressingMode.AbsoluteY:
                    // absolute y address is the absolute address offset by the Y register
                    address = (UInt16)(Read16(immediateAddress) + State.RegisterY);
                    pageCrossed = PagesDiffer((UInt16)(address - State.RegisterY), address);
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
                    address = Read16Bug((UInt16)(SystemBus.Read(immediateAddress) + State.RegisterX));
                    break;
                case AddressingMode.Indirect:
                    // indirect address is the address located at the absolute address (with 6502 addressing bug)
                    address = Read16Bug(Read16(immediateAddress));
                    break;
                case AddressingMode.IndirectIndexed:
                    // indirect indexed address is the address located at the byte address immediately after the op code, then offset by the Y register
                    address = (UInt16)(Read16Bug((SystemBus.Read(immediateAddress))) + State.RegisterY);
                    pageCrossed = PagesDiffer((UInt16)(address - State.RegisterY), address);
                    break;
                case AddressingMode.Relative:
                    // address is relative signed byte offset following op code, applied at the end of the instruction
                    byte offset = SystemBus.Read(immediateAddress);
                    address = (UInt16)(State.ProgramCounter + 2 + offset);
                    if (offset >= 0x80)
                        address -= 0x100;
                    break;
                case AddressingMode.ZeroPage:
                    // address is absolute byte address within 0th page
                    address = SystemBus.Read(immediateAddress);
                    break;
                case AddressingMode.ZeroPageX:
                    // address is absolute byte address within 0th page, offset by x register
                    address = (UInt16)(SystemBus.Read(immediateAddress) + State.RegisterX);
                    break;
                case AddressingMode.ZeroPageY:
                    // address is absolute byte address within 0th page, offset by y register
                    address = (UInt16)(SystemBus.Read(immediateAddress) + State.RegisterY);
                    break;
            }

            return address;
        }

        // returns true if address pages differ (differ by high byte)
        public bool PagesDiffer(UInt16 addressOne, UInt16 addressTwo)
        {
            return (addressOne & 0xFF00) != (addressTwo & 0xFF00);
        }

        // reads 16-bit value from the system bus in little-endian order
        public UInt16 Read16(UInt16 address)
        {
            byte valueLoByte = SystemBus.Read(address++);
            byte valueHiByte = SystemBus.Read(address);
            return (UInt16)(valueHiByte << 8 | valueLoByte);
        }

        public void Write16(UInt16 address, UInt16 value)
        {
            SystemBus.Write(address++, (byte)(value & 0xFF));
            SystemBus.Write(address, (byte)(value >> 8));
        }

        // reads 16-bit value from the system bus in little-endian order
        // but emulates a 6502 bug that caused the low byte to wrap without
        // incrementing the high byte
        public UInt16 Read16Bug(UInt16 address)
        {
            byte addressHiByte = (byte)(address >> 8);
            byte addressLoByte = (byte)(address & 0xFF);
            ++addressLoByte;
            UInt16 nextAddress = (UInt16)(addressHiByte << 8 | addressLoByte);

            byte valueLoByte = SystemBus.Read(address);
            byte valueHiByte = SystemBus.Read(nextAddress);
            return (UInt16)(valueHiByte << 8 | valueLoByte);
        }
    }
}
