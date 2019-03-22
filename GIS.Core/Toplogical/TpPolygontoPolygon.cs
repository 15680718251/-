using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Windows.Forms;

namespace GIS.Toplogical
{
    public static class TpPolygontoPolygon
    {

	    private static bool m_pout;
	    private static bool m_pon;
	    private static bool m_pin;
	    private static bool m_poutmeet;
	    private static bool m_pinmeet;

	    private static int  m_pout_num;
	    private static int  m_pon_num;
	    private static int  m_pin_num;
	    private static int  m_pinmeet_num;
	    private static int  m_poutmeet_num;

        private static int GetOnNum()
        {
            return m_pon_num;
        }

        private static void ReSet()
        {
            m_pout=false;
            m_pon=false;
            m_pin=false;
            m_pinmeet=false;
            m_poutmeet=false;	

            m_pout_num=0;
            m_pon_num=0;
            m_pin_num=0;
            m_pinmeet_num=0;
            m_poutmeet_num=0;
        }

        private static void AddPOut(int num)
        {
            m_pout=true;
            m_pout_num+=num;
            //return m_pout_num;
        }

        private static void AddPOn(int num)
        {
            m_pon=true;
            m_pon_num+=num;
            //return m_pon_num;
        }

        private static void AddPIn(int num)
        {
            m_pin=true;
            m_pin_num+=num;
            //return m_pin_num;
        }

        private static void AddPInMeet(int num)
        {
            m_pinmeet=true;
            m_pinmeet_num+=num;
            //return m_pinmeet_num;
        }

        private static void AddPOutMeet(int num)
        {
            m_poutmeet=true;
            m_poutmeet_num+=num;
            //return m_poutmeet_num;
        }

