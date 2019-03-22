namespace GIS.TreeIndex.Forms
{
    partial class PolygonSymbolizeFrm
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
            this.m_cbxFiledNames = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.m_btnSymbolize = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.aphaValueTrackBar = new System.Windows.Forms.TrackBar();
            this.m_dgvSymbolize = new System.Windows.Forms.DataGridView();
            this.符号 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.属性 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aphaValueTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvSymbolize)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "选择字段：";
            // 
            // m_cbxFiledNames
            // 
            this.m_cbxFiledNames.FormattingEnabled = true;
            this.m_cbxFiledNames.Location = new System.Drawing.Point(101, 34);
            this.m_cbxFiledNames.Name = "m_cbxFiledNames";
            this.m_cbxFiledNames.Size = new System.Drawing.Size(229, 20);
            this.m_cbxFiledNames.TabIndex = 1;
            this.m_cbxFiledNames.SelectedValueChanged += new System.EventHandler(this.m_cbxFiledNames_SelectedValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.m_btnSymbolize);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.m_cbxFiledNames);
            this.groupBox1.Location = new System.Drawing.Point(24, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(484, 78);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // m_btnSymbolize
            // 
            this.m_btnSymbolize.Font = new System.Drawing.Font("华文行楷", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.m_btnSymbolize.Location = new System.Drawing.Point(355, 25);
            this.m_btnSymbolize.Name = "m_btnSymbolize";
            this.m_btnSymbolize.Size = new System.Drawing.Size(82, 34);
            this.m_btnSymbolize.TabIndex = 2;
            this.m_btnSymbolize.Text = "确定";
            this.m_btnSymbolize.UseVisualStyleBackColor = true;
            this.m_btnSymbolize.Click += new System.EventHandler(this.m_btnSymbolize_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("隶书", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ForeColor = System.Drawing.Color.Maroon;
            this.label2.Location = new System.Drawing.Point(17, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(18, 18);
            this.label2.TabIndex = 15;
            this.label2.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("隶书", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ForeColor = System.Drawing.Color.Maroon;
            this.label3.Location = new System.Drawing.Point(417, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 18);
            this.label3.TabIndex = 15;
            this.label3.Text = "255";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.aphaValueTrackBar);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(24, 453);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(484, 81);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "透明度设置";
            // 
            // aphaValueTrackBar
            // 
            this.aphaValueTrackBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.aphaValueTrackBar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.aphaValueTrackBar.Location = new System.Drawing.Point(41, 30);
            this.aphaValueTrackBar.Maximum = 255;
            this.aphaValueTrackBar.Name = "aphaValueTrackBar";
            this.aphaValueTrackBar.Size = new System.Drawing.Size(361, 45);
            this.aphaValueTrackBar.TabIndex = 16;
            this.aphaValueTrackBar.TickFrequency = 10;
            this.aphaValueTrackBar.Value = 255;
            this.aphaValueTrackBar.Scroll += new System.EventHandler(this.aphaValueTrackBar_Scroll);
            // 
            // m_dgvSymbolize
            // 
            this.m_dgvSymbolize.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvSymbolize.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.符号,
            this.属性});
            this.m_dgvSymbolize.Location = new System.Drawing.Point(24, 96);
            this.m_dgvSymbolize.Name = "m_dgvSymbolize";
            this.m_dgvSymbolize.RowTemplate.Height = 23;
            this.m_dgvSymbolize.Size = new System.Drawing.Size(484, 351);
            this.m_dgvSymbolize.TabIndex = 17;
            this.m_dgvSymbolize.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.m_dgvSymbolize_CellMouseDoubleClick);
            this.m_dgvSymbolize.SelectionChanged += new System.EventHandler(this.m_dgvSymbolize_SelectionChanged);
            // 
            // 符号
            // 
            this.符号.HeaderText = "符号";
            this.符号.Name = "符号";
            this.符号.Width = 150;
            // 
            // 属性
            // 
            this.属性.HeaderText = "属性";
            this.属性.Name = "属性";
            this.属性.Width = 150;
            // 
            // PolygonSymbolizeFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(535, 535);
            this.Controls.Add(this.m_dgvSymbolize);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "PolygonSymbolizeFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PolygonSymbolizeFrm";
            this.Load += new System.EventHandler(this.PolygonSymbolizeFrm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aphaValueTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvSymbolize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox m_cbxFiledNames;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button m_btnSymbolize;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TrackBar aphaValueTrackBar;
        private System.Windows.Forms.DataGridView m_dgvSymbolize;
        private System.Windows.Forms.DataGridViewTextBoxColumn 符号;
        private System.Windows.Forms.DataGridViewTextBoxColumn 属性;
    }
}