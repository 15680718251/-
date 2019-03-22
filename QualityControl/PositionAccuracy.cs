using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using GIS.Converters.WellKnownText;
using GIS.Geometries;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;
using System.Collections;
using ESRI.ArcGIS.Controls;
using System.IO;
using System.Threading;
using Oracle.ManagedDataAccess.Types;
using ESRI.ArcGIS.Carto;
using System.Text;
//oracle的引用


namespace QualityControl
{
    public partial class PositionAccuracy : Form
    {
        private AxMapControl axMapcontrol;
        private AxTOCControl axTOCControl;
        AddMap showmap = new AddMap();
        DateTime start;
        StreamWriter sw;
        string path = @"..\..\..\fileImport\areaPointDistance.txt";
        //ArrayList arrayListMainobjectid = new ArrayList();
        //ArrayList arrayListSearchobjectid = new ArrayList();

        //定义全局变量
        //int a = 0;//进度条的值
        //int max = 0;
        //string info;
        public double pointDistanceThreshold = 5;//点对之间的距离阈值米
        public double areaThreshold = 1000;//主多边形的面积阈值
        public double lengthAreaRatioThreshold = 1;//点对距离与面积商阈值应该小于1
        public double nearestPointDistance = 10;//最近点距离阈值3米（点到多边形或多边形之间的距离）
        public static double ratio;

        private OracleDBHelper conHelper = new OracleDBHelper();
        public OracleDBHelper ConHelper
        {
            get { return conHelper; }
            set { conHelper = value; }
        }

        public PositionAccuracy(AxMapControl axMapcontrol, AxTOCControl axTOCControl, OracleDBHelper ConHelper)
        {//董海峰201809
            
            InitializeComponent();
            this.axMapcontrol = axMapcontrol;
            this.axTOCControl = axTOCControl;
            this.ConHelper = ConHelper;

            //createStoredProduce();//创建存储过程
            //copyTable();//备份表
            ////createIndex();//创建索引//不需要创建索引了

            //Fun1();

        }

        public void copyTable()
        {
            string[] tableNameList = { "RESIDENTIAL_AREA", "SOIL_AREA", "VEGETATION_AREA", "WATER_AREA", "TRAFFIC_AREA", "RESIDENTIAL_LINE", "SOIL_LINE", "TRAFFIC_LINE", "VEGETATION_LINE", "WATER_LINE" };
            string SQL;
            foreach (string tablename in tableNameList)
            {
                SQL = string.Format("drop  table {0}_COPY", tablename);//删除
                conHelper.sqlExecuteUnClose(SQL);

                SQL = string.Format("create  table {0}_COPY as select * from {0}", tablename);//更新之前复制一份
                conHelper.sqlExecuteUnClose(SQL);
            }
        }

