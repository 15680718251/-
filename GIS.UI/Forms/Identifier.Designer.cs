namespace GIS.UI.Forms
{
    partial class Identifier
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAttribute)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridAttribute
            // 
            this.dataGridAttribute.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridAttribute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridAttribute.Location = new System.Drawing.Point(0, 0);
            this.dataGridAttribute.Name = "dataGridAttribute";
            this.dataGridAttribute.RowTemplate.Height = 23;
            this.dataGridAttribute.Size = new System.Drawing.Size(607, 455);
            this.dataGridAttribute.TabIndex = 0;
            // 
            // Identifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 455);
            this.Controls.Add(this.dataGridAttribute);
            this.Name = "Identifier";
            this.Text = "Identifier";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAttribute)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridAttribute;
    }
}