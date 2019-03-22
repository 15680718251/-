using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GIS.Toplogical;
using GIS.Layer;
using GIS.Geometries;
using GIS.Increment;
using GIS.Converters.WellKnownText;

using System.Windows.Forms;
using System.Drawing;
using GIS.Map;
using GIS.Utilities;
using GIS.TreeIndex.Tool;
using GIS.SpatialRelation;
using GIS.TreeIndex.GeometryComputation;
using System.IO;
using System.Threading;
using OSGeo.OGR;


namespace GIS.TreeIndex.GeometryComputation
{
    class PolygonSubtractPolygon
    {
        /// <summary>
        /// pg1差pg2,plg记录结果
        /// </summary>
        public static void PolygonSubtract(GeoPolygon pg1,GeoPolygon pg2,List<GeoPolygon>plg)
        {
            List<GeoPolygon> outPgs = new List<GeoPolygon>();//记录新构成的内部多边形
            List<GeoPolygon> inPgs = new List<GeoPolygon>();//记录新构成的外部多边形
            List<GeoLineString> src = new List<GeoLineString>();//记录选择面得边界线

            //if((Toplogical.TpPolygontoPolygon.IsTpWith(pg1, pg2,false) == Toplogical.TpRelateConstants.tpDisjoint)||
            //(Toplogical.TpPolygontoPolygon.IsTpWith(pg1, pg2, false) == Toplogical.TpRelateConstants.tpTouch))
               
            //{
            //    plg.Add(pg1);
            //}

            //else 
            //{


                //将被选择的面的边界线提取并记录到pls

                GeoLineString pl1 = new GeoLineString(pg1.ExteriorRing.Vertices);
                src.Add(pl1);
                for (int j = 0; j < pg1.InteriorRings.Count; j++)
                {
                    GeoLineString pl = new GeoLineString(pg1.InteriorRings[j].Vertices);
                    src.Add(pl);
                }

                GeoLineString pl2 = new GeoLineString(pg2.ExteriorRing.Vertices);
                src.Add(pl2);
                for (int j = 0; j < pg2.InteriorRings.Count; j++)
                {
                    GeoLineString pl = new GeoLineString(pg2.InteriorRings[j].Vertices);
                    src.Add(pl);
                }

                //将边界线重新构成多边形，引用的是GeometryComputation.CreatePolygon.CreatePolylineToPolygon
                GeometryComputation.CreatePolygon.CreatePolylineToPolygon(src, outPgs);

                //将outPgs中的多边形与被选择的面做拓扑运算，把在pg1内且不在pg2内的多边形记录到desout
                List<GeoPolygon> desout = new List<GeoPolygon>();
                for (int i = 0; i < outPgs.Count; i++)
                {

                    Toplogical.TpRelateConstants tp1 = Toplogical.TpPolygontoPolygon.IsTpWith(outPgs[i], pg1, false);
                    if (tp1 == Toplogical.TpRelateConstants.tpInside || tp1 == Toplogical.TpRelateConstants.tpCoveredBy
                        ||tp1==Toplogical.TpRelateConstants.tpEqual)
                    {
                        Toplogical.TpRelateConstants tp2 = Toplogical.TpPolygontoPolygon.IsTpWith(outPgs[i], pg2, false);
                        if (!(tp2 == Toplogical.TpRelateConstants.tpInside || tp2 == Toplogical.TpRelateConstants.tpCoveredBy
                            || tp2 == Toplogical.TpRelateConstants.tpEqual))
                        {
                            GeoPolygon pg = new GeoPolygon(outPgs[i].ExteriorRing, outPgs[i].InteriorRings);
                            desout.Add(pg);

                        }

                    }

                }

                //判断desout中的多边形是否边界相接，如果相接，就把两个多边形合并
                if (desout.Count > 1)
                {
                    for (int i = 0; i < desout.Count; i++)
                    {
                        for (int j = i+1; j < desout.Count; j++)
                        {

                            //if (i == j)
                            //    continue;

                            bool brt = GeometryComputation.CreatePolygon.CheckPolygonLineTouch(desout[i], desout[j]);//判断两个多边形是否相线接
                            //是则合并
                            if (brt)
                            {
                                List<GeoLineString> pls = new List<GeoLineString>();
                                GeoLineString pll1 = new GeoLineString(desout[i].ExteriorRing.Vertices);
                                GeoLineString pll2 = new GeoLineString(desout[j].ExteriorRing.Vertices);

                                List<GeoPolygon> pgg1 = new List<GeoPolygon>();
                                List<GeoPolygon> pgg2 = new List<GeoPolygon>();
                                pls.Add(pll1);
                                pls.Add(pll2);

                                GeometryComputation.CreatePolygon.CreatePolylineToPolygon1(pls, pgg1, pgg2);
                                GeoPolygon pg = new GeoPolygon(pgg2[0].ExteriorRing);
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
                }
                //记录到plg

                for (int i = 0; i < desout.Count; i++)
                {
                    GeoPolygon pg = new GeoPolygon(desout[i].ExteriorRing, desout[i].InteriorRings);
                    plg.Add(pg);
                }

            //}
        }
        public static void PolygonDifference(GeoPolygon pg1, GeoPolygon pg2, List<GeoPolygon> plg)
        {
            OSGeo.OGR.Ogr.RegisterAll();
            string b1 = GIS.Converters.WellKnownText.GeometryToWKT.Write(pg1 as Geometries.Geometry);
            string b2 = GIS.Converters.WellKnownText.GeometryToWKT.Write(pg2 as Geometries.Geometry);
            OSGeo.OGR.Geometry p1 = OSGeo.OGR.Geometry.CreateFromWkt(b1);
            OSGeo.OGR.Geometry p2 = OSGeo.OGR.Geometry.CreateFromWkt(b2);
            OSGeo.OGR.Geometry p3 = p1.Difference(p2);
            List<OSGeo.OGR.Geometry> pa = new List<OSGeo.OGR.Geometry>();
            ProcessGeometryToList(p3, pa);
            for (int i = 0; i < pa.Count; i++)
            {
                string wkt = null;
                try
                {
                    pa[i].ExportToWkt(out wkt);
                }
                catch (Exception e)
                {
                    MessageBox.Show("多边形转wkt出错！" + e.Message);
                }
                GeoPolygon g = GIS.Converters.WellKnownText.GeometryFromWKT.Parse(wkt) as GeoPolygon;
                plg.Add(g);
            }

            ////转wkb格式不成功，原因：无法正确取WbkSize，返回值总为0
            //byte[] b1 = GIS.Converters.WellKnownBinary.GeometryToWKB.Write(pg1 as Geometries.Geometry);
            //OSGeo.OGR.Geometry p1 = OSGeo.OGR.Geometry.CreateFromWkb(b1);
            //byte[] b2 = GIS.Converters.WellKnownBinary.GeometryToWKB.Write(pg2 as Geometries.Geometry);
            //OSGeo.OGR.Geometry p2 = OSGeo.OGR.Geometry.CreateFromWkb(b2);
            //OSGeo.OGR.Geometry p3 = p1.Difference(p2);
            //List<OSGeo.OGR.Geometry> pa = new List<OSGeo.OGR.Geometry>();
            //ProcessGeometryToList(p3, pa);
            //for (int i = 0; i < pa.Count; i++)
            //{
            //    int wkbSize = pa[i].WkbSize();
            //    double a1=pa[i].GetArea();
            //    if (wkbSize == 0)
            //        continue;
            //    byte[] wkb = new byte[wkbSize];
            //    try
            //    {
            //        pa[i].ExportToWkb(wkb);
            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show("多边形转wkb出错！" + e.Message);
            //    }
            //    GeoPolygon g = GIS.Converters.WellKnownBinary.GeometryFromWKB.Parse(wkb) as GeoPolygon;
            //    plg.Add(g);
            //}
        }

        /// <summary>
        /// 将Geometry多多边形转换成单多边形List表
        /// </summary>
        /// <param name="gg"></param>
        /// <param name="plgArr"></param>
        private static void ProcessGeometryToList(OSGeo.OGR.Geometry gg, List<OSGeo.OGR.Geometry> plgArr)
        {
            if (gg.GetArea() < 10.0)
                return;
            if (gg.GetGeometryType() == wkbGeometryType.wkbGeometryCollection)  //如果是集合，则转多多边形
            {
                OSGeo.OGR.Geometry temp = new OSGeo.OGR.Geometry(wkbGeometryType.wkbMultiPolygon);
                int n = gg.GetGeometryCount();
                for (int k = 0; k < n; k++)
                {
                    OSGeo.OGR.Geometry gt = new OSGeo.OGR.Geometry(wkbGeometryType.wkbPolygon);
                    gt = gg.GetGeometryRef(k);
                    if (gt.GetArea() > 10.0)
                    {
                        temp.AddGeometry(gt);
                    }
                }
                gg = temp;
            }
            if (gg.GetGeometryType() == wkbGeometryType.wkbMultiPolygon)
            {
                int plgCount = gg.GetGeometryCount();
                for (int i = 0; i < plgCount; i++)
                {
                    OSGeo.OGR.Geometry tmp = new OSGeo.OGR.Geometry(wkbGeometryType.wkbPolygon);
                    tmp = gg.GetGeometryRef(i);
                    if (tmp.GetArea() > 10.0)
                        plgArr.Add(tmp);
                }
                return;
            }
            if (gg.GetGeometryType() == wkbGeometryType.wkbPolygon)
                plgArr.Add(gg);
        }
    }
}
