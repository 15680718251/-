using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Map;
using GIS.Geometries;
using GIS.Utilities;
using GIS.TreeIndex.Tool;
using GIS.SpatialRelation;
using GIS.Layer;
using System.IO;
using System.Threading;
using GIS.mm_Conv_Symbol;
namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    {

        /// <summary>
        /// 设置当前绘图工具
        /// </summary>
        public MapTool MapTool
        {
            get
            {
                return m_Tool;
            }
            set
            {
                if (m_Tool != null)
                    m_Tool.Finish();
                m_Tool = value;
                Cursor = m_Tool.ToolCursor;
            }
        }
        public void Pan()
        {
           MapTool = new MapPanTool(this);
        }

        public void ZoomIn()
        {
            MapTool = new MapZoomTool(this, ZoomType.ZoomIn);
        }

        public void ZoomOut()
        {
            MapTool = new MapZoomTool(this, ZoomType.ZoomOut);
        }

        public void SelectByPt()
        {
            MapTool = new MapMoveNodeTool(this);
        }

        public void ToolMoveNode()
        {
            MapTool = new MapMoveNodeTool(this);
        }
        public void ToolDeleteObj()
        {
            //如果没有选中目标，则激活删除工具
            MapTool = new MapDeleteTool(this);
        }
        public void ToolAddLine()
        {
           MapTool = new MapAddLineStringTool(this);   
        }
        public void ToolAddPolygon()
        {
            MapTool = new MapAddPolygonTool(this);
        }
        public void ToolAddPoint()
        {
            MapTool = new MapAddPointTool(this);
        }
        public void ToolLineContinue()
        {
            MapTool = new MapLineContinueTool(this);
        }
        public void ToolAddLabel()
        {
            MapTool = new MapAddLabelTool(this);
        } 

        public void AttributeEdit()
        {
            MapTool = new MapAttributeEditTool(this);
        }
  
        public void ToolLineSmooth()
        {
            MapTool = new MapLineSmoothTool(this);
        }

        public void ToolLineBomb()
        {
            if (SltGeoSet.Count != 1 ||
                                      (!(SltGeoSet[0].Geometry is GeoLineString) &&
                                        !(SltGeoSet[0].Geometry is GeoMultiLineString)))
            {
                OutPutTextInfo("请选择一条要炸开的线，可以是单线也可以是多线\r\n");
            }
            else
            {
                GeoData.GeoDataRow row = SltGeoSet[0];
                GeoVectorLayer layer = GetLayerByTable(((GeoData.GeoDataTable)row.Table)) as GeoVectorLayer;
                List<GeoLineString> m_Lines = null;
                if (SltGeoSet[0].Geometry is GeoLineString)
                {
                    GeoLineString line = SltGeoSet[0].Geometry as GeoLineString;
                    m_Lines = line.Bomb();
                }
                else
                {
                    GeoMultiLineString lines = SltGeoSet[0].Geometry as GeoMultiLineString;
                    m_Lines = lines.Bomb();

                }
                List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                m_OprtManager.AddOprt(oprts);
                ///////////////操作回退
                for (int i = 0; i < m_Lines.Count; i++)
                {
                    GeoData.GeoDataRow rowNew = layer.AddGeometry(m_Lines[i]);
                    InitialNewGeoFeature(rowNew);
                    m_list.Add(rowNew);
                    rowNew.EditState = EditState.Appear;
                    GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                    oprts.m_NewOperands.Add(oprtNew);
                }
                EditState state = row.EditState;
                if (row.EditState == EditState.Original)
                    row.EditState = EditState.Disappear;
                else
                    row.EditState = EditState.Invalid;

                GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(row, state, row.EditState);
                oprts.m_OldOperands.Add(oprtOld);
                if (m_list.Count > 0)
                {                 
                    OutPutTextInfo(string.Format(" 直线炸开成功，产生了{0}条新线段\r\n", m_Lines.Count));
                }
                else
                {
                    OutPutTextInfo(string.Format(" 直线炸开失败，线段只有2个点，不能炸开啦\r\n", m_Lines.Count));
                }
                ClearAllSlt();
            }
        }
        public void ToolMultiToSingles()
        {
            int nCount = 0;
            for (int i = 0; i < SltGeoSet.Count; i++)
            {
                GeoData.GeoDataRow row = SltGeoSet[i];              
                if (row.Geometry is GeometryCollection)
                {
                    GeoVectorLayer layer = GetLayerByTable(((GIS.GeoData.GeoDataTable)row.Table)) as GeoVectorLayer;
                    GeometryCollection geoms = row.Geometry as GeometryCollection;
                    nCount++;
                    for (int j = 0; j < geoms.NumGeometries; j++)
                    {
                        Geometry geom = null;
                        if (geoms is GeoMultiLineString)
                        {
                            geom = ((GeoMultiLineString)geoms).LineStrings[j];                            
                        }
                        else if (geoms is GeoMultiPoint)
                        {
                            geom = ((GeoMultiPoint)geoms).Points[j];
                        }
                        else if (geoms is GeoMultiPolygon)
                        {
                            geom = ((GeoMultiPolygon)geoms).Polygons[j];
                        }
                        GeoData.GeoDataRow newRow = layer.AddGeometry(geom);
                        newRow.EditState = EditState.Appear;
                    }
                    row.EditState = EditState.Disappear;
                }                
            }
            if (nCount == 0)
            {
                OutPutTextInfo("提示：没有选择组合目标，请选中组合目标后再进行此项功能！\r\n");
            }
            else
            {
                OutPutTextInfo(string.Format("提示：拆分成功，共拆分{0}个组合目标！\r\n",nCount));
            }
        }
        public void ToolLinesToMultiLine()
        {
            GeoLayer lyrFirst = null;
            List<GeoData.GeoDataRow> m_list = new List<GeoData.GeoDataRow>();
      
            for (int i = 0; i < SltGeoSet.Count; i++)
            {
                GeoData.GeoDataRow row = SltGeoSet[i];
                
                if (row.Geometry is GeoLineString)
                { 
                    GeoLayer layer = GetLayerByTable((GeoData.GeoDataTable)row.Table);
                    if(lyrFirst ==null)
                    {
                        lyrFirst = layer;
                    }
                    else 
                    {
                        if(lyrFirst != layer)
                        {
                            OutPutTextInfo(" 提示：所选线段不在同一图层， 合并失败！\r\n");
                            return;
                        }
                    }

                    m_list.Add(row);
                }
            }
            if (m_list.Count < 2)
            {
                OutPutTextInfo("提示： 所选线段个数少于2个，不能完成组合\r\n");
                return;
            }
             GeoMultiLineString line = new GeoMultiLineString();
           
            for (int i = 0; i < m_list.Count; i++)
            {
                GeoData.GeoDataRow row = m_list[i];       
             
               // row.EditState = EditState.UnionBef;
                line.LineStrings.Add(row.Geometry as GeoLineString);
            }

            GeoData.GeoDataRow newRow = ((GeoVectorLayer)lyrFirst).AddGeometry(line);
         //   newRow.EditState = EditState.UnionAft;
            OutPutTextInfo("提示： 线段组合成功！\r\n");
            ClearAllSlt();
        }

        public void ToolLineToPolygon()
        {
            List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
            for (int i = 0; i < SltGeoSet.Count; i++)
            {
                if (SltGeoSet[i].Geometry is GeoLineString)
                {
                    GeoLineString line = SltGeoSet[i].Geometry as GeoLineString;
                    if (line.IsClosed)
                        m_list.Add(SltGeoSet[i]);
                }
            }
            if (m_list.Count == 0)
            {
                OutPutTextInfo("提示： 没有封闭的线段，不能转成多边形\r\n");
                return;
            }
            else
            {
                GIS.TreeIndex.Forms.LineToPolygonForm form = new GIS.TreeIndex.Forms.LineToPolygonForm(this);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    GeoVectorLayer lyr = GetLayerByName(form.LayerName) as GeoVectorLayer;
                   
                    if (lyr != null)
                    {
                        for(int i =0 ;i<m_list.Count;i++)
                        {
                            GeoLinearRing line = new GeoLinearRing((m_list[i].Geometry as GeoLineString).Vertices);
                            GeoPolygon plg = new GeoPolygon(line);
                            GeoData.GeoDataRow newRow = lyr.AddGeometry(plg);
                            newRow.EditState = EditState.Appear;
                            m_list[i].EditState = EditState.Disappear;
                        }
                        OutPutTextInfo(string.Format("提示： 完成{0}个多边形构建\r\n",m_list.Count));
                        ClearAllSlt();
                       
                    }
                    else
                        OutPutTextInfo("提示： 没有选择目标面层\r\n");
                }
            }
        }

        public void ToolSpline()
        {
            MapTool = new MapAddSplineTool(this);
        }
        public void ToolArc()
        {
            MapTool = new MapAddArcTool(this, ArcType.Arc);
        }
        public void ToolDigIsland()
        {
            MapTool = new MapDigIslandTool(this);
        }
        public void ToolMesureLength()
        {
            MapTool =new  MapMesureLengthTool(this);
        }
        public void ToolMesureArea()
        {
            MapTool = new MapMesureAreaTool(this);
        }

        public void ToolLineModify()
        {
            MapTool = new MapLineModifyTool(this);
        }
        public void ToolRegionSelect( )
        {
            MapTool = new MapRegionSelectTool(this, MapRegionSelectTool.SelectType.RegionSelect);
        }
        public void ToolRectSelect()
        {
            MapTool = new MapRegionSelectTool(this, MapRegionSelectTool.SelectType.RectSelect);
        }

        public void ToolLineToArc()
        {
            MapTool = new MapLineToArcTool(this); 
        }

        public void ToolSplineModify()
        {
            MapTool = new MapSplineModifyTool(this);
        }


        public void ToolDeleteNode()
        {
            MapTool = new MapNodeDeleteTool(this);
        }

        public void ToolCircle()
        {
            MapTool = new MapAddArcTool(this, ArcType.Circle);
        }
        public void ToolArcModify()
        {
            MapTool = new MapArcModifyTool(this);
        }

        public void ToolMoveGeometry()
        {
            MapTool = new MapMoveGeomtryTool(this);
        }
        public void ToolRotateGeometry()
        {
            MapTool = new MapRotateGeometryTool(this);
        }

        public void ToolCopyGeometry()
        {
            MapTool = new MapCopyGeometryTool(this);
        }        

        public void ToolMirrorImage()
        {
            MapTool = new MapMirrorImageTool(this);
        }
        public void ToolSpiltLine()
        {
            MapTool = new MapSpiltLineTool(this);
        }

        public void ToolLineParallel()
        {
            MapTool = new MapLineParallelTool(this);
        }
        public void ToolMoveLabel()
        {
            MapTool = new MapMoveLabelTool(this);
        }

        public void ToolRotateLabel()
        {
            MapTool = new MapRotateLabelTool(this);
        }
        public void ToolLabelStretch()
        {
            MapTool = new MapStretchLabelTool(this);
        }
        public void ToolOneLabelSetting()
        {
            MapTool = new MapOneLabelSettingTool(this);
        }

        public void ToolAddRectangle()
        {
            MapTool = new MapAddRectangleTool(this);
        }
        public void ToolLineExtent()
        {
            MapTool = new MapLineExtentTool(this);
        }
        public void ToolPointSelect()
        {
            MapTool = new MapPointSelectTool(this);
        }

        public void ToolDeleteLabel()
        {
            MapTool = new MapDeleteLabelTool(this);
        }

        public void ToolLineToModify()
        {
            MapTool = new MapLineToModifyTool(this);
        }
 
        public void ToolLineSplitLine()
        {
            MapTool = new MapLineSplitTool(this);
        }

        public void ToolCreatPolygon()
        {
            MapTool = new MapCreatPolygonTool(this);
        }
        public void ToolPolyCombine()
        {
            MapTool = new MapPolygonCombineTool(this);
        }
        public void ToolPolygonIntersect()
        {
            MapTool = new MapPolygonIntersectTool(this);
        }
        public void ToolPolygonSubtract()
        {
            MapTool = new MapPolygonSubtractTool(this);
        }
        public void ToolPolygonSplit()
        {
            MapTool = new MapPolygonSplitTool(this);
        }
        public void ToolPolygonToLine()
        {
            MapTool = new MapPolygontoLineTool(this);
        }
        public void ToolTransformLayer()
        {
            m_Tool = new GIS.TreeIndex.Tool.MapLayerTransformTool(this);
        }
        public void ToolRasterToVector()
        {
            MapTool = new MapRasterToVectorTool(this);
        }
        public void ToolAreaQuery()
        {
            MapTool = new MapAreaQueryTool(this);
        }
        public void ToolDrawBufferLine()
        {
            MapTool = new MapBufferTool(this);
        }
        public void ToolChooseBufferLine()
        {
            MapTool = new MapChooseBufferTool(this);
        }
        public void ToolAttributeQuery()
        {
            MapTool = new MapAttributeQueryTool(this);
        }
        public void VectorUpdate()
        {
            MapTool = new MapVectorUpdate(this);
        }
        public void ToolRemoveHoles()
        {
            MapTool = new MapRemoveHolesTool(this);
        }
    }
}
