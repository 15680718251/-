using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;

namespace GIS.SpatialRelation
{
    public class GeoAlgorithm
    {

        private static double m_Tolerance = 4; //容差 四米
        private static double m_PowerOfToler;
        public static double Tolerance
        {
            get { return GeoAlgorithm.m_Tolerance; }
            set
            {
                GeoAlgorithm.m_Tolerance = value;
                m_PowerOfToler = value * value;
            }
        }

        //两点间的距离的平方
        public static double DistanceOfTwoPt(GeoPoint pt1, GeoPoint pt2)
        {
            return Math.Pow((pt1.X - pt2.X), 2) + Math.Pow((pt1.Y - pt2.Y), 2);
        }

        //点到直线的距离的平方
        public static double DistanceOfPtToLine(GeoPoint pt, GeoPoint pt1, GeoPoint pt2)
        {
            double a = pt1.Y - pt2.Y;
            double b = pt2.X - pt1.X;
            double c = pt2.Y * pt1.X - pt1.Y * pt2.X;
            if (a == 0 && b == 0)
            {
                return DistanceOfTwoPt(pt, pt1);
            }
            double tmp = (a * pt.X + b * pt.Y + c);
            double dist = tmp * tmp / (a * a + b * b); //点直线距离公式的平方
            return dist;
        }

        //求点(x0,y0)到线段(x1,y1)-(x2,y2)的最短距离的平方
        public static double Dist_Point_Line(double x0, double y0, double x1, double y1, double x2, double y2)
        {
            double x, y;//点到直线的垂点
            bool bVert = Pt2Line_VerticalPt(x0, y0, x1, y1, x2, y2, out x, out y);
            double s1 = (x0 - x) * (x0 - x) + (y0 - y1) * (y0 - y1);
            double s2 = (x0 - x2) * (x0 - x2) + (y0 - y2) * (y0 - y2);
            if (!bVert)
                return s1;
            double s3 = (x0 - x) * (x0 - x) + (y0 - y) * (y0 - y);

            s1 = s1 < s2 ? s1 : s2;
            return s1 < s3 ? s1 : s3;
        }
        //求两条直线的交点
        public static bool CrossPtOfTwoLine(double x1, double y1, double x2, double y2,
                             double x3, double y3, double x4, double y4,
                             out double xc, out double yc)
        {
            double a1, b1, c1, a2, b2, c2, d;
            xc = 1e20;
            yc = 1e20;
            a1 = y1 - y2;
            b1 = x2 - x1;
            c1 = y2 * x1 - y1 * x2;
            a2 = y3 - y4;
            b2 = x4 - x3;
            c2 = y4 * x3 - y3 * x4;
            d = a1 * b2 - a2 * b1;
            //d==0表示两直线平行
            if (Math.Abs(d) > 1e-5)
            {
                xc = (-c1 * b2 + c2 * b1) / d;
                yc = (-a1 * c2 + a2 * c1) / d;
                return true;
            }
            else
                return false;
        }

