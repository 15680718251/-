using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GIS.Toplogical;
using GIS.Layer;
using GIS.Geometries;
using GIS.Increment;

using System.Windows.Forms;
using System.Drawing;
using GIS.Map;
using GIS.Utilities;
using GIS.TreeIndex.Tool;
using GIS.SpatialRelation;
using GIS.TreeIndex.GeometryComputation;
using System.IO;
using System.Threading;


namespace GIS.TreeIndex.GeometryComputation
{

    public class PointSign
    {
        public GeoPoint point=new GeoPoint();
        public bool sign = false;

    }


    public class LinesSplitLines
    {

        /// <summary>
        /// 去除一个点组中的重复点
        /// </summary>
        /// <param name="pts"></param>
        public static void ClearRepeatPoint(List<GeoPoint> pts)
        {
            int i=0;
            int j=0;

            for( i=0; i<pts.Count-1; i++ )
            {
	            for( j=i+1; j<pts.Count; j++ )
	            {
		            
                  if(pts[i].IsEqual(pts[j]))
                  {
                      pts.RemoveAt(j);
                        j--;

                  }


	            }
            }
        }

        /// <summary>
        /// 去除一个点组中的重复点
        /// </summary>
        /// <param name="pts"></param>
        public static void ClearRepeatSPoint(List<PointSign> pts)
        {
            int i = 0;
            int j = 0;

            for (i = 0; i < pts.Count - 1; i++)
            {
                for (j = i + 1; j < pts.Count; j++)
                {

                    if (pts[i].point.IsEqual(pts[j].point))
                    {
                        pts.RemoveAt(j);
                        j--;

                    }


                }
            }
        }


        /// <summary>
        /// 去除一个线组中的重复线
        /// </summary>
        /// <param name="m_Lines"></param>
        public static void ClearRepeatLines(List<GeoLineString> m_Lines)
        {
            int index = 0;
            int count = 0;
            int sum = 0;
            sum = m_Lines.Count ;
            for (index = 0; index < sum - 1; index++)
            {
                for (count = index + 1; count < sum; count++)
                {
                    if (m_Lines [index].IsEqual(m_Lines [count]))
                    {
                        m_Lines.RemoveAt(count);
                        sum--;
                        count--;
                    }
                }
            }
        }
        /// <summary>
        /// 重新排列点的顺序，标准是每个点到同一点的距离，由小到大
        /// </summary>
        /// <param name="m_Points"></param>
        /// <param name="pt"></param>
        public static void ResortPoints( List<GeoPoint> m_Points, GeoPoint pt)
        {
            GeoPoint midLPoint=new GeoPoint();	
            double midf=0;
            int i=0;
            int n=0;
            int index=0;
            int sum=m_Points.Count;
            double[] length= new double[sum];

            for( i=0; i<sum; i++ )
            {
	            length[i]=m_Points[i].DistanceTo(pt);
            }
            for( i=0;i<sum-1;i++)
            {
	            index=i;
	            for( n=i+1; n<sum; n++ )
	            {
		            if( length[index]>length[n] )
			            index=n;
	            }
	            if( index!=i )
	            {
		            midf=length[i];
		            length[i]=length[index];
		            length[index]=midf;
        			
		            midLPoint=m_Points[i];
		            m_Points[i]=m_Points[index];
		            m_Points[index]=midLPoint;
	            }
            }
            //delete[] length; 
        }

