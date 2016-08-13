using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes.Cheats
{
    class CheckedListBoxPlus: CheckedListBox
    {
        public CheckedListBoxPlus(): base()
        {
        }

        [Browsable(true)]
        [Category("Appearance")]
        public string EmptyMessage { get; set; }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
        }

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            base.OnPaint(paintEventArgs);

            if (Items.Count > 0)
                return;

            string message = EmptyMessage.Trim().Length == 0 ? "List is empty" : EmptyMessage;
            Rectangle rectangle = paintEventArgs.ClipRectangle;
            Graphics graphics = paintEventArgs.Graphics;
            SizeF textSize = graphics.MeasureString(message, this.Font);
            PointF messagePosition = new PointF(
                rectangle.Left + rectangle.Width * 0.5f - textSize.Width * 0.5f,
                rectangle.Top + rectangle.Height * 0.5f - textSize.Height * 0.5f);
            graphics.DrawString(message, this.Font, SystemBrushes.GrayText, messagePosition);
        }
    }
}
