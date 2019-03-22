using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;

//by dy 20180820
namespace GIS.UI.Entity
{
  public class Poly
    {
        private long objectid;
        private long osmid;
        private int version;
        private string starttime;
        private string endtime;
        private string changeset;
        private int userid;
        private string username;
        private string fc;
        private string dsg;
        private string tags;
        private double trustvalue;
        private long userreputation;
        private string pointsId;
        private string shape;
        private int polytype;
        private string wkt;
        private int source;
        private string nationelename;
        private string nationcode;
        private string changetype;
        private int matchid;


        public long getUserreputation()
        {
             return userreputation; 
        }
        public void setUserreputation(long userreputation)
        {
            this.userreputation = userreputation;
        }
        public long getMatchid()
        {
            return matchid;
        }
        public void setMatchid(int matchid)
        {
            this.matchid = matchid;
        }
        public long getObjectid()
        {
            return this.objectid;
        }
        public void setObjectid(long objectid)
        {
            this.objectid = objectid;
        }
        public long getOsmid()
        {
            return this.osmid;
        }
        public void setOsmid(long osmid)
        {
            this.osmid = osmid;
        }
        public int getVersion()
        {
            return this.version;
        }
        public void setVersion(int version)
        {
            this.version = version;
        }
        public string getStartTime()
        {
            return this.starttime;
        }
        public void setStartTime(string starttime)
        {
            this.starttime = starttime;
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

        public string getPointsId()
        {
            return this.pointsId;
        }
        public void setPointsId(string pointsId)
        {
            this.pointsId = pointsId;
        }

        public string getShape()
        {
            return this.shape;
        }
        public void setShape(string shape)
        {
            this.shape = shape;
        }
        public int getPolytype()
        {
            return this.polytype;
        }
        public void setPolytype(int polytype)
        {
            this.polytype = polytype;
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
        public string getNationelename()
        {
            return this.nationelename;
        }
        public void setNationelename(string nationelename)
        {
            this.nationelename = nationelename;
        }
        public string getNationcode()
        {
            return this.nationcode;
        }
        public void setNationcode(string nationcode)
        {
            this.nationcode = nationcode;
        }
        public string getChangetype()
        {
            return this.changetype;
        }
        public void setChangetype(string changetype)
        {
            this.changetype = changetype;
        }
    }
}
