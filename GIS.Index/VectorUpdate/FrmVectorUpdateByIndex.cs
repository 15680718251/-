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
using GIS.Layer;
using GIS.Map;
using GIS.TreeIndex;

namespace GIS.TreeIndex.VectorUpdate
{
    public partial class FrmVectorUpdateByIndex : Form
    {
        public string baslyr;//底图图层
        public string inclyr;//增量图层
        MapUI m_map;
        public double FilterArea = 10000.0;

        BackgroundWorker bw;
        frmVectorUpdateProgress m_fmProgress;

        public FrmVectorUpdateByIndex(MapUI map)
        {
            InitializeComponent();
            m_map = map;
            comboBox1.BeginUpdate();
            comboBox2.BeginUpdate(); 
            for (int i = 2; i < m_map.LayerCounts; i++)//前两层为系统图层
            {
                GeoVectorLayer lyr = m_map.GetLayerAt(i) as GeoVectorLayer;
                if (lyr != null )
                {
                    comboBox1.Items.Add(lyr.LayerName.ToString());
                    comboBox2.Items.Add(lyr.LayerName.ToString());
                }
            }
            comboBox1.EndUpdate();
            comboBox2.EndUpdate();

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;

            bw.DoWork += new DoWorkEventHandler(UpdateWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        }


        //判断图层
        public bool isValid()
        {
            if (comboBox1.Items.Count == 0 || comboBox2.Items.Count == 0)
            {
                return false;
            }
            return true;
        }

        //确定
        private void button3_Click(object sender, EventArgs e)
        {
            if (!isValid())
            {
                MessageBox.Show("请选择更新图层！", "提示");
                return;
            }
            string area = this.txtbxAreaFilter.Text.Trim();
            if (area != "")
            {
                Double.TryParse(area,out FilterArea);
            }
            baslyr = comboBox1.SelectedItem.ToString();
            inclyr = comboBox2.SelectedItem.ToString();
            this.Hide();

            m_fmProgress = new frmVectorUpdateProgress();
            bw.RunWorkerAsync();
            m_fmProgress.ShowDialog(this);

            this.Close();
            m_fmProgress.Dispose();

        }

        private void UpdateWork(object sender, DoWorkEventArgs e)
        {
            GIS.TreeIndex.VectorUpdate.HandleVectorUpdateByIndex hvu = new GIS.TreeIndex.VectorUpdate.HandleVectorUpdateByIndex(baslyr, inclyr, FilterArea,m_fmProgress,m_map);
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
