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
    public partial class Form1 : Form
    {
        string tablename = "ABC";
        string tablename1 = "ABC1";
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
        public Form1(string feature, string dtp,string dtp1)
        {
            InitializeComponent();
            this.Feature = feature;
            this.Dtp = dtp;
            this.Dtp1 = dtp1;
        }
        public Form1(string tablename,string tablename1)//
        {
            InitializeComponent();
            this.tablename = tablename;
            tablename = tablename.Substring(7);
            this.tablename1 = tablename1;
            tablename1 = tablename1.Substring(7);
           
          
        }
        //public Form1(string tablename2)
        //{
        //    InitializeComponent();
        //    this.tablename2 = tablename2;
        //    tablename2 = tablename2.Substring(7);
        //}
        //private string tablename2;
        //public string Tablename2 
        //{
        //    get { return tablename2; }
        //    set { tablename2 = value; }
        //}
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
      
       // IMapControlDefault mapControlDefault; IGeometry iGeometry;
        private void Form1_Load(object sender, EventArgs e)
        {
            AddMap showmap1 = new AddMap();
            showmap1.showMap(tablename, this.axMapControl1);
             AddMap showmap2=new AddMap();
            showmap2.showMap(feature.Substring(7), this.axMapControl2);
            AddMap showmap3 = new AddMap();
            showmap3.showMap(tablename1, this.axMapControl3);
            AddMap showmap4 = new AddMap();
            showmap4.showMap(feature.Substring(7), this.axMapControl4);
            label1.Text = DateTime.Now.ToString("yyyy-MM-dd");
            label2.Text = dtp;
            label3.Text = dtp1;
            label4.Text = DateTime.Now.ToString("yyyy-MM-dd");
            axMapControl1.Map.Name = "图层列表";
            axMapControl3.Map.Name = "图层列表";
            //this.Axmapcontrol.AddLayer(feature);//在底图中增添图层，该图层增添的对象为数据集转换为要素类的对象                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
            this.Axmapcontrol.Extent = this.axMapControl1.FullExtent;
           //axMapControl1.ActiveView.Refresh();
           // axMapControl1.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewAll, feature, this.Axmapcontrol.FullExtent);
           //axMapControl1.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, feature, null);
        }

       
       
       
    }
}
