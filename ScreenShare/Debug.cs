using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using System.Timers;

namespace ScreenShare
{
    /// <summary>
    /// デバッグ時に実行する関数群
    /// </summary>
    class Debug
    {
        public static TextWriter Writer { get; set; }

        /// <summary>
        /// ログ
        /// </summary>
        /// <param name="log">文字列</param>
        //[Conditional("DEBUG")]
        public static void Log(string tag, string log)
        {
            //Console.WriteLine(log);
            if (Writer == null)
            {
                Writer = new StreamWriter("debug.log", true);
                Writer.WriteLine("------------------------------------s");
            }

            Writer.WriteLine(DateTime.Now + " : ["+ tag +"] "+ log);
        }

        public static void Terminate()
        {
            if (Writer != null)
            {
                Writer.Close();
            }
        }

        /// <summary>
        /// 処理の時間をログに出す
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        [Conditional("DEBUG")]
        public static void ShowElapsedMilliSecond(Action act, string tag)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            act.Invoke();
            sw.Stop();

            Debug.Log(tag, "" + sw.ElapsedMilliseconds);
        }
    }
}
