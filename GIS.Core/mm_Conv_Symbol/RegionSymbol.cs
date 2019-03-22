using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace GIS.mm_Conv_Symbol
{


    public class RegionSymbol : Symbol_Base
    {
        protected LineSymbol m_outline;                //边界应该只有一条
        protected List<PointSymbol> m_fillsymbol;      //填充的点图元可能有好几种
        protected List<PointSymbol> m_mothersymbol;
        protected double eps = 0.05;
        public float m_udgap = 0.0f;
        public float m_lrgap = 0.0f;

        public RegionSymbol()
        {
            m_outline = new LineSymbol();
            m_fillsymbol = new List<PointSymbol>();
            m_mothersymbol = new List<PointSymbol>();
        }

        public override SymbolBound GetBoundingBox()
        {
            //throw new NotImplementedException();
            m_Bound.UnionBound(m_outline.Bound);
            for (int i = 0; i < m_fillsymbol.Count; i++)
            {
                m_Bound.UnionBound(m_fillsymbol[i].Bound);
            }
            return m_Bound;
        }

        public LineSymbol outline
        {
            get { return m_outline; }
            set { m_outline = value; }
        }
        public List<PointSymbol> mothersymbol
        {
            get { return m_mothersymbol; }
            set { m_mothersymbol = value; }
        }

        public List<PointSymbol> fillsymbol
        {
            get { return m_fillsymbol; }
            set { m_fillsymbol = value; }
        }

        public void appoint_outline(LineSymbol outline)
        {
            m_outline = outline;
        }
        public void initialself(PointF[] pts)
        {
            GeoLinearRing ring = new GeoLinearRing();
            for (int i = 0; i < pts.Length; i++)
            {
                ring.Vertices.Add(new GeoPoint(pts[i].X, pts[i].Y));
            }
            add_fillsymbol(new List<GeoLinearRing>(), ring, ring.Bound, m_mothersymbol.Count >= 1 ? m_mothersymbol[0] : null,
                m_mothersymbol.Count >= 2 ? m_mothersymbol[1] : null, m_udgap, m_lrgap);
        }

        public void add_fillsymbol(List<GeoLinearRing> InteriorRings, GeoLinearRing ExteriorRing, GeoBound bound,
            PointSymbol ptsymbol0, PointSymbol ptsymbol1, float udgap, float lrgap/*, bool lorr, bool bfirst*/)
            
        {
            GeoBound rectbound = bound.Clone();

            rectbound.Left += lrgap / 2;
            rectbound.Top -= udgap / 2;


            #region first pointsymbol
            if (ptsymbol0 == null)
                return;
            PointSymbol startsymbol0 = ptsymbol0.Clone();
            startsymbol0.Translate((float)rectbound.Left, (float)rectbound.Top);

            int nrow = (int)(rectbound.Height / udgap) + 1;
            int ncol = (int)(rectbound.Width / lrgap) + 1;
            int nncol = (int)((rectbound.Width - lrgap / 2) / lrgap) + 1;

            bool bodd = true;
            for (int j = 0; j < nrow; j++, bodd = !bodd)
            {

                int nnncol = bodd ? ncol : nncol;

                for (int i = 0; i < nnncol; i++)
                {
                    GeoPoint centerpt = new GeoPoint(bodd ? (rectbound.Left + lrgap * i) : (rectbound.Left + lrgap * i + lrgap / 2), 
                        rectbound.Top - udgap * j );

                    //center
                    if (!PtInRegion(InteriorRings, ExteriorRing, centerpt))
                    {
                        continue;
                    }

                    #region four vertices
                    //
                    //check four vertices
                    //
                    double top = centerpt.Y + ptsymbol0.Bound.Top - eps;
                    double left = centerpt.X - ptsymbol0.Bound.Left + eps;
                    double bottom = centerpt.Y - ptsymbol0.Bound.Bottom + eps;
                    double right = centerpt.X + ptsymbol0.Bound.Right - eps;

                    if (bodd)
                    {
                        if (!PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(left, top)) || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(left, bottom))
                            || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(right, top)) || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(right, bottom)))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(left + lrgap / 2, top)) || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(left + lrgap / 2, bottom))
                            || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(right + lrgap / 2, top)) || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(right + lrgap / 2, bottom)))
                        {
                            continue;
                        }
                    }
                    #endregion

                    PointSymbol insymbol0 = startsymbol0.Clone();
                    if (bodd)
                        insymbol0.Translate(lrgap * i, -udgap * j);
                    else
                        insymbol0.Translate(lrgap / 2 + lrgap * i, -udgap * j);
                    
                    m_fillsymbol.Add(insymbol0);
             

                }

            }
            #endregion


            if (ptsymbol1 == null) return;

            #region second pointsymbol

            rectbound.Left += lrgap / 2;

            PointSymbol startsymbol1 = ptsymbol1.Clone();
            startsymbol1.Translate((float)rectbound.Left, (float)rectbound.Top);

            nrow = (int)(rectbound.Height / udgap) + 1;
            ncol = (int)(rectbound.Width / lrgap) + 1;
            nncol = (int)((rectbound.Width + lrgap / 2) / lrgap) + 1;

            bodd = true;
            for (int j = 0; j < nrow; j++, bodd = !bodd)
            {

                int nnncol = bodd ? ncol : nncol;

                for (int i = 0; i < nnncol; i++)
                {
                    GeoPoint centerpt = new GeoPoint(bodd ? (rectbound.Left + lrgap * i) : (rectbound.Left + lrgap * i - lrgap / 2),
                        rectbound.Top - udgap * j);

                    //center
                    if (!PtInRegion(InteriorRings, ExteriorRing, centerpt))
                    {
                        continue;
                    }

                    //
                    //check four vertices
                    //
                    double top = centerpt.Y + ptsymbol0.Bound.Top - eps;
                    double left = centerpt.X - ptsymbol0.Bound.Left + eps;
                    double bottom = centerpt.Y - ptsymbol0.Bound.Bottom + eps;
                    double right = centerpt.X + ptsymbol0.Bound.Right - eps;

                    if (bodd)
                    {
                        if (!PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(left, top)) || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(left, bottom))
                            || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(right, top)) || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(right, bottom)))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(left - lrgap / 2, top)) || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(left - lrgap / 2, bottom))
                            || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(right - lrgap / 2, top)) || !PtInRegion(InteriorRings, ExteriorRing, new GeoPoint(right - lrgap / 2, bottom)))
                        {
                            continue;
                        }
                    }

                    PointSymbol insymbol1 = startsymbol1.Clone();
                    if (bodd)
                        insymbol1.Translate(lrgap * i, -udgap * j);
                    else
                        insymbol1.Translate(-lrgap / 2 + lrgap * i, -udgap * j);

                    m_fillsymbol.Add(insymbol1);


                }

            }
            #endregion

        }


        public bool PtInRegion(List<GeoLinearRing> InteriorRings, GeoLinearRing ExteriorRing, GeoPoint testpt)
        {
            bool bin = false;
            if (G_PtInPolygon(ExteriorRing.Vertices, ExteriorRing.Bound, testpt) != 2 ? true : false)
            {
                bin = true;
                for (int k = 0; k < InteriorRings.Count; k++)
                {
                    if (G_PtInPolygon(InteriorRings[k].Vertices, InteriorRings[k].Bound, testpt) != 2 ? true : false)
                    {
                        bin = false;
                        break;
                    }
                }
            }
            return bin;
        }

        //
        //射线法判断点q与多边形polygon的位置关系，要求polygon为简单多边形，顶点逆时针排列 
        //如果点在多边形内：   返回0 
        //如果点在多边形边上： 返回1 
        //如果点在多边形外：    返回2 
        //
        public int G_PtInPolygon(List<GeoPoint> plg, GeoBound bound, GeoPoint ptTest)

        {

            if (ptTest.X < bound.Left || ptTest.X > bound.Right || ptTest.Y > bound.Top || ptTest.Y < bound.Bottom)
                return 0;

            GeoPoint pt1 = ptTest.Clone() as GeoPoint;
            GeoPoint pt2 = ptTest.Clone() as GeoPoint;
            pt2.X = bound.Right + 50;
 
            int c = 0;
            bool bintersect_a, bonline1, bonline2, bonline3;
            double r1, r2;

            int n = plg.Count;
            for (int i = 0; i < plg.Count; i++)
            {

                if (online(plg[i], plg[(i + 1) % n], ptTest))
                    return 1; // 如果点在边上，返回1 
                if ((bintersect_a = intersect_A(pt1, pt2, plg[i], plg[(i + 1) % n])) || // 相交且不在端点 
                ((bonline1 = online(pt1, pt2, plg[(i + 1) % n])) && // 第二个端点在射线上 
                ((!(bonline2 = online(pt1, pt2, plg[(i + 2) % n]))) && /* 前一个端点和后一个端点在射线两侧 */
                ((r1 = multiply(plg[i % n], plg[(i + 1) % n], pt1) * multiply(plg[(i + 1) % n], plg[(i + 2) % n], pt1)) > 0) ||
                (bonline3 = online(pt1, pt2, plg[(i + 2) % n])) &&     /* 下一条边是水平线，前一个端点和后一个端点在射线两侧  */
                    ((r2 = multiply(plg[i], plg[(i + 2) % n], pt1) * multiply(plg[(i + 2) % n],
                plg[(i + 3) % n], pt1)) > 0)  ))) 
                    c++;
            }
            if (c % 2 == 1)
                return 0;
            else
                return 2;
        }



        //  (线段u和v相交)&&(交点不是双方的端点) 时返回true    
        public bool intersect_A(GeoPoint u0, GeoPoint u1, GeoPoint v0, GeoPoint v1)
        {
            return ((intersect(u0, u1, v0, v1)) &&
                    (!online(u0, u1, v0)) &&
                    (!online(u0, u1, v1)) &&
                    (!online(v0, v1, u0)) &&
                    (!online(v0, v1, u1)));
        }

        public bool intersect( GeoPoint u_a, GeoPoint u_b, GeoPoint v_a, GeoPoint v_b)
        {
            return ((Math.Max(u_a.X, u_b.X) >= Math.Min(v_a.X, v_b.X)) &&
                     (Math.Max(v_a.X, v_b.X) >= Math.Min(u_a.X, u_b.X)) &&
                     (Math.Max(u_a.Y, u_b.Y) >= Math.Min(v_a.Y, v_b.Y)) &&
                     (Math.Max(v_a.Y, v_b.Y) >= Math.Min(u_a.Y, u_b.Y)) &&  //以两线段为对角线的矩形是否相交
                     (multiply(v_a, u_b, u_a) * multiply(u_b, v_b, u_a) >= 0) &&
                     (multiply(u_a, v_b, v_a) * multiply(v_b, u_b, v_a) >= 0));       //是否跨立
        }

        public bool online(GeoPoint s, GeoPoint e, GeoPoint p)
        {
            return ((multiply(e, p, s) == 0) && (((p.X- s.X) * (p.X - e.X) <= 0) && ((p.Y - s.Y) * (p.Y - e.Y) <= 0)));
        }

        public double multiply(GeoPoint sp, GeoPoint ep, GeoPoint op)
        {
            return ((sp.X - op.X) * (ep.Y - op.Y) - (ep.X - op.X) * (sp.Y - op.Y));
        } 

        public int is_same(GeoPoint l_start, GeoPoint l_end, /* line l */GeoPoint p, GeoPoint q)
        {
            double dx = l_end.X - l_start.X;
            double dy = l_end.Y - l_start.Y;

            double dx1 = p.X - l_start.X;
            double dy1 = p.Y - l_start.Y;

            double dx2 = q.X - l_end.X;
            double dy2 = q.Y - l_end.Y;

            return ((dx * dy1 - dy * dx1) * (dx * dy2 - dy * dx2) > 0 ? 1 : 0);
        }

        public bool is_intersect(GeoPoint p1, GeoPoint p2, GeoPoint p3, GeoPoint p4)
        {

            return System.Convert.ToBoolean(((CCW(p1, p2, p3) * CCW(p1, p2, p4)) <= 0) && ((CCW(p3, p4, p1) * CCW(p3, p4, p2) <= 0)));

        }

        public int CCW(GeoPoint p0, GeoPoint p1, GeoPoint p2)
        {

            double dx1, dx2;
            double dy1, dy2;

            dx1 = p1.X - p0.X; dx2 = p2.X - p0.X;
            dy1 = p1.Y - p0.Y; dy2 = p2.Y - p0.Y;

            /* This is basically a slope comparison: we don't do divisions because

             * of divide by zero possibilities with pure horizontal and pure
             * vertical lines.
             */

            return ((dx1 * dy2 > dy1 * dx2) ? 1 : -1);

        }




    }
}