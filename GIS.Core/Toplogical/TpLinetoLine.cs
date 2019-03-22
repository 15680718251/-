using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Windows.Forms;

namespace GIS.Toplogical
{

    public static class TpLinetoLine
    {
        private static bool m_out = false;
        private static bool m_on = false;
        private static bool m_meet = false;

        private static int m_out_num = 0;
        private static int m_on_num = 0;
        private static int m_meet_num = 0;

        private static int GetMeetNum()
        {
            return m_meet_num;
        }

        private static bool IsOverLap()
        {
            return m_on && m_meet;
        }

        private static bool IsMeet()
        {
            return !m_on && m_meet;
        }

        private static bool IsOn()
        {
            return !m_out && m_on && !m_meet;
        }

        private static bool IsDisJoint()
        {
            return m_out && !m_on && !m_meet;
        }


        private static void Reset()
        {
            m_out = false;
            m_on = false;
            m_meet = false;

            m_out_num = 0;
            m_on_num = 0;
            m_meet_num = 0;
        }


        private static void AddOut(int num)
        {
            m_out = true;
            m_out_num += num;
            //return m_out_num;
        }

        private static void AddOn(int num)
        {
            m_on = true;
            m_on_num += num;
            //return m_on_num;
        }

        private static void AddMeet(int num)
        {
            m_meet = true;
            m_meet_num += num;
            //return m_meet_num;
        }


        /// <summary>
        /// 其功能为计算界址线pl1相对于界址线pl2，进行打断后的局部线段关系，
        //其过程为：求取pl1与pl2相交点，利用交点对线pl1进行打断（其过程为插入交点），再遍历求取pl1中的每段线段与pl2的关系
        /// </summary>
        /// <param name="pl1"></param>
        /// <param name="pl2"></param>
        private static void SetValueByPls(GeoLineString pl1, GeoLineString pl2)
        {
            
            GeoLineString pl3 = new GeoLineString();
            TpPLPstruct.TpInsertSplitTwoLine(pl1, pl2, pl3);

            TpRelateConstants tp1 = TpRelateConstants.tpUnknow;
            TpRelateConstants tp2 = TpRelateConstants.tpUnknow;
            TpRelateConstants tp = TpRelateConstants.tpUnknow;
            GeoPoint pt = new GeoPoint(pl3.Vertices[0].X, pl3.Vertices[0].Y);

            tp1 = TpPointtoLines.IsTpWith(pt, pl2, false);
            int sum = 0;
            sum = pl3.Vertices.Count;
            for (int i = 1; i < sum; i++, tp1 = tp2)
            {
                pt.SetXY(pl3.Vertices[i].X, pl3.Vertices[i].Y);
                tp2 = TpPointtoLines.IsTpWith(pt, pl2, false);

                if (tp1 == TpRelateConstants.tpDisjoint && tp2 == TpRelateConstants.tpDisjoint)
                {
                    AddOut(1);
                    continue;
                }
                else if ((tp1 == TpRelateConstants.tpDisjoint && (tp2 == TpRelateConstants.tpInside || tp2 == TpRelateConstants.tpTouch))
                    || (tp2 == TpRelateConstants.tpDisjoint && (tp1 == TpRelateConstants.tpInside || tp1 == TpRelateConstants.tpTouch)))
                {
                    AddMeet(1);
                    continue;
                }
                else if ((tp1 == TpRelateConstants.tpTouch || tp1 == TpRelateConstants.tpInside) && (tp2 == TpRelateConstants.tpTouch || tp2 == TpRelateConstants.tpInside))
                {
                    GeoPoint ptt = new GeoPoint((pl3.Vertices[i].X + pl3.Vertices[i - 1].X) / 2, (pl3.Vertices[i].Y + pl3.Vertices[i - 1].Y) / 2);
                    tp = TpPointtoLines.IsTpWith(ptt, pl2, false);
                    if (tp == TpRelateConstants.tpDisjoint)
                    {
                        AddMeet(1);
                    }
                    else if (tp == TpRelateConstants.tpInside || tp == TpRelateConstants.tpTouch)
                    {
                        AddOn(1);
                    }
                    continue;
                }


                //tp1 = tp2;
            }




        }


