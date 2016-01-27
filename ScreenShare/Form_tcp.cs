using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.IO.Compression;

using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using Alchemy;
using Alchemy.Classes;
using Alchemy.Handlers.WebSocket;

using NAudio.Wave;
using NAudio.CoreAudioApi;

using SharpAvi;
using SharpAvi.Codecs;
using SharpAvi.Output;

using Newtonsoft.Json;

namespace ScreenShare
{
    public partial class Form_tcp : Form
    {
        /// <summary>
        /// HTTPサーバのポート
        /// </summary>
        static readonly int Port_HTTP = 8080;

        /// <summary>
        /// WebSocketサーバのポート
        /// </summary>
        static readonly int Port_WebSocket = 8081;

        /// <summary>
        /// 最大画質
        /// </summary>
        static readonly int MaxQuality = 100;

        /// <summary>
        /// 最小画質
        /// </summary>
        static readonly int MinQuality = 0;

        /// <summary>
        /// 最大フレームレート
        /// </summary>
        static readonly int MaxFps = 30;

        /// <summary>
        /// 最小フレームレート
        /// </summary>
        static readonly int MinFps = 1;

        /// <summary>
        /// キャプチャ領域選択用フォーム
        /// </summary>
        Form_CaptureArea m_formCapture = new Form_CaptureArea();

        /// <summary>
        /// キャプチャ領域オーバーレイ用フォーム
        /// </summary>
        Form_OverRay m_formOverRay = new Form_OverRay();

        /// <summary>
        /// キャプチャ用インスタンス
        /// </summary>
        Capture m_capture = new Capture();

        /// <summary>
        /// HTTPサーバインスタンス
        /// </summary>
        HttpServer m_httpServer;

        /// <summary>
        /// WebSocketサーバインスタンス
        /// </summary>
        WebSocketServer m_webSocketServer;

        /// <summary>
        /// 木構造
        /// </summary>
        Tree<UserContext> m_webSocketClients = new Tree<UserContext>();

        /// <summary>
        /// 音声録音インスタンス
        /// </summary>
        WaveIn m_recorder = new WaveIn();

        /// <summary>
        /// AVI作成インスタンス
        /// </summary>
        AviWriter m_aviWriter;

        /// <summary>
        /// 録画用インスタンス
        /// </summary>
        IAviVideoStream m_aviVideoStream;

        /// <summary>
        /// 録音用インスタンス
        /// </summary>
        IAviAudioStream m_aviAudioStream;

        /// <summary>
        /// 起動中プロセス一覧
        /// </summary>
        Process[] m_runningProcesses;

        /// <summary>
        /// キャプチャ領域
        /// </summary>
        Rectangle m_captureBounds = Screen.PrimaryScreen.Bounds;

        /// <summary>
        /// フォームスレッドで実行するためのデリゲート
        /// </summary>
        delegate void FormDelegate();
        delegate T FormDelegate<T>();

        /// <summary>
        /// 最新の分割画面
        /// </summary>
        byte[][] m_latestIntraFrameBuffer;

        public Form_tcp()
        {
            InitializeComponent();

            ReloadRunningProcesses();
            ReloadWaveInDevices();

            IPHostEntry ipentry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in ipentry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    textBox_ip.Text = ip.ToString();
                    break;
                }
            }

            var codecs = Mpeg4VideoEncoderVcm.GetAvailableCodecs();
            foreach (var codec in codecs)
            {
                comboBox_videoCodec.Items.Add(codec.Name);
            }

            comboBox_audioQuality.SelectedIndex = 1;
            comboBox_captureScale.SelectedIndex = 0;
            comboBox_divisionNumber.SelectedIndex = 2;
            comboBox_videoCodec.SelectedIndex = 0;
            comboBox_waveInDevices.SelectedIndex = 0;

            m_formCapture.Selected += (rect) => 
            { 
                m_captureBounds = rect;

                m_formCapture.Hide();

                m_formOverRay.CaptureBounds = rect;

                if (checkBox_showOverRayForm.Checked)
                    m_formOverRay.Show();
            };

            m_capture.SegmentCaptured += (s, data) =>
            {
                var frameHeader = new FrameHeader
                {
                    type = BufferType.FrameBuffer,
                    segmentIndex = (byte)data.segmentIdx,
                };
                var headerBuffer = Utils.GetBytesFromStructure(frameHeader);
                var buffer = Utils.Concatenation(headerBuffer, data.encodedFrameBuffer);

                if (m_webSocketClients.ContainsKey(0))
                {
                    //Debug.Log("send");
                    m_webSocketClients[0].Send(buffer);
                }

                m_latestIntraFrameBuffer[data.segmentIdx] = (byte[])buffer.Clone();
            };

