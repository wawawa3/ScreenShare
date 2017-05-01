﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System.Reflection;

using Alchemy;
using Alchemy.Classes;


using NAudio.Wave;

using SharpAvi.Codecs;
using SharpAvi.Output;
using Accord.Video;

using Newtonsoft.Json;

using ScreenShare.Properties;

namespace ScreenShare
{
    public partial class Form_ImageCast : Form
    {
        #region Member Variables

        /// <summary>
        /// 最大画質
        /// </summary>
        private const int MaxQuality = 100;

        /// <summary>
        /// 最小画質
        /// </summary>
        private const int MinQuality = 0;

        /// <summary>
        /// 最大フレームレート
        /// </summary>
        private const int MaxFps = 30;

        /// <summary>
        /// 最小フレームレート
        /// </summary>
        private const int MinFps = 1;

        /// <summary>
        /// 遅延報告上限回数
        /// </summary>
        private const int BusyIpCountLimit = 3;

        /// <summary>
        /// バッチファイル名
        /// </summary>
        private const string Batch_FirewallAdd = "batch\\firewall_add.bat";
        private const string Batch_FirewallDelete = "batch\\firewall_delete.bat";

        /// <summary>
        /// フォームスレッドで実行するためのデリゲート
        /// </summary>
        public delegate void FormDelegate();
        public delegate T FormDelegate<T>();

        /// <summary>
        /// キャプチャ領域選択用フォーム
        /// </summary>
        private Form_CaptureArea m_FormCapture = new Form_CaptureArea();

        /// <summary>
        /// キャプチャ領域オーバーレイ用フォーム
        /// </summary>
        private Form_OverRay m_FormOverRay = new Form_OverRay();

        /// <summary>
        /// キャプチャ用インスタンス
        /// </summary>
        private Capture m_Capture = new Capture();

        /// <summary>
        /// HTTPサーバインスタンス
        /// </summary>
        private HttpServer m_HttpServer;

        /// <summary>
        /// WebSocketサーバインスタンス
        /// </summary>
        private WebSocketServer m_WebSocketServer;

        /// <summary>
        /// 木構造
        /// </summary>
        private Tree<UserContext> m_WebSocketClients = new Tree<UserContext>();

        /// <summary>
        /// 遅延報告を受けたIPアドレスの報告回数
        /// </summary>
        private Dictionary<string, int> m_BusyIpCount = new Dictionary<string, int>();

        /// <summary>
        /// 遅延しているノードリスト
        /// </summary>
        private List<EndPoint> m_BusyClientIpAddress = new List<EndPoint>();

        /// <summary>
        /// サーバの状態
        /// </summary>
        private bool m_ServerRunning = false;

        /// <summary>
        /// 配信を開始した時刻(ns)
        /// </summary>
        private int m_CastingStartMilliseconds = 0;

        /// <summary>
        /// 音声録音インスタンス
        /// </summary>
        private WaveIn m_Recorder = new WaveIn();

        /// <summary>
        /// AVIファイル名
        /// </summary>
        private string m_VideoFilePath = "";

        /// <summary>
        /// キャプチャ領域
        /// </summary>
        private Rectangle m_CaptureBounds = Screen.PrimaryScreen.Bounds;

        /// <summary>
        /// 最新の分割画面
        /// </summary>
        private byte[][] m_LastFrameBuffer;

        /// <summary>
        /// 最後に配信される画像がキャプチャされたアプリケーション時刻(ms)
        /// </summary>
        private int m_LatestCapturedTime;

        /// <summary>
        /// プログラムが初期化されたアプリケーション時刻(ms)
        /// </summary>
        private DateTime m_InitializedLocalDate;

        /// <summary>
        /// プログラムが初期化された時刻(ms)(ネットワーク)
        /// </summary>
        private int m_InitializedNetworkTime;


        private Process ffconv = null;
        private NamedPipeServerStream ffpipein_video = null, ffpipein_audio = null;

        #endregion

        /// <summary>
        /// 一日内の合計ミリ秒
        /// </summary>
        private int NowMilliseconds
        {
            get
            {
                /*
                var ms_diff = (int)(DateTime.UtcNow - m_InitializedLocalDate).TotalMilliseconds;
                return m_InitializedNetworkTime + ms_diff;*/
                return (int)(DateTime.UtcNow - m_InitializedLocalDate).TotalMilliseconds;
            }
        }

