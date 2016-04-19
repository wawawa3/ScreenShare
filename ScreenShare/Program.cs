using System;
using System.Windows.Forms;

using System.Diagnostics;

namespace ScreenShare
{
    static class Program
    {
        private static readonly string Batch_FirewallAdd = "Batch\\firewall_add.bat";
        private static readonly string Batch_FirewallDelete = "Batch\\firewall_delete.bat";

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
                Debug.Log(ex.Message + Batch_FirewallAdd);
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

                Debug.Log("Application Exit");
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form_tcp());
        }
    }
}
