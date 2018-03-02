using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace ScreenShare
{
    public interface IPackable
    {
        byte[] Pack();
    }

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
            Report,
            Reset,
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
    public enum DataType : byte
    {
        CapturedImage = 0x00,
        CapturedAudio = 0x01,
    }

    public class CommunicationData : IPackable
    {
        public CommunicationDataHeader Header { get; set; }
        public IPackable Body { get; set; }

        public byte[] Pack()
        {
            var packedBody = Body.Pack();
            Header.BodyChank.SetBodyLength(packedBody.Length);

            return ByteUtils.Concatenation(Header.Pack(), packedBody);
        }
    }

    public class CommunicationDataHeader : IPackable
    {
        public TimeChank TimeChank { get; set; }
        public BodyChank BodyChank { get; set; }

        public byte[] Pack()
        {
            return ByteUtils.Concatenation(TimeChank.Pack(), BodyChank.Pack());
        }
    }

    /// <summary>
    /// データを送信/受信した時刻を格納するチャンク
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TimeChank : IPackable
    {
        private uint totalms;
        private uint currentms;

        public TimeChank(int ms)
        {
            totalms = (uint)ms;
            currentms = (uint)ms;
        }

        public byte[] Pack()
        {
            return ByteUtils.GetBytesFromStructure(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BodyChank : IPackable
    {
        private DataType bodyType;
        private int bodyLength;

        public BodyChank(DataType type)
        {
            bodyType = type;
            bodyLength = -1;
        }

        public void SetBodyLength(int length)
        {
            bodyLength = length;
        }

        public byte[] Pack()
        {
            return ByteUtils.GetBytesFromStructure(this);
        }
    }


    public class CapturedImage : IPackable
    {
        private List<CapturedImageSegmentChank> capturedImageSegments = new List<CapturedImageSegmentChank>();

        public void AddSegment(CapturedImageSegmentChank segment)
        {
            capturedImageSegments.Add(segment);
        }

        public byte[] Pack()
        {
            var buf = BitConverter.GetBytes((short)capturedImageSegments.Count);
            foreach (var seg in capturedImageSegments)
            {
                buf = ByteUtils.Concatenation(buf, seg.Pack());
            }
            return buf;
        }
    }


    public class CapturedImageSegmentChank : IPackable
    {
        public Rect_s capturedRect { get; set; }
        public int capturedSize { get; set; }
        public byte[] capturedData { get; set; }

        public byte[] Pack()
        {
            var buf = ByteUtils.GetBytesFromStructure(capturedRect);
            buf = ByteUtils.Concatenation(buf, BitConverter.GetBytes(capturedSize));
            buf = ByteUtils.Concatenation(buf, capturedData);

            return buf;
        }
    }

    public class CapturedAudio : IPackable
    {
        public byte[] normalizedSamples { get; set; }

        public byte[] Pack()
        {
            return normalizedSamples;
        }
    }

    /// <summary>
    /// サイズ用
    /// </summary>
    public struct Vec2
    {
        public ushort x, y;

        public Vec2(int x, int y)
        {
            this.x = (ushort)x;
            this.y = (ushort)y;
        }
    }

    public struct Rect_s
    {
        public short x, y, width, height;

        public Rect_s(int x, int y, int w, int h)
        {
            this.x = (short)x;
            this.y = (short)y;
            this.width = (short)w;
            this.height = (short)h;
        }
    }

    public class Commons
    {
        public const string CheckContinuesMessage = "cp";
    }
}
