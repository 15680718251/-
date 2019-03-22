using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.TreeIndex;
using GIS.Map;
using GIS.Utilities;

namespace GIS.TreeIndex.Vectorize
{
    public partial class FrmVectorizePara : Form
    {
        private string[] strRasters;
        private string strExportfolderName;
        private MapUI mapUI;
        BackgroundWorker bw;
        private Dictionary<short, string> m_currentDics = new Dictionary<short, string>();
        frmVectorizeProgress m_fmProgress;

        public FrmVectorizePara(MapUI map)
        {
            InitializeComponent();
            this.mapUI = map;

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            
            bw.DoWork += new DoWorkEventHandler(VectorizeWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            Dictionary<string, string> dics = GIS.Utilities.ConfigHelper.GetAppConfigs();
            int count = dics.Count;

            foreach (KeyValuePair<string, string> item in dics)
            {
                int index = dataGridView1.Rows.Add();
                this.dataGridView1.Rows[index].Cells[0].Value = item.Key;
                this.dataGridView1.Rows[index].Cells[1].Value = item.Value;
            }
        }

        //指定输入栅格文件
        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "影像文件|*.img;*.IMG;*.tif;*.TIF;*.jpg;*.JPEG;*.jpeg;*.JPEG;*.dem;*.adf;*.DEM;*.gif;*.GIF;*.bmp;*.BMP;*.png;*.PNG;*.jp2;*.JP2;*.j2k;*.J2K";
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    strRasters = dlg.FileNames;
                    string folderPath = System.IO.Path.GetDirectoryName(strRasters[0]);
                    this.txtRaster.Text = folderPath + "||" + strRasters.Length + "个文件";

                    mapUI.OutPutTextInfo("导入了" + strRasters.Length + "个栅格文件\r\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        //矢量文件存放目录
        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                strExportfolderName = dlg.SelectedPath;
                txtShp.Text = strExportfolderName;
            }
        }

        //开始处理
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (txtRaster.Text.Trim() == "" || txtShp.Text == "")
            {
                MessageBox.Show("必须指定有效路径","提示");
                return;
            }

            //处理前先获取DataGridView中的状态
            int count = this.dataGridView1.Rows.Count;
            int selCount = 0; //选择矢量化类型的个数
            for (int i = 0; i < count; i++)
            {
                object value = this.dataGridView1.Rows[i].Cells[2].Value;
                if (value != null)
                {
                    selCount++;
                    short str1 = Convert.ToInt16(this.dataGridView1.Rows[i].Cells[0].Value);
                    string str2 = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                    m_currentDics.Add(str1,str2);
                }
            }

            if (selCount == 0)
            {
                MessageBox.Show("必须选择矢量化目标类型","提示");
                return;
            }

            this.Hide();

            m_fmProgress = new frmVectorizeProgress();
            bw.RunWorkerAsync();
            m_fmProgress.ShowDialog(this);
            this.Close();
            m_fmProgress.Dispose();
        }

        //主要的后台工作
        private void VectorizeWork(object sender,DoWorkEventArgs e)
        {
            int count = strRasters.Length;
            for (int i = 0; i < count; i++)
            {
                if (m_fmProgress.Cancel)
                    return;

                ObjectVec vec = new ObjectVec(strRasters[i], strExportfolderName,m_currentDics,bw,m_fmProgress,e,count);
                vec.RaToVe();//矢量化
                vec.Dispose();//释放托管资源
            }
        }

        private void bw_RunWorkerCompleted(object sender,RunWorkerCompletedEventArgs e)
        {

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                return;
            }

            if (e.Cancelled)
            {
                MessageBox.Show("取消操作","提示");
                m_fmProgress.Dispose();
                return;
            }

            //m_fmProgress.progressBar1.Invoke(
            //(MethodInvoker)delegate()
            //{
            //    m_fmProgress.progressBar1.Value = 1000;
            //}
            //);

            m_fmProgress.progressBar1.Value = 1000;

            mapUI.OutPutTextInfo("矢量化完成");
            m_fmProgress.lblDescription.Text = "矢量化完成";
            m_fmProgress.btnCancel.Text = "确定";
            m_fmProgress.Text = "执行完成";

            if (m_fmProgress.checkBox1.Checked)
                m_fmProgress.Dispose();
        }

        //全选
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right&&e.ColumnIndex==2)
            {
                contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
            }
        }

        //全选或全不选
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.toolStripMenuItem1.Checked = !this.toolStripMenuItem1.Checked;
            int count = this.dataGridView1.Rows.Count;

            if (this.toolStripMenuItem1.Checked == true)
            {
                for (int i = 0; i < count; i++)
                {
                    this.dataGridView1.Rows[i].Cells[2].Value = true;
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    this.dataGridView1.Rows[i].Cells[2].Value = null;
                }
            }

            this.btnCancel.Focus();//datagridview失去焦点也不行，否则datagridview当前选择行有问题
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}