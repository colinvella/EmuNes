using NesCore;
using NesCore.Processor;
using NesCore.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmuNES.Diagnostics
{
    public partial class CodeDisassemblyForm : Form
    {
        public CodeDisassemblyForm(NesCore.Console console)
        {
            InitializeComponent();

            this.console = console;
            this.processor = console.Processor;           
        }

        public void Trace()
        {
            ushort address = console.Processor.State.ProgramCounter;

            if (disassemblyLines[address] == null)
            {
                byte opCode = console.Memory[address];

                DisassemblyLine disassemblyLine = new DisassemblyLine();
                disassemblyLine.Address = Hex.Format(address);

                // determine instruction meta data
                Instruction instruction = console.Processor.InstructionSet[opCode];

                // machine code
                ushort operandAddress = (ushort)(address + 1);
                if (instruction.Size == 1)
                    disassemblyLine.MachineCode = Hex.Format(opCode);
                else if (instruction.Size == 2)
                    disassemblyLine.MachineCode = Hex.Format(opCode)
                        + " " + Hex.Format(processor.ReadByte(operandAddress));
                else // 3
                    disassemblyLine.MachineCode = Hex.Format(opCode)
                        + " " + Hex.Format(processor.ReadByte(operandAddress))
                        + " " + Hex.Format(processor.ReadByte((ushort)(operandAddress + 1)));

                // format instruction source
                disassemblyLine.Source
                    = instruction.Name + " " + FormatOperand(operandAddress, instruction.AddressingMode);

                // remarks
                if (instruction.Name == "RTS")
                    disassemblyLine.Remarks = "----------------";

                // determine labels when applicable
                if (instruction.AddressingMode == AddressingMode.Relative)
                {
                    ushort nextInstructionAddress = (ushort)(address + 2);
                    sbyte offset = (sbyte) processor.ReadByte(operandAddress);
                    ushort branchAddress = (ushort)(nextInstructionAddress + offset);
                    string addressLabel = null;
                    if (addressLabels.ContainsKey(branchAddress))
                    {
                        addressLabel = addressLabels[branchAddress] + ":";
                    }
                    else
                    {
                        addressLabel = "Label" + Hex.Format(branchAddress).Replace("$", "");
                        addressLabels[branchAddress] = addressLabel;
                        if (disassemblyLines[branchAddress] != null)
                            disassemblyLines[branchAddress].Label = addressLabel;
                    }
                    disassemblyLine.Remarks = "branch to " + addressLabel;
                }

                disassemblyLines[address] = disassemblyLine;  
                needsRefresh = true;
            }
            else
            {
                if (needsRefresh && (DateTime.Now - lastRefresh).TotalSeconds > 2)
                {
                    lastRefresh = DateTime.Now;
                    activeLines = disassemblyLines.Where((x) => x != null).ToArray();
                    dataGridView.DataSource = activeLines;
                    needsRefresh = false;
                }
            }
        }

        public void InvalidateMemoryRange(ushort address, ushort size)
        {
            int endExclusive = address + size;

            for (int index = address; index < endExclusive; index++)
                disassemblyLines[address] = null;

            var labelAddressesToRemove = addressLabels.Keys.Where((x) => x >= address && x < endExclusive ).ToArray();
            foreach (ushort addressToRemove in labelAddressesToRemove)
                addressLabels.Remove(addressToRemove);

            needsRefresh = true;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            disassemblyLines = new DisassemblyLine[0x10000];
            activeLines = new DisassemblyLine[0];
            addressLabels = new Dictionary<ushort, string>();

            dataGridView.AutoGenerateColumns = true;

            lastRefresh = DateTime.Now;
            needsRefresh = true;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs formClosingEventArgs)
        {
            formClosingEventArgs.Cancel = true;
        }

        private string FormatOperand(ushort operandAddress, AddressingMode addressingMode)
        {
            switch (addressingMode)
            {
                case AddressingMode.Absolute: return Hex.Format(processor.ReadWord(operandAddress));
                case AddressingMode.AbsoluteX: return Hex.Format(processor.ReadWord(operandAddress)) + ",X";
                case AddressingMode.AbsoluteY: return Hex.Format(processor.ReadWord(operandAddress)) + ",Y";
                case AddressingMode.Accumulator: return "A";
                case AddressingMode.Immediate: return "#" + Hex.Format(processor.ReadByte(operandAddress));
                case AddressingMode.Implied: return "";
                case AddressingMode.IndexedIndirect: return "(" + Hex.Format(processor.ReadByte(operandAddress)) + ",X)";
                case AddressingMode.Indirect: return "(" + Hex.Format(processor.ReadWord(operandAddress)) + ")";
                case AddressingMode.IndirectIndexed: return "(" + Hex.Format(processor.ReadByte(operandAddress)) + "), Y";
                case AddressingMode.Relative:
                case AddressingMode.ZeroPage:
                    return Hex.Format(processor.ReadByte(operandAddress));
                case AddressingMode.ZeroPageX: return Hex.Format(processor.ReadByte(operandAddress)) + ",X";
                case AddressingMode.ZeroPageY: return Hex.Format(processor.ReadByte(operandAddress)) + ",Y";
                default: return "";
            }
        }

        private NesCore.Console console;
        private NesCore.Processor.Mos6502 processor;
        private DisassemblyLine[] disassemblyLines, activeLines;
        private Dictionary<ushort, string> addressLabels;
        private DateTime lastRefresh;
        private bool needsRefresh;
    }
}
