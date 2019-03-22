namespace TrustValueAndReputation.historyToDatabase
{
    partial class ImportOsc
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportOsc));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.showLbl = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.连接数据库ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.开始入库ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.创建增量数据表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.选择OSC文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.开始增量入库ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.优化OSC数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.清空增量数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除增量数据表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.清除查找痕迹ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.增量信誉度交互ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gB图层处理ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.国标图层建表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.国标图层删表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.引入数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.导出ShapeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.清除查找痕迹ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.按时间查找增量ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.数据统计ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gB基态建表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.新建GB基态数据表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除GB基态数据表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.引入当前GB基态数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.更新当前GB数据ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.增量筛选入库ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.录入文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.开始筛选入库ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.导入国家边界ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.功能测试ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.配置文件转换ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oscOpenTB = new System.Windows.Forms.TextBox();
            this.oscPgBar = new System.Windows.Forms.ProgressBar();
            this.exeStateGbox = new System.Windows.Forms.GroupBox();
            this.exeStarteTBox = new System.Windows.Forms.TextBox();
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDlg = new System.Windows.Forms.FolderBrowserDialog();
            this.exePgLbl = new System.Windows.Forms.Label();
            this.saveFileDlg = new System.Windows.Forms.SaveFileDialog();
            this.timeGBox = new System.Windows.Forms.GroupBox();
            this.StartBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.startTimePicker = new System.Windows.Forms.DateTimePicker();
            this.endTimePicker = new System.Windows.Forms.DateTimePicker();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.exeStateGbox.SuspendLayout();
            this.timeGBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.showLbl);
            this.groupBox1.Location = new System.Drawing.Point(14, 79);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(305, 162);
            this.groupBox1.TabIndex = 48;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PostSql连接状态:";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // showLbl
            // 
            this.showLbl.AutoSize = true;
            this.showLbl.Font = new System.Drawing.Font("微软雅黑", 9.163636F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.showLbl.Location = new System.Drawing.Point(17, 35);
            this.showLbl.Name = "showLbl";
            this.showLbl.Size = new System.Drawing.Size(58, 21);
            this.showLbl.TabIndex = 59;
            this.showLbl.Text = "未连接";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.连接数据库ToolStripMenuItem,
            this.开始入库ToolStripMenuItem,
            this.gB图层处理ToolStripMenuItem,
            this.gB基态建表ToolStripMenuItem,
            this.增量筛选入库ToolStripMenuItem,
            this.功能测试ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(694, 28);
            this.menuStrip1.TabIndex = 49;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 连接数据库ToolStripMenuItem
            // 
            this.连接数据库ToolStripMenuItem.Name = "连接数据库ToolStripMenuItem";
            this.连接数据库ToolStripMenuItem.Size = new System.Drawing.Size(96, 24);
            this.连接数据库ToolStripMenuItem.Text = "连接数据库";
            this.连接数据库ToolStripMenuItem.Click += new System.EventHandler(this.连接数据库ToolStripMenuItem_Click);
            // 
            // 开始入库ToolStripMenuItem
            // 
            this.开始入库ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.创建增量数据表ToolStripMenuItem,
            this.选择OSC文件ToolStripMenuItem,
            this.开始增量入库ToolStripMenuItem,
            this.优化OSC数据ToolStripMenuItem,
            this.清空增量数据ToolStripMenuItem,
            this.删除增量数据表ToolStripMenuItem,
            this.清除查找痕迹ToolStripMenuItem,
            this.增量信誉度交互ToolStripMenuItem});
            this.开始入库ToolStripMenuItem.Name = "开始入库ToolStripMenuItem";
            this.开始入库ToolStripMenuItem.Size = new System.Drawing.Size(81, 24);
            this.开始入库ToolStripMenuItem.Text = "增量入库";
            this.开始入库ToolStripMenuItem.Click += new System.EventHandler(this.开始入库ToolStripMenuItem_Click);
            // 
            // 创建增量数据表ToolStripMenuItem
            // 
            this.创建增量数据表ToolStripMenuItem.Name = "创建增量数据表ToolStripMenuItem";
            this.创建增量数据表ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.创建增量数据表ToolStripMenuItem.Text = "创建增量数据表";
            this.创建增量数据表ToolStripMenuItem.Click += new System.EventHandler(this.创建增量数据表ToolStripMenuItem_Click);
            // 
            // 选择OSC文件ToolStripMenuItem
            // 
            this.选择OSC文件ToolStripMenuItem.Name = "选择OSC文件ToolStripMenuItem";
            this.选择OSC文件ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.选择OSC文件ToolStripMenuItem.Text = "选择OSC文件";
            this.选择OSC文件ToolStripMenuItem.Click += new System.EventHandler(this.选择OSC文件ToolStripMenuItem_Click);
            // 
            // 开始增量入库ToolStripMenuItem
            // 
            this.开始增量入库ToolStripMenuItem.Name = "开始增量入库ToolStripMenuItem";
            this.开始增量入库ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.开始增量入库ToolStripMenuItem.Text = "开始增量入库";
            this.开始增量入库ToolStripMenuItem.Click += new System.EventHandler(this.开始增量入库ToolStripMenuItem_Click);
            // 
            // 优化OSC数据ToolStripMenuItem
            // 
            this.优化OSC数据ToolStripMenuItem.Name = "优化OSC数据ToolStripMenuItem";
            this.优化OSC数据ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.优化OSC数据ToolStripMenuItem.Text = "优化OSC数据";
            this.优化OSC数据ToolStripMenuItem.Click += new System.EventHandler(this.优化OSC数据ToolStripMenuItem_Click);
            // 
            // 清空增量数据ToolStripMenuItem
            // 
            this.清空增量数据ToolStripMenuItem.Name = "清空增量数据ToolStripMenuItem";
            this.清空增量数据ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.清空增量数据ToolStripMenuItem.Text = "清空增量数据";
            this.清空增量数据ToolStripMenuItem.Click += new System.EventHandler(this.清空增量数据ToolStripMenuItem_Click);
            // 
            // 删除增量数据表ToolStripMenuItem
            // 
            this.删除增量数据表ToolStripMenuItem.Name = "删除增量数据表ToolStripMenuItem";
            this.删除增量数据表ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.删除增量数据表ToolStripMenuItem.Text = "删除增量数据表";
            this.删除增量数据表ToolStripMenuItem.Click += new System.EventHandler(this.删除增量数据表ToolStripMenuItem_Click);
            // 
            // 清除查找痕迹ToolStripMenuItem
            // 
            this.清除查找痕迹ToolStripMenuItem.Name = "清除查找痕迹ToolStripMenuItem";
            this.清除查找痕迹ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.清除查找痕迹ToolStripMenuItem.Text = "清除查找痕迹";
            this.清除查找痕迹ToolStripMenuItem.Click += new System.EventHandler(this.清除查找痕迹ToolStripMenuItem_Click);
            // 
            // 增量信誉度交互ToolStripMenuItem
            // 
            this.增量信誉度交互ToolStripMenuItem.Name = "增量信誉度交互ToolStripMenuItem";
            this.增量信誉度交互ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.增量信誉度交互ToolStripMenuItem.Text = "增量信誉度交互";
            this.增量信誉度交互ToolStripMenuItem.Click += new System.EventHandler(this.增量信誉度交互ToolStripMenuItem_Click);
            // 
            // gB图层处理ToolStripMenuItem
            // 
            this.gB图层处理ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.国标图层建表ToolStripMenuItem,
            this.国标图层删表ToolStripMenuItem,
            this.引入数据ToolStripMenuItem,
            this.导出ShapeToolStripMenuItem,
            this.清除查找痕迹ToolStripMenuItem1,
            this.按时间查找增量ToolStripMenuItem,
            this.数据统计ToolStripMenuItem});
            this.gB图层处理ToolStripMenuItem.Name = "gB图层处理ToolStripMenuItem";
            this.gB图层处理ToolStripMenuItem.Size = new System.Drawing.Size(101, 24);
            this.gB图层处理ToolStripMenuItem.Text = "GB图层处理";
            // 
            // 国标图层建表ToolStripMenuItem
            // 
            this.国标图层建表ToolStripMenuItem.Name = "国标图层建表ToolStripMenuItem";
            this.国标图层建表ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.国标图层建表ToolStripMenuItem.Text = "国标图层建表";
            this.国标图层建表ToolStripMenuItem.Click += new System.EventHandler(this.国标图层建表ToolStripMenuItem_Click);
            // 
            // 国标图层删表ToolStripMenuItem
            // 
            this.国标图层删表ToolStripMenuItem.Name = "国标图层删表ToolStripMenuItem";
            this.国标图层删表ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.国标图层删表ToolStripMenuItem.Text = "国标图层删表";
            this.国标图层删表ToolStripMenuItem.Click += new System.EventHandler(this.国标图层删表ToolStripMenuItem_Click);
            // 
            // 引入数据ToolStripMenuItem
            // 
            this.引入数据ToolStripMenuItem.Name = "引入数据ToolStripMenuItem";
            this.引入数据ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.引入数据ToolStripMenuItem.Text = "引入OSC数据";
            //this.引入数据ToolStripMenuItem.Click += new System.EventHandler(this.引入数据ToolStripMenuItem_Click);
            // 
            // 导出ShapeToolStripMenuItem
            // 
            this.导出ShapeToolStripMenuItem.Name = "导出ShapeToolStripMenuItem";
            this.导出ShapeToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.导出ShapeToolStripMenuItem.Text = "导出shp文件";
            this.导出ShapeToolStripMenuItem.Click += new System.EventHandler(this.导出ShapeToolStripMenuItem_Click);
            // 
            // 清除查找痕迹ToolStripMenuItem1
            // 
            this.清除查找痕迹ToolStripMenuItem1.Name = "清除查找痕迹ToolStripMenuItem1";
            this.清除查找痕迹ToolStripMenuItem1.Size = new System.Drawing.Size(183, 24);
            this.清除查找痕迹ToolStripMenuItem1.Text = "清除查找痕迹";
            this.清除查找痕迹ToolStripMenuItem1.Click += new System.EventHandler(this.清除查找痕迹ToolStripMenuItem1_Click);
            // 
            // 按时间查找增量ToolStripMenuItem
            // 
            this.按时间查找增量ToolStripMenuItem.Name = "按时间查找增量ToolStripMenuItem";
            this.按时间查找增量ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.按时间查找增量ToolStripMenuItem.Text = "按时间查找增量";
            this.按时间查找增量ToolStripMenuItem.Click += new System.EventHandler(this.按时间查找增量ToolStripMenuItem_Click);
            // 
            // 数据统计ToolStripMenuItem
            // 
            this.数据统计ToolStripMenuItem.Name = "数据统计ToolStripMenuItem";
            this.数据统计ToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.数据统计ToolStripMenuItem.Text = "数据统计";
            this.数据统计ToolStripMenuItem.Click += new System.EventHandler(this.数据统计ToolStripMenuItem_Click);
            // 
            // gB基态建表ToolStripMenuItem
            // 
            this.gB基态建表ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新建GB基态数据表ToolStripMenuItem,
            this.删除GB基态数据表ToolStripMenuItem,
            this.引入当前GB基态数据ToolStripMenuItem,
            this.更新当前GB数据ToolStripMenuItem});
            this.gB基态建表ToolStripMenuItem.Name = "gB基态建表ToolStripMenuItem";
            this.gB基态建表ToolStripMenuItem.Size = new System.Drawing.Size(101, 24);
            this.gB基态建表ToolStripMenuItem.Text = "GB基态更新";
            // 
            // 新建GB基态数据表ToolStripMenuItem
            // 
            this.新建GB基态数据表ToolStripMenuItem.Name = "新建GB基态数据表ToolStripMenuItem";
            this.新建GB基态数据表ToolStripMenuItem.Size = new System.Drawing.Size(222, 24);
            this.新建GB基态数据表ToolStripMenuItem.Text = "新建GB新基态数据表 ";
            this.新建GB基态数据表ToolStripMenuItem.Click += new System.EventHandler(this.新建GB基态数据表ToolStripMenuItem_Click);
            // 
            // 删除GB基态数据表ToolStripMenuItem
            // 
            this.删除GB基态数据表ToolStripMenuItem.Name = "删除GB基态数据表ToolStripMenuItem";
            this.删除GB基态数据表ToolStripMenuItem.Size = new System.Drawing.Size(222, 24);
            this.删除GB基态数据表ToolStripMenuItem.Text = "删除GB新基态数据表";
            this.删除GB基态数据表ToolStripMenuItem.Click += new System.EventHandler(this.删除GB基态数据表ToolStripMenuItem_Click);
            // 
            // 引入当前GB基态数据ToolStripMenuItem
            // 
            this.引入当前GB基态数据ToolStripMenuItem.Name = "引入当前GB基态数据ToolStripMenuItem";
            this.引入当前GB基态数据ToolStripMenuItem.Size = new System.Drawing.Size(222, 24);
            this.引入当前GB基态数据ToolStripMenuItem.Text = "引入当前GB数据";
            this.引入当前GB基态数据ToolStripMenuItem.Click += new System.EventHandler(this.引入当前GB基态数据ToolStripMenuItem_Click);
            // 
            // 更新当前GB数据ToolStripMenuItem
            // 
            this.更新当前GB数据ToolStripMenuItem.Name = "更新当前GB数据ToolStripMenuItem";
            this.更新当前GB数据ToolStripMenuItem.Size = new System.Drawing.Size(222, 24);
            this.更新当前GB数据ToolStripMenuItem.Text = "更新当前GB数据";
            this.更新当前GB数据ToolStripMenuItem.Click += new System.EventHandler(this.更新当前GB数据ToolStripMenuItem_Click);
            // 
            // 增量筛选入库ToolStripMenuItem
            // 
            this.增量筛选入库ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.录入文件ToolStripMenuItem,
            this.开始筛选入库ToolStripMenuItem,
            this.导入国家边界ToolStripMenuItem});
            this.增量筛选入库ToolStripMenuItem.Name = "增量筛选入库ToolStripMenuItem";
            this.增量筛选入库ToolStripMenuItem.Size = new System.Drawing.Size(111, 24);
            this.增量筛选入库ToolStripMenuItem.Text = "增量筛选入库";
            // 
            // 录入文件ToolStripMenuItem
            // 
            this.录入文件ToolStripMenuItem.Name = "录入文件ToolStripMenuItem";
            this.录入文件ToolStripMenuItem.Size = new System.Drawing.Size(169, 24);
            this.录入文件ToolStripMenuItem.Text = "选择OSC文件";
            this.录入文件ToolStripMenuItem.Click += new System.EventHandler(this.录入文件ToolStripMenuItem_Click);
            // 
            // 开始筛选入库ToolStripMenuItem
            // 
            this.开始筛选入库ToolStripMenuItem.Name = "开始筛选入库ToolStripMenuItem";
            this.开始筛选入库ToolStripMenuItem.Size = new System.Drawing.Size(169, 24);
            this.开始筛选入库ToolStripMenuItem.Text = "开始筛选入库";
            this.开始筛选入库ToolStripMenuItem.Click += new System.EventHandler(this.开始筛选入库ToolStripMenuItem_Click);
            // 
            // 导入国家边界ToolStripMenuItem
            // 
            this.导入国家边界ToolStripMenuItem.Name = "导入国家边界ToolStripMenuItem";
            this.导入国家边界ToolStripMenuItem.Size = new System.Drawing.Size(169, 24);
            this.导入国家边界ToolStripMenuItem.Text = "导入国家边界";
            this.导入国家边界ToolStripMenuItem.Click += new System.EventHandler(this.导入国家边界ToolStripMenuItem_Click);
            // 
            // 功能测试ToolStripMenuItem
            // 
            this.功能测试ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.配置文件转换ToolStripMenuItem});
            this.功能测试ToolStripMenuItem.Name = "功能测试ToolStripMenuItem";
            this.功能测试ToolStripMenuItem.Size = new System.Drawing.Size(81, 24);
            this.功能测试ToolStripMenuItem.Text = "功能测试";
            // 
            // 配置文件转换ToolStripMenuItem
            // 
            this.配置文件转换ToolStripMenuItem.Name = "配置文件转换ToolStripMenuItem";
            this.配置文件转换ToolStripMenuItem.Size = new System.Drawing.Size(168, 24);
            this.配置文件转换ToolStripMenuItem.Text = "配置文件转换";
            this.配置文件转换ToolStripMenuItem.Click += new System.EventHandler(this.配置文件转换ToolStripMenuItem_Click);
            // 
            // oscOpenTB
            // 
            this.oscOpenTB.Font = new System.Drawing.Font("微软雅黑", 9.163636F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.oscOpenTB.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.oscOpenTB.Location = new System.Drawing.Point(21, 33);
            this.oscOpenTB.Name = "oscOpenTB";
            this.oscOpenTB.Size = new System.Drawing.Size(659, 28);
            this.oscOpenTB.TabIndex = 51;
            this.oscOpenTB.Text = "<--文件目录-->";
            // 
            // oscPgBar
            // 
            this.oscPgBar.Location = new System.Drawing.Point(6, 553);
            this.oscPgBar.Name = "oscPgBar";
            this.oscPgBar.Size = new System.Drawing.Size(674, 13);
            this.oscPgBar.TabIndex = 53;
            // 
            // exeStateGbox
            // 
            this.exeStateGbox.Controls.Add(this.exeStarteTBox);
            this.exeStateGbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exeStateGbox.Location = new System.Drawing.Point(14, 247);
            this.exeStateGbox.Name = "exeStateGbox";
            this.exeStateGbox.Size = new System.Drawing.Size(666, 268);
            this.exeStateGbox.TabIndex = 55;
            this.exeStateGbox.TabStop = false;
            // 
            // exeStarteTBox
            // 
            this.exeStarteTBox.Font = new System.Drawing.Font("宋体", 10.47273F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.exeStarteTBox.Location = new System.Drawing.Point(8, 21);
            this.exeStarteTBox.Multiline = true;
            this.exeStarteTBox.Name = "exeStarteTBox";
            this.exeStarteTBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.exeStarteTBox.Size = new System.Drawing.Size(647, 232);
            this.exeStarteTBox.TabIndex = 52;
            // 
            // openFileDlg
            // 
            this.openFileDlg.FileName = "openFile";
            this.openFileDlg.Multiselect = true;
            // 
            // exePgLbl
            // 
            this.exePgLbl.AutoSize = true;
            this.exePgLbl.Location = new System.Drawing.Point(10, 528);
            this.exePgLbl.Name = "exePgLbl";
            this.exePgLbl.Size = new System.Drawing.Size(67, 15);
            this.exePgLbl.TabIndex = 59;
            this.exePgLbl.Text = "操作进度";
            // 
            // timeGBox
            // 
            this.timeGBox.Controls.Add(this.StartBtn);
            this.timeGBox.Controls.Add(this.label1);
            this.timeGBox.Controls.Add(this.label2);
            this.timeGBox.Controls.Add(this.startTimePicker);
            this.timeGBox.Controls.Add(this.endTimePicker);
            this.timeGBox.Location = new System.Drawing.Point(334, 90);
            this.timeGBox.Name = "timeGBox";
            this.timeGBox.Size = new System.Drawing.Size(318, 151);
            this.timeGBox.TabIndex = 60;
            this.timeGBox.TabStop = false;
            this.timeGBox.Text = "时间选择";
            // 
            // StartBtn
            // 
            this.StartBtn.Location = new System.Drawing.Point(169, 98);
            this.StartBtn.Name = "StartBtn";
            this.StartBtn.Size = new System.Drawing.Size(73, 31);
            this.StartBtn.TabIndex = 60;
            this.StartBtn.Text = "确定";
            this.StartBtn.UseVisualStyleBackColor = true;
            this.StartBtn.Click += new System.EventHandler(this.StartBtn_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 59;
            this.label1.Text = "结束时间：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 58;
            this.label2.Text = "开始时间：";
            // 
            // startTimePicker
            // 
            this.startTimePicker.Location = new System.Drawing.Point(126, 24);
            this.startTimePicker.Name = "startTimePicker";
            this.startTimePicker.Size = new System.Drawing.Size(169, 25);
            this.startTimePicker.TabIndex = 57;
            this.startTimePicker.Value = new System.DateTime(2014, 8, 9, 0, 0, 0, 0);
            // 
            // endTimePicker
            // 
            this.endTimePicker.Location = new System.Drawing.Point(126, 67);
            this.endTimePicker.Name = "endTimePicker";
            this.endTimePicker.Size = new System.Drawing.Size(169, 25);
            this.endTimePicker.TabIndex = 56;
            // 
            // ImportOsc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 579);
            this.Controls.Add(this.timeGBox);
            this.Controls.Add(this.exePgLbl);
            this.Controls.Add(this.exeStateGbox);
            this.Controls.Add(this.oscPgBar);
            this.Controls.Add(this.oscOpenTB);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ImportOsc";
            this.Text = "OSM增量文件入库";
            this.Load += new System.EventHandler(this.ImportOsc_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.exeStateGbox.ResumeLayout(false);
            this.exeStateGbox.PerformLayout();
            this.timeGBox.ResumeLayout(false);
            this.timeGBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 连接数据库ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 开始入库ToolStripMenuItem;
        private System.Windows.Forms.TextBox oscOpenTB;
        private System.Windows.Forms.ProgressBar oscPgBar;
        private System.Windows.Forms.ToolStripMenuItem 创建增量数据表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 开始增量入库ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除增量数据表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 清空增量数据ToolStripMenuItem;
        private System.Windows.Forms.GroupBox exeStateGbox;
        private System.Windows.Forms.ToolStripMenuItem 优化OSC数据ToolStripMenuItem;
        private System.Windows.Forms.TextBox exeStarteTBox;
        private System.Windows.Forms.OpenFileDialog openFileDlg;
        private System.Windows.Forms.ToolStripMenuItem 选择OSC文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gB图层处理ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 国标图层建表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 国标图层删表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 引入数据ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 导出ShapeToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDlg;
        private System.Windows.Forms.ToolStripMenuItem 清除查找痕迹ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 清除查找痕迹ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem 按时间查找增量ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gB基态建表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 新建GB基态数据表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除GB基态数据表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 引入当前GB基态数据ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 更新当前GB数据ToolStripMenuItem;
        private System.Windows.Forms.Label showLbl;
        private System.Windows.Forms.ToolStripMenuItem 增量筛选入库ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 录入文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 开始筛选入库ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 导入国家边界ToolStripMenuItem;
        private System.Windows.Forms.Label exePgLbl;
        private System.Windows.Forms.ToolStripMenuItem 数据统计ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 功能测试ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 配置文件转换ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 增量信誉度交互ToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDlg;
        private System.Windows.Forms.GroupBox timeGBox;
        private System.Windows.Forms.Button StartBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker startTimePicker;
        private System.Windows.Forms.DateTimePicker endTimePicker;
    }
}