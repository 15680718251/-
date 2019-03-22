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
    public class MapArcModifyTool:MapTool
    {
        public MapArcModifyTool(MapUI ui)
            : base(ui)
        {
            initial();
        }

        protected GeoData.GeoDataRow m_EditingRow;//节点移动的目标记录

        protected bool bMouseDowing = false;
        protected Point m_DragPoint;              //拖拽前的屏幕坐标      
        protected GeoPoint m_EditPoint;           //节点移动的目标点
        protected GeoArc m_GeomDrag;            //拖拽用的几何备份。
        protected GeoPoint m_PointDrag;           //拖拽的点


        private void ArcRender()
        {
            if (m_EditingRow != null)
            {
                Bitmap _ImgTemp = new Bitmap(m_MapUI.Width,m_MapUI.Height);

                Graphics g = Graphics.FromImage(_ImgTemp);

                m_MapUI.RePaint(g);

                GeoArc arc = m_EditingRow.Geometry as GeoArc;

                Point[] pts = m_MapUI.TransLineToMap(arc.SkeletonPtList);  
                
                Brush brush = new SolidBrush(Color.Blue);
                Pen pen = new Pen(Color.Black,2);
                for (int i = 0; i < pts.Length; i++)
                {
                    g.FillRectangle(brush, new Rectangle(pts[i].X - 4, pts[i].Y -4, 8, 8));
                    g.DrawRectangle(pen, new Rectangle(pts[i].X - 8, pts[i].Y - 8, 16, 16));

                }

                m_MapUI.Image = _ImgTemp;
                m_MapUI.BaseRefresh();

                brush.Dispose();
                pen.Dispose();
                g.Dispose();                
            }
        }

        public override void initial()
        {
            m_MapUI.OutPutTextInfo("提示：开始三点弧修改功能............\r\n");
            if (m_MapUI.SltGeoSet.Count == 1
              && m_MapUI.SltGeoSet[0].Geometry is Geometries.GeoArc)
            {
                m_EditingRow = m_MapUI.SltGeoSet[0];               
            }
            else if(m_MapUI.SltGeoSet.Count==1)
            {
                m_MapUI.OutPutTextInfo("提示：选中的不是弧线，请选择弧线............\r\n");
            } 
 
            ArcRender();
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
            //    m_MapUI.ClearAllSlt();
                base.Cancel(); 
            }
        }
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_EditingRow == null)
                {
                    m_MapUI.SelectByPt(e.Location, SelectType.Line);
                    initial();
      
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
                        Cancel();
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
                GeoArc arc = m_EditingRow.Geometry as GeoArc;
                arc.Interpolation();
                m_MapUI.Refresh();                                  //屏幕和鹰眼的刷新
                m_MapUI.BoundingBoxChangedBy(m_EditingRow);   //重新计算边界矩形
             
            }
            bMouseDowing = false;
        }
        public GeoPoint MouseCatchInSpline(GeoArc arc,GeoPoint pt )
        {
            for (int i = 0; i < arc.SkeletonPtList.Count; i++)
            {
                if (SpatialRelation.GeoAlgorithm.IsOnPointCoarse(pt, arc.SkeletonPtList[i]))
                {
                    return arc.SkeletonPtList[i];
                }
            }
            return null;
        }
        private void FirstMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);

            GeoPoint SnapPt = MouseCatchInSpline(m_EditingRow.Geometry as GeoArc, pt);
            if (SnapPt == null)
                return;

            m_MapUI.OutPutTextInfo("提示：左键拖动节点，右键取消。\r\n");
            bMouseDowing = true;
            m_GeomDrag = m_EditingRow.Geometry.Clone() as GeoArc;
            m_PointDrag = MouseCatchInSpline(m_GeomDrag, pt); //备份捕捉点
            m_EditPoint = SnapPt;   //真实的捕捉点 
            m_DragPoint = e.Location;

        }
        public override void Finish()
        {
            bMouseDowing = false;
            m_EditingRow = null;
            m_EditPoint = null;           //节点移动的目标点
            m_GeomDrag = null;            //拖拽用的几何备份。
            m_PointDrag = null;           //拖拽的点
            m_MapUI.OutPutTextInfo("提示:退出曲线修改命令.......\r\n");

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
                m_GeomDrag.Interpolation();
                Style.VectorStyle style = new Style.VectorStyle(2, 2, Color.Orchid, Color.Orchid, false);
                GIS.Render.RenderAPI.DrawGeometry(g, m_GeomDrag, style, m_MapUI);
                m_MapUI.Image.Dispose();
                m_MapUI.Image = imgTemp;
                g.Dispose();
                m_MapUI.BaseRefresh();
            }
        }
    }
}
