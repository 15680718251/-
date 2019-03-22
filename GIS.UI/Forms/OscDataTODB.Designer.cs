namespace GIS.UI.Forms
{
    partial class OscDataTODB
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
            this.OscPgBar = new System.Windows.Forms.ProgressBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ExitBtn = new System.Windows.Forms.Button();
            this.开始入库Btn = new System.Windows.Forms.Button();
            this.OSCexeStarteTBox = new System.Windows.Forms.TextBox();
            this.OscFilebtn = new System.Windows.Forms.Button();
            this.OscOpenTBox = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.新建增量数据表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除增量数据表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.清空增量数据表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(366, 433);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "100%";
            // 
            // OscPgBar
            // 
            this.OscPgBar.Location = new System.Drawing.Point(12, 426);
            this.OscPgBar.Name = "OscPgBar";
            this.OscPgBar.Size = new System.Drawing.Size(354, 23);
            this.OscPgBar.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ExitBtn);
            this.groupBox1.Controls.Add(this.开始入库Btn);
            this.groupBox1.Controls.Add(this.OSCexeStarteTBox);
            this.groupBox1.Controls.Add(this.OscFilebtn);
            this.groupBox1.Controls.Add(this.OscOpenTBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(394, 379);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            // 
            // ExitBtn
            // 
            this.ExitBtn.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ExitBtn.Location = new System.Drawing.Point(230, 348);
            this.ExitBtn.Name = "ExitBtn";
            this.ExitBtn.Size = new System.Drawing.Size(75, 23);
            this.ExitBtn.TabIndex = 4;
            this.ExitBtn.Text = "退出";
            this.ExitBtn.UseVisualStyleBackColor = true;
            this.ExitBtn.Click += new System.EventHandler(this.ExitBtn_Click_1);
            // 
            // 开始入库Btn
            // 
            this.开始入库Btn.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.开始入库Btn.Location = new System.Drawing.Point(66, 348);
            this.开始入库Btn.Name = "开始入库Btn";
            this.开始入库Btn.Size = new System.Drawing.Size(75, 23);
            this.开始入库Btn.TabIndex = 3;
            this.开始入库Btn.Text = "开始入库";
            this.开始入库Btn.UseVisualStyleBackColor = true;
            this.开始入库Btn.Click += new System.EventHandler(this.开始入库Btn_Click);
            // 
            // OSCexeStarteTBox
            // 
            this.OSCexeStarteTBox.BackColor = System.Drawing.SystemColors.MenuBar;
            this.OSCexeStarteTBox.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.OSCexeStarteTBox.Location = new System.Drawing.Point(6, 47);
            this.OSCexeStarteTBox.Multiline = true;
            this.OSCexeStarteTBox.Name = "OSCexeStarteTBox";
            this.OSCexeStarteTBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.OSCexeStarteTBox.Size = new System.Drawing.Size(378, 295);
            this.OSCexeStarteTBox.TabIndex = 2;
            // 
            // OscFilebtn
            // 
            this.OscFilebtn.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.OscFilebtn.Location = new System.Drawing.Point(309, 18);
            this.OscFilebtn.Name = "OscFilebtn";
            this.OscFilebtn.Size = new System.Drawing.Size(75, 23);
            this.OscFilebtn.TabIndex = 1;
            this.OscFilebtn.Text = "打开";
            this.OscFilebtn.UseVisualStyleBackColor = true;
            this.OscFilebtn.Click += new System.EventHandler(this.OscFilebtn_Click);
            // 
            // OscOpenTBox
            // 
            this.OscOpenTBox.BackColor = System.Drawing.SystemColors.MenuBar;
            this.OscOpenTBox.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.OscOpenTBox.Location = new System.Drawing.Point(6, 20);
            this.OscOpenTBox.Name = "OscOpenTBox";
            this.OscOpenTBox.Size = new System.Drawing.Size(299, 23);
            this.OscOpenTBox.TabIndex = 0;
            this.OscOpenTBox.Text = "<- -OSC文件目录- ->";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.MenuBar;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新建增量数据表ToolStripMenuItem,
            this.删除增量数据表ToolStripMenuItem,
            this.清空增量数据表ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(418, 29);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 新建增量数据表ToolStripMenuItem
            // 
            this.新建增量数据表ToolStripMenuItem.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.新建增量数据表ToolStripMenuItem.Name = "新建增量数据表ToolStripMenuItem";
            this.新建增量数据表ToolStripMenuItem.Size = new System.Drawing.Size(134, 25);
            this.新建增量数据表ToolStripMenuItem.Text = "新建增量数据表";
            this.新建增量数据表ToolStripMenuItem.Click += new System.EventHandler(this.新建增量数据表ToolStripMenuItem_Click);
            // 
            // 删除增量数据表ToolStripMenuItem
            // 
            this.删除增量数据表ToolStripMenuItem.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.删除增量数据表ToolStripMenuItem.Name = "删除增量数据表ToolStripMenuItem";
            this.删除增量数据表ToolStripMenuItem.Size = new System.Drawing.Size(134, 25);
            this.删除增量数据表ToolStripMenuItem.Text = "删除增量数据表";
            this.删除增量数据表ToolStripMenuItem.Click += new System.EventHandler(this.删除增量数据表ToolStripMenuItem_Click);
            // 
            // 清空增量数据表ToolStripMenuItem
            // 
            this.清空增量数据表ToolStripMenuItem.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.清空增量数据表ToolStripMenuItem.Name = "清空增量数据表ToolStripMenuItem";
            this.清空增量数据表ToolStripMenuItem.Size = new System.Drawing.Size(134, 25);
            this.清空增量数据表ToolStripMenuItem.Text = "清空增量数据表";
            this.清空增量数据表ToolStripMenuItem.Click += new System.EventHandler(this.清空增量数据表ToolStripMenuItem_Click);
            // 
            // OscDataTODB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 462);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OscPgBar);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "OscDataTODB";
            this.Text = "OscDataTODB";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar OscPgBar;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ExitBtn;
        private System.Windows.Forms.Button 开始入库Btn;
        private System.Windows.Forms.TextBox OSCexeStarteTBox;
        private System.Windows.Forms.Button OscFilebtn;
        private System.Windows.Forms.TextBox OscOpenTBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 新建增量数据表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除增量数据表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 清空增量数据表ToolStripMenuItem;
    }
}