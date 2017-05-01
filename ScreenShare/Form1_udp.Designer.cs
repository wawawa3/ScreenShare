namespace ScreenCapture
{
    partial class Form1_udp
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
            this.button_startCapture = new System.Windows.Forms.Button();
            this.button_stopCapture = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.textBox_ip = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_connect = new System.Windows.Forms.Button();
            this.textBox_port = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // button_startCapture
            // 
            this.button_startCapture.Enabled = false;
            this.button_startCapture.Location = new System.Drawing.Point(207, 41);
            this.button_startCapture.Name = "button_startCapture";
            this.button_startCapture.Size = new System.Drawing.Size(106, 23);
            this.button_startCapture.TabIndex = 0;
            this.button_startCapture.Text = "キャプチャ開始";
            this.button_startCapture.UseVisualStyleBackColor = true;
            this.button_startCapture.Click += new System.EventHandler(this.button_startCapture_Click);
            // 
            // button_stopCapture
            // 
            this.button_stopCapture.Enabled = false;
            this.button_stopCapture.Location = new System.Drawing.Point(318, 41);
            this.button_stopCapture.Name = "button_stopCapture";
            this.button_stopCapture.Size = new System.Drawing.Size(100, 23);
            this.button_stopCapture.TabIndex = 1;
            this.button_stopCapture.Text = "キャプチャ終了";
            this.button_stopCapture.UseVisualStyleBackColor = true;
            this.button_stopCapture.Click += new System.EventHandler(this.button_stopCapture_Click);
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(13, 77);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(405, 180);
            this.pictureBox.TabIndex = 2;
            this.pictureBox.TabStop = false;
            // 
            // textBox_ip
            // 
            this.textBox_ip.Location = new System.Drawing.Point(69, 14);
            this.textBox_ip.Name = "textBox_ip";
            this.textBox_ip.Size = new System.Drawing.Size(80, 19);
            this.textBox_ip.TabIndex = 3;
            this.textBox_ip.Text = "127.0.0.1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "接続先IP";
            // 
            // button_connect
            // 
            this.button_connect.Location = new System.Drawing.Point(319, 12);
            this.button_connect.Name = "button_connect";
            this.button_connect.Size = new System.Drawing.Size(99, 23);
            this.button_connect.TabIndex = 5;
            this.button_connect.Text = "接続";
            this.button_connect.UseVisualStyleBackColor = true;
            this.button_connect.Click += new System.EventHandler(this.button_connect_Click);
            // 
            // textBox_port
            // 
            this.textBox_port.Location = new System.Drawing.Point(207, 14);
            this.textBox_port.Name = "textBox_port";
            this.textBox_port.Size = new System.Drawing.Size(106, 19);
            this.textBox_port.TabIndex = 6;
            this.textBox_port.Text = "33333";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(168, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "ポート";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 269);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_port);
            this.Controls.Add(this.button_connect);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_ip);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.button_stopCapture);
            this.Controls.Add(this.button_startCapture);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_startCapture;
        private System.Windows.Forms.Button button_stopCapture;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TextBox textBox_ip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_connect;
        private System.Windows.Forms.TextBox textBox_port;
        private System.Windows.Forms.Label label2;
    }
}

