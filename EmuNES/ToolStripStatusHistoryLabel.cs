using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes
{
    class ToolStripStatusHistoryLabel: ToolStripStatusLabel
    {
        public ToolStripStatusHistoryLabel()
            :base()
        {
            statusHistory = new List<string>();
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }

            set
            {
                if (value == base.Text)
                    return;
                base.Text = value;

                statusHistory.Add(value);
            }
        }

        protected override void OnClick(EventArgs eventArgs)
        {
            base.OnClick(eventArgs);
            MessageBox.Show(string.Join("\r", statusHistory));
        }

        private List<string> statusHistory;
    }
}
