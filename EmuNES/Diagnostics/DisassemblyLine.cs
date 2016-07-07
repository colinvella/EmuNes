using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Diagnostics
{
    class DisassemblyLine
    {
        public string Label { get; set; }

        public string Address { get; set; }

        [DisplayName("M Code")]
        public string MachineCode { get; set; }

        public string Source { get; set; }

        public string Remarks { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (Label != null)
            {
                stringBuilder.Append(Label);
                stringBuilder.Append(":\r\n");
            }
            stringBuilder.Append("    ");
            stringBuilder.Append(Address);
            stringBuilder.Append(" ");
            stringBuilder.Append(MachineCode.PadRight(12, ' '));
            stringBuilder.Append(" ");
            stringBuilder.Append(Source.PadRight(11, ' '));
            if (Remarks != null && Remarks.Trim() != "")
            {
                stringBuilder.Append(" ; ");
                stringBuilder.Append(Remarks);
            }

            return stringBuilder.ToString();
        }
    }
}