        /// <summary>
        /// 求两条单线的拓扑关系：tpEqual, tpDisjoint, tpInside, tpTouch, tpContains, tpCoveredBy, tpCovers, tpIntersect
        /// </summary>
        /// <param name="pl1"></param>
        /// <param name="pl2"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoLineString pl1, GeoLineString pl2, bool bReport)
        {
            int aa = 0;
            aa = pl1.Vertices.Count - 1;
            GeoPoint pt = new GeoPoint(pl1.Vertices[aa].X, pl1.Vertices[aa].Y);
            GeoPoint pt1 = new GeoPoint(pl1.Vertices[aa - 1].X, pl1.Vertices[aa - 1].Y);

            GeoBound bound1 = pl1.GetBoundingBox();
            GeoBound bound2 = pl2.GetBoundingBox();

            if (bound1.IsIntersectWith(bound2))
            {
                Reset();
                TpLinetoLine.SetValueByPls(pl1, pl2);
                bool IsDisJoint1 = IsDisJoint();
                bool IsOverLap1 = IsOverLap();
                bool IsMeet1 = IsMeet();
                bool IsOn1 = IsOn();
                int GetMeetNum1 = GetMeetNum();


                if (IsDisJoint1)//相离
                {
                    if (bReport)
                    {
                        MessageBox.Show("两条折线相离", "GIS");///两线相离
                    }

                    return TpRelateConstants.tpDisjoint;
                }
                if (IsMeet1)//相接或相交
                {//只有在两线的两个端点(首端点或是末端点)相接时，才认为两线是相接的
                    int up1 = pl1.Vertices.Count - 1;
                    int up2 = pl2.Vertices.Count - 1;

                    if (GetMeetNum1 == 1)
                    {
                        if (pl1.Vertices[0].IsEqual(pl2.Vertices[0]) || pl1.Vertices[0].IsEqual(pl2.Vertices[up2]))
                        {
                            if (bReport)
                            {
                                MessageBox.Show("两条线端点相接", "GIS");///"两线目标端点相接"
                            }
                            return TpRelateConstants.tpTouch;
                        }
                        else if (pl1.Vertices[up1].IsEqual(pl2.Vertices[0]) || pl1.Vertices[up1].IsEqual(pl2.Vertices[up2]))
                        {
                            if (bReport)
                            {
                                MessageBox.Show("两条线端点相接", "GIS");///两线目标端点相接
                            }

                            return TpRelateConstants.tpTouch;
                        }
                        else
                        {
                            if (bReport)
                            {
                                MessageBox.Show("两线目标相交", "GIS");///两线目标相交
                            }

                            return TpRelateConstants.tpIntersect;

                        }
                    }
                    else if (GetMeetNum1 == 2)
                    {
                        bool sNode = false, eNode = false;

                        if (pl1.Vertices[0].IsEqual(pl2.Vertices[0]) || pl1.Vertices[0].IsEqual(pl2.Vertices[up2]))
                        {
                            sNode = true;
                        }
                        if (pl1.Vertices[up1].IsEqual(pl2.Vertices[0]) || pl1.Vertices[up1].IsEqual(pl2.Vertices[up2]))
                        {
                            eNode = true;
                        }
                        if (sNode && eNode)
                        {
                            if (bReport)
                            {
                                MessageBox.Show("两线目标端点相接，且二者的首尾端点都相接", "GIS");///"两线目标端点相接，且二者的首尾端点都相接"
                            }
                            return TpRelateConstants.tpTouch;
                        }
                        else
                        {
                            if (bReport)
                            {
                                MessageBox.Show("两线目标相交", "GIS");///两线目标相交
                            }
                            return TpRelateConstants.tpIntersect;
                        }
                    }

                    else
                    {
                        if (bReport)
                        {
                            MessageBox.Show("两线目标相交", "GIS");///"两线目标相交"
                        }
                        return TpRelateConstants.tpIntersect;
                    }
                }

                Reset();
                TpLinetoLine.SetValueByPls(pl2, pl1);
                bool IsDisJoint2 = IsDisJoint();
                bool IsOverLap2 = IsOverLap();
                bool IsMeet2 = IsMeet();
                bool IsOn2 = IsOn();


                if (IsOn1 && IsOn2)  //相等
                {
                    if (bReport)
                    {
                        MessageBox.Show("两线目标相等", "GIS");///"两线目标相等"
                    }
                    return TpRelateConstants.tpEqual;
                }

                if (IsOn1 && IsOverLap2)  //被包含或覆盖
                {
                    int up1 = pl1.NumPoints - 1;
                    int up2 = pl2.NumPoints - 1;
                    if (pl1.Vertices[0].IsEqual(pl2.Vertices[0]) || pl1.Vertices[0].IsEqual(pl2.Vertices[up2]))
                    {
                        if (bReport)
                        {
                            MessageBox.Show("按照选择的先后顺序，线目标1被线目标2覆盖", "GIS");///"按照选择的先后顺序，线目标1被线目标2覆盖"
                        }
                        return TpRelateConstants.tpCoveredBy;
                    }

                    else if (pl1.Vertices[up1].IsEqual(pl2.Vertices[0]) || pl1.Vertices[up1].IsEqual(pl2.Vertices[up2]))
                    {
                        if (bReport)
                        {
                            MessageBox.Show("按照选择的先后顺序，线目标1被线目标2覆盖", "GIS");///"按照选择的先后顺序，线目标1被线目标2覆盖"
                        }
                        return TpRelateConstants.tpCoveredBy;
                    }

                    else
                    {
                        if (bReport)
                        {
                            MessageBox.Show("按照选择的先后顺序，线目标1被线目标2包含", "GIS");///"按照选择的先后顺序，线目标1被线目标2包含"
                        }
                        return TpRelateConstants.tpInside;
                        

                    }
                }
                if (IsOn2 && IsOverLap1)  //包含或覆盖
                {
                    int up1 = pl1.NumPoints - 1;
                    int up2 = pl2.NumPoints - 1;
                    if (pl2.Vertices[0].IsEqual(pl1.Vertices[0]) || pl2.Vertices[0].IsEqual(pl1.Vertices[up1]))
                    {
                        if (bReport)
                        {
                            MessageBox.Show("按照选择的先后顺序，线目标1覆盖线目标2", "GIS");///"按照选择的先后顺序，线目标1覆盖线目标2"
                        }
                        return TpRelateConstants.tpCovers;
                    }
                    else if (pl2.Vertices[up2].IsEqual(pl1.Vertices[0]) || pl2.Vertices[up2].IsEqual(pl1.Vertices[up1]))
                    {
                        if (bReport)
                        {
                            MessageBox.Show("按照选择的先后顺序，线目标1覆盖线目标2", "GIS");/// "按照选择的先后顺序，线目标1覆盖线目标2"
                        }
                        return TpRelateConstants.tpCovers;
                    }
                    else
                    {
                        if (bReport)
                        {
                            MessageBox.Show("按照选择的先后顺序，线目标1包含线目标2", "GIS");///"按照选择的先后顺序，线目标1包含线目标2"
                        }
                        return TpRelateConstants.tpContains;
                    }
                    
                }
                
                if (IsOverLap1)
                {
                    //相交，这里还存在着部分重叠的关系，以后若要细分时，可作扩充
                    if (bReport)
                    {
                        MessageBox.Show("两线目标相交", "GIS");///"两线目标相交
                    }
                    return TpRelateConstants.tpIntersect;
                }


                if (bReport)
                {
                    MessageBox.Show("两线目标相离", "GIS");
                }
                return TpRelateConstants.tpDisjoint;
                
            }
            else
            {
                if (bReport)
                {
                    MessageBox.Show("两线目标相离", "GIS");///"两线目标相离"
                }
                return TpRelateConstants.tpDisjoint;
            }


        }


        /// <summary>
        /// 求多线集、单线的拓扑关系：tpEqual（）, tpDisjoint, tpInside, tpTouch, tpContains, tpCoveredBy, tpCovers, tpIntersect
        /// </summary>
        /// <param name="pls"></param>
        /// <param name="pl"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoMultiLineString pls, GeoLinearRing pl, bool bReport)
        {
            return TpRelateConstants.tpUnknow;
        }


        /// <summary>
        /// 求单线、多线集的拓扑关系： tpEqual（）, tpDisjoint, tpInside, tpTouch, tpContains, tpCoveredBy, tpCovers, tpIntersect
        /// </summary>
        /// <param name="pls"></param>
        /// <param name="pl"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoLinearRing pl, GeoMultiLineString pls, bool bReport)
        {
            return TpRelateConstants.tpUnknow;
        }


        /// <summary>
        /// 求多线集、多线集的拓扑关系： tpEqual, tpDisjoint, tpInside, tpTouch, tpContains, tpCoveredBy, tpCovers, tpIntersect
        /// </summary>
        /// <param name="pls"></param>
        /// <param name="pl"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoMultiLineString pls1, GeoMultiLineString pls2, bool bReport)
        {
            return TpRelateConstants.tpUnknow;
        }


    }
}
