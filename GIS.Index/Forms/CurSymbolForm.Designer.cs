namespace GIS.TreeIndex.Forms
{
    partial class CurSymbolForm
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
            this.cancel = new System.Windows.Forms.Button();
            this.listView_cursymbol = new System.Windows.Forms.ListView();
            this.comboBox_type = new System.Windows.Forms.ComboBox();
            this.delete = new System.Windows.Forms.Button();
            this.searchid = new System.Windows.Forms.NumericUpDown();
            this.search = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ok = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.count = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.searchid)).BeginInit();
            this.SuspendLayout();
            // 
            // cancel
            // 
            this.cancel.Location = new System.Drawing.Point(314, 434);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(72, 31);
            this.cancel.TabIndex = 4;
            this.cancel.Text = "退出";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // listView_cursymbol
            // 
            this.listView_cursymbol.Location = new System.Drawing.Point(12, 43);
            this.listView_cursymbol.MultiSelect = false;
            this.listView_cursymbol.Name = "listView_cursymbol";
            this.listView_cursymbol.Size = new System.Drawing.Size(380, 386);
            this.listView_cursymbol.TabIndex = 3;
            this.listView_cursymbol.UseCompatibleStateImageBehavior = false;
            this.listView_cursymbol.View = System.Windows.Forms.View.Tile;
            // 
            // comboBox_type
            // 
            this.comboBox_type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_type.FormattingEnabled = true;
            this.comboBox_type.Location = new System.Drawing.Point(83, 16);
            this.comboBox_type.Name = "comboBox_type";
            this.comboBox_type.Size = new System.Drawing.Size(82, 20);
            this.comboBox_type.TabIndex = 6;
            this.comboBox_type.SelectedIndexChanged += new System.EventHandler(this.comboBox_type_SelectedIndexChanged);
            // 
            // delete
            // 
            this.delete.Location = new System.Drawing.Point(220, 435);
            this.delete.Name = "delete";
            this.delete.Size = new System.Drawing.Size(73, 30);
            this.delete.TabIndex = 8;
            this.delete.Text = "删除";
            this.delete.UseVisualStyleBackColor = true;
            this.delete.Click += new System.EventHandler(this.delete_Click);
            // 
            // searchid
            // 
            this.searchid.Location = new System.Drawing.Point(222, 16);
            this.searchid.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.searchid.Name = "searchid";
            this.searchid.Size = new System.Drawing.Size(92, 21);
            this.searchid.TabIndex = 9;
            // 
            // search
            // 
            this.search.Location = new System.Drawing.Point(322, 14);
            this.search.Name = "search";
            this.search.Size = new System.Drawing.Size(69, 25);
            this.search.TabIndex = 10;
            this.search.Text = "查询";
            this.search.UseVisualStyleBackColor = true;
            this.search.Click += new System.EventHandler(this.search_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 11;
            this.label1.Text = "选择类别：";
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(124, 436);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(74, 28);
            this.ok.TabIndex = 12;
            this.ok.Text = "确定";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 442);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 13;
            this.label2.Text = "项数:";
            // 
            // count
            // 
            this.count.AutoSize = true;
            this.count.Location = new System.Drawing.Point(51, 441);
            this.count.Name = "count";
            this.count.Size = new System.Drawing.Size(11, 12);
            this.count.TabIndex = 14;
            this.count.Text = "0";
            // 
            // CurSymbolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 474);
            this.Controls.Add(this.count);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_type);
            this.Controls.Add(this.search);
            this.Controls.Add(this.searchid);
            this.Controls.Add(this.delete);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.listView_cursymbol);
            this.MaximizeBox = false;
            this.Name = "CurSymbolForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "符号管理器";
            ((System.ComponentModel.ISupportInitialize)(this.searchid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.ListView listView_cursymbol;
        private System.Windows.Forms.ComboBox comboBox_type;
        private System.Windows.Forms.Button delete;
        private System.Windows.Forms.NumericUpDown searchid;
        private System.Windows.Forms.Button search;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label count;
    }
}