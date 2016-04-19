using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using OpenCvSharp;

namespace ScreenShare
{
    using Timer = System.Timers.Timer;
    using Size = System.Drawing.Size;

    /// <summary>
    /// プロセス及びデスクトップ画面のキャプチャを行います。
    /// </summary>
    class Capture
    {
        /// <summary>
        /// キャプチャ時のデータを格納します
        /// </summary>
        public struct CaptureData
        {
            /// <summary>
            /// キャプチャされた画像
            /// </summary>
            public IntPtr captureData;

            /// <summary>
            /// キャプチャサイズ
            /// </summary>
            public Size captureSize;

            /// <summary>
            /// Iフレームかどうか
            /// </summary>
            public bool isIntraFrame;
        }

        /// <summary>
        /// 分割画面のキャプチャ時のデータを格納します
        /// </summary>
        public struct SegmentCaptureData
        {
            /// <summary>
            /// キャプチャされた領域の識別子
            /// </summary>
            public int segmentIdx;

            /// <summary>
            /// キャプチャされた領域
            /// </summary>
            public Rectangle rect;

            /// <summary>
            /// キャプチャされた領域のエンコード済みバッファ
            /// </summary>
            public byte[] encodedFrameBuffer;
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
        /// キャプチャした画面のスケーリング値を設定、取得します。
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// キャプチャを行う頻度(fps)を設定、取得します。
        /// </summary>
        public int FramesPerSecond { get; set; }

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
        public int CaptureDivisionNum { get; set; }

        /// <summary>
        /// キャプチャする全体のサイズを取得します。
        /// </summary>
        public Size CaptureSize { get; private set; }

        /// <summary>
        /// キャプチャ後のスケールされたサイズを取得します。
        /// </summary>
        public Size ScaledCaptureSize { get; private set; }

        /// <summary>
        /// 画面がキャプチャされた時に発生します。
        /// </summary>
        public event EventHandler<CaptureData> Captured;

        /// <summary>
        /// 分割画面がキャプチャされた時に発生します。
        /// </summary>
        public event EventHandler<SegmentCaptureData> SegmentCaptured;

        /// <summary>
        /// 例外が発生した際に発生します。
        /// </summary>
        public event EventHandler<Exception> Error;

        /// <summary>
        /// 現在送信しているかどうかを返します。
        /// </summary>
        public bool Capturing { get; private set; }

        /// <summary>
        /// エンコードパラメータ
        /// </summary>
        private ImageEncodingParam m_EncodingParam = new ImageEncodingParam(ImwriteFlags.JpegQuality, 95);
        
        /// <summary>
        /// エンコードフォーマット
        /// </summary>
        private string m_EncodeFormatExtension = ".jpg";

        /// <summary>
        /// エンコード品質
        /// </summary>
        private int m_EncodeQuality = 95;

        /// <summary>
        /// 差分割合
        /// </summary>
        private float m_DiffFrameRatio = 0.25f;

        /// <summary>
        /// Iフレーム
        /// </summary>
        private Mat m_IntraFrameMat;

        /// <summary>
        /// キャプチャタイマー
        /// </summary>
        private Timer m_Timer = new Timer();

        /// <summary>
        /// キャプチャプロセスハンドル
        /// </summary>
        private IntPtr m_Handle = IntPtr.Zero;

        /// <summary>
        /// キャプチャプロセスデバイスコンテキスト
        /// </summary>
        private IntPtr m_ProcessDC = IntPtr.Zero;
        private IntPtr m_Hbmp;
        private IntPtr m_Hdc;
        private IntPtr m_Bits;

        /// <summary>
        /// キャプチャする短形
        /// </summary>
        private Rectangle m_SrcRect;

        /// <summary>
        /// キャプチャイメージサイズ
        /// </summary>
        private Size m_DstSize;
        
        /// <summary>
        /// キャプチャ状態
        /// </summary>
        private bool m_IsCapturing = false;

        /// <summary>
        /// キャプチャタスク
        /// </summary>
        private Task m_Task;

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public Capture()
        {
            CaptureBounds = Screen.PrimaryScreen.Bounds;
        }

        /// <summary>
        /// 画面のキャプチャを開始します。
        /// </summary>
        public void Start()
        {
            if (CaptureProcess != null && CaptureProcess.HasExited)
                Error(this, new ObjectDisposedException(CaptureProcess.ProcessName, "プロセス オブジェクトは既に破棄されています。"));

            Capturing = true;

            m_IsCapturing = true;
            PrepareCapturing();

            Debug.Log("Start Capturing");
            
            StartCaptureLoop(1000.0 / FramesPerSecond);
        }

        /// <summary>
        /// キャプチャの停止を待ちます。
        /// </summary>
        public async void StopAsync(int timeoutms = 1000)
        {
            m_IsCapturing = false;

            if (m_Task == null) return;

            await Task.Run(() => m_Task.Wait());

            CleanUp();

            m_IntraFrameMat = null;

            Capturing = false;
            Debug.Log("Stop Capturing");
        }

