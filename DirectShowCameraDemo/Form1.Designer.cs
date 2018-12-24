namespace DirectShowCameraDemo
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.ImgSrcShow = new System.Windows.Forms.PictureBox();
            this.ImgCbShow = new System.Windows.Forms.PictureBox();
            this.CtrlCamera = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ImgSrcShow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgCbShow)).BeginInit();
            this.SuspendLayout();
            // 
            // ImgSrcShow
            // 
            this.ImgSrcShow.Location = new System.Drawing.Point(12, 12);
            this.ImgSrcShow.Name = "ImgSrcShow";
            this.ImgSrcShow.Size = new System.Drawing.Size(449, 364);
            this.ImgSrcShow.TabIndex = 0;
            this.ImgSrcShow.TabStop = false;
            // 
            // ImgCbShow
            // 
            this.ImgCbShow.Location = new System.Drawing.Point(477, 12);
            this.ImgCbShow.Name = "ImgCbShow";
            this.ImgCbShow.Size = new System.Drawing.Size(449, 364);
            this.ImgCbShow.TabIndex = 1;
            this.ImgCbShow.TabStop = false;
            // 
            // CtrlCamera
            // 
            this.CtrlCamera.Location = new System.Drawing.Point(408, 401);
            this.CtrlCamera.Name = "CtrlCamera";
            this.CtrlCamera.Size = new System.Drawing.Size(120, 38);
            this.CtrlCamera.TabIndex = 2;
            this.CtrlCamera.Text = "StartCamera";
            this.CtrlCamera.UseVisualStyleBackColor = true;
            this.CtrlCamera.Click += new System.EventHandler(this.CtrlCamera_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(939, 461);
            this.Controls.Add(this.CtrlCamera);
            this.Controls.Add(this.ImgCbShow);
            this.Controls.Add(this.ImgSrcShow);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.ImgSrcShow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgCbShow)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox ImgSrcShow;
        private System.Windows.Forms.PictureBox ImgCbShow;
        private System.Windows.Forms.Button CtrlCamera;
    }
}

