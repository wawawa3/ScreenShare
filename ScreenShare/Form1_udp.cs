using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

using System.Runtime.InteropServices;


namespace ScreenCapture
{
    public partial class Form1_udp : Form
    {
        Timer timer = new Timer();
        Capture capture = new Capture();
        UdpClient udp = new UdpClient();

        public Form1_udp()
        {
            InitializeComponent();

            timer.Tick += (s, ea) => 
            {
                Bitmap bmp = capture.GetScreenCapture();

                pictureBox.Image = bmp;
                    
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                var length = bitmapData.Stride * bitmapData.Height;

                byte[] bytes = new byte[length];

                Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
                bmp.UnlockBits(bitmapData);

                sendBytes(bytes);
            };
            timer.Interval = 2000;
        }

        private async void sendBytes(byte[] bytes)
        {
            var list = new List<byte>(bytes);
            var len = bytes.Length / 10;
            var index = 0;

            for (int i = 0; i < 10; i++, index += len)
            {
                var range = list.GetRange(index, len);

                await Task.Run(() => udp.Send(range.ToArray(), range.Count));
            }
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            button_connect.Enabled = false;
            textBox_ip.Enabled = false;
            textBox_port.Enabled = false;

            button_startCapture.Enabled = true;

            udp = new UdpClient(textBox_ip.Text, Convert.ToInt32(textBox_port.Text));
        }

        private void button_startCapture_Click(object sender, EventArgs e)
        {
            button_startCapture.Enabled = false;
            button_stopCapture.Enabled = true;

            timer.Start();

            Task.Run(() =>
            {
                IPEndPoint remoteEP = null;
                try
                {
                    byte[] recv = udp.Receive(ref remoteEP);
                    Console.WriteLine(recv.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            });
        }

        private void button_stopCapture_Click(object sender, EventArgs e)
        {
            button_startCapture.Enabled = true;
            button_stopCapture.Enabled = false;

            timer.Stop();
        }

        
    }
}
