using System;
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
using System.IO;
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

namespace GIS.UI.Forms
{
    public partial class historyQuery : Form
    {
      
        public historyQuery()
        {
            InitializeComponent();
        }
        public AxMapControl Axmapcontrol
        {
            get { return axMapControl1; }
            set { axMapControl1 = value; }
        }
        private string feature;
        public string Feature
        {
            get { return feature; }
            set { feature = value; }
        }
        private string dtp;
        public string Dtp
        {
            get { return dtp; }
            set { dtp = value; }
        }
        private string dtp1;
        public string Dtp1
        {
            get { return dtp1; }
            set { dtp1 = value; }
        }
        
        private AxMapControl axMapControl1;//这是字段
        
        IWorkspace workspace;//创建工作空间对象
        private void button1_Click(object sender, EventArgs e)
        {
            timeQuery(this.FeatureLayer.Text,this.dTPicker1.Text,this.dTPicker2.Text);
        }
        public void timeQuery(string feature,string dtp,string dtp1)
        {
            this.feature = feature;
            this.dtp = dtp;
            this.dtp1 = dtp1;
          
            OracleDBHelper help = new OracleDBHelper();
            OracleConnection mycon = help.getOracleConnection();
            if (mycon.State == ConnectionState.Closed)
                mycon.Open();
            try
            {
                //  string sql = string.Format("select osmid,timestamp,userid,tags from {0} where timestamp<'{1}'",this.FeatureLayer.Text, dTPicker1.Text);
                // string sql1 = "select timestamp "+"from" +this.FeatureLayer.Text+"where timestamp<"+dTPicker1.Text;
                string sql2 = string.Format("insert into abc select * from {0} where starttime<'{1}'", this.FeatureLayer.Text, dTPicker1.Text);
                string sql3 = string.Format("delete from abc where starttime>'2007-10-28%'");
                string sql4 = string.Format("insert into abc1 select * from {0} where starttime<'{1}'", this.FeatureLayer.Text, dTPicker2.Text);
                OracleCommand mycom = new OracleCommand(sql2, mycon);
                OracleCommand mycom1 = new OracleCommand(sql4, mycon);
                mycom.ExecuteNonQuery();
                mycom1.ExecuteNonQuery();
                // OracleDataReader mydr； 
                //mycom.CommandText = sql;
                //mycom.CommandType = CommandType.Text;

                //OracleDataAdapter myda = new OracleDataAdapter(sql, mycon);
                //DataSet myds = new DataSet();
                //myda.Fill(myds, this.FeatureLayer.Text);
                //dataGridView1.DataSource = myds.Tables[0];
            }
            catch (OracleException ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
             IEnumDataset enumDataset;//枚举类型，装数据集的
             IDataset dataset;
             //获取图层名
             enumDataset = workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);//使用工作空间获取数据库中的要素集，放入枚举对象中
             dataset = enumDataset.Next();//在枚举对象内部查找下一个数据
             if (dTPicker2.Text == dTPicker1.Text)
             {
                 Form2 frm = new Form2(feature,dtp);
                 frm.Show();
             }
             else
             {
                 Form1 frm = new Form1(feature, dtp, dtp1);//首先实例化
                 // frm.WindowState = FormWindowState.Maximized; // 最大化
                 frm.Show();//Show方法
             }
            }
        /// <summary>
        /// 执行数据导出操作
        /// </summary>
      
        //private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    for (int i = 0; i < dataGridView1.Rows.Count; i++)
        //    {
        //        dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
        //    }
        //}
      
        //private void button2_Click(object sender, EventArgs e)
        //{
            
        //        IEnumDataset enumDataset;//枚举类型，装数据集的
        //        IDataset dataset;
        //        pFeatureLayer = new FeatureLayerClass();//新建一个要素图层对象
        //        //获取图层名
        //        enumDataset = workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);//使用工作空间获取数据库中的要素集，放入枚举对象中
        //        dataset = enumDataset.Next();//在枚举对象内部查找下一个数据
        //        string tableName = "SYSTEM.ABC";
        //        while (dataset != null)//当存在数据时
        //        {
        //            if (dataset.Name == tableName)//判断和要素层是否匹配
        //            {
        //                pFeatureLayer.FeatureClass = dataset as IFeatureClass;//如果数据集中的数据和要素图层匹配成功，那么将数据集转换为要素类
        //                string layerName = pFeatureLayer.FeatureClass.AliasName;   //获取别名
        //                pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;
        //                break;
        //            }
        //            dataset = enumDataset.Next();//循环遍历要素集
        //        }
        //        Form1 frm = new Form1(pFeatureLayer);//首先实例化
        //        frm.Show();//Show方法
           
        //}

