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

        /// <summary>
        /// Non-maskable interrupt vector
        /// </summary>
        public const ushort NmiVector = 0xFFFA;

        /// <summary>
        /// Reset vector
        /// </summary>
        public const ushort ResetVector = 0xFFFC;

        /// <summary>
        /// Interrupt request vector
        /// </summary>
        public const ushort IrqVector = 0xFFFE;
        
        public Mos6502()
        {
            State = new State();
            InstructionSet = new InstructionSet(this);
        }

        public State State { get; private set; }

        /// <summary>
        /// Hook for reading a byte from a given memory address
        /// </summary>
        public ReadByte ReadByte { get; set; }

        /// <summary>
        /// Hook for writing a byte to a given memory address
        /// </summary>
        public WriteByte WriteByte { get; set; }

        /// <summary>
        /// Action triggered when a KIL opcode is encountered
        /// </summary>
        public Action Lockup { get; set; }

        /// <summary>
        /// Action triggered prior to execution of every instruction
        /// </summary>
        public Action Trace { get; set; }

        /// <summary>
        /// Processor's instruction set
        /// </summary>
        public InstructionSet InstructionSet { get; private set; }

        /// <summary>
        /// resets the processor state
        /// </summary>
        public void Reset()
        {
            State.ProgramCounter = ReadWord(ResetVector);
            State.StackPointer = StackBase;
            State.InterruptDisableFlag = true;
        }

        /// <summary>
        /// Execute a given number of instructions
        /// </summary>
        /// <param name="count">number of instructions to execute</param>
        /// <returns>total cycles consumed by the instructions</returns>
        public UInt64 ExecuteInstructions(ushort count)
        {
            UInt64 consumedCycles = 0;
            while (count-- > 0)
                consumedCycles += ExecuteInstruction();
            return consumedCycles;
        }

        /// <summary>
        /// Execute until BRK opcode is executed
        /// </summary>
        /// <returns>total cycles consumed by the instructions</returns>
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

        /// <summary>
        /// Executes the next instruction referenced by the program counter
        /// </summary>
        /// <returns>Cycles consumed by the instruction</returns>
        public byte ExecuteInstruction()
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

            // run tracer if set
            Trace?.Invoke();

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
            // note: this takes care of fixed durations; variable durations e.g. for branching are computed within the instruction
            State.Cycles += instruction.Cycles;

            // consume an extra cycle if a page is crossed during addressing (unless it is an exception?)
            bool pageCrossException = 
                opCode == 0x1E ||
                opCode == 0x13 || opCode == 0x1B || opCode == 0x1F ||
                opCode == 0x33 || opCode == 0x3B || opCode == 0x3E || opCode == 0x3F ||
                opCode == 0x53 || opCode == 0x5B || opCode == 0x5E || opCode == 0x5F ||
                opCode == 0x73 || opCode == 0x7B || opCode == 0x7E || opCode == 0x7F ||
                opCode == 0x91 || opCode == 0x93 || opCode == 0x99 || opCode == 0x9B || opCode == 0x9C || opCode == 0x9D || opCode == 0x9E || opCode == 0x9F ||
                opCode == 0xD3 || opCode == 0xDB || opCode == 0xDE || opCode == 0xDF ||
                opCode == 0xF3 || opCode == 0xFB || opCode == 0xFE || opCode == 0xFF;

            if (pageCrossed && !pageCrossException)
                ++State.Cycles;

            // execute the instruction
            instruction.Exceute(address);

            // determine and return cycles consumed by this instruction
            byte consumedCycles = (byte)(State.Cycles - cycles);
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

        // causes a non-maskable interrupt to occur on the next cycle
        public void TriggerNonMaskableInterrupt()
        {
            State.InterruptType = InterruptType.NonMaskable;
        }

        // causes an IRQ interrupt to occur on the next cycle
        public void TriggerInterruptRequest()
        {
            if (!State.InterruptDisableFlag)
                State.InterruptType = InterruptType.Request;
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

        private void HandleInterrupt(ushort interruptVector)
        {
            PushWord(State.ProgramCounter);
            InstructionSet.PushProcessorStatus(0x0000);
            State.ProgramCounter = ReadWord(interruptVector);
            State.InterruptDisableFlag = true;
            State.Cycles += 7;
        }

    }
}
