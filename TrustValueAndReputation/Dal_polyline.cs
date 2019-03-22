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
    public class Dal_polyline
    {
        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(polyline model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("INSERT INTO polyline(");
            strSql.Append("osmid,versionid,userid,username,changeset,starttime,tags,pointsid,points)");
            strSql.Append(" VALUES (");
            strSql.Append("@in_id,@in_version,@in_userid,@in_username,@in_changeset,@in_timestamp,@in_tags,@in_pointids,@in_points)");
            OracleParameter[] cmdParms = {
				new OracleParameter("@in_id", OracleDbType.Varchar2),
				new OracleParameter("@in_version", OracleDbType.Int16),
				new OracleParameter("@in_userid",OracleDbType.Varchar2),
				new OracleParameter("@in_username", OracleDbType.Varchar2),
				new OracleParameter("@in_changeset",  OracleDbType.Varchar2),
                new OracleParameter("@in_timestamp", OracleDbType.TimeStampTZ),
				new OracleParameter("@in_tags", OracleDbType.Varchar2),
				new OracleParameter("@in_pointids",OracleDbType.Clob),
                new OracleParameter("@in_points", OracleDbType.Clob)};
            cmdParms[0].Value = model.id;
            cmdParms[1].Value = model.version;
            cmdParms[2].Value = model.userid;
            cmdParms[3].Value = model.username;
            cmdParms[4].Value = model.changeset;
            cmdParms[5].Value = model.timestamp;
            cmdParms[6].Value = model.tags;
            cmdParms[7].Value = model.pointids;
            cmdParms[8].Value = model.points;
            return OracleDBHelper.ExecuteSql(strSql.ToString(), cmdParms);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public int Update(polyline model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polyline SET ");
            strSql.Append("objectid=@in_objectid,");
            strSql.Append("osmid=@in_id,");
            strSql.Append("versionid=@in_version,");
            strSql.Append("versionsub=@in_versionsub,");
            strSql.Append("versionfinal=@in_versionfinal,");
            strSql.Append("isArea=@in_isArea,");
            strSql.Append("userid=@in_userid,");
            strSql.Append("username=@in_username,");
            strSql.Append("userReputation=@in_userReputation,");
            strSql.Append("changeset=@in_changeset,");
            strSql.Append("visible=@in_visible,");
            strSql.Append("timestamps=@in_timestamp,");
            strSql.Append("tags=@in_tags,");
            strSql.Append("updateTime=@in_updateTime,");
            strSql.Append("pointids=@in_pointids,");
            strSql.Append("points=@in_points,");
            strSql.Append("centroidX=@in_centroidX,");
            strSql.Append("centroidY=@in_centroidY,");
            strSql.Append("areaV=@in_areaV,");
            strSql.Append("areaG=@in_areaG,");
            strSql.Append("code=@in_code,");
            strSql.Append("geomLine=@in_geomLine,");
            strSql.Append("geom=@in_geom,");
            strSql.Append("centrdiff=@in_centrdiff,");
            strSql.Append("areadiffsim=@in_areadiffsim,");
            strSql.Append("shapediffsim=@in_shapediffsim,");
            strSql.Append("timestampDiff=@in_timestampDiff,");
            strSql.Append("supportDegreeSimArea=@in_supportDegreeSimArea,");
            strSql.Append("supportDegreeSimShape=@in_supportDegreeSimShape,");
            strSql.Append("trustValue=@in_trustValue");
            strSql.Append(" WHERE objectid=@in_objectid");
            OracleParameter[] cmdParms = {
                new OracleParameter("@in_objectid", OracleDbType.Int64), 
				new OracleParameter("@in_id", OracleDbType.Varchar2),
				new OracleParameter("@in_version", OracleDbType.Int16),
				new OracleParameter("@in_userid",OracleDbType.Varchar2),
				new OracleParameter("@in_username", OracleDbType.Varchar2),
				new OracleParameter("@in_changeset",  OracleDbType.Varchar2),
				new OracleParameter("@in_timestamp", OracleDbType.TimeStampTZ),
				new OracleParameter("@in_tags", OracleDbType.Varchar2),
				new OracleParameter("@in_pointids",OracleDbType.Clob),
                new OracleParameter("@in_points", OracleDbType.Clob)};
            cmdParms[0].Value = model.objectid;
            cmdParms[1].Value = model.id;
            cmdParms[2].Value = model.version;
            cmdParms[3].Value = model.userid;
            cmdParms[4].Value = model.username;
            cmdParms[5].Value = model.changeset;
            cmdParms[6].Value = model.timestamp;
            cmdParms[7].Value = model.tags;
            cmdParms[8].Value = model.pointids;
            cmdParms[9].Value = model.points;
            return OracleDBHelper.ExecuteSql(strSql.ToString(), cmdParms);
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public int Delete(long objectid)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("DELETE FROM polyline ");
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
            return OracleDBHelper.GetMaxID("objectid", "polyline");
        }
        /// <summary>
        /// 是否存在该记录
        /// </summary>
        //public bool Exists(long id)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("SELECT COUNT(1) FROM polyline");
        //    strSql.Append(" WHERE id=@in_id");
        //    DbParameter[] cmdParms = {
        //        new OracleParameter("@in_id", DbType.Int64)};

        //    return PostSqlHelper.Exists(strSql.ToString(), cmdParms);
        //}

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public polyline GetModel(long objectid)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT * FROM polyline ");
            strSql.Append(" WHERE objectid={0}"+objectid);
            //OracleParameter[] cmdParms = {
            //    new OracleParameter("@in_objectid", OracleDbType.Int64)};
            //cmdParms[0].Value = objectid;
            polyline model = null;
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                while (dr.Read())
                {
                    model = GetModel(dr);
                }
                return model;
            }
        }

        /// <summary>
        /// 获取泛型数据列表
        /// </summary>
        public List<polyline> GetList()
        {
            StringBuilder strSql = new StringBuilder("SELECT objectid, osmid,versionid,userid,username,changeset,pointsid, starttime,tags FROM polyline order by objectid ");
            //StringBuilder strSql = new StringBuilder("SELECT * FROM polyline order by objectid");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                //List<polyline> lst = GetList(dr);
                //return lst;

                List<polyline> lst = new List<polyline>();
                while (dr.Read())
                {
                    lst.Add(GetModel(dr));
                }
                //dr.Close();
                return lst;
            }
        }
        /// <summary>
        /// 得到特定ID的LIST
        /// </summary>
        /// <returns></returns>
        public List<polyline> GetListBYID(string a)
        {
            //StringBuilder strSql = new StringBuilder("SELECT * FROM polyline where id=10636421 order by objectid ");
            //StringBuilder strSql = new StringBuilder("SELECT * FROM polyline where id ="+a+"order by objectid");
            StringBuilder strSql = new StringBuilder("SELECT * FROM polyline where osmid in(" + a + ")order by objectid");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<polyline> lst = GetList(dr);
                return lst;
            }
        }
        /// <summary>
        /// 获取只包含ID的LIST
        /// </summary>
        /// <returns></returns>
        public List<long> GetIdList()
        {
            //StringBuilder strSql = new StringBuilder("SELECT DISTINCT id FROM polylinesonversion WHERE id=10636421");
            StringBuilder strSql = new StringBuilder("SELECT DISTINCT osmid FROM polyline ");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<long> lst = GetIdList(dr);
                return lst;
            }
        }

        /// <summary>
        /// 得到数据条数
        /// </summary>
        public int GetCount(string condition)
        {
            return OracleDBHelper.GetCount("polyline", condition);
        }

        ///// <summary>
        ///// 分页获取泛型数据列表
        /////// </summary>
        ////public List<polyline> GetPageList(int pageSize, int pageIndex, string fldSort, bool sort, string condition)
        ////{
        ////    using (OracleDataReader dr = PostSqlHelper.GetPageList("osmHistoryWay", pageSize, pageIndex, fldSort, sort, condition))
        ////    {
        ////        List<polyline> lst = GetList(dr);
        ////        return lst;
        ////    }
        ////}

        #region -------- 私有方法，通常情况下无需修改 --------

        /// <summary>
        /// 由一行数据得到一个实体
        /// </summary>
        private polyline GetModel(OracleDataReader dr)
        {
            polyline model = new polyline();
            model.objectid = Convert.ToInt64(dr["objectid"]);
            model.id = Convert.ToInt64(dr["osmid"]);
            model.version = Convert.ToInt32(dr["versionid"]);
            model.userid = Convert.ToInt32(dr["userid"]);
            model.username = Convert.ToString(dr["username"]);
            if (dr["changeset"] == DBNull.Value)
            {
                model.changeset = -1;
            }
            else
            {
                model.changeset = Convert.ToInt64(dr["changeset"]);
            }
            model.timestamp = Convert.ToDateTime(dr["starttime"]);
            model.tags = Convert.ToString(dr["tags"]);
            model.pointids = Convert.ToString(dr["pointsid"]);
            //model.points = Convert.ToString(dr["points"]);
            return model;
        }

        /// <summary>
        /// 由OracleDataReader得到泛型数据列表
        /// </summary>
        private List<polyline> GetList(OracleDataReader dr)
        {
            List<polyline> lst = new List<polyline>();
            while (dr.Read())
            {
                lst.Add(GetModel(dr));
            }
            return lst;
        }
        /// <summary>
        /// 由OracleDataReader得到只包含ID的数据列表
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private List<long> GetIdList(OracleDataReader dr)
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



