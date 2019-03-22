namespace GIS.TreeIndex.Forms
{
    partial class SaveCurPtSymForm
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
            this.name = new System.Windows.Forms.TextBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.id = new System.Windows.Forms.NumericUpDown();
            this.checkBox_ptsym = new System.Windows.Forms.CheckBox();
            this.checkBox_ptele = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.id)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // name
            // 
            this.name.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.name.Location = new System.Drawing.Point(62, 42);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(97, 21);
            this.name.TabIndex = 2;
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(63, 190);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(83, 30);
            this.button_ok.TabIndex = 4;
            this.button_ok.Text = "确定";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(191, 190);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(86, 30);
            this.button_cancel.TabIndex = 5;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(64, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "名字";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(189, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "ID";
            // 
            // id
            // 
            this.id.Location = new System.Drawing.Point(191, 42);
            this.id.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.id.Name = "id";
            this.id.Size = new System.Drawing.Size(79, 21);
            this.id.TabIndex = 8;
            // 
            // checkBox_ptsym
            // 
            this.checkBox_ptsym.AutoSize = true;
            this.checkBox_ptsym.Location = new System.Drawing.Point(126, 22);
            this.checkBox_ptsym.Name = "checkBox_ptsym";
            this.checkBox_ptsym.Size = new System.Drawing.Size(60, 16);
            this.checkBox_ptsym.TabIndex = 9;
            this.checkBox_ptsym.Text = "点符号";
            this.checkBox_ptsym.UseVisualStyleBackColor = true;
            // 
            // checkBox_ptele
            // 
            this.checkBox_ptele.AutoSize = true;
            this.checkBox_ptele.Location = new System.Drawing.Point(13, 22);
            this.checkBox_ptele.Name = "checkBox_ptele";
            this.checkBox_ptele.Size = new System.Drawing.Size(60, 16);
            this.checkBox_ptele.TabIndex = 10;
            this.checkBox_ptele.Text = "点图元";
            this.checkBox_ptele.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_ptele);
            this.groupBox1.Controls.Add(this.checkBox_ptsym);
            this.groupBox1.Location = new System.Drawing.Point(81, 129);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(195, 49);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "请选择保存类型";
            // 
            // SaveCurPtSymForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(363, 247);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.id);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.name);
            this.Name = "SaveCurPtSymForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "保存当前点符号";
            ((System.ComponentModel.ISupportInitialize)(this.id)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox name;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown id;
        private System.Windows.Forms.CheckBox checkBox_ptsym;
        private System.Windows.Forms.CheckBox checkBox_ptele;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}