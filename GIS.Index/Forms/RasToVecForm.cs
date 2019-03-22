using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.TreeIndex;

namespace GIS.TreeIndex.Forms
{
    public partial class RasToVecForm : Form
    {
        public string[] strRasterPathNames;
        public string strDEMFilePathName;
        public string strExportfolderName;
        public double Tolerance;
        public Dictionary<string, string> dics = null;

        public RasToVecForm()
        {
            InitializeComponent();
            groupBox1.Enabled = false;
            groupBox4.Enabled = false;
            numericUpDown1.Value = 9;
            dics = GIS.Utilities.ConfigHelper.GetAppConfigs();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenRasterFile();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenDEMFile();
            textBox3.Text = strDEMFilePathName;
        }

        public void OpenRasterFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "影像文件|*.img;*.IMG;*.tif;*.TIF;*.jpg;*.JPEG;*.jpeg;*.JPEG;*.dem;*.adf;*.DEM;*.gif;*.GIF;*.bmp;*.BMP;*.png;*.PNG;*.jp2;*.JP2;*.j2k;*.J2K";
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    strRasterPathNames = dlg.FileNames;
                    //foreach (string strFileName in strFileNames)
                    //{
                    //    strRasterPathNames = strFileName;
                    //}
                    //OutPutTextInfo("打开栅格文件成功:\r\n");

                    string str = Path.GetDirectoryName(strRasterPathNames[0]);
                    int FileCount = strRasterPathNames.Count();
                    textBox1.Text = str + " ||打开" + FileCount + "个文件";
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        public void OpenDEMFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "影像文件|*.img;*.IMG;*.tif;*.TIF;*.jpg;*.JPEG;*.jpeg;*.JPEG;*.dem;*.adf;*.DEM;*.gif;*.GIF;*.bmp;*.BMP;*.png;*.PNG;*.jp2;*.JP2;*.j2k;*.J2K";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] strFileNames = dlg.FileNames;
                    foreach (string strFileName in strFileNames)
                    {
                        strDEMFilePathName = strFileName;
                    }
                    //OutPutTextInfo("打开栅格文件成功:\r\n");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                groupBox1.Enabled = true;
            else
                groupBox1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                strExportfolderName = dlg.SelectedPath;
                textBox2.Text = strExportfolderName;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                groupBox4.Enabled = true;
            else
                groupBox4.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (groupBox4.Enabled)
                Tolerance = Convert.ToDouble(this.numericUpDown1.Value);
            else
                Tolerance = -1;
        }
    }
}
