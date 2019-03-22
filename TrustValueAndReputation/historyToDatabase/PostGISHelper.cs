using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using GIS.UI.Forms;
using Oracle.ManagedDataAccess.Client;

namespace TrustValueAndReputation.historyToDatabase
{
    /*
  INSERT INTO nodes(
          osmid, "user", uid, lat, lon, visible, "version", changeset, 
          "timestamp", issimple, fc, dsg, code, gbcode, gbdes, tags, bz, 
          "name", name_en, name_zh, shape)
  VALUES ('10001001', 'smith', '1000998', 116.2,39.9,2, '2223', '2014-04-22', 
          ?, ?, ?, ?, ?, ?, ?, ?, ?, 
          ?, ?, ?, ?);
   * 
   * 
      INSERT INTO ways(
          osmid, "user", uid, visible, "version", changeset, "timestamp", 
          issimple, fc, dsg, code, gbcode, gbdes, tags, bz, "name", name_en, 
          name_zh, shape)
  VALUES (?, ?, ?, ?, ?, ?, ?, 
          ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 
          ?, ?);
   * 
   * 
   INSERT INTO relations(
          osmid, "user", uid, visible, "version", changeset, "timestamp", 
          issimple, fc, dsg, code, gbcode, gbdes, tags, bz, "name", name_en, 
          name_zh, shape)
  VALUES (?, ?, ?, ?, ?, ?, ?, 
          ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 
          ?, ?);

   */
    
    enum GeoRelType
    {
        /// <summary>
        /// way
        /// </summary>
        way,
        /// <summary>
        /// relation
        /// </summary>
        relation
    }
    /// <summary>
    /// 该类主要用于数据的导入
    /// </summary>
    class PostGISHelper
    {
        private string _conString;
        string osmpoint = "osmpoint";
        string osmline = "osmline";
        string osmarea = "osmarea";
        public string conString 
        {
            get { return _conString; }
            set { _conString = value; }
        }
        
