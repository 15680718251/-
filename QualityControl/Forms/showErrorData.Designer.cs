namespace QualityControl.Forms
{
    partial class showErrorData
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
            this.errorDataGV = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.errorDataGV)).BeginInit();
            this.SuspendLayout();
            // 
            // errorDataGV
            // 
            this.errorDataGV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.errorDataGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.errorDataGV.Location = new System.Drawing.Point(12, 12);
            this.errorDataGV.Name = "errorDataGV";
            this.errorDataGV.RowTemplate.Height = 23;
            this.errorDataGV.Size = new System.Drawing.Size(516, 322);
            this.errorDataGV.TabIndex = 0;
            this.errorDataGV.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.showFeatrue_headerMouseClick);
            // 
            // showErrorData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 346);
            this.Controls.Add(this.errorDataGV);
            this.Name = "showErrorData";
            this.Text = "showErrorData";
            this.Load += new System.EventHandler(this.showErrorData_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorDataGV)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView errorDataGV;
    }
}