        /// <summary>
        /// 重新排列点的顺序，标准是每个点到同一点的距离，由小到大
        /// </summary>
        /// <param name="m_Points"></param>
        /// <param name="pt"></param>
        public static void ResortSPoints(List<PointSign> m_Points, PointSign pt)
        {
            double midf = 0;
            int i = 0;
            int n = 0;
            int index = 0;
            int sum = m_Points.Count;
            double[] length = new double[sum];

            for (i = 0; i < sum; i++)
            {
                length[i] = m_Points[i].point.DistanceTo(pt.point);
            }
            for (i = 0; i < sum - 1; i++)
            {
                index = i;
                for (n = i + 1; n < sum; n++)
                {
                    if (length[index] > length[n])
                        index = n;
                }
                if (index != i)
                {
                    midf = length[i];
                    length[i] = length[index];
                    length[index] = midf;

                    PointSign midLPoint1 = new PointSign();
                    PointSign midLPoint2 = new PointSign();

                    midLPoint1.point.SetXY(m_Points[i].point.X,m_Points[i].point.Y);
                    midLPoint1.sign = m_Points[i].sign;
                    midLPoint2.point.SetXY(m_Points[index].point.X, m_Points[index].point.Y);
                    midLPoint2.sign = m_Points[index].sign;

                    m_Points.RemoveAt(i);
                    m_Points.Insert(i,midLPoint2);
                    m_Points.RemoveAt(index);
                    m_Points.Insert(index,midLPoint1);
                }
            }
            //delete[] length; 
        }
        public static bool IntersectTwoLine(GeoPoint pt1,GeoPoint pt2,GeoPoint pt3,GeoPoint pt4, GeoPoint pt5,
            GeoPoint pt6,ref int jd0,ref int jd1,ref int jd2,ref int jd3)
        {
	        
            pt5.SetXY(0,0);
            pt6.SetXY(0,0);
            jd0=0; jd1=0; jd2=0;  jd3=0;
            
            
            GeoBound bound1=new GeoBound(Math.Min(pt1.X,pt2.X),Math.Min(pt1.Y,pt2.Y),
                Math.Max(pt1.X,pt2.X),Math.Max(pt1.Y,pt2.Y));
            GeoBound bound2=new GeoBound(Math.Min(pt3.X,pt4.X),Math.Min(pt3.Y,pt4.Y),
                Math.Max(pt3.X,pt4.X),Math.Max(pt3.Y,pt4.Y));
            bool bl=bound1.LeftBottomPt.X-0.0001<bound2.RightUpPt.X && 
                bound2.LeftBottomPt.X-0.0001<bound1.RightUpPt.X &&
                bound1.LeftBottomPt.Y-0.0001<bound2.RightUpPt.Y &&
                bound2.LeftBottomPt.Y-0.0001<bound1.RightUpPt.Y;

	        if( bl)
	        {
		        
		         bool has=false;
                double a=0;   double b=0;
                double r1=0;  double r2=0;
                double x1=pt1.X;  double y1=pt1.Y;
                double x2=pt2.X;  double y2=pt2.Y;
                double x3=pt3.X;  double y3=pt3.Y;
                double x4=pt4.X;  double y4=pt4.Y;

                double r=(x2-x1)*(y3-y4)-(y2-y1)*(x3-x4);//向量pt1pt2跟向量pt4pt3的差乘
                
                if(Math.Abs(r)<0.000001)//表示两向量平行
		        {
			       
                    r=(x2-x3)*(y1-y3)-(y2-y3)*(x1-x3);//pt3pt2差乘pt3pt1

			        if(Math.Abs(r)<0.000001)//两向量共线
			        {
                        if(Toplogical.TpPLPstruct.TpPointToLine(pt1,pt3,pt4)>0)//起点交
                        {
                            jd0=1;
                            has=true;
                               
                        }
				        if(Toplogical.TpPLPstruct.TpPointToLine(pt2,pt3,pt4)>0)//终点交
				        {
					        jd1=1;
					        has=true;
				        }
				        if(Toplogical.TpPLPstruct.TpPointToLine(pt3,pt1,pt2) == 3)//起点交于原线内部，pt3在线pt1pt2内部
				        {
                          pt5.SetXY(pt3.X,pt3.Y);
					        jd2=1;
					        has=true;
				        }
				        if(Toplogical.TpPLPstruct.TpPointToLine(pt4,pt1,pt2) == 3)//终点交于原线内部，pt4在线pt1pt2内部
				        {
					        
                          pt6.SetXY(pt4.X,pt4.Y);
					        jd3=1;
					        has=true;
				        }

				        if(has==true)
					        return true;
			        }
			        return false;
		        }

		        r1=(x3-x1)*(y3-y4)-(x3-x4)*(y3-y1);	
                r2=(x2-x1)*(y3-y1)-(y2-y1)*(x3-x1);
		        a=r1/r; b=r2/r;
		        
		        if ((Math.Abs(a)< 0.000001 || (a > 0 && a < 1) || Math.Abs(a - 1) < 0.000001) && 
			        (Math.Abs(b) < 0.000001 || (b > 0 && b < 1) || Math.Abs(b - 1) < 0.000001))
		        {//在a>=0 && a<=1 且 b>=0 && b<=1的情况下

                    pt5.SetXY(x1+a*(x2-x1),y1+a*(y2-y1));
			        if(pt5.IsEqual(pt1))//起点交于内部
			        {
				        jd0=1;
				        return true;
			        }
			        if(pt5.IsEqual(pt2))//终点交于内部
			        {
				        jd1=1;
				        return true;
			        }

			        jd2=1;//注：两线内部相交
			        return true;//相交
		        }
	        }
	        return false;  //相离
        }
        
