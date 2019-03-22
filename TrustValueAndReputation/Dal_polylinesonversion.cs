using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using GIS.UI.AdditionalTool;

namespace TrustValueAndReputation
{
    public class Dal_polylinesonversion
    {
        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(polylinesonversion model)
        {
            //StringBuilder strSql = new StringBuilder();
            //strSql.Append("INSERT INTO polylinesonversion(");
            //string strSql = String.Format("INSERT INTO polylinesonversion(osmid,versionid,versionsub,versionfinal,userid,username,changeset,starttime,tags,pointsid,points,trustValue,shape) VALUES ({0},{1},{2},{3},{4},'{5}','{6}','{7}','{8}','{9}','{10}',{11},sdo_geometry(geomline,54004));end;", model.id, model.version, model.versionsub, model.versionfinal, model.userid, model.username.ToString(), model.changeset.ToString(), model.timestamp.ToString(), model.tags.ToString().Replace("&", ";"), model.pointids.ToString(), model.points.ToString(), model.trustValue, model.geomline);
            string strSql = String.Format("declare  geomline clob;begin  geomline := '{12}'; INSERT INTO polylinesonversion(osmid,versionid,versionsub,versionfinal,userid,username,changeset,starttime,tags,pointsid,points,trustValue,shape) VALUES ({0},{1},{2},{3},{4},'{5}','{6}','{7}','{8}','{9}','{10}',{11},sdo_geometry(geomline,54004));end;", model.id, model.version, model.versionsub, model.versionfinal, model.userid, model.username.ToString(), model.changeset.ToString(), model.timestamp.ToString(), model.tags.ToString().Replace("&", ";"), model.pointids.ToString(), model.points.ToString(), model.trustValue, model.geomline);
            //strSql.Append("id,version,versionsub,versionfinal,userid,username,changeset,timestamp,tags,pointids,points,trustValue,geomline) VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12})",);
            //strSql.Append(" ");
            //string sql = String.Format({0},{1},{2},{3},{4},{5}{}{}{}{}{}{}{}{}{}{}{}{});
            //strSql.Append("@in_id,@in_version,@in_versionsub,@in_versionfinal,@in_userid,@in_username,@in_changeset,@in_timestamp,@in_tags,@in_pointids,@in_points,@in_trustValue,'" + model.geomline + "')");
            //OracleParameter[] cmdParms = {
            //    new OracleParameter("@in_id", OracleDbType.Int64),
            //    new OracleParameter("@in_version", OracleDbType.Int16),
            //    new OracleParameter("@in_versionsub",  OracleDbType.Int16),
            //    new OracleParameter("@in_versionfinal",  OracleDbType.Int16),                    
            //    new OracleParameter("@in_userid",OracleDbType.Int64),
            //    new OracleParameter("@in_username", OracleDbType.Varchar2),
            //    //new OracleParameter("@in_userReputation", OracleDbType.Numeric),
            //    new OracleParameter("@in_changeset",  OracleDbType.Varchar2),
            //    new OracleParameter("@in_timestamp", OracleDbType.TimeStampTZ),
            //    new OracleParameter("@in_tags", OracleDbType.Varchar2),
            //    new OracleParameter("@in_pointids",OracleDbType.Varchar2),
            //    new OracleParameter("@in_points", OracleDbType.Varchar2),
            //    new OracleParameter("@in_trustValue", OracleDbType.Decimal)};
            //cmdParms[0].Value = model.id;
            //cmdParms[1].Value = model.version;
            //cmdParms[2].Value = model.versionsub;
            //cmdParms[3].Value = model.versionfinal;
            //cmdParms[4].Value = model.userid;
            //cmdParms[5].Value = model.username;
            ////cmdParms[6].Value = model.userReputation;
            //cmdParms[6].Value = model.changeset;
            //cmdParms[7].Value = model.timestamp;
            //cmdParms[8].Value = model.tags;
            //cmdParms[9].Value = model.pointids;
            //cmdParms[10].Value = model.points;
            //cmdParms[11].Value = model.trustValue;
            ////cmdParms[13].Value = model.geomline;
            return OracleDBHelper.ExecuteSql(strSql);
        }
        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(polylinesonversion model)
        {
            //StringBuilder strSql = new StringBuilder();
            string strSql = String.Format("UPDATE polylinesonversion SET osmid={0},versionid={1},versionsub={2},versionfinal={3},userid={4},username='{5}',userReputation={6},changeset='{7}',starttime='{8}',tags='{9}',pointsid='{10}',points='{11}',trustValue={12},areadiffsim={13} WHERE objectid={14}", model.id, model.version, model.versionsub, model.versionfinal, model.userid, model.username, model.userReputation, model.changeset, model.timestamp, model.tags, model.pointids, model.points, model.trustValue, model.areadiffsim,model.objectid);
            //strSql.Append("UPDATE polylinesonversion SET ");
            //strSql.Append("id=@in_id,");
            //strSql.Append("version=@in_version,");
            //strSql.Append("versionsub=@in_versionsub,");
            //strSql.Append("versionfinal=@in_versionfinal,");
            //strSql.Append("userid=@in_userid,");
            //strSql.Append("username=@in_username,");
            //strSql.Append("userReputation=@in_userReputation,");
            //strSql.Append("changeset=@in_changeset,");
            //strSql.Append("timestamp=@in_timestamp,");
            //strSql.Append("tags=@in_tags,");
            //strSql.Append("pointids=@in_pointids,");
            //strSql.Append("points=@in_points,");
            //strSql.Append("trustValue=@in_trustValue,");
            //strSql.Append("areaV=@in_areaV,");
            //strSql.Append("areaG=@in_areaG,");
            //strSql.Append("areadiffsim=@in_areadiffsim ");
            ////strSql.Append("centroidX=@in_centroidX,");
            ////strSql.Append("centroidY=@in_centroidY,");
            ////strSql.Append("shapediffsim=@in_shapediffsim");
            //strSql.Append(" WHERE objectid=@in_objectid");
            //OracleParameter[] cmdParms = {
            //    new OracleParameter("@in_id", OracleDbType.Int64),
            //    new OracleParameter("@in_version", OracleDbType.Int16),
            //    new OracleParameter("@in_versionsub",  OracleDbType.Int16),
            //    new OracleParameter("@in_versionfinal",  OracleDbType.Int16),
            //    new OracleParameter("@in_userid",OracleDbType.Int64),
            //    new OracleParameter("@in_username", OracleDbType.Varchar2),
            //    new OracleParameter("@in_userReputation", OracleDbType.Decimal),
            //    new OracleParameter("@in_changeset",  OracleDbType.Varchar2),
            //    new OracleParameter("@in_timestamp", OracleDbType.TimeStampTZ),
            //    new OracleParameter("@in_tags", OracleDbType.Varchar2),
            //    new OracleParameter("@in_pointids",OracleDbType.Varchar2),
            //    new OracleParameter("@in_points", OracleDbType.Varchar2),
            //    new OracleParameter("@in_trustValue", OracleDbType.Decimal),
            //    new OracleParameter("@in_objectid", OracleDbType.Int64),
            //    new OracleParameter("@in_areaV", OracleDbType.Varchar2),
            //    new OracleParameter("@in_areaG", OracleDbType.Varchar2),
            //    new OracleParameter("@in_areadiffsim", OracleDbType.Decimal)};
            ////new OracleParameter("@in_centroidX",OracleDbType.Varchar),
            ////new OracleParameter("@in_centroidY",OracleDbType.Varchar),
            ////new OracleParameter("@in_shapediffsim",OracleDbType.Numeric)};


            //cmdParms[0].Value = model.id;
            //cmdParms[1].Value = model.version;
            //cmdParms[2].Value = model.versionsub;
            //cmdParms[3].Value = model.versionfinal;
            //cmdParms[4].Value = model.userid;
            //cmdParms[5].Value = model.username;
            //cmdParms[6].Value = model.userReputation;
            //cmdParms[7].Value = model.changeset;
            //cmdParms[8].Value = model.timestamp;
            //cmdParms[9].Value = model.tags;
            //cmdParms[10].Value = model.pointids;
            //cmdParms[11].Value = model.points;
            //cmdParms[12].Value = model.trustValue;
            //cmdParms[13].Value = model.objectid;
            //cmdParms[14].Value = model.areaV;
            //cmdParms[15].Value = model.areaG;
            //cmdParms[16].Value = model.areadiffsim;
            ////cmdParms[14].Value = model.centroidX;
            ////cmdParms[15].Value = model.centroidY;
            ////cmdParms[16].Value = model.shapediffsim;
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int update(polylinesonversion model)
        {
            string strSql = String.Format("UPDATE polylinesonversion SET osmid={0},versionid={1},versionsub={2},versionfinal={3},userid={4},username='{5}',userReputation={6},changeset='{7}',starttime='{8}',tags='{9}',pointsid='{10}',points='{11}',trustValue={12},centroidX={13},centroidY={14},shapediffsim='{15}' WHERE objectid={16}", model.id, model.version, model.versionsub, model.versionfinal, model.userid, model.username, model.userReputation, model.changeset, model.timestamp, model.tags, model.pointids, model.points, model.trustValue, model.centroidX, model.centroidY, model.shapediffsim, model.objectid);

            //StringBuilder strSql = new StringBuilder();
            //strSql.Append("UPDATE polylinesonversion SET ");
            //strSql.Append("id=@in_id,");
            //strSql.Append("version=@in_version,");
            //strSql.Append("versionsub=@in_versionsub,");
            //strSql.Append("versionfinal=@in_versionfinal,");
            //strSql.Append("userid=@in_userid,");
            //strSql.Append("username=@in_username,");
            //strSql.Append("userReputation=@in_userReputation,");
            //strSql.Append("changeset=@in_changeset,");
            //strSql.Append("timestamp=@in_timestamp,");
            //strSql.Append("tags=@in_tags,");
            //strSql.Append("pointids=@in_pointids,");
            //strSql.Append("points=@in_points,");
            //strSql.Append("trustValue=@in_trustValue,");
            ////strSql.Append("areaV=@in_areaV,");
            ////strSql.Append("areaG=@in_areaG,");
            ////strSql.Append("areadiffsim=@in_areadiffsim,");
            //strSql.Append("centroidX=@in_centroidX,");
            //strSql.Append("centroidY=@in_centroidY,");
            //strSql.Append("shapediffsim=@in_shapediffsim");
            //strSql.Append(" WHERE objectid=@in_objectid");
            //OracleParameter[] cmdParms = {
            //    new OracleParameter("@in_id", OracleDbType.Int64),
            //    new OracleParameter("@in_version", OracleDbType.Int16),
            //    new OracleParameter("@in_versionsub",  OracleDbType.Int16),
            //    new OracleParameter("@in_versionfinal",  OracleDbType.Int16),
            //    new OracleParameter("@in_userid",OracleDbType.Int64),
            //    new OracleParameter("@in_username", OracleDbType.Varchar2),
            //    new OracleParameter("@in_userReputation", OracleDbType.Decimal),
            //    new OracleParameter("@in_changeset",  OracleDbType.Varchar2),
            //    new OracleParameter("@in_timestamp", OracleDbType.TimeStampTZ),
            //    new OracleParameter("@in_tags", OracleDbType.Varchar2),
            //    new OracleParameter("@in_pointids",OracleDbType.Varchar2),
            //    new OracleParameter("@in_points", OracleDbType.Varchar2),
            //    new OracleParameter("@in_trustValue", OracleDbType.Decimal),
            //    new OracleParameter("@in_objectid", OracleDbType.Int64),
            //    //new OracleParameter("@in_areaV", OracleDbType.NVarChar),
            //    //new OracleParameter("@in_areaG", OracleDbType.NVarChar),
            //    //new OracleParameter("@in_areadiffsim", OracleDbType.NVarChar),
            //    new OracleParameter("@in_centroidX",OracleDbType.Varchar2),
            //    new OracleParameter("@in_centroidY",OracleDbType.Varchar2),
            //    new OracleParameter("@in_shapediffsim",OracleDbType.Decimal)};


            //cmdParms[0].Value = model.id;
            //cmdParms[1].Value = model.version;
            //cmdParms[2].Value = model.versionsub;
            //cmdParms[3].Value = model.versionfinal;
            //cmdParms[4].Value = model.userid;
            //cmdParms[5].Value = model.username;
            //cmdParms[6].Value = model.userReputation;
            //cmdParms[7].Value = model.changeset;
            //cmdParms[8].Value = model.timestamp;
            //cmdParms[9].Value = model.tags;
            //cmdParms[10].Value = model.pointids;
            //cmdParms[11].Value = model.points;
            //cmdParms[12].Value = model.trustValue;
            //cmdParms[13].Value = model.objectid;

            //cmdParms[14].Value = model.centroidX;
            //cmdParms[15].Value = model.centroidY;
            //cmdParms[16].Value = model.shapediffsim;
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }


        ///// <summary>
        ///// 更新一条数据,point字段
        ///// </summary>
        //public int UpdateShp(double objectid, int userid, string point, DateTime time)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("UPDATE polylinesonversion SET ");
        //    strSql.Append("point='" + point + "'");
        //    strSql.Append(" WHERE objectid=" + objectid + " and userid=" + userid + " and timestamp='" + time + "'");
        //    return OracleDBHelper.ExecuteSql(strSql.ToString());
        //}

        ///// <summary>
        ///// 更新一条数据,是否为有效实例
        ///// </summary>
        //public int UpdateIsValid(double objectid, string isArea)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("UPDATE polylinesonversion SET ");
        //    strSql.Append("isArea='" + isArea + "'");
        //    strSql.Append(" WHERE objectid=" + objectid + "");
        //    return OracleDBHelper.ExecuteSql(strSql.ToString());
        //}

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public int Delete(long objectid)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("DELETE FROM polylinesonversion ");
            strSql.Append(" WHERE objectid=" + objectid);
            //OracleParameter[] cmdParms = {
            //    new OracleParameter("@in_objectid", OracleDbType.Int64)};
            //cmdParms[0].Value = objectid;
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public int Delete(polylinesonversion model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("DELETE FROM polylinesonversion ");
            strSql.Append(" WHERE id="+model.id);
            strSql.Append(" AND versionid="+model.version);
            strSql.Append(" AND versionsub="+model.versionsub);
            //OracleParameter[] cmdParms = {
            //    new OracleParameter("@in_id", OracleDbType.Varchar2),
            //    new OracleParameter("@in_version", OracleDbType.Int16),
            //    new OracleParameter("@in_versionsub", OracleDbType.Int16)};
            //cmdParms[0].Value = model.id;
            //cmdParms[1].Value = model.version;
            //cmdParms[2].Value = model.versionsub;
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }

        /// <summary>
        /// 得到最大ID
        /// </summary>
        public long GetMaxId()
        {
            return OracleDBHelper.GetMaxID("objectid", "polylinesonversion");
        }
        /// <summary>
        /// 得到最大OBJECTID
        /// </summary>
        /// <returns></returns>
        public long Getmaxobjectid()
        {
            return OracleDBHelper.GetMaxobj( "polylinesonversion");
        }
        ///// <summary>
        ///// 是否存在该记录
        ///// </summary>
        //public bool Exists(long objectid)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("SELECT COUNT(1) FROM polylinesonversion");
        //    strSql.Append(" WHERE objectid=@in_objectid");
        //    DbParameter[] cmdParms = {
        //        laowoHelper.CreateInDbParameter("@in_objectid", OracleDbType.Int64, objectid)};

        //    object obj = laowoHelper.ExecuteScalar(CommandType.Text, strSql.ToString(), cmdParms);
        //    return laowoHelper.GetInt(obj) > 0;
        //}

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public polylinesonversion GetModel(long objectid)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT OBJECTID,osmid,versionid,versionsub,versionfinal,userid,username,userReputation,urank,prank,changeset,starttime,fc,dsg,tags, pointsid,trustValue, areavs,areags,centrdiff,timestampdiff,visible, issimple,areadiffsim,shapediffsim,centroidX,centroidY,shapediffsim,isArea,sdo_geometry.get_wkt(shape),sdo_geometry.get_wkt(geomline) FROM polylinesonversion ");
            strSql.Append(" WHERE objectid=" + objectid);
            //OracleParameter[] cmdParms = {
            //    new OracleParameter("@in_objectid", OracleDbType.Int64)};
            //cmdParms[0].Value = objectid;
            polylinesonversion model = null;
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
        public List<polylinesonversion> GetList()
        {
            //StringBuilder strSql = new StringBuilder("SELECT * FROM polylinesonversion where id=4825630");
            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,fc,dsg,tags, pointsid,points,trustValue, areavs,areags,areadiffsim,shapediffsim,centroidX,centroidY,isArea,sdo_geometry.get_wkt(shape) FROM polylinesonversion order by objectid");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<polylinesonversion> lst = GetList(dr);
                return lst;
            }
        }


