using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using GIS.UI.AdditionalTool;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesGDB;
using GIS.UI.Forms;
using ESRI.ArcGIS.GlobeCore;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.ADF.BaseClasses;



namespace GIS.UI.Forms
{
    public partial class hist : Form
    {
        public hist()
        {
            InitializeComponent();

        }
        private AxMapControl AxMapcontrol;
      
        private AxTOCControl axTOCControl;
        public AxTOCControl AxTOCControl
        {
            get { return axTOCControl; }
            set { axTOCControl = value; }
        }
        public hist(AxMapControl axmaocontrol)
        {
            InitializeComponent();
            this.AxMapcontrol = axmaocontrol;
        }
        private void FeatureLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles(@"..\..\..\testfile", "*.shp");
            List<string> fileNameWithoutExtensions = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(files[i]);// 没有扩展名的文件名 
                fileNameWithoutExtensions.Add(fileNameWithoutExtension);
            }
            //如果已经生成shp数据则直接返回
            if (fileNameWithoutExtensions.Contains(comBox.Text))
            {
                return;
            }
            //否则生成该shp数据
            else
            {
                OracleDBHelper helper = new OracleDBHelper();
                ShpHelper.GetShpFileByTableName(helper, comBox.Text, @"..\..\..\testfile");
            }


        }

        private void hist_Load(object sender, EventArgs e)
        {
            List<string> areaTableNames = getAreaTableNames();
            this.comBox.Items.Clear();
            if (areaTableNames != null && areaTableNames.Count > 0)
            {
                foreach (string name in areaTableNames)
                {
                    comBox.Items.Add(name);
                }
            }
            else
            {
                MessageBox.Show("对不起，您连接的数据库中数据表！！！");
            }

        }
        public List<string> getAreaTableNames()
        {
            List<string> areaTableNames = new List<string>();
            try
            {
                OracleDBHelper helper = new OracleDBHelper();
                OracleConnection con = helper.getOracleConnection();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                string sql = "select distinct table_name  from user_col_comments where lower(table_name) like '%point%' or  lower(table_name) like '%polyline%' or  lower(table_name) like '%line%' or  lower(table_name) like '%polygon%' or lower(table_name) like '%area%' order by table_name asc";
                using (OracleCommand com = new OracleCommand(sql, con))
                {
                    OracleDataReader dr = com.ExecuteReader();
                    while (dr.Read())
                    {
                        string name = dr[0].ToString();
                        areaTableNames.Add(name);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return areaTableNames;
        }

        private void 查询_Click(object sender, EventArgs e)
        {
            string[] a = new string[listBox1.Items.Count];
            //循环便利listBox1中的每一项

            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                //赋值给数组

                a[i] = Convert.ToString(listBox1.Items[i]);

                //MessageBox.Show(a[i].ToString());
            }
            string time1 = listBox1.Items[0].ToString();//dTPicker1.Text
            string time2 = listBox1.Items[1].ToString();
            string time3 = listBox1.Items[2].ToString();
            string time4 = listBox1.Items[3].ToString();
            string tableName = comBox.Text;
            Form3 form = new Form3(time1, time2, time3, time4, tableName);
            form.ShowDialog();
            //将所有数据显示在axMapControl1上
            

        }

        private void dTPicker1_ValueChanged(object sender, System.EventArgs e)
        {
            listBox1.Items.Add(dTPicker1.Text);
        }
    }
}
