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

            disassemblyLines = new DisassemblyLine[0x10000];
            activeLines = new DisassemblyLine[0];
            addressLabels = new string[0x10000];
            queuedAddresses = new bool[0x10000];
            lastRefresh = DateTime.Now;
            needsRefresh = true;

            this.disassemblyQueue = new Queue<ushort>();
        }

        public void Trace()
        {
            ushort address = console.Processor.State.ProgramCounter;

            if (disassemblyLines[address] == null && !queuedAddresses[address])
            {
                disassemblyQueue.Enqueue(address);
                queuedAddresses[address] = true;
            }
            lastTrace = DateTime.Now;
        }

        private void OnDisassemblyTick(object sender, EventArgs e)
        {
            if (disassemblyQueue.Count > 0)
            {
                ushort address = disassemblyQueue.Dequeue();
                queuedAddresses[address] = false;
                DisassembleLine(address);
            }

            if (needsRefresh && (DateTime.Now - lastRefresh).TotalSeconds > 5)
            {
                    lastRefresh = DateTime.Now;
                    activeLines = disassemblyLines.Where((x) => x != null).ToArray();
                    dataGridView.DataSource = activeLines;
                    needsRefresh = false;                
            }

            if ((DateTime.Now - lastTrace).TotalSeconds > 1 && dataGridView.SelectedRows.Count == 0)
            {
                string programCounterHex = Hex.Format(processor.State.ProgramCounter);
                for (int i = 0; i < activeLines.Length; i++)
                if (activeLines[i].Address == programCounterHex)
                {
                    dataGridView.Rows[i].Selected = true;
                    dataGridView.CurrentCell = dataGridView.Rows[i].Cells[0];
                    break;
                }
            }
        }

        private void DisassembleLine(ushort address)
        {
            if (disassemblyLines[address] == null)
            {
                DisassemblyLine disassemblyLine = new DisassemblyLine();
                disassemblyLine.Address = Hex.Format(address);

                byte opCode = console.Memory[address];

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

                // if label assigned by forward branching or jumping, assign to new disassembled line
                if (addressLabels[address] != null)
                    disassemblyLine.Label = addressLabels[address] + ":";

                // determine labels for relative branching
                if (instruction.AddressingMode == AddressingMode.Relative)
                {
                    ushort nextInstructionAddress = (ushort)(address + 2);
                    sbyte offset = (sbyte) processor.ReadByte(operandAddress);
                    ushort branchAddress = (ushort)(nextInstructionAddress + offset);
                    string addressLabel = GetAddressLabel(branchAddress);
                    disassemblyLine.Remarks = "branch to " + addressLabel;
                }

                disassemblyLines[address] = disassemblyLine;  
                needsRefresh = true;
            }
            
        }

        public void InvalidateMemoryRange(ushort address, ushort size)
        {
            int endExclusive = address + size;

            for (int index = address; index < endExclusive; index++)
            {
                disassemblyLines[address] = null;
                addressLabels[address] = null;
            }

            needsRefresh = true;
        }

        private string GetAddressLabel(ushort address)
        {
            String addressLabel = addressLabels[address];
            if (addressLabel != null)
                return addressLabels[address];

            addressLabel = "Label" + Hex.Format(address).Replace("$", "");
            addressLabels[address] = addressLabel;

            // label destination if already disassembled (back reference)
            if (disassemblyLines[address] != null)
                disassemblyLines[address].Label = addressLabel + ":";
            return addressLabel;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            dataGridView.AutoGenerateColumns = true;
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
        private string[] addressLabels;
        private bool[] queuedAddresses;
        private DateTime lastRefresh, lastTrace;
        private bool needsRefresh;

        private Queue<ushort> disassemblyQueue;
    }
}
