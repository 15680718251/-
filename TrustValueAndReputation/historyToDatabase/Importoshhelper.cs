using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using GIS.UI.Forms;
using GIS.Geometries;
using GIS.UI.AdditionalTool;

namespace TrustValueAndReputation.historyToDatabase
{
    
 
    /// <summary>
    /// 该类主要用于数据的导入
    /// </summary>
    class Importoshhelper
    {
        private string _conString;

        public string conString 
        {
            get { return _conString; }
            set { _conString = value; }
        }
        
       public Importoshhelper(string constr)
       {
           conString = constr;
       }
        /// <summary>
        /// 将node类型导入POSTGIS数据库中
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public  int node2PostGIS(NODE node,string wkt) 
        {
            string cmdtext = String.Format("INSERT INTO point(osmid,username,userid,versionid,changeset,starttime,tags,fc,dsg,lon,lat,shape)VALUES ({0}, '{1}', {2}, {3}, '{4}', '{5}','{6}', '{7}', '{8}','{9}','{10}',sdo_geometry ('{11}',4326))",
               node.osmid, node.user, node.uid,  (int)node.version, node.changeset, node.timestamp, node.tags,  node.fc, node.dsg,  node.lon, node.lat, wkt);
            using(OracleConnection con = new OracleConnection(conString))
            using (OracleCommand command = new OracleCommand(cmdtext,con))
            {
                con.Open();
               // OracleParameter param = new OracleParameter("id",OracleDbType.Int64);
               // param.Value = node.osmid;
               // command.Parameters.Add(param);

               // param = new OracleParameter("\"username\"", OracleDbType.Varchar2);
               // param.Value = node.user;
               // command.Parameters.Add(param);

               // param = new OracleParameter("usreid",OracleDbType.Int64);
               // param.Value = node.uid;
               // command.Parameters.Add(param);

               // param = new OracleParameter("visible", OracleDbType.Int16);
               // param.Value = node.visible;
               // command.Parameters.Add(param);

               // param = new OracleParameter("version", OracleDbType.Int16);
               // param.Value = (int)node.version;
               // command.Parameters.Add(param);

               // param = new OracleParameter("changeset", OracleDbType.Varchar2);
               // param.Value = node.changeset;
               // command.Parameters.Add(param);

               // param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
               // param.Value = node.timestamp;
               // command.Parameters.Add(param);

               // //param = new OracleParameter("issimple", OracleDbType.Int16);
               // //param.Value = polyline.issimple;
               // //command.Parameters.Add(param);
               // //param = new OracleParameter("fc", OracleDbType.Varchar2);
               // //param.Value = polyline.fc;
               // //command.Parameters.Add(param);

               // //param.Value = way.nd ref;
               // //command.Parameters.Add(param);
               // param = new OracleParameter("code", OracleDbType.Varchar2);
               // param.Value = node.code;
               // command.Parameters.Add(param);

               // //param = new OracleParameter("gbcode", OracleDbType.Varchar2);
               // //param.Value = polyline.gbcode;
               // //command.Parameters.Add(param);
               // //param = new OracleParameter("gbdes", OracleDbType.Varchar2);
               // //param.Value = polyline.gbdes;
               // //command.Parameters.Add(param);

               // param = new OracleParameter("tags", OracleDbType.Clob);
               // param.Value = node.tags;
               // command.Parameters.Add(param);

               

               // param = new OracleParameter("bz", OracleDbType.Varchar2);
               // param.Value = node.bz;
               // command.Parameters.Add(param);

               // param = new OracleParameter("name", OracleDbType.Varchar2);
               // param.Value = node.name;
               // command.Parameters.Add(param);

               // param = new OracleParameter("name_en", OracleDbType.Varchar2);
               // param.Value = node.name_en;
               // command.Parameters.Add(param);

               // param = new OracleParameter("name_zh", OracleDbType.Varchar2);
               // param.Value = node.name_zh;
               // command.Parameters.Add(param);

               // param = new OracleParameter("fc", OracleDbType.Varchar2);
               // param.Value = node.fc;
               // command.Parameters.Add(param);

               // param = new OracleParameter("dsg", OracleDbType.Varchar2);
               // param.Value = node.dsg;
               // command.Parameters.Add(param);
               // param = new OracleParameter("issimple", OracleDbType.Int16);
               // param.Value = node.issimple;
               // command.Parameters.Add(param);

               // param = new OracleParameter("lon", OracleDbType.Varchar2);
               // param.Value = node.lon;
               // command.Parameters.Add(param);

               // param = new OracleParameter("lat", OracleDbType.Varchar2);
               // param.Value = node.lat;
               // command.Parameters.Add(param);

               //param = new OracleParameter("geom", OracleDbType.Varchar2);
               // param.Value = wkt;
               // command.Parameters.Add(param);
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
        /// 将way导入postgis数据库
        /// </summary>
        /// <param name="way"></param>
        /// <param name="wkt"></param>
        /// <param name="type">type表示way是线，还是面</param>
        public int way2PostGIS(WAY way,string wkt,int type)
        {
            //line
            if (type == 0)
            {
                string cmdtext = String.Format("INSERT INTO polyline(osmid,username,userid, versionid, changeset,starttime, tags,pointsid,fc,dsg,shape)VALUES ({0}, '{1}', {2}, {3}, '{4}', '{5}','{6}', '{7}', '{8}','{9}',sdo_geometry ('{10}',4326))",
                                                               way.osmid, way.user, way.uid, (int)way.version, way.changeset, way.timestamp,  way.tags, way.pointids,  way.fc, way.dsg ,wkt);
                //wkt = "GeomFromText(" + wkt + ",4326)";

                using (OracleConnection con = new OracleConnection(conString))
                using (OracleCommand command = new OracleCommand(cmdtext, con))
                {
                    con.Open();
                    //OracleParameter param = new OracleParameter("id",OracleDbType.Int64);
                    //param.Value = way.osmid;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("\"username\"", OracleDbType.Varchar2);
                    //param.Value = way.user;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("usreid",OracleDbType.Int64);
                    //param.Value = way.uid;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("visible", OracleDbType.Int16);
                    //param.Value = way.visible;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("version", OracleDbType.Int16);
                    //param.Value = (int)way.version;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("changeset", OracleDbType.Varchar2);
                    //param.Value = way.changeset;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                    //param.Value = way.timestamp;
                    //command.Parameters.Add(param);

                    ////param = new OracleParameter("issimple", OracleDbType.Int16);
                    ////param.Value = polyline.issimple;
                    ////command.Parameters.Add(param);
                    ////param = new OracleParameter("fc", OracleDbType.Varchar2);
                    ////param.Value = polyline.fc;
                    ////command.Parameters.Add(param);

                    ////param.Value = way.nd ref;
                    ////command.Parameters.Add(param);
                    //param = new OracleParameter("code", OracleDbType.Varchar2);
                    //param.Value = way.code;
                    //command.Parameters.Add(param);

                    ////param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                    ////param.Value = polyline.gbcode;
                    ////command.Parameters.Add(param);
                    ////param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                    ////param.Value = polyline.gbdes;
                    ////command.Parameters.Add(param);

                    //param = new OracleParameter("tags", OracleDbType.Clob);
                    //param.Value = way.tags;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("pointids", OracleDbType.Varchar2);
                    //param.Value = way.pointids;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("bz", OracleDbType.Varchar2);
                    //param.Value = way.bz;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("name", OracleDbType.Varchar2);
                    //param.Value = way.name;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("name_en", OracleDbType.Varchar2);
                    //param.Value = way.name_en;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                    //param.Value = way.name_zh;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("fc", OracleDbType.Varchar2);
                    //param.Value = way.fc;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("dsg", OracleDbType.Varchar2);
                    //param.Value = way.dsg;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("issimple", OracleDbType.Int16);
                    //param.Value = way.issimple;
                    //command.Parameters.Add(param);
                    ////param = new OracleParameter("shape", OracleDbType.Varchar2);
                    ////param.Value = wkt;
                    ////command.Parameters.Add(param);
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
                string cmdtext = String.Format("INSERT INTO polygon(osmid,username,userid,versionid, changeset,starttime, tags,pointsid,fc,dsg,shape)VALUES ({0}, '{1}', '{2}', {3}, '{4}', '{5}','{6}', '{7}', '{8}','{9}',sdo_geometry ('{10}',4326))",
                 way.osmid, way.user, way.uid, (int)way.version, way.changeset, way.timestamp, way.tags, way.pointids,  way.fc, way.dsg, wkt);
                //wkt = "GeomFromText(" + wkt + ",4326)";
                //string cmdtext = String.Format("INSERT INTO polyline(osmid,username,userid, versionid, changeset,timestamps, tags,pointsid,fc,dsg,shape)VALUES ({0}, '{1}', {2}, {3}, '{4}', '{5}','{6}', '{7}', '{8}','{9}',sdo_geometry ('{10}',4326))",
                //                                               way.osmid, way.user, way.uid, (int)way.version, way.changeset, way.timestamp, way.tags, way.pointids, way.fc, way.dsg, wkt);

                using (OracleConnection con = new OracleConnection(conString))
                using (OracleCommand command = new OracleCommand(cmdtext, con))
                {

                    con.Open();
                    //OracleParameter param = new OracleParameter("id",OracleDbType.Int64);
                    //param.Value = way.osmid;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("\"username\"", OracleDbType.Varchar2);
                    //param.Value = way.user;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("usreid",OracleDbType.Int64);
                    //param.Value = way.uid;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("visible", OracleDbType.Int16);
                    //param.Value = way.visible;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("version", OracleDbType.Int16);
                    //param.Value = (int)way.version;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("changeset", OracleDbType.Varchar2);
                    //param.Value = way.changeset;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                    //param.Value = way.timestamp;
                    //command.Parameters.Add(param);

                    ////param = new OracleParameter("issimple", OracleDbType.Int16);
                    ////param.Value = polyline.issimple;
                    ////command.Parameters.Add(param);
                    ////param = new OracleParameter("fc", OracleDbType.Varchar2);
                    ////param.Value = polyline.fc;
                    ////command.Parameters.Add(param);
                    
                    ////param.Value = way.nd ref;
                    ////command.Parameters.Add(param);
                    //param = new OracleParameter("code", OracleDbType.Varchar2);
                    //param.Value = way.code;
                    //command.Parameters.Add(param);

                    ////param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                    ////param.Value = polyline.gbcode;
                    ////command.Parameters.Add(param);
                    ////param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                    ////param.Value = polyline.gbdes;
                    ////command.Parameters.Add(param);

                    //param = new OracleParameter("tags", OracleDbType.Clob);
                    //param.Value = way.tags;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("pointids", OracleDbType.Varchar2);
                    //param.Value = way.pointids;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("bz", OracleDbType.Varchar2);
                    //param.Value = way.bz;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("name", OracleDbType.Varchar2);
                    //param.Value = way.name;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("name_en", OracleDbType.Varchar2);
                    //param.Value = way.name_en;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                    //param.Value = way.name_zh;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("fc", OracleDbType.Varchar2);
                    //param.Value = way.fc;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("dsg", OracleDbType.Varchar2);
                    //param.Value = way.dsg;
                    //command.Parameters.Add(param);

                    //param = new OracleParameter("issimple", OracleDbType.Int16);
                    //param.Value = way.issimple;
                    //command.Parameters.Add(param);
                    ////param = new OracleParameter("shape", OracleDbType.Varchar2);
                    ////param.Value = wkt;
                    ////command.Parameters.Add(param);
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
        /// 将relation导入postgis数据库
        /// </summary>
        /// <param name="way"></param>
        /// <param name="wkt"></param>
        public int relation2PostGIS(RELATION relation, string wkt)
        {
            string cmdtext = "INSERT INTO polygon(osmid, username,userid, lat, lon,versionid, changeset,starttime,pointsid, code, tags,shape)VALUES (:osmid, :user, :uid, :lat, :lon, :version, :changeset,:timestamp,:nd ref :code, :tags, sdo_geometry(:shape,4326))";
            //wkt = "GeomFromText(" + wkt + ",4326)";

            using (OracleConnection con = new OracleConnection(conString))
            using (OracleCommand command = new OracleCommand(cmdtext, con))
            {

                con.Open();
                //OracleParameter param = new OracleParameter("id", OracleDbType.Varchar2);
                //param.Value = relation.osmid;
                //command.Parameters.Add(param);

                //param = new OracleParameter("username", OracleDbType.Varchar2);
                //param.Value = relation.user;
                //command.Parameters.Add(param);

                //param = new OracleParameter("usreid", OracleDbType.Varchar2);
                //param.Value = relation.uid;
                //command.Parameters.Add(param);

                //param = new OracleParameter("visible", OracleDbType.Int16);
                //param.Value = relation.visible;
                //command.Parameters.Add(param);

                //param = new OracleParameter("version", OracleDbType.Int16);
                //param.Value = (int)relation.version;
                //command.Parameters.Add(param);

                //param = new OracleParameter("changeset", OracleDbType.Varchar2);
                //param.Value = relation.changeset;
                //command.Parameters.Add(param);

                //param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                //param.Value = relation.timestamp;
                //command.Parameters.Add(param);

                ////param = new OracleParameter("issimple", OracleDbType.Int16);
                ////param.Value = polyline.issimple;
                ////command.Parameters.Add(param);
                ////param = new OracleParameter("fc", OracleDbType.Varchar2);
                ////param.Value = polyline.fc;
                ////command.Parameters.Add(param);
                ////param = new OracleParameter("pointids", OracleDbType.Varchar2);
                ////param.Value = polyline.id;
                ////command.Parameters.Add(param);

                //param = new OracleParameter("code", OracleDbType.Varchar2);
                //param.Value = relation.code;
                //command.Parameters.Add(param);

                ////param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                ////param.Value = polyline.gbcode;
                ////command.Parameters.Add(param);
                ////param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                ////param.Value = polyline.gbdes;
                ////command.Parameters.Add(param);

                //param = new OracleParameter("tags", OracleDbType.Clob);
                //param.Value = relation.tags;
                //command.Parameters.Add(param);

                ////param = new OracleParameter("bz", OracleDbType.Varchar2);
                ////param.Value = polyline.bz;
                ////command.Parameters.Add(param);
                ////param = new OracleParameter("name", OracleDbType.Varchar2);
                ////param.Value = polyline.name;
                ////command.Parameters.Add(param);
                ////param = new OracleParameter("name_en", OracleDbType.Varchar2);
                ////param.Value = polyline.name_en;
                ////command.Parameters.Add(param);
                ////param = new OracleParameter("name_zh", OracleDbType.Varchar2);
                ////param.Value = polyline.name_zh;
                ////command.Parameters.Add(param);

                //param = new OracleParameter("shape", OracleDbType.Varchar2);
                //param.Value = wkt;
                //command.Parameters.Add(param);
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
        /// 根据way生成相应的geo
        /// </summary>
        /// <param name="osmids"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public GIS.Geometries.Geometry buildway(List<string> osmids)
        {
            if (osmids.Count() < 1)
            {
                return null;
            }

            PostGIS pg = new PostGIS(conString, "point", "shape", "oid", "osmid");

            if (osmids[0] == osmids[osmids.Count - 1])
            {
                GeoPolygon ply = new GeoPolygon();
                for (int i = 0; i < osmids.Count; i++)
                {
                    Geometry geom = pg.GetGeometryByKey(osmids[i]);
                    if (geom == null)
                    {
                        continue;
                    }
                    // GeoPoint pt = pg.GetGeometryByKey(osmids[i]) as GeoPoint;
                    ply.ExteriorRing.Vertices.Add(geom as GeoPoint);
                }
                if (ply.ExteriorRing.NumPoints <= 3)
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
                    if (geom == null)
                    {
                        continue;
                    }
                    line.Vertices.Add(geom as GeoPoint);
                }
                if (line.NumPoints < 2)
                {
                    return null;
                }
                pg.Dispose();
                return line;

            }


        }
        ///// <summary>
        ///// 对relation生成相应的面
        ///// </summary>
        ///// <param name="outers">外环</param>
        ///// <param name="inners">内环</param>
        ///// <returns></returns>
        //public Geometry buildRelation(List<string> outers,List<string> inners) 
        //{
        //    if (outers.Count==0)
        //    {
        //        return null;
        //    }
        //    PostGIS wayPg = new PostGIS(conString, "ways", "shape", "oid", "osmid");
        //    PostGIS relatinPg = new PostGIS(conString, "relations", "shape", "oid", "osmid");
        //    GeoPolygon ply = new GeoPolygon();
        //    for (int i = 0; i < outers.Count; i++)
        //    {
        //        string outer = outers[i];
        //        Geometry geo = wayPg.GetGeometryByKey(outer);
        //        if (geo!=null)
        //        {
        //            GeoLineString geoline=geo as GeoLineString;
        //            for (int j = 0; j < geoline.NumPoints; j++)
        //            {
        //                ply.ExteriorRing.Vertices.Add(geoline.Vertices[j]);
        //            }
        //        }
        //        else
        //        {
        //            geo = relatinPg.GetGeometryByKey(outer);
        //            if (geo!=null)
        //            {
        //                GeoPolygon plygeo = geo as GeoPolygon;
        //                ply.ExteriorRing = plygeo.ExteriorRing;
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }
        //    }
        //    for (int i = 0; i < inners.Count; i++)
        //    {
        //        string inner = inners[i];
        //        Geometry geom = relatinPg.GetGeometryByKey(inner);
        //        if (geom!=null)
        //        {
        //            GeoPolygon plygeo=geom as GeoPolygon;
        //            ply.InteriorRings.Add(plygeo.ExteriorRing);
        //        }
        //    }
        //    wayPg.Dispose();
        //    relatinPg.Dispose();
        //    try
        //    {
        //        ply.ExteriorRing.MakeClosed();
        //    }
        //    catch (Exception)
        //    {

        //        return null;
        //    }
        //    return ply;
          
        //}
        //public int updateNode(NODE node,string wkt)
        //{
        //    string cmdtext = "UPDATE nodes SET \"user\"=:user, uid=:uid, lat=:lat, lon=:lon, visible=:visible, \"version\"=:version,changeset=:changeset, \"timestamp\"=:timestamp, issimple=:issimple, fc=:fc, dsg=:dsg, code=:code,gbcode=:gbcode, gbdes=:gbdes, tags=:tags, bz=:bz, \"name\"=:name, name_en=:name_en, name_zh=:name_zh,shape=GeomFromText(:shape,4326) WHERE osmid='"+node.osmid+"'";
        //    //wkt = "GeomFromText(\""+wkt+"\",4326)";

        //    using (OracleConnection con = new OracleConnection(conString))
        //    using (OracleCommand command = new OracleCommand(cmdtext, con))
        //    {
        //        con.Open();
        //        OracleParameter 
        //        param = new OracleParameter("user", OracleDbType.Varchar2);
        //        param.Value = node.user;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("uid", OracleDbType.Varchar2);
        //        param.Value = node.uid;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("lat", NpgsqlTypes.NpgsqlDbType.Double);
        //        param.Value = node.lat;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("lon", NpgsqlTypes.NpgsqlDbType.Double);
        //        param.Value = node.lon;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("visible", OracleDbType.Int16);
        //        param.Value = node.visible;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("version", OracleDbType.Int16);
        //        param.Value = (int)node.version;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("changeset", OracleDbType.Varchar2);
        //        param.Value = node.changeset;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
        //        param.Value = node.timestamp;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("issimple", OracleDbType.Int16);
        //        param.Value = node.issimple;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("fc", OracleDbType.Varchar2);
        //        param.Value = node.fc;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("dsg", OracleDbType.Varchar2);
        //        param.Value = node.dsg;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("code", OracleDbType.Varchar2);
        //        param.Value = node.code;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("gbcode", OracleDbType.Varchar2);
        //        param.Value = node.gbcode;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("gbdes", OracleDbType.Varchar2);
        //        param.Value = node.gbdes;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("tags", OracleDbType.Clob);
        //        param.Value = node.tags;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("bz", OracleDbType.Varchar2);
        //        param.Value = node.bz;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("name", OracleDbType.Varchar2);
        //        param.Value = node.name;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("name_en", OracleDbType.Varchar2);
        //        param.Value = node.name_en;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("name_zh", OracleDbType.Varchar2);
        //        param.Value = node.name_zh;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("shape", OracleDbType.Varchar2);
        //        param.Value = wkt;
        //        command.Parameters.Add(param);
        //        int num = 0;
        //        try
        //        {
        //            num = command.ExecuteNonQuery();
        //        }
        //        catch (Exception)
        //        {


        //        }
        //        con.Close();
        //        return num;
        //    }
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="way"></param>
        ///// <param name="wkt"></param>
        ///// <param name="type">0=line,1=polygon</param>
        ///// <returns></returns>
        //public int updateWay(WAY way,string wkt,int type) 
        //{
        //    if (type == 0)
        //    {
        //        string cmdtext = "UPDATE ways SET \"user\"=:user, uid=:uid, visible=:visible, \"version\"=:version,changeset=:changeset, \"timestamp\"=:timestamp, issimple=:issimple, fc=:fc, dsg=:dsg, code=:code,gbcode=:gbcode, gbdes=:gbdes, tags=:tags, bz=:bz, \"name\"=:name, name_en=:name_en, name_zh=:name_zh,shape=GeomFromText(:shape,4326) WHERE osmid='" + way.osmid + "'";
        //        //wkt = "GeomFromText(" + wkt + ",4326)";

        //        using (OracleConnection con = new OracleConnection(conString))
        //        using (OracleCommand command = new OracleCommand(cmdtext, con))
        //        {
        //            con.Open();
        //            OracleParameter 
        //            param = new OracleParameter("user", OracleDbType.Varchar2);
        //            param.Value = way.user;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("uid", OracleDbType.Varchar2);
        //            param.Value = way.uid;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("visible", OracleDbType.Int16);
        //            param.Value = way.visible;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("version", OracleDbType.Int16);
        //            param.Value = (int)way.version;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("changeset", OracleDbType.Varchar2);
        //            param.Value = way.changeset;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
        //            param.Value = way.timestamp;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("issimple", OracleDbType.Int16);
        //            param.Value = way.issimple;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("fc", OracleDbType.Varchar2);
        //            param.Value = way.fc;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("dsg", OracleDbType.Varchar2);
        //            param.Value = way.dsg;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("code", OracleDbType.Varchar2);
        //            param.Value = way.code;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("gbcode", OracleDbType.Varchar2);
        //            param.Value = way.gbcode;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("gbdes", OracleDbType.Varchar2);
        //            param.Value = way.gbdes;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("tags", OracleDbType.Clob);
        //            param.Value = way.tags;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("bz", OracleDbType.Varchar2);
        //            param.Value = way.bz;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("name", OracleDbType.Varchar2);
        //            param.Value = way.name;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("name_en", OracleDbType.Varchar2);
        //            param.Value = way.name_en;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("name_zh", OracleDbType.Varchar2);
        //            param.Value = way.name_zh;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("shape", OracleDbType.Varchar2);
        //            param.Value = wkt;
        //            command.Parameters.Add(param);
        //            int num = 0;
        //            try
        //            {
        //                num = command.ExecuteNonQuery();
        //            }
        //            catch (Exception)
        //            {


        //            }
        //            con.Close();
        //            return num;
        //        }
        //    }
        //    else
        //    {
        //        string cmdtext = "UPDATE relations SET \"user\"=:user, uid=:uid, visible=:visible, \"version\"=:version,changeset=:changeset, \"timestamp\"=:timestamp, issimple=:issimple, fc=:fc, dsg=:dsg, code=:code,gbcode=:gbcode, gbdes=:gbdes, tags=:tags, bz=:bz, \"name\"=:name, name_en=:name_en, name_zh=:name_zh,shape=GeomFromText(:shape,4326) WHERE osmid='" + way.osmid + "'";
        //        //wkt = "GeomFromText(" + wkt + ",4326)";

        //        using (OracleConnection con = new OracleConnection(conString))
        //        using (OracleCommand command = new OracleCommand(cmdtext, con))
        //        {
        //            con.Open();
        //            OracleParameter
        //            param = new OracleParameter("user", OracleDbType.Varchar2);
        //            param.Value = way.user;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("uid", OracleDbType.Varchar2);
        //            param.Value = way.uid;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("visible", OracleDbType.Int16);
        //            param.Value = way.visible;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("version", OracleDbType.Int16);
        //            param.Value = (int)way.version;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("changeset", OracleDbType.Varchar2);
        //            param.Value = way.changeset;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
        //            param.Value = way.timestamp;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("issimple", OracleDbType.Int16);
        //            param.Value = way.issimple;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("fc", OracleDbType.Varchar2);
        //            param.Value = way.fc;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("dsg", OracleDbType.Varchar2);
        //            param.Value = way.dsg;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("code", OracleDbType.Varchar2);
        //            param.Value = way.code;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("gbcode", OracleDbType.Varchar2);
        //            param.Value = way.gbcode;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("gbdes", OracleDbType.Varchar2);
        //            param.Value = way.gbdes;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("tags", OracleDbType.Clob);
        //            param.Value = way.tags;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("bz", OracleDbType.Varchar2);
        //            param.Value = way.bz;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("name", OracleDbType.Varchar2);
        //            param.Value = way.name;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("name_en", OracleDbType.Varchar2);
        //            param.Value = way.name_en;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("name_zh", OracleDbType.Varchar2);
        //            param.Value = way.name_zh;
        //            command.Parameters.Add(param);
        //            param = new OracleParameter("shape", OracleDbType.Varchar2);
        //            param.Value = wkt;
        //            command.Parameters.Add(param);
        //            int num = 0;
        //            try
        //            {
        //                num = command.ExecuteNonQuery();
        //            }
        //            catch (Exception)
        //            {


        //            }
        //            con.Close();
        //            return num;
        //        }
        //    }
        //}
        ///// <summary>
        ///// 更新relation
        ///// </summary>
        ///// <param name="relation"></param>
        ///// <param name="wkt"></param>
        ///// <returns></returns>
        //public int updateRelation(RELATION relation, string wkt) 
        //{
        //    string cmdtext = "UPDATE relations SET \"user\"=:user, uid=:uid, visible=:visible, \"version\"=:version,changeset=:changeset, \"timestamp\"=:timestamp, issimple=:issimple, fc=:fc, dsg=:dsg, code=:code,gbcode=:gbcode, gbdes=:gbdes, tags=?, bz=?, \"name\"=:name, name_en=:name_en, name_zh=:name_zh,shape=GeomFromText(:shape,4326) WHERE osmid='" + relation.osmid + "'";
        //    //wkt = "GeomFromText(" + wkt + ",4326)";

        //    using (OracleConnection con = new OracleConnection(conString))
        //    using (OracleCommand command = new OracleCommand(cmdtext, con))
        //    {
        //        con.Open();
        //        OracleParameter 
        //        param = new OracleParameter("user", OracleDbType.Varchar2);
        //        param.Value = relation.user;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("uid", OracleDbType.Varchar2);
        //        param.Value = relation.uid;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("visible", OracleDbType.Int16);
        //        param.Value = relation.visible;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("version", OracleDbType.Int16);
        //        param.Value = (int)relation.version;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("changeset", OracleDbType.Varchar2);
        //        param.Value = relation.changeset;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
        //        param.Value = relation.timestamp;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("issimple", OracleDbType.Int16);
        //        param.Value = relation.issimple;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("fc", OracleDbType.Varchar2);
        //        param.Value = relation.fc;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("dsg", OracleDbType.Varchar2);
        //        param.Value = relation.dsg;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("code", OracleDbType.Varchar2);
        //        param.Value = relation.code;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("gbcode", OracleDbType.Varchar2);
        //        param.Value = relation.gbcode;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("gbdes", OracleDbType.Varchar2);
        //        param.Value = relation.gbdes;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("tags", OracleDbType.Clob);
        //        param.Value = relation.tags;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("bz", OracleDbType.Varchar2);
        //        param.Value = relation.bz;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("name", OracleDbType.Varchar2);
        //        param.Value = relation.name;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("name_en", OracleDbType.Varchar2);
        //        param.Value = relation.name_en;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("name_zh", OracleDbType.Varchar2);
        //        param.Value = relation.name_zh;
        //        command.Parameters.Add(param);
        //        param = new OracleParameter("shape", OracleDbType.Varchar2);
        //        param.Value = wkt;
        //        command.Parameters.Add(param);
        //        int num = 0;
        //        try
        //        {
        //            num = command.ExecuteNonQuery();
        //        }
        //        catch (Exception)
        //        {


        //        }
        //        con.Close();
        //        return num;
        //    }
        //}
        ///// <summary>
        ///// delete node
        ///// </summary>
        ///// <param name="osmid"></param>
        ///// <param name="type">0=node,1=line,2=polygon</param>
        ///// <returns></returns>
        //public int delete(string osmid,int type) 
        //{
        //    string cmdtext = null;
        //    if (type==0)
        //    {
        //        cmdtext = "DELETE FROM nodes WHERE osmid='"+osmid+"'";
        //    }
        //    else if (type==1)
        //    {
        //        cmdtext = "DELETE FROM ways WHERE osmid='" + osmid + "'";
        //    }
        //    else if (type==2)
        //    {
        //        cmdtext = "DELETE FROM relations WHERE osmid='"+osmid+"'";
        //    }
        //    else
        //    {
        //        return 0;
        //    }
            
        //    using(OracleConnection con=new OracleConnection(conString))
        //    using(OracleCommand command=new OracleCommand(cmdtext,con))
        //    {
        //        con.Open();
        //        int num = 0;
        //        try
        //        {
        //            num = command.ExecuteNonQuery();
        //        }
        //        catch (Exception)
        //        {


        //        }
        //        con.Clone();
        //        return num;
        //    }
        //}
        
    }
    
}

    