        /// <summary>
        /// 去除一条线上的重复点，即紧邻的两点相等
        /// </summary>
        /// <param name="src"></param>
        public static void ClearLineRepeatPoints(GeoLineString src)
        {
         
	        for(int index=0; index<src.NumPoints-1; index++)
	        {
                if(src.Vertices[index].IsEqual(src.Vertices[index+1]))
                {
                    src.Vertices.RemoveAt(index);
                    index--;
                }
	        }
        }
        

        /// <summary>
        /// 打断自相交
        /// </summary>
        /// <param name="src"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public static bool SelfSplitLine(GeoLineString src, List<GeoLineString> des, bool hasRepeat)
        {

            des.Clear();
            bool isSelfIntersect=false;
            int index=0;  int count=0;
            int srcUpper=0;
            int jd0=0; int jd1=0; int jd2=0;  int jd3=0;

            //List<GeoPoint> m_Points=new List<GeoPoint>();
            //List<GeoPoint> m_insertPoints=new List<GeoPoint>();
            
            GeoLineString parts=new GeoLineString();

            List<PointSign> m_Points = new List<PointSign>();//线端点
            List<PointSign> m_insertPoints = new List<PointSign>();//源线和目的线的内交点

            srcUpper=src.Vertices.Count-1;//注意：暂时不处理多段线的情况*********
	        //将求多义线的交转换为求线段的交
	        for(index=0; index<srcUpper; index++)
	        {
                m_insertPoints.Clear();

                PointSign spoint = new PointSign();//交点：起点，终点，内点

                spoint.point.SetXY(src.Vertices[index].X, src.Vertices[index].Y);
                spoint.sign = false;//开始不为断点



		        for( count=0 ; count < srcUpper; count++ )
		        {
                    //GeoPoint point1 = new GeoPoint();
                    //GeoPoint point2 = new GeoPoint();
                    PointSign spoint1 = new PointSign();
                    PointSign spoint2 = new PointSign();

                    //不考虑与自己相交的情况
			        if(index==count)
				        continue;  
                    
                    bool brt=IntersectTwoLine(src.Vertices[index],src.Vertices[index+1],src.Vertices[count]
                        , src.Vertices[count + 1], spoint1.point, spoint2.point, ref jd0, ref jd1, ref jd2, ref jd3);
			        //求直线段与其他线段的相交情况，返回第一条线交第二条线的情况：0表示无交点，1表示有交点
			        if(brt)//两条线的相交情况
			        {
				        /*注意：这里不考虑终点的相交情况，因为前一条线段的终点会成为下一条线段的起点*/
				        if( count!=index-1 && jd0==1 )
				        {//相接，但第二条线不是前相邻的线段
                            spoint.sign = true;
				        }
				        if( jd2==1  )
				        { 
                            spoint1.sign=true;
                           
                            PointSign spt = new PointSign();
                            spt.point.SetXY(spoint1.point.X,spoint1.point.Y);
                            spt.sign = spoint1.sign;
                            m_insertPoints.Add(spt);  
				        }
				        if( jd3==1  )
				        { 
                            //point2.SetSign(true);
                            
                            //GeoPoint pt = new GeoPoint(point2.X, point2.Y);
                            //pt.SetSign(point2.sign);
                            //m_insertPoints.Add(pt);
                            spoint2.sign = true;

                            PointSign spt = new PointSign();
                            spt.point.SetXY(spoint2.point.X, spoint2.point.Y);
                            spt.sign = spoint2.sign;
                            m_insertPoints.Add(spt);  
				        }
			        }
		        }

                //GeoPoint ptt = new GeoPoint(point.X, point.Y);
                //ptt.SetSign(point.sign);  
                //m_Points.Add(ptt);
                PointSign sptt = new PointSign();
                sptt.point.SetXY(spoint.point.X, spoint.point.Y);
                sptt.sign = spoint.sign;

                m_Points.Add(sptt);

		        if(m_insertPoints.Count>1)
		        {
                    
                    ClearRepeatSPoint(m_insertPoints);//清楚重复点
                    ResortSPoints(m_insertPoints,spoint);//排序

		        }
		        if(m_insertPoints.Count>0)//该线自相交且需重新创建线,需归还原线的内存
		        {
			        isSelfIntersect=true;
                    for(int ii=0;ii<m_insertPoints.Count;ii++)//添加交点
                    {
                        PointSign spt = new PointSign();
                        spt.point.SetXY(m_insertPoints[ii].point.X, m_insertPoints[ii].point.Y);
                        spt.sign=m_insertPoints[ii].sign;
                        m_Points.Add(spt);
                    }
		        }
	        }

	        //虽然没有内相交点，但可能有起点相交点。
	        if (!isSelfIntersect)
	        {
		        for(int cc=1;cc<m_Points.Count;cc++)
		        {//cc为1是为了去掉线条起始点，它不应被考虑为交点
			        if(m_Points[cc].sign==true)
			        {
				        isSelfIntersect=true;
				        break;
			        }
		        }
	        }

	        if(isSelfIntersect==false)//未自相交
	        {
		        return false;
	        }
	        else
	        {
                //m_Points.Add(src.Vertices[srcUpper]);  
                // m_Points[0].SetSign(true);
                //m_Points[m_Points.Count-1].SetSign(true);
                PointSign spt = new PointSign();
                spt.point.SetXY(src.Vertices[srcUpper].X, src.Vertices[srcUpper].Y);//添加线上的最后一点
                spt.sign=true;
                m_Points.Add(spt);
                m_Points[0].sign = true;

		        //断点为二时构成线
                List<GeoPoint> linePoints=new List<GeoPoint>();
		        int tc=0;
		        for( count=0; count<m_Points.Count; count++ )
		        {
			        
                    if(m_Points[count].sign==true)
			        {
				        tc++;
			        }
                    GeoPoint ptt = new GeoPoint();
                    ptt.SetXY(m_Points[count].point.X, m_Points[count].point.Y);
                    linePoints.Add(ptt);

			        if(tc==2)
			        {
				        tc=1;
				        //由linePoints形成一个线对象
				        GeoLineString l=new GeoLineString();
                        for (int i = 0; i < linePoints.Count; i++)
                        {
                            GeoPoint pt = new GeoPoint();
                            pt.SetXY(linePoints[i].X, linePoints[i].Y);
                            //pt.SetSign(linePoints[i].sign);
                            l.Vertices.Add(pt);
                                
                        }

                            des.Add(l);
				        linePoints.Clear();
                      linePoints.Add(m_Points[count].point);
			        }
		        }

		        if (hasRepeat==false)
		        {
			        ClearRepeatLines(des);
		        }

		        return true;
	        }
        }
        
        
        
