namespace GISEditor.EditTool.Tool.UC
{
    partial class frmAttributeEdit
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
            this.gridViewAttribute = new System.Windows.Forms.DataGridView();
            this.tvLayer = new System.Windows.Forms.TreeView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewAttribute)).BeginInit();
            this.SuspendLayout();
            // 
            // gridViewAttribute
            // 
            this.gridViewAttribute.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridViewAttribute.Location = new System.Drawing.Point(171, 12);
            this.gridViewAttribute.Name = "gridViewAttribute";
            this.gridViewAttribute.RowTemplate.Height = 23;
            this.gridViewAttribute.Size = new System.Drawing.Size(359, 297);
            this.gridViewAttribute.TabIndex = 0;
            // 
            // tvLayer
            // 
            this.tvLayer.Location = new System.Drawing.Point(12, 23);
            this.tvLayer.Name = "tvLayer";
            this.tvLayer.Size = new System.Drawing.Size(153, 286);
            this.tvLayer.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(12, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(153, 306);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "图层名称";
            // 
            // frmAttributeEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 319);
            this.Controls.Add(this.tvLayer);
            this.Controls.Add(this.gridViewAttribute);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmAttributeEdit";
            this.Text = "属性编辑";
            ((System.ComponentModel.ISupportInitialize)(this.gridViewAttribute)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gridViewAttribute;
        private System.Windows.Forms.TreeView tvLayer;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}