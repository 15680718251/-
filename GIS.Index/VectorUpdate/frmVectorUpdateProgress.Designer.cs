namespace GIS.TreeIndex.VectorUpdate
{
    partial class frmVectorUpdateProgress
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
            this.lblDescription = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnDetail = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.text1 = new System.Windows.Forms.Label();
            this.text2 = new System.Windows.Forms.Label();
            this.incrNumber = new System.Windows.Forms.Label();
            this.procNumber = new System.Windows.Forms.Label();
            this.text3 = new System.Windows.Forms.Label();
            this.srcNumber = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(14, 15);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(331, 23);
            this.lblDescription.TabIndex = 0;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(16, 58);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(430, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // btnDetail
            // 
            this.btnDetail.Location = new System.Drawing.Point(371, 15);
            this.btnDetail.Name = "btnDetail";
            this.btnDetail.Size = new System.Drawing.Size(66, 23);
            this.btnDetail.TabIndex = 3;
            this.btnDetail.Text = "<< 简要";
            this.btnDetail.UseVisualStyleBackColor = true;
            this.btnDetail.Click += new System.EventHandler(this.btnDetail_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(16, 105);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(156, 16);
            this.checkBox1.TabIndex = 4;
            this.checkBox1.Text = "处理完后自动关闭本窗体";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(16, 190);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(448, 97);
            this.txtDescription.TabIndex = 5;
            this.txtDescription.TextChanged += new System.EventHandler(this.txtDescription_TextChanged);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(371, 105);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(66, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Visible = false;
            this.btnOK.Click += new System.EventHandler(this.button1_Click);
            // 
            // text1
            // 
            this.text1.AutoSize = true;
            this.text1.Location = new System.Drawing.Point(14, 139);
            this.text1.Name = "text1";
            this.text1.Size = new System.Drawing.Size(173, 12);
            this.text1.TabIndex = 7;
            this.text1.Text = "增量多边形共有：          个";
            // 
            // text2
            // 
            this.text2.AutoSize = true;
            this.text2.Location = new System.Drawing.Point(240, 139);
            this.text2.Name = "text2";
            this.text2.Size = new System.Drawing.Size(155, 12);
            this.text2.TabIndex = 8;
            this.text2.Text = "正在处理第             个";
            // 
            // incrNumber
            // 
            this.incrNumber.AutoSize = true;
            this.incrNumber.Location = new System.Drawing.Point(127, 139);
            this.incrNumber.Name = "incrNumber";
            this.incrNumber.Size = new System.Drawing.Size(11, 12);
            this.incrNumber.TabIndex = 9;
            this.incrNumber.Text = "0";
            // 
            // procNumber
            // 
            this.procNumber.AutoSize = true;
            this.procNumber.Location = new System.Drawing.Point(322, 139);
            this.procNumber.Name = "procNumber";
            this.procNumber.Size = new System.Drawing.Size(11, 12);
            this.procNumber.TabIndex = 10;
            this.procNumber.Text = "0";
            // 
            // text3
            // 
            this.text3.AutoSize = true;
            this.text3.Location = new System.Drawing.Point(14, 163);
            this.text3.Name = "text3";
            this.text3.Size = new System.Drawing.Size(197, 12);
            this.text3.TabIndex = 11;
            this.text3.Text = "当前底图共有            个多边形";
            // 
            // srcNumber
            // 
            this.srcNumber.AutoSize = true;
            this.srcNumber.Location = new System.Drawing.Point(105, 163);
            this.srcNumber.Name = "srcNumber";
            this.srcNumber.Size = new System.Drawing.Size(11, 12);
            this.srcNumber.TabIndex = 12;
            this.srcNumber.Text = "0";
            // 
            // frmVectorUpdateProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 299);
            this.ControlBox = false;
            this.Controls.Add(this.srcNumber);
            this.Controls.Add(this.text3);
            this.Controls.Add(this.procNumber);
            this.Controls.Add(this.incrNumber);
            this.Controls.Add(this.text2);
            this.Controls.Add(this.text1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.btnDetail);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lblDescription);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmVectorUpdateProgress";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "正在执行";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmVectorizeProgress_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label lblDescription;
        public System.Windows.Forms.ProgressBar progressBar1;
        public System.Windows.Forms.TextBox txtDescription;
        public System.Windows.Forms.CheckBox checkBox1;

        private System.Windows.Forms.Button btnDetail;
        public System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label text1;
        private System.Windows.Forms.Label text2;
        public System.Windows.Forms.Label incrNumber;
        public System.Windows.Forms.Label procNumber;
        private System.Windows.Forms.Label text3;
        public System.Windows.Forms.Label srcNumber;
    }
}