        /// <summary>
        /// 打断线
        /// </summary>
        /// <param name="?"></param>
        public static void SplitLines(List<GeoLineString> src, List<GeoLineString> des, bool hasRepeat)
        {

            //自相交线打断，打断后存储到数组tempDes中,未自相交的线也存储到tempDes
            List<GeoLineString> linesSplited=new List<GeoLineString>();//记录src中被打断的线
           List<GeoLineString> tempDes=new List<GeoLineString>();
            
            

            GeoLineString lines=new GeoLineString();
            bool isSelfIntesect=false;
            int lc=0;
	        for(lc=0;lc<src.Count;lc++)
	        {

                
                lines.Vertices.Clear();
                for(int i=0;i<src[lc].Vertices.Count;i++)
                {
                    lines.Vertices.Add(src[lc].Vertices[i]);
                }

		        ClearLineRepeatPoints(lines);
               
		        if(lines.Vertices.Count == 2)
		        {

                    GeoLineString pl = new GeoLineString();
                    for (int i = 0; i < src[lc].Vertices.Count; i++)
                    {
                        pl.Vertices.Add(src[lc].Vertices[i]);
                    }

                        tempDes.Add(pl);
		        }
		        else
		        {

                    List<GeoLineString> tempLines = new List<GeoLineString>();
                    //如果是自相交原线不加入到tempDes中，打断后的新线加入；如果不自相交，则直接加入到tempDes中。
			        isSelfIntesect = SelfSplitLine(lines, tempLines, hasRepeat);
                    if(isSelfIntesect)
			        {
				        for(int j=0;j<tempLines.Count;j++)
				        {

                            GeoLineString pl = new GeoLineString();
                            for (int i = 0; i < tempLines[j].Vertices.Count; i++)
                            {
                                GeoPoint pt = new GeoPoint();
                                //pt.SetSign(tempLines[j].Vertices[i].sign);
                                pt.SetXY(tempLines[j].Vertices[i].X, tempLines[j].Vertices[i].Y);
                                pl.Vertices.Add(pt);
                            }

                            tempDes.Add(pl);


				        }
                        //linesSplited.Add(lines);

			        }
			        else
			        {
                        GeoLineString pl = new GeoLineString();
                        for (int i = 0; i < src[lc].Vertices.Count; i++)
                        {
                            pl.Vertices.Add(src[lc].Vertices[i]);
                        }

                        tempDes.Add(pl);
			        }
		        }
	        }

	       
           //与其他线相交的打断:将线与线在交点处打断，未相交的线和打断后的线集保存到des	
	        //变量说明参考g_SelfSplitLine说明


            int sj0=0; int sj1=0; int sj2=0;  int sj3=0;
            int desLineUpper=0;   int desLineUpper2=0;
            bool haveInterIntersect=false;

           //记录中间过渡性的线，它是自相交打断成的新线，在后面又与其他线相交需再次打断，故需立即删除
            List<GeoLineString> LinesNeedToDel=new List<GeoLineString>();

	        for( lc=0; lc<tempDes.Count; lc++ )
	        {
                List<PointSign> m_Points = new List<PointSign>();

                GeoLineString parts=new GeoLineString(tempDes[lc].Vertices);
                desLineUpper= parts.Vertices.Count-1;
		        for(int pi=0; pi<desLineUpper; pi++)
		        {//每一条多义线的线段
			        
                  List<PointSign> m_insertPoints = new List<PointSign>();
                  PointSign spoint = new PointSign();
                    spoint.point.SetXY(parts.Vertices[pi].X, parts.Vertices[pi].Y);
                    spoint.sign=false;//默认不为断点
			        
                    for(int index=0 ;  index<tempDes.Count ; index++ )
			        {//线段相对的每一条线

				        if(index==lc)
					        continue;//不考虑与自己的情况

                        PointSign spoint1 = new PointSign();
                        PointSign spoint2 = new PointSign();

                        GeoLineString parts2=new GeoLineString(tempDes[index].Vertices);

                        GeoBound bound1=parts.GetBoundingBox();
                        GeoBound bound2=parts2.GetBoundingBox();
				        if( bound1.IsIntersectWith(bound2))
				        {
					        desLineUpper2=parts2.Vertices.Count-1 ;
					        for( int pc=0; pc<desLineUpper2; pc++ )
					        {
                                 
                                bool brt1 = IntersectTwoLine(tempDes[lc].Vertices[pi], tempDes[lc].Vertices[pi+1],
                                       tempDes[index].Vertices[pc], tempDes[index].Vertices[pc + 1], spoint1.point,
                                       spoint2.point,ref sj0, ref sj1, ref sj2, ref sj3);
                                       
                               
                               if(brt1)
						        {
							        if( sj0==1 )
							        {
                                        spoint.sign=true;
							        }
							        if( sj2==1)
							        {
                                        spoint1.sign=true;
                                        
                                        PointSign pt = new PointSign();
                                        pt.sign=spoint1.sign;
                                        pt.point.SetXY(spoint1.point.X,spoint1.point.Y);
                                        m_insertPoints.Add(pt);  

							        }
							        if( sj3==1)
							        {
                                        spoint2.sign=true;
                                        PointSign pt = new PointSign();
                                        pt.sign = spoint2.sign;
                                        pt.point.SetXY(spoint2.point.X, spoint2.point.Y);
                                        m_insertPoints.Add(pt);    
							        }
						        }
					        }
				        }
			        }			


                    PointSign sptt = new PointSign();
                    sptt.point.SetXY(spoint.point.X, spoint.point.Y);
                    sptt.sign = spoint.sign;

                    m_Points.Add(sptt);

                    //for (int ii = 0; ii < m_insertPoints.Count; ii++)
                    //{
                    //    GeoPoint pt = new GeoPoint(m_insertPoints[ii].X,m_insertPoints[ii].Y);
                    //}

                        if (m_insertPoints.Count > 1)
                        {
                            ClearRepeatSPoint(m_insertPoints);   //清楚重复点
                            ResortSPoints(m_insertPoints, spoint);  //排序
                        }
			        if( m_insertPoints.Count>0 )//该线与他线相交且需重新创建线,需归还原线的内存
			        {
				        haveInterIntersect=true;
				         for(int ii=0;ii<m_insertPoints.Count;ii++)//添加交点
                        {
                            PointSign pt = new PointSign();
                            pt.point.SetXY(m_insertPoints[ii].point.X, m_insertPoints[ii].point.Y);
                            pt.sign=m_insertPoints[ii].sign;
                               m_Points.Add(pt);


                        }		//添加交点
			        } 
		        }

		        //虽然没有内相交点，但可能有起点相交点。
		        if (!haveInterIntersect)
		        {
			        for(int cc=1;cc<m_Points.Count;cc++)
			        {//cc为1是为了去掉线条起始点，它不应被考虑为交点
				        if(m_Points[cc].sign==true)
				        {
					        haveInterIntersect=true;
					        break;
				        }
			        }
		        }

                

                    //与其他线有相交的线，将打断的新线保存到目标线集des中，如果与其他线没有相交的线，则直接保存到目标线集中
                    if (haveInterIntersect == true)
                    {
                        //m_Points.Add(parts.Vertices[desLineUpper]);
                        //m_Points[0].SetSign(true);
                        //m_Points[m_Points.Count - 1].SetSign(true);
                        PointSign pt = new PointSign();
                        pt.point.SetXY(parts.Vertices[desLineUpper].X, parts.Vertices[desLineUpper].Y);//添加最后一点
                        pt.sign = true;

                        m_Points.Add(pt);
                        m_Points[0].sign = true;//起点和最后一点为断点

                        List<GeoPoint> linePoints = new List<GeoPoint>();
                        int tc = 0;
                        int sumL = m_Points.Count;//断点为二时构成线
                        for (int count = 0; count < sumL; count++)
                        {
                            if (m_Points[count].sign == true)
                            {
                                tc++;
                            }

                            linePoints.Add(m_Points[count].point);

                            if (tc == 2)
                            {
                                tc = 1;
                                //由linePoints形成一个线对象---!暂时不要加入到活动层中，只保存到des中

                                GeoLineString l = new GeoLineString();
                                for (int i = 0; i < linePoints.Count; i++)
                                {
                                    GeoPoint ptt = new GeoPoint();
                                    ptt.SetXY(linePoints[i].X, linePoints[i].Y);
                                    l.Vertices.Add(ptt);

                                }

                                des.Add(l);
                                linePoints.Clear();
                                linePoints.Add(m_Points[count].point);

                            }
                        }

                        bool bNeedToDel = true;
                        int nSrcCount = src.Count;
                        for (int kk = 0; kk < nSrcCount; kk++)
                        {
                            if (src[kk].IsEqual(parts))
                            {//表明该线是src中的线
                                bNeedToDel = false;
                                linesSplited.Add(parts);
                                break;
                            }
                        }
                        if (bNeedToDel)
                            LinesNeedToDel.Add(parts);
                    }
                    else
                    {

                        GeoLineString l = new GeoLineString();
                        for (int i = 0; i < tempDes[lc].Vertices.Count; i++)
                        {
                            GeoPoint pt = new GeoPoint();
                            pt.SetXY(tempDes[lc].Vertices[i].X, tempDes[lc].Vertices[i].Y);
                            l.Vertices.Add(pt);

                        }

                        des.Add(l);
                    }
	        }

	        //被打断的线的内存应归还

            //LinesNeedToDel.Clear();
            //src.RemoveAll();
            //src.Append(linesSplited);//返回src中打断的线

	        if(hasRepeat==false && des.Count>0)
	        {
		        ClearRepeatLines(des);//清楚重复线
	        }
        }



    }
}
