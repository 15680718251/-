using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Toplogical;
using GIS.Layer;
using GIS.Geometries;
using GIS.TreeIndex.OprtRollBack;
using GIS.Increment;
namespace GIS.TreeIndex.Tool
{
    public class MapDigIslandTool:MapAddGeometryTool
    {
        public MapDigIslandTool(MapUI ui) :
            base(ui)
        {
            ui.OutPutTextInfo("提示： 创建岛屿工具开始，请在目标多边形内创建岛屿！\r\n");
        }         
        public override void Finish()
        { 
            if (m_PtList.Count < 3)
            {
                m_MapUI.OutPutTextInfo("提示：  创建岛屿失败，退出创建岛屿命令\r\n");
                m_MapUI.OutPutTextInfo("提示：  构面点数少于3个，无法创建岛屿\r\n");
                return;
            }
            GeoLinearRing ring = new GeoLinearRing(m_PtList);
            if (!ring.IsCCW())
            {
                ring.Vertices.Reverse();
            }
            bool bok = false;
            for (int i = 0; i < m_MapUI.LayerCounts; i++)
            {
                GeoLayer layer = m_MapUI.GetLayerAt(i);
                if (layer.LayerTypeDetail != LAYERTYPE_DETAIL.PolygonLayer
                    &&layer.LayerTypeDetail != LAYERTYPE_DETAIL.MixLayer
                    &&layer.LayerTypeDetail != LAYERTYPE_DETAIL.DraftLayer)
                    continue;
                GeoVectorLayer actLayer = layer as GeoVectorLayer;
                for (int k = 0; k < actLayer.DataTable.Count; k++)
                {

                  

                    GeoData.GeoDataRow row = actLayer.DataTable[k];
                    if ((int)row.EditState >5||
                        row.Geometry == null || 
                        !row.Geometry.Bound.IsIntersectWith(ring.Bound))
                        continue;
                    GeoPolygon plg = null;
                    if (row.Geometry is GeoPolygon || row.Geometry is GeoMultiPolygon)
                    {
                        ///////////////操作回退
                        GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                        m_MapUI.m_OprtManager.AddOprt(oprts);            
                        ///////////////操作回退
                        GeoData.GeoDataRow rowNew = row.Clone();
                        ((GIS.GeoData.GeoDataTable)row.Table).AddRow(rowNew);
                        if (rowNew.Geometry is GeoMultiPolygon)
                        {
                            GeoMultiPolygon plgs = rowNew.Geometry as GeoMultiPolygon;
                            for (int j = 0; j < plgs.NumGeometries; j++)
                            {
                                if (TpRelatemain.IsTpWith(plgs.Polygons[j], ring, false) == TpRelateConstants.tpContains)
                                {
                                    plg = plgs.Polygons[j];
                                    break;
                                }

                            }
                        }
                        else if (rowNew.Geometry is GeoPolygon)
                        {
                            if (TpRelatemain.IsTpWith((GeoPolygon)rowNew.Geometry, ring, false) == TpRelateConstants.tpContains)
                            {
                                plg = (GeoPolygon)rowNew.Geometry;
                            }
                        }
                        EditState state = row.EditState;
                        plg.InteriorRings.Add(ring);
                        if (row.EditState == EditState.Original)
                        {                           
                            rowNew.EditState = EditState.GeometryAft;
                            row.EditState = EditState.GeometryBef;
                        }
                        else
                        {
                            rowNew.EditState =state;
                            row.EditState = EditState.Invalid;
                        }
                        bok = true;
                        m_MapUI.BoundingBoxChangedBy(row);//重新计算边界矩形      
                        ///////////////操作回退
                        GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                        oprts.m_NewOperands.Add(oprtNew);

                        GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(row, state, row.EditState);
                        oprts.m_OldOperands.Add(oprtOld);
                        ///////////////操作回退

                        break;
                    }
                    else
                        continue;
                }
            }
            if (bok)
            {
                m_MapUI.Refresh();
                m_MapUI.OutPutTextInfo("提示：  创建岛屿成功，退出创建岛屿命令\r\n");
            }
            else
            {
                m_MapUI.RePaint(); 
           
                m_MapUI.OutPutTextInfo("提示：  创建岛屿失败，岛屿不在面内，退出命令\r\n");
            }   
       
        }
        public override void initial()
        {
            base.initial();
            m_MapUI.OutPutTextInfo("提示： 创建岛屿工具开始，请在目标多边形内创建岛屿！\r\n");
        }
    }
}