        public Form_ImageCast()
        {
            #region Application Initialization

            try
            {
                ProcessStartInfo info = new ProcessStartInfo(Batch_FirewallAdd);
                info.UseShellExecute = true;
                info.Verb = "runas";

                var p = Process.Start(info);
                p.WaitForExit();
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(Resources.BatchNotFound + "\n\"" + Batch_FirewallAdd + "\"", Resources.CaptionError);
                Debug.Log("App", ex.Message + Batch_FirewallAdd);
            }
            catch
            {
                MessageBox.Show(Resources.BatchFailed + "\n\"" + Batch_FirewallAdd + "\"", Resources.CaptionError);
            }

            Debug.Writer = Console.Out;
            
            Application.ApplicationExit += (s, e) =>
            {
                try
                {
                    ProcessStartInfo info = new ProcessStartInfo(Batch_FirewallDelete);
                    info.UseShellExecute = true;
                    info.Verb = "runas";

                    var p = Process.Start(info);
                    p.WaitForExit();
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show(Resources.BatchNotFound + "\n\"" + Batch_FirewallDelete + "\"", Resources.CaptionError);
                    Debug.Log("App", ex.Message);
                }
                catch
                {
                    MessageBox.Show(Resources.BatchFailed + "\n\"" + Batch_FirewallDelete + "\"", Resources.CaptionError);
                }

                Debug.Log("App", "application exit");
                Debug.Terminate();
            };

            #endregion

            #region Form Initialization

            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(Settings.Default.SpecificCulture);

            InitializeComponent();

            InitializeForm();

            m_FormCapture.Selected += (rect) =>
            {
                m_CaptureBounds = rect;

                m_FormCapture.Hide();

                m_FormOverRay.CaptureBounds = rect;

                if (checkBox_showOverRayForm.Checked)
                    m_FormOverRay.Show();

                this.Enabled = true;
            };

            #endregion

            #region Capture

            m_Capture.OnCaptured += (s, data) =>
            {
                if (m_WebSocketClients.ContainsKey(0))
                {
                    // delay check packet
                    m_WebSocketClients[0].Send(Commons.CheckPacketIdentifier);
                }
            };

            m_Capture.OnSegmentCaptured += (s, data) =>
            {
                m_LatestCapturedTime = NowMilliseconds;

                var frameHeader = new FrameHeader((byte)data.segmentIdx, m_LatestCapturedTime);
                var headerBuffer = ByteUtils.GetBytesFromStructure(frameHeader);
                var buffer = ByteUtils.Concatenation(headerBuffer, data.encodedFrameBuffer);
                //Console.WriteLine("net:"+MillisecondsOfDay+", local:"+TimeUtils_HMSM.GetTotalMilliseconds(DateTime.UtcNow));
                //Console.WriteLine("");
                if (m_WebSocketClients.ContainsKey(0))
                {
                    m_WebSocketClients[0].Send(buffer);
                        
                }

                m_LastFrameBuffer[data.segmentIdx] = (byte[])buffer.Clone();
            };

            m_Capture.OnError += (s, ex) =>
            {
                label_message.Text = Resources.CaptureError + " : " + ex.Message;
            };

            m_Capture.OnCapturedBitmap += (s, bmp) =>
            {
                /*lock (lockObj)
                    bmp.Save(stream.BaseStream, System.Drawing.Imaging.ImageFormat.Jpeg);*/

            };

            #endregion

            #region Voice Recorder

            //m_Recorder.BufferMilliseconds = 100;
            m_Recorder.DataAvailable += (s, we) =>
            {
                var normalizedSampleBuffer = new byte[we.BytesRecorded / 2 * 4];

                for (int i = 0, j = 0; i < we.BytesRecorded; i += 2)
                {
                    short sample = (short)((we.Buffer[i + 1] << 8) | we.Buffer[i + 0]);
                    float s32 = sample / 32768f;
                    var sampleByte = BitConverter.GetBytes(s32);

                    normalizedSampleBuffer[j++] = sampleByte[0];
                    normalizedSampleBuffer[j++] = sampleByte[1];
                    normalizedSampleBuffer[j++] = sampleByte[2];
                    normalizedSampleBuffer[j++] = sampleByte[3];
                }

                var frameHeader = new AudioHeader(NowMilliseconds);
                var headerBuffer = ByteUtils.GetBytesFromStructure(frameHeader);
                var buffer = ByteUtils.Concatenation(headerBuffer, normalizedSampleBuffer);

                if (checkBox_captureVoice.Checked)
                {
                    if (m_WebSocketClients.ContainsKey(0))
                    {
                        m_WebSocketClients[0].Send(buffer);
                    }
                }

                if (ffconv == null || ffpipein_audio == null || !ffpipein_audio.IsConnected) return;
                ffpipein_audio.Write(we.Buffer, 0, we.Buffer.Length);
            };

            #endregion

            #region WebSocket Server

            Action<UserContext> addClient = (UserContext ctx) =>
            {
                int id, parentId;
                UserContext parent;

                MessageData data;
                string json;

                Debug.Log("WebSocket", "----------------- Connection  ------------");

                if (m_WebSocketClients.Where(c => c.Value.ClientAddress.ToString() == ctx.ClientAddress.ToString()).Count() != 0)
                {
                    Debug.Log("WebSocket", "the same IP Address already exist : " + ctx.ClientAddress);
                    return;
                }

                for (id = 0; m_WebSocketClients.Keys.Contains(id); id++) ;

                Debug.Log("WebSocket", "connected id : " + id);

                try
                {
                    try
                    {
                        parentId = m_WebSocketClients.GetParentKey(id);
                        parent = m_WebSocketClients[parentId];
                    }
                    catch
                    {
                        parentId = -1;
                        parent = null;
                    }

                    var childrenId = m_WebSocketClients.GetChildrenKey(id);
                    

                    data = new MessageData { type = MessageData.Type.Connected, id = id, data = NowMilliseconds };
                    json = JsonConvert.SerializeObject(data);

                    ctx.Send(json);

                    if (m_Capture.Capturing)
                        SendStartCastingMessage(ctx, true);

                    if (parent != null)
                    {
                        data = new MessageData { type = MessageData.Type.PeerConnection, targetId = id, };
                        json = JsonConvert.SerializeObject(data);
                        parent.Send(json);

                        Debug.Log("WebSocket", "parent : " + parentId);
                    }

                    Parallel.ForEach(childrenId, cId =>
                    {
                        data = new MessageData { type = MessageData.Type.PeerConnection, targetId = cId, };
                        json = JsonConvert.SerializeObject(data);
                        ctx.Send(json);

                        Debug.Log("WebSocket", "child : " + cId);
                    });

                    m_WebSocketClients[id] = ctx;
                }
                catch (Exception e)
                {
                    Debug.Log("WebSocket", "Connected Error : " + e.Message);
                }

                Debug.Log("WebSocket", "-----------------------------------------------");

                ctx.MaxFrameSize = 0x80000;
            };

            Action<UserContext> deleteClient = (UserContext ctx) =>
            {
                int rmId,
                    updatedId,
                    rmParentId,
                    lastId,
                    lastParentId;
                int[] rmChildrenId;

                UserContext rm, updated, rmParent, last, lastParent;
                UserContext[] rmChildren;

                MessageData data;
                string json;

                Debug.Log("WebSocket", "----------------- Disconnection  ------------");
                rm = ctx;

                // 削除するノードIDを検索
                try
                {
                    rmId = m_WebSocketClients.TryGetKey(rm);
                }
                catch
                {
                    Debug.Log("WebSocket", "can't find user");
                    Debug.Log("WebSocket", "-----------------------------------------------");
                    return;
                }

                Debug.Log("WebSocket", "disconnect node id : " + rmId);

                // 削除するノードの親を検索
                try
                {
                    rmParentId = m_WebSocketClients.GetParentKey(rmId);
                    rmParent = m_WebSocketClients[rmParentId];
                }
                catch
                {
                    rmParentId = -1;
                    rmParent = null;
                }

                // 削除するノードの子を検索
                rmChildrenId = m_WebSocketClients.GetChildrenKey(rmId);
                rmChildren = m_WebSocketClients.GetValues(rmChildrenId);

                // 入れ替え元ノードを検索
                var notBusyClients = m_WebSocketClients.Where(c => !m_BusyClientIpAddress.Contains(c.Value.ClientAddress));
                if (notBusyClients.Count() < 1)
                    lastId = m_WebSocketClients.GetLastKey();
                else
                    lastId = notBusyClients.Last().Key;

                if (lastId < rmId) lastId = rmId;
                last = m_WebSocketClients[lastId];

                Debug.Log("WebSocket", "replace node id : " + lastId);

                // 入れ替え元ノードの親を検索
                try
                {
                    lastParentId = m_WebSocketClients.GetParentKey(lastId);
                    lastParent = m_WebSocketClients[lastParentId];
                }
                catch
                {
                    lastParentId = -1;
                    lastParent = null;
                }

                try
                {
                    if (rmParent != null)
                    {
                        data = new MessageData { type = MessageData.Type.RemoveOffer, id = rmId, };
                        json = JsonConvert.SerializeObject(data);
                        rmParent.Send(json);
                    }

                    data = new MessageData { type = MessageData.Type.RemoveAnswer, id = rmId, };
                    json = JsonConvert.SerializeObject(data);
                    Parallel.ForEach(rmChildren, child => child.Send(json));

                    if (lastId == rmId)
                    {
                        m_WebSocketClients.Remove(lastId);
                        Debug.Log("WebSocket", "replace Id == disconnect Id");
                        Debug.Log("WebSocket", "-----------------------------------------------");
                        return;
                    }

                    if (lastParentId != rmId)
                    {
                        data = new MessageData { type = MessageData.Type.RemoveOffer, id = lastId };
                        json = JsonConvert.SerializeObject(data);
                        lastParent.Send(json);
                    }

                    data = new MessageData { type = MessageData.Type.RemoveAnswer, id = lastParentId, };
                    json = JsonConvert.SerializeObject(data);
                    last.Send(json);

                    m_WebSocketClients[rmId] = m_WebSocketClients[lastId];
                    m_WebSocketClients.Remove(lastId);

                    updatedId = rmId;
                    updated = m_WebSocketClients[rmId];

                    data = new MessageData { type = MessageData.Type.UpdateID, id = rmId, };
                    json = JsonConvert.SerializeObject(data);
                    updated.Send(json);

                    Debug.Log("WebSocket", "replace node Id : " + lastId + " => disconnect node Id : " + rmId);

                    if (rmParent != null)
                    {
                        data = new MessageData { type = MessageData.Type.PeerConnection, targetId = updatedId, };
                        json = JsonConvert.SerializeObject(data);
                        rmParent.Send(json);
                    }

                    rmChildrenId = m_WebSocketClients.GetChildrenKey(rmId);
                    rmChildren = m_WebSocketClients.GetValues(rmChildrenId);

                    Parallel.ForEach(rmChildrenId, cId =>
                    {
                        data = new MessageData { type = MessageData.Type.PeerConnection, targetId = cId, };
                        json = JsonConvert.SerializeObject(data);
                        updated.Send(json);
                    });

                    Debug.Log("WebSocket", "-----------------------------------------------");
                }
                catch (Exception e)
                {
                    Debug.Log("WebSocket", "Disconnection Error : " + e.Message);
                }
            };

            m_WebSocketServer = new WebSocketServer(Settings.Default.Port_WebSocket)
            {
                OnConnected = (ctx) =>
                {
                    if (!m_ServerRunning) return;
                    lock (m_WebSocketServer) 
                        addClient(ctx);

                    Invoke(new FormDelegate(() => { this.label_ConnectionNum.Text = "" + m_WebSocketClients.Count; }));
                },
                OnDisconnect = (ctx) =>
                {
                    if (!m_ServerRunning) return;
                    lock (m_WebSocketServer)
                        deleteClient(ctx);

                    Invoke(new FormDelegate(() => { this.label_ConnectionNum.Text = "" + m_WebSocketClients.Count; }));
                },
                OnReceive = (ctx) =>
                {
                    if (!m_ServerRunning) return;

                    //Debug.Log("WebSocket", "Receive : " + ctx.ClientAddress);
                    lock (m_WebSocketServer)
                    {
                        try
                        {
                            var json = ctx.DataFrame.ToString();
                            if (json[0] == 'T') return;

                            var recv = JsonConvert.DeserializeObject<MessageData>(json);
                            try
                            {


                                switch (recv.type)
                                {
                                    case MessageData.Type.SDPOffer:
                                    case MessageData.Type.SDPAnswer:
                                    case MessageData.Type.ICECandidateOffer:
                                    case MessageData.Type.ICECandidateAnswer:
                                        {
                                            Debug.Log("WebSocket", "Relay. Type = " + recv.type + ", From = " + recv.id + ", To = " + recv.targetId);
                                            var dest = m_WebSocketClients[recv.targetId];
                                            dest.Send(json);
                                        }
                                        break;

                                    case MessageData.Type.RequestReconnect:
                                        { 
                                            if (recv.targetId == -1)
                                            {
                                                Debug.Log("WebSocket", "failed to Send");
                                                return;
                                            }
                                            var dest = m_WebSocketClients[recv.targetId];
                                            dest.Send(json);

                                            var ip = dest.ClientAddress.ToString();
                                            if (!m_BusyIpCount.ContainsKey(ip))
                                                m_BusyIpCount[ip] = 0;
                                            m_BusyIpCount[ip]++;

                                            if (m_BusyIpCount[ip] == BusyIpCountLimit)
                                            {
                                                Debug.Log("WebSocket", "add [" + dest.ClientAddress + "] to Busy Client List");
                                                m_BusyClientIpAddress.Add(dest.ClientAddress);
                                            }
                                                

                                            deleteClient(dest);
                                            addClient(dest);
                                        }
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.Log("WebSocket", "Receive Error : " + e.Message+ ", Type = "+ recv.type + ", From = "+ recv.id +", To = "+recv.targetId);
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log("WebSocket", "Deserialize Error : " + e.Message);
                            Debug.Log("WebSocket", "Data:" + ctx.DataFrame.ToString());
                        }
                    }
                },
                TimeOut = new TimeSpan(0, 5, 0),
            };

            #endregion

            #region HTTP Server

            m_HttpServer = new HttpServer()
            {
                Port = Settings.Default.Port_HTTP,
                DocumentRootPath = Settings.Default.DocumentPath,
                //TopPage = "/index_"+ Settings.Default.SpecificCulture + ".html",
            };
            m_HttpServer.OnError += (s, e) =>
            {
                Debug.Log("HTTP", e.Message);
            };

            #endregion

            #region NTPSync

            /*
            DateTime tmp = DateTime.UtcNow;
            var client = new Yort.Ntp.NtpClient("calc.cis.ibaraki.ac.jp");// Settings.Default.NtpServer);
            client.TimeReceived += (s, e) =>
            {
                var elapsed = (int)(DateTime.UtcNow - tmp).TotalMilliseconds / 2;
                m_InitializedLocalDate = DateTime.UtcNow;
                m_InitializedNetworkTime = TimeUtils_HMSM.GetTotalMilliseconds(e.CurrentTime) - elapsed;

                Console.WriteLine("InitializedLocalTime :" + TimeUtils_HMSM.GetTotalMilliseconds(m_InitializedLocalDate));
                Console.WriteLine("InitializedNetworkTime :" + m_InitializedNetworkTime);
                Console.WriteLine("NTP Delay:" + elapsed);
            };
            client.ErrorOccurred += (s, e) =>
            {
                Console.WriteLine(e.Exception);
            };
            tmp = DateTime.UtcNow;
            client.BeginRequestTime();

            using (var client = new WebClient())
            {
                var tmp = DateTime.UtcNow;
                var timestr = client.DownloadString(Settings.Default.NtpHttpUrl);
                var elapsed = (int)(DateTime.UtcNow - tmp).TotalMilliseconds / 2;
                timestr = timestr.Substring(0, timestr.Length - 1).Replace(".", "");
                var lt = Int64.Parse(timestr);
                var timespan = TimeSpan.FromMilliseconds(lt - elapsed);
                Console.WriteLine("timespan:"+ timestr);

                m_InitializedLocalDate = DateTime.UtcNow;
                m_InitializedNetworkTime = TimeUtils_HMSM.GetTotalMilliseconds(timespan);

                Console.WriteLine("InitializedLocalTime :" + m_InitializedLocalDate);
                Console.WriteLine("InitializedNetworkTime :" + m_InitializedNetworkTime);
                Console.WriteLine("NTP Delay:" + elapsed);
            }*/

            m_InitializedLocalDate = DateTime.UtcNow;

            #endregion


            //var ntpDateTime = DateTime.Now.FromNtp();

            Debug.Log("App", "application started.");
        }

        /// <summary>
        /// Formコンポーネントの初期値設定
        /// </summary>
        private void InitializeForm()
        {
            //this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            //this.Validate();

            ReloadDisplays();
            ReloadRunningProcesses();
            ReloadWaveInDevices();

            var ipentry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in ipentry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    textBox_ip.Text = ip.ToString();
                    break;
                }
            }

            /*
            comboBox_targetDisplay.SelectedIndex = 0;
            comboBox_process.SelectedIndex = 0;
            comboBox_waveInDevices.SelectedIndex = 0;
            */
            var display = Screen.AllScreens[comboBox_targetDisplay.SelectedIndex];
            m_FormOverRay.Display = display;
            m_FormCapture.Display = display;

            comboBox_audioQuality.SelectedIndex = 1;
            comboBox_captureScale.SelectedIndex = Settings.Default.Scaling;
            textBox_captureFps.Text = Settings.Default.FramePerSecond;
            comboBox_divisionNumber.SelectedIndex = Settings.Default.Division;

            textBox_captureQuality.Text = "" + Settings.Default.ImageQuality;
        }