        private static bool IsInside()  //INSIDE
        {
            if((!m_pout) && (!m_pon) && (!m_pinmeet) && (!m_poutmeet) && m_pin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsTouch()
        {
            if (m_poutmeet && !m_pinmeet && !m_pin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsIntersect()
        {
            if(m_pinmeet && m_poutmeet)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsCoveredBy()
        {
            if((!m_pout) && (!m_poutmeet) && (m_pinmeet))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsDisjoint() //DISJOINT
        {
            if((m_pout) && (!m_pon) && (!m_pinmeet) && (!m_poutmeet) && (!m_pin))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsEqual()
        {
            if((!m_pout) && (m_pon) && (!m_pinmeet) && (!m_poutmeet) && (!m_pin))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void SetValueByPlgs(GeoPolygon pg1,GeoPolygon pg2 )
        {
            //CGeoLineRing pl1(*pg1.GetPart(0));
            //CGeoLineRing pl2(*pg2.GetPart(0));
            GeoLineString pl1=new GeoLineString(pg1.ExteriorRing.Vertices);
            GeoLineString pl2=new GeoLineString(pg2.ExteriorRing.Vertices);
            GeoLineString pl3=new GeoLineString();

            TpPLPstruct.TpInsertSplitTwoLine(pl1,pl2,pl3);
	        TpRelateConstants tp1,tp2,tp;
            GeoPoint pt=new GeoPoint(pl3.Vertices[0].X,pl3.Vertices[0].Y);;

            tp1=TpPointtoPolygon.IsTpWith(pt,pg2,false);

	        for(int c =1; c<pl3.Vertices.Count; c++, tp1=tp2)
	        {
		        pt.SetXY(pl3.Vertices[c].X,pl3.Vertices[c].Y);
		        tp2=TpPointtoPolygon.IsTpWith(pt,pg2,false);
		        if(tp1==TpRelateConstants.tpDisjoint && tp2==TpRelateConstants.tpDisjoint )
		        {
			        AddPOut(1);
			        continue;
		        }
		        else if((tp1==TpRelateConstants.tpDisjoint && tp2==TpRelateConstants.tpTouch ) || (tp2==TpRelateConstants.tpDisjoint && tp1==TpRelateConstants.tpTouch ))
		        {
			        AddPOutMeet(1);
			        continue;
		        }
		        else if(tp1==TpRelateConstants.tpTouch && tp2==TpRelateConstants.tpTouch)
		        {
			        GeoPoint midPt=new GeoPoint((pl3.Vertices[c-1].X+pl3.Vertices[c].X)/2,(pl3.Vertices[c-1].Y+pl3.Vertices[c].Y)/2);
                    tp = TpPointtoPolygon.IsTpWith(midPt,pg2,false);

			        if(tp==TpRelateConstants.tpDisjoint)
			        {
				        AddPOutMeet(1);
			        }
			        else if(tp==TpRelateConstants.tpTouch)
			        {
				        AddPOn(1);
			        }
			        else if(tp==TpRelateConstants.tpInside)
			        {
				        AddPInMeet(1);
			        }
			        continue;
		        }
		        else if((tp1==TpRelateConstants.tpTouch && tp2==TpRelateConstants.tpInside) || (tp2==TpRelateConstants.tpTouch && tp1==TpRelateConstants.tpInside))
		        {
			        AddPInMeet(1);
			        continue;
		        }
		        else if(tp1==TpRelateConstants.tpInside && tp2==TpRelateConstants.tpInside)
		        {
			        AddPIn(1);
			        continue;
		        }

	        }
        }

        /// <summary>
        /// 线环与线环的关系，返回tpEqual, tpDisjoint, tpInside, tpTouch, tpContains, tpCoveredBy, tpCovers, tpIntersect, tpUnknow
        /// </summary>
        public static TpRelateConstants IsTpWith(GeoLinearRing pr1,GeoLinearRing pr2)
        {
            GeoBound bound1 = pr1.GetBoundingBox();
            GeoBound bound2 = pr2.GetBoundingBox();

            if (bound1.IsIntersectWith(bound2))
            {

                GeoPolygon pg1 = new GeoPolygon(pr1);
                GeoPolygon pg2 = new GeoPolygon(pr2);

                TpPolygontoPolygon.ReSet();
                TpPolygontoPolygon.SetValueByPlgs(pg1,pg2);
                bool pout1 = m_pout;
                bool pon1 = m_pon;
                bool pin1 = m_pin;
                bool poutmeet1 = m_poutmeet;
                bool pinmeet1 = m_pinmeet;

                int pout_num1 = m_pout_num;
                int pon_num1 = m_pon_num;
                int pin_num1 = m_pin_num;
                int poutmeet_num1 = m_poutmeet_num;
                int pinmeet_num1 = m_pinmeet_num;


                bool IsIntersect1=TpPolygontoPolygon.IsIntersect();
	            bool IsTouch1=TpPolygontoPolygon.IsTouch();
	            bool IsDisjoint1=TpPolygontoPolygon.IsDisjoint();
	            bool IsInside1=TpPolygontoPolygon.IsInside();
	            bool IsEqual1=TpPolygontoPolygon.IsEqual();
	            bool IsCoveredBy1=TpPolygontoPolygon.IsCoveredBy();

	            int GetOnNum1=TpPolygontoPolygon.GetOnNum();


                
                
                //对于简单多边形，利用plg1的边界线与plg2的关系来做拓扑判定，
                //这样与plg2内部和边界交和差的情况，可以先确定plg1和plg2相交、被包含和被覆盖的拓扑性质
                if (IsIntersect1)
                {
                    return TpRelateConstants.tpIntersect;
                }
                else if (IsCoveredBy1)
                {
                    return TpRelateConstants.tpCoveredBy;
                }
                else if (IsInside1)
                {
                    return TpRelateConstants.tpInside;
                }
                else if (IsEqual1)
                {
                    return TpRelateConstants.tpEqual;
                }

                else
                {


                    TpPolygontoPolygon.ReSet();
                    TpPolygontoPolygon.SetValueByPlgs(pg2, pg1);
                    bool pout2 = m_pout;
                    bool pon2 = m_pon;
                    bool pin2 = m_pin;
                    bool poutmeet2 = m_poutmeet;
                    bool pinmeet2 = m_pinmeet;

                    int pout_num2 = m_pout_num;
                    int pon_num2 = m_pon_num;
                    int pin_num2 = m_pin_num;
                    int poutmeet_num2 = m_poutmeet_num;
                    int pinmeet_num2 = m_pinmeet_num;


                    bool IsIntersect2 = TpPolygontoPolygon.IsIntersect();
                    bool IsTouch2 = TpPolygontoPolygon.IsTouch();
                    bool IsDisjoint2 = TpPolygontoPolygon.IsDisjoint();
                    bool IsInside2 = TpPolygontoPolygon.IsInside();
                    bool IsEqual2 = TpPolygontoPolygon.IsEqual();
                    bool IsCoveredBy2 = TpPolygontoPolygon.IsCoveredBy();

                    int GetOnNum2 = TpPolygontoPolygon.GetOnNum();


                    if (IsTouch1)
                    {
                        if (IsCoveredBy2)
                        {
                            return TpRelateConstants.tpCovers;
                        }
                        else
                        {
                            return TpRelateConstants.tpTouch;
                        }
                    }
                    else if (IsDisjoint1)
                    {
                        if (IsInside2)
                        {
                            return TpRelateConstants.tpContains;
                        }
                        else
                        {
                            return TpRelateConstants.tpDisjoint;
                        }
                    }
                    else
                    {
                        return TpRelateConstants.tpUnknow;
                    }
                }
            }
            else
            {
                return TpRelateConstants.tpDisjoint;
            }
        }

        public static TpRelateConstants IsTpWith(GeoPolygon pg1, GeoPolygon pg2, bool bReport)
        {
            GeoBound bound1 = pg1.GetBoundingBox();
            GeoBound bound2 = pg2.GetBoundingBox();

            if (bound1.IsIntersectWith(bound2))
            {

                TpRelateConstants tp = TpPolygontoPolygon.IsTpWith(pg1.ExteriorRing, pg2.ExteriorRing);

                if (pg1.InteriorRings.Count == 0 && pg2.InteriorRings.Count == 0)
                {
                    if (tp == TpRelateConstants.tpIntersect)
                    {
                        if (bReport)
                             MessageBox.Show( "两面目标相交","GIS");

                        return TpRelateConstants.tpIntersect;
                    }
                    else if (tp == TpRelateConstants.tpCoveredBy)
                    {
                        if (bReport)
                             MessageBox.Show( "按照选择的先后顺序，面目标1被面目标2覆盖","GIS");

                        return TpRelateConstants.tpCoveredBy;
                    }
                    else if (tp == TpRelateConstants.tpInside)
                    {
                        if (bReport)
                             MessageBox.Show( "按照选择的先后顺序，面目标1被面目标2包含","GIS");

                        return TpRelateConstants.tpInside;
                    }
                    else if (tp == TpRelateConstants.tpEqual)
                    {
                        if (bReport)
                             MessageBox.Show( "两面目标相等","GIS");

                        return TpRelateConstants.tpEqual;
                    }
                    else if (tp == TpRelateConstants.tpCovers)
                    {
                        if (bReport)
                             MessageBox.Show( "按照选择的先后顺序，面目标1覆盖面目标2","GIS");

                        return TpRelateConstants.tpCovers;
                    }
                    else if (tp == TpRelateConstants.tpTouch)
                    {
                        if (bReport)
                             MessageBox.Show( "两面目标相接","GIS");

                        return TpRelateConstants.tpTouch;
                    }
                    else if (tp == TpRelateConstants.tpContains)
                    {
                        if (bReport)
                             MessageBox.Show( "按照选择的先后顺序，面目标1包含面目标2","GIS");

                        return TpRelateConstants.tpContains;
                    }
                    else if (tp == TpRelateConstants.tpDisjoint)
                    {
                        if (bReport)
                            MessageBox.Show("两面目标相离", "GIS");

                        return TpRelateConstants.tpDisjoint;
                    }
                    else
                    {
                        if (bReport)
                            MessageBox.Show("两面目标关系未知", "GIS");

                        return TpRelateConstants.tpUnknow;
                    }

                }
                else
                {
                    if (tp == TpRelateConstants.tpDisjoint)  //相离
                    {
                        if (bReport)
                            MessageBox.Show("两面目标外相离","GIS");

                        return TpRelateConstants.tpDisjoint;
                    }
                    else if (tp == TpRelateConstants.tpTouch)  //相接
                    {
                        if (bReport)
                            MessageBox.Show("两面目标外相接","GIS");

                        return TpRelateConstants.tpTouch;
                    }
                    else if (tp == TpRelateConstants.tpIntersect) //相交
                    {
                        if (bReport)
                            MessageBox.Show("两面目标相交","GIS");

                        return TpRelateConstants.tpIntersect;
                    }
                    else if (tp == TpRelateConstants.tpCovers)
                    {
                        if (pg1.InteriorRings.Count == 0 && pg2.InteriorRings.Count!=0)//不包含空洞与包含空洞
                        {
                            if (bReport)
                                MessageBox.Show("按照选择的先后顺序，面目标1覆盖面目标2", "GIS");

                            return TpRelateConstants.tpCovers;
                        }
                        else if (pg1.InteriorRings.Count != 0 && pg2.InteriorRings.Count == 0)//包含空洞与不包含空洞
                        {
                            for (int i = 0; i < pg1.InteriorRings.Count; i++)
                            {
                                TpRelateConstants tph = TpPolygontoPolygon.IsTpWith(pg1.InteriorRings[i],pg2.ExteriorRing);
                                if (tph ==TpRelateConstants.tpIntersect || tph == TpRelateConstants.tpInside || tph == TpRelateConstants.tpCoveredBy)
                                {
                                    if (bReport)
                                        MessageBox.Show("两面目标相交", "GIS");

                                    return TpRelateConstants.tpIntersect;
                                }
                            }

                            if (bReport)
                                MessageBox.Show("按照选择的先后顺序，面目标1覆盖面目标2", "GIS");

                            return TpRelateConstants.tpCovers;
                        }
                        else//两都包含空洞
                        {
                            for (int i = 0; i < pg1.InteriorRings.Count; i++)
                            {
                                TpRelateConstants tph = TpPolygontoPolygon.IsTpWith(pg1.InteriorRings[i],pg2.ExteriorRing);
                                if (tph == TpRelateConstants.tpIntersect || tph == TpRelateConstants.tpCoveredBy)
                                {
                                    if (bReport)
                                        MessageBox.Show("两面目标相交", "GIS");

                                    return TpRelateConstants.tpIntersect;
                                }
                                else if (tph == TpRelateConstants.tpInside)
                                {
                                    int i2 = 0;
                                    for (i2 = 0; i2 < pg2.InteriorRings.Count; i2++)
                                    {
                                        TpRelateConstants tphh = TpPolygontoPolygon.IsTpWith(pg1.InteriorRings[i],pg2.InteriorRings[i2]);
                                        if (tphh == TpRelateConstants.tpEqual || tphh == TpRelateConstants.tpInside || tphh == TpRelateConstants.tpCoveredBy)
                                        {
                                            break;
                                        }
                                        if (tphh == TpRelateConstants.tpIntersect || tphh == TpRelateConstants.tpCovers || tphh == TpRelateConstants.tpContains || tphh == TpRelateConstants.tpTouch)
                                        {
                                            if (bReport)
                                                MessageBox.Show("两面目标相交", "GIS");

                                            return TpRelateConstants.tpIntersect;
                                        }
                                    }
                                    if (i2 == pg2.InteriorRings.Count)
                                    {//说明tphh在上面的循环中始终是tphh==tpDisjoint
                                        if (bReport)
                                            MessageBox.Show("两面目标相交", "GIS");

                                        return TpRelateConstants.tpIntersect;
                                    }
                                }
                            }
                            
                            if (bReport)
                                MessageBox.Show("按照选择的先后顺序，面目标1覆盖面目标2", "GIS");

                            return TpRelateConstants.tpCovers;
                        }
                    }
                    else if (tp == TpRelateConstants.tpContains)  //包含
                    {
                        if (pg1.InteriorRings.Count==0&&pg2.InteriorRings.Count!=0)//不包含空洞与包含空洞
                        {
                            if (bReport)
                                MessageBox.Show("按照选择的先后顺序，面目标1包含面目标2", "GIS");

                            return TpRelateConstants.tpContains;
                        }
                        else if (pg1.InteriorRings.Count!=0&&pg2.InteriorRings.Count==0)//包含空洞与不包含空洞
                        {
                            bool touch = false;
                            for (int i = 0; i < pg1.InteriorRings.Count; i++)
                            {
                                TpRelateConstants tph = TpPolygontoPolygon.IsTpWith(pg1.InteriorRings[i],pg2.ExteriorRing);
                                if (tph == TpRelateConstants.tpIntersect || tph == TpRelateConstants.tpInside || tph == TpRelateConstants.tpCoveredBy)
                                {
                                    if (bReport)
                                        MessageBox.Show("两面目标相交", "GIS");

                                    return TpRelateConstants.tpIntersect;
                                }
                                else if (tph == TpRelateConstants.tpCovers || tph == TpRelateConstants.tpEqual)
                                {
                                    if (bReport)
                                        MessageBox.Show("按照选择的先后顺序，面目标2相接于面目标1的内边界", "GIS");

                                    return TpRelateConstants.tpTouch;
                                }
                                else if (tph == TpRelateConstants.tpTouch)
                                {
                                    touch = true;
                                }
                                else if (tph == TpRelateConstants.tpContains)
                                {
                                    if (bReport)
                                        MessageBox.Show("按照选择的先后顺序，面目标2内相离于面目标1", "GIS");

                                    return TpRelateConstants.tpDisjoint;
                                }
                            }
                            if (touch)
                            {
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1覆盖面目标2", "GIS");

                                return TpRelateConstants.tpCovers;
                            }
                            else
                            {
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1包含面目标2", "GIS");

                                return TpRelateConstants.tpContains;
                            }
                        }
                        else//都包含空洞
                        {
                            bool touch = false;
                            for (int i = 0; i < pg1.InteriorRings.Count; i++)
                            {
                                TpRelateConstants tph = TpPolygontoPolygon.IsTpWith(pg1.InteriorRings[i],pg2.ExteriorRing);
                                if (tph == TpRelateConstants.tpIntersect || tph == TpRelateConstants.tpCoveredBy)
                                {
                                    if (bReport)
                                        MessageBox.Show("两面目标相交", "GIS");

                                    return TpRelateConstants.tpIntersect;
                                }
                                else if (tph == TpRelateConstants.tpCovers || tph == TpRelateConstants.tpEqual)
                                {
                                    if (bReport)
                                        MessageBox.Show("按照选择的先后顺序，面目标2内相接于面目标1的内边界", "GIS");

                                    return TpRelateConstants.tpTouch;
                                }
                                else if (tph == TpRelateConstants.tpTouch)
                                {
                                    touch = true;
                                }
                                else if (tph == TpRelateConstants.tpContains)
                                {
                                    if (bReport)
                                        MessageBox.Show("按照选择的先后顺序，面目标2内相离于面目标1", "GIS");

                                    return TpRelateConstants.tpDisjoint;
                                }
                                else if (tph == TpRelateConstants.tpInside)
                                {
                                    int i2 = 0;
                                    for (i2 = 0; i2 < pg2.InteriorRings.Count; i2++)
                                    {
                                        TpRelateConstants tphh = TpPolygontoPolygon.IsTpWith(pg1.InteriorRings[i],pg2.InteriorRings[i2]);
                                        if (tphh == TpRelateConstants.tpEqual || tphh == TpRelateConstants.tpCoveredBy)
                                        {
                                            touch = true;
                                            break;
                                        }
                                        else if (tphh == TpRelateConstants.tpInside)
                                        {
                                            break;
                                        }
                                        else if (tphh == TpRelateConstants.tpIntersect || tphh == TpRelateConstants.tpCovers || tphh == TpRelateConstants.tpContains || tphh == TpRelateConstants.tpTouch)
                                        {
                                            if (bReport)
                                                MessageBox.Show("两面目标相交", "GIS");
                                            return TpRelateConstants.tpIntersect;
                                        }
                                    }

                                    if (i2 == pg2.InteriorRings.Count)
                                    {//说明tphh在上面的循环中始终是tphh==tpDisjoint
                                        if (bReport)
                                            MessageBox.Show("两面目标相交", "GIS");

                                        return TpRelateConstants.tpIntersect;
                                    }
                                }
                            }//end for

                            if (touch)
                            {
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1覆盖面目标2", "GIS");

                                return TpRelateConstants.tpCovers;
                            }
                            else
                            {
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1包含面目标2", "GIS");

                                return TpRelateConstants.tpContains;
                            }
                        }
                    }

                    else if (tp == TpRelateConstants.tpEqual)  //相等
                    {
                        if (pg1.InteriorRings.Count==0&&pg2.InteriorRings.Count!=0)//不包含空洞与包含空洞
                        {
                            if (bReport)
                                MessageBox.Show("按照选择的先后顺序，面目标1覆盖面目标2", "GIS");

                            return TpRelateConstants.tpCovers;
                        }
                        else if (pg1.InteriorRings.Count!=0&&pg2.InteriorRings.Count==0)//包含空洞与不包含空洞
                        {
                            if (bReport)
                                MessageBox.Show("按照选择的先后顺序，面目标1被面目标2覆盖", "GIS");

                            return TpRelateConstants.tpCoveredBy;
                        }
                        else//都包含空洞
                        {//这种情况些会出现相交、相等、被覆盖和覆盖四种拓扑性质

                            bool bEqual = false; 
                            bool bDisjoint = false;
                            bool bCoversOrContains = false; 
                            bool bCoveredByOrInside = false;
                            int nCountEqual = 0;
                            int nCountOut = 0;
                            int nCountIn = 0;

                            int i1 = 0, i2 = 0;
                            for (i1 = 0; i1 < pg1.InteriorRings.Count; i1++)
                            {
                                for (i2 = 0; i2 < pg2.InteriorRings.Count; i2++)
                                {
                                    TpRelateConstants tph = TpPolygontoPolygon.IsTpWith(pg1.InteriorRings[i1],pg2.InteriorRings[i2]);
                                    if (tph == TpRelateConstants.tpEqual)
                                    {
                                        bEqual =true;
                                        nCountEqual++;
                                        break;
                                    }
                                    else if (tph == TpRelateConstants.tpCovers || tph == TpRelateConstants.tpContains)
                                    {
                                        bCoversOrContains = true;
                                        nCountIn++;
                                        break;
                                    }
                                    else if (tph == TpRelateConstants.tpCoveredBy || tph == TpRelateConstants.tpInside)
                                    {
                                        bCoveredByOrInside = true;
                                        nCountOut++;
                                        break;
                                    }
                                    else if (tph == TpRelateConstants.tpTouch || tph == TpRelateConstants.tpIntersect)
                                    {
                                        if (bReport)
                                            MessageBox.Show("两面目标相交", "GIS");

                                        return TpRelateConstants.tpIntersect;
                                    }
                                }

                                if (i2 == pg2.InteriorRings.Count)
                                {
                                    bDisjoint =true;
                                }
                            }

                            if (bEqual && !bCoversOrContains && !bCoveredByOrInside && !bDisjoint)
                            {
                                if (pg1.InteriorRings.Count==pg2.InteriorRings.Count)
                                {
                                    if (bReport)
                                        MessageBox.Show("两面目标相等", "GIS");

                                    return TpRelateConstants.tpEqual;
                                }
                                else if (pg1.InteriorRings.Count<pg2.InteriorRings.Count)
                                {
                                    if (bReport)
                                        MessageBox.Show("按照选择的先后顺序，面目标1覆盖面目标2", "GIS");

                                    return TpRelateConstants.tpCovers;
                                }
                            }
                            else if (bEqual && !bCoversOrContains && !bCoveredByOrInside && bDisjoint)
                            {
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1被面目标2覆盖", "GIS");

                                return TpRelateConstants.tpCoveredBy;
                            }
                            else if (bCoversOrContains && (nCountEqual + nCountIn) == pg2.InteriorRings.Count)
                            {//ppg中除了相等的岛外，所有的岛都被覆盖或是被包含
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1被面目标2覆盖", "GIS");

                                return TpRelateConstants.tpCoveredBy;
                            }
                            else if (bCoveredByOrInside && (nCountEqual + nCountOut) == pg1.InteriorRings.Count)
                            {//this中除了相等的岛外，所有的岛都被覆盖或是被包含
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1覆盖面目标2", "GIS");

                                return TpRelateConstants.tpCovers;
                            }

                            if (bReport)
                                MessageBox.Show("两面目标相交", "GIS");

                            return TpRelateConstants.tpIntersect;
                        }
                    }

                    else if (tp == TpRelateConstants.tpCoveredBy)  //被覆盖
                    {
                        if (pg1.InteriorRings.Count==0&&pg2.InteriorRings.Count!=0)
                        {
                            for (int i = 0; i < pg2.InteriorRings.Count; i++)
                            {
                                TpRelateConstants tph = TpPolygontoPolygon.IsTpWith(pg2.InteriorRings[i],pg1.ExteriorRing);
                                if (tph == TpRelateConstants.tpIntersect || tph == TpRelateConstants.tpInside || tph == TpRelateConstants.tpCoveredBy)
                                {
                                    if (bReport)
                                        MessageBox.Show("两面目标相交", "GIS");

                                    return TpRelateConstants.tpIntersect;
                                }
                            }

                            if (bReport)
                                MessageBox.Show("按照选择的先后顺序，面目标1被面目标2覆盖", "GIS");

                            return TpRelateConstants.tpCoveredBy;
                        }
                        else if (pg1.InteriorRings.Count!=0&&pg2.InteriorRings.Count==0)
                        {
                            if (bReport)
                                MessageBox.Show("按照选择的先后顺序，面目标1被面目标2覆盖", "GIS");

                            return TpRelateConstants.tpCoveredBy;
                        }
                        else
                        {
                            for (int i = 0; i < pg2.InteriorRings.Count; i++)
                            {
                                TpRelateConstants tph = TpPolygontoPolygon.IsTpWith(pg2.InteriorRings[i],pg1.ExteriorRing);
                                if (tph == TpRelateConstants.tpIntersect || tph == TpRelateConstants.tpCoveredBy)
                                {
                                    if (bReport)
                                        MessageBox.Show("两面目标相交", "GIS");

                                    return TpRelateConstants.tpIntersect;
                                }
                                if (tph == TpRelateConstants.tpInside)
                                {
                                    int i2 = 0;
                                    for (i2 = 0; i2 < pg1.InteriorRings.Count; i2++)
                                    {
                                        TpRelateConstants tphh =TpPolygontoPolygon.IsTpWith(pg2.InteriorRings[i],pg1.InteriorRings[i2]);
                                        if (tphh == TpRelateConstants.tpEqual || tphh == TpRelateConstants.tpInside || tphh == TpRelateConstants.tpCoveredBy)
                                        {
                                            break;
                                        }
                                        else if (tphh == TpRelateConstants.tpIntersect || tphh == TpRelateConstants.tpCovers || tphh == TpRelateConstants.tpContains || tphh == TpRelateConstants.tpTouch)
                                        {
                                            if (bReport)
                                                MessageBox.Show("两面目标相交", "GIS");

                                            return TpRelateConstants.tpIntersect;
                                        }
                                    }

                                    if (i2 == pg1.InteriorRings.Count)
                                    {
                                        if (bReport)
                                            MessageBox.Show("两面目标相交", "GIS");

                                        return TpRelateConstants.tpIntersect;
                                    }
                                }
                            }

                            if (bReport)
                                MessageBox.Show("按照选择的先后顺序，面目标1被面目标2覆盖", "GIS");

                            return TpRelateConstants.tpCoveredBy;
                        }
                    }

                    else if (tp == TpRelateConstants.tpInside)  //被包含
                    {
                        if (pg2.InteriorRings.Count == 0 && pg1.InteriorRings.Count != 0)
                        {
                            if (bReport)
                                MessageBox.Show("按照选择的先后顺序，面目标1被面目标2包含", "GIS");

                            return TpRelateConstants.tpInside;
                        }

                        else if (pg2.InteriorRings.Count != 0 && pg1.InteriorRings.Count == 0)
                        {
                            bool touch = false;
                            for (int i = 0; i < pg2.InteriorRings.Count; i++)
                            {
                                TpRelateConstants tph = TpPolygontoPolygon.IsTpWith(pg2.InteriorRings[i],pg1.ExteriorRing );
                                if (tph == TpRelateConstants.tpIntersect || tph == TpRelateConstants.tpInside || tph == TpRelateConstants.tpCoveredBy)
                                {
                                    if (bReport)
                                        MessageBox.Show("两面目标相交", "GIS");

                                    return TpRelateConstants.tpIntersect;
                                }
                                else if (tph == TpRelateConstants.tpCovers || tph == TpRelateConstants.tpEqual)
                                {
                                    if (bReport)
                                        MessageBox.Show("按照选择的先后顺序，面目标1内相接于面目标2的内边界", "GIS");

                                    return TpRelateConstants.tpTouch;
                                }
                                else if (tph == TpRelateConstants.tpTouch)
                                {
                                    touch = true;
                                }
                                else if (tph == TpRelateConstants.tpContains)
                                {
                                    if (bReport)
                                        MessageBox.Show("按照选择的先后顺序，面目标1内相离于面目标2", "GIS");

                                    return TpRelateConstants.tpDisjoint;
                                }
                            }

                            if (touch)
                            {
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1被面目标2覆盖", "GIS");

                                return TpRelateConstants.tpCoveredBy;
                            }
                            else
                            {
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1被面目标2包含", "GIS");

                                return TpRelateConstants.tpInside;
                            }
                        }

                        else
                        {
                            bool touch = false;
                            for (int i = 0; i < pg2.InteriorRings.Count; i++)
                            {
                                TpRelateConstants tph = TpPolygontoPolygon.IsTpWith( pg2.InteriorRings[i],pg1.ExteriorRing);
                                if (tph == TpRelateConstants.tpIntersect || tph == TpRelateConstants.tpCoveredBy)
                                {
                                    if (bReport)
                                        MessageBox.Show("两面目标相交", "GIS");

                                    return TpRelateConstants.tpIntersect;
                                }
                                if (tph == TpRelateConstants.tpCovers || tph == TpRelateConstants.tpEqual)
                                {
                                    if (bReport)
                                        MessageBox.Show("按照选择的先后顺序，面目标1内相接于面目标2的内边界", "GIS");

                                    return TpRelateConstants.tpTouch;
                                }
                                if (tph == TpRelateConstants.tpContains)
                                {
                                    if (bReport)
                                        MessageBox.Show("按照选择的先后顺序，面目标1内相离于面目标2", "GIS");

                                    return TpRelateConstants.tpDisjoint;
                                }
                                if (tph == TpRelateConstants.tpTouch)
                                {
                                    touch = true;
                                }
                                else if (tph == TpRelateConstants.tpInside)
                                {
                                    int i2 = 0;
                                    for (i2 = 0; i2 < pg1.InteriorRings.Count; i2++)
                                    {
                                        TpRelateConstants tphh = TpPolygontoPolygon.IsTpWith( pg2.InteriorRings[i],pg1.InteriorRings[i2]);
                                        if (tphh == TpRelateConstants.tpEqual || tphh == TpRelateConstants.tpCoveredBy)
                                        {
                                            touch = true;
                                            break;
                                        }
                                        if (tphh == TpRelateConstants.tpInside)
                                        {
                                            break;
                                        }
                                        if (tphh == TpRelateConstants.tpIntersect || tphh == TpRelateConstants.tpCovers || tphh == TpRelateConstants.tpContains || tphh == TpRelateConstants.tpTouch)
                                        {
                                            if (bReport)
                                                MessageBox.Show("两面目标相交", "GIS");

                                            return TpRelateConstants.tpIntersect;
                                        }
                                    }

                                    if (i2 == pg1.InteriorRings.Count)
                                    {
                                        if (bReport)
                                            MessageBox.Show("两面目标相交", "GIS");

                                        return TpRelateConstants.tpIntersect;
                                    }
                                }
                            }

                            if (touch)
                            {
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1被面目标2覆盖", "GIS");

                                return TpRelateConstants.tpCoveredBy;
                            }
                            else
                            {
                                if (bReport)
                                    MessageBox.Show("按照选择的先后顺序，面目标1被面目标2包含", "GIS");

                                return TpRelateConstants.tpInside;
                            }
                        }
                    }

                    else
                    {
                        if (bReport)
                        {
                            MessageBox.Show("两面相离", "GIS");
                        }
                        return TpRelateConstants.tpDisjoint;
                    }

                }

            }
            else
            {
                if (bReport)
                {
                    MessageBox.Show("两面相离","GIS");
                }
                return TpRelateConstants.tpDisjoint;
            }

        }
        
    }    

}
