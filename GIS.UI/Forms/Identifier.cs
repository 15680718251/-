using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Controls;

namespace GIS.UI.Forms
{
    public partial class Identifier : Form
    {
        public Identifier()
        {
            InitializeComponent();
        }

        public Identifier(AxMapControl axMapcontrol)
        {
            InitializeComponent();
            this.AxMapcontrol = axMapcontrol;
        }

        private IFeature pFeature;
        public IFeature PFeature
        {
            get { return pFeature; }
            set { pFeature = value; }
        }

        private AxMapControl axMapcontrol;
        public AxMapControl AxMapcontrol
        {
            get { return axMapcontrol; }
            set { axMapcontrol = value; }
        }

        /// <summary>
        /// 根据feature获得图层名featurelayername
        /// </summary>
        /// <param name="pFeature">IFeature</param>
        /// <returns></returns>
        private string getLayerNameByFeature(IFeature pFeature)
        {
            int index = -1;
            IFeatureClass pFeatureClass = pFeature.Class as IFeatureClass;
            IFeatureLayer pFeatureLayer=null;
            for (int i = 0; i < AxMapcontrol.Map.LayerCount; i++)
            {
                ILayer pLayer = AxMapcontrol.get_Layer(i);
                if (pLayer is IGroupLayer)//判断是不是图层组
                {
                    ICompositeLayer pGroupLayer = pLayer as ICompositeLayer;//将图层组转换为子图层
                    for (int j = 0; j < pGroupLayer.Count; j++)
                    {
                        pFeatureLayer = pGroupLayer.get_Layer(j) as IFeatureLayer;//将子图层转换为几何图层
                        IFeatureClass iFeatureCla = pFeatureLayer.FeatureClass;
                        if (iFeatureCla == pFeatureClass)
                        {
                            index = i;
                            goto tiaochu;
                        }
                    }
                }
                else 
                {
                    pFeatureLayer = AxMapcontrol.get_Layer(i) as IFeatureLayer;
                    IFeatureClass iFeatureCla = pFeatureLayer.FeatureClass;
                    if (iFeatureCla == pFeatureClass)
                    {
                        index = i;
                        goto tiaochu;
                    }
                }

                

            }
        tiaochu: ;
            string layername = AxMapcontrol.get_Layer(index).Name;
            return layername;
            
        }


        public void InitUI()
        {
                GetAttributesTable(pFeature.Class as IFeatureClass);
        }
       
        /// <summary>
        /// 获取要素属性表
        /// </summary>
        /// <param name="pFeatureClass"></param>
        /// <returns></returns>
        private  void GetAttributesTable(IFeatureClass pFeatureClass)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("所属图层");
            
            string geometryType = string.Empty;
            if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
            {
                geometryType = "点";
                dataTable.Columns.Add("点位");
            }
            if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryMultipoint)
            {
                geometryType = "点集";
                dataTable.Columns.Add("点位");
            }
            if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
            {
                geometryType = "折线";
                dataTable.Columns.Add("长度");
            }
            if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
            {
                geometryType = "面";
                dataTable.Columns.Add("面积");
            }

            // 字段集合
            IFields pFields = pFeatureClass.Fields;
            int fieldsCount = pFields.FieldCount;

            // 写入字段名
            
            for (int i = 0; i < fieldsCount; i++)
            {
                dataTable.Columns.Add(pFields.get_Field(i).Name);
            }
            
            IQueryFilter filter =new QueryFilterClass();

            filter.WhereClause = "FID =" + pFeature.get_Value(0);
            // 要素游标
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(filter, false);
            IFeature pFeature1 = pFeatureCursor.NextFeature();
            if (pFeature1 == null)
            {
                
            }

            // 获取MZ值
            IMAware pMAware = pFeature1.Shape as IMAware;
            IZAware pZAware = pFeature1.Shape as IZAware;
            if (pMAware.MAware)
            {
                geometryType += " M";
            }
            if (pZAware.ZAware)
            {
                geometryType += "Z";
            }

            // 写入字段值
            while (pFeature1 != null)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["所属图层"] = getLayerNameByFeature(pFeature1);
                IGeometry geo = pFeature1.Shape;
                geo.Project(AxMapcontrol.Map.SpatialReference);
                esriUnits unit = AxMapcontrol.Map.MapUnits;
                if (pFeature1.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                { 
                
                }
                if (pFeature1.Shape.GeometryType == esriGeometryType.esriGeometryMultipoint)
                {

                }
                if (pFeature1.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                {
                    dataRow["长度"] = (geo as IPolyline).Length.ToString() ;
                }
                if (pFeature1.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    dataRow["面积"] = (geo as IArea).Area.ToString() ;
                }
               
                for (int i = 0; i < fieldsCount; i++)
                {
                    if (pFields.get_Field(i).Type == esriFieldType.esriFieldTypeGeometry)
                    {
                        dataRow[i+2] = geometryType;
                    }
                    else
                    {
                        dataRow[i+2] = pFeature1.get_Value(i).ToString();
                    }
                }
                dataTable.Rows.Add(dataRow);
                pFeature1 = pFeatureCursor.NextFeature();
            }

             //释放游标
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);

            #region 行列转置
            DataTable dtNew = new DataTable();
        dtNew.Columns.Add("字段", typeof(string));

        for (int i = 0; i < dataTable.Rows.Count; i++)

       {

            dtNew.Columns.Add("字段值");

       }

        foreach (DataColumn dc in dataTable.Columns)

        {

            DataRow drNew = dtNew.NewRow();

            drNew["字段"] = dc.ColumnName;

            for (int i = 0; i < dataTable.Rows.Count; i++)

            {

                drNew[i + 1] = dataTable.Rows[i][dc].ToString();

            }

            dtNew.Rows.Add(drNew);

        }
            #endregion

        //this.gridControl1.DataSource = dtNew;
        this.dataGridAttribute.DataSource = dtNew;
        }

       
    }
}
