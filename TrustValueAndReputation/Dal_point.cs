using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using GIS.UI.AdditionalTool;


namespace TrustValueAndReputation
{
    public class Dal_point
    {
        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(point model)
        {
            model.objectid = GetMaxId() + 1;
            StringBuilder strSql = new StringBuilder();
            strSql.Append("INSERT INTO point(");
            strSql.Append("objectid，osmid,versionid,userid,username,lon,lat,changeset,visible,tags,starttime)");
            strSql.Append(" VALUES (");
            strSql.Append("@in_id,@in_version,@in_userid,@in_username,@in_lon,@in_lat,@in_changeset,@in_visible,@in_tags,@in_timestamp)");
            OracleParameter[] cmdParms = {
                new OracleParameter("@in_objectid", OracleDbType.Int64),
				new OracleParameter("@in_id", OracleDbType.Int64),
				new OracleParameter("@in_version", OracleDbType.Int16),
				new OracleParameter("@in_userid",OracleDbType.Int64),
				new OracleParameter("@in_username", OracleDbType.Varchar2),
				new OracleParameter("@in_lon", OracleDbType.Varchar2),
				new OracleParameter("@in_lat", OracleDbType.Varchar2),
				new OracleParameter("@in_changeset", OracleDbType.Varchar2),
				new OracleParameter("@in_tags", OracleDbType.Varchar2),
				new OracleParameter("@in_timestamp", OracleDbType.TimeStampTZ)};
            cmdParms[0].Value = model.objectid;
            cmdParms[1].Value = model.id;
            cmdParms[2].Value = model.version;
            cmdParms[3].Value = model.userid;
            cmdParms[4].Value = model.username;
            cmdParms[5].Value = model.lon;
            cmdParms[6].Value = model.lat;
            cmdParms[7].Value = model.changeset;
            cmdParms[8].Value = model.tags;
            cmdParms[9].Value = model.timestamp;

            return OracleDBHelper.ExecuteSql(strSql.ToString(), cmdParms);
        }
        /// <summary>
        /// 更新一条数据
        /// </summary>
        public int Update(point model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE point SET ");
            strSql.Append("objectid=@in_objectid,");
            strSql.Append("osmid=@in_id,");
            strSql.Append("versionid=@in_version,");
            strSql.Append("userid=@in_userid,");
            strSql.Append("username=@in_username,");
            strSql.Append("lon=@in_lon,");
            strSql.Append("lat=@in_lat,");
            strSql.Append("changeset=@in_changeset,");
            strSql.Append("visible=@in_visible,");
            strSql.Append("tags=@in_tags,");
            strSql.Append("timestamps=@in_timestamp,");
            strSql.Append("updateTime=@in_updateTime");
            strSql.Append(" WHERE objectid=@in_objectid");
            OracleParameter[] cmdParms = {
				new OracleParameter("@in_objectid", OracleDbType.Int64),
				new OracleParameter("@in_id", OracleDbType.Int64),
				new OracleParameter("@in_version", OracleDbType.Int16),
				new OracleParameter("@in_userid",OracleDbType.Int64),
				new OracleParameter("@in_username", OracleDbType.Varchar2),
				new OracleParameter("@in_lon", OracleDbType.Varchar2),
				new OracleParameter("@in_lat", OracleDbType.Varchar2),
				new OracleParameter("@in_changeset", OracleDbType.Varchar2),
				new OracleParameter("@in_tags", OracleDbType.Varchar2),
				new OracleParameter("@in_timestamp", OracleDbType.TimeStampTZ)};
            cmdParms[0].Value = model.objectid;
            cmdParms[1].Value = model.id;
            cmdParms[2].Value = model.version;
            cmdParms[3].Value = model.userid;
            cmdParms[4].Value = model.username;
            cmdParms[5].Value = model.lon;
            cmdParms[6].Value = model.lat;
            cmdParms[7].Value = model.changeset;
            cmdParms[8].Value = model.tags;
            cmdParms[9].Value = model.timestamp;

            return OracleDBHelper.ExecuteSql(strSql.ToString(), cmdParms);
        }
        /// <summary>
        /// 删除一条数据
        /// </summary>
        public int Delete(long objectid)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("DELETE FROM point ");
            strSql.Append(" WHERE objectid=@in_objectid");
            OracleParameter[] cmdParms = {
				new OracleParameter("@in_objectid", OracleDbType.Int64)};
            cmdParms[0].Value = objectid;
            return OracleDBHelper.ExecuteSql(strSql.ToString(), cmdParms);
        }

