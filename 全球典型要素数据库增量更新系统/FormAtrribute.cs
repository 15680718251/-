using System;
using System.Data;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;

namespace 全球典型要素数据库增量更新系统
{
    public partial class FormAtrribute : Form
    {
        public FormAtrribute()
        {

            InitializeComponent();
           
        }
        private AxMapControl axMapControl1;

        public AxMapControl Axmapcontrol
        {
            get { return axMapControl1; }
            set { axMapControl1 = value; }
        }

        public FormAtrribute(AxMapControl axmapcontrol)
        {
            this.axMapControl1 = axmapcontrol;
            InitializeComponent();

        }
        //要查询的属性图层
        private IFeatureLayer _curFeatureLayer;
        public IFeatureLayer CurFeatureLayer
        {
            get { return _curFeatureLayer; }
            set { _curFeatureLayer = value; }
        }

        public void InitUI()
        {
            if (_curFeatureLayer == null) return;
            string geometryType = string.Empty;
            IFeatureClass pFeatureClass = _curFeatureLayer.FeatureClass;

            if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
            {
                geometryType = "点";
            }
            if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryMultipoint)
            {
                geometryType = "点集";
            }
            if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
            {
                geometryType = "折线";
            }
            if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
            {
                geometryType = "面";
            }
            IFeature pFeature = null;
            DataTable pFeatDT = new DataTable(); //创建数据表
            DataRow pDataRow = null; //数据表行变量
            DataColumn pDataCol = null; //数据表列变量
            IField pField = null;
            for (int i = 0; i < pFeatureClass.Fields.FieldCount; i++)
            {
                pDataCol= new DataColumn();
                pField = pFeatureClass.Fields.get_Field(i);
                pDataCol.ColumnName = pField.AliasName; //获取字段名作为列标题
                pDataCol.DataType = Type.GetType("System.Object");//定义列字段类型
                pFeatDT.Columns.Add(pDataCol); //在数据表中添加字段信息
            }

            IFeatureCursor pFeatureCursor = _curFeatureLayer.Search(null, true);
            
                pFeature = pFeatureCursor.NextFeature();
            
          
           
            while (pFeature != null)

            {
                //try { 
                    //pDataRow = pFeatDT.NewRow();
                pDataRow = pFeatDT.NewRow();
                //获取字段属性
                for (int k = 0; k < pFeatDT.Columns.Count; k++)
                {
                    if (pFeatureClass.Fields.get_Field(k).Type == esriFieldType.esriFieldTypeGeometry)
                    {
                        pDataRow[k] = geometryType;
                    }
                    else
                    {
                        pDataRow[k] = pFeature.get_Value(k);
                        //Console.WriteLine(k + "：" + pFeature.get_Value(k));
                    }

                    
                }

                pFeatDT.Rows.Add(pDataRow); //在数据表中添加字段属性信息
                pFeature = pFeatureCursor.NextFeature();
                
                //}

                //catch(Exception e)
                //{
                
                //}
            }
            //释放指针
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);

            //dataGridAttribute.BeginInit();
            dataGridAttribute.DataSource = pFeatDT;
            //dataGridAttribute.EndInit();
        }

        private void dataGridAttribute_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
         
            axMapControl1.ActiveView.Refresh();
            IQueryFilter pQuery = new QueryFilterClass();
            string val;
            string col ;
            col = string.Format("{0}", this.dataGridAttribute.Columns[0].Name);
            val = string.Format("{0}",this.dataGridAttribute.CurrentCell.OwningRow.Cells[0].Value.ToString());
            //设置高亮要素的查询条件
            pQuery.WhereClause = col + "=" + val;
            //"Name" = '行政区E'
            IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
            if (pFeatureLayer == null) return;
            IFeatureSelection pFeatSelection;

            pFeatSelection = pFeatureLayer as IFeatureSelection;
            pFeatSelection.Clear();
            pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);

           IFeatureCursor pFeatureCursor = _curFeatureLayer.Search(pQuery, true);
           IEnvelope envelope = new EnvelopeClass();

            IFeature pFeature = pFeatureCursor.NextFeature();
            while (pFeature != null)
                    {
                        IGeometry geometry = pFeature.Shape;
                        IEnvelope featureExtent = geometry.Envelope;
                        envelope.Union(featureExtent);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeature);
                        pFeature = pFeatureCursor.NextFeature();
                    }
            axMapControl1.ActiveView.FullExtent = envelope;

            //(axMapControl1.Map as IActiveView).FullExtent = pFeature.Extent;

            axMapControl1.ActiveView.Refresh();
        }

        
    }
}