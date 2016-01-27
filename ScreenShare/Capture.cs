using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using System.Diagnostics;

using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using OpenCvSharp;

namespace ScreenShare
{
    using Point = OpenCvSharp.Point;
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
            public Bitmap captureBitmap;

            /// <summary>
            /// キャプチャ領域
            /// </summary>
            public Rectangle captureRect;

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
        public bool IsCapturing { get; private set; }

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

        private object m_captureLocking = new object();

        private Queue m_CaptureEventQueue = new Queue();

        private Queue m_SegmentCaptureEventQueue = new Queue();

        private bool m_IsCapturing = false;
        private bool m_IsConverting = false;

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public Capture()
        {
            CaptureBounds = Screen.PrimaryScreen.Bounds;
            Scale = 0.5f;
            FramesPerSecond = 33;

            CaptureDivisionNum = 3;
        }

        /// <summary>
        /// 画面のキャプチャを開始します。
        /// </summary>
        public void Start()
        {
            if (CaptureProcess != null && CaptureProcess.HasExited)
                Error(this, new ObjectDisposedException(CaptureProcess.ProcessName, "プロセス オブジェクトは既に破棄されています。"));

            m_Handle = CaptureProcess != null ? CaptureProcess.MainWindowHandle : IntPtr.Zero;
            m_ProcessDC = Win32.GetWindowDC(m_Handle);

            Debug.Log("start");

            IsCapturing = true;

            StartCaptureLoop(1000.0 / FramesPerSecond);

            m_IsCapturing = true;
            m_IsConverting = true;
        }

        /// <summary>
        /// キャプチャを停止します。
        /// </summary>
        public void Stop()
        {
            IsCapturing = false;
        }

        /// <summary>
        /// キャプチャの停止を待ちます。
        /// </summary>
        public void WaitStop(int timeoutms = 1000)
        {
            var sw = new Stopwatch();
            sw.Start();

            while (m_IsCapturing || m_IsConverting ||
                m_CaptureEventQueue.Count > 0 || m_SegmentCaptureEventQueue.Count > 0)
            {
                if (sw.Elapsed.TotalMilliseconds > timeoutms)
                    break;
            }

            m_CaptureEventQueue.Clear();
            m_SegmentCaptureEventQueue.Clear();

            Win32.ReleaseDC(IntPtr.Zero, m_ProcessDC);
            m_IntraFrameMat = null;
        }

        /// <summary>
        /// キャプチャループスレッド作成
        /// </summary>
        /// <param name="ms">キャプチャ間隔</param>
        private void StartCaptureLoop(double ms = 50)
        {
            Queue queue = new Queue();

            StartConvertLoop(queue);
            StartCaptureEventLoop();

            Task.Run(() =>
            {
                var capCnt = 0;
                var sw = new Stopwatch();
                sw.Start();

                while (IsCapturing)
                {
                    var rect = GetCaptureRect();

                    if (rect.Width == 0 || rect.Height == 0)
                    {
                        IsCapturing = false;
                        break;
                    }

                    var captureBitmap = GetCaptureBitmap(rect);

                    if (captureBitmap == null)
                    {
                        IsCapturing = false;
                        break;
                    }

                    var data = new CaptureData()
                    {
                        captureBitmap = captureBitmap,
                        captureRect = rect,
                        isIntraFrame = true,
                    };

                    m_CaptureEventQueue.Enqueue(data);
                    queue.Enqueue(data);

                    var sleepMs = (ms * capCnt) - sw.Elapsed.TotalMilliseconds;

                    if (sleepMs > 0)
                    {
                        Thread.Sleep((int)sleepMs);
                    }

                    capCnt++;
                }

                sw.Stop();

                m_IsCapturing = false;
            });
        }

        /// <summary>
        /// キャプチャ画像をJpegに変換するループスレッド作成
        /// </summary>
        /// <param name="queue"></param>
        private void StartConvertLoop(Queue queue)
        {
            StartSegCaptureEventLoop();

            Task.Run(() =>
            {
                while (true)
                {
                    if (!IsCapturing)
                    {
                        break;
                    }
                    else if (queue.Count > 0)
                    {
                        CaptureData cd = new CaptureData() { captureBitmap = null, };

                        while (queue.Count > 0)
                        {
                            cd = (CaptureData)queue.Dequeue();
                        }

                        SegConvert(cd.captureBitmap, cd.captureRect);
                    }
                }

                m_IsConverting = false;
            });
        }

