using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GIS.Toplogical;
using GIS.Layer;
using GIS.Geometries;
using GIS.Increment;


namespace GIS.TreeIndex.GeometryComputation
{
    /// <summary>
    /// 求两面的的拓扑关系，得出结果r[6]
    /// r[0]=fD (A^B),r[1]=fE (A^B) 
    /// r[2]=fD (A\B),r[3]=fE (A\B)
    /// r[4]=fD (B\A),r[5]=fE (B\A)
    /// </summary>
    public class ToplogicalPlgAndPlg
    {
        public static void ToplogicalPlgAPlgB(GeoPolygon plg1, GeoPolygon plg2,ref int[] r)
        {
            List<GeoPolygon> outPgsJ = new List<GeoPolygon>();//记录交面的结果
            List<GeoPolygon> inputPgs = new List<GeoPolygon>();//添加plg1，plg2
            List<GeoPolygon> outPgsC = new List<GeoPolygon>();//记录A\B的结果
            List<GeoPolygon> outPgsBC = new List<GeoPolygon>();//记录B\A的结果


            inputPgs.Add(plg1);
            inputPgs.Add(plg2);
            PolygonIntersectPolygon.PolygonsIntersect(inputPgs, outPgsJ);
            PolygonSubtractPolygon.PolygonSubtract(plg1, plg2, outPgsC);
            PolygonSubtractPolygon.PolygonSubtract(plg2, plg1, outPgsBC);



            //求r[0]=fD (A^B),r[1]=fE (A^B) 

            //求交面的空洞数
            int NumHole=0;
            if(outPgsJ.Count>0)
            {
                for(int i=0;i<outPgsJ.Count;i++)
                {
                    NumHole+=outPgsJ[i].InteriorRings.Count;
                }
            }



            //打断边界线、连接伪节点
            List<GeoLineString> des1 = new List<GeoLineString>();
            List<GeoLineString> src = new List<GeoLineString>();
            List<GeoLineString> des = new List<GeoLineString>();
            src.Add(plg1.ExteriorRing);
            if (plg1.InteriorRings.Count > 0)
            {
                for (int i = 0; i < plg1.InteriorRings.Count; i++)
                {
                    src.Add(plg1.InteriorRings[i]);
                }
            }
            src.Add(plg2.ExteriorRing);
            if (plg2.InteriorRings.Count > 0)
            {
                for (int i = 0; i < plg2.InteriorRings.Count; i++)
                {
                    src.Add(plg2.InteriorRings[i]);
                }
            }

            LinesSplitLines.SplitLines(src, des1, false);
            LinkLine.LinkLines1(des1,des);



            //查找公共边界线,公共点
            List<GeoLineString> sameline = new List<GeoLineString>();
            List<GeoPoint> samept=new List<GeoPoint>();
            for (int i = 0; i < des.Count; i++)
            {
                Toplogical.TpRelateConstants tp1 = Toplogical.TpLinetoPolygon.IsTpWith(des[i],plg1,false);
                if (tp1 == Toplogical.TpRelateConstants.tpOn)
                {
                    Toplogical.TpRelateConstants tp2 = Toplogical.TpLinetoPolygon.IsTpWith(des[i], plg2, false);
                    if(tp2==Toplogical.TpRelateConstants.tpOn)
                    {
                        sameline.Add(des[i]);
                    }
                }
            }
            //公共点
            List<GeoPoint> pts1=new List<GeoPoint>();
            List<GeoPoint> pts2=new List<GeoPoint>();

            for(int i=0;i<plg1.ExteriorRing.NumPoints-1;i++)
            {
                //GeoLineString pl = new GeoLineString(plg1.ExteriorRing.Vertices);
                Toplogical.TpRelateConstants tp=Toplogical.TpPointtoPolygon.IsTpWith(plg1.ExteriorRing.Vertices[i],
                    plg2,false);
                if(tp==Toplogical.TpRelateConstants.tpTouch)
                {
                    samept.Add(plg1.ExteriorRing.Vertices[i]);
                }

            }
            if(plg1.InteriorRings.Count>0)
            {
                for(int j=0;j<plg1.InteriorRings.Count;j++)
                {
                    for(int k=0;k<plg1.InteriorRings[j].NumPoints-1;k++)
                    {
                        
                        Toplogical.TpRelateConstants tp=Toplogical.TpPointtoPolygon.IsTpWith(
                            plg1.InteriorRings[j].Vertices[k],plg2,false);
                        if(tp==Toplogical.TpRelateConstants.tpTouch)
                        {
                            samept.Add(plg1.InteriorRings[j].Vertices[k]);
                        }
                    }
                }
            }


            for(int i=0;i<plg2.ExteriorRing.NumPoints-1;i++)
            {
                
                Toplogical.TpRelateConstants tp=Toplogical.TpPointtoPolygon.IsTpWith(plg2.ExteriorRing.Vertices[i],
                    plg1,false);
                if(tp==Toplogical.TpRelateConstants.tpTouch)
                {
                    samept.Add(plg2.ExteriorRing.Vertices[i]);
                }

            }
            if(plg2.InteriorRings.Count>0)
            {
                for(int j=0;j<plg2.InteriorRings.Count;j++)
                {
                    for(int k=0;k<plg2.InteriorRings[j].NumPoints-1;k++)
                    {
                        
                        Toplogical.TpRelateConstants tp=Toplogical.TpPointtoPolygon.IsTpWith(
                            plg2.InteriorRings[j].Vertices[k],plg1,false);
                        if(tp==Toplogical.TpRelateConstants.tpTouch)
                        {
                            samept.Add(plg2.InteriorRings[j].Vertices[k]);
                        }
                    }
                }
            }


            //去除重复的点、线、以及去除在面上的点线、在线上的点
            DelrepeatLine(sameline);
            DelrepeatPoint(samept);

            for (int i = 0; i < outPgsJ.Count;i++ )
            {
                for (int j = 0; j < samept.Count; j++)
                {
                    Toplogical.TpRelateConstants tp = Toplogical.TpPointtoPolygon.IsTpWith(
                        samept[j], outPgsJ[i],false);
                    if (!(tp == Toplogical.TpRelateConstants.tpDisjoint))
                    {
                        samept.RemoveAt(j);
                        j--;
                    }
                }

                for (int j = 0; j < sameline.Count; j++)
                {
                    Toplogical.TpRelateConstants tp = Toplogical.TpLinetoPolygon.IsTpWith(
                        sameline[j], outPgsJ[i],false);
                    if (!((tp == Toplogical.TpRelateConstants.tpDisjoint) || (tp == Toplogical.TpRelateConstants.tpTouch)))
                    {
                        sameline.RemoveAt(j);
                        j--;

                    }
                    
                }

            }
            for (int i = 0; i < sameline.Count; i++)
            {
                for (int j = 0; j < samept.Count; j++)
                {
                    Toplogical.TpRelateConstants tp = Toplogical.TpPointtoLines.IsTpWith(
                        samept[j],sameline[i],false);
                    if (!(tp == Toplogical.TpRelateConstants.tpDisjoint) )
                    {
                        samept.RemoveAt(j);
                        j--;

                    }

                }
            }


            int pgandpg = 0;
            int lineandpg = 0;
            //求点、线、面的连通数
            for (int i = 0; i < outPgsJ.Count; i++)
            {
                if (outPgsJ.Count > 1)
                {
                    for (int j = i + 1; j < outPgsJ.Count; j++)
                    {
                        Toplogical.TpRelateConstants tp = Toplogical.TpPolygontoPolygon.IsTpWith(
                            outPgsJ[i], outPgsJ[j], false);
                        if (tp == Toplogical.TpRelateConstants.tpTouch)
                            pgandpg++;
                    }
                }
                for(int j=0;j<sameline.Count;j++)
                {
                    Toplogical.TpRelateConstants tp = Toplogical.TpLinetoPolygon.IsTpWith(
                        sameline[j], outPgsJ[i],false);
                        if(tp==Toplogical.TpRelateConstants.tpTouch)
                            lineandpg++;
                }

            }


            //分列几种情况，得出r【0】r【1】
            if ((outPgsJ.Count == 0) && (sameline.Count == 0) && (samept.Count == 0))
            {
                r[0] = -1;
                r[1] = -1;
            }

            if((samept.Count>0)&&(sameline.Count==0)&&(outPgsJ.Count==0))
            {
                r[0]=0;
                r[1]=samept.Count;
            }
            if((samept.Count==0)&&(sameline.Count>0)&&(outPgsJ.Count==0))
            {
                r[0]=1;
                r[1]=sameline.Count;
            }
            if((samept.Count==0)&&(sameline.Count==0)&&(outPgsJ.Count>0))
            {
                r[0]=2;
                r[1]=outPgsJ.Count-NumHole-pgandpg;
            }
            if((samept.Count>0)&&(sameline.Count>0)&&(outPgsJ.Count==0))
            {
                r[0]=3;
                r[1]=samept.Count+sameline.Count;
            }
            if((samept.Count>0)&&(sameline.Count==0)&&(outPgsJ.Count>0))
            {
                r[0]=4;
                r[1]=samept.Count+outPgsJ.Count-NumHole-pgandpg;
            }
            if((samept.Count==0)&&(sameline.Count>0)&&(outPgsJ.Count>0))
            {
                r[0]=5;
                r[1]=sameline.Count+outPgsJ.Count-NumHole-pgandpg-lineandpg;
            }
            if((samept.Count>0)&&(sameline.Count>0)&&(outPgsJ.Count>0))
            {
                r[0]=6;
                r[1]=samept.Count+sameline.Count+outPgsJ.Count-NumHole-pgandpg-lineandpg;
            }


            //r[2]=fD (A\B),r[3]=fE (A\B)

            //求A\B面的空洞数
            NumHole=0;
            if(outPgsC.Count>0)
            {
                for(int i=0;i<outPgsC.Count;i++)
                {
                    NumHole+=outPgsC[i].InteriorRings.Count;
                }
            }
            //求面的连通数
            pgandpg=0;
            for (int i = 0; i < outPgsC.Count; i++)
            {
                if (outPgsC.Count > 1)
                {
                    for (int j = i + 1; j < outPgsC.Count; j++)
                    {
                        Toplogical.TpRelateConstants tp = Toplogical.TpPolygontoPolygon.IsTpWith(
                            outPgsC[i], outPgsC[j], false);
                        if (tp == Toplogical.TpRelateConstants.tpTouch)
                            pgandpg++;
                    }
                }
            }
            if(outPgsC.Count==0)
            {
                r[2]=-1;
                r[3]=-1;
            }
            else
            {
                r[2]=2;
                r[3]=outPgsC.Count-NumHole-pgandpg;
            }

            //r[4]=fD (B\A),r[5]=fE (B\A)
            //求B\A面的空洞数
            NumHole=0;
            if(outPgsBC.Count>0)
            {
                for(int i=0;i<outPgsBC.Count;i++)
                {
                    NumHole+=outPgsBC[i].InteriorRings.Count;
                }
            }
            //求面的连通数
            pgandpg = 0;
            for (int i = 0; i < outPgsBC.Count; i++)
            {
                if (outPgsBC.Count > 1)
                {
                    for (int j = i + 1; j < outPgsBC.Count; j++)
                    {
                        Toplogical.TpRelateConstants tp = Toplogical.TpPolygontoPolygon.IsTpWith(
                            outPgsBC[i], outPgsBC[j], false);
                        if (tp == Toplogical.TpRelateConstants.tpTouch)
                            pgandpg++;
                    }
                }
            }
            if(outPgsBC.Count==0)
            {
                r[4]=-1;
                r[5]=-1;
            }
            else
            {
                r[4]=2;
                r[5]=outPgsBC.Count-NumHole-pgandpg;
            }

        }

