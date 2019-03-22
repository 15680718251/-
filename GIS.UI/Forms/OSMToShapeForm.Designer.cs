namespace GIS.UI.Forms
{
    partial class OSMToShapeForm
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
            this.TransCoord = new System.Windows.Forms.CheckBox();
            this.lblShpTip = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.newRadioBt = new System.Windows.Forms.RadioButton();
            this.oscRadioBt = new System.Windows.Forms.RadioButton();
            this.baseRadioBt = new System.Windows.Forms.RadioButton();
            this.allCheckBox = new System.Windows.Forms.CheckBox();
            this.showCheckedBox = new System.Windows.Forms.CheckBox();
            this.area_CheckBox = new System.Windows.Forms.CheckBox();
            this.line_CheckBox = new System.Windows.Forms.CheckBox();
            this.point_CheckBox = new System.Windows.Forms.CheckBox();
            this.NoOfSelectedTable = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label2 = new System.Windows.Forms.Label();
            this.saveBtn = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // TransCoord
            // 
            this.TransCoord.AutoSize = true;
            this.TransCoord.Location = new System.Drawing.Point(279, 2);
            this.TransCoord.Name = "TransCoord";
            this.TransCoord.Size = new System.Drawing.Size(72, 16);
            this.TransCoord.TabIndex = 98;
            this.TransCoord.Text = "投影转换";
            this.TransCoord.UseVisualStyleBackColor = true;
            // 
            // lblShpTip
            // 
            this.lblShpTip.AutoSize = true;
            this.lblShpTip.Location = new System.Drawing.Point(12, 446);
            this.lblShpTip.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblShpTip.Name = "lblShpTip";
            this.lblShpTip.Size = new System.Drawing.Size(0, 12);
            this.lblShpTip.TabIndex = 97;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 450);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip1.Size = new System.Drawing.Size(355, 22);
            this.statusStrip1.TabIndex = 96;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(338, 16);
            // 
            // newRadioBt
            // 
            this.newRadioBt.AutoSize = true;
            this.newRadioBt.Location = new System.Drawing.Point(111, 0);
            this.newRadioBt.Name = "newRadioBt";
            this.newRadioBt.Size = new System.Drawing.Size(59, 16);
            this.newRadioBt.TabIndex = 95;
            this.newRadioBt.TabStop = true;
            this.newRadioBt.Text = "更新后";
            this.newRadioBt.UseVisualStyleBackColor = true;
            // 
            // oscRadioBt
            // 
            this.oscRadioBt.AutoSize = true;
            this.oscRadioBt.Location = new System.Drawing.Point(59, 0);
            this.oscRadioBt.Name = "oscRadioBt";
            this.oscRadioBt.Size = new System.Drawing.Size(47, 16);
            this.oscRadioBt.TabIndex = 94;
            this.oscRadioBt.TabStop = true;
            this.oscRadioBt.Text = "增量";
            this.oscRadioBt.UseVisualStyleBackColor = true;
            // 
            // baseRadioBt
            // 
            this.baseRadioBt.AutoSize = true;
            this.baseRadioBt.Checked = true;
            this.baseRadioBt.Location = new System.Drawing.Point(14, 0);
            this.baseRadioBt.Name = "baseRadioBt";
            this.baseRadioBt.Size = new System.Drawing.Size(47, 16);
            this.baseRadioBt.TabIndex = 93;
            this.baseRadioBt.TabStop = true;
            this.baseRadioBt.Text = "基态";
            this.baseRadioBt.UseVisualStyleBackColor = true;
            // 
            // allCheckBox
            // 
            this.allCheckBox.AutoSize = true;
            this.allCheckBox.Location = new System.Drawing.Point(176, 22);
            this.allCheckBox.Name = "allCheckBox";
            this.allCheckBox.Size = new System.Drawing.Size(48, 16);
            this.allCheckBox.TabIndex = 92;
            this.allCheckBox.Text = "全选";
            this.allCheckBox.UseVisualStyleBackColor = true;
            // 
            // showCheckedBox
            // 
            this.showCheckedBox.AutoSize = true;
            this.showCheckedBox.Location = new System.Drawing.Point(176, 1);
            this.showCheckedBox.Name = "showCheckedBox";
            this.showCheckedBox.Size = new System.Drawing.Size(96, 16);
            this.showCheckedBox.TabIndex = 91;
            this.showCheckedBox.Text = "显示选中表格";
            this.showCheckedBox.UseVisualStyleBackColor = true;
            // 
            // area_CheckBox
            // 
            this.area_CheckBox.AutoSize = true;
            this.area_CheckBox.Location = new System.Drawing.Point(111, 22);
            this.area_CheckBox.Name = "area_CheckBox";
            this.area_CheckBox.Size = new System.Drawing.Size(36, 16);
            this.area_CheckBox.TabIndex = 90;
            this.area_CheckBox.Text = "面";
            this.area_CheckBox.UseVisualStyleBackColor = true;
            // 
            // line_CheckBox
            // 
            this.line_CheckBox.AutoSize = true;
            this.line_CheckBox.Location = new System.Drawing.Point(59, 22);
            this.line_CheckBox.Name = "line_CheckBox";
            this.line_CheckBox.Size = new System.Drawing.Size(36, 16);
            this.line_CheckBox.TabIndex = 89;
            this.line_CheckBox.Text = "线";
            this.line_CheckBox.UseVisualStyleBackColor = true;
            // 
            // point_CheckBox
            // 
            this.point_CheckBox.AutoSize = true;
            this.point_CheckBox.Location = new System.Drawing.Point(14, 22);
            this.point_CheckBox.Name = "point_CheckBox";
            this.point_CheckBox.Size = new System.Drawing.Size(36, 16);
            this.point_CheckBox.TabIndex = 88;
            this.point_CheckBox.Text = "点";
            this.point_CheckBox.UseVisualStyleBackColor = true;
            // 
            // NoOfSelectedTable
            // 
            this.NoOfSelectedTable.AutoSize = true;
            this.NoOfSelectedTable.Location = new System.Drawing.Point(230, 26);
            this.NoOfSelectedTable.Name = "NoOfSelectedTable";
            this.NoOfSelectedTable.Size = new System.Drawing.Size(47, 12);
            this.NoOfSelectedTable.TabIndex = 87;
            this.NoOfSelectedTable.Text = "TableNo";
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            this.dataGridView1.Location = new System.Drawing.Point(12, 41);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 5;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(300, 324);
            this.dataGridView1.TabIndex = 86;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "选择";
            this.Column1.Name = "Column1";
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Column1.Width = 45;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "表名";
            this.Column2.Name = "Column2";
            this.Column2.Width = 250;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 376);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 85;
            this.label2.Text = "保存：";
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(286, 371);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(26, 23);
            this.saveBtn.TabIndex = 84;
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(59, 371);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(215, 21);
            this.textBox1.TabIndex = 83;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(206, 398);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(54, 23);
            this.button2.TabIndex = 82;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(59, 398);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(54, 23);
            this.button1.TabIndex = 81;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // OSMToShapeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 472);
            this.Controls.Add(this.TransCoord);
            this.Controls.Add(this.lblShpTip);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.newRadioBt);
            this.Controls.Add(this.oscRadioBt);
            this.Controls.Add(this.baseRadioBt);
            this.Controls.Add(this.allCheckBox);
            this.Controls.Add(this.showCheckedBox);
            this.Controls.Add(this.area_CheckBox);
            this.Controls.Add(this.line_CheckBox);
            this.Controls.Add(this.point_CheckBox);
            this.Controls.Add(this.NoOfSelectedTable);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "OSMToShapeForm";
            this.Text = "OSMToShapeForm";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox TransCoord;
        private System.Windows.Forms.Label lblShpTip;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.RadioButton newRadioBt;
        private System.Windows.Forms.RadioButton oscRadioBt;
        private System.Windows.Forms.RadioButton baseRadioBt;
        private System.Windows.Forms.CheckBox allCheckBox;
        private System.Windows.Forms.CheckBox showCheckedBox;
        private System.Windows.Forms.CheckBox area_CheckBox;
        private System.Windows.Forms.CheckBox line_CheckBox;
        private System.Windows.Forms.CheckBox point_CheckBox;
        private System.Windows.Forms.Label NoOfSelectedTable;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}