using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;
using GIS.UI.AdditionalTool;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesGDB;
using System.IO;
using System.Threading;
using System.Collections;



//********** by dy 20180806

namespace GIS.UI.Forms
{
    public partial class AreaElementUpdate : Form
    {
        string conStr = "";//数据库连接字符串
        double bufferRadius = 0;//缓冲区半径字段
        double hausdroffThreshold = 0;//hausdroff阈值
        double AreaTreshold = 0;//重叠面积阈值
        int updataCount = 0;//更新量统计
        string tableName = "";
        int matchId = 0;
        int deleteId = 1;
        int createId = 2;
        int modifyId = 3;
        int keepId = 0;
        //System.Timers.Timer Mytimer = new System.Timers.Timer();
        //private long Timecount=0;
        //public delegate void SetControlValue(string value);
        private AxMapControl axMapControl1;//这是字段
        private AxTOCControl axTocContorl;

        AddMap addmap = new AddMap();
        public AxMapControl Axmapcontrol
        {
            get { return axMapControl1; }
            set { axMapControl1 = value; }
        }
        public AxTOCControl axTocControl
        {
            get { return axTocContorl; }
            set { axTocContorl = value; }
        }
        IFeatureLayer pFeatureLayer;//这是属性
        IWorkspace workspace;//创建工作空间对象

        Boolean matchSuccess = false;
        OracleDBHelper helper = new OracleDBHelper();

        public AreaElementUpdate(AxMapControl axMapControl1,AxTOCControl axTocContorl1)
        {
            InitializeComponent();
            this.Axmapcontrol = axMapControl1;
            this.axTocControl = axTocContorl1;
        }
        //public AreaElementUpdate(string conStr,AxMapControl axMapControl1)
        //{
        //    InitializeComponent();
        //    this.Axmapcontrol = axMapControl1;
        //    this.conStr = conStr;
        //}

