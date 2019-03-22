namespace GIS.UI.Forms
{
    partial class ModelTrans
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
            this.closeBtn = new System.Windows.Forms.Button();
            this.startBtn = new System.Windows.Forms.Button();
            this.pgBar = new System.Windows.Forms.ProgressBar();
            this.stateTBox = new System.Windows.Forms.TextBox();
            this.tableNameCBX = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // closeBtn
            // 
            this.closeBtn.Location = new System.Drawing.Point(159, 242);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(75, 23);
            this.closeBtn.TabIndex = 22;
            this.closeBtn.Text = "退出";
            this.closeBtn.UseVisualStyleBackColor = true;
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // startBtn
            // 
            this.startBtn.Location = new System.Drawing.Point(41, 242);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(75, 23);
            this.startBtn.TabIndex = 21;
            this.startBtn.Text = "开始";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // pgBar
            // 
            this.pgBar.Location = new System.Drawing.Point(0, 283);
            this.pgBar.Name = "pgBar";
            this.pgBar.Size = new System.Drawing.Size(284, 15);
            this.pgBar.TabIndex = 20;
            // 
            // stateTBox
            // 
            this.stateTBox.Location = new System.Drawing.Point(12, 37);
            this.stateTBox.Multiline = true;
            this.stateTBox.Name = "stateTBox";
            this.stateTBox.ReadOnly = true;
            this.stateTBox.Size = new System.Drawing.Size(259, 190);
            this.stateTBox.TabIndex = 19;
            // 
            // tableNameCBX
            // 
            this.tableNameCBX.FormattingEnabled = true;
            this.tableNameCBX.Location = new System.Drawing.Point(169, 5);
            this.tableNameCBX.Name = "tableNameCBX";
            this.tableNameCBX.Size = new System.Drawing.Size(102, 20);
            this.tableNameCBX.TabIndex = 23;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(98, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 24;
            this.label1.Text = "选择表名：";
            // 
            // ModelTrans
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 298);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tableNameCBX);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.pgBar);
            this.Controls.Add(this.stateTBox);
            this.Name = "ModelTrans";
            this.Text = "ModelTrans";
            this.Load += new System.EventHandler(this.ModelTrans_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.ProgressBar pgBar;
        private System.Windows.Forms.TextBox stateTBox;
        private System.Windows.Forms.ComboBox tableNameCBX;
        private System.Windows.Forms.Label label1;
    }
}