        /// <summary>
        /// 去掉重复线
        /// </summary>
        public static void DelrepeatLine(List<GeoLineString> src)
        {
            //des.Clear();
            for (int i = 0; i < src.Count; i++)
            {
                for (int j = i + 1; j < src.Count; j++)
                {
                    if (src[i].NumPoints == src[j].NumPoints)
                    {
                        

                        int k = 0;
                        int n = 0;
                        for (k = 0; k < src[j].NumPoints; k++)
                        {
                            
                            if (!(src[i].Vertices[k].IsEqual(src[j].Vertices[k])))
                            {
                                break;
                            }
                        }
                        if (k == src[j].NumPoints)
                        {
                            src.RemoveAt(j);
                            j--;
                        }
                        else
                        {
                            for (n = 0; n < src[j].NumPoints; n++)
                            {

                                if (!(src[i].Vertices[n].IsEqual(src[j].Vertices[src[j].NumPoints - 1 - n])))
                                {
                                    break;
                                }
                            }
                            if (n == src[j].NumPoints)
                            {
                                src.RemoveAt(j);
                                j--;
                            }

                        }

                    }
                }
            }

        }

        /// <summary>
        /// 去掉重复点
        /// </summary>
        public static void DelrepeatPoint(List<GeoPoint> src)
        {
            for (int i = 0; i < src.Count; i++)
            {
                for (int j = i+1; j < src.Count; j++)
                {
                    if (src[i].IsEqual(src[j]))
                    {
                        src.RemoveAt(j);
                        j--;
                    }
                }
            }
        }



    }
}
