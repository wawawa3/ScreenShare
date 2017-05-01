using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace ScreenShare
{
    /// <summary>
    /// WebSocketでやり取りされるメッセージ構造
    /// </summary>
    struct MessageData
    {
        public enum Type : byte
        {
            Connected = 0x00,
            UpdateID,
            PeerConnection,
            SDPOffer,
            SDPAnswer,
            ICECandidateOffer,
            ICECandidateAnswer,
            RemoveOffer,
            RemoveAnswer,
            Settings,
            StartCasting,
            StopCasting,
            RequestReconnect,
            Disconnect,
        };

        /// <summary>
        /// メッセージタイプ
        /// </summary>
        public Type type { get; set; }

        /// <summary>
        /// クライアントID
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 対象のクライアントID
        /// </summary>
        public int targetId { get; set; }
        
        /// <summary>
        /// メッセージデータ
        /// </summary>
        public object data { get; set; }
    }

    /// <summary>
    /// 設定データ
    /// </summary>
    struct SettingData
    {
        /// <summary>
        /// 音声設定データ
        /// </summary>
        public struct AudioSettingData
        {
            /// <summary>
            /// 録音周波数
            /// </summary>
            public int sampleRate { get; set; }

            /// <summary>
            /// ステレオ録音かどうか
            /// </summary>
            public bool isStereo { get; set; }
        }

        /// <summary>
        /// キャプチャ設定データ
        /// </summary>
        public struct CaptureSettingData
        {
            /// <summary>
            /// 分割数
            /// </summary>
            public int divisionNum { get; set; }

            /// <summary>
            /// 画面アスペクト比
            /// </summary>
            public float aspectRatio { get; set; }

            /// <summary>
            /// キャプチャFPS
            /// </summary>
            public int framePerSecond { get; set; }

            public int width { get; set; }

            public int height { get; set; }
        }

        /// <summary>
        /// 音声設定データ
        /// </summary>
        public AudioSettingData audioSettingData;

        /// <summary>
        /// キャプチャ設定データ
        /// </summary>
        public CaptureSettingData captureSettingData;
    }

    /// <summary>
    /// WebRTCのDataChannelで送信されるデータの型
    /// </summary>
    public enum BufferType : byte
    {
        FrameBuffer = 0x00, AudioBuffer = 0x01,
    }

    /// <summary>
    /// WebRTCのDataChannelで送信されるデータの主ヘッダ
    /// 9 byte
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BufferHeader
    {
        public BufferType type { get; private set; }
        public int totalms { get; private set; }
        public int currentms { get; private set; }

        public BufferHeader(BufferType t, int ms)
        {
            type = t;
            //ticks = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);

            totalms = ms;
            currentms = ms;
        }
    }

    /// <summary>
    /// WebRTCのDataChannelで送信されるキャプチャデータのヘッダ
    /// 10 byte
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FrameHeader
    {
        public BufferHeader bufferheader { get; private set; }
        public byte segmentIndex { get; private set; }

        public FrameHeader(byte segIdx, int ms)
        {
            bufferheader = new BufferHeader(BufferType.FrameBuffer, ms);
            segmentIndex = segIdx;
        }
    }

    /// <summary>
    /// WebRTCのDataChannelで送信される音声データのヘッダ
    /// 12 byte
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AudioHeader
    {
        public BufferHeader bufferheader { get; private set; }
        public byte dum1, dum2, dum3;

        public AudioHeader(int ms)
        {
            bufferheader = new BufferHeader(BufferType.AudioBuffer, ms);
            dum1 = dum2 = dum3 = 0;
        }
    }

    /// <summary>
    /// サイズ用
    /// </summary>
    struct Vec2
    {
        public ushort x, y;

        public Vec2(int x, int y)
        {
            this.x = (ushort)x;
            this.y = (ushort)y;
        }
    }

    public class Commons
    {
        public const string CheckPacketIdentifier = "cp";
    }
}
