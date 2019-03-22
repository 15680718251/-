using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;

namespace GIS.UI.AdditionalTool
{
    class TopHelper
    {
        const double LIMIT = 1e-5;
        public static bool IsSelfIntersect(GeoPolygon polygon)
        {
            GeoLinearRing geoLinearRing = polygon.ExteriorRing;
            return IsSelfIntersect(geoLinearRing);
        }
        public static bool IsSelfIntersect(GeoLinearRing geoLinearRing)
        {
            GeoLineString geo = (GeoLineString)geoLinearRing;
            for (int j = 0; j < geo.Vertices.Count - 3; j++)
            {
                GeoPoint pt_a1 = geo.Vertices[j];
                GeoPoint pt_a2 = geo.Vertices[j + 1];
                for (int k = j + 2; k < geo.Vertices.Count - 1; k++)
                {
                    GeoPoint pt_b1 = geo.Vertices[k];
                    GeoPoint pt_b2 = geo.Vertices[k + 1];
                    if (IntersectTwoLine(pt_a1, pt_a2, pt_b1, pt_b2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 判断两条线是否相交
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <returns></returns>
        public static bool IntersectTwoLine(GeoPoint p1, GeoPoint p2, GeoPoint p3, GeoPoint p4)
        {
            int ccw312 = CCW(p3, p1, p2);
            int ccw412 = CCW(p4, p1, p2);
            int ccw134 = CCW(p1, p3, p4);
            int ccw234 = CCW(p2, p3, p4);
            if (ccw134 * ccw234 < 0 && ccw312 * ccw412 < 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 判断点C与线段AB的关系
        /// </summary>
        /// <param name="p0">单独点C</param>
        /// <param name="p1">线段端点A点</param>
        /// <param name="p2">线段端点B点</param>
        /// <returns>C在AB线上（包括端点）返回0，其余根据计算结果返回正负值</returns>
        public static int CCW(GeoPoint p0, GeoPoint p1, GeoPoint p2)
        {
            double dx1, dx2;
            double dy1, dy2;

            dx1 = p1.X - p0.X;
            dx2 = p2.X - p0.X;
            dy1 = p1.Y - p0.Y;
            dy2 = p2.Y - p0.Y;
            //向量p0p1(dx1,dy1), p0p2(dx2,dy2),向量叉乘表达dx1*dy2-dx2*dy1
            if (Math.Abs(dx1 * dy2 - dy1 * dx2) < Math.Pow(LIMIT, 2))
            {
                //p0在p1和p2的线上
                return 0;
            }
            else
            {
                return ((dx1 * dy2 > dy1 * dx2) ? 1 : -1);
            }
        }
    }
}
