using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using GIS.Geometries;
using GIS.Layer;
using GIS.SpatialRelation;

namespace GIS.TreeIndex.Tool
{
   public class MapLineExtentTool:MapMoveNodeTool
    {
       public MapLineExtentTool(MapUI ui)
           : base(ui)
       {
           initial();
       }
       private enum LineExtentState
       {
           State_SelectLine,//选线
           State_SelectNode, //选节点
           State_ToBeFinish //选终点
       }
       private LineExtentState m_State = LineExtentState.State_SelectLine;
       private GeoLineString m_SnapLine = null;
     
       private int ptIndex = -1;//插入点 位置
       


       private void SelectLine(System.Windows.Forms.MouseEventArgs e)
       {
           m_MapUI.ClearAllSltWithoutRefresh();
           GeoData.GeoDataRow row = m_MapUI.SelectByPt(e.Location, SelectType.Geomtry);
           if (row != null && !(row.Geometry is GeoPoint))
           {
               m_EditingRow = row;
               m_State = LineExtentState.State_SelectNode;
               m_MapUI.OutPutTextInfo("提示：请选择需要拉伸的线段\r\n");
               m_MapUI.Refresh();
           }
           else 
               m_MapUI.ClearAllSlt();  
       }
       private void SelectNode(MouseEventArgs e)
       {
           if (m_SnapLine != null && m_MapUI.m_SnapPoint != null
               && ptIndex != -1 )
           {
               GeoPoint pt1 = m_SnapLine.Vertices[ptIndex - 1];
               GeoPoint pt2 = m_SnapLine.Vertices[ptIndex];
               double dist1 = m_MapUI.m_SnapPoint.DistanceTo(pt1);
               double dist2 = m_MapUI.m_SnapPoint.DistanceTo(pt2);
               m_EditPoint = (dist1 < dist2) ? pt1 : pt2;
               m_State = LineExtentState.State_ToBeFinish;

               bMouseDowing = true;
               m_EditingRowNew= m_EditingRow.Clone();//备份一个字段

               m_MapUI.OutPutTextInfo("提示：左键拖动节点，右键取消。\r\n");
             
               m_GeomDrag = m_EditingRow.Geometry.Clone();

               m_PointDrag = m_GeomDrag.MouseCatchPt(m_EditPoint, MouseCatchType.Vertex);
              
           }
       }
       private void FinishNode(MouseEventArgs e)
       {
           base.SecondMouseDown(null, e);
           Cancel();
       }
       public override void initial()
       {
           m_MapUI.ClearAllSlt();
           m_MapUI.OutPutTextInfo("提示：任意线延长功能开始,请先选择线体！\r\n");
           m_State = LineExtentState.State_SelectLine;
           m_SnapLine = null;
           m_EditPoint = null;
           ptIndex = -1;
           m_EditingRow = null;
       }
       public override void OnMouseMove(object sender, MouseEventArgs e)
       {
           if (m_EditingRow == null)
               return;
           if ( m_State == LineExtentState.State_SelectNode)
           {
               DrawSelectNode(e);
           }
           else if (m_State == LineExtentState.State_ToBeFinish)
           {
               DrawFinishNode(e);
           }
       }
       private void DrawFinishNode(MouseEventArgs e)
       {
           GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);

           GeoPoint ptVertical;

           bool valid = GeoAlgorithm.VerticalPtofPtToLine(pt,m_SnapLine.Vertices[ptIndex-1],m_SnapLine.Vertices[ptIndex],out ptVertical);

           if ( valid)
           {                
               m_MapUI.m_SnapPoint = ptVertical;
               Bitmap bmp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
               Graphics g = Graphics.FromImage(bmp);
               m_MapUI.RePaint(g);
               Point ppt = m_MapUI.TransFromWorldToMap(ptVertical);
               Pen pen = new Pen(Color.Red, 0.5f);
               pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
               g.DrawEllipse(pen, new Rectangle(ppt.X - 3, ppt.Y - 3, 6, 6));
               g.DrawRectangle(pen, new Rectangle(ppt.X - m_MapUI.SnapPixels, ppt.Y - m_MapUI.SnapPixels, 2 * m_MapUI.SnapPixels, 2 * m_MapUI.SnapPixels));

               m_PointDrag.SetXY(m_MapUI.m_SnapPoint.X, m_MapUI.m_SnapPoint.Y);
               Style.VectorStyle style = new Style.VectorStyle(1, 1, Color.Orchid, Color.Orchid, false);
               GIS.Render.RenderAPI.DrawGeometry(g, m_GeomDrag, style, m_MapUI);
               m_MapUI.Image = bmp;

               m_MapUI.BaseRefresh();
               g.Dispose();
           }
       }
       private void DrawSelectNode(MouseEventArgs e)
       {
           GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);

           GeoPoint ptVertical;
           GeoLineString line = GeoAlgorithm.VerticalPtOfPtToGeometry(pt, m_EditingRow.Geometry, out ptVertical, out ptIndex);

           if (line != null &&
               GeoAlgorithm.IsOnPointCoarse(ptVertical, pt))
           {
               m_SnapLine = line;
               m_MapUI.m_SnapPoint = ptVertical;
               Bitmap bmp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
               Graphics g = Graphics.FromImage(bmp);
               m_MapUI.RePaint(g);
               Point ppt = m_MapUI.TransFromWorldToMap(ptVertical);
               Pen pen = new Pen(Color.Red, 0.5f);
               pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
               g.DrawEllipse(pen, new Rectangle(ppt.X - 3, ppt.Y - 3, 6, 6));
               g.DrawRectangle(pen, new Rectangle(ppt.X - m_MapUI.SnapPixels, ppt.Y - m_MapUI.SnapPixels, 2 * m_MapUI.SnapPixels, 2 * m_MapUI.SnapPixels));
               m_MapUI.Image = bmp;

               m_MapUI.BaseRefresh();
               g.Dispose();
           }
           else
           {
               m_SnapLine = null;
               m_MapUI.m_SnapPoint = null;
               return;
           }
       }
  
       public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
       {
           if (e.Button == MouseButtons.Left)
           {
               if (m_State == LineExtentState.State_SelectLine)
               {
                   SelectLine(e);
               }
               else if (m_State == LineExtentState.State_SelectNode)
               {
                   SelectNode(e);
               }
               else if (m_State == LineExtentState.State_ToBeFinish)
               {
                   FinishNode(e);
               }
              
           }
           else if (e.Button == MouseButtons.Right)
           {
               Cancel();
           }
       }

       public override void Cancel()
       {
           bMouseDowing = false;
           m_State = LineExtentState.State_SelectLine;
           m_SnapLine = null;
           m_EditPoint = null;
           ptIndex = -1;
           m_EditingRow = null;
           m_MapUI.m_EditToolBack = m_MapUI.MapTool;
           m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
           m_MapUI.OutPutTextInfo("提示：任意线延长功能结束！\r\n"); 
       }
     

      
    }
}
