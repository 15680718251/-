using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GIS.Geometries;
using GIS.Layer;
namespace GIS.Utilities
{
    public class Transform
    {
        /// <summary>
        /// 地理坐标或者图纸坐标到屏幕坐标
        /// </summary>
        /// <param name="pt">地理坐标点或者图纸坐标</param>
        /// <param name="bound">地理范围</param>
        /// <param name="PixelAspectRatio">缩放比例尺</param>
        /// <returns>屏幕坐标</returns>
        public static Point MapToScreen(GeoPoint pt, GeoBound bound, double  PixelAspectRatio)
        {
            int x = (int)((pt.X - bound.Left) / PixelAspectRatio);
            int y = (int)((bound.Top - pt.Y) / PixelAspectRatio);
            return new Point(x, y);
        }
        public static PointF MapToScreenF(GeoPoint pt, GeoBound bound, double PixelAspectRatio)
        {
            float x = (float)((pt.X - bound.Left) / PixelAspectRatio);
            float y = (float)((bound.Top - pt.Y) / PixelAspectRatio);
            return new PointF(x, y);
        }
        public static PointF MapToScreenF(PointF pt, GeoBound bound, double PixelAspectRatio)
        {
            float x = (float)((pt.X - bound.Left) / PixelAspectRatio);
            float y = (float)((bound.Top - pt.Y) / PixelAspectRatio);
            return new PointF(x, y);
        }
        /// <summary>
        /// 屏幕坐标到地理坐标或者图纸坐标
        /// </summary>
        /// <param name="pt">屏幕坐标</param>
        /// <param name="bound">地理范围</param>
        /// <param name="PixelAspectRatio">缩放比例尺</param>
        /// <returns>地理坐标</returns>
        public static GeoPoint ScreenToMap(Point pt, GeoBound bound, double PixelAspectRatio)
        {
            double x = bound.Left + pt.X * PixelAspectRatio;
            double y = bound.Top - pt.Y * PixelAspectRatio;
            return new GeoPoint(x,y);
        }

        public static GeoPoint ScreenToMapF(PointF pt, GeoBound bound, double PixelAspectRatio)
        {
            double x = bound.Left + pt.X * PixelAspectRatio;
            double y = bound.Top - pt.Y * PixelAspectRatio;
            return new GeoPoint(x, y);
        }
        #region PaperToWorld
        /// <summary>
        /// 将图层里的所有图纸坐标 转为 地理数据
        /// </summary>
        /// <param name="layer">图层</param>
        /// <param name="bound">图层的地理坐标范围</param>
        /// <param name="dblc">当前地图比例尺</param>
        public static void PaperToWorld(GeoLayer layer, GeoBound bound, double dblc)
        {
            if (layer.LayerType != LAYERTYPE.VectorLayer)
                return;
            for (int i = 0; i < ((GeoVectorLayer)layer).DataTable.Count; i++)
            {
                Geometry geom = ((GeoVectorLayer)layer).DataTable[i].Geometry;
                Transform.PaperToWorld(geom, bound, dblc);
            }
        }

        public static void PaperToWorld(Geometry geom, GeoBound bound, double dblc)
        {
            if (geom is GeoPoint)
            {
                GeoPoint pt = (GeoPoint)geom;
                PaperToWorld(pt, bound, dblc);
            }
            else if (geom is GeoLineString)
            {
                GeoLineString line = (GeoLineString)geom;
                PaperToWorld(line, bound, dblc);
            }
            else if (geom is GeoPolygon)
            {
                GeoPolygon plg = (GeoPolygon)geom;
                PaperToWorld(plg, bound, dblc);
            }
            else if (geom is GeoMultiLineString)
            {
                GeoMultiLineString lines = (GeoMultiLineString)geom;
                PaperToWorld(lines, bound, dblc);
            }
            else if (geom is GeoMultiPoint)
            {
                GeoMultiPoint pts = (GeoMultiPoint)geom;
                PaperToWorld(pts, bound, dblc);
            }
            else if (geom is GeoMultiPolygon)
            {
                GeoMultiPolygon plgs = (GeoMultiPolygon)geom;
                PaperToWorld(plgs, bound, dblc);
            }
            else
            {
                throw new Exception("转换到图纸坐标的几何类型未知");
            }
            geom.Bound = geom.GetBoundingBox();

        }
        /// <summary>
        /// 将图纸坐标转换为地理坐标
        /// </summary>
        /// <param name="pt">图纸坐标点</param>
        /// <param name="bound">地理坐标范围</param>
        /// <param name="dblc">比例尺 500,1000</param>
        /// <returns>返回地理坐标</returns>
        public static void PaperToWorld( GeoPoint pt, GeoBound bound, double dblc)
        {
            double x = pt.X * dblc + bound.Left;
            double y = pt.Y * dblc + bound.Bottom;
            pt.SetXY(x, y);
        }

