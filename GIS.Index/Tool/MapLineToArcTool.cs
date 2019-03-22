using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using GIS.Geometries;
using GIS.GeoData;
using GIS.SpatialRelation;
using GIS.TreeIndex.OprtRollBack;

namespace GIS.TreeIndex.Tool
{
    public class MapLineToArcTool:MapTool
    {
        public MapLineToArcTool(MapUI ui)
            : base(ui)
        {
            initial();
        }
        protected GeoData.GeoDataRow m_EditingRow;//节点移动的目标记录       
        protected GeoDataRow NewRow;
        protected GeoLineString m_SnapLine;
        protected int ptIndex = -1;
        protected Point m_DragPoint;              //拖拽前的屏幕坐标


        public override void initial()
        {
            m_MapUI.OutPutTextInfo("提示：开始线段圆滑功能............\r\n");
            if (m_MapUI.SltGeoSet.Count == 1
              && !(m_MapUI.SltGeoSet[0].Geometry is Geometries.GeoPoint))
            {
                m_EditingRow = m_MapUI.SltGeoSet[0];
            }
            else 
            {
                m_MapUI.OutPutTextInfo("提示：选中的目标不对，请选择一个包含线段的目标...........\r\n");
                m_MapUI.ClearAllSlt();
            }         
        }

        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_EditingRow == null)//如果没选中
                {
                    GeoDataRow row = m_MapUI.SelectByPt(e.Location, SelectType.Geomtry);
                    if (row != null && !(row.Geometry is GeoPoint))
                    {
                        m_EditingRow = row;
                        m_MapUI.OutPutTextInfo("提示：请选中需要变圆弧的边，左键点击开始拖动。\r\n");
                        m_MapUI.Refresh();
                    }
                }
                else                       //如果有选中
                {
                    if (m_SnapLine == null)     //如果没选中插入点的线，则选中插入点的线
                    {
                        FirstMouseDown(sender, e);
                    }
                    else
                    {
                        SecondMouseDown(sender, e);
                        base.Cancel();
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                Cancel();
            }
        }

        public override void Cancel()
        {
            if (m_SnapLine != null)  //如果已经再拖动 ，就取消
            {
                m_SnapLine = null;
                ptIndex = -1;
                m_MapUI.RePaint();
            }
            else  //切换到上一个工具
            {
                base.Cancel();
            }
        }
        public override void Finish()
        {
            m_EditingRow = null;
            m_SnapLine = null;
            ptIndex = -1;
            m_MapUI.ClearAllSlt();
            m_MapUI.OutPutTextInfo("提示：线段圆滑工具结束...........\r\n");
        }
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
            if (m_SnapLine != null )
            {
                m_MapUI.MouseCatch(pt);
                Bitmap _imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
                Graphics g = Graphics.FromImage(_imgTemp);
                m_MapUI.RePaint(g);

               List<GeoPoint> ptList= InterpolationArc(pt);
               if (ptList == null)
                   return;
               Point[] pts = m_MapUI.TransLineToMap(ptList);

               Pen pen = new Pen(Color.Red, 1);
               pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
               g.DrawLines(pen, pts);

               m_MapUI.Image = _imgTemp;
               pen.Dispose();
               g.Dispose();
            }
        }
        private List<GeoPoint> InterpolationArc(GeoPoint ptInsert)
        {
            GeoPoint pt1 = m_SnapLine.Vertices[ptIndex - 1];
            GeoPoint pt2 = m_SnapLine.Vertices[ptIndex];
            return GeoAlgorithm.ThreePointsArc(pt1, ptInsert, pt2, ArcType.Arc);

        }
        private void SecondMouseDown(object sender, MouseEventArgs e)
        {
            if (m_SnapLine != null && e.Location != m_DragPoint)
            {
                GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                  m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;

                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                m_MapUI.m_OprtManager.AddOprt(oprts);                ///////////////操作回退

             
                ((GeoDataTable)m_EditingRow.Table).AddRow(NewRow);

                EditState state = m_EditingRow.EditState;
                if (m_EditingRow.EditState == EditState.Original)
                {
                    NewRow.EditState = EditState.GeometryAft;
                    m_EditingRow.EditState = EditState.GeometryBef;
                }
                else
                {
                    NewRow.EditState = state;
                    m_EditingRow.EditState = EditState.Invalid;
                }
                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(NewRow, EditState.Invalid, NewRow.EditState);
                oprts.m_NewOperands.Add(oprtNew);

                GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(m_EditingRow, state, m_EditingRow.EditState);
                oprts.m_OldOperands.Add(oprtOld);
                ///////////////操作回退

                List<GeoPoint> ptList = InterpolationArc(pt);
                if (ptList == null)
                    return;
                ptList.RemoveAt(ptList.Count - 1);
                ptList.RemoveAt(0);
                m_SnapLine.Vertices.InsertRange(ptIndex, ptList);
                m_MapUI.Refresh();                                  //屏幕和鹰眼的刷新
                m_MapUI.BoundingBoxChangedBy(m_EditingRow);   //重新计算边界矩形
            }
        }
    
        private void FirstMouseDown(object sender, MouseEventArgs e)
        {
            GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
            GeoPoint ptVertical;
            NewRow = m_EditingRow.Clone();
            GeoLineString line = GeoAlgorithm.VerticalPtOfPtToGeometry(pt, NewRow.Geometry, out ptVertical, out ptIndex);
          
            if ( line != null && GeoAlgorithm.IsOnPointCoarse(ptVertical, pt))
            {
                m_SnapLine = line; 
                m_DragPoint = e.Location; 
                m_MapUI.OutPutTextInfo("提示：鼠标移动画弧线，左键确认，右键取消。\r\n"); 
            }         
        }
    }
}