        /// <summary>
        /// キャプチャループスレッド作成
        /// </summary>
        /// <param name="ms">キャプチャ間隔</param>
        private void StartCaptureLoop(double ms = 50)
        {
            //Parallel.Invoke(() =>
            m_Task = Task.Run(() =>
            {
                var capCnt = 0;
                var sw = new Stopwatch();

                var mat = new Mat(m_DstSize.Height, m_DstSize.Width, MatType.CV_8UC4, m_Bits);
                var mat_xor = new Mat();
                var mat_diff = new Mat();

                var segRect = new Rectangle(0, 0, m_DstSize.Width / CaptureDivisionNum, m_DstSize.Height / CaptureDivisionNum);

                var cData = new CaptureData()
                {
                    captureData = m_Bits,
                    captureSize = m_DstSize,
                    isIntraFrame = true,
                };

                m_IntraFrameMat = new Mat(m_DstSize.Height, m_DstSize.Width, MatType.CV_8UC4);

                sw.Start();

                while (m_IsCapturing)
                {
                    //Win32.BitBlt(hdc, 0, 0, dstSize.Height, dstSize.Width, m_ProcessDC, 0, 0, Win32.SRCCOPY);

                    Win32.StretchBlt(m_Hdc, 0, m_DstSize.Height, m_DstSize.Width, -m_DstSize.Height, m_ProcessDC, 
                        m_SrcRect.X, m_SrcRect.Y, m_SrcRect.Width, m_SrcRect.Height, Win32.SRCCOPY);

                    Captured(this, cData);

                    try
                    {
                        Cv2.BitwiseXor(mat, m_IntraFrameMat, mat_xor);
                        Cv2.CvtColor(mat_xor, mat_diff, ColorConversionCodes.RGBA2GRAY);
                    }
                    catch
                    {
                        continue;
                    }

                    for (int y = 0; y < CaptureDivisionNum; y++)
                    {
                        for (int x = 0; x < CaptureDivisionNum; x++)
                        {
                            var segIdx = y * CaptureDivisionNum + x;

                            segRect.X = segRect.Width * x;
                            segRect.Y = segRect.Height * y;

                            var sRect = new Rect(segRect.X, segRect.Y, segRect.Width, segRect.Height);
                            var segDiff = mat_diff.SubMat(sRect);
                            var nonZero = segDiff.CountNonZero();

                            if (nonZero != 0)
                            {
                                var segCapture = mat.SubMat(sRect);
                                var img_buffer = segCapture.ImEncode(EncodeFormatExtension, m_EncodingParam);

                                var sData = new SegmentCaptureData()
                                {
                                    segmentIdx = segIdx,
                                    rect = segRect,
                                    encodedFrameBuffer = img_buffer,
                                };

                                SegmentCaptured(this, sData);

                                var segIntra = m_IntraFrameMat.SubMat(sRect);
                                segCapture.CopyTo(segIntra);
                            }
                        }
                    }

                    var sleepMs = (ms * capCnt) - sw.Elapsed.TotalMilliseconds;
                    if (sleepMs > 0) Thread.Sleep((int)sleepMs);
                    capCnt++;

                    //Debug.Log(""+capCnt);
                    //GC.Collect();
                    //File.WriteAllBytes("dump/"+capCnt+".jpg", mat.ImEncode(EncodeFormatExtension, m_EncodingParam));
                }

                sw.Stop();

                //Win32.SelectObject(hdc, hbmpPrev);
                
            });
        }

        /// <summary>
        /// キャプチャの準備
        /// </summary>
        private void PrepareCapturing()
        {
            m_Handle = CaptureProcess != null ? CaptureProcess.MainWindowHandle : IntPtr.Zero;
            m_ProcessDC = Win32.GetWindowDC(m_Handle);

            m_SrcRect = GetCaptureRect();
            m_DstSize = new Size((int)(m_SrcRect.Width * Scale), (int)(m_SrcRect.Height * Scale));

            var bmi = new Win32.BITMAPINFO();
            var bmiHeader = new Win32.BITMAPINFOHEADER();

            bmiHeader.biSize = (uint)Marshal.SizeOf(bmiHeader);
            bmiHeader.biWidth = m_DstSize.Width;
            bmiHeader.biHeight = m_DstSize.Height;
            bmiHeader.biPlanes = 1;
            bmiHeader.biBitCount = 32;
            bmiHeader.biSizeImage = (uint)(m_DstSize.Width * m_DstSize.Height * 4);

            bmi.bmiHeader = bmiHeader;

            m_Hbmp = Win32.CreateDIBSection(IntPtr.Zero, ref bmi, Win32.DIB_RGB_COLORS, out m_Bits, IntPtr.Zero, 0);
            m_Hdc = Win32.CreateCompatibleDC(IntPtr.Zero);

            var hbmpPrev = Win32.SelectObject(m_Hdc, m_Hbmp);
            Win32.DeleteObject(hbmpPrev);
            Win32.SetStretchBltMode(m_Hdc, Win32.STRETCH_HALFTONE);
        }

        /// <summary>
        /// キャプチャ後始末
        /// </summary>
        private void CleanUp()
        {
            Win32.ReleaseDC(IntPtr.Zero, m_ProcessDC);
            Win32.DeleteObject(m_Hbmp);
            Win32.DeleteDC(m_Hdc);
        }

        /// <summary>
        /// キャプチャ範囲取得
        /// </summary>
        /// <returns></returns>
        private Rectangle GetCaptureRect()
        {
            var rect = new Rectangle();

            if (UseCaptureBounds)
            {
                rect = CaptureBounds;
            }
            else if (m_Handle != IntPtr.Zero)
            {
                Win32.RECT w32rect;
                Win32.GetWindowRect(m_Handle, out w32rect);

                rect.X = 0;
                rect.Y = 0;
                rect.Width = w32rect.right - w32rect.left;
                rect.Height = w32rect.bottom - w32rect.top;

                if (rect.Width == 0 || rect.Height == 0)
                {
                    Error(this, new ArgumentException(CaptureProcess.ProcessName, "無効なプロセスです。"));
                }
            }
            else
            {
                rect = Screen.PrimaryScreen.Bounds;
            }

            return rect;
        }
    }
}
