using System;
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
            StartCapture,
            StopCapture,
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
    /// WebRTCのDataChannelで送信されるキャプチャデータのヘッダ
    /// </summary>
    struct FrameHeader
    {
        public BufferType type;
        public byte segmentIndex;
    }

    /// <summary>
    /// WebRTCのDataChannelで送信される音声データのヘッダ
    /// </summary>
    struct AudioHeader
    {
        public BufferType type;
        private byte dum1, dum2, dum3;
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
}