        //2009-09-25
        //求直线a1x+b1y+c1=0和直线a2x+b2y+c2=0的交点cx,cy
        //返回0表示两直线平行
        //返回2表示处于同一直线上
        //返回1表示有交点
        public static int g_two_line_cross(double a1, double b1, double c1, double a2, double b2, double c2, out double xc, out double yc)
        {
            xc = 0;
            yc = 0;
            double d = a1 * b2 - a2 * b1;

            //平行
            if (Math.Abs(d) < 1e-6)
            {
                if (Math.Abs(b1 * c2 - b2 * c1) < 1e-6)
                    return 2;
                else
                    return 0;
            }

            xc = (-c1 * b2 + c2 * b1) / d;
            yc = (-a1 * c2 + a2 * c1) / d;

            return 1;

        }
        public static List<GeoPoint> CalcuParallel(List<GeoPoint> pts, double Dist)
        {
            //两线的交点
            double xc, yc;
            double x1, y1, x2, y2, x3, y3;
            double a1, b1, c1, a2, b2, c2;
            double Parallec_C2, Parallel_C1;
            List<GeoPoint> ResultPts = new List<GeoPoint>();
            for (int i = 0; i < pts.Count - 2; ++i)
            {
                x1 = pts[i].X;
                y1 = pts[i].Y;
                x2 = pts[i + 1].X;
                y2 = pts[i + 1].Y;

                a1 = y1 - y2;
                b1 = x2 - x1;
                c1 = y2 * x1 - y1 * x2;

                Parallel_C1 = c1 + Dist * Math.Sqrt(a1 * a1 + b1 * b1);

                x3 = pts[i + 2].X;
                y3 = pts[i + 2].Y;

                a2 = y2 - y3;
                b2 = x3 - x2;
                c2 = y3 * x2 - y2 * x3;

                Parallec_C2 = c2 + Dist * Math.Sqrt(a2 * a2 + b2 * b2);

                int nResult = g_two_line_cross(a1, b1, Parallel_C1, a2, b2, Parallec_C2, out xc, out yc);
                if (nResult == 1)	//有交点
                {

                }
                else if (nResult == 2)	//处于同一直线上
                {
                    GeoPoint PT = CalcuLineLCommand(new GeoPoint(x1, y1), new GeoPoint(x2, y2), Dist);
                    if (PT != null)
                    {
                        xc = PT.X;
                        yc = PT.Y;
                    }
                }
                else					//平行
                {
                }

                ResultPts.Add(new GeoPoint(xc, yc));
            }
            return ResultPts;
        }
        public static GeoPoint CalcuLineTCommand(GeoPoint pt1, GeoPoint pt2, GeoPoint pt)
        {
            double a, b, c, d, ab2;

            a = pt2.X - pt1.X;
            b = pt2.Y - pt1.Y;
            c = pt1.Y * pt2.Y + pt1.X * pt2.X - pt2.X * pt2.X - pt2.Y * pt2.Y; //ax+by+c=0(过(x2,y2)且与(x1,y1)-(x2,y2)垂直的直线公式)
            ab2 =  a*a + b*b;
            if (ab2 < 1e-4)
                return null;
            else
            {
                d = (a * pt.X + b * pt.Y + c) / ab2;	//点到直线的距离公式

                if (Math.Abs(d) < 0.0001)
                    return null;

                double xx = pt.X- a * d;
                double yy = pt.Y - b * d; //求点x1,y1到直线ax+by+c=0的垂点
                return new GeoPoint(xx, yy);

            } 
        }
        public static GeoPoint CalcuLineLCommand(GeoPoint pt1, GeoPoint pt2, double len)
        {
            double deltX = pt2.X - pt1.X;
            double deltY = pt2.Y - pt1.Y;
            double s = Math.Sqrt( deltX * deltX + deltY * deltY) ;//求出当前线的长度

            if (Math.Abs(s) < 0.00000000001)
            {
                return null;
            }

            deltX *= len / s;
            deltY *= len / s;

            //旋转90度
            double x = pt2.X + deltY;
            double y = pt2.Y - deltX;
            return new GeoPoint(x, y);

        }
        //根据两点(x1,y1,x2,y2)求出当前状态 拐长为L后的点坐标
        public static bool g_CalcuLine_L(double x1, double y1, double x2, double y2, double len, out double x, out double y)
        {

            double deltX = x2 - x1;
            double deltY = y2 - y1;
            double s = Math.Sqrt(deltX * deltX + deltY * deltY);//求出当前线的长度

            if (Math.Abs(s) < 0.00000000001)
            {
                x = -1;
                y = -1;
                return false;
            }

            deltX *= len / s;
            deltY *= len / s;

            //旋转90度
            x = x2 + deltY;
            y = y2 - deltX;

            return true;

        }
        //求取x,y绕(baseX,baseY)旋转angle后到xx,yy,的中心点,angle 顺时针方向为负
        public static void CalcCenterPoint(double x, double y, double angle, double xx, double yy, out double baseX, out double baseY)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);
            baseX = ((y * cos + x * sin - yy) * sin + (xx + y * sin - x * cos) * (1 - cos)) / (sin * sin + (1 - cos) * (1 - cos));
            baseY = (xx + y * sin - x * cos - (baseX) * (1 - cos)) / sin;
        }
        //求点到几何的 垂足
        public static GeoLineString VerticalPtOfPtToGeometry(GeoPoint pt, Geometry geom, out GeoPoint ptVertical, out int ptIndex )
        {
            ptVertical = new GeoPoint();
            ptIndex = -1;

            if (geom is GeoLineString)
            {
                GeoLineString m_Line = geom as GeoLineString;
                if (VerticalPtofPtToLineString(pt, m_Line, out ptVertical, out ptIndex))
                    return m_Line;               
            }
            else if (geom is GeoPolygon)
            {
                GeoPolygon plg = geom as GeoPolygon;
                GeoLineString line = plg.ExteriorRing;
                bool bVertical = VerticalPtofPtToLineString(pt, line, out ptVertical, out ptIndex);
                if (!bVertical)
                {
                    for (int i = 0; i < plg.InteriorRings.Count; i++)
                    {
                        if (VerticalPtofPtToLineString(pt, plg.InteriorRings[i], out ptVertical, out ptIndex))
                        {
                            return plg.InteriorRings[i];
                        }
                    }
                    return null;
                }
                else
                {
                    return line;
                }
            }
            else if (geom is GeoMultiLineString)
            {
                GeoMultiLineString lines = geom as GeoMultiLineString;
                for (int i = 0; i < lines.NumGeometries; i++)
                {
                    if (VerticalPtofPtToLineString(pt, lines[i], out  ptVertical, out  ptIndex))
                    {
                        return lines[i];
                    }
                }
                return null;
            }
            return null;
        }


        //点到直线的垂足，在线段内 。最短垂距 PT 为 外点，PT1 PT2 为线段端点。
        public static bool VerticalPtofPtToLineString(GeoPoint pt, GeoLineString line, out GeoPoint ptVertical,out int ptIndex)
        {
            ptVertical = new GeoPoint();
            ptIndex = -1;
            GeoPoint ptTemp = new GeoPoint();
            double minDist = double.MaxValue;
            for (int i = 0; i < line.NumPoints - 1; i++)
            {
                if (VerticalPtofPtToLine(pt, line.Vertices[i], line.Vertices[i + 1], out ptTemp))
                {
                    if (PtToLine(ptTemp, line.Vertices[i], line.Vertices[i + 1]) == 3)
                    {
                        double temp = ptTemp.DistanceTo( pt);
                        if ( temp < minDist)
                        {
                            minDist = temp;
                            ptVertical = ptTemp;
                            ptIndex = i + 1;
                        }                      
                    }
                }
            }
            if (ptIndex != -1)
                return true;
            return false;
        }
        //点到线段的垂足， 可以在线段外，  PT 为 外点，PT1 PT2 为线段端点。
        public static bool VerticalPtofPtToLine(GeoPoint pt, GeoPoint pt1, GeoPoint pt2, out GeoPoint ptVertical)
        {
            ptVertical = new GeoPoint();
            double a, b, c, d, ab2;
            ptVertical.X = pt.X;
            ptVertical.Y = pt.Y;

            a = pt1.Y - pt2.Y;
            b = pt2.X - pt1.X;
            c = pt2.Y * pt1.X - pt1.Y * pt2.X; //ax+by+c=0(过(x1,y1)-(x2,y2)的直线公式)

            ab2 = a * a + b * b;
            if (ab2 < 1e-4)
                return false;
            else
            {
                d = (a * pt.X + b * pt.Y + c) / ab2;	//点到直线的距离公式
                ptVertical.X = pt.X - a * d;
                ptVertical.Y = pt.Y - b * d; //求点x1,y1到直线ax+by+c=0的垂点
            }

            return true;
        }
        public static bool Pt2Line_VerticalPt(double x0, double y0, double x1, double y1, double x2, double y2, out double xv, out double yv)
        {
            double a, b, c, d, ab2;
            xv = x0;
            yv = y0;

            a = y1 - y2;
            b = x2 - x1;
            c = y2 * x1 - y1 * x2; //ax+by+c=0(过(x1,y1)-(x2,y2)的直线公式)

            ab2 = Math.Sqrt(a) + Math.Sqrt(b);
            if (ab2 < 1e-4)
                return false;
            else
            {
                d = (a * x0 + b * y0 + c) / ab2;	//点到直线的距离公式
                xv = x0 - a * d;
                yv = y0 - b * d; //求点x1,y1到直线ax+by+c=0的垂点
            }

            return true;
        }
        /// <summary>
        /// 用于点选时判断点选时判断点是否在线段上
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="pt1">线段上的第一点</param>
        /// <param name="pt2">线段上的第二点</param>
        /// <returns></returns>
        public static bool IsOnLineCoarse(GeoPoint pt, GeoPoint pt1, GeoPoint pt2)
        {
            if (pt.X < Math.Min(pt1.X, pt2.X) - Tolerance || pt.X > Math.Max(pt1.X, pt2.X) + Tolerance
                || pt.Y < Math.Min(pt1.Y, pt2.Y) - Tolerance || pt.Y > Math.Max(pt1.Y, pt2.Y) + Tolerance)
                return false;

            double dist = DistanceOfPtToLine(pt, pt1, pt2);//点直线距离公式的平方
            if (dist < m_PowerOfToler)
            {
                return true;
            }
            return false;
        }


        public static bool IsOnPointCoarse(GeoPoint pt1, GeoPoint pt2)
        {
            if (Math.Abs(pt1.X - pt2.X) < Tolerance && Math.Abs(pt1.Y - pt2.Y) < Tolerance)
            {
                return true;
            }
            return false;
        }

        // 判断点p0是否在线p1与p2上
        public static double Multiply(GeoPoint p1, GeoPoint p2, GeoPoint p0)
        {
            return ((p1.X - p0.X) * (p2.Y - p0.Y) - (p2.X - p0.X) * (p1.Y - p0.Y));
        }
        public static GeoLineString IntersectTwoLine(GeoPoint pt1, GeoPoint pt2, Geometry geom, out int index)
        {
            index = -1;
            if (geom is GeoLineString)
            {
                if( IntersectTwoLine(pt1, pt2, (GeoLineString)geom, out index))
                {
                    return (GeoLineString)geom;
                }
            }
            else if (geom is GeoMultiLineString)
            {
                GeoMultiLineString lines = geom as GeoMultiLineString;
                for (int i = 0; i < lines.NumGeometries; i++)
                {
                    if (IntersectTwoLine(pt1, pt2, lines[i], out index))
                    {
                        return lines[i];
                    }
                }
            }
            else if (geom is GeoPolygon)
            {
                GeoPolygon plg = geom as GeoPolygon;
                if (IntersectTwoLine(pt1, pt2, plg.ExteriorRing, out index))
                {
                    return plg.ExteriorRing;
                }
                else
                {
                    for (int i = 0; i < plg.InteriorRings.Count; i++)
                    {
                        if (IntersectTwoLine(pt1, pt2, plg.InteriorRings[i], out index))
                        {
                            return  plg.InteriorRings[i];
                        }
                    }
                }
            }
            return null;
        }
        public static bool IntersectTwoLine(GeoPoint pt1, GeoPoint pt2, GeoLineString line, out int index)
        {
            index = -1;
            double minDist = double.MaxValue;
            for (int i = 0; i < line.NumPoints - 1; i++)
            {
                if (IntersectTwoLine(pt1, pt2, line.Vertices[i], line.Vertices[i + 1]))
                {
                    double distTemp = DistanceOfPtToLine(pt1, line.Vertices[i], line.Vertices[i + 1]);
                    if (distTemp < minDist)
                    {
                        index = i;
                    }
                    
                }
            }
            if (index != -1)
                return true;
            return false;
        }

        //判断两条线段是否相交
        public static bool IntersectTwoLine(GeoPoint pt1, GeoPoint pt2, GeoPoint pt3, GeoPoint pt4)
        {
            return ((Math.Max(pt1.X, pt2.X) >= Math.Min(pt3.X, pt4.X)) &&
                (Math.Max(pt3.X, pt4.X) >= Math.Min(pt1.X, pt2.X)) &&
                (Math.Max(pt1.Y, pt2.Y) >= Math.Min(pt3.Y, pt4.Y)) &&
                (Math.Max(pt3.Y, pt4.Y) >= Math.Min(pt1.Y, pt2.Y)) &&
                (Multiply(pt3, pt2, pt1) * Multiply(pt2, pt4, pt1) >= 0) &&
                (Multiply(pt1, pt4, pt3) * Multiply(pt4, pt2, pt3) >= 0));
        }

        public static double determinant(double v1, double v2, double v3, double v4)  // 行列式  
        {
            return (v1 * v4 - v2 * v3);
        }
        //判断两条线段是否相交WHS
        public static bool IntersectTwoLineWHS(GeoPoint aa, GeoPoint bb, GeoPoint cc, GeoPoint dd)
        {
            double delta = determinant(bb.X - aa.X, cc.X - dd.X, bb.Y - aa.Y, cc.Y - dd.Y);
            if (delta <= Geometry.EPSIONAL && delta >= -Geometry.EPSIONAL)  // delta=0，表示两线段重合或平行  
            {
                return false;
            }
            double namenda = determinant(cc.X - aa.X, cc.X - dd.X, cc.Y - aa.Y, cc.Y - dd.Y) / delta;
            if (namenda > 1 || namenda < 0)
            {
                return false;
            }
            double miu = determinant(bb.X - aa.X, cc.X - aa.X, bb.Y - aa.Y, cc.Y - aa.Y) / delta;
            if (miu > 1 || miu < 0)
            {
                return false;
            }
            return true;
        } 

        /// <summary>
        /// 判断点pt和线段pt1、pt2的关系，若pt与起端点pt1相等则返回1，若与终端点pt2相等则返回2，如果在线段内部返回3
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="pt1">线段上的第一点</param>
        /// <param name="pt2">线段上的第二点</param>
        /// <returns> 若pt与起端点pt1相等则返回1，若与终端点pt2相等则返回2，如果在线段内部返回3</returns>
        public static int PtToLine(GeoPoint pt, GeoPoint pt1, GeoPoint pt2)
        {
            if (pt1.IsEqual(pt))
                return 1;
            if (pt2.IsEqual(pt))
                return 2;

            if (pt.X + Geometry.EPSIONAL < Math.Min(pt1.X, pt2.X) || pt.X - Geometry.EPSIONAL > Math.Max(pt1.X, pt2.X)
                || pt.Y + Geometry.EPSIONAL < Math.Min(pt1.Y, pt2.Y) || pt.Y - Geometry.EPSIONAL > Math.Max(pt1.Y, pt2.Y))
                return 0;

            double a = pt1.Y - pt2.Y;
            double b = pt2.X - pt1.X;
            double c = pt2.Y * pt1.X - pt1.Y * pt2.X;

            //ax + by + c= 0时，表示点在直线上
            double aa = Math.Abs(a * pt.X + b * pt.Y + c);
            if (aa < 1)
            {
                int testdata = 0;
            }
            if (Math.Abs(a * pt.X + b * pt.Y + c) <= 0.05)  //汪2017/8/2修改（由于判断点是否在线上，误差过大，导致包含关系不准确
                return 3;
            return 0;
        }

        /// <summary>
        /// 判断点pt和线段pt1、pt2的关系，若pt与起端点pt相等则返回1，如果在线段内部返回2
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="pt1">线段上的第一点</param>
        /// <param name="pt2">线段上的第二点</param>
        /// <returns> 若pt与起端点pt相等则返回1，如果在线段内部返回2</returns>
        public static int PtToLineWHS(GeoPoint pt, GeoPoint pt1, GeoPoint pt2)
        {
            if ((Math.Abs(pt.X - pt1.X) < Geometry.EPSIONAL && Math.Abs(pt.Y - pt1.Y) < Geometry.EPSIONAL) || (Math.Abs(pt.X - pt2.X) < Geometry.EPSIONAL && Math.Abs(pt.Y - pt2.Y) < Geometry.EPSIONAL))
                return 1;
            if (pt.X + Geometry.EPSIONAL < Math.Min(pt1.X, pt2.X) || pt.X - Geometry.EPSIONAL > Math.Max(pt1.X, pt2.X)
                || pt.Y + Geometry.EPSIONAL < Math.Min(pt1.Y, pt2.Y) || pt.Y - Geometry.EPSIONAL > Math.Max(pt1.Y, pt2.Y))
                return 0;

            double a = pt1.Y - pt2.Y;
            double b = pt2.X - pt1.X;
            double c = pt2.Y * pt1.X - pt1.Y * pt2.X;

            //ax + by + c= 0时，表示点在直线上
            if (Math.Abs(a * pt.X + b * pt.Y + c) <= Geometry.EPSIONAL)  //汪2017/8/2修改（由于判断点是否在线上，误差过大，导致包含关系不准确
                return 2;
            return 0;
        }




        //判断点x0,y0是否在线段(x1,y1)-(x2,y2)上
        public static bool PointOnLine(double x0, double y0, double x1, double y1, double x2, double y2)
        {
            bool result = false;
            result = ((x1 - x0) * (x2 - x0) <= 1e-4) && ((y1 - y0) * (y2 - y0) <= 1e-4);//判断点x0,y0是否在线段内部
            if (result)
                result = (Math.Abs(y0 * x2 - y0 * x1 - y1 * x2 + y1 * x1 - y2 * x0 + y2 * x1 + y1 * x0 - x1 * y1) < 1e-3);
            return result;
        }
        //判断点与线的关系
        //1:在端点上
        //2:在线上
        //0:不在线上
        public static int point_tpwith_line(double x0, double y0, double x1, double y1, double x2, double y2)
        {
            if (((Math.Abs(x0 - x1) < 1e-4 && Math.Abs(y0 - y1) < 1e-4)) || ((Math.Abs(x0 - x2) < 1e-4 && Math.Abs(y0 - y2) < 1e-4)))
                return 1;
            if (!((x1 - x0) * (x2 - x0) <= 1e-4) && ((y1 - y0) * (y2 - y0) <= 1e-4))
                return 0;
            if ((Math.Abs(y0 * x2 - y0 * x1 - y1 * x2 + y1 * x1 - y2 * x0 + y2 * x1 + y1 * x0 - x1 * y1) < 1e-3))
                return 2;
            return 0;
        }
        //老曾
        //判断点
        public static bool LineInSearchRadius(double x0, double y0, double x1, double y1, double x2, double y2, double rds)
        {
            double vx, vy, ds;

            if (Pt2Line_VerticalPt(x0, y0, x1, y1, x2, y2, out vx, out vy))
            {
                ds = x0 * x0 + y0 * y0;
                if (ds < rds * rds)
                {
                    if (PointOnLine(vx, vy, x1, y1, x2, y2))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 射线法判断点是否在圆环内部  
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="ring"></param>
        /// <returns> 0 相离 1 端点 2 线上 3 内部,关系准确)</returns>
        public static int IsInLinearRing(GeoPoint pt, GeoLinearRing ring)
        {
            ring.MakeClosed();
            GeoBound bound = ring.Bound;
            if (!bound.IsPointIn(pt))
            {
                return 0;
            }
            int p = 0;
            for (int i = 0; i < ring.NumPoints - 1; i++)
            {
                p = PtToLine(pt, ring.Vertices[i], ring.Vertices[i + 1]);
                if (p > 0)
                {
                    if (p > 2)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }

            GeoPoint pt1 = new GeoPoint();
            GeoPoint pt2 = new GeoPoint();
            GeoPoint pt3 = new GeoPoint(-1.0e+100, pt.Y);
            int count = 0;
            for (int i = 0; i < ring.NumPoints - 1; i++)
            {//射线法判断点是否在多边形内
                pt1.SetXY(ring.Vertices[i].X, ring.Vertices[i].Y);
                pt2.SetXY(ring.Vertices[i + 1].X, ring.Vertices[i + 1].Y);

                if (Math.Abs(pt1.Y - pt2.Y) < Geometry.EPSIONAL)
                {
                    continue;
                }

                if (PtToLine(pt1, pt, pt3) == 3)
                {
                    //加上这个判断条件是为了防止一个节点在前后两条线段的判断中被重复的计数
                    if (pt1.Y > pt2.Y)
                        count++;
                }
                else if (PtToLine(pt2, pt, pt3) == 3)
                {
                    if (pt2.Y > pt1.Y)
                        count++;
                }
                else if (IntersectTwoLine(pt1, pt2, pt3, pt))
                {
                    count++;
                }
            }
            if (count % 2 == 1)
            {
                return 3; //3  在多边形内	
            }
            return 0;
        }

        /// <summary>
        /// 射线法判断点是否在圆环内部  
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="ring"></param>
        /// <returns> 0 相离 1 端点 2 线上 3 内部,关系准确)</returns>
        public static int IsInLinearRingWHS(GeoPoint pt, GeoLinearRing ring)
        {
            ring.MakeClosed();
            GeoBound bound = ring.Bound;
            if (!bound.IsPointIn(pt))
            {
                return 0;
            }
            int p = 0;
            //for (int i = 0; i < ring.NumPoints - 1; i++)
            //{
            //    p = PtToLine(pt, ring.Vertices[i], ring.Vertices[i + 1]);
            //    if (p > 0)
            //    {
            //        if (p > 2)
            //        {
            //            return 2;
            //        }
            //        else
            //        {
            //            return 1;
            //        }
            //    }
            //}
            GeoPoint pt1 = new GeoPoint();
            GeoPoint pt2 = new GeoPoint();
            GeoPoint pt3 = new GeoPoint(-1.0e+100, pt.Y);
            int count = 0;
            for (int i = 0; i < ring.NumPoints - 1; i++)
            {//射线法判断点是否在多边形内
                pt1.SetXY(ring.Vertices[i].X, ring.Vertices[i].Y);
                pt2.SetXY(ring.Vertices[i + 1].X, ring.Vertices[i + 1].Y);
                p = PtToLine(pt, pt1, pt2);
                if (p > 2)
                    return 2;
                else if (p > 0)
                    return 1;
                if (Math.Abs(pt1.Y - pt2.Y) < Geometry.EPSIONAL)
                {
                    continue;
                }
                double delta = Math.Max(Math.Abs(pt.X - pt1.X), Math.Abs(pt.X - pt2.X));
                if (delta < 1)
                {
                    delta = 50;
                }
                pt3.X = 0;// pt.X - delta * 2;//防止精度误差被放大
                if ((Math.Abs(pt1.Y - pt.Y) < Geometry.EPSIONAL) && pt1.X < pt.X)
                {
                    if (pt1.Y > pt2.Y)
                        count++;
                }
                //if (PtToLine(pt1, pt, pt3) == 3)
                //{
                //    //加上这个判断条件是为了防止一个节点在前后两条线段的判断中被重复的计数
                //    if (pt1.Y > pt2.Y)
                //        count++;
                //}
                else if ((Math.Abs(pt2.Y - pt.Y) < Geometry.EPSIONAL) && pt2.X < pt.X)
                {
                    if (pt2.Y > pt1.Y)
                        count++;
                }
                //else if (PtToLine(pt2, pt, pt3) == 3)//pt2的Y坐标与pt的Y坐标相等！！！！
                //{
                //    if (pt2.Y > pt1.Y)
                //        count++;
                //}
                else if (IntersectTwoLine(pt1, pt2, pt3, pt))
                {
                    count++;
                }
            }
            if (count % 2 == 1)
            {
                return 3; //3  在多边形内	
            }
            return 0;
        }

        
        
        
        
        public static bool ComputeCircleRadius(double x1, double y1, double x2, double y2, double x3, double y3,
                            out double cex, out double cey, out double r)
        {
            double xy1, xy2, xy3, xy0;
            xy1 = Math.Pow(x1, 2) + Math.Pow(y1, 2);
            xy2 = Math.Pow(x2, 2) + Math.Pow(y2, 2);
            xy3 = Math.Pow(x3, 2) + Math.Pow(y3, 2);
            xy0 = (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
            cex = 0.5 * ((xy2 - xy1) * (y3 - y1) - (xy3 - xy1) * (y2 - y1));
            cey = -0.5 * ((xy2 - xy1) * (x3 - x1) - (xy3 - xy1) * (x2 - x1));

            if (Math.Abs(xy0) < Geometry.EPSIONAL)
            {
                cex = (x1 + x2) / 2;
                cey = (y1 + y2) / 2;
                r = 0;
                return false;
            }
            else
            {
                cex /= xy0;
                cey /= xy0;
                r = Math.Sqrt(Math.Pow(cex - x1, 2) + Math.Pow(cey - y1, 2));
                return true;
            }
        }
        public static double CalcAngle(GeoPoint StartPt, GeoPoint CenterPt, GeoPoint Pt)
        {
            double angle1 = GeoAlgorithm.CalcAzimuth(CenterPt.X, CenterPt.Y, StartPt.X, StartPt.Y);
            double angle2 = GeoAlgorithm.CalcAzimuth(CenterPt.X, CenterPt.Y, Pt.X, Pt.Y);
            double angle = angle2 - angle1;//角度从逆时针开始算起

            if (angle < 0)
            {
                angle += 2 * Math.PI;
            }

            double angleArc = angle * 180 / Math.PI;
            return angleArc;
        }

        public static double CalcAzimuth(double x1, double y1, double x2, double y2)
        {
            double x = x2 - x1;
            double y = y2 - y1;
            double ang = Math.Atan2(y, x);//atan2函数求的角度为直线与x轴正半轴的夹角（-PI 至 PI之间)
            if (ang < 0)
                ang += 2 * Math.PI;
            return ang;
        }

        public static List<GeoPoint> ThreePointsArc(GeoPoint pt1, GeoPoint pt2, GeoPoint pt3, ArcType type)
        {
            GeoPoint pt1Backup = (GeoPoint)pt1.Clone();

            List<GeoPoint> ArcPts = new List<GeoPoint>();

            double cex, cey, r;

            //如果三点在同一条直线上则返回-1;
            if (!ComputeCircleRadius(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, out  cex, out  cey, out  r))
                return null;

            //分别获得三个点与圆心的方位角
            double ang1 = CalcAzimuth(cex, cey, pt1.X, pt1.Y);
            double ang2 = CalcAzimuth(cex, cey, pt2.X, pt2.Y);
            double ang3 = CalcAzimuth(cex, cey, pt3.X, pt3.Y);

            //用来记录中间变量的点和角度
            GeoPoint pt = new GeoPoint();
            double ang;


            if (ang1 > ang3)	//ang1 ang3排序
            {
                //交换角度和点
                ang = ang1;
                ang1 = ang3;
                ang3 = ang;

                pt = pt3;
                pt3 = pt1;
                pt1 = pt;
            }

            double ang0 = ang3 - ang1;	//夹角

            //如果ang2不在第一点和第三点的中间
            //则交换位置，弧段的绘制颠倒方向
            if (!((ang2 > ang1) && (ang2 < ang3)))
            {
                //交换角度和点
                ang = ang1;
                ang1 = ang3;
                ang3 = ang;

                pt = pt3;
                pt3 = pt1;
                pt1 = pt;
            }

            double x, y;
            int nCount = 16;		//平分为10段
            double stepAngle = ang0 / nCount;	//插值步值
            ang = ang1;

            ArcPts.Add(pt1);

            //从第一点绘制到第二点
            while (Math.Abs(ang - ang2) > stepAngle)
            {
                if (ArcPts.Count == 799)
                {
                    return null;
                }
                x = cex + r * Math.Cos(ang);
                y = cey + r * Math.Sin(ang);

                ArcPts.Add(new GeoPoint(x, y));
                ang += stepAngle;

                if (ang > Math.PI + Math.PI)
                    ang -= (Math.PI + Math.PI);
            }

            ArcPts.Add(pt2);


            ang = ang2;
            //从第二点绘制到第三点
            while (Math.Abs(ang - ang3) > stepAngle)
            {
                if (ArcPts.Count == 799)
                {
                    return null;
                }
                x = cex + r * Math.Cos(ang);
                y = cey + r * Math.Sin(ang);

                ArcPts.Add(new GeoPoint(x, y));

                ang += stepAngle;
                if (ang > Math.PI + Math.PI)
                    ang -= (Math.PI + Math.PI);
            }

            ArcPts.Add(pt3);

            if (type == ArcType.Circle)
            {
                ang = ang3;
                while (Math.Abs(ang - ang1) > stepAngle)
                {
                    x = cex + r *Math.Cos(ang);
                    y = cey + r * Math.Sin(ang);

                    ArcPts.Add(new GeoPoint(x, y));

                   

                    ang += stepAngle;
                    if (ang > Math.PI + Math.PI)
                        ang -= (Math.PI + Math.PI);
                }
                if (ArcPts.Count == 799)
                {
                    return null;
                }
                ArcPts.Add(pt1);
                return ArcPts;
            }
            if (!pt1Backup.IsEqual(pt1))
                ArcPts.Reverse();
            return ArcPts;

        }
        //延长线段PT1，PT2 至距离length长度
        public static GeoPoint ExtentLine(GeoPoint pt1, GeoPoint pt2, double length)
        {
            double ang = CalcAzimuth(pt1.X, pt1.Y, pt2.X, pt2.Y);
            GeoPoint pt = new GeoPoint();
            pt.X = length * Math.Cos(ang) + pt1.X;
            pt.Y = length * Math.Sin(ang) + pt1.Y;
            return pt;
        }
        //求取x,y绕(baseX,baseY)旋转angle后的坐标,angle以逆时针方向为正 
        public static GeoPoint PointRotate(GeoPoint rotPt, GeoPoint basePt, double angle)
        {
           // angle = angle * Math.PI / 180;
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            GeoPoint result = new GeoPoint();

            result.X = -rotPt.Y * sin + rotPt.X * cos + basePt.X * (1.0 - cos) + basePt.Y * sin;
            result.Y = rotPt.Y * cos + rotPt.X * sin + basePt.Y * (1.0 - cos) - basePt.X * sin;
            return result;
        }
        public static void ClearRepeatPoints(List<GeoPoint> ptList)
        {
            for (int i = 1; i < ptList.Count; ++i)
            {
                if (ptList[i - 1].IsEqual(ptList[i]))
                {
                    ptList.RemoveAt(i);
                    --i;
                }
            }
        }
        public static List<GeoPoint> CubicSpline(List<GeoPoint> m_Points)
        {
            ClearRepeatPoints(m_Points);
            int N = m_Points.Count;
            if (N < 3)
            {
                return null;
            }
          
            List<GeoPoint> Spline = new List<GeoPoint>();

            double[] s = new double[N + 1];
            double[] Mx = new double[N + 1];
            double[] My = new double[N + 1];
            double[] sAll = new double[N + 1];
            if (!m_Points[0].IsEqual(m_Points[N - 1]))//如果不封闭
            {
                double[] u = new double[N + 1];
                double[] v = new double[N + 1];
                double[] v1 = new double[N + 1];
                double[] b = new double[N + 1];
                double[] b1 = new double[N + 1];


                sAll[0] = 0;
                for (int i = 1; i < N; i++)
                {
                    s[i] = m_Points[i - 1].DistanceTo(m_Points[i]);
                    b[i] = (m_Points[i].X - m_Points[i - 1].X) / s[i];
                    b1[i] = (m_Points[i].Y - m_Points[i - 1].Y) / s[i];
                    sAll[i] = sAll[i - 1] + s[i];
                }
                u[2] = 2 * (s[1] + s[2]);
                v[2] = 6 * (b[2] - b[1]);
                v1[2] = 6 * (b1[2] - b1[1]);
                for (int i = 3; i < N; i++)
                {
                    u[i] = 2 * (s[i - 1] + s[i]) - s[i - 1] * s[i - 1] / u[i - 1];
                    v[i] = 6 * (b[i] - b[i - 1]) - s[i - 1] * v[i - 1] / u[i - 1];
                    v1[i] = 6 * (b1[i] - b1[i - 1]) - s[i - 1] * v1[i - 1] / u[i - 1];
                }
                Mx[N] = 0;
                Mx[1] = 0;
                My[N] = 0;
                My[1] = 0;
                for (int i = N - 1; i >= 2; i--)
                {
                    Mx[i] = (v[i] - s[i] * Mx[i + 1]) / u[i];
                    My[i] = (v1[i] - s[i] * My[i + 1]) / u[i];
                }

            }
            else//如果封闭
            {
                double[] x = new double[N];
                double[] y = new double[N];
                s[0] = 0;
                sAll[0] = 0;
                for (int i = 1; i < N; i++)
                {
                    s[i] = m_Points[i - 1].DistanceTo(m_Points[i]);
                    sAll[i] = sAll[i - 1] + s[i];
                }
                for (int i = 0; i < N; ++i)
                {
                    x[i] = m_Points[i].X;
                    y[i] = m_Points[i].Y;
                }
                double[][] a = new double[N - 1][];
                double[][] a1 = new double[N - 1][];
                for (int i = 0; i < N - 1; ++i)
                {
                    a[i] = new double[N - 1];
                    a1[i] = new double[N - 1];
                }
                double[] b = new double[N - 1];
                double[] b1 = new double[N - 1];
                GenarateMatrix(sAll, x, a, b, N);
                GenarateMatrix(sAll, y, a1, b1, N);
                gauss_cpivot(N - 1, a, b);
                gauss_cpivot(N - 1, a1, b1);
                for (int i = 0; i < N - 1; ++i)
                {
                    Mx[i + 2] = b[i];
                    My[i + 2] = b1[i];

                }
                Mx[1] = Mx[N];
                My[1] = My[N];
            }

            for (int i = 0; i < N - 1; ++i)
            {
                double disBef = sAll[i];
                double disInterval = s[i + 1];
                double spiltpart = disInterval / 5;
                for (int j = 0; j < spiltpart; ++j)
                {
                    double x = disBef + disInterval * j / spiltpart;
                    int part = i + 1;
                    double xx, yy, x_ti, ti1_x;

                    x_ti = x - sAll[part - 1];
                    ti1_x = sAll[part] - x;
                    yy = My[part + 1] * x_ti * x_ti * x_ti / (6 * s[part]) + My[part] * ti1_x * ti1_x * ti1_x / (6 * s[part])
                        + (m_Points[part].Y / s[part] - s[part] * My[part + 1] / 6.0) * x_ti + (m_Points[part - 1].Y / s[part] - s[part] * My[part] / 6.0) * ti1_x;

                    xx = Mx[part + 1] * x_ti * x_ti * x_ti / (6 * s[part]) + Mx[part] * ti1_x * ti1_x * ti1_x / (6 * s[part])
                        + (m_Points[part].X / s[part] - s[part] * Mx[part + 1] / 6.0) * x_ti + (m_Points[part - 1].X / s[part] - s[part] * Mx[part] / 6.0) * ti1_x;
                    Spline.Add(new GeoPoint(xx, yy));
                }
            }

            return Spline;
        }

        private static int gauss_cpivot(int n, double[][] a, double[] b)
        {
            int i, j, k, row = 0;
            double maxp, t;
            for (k = 0; k < n; k++)
            {
                for (maxp = 0, i = k; i < n; i++)
                {
                    if (Math.Abs(a[i][k]) > Math.Abs(maxp))
                    {
                        maxp = a[row = i][k];
                    }
                }
                if (Math.Abs(maxp) < 0.000001) return 0;
                if (row != k)
                {
                    for (j = k; j < n; j++)
                    {
                        t = a[k][j]; a[k][j] = a[row][j]; a[row][j] = t;
                    }
                    t = b[k]; b[k] = b[row]; b[row] = t;
                }
                for (j = k + 1; j < n; j++)
                {
                    a[k][j] /= maxp;
                    double mmm = a[k][j];
                    for (i = k + 1; i < n; i++)
                    {
                        a[i][j] -= a[i][k] * a[k][j];
                        double m = a[i][j];
                        double zz = m;
                    }
                }
                b[k] /= maxp;
                for (i = k + 1; i < n; i++)
                    b[i] -= b[k] * a[i][k];
            }

            for (i = n - 1; i >= 0; i--)
                for (j = i + 1; j < n; j++)
                    b[i] -= a[i][j] * b[j];
            return 1;
        }

        private static void GenarateMatrix(double[] x, double[] y, double[][] a, double[] b, int n)
        {
            double[] h = new double[n + 1];
            double[] u = new double[n + 1];
            double[] v = new double[n + 1];

            for (int i = 0; i < n - 1; ++i)
                h[i] = x[i + 1] - x[i];

            for (int i = 1; i < n - 1; ++i)
            {
                v[i] = h[i] / (h[i] + h[i - 1]);

                u[i] = 1 - v[i];

                b[i - 1] = ((y[i + 1] - y[i]) / h[i] - (y[i] - y[i - 1]) / h[i - 1]) * 6 / (h[i] + h[i - 1]);

            }
            v[n - 1] = h[0] / (h[0] + h[n - 2]);

            u[n - 1] = 1 - v[n - 1];

            b[n - 2] = ((y[1] - y[0]) / h[0] - (y[n - 1] - y[n - 2]) / h[n - 2]) * 6 / (h[0] + h[n - 2]);


            for (int i = 0; i < n - 1; ++i)
                for (int j = 0; j < n - 1; ++j)
                {
                    if (i == j)
                        a[i][j] = 2;
                    else if ((j - i) == 1)
                        a[i][j] = v[i + 1];
                    else if ((i - j) == 1)
                        a[i][j] = u[i + 1];
                    else a[i][j] = 0;
                }
            a[0][n - 2] = u[1];
            a[n - 2][0] = v[n - 1];
        }

        public static GeoPoint SymmetryPtOfLine(GeoPoint ptSymmetry, GeoPoint pt1, GeoPoint pt2)
        {
            //直线AX +BY +C =0;
            double A = pt2.Y - pt1.Y;
            double B = pt1.X - pt2.X;
            double C = pt1.Y * (pt2.X - pt1.X) - pt1.X * (pt2.Y - pt1.Y);

            double x = ptSymmetry.X - 2 * A*(A * ptSymmetry.X + B * ptSymmetry.Y + C) / (A * A + B * B);
            double y = ptSymmetry.Y - 2 * B*(A * ptSymmetry.X + B * ptSymmetry.Y + C) / (A * A + B * B);
            return new GeoPoint(x, y);
        }
        //根据两点和弧度，求第三点坐标
        public static GeoPoint g_Calu_thirdPoint(GeoPoint pt1, GeoPoint pt2, double bugle)
        {
            double angle = 4 * Math.Atan(bugle);
            double xx = angle * 180 / Math.PI;

            double dist = pt1.DistanceTo(pt2);
            double r = dist / 2 / Math.Sin(angle / 2);
            double baseX, baseY;
            CalcCenterPoint(pt1.X, pt1.Y, angle, pt2.X, pt2.Y, out baseX, out baseY);
            return PointRotate(pt1,new GeoPoint( baseX, baseY), angle / 2);
        }

        public static  List<GeoPoint> ThreePointRect(List<GeoPoint> m_PtList)
        {
            GeoPoint ptVertical = null;
            if (GeoAlgorithm.VerticalPtofPtToLine(m_PtList[2], m_PtList[0], m_PtList[1], out ptVertical))
            {

                double dist0 = ptVertical.DistanceTo(m_PtList[0]);
                double dist1 = ptVertical.DistanceTo(m_PtList[1]);
                List<GeoPoint> rectList = new List<GeoPoint>();
                GeoPoint StartPt = null;
                GeoPoint CenterPt = null;
                GeoPoint ptThird = null;
                GeoPoint ptForth = null;
                double DistOfPt0ToPt1 = m_PtList[0].DistanceTo(m_PtList[1]);
                if (dist1 < dist0)
                {
                    StartPt = m_PtList[0];
                    CenterPt = m_PtList[1];
                }
                else
                {
                    StartPt = m_PtList[1];
                    CenterPt = m_PtList[0];
                }
                double distance = m_PtList[2].DistanceTo(CenterPt);
                double angle = GeoAlgorithm.CalcAngle(StartPt, CenterPt, m_PtList[2]);
                if (angle > 180)
                    distance *= -1;
                ptThird = GeoAlgorithm.CalcuLineLCommand(StartPt, CenterPt, distance);


                angle = GeoAlgorithm.CalcAngle(CenterPt, ptThird, StartPt);
                if (angle > 180)
                    DistOfPt0ToPt1 *= -1;

                ptForth = GeoAlgorithm.CalcuLineLCommand(CenterPt, ptThird, DistOfPt0ToPt1);
                rectList.Add(StartPt);
                rectList.Add(CenterPt);
                rectList.Add(ptThird);
                rectList.Add(ptForth);
                rectList.Add(StartPt.Clone() as GeoPoint);
                return rectList;
            }
            return null;
        }

        public static GeoMultiLineString MultiLineModifiedByLine(GeoMultiLineString lines, List<GeoPoint> m_PtList)
        {
            GeoMultiLineString DestLines = lines.Clone() as GeoMultiLineString;
            bool bModified = false;
            for (int i = 0; i < lines.NumGeometries; i++)
            {
                GeoLineString DestLine = LineModifiedByLine(lines.LineStrings[i], m_PtList);
                if (DestLine != null)
                {
                    bModified = true;
                    DestLines.LineStrings[i] = DestLine;
                }
            }
            if (bModified)
                return DestLines;
            else
                return null;
        }
        public static GeoPolygon PolygonModifiedByLine(GeoPolygon plg, List<GeoPoint> m_PtList)
        {
            GeoPolygon DestPlg = plg.Clone() as GeoPolygon ;
            bool bModified = false;
            GeoLineString DestOutLine = LineModifiedByLine(plg.ExteriorRing, m_PtList);
            if (DestOutLine != null)
            {
                GeoLinearRing ring = new GeoLinearRing(DestOutLine.Vertices);
                DestPlg.ExteriorRing = ring;
                bModified = true;
            }
            for (int i = 0; i < plg.InteriorRings.Count; i++)
            {
                GeoLineString DestInLine =  LineModifiedByLine(plg.InteriorRings[i], m_PtList);
                if (DestInLine != null)
                {
                    GeoLinearRing ring = new GeoLinearRing(DestInLine.Vertices);
                    DestPlg.InteriorRings.Add(ring);
                    bModified = true;
                }
            }
            if (bModified)
                return DestPlg;
            else 
                return null;
        }
        public static GeoMultiPolygon MultiPolygonModifiedByLine(GeoMultiPolygon plgs, List<GeoPoint> m_PtList)
        {
            GeoMultiPolygon DestPlgs = plgs.Clone() as GeoMultiPolygon;
            bool bModified = false;
            for (int i = 0; i < plgs.NumGeometries; i++)
            {
                GeoPolygon DestPlg = PolygonModifiedByLine(plgs.Polygons[i], m_PtList);
                if (DestPlg != null)
                {
                    bModified = true;
                    DestPlgs.Polygons[i] = DestPlg;
                }
            }
            if (bModified)
                return DestPlgs;
            else
                return null;
        }
         public static  GeoLineString LineModifiedByLine(GeoLineString line, List<GeoPoint> m_PtList)
        {
            List<int> m_IntersectSrcList = new List<int>();
            List<int> m_IntersectIstList = new List<int>();
            List<GeoPoint> m_IntersectPts = new List<GeoPoint>();
            for (int i = 0; i < line.NumPoints-1; i++)
            {
                GeoPoint pt1 = line.Vertices[i];
                GeoPoint pt2 = line.Vertices[i + 1];
                List<GeoPoint> m_TmpPts = new List<GeoPoint>();
                List<int> m_TmpInsertIstList = new List<int> ();
                for (int j = 0; j < m_PtList.Count - 1; j++)
                {
                    GeoPoint pt3 = m_PtList[j];
                    GeoPoint pt4 = m_PtList[j + 1];
                    double X,Y;
                    if (CrossPtOfTwoLine(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y, out X, out Y))
                    {
                        if (PointOnLine(X, Y, pt1.X, pt1.Y, pt2.X, pt2.Y)&& PointOnLine(X,Y,pt3.X,pt3.Y,pt4.X,pt4.Y))
                        { 
                            m_TmpPts.Add(new GeoPoint(X, Y));
                            m_TmpInsertIstList.Add(j+1);
                        }
                    }
                }
                if (m_TmpPts.Count > 1)
                {
                    double dist1 = pt1.DistanceTo(m_TmpPts[0]);
                    double dist2 = pt1.DistanceTo(m_TmpPts[m_TmpPts.Count - 1]);
                    if (dist1 > dist2)
                    {
                        m_TmpPts.Reverse();
                        m_TmpInsertIstList.Reverse();
                    }
                }
                if (m_TmpPts.Count > 0)
                {
                    m_IntersectIstList.AddRange(m_TmpInsertIstList);
                  
                }
            }

            if (m_IntersectIstList.Count < 2)
                return null;

            if (m_IntersectIstList[0] > m_IntersectIstList[m_IntersectIstList.Count - 1])
                m_PtList.Reverse();

            m_IntersectIstList.Clear();

            for (int i = 0; i < line.NumPoints - 1; i++)
            {
                GeoPoint pt1 = line.Vertices[i];
                GeoPoint pt2 = line.Vertices[i + 1];
                List<GeoPoint> m_TmpPts = new List<GeoPoint>();
                List<int> m_TmpInsertIstList = new List<int>();
                for (int j = 0; j < m_PtList.Count - 1; j++)
                {
                    GeoPoint pt3 = m_PtList[j];
                    GeoPoint pt4 = m_PtList[j + 1];
                    double X, Y;
                    if (CrossPtOfTwoLine(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y, out X, out Y))
                    {
                        if (PointOnLine(X, Y, pt1.X, pt1.Y, pt2.X, pt2.Y) && PointOnLine(X, Y, pt3.X, pt3.Y, pt4.X, pt4.Y))
                        {
                            m_IntersectIstList.Add(j + 1);
                            m_IntersectSrcList.Add(i + 1);
                            m_IntersectPts.Add(new GeoPoint(X, Y));
                        }
                    }
                }
                
            }
 
            if (m_IntersectPts.Count > 1)
            {
                GeoLineString lineDest = new GeoLineString();

                int SrcLineBeginIndex = m_IntersectSrcList[0];
                int SrcLineEndIndex =m_IntersectSrcList[m_IntersectSrcList.Count - 1];

                int InsertLineBeginIndex = m_IntersectIstList[0];
                int InsertLineEndIndex = m_IntersectIstList[m_IntersectIstList.Count-1];
              

                for (int i = 0; i < SrcLineBeginIndex ; i++)
                {
                    lineDest.AddPoint(line.Vertices[i].Clone() as GeoPoint);
                }
                lineDest.AddPoint(m_IntersectPts[0].Clone() as GeoPoint);
                for (int i = Math.Min(InsertLineBeginIndex,InsertLineEndIndex); i < Math.Max(InsertLineBeginIndex,InsertLineEndIndex); i++)
                {
                    lineDest.AddPoint(m_PtList[i].Clone() as GeoPoint);
                }
                lineDest.AddPoint(m_IntersectPts[m_IntersectPts.Count - 1].Clone() as GeoPoint);
                for (int i = SrcLineEndIndex; i < line.NumPoints; i++)
                {
                    lineDest.AddPoint(line.Vertices[i].Clone() as GeoPoint);
                }
                return lineDest;
            }
            return null;
        }
         //public static double g_DEG(double ang)
         //{
         //    int nDeg, nMin;
         //    double dSec;
         //    nDeg =  Convert.ToInt32(ang);
         //    ang -= nDeg;
         //    ang *= 100;
         //    nMin = Convert.ToInt32(ang);
         //    ang -= nMin;
         //    ang *= 100;
         //    dSec = ang;

         //    return (nDeg + nMin / 60.0 + dSec / 3600.0) * 3.14159265 / 180;

         //}
    }
}

