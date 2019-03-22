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
    public class ToplogicalplgAndLine
    {
        /// <summary>
        /// 求两面的的拓扑关系，得出结果r[6]
        /// A：面B:线
        /// r[0]=fD (A^B),r[1]=fE (A^B) 
        /// r[2]=fD (A\B),r[3]=fE (A\B)
        /// r[4]=fD (B\A),r[5]=fE (B\A)
        /// </summary>
        /// <param name="plg"></param>
        /// <param name="pl"></param>
        /// <param name="r"></param>
        public static void ToplogicalPlgPl(GeoPolygon plg, GeoLineString pl, ref int[] r)
        {
            List<GeoLineString> src = new List<GeoLineString>();
            List<GeoLineString> des = new List<GeoLineString>();
            List<GeoLineString> des1 = new List<GeoLineString>();

            List<GeoPoint> samept = new List<GeoPoint>();//交的点
            List<GeoLineString> sameline = new List<GeoLineString>();//交的线

            //打断线与面的边界线
            src.Add(plg.ExteriorRing);
            if (plg.InteriorRings.Count > 0)
            {
                for (int i = 0; i < plg.InteriorRings.Count; i++)
                {
                    src.Add(plg.InteriorRings[i]);
                }
            }
            src.Add(pl);

            LinesSplitLines.SplitLines(src, des1, false);
            LinkLine.LinkLines1(des1, des);

            //求交
            for (int i = 0; i < des.Count; i++)
            {
                Toplogical.TpRelateConstants tp1 = Toplogical.TpLinetoLine.IsTpWith(des[i],pl,false);
                if (tp1 == Toplogical.TpRelateConstants.tpInside || tp1 == Toplogical.TpRelateConstants.tpCoveredBy
                    || tp1 == Toplogical.TpRelateConstants.tpEqual)
                {
                    Toplogical.TpRelateConstants tp2 = Toplogical.TpLinetoPolygon.IsTpWith(des[i],plg,false);
                    if (tp2 == Toplogical.TpRelateConstants.tpOn|| tp2 == Toplogical.TpRelateConstants.tpCoveredBy
                        || tp2==Toplogical.TpRelateConstants.tpInside)
                    {
                        sameline.Add(des[i]);
                    }
                    if (tp2 == Toplogical.TpRelateConstants.tpTouch )
                    {
                        for (int j = 0; j < des[i].Vertices.Count; j++)
                        {
                            Toplogical.TpRelateConstants tp3 = Toplogical.TpPointtoPolygon.IsTpWith(
                                des[i].Vertices[j],plg,false);
                            if (tp3 == Toplogical.TpRelateConstants.tpTouch)
                            {
                                samept.Add(des[i].Vertices[j]);
                            }
                        }
                    }
                }


            }

            //去重
            DelrepeatLine(sameline);
            DelrepeatPoint(samept);
            for (int i = 0; i < sameline.Count; i++)
            {
                for (int j = 0; j < samept.Count; j++)
                {
                    Toplogical.TpRelateConstants tp = Toplogical.TpPointtoLines.IsTpWith(
                        samept[j], sameline[i], false);
                    if (!(tp == Toplogical.TpRelateConstants.tpDisjoint))
                    {
                        samept.RemoveAt(j);
                        j--;

                    }

                }
            }


            //求r[0]=fD (A^B),r[1]=fE (A^B) 
            if (samept.Count == 0 && sameline.Count == 0)
            {
                r[0] = -1;
                r[1] = -1;
            }
            if(samept.Count>0 && sameline.Count==0)
            {
                r[0] = 0;
                r[1] = samept.Count;
            }
            if (samept.Count == 0 && sameline.Count > 0)
            {
                r[0] = 1;
                r[1] = sameline.Count;
            }
            if (samept.Count > 0 && sameline.Count > 0)
            {
                r[0] = 9;
                r[1] = sameline.Count+samept.Count;
            }
        
            //求面/线
            List<GeoPolygon> plgSpl = new List<GeoPolygon>();
            int sumhole = 0;
            GeometryComputation.CreatePolygon.CreatePolylineToPolygon(src,plgSpl);

            for (int i = 0; i < plgSpl.Count; i++)
            {
                Toplogical.TpRelateConstants tp1 = Toplogical.TpPolygontoPolygon.IsTpWith(plgSpl[i],plg,false);
                if(tp1== Toplogical.TpRelateConstants.tpInside || tp1 == Toplogical.TpRelateConstants.tpCoveredBy
                        ||tp1==Toplogical.TpRelateConstants.tpEqual)
                {
                
                    sumhole += plgSpl[i].InteriorRings.Count;
                    for (int j = 0; j < des.Count; j++)
                    {
                        Toplogical.TpRelateConstants tp = Toplogical.TpLinetoPolygon.IsTpWith(des[j],plgSpl[i],false);
                        if (tp == Toplogical.TpRelateConstants.tpInside)
                            sumhole++;
                    }
                }
                else
                {
                    plgSpl.RemoveAt(i);
                    i--;
                }
            }

            r[2] = 2;
            r[3] = plgSpl.Count - sumhole;

            //求线/面

            List<GeoLineString> plspg = new List<GeoLineString>();
            for (int i = 0; i < des.Count; i++)
            {
                Toplogical.TpRelateConstants tp1 = Toplogical.TpLinetoLine.IsTpWith(des[i], pl, false);
                if (tp1 == Toplogical.TpRelateConstants.tpInside || tp1 == Toplogical.TpRelateConstants.tpCoveredBy
                    || tp1 == Toplogical.TpRelateConstants.tpEqual)
                {
                    Toplogical.TpRelateConstants tp = Toplogical.TpLinetoPolygon.IsTpWith(des[i], plg, false);
                    if (tp == Toplogical.TpRelateConstants.tpTouch || tp == Toplogical.TpRelateConstants.tpDisjoint)
                    {
                        plspg.Add(des[i]);
                    }
                }
            }
            if (plspg.Count > 0)
            {
                r[4] = 1;
                r[5] = plspg.Count;
            }
            else
            {
                r[4] = -1;
                r[5] = -1;
            }

        }


        /// <summary>
        /// 求线面的的拓扑关系，得出结果r[6]
        /// A：线B:面
        /// r[0]=fD (A^B),r[1]=fE (A^B) 
        /// r[2]=fD (A\B),r[3]=fE (A\B)
        /// r[4]=fD (B\A),r[5]=fE (B\A)
        /// </summary>
        /// <param name="plg"></param>
        /// <param name="pl"></param>
        /// <param name="r"></param>
        public static void ToplogicalPlPlg(GeoLineString pl, GeoPolygon plg, ref int[] r)
        {
            int[] r1 = { 0,0,0,0,0,0};
            ToplogicalPlgPl(plg,pl,ref r1);
            int t = 0;
            r[0]=r1[0];
            r[1]=r1[1];
            r[2]=r1[4];
            r[3]=r1[5];
            r[4]=r1[2];
            r[5]=r1[3];

        }


        /// <summary>
        /// 去掉重复线
        /// </summary>
        private static void DelrepeatLine(List<GeoLineString> src)
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
        private static void DelrepeatPoint(List<GeoPoint> src)
        {
            for (int i = 0; i < src.Count; i++)
            {
                for (int j = i + 1; j < src.Count; j++)
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
