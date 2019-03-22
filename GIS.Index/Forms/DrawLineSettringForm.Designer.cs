namespace GIS.TreeIndex.Forms
{
    partial class DrawLineSettringForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.numericUpDownSnapAngle = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownAngleToler = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownVertexToler = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxSnap = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSnapAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAngleToler)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVertexToler)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "线捕捉角度：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "角度捕捉容差：";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(26, 192);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "确认";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(154, 192);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // numericUpDownSnapAngle
            // 
            this.numericUpDownSnapAngle.Location = new System.Drawing.Point(154, 65);
            this.numericUpDownSnapAngle.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDownSnapAngle.Name = "numericUpDownSnapAngle";
            this.numericUpDownSnapAngle.Size = new System.Drawing.Size(100, 21);
            this.numericUpDownSnapAngle.TabIndex = 6;
            // 
            // numericUpDownAngleToler
            // 
            this.numericUpDownAngleToler.Location = new System.Drawing.Point(154, 112);
            this.numericUpDownAngleToler.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownAngleToler.Name = "numericUpDownAngleToler";
            this.numericUpDownAngleToler.Size = new System.Drawing.Size(100, 21);
            this.numericUpDownAngleToler.TabIndex = 7;
            // 
            // numericUpDownVertexToler
            // 
            this.numericUpDownVertexToler.Location = new System.Drawing.Point(154, 152);
            this.numericUpDownVertexToler.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownVertexToler.Name = "numericUpDownVertexToler";
            this.numericUpDownVertexToler.Size = new System.Drawing.Size(100, 21);
            this.numericUpDownVertexToler.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 154);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "节点捕捉容差：";
            // 
            // checkBoxSnap
            // 
            this.checkBoxSnap.AutoSize = true;
            this.checkBoxSnap.Location = new System.Drawing.Point(154, 24);
            this.checkBoxSnap.Name = "checkBoxSnap";
            this.checkBoxSnap.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSnap.TabIndex = 10;
            this.checkBoxSnap.Text = "开启";
            this.checkBoxSnap.UseVisualStyleBackColor = true;
            this.checkBoxSnap.CheckedChanged += new System.EventHandler(this.checkBoxSnap_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "角度捕捉：";
            // 
            // DrawLineSettringForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 241);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkBoxSnap);
            this.Controls.Add(this.numericUpDownVertexToler);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDownAngleToler);
            this.Controls.Add(this.numericUpDownSnapAngle);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "DrawLineSettringForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "画线参数设置：";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSnapAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAngleToler)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVertexToler)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.NumericUpDown numericUpDownSnapAngle;
        private System.Windows.Forms.NumericUpDown numericUpDownAngleToler;
        private System.Windows.Forms.NumericUpDown numericUpDownVertexToler;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxSnap;
        private System.Windows.Forms.Label label4;
    }
}