using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeatureMatchUpdate
{
    public class LineRuleItems
    {
        private string osm_key;
        private string osm_value;
        private string nationEleName;
        private string elementType;
        private string elementTypeID;
        private string featureID;
        private string geometry;

        public string Osm_key
        {
            get { return osm_key; }
            set { osm_key = value; }
        }
        public string Osm_value
        {
            get { return osm_value; }
            set { osm_value = value; }
        }
        public string NationEleName
        {
            get { return nationEleName; }
            set { nationEleName = value; }
        }
        public string ElementType
        {
            get { return elementType; }
            set { elementType = value;}
        }
        public string ElementTypeID
        {
            get { return elementTypeID;}
            set { elementTypeID = value; }
        }
        public string Geometry
        {
            get { return geometry; }
            set { geometry = value; }
        }

        public LineRuleItems(string osm_key, string osm_value,string nationEleName,string elementType,string elementTypeID,string featureID,string geometry)
        {
            this.osm_key = osm_key;
            this.osm_value = osm_value;
            this.nationEleName = nationEleName;
            this.elementType = elementType;
            this.elementTypeID = elementTypeID;
            this.featureID = featureID;
            this.geometry = geometry;
        }
    }
}