        /// <summary>
        /// HTTPサーバとWebSocketサーバの立ち上げ
        /// </summary>
        private void Connect()
        {
            try
            {
                m_HttpServer.Start();
                m_WebSocketServer.Restart();
            }
            catch (Exception e)
            {
                Debug.Log("App", "failed to start servers : " + e.Message);
                return;
            }
            

            m_ServerRunning = true;

            Debug.Log("App", "HTTP & WebSocket server started.");
            Debug.Log("HTTP", "home directory : " + m_HttpServer.TopPage);
        }

        /// <summary>
        /// HTTPサーバとWebSocketサーバの停止/切断
        /// </summary>
        private void Disconnect()
        {
            if (m_Capture.Capturing)
                StopCapture();

            if (m_ServerRunning)
            {
                m_ServerRunning = false;

                var data = new MessageData { type = MessageData.Type.Disconnect };
                var json = JsonConvert.SerializeObject(data);
                foreach (var pair in m_WebSocketClients)
                    pair.Value.Send(json);

                m_WebSocketClients.Clear();
                m_BusyIpCount.Clear();
                m_BusyClientIpAddress.Clear();

                try
                {
                    m_WebSocketServer.Stop();
                    m_HttpServer.Stop();

                    this.label_ConnectionNum.Text = "0";
                }
                catch (Exception e)
                {
                    Debug.Log("App", "failed to close servers : " + e.Message);
                    return;
                }
            }

            Invoke(new FormDelegate(() => m_FormOverRay.Hide()));

            Debug.Log("App", "HTTP & WebSocket server closed.");
        }

