using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;

namespace GIS.Converters.WellKnownText
{
    public class GeometryFromWKT
    {
        public static Geometry Parse(string wellKnownText)
        {
            // throws a parsing exception is there is a problem.
            System.IO.StringReader reader = new System.IO.StringReader(wellKnownText);
            return Parse(reader);
        }

        public static Geometry Parse(System.IO.TextReader reader)
        {
            WktStreamTokenizer tokenizer = new WktStreamTokenizer(reader);

            return ReadGeometryTaggedText(tokenizer);
        }

        private static List<GIS.Geometries.GeoPoint> GetCoordinates(WktStreamTokenizer tokenizer)
        {
            List<GIS.Geometries.GeoPoint> coordinates = new List<GIS.Geometries.GeoPoint>();
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return coordinates;

            GIS.Geometries.GeoPoint externalCoordinate = new GIS.Geometries.GeoPoint();
            GIS.Geometries.GeoPoint internalCoordinate = new GIS.Geometries.GeoPoint();

            externalCoordinate.X = GetNextNumber(tokenizer);
            externalCoordinate.Y = GetNextNumber(tokenizer);
            coordinates.Add(externalCoordinate);
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken == ",")
            {
                internalCoordinate = new GIS.Geometries.GeoPoint();
                internalCoordinate.X = GetNextNumber(tokenizer);
                internalCoordinate.Y = GetNextNumber(tokenizer);
                coordinates.Add(internalCoordinate);
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return coordinates;
        }

        private static double GetNextNumber(WktStreamTokenizer tokenizer)
        {
            tokenizer.NextToken();
            return tokenizer.GetNumericValue();
        }

        private static string GetNextEmptyOrOpener(WktStreamTokenizer tokenizer)
        {
            tokenizer.NextToken();
            string nextWord = tokenizer.GetStringValue();
            if (nextWord == "EMPTY" || nextWord == "(")
                return nextWord;

            throw new Exception("Expected 'EMPTY' or '(' but encountered '" + nextWord + "'");
        }

        private static string GetNextCloserOrComma(WktStreamTokenizer tokenizer)
        {
            tokenizer.NextToken();
            string nextWord = tokenizer.GetStringValue();
            if (nextWord == "," || nextWord == ")")
            {
                return nextWord;
            }
            throw new Exception("Expected ')' or ',' but encountered '" + nextWord + "'");
        }

        private static string GetNextCloser(WktStreamTokenizer tokenizer)
        {

            string nextWord = GetNextWord(tokenizer);
            if (nextWord == ")")
                return nextWord;

            throw new Exception("Expected ')' but encountered '" + nextWord + "'");
        }

        private static string GetNextWord(WktStreamTokenizer tokenizer)
        {
            TokenType type = tokenizer.NextToken();
            string token = tokenizer.GetStringValue();
            if (type == TokenType.Number)
                throw new Exception("Expected a number but got " + token);
            else if (type == TokenType.Word)
                return token.ToUpper();
            else if (token == "(")
                return "(";
            else if (token == ")")
                return ")";
            else if (token == ",")
                return ",";

            throw new Exception("Not a valid symbol in WKT format.");
        }

        private static Geometry ReadGeometryTaggedText(WktStreamTokenizer tokenizer)
        {
            tokenizer.NextToken();

            string type = tokenizer.GetStringValue().ToUpper();

            Geometry geometry = null;
            switch (type)
            {
                case "POINT":
                    geometry = ReadPointText(tokenizer);
                    break;
                case "LINESTRING":
                    geometry = ReadLineStringText(tokenizer);
                    break;
                case "MULTIPOINT":
                    geometry = ReadMultiPointText(tokenizer);
                    break;
                case "MULTILINESTRING":
                    geometry = ReadMultiLineStringText(tokenizer);
                    break;
                case "POLYGON":
                    geometry = ReadPolygonText(tokenizer);
                    break;
                case "MULTIPOLYGON":
                    geometry = ReadMultiPolygonText(tokenizer);
                    break;
                case "GEOMETRYCOLLECTION":
                    geometry = ReadGeometryCollectionText(tokenizer);
                    break;
                default:
                    throw new Exception(String.Format(GIS.Map.GeoMap.numberFormat_zhCN, "Geometrytype '{0}' is not supported.", type));
            }
            return geometry;
        }

        private static GeoMultiPolygon ReadMultiPolygonText(WktStreamTokenizer tokenizer)
        {
            GeoMultiPolygon polygons = new GeoMultiPolygon();
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return polygons;

            GeoPolygon polygon = ReadPolygonText(tokenizer);
            polygons.Polygons.Add(polygon);
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken == ",")
            {
                polygon = ReadPolygonText(tokenizer);
                polygons.Polygons.Add(polygon);
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return polygons;
        }

        private static GeoPolygon ReadPolygonText(WktStreamTokenizer tokenizer)
        {
            GIS.Geometries.GeoPolygon pol = new GeoPolygon();
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return pol;

            pol.ExteriorRing = new GeoLinearRing(GetCoordinates(tokenizer));
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken == ",")
            {
                //Add holes
                pol.InteriorRings.Add(new GeoLinearRing(GetCoordinates(tokenizer)));
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return pol;

        }

        private static GeoPoint ReadPointText(WktStreamTokenizer tokenizer)
        {
            GIS.Geometries.GeoPoint p = new GIS.Geometries.GeoPoint();
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return p;
            p.X = GetNextNumber(tokenizer);
            p.Y = GetNextNumber(tokenizer);
            GetNextCloser(tokenizer);
            return p;
        }

        private static GeoMultiPoint ReadMultiPointText(WktStreamTokenizer tokenizer)
        {
            GIS.Geometries.GeoMultiPoint mp = new GeoMultiPoint();
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return mp;
            mp.Points.Add(new GIS.Geometries.GeoPoint(GetNextNumber(tokenizer), GetNextNumber(tokenizer)));
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken == ",")
            {
                mp.Points.Add(new GIS.Geometries.GeoPoint(GetNextNumber(tokenizer), GetNextNumber(tokenizer)));
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return mp;
        }

        private static GeoMultiLineString ReadMultiLineStringText(WktStreamTokenizer tokenizer)
        {
            GeoMultiLineString lines = new GeoMultiLineString();
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken == "EMPTY")
                return lines;

            lines.LineStrings.Add(ReadLineStringText(tokenizer));
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken == ",")
            {
                lines.LineStrings.Add(ReadLineStringText(tokenizer));
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return lines;
        }

        private static GeoLineString ReadLineStringText(WktStreamTokenizer tokenizer)
        {
            return new GIS.Geometries.GeoLineString(GetCoordinates(tokenizer));
        }

        private static GeometryCollection ReadGeometryCollectionText(WktStreamTokenizer tokenizer)
        {
            GeometryCollection geometries = new GeometryCollection();
            string nextToken = GetNextEmptyOrOpener(tokenizer);
            if (nextToken.Equals("EMPTY"))
                return geometries;
            geometries.Collection.Add(ReadGeometryTaggedText(tokenizer));
            nextToken = GetNextCloserOrComma(tokenizer);
            while (nextToken.Equals(","))
            {
                geometries.Collection.Add(ReadGeometryTaggedText(tokenizer));
                nextToken = GetNextCloserOrComma(tokenizer);
            }
            return geometries;
        }

    }
}
