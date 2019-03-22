using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.Converters.WellKnownBinary
{
    internal enum WKBGeometryType : uint
    {
        wkbPoint = 1,
        wkbLineString = 2,
        wkbPolygon = 3,
        wkbMultiPoint = 4,
        wkbMultiLineString = 5,
        wkbMultiPolygon = 6,
        wkbGeometryCollection = 7
    }
}
