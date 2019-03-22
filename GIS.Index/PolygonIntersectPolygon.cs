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
    public class PolygonIntersectPolygon
    {
        /// <summary>
        /// 多个面相交成新的面
        /// </summary>
        /// <param name="plgs1"></param>
        /// <param name="plgs2"></param>
        public static void PolygonsIntersect(List<GeoPolygon> plgs1, List<GeoPolygon> plgs2)
        {

            List<GeoPolygon> outPgs = new List<GeoPolygon>();//记录新构成的内部多边形
            List<GeoPolygon> inPgs = new List<GeoPolygon>();//记录新构成的外部多边形
            List<GeoLineString> src = new List<GeoLineString>();//记录选择面得边界线

            //if ((Toplogical.TpPolygontoPolygon.IsTpWith(plgs1[0], plgs1[1], false) == Toplogical.TpRelateConstants.tpCoveredBy)||
            //    (Toplogical.TpPolygontoPolygon.IsTpWith(plgs1[0], plgs1[1], false)==Toplogical.TpRelateConstants.tpInside))
            //{
            //    plgs2.Add(plgs1[0]);
            //}
            //else if ((Toplogical.TpPolygontoPolygon.IsTpWith(plgs1[0], plgs1[1], false) == Toplogical.TpRelateConstants.tpCovers) ||
            //    (Toplogical.TpPolygontoPolygon.IsTpWith(plgs1[0], plgs1[1], false) == Toplogical.TpRelateConstants.tpContains))
            //{
            //    plgs2.Add(plgs1[1]);
            //}
            //else
            //{



                //将被选择的面的边界线提取并记录到pls
                for (int i = 0; i < plgs1.Count; i++)
                {
                    GeoLineString pl = new GeoLineString(plgs1[i].ExteriorRing.Vertices);
                    src.Add(pl);
                    for (int j = 0; j < plgs1[i].InteriorRings.Count; j++)
                    {
                        GeoLineString pll = new GeoLineString(plgs1[i].InteriorRings[j].Vertices);
                        src.Add(pll);
                    }
                }

                //将边界线重新构成多边形，引用的是GeometryComputation.CreatePolygon.CreatePolylineToPolygon
                GeometryComputation.CreatePolygon.CreatePolylineToPolygon(src, outPgs);

                //将outPgs中的多边形与被选择的面做拓扑运算，与所有面都包含或被覆盖的多边形记录到desout中，作为相交运算结果
                List<GeoPolygon> desout = new List<GeoPolygon>();
                for (int i = 0; i < outPgs.Count; i++)
                {
                    bool brt = true;
                    for (int j = 0; j < plgs1.Count; j++)
                    {
                        Toplogical.TpRelateConstants tp1 = Toplogical.TpPolygontoPolygon.IsTpWith(outPgs[i], plgs1[j], false);
                        if (!(tp1 == Toplogical.TpRelateConstants.tpInside || tp1 == Toplogical.TpRelateConstants.tpCoveredBy
                            || tp1 == Toplogical.TpRelateConstants.tpEqual))
                        {
                            brt = false;
                        }
                    }
                    if (brt)
                    {
                        GeoPolygon pg = new GeoPolygon(outPgs[i].ExteriorRing, outPgs[i].InteriorRings);
                        desout.Add(pg);
                    }
                }

                //判断desout中的多边形是否边界相接，如果相接，就把两个多边形合并
                for (int i = 0; i < desout.Count; i++)
                {
                    for (int j = 0; j < desout.Count; j++)
                    {

                        if (i == j)
                            continue;

                        bool brt = GeometryComputation.CreatePolygon.CheckPolygonLineTouch(desout[i], desout[j]);//判断两个多边形是否相线接
                        //是则合并
                        if (brt)
                        {
                            List<GeoLineString> pls = new List<GeoLineString>();
                            GeoLineString pl1 = new GeoLineString(desout[i].ExteriorRing.Vertices);
                            GeoLineString pl2 = new GeoLineString(desout[j].ExteriorRing.Vertices);

                            List<GeoPolygon> pg1 = new List<GeoPolygon>();
                            List<GeoPolygon> pg2 = new List<GeoPolygon>();
                            pls.Add(pl1);
                            pls.Add(pl2);

                            GeometryComputation.CreatePolygon.CreatePolylineToPolygon1(pls, pg1, pg2);
                            GeoPolygon pg = new GeoPolygon(pg2[0].ExteriorRing);
                            for (int kk = 0; kk < desout[i].InteriorRings.Count; kk++)
                            {
                                pg.InteriorRings.Add(desout[i].InteriorRings[kk]);
                            }
                            for (int kk = 0; kk < desout[j].InteriorRings.Count; kk++)
                            {
                                pg.InteriorRings.Add(desout[j].InteriorRings[kk]);
                            }
                            desout.RemoveAt(i);
                            desout.Insert(i, pg);
                            desout.RemoveAt(j);


                        }

                    }
                }

                //记录到des

                for (int i = 0; i < desout.Count; i++)
                {
                    GeoPolygon pg = new GeoPolygon(desout[i].ExteriorRing, desout[i].InteriorRings);
                    plgs2.Add(pg);
                }
            //}
            
        }


    }
}
