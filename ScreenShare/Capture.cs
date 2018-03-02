using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
    using Point = OpenCvSharp.Point;

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
        /// 1ピクセルあたりのビット数
        /// </summary>
        public int bitCount;
    }

    /// <summary>
    /// 前フレームとの差分が生じた領域のキャプチャデータを格納します
    /// </summary>
    public struct SegmentCaptureData
    {
        /// <summary>
        /// キャプチャされた短径
        /// </summary>
        public Rectangle segment;

        /// <summary>
        /// キャプチャされた領域のエンコード済みバッファ
        /// </summary>
        public byte[] encodedFrameBuffer;
    }


    /// <summary>
    /// 過去のフレームと同じ部分の領域のキャプチャデータを格納します
    /// </summary>
    public struct SegmentCopyData
    {
        /// <summary>
        /// 過去のフレームのコピー領域
        /// </summary>
        public Rectangle source;

        /// <summary>
        /// 過去のフレームのコピー領域の貼り付け先領域
        /// </summary>
        public Rectangle dest;
    }

    /// <summary>
    /// キャプチャする対象を表します
    /// </summary>
    public enum CaptureTarget
    {
        Desktop,
        Process,
    }

    /// <summary>
    /// プロセス及びデスクトップ画面のキャプチャを行います。
    /// </summary>
    class Capture
    {

        /// <summary>
        /// キャプチャする対象を設定、取得します。
        /// </summary>
        public CaptureTarget CaptureTarget { get; set; }

        /// <summary>
        /// キャプチャするデスクトップの領域を設定、取得します。
        /// </summary>
        public Rectangle CaptureBounds { get; set; }

        /// <summary>
        /// デスクトップをキャプチャするディスプレイを設定、取得します。
        /// </summary>
        public Screen CaptureDisplay { get; set; }

        /// <summary>
        /// キャプチャするプロセスを設定、取得します。
        /// </summary>
        public Process CaptureProcess
        {
            get
            {
                return m_CaptureProcess;
            }
            set
            {
                m_CaptureProcess = value;
                if (value != null)
                {
                    CaptureBounds = GetCaptureRect(value.MainWindowHandle);
                }
            }
        }

        /// <summary>
        /// キャプチャする領域のアスペクト比を返します。
        /// </summary>
        public float AspectRatio
        {
            get
            {
                return (float)CaptureBounds.Width / CaptureBounds.Height;
            }
        }

        /// <summary>
        /// キャプチャした画面のスケーリング値を設定、取得します。
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// キャプチャを行う頻度(fps)を設定、取得します。
        /// </summary>
        public int FramesPerSecond { get; set; }

        /// <summary>
        /// キャプチャした画面のエンコード形式を文字列で設定(".jpeg", ".png", ".webp")、取得します。デフォルトは ".webp" です。
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
                    case ".webp":
                        m_EncodingParam.EncodingId = ImwriteFlags.WebPQuality;
                        break;

                    case ".jpg":
                        m_EncodingParam.EncodingId = ImwriteFlags.JpegQuality;
                        break;

                    case ".png":
                        m_EncodingParam.EncodingId = ImwriteFlags.PngCompression;
                        break;

                    default:
                        m_EncodingParam.EncodingId = ImwriteFlags.WebPQuality;
                        break;
                }
            }
        }

        /// <summary>
        /// キャプチャした画面のエンコード品質を設定、取得します。デフォルトは 20 です。
        /// </summary>
        public int EncoderQuality
        { 
            get
            {
                return m_EncodingParam.Value;
            }
            set
            {
                m_EncodingParam.Value = value;
            }
        }

        /// <summary>
        /// キャプチャ後のスケールされたサイズを取得します。
        /// </summary>
        public Size DestinationSize { get; private set; }

        /// <summary>
        /// 現在送信しているかどうかを返します。
        /// </summary>
        public bool Capturing { get; private set; }

        /// <summary>
        /// 送信されるセグメントのグリッド幅を設定します。デフォルトは 16 です。
        /// </summary>
        public int GridWidth
        {
            get
            {
                return m_GridWidth;
            }
            set
            {
                if (value <= 2) value = 2;
                m_GridWidth = value;
            }
        }

        public int CapturedCount { get; private set; }

        /// <summary>
        /// 画面がキャプチャされた時に発生します。
        /// </summary>
        public event EventHandler<CaptureData> OnCaptured = delegate {};

        /// <summary>
        /// 前フレームとの差分が生じた時に発生します。
        /// </summary>
        public event EventHandler<List<SegmentCaptureData>> OnSegmentCaptured = delegate { };

        /// <summary>
        /// キャプチャが開始されたときに発生します。
        /// </summary>
        public event EventHandler OnStarted = delegate { };

        /// <summary>
        /// キャプチャが停止したときに発生します。
        /// </summary>
        public event EventHandler OnStopped = delegate { };

        /// <summary>
        /// 例外が発生した際に発生します。
        /// </summary>
        public event EventHandler<Exception> OnError = delegate { };



        /// <summary>
        /// キャプチャスクリーン
        /// </summary>
        private Screen m_CaptureScreen = Screen.PrimaryScreen;

        /// <summary>
        /// エンコードパラメータ
        /// </summary>
        private ImageEncodingParam m_EncodingParam = new ImageEncodingParam(ImwriteFlags.WebPQuality, 20);
        
        /// <summary>
        /// エンコードフォーマット
        /// </summary>
        private string m_EncodeFormatExtension = ".webp";

        /// <summary>
        /// キャプチャデバイスコンテキスト
        /// </summary>
        private IntPtr m_Hdc = IntPtr.Zero;

        /// <summary>
        /// キャプチャプロセスデバイスコンテキスト
        /// </summary>
        private IntPtr m_ProcessHdc = IntPtr.Zero;
        private IntPtr m_Hbmp;
        private IntPtr m_HdcMem;
        private IntPtr m_Bits;

        /// <summary>
        /// キャプチャ短形
        /// </summary>
        private Rectangle m_SrcRect;

        /// <summary>
        /// キャプチャプロセス
        /// </summary>
        private Process m_CaptureProcess;

        /// <summary>
        /// 内部キャプチャ状態
        /// </summary>
        private bool m_IsCapturing = false;

        /// <summary>
        /// キャプチャタスク
        /// </summary>
        private Task m_Task;

        /// <summary>
        /// セグメントグリッド幅
        /// </summary>
        private int m_GridWidth = 16;

        private int m_LastImageGotCount = 0;
        private SegmentCaptureData m_LastCapturedImage;
        
        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public Capture()
        {
            CaptureTarget = CaptureTarget.Desktop;
            CaptureBounds = Screen.PrimaryScreen.Bounds;
        }

        /// <summary>
        /// 画面のキャプチャを開始します。
        /// </summary>
        public void Start()
        {
            if (CaptureProcess != null && CaptureProcess.HasExited)
            {
                OnError(this, new ObjectDisposedException(CaptureProcess.ProcessName, "プロセス オブジェクトは既に破棄されています。"));
            }

            Capturing = true;

            m_IsCapturing = true;
            Initialize();

            StartCaptureLoop(1000.0 / FramesPerSecond);

            OnStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// キャプチャの停止を待ちます。
        /// </summary>
        public void Stop(int timeoutms = 1000)
        {
            m_IsCapturing = false;

            if (m_Task == null) return;

            m_Task.Wait();
            
            CleanUp();

            Capturing = false;
            
            OnStopped(this, EventArgs.Empty);
        }

        public SegmentCaptureData GetLastCapturedImage(string ext = null)
        {
            if (CapturedCount == m_LastImageGotCount) return m_LastCapturedImage;

            m_LastImageGotCount = CapturedCount;
            m_LastCapturedImage = new SegmentCaptureData()
            {
                encodedFrameBuffer = new Mat(DestinationSize.Height, DestinationSize.Width, MatType.CV_8UC3, m_Bits).ImEncode(ext == null ? m_EncodeFormatExtension : ext),
                segment = new Rectangle(0, 0, DestinationSize.Width, DestinationSize.Height),
            };

            return m_LastCapturedImage;
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
                var sw = new Stopwatch();
                var cData = new CaptureData()
                {
                    captureData = m_Bits,
                    captureSize = DestinationSize,
                    bitCount = 32,
                };

                byte[] imageBuffer;

                var mat = new Mat(DestinationSize.Height, DestinationSize.Width, MatType.CV_8UC4, m_Bits);
                var mat_U3 = new Mat(mat.Size(), MatType.CV_8UC3);
                var mat_prev = new Mat(mat.Size(), MatType.CV_8UC3, Scalar.All(0x00));
                var mat_xor = new Mat(mat.Size(), MatType.CV_8UC3, Scalar.All(0xff));
                var mat_gray = new Mat(mat.Size(), MatType.CV_8UC1);
                var mat_diff = new Mat(mat.Size(), MatType.CV_8UC1, Scalar.All(0xff));
                var mat_close = new Mat(mat.Size(), MatType.CV_8UC1, Scalar.All(0xff));
                var mat_mask = new Mat(mat.Size(), MatType.CV_8UC1, Scalar.All(0xff));
                var mat_maskedU3 = new Mat(mat.Size(), MatType.CV_8UC3);
                var mat_masked = new Mat(mat.Size(), MatType.CV_8UC4);

                var segCapData = new SegmentCaptureData();
                var segCaptureList = new List<SegmentCaptureData>();

                CapturedCount = 0;
                sw.Start();

                var srcHdc = CaptureTarget == CaptureTarget.Desktop ? m_Hdc : m_ProcessHdc;

                while (m_IsCapturing)
                {
                    
                    var t = sw.ElapsedMilliseconds;
                    
                    Win32.StretchBlt(m_HdcMem, 0, DestinationSize.Height, DestinationSize.Width, -DestinationSize.Height, srcHdc,
                        m_SrcRect.X, m_SrcRect.Y, m_SrcRect.Width, m_SrcRect.Height, Win32.SRCCOPY | Win32.CAPTUREBLT);

                    Debug.Log("test", "" + (sw.ElapsedMilliseconds - t));
                    OnCaptured(this, cData);

#if BROADCAST_IMAGE
                    try
                    {
                        Cv2.CvtColor(mat, mat_U3, ColorConversionCodes.BGRA2BGR);
                        mat_mask.SetTo(Scalar.All(0xff));
                        mat_maskedU3.SetTo(Scalar.All(0x00));
                        mat_masked.SetTo(Scalar.All(0x00));

                        if (CapturedCount == 0)
                        {
                            Cv2.ImEncode(EncodeFormatExtension, mat_U3, out imageBuffer, m_EncodingParam);
                            segCapData.segment = new Rectangle(0, 0, mat_U3.Width, mat_U3.Height);
                            segCapData.encodedFrameBuffer = imageBuffer;

                            segCaptureList.Add(segCapData);
                        }
                        else
                        {
                            mat_xor = mat_U3 ^ mat_prev;
                            Cv2.CvtColor(mat_xor, mat_gray, ColorConversionCodes.BGR2GRAY);
                            Cv2.Threshold(mat_gray, mat_diff, 1, 0xff, ThresholdTypes.Binary);
                            Cv2.MorphologyEx(mat_diff, mat_close, MorphTypes.Close, new Mat(), null, 8);

                            var cc = Cv2.ConnectedComponentsEx(mat_close);
                            foreach (var blob in cc.Blobs.Skip(1))
                            {
                                var rect = blob.Rect;
                                var mat_label = new Mat(mat_U3, rect);
                                var mat_labelMask = new Mat(mat_mask, rect);
                                var mat_labelMaskedU3 = new Mat(mat_maskedU3, rect);
                                var mat_labelMasked = new Mat(mat_masked, rect);

                                var dx = (rect.X + rect.Width) / GridWidth;
                                var dy = (rect.Y + rect.Height) / GridWidth;
                                for (var x = rect.X / GridWidth; x <= dx; ++x)
                                {
                                    for (var y = rect.Y / GridWidth; y <= dy; ++y)
                                    {
                                        var segRect = new Rect(x * GridWidth, y * GridWidth,
                                            Math.Min(mat.Width - x * GridWidth, GridWidth),
                                            Math.Min(mat.Height - y * GridWidth, GridWidth));

                                        if (new Mat(mat_close, segRect).CountNonZero() == 0)
                                            new Mat(mat_mask, segRect).SetTo(Scalar.All(0x00));
                                    }
                                }

                                mat_label.CopyTo(mat_labelMaskedU3, mat_labelMask);
                                Cv2.Merge(new Mat[] { mat_labelMaskedU3, mat_labelMask }, mat_labelMasked);
                                Cv2.ImEncode(EncodeFormatExtension, mat_labelMasked, out imageBuffer, m_EncodingParam);
                                segCapData.segment = new Rectangle(blob.Rect.X, blob.Rect.Y, blob.Rect.Width, blob.Rect.Height);
                                segCapData.encodedFrameBuffer = imageBuffer;
                                segCaptureList.Add(segCapData);
                            }
                        }

                        mat_U3.CopyTo(mat_prev);

                    }
                    catch (Exception e) { Debug.Log("OpenCV", e.ToString()); continue; }

                    if (segCaptureList.Count != 0)
                        OnSegmentCaptured(this, segCaptureList);
                    segCaptureList.Clear();
                            
#endif

                    var sleepMs = (ms * CapturedCount) - sw.Elapsed.TotalMilliseconds;
                    if (sleepMs > 0) Thread.Sleep((int)sleepMs);
                    CapturedCount++;
                }

                sw.Stop();
            });
        }

        /// <summary>
        /// キャプチャの準備
        /// </summary>
        private void Initialize()
        {
            switch (CaptureTarget)
            {
                case CaptureTarget.Desktop:
                    m_Hdc = Win32.CreateDC(CaptureDisplay.DeviceName, "", "", IntPtr.Zero);
                    m_SrcRect = CaptureBounds;
                    break;

                case CaptureTarget.Process:
                    m_Hdc = CaptureProcess != null ? CaptureProcess.MainWindowHandle : IntPtr.Zero;
                    m_SrcRect = GetCaptureRect(m_Hdc);
                    m_ProcessHdc = Win32.GetWindowDC(m_Hdc);
                    break;
            }

            //m_SrcRect.Width += m_SrcRect.Width & 1;
            //m_SrcRect.Height += m_SrcRect.Height & 1;
            var w = (int)(m_SrcRect.Width * Scale);
            var h = (int)(m_SrcRect.Height * Scale);

            DestinationSize = new Size(w + (w & 1), h + (h & 1));
            //m_DstSize = new Size(w, h);

            //m_DstSize = new Size(927, 692);

            var bmi = new Win32.BITMAPINFO();
            var bmiHeader = new Win32.BITMAPINFOHEADER();

            bmiHeader.biSize = (uint)Marshal.SizeOf(bmiHeader);
            bmiHeader.biWidth = DestinationSize.Width;
            bmiHeader.biHeight = DestinationSize.Height;
            bmiHeader.biPlanes = 1;
            bmiHeader.biBitCount = 32;
            bmiHeader.biSizeImage = 0;// (uint)(m_DstSize.Width * m_DstSize.Height * 4);
            bmiHeader.biCompression = Win32.BI_RGB;
            bmiHeader.biXPelsPerMeter = 0;
            bmiHeader.biYPelsPerMeter = 0;
            bmiHeader.biClrUsed = 0;
            bmiHeader.biClrImportant = 0;

            bmi.bmiHeader = bmiHeader;

            m_Hbmp = Win32.CreateDIBSection(m_Hdc, ref bmi, Win32.DIB_RGB_COLORS, out m_Bits, IntPtr.Zero, 0);
            m_HdcMem = Win32.CreateCompatibleDC(IntPtr.Zero);
            //m_Hbmp = Win32.CreateDIBitmap(m_ProcessDC, ref bmiHeader, 0, null, ref bmi, 0);

            var hbmpPrev = Win32.SelectObject(m_HdcMem, m_Hbmp);

            Win32.DeleteObject(hbmpPrev);
            Win32.SetStretchBltMode(m_HdcMem, Win32.STRETCH_HALFTONE);
        }

        /// <summary>
        /// キャプチャ後始末
        /// </summary>
        private void CleanUp()
        {
            if (m_ProcessHdc != IntPtr.Zero)
                Win32.ReleaseDC(m_Hdc, m_ProcessHdc);
            Win32.DeleteObject(m_Hbmp);
            Win32.DeleteDC(m_HdcMem);
            Win32.DeleteDC(m_Hdc);
        }

        /// <summary>
        /// キャプチャ範囲取得
        /// </summary>
        /// <returns></returns>
        private Rectangle GetCaptureRect(IntPtr handle)
        {
            var rect = new Rectangle();

            Win32.RECT w32rect;
            Win32.GetWindowRect(handle, out w32rect);

            rect.X = 0;
            rect.Y = 0;
            rect.Width = w32rect.right - w32rect.left;
            rect.Height = w32rect.bottom - w32rect.top;

            if (rect.Width == 0 || rect.Height == 0)
            {
                OnError(this, new ArgumentException(CaptureProcess.ProcessName, "無効なプロセスです。"));
            }

            return rect;
        }
    }
}
