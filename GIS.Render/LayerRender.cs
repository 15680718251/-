using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GIS.Layer;
using GIS.Geometries;
using GIS.GeoData;
using GIS.Utilities;
using GIS.Style;
namespace GIS.Render
{
    public class LayerRender
    {
        //public static void VectorLayerSymbolRender(SymbolSolver SymSol, GeoVectorLayer layer, GeoBound renderbound,
        //                           ITransForm map)
        //{
        //    VectorStyle unsltStyle = layer.LayerStyle;
        //    int nNumsGeo = layer.DataTable.Count;
        //    for (int k = 0; k < nNumsGeo; k++)
        //    {
        //        GeoData.GeoDataRow Row = layer.DataTable[k];
        //        Geometry geom = Row.Geometry;
        //        if (geom == null || (int)Row.EditState >= 6 ||
        //            !geom.Bound.IsIntersectWith(renderbound))    /// 如果已经删除或者与当前屏幕坐标范围不相交 则不画
        //            continue;
        //        int tmpID = System.Convert.ToInt32((long)Row["ClasID"]);

        //        if (Row.SelectState == true)
        //        {
        //            continue;
        //        }
              

        //        SymSol.emptyEleStyles();
        //        if (geom is GeoPoint)
        //        {
        //            if (!SymSol.ExtractSymboler(tmpID, SymbolSolver.SymbolType.POINTTYPE))
        //            {
        //                //throw new Exception("classID not found!");
        //                continue;
        //            }

        //            PointF pt = map.TransFromWorldToMapF(((GeoPoint)geom));

        //            SymSol.CreatingPointSymbol(tmpID, pt, unsltStyle.FillColor, 4.0f);
                  
        //        }
        //        else if (geom is GeoLineString)
        //        {
        //            if (!SymSol.ExtractSymboler(tmpID, SymbolSolver.SymbolType.LINETYPE))
        //            {
        //                //throw new Exception("classID not found!");
        //                continue;
        //            }
        //            int ptnum = ((GeoLineString)geom).NumPoints;

        //            PointF[] points = map.TransLineToMapF(((GeoLineString)geom));

        //            SymSol.CreatingPolyLine(tmpID, points, ptnum, 0, "", unsltStyle.FillColor);
                    
        //        }
        //        else if (geom is GeoPolygon)
        //        {
        //            if (!SymSol.ExtractSymboler(tmpID, SymbolSolver.SymbolType.REGIONTYPE))
        //            {
        //                // throw new Exception("classID not found!");
        //                continue;
        //            }
        //            GeoPolygon GeoPlg = ((GeoPolygon)geom);
        //            int ptnum = GeoPlg.ExteriorRing.NumPoints;
        //            PointF[] points = map.TransLineToMapF(GeoPlg.ExteriorRing);

        //            SymSol.CreatingPolygon(tmpID, points, ptnum, 0, "", Color.Blue, unsltStyle.FillColor);
                    

        //            for (int i = 0; i < GeoPlg.InteriorRings.Count; i++)
        //            {
        //                PointF[] pts = map.TransLineToMapF(GeoPlg.InteriorRings[i]);
        //                int ptcnt = GeoPlg.InteriorRings[i].NumPoints;
        //                SymSol.FillRegion(pts, ptcnt, Color.Blue, SymSol.BackColor);
        //            }
        //        }
        //        else
        //        {
        //            //do nothing   
        //        }
        //    }
        //}

        public static void VectorLayerRender(Graphics g, GeoVectorLayer layer, GeoBound renderbound,ITransForm map)
        {
            VectorStyle unsltStyle = layer.LayerStyle;
            int nNumsGeo = layer.DataTable.Count;
            for (int k = 0; k < nNumsGeo; k++)
            {
                GeoData.GeoDataRow row = layer.DataTable[k];
                Geometry geom = row.Geometry;
                if (geom == null || (int)row.EditState >= 6 ||
                    !geom.Bound.IsIntersectWith(renderbound))    /// 如果已经删除或者与当前屏幕坐标范围不相交 则不画
                    continue;

                if (row.SelectState == true)
                {
                    continue;           //如果选中 则不绘制
                }

                Render.RenderAPI.DrawGeometry(g,geom, unsltStyle, map);
            }
        }

        public static void RasterLayerRender(Graphics g, GeoRasterLayer layer, Size ScrSize, ITransForm transform)
        {
            layer.DrawImage(g, ScrSize, transform);
        }

        //public static void LabelLayerRender(Graphics g, GeoLabelLayer layer, GeoBound renderbound,ITransForm map)
        //{
           
        //    int nNumsLabel = layer.DataTable.Count;
        //    for (int k = 0; k < nNumsLabel; k++)
        //    {
        //        GeoData.GeoDataRow row = layer.DataTable[k];
        //        if (row.SelectState == true)
        //            continue;
        //        GeoLabel label = row.Geometry as GeoLabel;
        //        if (label == null || (int)row.EditState >= 6 ||
        //            !label.Bound.IsIntersectWith(renderbound))    /// 如果已经删除或者与当前屏幕坐标范围不相交 则不画
        //            continue;
                
        //         Render.RenderAPI.DrawLabel(g, label, label.Color, map);
               
        //    }
        //}
    }
}
