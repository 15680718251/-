using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.Geometries;
using GIS.UI.Forms;
using GIS.UI.WellKnownText;
using Oracle.ManagedDataAccess.Client;

namespace TrustValueAndReputation.historyToDatabase
{
    public class SqlHelper_OSC
    {
        
        /// <summary>
        /// 读取默认路径下\Debug\存好的txt文档，行其中的sql代码，设计为创建、删除表格
        /// </summary>
        /// <param name="con"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool CreateTable(OracleConnection con, string fileName)
        {
            string sqlCreateTbl = null;
            StreamReader myRead = new StreamReader(String.Format("OsmSql\\{0}.txt", fileName));
            while (!myRead.EndOfStream)
            {
                sqlCreateTbl += myRead.ReadLine();
            }

            myRead.Close();
            myRead.Dispose();
            try
            {
                using (OracleCommand cmd = new OracleCommand(sqlCreateTbl, con))
                {
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 针对点，线，面的国标多个图层建表法，即指定点线面表格要素与表名操作
        /// </summary>
        /// <param name="con"></param>
        /// <param name="filePath"></param>
        /// <param name="tblname"></param>
        /// <returns></returns>
        public static int CreateOSCTable(OracleConnection con, string filePath, string tblname)
        {
            int sum = -100;
            string sqlCreateTbl = null;
            StreamReader myRead = new StreamReader(filePath);
            while (!myRead.EndOfStream)
            {
                sqlCreateTbl += myRead.ReadLine();
            }
            myRead.Close();
            myRead.Dispose();
            sqlCreateTbl = String.Format("Create Table {0}", tblname) + sqlCreateTbl;
            try
            {
                using (OracleCommand cmd = new OracleCommand(sqlCreateTbl, con))
                {
                    sum = cmd.ExecuteNonQuery();
                }
            }
            catch { }
            return sum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="tblname"></param>
        /// <returns></returns>
        public static int DropTable(OracleConnection con, string tblname)
        {
            int sum = -100;
            string dropText = String.Format("drop table {0}", tblname);
            try
            {
                using (OracleCommand cmd = new OracleCommand(dropText, con))
                {
                    sum = cmd.ExecuteNonQuery();
                }
            }
            catch(OracleException e )
            {
                Console.WriteLine(e);
                //if (e.Errors == "42p01")
                //{
                //    //代表数据库中没有这类表
                //    sum = -200;
                //}
            }
            return sum;
        }

        public static int InsertOscNode(OracleConnection con, OsmDataNode node)
        {
            string cmdtext = "INSERT INTO " + ImportOsc.oscPointTblName + "(osmid, \"user\", uid, lat, lon, \"version\", changeset,\"timestamp\", fc, dsg, code, gbcode, gbdes, tags, bz, \"name\", name_en, name_zh, shape, changetype, valid) "
            + "VALUES (:osmid, :user, :uid, :lat, :lon,  :version, :changeset,:timestamp, :fc, :dsg, :code, :gbcode, :gbdes, :tags, :bz, :name, :name_en, :name_zh, GeomFromText(:shape,4326), :changetype, :valid)";
            //wkt = "GeomFromText(\""+wkt+"\",4326)";
            
            using (OracleCommand command = new OracleCommand(cmdtext, con))
            {
                OracleParameter param = new OracleParameter("osmid", OracleDbType.Varchar2);
                param.Value = node.id;
                command.Parameters.Add(param);
                param = new OracleParameter("user", OracleDbType.Varchar2);
                param.Value = node.user;
                command.Parameters.Add(param);
                param = new OracleParameter("uid", OracleDbType.Varchar2);
                param.Value = node.uid;
                command.Parameters.Add(param);
                param = new OracleParameter("lat",OracleDbType.Double);
                param.Value = node.lat;
                command.Parameters.Add(param);
                param = new OracleParameter("lon",OracleDbType.Double);
                param.Value = node.lon;
                command.Parameters.Add(param);
                
                param = new OracleParameter("version", OracleDbType.Int16);
                param.Value = node.version;
                command.Parameters.Add(param);
                param = new OracleParameter("changeset", OracleDbType.Varchar2);
                param.Value = node.changeset;
                command.Parameters.Add(param);
                param = new OracleParameter("timestamp", OracleDbType.TimeStamp);
                param.Value = node.time;
                command.Parameters.Add(param);
                
                param = new OracleParameter("fc", OracleDbType.Varchar2);
                param.Value = node.fc;
                command.Parameters.Add(param);
                param = new OracleParameter("dsg", OracleDbType.Varchar2);
                param.Value = node.dsg;
                command.Parameters.Add(param);
                param = new OracleParameter("code",  OracleDbType.Varchar2);
                param.Value = node.code;
                command.Parameters.Add(param);
                param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                param.Value = node.gbcode;
                command.Parameters.Add(param);
                param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                param.Value = node.gbdes;
                command.Parameters.Add(param);
                param = new OracleParameter("tags", OracleDbType.Clob);
                param.Value = node.tagsToXml();
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
                param.Value = node.shapewkt;
                command.Parameters.Add(param);
                param = new OracleParameter("changetype", OracleDbType.Varchar2);
                param.Value = node.changeType;
                command.Parameters.Add(param);

                param = new OracleParameter("valid", OracleDbType.Int16);
                param.Value = 1;
                command.Parameters.Add(param);

                int num = 0;
                try
                {
                    num = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }
                return num;
            }
            
        }
        public static int InsertOscWay(OracleConnection con, OsmDataWay way)
        {
            int num=-100;//作返回值
            string cmdtext = "INSERT INTO " + ImportOsc.oscLineTblName + "(osmid, \"user\", uid,\"version\", changeset,\"timestamp\", fc, dsg, code, gbcode, gbdes, tags, bz, \"name\", name_en, name_zh, shape, changetype, valid) VALUES (:osmid, :user, :uid, :version, :changeset,:timestamp, :fc, :dsg, :code, :gbcode, :gbdes, :tags, :bz, :name, :name_en, :name_zh, GeomFromText(:shape,4326),:changetype, :valid)";
            if (way.isPolygon)
            {
                cmdtext = "INSERT INTO " + ImportOsc.oscAreaTblName + "(osmid, \"user\", uid, \"version\", changeset,\"timestamp\", fc, dsg, code, gbcode, gbdes, tags, bz, \"name\", name_en, name_zh, shape, changetype, valid,area_source) VALUES (:osmid, :user, :uid,:version, :changeset,:timestamp, :fc, :dsg, :code, :gbcode, :gbdes, :tags, :bz, :name, :name_en, :name_zh, GeomFromText(:shape,4326), :changetype, :valid, :area_source)";
             }
            using (OracleCommand command = new OracleCommand(cmdtext, con))
            {
                #region 线对象插入确定各个参数
                OracleParameter param = new OracleParameter("osmid", OracleDbType.Varchar2);
                param.Value = way.id;
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
                param.Value = way.time;
                command.Parameters.Add(param);
                
                param = new OracleParameter("fc", OracleDbType.Varchar2);
                param.Value = way.fc;;
                command.Parameters.Add(param);
                param = new OracleParameter("dsg", OracleDbType.Varchar2);
                param.Value = way.dsg;
                command.Parameters.Add(param);
                param = new OracleParameter("code",  OracleDbType.Varchar2);
                param.Value = way.code;
                command.Parameters.Add(param);
                param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                param.Value = way.gbcode;
                command.Parameters.Add(param);
                param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                param.Value = way.gbcode;
                command.Parameters.Add(param);
                param = new OracleParameter("tags", OracleDbType.Clob);
                param.Value = way.TagsToXml();
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
                param.Value = way.shapewkt;
                command.Parameters.Add(param);
                param = new OracleParameter("changetype", OracleDbType.Varchar2);
                param.Value = way.changeType;
                command.Parameters.Add(param);
                param = new OracleParameter("valid", OracleDbType.Int16);
                param.Value = 1;
                command.Parameters.Add(param);
                if (way.isPolygon) 
                {
                    param = new OracleParameter("area_source", OracleDbType.Varchar2);
                    param.Value = "way";
                    command.Parameters.Add(param);
                }
                #endregion
                try
                { num = command.ExecuteNonQuery();}
                catch (Exception e)
                {
                    
                }
                return num;
            }
        }
        public static int InsertOscRelation(OracleConnection con, OsmDataRelation relation,string tableName)
        {
            int num = -100;
            string cmdtext = "INSERT INTO " + ImportOsc.oscAreaTblName + "(osmid, \"user\", uid, \"version\", changeset,\"timestamp\", fc, dsg, code, gbcode, gbdes, tags, bz, \"name\", name_en, name_zh, shape, changetype, valid,area_source) VALUES (:osmid, :user, :uid, :version, :changeset,:timestamp,:fc, :dsg, :code, :gbcode, :gbdes, :tags, :bz, :name, :name_en, :name_zh, GeomFromText(:shape,4326), :changetype, :valid,:area_source)";
            //wkt = "GeomFromText(" + wkt + ",4326)";
            using (OracleCommand command = new OracleCommand(cmdtext, con))
            {
                OracleParameter param = new OracleParameter("osmid", OracleDbType.Varchar2);
                param.Value = relation.id;
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
                param.Value = relation.time;
                command.Parameters.Add(param);
                
                param = new OracleParameter("fc", OracleDbType.Varchar2);
                param.Value = relation.fc;
                command.Parameters.Add(param);
                param = new OracleParameter("dsg", OracleDbType.Varchar2);
                param.Value = relation.dsg;
                command.Parameters.Add(param);
                param = new OracleParameter("code",  OracleDbType.Varchar2);
                param.Value = relation.code;
                command.Parameters.Add(param);
                param = new OracleParameter("gbcode", OracleDbType.Varchar2);
                param.Value = relation.gbcode;
                command.Parameters.Add(param);
                param = new OracleParameter("gbdes", OracleDbType.Varchar2);
                param.Value = relation.gbdes;
                command.Parameters.Add(param);
                param = new OracleParameter("tags", OracleDbType.Clob);
                param.Value = relation.TagsToXml();
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
                param.Value = relation.shapewkt;
                command.Parameters.Add(param);
                param = new OracleParameter("changetype", OracleDbType.Varchar2);
                param.Value = relation.changeType;
                command.Parameters.Add(param);
                param = new OracleParameter("valid", OracleDbType.Int16);
                param.Value = 1;
                command.Parameters.Add(param);
                param = new OracleParameter("area_source", OracleDbType.Varchar2);
                param.Value = "relation";
                command.Parameters.Add(param);
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
        //由Node生成插入数据库操作所需要的字符串
        
        

       
        /// <summary>
        /// 查询数据库表格中有重复ID的数据
        /// </summary>
        /// <param name="tableName">数据表名</param>
        /// <param name="idname">数据表ID名</param>
        /// <param name="con">数据库连接对象</param>
        /// <returns></returns>
        public static List<uint> GetRepIds(string tableName,string idname,OracleConnection con)
        {
            List<uint> repids=new List<uint>();
            string repText =String.Format("select {0} from {1} group by {2} having count(*)>1",
                idname,tableName,idname);
            try
            {
                using (OracleCommand cmd = new OracleCommand(repText, con))
                {
                    using (OracleDataReader  reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                repids.Add(uint.Parse(reader.GetValue(0).ToString()));
                            }
                        }
                    }
                }
            }
            catch 
            {

            }
            return repids;
        }
        /// <summary>
        /// 根据元素ID在数据库中元素对应数据表中查找数据元素，返回DataReader对象
        /// </summary>
        /// <param name="tableName">数据表名</param>
        /// <param name="idname">元素ID在数据表中的名称</param>
        /// <param name="eleId">所查找元素的ID</param>
        /// <param name="con">数据库连接对象</param>
        /// <returns>返回DataReader对象</returns>
        public static OracleDataReader GetElesByID(string tableName,string idname, uint eleId,OracleConnection con)
        {
            string cmdText=String.Format("select * from {0} where {1}={2} order by version",tableName,idname,eleId);
            using(OracleCommand cmd=new OracleCommand(cmdText,con))
            {
                return cmd.ExecuteReader();
            }
        }

        public static List<OsmDataNode> GetNodesByID(uint eleId, OracleConnection con)
        {
            List<OsmDataNode> nodeLst=new List<OsmDataNode>();
            try
            {
                using (OracleDataReader nr = GetElesByID("oscNode", "nodeid", eleId, con))
                {
                    if (nr.HasRows)
                    {
                        while (nr.Read())
                        {
                            OsmDataNode node = new OsmDataNode();
                            node.InitialNodeFromNpgsqlreader(nr);
                            nodeLst.Add(node);
                        }
                    }
                }
            }
            catch
            { }
            return nodeLst;
        }

        public static List<OsmDataWay> GetWaysByID(uint eleId, OracleConnection con)
        {
            List<OsmDataWay> wayLst = new List<OsmDataWay>();
            try
            {
                using (OracleDataReader nr = GetElesByID("oscWay", "wayid", eleId, con))
                {
                    if (nr.HasRows)
                    {
                        while (nr.Read())
                        {
                            OsmDataWay way = new OsmDataWay();
                            way.InitialWayFromNpgsqlreader(nr);
                            wayLst.Add(way);
                        }
                    }
                }
            }
            catch
            { }
            return wayLst;
        }

        public static List<OsmDataRelation> GetRelationsByID(uint eleId, OracleConnection con)
        {
            List<OsmDataRelation> raletionLst = new List<OsmDataRelation>();
            try
            {
                using (OracleDataReader nr = GetElesByID("oscRelation", "rid", eleId, con))
                {
                    if (nr.HasRows)
                    {
                        while (nr.Read())
                        {
                            OsmDataRelation relation = new OsmDataRelation();
                            relation.InitialRelationFromNpgsqlReader(nr);
                            raletionLst.Add(relation);
                        }
                    }
                }
            }
            catch
            { }
            return raletionLst;
        }
        
        /// <summary>
        /// 根据元素所在数据表名、id名称、id号码来删除元素
        /// </summary>
        /// <param name="tableName">数据表名</param>
        /// <param name="idname">id名称</param>
        /// <param name="eleId">id号码</param>
        /// <param name="version">版本号</param>
        /// <param name="con">数据库连接对象</param>
        /// <returns>删除完毕返回成功</returns>
        public static bool DeleteEleByIdAndVersion(string tableName,string idname,uint eleId,int version,OracleConnection con)
        {
            try 
            {
                string delText = String.Format("delete from {0} where {1}={2} and version={3}",tableName,idname,eleId,version);
                using (OracleCommand cmd = new OracleCommand(delText, con))
                {
                    cmd.ExecuteNonQuery();
                }
                return true; 
            }
            catch 
            { 
                return false; 
            }
        }

        public static bool UpdateNodeChangeType(string tableName,uint nodeid,int version,OracleConnection con)
        {
            try
            {
                string updateText = String.Format("update {0} set changeType = 'create' where nodeid={1} and version ={2}"
                    ,tableName,nodeid,version);
                using (OracleCommand cmd = new OracleCommand(updateText, con))
                {
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch 
            {
                return false;
            }
 
        }

        public static bool InsertRelationIntoOsm(string conString, RELATION rela, List<string> outerrels, List<string> innerrels)
        {
            try
            {
                PostGISHelper helper = new PostGISHelper(conString);
                Geometry relationGeo = helper.buildRelation(outerrels, innerrels);
                if (relationGeo != null)
                {
                    string wkt = GeometryToWKT.Write(relationGeo);
                    //if (//helper.relation2PostGIS(rela, wkt) >= 1)
                    //{
                        return true;
                    //}
                    //else { return false; }
                }
                else { return false; }
                
            }
            catch
            {
                return false;
            }
        }

        
        public static bool InsertWayIntoOsm(WAY way, string conString, List<string> rels)
        {
            try 
            {
                PostGISHelper helper = new PostGISHelper(conString);
                Geometry wayGeo = helper.buildway(rels);
                if (wayGeo == null)
                {
                    return false;
                }
                string wkt = GeometryToWKT.Write(wayGeo);
                if (wayGeo is GeoLineString)
                {
                    //if (helper.way2PostGIS(way, wkt, 0,con) < 1)
                    //{
                    //    return false;
                    //}
                }
                if (wayGeo is GeoPolygon)
                {
                    //if (helper.way2PostGIS(way, wkt, 1) < 1)
                    //{
                    //    return false;
                    //}
                }
                return true;
                }
            catch 
            { 
                return false; 
            }
        }

        public static bool DeleteOsmEleByID(string tableName, string idname, uint id, OracleConnection con)
        {
            try 
            {
                string delText = String.Format("delete from {0} where {1} = '{2}'", tableName, idname, id);
                using( OracleCommand cmd=new OracleCommand(delText,con))
                {
                    cmd.ExecuteNonQuery();
                }
                return true; 
            }
            catch 
            { 
                return false; 
            }
        }

        public static List<uint> SelectIdsFromTable(string tableName, string idname, OracleConnection con)
        {
            List<uint> eleids = new List<uint>();
            try
            {
                string selText = String.Format("select {0} from {1} where {2}>0", idname, tableName, idname);
                using (OracleCommand cmd = new OracleCommand(selText, con))
                {
                    using (OracleDataReader nr = cmd.ExecuteReader())
                    {
                        if (nr.HasRows)
                        {
                            while (nr.Read())
                            {
                                eleids.Add(uint.Parse(nr.GetValue(0).ToString()));
                            }
                        }
                    }
                }
            }
            catch { }
            return eleids;
        }

        /// <summary>
        /// 通过way数据内涵的node的编号找到对应的geometry数据并填充进来
        /// </summary>
        /// <param name="reNodeIds">node编号队列</param>
        /// <returns>返回wkt字符串作为shape</returns>
        public static string BuildWay(List<uint> osmids,OracleConnection con,out bool isPolygon)
        {
            isPolygon = false;
            string shapewkt = null;
            if (osmids.Count() < 1)
            {
                return null;
            }
            #region way本身闭合
            if (osmids[0] == osmids[osmids.Count - 1])
            {
                GeoPolygon ply = new GeoPolygon();
                for (int i = 0; i < osmids.Count; i++)
                {
                    //查找对应的 shape 现在osc数据库中查找，然后在nodes数据库中查找。
                    string geomStr = GetGeometryByKey(osmids[i].ToString(), "shape", ImportOsc.oscPointTblName, "osmid", con);
                    if (geomStr == null)
                    {
                        geomStr = GetGeometryByKey(osmids[i].ToString(), "shape", ImportOsc.osmPointTblName, "osmid", con);
                    }
                    ply.ExteriorRing.Vertices.Add(GeometryFromWKT.Parse(geomStr) as GeoPoint);
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
                shapewkt = GeometryToWKT.Write(ply);
                isPolygon = true;
            }
            #endregion
            #region way本身不闭合
            else
            {
                GeoLineString line = new GeoLineString();
                for (int i = 0; i < osmids.Count; i++)
                {
                    string geomStr = GetGeometryByKey(osmids[i].ToString(), "shape", ImportOsc.oscPointTblName, "osmid", con);
                    if (geomStr == null)
                    {
                        geomStr = GetGeometryByKey(osmids[i].ToString(), "shape", ImportOsc.osmPointTblName, "osmid", con);
                        
                    }
                    line.Vertices.Add(GeometryFromWKT.Parse(geomStr) as GeoPoint);
                }
                if (line.NumPoints < 2)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
                {
                    return null;
                }
                shapewkt = GeometryToWKT.Write(line);
            }
            #endregion
            return shapewkt;
        }
        /// <summary>
        /// 通过Relation 数据内涵的way 的编号找到对应的way 数据并填充进来
        /// </summary>
        /// <param name="outers">外边界连线</param>
        /// <param name="inners">内边界连线</param>
        /// <returns></returns>
        public static string BuildRelation(List<uint> outers, List<uint> inners,OracleConnection con)
        {
            string shapewkt = null;
            if (outers.Count == 0)
            {
                return null;
            }
            GeoPolygon ply = new GeoPolygon();
            for (int i = 0; i < outers.Count; i++)
            {
                string outer = outers[i].ToString();
                string geomStr = GetGeometryByKey(outer, "shape", ImportOsc.oscLineTblName, "osmid", con);
                if (geomStr == null)
                {
                    geomStr = GetGeometryByKey(outer, "shape", ImportOsc.osmLineTblName, "osmid", con);
                }
                if (geomStr != null)
                {
                    //geomStr = "POLYGON((65.5577169 27.0772982,65.5578513 27.0773239,65.5578989 27.0773295,65.5579589 27.0773147,65.5579899 27.0772816,65.5580395 27.0772816,65.5581015 27.0772982,65.5581449 27.0773405,65.5581863 27.0774049,65.55824 27.0774362,65.5582938 27.0774712,65.5583248 27.0774988,65.5583744 27.0774915,65.5584137 27.0774639,65.5584427 27.077427,65.5585006 27.0774013,65.5585584 27.0774049,65.5586349 27.0774234,65.5587383 27.077427,65.5588231 27.0774381,65.5588789 27.0774547,65.5589864 27.0774786,65.5590257 27.0775154,65.5591374 27.0775375,65.5593152 27.0775909,65.5593999 27.0776314,65.5595426 27.0777105,65.5595777 27.0777584,65.5596067 27.0777713,65.5596501 27.0777731,65.5596687 27.0777602,65.5596563 27.0777308,65.5596129 27.0777105,65.559584 27.0776645,65.5595219 27.0776406,65.5594599 27.0775872,65.5595364 27.0775577,65.559617 27.0775172,65.5596522 27.0774988,65.5596791 27.0775117,65.5597101 27.0774804,65.5597597 27.0775265,65.5597721 27.0775817,65.5597638 27.0776185,65.5597473 27.0776406,65.5597411 27.0776829,65.5597597 27.0777013,65.5597928 27.0777142,65.5598197 27.0777124,65.5598238 27.077694,65.5598279 27.0776719,65.5598533 27.0776756,65.5598863 27.0776977,65.5599111 27.0777142,65.5599225 27.0777437,65.5599318 27.0777685,65.5600104 27.0777787,65.5601303 27.0777869,65.5601841 27.0777814,65.5602358 27.0777575,65.5603133 27.077717,65.5603495 27.0776802,65.5604969 27.0775799,65.560513 27.0775773,65.5605247 27.0775968,65.5605349 27.0776151,65.5605583 27.0776021,65.5606314 27.0775305,65.5606373 27.0775083,65.5606694 27.077481,65.5607148 27.0774888,65.5607382 27.0775187,65.560782 27.0775279,65.5608098 27.0775031,65.5608595 27.0774849,65.5608376 27.0773014,65.5607937 27.0773079,65.5607499 27.0773313,65.5606914 27.0773469,65.5605759 27.0773716,65.5604867 27.0773521,65.5604151 27.0773222,65.5602513 27.0773417,65.5601826 27.0773326,65.5601241 27.0773105,65.56007 27.0773027,65.5600408 27.0773378,65.5600525 27.0773912,65.5600291 27.0774185,65.5599925 27.0774393,65.5600583 27.0774628,65.560127 27.0774654,65.5601724 27.0774667,65.5601899 27.0774849,65.5602265 27.0774758,65.5602294 27.0775526,65.5602469 27.0775695,65.5602338 27.0776008,65.560187 27.0776528,65.5601417 27.0776632,65.5600832 27.0776372,65.5600349 27.0775799,65.5599721 27.0775357,65.5599063 27.0774966,65.5598463 27.0774354,65.5598288 27.0773964,65.559877 27.0773847,65.5598566 27.0773586,65.5597966 27.0773547,65.5597484 27.0773417,65.5597411 27.0773027,65.5597718 27.0772831,65.5598186 27.0772259,65.5598376 27.0771634,65.5598727 27.0771074,65.5599107 27.0770814,65.5599458 27.0770631,65.5598858 27.0770618,65.5598288 27.0770879,65.5597835 27.0771308,65.5597382 27.077149,65.5597382 27.0771803,65.5597557 27.0772063,65.5597235 27.0772519,65.5596899 27.0772649,65.5596621 27.0772259,65.5596402 27.0771959,65.5596124 27.0771764,65.5595788 27.0771777,65.5595291 27.0772037,65.5594911 27.0771946,65.5594414 27.0771634,65.559377 27.0771282,65.5592542 27.0770657,65.5590481 27.0770033,65.5587966 27.0769421,65.5587309 27.0769538,65.5586768 27.0769317,65.558592 27.0769082,65.5585788 27.0768666,65.5585262 27.0767885,65.5584721 27.0767585,65.5583697 27.0767039,65.5583361 27.0766518,65.5583244 27.0765971,65.5583259 27.0765307,65.5583376 27.0764695,65.5583624 27.0764175,65.5583537 27.0763836,65.5583127 27.0763758,65.5581972 27.076424,65.5580686 27.0764708,65.5580057 27.0764904,65.5579589 27.0765372,65.5579428 27.0766231,65.5579721 27.0766739,65.5580101 27.0766843,65.5580159 27.0767663,65.557994 27.0768379,65.5579589 27.0769707,65.5578887 27.0770006,65.5578273 27.0770228,65.5577835 27.0770631,65.5578069 27.0771178,65.557823 27.0771503,65.5578186 27.0771894,65.5577791 27.0772141,65.5577309 27.0772193,65.5577169 27.0772982))";
                    Geometry geom=GeometryFromWKT.Parse(geomStr);
                    if (geom.Area > 0)
                    {
                        ply = geom as GeoPolygon;
                    }
                    else 
                    {
                        GeoLineString geoline = geom as GeoLineString;
                        for (int j = 0; j < geoline.NumPoints; j++)
                        {
                            ply.ExteriorRing.Vertices.Add(geoline.Vertices[j]);
                        }
                    }
                    
                }
            }
            for (int i = 0; i < inners.Count; i++)
            {
                string inner = inners[i].ToString();
                string geomStr = GetGeometryByKey(inner, "shape", ImportOsc.oscLineTblName, "osmid", con);
                if (geomStr == null)
                {
                    geomStr = GetGeometryByKey(inner, "shape", ImportOsc.osmLineTblName, "osmid", con);
                }
                if (geomStr != null)
                {
                    Geometry geom = GeometryFromWKT.Parse(geomStr);
                    if (geom.Area > 0)
                    {
                        GeoPolygon plygeo = geom as GeoPolygon;
                        ply.InteriorRings.Add(plygeo.ExteriorRing);
                    }
                }
            }
            try
            {
                ply.ExteriorRing.MakeClosed();
            }
            catch (Exception)
            {
                return null;
            }
            shapewkt=GeometryToWKT.Write(ply);
            return shapewkt;
 
        }

        /// <summary>
        /// 把点对象或者线对象的shape以字符串的形式返回
        /// </summary>
        /// <param name="key"></param>
        /// <param name="GeometryColumn"></param>
        /// <param name="QualifiedTable"></param>
        /// <param name="KeyColumnName"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public static string GetGeometryByKey(String key, string GeometryColumn, string QualifiedTable, string KeyColumnName, OracleConnection con)
        {
            string geomStr = null;
            string strSQL = "SELECT AsText(" +GeometryColumn+ ") FROM " + QualifiedTable +
                            " WHERE " + KeyColumnName + "='" + key + "'";
            using (OracleCommand command = new OracleCommand(strSQL, con))
            {
                using (OracleDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        if (dr[0] != DBNull.Value)
                        {
                            geomStr = dr[0].ToString();
                        }
                    }
                }
            }
            return geomStr;
        }

        
        public static string nodeHomeTest(List<uint> osmid, OracleConnection con)
        {
            //返回way中node元素在nodes和oscnode中存在状况
            string text = null;
            for (int i = 0; i < osmid.Count; i++)
            {
                string cnt = GetShapeByID(con, ImportOsc.osmPointTblName, osmid[i].ToString());
                string oscCnt = GetShapeByID(con, ImportOsc.oscPointTblName, "osmid");
                string ecnt = cnt + oscCnt;
                string home = String.Format(" 基: {0} 增: {1}",cnt,oscCnt);
                text += osmid[i] + " : " + home + "\r\n";
            }
            return text;
        }

        /// <summary>
        /// 统计表中ID为某一值的元素数目
        /// </summary>
        /// <param name="con"></param>
        /// <param name="tblName"></param>
        /// <param name="eid"></param>
        /// <returns></returns>
        public static string GetShapeByID(OracleConnection con, string tblName, string eid)
        {
            string shp = null;
            string cmdText = String.Format("SELECT AsText(shape) FROM {0} where osmid='{1}';", ImportOsc.oscPointTblName, eid);
            try
            {
                using (OracleCommand cmd = new OracleCommand(cmdText, con))
                {
                    OracleDataReader nr = cmd.ExecuteReader();
                    {
                        if (nr.HasRows)
                        {
                            while (nr.Read())
                            {
                                shp = nr.GetValue(0).ToString();
                            }
                        }
                    }
                }
            }
            catch { }
            return shp;
        }

        /// <summary>
        /// 20140713 指定osc数据表，获取timestamp、osmid、version、changetype
        /// </summary>
        /// <param name="tblname">表</param>
        /// <returns>nr</returns>
        public static OracleDataReader GetOscData(string tblname,OracleConnection con)
        {
            //按版本降序排列，得到的总是最后一个版本的数据，保证数据是对应的最新的
            string cmdselectText = String.Format("Select  timestamp, osmid, version , changetype from {0} order by version DESC", tblname);
            OracleDataReader nr;
            using (OracleCommand cmd = new OracleCommand(cmdselectText, con))
            {
                nr = cmd.ExecuteReader();
            }
            return nr;
        }
        /// <summary>
        /// 20140713- 指定OSC表，开始结束时间筛选数据，设置valid值
        /// </summary>
        /// <param name="tblname"></param>
        /// <param name="nr"></param>
        /// <param name="con"></param>
        public static int UpdateTblValidWithTime(string tblname, OracleDataReader nr, OracleConnection con,DateTime startTime,DateTime endTime)
        {
            int row = 0;
            if (nr.HasRows)
            {
                List<DateTime> timeLst = new List<DateTime>();
                List<uint> osmidLst = new List<uint>();
                List<int> versionLst = new List<int>();
                List<string> changetypeLst = new List<string>();
                int cnt = 0;
                while (nr.Read())
                {
                    try
                    {
                        timeLst.Add(DateTime.Parse(nr.GetValue(0).ToString()));
                        string id = nr.GetValue(1).ToString();
                        
                        osmidLst.Add(uint.Parse(nr.GetValue(1).ToString()));
                        versionLst.Add(int.Parse(nr.GetValue(2).ToString()));
                        changetypeLst.Add(nr.GetValue(3).ToString());
                    }
                    catch 
                    { 

                    }
                    finally
                    {
                        cnt += 1;
                    }
                }
                nr.Close();
                nr.Dispose();
                for (int i = 0; i < osmidLst.Count; i++)
                {
                    
                    if (startTime.CompareTo(timeLst[i]) <= 0 && endTime.CompareTo(timeLst[i]) >= 0)
                    {
                        string cmdupdateText = String.Format("update {0} set valid = 1 where osmid ='{1}' and version = {2} and changetype = '{3}'",
                            tblname, osmidLst[i], versionLst[i], changetypeLst[i]);
                        using (OracleCommand cmd = new OracleCommand(cmdupdateText, con))
                        {
                            cmd.ExecuteNonQuery();
                                row += 1;
                        }
                    }
                    else
                    {
                    }
                }
            }
            return row;
        }
        /// <summary>
        /// 初始化设置表内Valid字段为0,以及设置c_modify设置为modify
        /// </summary>
        /// <param name="con">数据库连接对象</param>
        /// <param name="tblName">表名</param>
        /// <returns></returns>
        public static int SetTblInvalid(OracleConnection con,string tblName)
        {
            int num = -100;
            string cmd_text = String.Format("update {0} set valid = 0 where valid = 1", tblName); ;
            try
            {
                using (OracleCommand cmd = new OracleCommand(cmd_text, con))
                {
                    num = cmd.ExecuteNonQuery();
                }
            }
            catch { }
            string cmd_text1 = String.Format("update {0} set changetype = 'modify' where changetype = 'c_modify'", tblName); ;
            try
            {
                using (OracleCommand cmd = new OracleCommand(cmd_text1, con))
                {
                    //num = cmd.ExecuteNonQuery();
                }
            }
            catch 
            { }
            return num;
        }
        public static bool GetRulesKVByLyrName(string conString,string lyr_name,out List<string> osmkey,
            out List<string> osmvalue,out List<string> code) 
        {
            osmkey = new List<string>();
            osmvalue = new List<string>();
            code = new List<string>();
            try
            {
                using(OracleConnection con=new OracleConnection(conString))
                {
                    con.Open();
                    string cmdText=String.Format("select osm_key , osm_value ,code from rule where targetlyr = '{0}';",lyr_name);
                    using (OracleCommand cmd = new OracleCommand(cmdText, con))
                    {
                        
                        OracleDataReader nr = cmd.ExecuteReader();
                        if (nr.HasRows)
                        {
                            while (nr.Read())
                            {
                                osmkey.Add(safetyString(nr.GetValue(0).ToString()));
                                osmvalue.Add(safetyString(nr.GetValue(1).ToString()));
                                code.Add(nr.GetValue(2).ToString());
                            }
                        }
                        
                    }
                    con.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool GetMaxTime(string tblname, OracleConnection con,ref DateTime time)
        {
            //获取数据表中时间记录最大的值
            bool hasTime=false;
            string seleText = String.Format("Select timestamp from {0} order by timestamp DESC",tblname);
            try
            {
                using (OracleCommand cmd = new OracleCommand(seleText, con))
                {
                    using (OracleDataReader nr = cmd.ExecuteReader())
                    {
                        if (nr.HasRows)
                        {
                            nr.Read();
                            time = DateTime.Parse(nr.GetValue(0).ToString());
                            hasTime = true;
                        }
                        else
                        {
                        }
                    }
                }
            }
            catch { }
            return hasTime;
        }
        /// <summary>
        /// 获取表内最小时间
        /// </summary>
        /// <param name="tblname">表名</param>
        /// <param name="con">数据库连接对象</param>
        /// <param name="time">返回的时间</param>
        /// <returns>如果表内有时间，返回TRUE；如果表内没有时间，返回FALSE</returns>
        public static bool GetMinTime(string tblname, OracleConnection con, ref DateTime time)
        {
            //获取数据表中时间记录最小的值
            bool hasTime = false;
            string seleText = String.Format("Select timestamp from {0} order by timestamp ASC", tblname);
            try
            {
                using (OracleCommand cmd = new OracleCommand(seleText, con))
                {
                    using (OracleDataReader nr = cmd.ExecuteReader())
                    {
                        if (nr.HasRows)
                        {
                            nr.Read();
                            hasTime = true;
                            time = DateTime.Parse(nr.GetValue(0).ToString());
                            hasTime = true;
                        }
                        else { }
                    }
                }
            }
            catch { }
            return hasTime;
        }

        public static int OptimizeTbl(string tblname,OracleConnection con)
        {
            int row = 0 ;
            List<string> repEleLst = new List<string>();
            string selectText = String.Format("select osmid from ( select all osmid from {0} where valid = 1 )as osmid GROUP BY osmid having Count(osmid)>1 ", tblname);
            using (OracleCommand cmd = new OracleCommand(selectText,con))
            {
                using (OracleDataReader nr = cmd.ExecuteReader())
                {
                    if (nr.HasRows)
                    {
                        while (nr.Read())
                        {
                            repEleLst.Add(nr.GetValue(0).ToString());
                        }
                    }
                    else { }
                }
            }
            if (repEleLst.Count > 0)
            {
                for (int i = 0; i < repEleLst.Count; i++)
                {
                    OptimizeEle(tblname, repEleLst[i], con);
                    row += 1;
                }
            }
            return row;
        }

        private static int OptimizeEle(string tblname,string osmid, OracleConnection con)
        {
            if (osmid == "10117007") 
            {
                string mess = "";
            }
            int row=0;
            string optiText = String.Format("select version , changetype from {0} where osmid = '{1}' order by timestamp asc"
                ,tblname,osmid);
            List<string> versionLst = new List<string>();
            List<string> changeTpeLst = new List<string>();
            using (OracleCommand cmd = new OracleCommand(optiText, con))
            {
                using (OracleDataReader nr = cmd.ExecuteReader())
                {
                    if (nr.HasRows)
                    {
                        while (nr.Read())
                        {
                            versionLst.Add(nr.GetValue(0).ToString());
                            changeTpeLst.Add(nr.GetValue(1).ToString());
                        }
                    }
                }
            }
            for (int i = 0; i < versionLst.Count - 1; i++)
            {
                SetEleInvalid(tblname, osmid, versionLst[i], changeTpeLst[i], con);
            }
            if (versionLst[0] == "create" && changeTpeLst[versionLst.Count - 1] == "delete")
            {
                SetEleInvalid(tblname, osmid, versionLst[versionLst.Count - 1], changeTpeLst[versionLst.Count - 1], con);
            }
            else if (versionLst[0] == "create" && changeTpeLst[versionLst.Count - 1] == "modify")
            {
                string upText = String.Format("update {0} set changetype = 'c_modify' where osmid = '{1}' and version = {2}",
                    tblname,osmid,versionLst[versionLst.Count-1]);
                using (OracleCommand cmd = new OracleCommand(upText, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            return row;
        }

        /// <summary>
        /// 将表tblname内确定了osmid version changetype的数据valid设置为0
        /// </summary>
        /// <param name="tblname"></param>
        /// <param name="osmid"></param>
        /// <param name="version"></param>
        /// <param name="changeTpe"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        private static int SetEleInvalid(string tblname, string osmid, string version, string changeTpe, OracleConnection con)
        {
            int row = -100;
            string setText = String.Format("update {0} set valid = 0 where osmid = '{1}' and version = {2} and changetype = '{3}'"
                ,tblname,osmid,version,changeTpe);
            using (OracleCommand cmd = new OracleCommand(setText, con))
            {
                row=cmd.ExecuteNonQuery(); 
            }
            return row;
        }
        public static int SetBackModify(string tblname, OracleConnection con) 
        {
            int row = -100;
            string upText = String.Format("update {0} set changetype = 'modify' where changetype = 'c_modify'",tblname);
            using (OracleCommand cmd = new OracleCommand(upText, con))
            {
                row=cmd.ExecuteNonQuery();
            }
            return row;
        }
        /// <summary>
        /// 从源数据表向目标数据表中导入数据
        /// </summary>
        /// <param name="sourceTbl"></param>
        /// <param name="targetTbl"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public static int CopyDataBetween( string sourceTbl,string targetTbl,OracleConnection con)
        {
            int sum = 0;
            string cmdText = String.Format("insert into {0} ( select * from {1})",sourceTbl,targetTbl);
            try
            {
                using (OracleCommand cmd = new OracleCommand(cmdText, con))
                {
                    sum = cmd.ExecuteNonQuery();
                }
            }
            catch { }

            return sum;
        }

        /// <summary>
        /// 使用osc_国标图层更新new_国标图层
        /// </summary>
        /// <param name="osc_Tbl"></param>
        /// <param name="new_Tbl"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        internal static string FreshNewFromOsc(string osc_Tbl, string new_Tbl, OracleConnection con,string eleType)
        {
            int sum = 0;
            List<string> osmidLst = new List<string>();
            List<string> versionLst = new List<string>();
            List<string> changetpeLst = new List<string>();
            bool hasData=GetOscChangeInfo(osc_Tbl, con, out osmidLst, out versionLst,  out changetpeLst);
            if (hasData)
            {
                for (int i = 0; i < osmidLst.Count; i++)
                {
                    string mergeid = null;
                    string reputation = null;
                    bool newHasData=GetNewInfoNewMethod(new_Tbl, con, osmidLst[i], ref mergeid, ref reputation);
                    
                    if (changetpeLst[i] == "create" || changetpeLst[i] == "c_modify")
                    {
                        //执行插入
                        if (DeleteEleByID(new_Tbl, osmidLst[i], con) == 1)
                        { 
                        }
                        if (InsertNewFromOsc(eleType,con, osc_Tbl, new_Tbl, osmidLst[i], versionLst[i], changetpeLst[i]) == 1)
                        {
                        }
                    }
                    else if (changetpeLst[i] == "modify")
                    {
                        //先执行删除，后执行插入
                        if (DeleteEleByID(new_Tbl, osmidLst[i], con) == 1)              {
                        }
                        if (InsertNewFromOsc(eleType,con, osc_Tbl, new_Tbl, osmidLst[i], versionLst[i], changetpeLst[i]) == 1)
                        {
                        }
                    }
                    else if (changetpeLst[i] == "delete")
                    {
                        if (DeleteEleByID(new_Tbl, osmidLst[i], con) == 1)
                        {
                        }
                    }
                    sum += 1;
                }
            }
            return sum.ToString();
        }
        /// <summary>
        /// 由osc数据表中指定osmid version changetype向new表格中插入数据
        /// </summary>
        /// <param name="con"></param>
        /// <param name="osc_Tbl"></param>
        /// <param name="new_Tbl"></param>
        /// <param name="osmid"></param>
        /// <param name="version"></param>
        /// <param name="changetpe"></param>
        /// <returns></returns>
        private static int InsertNewFromOsc(string EleType ,OracleConnection con, string osc_Tbl, string new_Tbl, string osmid,string version,string changetpe)
        {
            int sum = -100;
            string c_SqlText = null;
            switch(EleType)
            {
                case "point":
                    {
                        string parameters = "osmid, \"user\", uid, lat, lon, version, changeset, timestamp, fc, dsg, code, gbcode, gbdes, tags, bz, name, name_en, name_zh, shape,targetlayer,oreputation,ureputation";
                        c_SqlText = String.Format("insert into {0}({1}) ( select {2} from {3} where osmid = '{4}' and version ={5} and changetype='{6}')" , new_Tbl, parameters,parameters,osc_Tbl, osmid, version, changetpe);
                        break;
                    }
                case "line": 
                    {
                        string parameters = "osmid, \"user\", uid, version, changeset, timestamp, fc, dsg, code, gbcode, gbdes, tags, bz, name, name_en, name_zh, shape,l_length,mergeid,targetlayer,oreputation,ureputation";
                        c_SqlText = String.Format("insert into {0}({1}) ( select {2} from {3} where osmid = '{4}' and version ={5} and changetype='{6}')", new_Tbl, parameters,parameters, osc_Tbl, osmid, version, changetpe);
                        break;
                    }
                case "area": 
                    {
                        string parameters = "osmid, \"user\", uid, version, changeset, timestamp, fc, dsg, code, gbcode, gbdes, tags, bz, name, name_en, name_zh, a_length,a_area,mergeid,shape,targetlayer,oreputation,ureputation,area_source";
                        c_SqlText = String.Format("insert into {0}({1}) ( select {2} from {3} where osmid = '{4}' and version ={5} and changetype='{6}')" , new_Tbl, parameters,parameters,osc_Tbl, osmid, version, changetpe);
                        break;
                    }
            }
            using (OracleCommand cmd = new OracleCommand(c_SqlText,con))
            {
                int result = cmd.ExecuteNonQuery();
                if (result == 1) 
                {
                    sum = 1;
                }
            }
            return sum;
        }
        /// <summary>
        /// 查询new数据表中的mergeid和reputation
        /// </summary>
        /// <param name="new_Tbl"></param>
        /// <param name="con"></param>
        /// <param name="osmid"></param>
        /// <param name="mergeid"></param>
        /// <param name="reputation"></param>
        /// <returns></returns>
        private static bool GetNewInfoNewMethod(string new_Tbl, OracleConnection con,string  osmid, ref string mergeid, ref string reputation)
        {
            bool sum = false;
            string selectNewMergeTpe = String.Format("select mergeid,oreputation from {0} where osmid = {1}", new_Tbl, osmid);
            try
            {
                using (OracleCommand cmd = new OracleCommand(selectNewMergeTpe, con))
                {
                    using (OracleDataReader nr = cmd.ExecuteReader())
                    {
                        if (nr.HasRows)
                        {
                            sum = true;
                            while (nr.Read())
                            {
                                mergeid = nr.GetValue(0).ToString();
                                reputation = nr.GetValue(1).ToString();
                            }
                        }
                        else { }
                    }
                }
            }
            catch { }
            return sum;
        }
        /// <summary>
        /// 查询osc表格中需要更新到基态数据库中的osmid,version,changetype并以数组的形式返回
        /// </summary>
        /// <param name="osc_Tbl"></param>
        /// <param name="con"></param>
        /// <param name="osmidLst"></param>
        /// <param name="versionLst"></param>
        /// <param name="changetpeLst"></param>
        /// <returns>TRUE代表有数据并返回了，FALSE代表没有数据或者出错了</returns>
        private static bool GetOscChangeInfo(string osc_Tbl, OracleConnection con, out List<string> osmidLst,out List<string> versionLst,out List<string> changetpeLst)
        {
            bool sum = false;
            osmidLst = new List<string>();
            versionLst = new List<string>();
            changetpeLst = new List<string>();
            string selectText = String.Format("select osmid,version,changetype from {0} where valid =1", osc_Tbl);
            try
            {
                using (OracleCommand cmd = new OracleCommand(selectText, con))
                {
                    using (OracleDataReader nr = cmd.ExecuteReader())
                    {
                        if (nr.HasRows)
                        {
                            sum = true;
                            while (nr.Read())
                            {
                                osmidLst.Add(nr.GetValue(0).ToString());
                                versionLst.Add(nr.GetValue(1).ToString());
                                changetpeLst.Add(nr.GetValue(2).ToString());
                            }
                        }
                        else { }
                    }
                }
            }
            catch { }
            return sum;
        }
        /// <summary>
        /// 根据元素的id删除元素的数据
        /// </summary>
        /// <param name="tblname"></param>
        /// <param name="osmid"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        private static int DeleteEleByID(string tblname, string osmid,OracleConnection con)
        {
            int sum = -100;
            try 
            {
                string cmdText=String.Format("delete from {0} where osmid ='{1}'",tblname,osmid);
                using (OracleCommand cmd = new OracleCommand(cmdText, con))
                {
                    
                    int t = cmd.ExecuteNonQuery();
                    if (t == 1) 
                    {
                        sum = 1;
                    }
                }
            }
            catch { }
            return sum;
        }

        /// <summary>
        /// 判断OSC点元素是否在所需范围内
        /// </summary>
        /// <returns></returns>
        public static bool IsNodeInShape(OsmDataNode node,string shape,OracleConnection con) 
        {
            bool sum = false;
            if (node.shapewkt != null)
            {
                string cmdText = String.Format("select ST_intersects('{0}','{1}')", node.shapewkt, shape);
                try
                {
                    using (OracleCommand cmd = new OracleCommand(cmdText, con))
                    {
                        using (OracleDataReader nr = cmd.ExecuteReader())
                        {
                            if (nr.HasRows)
                            {
                                nr.Read();
                                if (nr.GetValue(0).ToString() == "t")
                                {
                                    sum = true;
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            return sum;
        }
        /// <summary>
        /// 判断线元素是否在所需范围内
        /// </summary>
        /// <returns></returns>
        public static bool IsWayInShape(OsmDataWay way, string shape, OracleConnection con)
        {
            bool sum = false;
            if (way.shapewkt != null)
            {
                string cmdText = String.Format("select ST_intersects('{0}','{1}')", way.shapewkt, shape);
                try
                {
                    using (OracleCommand cmd = new OracleCommand(cmdText, con))
                    {
                        using (OracleDataReader nr = cmd.ExecuteReader())
                        {
                            if (nr.HasRows)
                            {
                                nr.Read();
                                if (nr.GetValue(0).ToString() == "t")
                                {
                                    sum = true;
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            return sum;
        }
        /// <summary>
        /// 判断关系元素是否在所需范围内
        /// </summary>
        /// <returns></returns>
        public static bool IsRelationInShape(OsmDataRelation relation, string shape, OracleConnection con)
        {
            bool sum = false;
            if (relation.shapewkt != null)
            {
                string cmdText = String.Format("select ST_intersects('{0}','{1}')", relation.shapewkt, shape);
                try
                {
                    using (OracleCommand cmd = new OracleCommand(cmdText, con))
                    {
                        using (OracleDataReader nr = cmd.ExecuteReader())
                        {
                            if (nr.HasRows)
                            {
                                nr.Read();
                                if (nr["st_intersects"].ToString() == "t")
                                {
                                    sum = true;
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            return sum;
            return false;
        }
        /// <summary>
        /// 获取数据表中valid值的数目
        /// </summary>
        /// <param name="tblname"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public static int GetValidCnt(string tblname, OracleConnection con)
        {
            int sum = 0;
            try
            {
                string cmdtext = String.Format("select count(*) from {0} where valid=1",tblname);
                using (OracleCommand cmd = new OracleCommand(cmdtext, con))
                {
                    using (OracleDataReader nr = cmd.ExecuteReader())
                    {
                        if (nr.HasRows)
                        {
                            nr.Read();
                            sum = int.Parse(nr.GetValue(0).ToString());
                        }
                    }
                }
            }
            catch { }
            return sum;
        }
        public static Dictionary<string,int> GetChangeType(string tblName, OracleConnection con) 
        {
            Dictionary<string, int> cntLst = new Dictionary<string, int>();
            cntLst.Add("create", GetTypeCnt(tblName,"create",con));
            cntLst.Add("modify", GetTypeCnt(tblName, "modify", con));
            cntLst.Add("delete", GetTypeCnt(tblName, "delete", con));
            return cntLst;
        }
        public static int GetTypeCnt(string tblName, string c_type, OracleConnection con) 
        {
            int typeCnt = 0;
            string cmdText = String.Format("select count(*) from {0} where changetype='{1}'",tblName,c_type);
            try
            {
                using (OracleCommand cmd = new OracleCommand(cmdText, con))
                {
                    using (OracleDataReader nr = cmd.ExecuteReader())
                    {
                        nr.Read();
                        typeCnt = int.Parse(nr.GetValue(0).ToString());
                    }
                }
            }
            catch { }
            return typeCnt;
        }
        /// <summary>
        /// 查询增量未赋值信誉度数据线数据（非闭合线）的id
        /// </summary>
        /// <param name="tblname"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public static List<string> GetNoRepuIdsFromLine(string tblname, OracleConnection con) 
        {
            List<string> noRepuIds = new List<string>();
            //line
            string cmdText = String.Format("select osmid ,version from {0} where oreputation=0 and changetype!='delete'",tblname);
            using (OracleCommand cmd = new OracleCommand(cmdText, con)) 
            {
                using (OracleDataReader nr = cmd.ExecuteReader()) 
                {
                    if (nr.HasRows)
                    {
                        while (nr.Read())
                        {
                            string id_temp = nr.GetValue(0).ToString();
                            noRepuIds.Add(id_temp);
                        }
                    }
                }
            }
            return noRepuIds;
        }
        /// <summary>
        /// 查询增量未赋值信誉度线数据（闭合线）的id
        /// </summary>
        /// <param name="tblname"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public static List<string> GetNoRepuIdsFromArea(string tblname, OracleConnection con)
        {
            List<string> noRepuIds = new List<string>();
            //line
            string cmdText = String.Format("select osmid ,version from {0} where oreputation=0 and changetype!='delete'and area_source= 'way'", tblname);
            using (OracleCommand cmd = new OracleCommand(cmdText, con))
            {
                using (OracleDataReader nr = cmd.ExecuteReader())
                {
                    if (nr.HasRows)
                    {
                        while (nr.Read())
                        {
                            string id_temp = nr.GetValue(0).ToString();
                            noRepuIds.Add(id_temp);
                        }
                    }
                }
            }
            return noRepuIds;
        }
        /// <summary>
        /// 查询数据表未赋值信誉度数据线数据（非闭合线）的id
        /// </summary>
        /// <param name="tblname"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public static List<string> GetNoRepuIdsFromLineTable(string tblname, OracleConnection con)
        {
            List<string> noRepuIds = new List<string>();
            //line
            string cmdText = String.Format("select osmid from {0} where oreputation=0", tblname);
            using (OracleCommand cmd = new OracleCommand(cmdText, con))
            {
                using (OracleDataReader nr = cmd.ExecuteReader())
                {
                    if (nr.HasRows)
                    {
                        while (nr.Read())
                        {
                            string id_temp = nr.GetValue(0).ToString();
                            noRepuIds.Add(id_temp);
                        }
                    }
                }
            }
            return noRepuIds;
        }
        /// <summary>
        /// 查询数据表未赋值信誉度线数据（闭合线）的id
        /// </summary>
        /// <param name="tblname"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public static List<string> GetNoRepuIdsFromAreaTable(string tblname, OracleConnection con)
        {
            List<string> noRepuIds = new List<string>();
            //line
            string cmdText = String.Format("select osmid from {0} where oreputation=0 and area_source= 'way';", tblname);
            using (OracleCommand cmd = new OracleCommand(cmdText, con))
            {
                using (OracleDataReader nr = cmd.ExecuteReader())
                {
                    if (nr.HasRows)
                    {
                        while (nr.Read())
                        {
                            string id_temp = nr.GetValue(0).ToString();
                            noRepuIds.Add(id_temp);
                        }
                    }
                }
            }
            return noRepuIds;
        }

        public static int GetEleCount(string tableName, OracleConnection con)
        {
            int cnt = 0;
            try
            {
                string cmdText = String.Format("select count(*) from {0}", tableName);
                using (OracleCommand cmd = new OracleCommand(cmdText, con))
                {
                    using (OracleDataReader nr = cmd.ExecuteReader()) 
                    {
                        if (nr.HasRows)
                        {
                            nr.Read();
                            cnt = int.Parse(nr.GetValue(0).ToString());
                        }
                    }
                }
                
            }
            catch 
            {
            }
            return cnt;
        }
        /// <summary>
        /// 向GNS数据表中插入一行记录
        /// </summary>
        /// <param name="gnsrecord"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public static int InsertIntoGnsTable(string[] gnsrecord,OracleConnection con)
        {

            int cnt = 0;
            string fields = "rc, ufi, uni, lat, long, dms_lat, dms_long, mgrs, jog, fc, dsg, pc, cc1, adm1, pop, elev, cc2, nt, lc, short_form, generic, sort_name_ro, full_name_ro, full_name_nd_ro, sort_name_rg, full_name_rg, full_name_nd_rg, note, modify_date, display, name_rank, name_link, transl_cd, nm_modify_date,geom";
            Dictionary<string,int> notChar = new Dictionary<string,int>();
            notChar.Add("lat",3);
            notChar.Add("lon",4);
            string values = GenerateLabelValue(gnsrecord,notChar);
            string cmdText = String.Format("insert into {0} ({1}) values ({2})",ImportOsc.gnsTblName,fields,values);
            try
            {
                using (OracleCommand cmd = new OracleCommand(cmdText, con))
                {
                    cnt = cmd.ExecuteNonQuery();
                }
            }
            catch 
            {

            }
            return cnt;
        }
        /// <summary>
        /// 根据读取的文件生成Geonames的值串
        /// </summary>
        /// <param name="record"></param>
        /// <param name="notChar">参数，代表数据表中哪些字段是非字符的，必含lat lon字段，用于组成shape</param>
        /// <returns></returns>
        private static string GenerateLabelValue(string[] record,Dictionary <string,int> notChar) 
        {
            string temp = null;
            for (int i = 0; i < record.Length; i++)
            {
                string recordTemp=record[i]==null?" ":record[i];
                if (notChar.Values.Contains(i))
                {
                    temp += recordTemp + ", ";
                }
                else 
                {
                    temp += "'" + recordTemp + "', ";
                }
            }

            double lon = System.Convert.ToDouble(record[notChar["lon"]]);
            double lat = System.Convert.ToDouble(record[notChar["lat"]]);
            string shapewkt = GeometryToWKT.Write(new GIS.Geometries.GeoPoint(lon, lat));
            temp += "'" +shapewkt + "' ";
            return temp;
        }
        /// <summary>
        /// 执行geonames单行记录的插入
        /// </summary>
        /// <param name="geoNsRecord"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        internal static int InsertIntoGeonamesTable(string[] geoNsRecord, OracleConnection con)
        {
            int cnt = 0;
            string fields = "geonameid, name, asciiname, alternatenames, latitude, "+
                "longitude, feature_class, feature_code, country_code, cc2, admin1_code, admin2_code, admin3_code, admin4_code,"+
                " population, elevation, dem, timezone, modification_date, geom";
            Dictionary< string,int> notChar = new Dictionary<string,int>();
            notChar.Add("geonameid", 0);
            notChar.Add("lat", 4);
            notChar.Add("lon", 5);
            
            
            string values = GenerateLabelValue(geoNsRecord, notChar);

            string cmdText = String.Format("insert into {0} ({1}) values ({2})", ImportOsc.geonamesTblName, fields, values);
            try
            {
                using (OracleCommand cmd = new OracleCommand(cmdText, con))
                {
                    cnt = cmd.ExecuteNonQuery();
                }
            }
            catch
            {

            }
            return cnt;
        }
        /// <summary>
        /// 执行Sql统计语句，返回结果，错误情况下返回-1
        /// </summary>
        /// <param name="countText"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public static int ExeCountText(string countText,OracleConnection con)
        {
            int result=-1;
            try
            {
                using (OracleCommand cmd = new OracleCommand(countText, con))
                {
                    using (OracleDataReader nr = cmd.ExecuteReader())
                    {
                        if (nr.HasRows)
                        {
                            nr.Read();
                            result = System.Convert.ToInt32(nr.GetValue(0).ToString());
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }
        
        
        /// <summary>
        /// 用翻译后的注记填充数据表
        /// </summary>
        /// <param name="tblName">更新表名</param>
        /// <param name="locateField">定位字段名-唯一标识字段 非字符型</param>
        /// <param name="locateValue">定位字段值</param>
        /// <param name="updateField">更新字段名 字符型</param>
        /// <param name="updateValue">更新字段值</param>
        /// <param name="con">数据库连接对象</param>
        /// <returns>影响的行数</returns>
        public static int  UpdateLabel(string tblName,string locateField,string locateValue 
            ,string updateField,string updateValue,OracleConnection con)
        {
            int result = -100;
            try
            {
                string cmdText = String.Format("update {0} set {1}='{2}' where {3}={4}"
                    , tblName, updateField,updateValue, locateField, locateValue);
                using (OracleCommand cmd = new OracleCommand(cmdText, con))
                {
                    result = cmd.ExecuteNonQuery();
                }
            }
            catch 
            {

            }
            return result;
        }
        /// <summary>
        /// 根据sql语句统计元素数据
        /// </summary>
        /// <param name="countSql">sql统计语句</param>
        /// <param name="con">数据库连接对象</param>
        /// <returns>操作失败返回-1 操作成功返回整数</returns>
        public static int CountBySql(string countSql, OracleConnection con)
        {
            int result = -1;
            try
            {
                using (OracleCommand cmd = new OracleCommand(countSql, con))
                {
                    using (OracleDataReader nr = cmd.ExecuteReader())
                    {
                        if (nr.HasRows)
                        {
                            nr.Read();
                            result = System.Convert.ToInt32(nr.GetValue(0));
                        }

                    }
                }
            }
            catch { }
            return result;
        }

        public static string safetyString(string input) 
        {
            string result=null;
            char[] sss = input.ToCharArray();
            List<char> ooo = new List<char>();
            for (int i = 0; i < sss.Length; i++)
            {

               if (sss[i] == '\'') 
                {
                    ooo.Add('\\');
                }
               ooo.Add(sss[i]);
              
            }
            for (int i = 0; i < ooo.Count; i++) 
            {
                result += ooo[i].ToString();
            }
                return result;
        }
    }
    public class Helper 
    {
        public static void SaveTxtFile(string content,string path) 
        {
            if (!System.IO.File.Exists(path))
            {
                FileStream fs;
                fs = File.Create(path);
                fs.Close();
            }

            FileStream fsTxtWrite = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter srWrite = new StreamWriter(fsTxtWrite, System.Text.Encoding.UTF8);
            srWrite.Write(content);
            srWrite.Close();
            srWrite.Dispose();
            fsTxtWrite.Close();
            fsTxtWrite.Dispose(); 
        }
    }
}
