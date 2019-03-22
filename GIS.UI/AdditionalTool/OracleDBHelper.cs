using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.IO;
using GIS.UI.Entity;
using System.Windows.Forms;
using GIS.UI.Forms;
using System.Collections;

namespace GIS.UI.AdditionalTool
{
    //数据库帮助文档 by 丁洋 20180620
    public class OracleDBHelper
    {
        public static string _conString;//zh 修改0705 sqlplus连接参数
        private static OracleConnection con;


        public string conString
        {
            get { return _conString; }
            set { _conString = value; }
        }

        public OracleDBHelper()
        {

        }

        //public OracleDBHelper(string constr)
        //{
        //    _conString = constr;
        //    con = new OracleConnection(constr);

        //}

        //设置oracle连接参数
        public void setOracleConnection(string conStr)
        {
            con = new OracleConnection(conStr);
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
        }

        public OracleConnection getOracleConnection()
        {
            return con;
        }
        #region 基态数据入库 by dy 
        public DataTable queryBySql(string sql)
        {
            DataTable table = new DataTable();
            table.Columns.Add("userid");
            table.Columns.Add("username");
            try
            {
                using (OracleCommand command = new OracleCommand(sql, con))
                {
                    using (OracleDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = table.NewRow();
                            row["username"] = dr["USERNAME"].ToString();
                            table.Rows.Add(row);
                        }
                        return table;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        // 线数据入库
        public void insertPolyDataBySql(string tableName, List<Poly> listPara)
        {
            string osmid = "";
            string tags = "";
            List<Poly> polyList = listPara;
            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                string sql = "";
                for (int i = 0; i < polyList.Count(); i++)
                {
                    Poly poly = polyList[i];
                    string pointsId = poly.getPointsId();
                    if (pointsId.Length > 4000) continue;//点字符数多于4000的不要
                    string[] pointids = pointsId.Split(new string[1] { "," }, System.StringSplitOptions.None);

                    //List<Ppoint> latLonList = new List<Ppoint>();
                    List<string> latLonList = new List<string>();
                    StringBuilder checkStrSql = new StringBuilder("select lat,lon from apoint where osmid in( ");
                    //string pointIds = "";
                    int length = pointids.Length;
                    //int count = 1;
                    //const int number = 1000;
                    //while (length > number)
                    //{
                    //    for (int j = 0 + (count - 1) * number; j < number * count; j++)
                    //    {
                    //        string pointid = pointids[j];
                    //        string sql1 = string.Format("select lat,lon from apoint where osmid='{0}'", pointid);
                    //        using (OracleCommand conmand = new OracleCommand(sql1, con))
                    //        {
                    //            using (OracleDataReader rd = conmand.ExecuteReader())
                    //            {
                    //                while (rd.Read())
                    //                {
                    //                    Ppoint point = new Ppoint();
                    //                    point.setLat(float.Parse(rd["lat"].ToString()));
                    //                    point.setLon(float.Parse(rd["lon"].ToString()));
                    //                    latLonList.Add(point);

                    //                }
                    //            }
                    //        }
                    //        pointIds += pointids[j] + ",";
                    //    }
                    //    pointIds = pointIds.Substring(0, pointIds.Length - 1);
                    //    checkStrSql.Append(pointIds);
                    //    checkStrSql.Append(") or osmid in ( ");
                    //    length = pointids.Length - number * count;
                    //    count++;
                    //    pointIds = "";
                    //}
                    if (length > 0)
                    {
                        for (int j = 0; j < pointids.Length; j++)
                        {
                            string pointid = pointids[j];

                            //string sql1 = string.Format("select lat,lon from apoint where osmid='{0}'", pointid);
                            //using (OracleCommand conmand = new OracleCommand(sql1, con))
                            //{
                            //    using (OracleDataReader rd = conmand.ExecuteReader())
                            //    {
                            //        while (rd.Read())
                            //        {
                            //            Ppoint point = new Ppoint();
                            //            point.setLat(float.Parse(rd["lat"].ToString()));
                            //            point.setLon(float.Parse(rd["lon"].ToString()));
                            //            //latLonList.Add(point);
                            //        }
                            //    }
                            //}
                            string sql2 = string.Format("select sdo_geometry.get_wkt(shape) from apoint where osmid='{0}'", pointid);
                            using (OracleCommand conmand = new OracleCommand(sql2, con))
                            {
                                using (OracleDataReader rd = conmand.ExecuteReader())
                                {
                                    while (rd.Read())
                                    {
                                        string pointStr = rd[0].ToString();
                                        string pointUnit = pointStr.Substring(7, pointStr.Length - 8);
                                        latLonList.Add(pointUnit);
                                    }
                                }
                            }
                        }
                    }

                    if (latLonList.Count <= 0)
                    {
                        continue;
                    }
                    string linestring = "";
                    for (int j = 0; j < latLonList.Count; j++)
                    {
                        //linestring += latLonList[j].getLon() + " ";
                        //linestring += latLonList[j].getLat() + ",";
                        linestring += latLonList[j] + ",";
                        
                    }

                    int source = 0;
                    if (linestring.Length > 0)
                    {
                        linestring = linestring.Substring(0, linestring.Length - 1);
                    }
                    string wkt = "";

                    if (pointids[0].Equals(pointids[pointids.Length - 1]))
                    {
                      
                        if (linestring.Length < 105)
                        {
                            wkt = "";
                        }
                        else
                        {
                            //polyType = 2;
                            wkt = "POLYGON((" + linestring + "))";
                            //wkt = "POLYGON((1 1,5 1,5 5,1 5,1 1),(2 2,2 3,3 3,3 2,2 2))";
                        }                         
                    }
                    else
                    {
                        //polyType = 1;
                        wkt = "LINESTRING("+ linestring +")";
                        //LINESTRING(3 4,10 50,20 25)
                    }                                                     
                    //poly.setWkt(wkt);
                    poly.setSource(source);
                    osmid = poly.getOsmid().ToString();
                    tags = poly.getTags();
                    if (poly.getTags().Length > 400)
                    {
                        tags = tags.Substring(0, 400);
                    }

                    if (wkt == null)
                    {
                        Console.WriteLine("poly.getWkt()=" + wkt);
                        continue;
                    }
                    if (wkt.Length > 0)
                    {
                        StringBuilder strSql = new StringBuilder();
                        string sql1 = string.Format("insert into {0} (osmid,versionid,starttime,changeset,userid,username,fc,dsg,tags,trustvalue,pointsId,shape,source) values", tableName);
                        strSql.Append(sql1);
                        //string sql2 = string.Format("({0},{1},'{2}','{3}',{4},'{5}','{6}','{7}','{8}',{9},'{10}',{11},sdo_geometry ('{12}',31297 ))",poly.getOsmid(), poly.getVersion(), poly.getTimestamp(), poly.getChangeset(), poly.getUserid(), "",poly.getFc(),poly.getDsg(), tags, poly.getTrustvalue(), poly.getPointsId(), poly.getPolytype(), poly.getWkt());
                        string sql2 = string.Format("({0},{1},'{2}','{3}',{4},'{5}','{6}','{7}','{8}',{9},'{10}',{11},{12})", poly.getOsmid(), poly.getVersion(), poly.getStartTime(), poly.getChangeset(), poly.getUserid(), "", poly.getFc(), poly.getDsg(), tags, poly.getTrustvalue(), poly.getPointsId(), "sdo_geometry ( :geom,4326)",poly.getSource());

                        strSql.Append(sql2);
                        //Console.WriteLine("strSql=" + strSql.ToString());
                        sql = strSql.ToString();
                        //sql.TrimEnd(',');
                        using (OracleCommand cmd = new OracleCommand(strSql.ToString(), con))
                        {
                            using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                            {
                                p1.Value = wkt;
                                cmd.Parameters.Add(p1);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                //return sql;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + "");
                Console.WriteLine("华丽的分割线*************************************华丽的分割线");
                Console.WriteLine("osmid= " + osmid);
                Console.WriteLine(ex.ToString());
                //return null;
            }
        }
        
        /// <summary>
        /// 点数据入库(如出现表或视图不存在则执行此步以最小减少数据损失)
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public void insertPointDataBySql1(string tableName, List<Ppoint> pointList)
        {
            List<Ppoint> list = pointList;

            string sql = "";
            //string sql1 = string.Format("insert ");
            StringBuilder strSql = new StringBuilder();
            int source = 0;
            for (int i = 0; i < list.Count(); i++)
            {
                Ppoint point = list[i];
                string dsg = point.getDsg();
                dsg = FormatString(dsg);
                string tags = "";
                if (point.issimple == false)
                {
                    tags = point.getTags();
                    if (point.getTags().Length > 500)
                    {
                        tags = tags.Substring(0, 500);
                    }
                }
                else { tags = null; }
                point.setSource(source);
                string sql1 = string.Format("insert into {0}(osmid,lat,lon,versionid,starttime,changeset,userid,username,fc,dsg,tags,shape,source) values({1},{2},{3},{4},'{5}','{6}',{7},'{8}','{9}','{10}','{11}',sdo_geometry ('{12}',4326),{13})", tableName, point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getStartTime(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags, point.getWkt(),point.getSource());
                //strSql.Append(sql1);
                ////string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11})", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getTimeStamp(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags,"sdo_geometry (:geom,31297)");
                //string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',sdo_geometry ('{11}',4326))", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getTimeStamp(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags, point.getWkt());
                //strSql.Append(sql3);
                //sql = strSql.ToString();
                try {
                    using (OracleCommand cmd = new OracleCommand(sql1, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }             
            }
        }

        

        /// <summary>
        /// 点数据入库
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public string insertPointDataBySql(string tableName, List<Ppoint> pointList)
        {
            List<Ppoint> list = pointList;
            int source = 0;
            try
            {
                string sql = "";
                string sql1 = string.Format("insert all ");
                StringBuilder strSql = new StringBuilder();
                strSql.Append(sql1);
                for (int i = 0; i < list.Count(); i++)
                {
                    Ppoint point = list[i];
                    string dsg = point.getDsg();
                    dsg = FormatString(dsg);
                    string tags = "";
                    if (point.issimple == false)
                    {
                        tags = point.getTags(); 
                        if (point.getTags().Length > 500)
                        {
                            tags = tags.Substring(0, 500);
                        }
                    }
                    else { tags = null; }
                    point.setSource(source);
                    string sql2 = string.Format("into {0}(osmid,lat,lon,versionid,starttime,changeset,userid,username,fc,dsg,tags,shape,source) values", tableName);
                    strSql.Append(sql2);
                    //string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11})", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getTimeStamp(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags,"sdo_geometry (:geom,31297)");
                    string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',sdo_geometry ('{11}',4326),{12})", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getStartTime(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags, point.getWkt(),point.getSource());
                    strSql.Append(sql3);
                    sql = strSql.ToString();
                   
                }
                string sql4 = string.Format("select 1 from DAUL");
                strSql.Append(sql4);
                sql = strSql.ToString();
                return sql;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }

        }

        //点数据入库
        public void pointToOracle(string tableName, List<Ppoint> pointlist)
        {
            string sqlStr = insertPointDataBySql(tableName, pointlist);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            using (OracleCommand com = new OracleCommand(sqlStr, con))
            {
                try { com.ExecuteNonQuery(); }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    insertPointDataBySql1(tableName,pointlist);
                }
            }
        }
        /// <summary>
        /// relation入库
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="relationList"></param>
        public void insertRelationDataBySql(string tableName, List<Relation> relationList)
        {
            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                Relation relation = new Relation();
                List<string> pointlist = new List<string>();
                for (int i = 0; i < relationList.Count(); i++)
                {
                    relation = relationList[i];
                    string polyOsmid = relation.getId();
                    string[] polyOsmids = polyOsmid.Split(new string[1] { "," }, System.StringSplitOptions.None);
                    //StringBuilder checkStrSql = new StringBuilder("select pointsid from apoly where osmid in(");
                    //string osmids = "";
                    int length = polyOsmids.Length;
                    int count = 1;
                    const int number = 1000;
                    if (length > 0)
                    {
                        for (int j = 0 + (count - 1) * number; j < length; j++)
                        {
                            string sql1 = string.Format("select pointsid from apoly where osmid='{0}'", polyOsmids[j]);
                            using (OracleCommand com = new OracleCommand(sql1, con))
                            {
                                using (OracleDataReader rd = com.ExecuteReader())
                                {
                                    while (rd.Read())
                                    {
                                        pointlist.Add(rd[0].ToString());

                                    }
                                }
                            }
                            //osmids += polyOsmids[j] + ",";
                        }
                    }
                    //checkStrSql.Append(osmids);
                    //checkStrSql.Append(")");

                    //using (OracleCommand com = new OracleCommand(checkStrSql.ToString(), con))
                    //{
                    //    using (OracleDataReader rd = com.ExecuteReader())
                    //    {
                    //        while (rd.Read())
                    //        {
                    //            pointlist.Add(rd[0].ToString());

                    //        }
                    //    }
                    //}
                }
                string wkt = "";
                string wktString = "";
                int source = 0;
                for (int i = 0; i < pointlist.Count; i++)
                {
                    string osmIds = pointlist[i];
                    string[] pointOsmids = osmIds.Split(new string[1] { "," }, System.StringSplitOptions.None);
                    List<Ppoint> latLonList = new List<Ppoint>();
                    for (int j = 0; j < pointOsmids.Length; j++)
                    {
                        
                        string checkStrSql = string.Format("select lat,lon from apoint where osmid='{0}'", pointOsmids[j]);
                        using (OracleCommand conmand = new OracleCommand(checkStrSql, con))
                        {
                            using (OracleDataReader rd = conmand.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    Ppoint point = new Ppoint();
                                    point.setLat(float.Parse(rd["lat"].ToString()));
                                    point.setLon(float.Parse(rd["lon"].ToString()));
                                    latLonList.Add(point);
                                }
                            }
                        }
                    }
                   
                    if (latLonList.Count <= 0)
                    {
                        continue;
                    }
                    string linestring = "";
                    for (int j = 0; j < latLonList.Count; j++)
                    {
                        linestring += latLonList[j].getLon() + " ";
                        linestring += latLonList[j].getLat() + ",";
                        
                    }
                    if (linestring.Length > 0)
                    {
                        linestring = linestring.Substring(0, linestring.Length - 1);
                    }
                    wktString += "((" + linestring + "))" + ",";

                    //wkt = "POLYGON(((1 1,5 1,5 5,1 5,1 1)),((2 2,2 3,3 3,3 2,2 2)))";

                }
                wktString = wktString.Substring(0, wktString.Length - 1);

                if (wktString.Length > 4000)
                {
                    wkt = "";
                }
                else
                {
                    wkt = "MULTIPOLYGON(" + wktString + ")";
                }
                relation.setSource(source);
                if (wkt.Length > 0)
                {
                    StringBuilder strSql = new StringBuilder();
                    string sql1 = string.Format("insert into {0} (osmid,versionid,starttime,changeset,userid,username,fc,dsg,tags,trustvalue,shape,source) values", tableName);
                    strSql.Append(sql1);
                    string sql2 = string.Format("({0},{1},'{2}','{3}',{4},'{5}','{6}','{7}','{8}',{9},{10},{11})", relation.getOsmid(), relation.getVersion(), relation.getStartTime(), relation.getChangeset(), relation.getUserid(), "", relation.getFc(), relation.getDsg(), relation.getTags(), relation.getTrustvalue(), "sdo_geometry ( :geom,4326)",relation.getSource());

                    strSql.Append(sql2);
                    //Console.WriteLine("strSql=" + strSql.ToString());
                    string sql = strSql.ToString();
                    //sql.TrimEnd(',');
                    using (OracleCommand cmd = new OracleCommand(strSql.ToString(), con))
                    {
                        using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                        {
                            p1.Value = wkt;
                            cmd.Parameters.Add(p1);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
        }
        #endregion
        #region by dy

        //创建基态数据表
        public void createTable()
        {
            string tableName = "";
            //建表polygon
            tableName = "apoly";
            //string createTableSql1 = "(objectId NUMBER(30) primary key not null,osmid NUMBER(30) NOT NULL,version NUMBER(10),timestamp VARCHAR2(30),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,pointsId VARCHAR2(4000),shape SDO_GEOMETRY,polytype NUMBER(4))";
            string createTableSql1 = "(objectid NUMBER(30) primary key NOT NULL, osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,pointsId VARCHAR2(4000),shape SDO_GEOMETRY,source NUMBER(4),matchid  NUMBER(30))";
            createTable(tableName, createTableSql1);

            //建表line
            tableName = "aline";
            //string createTableSql2= "(objectId NUMBER(30) primary key not null,osmid NUMBER(30) NOT NULL,version NUMBER(10),timestamp VARCHAR2(30),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,pointsId VARCHAR2(4000),shape SDO_GEOMETRY,polytype NUMBER(4))";
            string createTableSql2 = "(objectid NUMBER(30) primary key NOT NULL, osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,pointsId VARCHAR2(4000),shape SDO_GEOMETRY,source NUMBER(4),matchid  NUMBER(30))";
            createTable(tableName, createTableSql2);

            //建表point
            tableName = "apoint";
            //string createTableSql3 = "(objectId NUMBER(30) primary key not null,osmid NUMBER(30) NOT NULL,lat FLOAT,lon FLOAT,version NUMBER(10),timestamp VARCHAR2(30),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),shape SDO_GEOMETRY)";
            string createTableSql3 = "(objectid NUMBER(30) primary key NOT NULL,osmid NUMBER(30),lat FLOAT,lon FLOAT,versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),shape SDO_GEOMETRY,source NUMBER(4),matchid  NUMBER(30))";
            createTable(tableName, createTableSql3);


            tableName = "daul";
            string createTablesql4 = "(osmid NUMBER(30) NOT NULL)";
            createTable(tableName, createTablesql4);//辅助表

            con.Close();
        }

        /// <summary>
        /// 创建序列自增，by dy20181006
        /// </summary>
        /// <param name="tableName"></param>
        public void CreateSequenceSelfIncrease(string tableName)
        {
             string sql1 = string.Format("create sequence SEQ_{0} start with 1 increment by 1 nomaxvalue nominvalue nocache",tableName);
              string sql = "select count(*)  from user_sequences where SEQUENCE_NAME =upper('SEQ_"+ tableName + "')";
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                using (OracleCommand conmand = new OracleCommand(sql, con))
                {
                    using (OracleDataReader rd = conmand.ExecuteReader())
                    {
                        int count = 0;
                        while (rd.Read())
                        {
                            count += Int32.Parse(rd["count(*)"].ToString());
                        }
                        if (count > 0)
                        {
                            sql = "drop SEQUENCE SEQ_" + tableName;
                            conmand.CommandText = sql;
                            conmand.ExecuteNonQuery();
                        }
                    }
                }
            sqlExecute(sql1);
            string sql2 = "CREATE OR REPLACE TRIGGER tg_"+tableName+" before INSERT ON " + tableName + " FOR EACH ROW WHEN (new.objectid is null) begin select SEQ_"+tableName+".nextval into:new.objectid from dual; end;";
            sqlExecute(sql2);

        }
        //根据表名和sql语句创建单个表
        public void createTable(string tableName, string createTableSql)
        {
            try
            {
                string sql = "select count(*)  from USER_TABLES where Table_Name = upper('" + tableName + "')";
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                using (OracleCommand conmand = new OracleCommand(sql, con))
                {
                    using (OracleDataReader rd = conmand.ExecuteReader())
                    {
                        int count = 0;
                        while (rd.Read())
                        {
                            count += Int32.Parse(rd["count(*)"].ToString());
                        }
                        if (count > 0)
                        {
                            sql = "drop table " + tableName;
                            conmand.CommandText = sql;
                            conmand.ExecuteNonQuery();
                        }

                        //createTableSql = "(objectId NUMBER(20) NOT NULL,osmid NUMBER(20) NOT NULL,version NUMBER(4),timestamp TIMESTAMP(6),changeset VARCHAR2(500),userid NUMBER(20),username VARCHAR2(20),tags VARCHAR2(1000),trustvalue FLOAT,pointsId VARCHAR2(1000),shape SDO_GEOMETRY,polytype NUMBER(4))";
                        sql = "CREATE TABLE " + tableName + createTableSql;
                        conmand.CommandText = sql;
                        conmand.ExecuteNonQuery();

                        if (tableName == "daul")
                        {
                            sql = "insert into " + tableName + " values(123456)";
                            conmand.CommandText = sql;
                            conmand.ExecuteNonQuery();
                        }
                        else 
                        {
                            CreateSequenceSelfIncrease(tableName);
                        }
                    }

                }
 

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                con.Close();
            }

        }
        /// <summary>
        /// 创建历史数据表 by zbl 20181109
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="createTableSql"></param>
        public void createHistoryTable(string tableName, string createTableSql)
        {
           try
            {
                string sql = "select count(*)  from USER_TABLES where Table_Name = upper('" + tableName + "')";
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                using (OracleCommand conmand = new OracleCommand(sql, con))
                {
                    using (OracleDataReader rd = conmand.ExecuteReader())
                    {
                        int count = 0;
                        while (rd.Read())
                        {
                            count += Int32.Parse(rd["count(*)"].ToString());
                        }
                        sql = "CREATE TABLE " + tableName + createTableSql;
                        conmand.CommandText = sql;
                        conmand.ExecuteNonQuery();

                        if (tableName == "daul")
                        {
                            sql = "insert into " + tableName + " values(123456)";
                            conmand.CommandText = sql;
                            conmand.ExecuteNonQuery();
                        }
                        else
                        {
                            CreateSequenceSelfIncrease(tableName);
                        }
                    }
                }
            }
           catch (Exception ex)
           {
               MessageBox.Show(ex.ToString());
           }
           finally
           {
               con.Close();
           }
        }
        /// <summary>
        /// 读取默认路径下\\Debug\\OsmSql\\存好的txt文档，行其中的sql代码，设计为创建、删除表格
        /// </summary>
        /// <param name="con"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool CreateTable(string fileName)
        {
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
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
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="oldString"></param>
        /// <returns></returns>
        public static string FormatString(string oldString)
        {
            if (string.IsNullOrEmpty(oldString))
            {
                return null;
            }
            else
            {
                if (oldString.Contains('\''))
                {
                    return oldString.Replace("'", "''");
                }
                else if (oldString.Contains('\\'))
                {
                    return oldString.Replace("\\", "\\\\");
                }
                return oldString;
            }
        }

        #endregion



        #region 增量数据入库 by zbl 20181006
        /// <summary>
        /// 创建增量数据表  by zbl 2018.7.3
        /// </summary>
        public void createOscTable()
        {      
            string OsctableName = "";
            if (IsExistTable("OSCAREA"))//判断当前数据库中是否存在增量线数据表
            {
                //string sql1 = string.Format("delete from USER_SDO_GEOM_METADATA where TABLE_NAME='OSCAREA'");//删除元数据视图中的数据表
                //sqlExecuteUnClose(sql1);
                //sql1 = string.Format("drop index idx_OSCAREA");//删除数据表中的索引
                //sqlExecuteUnClose(sql1);
                string sql = string.Format("drop table OSCAREA");//若数据表存在将其删除
                sqlExecuteUnClose(sql);
            }
            OsctableName = "OSCAREA";
            string createOscTableSql1 = "(objectid NUMBER(30) primary key NOT NULL,osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(500),dsg VARCHAR2(500),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,pointsId VARCHAR2(4000),shape SDO_GEOMETRY,changeType VARCHAR2(30),source NUMBER(4) )";
            createTable(OsctableName, createOscTableSql1);//建表OSCArea
            createTriger(OsctableName);
            //string sql2 = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID) VALUES ('OSCAREA', 'shape',MDSYS.SDO_DIM_ARRAY  (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050),MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050)),4326)");
            //sqlExecuteUnClose(sql2);
            //string sql3 = string.Format("create index idx_OSCAREA on OSCAREA(shape) indextype is mdsys.spatial_index");//by 20180916修改
            //sqlExecuteUnClose(sql3);

            if (IsExistTable("OSCLINE"))//判断当前数据库中是否存在增量线数据表
            {
                //string sql1 = string.Format("delete from USER_SDO_GEOM_METADATA where TABLE_NAME='OSCLINE'");//删除元数据视图中的数据表
                //sqlExecuteUnClose(sql1);
                //sql1 = string.Format("drop index idx_OSCLINE");//删除数据表中的索引
                //sqlExecuteUnClose(sql1);
                string sql = string.Format("drop table OSCLINE");//若数据表存在将其删除
                sqlExecuteUnClose(sql);
            }
            OsctableName = "OSCLINE";
            string createOscTableSql2 = "(objectid NUMBER(30) primary key NOT NULL,osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(500),dsg VARCHAR2(500),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,pointsId VARCHAR2(4000),shape SDO_GEOMETRY,changeType VARCHAR2(30),source NUMBER(4))";
            createTable(OsctableName, createOscTableSql2);//建表OSCLine
            createTriger(OsctableName);
            //string sql4 = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID) VALUES ('OSCLINE', 'shape',MDSYS.SDO_DIM_ARRAY  (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050),MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050)),4326)");
            //sqlExecuteUnClose(sql4);
            //string sql5 = string.Format("create index idx_OSCLINE on OSCLINE(shape) indextype is mdsys.spatial_index");//by 20180916修改
            //sqlExecuteUnClose(sql5);

            if (IsExistTable("OSCPOINT"))//判断当前数据库中是否存在增量线数据表
            {
                //string sql1 = string.Format("delete from USER_SDO_GEOM_METADATA where TABLE_NAME='OSCPOINT'");//删除元数据视图中的数据表
                //sqlExecuteUnClose(sql1);
                //sql1 = string.Format("drop index idx_OSCPOINT");//删除数据表中的索引
                //sqlExecuteUnClose(sql1);
                string sql = string.Format("drop table OSCPOINT");//若数据表存在将其删除
                sqlExecuteUnClose(sql);
            }
            OsctableName = "OSCPOINT";
            string createOscTableSql3 = "(objectid NUMBER(30) primary key NOT NULL,osmid NUMBER(30),lat FLOAT,lon FLOAT,versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(500),dsg VARCHAR2(500),tags VARCHAR2(2000),shape SDO_GEOMETRY,changeType VARCHAR2(30),source NUMBER(4))";
            createTable(OsctableName, createOscTableSql3);//建表OSCPoint
            createTriger(OsctableName);
            //string sql6 = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID) VALUES ('OSCPOINT', 'shape',MDSYS.SDO_DIM_ARRAY  (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050),MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050)),4326)");
            //sqlExecuteUnClose(sql6);
            //string sql7 = string.Format("create index idx_OSCPOINT on OSCPOINT(shape) indextype is mdsys.spatial_index");//by 20180916修改
            //sqlExecuteUnClose(sql7);

            OsctableName = "daul";
            string createTablesql4 = "(osmid NUMBER(30) NOT NULL )";
            createTable(OsctableName, createTablesql4);//辅助表
        }

        

        ///<summary>
        /// osm增量Node数据入库    by zbl 2018.7.4
        ///<summary>
        public string insertoscPointDataBySql(string OsctableName, List<OscDataNode> oscpointlist)
        {
            List<OscDataNode> osclist = oscpointlist;
            int source = 0;
            string endtime = "-1";
            try
            {
                //string sql = "";
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                StringBuilder strSql = new StringBuilder();
                for (int i = 0; i < osclist.Count(); i++)
                {
                    OscDataNode point = osclist[i];
                    string dsg = point.getDsg();
                    dsg = FormatString(dsg);
                    string tags = "";
                    if (point.issimple == false)
                    {
                        tags = point.getTags();
                        if (point.getTags().Length > 500)
                        {
                            tags = tags.Substring(0, 500);
                        }
                    }
                    else { tags = null; };
                    point.setSource(source);
                    point.setEndTime(endtime);
                    string sql2 = string.Format("insert into {0} (osmid,lat,lon,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,changeType,shape,source) values({1},{2},{3},{4},'{5}','{6}','{7}',{8},'{9}','{10}','{11}','{12}','{13}',sdo_geometry ('{14}',4326),{15})", OsctableName, point.getOscid(), point.getLat(), point.getLon(), point.getVersion(), point.getStartTime(),point .getEndTime(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags, point.getChangetype(), point.getWkt(),point.getSource ());
                    //strSql.Append(sql2);
                    //sql = strSql.ToString();
                    sqlExecuteUnClose(sql2);
                }
                //string sql4 = string.Format(" select 1 from daul");
                ////strSql.Append(sql4);
                ////sql = strSql.ToString();
                //sqlExecuteUnClose(sql4);
                //return sql;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return null;
        }
        #region 废弃的增量点入库代码
        //public void OscpointToOracle(string OsctableName, List<OscDataNode> oscpointlist)
        //{
        //    string sqlStr = insertoscPointDataBySql(OsctableName, oscpointlist);
        //    if (con.State != ConnectionState.Open)
        //    {
        //        con.Open();
        //    }
        //    using (OracleCommand com = new OracleCommand(sqlStr, con))
        //    {
        //        try { com.ExecuteNonQuery(); }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString());
        //            insertOscPointDataBySql(OsctableName, oscpointlist);
        //        }
        //    }
        #endregion

        /// <summary>
        ///  osm增量Way数据入库   by zbl 2018.7.4
        /// </summary>
        /// <param name="OsctableName"></param>
        /// <param name="oscwaylist"></param>
        /// <returns></returns>
        public string insertoscWayDataBySql(string OsctableName, List<OscDataWay> oscwaylist)
        {
            string oscid = "";
            string tags = "";
            int source = 0;
            string endtime = "-1";
            double userreputation = 0.00;
            List<OscDataWay> waylist = oscwaylist;
            try
            {

                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                string sql= "";
                for (int i = 0; i < waylist.Count(); i++)
                {
                    OscDataWay way = waylist[i];
                    string pointsId = way.getPointsld();
                    if (pointsId.Length > 4000) continue;//点字符数多于4000的不要
                    string[] pointids = pointsId.Split(new string[1] { "," }, System.StringSplitOptions.None);

                    List<OscDataNode> latLonlist = new List<OscDataNode>();
                    StringBuilder checkStrSql = new StringBuilder("select lat,lon from OSCPOINT where osmid in( ");
                    //string pointIds = "";
                    int length = pointids.Length;
                    if (length > 0)
                    {
                        for (int j = 0; j < pointids.Length; j++)
                        {
                            string pointid = pointids[j];
                            sql = string.Format("select lat,lon from OSCPOINT where osmid='{0}'", pointid);
                            using (OracleCommand command = new OracleCommand(sql, con))
                            {
                                using (OracleDataReader rdr = command.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        OscDataNode point = new OscDataNode();
                                        point.setLat(float.Parse(rdr["lat"].ToString()));
                                        point.setLon(float.Parse(rdr["lon"].ToString()));
                                        latLonlist.Add(point);
                                    }
                                }
                            }
                        }
                    }
                    if (latLonlist.Count <= 0)
                    {
                        continue;
                    }
                    string linestring = "";
                    for (int j = 0; j < latLonlist.Count; j++)
                    {
                        linestring += latLonlist[j].getLon() + " ";
                        linestring += latLonlist[j].getLat() + ",";
                    }

                    int polyType = 1;
                    if (linestring.Length > 0)
                    {
                        linestring = linestring.Substring(0, linestring.Length - 1);
                    }
                    string wkt = "";
                
                    if (pointids[0].Equals(pointids[pointids.Length - 1]))
                    {

                        if (linestring.Length < 110) // 20180826修改
                        {
                            wkt = "";
                        }
                        else
                        {
                            polyType = 2;
                            wkt = "POLYGON((" + linestring + "))";
                        }
                       
                    }
                    else
                    {
                        polyType = 1;
                        wkt = "LINESTRING(" + linestring + ")";
                    }
                
                    way.setPolytype(polyType);

                    oscid = way.getOscid().ToString();
                    tags = way.getTags();
                    way.setSource (source);
                    way.setEndTime(endtime);
                    if (way.getTags().Length > 400)
                    {
                        tags = tags.Substring(0, 400);
                    }

                    if (wkt == null)
                    {
                        Console.WriteLine("poly.getWkt()=" + wkt);
                        StringBuilder strSql = new StringBuilder();
                        string sql1 = string.Format("insert into {0} (osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,pointsId,changeType,shape,source) values({1},{2},'{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11},{12},'{13}','{14}',{15},{16})", OsctableName, way.getOscid(), way.getVersion(), way.getStartTime(), way.getEndTime(), way.getChangeset(), way.getUserid(), "", way.getFc(), way.getDsg(), tags, way.getTrustvalue(), userreputation, way.getPointsld(), way.getChangetype(), "sdo_geometry ( :geom,4326)", way.getSource());
                        strSql.Append(sql1);
           

                        continue;
                    }
                    if (wkt.Length > 0)
                    {

                        StringBuilder strSql = new StringBuilder();
                        string sql1 = string.Format("insert into {0} (osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,pointsId,changeType,shape,source) values({1},{2},'{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11},{12},'{13}','{14}',{15},{16})", OsctableName, way.getOscid(), way.getVersion(), way.getStartTime(), way.getEndTime(), way.getChangeset(), way.getUserid(), "", way.getFc(), way.getDsg(), tags, way.getTrustvalue(), userreputation, way.getPointsld(), way.getChangetype(), "sdo_geometry ( :geom,4326)", way.getSource());
                        strSql.Append(sql1);
                        using (OracleCommand cmd = new OracleCommand(strSql.ToString(), con))
                        {
                            using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                            {
                                p1.Value = wkt;
                                cmd.Parameters.Add(p1);
                                cmd.ExecuteNonQuery();
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + "");
                Console.WriteLine("华丽的分割线*************************************华丽的分割线");
                Console.WriteLine("osmid= " + oscid);
                Console.WriteLine(ex.ToString());
            }
            return waylist.ToString();
        }

        /// <summary>
        /// osm增量Relation数据入库  by zbl 2018.7.9
        /// </summary>
        /// <param name="OsctableName"></param>
        /// <param name="oscrelationlist"></param>
        /// <returns></returns>
        public string insertoscRelationDataBySql(string OsctableName, List<OscDataRelation> oscrelationlist)
        {
            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                int source = 0;
                string endtime = "-1";
                OscDataRelation relation = new OscDataRelation();
                List<string> pointlist = new List<string>();
                for (int i = 0; i < oscrelationlist.Count(); i++)
                {
                    relation = oscrelationlist[i];
                    string polyOscid = relation.getId();
                    string[] polyOscids = polyOscid.Split(new string[1] { "," }, System.StringSplitOptions.None);
                    StringBuilder checkStrSql = new StringBuilder("select pointsid from OSCAREA where osmid in(");
                    int length = polyOscids.Length;
                    int count = 1;
                    const int number = 1000;
                    if (length > 0)
                    {
                        for (int j = 0 + (count - 1) * number; j < length; j++)
                        {
                           string sql1 = string.Format("select pointsid from OSCAREA where osmid='{0}'", polyOscids[j]);
                           using (OracleCommand com = new OracleCommand(sql1, con))
                           {
                               using (OracleDataReader rd = com.ExecuteReader())
                               {
                                   while (rd.Read())
                                   {
                                       pointlist.Add(rd[0].ToString());

                                   }
                               }
                           }
                        }
                    }
                }
                string wkt = "";
                string wktString = "";
                for (int i = 0; i < pointlist.Count; i++)
                {
                    string oscIds = pointlist[i];
                    string[] pointOscids = oscIds.Split(new string[1] { "," }, System.StringSplitOptions.None);
                    List<OscDataNode> latLonList = new List<OscDataNode>();
                    for (int j = 0; j < pointOscids.Length; j++)
                    {
                        string checkStrSql = string.Format("select lat,lon from OSCPOINT where osmid='{0}'", pointOscids[j]);
                        using (OracleCommand conmand = new OracleCommand(checkStrSql, con))
                        {
                            using (OracleDataReader rd = conmand.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    OscDataNode point = new OscDataNode();
                                    point.setLat(float.Parse(rd["lat"].ToString()));
                                    point.setLon(float.Parse(rd["lon"].ToString()));
                                    latLonList.Add(point);
                                }
                            }
                        }
                    }
                    if (wktString.Length > 0)
                    {
                        if (latLonList.Count <= 0)
                        {
                            continue;
                        }
                        string linestring = "";
                        for (int j = 0; j < latLonList.Count; j++)
                        {
                            linestring += latLonList[j].getLon() + " ";
                            linestring += latLonList[j].getLat() + ",";
                        }
                        if (linestring.Length > 0)
                        {
                            linestring = linestring.Substring(0, linestring.Length - 1);
                        }

                        wktString += "((" + linestring + "))" + ",";
                        wktString = wktString.Substring(0, wktString.Length - 1);
                    }

                }
                if (wktString.Length > 4000 || wktString.Length ==0)// 20180818 修改 
                {
                    wkt = "";
                }
                else { wkt = "MULTIPOLYGON(" + wktString + ")"; }
                relation.setSource(source);
                relation.setEndTime(endtime);
                if (wkt.Length > 0)
                {

                    StringBuilder strSql = new StringBuilder();
                    string sql1 = string.Format("insert into {0} (osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,changeType,shape,source) values", OsctableName);
                    strSql.Append(sql1);
                    string sql2 = string.Format("({0},{1},'{2}','{3}','{4}',{5},'{6}','{7}','{8}','{9}',{10},'{11}',{12},{13})", relation.getOscid(), relation.getVersion(), relation.getStartTime(),relation.getEndTime(), relation.getChangeset(), relation.getUserid(), "", relation.getFc(), relation.getDsg(), relation.getTags(), relation.getTrustvalue(), relation.getChangeType(), "sdo_geometry ( :geom,4326 )",relation.getSource ());
                    strSql.Append(sql2);
                    string sql = strSql.ToString();
                    using (OracleCommand cmd = new OracleCommand(strSql.ToString(), con))
                    {
                        using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                        {
                            p1.Value = wkt;
                            cmd.Parameters.Add(p1);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return oscrelationlist.ToString();
        }
        #endregion 

        #region by dy20180711
        /// <summary>
        /// 读取所选的数据库内容
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public OracleDataReader queryReader(string queryString)
        {
            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                 OracleCommand Command = new OracleCommand(queryString, con); 
                Command.CommandTimeout = 10000000;
                OracleDataReader dr = Command.ExecuteReader();
                Console.WriteLine("successfully Reader!");
                //con.Close();
                return dr;
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

        public DataTable dhfQueryReader(string queryString)
        {
            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                OracleCommand Command = new OracleCommand(queryString, con);
                Command.CommandTimeout = 10000000;
                OracleDataReader dr = Command.ExecuteReader();
                Console.WriteLine("successfully Reader!");
                OracleDataAdapter da = new OracleDataAdapter(Command);
                DataTable ds = new DataTable();
                da.Fill(ds);
                return ds;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            { 
                //con.Close(); 
            }
        }

        public OracleDataReader ReaderFloat(string queryString)
        {
            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                OracleCommand Command = new OracleCommand(queryString, con);
                Command.CommandTimeout = 10000000;
                OracleDataReader reader = Command.ExecuteReader();
                Console.WriteLine("successfully Reader!");
                // Always call Read before accessing data.
                while (reader.Read())
                {
                    Console.WriteLine(reader.GetFloat(0) + ", " + reader.GetFloat(1));
                }

                // Always call Close when done reading.
                reader.Close();
                //con.Close();
                return reader;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

        /// <summary>
        /// 读取数据表中某一列的数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isDistinct"></param>
        /// <returns></returns>
        public string[] getColumnData(string tableName, string columnName, Boolean isDistinct)
        {
            string sql = "";
            List<string> cd = null;
            if (isDistinct)
            {
                sql = string.Format("select distinct {0} from {1}", columnName, tableName);
            }
            else
            {
                sql = string.Format("select {0} from {1}", columnName, tableName);
            }
            using (OracleDataReader dr = queryReader(sql))
            {
                if (dr == null || !dr.HasRows)
                {
                    return null;
                }
                try
                {
                    cd = new List<string>();
                    while (dr.Read())
                    {
                        cd.Add(dr.GetValue(0).ToString());
                    }
                    return cd.ToArray();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally { con.Close(); }

            }
            return null;
        }
        /// <summary>
        /// 读取数据表中某一列的数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isDistinct"></param>
        /// <returns></returns>
        public string[] getColumnData(string tableName, string columnName, Boolean isDistinct, string orderField)
        {
            string sql;
            List<string> cd = null;
            if (isDistinct)
            {
                sql = string.Format("select distinct {0} from {1} order by {2}", columnName, tableName, orderField);
            }
            else
            {
                sql = string.Format("select {0} from {1} order by {2}", columnName, tableName, orderField);
            }
            using (OracleDataReader dr = queryReader(sql))
            {
                try
                {
                    if (dr == null)
                    {
                        return null;
                    }
                    if (dr.HasRows)
                    {
                        cd = new List<string>();
                        while (dr.Read())
                        {
                            cd.Add(dr.GetValue(0).ToString());
                        }
                        dr.Dispose();
                        dr.Close();
                        return cd.ToArray();

                    }
                }
                finally
                {
                    con.Close();
                }
            }
            return null;

        }

        /// <summary>
        /// 判断表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool IsExistTable(string tableName)
        {
            string sql = string.Format("select count(*) from user_tab_comments where TABLE_NAME='{0}'", tableName);

            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            using (OracleCommand com = new OracleCommand(sql, con))
            {
                using (OracleDataReader dr = com.ExecuteReader())
                {
                    if (dr != null && dr.HasRows)
                    {
                        dr.Read();
                        if ("1".Equals(dr.GetValue(0).ToString()))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 判断空间索引表是否存在是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool IsExistSpatialTable(string tableName)
        {
            //string sql = string.Format("select count(*) from USER_SDO_GEOM_METADATA where TABLE_NAME='{0}'", tableName);
            string sql = "select count(*) from USER_SDO_GEOM_METADATA where TABLE_NAME=upper('" + tableName + "')";
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            using (OracleCommand com = new OracleCommand(sql, con))
            {
                using (OracleDataReader dr = com.ExecuteReader())
                {
                    if (dr != null && dr.HasRows)
                    {
                        dr.Read();
                        if ("1".Equals(dr.GetValue(0).ToString()))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 判断空间索引表是否存在是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool IsExistSequence(string tableName)
        {
            string sql = string.Format("select count(*) from USER_SDO_GEOM_METADATA where TABLE_NAME='{0}'", tableName);
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            using (OracleCommand com = new OracleCommand(sql, con))
            {
                using (OracleDataReader dr = com.ExecuteReader())
                {
                    if (dr != null && dr.HasRows)
                    {
                        dr.Read();
                        if ("1".Equals(dr.GetValue(0).ToString()))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableName"></param>
        public void DropTable(string tableName)
        {
            string sql = string.Format("drop table {0}", tableName);
            using (OracleCommand com = new OracleCommand(sql, con))
            {
                try { com.ExecuteNonQuery(); }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void close()
        {
            if (con.State != ConnectionState.Closed)
            {
                con.Close();
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void dispose()
        {
            con.Dispose();
        }

        /// <summary>
        /// 用OracleCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除等操作 无错返回操作的行数  有错返回错误原因
        /// </summary>
        /// <param name="changeString"></param>
        /// <param name="conn"></param>
        public int sqlExecuteUnClose(string sql)
        {
            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                OracleCommand objCommand = new OracleCommand(sql, con);
                objCommand.CommandTimeout = 1000;
                int No = objCommand.ExecuteNonQuery();
                return No;
            }
            catch (OracleException e)
            {
                Console.WriteLine("错误码：" + e.ErrorCode);
                if (e.ErrorCode.Equals("23505"))
                {
                    Console.WriteLine("违反唯一性约束！");
                }
                else
                {

                    Console.WriteLine(e.ToString());
                }
                con.Close();
                return -1;
            }
            finally
            {

            }

        }
        #endregion

        /// <summary>
        /// by zbl 2018.7.13
        /// 用OracleCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除等操作 无错返回操作的行数  有错返回错误原因 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int sqlExecute(string sql)
        {
            try
            {
                OracleDBHelper helper = new OracleDBHelper();
                OracleConnection con = helper.getOracleConnection();//连接数据库
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                OracleCommand objCommand = new OracleCommand(sql, con);
                objCommand.CommandTimeout = 1000;
                int No = objCommand.ExecuteNonQuery();
                return No;
            }
            catch (OracleException e)
            {
                Console.WriteLine("错误码：" + e.ErrorCode);
                if (e.ErrorCode.Equals("23505"))
                {
                    Console.WriteLine("违反唯一性约束！");
                }
                else
                {
                    Console.WriteLine(e.ToString());
                }
                return -1;
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        ///董海峰20180816静态
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, params OracleParameter[] cmdParms)
        {
            OracleDBHelper helper = new OracleDBHelper();
            using (OracleConnection con = helper.getOracleConnection()) //连接数据库
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        cmd.Connection = con;
                        cmd.CommandText = SQLString;

                        OracleCommand cmd1 = PrepareCommand(cmd, con, null, SQLString, cmdParms);
                        int rows = cmd1.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (Oracle.ManagedDataAccess.Client.OracleException ex)
                    {
                        throw (ex);
                    }
                }
            }
        }

        private static OracleCommand PrepareCommand(OracleCommand cmd, OracleConnection conn, OracleTransaction trans, string cmdText, OracleParameter[] cmdParms)
        {
            //if (conn.State != ConnectionState.Open)
            //    conn.Open();
            //cmd.Connection = conn;
            //cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (OracleParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&(parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
            return cmd;
        }
        /// <summary>
        /// by zbl 2018.7.13
        /// 用OracleCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除等操作 无错返回操作的行数  有错返回错误原因 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int ExecuteSql(string sql)
        {
            try
            {
                OracleDBHelper helper = new OracleDBHelper();
                OracleConnection con = helper.getOracleConnection();//连接数据库
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                OracleCommand objCommand = new OracleCommand(sql, con);
                objCommand.CommandTimeout = 1000;
                int No = objCommand.ExecuteNonQuery();
                //con.Close();
                return No;
            }
            catch (OracleException e)
            {
                Console.WriteLine("错误码：" + e.ErrorCode);
                if (e.ErrorCode.Equals("23505"))
                {
                    Console.WriteLine("违反唯一性约束！");
                }
                else
                {
                    Console.WriteLine(e.ToString());
                }
                return -1;
            }
            //finally
            //{
            //    con.Close();
            //}
        }
        #region 废弃
        //public OracleConnection getOracleConnection()
        //{
        //    if (con.State == ConnectionState.Closed)
        //    {
        //        con.Open();
        //    }
        //    return con;
        //}

        ///// <summary>
        ///// 给sql语句中的字符串格式化，避免字符串中出现单引号，反斜线
        ///// </summary>
        ///// <param name="oldString"></param>
        ///// <returns></returns>
        //public static string FormatString(string oldString)
        //{
        //    if (string.IsNullOrEmpty(oldString))
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        if (oldString.Contains('\''))
        //        {
        //            return oldString.Replace("'", "''");
        //        }
        //        else if (oldString.Contains('\\'))
        //        {
        //            return oldString.Replace("\\", "\\\\");
        //        }
        //        return oldString;
        //    }
        //}
        ///// <summary>
        ///// 用NpgsqlCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除等操作 无错返回操作的行数  有错返回错误原因
        ///// </summary>
        ///// <param name="changeString"></param>
        ///// <param name="conn"></param>
        //public int sqlExecute(string sql)
        //{
        //    try
        //    {

        //        if (con.State == ConnectionState.Closed)
        //        {
        //            con.Open();
        //        }

        //        OracleCommand command = new OracleCommand();
        //        command.CommandText = sql;
        //        command.CommandTimeout = 1000;
        //        int No = command.ExecuteNonQuery();
        //        return No;
        //    }
        //    catch (OracleException e)
        //    {
        //        Console.WriteLine("错误码：" + e.ErrorCode);
        //        if (e.ErrorCode.Equals("23505"))
        //        {
        //            Console.WriteLine("违反唯一性约束！");
        //        }
        //        else
        //        {

        //            Console.WriteLine(e.ToString());
        //        }
        //        return -1;
        //    }
        //    finally
        //    {
        //        con.Close();
        //    }

        //}
        ///// <summary>
        ///// 用NpgsqlCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除等操作 无错返回操作的行数  有错返回错误原因
        ///// </summary>
        ///// <param name="changeString"></param>
        ///// <param name="conn"></param>
        //public int sqlExecuteUntreatException(string sql)
        //{
        //    try
        //    {

        //        if (con.State == ConnectionState.Closed)
        //        {
        //            con.Open();
        //        }
        //        OracleCommand command = new OracleCommand(sql, con);
        //        command.CommandText = sql;
        //        command.CommandTimeout = 1000;
        //        int No = command.ExecuteNonQuery();
        //        return No;
        //    }
        //    catch (OracleException e)
        //    {
        //        throw new Exception(e.ToString());
        //    }
        //    finally
        //    {
        //        con.Close();
        //    }

        //}

        /// <summary>
        /// 用NpgsqlCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除等操作 无错返回操作的行数  有错返回错误原因
        /// </summary>
        /// <param name="changeString"></param>
        ///// <param name="conn"></param>
        //public int sqlExecuteUnClose(string sql)
        //{
        //    try
        //    {
        //        if (con.State == ConnectionState.Closed)
        //        {
        //            con.Open();
        //        }
        //        OracleCommand objCommand = new OracleCommand(sql, con);
        //        objCommand.CommandTimeout = 1000;
        //        int No = objCommand.ExecuteNonQuery();
        //        return No;
        //    }
        //    catch (OracleException e)
        //    {
        //        Console.WriteLine("错误码：" + e.ErrorCode);
        //        if (e.ErrorCode.Equals("23505"))
        //        {
        //            Console.WriteLine("违反唯一性约束！");
        //        }
        //        else
        //        {

        //            Console.WriteLine(e.ToString());
        //        }
        //        con.Close();
        //        return -1;
        //    }
        //    finally
        //    {

        //    }

        //}

        /// <summary>
        /// 读取所选的数据库内容
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        //public OracleDataReader queryReader(string queryString)
        //{
        //    try
        //    {
        //        if (con.State == ConnectionState.Closed)
        //        {
        //            con.Open();
        //        }
        //        OracleCommand Command = new OracleCommand(queryString, con); ;
        //        Command.CommandTimeout = 10000000;
        //        OracleDataReader dr = Command.ExecuteReader();
        //        Console.WriteLine("successfully Reader!");
        //        return dr;

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        return null;
        //    }

        //}

        //public List<string> GetAllTableName() { return GetAllTableName(null); }
        /// <summary>
        /// 获取所有表名
        /// </summary>
        /// <param name="preString"></param>
        /// <returns></returns>
        //public List<string> GetAllTableName(string preString)
        //{
        //    List<string> tableNames = new List<string>();
        //    string condition = "";
        //    if (preString != null)
        //    {
        //        //condition = string.Format("and TABLE_NAME like '{0}%'", preString);
        //        condition = string.Format("T.TABLE_NAME like '{0}%'", preString);
        //    }
        //    string sql = string.Format("select t.table_name from user_table t where {0};", condition);
        //    // string sql = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'public' {0};", condition);
        //    using (OracleDataReader dr = queryReader(sql))
        //    {
        //        if (dr == null || !dr.HasRows)
        //        {
        //            return tableNames;
        //        }
        //        while (dr.Read())
        //        {
        //            tableNames.Add(dr["TABLE_NAME"].ToString());
        //        }
        //    }
        //    return tableNames;
        //}

        //public DataTable queryBySql(String sql, OracleConnection con)
        //{
        //    DataTable table = new DataTable();
        //    table.Columns.Add("userid");
        //    table.Columns.Add("username");
        //    try
        //    {
        //        using (OracleCommand command = con.CreateCommand())
        //        {
        //            command.CommandText = sql;
        //            using (OracleDataReader dr = command.ExecuteReader())
        //            {
        //                while (dr.Read())
        //                {
        //                    DataRow row = table.NewRow();
        //                    row["username"] = dr["USERNAME"].ToString();
        //                    table.Rows.Add(row);
        //                }
        //                return table;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        return null;
        //    }

        //}


        /// <summary>
        /// 读取默认路径下\\Debug\\OsmSql\\存好的txt文档，行其中的sql代码，设计为创建、删除表格
        /// </summary>
        /// <param name="con"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //public static bool CreateTable(OracleConnection con, string fileName)
        //{
        //    string sqlCreateTbl = null;
        //    StreamReader myRead = new StreamReader(String.Format("OsmSql\\{0}.txt", fileName));
        //    while (!myRead.EndOfStream)
        //    {
        //        sqlCreateTbl += myRead.ReadLine();
        //    }
        //    myRead.Close();
        //    myRead.Dispose();
        //    try
        //    {
        //        using (OracleCommand cmd = new OracleCommand(sqlCreateTbl, con))
        //        {
        //            cmd.ExecuteNonQuery();
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        return false;
        //    }
        //}
        ///// <summary>
        ///// 读取默认路径下\\Debug\\OsmSql\\存好的txt文档，行其中的sql代码，设计为创建、删除表格
        ///// </summary>
        ///// <param name="con"></param>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //public bool CreateTable(string fileName)
        //{
        //    if (con.State != ConnectionState.Open)
        //    {
        //        con.Open();
        //    }
        //    string sqlCreateTbl = null;
        //    StreamReader myRead = new StreamReader(String.Format("OsmSql\\{0}.txt", fileName));
        //    while (!myRead.EndOfStream)
        //    {
        //        sqlCreateTbl += myRead.ReadLine();
        //    }
        //    myRead.Close();
        //    myRead.Dispose();
        //    try
        //    {
        //        using (OracleCommand cmd = new OracleCommand(sqlCreateTbl, con))
        //        {
        //            cmd.ExecuteNonQuery();
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        return false;
        //    }
        //}

        /// <summary>
        /// 针对点，线，面的国标多个图层建表法，即指定点线面表格要素与表名操作
        /// </summary>
        /// <param name="con"></param>
        /// <param name="filePath"></param>
        /// <param name="tblname"></param>
        /// <returns></returns>
        //public static int CreateTable(OracleConnection con, string filePath, string tblname)
        //{
        //    int sum = -100;
        //    string sqlCreateTbl = null;
        //    StreamReader myRead = new StreamReader(filePath);
        //    while (!myRead.EndOfStream)
        //    {
        //        sqlCreateTbl += myRead.ReadLine();
        //    }
        //    myRead.Close();
        //    myRead.Dispose();
        //    sqlCreateTbl = String.Format("Create Table {0}", tblname) + sqlCreateTbl;
        //    try
        //    {
        //        using (OracleCommand cmd = new OracleCommand(sqlCreateTbl, con))
        //        {
        //            sum = cmd.ExecuteNonQuery();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //    return sum;
        //}

        /// <summary>
        /// 建表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="createTableSql"></param>
        /// <returns></returns>
        //public Boolean createTable(string tableName, string createTableSql)
        //{
        //    try
        //    {
        //        string sql = "select count(*) from USER_TABLES where Table_Name=upper('" + tableName + "')";
        //        if (con.State == ConnectionState.Closed)
        //        {
        //            con.Open();
        //        }
        //        using (OracleCommand command = new OracleCommand(sql, con))
        //        {
        //            using (OracleDataReader dr = command.ExecuteReader())
        //            {
        //                int count = 0;
        //                while (dr.Read())
        //                {
        //                    count += Int32.Parse(dr["count(*)"].ToString());
        //                }
        //                if (count > 0)
        //                {
        //                    sql = "drop table" + tableName;
        //                    command.CommandText = sql;
        //                    command.ExecuteNonQuery();
        //                }
        //                sql = "CREATE TABLE" + tableName + createTableSql;
        //                command.CommandText = sql;
        //                command.ExecuteNonQuery();
        //            }

        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        return false;
        //    }
        //    finally { con.Close(); }
        //}
        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableName"></param>
        //public void DropTable(string tableName)
        //{
        //    string sql = string.Format("drop table {0};", tableName);
        //    sqlExecute(sql);
        //}
        /// <summary>
        /// 判断数据库是否存在tablename表
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        //public bool IsExistTable(string tableName)
        //{
        //    if (con.State == ConnectionState.Closed)
        //    {
        //        con.Open();
        //    }
        //    string sql = string.Format("select * from user_tab_comments where Table_Name='{0}'", tableName);
        //    using (OracleCommand cmd = new OracleCommand(sql, con))
        //    {
        //        using (OracleDataReader dr = cmd.ExecuteReader())
        //        {
        //            if (dr != null && dr.HasRows)
        //            {
        //                dr.Read();
        //                if ("1".Equals(dr.GetValue(0).ToString()))
        //                { return true; }
        //            }
        //        }

        //    }
        //    return false;
        //}
        #endregion

        //董海峰静态读取
        public static OracleDataReader ExecuteReader(string queryString)
        {
            try
            {
                //OracleDBHelper helper = new OracleDBHelper();
                //using( con )//连接数据库
                //{
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    OracleCommand Command = new OracleCommand(queryString, con);
                    
                        Command.CommandTimeout = 10000000;
                        OracleDataReader dr = Command.ExecuteReader();
                        Console.WriteLine("successfully Reader!");
                        //con.Close();
                        return dr;
                    
                    
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            //finally
            //{
            //    return dr;
            //}
            //try
            //{
            //    //if (con.State == ConnectionState.Closed)
            //    //{
            //    //    con.Open();
            //    //}
            //    //OracleCommand Command = new OracleCommand(queryString, con); ;
            //    //Command.CommandTimeout = 10000000;
            //    //using (OracleDataReader dr = queryReader(queryString))
            //    //{ 

            //    //}
            //    //OracleDataReader dr = Command.ExecuteReader();
            //    //Console.WriteLine("successfully Reader!");
            //    //return dr;

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    return null;
            //}
            //throw new NotImplementedException();
        }

       
        /// <summary>
        /// 执行查询语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public static OracleDataReader ExecuteReader(string SQLString, params OracleParameter[] cmdParms)
        {
            OracleDBHelper helper = new OracleDBHelper();
            OracleConnection connection = helper.getOracleConnection();//连接数据库
            //OracleConnection connection = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                OracleDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException e)
            {
                throw e;
            }

        }

        public static int GetMaxobj(string TableName)
        {
            string strsql = "select max(objectid) from " + TableName;
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }

        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            OracleDBHelper helper = new OracleDBHelper();
            using (OracleConnection connection = helper.getOracleConnection())
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (Oracle.ManagedDataAccess.Client.OracleException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, int Times)
        {
            OracleDBHelper helper = new OracleDBHelper();
            using (OracleConnection connection = helper.getOracleConnection())
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (Oracle.ManagedDataAccess.Client.OracleException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        public static int GetCount(string p, string condition)
        {
            throw new NotImplementedException();
        }

            #region 废弃
            //public OracleConnection getOracleConnection()
            //{
            //    if (con.State == ConnectionState.Closed)
            //    {
            //        con.Open();
            //    }
            //    return con;
            //}

            ///// <summary>
            ///// 给sql语句中的字符串格式化，避免字符串中出现单引号，反斜线
            ///// </summary>
            ///// <param name="oldString"></param>
            ///// <returns></returns>
            //public static string FormatString(string oldString)
            //{
            //    if (string.IsNullOrEmpty(oldString))
            //    {
            //        return null;
            //    }
            //    else
            //    {
            //        if (oldString.Contains('\''))
            //        {
            //            return oldString.Replace("'", "''");
            //        }
            //        else if (oldString.Contains('\\'))
            //        {
            //            return oldString.Replace("\\", "\\\\");
            //        }
            //        return oldString;
            //    }
            //}
            ///// <summary>
            ///// 用NpgsqlCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除等操作 无错返回操作的行数  有错返回错误原因
            ///// </summary>
            ///// <param name="changeString"></param>
            ///// <param name="conn"></param>
            //public int sqlExecute(string sql)
            //{
            //    try
            //    {

            //        if (con.State == ConnectionState.Closed)
            //        {
            //            con.Open();
            //        }

            //        OracleCommand command = new OracleCommand();
            //        command.CommandText = sql;
            //        command.CommandTimeout = 1000;
            //        int No = command.ExecuteNonQuery();
            //        return No;
            //    }
            //    catch (OracleException e)
            //    {
            //        Console.WriteLine("错误码：" + e.ErrorCode);
            //        if (e.ErrorCode.Equals("23505"))
            //        {
            //            Console.WriteLine("违反唯一性约束！");
            //        }
            //        else
            //        {

            //            Console.WriteLine(e.ToString());
            //        }
            //        return -1;
            //    }
            //    finally
            //    {
            //        con.Close();
            //    }

            //}
            ///// <summary>
            ///// 用NpgsqlCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除等操作 无错返回操作的行数  有错返回错误原因
            ///// </summary>
            ///// <param name="changeString"></param>
            ///// <param name="conn"></param>
            //public int sqlExecuteUntreatException(string sql)
            //{
            //    try
            //    {

            //        if (con.State == ConnectionState.Closed)
            //        {
            //            con.Open();
            //        }
            //        OracleCommand command = new OracleCommand(sql, con);
            //        command.CommandText = sql;
            //        command.CommandTimeout = 1000;
            //        int No = command.ExecuteNonQuery();
            //        return No;
            //    }
            //    catch (OracleException e)
            //    {
            //        throw new Exception(e.ToString());
            //    }
            //    finally
            //    {
            //        con.Close();
            //    }

            //}

            /// <summary>
            /// 用NpgsqlCommand.ExecuteNonQuery()方法对指定表进行添加、更新和删除等操作 无错返回操作的行数  有错返回错误原因
            /// </summary>
            /// <param name="changeString"></param>
            ///// <param name="conn"></param>
            //public int sqlExecuteUnClose(string sql)
            //{
            //    try
            //    {
            //        if (con.State == ConnectionState.Closed)
            //        {
            //            con.Open();
            //        }
            //        OracleCommand objCommand = new OracleCommand(sql, con);
            //        objCommand.CommandTimeout = 1000;
            //        int No = objCommand.ExecuteNonQuery();
            //        return No;
            //    }
            //    catch (OracleException e)
            //    {
            //        Console.WriteLine("错误码：" + e.ErrorCode);
            //        if (e.ErrorCode.Equals("23505"))
            //        {
            //            Console.WriteLine("违反唯一性约束！");
            //        }
            //        else
            //        {

            //            Console.WriteLine(e.ToString());
            //        }
            //        con.Close();
            //        return -1;
            //    }
            //    finally
            //    {

            //    }

            //}

            /// <summary>
            /// 读取所选的数据库内容
            /// </summary>
            /// <param name="queryString"></param>
            /// <param name="conn"></param>
            /// <returns></returns>
            //public OracleDataReader queryReader(string queryString)
            //{
            //    try
            //    {
            //        if (con.State == ConnectionState.Closed)
            //        {
            //            con.Open();
            //        }
            //        OracleCommand Command = new OracleCommand(queryString, con); ;
            //        Command.CommandTimeout = 10000000;
            //        OracleDataReader dr = Command.ExecuteReader();
            //        Console.WriteLine("successfully Reader!");
            //        return dr;

            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.Message);
            //        return null;
            //    }

            //}

            //public List<string> GetAllTableName() { return GetAllTableName(null); }
            /// <summary>
            /// 获取所有表名
            /// </summary>
            /// <param name="preString"></param>
            /// <returns></returns>
            //public List<string> GetAllTableName(string preString)
            //{
            //    List<string> tableNames = new List<string>();
            //    string condition = "";
            //    if (preString != null)
            //    {
            //        //condition = string.Format("and TABLE_NAME like '{0}%'", preString);
            //        condition = string.Format("T.TABLE_NAME like '{0}%'", preString);
            //    }
            //    string sql = string.Format("select t.table_name from user_table t where {0};", condition);
            //    // string sql = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'public' {0};", condition);
            //    using (OracleDataReader dr = queryReader(sql))
            //    {
            //        if (dr == null || !dr.HasRows)
            //        {
            //            return tableNames;
            //        }
            //        while (dr.Read())
            //        {
            //            tableNames.Add(dr["TABLE_NAME"].ToString());
            //        }
            //    }
            //    return tableNames;
            //}

            //public DataTable queryBySql(String sql, OracleConnection con)
            //{
            //    DataTable table = new DataTable();
            //    table.Columns.Add("userid");
            //    table.Columns.Add("username");
            //    try
            //    {
            //        using (OracleCommand command = con.CreateCommand())
            //        {
            //            command.CommandText = sql;
            //            using (OracleDataReader dr = command.ExecuteReader())
            //            {
            //                while (dr.Read())
            //                {
            //                    DataRow row = table.NewRow();
            //                    row["username"] = dr["USERNAME"].ToString();
            //                    table.Rows.Add(row);
            //                }
            //                return table;
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.ToString());
            //        return null;
            //    }

            //}


            /// <summary>
            /// 读取默认路径下\\Debug\\OsmSql\\存好的txt文档，行其中的sql代码，设计为创建、删除表格
            /// </summary>
            /// <param name="con"></param>
            /// <param name="fileName"></param>
            /// <returns></returns>
            //public static bool CreateTable(OracleConnection con, string fileName)
            //{
            //    string sqlCreateTbl = null;
            //    StreamReader myRead = new StreamReader(String.Format("OsmSql\\{0}.txt", fileName));
            //    while (!myRead.EndOfStream)
            //    {
            //        sqlCreateTbl += myRead.ReadLine();
            //    }
            //    myRead.Close();
            //    myRead.Dispose();
            //    try
            //    {
            //        using (OracleCommand cmd = new OracleCommand(sqlCreateTbl, con))
            //        {
            //            cmd.ExecuteNonQuery();
            //        }
            //        return true;
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.ToString());
            //        return false;
            //    }
            //}
            ///// <summary>
            ///// 读取默认路径下\\Debug\\OsmSql\\存好的txt文档，行其中的sql代码，设计为创建、删除表格
            ///// </summary>
            ///// <param name="con"></param>
            ///// <param name="fileName"></param>
            ///// <returns></returns>
            //public bool CreateTable(string fileName)
            //{
            //    if (con.State != ConnectionState.Open)
            //    {
            //        con.Open();
            //    }
            //    string sqlCreateTbl = null;
            //    StreamReader myRead = new StreamReader(String.Format("OsmSql\\{0}.txt", fileName));
            //    while (!myRead.EndOfStream)
            //    {
            //        sqlCreateTbl += myRead.ReadLine();
            //    }
            //    myRead.Close();
            //    myRead.Dispose();
            //    try
            //    {
            //        using (OracleCommand cmd = new OracleCommand(sqlCreateTbl, con))
            //        {
            //            cmd.ExecuteNonQuery();
            //        }
            //        return true;
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.ToString());
            //        return false;
            //    }
            //}

            /// <summary>
            /// 针对点，线，面的国标多个图层建表法，即指定点线面表格要素与表名操作
            /// </summary>
            /// <param name="con"></param>
            /// <param name="filePath"></param>
            /// <param name="tblname"></param>
            /// <returns></returns>
            //public static int CreateTable(OracleConnection con, string filePath, string tblname)
            //{
            //    int sum = -100;
            //    string sqlCreateTbl = null;
            //    StreamReader myRead = new StreamReader(filePath);
            //    while (!myRead.EndOfStream)
            //    {
            //        sqlCreateTbl += myRead.ReadLine();
            //    }
            //    myRead.Close();
            //    myRead.Dispose();
            //    sqlCreateTbl = String.Format("Create Table {0}", tblname) + sqlCreateTbl;
            //    try
            //    {
            //        using (OracleCommand cmd = new OracleCommand(sqlCreateTbl, con))
            //        {
            //            sum = cmd.ExecuteNonQuery();
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.ToString());
            //    }
            //    return sum;
            //}

            /// <summary>
            /// 建表
            /// </summary>
            /// <param name="tableName"></param>
            /// <param name="createTableSql"></param>
            /// <returns></returns>
            //public Boolean createTable(string tableName, string createTableSql)
            //{
            //    try
            //    {
            //        string sql = "select count(*) from USER_TABLES where Table_Name=upper('" + tableName + "')";
            //        if (con.State == ConnectionState.Closed)
            //        {
            //            con.Open();
            //        }
            //        using (OracleCommand command = new OracleCommand(sql, con))
            //        {
            //            using (OracleDataReader dr = command.ExecuteReader())
            //            {
            //                int count = 0;
            //                while (dr.Read())
            //                {
            //                    count += Int32.Parse(dr["count(*)"].ToString());
            //                }
            //                if (count > 0)
            //                {
            //                    sql = "drop table" + tableName;
            //                    command.CommandText = sql;
            //                    command.ExecuteNonQuery();
            //                }
            //                sql = "CREATE TABLE" + tableName + createTableSql;
            //                command.CommandText = sql;
            //                command.ExecuteNonQuery();
            //            }

            //        }
            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.ToString());
            //        return false;
            //    }
            //    finally { con.Close(); }
            //}
            /// <summary>
            /// 删除表
            /// </summary>
            /// <param name="tableName"></param>
            //public void DropTable(string tableName)
            //{
            //    string sql = string.Format("drop table {0};", tableName);
            //    sqlExecute(sql);
            //}
            /// <summary>
            /// 判断数据库是否存在tablename表
            /// </summary>
            /// <param name="tableName"></param>
            /// <returns></returns>
            //public bool IsExistTable(string tableName)
            //{
            //    if (con.State == ConnectionState.Closed)
            //    {
            //        con.Open();
            //    }
            //    string sql = string.Format("select * from user_tab_comments where Table_Name='{0}'", tableName);
            //    using (OracleCommand cmd = new OracleCommand(sql, con))
            //    {
            //        using (OracleDataReader dr = cmd.ExecuteReader())
            //        {
            //            if (dr != null && dr.HasRows)
            //            {
            //                dr.Read();
            //                if ("1".Equals(dr.GetValue(0).ToString()))
            //                { return true; }
            //            }
            //        }

            //    }
            //    return false;
            //}
            #endregion

        #region zh 数据库的公用接口
        //为shape字段创建索引
        public void createIndex(string tableName)
        {

            string sql1 = "";
            string sqlInset = "";
            string sqlCreate = "";

            if (IsExistSpatialTable(tableName))
            {
                sqlCreate = string.Format("create index idx_{0} on {0}(shape) indextype is mdsys.spatial_index", tableName);
            }
            else
            {
                sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('{0}', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", tableName);
                sqlExecuteUnClose(sqlInset);
                sqlCreate = string.Format("create index idx_{0} on {0}(shape) indextype is mdsys.spatial_index", tableName);

            }

            sqlExecuteUnClose(sqlCreate);
        }

        //设置表的key字段为自增字段
        public void CreateSequenceSelfIncrease(string tableName,string key)
        {
            string sql1 = string.Format("create sequence SEQ_{0} start with 1 increment by 1 nomaxvalue nominvalue nocache", tableName);
            string sql = "select count(*)  from user_sequences where SEQUENCE_NAME =upper('SEQ_" + tableName + "')";
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            using (OracleCommand conmand = new OracleCommand(sql, con))
            {
                using (OracleDataReader rd = conmand.ExecuteReader())
                {
                    int count = 0;
                    while (rd.Read())
                    {
                        count += Int32.Parse(rd["count(*)"].ToString());
                    }
                    if (count > 0)
                    {
                        sql = "drop SEQUENCE SEQ_" + tableName;
                        conmand.CommandText = sql;
                        conmand.ExecuteNonQuery();
                    }
                }
            }
            sqlExecute(sql1);
            string sql2 = "CREATE OR REPLACE TRIGGER tg_" + tableName + " before INSERT ON " + tableName + " FOR EACH ROW WHEN (new."+key+ " is null) begin select SEQ_" + tableName + ".nextval into:new."+key +" from dual; end;";
            sqlExecute(sql2);

        }

        //根据表名和sql语句创建单个表
        public void createTable1(string tableName, string createTableSql)
        {
            try
            {
                string sql = "select count(*)  from USER_TABLES where Table_Name = upper('" + tableName + "')";
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                using (OracleCommand conmand = new OracleCommand(sql, con))
                {
                    using (OracleDataReader rd = conmand.ExecuteReader())
                    {
                        int count = 0;
                        while (rd.Read())
                        {
                            count += Int32.Parse(rd["count(*)"].ToString());
                        }
                        if (count > 0)
                        {
                            sql = "drop table " + tableName;
                            conmand.CommandText = sql;
                            conmand.ExecuteNonQuery();
                        }

                        //createTableSql = "(objectId NUMBER(20) NOT NULL,osmid NUMBER(20) NOT NULL,version NUMBER(4),timestamp TIMESTAMP(6),changeset VARCHAR2(500),userid NUMBER(20),username VARCHAR2(20),tags VARCHAR2(1000),trustvalue FLOAT,pointsId VARCHAR2(1000),shape SDO_GEOMETRY,polytype NUMBER(4))";
                        sql = "CREATE TABLE " + tableName + createTableSql;
                        conmand.CommandText = sql;
                        conmand.ExecuteNonQuery();

                        if (tableName == "daul")
                        {
                            sql = "insert into " + tableName + " values(123456)";
                            conmand.CommandText = sql;
                            conmand.ExecuteNonQuery();
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                con.Close();
            }

        }

        //批量删除数据，idList为主键key集合
        public void deleteDataByList(string tableName,string key, ArrayList idList)
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                for (int i = 0; i < idList.Count; i++)
                {
                    string sql1 = string.Format("delete from {0} where {1}={2}", tableName, key, (string)idList[i]);
                    using (OracleCommand cmd = new OracleCommand(sql1, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + "");
                Console.WriteLine("华丽的分割线*************************************华丽的分割线");
                Console.WriteLine(ex.ToString());
            }

        }

        #region 基态
        // 基态shp线面数据批量入库
        public void insertPolyDataByList(string tableName, List<Poly> polyList)
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            string osmid = "";
            string sql = "";
            try
            {
                for (int i = 0; i < polyList.Count; i++)
                {

                    Poly poly = polyList[i];
                    StringBuilder strSql = new StringBuilder();
                    string sql1 = string.Format("insert into {0} (osmid,versionid,starttime,changeset,userid,username,fc,dsg,tags,trustvalue,shape,source,nationelename,nationcode) values", tableName);
                    strSql.Append(sql1);
                    poly.setSource(1);
                    //string sql2 = string.Format("({0},{1},'{2}','{3}',{4},'{5}','{6}','{7}','{8}',{9},'{10}',{11},sdo_geometry ('{12}',31297 ))",poly.getOsmid(), poly.getVersion(), poly.getTimestamp(), poly.getChangeset(), poly.getUserid(), "",poly.getFc(),poly.getDsg(), tags, poly.getTrustvalue(), poly.getPointsId(), poly.getPolytype(), poly.getWkt());
                    string sql2 = string.Format("({0},{1},'{2}','{3}',{4},'{5}','{6}','{7}','{8}',{9},{10},{11},'{12}','{13}')", poly.getOsmid(), poly.getVersion(), poly.getStartTime(), poly.getChangeset(), poly.getUserid(), poly.getUsername(), poly.getFc(), poly.getDsg(), poly.getTags(), poly.getTrustvalue(), "sdo_geometry ( :geom,4326)", poly.getSource(), poly.getNationelename(), poly.getNationcode());

                    strSql.Append(sql2);
                    //Console.WriteLine("strSql=" + strSql.ToString());
                    sql = strSql.ToString();
                    //sql.TrimEnd(',');
                    using (OracleCommand cmd = new OracleCommand(strSql.ToString(), con))
                    {
                        using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                        {
                            p1.Value = poly.getWkt();
                            cmd.Parameters.Add(p1);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + "");
                Console.WriteLine("华丽的分割线*************************************华丽的分割线");
                Console.WriteLine("osmid= " + osmid);
                Console.WriteLine(ex.ToString());
                //return null;
            }
        }

        //基态点数据批量入库
        public void insertPointDataByList(string tableName, List<Ppoint> pointList)
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            List<Ppoint> list = pointList;
            try
            {
                string sql = "";
                //string sql1 = string.Format("insert all ");
                //StringBuilder strSql = new StringBuilder();
                //strSql.Append(sql1);
                for (int i = 0; i < list.Count(); i++)
                {
                    Ppoint point = list[i];
                    string dsg = point.getDsg();
                    dsg = FormatString(dsg);
                    string tags = "";
                    //if (point.issimple == false)
                    //{
                    //    tags = point.getTags();
                    //    if (point.getTags().Length > 500)
                    //    {
                    //        tags = tags.Substring(0, 500);
                    //    }
                    //}
                    //else { tags = null; }
                    //StringBuilder strSql = new StringBuilder();
                    StringBuilder strSql = new StringBuilder();
                    string sql2 = string.Format("insert into {0}(osmid,lat,lon,versionid,starttime,changeset,userid,username,fc,dsg,tags,shape,source,nationelename,nationcode) values", tableName);
                    strSql.Append(sql2);
                    point.setSource(1);
                    //string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11})", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getTimeStamp(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags,"sdo_geometry (:geom,31297)");
                    string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11},{12},'{13}','{14}')", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getStartTime(), point.getChangeset(), point.getUserid(), point.getUsername(), point.getFc(), point.getDsg(), point.getTags(), "sdo_geometry ( :geom,4326)", point.getSource(), point.getNationelename(), point.getNationcode());
                    strSql.Append(sql3);
                    sql = strSql.ToString();
                    using (OracleCommand cmd = new OracleCommand(strSql.ToString(), con))
                    {
                        using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                        {
                            p1.Value = point.getWkt();
                            cmd.Parameters.Add(p1);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                //string sql4 = string.Format("select 1 from DAUL");
                //strSql.Append(sql4);
                //sql = strSql.ToString();
                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        //创建基态线面数据表
        public void createPolyTable(string tableName)
        {
            //建表polygon
            string createTableSql1 = "(objectid NUMBER(30) primary key NOT NULL,osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT ,shape SDO_GEOMETRY,nationelename VARCHAR2(30),nationcode VARCHAR2(30),source NUMBER(10))";
            createTable(tableName, createTableSql1);
            CreateSequenceSelfIncrease(tableName);//设置主键自加
            createIndex(tableName);//为表添加索引

            //tableName = "daul";
            //string createTablesql4 = "(osmid NUMBER(30) NOT NULL)";
            //createTable(tableName, createTablesql4);//辅助表
        }

        //创建基态点数据表
        public void createPointTable(string tableName)
        {
            //建表point
            //string createTableSql3 = "(objectId NUMBER(30) primary key not null,osmid NUMBER(30) NOT NULL,lat FLOAT,lon FLOAT,version NUMBER(10),timestamp VARCHAR2(30),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),shape SDO_GEOMETRY)";
            string createTableSql3 = "(objectid NUMBER(30) primary key NOT NULL,osmid NUMBER(30),lat FLOAT,lon FLOAT,versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),shape SDO_GEOMETRY,nationelename VARCHAR2(30),nationcode VARCHAR2(30),source NUMBER(10),userreputation FLOAT)";
            createTable(tableName, createTableSql3);
            CreateSequenceSelfIncrease(tableName);//设置主键自加
        }
        #endregion

        #region 增量
        // 增量shp线面数据批量入库
        public void insertZPolyDataByList(string tableName, List<Poly> polyList)
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            string osmid = "";
            string sql = "";
            try
            {
                for (int i = 0; i < polyList.Count; i++)
                {

                    Poly poly = polyList[i];
                    StringBuilder strSql = new StringBuilder();
                    string sql1 = string.Format("insert into {0} (osmid,versionid,starttime,changeset,userid,username,fc,dsg,tags,trustvalue,pointsId,shape,source,nationelename,nationcode,changetype) values", tableName);
                    strSql.Append(sql1);
                    poly.setSource(1);
                    //string sql2 = string.Format("({0},{1},'{2}','{3}',{4},'{5}','{6}','{7}','{8}',{9},'{10}',{11},sdo_geometry ('{12}',31297 ))",poly.getOsmid(), poly.getVersion(), poly.getTimestamp(), poly.getChangeset(), poly.getUserid(), "",poly.getFc(),poly.getDsg(), tags, poly.getTrustvalue(), poly.getPointsId(), poly.getPolytype(), poly.getWkt());
                    string sql2 = string.Format("({0},{1},'{2}','{3}',{4},'{5}','{6}','{7}','{8}',{9},'{10}',{11},{12},'{13}','{14}','{15}')", poly.getOsmid(), poly.getVersion(), poly.getStartTime(), poly.getChangeset(), poly.getUserid(), poly.getUsername(), poly.getFc(), poly.getDsg(), poly.getTags(), poly.getTrustvalue(), poly.getPointsId(), "sdo_geometry ( :geom,4326)", poly.getSource(),poly.getNationelename(),poly.getNationcode(),poly.getChangetype());

                    strSql.Append(sql2);
                    //Console.WriteLine("strSql=" + strSql.ToString());
                    sql = strSql.ToString();
                    //sql.TrimEnd(',');
                    using (OracleCommand cmd = new OracleCommand(strSql.ToString(), con))
                    {
                        using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                        {
                            p1.Value = poly.getWkt();
                            cmd.Parameters.Add(p1);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + "");
                Console.WriteLine("华丽的分割线*************************************华丽的分割线");
                Console.WriteLine("osmid= " + osmid);
                Console.WriteLine(ex.ToString());
                //return null;
            }
        }

        //增量点数据批量入库
        public void insertZPointDataByList(string tableName, List<Ppoint> pointList)
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            List<Ppoint> list = pointList;
            try
            {
                string sql = "";
                //string sql1 = string.Format("insert all ");
                //StringBuilder strSql = new StringBuilder();
                //strSql.Append(sql1);
                for (int i = 0; i < list.Count(); i++)
                {
                    Ppoint point = list[i];
                    string dsg = point.getDsg();
                    dsg = FormatString(dsg);
                    string tags = "";
                    //if (point.issimple == false)
                    //{
                    //    tags = point.getTags();
                    //    if (point.getTags().Length > 500)
                    //    {
                    //        tags = tags.Substring(0, 500);
                    //    }
                    //}
                    //else { tags = null; }
                    //StringBuilder strSql = new StringBuilder();
                    StringBuilder strSql = new StringBuilder();
                    string sql2 = string.Format("insert into {0}(osmid,lat,lon,versionid,starttime,changeset,userid,username,fc,dsg,tags,shape,source,nationelename,nationcode,changetype) values", tableName);
                    strSql.Append(sql2);
                    point.setSource(1);
                    //string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11})", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getTimeStamp(), point.getChangeset(), point.getUserid(), "", point.getFc(), point.getDsg(), tags,"sdo_geometry (:geom,31297)");
                    string sql3 = string.Format("({0},{1},{2},{3},'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11},{12},'{13}','{14}','{15}')", point.getOsmid(), point.getLat(), point.getLon(), point.getVersion(), point.getStartTime(), point.getChangeset(), point.getUserid(), point.getUsername(), point.getFc(), point.getDsg(), point.getTags(), "sdo_geometry ( :geom,4326)", point.getSource(), point.getNationelename(), point.getNationcode(), point.getChangetype());
                    strSql.Append(sql3);
                    sql = strSql.ToString();
                    using (OracleCommand cmd = new OracleCommand(strSql.ToString(), con))
                    {
                        using (OracleParameter p1 = new OracleParameter(":geom", OracleDbType.Clob))
                        {
                            p1.Value = point.getWkt();
                            cmd.Parameters.Add(p1);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                //string sql4 = string.Format("select 1 from DAUL");
                //strSql.Append(sql4);
                //sql = strSql.ToString();


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }


        //创建增量线面数据表
        public void createZPolyTable(string tableName)
        {
            //建表polygon
            string createTableSql1 = "(objectid NUMBER(30) primary key NOT NULL,osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,pointsId VARCHAR2(4000),shape SDO_GEOMETRY,nationelename VARCHAR2(30),nationcode VARCHAR2(30),source NUMBER(10),changetype VARCHAR2(30))";
            createTable(tableName, createTableSql1);
            CreateSequenceSelfIncrease(tableName);//设置主键自加

            //tableName = "daul";
            //string createTablesql4 = "(osmid NUMBER(30) NOT NULL)";
            //createTable(tableName, createTablesql4);//辅助表
        }

        //创建增量点数据表
        public void createZPointTable(string tableName)
        {
            //建表point
            //string createTableSql3 = "(objectId NUMBER(30) primary key not null,osmid NUMBER(30) NOT NULL,lat FLOAT,lon FLOAT,version NUMBER(10),timestamp VARCHAR2(30),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),shape SDO_GEOMETRY)";
            string createTableSql3 = "(objectid NUMBER(30) primary key NOT NULL,osmid NUMBER(30),lat FLOAT,lon FLOAT,versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),shape SDO_GEOMETRY,nationelename VARCHAR2(30),nationcode VARCHAR2(30),source NUMBER(10),changetype VARCHAR2(30),userreputation FLOAT)";
            createTable(tableName, createTableSql3);
            CreateSequenceSelfIncrease(tableName);//设置主键自加
        }
        #endregion
        #endregion


        //dhf创建触发器
        public  void createTriger(string tableName )
        {
            string sql1 = "select count(*) from " + tableName;
            int count1=1;
            using (OracleDataReader dr = queryReader(sql1))
            {
                while (dr.Read())
                {
                    count1 = Convert.ToInt32(dr["count(*)"])+1;
                }
            }
           
            sql1 = string.Format("create sequence {0}_seq increment by 1 start with " + count1 + " nomaxvalue nocycle cache 10", tableName);
            
            string sql = "select count(*)  from USER_TRIGGERS where TRIGGER_NAME =upper('" + tableName + "_trigger')";
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            using (OracleCommand conmand = new OracleCommand(sql, con))
            {
                using (OracleDataReader rd = conmand.ExecuteReader())
                {
                    int count = 0;
                    while (rd.Read())
                    {
                        count += Int32.Parse(rd["count(*)"].ToString());
                    }
                    if (count > 0)
                    {
                        sql = "drop sequence " + tableName + "_seq";
                        conmand.CommandText = sql;
                        conmand.ExecuteNonQuery();
                        sql = "drop TRIGGER " + tableName + "_trigger";
                        conmand.CommandText = sql;
                        conmand.ExecuteNonQuery();

                    }
                }
            }
            sqlExecute(sql1);
            string sql2 = "CREATE OR REPLACE TRIGGER " + tableName + "_trigger  before INSERT ON " + tableName + " FOR EACH ROW declare nextid number;begin if :new.OBJECTID is null or :new.OBJECTID=0 then select " + tableName + "_seq.nextval into:new.OBJECTID  from dual;  end if;end " + tableName + "_trigger; ";
            sqlExecute(sql2);

        
        }
        
        /// <summary>
        /// 匹配成功则给matchid赋值
        /// </summary>
        public void setMatchId( string osmTable,string oscTable,int matchId)
        {
            string sql = "UPDATE  " + osmTable + " SET  MATCHID = " + matchId + "  WHERE 条件语句未知";
            ExecuteSql(sql);
            sql = "UPDATE  " + oscTable + " SET  MATCHID = " + matchId + "  WHERE 条件语句未知";
            ExecuteSql(sql);
        }


    }
    }

