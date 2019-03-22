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
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;


/*OSM交通线要素匹配更新    by zbl 2018.7.13*/
namespace GIS.UI.Forms
{
    public partial class LineElementMatch : Form
    {
        string conStr = "";//数据库连接字符串
        int updateCount = 0;//更新线数据量字段

        double distanceThreshold = 0;//距离阈值
        double bufferRadius = 0;//缓冲区搜索半径
        double hausdorffThreshold = 0;//hausdorff整体匹配阈值
        double Hausdorff2Threshold = 0;//hausdorff局部匹配阈值
        double angleThreshold = 0;//局部匹配两直线夹角阈值
        //string temporaryLineUpData = "temporarylineUpData";//存储更新后的线数据临时表

        public LineElementMatch()
        {
            InitializeComponent();
        }
        public LineElementMatch(string conStr)
        {
            InitializeComponent();
            this.conStr = conStr;
            this.progressBar1.Minimum = 0;
        }
        private AxMapControl axMapControl1;//这是字段
        IFeatureLayer pFeatureLayer;//这是属性
        IWorkspace workspace;//创建工作空间对象

        public AxMapControl Axmapcontrol
        {
            get { return axMapControl1; }
            set { axMapControl1 = value; }
        }

        //道路匹配更新容器触发事件 
        private void TrafficUpdategBox_Enter(object sender, EventArgs e)
        {
            List<string> basicTables = getTableNames(conStr);//获取数据库表名
            this.basicDataCOMB.Items.Clear();
            this.zhuanyeDataCOMbB.Items.Clear();
            if (basicTables != null && basicTables.Count > 0)
            {
                foreach (string name in basicTables)//遍历数据库中的基态数据表
                {
                    this.basicDataCOMB.Items.Add(name);
                    this.zhuanyeDataCOMbB.Items.Add(name);
                }
            }
            else
            {
                MessageBox.Show("对不起，您选择的数据库中无道路数据表！");
            }
        }  
        /// <summary>
        /// 获取所有的表名
        /// </summary>
        /// <param name="conString">数据库连接字符串</param>
        /// <returns>所有的表名</returns>
        private List<string> getTableNames(string conString)
        {
            List<string> baseTables = new List<string>();//系统自带的数据表
            List<string> tableNames = new List<string>();
            try
            {
                OracleDBHelper conHelper = new OracleDBHelper();//获取数据库帮助对象
                OracleConnection con = conHelper.getOracleConnection();
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
                string sqlString = "select distinct table_name  from user_col_comments where lower(table_name) like '%line%' or  table_name like '%currentlinedata%'or table_name like '%incrementlinedata%'or table_name like '%atraffic%'or table_name like '%awater%'or table_name like '%avegetation%'or table_name like '%asoil%'or table_name like '%aresidential%'order by  lower(table_name) ASC";
                using (OracleCommand command = new OracleCommand(sqlString, con))
                {
                    OracleDataReader dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        string name = dr[0].ToString();
                        if (baseTables.Contains(name))
                        {
                            continue;
                        }
                        else
                        {
                            tableNames.Add(name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return tableNames;
        }
        ///// <summary>
        ///// 创建存储匹配更新后的线数据临时表
        ///// </summary>
        //public void CreateTemLineUPData()
        //{
        //    OracleDBHelper creHelper = new OracleDBHelper();
        //    OracleConnection con = creHelper.getOracleConnection();//Oracle数据库连接
        //    if (con.State == ConnectionState.Closed)
        //    {
        //        con.Open();
        //    }
        //    if (creHelper.IsExistTable("temporaryLineUpData"))//判断当前数据库中是否存在该临时线数据表
        //    {
        //        string sql = string.Format("drop table temporaryLineUpData");//若存在将其删除
        //        creHelper.sqlExecuteUnClose(sql);
        //    }
        //    string createTableSql = "(osm_id NUMBER(30) ,version_id NUMBER(30),times VARCHAR2(30),osm_key VARCHAR2(50),osm_value VARCHAR2(50),shape SDO_GEOMETRY,nationcode VARCHAR2(50),nationEleName VARCHAR2(50),elementType VARCHAR2(50),elementTypeID NUMBER(30))";
        //    creHelper.createTable(temporaryLineUpData, createTableSql);
        //} 

        /// <summary>
        /// 通过坐标和方向计算两直线向量之间的角度
        /// </summary>
        public double strokeLineAngleJudgeByCoordinate(double startAx, double startAy, double endAx, double endAy, double startBx, double startBy, double endBx, double endBy)
        {
            //vector_a = (endAx - startAx,endAy-startAy);
            //vector_b = (endBx - startBx,endBy-startBy);
            double vector_ax = endAx - startAx;
            double vector_ay = endAy - startAy;

            double vector_bx = endBx - startBx;
            double vector_by = endBy - startBy;

            double vector_aXvector_b = (vector_ax * vector_bx + vector_ay * vector_by);
            double vector_aLength = Math.Sqrt(Math.Abs(vector_ax * vector_ax) + Math.Abs(vector_ay * vector_ay));
            double vector_bLength = Math.Sqrt(Math.Abs(vector_bx * vector_bx) + Math.Abs(vector_by * vector_by));
            double cosθ = vector_aXvector_b / (vector_aLength * vector_bLength);

            return (Math.Acos(cosθ)) * 180 / Math.PI;//返回的是弧度值，需要乘以180/Math.PI转为角度

        }
        /// <summary>
        /// 通过缓冲区半径和对象geom得到缓冲区内对象
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="geometry_wkt"></param>
        /// <returns></returns>
        public DataTable getDataByBuffer(string tableName, string geometry_wkt)
        {
            DataTable bufferData = new DataTable();//读取某个线对象缓冲区内所有对象的数据集表
            OracleDBHelper helper = new OracleDBHelper();
            try
            {
                string sql = "";
                if (geometry_wkt.Length > 4000)
                {
                    return bufferData;
                }
                else
                {
                    //string sql1 = string.Format("insert into {0} (osm_id, version_id, times, osm_key, osm_value, shape, nationcode, nationelename, elementtype, elementtypeid)(select osm_id,version_id,times,osm_key,osm_value,shape,nationCode,nationEleName,elementType,elementTypeID from {1})",temporaryLineUpData,tableName);
                    //helper.sqlExecute(sql1);
                    sql = tableName.Equals("CURRENTLINEDATA") ? string.Format("select osm_id,version_id,objectid,times,NationEleName,nationcode,elementTypeID,sdo_geometry.get_wkt(shape)geometry_wkt from {0} where SDO_WITHIN_DISTANCE(shape,sdo_geometry ('{1}',4326) ,'distance={2},unit=m')='TRUE'", tableName, geometry_wkt, bufferRadius)
                        : string.Format("select osm_id,NationEleName, sdo_geometry.get_wkt(shape)geometry_wkt from {0} where SDO_WITHIN_DISTANCE(geom,sdo_geometry ('{1}',4326)) ,'distance={2},unit=m')='TRUE'", tableName, geometry_wkt, bufferRadius);//得到缓冲区内对象
                    //也可以用ST_Touches()函数代替ST_DWithin()判断两个几何对象的边缘是否接触 ST_Touches(geometry, geometry)

                    bufferData.Columns.Add("objectid");
                    bufferData.Columns.Add("osm_id");
                    bufferData.Columns.Add("version_id");
                    bufferData.Columns.Add("times");
                    //bufferData.Columns.Add("osm_key");
                    //bufferData.Columns.Add("osm_value");
                    bufferData.Columns.Add("NationEleName");
                    bufferData.Columns.Add("nationCode");
                    //bufferData.Columns.Add("elementType");
                    bufferData.Columns.Add("elementTypeID");
                    bufferData.Columns.Add("geometry_wkt");//可能出错的wkt//当geometry_wkt中的linestring长度超过4000时会报错：‘ora01704：字符串过长’的错误
                    using (OracleDataReader rd = helper.queryReader(sql))
                    {
                        while (rd.Read())
                        {
                            DataRow bufferDataRow = bufferData.NewRow();
                            if (tableName.Equals("CURRENTLINEDATA"))
                            {
                                bufferDataRow["osm_id"] = rd["osm_id"].ToString();
                                bufferDataRow["version_id"] = rd["version_id"].ToString();
                                bufferDataRow["objectid"] = rd["objectid"].ToString();
                                bufferDataRow["times"] = rd["times"].ToString();
                                //bufferDataRow["osm_key"] = rd["osm_key"].ToString();
                                //bufferDataRow["osm_value"] = rd["osm_value"].ToString();
                            }
                            else
                            {
                                bufferDataRow["objectid"] = rd["objectid"].ToString();
                                bufferDataRow["osm_id"] = rd["osm_id"].ToString();
                            }
                            bufferDataRow["NationEleName"] = rd["NationEleName"].ToString();
                            bufferDataRow["nationCode"] = rd["nationCode"].ToString();
                            //bufferDataRow["elementType"] = rd["elementType"].ToString();
                            bufferDataRow["elementTypeID"] = rd["elementTypeID"].ToString();
                            bufferDataRow["geometry_wkt"] = rd["geometry_wkt"].ToString();
                            bufferData.Rows.Add(bufferDataRow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return bufferData;
        }
        /// <summary>
        /// 通过数据库表名获得增量数据表内所有对象
        /// 遍历专业矢量来源的增量数据表获得增量数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable getDataAll(string tableName)
        {
            string sql = "";
            DataTable table = new DataTable();
            try
            {
                OracleDBHelper helper = new OracleDBHelper();
                if ("INCREMENTLINEDATA".Equals(tableName))
                {
                    sql = string.Format("select osm_id,version_id,times,nationcode,NationEleName,elementTypeID,sdo_geometry.get_wkt(shape)geometry_wkt from {0} order by osm_id,version_id ", tableName);
                    //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('174857072','563632939','55414786') order by osmid,version ", tableName);//测试整体匹配部分
                    //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('562259740','562216181','562787930','563632939') order by osmid,version ", tableName);//测试局部匹配部分
                    //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('562787930') order by osmid,version ", tableName);//测试局部匹配部分_包含或被包含完全替代情况_            成功！
                    //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('92361574') order by osmid,version ", tableName);//测试局部匹配部分_头顶头没有共同部分情况                  成功！
                    //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('203711643') order by osmid,version ", tableName);//测试局部匹配部分_错位情况           当整体匹配hausdorff距离阈值为1，局部匹配hausdorff距离阈值为5时，局部替换整体阈值大于4时整体匹配成功！
                    //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('357846417') order by osmid,version ", tableName);//测试局部匹配部分_错位情况                                成功！
                    //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('329760827') order by osmid,version ", tableName);//测试局部匹配部分_一方有拐点的情况

                    table.Columns.Add("osm_id");
                    table.Columns.Add("version_id");
                    table.Columns.Add("times");
                    //table.Columns.Add("osm_key");
                    //table.Columns.Add("osm_value");
                    table.Columns.Add("nationCode");
                    table.Columns.Add("NationEleName");
                    //table.Columns.Add("elementType");
                    table.Columns.Add("elementTypeID");
                    table.Columns.Add("geometry_wkt");
                }
                #region 废弃（如果是专业矢量数据读取下面sql语句）
                else
                {
                    //sql = string.Format("select objectid,roadname,sdo_geometry.get_wkt(shape)geometry_wkt from {0} order by objectid ", tableName);
                    sql = string.Format("select roadname,sdo_geometry.get_wkt(shape)geometry_wkt from {0}  ", tableName);
                    //table.Columns.Add("objectid");
                    table.Columns.Add("roadname");
                    table.Columns.Add("geometry_wkt");
                }
                #endregion
                using (OracleDataReader rd = helper.queryReader(sql))
                {
                    while (rd.Read())
                    {
                        DataRow tableRow = table.NewRow();
                        if ("INCREMENTLINEDATA".Equals(tableName))
                        {
                            tableRow["osm_id"] = rd["osm_id"].ToString();
                            tableRow["version_id"] = rd["version_id"].ToString();
                            tableRow["times"] = rd["times"].ToString();
                            //tableRow["osm_key"] = rd["osm_key"].ToString();
                            //tableRow["osm_value"] = rd["osm_value"].ToString();
                            tableRow["nationCode"] = rd["nationCode"].ToString();
                            tableRow["NationEleName"] = rd["NationEleName"].ToString();
                            //tableRow["elementType"] = rd["elementType"].ToString();
                            tableRow["elementTypeID"] = rd["elementTypeID"].ToString();
                            tableRow["geometry_wkt"] = rd["geometry_wkt"].ToString();
                        }
                        //else
                        //{
                        //    //tableRow["objectid"] = rd["objectid"].ToString();
                        //    tableRow["roadname"] = rd["roadname"].ToString();
                        //    tableRow["geometry_wkt"] = rd["geometry_wkt"].ToString();
                        //}
                        table.Rows.Add(tableRow);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return table;
        }

        /// <summary>
        /// 计算两个点集合的hausdorff距离,公式：
        /// H(A,B)=max(h(A,B),h(B,A))
        /// h(A,B)=max min||a-b||
        /// h(B,A)=max min||b-a||
        /// </summary>
        /// <param name="pointStringA"></param>
        /// <param name="pointStringB"></param>
        /// <returns></returns>
        public double hausdorffDistanceOneWay(string pointStringA, string pointStringB)
        {
            double hA_B = hausdorffDistance(pointStringA, pointStringB);
            double hB_A = hausdorffDistance(pointStringB, pointStringA);
            return Math.Max(hA_B, hB_A);
        }
        /// <summary>
        /// 计算两个点集合的单项hausdorff距离
        /// </summary>
        /// <param name="pointStringA"></param>
        /// <param name="pointStringB"></param>
        /// <returns></returns>
        public double hausdorffDistance(string pointStringA, string pointStringB)
        {
            List<double> minABDistanceList = new List<double>();
            try
            {
                string pointStrA = pointStringA.Substring(12, pointStringA.Length - 13);
                pointStrA = pointStrA.Trim();
                string[] pointsA = pointStrA.Split(',');

                string pointsStrB = pointStringB.Substring(12, pointStringB.Length - 13);
                pointsStrB = pointsStrB.Trim();
                string[] pointsB = pointsStrB.Split(',');

                if (pointStrA.Length <= 0 || pointsStrB.Length <= 0)//如果有一方点不存在，即返回
                {
                    return 1000000;
                }

                for (int i = 0; i < pointsA.Length; i++)
                {
                    pointsA[i] = pointsA[i].Trim();
                    string[] pointCoordinateA = pointsA[i].Split(' '); ;
                    double pointAx = double.Parse(pointCoordinateA[0].ToString());
                    double pointAy = double.Parse(pointCoordinateA[1].ToString());
                    List<double> ABDistancelist = new List<double>();
                    for (int j = 0; j < pointsB.Length; j++)
                    {
                        pointsB[j] = pointsB[j].Trim();
                        string[] pointCoordinateB = pointsB[j].Split(' '); ;
                        double pointBx = double.Parse(pointCoordinateB[0].ToString());
                        double pointBy = double.Parse(pointCoordinateB[1].ToString());
                        ABDistancelist.Add(distance(pointAx, pointAy, pointBx, pointBy));
                    }
                    minABDistanceList.Add(ABDistancelist.Min());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return minABDistanceList.Max();
        }
        /// <summary>
        /// 计算两个点之间的欧氏距离
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
        /// 线要素匹配
        /// </summary>
        /// <param name="tableNameA">专业矢量增量数据</param>
        /// <param name="tableNameB">基态数据</param>
        /// <param name="conStringA"></param>
        /// <param name="conStringB"></param>
        public void LineMatching(string tableNameA, string tableNameB, string conStringA, string conStringB)
        {
            try
            {
                DataTable tableA = getDataAll(tableNameA);//得到增量数据表中的匹配更新列数据
                int tableCount = tableA.Rows.Count;//增量数据表中行数据统计
                for (int i = 0; i < tableCount; i++)//遍历循环表中数据
                {
                    this.progressBar1.Maximum = tableCount;//控制进度条的最大值
                    progressBar1.Value = i+1;//更新进度条的值
                    string osmid_V = tableA.Rows[i]["osm_id"].ToString() + "_" + tableA.Rows[i]["version_id"].ToString();//确定一个线对象
                    string geomStr_wktA = tableA.Rows[i]["geometry_wkt"].ToString();
                    string timesA=tableA.Rows[i]["times"].ToString ();
                    string nationCodeA=tableA.Rows[i]["nationCode"].ToString();
                    string nationEleNameA = tableA.Rows[i]["NationEleName"].ToString();
                    string elementTypeIDA=tableA.Rows [i]["elementTypeID"].ToString ();
                    tableNameB = basicDataCOMB.Text;//读取基态数据表名
                    DataTable tableB = getDataByBuffer(tableNameB, geomStr_wktA);//在基态数据库中搜索增量数据对象缓冲区范围内的所有对象数据
                    if (tableB.Rows.Count > 0)
                    {
                        if (!overallMatching(tableB, osmid_V, geomStr_wktA, nationEleNameA, conStringB, tableNameB))//整体匹配没有成功，则进行局部匹配
                        {
                            if (!partialMatch(tableB, osmid_V, geomStr_wktA, nationEleNameA, conStringB, tableNameB))
                            { //如果局部匹配也没成功，则说明基态数据中不存在与该对象可以匹配的对象，将其保留                               

                                dataRetain(tableA.Rows[i]["osm_id"].ToString(), int.Parse(tableA.Rows[i]["version_id"].ToString()), tableNameA, tableNameB, conStringA, conStringB);
                            }
                        }
                    }
                    else
                    { //说明缓冲区内没有找到可能与该对象相匹配的对象，则该对象保留

                        dataRetain(tableA.Rows[i]["osm_id"].ToString(), int.Parse(tableA.Rows[i]["version_id"].ToString()), tableNameA, tableNameB, conStringA, conStringB);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 缓冲区内没有找到可能与A对象相匹配的B对象，则该对象保留
        /// </summary>
        /// <param name="osmid"></param>
        /// <param name="version_id"></param>
        /// <param name="tableNameA"></param>
        /// <param name="tableNameB"></param>
        /// <param name="conStringA"></param>
        /// <param name="conStringB"></param>
        public void dataRetain(string osm_id, int version_id, string tableNameA, string tableNameB, string conStringA, string conStringB)
        {
            try
            {
                int objectidB = 0, osmidB = 0, useridB = 0,versionidB = 0,elementTypeIDB=0;
                string usernameB = "", timesB = "", osmkeyB = "", osmvalueB = "", geomB = "", changesetB = "";
                string nationEleNameB = "", nationCodeB = "";
                string sql = string.Format("select osm_id,user_name,user_id,version_id,times,osm_key,osm_value,nationELeName,nationCode,elementTypeID,changeset,sdo_geometry.get_wkt(shape)geometry_wkt from {0} where osm_id='{1}' and version_id ={2} ", tableNameA, osm_id, version_id);
                OracleDBHelper conhelper = new OracleDBHelper();//获取数据库帮助对象以连接数据库
                OracleConnection conn = conhelper.getOracleConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    using (OracleDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            osmidB = int.Parse(rd["osm_id"].ToString());
                            useridB = int.Parse(rd["user_id"].ToString());
                            versionidB = int.Parse(rd["version_id"].ToString());
                            elementTypeIDB = int.Parse(rd["elementTypeID"].ToString());
                            usernameB = rd["user_name"].ToString();
                            timesB = rd["times"].ToString();//to_timestamp('2010-10-01 21:30:50', 'YYYY-MM-DD HH24:MI:SS')sql
                            osmkeyB = rd["osm_key"].ToString();
                            osmvalueB=rd["osm_value"].ToString();
                            geomB = rd["geometry_wkt"].ToString();
                            nationEleNameB = rd["nationEleName"].ToString();
                            nationCodeB = rd["nationCode"].ToString();
                            changesetB = rd["changeset"].ToString();
                        }
                    }
                }
                conn .Close();
                sql = string.Format("SELECT objectid FROM  {0} ORDER BY {0}.objectid DESC", tableNameB);
                //sql = string.Format("SELECT * FROM  {0} ORDER BY {0}.osm_id DESC ", tableNameB);
                if (conn.State == ConnectionState.Closed)
                {   
                    conn.Open();
                }
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    //objectidB = int.Parse(cmd.ExecuteScalar().ToString());
                    objectidB =Convert.ToInt32  (cmd.ExecuteScalar());
                }
                objectidB++;
                if (geomB.Length > 4000)
                {
                    sql = string.Format("insert into {0} (objectid,osm_id,user_id,user_name,version_id,times,osm_key,osm_value,shape,nationEleName,nationCode,elementTypeID,changeset)(select {1}, {2},{3},'{4}',{5},to_timestamp('{6}', 'YYYY-MM-DD\"T\" HH24:MI:SS\"Z\"'),'{7}','{8}',{9}.shape,'{10}', '{11}', {12},'{13}' from {9} where exists (select osm_id from {0} where osm_id={2})) ", tableNameB, objectidB, osmidB, useridB, usernameB, versionidB, timesB, osmkeyB, osmvalueB, tableNameA, nationEleNameB, nationCodeB, elementTypeIDB, changesetB);
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("成功插入数据：objectid="+objectidB );
                    }
                }
                else
                {
                    sql = string.Format("insert into {0} (objectid,osm_id,user_id,user_name,version_id,times,osm_key,osm_value,shape,nationEleName,nationCode,elementTypeID,changeset)values({1},{2},{3},'{4}',{5},to_timestamp('{6}', 'YYYY-MM-DD\"T\" HH24:MI:SS\"Z\"'),'{7}','{8}',sdo_geometry ('{9}',4326),'{10}','{11}',{12},'{13}')", tableNameB, objectidB, osmidB, useridB, usernameB, versionidB, timesB, osmkeyB, osmvalueB, geomB, nationEleNameB, nationCodeB, elementTypeIDB, changesetB);
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("成功插入数据：objectid=" + objectidB);
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 整体匹配-根据hausdorff距离和属性值进行匹配
        /// </summary>
        public Boolean overallMatching(DataTable bufferDataTable, string osmid_v, string geomStr_wktA, string nationEleNameA, string conStringB, string tableNameB)
        {
            Boolean matchSuccess = false;
            OracleDBHelper helper = new OracleDBHelper();
            try
            {
                int bufferDataTableCount = bufferDataTable.Rows.Count;
                string foundObjectid = "";
                double foundHausdorff = double.MaxValue;
                Boolean flag = false;//记录上一对儿属性值是否相同
                for (int j = 0; j < bufferDataTableCount; j++)//循环遍历基态线对象缓冲区内对象
                {
                    string objectidB = bufferDataTable.Rows[j]["objectid"].ToString();
                    string geomStr_wktB = bufferDataTable.Rows[j]["geometry_wkt"].ToString();
                    string nationEleNameB = bufferDataTable.Rows[j]["nationEleName"].ToString();
                    string elementTypeIDB = bufferDataTable.Rows[j]["elementTypeID"].ToString();
                    double hausdorff = hausdorffDistance(geomStr_wktA, geomStr_wktB);

                    if (hausdorff <= hausdorffThreshold)//比阈值小，说明满足条件整体匹配成功，继续比较其他条件，如属性值，进入更新环节
                    {
                        if (nationEleNameA.Equals(nationEleNameB) && !flag)//第一次出现属性相同的情况
                        {
                            foundObjectid = objectidB;
                            foundHausdorff = hausdorff;
                            flag = true;
                        }
                        else if (nationEleNameA.Equals(nationEleNameB) && flag)//上一对属性值也相同
                        {
                            if (hausdorff < foundHausdorff)//且hausdorff值比上一对儿的还小，则将其作为最匹配对象
                            {
                                foundObjectid = objectidB;
                                foundHausdorff = hausdorff;
                                flag = true;
                            }
                        }
                        else if (!nationEleNameA.Equals(nationEleNameB) && !flag)//未出现属性值相同的情况
                        {
                            //还需要判断elementTypeIDA与elementTypeIDB是否相同，即是否属于同一要素图层的。

                            if (hausdorff < foundHausdorff)//选取hausdorff值最小的一个，将其作为最匹配对象
                            {
                                foundObjectid = objectidB;
                                foundHausdorff = hausdorff;
                            }
                        }
                        matchSuccess = true;
                    }
                    //更新匹配成功的道路
                    if (matchSuccess)
                    {
                        String sql = "";
                        OracleConnection con = helper.getOracleConnection();
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        //如果匹配成功，此处的更新操作是将增量A的shape线串更新基态的shape。
                        sql = string.Format("update {0} set shape =sdo_geometry ('{1}',4326) where objectid={2}", tableNameB, geomStr_wktA, Int32.Parse(foundObjectid));
                        helper.sqlExecute(sql);
                        //using (OracleCommand cmd = new OracleCommand(sql, con))
                        //{
                        //    cmd.ExecuteNonQuery();
                        //}
                        updateCount++;
                        Console.WriteLine("Objectid为：" + foundObjectid + "的道路被osmid_v:" + osmid_v + "整体更新");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return matchSuccess;
        }
        /// <summary>
        /// 局部匹配
        /// </summary>
        public Boolean partialMatch(DataTable bufferDataTable, string osmid_v, string geomStr_wktA, string nationEleNameA, string conStringB, string tableNameB)
        {
            Boolean matchSuccess = false;
            try
            {
                int bufferDataTableCount = bufferDataTable.Rows.Count;
                Boolean flag = false;//记录局部是否相同
                for (int j = 0; j < bufferDataTableCount; j++)
                {
                    string objectidB = bufferDataTable.Rows[j]["objectid"].ToString();
                    string geomStr_wktB = bufferDataTable.Rows[j]["geometry_wkt"].ToString();
                    string nationEleNameB = bufferDataTable.Rows[j]["nationEleName"].ToString();

                    flag = partialMatchSituation(osmid_v, geomStr_wktA, nationEleNameA, geomStr_wktB, nationEleNameB, tableNameB, int.Parse(objectidB), conStringB);
                    if (flag)
                    {
                        flag = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return matchSuccess;
        }
        /// <summary>
        /// 局部匹配情况
        /// </summary>
        /// <param name="osmid_V"></param>
        /// <param name="geomStr_wktA"></param>
        /// <param name="nationEleNameA"></param>
        /// <param name="geomStr_wktB"></param>
        /// <param name="nationEleNameB"></param>
        /// <param name="tableName"></param>
        /// <param name="objectid"></param>
        /// <param name="conStringB"></param>
        /// <returns></returns>
        public Boolean partialMatchSituation(string osmid_V, string geomStr_wktA, string nationEleNameA, string geomStr_wktB, string nationEleNameB, string tableName, int objectid, string conStringB)
        {
            Boolean flag = false;
            try
            {
                if (!nationEleNameA.Equals(nationEleNameB))//如果属性不相同，直接认为不匹配，返回false
                {
                    return flag;
                }
                string situation = partialHausdorff(osmid_V, geomStr_wktA, geomStr_wktB, tableName, objectid, conStringB);
                if (!"miss".Equals(situation))
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return flag;
        }
        /// <summary>
        /// 返回两条道路的局部是否匹配（在这里也是计算局部的hausdorff距离）
        /// </summary>
        public string partialHausdorff(string osmid_V, string geomStr_wktA, string geomStr_wktB, string tableName, int objectid, string conStringB)
        {
            try
            {
                string pointstrA = geomStr_wktA.Substring(12, geomStr_wktA.Length - 13);
                pointstrA = pointstrA.Trim();
                string[] pointA = pointstrA.Split(',');

                string pointsStrB = geomStr_wktB.Substring(12, geomStr_wktB.Length - 13);
                pointsStrB = pointsStrB.Trim();
                string[] pointB = pointsStrB.Split(',');

                pointA[0] = pointA[0].Trim();
                string[] A1 = pointA[0].ToString().Split(' ');//A的首节点
                double vector_A1x = double.Parse(A1[0]);
                double vector_A1y = double.Parse(A1[1]);

                pointA[pointA.Length - 1] = pointA[pointA.Length - 1].Trim();
                string[] A2 = pointA[pointA.Length - 1].ToString().Split(' ');//A的尾节点
                double vector_A2x = double.Parse(A2[0]);
                double vector_A2y = double.Parse(A2[1]);

                pointB[0] = pointB[0].Trim();
                string[] B1 = pointB[0].ToString().Split(' ');//B的首节点
                double vector_B1x = double.Parse(B1[0]);
                double vector_B1y = double.Parse(B1[1]);

                pointB[pointB.Length - 1] = pointB[pointB.Length - 1].Trim();
                string[] B2 = pointB[pointB.Length - 1].ToString().Split(' ');//B的尾节点
                double vector_B2x = double.Parse(B2[0]);
                double vector_B2y = double.Parse(B2[1]);

                double a = distance(vector_A1x, vector_A1y, vector_B1x, vector_B1y);//A的首节点与B的首节点之间的距离
                double b = distance(vector_A1x, vector_A1y, vector_B2x, vector_B2y);//A的首节点与B的尾节点之间的距离

                double c = distance(vector_A2x, vector_A2y, vector_B1x, vector_B1y);//A的尾节点与B的首节点之间的距离
                double d = distance(vector_A2x, vector_A2y, vector_B2x, vector_B2y);//A的尾节点与B的尾节点之间的距离             


                //直接采用欧氏距离确定对应节点，距离较近的确定为对应节点
                if (a <= b && a <= c && a <= d)//则两条道路的首节点对应
                {
                    pointA[1] = pointA[1].Trim();
                    string[] A1_2 = pointA[1].Split(' ');
                    double A1_2x = double.Parse(A1_2[0]);
                    double A1_2y = double.Parse(A1_2[1]);

                    pointB[1] = pointB[1].Trim();
                    string[] B1_2 = pointB[1].Split(' ');
                    double B1_2x = double.Parse(B1_2[0]);
                    double B1_2Y = double.Parse(B1_2[1]);

                    double angle = strokeLineAngleJudgeByCoordinate(vector_A1x, vector_A1y, A1_2x, A1_2y, vector_B1x, vector_B1y, B1_2x, B1_2Y);
                    if (Math.Abs(angle) <= angleThreshold || 180 - Math.Abs(angle) <= angleThreshold)
                        return partialUpdate(pointA, pointB, tableName, objectid, geomStr_wktA, osmid_V, a, conStringB);

                }
                else if (b <= a && b <= c && a <= d)
                {//则道路A的首节点与道路B的尾节点对应
                    System.Array.Reverse(pointB);
                    pointA[1] = pointA[1].Trim();
                    string[] A1_2 = pointA[1].Split(' ');
                    double A1_2x = double.Parse(A1_2[0]);
                    double A1_2y = double.Parse(A1_2[1]);

                    pointB[1] = pointB[1].Trim();
                    string[] B1_2 = pointB[1].Split(' ');
                    double B1_2x = double.Parse(B1_2[0]);//pointB经过了翻转所以现在的首第二个就是原先的尾第二个
                    double B1_2Y = double.Parse(B1_2[1]);

                    double angle = strokeLineAngleJudgeByCoordinate(vector_A1x, vector_A1y, A1_2x, A1_2y, vector_B2x, vector_B2y, B1_2x, B1_2Y);
                    if (Math.Abs(angle) <= angleThreshold)
                        return partialUpdate(pointA, pointB, tableName, objectid, geomStr_wktA, osmid_V, b, conStringB);

                }
                else if (c <= a && c <= b && c <= d)//A的尾节点与B的首节点对应
                {
                    System.Array.Reverse(pointA);
                    pointA[1] = pointA[1].Trim();
                    string[] A1_2 = pointA[1].Split(' ');
                    double A1_2x = double.Parse(A1_2[0]);
                    double A1_2y = double.Parse(A1_2[1]);

                    pointB[1] = pointB[1].Trim();
                    string[] B1_2 = pointB[1].Split(' ');
                    double B1_2x = double.Parse(B1_2[0]);
                    double B1_2Y = double.Parse(B1_2[1]);

                    double angle = strokeLineAngleJudgeByCoordinate(vector_A2x, vector_A2y, A1_2x, A1_2y, vector_B1x, vector_B1y, B1_2x, B1_2Y);
                    if (Math.Abs(angle) <= angleThreshold)
                        return partialUpdate(pointA, pointB, tableName, objectid, geomStr_wktA, osmid_V, c, conStringB);
                }
                else if (d <= a && d <= b && d <= c)//A的尾节点与B的尾节点对应
                {
                    System.Array.Reverse(pointA);
                    //Console.WriteLine("翻转前：pointB：" + pointB);
                    System.Array.Reverse(pointB);
                    //Console.WriteLine("翻转后：pointB：" + pointB);
                    pointA[1] = pointA[1].Trim();
                    string[] A1_2 = pointA[1].Split(' ');
                    double A1_2x = double.Parse(A1_2[0]);
                    double A1_2y = double.Parse(A1_2[1]);

                    pointB[1] = pointB[1].Trim();
                    string[] B1_2 = pointB[1].Split(' ');
                    double B1_2x = double.Parse(B1_2[0]);
                    double B1_2Y = double.Parse(B1_2[1]);

                    double angle = strokeLineAngleJudgeByCoordinate(vector_A2x, vector_A2y, A1_2x, A1_2y, vector_B2x, vector_B2y, B1_2x, B1_2Y);
                    if (Math.Abs(angle) <= angleThreshold)
                        return partialUpdate(pointA, pointB, tableName, objectid, geomStr_wktA, osmid_V, d, conStringB);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return "miss";//未知情况
        }
        /// <summary>
        /// 局部匹配更新
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="tableName"></param>
        /// <param name="objectid"></param>
        /// <param name="geomStr_wktA"></param>
        /// <param name="osmid_V"></param>
        /// <param name="distance"></param>
        /// <param name="conStringB"></param>
        /// <returns></returns>
        public string partialUpdate(string[] pointA, string[] pointB, string tableName, int objectid, string geomStr_wktA, string osmid_V, double distance, string conStringB)
        {
            try
            {
                string verticalPointA_B_f = "ss";//过A的首端点的垂线与B的交点
                string verticalPointA_B_e = "ss";//过A的末端点的垂线与B的交点
                string verticalPointB_A_f = "ss";//过B的首端点的垂线与A的交点
                string verticalPointB_A_e = "ss";//过B的末端点的垂线与A的交点


                string B_f_fcz = "";//B上的从首端点到首垂足的点
                string B_f_ecz = "";//B上的从首端点到尾垂足的点
                string B_e_fcz = "";//B上的从首垂足到尾端点的点
                string B_e_ecz = "";//B上的从尾垂足到尾端点的点

                Boolean AE_B = false;//A的尾端点到B的垂足是否存在
                Boolean AF_B = false;//A的首端点到B的垂足是否存在

                for (int i = 0; i < pointB.Length - 1; i++)
                {
                    if (!AF_B)//记录B的首端点到首垂足的节点
                    {
                        B_f_fcz += pointB[i] + ",";
                    }

                    if (!AE_B)//记录B的首端点到尾垂足的节点
                    {
                        B_f_ecz += pointB[i] + ",";
                    }

                    if (verticalPointA_B_f.Equals("N") || verticalPointA_B_f.Equals("ss"))
                    {
                        verticalPointA_B_f = verticalIntersect(pointA[1], pointA[0], pointB[i], pointB[i + 1]);//从首端开始，B上的首垂足
                        if (!verticalPointA_B_f.Equals("N"))
                        {
                            AF_B = true;
                            B_f_fcz += verticalPointA_B_f;
                            B_e_fcz += verticalPointA_B_f;
                        }

                    }

                    if (verticalPointA_B_e.Equals("N") || verticalPointA_B_e.Equals("ss"))
                    {
                        verticalPointA_B_e = verticalIntersect(pointA[pointA.Length - 2], pointA[pointA.Length - 1], pointB[i], pointB[i + 1]);//从尾端开始，B上的尾垂足
                        if (!verticalPointA_B_e.Equals("N"))
                        {
                            AE_B = true;
                            B_f_ecz += verticalPointA_B_e;
                            B_e_ecz += verticalPointA_B_e;
                        }
                    }
                    if (AF_B)//记录B的首垂足到尾端点的节点
                    {
                        B_e_fcz += "," + pointB[i + 1];
                    }

                    if (AE_B)//记录B的尾垂足到尾端点的节点
                    {
                        B_e_ecz += "," + pointB[i + 1];
                    }
                }

                string A_f_fcz = "";//A上的从首端点到首垂足的点
                string A_f_ecz = "";//A上的从首端点到尾垂足的点
                string A_e_fcz = "";//A上的从首垂足到尾端点的点
                string A_e_ecz = "";//A上的从尾垂足到尾端点的点

                Boolean BE_A = false;//B的尾端点到A的垂足是否存在
                Boolean BF_A = false;//B的首端点到A的垂足是否存在
                for (int i = 0; i < pointA.Length - 1; i++)
                {
                    if (!BF_A)//记录A的首端点到首垂足的节点
                    {
                        A_f_fcz += pointA[i] + ",";
                    }
                    if (!BE_A)//记录A的首端点到首垂足的节点
                    {
                        A_f_ecz += pointA[i] + ",";
                    }
                    if (verticalPointB_A_e.Equals("N") || verticalPointB_A_e.Equals("ss"))
                    {
                        verticalPointB_A_e = verticalIntersect(pointB[pointB.Length - 2], pointB[pointB.Length - 1], pointA[i], pointA[i + 1]);//B的尾端点到A的垂足，A上的尾垂足
                        if (!verticalPointB_A_e.Equals("N"))
                        {
                            BE_A = true;
                            A_f_ecz += verticalPointB_A_e;
                            A_e_ecz += verticalPointB_A_e;
                        }
                    }
                    if (verticalPointB_A_f.Equals("N") || verticalPointB_A_f.Equals("ss"))
                    {
                        verticalPointB_A_f = verticalIntersect(pointB[1], pointB[0], pointA[i], pointA[i + 1]);//B的首端点到A的垂足，A上的首垂足
                        if (!verticalPointB_A_f.Equals("N"))
                        {
                            BF_A = true;
                            A_f_fcz += verticalPointB_A_f;
                            A_e_fcz += verticalPointB_A_f;
                        }
                    }
                    if (BF_A)//记录A的首垂足到尾端点的节点
                    {
                        A_e_fcz += "," + pointA[i + 1];
                    }

                    if (BE_A)//记录A的尾垂足到尾端点的节点
                    {
                        A_e_ecz += "," + pointA[i];
                    }
                }

                if (verticalPointA_B_f.Equals("N"))//B上的首垂足不存在，两种情况：1A的首节点方向比B长，2两者尾头顶头无共同部分
                {
                    if (verticalPointA_B_e.Equals("N"))
                    {//B上的尾垂足也不存在，1A的尾节点方向比B长，2两者头顶头无公同部分 

                        if (verticalPointB_A_f.Equals("N"))
                        {//A上的首垂足也不存在，1B的首节点方向比A长，2两者头顶头无共同部分

                            //三者决定了，《A、B为头顶头无共同部分的情况》
                            if (verticalPointB_A_e.Equals("N"))//此时A上的尾垂足点必须不存在，否则就出现了错误
                            {
                                if (distance < distanceThreshold)//两端点之间的距离小于阈值
                                {
                                    string newGeomStr = "";
                                    for (int i = pointA.Length - 1; i >= 0; i--)
                                    {
                                        newGeomStr += pointA[i] + ",";
                                    }

                                    newGeomStr = newGeomStr + B_f_ecz + pointB[pointB.Length - 1];//B_f_ecz + pointB[pointB.Length-1]这部分为B
                                    newGeomStr = "LINESTRING(" + newGeomStr + ")";
                                    //OracleConnection con = new OracleConnection(conStringB);
                                    OracleDBHelper helper = new OracleDBHelper();
                                    OracleConnection con = helper.getOracleConnection();
                                    con.Open();
                                    string sql = string.Format("update {0} set shape =sdo_geometry ('{1}',4326)) where objectid = {2} ", tableName, newGeomStr, objectid);
                                    using (OracleCommand cmd = new OracleCommand(sql, con))
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                    con.Close();
                                    Console.WriteLine("Objectid为：" + objectid + "的道路被osmid_v:" + osmid_V + "局部更新");
                                    updateCount++;
                                    return "Y1";//返回成功匹配情况1
                                }

                            }
                            else
                            {//报错 

                            }
                        }
                        else if (!verticalPointB_A_f.Equals("N"))//A上的首垂足存在，1B的首节点方向比A短,此时已经可以确定《B被A包含的情况了，A长B短》
                        {
                            if (!verticalPointB_A_e.Equals("N"))//此时A上的尾垂足点必须存在，否则就出现了错误
                            {
                                string newGeomStr = geomStr_wktA;
                                //OracleConnection con = new OracleConnection(conStringB);
                                OracleDBHelper helper = new OracleDBHelper();
                                OracleConnection con = helper.getOracleConnection();
                                con.Open();
                                string sql = string.Format("update {0} set shape =sdo_geometry ('{1}',4326)) where objectid = {2} ", tableName, newGeomStr, objectid);
                                using (OracleCommand cmd = new OracleCommand(sql, con))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                con.Close();
                                Console.WriteLine("Objectid为：" + objectid + "的道路被osmid_v:" + osmid_V + "局部更新");
                                updateCount++;
                                return "Y2";//返回成功匹配情况1
                            }
                            else
                            { //报错

                            }
                        }
                    }
                    else if (!verticalPointA_B_e.Equals("N"))//B上的尾垂足存在，B的尾节点方向比A长，此时已经可以确定《A、B为错位情况，且A首节点方向比B长，A的尾节点方向比B短》
                    {
                        if (!AE_B)
                        {
                            B_f_ecz = B_f_ecz.Substring(0, B_f_ecz.Length - 1);
                        }
                        A_e_fcz = "LINESTRING(" + A_e_fcz + ")";
                        B_f_ecz = "LINESTRING(" + B_f_ecz + ")";
                        double hausdorff = hausdorffDistance(A_e_fcz, B_f_ecz);//A首垂足点到A的尾端点，与B的首端点到B的尾垂足进行匹配
                        if (hausdorff < Hausdorff2Threshold)
                        {
                            string newGeomStr = geomStr_wktA.Substring(11, geomStr_wktA.Length - 12) + "," + B_e_ecz;//A+B的尾垂足到B的尾节点
                            newGeomStr = "LINESTRING(" + newGeomStr + ")";
                            //OracleConnection con = new OracleConnection(conStringB);
                            OracleDBHelper helper = new OracleDBHelper();
                            OracleConnection con = helper.getOracleConnection();
                            con.Open();
                            string sql = string.Format("update {0} set shape =sdo_geometry ('{1}',4326)) where objectid = {2} ", tableName, newGeomStr, objectid);
                            using (OracleCommand cmd = new OracleCommand(sql, con))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            con.Close();
                            Console.WriteLine("Objectid为：" + objectid + "的道路被局部更新");
                            updateCount++;

                            return "Y3";//返回成功匹配情况1
                        }
                    }
                }
                else if (!verticalPointA_B_f.Equals("N"))//B上的首垂足存在，1A的首节点方向比B短
                {
                    if (!verticalPointA_B_e.Equals("N"))//B上的尾垂足也存在，1A尾节点方向比B短，此时已经确定了《A被B包含的情况》
                    {
                        string newGeomStr = geomStr_wktA;
                        //OracleConnection con = new OracleConnection(conStringB);
                        OracleDBHelper helper = new OracleDBHelper();
                        OracleConnection con = helper.getOracleConnection();
                        con.Open();
                        string sql = string.Format("update {0} set shape =sdo_geometry ('{1}',4326)) where objectid = {2} ", tableName, newGeomStr, objectid);
                        using (OracleCommand cmd = new OracleCommand(sql, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("Objectid为：" + objectid + "的道路被oscid_v:" + osmid_V + "局部更新");
                        updateCount++;
                        con.Close();
                        return "Y4";//返回成功匹配情况1


                    }
                    else if (verticalPointA_B_e.Equals("N"))//B上的尾垂足不存在，1A的尾节点方向比B长，此时已经确定了《A、B为错位情况，且A的首节点方向比B短，A的尾节点方向比B长》
                    {
                        if (!BE_A)
                        {//说明垂足点始终没出现
                            A_f_ecz = A_f_ecz.Substring(0, A_f_ecz.Length - 1);
                        }
                        A_f_ecz = "LINESTRING(" + A_f_ecz + ")";
                        B_e_fcz = "LINESTRING(" + B_e_fcz + ")";
                        double hausdorff = hausdorffDistance(A_f_ecz, B_e_fcz);//A首节点到A上的尾垂足，与B的首垂足到B的尾节点进行匹配
                        if (hausdorff < Hausdorff2Threshold)
                        {
                            string newGeomStr = B_f_fcz + "," + geomStr_wktA.Substring(11, geomStr_wktA.Length - 12);//B的首节点到B的首垂足+A
                            newGeomStr = "LINESTRING(" + newGeomStr + ")";
                            //OracleConnection con = new OracleConnection(conStringB);
                            OracleDBHelper helper = new OracleDBHelper();
                            OracleConnection con = helper.getOracleConnection();
                            con.Open();
                            string sql = string.Format("update {0} set shape =sdo_geometry ('{1}',4326)) where objectid = {2} ", tableName, newGeomStr, objectid);
                            using (OracleCommand cmd = new OracleCommand(sql, con))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            con.Close();
                            Console.WriteLine("Objectid为：" + objectid + "的道路被局部更新");
                            updateCount++;

                            return "Y5";//返回成功匹配情况1
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return "miss";//未知情况
        }
        ////知道一点和斜率求直线方程：y=k(x-a)+b//判断点是否在线的缓冲区范围内：ST_Contains（） 
        //////求过某条线段端点的垂线与另一条线段的交点
        //知道一点和斜率求直线方程：y=k(x-a)+b//判断点是否在线的缓冲区范围内：ST_Contains（） 
        /// <summary>
        /// 求过某条线段端点的垂线与另一条线段的交点,存在交点返回交点，否则返回N
        /// </summary>
        /// <param name="pointsA1"></param>
        /// <param name="pointsA2"></param>
        /// <param name="pointsB1"></param>
        /// <param name="pointsB2"></param>
        /// <returns></returns>
        public string verticalIntersect(string pointsA2, string pointsA1, string pointsB1, string pointsB2)
        {
            try
            {
                pointsA1 = pointsA1.Trim();
                string[] A1 = pointsA1.ToString().Split(' ');
                double vector_A1x = double.Parse(A1[0]);
                double vector_A1y = double.Parse(A1[1]);
                pointsA2 = pointsA2.Trim();
                string[] A2 = pointsA2.ToString().Split(' ');
                double vector_A2x = double.Parse(A2[0]);
                double vector_A2y = double.Parse(A2[1]);

                double k1 = 0;

                double verticalX = 0;
                double verticalY = 0;
                if (vector_A2x - vector_A1x != 0)
                {
                    k1 = (vector_A2y - vector_A1y) / (vector_A2x - vector_A1x);
                }
                else
                { //该直线垂直于X轴，其垂线斜率为0，垂线垂直于Y轴；

                    verticalX = vector_A1x - 10;
                    verticalY = vector_A1y;
                }
                double k2 = 0;
                if (k1 != 0)
                {
                    k2 = -1 / k1;
                }
                else
                { //k2不存在，垂线垂直于X轴
                    verticalX = vector_A1x;
                    verticalY = vector_A1y - 10;
                }
                if (k2 != 0)
                {//得到垂线的另一个点
                    //verticalX = (0 - vector_A1y) / k2 + vector_A1x;
                    //verticalY = (0 - k2) * vector_A1x + vector_A1y;
                    verticalX = 0;
                    verticalY = (0 - vector_A1x) * k2 + vector_A1y;
                }
                else
                {
                    verticalX = vector_A1x - 10;
                    verticalY = vector_A1y;
                }
                //则垂线上的两个点为verticalA（vector_A1x，vector_A1y）verticalB（verticalX，verticalY）
                pointsB1 = pointsB1.Trim();
                string[] B1 = pointsB1.Split(' ');
                double vector_B1x = double.Parse(B1[0]);
                double vector_B1y = double.Parse(B1[1]);
                pointsB2 = pointsB2.Trim();
                string[] B2 = pointsB2.Split(' ');
                double vector_B2x = double.Parse(B2[0]);
                double vector_B2y = double.Parse(B2[1]);

                string intersectPoint = getIntersectPoint(vector_A1x, vector_A1y, verticalX, verticalY, vector_B1x, vector_B1y, vector_B2x, vector_B2y);

                ////SELECT ST_AsText(ST_Intersection('LINESTRING(0 0,1 1)'::geometry, 'LINESTRING ( 2 0, 0 2 )'::geometry));返回：point(1 1)
                //string linString1 = "LINESTRING(" + vector_A1x + " " + vector_A1y + "," + verticalX + " " + verticalY + ")";
                string linString2 = "LINESTRING(" + pointsB1 + "," + pointsB2 + ")";

                if (intersectPoint.Equals("N"))
                {
                    return "N";
                }
                string sql = string.Format("select SDO_WITHIN_DISTANCE(sdo_geometry ('{0}'),sdo_geometry ('{1}',4326),0.001)", intersectPoint, linString2);
                string flag = "";
                OracleDBHelper helper = new OracleDBHelper();
                OracleConnection con = helper.getOracleConnection();
                if(con.State ==ConnectionState .Closed )
                {
                con.Open();
                }
                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    flag = cmd.ExecuteScalar().ToString();
                }
                con.Close();
                if (flag == "True")//说明垂足在线段上,两条线段相交，返回交点
                {
                    return intersectPoint.Substring(6, intersectPoint.Length - 7);//该部分需要调试修改
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //垂足不在线段上，两条线段不相交
            return "N";
        }
        /// <summary>
        /// 得到插入的线数据
        /// </summary>
        /// <param name="ax1"></param>
        /// <param name="ay1"></param>
        /// <param name="ax2"></param>
        /// <param name="ay2"></param>
        /// <param name="bx1"></param>
        /// <param name="by1"></param>
        /// <param name="bx2"></param>
        /// <param name="by2"></param>
        /// <returns></returns>
        public string getIntersectPoint(double ax1, double ay1, double ax2, double ay2, double bx1, double by1, double bx2, double by2)
        {
            double intersectX = 0;
            double intersectY = 0;
            double k1 = 0;
            double k2 = 0;
            if (ax2 - ax1 != 0)
            {
                k1 = (ay2 - ay1) / (ax2 - ax1);
            }
            else
            { //直线1垂直于X轴
                intersectX = ax1;
                if (bx2 - bx1 != 0)
                {
                    k2 = (by2 - by1) / (bx2 - bx1);
                }
                else
                {//直线2垂直于X轴

                    return "N";
                }
                intersectY = k2 * (intersectX - bx1) + by1;
                return "POINT(" + intersectX + " " + intersectY + ")";
            }
            if (bx2 - bx1 != 0)
            {
                k2 = (by2 - by1) / (bx2 - bx1);
            }
            else
            { //直线2垂直于X轴
                intersectX = bx1;
                intersectY = k1 * (intersectX - ax1) + ay1;
                return "POINT(" + intersectX + " " + intersectY + ")";
            }

            if (k1 != k2)
            {
                intersectX = (k1 * ax1 - k2 * bx1 - ay1 + by1) / (k1 - k2);
                intersectY = k1 * ((k1 * ax1 - k2 * bx1 - ay1 + by1) / (k1 - k2) - ax1) + ay1;
                return "POINT(" + intersectX + " " + intersectY + ")";
                //return intersectX + " " + intersectY;
            }
            else
            {
                return "N";
            }
        }
        /// <summary>
        /// 道路要素开始匹配更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void roadMatchingBT_Click(object sender, EventArgs e)
        {
            try
            {
                updateCount = 0;
                string tableNameA = zhuanyeDataCOMbB.Text;//读取增量数据表
                string tableNameB = basicDataCOMB.Text;//读取基态数据表
                bufferRadius = double.Parse(radiusTB.Text.ToString());//缓冲区半径
                hausdorffThreshold = double.Parse(HausdorffTB.Text.ToString());//Hausdorff整体匹配阈值
                Hausdorff2Threshold = double.Parse(Hausdorff2TB.Text);//Hausdorff局部匹配阈值
                angleThreshold = double.Parse(angleThresholdTB.Text);//夹角阈值
                distanceThreshold = 5;
                string constringA = this.conStr;//更新数据表，目前更新数据表和基态数据表在同一个数据库中所以暂用同一个连接字符串
                string constringB = this.conStr;//基态数据表
                //string tableNameA, string tableNameB, string conStringA, string conStringB, double bufferRadius, double hausdorffThreshold double,double hausdorffThreshold
                //CreateTemLineUPData();
                LineMatching(tableNameA, tableNameB, constringA, constringB);//线要素匹配函数
                MessageBox.Show("线要素匹配更新完成！共计" + updateCount + "条线数据被更新！");
                LineElementMatch lineform = new LineElementMatch();
                lineform.Hide();//关闭子窗体
                IEnumDataset enumDataset;//枚举类型，装数据集的
                IDataset dataset;
                pFeatureLayer = new FeatureLayer();//新建一个要素图层对象
                //获取图层名  
                enumDataset = workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);//使用工作空间获取数据库中的要素集，放入枚举对象中
                dataset = enumDataset.Next();//在枚举对象内部查找下一个数据
                string tableName = "SYSTEM.CURRENTLINEDATA";
                while (dataset != null)//当存在数据时
                {
                    if (dataset.Name == tableName)//判断和要素层是否匹配
                    {
                        pFeatureLayer.FeatureClass = dataset as IFeatureClass;//如果数据集中的数据和要素图层匹配成功，那么将数据集转换为要素类
                        string layerName = pFeatureLayer.FeatureClass.AliasName;   //获取别名
                        pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;
                        break;
                    }
                    dataset = enumDataset.Next();//循环遍历要素集
                }
                LineUpdataResultShow frm = new LineUpdataResultShow(pFeatureLayer);//首先实例化
                frm.Show();//Show方法
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LineElementMatch_Load(object sender, EventArgs e)
        {
            string server = OSMDataBaseLinkForm.Server_;
            string user = OSMDataBaseLinkForm.User_;
            string password = OSMDataBaseLinkForm.Password_;
            string database = OSMDataBaseLinkForm.DataBase_;

            IPropertySet pPropset = new PropertySet();//创建一个属性设置对象
            IWorkspaceFactory pWorkspaceFact = new SdeWorkspaceFactory();//创建一个空间数据引擎工作空间工厂
            pPropset.SetProperty("server", server);
            // propertySet.SetProperty("INSTANCE", Instance.Text );//如果没有设置INSTANCE属性，会有连接窗体弹出  
            pPropset.SetProperty("INSTANCE", "sde:oracle11g:" + server + "/" + database);// by 丁洋修改
            pPropset.SetProperty("database", database);
            pPropset.SetProperty("user", user);
            pPropset.SetProperty("password", password);
            pPropset.SetProperty("version", "SDE.DEFAULT");
            workspace = pWorkspaceFact.Open(pPropset, 0);//使用属性集来打开地理数据库

            IEnumDatasetName enumDatasetName;//定义枚举数据集名称
            IDatasetName datasetName;//定义数据集名字对象
            //获取矢量数据集
            enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureDataset);//工作空间获取数据集名称，返回一个枚举集，装入不同的名字
            datasetName = enumDatasetName.Next();//获取枚举集中的下一个元素

            enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureClass);
            datasetName = enumDatasetName.Next();
            while (datasetName != null)
            {
                //this.FeatureLayer.Items.Add(datasetName.Name);
                datasetName = enumDatasetName.Next();
            }
        }
    }
}
