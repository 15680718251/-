namespace GIS.UI.Forms
{
    partial class OSMDataBaseLinkForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DataBase = new System.Windows.Forms.ComboBox();
            this.Password = new System.Windows.Forms.TextBox();
            this.User = new System.Windows.Forms.TextBox();
            this.Server = new System.Windows.Forms.TextBox();
            this.keepPasswordCkBox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DataBase);
            this.groupBox1.Controls.Add(this.Password);
            this.groupBox1.Controls.Add(this.User);
            this.groupBox1.Controls.Add(this.Server);
            this.groupBox1.Controls.Add(this.keepPasswordCkBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(31, 28);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(247, 237);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "设置参数";
            // 
            // DataBase
            // 
            this.DataBase.FormattingEnabled = true;
            this.DataBase.Location = new System.Drawing.Point(98, 136);
            this.DataBase.Name = "DataBase";
            this.DataBase.Size = new System.Drawing.Size(129, 20);
            this.DataBase.TabIndex = 10;
            this.DataBase.Text = "orcl";
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(98, 102);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(129, 21);
            this.Password.TabIndex = 9;
            this.Password.Text = "123456";
            // 
            // User
            // 
            this.User.Location = new System.Drawing.Point(98, 67);
            this.User.Name = "User";
            this.User.Size = new System.Drawing.Size(129, 21);
            this.User.TabIndex = 8;
            this.User.Text = "system";
            // 
            // Server
            // 
            this.Server.Location = new System.Drawing.Point(98, 26);
            this.Server.Name = "Server";
            this.Server.Size = new System.Drawing.Size(129, 21);
            this.Server.TabIndex = 6;
            this.Server.Text = "192.168.1.93";
            // 
            // keepPasswordCkBox
            // 
            this.keepPasswordCkBox.AutoSize = true;
            this.keepPasswordCkBox.Location = new System.Drawing.Point(98, 191);
            this.keepPasswordCkBox.Name = "keepPasswordCkBox";
            this.keepPasswordCkBox.Size = new System.Drawing.Size(72, 16);
            this.keepPasswordCkBox.TabIndex = 5;
            this.keepPasswordCkBox.Text = "记住密码";
            this.keepPasswordCkBox.UseVisualStyleBackColor = true;
            this.keepPasswordCkBox.CheckedChanged += new System.EventHandler(this.keepPasswordCkBox_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 139);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "OSM数据库名：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(39, 105);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "密码：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(39, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "用户名：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器名：";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(42, 291);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "取消";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(183, 291);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "确定";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // OSMDataBaseLinkForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 342);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Name = "OSMDataBaseLinkForm";
            this.Text = "OSM数据库参数设置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox DataBase;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.TextBox User;
        private System.Windows.Forms.TextBox Server;
        private System.Windows.Forms.CheckBox keepPasswordCkBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}