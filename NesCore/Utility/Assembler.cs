using NesCore.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NesCore.Utility
{
    public class Assembler
    {
        public Assembler(Processor processor)
        {
            Processor = processor;
            WriteAddress = 0x0000;
            labelToAddress = new Dictionary<string, ushort>();
            unresolvedLabelOperands = new Dictionary<string, ushort>();
        }

        public Processor Processor { get; private set; }
        public UInt16 WriteAddress { get; set; }

        public void GenerateProgram(UInt16 startAddress, string programSource)
        {
            WriteAddress = startAddress;
            GenerateProgram(programSource);
        }

        public void GenerateProgram(string source)
        {
            labelToAddress.Clear();
            unresolvedLabelOperands.Clear();

            string[] sourceLines = source.Split(new char[] { '\r', '\n' });
            UInt16 sourceLineNumber = 0;
            foreach (string sourceLine in sourceLines)
                GenerateInstruction(sourceLine, sourceLineNumber++);

            //TODO: handle unresolved labels
        }

        private void GenerateInstruction(string sourceLine, UInt16 sourceLineNumber = 0)
        {
            // trim from spaces
            sourceLine = sourceLine.Trim();

            // ignore if empty line
            if (sourceLine.Length == 0)
                return;

            // strip comments
            sourceLine = sourceLine.Split(new char[] { ';' })[0];

            // trim again due to possible comment padding
            sourceLine = sourceLine.Trim();

            // ignore if just a comment
            if (sourceLine.Length == 0)
                return;

            // check if label
            if (sourceLine.EndsWith(":"))
            {
                string label = sourceLine.Remove(sourceLine.Length - 1);
                if (!IsLabel(label))
                    throw new AssemblerException(sourceLineNumber, sourceLine, "Invalid label: " + label);

                // index label for reference by branch and jump instructions
                labelToAddress[label] = WriteAddress;
                return;
            }

            // split lines into tokens using whitespace as delimeters
            string[] tokens = sourceLine.Split(new char[] { ' ', '\t' });

            string opName = tokens[0].ToUpper();

            // basic operand name check
            if (Processor.InstructionSet.GetInstructionVariants(opName) == null)
                throw new AssemblerException(sourceLineNumber, sourceLine, "Undefined instruction: " + opName);

            SystemBus systemBus = Processor.SystemBus;

            if (tokens.Length == 1)
            {
                // implied or accumulator instruction
                Instruction instruction = Processor.InstructionSet.GetInstructionVariants(opName).First();
                // write implied or accumulator mode opcode
                systemBus.Write(WriteAddress++, instruction.Code);
                return;
            }
            else
            {
                // join remaining tokens together
                String operandToken = string.Join("", tokens.Skip(1));

                byte byteOperand = 0;
                UInt16 wordOperand = 0;
                // determine addressing mode from operand
                if (ParseImmediateOperand(operandToken, out byteOperand))
                {
                    // immediate mode instructions
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.Immediate);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support immediate mode");

                    // write opcode and immediate operand
                    systemBus.Write(WriteAddress++, instruction.Code);
                    systemBus.Write(WriteAddress++, byteOperand);
                }
                else if (ParseIndexedIndirectOperand(operandToken, out byteOperand))
                {
                    // indexed indirect mode instructions (izx)
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.IndexedIndirect);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support indexed indirect mode");

                    // write opcode and indirect operand
                    systemBus.Write(WriteAddress++, instruction.Code);
                    systemBus.Write(WriteAddress++, byteOperand);
                }
                else if (ParseIndirectOperand(operandToken, out wordOperand))
                {
                    // indirect mode instructions
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.Indirect);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support indirect mode");

                    // write opcode and indirect operand
                    systemBus.Write(WriteAddress++, instruction.Code);
                    Processor.Write16(WriteAddress, byteOperand);
                    WriteAddress += 2;
                }
                else if (ParseByteHexValue(operandToken, out byteOperand))
                {
                    // zero page or relative mode instruction
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.ZeroPage);
                    if (instruction == null)
                        instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.Relative);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support zero page or relative addressing modes");

                    // write opcode and zero page/relative operand
                    systemBus.Write(WriteAddress++, instruction.Code);
                    systemBus.Write(WriteAddress++, byteOperand);
                }
                else if (ParseLabel(operandToken, out wordOperand))
                {
                    // absolute address (JMP, JSR) or relative address (branch instructions)
                    Instruction instruction = Processor.InstructionSet.GetInstructionVariants(opName).First();

                    // write instruction
                    systemBus.Write(WriteAddress++, instruction.Code);

                    if (instruction.AddressingMode == AddressingMode.Absolute)
                    {
                        // operand is absolute address
                        Processor.Write16(WriteAddress, wordOperand);
                        WriteAddress += 2;
                    }
                    else if (instruction.AddressingMode == AddressingMode.Relative)
                    {
                        // compute relative byte offset from end of branch instruction
                        // to target label
                        int addressOffset = wordOperand - (WriteAddress + 1);

                        // validate range
                        if (addressOffset > 0xFF || addressOffset < -0x80)
                            throw new AssemblerException(sourceLineNumber, sourceLine, 
                                "Branch offset to label '" + operandToken + "' is out of range");

                        // write branch offset operand
                        byte branchOffset = (byte)addressOffset;
                        systemBus.Write(WriteAddress++, branchOffset);
                    }
                    else
                    {
                        throw new AssemblerException(sourceLineNumber, sourceLine, "Unrecognised addressing mode");
                    }
                }
            }
        }

        private bool IsLabel(string input)
        {
            Regex regex = new Regex(@"[\w_][\w\d_]*");
            return regex.Match(input) != null;
        }

        private bool ParseLabel(string operand, out UInt16 absoluteAddress)
        {
            absoluteAddress = 0;
            if (!IsLabel(operand))
                return false;
            if (labelToAddress.ContainsKey(operand))
            {
                absoluteAddress = labelToAddress[operand];
            }
            else
            {
                // keep track of instruction locations with unresolved label operands
                unresolvedLabelOperands[operand] = (UInt16)(WriteAddress);
            }
            return true;
        }

        private bool ParseAbsoluteOperand(string operand, out UInt16 value)
        {
            return ParseHexValue(operand, out value);
        }

        private bool ParseAbsoluteXOperand(string operand, out UInt16 value)
        {
            value = 0;
            operand = operand.ToUpper();
            if (!operand.EndsWith(",X"))
                return false;

            return ParseHexValue(operand.Replace(",X", ""), out value);
        }

        private bool ParseAbsoluteYOperand(string operand, out UInt16 value)
        {
            value = 0;
            operand = operand.ToUpper();
            if (!operand.EndsWith(",Y"))
                return false;

            return ParseHexValue(operand.Replace(",Y", ""), out value);
        }

        private bool ParseImmediateOperand(string operand, out byte value)
        {
            value = 0;
            if (!operand.StartsWith("#"))
                return false;
            return ParseByteHexValue(operand.Substring(1), out value);        
        }

        private bool ParseIndexedIndirectOperand(string operand, out byte value)
        {
            value = 0;
            if (!operand.StartsWith("("))
                return false;
            if (!operand.EndsWith(",X)"))
                return false;
            return ParseByteHexValue(operand.Substring(1, operand.Length - 4), out value);
        }

        private bool ParseIndirectOperand(string operand, out UInt16 value)
        {
            value = 0;
            if (!operand.StartsWith("("))
                return false;
            if (!operand.EndsWith(")"))
                return false;
            return ParseHexValue(operand.Substring(1, operand.Length - 2), out value);
        }

        private bool ParseZeroPageXOperand(string operand, out byte value)
        {
            value = 0;
            operand = operand.ToUpper();
            if (!operand.EndsWith(",X"))
                return false;
            return ParseByteHexValue(operand.Replace(",X", ""), out value);
        }

        private bool ParseZeroPageYOperand(string operand, out byte value)
        {
            value = 0;
            operand = operand.ToUpper();
            if (!operand.EndsWith(",Y"))
                return false;
            return ParseByteHexValue(operand.Replace(",Y", ""), out value);
        }

        private bool ParseZeroPageOperand(string operand, out byte value)
        {
            return ParseByteHexValue(operand, out value);
        }

        private bool ParseByteHexValue(string operand, out byte value)
        {
            value = 0;
            UInt16 wordValue = 0;
            if (!ParseHexValue(operand, out wordValue))
                return false;
            if (wordValue > 0xFF)
                return false;
            value = (byte)wordValue;
            return true;
        }

        private bool ParseHexValue(string operand, out UInt16 value)
        {
            value = 0;
            operand = operand.ToUpper();
            if (operand.Length < 2)
                return false;
            if (!operand.StartsWith("$"))
                return false;
            try
            {
                value = Convert.ToUInt16(operand.Substring(1), 16);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Dictionary<string, UInt16> labelToAddress;
        private Dictionary<string, UInt16> unresolvedLabelOperands;
    }
}
