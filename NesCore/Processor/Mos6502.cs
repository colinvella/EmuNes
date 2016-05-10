using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Processor
{
    // delegate for reading a byte from a givne memory address
    public delegate byte ReadByte(ushort address);

    // delegate for writing a byte from a given memory address
    public delegate void WriteByte(ushort address, byte value);

    public class Mos6502
    {
        public const int Frequency = 1789773;
        public const byte StackBase = 0xFD;

        public const ushort NmiVector = 0xFFFA;
        public const ushort ResetVector = 0xFFFC;
        public const ushort IrqVector = 0xFFFE;


        public Mos6502()
        {
            State = new State();
            InstructionSet = new InstructionSet(this);
        }

        public State State { get; private set; }

        public ReadByte ReadByte { get; set; }
        public WriteByte WriteByte { get; set; }

        public InstructionSet InstructionSet { get; private set; }

        // resets the processor state
        public void Reset()
        {
            State.ProgramCounter = ResetVector;
            State.StackPointer = StackBase;
            State.InterruptDisableFlag = true;
        }

        public UInt64 ExecuteInstructions(ushort count)
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
                bool breakReached = ReadByte(State.ProgramCounter) == 0x00;
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
            byte opCode = ReadByte(State.ProgramCounter);

            // get corresponding instruction
            Instruction instruction = InstructionSet[opCode];

            // determine address operand and if a page is crossed
            bool pageCrossed = false;
            ushort address = instruction.Fetch((ushort)(State.ProgramCounter + 1), out pageCrossed);

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


        // reads 16-bit value from the system bus in little-endian order
        public ushort ReadWord(ushort address)
        {
            byte valueLoByte = ReadByte(address++);
            byte valueHiByte = ReadByte(address);
            return (ushort)(valueHiByte << 8 | valueLoByte);
        }

        // write 16-bit value to the system bus in little-endian order
        public void WriteWord(ushort address, ushort value)
        {
            WriteByte(address++, (byte)(value & 0xFF));
            WriteByte(address, (byte)(value >> 8));
        }

        // push byte onto stack
        public void PushByte(byte value)
        {
            // stack is located in address range 0x100 to 0x1FF and initially set to 0x1FF
            WriteByte((ushort)(0x100 | State.StackPointer), value);
            State.StackPointer--;
        }

        // push word onto stack
        public void PushWord(ushort value)
        {
            // push hi, then lo
            PushByte((byte)(value >> 8));
            PushByte((byte)(value & 0xFF));
        }

        // pull byte from stack
        public byte PullByte()
        {
            State.StackPointer++;
            return ReadByte((ushort)(0x100 | State.StackPointer));
        }

        // pull word from stack
        public ushort PullWord()
        {
            // pull lo, then hi, then combine into word
            byte lo = PullByte();
            byte hi = PullByte();
            return (ushort)((hi << 8) | lo);
        }

        public void HandleInterrupt(ushort interruptVector)
        {
            PushWord(State.ProgramCounter);
            InstructionSet.PushProcessorStatus(0x0000);
            State.ProgramCounter = ReadWord(interruptVector);
            State.InterruptDisableFlag = true;
            State.Cycles += 7;
        }
        
        // returns true if address pages differ (differ by high byte)
        public bool PagesDiffer(ushort addressOne, ushort addressTwo)
        {
            return (addressOne & 0xFF00) != (addressTwo & 0xFF00);
        }

        // reads 16-bit value from the system bus in little-endian order
        // but emulates a 6502 bug that caused the low byte to wrap without
        // incrementing the high byte
        public ushort ReadWordWrap(ushort address)
        {
            byte addressHiByte = (byte)(address >> 8);
            byte addressLoByte = (byte)(address & 0xFF);
            ++addressLoByte;
            ushort nextAddress = (ushort)(addressHiByte << 8 | addressLoByte);

            byte valueLoByte = ReadByte(address);
            byte valueHiByte = ReadByte(nextAddress);
            return (ushort)(valueHiByte << 8 | valueLoByte);
        }
    }
}
