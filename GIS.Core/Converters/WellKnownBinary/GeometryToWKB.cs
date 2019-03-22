using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GIS.Geometries;

namespace GIS.Converters.WellKnownBinary
{
    public class GeometryToWKB
    {
        public static byte[] Write(Geometry g)
        {
            return Write(g, WkbByteOrder.Ndr);
        }

        public static byte[] Write(Geometry g, WkbByteOrder wkbByteOrder)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            //Write the byteorder format.
            bw.Write((byte)wkbByteOrder);

            //Write the type of this geometry
            WriteType(g, bw, wkbByteOrder);

            //Write the geometry
            WriteGeometry(g, bw, wkbByteOrder);

            return ms.ToArray();
        }

        #region Methods

        private static void WriteType(Geometry geometry, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Determine the type of the geometry.
            switch (geometry.GetType().FullName)
            {
                //Points are type 1.
                case "GIS.Geometries.GeoPoint":
                    WriteUInt32((uint)WKBGeometryType.wkbPoint, bWriter, byteorder);
                    break;
                //Linestrings are type 2.
                case "GIS.Geometries.GeoLineString":
                    WriteUInt32((uint)WKBGeometryType.wkbLineString, bWriter, byteorder);
                    break;
                //Polygons are type 3.
                case "GIS.Geometries.GeoPolygon":
                    WriteUInt32((uint)WKBGeometryType.wkbPolygon, bWriter, byteorder);
                    break;
                //Mulitpoints are type 4.
                case "GIS.Geometries.GeoMultiPoint":
                    WriteUInt32((uint)WKBGeometryType.wkbMultiPoint, bWriter, byteorder);
                    break;
                //Multilinestrings are type 5.
                case "GIS.Geometries.GeoMultiLineString":
                    WriteUInt32((uint)WKBGeometryType.wkbMultiLineString, bWriter, byteorder);
                    break;
                //Multipolygons are type 6.
                case "GIS.Geometries.GeoMultiPolygon":
                    WriteUInt32((uint)WKBGeometryType.wkbMultiPolygon, bWriter, byteorder);
                    break;
                //Geometrycollections are type 7.
                case "GIS.Geometries.GeometryCollection":
                    WriteUInt32((uint)WKBGeometryType.wkbGeometryCollection, bWriter, byteorder);
                    break;
                default:
                    throw new ArgumentException("Invalid Geometry Type");
            }
        }

        private static void WriteGeometry(Geometry geometry, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            switch (geometry.GetType().FullName)
            {
                //Write the point.
                case "GIS.Geometries.GeoPoint":
                    WritePoint((GeoPoint)geometry, bWriter, byteorder);
                    break;
                case "GIS.Geometries.GeoLineString":
                    GeoLineString ls = (GeoLineString)geometry;
                    WriteLineString(ls, bWriter, byteorder);
                    break;
                case "GIS.Geometries.GeoPolygon":
                    WritePolygon((GeoPolygon)geometry, bWriter, byteorder);
                    break;
                //Write the Multipoint.
                case "GIS.Geometries.GeoMultiPoint":
                    WriteMultiPoint((GeoMultiPoint)geometry, bWriter, byteorder);
                    break;
                //Write the Multilinestring.
                case "GIS.Geometries.GeoMultiLineString":
                    WriteMultiLineString((GeoMultiLineString)geometry, bWriter, byteorder);
                    break;
                //Write the Multipolygon.
                case "SharpMap.Geometries.MultiPolygon":
                    WriteMultiPolygon((GeoMultiPolygon)geometry, bWriter, byteorder);
                    break;
                //Write the Geometrycollection.
                case "GIS.Geometries.GeometryCollection":
                    WriteGeometryCollection((GeometryCollection)geometry, bWriter, byteorder);
                    break;
                default:
                    throw new ArgumentException("Invalid Geometry Type");
            }
        }

        private static void WritePoint(GeoPoint point, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            WriteDouble(point.X, bWriter, byteorder);
            WriteDouble(point.Y, bWriter, byteorder);
        }

        private static void WriteLineString(GeoLineString ls, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the number of points in this linestring.
            WriteUInt32((uint)ls.Vertices.Count, bWriter, byteorder);

            //Loop on each vertices.
            foreach (GeoPoint p in ls.Vertices)
                WritePoint(p, bWriter, byteorder);
        }

        private static void WritePolygon(GeoPolygon poly, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            int numRings = poly.InteriorRings.Count + 1;

            WriteUInt32((uint)numRings, bWriter, byteorder);

            WriteLineString((GeoLineString)poly.ExteriorRing, bWriter, byteorder);

            foreach (GeoLinearRing lr in poly.InteriorRings)
                WriteLineString((GeoLineString)lr, bWriter, byteorder);
        }

        private static void WriteMultiPoint(GeoMultiPoint mp, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            WriteUInt32((uint)mp.Points.Count, bWriter, byteorder);

            foreach (GeoPoint p in mp.Points)
            {
                //Write Points Header
                bWriter.Write((byte)byteorder);
                WriteUInt32((uint)WKBGeometryType.wkbPoint, bWriter, byteorder);
                //Write each point.
                WritePoint((GeoPoint)p, bWriter, byteorder);
            }
        }

        private static void WriteMultiLineString(GeoMultiLineString mls, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the number of linestrings.
            WriteUInt32((uint)mls.LineStrings.Count, bWriter, byteorder);

            //Loop on the number of linestrings.
            foreach (GeoLineString ls in mls.LineStrings)
            {
                //Write LineString Header
                bWriter.Write((byte)byteorder);
                WriteUInt32((uint)WKBGeometryType.wkbLineString, bWriter, byteorder);
                //Write each linestring.
                WriteLineString(ls, bWriter, byteorder);
            }
        }

        private static void WriteMultiPolygon(GeoMultiPolygon mp, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Write the number of polygons.
            WriteUInt32((uint)mp.Polygons.Count, bWriter, byteorder);

            //Loop on the number of polygons.
            foreach (GeoPolygon poly in mp.Polygons)
            {
                //Write polygon header
                bWriter.Write((byte)byteorder);
                WriteUInt32((uint)WKBGeometryType.wkbPolygon, bWriter, byteorder);
                //Write each polygon.
                WritePolygon(poly, bWriter, byteorder);
            }
        }

        private static void WriteGeometryCollection(GeometryCollection gc, BinaryWriter bWriter, WkbByteOrder byteorder)
        {
            //Get the number of geometries in this geometrycollection.
            int numGeometries = gc.NumGeometries;

            //Write the number of geometries.
            WriteUInt32((uint)numGeometries, bWriter, byteorder);

            //Loop on the number of geometries.
            for (int i = 0; i < numGeometries; i++)
            {
                //Write the byte-order format of the following geometry.
                bWriter.Write((byte)byteorder);
                //Write the type of each geometry.
                WriteType(gc[i], bWriter, byteorder);
                //Write each geometry.
                WriteGeometry(gc[i], bWriter, byteorder);
            }
        }
        #endregion

        private static void WriteUInt32(UInt32 value, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                writer.Write(BitConverter.ToUInt32(bytes, 0));
            }
            else
                writer.Write(value);
        }

        private static void WriteDouble(double value, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                writer.Write(BitConverter.ToDouble(bytes, 0));
            }
            else
                writer.Write(value);
        }
    }
}
