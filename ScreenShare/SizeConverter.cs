using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShare
{
    class SizeConverter
    {
        private static char[] map = {'k', 'M'};
        public static int Parse(string size)
        {
            return Convert.ToInt32(size.Remove(size.Length-1)) * (1024 >> (10 * Array.IndexOf(map, size.Last())));
        }
    }
}
