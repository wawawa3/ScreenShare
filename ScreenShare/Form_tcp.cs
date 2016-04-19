using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using Alchemy;
using Alchemy.Classes;

using NAudio.Wave;

using SharpAvi.Codecs;
using SharpAvi.Output;

using Newtonsoft.Json;

using ScreenShare.Properties;

namespace ScreenShare
{
    public partial class Form_tcp : Form
    {
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
        /// サーバの状態
        /// </summary>
        private bool m_ServerRunning = false;

        /// <summary>
        /// 音声録音インスタンス
        /// </summary>
        private WaveIn m_Recorder = new WaveIn();

        /// <summary>
        /// AVIファイル名
        /// </summary>
        private string m_AviFilePath = "";

        /// <summary>
        /// AVI作成インスタンス
        /// </summary>
        private AviWriter m_AviWriter;

        /// <summary>
        /// 録画用インスタンス
        /// </summary>
        private IAviVideoStream m_AviVideoStream;

        /// <summary>
        /// 録音用インスタンス
        /// </summary>
        private IAviAudioStream m_AviAudioStream;

        /// <summary>
        /// 起動中プロセス一覧
        /// </summary>
        private Process[] m_RunningProcesses;

        /// <summary>
        /// キャプチャ領域
        /// </summary>
        private Rectangle m_CaptureBounds = Screen.PrimaryScreen.Bounds;

        /// <summary>
        /// 最新の分割画面
        /// </summary>
        private byte[][] m_LatestIntraFrameBuffer;



        public Form_tcp()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en");

            InitializeComponent();

            ReloadRunningProcesses();
            ReloadWaveInDevices();

            IPHostEntry ipentry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in ipentry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
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

            m_FormCapture.Selected += (rect) => 
            { 
                m_CaptureBounds = rect;

                m_FormCapture.Hide();

                m_FormOverRay.CaptureBounds = rect;

                if (checkBox_showOverRayForm.Checked)
                    m_FormOverRay.Show();
            };

            m_Capture.SegmentCaptured += (s, data) =>
            {
                var frameHeader = new FrameHeader
                {
                    type = BufferType.FrameBuffer,
                    segmentIndex = (byte)data.segmentIdx,
                };
                var headerBuffer = Utils.GetBytesFromStructure(frameHeader);
                var buffer = Utils.Concatenation(headerBuffer, data.encodedFrameBuffer);

                if (m_WebSocketClients.ContainsKey(0))
                {
                    //Debug.Log("send");
                    m_WebSocketClients[0].Send(buffer);
                }

                m_LatestIntraFrameBuffer[data.segmentIdx] = (byte[])buffer.Clone();
            };

            m_Capture.Captured += (s, data) =>
            {
                if (m_AviVideoStream == null) return;

                var buf = Utils.GetBytesFromPtr(data.captureData, data.captureSize.Width * data.captureSize.Height * 4);

                try
                {
                    m_AviVideoStream.WriteFrame(true, buf, 0, buf.Length);
                }
                catch (Exception e)
                {
                    Debug.Log("Captured Exception: " + e.Message);
                }
            };

            m_Capture.Error += (s, ex) =>
            {
                label_message.Text = Resources.CaptureError + " : " + ex.Message;
            };

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

                var frameHeader = new AudioHeader
                {
                    type = BufferType.AudioBuffer,
                };
                var headerBuffer = Utils.GetBytesFromStructure(frameHeader);
                var buffer = Utils.Concatenation(headerBuffer, normalizedSampleBuffer);

                if (checkBox_sendAudio.Checked)
                {
                    if (m_WebSocketClients.ContainsKey(0))
                    {
                        m_WebSocketClients[0].Send(buffer);
                    }
                }

                if (m_AviAudioStream == null || (m_AviVideoStream != null && m_AviVideoStream.FramesWritten == 0)) return;

                try
                {
                    m_AviAudioStream.WriteBlock(we.Buffer, 0, we.BytesRecorded);
                }
                catch (Exception e)
                {
                    Debug.Log("Audio Recorded Exception: " + e.Message);
                }
            };