        private void historyQuery_Load(object sender, EventArgs e)
        {

            string server = OSMDataBaseLinkForm.Server_;
            string user = OSMDataBaseLinkForm.User_;
            string password = OSMDataBaseLinkForm.Password_;
            string database = OSMDataBaseLinkForm.DataBase_;


            IPropertySet pPropset = new PropertySet();//创建一个属性设置对象
            IWorkspaceFactory pWorkspaceFact = new SdeWorkspaceFactory();//创建一个空间数据引擎工作空间工厂
            pPropset.SetProperty("server", server);
            // propertySet.SetProperty("INSTANCE", Instance.Text );//如果没有设置INSTANCE属性，会有连接窗体弹出  
            pPropset.SetProperty("INSTANCE", "sde:oracle11g:" + server + "/" + database);// by 丁洋修改
            pPropset.SetProperty("database", database);
            pPropset.SetProperty("user", user);
            pPropset.SetProperty("password", password);
            pPropset.SetProperty("version", "SDE.DEFAULT");
            workspace = pWorkspaceFact.Open(pPropset, 0);//使用属性集来打开地理数据库
            //MessageBox.Show("连接成功");
            //原理就是根据空间数据引擎工作空间工厂进行属性对象设置，返回一个工作空间，正确就通过
            //有个问题：属性集不正确的话就直接抛出异常而不是叫我们自己重新输入

            // this.FeatureLayer.Items.Clear();
            IEnumDatasetName enumDatasetName;//定义枚举数据集名称
            IDatasetName datasetName;//定义数据集名字对象
            //获取矢量数据集
            enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureDataset);//工作空间获取数据集名称，返回一个枚举集，装入不同的名字
            datasetName = enumDatasetName.Next();//获取枚举集中的下一个元素

            enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureClass);
            datasetName = enumDatasetName.Next();
            while (datasetName != null)
            {
                this.FeatureLayer.Items.Add(datasetName.Name);
                datasetName = enumDatasetName.Next();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OracleDBHelper help = new OracleDBHelper();
            OracleConnection mycon = help.getOracleConnection();
            if (mycon.State == ConnectionState.Closed)
                mycon.Open();
                //  string sql = string.Format("select osmid,timestamp,userid,tags from {0} where timestamp<'{1}'",this.FeatureLayer.Text, dTPicker1.Text);
                // string sql1 = "select timestamp "+"from" +this.FeatureLayer.Text+"where timestamp<"+dTPicker1.Text;
                string sql2 = string.Format("insert into abc select * from apoly where starttime<'{0}'", dTPicker1.Text);
                string sql3 = string.Format("delete from abc where starttime<'{0}'", dTPicker1.Text);
                string sql4 = string.Format("delete from abc1 where starttime<'{0}'", dTPicker2.Text);
                OracleCommand mycom = new OracleCommand(sql3, mycon);
                OracleCommand mycom1 = new OracleCommand(sql4, mycon);
                mycom.ExecuteNonQuery();
                mycom1.ExecuteNonQuery();
         
        }

      
        //private void button3_Click(object sender, EventArgs e)
        //{
        //    string server = OSMDataBaseLinkForm.Server_;
        //    string user = OSMDataBaseLinkForm.User_;
        //    string password = OSMDataBaseLinkForm.Password_;
        //    string database = OSMDataBaseLinkForm.DataBase_;


        //    IPropertySet pPropset = new PropertySet();//创建一个属性设置对象
        //    IWorkspaceFactory pWorkspaceFact = new SdeWorkspaceFactory();//创建一个空间数据引擎工作空间工厂
        //    pPropset.SetProperty("server", server);
        //    // propertySet.SetProperty("INSTANCE", Instance.Text);//如果没有设置INSTANCE属性，会有连接窗体弹出  
        //    pPropset.SetProperty("INSTANCE", "sde:oracle11g:" + server + "/" + database);// by 丁洋修改
        //    pPropset.SetProperty("database", database);
        //    pPropset.SetProperty("user", user);
        //    pPropset.SetProperty("password", password);
        //    pPropset.SetProperty("version", "SDE.DEFAULT");
        //    workspace = pWorkspaceFact.Open(pPropset, 0);//使用属性集来打开地理数据库
        //    MessageBox.Show("连接成功");
        //    //原理就是根据空间数据引擎工作空间工厂进行属性对象设置，返回一个工作空间，正确就通过
        //    //有个问题：属性集不正确的话就直接抛出异常而不是叫我们自己重新输入
        //}
        //public static void Outvalues(DataSet ds)
        //{
        //    foreach (DataTable dt in ds.Tables)
        //    {
        //        //        MessageBox.Show(dt.TableName);
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            foreach (DataColumn col in dt.Columns)
        //            {
        //                MessageBox.Show(row[col] + "/t");
        //            }
        //        }
        //    }
        //}


    }
}