        /// <summary>
        /// ディスプレイの再読み込み
        /// </summary>
        private void ReloadDisplays()
        {
            var sel = comboBox_targetDisplay.SelectedIndex < 0 ? 0 : comboBox_targetDisplay.SelectedIndex;
            var data = Screen.AllScreens;
            comboBox_targetDisplay.DataSource = data;
            comboBox_targetDisplay.DisplayMember = "DeviceName";
            using (var g = comboBox_process.CreateGraphics())
                comboBox_targetDisplay.DropDownWidth =data.Max(s => (int)g.MeasureString(s.DeviceName, comboBox_targetDisplay.Font).Width);
            if (comboBox_targetDisplay.Items.Count != 0)
                comboBox_targetDisplay.SelectedIndex = Math.Min(sel, comboBox_targetDisplay.Items.Count - 1);
        }

        /// <summary>
        /// プロセスの再読み込み
        /// </summary>
        private void ReloadRunningProcesses()
        {
            var sel = comboBox_process.SelectedIndex < 0 ? 0 : comboBox_process.SelectedIndex;
            var data = Process.GetProcesses().Where((p) => { return p.MainWindowHandle != IntPtr.Zero && p.MainWindowTitle.Length != 0; }).ToArray();
            comboBox_process.DataSource = data;
            comboBox_process.DisplayMember = "MainWindowTitle";
            using (var g = comboBox_process.CreateGraphics())
                comboBox_process.DropDownWidth = data.Max(p => (int)g.MeasureString(p.MainWindowTitle, comboBox_process.Font).Width);
            if (comboBox_process.Items.Count != 0)
                comboBox_process.SelectedIndex = Math.Min(sel, comboBox_process.Items.Count - 1);
        }

