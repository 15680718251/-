using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

namespace 全球典型要素数据库增量更新系统
{
    public partial class ShowMap : Form
    {
        public ShowMap()
        {
            InitializeComponent();
        }
        private AxMapControl axMapControl1;//这是字段
        IFeatureLayer pTocFeatureLayer;//这是属性
        //public IFeatureLayer pTocFeatureLayer = null;            //鼠标点击的要素图层
        public AxMapControl Axmapcontrol
        {
            get { return axMapControl1; }
            set { axMapControl1 = value; }
        }

        public ShowMap(AxMapControl axMapControl1)//进行初始化
        {
            this.Axmapcontrol = axMapControl1;
            InitializeComponent();
        }
        IWorkspace workspace;//创建工作空间对象
        private void button1_Click(object sender, EventArgs e)
        {
            
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
        //    // propertySet.SetProperty("INSTANCE", Instance.Text );//如果没有设置INSTANCE属性，会有连接窗体弹出  
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


        private void button2_Click_1(object sender, EventArgs e)
        {
            //直接从sde中读取数据.  
            if (this.FeatureLayer.Text != "")//如果要素图层的文本不为空
            {
                IEnumDataset enumDataset;//枚举类型，装数据集的
                IDataset dataset;
                pTocFeatureLayer = new FeatureLayer();//新建一个要素图层对象
                //获取图层名  
                enumDataset = workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);//使用工作空间获取数据库中的要素集，放入枚举对象中
                dataset = enumDataset.Next();//在枚举对象内部查找下一个数据


                while (dataset != null)//当存在数据时
                {
                    if (dataset.Name == this.FeatureLayer.Text)//判断和要素层是否匹配
                    {
                        pTocFeatureLayer.FeatureClass = dataset as IFeatureClass;//如果数据集中的数据和要素图层匹配成功，那么将数据集转换为要素类
                        
                        break;
                    }
                    dataset = enumDataset.Next();//循环遍历要素集
                }
                string temp;
                temp = pTocFeatureLayer.FeatureClass.AliasName;//用于接收要素类的名字，赋给temp进行处理
                int i = temp.IndexOf('.');//找到.的下标
                temp = temp.Substring(i + 1);//从.开始截取，substring是从i+1开始截取的，也包括i+1
                temp = temp.ToLower();//把大写转小写
                pTocFeatureLayer.Name = temp;//最终把处理好的名字赋给图层名
                IActiveView pActiveView = this.axMapControl1.ActiveView;

                //IEnvelope envelope =this.axMapControl1.Extent;
                //envelope.Height = 2 * pActiveView.Extent.Height;
                //this.axMapControl1.ActiveView.Extent = envelope;

                this.axMapControl1.Map.AddLayer(pTocFeatureLayer); //在底图中增添图层，该图层增添的对象为数据集转换为要素类的对象 
                axMapControl1.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, pTocFeatureLayer, null);
                //axMapControl1.Refresh();
                //IPoint centPoint = new PointClass();
                //centPoint.PutCoords((pActiveView.Extent.XMin + pActiveView.Extent.XMax) / 2,
                //    2 * (pActiveView.Extent.YMax + pActiveView.Extent.YMin) / 3);
                //IEnvelope pEnvelop = pActiveView.FullExtent;
                //pEnvelop.Expand(0.5, 0.5, true);//与放大的区别在于expand的参数不同            
                //pActiveView.Extent.CenterAt(centPoint);
                //pActiveView.Extent = pEnvelop;
                //pActiveView.Refresh();

                //IScreenDisplay m_pScreenDisplay = new ScreenDisplayClass();
                //m_pScreenDisplay.hWnd = this.axMapControl1.hWnd;
                //ISimpleFillSymbol m_pFillSymbol = new SimpleFillSymbolClass();
                //IEnvelope envelope = this.axMapControl1.FullExtent;
                //envelope.PutCoords(pActiveView.Extent.XMin, pActiveView.Extent.YMin, pActiveView.Extent.XMax, pActiveView.Extent.YMax);
                //m_pScreenDisplay.DisplayTransformation.Bounds = envelope;
                //m_pScreenDisplay.DisplayTransformation.VisibleBounds = envelope;
                //this.axMapControl1.ActiveView.Extent = envelope;
                //this.axMapControl1.Extent = this.axMapControl1.FullExtent;//进行全图显示
               
                //centerPoint.PutCoords((pActiveView.Extent.XMin + pActiveView.Extent.XMax) / 2,
                //   (pActiveView.Extent.YMax + pActiveView.Extent.YMin) / 2);
                //envelope.CenterAt(centerPoint);

                IEnvelope envelope = this.axMapControl1.FullExtent;//获取到地图的全图范围
                IPoint centerPoint = new PointClass();
                envelope.Height = 2 * pActiveView.Extent.Height;
                String report2 = "Re-cetered Envelope: \n" +
                                 "LowerLeft  X = " + envelope.LowerLeft.X + "\n" +
                                 "LowerLeft  Y = " + envelope.LowerLeft.Y + "\n\n" +
                                 "LowerRight X =  " + envelope.LowerRight.X + "\n" +
                                 "LowerRight Y =  " + envelope.LowerRight.Y + "\n\n" +
                                 "UpperLeft  X = " + envelope.UpperLeft.X + "\n" +
                                 "UpperLeft  Y = " + envelope.UpperLeft.Y + "\n\n" +
                                 "UpperRight X =  " + envelope.UpperRight.X + "\n" +
                                 "UpperRight Y =  " + envelope.UpperRight.Y; System.Windows.Forms.MessageBox.Show(report2);

                envelope.Expand(0.5, 0.5, true);//放大缩放比例
                this.axMapControl1.ActiveView.Extent = envelope;
                axMapControl1.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, pTocFeatureLayer, null);
                MessageBox.Show("加载成功");
            }
            

            
        }

        private void ShowMap_Load(object sender, EventArgs e)
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


            this.cbbDataSet.Items.Clear();
            this.FeatureLayer.Items.Clear();
            this.RasterLayer.Items.Clear();
            IEnumDatasetName enumDatasetName;//定义枚举数据集名称
            IDatasetName datasetName;//定义数据集名字对象
            //获取矢量数据集
            enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureDataset);//工作空间获取数据集名称，返回一个枚举集，装入不同的名字
            datasetName = enumDatasetName.Next();//获取枚举集中的下一个元素
            while (datasetName != null)
            {
                this.cbbDataSet.Items.Add(datasetName.Name);//将该元素的名字添加到指定位置
                datasetName = enumDatasetName.Next();//获取枚举集中的下一个元素
            }
            //获取图层名  
            enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureClass);
            datasetName = enumDatasetName.Next();
            while (datasetName != null)
            {
                this.FeatureLayer.Items.Add(datasetName.Name);
                datasetName = enumDatasetName.Next();
            }
            //获取栅格图层名  
            enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTRasterDataset);
            datasetName = enumDatasetName.Next();
            while (datasetName != null)
            {
                this.RasterLayer.Items.Add(datasetName.Name);
                datasetName = enumDatasetName.Next();
            }
            //MessageBox.Show("读取成功");
            //该事件的原理就是通过工作空间接口的一种方法获取数据库中栅格，矢量等要素的名称集合，遍历将这些名字索引输出到相应的下拉框
        }


        //public void ss(AxMapControl axMapControl1)
        //{

        //}
    }
}
