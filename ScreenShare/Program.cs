using System;
using System.Windows.Forms;

using System.Diagnostics;

namespace ScreenShare
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if BROADCAST_IMAGE
            Application.Run(new Form_ImageCast());
#else
            Application.Run(new Form_tcp());
#endif
        }
    }
}
