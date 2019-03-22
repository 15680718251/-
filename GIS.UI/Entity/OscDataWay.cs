using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.UI.Entity
{
    public class OscDataWay
    {
        //osm增量数据Way类  by zbl 2018.7.3
        private long objectid;
        private long osmid;
        private int versionid;
        private string starttime;
        private string endtime;
        private string changeset;
        public string changeType;
        private int userid;
        private string username;
        private string fc;
        private string dsg;
        private string tags;
        private double trustvalue;
        private string pointsId;
        private string shape;
        private int polytype;
        private int source;
        private string wkt;

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
        public int getPolytype()
        {
            return this.polytype;
        }
        public void setPolytype(int polytype)
        {
            this.polytype = polytype;
        }
        public string getPointsld()
        {
            return this.pointsId;
        }
        public void setPointsld(string pointsId)
        {
            this.pointsId = pointsId;
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