            m_capture.Captured += (s, data) =>
            {
                if (m_aviVideoStream == null) return;

                Bitmap bmp = data.captureBitmap;

                if (m_aviVideoStream.Codec == KnownFourCCs.Codecs.Uncompressed)
                {
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }

                BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                byte[] buf = Utils.GetBytesFromPtr(bmpData.Scan0, bmp.Width * bmp.Height * 4);

                bmp.UnlockBits(bmpData);

                try
                {
                    m_aviVideoStream.WriteFrame(true, buf, 0, buf.Length);
                }
                catch (Exception e)
                {
                    Debug.Log("Captured Exception: " + e.Message);
                }
            };

            m_capture.Error += (s, ex) =>
            {
                label_message.Text = ex.Message;
                //Debug.Log("Capture Error: " + ex.Message);
            };

            m_recorder.BufferMilliseconds = 100;
            m_recorder.DataAvailable += (s, we) =>
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

                var frameHeader = new AudioHeader
                {
                    type = BufferType.AudioBuffer,
                };
                var headerBuffer = Utils.GetBytesFromStructure(frameHeader);
                var buffer = Utils.Concatenation(headerBuffer, normalizedSampleBuffer);

                if (checkBox_sendAudio.Checked)
                {
                    if (m_webSocketClients.ContainsKey(0))
                    {
                        m_webSocketClients[0].Send(buffer);
                    }
                }

                if (m_aviAudioStream == null || (m_aviVideoStream != null && m_aviVideoStream.FramesWritten == 0)) return;

