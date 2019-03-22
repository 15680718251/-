using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.UI.ModelTranByOSM
{
    //***********by dy20180806
  public class RuleItem
    {
        private string osm_key;
        private string osm_value;
        private string nationcode;
        private string nationelename;
        private int objectid;
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

        public string NationCode
        {
            get { return nationcode; }
            set { nationcode = value; }        
        }
        public string NationEleName
        {
            get { return nationelename; }
            set { nationelename = value; }
        }
        //public int objectId 
        //{
        //    get { return objectid; }
        //    set { objectid = value; }
        //}
        public RuleItem() { }
        public RuleItem(string osm_key, string osm_value,string nationcode,string nationelename)
        {
            //this.objectid = objectid;
            this.osm_key = osm_key;
            this.osm_value = osm_value;
            this.nationcode = nationcode;
            this.nationelename = nationelename;
        }
    }
}
