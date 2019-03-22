using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Windows.Forms;

namespace GIS.Toplogical
{
    public static class TpLinetoPolygon
    {
 	
        private static bool m_lout=false;
        private static bool m_lon=false;
        private static bool m_lin=false;
        private static bool m_linmeet=false;
        private static bool m_loutmeet=false;
	
        private static int  m_lout_num=0;
        private static int  m_lon_num=0;
        private static int  m_lin_num=0;
        private static int  m_linmeet_num=0;
        private static int  m_loutmeet_num=0;	
        

        private static void  Reset()
        {
            m_lout=false;
            m_lon=false;
            m_lin=false;
            m_linmeet=false;
            m_loutmeet=false;	

            m_lout_num=0;
            m_lon_num=0;
            m_lin_num=0;
            m_linmeet_num=0;
            m_loutmeet_num=0;
        }



        private static int AddLOut(int num)
        {
            m_lout=true;
            m_lout_num+=num;
            return m_lout_num;
        }
        private static  int  AddLOn(int num)
        {
            m_lon=true;
            m_lon_num+=num;
            return m_lon_num;
        }
        private static  int  AddLIn(int num)
        {	
            m_lin=true;
            m_lin_num+=num;
            return m_lin_num;
        }
        private static  int  AddLInMeet(int num)
        {	
            m_linmeet=true;
            m_linmeet_num+=num;
            return m_linmeet_num;
        }

        private static  void  AddLOutMeet(int num)
        {	
            m_loutmeet=true;
            m_loutmeet_num+=num;
            //return m_loutmeet_num;
        }