        /// <summary>
        /// 面要素更新窗体触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AreaElementUpdate_Load(object sender, EventArgs e)
        {
            List<string> areaTableNames = getAreaTableNames( );
            this.areaEleBaseDataCBox.Items.Clear();
            this.areaEleIncreDataCBox.Items.Clear();
            
            if (areaTableNames != null && areaTableNames.Count > 0)
            {
                foreach (string name in areaTableNames)
                {
                    areaEleBaseDataCBox.Items.Add(name);
                    areaEleIncreDataCBox.Items.Add(name);
                }
                //InitTimer();
            }
            else
            {
                MessageBox.Show("对不起，您连接的数据库中无基态数据表！！！");
            }

            
            
        }
        /// <summary>
        /// 获取Oracle数据库中的所有基态表名
        /// </summary>
        /// <param name="conString"></param>
        /// <returns></returns>
        public List<string> getAreaTableNames( )
        {
            List<string> areaTableNames = new List<string>();
            try
            {
                OracleConnection con = helper.getOracleConnection();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                string sql = "select distinct table_name  from user_col_comments where lower(table_name) like '%residential%' or  lower(table_name) like '%vegetation%'or  lower(table_name) like '%water%'or lower(table_name) like '%soil%'or lower(table_name) like '%traffic%' order by  lower(table_name) ASC";
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

        /// <summary>
        /// 通过读取数据库中的数据表名，获取表中所有数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable getAllAreaData(string tableName)
        {
            DataTable dataTable = new DataTable();
            string sql = "";
            try
            {
                if ("OSCArea".Equals(tableName))
                {
                    sql = string.Format("SELECT oscid,versionid,fc,dsg,changetype,sdo_geometry.get_wkt(shape) geometry_wkt,sdo_geom.sdo_area(shape, 0.005) area,sdo_geometry.get_wkt(sdo_geom.sdo_centroid(shape,0.005)) centerPoint from {0} ORDER BY oscid", tableName);
                    dataTable.Columns.Add("osmid");
                    dataTable.Columns.Add("versionid");
                    dataTable.Columns.Add("fc");
                    dataTable.Columns.Add("dsg");
                    dataTable.Columns.Add("changetype");
                    dataTable.Columns.Add("geometry_wkt");
                    dataTable.Columns.Add("area");
                    dataTable.Columns.Add("centerPoint");
                }
                else
                {
                    sql = string.Format("SELECT objectid,nationcode,nationelename,osmid,versionid,fc,dsg,changetype,sdo_geometry.get_wkt(shape) geometry_wkt,sdo_geom.sdo_area(shape,0.005) area,sdo_geometry.get_wkt(sdo_geom.sdo_centroid(shape,0.005)) centerPoint from {0} ORDER BY osmid", tableName);
                    dataTable.Columns.Add("nationcode");
                    dataTable.Columns.Add("nationelename");
                    dataTable.Columns.Add("osmid");
                    dataTable.Columns.Add("versionid");
                    dataTable.Columns.Add("fc");
                    dataTable.Columns.Add("dsg");
                    dataTable.Columns.Add("changetype");
                    dataTable.Columns.Add("geometry_wkt");
                    dataTable.Columns.Add("area");
                    dataTable.Columns.Add("centerPoint");
                    dataTable.Columns.Add("objectid");
                }
                using (OracleDataReader dr = helper.queryReader(sql))
                {
                    while (dr.Read())
                    {
                        DataRow dataRow = dataTable.NewRow();
                        if ("OSCArea".Equals(tableName))
                        {
                            dataRow["oscid"] = dr["oscid"].ToString();
                            dataRow["versionid"] = dr["versionid"].ToString();
                            dataRow["fc"] = dr["fc"].ToString();
                            dataRow["dsg"] = dr["dsg"].ToString();
                            dataRow["changetype"] = dr["changetype"].ToString();
                            dataRow["geometry_wkt"] = dr["geometry_wkt"].ToString();
                            dataRow["area"] = dr["area"].ToString();
                            dataRow["centerPoint"] = dr["centerPoint"].ToString();
                            dataRow["objectid"] = dr["objectid"].ToString();
                        }
                        else
                        {
                            dataRow["nationcode"] = dr["nationcode"].ToString();
                            dataRow["nationelename"] = dr["nationelename"].ToString();
                            dataRow["osmid"] = dr["osmid"].ToString();
                            dataRow["versionid"] = dr["versionid"].ToString();
                            dataRow["fc"] = dr["fc"].ToString();
                            dataRow["dsg"] = dr["dsg"].ToString();
                            dataRow["changetype"] = dr["changetype"].ToString();
                            dataRow["geometry_wkt"] = dr["geometry_wkt"].ToString();
                            dataRow["area"] = dr["area"].ToString();
                            dataRow["centerPoint"] = dr["centerPoint"].ToString();
                            dataRow["objectid"] = dr["objectid"].ToString();
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return dataTable;
        }

        /// <summary>
        /// 通过缓冲区半径和geometry_wkt得到缓冲区内对象
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="geometry_wkt"></param>
        /// <returns></returns>
        public DataTable getDataByBuffer(string tableName, string geometry_wkt)
        {
            DataTable bufferData = new DataTable();
            string sql = "";
            try
            {
                sql = tableName.Equals("OSCArea") ? string.Format("SELECT oscid,versionid,fc,dsg,sdo_geometry.get_wkt(shape)geometry_wkt, sdo_geom.sdo_area(shape, 0.005)area FROM {0} WHERE SDO_WITHIN_DISTANCE(sdo_geometry.get_wkt(shape),'{1}',{2})=true", tableName, geometry_wkt, bufferRadius) :
                string.Format("SELECT objectid,nationcode,nationelename,osmid,fc,dsg,sdo_geometry.get_wkt(shape) geometry_wkt,sdo_geom.sdo_area(shape, 0.005) area,sdo_geom.sdo_area(SDO_GEOM.SDO_INTERSECTION(shape,sdo_geometry ('{1}',4326),0.05),0.05) overlayArea,sdo_geometry.get_wkt(sdo_geom.sdo_centroid(shape,0.005)) centerPoint FROM {0} WHERE  SDO_GEOM.SDO_DISTANCE(shape,sdo_geometry ('{1}',4326),0.5)=0", tableName, geometry_wkt);
                bufferData.Columns.Add("nationcode");
                bufferData.Columns.Add("nationelename");
                bufferData.Columns.Add("osmid");
                bufferData.Columns.Add("versionid");
                bufferData.Columns.Add("fc");
                bufferData.Columns.Add("dsg");
                bufferData.Columns.Add("geometry_wkt");
                bufferData.Columns.Add("area");
                bufferData.Columns.Add("overlayArea");
                bufferData.Columns.Add("centerPoint");
                bufferData.Columns.Add("objectid");

                using (OracleDataReader dr = helper.queryReader(sql))
                {
                    while (dr.Read())
                    {
                        DataRow bufferDataRow = bufferData.NewRow();
                        //if (tableName.Equals("OSCArea"))
                        //{
                        //    bufferDataRow["osmid"] = dr["osmid"].ToString();
                        //    bufferDataRow["version"] = dr["versionid"].ToString();
                        //}
                        //else
                        //{
                            bufferDataRow["osmid"] = dr["osmid"].ToString();
                        //}
                        bufferDataRow["nationcode"] = dr["nationcode"].ToString();
                        bufferDataRow["nationelename"] = dr["nationelename"].ToString();
                        bufferDataRow["fc"] = dr["fc"].ToString();
                        bufferDataRow["dsg"] = dr["dsg"].ToString();
                        bufferDataRow["geometry_wkt"] = dr["geometry_wkt"].ToString();
                        bufferDataRow["area"] = dr["area"].ToString();
                        bufferDataRow["overlayArea"] = dr["overlayArea"].ToString();
                        bufferDataRow["centerPoint"] = dr["centerPoint"].ToString();
                        bufferDataRow["objectid"] = dr["objectid"].ToString();
                        bufferData.Rows.Add(bufferDataRow);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return bufferData;
        }

       
        /// <summary>
        /// 搜索相邻且有公共边界的地物
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="geometry_wkt"></param>
        /// <returns></returns>
        public DataTable getDataNearby(string tableName,string geometry_wkt)
        {
            DataTable NearbyData = new DataTable();
            try
            {
                string sql = string.Format("SELECT objectid,nationcode,nationelename,osmid,fc,dsg,sdo_geometry.get_wkt(shape) geometry_wkt,sdo_geom.sdo_area(shape, 0.005) area,sdo_geom.sdo_area(SDO_GEOM.SDO_INTERSECTION(shape,sdo_geometry ('{1}',4326),0.05),0.05) overlayArea,sdo_geometry.get_wkt(sdo_geom.sdo_centroid(shape,0.005)) centerPoint FROM {0} WHERE  SDO_RELATE(shape,sdo_geometry ('{1}',4326), 'mask=TOUCH') ='TRUE'", tableName, geometry_wkt);
                NearbyData.Columns.Add("nationcode");
                NearbyData.Columns.Add("nationelename");
                NearbyData.Columns.Add("osmid");
                NearbyData.Columns.Add("versionid");
                NearbyData.Columns.Add("fc");
                NearbyData.Columns.Add("dsg");
                NearbyData.Columns.Add("geometry_wkt");
                NearbyData.Columns.Add("area");
                NearbyData.Columns.Add("overlayArea");
                NearbyData.Columns.Add("centerPoint");
                NearbyData.Columns.Add("objectid");
                using (OracleDataReader dr = helper.queryReader(sql))
                {
                    while (dr.Read())
                    {
                        DataRow NearbyDataRow = NearbyData.NewRow();
                        if (tableName.Equals("OSCArea"))
                        {
                            NearbyDataRow["osmid"] = dr["osmid"].ToString();
                            NearbyDataRow["version"] = dr["versionid"].ToString();
                        }
                        else
                        {
                            NearbyDataRow["osmid"] = dr["osmid"].ToString();
                        }
                        NearbyDataRow["nationcode"] = dr["nationcode"].ToString();
                        NearbyDataRow["nationelename"] = dr["nationelename"].ToString();
                        NearbyDataRow["fc"] = dr["fc"].ToString();
                        NearbyDataRow["dsg"] = dr["dsg"].ToString();
                        NearbyDataRow["geometry_wkt"] = dr["geometry_wkt"].ToString();
                        NearbyDataRow["area"] = dr["area"].ToString();
                        NearbyDataRow["overlayArea"] = dr["overlayArea"].ToString();
                        NearbyDataRow["centerPoint"] = dr["centerPoint"].ToString();
                        NearbyDataRow["objectid"] = dr["objectid"].ToString();
                        NearbyData.Rows.Add(NearbyDataRow);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }           
            return NearbyData;
        }

        public ArrayList getUpdateAfterObjectId(string tableName)
        {
            ArrayList objectid = new ArrayList(); ;
            string sql=string.Format("select objectid from {0} where updatestate is not NULL",tableName);
            using (OracleDataReader dr = helper.queryReader(sql))
            {
                while (dr.Read())
                {
                    objectid.Add(dr[0].ToString());
                }
            }
            return objectid;

        }

    
        //  public void dataRetain(string osmidA, string tableNameA, string tableNameB, string conString)
        //{
        //    try
        //    {
        //        //字段定义
        //        int osmidB = 0;
        //        string usernameB = "";
        //        int type_idB = 0;
        //        int versionB = 0;
        //        string timestampB = "";
        //        string tagB = "";
        //        string geomB = "";
        //        string fc_B = "";
        //        string dsg_B = "";
        //        string uid_B = "";
        //        int nationcodeB = 0;
        //        string nationelename = "";
        //        int objectidB = 0;

        //        //string sql = string.Format("SELECT nationcode,nationelename,osmid,username,userid,versionid,timestamps,fc,dsg,tags,sdo_geometry.get_wkt(shape) geometry_wkt FROM {0} WHERE  osmid='{1}'", tableNameA,osmidA);
        //        OracleConnection con = helper.getOracleConnection();
        //        //if (con.State == ConnectionState.Closed)
        //        //{
        //        //    con.Open();
        //        //}
        //        //using (OracleCommand com = new OracleCommand(sql, con))
        //        //{
        //        //    using (OracleDataReader rd = com.ExecuteReader())
        //        //    {
        //        //        while (rd.Read())
        //        //        {
        //        //            osmidB = int.Parse(rd["osmid"].ToString());
        //        //            nationcodeB =int.Parse(rd["nationcode"].ToString());
        //        //            nationelename=rd["nationelename"].ToString();
        //        //            usernameB = rd["username"].ToString();
        //        //            uid_B = rd["userid"].ToString();
        //        //            versionB = int.Parse(rd["versionid"].ToString());
        //        //            timestampB = rd["timestamps"].ToString();
        //        //            fc_B = rd["fc"].ToString();
        //        //            dsg_B = rd["dsg"].ToString();
        //        //            tagB = rd["tags"].ToString();
        //        //            geomB = rd["geometry_wkt"].ToString();
        //        //        }
        //        //    }
        //        ////}
        //        //con.Close();

        //       string sql2 = string.Format("SELECT objectid FROM (select * from {0} order by {0}.objectid desc) where rownum<=1", tableNameB);
        //        if (con.State == ConnectionState.Closed)
        //        {
        //            con.Open();
        //        }
        //        using (OracleCommand cmd = new OracleCommand(sql2, con))
        //        {
                    
        //            using (OracleDataReader dr = cmd.ExecuteReader())
        //            {
        //                while (dr.Read())
        //                {
        //                    objectidB = int.Parse(dr[0].ToString());
        //                }
                       

        //            }
                    
        //        }
        //        objectidB++;
        //       string sql1 = string.Format("INSERT INTO {0} (objectid,nationcode,nationelename,osmid,username,userid,versionid,timestamps,fc,dsg,tags,shape)(SELECT {1},{2}.nationcode,{2}.nationelename,{2}.osmid,{2}.username,{2}.userid,{2}.versionid,{2}.timestamps,{2}.fc,{2}.dsg,{2}.tags,{2}.shape FROM {2} WHERE  osmid='{3}')", tableNameB, objectidB, tableNameA, osmidA);
        //        //sql = string.Format("INSERT INTO {0} (objectid,nationcode,nationelename,osmid,username,userid,versionid,timestamps,fc,dsg,tags,shape) VALUES({1},{2},'{3}',{4},'{5}',{6},{7},'{8}','{9}','{10}','{11}',sdo_geometry ('{12}',4326))", tableNameB,objectidB,nationcodeB,nationelename,osmidB, usernameB, uid_B, versionB, timestampB, fc_B, dsg_B, tagB, geomB);
        //        using (OracleCommand cmd = new OracleCommand(sql1, con))
        //        {               
        //            cmd.ExecuteNonQuery();
        //            Console.WriteLine("成功插入osmid='{0}'数据:osmid='{1}'",osmidA,osmidB);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}
         //<summary>
         //将没有匹配到基态数据的情况设置为保留新增数据
         //</summary>
         //<param name="oscid"></param>
         //<param name="tableNameA"></param>
         //<param name="tableNameB"></param>
         //<param name="conString"></param>
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="osmidA"></param>
        /// <param name="tableNameA"></param>
        /// <param name="tableNameB"></param>
        /// <param name="conString"></param>
        public void dataRetain(string objectidA, string tableNameA, string tableNameB,string changeType,List<string> combineData)
        {
            try
            {
                //字段定义
                int objectidB = 0;
                string sql = "";
                OracleConnection con = helper.getOracleConnection();
                sql = string.Format("SELECT objectid FROM (select * from {0} order by {0}.objectid desc) where rownum<=1", tableNameB);
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                using (OracleCommand cmd = new OracleCommand(sql, con))
                {

                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            objectidB = int.Parse(dr[0].ToString());
                        }


                    }

                }
                objectidB++;
                matchId++;
                if (changeType == "create")
                {
                    sql = string.Format("INSERT INTO {0} (nationcode,nationelename,osmid,username,userid,versionid,starttime,fc,dsg,tags,shape,matchid,pointsid,updatestate)(SELECT {1}.nationcode,{1}.nationelename,{1}.osmid,{1}.username,{1}.userid,{1}.versionid,{1}.starttime,{1}.fc,{1}.dsg,{1}.tags,{1}.shape,{2},{1}.pointsid,'{3}' FROM {1} WHERE  objectid='{4}')", tableNameB, tableNameA, matchId,createId,objectidA);
                }
                else if (changeType == "modify")
                {
                    sql = string.Format("INSERT INTO {0} (nationcode,nationelename,osmid,username,userid,versionid,starttime,fc,dsg,tags,shape,matchid,pointsid,updatestate)(SELECT {1}.nationcode,{1}.nationelename,{1}.osmid,{1}.username,{1}.userid,{1}.versionid,{1}.starttime,{1}.fc,{1}.dsg,{1}.tags,{1}.shape,{2},{1}.pointsid ,'{3}' FROM {1} WHERE  objectid='{3}')", tableNameB, tableNameA, matchId,modifyId,objectidA);
                }
                else 
                {
                    sql = string.Format("INSERT INTO {0} (nationcode,nationelename,osmid,username,userid,versionid,starttime,fc,dsg,tags,shape,matchid,pointsid,updatestate)(SELECT {1}.nationcode,{1}.nationelename,{1}.osmid,{1}.username,{1}.userid,{1}.versionid,{1}.starttime,{1}.fc,{1}.dsg,{1}.tags,{1}.shape,{2},{1}.pointsid,'{3}' FROM {1} WHERE  objectid='{3}')", tableNameB, tableNameA, matchId,keepId,objectidA);
                }
                //sql = string.Format("INSERT INTO {0} (nationcode,nationelename,osmid,username,userid,versionid,starttime,fc,dsg,tags,shape,matchid,pointsid)(SELECT {1}.nationcode,{1}.nationelename,{1}.osmid,{1}.username,{1}.userid,{1}.versionid,{1}.starttime,{1}.fc,{1}.dsg,{1}.tags,{1}.shape,{2},{1}.pointsid FROM {1} WHERE  objectid='{3}')", tableNameB, tableNameA, matchId, objectidA);
                //sql = string.Format("INSERT INTO {0} (objectid,nationcode,nationelename,osmid,username,userid,versionid,starttime,fc,dsg,tags,shape)(SELECT {1},{2}.nationcode,{2}.nationelename,{2}.osmid,{2}.username,{2}.userid,{2}.versionid,{2}.starttime,{2}.fc,{2}.dsg,{2}.tags,{2}.shape FROM {2} WHERE  osmid='{3}')", tableNameB, objectidB, tableNameA, osmidA);
                //sql = string.Format("INSERT INTO {0} (objectid,nationcode,nationelename,osmid,username,userid,versionid,timestamps,fc,dsg,tags,shape) VALUES({1},{2},'{3}',{4},'{5}',{6},{7},'{8}','{9}','{10}','{11}',sdo_geometry ('{12}',4326))", tableNameB,objectidB,nationcodeB,nationelename,osmidB, usernameB, uid_B, versionB, timestampB, fc_B, dsg_B, tagB, geomB);
                using (OracleCommand cmd = new OracleCommand(sql, con))
                {               
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("成功插入objectid='{0}'的数据:objectid='{1}'",objectidA,objectidB);
                }
                if (combineData != null)
                {

 
                }

                string sqlA = string.Format("update {0} set matchid={1} where objectid={2}",tableNameA,matchId,objectidA);
                helper.sqlExecuteUnClose(sqlA);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        /// <summary>
        /// 面数据整体匹配更新
        /// </summary>
        /// <param name="bufferDataTable"></param>
        /// <param name="oscid_A"></param>
        /// <param name="tableNameA"></param>
        /// <param name="geometry_wktA"></param>
        /// <param name="Area_A"></param>
        /// <param name="tableNameB"></param>
        /// <param name="conString"></param>
        /// <param name="fc_A"></param>
        /// <param name="dsg_A"></param>
        /// <returns></returns>
        //public Boolean overallMatching(DataTable bufferDataTable, string oscid_A, string tableNameA, string geometry_wktA, double Area_A, string tableNameB, string conString, string fc_A, string dsg_A)
        //{
        //    Boolean matchSuccess = false;
        //    bufferDataTable = getDataByBuffer(tableNameB, geometry_wktA);

        //    try
        //    {
        //        Boolean flag = false;
        //        int bufferDataCount = bufferDataTable.Rows.Count;
        //        string updataOsmid = "";
        //        double MinDistance = 0;
        //        double foudDistance = double.MaxValue;

        //        if (bufferDataCount == 1)
        //        {

        //            string fc_B = bufferDataTable.Rows[0]["fc"].ToString();
        //            string dsg_B = bufferDataTable.Rows[0]["dsg"].ToString();
        //            string geomStr_wktB = bufferDataTable.Rows[0]["geometry_wkt"].ToString();
        //            string oscid_B = bufferDataTable.Rows[0]["oscid"].ToString();
        //            double Area_B = double.Parse(bufferDataTable.Rows[0]["area"].ToString());
        //            double overlayArea = double.Parse(bufferDataTable.Rows[0]["overlayArea"].ToString());
        //            string topologyRelation = overlapRatio(overlayArea, Area_A, Area_B);
        //            double overlayRatio = AreaRatio(overlayArea, Area_A, Area_B);

        //            string sql = "";
        //            OracleDBHelper conHelper = new OracleDBHelper();
        //            OracleConnection con = conHelper.getOracleConnection();
        //            if (con.State == ConnectionState.Closed)
        //            {
        //                con.Open();
        //            }
        //            if (topologyRelation == "重叠")
        //            {
        //                sql = string.Format("delete from {0} where oscid='{1}'", tableNameB, oscid_B);
        //            }
        //            if (topologyRelation == "包含")
        //            {
        //                if (fc_B == fc_A && dsg_B == dsg_A)
        //                {
        //                    sql = string.Format("delete from {0} where oscid='{1}'", tableNameB, oscid_B);
        //                }
        //                else
        //                {
        //                    if (overlayRatio > AreaTreshold)
        //                    {
        //                        sql = string.Format("delete from {0} where oscid='{1}'", tableNameB, oscid_B);
        //                    }
        //                    else
        //                    {
        //                        sql = string.Format("update {0} set shape=SDE_ST_GeomFromText(SDE.ST_Difference(sdo_geometry.get_wkt(shape) ,'{1}'),4326) where oscid='{2}'", tableNameB, geometry_wktA, oscid_B);
        //                    }
        //                }
        //            }
        //            else if (topologyRelation == "相交")
        //            {
        //                sql = string.Format("update {0} set shape=SDE_ST_GeomFromText(SDE.ST_Difference(sdo_geometry.get_wkt(shape) ,'{1}' ),4326) where oscid='{2}'", tableNameB, geometry_wktA, oscid_B);
        //            }
        //            using (OracleCommand com = new OracleCommand(sql, con))
        //            {
        //                com.ExecuteNonQuery();
        //            }
        //            dataRetain(oscid_A, tableNameA, tableNameB, conString);
        //            con.Close();
        //            updataCount++;
        //            Console.WriteLine("oscid为：" + oscid_B + "的面被id为:" + oscid_A + "整体更新");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //    return matchSuccess;
        //}
       
        /// <summary>
        /// 局部匹配更新
        /// </summary>
        /// <param name="bufferDataTable"></param>
        /// <param name="oscid_A"></param>
        /// <param name="version_A"></param>
        /// <param name="geometry_wktA"></param>
        /// <param name="Area_A"></param>
        /// <param name="tableNameA"></param>
        /// <param name="tableNameB"></param>
        /// <param name="conString"></param>
        /// <param name="fc_A"></param>
        /// <param name="dsg_A"></param>
        /// <returns></returns>
        public Boolean patialMatching(DataTable bufferDataTable, string objectidA, string geomeStr_wktA,double Area_A, string tableNameA, string tableNameB,string fc_A, string dsg_A,string changeType)
        {
            matchSuccess = false;
            //bufferDataTable = getDataByBuffer(tableNameB, geomeStr_wktA);
            double areaMax = 0;
            List<string> combineData = new List<string>();
            try
            {
                //Boolean flag = false;
                int bufferDataCount = bufferDataTable.Rows.Count;
                string updataOsmid = "";
                string topologyRelation = "";
                string osmid_B = "";
                string fc_B = "";
                string dsg_B = "";
                if (bufferDataCount > 1)                           
                {
                    //  areaMax = getMaxArea(bufferDataTable,tableNameB,geomeStr_wktA,Area_A);
                    for (int j = 0; j < bufferDataCount; j++)
                    {
                        fc_B = bufferDataTable.Rows[j]["fc"].ToString();
                        dsg_B = bufferDataTable.Rows[j]["dsg"].ToString();
                        string geomStr_wktB = bufferDataTable.Rows[j]["geometry_wkt"].ToString();
                        osmid_B = bufferDataTable.Rows[j]["osmid"].ToString();
                        double Area_B = double.Parse(bufferDataTable.Rows[j]["area"].ToString());
                        double overlayArea =0;
                        if (bufferDataTable.Rows[j]["overlayArea"].ToString() != "")
                        {
                            overlayArea = double.Parse(bufferDataTable.Rows[j]["overlayArea"].ToString());
                        }
                        else { continue; }
                        double areaRatio = AreaRatio(overlayArea, Area_A, Area_B);
                        double minareaRatio = minAreaRatio(overlayArea, Area_A, Area_B);
                        topologyRelation = overlapRatio(overlayArea, Area_A, Area_B);
                        double areaRatio_B = AreaRatio(overlayArea, Area_A, Area_B);
                        string objectidB = bufferDataTable.Rows[j]["objectid"].ToString();
                        string sql = "";
                        OracleConnection con = helper.getOracleConnection();
                        if (topologyRelation == "相交")
                        {
                            //areaRatio > AreaTreshold && fc_A == fc_B && dsg_A == dsg_B
                            if (minareaRatio > 0.8)
                            {
                                continue;
                            }
                            else
                            {
                                if (areaRatio > 0.8)
                                {
                                    sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                                    //if (fc_B == fc_A && dsg_B == dsg_A)
                                    //{
                                    //    sql = string.Format("update {0} set shape=ST_GeomFromText('{1}',3857) where osmid='{2}'", tableNameB, geomeStr_wktA, osmid_B);
                                    //}
                                    //else
                                    //{
                                    //    sql = string.Format("update {0} set fc='{1}',dsg='{2}',shape=ST_GeomFromText('{3}',3857) where osmid='{4}'", tableNameB, fc_A, dsg_A, geomeStr_wktA, osmid_B);
                                    //}

                                }
                                else
                                {
                                    sql = string.Format("update {0} set shape=SDO_GEOM.SDO_DIFFERENCE(shape,sdo_geometry ('{1}',4326),0.5),updatestate='{2}' where objectid='{3}'", tableNameB, geomeStr_wktA,modifyId,objectidB);
                                }
                                inputToHistoryDB(objectidB, tableNameB);
                            }


                        }
                        else if (topologyRelation == "包含")
                        {
                            if (fc_A == fc_B && dsg_A == dsg_B)
                            {
                                //dataRetain(osmid_A, tableNameA, tableNameB, conString);
                                sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);

                                //if (fc_B == fc_A && dsg_B == dsg_A)
                                //{
                                //    sql = string.Format("update {0} set shape=ST_GeomFromText('{1}',3857) where osmid='{2}'", tableNameB, geomeStr_wktA, osmid_B);
                                //}
                                //else
                                //{
                                //    sql = string.Format("update {0} set fc='{1}',dsg='{2}',shape=ST_GeomFromText('{3}',3857) where osmid='{4}'", tableNameB, fc_A, dsg_A, geomeStr_wktA, osmid_B);
                                //}

                            }
                            else
                            {
                                sql = string.Format("update {0} set shape=SDO_GEOM.SDO_DIFFERENCE(shape,sdo_geometry ('{1}',4326),0.5),updatestate='{2}' where objectid='{3}'", tableNameB, geomeStr_wktA,modifyId,objectidB);
                            }
                            inputToHistoryDB(objectidB, tableNameB);
                        }
                        else if (topologyRelation == "被包含")
                        {
                            //  dataRetain(osmid_A, version_A, tableNameA, tableNameB, conStr);

                            sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                            inputToHistoryDB(objectidB, tableNameB);
                        }
                        else if (topologyRelation == "重叠")
                        {
                            if (fc_B == fc_A && dsg_B == dsg_A)
                            {
                                sql = string.Format("update {0} set shape=SDO_UTIL.FROM_WKTGEOMETRY('{1}'),updatestate='{2}' where objectid='{3}'", tableNameB, geomeStr_wktA,modifyId,objectidB);
                            }
                            else
                            {
                                sql = string.Format("update {0} set fc='{1}',dsg='{2}',shape=SDO_UTIL.FROM_WKTGEOMETRY('{3}'),updatestate='{4}' where objectid='{5}'", tableNameB, fc_A, dsg_A, geomeStr_wktA,modifyId,objectidB);
                            }
                            inputToHistoryDB(objectidB, tableNameB);
                        }
                        else if (topologyRelation == "分离")
                        {

                            combineData.Add(objectidB);
                            continue;
                        }
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (OracleCommand command = new OracleCommand(sql, con))
                        {
                            command.ExecuteNonQuery();
                        }
                        con.Close();
                        updataCount++;
                        Console.WriteLine("osmid为：" + osmid_B + "的面被id为:" + objectidA + "局部更新");
                    }
                    dataRetain(objectidA, tableNameA, tableNameB,changeType,combineData);
                    matchSuccess = true;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return matchSuccess;
        }
        /// <summary>
        /// 整体匹配（空间相似性）
        /// </summary>
        /// <param name="bufferDataTable"></param>
        /// <param name="osmid_A"></param>
        /// <param name="version_A"></param>
        /// <param name="geomeStr_wktA"></param>
        /// <param name="Area_A"></param>
        /// <param name="tableNameA"></param>
        /// <param name="tableNameB"></param>
        /// <param name="conString"></param>
        /// <param name="fc_A"></param>
        /// <param name="dsg_A"></param>
        /// <param name="centerPointA"></param>
        /// <param name="changeType"></param>
        /// <returns></returns>
        public Boolean spatialMatchUpdata(DataTable bufferDataTable, string objectidA, string geomeStr_wktA, double Area_A, string tableNameA, string tableNameB, string fc_A, string dsg_A, string centerPointA, string changeType)
        {
            matchSuccess = false;
            //bufferDataTable = getDataByBuffer(tableNameB, geomeStr_wktA);
            OracleConnection con = helper.getOracleConnection();
            List<string> combineData = new List<string>();
           try
            {
                //Boolean flag = false;
                int bufferDataCount = bufferDataTable.Rows.Count;
                //string updataOsmid = "";
                //double MinDistance = 0;
                //double foudDistance = double.MaxValue;
                string osmid_B = "";
                string sql = "";
               
                if (bufferDataCount == 1)
                {
                    
                    string fc_B = bufferDataTable.Rows[0]["fc"].ToString();
                    string dsg_B = bufferDataTable.Rows[0]["dsg"].ToString();
                    string geomStr_wktB = bufferDataTable.Rows[0]["geometry_wkt"].ToString();
                    osmid_B = bufferDataTable.Rows[0]["osmid"].ToString();
                    double Area_B = double.Parse(bufferDataTable.Rows[0]["area"].ToString());
                    double overlayArea = double.Parse(bufferDataTable.Rows[0]["overlayArea"].ToString());
                    string topologyRelation = overlapRatio(overlayArea, Area_A, Area_B);
                    double areaRatio = AreaRatio(overlayArea, Area_A, Area_B);
                    string objectidB = bufferDataTable.Rows[0]["objectid"].ToString();
                    if (changeType == "delete" && areaRatio > 0.8)
                    {
                        sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                        matchSuccess = true;
                    }
                    else if (changeType == "delete" && areaRatio < 0.8)
                    {
                        matchSuccess = false;
                    }
                    else
                    {
                        if (topologyRelation == "重叠")
                        {
                            if (changeType == "delete")
                            {
                                sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                            }
                            else
                            {
                                dataRetain(objectidA, tableNameA, tableNameB,changeType,combineData);
                                sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                            }

                            // dataRetain(osmid_A, tableNameA, tableNameB, conString);
                        }
                        else if (topologyRelation == "包含")
                        {
                            if (changeType == "create"&& areaRatio>0.8)
                            {
                                dataRetain(objectidA, tableNameA, tableNameB, changeType,combineData);
                                sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                            }
                            else
                            {
                                if (fc_B == fc_A && dsg_B == dsg_A)
                                {
                                    dataRetain(objectidA, tableNameA, tableNameB, changeType,combineData);
                                    sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                                    // sql = string.Format("update {0} set shape=ST_GeomFromText('{1}',3857) where osmid='{2}'", tableNameB, geomeStr_wktA, osmid_B);
                                }
                                else
                                {
                                    if (areaRatio > AreaTreshold)
                                    {
                                        dataRetain(objectidA, tableNameA, tableNameB, changeType,combineData);
                                        sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                                    }
                                    else
                                    {
                                        sql = string.Format("update {0} set shape=SDO_GEOM.SDO_DIFFERENCE(shape,sdo_geometry ('{1}',4326),0.5),updatestate='{2}' where objectid='{3}'", tableNameB, geomeStr_wktA,modifyId,objectidB);
                                    }

                                    //  dataRetain(osmid_A, tableNameA, tableNameB, conString);
                                    //  sql = string.Format("update {0} set fc='{1}',dsg='{2}',shape=ST_GeomFromText('{3}',3857) where osmid='{4}'", tableNameB, fc_A, dsg_A, geomeStr_wktA, osmid_B);
                                }
                            }

                        }
                        else if (topologyRelation == "相交")
                        {
                            if (changeType == "create")
                            {
                                if (areaRatio > 0.8)
                                {
                                    sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                                    using (OracleCommand command = new OracleCommand(sql, con))
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                }
                                dataRetain(objectidA, tableNameA, tableNameB, changeType,combineData);
                            }
                            //else if (changeType == "delete")
                            //{
                            //    sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                            //}
                            else
                            {
                                if (areaRatio > AreaTreshold && fc_A == fc_B && dsg_A == dsg_B)
                                {
                                    sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);                                   
                                }
                                else
                                {
                                    sql = string.Format("update {0} set shape=SDO_GEOM.SDO_DIFFERENCE(shape,sdo_geometry ('{1}',4326),0.5),updatestate='{2}' where objectid='{3}'", tableNameB, geomeStr_wktA,modifyId,objectidB);
                                    //dataRetain(objectidA, tableNameA, tableNameB);
                                }
                                dataRetain(objectidA, tableNameA, tableNameB, changeType,combineData);
                            }

                        }
                        else if (topologyRelation == "被包含")
                        {
                            dataRetain(objectidA, tableNameA, tableNameB, changeType,combineData);
                            sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                        }
                        else if (topologyRelation == "分离")
                        {
                            dataRetain(objectidA, tableNameA, tableNameB, changeType,combineData);
                            sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                        }
                        matchSuccess = true;
                    }               
                    
                    if (changeType != "create"&& matchSuccess==true)
                    {
                        using (OracleCommand command = new OracleCommand(sql, con))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    con.Close();
                    updataCount++;
                    inputToHistoryDB(objectidB, tableNameB);
                    Console.WriteLine("osmid为：" + osmid_B + "的面被id为:" + objectidA + "更新");
                }
                else if (bufferDataCount > 1)
                {
                    for (int j = 0; j < bufferDataCount; j++)
                    {
                        string fc_B = bufferDataTable.Rows[j]["fc"].ToString();
                        string dsg_B = bufferDataTable.Rows[j]["dsg"].ToString();
                        string geomStr_wktB = bufferDataTable.Rows[j]["geometry_wkt"].ToString();
                        double overlayArea = 0;
                        double Area_B = double.Parse(bufferDataTable.Rows[j]["area"].ToString());
                        if (bufferDataTable.Rows[j]["overlayArea"].ToString()!="")
                        {  
                            overlayArea = double.Parse(bufferDataTable.Rows[j]["overlayArea"].ToString()); 
                        }
                        else { continue; }                       
                        string topologyRelation = overlapRatio(overlayArea, Area_A, Area_B);
                        double areaRatio = AreaRatio(overlayArea, Area_A, Area_B);
                        string objectidB = bufferDataTable.Rows[0]["objectid"].ToString();
                        string centerPointB = bufferDataTable.Rows[j]["centerPoint"].ToString();
                        osmid_B = bufferDataTable.Rows[j]["osmid"].ToString();
                        double distanceSimilar = DistanceSimilar(centerPointA, centerPointB, geomeStr_wktA, geomStr_wktB);

                        if (distanceSimilar < 0.001)
                        {
                            if (areaRatio > AreaTreshold)
                            {
                                dataRetain(objectidA, tableNameA, tableNameB, changeType,combineData);
                                if (topologyRelation == "重叠")
                                {

                                    sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                                }
                                else if (topologyRelation == "包含")
                                {

                                    if (fc_B == fc_A && dsg_B == dsg_A)
                                    {
                                        sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                                        // sql = string.Format("update {0} set shape=ST_GeomFromText('{1}',3857) where osmid='{2}'", tableNameB, geomeStr_wktA, osmid_B);
                                    }
                                    else
                                    {
                                        sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                                    }
                                }
                                else if (topologyRelation == "相交")
                                {

                                    sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                                }
                                else if (topologyRelation == "分离")
                                {
                                    sql = string.Format("delete from {0} where objectid='{1}'", tableNameB, objectidB);
                                }
                                matchSuccess = true;
                                using (OracleCommand command = new OracleCommand(sql, con))
                                {
                                    command.ExecuteNonQuery();
                                }
                                con.Close();
                                updataCount++;
                                inputToHistoryDB(objectidB, tableNameB);
                                Console.WriteLine("osmid为：" + osmid_B + "的面被id为:" + objectidA + "更新");
                            }
                        }
                    }
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return matchSuccess;

        }


        /// <summary>
        /// 面要素匹配更新
        /// </summary>
        /// <param name="tableNameA"></param>
        /// <param name="tableNameB"></param>
        /// <param name="conString"></param>
        public void matchUpdata(string tableNameA, string tableNameB)
        {
            try
            {
                DataTable tableA = getAllAreaData(tableNameA);
                int tableCount = tableA.Rows.Count;
                double Area_A = 0;
                List<string> combineData = new List<string>();
                for (int i = 0; i < tableCount; i++)
                {
                     this.progressBar1.Maximum = tableCount;//控制进度条的最大值
                    progressBar1.Value = i + 1;//更新进度条的值
                    string osmid_A = tableA.Rows[i]["osmid"].ToString();
                    int version = int.Parse(tableA.Rows[i]["versionid"].ToString());
                    string geometry_wktA = tableA.Rows[i]["geometry_wkt"].ToString();
                    string centerPointA = tableA.Rows[i]["centerPoint"].ToString();
                    string fc_A = tableA.Rows[i]["fc"].ToString();
                    string dsg_A = tableA.Rows[i]["dsg"].ToString();
                    string objectidA = tableA.Rows[i]["objectid"].ToString();
                
                    string changeType = tableA.Rows[i]["changetype"].ToString();
                    if (tableA.Rows[i]["area"].ToString() != "")
                    {
                        Area_A = double.Parse(tableA.Rows[i]["area"].ToString());
                    }
                    else { continue; }
                    //double Area_A = double.Parse(tableA.Rows[i]["area"].ToString());
                    tableNameB = areaEleBaseDataCBox.Text;
                    DataTable tableB = getDataByBuffer(tableNameB, geometry_wktA);
                    //DataTable tableNearby = getDataNearby(tableNameB, geometry_wktA);
                    if (tableB.Rows.Count!=0)
                    {

                        if (!spatialMatchUpdata(tableB,objectidA,geometry_wktA,Area_A, tableNameA, tableNameB,fc_A, dsg_A, centerPointA, changeType))
                        {
                            if (!patialMatching(tableB, objectidA, geometry_wktA, Area_A, tableNameA, tableNameB, fc_A, dsg_A,changeType))
                            {
                                dataRetain(objectidA, tableNameA, tableNameB,changeType,combineData);
                            }
                        }
                    }
                    else
                    {
                        if (changeType != "delete")
                        {
                            dataRetain(objectidA, tableNameA, tableNameB,changeType,combineData);
                        }
                        else {
                            continue;
                        }
                     
                    }
                    

                }
                Fun1();
                addmap.showMap(tableNameB, axMapControl1);
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }




        private void startUpdateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                updataCount = 0;
                string tableNameA = areaEleIncreDataCBox.Text;
                string tableNameB = areaEleBaseDataCBox.Text;
                createIndex(tableNameA);
                AreaTreshold = double.Parse(areaThresholdTBox.Text.ToString());
                hausdroffThreshold = 2;
                string conString = this.conStr;
                matchUpdata(tableNameA, tableNameB);

                //createIndex(tableNameB);
                MessageBox.Show("匹配更新完成！共计" + updataCount + "个面被更新");               
                //showOnMapControl(tableNameB);
                flashDisplay(tableNameB);
                //if (addmap.showMap(tableNameB, axMapControl1) == true)
                //{
                //    //flashDisplay();
                //}
                //else { MessageBox.Show("图层不能显示！！！"); }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #region 空间相似性计算

        /// <summary>
        /// 获取最大面积
        /// </summary>
        /// <param name="bufferDatatable"></param>
        /// <param name="tableNameB"></param>
        /// <param name="geometry_wktA"></param>
        /// <param name="Area_A"></param>
        /// <returns></returns>
        public double getMaxArea(DataTable bufferDatatable, string tableNameB, string geometry_wktA,double Area_A)
        {
            double maxArea = 0;
            bufferDatatable = getDataByBuffer(tableNameB, geometry_wktA);
            int bufferDataCount = bufferDatatable.Rows.Count;
            double[] AreaRatios = new Double[bufferDataCount];
            for (int i = 0; i < bufferDataCount; i++)
            {
                double Area_B = double.Parse(bufferDatatable.Rows[i]["area"].ToString());
                double overlayArea = double.Parse(bufferDatatable.Rows[i]["overlayArea"].ToString());
                double areaRatio = AreaRatio(overlayArea, Area_A, Area_B);
                AreaRatios[i] = areaRatio;//求两个面的面积重叠率                      
                if (AreaRatios[i] > maxArea)
                {
                    maxArea = AreaRatios[i];

                }
            }
            return maxArea;
        }

        /// <summary>
        /// 计算两个多边形的面积重叠率，并判断它们拓扑几何关系类型
        /// </summary>
        /// <param name="overlay"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public string overlapRatio(double overlay, double A, double B)
        {
            string TopologyRelation = "";
            double areaValue = overlay / Math.Max(A, B);//求面积重叠率，即A,B交集与面积最大值的比
            double areaValue1 = overlay / Math.Min(A, B);
            if (areaValue < 1 && areaValue1 == 1 && A < B)
            {
                TopologyRelation = "包含";
            }
            else if (areaValue < 1 && areaValue1 == 1 && A > B)
            {
                TopologyRelation = "被包含";
            }
            else if (areaValue > 0 && areaValue < 1 && areaValue1 < 1 && areaValue1 > 0)
            {
                TopologyRelation = "相交";
            }
            else if (areaValue == 1 && areaValue1 == 1)
            {
                TopologyRelation = "重叠";
            }
            else
            {
                TopologyRelation = "分离";
            }
            return TopologyRelation;
        }
        /// <summary>
        /// 计算两个面对象的面积重叠率
        /// </summary>
        /// <param name="overlay"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public double AreaRatio(double overlay, double A, double B)
        {
            double areaRatio = 0;
            areaRatio = overlay / Math.Max(A, B);
            return areaRatio;
        }

        public double minAreaRatio(double overlay, double A, double B)
        {
            double areaRatio = 0;
            areaRatio=overlay/Math.Min(A,B);
            return areaRatio;
        }
        /// <summary>
        /// 获取最大面积
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public double MaxArea(double A, double B)
        {
            double maxArea = 0;
            if (A > B)
            {
                maxArea = A;
            }
            else { maxArea = B; }
            return maxArea;
        }
        /// <summary>
        /// 计算两个点间的欧式距离
        /// </summary>
        /// <param name="Ax"></param>
        /// <param name="Ay"></param>
        /// <param name="Bx"></param>
        /// <param name="By"></param>
        /// <returns></returns>
        public double distance(double Ax, double Ay, double Bx, double By)
        {
            double distance = Math.Sqrt((Ax - Bx) * (Ax - Bx) + (Ay - By) * (Ay - By));
            return distance;
        }

        /// <summary>
        /// 距离相似性
        /// </summary>
        /// <param name="centerPointStringA"></param>
        /// <param name="centerPointStringB"></param>
        public double  DistanceSimilar(string centerPointStringA, string centerPointStringB, string pointStringA, string pointStringB)
        {       
            string[] pointCoordinateA = centerPointStringA.Substring(7, centerPointStringA.Length -8).Split(' ');
            string[] pointCoordinateB = centerPointStringB.Substring(7, centerPointStringB.Length -8).Split(' ');
            double pointAx = double.Parse(pointCoordinateA[0].ToString());
            double pointAy = double.Parse(pointCoordinateA[1].ToString());
            double pointBx = double.Parse(pointCoordinateB[0].ToString());
            double pointBy = double.Parse(pointCoordinateB[1].ToString());
            double centerDistance=distance(pointAx,pointAy,pointBx,pointBy);
            double distanceSimilar = 1 - longHalShaftHalfLength(pointStringA, pointStringB) / (longHalShaftHalfLength(pointStringA, pointStringB) + centerDistance);
            return distanceSimilar;
        }
      /// <summary>
      /// 获取半轴长的一半
      /// </summary>
      /// <param name="pointStringA"></param>
      /// <param name="pointStringB"></param>
      /// <returns></returns>
        public double longHalShaftHalfLength(string pointStringA, string pointStringB)
        {
           double length= (longHalfShaftLength(pointStringA)+longHalfShaftLength(pointStringB))/2;
           return length;
        }
      /// <summary>
      /// 计算最小外包矩形长半轴的长
      /// </summary>
      /// <param name="pointString"></param>
      /// <returns></returns>
        public double longHalfShaftLength(string pointString)
        {
            string match1 = "";
            Regex rg = new Regex(@"\(.*?\)");
            Match match = rg.Match(pointString);
            match1 = match.Value.Replace("((", " ");
            match1 = match1.Replace("(","");
            string match2= match1.Replace(")"," ");
            string pointStr = match2;
            string pointStrs = pointStr.Substring(1, pointStr.Length -1);
           
            string[] points = pointStrs.Split(',');

            string[] pointCoordinate = points[0].Split(' ');
            double minPointX = double.Parse(pointCoordinate[0].ToString());
            double minPointY = double.Parse(pointCoordinate[1].ToString());
            double maxPointX = double.Parse(pointCoordinate[0].ToString());
            double maxPointY = double.Parse(pointCoordinate[1].ToString());

            for (int i = 0; i < points.Length-1; i++)
            {
                string[] pointCoordinates = points[i+1].Split(' ');
                double pontx = double.Parse(pointCoordinates[1].ToString());
                if (pontx < minPointX)
                {
                    minPointX = pontx;
                }
                else if (pontx > minPointX)
                {
                    maxPointX = pontx;
                }
                double pointy = double.Parse(pointCoordinates[2].ToString());
                if (pointy < minPointX)
                {
                    minPointY = pointy;
                }
                else if (pointy > maxPointY)
                {
                    maxPointY = pointy;
                }
            }
            double longhalfshaftlength = distance(minPointX, minPointY, maxPointX, maxPointY) / 2;
            return longhalfshaftlength;
        }
       #endregion

        #region 边界调整
        //public void BoundaryUpdate(List<string> combinData,string objectIdA,string tableB,string objectidB)
        //{
        //    double distance = 0;
        //    double mainArea = 0;
        //    double searchArea = 0;
        //    if (combinData != null)
        //    {
        //        for (int i = 0; i < combinData.Count; i++)
        //        {
        //            if (nearestDistance(tableB,objectIdA,objectidB) < 3)
        //            {
 
        //            }
        //        }
        //    }
        //}
        ///// <summary>
        ///// 计算两个对象之间的最近距离
        ///// </summary>
        ///// <param name="mainTable"> 表1</param>
        ///// <param name="mainObjectID">表1中的ObjectID</param>
        ///// <param name="searchTable">表2</param>
        ///// <param name="searchObjectid">表2中的ObjectID</param>
        //public double nearestDistance(string tableB,string objectidA,string objectidB)
        //{
        //    string sql = string.Format("select SDO_GEOM.SDO_DISTANCE((select shape from {0} where objectid={1}),(select shape from {0} where objectid={2}),0.05,'UNIT=M') from daul", tableB,objectidA,objectidB);
        //    double distance = 0;
        //    using (OracleDataReader dr = helper.queryReader(sql))
        //    {
        //        while (dr.Read())
        //        {
        //            distance = double.Parse(dr[0].ToString());
        //        }
        //    }
        //    //using (OracleDataReader drdis = conHelper.queryReader(SQL))
        //    //    if (drdis.Read() && drdis[0] != DBNull.Value)
        //    //    {
        //    //        distance = Convert.ToDouble(drdis.GetFloat(0));
        //    //    }
        //    return distance;
        //}


     #endregion
        #region 显示或索引
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="tableName"></param>
        public void createIndex(string tableName)
        {

            string sql1 = "";
            string sqlInset = "";
            string sqlCreate = "";
      
            if (helper.IsExistSpatialTable(tableName))
            {
                sqlCreate = string.Format("create index idx_{0} on {0}(shape) indextype is mdsys.spatial_index", tableName);
            }
            else
            {
                sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('{0}', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", tableName);
                helper.sqlExecuteUnClose(sqlInset); 
                sqlCreate = string.Format("create index idx_{0} on {0}(shape) indextype is mdsys.spatial_index", tableName);             

            }            
            helper.sqlExecuteUnClose(sqlCreate);
        }
       /// <summary>
       /// 将数据插入历史数据库
       /// </summary>
       /// <param name="objectidA"></param>
       /// <param name="tableNameA"></param>
        public void inputToHistoryDB(string objectidA, string tableNameA)
        {
            try
            {
                //字段定义
                ////int osmidB = 0;
                //int objectidB = 0;
                string sql = "";
                OracleConnection con = helper.getOracleConnection();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

          
                string tableName = "history" + tableNameA;
                sql = string.Format("INSERT INTO {0} (nationcode,nationelename,osmid,username,userid,versionid,starttime,endtime,fc,dsg,tags,shape,source)(SELECT {1}.nationcode,{1}.nationelename,{1}.osmid,{1}.username,{1}.userid,{1}.versionid,{1}.starttime,to_char(sysdate,'YYYY-MM-DD HH24:MI:SS'),{1}.fc,{1}.dsg,{1}.tags,{1}.shape,{1}.source FROM {1} WHERE  objectid='{2}')",tableName,tableNameA, objectidA);
                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("成功插入数据:objectid='{0}'，到历史数据库：{1}", objectidA,tableName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// 显示更新后的图在mapcontrol上
        /// </summary>
        /// <param name="tableName"></param>
        //public void showOnMapControl(string tableName)
        //{

        //    string server = OSMDataBaseLinkForm.Server_;
        //    string user = OSMDataBaseLinkForm.User_;
        //    string password = OSMDataBaseLinkForm.Password_;
        //    string database = OSMDataBaseLinkForm.DataBase_;

        //    IPropertySet pPropset = new PropertySet();//创建一个属性设置对象
        //    IWorkspaceFactory pWorkspaceFact = new SdeWorkspaceFactory();//创建一个空间数据引擎工作空间工厂
        //    pPropset.SetProperty("server", server);
        //    // propertySet.SetProperty("INSTANCE", Instance.Text );//如果没有设置INSTANCE属性，会有连接窗体弹出  
        //    pPropset.SetProperty("INSTANCE", "sde:oracle11g:" + server + "/" + database);// by 丁洋修改
        //    pPropset.SetProperty("database", database);
        //    pPropset.SetProperty("user", user);
        //    pPropset.SetProperty("password", password);
        //    pPropset.SetProperty("version", "SDE.DEFAULT");
        //    workspace = pWorkspaceFact.Open(pPropset, 0);//使用属性集来打开地理数据库

        //    //直接从sde中读取数据.  
        //    if (tableName != "")//如果要素图层的文本不为空
        //    {
        //        IEnumDataset enumDataset;//枚举类型，装数据集的
        //        IDataset dataset;
        //        pFeatureLayer = new FeatureLayerClass();//新建一个要素图层对象
        //        //获取图层名
        //        enumDataset = workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);//使用工作空间获取数据库中的要素集，放入枚举对象中
        //        dataset = enumDataset.Next();//在枚举对象内部查找下一个数据
        //        string tablename = "SYSTEM." + tableName;
        //        while (dataset != null)//当存在数据时
        //        {
        //            if (dataset.Name == tablename)//判断和要素层是否匹配
        //            {
        //                pFeatureLayer.FeatureClass = dataset as IFeatureClass;//如果数据集中的数据和要素图层匹配成功，那么将数据集转换为要素类
                    
        //                break;
        //            }
        //            dataset = enumDataset.Next();//循环遍历要素集
        //        }
        //        string temp;
        //        temp = pFeatureLayer.FeatureClass.AliasName;//用于接收要素类的名字，赋给temp进行处理
        //        int i = temp.IndexOf('.');//找到.的下标
        //        temp = temp.Substring(i + 1);//从.开始截取，substring是从i+1开始截取的，也包括i+1
        //        temp = temp.ToLower();//把大写转小写
        //        pFeatureLayer.Name = temp;//最终把处理好的名字赋给图层名
        //        IActiveView pActiveView = this.axMapControl1.ActiveView;

        //        this.axMapControl1.Map.AddLayer(pFeatureLayer); //在底图中增添图层，该图层增添的对象为数据集转换为要素类的对象 
        //        axMapControl1.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, pFeatureLayer, null);
        //        IEnvelope envelope = this.axMapControl1.FullExtent;//获取到地图的全图范围
        //        IPoint centerPoint = new PointClass();
        //        envelope.Height = 2 * pActiveView.Extent.Height;
        //        String report2 = "Re-cetered Envelope: \n" +
        //                         "LowerLeft  X = " + envelope.LowerLeft.X + "\n" +
        //                         "LowerLeft  Y = " + envelope.LowerLeft.Y + "\n\n" +
        //                         "LowerRight X =  " + envelope.LowerRight.X + "\n" +
        //                         "LowerRight Y =  " + envelope.LowerRight.Y + "\n\n" +
        //                         "UpperLeft  X = " + envelope.UpperLeft.X + "\n" +
        //                         "UpperLeft  Y = " + envelope.UpperLeft.Y + "\n\n" +
        //                         "UpperRight X =  " + envelope.UpperRight.X + "\n" +
        //                         "UpperRight Y =  " + envelope.UpperRight.Y; 
        //        //System.Windows.Forms.MessageBox.Show(report2);
        //        envelope.Expand(0.5, 0.5, true);//放大缩放比例
        //        this.axMapControl1.ActiveView.Extent = envelope;
        //        axMapControl1.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, pFeatureLayer, null);
        //        MessageBox.Show("加载成功");
        //        this.Close();

        //    }
        //}

        private void ThreadFun()
        {
            //线程操作
            //入库完成之后，清空缓存的文件byZYH
            string path = @"..\..\..\testfile\";
            //string path = @"..\..\" + tableName + ".shp\testfile\"";
            //string path = @"~/testfile/"+tableName+".shp";
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
        //需要启动线程的方法
        private void Fun1()
        {
            Thread m_thread = new Thread(ThreadFun);
            m_thread.Start();
        }

        //}
        public void FlashShow()
        {
            string sql = "";

        }
        public void flashDisplay(string tableName)
        {
            IMap pMap =axMapControl1.Map;
            List<string> layerNames = new List<string>();
            ArrayList countlist = new ArrayList();
            countlist = getUpdateAfterObjectId(tableName);
            OperateMap player = new OperateMap();
            IFeatureLayer pFeatLyr = player.GetFeatLyrByName(axMapControl1.Map, tableName);
            addmap.Flash(axMapControl1, pFeatLyr, "objectid", "=", countlist);
            addmap.showByAttibute(pFeatLyr, "objectid", "=", countlist, 255, 0, 0);
            axMapControl1.Refresh();
        }





        //public void flashDisplay( )
        //{
        //    IMap pMap = axMapControl1.Map;
        //    List<string> layerNames = new List<string>();
        //    for (int i = 0; i < pMap.LayerCount; i++)
        //    {
        //        string namelist = this.axMapControl1.Map.get_Layer(i).Name;
        //        layerNames.Add(namelist);
        //    }
        //    if (layerNames.Count != 0)
        //    {
        //        for (int j = 0; j < layerNames.Count; j++)
        //        {
        //            string sql = string.Format("select * from {0} where ", layerNames[j],j);
        //            addmap.FlashShow(sql,layerNames[j]);
        //            //FlashLine(sql, layerNames[j]);
        //        }
        //    }
        //    else { MessageBox.Show("OSM数据库尚未连接，请先连接数据库！"); }
        //}
        #endregion
    }
}
