namespace GIS.UI.Forms
{
    partial class AreaElementUpdate
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
            this.AreaEleUpdateGBox = new System.Windows.Forms.GroupBox();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.startUpdateBtn = new System.Windows.Forms.Button();
            this.paraSetGBox = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.areaThresholdTBox = new System.Windows.Forms.TextBox();
            this.areaThresholdlabel = new System.Windows.Forms.Label();
            this.areaEleIncreDataCBox = new System.Windows.Forms.ComboBox();
            this.areaEleBaseDataCBox = new System.Windows.Forms.ComboBox();
            this.AreaEleIncreDatalabel = new System.Windows.Forms.Label();
            this.AreaEleDataBaselabel = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.AreaEleUpdateGBox.SuspendLayout();
            this.paraSetGBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // AreaEleUpdateGBox
            // 
            this.AreaEleUpdateGBox.Controls.Add(this.cancelBtn);
            this.AreaEleUpdateGBox.Controls.Add(this.startUpdateBtn);
            this.AreaEleUpdateGBox.Controls.Add(this.paraSetGBox);
            this.AreaEleUpdateGBox.Controls.Add(this.areaEleIncreDataCBox);
            this.AreaEleUpdateGBox.Controls.Add(this.areaEleBaseDataCBox);
            this.AreaEleUpdateGBox.Controls.Add(this.AreaEleIncreDatalabel);
            this.AreaEleUpdateGBox.Controls.Add(this.AreaEleDataBaselabel);
            this.AreaEleUpdateGBox.Location = new System.Drawing.Point(12, 12);
            this.AreaEleUpdateGBox.Name = "AreaEleUpdateGBox";
            this.AreaEleUpdateGBox.Size = new System.Drawing.Size(354, 267);
            this.AreaEleUpdateGBox.TabIndex = 0;
            this.AreaEleUpdateGBox.TabStop = false;
            //this.AreaEleUpdateGBox.Enter += new System.EventHandler(this.AreaEleUpdateGBox_Enter);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(202, 234);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 10;
            this.cancelBtn.Text = "取消";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // startUpdateBtn
            // 
            this.startUpdateBtn.Location = new System.Drawing.Point(33, 234);
            this.startUpdateBtn.Name = "startUpdateBtn";
            this.startUpdateBtn.Size = new System.Drawing.Size(95, 23);
            this.startUpdateBtn.TabIndex = 9;
            this.startUpdateBtn.Text = "开始匹配更新";
            this.startUpdateBtn.UseVisualStyleBackColor = true;
            this.startUpdateBtn.Click += new System.EventHandler(this.startUpdateBtn_Click);
            // 
            // paraSetGBox
            // 
            this.paraSetGBox.Controls.Add(this.label3);
            this.paraSetGBox.Controls.Add(this.areaThresholdTBox);
            this.paraSetGBox.Controls.Add(this.areaThresholdlabel);
            this.paraSetGBox.Location = new System.Drawing.Point(12, 125);
            this.paraSetGBox.Name = "paraSetGBox";
            this.paraSetGBox.Size = new System.Drawing.Size(328, 94);
            this.paraSetGBox.TabIndex = 8;
            this.paraSetGBox.TabStop = false;
            this.paraSetGBox.Text = "参数设置：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(271, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(11, 12);
            this.label3.TabIndex = 14;
            this.label3.Text = "m";
            // 
            // areaThresholdTBox
            // 
            this.areaThresholdTBox.Location = new System.Drawing.Point(137, 41);
            this.areaThresholdTBox.Name = "areaThresholdTBox";
            this.areaThresholdTBox.Size = new System.Drawing.Size(121, 21);
            this.areaThresholdTBox.TabIndex = 12;
            // 
            // areaThresholdlabel
            // 
            this.areaThresholdlabel.AutoSize = true;
            this.areaThresholdlabel.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.areaThresholdlabel.Location = new System.Drawing.Point(54, 42);
            this.areaThresholdlabel.Name = "areaThresholdlabel";
            this.areaThresholdlabel.Size = new System.Drawing.Size(77, 14);
            this.areaThresholdlabel.TabIndex = 11;
            this.areaThresholdlabel.Text = "面积阈值：";
            // 
            // areaEleIncreDataCBox
            // 
            this.areaEleIncreDataCBox.FormattingEnabled = true;
            this.areaEleIncreDataCBox.Location = new System.Drawing.Point(119, 84);
            this.areaEleIncreDataCBox.Name = "areaEleIncreDataCBox";
            this.areaEleIncreDataCBox.Size = new System.Drawing.Size(221, 20);
            this.areaEleIncreDataCBox.TabIndex = 7;
            // 
            // areaEleBaseDataCBox
            // 
            this.areaEleBaseDataCBox.FormattingEnabled = true;
            this.areaEleBaseDataCBox.Location = new System.Drawing.Point(119, 37);
            this.areaEleBaseDataCBox.Name = "areaEleBaseDataCBox";
            this.areaEleBaseDataCBox.Size = new System.Drawing.Size(221, 20);
            this.areaEleBaseDataCBox.TabIndex = 6;
            // 
            // AreaEleIncreDatalabel
            // 
            this.AreaEleIncreDatalabel.AutoSize = true;
            this.AreaEleIncreDatalabel.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.AreaEleIncreDatalabel.Location = new System.Drawing.Point(9, 85);
            this.AreaEleIncreDatalabel.Name = "AreaEleIncreDatalabel";
            this.AreaEleIncreDatalabel.Size = new System.Drawing.Size(119, 14);
            this.AreaEleIncreDatalabel.TabIndex = 5;
            this.AreaEleIncreDatalabel.Text = "面要素增量数据：";
            // 
            // AreaEleDataBaselabel
            // 
            this.AreaEleDataBaselabel.AutoSize = true;
            this.AreaEleDataBaselabel.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.AreaEleDataBaselabel.Location = new System.Drawing.Point(9, 38);
            this.AreaEleDataBaselabel.Name = "AreaEleDataBaselabel";
            this.AreaEleDataBaselabel.Size = new System.Drawing.Size(119, 14);
            this.AreaEleDataBaselabel.TabIndex = 4;
            this.AreaEleDataBaselabel.Text = "面要素基态数据：";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(-1, 295);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(378, 15);
            this.progressBar1.TabIndex = 1;
            // 
            // AreaElementUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 308);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.AreaEleUpdateGBox);
            this.Name = "AreaElementUpdate";
            this.Text = "面要素匹配更新";
            this.Load += new System.EventHandler(this.AreaElementUpdate_Load);
            this.AreaEleUpdateGBox.ResumeLayout(false);
            this.AreaEleUpdateGBox.PerformLayout();
            this.paraSetGBox.ResumeLayout(false);
            this.paraSetGBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox AreaEleUpdateGBox;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button startUpdateBtn;
        private System.Windows.Forms.GroupBox paraSetGBox;
        private System.Windows.Forms.TextBox areaThresholdTBox;
        private System.Windows.Forms.Label areaThresholdlabel;
        private System.Windows.Forms.ComboBox areaEleIncreDataCBox;
        private System.Windows.Forms.ComboBox areaEleBaseDataCBox;
        private System.Windows.Forms.Label AreaEleIncreDatalabel;
        private System.Windows.Forms.Label AreaEleDataBaselabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}