namespace GIS.UI.Forms
{
    partial class SHPToDataBase
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
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.tbShow = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rb_poly = new System.Windows.Forms.RadioButton();
            this.rb_line = new System.Windows.Forms.RadioButton();
            this.rb_point = new System.Windows.Forms.RadioButton();
            this.tb_name = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rb_jitai = new System.Windows.Forms.RadioButton();
            this.rb_zengliang = new System.Windows.Forms.RadioButton();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(344, 35);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "浏览";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(92, 37);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(231, 21);
            this.textBox1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "选择shp数据：";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(23, 483);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "导入";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(297, 483);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "取消";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "入库表名：";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(14, 463);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(405, 14);
            this.progressBar1.TabIndex = 8;
            // 
            // tbShow
            // 
            this.tbShow.Location = new System.Drawing.Point(14, 236);
            this.tbShow.Multiline = true;
            this.tbShow.Name = "tbShow";
            this.tbShow.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbShow.Size = new System.Drawing.Size(423, 221);
            this.tbShow.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 197);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 13;
            this.label3.Text = "拓扑类型：";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rb_poly);
            this.panel2.Controls.Add(this.rb_line);
            this.panel2.Controls.Add(this.rb_point);
            this.panel2.Location = new System.Drawing.Point(102, 186);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(285, 37);
            this.panel2.TabIndex = 15;
            // 
            // rb_poly
            // 
            this.rb_poly.AutoSize = true;
            this.rb_poly.Location = new System.Drawing.Point(192, 10);
            this.rb_poly.Name = "rb_poly";
            this.rb_poly.Size = new System.Drawing.Size(35, 16);
            this.rb_poly.TabIndex = 2;
            this.rb_poly.TabStop = true;
            this.rb_poly.Text = "面";
            this.rb_poly.UseVisualStyleBackColor = true;
            // 
            // rb_line
            // 
            this.rb_line.AutoSize = true;
            this.rb_line.Checked = true;
            this.rb_line.Location = new System.Drawing.Point(108, 11);
            this.rb_line.Name = "rb_line";
            this.rb_line.Size = new System.Drawing.Size(35, 16);
            this.rb_line.TabIndex = 1;
            this.rb_line.TabStop = true;
            this.rb_line.Text = "线";
            this.rb_line.UseVisualStyleBackColor = true;
            // 
            // rb_point
            // 
            this.rb_point.AutoSize = true;
            this.rb_point.Location = new System.Drawing.Point(22, 11);
            this.rb_point.Name = "rb_point";
            this.rb_point.Size = new System.Drawing.Size(35, 16);
            this.rb_point.TabIndex = 0;
            this.rb_point.TabStop = true;
            this.rb_point.Text = "点";
            this.rb_point.UseVisualStyleBackColor = true;
            // 
            // tb_name
            // 
            this.tb_name.Location = new System.Drawing.Point(92, 92);
            this.tb_name.Name = "tb_name";
            this.tb_name.Size = new System.Drawing.Size(231, 21);
            this.tb_name.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 147);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 17;
            this.label4.Text = "数据类型：";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rb_zengliang);
            this.panel1.Controls.Add(this.rb_jitai);
            this.panel1.Location = new System.Drawing.Point(102, 132);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(285, 39);
            this.panel1.TabIndex = 18;
            // 
            // rb_jitai
            // 
            this.rb_jitai.AutoSize = true;
            this.rb_jitai.Checked = true;
            this.rb_jitai.Location = new System.Drawing.Point(22, 12);
            this.rb_jitai.Name = "rb_jitai";
            this.rb_jitai.Size = new System.Drawing.Size(71, 16);
            this.rb_jitai.TabIndex = 0;
            this.rb_jitai.TabStop = true;
            this.rb_jitai.Text = "基态数据";
            this.rb_jitai.UseVisualStyleBackColor = true;
            // 
            // rb_zengliang
            // 
            this.rb_zengliang.AutoSize = true;
            this.rb_zengliang.Location = new System.Drawing.Point(143, 12);
            this.rb_zengliang.Name = "rb_zengliang";
            this.rb_zengliang.Size = new System.Drawing.Size(71, 16);
            this.rb_zengliang.TabIndex = 1;
            this.rb_zengliang.TabStop = true;
            this.rb_zengliang.Text = "增量数据";
            this.rb_zengliang.UseVisualStyleBackColor = true;
            // 
            // SHPToDataBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(462, 511);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_name);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbShow);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "SHPToDataBase";
            this.Text = "矢量数据入库";
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox tbShow;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rb_poly;
        private System.Windows.Forms.RadioButton rb_line;
        private System.Windows.Forms.RadioButton rb_point;
        private System.Windows.Forms.TextBox tb_name;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rb_zengliang;
        private System.Windows.Forms.RadioButton rb_jitai;
    }
}