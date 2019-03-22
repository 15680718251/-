using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using GIS.UI.AdditionalTool;

namespace FeatureMatchUpdate.Forms
{
    /****************基态数据和增量数据的线要素数据提取  by zbl   2018.7.31***********************/
    public partial class LineFeatureExtraction : Form
    {
        string conStr = "";//数据库连接字符串
        string lineRuleTableName= "LineFeatureRule";//线要素规则表
        string  currentLineTableName = "CurrentLineData";//基态线要素数据表用于存储提取处理后的基态线数据
        string IncreLineTableName = "IncrementLineData";//增量线要素数据表用于存储提取处理后的增量线数据
        //public enum geoTypes { Node=0,Way = 1,Area=2};//几何类型
        //public enum layerIndex { Atraffic=1,Water=2,Residential=3,Vegetation=4,Soil=5};
        //double bufferRadius = 0;//缓冲区搜索半径
        //int objectid = 1;

        public LineFeatureExtraction()
        {
            InitializeComponent();
        }
        public LineFeatureExtraction(string conString)
        {
            InitializeComponent();
            this.conStr = conString;
            this.LineExtractionpgBar.Minimum = 0;
        }

        #region 提取全部图层的线要素数据  by zbl 2018.8.14修改
        /// <summary>
        /// 提取基态数据库中的所有线要素数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurDataExtractionLineBtn_Click(object sender, EventArgs e)
        {
            CreateExaCurLineFeatureTable();//创建存储提取后的基态线数据表
            DataTable curdataTable = CurDataCheck();//数据表检索获取数据
            DataTable ruleTable = ruleCheck();//规则表检索获取数据
            CurLineDataExtraction(ruleTable, curdataTable);
            MessageBox.Show("您需要的线数据已提取完成！");
        }
        /// <summary>
        /// 创建存储提取处理后的基态线数据表的方法函数  
        /// </summary>
        public void CreateExaCurLineFeatureTable()
        {
            OracleDBHelper creHelper = new OracleDBHelper();
            OracleConnection con = creHelper.getOracleConnection();//Oracle数据库连接
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            if (creHelper.IsExistTable("CURRENTLINEDATA"))//判断当前数据库中是否存在基态线数据表
            {
                string sql1 = string.Format("delete from USER_SDO_GEOM_METADATA where TABLE_NAME='CURRENTLINEDATA'");//删除原数据视图中的数据表 by zbl 2018.9.16修改
                creHelper.sqlExecuteUnClose(sql1);
                sql1 = string.Format("drop index idx_CURRENTLINEDATA");//删除数据表中的索引
                creHelper.sqlExecuteUnClose(sql1);
                string sql = string.Format("drop table CURRENTLINEDATA");//若存在将其删除
                creHelper.sqlExecuteUnClose(sql);
            }
            string createTableSql1 = "(objectid NUMBER(30) primary key NOT NULL,osmid NUMBER(30) ,versionid NUMBER(30),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),osmkey VARCHAR2(50),osmvalue VARCHAR2(50),trustvalue FLOAT,userreputation FLOAT,shape SDO_GEOMETRY,source NUMBER(4))";
            creHelper.createTable(currentLineTableName, createTableSql1);
            string sql2 = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID) VALUES ('CURRENTLINEDATA', 'shape',MDSYS.SDO_DIM_ARRAY  (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050),MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050)),4326)");
            creHelper.sqlExecuteUnClose(sql2);
            string sql3 = string.Format("create index idx_CURRENTLINEDATA on CURRENTLINEDATA(shape) indextype is mdsys.spatial_index");//by 20180916修改
            creHelper.sqlExecuteUnClose(sql3);
        }
        /// <summary>
        /// 基态数据表遍历读取数据
        /// </summary>
        /// <returns></returns>
        public DataTable CurDataCheck()
        {
            DataTable curdataTable = new DataTable();
            OracleDBHelper helper = new OracleDBHelper();
            OracleConnection con = helper.getOracleConnection();
            //dataTable.Columns.Add("osmid");
            //curdataTable.Columns.Add("objectid");
            curdataTable.Columns.Add("osmid");
            curdataTable.Columns.Add("osmkey");
            curdataTable.Columns.Add("osmvalue");
            curdataTable.Columns.Add("nationelename");
            try
            {
                string sqlStr1 = string.Format("insert into {0} (osmid, versionid, starttime,endtime,changeset, userid, username,osmkey,osmvalue,trustvalue,userreputation,shape,source)(select  aline.osmid,aline.versionid,aline.starttime,aline.endtime,aline.changeset,aline.userid,aline.username,aline.fc,aline.dsg,aline.trustvalue,aline.userreputation,aline.shape,aline.source  from aline WHERE EXISTS(select osm_key, osm_value FROM {1} where aline.fc= osm_key and aline.dsg= osm_value))", currentLineTableName, lineRuleTableName);
                helper.sqlExecute(sqlStr1);//选取基态数据中的线要素数据插入基态线数据表中 
                string sqlStr2 = string.Format("alter table {0} add (nationEleName VARCHAR2(50),elementTypeID NUMBER(30),nationCode VARCHAR2(50))", currentLineTableName);
                helper.sqlExecute(sqlStr2);//向提取后的Currentlinedata数据表中增加字段 
                string sqlStr3 = string.Format("select osmid,osmkey,osmvalue,nationEleName from CurrentLineData");
                using (OracleDataReader rd = helper.queryReader(sqlStr3))
                {
                    while (rd.Read())
                    {
                        DataRow dataTableRow = curdataTable.NewRow();
                        //dataTableRow["osmid"] = rd["osm_id"].ToString();
                        //dataTableRow["versionid"] = rd["version_id"].ToString();
                        dataTableRow["osmid"] = rd["osmid"].ToString();
                        dataTableRow["osmkey"] = rd["osmkey"].ToString();
                        dataTableRow["osmvalue"] = rd["osmvalue"].ToString();
                        dataTableRow["nationelename"] = rd["nationEleName"].ToString();
                        curdataTable.Rows.Add(dataTableRow);//将读取到的基态数据添加到数据表行
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return curdataTable;
        }
        /// <summary>
        /// 规则表遍历读取
        /// </summary>
        /// <returns></returns>
        public DataTable ruleCheck()
        {
            DataTable ruleTable = new DataTable();
            ruleTable.Columns.Add("osm_key");
            ruleTable.Columns.Add("osm_value");
            ruleTable.Columns.Add("nationEleName");
            ruleTable.Columns.Add("elementTypeID");
            ruleTable.Columns.Add("nationCode");

            OracleDBHelper ruleHelper = new OracleDBHelper(); //获取规则库帮助对象
            string ruleSql = string.Format("select*from {0}", lineRuleTableName);
            using (OracleDataReader rd = ruleHelper.queryReader(ruleSql))
            {
                while (rd.Read())
                {
                    DataRow ruleTableRow = ruleTable.NewRow();
                    ruleTableRow["osm_key"] = rd["osm_key"].ToString();
                    ruleTableRow["osm_value"] = rd["osm_value"].ToString();
                    ruleTableRow["nationEleName"] = rd["nationEleName"].ToString();
                    //ruleTableRow["elementType"] = rd["elementType"].ToString();
                    ruleTableRow["elementTypeID"] = rd["elementTypeID"].ToString();
                    ruleTableRow["nationCode"] = rd["nationCode"].ToString();
                    ruleTable.Rows.Add(ruleTableRow);
                }
            }
            return ruleTable;
        }
        /// <summary>
        /// 增量数据表遍历读取数据
        /// </summary>
        /// <returns></returns>
        public DataTable IncreDataCheck()
        {
            DataTable incredataTable = new DataTable();
            OracleDBHelper helper = new OracleDBHelper();
            OracleConnection con = helper.getOracleConnection();
            incredataTable.Columns.Add("osmkey");
            incredataTable.Columns.Add("osmvalue");
            incredataTable.Columns.Add("nationelename");
            try
            {
                string sqlStr1 = string.Format("insert into {0} (osmid,versionid,starttime,endtime,changeset,userid,username,osmkey,osmvalue,trustvalue,userreputation,shape,changeType,source)(select  oscline.osmid,oscline.versionid,oscline.starttime,oscline.endtime,oscline.changeset,oscline.userid,oscline.username,oscline.fc,oscline.dsg,oscline.trustvalue,oscline.userreputation,oscline.shape,oscline.changeType,oscline.source  from oscline WHERE EXISTS(select osm_key, osm_value FROM {1} where oscline.fc= osm_key and oscline.dsg= osm_value))", IncreLineTableName, lineRuleTableName);
                helper.sqlExecute(sqlStr1);//选取增量数据中的线要素数据插入增量数据表中
                string sqlStr2 = string.Format("alter table {0} add (nationEleName VARCHAR2(50),elementTypeID NUMBER(30),nationCode VARCHAR2(50))", IncreLineTableName);
                helper.sqlExecute(sqlStr2);//向提取后的Currentlinedata数据表中增加字段 
                string sqlStr3 = string.Format("select osmkey,osmvalue,nationEleName from IncrementLineData");
                using (OracleDataReader rd = helper.queryReader(sqlStr3))
                {
                    while (rd.Read())
                    {
                        DataRow dataTableRow = incredataTable.NewRow();
                        //dataTableRow["osmid"] = rd["osm_id"].ToString();
                        //dataTableRow["versionid"] = rd["version_id"].ToString();
                        dataTableRow["osmkey"] = rd["osmkey"].ToString();
                        dataTableRow["osmvalue"] = rd["osmvalue"].ToString();
                        dataTableRow["nationelename"] = rd["nationEleName"].ToString();
                        incredataTable.Rows.Add(dataTableRow);//将读取到的基态数据添加到数据表行
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return incredataTable;
        }
        /// <summary>
        /// 基态线数据提取方法函数
        /// </summary>
        public void CurLineDataExtraction(DataTable ruleTable, DataTable curdataTable)
        {
            OracleDBHelper helper = new OracleDBHelper();
            OracleConnection con = helper.getOracleConnection();
            for (int i = 0; i < curdataTable.Rows.Count; i++)//基态数据表字段获取
            {
                this.LineExtractionpgBar.Maximum = curdataTable.Rows.Count;
                LineExtractionpgBar.Value = i;
                string nationEleName = "";string elementTypeID = "";  string nationCode = "";
                string osmid = curdataTable.Rows[i]["osmid"].ToString();
                string key = curdataTable.Rows[i]["osmkey"].ToString();
                string value = curdataTable.Rows[i]["osmvalue"].ToString();
                string nationelename = curdataTable.Rows[i]["nationEleName"].ToString();
                //objectid=i+1;
                //string sql = string.Format("update {0} set objectid='{1}'where osmid={2}", currentLineTableName,objectid,osmid);
                //helper.sqlExecute(sql);
                try
                {
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    //循环遍历线规则表中的数据
                    for (int j = 0; j < ruleTable.Rows.Count; j++)
                    {
                        string osm_key = ruleTable.Rows[j]["osm_key"].ToString();
                        string osm_value = ruleTable.Rows[j]["osm_value"].ToString();
                        string NationEleName = ruleTable.Rows[j]["nationEleName"].ToString(); ;
                        //string ElementType = ruleTable.Rows[j]["elementType"].ToString();
                        string ElementTypeID = ruleTable.Rows[j]["elementTypeID"].ToString();
                        //string Geometry = ruleTable.Rows[j]["geometry"].ToString();
                        string NationCode = ruleTable.Rows[j]["nationcode"].ToString();
                        
                        if (nationelename == "")
                        {
                            if (key == osm_key && value == osm_value)
                            {
                                nationEleName = NationEleName;
                                //elementType = ElementType;
                                elementTypeID = ElementTypeID;
                                //geometry = Geometry;
                                nationCode = NationCode;
                                
                                string sqlStr2 = string.Format("update {0} set nationEleName='{1}' ,elementTypeID={2},nationCode='{3}' where osmkey='{4}' and osmvalue='{5}'", currentLineTableName, nationEleName, elementTypeID, nationCode,osm_key, osm_value);
                                using (OracleCommand objCommand = new OracleCommand(sqlStr2, con))
                                {
                                    //con.Open();
                                    objCommand.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        /// <summary>
        /// 提取增量数据库中的所有线要素数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IncreDataExtractionLineBtn_Click(object sender, EventArgs e)
        {
            CreateExaIncreLineFeatureTable();//创建存储提取后的增量线数据表
            DataTable incredataTable = IncreDataCheck();//数据表检索获取数据
            DataTable ruleTable = ruleCheck();//规则表检索获取数据
            IncreLineDataExtraction(ruleTable, incredataTable);
            MessageBox.Show("您需要的线数据已提取完成！");
        }
        /// <summary>
        /// 创建存储提取处理后的增量线数据表的方法函数  
        /// </summary>
        public void CreateExaIncreLineFeatureTable()
        {
            OracleDBHelper creHelper = new OracleDBHelper();
            OracleConnection con = creHelper.getOracleConnection();//Oracle数据库连接
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            if (creHelper.IsExistTable("INCREMENTLINEDATA"))//判断当前数据库中是否存在增量线数据表
            {
                string sql1 = string.Format("delete from USER_SDO_GEOM_METADATA where TABLE_NAME='INCREMENTLINEDATA'");//删除元数据视图中的数据表
                creHelper.sqlExecuteUnClose(sql1);
                sql1 = string.Format("drop index idx_INCREMENTLINEDATA");//删除数据表中的索引
                creHelper.sqlExecuteUnClose(sql1);
                string sql = string.Format("drop table INCREMENTLINEDATA");//若数据表存在将其删除
                creHelper.sqlExecuteUnClose(sql);
            }
            string createTableSql2 = "(objectid NUMBER(30) primary key NOT NULL,osmid NUMBER(30) ,versionid NUMBER(30),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(50),osmkey VARCHAR2(500),osmvalue VARCHAR2(500),trustvalue FLOAT,userreputation FLOAT,shape SDO_GEOMETRY,changeType VARCHAR2(30),source NUMBER(4))";
            creHelper.createTable(IncreLineTableName, createTableSql2);
            //为该数据表创建索引
            string sql2 = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID) VALUES ('INCREMENTLINEDATA', 'shape',MDSYS.SDO_DIM_ARRAY  (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050),MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050)),4326)");
            creHelper.sqlExecuteUnClose(sql2);
            string sql3 = string.Format("create index idx_INCREMENTLINEDATA on INCREMENTLINEDATA(shape) indextype is mdsys.spatial_index");//by 20180916修改
            creHelper.sqlExecuteUnClose(sql3);
        }
        /// <summary>
        /// 增量线数据提取方法函数
        /// </summary>
        public void IncreLineDataExtraction(DataTable ruleTable, DataTable incredataTable)
        {
            OracleDBHelper helper = new OracleDBHelper();
            OracleConnection con = helper.getOracleConnection();
            for (int i = 0; i < incredataTable.Rows.Count; i++)//基态数据表字段获取
            {
                this.LineExtractionpgBar.Maximum = incredataTable.Rows.Count;
                LineExtractionpgBar.Value = i+1;
                string nationEleName = "";  string elementTypeID = "";string nationCode = "";
                string key = incredataTable.Rows[i]["osmkey"].ToString();
                string value = incredataTable.Rows[i]["osmvalue"].ToString();
                string nationelename = incredataTable.Rows[i]["nationEleName"].ToString();
                try
                {
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    //循环遍历线规则表中的数据
                    for (int j = 0; j < ruleTable.Rows.Count; j++)
                    {
                        string osm_key = ruleTable.Rows[j]["osm_key"].ToString();
                        string osm_value = ruleTable.Rows[j]["osm_value"].ToString();
                        string NationEleName = ruleTable.Rows[j]["nationEleName"].ToString(); ;
                        //string ElementType = ruleTable.Rows[j]["elementType"].ToString();
                        string ElementTypeID = ruleTable.Rows[j]["elementTypeID"].ToString();
                        //string Geometry = ruleTable.Rows[j]["geometry"].ToString();
                        string NationCode = ruleTable.Rows[j]["nationcode"].ToString();
                        if (nationelename == "")
                        {
                            if (key == osm_key && value == osm_value)
                            {
                                nationEleName = NationEleName;
                                //elementType = ElementType;
                                elementTypeID = ElementTypeID;
                                //geometry = Geometry;
                                nationCode = NationCode;
                                string sqlStr2 = string.Format("update {0} set nationEleName='{1}' ,elementTypeID={2},nationCode='{3}'where osmkey='{4}' and osmvalue='{5}'", IncreLineTableName, nationEleName,elementTypeID,  nationCode, osm_key, osm_value);
                                using (OracleCommand objCommand = new OracleCommand(sqlStr2, con))
                                {
                                    //con.Open();
                                    objCommand.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        #endregion


        //#region 道路匹配的Stroke化预处理 by zbl 2018.8.14
        //private void StrokegBox_Enter(object sender, EventArgs e)
        //{
        //    List<string> strokeTables = getTableNames(conStr);//获取要stroke化处理的数据表的表名
        //    this.strokeCOMB.Items.Clear();
        //    if (strokeTables != null && strokeTables.Count > 0)
        //    {
        //        foreach (string name in strokeTables)
        //        {
        //            this.strokeCOMB.Items.Add(name);
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("对不起，您选择的数据库中无线数据表！");
        //    }
        //}
        ///// <summary>
        ///// 获取所有的表名
        ///// </summary>
        ///// <param name="conString">数据库连接字符串</param>
        ///// <returns>所有的表名</returns>
        //private List<string> getTableNames(string conString)
        //{
        //    List<string> baseTables = new List<string>();//系统自带的数据表
        //    List<string> tableNames = new List<string>();
        //    try
        //    {
        //        OracleDBHelper conHelper = new OracleDBHelper();//获取数据库帮助对象
        //        OracleConnection con = conHelper.getOracleConnection();
        //        if (con.State != ConnectionState.Open)
        //        {
        //            con.Open();
        //        }
        //        string sqlString = "select distinct table_name  from user_col_comments where lower(table_name) like '%line%' or  table_name like '%Atraffic%'or table_name like '%Awater%'or table_name like '%currentlinedata%'or table_name like '%Avegetation%'or table_name like '%Asoil%'or table_name like '%Aresidential%'or table_name like '%incrementlinedata%'order by   lower(table_name) ASC";
        //        using (OracleCommand command = new OracleCommand(sqlString, con))
        //        {
        //            OracleDataReader dr = command.ExecuteReader();
        //            while (dr.Read())
        //            {
        //                string name = dr[0].ToString();
        //                if (baseTables.Contains(name))
        //                {
        //                    continue;
        //                }
        //                else
        //                {
        //                    tableNames.Add(name);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }

        //    return tableNames;
        //}

        //private void StrokeBtn_Click(object sender, EventArgs e)
        //{
        //    double bufferRadius = 0;
        //    double telorence = 0;
        //    double threshold = 0;
        //    try
        //    {
        //        bufferRadius = Double.Parse(this.bufferRadiusTB.Text);//获取手动设置的缓冲区半径值
        //        telorence = Double.Parse(this.telorenceTB.Text);//获取手动设置的容差值
        //        threshold = Double.Parse(this.thresholdTB.Text);//获取手动设置的夹角阈值
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("请查看输入参数是否为非负实数！");
        //        Console.WriteLine(ex.ToString());
        //        return;
        //    }
        //    int mergeCount = stroke(strokeCOMB.Text, conStr, bufferRadius, telorence, threshold);
        //    MessageBox.Show("Stroke化完成！此次共处理了" + mergeCount + "条线数据！");
        //}
        ///// <summary>
        ///// 定义道路要素Stroke化处理算法函数方法
        ///// </summary>
        ///// <param name="tablename">数据表名</param>
        ///// <param name="conStr">数据库连接字符串</param>
        ///// <param name="bufferRadius">缓冲区半径</param>
        ///// <param name="tolerance">连接点容差</param>
        ///// <param name="threshold">夹角阈值</param>
        ///// <returns></returns>
        //public int stroke(string tablename, string conStr, double bufferRadius, double tolerance, double threshold)
        //{
        //    string tableName = tablename;
        //    OracleDBHelper helper = new OracleDBHelper();
        //    OracleConnection con = helper.getOracleConnection();//连接Oracle数据库
        //    DataTable table = getDataAll(tableName);//通过数据库表名获取数据表中的所有对象并存于内存中的一个数据表中
        //    int tableCount = table.Rows.Count;
        //    this.LineExtractionpgBar.Maximum = tableCount;
        //    int mergeCount = 0;//定义存储stroke化合并的数据量字段
        //    if (tableCount > 0)
        //    {
        //        List<String> deleteList = new List<string>();//存储被删除过的元素的集合
        //        for (int i = 0; i < tableCount; i++)//对数据表中的所有对象执行下面操作
        //        {
        //            if ("CURRENTLINEDATA".Equals(tableName))
        //            {
        //                if (deleteList.Contains(table.Rows[i]["osm_id"].ToString() + "_" + table.Rows[i]["version_id"].ToString()))
        //                    continue;
        //            }
        //            else
        //            {
        //                if (deleteList.Contains(table.Rows[i]["objectid"].ToString()))
        //                    continue;
        //            }
        //            LineExtractionpgBar.Value = i;//进度条更新值
        //            string geomStr = table.Rows[i]["geometry_wkt"].ToString();//获得对象的线串值
        //            //string bufferShape = table.Rows[i]["shape"].ToString();
        //            //DataTable bufferDataTable = getDataByBuffer(tableName, bufferShape, bufferRadius);//得到在第i个对象缓冲区内的所有对象

        //            DataTable bufferDataTable = getDataByBuffer(tableName, geomStr, bufferRadius);//得到在第i个对象缓冲区内的所有对象
        //            int bufferDataTableCount = bufferDataTable.Rows.Count;//将得到的缓冲区数据表内所有对象存储给bufferDataTableCount字段
        //            try
        //            {
        //                if (con.State == ConnectionState.Closed)
        //                {
        //                    con.Open();
        //                }
        //                for (int j = 0; j < bufferDataTableCount; j++)//对查询到的缓冲区对象数据表中的所有对象执行下面操作
        //                {
        //                    //Console.WriteLine("bufferDataTable表中osmid:" + bufferDataTable.Rows[j]["osmid"].ToString() + ",bufferDataTable表中version:" + bufferDataTable.Rows[j]["version"].ToString());

        //                    if ("CURRENTLINEDATA".Equals(tableName))
        //                    {
        //                        if (deleteList.Contains(table.Rows[i]["osm_id"].ToString() + "_" + table.Rows[i]["version_id"].ToString()))
        //                            continue;
        //                    }
        //                    else
        //                    {
        //                        if (deleteList.Contains(table.Rows[i]["objectid"].ToString()))
        //                            continue;
        //                    }
        //                    string bufferGeomStr = bufferDataTable.Rows[j]["geometry_wkt"].ToString();
        //                    string mergeString = strokeJudge(geomStr, bufferGeomStr, tolerance, threshold);//判断是否满足stroke条件:是否存在共同点；属性是否相同；或者夹角是否在阈值内
        //                    //string oscid = bufferDataTable.Rows[j]["oscid"].ToString();

        //                    //这里暂时定位，找到满足条件的就直接合并，不再进行查找后面的是否也有满足条件的

        //                    if (!mergeString.Equals("N"))//如果不为N（即NO）说明满足条件，返回了合并后的GeomString字段值,可以进行Stroke处理,并删除数据库中被吞并的道路，更新合并后的道路
        //                    {
        //                        //合并两条道路
        //                        String sql = "";
        //                        sql = "CURRENTLINEDATA".Equals(tableName) ? string.Format("delete from {0} where osm_id='{1}' and version_id={2}", tableName, bufferDataTable.Rows[j]["osm_id"].ToString(), Int32.Parse(bufferDataTable.Rows[j]["version_id"].ToString())) :
        //                            //string.Format("delete from {0} where objectid={1}", tableName, Int32.Parse(bufferDataTable.Rows[j]["objectid"].ToString()));
        //                        string.Format("delete from {0} where osm_id={1}", tableName, Int32.Parse(bufferDataTable.Rows[j]["osm_id"].ToString()));
        //                        helper.sqlExecute(sql);

        //                        if ("CURRENTLINEDATA".Equals(tableName))
        //                            deleteList.Add(bufferDataTable.Rows[j]["osm_id"].ToString() + "_" + bufferDataTable.Rows[j]["version_id"].ToString());
        //                        else
        //                            deleteList.Add(bufferDataTable.Rows[j]["objectid"].ToString());

        //                        sql = "CURRENTLINEDATA".Equals(tableName) ? string.Format("update {0} set shape =sdo_geometry ('{1}',4326)  where osm_id='{2}' and version_id={3}", tableName, mergeString, table.Rows[i]["osm_id"].ToString(), Int32.Parse(table.Rows[i]["version_id"].ToString())) :
        //                          string.Format("update {0} set shape =sdo_geometry ('{1}',4326))  where osm_id={2}", tableName, mergeString, Int32.Parse(table.Rows[i]["osm_id"].ToString()));
        //                        using (OracleCommand cmd = new OracleCommand(sql, con))
        //                        {
        //                            con.Open();
        //                            cmd.ExecuteNonQuery();
        //                        }
        //                        bufferDataTable.Rows.RemoveAt(j);//从获得的缓冲区内对象数据表中移除相应的行数据
        //                        bufferDataTableCount = bufferDataTable.Rows.Count;//获取移除之后的数据表总数
        //                        j = 0;
        //                        geomStr = mergeString;//为查找与没有与合并后的道路可以再次合并的做准备
        //                    }
        //                }
        //                con.Close();
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine(ex.ToString());

        //                if ("CURRENTLINEDATA".Equals(tableName))
        //                {
        //                    Console.WriteLine("osm_id==" + table.Rows[i]["osm_id"].ToString() + ",version_id==" + table.Rows[i]["version_id"].ToString());
        //                }
        //                else
        //                {
        //                    Console.WriteLine("objectid==" + table.Rows[i]["objectid"].ToString());
        //                    Console.WriteLine("osm_id==" + table.Rows[i]["osm_id"].ToString());
        //                }
        //            }
        //        }
        //        mergeCount = deleteList.Count;
        //    }
        //    else
        //    {
        //        MessageBox.Show("未获取到数据！");
        //    }
        //    return mergeCount;
        //}
        ///// <summary>
        ///// 判断两个线对象是否满足Stroke化的条件：如是否存在共同点？属性是否相同?夹角是否在阈值之内?
        ///// </summary>
        ///// <param name="geomStr"></param>
        ///// <param name="bufferGeomStr"></param>
        ///// <param name="tolerance"></param>
        ///// <param name="threshold"></param>
        ///// <returns></returns>
        //public string strokeJudge(string geomStr, string bufferGeomStr, double tolerance, double threshold)
        //{
        //    string mergeString = "";
        //    //获取线的起始点 ST_StartPoint(geometry)// 获取线的终点 ST_EndPoint(geometry)
        //    //判断两个几何对象是否相等（比如LINESTRING(0 0, 2 2)和LINESTRING(0 0, 1 1, 2 2)是相同的几何对象） ST_Equals(geometry, geometry)
        //    string pointString = geomStr.Substring(12, geomStr.Length - 13);
        //    string[] points = pointString.Split(',');
        //    string bufferPointString = bufferGeomStr.Substring(12, bufferGeomStr.Length - 13);
        //    string[] bufferPoints = bufferPointString.Split(',');

        //    string pointsStart1 = points[0];//第一个点
        //    string pointsStart2 = points[1];//第二个点
        //    string pointsEnd1 = points[points.Length - 1];//最后一个点
        //    string pointsEnd2 = points[points.Length - 2];//倒数第二个点

        //    string pointsBufferStart1 = bufferPoints[0];//第一个点
        //    string pointsBufferStart2 = bufferPoints[1];//第二个点
        //    string pointsBufferEnd1 = bufferPoints[bufferPoints.Length - 1];//最后一个点
        //    string pointsBufferEnd2 = bufferPoints[bufferPoints.Length - 2];//倒数第二个点

        //    //先判断points最后一个点和pointsBuffer的第一个点是否相同
        //    //double tolerance = 0.001;//两个点之间的距离容差
        //    //double threshold = 90;//两个直线之间的角度阈值
        //    if (strokeContactPointJudge(pointsEnd1, pointsBufferStart1, tolerance) && !strokeContactPointJudge(pointsBufferEnd1, pointsStart1, tolerance))//看第一条线的尾点和第二条线的起点是否相同，如果相同,说明两个点为连接点
        //    {
        //        if ((strokeLineAngleJudge(pointsEnd2, pointsEnd1, pointsBufferStart1, pointsBufferStart2) < threshold))//看两条线的夹角是否满足条件
        //        {
        //            mergeString = "LINESTRING(" + pointString + "," + bufferPointString + ")";//返回合并后的geometry
        //        }
        //        else
        //        {
        //            mergeString = "N";//没有满足条件返回N
        //        }
        //        //可以在这里直接合并了
        //    }
        //    else if (!strokeContactPointJudge(pointsEnd1, pointsBufferStart1, tolerance) && strokeContactPointJudge(pointsBufferEnd1, pointsStart1, tolerance))//看第二条线的尾点和第一条线的起点是否相同，如果相同,说明两个点为连接点
        //    {
        //        if (strokeLineAngleJudge(pointsBufferEnd2, pointsBufferEnd1, pointsStart1, pointsStart2) < threshold)//看两条线的夹角是否满足条件
        //        {
        //            mergeString = "LINESTRING(" + bufferPointString + "," + pointString + ")";//返回合并后的geometry
        //        }
        //        else
        //        {
        //            mergeString = "N";//没有满足条件返回N
        //        }
        //    }
        //    else if (strokeContactPointJudge(pointsStart1, pointsBufferStart1, tolerance) && !strokeContactPointJudge(pointsEnd1, pointsBufferEnd1, tolerance))//第一个点一样，最后一个点不一样，把第一条线的第一个点作为起点，最后一个点作为尾点
        //    {
        //        if ((strokeLineAngleJudge(pointsEnd2, pointsEnd1, pointsBufferStart1, pointsBufferStart2) < threshold))//看两条线的夹角是否满足条件
        //        {
        //            Console.WriteLine("pointsReverse(pointString):" + pointsReverse(pointString));
        //            //mergeString = "LINESTRING(" + pointString.Reverse().ToString() + "," + bufferPointString + ")";//返回合并后的geometry
        //            mergeString = "LINESTRING(" + pointsReverse(pointString) + "," + bufferPointString + ")";//返回合并后的geometry
        //        }
        //        else
        //        {
        //            mergeString = "N";//没有满足条件返回N
        //        }
        //    }
        //    else if (!strokeContactPointJudge(pointsStart1, pointsBufferStart1, tolerance) && strokeContactPointJudge(pointsEnd1, pointsBufferEnd1, tolerance))//最后一个点一样，第一个点不一样，把第一条线的第一个点作为起点，最后一个点作为尾点
        //    {
        //        if ((strokeLineAngleJudge(pointsEnd2, pointsEnd1, pointsBufferEnd1, pointsBufferEnd2) < threshold))//看两条线的夹角是否满足条件
        //        {
        //            Console.WriteLine("pointsReverse(bufferPointString):" + pointsReverse(bufferPointString));
        //            //mergeString = "LINESTRING(" + pointString + "," + bufferPointString.Reverse().ToString() + ")";//返回合并后的geometry
        //            mergeString = "LINESTRING(" + pointString + "," + pointsReverse(bufferPointString) + ")";//返回合并后的geometry
        //        }
        //        else
        //        {
        //            mergeString = "N";//没有满足条件返回N
        //        }
        //    }
        //    else
        //    {
        //        mergeString = "N";//没有满足条件返回N
        //    }
        //    return mergeString;//返回合并字符串
        //}

        ///// <summary>
        ///// 点串的反转
        ///// </summary>
        ///// <param name="inputPointString"></param>
        ///// <returns></returns>
        //public string pointsReverse(string inputPointString)
        //{
        //    string[] inputPoints = inputPointString.Split(',');
        //    string outPointString = "";
        //    for (int i = inputPoints.Length - 1; i >= 0; i--)
        //    {
        //        outPointString += inputPoints[i] + ",";
        //    }
        //    outPointString = outPointString.Substring(0, outPointString.Length - 1);
        //    return outPointString;
        //}
        ///// <summary>
        ///// 判断连接点是否相同
        ///// </summary>
        ///// <param name="pointA"></param>
        ///// <param name="pointB"></param>
        ///// <param name="tolerance"></param>
        ///// <returns></returns>
        //public Boolean strokeContactPointJudge(string pointA, string pointB, double tolerance)
        //{
        //    pointA = pointA.Trim();
        //    string[] pointsA = pointA.Split(' ');
        //    double pointAx = Double.Parse(pointsA[0].ToString());
        //    double pointAy = Double.Parse(pointsA[1].ToString());
        //    pointB = pointB.Trim();
        //    string[] pointsB = pointB.Split(' ');
        //    double pointBx = Double.Parse(pointsB[0].ToString());
        //    double pointBy = Double.Parse(pointsB[1].ToString());
        //    return Math.Max(pointAx, pointBx) - Math.Min(pointAx, pointBx) <= tolerance && Math.Max(pointAy, pointBy) - Math.Min(pointAy, pointBy) <= tolerance ? true : false;
        //}
        ///// <summary>
        ///// 计算两条线的夹角//返回夹角值
        ///// </summary>
        ///// <param name="pointA"></param>
        ///// <param name="pointB"></param>
        ///// <param name="bufferPointA"></param>
        ///// <param name="bufferPointB"></param>
        ///// <returns></returns>
        //public double strokeLineAngleJudge(string pointA, string pointB, string bufferPointA, string bufferPointB)
        //{
        //    pointA = pointA.Trim();
        //    string[] pointsA = pointA.Split(' ');
        //    double pointAx = Double.Parse(pointsA[0].ToString());
        //    double pointAy = Double.Parse(pointsA[1].ToString());
        //    pointB = pointB.Trim();
        //    string[] pointsB = pointB.Split(' ');
        //    double pointBx = Double.Parse(pointsB[0].ToString());
        //    double pointBy = Double.Parse(pointsB[1].ToString());

        //    bufferPointA = bufferPointA.Trim();
        //    string[] bufferPointsA = bufferPointA.Split(' ');
        //    double bufferPointAx = Double.Parse(bufferPointsA[0].ToString());
        //    double bufferPointAy = Double.Parse(bufferPointsA[1].ToString());
        //    bufferPointB = bufferPointB.Trim();
        //    string[] bufferPointsB = bufferPointB.Split(' ');
        //    double bufferPointBx = Double.Parse(bufferPointsB[0].ToString());
        //    double bufferPointBy = Double.Parse(bufferPointsB[1].ToString());

        //    return strokeLineAngleJudgeByCoordinate(pointAx, pointAy, pointBx, pointBy, bufferPointAx, bufferPointAy, bufferPointBx, bufferPointBy);
        //}
        ///// <summary>
        ///// 通过坐标和方向计算两直线向量之间的角度
        ///// </summary>
        //public double strokeLineAngleJudgeByCoordinate(double startAx, double startAy, double endAx, double endAy, double startBx, double startBy, double endBx, double endBy)
        //{
        //    //vector_a = (endAx - startAx,endAy-startAy);
        //    //vector_b = (endBx - startBx,endBy-startBy);
        //    double vector_ax = endAx - startAx;
        //    double vector_ay = endAy - startAy;

        //    double vector_bx = endBx - startBx;
        //    double vector_by = endBy - startBy;

        //    double vector_aXvector_b = (vector_ax * vector_bx + vector_ay * vector_by);
        //    double vector_aLength = Math.Sqrt(Math.Abs(vector_ax * vector_ax) + Math.Abs(vector_ay * vector_ay));
        //    double vector_bLength = Math.Sqrt(Math.Abs(vector_bx * vector_bx) + Math.Abs(vector_by * vector_by));
        //    double cosθ = vector_aXvector_b / (vector_aLength * vector_bLength);

        //    return (Math.Acos(cosθ)) * 180 / Math.PI;//返回的是弧度值，需要乘以180/Math.PI转为角度

        //}
        //public Boolean strokeLineGradeJudge()//判断道路级别是否相同
        //{
        //    return true;
        //}
        ///// <summary>
        ///// 通过缓冲区半径和对象geom得到缓冲区内对象
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <param name="geomStr"></param>
        ///// <returns></returns>
        //public DataTable getDataByBuffer(string tableName, string geomStr, double bufferRadius)
        //{
        //    DataTable bufferData = new DataTable();//读取某个线对象缓冲区内所有对象的数据集表
        //    OracleDBHelper helper = new OracleDBHelper();
        //    OracleConnection con = helper.getOracleConnection();
        //    try
        //    {
        //        string sql = "";
        //        if (geomStr.Length > 4000)
        //        {
        //            //return bufferData;
        //        }
        //        else
        //        {
        //            sql = tableName.Equals("CURRENTLINEDATA") ? string.Format("select osm_id,version_id,NationEleName,ElementTypeID,sdo_geometry.get_wkt(shape)geometry_wkt from {0} where SDO_WITHIN_DISTANCE(shape,sdo_geometry ('{1}',4326),'distance={2},unit=m')='TRUE'", tableName, geomStr, bufferRadius)
        //                : string.Format("select osm_id,NationEleName, sdo_geometry.get_wkt(shape)geometry_wkt from {0} where SDO_WITHIN_DISTANCE(geom,sdo_geometry ('{1}',4326)) ,'distance={2},unit=m')='TRUE'", tableName, geomStr, bufferRadius);//得到缓冲区内对象
        //            //也可以用ST_Touches()函数代替ST_DWithin()判断两个几何对象的边缘是否接触 ST_Touches(geometry, geometry)

        //            //bufferData.Columns.Add("objectid");
        //            bufferData.Columns.Add("osm_id");
        //            bufferData.Columns.Add("version_id");
        //            bufferData.Columns.Add("NationEleName");
        //            bufferData.Columns.Add("ElementTypeID");
        //            bufferData.Columns.Add("geometry_wkt");
        //            //bufferData.Columns.Add("geometry_wkt",typeof(String));//可能出错的wkt
        //            using (OracleDataReader rd = helper.queryReader(sql))
        //            {

        //                while (rd.Read())
        //                {
        //                    DataRow bufferDataRow = bufferData.NewRow();
        //                    if (tableName.Equals("CURRENTLINEDATA"))
        //                    {
        //                        bufferDataRow["osm_id"] = rd["osm_id"].ToString();
        //                        bufferDataRow["version_id"] = rd["version_id"].ToString();
        //                    }
        //                    else
        //                    {
        //                        bufferDataRow["objectid"] = rd["objectid"].ToString();
        //                        bufferDataRow["osm_id"] = rd["osm_id"].ToString();
        //                    }
        //                    bufferDataRow["NationEleName"] = rd["NationEleName"].ToString();
        //                    bufferDataRow["ElementTypeID"] = rd["ElementTypeID"].ToString();
        //                    bufferDataRow["geometry_wkt"] = rd["geometry_wkt"].ToString();
        //                    bufferData.Rows.Add(bufferDataRow);
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //    return bufferData;
        //}
        ///// <summary>
        ///// 通过数据库表名获得表内所有对象
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <returns></returns>
        //public DataTable getDataAll(string tableName)
        //{
        //    DataTable table = new DataTable();
        //    try
        //    {
        //        string sql = "";
        //        OracleDBHelper helper = new OracleDBHelper();
        //        if ("CURRENTLINEDATA".Equals(tableName))
        //        {
        //            sql = string.Format("select osm_id,version_id,NationEleName,ElementTypeID,sdo_geometry.get_wkt(shape)geometry_wkt from {0} order by osm_id,version_id ", tableName);
        //            //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('174857072','563632939','55414786') order by osmid,version ", tableName);//测试整体匹配部分
        //            //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('562259740','562216181','562787930','563632939') order by osmid,version ", tableName);//测试局部匹配部分
        //            //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('562787930') order by osmid,version ", tableName);//测试局部匹配部分_包含或被包含完全替代情况_            成功！
        //            //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('92361574') order by osmid,version ", tableName);//测试局部匹配部分_头顶头没有共同部分情况                  成功！
        //            //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('203711643') order by osmid,version ", tableName);//测试局部匹配部分_错位情况           当整体匹配hausdorff距离阈值为1，局部匹配hausdorff距离阈值为5时，局部替换整体阈值大于4时整体匹配成功！
        //            //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('357846417') order by osmid,version ", tableName);//测试局部匹配部分_错位情况                                成功！
        //            //sql = string.Format("select osmid,version,roadname, astext(shape) geomStr_wkt from {0} where osmid in ('329760827') order by osmid,version ", tableName);//测试局部匹配部分_一方有拐点的情况

        //            table.Columns.Add("osm_id");
        //            table.Columns.Add("version_id");
        //            table.Columns.Add("NationEleName");
        //            table.Columns.Add("ElementTypeID");
        //            //table.Columns.Add("shape");
        //            table.Columns.Add("geometry_wkt");
        //        }
        //        else
        //        {
        //            //sql = string.Format("select objectid,roadname,sdo_geometry.get_wkt(shape)geometry_wkt from {0} order by objectid ", tableName);
        //            sql = string.Format("select objectid,NationEleName,,ElementTypeID,sdo_geometry.get_wkt(shape)geometry_wkt from {0} order by objectid  ", tableName);

        //            table.Columns.Add("objectid");
        //            table.Columns.Add("NationEleName");
        //            table.Columns.Add("geometry_wkt");
        //        }

        //        using (OracleDataReader rd = helper.queryReader(sql))
        //        {
        //            while (rd.Read())
        //            {
        //                DataRow tableRow = table.NewRow();
        //                if ("CURRENTLINEDATA".Equals(tableName))
        //                {
        //                    tableRow["osm_id"] = rd["osm_id"].ToString();
        //                    tableRow["version_id"] = rd["version_id"].ToString();
        //                    tableRow["NationEleName"] = rd["NationEleName"].ToString();
        //                    tableRow["ElementTypeID"] = rd["ElementTypeID"].ToString();
        //                    //tableRow["shape"] = rd["shape"].ToString();
        //                    tableRow["geometry_wkt"] = rd["geometry_wkt"].ToString();
        //                }
        //                else
        //                {
        //                    tableRow["objectid"] = rd["objectid"].ToString();
        //                    tableRow["NationEleName"] = rd["NationEleName"].ToString();
        //                    tableRow["geometry_wkt"] = rd["geometry_wkt"].ToString();
        //                }
        //                table.Rows.Add(tableRow);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //    return table;
        //}

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    this.Close();
        //}
        //#endregion
    }
}