        //线已经创建了索引 只需要对面创建索引
        public void createIndex()
        {
            string[] tableName = { "RESIDENTIAL_NEWAREA", "SOIL_NEWAREA", "VEGETATION_NEWAREA", "WATER_NEWAREA", "TRAFFIC_NEWAREA" };
            string SQL;
            foreach (string tablename in tableName)
            {
                //string SQL = string.Format("select count(*) from user_tables where table_name = '{0}_COPY'", tablename);
                // //OracleDBHelper conHelper = new OracleDBHelper();
                // using (OracleDataReader dr = conHelper.queryReader(SQL))
                // {
                //     while (dr.Read())
                //     {
                //         if (dr.GetInt32(0) > 0)
                //         {//先删除再复制
                //             SQL = string.Format("drop  table {0}_COPY", tablename);//删除
                //             conHelper.sqlExecuteUnClose(SQL);

                //             SQL = string.Format("create  table {0}_COPY as select * from {0}", tablename);//更新之前复制一份
                //             conHelper.sqlExecuteUnClose(SQL);
                //         }

                //         else
                //         {//直接复制
                //             SQL = string.Format("create  table {0}_COPY as select * from {0}", tablename);//更新之前复制一份
                //             conHelper.sqlExecuteUnClose(SQL);

                //         }
                //     }
                // }

                if (conHelper.IsExistSpatialTable(tablename))
                {//如果元数据表中已经存在则直接创建
                    SQL = string.Format("create index idx_{0} on {0}(shape) indextype is mdsys.spatial_index", tablename);//为更新前的表创建索引
                    conHelper.sqlExecuteUnClose(SQL);
                }
                else
                {//否则 先创建元数据 在创建索引
                    SQL = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('{0}', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", tablename);
                    conHelper.sqlExecuteUnClose(SQL);
                    SQL = string.Format("create index idx_{0} on {0}(shape) indextype is mdsys.spatial_index", tablename);//为更新前的表创建索引
                    conHelper.sqlExecuteUnClose(SQL);

                }
                ////备份表不需要创建索引
                //if (conHelper.IsExistSpatialTable(tablename + "_COPY"))
                //{
                //    SQL = string.Format("create index idx_{0}_COPY on {0}_COPY(shape) indextype is mdsys.spatial_index", tablename);//为复制的表创建索引
                //    conHelper.sqlExecuteUnClose(SQL);
                //}
                //else
                //{
                //    SQL = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('{0}_COPY', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", tablename);
                //    conHelper.sqlExecuteUnClose(SQL);
                //    SQL = string.Format("create index idx_{0}_COPY on {0}_COPY(shape) indextype is mdsys.spatial_index", tablename);//为复制的表创建索引
                //    conHelper.sqlExecuteUnClose(SQL);
                //}

            }
        }


        public void createStoredProduce()
        {
            //创建存储过程将重组的wkt导入数据库
            string[] tableNameList = { "RESIDENTIAL_AREA", "SOIL_AREA", "VEGETATION_AREA", "WATER_AREA", "TRAFFIC_AREA", "RESIDENTIAL_LINE", "SOIL_LINE", "TRAFFIC_LINE", "VEGETATION_LINE", "WATER_LINE" };
            foreach (string tableName in tableNameList)
            {
                //string sql = string.Format("select count(*) from user_source where type='PROCEDURE' and name='UPDATEPRODUCE_{0}'", tableName);
                //OracleDataReader dr = conHelper.queryReader(sql);
                //if (dr.Read())
                //{
                //    int count = dr.GetInt32(0);
                //    if (count < 1)
                //    {
                OracleDBHelper db = new OracleDBHelper();
                OracleConnection myConnection = db.getOracleConnection();
                if (myConnection.State == ConnectionState.Closed)
                {
                    myConnection.Open();
                }
                string sql = string.Format(@"create or replace procedure UP_{0}
                        (objid number,
                        wkt clob)
                        is
                          updatewkt clob;
                        begin
                          updatewkt:=wkt;
                          update {0} set shape=sdo_geometry(updatewkt,4326) where objectid=objid;
                          update {0} set matchid=objid where objectid=objid;
                        commit;
                        end;", tableName);

                OracleCommand cmd = new OracleCommand(sql, myConnection);
                cmd.ExecuteScalar();//执行command命令
            }
            //    }
            //}


        }
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




        #region 废弃
        /// <summary>
        /// 对更新数据建立缓冲区
        /// </summary>
        /// <param name="bufferRadius"> 缓冲距离</param>
        /// <param name="sourceTable">缓冲源表 </param>
        /// <param name="bufferTable">缓冲目标表</param>
        /// <returns></returns>
        //public DataTable buffer(double bufferRadius, string sourceTable, string bufferTable)
        //{
        //    DataTable dataTableA = readData(sourceTable);//读取更新数据表
        //    //DataTable dataTableB = new DataTable();//新建表用于存放更新数据表缓冲区内的增量数据表中内容
        //    DataTable tableBufferResult = new DataTable();//新建表将更新数据和缓冲到的增量数据存放在一个表中

        //    string geometry_wktA = null;
        //    string osmidA = null;
        //    for (int i = 0; i < dataTableA.Rows.Count; i++)
        //    {
        //        geometry_wktA = dataTableA.Rows[i]["geometry_wkt"].ToString();
        //        osmidA = dataTableA.Rows[i]["objectid"].ToString();
        //        string sql = string.Format("select aa.osmidB, aa.shape geometry_wktB  from {0} aa where sdo_within_distance(aa.shape, sdo_geometry('{1}',31297), 'distance={2},unit=meter')='TRUE'", bufferTable, geometry_wktA, bufferRadius);
        //        OracleDBHelper conHelper = new OracleDBHelper();
        //        using (OracleDataReader dr = conHelper.queryReader(sql))
        //        {
        //            while (dr.Read())
        //            {
        //                DataRow dataTableRow = tableBufferResult.NewRow();
        //                dataTableRow["osmidB"] = dr["osmidB"].ToString();
        //                dataTableRow["geometry_wktB"] = dr["geometry_wktB"].ToString();
        //                dataTableRow["osmidA"] = osmidA;//不需要担心行数不一致问题
        //                dataTableRow["geometry_wktA"] = geometry_wktA;

        //                tableBufferResult.Rows.Add(dataTableRow);
        //            }
        //        }
        //    }
        //    return tableBufferResult;
        //}
        #endregion


        //计算点之间的距离
        public DataTable pointDiffer(string osctable, string osmtable)
        {
            //textBox1.AppendText("请稍后，正在计算点距离！\r\n");
            DataTable tableMatchResult = match(osctable, osmtable);
            DataTable pointDiffer = new DataTable();
            pointDiffer.Columns.Add("differ", typeof(double));

            for (int i = 0; i < tableMatchResult.Rows.Count; i++)
            {
                //a++;
                //progressBar1.Value++;
                string matchid = tableMatchResult.Rows[i]["matchid"].ToString();
                //string geometry_wktOSC = tableMatchResult.Rows[i]["geometry_wktOSC"].ToString();
                //string geometry_wktOSM = tableMatchResult.Rows[i]["geometry_wktOSM"].ToString();
                //string sql = string.Format("select (sdo_geom.sdo_length((select shape from {1} where objectid={0}),0.05) + sdo_geom.sdo_length((select shape from {2} where objectid={0}),0.05)) differ from daul", objectid, osctable, osmtable);
                string sql = string.Format("select SDO_GEOM.SDO_DISTANCE((select shape from {1} where matchid={0}),(select shape from {2} where matchid={0}),0.05,'unit=M') differ from daul", matchid, osctable, osmtable);
                OracleDBHelper conHelper = new OracleDBHelper();
                using (OracleDataReader dr = conHelper.queryReader(sql))
                {
                    while (dr.Read())
                    {
                        //max++;
                        DataRow dataTableRow = pointDiffer.NewRow();
                        //if (Convert.ToDouble(dr["differ"]) == 0)
                        //{
                        //    dataTableRow["differ"] = 0;
                        //}
                        if (dr.IsDBNull(0))
                        {
                            dataTableRow["differ"] = 0;
                        }
                        else
                        {
                            dataTableRow["differ"] = dr["differ"];
                        }
                        pointDiffer.Rows.Add(dataTableRow);
                    }
                }
            }
            //dataGridView1.DataSource = pointDiffer;
            return pointDiffer;
        }

        /// <summary>
        /// 线之间的距离
        /// </summary>
        public DataTable lineDiffer(string tableBefore, string tableAfter)
        {
            //textBox1.AppendText("请稍后，正在计算线距离！\r\n");
            DataTable lineDiffer = new DataTable();
            lineDiffer.Columns.Add("differ", typeof(double));

            string sql = string.Format("select objectid from {0} where matchid=objectid", tableAfter);
            using (OracleDataReader dr = conHelper.queryReader(sql))
            {
                while (dr.Read())
                {

                    sql = string.Format("select abs(( select sdo_geom.sdo_length(shape,0.05) from {1} where objectid ={0})-(select sdo_geom.sdo_length(shape,0.05) from {2} where objectid ={0})) length from daul", dr.GetInt32(0), tableBefore, tableAfter);

                    using (OracleDataReader dr1 = OracleDBHelper.ExecuteReader(sql))
                    {
                        if (dr1.Read())
                        {
                            DataRow dataTableRow = lineDiffer.NewRow();

                            if (dr1.IsDBNull(0))
                            {
                                dataTableRow["differ"] = 0;
                            }
                            else
                            {
                                if (dr1.GetDouble(0) < nearestPointDistance)
                                {
                                    dataTableRow["differ"] = dr1.GetDouble(0);
                                    //Console.WriteLine(dataTableRow["differ"]);
                                    //Console.WriteLine(sql);
                                }
                               
                            }
                            lineDiffer.Rows.Add(dataTableRow);

                        }
                    }


                }
            }
            return lineDiffer;
        }


        public DataTable areaDiffer(string tableBefore, string tableAfter)
        {
           

            //this.textBox1.AppendText("请稍后，正在计算面距离！\r\n");
            DataTable polygonDiffer = new DataTable();
            polygonDiffer.Columns.Add("differ", typeof(double));

            #region 面的点位
            string[] contents = File.ReadAllLines(path, Encoding.Default);//读取外部文件所有行的信息
            ArrayList arrar = new ArrayList(contents);

            string[] tableNameList = { "RESIDENTIAL_AREA", "TRAFFIC_AREA", "SOIL_AREA", "VEGETATION_AREA", "WATER_AREA" };
            int thisIndex = 0;
            List<int> indexList = new List<int>();
            foreach (string tableName in tableNameList)
            {
                indexList.Add(arrar.IndexOf(tableName));

                if (tableName == tableAfter)
                {
                    thisIndex = arrar.IndexOf(tableName);
                }
            }
            indexList.Sort();

            int ii = indexList.IndexOf(thisIndex);
            if (ii == 4)
            {
                for (int i = thisIndex + 1; i < contents.Count(); i++)
                {
                    DataRow dataTableRow = polygonDiffer.NewRow();
                    dataTableRow["differ"] = Convert.ToDouble(contents[i]);
                    polygonDiffer.Rows.Add(dataTableRow);
                }
            }
            else
            {
                int j = indexList[ii + 1];
                for (int i = thisIndex + 1; i < j; i++)
                {
                    DataRow dataTableRow = polygonDiffer.NewRow();
                    dataTableRow["differ"] = Convert.ToDouble(contents[i]);
                    polygonDiffer.Rows.Add(dataTableRow);
                }
            }
            
            #endregion

            #region 面的位置
            //string sql = string.Format("select matchid from {0} where matchid is not null", tableAfter);
            //using (OracleDataReader dr = conHelper.queryReader(sql))
            //{
            //    while (dr.Read())
            //    {

            //        sql = string.Format("select sdo_geom.sdo_area( SDO_GEOM.SDO_difference ( (select shape from {1} where matchid={0}), (select shape from {2} where objectID={0}),0.05),0.05,'unit=SQ_METER') differ from daul", dr.GetInt32(0), tableAfter, tableBefore);

            //        using (OracleDataReader dr1 = conHelper.queryReader(sql))
            //        {
            //            while (dr1.Read())
            //            {
            //                DataRow dataTableRow = polygonDiffer.NewRow();
            //                if (dr1.IsDBNull(0))
            //                {
            //                    dataTableRow["differ"] = 0;
            //                }
            //                else
            //                {
            //                    dataTableRow["differ"] = Convert.ToDouble(dr1["differ"]);
            //                }
            //                polygonDiffer.Rows.Add(dataTableRow);
            //            }
            //        }
            //    }
            //}
            #endregion
            for (int i = 0; i < polygonDiffer.Rows.Count; i++)
            {
                Console.WriteLine(polygonDiffer.Rows[i]["differ"].ToString());
            }
                return polygonDiffer;


        }
        public DataTable readDataTableBefore(string tablenameBefore, DataTable TableAfter)
        {
            DataTable TableBefore = new DataTable();
            TableBefore.Columns.Add("matchid");
            TableBefore.Columns.Add("geometry_wkt");
            for (int j = 0; j < TableAfter.Rows.Count; j++)
            {
                DataRow row = TableBefore.NewRow();
                row["matchid"] = TableAfter.Rows[j]["matchid"];

                string sql = string.Format("SELECT sdo_geometry.get_wkt(shape) geometry_wkt FROM {1} where objectid= {0}", row["matchid"], tablenameBefore);
                using (OracleDataReader dr = conHelper.queryReader(sql))
                {
                    if (dr.Read())
                    {
                        row["geometry_wkt"] = dr["geometry_wkt"].ToString();
                    }
                }
                TableBefore.Rows.Add(row);
            }
            return TableBefore;
        }
        //根据objectid objectid匹配数据
        public DataTable match(string tablenameBefore, string tablenameAfter)
        {
            //this.textBox1.AppendText("请稍后，正在匹配数据！\r\n");
            DataTable TableAfter = readData(tablenameAfter);
            DataTable TableBefore = readDataTableBefore(tablenameBefore, TableAfter);

            DataTable dt = new DataTable();

            dt.Columns.Add("matchid");
            dt.Columns.Add("geometry_wktOSC");
            dt.Columns.Add("geometry_wktOSM");
            for (int i = 0; i < TableBefore.Rows.Count; i++)
            {
                for (int j = 0; j < TableAfter.Rows.Count; j++)
                {
                    if (TableAfter.Rows[j]["matchid"].ToString() == TableBefore.Rows[i]["matchid"].ToString())
                    {
                        //a++;
                        //progressBar1.Value++;
                        DataRow row = dt.NewRow();
                        row["matchid"] = TableAfter.Rows[j]["matchid"];
                        row["geometry_wktOSM"] = TableAfter.Rows[j]["geometry_wkt"];
                        row["geometry_wktOSC"] = TableBefore.Rows[i]["geometry_wkt"];
                        dt.Rows.Add(row);
                        break;
                    }
                }
            }
            if (dt.Rows.Count == 0)
            {
                Console.WriteLine(tablenameBefore + "和" + tablenameAfter + "匹配结果为零");
            }
            //max += dt.Rows.Count;
            Console.WriteLine("匹配完成");
            return dt;
        }

        /// <summary>
        /// 读取数据表中所有数据，并以DataTable形式返回
        /// </summary>
        /// <param name="tableName">数据表中的表名称</param>
        /// <returns></returns>
        public DataTable readData(string tableName)
        {
            //textBox1.AppendText("请稍后，正在读取数据！\r\n");
            DataTable dataTable = new DataTable();//从数据表中读取的所有数据

            dataTable.Columns.Add("matchid");

            //dataTable.Columns.Add("version");
            //dataTable.Columns.Add("fc");
            //dataTable.Columns.Add("dsg");
            dataTable.Columns.Add("geometry_wkt");
            OracleDBHelper conHelper = new OracleDBHelper();

            string sql = string.Format("SELECT matchid,sdo_geometry.get_wkt(shape) geometry_wkt FROM {0} where matchid is not null", tableName);
            using (OracleDataReader dr = conHelper.queryReader(sql))
            {
                while (dr.Read())
                {

                    //a++;
                    //progressBar1.Value++;
                    DataRow dataTableRow = dataTable.NewRow();


                    dataTableRow["matchid"] = dr["matchid"].ToString();

                    //dataTableRow["version"] = dr["version"].ToString();
                    //dataTableRow["fc"] = dr["fc"].ToString();
                    //dataTableRow["dsg"] = dr["dsg"].ToString();
                    dataTableRow["geometry_wkt"] = dr["geometry_wkt"].ToString();

                    dataTable.Rows.Add(dataTableRow);
                    //max += dataTable.Rows.Count;
                }
            }
            Console.WriteLine("读取完成");
            return dataTable;
        }

        /// <summary>
        /// 计算各指标
        /// </summary>
        /// <param name="dataTableDiffer">存放点之间的距离，线之间的距离，面之间的面积差的数据表DataTable</param>
        public ArrayList calculateIndex(DataTable dataTableDiffer)
        {
            ArrayList List = new ArrayList();
            if (dataTableDiffer.Rows.Count == 0)
            {
                List.Add(0);
                List.Add(0);
                List.Add(0);
                List.Add(0);
                List.Add(0);
            }
            else
            {

                //平均
                object average = dataTableDiffer.Compute("avg(differ)", "");
                Console.WriteLine("average=" + average);
                List.Add(average);
                //最小
                object min = dataTableDiffer.Compute("min(differ)", "");
                Console.WriteLine("min=" + min);
                List.Add(min);
                //最大
                object max = dataTableDiffer.Compute("max(differ)", "");
                Console.WriteLine("max=" + max);
                List.Add(max);
                //标准偏差
                object standardDeviation = dataTableDiffer.Compute("StDev(differ)", "");
                if (standardDeviation == DBNull.Value)
                {
                    List.Add(0);
                }
                else
                {
                    List.Add(standardDeviation);
                }
                Console.WriteLine("standardDeviation=" + standardDeviation);

                //方差
                object variance = dataTableDiffer.Compute("Var(differ)", "");
                if (variance == DBNull.Value)
                {
                    List.Add(0);
                }
                else
                {
                    List.Add(variance);
                }
                Console.WriteLine("variance=" + variance);

            }
            return List;
        }


        private void button1_Click(object sender, EventArgs e)
        {

            ////progressBar1.Minimum = 0;
            ////progressBar1.Maximum = 1000000;
            ////progressBar1.Value = a;
            DataTable datatable = new DataTable();
            datatable.Columns.Add("要素类型");
            datatable.Columns.Add("平均值");
            datatable.Columns.Add("最小值");
            datatable.Columns.Add("最大值");
            datatable.Columns.Add("标准偏差");
            datatable.Columns.Add("方差");

            string[] elementType = { "RESIDENTIAL", "TRAFFIC", "SOIL", "VEGETATION", "WATER" };
            foreach (string ELE in elementType)
            {
                textBox1.Text = ("正在计算" + ELE + "要素位置改进精度。。。。\r\n");
                DataRow row = datatable.NewRow();

                DataTable areaDiffertable = areaDiffer(ELE + "_AREA_COPY", ELE + "_AREA");
                Console.WriteLine(areaDiffertable.Rows.Count);

                DataTable lineDiffertable = lineDiffer(ELE + "_LINE_copy", ELE + "_LINE");
                Console.WriteLine(lineDiffertable.Rows.Count);

                DataTable dt = MergeDataTable(areaDiffertable, lineDiffertable, "differ");
                Console.WriteLine(dt.Rows.Count);

                ArrayList list = calculateIndex(dt);

               //ArrayList listArea = calculateIndex(areaDiffer(ELE + "_AREA_COPY", ELE + "_AREA"));
               //ArrayList listLine = calculateIndex(lineDiffer(ELE + "_LINE_copy", ELE + "_LINE"));
               ////ArrayList listPoint = calculateIndex(pointDiffer(ELE + "_POINT_copy", ELE + "_POINT"));
               //ArrayList list = new ArrayList();
               //for (int i = 0; i < listArea.Count; i++)
               //{
               //    //Console.WriteLine(Convert.ToDouble(listArea[i]));
               //    //Console.WriteLine(Convert.ToDouble(listLine[i]));
               //    //Console.WriteLine(Convert.ToDouble(listPoint[i]));
               //    //double db= Convert.ToDouble(listArea[i]) + Convert.ToDouble(listLine[i]) + Convert.ToDouble(listPoint[i]);
               //    //object db1 = db / 3;
               //    //list.Add(db1);
               //    list.Add(Math.Round((Convert.ToDouble(listArea[i]) + Convert.ToDouble(listLine[i])) / 2.0, 2));
               //    //list.Add(Math.Round(Convert.ToDouble(listArea[i]), 2));
               //    //list.Add(Math.Round((Convert.ToDouble(listArea[i])+ Convert.ToDouble(listPoint[i])) / 3.0, 2));
               //}

                row["要素类型"] = ELE;
                row["平均值"] = Math.Round(Convert.ToDouble(list[0]),3);
                row["最小值"] =Math.Round(Convert.ToDouble( list[1]),3);
                row["最大值"] = Math.Round(Convert.ToDouble(list[2]),3);
                row["标准偏差"] = Math.Round(Convert.ToDouble(list[3]),3);
                row["方差"] = Math.Round(Convert.ToDouble(list[4]), 3);
                datatable.Rows.Add(row);
            }

            dataGridView1.DataSource = datatable;

            for (int i = 0; i < datatable.Rows.Count; i++)
            {
                if (datatable.Rows[i]["标准偏差"] == DBNull.Value)
                {
                    datatable.Rows[i]["标准偏差"] = 0;
                }
                ratio += (Convert.ToDouble(datatable.Rows[i]["标准偏差"])) / datatable.Rows.Count;
            }

            this.textBox1.Text = ("计算完成");

        }

        protected DataTable MergeDataTable(DataTable dt1, DataTable dt2, string KeyColName)
        {
            int count1 = dt1.Rows.Count;
            int count2 = dt2.Rows.Count;
            if (count1 > count2)
            {
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    DataRow row = dt1.NewRow();
                    row["differ"] = dt2.Rows[i]["differ"];
                    dt1.Rows.Add(row);
                }
                return dt1;
            }
            else
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    DataRow row = dt2.NewRow();
                    row["differ"] = dt1.Rows[i]["differ"];
                    dt2.Rows.Add(row);
                }
                return dt2;
            }
         
        }