        /// <summary>
        /// 根据ID获取泛型数据列表
        /// </summary>
        public List<polylinesonversion> GetList(long id)
        {
            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,fc,dsg,tags, pointsid,points,trustValue, areavs,areags,areadiffsim,shapediffsim,centroidX,centroidY,isArea,sdo_geometry.get_wkt(shape) FROM polylinesonversion WHERE osmid='" + id + "'ORDER BY objectid");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<polylinesonversion> lst = GetList(dr);
                return lst;
            }
        }
        //// <summary>
        //// 根据ID获取泛型数据列表
        //// </summary>
        ////public List<polylinesonversion> GetuseridList(long userid)
        ////{
        ////    StringBuilder strSql = new StringBuilder("SELECT userid FROM polylinesonversion where userid =" + userid + "ORDER BY objectid");
        ////    using (OracleDataReader dr = PostSqlHelper.ExecuteReader(strSql.ToString()))
        ////    {
        ////        string str = Convert.ToString(dr["userid"]);
        ////        List<polylinesonversion> lst = GetList(dr);
        ////        return lst;
        ////    }
        ////}
        /// <summary>
        /// 得到数据条数
        /// </summary>
        public int GetCount(string condition)
        {
            return OracleDBHelper.GetCount("polylinesonversion", condition); ;
        }

