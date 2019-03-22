using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Map;
using GIS.Geometries;
using GIS.Utilities;
namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    { 
        /// <summary>
        /// 将地理坐标转换为屏幕坐标
        /// </summary>
        /// <param name="pt">地理坐标点PT</param>
        /// <returns>返回屏幕坐标</returns>
        public Point TransFromWorldToMap(GeoPoint pt)
        {
            return Transform.MapToScreen(pt, m_ViewExtents, DBlc);
        }
        /// <summary>
        /// 给其它函数在需要的时候获取比例尺，2013.4.14日，阳成飞
        /// </summary>
        /// <returns></returns>
        public double GetBlc() {
            return DBlc;
        }
        public PointF TransFromWorldToMapF(GeoPoint pt)
        {
            return Transform.MapToScreenF(pt, m_ViewExtents, DBlc);
        }
        public PointF TransFromWorldToMapF(PointF pt)
        {
            return Transform.MapToScreenF(pt, m_ViewExtents, DBlc);
        }
        /// <summary>
        /// 将屏幕坐标转换成地理坐标
        /// </summary>
        /// <param name="pt">屏幕坐标PT</param>
        /// <returns>地理坐标</returns>
        public GeoPoint TransFromMapToWorld(Point pt)
        {
            return Transform.ScreenToMap(pt, m_ViewExtents, DBlc);
        }
        public GeoPoint TransFromMapToWorldF(PointF pt)
        {
            return Transform.ScreenToMapF(pt, m_ViewExtents, DBlc);
        }
        /// <summary>
        /// 将屏幕大小转换为地理坐标形式的矩形
        /// </summary>
        /// <param name="size">屏幕大小</param>
        /// <returns></returns>
        public GeoBound TransFromMapToWorld(Rectangle rect)
        {
            Point lbPt = new Point(rect.Left, rect.Bottom);
            Point rtPt = new Point(rect.Right, rect.Top);

            GeoBound bound = new GeoBound(TransFromMapToWorld(lbPt), TransFromMapToWorld(rtPt));
            //bound.LeftBottomPt = TransFromMapToWorld(lbPt);
            //bound.RightUpPt = TransFromMapToWorld(rtPt);

            return bound;
        }
        /// <summary>
        /// 将一条线的地理坐标转为屏幕坐标数组
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public Point[] TransLineToMap(GeoLineString line)
        {
            if (line == null)
                return null;
            Point[] pts = new Point[line.NumPoints];
            for (int i = 0; i < line.NumPoints; i++)
            {
                pts[i] = TransFromWorldToMap(line.Vertices[i]);
            }
            return pts;
        }

        public Point[] TransLineToMap(List<GeoPoint> ptlist)
        {
            if (ptlist == null)
                return null;
            Point[] pts = new Point[ptlist.Count];
            for (int i = 0; i < ptlist.Count; i++)
            {
                pts[i] = TransFromWorldToMap(ptlist[i]);
            }
            return pts;
        }
        public PointF[] TransLineToMapF(GeoLineString line)
        {
            if (line == null)
                return null;
            PointF[] pts = new PointF[line.NumPoints];
            for (int i = 0; i < line.NumPoints; i++)
            {
                pts[i] = TransFromWorldToMapF(line.Vertices[i]);
            }
            return pts;
        }

       

        /// <summary>
        /// 将矩形范围转成屏幕坐标范围
        /// </summary>
        /// <param name="bound"></param>
        /// <returns></returns>
        public Rectangle TransFromWorldToMap(GeoBound bound)
        { 
            Point pt1 = TransFromWorldToMap(bound.LeftBottomPt);
            Point pt2 = TransFromWorldToMap(bound.RightUpPt);
            return new Rectangle(pt1.X, pt2.Y, pt2.X - pt1.X, pt1.Y - pt2.Y);
        }
        public double TransFromMapToWorld(float len)
        {
            return len * DBlc;
        }
        public double TransFromMapToWorld(int len)
        {
            return len * DBlc;
        }

        public float TransFromWorldToMap(double len)
        {
            return  (float)(len / DBlc);
        }
    }
}
