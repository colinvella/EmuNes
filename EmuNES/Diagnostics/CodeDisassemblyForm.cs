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

                Instruction instruction = console.Processor.InstructionSet[opCode];
                disassemblyLine.Instruction 
                    = instruction.Name + " " + FormatOperand((ushort)(address + 1), instruction.AddressingMode);

                disassemblyLines[address] = disassemblyLine;  
                needsRefresh = true;
            }
            else
            {
                if (needsRefresh && (DateTime.Now - lastRefresh).TotalSeconds > 1)
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

            needsRefresh = true;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            disassemblyLines = new DisassemblyLine[0x10000];
            activeLines = new DisassemblyLine[0];

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
        private DateTime lastRefresh;
        private bool needsRefresh;
    }
}
