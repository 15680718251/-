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
    public class SplitPolygon
    {
        /// <summary>
        /// 几条线分割几个面,结果记录到plg2
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="polygons"></param>
        /// <param name="breport"></param>
        public static void PolygonSplitByLine(List<GeoLineString> lines, List<GeoPolygon> plg1, List<GeoPolygon>plg2 )
        {
            List<GeoPolygon> outPgs = new List<GeoPolygon>();//记录新构成的内部多边形
            List<GeoPolygon> inPgs = new List<GeoPolygon>();//记录新构成的外部多边形
            List<GeoLineString> src1 = new List<GeoLineString>();//记录选择面得边界线
            

            //将被选择的面的边界线提取并记录到src1
            for (int i = 0; i < plg1.Count; i++)
            {
                GeoLineString pl = new GeoLineString(plg1[i].ExteriorRing.Vertices);
                src1.Add(pl);
                for (int j = 0; j < plg1[i].InteriorRings.Count; j++)
                {
                    GeoLineString pll = new GeoLineString(plg1[i].InteriorRings[j].Vertices);
                    src1.Add(pll);
                }
            }
            //将lines中的先记录到src1
            for (int i = 0; i < lines.Count; i++)
            {
                GeoLineString pl = new GeoLineString(lines[i].Vertices);
                src1.Add(pl);
            }

            //打断src1，记录结果到src
            List<GeoLineString> src = new List<GeoLineString>();
            GeometryComputation.LinesSplitLines.SplitLines(src1,src,false);

            //剔除在所有多边形外的线
            for (int i = 0; i < src.Count; i++)
            {
                bool brt=true;
                for (int j = 0; j < plg1.Count; j++)
                {
                    Toplogical.TpRelateConstants tp1 = Toplogical.TpLinetoPolygon.IsTpWith(src[i],plg1[j],false);
                    if (!(tp1 == Toplogical.TpRelateConstants.tpTouch || tp1 == Toplogical.TpRelateConstants.tpDisjoint))
                        brt = false;
                }
                if (brt)
                {
                    src.RemoveAt(i);
                    i--;
                }
            }

            //重构多边形
            GeometryComputation.CreatePolygon.CreatePolylineToPolygon(src,outPgs);
            //将空洞内的多边形剔除
            for (int i = 0; i < outPgs.Count; i++)
            {
                bool brt = true;
                for (int j = 0; j < plg1.Count; j++)
                {
                    Toplogical.TpRelateConstants tp1 = Toplogical.TpPolygontoPolygon.IsTpWith(outPgs[i], plg1[j], false);
                    if (!(tp1 == Toplogical.TpRelateConstants.tpDisjoint || tp1 == Toplogical.TpRelateConstants.tpTouch))
                        brt = false;
                }
                if (brt)
                {
                    outPgs.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < outPgs.Count; i++)
            {
                GeoPolygon pg = new GeoPolygon(outPgs[i].ExteriorRing, outPgs[i].InteriorRings);

                plg2.Add(pg);

            }

        }


    }
}