            m_WebSocketServer = new Alchemy.WebSocketServer(Settings.Default.Port_WebSocket)
            {
                OnConnected = (ctx) =>
                {
                    if (!m_ServerRunning) return;

                    lock (m_WebSocketServer)
                    {
                        int id = m_WebSocketClients.Count, parentId;
                        UserContext parent;

                        MessageData data;
                        string json;

                        Debug.Log("User Connected: id = " + id);

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

                            data = new MessageData { type = MessageData.Type.Connected, id = id, };
                            json = JsonConvert.SerializeObject(data);

                            m_WebSocketClients[id] = ctx;
                            m_WebSocketClients[id].Send(json);

                            if (m_Capture.Capturing)
                            {
                                SendCaptureStartMessage(m_WebSocketClients[id]);

                                /*
                                if (parent == null)
                                {
                                    foreach (var buffer in m_LatestIntraFrameBuffer)
                                        m_WebSocketClients[id].Send(buffer);
                                }
                                    * */
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

                    Invoke(new FormDelegate(() => { this.label_ConnectionNum.Text = "" + m_WebSocketClients.Count; }));
                },
                OnDisconnect = (ctx) =>
                {
                    if (!m_ServerRunning) return;

                    lock (m_WebSocketServer)
                    {
                        int id, parentId, lastId, lastParentId;
                        int[] childrenId;

                        UserContext updated, parent, last, lastParent;
                        UserContext[] children;

                        MessageData data;
                        string json;

                        try
                        {
                            id = m_WebSocketClients.TryGetKey(ctx);
                        }
                        catch
                        {
                            Debug.Log("Disconnect: Can't find User");
                            return;
                        }

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

                        lastId = m_WebSocketClients.GetLastKey();
                        last = m_WebSocketClients[lastId];

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

                        childrenId = m_WebSocketClients.GetChildrenKey(id);
                        children = m_WebSocketClients.GetValues(childrenId);

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
                                m_WebSocketClients.Remove(lastId);
                                Debug.Log("Removed Last." + lastId);
                                return;
                            }
                            else
                            {
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

                                m_WebSocketClients[id] = m_WebSocketClients[lastId];

                                foreach (var c in m_WebSocketClients)
                                {
                                    Debug.Log(c.Key + " : " + c.Value.ClientAddress);
                                }
                                m_WebSocketClients.Remove(lastId);
                                foreach (var c in m_WebSocketClients)
                                {
                                    Debug.Log(c.Key + " : " + c.Value.ClientAddress);
                                }

                                updated = m_WebSocketClients[id];

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
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Connected Error : " + e.Message);
                        }

                        Debug.Log("User Disonnected: id = " + id);
                    }

                    Invoke(new FormDelegate(() => { this.label_ConnectionNum.Text = "" + m_WebSocketClients.Count; }));
                },
                OnReceive = (ctx) =>
                {
                    if (!m_ServerRunning) return;

                    lock (m_WebSocketServer)
                    {
                        try
                        {
                            var json = ctx.DataFrame.ToString();
                            var recv = JsonConvert.DeserializeObject<MessageData>(json);
                            try
                            {
                                var dest = m_WebSocketClients[recv.targetId];

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
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Receive Error : " + e.Message);
                        }
                    }
                },
                TimeOut = new TimeSpan(0, 5, 0),
            };
        }

        /// <summary>
        /// HTTPサーバとWebSocketサーバの立ち上げ
        /// </summary>
        private void Connect()
        {
            var connectionLocking = new Object();

            m_HttpServer = new HttpServer("+", Settings.Default.Port_HTTP, Settings.Default.DocumentPath);
            m_HttpServer.Start();

            m_WebSocketServer.Start();

            m_ServerRunning = true;
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
                m_WebSocketServer.Stop();
                this.label_ConnectionNum.Text = "0";

                m_HttpServer.Close();
            }

            Invoke(new FormDelegate(() => m_FormOverRay.Hide()));
        }