        /// <summary>
        /// 録音デバイスの再読み込み
        /// </summary>
        private void ReloadWaveInDevices()
        {
            groupBox_voice.Enabled = WaveIn.DeviceCount > 0;
            
            if (WaveIn.DeviceCount <= 0)
                return;

            var sel = comboBox_waveInDevices.SelectedIndex < 0 ? 0 : comboBox_waveInDevices.SelectedIndex;
            var data = Enumerable.Range(0, WaveIn.DeviceCount).Select(n => WaveIn.GetCapabilities(n)).ToArray();
            comboBox_waveInDevices.DataSource = data;
            comboBox_waveInDevices.DisplayMember = "ProductName";
            using (var g = comboBox_process.CreateGraphics())
                comboBox_waveInDevices.DropDownWidth = data.Max(d => (int)g.MeasureString(d.ProductName, comboBox_waveInDevices.Font).Width);
            if (comboBox_waveInDevices.Items.Count != 0)
                comboBox_waveInDevices.SelectedIndex = Math.Min(sel, comboBox_waveInDevices.Items.Count - 1);
        }

        private bool PrepareCapturing()
        {
            float captureScale = 1.0f;
            int captureDivisionNum = 0,
                captureEncordingQuality = 0,
                captureFps = 0,
                audioSampleRate = 0,
                audioBps = 0,
                audioChannels = 0;


            if (radioButton_process.Checked)
            {
                var process = (Process)comboBox_process.SelectedItem;

                if (process.HasExited)
                {
                    MessageBox.Show(Resources.ProcessNotFound);
                    return false;
                }

                m_Capture.CaptureTarget = CaptureTarget.Process;
                m_Capture.CaptureProcess = process;
            }
            else
            {
                m_Capture.CaptureTarget = CaptureTarget.Desktop;
                m_Capture.CaptureDisplay = Screen.AllScreens[comboBox_targetDisplay.SelectedIndex];
                m_Capture.CaptureBounds = m_CaptureBounds;
                //Console.WriteLine(m_Capture.CaptureDisplay.DeviceName);
            }

            captureScale = (1.0f - (float)comboBox_captureScale.SelectedIndex / 10);
            captureDivisionNum = comboBox_divisionNumber.SelectedIndex + 1;
            captureEncordingQuality = Convert.ToInt32(textBox_captureQuality.Text);
            captureFps = Convert.ToInt32(textBox_captureFps.Text);

            audioSampleRate = Convert.ToInt32(comboBox_audioQuality.SelectedItem);
            audioBps = 16;
            audioChannels = checkBox_stereo.Enabled ? (checkBox_stereo.Checked ? 2 : 1) : 1;

            m_Capture.Scale = captureScale;
            m_Capture.CaptureDivisionNum = captureDivisionNum;
            m_Capture.EncoderQuality = captureEncordingQuality;
            m_Capture.FramesPerSecond = captureFps;

            m_Recorder.DeviceNumber = comboBox_waveInDevices.SelectedIndex;
            m_Recorder.WaveFormat = new WaveFormat(audioSampleRate, audioBps, audioChannels);

            m_LastFrameBuffer = new byte[m_Capture.CaptureDivisionNum * m_Capture.CaptureDivisionNum][];

            return true;
        }

