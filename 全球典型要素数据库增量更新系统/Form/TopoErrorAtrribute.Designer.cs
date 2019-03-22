namespace 全球典型要素数据库增量更新系统
{
    partial class TopoErrorAtrribute
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
            this.dataGridAttribute = new System.Windows.Forms.DataGridView();
            this.check = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.cbb_solution = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_ok = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAttribute)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridAttribute
            // 
            this.dataGridAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridAttribute.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridAttribute.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.check});
            this.dataGridAttribute.Location = new System.Drawing.Point(1, 1);
            this.dataGridAttribute.Name = "dataGridAttribute";
            this.dataGridAttribute.RowTemplate.Height = 23;
            this.dataGridAttribute.Size = new System.Drawing.Size(618, 368);
            this.dataGridAttribute.TabIndex = 0;
            this.dataGridAttribute.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridAttribute_RowHeaderMouseClick);
            // 
            // check
            // 
            this.check.HeaderText = "check";
            this.check.Name = "check";
            // 
            // cbb_solution
            // 
            this.cbb_solution.FormattingEnabled = true;
            this.cbb_solution.Location = new System.Drawing.Point(141, 386);
            this.cbb_solution.Name = "cbb_solution";
            this.cbb_solution.Size = new System.Drawing.Size(121, 20);
            this.cbb_solution.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(62, 389);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "处理方案：";
            // 
            // tb_ok
            // 
            this.tb_ok.Location = new System.Drawing.Point(453, 387);
            this.tb_ok.Name = "tb_ok";
            this.tb_ok.Size = new System.Drawing.Size(75, 23);
            this.tb_ok.TabIndex = 3;
            this.tb_ok.Text = "确定";
            this.tb_ok.UseVisualStyleBackColor = true;
            this.tb_ok.Click += new System.EventHandler(this.tb_ok_Click);
            // 
            // TopoErrorAtrribute
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 433);
            this.Controls.Add(this.tb_ok);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbb_solution);
            this.Controls.Add(this.dataGridAttribute);
            this.Name = "TopoErrorAtrribute";
            this.Text = "TopoErrorAtrribute";
            this.Load += new System.EventHandler(this.TopoErrorAtrribute_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAttribute)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridAttribute;
        private System.Windows.Forms.DataGridViewCheckBoxColumn check;
        private System.Windows.Forms.ComboBox cbb_solution;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button tb_ok;
    }
}