        /// <summary>
        /// 実行中のプロセスの再読み込み
        /// </summary>
        private void ReloadRunningProcesses()
        {
            m_RunningProcesses = Process.GetProcesses().Where((p) => { return p.MainWindowHandle != IntPtr.Zero && p.MainWindowTitle.Length != 0; }).ToArray();
            
            comboBox_process.Items.Clear();

            var maxWidth = comboBox_process.DropDownWidth;
            using (var g = comboBox_process.CreateGraphics())
            {
                foreach (var p in m_RunningProcesses)
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
            groupBox_audio.Enabled = WaveIn.DeviceCount > 0;
            if (!groupBox_audio.Enabled)
            {
                return;
            }

            comboBox_waveInDevices.Items.Clear();

            var maxWidth = comboBox_waveInDevices.DropDownWidth;
            using (var g = comboBox_waveInDevices.CreateGraphics())
            {
                for (int waveInDevice = 0; waveInDevice < WaveIn.DeviceCount; waveInDevice++)
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

        private bool PrepareCapturing()
        {
            float captureScale = 1.0f;
            int captureDivisionNum = 0,
                captureEncordingQuality = 0,
                captureFps = 0,
                audioSampleRate = 0,
                audioBps = 0,
                audioChannels = 0;


            m_Capture.CaptureProcess = null;
            if (radioButton_process.Checked)
            {
                var index = comboBox_process.SelectedIndex;

                if (index < 0 || index >= m_RunningProcesses.Length || m_RunningProcesses[index].HasExited)
                {
                    MessageBox.Show(Resources.ProcessNotFound);
                    comboBox_process.SelectedIndex = 0;
                    return false;
                }

                m_Capture.CaptureProcess = m_RunningProcesses[index];
            }

            if (radioButton_area.Checked)
            {
                m_Capture.UseCaptureBounds = true;
                m_Capture.CaptureBounds = m_CaptureBounds;
            }
            else
            {
                m_Capture.UseCaptureBounds = false;
                m_Capture.CaptureBounds = Screen.PrimaryScreen.Bounds;
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

            m_Recorder.WaveFormat = new WaveFormat(audioSampleRate, audioBps, audioChannels);
            m_LatestIntraFrameBuffer = new byte[m_Capture.CaptureDivisionNum * m_Capture.CaptureDivisionNum][];

            return true;
        }

        private bool PrepareRecording()
        {
            var dialog = new SaveFileDialog()
            {
                FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss.avi"),
                Filter = Resources.AVIFilter,
                InitialDirectory = Directory.GetCurrentDirectory(),
                RestoreDirectory = true,
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                m_AviFilePath = dialog.FileName;
            else
                return false;

            return true;
        }

        private bool StartRecording()
        {
            int captureWidth = (int)(m_Capture.CaptureBounds.Width * m_Capture.Scale),
                captureHeight = (int)(m_Capture.CaptureBounds.Height * m_Capture.Scale),
                codecSelectedIndex = comboBox_videoCodec.SelectedIndex,
                codecQuality = Convert.ToInt32(textBox_recordQuality.Text);

            try
            {
                m_AviWriter = new AviWriter(m_AviFilePath)
                {
                    FramesPerSecond = (int)m_Capture.FramesPerSecond,
                    EmitIndex1 = true,
                };

                if (codecSelectedIndex == 0)
                {
                    m_AviVideoStream = m_AviWriter.AddVideoStream(captureWidth, captureHeight);
                }
                else if (codecSelectedIndex == 1)
                {
                    m_AviVideoStream = m_AviWriter.AddMotionJpegVideoStream(captureWidth, captureHeight, codecQuality);
                }
                else
                {
                    var codecs = Mpeg4VideoEncoderVcm.GetAvailableCodecs();
                    var encoder = new Mpeg4VideoEncoderVcm(captureWidth, captureHeight, m_Capture.FramesPerSecond, 0, codecQuality, codecs[codecSelectedIndex - 2].Codec);
                    m_AviVideoStream = m_AviWriter.AddEncodingVideoStream(encoder);
                }


                if (checkBox_recordAudio.Checked)
                {
                    m_AviAudioStream = m_AviWriter.AddAudioStream(m_Recorder.WaveFormat.Channels, m_Recorder.WaveFormat.SampleRate, m_Recorder.WaveFormat.BitsPerSample);
                }
            }
            catch
            {
                Debug.Log("Failed to Start Recording.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// キャプチャ開始
        /// </summary>
        /// <returns></returns>
        private void StartCapturing()
        {
            foreach (var pair in m_WebSocketClients)
            {
                SendCaptureStartMessage(pair.Value);
            }

            m_Capture.Start();

            if (checkBox_sendAudio.Checked || checkBox_recordAudio.Checked)
            {
                m_Recorder.StartRecording();
            }
        }

        /// <summary>
        /// 画面キャプチャ(音声録音)の停止
        /// </summary>
        private void StopCapture()
        {
            foreach (var pair in m_WebSocketClients)
            {
                var data = new MessageData { type = MessageData.Type.StopCapture };
                var json = JsonConvert.SerializeObject(data);

                pair.Value.Send(json);
            }

            m_Capture.StopAsync();

            if (checkBox_sendAudio.Checked || checkBox_recordAudio.Checked)
            {
                m_Recorder.StopRecording();
            }

            if (m_AviWriter != null)
            {
                Debug.Log("" + m_AviVideoStream.FramesWritten);
                m_AviWriter.Close();
                m_AviWriter = null;
                m_AviVideoStream = null;
                m_AviAudioStream = null;
            }
        }
     

        private void SendCaptureStartMessage(UserContext ctx)
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
            checkBox_recordCapture.Enabled = true;

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
            m_FormCapture.Show();
            m_FormOverRay.Hide();
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

            if (checkBox_recordCapture.Enabled && checkBox_recordCapture.Checked && !PrepareRecording())
            {
                label_message.Text = Resources.RecordCancelled;
                return;
            }

            if (!StartRecording())
            {
                label_message.Text = Resources.RecordFailed;
                return;
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

        private void Form_tcp_FormClosed(object sender, FormClosedEventArgs e)
        {
            Disconnect();
            m_WebSocketServer.Dispose();
        }
    }
}
