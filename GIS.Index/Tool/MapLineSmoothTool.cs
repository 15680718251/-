using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.GeoData;
using GIS.Geometries;
using GIS.Layer;
using GIS.Style;
using GIS.SpatialRelation;
using GIS.TreeIndex.Forms;
using GIS.TreeIndex.OprtRollBack;
namespace GIS.TreeIndex.Tool
{
    public class MapLineSmoothTool : MapTool
    {
        public MapLineSmoothTool(MapUI ui)
            : base(ui)
        {
            initial();
        }
        private void LineSmooth()
        {
            if (m_MapUI. SltGeoSet.Count > 0)
            {
                ///////////////操作回退
                  OperandList oprts = new OperandList();
                m_MapUI.m_OprtManager.AddOprt(oprts);
                ///////////////操作回退
                for (int j = 0; j < m_MapUI.SltGeoSet.Count; j++)
                {
                    List<GeoPoint> pts = null;
                    GeoDataRow row = m_MapUI.SltGeoSet[j];
                    Geometry geom =null;
                    GeoDataRow NewRow = row.Clone();
                    geom = NewRow.Geometry;
                    if (geom is GeoLineString || geom is GeoPolygon || geom is GeoMultiLineString)
                    {

                        ((GeoDataTable)row.Table).AddRow(NewRow);
                        EditState state = row.EditState;

                        if (row.EditState == EditState.Original)
                        {
                            NewRow.EditState = EditState.GeometryAft;
                            row.EditState = EditState.GeometryBef;

                        }
                        else
                        {
                            row.EditState = EditState.Invalid;
                            NewRow.EditState = state;
                        }
                        ///////////////操作回退
                        Operand oprtNew = new Operand(NewRow, EditState.Invalid, NewRow.EditState);
                        oprts.m_NewOperands.Add(oprtNew);

                        Operand oprtOld = new Operand(row, state, row.EditState);
                        oprts.m_OldOperands.Add(oprtOld);
                        ///////////////操作回退
                    }
                    else continue;
                    if (geom is GeoLineString)
                    {
                        GeoLineString line = geom as GeoLineString;
                        pts =  GeoAlgorithm.CubicSpline(line.Vertices);
                        if(pts !=null)
                            line.Vertices = pts;  
                    }
                    else if (geom is GeoMultiLineString)
                    {
                        GeoMultiLineString lines = geom as GeoMultiLineString;
                        for (int i = 0; i < lines.NumGeometries; i++)
                        {
                            GeoLineString Line = lines[i];
                            pts = GeoAlgorithm.CubicSpline(Line.Vertices); 
                            if(pts !=null)
                                Line.Vertices = pts;  
                        }
                    }
                    else if (geom is GeoPolygon)
                    {
                        GeoPolygon plg = geom as GeoPolygon;
                        plg.ExteriorRing.Vertices = GeoAlgorithm.CubicSpline(plg.ExteriorRing.Vertices);
                        for (int i = 0; i < plg.InteriorRings.Count; i++)
                        {
                            GeoLinearRing ring = plg.InteriorRings[i];
                            pts = GeoAlgorithm.CubicSpline(ring.Vertices);
                            if(pts != null)
                             ring.Vertices = pts;
                        }
                    }
                }
                m_MapUI.OutPutTextInfo(string.Format("提示：已经圆滑{0}个几何目标，右键结束，左键继续选择需要圆滑的对象!\r\n", m_MapUI.SltGeoSet.Count));
                m_MapUI.ClearAllSlt();
                
            }
        }

        public override void initial()
        { 
            if (m_MapUI.SltGeoSet.Count > 0)
            {
                LineSmooth(); 
            }
            else
            {
                m_MapUI.OutPutTextInfo("提示：曲线圆滑功能开始，请选择目标后按右键确认圆滑,退出直接按右键!\r\n");
            }
        }
        public override void Cancel()
        {
            if (m_MapUI.SltGeoSet.Count > 0)
            {
                LineSmooth(); 
            }
            else
            {
                m_MapUI.OutPutTextInfo("提示：曲线圆滑工具结束！\r\n");
                base.Cancel();
            }
        }
        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GeoDataRow row = m_MapUI.SelectByPt(e.Location, SelectType.Geomtry);                
                m_MapUI.Refresh();
                m_MapUI.OutPutTextInfo(string.Format("提示：已经选中{0}个几何目标，请按右键确认圆滑!\r\n", m_MapUI.SltGeoSet.Count));
            }
            else if (e.Button == MouseButtons.Right)
            { 
                Cancel();
            }
        }
    }

}
