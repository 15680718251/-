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
    /******************************************
     * 打开单图层的对话框
     * 李海欧 2012.7.23
     * ***************************************/
    public partial class openDlgShpfile : Form
    {
        public string filename;//文件的路径名
        public bool isOKbtn = false;//是否是确定按钮退出对话框
        //面积筛选条件，用于【合并特定小面积多边形】、【剔除特定小面积空洞】等
        public double FilterArea;

        public openDlgShpfile()
        {
            InitializeComponent();
        }

        //确定按钮
        private void button2_Click(object sender, EventArgs e)
        {
            if (!isValid())
            {
                MessageBox.Show("文件不合法！");
                return;
            }
            isOKbtn = true;
            FilterArea = Convert.ToDouble(txtbxAreaFilter.Text);
            this.Close();
        }

        //取消按钮
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.openFileDialog1.ShowDialog())
            {
                filename = this.openFileDialog1.FileName;
                this.textBox1.Text = filename;

            }
        }

        private void openDlgShpfile_Load(object sender, EventArgs e)
        {
            this.txtbxAreaFilter.Text = "10000";
            FilterArea = Convert.ToDouble(txtbxAreaFilter.Text);
        }

       //判断打开文件是否合法
        public bool isValid()
        {
            if (filename == null )
            {
                return false;
            }
            else
            {
                string extension = Path.GetExtension(filename);
                if (extension != ".shp")
                {
                    return false;
                }
            }
            return true;
        }
    }
}