        /// <summary>
        /// 不考虑多边形内部是否包含子多边形的面目标公共边界调整
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="searchTable"></param>
        public void PolygonBoundaryUpdateRegardlessWhetherIncludedPolygons(string mainTable, string searchTable)
        {
            double ratio1;
            double ratio2;
            double distance = 0;
            double mainArea = 0;
            double searchArea = 0;

            List<int> objectidList = new List<int>();
            objectidList = getObject(mainTable);

            if (objectidList != null)
            {
                foreach (int mainObjectID in objectidList)
                {//筛选主多边形周围的多边形
                    List<int> searchObjectidList = new List<int>();
                    searchObjectidList = surroundingPolygonsRegardlessWhetherIncludedPolygons(mainTable, searchTable, mainObjectID);
                    if (searchObjectidList != null)
                    {
                        foreach (int searchObjectid in searchObjectidList)
                        {
                            distance = nearestDistance(mainTable, mainObjectID, searchTable, searchObjectid);
                            mainArea = area(mainTable, mainObjectID);
                            searchArea = area(searchTable, searchObjectid);

                            //ratio1 = distance / mainArea;
                            //ratio2 = distance / searchArea;

                            //if (ratio1 < lengthAreaRatioThreshold && ratio2 < lengthAreaRatioThreshold && mainArea > areaThreshold)
                            //{//当主多边形与搜索多边形长度面积商满足条件时

                            //以下要区分数据来源


                            getClosePoints1(mainTable, searchTable, mainObjectID, searchObjectid, mainArea, searchArea);

                            //}
                        }
                    }


                }

            }
        }

        /// <summary>
        /// 考虑多边形内部是否包含子多边形的面目标公共边界调整
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="searchTable"></param>
        public void PolygonBoundaryUpdate(string mainTable, string searchTable)
        {
            double ratio1;
            double ratio2;
            double distance = 0;
            double mainArea = 0;
            double searchArea = 0;

            List<int> objectidList = new List<int>();
            objectidList = getObject(mainTable);

            if (objectidList != null)
            {
                foreach (int mainObjectID in objectidList)
                {//筛选主多边形周围的多边形
                    List<int> searchObjectidList = new List<int>();
                    searchObjectidList = surroundingPolygons(mainTable, searchTable, mainObjectID);
                    if (searchObjectidList != null)
                    {
                        foreach (int searchObjectid in searchObjectidList)
                        {
                            if (surroundingPolygons(searchTable, searchObjectid) > 0)
                            {//如果搜索多边形内部也包含子多边形

                                distance = nearestDistance(mainTable, mainObjectID, searchTable, searchObjectid);
                                mainArea = area(mainTable, mainObjectID);
                                searchArea = area(searchTable, searchObjectid);

                                //ratio1 = distance / mainArea;
                                //ratio2 = distance / searchArea;
                                //长度面积商阈值和主多边形面积是否可以不要？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？
                                //if (ratio1 < lengthAreaRatioThreshold && ratio2 < lengthAreaRatioThreshold && mainArea > areaThreshold)
                                //{//当主多边形与搜索多边形长度面积商满足条件时

                                //    //以下要区分数据来源

                                getClosePoints1(mainTable, searchTable, mainObjectID, searchObjectid, mainArea, searchArea);

                                //}
                            }

                        }
                    }


                }


            }
        }


        /// <summary>
        /// 根据ObjectID计算该多边形的面积
        /// </summary>
        /// <param name="mainTable">表名</param>
        /// <param name="mainObjectID">ObjectID</param>
        /// <returns></returns>
        public double area(string mainTable, int mainObjectID)
        {
            string SQL = string.Format("select sdo_geom.sdo_area((select shape from {0} where objectid={1}),0.05,'unit=SQ_METER') from daul", mainTable, mainObjectID);
            double mainArea = 0;
            using (OracleDataReader drarea1 = conHelper.queryReader(SQL))
                if (drarea1.Read() && drarea1[0] != DBNull.Value)
                {
                    mainArea = Convert.ToDouble(drarea1.GetFloat(0));
                }
            return mainArea;
        }