        /// <summary>
        /// キャプチャイベントを発生させるループスレッド作成
        /// </summary>
        private void StartCaptureEventLoop()
        {
            Task.Run(() =>
            {
                while (m_IsCapturing || m_CaptureEventQueue.Count > 0)
                {
                    if (m_CaptureEventQueue.Count > 0)
                    {
                        lock (m_captureLocking)
                        {
                            Captured(this, (CaptureData)m_CaptureEventQueue.Dequeue());
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 分割画像のキャプチャイベントを発生させるループスレッド作成
        /// </summary>
        private void StartSegCaptureEventLoop()
        {
            Task.Run(() =>
            {
                while (m_IsConverting || m_SegmentCaptureEventQueue.Count > 0)
                {
                    if (m_SegmentCaptureEventQueue.Count > 0)
                    {
                        SegmentCaptured(this, (SegmentCaptureData)m_SegmentCaptureEventQueue.Dequeue());
                    }
                }
            });
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

        /// <summary>
        /// キャプチャ範囲のBitmap取得
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        private Bitmap GetCaptureBitmap(Rectangle rect)
        {
            CaptureSize = new Size(rect.Width, rect.Height);

            var captureBitmap = new Bitmap((int)(rect.Width * Scale), (int)(rect.Height * Scale));

            lock (m_captureLocking)
            {
                using (var captureGraphic = Graphics.FromImage(captureBitmap))
                {
                    var captureHDC = captureGraphic.GetHdc();

                    Win32.SetStretchBltMode(captureHDC, Win32.STRETCH_HALFTONE);
                    Win32.StretchBlt(captureHDC, 0, 0, captureBitmap.Width, captureBitmap.Height,
                        m_ProcessDC, rect.X, rect.Y, rect.Width, rect.Height, Win32.SRCCOPY | Win32.CAPTUREBLT);

                    captureGraphic.ReleaseHdc(captureHDC);
                }
            }

            return captureBitmap;
        }

        /// <summary>
        /// 分割数でキャプチャしたBitmapを分割し、イベントを発生させる
        /// </summary>
        /// <param name="bmp">キャプチャ画像</param>
        /// <param name="rect">分割サイズ</param>
        private void SegConvert(Bitmap bmp, Rectangle rect)
        {
            var firstIntra = false;
            Mat mat_capture;

            var mat_xor = new Mat();
            var mat_diff = new Mat();

            var segRect = rect;

            lock (m_captureLocking)
            {
                try
                {
                    mat_capture = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
                }
                catch
                {
                    return;
                }
                
                segRect.Width = bmp.Width / CaptureDivisionNum;
                segRect.Height = bmp.Height / CaptureDivisionNum;
            }

            if (m_IntraFrameMat == null)
            {
                m_IntraFrameMat = mat_capture;
                firstIntra = true;
            }
            else
            {
                try
                {
                    Cv2.BitwiseXor(mat_capture, m_IntraFrameMat, mat_xor);
                    Cv2.CvtColor(mat_xor, mat_diff, ColorConversionCodes.RGBA2GRAY);
                }
                catch
                {
                    return;
                }
                
            }

            for (int y = 0; y < CaptureDivisionNum; y++)
            {
                for (int x = 0; x < CaptureDivisionNum; x++)
                {
                    var segIdx = y * CaptureDivisionNum + x;

                    segRect.X = segRect.Width * x;
                    segRect.Y = segRect.Height * y;

                    var sRect = new Rect(segRect.X, segRect.Y, segRect.Width, segRect.Height);
                    var m_segDiff = !firstIntra ? mat_diff.SubMat(sRect) : null;
                    var nonZero = !firstIntra ? m_segDiff.CountNonZero() : 0;

                    if (firstIntra || nonZero != 0)
                    {
                        var m_segCapture = mat_capture.SubMat(sRect);
                        var img_buffer = m_segCapture.ImEncode(EncodeFormatExtension, m_EncodingParam);

                        var sData = new SegmentCaptureData()
                        {
                            segmentIdx = segIdx,
                            rect = segRect,
                            encodedFrameBuffer = img_buffer,
                        };

                        m_SegmentCaptureEventQueue.Enqueue(sData);

                        if (!firstIntra)
                        {
                            var m_segIntra = m_IntraFrameMat.SubMat(sRect);
                            m_segCapture.CopyTo(m_segIntra);
                        }
                    }
                }
            }
        }
    }
}
