using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
    public partial class Form_CaptureArea : Form
    {
        public delegate void AreaSelectedEventHandler(Rectangle rect);
        public event AreaSelectedEventHandler Selected;

        bool MouseDowned = false;
        Point DownPoint = new Point();
        Pen Pen = new Pen(Color.Red, 3f);

        public Form_CaptureArea()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }

        private void Form_CaptureArea_MouseDown(object sender, MouseEventArgs e)
        {
            DownPoint = e.Location;
            MouseDowned = true;
        }

        private void Form_CaptureArea_MouseUp(object sender, MouseEventArgs e)
        {
            if (!MouseDowned) return;

            this.Region = new Region(this.DisplayRectangle);

            MouseDowned = false;

            Selected(GetRect(e.Location));
        }

        private void Form_CaptureArea_MouseMove(object sender, MouseEventArgs e)
        {
            this.Refresh();
        }

        private Rectangle GetRect(Point p)
        {
            var x = Math.Min(DownPoint.X, p.X);
            var y = Math.Min(DownPoint.Y, p.Y);
            var width = Math.Abs(DownPoint.X - p.X);
            var height = Math.Abs(DownPoint.Y - p.Y);
            return new Rectangle(x, y, width, height);
        }

        private void Form_CaptureArea_Paint(object sender, PaintEventArgs e)
        {
            if (!MouseDowned) return;

            var rect = GetRect(System.Windows.Forms.Cursor.Position);

            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(this.DisplayRectangle);
            path.AddRectangle(rect);

            this.Region = new Region(path);

            rect.Inflate(new Size(2, 2));

            e.Graphics.Clear(SystemColors.Control);
            e.Graphics.DrawRectangle(Pen, rect);
        }
    }
}
