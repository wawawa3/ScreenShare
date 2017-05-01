namespace ScreenShare
{
    partial class Form_CaptureArea
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form_CaptureArea
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(10, 10);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_CaptureArea";
            this.Opacity = 0.5D;
            this.ShowInTaskbar = false;
            this.Text = "Form_CaptureArea";
            this.Activated += new System.EventHandler(this.Form_CaptureArea_Activated);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form_CaptureArea_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form_CaptureArea_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form_CaptureArea_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form_CaptureArea_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}