namespace GIS.UI.Forms
{
    partial class CombineOSC
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.oscPgBar = new System.Windows.Forms.ProgressBar();
            this.label4 = new System.Windows.Forms.Label();
            this.polygenClearNum = new System.Windows.Forms.Label();
            this.lineClearNum = new System.Windows.Forms.Label();
            this.pointClearNum = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(32, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(168, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "面增量数据清理条数：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(32, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(168, 16);
            this.label2.TabIndex = 0;
            this.label2.Text = "线增量数据清理条数：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(32, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(168, 16);
            this.label3.TabIndex = 0;
            this.label3.Text = "点增量数据清理条数：";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(131, 213);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "开始整合";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // oscPgBar
            // 
            this.oscPgBar.Location = new System.Drawing.Point(-4, 252);
            this.oscPgBar.Name = "oscPgBar";
            this.oscPgBar.Size = new System.Drawing.Size(456, 29);
            this.oscPgBar.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(206, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 16);
            this.label4.TabIndex = 0;
            // 
            // polygenClearNum
            // 
            this.polygenClearNum.AutoSize = true;
            this.polygenClearNum.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.polygenClearNum.Location = new System.Drawing.Point(204, 45);
            this.polygenClearNum.Name = "polygenClearNum";
            this.polygenClearNum.Size = new System.Drawing.Size(16, 16);
            this.polygenClearNum.TabIndex = 0;
            this.polygenClearNum.Text = "0";
            // 
            // lineClearNum
            // 
            this.lineClearNum.AutoSize = true;
            this.lineClearNum.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lineClearNum.Location = new System.Drawing.Point(204, 88);
            this.lineClearNum.Name = "lineClearNum";
            this.lineClearNum.Size = new System.Drawing.Size(16, 16);
            this.lineClearNum.TabIndex = 0;
            this.lineClearNum.Text = "0";
            // 
            // pointClearNum
            // 
            this.pointClearNum.AutoSize = true;
            this.pointClearNum.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pointClearNum.Location = new System.Drawing.Point(204, 130);
            this.pointClearNum.Name = "pointClearNum";
            this.pointClearNum.Size = new System.Drawing.Size(16, 16);
            this.pointClearNum.TabIndex = 0;
            this.pointClearNum.Text = "0";
            // 
            // CombineOSC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(452, 282);
            this.Controls.Add(this.oscPgBar);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pointClearNum);
            this.Controls.Add(this.lineClearNum);
            this.Controls.Add(this.polygenClearNum);
            this.Controls.Add(this.label1);
            this.Name = "CombineOSC";
            this.Text = "CombineOSC";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ProgressBar oscPgBar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label polygenClearNum;
        private System.Windows.Forms.Label lineClearNum;
        private System.Windows.Forms.Label pointClearNum;

    }
}