namespace QualityControl.Forms
{
    partial class TopoControlForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.cbb_layer1 = new System.Windows.Forms.ComboBox();
            this.cbb_layer2 = new System.Windows.Forms.ComboBox();
            this.bt_confirm = new System.Windows.Forms.Button();
            this.probar_topo = new System.Windows.Forms.ProgressBar();
            this.resultBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(128, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "图层1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(127, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "图层2";
            // 
            // cbb_layer1
            // 
            this.cbb_layer1.FormattingEnabled = true;
            this.cbb_layer1.Location = new System.Drawing.Point(198, 46);
            this.cbb_layer1.Name = "cbb_layer1";
            this.cbb_layer1.Size = new System.Drawing.Size(225, 20);
            this.cbb_layer1.TabIndex = 2;
            // 
            // cbb_layer2
            // 
            this.cbb_layer2.FormattingEnabled = true;
            this.cbb_layer2.Location = new System.Drawing.Point(198, 113);
            this.cbb_layer2.Name = "cbb_layer2";
            this.cbb_layer2.Size = new System.Drawing.Size(225, 20);
            this.cbb_layer2.TabIndex = 3;
            // 
            // bt_confirm
            // 
            this.bt_confirm.Location = new System.Drawing.Point(198, 315);
            this.bt_confirm.Name = "bt_confirm";
            this.bt_confirm.Size = new System.Drawing.Size(122, 23);
            this.bt_confirm.TabIndex = 5;
            this.bt_confirm.Text = "确认";
            this.bt_confirm.UseVisualStyleBackColor = true;
            this.bt_confirm.Click += new System.EventHandler(this.bt_confirm_Click);
            // 
            // probar_topo
            // 
            this.probar_topo.Location = new System.Drawing.Point(-1, 357);
            this.probar_topo.Name = "probar_topo";
            this.probar_topo.Size = new System.Drawing.Size(518, 23);
            this.probar_topo.TabIndex = 6;
            // 
            // resultBox
            // 
            this.resultBox.Location = new System.Drawing.Point(71, 169);
            this.resultBox.Multiline = true;
            this.resultBox.Name = "resultBox";
            this.resultBox.ReadOnly = true;
            this.resultBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.resultBox.Size = new System.Drawing.Size(397, 125);
            this.resultBox.TabIndex = 7;
            // 
            // TopoControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 380);
            this.Controls.Add(this.resultBox);
            this.Controls.Add(this.probar_topo);
            this.Controls.Add(this.bt_confirm);
            this.Controls.Add(this.cbb_layer2);
            this.Controls.Add(this.cbb_layer1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "TopoControlForm";
            this.Text = "TopoControlForm";
            this.Load += new System.EventHandler(this.TopoControlForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbb_layer1;
        private System.Windows.Forms.ComboBox cbb_layer2;
        private System.Windows.Forms.Button bt_confirm;
        private System.Windows.Forms.ProgressBar probar_topo;
        private System.Windows.Forms.TextBox resultBox;
    }
}