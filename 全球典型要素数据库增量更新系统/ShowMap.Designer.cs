namespace 全球典型要素数据库增量更新系统
{
    partial class ShowMap
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
            this.button2 = new System.Windows.Forms.Button();
            this.RasterLayer = new System.Windows.Forms.ComboBox();
            this.FeatureLayer = new System.Windows.Forms.ComboBox();
            this.cbbDataSet = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(161, 326);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(151, 23);
            this.button2.TabIndex = 12;
            this.button2.Text = "加载数据到地图";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // RasterLayer
            // 
            this.RasterLayer.FormattingEnabled = true;
            this.RasterLayer.Location = new System.Drawing.Point(249, 268);
            this.RasterLayer.Name = "RasterLayer";
            this.RasterLayer.Size = new System.Drawing.Size(121, 20);
            this.RasterLayer.TabIndex = 10;
            // 
            // FeatureLayer
            // 
            this.FeatureLayer.FormattingEnabled = true;
            this.FeatureLayer.Location = new System.Drawing.Point(249, 216);
            this.FeatureLayer.Name = "FeatureLayer";
            this.FeatureLayer.Size = new System.Drawing.Size(121, 20);
            this.FeatureLayer.TabIndex = 11;
            // 
            // cbbDataSet
            // 
            this.cbbDataSet.FormattingEnabled = true;
            this.cbbDataSet.Location = new System.Drawing.Point(249, 160);
            this.cbbDataSet.Name = "cbbDataSet";
            this.cbbDataSet.Size = new System.Drawing.Size(121, 20);
            this.cbbDataSet.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(103, 271);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "栅格数据";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(103, 216);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "矢量数据";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(103, 169);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "数据集";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(143, 72);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(151, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "刷新";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ShowMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 457);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.RasterLayer);
            this.Controls.Add(this.FeatureLayer);
            this.Controls.Add(this.cbbDataSet);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Name = "ShowMap";
            this.Text = "ShowMap";
            this.Load += new System.EventHandler(this.ShowMap_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox RasterLayer;
        private System.Windows.Forms.ComboBox FeatureLayer;
        private System.Windows.Forms.ComboBox cbbDataSet;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}