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
    public class PolygonCombinePolyg
    {
        /// <summary>
        /// src是要求并的多边形，des是结果
        /// </summary>
        /// <param name="src"></param>
        /// <param name="des"></param>
        public static void PolygonCombine(List<GeoPolygon> src,List<GeoPolygon> des)
        {
            List<GeoPolygon> outPgs = new List<GeoPolygon>();//记录新构成的内部多边形
            List<GeoPolygon> inPgs = new List<GeoPolygon>();//记录新构成的外部多边形
            List<GeoLineString> pls = new List<GeoLineString>();//记录选择面得边界线

            //将被选择的面的边界线提取并记录到pls
            for (int i = 0; i < src.Count; i++)
            {
                GeoLineString pl = new GeoLineString(src[i].ExteriorRing.Vertices);
                pls.Add(pl);
                for (int j = 0; j < src[i].InteriorRings.Count; j++)
                {
                    GeoLineString pll = new GeoLineString(src[i].InteriorRings[j].Vertices);
                    pls.Add(pll);
                }
            }

            //将边界线重新构成多边形，引用的是GeometryComputation.CreatePolygon.CreatePolylineToPolygon1
            GeometryComputation.CreatePolygon.CreatePolylineToPolygon1(pls,outPgs,inPgs);

            //将outPgs中的多边形与被选择的面做拓扑运算，与所有面都相离或相接的多边形记录到desout中，作为内岛
            List<GeoPolygon> desout = new List<GeoPolygon>();
            for (int i = 0; i < outPgs.Count; i++)
            {
                bool brt = true;
                for (int j = 0; j < src.Count; j++)
                {
                    Toplogical.TpRelateConstants tp1 = Toplogical.TpPolygontoPolygon.IsTpWith(outPgs[i], src[j], false);
                    if (!(tp1 == Toplogical.TpRelateConstants.tpTouch || tp1 == Toplogical.TpRelateConstants.tpDisjoint))
                    {
                        brt = false;
                    }
                }
                if (brt)
                {
                    GeoPolygon pg = new GeoPolygon(outPgs[i].ExteriorRing);
                    desout.Add(pg);
                }
            }

            //判断desout中的内岛是否边界相接，如果相接，就把两个岛合并
            for (int i = 0; i < desout.Count; i++)
            {
                for (int j = 0; j < desout.Count; j++)
                {

                    if (i == j)
                        continue;

                    bool brt =GeometryComputation.CreatePolygon.CheckPolygonLineTouch(desout[i], desout[j]);//判断两个多边形是否相线接
                    //是则合并
                    if (brt)
                    {
                        List<GeoLineString> pls1 = new List<GeoLineString>();
                        GeoLineString pl1 = new GeoLineString(desout[i].ExteriorRing.Vertices);
                        GeoLineString pl2 = new GeoLineString(desout[j].ExteriorRing.Vertices);

                        List<GeoPolygon> pg1 = new List<GeoPolygon>();
                        List<GeoPolygon> pg2 = new List<GeoPolygon>();
                        pls.Add(pl1);
                        pls.Add(pl2);

                        GeometryComputation.CreatePolygon.CreatePolylineToPolygon1(pls1, pg1, pg2);
                        desout.RemoveAt(i);
                        desout.Insert(i, pg2[0]);
                        desout.RemoveAt(j);

                    }

                }
            }

            //合并后的岛的边界转为逆时针的线环
            List<GeoLinearRing> prs = new List<GeoLinearRing>();
            for (int i = 0; i < desout.Count; i++)
            {
                GeoLinearRing pr = new GeoLinearRing();
                int aa = desout[i].ExteriorRing.Vertices.Count - 1;
                for (int j = aa; j > -1; j--)
                {
                    GeoPoint pt = new GeoPoint();
                    pt.SetXY(desout[i].ExteriorRing.Vertices[j].X, desout[i].ExteriorRing.Vertices[j].Y);
                    //pt.SetSign(plgPart[i].ExteriorRing.Vertices[j].sign);
                    pr.Vertices.Add(pt);
                }
                prs.Add(pr);
            }
            //将逆时针的线环，添加到多边形的内岛组中形成一个内岛
            for (int j = 0; j < prs.Count; j++)
            {
                for (int i = 0; i < inPgs.Count; i++)
                {
                    Toplogical.TpRelateConstants tp1 = Toplogical.TpPolygontoPolygon.IsTpWith(desout[j], inPgs[i], false);
                    if (tp1 == TpRelateConstants.tpInside)
                    {
                        inPgs[i].InteriorRings.Add(prs[j]);
                    }
                }
            }

            //记录到des
            for (int i = 0; i < inPgs.Count; i++)
            {
                GeoPolygon pg = new GeoPolygon(inPgs[i].ExteriorRing,inPgs[i].InteriorRings);
                des.Add(pg);
            }
        }

    }
}
