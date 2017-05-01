namespace ScreenShare
{
    partial class Form_ImageCast
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_ImageCast));
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
            this.groupBox_recording = new System.Windows.Forms.GroupBox();
            this.checkBox_recordVideo = new System.Windows.Forms.CheckBox();
            this.checkBox_recordAudio = new System.Windows.Forms.CheckBox();
            this.groupBox_voice = new System.Windows.Forms.GroupBox();
            this.panel_voice = new System.Windows.Forms.Panel();
            this.label_audioQuality = new System.Windows.Forms.Label();
            this.checkBox_stereo = new System.Windows.Forms.CheckBox();
            this.label_waveInDevices = new System.Windows.Forms.Label();
            this.comboBox_audioQuality = new System.Windows.Forms.ComboBox();
            this.comboBox_waveInDevices = new System.Windows.Forms.ComboBox();
            this.checkBox_captureVoice = new System.Windows.Forms.CheckBox();
            this.groupBox_casting = new System.Windows.Forms.GroupBox();
            this.panel_manualCasting = new System.Windows.Forms.Panel();
            this.label_captureQuality = new System.Windows.Forms.Label();
            this.textBox_captureFps = new System.Windows.Forms.TextBox();
            this.label_captureScale = new System.Windows.Forms.Label();
            this.textBox_captureQuality = new System.Windows.Forms.TextBox();
            this.comboBox_captureScale = new System.Windows.Forms.ComboBox();
            this.label_divisionNumber = new System.Windows.Forms.Label();
            this.comboBox_divisionNumber = new System.Windows.Forms.ComboBox();
            this.groupBox_capture = new System.Windows.Forms.GroupBox();
            this.panel_captureDesktop = new System.Windows.Forms.Panel();
            this.button_areaReset = new System.Windows.Forms.Button();
            this.checkBox_showOverRayForm = new System.Windows.Forms.CheckBox();
            this.label_targetDisplay = new System.Windows.Forms.Label();
            this.comboBox_targetDisplay = new System.Windows.Forms.ComboBox();
            this.radioButton_desktop = new System.Windows.Forms.RadioButton();
            this.label_Client = new System.Windows.Forms.Label();
            this.label_ConnectionNum = new System.Windows.Forms.Label();
            this.panel_capture.SuspendLayout();
            this.groupBox_recording.SuspendLayout();
            this.groupBox_voice.SuspendLayout();
            this.panel_voice.SuspendLayout();
            this.groupBox_casting.SuspendLayout();
            this.panel_manualCasting.SuspendLayout();
            this.groupBox_capture.SuspendLayout();
            this.panel_captureDesktop.SuspendLayout();
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
            this.comboBox_process.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.comboBox_process, "comboBox_process");
            this.comboBox_process.FormattingEnabled = true;
            this.comboBox_process.Name = "comboBox_process";
            this.comboBox_process.Click += new System.EventHandler(this.comboBox_processes_Click);
            // 
            // panel_capture
            // 
            resources.ApplyResources(this.panel_capture, "panel_capture");
            this.panel_capture.Controls.Add(this.groupBox_recording);
            this.panel_capture.Controls.Add(this.groupBox_voice);
            this.panel_capture.Controls.Add(this.groupBox_casting);
            this.panel_capture.Controls.Add(this.groupBox_capture);
            this.panel_capture.Name = "panel_capture";
            // 
            // groupBox_recording
            // 
            resources.ApplyResources(this.groupBox_recording, "groupBox_recording");
            this.groupBox_recording.Controls.Add(this.checkBox_recordVideo);
            this.groupBox_recording.Controls.Add(this.checkBox_recordAudio);
            this.groupBox_recording.Name = "groupBox_recording";
            this.groupBox_recording.TabStop = false;
            // 
            // checkBox_recordVideo
            // 
            resources.ApplyResources(this.checkBox_recordVideo, "checkBox_recordVideo");
            this.checkBox_recordVideo.Name = "checkBox_recordVideo";
            this.checkBox_recordVideo.UseVisualStyleBackColor = true;
            this.checkBox_recordVideo.CheckedChanged += new System.EventHandler(this.checkBox_recordCapture_CheckedChanged);
            // 
            // checkBox_recordAudio
            // 
            resources.ApplyResources(this.checkBox_recordAudio, "checkBox_recordAudio");
            this.checkBox_recordAudio.Name = "checkBox_recordAudio";
            this.checkBox_recordAudio.UseVisualStyleBackColor = true;
            // 
            // groupBox_voice
            // 
            resources.ApplyResources(this.groupBox_voice, "groupBox_voice");
            this.groupBox_voice.Controls.Add(this.panel_voice);
            this.groupBox_voice.Controls.Add(this.checkBox_captureVoice);
            this.groupBox_voice.Name = "groupBox_voice";
            this.groupBox_voice.TabStop = false;
            // 
            // panel_voice
            // 
            resources.ApplyResources(this.panel_voice, "panel_voice");
            this.panel_voice.Controls.Add(this.label_audioQuality);
            this.panel_voice.Controls.Add(this.checkBox_stereo);
            this.panel_voice.Controls.Add(this.label_waveInDevices);
            this.panel_voice.Controls.Add(this.comboBox_audioQuality);
            this.panel_voice.Controls.Add(this.comboBox_waveInDevices);
            this.panel_voice.Name = "panel_voice";
            // 
            // label_audioQuality
            // 
            resources.ApplyResources(this.label_audioQuality, "label_audioQuality");
            this.label_audioQuality.Name = "label_audioQuality";
            // 
            // checkBox_stereo
            // 
            resources.ApplyResources(this.checkBox_stereo, "checkBox_stereo");
            this.checkBox_stereo.Name = "checkBox_stereo";
            this.checkBox_stereo.UseVisualStyleBackColor = true;
            // 
            // label_waveInDevices
            // 
            resources.ApplyResources(this.label_waveInDevices, "label_waveInDevices");
            this.label_waveInDevices.Name = "label_waveInDevices";
            // 
            // comboBox_audioQuality
            // 
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
            resources.ApplyResources(this.comboBox_audioQuality, "comboBox_audioQuality");
            this.comboBox_audioQuality.Name = "comboBox_audioQuality";
            this.comboBox_audioQuality.TabStop = false;
            // 
            // comboBox_waveInDevices
            // 
            this.comboBox_waveInDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_waveInDevices.FormattingEnabled = true;
            resources.ApplyResources(this.comboBox_waveInDevices, "comboBox_waveInDevices");
            this.comboBox_waveInDevices.Name = "comboBox_waveInDevices";
            this.comboBox_waveInDevices.TabStop = false;
            this.comboBox_waveInDevices.SelectedIndexChanged += new System.EventHandler(this.comboBox_waveInDevices_SelectedIndexChanged);
            this.comboBox_waveInDevices.Click += new System.EventHandler(this.comboBox_waveInDevices_Click);
            // 
            // checkBox_captureVoice
            // 
            resources.ApplyResources(this.checkBox_captureVoice, "checkBox_captureVoice");
            this.checkBox_captureVoice.Name = "checkBox_captureVoice";
            this.checkBox_captureVoice.UseVisualStyleBackColor = true;
            this.checkBox_captureVoice.CheckedChanged += new System.EventHandler(this.checkBox_recordAudio_CheckedChanged);
            // 
            // groupBox_casting
            // 
            resources.ApplyResources(this.groupBox_casting, "groupBox_casting");
            this.groupBox_casting.Controls.Add(this.panel_manualCasting);
            this.groupBox_casting.Name = "groupBox_casting";
            this.groupBox_casting.TabStop = false;
            // 
            // panel_manualCasting
            // 
            resources.ApplyResources(this.panel_manualCasting, "panel_manualCasting");
            this.panel_manualCasting.Controls.Add(this.label_interval);
            this.panel_manualCasting.Controls.Add(this.label_captureQuality);
            this.panel_manualCasting.Controls.Add(this.textBox_captureFps);
            this.panel_manualCasting.Controls.Add(this.label_captureScale);
            this.panel_manualCasting.Controls.Add(this.textBox_captureQuality);
            this.panel_manualCasting.Controls.Add(this.comboBox_captureScale);
            this.panel_manualCasting.Controls.Add(this.label_divisionNumber);
            this.panel_manualCasting.Controls.Add(this.comboBox_divisionNumber);
            this.panel_manualCasting.Name = "panel_manualCasting";
            // 
            // label_captureQuality
            // 
            resources.ApplyResources(this.label_captureQuality, "label_captureQuality");
            this.label_captureQuality.Name = "label_captureQuality";
            // 
            // textBox_captureFps
            // 
            resources.ApplyResources(this.textBox_captureFps, "textBox_captureFps");
            this.textBox_captureFps.Name = "textBox_captureFps";
            this.textBox_captureFps.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_captureFps_Validating);
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
            this.textBox_captureQuality.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_imageQuality_KeyPress);
            this.textBox_captureQuality.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_imageQuality_Validating);
            // 
            // comboBox_captureScale
            // 
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
            resources.ApplyResources(this.comboBox_captureScale, "comboBox_captureScale");
            this.comboBox_captureScale.Name = "comboBox_captureScale";
            this.comboBox_captureScale.TabStop = false;
            // 
            // label_divisionNumber
            // 
            resources.ApplyResources(this.label_divisionNumber, "label_divisionNumber");
            this.label_divisionNumber.Name = "label_divisionNumber";
            // 
            // comboBox_divisionNumber
            // 
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
            resources.ApplyResources(this.comboBox_divisionNumber, "comboBox_divisionNumber");
            this.comboBox_divisionNumber.Name = "comboBox_divisionNumber";
            this.comboBox_divisionNumber.TabStop = false;
            // 
            // groupBox_capture
            // 
            resources.ApplyResources(this.groupBox_capture, "groupBox_capture");
            this.groupBox_capture.Controls.Add(this.panel_captureDesktop);
            this.groupBox_capture.Controls.Add(this.radioButton_process);
            this.groupBox_capture.Controls.Add(this.radioButton_desktop);
            this.groupBox_capture.Controls.Add(this.comboBox_process);
            this.groupBox_capture.Name = "groupBox_capture";
            this.groupBox_capture.TabStop = false;
            // 
            // panel_captureDesktop
            // 
            resources.ApplyResources(this.panel_captureDesktop, "panel_captureDesktop");
            this.panel_captureDesktop.Controls.Add(this.button_areaReset);
            this.panel_captureDesktop.Controls.Add(this.checkBox_showOverRayForm);
            this.panel_captureDesktop.Controls.Add(this.label_targetDisplay);
            this.panel_captureDesktop.Controls.Add(this.button_area);
            this.panel_captureDesktop.Controls.Add(this.comboBox_targetDisplay);
            this.panel_captureDesktop.Cursor = System.Windows.Forms.Cursors.Default;
            this.panel_captureDesktop.Name = "panel_captureDesktop";
            // 
            // button_areaReset
            // 
            resources.ApplyResources(this.button_areaReset, "button_areaReset");
            this.button_areaReset.Name = "button_areaReset";
            this.button_areaReset.UseVisualStyleBackColor = true;
            this.button_areaReset.Click += new System.EventHandler(this.button_areaReset_Click);
            // 
            // checkBox_showOverRayForm
            // 
            resources.ApplyResources(this.checkBox_showOverRayForm, "checkBox_showOverRayForm");
            this.checkBox_showOverRayForm.Name = "checkBox_showOverRayForm";
            this.checkBox_showOverRayForm.UseVisualStyleBackColor = true;
            this.checkBox_showOverRayForm.CheckedChanged += new System.EventHandler(this.checkBox_showOverRayForm_CheckedChanged);
            // 
            // label_targetDisplay
            // 
            resources.ApplyResources(this.label_targetDisplay, "label_targetDisplay");
            this.label_targetDisplay.Name = "label_targetDisplay";
            // 
            // comboBox_targetDisplay
            // 
            this.comboBox_targetDisplay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_targetDisplay.FormattingEnabled = true;
            resources.ApplyResources(this.comboBox_targetDisplay, "comboBox_targetDisplay");
            this.comboBox_targetDisplay.Name = "comboBox_targetDisplay";
            this.comboBox_targetDisplay.SelectedIndexChanged += new System.EventHandler(this.comboBox_targetDisplay_SelectedIndexChanged);
            this.comboBox_targetDisplay.Click += new System.EventHandler(this.comboBox_targetDisplay_Click);
            // 
            // radioButton_desktop
            // 
            resources.ApplyResources(this.radioButton_desktop, "radioButton_desktop");
            this.radioButton_desktop.Checked = true;
            this.radioButton_desktop.Name = "radioButton_desktop";
            this.radioButton_desktop.TabStop = true;
            this.radioButton_desktop.UseVisualStyleBackColor = true;
            this.radioButton_desktop.CheckedChanged += new System.EventHandler(this.radioButton_area_CheckedChanged);
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
            // Form_ImageCast
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
            this.Name = "Form_ImageCast";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_tcp_FormClosed);
            this.panel_capture.ResumeLayout(false);
            this.panel_capture.PerformLayout();
            this.groupBox_recording.ResumeLayout(false);
            this.groupBox_recording.PerformLayout();
            this.groupBox_voice.ResumeLayout(false);
            this.groupBox_voice.PerformLayout();
            this.panel_voice.ResumeLayout(false);
            this.panel_voice.PerformLayout();
            this.groupBox_casting.ResumeLayout(false);
            this.groupBox_casting.PerformLayout();
            this.panel_manualCasting.ResumeLayout(false);
            this.panel_manualCasting.PerformLayout();
            this.groupBox_capture.ResumeLayout(false);
            this.groupBox_capture.PerformLayout();
            this.panel_captureDesktop.ResumeLayout(false);
            this.panel_captureDesktop.PerformLayout();
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
        private System.Windows.Forms.RadioButton radioButton_desktop;
        private System.Windows.Forms.Label label_captureQuality;
        private System.Windows.Forms.GroupBox groupBox_capture;
        private System.Windows.Forms.GroupBox groupBox_casting;
        private System.Windows.Forms.Button button_areaReset;
        private System.Windows.Forms.CheckBox checkBox_captureVoice;
        private System.Windows.Forms.GroupBox groupBox_voice;
        private System.Windows.Forms.ComboBox comboBox_audioQuality;
        private System.Windows.Forms.Label label_audioQuality;
        private System.Windows.Forms.CheckBox checkBox_stereo;
        private System.Windows.Forms.ComboBox comboBox_divisionNumber;
        private System.Windows.Forms.Label label_divisionNumber;
        private System.Windows.Forms.TextBox textBox_captureQuality;
        private System.Windows.Forms.ComboBox comboBox_captureScale;
        private System.Windows.Forms.Label label_captureScale;
        private System.Windows.Forms.CheckBox checkBox_showOverRayForm;
        private System.Windows.Forms.CheckBox checkBox_recordVideo;
        private System.Windows.Forms.TextBox textBox_captureFps;
        private System.Windows.Forms.GroupBox groupBox_recording;
        private System.Windows.Forms.CheckBox checkBox_recordAudio;
        private System.Windows.Forms.Label label_waveInDevices;
        private System.Windows.Forms.ComboBox comboBox_waveInDevices;
        private System.Windows.Forms.Label label_Client;
        private System.Windows.Forms.Label label_ConnectionNum;
        private System.Windows.Forms.ComboBox comboBox_targetDisplay;
        private System.Windows.Forms.Label label_targetDisplay;
        private System.Windows.Forms.Panel panel_captureDesktop;
        private System.Windows.Forms.Panel panel_voice;
        private System.Windows.Forms.Panel panel_manualCasting;
    }
}

