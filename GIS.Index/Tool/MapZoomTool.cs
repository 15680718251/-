using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Geometries;

namespace GIS.TreeIndex.Tool
{    
    public class MapZoomTool:MapTool
    {
        protected MapZoomTool() 
        { 
        }
        public MapZoomTool(MapUI ui,ZoomType type ):base(ui)
        {
            m_Type = type;
            if (type == ZoomType.ZoomIn)
            {
                m_Cursor = Cursors.SizeAll;
            }         
        }

        
        private ZoomType m_Type;
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
        }
        public override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Location != m_MapUI.MouseDragPt && m_MapUI.MouseDowning)//表示正在拖动
            {
                if (m_MapUI.Image != null)
                {
                    // 绘制在屏幕缓冲区上
                    Image _imgTemp = new Bitmap(m_MapUI.Size.Width, m_MapUI.Size.Height);
                    Graphics g = Graphics.FromImage(_imgTemp);
                    g.Clear(Color.Transparent);
                    m_MapUI.RePaint(g);
                    Pen pen = new Pen(Color.RoyalBlue, 2);
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                    int xMin = Math.Min(e.X, m_MapUI.MouseDragPt.X);
                    int yMin = Math.Min(e.Y, m_MapUI.MouseDragPt.Y);
                    g.DrawRectangle(pen, xMin, yMin, Math.Abs(e.X - m_MapUI.MouseDragPt.X), Math.Abs(e.Y - m_MapUI.MouseDragPt.Y));                    
                    g.Dispose();
                    pen.Dispose();
                    m_MapUI.Image = _imgTemp;
                    m_MapUI.BaseRefresh();
                }
            }
        }
        public override void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
           
            if (e.Button == MouseButtons.Left  && m_MapUI.MouseDowning )//表示正在拖动
            {
                if (m_Type == ZoomType.ZoomIn) //如果是放大
                {
                    if (e.Location != m_MapUI.MouseDragPt)
                    {
                        GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
                        GeoPoint pt2 = m_MapUI.TransFromMapToWorld(m_MapUI.MouseDragPt);
                        GeoBound bound = new GeoBound(pt, pt2);
                        m_MapUI.ZoomToBox(bound);
                    }
                    else
                    {
                        m_MapUI.DBlc *= 0.8;
                        m_MapUI.SetCenterGeoPoint( m_MapUI.TransFromMapToWorld(e.Location));
                    }
                }
                else if (m_Type == ZoomType.ZoomOut) //如果是缩小
                { 
                     m_MapUI.DBlc /= 0.8;
                     m_MapUI.SetCenterGeoPoint( m_MapUI.TransFromMapToWorld(e.Location));                    
                }
                 m_MapUI.Refresh();
             }     
        }
    }
}
