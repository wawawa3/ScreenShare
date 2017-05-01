using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenCapture
{
    class ActionMonitor
    {
        public bool IsWaiting { get; private set; }
        public bool Enable { get; private set; }

        private int m_locking = 0;

        public void DoWork(Action action)
        {
            if (IsWaiting) return;

            Interlocked.Increment(ref m_locking);

            action.Invoke();

            Interlocked.Decrement(ref m_locking);
        }

        public async Task<bool> WaitAllAsync(long timeoutMs = 5000)
        {
            IsWaiting = true;

            var s = DateTime.Now.Millisecond;

            var res = await Task.Run<bool>(() =>
            {
                while (m_locking != 0)
                {
                    if (DateTime.Now.Millisecond - s >= timeoutMs)
                        return false;
                }

                return true;
            });

            IsWaiting = false;

            return res;
        }


    }
}
