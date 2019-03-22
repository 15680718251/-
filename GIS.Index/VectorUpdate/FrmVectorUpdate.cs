using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GIS.TreeIndex.VectorUpdate
{
    public partial class FrmVectorUpdate : Form
    {
        public string srcFilename;//底图图层的文件路径名
        public string incrFilename;//增量图层的文件路径名
        public double FilterArea = 10000.0;

        BackgroundWorker bw;
        frmVectorUpdateProgress m_fmProgress;

        public FrmVectorUpdate()
        {
            InitializeComponent();

            openFileDialog1.Filter = "矢量文件|*.shp";
            openFileDialog1.Multiselect = false;

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;

            bw.DoWork += new DoWorkEventHandler(UpdateWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
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
            }
            return true;
        }

        //确定
        private void button3_Click(object sender, EventArgs e)
        {
            if (!isValid())
            {
                MessageBox.Show("文件不合法！", "提示");
                return;
            }

            string area = this.txtbxAreaFilter.Text.Trim();
            if (area != "")
            {
                Double.TryParse(area,out FilterArea);
            }

            this.Hide();

            m_fmProgress = new frmVectorUpdateProgress();
            bw.RunWorkerAsync();
            m_fmProgress.ShowDialog(this);

            this.Close();
            m_fmProgress.Dispose();

        }

        private void UpdateWork(object sender, DoWorkEventArgs e)
        {
            GIS.TreeIndex.VectorUpdate.HandleVectorUpdate hvu = new GIS.TreeIndex.VectorUpdate.HandleVectorUpdate(srcFilename, incrFilename, FilterArea,m_fmProgress);
            hvu.DoWork();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                return;
            }

            m_fmProgress.progressBar1.Value = 100; //maxvalue位1000
            m_fmProgress.btnOK.Visible = true;
            //mapUI.OutPutTextInfo("矢量更新化完成");
            m_fmProgress.Text = "执行完成";
            m_fmProgress.lblDescription.Text = "更新完成";

            if (m_fmProgress.checkBox1.Checked)
                m_fmProgress.Dispose();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
