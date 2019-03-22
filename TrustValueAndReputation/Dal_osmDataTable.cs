using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;

namespace TrustValueAndReputation
{
    public class Dal_osmDataTable
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public osmDataTable GetModel(string tableName)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT   tablename   FROM   pg_tables  ");
            strSql.Append("where tablename ='"+tableName+"'");
            osmDataTable model = null;
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
        /// 获取泛型数据列表
        /// </summary>
        public List<osmDataTable> GetList(string tableName)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT table_name as name FROM user_tables ");
            strSql.Append(" where table_name like '" + tableName + "%'");
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(strSql.ToString()))
            {
                List<osmDataTable> lst = GetList(dr);
                return lst;
            }
        }

        #region -------- 私有方法，通常情况下无需修改 --------

        /// <summary>
        /// 由一行数据得到一个实体
        /// </summary>
        private osmDataTable GetModel(OracleDataReader dr)
        {
            osmDataTable model = new osmDataTable();
            model.TableName = Convert.ToString(dr["name"]);
            return model;
        }


        /// <summary>
        /// 由OracleDataReader得到泛型数据列表
        /// </summary>
        private List<osmDataTable> GetList(OracleDataReader dr)
        {
            List<osmDataTable> lst = new List<osmDataTable>();
            while (dr.Read())
            {
                lst.Add(GetModel(dr));
            }
            return lst;
        }

        #endregion
    }
}
