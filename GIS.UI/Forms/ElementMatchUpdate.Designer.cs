namespace GIS.UI.Forms
{
    partial class ElementMatchUpdate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ElementMatchUpdate));
            this.label1 = new System.Windows.Forms.Label();
            this.CloseBtn = new System.Windows.Forms.Button();
            this.UpdatepgBar = new System.Windows.Forms.ProgressBar();
            this.StartUpdateBtn = new System.Windows.Forms.Button();
            this.ParameterSetgBox = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.areaThresholdTBox = new System.Windows.Forms.TextBox();
            this.areaThresholdlabel = new System.Windows.Forms.Label();
            this.unite = new System.Windows.Forms.Label();
            this.radiusTB = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.LineUpdategBox = new System.Windows.Forms.GroupBox();
            this.soilArea = new System.Windows.Forms.Label();
            this.vegetationArea = new System.Windows.Forms.Label();
            this.residentialArea = new System.Windows.Forms.Label();
            this.waterArea = new System.Windows.Forms.Label();
            this.trafficArea = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SoilUpLabel = new System.Windows.Forms.Label();
            this.SoilLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.VegetationUpLabel = new System.Windows.Forms.Label();
            this.ResidentialUpLabel = new System.Windows.Forms.Label();
            this.WaterUpLabel = new System.Windows.Forms.Label();
            this.TrafficUpLabel = new System.Windows.Forms.Label();
            this.VegetationLabel = new System.Windows.Forms.Label();
            this.ResidentialLabel = new System.Windows.Forms.Label();
            this.WaterLabel = new System.Windows.Forms.Label();
            this.TrafficLabel = new System.Windows.Forms.Label();
            this.ParameterSetgBox.SuspendLayout();
            this.LineUpdategBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(482, 388);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 20);
            this.label1.TabIndex = 46;
            this.label1.Text = "100%";
            // 
            // CloseBtn
            // 
            this.CloseBtn.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CloseBtn.Location = new System.Drawing.Point(337, 349);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.Size = new System.Drawing.Size(84, 33);
            this.CloseBtn.TabIndex = 44;
            this.CloseBtn.Text = "退出";
            this.CloseBtn.UseVisualStyleBackColor = true;
            this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // UpdatepgBar
            // 
            this.UpdatepgBar.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.UpdatepgBar.ForeColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.UpdatepgBar.Location = new System.Drawing.Point(12, 388);
            this.UpdatepgBar.Name = "UpdatepgBar";
            this.UpdatepgBar.Size = new System.Drawing.Size(464, 17);
            this.UpdatepgBar.TabIndex = 45;
            // 
            // StartUpdateBtn
            // 
            this.StartUpdateBtn.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.StartUpdateBtn.Location = new System.Drawing.Point(104, 349);
            this.StartUpdateBtn.Name = "StartUpdateBtn";
            this.StartUpdateBtn.Size = new System.Drawing.Size(96, 33);
            this.StartUpdateBtn.TabIndex = 43;
            this.StartUpdateBtn.Text = "开始更新";
            this.StartUpdateBtn.UseVisualStyleBackColor = true;
            this.StartUpdateBtn.Click += new System.EventHandler(this.StartUpdateBtn_Click);
            // 
            // ParameterSetgBox
            // 
            this.ParameterSetgBox.Controls.Add(this.label7);
            this.ParameterSetgBox.Controls.Add(this.areaThresholdTBox);
            this.ParameterSetgBox.Controls.Add(this.areaThresholdlabel);
            this.ParameterSetgBox.Controls.Add(this.unite);
            this.ParameterSetgBox.Controls.Add(this.radiusTB);
            this.ParameterSetgBox.Controls.Add(this.label10);
            this.ParameterSetgBox.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ParameterSetgBox.Location = new System.Drawing.Point(12, 262);
            this.ParameterSetgBox.Name = "ParameterSetgBox";
            this.ParameterSetgBox.Size = new System.Drawing.Size(515, 81);
            this.ParameterSetgBox.TabIndex = 42;
            this.ParameterSetgBox.TabStop = false;
            this.ParameterSetgBox.Text = "更新参数设置";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(433, 41);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(22, 20);
            this.label7.TabIndex = 5;
            this.label7.Text = "m";
            // 
            // areaThresholdTBox
            // 
            this.areaThresholdTBox.BackColor = System.Drawing.SystemColors.MenuBar;
            this.areaThresholdTBox.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.areaThresholdTBox.Location = new System.Drawing.Point(369, 38);
            this.areaThresholdTBox.Name = "areaThresholdTBox";
            this.areaThresholdTBox.Size = new System.Drawing.Size(58, 23);
            this.areaThresholdTBox.TabIndex = 4;
            this.areaThresholdTBox.Text = "0.9";
            // 
            // areaThresholdlabel
            // 
            this.areaThresholdlabel.AutoSize = true;
            this.areaThresholdlabel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.areaThresholdlabel.Location = new System.Drawing.Point(273, 37);
            this.areaThresholdlabel.Name = "areaThresholdlabel";
            this.areaThresholdlabel.Size = new System.Drawing.Size(90, 21);
            this.areaThresholdlabel.TabIndex = 3;
            this.areaThresholdlabel.Text = "面积阈值：";
            // 
            // unite
            // 
            this.unite.AutoSize = true;
            this.unite.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.unite.Location = new System.Drawing.Point(178, 39);
            this.unite.Name = "unite";
            this.unite.Size = new System.Drawing.Size(22, 20);
            this.unite.TabIndex = 2;
            this.unite.Text = "m";
            // 
            // radiusTB
            // 
            this.radiusTB.BackColor = System.Drawing.SystemColors.MenuBar;
            this.radiusTB.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radiusTB.Location = new System.Drawing.Point(114, 38);
            this.radiusTB.Name = "radiusTB";
            this.radiusTB.Size = new System.Drawing.Size(58, 23);
            this.radiusTB.TabIndex = 1;
            this.radiusTB.Text = "28";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(29, 38);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(78, 21);
            this.label10.TabIndex = 0;
            this.label10.Text = "容差半径:";
            // 
            // LineUpdategBox
            // 
            this.LineUpdategBox.Controls.Add(this.soilArea);
            this.LineUpdategBox.Controls.Add(this.vegetationArea);
            this.LineUpdategBox.Controls.Add(this.residentialArea);
            this.LineUpdategBox.Controls.Add(this.waterArea);
            this.LineUpdategBox.Controls.Add(this.trafficArea);
            this.LineUpdategBox.Controls.Add(this.label6);
            this.LineUpdategBox.Controls.Add(this.SoilUpLabel);
            this.LineUpdategBox.Controls.Add(this.SoilLabel);
            this.LineUpdategBox.Controls.Add(this.label5);
            this.LineUpdategBox.Controls.Add(this.label4);
            this.LineUpdategBox.Controls.Add(this.label3);
            this.LineUpdategBox.Controls.Add(this.label2);
            this.LineUpdategBox.Controls.Add(this.VegetationUpLabel);
            this.LineUpdategBox.Controls.Add(this.ResidentialUpLabel);
            this.LineUpdategBox.Controls.Add(this.WaterUpLabel);
            this.LineUpdategBox.Controls.Add(this.TrafficUpLabel);
            this.LineUpdategBox.Controls.Add(this.VegetationLabel);
            this.LineUpdategBox.Controls.Add(this.ResidentialLabel);
            this.LineUpdategBox.Controls.Add(this.WaterLabel);
            this.LineUpdategBox.Controls.Add(this.TrafficLabel);
            this.LineUpdategBox.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LineUpdategBox.Location = new System.Drawing.Point(12, 12);
            this.LineUpdategBox.Name = "LineUpdategBox";
            this.LineUpdategBox.Size = new System.Drawing.Size(515, 237);
            this.LineUpdategBox.TabIndex = 41;
            this.LineUpdategBox.TabStop = false;
            this.LineUpdategBox.Text = "典型要素匹配更新";
            // 
            // soilArea
            // 
            this.soilArea.AutoSize = true;
            this.soilArea.Location = new System.Drawing.Point(273, 192);
            this.soilArea.Name = "soilArea";
            this.soilArea.Size = new System.Drawing.Size(15, 17);
            this.soilArea.TabIndex = 53;
            this.soilArea.Text = "0";
            // 
            // vegetationArea
            // 
            this.vegetationArea.AutoSize = true;
            this.vegetationArea.Location = new System.Drawing.Point(273, 158);
            this.vegetationArea.Name = "vegetationArea";
            this.vegetationArea.Size = new System.Drawing.Size(15, 17);
            this.vegetationArea.TabIndex = 52;
            this.vegetationArea.Text = "0";
            // 
            // residentialArea
            // 
            this.residentialArea.AutoSize = true;
            this.residentialArea.Location = new System.Drawing.Point(273, 123);
            this.residentialArea.Name = "residentialArea";
            this.residentialArea.Size = new System.Drawing.Size(15, 17);
            this.residentialArea.TabIndex = 51;
            this.residentialArea.Text = "0";
            // 
            // waterArea
            // 
            this.waterArea.AutoSize = true;
            this.waterArea.Location = new System.Drawing.Point(273, 91);
            this.waterArea.Name = "waterArea";
            this.waterArea.Size = new System.Drawing.Size(15, 17);
            this.waterArea.TabIndex = 50;
            this.waterArea.Text = "0";
            // 
            // trafficArea
            // 
            this.trafficArea.AutoSize = true;
            this.trafficArea.Location = new System.Drawing.Point(273, 54);
            this.trafficArea.Name = "trafficArea";
            this.trafficArea.Size = new System.Drawing.Size(15, 17);
            this.trafficArea.TabIndex = 49;
            this.trafficArea.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(395, 192);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 17);
            this.label6.TabIndex = 48;
            this.label6.Text = "基态/增量";
            // 
            // SoilUpLabel
            // 
            this.SoilUpLabel.AutoSize = true;
            this.SoilUpLabel.Location = new System.Drawing.Point(146, 192);
            this.SoilUpLabel.Name = "SoilUpLabel";
            this.SoilUpLabel.Size = new System.Drawing.Size(15, 17);
            this.SoilUpLabel.TabIndex = 47;
            this.SoilUpLabel.Text = "0";
            // 
            // SoilLabel
            // 
            this.SoilLabel.AutoSize = true;
            this.SoilLabel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.SoilLabel.Location = new System.Drawing.Point(68, 188);
            this.SoilLabel.Name = "SoilLabel";
            this.SoilLabel.Size = new System.Drawing.Size(58, 21);
            this.SoilLabel.TabIndex = 46;
            this.SoilLabel.Text = "土质：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(395, 158);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 17);
            this.label5.TabIndex = 45;
            this.label5.Text = "基态/增量";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(395, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 17);
            this.label4.TabIndex = 44;
            this.label4.Text = "基态/增量";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(395, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 17);
            this.label3.TabIndex = 43;
            this.label3.Text = "基态/增量";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(395, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 17);
            this.label2.TabIndex = 42;
            this.label2.Text = "基态/增量";
            // 
            // VegetationUpLabel
            // 
            this.VegetationUpLabel.AutoSize = true;
            this.VegetationUpLabel.Location = new System.Drawing.Point(146, 158);
            this.VegetationUpLabel.Name = "VegetationUpLabel";
            this.VegetationUpLabel.Size = new System.Drawing.Size(15, 17);
            this.VegetationUpLabel.TabIndex = 41;
            this.VegetationUpLabel.Text = "0";
            // 
            // ResidentialUpLabel
            // 
            this.ResidentialUpLabel.AutoSize = true;
            this.ResidentialUpLabel.Location = new System.Drawing.Point(146, 123);
            this.ResidentialUpLabel.Name = "ResidentialUpLabel";
            this.ResidentialUpLabel.Size = new System.Drawing.Size(15, 17);
            this.ResidentialUpLabel.TabIndex = 40;
            this.ResidentialUpLabel.Text = "0";
            // 
            // WaterUpLabel
            // 
            this.WaterUpLabel.AutoSize = true;
            this.WaterUpLabel.Location = new System.Drawing.Point(146, 91);
            this.WaterUpLabel.Name = "WaterUpLabel";
            this.WaterUpLabel.Size = new System.Drawing.Size(15, 17);
            this.WaterUpLabel.TabIndex = 39;
            this.WaterUpLabel.Text = "0";
            // 
            // TrafficUpLabel
            // 
            this.TrafficUpLabel.AutoSize = true;
            this.TrafficUpLabel.Location = new System.Drawing.Point(146, 54);
            this.TrafficUpLabel.Name = "TrafficUpLabel";
            this.TrafficUpLabel.Size = new System.Drawing.Size(15, 17);
            this.TrafficUpLabel.TabIndex = 38;
            this.TrafficUpLabel.Text = "0";
            // 
            // VegetationLabel
            // 
            this.VegetationLabel.AutoSize = true;
            this.VegetationLabel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.VegetationLabel.Location = new System.Drawing.Point(68, 154);
            this.VegetationLabel.Name = "VegetationLabel";
            this.VegetationLabel.Size = new System.Drawing.Size(58, 21);
            this.VegetationLabel.TabIndex = 37;
            this.VegetationLabel.Text = "植被：";
            // 
            // ResidentialLabel
            // 
            this.ResidentialLabel.AutoSize = true;
            this.ResidentialLabel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ResidentialLabel.Location = new System.Drawing.Point(52, 119);
            this.ResidentialLabel.Name = "ResidentialLabel";
            this.ResidentialLabel.Size = new System.Drawing.Size(74, 21);
            this.ResidentialLabel.TabIndex = 36;
            this.ResidentialLabel.Text = "居民地：";
            // 
            // WaterLabel
            // 
            this.WaterLabel.AutoSize = true;
            this.WaterLabel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.WaterLabel.Location = new System.Drawing.Point(68, 87);
            this.WaterLabel.Name = "WaterLabel";
            this.WaterLabel.Size = new System.Drawing.Size(58, 21);
            this.WaterLabel.TabIndex = 35;
            this.WaterLabel.Text = "水系：";
            // 
            // TrafficLabel
            // 
            this.TrafficLabel.AutoSize = true;
            this.TrafficLabel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TrafficLabel.Location = new System.Drawing.Point(68, 50);
            this.TrafficLabel.Name = "TrafficLabel";
            this.TrafficLabel.Size = new System.Drawing.Size(51, 21);
            this.TrafficLabel.TabIndex = 34;
            this.TrafficLabel.Text = "交通 :";
            // 
            // ElementMatchUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(542, 414);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.UpdatepgBar);
            this.Controls.Add(this.StartUpdateBtn);
            this.Controls.Add(this.ParameterSetgBox);
            this.Controls.Add(this.LineUpdategBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ElementMatchUpdate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ElementMatchUpdate";
            this.Load += new System.EventHandler(this.LineEleMatchUpdate_Load);
            this.ParameterSetgBox.ResumeLayout(false);
            this.ParameterSetgBox.PerformLayout();
            this.LineUpdategBox.ResumeLayout(false);
            this.LineUpdategBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button CloseBtn;
        private System.Windows.Forms.ProgressBar UpdatepgBar;
        private System.Windows.Forms.Button StartUpdateBtn;
        private System.Windows.Forms.GroupBox ParameterSetgBox;
        private System.Windows.Forms.Label unite;
        private System.Windows.Forms.TextBox radiusTB;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox LineUpdategBox;
        private System.Windows.Forms.Label VegetationUpLabel;
        private System.Windows.Forms.Label ResidentialUpLabel;
        private System.Windows.Forms.Label WaterUpLabel;
        private System.Windows.Forms.Label TrafficUpLabel;
        private System.Windows.Forms.Label VegetationLabel;
        private System.Windows.Forms.Label ResidentialLabel;
        private System.Windows.Forms.Label WaterLabel;
        private System.Windows.Forms.Label TrafficLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label SoilUpLabel;
        private System.Windows.Forms.Label SoilLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label soilArea;
        private System.Windows.Forms.Label vegetationArea;
        private System.Windows.Forms.Label residentialArea;
        private System.Windows.Forms.Label waterArea;
        private System.Windows.Forms.Label trafficArea;
        private System.Windows.Forms.Label areaThresholdlabel;
        private System.Windows.Forms.TextBox areaThresholdTBox;
        private System.Windows.Forms.Label label7;
    }
}