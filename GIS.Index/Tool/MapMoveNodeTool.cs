using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.GeoData;
using GIS.Geometries;
using GIS.Layer;
namespace GIS.TreeIndex.Tool
{
    public class MapMoveNodeTool:MapTool
    {
        protected MapMoveNodeTool() 
        { 
        }
        public MapMoveNodeTool(MapUI ui)
            :base(ui)
        {
            m_Cursor = Cursors.Cross;
        }

        protected bool bMouseDowing = false;
        protected Point m_DragPoint;              //拖拽前的屏幕坐标
        protected GeoData.GeoDataRow m_EditingRow;//节点移动的目标记录
        protected GeoData.GeoDataRow m_EditingRowNew;//节点移动的目标记录备份，用作增量信息前对象
        protected GeoPoint m_EditPoint;           //节点移动的目标点
        protected Geometry m_GeomDrag;            //拖拽用的几何备份。
        protected GeoPoint m_PointDrag;           //拖拽的点

        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (! bMouseDowing )
                {
                    FirstMouseDown(sender, e);
                }
                else
                {
                    SecondMouseDown(sender, e);
                }
            }
            else if (e.Button == MouseButtons.Right) //鼠标的右键取消命令
            {
                Cancel();
            }
        }
        public override void Cancel()
        {
            if (bMouseDowing)  //如果已经再拖动 ，就取消
            {
                bMouseDowing = false;
                m_MapUI.RePaint();
            }
            else  //切换到上一个工具
            {
                if (m_MapUI.m_EditToolBack != null)
                {
                    m_MapUI.MapTool = m_MapUI.m_EditToolBack;
                    m_MapUI.MapTool.initial();
                }
            }
        }
        public override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (bMouseDowing && m_DragPoint != e.Location)
            {
                GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);         
                m_MapUI.MouseCatch(pt);               
                Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
                Graphics g = Graphics.FromImage(imgTemp);
                m_MapUI.RePaint(g);
                m_PointDrag.SetXY(pt.X, pt.Y);
                Style.VectorStyle style = new Style.VectorStyle(1, 1, Color.Orchid, Color.Orchid, false);
                GIS.Render.RenderAPI.DrawGeometry(g, m_GeomDrag, style, m_MapUI);
                m_MapUI.Image.Dispose();
                m_MapUI.Image = imgTemp;
                g.Dispose();
                m_MapUI.BaseRefresh();   
            }
        }

        private void FirstMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {            
            GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
            GeoDataRow RowCatched = null;
            int sltCount = m_MapUI.SltGeoSet.Count;
            if (sltCount == 0)
            {
                if (m_MapUI.SelectByPt(e.Location, SelectType.Geomtry) != null)
                {
                    m_MapUI.Refresh();
                }
                return;
            }
            else
            {
                RowCatched = m_MapUI.MouseCatchInSltGeoSet(pt, MouseCatchType.Vertex);
            }
            if (RowCatched == null)
            {
                if (m_MapUI.SelectByPt(e.Location, SelectType.Geomtry) != null)
                {
                    m_MapUI.Refresh();
                }
                return;
            }  

            m_EditingRow = RowCatched;
            if (m_EditingRow != null)
            {
                m_EditingRowNew = m_EditingRow.Clone();//备份一个字段
             
                m_MapUI.OutPutTextInfo("提示：左键拖动节点，右键取消。\r\n");
               
                m_GeomDrag = m_EditingRow.Geometry.Clone(); 
                m_PointDrag = m_GeomDrag.MouseCatchPt(pt, MouseCatchType.Vertex );

                if (m_PointDrag == null)
                    return;
             //   m_EditPoint = m_MapUI.m_SnapPoint;
                m_EditPoint = m_EditingRowNew.Geometry.MouseCatchPt(pt, MouseCatchType.Vertex);
                m_DragPoint = e.Location; 
                bMouseDowing = true;
            }
        }
        public void MultiPolygonVeterxMove(GeoMultiPolygon plgs, GeoPoint pt)
        {
            for (int i = 0; i < plgs.NumGeometries; i++)
            {
                PolygonVertexMove(plgs.Polygons[i], pt);
            }
        }
        private void  PolygonVertexMove(GeoPolygon plg, GeoPoint pt)
        {
            bool bStartPt = false;
            if (plg.ExteriorRing.StartPoint.IsEqual(m_EditPoint) ||
                plg.ExteriorRing.EndPoint.IsEqual(m_EditPoint))
            {
                bStartPt = true;
                plg.ExteriorRing.StartPoint.SetXY(pt.X, pt.Y);
                plg.ExteriorRing.EndPoint.SetXY(pt.X, pt.Y);
            }
            for (int i = 0; i < plg.InteriorRings.Count; i++)
            {
                if (plg.InteriorRings[i].StartPoint.IsEqual(m_EditPoint) ||
                    plg.InteriorRings[i].EndPoint.IsEqual(m_EditPoint))
                {
                    bStartPt = true;
                    plg.InteriorRings[i].StartPoint.SetXY(pt.X, pt.Y);
                    plg.InteriorRings[i].EndPoint.SetXY(pt.X, pt.Y);
                }
            }
            if (!bStartPt)
                m_EditPoint.SetXY(pt.X, pt.Y);
        }
        protected void SecondMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Location != m_DragPoint && bMouseDowing == true)
            {
                GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                    m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
                

                if (m_EditingRow.Geometry is GeoPolygon)//如果是多边形的起点或者终点
                {
                    GeoPolygon geom = m_EditingRowNew.Geometry as GeoPolygon;
                    PolygonVertexMove(geom, pt);
                }
                else if (m_EditingRow.Geometry is GeoMultiPolygon)
                {
                    GeoMultiPolygon plgs = m_EditingRowNew.Geometry as GeoMultiPolygon;
                    MultiPolygonVeterxMove(plgs,pt);
                }
                else
                    m_EditPoint.SetXY(pt.X, pt.Y);
            
              
                                            //屏幕和鹰眼的刷新
                m_MapUI.BoundingBoxChangedBy(m_EditingRow);   //重新计算边界矩形
                EditState state = m_EditingRow.EditState;
                ((GeoDataTable)m_EditingRow.Table).AddRow(m_EditingRowNew); //保存的数据不添加到图层中，由增量管理器管理
                if (m_EditingRowNew != null)
                {
                    if (m_EditingRow.EditState == EditState.Original)//几何变化的增量信息
                    {

                        m_EditingRowNew.EditState = EditState.GeometryAft;
                        m_EditingRow.EditState = EditState.GeometryBef;
                    }
                    else
                    {
                        m_EditingRow.EditState = EditState.Invalid;
                        m_EditingRowNew.EditState = state;
                    }
                }
                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                m_MapUI.m_OprtManager.AddOprt(oprts);

                GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(m_EditingRowNew, EditState.Invalid, m_EditingRowNew.EditState);
                oprts.m_NewOperands.Add(oprtNew);

                GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(m_EditingRow, state, m_EditingRow.EditState);
                oprts.m_OldOperands.Add(oprtOld);
                ///////////////操作回退

                m_MapUI.Refresh();   
            }
            bMouseDowing = false;
        }
  
    }
}
