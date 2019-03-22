namespace GIS.TreeIndex.Index
{
    partial class FrmCreateIndex
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
            this.IDOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.IdxHeight = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.IdxNdNum = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.NdAvgNum = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.CrtIdxTime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // IDOK
            // 
            this.IDOK.Location = new System.Drawing.Point(278, 168);
            this.IDOK.Name = "IDOK";
            this.IDOK.Size = new System.Drawing.Size(75, 23);
            this.IDOK.TabIndex = 0;
            this.IDOK.Text = "确定";
            this.IDOK.UseVisualStyleBackColor = true;
            this.IDOK.Click += new System.EventHandler(this.IDOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15F);
            this.label1.Location = new System.Drawing.Point(108, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "索引建立完成";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "索引层深：";
            // 
            // IdxHeight
            // 
            this.IdxHeight.AutoSize = true;
            this.IdxHeight.Location = new System.Drawing.Point(125, 52);
            this.IdxHeight.Name = "IdxHeight";
            this.IdxHeight.Size = new System.Drawing.Size(11, 12);
            this.IdxHeight.TabIndex = 3;
            this.IdxHeight.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(49, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "索引树结点数：";
            // 
            // IdxNdNum
            // 
            this.IdxNdNum.AutoSize = true;
            this.IdxNdNum.Location = new System.Drawing.Point(147, 84);
            this.IdxNdNum.Name = "IdxNdNum";
            this.IdxNdNum.Size = new System.Drawing.Size(11, 12);
            this.IdxNdNum.TabIndex = 5;
            this.IdxNdNum.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(49, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(173, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "每个结点上平均多边形对象数：";
            // 
            // NdAvgNum
            // 
            this.NdAvgNum.AutoSize = true;
            this.NdAvgNum.Location = new System.Drawing.Point(233, 116);
            this.NdAvgNum.Name = "NdAvgNum";
            this.NdAvgNum.Size = new System.Drawing.Size(11, 12);
            this.NdAvgNum.TabIndex = 7;
            this.NdAvgNum.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(49, 148);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "建立索引时间：";
            // 
            // CrtIdxTime
            // 
            this.CrtIdxTime.AutoSize = true;
            this.CrtIdxTime.Location = new System.Drawing.Point(151, 148);
            this.CrtIdxTime.Name = "CrtIdxTime";
            this.CrtIdxTime.Size = new System.Drawing.Size(11, 12);
            this.CrtIdxTime.TabIndex = 9;
            this.CrtIdxTime.Text = "0";
            // 
            // FrmCreateIndex
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(387, 214);
            this.Controls.Add(this.CrtIdxTime);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.NdAvgNum);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.IdxNdNum);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.IdxHeight);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.IDOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmCreateIndex";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "建立索引";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button IDOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label IdxHeight;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label IdxNdNum;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label NdAvgNum;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label CrtIdxTime;
    }
}