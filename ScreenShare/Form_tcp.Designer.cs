namespace ScreenShare
{
    partial class Form_tcp
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_tcp));
            this.button_startCapture = new System.Windows.Forms.Button();
            this.button_stopCapture = new System.Windows.Forms.Button();
            this.textBox_ip = new System.Windows.Forms.TextBox();
            this.label_ip = new System.Windows.Forms.Label();
            this.button_connect = new System.Windows.Forms.Button();
            this.button_disconnect = new System.Windows.Forms.Button();
            this.label_message = new System.Windows.Forms.Label();
            this.label_interval = new System.Windows.Forms.Label();
            this.button_area = new System.Windows.Forms.Button();
            this.radioButton_process = new System.Windows.Forms.RadioButton();
            this.comboBox_process = new System.Windows.Forms.ComboBox();
            this.panel_capture = new System.Windows.Forms.Panel();
            this.groupBox_record = new System.Windows.Forms.GroupBox();
            this.textBox_recordQuality = new System.Windows.Forms.TextBox();
            this.label_recordQualty = new System.Windows.Forms.Label();
            this.comboBox_videoCodec = new System.Windows.Forms.ComboBox();
            this.label_videoCodec = new System.Windows.Forms.Label();
            this.checkBox_recordAudio = new System.Windows.Forms.CheckBox();
            this.checkBox_recordCapture = new System.Windows.Forms.CheckBox();
            this.groupBox_audio = new System.Windows.Forms.GroupBox();
            this.button_reloadWaveInDevices = new System.Windows.Forms.Button();
            this.label_waveInDevices = new System.Windows.Forms.Label();
            this.comboBox_waveInDevices = new System.Windows.Forms.ComboBox();
            this.checkBox_stereo = new System.Windows.Forms.CheckBox();
            this.comboBox_audioQuality = new System.Windows.Forms.ComboBox();
            this.label_audioQuality = new System.Windows.Forms.Label();
            this.checkBox_sendAudio = new System.Windows.Forms.CheckBox();
            this.groupBox_sending = new System.Windows.Forms.GroupBox();
            this.textBox_captureFps = new System.Windows.Forms.TextBox();
            this.comboBox_captureScale = new System.Windows.Forms.ComboBox();
            this.label_captureScale = new System.Windows.Forms.Label();
            this.textBox_captureQuality = new System.Windows.Forms.TextBox();
            this.label_divisionNumber = new System.Windows.Forms.Label();
            this.label_imageCompressQuality = new System.Windows.Forms.Label();
            this.comboBox_divisionNumber = new System.Windows.Forms.ComboBox();
            this.groupBox_capture = new System.Windows.Forms.GroupBox();
            this.checkBox_showOverRayForm = new System.Windows.Forms.CheckBox();
            this.button_areaReset = new System.Windows.Forms.Button();
            this.radioButton_area = new System.Windows.Forms.RadioButton();
            this.button_reloadProcesses = new System.Windows.Forms.Button();
            this.panel_record = new System.Windows.Forms.Panel();
            this.panel_capture.SuspendLayout();
            this.groupBox_record.SuspendLayout();
            this.groupBox_audio.SuspendLayout();
            this.groupBox_sending.SuspendLayout();
            this.groupBox_capture.SuspendLayout();
            this.panel_record.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_startCapture
            // 
            this.button_startCapture.Enabled = false;
            this.button_startCapture.Location = new System.Drawing.Point(319, 415);
            this.button_startCapture.Name = "button_startCapture";
            this.button_startCapture.Size = new System.Drawing.Size(99, 23);
            this.button_startCapture.TabIndex = 0;
            this.button_startCapture.Text = "キャプチャ開始";
            this.button_startCapture.UseVisualStyleBackColor = true;
            this.button_startCapture.Click += new System.EventHandler(this.button_startCapture_Click);
            // 
            // button_stopCapture
            // 
            this.button_stopCapture.Enabled = false;
            this.button_stopCapture.Location = new System.Drawing.Point(424, 415);
            this.button_stopCapture.Name = "button_stopCapture";
            this.button_stopCapture.Size = new System.Drawing.Size(97, 23);
            this.button_stopCapture.TabIndex = 1;
            this.button_stopCapture.Text = "キャプチャ終了";
            this.button_stopCapture.UseVisualStyleBackColor = true;
            this.button_stopCapture.Click += new System.EventHandler(this.button_stopCapture_Click);
            // 
            // textBox_ip
            // 
            this.textBox_ip.Location = new System.Drawing.Point(78, 14);
            this.textBox_ip.Name = "textBox_ip";
            this.textBox_ip.ReadOnly = true;
            this.textBox_ip.Size = new System.Drawing.Size(115, 19);
            this.textBox_ip.TabIndex = 3;
            this.textBox_ip.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_ip_KeyPress);
            // 
            // label_ip
            // 
            this.label_ip.AutoSize = true;
            this.label_ip.Location = new System.Drawing.Point(19, 17);
            this.label_ip.Name = "label_ip";
            this.label_ip.Size = new System.Drawing.Size(53, 12);
            this.label_ip.TabIndex = 4;
            this.label_ip.Text = "ローカルIP";
            // 
            // button_connect
            // 
            this.button_connect.Location = new System.Drawing.Point(320, 12);
            this.button_connect.Name = "button_connect";
            this.button_connect.Size = new System.Drawing.Size(99, 23);
            this.button_connect.TabIndex = 5;
            this.button_connect.Text = "サーバ設立";
            this.button_connect.UseVisualStyleBackColor = true;
            this.button_connect.Click += new System.EventHandler(this.button_connect_Click);
            // 
            // button_disconnect
            // 
            this.button_disconnect.Enabled = false;
            this.button_disconnect.Location = new System.Drawing.Point(425, 12);
            this.button_disconnect.Name = "button_disconnect";
            this.button_disconnect.Size = new System.Drawing.Size(99, 23);
            this.button_disconnect.TabIndex = 8;
            this.button_disconnect.Text = "切断";
            this.button_disconnect.UseVisualStyleBackColor = true;
            this.button_disconnect.Click += new System.EventHandler(this.button_disconnect_Click);
            // 
            // label_message
            // 
            this.label_message.BackColor = System.Drawing.SystemColors.Control;
            this.label_message.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_message.Location = new System.Drawing.Point(12, 441);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(512, 19);
            this.label_message.TabIndex = 9;
            // 
            // label_interval
            // 
            this.label_interval.AutoSize = true;
            this.label_interval.Location = new System.Drawing.Point(347, 52);
            this.label_interval.Name = "label_interval";
            this.label_interval.Size = new System.Drawing.Size(97, 12);
            this.label_interval.TabIndex = 11;
            this.label_interval.Text = "キャプチャ頻度(fps)";
            // 
            // button_area
            // 
            this.button_area.Location = new System.Drawing.Point(283, 41);
            this.button_area.Name = "button_area";
            this.button_area.Size = new System.Drawing.Size(123, 23);
            this.button_area.TabIndex = 12;
            this.button_area.Text = "キャプチャ領域選択";
            this.button_area.UseVisualStyleBackColor = true;
            this.button_area.Click += new System.EventHandler(this.button_area_Click);
            // 
            // radioButton_process
            // 
            this.radioButton_process.AutoSize = true;
            this.radioButton_process.Location = new System.Drawing.Point(6, 77);
            this.radioButton_process.Name = "radioButton_process";
            this.radioButton_process.Size = new System.Drawing.Size(132, 16);
            this.radioButton_process.TabIndex = 15;
            this.radioButton_process.Text = "プロセスをキャプチャする";
            this.radioButton_process.UseVisualStyleBackColor = true;
            this.radioButton_process.CheckedChanged += new System.EventHandler(this.radioButton_process_CheckedChanged);
            // 
            // comboBox_process
            // 
            this.comboBox_process.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_process.Enabled = false;
            this.comboBox_process.FormattingEnabled = true;
            this.comboBox_process.Location = new System.Drawing.Point(199, 76);
            this.comboBox_process.MaxDropDownItems = 32;
            this.comboBox_process.Name = "comboBox_process";
            this.comboBox_process.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.comboBox_process.Size = new System.Drawing.Size(271, 20);
            this.comboBox_process.TabIndex = 16;
            // 
            // panel_capture
            // 
            this.panel_capture.Controls.Add(this.groupBox_record);
            this.panel_capture.Controls.Add(this.groupBox_audio);
            this.panel_capture.Controls.Add(this.groupBox_sending);
            this.panel_capture.Controls.Add(this.groupBox_capture);
            this.panel_capture.Enabled = false;
            this.panel_capture.Location = new System.Drawing.Point(12, 41);
            this.panel_capture.Name = "panel_capture";
            this.panel_capture.Size = new System.Drawing.Size(512, 368);
            this.panel_capture.TabIndex = 0;
            // 
            // groupBox_record
            // 
            this.groupBox_record.Controls.Add(this.checkBox_recordCapture);
            this.groupBox_record.Controls.Add(this.panel_record);
            this.groupBox_record.Controls.Add(this.checkBox_recordAudio);
            this.groupBox_record.Location = new System.Drawing.Point(3, 278);
            this.groupBox_record.Name = "groupBox_record";
            this.groupBox_record.Size = new System.Drawing.Size(506, 71);
            this.groupBox_record.TabIndex = 24;
            this.groupBox_record.TabStop = false;
            this.groupBox_record.Text = "録画設定";
            // 
            // textBox_recordQuality
            // 
            this.textBox_recordQuality.Location = new System.Drawing.Point(333, 3);
            this.textBox_recordQuality.MaxLength = 3;
            this.textBox_recordQuality.Name = "textBox_recordQuality";
            this.textBox_recordQuality.Size = new System.Drawing.Size(49, 19);
            this.textBox_recordQuality.TabIndex = 27;
            this.textBox_recordQuality.Text = "95";
            this.textBox_recordQuality.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_captureQuality_Validating);
            // 
            // label_recordQualty
            // 
            this.label_recordQualty.AutoSize = true;
            this.label_recordQualty.Location = new System.Drawing.Point(238, 6);
            this.label_recordQualty.Name = "label_recordQualty";
            this.label_recordQualty.Size = new System.Drawing.Size(91, 12);
            this.label_recordQualty.TabIndex = 26;
            this.label_recordQualty.Text = "画面品質(0-100)";
            // 
            // comboBox_videoCodec
            // 
            this.comboBox_videoCodec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_videoCodec.FormattingEnabled = true;
            this.comboBox_videoCodec.Items.AddRange(new object[] {
            "無圧縮",
            "MotionJpeg"});
            this.comboBox_videoCodec.Location = new System.Drawing.Point(83, 3);
            this.comboBox_videoCodec.Name = "comboBox_videoCodec";
            this.comboBox_videoCodec.Size = new System.Drawing.Size(90, 20);
            this.comboBox_videoCodec.TabIndex = 25;
            this.comboBox_videoCodec.TabStop = false;
            this.comboBox_videoCodec.SelectedIndexChanged += new System.EventHandler(this.comboBox_videoCodec_SelectedIndexChanged);
            // 
            // label_videoCodec
            // 
            this.label_videoCodec.AutoSize = true;
            this.label_videoCodec.Location = new System.Drawing.Point(3, 6);
            this.label_videoCodec.Name = "label_videoCodec";
            this.label_videoCodec.Size = new System.Drawing.Size(74, 12);
            this.label_videoCodec.TabIndex = 24;
            this.label_videoCodec.Text = "エンコード方式";
            // 
            // checkBox_recordAudio
            // 
            this.checkBox_recordAudio.AutoSize = true;
            this.checkBox_recordAudio.Location = new System.Drawing.Point(6, 43);
            this.checkBox_recordAudio.Name = "checkBox_recordAudio";
            this.checkBox_recordAudio.Size = new System.Drawing.Size(67, 16);
            this.checkBox_recordAudio.TabIndex = 23;
            this.checkBox_recordAudio.Text = "録音する";
            this.checkBox_recordAudio.UseVisualStyleBackColor = true;
            // 
            // checkBox_recordCapture
            // 
            this.checkBox_recordCapture.AutoSize = true;
            this.checkBox_recordCapture.Location = new System.Drawing.Point(6, 21);
            this.checkBox_recordCapture.Name = "checkBox_recordCapture";
            this.checkBox_recordCapture.Size = new System.Drawing.Size(67, 16);
            this.checkBox_recordCapture.TabIndex = 22;
            this.checkBox_recordCapture.Text = "録画する";
            this.checkBox_recordCapture.UseVisualStyleBackColor = true;
            this.checkBox_recordCapture.CheckedChanged += new System.EventHandler(this.checkBox_recordCapture_CheckedChanged);
            // 
            // groupBox_audio
            // 
            this.groupBox_audio.Controls.Add(this.button_reloadWaveInDevices);
            this.groupBox_audio.Controls.Add(this.label_waveInDevices);
            this.groupBox_audio.Controls.Add(this.comboBox_waveInDevices);
            this.groupBox_audio.Controls.Add(this.checkBox_stereo);
            this.groupBox_audio.Controls.Add(this.comboBox_audioQuality);
            this.groupBox_audio.Controls.Add(this.label_audioQuality);
            this.groupBox_audio.Controls.Add(this.checkBox_sendAudio);
            this.groupBox_audio.Location = new System.Drawing.Point(3, 112);
            this.groupBox_audio.Name = "groupBox_audio";
            this.groupBox_audio.Size = new System.Drawing.Size(506, 72);
            this.groupBox_audio.TabIndex = 23;
            this.groupBox_audio.TabStop = false;
            this.groupBox_audio.Text = "音声設定";
            // 
            // button_reloadWaveInDevices
            // 
            this.button_reloadWaveInDevices.Enabled = false;
            this.button_reloadWaveInDevices.Image = ((System.Drawing.Image)(resources.GetObject("button_reloadWaveInDevices.Image")));
            this.button_reloadWaveInDevices.Location = new System.Drawing.Point(476, 13);
            this.button_reloadWaveInDevices.Name = "button_reloadWaveInDevices";
            this.button_reloadWaveInDevices.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.button_reloadWaveInDevices.Size = new System.Drawing.Size(24, 24);
            this.button_reloadWaveInDevices.TabIndex = 27;
            this.button_reloadWaveInDevices.UseVisualStyleBackColor = true;
            this.button_reloadWaveInDevices.Click += new System.EventHandler(this.button_reloadWaveInDevices_Click);
            // 
            // label_waveInDevices
            // 
            this.label_waveInDevices.AutoSize = true;
            this.label_waveInDevices.Enabled = false;
            this.label_waveInDevices.Location = new System.Drawing.Point(202, 19);
            this.label_waveInDevices.Name = "label_waveInDevices";
            this.label_waveInDevices.Size = new System.Drawing.Size(67, 12);
            this.label_waveInDevices.TabIndex = 26;
            this.label_waveInDevices.Text = "録音デバイス";
            // 
            // comboBox_waveInDevices
            // 
            this.comboBox_waveInDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_waveInDevices.Enabled = false;
            this.comboBox_waveInDevices.FormattingEnabled = true;
            this.comboBox_waveInDevices.Location = new System.Drawing.Point(275, 16);
            this.comboBox_waveInDevices.Name = "comboBox_waveInDevices";
            this.comboBox_waveInDevices.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.comboBox_waveInDevices.Size = new System.Drawing.Size(195, 20);
            this.comboBox_waveInDevices.TabIndex = 25;
            this.comboBox_waveInDevices.TabStop = false;
            this.comboBox_waveInDevices.SelectedIndexChanged += new System.EventHandler(this.comboBox_waveInDevices_SelectedIndexChanged);
            // 
            // checkBox_stereo
            // 
            this.checkBox_stereo.AutoSize = true;
            this.checkBox_stereo.Enabled = false;
            this.checkBox_stereo.Location = new System.Drawing.Point(275, 48);
            this.checkBox_stereo.Name = "checkBox_stereo";
            this.checkBox_stereo.Size = new System.Drawing.Size(60, 16);
            this.checkBox_stereo.TabIndex = 24;
            this.checkBox_stereo.Text = "ステレオ";
            this.checkBox_stereo.UseVisualStyleBackColor = true;
            // 
            // comboBox_audioQuality
            // 
            this.comboBox_audioQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_audioQuality.Enabled = false;
            this.comboBox_audioQuality.FormattingEnabled = true;
            this.comboBox_audioQuality.Items.AddRange(new object[] {
            "4000",
            "8000",
            "11025",
            "16000",
            "22050",
            "32000",
            "44100",
            "48000"});
            this.comboBox_audioQuality.Location = new System.Drawing.Point(410, 46);
            this.comboBox_audioQuality.Name = "comboBox_audioQuality";
            this.comboBox_audioQuality.Size = new System.Drawing.Size(90, 20);
            this.comboBox_audioQuality.TabIndex = 23;
            this.comboBox_audioQuality.TabStop = false;
            // 
            // label_audioQuality
            // 
            this.label_audioQuality.AutoSize = true;
            this.label_audioQuality.Enabled = false;
            this.label_audioQuality.Location = new System.Drawing.Point(342, 49);
            this.label_audioQuality.Name = "label_audioQuality";
            this.label_audioQuality.Size = new System.Drawing.Size(62, 12);
            this.label_audioQuality.TabIndex = 22;
            this.label_audioQuality.Text = "周波数(Hz)";
            // 
            // checkBox_sendAudio
            // 
            this.checkBox_sendAudio.AutoSize = true;
            this.checkBox_sendAudio.Location = new System.Drawing.Point(6, 18);
            this.checkBox_sendAudio.Name = "checkBox_sendAudio";
            this.checkBox_sendAudio.Size = new System.Drawing.Size(100, 16);
            this.checkBox_sendAudio.TabIndex = 21;
            this.checkBox_sendAudio.Text = "音声を送信する";
            this.checkBox_sendAudio.UseVisualStyleBackColor = true;
            this.checkBox_sendAudio.CheckedChanged += new System.EventHandler(this.checkBox_recordAudio_CheckedChanged);
            // 
            // groupBox_sending
            // 
            this.groupBox_sending.Controls.Add(this.textBox_captureFps);
            this.groupBox_sending.Controls.Add(this.comboBox_captureScale);
            this.groupBox_sending.Controls.Add(this.label_captureScale);
            this.groupBox_sending.Controls.Add(this.textBox_captureQuality);
            this.groupBox_sending.Controls.Add(this.label_divisionNumber);
            this.groupBox_sending.Controls.Add(this.label_imageCompressQuality);
            this.groupBox_sending.Controls.Add(this.label_interval);
            this.groupBox_sending.Controls.Add(this.comboBox_divisionNumber);
            this.groupBox_sending.Location = new System.Drawing.Point(3, 190);
            this.groupBox_sending.Name = "groupBox_sending";
            this.groupBox_sending.Size = new System.Drawing.Size(506, 82);
            this.groupBox_sending.TabIndex = 22;
            this.groupBox_sending.TabStop = false;
            this.groupBox_sending.Text = "送信設定";
            // 
            // textBox_captureFps
            // 
            this.textBox_captureFps.Location = new System.Drawing.Point(451, 49);
            this.textBox_captureFps.MaxLength = 3;
            this.textBox_captureFps.Name = "textBox_captureFps";
            this.textBox_captureFps.Size = new System.Drawing.Size(49, 19);
            this.textBox_captureFps.TabIndex = 29;
            this.textBox_captureFps.Text = "10";
            this.textBox_captureFps.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_captureFps_Validating);
            // 
            // comboBox_captureScale
            // 
            this.comboBox_captureScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_captureScale.FormattingEnabled = true;
            this.comboBox_captureScale.Items.AddRange(new object[] {
            "x1.0",
            "x0.9",
            "x0.8",
            "x0.7",
            "x0.6",
            "x0.5",
            "x0.4",
            "x0.3",
            "x0.2",
            "x0.1"});
            this.comboBox_captureScale.Location = new System.Drawing.Point(201, 21);
            this.comboBox_captureScale.Name = "comboBox_captureScale";
            this.comboBox_captureScale.Size = new System.Drawing.Size(67, 20);
            this.comboBox_captureScale.TabIndex = 28;
            this.comboBox_captureScale.TabStop = false;
            // 
            // label_captureScale
            // 
            this.label_captureScale.AutoSize = true;
            this.label_captureScale.Location = new System.Drawing.Point(84, 24);
            this.label_captureScale.Name = "label_captureScale";
            this.label_captureScale.Size = new System.Drawing.Size(111, 12);
            this.label_captureScale.TabIndex = 27;
            this.label_captureScale.Text = "画面解像度(スケール)";
            // 
            // textBox_captureQuality
            // 
            this.textBox_captureQuality.Location = new System.Drawing.Point(451, 21);
            this.textBox_captureQuality.MaxLength = 3;
            this.textBox_captureQuality.Name = "textBox_captureQuality";
            this.textBox_captureQuality.Size = new System.Drawing.Size(49, 19);
            this.textBox_captureQuality.TabIndex = 26;
            this.textBox_captureQuality.Text = "50";
            this.textBox_captureQuality.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_captureQuality_KeyPress);
            this.textBox_captureQuality.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_captureQuality_Validating);
            // 
            // label_divisionNumber
            // 
            this.label_divisionNumber.AutoSize = true;
            this.label_divisionNumber.Location = new System.Drawing.Point(130, 52);
            this.label_divisionNumber.Name = "label_divisionNumber";
            this.label_divisionNumber.Size = new System.Drawing.Size(65, 12);
            this.label_divisionNumber.TabIndex = 24;
            this.label_divisionNumber.Text = "画面分割数";
            // 
            // label_imageCompressQuality
            // 
            this.label_imageCompressQuality.AutoSize = true;
            this.label_imageCompressQuality.Location = new System.Drawing.Point(353, 24);
            this.label_imageCompressQuality.Name = "label_imageCompressQuality";
            this.label_imageCompressQuality.Size = new System.Drawing.Size(91, 12);
            this.label_imageCompressQuality.TabIndex = 19;
            this.label_imageCompressQuality.Text = "画面品質(0-100)";
            // 
            // comboBox_divisionNumber
            // 
            this.comboBox_divisionNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_divisionNumber.FormattingEnabled = true;
            this.comboBox_divisionNumber.Items.AddRange(new object[] {
            "1x1",
            "2x2",
            "3x3",
            "4x4",
            "5x5",
            "6x6",
            "7x7",
            "8x8"});
            this.comboBox_divisionNumber.Location = new System.Drawing.Point(201, 49);
            this.comboBox_divisionNumber.Name = "comboBox_divisionNumber";
            this.comboBox_divisionNumber.Size = new System.Drawing.Size(67, 20);
            this.comboBox_divisionNumber.TabIndex = 25;
            this.comboBox_divisionNumber.TabStop = false;
            // 
            // groupBox_capture
            // 
            this.groupBox_capture.Controls.Add(this.checkBox_showOverRayForm);
            this.groupBox_capture.Controls.Add(this.button_areaReset);
            this.groupBox_capture.Controls.Add(this.radioButton_process);
            this.groupBox_capture.Controls.Add(this.radioButton_area);
            this.groupBox_capture.Controls.Add(this.comboBox_process);
            this.groupBox_capture.Controls.Add(this.button_reloadProcesses);
            this.groupBox_capture.Controls.Add(this.button_area);
            this.groupBox_capture.Location = new System.Drawing.Point(3, 3);
            this.groupBox_capture.Name = "groupBox_capture";
            this.groupBox_capture.Size = new System.Drawing.Size(506, 103);
            this.groupBox_capture.TabIndex = 21;
            this.groupBox_capture.TabStop = false;
            this.groupBox_capture.Text = "キャプチャ設定";
            // 
            // checkBox_showOverRayForm
            // 
            this.checkBox_showOverRayForm.AutoSize = true;
            this.checkBox_showOverRayForm.Location = new System.Drawing.Point(356, 19);
            this.checkBox_showOverRayForm.Name = "checkBox_showOverRayForm";
            this.checkBox_showOverRayForm.Size = new System.Drawing.Size(144, 16);
            this.checkBox_showOverRayForm.TabIndex = 25;
            this.checkBox_showOverRayForm.Text = "キャプチャ領域を表示する";
            this.checkBox_showOverRayForm.UseVisualStyleBackColor = true;
            this.checkBox_showOverRayForm.CheckedChanged += new System.EventHandler(this.checkBox_showOverRayForm_CheckedChanged);
            // 
            // button_areaReset
            // 
            this.button_areaReset.Location = new System.Drawing.Point(410, 41);
            this.button_areaReset.Name = "button_areaReset";
            this.button_areaReset.Size = new System.Drawing.Size(90, 23);
            this.button_areaReset.TabIndex = 19;
            this.button_areaReset.Text = "領域リセット";
            this.button_areaReset.UseVisualStyleBackColor = true;
            this.button_areaReset.Click += new System.EventHandler(this.button_areaReset_Click);
            // 
            // radioButton_area
            // 
            this.radioButton_area.AutoSize = true;
            this.radioButton_area.Checked = true;
            this.radioButton_area.Location = new System.Drawing.Point(6, 18);
            this.radioButton_area.Name = "radioButton_area";
            this.radioButton_area.Size = new System.Drawing.Size(146, 16);
            this.radioButton_area.TabIndex = 17;
            this.radioButton_area.TabStop = true;
            this.radioButton_area.Text = "デスクトップをキャプチャする";
            this.radioButton_area.UseVisualStyleBackColor = true;
            this.radioButton_area.CheckedChanged += new System.EventHandler(this.radioButton_area_CheckedChanged);
            // 
            // button_reloadProcesses
            // 
            this.button_reloadProcesses.Image = ((System.Drawing.Image)(resources.GetObject("button_reloadProcesses.Image")));
            this.button_reloadProcesses.Location = new System.Drawing.Point(476, 73);
            this.button_reloadProcesses.Name = "button_reloadProcesses";
            this.button_reloadProcesses.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.button_reloadProcesses.Size = new System.Drawing.Size(24, 24);
            this.button_reloadProcesses.TabIndex = 18;
            this.button_reloadProcesses.UseVisualStyleBackColor = true;
            this.button_reloadProcesses.Click += new System.EventHandler(this.button_reloadProcesses_Click);
            // 
            // panel_record
            // 
            this.panel_record.Controls.Add(this.textBox_recordQuality);
            this.panel_record.Controls.Add(this.label_videoCodec);
            this.panel_record.Controls.Add(this.label_recordQualty);
            this.panel_record.Controls.Add(this.comboBox_videoCodec);
            this.panel_record.Enabled = false;
            this.panel_record.Location = new System.Drawing.Point(118, 16);
            this.panel_record.Name = "panel_record";
            this.panel_record.Size = new System.Drawing.Size(391, 27);
            this.panel_record.TabIndex = 28;
            // 
            // Form_tcp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 469);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_disconnect);
            this.Controls.Add(this.button_stopCapture);
            this.Controls.Add(this.button_startCapture);
            this.Controls.Add(this.button_connect);
            this.Controls.Add(this.label_ip);
            this.Controls.Add(this.textBox_ip);
            this.Controls.Add(this.panel_capture);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form_tcp";
            this.Text = "ScreenShare";
            this.panel_capture.ResumeLayout(false);
            this.groupBox_record.ResumeLayout(false);
            this.groupBox_record.PerformLayout();
            this.groupBox_audio.ResumeLayout(false);
            this.groupBox_audio.PerformLayout();
            this.groupBox_sending.ResumeLayout(false);
            this.groupBox_sending.PerformLayout();
            this.groupBox_capture.ResumeLayout(false);
            this.groupBox_capture.PerformLayout();
            this.panel_record.ResumeLayout(false);
            this.panel_record.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_startCapture;
        private System.Windows.Forms.Button button_stopCapture;
        private System.Windows.Forms.TextBox textBox_ip;
        private System.Windows.Forms.Label label_ip;
        private System.Windows.Forms.Button button_connect;
        private System.Windows.Forms.Button button_disconnect;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Label label_interval;
        private System.Windows.Forms.Button button_area;
        private System.Windows.Forms.RadioButton radioButton_process;
        private System.Windows.Forms.ComboBox comboBox_process;
        private System.Windows.Forms.Panel panel_capture;
        private System.Windows.Forms.Button button_reloadProcesses;
        private System.Windows.Forms.RadioButton radioButton_area;
        private System.Windows.Forms.Label label_imageCompressQuality;
        private System.Windows.Forms.GroupBox groupBox_capture;
        private System.Windows.Forms.GroupBox groupBox_sending;
        private System.Windows.Forms.Button button_areaReset;
        private System.Windows.Forms.CheckBox checkBox_sendAudio;
        private System.Windows.Forms.GroupBox groupBox_audio;
        private System.Windows.Forms.ComboBox comboBox_audioQuality;
        private System.Windows.Forms.Label label_audioQuality;
        private System.Windows.Forms.CheckBox checkBox_stereo;
        private System.Windows.Forms.ComboBox comboBox_divisionNumber;
        private System.Windows.Forms.Label label_divisionNumber;
        private System.Windows.Forms.TextBox textBox_captureQuality;
        private System.Windows.Forms.ComboBox comboBox_captureScale;
        private System.Windows.Forms.Label label_captureScale;
        private System.Windows.Forms.CheckBox checkBox_showOverRayForm;
        private System.Windows.Forms.CheckBox checkBox_recordCapture;
        private System.Windows.Forms.TextBox textBox_captureFps;
        private System.Windows.Forms.GroupBox groupBox_record;
        private System.Windows.Forms.CheckBox checkBox_recordAudio;
        private System.Windows.Forms.TextBox textBox_recordQuality;
        private System.Windows.Forms.Label label_recordQualty;
        private System.Windows.Forms.ComboBox comboBox_videoCodec;
        private System.Windows.Forms.Label label_videoCodec;
        private System.Windows.Forms.Button button_reloadWaveInDevices;
        private System.Windows.Forms.Label label_waveInDevices;
        private System.Windows.Forms.ComboBox comboBox_waveInDevices;
        private System.Windows.Forms.Panel panel_record;
    }
}

