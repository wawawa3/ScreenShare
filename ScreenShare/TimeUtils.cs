using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShare
{
    class TimeUtils_HMSM
    {
        public static int UTCMilliseconds
        {
            get
            {
                var ts = new TimeSpan(0, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
                //Console.WriteLine(ts.TotalMilliseconds + ":" + (int)ts.TotalMilliseconds);
                return (int)ts.TotalMilliseconds;
            }
        }

        public static int GetTotalMilliseconds(DateTime dt)
        {
            var ts = new TimeSpan(0, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
            //Console.WriteLine(ts.TotalMilliseconds + ":" + (int)ts.TotalMilliseconds);
            return (int)ts.TotalMilliseconds;
        }

        public static int GetTotalMilliseconds(TimeSpan day)
        {
            var dayts = TimeSpan.FromDays(day.Days);
            Console.WriteLine("dayts:" + dayts+":"+ day+":"+(day-dayts));
            //Console.WriteLine(ts.TotalMilliseconds + ":" + (int)ts.TotalMilliseconds);
            //return (int)(day - dayts).TotalMilliseconds;
            Console.WriteLine("ms:" + day.Milliseconds + "\nsc" + day.Seconds + "\nmn" + day.Minutes + "\nhr" + day.Hours);
            return day.Milliseconds + 1000 * (day.Seconds + 60 * (day.Minutes + 60 * day.Hours));
        }
    }
}
