namespace GIS.UI.Forms
{
    partial class historyQuery
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
            this.查询 = new System.Windows.Forms.Button();
            this.dTPicker1 = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.FeatureLayer = new System.Windows.Forms.ComboBox();
            this.dTPicker2 = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // 查询
            // 
            this.查询.Location = new System.Drawing.Point(12, 90);
            this.查询.Name = "查询";
            this.查询.Size = new System.Drawing.Size(75, 23);
            this.查询.TabIndex = 0;
            this.查询.Text = "查询";
            this.查询.UseVisualStyleBackColor = true;
            this.查询.Click += new System.EventHandler(this.button1_Click);
            // 
            // dTPicker1
            // 
            this.dTPicker1.CustomFormat = "yyyy-MM-dd ";
            this.dTPicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dTPicker1.Location = new System.Drawing.Point(120, 73);
            this.dTPicker1.Name = "dTPicker1";
            this.dTPicker1.ShowUpDown = true;
            this.dTPicker1.Size = new System.Drawing.Size(200, 21);
            this.dTPicker1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(360, 93);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "清除";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "选择数据表";
            // 
            // FeatureLayer
            // 
            this.FeatureLayer.FormattingEnabled = true;
            this.FeatureLayer.Location = new System.Drawing.Point(120, 27);
            this.FeatureLayer.Name = "FeatureLayer";
            this.FeatureLayer.Size = new System.Drawing.Size(121, 20);
            this.FeatureLayer.TabIndex = 8;
            // 
            // dTPicker2
            // 
            this.dTPicker2.CustomFormat = "yyyy-MM-dd ";
            this.dTPicker2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dTPicker2.Location = new System.Drawing.Point(120, 117);
            this.dTPicker2.Name = "dTPicker2";
            this.dTPicker2.ShowUpDown = true;
            this.dTPicker2.Size = new System.Drawing.Size(200, 21);
            this.dTPicker2.TabIndex = 9;
            // 
            // historyQuery
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(463, 170);
            this.Controls.Add(this.dTPicker2);
            this.Controls.Add(this.FeatureLayer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dTPicker1);
            this.Controls.Add(this.查询);
            this.Name = "historyQuery";
            this.Text = "historyQuery";
            this.Load += new System.EventHandler(this.historyQuery_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button 查询;
        private System.Windows.Forms.DateTimePicker dTPicker1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox FeatureLayer;
        private System.Windows.Forms.DateTimePicker dTPicker2;
    }
}