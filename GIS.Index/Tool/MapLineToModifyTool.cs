using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Geometries;
using GIS.Map;
using GIS.Layer;
using GIS.TreeIndex.Forms;
using GIS.TreeIndex.OprtRollBack;

namespace GIS.TreeIndex.Tool
{
    public class MapLineToModifyTool : MapGenerateLineTool
    {
        public MapLineToModifyTool(MapUI ui)
            : base(ui)
        {
            initial();
        }
        private LineToModifyState m_State = LineToModifyState.State_SelectEditObj;
        private GeoData.GeoDataRow m_EditObject = null;
        public override void initial()
        {
            m_State = LineToModifyState.State_SelectEditObj;
            m_EditObject = null;
            m_MapUI.ClearAllSlt();
            m_MapUI.OutPutTextInfo("提示：线条修改任意对象工具开始，请先选择被编辑的对象，确认后按右键!\r\n");
            base.initial();
        }
        private enum LineToModifyState
        {
            State_SelectEditObj,//选择被编辑对象截断
            State_AddLine, //画线 
        }
        private void RenderPolygon(Graphics g, Brush brush, Pen pen, Font font,GeoPolygon plg)
        {
            RenderLine(g, brush, pen, font, plg.ExteriorRing);
            for (int i = 0; i < plg.InteriorRings.Count; i++)
            {
                RenderLine(g, brush, pen, font, plg.InteriorRings[i]);
            }
        }
        private void RenderLine(Graphics g, Brush brush, Pen pen, Font font, GeoLineString line)
        {    
            Point pt = m_MapUI.TransFromWorldToMap(line.StartPoint);
            g.DrawEllipse(pen, pt.X - 7, pt.Y - 7, 14, 14);
            g.DrawString("(起点)", font, brush, pt);
        }
        private void RenderMultiLines(Graphics g, Brush brush, Pen pen, Font font, GeoMultiLineString lines)
        {
            for (int i = 0; i < lines.NumGeometries; i++)
            {
                RenderLine(g, brush, pen, font, lines.LineStrings[i]);
            }
        }
        private void RenderMultiPolygon(Graphics g, Brush brush, Pen pen, Font font, GeoMultiPolygon plgs)
        {
            for (int i = 0; i < plgs.NumGeometries; i++)
            {
                RenderPolygon(g, brush, pen, font, plgs.Polygons[i]);
            }
        }
 
        private void RenderGeometry()
        {
            if (m_EditObject != null)
            {
                try
                {
                    Graphics g = Graphics.FromImage(m_MapUI.m_ImgBackUp);

                    Brush brush = new SolidBrush(Color.Black);
                    Pen pen = new Pen(Color.Black);
                    System.Drawing.Font font = new Font("Times New Roman", 15, GraphicsUnit.Pixel);
                    if (m_EditObject.Geometry is GeoLineString)
                    {
                        RenderLine(g, brush, pen, font, m_EditObject.Geometry as GeoLineString);
                    }
                    else if (m_EditObject.Geometry is GeoPolygon)
                    {
                        RenderPolygon(g, brush, pen, font, m_EditObject.Geometry as GeoPolygon);
                    }
                    else if (m_EditObject.Geometry is GeoMultiLineString)
                    {
                        RenderMultiLines(g, brush, pen, font, m_EditObject.Geometry as GeoMultiLineString);
                    }
                    else if (m_EditObject.Geometry is GeoMultiPolygon)
                    {
                        RenderMultiPolygon(g, brush, pen, font, m_EditObject.Geometry as GeoMultiPolygon);
                    }
                    brush.Dispose();
                    g.Dispose();
                    m_MapUI.RePaint();
                }
                catch
                {
                }
            }
           
        }
        public override void Cancel()
        {
            if (m_State == LineToModifyState.State_SelectEditObj)
            {
                if (m_MapUI.SltGeoSet.Count == 1)
                {
                    GeoData.GeoDataRow row = m_MapUI.SltGeoSet[0];
                    if (!(row.Geometry is GeoLabel) && !(row.Geometry is GeoPoint))
                    {
                        m_EditObject = row;
                        m_State = LineToModifyState.State_AddLine;
                        m_MapUI.OutPutTextInfo("提示：目标选择完成，请绘制线条！右键结束\r\n");
                        RenderGeometry();
                    }
                }
            }
            else
            {
                if (m_CurEditType == EditType.AddOnePoint)
                {
                    if (m_PtList.Count >= 2)
                    {
                        LineToChangeGeometry();
                        m_MapUI.OutPutTextInfo("提示：  退出线条任意修改命令\r\n"); 
                        m_MapUI.m_EditToolBack = m_MapUI.MapTool;
                        m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
                    }
                    else
                    {
                        m_MapUI.OutPutTextInfo("提示：  构线点数少于2个，无法完成线的绘制\r\n");
                    }

                }
                else
                    base.Cancel();
            }
        }
        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_State == LineToModifyState.State_SelectEditObj)
                {
                    GeoData.GeoDataRow row = m_MapUI.SelectByPt(e.Location);
                    if (row != null)
                        m_MapUI.Refresh();
                }
                else
                {
                    RenderGeometry();
                    base.OnMouseDown(sender, e);
                }

                
            }
            else
            {
                Cancel();
            }
        }
        private void LineToChangeGeometry()
        {
            Geometry DestGeom = null;
            if (m_EditObject.Geometry is GeoLineString)
            {
                GeoLineString line = m_EditObject.Geometry as GeoLineString;
                DestGeom = SpatialRelation.GeoAlgorithm.LineModifiedByLine(line, m_PtList);        
            }
            else if (m_EditObject.Geometry is GeoPolygon)
            {
                GeoPolygon plg = m_EditObject.Geometry as GeoPolygon;
                DestGeom  = SpatialRelation.GeoAlgorithm.PolygonModifiedByLine(plg, m_PtList); 
            }
            else if (m_EditObject.Geometry is GeoMultiLineString)
            {
                GeoMultiLineString lines = m_EditObject.Geometry as GeoMultiLineString;
                DestGeom = SpatialRelation.GeoAlgorithm.MultiLineModifiedByLine(lines, m_PtList);
            }
            else if (m_EditObject.Geometry is GeoMultiPolygon)
            {
                GeoMultiPolygon plgs = m_EditObject.Geometry as GeoMultiPolygon;
                DestGeom = SpatialRelation.GeoAlgorithm.MultiPolygonModifiedByLine(plgs, m_PtList);
            }
            if (DestGeom != null)
            {
                GIS.GeoData.GeoDataRow NewRow = m_EditObject.Clone();
                ((GIS.GeoData.GeoDataTable)m_EditObject.Table).AddRow(NewRow);
                EditState state = m_EditObject.EditState;
                if (m_EditObject.EditState == EditState.Original)
                {
                    m_EditObject.EditState = EditState.GeometryBef;
                    NewRow.EditState = EditState.GeometryAft;
                }
                else
                {
                    m_EditObject.EditState = EditState.Invalid;
                    NewRow.EditState = state;
                }
                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                m_MapUI.m_OprtManager.AddOprt(oprts);
                ///////////////操作回退
                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(NewRow, EditState.Invalid, NewRow.EditState);
                oprts.m_NewOperands.Add(oprtNew);

                GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(m_EditObject, state, m_EditObject.EditState);
                oprts.m_OldOperands.Add(oprtOld);
                ///////////////操作回退

                NewRow.Geometry = DestGeom;
                m_MapUI.Refresh();
            }
        }
    }
}
