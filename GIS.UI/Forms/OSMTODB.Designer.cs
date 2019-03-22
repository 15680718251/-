namespace GIS.UI.Forms
{
    partial class OSMTODB
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
            this.txtOSMPath = new System.Windows.Forms.TextBox();
            this.osmFilebtn = new System.Windows.Forms.Button();
            this.exeStarteTBox = new System.Windows.Forms.TextBox();
            this.oscPgBar = new System.Windows.Forms.ProgressBar();
            this.button2 = new System.Windows.Forms.Button();
            this.Closebtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtOSMPath
            // 
            this.txtOSMPath.Font = new System.Drawing.Font("宋体", 10.47273F);
            this.txtOSMPath.Location = new System.Drawing.Point(12, 23);
            this.txtOSMPath.Name = "txtOSMPath";
            this.txtOSMPath.ReadOnly = true;
            this.txtOSMPath.Size = new System.Drawing.Size(350, 23);
            this.txtOSMPath.TabIndex = 0;
            this.txtOSMPath.Text = "<- -OSM文件目录- ->";
            // 
            // osmFilebtn
            // 
            this.osmFilebtn.Location = new System.Drawing.Point(368, 23);
            this.osmFilebtn.Name = "osmFilebtn";
            this.osmFilebtn.Size = new System.Drawing.Size(75, 23);
            this.osmFilebtn.TabIndex = 1;
            this.osmFilebtn.Text = "打开";
            this.osmFilebtn.UseVisualStyleBackColor = true;
            this.osmFilebtn.Click += new System.EventHandler(this.osmFilebtn_Click);
            // 
            // exeStarteTBox
            // 
            this.exeStarteTBox.Location = new System.Drawing.Point(12, 66);
            this.exeStarteTBox.Multiline = true;
            this.exeStarteTBox.Name = "exeStarteTBox";
            this.exeStarteTBox.ReadOnly = true;
            this.exeStarteTBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.exeStarteTBox.Size = new System.Drawing.Size(431, 210);
            this.exeStarteTBox.TabIndex = 2;
            // 
            // oscPgBar
            // 
            this.oscPgBar.Location = new System.Drawing.Point(-1, 316);
            this.oscPgBar.Name = "oscPgBar";
            this.oscPgBar.Size = new System.Drawing.Size(456, 14);
            this.oscPgBar.TabIndex = 3;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(83, 282);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "开始入库";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Closebtn
            // 
            this.Closebtn.Location = new System.Drawing.Point(282, 282);
            this.Closebtn.Name = "Closebtn";
            this.Closebtn.Size = new System.Drawing.Size(75, 23);
            this.Closebtn.TabIndex = 5;
            this.Closebtn.Text = "退出";
            this.Closebtn.UseVisualStyleBackColor = true;
            this.Closebtn.Click += new System.EventHandler(this.Closebtn_Click);
            // 
            // OSMTODB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 327);
            this.Controls.Add(this.Closebtn);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.oscPgBar);
            this.Controls.Add(this.exeStarteTBox);
            this.Controls.Add(this.osmFilebtn);
            this.Controls.Add(this.txtOSMPath);
            this.Name = "OSMTODB";
            this.Text = "OSMTODB";
            //this.Load += new System.EventHandler(this.OSMTODB_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtOSMPath;
        private System.Windows.Forms.Button osmFilebtn;
        private System.Windows.Forms.TextBox exeStarteTBox;
        private System.Windows.Forms.ProgressBar oscPgBar;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button Closebtn;
    }
}