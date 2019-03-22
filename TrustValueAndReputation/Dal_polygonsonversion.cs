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
    public class Dal_polygonsonversion
    {

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public void Add(polygonsonversion model)
        {
           
            // OracleDBHelper helper = new OracleDBHelper();
            // OracleConnection connection = helper.getOracleConnection();
            // if (connection.State == ConnectionState.Closed)
            //{
            //    connection.Open();
            //}
            // using (connection) //连接数据库
            // {
            //     int num = 0;//记录更新成功的数量
                 //StringBuilder strSql = new StringBuilder();
                 //strSql.Append("declare geom clob; begin  geom := {18};  INSERT INTO polygonsonversion(");
                 //strSql.Append("osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,tags,pointsid,points,trustValue,areadiffsim,centroidX,centroidY,shapediffsim,isArea,shape)");
                 //strSql.Append(" VALUES (");
                 //string str = string.Format("{0},{1},{2},{3},{4},'{5}',{6},'{7}','{8}','{9}','{10}','{11}',{12},'{13}',{14},{15},'{16}','{17}',sdo_geometry(geom,54004));end;", model.id, model.version, model.versionsub, model.versionfinal, model.userid, model.username.ToString(), model.userReputation, model.changeset.ToString(), model.timestamp.ToString(), model.tags.ToString().Replace("&", ";"), model.pointids.ToString(), model.points.ToString(), model.trustValue, model.areadiffsim.ToString(), model.centroidX, model.centroidY, model.shapediffsim.ToString(), model.isArea.ToString(), model.geom);
                 //strSql.Append(str);
                 string sql = string.Format("declare geom clob; begin  geom := '{18}';  INSERT INTO polygonsonversion(osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,tags,pointsid,points,trustValue,areadiffsim,centroidX,centroidY,shapediffsim,isArea,shape) VALUES ({0},{1},{2},{3},{4},'{5}',{6},'{7}','{8}','{9}','{10}','{11}',{12},'{13}',{14},{15},'{16}','{17}',sdo_geometry(geom,54004));end;", model.id, model.version, model.versionsub, model.versionfinal, model.userid, model.username.ToString(), model.userReputation, model.changeset.ToString(), model.timestamp.ToString(), model.tags.ToString().Replace("&", ";"), model.pointids.ToString(), model.points.ToString(), model.trustValue, model.areadiffsim.ToString(), model.centroidX, model.centroidY, model.shapediffsim.ToString(), model.isArea.ToString(), model.geom);
                 OracleDBHelper.ExecuteSql(sql);
            //OracleDBHelper.ExecuteSql(strSql.ToString());
                 //strSql.Append("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}", model.id, model.version, model.versionsub, model.versionfinal, model.userid,model.username,model.userReputation,model.changeset,model.timestamp,model.tags,model.pointids,model.points,model.trustValue,model.areaV,model.areaG,model.areadiffsim,model.centroidX,model.centroidY,model.shapediffsim,model.isArea,model.geom);
                 // strSql.Append("@in_id,@in_version,@in_versionsub,@in_versionfinal,@in_userid,@in_username,@in_userReputation,@in_changeset,@in_timestamp,@in_tags,@in_pointids,@in_points,@in_trustValue,@in_areaV,@in_areaG,@in_areadiffsim,@in_centroidX,@in_centroidY,@in_shapediffsim,@in_isArea,sdo_geometry('" + model.geom + "',4326))");
                 // OracleParameter[] cmdParms = {
                 //new OracleParameter("@in_id", OracleDbType.Int64),
                 //new OracleParameter("@in_version", OracleDbType.Int16),
                 //new OracleParameter("@in_versionsub",  OracleDbType.Int16),
                 //new OracleParameter("@in_versionfinal",  OracleDbType.Int16),                    
                 //new OracleParameter("@in_userid",OracleDbType.Int64),
                 //new OracleParameter("@in_username", OracleDbType.Varchar2),
                 //new OracleParameter("@in_userReputation", OracleDbType.Decimal),
                 //new OracleParameter("@in_changeset",  OracleDbType.Varchar2),
                 //new OracleParameter("@in_timestamp", OracleDbType.TimeStampTZ),
                 //new OracleParameter("@in_tags", OracleDbType.Varchar2),
                 //new OracleParameter("@in_pointids",OracleDbType.Clob),
                 //new OracleParameter("@in_points", OracleDbType.Clob),
                 //new OracleParameter("@in_trustValue", OracleDbType.Decimal),
                 //new OracleParameter("@in_areaV", OracleDbType.Varchar2),
                 //new OracleParameter("@in_areaG", OracleDbType.Varchar2),
                 //new OracleParameter("@in_areadiffsim", OracleDbType.Decimal),
                 //new OracleParameter("@in_centroidX",OracleDbType.Varchar2),
                 //new OracleParameter("@in_centroidY",OracleDbType.Varchar2),
                 //new OracleParameter("@in_shapediffsim",OracleDbType.Decimal),
                 //new OracleParameter("@in_isArea",OracleDbType.Decimal)};
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
                 ////cmdParms[13].Value = model.geomline;
                 //cmdParms[13].Value = model.areaV;
                 //cmdParms[14].Value = model.areaG;
                 //cmdParms[15].Value = model.areadiffsim;
                 //cmdParms[16].Value = model.centroidX;
                 //cmdParms[17].Value = model.centroidY;
                 //cmdParms[18].Value = model.shapediffsim;
                 //cmdParms[19].Value = model.isArea;



                 //using (OracleCommand cmd = new OracleCommand(strSql.ToString(), connection))
                 //{
                 //    try
                 //    {
                 //        //PrepareCommand(cmd, connection, null, strSql.ToString(), cmdParms); //调用PrepareCommand方法
                 //        Console.WriteLine(" strSql.ToString(): " + strSql.ToString());
                 //        Console.WriteLine("insert语句的connection状态为"+connection.State);
                 //        int obj = cmd.ExecuteNonQuery();
                 //        //cmd.Parameters.Clear();
                 //        if (obj > 0)
                 //        {
                 //            num++;
                 //        }
                 //    }
                     //catch (OracleException e)
                     //{
                     //    //continue;
                     //    throw new Exception(e.Message);
                     //}
                 //}
                 //if (num > 0)
                 //{
                 //    return true;
                 //}
                 //else
                 //{
                 //    return false;
                 //}

                 //}






                 //string  strSql = string.Format("INSERT INTO polygonsonversion(osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,timestamps,tags,pointids,points,trustValue,areaV,areaG,areadiffsim,centroidX,centroidY,shapediffsim,isArea,geom) VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}",model.id, model.version, model.versionsub, model.versionfinal, model.userid, model.username, model.userReputation, model.changeset, model.timestamp, model.tags, model.pointids, model.points, model.trustValue, model.areaV, model.areaG, model.areadiffsim, model.centroidX, model.centroidY, model.shapediffsim, model.isArea,model.geom);
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
                 //    new OracleParameter("@in_pointids",OracleDbType.Clob),
                 //    new OracleParameter("@in_points", OracleDbType.Clob),
                 //    new OracleParameter("@in_trustValue", OracleDbType.Decimal),
                 //    new OracleParameter("@in_areaV", OracleDbType.Varchar2),
                 //    new OracleParameter("@in_areaG", OracleDbType.Varchar2),
                 //    new OracleParameter("@in_areadiffsim", OracleDbType.Decimal),
                 //    new OracleParameter("@in_centroidX",OracleDbType.Varchar2),
                 //    new OracleParameter("@in_centroidY",OracleDbType.Varchar2),
                 //    new OracleParameter("@in_shapediffsim",OracleDbType.Decimal),
                 //    new OracleParameter("@in_isArea",OracleDbType.Decimal)};
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
                 ////cmdParms[13].Value = model.geomline;
                 //cmdParms[13].Value = model.areaV;
                 //cmdParms[14].Value = model.areaG;
                 //cmdParms[15].Value = model.areadiffsim;
                 //cmdParms[16].Value = model.centroidX;
                 //cmdParms[17].Value = model.centroidY;
                 //cmdParms[18].Value = model.shapediffsim;
                 //cmdParms[19].Value = model.isArea;
                 //return OracleDBHelper.ExecuteSql(strSql.ToString(), cmdParms);
             //}
        }
        /// <summary>
        /// 更新，一次打开数据库批量更新
        /// </summary>
        /// <param name="list">传入的参数是集合，批量传入</param>
        /// <returns>如果更新成功，则返回true</returns>
        public static bool update1(List<polygonsonversion> list) //MO_Model为 model层的用到的类
        {
            //DB.connectionString,数据库连接，可以调用DB里的connectionString
            //打开数据库
            OracleDBHelper helper = new OracleDBHelper();
            using (OracleConnection connection = helper.getOracleConnection()) //连接数据库
            {
                int num = 0;//记录更新成功的数量
                for (int i = 0; i < list.Count; i++) //循环集合
                {
                    polygonsonversion model = new polygonsonversion();
                    model = list[i];

                    StringBuilder sb = new StringBuilder();
                    sb.Append("update table set ");
                    string sql = String.Format("objectid ={0} where osmid={1}", model.objectid,model.id);
                    sb.Append(sql);


                    //sb.Append(" objectid =:classid");
                    //sb.Append(" where osmid=:id");

                    //OracleParameter[] parameters = {
                    //   new OracleParameter(":classid",OracleDbType.Int64,8),
                    //   new OracleParameter(":id",OracleDbType.Int64,9)
                    //};
                    //parameters[0].Value = model.objectid;
                    //parameters[3].Value = model.id;


                    using (OracleCommand cmd = new OracleCommand(sb.ToString()))
                    {
                        try
                        {
                            //PrepareCommand(cmd, connection, null, sb.ToString(), parameters); //调用PrepareCommand方法
                            //OracleCommand cmd = new OracleCommand(sb.ToString());
                            int obj = cmd.ExecuteNonQuery();
                            //cmd.Parameters.Clear();
                            if (obj > 0)
                            {
                                num++;
                            }
                        }
                        catch (OracleException e)
                        {
                            continue;
                            //throw new Exception(e.Message);
                        }
                    }
                }

                if (num > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// PrepareCommand
        /// </summary>
        //private static void PrepareCommand(OracleCommand cmd, OracleConnection conn, OracleTransaction trans, string cmdText, OracleParameter[] cmdParms)
        //{
        //    if (conn.State != ConnectionState.Open)
        //        conn.Open();
        //    cmd.Connection = conn;
        //    cmd.CommandText = cmdText;
        //    if (trans != null)
        //        cmd.Transaction = trans;
        //    cmd.CommandType = CommandType.Text;
        //    if (cmdParms != null)
        //    {
        //        foreach (OracleParameter parm in cmdParms)
        //            cmd.Parameters.Add(parm);
        //    }
        //} 

        ///// <summary>
        ///// 增加一条数据
        ///// </summary>
        //public int Add(polygonsonversion model)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("INSERT INTO polygonsonversion(");
        //    strSql.Append("id,version,versionsub,versionfinal,userid,username,changeset,timestamp,tags,pointids,points,geom)");
        //    strSql.Append(" VALUES (");
        //    strSql.Append("@in_id,@in_version,@in_versionsub,@in_versionfinal,@in_userid,@in_username,@in_changeset,@in_timestamp,@in_tags,@in_pointids,@in_points,'" + model.geom + "')");
        //    OracleParameter[] cmdParms = {
        //        new OracleParameter("@in_id", OracleDbType.Bigint),
        //        new OracleParameter("@in_version", OracleDbType.Smallint),
        //        new OracleParameter("@in_versionsub",  OracleDbType.Smallint),
        //        new OracleParameter("@in_versionfinal",  OracleDbType.Smallint),                    
        //        new OracleParameter("@in_userid",OracleDbType.Bigint),
        //        new OracleParameter("@in_username", OracleDbType.Varchar),
        //        //new OracleParameter("@in_userReputation", OracleDbType.Numeric),
        //        new OracleParameter("@in_changeset",  OracleDbType.Varchar),
        //        new OracleParameter("@in_timestamp", OracleDbType.TimestampTZ),
        //        new OracleParameter("@in_tags", OracleDbType.Varchar),
        //        new OracleParameter("@in_pointids",OracleDbType.Text),
        //        new OracleParameter("@in_points", OracleDbType.Text)};
        //        //new OracleParameter("@in_trustValue", OracleDbType.Numeric)};
        //    cmdParms[0].Value = model.id;
        //    cmdParms[1].Value = model.version;
        //    cmdParms[2].Value = model.versionsub;
        //    cmdParms[3].Value = model.versionfinal;
        //    cmdParms[4].Value = model.userid;
        //    cmdParms[5].Value = model.username;
        //    //cmdParms[6].Value = model.userReputation;
        //    cmdParms[6].Value = model.changeset;
        //    cmdParms[7].Value = model.timestamp;
        //    cmdParms[8].Value = model.tags;
        //    cmdParms[9].Value = model.pointids;
        //    cmdParms[10].Value = model.points;
        //    //cmdParms[11].Value = model.trustValue;
        //    //cmdParms[13].Value = model.geomline;
        //    return PostSqlHelper.ExecuteSql(strSql.ToString(), cmdParms);
        //}

        ///// <summary>
        ///// 更新一条数据
        ///// </summary>
        //public int Update(polygonsonversion model)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("UPDATE polygonsonversion SET ");
        //    strSql.Append("id=@in_id,");
        //    strSql.Append("version=@in_version,");
        //    strSql.Append("versionsub=@in_versionsub,");
        //    strSql.Append("versionfinal=@in_versionfinal,");
        //    strSql.Append("userid=@in_userid,");
        //    strSql.Append("username=@in_username,");
        //    strSql.Append("userReputation=@in_userReputation,");
        //    strSql.Append("changeset=@in_changeset,");
        //    strSql.Append("timestamp=@in_timestamp,");
        //    strSql.Append("tags=@in_tags,");
        //    strSql.Append("pointids=@in_pointids,");
        //    strSql.Append("points=@in_points,");
        //    strSql.Append("trustValue=@in_trustValue,");
        //    strSql.Append("areaV=@in_areaV,");
        //    strSql.Append("areaG=@in_areaG,");
        //    strSql.Append("areadiffsim=@in_areadiffsim");
        //    //strSql.Append("centroidX=@in_centroidX,");
        //    //strSql.Append("centroidY=@in_centroidY,");
        //    //strSql.Append("shapediffsim=@in_shapediffsim");
        //    strSql.Append(" WHERE objectid=@in_objectid");
        //    OracleParameter[] cmdParms = {
        //        new OracleParameter("@in_id", OracleDbType.Varchar),
        //        new OracleParameter("@in_version", OracleDbType.Smallint),
        //        new OracleParameter("@in_versionsub",  OracleDbType.Smallint),
        //        new OracleParameter("@in_versionfinal",  OracleDbType.Smallint),
        //        new OracleParameter("@in_userid",OracleDbType.Varchar),
        //        new OracleParameter("@in_username", OracleDbType.Varchar),
        //        new NpgsqlParameter("@in_userReputation", OracleDbType.Numeric),
        //        new NpgsqlParameter("@in_changeset",  OracleDbType.Varchar),
        //        new NpgsqlParameter("@in_timestamp", OracleDbType.TimestampTZ),
        //        new NpgsqlParameter("@in_tags", OracleDbType.Varchar),
        //        new NpgsqlParameter("@in_pointids",OracleDbType.Text),
        //        new NpgsqlParameter("@in_points", OracleDbType.Text),
        //        new NpgsqlParameter("@in_trustValue", OracleDbType.Text),
        //        new NpgsqlParameter("@in_objectid", OracleDbType.Bigint),
        //        new NpgsqlParameter("@in_areaV", OracleDbType.Varchar),
        //        new NpgsqlParameter("@in_areaG", OracleDbType.Varchar),
        //       // new NpgsqlParameter("@in_areadiffsim", OracleDbType.NVarChar),
        //        new NpgsqlParameter("@in_areadiffsim", OracleDbType.Numeric)};
        //    //new NpgsqlParameter("@in_centroidX",OracleDbType.NVarChar),
        //    //new NpgsqlParameter("@in_centroidY",OracleDbType.NVarChar),
        //    //new NpgsqlParameter("@in_shapediffsim",OracleDbType.NVarChar)};


        //    cmdParms[0].Value = model.id;
        //    cmdParms[1].Value = model.version;
        //    cmdParms[2].Value = model.versionsub;
        //    cmdParms[3].Value = model.versionfinal;
        //    cmdParms[4].Value = model.userid;
        //    cmdParms[5].Value = model.username;
        //    cmdParms[6].Value = model.userReputation;
        //    cmdParms[7].Value = model.changeset;
        //    cmdParms[8].Value = model.timestamp;
        //    cmdParms[9].Value = model.tags;
        //    cmdParms[10].Value = model.pointids;
        //    cmdParms[11].Value = model.points;
        //    cmdParms[12].Value = model.trustValue;
        //    cmdParms[13].Value = model.objectid;
        //    cmdParms[14].Value = model.areaV;
        //    cmdParms[15].Value = model.areaG;
        //    cmdParms[16].Value = model.areadiffsim;
        //    //cmdParms[17].Value = model.centroidX;
        //    //cmdParms[18].Value = model.centroidY;
        //    //cmdParms[19].Value = model.shapediffsim;
        //    return PostSqlHelper.ExecuteSql(strSql.ToString(), cmdParms);
        //}
        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(polygonsonversion model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polygonsonversion SET ");
            string sql = String.Format("osmid={0},versionid={1},versionsub={2},versionfinal={3},userid={4},username='{5}',userReputation={6},changeset='{4}',starttime='{8}',tags='{9}',pointsid='{10}',points='{11}',trustValue={12},areavs={13},areags={14},areadiffsim={15} WHERE objectid={16}", model.id, model.version, model.versionsub, model.versionfinal, model.userid, model.username, model.userReputation, model.changeset, model.timestamp, model.tags, model.pointids, model.points, model.trustValue, model.areaV, model.areaG, model.areadiffsim, model.objectid);
            strSql.Append(sql);

            //strSql.Append("osmid=@in_id,");
            //strSql.Append("versionid=@in_version,");
            //strSql.Append("versionsub=@in_versionsub,");
            //strSql.Append("versionfinal=@in_versionfinal,");
            //strSql.Append("userid=@in_userid,");
            //strSql.Append("username=@in_username,");
            //strSql.Append("userReputation=@in_userReputation,");
            //strSql.Append("changeset=@in_changeset,");
            //strSql.Append("timestamps=@in_timestamp,");
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
            //    new OracleParameter("@in_pointids",OracleDbType.Clob),
            //    new OracleParameter("@in_points", OracleDbType.Clob),
            //    new OracleParameter("@in_trustValue", OracleDbType.Decimal),
            //    new OracleParameter("@in_objectid", OracleDbType.Int64),
            //    new OracleParameter("@in_areaV", OracleDbType.Varchar2),
            //    new OracleParameter("@in_areaG", OracleDbType.Varchar2),
            //    new OracleParameter("@in_areadiffsim", OracleDbType.Decimal)};
            ////new OracleParameter("@in_centroidX",OracleDbType.Varchar),
            ////new NpgsqlParameter("@in_centroidY",OracleDbType.Varchar),
            ////new NpgsqlParameter("@in_shapediffsim",OracleDbType.Numeric)};


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
            //cmdParms[14].Value = model.centroidX;
            //cmdParms[15].Value = model.centroidY;
            //cmdParms[16].Value = model.shapediffsim;
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int update(polygonsonversion model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polygonsonversion SET ");
            string sql = String.Format("osmid={0},versionid={1},versionsub={2},versionfinal={3},userid={4},username='{5}',userReputation={6},changeset='{7}',starttime='{8}',tags='{9}',pointsid='{10}',points='{11}',trustValue={12},areavs={13},areags={14},areadiffsim={15},centroidX={16},centroidY={17},shapediffsim={18} WHERE objectid={19}", model.id, model.version, model.versionsub, model.versionfinal, model.userid, model.username, model.userReputation, model.changeset, model.timestamp, model.tags.ToString(), model.pointids, model.points, model.trustValue, model.areaV, model.areaG, model.areadiffsim,model.centroidX,model.centroidY,model.shapediffsim, model.objectid);
            strSql.Append(sql);
            //strSql.Append("osmid=@in_id,");
            //strSql.Append("versionid=@in_version,");
            //strSql.Append("versionsub=@in_versionsub,");
            //strSql.Append("versionfinal=@in_versionfinal,");
            //strSql.Append("userid=@in_userid,");
            //strSql.Append("username=@in_username,");
            //strSql.Append("userReputation=@in_userReputation,");
            //strSql.Append("changeset=@in_changeset,");
            //strSql.Append("timestamps=@in_timestamp,");
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
            //    new OracleParameter("@in_pointids",OracleDbType.Clob),
            //    new OracleParameter("@in_points", OracleDbType.Clob),
            //    new OracleParameter("@in_trustValue", OracleDbType.Decimal),
            //    new OracleParameter("@in_objectid", OracleDbType.Int64),
            //    //new NpgsqlParameter("@in_areaV", OracleDbType.NVarChar),
            //    //new NpgsqlParameter("@in_areaG", OracleDbType.NVarChar),
            //    //new NpgsqlParameter("@in_areadiffsim", OracleDbType.NVarChar),
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
            ////cmdParms[14].Value = model.areaV;
            ////cmdParms[15].Value = model.areaG;
            ////cmdParms[16].Value = model.areadiffsim;
            //cmdParms[14].Value = model.centroidX;
            //cmdParms[15].Value = model.centroidY;
            //cmdParms[16].Value = model.shapediffsim;
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 更新一条数据,point字段
        /// </summary>
        public int UpdateShp(double objectid, int userid, string point, DateTime time)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polygonsonversion SET ");
            strSql.Append("point='" + point + "'");
            strSql.Append(" WHERE objectid=" + objectid + " and userid=" + userid + " and starttime='" + time + "'");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }

        /// <summary>
        /// 更新一条数据,是否为有效实例
        /// </summary>
        public int UpdateIsValid(double objectid, string isArea)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polygonsonversion SET ");
            strSql.Append("isArea='" + isArea + "'");
            //strSql.Append("areadiffsim=" + areadiffsim + "");
            strSql.Append(" WHERE objectid=" + objectid + "");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 更新某表所有数据,标志为线或者面数据20180608_hz
        /// </summary>
        public int UpdateTableIsValid(string tableName, string isArea)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(" UPDATE " + tableName + " SET ");
            strSql.Append("isArea='" + isArea + "'");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 更新某表信誉度值和可信度值为初始值
        /// </summary>
        public int UpdateTablereputationAndtrustvalueNull(string tableName)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(" UPDATE " + tableName + " SET ");
            strSql.Append("userreputation= null,trustvalue= null");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public int Delete(long objectid)
        {
            OracleDBHelper helper=new OracleDBHelper ();
            StringBuilder strSql = new StringBuilder();
            strSql.Append("DELETE FROM polygonsonversion ");
            string sql = String.Format(" WHERE objectid={0}",objectid);
            strSql.Append(sql);
            //using (OracleCommand cmd = new OracleCommand(strSql.ToString()))
            //{
                try
                {
            //        //PrepareCommand(cmd, connection, null, strSql.ToString(), cmdParms); //调用PrepareCommand方法
            //        Console.WriteLine(" strSql.ToString(): " + strSql.ToString());
            //        int obj = cmd.ExecuteNonQuery();
                    int obj=helper.sqlExecuteUnClose(strSql.ToString());
                    //cmd.Parameters.Clear();
                    //if (obj > 0)
                    //{
                    //    num++;
                    //}
                    return obj;
                }
                catch (Exception e)
                {
                    //continue;
                    throw new Exception(e.Message);
                }
            //}
            //OracleParameter[] cmdParms = {
            //    new OracleParameter("@in_objectid", OracleDbType.Int64)};
            //cmdParms[0].Value = objectid;
            //return OracleDBHelper.ExecuteSql(strSql.ToString(), cmdParms);
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public int Delete(polygonsonversion model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("DELETE FROM polygonsonversion ");
            //strSql.Append(" WHERE osmid=@in_id");
            //strSql.Append(" AND versionid=@in_version");
            //strSql.Append(" AND versionsub=@in_versionsub");
            string sql = String.Format(" WHERE osmid={0} AND versionid={1} AND versionsub={2}",model.id,model.version,model.versionsub);
            strSql.Append(sql);
            //OracleParameter[] cmdParms = {
            //    new OracleParameter("@in_id", OracleDbType.Int64),
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
            return OracleDBHelper.GetMaxID("objectid", "polygonsonversion");
        }

        ///// <summary>
        ///// 是否存在该记录
        ///// </summary>
        //public bool Exists(long objectid)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    strSql.Append("SELECT COUNT(1) FROM polygonsonversion");
        //    strSql.Append(" WHERE objectid=@in_objectid");
        //    DbParameter[] cmdParms = {
        //        laowoHelper.CreateInDbParameter("@in_objectid", OracleDbType.Int64, objectid)};

        //    object obj = laowoHelper.ExecuteScalar(CommandType.Text, strSql.ToString(), cmdParms);
        //    return laowoHelper.GetInt(obj) > 0;
        //}

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public polygonsonversion GetModel(long objectid)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT OBJECTID,osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,tags,pointsid,trustValue, areavs,areags,areadiffsim,centroidX,centroidY,shapediffsim,isArea,sdo_geometry.get_wkt(shape) FROM polygonsonversion ");
            //strSql.Append(" WHERE objectid=@in_objectid");
            string sql = String.Format(" WHERE objectid={0}",objectid);
            //OracleParameter[] cmdParms = {
            //    new OracleParameter("@in_objectid", OracleDbType.Int64)};
            //cmdParms[0].Value = objectid;
            polygonsonversion model = null;
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
        public List<polygonsonversion> GetList()
        {
            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,tags,pointsid,points,trustValue, areavs,areags,areadiffsim,centroidX,centroidY,shapediffsim,isArea,sdo_geometry.get_wkt(shape) FROM polygonsonversion order by objectid");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<polygonsonversion> lst = GetList(dr);
                return lst;
            }
        }


        /// <summary>
        /// 根据ID获取泛型数据列表
        /// </summary>
        public List<polygonsonversion> GetList(long id)
        {
            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,tags,pointsid,points,trustValue, areavs,areags,areadiffsim,centroidX,centroidY,shapediffsim,isArea,sdo_geometry.get_wkt(shape) FROM polygonsonversion WHERE osmid='" + id + "'ORDER BY objectid");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<polygonsonversion> lst = GetList(dr);
                return lst;
            }
        }
        //// <summary>
        //// 根据ID获取泛型数据列表
        //// </summary>
        ////public List<polygonsonversion> GetuseridList(long userid)
        ////{
        ////    StringBuilder strSql = new StringBuilder("SELECT userid FROM polygonsonversion where userid =" + userid + "ORDER BY objectid");
        ////    using (OracleDataReader dr = PostSqlHelper.ExecuteReader(strSql.ToString()))
        ////    {
        ////        string str = Convert.ToString(dr["userid"]);
        ////        List<polygonsonversion> lst = GetList(dr);
        ////        return lst;
        ////    }
        ////}
        /// <summary>
        /// 得到数据条数
        /// </summary>
        public int GetCount(string condition)
        {
            return OracleDBHelper.GetCount("polygonsonversion", condition); ;
        }

        /// <summary>
        /// 获取点串
        /// </summary>
        public string GetPoints(long objectid)
        {
            // StringBuilder strSql = new StringBuilder("select points from dbo.polygonsonversion where objectid=537" + objectid);
            StringBuilder strSql = new StringBuilder("select points from polygonsonversion " + objectid);
            //StringBuilder strSql = new StringBuilder("SELECT DISTINCT id FROM polygonsonversion ");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                string str = Convert.ToString(dr["points"]);
                return str;
            }
        }

        /// <summary>
        /// 获取泛型数据列表
        /// </summary>
        public List<long> GetIdList()
        {
            //StringBuilder strSql = new StringBuilder("SELECT DISTINCT id FROM polygonsonversion WHERE objectid>22");
            StringBuilder strSql = new StringBuilder("SELECT DISTINCT osmid FROM polygonsonversion ");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<long> lst = GetIdList(dr);
                return lst;
            }
        }
        /// <summary>
        /// 根据userID获取线和面按id和贡献时间排列的的泛型数据列表--2018hz
        /// </summary>
        public List<polygonsonversion> GetAllListbyuserid(long userid)
        {
            //StringBuilder strSql = new StringBuilder("select * from polygonsonversion where userid='1868724' union  select * from polylinesonversion where userid='1868724' order by id,timestamp ");//测试
            //StringBuilder strSql = new StringBuilder("select * from polygonsonversion where userid='43766' union  select * from polylinesonversion where userid='43766' order by id,timestamp ");//测试43766
            StringBuilder strSql = new StringBuilder("select OBJECTID,osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,tags,pointsid,points,trustValue,areadiffsim,centroidX,centroidY,shapediffsim,isArea from polygonsonversion where userid='" + userid + "' union  select OBJECTID, osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,tags,pointsid,points,trustValue,areadiffsim,centroidX,centroidY,shapediffsim,isArea from polylinesonversion where userid='" + userid + "' order by osmid,starttime ");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<polygonsonversion> lst = GetList(dr);
                return lst;
            }
        }
        /// <summary>
        /// 根据userid获得泛型数据列表
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<polygonsonversion> GetListbyuserid(long userid)
        {
            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,tags,pointsid,trustValue, areavs,areags,areadiffsim,centroidX,centroidY,shapediffsim,isArea,sdo_geometry.get_wkt(shape) FROM polygonsonversion where userid =" + userid + "ORDER BY objectid");

            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<polygonsonversion> lst = GetList(dr);
                return lst;
            }
        }
        /// <summary>
        /// 更新用户信誉度
        /// 通过视图userStatic中的average字段更新用户信誉度
        /// </summary>
        /// <returns></returns>
        public int UpdateUserReputation()
        {
            StringBuilder strSql = new StringBuilder("update polygonsonversion set userReputation=(select average from userStatic where polygonsonversion.userid=userStatic.userid)");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
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
            StringBuilder strSql = new StringBuilder("SELECT avg(userreputation) FROM polygonsonversion;");
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
        /// 更新用户信誉度和目标信誉度
        /// </summary>
        /// <returns></returns>
        public int UpdateReputationandtrustvalue(long objectid, double userreputation,double trusvalue)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polygonsonversion SET ");
            strSql.Append("userreputation='" + userreputation + "',trustvalue='"+trusvalue+"'WHERE objectid='" + objectid + "'");
            //strSql.Append("name='" + rank + "'");
            //strSql.Append(" WHERE userid=" + userid + "");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 更新用户信誉度和目标信誉度—2018hz
        /// </summary>
        /// <returns></returns>
        public int UpdateReputationAndTrustvalue(long objectid,string tableName, double userreputation, double trusvalue)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE " + tableName + " SET ");
            strSql.Append("userreputation='" + userreputation + "',trustvalue='" + trusvalue + "'WHERE objectid='" + objectid + "'");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }
        /// <summary>
        /// 更新用户信誉度
        /// </summary>
        /// <returns></returns>
        public int UpdateUserReputation(int userid, decimal userreputation)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("UPDATE polygonsonversion SET ");
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
            strSql.Append("UPDATE polygonsonversion SET ");
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
            strSql.Append("UPDATE polygonsonversion SET ");
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
            strSql.Append("UPDATE polygonsonversion SET ");
            strSql.Append("prank='" + rank + "'");
            //strSql.Append("name='" + rank + "'");
            strSql.Append(" WHERE objectid=" + objectid + "");
            return OracleDBHelper.ExecuteSql(strSql.ToString());
        }

        /// <summary>
        /// 获取泛型数据列表
        /// </summary>
        public List<polygonsonversion> Getbytimelist()
        {
            //StringBuilder strSql = new StringBuilder("SELECT * FROM polygonsonversion where id=10640531");
            StringBuilder strSql = new StringBuilder("SELECT OBJECTID,osmid,versionid,versionsub,versionfinal,userid,username,userReputation,changeset,starttime,tags,pointsid,trustValue, areavs,areags,areadiffsim,centroidX,centroidY,shapediffsim,isArea,shape FROM polygonsonversion where userid=55696 order by timestamp asc");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<polygonsonversion> lst = GetList(dr);
                return lst;
            }
        }
        ///// <summary>
        ///// 分页获取泛型数据列表
        ///// </summary>
        //public List<polygonsonversion> GetPageList(int pageSize, int pageIndex, string fldSort, bool sort, string condition)
        //{
        //    using (OracleDataReader dr = laowoHelper.GetPageList("polygonsonversion", pageSize, pageIndex, fldSort, sort, condition))
        //    {
        //        List<polygonsonversion> lst = GetList(dr);
        //        return lst;
        //    }
        //}

        #region -------- 私有方法，通常情况下无需修改 --------

        /// <summary>
        /// 由一行数据得到一个实体
        /// </summary>
        private polygonsonversion GetModel(OracleDataReader dr)
        {
            polygonsonversion model = new polygonsonversion();
            model.objectid = Convert.ToInt64(dr["objectid"]);
            model.id = Convert.ToInt64(dr["osmid"]);
            model.version = Convert.ToInt32(dr["versionid"]);
            model.versionsub = Convert.ToInt32(dr["versionsub"]);
            model.versionfinal = Convert.ToInt32(dr["versionfinal"]);
            model.userid = Convert.ToInt64(dr["userid"]);
            model.username = Convert.ToString(dr["username"]);
            try {
                if (dr["isarea"].ToString() == "")
                {
                    model.isArea = -1;
                }
                else
                {
                    Console.WriteLine(dr["isarea"]);
                    model.isArea = Convert.ToInt32(dr["isarea"]);

                }
            }catch(Exception ex)
            {

                ex.Message.ToString();

            }


            try
            {
                if (dr["userReputation"].ToString() == "")
                {
                    model.userReputation = -1;
                }
                else {
                    model.userReputation = Convert.ToDouble(dr["userReputation"].ToString());
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
                model.trustValue = -1;
            }
                try
            {
                if (dr["areadiffsim"].ToString() == "")
                {
                    model.areadiffsim = -1;
                }
                else
                {
                    model.areadiffsim = Convert.ToDecimal(dr["areadiffsim"]);
                }

               
            }
            catch (System.Exception ex)
            {
                model.areadiffsim = -1;
            }

            try
            {
                if (dr["shapediffsim"].ToString() == "")
                {
                    model.areadiffsim = -1;
                }
                else
                {
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
        private List<polygonsonversion> GetList(OracleDataReader dr)
        {
            List<polygonsonversion> lst = new List<polygonsonversion>();
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
    }
}
        #endregion