        /// <summary>
        /// 计算两个对象之间的最近距离
        /// </summary>
        /// <param name="mainTable"> 表1</param>
        /// <param name="mainObjectID">表1中的ObjectID</param>
        /// <param name="searchTable">表2</param>
        /// <param name="searchObjectid">表2中的ObjectID</param>
        public double nearestDistance(string mainTable, int mainObjectID, string searchTable, int searchObjectid)
        {
            string SQL = string.Format("select SDO_GEOM.SDO_DISTANCE((select shape from {0} where objectid={1}),(select shape from {2} where objectid={3}),0.05,'UNIT=M') from daul", mainTable, mainObjectID, searchTable, searchObjectid);
            double distance = 0;
            using (OracleDataReader drdis = conHelper.queryReader(SQL))
                if (drdis.Read() && drdis[0] != DBNull.Value)
                {
                    distance = Convert.ToDouble(drdis.GetFloat(0));
                }
            return distance;
        }

        /// <summary>
        /// 查找包含子多边形的主多边形周围一定范围内的其他多边形的objectid
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="mainObjectID"></param>
        /// <returns></returns>
        public List<int> surroundingPolygons(string mainTable, string searchTable, int mainObjectID)
        {
            //查看主多边形是否包含其他多边形
            List<int> objectidList = new List<int>();
            string SQL = string.Format("select count(*) from {0} where sdo_inside(shape, (select shape from {0} where objectid={1}))='TRUE'", mainTable, mainObjectID);

            using (OracleDataReader dr0 = conHelper.queryReader(SQL))
            {
                if (dr0.Read())
                {
                    if (dr0.GetInt32(0) > 0)
                    {//如果包含子多边形则执行以下操作 否则返回的 List<int> objectidList个数为零

                        //外部和内部所有多边形(获取距离主多边形3m以内的所以多边形作为搜索多边形)
                        SQL = string.Format("select objectid from {0} where SDO_WITHIN_DISTANCE(shape,(select shape from {1} where objectid={2}), 'distance={3} UNIT=M')= 'TRUE'", searchTable, mainTable, mainObjectID, nearestPointDistance);
                        using (OracleDataReader dr1 = conHelper.queryReader(SQL))
                        {
                            while (dr1.Read())
                            {
                                objectidList.Add(dr1.GetInt32(0));
                            }
                        }
                        ////内部多边形(删除内部多边形)
                        //SQL = string.Format("select objectid from {0} where sdo_inside(shape, (select shape from {0} where objectid={1}))='TRUE'", mainTable, mainObjectID);

                        //using (OracleDataReader dr2 = conHelper.queryReader(SQL))
                        //{
                        //    while (dr2.Read())
                        //    {
                        //        objectidList.Remove(dr2.GetInt32(0));
                        //    }
                        //}

                        //排除相交于面的多边形（包括内部多边形 和相交的多边形）
                        SQL = string.Format("select objectid from {0} where  sdo_geom.sdo_area( sdo_geom.sdo_intersection(shape,(select shape from {1} where objectid={2}),0.05),0.05）>0", searchTable, mainTable, mainObjectID);
                        using (OracleDataReader dr1 = conHelper.queryReader(SQL))
                        {
                            while (dr1.Read())
                            {
                                objectidList.Remove(dr1.GetInt32(0));
                            }
                        }
                    }
                }
            }
            objectidList.Remove(mainObjectID);
            return objectidList;

        }

        /// <summary>
        /// 查看搜索多边形内部是否包含子多边形
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="mainObjectID"></param>
        /// <returns></returns>
        public int surroundingPolygons(string searchTable, int searchObjectID)
        {
            //查看搜索多边形是否包含子多边形
            //List<int> objectidList = new List<int>();
            string SQL = string.Format("select count(*) from {0} where sdo_inside(shape, (select shape from {0} where objectid={1}))='TRUE'", searchTable, searchObjectID);
            int count = 0;
            using (OracleDataReader dr0 = conHelper.queryReader(SQL))
            {
                if (dr0.Read())
                {
                    count = dr0.GetInt32(0);
                }
            }
            return count;

        }

        /// <summary>
        /// 查找主多边形周围一定范围内的其他多边形的objectid
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="mainObjectID"></param>
        /// <returns></returns>
        public List<int> surroundingPolygonsRegardlessWhetherIncludedPolygons(string mainTable, string searchTable, int mainObjectID)
        {

            List<int> objectidList = new List<int>();

            //外部和内部所有多边形
            string SQL = string.Format("select objectid from {0} where SDO_WITHIN_DISTANCE(shape,(select shape from {1} where objectid={2}), 'distance={3} UNIT=M')= 'TRUE'", searchTable, mainTable, mainObjectID, nearestPointDistance);
            using (OracleDataReader dr1 = conHelper.queryReader(SQL))
            {
                while (dr1.Read())
                {
                    objectidList.Add(dr1.GetInt32(0));
                }
            }
            //内部多边形
            SQL = string.Format("select objectid from {0} where sdo_inside(shape, (select shape from {0} where objectid={1}))='TRUE'", mainTable, mainObjectID);

            using (OracleDataReader dr2 = conHelper.queryReader(SQL))
            {
                while (dr2.Read())
                {
                    objectidList.Remove(dr2.GetInt32(0));
                }
            }


            objectidList.Remove(mainObjectID);
            return objectidList;

        }

        /// <summary>
        /// //查找距离主多边形最近点距离小于阈值的所有多边形的objectid
        /// </summary>
        /// <param name="searchTable">搜索表</param>
        /// <param name="mainTable">主表</param>
        /// <param name="mainObjectID">主表中的一个objectID</param>
        /// <returns></returns>
        public DataTable PolygonsInRange(string searchTable, string mainTable, int mainObjectID)
        {
            string SQL = string.Format("select objectid from {0} where SDO_WITHIN_DISTANCE(shape,(select shape from {1} where objectid={2}), 'distance={3} UNIT=M')= 'TRUE'", searchTable, mainTable, mainObjectID, nearestPointDistance);
            using (DataTable dataTable = conHelper.dhfQueryReader(SQL))
            {
                return dataTable;
            }
        }