        public static void PaperToWorld(GeoMultiPoint pts, GeoBound bound, double dblc)
        {
            for (int i = 0; i < pts.NumGeometries; i++)
            {
                PaperToWorld(pts.Points[i], bound, dblc);
            }
        }
        public static void PaperToWorld(GeoLineString line, GeoBound bound, double dblc)
        {
            for (int i = 0; i < line.NumPoints; i++)
            {
                PaperToWorld(line.Vertices[i], bound, dblc);
            }
        }

        public static void PaperToWorld(GeoMultiLineString lines, GeoBound bound, double dblc)
        {
            for (int i = 0; i < lines.NumGeometries; i++)
            {
                PaperToWorld(lines.LineStrings[i], bound, dblc);
            }
        }

        public static void PaperToWorld(GeoMultiPolygon plgs, GeoBound bound, double dblc)
        {
            for (int i = 0; i < plgs.NumGeometries; i++)
            {
                PaperToWorld(plgs.Polygons[i], bound, dblc);
            }
        }
        public static void PaperToWorld(GeoPolygon polygon, GeoBound bound, double dblc)
        {
            PaperToWorld(polygon.ExteriorRing, bound, dblc);

            for (int i = 0; i < polygon.InteriorRings.Count; i++)
            {
                PaperToWorld(polygon.InteriorRings[i], bound, dblc);
            }
        } 
        #endregion
        
        
        #region WorldToPaper
 
        /// <summary>
        /// 将图层里的所有地理数据转为图纸坐标
        /// </summary>
        /// <param name="layer">图层</param>
        /// <param name="bound">图层的地理坐标范围</param>
        /// <param name="dblc">当前地图比例尺</param>
        public static  void WorldToPaper(GeoLayer layer, GeoBound bound, double dblc)
        {
            if (layer.LayerType != LAYERTYPE.VectorLayer)
                return;

            for (int i = 0; i < ((GeoVectorLayer)layer).DataTable.Count; i++)
            {
                Geometry geom = ((GeoVectorLayer)layer).DataTable[i].Geometry;
                Transform.WorldToPaper(geom, bound, dblc);
            }
        }
  
        public static void WorldToPaper( Geometry geom, GeoBound bound, double dblc)
        {
            if (geom is GeoPoint)
            {
                GeoPoint pt = (GeoPoint) geom;
                WorldToPaper( pt, bound, dblc);
            }
            else if(geom is GeoLineString)
            {
                GeoLineString line = (GeoLineString)geom;
                WorldToPaper( line, bound, dblc);
            }
            else if (geom is GeoPolygon)
            {
                GeoPolygon plg = (GeoPolygon)geom;
                WorldToPaper(plg, bound, dblc);
            }
            else if (geom is GeoMultiLineString)
            {
                GeoMultiLineString lines = (GeoMultiLineString)geom;
                WorldToPaper( lines, bound, dblc);
            }
            else if (geom is GeoMultiPoint)
            {
                GeoMultiPoint pts = (GeoMultiPoint)geom;
                WorldToPaper( pts, bound, dblc);
            }
            else if (geom is GeoMultiPolygon)
            {
                GeoMultiPolygon plgs = (GeoMultiPolygon)geom;
                WorldToPaper(plgs, bound, dblc);
            }
            else
            {
                throw new Exception("转换到图纸坐标的几何类型未知");
            }
            geom.Bound = geom.GetBoundingBox();

        }
        /// <summary>
        /// 将地理坐标转换成图纸坐标
        /// </summary>
        /// <param name="pt">地理坐标点</param>
        /// <param name="bound">地理坐标范围</param>
        /// <param name="dblc">比例尺</param>
        /// <returns>图纸坐标点</returns>
        public static void WorldToPaper( GeoPoint pt, GeoBound bound, double dblc)
        {
            double x = pt.X - bound.Left;
            double y = pt.Y - bound.Bottom;
            pt.SetXY(x / dblc, y / dblc);
        }
        public static void WorldToPaper( GeoMultiPoint pts, GeoBound bound, double dblc)
        {
            for (int i = 0; i < pts.NumGeometries; i++)
            {
                WorldToPaper( pts.Points[i], bound, dblc);
            }
        }
        public static void WorldToPaper(GeoLineString line, GeoBound bound, double dblc)
        {
            for (int i = 0; i < line.NumPoints; i++)
            {
                WorldToPaper( line.Vertices[i], bound, dblc);
            }
        }

        public static void WorldToPaper(GeoMultiLineString lines, GeoBound bound, double dblc)
        {
            for (int i = 0; i < lines.NumGeometries; i++)
            {
                WorldToPaper( lines.LineStrings[i], bound, dblc);
            }
        }

        public static void WorldToPaper(GeoMultiPolygon plgs, GeoBound bound, double dblc)
        {
            for (int i = 0; i < plgs.NumGeometries; i++)
            {
                WorldToPaper(plgs.Polygons[i], bound, dblc);
            }
        }
        public static void WorldToPaper(GeoPolygon polygon, GeoBound bound, double dblc)
        {
            WorldToPaper(polygon.ExteriorRing, bound, dblc);

            for (int i = 0; i < polygon.InteriorRings.Count; i++)
            {
                WorldToPaper( polygon.InteriorRings[i], bound, dblc);
            }
        } 
        #endregion
    }
}
