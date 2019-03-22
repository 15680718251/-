namespace FeatureMatchUpdate.Forms
{
    partial class LineFeatureExtraction
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LineFeatureExtraction));
            this.pgbarlabel = new System.Windows.Forms.Label();
            this.LineExtractionpgBar = new System.Windows.Forms.ProgressBar();
            this.ExtractionAllLinegBox = new System.Windows.Forms.GroupBox();
            this.IncreDataExtractionLineBtn = new System.Windows.Forms.Button();
            this.CurDataExtractionLineBtn = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.ExtractionAllLinegBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // pgbarlabel
            // 
            this.pgbarlabel.AutoSize = true;
            this.pgbarlabel.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pgbarlabel.Location = new System.Drawing.Point(415, 144);
            this.pgbarlabel.Name = "pgbarlabel";
            this.pgbarlabel.Size = new System.Drawing.Size(40, 16);
            this.pgbarlabel.TabIndex = 5;
            this.pgbarlabel.Text = "100%";
            // 
            // LineExtractionpgBar
            // 
            this.LineExtractionpgBar.Location = new System.Drawing.Point(6, 141);
            this.LineExtractionpgBar.Name = "LineExtractionpgBar";
            this.LineExtractionpgBar.Size = new System.Drawing.Size(403, 19);
            this.LineExtractionpgBar.TabIndex = 4;
            // 
            // ExtractionAllLinegBox
            // 
            this.ExtractionAllLinegBox.Controls.Add(this.IncreDataExtractionLineBtn);
            this.ExtractionAllLinegBox.Controls.Add(this.pgbarlabel);
            this.ExtractionAllLinegBox.Controls.Add(this.CurDataExtractionLineBtn);
            this.ExtractionAllLinegBox.Controls.Add(this.LineExtractionpgBar);
            this.ExtractionAllLinegBox.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ExtractionAllLinegBox.Location = new System.Drawing.Point(12, 23);
            this.ExtractionAllLinegBox.Name = "ExtractionAllLinegBox";
            this.ExtractionAllLinegBox.Size = new System.Drawing.Size(458, 176);
            this.ExtractionAllLinegBox.TabIndex = 13;
            this.ExtractionAllLinegBox.TabStop = false;
            this.ExtractionAllLinegBox.Text = "对所有典型线要素图层处理";
            // 
            // IncreDataExtractionLineBtn
            // 
            this.IncreDataExtractionLineBtn.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.IncreDataExtractionLineBtn.Location = new System.Drawing.Point(248, 55);
            this.IncreDataExtractionLineBtn.Name = "IncreDataExtractionLineBtn";
            this.IncreDataExtractionLineBtn.Size = new System.Drawing.Size(179, 52);
            this.IncreDataExtractionLineBtn.TabIndex = 1;
            this.IncreDataExtractionLineBtn.Text = "增量线要素图层预处理";
            this.IncreDataExtractionLineBtn.UseVisualStyleBackColor = true;
            this.IncreDataExtractionLineBtn.Click += new System.EventHandler(this.IncreDataExtractionLineBtn_Click);
            // 
            // CurDataExtractionLineBtn
            // 
            this.CurDataExtractionLineBtn.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CurDataExtractionLineBtn.Location = new System.Drawing.Point(24, 55);
            this.CurDataExtractionLineBtn.Name = "CurDataExtractionLineBtn";
            this.CurDataExtractionLineBtn.Size = new System.Drawing.Size(182, 52);
            this.CurDataExtractionLineBtn.TabIndex = 0;
            this.CurDataExtractionLineBtn.Text = "基态线要素图层预处理";
            this.CurDataExtractionLineBtn.UseVisualStyleBackColor = true;
            this.CurDataExtractionLineBtn.Click += new System.EventHandler(this.CurDataExtractionLineBtn_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.textBox1.Location = new System.Drawing.Point(12, 205);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(458, 74);
            this.textBox1.TabIndex = 14;
            this.textBox1.Text = "说明：\r\n\r\n    请分别点击基态和增量线要素预处理功能按钮进行线要素更新前的预处理，\r\n\r\n          该功能用于处理全球典型要素中的所有线要素图层数" +
                "据。\r\n";
            // 
            // LineFeatureExtraction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(482, 288);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.ExtractionAllLinegBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LineFeatureExtraction";
            this.Text = "基态和增量的线要素预处理";
            this.ExtractionAllLinegBox.ResumeLayout(false);
            this.ExtractionAllLinegBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label pgbarlabel;
        private System.Windows.Forms.ProgressBar LineExtractionpgBar;
        private System.Windows.Forms.GroupBox ExtractionAllLinegBox;
        private System.Windows.Forms.Button IncreDataExtractionLineBtn;
        private System.Windows.Forms.Button CurDataExtractionLineBtn;
        private System.Windows.Forms.TextBox textBox1;

    }
}