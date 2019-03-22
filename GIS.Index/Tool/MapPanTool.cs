using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GIS.TreeIndex.Tool
{
    public class MapPanTool:MapTool
    {
        protected MapPanTool()
        {
        }
        public MapPanTool(MapUI ui)
            : base(ui)
        {            
            m_Cursor = Cursors.Hand;        
        }
        private Image m_DragImg = null;

        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(m_MapUI.Image !=null)
                m_DragImg = m_MapUI.Image.Clone() as Image;

        }

        public override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Location != m_MapUI.MouseDragPt && m_MapUI.MouseDowning) //表示正在拖动
            {
                if (m_MapUI.Image != null && m_DragImg != null)
                {
                    //绘制在屏幕缓冲区上
                    Image _imgTemp = new Bitmap(m_MapUI.Size.Width, m_MapUI.Size.Height);                   
                    Graphics g = Graphics.FromImage(_imgTemp);
                    g.Clear(Color.Transparent);

                   // if (m_DragImg != null)
                        g.DrawImage(m_DragImg, e.X - m_MapUI.MouseDragPt.X, e.Y - m_MapUI.MouseDragPt.Y);
                    g.Dispose();

                    m_MapUI.Image = _imgTemp;
                    m_MapUI.BaseRefresh();
                }
             }
         }

        public override void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Location != m_MapUI.MouseDragPt && m_MapUI.MouseDowning)
            {
                Point pt = new Point(m_MapUI.Width / 2 + m_MapUI.MouseDragPt.X - e.X, m_MapUI.Height / 2 + m_MapUI.MouseDragPt.Y - e.Y);
                m_MapUI.SetCenterGeoPoint( m_MapUI.TransFromMapToWorld(pt));
                m_MapUI.Refresh();
            }
            if (m_DragImg != null)
            {
                m_DragImg.Dispose();
                m_DragImg = null;
            }
        }
    }
}
