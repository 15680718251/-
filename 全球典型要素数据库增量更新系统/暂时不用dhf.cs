using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;

namespace 全球典型要素数据库增量更新系统
{
    public partial class 暂时不用dhf : Form
    {
        public 暂时不用dhf()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog pOpenFileDialog = new OpenFileDialog();
                pOpenFileDialog.CheckFileExists = true;
                pOpenFileDialog.Title = "打开Shape文件";
                pOpenFileDialog.Filter = "Shape文件（*.shp）|*.shp";
                pOpenFileDialog.Multiselect = true;
                pOpenFileDialog.ShowDialog();

                //获取文件路径
                //FileInfo pFileInfo = new FileInfo(pOpenFileDialog.FileName);
                //string pPath = pOpenFileDialog.FileName.Substring(0, pOpenFileDialog.FileName.Length - pFileInfo.Name.Length);
                //axMapControl1.AddShapeFile(pPath, pFileInfo.Name);

                //IWorkspaceFactory pWorkspaceFactory;
                //IFeatureWorkspace pFeatureWorkspace;
                //IFeatureLayer pFeatureLayer;
                string[] pFullPath = pOpenFileDialog.FileNames;
                for (int i = 0; i < pFullPath.Length; i++)
                {
                    if (pFullPath[i] == "") return;

                    int pIndex = pFullPath[i].LastIndexOf("\\");
                    string pFilePath = pFullPath[i].Substring(0, pIndex); //文件路径
                    string pFileName = pFullPath[i].Substring(pIndex + 1); //文件名


                    axMapControl1.AddShapeFile(pFilePath, pFileName);
                }

               
                axMapControl1.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("图层加载失败！" + ex.Message);
            }

           
        }
        bool bu = false;

        private void button2_Click(object sender, EventArgs e)
        {
            bu = true;
        }

        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            try
            {
                if (bu)
                {
                    DataTable pDataTable = new DataTable();
                    DataRow pDataRow = null;
                    pDataTable.Columns.Add("ID");
                    pDataTable.Columns.Add("Name");
                    pDataTable.Columns.Add("ParentID");
                    pDataTable.Columns.Add("Value");

                    for (int i = 0; i < axMapControl1.Map.LayerCount; i++)
                    {
                        pDataRow = pDataTable.NewRow();
                        string lyrName = axMapControl1.Map.get_Layer(i).Name;
                        pDataRow["ID"] = lyrName;
                        pDataRow["Name"] = lyrName;
                        pDataRow["ParentID"] = -1;

                        pDataTable.Rows.Add(pDataRow);


                        //开始点选查询
                        IMap pMap;
                        pMap = axMapControl1.Map as IMap;

                        //获取点图层
                        IFeatureLayer pFeatureLayer;
                        pFeatureLayer = pMap.get_Layer(i) as IFeatureLayer;
                        IFeatureClass pFeatureClass;
                        pFeatureClass = pFeatureLayer.FeatureClass;

                        //获取鼠标点击点
                        IPoint pPoint;
                        pPoint = new PointClass();
                        pPoint.PutCoords(e.mapX, e.mapY);

                        IGeometry pGeometry;

                        //定义缓冲区
                        double db = 2;
                        ITopologicalOperator pTop;
                        pTop = pPoint as ITopologicalOperator;
                        pGeometry = pTop.Buffer(db);

                        //选取
                        pMap.SelectByShape(pGeometry, null, false);
                        pMap.ClearSelection();

                        //空间过滤运算
                        ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                        pSpatialFilter.Geometry = pGeometry;


                        //设置为不同的要素类型的图层


                        switch (pFeatureClass.ShapeType)
                        {
                            case esriGeometryType.esriGeometryPoint:
                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                break;
                            case esriGeometryType.esriGeometryPolyline:
                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                                break;
                            case esriGeometryType.esriGeometryPolygon:
                                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                break;

                        }
                        pSpatialFilter.GeometryField = pFeatureClass.ShapeFieldName;

                        //指针
                        IFeatureCursor pFeatureCursor;
                        pFeatureCursor = pFeatureClass.Search(pSpatialFilter, false);
                        IFeature pFeature;
                        pFeature = pFeatureCursor.NextFeature();

                        //开始遍历
                        while (pFeature != null)
                        {

                            //获取要素的字段名和字段值
                            int n = pFeature.Fields.FieldCount;
                            string sName;
                            string sValue;


                            //这句话的对象需要随着地图的改变而改变。可以是ID，FID 等带有唯一标识身份的 东西。
                            int index = pFeature.Fields.FindField("ObjectID");
                            if (index == -1)
                                return;
                            IField pField = pFeature.Fields.get_Field(index);
                            sName = pField.Name;

                            sValue = pFeature.get_Value(index).ToString();


                            pDataRow = pDataTable.NewRow();

                            //之所以这样赋值是为了保证ID的唯一性；
                            pDataRow["ID"] = lyrName + sValue;
                            pDataRow["Name"] = sValue;
                            pDataRow["ParentID"] = lyrName;
                            pDataTable.Rows.Add(pDataRow);

                            pFeature = pFeatureCursor.NextFeature();

                        }
                        //这个是师兄交的，指针等占内存的东西在用完之后一定要释放；！！！
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
                    }

                    treeList1.DataSource = pDataTable;
                    treeList1.ParentFieldName = "ParentID";



                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            treeList1.DataSource = null;
            // dataGridView1.DataSource = null;
        }

        private void treeList1_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            try
            {
                int layerIndex;
                if (e.Node.HasChildren)
                {
                    return;
                }
                if (e.Node.ParentNode != null) //***********//这个存在bug,若节点超过两级则出错
                {
                    //创建一个新的表作为属性表
                    DataTable dt = new DataTable();
                    DataRow dr = null;
                    dt.Columns.Add("Name");
                    dt.Columns.Add("Value");


                    //循环图层
                    for (int i = 0; i < this.axMapControl1.LayerCount; i++)
                    {
                        //如果父节点名称和图层名相同，获取索引
                        if (e.Node.ParentNode.GetValue(0).ToString() == this.axMapControl1.get_Layer(i).Name)
                        {
                            layerIndex = i;
                            IFeature pFeature;

                            pFeature = (this.axMapControl1.get_Layer(layerIndex) as IFeatureLayer).FeatureClass.GetFeature(int.Parse(this.treeList1.FocusedNode.GetValue(0).ToString())); 

                            if (pFeature != null)
                            {
                                //循环字段集，赋值给表dt
                                int n = pFeature.Fields.FieldCount;

                                for (int k = 0; k < n - 1; k++)
                                {

                                    IField pField = pFeature.Fields.get_Field(k);
                                    string fieldName = pField.Name;
                                    string fieldValue = pFeature.get_Value(k).ToString();

                                    //赋值给表
                                    dr = dt.NewRow();
                                    dr["Name"] = fieldName;
                                    dr["Value"] = fieldValue;
                                    dt.Rows.Add(dr);
                                }

                                gridControl1.DataSource = dt;

                            }
                            else
                                return;

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
