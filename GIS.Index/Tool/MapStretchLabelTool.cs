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
using GIS.TreeIndex.Forms;
namespace GIS.TreeIndex.Tool
{
    public class MapStretchLabelTool:MapTool
    {
        public MapStretchLabelTool(MapUI ui)
            : base(ui)
        {
            initial();
        }
        protected GeoData.GeoDataRow m_EditingRow;//节点移动的目标记录

        protected bool bMouseDowing = false;
        protected Point m_DragPoint;              //拖拽前的屏幕坐标      
        protected GeoPoint m_EditPoint;           //节点移动的目标点
        protected GeoLabel m_LabelDrag;            //拖拽用的几何备份。
        protected GeoPoint m_PointDrag;           //拖拽的点
        public override void initial()
        {
            m_MapUI.OutPutTextInfo("提示：开始注记拉伸功能，请选择注记和拉伸点............\r\n");
            if (m_MapUI.SltLabelSet.Count == 1 )
            {
                m_EditingRow = m_MapUI.SltLabelSet[0];
            }
            else
            {
                m_MapUI.OutPutTextInfo("提示：请选择一个注记............\r\n");
            }
            m_MapUI.ClearAllSlt(); 
            LabelRender();
        }
        private void LabelRender()
        {
            if (m_EditingRow != null && m_EditingRow.Geometry != null)
            {
                GeoLabel label = m_EditingRow.Geometry as GeoLabel;

                Bitmap _ImgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);

                Graphics g = Graphics.FromImage(_ImgTemp);

                m_MapUI.RePaint(g);
                
                Point[] pts =  new Point[2];
                pts[0] = m_MapUI.TransFromWorldToMap(label.StartPt);
                pts[1] = m_MapUI.TransFromWorldToMap(label.EndPt);

                Brush brush = new SolidBrush(Color.Red);

                for (int i = 0; i < pts.Length; i++)
                {
                    g.FillRectangle(brush, new Rectangle(pts[i].X - 4, pts[i].Y - 4, 8, 8));

                }

                m_MapUI.Image = _ImgTemp;
                m_MapUI.BaseRefresh();

                brush.Dispose();
                g.Dispose();  
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
                base.Cancel();
            }
        }
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_EditingRow == null)
                {
                    m_MapUI.SelectByPt(e.Location, SelectType.Label);
                    initial();
                    m_MapUI.ClearAllSltWithoutRefresh();
                }
                else
                {
                    if (!bMouseDowing)
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
            else if (e.Button == MouseButtons.Right) //鼠标的右键取消命令
            {
                Cancel();
            }
        }

        private void SecondMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Location != m_DragPoint && bMouseDowing == true)
            {
                GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                    m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;

                m_EditPoint.SetXY(pt.X, pt.Y);
                GeoLabel label = m_EditingRow.Geometry as GeoLabel;
              
                m_MapUI.Refresh();                                  //屏幕和鹰眼的刷新
                m_MapUI.BoundingBoxChangedBy(m_EditingRow);   //重新计算边界矩形

            }
            bMouseDowing = false;
        }
      
        private void FirstMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
            LabelRender();
            GeoPoint SnapPt = (m_EditingRow.Geometry as GeoLabel).MouseCatchPt(pt, MouseCatchType.Vertex);
            if (SnapPt == null)
                return;

            m_MapUI.OutPutTextInfo("提示：左键拖动节点，右键取消。\r\n");
            bMouseDowing = true;
            m_LabelDrag = m_EditingRow.Geometry.Clone() as GeoLabel;
            m_PointDrag = m_LabelDrag.MouseCatchPt(pt, MouseCatchType.Vertex);
            m_EditPoint = SnapPt;   //真实的捕捉点 
            m_DragPoint = e.Location;

        }
        public override void Finish()
        {
            bMouseDowing = false;
            m_EditingRow = null;
            m_EditPoint = null;           //节点移动的目标点
            m_LabelDrag = null;            //拖拽用的几何备份。
            m_PointDrag = null;           //拖拽的点
            m_MapUI.OutPutTextInfo("提示:退出注记拉伸修改命令.......\r\n");

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
                
                Style.VectorStyle style = new Style.VectorStyle(2, 2, Color.Orchid, Color.Orchid, false);
                GIS.Render.RenderAPI.DrawLabel(g, m_LabelDrag,Color.Blue, m_MapUI);
                m_MapUI.Image.Dispose();
                m_MapUI.Image = imgTemp;
                g.Dispose();
                m_MapUI.BaseRefresh();
            }
        }

    }
}
