using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;
using GIS.UI.Entity;
using ESRI.ArcGIS.Geometry;

namespace GIS.UI.Entity
{
    public class Relation
    {
        //by dy 于2018.7.5
        public static int relationNum = 0;

        //#region 字段、属性的定义
        //public long  osmid;
        //public int version;
        //public string username;
        //public int  userid;
        //public bool visible;
        //public DateTime timestamp;
        //public string changeset;
        //public string type;
        //public string role;
        //public long refid;
        //public string tag;
        //public string allTag;
        //public string keyTag;
        //public string wkt;
        //public string fc;
        //public string dsg;
        //public string bz;
        //public string name;
        //public string name_en;
        //public string name_zh;
        //#endregion

        #region by dy 修改20180705
        private long objectid;
        private long osmid;
        private double lat;
        private double lon;
        private int version;
        private string starttime;
        private string endtime;
        private string changeset;
        private int userid;
        private string username;
        public bool issimple;
        private string fc;
        private string dsg;
        private string tags;
        private double trustvalue;
        private string shape;
        private string wkt;
        private string id;
        private int source;

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
        public string getId()
        {
            return this.id;
        }
        public void setId(string id)
        {
            this.id = id;
        }
        public int getSource()
        {
            return this.source;
        }
        public void setSource(int source)
        {
            this.source = source;
        }
        #endregion

        /// <summary>
        /// 将Relation导入Oracle数据库
        /// </summary>
        /// <param name="Poly"></param>
        /// <param name="wkt"></param>

        //public static int relationtoOracle(Relation relation, string wkt, OracleConnection con)
        //{
        //    if (con.State != ConnectionState.Open)
        //    {
        //        con.Open();
        //    }
        //    string cmdtext = "INSERT INTO " + "(osmid, \"user\", uid, \"version\", changeset,\"timestamp\",fc, dsg,  tag,a_tag,k_tag,bz, \"name\", name_en, name_zh,area_source ,shape ) VALUES (:osmid, :user, :uid, :version, :changeset,:timestamp,  :fc, :dsg,:tag,:a_tag,:k_tag, :bz, :name,name_en, name_zh, :area_source, GeomFromText(:shape,4326))";
        //    //wkt = "GeomFromText(" + wkt + ",4326)";

        //    using (OracleCommand command = new OracleCommand(cmdtext, con))
        //    {
        //        OracleParameter param = new OracleParameter("osmid", OracleDbType.Varchar2);
        //        param.Value = relation.osmid;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("username", OracleDbType.Varchar2);
        //        param.Value = relation.username;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("userid", OracleDbType.Varchar2);
        //        param.Value = relation.userid;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("version", OracleDbType.Varchar2);
        //        param.Value = (int)relation.version;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("role", OracleDbType.Varchar2);
        //        param.Value = relation.role;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("refid", OracleDbType.Varchar2);
        //        param.Value = relation.refid;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("changeset", OracleDbType.Varchar2);
        //        param.Value = relation.changeset;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("timestamp", OracleDbType.Varchar2);
        //        param.Value = relation.timestamp;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("tag", OracleDbType.Varchar2);
        //        param.Value = relation.tag;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("a_tag", OracleDbType.Varchar2);
        //        param.Value = relation.allTag;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("k_tag", OracleDbType.Varchar2);
        //        param.Value = relation.keyTag;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("area_source", OracleDbType.Varchar2);
        //        param.Value = "relation";
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("shape", OracleDbType.Varchar2);
        //        param.Value = relation.wkt;
        //        command.Parameters.Add(param);

        //        int num = 0;
        //        try
        //        {
        //            num = command.ExecuteNonQuery();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString());
        //        }

        //        return num;
        //    }
        //}
        
    }
}
