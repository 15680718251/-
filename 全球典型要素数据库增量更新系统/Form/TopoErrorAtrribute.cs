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
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using GIS.UI.AdditionalTool;
using System.Collections;
using Oracle.ManagedDataAccess.Client;
using QualityControl.Topo;

//zh编写
//拓扑冲突显示处理表
namespace 全球典型要素数据库增量更新系统
{
    public partial class TopoErrorAtrribute : Form
    {
        public TopoErrorAtrribute()
        {
            m_OperateMap = new OperateMap();
            map = new AddMap();
            InitializeComponent();
        }

        #region 变量
        private AxMapControl axMapControl1;
        private OperateMap m_OperateMap;
        private AddMap map;
        private string solution;//选择的处理方案
        private string layer1;//图层1名称
        private string layer2;//图层2名称
        private ArrayList id1List;//图层1的冲突id集合
        private ArrayList id2List;//图层2的冲突id集合
        #endregion

        public AxMapControl Axmapcontrol
        {
            get { return axMapControl1; }
            set { axMapControl1 = value; }
        }

        public TopoErrorAtrribute(AxMapControl axmapcontrol)
        {
            this.axMapControl1 = axmapcontrol;
            m_OperateMap = new OperateMap();
            map = new AddMap();
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
            //定位本图层要素
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            IQueryFilter pQuery = new QueryFilterClass();
            string val;
            string col ;
            col = string.Format("{0}", this.dataGridAttribute.Columns[1].Name);
            val = string.Format("{0}",this.dataGridAttribute.CurrentCell.OwningRow.Cells[1].Value.ToString());
            //设置高亮要素的查询条件
            pQuery.WhereClause = col + "=" + val;
            //"Name" = '行政区E'
            IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
            if (pFeatureLayer == null) return;
            IFeatureSelection pFeatSelection;

            pFeatSelection = pFeatureLayer as IFeatureSelection;
            pFeatSelection.Clear();
            pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);

            //联动更新相关图层要素
            string objectid1 = this.dataGridAttribute.CurrentRow.Cells["objectid1"].Value.ToString();
            string objectid2 = this.dataGridAttribute.CurrentRow.Cells["objectid2"].Value.ToString();
            string layer1 = this.dataGridAttribute.CurrentRow.Cells["layer1"].Value.ToString();
            string layer2 = this.dataGridAttribute.CurrentRow.Cells["layer2"].Value.ToString();
            
            
            if (!layer1.Equals(layer2))
            {
                IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer1);
                //map.showByAttibute(axMapControl1, "objectid", "=", int.Parse(objectid));
                map.showByAttibute(pFeatLyr, "objectid", "=", objectid1, 255, 0, 0);//图层1的显示颜色

