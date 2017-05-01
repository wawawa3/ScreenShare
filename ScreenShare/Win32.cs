using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ScreenShare
{
    /// <summary>
    /// Win32API
    /// </summary>
    class Win32
    {
        public static readonly int SRCCOPY = 0x00CC0020;
        public static readonly int SRCPAINT = 0x00EE0086;
        public static readonly int CAPTUREBLT = 0x40000000;
        public static readonly int STRETCH_HALFTONE = 0x04;
        public static readonly uint PM_NOREMOVE = 0x00;

        public static readonly uint DIB_RGB_COLORS = 0; /* color table in RGBs */
        public static readonly uint DIB_PAL_COLORS = 1; /* color table in palette indices */
        public static readonly uint BI_RGB = 0;
        public static readonly uint BI_RLE8 = 1;
        public static readonly uint BI_RLE4 = 2;
        public static readonly uint BI_BITFIELDS = 3;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public RGBQUAD bmiColors_1;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public byte bmPlanes;
            public byte bmBitsPixel;
            public IntPtr bmBits;
        } 

        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(string pszDriver, string pszDevice, string pszOutput, IntPtr pInitData);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        public static extern int BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("gdi32.dll")]
        public static extern int StretchBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int nSrcWidth, int nSrcHeight, int dwRop);

        [DllImport("gdi32.dll")]
        public static extern int SetStretchBltMode(IntPtr hdc, int iStretchMode);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

        public delegate bool WNDENUMPROC(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, IntPtr lParam);

        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi, uint pila, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDIBitmap(IntPtr hdc, ref BITMAPINFOHEADER lpbmih, uint fdwInit, byte[] lpbInit, ref BITMAPINFO lpbmi, uint fuUsage);

        [DllImport("gdi32.dll")] 
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        public static extern int GetObject(IntPtr hgdiobj, int cbBuffer, ref BITMAP lpvObject);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

    }
}
