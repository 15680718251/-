using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using GIS.UI.AdditionalTool;
using GIS.UI.Entity;
using Oracle.ManagedDataAccess.Client;

namespace GIS.UI.Forms
{
    public partial class SaveEdit : Form
    {
        public SaveEdit()
        {
            InitializeComponent();
        }
        public SaveEdit(string tablename, AxMapControl axMapControl1)
        {
            InitializeComponent();
            this.axMapControl1 = axMapControl1;
            this.tablename = tablename;
        }
        string pFilePath = null;
        string pFileName = null;
        string[] pFullPath = null;
        string dataBasePara = null;
        AxMapControl axMapControl1;
        string tablename;

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog pOpenFileDialog = new OpenFileDialog();
            pOpenFileDialog.CheckFileExists = true;
            pOpenFileDialog.Title = "打开Shp数据";
            pOpenFileDialog.Filter = "Shape文件（*.shp）|*.shp";
            pOpenFileDialog.Multiselect = false;

            if (pOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                pFullPath = pOpenFileDialog.FileNames;
                string files = "";
                for (int i = 0; i < pFullPath.Length; i++)
                {
                    if (pFullPath[i] == "") return;
                    int pIndex = pFullPath[i].LastIndexOf("\\");
                    pFileName = pFullPath[i].Substring(pIndex + 1); //文件名
                    pIndex = pFileName.LastIndexOf(".");
                    pFileName = pFileName.Substring(0, pIndex); //文件名

                    pIndex = pFullPath[i].LastIndexOf(".");
                    pFilePath = pFullPath[i].Substring(0, pIndex); //文件路径

                    files += pFullPath[i];
                    textBox1.Text = files;
                }
            }
        }
        List<Poly> polyList = null;//线面属性集合
        List<Ppoint> pointList = null;//点属性集合
        private void 保存编辑_Click(object sender, EventArgs e)
        {
            OracleDBHelper helper = new OracleDBHelper();
            OracleConnection mycon = helper.getOracleConnection();
            string sql = string.Format("delete from {0} ", tablename);
            OracleCommand mycom = new OracleCommand(sql, mycon);
            mycom.ExecuteNonQuery();
            int dataType = 0;//0代表基态，1代表增量
            int topoType = -1;//0代表点，1代表线，2代表面

            //if (this.rb_line.Checked == true) topoType = 1;
            //if (this.rb_poly.Checked == true) topoType = 2;
            //
            //判断表名是否为空
            //string tablename = pTocFeatureLayer.Name.ToString();
            if (tablename.Contains("LINE")) topoType = 1;
            if (tablename.Contains("POINT")) topoType = 0;
            if (tablename.Contains("AREA")) topoType = 2;
            //string pFullPath = "F:\\典型要素更新系统改动\\testfile\\WATER_AREA.shp";
            //判断数据库中是否含有该表名，若没有则新建
            if (!helper.IsExistTable(tablename))//判断当前数据库中是否存在该表
            {
                if (dataType == 0)
                {
                    if (topoType == 0) helper.createPointTable(tablename);
                    else helper.createPolyTable(tablename);
                }
                else
                {
                    if (topoType == 0) helper.createZPointTable(tablename);
                    else helper.createZPolyTable(tablename);
                }
            }
            //将shp数据显示到地图控件上，并获取其属性信息
            ShpHelper shphelper = new ShpHelper(axMapControl1);
            shphelper.addShpToAxmap(pFullPath);
            int index = axMapControl1.LayerCount;
            DataTable dt = shphelper.getShpAttributes(index - 2);

            //将属性datatable转成List
            if (topoType == 0) pointList = shphelper.datatableToPointList(dt, dataType);
            else polyList = shphelper.datatabletoPolyList(dt, dataType);
            insertShpToOracle(dataType, topoType);
            this.Close();

        }
        public void insertShpToOracle(int dataType, int topoTpye)
        {
            if (dataType == 0)
            {
                switch (topoTpye)
                {
                    case 0://目标为点
                        new System.Threading.Thread(new System.Threading.ThreadStart(processJPoint)).Start();
                        break;
                    case 1://目标为线
                        new System.Threading.Thread(new System.Threading.ThreadStart(processJLine)).Start();
                        break;
                    case 2://目标为面
                        new System.Threading.Thread(new System.Threading.ThreadStart(processJArea)).Start();
                        break;
                }
            }
            //else
            //{
            //    switch (topoTpye)
            //    {
            //        case 0://目标为增量点
            //            new System.Threading.Thread(new System.Threading.ThreadStart(processZPoint)).Start();
            //            break;
            //        case 1://目标为增量线
            //            new System.Threading.Thread(new System.Threading.ThreadStart(processZLine)).Start();
            //            break;
            //        case 2://目标为增量面
            //            new System.Threading.Thread(new System.Threading.ThreadStart(processZArea)).Start();
            //            break;
            //    }
            //}
        }
        public void processJPoint()
        {

            OracleDBHelper odh = new OracleDBHelper();
            odh.insertPointDataByList(tablename, pointList);


        }

        //线程，基态shp线入库
        public void processJLine()
        {

            OracleDBHelper odh = new OracleDBHelper();
            odh.insertPolyDataByList(tablename, polyList);

        }

        //线程，基态shp面入库
        public void processJArea()
        {
            OracleDBHelper odh = new OracleDBHelper();
            odh.insertPolyDataByList(tablename, polyList);

        }



    }
}
