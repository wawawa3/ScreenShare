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
            this.checkBox_recordCapture = new System.Windows.Forms.CheckBox();
            this.panel_record = new System.Windows.Forms.Panel();
            this.textBox_recordQuality = new System.Windows.Forms.TextBox();
            this.label_videoCodec = new System.Windows.Forms.Label();
            this.label_recordQualty = new System.Windows.Forms.Label();
            this.comboBox_videoCodec = new System.Windows.Forms.ComboBox();
            this.checkBox_recordAudio = new System.Windows.Forms.CheckBox();
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
            this.label_Client = new System.Windows.Forms.Label();
            this.label_ConnectionNum = new System.Windows.Forms.Label();
            this.panel_capture.SuspendLayout();
            this.groupBox_record.SuspendLayout();
            this.panel_record.SuspendLayout();
            this.groupBox_audio.SuspendLayout();
            this.groupBox_sending.SuspendLayout();
            this.groupBox_capture.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_startCapture
            // 
            resources.ApplyResources(this.button_startCapture, "button_startCapture");
            this.button_startCapture.Name = "button_startCapture";
            this.button_startCapture.UseVisualStyleBackColor = true;
            this.button_startCapture.Click += new System.EventHandler(this.button_startCapture_Click);
            // 
            // button_stopCapture
            // 
            resources.ApplyResources(this.button_stopCapture, "button_stopCapture");
            this.button_stopCapture.Name = "button_stopCapture";
            this.button_stopCapture.UseVisualStyleBackColor = true;
            this.button_stopCapture.Click += new System.EventHandler(this.button_stopCapture_Click);
            // 
            // textBox_ip
            // 
            resources.ApplyResources(this.textBox_ip, "textBox_ip");
            this.textBox_ip.Name = "textBox_ip";
            this.textBox_ip.ReadOnly = true;
            this.textBox_ip.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_ip_KeyPress);
            // 
            // label_ip
            // 
            resources.ApplyResources(this.label_ip, "label_ip");
            this.label_ip.Name = "label_ip";
            // 
            // button_connect
            // 
            resources.ApplyResources(this.button_connect, "button_connect");
            this.button_connect.Name = "button_connect";
            this.button_connect.UseVisualStyleBackColor = true;
            this.button_connect.Click += new System.EventHandler(this.button_connect_Click);
            // 
            // button_disconnect
            // 
            resources.ApplyResources(this.button_disconnect, "button_disconnect");
            this.button_disconnect.Name = "button_disconnect";
            this.button_disconnect.UseVisualStyleBackColor = true;
            this.button_disconnect.Click += new System.EventHandler(this.button_disconnect_Click);
            // 
            // label_message
            // 
            resources.ApplyResources(this.label_message, "label_message");
            this.label_message.BackColor = System.Drawing.SystemColors.Control;
            this.label_message.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_message.Name = "label_message";
            // 
            // label_interval
            // 
            resources.ApplyResources(this.label_interval, "label_interval");
            this.label_interval.Name = "label_interval";
            // 
            // button_area
            // 
            resources.ApplyResources(this.button_area, "button_area");
            this.button_area.Name = "button_area";
            this.button_area.UseVisualStyleBackColor = true;
            this.button_area.Click += new System.EventHandler(this.button_area_Click);
            // 
            // radioButton_process
            // 
            resources.ApplyResources(this.radioButton_process, "radioButton_process");
            this.radioButton_process.Name = "radioButton_process";
            this.radioButton_process.UseVisualStyleBackColor = true;
            this.radioButton_process.CheckedChanged += new System.EventHandler(this.radioButton_process_CheckedChanged);
            // 
            // comboBox_process
            // 
            resources.ApplyResources(this.comboBox_process, "comboBox_process");
            this.comboBox_process.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_process.FormattingEnabled = true;
            this.comboBox_process.Name = "comboBox_process";
            // 
            // panel_capture
            // 
            resources.ApplyResources(this.panel_capture, "panel_capture");
            this.panel_capture.Controls.Add(this.groupBox_record);
            this.panel_capture.Controls.Add(this.groupBox_audio);
            this.panel_capture.Controls.Add(this.groupBox_sending);
            this.panel_capture.Controls.Add(this.groupBox_capture);
            this.panel_capture.Name = "panel_capture";
            // 
            // groupBox_record
            // 
            resources.ApplyResources(this.groupBox_record, "groupBox_record");
            this.groupBox_record.Controls.Add(this.checkBox_recordCapture);
            this.groupBox_record.Controls.Add(this.panel_record);
            this.groupBox_record.Controls.Add(this.checkBox_recordAudio);
            this.groupBox_record.Name = "groupBox_record";
            this.groupBox_record.TabStop = false;
            // 
            // checkBox_recordCapture
            // 
            resources.ApplyResources(this.checkBox_recordCapture, "checkBox_recordCapture");
            this.checkBox_recordCapture.Name = "checkBox_recordCapture";
            this.checkBox_recordCapture.UseVisualStyleBackColor = true;
            this.checkBox_recordCapture.CheckedChanged += new System.EventHandler(this.checkBox_recordCapture_CheckedChanged);
            // 
            // panel_record
            // 
            resources.ApplyResources(this.panel_record, "panel_record");
            this.panel_record.Controls.Add(this.textBox_recordQuality);
            this.panel_record.Controls.Add(this.label_videoCodec);
            this.panel_record.Controls.Add(this.label_recordQualty);
            this.panel_record.Controls.Add(this.comboBox_videoCodec);
            this.panel_record.Name = "panel_record";
            // 
            // textBox_recordQuality
            // 
            resources.ApplyResources(this.textBox_recordQuality, "textBox_recordQuality");
            this.textBox_recordQuality.Name = "textBox_recordQuality";
            this.textBox_recordQuality.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_captureQuality_Validating);
            // 
            // label_videoCodec
            // 
            resources.ApplyResources(this.label_videoCodec, "label_videoCodec");
            this.label_videoCodec.Name = "label_videoCodec";
            // 
            // label_recordQualty
            // 
            resources.ApplyResources(this.label_recordQualty, "label_recordQualty");
            this.label_recordQualty.Name = "label_recordQualty";
            // 
            // comboBox_videoCodec
            // 
            resources.ApplyResources(this.comboBox_videoCodec, "comboBox_videoCodec");
            this.comboBox_videoCodec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_videoCodec.FormattingEnabled = true;
            this.comboBox_videoCodec.Items.AddRange(new object[] {
            resources.GetString("comboBox_videoCodec.Items"),
            resources.GetString("comboBox_videoCodec.Items1")});
            this.comboBox_videoCodec.Name = "comboBox_videoCodec";
            this.comboBox_videoCodec.TabStop = false;
            this.comboBox_videoCodec.SelectedIndexChanged += new System.EventHandler(this.comboBox_videoCodec_SelectedIndexChanged);
            // 
            // checkBox_recordAudio
            // 
            resources.ApplyResources(this.checkBox_recordAudio, "checkBox_recordAudio");
            this.checkBox_recordAudio.Name = "checkBox_recordAudio";
            this.checkBox_recordAudio.UseVisualStyleBackColor = true;
            // 
            // groupBox_audio
            // 
            resources.ApplyResources(this.groupBox_audio, "groupBox_audio");
            this.groupBox_audio.Controls.Add(this.button_reloadWaveInDevices);
            this.groupBox_audio.Controls.Add(this.label_waveInDevices);
            this.groupBox_audio.Controls.Add(this.comboBox_waveInDevices);
            this.groupBox_audio.Controls.Add(this.checkBox_stereo);
            this.groupBox_audio.Controls.Add(this.comboBox_audioQuality);
            this.groupBox_audio.Controls.Add(this.label_audioQuality);
            this.groupBox_audio.Controls.Add(this.checkBox_sendAudio);
            this.groupBox_audio.Name = "groupBox_audio";
            this.groupBox_audio.TabStop = false;
            // 
            // button_reloadWaveInDevices
            // 
            resources.ApplyResources(this.button_reloadWaveInDevices, "button_reloadWaveInDevices");
            this.button_reloadWaveInDevices.Name = "button_reloadWaveInDevices";
            this.button_reloadWaveInDevices.UseVisualStyleBackColor = true;
            this.button_reloadWaveInDevices.Click += new System.EventHandler(this.button_reloadWaveInDevices_Click);
            // 
            // label_waveInDevices
            // 
            resources.ApplyResources(this.label_waveInDevices, "label_waveInDevices");
            this.label_waveInDevices.Name = "label_waveInDevices";
            // 
            // comboBox_waveInDevices
            // 
            resources.ApplyResources(this.comboBox_waveInDevices, "comboBox_waveInDevices");
            this.comboBox_waveInDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_waveInDevices.FormattingEnabled = true;
            this.comboBox_waveInDevices.Name = "comboBox_waveInDevices";
            this.comboBox_waveInDevices.TabStop = false;
            this.comboBox_waveInDevices.SelectedIndexChanged += new System.EventHandler(this.comboBox_waveInDevices_SelectedIndexChanged);
            // 
            // checkBox_stereo
            // 
            resources.ApplyResources(this.checkBox_stereo, "checkBox_stereo");
            this.checkBox_stereo.Name = "checkBox_stereo";
            this.checkBox_stereo.UseVisualStyleBackColor = true;
            // 
            // comboBox_audioQuality
            // 
            resources.ApplyResources(this.comboBox_audioQuality, "comboBox_audioQuality");
            this.comboBox_audioQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_audioQuality.FormattingEnabled = true;
            this.comboBox_audioQuality.Items.AddRange(new object[] {
            resources.GetString("comboBox_audioQuality.Items"),
            resources.GetString("comboBox_audioQuality.Items1"),
            resources.GetString("comboBox_audioQuality.Items2"),
            resources.GetString("comboBox_audioQuality.Items3"),
            resources.GetString("comboBox_audioQuality.Items4"),
            resources.GetString("comboBox_audioQuality.Items5"),
            resources.GetString("comboBox_audioQuality.Items6"),
            resources.GetString("comboBox_audioQuality.Items7")});
            this.comboBox_audioQuality.Name = "comboBox_audioQuality";
            this.comboBox_audioQuality.TabStop = false;
            // 
            // label_audioQuality
            // 
            resources.ApplyResources(this.label_audioQuality, "label_audioQuality");
            this.label_audioQuality.Name = "label_audioQuality";
            // 
            // checkBox_sendAudio
            // 
            resources.ApplyResources(this.checkBox_sendAudio, "checkBox_sendAudio");
            this.checkBox_sendAudio.Name = "checkBox_sendAudio";
            this.checkBox_sendAudio.UseVisualStyleBackColor = true;
            this.checkBox_sendAudio.CheckedChanged += new System.EventHandler(this.checkBox_recordAudio_CheckedChanged);
            // 
            // groupBox_sending
            // 
            resources.ApplyResources(this.groupBox_sending, "groupBox_sending");
            this.groupBox_sending.Controls.Add(this.textBox_captureFps);
            this.groupBox_sending.Controls.Add(this.comboBox_captureScale);
            this.groupBox_sending.Controls.Add(this.label_captureScale);
            this.groupBox_sending.Controls.Add(this.textBox_captureQuality);
            this.groupBox_sending.Controls.Add(this.label_divisionNumber);
            this.groupBox_sending.Controls.Add(this.label_imageCompressQuality);
            this.groupBox_sending.Controls.Add(this.label_interval);
            this.groupBox_sending.Controls.Add(this.comboBox_divisionNumber);
            this.groupBox_sending.Name = "groupBox_sending";
            this.groupBox_sending.TabStop = false;
            // 
            // textBox_captureFps
            // 
            resources.ApplyResources(this.textBox_captureFps, "textBox_captureFps");
            this.textBox_captureFps.Name = "textBox_captureFps";
            this.textBox_captureFps.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_captureFps_Validating);
            // 
            // comboBox_captureScale
            // 
            resources.ApplyResources(this.comboBox_captureScale, "comboBox_captureScale");
            this.comboBox_captureScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_captureScale.FormattingEnabled = true;
            this.comboBox_captureScale.Items.AddRange(new object[] {
            resources.GetString("comboBox_captureScale.Items"),
            resources.GetString("comboBox_captureScale.Items1"),
            resources.GetString("comboBox_captureScale.Items2"),
            resources.GetString("comboBox_captureScale.Items3"),
            resources.GetString("comboBox_captureScale.Items4"),
            resources.GetString("comboBox_captureScale.Items5"),
            resources.GetString("comboBox_captureScale.Items6"),
            resources.GetString("comboBox_captureScale.Items7"),
            resources.GetString("comboBox_captureScale.Items8"),
            resources.GetString("comboBox_captureScale.Items9")});
            this.comboBox_captureScale.Name = "comboBox_captureScale";
            this.comboBox_captureScale.TabStop = false;
            // 
            // label_captureScale
            // 
            resources.ApplyResources(this.label_captureScale, "label_captureScale");
            this.label_captureScale.Name = "label_captureScale";
            // 
            // textBox_captureQuality
            // 
            resources.ApplyResources(this.textBox_captureQuality, "textBox_captureQuality");
            this.textBox_captureQuality.Name = "textBox_captureQuality";
            this.textBox_captureQuality.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_captureQuality_KeyPress);
            this.textBox_captureQuality.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_captureQuality_Validating);
            // 
            // label_divisionNumber
            // 
            resources.ApplyResources(this.label_divisionNumber, "label_divisionNumber");
            this.label_divisionNumber.Name = "label_divisionNumber";
            // 
            // label_imageCompressQuality
            // 
            resources.ApplyResources(this.label_imageCompressQuality, "label_imageCompressQuality");
            this.label_imageCompressQuality.Name = "label_imageCompressQuality";
            // 
            // comboBox_divisionNumber
            // 
            resources.ApplyResources(this.comboBox_divisionNumber, "comboBox_divisionNumber");
            this.comboBox_divisionNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_divisionNumber.FormattingEnabled = true;
            this.comboBox_divisionNumber.Items.AddRange(new object[] {
            resources.GetString("comboBox_divisionNumber.Items"),
            resources.GetString("comboBox_divisionNumber.Items1"),
            resources.GetString("comboBox_divisionNumber.Items2"),
            resources.GetString("comboBox_divisionNumber.Items3"),
            resources.GetString("comboBox_divisionNumber.Items4"),
            resources.GetString("comboBox_divisionNumber.Items5"),
            resources.GetString("comboBox_divisionNumber.Items6"),
            resources.GetString("comboBox_divisionNumber.Items7")});
            this.comboBox_divisionNumber.Name = "comboBox_divisionNumber";
            this.comboBox_divisionNumber.TabStop = false;
            // 
            // groupBox_capture
            // 
            resources.ApplyResources(this.groupBox_capture, "groupBox_capture");
            this.groupBox_capture.Controls.Add(this.checkBox_showOverRayForm);
            this.groupBox_capture.Controls.Add(this.button_areaReset);
            this.groupBox_capture.Controls.Add(this.radioButton_process);
            this.groupBox_capture.Controls.Add(this.radioButton_area);
            this.groupBox_capture.Controls.Add(this.comboBox_process);
            this.groupBox_capture.Controls.Add(this.button_reloadProcesses);
            this.groupBox_capture.Controls.Add(this.button_area);
            this.groupBox_capture.Name = "groupBox_capture";
            this.groupBox_capture.TabStop = false;
            // 
            // checkBox_showOverRayForm
            // 
            resources.ApplyResources(this.checkBox_showOverRayForm, "checkBox_showOverRayForm");
            this.checkBox_showOverRayForm.Name = "checkBox_showOverRayForm";
            this.checkBox_showOverRayForm.UseVisualStyleBackColor = true;
            this.checkBox_showOverRayForm.CheckedChanged += new System.EventHandler(this.checkBox_showOverRayForm_CheckedChanged);
            // 
            // button_areaReset
            // 
            resources.ApplyResources(this.button_areaReset, "button_areaReset");
            this.button_areaReset.Name = "button_areaReset";
            this.button_areaReset.UseVisualStyleBackColor = true;
            this.button_areaReset.Click += new System.EventHandler(this.button_areaReset_Click);
            // 
            // radioButton_area
            // 
            resources.ApplyResources(this.radioButton_area, "radioButton_area");
            this.radioButton_area.Checked = true;
            this.radioButton_area.Name = "radioButton_area";
            this.radioButton_area.TabStop = true;
            this.radioButton_area.UseVisualStyleBackColor = true;
            this.radioButton_area.CheckedChanged += new System.EventHandler(this.radioButton_area_CheckedChanged);
            // 
            // button_reloadProcesses
            // 
            resources.ApplyResources(this.button_reloadProcesses, "button_reloadProcesses");
            this.button_reloadProcesses.Name = "button_reloadProcesses";
            this.button_reloadProcesses.UseVisualStyleBackColor = true;
            this.button_reloadProcesses.Click += new System.EventHandler(this.button_reloadProcesses_Click);
            // 
            // label_Client
            // 
            resources.ApplyResources(this.label_Client, "label_Client");
            this.label_Client.Name = "label_Client";
            // 
            // label_ConnectionNum
            // 
            resources.ApplyResources(this.label_ConnectionNum, "label_ConnectionNum");
            this.label_ConnectionNum.Name = "label_ConnectionNum";
            // 
            // Form_tcp
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label_ConnectionNum);
            this.Controls.Add(this.label_Client);
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
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_tcp_FormClosed);
            this.panel_capture.ResumeLayout(false);
            this.groupBox_record.ResumeLayout(false);
            this.groupBox_record.PerformLayout();
            this.panel_record.ResumeLayout(false);
            this.panel_record.PerformLayout();
            this.groupBox_audio.ResumeLayout(false);
            this.groupBox_audio.PerformLayout();
            this.groupBox_sending.ResumeLayout(false);
            this.groupBox_sending.PerformLayout();
            this.groupBox_capture.ResumeLayout(false);
            this.groupBox_capture.PerformLayout();
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
        private System.Windows.Forms.Label label_Client;
        private System.Windows.Forms.Label label_ConnectionNum;
    }
}

