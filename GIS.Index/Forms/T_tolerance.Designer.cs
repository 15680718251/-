namespace GIS.TreeIndex.Forms
{
    partial class T_tolerance
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
            this.T_num = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.T_num)).BeginInit();
            this.SuspendLayout();
            // 
            // T_num
            // 
            this.T_num.DecimalPlaces = 2;
            this.T_num.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.T_num.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.T_num.Location = new System.Drawing.Point(102, 31);
            this.T_num.Name = "T_num";
            this.T_num.Size = new System.Drawing.Size(76, 23);
            this.T_num.TabIndex = 0;
            this.T_num.Value = new decimal(new int[] {
            15,
            0,
            0,
            131072});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(41, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "公差：";
            // 
            // button_OK
            // 
            this.button_OK.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_OK.Location = new System.Drawing.Point(238, 31);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(68, 26);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // T_tolerance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 98);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.T_num);
            this.Name = "T_tolerance";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "容差设置";
            ((System.ComponentModel.ISupportInitialize)(this.T_num)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown T_num;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OK;
    }
}