        /// <summary>
        /// 得到最大ID
        /// </summary>
        public long GetMaxId()
        {
            return OracleDBHelper.GetMaxID("objectid", "point");
        }


        ///// <summary>
        ///// 是否存在该记录
        ///// </summary>
        //public bool Exists(long objectid)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("SELECT COUNT(1) FROM point");
        //    strSql.Append(" WHERE objectid=@in_objectid");
        //    DbParameter[] cmdParms = {
        //        laowoHelper.CreateInDbParameter("@in_objectid", DbType.Int64, objectid)};

        //    object obj = laowoHelper.ExecuteScalar(CommandType.Text, strSql.ToString(), cmdParms);
        //    return laowoHelper.GetInt(obj) > 0;
        //}

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public point GetModel(long id,long userid,DateTime time)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,starttime,issimple,fc,dsg, tags, sdo_geometry.get_wkt(shape) FROM point ");
            strSql.Append(" WHERE osmid='" + id + "' and userid='" + userid + "' and starttime<='" + time + "' order by nodetime desc limit 1");
            //strSql.Append(" WHERE nodeId=" + nodeId + " and nodetime<='" + time + "' order by nodetime desc");
            point model = null;
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                if (dr.Read())
                {
                    model = GetModel(dr);

                }
                else
                {
                    model = GetModel(id, time);  //还可以编辑别人的点，好吧，忽略用户id
                }
                return model;
            }
        }

        /// <summary>
        /// 得到一个对象实体,忽略用户
        /// </summary>
        /// <param name="objectid"></param>
        /// <param name="userid"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public point GetModel(long id, DateTime time)
        {
            //OracleDBHelper hepler = new OracleDBHelper();
            //StringBuilder strSql = new StringBuilder();
            //strSql.Append("SELECT OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,timestamps,issimple,fc,dsg, tags, sdo_geometry.get_wkt(geom) FROM point ");
            ////strSql.Append(" WHERE objectid=" +objectid + " and userId=" + userid + " and timestamp<='" + time + "' order by nodetime desc");
            //strSql.Append(" WHERE osmid='" + id + "' and timestamps<='" + time.ToString() + "'  and rownum=1 order by timestamps desc");
            string strSql = String.Format("select OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,starttime,issimple,fc,dsg, tags  from (select OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,starttime,issimple,fc,dsg, tags, sdo_geometry.get_wkt(shape) from point where osmid={0} and starttime<='{1}' order by starttime desc) where rownum=1", id, time.ToString());
            point model = null;
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                if (dr.Read())
                {
                    model = GetModel(dr);
                }
                return model;
            }
        }
        /// <summary>
        /// 根据面的ID串获取时间最晚的点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public point GetModel(string ids)
        {
            //StringBuilder strSql = new StringBuilder();
            //strSql.Append("SELECT OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,timestamps,issimple,fc,dsg, tags, sdo_geometry.get_wkt(geom) FROM point ");
            ////strSql.Append(" WHERE objectid=" +objectid + " and userId=" + userid + " and timestamp<='" + time + "' order by nodetime desc");
            //strSql.Append(" WHERE osmid IN(" + ids + ") order by timestamps desc rownum=1");
            string strSql = String.Format("select OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,starttime,issimple,fc,dsg, tags  from (select OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,starttime,issimple,fc,dsg, tags, sdo_geometry.get_wkt(shape) from point WHERE osmid IN ({0}) order by starttime desc) where rownum=1", ids);
            point model = null;
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                if (dr.Read())
                {
                    model = GetModel(dr);
                }
                return model;
            }
        }
        ///// <summary>
        ///// 得到一个实体对象，忽略时间
        ///// </summary>
        ///// <param name="nodeId"></param>
        ///// <param name="userid"></param>
        ///// <param name="time"></param>
        ///// <returns></returns>
        //public osmHistoryNode1 GetModelTime(long nodeId, DateTime time)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("SELECT top 1 * FROM osmHistoryNode1 ");
        //    //strSql.Append(" WHERE nodeId=" + nodeId + " and userId=" + userid + " and nodetime<='" + time + "' order by nodetime desc");
        //    strSql.Append(" WHERE nodeId=" + nodeId + " and nodetime>='" + time + "' order by nodetime asc");
        //    osmHistoryNode1 model = null;
        //    using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
        //    {
        //        while (dr.Read())
        //        {
        //            model = GetModel(dr);
        //        }
        //        return model;
        //    }
        //}


        private point GetModel(object id, string time)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取泛型数据列表
        /// </summary>
        public List<point> GetList()
        {
            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,starttime,issimple,fc,dsg, tags, sdo_geometry.get_wkt(shape) FROM point");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<point> lst = GetList(dr);
                return lst;
            }
        }
        /// <summary>
        /// 获取id
        /// </summary>
        public List<long> GetIDList()
        {
            StringBuilder strSql = new StringBuilder("SELECT distinct osmid FROM point");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<long> lst = GetListid(dr);
                return lst;
            }
        }
        //public static String getSqlStrByArrays(String sqhArrays, String splitStr, int splitNum, String columnName)
        //{
        //    if (Tools.isNullStr(sqhArrays))
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        String[] arrStr = sqhArrays.split(splitStr);
        //        return getSqlStrByList(Arrays.asList(arrStr), splitNum, columnName);
        //    }
        //}
        //public static String getSqlStrByList(List sqhList, string whereClause, String columnName)
        //{
        //    string[] arrys = whereClause.Split(',');
        //     int splitNum = arrys.Length;
        //    if (splitNum > 1000) //因为数据库的列表sql限制，不能超过1000.  
        //        return null;
        //    StringBuilder sql = new StringBuilder("");
        //    if (sqhList != null)
        //    {
        //        sql.Append(" ").Append(columnName).Append(" IN ( ");
        //        for (int i = 0; i < sqhList.size(); i++)
        //        {
        //            sql.Append("'").Append(sqhList.get(i) + "',");
        //            if ((i + 1) % splitNum == 0 && (i + 1) < sqhList.size())
        //            {
        //                sql.deleteCharAt(sql.length() - 1);
        //                sql.Append(" ) OR ").Append(columnName).Append(" IN (");
        //            }
        //        }
        //        sql.deleteCharAt(sql.length() - 1);
        //        sql.Append(" )");
        //    }
        //    return sql.ToString();
        //}
        public string getwhereclause(string whereClause)
        {
            //try
            //{
            //char[] split = { ',' };
            String[] arrys = whereClause.Split(',');
            int length = whereClause.Split(',').Length;
            StringBuilder sql = new StringBuilder();
            //string sqlclase = null;
            if (length > 1000)//当IN("里面的数大于1000时分别处理")
            {
                //int j = length / 1000;
                for (int i = 0; i < length; i++)
                {
                    sql.Append(arrys[i]).Append(",");

                    if ((i + 1) % 1000 == 0 && (i + 1) < length)
                    {
                        sql.Remove(sql.Length - 1, 1);
                        //sql.Remove(length%1000,);
                        sql.Append(") OR ").Append("osmid").Append(" IN (");
                    }
                    //sqlclase +=(sql[i] + ") or in (");
                }
                //sql.Append(" )");
                sql.Remove(sql.Length - 1, 1);
                return sql.ToString();

            }
            else
            {
                return whereClause;
            }

            //}
            //catch(Exception e){
            //    Console.WriteLine(e);
            //}

        }
        /// <summary>
        /// 获取泛型数据列表
        /// </summary>
        public List<point> GetListByCond(string whereClause)
        {
            string clause = getwhereclause(whereClause);

            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,starttime,issimple,fc,dsg,tags,sdo_geometry.get_wkt(shape) FROM point WHERE osmid in(" + clause + ") order by starttime asc");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<point> lst = GetList(dr);
                return lst;
            }
        }

        /// <summary>
        /// 获取泛型数据列表
        /// </summary>
        public List<point> GetListByRange(string whereClause, DateTime endTime)
        {
            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,starttime,issimple,fc,dsg, tags, sdo_geometry.get_wkt(shape) FROM point WHERE " + whereClause + "  AND starttime<='" + endTime + "'order by starttime desc ");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<point> lst = GetList(dr);
                return lst;
            }
        }


        /// <summary>
        /// 获取泛型数据列表,列表通过nodeId及nodetime排序
        /// </summary>
        public List<point> GetListSortByidandTime(string range, DateTime startTime, DateTime endTime)
        {
            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,username,userid,versionid,lat,lon,visible,changeset,starttime,issimple,fc,dsg, tags, sdo_geometry.get_wkt(shape) FROM point");
            strSql.Append(" WHERE osmid IN(" + range + ")");
            strSql.Append(" AND starttime>'" + startTime + "' AND starttime<'" + endTime + "'");
            strSql.Append(" ORDER BY osmid,starttime");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<point> lst = GetList(dr);
                return lst;
            }
        }

        /// <summary>
        /// 得到数据条数
        /// </summary>
        public int GetCount(string condition)
        {
            return OracleDBHelper.GetCount("point", condition);
        }

        ///// <summary>
        ///// 分页获取泛型数据列表
        ///// </summary>
        //public List<Model.point> GetPageList(int pageSize, int pageIndex, string fldSort, bool sort, string condition)
        //{
        //    using (OracleDataReader dr = laowoHelper.GetPageList("point", pageSize, pageIndex, fldSort, sort, condition))
        //    {
        //        List<Model.point> lst = GetList(dr);
        //        return lst;
        //    }
        //}


        #region -------- 私有方法，通常情况下无需修改 --------

        /// <summary>
        /// 由一行数据得到一个实体
        /// </summary>
        private point GetModel(OracleDataReader dr)
        {
            point model = new point();
            model.objectid = Convert.ToInt64(dr["objectid"]);
            model.id = Convert.ToInt64(dr["osmid"]);
            model.version = Convert.ToInt32(dr["versionid"]);
            model.userid = Convert.ToInt32(dr["userid"]);
            model.username = Convert.ToString(dr["username"]);
            model.lon = Convert.ToDouble(dr["lon"]);
            model.lat = Convert.ToDouble(dr["lat"]);
            if (dr["changeset"] == DBNull.Value)
            {
                model.changeset = -1;
            }
            else
            {
                model.changeset = Convert.ToInt64(dr["changeset"]);
            }
            model.tags = Convert.ToString(dr["tags"]);
            model.timestamp = Convert.ToDateTime(dr["starttime"]);
            //model.updateTime = Convert.ToDateTime(dr["updateTime"]);
            return model;
        }

        /// <summary>
        /// 由OracleDataReader得到泛型数据列表
        /// </summary>
        private List<point> GetList(OracleDataReader dr)
        {
            List<point> lst = new List<point>();
            while (dr.Read())
            {
                lst.Add(GetModel(dr));
            }
            return lst;
        }
        /// <summary>
        /// 获得ID
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private List<long> GetListid(OracleDataReader dr)
        {
            List<long> lst = new List<long>();
            while (dr.Read())
            {
                lst.Add(Convert.ToInt64(dr["osmid"]));
            }
            return lst;
        }
        #endregion


    }
}

    


