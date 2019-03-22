using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GIS.TreeIndex.Forms
{
    public partial class FrmHandleMutipolygons : Form
    {
        private string[] strPathNames;
        public FrmHandleMutipolygons()
        {
            InitializeComponent();
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
                    strPathNames = dlg.FileNames;

                    foreach (string str in strPathNames)
                    {
                        this.listView1.Items.Add(str);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("请选择矢量文件", "提示");
                return;
            }

            foreach (string str in strPathNames)
            {
                //ThreadPool.QueueUserWorkItem(dowork,new object[]{str});

                GIS.HandleShpsAndImage.HandleMutiPolygons hmp = new GIS.HandleShpsAndImage.HandleMutiPolygons(str);
                hmp.HandleWork();
                hmp.Dispose();
            }
            MessageBox.Show("处理完成","提示");
            this.Close();
        }

        //private void dowork(object parameter)
        //{

        //    object[] parameters = (object[])parameter;
        //    string str = parameters[0].ToString();

        //    try
        //    {
        //        GIS.HandleShpsAndImage.HandleMutiPolygons hmp = new GIS.HandleShpsAndImage.HandleMutiPolygons(str);
        //        hmp.HandleWork();

        //    }
        //    catch (Exception ex)
        //    {
        //        //m_MapUI.OutPutTextInfo(ex.ToString());
        //    }
        //    finally
        //    {
        //        //autoResetEvent.Set();
        //    }
        //}

        private void btnCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
