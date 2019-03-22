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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            //firest(Time1);

        }
        //string axMapControl;
        public AxMapControl Axmapcontrol
        {
            get { return axMapControl1; }
            set { axMapControl1 = value; }
        }
        string time1;
        public string Time1
        {
            get { return time1; }
            set { time1 = value; }
        }

        string time2;
        public string Time2
        {
            get { return time2; }
            set { time2 = value; }
        }
        string time3;
        public string Time3
        {
            get { return time3; }
            set { time3 = value; }
        }
        string time4;
        public string Time4
        {
            get { return time4; }
            set { time4 = value; }
        }
        string tablename;
        public string Tablename
        {
            get { return tablename; }
            set { tablename = value; }
        }
       
        public Form3(string time1, string time2, string time3, string time4, string tableName)
        {
            InitializeComponent();
            this.Time1 = time1;
            this.Time2 = time2;
            this.Time3 = time3;
            this.Time4 = time4;
            this.Tablename = tableName;
            

        }


        private void Form3_Load(object sender, EventArgs e)
        {
            string path = @"..\..\..\testfile";//获取文件路径
           // string path = "F:\\典型要素更新系统\\testfile";
            string fileName = string.Format("{0}.shp", Tablename); 
            //将所有数据显示在axMapControl1上
            this.axMapControl1.AddShapeFile(path, fileName);
            this.axMapControl1.ActiveView.Refresh();
            this.axMapControl2.AddShapeFile(path, fileName);
            this.axMapControl2.ActiveView.Refresh();
            this.axMapControl3.AddShapeFile(path, fileName);
            this.axMapControl3.ActiveView.Refresh();
            this.axMapControl4.AddShapeFile(path, fileName);
            this.axMapControl4.ActiveView.Refresh();
            AddMap am = new AddMap(axMapControl1, axTOCControl1);
            AddMap am1 = new AddMap(axMapControl2, axTOCControl1);
            AddMap am2 = new AddMap(axMapControl3, axTOCControl1);
            AddMap am3 = new AddMap(axMapControl4, axTOCControl1);
            am.ss();
            am1.ss();
            am2.ss();
            am3.ss();
            //firest(Time1);
            label1.Text = time1;
            label2.Text = time2;
            label3.Text = time3;
            label4.Text = time4;
            //根据时间高亮显示
            selectByTime(Time1);
            firest(Time1, axMapControl1);
            selectByTime2(Time2);
            selectByTime3(Time3);
            selectByTime4(Time4);
        }
        public void selectByTime(string timepick1)
        {

            this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            IQueryFilter pQuery = new QueryFilterClass();
            string time1 = string.Format("'{0}'", timepick1);
            //col = string.Format("\"{0}\"", this.dataGridAttribute.Columns[7].Name);
            //val = string.Format("'{0}'", this.dataGridAttribute.CurrentCell.OwningRow.Cells[7].Value.ToString());
            ILayer pLayer = axMapControl1.get_Layer(0); //pLayer 已经获取到
            Console.WriteLine(pLayer.GetType());
            IFeatureLayer pFLayer = pLayer as IFeatureLayer; //转换失败 pFLayer为空
            IFeatureClass fcls = pFLayer.FeatureClass;
            //int index=pFLayer.FeatureClass.Fields.FindField("OSMID");
            //string startTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(index));
            //string endTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(8));
            //设置高亮要素的查询条件
            pQuery.WhereClause = time1 + "> STARTTIME" + " AND " + time1 + "< ENDTIME";
            //+"and" + time + "<" + endTime;
            //"Name" = '行政区E'
            //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
            //ILayer pLayer = this.axMapControl1.Map.get_Layer(0);
            //IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
            //IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            //IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
            //IFeature pFeature = pFeatureCursor.NextFeature();
            //IGeometry pGeometry = pFeature.Shape;
            //flashshape(pGeometry);
            //firest();
            //IArray geoArray = new ArrayClass();
            //IFeature feature = null;//= pFLayer.Search(pQuery,false) as IFeature;
            //IFeatureCursor fc;
            //IGeometry pGeometry;
            //fc = fcls.Search(pQuery, false);
            //while ((feature = fc.NextFeature()) != null)
            //{
            //    geoArray.Add(feature.ShapeCopy);
            //}
            //feature = fc.NextFeature();
            //pGeometry = feature.ShapeCopy;
            //axMapControl1.FlashShape(pGeometry);

            //if (feature == null) return;
            //if (pFLayer == null) return;
            IFeatureSelection pFeatSelection;
            pFeatSelection = pFLayer as IFeatureSelection;
            pFeatSelection.Clear();
            pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);
            this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        public void firest(string timepick1,AxMapControl axmapcontrol)
        {
            string time1 = string.Format("'{0}'", timepick1);
            IFeatureLayer pFeatureLayer = axmapcontrol.get_Layer(0) as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            IQueryFilter pQFilter = new QueryFilterClass();
            pQFilter.WhereClause = time1 + "> STARTTIME" + " AND " + time1 + "< ENDTIME";
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(pQFilter, false);
            IFeature pFeature = pFeatureCursor.NextFeature();
            IArray pArray = new ArrayClass();
            while (pFeature != null)
            {
                pArray.Add(pFeature.ShapeCopy);
                pFeature = pFeatureCursor.NextFeature();
            }
            HookHelperClass pHook = new HookHelperClass();
            pHook.Hook = axmapcontrol.Object;
            IHookActions pHookActions = pHook as IHookActions;
            pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsPan);
            Application.DoEvents();
            pHook.ActiveView.ScreenDisplay.UpdateWindow();
            for (int i = 0; i < 5; i++)
            {
                pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsFlash);
                System.Threading.Thread.Sleep(300);
            }
        }

        private bool bCanDrag;              //鹰眼地图上的矩形框可移动的标志
        private IPoint pMoveRectPoint;      //记录在移动鹰眼地图上的矩形框时鼠标的位置
        private IEnvelope pEnv;
        private void axMapControl2_OnMouseDown(object sender,IMapControlEvents2_OnMouseDownEvent e)
        {

            if (e.button == 1 && pMoveRectPoint != null)
            {
                if (e.mapX == pMoveRectPoint.X && e.mapY == pMoveRectPoint.Y)
                {
                    axMapControl1.CenterAt(pMoveRectPoint);
                }
                bCanDrag = false;
            }
        }
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            firest(Time1, axMapControl1);
        }
        private void axMapControl1_OnMouseDown_2(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            firest(Time1, axMapControl1);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
           //IFeatureLayer pFLayer = this.axMapControl1.get_Layer(0) as IFeatureLayer;
           // AddMap am = new AddMap();
           // am.Flash(axMapControl1,pFLayer, "STARTTIME", "<", Time1);
            firest(Time1,axMapControl1);
            firest(Time2, axMapControl2);
            firest(Time3, axMapControl3);
            firest(Time4, axMapControl4);
        }

        //  public IEnvelope pEnvelop { get; set; }
             public void selectByTime2(string timepick2)
             {
                 this.axMapControl2.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                 IQueryFilter pQuery = new QueryFilterClass();
                 string time2 = string.Format("'{0}'", timepick2);
                 //col = string.Format("\"{0}\"", this.dataGridAttribute.Columns[7].Name);
                 //val = string.Format("'{0}'", this.dataGridAttribute.CurrentCell.OwningRow.Cells[7].Value.ToString());
                 ILayer pLayer = axMapControl2.get_Layer(0); //pLayer 已经获取到
                 Console.WriteLine(pLayer.GetType());
                 IFeatureLayer pFLayer = pLayer as IFeatureLayer; //转换失败 pFLayer为空
                 //int index=pFLayer.FeatureClass.Fields.FindField("OSMID");
                 //string startTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(index));
                 //string endTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(8));
                 //设置高亮要素的查询条件
                 pQuery.WhereClause = time2 + "> STARTTIME" + " AND " + time2 + "< ENDTIME";
                 //+"and" + time + "<" + endTime;
                 //"Name" = '行政区E'
                 //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
                 if (pFLayer == null) return;
                 IFeatureSelection pFeatSelection;
                 pFeatSelection = pFLayer as IFeatureSelection;
                 pFeatSelection.Clear();
                 pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);
                 this.axMapControl2.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
             }
             public void selectByTime3(string timepick3)
             {
                 this.axMapControl3.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                 IQueryFilter pQuery = new QueryFilterClass();
                 string time3 = string.Format("'{0}'", timepick3);
                 //col = string.Format("\"{0}\"", this.dataGridAttribute.Columns[7].Name);
                 //val = string.Format("'{0}'", this.dataGridAttribute.CurrentCell.OwningRow.Cells[7].Value.ToString());
                 ILayer pLayer = axMapControl3.get_Layer(0); //pLayer 已经获取到
                 Console.WriteLine(pLayer.GetType());
                 IFeatureLayer pFLayer = pLayer as IFeatureLayer; //转换失败 pFLayer为空
                 //int index=pFLayer.FeatureClass.Fields.FindField("OSMID");
                 //string startTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(index));
                 //string endTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(8));
                 //设置高亮要素的查询条件
                 pQuery.WhereClause = time3 + "> STARTTIME" + " AND " + time3 + "< ENDTIME";
                 //+"and" + time + "<" + endTime;
                 //"Name" = '行政区E'
                 //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
                 if (pFLayer == null) return;
                 IFeatureSelection pFeatSelection;
                 pFeatSelection = pFLayer as IFeatureSelection;
                 pFeatSelection.Clear();
                 pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);
                 this.axMapControl3.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
             }
             public void selectByTime4(string timepick4)
             {
                 this.axMapControl4.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                 IQueryFilter pQuery = new QueryFilterClass();
                 string time4 = string.Format("'{0}'", timepick4);
                 //col = string.Format("\"{0}\"", this.dataGridAttribute.Columns[7].Name);
                 //val = string.Format("'{0}'", this.dataGridAttribute.CurrentCell.OwningRow.Cells[7].Value.ToString());
                 ILayer pLayer = axMapControl4.get_Layer(0); //pLayer 已经获取到
                 Console.WriteLine(pLayer.GetType());
                 IFeatureLayer pFLayer = pLayer as IFeatureLayer; //转换失败 pFLayer为空
                 //int index=pFLayer.FeatureClass.Fields.FindField("OSMID");
                 //string startTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(index));
                 //string endTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(8));
                 //设置高亮要素的查询条件
                 pQuery.WhereClause = time4 + "> STARTTIME" + " AND " + time4 + "< ENDTIME";
                 //+"and" + time + "<" + endTime;
                 //"Name" = '行政区E'
                 //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
                 if (pFLayer == null) return;
                 IFeatureSelection pFeatSelection;
                 pFeatSelection = pFLayer as IFeatureSelection;
                 pFeatSelection.Clear();
                 pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);
                 this.axMapControl4.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);  
          }

             private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
             {
                 if (axMapControl2.LayerCount > 0)
                 {
                     axMapControl2.ClearLayers();
                 }
                 //设置鹰眼和主地图的坐标系统一致
                 axMapControl2.SpatialReference = axMapControl1.SpatialReference;
                 for (int i = axMapControl1.LayerCount - 1; i >= 0; i--)
                 {
                     //使鹰眼视图与数据视图的图层上下顺序保持一致
                     ILayer pLayer = axMapControl1.get_Layer(i);
                     if (pLayer is IGroupLayer || pLayer is ICompositeLayer)
                     {
                         ICompositeLayer pCompositeLayer = (ICompositeLayer)pLayer;
                         for (int j = pCompositeLayer.Count - 1; j >= 0; j--)
                         {
                             ILayer pSubLayer = pCompositeLayer.get_Layer(j);
                             IFeatureLayer pFeatureLayer = pSubLayer as IFeatureLayer;
                             if (pFeatureLayer != null)
                             {
                                 //由于鹰眼地图较小，所以过滤点图层不添加
                                 if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint
                                     && pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryMultipoint)
                                 {
                                     axMapControl2.AddLayer(pLayer);
                                 }
                             }
                         }
                     }
                     else
                     {
                         IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
                         if (pFeatureLayer != null)
                         {
                             if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint
                                 && pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryMultipoint)
                             {
                                 axMapControl2.AddLayer(pLayer);
                             }
                         }
                     }
                     //设置鹰眼地图全图显示  
                     axMapControl2.Extent = axMapControl1.FullExtent;
                     pEnv = axMapControl1.Extent as IEnvelope;
                     DrawRectangle(pEnv);
                     axMapControl2.ActiveView.Refresh();
                 }
             }

             private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
             {
                 pEnv = (IEnvelope)e.newEnvelope;
                 DrawRectangle(pEnv);
             }
             private void DrawRectangle(IEnvelope pEnvelope)
             {
                 //在绘制前，清除鹰眼中之前绘制的矩形框
                 IGraphicsContainer pGraphicsContainer = axMapControl2.Map as IGraphicsContainer;
                 IActiveView pActiveView = pGraphicsContainer as IActiveView;
                 pGraphicsContainer.DeleteAllElements();
                 //得到当前视图范围
                 IRectangleElement pRectangleElement = new RectangleElementClass();
                 IElement pElement = pRectangleElement as IElement;
                 pElement.Geometry = pEnvelope;
                 //设置矩形框（实质为中间透明度面）
                 IRgbColor pColor = new RgbColorClass();
                 pColor = GetRgbColor(255, 0, 0);
                 pColor.Transparency = 255;
                 ILineSymbol pOutLine = new SimpleLineSymbolClass();
                 pOutLine.Width = 1;
                 pOutLine.Color = pColor;
                 IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
                 pColor = new RgbColorClass();
                 pColor.Transparency = 0;
                 pFillSymbol.Color = pColor;
                 pFillSymbol.Outline = pOutLine;
                 //向鹰眼中添加矩形框
                 IFillShapeElement pFillShapeElement = pElement as IFillShapeElement;
                 pFillShapeElement.Symbol = pFillSymbol;
                 pGraphicsContainer.AddElement((IElement)pFillShapeElement, 0);
                 //刷新
                 pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
             }

             private void axMapControl2_OnMouseDown_1(object sender, IMapControlEvents2_OnMouseDownEvent e)
             {
                 if (axMapControl2.Map.LayerCount > 0)
                 {
                     //按下鼠标左键移动矩形框
                     if (e.button == 1)
                     {
                         //如果指针落在鹰眼的矩形框中，标记可移动
                         if (e.mapX > pEnv.XMin && e.mapY > pEnv.YMin && e.mapX < pEnv.XMax && e.mapY < pEnv.YMax)
                         {
                             bCanDrag = true;
                         }
                         pMoveRectPoint = new PointClass();

                         pMoveRectPoint.PutCoords(e.mapX, e.mapY);  //记录点击的第一个点的坐标
                     }
                     //按下鼠标右键绘制矩形框
                     else if (e.button == 2)
                     {
                         IEnvelope pEnvelope = axMapControl2.TrackRectangle();

                         IPoint pTempPoint = new PointClass();
                         pTempPoint.PutCoords(pEnvelope.XMin + pEnvelope.Width / 2, pEnvelope.YMin + pEnvelope.Height / 2);
                         axMapControl1.Extent = pEnvelope;
                         //矩形框的高宽和数据试图的高宽不一定成正比，这里做一个中心调整
                         axMapControl1.CenterAt(pTempPoint);
                     }
                 }
             }

             private void axMapControl2_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
             {
                 if (e.mapX > pEnv.XMin && e.mapY > pEnv.YMin && e.mapX < pEnv.XMax && e.mapY < pEnv.YMax)
                 {
                     //如果鼠标移动到矩形框中，鼠标换成小手，表示可以拖动
                     axMapControl2.MousePointer = esriControlsMousePointer.esriPointerHand;
                     if (e.button == 2)  //如果在内部按下鼠标右键，将鼠标演示设置为默认样式
                     {
                         axMapControl2.MousePointer = esriControlsMousePointer.esriPointerDefault;
                     }
                 }
                 else
                 {
                     //在其他位置将鼠标设为默认的样式
                     axMapControl2.MousePointer = esriControlsMousePointer.esriPointerDefault;
                 }

                 if (bCanDrag)
                 {
                     double Dx, Dy;  //记录鼠标移动的距离
                     Dx = e.mapX - pMoveRectPoint.X;
                     Dy = e.mapY - pMoveRectPoint.Y;
                     pEnv.Offset(Dx, Dy); //根据偏移量更改 pEnv 位置
                     pMoveRectPoint.PutCoords(e.mapX, e.mapY);
                     DrawRectangle(pEnv);
                     axMapControl1.Extent = pEnv;
                 }
             }

             private void axMapControl2_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
             {
                 if (e.button == 1 && pMoveRectPoint != null)
                 {
                     if (e.mapX == pMoveRectPoint.X && e.mapY == pMoveRectPoint.Y)
                     {
                         axMapControl1.CenterAt(pMoveRectPoint);
                     }
                     bCanDrag = false;
                 }
             }
             private IRgbColor GetRgbColor(int intR, int intG, int intB)
             {
                 IRgbColor pRgbColor = null;
                 if (intR < 0 || intR > 255 || intG < 0 || intG > 255 || intB < 0 || intB > 255)
                 {
                     return pRgbColor;
                 }
                 pRgbColor = new RgbColorClass();
                 pRgbColor.Red = intR;
                 pRgbColor.Green = intG;
                 pRgbColor.Blue = intB;
                 return pRgbColor;
             }

             //private void axMapControl3_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
             //{
             //    firest(Time3, axMapControl3);
             //}

             //private void axMapControl3_OnAfterDraw(object sender, IMapControlEvents2_OnAfterDrawEvent e)
             //{
             //    firest(Time3, axMapControl3);
             //}
    }
}