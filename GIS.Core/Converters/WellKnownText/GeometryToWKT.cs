using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using GIS.Geometries;

namespace GIS.Converters.WellKnownText
{
    public class GeometryToWKT
    {
        #region Methods

        public static string Write(Geometry geometry)
        {
            StringWriter sw = new StringWriter();
            Write(geometry, sw);
            return sw.ToString();
        }

        public static void Write(Geometry geometry, StringWriter writer)
        {
            AppendGeometryTaggedText(geometry, writer);
        }


        private static void AppendGeometryTaggedText(Geometry geometry, StringWriter writer)
        {
            if (geometry == null)
                throw new NullReferenceException("Cannot write Well-Known Text: geometry was null"); ;
            if (geometry is GeoPoint)
            {
                GeoPoint point = geometry as GeoPoint;
                AppendPointTaggedText(point, writer);
            }
            else if (geometry is GeoLineString)
                AppendLineStringTaggedText(geometry as GeoLineString, writer);
            else if (geometry is GeoPolygon)
                AppendPolygonTaggedText(geometry as GeoPolygon, writer);
            else if (geometry is GeoMultiPoint)
                AppendMultiPointTaggedText(geometry as GeoMultiPoint, writer);
            else if (geometry is GeoMultiLineString)
                AppendMultiLineStringTaggedText(geometry as GeoMultiLineString, writer);
            else if (geometry is GeoMultiPolygon)
                AppendMultiPolygonTaggedText(geometry as GeoMultiPolygon, writer);
            else if (geometry is GeometryCollection)
                AppendGeometryCollectionTaggedText(geometry as GeometryCollection, writer);
            else
                throw new NotSupportedException("Unsupported Geometry implementation:" + geometry.GetType().Name);
        }

        private static void AppendPointTaggedText(GeoPoint coordinate, StringWriter writer)
        {
            writer.Write("POINT ");
            AppendPointText(coordinate, writer);
        }


        private static void AppendLineStringTaggedText(GeoLineString lineString, StringWriter writer)
        {
            writer.Write("LINESTRING ");
            AppendLineStringText(lineString, writer);
        }

        private static void AppendPolygonTaggedText(GeoPolygon polygon, StringWriter writer)
        {
            writer.Write("POLYGON ");
            AppendPolygonText(polygon, writer);
        }

        private static void AppendMultiPointTaggedText(GeoMultiPoint multipoint, StringWriter writer)
        {
            writer.Write("MULTIPOINT ");
            AppendMultiPointText(multipoint, writer);
        }

        private static void AppendMultiLineStringTaggedText(GeoMultiLineString multiLineString, StringWriter writer)
        {
            writer.Write("MULTILINESTRING ");
            AppendMultiLineStringText(multiLineString, writer);
        }

        private static void AppendMultiPolygonTaggedText(GeoMultiPolygon multiPolygon, StringWriter writer)
        {

            writer.Write("MULTIPOLYGON ");
            AppendMultiPolygonText(multiPolygon, writer);

        }

        private static void AppendGeometryCollectionTaggedText(GeometryCollection geometryCollection, StringWriter writer)
        {
            writer.Write("GEOMETRYCOLLECTION ");
            AppendGeometryCollectionText(geometryCollection, writer);
        }

        private static void AppendPointText(GeoPoint coordinate, StringWriter writer)
        {
            if (coordinate == null || coordinate.IsEmpty())
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                AppendCoordinate(coordinate, writer);
                writer.Write(")");
            }
        }

        private static void AppendCoordinate(GeoPoint coordinate, StringWriter writer)
        {
            for (uint i = 0; i < coordinate.NumOrdinates; i++)
                writer.Write(WriteNumber(coordinate[i]) + (i < coordinate.NumOrdinates - 1 ? " " : ""));
        }

        private static string WriteNumber(double d)
        {
            try
            {
                return d.ToString(GIS.Map.GeoMap.numberFormat_zhCN);
            }
            catch (Exception e)
            {
                MessageBox.Show(d.ToString() + ":  " + e.Message.ToString());
            }
            return d.ToString();
        }

        private static void AppendLineStringText(GeoLineString lineString, StringWriter writer)
        {

            if (lineString == null || lineString.IsEmpty())
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                for (int i = 0; i < lineString.NumPoints; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    AppendCoordinate(lineString.Vertices[i], writer);
                }
                writer.Write(")");
            }
        }

        private static void AppendPolygonText(GeoPolygon polygon, StringWriter writer)
        {
            if (polygon == null || polygon.IsEmpty())
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                AppendLineStringText(polygon.ExteriorRing, writer);
                for (int i = 0; i < polygon.InteriorRings.Count; i++)
                {
                    writer.Write(", ");
                    AppendLineStringText(polygon.InteriorRings[i], writer);
                }
                writer.Write(")");
            }
        }

        private static void AppendMultiPointText(GeoMultiPoint multiPoint, StringWriter writer)
        {

            if (multiPoint == null || multiPoint.IsEmpty())
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                for (int i = 0; i < multiPoint.Points.Count; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    AppendCoordinate(multiPoint[i], writer);
                }
                writer.Write(")");
            }
        }

        private static void AppendMultiLineStringText(GeoMultiLineString multiLineString, StringWriter writer)
        {

            if (multiLineString == null || multiLineString.IsEmpty())
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                for (int i = 0; i < multiLineString.LineStrings.Count; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    AppendLineStringText(multiLineString[i], writer);
                }
                writer.Write(")");
            }
        }

        private static void AppendMultiPolygonText(GeoMultiPolygon multiPolygon, StringWriter writer)
        {
            if (multiPolygon == null || multiPolygon.IsEmpty())
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                for (int i = 0; i < multiPolygon.Polygons.Count; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    AppendPolygonText(multiPolygon[i], writer);
                }
                writer.Write(")");
            }
        }

        private static void AppendGeometryCollectionText(GeometryCollection geometryCollection, StringWriter writer)
        {
            if (geometryCollection == null || geometryCollection.IsEmpty())
                writer.Write("EMPTY");
            else
            {
                writer.Write("(");
                for (int i = 0; i < geometryCollection.Collection.Count; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    AppendGeometryTaggedText(geometryCollection[i], writer);
                }
                writer.Write(")");
            }
        }
        #endregion
    }
}
