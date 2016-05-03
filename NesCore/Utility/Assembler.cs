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
            unresolvedLabelReferences = new Dictionary<string, List<LabelReference>>();
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
            unresolvedLabelReferences.Clear();

            string[] sourceLines = source.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            UInt16 sourceLineNumber = 0;
            foreach (string sourceLine in sourceLines)
                GenerateInstruction(sourceLine, sourceLineNumber++);

            ResolveLabelReferences();
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

                if (labelToAddress.ContainsKey(label))
                    throw new AssemblerException(sourceLineNumber, sourceLine, "Duplicate label not allowed");

                // index label for reference by branch and jump instructions
                labelToAddress[label] = WriteAddress;
                return;
            }

            // split lines into tokens using whitespace as delimeters
            string[] tokens = sourceLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            string opName = tokens[0].ToUpper();

            // basic operand name check
            if (Processor.InstructionSet.GetInstructionVariants(opName) == null)
                throw new AssemblerException(sourceLineNumber, sourceLine, "Undefined instruction: " + opName);

            SystemBus systemBus = Processor.SystemBus;

            if (tokens.Length == 1)
            {
                // implied instructions
                Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.Implied);
                if (instruction == null)
                    throw new AssemblerException(sourceLineNumber, sourceLine,
                        "Instruction " + opName + " does not support implied mode");
                // write implied mode opcode
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
                if (operandToken.ToUpper() == "A")
                {
                    // accumulator mode instructions
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.Accumulator);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support accumulator mode");
                    // accumulator is implied within op code
                    systemBus.Write(WriteAddress++, instruction.Code);
                }
                else if (ParseImmediateOperand(operandToken, out byteOperand))
                {
                    // immediate mode instructions
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.Immediate);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support immediate addressing mode");

                    // write opcode and immediate operand
                    systemBus.Write(WriteAddress++, instruction.Code);
                    systemBus.Write(WriteAddress++, byteOperand);
                }
                else if (ParseIndexedIndirectOperand(operandToken, out byteOperand)) // ($NN, X)
                {
                    // indexed indirect mode instructions (izx)
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.IndexedIndirect);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support indexed indirect addressing mode");

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
                            "Instruction " + opName + " does not support indirect (IZX) addressing mode");

                    // write opcode and indirect operand
                    systemBus.Write(WriteAddress++, instruction.Code);
                    Processor.Write16(WriteAddress, byteOperand);
                    WriteAddress += 2;
                }
                else if (ParseIndirectIndexedOperand(operandToken, out byteOperand)) // ($NN), Y
                {
                    // indirect indexed mode instructions (izy)
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.IndirectIndexed);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support indirect indexed (IZY) addressing mode");

                    // write opcode and indirect operand
                    systemBus.Write(WriteAddress++, instruction.Code);
                    systemBus.Write(WriteAddress++, byteOperand);
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
                else if (ParseZeroPageXOperand(operandToken, out byteOperand))
                {
                    // zero page x mode instruction
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.ZeroPageX);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support zero page X addressing mode");

                    // write opcode and zero page x operand
                    systemBus.Write(WriteAddress++, instruction.Code);
                    systemBus.Write(WriteAddress++, byteOperand);
                }
                else if (ParseZeroPageYOperand(operandToken, out byteOperand))
                {
                    // zero page x mode instruction
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.ZeroPageY);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support zero page Y addressing mode");

                    // write opcode and zero page y operand
                    systemBus.Write(WriteAddress++, instruction.Code);
                    systemBus.Write(WriteAddress++, byteOperand);
                }
                else if (ParseAbsoluteOperand(operandToken, out wordOperand))
                {
                    // absolute mode instructions
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.Absolute);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support absolute addressing mode");

                    // write instruction
                    systemBus.Write(WriteAddress++, instruction.Code);

                    // operand is absolute address
                    Processor.Write16(WriteAddress, wordOperand);
                    WriteAddress += 2;
                }
                else if (ParseLabelReference(sourceLineNumber, sourceLine, operandToken, out wordOperand))
                {
                    // absolute or relative address (jump, branch instructions)
                    //Instruction instruction = Processor.InstructionSet.GetInstructionVariants(opName).First();
                    Instruction instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.Absolute);
                    if (instruction == null)
                        instruction = Processor.InstructionSet.FindBy(opName, AddressingMode.Relative);
                    if (instruction == null)
                        throw new AssemblerException(sourceLineNumber, sourceLine,
                            "Instruction " + opName + " does not support absolute or relative addressing mode");
                    
                    // write instruction
                    systemBus.Write(WriteAddress++, instruction.Code);
                    if (instruction.AddressingMode == AddressingMode.Absolute)
                        WriteAddress += 2; // leave space for absolute address
                    else
                        WriteAddress += 1; // leave space for relative byte offset
                }
                else
                {
                    throw new AssemblerException(sourceLineNumber, sourceLine, "Unrecognised addressing mode");
                }
            }
        }

        private void ResolveLabelReferences()
        {
            foreach (string label in unresolvedLabelReferences.Keys)
            {
                if (!labelToAddress.ContainsKey(label))
                    throw new AssemblerException(0, "", "Unable to resolve label: " + label);
                UInt16 resolvedAddress = labelToAddress[label];
                foreach (LabelReference labelReference in unresolvedLabelReferences[label])
                {
                    // get opcode to determine if absolute or relative
                    UInt16 instructionAddress = labelReference.OperandAddress;
                    --instructionAddress;
                    byte opCode = Processor.SystemBus.Read(instructionAddress);
                    Instruction instruction = Processor.InstructionSet[opCode];
                    bool relative = instruction.AddressingMode == AddressingMode.Relative;

                    if (relative)
                    {
                        // compute relative byte offset
                        int addressOffset = resolvedAddress - (labelReference.OperandAddress + 1);

                        // validate range
                        if (addressOffset > 0x7F || addressOffset < -0x80)
                            throw new AssemblerException(labelReference.SourceLineNumber, labelReference.SourceLine,
                                "Branch offset to label '" + label + "' is out of range");

                        byte branchOffset = (byte)addressOffset;

                        // write branch offset
                        Processor.SystemBus.Write(labelReference.OperandAddress, branchOffset);
                    }
                    else
                    {
                        // write absolute address
                        Processor.Write16(labelReference.OperandAddress, resolvedAddress);
                    }
                }
            }
        }

        private bool IsLabel(string input)
        {
            Regex regex = new Regex(@"[\w_][\w\d_]*");
            return regex.Match(input) != null;
        }

        private bool ParseLabelReference(UInt16 sourceLineNumber, string sourceLine, string operand, out UInt16 absoluteAddress)
        {
            absoluteAddress = 0;
            if (!IsLabel(operand))
                return false;

            // keep track of locations with unresolved label operands
            List<LabelReference> labelReferences = null;
            if (unresolvedLabelReferences.ContainsKey(operand))
                labelReferences = unresolvedLabelReferences[operand];
            else
            {
                labelReferences = new List<LabelReference>();
                unresolvedLabelReferences[operand] = labelReferences;
            }

            labelReferences.Add(new LabelReference(sourceLineNumber, sourceLine, (UInt16)(WriteAddress + 1)));

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

        private bool ParseIndexedIndirectOperand(string operand, out byte value) // ($NN,X)
        {
            operand = operand.Replace(" ", "");
            value = 0;
            if (!operand.StartsWith("("))
                return false;
            if (!operand.EndsWith(",X)"))
                return false;
            return ParseByteHexValue(operand.Substring(1, operand.Length - 4), out value);
        }

        private bool ParseIndirectIndexedOperand(string operand, out byte value) // ($NN),Y
        {
            operand = operand.Replace(" ", "");
            value = 0;
            if (!operand.StartsWith("("))
                return false;
            if (!operand.EndsWith("),Y"))
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
        private Dictionary<string, List<LabelReference>> unresolvedLabelReferences;

        private class LabelReference
        {
            public LabelReference(UInt16 sourceLineNumber, string sourceLine, UInt16 operandAddress)
            {
                SourceLineNumber = sourceLineNumber;
                SourceLine = sourceLine;
                OperandAddress = operandAddress;
            }

            public UInt16 SourceLineNumber { get; set; }
            public string SourceLine { get; set; }
            public UInt16 OperandAddress { get; set; }
        }
    }
}
