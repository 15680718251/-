namespace GIS.TreeIndex.Forms
{
    partial class SaveCurLineSymForm
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
            this.id = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            this.name = new System.Windows.Forms.TextBox();
            this.checkBox_lineele = new System.Windows.Forms.CheckBox();
            this.checkBox_linesym = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.id)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // id
            // 
            this.id.Location = new System.Drawing.Point(168, 52);
            this.id.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.id.Name = "id";
            this.id.Size = new System.Drawing.Size(79, 21);
            this.id.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(166, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 15;
            this.label3.Text = "ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(41, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "名字";
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(168, 200);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(86, 30);
            this.button_cancel.TabIndex = 13;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(40, 200);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(83, 30);
            this.button_ok.TabIndex = 12;
            this.button_ok.Text = "确定";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // name
            // 
            this.name.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.name.Location = new System.Drawing.Point(39, 52);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(97, 21);
            this.name.TabIndex = 11;
            // 
            // checkBox_lineele
            // 
            this.checkBox_lineele.AutoSize = true;
            this.checkBox_lineele.Location = new System.Drawing.Point(14, 19);
            this.checkBox_lineele.Name = "checkBox_lineele";
            this.checkBox_lineele.Size = new System.Drawing.Size(60, 16);
            this.checkBox_lineele.TabIndex = 18;
            this.checkBox_lineele.Text = "线图元";
            this.checkBox_lineele.UseVisualStyleBackColor = true;
            // 
            // checkBox_linesym
            // 
            this.checkBox_linesym.AutoSize = true;
            this.checkBox_linesym.Location = new System.Drawing.Point(127, 19);
            this.checkBox_linesym.Name = "checkBox_linesym";
            this.checkBox_linesym.Size = new System.Drawing.Size(60, 16);
            this.checkBox_linesym.TabIndex = 17;
            this.checkBox_linesym.Text = "线符号";
            this.checkBox_linesym.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_lineele);
            this.groupBox1.Controls.Add(this.checkBox_linesym);
            this.groupBox1.Location = new System.Drawing.Point(49, 117);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(198, 45);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "请选择保存类型";
            // 
            // SaveCurLineSymForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 252);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.id);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.name);
            this.Name = "SaveCurLineSymForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "保存当前线";
            ((System.ComponentModel.ISupportInitialize)(this.id)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown id;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.TextBox name;
        private System.Windows.Forms.CheckBox checkBox_lineele;
        private System.Windows.Forms.CheckBox checkBox_linesym;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}