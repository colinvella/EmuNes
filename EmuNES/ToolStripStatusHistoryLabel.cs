using System;
using System.Collections.Generic;
using System.Drawing;
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

            if (dropDownForm == null)
                ConstructDropDownForm();

            // position above status label
            Rectangle statusStripRectangle = this.Parent.RectangleToScreen(this.ContentRectangle);
            int dropDownHeight = this.Height * 5;
            dropDownForm.Left = statusStripRectangle.Left;
            dropDownForm.Top = statusStripRectangle.Top - dropDownHeight;
            dropDownForm.Width = this.Width;
            dropDownForm.Height = dropDownHeight;

            statusTextBox.Text = string.Join("\r\n", statusHistory);
            dropDownForm.Show();
            statusTextBox.SelectionStart = statusTextBox.Text.Length;
            statusTextBox.SelectionLength = 0;
            statusTextBox.ScrollToCaret();
        }

        private void ConstructDropDownForm()
        {
            dropDownForm = new Form();
            dropDownForm.FormBorderStyle = FormBorderStyle.FixedSingle;
            dropDownForm.StartPosition = FormStartPosition.Manual;
            dropDownForm.ControlBox = false;

            statusTextBox = new TextBox();
            statusTextBox.ReadOnly = true;
            statusTextBox.Multiline = true;
            statusTextBox.ScrollBars = ScrollBars.Both;
            statusTextBox.Dock = DockStyle.Fill;

            statusTextBox.LostFocus += (sender, e) => dropDownForm.Hide();

            dropDownForm.Controls.Add(statusTextBox);
        }

        private Form dropDownForm;
        private TextBox statusTextBox;
        private List<string> statusHistory;
    }
}