        private static bool IsInside()  //INSIDE
        {

            if( (!m_lout) && (!m_lon) && (!m_linmeet) && (!m_loutmeet) && m_lin)
            {   
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsCoveredBy()  //COVEREDBY
        {
            if((!m_lout) && (!m_loutmeet) &&  m_linmeet)
	        {   
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsOn()
        {
            if((!m_lout) && (!m_loutmeet) &&  m_lon && (!m_linmeet) && (!m_lin ))
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
            if(m_loutmeet && !m_linmeet && !m_lin)
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
            if(m_linmeet && m_loutmeet)
	        {   
                return true;
            }
            else
            {
                return false;
            }
        }


        private static bool IsDisJoint() //DISJOINT
        {
            if( (m_lout) && (!m_lon) && (!m_linmeet) && (!m_loutmeet) && (!m_lin))
	        {   
                return true;
            }
            else
            {
                return false;
            }
        }

       
        
        /// <summary>
        /// /该函数的设计改为只对单独的一个多边形与线的关系进行拓扑判定，而复合地块与线的拓扑关系判定
         //可以转化为对各个简单多边形与线的拓扑关系判定,最终的拓扑关系可以由这些关系组合推断出来
        //nIndex=0,是为pg的外边界，pg>0,是为pg的内岛数
        /// </summary>
        /// <param name="?"></param>
        private static void SetValueByPlgs(GeoLineString pl,GeoPolygon pg, int nIndex)
        {

            GeoLineString pl1=new GeoLineString();
            GeoLineString pl2 = new GeoLineString();
            pl1.Vertices.Clear();
            if (nIndex == 0)
            {
                int c = pg.ExteriorRing.Vertices.Count;
                for (int i = 0; i < c; i++)
                {
                    pl1.Vertices.Add(pg.ExteriorRing.Vertices[i]);
                }
                
            }
            else
            {
                int c = pg.InteriorRings[nIndex-1].NumPoints;
                for (int i = 0; i < c; i++)
                {
                    pl1.Vertices.Add(pg.InteriorRings[nIndex-1].Vertices[i]);
                }
            }
            GeoLinearRing pll = new GeoLinearRing(pl1.Vertices);

            TpPLPstruct.TpInsertSplitTwoLine(pl,pl1,pl2);
          

            GeoPoint pt=new GeoPoint(pl2.Vertices[0].X,pl2.Vertices[0].Y);

	        int tp1=0;
            int tp2=0;
            int tp=0;
            
            tp1 = TpPLPstruct.TpPointToLineRing(pt,pll);
 

            for(int c =1; c<pl2.Vertices.Count ;c++, tp1=tp2)
	        {
		        pt.SetXY(pl2.Vertices[c].X,pl2.Vertices[c].Y);
                
                tp2 = TpPLPstruct.TpPointToLineRing(pt,pll);
                
		        //本来应该有9种组合情况，但因为采用了线打断的处理，所以tp1==0&&tp2==1和
		        //tp1==1&&tp2==0两种情况被排除了
		        if(tp1==0 && tp2==0)
		        {
			        AddLOut(1);
			        continue;
		        }
		        else if((tp1==0 && tp2==2 ) || (tp2==0 && tp1==2))
		        {
			        AddLOutMeet(1);
			        continue;
		        }
		        else if(tp1==2 &&  tp2==2)
		        {
			        GeoPoint midPt=new GeoPoint((pl2.Vertices[c-1].X+pl2.Vertices[c].X)/2,(pl2.Vertices[c-1].Y+pl2.Vertices[c].Y)/2);
                    
                    tp = TpPLPstruct.TpPointToLineRing(midPt, pll);
                    

			        if(tp==0)
			        {
				        AddLOutMeet(1);
			        }
			        else if(tp==2)
			        {
				        AddLOn(1);
			        }
			        else if(tp==1 )
			        {
				        AddLInMeet(1);
			        }
			        continue;
		        }
		        else if((tp1==2 && tp2==1) || ( tp2==2 && tp1==1))
		        {
			        AddLInMeet(1);
			        continue;
		        }
		        else if( tp1==1 && tp2==1)
		        {
			        AddLIn(1);
			        continue;
		        }
            }



        }

        /// <summary>
        /// 单线pl，面pg的拓扑关系,返回 tpDisjoint, tpInside(在面内), tpTouch, tpCoveredBy（在面内，并与边界相接）, tpIntersect, tpOn, 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="pg"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoLineString pl,GeoPolygon pg,bool bReport)
        {
            GeoBound bound1 = pl.GetBoundingBox();
            GeoBound bound2 = pg.ExteriorRing.GetBoundingBox();

            if (bound1.IsIntersectWith(bound2))
            {
                
                //先判断面的外边界与线的拓扑关系
                TpLinetoPolygon.Reset();
                TpLinetoPolygon.SetValueByPlgs(pl,pg,0);

                bool IsIntersect1=TpLinetoPolygon.IsIntersect();
	            bool IsTouch1=TpLinetoPolygon.IsTouch();
	            bool IsCoveredBy1=TpLinetoPolygon.IsCoveredBy();
	            bool IsDisJoint1=TpLinetoPolygon.IsDisJoint();
	            bool IsOn1=TpLinetoPolygon.IsOn();
	            bool IsInside1=TpLinetoPolygon.IsInside();

                if (IsDisJoint1)//分离
                {
                    if (bReport)
                       MessageBox.Show("线目标与面目标外相离","GIS");

                    return TpRelateConstants.tpDisjoint;
                }
                else if (IsTouch1)//相接
                {
                    if (bReport)
                        MessageBox.Show("线目标与面目标的外边界相接","GIS");

                    return TpRelateConstants.tpTouch;
                }
                else if (IsOn1)//在边界上（依靠）
                {
                    if (bReport)
                        MessageBox.Show("线目标在面目标的外边界上", "GIS");

                    return TpRelateConstants.tpOn;
                }
                else if( IsCoveredBy1)//被覆盖
                {//考虑带岛多边形的情况，还可能出现相交的情况
                    
                    int plgPartCount = pg.InteriorRings.Count;
                    for (int k = 1; k < plgPartCount+1; k++)
                    {
                        TpLinetoPolygon.Reset();
                        TpLinetoPolygon.SetValueByPlgs(pl,pg,k);
                      
                        bool IsIntersect2=TpLinetoPolygon.IsIntersect();
	                    bool IsTouch2=TpLinetoPolygon.IsTouch();
	                    bool IsCoveredBy2=TpLinetoPolygon.IsCoveredBy();
	                    bool IsDisJoint2=TpLinetoPolygon.IsDisJoint();
	                    bool IsOn2=TpLinetoPolygon.IsOn();
	                    bool IsInside2=TpLinetoPolygon.IsInside();


                        if (IsIntersect2)
                        {
                            if (bReport)
                                MessageBox.Show( "线目标与面目标相交","GIS");

                            return TpRelateConstants.tpIntersect;
                        }
                    }

                    if (bReport)
                        MessageBox.Show( "线目标被面目标覆盖","GIS");

                    return TpRelateConstants.tpCoveredBy;
                }
                else if(IsInside1)//被包含
                {//考虑带岛多边形的情况，还可能出现被覆盖、在边界线上、相接、相交、分离的情形
                    
                    int plgPartCount =  pg.InteriorRings.Count ;
                    bool bIsCoveredBy = false;
                    for (int k = 1; k < plgPartCount+1; k++)
                    {
                        
                        TpLinetoPolygon.Reset();
                        TpLinetoPolygon.SetValueByPlgs(pl,pg,k);

                        bool IsIntersect2=TpLinetoPolygon.IsIntersect();
	                    bool IsTouch2=TpLinetoPolygon.IsTouch();
	                    bool IsCoveredBy2=TpLinetoPolygon.IsCoveredBy();
	                    bool IsDisJoint2=TpLinetoPolygon.IsDisJoint();
	                    bool IsOn2=TpLinetoPolygon.IsOn();
	                    bool IsInside2=TpLinetoPolygon.IsInside();

                        if (IsOn2)
                        {
                            if (bReport)
                                MessageBox.Show( "线目标在面目标的内边界上","GIS");

                            return TpRelateConstants.tpOn;
                        }
                        else if (IsCoveredBy2)
                        {
                            if (bReport)
                                MessageBox.Show( "线目标与面目标的内边界相接","GIS");

                            return TpRelateConstants.tpTouch;
                        }
                        else if (IsInside2)
                        {
                            if (bReport)
                                MessageBox.Show( "线目标与面目标内相离","GIS");

                            return TpRelateConstants.tpDisjoint;
                        }
                        else if (IsIntersect2)
                        {
                            if (bReport)
                                MessageBox.Show( "线目标与面目标相交","GIS");

                            return TpRelateConstants.tpIntersect;
                        }
                        else if (IsTouch2)
                        {
                            bIsCoveredBy = true;
                        }
                    }

                    if (bIsCoveredBy)
                    {
                        if (bReport)
                            MessageBox.Show( "线目标被面目标覆盖","GIS");

                        return TpRelateConstants.tpCoveredBy;
                    }

                    if (bReport)
                        MessageBox.Show( "线目标被面目标包含","GIS");

                    return TpRelateConstants.tpInside;
                }
                else if(IsIntersect1)//相交
                {
                    if (bReport)
                        MessageBox.Show( "线目标与面目标相交","GIS");

                    return TpRelateConstants.tpIntersect;
                }
                else
                {
                    if (bReport)
                    {
                        MessageBox.Show("线与面相离","GIS");
                    }

                    return TpRelateConstants.tpDisjoint;

                }

		    }
            
            else
            {
                if (bReport)
                {
                    MessageBox.Show("线与面相离","GIS");
                }

                return TpRelateConstants.tpDisjoint;
            }


        }



        /// <summary>
        /// 面pg，单线pl的拓扑关系,返回tpDisjoint, tpContains（在内部）, tpTouch, tpCovers（在面内，并与边界相接）, tpIntersect, tpOn, 
        /// </summary>
        /// <param name="pg"></param>
        /// <param name="pl"></param>
        /// <param name="bReport"></param>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoPolygon pg, GeoLineString pl, bool bReport)

        {
            TpRelateConstants tppl = TpLinetoPolygon.IsTpWith(pl,pg,false);
            if (tppl == TpRelateConstants.tpCoveredBy)
            {
                if (bReport)
                     MessageBox.Show( "面目标覆盖线目标" );

                return TpRelateConstants.tpCovers;
            }
            else if (tppl == TpRelateConstants.tpInside)
            {
                if (bReport)
                    MessageBox.Show( "面目标包含线目标","GIS");

                return TpRelateConstants.tpContains;
            }
            else if (tppl == TpRelateConstants.tpDisjoint)
            {
                if (bReport)
                    MessageBox.Show( "面目标与线目标相离","GIS");

                return TpRelateConstants.tpDisjoint;
            }
            else if (tppl == TpRelateConstants.tpTouch)
            {
                if (bReport)
                    MessageBox.Show( "面目标与线目标相接","GIS");

                return TpRelateConstants.tpTouch;
            }
            else if (tppl == TpRelateConstants.tpOn)
            {
                if (bReport)
                    MessageBox.Show( "线目标在面目标的边界上","GIS");

                return TpRelateConstants.tpOn;
            }
            else
            {
                if (bReport)
                    MessageBox.Show( "面目标与线目标相交","GIS");

                return TpRelateConstants.tpIntersect;
            }

        }


        /// <summary>
        /// 多线pls，多面pgs的拓扑关系,返回tpDisjoint, tpInside（在内部）, tpTouch, tpCoverBy（在面内，并与边界相接）, tpIntersect, tpOn
        /// </summary>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoMultiLineString pls,GeoMultiPolygon pgs, bool bReport)
        {
            return TpRelateConstants.tpUnknow;
        }


        /// <summary>
        /// 多线pls，多面pgs的拓扑关系,返回tpDisjoint, tpContains（在内部）, tpTouch, tpCovers（在面内，并与边界相接）, tpIntersect, tpOn
        /// </summary>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith( GeoMultiPolygon pgs, GeoMultiLineString pls, bool bReport)
        {
            return TpRelateConstants.tpUnknow;
        }




        /// <summary>
        /// 多线pls，面pg的拓扑关系,返回tpDisjoint, tpInside（在内部）, tpTouch, tpCoverBy（在面内，并与边界相接）, tpIntersect, tpOn
        /// </summary>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoMultiLineString pls, GeoPolygon pg, bool bReport)
        {
            return TpRelateConstants.tpUnknow;
        }



        /// <summary>
        /// 多线pls，面pg的拓扑关系,返回tpDisjoint, tpContains（在内部）, tpTouch, tpCovers（在面内，并与边界相接）, tpIntersect, tpOn
        /// </summary>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoPolygon pg, GeoMultiLineString pls, bool bReport)
        {
            return TpRelateConstants.tpUnknow;
        }


        /// <summary>
        /// 多线pls，面pg的拓扑关系,返回tpDisjoint, tpInside（在内部）, tpTouch, tpCoverBy（在面内，并与边界相接）, tpIntersect, tpOn
        /// </summary>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoLineString pls, GeoMultiPolygon pg, bool bReport)
        {
            return TpRelateConstants.tpUnknow;
        }



        /// <summary>
        /// 多线pls，面pg的拓扑关系,返回tpDisjoint, tpContains（在内部）, tpTouch, tpCovers（在面内，并与边界相接）, tpIntersect, tpOn
        /// </summary>
        /// <returns></returns>
        public static TpRelateConstants IsTpWith(GeoMultiPolygon pg, GeoLineString pls, bool bReport)
        {
            return TpRelateConstants.tpUnknow;
        }
    
    
    }
}
