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
    public  class NodeTp
    {
        public double SQI=0;//线起点QI值
        public double EQI=0;//线终点QI值
        public bool SUsed=false;//线起点是否搜索
        public bool EUsed=false;//线终点是否搜索


        public static double CountQI(GeoPoint pt1, GeoPoint pt2)
        {
            double dx=pt2.X-pt1.X;
            double dy=pt2.Y-pt1.Y;
            double CQI = 0;

            if( dy>0 )
            {
	            if( dx<0 )
	            {
		            if( dy > -dx )
		            {
                        CQI = 8 + dx / dy;
                        return CQI;
		            }
		            else
		            {
                        CQI = 6 - dy / dx;
                        return CQI;
		            }
	            }
	            else
	            {
		            if( dy > dx )
		            {
                        CQI = dx / dy;
                        return CQI;
		            }
		            else
		            {
                        CQI = 2 - dy / dx;
                        return CQI;
		            }
	            }
            }
            else
            {
	            if(dx<0)
	            {
		            if( dy>dx )
		            {
                        CQI = 6 - dy / dx;
                        return CQI;
		            }
		            else
		            {
                        CQI = 4 + dx / dy;
                        return CQI;
		            }
	            }
	            else
	            {
		            if(dx>-dy)
		            {
                        CQI = 2 - dy / dx;
                        return CQI;
		            }
		            else
		            {
                        CQI = 4 + dx / dy;
                        return CQI;
		            }
	            }
            }
        }

    }
    public class LineInfo
    {
        public int id=0;//线ID
        public bool aspect=false;//与线相邻的左界址线方向
        public double Qi=0;//QI值
    }
    
    public class CreatePolygon
    {
        public static bool IsClockWise(GeoLinearRing plr)
        {
            double area =0;
            int sum = plr.NumPoints;
            for (int n = 1; n < sum; n++)
            {
                area += (plr.Vertices[n].X - plr.Vertices[n - 1].X) * (plr.Vertices[n].Y + plr.Vertices[n - 1].Y);
            }
            if (area > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static void GetLeftTouchLine(List<GeoLineString> src, List<NodeTp> lineTp, int index, bool aspect, ref int touchId, ref bool touchAspect,ref bool result )
            {
	            
                GeoPoint pt=new GeoPoint();
                double Qi=0; 
                GeoLineString line=new GeoLineString();
                GeoLineString parts=new GeoLineString();
                for(int i=0;i<src[index].Vertices.Count;i++)
                {
                    line.AddPoint(src[index].Vertices[i]);
                    parts.AddPoint(src[index].Vertices[i]);
                }



                if(aspect==true)
	            {
                    pt.SetXY(parts.EndPoint.X,parts.EndPoint.Y);
                    Qi=lineTp[index].EQI;//获取终点和终点的qi值

	            }
	            else
	            {
		            pt.SetXY(parts.StartPoint.X,parts.StartPoint.Y);	
                    Qi=lineTp[index].SQI;
	            }


                int count=src.Count;
                List<LineInfo> InfoLarge=new List<LineInfo>();//存储大于当前qi值的线
                List<LineInfo> InfoSmall=new List<LineInfo>();//存储小于当前qi值的线
                //LineInfo varInfo=new LineInfo();

	            for(int i=0; i<count; i++)
	            {

                    line.Vertices.Clear();
                    parts.Vertices.Clear();
                    for (int j = 0; j < src[i].Vertices.Count; j++)
                    {
                        line.AddPoint(src[i].Vertices[j]);
                        parts.AddPoint(src[i].Vertices[j]);
                    }

                    if (i == index)
                    {
                        if (parts.EndPoint.IsEqual(parts.StartPoint))
                        {
                            LineInfo varInfo = new LineInfo();

                            varInfo.aspect = aspect;
                            varInfo.id = index;
                            if (aspect == true)
                            {
                                varInfo.Qi = lineTp[i].SQI;
                            }
                            else
                            {
                                varInfo.Qi = lineTp[i].EQI;
                            }
                            if (varInfo.Qi > Qi)
                            {
                                InfoLarge.Add(varInfo);
                            }
                            else
                            {
                                InfoSmall.Add(varInfo);
                            }
                        }
                        continue;
                    }

		            if(parts.StartPoint.IsEqual(pt))
		            {
                        LineInfo varInfo = new LineInfo();

                        varInfo.aspect=true;
			            varInfo.id=i;
			            varInfo.Qi=lineTp[i].SQI;
			            if( varInfo.Qi>Qi )
			            {
				            InfoLarge.Add(varInfo);
			            }
			            else
			            {
				            InfoSmall.Add(varInfo);
			            }
		            }
		            if(parts.EndPoint.IsEqual(pt) )
		            {
                        LineInfo varInfo = new LineInfo();

                        varInfo.aspect=false;
			            varInfo.id=i;
			            varInfo.Qi=lineTp[i].EQI;
			            if( varInfo.Qi>Qi )
			            {
				            InfoLarge.Add(varInfo);
			            }
			            else
			            {
				            InfoSmall.Add(varInfo);
			            }
		            }
	            }






	            if( InfoLarge.Count==0 && InfoSmall.Count==0 )
	            {
		            touchId = index;
		            touchAspect = !aspect;

                    result = false;
	            }
	            else if( InfoLarge.Count!=0 )
	            {
                    int infoCount = InfoLarge.Count;
                    int  m = 0;
		            for (int n=1; n<infoCount; n++)
		            {
			            if ( InfoLarge[n].Qi < InfoLarge[m].Qi )
			            {
				            m=n;
			            }
		            }

                    int aa = InfoLarge[m].id;
                    bool bb = InfoLarge[m].aspect;
                    touchId=aa;
		            touchAspect=bb;

                    result = true;
	            }
	            else
	            {
                    int infoCount = InfoSmall.Count;
                    int m = 0;
		            for (int n=1; n<infoCount; n++)
		            {
			            if ( InfoSmall[n].Qi < InfoSmall[m].Qi )
			            {
				            m=n;
			            }
		            }
		            touchId=InfoSmall[m].id;
		            touchAspect=InfoSmall[m].aspect;

                    result = true;
	            }
            }
        
        /// <summary>
            /// 由线创建简单多边形（创建多边形前必须先打断线，清除悬挂线，连接伪结点）
            /// </summary>
            /// <param name="src"></param>
            /// <param name="desOutPgs"></param>
            /// <param name="desInPgs"></param>

        public static void CreatePolylineToPolygon1(List<GeoLineString> src, List<GeoPolygon> desOutPgs, List<GeoPolygon> desInPgs)
							               
            {
                



	            //进行线创建面前必须执行打断，清除悬挂线，连接线和伪结点
                List<GeoLineString> tempSrc1=new List<GeoLineString>();
                //GeometryComputation.LineSplitLine.SplitLine(src,tempSrc1);
                GeometryComputation.LinesSplitLines.SplitLines(src,tempSrc1,false);//打断

          
                List<GeoLineString> tempSrc2=new List<GeoLineString> ();
                GeometryComputation.ClearHangingLine.ClearHangingLines(tempSrc1,tempSrc2);//清楚悬挂线


                List<GeoLineString> tempSrc=new List<GeoLineString> ();
                GeometryComputation.LinkLine.LinkLines1(tempSrc2,tempSrc);//连接伪节点


          
                //计算每条线的起点和终点QI
                //GeoPoint pt=new GeoPoint();
	            int count=tempSrc.Count;
                List<NodeTp> lineTp=new List<NodeTp>(); //线的拓扑信息
                //lineTp.SetSize(count);

	            GeoLineString line=new GeoLineString();
                GeoLineString parts=new GeoLineString();
                
	            for(int i=0; i<count; i++)
	            {
                    //line=tempSrc.GetAt(i);
                    //parts=line; 

                    int c1=0;
                    c1=tempSrc[i].Vertices.Count;
                    NodeTp linetp1 = new NodeTp();
                    lineTp.Add(linetp1);

                    lineTp[i].SQI=NodeTp.CountQI(tempSrc[i].Vertices[0],tempSrc[i].Vertices[1]);
                    lineTp[i].SUsed=false;
                    lineTp[i].EQI=NodeTp.CountQI(tempSrc[i].Vertices[c1-1],tempSrc[i].Vertices[c1-2]);
                    lineTp[i].EUsed=false;



	            }
	            
                List<LineInfo> lineInfoArray1=new List<LineInfo>();
                List<LineInfo> lineInfoArray2=new List<LineInfo>();
                int touchId=0;
                bool touchAspect=false;
                
                for(int i=0; i<count; i++)
	            {


                    lineInfoArray1.Clear();
                    lineInfoArray2.Clear();
                    LineInfo varInfo=new LineInfo();
                    int n2=0; int n1=0;
                    bool con1=false; bool con2=false;
                    bool sValide=true; bool eValide=true;//用来控制正向跟反向搜索是否可以构成多边形
                    varInfo.id=i;
                    varInfo.aspect=true;


	            //	左转算法计算左界址线
		            do
		            {	
	            //		如果是正方向搜索该线，发现该线的终点搜索完成则sValide=FALSE，跳出循环。
			            if( varInfo.aspect==true )//正方向
			            {
				            if( lineTp[varInfo.id].EUsed==true )//终点搜索完成
				            {
					            sValide=false;
					            break;
				            }
			            }
                        else//反方向
			            {
				            if( lineTp[varInfo.id].SUsed==true )//起点搜索完成
				            {
					            sValide=false;
					            break;
				            }
			            }

                       LineInfo v1=new LineInfo();
                       v1.aspect = varInfo.aspect;
                       v1.id = varInfo.id;
                       v1.Qi = varInfo.Qi;
			            lineInfoArray1.Add(v1);

                        
                        bool brt = false;
                        GetLeftTouchLine(tempSrc, lineTp, varInfo.id, varInfo.aspect, ref touchId, ref touchAspect,ref brt);
			            if (!brt)
			            {
				            sValide = false;
			            }

                        //ASSERT( touchId<count );
			            varInfo.id=touchId;
			            varInfo.aspect=touchAspect;//当前线的左界址线的id和方向
		            } while( varInfo.id!=i );//当左界址线为自身时退出

	            //	检查是否有桥界址线，有则删除
		            if( sValide==true )//检查
		            {
			            con1=false;
                        con2=false;	//检查是否有桥界址线
			            if(varInfo.aspect==false)//起始搜索线为桥线
			            {
                            //tempSrc1.Add(tempSrc[i]);
				            tempSrc.RemoveAt(i);
				            lineTp.RemoveAt(i);
				            i--; 
                            count--; 
                            con1=true;
			            }
			            for( n1=1; n1<lineInfoArray1.Count-1; n1++) //检查搜索过程中是否有桥界址线
			            {
				            for( n2=n1+1; n2<lineInfoArray1.Count; n2++)
				            {
					            if ( lineInfoArray1[n1].id==lineInfoArray1[n2].id ) 
					            {
                                    //tempSrc1.Add(tempSrc[lineInfoArray1[n1].id]);
						            tempSrc.RemoveAt(lineInfoArray1[n1].id);
						            lineTp.RemoveAt(lineInfoArray1[n1].id);
						            i--;
                                  count--;
                                  con2=true;
						            break;
					            }
				            }
				            if (con2==true) 
				            {
					            break;
				            }
			            }
			            if ( con2==true || con1==true )  //有桥界址线需要重新搜索
			            {
				            continue;
			            }
		            }

	            //	2.反方向搜索
		            varInfo.id=i;varInfo.aspect=false;
		            do 
		            {
			            if( varInfo.aspect==true )
			            {
				            if( lineTp[varInfo.id].EUsed==true )
				            {
					            eValide=false;
					            break;
				            }
			            }
			            else
			            {
				            if( lineTp[varInfo.id].SUsed==true )
				            {
					            eValide=false;
					            break;
				            }
			            }
                        LineInfo v1 = new LineInfo();
                        v1.aspect = varInfo.aspect;
                        v1.id = varInfo.id;
                        v1.Qi = varInfo.Qi;
			            lineInfoArray2.Add(v1);

			            
                        
                        bool brt = false;
                        GetLeftTouchLine(tempSrc, lineTp, varInfo.id, varInfo.aspect,ref touchId,ref touchAspect,ref brt);
                        if (!brt)
                        {
                            sValide = false;
                        }

                        //ASSERT( touchId<count );
                        varInfo.id = touchId;
                        varInfo.aspect = touchAspect;
		            } while( varInfo.id !=i );

		            if( eValide==true )//检查
		            {
			            con1=false;
                        con2=false;//检查是否有桥界址线
			            if( varInfo.aspect==true)  //被搜索线为桥线
			            {
                            //tempSrc1.Add(tempSrc[i]);
				            tempSrc.RemoveAt(i);
				            lineTp.RemoveAt(i);
				            i--;
                            count--; 
                            con1=true;
			            }
			            for( n1=1; n1<lineInfoArray2.Count-1; n1++) //检查搜索过程中是否有桥界址线
			            {
				            for( n2=n1+1; n2<lineInfoArray2.Count; n2++)
				            {
					            if ( lineInfoArray2[n1].id==lineInfoArray2[n2].id ) 
					            {
                                    //tempSrc1.Add(tempSrc[lineInfoArray2[n1].id]);
						            tempSrc.RemoveAt(lineInfoArray2[n1].id);
						            lineTp.RemoveAt(lineInfoArray2[n1].id);
						            con2=true;	
                                    i--;
                                    count--; 
                                    break;
					            }
				            }
				            if (con2==true) 
				            {
					            break;
				            }
			            }
			            if (con2==true || con1==true  )  //有桥界址线需要重新搜索
			            {
				            continue;
			            }
		            }

	            //	正方向构建多边形
		            if( sValide==true )
		            {
			            int index=0;
                        List<GeoPoint>plgPts=new List<GeoPoint>();
			            for ( n1=0; n1<lineInfoArray1.Count; n1++ )
			            {
				            index=lineInfoArray1[n1].id;
                            //CGeoLineString* line=tempSrc[index];
                            //CGeoLineString* parts=line;

                            line.Vertices.Clear();
                            parts.Vertices.Clear();
                            for(int j=0;j<tempSrc[index].Vertices.Count;j++)
                            {
                                line.AddPoint(tempSrc[index].Vertices[j]);
                                parts.AddPoint(tempSrc[index].Vertices[j]);
                            }


				            if ( lineInfoArray1[n1].aspect == true )
				            {
					            for(n2=0; n2<parts.NumPoints-1; n2++)
					            {
						            plgPts.Add(parts.Vertices[n2]);	
					            }
					            lineTp[index].EUsed=true;//索引为index的线的终点搜索完毕
				            }
				            else
				            {
					            for(n2=parts.NumPoints-1; n2>0; n2--)
					            {
						            plgPts.Add(parts.Vertices[n2]);
					            }
					            lineTp[index].SUsed=true;//索引为index的线的起点搜索完毕
				            }
			            }
                        GeoPoint pt = new GeoPoint();
                        pt.SetXY(plgPts[0].X,plgPts[0].Y); 
                        plgPts.Add(pt);
                        GeoLinearRing plg1=new GeoLinearRing(plgPts);
                        //CGeoPolygon* pg=new CGeoPolygon(plgPts);



                        GeoPolygon pg=new GeoPolygon(plg1);


			            if( IsClockWise(pg.ExteriorRing) ==true )
			            {
				            desInPgs.Add(pg);
			            }
			            else
			            {
				            desOutPgs.Add(pg);
			            }
		            }

		            //反方向构建多边形
		            if( eValide==true )
		            {

                        List< GeoPoint> plgPts=new List<GeoPoint>();
                        int index=0;

			            for ( n1=0; n1<lineInfoArray2.Count; n1++ )
			            {
				            index=lineInfoArray2[n1].id;

                            line.Vertices.Clear();
                            parts.Vertices.Clear();
                            for(int j=0;j<tempSrc[index].Vertices.Count;j++)
                            {
                                line.AddPoint(tempSrc[index].Vertices[j]);
                                parts.AddPoint(tempSrc[index].Vertices[j]);
                            }

				            if ( lineInfoArray2[n1].aspect == true )
				            {
					            for(n2=0; n2<parts.NumPoints-1; n2++)
					            {
						            plgPts.Add(parts.Vertices[n2]);
					            }
					            lineTp[index].EUsed=true;
				            }
				            else
				            {
					            for(n2=parts.NumPoints-1; n2>0; n2--)
					            {
						            plgPts.Add(parts.Vertices[n2]);
					            }
					            lineTp[index].SUsed=true;
				            }
			            }

                        GeoPoint pt = new GeoPoint();
                        pt.SetXY(plgPts[0].X,plgPts[0].Y); 
                        plgPts.Add(pt);
                        GeoLinearRing plg1=new GeoLinearRing();
                        //CGeoPolygon* pg=new CGeoPolygon(plgPts);

                        for (int ii = 0; ii < plgPts.Count; ii++)
                        {
                            plg1.AddPoint(plgPts[ii]);
                        }


                        GeoPolygon pg=new GeoPolygon(plg1);


			            if( IsClockWise(pg.ExteriorRing) ==true )
			            {
				            desInPgs.Add(pg);
			            }
			            else
			            {
				            desOutPgs.Add(pg);
			            }

		            }
	            }//end for

                //将简单多边形的外边界设置为顺时针
                for (int i = 0; i < desOutPgs.Count; i++)
                {
                    if (desOutPgs[i].ExteriorRing.IsCCW())
                    {
                        int aa = desOutPgs[i].ExteriorRing.Vertices.Count;
                        List<GeoPoint> pts = new List<GeoPoint>();
                        for (int j = aa-1; j >-1; j--)
                        {
                            GeoPoint pt = new GeoPoint();
                            pt.SetXY(desOutPgs[i].ExteriorRing.Vertices[j].X, desOutPgs[i].ExteriorRing.Vertices[j].Y);
                            desOutPgs[i].ExteriorRing.Vertices.Add(pt);
                            pts.Add(pt);
                        }
                        for (int j = 0; j < aa ; j++)
                        {
                            desOutPgs[i].ExteriorRing.Vertices.RemoveAt(0);
                        }

                    }
                }

                for (int i = 0; i < desInPgs.Count; i++)
                {

                    
                    if (desInPgs[i].ExteriorRing.IsCCW())
                    {
                        int aa = desInPgs[i].ExteriorRing.NumPoints - 1;
                        for (int j = aa; j >-1; j--)
                        {
                            GeoPoint pt = new GeoPoint();
                            pt.SetXY(desInPgs[i].ExteriorRing.Vertices[j].X, desInPgs[i].ExteriorRing.Vertices[j].Y);
                            desInPgs[i].ExteriorRing.Vertices.Add(pt);
                        }

                        for (int j = 0; j < aa + 1; j++)
                        {
                            desInPgs[i].ExteriorRing.Vertices.RemoveAt(0);
                        }
                    }
                }


            }

        /// <summary>
        /// 两个多边形是否边相接
        /// </summary>
        /// <param name="plg1"></param>
        /// <param name="plg2"></param>
        /// <returns></returns>
        public static bool CheckPolygonLineTouch(GeoPolygon plg1,GeoPolygon plg2)
            {
                for (int i = 0; i < plg1.ExteriorRing.Vertices.Count - 1; i++)
                {
                    
                    for (int j = 0; j < plg2.ExteriorRing.Vertices.Count-1; j++)
                    {
                        if (plg1.ExteriorRing.Vertices[i].IsEqual(plg2.ExteriorRing.Vertices[j]))
                            if (plg1.ExteriorRing.Vertices[i + 1].IsEqual(plg2.ExteriorRing.Vertices[j + 1]))
                                return true;
                       if(plg1.ExteriorRing.Vertices[i].IsEqual(plg2.ExteriorRing.Vertices[j+1]))
                           if(plg1.ExteriorRing.Vertices[i + 1].IsEqual(plg2.ExteriorRing.Vertices[j]))
                               return true;

                        
                    }
                   
                }
                return false;
            }

        /// <summary>
        /// 将线组，组成多边形，可以是复杂多边形
        /// </summary>
        /// <param name="src"></param>
        /// <param name="des"></param>
        public static void CreatePolylineToPolygon(List<GeoLineString> src,List<GeoPolygon> des)
        {

            List<GeoPolygon> outPgs=new List<GeoPolygon>();
            List<GeoPolygon> inPgs=new List<GeoPolygon>();
            CreatePolylineToPolygon1(src,outPgs,inPgs);//将线构建成简单多边形

            inPgs.Clear();
            List<GeoPolygon> plgOut=new List<GeoPolygon>();//记录没有内含多边形的多边形
            List<GeoPolygon> plgPart=new List<GeoPolygon>();//记录被一个多边形包含的多边形
            List<GeoPolygon> plgDel=new List<GeoPolygon>();//记录被两个或以上多边形包含的多边形

            int nOutPlgCount = outPgs.Count;

            for (int i = 0; i < nOutPlgCount; i++)
            {
                int nContainedCount = 0;//用来记录该多边形被包含的次数


                for (int i2 = 0; i2 < nOutPlgCount; i2++)
                {
                    if (i == i2)
                        continue;


                    TpRelateConstants tp1=Toplogical.TpPolygontoPolygon.IsTpWith(outPgs[i],outPgs[i2],false);
                    if (tp1==TpRelateConstants.tpInside)
                    {
                        nContainedCount++;

                        if (2 == nContainedCount)
                            break;
                    }
                }

                GeoPolygon plg = new GeoPolygon(outPgs[i].ExteriorRing,outPgs[i].InteriorRings);
                if (0 == nContainedCount)
                {
                    plgOut.Add(plg);
                }
                else if (1 == nContainedCount)
                {
                    plgPart.Add(plg);
                }
                else if (2 == nContainedCount)
                {
                    plgDel.Add(plg);

                }
            }

            if (plgPart.Count == 0)
            {
                for (int i = 0; i < plgOut.Count; i++)
                {
                    GeoPolygon pg = new GeoPolygon(plgOut[i].ExteriorRing, plgOut[i].InteriorRings);
                    des.Add(pg);
                }
            }

            while (plgPart.Count > 0)
            {
                //将处理一次后的，被内含的多边形汇总，记录
                List<GeoPolygon> plgout1 = new List<GeoPolygon>();
                for (int i = 0; i < plgPart.Count; i++)
                {
                    GeoPolygon pg = new GeoPolygon(plgPart[i].ExteriorRing, plgPart[i].InteriorRings);
                    plgout1.Add(pg);
                }
                for (int i = 0; i < plgDel.Count; i++)
                {
                    GeoPolygon pg = new GeoPolygon(plgDel[i].ExteriorRing, plgDel[i].InteriorRings);
                    plgout1.Add(pg);
                }

                //将岛多边形中线相接的合并
                int nPartCount = plgPart.Count;


                for (int i = 0; i < plgPart.Count; i++)
                {
                    int combinecount = 0;
                    for (int j = 0; j < plgPart.Count; j++)
                    {

                        if (i == j)
                            continue;

                        bool brt = CheckPolygonLineTouch(plgPart[i], plgPart[j]);//判断两个多边形是否相线接
                        //是则合并
                        if (brt)
                        {
                            List<GeoLineString> pls = new List<GeoLineString>();
                            GeoLineString pl1 = new GeoLineString(plgPart[i].ExteriorRing.Vertices);
                            GeoLineString pl2 = new GeoLineString(plgPart[j].ExteriorRing.Vertices);

                            List<GeoPolygon> pg1 = new List<GeoPolygon>();
                            List<GeoPolygon> pg2 = new List<GeoPolygon>();
                            pls.Add(pl1);
                            pls.Add(pl2);

                            CreatePolylineToPolygon1(pls, pg1, pg2);
                            plgPart.RemoveAt(i);
                            plgPart.Insert(i, pg2[0]);
                            plgPart.RemoveAt(j);
                            combinecount++;
                        }
                        if (combinecount > 0)
                            i--;

                    }
                }

                //合并后的岛的边界转为逆时针的线环
                List<GeoLinearRing> prs = new List<GeoLinearRing>();
                for (int i = 0; i < plgPart.Count; i++)
                {
                    GeoLinearRing pr = new GeoLinearRing();
                    int aa = plgPart[i].ExteriorRing.Vertices.Count - 1;
                    for (int j = aa; j > -1; j--)
                    {
                        GeoPoint pt = new GeoPoint();
                        pt.SetXY(plgPart[i].ExteriorRing.Vertices[j].X, plgPart[i].ExteriorRing.Vertices[j].Y);
                        pr.Vertices.Add(pt);
                    }
                    prs.Add(pr);
                }
                //将逆时针的线环，添加到多边形的内岛中形成一个内岛
                for (int j = 0; j < prs.Count; j++)
                {
                    for (int i = 0; i < plgOut.Count; i++)
                    {
                        Toplogical.TpRelateConstants tp1 = Toplogical.TpPolygontoPolygon.IsTpWith(plgPart[j], plgOut[i], false);
                        if (tp1 == TpRelateConstants.tpInside || tp1==TpRelateConstants.tpCoveredBy)
                        {
                            plgOut[i].InteriorRings.Add(prs[j]);
                        }

                    }
                }

                //添加过内岛的多边形添加到des中
                for (int i = 0; i < plgOut.Count; i++)
                {
                    des.Add(plgOut[i]);
                }

                //将被包含的多边形再一次分类，
                plgPart.Clear();
                plgDel.Clear();
                plgOut.Clear();
                for (int i = 0; i < plgout1.Count; i++)
                {
                    int nContainedCount = 0;//用来记录该多边形被包含的次数


                    for (int i2 = 0; i2 < plgout1.Count; i2++)
                    {
                        if (i == i2)
                            continue;


                        TpRelateConstants tp1 = Toplogical.TpPolygontoPolygon.IsTpWith(plgout1[i], plgout1[i2], false);
                        if (tp1 == TpRelateConstants.tpInside)
                        {
                            nContainedCount++;

                            if (2 == nContainedCount)
                                break;
                        }
                    }

                    GeoPolygon plg = new GeoPolygon(plgout1[i].ExteriorRing, plgout1[i].InteriorRings);
                    if (0 == nContainedCount)
                    {
                        plgOut.Add(plg);
                    }
                    else if (1 == nContainedCount)
                    {
                        plgPart.Add(plg);
                    }
                    else if (2 == nContainedCount)
                    {
                        plgDel.Add(plg);

                    }



                }

                if (plgPart.Count == 0)
                {
                    for (int i = 0; i < plgOut.Count; i++)
                    {
                        GeoPolygon pg = new GeoPolygon(plgOut[i].ExteriorRing, plgOut[i].InteriorRings);
                        des.Add(pg);
                    }
                }



            
            }

        }
    }
}
