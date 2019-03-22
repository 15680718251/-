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
    public class Dal_polygon
    {
        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(polygon model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("INSERT INTO polygon(");
            strSql.Append("osmid,versionid,userid,username,changeset,starttime,tags,pointsid,points)");
            strSql.Append(" VALUES (");
            strSql.Append("@in_id,@in_version,@in_userid,@in_username,@in_changeset,@in_timestamp,@in_tags,@in_pointids,@in_points)");
            OracleParameter[] cmdParms = {
				new OracleParameter("@in_id", OracleDbType.Int64),
				new OracleParameter("@in_version", OracleDbType.Int16),
				new OracleParameter("@in_userid",OracleDbType.Int64),
				new OracleParameter("@in_username", OracleDbType.Varchar2),
				new OracleParameter("@in_changeset",  OracleDbType.Varchar2),
				new OracleParameter("@in_timestamp", OracleDbType.TimeStampTZ),
				new OracleParameter("@in_tags", OracleDbType.Varchar2),
				new OracleParameter("@in_pointids",OracleDbType.Varchar2),
                new OracleParameter("@in_points", OracleDbType.Varchar2)};
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
        public int Update(polygon model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polygon SET ");
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
				new OracleParameter("@in_id", OracleDbType.Int64),
				new OracleParameter("@in_version", OracleDbType.Int16),
				new OracleParameter("@in_userid",OracleDbType.Int64),
				new OracleParameter("@in_username", OracleDbType.Varchar2),
				new OracleParameter("@in_changeset",  OracleDbType.Varchar2),
				new OracleParameter("@in_timestamp", OracleDbType.TimeStampTZ),
				new OracleParameter("@in_tags", OracleDbType.Varchar2),
				new OracleParameter("@in_pointids",OracleDbType.Varchar2),
                new OracleParameter("@in_points", OracleDbType.Varchar2)};
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
            strSql.Append("DELETE FROM polygon ");
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
            return OracleDBHelper.GetMaxID("objectid", "polygon");
        }
        /// <summary>
        /// 是否存在该记录
        /// </summary>
        //public bool Exists(long id)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("SELECT COUNT(1) FROM polygon");
        //    strSql.Append(" WHERE id=@in_id");
        //    DbParameter[] cmdParms = {
        //        new OracleParameter("@in_id", DbType.Int64)};

        //    return PostSqlHelper.Exists(strSql.ToString(), cmdParms);
        //}

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public polygon GetModel(long objectid)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT * FROM polygon ");
            strSql.Append(" WHERE objectid="+objectid);
            //OracleParameter[] cmdParms = {
            //new OracleParameter("@in_objectid", OracleDbType.Int64)};
            //cmdParms[0].Value = objectid;
            polygon model = null;
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
        /// 获取所有面的ID，合并成字符串，格式：‘id1,id2,id3...idn’
        /// </summary>
        public string GetPolygonIdStr()
        {
            //StringBuilder strSql = new StringBuilder("SELECT * FROM polygon ");
            string str = "";
            StringBuilder strSql = new StringBuilder("SELECT DISTINCT osmid FROM polygon where objectid<31");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                while (dr.Read())
                {
                    str += Convert.ToString(dr["osmid"])+",";
                }
                return str;
            }
        }

        /// <summary>
        /// 获取pintids，合并成字符串，格式：‘id1,id2,id3...idn’
        /// </summary>
        public List<polygon> GetPointIdStr(string whereClause)
        {
            //StringBuilder strSql = new StringBuilder("SELECT * FROM polygon ");
            string str = "";
            StringBuilder strSql = new StringBuilder("SELECT * FROM polygon where objectid<31 AND osm='"+whereClause+"'");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<polygon> lst = GetList(dr);
                return lst;
            }
        }

        /// <summary>
        /// 获取泛型数据列表
        /// </summary>
        public List<polygon> GetList()
        {
            //OracleDBHelper helper = new OracleDBHelper();
            StringBuilder strSql = new StringBuilder("select objectid, osmid,versionid,userid,username,changeset, starttime,tags,pointsid from polygon order by objectid ");
            //StringBuilder strSql = new StringBuilder("SELECT * FROM polygon where id=10640531 order by objectid");
            //OracleDBHelper helper = new OracleDBHelper();
            //using (OracleDataReader rd = helper.queryReader(strSql.ToString()))
            using (OracleDataReader rd = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                //rd.IsClosed = false;
                //List<polygon> lst=null;
                //while (rd.Read())
                //{
                    //List<polygon> lst = GetList(rd);
                    //return lst;
                //}
                    List<polygon> lst = new List<polygon>();
                    while (rd.Read())
                    {
                        lst.Add(GetModel(rd));
                    }
                    return lst;
            }
        }
      
        ///// <summary>
        ///// 得到特定ID的LIST
        ///// </summary>
        ///// <returns></returns>
        //public List<polygon> GetListBYID(string a)
        //{
        //    //StringBuilder strSql = new StringBuilder("SELECT * FROM polyline where id=10636421 order by objectid ");
        //    //StringBuilder strSql = new StringBuilder("SELECT * FROM polyline where id ="+a+"order by objectid");
        //    StringBuilder strSql = new StringBuilder("SELECT * FROM polygon where osmid in(" + a + ")order by objectid");
        //    using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
        //    {
        //        List<polygon> lst = GetList(dr);
        //        return lst;
        //    }
        //}

        /// <summary>
        /// 得到数据条数
        /// </summary>
        public int GetCount(string condition)
        {
            return OracleDBHelper.GetCount("polygon", condition);
        }
       
        ///// <summary>
        ///// 分页获取泛型数据列表
        /////// </summary>
        ////public List<polygon> GetPageList(int pageSize, int pageIndex, string fldSort, bool sort, string condition)
        ////{
        ////    using (OracleDataReader dr = PostSqlHelper.GetPageList("osmHistoryWay", pageSize, pageIndex, fldSort, sort, condition))
        ////    {
        ////        List<polygon> lst = GetList(dr);
        ////        return lst;
        ////    }
        ////}

        #region -------- 私有方法，通常情况下无需修改 --------

        /// <summary>
        /// 由一行数据得到一个实体
        /// </summary>
        private polygon GetModel(OracleDataReader dr)
        {
            polygon model = new polygon();
            model.objectid = Convert.ToInt64(dr["objectid"]);
            model.id = Convert.ToInt64(dr["osmid"]);
            model.version = Convert.ToInt32(dr["versionid"]);
            model.userid = Convert.ToInt32(dr["userid"]);
            model.username = Convert.ToString(dr["username"]);
            if (dr["changeset"] == DBNull.Value)
            {
                model.changeset = -1;
            }
            else {
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
        private List<polygon> GetList(OracleDataReader dr)
        {
            List<polygon> lst = new List<polygon>();
            while (dr.Read())
            {
                lst.Add(GetModel(dr));
            }
            return lst;
        }

        #endregion

        public object id { get; set; }
    }
}


