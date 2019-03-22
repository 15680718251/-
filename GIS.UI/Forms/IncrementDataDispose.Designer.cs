namespace GIS.UI.Forms
{
    partial class IncrementDataDispose
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
            this.DisposeBt = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tableNameCBX = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // DisposeBt
            // 
            this.DisposeBt.Location = new System.Drawing.Point(74, 71);
            this.DisposeBt.Name = "DisposeBt";
            this.DisposeBt.Size = new System.Drawing.Size(130, 23);
            this.DisposeBt.TabIndex = 5;
            this.DisposeBt.Text = "处理";
            this.DisposeBt.UseVisualStyleBackColor = true;
            this.DisposeBt.Click += new System.EventHandler(this.DisposeBt_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "选择处理数据：";
            // 
            // tableNameCBX
            // 
            this.tableNameCBX.FormattingEnabled = true;
            this.tableNameCBX.Location = new System.Drawing.Point(102, 18);
            this.tableNameCBX.Name = "tableNameCBX";
            this.tableNameCBX.Size = new System.Drawing.Size(160, 20);
            this.tableNameCBX.TabIndex = 3;
            // 
            // IncrementDataDispose
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 121);
            this.Controls.Add(this.DisposeBt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tableNameCBX);
            this.Name = "IncrementDataDispose";
            this.Text = "IncrementDataDispose";
            this.Load += new System.EventHandler(this.IncrementDataDispose_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button DisposeBt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox tableNameCBX;
    }
}