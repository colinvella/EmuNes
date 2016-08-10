using NesCore;
using NesCore.Processor;
using NesCore.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes.Diagnostics
{
    public partial class CodeDisassemblyForm : Form
    {
        public CodeDisassemblyForm(NesCore.Console console)
        {
            InitializeComponent();

            this.console = console;
            this.processor = console.Processor;

            disassemblyLines = new DisassemblyLine[0x10000];
            disassemblyLineSource = new string[0x10000];
            addressLabels = new string[0x10000];
            queuedAddresses = new bool[0x10000];
            lastRefresh = DateTime.Now;
            needsRefresh = true;

            this.disassemblyQueue = new Queue<ushort>();

#if DEBUG
            disassemblyTimer.Enabled = false;
#endif
        }

        public void Trace()
        {
            ushort address = console.Processor.State.ProgramCounter;

            if (disassemblyLines[address] == null && !queuedAddresses[address])
            {
                disassemblyQueue.Enqueue(address);
                queuedAddresses[address] = true;
            }

#if DEBUG
            DisassembleLine(address);
            Debug.WriteLine(disassemblyLines[address]);
#endif

            lastTrace = DateTime.Now;
        }

        private void OnDisassemblyTick(object sender, EventArgs e)
        {
            int linesToDisassemble = Math.Min(10, disassemblyQueue.Count);
            if (linesToDisassemble-- > 0)
            {
                ushort address = disassemblyQueue.Dequeue();
                queuedAddresses[address] = false;
                DisassembleLine(address);
            }

            if (needsRefresh && (DateTime.Now - lastRefresh).TotalSeconds > 5)
            {
                lastRefresh = DateTime.Now;

                SuspendRedraw(disassemblyRichTextBox);

                disassemblyRichTextBox.Clear();
                for (int address = 0; address < 0x10000; address++)
                {
                    /*
                    if (queuedAddresses[address])
                    {
                        disassemblyRichTextBox.SelectionColor = Color.Gray;
                        disassemblyRichTextBox.AppendText("Queued for disassembly...\r\n");
                        continue;
                    }*/

                    DisassemblyLine disassemblyLine = disassemblyLines[address];
                    if (disassemblyLine == null)
                        continue;

                    string label = disassemblyLine.Label;
                    if (label != null)
                    {
                        disassemblyRichTextBox.SelectionColor = Color.Green;
                        disassemblyRichTextBox.AppendText((label + ":").PadRight(16));
                    }
                    else
                        disassemblyRichTextBox.AppendText("".PadRight(16));
                    disassemblyRichTextBox.SelectionColor = Color.DarkRed;
                    disassemblyRichTextBox.AppendText(disassemblyLine.Address + " ");
                    disassemblyRichTextBox.SelectionColor = Color.DarkBlue;
                    disassemblyRichTextBox.AppendText(disassemblyLine.MachineCode.PadRight(12));
                    disassemblyRichTextBox.SelectionColor = Color.Black;
                    disassemblyRichTextBox.AppendText(disassemblyLine.Source.PadRight(14));
                    string remarks = disassemblyLine.Remarks;

                    if (remarks != null)
                    {
                        disassemblyRichTextBox.SelectionColor = Color.Gray;
                        disassemblyRichTextBox.AppendText("; " + remarks);
                    }
                    disassemblyRichTextBox.AppendText("\r\n");
                }

                ResumeRedraw(disassemblyRichTextBox);

                needsRefresh = false;                
            }

        }

        private void SuspendRedraw(Control control)
        {
            SendMessage(control.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            OldEventMask = (IntPtr)SendMessage(control.Handle, EM_SETEVENTMASK, IntPtr.Zero, IntPtr.Zero);
        }

        private void ResumeRedraw(Control control)
        {
            SendMessage(control.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            SendMessage(control.Handle, EM_SETEVENTMASK, IntPtr.Zero, OldEventMask);
        }

        private void DisassembleLine(ushort address)
        {
            if (disassemblyLines[address] != null)
                return;

            // prepare main labels
            ushort resetVector = processor.ReadWord(Mos6502.ResetVector);
            addressLabels[resetVector] = "Start";
            ushort irqVector = processor.ReadWord(Mos6502.IrqVector);
            addressLabels[irqVector] = "IrqHandler";
            ushort nmiVector = processor.ReadWord(Mos6502.NmiVector);
            addressLabels[nmiVector] = "NmiHandler";

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
                disassemblyLine.Remarks = "- end of subroutine ---------";
            if (instruction.Name == "RTI")
                disassemblyLine.Remarks = "- end of interrupt handler --";

            // if label assigned by forward branching or jumping, assign to new disassembled line
            if (addressLabels[address] != null)
                disassemblyLine.Label = addressLabels[address];

            // determine labels for relative branching
            if (instruction.AddressingMode == AddressingMode.Relative)
            {
                ushort nextInstructionAddress = (ushort)(address + 2);
                sbyte offset = (sbyte) processor.ReadByte(operandAddress);
                ushort branchAddress = (ushort)(nextInstructionAddress + offset);
                string addressLabel = GetAddressLabel("RelBr", branchAddress);
                disassemblyLine.Remarks = "branch to " + addressLabel;
            }

            // determine labels for absolute jumps
            if (instruction.AddressingMode == AddressingMode.Absolute)
            {
                ushort destAddress = processor.ReadWord(operandAddress);
                if (instruction.Name == "JMP")
                {
                    string addressLabel = GetAddressLabel("AbsBr", destAddress);
                    disassemblyLine.Remarks = "jump to " + addressLabel;
                }
                else if (instruction.Name == "JSR")
                {
                    string addressLabel = GetAddressLabel("SubRt", destAddress);
                    disassemblyLine.Remarks = "call subroutine at " + addressLabel;
                }
            }

            disassemblyLines[address] = disassemblyLine;
            disassemblyLineSource[address] = disassemblyLine.ToString();
            needsRefresh = true;
        }

        public void InvalidateMemoryRange(ushort address, ushort size)
        {
            int endExclusive = address + size;

            for (int index = address; index < endExclusive; index++)
            {
                disassemblyLines[address] = null;
                disassemblyLineSource[address] = null;
                addressLabels[address] = null;
            }

            needsRefresh = true;
        }

        private string GetAddressLabel(string labelType, ushort address)
        {
            String addressLabel = addressLabels[address];
            if (addressLabel != null)
                return addressLabels[address];

            addressLabel = labelType + Hex.Format(address).Replace("$", "");
            addressLabels[address] = addressLabel;

            // label destination if already disassembled (back reference)
            if (disassemblyLines[address] != null)
                disassemblyLines[address].Label = addressLabel;
            return addressLabel;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
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
        private DisassemblyLine[] disassemblyLines;
        private string[] disassemblyLineSource;
        private string[] addressLabels;
        private bool[] queuedAddresses;
        private DateTime lastRefresh, lastTrace;
        private bool needsRefresh;

        private Queue<ushort> disassemblyQueue;

        private const int WM_USER = 0x0400;
        private const int EM_SETEVENTMASK = (WM_USER + 69);
        private const int WM_SETREDRAW = 0x0b;
        private IntPtr OldEventMask;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