        private bool PrepareRecording()
        {
            var dialog = new SaveFileDialog()
            {
                FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss.mp4"),
                Filter = Resources.MP4Filter,
                InitialDirectory = Directory.GetCurrentDirectory(),
                RestoreDirectory = true,
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                m_VideoFilePath = dialog.FileName;
            else
            {
                Debug.Log("Recoder", "Failed to Start Recording.");
                return false;
            }

            m_Capture.OnStarted += StartRecoder;
            m_Capture.OnCaptured += PushCapturedBuffer;
            m_Capture.OnStopped += TerminateRecoder;

            return true;
        }

        /// <summary>
        /// キャプチャ開始
        /// </summary>
        private void StartCapturing()
        {
            //m_CastingStartMilliseconds = TimeUtils.Milliseconds;
            SendStartCastingMessage();

            m_Capture.Start();

            if (checkBox_captureVoice.Checked || checkBox_recordAudio.Checked)
            {
                m_Recorder.StartRecording();
            }

            Debug.Log("App", "casting started.");
        }

        /// <summary>
        /// 画面キャプチャ(音声録音)の停止
        /// </summary>
        private async void StopCapture()
        {
            await Task.Run(() => m_Capture.Stop());

            Parallel.ForEach(m_WebSocketClients, pair =>
            {
                var data = new MessageData { type = MessageData.Type.StopCasting };
                var json = JsonConvert.SerializeObject(data);

                pair.Value.Send(json);
            });

            if (checkBox_recordVideo.Enabled && checkBox_recordVideo.Checked)
            {
                if (checkBox_captureVoice.Checked || checkBox_recordAudio.Checked)
                {
                    m_Recorder.StopRecording();
                }

                m_Capture.OnStarted -= StartRecoder;
                m_Capture.OnCaptured -= PushCapturedBuffer;
                m_Capture.OnStopped -= TerminateRecoder;
            }
            

            Debug.Log("App", "casting stopped.");
        }

        private void SendStartCastingMessage(UserContext ctx = null, bool sendLastFrame = false)
        {
            var setting = new SettingData
            {
                audioSettingData = new SettingData.AudioSettingData
                {
                    sampleRate = m_Recorder.WaveFormat.SampleRate,
                    isStereo = m_Recorder.WaveFormat.Channels == 2,
                },
                captureSettingData = new SettingData.CaptureSettingData
                {
                    divisionNum = m_Capture.CaptureDivisionNum,
                    aspectRatio = m_Capture.AspectRatio,
                    framePerSecond = m_Capture.FramesPerSecond,
                    width = m_Capture.DestinationSize.Width,
                    height = m_Capture.DestinationSize.Height,
                },
            };
            var data = new MessageData { type = MessageData.Type.Settings, data = setting };
            var json = JsonConvert.SerializeObject(data);

            if (ctx != null)
                ctx.Send(json);
            else
            {
                Parallel.ForEach(m_WebSocketClients, pair => pair.Value.Send(json));
            }

            data = new MessageData { type = MessageData.Type.StartCasting };//, data = TimeUtils.Milliseconds - m_CastingStartMilliseconds };
            json = JsonConvert.SerializeObject(data);

            if (ctx != null)
                ctx.Send(json);
            else
            {
                Parallel.ForEach(m_WebSocketClients, pair => pair.Value.Send(json));
            }

            if (!sendLastFrame) return;

            if (ctx != null)
            {
                foreach (var bytes in m_LastFrameBuffer)
                {
                    if (bytes != null)
                        ctx.Send(bytes);
                }
            }
            else
            {
                foreach (var bytes in m_LastFrameBuffer)
                {
                    if (bytes != null)
                    {
                        Parallel.ForEach(m_WebSocketClients, pair => pair.Value.Send(bytes));
                    }
                }
                
            }
            
        }

        private void StartRecoder(object sender, EventArgs e)
        {
            ffpipein_video = new NamedPipeServerStream("ffpipein_video", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            var psInfo = new ProcessStartInfo();
            if (checkBox_recordAudio.Enabled && checkBox_recordAudio.Checked)
            {
                ffpipein_audio = new NamedPipeServerStream("ffpipein_audio", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                psInfo.Arguments = String.Format("-y " +
                    @"-f rawvideo -pix_fmt bgr24 -s {0}x{1} -r {4} -vcodec rawvideo -i {8} " +
                    @"-f s16le -sample_rate {5} -channels {6} -channel_layout {7} -acodec pcm_s16le -i {9} " +
                    @"-movflags empty_moov+omit_tfhd_offset+frag_keyframe+default_base_moof+faststart " +
                    @"-f mp4 -vcodec libx264 -pix_fmt yuv420p -vf scale={2}:{3} -preset fast " +
                    @"-acodec libmp3lame -strict unofficial " +
                    "-map 0:v:0 -map 1:a:0 -threads 0 \"{10}\"",
                    m_Capture.DestinationSize.Width, m_Capture.DestinationSize.Height,
                    m_Capture.CaptureBounds.Width, m_Capture.CaptureBounds.Height,
                    m_Capture.FramesPerSecond,
                    m_Recorder.WaveFormat.SampleRate,
                    m_Recorder.WaveFormat.Channels,
                    (m_Recorder.WaveFormat.Channels == 1 ? "mono" : "stereo"),
                    @"\\.\pipe\ffpipein_video", @"\\.\pipe\ffpipein_audio",
                    m_VideoFilePath);
            }
            else
            {
                psInfo.Arguments = String.Format("-y " +
                     @"-f rawvideo -pix_fmt bgra -s {0}x{1} -r {4} -vcodec rawvideo -i {5} " +
                     @"-movflags empty_moov+omit_tfhd_offset+frag_keyframe+default_base_moof+faststart " +
                     @"-f mp4 -vcodec libx264 -pix_fmt yuv420p -vf scale={2}:{3} -preset fast " +
                     "-map 0:v:0 -threads 0 \"{6}\"",
                     m_Capture.DestinationSize.Width, m_Capture.DestinationSize.Height,
                     m_Capture.CaptureBounds.Width, m_Capture.CaptureBounds.Height,
                     m_Capture.FramesPerSecond,
                     @"\\.\pipe\ffpipein_video", m_VideoFilePath);
            }

            Debug.Log("ff", psInfo.Arguments);
            psInfo.FileName = "ffmpeg.exe";
            psInfo.CreateNoWindow = false;
            psInfo.UseShellExecute = false;
            psInfo.RedirectStandardInput = true;

            ffconv = Process.Start(psInfo);
            ffpipein_video.WaitForConnection();
            if (checkBox_recordAudio.Enabled && checkBox_recordAudio.Checked)
                ffpipein_audio.WaitForConnection();

            ffconv.StandardInput.AutoFlush = true;
        }

        private void PushCapturedBuffer(object sender, CaptureData data)
        {
            var buf = ByteUtils.GetBytesFromPtr(data.captureData, data.captureSize.Width * data.captureSize.Height * (data.bitCount / 8));
            ffpipein_video.Write(buf, 0, buf.Length);
        }

        private void TerminateRecoder(object sender, EventArgs e)
        {
            try
            {
                if (ffconv != null)
                {
                    ffconv.StandardInput.WriteLine("q");
                    ffconv.Close();
                }

                if (ffpipein_video != null)
                {
                    ffpipein_video.Close();
                    ffpipein_video.Dispose();
                }

                if (ffpipein_audio != null)
                {
                    ffpipein_audio.Close();
                    ffpipein_audio.Dispose();
                }

                ffconv = null;
                ffpipein_video = ffpipein_audio = null;
            }
            catch (Exception ex)
            {
                Debug.Log("Record", ex.Message);
            }
        }

        #region Form Components Callback

        private async void button_connect_Click(object sender, EventArgs e)
        {
            label_message.Text = Resources.ServerStarting;

            textBox_ip.Enabled = false;
            button_connect.Enabled = false;

            var res = await Task.Run<Exception>(() =>
            {
                try
                {
                    Connect();
                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            });

            if (res != null)
            {
                label_message.Text = Resources.ServerFailed + " : " + res.Message;

                textBox_ip.Enabled = true;
                button_connect.Enabled = true;

                return;
            }

            button_disconnect.Enabled = true;
            panel_capture.Enabled = true;
            button_startCapture.Enabled = true;
            checkBox_recordVideo.Enabled = true;

            label_message.Text = Resources.ServerStarted;
        }

        private void button_disconnect_Click(object sender, EventArgs e)
        {
            label_message.Text = Resources.ServerClosing;

            button_disconnect.Enabled = false;
            button_startCapture.Enabled = false;
            button_stopCapture.Enabled = false;
            panel_capture.Enabled = false;

            Disconnect();

            textBox_ip.Enabled = true;
            button_connect.Enabled = true;

            label_message.Text = Resources.ServerClosed;
        }

        private void comboBox_targetDisplay_Click(object sender, EventArgs e)
        {
            ReloadDisplays();
        }

        private void comboBox_processes_Click(object sender, EventArgs e)
        {
            ReloadRunningProcesses();
        }

        private void comboBox_waveInDevices_Click(object sender, EventArgs e)
        {
            ReloadWaveInDevices();
        }

        private void button_area_Click(object sender, EventArgs e)
        {
            var location = Screen.AllScreens[comboBox_targetDisplay.SelectedIndex].Bounds.Location;

            m_FormCapture.Show();
            m_FormOverRay.Hide();

            this.Enabled = false;
        }

        private void button_areaReset_Click(object sender, EventArgs e)
        {
            m_CaptureBounds = Screen.PrimaryScreen.Bounds;
            m_FormOverRay.ResetArea();
        }

        private void button_startCapture_Click(object sender, EventArgs e)
        {
            label_message.Text = Resources.CaptureStarting;

            if (!PrepareCapturing())
            {
                label_message.Text = Resources.CaptureFailed;
                return;
            }

            if (checkBox_recordVideo.Enabled && checkBox_recordVideo.Checked)
            {
                if (!PrepareRecording())
                {
                    label_message.Text = Resources.RecordFailed;
                    return;
                }
            }

            StartCapturing();

            panel_capture.Enabled = false;
            button_startCapture.Enabled = false;
            button_stopCapture.Enabled = true;

            label_message.Text = Resources.CaptureStarted;
        }

        private async void button_stopCapture_Click(object sender, EventArgs e)
        {
            label_message.Text = Resources.CaptureStopping;
            button_stopCapture.Enabled = false;

            await Task.Run(() => StopCapture());

            panel_capture.Enabled = true;
            button_startCapture.Enabled = true;

            label_message.Text = Resources.CaptureStopped;
        }

        private void radioButton_process_CheckedChanged(object sender, EventArgs e)
        {
            var chk = ((RadioButton)sender).Checked;

            comboBox_process.Enabled = chk;
            checkBox_recordVideo.Enabled = !chk;
        }

        private void radioButton_area_CheckedChanged(object sender, EventArgs e)
        {
            var chk = ((RadioButton)sender).Checked;

            panel_captureDesktop.Enabled = chk;

            if (!chk)
            {
                m_FormOverRay.Hide();
            }
            else if (checkBox_showOverRayForm.Checked)
            {
                m_FormOverRay.Show();
            }
        }

        private void checkBox_showOverRayForm_CheckedChanged(object sender, EventArgs e)
        {
            var chk = ((CheckBox)sender).Checked;

            if (chk)
            {
                m_FormOverRay.Show();
            }
            else
            {
                m_FormOverRay.Hide();
            }
        }

        private void checkBox_recordAudio_CheckedChanged(object sender, EventArgs e)
        {
            var chk = ((CheckBox)sender).Checked;
            var deviceInfo = WaveIn.GetCapabilities(comboBox_waveInDevices.SelectedIndex);

            panel_voice.Enabled = chk;
            checkBox_recordAudio.Enabled = chk;
        }

        private void checkBox_recordCapture_CheckedChanged(object sender, EventArgs e)
        {
            var chk = ((CheckBox)sender).Checked;

        }

        private static Regex m_NumberReg = new Regex(@"[0-9\b]");

        private void textBox_ip_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '\b')
                e.Handled = true;
        }

        private void textBox_port_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!m_NumberReg.IsMatch("" + e.KeyChar))
                e.Handled = true;
        }