        /// <summary>
        /// 根据表名称获取表中所有objectid并返回
        /// </summary>
        /// <param name="mainTable"></param>
        public List<int> getObject(string mainTable)
        {
            string SQL = string.Format("select objectid from {0} where matchid is not null", mainTable);
            List<int> objectidList = new List<int>();
            //OracleDBHelper conHelper = new OracleDBHelper();

            using (OracleDataReader dr0 = conHelper.queryReader(SQL))
            {
                while (dr0.Read())
                {
                    objectidList.Add(Convert.ToInt32(dr0[0]));

                }
                dr0.Close();
            }
            return objectidList;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 根据ObjectID提取经纬度
        /// </summary>
        /// <param name="mainTable">表名</param>
        /// <param name="mainObjectID">ObjectID</param>
        /// <returns></returns>
        public List<MyPoint> extractlLonLat(string mainTable, int mainObjectID)
        {
            string SQL = string.Format("select sdo_geometry.get_wkt(shape) from {0} where objectid={1}", mainTable, mainObjectID);
            //OracleDBHelper conHelper = new OracleDBHelper();
            string lonLatString = null;
            using (OracleDataReader dr = conHelper.queryReader(SQL))
            {
                if (dr.Read())
                {
                    lonLatString = dr[0].ToString();
                }
                dr.Close();
            }
            string[] lonLatList = lonLatString.Replace("POLYGON ((", "").Replace("(", "").Replace("))", "").Replace("LINESTRING (", "").Replace("）", "").Trim().Split(',');

            string[] lonandlat;
            MyPoint point;
            List<MyPoint> PointList = new List<MyPoint>();
            for (int i = 0; i < lonLatList.Length; i++)
            {
                lonandlat = lonLatList[i].Trim().Split(' ');
                point = new MyPoint(conHelper);
                point.Lon = Convert.ToDouble(lonandlat[0]);
                point.Lat = Convert.ToDouble(lonandlat[1]);
                PointList.Add(point);
            }
            GC.Collect();
            return PointList;

        }



        public void getClosePoints1(string mainTable, string searchTable, int mainObjectid, int searchObjectid, double mainArea, double searchArea)
        {
            List<MyPoint> mainPointList = extractlLonLat(mainTable, mainObjectid);
            List<MyPoint> searchPointList = extractlLonLat(searchTable, searchObjectid);
            MyPoint pointUsedTo = new MyPoint(conHelper);
            string mainWkt = null;
            string searchWkt = null;
            string newMainWkt = null;
            string newSearchWkt = null;
            string SQL = string.Format("select sdo_geometry.get_wkt(shape) from {0} where objectid={1}", mainTable, mainObjectid);
            using (OracleDataReader dr11 = conHelper.queryReader(SQL))
            {
                if (dr11.Read())
                {
                    mainWkt = dr11[0].ToString();
                    //.Replace(mainlat.ToString(), newPointlat.ToString()).Replace(mainlon.ToString(), newPointlon.ToString());
                }
                dr11.Close();
            }

            SQL = string.Format("select sdo_geometry.get_wkt(shape) from {0} where objectid={1}", searchTable, searchObjectid);
            using (OracleDataReader dr12 = conHelper.queryReader(SQL))
            {
                if (dr12.Read())
                {
                    searchWkt = dr12[0].ToString();
                    //.Replace(searchlat.ToString(), newPointlat.ToString()).Replace(searchlon.ToString(), newPointlon.ToString())
                }
                dr12.Close();
            }
            newMainWkt = mainWkt;
            newSearchWkt = searchWkt;
            //1如果主多边形上的点到搜索多边形的距离(最短距离)小于阈值 2则查找搜索多边形上距离主点最近的一个点 3对这两个点进行位置调整
            for (int i = 0; i < mainPointList.Count(); i++)
            {
                SQL = string.Format("select SDO_GEOM.SDO_DISTANCE (sdo_geometry('POINT ({0} {1})',4326),(select shape from {2} where objectid={3} ),0.05) from  daul", mainPointList[i].Lon, mainPointList[i].Lat, searchTable, searchObjectid);
                double pointandPolygondistance = 0;
                using (OracleDataReader dr12 = conHelper.queryReader(SQL))
                {
                    if (dr12.Read())
                    {
                        pointandPolygondistance = dr12.GetDouble(0);
                    }
                    dr12.Close();
                }


                if (pointandPolygondistance < nearestPointDistance && pointandPolygondistance > 0)
                { //1如果主多边形上的点到搜索多边形的距离(最短距离)小于阈值

                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("distance", typeof(double));
                    dataTable.Columns.Add("point", typeof(MyPoint));

                    for (int j = 0; j < searchPointList.Count(); j++)
                    {//2查找搜索多边形上距离主点最近的一个点
                        DataRow dataRow = dataTable.NewRow();


                        double polinandPointdistance = pointUsedTo.calculateDistance(mainPointList[i], searchPointList[j]);

                        if (polinandPointdistance < pointDistanceThreshold)//pointDistanceThreshold
                        {
                            dataRow["distance"] = Convert.ToDouble(polinandPointdistance);
                            dataRow["point"] = searchPointList[j];
                            dataTable.Rows.Add(dataRow);
                        }

                        //??=

                        //if (polinandPointdistance <= pointDistanceThreshold)
                        //{//如果两点之间的距离小于阈值，存贮
                        //MyPoint mainPoint = mainPointList[i];
                        //    MyPoint searchPoint = searchPointList[j];
                        //    MyPoint newPoint = getNewPoint(mainPoint, searchPoint, mainArea, searchArea);
                        //    mainWkt = mainWkt.Replace(mainPoint.Lat.ToString(), newPoint.Lat.ToString()).Replace(mainPoint.Lon.ToString(), newPoint.Lon.ToString());
                        //    searchWkt = searchWkt.Replace(searchPoint.Lat.ToString(), newPoint.Lat.ToString()).Replace(searchPoint.Lon.ToString(), newPoint.Lon.ToString());
                        //    //updatePoint1(mainTable, searchTable, mainObjectid, searchObjectid, mainPoint, searchPoint, mainArea, searchArea);
                        //}
                    }
                    if (dataTable.Rows.Count > 0)
                    {
                        double min = (double)dataTable.Compute("min(distance)", null);
                        int rowindex = 0;
                        for (int k = 0; k < dataTable.Rows.Count; k++)
                        {
                            if (dataTable.Rows[k]["distance"].ToString() == min.ToString())
                            {
                                rowindex = k;//得到索引
                            }
                        }
                        MyPoint searchPoint = (MyPoint)dataTable.Rows[rowindex]["point"];
                        MyPoint mainPoint = mainPointList[i];
                        double dis = pointUsedTo.calculateDistance(searchPoint, mainPoint);
                        sw.WriteLine(dis);
                        MyPoint newPoint = getNewPoint(mainPoint, searchPoint, mainArea, searchArea);


                        newMainWkt = newMainWkt.Replace(mainPoint.Lat.ToString(), newPoint.Lat.ToString()).Replace(mainPoint.Lon.ToString(), newPoint.Lon.ToString());

                        newSearchWkt = newSearchWkt.Replace(searchPoint.Lat.ToString(), newPoint.Lat.ToString()).Replace(searchPoint.Lon.ToString(), newPoint.Lon.ToString());

                    }

                }

            }

            if (newMainWkt != mainWkt && newSearchWkt != searchWkt)
            {
                try
                {
                    //调用存储过程更新 3对这两个点进行位置调整
                    excuteStoredProduce(mainTable, mainObjectid, newMainWkt);
                    excuteStoredProduce(searchTable, searchObjectid, newSearchWkt);
                    GC.Collect();
                    Console.WriteLine("更新一次！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }

        public MyPoint getNewPoint(MyPoint mainPoint, MyPoint searchPoint, double mainArea, double searchArea)
        {
            MyPoint newPoint = new MyPoint();
            double mainlat = Convert.ToDouble(mainPoint.Lat);
            double mainlon = Convert.ToDouble(mainPoint.Lon);
            double searchlat = Convert.ToDouble(searchPoint.Lat);
            double searchlon = Convert.ToDouble(searchPoint.Lon);
            newPoint.Lat = (searchlat - mainlat) * (mainArea / (mainArea + searchArea)) + mainlat;
            newPoint.Lon = (searchlon - mainlon) * (mainArea / (mainArea + searchArea)) + mainlon;

            return newPoint;
        }



        public void getClosePoints(string mainTable, string searchTable, int mainObjectID, int searchObjectid, List<MyPoint> mainPointList, List<MyPoint> searchPointList, double mainArea, double searchArea)
        {
            //OracleDBHelper conHelper = new OracleDBHelper();
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("mainPointLon");
            dataTable.Columns.Add("mainPointLat");
            dataTable.Columns.Add("searchPointLon");
            dataTable.Columns.Add("searchPointLat");
            dataTable.Columns.Add("distance");
            int k = 0;

            for (int i = 0; i < mainPointList.Count(); i++)
            {
                for (int j = k; j < searchPointList.Count(); j++)
                {
                    MyPoint point = new MyPoint(conHelper);
                    double distance = point.calculateDistance(mainPointList[i], searchPointList[j]);
                    if (distance <= pointDistanceThreshold)
                    {//如果两点之间的距离小于阈值，存贮
                        k++;
                        DataRow row = dataTable.NewRow();
                        row["mainPointLon"] = mainPointList[i].Lon;
                        row["mainPointLat"] = mainPointList[i].Lat;
                        row["searchPointLon"] = searchPointList[j].Lon;
                        row["searchPointLat"] = searchPointList[j].Lat;
                        row["distance"] = distance;
                        dataTable.Rows.Add(row);
                        goto tiaochu;
                    }
                    else
                    {
                        if (dataTable.Rows.Count > 0)
                        {
                            DataRow dr_last = dataTable.Rows[dataTable.Rows.Count - 1];
                            if (dr_last["mainPointLon"].ToString() == "|" && dr_last["mainPointLat"].ToString() == "|" && dr_last["searchPointLon"].ToString() == "|" && dr_last["searchPointLat"].ToString() == "|")
                            {

                            }
                            else
                            {
                                DataRow row = dataTable.NewRow();
                                row["mainPointLon"] = "|";
                                row["mainPointLat"] = "|";
                                row["searchPointLon"] = "|";
                                row["searchPointLat"] = "|";
                                row["distance"] = "|";
                                dataTable.Rows.Add(row);
                                goto tiaochu;
                            }
                        }
                    }
                }
            tiaochu: ;
            }

            ArrayList mainPointLons = new ArrayList();
            ArrayList mainPointLats = new ArrayList();
            ArrayList searchPointLons = new ArrayList();
            ArrayList searchPointLats = new ArrayList();
            ArrayList distanceLists = new ArrayList();

            foreach (DataRow dataRow in dataTable.Rows)
            {
                mainPointLons.Add(dataRow["mainPointLon"].ToString());
                mainPointLats.Add(dataRow["mainPointLat"].ToString());
                searchPointLons.Add(dataRow["searchPointLon"].ToString());
                searchPointLats.Add(dataRow["searchPointLat"].ToString());
                distanceLists.Add(dataRow["distance"].ToString());
            }

            string mainPointLonString = string.Join(",", mainPointLons.ToArray()).Replace(",|,", "|");
            string mainPointLatString = string.Join(",", mainPointLats.ToArray()).Replace(",|,", "|");
            string searchPointLonString = string.Join(",", searchPointLons.ToArray()).Replace(",|,", "|");
            string searchPointLatString = string.Join(",", searchPointLats.ToArray()).Replace(",|,", "|");
            string distanceListString = string.Join(",", distanceLists.ToArray()).Replace(",|,", "|");

            ArrayList mainPointLonListStringList = new ArrayList(mainPointLonString.Trim().Split('|'));
            mainPointLonListStringList.Remove("");
            ArrayList mainPointLatListStringList = new ArrayList(mainPointLatString.Trim().Split('|'));
            mainPointLatListStringList.Remove("");
            ArrayList searchPointLonListStringList = new ArrayList(searchPointLonString.Trim().Split('|'));
            searchPointLonListStringList.Remove("");
            ArrayList searchPointLatListStringList = new ArrayList(searchPointLatString.Trim().Split('|'));
            searchPointLatListStringList.Remove("");
            ArrayList distanceListStringList = new ArrayList(distanceListString.Trim().Split('|'));
            distanceListStringList.Remove("");



            string mainPointLonListStringListResult;
            string mainPointLatListStringListResult;
            string searchPointLonListStringListResult;
            string searchPointLatListStringListResult;
            string distanceListStringListResult;
            //mainPointListStringList的长度和searchPointListStringList的长度应该是一样的
            for (int i = 0; i < mainPointLonListStringList.Count; i++)
            {
                ArrayList mainPointLonListStringListI = new ArrayList(mainPointLonListStringList[i].ToString().Split(','));
                mainPointLonListStringListI.Remove("");
                ArrayList mainPointLatListStringListI = new ArrayList(mainPointLatListStringList[i].ToString().Split(','));
                mainPointLatListStringListI.Remove("");
                ArrayList searchPointLonListStringListI = new ArrayList(searchPointLonListStringList[i].ToString().Split(','));
                searchPointLonListStringListI.Remove("");
                ArrayList searchPointLatListStringListI = new ArrayList(searchPointLatListStringList[i].ToString().Split(','));
                searchPointLatListStringListI.Remove("");
                ArrayList distanceListStringListI = new ArrayList(distanceListStringList[i].ToString().Split(','));
                distanceListStringListI.Remove("");

                //这里的长度应该是一样的
                for (int j = 0; j < mainPointLonListStringListI.Count; j++)
                {
                    if (mainPointLonListStringListI.Count > 1)
                    {//连续的点个数在两个以上测执行以下操作
                        mainPointLonListStringListResult = mainPointLonListStringListI[j].ToString();
                        mainPointLatListStringListResult = mainPointLatListStringListI[j].ToString();
                        searchPointLonListStringListResult = searchPointLonListStringListI[j].ToString();
                        searchPointLatListStringListResult = searchPointLatListStringListI[j].ToString();
                        distanceListStringListResult = distanceListStringListI[j].ToString();

                        updatePoint(mainTable, searchTable, mainObjectID, searchObjectid, mainPointLonListStringListResult, mainPointLatListStringListResult, searchPointLonListStringListResult, searchPointLatListStringListResult, distanceListStringListResult, mainArea, searchArea);
                    }

                }

            }
        }

        //符合条件的多边形每个点对都需要执行一次存储过程 较为耗时 需要改为将所有点坐标组合在一起执行一次存储过程
        public void updatePoint1(string mainTable, string searchTable, int mainObjectId, int searchObjectid, MyPoint mainPoint, MyPoint searchPoint, double mainArea, double searchArea)
        {
            double newPointlat;
            double newPointlon;
            double mainlat = Convert.ToDouble(mainPoint.Lat);
            double mainlon = Convert.ToDouble(mainPoint.Lon);
            double searchlat = Convert.ToDouble(searchPoint.Lat);
            double searchlon = Convert.ToDouble(searchPoint.Lon);
            newPointlat = (searchlat - mainlat) * (mainArea / (mainArea + searchArea)) + mainlat;
            newPointlon = (searchlon - mainlon) * (mainArea / (mainArea + searchArea)) + mainlon;
            string SQL;
            //string mainString = string.Format("{0} {1}", mainPoint.Lon, mainPoint.Lat);
            //string searchString = string.Format("{0} {1}", searchPoint.Lon, searchPoint.Lat);
            //string resultString = string.Format("{0} {1}", newPointlon, newPointlat);
            string mainInsertWkt = null;
            string searchInsertWkt = null;
            if (newPointlat != mainlat && newPointlat != searchlat && newPointlon != mainlon && newPointlon != searchlon)
            {
                //Console.WriteLine("mainString：" + mainString);
                //Console.WriteLine("searchString：" + searchString);
                //Console.WriteLine("resultString：" + resultString);
                SQL = string.Format("select sdo_geometry.get_wkt(shape) from {0} where objectid={1}", mainTable, mainObjectId);
                using (OracleDataReader dr11 = conHelper.queryReader(SQL))
                {
                    if (dr11.Read())
                    {
                        mainInsertWkt = dr11[0].ToString().Replace(mainlat.ToString(), newPointlat.ToString()).Replace(mainlon.ToString(), newPointlon.ToString());
                    }
                    dr11.Close();
                }

                SQL = string.Format("select sdo_geometry.get_wkt(shape) from {0} where objectid={1}", searchTable, searchObjectid);
                using (OracleDataReader dr12 = conHelper.queryReader(SQL))
                {
                    if (dr12.Read())
                    {
                        searchInsertWkt = dr12[0].ToString().Replace(searchlat.ToString(), newPointlat.ToString()).Replace(searchlon.ToString(), newPointlon.ToString());
                    }
                    dr12.Close();
                }





                excuteStoredProduce(mainTable, mainObjectId, mainInsertWkt);
                excuteStoredProduce(searchTable, searchObjectid, searchInsertWkt);

                Console.WriteLine("更新一次");
            }

        }

        public void excuteStoredProduce(string tableName, int ObjectId, string InsertWkt)
        {
            //将重组的wkt导入数据库
            //OracleDBHelper db = new OracleDBHelper();

            OracleDBHelper db = new OracleDBHelper();
            OracleConnection myConnection = db.getOracleConnection();

            if (myConnection.State == ConnectionState.Closed)
            {
                myConnection.Open();
            }
            OracleCommand cmd = myConnection.CreateCommand();

            //执行储存过程
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "UP_" + tableName;

            cmd.Parameters.Add("objid", OracleDbType.Int32).Direction = ParameterDirection.Input;
            cmd.Parameters["objid"].Value = ObjectId;
            cmd.Parameters.Add("wkt", OracleDbType.Clob).Direction = ParameterDirection.Input;
            cmd.Parameters["wkt"].Value = InsertWkt;

            cmd.ExecuteNonQuery();

            //string mainInsertSQL = string.Format("declare mainInsertWkt clob; begin  mainInsertWkt := '{1}'; update {0} set shape=sdo_geometry(mainInsertWkt,4326) where objectid={2};end;", mainTable, mainInsertWkt, mainObjectId);
            ////SQL = string.Format("update {0} set shape=sdo_geometry('{1}',4326) where objectid={2}", mainTable, wkt, mainMatchid);
            //conHelper.sqlExecuteUnClose(mainInsertSQL);
            //conHelper.sqlExecuteUnClose("COMMIT");
            //Console.WriteLine(mainTable + "中mainMatchid为" + mainObjectId + "的要素被更新");
            //arrayListMainobjectid.Add(mainObjectId.ToString());

            //string searchInsertSQL = string.Format("declare mainInsertWkt clob; begin  mainInsertWkt := '{1}'; update {0} set shape=sdo_geometry(mainInsertWkt,4326) where objectid={2};end;", searchTable, searchInsertWkt, searchObjectid);
            ////SQL = string.Format("update {0} set shape=sdo_geometry('{1}',4326) where objectid={2}", searchTable, wkt, searchObjectid);
            //conHelper.sqlExecuteUnClose(searchInsertSQL);
            //conHelper.sqlExecuteUnClose("COMMIT");
            //Console.WriteLine(searchTable + "中searchObjectid为" + searchObjectid + "的要素被更新");
            //arrayListSearchobjectid.Add(searchObjectid.ToString());


        }

        public void updatePoint(string mainTable, string searchTable, int mainMatchid, int searchObjectid, string mainPointLon, string mainPointLat, string searchPointLon, string searchPointLat, string distance, double mainArea, double searchArea)
        {
            OracleDBHelper conHelper = new OracleDBHelper();
            double newPointlat;
            double newPointlon;

            double mainlat = Convert.ToDouble(mainPointLat);
            double mainlon = Convert.ToDouble(mainPointLon);
            double searchlat = Convert.ToDouble(searchPointLat);
            double searchlon = Convert.ToDouble(searchPointLon);
            newPointlat = (searchlat - mainlat) * (mainArea / (mainArea + searchArea)) + mainlat;
            newPointlon = (searchlon - mainlon) * (mainArea / (mainArea + searchArea)) + mainlon;

            string mainString = string.Format("{0} {1}", mainPointLon, mainPointLat);
            string searchString = string.Format("{0} {1}", searchPointLon, searchPointLat);
            string resultString = string.Format("{0} {1}", newPointlon, newPointlat);
            string wkt = null;
            //string wktReplace = null;
            if (newPointlat != mainlat && newPointlat != searchlat && newPointlon != mainlon && newPointlon != searchlon)
            {
                Console.WriteLine("mainString：" + mainString);
                Console.WriteLine("searchString：" + searchString);
                Console.WriteLine("resultString：" + resultString);
                string sql11 = string.Format("select sdo_geometry.get_wkt(shape) from {0} where objectid={1}", mainTable, mainMatchid);
                using (OracleDataReader dr11 = conHelper.queryReader(sql11))
                {

                    if (dr11.Read())
                    {
                        wkt = dr11[0].ToString();
                        wkt = wkt.Replace(mainlat.ToString(), newPointlat.ToString());
                        wkt = wkt.Replace(mainlon.ToString(), newPointlon.ToString());

                    }
                    //dr11.Close();
                    //将重组的wkt导入数据库

                    string mainInsertWkt = string.Format("declare mainInsertWkt clob; begin  mainInsertWkt := '{1}'; update {0} set shape=sdo_geometry(mainInsertWkt,4326) where objectid={2};end;", mainTable, wkt, mainMatchid);
                    conHelper.sqlExecuteUnClose(mainInsertWkt);
                    conHelper.sqlExecuteUnClose("COMMIT");
                    Console.WriteLine(mainTable + "中mainMatchid为" + mainMatchid + "的要素被更新");
                    //arrayListMainobjectid.Add(mainMatchid.ToString());
                }
                string sql12 = string.Format("select sdo_geometry.get_wkt(shape) from {0} where objectid={1}", searchTable, searchObjectid);
                using (OracleDataReader dr12 = conHelper.queryReader(sql12))
                {
                    if (dr12.Read())
                    {
                        wkt = dr12[0].ToString();
                        wkt = wkt.Replace(searchlat.ToString(), newPointlat.ToString());
                        wkt = wkt.Replace(searchlon.ToString(), newPointlon.ToString());
                    }
                    //dr12.Close();
                    //将重组的wkt导入数据库
                    string searchInsertWkt = string.Format("declare mainInsertWkt clob; begin  mainInsertWkt := '{1}'; update {0} set shape=sdo_geometry(mainInsertWkt,4326) where objectid={2};end;", searchTable, wkt, searchObjectid);
                    //string insertWkt = string.Format("update {0} set shape=sdo_geometry('{1}',4326) where objectid={2}", searchTable, wkt, searchObjectid);
                    conHelper.sqlExecuteUnClose(searchInsertWkt);
                    conHelper.sqlExecuteUnClose("COMMIT");
                    Console.WriteLine(searchTable + "中searchObjectid为" + searchObjectid + "的要素被更新");
                    //arrayListSearchobjectid.Add(searchObjectid.ToString());
                }

            }

        }

        /// <summary>
        /// 线节点调整
        /// </summary>
        /// <param name="mainTable">主表</param>
        /// <param name="searchTable">搜索表</param>
        public void LineNodeUpdate(string mainTable, string searchTable)
        {

            List<int> objectidList = new List<int>();
            objectidList = getObject(mainTable);

            if (objectidList != null)
            {
                foreach (int mainObjectID in objectidList)
                {//筛选主线周围并且拓扑相离的的搜索线
                    List<int> searchObjectidList = new List<int>();
                    searchObjectidList = surroundinglines(mainTable, searchTable, mainObjectID);
                    if (searchObjectidList.Count != 0)
                    {
                        foreach (int searchObjectid in searchObjectidList)
                        {
                            List<MyPoint> EndPointListofSearchLine;

                            //获得搜索线的两个端点
                            EndPointListofSearchLine = GetEndPointsofLine(searchObjectid, searchTable);

                            //判断两个端点距离主线的距离是否小于阈值 （返回两个端点中距离主线的距离小于阈值的端点）
                            List<double> distanceList;
                            EndPointListofSearchLine = distanceBetweenSearchLineEndpointAndMainline(EndPointListofSearchLine, mainObjectID, mainTable, out distanceList);

                            //对于距离小于阈值的端点执行缓冲区分析（缓冲区半径为点到直线的最短距离）缓冲区与主线的交点即为最终求得点（线延长所需要添加的节点）
                            MyPoint newPoint = pointBuffer(EndPointListofSearchLine, distanceList, mainTable, mainObjectID);

                            updateNode1(newPoint, EndPointListofSearchLine, searchObjectid, searchTable);

                            ////获取距离搜索线端点小于阈值的主线的节点//暂时不用
                            //GetMainLineNode(EndPointListofSearchLine, mainObjectID, mainTable, searchObjectid, searchTable);

                        }



                    }



                }


            }
        }
        /// <summary>
        /// 对于距离小于阈值的端点执行缓冲区分析（缓冲区半径为点到直线的最短距离）缓冲区与主线的交点即为最终求得点（线延长所需要添加的节点）
        /// </summary>
        /// <param name="EndPointList"></param>
        /// <param name="distanceList"></param>
        /// <param name="mainTable"></param>
        /// <param name="mainObjectID"></param>
        public MyPoint pointBuffer(List<MyPoint> EndPointList, List<double> distanceList, string mainTable, int mainObjectID)
        {
            MyPoint newPoint = null;
            double k = 0.1;
            for (int i = 0; i < EndPointList.Count; i++)
            {//可适当调大缓冲区距离 若相交的为线 可求线的中点，即为所求点20181128！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
            tiaochu: ;
                string sql = string.Format("select sdo_geometry.get_wkt( SDO_GEOM.sdo_intersection((SDO_GEOM.SDO_BUFFER((sdo_geometry('POINT ({1} {2})',4326)),{0},0.05)),(select shape from {3} where objectid={4}),0.05) )from daul", distanceList[i] + k, EndPointList[i].Lon, EndPointList[i].Lat, mainTable, mainObjectID);

                using (OracleDataReader dr11 = conHelper.queryReader(sql))
                {
                    if (dr11.Read())
                    {
                        if (dr11[0] == DBNull.Value)
                        {
                            if (distanceList[i] + k > 3.0)
                            {
                                break;
                            }
                            else
                            {
                                k = k + 0.1;
                                goto tiaochu;
                            }

                        }
                        string wkt = dr11.GetString(0);
                        string[] lonLatList;
                        MyPoint point;
                        if (wkt.Contains("GEOMETRYCOLLECTION"))
                        {
                            break;
                        }
                        else if (wkt.Contains("LINESTRING"))
                        {
                            lonLatList = wkt.Replace("LINESTRING (", "").Replace(")", "").Trim().Split(',');

                            //string[] lonLatList = lonLatString.Replace("POLYGON ((", "").Replace("))", "").Replace("LINESTRING (", "").Replace(")", "").Trim().Split(',');

                            string[] lonandlat;
                            List<MyPoint> AllPointList = new List<MyPoint>();

                            for (int j = 0; j < lonLatList.Length; j++)
                            {
                                lonandlat = lonLatList[j].Trim().Split(' ');
                                point = new MyPoint();
                                point.Lon = Convert.ToDouble(lonandlat[0]);
                                point.Lat = Convert.ToDouble(lonandlat[1]);
                                AllPointList.Add(point);
                            }
                            point = new MyPoint(conHelper);
                            newPoint = point.getMidpointLonLat(AllPointList[0], AllPointList[AllPointList.Count - 1]);
                        }
                        else if (wkt.Contains("POINT"))
                        {
                            lonLatList = wkt.Replace("POINT (", "").Replace(")", "").Trim().Split(' ');
                            newPoint = new MyPoint();
                            newPoint.Lon = Convert.ToDouble(lonLatList[0]);
                            newPoint.Lat = Convert.ToDouble(lonLatList[1]);
                        }
                    }
                }
            }
            return newPoint;
        }


        public void updateNode1(MyPoint newPoint, List<MyPoint> EndPointListofSearchLine, int searchObjectid, string searchTable)
        {
            if (newPoint != null)
            {
                string searchLineWktNoPrefix = null;
                string SQL = string.Format("select sdo_geometry.get_wkt(shape) from {0} where objectid={1}", searchTable, searchObjectid);
                using (OracleDataReader dr12 = conHelper.queryReader(SQL))
                {
                    if (dr12.Read())
                    {
                        searchLineWktNoPrefix = dr12[0].ToString().Replace("LINESTRING (", "").Replace(")", "");
                        string[] searchLineWkt = searchLineWktNoPrefix.Split(',');
                        for (int i = 0; i < EndPointListofSearchLine.Count; i++)
                        {
                            string searchPointString = string.Format("{0} {1}", EndPointListofSearchLine[i].Lon, EndPointListofSearchLine[i].Lat);
                            int Pointer = Array.IndexOf(searchLineWkt, searchPointString);//查看搜索线的端点的位置
                            string addPointString = string.Format("{0} {1}", newPoint.Lon, newPoint.Lat);
                            if (Pointer == 0)
                            {
                                searchLineWktNoPrefix = addPointString + ", " + searchLineWktNoPrefix;//如果搜索线的端点的位置在头部
                            }
                            else
                            {
                                searchLineWktNoPrefix = searchLineWktNoPrefix + ", " + addPointString;//如果搜索线的端点的位置在尾部
                            }

                        }
                        searchLineWktNoPrefix = string.Format("LINESTRING ({0})", searchLineWktNoPrefix);

                    }
                    dr12.Close();
                }
                //调用存储过程更新线？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？
                excuteStoredProduce(searchTable, searchObjectid, searchLineWktNoPrefix);
                Console.WriteLine("更新一次！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！");
            }

        }
        //获取距离搜索线端点小于阈值的主线的节点和搜索线端点组成的点对
        public void GetMainLineNode(List<MyPoint> SearchLineEndpoint, int mainObjectID, string mainTable, int searchObjectid, string searchTable)
        {
            List<MyPoint> mainLinePointsList = GetAllPointsofLine(mainObjectID, mainTable);
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("point");
            dataTable.Columns.Add("distance");
            if (SearchLineEndpoint != null && mainLinePointsList != null)
            {
                foreach (MyPoint searchPoint in SearchLineEndpoint)
                {
                    foreach (MyPoint mainPoint in mainLinePointsList)
                    {
                        DataRow row = dataTable.NewRow();

                        string SQL = string.Format("select SDO_GEOM.SDO_DISTANCE((sdo_geometry('POINT ({0} {1})',4326)),(sdo_geometry('POINT ({2} {3})',4326)),0.05,'UNIT=M') from daul", searchPoint.Lon, searchPoint.Lat, mainPoint.Lon, mainPoint.Lat);

                        using (OracleDataReader drdis = conHelper.queryReader(SQL))
                            if (drdis.Read() && drdis[0] != DBNull.Value)
                            {
                                if (drdis.GetDouble(0) < nearestPointDistance)
                                {
                                    row["point"] = mainPoint;
                                    row["distance"] = drdis.GetDouble(0);
                                }

                            }
                    }
                    if (dataTable.Rows.Count > 0)
                    {
                        float max = (float)dataTable.Compute("Max(distance)", null);
                        int rowindex = 0;
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            if (dataTable.Rows[i]["distance"].ToString() == max.ToString())
                            {
                                rowindex = i;//得到索引
                            }
                        }

                        MyPoint mainPoint1 = (MyPoint)dataTable.Rows[rowindex]["point"];
                        //给搜索线添加一个端点（此端点的位置为主线上距离搜索线端点最近的点）
                        updateNode(mainPoint1, searchPoint, searchObjectid, searchTable);
                    }

                }
            }

        }

        /// <summary>
        /// 给搜索线添加一个端点（此端点的位置为主线上距离搜索线端点最近的点）
        /// </summary>
        /// <param name="mainPoint"></param>
        /// <param name="searchPoint"></param>
        public void updateNode(MyPoint mainPoint, MyPoint searchPoint, int searchObjectid, string searchTable)
        {
            string searchLineWktNoPrefix = null;
            string SQL = string.Format("select sdo_geometry.get_wkt(shape) from {0} where objectid={1}", searchTable, searchObjectid);
            using (OracleDataReader dr12 = conHelper.queryReader(SQL))
            {
                if (dr12.Read())
                {
                    searchLineWktNoPrefix = dr12[0].ToString().Replace("LINESTRING (", "").Replace(")", "");
                    string[] searchLineWkt = searchLineWktNoPrefix.Split(',');
                    string searchPointString = string.Format("{0} {1}", searchPoint.Lon, searchPoint.Lat);
                    int Pointer = Array.IndexOf(searchLineWkt, "searchPointString");//查看搜索线的端点的位置
                    string addPoinyString = string.Format("{0} {1}", mainPoint.Lon, mainPoint.Lat);
                    if (Pointer == 0)
                    {
                        searchLineWktNoPrefix = addPoinyString + searchLineWktNoPrefix;//如果搜索线的端点的位置在头部
                    }
                    else
                    {
                        searchLineWktNoPrefix = searchLineWktNoPrefix + addPoinyString;//如果搜索线的端点的位置在尾部
                    }
                    searchLineWktNoPrefix = string.Format("LINESTRING ({0})", searchLineWktNoPrefix);
                }
                dr12.Close();
            }
            //调用存储过程更新线？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？？
            excuteStoredProduce(searchTable, searchObjectid, searchLineWktNoPrefix);

        }
        /// <summary>
        /// 返回两个端点中距离主线的距离小于阈值的端点
        /// </summary>
        /// <param name="SearchLineEndpoint"></param>
        /// <param name="mainlineObjectid"></param>
        /// <param name="mainlineTable"></param>
        /// <returns></returns>
        public List<MyPoint> distanceBetweenSearchLineEndpointAndMainline(List<MyPoint> SearchLineEndpoint, int mainlineObjectid, string mainlineTable, out List<double> distanceList)
        {
            List<MyPoint> newSearchLineEndpoint = new List<MyPoint>();
            distanceList = new List<double>();
            if (SearchLineEndpoint != null)
            {
                foreach (MyPoint point in SearchLineEndpoint)
                {
                    string SQL = string.Format("select SDO_GEOM.SDO_DISTANCE((sdo_geometry('POINT ({2} {3})',4326)  ),(select shape from {0} where objectid={1}),0.05,'UNIT=M') from daul", mainlineTable, mainlineObjectid, point.Lon, point.Lat);

                    using (OracleDataReader drdis = conHelper.queryReader(SQL))
                        if (drdis.Read() && drdis[0] != DBNull.Value)
                        {
                            double dist = drdis.GetFloat(0);
                            if (drdis.GetFloat(0) < nearestPointDistance)
                            {
                                distanceList.Add(dist);
                                newSearchLineEndpoint.Add(point);
                            }

                        }
                }
            }

            return newSearchLineEndpoint;
        }

        /// <summary>
        /// 筛选主线周围并且拓扑相离的的搜索线
        /// </summary>
        /// <param name="mainTable">主表</param>
        /// <param name="searchTable">搜索表</param>
        /// <param name="mainObjectID">主线id</param>
        /// <returns></returns>
        public List<int> surroundinglines(string mainTable, string searchTable, int mainObjectID)
        {
            List<int> searchObjectidList = new List<int>();
            //搜索距离mainObjectID阈值范围内的所有线
            string SQL = string.Format("select objectid from {0} where SDO_WITHIN_DISTANCE(shape,(select shape from {1} where objectid={2}), 'distance={3} UNIT=M')= 'TRUE'", searchTable, mainTable, mainObjectID, nearestPointDistance);
            using (OracleDataReader dr1 = conHelper.queryReader(SQL))
            {
                while (dr1.Read())
                {
                    searchObjectidList.Add(dr1.GetInt32(0));
                }
            }
            //排除拓扑相交的所有线
            SQL = string.Format("select objectid from {0} where SDO_ANYINTERACT（shape,(select shape from {1} where objectid={2})）='TRUE'", searchTable, mainTable, mainObjectID);
            using (OracleDataReader dr1 = conHelper.queryReader(SQL))
            {
                while (dr1.Read())
                {
                    searchObjectidList.Remove(dr1.GetInt32(0));
                }
            }
            searchObjectidList.Remove(mainObjectID);

            return searchObjectidList;
        }



        /// <summary>
        /// 获取搜索线的两个端点(待优化！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！)
        /// </summary>
        /// <param name="objectidList"></param>
        /// <param name="searchTable"></param>
        /// <returns></returns>
        public List<MyPoint> GetEndPointsofLine(int searchObjectid, string searchTable)
        {

            List<MyPoint> AllPointList = GetAllPointsofLine(searchObjectid, searchTable);

            List<MyPoint> PointList = new List<MyPoint>();
            PointList.Add(AllPointList[0]);
            PointList.Add(AllPointList[AllPointList.Count - 1]);
            GC.Collect();
            return PointList;

        }

        /// <summary>
        /// 获取搜索线的所有节点(待优化！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！)
        /// </summary>
        /// <param name="objectidList"></param>
        /// <param name="Table"></param>
        /// <returns></returns>
        public List<MyPoint> GetAllPointsofLine(int Objectid, string Table)
        {
            string lonLatString = null;
            MyPoint point;

            string SQL = string.Format("select  sdo_geometry.get_wkt(shape) from {0} where objectid={1}", Table, Objectid);
            using (OracleDataReader dr1 = conHelper.queryReader(SQL))
            {
                if (dr1.Read())
                {
                    lonLatString = dr1[0].ToString();
                }
            }


            string[] lonLatList = lonLatString.Replace("POLYGON ((", "").Replace("))", "").Replace("LINESTRING (", "").Replace(")", "").Trim().Split(',');

            string[] lonandlat;
            List<MyPoint> AllPointList = new List<MyPoint>();
            for (int i = 0; i < lonLatList.Length; i++)
            {
                lonandlat = lonLatList[i].Trim().Split(' ');
                point = new MyPoint(conHelper);
                point.Lon = Convert.ToDouble(lonandlat[0]);
                point.Lat = Convert.ToDouble(lonandlat[1]);
                AllPointList.Add(point);
            }
            GC.Collect();
            return AllPointList;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string[] tableNameList = { "RESIDENTIAL_AREA", "SOIL_AREA", "VEGETATION_AREA", "WATER_AREA", "TRAFFIC_AREA", "RESIDENTIAL_LINE", "SOIL_LINE", "TRAFFIC_LINE", "VEGETATION_LINE", "WATER_LINE" };
            //showmap.showMap(tableNameList, axMapcontrol);
            foreach (string tableName in tableNameList)
            {
                ArrayList arrayListObjectid = new ArrayList();
                //string SQL = string.Format("");
                string sql = string.Format("select objectid from {0} where matchid=objectid", tableName);
                using (OracleDataReader dr = conHelper.queryReader(sql))
                {
                    while (dr.Read())
                    {
                        arrayListObjectid.Add(dr.GetInt32(0).ToString());
                    }
                }
                for (int j = 0; j < axMapcontrol.LayerCount; j++)
                {
                    string name = axMapcontrol.get_Layer(j).Name;
                    if (name == tableName)
                    {
                        Random R;
                        Random G;
                        Random B;
                        showmap.showByAttibute(axMapcontrol.get_Layer(j) as IFeatureLayer, "objectid", "=", arrayListObjectid);
                    }
                }
            }


            //showmap.showByAttibute(axMapcontrol, "objectid", "=", arrayListSearchobjectid);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            start = DateTime.Now;
            System.IO.File.WriteAllText(path, string.Empty);//先清空文件里面的内容
            sw = new StreamWriter(path, true);
            sw.AutoFlush = true;//自动刷入

            sw.WriteLine("RESIDENTIAL_AREA");
            PolygonBoundaryUpdate( "RESIDENTIAL_AREA","RESIDENTIAL_AREA");

            sw.WriteLine("TRAFFIC_AREA");
            PolygonBoundaryUpdate("TRAFFIC_AREA","TRAFFIC_AREA");

            sw.WriteLine("SOIL_AREA");
            PolygonBoundaryUpdateRegardlessWhetherIncludedPolygons("SOIL_AREA","SOIL_AREA");

            sw.WriteLine("VEGETATION_AREA");
            PolygonBoundaryUpdateRegardlessWhetherIncludedPolygons("VEGETATION_AREA","VEGETATION_AREA");

            sw.WriteLine("WATER_AREA");
            PolygonBoundaryUpdateRegardlessWhetherIncludedPolygons("WATER_AREA","WATER_AREA");

            sw.Close();//关闭写入流
            Console.WriteLine("更新结束");

            TimeSpan span = DateTime.Now - start;
            string expens = span.Days.ToString() + "天" + span.Hours.ToString() + "小时" + span.Minutes.ToString() + "分钟";
            textBox1.Text = "开始于：" + start + "\r\n结束于：" + DateTime.Now + "\r\n用时：" + expens;

        }

        private void button5_Click(object sender, EventArgs e)
        {
            start = DateTime.Now;
            LineNodeUpdate("RESIDENTIAL_LINE", "RESIDENTIAL_LINE");
            LineNodeUpdate("SOIL_LINE", "SOIL_LINE");
            LineNodeUpdate("TRAFFIC_LINE", "TRAFFIC_LINE");
            LineNodeUpdate("VEGETATION_LINE", "VEGETATION_LINE");
            LineNodeUpdate("WATER_LINE", "WATER_LINE");
            Console.WriteLine("更新完成");

            TimeSpan span = DateTime.Now - start;
            string expens = span.Days.ToString() + "天" + span.Hours.ToString() + "小时" + span.Minutes.ToString() + "分钟";
            textBox1.Text = "开始于：" + start + "\r\n结束于：" + DateTime.Now + "\r\n用时：" + expens;

        }

        private void button6_Click(object sender, EventArgs e)
        {
            string[] tableNameList = { "RESIDENTIAL_AREA", "RESIDENTIAL_AREA_COPY", "SOIL_AREA", "SOIL_AREA_COPY", "VEGETATION_AREA", "VEGETATION_AREA_COPY", "WATER_AREA", "WATER_AREA_COPY", "TRAFFIC_AREA", "TRAFFIC_AREA_COPY", "RESIDENTIAL_LINE", "RESIDENTIAL_LINE_COPY", "SOIL_LINE", "SOIL_LINE_COPY", "TRAFFIC_LINE", "TRAFFIC_LINE_COPY", "VEGETATION_LINE_COPY", "VEGETATION_LINE_COPY", "WATER_LINE", "WATER_LINE_COPY" };
            showmap.showMap(tableNameList, axMapcontrol);
        }




    }
}

