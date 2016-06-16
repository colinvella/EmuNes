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
        public CodeDisassemblyForm()
        {
            InitializeComponent();            
        }

        public void Trace(NesCore.Console console)
        {
            ushort address = console.Processor.State.ProgramCounter;
            byte opCode = console.Memory[address];

            DisassemblyLine disassemblyLine = new DisassemblyLine();
            disassemblyLine.Address = Hex.Format(address);
            disassemblyLine.OpCode = opCode;

            Instruction instruction = console.Processor.InstructionSet[opCode];
            disassemblyLine.Instruction = instruction.Name;


            if (!disassemblyLineMap.ContainsKey(address))
            {
                disassemblyLineMap[address] = disassemblyLine;
                disassemblyLines.Add(disassemblyLine);                
                needsRefresh = true;
            }
            else
            {
                if (needsRefresh && (DateTime.Now - lastRefresh).TotalSeconds > 5)
                {
                    lastRefresh = DateTime.Now;
                    BeginInvoke((new Action(() => dataGridView.DataSource = new SortableBindingList<DisassemblyLine>(disassemblyLines.OrderBy((x) => x.Address)))));
                    needsRefresh = false;
                }
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            disassemblyLineMap = new Dictionary<ushort, DisassemblyLine>();
            disassemblyLines = new List<DisassemblyLine>();
            SortableBindingList<DisassemblyLine> disassemblyBindingList = new SortableBindingList<DisassemblyLine>(disassemblyLines);

            dataGridView.AutoGenerateColumns = true;
            dataGridView.DataSource = disassemblyBindingList;
            dataGridView.Sort(dataGridView.Columns[0], ListSortDirection.Ascending);

            lastRefresh = DateTime.Now;
            needsRefresh = true;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs formClosingEventArgs)
        {
            formClosingEventArgs.Cancel = true;
        }

        private Dictionary<ushort, DisassemblyLine> disassemblyLineMap;
        private List<DisassemblyLine> disassemblyLines;
        private DateTime lastRefresh;
        private bool needsRefresh;
    }
}
