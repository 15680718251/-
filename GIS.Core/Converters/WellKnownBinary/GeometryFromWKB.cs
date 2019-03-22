using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GIS.Geometries;
using System.Diagnostics;

namespace GIS.Converters.WellKnownBinary
{
    public class GeometryFromWKB
    {
        public static Geometry Parse(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    return Parse(reader);
                }
            }
        }

        public static Geometry Parse(BinaryReader reader)
        {
            byte byteOrder = reader.ReadByte();

            if (!Enum.IsDefined(typeof(WkbByteOrder), byteOrder))
            {
                throw new ArgumentException("Byte order not recognized");
            }

            uint type = (uint)ReadUInt32(reader, (WkbByteOrder)byteOrder);

            if (!Enum.IsDefined(typeof(WKBGeometryType), type))
                throw new ArgumentException("Geometry type not recognized");

            switch ((WKBGeometryType)type)
            {
                case WKBGeometryType.wkbPoint:
                    return CreateWKBPoint(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbLineString:
                    return CreateWKBLineString(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbPolygon:
                    return CreateWKBPolygon(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbMultiPoint:
                    return CreateWKBMultiPoint(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbMultiLineString:
                    return CreateWKBMultiLineString(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbMultiPolygon:
                    return CreateWKBMultiPolygon(reader, (WkbByteOrder)byteOrder);

                case WKBGeometryType.wkbGeometryCollection:
                    return CreateWKBGeometryCollection(reader, (WkbByteOrder)byteOrder);

                default:
                    throw new NotSupportedException("Geometry type '" + type.ToString() + "' not supported");
            }
        }

        private static GeoPoint CreateWKBPoint(BinaryReader reader, WkbByteOrder byteOrder)
        {
            return new GeoPoint(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder));
        }

        private static GeoPoint[] ReadCoordinates(BinaryReader reader, WkbByteOrder byteOrder)
        {
            int numPoints = (int)ReadUInt32(reader, byteOrder);

            GeoPoint[] coords = new GeoPoint[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                coords[i] = new GeoPoint(ReadDouble(reader, byteOrder), ReadDouble(reader, byteOrder));
            }
            return coords;
        }

        private static GeoLineString CreateWKBLineString(BinaryReader reader, WkbByteOrder byteOrder)
        {
            GIS.Geometries.GeoLineString l = new GIS.Geometries.GeoLineString();
            l.Vertices.AddRange(ReadCoordinates(reader, byteOrder));
            return l;
        }

        private static GeoLinearRing CreateWKBLinearRing(BinaryReader reader, WkbByteOrder byteOrder)
        {
            GIS.Geometries.GeoLinearRing l = new GIS.Geometries.GeoLinearRing();
            l.Vertices.AddRange(ReadCoordinates(reader, byteOrder));
            //if polygon isn't closed, add the first point to the end (this shouldn't occur for correct WKB data)
            if (l.Vertices[0].X != l.Vertices[l.Vertices.Count - 1].X || l.Vertices[0].Y != l.Vertices[l.Vertices.Count - 1].Y)
                l.Vertices.Add(new GeoPoint(l.Vertices[0].X, l.Vertices[0].Y));
            return l;
        }

        private static GeoPolygon CreateWKBPolygon(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the Number of rings in this Polygon.
            int numRings = (int)ReadUInt32(reader, byteOrder);
            Debug.Assert(numRings >= 1, "Number of rings in polygon must be 1 or more.");
            GeoPolygon shell = new GeoPolygon(CreateWKBLinearRing(reader, byteOrder));

            // Create a new array of linearrings for the interior rings.
            for (int i = 0; i < (numRings - 1); i++)
                shell.InteriorRings.Add(CreateWKBLinearRing(reader, byteOrder));

            return shell;
        }

        private static GeoMultiPoint CreateWKBMultiPoint(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the number of points in this multipoint.
            int numPoints = (int)ReadUInt32(reader, byteOrder);

            // Create a new array for the points.
            GeoMultiPoint points = new GeoMultiPoint();

            // Loop on the number of points.
            for (int i = 0; i < numPoints; i++)
            {
                // Read point header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                points.Points.Add(CreateWKBPoint(reader, byteOrder));
            }
            return points;
        }

        private static GeoMultiLineString CreateWKBMultiLineString(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the number of linestrings in this multilinestring.
            int numLineStrings = (int)ReadUInt32(reader, byteOrder);

            // Create a new array for the linestrings .
            GeoMultiLineString mline = new GeoMultiLineString();

            // Loop on the number of linestrings.
            for (int i = 0; i < numLineStrings; i++)
            {
                // Read linestring header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                // Create the next linestring and add it to the array.
                mline.LineStrings.Add(CreateWKBLineString(reader, byteOrder));
            }

            return mline;
        }

        private static GeoMultiPolygon CreateWKBMultiPolygon(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // Get the number of Polygons.
            int numPolygons = (int)ReadUInt32(reader, byteOrder);

            // Create a new array for the Polygons.
            GeoMultiPolygon polygons = new GeoMultiPolygon();

            // Loop on the number of polygons.
            for (int i = 0; i < numPolygons; i++)
            {
                // read polygon header
                reader.ReadByte();
                ReadUInt32(reader, byteOrder);

                // TODO: Validate type

                // Create the next polygon and add it to the array.
                polygons.Polygons.Add(CreateWKBPolygon(reader, byteOrder));
            }

            //Create and return the MultiPolygon.
            return polygons;
        }

        private static Geometry CreateWKBGeometryCollection(BinaryReader reader, WkbByteOrder byteOrder)
        {
            // The next byte in the array tells the number of geometries in this collection.
            int numGeometries = (int)ReadUInt32(reader, byteOrder);

            GeometryCollection geometries = new GeometryCollection();

            for (int i = 0; i < numGeometries; i++)
            {
                // Call the main create function with the next geometry.
                geometries.Collection.Add(Parse(reader));
            }

            return geometries;
        }

        //NOT USED
        //private static int ReadInt32(BinaryReader reader, WKBByteOrder byteOrder)
        //{
        //    if (byteOrder == WKBByteOrder.Xdr)
        //    {
        //        byte[] bytes = BitConverter.GetBytes(reader.ReadInt32()); 
        //        Array.Reverse(bytes);
        //        return BitConverter.ToInt32(bytes, 0);
        //    }
        //    else
        //        return reader.ReadInt32();
        //}

        private static uint ReadUInt32(BinaryReader reader, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(reader.ReadUInt32());
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
            else
                return reader.ReadUInt32();
        }

        private static double ReadDouble(BinaryReader reader, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(reader.ReadDouble());
                Array.Reverse(bytes);
                return BitConverter.ToDouble(bytes, 0);
            }
            else
                return reader.ReadDouble();
        }
    }
}
