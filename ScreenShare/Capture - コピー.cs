using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using System.Diagnostics;
using System.Runtime.InteropServices;

using System.IO;
using System.Threading.Tasks;

using OpenCvSharp;

namespace ScreenCapture
{
    using Timer = System.Timers.Timer;
    using Size = System.Drawing.Size;

    /// <summary>
    /// プロセス及びデスクトップ画面のキャプチャを行います。
    /// </summary>
    class Capture
    {
        /// <summary>
        /// Win32API
        /// </summary>
        private class Win32
        {
            public static readonly int SRCCOPY = 13369376;

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);

            [DllImport("user32.dll")]
            public static extern IntPtr GetDC(IntPtr hwnd);

            [DllImport("gdi32.dll")]
            public static extern int BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);
        }

        /// <summary>
        /// 分割された画面後とのキャプチャを行う
        /// </summary>
        private class SegmentCapture
        {
            /// <summary>
            /// CaptureFrequencyListに対応するタイマーの間隔
            /// </summary>
            private static readonly int[] CaptureMillisecondMap = new int[]
            {
                1000, 500, 250, 100,
            };


            /// <summary>
            /// 分割画面がキャプチャされた時のイベントハンドラー。
            /// </summary>
            /// <param name="sender">センダー</param>
            /// <param name="captured">キャプチャされた分割画面</param>
            public delegate void SegmentCapturedEventHandler(object sender, Bitmap captured);

            /// <summary>
            /// 分割画面がキャプチャされた時に発生。
            /// </summary>
            public event SegmentCapturedEventHandler SegmentCaptured;

            /// <summary>
            /// 分割画面のキャプチャの際に例外が発生した際に発生。
            /// </summary>
            public event EventHandler<Exception> SegmentError;


            /// <summary>
            /// セグメント番号
            /// </summary>
            public int SegmentIndex { get; set; }

            /// <summary>
            /// キャプチャ領域
            /// </summary>
            public Rectangle SegmentRect { get; set; }

            /// <summary>
            /// キャプチャ(親)
            /// </summary>
            private Capture m_Capture;

            /// <summary>
            /// キャプチャ用タイマー
            /// </summary>
            private Timer m_Timer = new Timer();

            public SegmentCapture(Capture cap, int seg)
            {
                m_Capture = cap;

                SegmentIndex = seg;
                m_Timer.Elapsed += (s, e) =>
                    {
                        //try
                        {
                            var bmp = GetScreenCapture();
                            SegmentCaptured(this, bmp);
                        }
                        //catch (Exception ex)
                        {
                            //SegmentError(this, ex);
                            return;
                        }
                    };
            }

            public void Start(int capFreqIdx)
            {
                m_Timer.Interval = CaptureMillisecondMap[capFreqIdx];
                m_Timer.Start();
            }

            public void Stop()
            {
                m_Timer.Stop();
            }

            /// <summary>
            /// スクリーンキャプチャを行い、Bitmapを返す
            /// </summary>
            /// <returns>Bitmap</returns>
            private Bitmap GetScreenCapture()
            {
                if (m_Capture.CaptureProcess != null && m_Capture.CaptureProcess.HasExited)
                    throw new ObjectDisposedException(m_Capture.CaptureProcess.ProcessName, "プロセス オブジェクトは既に破棄されています。");

                var handle = m_Capture.CaptureProcess != null ? m_Capture.CaptureProcess.MainWindowHandle : IntPtr.Zero;
                var rect = new Rectangle();

                if (m_Capture.UseCaptureBounds)
                {
                    rect = m_Capture.CaptureBounds;
                }
                else if (handle != IntPtr.Zero)
                {
                    var w32rect = new Win32.RECT();
                    Win32.GetWindowRect(handle, ref w32rect);

                    rect.X = w32rect.left;
                    rect.Y = w32rect.top;
                    rect.Width = w32rect.right - w32rect.left;
                    rect.Height = w32rect.bottom - w32rect.top;

                    if (rect.X < 0) rect.X = 0;
                    if (rect.Y < 0) rect.Y = 0;
                }
                else
                {
                    rect = Screen.PrimaryScreen.Bounds;
                }

                Rectangle segRect = rect;
                segRect.Width /= m_Capture.CaptureDivisionNum;
                segRect.Height /= m_Capture.CaptureDivisionNum;
                segRect.X = rect.X + (rect.Width * (SegmentIndex % m_Capture.CaptureDivisionNum)) / m_Capture.CaptureDivisionNum;
                segRect.Y = rect.Y + (rect.Height * (SegmentIndex / m_Capture.CaptureDivisionNum)) / m_Capture.CaptureDivisionNum;

                var bmp = new Bitmap(segRect.Width, segRect.Height);

                var g = Graphics.FromImage(bmp);
                var hDC = g.GetHdc();
                var srcDC = Win32.GetDC(handle);

                Debug.ShowElapsedMilliSecond(() =>
                {
                    Win32.BitBlt(hDC, 0, 0, segRect.Width, segRect.Height,
                        srcDC, segRect.X, segRect.Y, Win32.SRCCOPY);
                    
                }, "bitblt");

                g.ReleaseHdc(hDC);
                g.Dispose();

                Win32.ReleaseDC(IntPtr.Zero, srcDC);

                SegmentRect = segRect;

                return bmp;
            }
        }



        /// <summary>
        /// キャプチャする頻度を表します。
        /// </summary>
        public enum CaptureFrequencyList
        {
            Low, Mid, High, Realtime,
        }

        /// <summary>
        /// キャプチャするプロセスを設定、取得します。
        /// </summary>
        public Process CaptureProcess { get; set; }

        /// <summary>
        /// キャプチャするデスクトップの領域を設定、取得します。
        /// </summary>
        public Rectangle CaptureBounds { get; set; }

        /// <summary>
        /// キャプチャに領域を使用するかどうかを設定、取得します。
        /// </summary>
        public bool UseCaptureBounds { get; set; }

        /// <summary>
        /// キャプチャした画面をスケーリングする際のサイズを設定、取得します。
        /// </summary>
        public Size ScaledSize { get; set; }

        /// <summary>
        /// キャプチャを行う頻度を設定、取得します。
        /// </summary>
        public CaptureFrequencyList CaptureFrequency { get; set; }

        /// <summary>
        /// キャプチャした画面のエンコード形式を文字列で設定、取得します。デフォルトは ".jpg" です。
        /// </summary>
        public string EncodeFormatExtension
        { 
            get
            {
                return m_EncodeFormatExtension; 
            }
            set
            {
                m_EncodeFormatExtension = value;

                switch (m_EncodeFormatExtension)
                {
                    case ".jpg":
                        m_EncodingParam.EncodingId = ImwriteFlags.JpegQuality;
                        break;

                    case ".png":
                        m_EncodingParam.EncodingId = ImwriteFlags.PngCompression;
                        break;

                    default:
                        m_EncodingParam.EncodingId = ImwriteFlags.JpegQuality;
                        break;
                }
            }
        }

        /// <summary>
        /// キャプチャした画面のエンコード品質を設定、取得します。デフォルトは 95 です。
        /// </summary>
        public int EncoderQuality
        { 
            get
            {
                return m_EncodeQuality;
            }
            set
            {
                m_EncodeQuality = value;
                m_EncodingParam.Value = value;
            }
        }

        /// <summary>
        /// キャプチャした画面の、差分の割合(異なる画素の数)が画素総数の何割かを超えた場合に元画像として得るかの割合を設定、取得します。デフォルトは 0.25f です。
        /// </summary>
        public float DiffFrameRatio 
        { 
            get
            {
                return m_DiffFrameRatio;
            }
            set
            {
                if (value < 0.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                m_DiffFrameRatio = value;
            }
        }

        /// <summary>
        /// キャプチャ画面を分割する縦、横の分割数を設定、取得します。デフォルトは 3 です。
        /// </summary>
        public int CaptureDivisionNum 
        { 
            get
            {
                return m_CaptureDivisionNum;
            }
            set
            {
                m_CaptureDivisionNum = value;

                var segNum = value * value;

                m_SegmentCaptures = new SegmentCapture[segNum];
                m_IntraFrameMat = new Mat[segNum];

                for (int i = 0; i < segNum; i++)
                {
                    m_SegmentCaptures[i] = new SegmentCapture(this, i);
                    m_SegmentCaptures[i].SegmentCaptured += (s, bmp) => SegmentCaptured((SegmentCapture)s, bmp);
                    m_SegmentCaptures[i].SegmentError += Error;
                }
            }
        }

        /// <summary>
        /// 画面がキャプチャされた時のイベントハンドラーです。
        /// </summary>
        /// <param name="sender">センダー</param>
        /// <param name="rect">キャプチャされた領域</param>
        /// <param name="frameBuffer">画像バッファ</param>
        /// <param name="isIntraFrame">Iフレームかどうか</param>
        public delegate void CapturedEventHandler(object sender, Rectangle rect, byte[] frameBuffer, bool isIntraFrame);

        /// <summary>
        /// 画面がキャプチャされた時に発生します。
        /// </summary>
        public event CapturedEventHandler Captured;

        /// <summary>
        /// 例外が発生した際に発生します。
        /// </summary>
        public event EventHandler<Exception> Error;

        /// <summary>
        /// エンコードパラメータ
        /// </summary>
        private ImageEncodingParam m_EncodingParam = new ImageEncodingParam(ImwriteFlags.PngCompression, 9);
        
        /// <summary>
        /// エンコードフォーマット
        /// </summary>
        private string m_EncodeFormatExtension = ".png";

        /// <summary>
        /// エンコード品質
        /// </summary>
        private int m_EncodeQuality = 95;

        /// <summary>
        /// 差分割合
        /// </summary>
        private float m_DiffFrameRatio = 0.25f;

        /// <summary>
        /// 分割数
        /// </summary>
        private int m_CaptureDivisionNum = 3;

        /// <summary>
        /// 分割画面キャプチャインスタンス
        /// </summary>
        private SegmentCapture[] m_SegmentCaptures;

        /// <summary>
        /// 分割画面ごとのIフレーム
        /// </summary>
        private Mat[] m_IntraFrameMat;

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public Capture()
        {
            CaptureBounds = Screen.PrimaryScreen.Bounds;
            ScaledSize = new Size(CaptureBounds.Width, CaptureBounds.Height);
            CaptureFrequency = CaptureFrequencyList.Mid;

            CaptureDivisionNum = 3;
        }

        /// <summary>
        /// 画面のキャプチャを開始します。
        /// </summary>
        public void Start()
        {
            if (CaptureProcess != null && CaptureProcess.HasExited)
                throw new ObjectDisposedException(CaptureProcess.ProcessName, "プロセス オブジェクトは既に破棄されています。");

            foreach (var segCap in m_SegmentCaptures)
            {
                segCap.Start((int)CaptureFrequency);
            }
        }

        /// <summary>
        /// キャプチャを停止します。
        /// </summary>
        public void Stop()
        {
            foreach (var seg in m_SegmentCaptures)
            {
                seg.Stop();
            }

            m_IntraFrameMat = new Mat[m_IntraFrameMat.Length];
        }

        private void SegmentCaptured(SegmentCapture segCap, Bitmap bmp)
        {
            //try
            {
                var intraMat = m_IntraFrameMat[segCap.SegmentIndex];
                var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
                //mat = mat.CvtColor(ColorConversionCodes.RGBA2RGB);

                //byte[] buffer = new byte[mat.Total() * mat.Channels()];
                //Marshal.Copy(mat.DataStart, buffer, 0, buffer.Length);

                if (intraMat == null)
                {
                    m_IntraFrameMat[segCap.SegmentIndex] = mat;

                    var img_buffer = mat.ImEncode(EncodeFormatExtension, m_EncodingParam);
                    //Debug.Log("id: " + segCap.SegmentIndex + "img_len:" + img_buffer.Length);
                    Captured(this, segCap.SegmentRect, img_buffer, true);
                }
                else
                {
                    var mat_xor = new Mat();

                    Cv2.BitwiseXor(mat, intraMat, mat_xor);

                    var diff_count = mat_xor.CvtColor(ColorConversionCodes.RGBA2GRAY).CountNonZero();

                    //Debug.Log("id: " + segCap.SegmentIndex + "diffcount:" + diff_count + " / " + (mat.Rows * mat.Cols) + " n:" + mat.Rows * mat.Cols * DiffFrameRatio);

                    if (diff_count >= mat.Rows * mat.Cols * DiffFrameRatio)
                    {
                        m_IntraFrameMat[segCap.SegmentIndex] = mat;

                        var img_buffer = mat.ImEncode(EncodeFormatExtension, m_EncodingParam);
                        //Debug.Log("id: " + segCap.SegmentIndex + "img_len:" + img_buffer.Length);
                        Captured(this, segCap.SegmentRect, img_buffer, true);
                    }
                    else
                    {
                        if (diff_count == 0)
                            return;

                        var img_buffer = mat_xor.ImEncode(EncodeFormatExtension, m_EncodingParam);
                        //Debug.Log("id: " + segCap.SegmentIndex + "img_len:" + img_buffer.Length);
                        Captured(this, segCap.SegmentRect, img_buffer, false);
                    }
                }
            }
            //catch (Exception ex)
            {
                //Error(this, ex);
            }
            
        }
    }
}
