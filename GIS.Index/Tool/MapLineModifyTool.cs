using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Layer;
using System.Windows.Forms;
using GIS.Geometries;
using GIS.Map;
using GIS.SpatialRelation;
using System.Drawing;
namespace GIS.TreeIndex.Tool
{
    public class MapLineModifyTool:MapMoveNodeTool
    {
        private enum LineModifyState
        {
            State_SelectLine,//选择线
            State_InsertNode,//插入节点
            State_MoveNode //移动节点
        }
        public MapLineModifyTool(MapUI ui)
            : base(ui)
        {
            initial();
        }
        private int ptIndex = -1;//插入点 位置
        private bool bHasInsertPt = false;
        private LineModifyState m_State = LineModifyState.State_SelectLine;
        private GeoLineString m_SnapLine = null;
        private void SelectLine(System.Windows.Forms.MouseEventArgs e)
        { 
            m_MapUI.ClearAllSltWithoutRefresh();
            GeoData.GeoDataRow row = m_MapUI.SelectByPt(e.Location, SelectType.Geomtry);
            if (row != null && !(row.Geometry is GeoPoint))
            {
                m_EditingRowNew = row.Clone();//备份一个字段
                m_EditingRow = row;
                m_State = LineModifyState.State_InsertNode;
                m_MapUI.OutPutTextInfo("提示：请选择需要修改的线段\r\n");
                m_MapUI.Refresh();
            }
            else
                m_MapUI.ClearAllSlt(); 

        }
        private void InsertPoint(System.Windows.Forms.MouseEventArgs e)
        {
            if (!bHasInsertPt && ptIndex != -1
                     && m_MapUI.m_SnapPoint != null
                     && m_SnapLine != null)
            {
                m_SnapLine.Vertices.Insert(ptIndex, m_MapUI.m_SnapPoint);
                bHasInsertPt = true;


               
                m_MapUI.OutPutTextInfo("提示：左键拖动节点，右键取消。\r\n");
                bMouseDowing = true;
                m_GeomDrag = m_EditingRowNew.Geometry.Clone();

                GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
                m_State = LineModifyState.State_MoveNode;
                m_PointDrag = m_GeomDrag.MouseCatchPt(pt, MouseCatchType.Vertex);
                m_EditPoint = m_EditingRowNew.Geometry.MouseCatchPt(pt, MouseCatchType.Vertex);
                m_DragPoint = e.Location;
            }
        }
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_State == LineModifyState.State_SelectLine)
                {
                    SelectLine(e);
                }
                else if(m_State == LineModifyState.State_InsertNode) 
                {
                    InsertPoint(e);                    
                }
                else if( m_State == LineModifyState.State_MoveNode)
                {
                    if (bMouseDowing)
                    {                        
                        SecondMouseDown(sender, e);
                        bHasInsertPt = false;
                        ptIndex = -1;
                        m_MapUI.ClearAllSlt();
                        m_SnapLine = null;
                        m_EditingRow = null;
                        bMouseDowing = false;
                        m_MapUI.OutPutTextInfo("线修改工具结束\n");
                        m_MapUI.m_EditToolBack = m_MapUI.MapTool;
                        m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);     
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
            if (bMouseDowing)  //如果已经再拖动 ，就取消
            {
                bMouseDowing = false;
                bHasInsertPt = false;

                m_SnapLine.Vertices.RemoveAt(ptIndex);
                m_EditingRow = null;
                m_SnapLine = null;
                ptIndex = -1;
                m_MapUI.ClearAllSlt();
                
            }
            else  //切换到上一个工具
            {
                m_MapUI.m_EditToolBack = m_MapUI.MapTool;
                m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
            }
        }
        public override void Finish()
        {
           
        }
        public override void initial()
        {
            m_MapUI.ClearAllSlt();
            m_MapUI.OutPutTextInfo("线修改工具激活：请选择线体，添加插入点后, 鼠标拖动开始修改\r\n");
            m_State = LineModifyState.State_SelectLine;
            m_Cursor = Cursors.Cross; 

        }
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (m_EditingRowNew == null)
                return;
            if (m_State == LineModifyState.State_InsertNode)
            {
                GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
            
                GeoPoint ptVertical;
                GeoLineString line = GeoAlgorithm.VerticalPtOfPtToGeometry(pt, m_EditingRowNew.Geometry, out ptVertical, out ptIndex);            
              
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
            else
                base.OnMouseMove(sender, e);

        }
    }
}
