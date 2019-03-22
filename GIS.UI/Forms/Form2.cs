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
    public partial class Form2 : Form
    {
        string tablename = "ABC";
        public AxMapControl Axmapcontrol
        {
            get { return axMapControl1; }
            set { axMapControl1 = value; }
        }
        public AxMapControl Axmapcontrol2
        {
            get { return axMapControl2; }
            set { axMapControl2 = value; }
        }

        public Form2(string feature, string dtp)
        {
            InitializeComponent();
            this.Feature = feature;
            this.Dtp = dtp;
        }
        public Form2(string tablename)
        {
            InitializeComponent();
            this.tablename = tablename;
            tablename = tablename.Substring(7);
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
        private void Form2_Load(object sender, EventArgs e)
        {
            AddMap showmap1 = new AddMap();
            showmap1.showMap(tablename, this.axMapControl1);
            AddMap showmap2 = new AddMap();
            showmap2.showMap(feature.Substring(7), this.axMapControl2);
            label1.Text = dtp;
            label2.Text = DateTime.Now.ToString("yyyy-MM-dd");
            axMapControl1.Map.Name = "图层列表";
            //this.Axmapcontrol.AddLayer(feature);//在底图中增添图层，该图层增添的对象为数据集转换为要素类的对象                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
            this.Axmapcontrol.Extent = this.axMapControl1.FullExtent;
        }
    }
}
