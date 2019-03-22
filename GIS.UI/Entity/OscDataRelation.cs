using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.UI.Entity
{
    public class OscDataRelation
    {
        // osm增量数据Relation类  by zbl 2018.7.3
        private long objectid;
        private long osmid;
        private double lat;
        private double lon;
        private int versionid;
        private string starttime;
        private string endtime;
        private string changeset;
        private string changeType;
        private int userid;
        private string username;
        public bool issimple;
        private string fc;
        private string dsg;
        private string tags;
        private double trustvalue;
        private string shape;
        private string wkt;
        private int source;
        private string id;

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
        public void setOscid(long oscid)
        {
            this.osmid = oscid;
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
        public string getChangeType()
        {
            return this.changeType;
        }
        public void setChangeType(string changetype)
        {
            this.changeType = changetype;
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
        public int getSource()
        {
            return this.source;
        }
        public void setSource(int source)
        {
            this.source = source;
        }
        public string getId()
        {
            return this.id;
        }
        public void setId(string id)
        {
            this.id = id;
        }
    }
}
