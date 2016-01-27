using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
    /// <summary>
    /// キャプチャ領域のオーバーレイ表示用フォーム
    /// </summary>
    public partial class Form_OverRay : Form
    {
        Pen Pen = new Pen(Color.Red, 3f);

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020;
                return cp;
            }
        }

        Rectangle captureBounds = Screen.PrimaryScreen.Bounds;
        public Rectangle CaptureBounds 
        { 
            set
            {
                captureBounds = value;
                captureBounds.Inflate(new Size(2, 2));
            }
        }

        public Form_OverRay()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }

        public void ResetArea()
        {
            captureBounds = Screen.PrimaryScreen.Bounds;
            Refresh();
        }

        private void Form_OverRay_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pen, captureBounds);
        }

    }
}
