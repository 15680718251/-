using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;

namespace GIS.UI.WellKnownText
{
    /// <summary>
    /// Converts spatial reference IDs to a Well-Known Text representation.
    /// </summary>
    public class SpatialReference
    {
        /// <summary>
        /// Converts a Spatial Reference ID to a Well-known Text representation
        /// </summary>
        /// <param name="srid">Spatial Reference ID</param>
        /// <returns>Well-known text</returns>
        public static string SridToWkt(int srid)
        {
            XmlDocument xmldoc = new XmlDocument();

            string file = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase) + "\\SpatialRefSys.xml";
            //if (!System.IO.File.Exists(file))
            //	throw new ApplicationException("Spatial reference system database not found: " + file);
            xmldoc.Load(file);
            //xmldoc.Load(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpMap.SpatialReference.SpatialRefSys.xml"));
            XmlNode node =
                xmldoc.DocumentElement.SelectSingleNode("/SpatialReference/ReferenceSystem[SRID='" + srid + "']");
            if (node != null)
                return node.LastChild.InnerText;
            else
                return "";
        }
    }
}
