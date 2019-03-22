namespace GIS.TreeIndex.Forms
{
    partial class SysParameterForm
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
            this.numericUpCheckedDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxShowOnlineNotes = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxSnapCenter = new System.Windows.Forms.CheckBox();
            this.checkBoxSnapVertex = new System.Windows.Forms.CheckBox();
            this.checkBoxShowSurveyPt = new System.Windows.Forms.CheckBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.numericUpDownMouseCatchToler = new System.Windows.Forms.NumericUpDown();
            this.trackBarRenderMode = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.大比例尺 = new System.Windows.Forms.RadioButton();
            this.中比例尺 = new System.Windows.Forms.RadioButton();
            this.小比例尺 = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.setcharmap = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpCheckedDown1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMouseCatchToler)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRenderMode)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // numericUpCheckedDown1
            // 
            this.numericUpCheckedDown1.Location = new System.Drawing.Point(123, 27);
            this.numericUpCheckedDown1.Name = "numericUpCheckedDown1";
            this.numericUpCheckedDown1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.numericUpCheckedDown1.Size = new System.Drawing.Size(120, 21);
            this.numericUpCheckedDown1.TabIndex = 0;
            this.numericUpCheckedDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 29);
            this.label1.Name = "label1";
            this.label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "自动存盘间隔：";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.button1.Location = new System.Drawing.Point(125, 63);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(52, 23);
            this.button1.TabIndex = 2;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 68);
            this.label2.Name = "label2";
            this.label2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "背景颜色：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 108);
            this.label3.Name = "label3";
            this.label3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "节点捕捉容差：";
            // 
            // checkBoxShowOnlineNotes
            // 
            this.checkBoxShowOnlineNotes.AutoSize = true;
            this.checkBoxShowOnlineNotes.Location = new System.Drawing.Point(231, 213);
            this.checkBoxShowOnlineNotes.Name = "checkBoxShowOnlineNotes";
            this.checkBoxShowOnlineNotes.Size = new System.Drawing.Size(72, 16);
            this.checkBoxShowOnlineNotes.TabIndex = 6;
            this.checkBoxShowOnlineNotes.Text = "在线提示";
            this.checkBoxShowOnlineNotes.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxSnapCenter);
            this.groupBox1.Controls.Add(this.checkBoxSnapVertex);
            this.groupBox1.Location = new System.Drawing.Point(231, 255);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(86, 91);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "捕捉类型";
            // 
            // checkBoxSnapCenter
            // 
            this.checkBoxSnapCenter.AutoSize = true;
            this.checkBoxSnapCenter.Location = new System.Drawing.Point(6, 61);
            this.checkBoxSnapCenter.Name = "checkBoxSnapCenter";
            this.checkBoxSnapCenter.Size = new System.Drawing.Size(72, 16);
            this.checkBoxSnapCenter.TabIndex = 9;
            this.checkBoxSnapCenter.Text = "中点捕捉";
            this.checkBoxSnapCenter.UseVisualStyleBackColor = true;
            // 
            // checkBoxSnapVertex
            // 
            this.checkBoxSnapVertex.AutoSize = true;
            this.checkBoxSnapVertex.Location = new System.Drawing.Point(6, 20);
            this.checkBoxSnapVertex.Name = "checkBoxSnapVertex";
            this.checkBoxSnapVertex.Size = new System.Drawing.Size(72, 16);
            this.checkBoxSnapVertex.TabIndex = 8;
            this.checkBoxSnapVertex.Text = "端点捕捉";
            this.checkBoxSnapVertex.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowSurveyPt
            // 
            this.checkBoxShowSurveyPt.AutoSize = true;
            this.checkBoxShowSurveyPt.Location = new System.Drawing.Point(231, 179);
            this.checkBoxShowSurveyPt.Name = "checkBoxShowSurveyPt";
            this.checkBoxShowSurveyPt.Size = new System.Drawing.Size(84, 16);
            this.checkBoxShowSurveyPt.TabIndex = 8;
            this.checkBoxShowSurveyPt.Text = "观测值显示";
            this.checkBoxShowSurveyPt.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(64, 379);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 9;
            this.buttonOk.Text = "确定";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // button3
            // 
            this.button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button3.Location = new System.Drawing.Point(214, 379);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 10;
            this.button3.Text = "取消";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMouseCatchToler
            // 
            this.numericUpDownMouseCatchToler.Location = new System.Drawing.Point(125, 99);
            this.numericUpDownMouseCatchToler.Name = "numericUpDownMouseCatchToler";
            this.numericUpDownMouseCatchToler.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.numericUpDownMouseCatchToler.Size = new System.Drawing.Size(120, 21);
            this.numericUpDownMouseCatchToler.TabIndex = 11;
            this.numericUpDownMouseCatchToler.ValueChanged += new System.EventHandler(this.numericUpDownMouseCatchToler_ValueChanged);
            // 
            // trackBarRenderMode
            // 
            this.trackBarRenderMode.Location = new System.Drawing.Point(4, 34);
            this.trackBarRenderMode.Maximum = 3;
            this.trackBarRenderMode.Minimum = 1;
            this.trackBarRenderMode.Name = "trackBarRenderMode";
            this.trackBarRenderMode.Size = new System.Drawing.Size(137, 45);
            this.trackBarRenderMode.TabIndex = 12;
            this.trackBarRenderMode.Value = 3;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.trackBarRenderMode);
            this.groupBox2.Location = new System.Drawing.Point(6, 166);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(184, 73);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "绘图质量：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(58, 19);
            this.label6.Name = "label6";
            this.label6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "平衡";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(106, 19);
            this.label5.Name = "label5";
            this.label5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 14;
            this.label5.Text = "质量";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 17);
            this.label4.Name = "label4";
            this.label4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 14;
            this.label4.Text = "速度";
            // 
            // 大比例尺
            // 
            this.大比例尺.AutoSize = true;
            this.大比例尺.Location = new System.Drawing.Point(2, 20);
            this.大比例尺.Name = "大比例尺";
            this.大比例尺.Size = new System.Drawing.Size(143, 16);
            this.大比例尺.TabIndex = 14;
            this.大比例尺.TabStop = true;
            this.大比例尺.Text = "1:500,1:1000, 1:2000";
            this.大比例尺.UseVisualStyleBackColor = true;
            // 
            // 中比例尺
            // 
            this.中比例尺.AutoSize = true;
            this.中比例尺.Location = new System.Drawing.Point(1, 54);
            this.中比例尺.Name = "中比例尺";
            this.中比例尺.Size = new System.Drawing.Size(107, 16);
            this.中比例尺.TabIndex = 15;
            this.中比例尺.TabStop = true;
            this.中比例尺.Text = "1:5000,1:10000";
            this.中比例尺.UseVisualStyleBackColor = true;
            // 
            // 小比例尺
            // 
            this.小比例尺.AutoSize = true;
            this.小比例尺.Location = new System.Drawing.Point(2, 86);
            this.小比例尺.Name = "小比例尺";
            this.小比例尺.Size = new System.Drawing.Size(167, 16);
            this.小比例尺.TabIndex = 16;
            this.小比例尺.TabStop = true;
            this.小比例尺.Text = "1:25000,1:50000,1:100000";
            this.小比例尺.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.小比例尺);
            this.groupBox3.Controls.Add(this.大比例尺);
            this.groupBox3.Controls.Add(this.中比例尺);
            this.groupBox3.Location = new System.Drawing.Point(8, 255);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(217, 102);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "当前比例尺";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 141);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 12);
            this.label7.TabIndex = 18;
            this.label7.Text = "符号映射表:";
            // 
            // setcharmap
            // 
            this.setcharmap.Location = new System.Drawing.Point(125, 137);
            this.setcharmap.Name = "setcharmap";
            this.setcharmap.Size = new System.Drawing.Size(38, 20);
            this.setcharmap.TabIndex = 19;
            this.setcharmap.Text = "...";
            this.setcharmap.UseVisualStyleBackColor = true;
            this.setcharmap.Click += new System.EventHandler(this.setcharmap_Click);
            // 
            // SysParameterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 419);
            this.Controls.Add(this.setcharmap);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.numericUpDownMouseCatchToler);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.checkBoxShowSurveyPt);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBoxShowOnlineNotes);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpCheckedDown1);
            this.Name = "SysParameterForm";
            this.Text = "系统参数：";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpCheckedDown1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMouseCatchToler)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRenderMode)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numericUpCheckedDown1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxShowOnlineNotes;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxSnapCenter;
        private System.Windows.Forms.CheckBox checkBoxSnapVertex;
        private System.Windows.Forms.CheckBox checkBoxShowSurveyPt;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.NumericUpDown numericUpDownMouseCatchToler;
        private System.Windows.Forms.TrackBar trackBarRenderMode;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton 大比例尺;
        private System.Windows.Forms.RadioButton 中比例尺;
        private System.Windows.Forms.RadioButton 小比例尺;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button setcharmap;

    }
}