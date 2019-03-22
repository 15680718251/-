using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using OSGeo.OGR;

namespace GIS.Converters.GeoConverter
{
    public class GeometryConverter
    {
        public static  Geometries.Geometry OGRTOGeometric(OSGeo.OGR.Geometry gin)
        {
            string wkt = null;
            gin.ExportToWkt(out wkt);
            Geometries.Geometry gout = GIS.Converters.WellKnownText.GeometryFromWKT.Parse(wkt) as GeoPolygon;
            return gout;
        }
        public static OSGeo.OGR.Geometry GeometricToOGR(Geometries.Geometry gin)
        {
            string wkt = GIS.Converters.WellKnownText.GeometryToWKT.Write(gin);
            OSGeo.OGR.Geometry gout = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
            return gout;
        }
    }
}
