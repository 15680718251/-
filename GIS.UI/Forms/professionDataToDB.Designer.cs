namespace GIS.UI.Forms
{
    partial class professionDataToDB
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
            this.txtTb1 = new System.Windows.Forms.TextBox();
            this.txtTb2 = new System.Windows.Forms.TextBox();
            this.opentxtincBt = new System.Windows.Forms.Button();
            this.opentxtBt = new System.Windows.Forms.Button();
            this.StratToDB = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "请选择文件：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "请选择文件：";
            // 
            // txtTb1
            // 
            this.txtTb1.Location = new System.Drawing.Point(118, 29);
            this.txtTb1.Name = "txtTb1";
            this.txtTb1.Size = new System.Drawing.Size(177, 21);
            this.txtTb1.TabIndex = 2;
            // 
            // txtTb2
            // 
            this.txtTb2.Location = new System.Drawing.Point(120, 91);
            this.txtTb2.Name = "txtTb2";
            this.txtTb2.Size = new System.Drawing.Size(177, 21);
            this.txtTb2.TabIndex = 3;
            // 
            // opentxtincBt
            // 
            this.opentxtincBt.Location = new System.Drawing.Point(313, 29);
            this.opentxtincBt.Name = "opentxtincBt";
            this.opentxtincBt.Size = new System.Drawing.Size(57, 23);
            this.opentxtincBt.TabIndex = 4;
            this.opentxtincBt.Text = "打开";
            this.opentxtincBt.UseVisualStyleBackColor = true;
            this.opentxtincBt.Click += new System.EventHandler(this.opentxtincBt_Click);
            // 
            // opentxtBt
            // 
            this.opentxtBt.Location = new System.Drawing.Point(313, 89);
            this.opentxtBt.Name = "opentxtBt";
            this.opentxtBt.Size = new System.Drawing.Size(55, 23);
            this.opentxtBt.TabIndex = 5;
            this.opentxtBt.Text = "打开";
            this.opentxtBt.UseVisualStyleBackColor = true;
            this.opentxtBt.Click += new System.EventHandler(this.opentxtBt_Click);
            // 
            // StratToDB
            // 
            this.StratToDB.Location = new System.Drawing.Point(131, 185);
            this.StratToDB.Name = "StratToDB";
            this.StratToDB.Size = new System.Drawing.Size(75, 23);
            this.StratToDB.TabIndex = 6;
            this.StratToDB.Text = "开始入库";
            this.StratToDB.UseVisualStyleBackColor = true;
            this.StratToDB.Click += new System.EventHandler(this.StratToDB_Click);
            // 
            // professionDataToDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 274);
            this.Controls.Add(this.StratToDB);
            this.Controls.Add(this.opentxtBt);
            this.Controls.Add(this.opentxtincBt);
            this.Controls.Add(this.txtTb2);
            this.Controls.Add(this.txtTb1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "professionDataToDB";
            this.Text = "professionDataToDB";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtTb1;
        private System.Windows.Forms.TextBox txtTb2;
        private System.Windows.Forms.Button opentxtincBt;
        private System.Windows.Forms.Button opentxtBt;
        private System.Windows.Forms.Button StratToDB;
    }
}