                IFeatureLayer pFeatLyr2 = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer2);
                map.showByAttibute(pFeatLyr2, "objectid", "=", objectid2, 255, 0, 0);//图层2的显示颜色
            }
            else
            {
                IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer1);
                ArrayList list = new ArrayList();
                list.Add(objectid1);
                list.Add(objectid2);
                map.showByAttibute(pFeatLyr, "objectid", "=", list, 255, 0, 0);

                //IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer1);
                //map.showByAttibute(pFeatLyr, "objectid", "=", objectid1, 255, 0, 0);

                //IFeatureLayer pFeatLyr2 = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer2);
                //map.showByAttibute(pFeatLyr2, "objectid", "=", objectid2, 0, 0, 255);
            }
        }

        private void TopoErrorAtrribute_Load(object sender, EventArgs e)
        {
            this.cbb_solution.Items.Add("不处理");
            this.cbb_solution.Items.Add("打断点");
            this.cbb_solution.Items.Add("分开线");
            this.cbb_solution.Items.Add("修改属性");
            this.cbb_solution.Items.Add("删除线");
        }

        //确定事件
        private void tb_ok_Click(object sender, EventArgs e)
        {
            solution = cbb_solution.Text.ToString();
            if (solution == null || solution.Equals(""))
            {
                MessageBox.Show("请选择处理方案");
                return;
            }
            id1List = new ArrayList();
            id2List = new ArrayList();
            ArrayList uidList = new ArrayList();
            layer1 = dataGridAttribute.Rows[0].Cells["layer1"].Value.ToString();
            layer2 = dataGridAttribute.Rows[0].Cells["layer2"].Value.ToString();
            for (int i = 0; i < this.dataGridAttribute.RowCount; i++)
            {
                if (dataGridAttribute.Rows[i].Cells["check"].Value == null) continue;
                if (dataGridAttribute.Rows[i].Cells["check"].Value.ToString().Equals("True"))
                {
                    uidList.Add(this.dataGridAttribute.Rows[i].Cells["uniqueid"].Value.ToString());
                    id1List.Add(this.dataGridAttribute.Rows[i].Cells["objectid1"].Value.ToString());
                    id2List.Add(this.dataGridAttribute.Rows[i].Cells["objectid2"].Value.ToString());
                }
            }
            //string s = "";
            //for (int i = 0; i < id1List.Count; i++) s = s + "\r\n"+id1List[i];
            //MessageBox.Show(s);

            resolveProblem(solution, id1List, id2List);

            //删除已处理冲突
            string layerName = _curFeatureLayer.FeatureClass.AliasName;   //获取冲突显示的图层名
            OracleDBHelper db = new OracleDBHelper();
            //删除数据库中的要素
            //db.deleteDataByList(layerName, "uniqueid", uidList);
            db.deleteDataByList(layerName, "objectid1", id1List);
            db.deleteDataByList(layerName, "objectid2", id2List);
            //deleteDataFromShp("uniqueid",uidList);
            deleteDataFromShp("objectid1", id1List);
            deleteDataFromShp("objectid2", id2List);

            //刷新界面
            InitUI();
            //闪烁显示
            //new System.Threading.Thread(new System.Threading.ThreadStart(flash)).Start();
            highLightShow(id1List, layer1);
            highLightShow(id2List, layer2);
        }

        //删除shp数据中的要素
        public void deleteDataFromShp(string name,ArrayList uidList)
        {
            for (int i = 0; i < uidList.Count; i++)
            {
                IFeatureClass featureClass = _curFeatureLayer.FeatureClass;
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = name +"= '" + (string)uidList[i] + "'";
                IFeatureCursor updateCursor = featureClass.Update(queryFilter, false);
                IFeature feature = updateCursor.NextFeature();
                int m = 0;
                while (feature != null)
                {
                    m++;
                    feature.Delete();
                    feature = updateCursor.NextFeature();
                }
            }
        }

        //闪烁显示
        public void flash()
        {
            IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer1);
            map.Flash(axMapControl1, pFeatLyr, "objectid", "=", id1List);
            //IFeatureLayer pFeatLyr2 = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer2);
            //map.Flash(axMapControl1, pFeatLyr2, "objectid", "=", id2List);
        }

        //高亮显示拓扑冲突的数据集
        public void highLightShow(ArrayList errorList, string layer)
        {
            OperateMap m_OperateMap = new OperateMap();
            IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, layer);
            AddMap map = new AddMap();
            map.showByAttibute(pFeatLyr, "objectid", "=", errorList,255,0,0);
            axMapControl1.Refresh();
        }

        //处理冲突
        public void resolveProblem(string relution,ArrayList id1List, ArrayList id2List)
        {
            TopoErrorSolution sol = new TopoErrorSolution();
            OracleDBHelper db = new OracleDBHelper();
            OracleConnection conn = db.getOracleConnection();
            switch (relution)
            {
                case "不处理":
                    break;
                case "打断点":
                    for (int i = 0; i < id1List.Count; i++)
                    {
                        sol.breakLine(conn, layer1, (string)id1List[i], layer2, (string)id2List[i]);
                    }
                    break;
                case "分开线":
                    for (int i = 0; i < id1List.Count; i++)
                    {
                        sol.cutLine(conn, layer1, (string)id1List[i], layer2, (string)id2List[i]);
                    }
                    break;
                default:
                    break;
            }
        }
        
    }
}