                try
                {
                    m_aviAudioStream.WriteBlock(we.Buffer, 0, we.BytesRecorded);
                }
                catch (Exception e)
                {
                    Debug.Log("Audio Recorded Exception: " + e.Message);
                }
            };
        }

        /// <summary>
        /// HTTPサーバとWebSocketサーバの立ち上げ
        /// </summary>
        private void Connect()
        {
            var connectionLocking = new Object();

            m_httpServer = new HttpServer("+", Port_HTTP, "scripts");
            m_httpServer.Start();

            m_webSocketClients.Clear();
            m_webSocketServer = new Alchemy.WebSocketServer(Port_WebSocket)
            {
                OnConnected = (ctx) =>
                {
                    lock (connectionLocking)
                    {
                        int id = m_webSocketClients.Count, parentId;
                        UserContext parent;

                        MessageData data;
                        string json;

                        Debug.Log("User Connected: id = " + id);

                        try
                        {
                            try
                            {
                                parentId = m_webSocketClients.GetParentKey(id);
                                parent = m_webSocketClients[parentId];
                            }
                            catch
                            {
                                parentId = -1;
                                parent = null;
                            }

                            data = new MessageData { type = MessageData.Type.Connected, id = id, };
                            json = JsonConvert.SerializeObject(data);

                            m_webSocketClients[id] = ctx;
                            m_webSocketClients[id].Send(json);

                            if (m_capture.IsCapturing)
                            {
                                SendCaptureStartMessage(m_webSocketClients[id]);

                                if (parent == null)
                                {
                                    foreach (var buffer in m_latestIntraFrameBuffer)
                                        m_webSocketClients[id].Send(buffer);
                                }
                            }

                            if (parent != null)
                            {
                                data = new MessageData { type = MessageData.Type.PeerConnection, targetId = id, };
                                json = JsonConvert.SerializeObject(data);

                                parent.Send(json);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Connected Error : " + e.Message);
                        }

                        ctx.MaxFrameSize = 0x80000;
                    }
                },
                OnDisconnect = (ctx) =>
                {
                    lock (connectionLocking)
                    {
                        int id, parentId, lastId, lastParentId;
                        int[] childrenId;

                        UserContext updated, parent, last, lastParent;
                        UserContext[] children;

                        MessageData data;
                        string json;

                        try
                        {
                            id = m_webSocketClients.TryGetKey(ctx);
                        }
                        catch
                        {
                            Debug.Log("Disconnect: Can't find User");
                            return;
                        }

                        try
                        {
                            parentId = m_webSocketClients.GetParentKey(id);
                            parent = m_webSocketClients[parentId];
                        }
                        catch
                        {
                            parentId = -1;
                            parent = null;
                        }

                        lastId = m_webSocketClients.GetLastKey();
                        last = m_webSocketClients[lastId];

                        try
                        {
                            lastParentId = m_webSocketClients.GetParentKey(lastId);
                            lastParent = m_webSocketClients[lastParentId];
                        }
                        catch
                        {
                            lastParentId = -1;
                            lastParent = null;
                        }

                        childrenId = m_webSocketClients.GetChildrenKey(id);
                        children = m_webSocketClients.GetValues(childrenId);

                        try
                        {
                            if (parent != null)
                            {
                                data = new MessageData { type = MessageData.Type.RemoveOffer, id = id, };
                                json = JsonConvert.SerializeObject(data);
                                parent.Send(json);
                            }

                            if (lastId == id)
                            {
                                m_webSocketClients.Remove(lastId);
                                Debug.Log("Removed Last." + lastId);
                                return;
                            }

                            data = new MessageData { type = MessageData.Type.RemoveAnswer, };
                            json = JsonConvert.SerializeObject(data);
                            foreach (var child in children)
                            {
                                child.Send(json);
                            }

                            if (lastParentId != id)
                            {
                                data = new MessageData { type = MessageData.Type.RemoveOffer, id = lastId };
                                json = JsonConvert.SerializeObject(data);
                                lastParent.Send(json);
                            }

                            data = new MessageData { type = MessageData.Type.RemoveAnswer, };
                            json = JsonConvert.SerializeObject(data);
                            last.Send(json);

                            m_webSocketClients[id] = m_webSocketClients[lastId];
                            m_webSocketClients.Remove(lastId);

                            updated = m_webSocketClients[id];

                            data = new MessageData { type = MessageData.Type.UpdateID, id = id, };
                            json = JsonConvert.SerializeObject(data);
                            updated.Send(json);

                            if (parent != null)
                            {
                                data = new MessageData { type = MessageData.Type.PeerConnection, targetId = id, };
                                json = JsonConvert.SerializeObject(data);
                                parent.Send(json);
                            }

                            foreach (var cId in childrenId)
                            {
                                data = new MessageData { type = MessageData.Type.PeerConnection, targetId = cId, };
                                json = JsonConvert.SerializeObject(data);
                                updated.Send(json);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Connected Error : " + e.Message);
                        }

                        Debug.Log("User Disonnected: id = " + id);
                    }
                },
                OnReceive = (ctx) =>
                {
                    try
                    {
                        var json = ctx.DataFrame.ToString();
                        var recv = JsonConvert.DeserializeObject<MessageData>(json);
                        var dest = m_webSocketClients[recv.targetId];

                        if (dest != null)
                        {
                            dest.Send(json);
                            Debug.Log("Relay '" + recv.type + "' from id:" + recv.id + " to id: " + recv.targetId);
                        }
                        else
                        {
                            Debug.Log("Failed to Relay Message: id = " + recv.targetId);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Receive Error : " + e.Message);
                    }
                   
                },
                TimeOut = new TimeSpan(0, 5, 0),
            };
            m_webSocketServer.Start();
        }

        /// <summary>
        /// HTTPサーバとWebSocketサーバの停止/切断
        /// </summary>
        private void Disconnect()
        {
            StopCapture();

            if (m_webSocketServer != null)
            {
                m_webSocketServer.Stop();
                m_webSocketServer = null;
            }

            if (m_httpServer != null)
            {
                m_httpServer.Close();
                m_httpServer = null;
            }

            Invoke(new FormDelegate(() => m_formOverRay.Hide()));
        }

        /// <summary>
        /// 実行中のプロセスの再読み込み
        /// </summary>
        private void ReloadRunningProcesses()
        {
            m_runningProcesses = Process.GetProcesses().Where((p) => { return p.MainWindowHandle != IntPtr.Zero && p.MainWindowTitle.Length != 0; }).ToArray();
            
            comboBox_process.Items.Clear();

            var maxWidth = comboBox_process.DropDownWidth;
            using (var g = comboBox_process.CreateGraphics())
            {
                foreach (var p in m_runningProcesses)
                {
                    var str = p.MainWindowTitle;

                    comboBox_process.Items.Add(str);
                    maxWidth = Math.Max(maxWidth, (int)g.MeasureString(str, comboBox_process.Font).Width);
                }
            }

            comboBox_process.DropDownWidth = maxWidth;
            comboBox_process.SelectedIndex = 0;
        }

        /// <summary>
        /// 録音デバイスの再読み込み
        /// </summary>
        private void ReloadWaveInDevices()
        {
            int waveInDevices = WaveIn.DeviceCount;

            comboBox_waveInDevices.Items.Clear();

            var maxWidth = comboBox_waveInDevices.DropDownWidth;
            using (var g = comboBox_waveInDevices.CreateGraphics())
            {
                for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
                {
                    var deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                    var str = deviceInfo.ProductName;

                    comboBox_waveInDevices.Items.Add(str);
                    maxWidth = Math.Max(maxWidth, (int)g.MeasureString(str, comboBox_waveInDevices.Font).Width);
                }
            }

            comboBox_waveInDevices.DropDownWidth = maxWidth;
            comboBox_waveInDevices.SelectedIndex = 0;
        }

        /// <summary>
        /// キャプチャ開始
        /// </summary>
        /// <returns></returns>
        private bool StartCapture()
        {
            var path = "";
            if (checkBox_recordCapture.Enabled && checkBox_recordCapture.Checked)
            {
                var dialog = new SaveFileDialog()
                {
                    FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss.avi"),
                    Filter = "AVIファイル(*.avi)|*.avi",
                    InitialDirectory = Directory.GetCurrentDirectory(),
                    RestoreDirectory = true,
                };

                Invoke(new FormDelegate(() =>
                {
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        path = dialog.FileName;
                    }
                }));
                
                if (path == "")
                {
                    return false;
                }
            }
            

            float captureScale = 1.0f;
            int captureDivisionNum = 0, 
                captureEncordingQuality = 0, 
                captureFps = 0, 
                captureWidth = 0, 
                captureHeight = 0,
                audioSampleRate = 0,
                audioBps = 0,
                audioChannels = 0,
                codecSelectedIndex = 0,
                codecQuality = 0;

            Invoke(new FormDelegate(() => 
            {
                if (radioButton_process.Checked)
                {
                    var index = comboBox_process.SelectedIndex;

                    if (index < 0 || index >= m_runningProcesses.Length || m_runningProcesses[index].HasExited)
                    {
                        MessageBox.Show("プロセスが存在しません。");
                        comboBox_process.SelectedIndex = 0;
                        return;
                    }

                    m_capture.CaptureProcess = m_runningProcesses[index];
                }
                else
                {
                    m_capture.CaptureProcess = null;
                }

                if (radioButton_area.Checked)
                {
                    m_capture.UseCaptureBounds = true;
                    m_capture.CaptureBounds = m_captureBounds;
                }
                else
                {
                    m_capture.UseCaptureBounds = false;
                    m_capture.CaptureBounds = Screen.PrimaryScreen.Bounds;
                }
                

                captureScale = (1.0f - (float)comboBox_captureScale.SelectedIndex / 10);
                captureDivisionNum = comboBox_divisionNumber.SelectedIndex + 1;
                captureEncordingQuality = Convert.ToInt32(textBox_captureQuality.Text);
                captureFps = Convert.ToInt32(textBox_captureFps.Text);
                captureWidth = (int)(m_capture.CaptureBounds.Width * captureScale);
                captureHeight = (int)(m_capture.CaptureBounds.Height * captureScale);
                
                audioSampleRate = Convert.ToInt32(comboBox_audioQuality.SelectedItem);
                audioBps = 16;
                audioChannels = checkBox_stereo.Enabled ? (checkBox_stereo.Checked ? 2 : 1) : 1;

                codecSelectedIndex = comboBox_videoCodec.SelectedIndex;
                codecQuality = Convert.ToInt32(textBox_recordQuality.Text);
            })); 

            m_capture.Scale = captureScale;
            m_capture.CaptureDivisionNum = captureDivisionNum;
            m_capture.EncoderQuality = captureEncordingQuality;
            m_capture.FramesPerSecond = captureFps;

            m_recorder.WaveFormat = new WaveFormat(audioSampleRate, audioBps, audioChannels);
            m_latestIntraFrameBuffer = new byte[m_capture.CaptureDivisionNum * m_capture.CaptureDivisionNum][];

            foreach (var pair in m_webSocketClients)
            {
                SendCaptureStartMessage(pair.Value);
            }

            if (checkBox_recordCapture.Enabled && checkBox_recordCapture.Checked)
            {
                try
                {
                    m_aviWriter = new AviWriter(path)
                    {
                        FramesPerSecond = (int)m_capture.FramesPerSecond,
                        EmitIndex1 = true,
                    };

                    if (codecSelectedIndex == 0)
                    {
                        m_aviVideoStream = m_aviWriter.AddVideoStream(captureWidth, captureHeight);
                    }
                    else if (codecSelectedIndex == 1)
                    {
                        m_aviVideoStream = m_aviWriter.AddMotionJpegVideoStream(captureWidth, captureHeight, codecQuality);
                    }
                    else
                    {
                        var codecs = Mpeg4VideoEncoderVcm.GetAvailableCodecs();
                        var encoder = new Mpeg4VideoEncoderVcm(captureWidth, captureHeight, m_capture.FramesPerSecond, 0, codecQuality, codecs[codecSelectedIndex - 2].Codec);
                        m_aviVideoStream = m_aviWriter.AddEncodingVideoStream(encoder);
                    }
                    

                    if (checkBox_recordAudio.Checked)
                    {
                        m_aviAudioStream = m_aviWriter.AddAudioStream(audioChannels, audioSampleRate, audioBps);
                    }
                }
                catch
                {
                    Debug.Log("Failed to Start Recording.");
                    return false;
                }
            }

            m_capture.Start();

            if (checkBox_sendAudio.Checked || checkBox_recordAudio.Checked)
            {
                m_recorder.StartRecording();
            }

            return true;
        }

        /// <summary>
        /// 画面キャプチャ(音声録音)の停止
        /// </summary>
        private void StopCapture()
        {
            foreach (var pair in m_webSocketClients)
            {
                var data = new MessageData { type = MessageData.Type.StopCapture };
                var json = JsonConvert.SerializeObject(data);

                pair.Value.Send(json);
            }

            m_capture.WaitStop();

            if (checkBox_sendAudio.Checked || checkBox_recordAudio.Checked)
            {
                m_recorder.StopRecording();
            }

            if (m_aviWriter != null)
            {
                Debug.Log("" + m_aviVideoStream.FramesWritten);
                m_aviWriter.Close();
                m_aviWriter = null;
                m_aviVideoStream = null;
                m_aviAudioStream = null;
            }
        }
     

        private void SendCaptureStartMessage(UserContext ctx)
        {
            var setting = new SettingData
            {
                audioSettingData = new SettingData.AudioSettingData
                {
                    sampleRate = m_recorder.WaveFormat.SampleRate,
                    isStereo = m_recorder.WaveFormat.Channels == 2,
                },
                captureSettingData = new SettingData.CaptureSettingData
                {
                    divisionNum = m_capture.CaptureDivisionNum,
                },
            };
            var data = new MessageData { type = MessageData.Type.Settings, data = setting };
            var json = JsonConvert.SerializeObject(data);

            ctx.Send(json);

            data = new MessageData { type = MessageData.Type.StartCapture };
            json = JsonConvert.SerializeObject(data);

            ctx.Send(json);
        }

        //*****************************************************************
        // イベント
        //*****************************************************************

        private async void button_connect_Click(object sender, EventArgs e)
        {
            label_message.Text = "サーバ設立中...";

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
                label_message.Text = "サーバ設立失敗 : " + res.Message;

                textBox_ip.Enabled = true;
                button_connect.Enabled = true;

                return;
            }

            button_disconnect.Enabled = true;
            panel_capture.Enabled = true;
            button_startCapture.Enabled = true;
            checkBox_recordCapture.Enabled = true;

            label_message.Text = "サーバ設立完了";
        }

        private async void button_disconnect_Click(object sender, EventArgs e)
        {
            label_message.Text = "切断中...";

            button_disconnect.Enabled = false;
            button_startCapture.Enabled = false;
            button_stopCapture.Enabled = false;
            panel_capture.Enabled = false;

            m_capture.Stop();
            await Task.Run(() => Disconnect());

            textBox_ip.Enabled = true;
            button_connect.Enabled = true;
            
            label_message.Text = "切断完了";
        }

        private void button_reloadProcesses_Click(object sender, EventArgs e)
        {
            ReloadRunningProcesses();
        }

        private void button_reloadWaveInDevices_Click(object sender, EventArgs e)
        {
            ReloadWaveInDevices();
        }

        private void button_area_Click(object sender, EventArgs e)
        {
            m_formCapture.Show();
            m_formOverRay.Hide();
        }

        private void button_areaReset_Click(object sender, EventArgs e)
        {
            m_captureBounds = Screen.PrimaryScreen.Bounds;
            m_formOverRay.ResetArea();
        }

        private async void button_startCapture_Click(object sender, EventArgs e)
        {
            label_message.Text = "キャプチャ準備中...";

            if (!(await Task.Run(() => StartCapture()))) 
            {
                label_message.Text = "キャプチャを開始できませんでした。";
                return;
            }

            panel_capture.Enabled = false;
            button_startCapture.Enabled = false;
            button_stopCapture.Enabled = true;

            label_message.Text = "キャプチャ中...";
        }

        private async void button_stopCapture_Click(object sender, EventArgs e)
        {
            label_message.Text = "キャプチャ終了中...";
            button_stopCapture.Enabled = false;

            m_capture.Stop();
            await Task.Run(() => StopCapture());

            panel_capture.Enabled = true;
            button_startCapture.Enabled = true;

            label_message.Text = "キャプチャ終了";
        }

        private void radioButton_process_CheckedChanged(object sender, EventArgs e)
        {
            var chk = ((RadioButton)sender).Checked;

            comboBox_process.Enabled = chk;
            checkBox_recordCapture.Enabled = !chk;
            panel_record.Enabled = checkBox_recordCapture.Checked && !chk;
        }

        private void radioButton_area_CheckedChanged(object sender, EventArgs e)
        {
            var chk = ((RadioButton)sender).Checked;

            button_area.Enabled = chk;
            button_areaReset.Enabled = chk;
            checkBox_showOverRayForm.Enabled = chk;

            if (!chk)
            {
                m_formOverRay.Hide();
            }
            else if (checkBox_showOverRayForm.Checked)
            {
                m_formOverRay.Show();
            }
        }

        private void checkBox_showOverRayForm_CheckedChanged(object sender, EventArgs e)
        {
            var chk = ((CheckBox)sender).Checked;

            if (chk)
            {
                m_formOverRay.Show();
            }
            else
            {
                m_formOverRay.Hide();
            }
        }

        private void checkBox_recordAudio_CheckedChanged(object sender, EventArgs e)
        {
            var chk = ((CheckBox)sender).Checked;
            var deviceInfo = WaveIn.GetCapabilities(comboBox_waveInDevices.SelectedIndex);

            label_waveInDevices.Enabled = chk;
            comboBox_waveInDevices.Enabled = chk;
            button_reloadWaveInDevices.Enabled = chk;
            checkBox_stereo.Enabled = deviceInfo.Channels == 1 ? false : chk;
            label_audioQuality.Enabled = chk;
            comboBox_audioQuality.Enabled = chk;
        }

        private void checkBox_recordCapture_CheckedChanged(object sender, EventArgs e)
        {
            var chk = ((CheckBox)sender).Checked;

            //label_videoCodec.Enabled = radioButton_area.Checked && chk;
            //comboBox_videoCodec.Enabled = radioButton_area.Checked && chk;
            panel_record.Enabled = chk;

        }
        
        private void comboBox_videoCodec_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = ((ComboBox)sender).SelectedIndex;

            if (idx == 0)
            {
                label_recordQualty.Enabled = false;
                textBox_recordQuality.Enabled = false;
            }
            else
            {
                label_recordQualty.Enabled = true;
                textBox_recordQuality.Enabled = true;
            }
        }

        private void comboBox_waveInDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = ((ComboBox)sender).SelectedIndex;
            var deviceInfo = WaveIn.GetCapabilities(idx);

            checkBox_stereo.Enabled = (checkBox_recordAudio.Enabled ? deviceInfo.Channels == 2 : false);
        }

        private void textBox_ip_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '\b')
                e.Handled = true;
        }

        private void textBox_port_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
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

        private void textBox_captureQuality_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
                e.Handled = true;
        }

        private void textBox_captureQuality_Validating(object sender, CancelEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (textBox.Text.Length == 0)
                textBox.Text = "0";

            try
            {
                var val = Convert.ToInt32(textBox.Text);
                if (val > MaxQuality)
                    textBox.Text = "" + MaxQuality;
                else if (val < 0)
                    textBox.Text = "" + MinQuality;
            }
            catch
            {
                textBox.Text = "" + MaxQuality;
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
    }
}
