using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.UI.Entity
{
    public class OscDataNode
    {
        //osm增量数据Node类   by zbl 2018.7.3
        public static string[] FClass = { "aerialway","aeroway","amenity", "barrier","boundary","building", "craft","emergency","geological",
                                            "highway","historic","landuse", "leisure","man_made","military", "natural","office","place",
                                         "power","public transport","railway","route","shop","sport","tourism", "waterway" };
        #region 字段、属性定义
        public long objectid;
        public long osmid;
        public string username;
        public int userid;
        public double lat;
        public double lon;
        public int versionid;
        public string starttime;
        public string endtime;
        public string changeset;
        public string fc;
        public string dsg;
        public string tags;
        public string shape;
        public double trustvalue;
        public int source;
        public string changeType;
        private string wkt;
        public bool issimple;

        #endregion

        public long getObjectid()
        {
            return this.objectid;
        }
        public void setObjectid(long objectid)
        {
            this.objectid = objectid;
        }
        public long getOscid()
        {
            return this.osmid;
        }
        public void setOscid(long osmid)
        {
            this.osmid = osmid;
        }
        public double getLat()
        {
            return this.lat;
        }
        public void setLat(double lat)
        {
            this.lat = lat;
        }
        public double getLon()
        {
            return this.lon;
        }
        public void setLon(double lon)
        {
            this.lon = lon;
        }
        public int getVersion()
        {
            return this.versionid;
        }
        public void setVersion(int version)
        {
            this.versionid = version;
        }
        public string getStartTime()
        {
            return this.starttime;
        }
        public void setStartTime(string timestamp)
        {
            this.starttime = timestamp;
        }
        public string getEndTime()
        {
            return this.endtime;
        }
        public void setEndTime(string endtime)
        {
            this.endtime = endtime;
        }
        public string getChangeset()
        {
            return this.changeset;
        }
        public void setChangeset(string changeset)
        {
            this.changeset = changeset;
        }
        public int getUserid()
        {
            return this.userid;
        }
        public void setUserid(int userid)
        {
            this.userid = userid;
        }
        public string getUsername()
        {
            return this.username;
        }
        public void setUsername(string username)
        {
            this.username = username;
        }
        public string getFc()
        {
            return this.fc;
        }
        public void setFc(string fc)
        {
            this.fc = fc;
        }
        public string getDsg()
        {
            return this.dsg;
        }
        public void setDsg(string dsg)
        {
            this.dsg = dsg;
        }

        public string getTags()
        {
            return this.tags;
        }
        public void setTags(string tags)
        {
            this.tags = tags;
        }
        public double getTrustvalue()
        {
            return this.trustvalue;
        }
        public void setTrustvalue(double trustvalue)
        {
            this.trustvalue = trustvalue;
        }
        public string getShape()
        {
            return this.shape;
        }
        public void setShape(string shape)
        {
            this.shape = shape;
        }
        public string getWkt()
        {
            return this.wkt;
        }
        public void setWkt(string wkt)
        {
            this.wkt = wkt;
        }
        public string getChangetype()
        {
            return this.changeType;
        }
        public void setChangetype(string changetype)
        {
            this.changeType = changetype;
        }
        public int getSource()
        {
            return this.source;
        }
        public void setSource(int source)
        {
            this.source = source;
        }
    }
}
