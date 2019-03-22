namespace GIS.UI.Forms
{
    partial class hist
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
            this.comBox = new System.Windows.Forms.ComboBox();
            this.查询 = new System.Windows.Forms.Button();
            this.dTPicker1 = new System.Windows.Forms.DateTimePicker();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // comBox
            // 
            this.comBox.FormattingEnabled = true;
            this.comBox.Location = new System.Drawing.Point(12, 33);
            this.comBox.Name = "comBox";
            this.comBox.Size = new System.Drawing.Size(171, 20);
            this.comBox.TabIndex = 9;
            this.comBox.SelectedIndexChanged += new System.EventHandler(this.FeatureLayer_SelectedIndexChanged);
            // 
            // 查询
            // 
            this.查询.Location = new System.Drawing.Point(189, 175);
            this.查询.Name = "查询";
            this.查询.Size = new System.Drawing.Size(62, 23);
            this.查询.TabIndex = 11;
            this.查询.Text = "查询";
            this.查询.UseVisualStyleBackColor = true;
            this.查询.Click += new System.EventHandler(this.查询_Click);
            // 
            // dTPicker1
            // 
            this.dTPicker1.CustomFormat = "yyyy-MM-dd ";
            this.dTPicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dTPicker1.Location = new System.Drawing.Point(12, 77);
            this.dTPicker1.Name = "dTPicker1";
            this.dTPicker1.ShowUpDown = true;
            this.dTPicker1.Size = new System.Drawing.Size(171, 21);
            this.dTPicker1.TabIndex = 10;
            this.dTPicker1.ValueChanged += new System.EventHandler(this.dTPicker1_ValueChanged);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(12, 122);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(171, 76);
            this.listBox1.TabIndex = 15;
            // 
            // hist
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(254, 234);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.dTPicker1);
            this.Controls.Add(this.查询);
            this.Controls.Add(this.comBox);
            this.Name = "hist";
            this.Text = "hist";
            this.Load += new System.EventHandler(this.hist_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comBox;
        private System.Windows.Forms.Button 查询;
        private System.Windows.Forms.DateTimePicker dTPicker1;
        private System.Windows.Forms.ListBox listBox1;
    }
}