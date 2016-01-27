using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Runtime.InteropServices;

namespace ScreenShare
{
    class Utils
    {
        public static byte[] GetBytesFromStructure(object str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static byte[] GetBytesFromPtr(IntPtr ptr, long size)
        {
            byte[] buf = new byte[size];
            Marshal.Copy(ptr, buf, 0, buf.Length);
            return buf;
        }

        public static byte[] Concatenation(params byte[][] arrays)
        {
            int len = 0;
            foreach (var arr in arrays) 
                len += arr.Length;

            var concat = new byte[len];
            int ofs = 0;

            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, concat, ofs, arr.Length);
                ofs += arr.Length;
            }
                
            return concat;
        }
    }
}