        private void textBox_port_Validating(object sender, CancelEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (textBox.Text.Length == 0)
                textBox.Text = "0";

            try
            {
                var port = Convert.ToInt32(textBox.Text);
                if (port > UInt16.MaxValue)
                    textBox.Text = "" + UInt16.MaxValue;
            }
            catch
            {
                textBox.Text = "" + UInt16.MaxValue;
            }

        }

        
        private void textBox_imageQuality_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!m_NumberReg.IsMatch(""+e.KeyChar))
                e.Handled = true;
        }

        private void textBox_imageQuality_Validating(object sender, CancelEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (textBox.Text.Length == 0)
                textBox.Text = "" + Settings.Default.ImageQuality;

            try
            {
                var n = Convert.ToInt32(textBox.Text);
                if (n > MaxQuality)
                    textBox.Text = "" + MaxQuality;
            }
            catch
            {
                textBox.Text = "" + Settings.Default.ImageQuality;
            }
        }

        private void textBox_captureFps_Validating(object sender, CancelEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (textBox.Text.Length == 0)
                textBox.Text = "0";

            try
            {
                var val = Convert.ToInt32(textBox.Text);
                if (val > MaxFps)
                    textBox.Text = "" + MaxFps;
                else if (val <= 0)
                    textBox.Text = "" + MinFps;
            }
            catch
            {
                textBox.Text = "" + MaxFps / 2;
            }
        }

        private void Form_tcp_FormClosed(object sender, FormClosedEventArgs e)
        {
            Disconnect();
            m_WebSocketServer.Dispose();
        }

        private void comboBox_targetDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            var display = (Screen)comboBox_targetDisplay.SelectedItem;
            m_FormOverRay.Display = display;
            m_FormCapture.Display = display;
        }

        private void comboBox_waveInDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            var device = (WaveInCapabilities)comboBox_waveInDevices.SelectedItem;
            checkBox_stereo.Enabled = device.Channels == 2;
            checkBox_stereo.Checked &= checkBox_stereo.Enabled;
        }
    }

    #endregion
}