        /// <summary>
        /// 获取点串
        /// </summary>
        public string GetPoints(long objectid)
        {
            // StringBuilder strSql = new StringBuilder("select points from dbo.polylinesonversion where objectid=537" + objectid);
            StringBuilder strSql = new StringBuilder("select points from polylinesonversion where objectid=" + objectid);
            //StringBuilder strSql = new StringBuilder("SELECT DISTINCT id FROM polylinesonversion ");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                string str = Convert.ToString(dr["points"]);
                return str;
            }
        }
        /// <summary>
        /// 根据objectid获得ID
        /// </summary>
        public string GetID(long objectid)
        {
            // StringBuilder strSql = new StringBuilder("select points from dbo.polylinesonversion where objectid=537" + objectid);
            StringBuilder strSql = new StringBuilder("select ID from polylinesonversion where objectid=" + objectid);
            //StringBuilder strSql = new StringBuilder("SELECT DISTINCT id FROM polylinesonversion ");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                string str = null;
                while (dr.Read())
                {
                   str = Convert.ToString(dr["id"]);
                    //double str = Getavguserreputation(dr);
                }
                return str;
            }
        }
        /// <summary>
        /// 获取泛型数据列表
        /// </summary>
        public List<long> GetIdList()
        {
            //StringBuilder strSql = new StringBuilder("SELECT DISTINCT id FROM polylinesonversion WHERE id=10636421");
            StringBuilder strSql = new StringBuilder("SELECT DISTINCT osmid FROM polylinesonversion");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<long> lst = GetIdList(dr);
                return lst;
            }
        }
        /// <summary>
        /// 更新一条数据,是否为有效实例
        /// </summary>
        public int UpdateIsValidline(double objectid, string isArea)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polylinesonversion SET ");
            strSql.Append("isArea='" + isArea + "'");
            //strSql.Append("areadiffsim=" + areadiffsim + "");
            strSql.Append(" WHERE objectid=" + objectid + "");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 根据userid获得泛型数据列表
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<polylinesonversion> GetListbyuserid(long userid)
        {
            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,versionid,versionsub,versionfinal,userid,username,userReputation,urank,prank,changeset,starttime,fc,dsg,tags, pointsid,trustValue, areavs,areags,centrdiff,timestampdiff,visible, issimple,areadiffsim,shapediffsim,centroidX,centroidY,shapediffsim,isArea, FROM polylinesonversion where userid =" + userid + "ORDER BY objectid");

            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<polylinesonversion> lst = GetList(dr);
                return lst;
            }
        }

        /// <summary>
        /// 获取线和面的所有userid
        /// </summary>
        public List<int> GetAlluserIdList()
        {
            //StringBuilder strSql = new StringBuilder("SELECT DISTINCT id FROM polygonsonversion WHERE objectid>22");
            StringBuilder strSql = new StringBuilder("select userid from polygonsonversion union select userid from polylinesonversion ");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<int> lst = GetuserIDList(dr);
                return lst;
            }
        }
        /// <summary>
        /// 更新用户信誉度
        /// 通过视图userStatic中的average字段更新用户信誉度
        /// </summary>
        /// <returns></returns>
        public double Getavguserreputation()
        {
            StringBuilder strSql = new StringBuilder("SELECT avg(userreputation) FROM polylinesonversion;");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                double str = 0;
                while (dr.Read())
                {
                    str = Convert.ToDouble(dr["avg"]);
                    //double str = Getavguserreputation(dr);
                }
                return str;
            }
        }
        /// <summary>
        /// 更新用户信誉度
        /// </summary>
        /// <returns></returns>
        public int UpdateUserReputation(int userid, decimal userreputation)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polylinesonversion SET ");
            strSql.Append("userreputation='" + userreputation + "'WHERE userid='" + userid + "'");
            //strSql.Append("name='" + rank + "'");
            //strSql.Append(" WHERE userid=" + userid + "");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 更新目标信誉度
        /// </summary>
        /// <returns></returns>
        public int UpdateUserT(long objectid, double trustvalue)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polylinesonversion SET ");
            strSql.Append("trustvalue='" + trustvalue + "'");
            //strSql.Append("name='" + rank + "'");
            strSql.Append(" WHERE objectid=" + objectid + "");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 更新用户信誉度等级
        /// </summary>
        /// <returns></returns>
        public int UpdateURank(int userid, int rank)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polylinesonversion SET ");
            strSql.Append("urank='" + rank + "'");
            //strSql.Append("name='" + rank + "'");
            strSql.Append(" WHERE userid=" + userid + "");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 更新目标信誉度等级
        /// </summary>
        /// <returns></returns>
        public int UpdatePRank(long objectid, int rank)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polylinesonversion SET ");
            strSql.Append("prank='" + rank + "'");
            //strSql.Append("name='" + rank + "'");
            strSql.Append(" WHERE objectid=" + objectid + "");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }


        ///// <summary>
        ///// 分页获取泛型数据列表
        ///// </summary>
        //public List<polylinesonversion> GetPageList(int pageSize, int pageIndex, string fldSort, bool sort, string condition)
        //{
        //    using (OracleDataReader dr = laowoHelper.GetPageList("polylinesonversion", pageSize, pageIndex, fldSort, sort, condition))
        //    {
        //        List<polylinesonversion> lst = GetList(dr);
        //        return lst;
        //    }
        //}

        #region -------- 私有方法，通常情况下无需修改 --------

        /// <summary>
        /// 由一行数据得到一个实体
        /// </summary>
        private polylinesonversion GetModel(OracleDataReader dr)
        {
            polylinesonversion model = new polylinesonversion();
            model.objectid = Convert.ToInt64(dr["objectid"]);
            model.id = Convert.ToInt64(dr["osmid"]);
            model.version = Convert.ToInt32(dr["versionid"]);
            model.versionsub = Convert.ToInt32(dr["versionsub"]);
            model.versionfinal = Convert.ToInt32(dr["versionfinal"]);
            model.userid = Convert.ToInt64(dr["userid"]);
            model.username = Convert.ToString(dr["username"]);
            try
            {
                if(dr["userReputation"].ToString()==""){
                 model.userReputation =-1;
                }else{
                model.userReputation = Convert.ToDouble(dr["userReputation"]);
                }
                
            }
            catch (System.Exception ex)
            {
                model.userReputation = -1;
            }

            model.changeset = Convert.ToInt32(dr["changeset"]);
            model.timestamp = Convert.ToDateTime(dr["starttime"]);
            model.tags = Convert.ToString(dr["tags"]);
            model.pointids = Convert.ToString(dr["pointsid"]);
            model.points = Convert.ToString(dr["points"]);
            try
            {
                if (dr["trustValue"].ToString() == "")
                {
                    model.trustValue = -1;
                }
                else
                {
                    model.trustValue = Convert.ToDouble(dr["trustValue"]);
                }
            }
            catch (System.Exception ex)
            {
                model.areadiffsim = -1;
            }

            
            try
            {
                if (dr["areadiffsim"].ToString() == "")
                {
                    model.areadiffsim = -1;
                }
                else {
                    model.areadiffsim = Convert.ToDecimal(dr["areadiffsim"]);
                }
            }
            catch (System.Exception ex)
            {
                model.areadiffsim = -1;
            }

            try
            {
                if (dr["shapediffsim"].ToString()=="")
                {
                    model.shapediffsim = -1;
                }else{
                    model.shapediffsim = Convert.ToDecimal(dr["shapediffsim"]);
                }

                
            }
            catch (System.Exception ex)
            {
                model.shapediffsim = -1;
            }

            return model;
        }

        /// <summary>
        /// 由OracleDataReader得到泛型数据列表
        /// </summary>
        private List<polylinesonversion> GetList(OracleDataReader dr)
        {
            List<polylinesonversion> lst = new List<polylinesonversion>();
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
        /// <summary>
        /// 由OracleDataReader得到只包含userID的数据列表
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private List<int> GetuserIDList(OracleDataReader dr)
        {
            List<int> lst = new List<int>();
            while (dr.Read())
            {
                lst.Add(Convert.ToInt32(dr["userid"]));
            }
            return lst;
        }
        #endregion
    }
}

