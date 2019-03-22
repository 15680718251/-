using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.VectorMerge
{
    public partial class FrmVectorMerge : Form
    {
        string[] strs;
        BackgroundWorker bw;
        frmVectorMergeProgress m_fmProgress;

        public FrmVectorMerge()
        {
            InitializeComponent();

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;

            bw.DoWork += new DoWorkEventHandler(MergeWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "矢量文件|*.shp";
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    strs = dlg.FileNames;
                    this.listBox1.DataSource = strs;
                    
                    string folderPath = System.IO.Path.GetDirectoryName(strs[0]);
                    this.textBox1.Text = folderPath + "||" + strs.Length + "个文件";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("", "提示");
                return;
            }

            this.Hide();

            m_fmProgress = new frmVectorMergeProgress();
            bw.RunWorkerAsync();
            m_fmProgress.ShowDialog(this);

            this.Close();
            m_fmProgress.Dispose();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MergeWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                HandleShpsMerge hsm = new HandleShpsMerge(strs, m_fmProgress);
                hsm.Merge();
                hsm.Dispose();
            }
            catch (Exception ex)
            {
                throw (new ApplicationException("矢量文件合并出错"));
            }
            finally
            {
                //m_fmProgress.Dispose();
            }
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
    }
}
