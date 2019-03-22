using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GIS.TreeIndex.Forms
{
    public partial class OpenDlg2Shpfiles : Form
    {
        /******************************************
        * 打开2个图层的对话框
        * 李海欧 2012.7.23
        * ***************************************/
        public OpenDlg2Shpfiles()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.openFileDialog1.ShowDialog())
            {
                srcFilename = this.openFileDialog1.FileName;
                this.textBox1.Text = srcFilename;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.openFileDialog1.ShowDialog())
            {
                incrFilename = this.openFileDialog1.FileName;
                this.textBox2.Text = incrFilename;
            }
        }

        //确定按钮
        private void button3_Click(object sender, EventArgs e)
        {
            if (!isValid())
            {
                MessageBox.Show("文件不合法！","提示");
                return;
            }
            isOKbtn = true;
            FilterArea = Convert.ToDouble(txtbxAreaFilter.Text);
            this.Close();
        }

        //取消按钮
        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OpenDlg2Shpfiles_Load(object sender, EventArgs e)
        {
            this.txtbxAreaFilter.Text = "10000";
            FilterArea = Convert.ToDouble(txtbxAreaFilter.Text);
        }

        public string srcFilename;//底图图层的文件路径名
        public string incrFilename;//增量图层的文件路径名
        public string outFilename; //输出的文件路径名
        public bool isOKbtn = false;//是否是确定按钮退出对话框
        public double FilterArea;// //面积筛选条件，用于【合并特定小面积多边形】
        //判断文件是否合法
        public bool isValid()
        {
            if (srcFilename == null || incrFilename == null)
            {
                return false;
            }
            else
            {
                string extension = Path.GetExtension(srcFilename);
                if (extension != ".shp")
                {
                    return false;
                }
                extension = Path.GetExtension(incrFilename);
                if (extension != ".shp")
                {
                    return false;
                }
                if (this.textBox3.Text.Trim() == "")
                {
                    return false;
                }
            }
            return true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (DialogResult.OK == sfd.ShowDialog())
            {
                if (sfd.FileName.EndsWith(".shp"))
                {
                    outFilename = sfd.FileName;
                }
                else
                {
                    outFilename = sfd.FileName + ".shp";
                }
                this.textBox3.Text = outFilename;
            }
        }
    }
}