       public PostGISHelper(string constr)
       {
           conString = constr;
       }
        /// <summary>
        /// 将node类型导入POSTGIS数据库中
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
       public int node2PostGIS(NODE node, string wkt,  OracleConnection con) 
        {
            string cmdtext = "INSERT INTO "+ImportOsc.osmPointTblName+"(osmid, \"user\", uid, lat, lon, \"version\", changeset,\"timestamp\", fc, dsg, code, gbcode, gbdes, tags, bz, \"name\", name_en, name_zh, shape) VALUES (:osmid, :user, :uid, :lat, :lon, :version, :changeset,:timestamp,  :fc, :dsg, :code, :gbcode, :gbdes, :tags, :bz, :name, :name_en, :name_zh, GeomFromText(:shape,4326))";
            //wkt = "GeomFromText(\""+wkt+"\",4326)";
            using (OracleCommand command = new OracleCommand(cmdtext, con))
            {
                OracleParameter param = new OracleParameter("osmid",  OracleDbType.Varchar2);
                param.Value = node.osmid;
                command.Parameters.Add(param);
                param = new OracleParameter("user", OracleDbType.Varchar2);
                param.Value = node.user;
                command.Parameters.Add(param);
                param = new OracleParameter("uid", OracleDbType.Varchar2);
                param.Value = node.uid;
                command.Parameters.Add(param);
                param = new OracleParameter("lat", OracleDbType.Double);
                param.Value = node.lat;
                command.Parameters.Add(param);
                param = new OracleParameter("lon", OracleDbType.Double);
                param.Value = node.lon;
                command.Parameters.Add(param);
                param = new OracleParameter("version",  OracleDbType.Int16);
                param.Value = (int)node.version;
                command.Parameters.Add(param);
                param = new OracleParameter("changeset", OracleDbType.Varchar2);
                param.Value = node.changeset;
                command.Parameters.Add(param);
                param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                param.Value = node.timestamp;
                command.Parameters.Add(param);
                param = new OracleParameter("fc", OracleDbType.Varchar2);
                param.Value = node.fc;
                command.Parameters.Add(param);
                param = new OracleParameter("dsg", OracleDbType.Varchar2);
                param.Value = node.dsg;
                command.Parameters.Add(param);
                param = new OracleParameter("code", OracleDbType.Char);
                param.Value = node.code;
                command.Parameters.Add(param);
                param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                param.Value = node.gbcode;
                command.Parameters.Add(param);
                param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                param.Value = node.gbdes;
                command.Parameters.Add(param);
                param = new OracleParameter("tags", OracleDbType.Clob);
                param.Value = node.tags;
                command.Parameters.Add(param);
                param = new OracleParameter("bz", OracleDbType.Varchar2);
                param.Value = node.bz;
                command.Parameters.Add(param);
                param = new OracleParameter("name", OracleDbType.Varchar2);
                param.Value = node.name;
                command.Parameters.Add(param);
                param = new OracleParameter("name_en", OracleDbType.Varchar2);
                param.Value = node.name_en;
                command.Parameters.Add(param);
                param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                param.Value = node.name_zh;
                command.Parameters.Add(param);
                param = new OracleParameter("shape", OracleDbType.Varchar2);
                param.Value = wkt;
                command.Parameters.Add(param);
                int num = 0;
                try
                {
                    num = command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    
                    
                }
                return num;
            }
        }
        /// <summary>
        /// 将way导入postgis数据库
        /// </summary>
        /// <param name="way"></param>
        /// <param name="wkt"></param>
        /// <param name="type">type表示way是线，还是面</param>
       public int way2PostGIS(WAY way, string wkt, int type, OracleConnection con)
        {
            //line
            if (type == 0)
            {
                string cmdtext = "INSERT INTO "+ImportOsc.osmLineTblName+"(osmid, \"user\", uid, \"version\", changeset,\"timestamp\", fc, dsg, code, gbcode, gbdes, tags, bz, \"name\", name_en, name_zh, shape) VALUES (:osmid, :user, :uid, :version, :changeset,:timestamp,  :fc, :dsg, :code, :gbcode, :gbdes, :tags, :bz, :name, :name_en, :name_zh, GeomFromText(:shape,4326))";
                //
                using (OracleCommand command = new OracleCommand(cmdtext, con))
                {
                    
                    OracleParameter param = new OracleParameter("osmid", OracleDbType.Varchar2);
                    param.Value = way.osmid;
                    command.Parameters.Add(param);
                    param = new OracleParameter("user", OracleDbType.Varchar2);
                    param.Value = way.user;
                    command.Parameters.Add(param);
                    param = new OracleParameter("uid", OracleDbType.Varchar2);
                    param.Value = way.uid;
                    command.Parameters.Add(param);
                   
                    param = new OracleParameter("version", OracleDbType.Int16);
                    param.Value = (int)way.version;
                    command.Parameters.Add(param);
                    param = new OracleParameter("changeset", OracleDbType.Varchar2);
                    param.Value = way.changeset;
                    command.Parameters.Add(param);
                    param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                    param.Value = way.timestamp;
                    command.Parameters.Add(param);
                    
                    param = new OracleParameter("fc", OracleDbType.Varchar2);
                    param.Value = way.fc;
                    command.Parameters.Add(param);
                    param = new OracleParameter("dsg", OracleDbType.Varchar2);
                    param.Value = way.dsg;
                    command.Parameters.Add(param);
                    param = new OracleParameter("code", OracleDbType.Char);
                    param.Value = way.code;
                    command.Parameters.Add(param);
                    param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                    param.Value = way.gbcode;
                    command.Parameters.Add(param);
                    param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                    param.Value = way.gbdes;
                    command.Parameters.Add(param);
                    param = new OracleParameter("tags", OracleDbType.Clob);
                    param.Value = way.tags;
                    command.Parameters.Add(param);
                    param = new OracleParameter("bz", OracleDbType.Varchar2);
                    param.Value = way.bz;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name", OracleDbType.Varchar2);
                    param.Value = way.name;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name_en", OracleDbType.Varchar2);
                    param.Value = way.name_en;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                    param.Value = way.name_zh;
                    command.Parameters.Add(param);
                    
                    param = new OracleParameter("shape", OracleDbType.Varchar2);
                    param.Value = wkt;
                    command.Parameters.Add(param);
                    int num = 0;
                    try
                    {
                        num = command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {


                    }
                    
                    return num;
                }
            }
            else
            {
                string cmdtext = "INSERT INTO "+ImportOsc.osmAreaTblName+"(osmid, \"user\", uid,  \"version\", changeset,\"timestamp\",  fc, dsg, code, gbcode, gbdes, tags, bz, \"name\", name_en, name_zh, area_source, shape) VALUES (:osmid, :user, :uid,  :version, :changeset,:timestamp, :fc, :dsg, :code, :gbcode, :gbdes, :tags, :bz, :name, :name_en, :name_zh,:area_source, GeomFromText(:shape,4326))";
                //wkt = "GeomFromText(" + wkt + ",4326)";

                
                using (OracleCommand command = new OracleCommand(cmdtext, con))
                {
        
                    OracleParameter param = new OracleParameter("osmid", OracleDbType.Varchar2);
                    param.Value = way.osmid;
                    command.Parameters.Add(param);
                    param = new OracleParameter("user", OracleDbType.Varchar2);
                    param.Value = way.user;
                    command.Parameters.Add(param);
                    param = new OracleParameter("uid", OracleDbType.Varchar2);
                    param.Value = way.uid;
                    command.Parameters.Add(param);
                   
                    param = new OracleParameter("version", OracleDbType.Int16);
                    param.Value = (int)way.version;
                    command.Parameters.Add(param);
                    param = new OracleParameter("changeset", OracleDbType.Varchar2);
                    param.Value = way.changeset;
                    command.Parameters.Add(param);
                    param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                    param.Value = way.timestamp;
                    command.Parameters.Add(param);
                   
                    param = new OracleParameter("fc", OracleDbType.Varchar2);
                    param.Value = way.fc;
                    command.Parameters.Add(param);
                    param = new OracleParameter("dsg", OracleDbType.Varchar2);
                    param.Value = way.dsg;
                    command.Parameters.Add(param);
                    param = new OracleParameter("code", OracleDbType.Char);
                    param.Value = way.code;
                    command.Parameters.Add(param);
                    param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                    param.Value = way.gbcode;
                    command.Parameters.Add(param);
                    param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                    param.Value = way.gbdes;
                    command.Parameters.Add(param);
                    param = new OracleParameter("tags", OracleDbType.Clob);
                    param.Value = way.tags;
                    command.Parameters.Add(param);
                    param = new OracleParameter("bz", OracleDbType.Varchar2);
                    param.Value = way.bz;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name", OracleDbType.Varchar2);
                    param.Value = way.name;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name_en", OracleDbType.Varchar2);
                    param.Value = way.name_en;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                    param.Value = way.name_zh;
                    command.Parameters.Add(param);
                    param = new OracleParameter("area_source", OracleDbType.Varchar2);
                    param.Value = "way";
                    command.Parameters.Add(param);
                    param = new OracleParameter("shape", OracleDbType.Varchar2);
                    param.Value = wkt;
                    command.Parameters.Add(param);
                    int num = 0;
                    try
                    {
                        num = command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {


                    }
                    
                    return num;
                }
            }
        }
        /// <summary>
        /// 将relation导入postgis数据库
        /// </summary>
        /// <param name="way"></param>
        /// <param name="wkt"></param>
       public int relation2PostGIS(RELATION relation, string wkt, OracleConnection con)
        {
            string cmdtext = "INSERT INTO " + ImportOsc.osmAreaTblName + "(osmid, \"user\", uid, \"version\", changeset,\"timestamp\",fc, dsg, code, gbcode, gbdes, tags, bz, \"name\", name_en, name_zh,area_source ,shape ) VALUES (:osmid, :user, :uid, :version, :changeset,:timestamp,  :fc, :dsg, :code, :gbcode, :gbdes, :tags, :bz, :name, :name_en, :name_zh,:area_source, GeomFromText(:shape,4326))";
            //wkt = "GeomFromText(" + wkt + ",4326)";

            
            using (OracleCommand command = new OracleCommand(cmdtext, con))
            {
                
                OracleParameter param = new OracleParameter("osmid", OracleDbType.Varchar2);
                param.Value = relation.osmid;
                command.Parameters.Add(param);
                param = new OracleParameter("user", OracleDbType.Varchar2);
                param.Value = relation.user;
                command.Parameters.Add(param);
                param = new OracleParameter("uid", OracleDbType.Varchar2);
                param.Value = relation.uid;
                command.Parameters.Add(param);
                
                param = new OracleParameter("version", OracleDbType.Int16);
                param.Value = (int)relation.version;
                command.Parameters.Add(param);
                param = new OracleParameter("changeset", OracleDbType.Varchar2);
                param.Value = relation.changeset;
                command.Parameters.Add(param);
                param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                param.Value = relation.timestamp;
                command.Parameters.Add(param);
               
                param = new OracleParameter("fc", OracleDbType.Varchar2);
                param.Value = relation.fc;
                command.Parameters.Add(param);
                param = new OracleParameter("dsg", OracleDbType.Varchar2);
                param.Value = relation.dsg;
                command.Parameters.Add(param);
                param = new OracleParameter("code", OracleDbType.Char);
                param.Value = relation.code;
                command.Parameters.Add(param);
                param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                param.Value = relation.gbcode;
                command.Parameters.Add(param);
                param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                param.Value = relation.gbdes;
                command.Parameters.Add(param);
                param = new OracleParameter("tags", OracleDbType.Clob);
                param.Value = relation.tags;
                command.Parameters.Add(param);
                param = new OracleParameter("bz", OracleDbType.Varchar2);
                param.Value = relation.bz;
                command.Parameters.Add(param);
                param = new OracleParameter("name", OracleDbType.Varchar2);
                param.Value = relation.name;
                command.Parameters.Add(param);
                param = new OracleParameter("name_en", OracleDbType.Varchar2);
                param.Value = relation.name_en;
                command.Parameters.Add(param);
                param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                param.Value = relation.name_zh;
                command.Parameters.Add(param);
                param = new OracleParameter("area_source", OracleDbType.Varchar2);
                param.Value = "relation";
                command.Parameters.Add(param);
                param = new OracleParameter("shape", OracleDbType.Varchar2);
                param.Value = wkt;
                command.Parameters.Add(param);
                int num = 0;
                try
                {
                    num = command.ExecuteNonQuery();
                }
                catch (Exception)
                {


                }
                
                return num;
            }
        }
        /// <summary>
        /// 根据way生成相应的geo
        /// </summary>
        /// <param name="osmids"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public GIS.Geometries.Geometry buildway(List<string> osmids) 
        {
            if (osmids.Count()<1)
            {
                return null;
            }

            PostGIS pg = new PostGIS(conString, ImportOsc.osmPointTblName, "shape", "oid", "osmid");
            if (osmids[0] == osmids[osmids.Count - 1])
            {
                GeoPolygon ply = new GeoPolygon();
                for (int i = 0; i < osmids.Count; i++)
                {
                    Geometry geom = pg.GetGeometryByKey(osmids[i]);
                    if (geom==null)
                    {
                        continue;
                    }
                   // GeoPoint pt = pg.GetGeometryByKey(osmids[i]) as GeoPoint;
                    ply.ExteriorRing.Vertices.Add(geom as GeoPoint);
                }
                if (ply.ExteriorRing.NumPoints<=3)
                {
                    return null;
                }
                try
                {
                    ply.ExteriorRing.MakeClosed();
                }
                catch (Exception)
                {

                    return null;
                }
                pg.Dispose();
                return ply;
               
            }
            else
            {
                GeoLineString line = new GeoLineString();
                for (int i = 0; i < osmids.Count; i++)
                {
                    Geometry geom = pg.GetGeometryByKey(osmids[i]);
                    if (geom==null)
                    {
                        continue;
                    }
                    line.Vertices.Add(geom as GeoPoint);
                }
                if (line.NumPoints<2)
                {
                    return null;
                }
                pg.Dispose();
                return line;
               
            }
                
           
        }
        /// <summary>
        /// 对relation生成相应的面
        /// </summary>
        /// <param name="outers">外环</param>
        /// <param name="inners">内环</param>
        /// <returns></returns>
        public Geometry buildRelation(List<string> outers,List<string> inners) 
        {
            if (outers.Count==0)
            {
                return null;
            }
            PostGIS wayPg = new PostGIS(conString,ImportOsc.osmLineTblName, "shape", "oid", "osmid");
            PostGIS relatinPg = new PostGIS(conString, ImportOsc.osmAreaTblName, "shape", "oid", "osmid");
            GeoPolygon ply = new GeoPolygon();
            for (int i = 0; i < outers.Count; i++)
            {
                string outer = outers[i];
                Geometry geo = wayPg.GetGeometryByKey(outer);
                if (geo!=null)
                {
                    GeoLineString geoline=geo as GeoLineString;
                    for (int j = 0; j < geoline.NumPoints; j++)
                    {
                        ply.ExteriorRing.Vertices.Add(geoline.Vertices[j]);
                    }
                }
                else
                {
                    geo = relatinPg.GetGeometryByKey(outer);
                    if (geo!=null)
                    {
                        GeoPolygon plygeo = geo as GeoPolygon;
                        ply.ExteriorRing = plygeo.ExteriorRing;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            for (int i = 0; i < inners.Count; i++)
            {
                string inner = inners[i];
                Geometry geom = relatinPg.GetGeometryByKey(inner);
                if (geom!=null)
                {
                    GeoPolygon plygeo=geom as GeoPolygon;
                    ply.InteriorRings.Add(plygeo.ExteriorRing);
                }
            }
            wayPg.Dispose();
            relatinPg.Dispose();
            try
            {
                ply.ExteriorRing.MakeClosed();
            }
            catch (Exception)
            {

                return null;
            }
            return ply;
          
        }
        public int updateNode(NODE node,string wkt)
        {
            string cmdtext = "UPDATE nodes SET \"user\"=:user, uid=:uid, lat=:lat, lon=:lon, visible=:visible, \"version\"=:version,changeset=:changeset, \"timestamp\"=:timestamp, issimple=:issimple, fc=:fc, dsg=:dsg, code=:code,gbcode=:gbcode, gbdes=:gbdes, tags=:tags, bz=:bz, \"name\"=:name, name_en=:name_en, name_zh=:name_zh,shape=GeomFromText(:shape,4326) WHERE osmid='"+node.osmid+"'";
            //wkt = "GeomFromText(\""+wkt+"\",4326)";

            using (OracleConnection con = new OracleConnection(conString))
            using (OracleCommand command = new OracleCommand(cmdtext, con))
            {
                con.Open();
                OracleParameter 
                param = new OracleParameter("user", OracleDbType.Varchar2);
                param.Value = node.user;
                command.Parameters.Add(param);
                param = new OracleParameter("uid", OracleDbType.Varchar2);
                param.Value = node.uid;
                command.Parameters.Add(param);
                param = new OracleParameter("lat", OracleDbType.Double);
                param.Value = node.lat;
                command.Parameters.Add(param);
                param = new OracleParameter("lon", OracleDbType.Double);
                param.Value = node.lon;
                command.Parameters.Add(param);
                param = new OracleParameter("visible", OracleDbType.Int16);
                param.Value = node.visible;
                command.Parameters.Add(param);
                param = new OracleParameter("version", OracleDbType.Int16);
                param.Value = (int)node.version;
                command.Parameters.Add(param);
                param = new OracleParameter("changeset", OracleDbType.Varchar2);
                param.Value = node.changeset;
                command.Parameters.Add(param);
                param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                param.Value = node.timestamp;
                command.Parameters.Add(param);
                param = new OracleParameter("issimple", OracleDbType.Int16);
                param.Value = node.issimple;
                command.Parameters.Add(param);
                param = new OracleParameter("fc", OracleDbType.Varchar2);
                param.Value = node.fc;
                command.Parameters.Add(param);
                param = new OracleParameter("dsg", OracleDbType.Varchar2);
                param.Value = node.dsg;
                command.Parameters.Add(param);
                param = new OracleParameter("code", OracleDbType.Char);
                param.Value = node.code;
                command.Parameters.Add(param);
                param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                param.Value = node.gbcode;
                command.Parameters.Add(param);
                param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                param.Value = node.gbdes;
                command.Parameters.Add(param);
                param = new OracleParameter("tags", OracleDbType.Clob);
                param.Value = node.tags;
                command.Parameters.Add(param);
                param = new OracleParameter("bz", OracleDbType.Varchar2);
                param.Value = node.bz;
                command.Parameters.Add(param);
                param = new OracleParameter("name", OracleDbType.Varchar2);
                param.Value = node.name;
                command.Parameters.Add(param);
                param = new OracleParameter("name_en", OracleDbType.Varchar2);
                param.Value = node.name_en;
                command.Parameters.Add(param);
                param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                param.Value = node.name_zh;
                command.Parameters.Add(param);
                param = new OracleParameter("shape", OracleDbType.Varchar2);
                param.Value = wkt;
                command.Parameters.Add(param);
                int num = 0;
                try
                {
                    num = command.ExecuteNonQuery();
                }
                catch (Exception)
                {


                }
                con.Close();
                return num;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="way"></param>
        /// <param name="wkt"></param>
        /// <param name="type">0=line,1=polygon</param>
        /// <returns></returns>
        public int updateWay(WAY way,string wkt,int type) 
        {
            if (type == 0)
            {
                string cmdtext = "UPDATE ways SET \"user\"=:user, uid=:uid, visible=:visible, \"version\"=:version,changeset=:changeset, \"timestamp\"=:timestamp, issimple=:issimple, fc=:fc, dsg=:dsg, code=:code,gbcode=:gbcode, gbdes=:gbdes, tags=:tags, bz=:bz, \"name\"=:name, name_en=:name_en, name_zh=:name_zh,shape=GeomFromText(:shape,4326) WHERE osmid='" + way.osmid + "'";
                //wkt = "GeomFromText(" + wkt + ",4326)";

                using (OracleConnection con = new OracleConnection(conString))
                using (OracleCommand command = new OracleCommand(cmdtext, con))
                {
                    con.Open();
                    OracleParameter 
                    param = new OracleParameter("user", OracleDbType.Varchar2);
                    param.Value = way.user;
                    command.Parameters.Add(param);
                    param = new OracleParameter("uid", OracleDbType.Varchar2);
                    param.Value = way.uid;
                    command.Parameters.Add(param);
                    param = new OracleParameter("visible", OracleDbType.Int16);
                    param.Value = way.visible;
                    command.Parameters.Add(param);
                    param = new OracleParameter("version", OracleDbType.Int16);
                    param.Value = (int)way.version;
                    command.Parameters.Add(param);
                    param = new OracleParameter("changeset", OracleDbType.Varchar2);
                    param.Value = way.changeset;
                    command.Parameters.Add(param);
                    param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                    param.Value = way.timestamp;
                    command.Parameters.Add(param);
                    param = new OracleParameter("issimple", OracleDbType.Int16);
                    param.Value = way.issimple;
                    command.Parameters.Add(param);
                    param = new OracleParameter("fc", OracleDbType.Varchar2);
                    param.Value = way.fc;
                    command.Parameters.Add(param);
                    param = new OracleParameter("dsg", OracleDbType.Varchar2);
                    param.Value = way.dsg;
                    command.Parameters.Add(param);
                    param = new OracleParameter("code", OracleDbType.Char);
                    param.Value = way.code;
                    command.Parameters.Add(param);
                    param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                    param.Value = way.gbcode;
                    command.Parameters.Add(param);
                    param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                    param.Value = way.gbdes;
                    command.Parameters.Add(param);
                    param = new OracleParameter("tags", OracleDbType.Clob);
                    param.Value = way.tags;
                    command.Parameters.Add(param);
                    param = new OracleParameter("bz", OracleDbType.Varchar2);
                    param.Value = way.bz;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name", OracleDbType.Varchar2);
                    param.Value = way.name;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name_en", OracleDbType.Varchar2);
                    param.Value = way.name_en;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                    param.Value = way.name_zh;
                    command.Parameters.Add(param);
                    param = new OracleParameter("shape", OracleDbType.Varchar2);
                    param.Value = wkt;
                    command.Parameters.Add(param);
                    int num = 0;
                    try
                    {
                        num = command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {


                    }
                    con.Close();
                    return num;
                }
            }
            else
            {
                string cmdtext = "UPDATE relations SET \"user\"=:user, uid=:uid, visible=:visible, \"version\"=:version,changeset=:changeset, \"timestamp\"=:timestamp, issimple=:issimple, fc=:fc, dsg=:dsg, code=:code,gbcode=:gbcode, gbdes=:gbdes, tags=:tags, bz=:bz, \"name\"=:name, name_en=:name_en, name_zh=:name_zh,shape=GeomFromText(:shape,4326) WHERE osmid='" + way.osmid + "'";
                //wkt = "GeomFromText(" + wkt + ",4326)";

                using (OracleConnection con = new OracleConnection(conString))
                using (OracleCommand command = new OracleCommand(cmdtext, con))
                {
                    con.Open();
                    OracleParameter
                    param = new OracleParameter("user", OracleDbType.Varchar2);
                    param.Value = way.user;
                    command.Parameters.Add(param);
                    param = new OracleParameter("uid", OracleDbType.Varchar2);
                    param.Value = way.uid;
                    command.Parameters.Add(param);
                    param = new OracleParameter("visible", OracleDbType.Int16);
                    param.Value = way.visible;
                    command.Parameters.Add(param);
                    param = new OracleParameter("version", OracleDbType.Int16);
                    param.Value = (int)way.version;
                    command.Parameters.Add(param);
                    param = new OracleParameter("changeset", OracleDbType.Varchar2);
                    param.Value = way.changeset;
                    command.Parameters.Add(param);
                    param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                    param.Value = way.timestamp;
                    command.Parameters.Add(param);
                    param = new OracleParameter("issimple", OracleDbType.Int16);
                    param.Value = way.issimple;
                    command.Parameters.Add(param);
                    param = new OracleParameter("fc", OracleDbType.Varchar2);
                    param.Value = way.fc;
                    command.Parameters.Add(param);
                    param = new OracleParameter("dsg", OracleDbType.Varchar2);
                    param.Value = way.dsg;
                    command.Parameters.Add(param);
                    param = new OracleParameter("code", OracleDbType.Char);
                    param.Value = way.code;
                    command.Parameters.Add(param);
                    param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                    param.Value = way.gbcode;
                    command.Parameters.Add(param);
                    param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                    param.Value = way.gbdes;
                    command.Parameters.Add(param);
                    param = new OracleParameter("tags", OracleDbType.Clob);
                    param.Value = way.tags;
                    command.Parameters.Add(param);
                    param = new OracleParameter("bz", OracleDbType.Varchar2);
                    param.Value = way.bz;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name", OracleDbType.Varchar2);
                    param.Value = way.name;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name_en", OracleDbType.Varchar2);
                    param.Value = way.name_en;
                    command.Parameters.Add(param);
                    param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                    param.Value = way.name_zh;
                    command.Parameters.Add(param);
                    param = new OracleParameter("shape", OracleDbType.Varchar2);
                    param.Value = wkt;
                    command.Parameters.Add(param);
                    int num = 0;
                    try
                    {
                        num = command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {


                    }
                    con.Close();
                    return num;
                }
            }
        }
        /// <summary>
        /// 更新relation
        /// </summary>
        /// <param name="relation"></param>
        /// <param name="wkt"></param>
        /// <returns></returns>
        public int updateRelation(RELATION relation, string wkt) 
        {
            string cmdtext = "UPDATE relations SET \"user\"=:user, uid=:uid, visible=:visible, \"version\"=:version,changeset=:changeset, \"timestamp\"=:timestamp, issimple=:issimple, fc=:fc, dsg=:dsg, code=:code,gbcode=:gbcode, gbdes=:gbdes, tags=?, bz=?, \"name\"=:name, name_en=:name_en, name_zh=:name_zh,shape=GeomFromText(:shape,4326) WHERE osmid='" + relation.osmid + "'";
            //wkt = "GeomFromText(" + wkt + ",4326)";

            using (OracleConnection con = new OracleConnection(conString))
            using (OracleCommand command = new OracleCommand(cmdtext, con))
            {
                con.Open();
                OracleParameter 
                param = new OracleParameter("user", OracleDbType.Varchar2);
                param.Value = relation.user;
                command.Parameters.Add(param);
                param = new OracleParameter("uid", OracleDbType.Varchar2);
                param.Value = relation.uid;
                command.Parameters.Add(param);
                param = new OracleParameter("visible", OracleDbType.Int16);
                param.Value = relation.visible;
                command.Parameters.Add(param);
                param = new OracleParameter("version", OracleDbType.Int16);
                param.Value = (int)relation.version;
                command.Parameters.Add(param);
                param = new OracleParameter("changeset", OracleDbType.Varchar2);
                param.Value = relation.changeset;
                command.Parameters.Add(param);
                param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                param.Value = relation.timestamp;
                command.Parameters.Add(param);
                param = new OracleParameter("issimple", OracleDbType.Int16);
                param.Value = relation.issimple;
                command.Parameters.Add(param);
                param = new OracleParameter("fc", OracleDbType.Varchar2);
                param.Value = relation.fc;
                command.Parameters.Add(param);
                param = new OracleParameter("dsg", OracleDbType.Varchar2);
                param.Value = relation.dsg;
                command.Parameters.Add(param);
                param = new OracleParameter("code", OracleDbType.Char);
                param.Value = relation.code;
                command.Parameters.Add(param);
                param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                param.Value = relation.gbcode;
                command.Parameters.Add(param);
                param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                param.Value = relation.gbdes;
                command.Parameters.Add(param);
                param = new OracleParameter("tags", OracleDbType.Clob);
                param.Value = relation.tags;
                command.Parameters.Add(param);
                param = new OracleParameter("bz", OracleDbType.Varchar2);
                param.Value = relation.bz;
                command.Parameters.Add(param);
                param = new OracleParameter("name", OracleDbType.Varchar2);
                param.Value = relation.name;
                command.Parameters.Add(param);
                param = new OracleParameter("name_en", OracleDbType.Varchar2);
                param.Value = relation.name_en;
                command.Parameters.Add(param);
                param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                param.Value = relation.name_zh;
                command.Parameters.Add(param);
                param = new OracleParameter("shape", OracleDbType.Varchar2);
                param.Value = wkt;
                command.Parameters.Add(param);
                int num = 0;
                try
                {
                    num = command.ExecuteNonQuery();
                }
                catch (Exception)
                {


                }
                con.Close();
                return num;
            }
        }
        /// <summary>
        /// delete node
        /// </summary>
        /// <param name="osmid"></param>
        /// <param name="type">0=node,1=line,2=polygon</param>
        /// <returns></returns>
        public int delete(string osmid,int type) 
        {
            string cmdtext = null;
            if (type==0)
            {
                cmdtext = "DELETE FROM nodes WHERE osmid='"+osmid+"'";
            }
            else if (type==1)
            {
                cmdtext = "DELETE FROM ways WHERE osmid='" + osmid + "'";
            }
            else if (type==2)
            {
                cmdtext = "DELETE FROM relations WHERE osmid='"+osmid+"'";
            }
            else
            {
                return 0;
            }
            
            using(OracleConnection con=new OracleConnection(conString))
            using(OracleCommand command=new OracleCommand(cmdtext,con))
            {
                con.Open();
                int num = 0;
                try
                {
                    num = command.ExecuteNonQuery();
                }
                catch (Exception)
                {


                }
                con.Clone();
                return num;
            }
        }
        
    }
    
}
