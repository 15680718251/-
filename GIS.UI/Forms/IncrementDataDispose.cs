using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using GIS.UI.AdditionalTool;
using GIS.Geometries;
using GIS.UI.WellKnownText;

namespace GIS.UI.Forms
{
    public partial class IncrementDataDispose : Form
    {

        OracleDBHelper helper = new OracleDBHelper();
        public IncrementDataDispose()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 获取表中的所有数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable getAllData(string tableName)
        {
            DataTable datatable = new DataTable();
            string sql = "";
            try
            {

                if ("oscarea".Equals(tableName))
                {
                    sql = string.Format("select osmid,version,fc,dsg,sdo_geometry.get_wkt(shape) geomStr_wkt,changetype,sdo_geom.sdo_area(shape, 0.005) area from {0} order by osmid,version", tableName);
                    datatable.Columns.Add("osmid");
                    datatable.Columns.Add("version");
                    datatable.Columns.Add("fc");
                    datatable.Columns.Add("dsg");
                    datatable.Columns.Add("geomStr_wkt");
                    datatable.Columns.Add("changetype");
                    datatable.Columns.Add("area");
                }
                else
                {
                    sql = string.Format("select osmid,fc,dsg,sdo_geometry.get_wkt(shape) geomStr_wkt,sdo_geom.sdo_area(shape, 0.005) area from {0} order by osmid", tableName);
                    datatable.Columns.Add("osmid");
                    datatable.Columns.Add("fc");
                    datatable.Columns.Add("dsg");
                    datatable.Columns.Add("geomStr_wkt");
                    datatable.Columns.Add("area");
                }
              
                using (OracleDataReader dr = helper.queryReader(sql))
                {

                    while (dr.Read())
                    {
                        DataRow dataRow = datatable.NewRow();
                        if ("oscarea".Equals(tableName))
                        {
                            dataRow["osmid"] = dr["osmid"].ToString();
                            dataRow["version"] = dr["version"].ToString();
                            dataRow["fc"] = dr["fc"].ToString();
                            dataRow["dsg"] = dr["dsg"].ToString();
                            dataRow["geomStr_wkt"] = dr["geomStr_wkt"].ToString();
                            dataRow["area"] = dr["area"].ToString();
                        }
                        else
                        {
                            dataRow["osmid"] = dr["osmid"].ToString();
                            dataRow["fc"] = dr["fc"].ToString();
                            dataRow["dsg"] = dr["dsg"].ToString();
                            dataRow["geomStr_wkt"] = dr["geomStr_wkt"].ToString();
                            dataRow["area"] = dr["area"].ToString();
                        }
                        datatable.Rows.Add(dataRow);
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return datatable;

        }

        /// <summary>
        /// 获取Oracle数据库中的所有基态表名
        /// </summary>
        /// <param name="conString"></param>
        /// <returns></returns>
        public List<string> getAreaTableNames()
        {
            List<string> areaTableNames = new List<string>();
            try
            {
                OracleDBHelper helper = new OracleDBHelper();
                OracleConnection con = helper.getOracleConnection();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                string sql = "select distinct table_name  from user_col_comments where lower(table_name) like '%residential%' or  lower(table_name) like '%vegetation%'or  lower(table_name) like '%OSC%'or lower(table_name) like '%soil%'or lower(table_name) like '%water%' order by  lower(table_name) ASC";
                using (OracleCommand com = new OracleCommand(sql, con))
                {
                    OracleDataReader dr = com.ExecuteReader();
                    while (dr.Read())
                    {
                        string name = dr[0].ToString();
                        areaTableNames.Add(name);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return areaTableNames;
        }


        public void DeleteData(DataTable getAlldata, string tableName)
        {
            getAlldata = getAllData(tableName);
            int dataCount = getAlldata.Rows.Count;
            int count = 0;
            string sql = "";
            try
            {
                OracleConnection con = helper.getOracleConnection();
                for (int i = 0; i < dataCount; i++)
                {
                    string geometry = getAlldata.Rows[i]["geomStr_wkt"].ToString();
                    string fc = getAlldata.Rows[i]["fc"].ToString();
                    string dsg = getAlldata.Rows[i]["dsg"].ToString();
                    string osmid = getAlldata.Rows[i]["osmid"].ToString();
                    double area =double.Parse(getAlldata.Rows[i]["osmid"].ToString());
                    if (fc==""||dsg==""||geometry==""||area==0)
                    {
                        sql = string.Format("delete from {0} where osmid='{1}'", tableName, osmid);
                        using (OracleCommand com = new OracleCommand(sql, con))
                        {
                            com.ExecuteNonQuery();
                        }
                        count++;
                    }
                    
                    
                }
                con.Close();
                Console.WriteLine("共删除{0}条数据！", count);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        /// <summary>
        /// 获取自交多边形并删除
        /// </summary>
        /// <param name="tableName"></param>
        private void GetSelfIntersectPolygonOsmidsCount(string tableName)
        {
            StringBuilder sb = null;
            try
            {
                   OracleConnection con = helper.getOracleConnection();
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    string sql = string.Format("select osmid,sdo_geometry.get_wkt(shape) geom from {0}", tableName);
                    using (OracleDataReader dr = helper.queryReader(sql))
                    {

                     sb = new StringBuilder();                       
                        while (dr.Read())
                        {
                            string osmid = dr["osmid"].ToString();
                            string wkt = dr["geom"].ToString();
                            Geometries.Geometry geom = GeometryFromWKT.Parse(wkt);
                            GeoPolygon polygon = geom as GeoPolygon;
                            if (polygon == null)
                            {
                                continue;
                            }
                            if (TopHelper.IsSelfIntersect(polygon))
                            {
                                sb.Append(string.Concat(",'", osmid, "'"));
                            }

                        }
                        if (sb.Length > 1)
                        {
                            string idsStr = sb.ToString().Substring(1);
                            string deleteStr = string.Format("delete from {0} where osmid in ({1})", tableName, idsStr);
                            Console.WriteLine(string.Format("\r\n\r\n\r\n表{0}中删除的数据是id为{1}的数据！\r\n\r\n\r\n", tableName, idsStr));      
                            helper.sqlExecute(sql);
                        }
                    }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private void DisposeBt_Click(object sender, EventArgs e)
        {
            string tableName = tableNameCBX.Text;
            DataTable dataTable = getAllData(tableName);
            DeleteData(dataTable, tableName);
            GetSelfIntersectPolygonOsmidsCount(tableName);
            MessageBox.Show("自交多边请清除完成！");

        }

        private void IncrementDataDispose_Load(object sender, EventArgs e)
        {
            List<string> tableName = getAreaTableNames();
            this.tableNameCBX.Items.Clear();
            foreach (string name in tableName)
            {
                this.tableNameCBX.Items.Add(name);
            }

        }

    }
}
