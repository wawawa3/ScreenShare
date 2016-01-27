using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;

namespace ScreenShare
{
    static class Program
    {
        private static readonly string Batch_FirewallAdd = "firewall_add.bat";
        private static readonly string Batch_FirewallDelete = "firewall_delete.bat";

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                var p = Process.Start(Batch_FirewallAdd);
                p.WaitForExit();
            }
            catch (Exception ex) 
            {
                Debug.Log(ex.Message);
            }

            Application.ApplicationExit += (s, e) =>
            {
                try
                {
                    var p = Process.Start(Batch_FirewallDelete);
                    p.WaitForExit();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            };
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form_tcp());
        }
    }
}
