using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Windows.Forms;

namespace GIS.Toplogical
{
        //    //public string tpEqual;///////相等
        //    //public string tpDisjoint;////相离
        //    //public string tpInside; ///在内部
        //    //public string tpTouch;////相接
        //    //public string tpContains;////包含
        //    //public string tpCoveredBy; ///被覆盖
        //    //public string tpCovers;///覆盖
        //    //public string tpContains;///
        //    //public string tpIntersect;///相交
        //    //public string tpOn;///
        //    //public string tpUnknow///; 未知
        
        public enum TpRelateConstants 
        { 
            tpEqual,
            tpDisjoint,
            tpInside, 
            tpTouch, 
            tpContains, 
            tpCoveredBy,
            tpCovers, 
            tpIntersect, 
            tpOn,
            tpUnknow 
        };

        public static class TpPLPstruct
        {
            //容差
            public static double tol=0.0000001;
            
            /// <summary>
            /// 判断点pt与外包矩形的关系，TRUE在内部，false在外部
            /// </summary>
            public static bool TpPointinBound(GeoBound bound, GeoPoint pt)
            {
                return bound.IsPointIn(pt);
            }

            /// <summary>
            /// 判断点pt和直线pt1、pt2的关系，若pt与起端点pt1相等则返回1，若与终端点pt2相等则返回2，如果在直线内部返回3,在线外返回0
            /// </summary>
            public static int TpPointToLine(GeoPoint pt, GeoPoint pt1, GeoPoint pt2)
            {
                if (pt1.IsEqual(pt))
                {
                    return 1;
                }
                else if (pt2.IsEqual(pt))
                {
                    return 2;
                }
                else
                {
                    //GeoBound bound = new GeoBound(pt1, pt2);

                    GeoBound bound = new GeoBound(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y), Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y));
                    if (bound.IsPointIn(pt))
                    {
                        if (Math.Abs(pt1.X - pt2.X) < tol)//斜率无限大时，即竖直的线段
                        {
                            if ((Math.Abs(pt.X - pt2.X) < tol) && (Math.Abs(pt.X - pt1.X) < tol))
                            {
                                if ((pt.Y > Math.Min(pt1.Y, pt2.Y)) && (pt.Y < Math.Max(pt1.Y, pt2.Y)))
                                {
                                    return 3;
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                return 0;
                            }
                        }
                        else
                        {
                            double k = (pt1.Y - pt2.Y) / (pt1.X - pt2.X);
                            double k1 = (pt.Y - pt2.Y) / (pt.X - pt2.X);
                            if (Math.Abs(k-k1)<tol)
                            {
                                return 3;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            /// <summary>
            /// 计算线段pt1pt2，pt3pt4的关系，相离返回0，
            ///// 交于一条线段的端点   	1；
            //交于两线段的公共端点		2；
            //两线段相等		3；
            //交于线段内部		4；
            //部分重叠      5；
            public static int TpIntersectTwoLine(GeoPoint pt1, GeoPoint pt2, GeoPoint pt3, GeoPoint pt4)
            {
                GeoBound bound1 = new GeoBound(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y), Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y));
                GeoBound bound2 = new GeoBound(Math.Min(pt3.X, pt4.X), Math.Min(pt3.Y, pt4.Y), Math.Max(pt3.X, pt4.X), Math.Max(pt3.Y, pt4.Y));

                if (bound1.IsIntersectWith(bound2))
                {

                    if ((bound1.Left == bound2.Left) && (bound1.Right == bound2.Right) && (bound1.Top == bound2.Top) && (bound1.Bottom == bound2.Bottom))
                    {
                        return 3;
                    }
                    else
                    {
                            int a = 0;
                            int b = 0;
                            int c = 0;
                            int d = 0;
                            a += TpPointToLine(pt1, pt3, pt4);
                            b += TpPointToLine(pt2, pt3, pt4);
                            if(a==1||a==2)
                            {
                                if (b == 1 || b == 2)
                                {
                                    return 3;
                                }
                                else if (b == 3)
                                {
                                    return 5;
                                }
                                else if (b == 0)
                                {
                                    c = TpPointToLine(pt3, pt1, pt2);
                                    d = TpPointToLine(pt4, pt1, pt2);
                                    if ((c + d) > 0 && (c + d) % 3 == 0)
                                    {
                                        return 5;
                                    }
                                    else
                                    {
                                        return 2;
                                    }


                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else if (a == 3)
                            {
                                if (b == 1 || b == 2)
                                {
                                    return 5;
                                }
                                else if (b == 3)
                                {
                                    return 5;
                                }
                                else if (b == 0)
                                {
                                    c = TpPointToLine(pt3, pt1, pt2);
                                    d = TpPointToLine(pt4, pt1, pt2);
                                    if ((c + d) > 0 && (c + d) % 3 == 0)
                                    {
                                        return 5;
                                    }
                                    else
                                    {
                                        return 1;
                                    }

                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else 
                            {
                                c = TpPointToLine(pt3, pt1, pt2);
                                d = TpPointToLine(pt4, pt1, pt2);
                                if (c + d ==3 )
                                {
                                    if (b == 1 || b == 2)
                                    {
                                        return 2;
                                    }
                                    else if (b == 3)
                                    {
                                        return 5;
                                    }
                                    else if (b == 0)
                                    {
                                        return 1;
                                    }
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else if (c + d == 6)
                                {
                                    return 5;
                                }
                                else
                                {
                                    if((Math.Abs(pt1.X - pt2.X)<tol)&&(Math.Abs(pt3.X - pt4.X)<tol))
                                    {
                                        return 0;
                                    }
                                    else if((Math.Abs(pt1.X - pt2.X)<tol)&&(Math.Abs(pt3.X - pt4.X)>tol))
                                    {
                                        double k=0;
                                        double bl=0;
                                        k=(pt3.Y - pt4.Y) / (pt3.X - pt4.X);
                                        bl = pt3.Y - k * pt3.Y;
                                        double y=0;
                                        y=k*pt1.X+bl;

                                        if((y>Math.Min(pt1.Y,pt2.Y))&&(y<Math.Max(pt1.Y,pt2.Y)))
                                        {
                                            return 4;
                                        }
                                        else
                                        {
                                            return 0;
                                        }


                                    }
                                    else if((Math.Abs(pt1.X - pt2.X)>tol)&&(Math.Abs(pt3.X - pt4.X)<tol))
                                    {
                                        double k=0;
                                        double bl=0;
                                        k=(pt1.Y - pt2.Y) / (pt1.X - pt2.X);
                                        bl = pt1.Y - k * pt1.X;
                                        double y=0;
                                        y=k*pt3.X+bl;

                                        if((y>Math.Min(pt3.Y,pt4.Y))&&(y<Math.Max(pt3.Y,pt4.Y)))
                                        {
                                            return 4;
                                        }
                                        else
                                        {
                                            return 0;
                                        }
                                    }
                                    else
                                    {
                                        double k1=0;
                                        double k2=0;
                                        k1 = (pt1.Y - pt2.Y) / (pt1.X - pt2.X);
                                        k2 = (pt3.Y - pt4.Y) / (pt3.X - pt4.X);
                                        double b1=0;
                                        double b2=0;
                                        b1 = pt1.Y - k1 * pt1.X;
                                        b2 = pt3.Y - k2 * pt3.X;
                                        if(Math.Abs(k2-k1)<=tol)
                                        {
                                            return 0;
                                        }
                                        else
                                        {
                                            double x=(b1-b2)/(k2-k1);
                                            double y = k1 * x + b1;
                                            double[] ptx = new double[4] { pt1.X, pt2.X, pt3.X, pt4.X };
                                            double[] pty = new double[4] { pt1.Y, pt2.Y, pt3.Y, pt4.Y };

                                            for (int i = 0; i < 4; i++)
                                            {
                                                for (int j = i + 1; j < 4; j++)
                                                {
                                                    double t;
                                                    if (ptx[i] > ptx[j])
                                                    {
                                                        t = ptx[i];
                                                        ptx[i] = ptx[j];
                                                        ptx[j] = t;
                                                    }
                                                    if (pty[i] > pty[j])
                                                    {
                                                        t = pty[i];
                                                        pty[i] = pty[j];
                                                        pty[j] = t;
                                                    }
                                                }
                                            }


                                            
                                                if (((ptx[1] < x) && (x < ptx[2])) && ((pty[1] < y) && (y < pty[2])))
                                                {
                                                    return 4;
                                                    ///交点是x，y
                                                }
                                                else
                                                {
                                                    return 0;
                                                    ///不相交
                                                }
                                            

                                        }
                                            


                                    }
                                    
                                }

                                
                            }
                            
                        

                    }
                }
                else
                {
                    return 0;
                }
                
                
                
                  
            }

            /// <summary>
            /// pt1pt2被pt3pt4分割，point1，point2返回分割点，rt返回情况
            /// 被分割为两段，即只有一个分割点   	rt=1，point1=point2=该点；
            //被分割成三段，既有两个分割点	rt=2，point1，point2反回不同的分割点；
            //两线段相等,或有只有公共端点		rt=0,point1，point2不做改变；
            //相离   rt=0，point1，point2不变
            /// </summary>
            public static int TpSplitTwoLine(GeoPoint pt1, GeoPoint pt2, GeoPoint pt3, GeoPoint pt4, GeoPoint point1, GeoPoint point2)
            {

                GeoBound bound1 = new GeoBound(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y), Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y));
                GeoBound bound2 = new GeoBound(Math.Min(pt3.X, pt4.X), Math.Min(pt3.Y, pt4.Y), Math.Max(pt3.X, pt4.X), Math.Max(pt3.Y, pt4.Y));
                ///
                if (bound1.IsIntersectWith(bound2))
                {
                             
                    int a = 0;
                    int b = 0;
                    a = TpPLPstruct.TpPointToLine(pt3,pt1,pt2);
                    b = TpPLPstruct.TpPointToLine(pt4,pt1,pt2);
                    if ((a == 3) && (b == 3))
                    {
                        
                        if ((pt1.X - pt2.X) * (pt3.X - pt4.X) > 0)
                        {
                            point1.X=pt3.X;
                            point1.Y = pt3.Y;
                            point2.Y = pt4.Y;
                            point2.X = pt4.X;
                        }
                        else if ((pt1.X - pt2.X) * (pt3.X - pt4.X) < 0)
                        {
                            point1.X = pt4.X;
                            point1.Y = pt4.Y;
                            point2.Y = pt3.Y;
                            point2.X = pt3.X;
                        }
                        else 
                        {
                            if ((pt1.Y - pt2.Y) * (pt3.Y - pt4.Y) > 0)
                            {
                                point1.X = pt3.X;
                                point1 .Y= pt3.Y;
                                point2 .Y= pt4.Y;
                                point2 .X= pt4.X;
                            }
                            else
                            {
                                point1.X = pt4.X;
                                point1.Y = pt4.Y;
                                point2.Y = pt3.Y;
                                point2.X = pt3.X;
                            }
                        }
                        return 2;
                    }
                    else if ((a == 3) && (b != 3))
                    {

                        point1.X = pt3.X;
                        point1.Y = pt3.Y;
                        point2.Y = pt3.Y;
                        point2.X = pt3.X;
                        return 1;
                    }
                    else if ((b == 3) && (a != 3))
                    {
                        point1.X = pt4.X;
                        point1.Y = pt4.Y;
                        point2.X = pt4.X;
                        point2.Y = pt4.Y;
                        return 1;
                    }

                    else
                    {

                       if((Math.Abs(pt1.X - pt2.X)<tol)&&(Math.Abs(pt3.X - pt4.X)<tol))
                        {
                            point1.SetXY(0,0);
                            point2.SetXY(0,0);
                            return 0;
                        }
                        else if((Math.Abs(pt1.X - pt2.X)<tol)&&(Math.Abs(pt3.X - pt4.X)>tol))
                        {
                            double k=0;
                            double bl=0;
                            k=(pt3.Y - pt4.Y) / (pt3.X - pt4.X);
                            bl = pt3.Y - k * pt3.Y;
                            double y=0;
                            y=k*pt1.X+bl;

                            if((y>Math.Min(pt1.Y,pt2.Y))&&(y<Math.Max(pt1.Y,pt2.Y)))
                            {
                                point1.SetXY(pt1.X, y);
                                point2.SetXY(pt1.X, y);
                                return 1;
                            }
                            else
                            {
                                point1.SetXY(0, 0);
                                point2.SetXY(0, 0);
                                return 0;
                            }


                                    }
                       else if ((Math.Abs(pt1.X - pt2.X) > tol) && (Math.Abs(pt3.X - pt4.X) < tol))
                       {
                           double k = 0;
                           double bl = 0;
                           k = (pt1.Y - pt2.Y) / (pt1.X - pt2.X);
                           bl = pt1.Y - k * pt1.X;
                           double y = 0;
                           y = k * pt3.X + bl;

                           if ((y > Math.Min(pt3.Y, pt4.Y)) && (y < Math.Max(pt3.Y, pt4.Y)))
                           {
                               point1.SetXY(pt3.X, y);
                               point2.SetXY(pt3.X,y);
                               return 1;
                           }
                           else
                           {
                               point1.SetXY(0, 0);
                               point2.SetXY(0, 0);
                               return 0;
                           }
                       }
                       else
                       {
                           double k1 = 0;
                           double k2 = 0;
                           k1 = (pt1.Y - pt2.Y) / (pt1.X - pt2.X);
                           k2 = (pt3.Y - pt4.Y) / (pt3.X - pt4.X);
                           double b1 = 0;
                           double b2 = 0;
                           b1 = pt1.Y - k1 * pt1.X;
                           b2 = pt3.Y - k2 * pt3.X;
                           if (Math.Abs(k2 - k1) <= tol)
                           {
                               point1.SetXY(0, 0);
                               point2.SetXY(0, 0);
                               return 0;
                           }
                           else
                           {
                               double x = (b1 - b2) / (k2 - k1);
                               double y = k1 * x + b1;
                               double[] ptx = new double[4] { pt1.X, pt2.X, pt3.X, pt4.X };
                               double[] pty = new double[4] { pt1.Y, pt2.Y, pt3.Y, pt4.Y };

                               for (int i = 0; i < 4; i++)
                               {
                                   for (int j = i + 1; j < 4; j++)
                                   {
                                       double t;
                                       if (ptx[i] > ptx[j])
                                       {
                                           t = ptx[i];
                                           ptx[i] = ptx[j];
                                           ptx[j] = t;
                                       }
                                       if (pty[i] > pty[j])
                                       {
                                           t = pty[i];
                                           pty[i] = pty[j];
                                           pty[j] = t;
                                       }
                                   }
                               }



                               if (((ptx[1] < x) && (x < ptx[2])) && ((pty[1] < y) && (y < pty[2])))
                               {
                                   point1.SetXY(x, y);
                                   point2.SetXY(x, y);
                                   return 1;
                                   ///交点是x，y
                               }
                               else
                               {
                                   point1.SetXY(0, 0);
                                   point2.SetXY(0, 0);
                                   return 0;
                                   ///不相交
                               }


                           }



                       }
                                    
                                
                    }
                }
                else
                {
                    point1.SetXY(0, 0);
                    point2.SetXY(0, 0);
                    return 0;
                    ///相离
                }


            }

            /// <summary>
            /// 点与线环的拓扑,返回0，在线环外部；返回1，在线环内部；返回2，在线环的边界上
            /// </summary>
            /// <returns></returns>
            public static int TpPointToLineRing(GeoPoint pt, GeoLinearRing line)
            {
                GeoBound bound = line.GetBoundingBox();

                if (TpPointinBound(bound, pt))
                {
                    int count = line.Vertices.Count;
                    int a = 0;
                    for (int i = 0; i < count - 1; i++)
                    {
                        a = TpPointToLine(pt, line.Vertices[i], line.Vertices[i + 1]);
                        if (a > 0)
                        {
                            break;
                        }

                    }

                    if (a == 1 || a == 2)
                    {
                        return 2;
                        ///点pt在线环的节点上
                    }
                    else if (a == 3)
                    {
                        return 2;
                        ///点pt在线环的边界上
                    }
                    else///判断在线环内部，还是外部
                    {
                        GeoPoint ppt = new GeoPoint(pt.X, 1.0E+100);
                        double ll = ppt.X;
                        double lp = ppt.Y;
                        int s = 0;
                        for (int i = 0; i < count - 1; i++)
                        {
                            int m = 0;
                            int n = 0;
                            m = TpPointToLine(line.Vertices[i], pt, ppt);
                            n = TpPointToLine(line.Vertices[i + 1], pt, ppt);
                            
                            if (m == 3)
                            {
                                if (line.Vertices[i].Y > line.Vertices[i + 1].Y)
                                {
                                    s++;
                                }
                            }
                            else if (n == 3)
                            {
                                if (line.Vertices[i].Y < line.Vertices[i + 1].Y)
                                {
                                    s++;
                                }
                            }
                            else
                            {
                                int k = TpIntersectTwoLine(line.Vertices[i], line.Vertices[i + 1], pt, ppt);
                                if (k == 4)
                                {
                                    s++;
                                }

                            }
                        }

                        if (s % 2 == 1)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }

                    }
                }
                else
                {
                    return 0;
                }
            }


            /// <summary>
            /// pl2打断pl1，pl3是返回改变后的pl1
            /// 
            public static void TpInsertSplitTwoLine(GeoLineString pl1, GeoLineString pl2, GeoLineString pl3)
            {
                int ps=0; 
                int pd=0;
                ps=pl1.Vertices.Count;
                pd=pl2.Vertices.Count;
                GeoPoint point1 = new GeoPoint();
                GeoPoint point2 = new GeoPoint();
                List<GeoPoint> m_point=new List<GeoPoint>();
                List<GeoPoint>m_insertpt=new List<GeoPoint>();

                for (int i = 0; i < ps-1; i++)
                {

                    m_point.Add(pl1.Vertices[i]);
                    for (int j = 0; j < pd-1; j++)
                    {
                        int a = 0;

                        a=TpSplitTwoLine(pl1.Vertices[i], pl1.Vertices[i + 1], pl2.Vertices[j], pl2.Vertices[j + 1],point1,point2);


                        if (a == 1)
                        {
                            //MessageBox.Show(Convert.ToString(point1.X)+"  "+Convert.ToString(point1.Y));
                            GeoPoint pt = new GeoPoint(point1.X,point1.Y);
                            if (!((pt.X - m_point[m_point.Count - 1].X) ==0 && (pt.Y - m_point[m_point.Count - 1].Y) ==0))
                            {
                                m_point.Add(pt);
                            }
                            
                            
                        }
                        if (a == 2)
                        {
                            GeoPoint pt1 = new GeoPoint(point1.X, point1.Y);
                            GeoPoint pt2 = new GeoPoint(point2.X, point2.Y);
                            if (!((pt1.X - m_point[m_point.Count - 1].X) < tol && (pt1.Y - m_point[m_point.Count - 1].Y) < tol))
                            {
                                m_point.Add(pt1);
                            }
                                                      
                            m_point.Add(pt2);
                        }


                    }
                }

                m_point.Add(pl1.EndPoint);
                ///将m_point赋值给pl3

                pl3.Vertices.Clear();

               for (int c = 0; c <m_point.Count; c++)
                {
                    GeoPoint pp = new GeoPoint(m_point[c].X, m_point[c].Y);
                    pl3.Vertices.Add(m_point[c]);
                }

            }

        }
}
