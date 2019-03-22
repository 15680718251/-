using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using GIS.UI.AdditionalTool;
using System.Threading;


/****************线状典型要素(交通、水系、居民地、植被)增量更新 by zbl  20181022***************/
namespace GIS.UI.Forms
{
    public partial class LineEleMatchUpdate : Form
    {
        #region 线状典型要素匹配更新的字段定义
        string conStr = "";//数据库连接字段
        int trafficUpCount = 0, waterUpCount = 0, residentialUpCount = 0, vegetationUpCount = 0, soilUpCount = 0;//各线要素更新量统计字段
        int modifyTrafficCount = 0,modifyWaterCount=0,modifyResidentialCount=0,modifyVegetationCount=0,modifySoilCount=0;//要素修改情况统计字段
        int createTrafficCount = 0, createWaterCount = 0, createResidentialCount = 0, createVegetationCount = 0, createSoilCount = 0;//要素新建情况统计字段
        int deleteTrafficCount = 0, deleteWaterCount = 0, deleteResidentialCount = 0, deleteVegetationCount = 0, deleteSoilCount = 0;//要素删除情况统计字段
        int detaintrafficCount=0,detainwaterCount=0,detainresidentialCount = 0, detainvegetationCount = 0, detainsoilCount = 0;//要素保留不作处理情况统计字段
        int incretrafficCount = 0, increwaterCount = 0, increresidentialCount = 0, increvegetationCount = 0, incresoilCount = 0;//要素增量统计字段
        double distanceThreshold=0, bufferRadius = 200, hausdorffThreshold = 0, Hausdorff2Threshold = 0, angleThreshold = 0;//匹配更新参数设置
        int matchid = 0;
        private AxMapControl axMapcontrol;
        private AxTOCControl axTOCControl;

        #endregion
        public LineEleMatchUpdate()
        {
            InitializeComponent();
        }
        public LineEleMatchUpdate(string conString, AxMapControl axMapcontrol, AxTOCControl axTOCControl)
        {
            InitializeComponent();
            this.conStr = conString;
            this.UpdatepgBar.Minimum = 0;
            this.axMapcontrol = axMapcontrol;
            this.axTOCControl = axTOCControl;
        }

        /// <summary>
        /// 获取数据库中交通、水系、居民地、植被、土质各要素的总数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">异常处理参数</param>
        private void LineEleMatchUpdate_Load(object sender, EventArgs e)
        {
            OracleDBHelper conhelper = new OracleDBHelper();
            OracleConnection con = conhelper.getOracleConnection();//连接数据库
            OracleCommand cmd = con.CreateCommand();
            cmd.CommandText = "select count(*)from TRAFFIC_LINE";
            int trafficCount = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(*)from TRAFFIC_NEWLINE";
            incretrafficCount = Convert.ToInt32(cmd.ExecuteScalar());
            this.TrafficUpLabel.Text = trafficCount.ToString() + "\\" + incretrafficCount.ToString();

            cmd.CommandText = "select count(*)from WATER_LINE";
            int waterCount = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(*)from WATER_NEWLINE";
            increwaterCount = Convert.ToInt32(cmd.ExecuteScalar());
            this.WaterUpLabel.Text = waterCount.ToString() + "\\" + increwaterCount.ToString() ;

            cmd.CommandText = "select count(*)from RESIDENTIAL_LINE";
            int residentialCount = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(*)from RESIDENTIAL_NEWLINE";
            increresidentialCount = Convert.ToInt32(cmd.ExecuteScalar());
            this.ResidentialUpLabel.Text = residentialCount.ToString() + "\\" + increresidentialCount.ToString();

            cmd.CommandText = "select count(*)from VEGETATION_LINE";
            int vegetationCount = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(*)from VEGETATION_NEWLINE";
            increvegetationCount = Convert.ToInt32(cmd.ExecuteScalar());
            this.VegetationUpLabel.Text = vegetationCount.ToString() + "\\" + increvegetationCount.ToString();

            cmd.CommandText = "select count(*)from SOIL_LINE";
            int soilCount = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(*)from SOIL_NEWLINE";
            incresoilCount = Convert.ToInt32(cmd.ExecuteScalar());
            this.SoilUpLabel.Text = soilCount.ToString() +"\\"+ incresoilCount.ToString();
        }

        /// <summary>
        /// 开始更新按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartUpdateBtn_Click(object sender, EventArgs e)
        {

            try
            {
                hausdorffThreshold = 2;
                Hausdorff2Threshold = 0.5;
                //angleThreshold = double.Parse(angleThresholdTB.Text);//夹角阈值
                angleThreshold = 90;
                distanceThreshold = 10;

                string baseTableName = "TRAFFIC_LINE";
                string IncreTableName = "TRAFFIC_NEWLINE";
                string constr = this.conStr;
                TrafficEleUpdate(baseTableName, IncreTableName, constr);
                trafficUpCount = createTrafficCount + deleteTrafficCount + modifyTrafficCount + detaintrafficCount;
                
                if (increwaterCount > 0)
                {
                    baseTableName = "WATER_LINE";
                    IncreTableName = "WATER_NEWLINE";
                    WaterEleUpdate(baseTableName, IncreTableName, constr);
                    waterUpCount = createWaterCount + deleteWaterCount + modifyWaterCount + detainwaterCount;
                }

                if (increresidentialCount > 0)
                {
                    baseTableName = "RESIDENTIAL_LINE";
                    IncreTableName = "RESIDENTIAL_NEWLINE";
                    ResidentialEleUpdate(baseTableName, IncreTableName, constr);
                    residentialUpCount = createResidentialCount + deleteResidentialCount + modifyResidentialCount + detainresidentialCount;
                }

                if (increvegetationCount > 0)
                {
                    baseTableName = "VEGETATION_LINE";
                    IncreTableName = "VEGETATION_NEWLINE";
                    VegetationEleUpdate(baseTableName, IncreTableName, constr);
                    vegetationUpCount = createVegetationCount + deleteVegetationCount + modifyVegetationCount + detainvegetationCount;
                }

                if (incresoilCount > 0)
                {
                    baseTableName = "SOIL_LINE";
                    IncreTableName = "SOIL_NEWLINE";
                    SoilEleUpdate(baseTableName, IncreTableName, constr);
                    soilUpCount = createSoilCount + deleteSoilCount + modifySoilCount + detainsoilCount;
                }
                MessageBox.Show("交通要素:共计更新" + trafficUpCount + "条数据！" + Environment.NewLine + "水系要素:共计更新" + waterUpCount + "条数据！" + Environment.NewLine + "居民地要素:共计更新" + residentialUpCount + "条数据！" + Environment.NewLine + "植被要素:共计更新" + vegetationUpCount + "条数据！" + Environment.NewLine + "土质要素:共计更新" + soilUpCount + "条数据！");
                Fun1();
                AddMap showmap = new AddMap();
                string[] tablelist = {"TRAFFIC_LINE", "WATER_LINE","RESIDENTIAL_LINE","VEGETATION_LINE","SOIL_LINE"};
                showmap.showMap(tablelist, axMapcontrol,axTOCControl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString ());
            }
        }

        #region 交通要素匹配更新
        /// <summary>
        /// 交通要素更新主函数
        /// </summary>
        /// <param name="baseTableName">交通要素基态数据表</param>
        /// <param name="IncreTableName">交通要素增量数据表</param>
        /// <param name="constr">数据库连接字符串</param>
        public void TrafficEleUpdate(string baseTableName, string IncreTableName, string constr)
        {
            try
            {
                OracleDBHelper helper = new OracleDBHelper();
                DataTable increTable = GetAllData(IncreTableName);//获得交通要素增量数据
                int increDataCount = increTable.Rows.Count;//统计交通要素增量数据总数
                for (int i = 0; i < increDataCount; i++)
                {
                    this.UpdatepgBar.Maximum = increDataCount;//控制进度条的最大值
                    UpdatepgBar.Value = i + 1;//更新进度条的值
                    string increobjectid = increTable.Rows[i]["objectid"].ToString();
                    string increosmid = increTable.Rows[i]["osmid"].ToString();
                    string increversionid = increTable.Rows[i]["versionid"].ToString();
                    string increosmid_V = increTable.Rows[i]["osmid"].ToString() + "_" + increTable.Rows[i]["versionid"].ToString();
                    string increnationEleName = increTable.Rows[i]["nationelename"].ToString();
                    string increnationcode = increTable.Rows[i]["nationcode"].ToString();
                    //string increshape = increTable.Rows[i]["shape"].ToString();
                    string incregromStr_wkt = increTable.Rows[i]["geometry_wkt"].ToString();
                    string increchangeType = increTable.Rows[i]["changetype"].ToString();
                    //DataTable baseTable = GetDataByBuffer(baseTableName, increshape);
                    DataTable baseTable = GetDataByBuffer(baseTableName, increosmid);//在基态数据库中搜索增量数据对象缓冲区范围内的所有对象数据
                    if (baseTable.Rows.Count > 0)
                    {
                        if (increchangeType =="create")
                        {
                            CreateToBaseDB(IncreTableName,baseTableName,increosmid, increobjectid, increnationEleName, increnationcode);
                            createTrafficCount++;
                        }
                        else if (increchangeType == "delete")
                        {
                            InsertToHistoryDB(IncreTableName, baseTableName, increosmid, increobjectid, increnationEleName, increnationcode);
                            string sql = string.Format("update {0} set updatestate=1  where {0}.osmid={1}", baseTableName, increosmid);
                            helper.sqlExecute(sql);
                            deleteTrafficCount++;
                        }
                        else if (increchangeType == "modify")
                        {
                            if (!overallMatching(baseTable,increosmid,increosmid_V, incregromStr_wkt, increnationEleName, constr, baseTableName, increchangeType))//整体匹配没有成功，则进行局部匹配
                            {
                                if (!partialMatch(baseTable, increosmid_V, incregromStr_wkt, increnationEleName, constr, baseTableName))
                                { //如果局部匹配也没成功，则说明基态数据中不存在与该对象可以匹配的对象，将其保留                               
                                    dataRetain(increosmid, increversionid, IncreTableName, baseTableName, constr);
                                }
                            }
                        }
                    }
                    else
                    { //说明缓冲区内没有找到可能与该对象相匹配的对象，则该对象保留
                        dataRetain(increosmid, increversionid, IncreTableName, baseTableName, constr);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
         
        /// <summary>
        /// 对于“create”类型的增量直接插入当前基态库
        /// </summary>
        private void CreateToBaseDB(string IncreTableName, string baseTableName,string increosmid, string increobjectid, string increnationEleName, string increnationcode)
        {
            try
            {
                OracleDBHelper helper=new OracleDBHelper ();
                OracleConnection con = helper.getOracleConnection();
                if (con.State == ConnectionState.Closed)
                { 
                    con.Open(); 
                }
               string sql = string.Format("INSERT INTO {0}(nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,updatestate)(select {1},'{2}',{3},{4}.versionid,{4}.starttime,{4}.endtime,{4}.changeset,{4}.userid,{4}.username,{4}.fc,{4}.dsg,{4}.tags,{4}.trustvalue,{4}.userreputation,{4}.shape,{4}.source,2 from {4} where {4}.osmid={3} )",baseTableName,increnationcode, increnationEleName, increosmid, IncreTableName);
               using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("成功插入{0}要素数据:osmid='{1}'", baseTableName,increosmid);
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///  对于“delete”类型的增量直接插入历史数据库
        /// </summary>
        private void InsertToHistoryDB(string IncreTableName,string baseTableName, string increosmid, string increobjectid, string increnationEleName, string increnationcode)
        {
             try
            {
                string sql = "";
                DateTime dt = DateTime.Now;
                string endtime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                OracleDBHelper helper=new OracleDBHelper ();
                OracleConnection con = helper.getOracleConnection();
                if (con.State == ConnectionState.Closed)
                { 
                    con.Open(); 
                }
                else  if ("TRAFFIC_NEWLINE".Equals(IncreTableName))
                {
                    sql = string.Format("INSERT INTO TRAFFIC_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,updatestate)(select {0},'{1}',{2}.osmid,{2}.versionid,{2}.starttime,'{3}',{2}.changeset,{2}.userid,{2}.username,{2}.fc,{2}.dsg,{2}.tags,{2}.trustvalue,{2}.userreputation,{2}.shape,{2}.source,1 from {2} where {2}.osmid={4})", increnationcode, increnationEleName, baseTableName, endtime, increosmid);
                     using (OracleCommand cmd = new OracleCommand(sql, con))
                     {
                         cmd.ExecuteNonQuery();
                         Console.WriteLine("成功插入交通要素数据:osmid='{0}'", increosmid);
                     }
                     //string sql1 = string.Format("delete from {0}  where osmid={1}", baseTableName, increosmid);
                     //helper.sqlExecuteUnClose(sql1);
                }
                else if ("WATER_NEWLINE".Equals(IncreTableName))
                {
                    sql = string.Format("INSERT INTO WATER_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,updatestate)(select {0},'{1}',{2}.osmid,{2}.versionid,{2}.starttime,'{3}',{2}.changeset,{2}.userid,{2}.username,{2}.fc,{2}.dsg,{2}.tags,{2}.trustvalue,{2}.userreputation,{2}.shape,{2}.source,1 from {2} where {2}.osmid={4})", increnationcode, increnationEleName, baseTableName, endtime, increosmid);
                    using (OracleCommand cmd = new OracleCommand(sql, con))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("成功插入水系要素数据:osmid='{0}'", increosmid);
                    }
                    //string sql1 = string.Format("delete from {0}  where osmid={1}", baseTableName, increosmid);
                    //helper.sqlExecuteUnClose(sql1);
                }
                else  if ("RESIDENTIAL_NEWLINE".Equals(IncreTableName))
                {
                    sql = string.Format("INSERT INTO RESIDENTIAL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,updatestate)(select {0},'{1}',{2}.osmid,{2}.versionid,{2}.starttime,'{3}',{2}.changeset,{2}.userid,{2}.username,{2}.fc,{2}.dsg,{2}.tags,{2}.trustvalue,{2}.userreputation,{2}.shape,{2}.source,1 from {2} where {2}.osmid={4})", increnationcode, increnationEleName, baseTableName, endtime, increosmid);
                    using (OracleCommand cmd = new OracleCommand(sql, con))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("成功插入居民地要素数据:osmid='{0}'", increosmid);
                    }
                    //string sql1 = string.Format("delete from {0}  where osmid={1}", baseTableName, increosmid);
                    //helper.sqlExecuteUnClose(sql1);
                }
                else  if ("VEGETATION_NEWLINE".Equals(IncreTableName))
                {
                    sql = string.Format("INSERT INTO VEGETATION_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,updatestate)(select {0},'{1}',{2}.osmid,{2}.versionid,{2}.starttime,'{3}',{2}.changeset,{2}.userid,{2}.username,{2}.fc,{2}.dsg,{2}.tags,{2}.trustvalue,{2}.userreputation,{2}.shape,{2}.source,1 from {2} where {2}.osmid={4})", increnationcode, increnationEleName, baseTableName, endtime, increosmid);
                    using (OracleCommand cmd = new OracleCommand(sql, con))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("成功插入植被要素数据:osmid='{0}'", increosmid);
                    }
                    //string sql1 = string.Format("delete from {0}  where osmid={1}", baseTableName, increosmid);
                    //helper.sqlExecuteUnClose(sql1);
                }
                else if ("SOIL_NEWLINE".Equals(IncreTableName))
                {
                    sql = string.Format("INSERT INTO SOIL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,updatestate)(select {0},'{1}',{2}.osmid,{2}.versionid,{2}.starttime,'{3}',{2}.changeset,{2}.userid,{2}.username,{2}.fc,{2}.dsg,{2}.tags,{2}.trustvalue,{2}.userreputation,{2}.shape,{2}.source,1 from {2} where {2}.osmid={4})", increnationcode, increnationEleName, baseTableName,endtime,increosmid);
                    using (OracleCommand cmd = new OracleCommand(sql, con))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("成功插入土质要素数据:osmid='{0}'", increosmid);
                    }
                    //string sql1 = string.Format("delete from {0}  where osmid={1}", baseTableName, increosmid);
                    //helper.sqlExecuteUnClose(sql1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion


        #region 未匹配到的数据将其保留

        /// <summary>   
        /// 缓冲区内没有找到可能与A对象相匹配的B对象，则该对象保留
        /// </summary>
        /// <param name="osmid">增量的osmid</param>
        /// <param name="versionid">增量的versionid</param>
        /// <param name="IncreTableName">增量表名</param>
        /// <param name="baseTableName">基态表名</param>
        /// <param name="constr"></param>
        private void dataRetain(string osmid, string versionid, string IncreTableName, string baseTableName, string constr)
        {
            try
            {
                int increosmid = 0, increversionid = 0,increuserid=0;
                string increusername = "", increstarttime = "", increfc = "", incredsg = "", incregeom = "", increchangeset = "";
                string increnationelename = "", increnationcode = "", incretags = "";
                string sql = string.Format("select osmid,username,userid,versionid,starttime,fc,dsg,tags,nationelename,nationcode,changeset,sdo_geometry.get_wkt(shape)geometry_wkt from {0} where {0}.osmid='{1}' and {0}.versionid ={2} ", IncreTableName,osmid,versionid);
                OracleDBHelper conhelper = new OracleDBHelper();//获取数据库帮助对象以连接数据库
                OracleConnection conn = conhelper.getOracleConnection();
                DateTime dt = DateTime.Now;
                string endtimeB = dt.ToString("yyyy-MM-dd HH:mm:ss");
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
                            increosmid = int.Parse(rd["osmid"].ToString());
                            increuserid = int.Parse(rd["userid"].ToString());
                            increversionid = int.Parse(rd["versionid"].ToString());
                            increusername = rd["username"].ToString();
                            increstarttime = rd["starttime"].ToString();//to_timestamp('2010-10-01 21:30:50', 'YYYY-MM-DD HH24:MI:SS')sql
                            increfc = rd["fc"].ToString();
                            incredsg = rd["dsg"].ToString();
                            incretags = rd["tags"].ToString();
                            incregeom = rd["geometry_wkt"].ToString();
                            increnationelename = rd["nationelename"].ToString();
                            increnationcode = rd["nationcode"].ToString();
                            increchangeset = rd["changeset"].ToString();
                        }
                    }
                }
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                //if (basegeom.Length > 4000)
                //{
                //    sql = string.Format("insert into {0} (osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,nationelename,nationcode,matchid,updatestate)(select {1}, {2},to_timestamp('{3}', 'YYYY-MM-DD\"T\" HH24:MI:SS\"Z\"'),{4},'{5}',{6},'{7}','{8}','{9}','{10}',{11}.trustvalue,{11}.userreputation,{11}.shape,{11}.source, '{12}', '{13}',2,0  FROM {11} where exists (select osmid FROM {0} where osmid={1})) ", baseTableName, baseosmid, baseversionid, basestarttime, endtimeB, basechangeset, baseuserid, baseusername, basefc, basedsg, basetags, IncreTableName, basenationelename, basenationcode);
                //    using (OracleCommand cmd = new OracleCommand(sql, conn))
                //    {
                //        cmd.ExecuteNonQuery();
                //        if ("TRAFFIC_LINE".Equals(baseTableName) || "WATER_LINE".Equals(baseTableName) || "RESIDENTIAL_LINE".Equals(baseTableName) || "VEGETATION_LINE".Equals(baseTableName) || "SOIL_LINE".Equals(baseTableName))
                //        {
                //            Console.WriteLine("成功插入{0}数据：osmid=" + baseosmid, baseTableName);
                //        }
                //    }
                //}
                //else
                //{
                    //sql = string.Format("insert into {0}(osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,nationelename,nationcode,matchid,updatestate)(select {1},{2},to_timestamp('{3}','YYYY-MM-DD\"T\" HH24:MI:SS\"Z\"'),'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11}.trustvalue,{11}.userreputation,{11}.shape,{11}.source,'{12}','{13}',2,0  FROM  {11} where exists (select osmid FROM {0} where osmid={1})) ", baseTableName, increosmid, increversionid, increstarttime, endtimeB, increchangeset, increuserid, increusername, increfc, incredsg, incretags, IncreTableName, increnationelename, increnationcode);
                    matchid++;
                    sql = string.Format("insert into {0}(osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,nationelename,nationcode,matchid,updatestate)(select {1},{2},to_timestamp('{3}','YYYY-MM-DD\"T\" HH24:MI:SS\"Z\"'),'{4}','{5}',{6},'{7}','{8}','{9}','{10}',{11}.trustvalue,{11}.userreputation,{11}.shape,{11}.source,'{12}','{13}',{14},0  FROM  {11} where {11}.osmid={1}) ", baseTableName, increosmid, increversionid, increstarttime, endtimeB, increchangeset, increuserid, increusername, increfc, incredsg, incretags, IncreTableName, increnationelename, increnationcode,matchid);
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                        if ("TRAFFIC_LINE".Equals(baseTableName) || "WATER_LINE".Equals(baseTableName) || "RESIDENTIAL_LINE".Equals(baseTableName) || "VEGETATION_LINE".Equals(baseTableName) || "SOIL_LINE".Equals(baseTableName))
                        {
                            Console.WriteLine("成功插入{0}数据：osmid=" + increosmid, baseTableName);
                        }
                    }
                   string sql1 = string.Format("update {0} set matchid={1} where {0}.osmid={2}", IncreTableName,matchid,increosmid);
                   conhelper.sqlExecute(sql1 );
                //}
                //conn.Close();
                if ("TRAFFIC_LINE".Equals(baseTableName))
                { detaintrafficCount++; }
                if ("WATER_LINE".Equals(baseTableName))
                { detainwaterCount++; }
                if ("RESIDENTIAL_LINE".Equals(baseTableName))
                { detainresidentialCount++; }
                if ("VEGETATION_LINE".Equals(baseTableName))
                { detainvegetationCount++; }
                if ("SOIL_LINE".Equals(baseTableName))
                { detainsoilCount++; }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion
        #region 局部匹配更新
        /// <summary>
        /// 局部匹配
        /// </summary>
        /// <param name="baseTable"></param>
        /// <param name="increosmid_V"></param>
        /// <param name="incregromStr_wkt"></param>
        /// <param name="increnationEleName"></param>
        /// <param name="constr"></param>
        /// <param name="baseTableName"></param>
        /// <returns></returns>
        private bool partialMatch(DataTable baseTable, string increosmid_V, string incregromStr_wkt, string increnationEleName, string constr, string baseTableName)
        {
             Boolean matchSuccess = false;
             try
             {
                 int bufferDataTableCount = baseTable.Rows.Count;
                 Boolean flag = false;//记录局部是否相同
                 for (int j = 0; j < bufferDataTableCount; j++)
                 {
                     string baseobjectid = baseTable.Rows[j]["objectid"].ToString();
                     string baseosmid = baseTable.Rows[j]["osmid"].ToString();
                     string basegeomStr_wkt = baseTable.Rows[j]["geometry_wkt"].ToString();
                     string basenationelename = baseTable.Rows[j]["nationelename"].ToString();

                     flag = partialMatchSituation(increosmid_V, incregromStr_wkt, increnationEleName, basegeomStr_wkt, basenationelename, baseTableName,int .Parse (baseosmid),int.Parse(baseobjectid), constr);
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
        /// <param name="increosmid_V"></param>
        /// <param name="incregromStr_wkt"></param>
        /// <param name="increnationEleName"></param>
        /// <param name="basegeomStr_wkt"></param>
        /// <param name="basenationelename"></param>
        /// <param name="baseTableName"></param>
        /// <param name="baseobjectid"></param>
        /// <param name="constr"></param>
        /// <returns></returns>
        private bool partialMatchSituation(string increosmid_V, string incregromStr_wkt, string increnationEleName, string basegeomStr_wkt, string basenationelename, string baseTableName,int baseosmid,int baseobjectid, string constr)
        {
            Boolean flag = false;
            try
            {
                if (!increnationEleName.Equals(basenationelename))//如果属性不相同，直接认为不匹配，返回false
                {
                    return flag;
                }
                string situation = partialHausdorff(increosmid_V, incregromStr_wkt, basegeomStr_wkt, baseTableName,baseosmid, baseobjectid, constr);
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
        /// <param name="increosmid_V"></param>
        /// <param name="incregromStr_wkt"></param>
        /// <param name="basegeomStr_wkt"></param>
        /// <param name="baseTableName"></param>
        /// <param name="baseobjectid"></param>
        /// <param name="constr"></param>
        /// <returns></returns>
        private string partialHausdorff(string increosmid_V, string incregromStr_wkt, string basegeomStr_wkt, string baseTableName,int baseosmid, int baseobjectid, string constr)
        {
            try
            {
                string pointstrA = incregromStr_wkt.Substring(12, incregromStr_wkt.Length - 13);
                pointstrA = pointstrA.Trim();
                string[] pointA = pointstrA.Split(',');

                string pointsStrB = basegeomStr_wkt.Substring(12, basegeomStr_wkt.Length - 13);
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
                        return partialUpdate(pointA, pointB, baseTableName,baseosmid,baseobjectid, incregromStr_wkt, increosmid_V, a, constr);

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
                        return partialUpdate(pointA, pointB, baseTableName,baseosmid,baseobjectid, incregromStr_wkt, increosmid_V, b, constr);

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
                        return partialUpdate(pointA, pointB, baseTableName,baseosmid,baseobjectid, incregromStr_wkt, increosmid_V, c, constr);
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
                        return partialUpdate(pointA, pointB, baseTableName,baseosmid, baseobjectid, incregromStr_wkt, increosmid_V, d, constr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return "miss";//未知情况
        }

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="baseTableName"></param>
        /// <param name="baseobjectid"></param>
        /// <param name="incregromStr_wkt"></param>
        /// <param name="increosmid_V"></param>
        /// <param name="a"></param>
        /// <param name="constr"></param>
        /// <returns></returns>
        private string partialUpdate(string[] pointA, string[] pointB, string baseTableName,int baseosmid,int baseobjectid, string incregromStr_wkt, string increosmid_V, double distance, string constr)
        {
            try
            {
                OracleDBHelper helper = new OracleDBHelper();
                OracleConnection con = helper.getOracleConnection();
                DateTime dt = DateTime.Now;
                string endtime = dt.ToString("yyyy-MM-dd HH:mm:ss");

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
                                    con.Open();
                                    if ("TRAFFIC_LINE".Equals(baseTableName))
                                    {
                                        string sql1 = string.Format("INSERT INTO TRAFFIC_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,{3},3 from {0} where {0}.osmid={1})",baseTableName,endtime,baseosmid,matchid);
                                        helper.sqlExecute(sql1);
                                        string sql2 = string.Format("update TRAFFIC_NEWLINE set matchid={0} where osmid={1}", matchid,baseosmid);
                                        helper.sqlExecute(sql2);
                                        modifyTrafficCount++;
                                    }
                                    else if ("WATER_LINE".Equals(baseTableName))
                                    {
                                        string sql1 = string.Format("INSERT INTO WATER_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,{3},3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid, matchid);
                                        helper.sqlExecute(sql1);
                                        string sql2 = string.Format("update WATER_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                        helper.sqlExecute(sql2);
                                        modifyWaterCount++;
                                    }
                                    else if ("RESIDENTIAL_LINE".Equals(baseTableName))
                                    {
                                        string sql1 = string.Format("INSERT INTO RESIDENTIAL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,{3},3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid,matchid);
                                        helper.sqlExecute(sql1);
                                        string sql2 = string.Format("update RESIDENTIAL_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                        helper.sqlExecute(sql2);
                                        modifyResidentialCount++;
                                    }
                                    else if ("VEGETATION_LINE".Equals(baseTableName))
                                    {
                                        string sql1 = string.Format("INSERT INTO VEGETATION_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,{3},3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid, matchid);
                                        helper.sqlExecute(sql1);
                                        string sql2 = string.Format("update VEGETATION_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                        helper.sqlExecute(sql2);
                                        modifyVegetationCount++;
                                    }
                                    else if ("SOIL_LINE".Equals(baseTableName))
                                    {
                                        string sql1 = string.Format("INSERT INTO SOIL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,{3},3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid, matchid);
                                        helper.sqlExecute(sql1);
                                        string sql2 = string.Format("update SOIL_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                        helper.sqlExecute(sql2);
                                        modifySoilCount++;
                                    }

                                    string sql = string.Format("update {0} set updatestate=3,shape =sdo_geometry ('{1}',4326)) where osmid = {2} ", baseTableName, newGeomStr, baseosmid);
                                    using (OracleCommand cmd = new OracleCommand(sql, con))
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                    con.Close();
                                    Console.WriteLine("Objectid为：" + baseobjectid + "的{0}要素被osmid_v:" + increosmid_V + "局部更新", baseTableName);
                                    trafficUpCount++;
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
                                string newGeomStr = incregromStr_wkt;
                                con.Open();
                                if ("TRAFFIC_LINE".Equals(baseTableName))
                                {
                                    string sql1 = string.Format("INSERT INTO TRAFFIC_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                    helper.sqlExecute(sql1);
                                    string sql2 = string.Format("update TRAFFIC_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                    helper.sqlExecute(sql2);
                                    modifyTrafficCount++;
                                }
                                else if ("WATER_LINE".Equals(baseTableName))
                                {
                                    string sql1 = string.Format("INSERT INTO WATER_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                    helper.sqlExecute(sql1);
                                    string sql2 = string.Format("update WATER_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                    helper.sqlExecute(sql2);
                                    modifyWaterCount++;
                                }
                                else if ("RESIDENTIAL_LINE".Equals(baseTableName))
                                {
                                    string sql1 = string.Format("INSERT INTO RESIDENTIAL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                    helper.sqlExecute(sql1);
                                    string sql2 = string.Format("update RESIDENTIAL_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                    helper.sqlExecute(sql2);
                                    modifyResidentialCount++;
                                }
                                else if ("VEGETATION_LINE".Equals(baseTableName))
                                {
                                    string sql1 = string.Format("INSERT INTO VEGETATION_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                    helper.sqlExecute(sql1);
                                    string sql2 = string.Format("update VEGETATION_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                    helper.sqlExecute(sql2);
                                    modifyVegetationCount++;
                                }
                                else if ("SOIL_LINE".Equals(baseTableName))
                                {
                                    string sql1 = string.Format("INSERT INTO SOIL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                    helper.sqlExecute(sql1);
                                    string sql2 = string.Format("update SOIL_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                    helper.sqlExecute(sql2);
                                    modifySoilCount++;
                                }

                                string sql = string.Format("update {0} set update=3,shape =sdo_geometry ('{1}',4326)) where osmid = {2} ", baseTableName, newGeomStr, baseosmid);
                                using (OracleCommand cmd = new OracleCommand(sql, con))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                con.Close();
                                Console.WriteLine("Objectid为：" + baseobjectid + "的{0}要素被osmid_v:" + increosmid_V + "局部更新", baseTableName);
                                trafficUpCount++;
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
                            string newGeomStr = incregromStr_wkt.Substring(11, incregromStr_wkt.Length - 12) + "," + B_e_ecz;//A+B的尾垂足到B的尾节点
                            newGeomStr = "LINESTRING(" + newGeomStr + ")";
                            con.Open();
                            if ("TRAFFIC_LINE".Equals(baseTableName))
                            {
                                string sql1 = string.Format("INSERT INTO TRAFFIC_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                helper.sqlExecute(sql1);
                                string sql2 = string.Format("update TRAFFIC_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                helper.sqlExecute(sql2);
                                modifyTrafficCount++;
                            }
                            else if ("WATER_LINE".Equals(baseTableName))
                            {
                                string sql1 = string.Format("INSERT INTO WATER_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                helper.sqlExecute(sql1);
                                string sql2 = string.Format("update WATER_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                helper.sqlExecute(sql2);
                                modifyWaterCount++;
                            }
                            else if ("RESIDENTIAL_LINE".Equals(baseTableName))
                            {
                                string sql1 = string.Format("INSERT INTO RESIDENTIAL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                helper.sqlExecute(sql1);
                                string sql2 = string.Format("update RESIDENTIAL_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                helper.sqlExecute(sql2);
                                modifyResidentialCount++;
                            }
                            else if ("VEGETATION_LINE".Equals(baseTableName))
                            {
                                string sql1 = string.Format("INSERT INTO VEGETATION_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                helper.sqlExecute(sql1);
                                string sql2 = string.Format("update VEGETATION_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                helper.sqlExecute(sql2);
                                modifyVegetationCount++;
                            }
                            else if ("SOIL_LINE".Equals(baseTableName))
                            {
                                string sql1 = string.Format("INSERT INTO SOIL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                helper.sqlExecute(sql1);
                                string sql2 = string.Format("update SOIL_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                helper.sqlExecute(sql2);
                                modifySoilCount++;
                            }
                            string sql = string.Format("update {0} set updatestate=3,shape =sdo_geometry ('{1}',4326)) where osmid = {2} ", baseTableName, newGeomStr, baseosmid);
                            using (OracleCommand cmd = new OracleCommand(sql, con))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            con.Close();
                            Console.WriteLine("Objectid为：" + baseobjectid + "的{0}要素被局部更新", baseTableName);
                            trafficUpCount++;

                            return "Y3";//返回成功匹配情况1
                        }
                    }
                }
                else if (!verticalPointA_B_f.Equals("N"))//B上的首垂足存在，1A的首节点方向比B短
                {
                    if (!verticalPointA_B_e.Equals("N"))//B上的尾垂足也存在，1A尾节点方向比B短，此时已经确定了《A被B包含的情况》
                    {
                        string newGeomStr = incregromStr_wkt;
                        con.Open();
                        if ("TRAFFIC_LINE".Equals(baseTableName))
                        {
                            string sql1 = string.Format("INSERT INTO TRAFFIC_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                            helper.sqlExecute(sql1);
                            string sql2 = string.Format("update TRAFFIC_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                            helper.sqlExecute(sql2);
                            modifyTrafficCount++;
                        }
                        else if ("WATER_LINE".Equals(baseTableName))
                        {
                            string sql1 = string.Format("INSERT INTO WATER_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                            helper.sqlExecute(sql1);
                            string sql2 = string.Format("update WATER_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                            helper.sqlExecute(sql2);
                            modifyWaterCount++;
                        }
                        else if ("RESIDENTIAL_LINE".Equals(baseTableName))
                        {
                            string sql1 = string.Format("INSERT INTO RESIDENTIAL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                            helper.sqlExecute(sql1);
                            string sql2 = string.Format("update RESIDENTIAL_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                            helper.sqlExecute(sql2);
                            modifyResidentialCount++;
                        }
                        else if ("VEGETATION_LINE".Equals(baseTableName))
                        {
                            string sql1 = string.Format("INSERT INTO VEGETATION_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                            helper.sqlExecute(sql1);
                            string sql2 = string.Format("update VEGETATION_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                            helper.sqlExecute(sql2);
                            modifyVegetationCount++;
                        }
                        else if ("SOIL_LINE".Equals(baseTableName))
                        {
                            string sql1 = string.Format("INSERT INTO SOIL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                            helper.sqlExecute(sql1);
                            string sql2 = string.Format("update SOIL_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                            helper.sqlExecute(sql2);
                            modifySoilCount++;
                        }
                        string sql = string.Format("update {0} set updatestate=3,shape =sdo_geometry ('{1}',4326)) where osmid = {2} ", baseTableName, newGeomStr, baseosmid);
                        using (OracleCommand cmd = new OracleCommand(sql, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("Objectid为：" + baseobjectid + "的{0}要素被oscid_v:" + increosmid_V + "局部更新", baseTableName);
                        trafficUpCount++;
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
                            string newGeomStr = B_f_fcz + "," + incregromStr_wkt.Substring(11, incregromStr_wkt.Length - 12);//B的首节点到B的首垂足+A
                            newGeomStr = "LINESTRING(" + newGeomStr + ")";
                            con.Open();
                            if ("TRAFFIC_LINE".Equals(baseTableName))
                            {
                                string sql1 = string.Format("INSERT INTO TRAFFIC_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                helper.sqlExecute(sql1);
                                string sql2 = string.Format("update TRAFFIC_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                helper.sqlExecute(sql2);
                                modifyTrafficCount++;
                            }
                            else if ("WATER_LINE".Equals(baseTableName))
                            {
                                string sql1 = string.Format("INSERT INTO WATER_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                helper.sqlExecute(sql1);
                                string sql2 = string.Format("update WATER_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                helper.sqlExecute(sql2);
                                modifyWaterCount++;
                            }
                            else if ("RESIDENTIAL_LINE".Equals(baseTableName))
                            {
                                string sql1 = string.Format("INSERT INTO RESIDENTIAL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                helper.sqlExecute(sql1);
                                string sql2 = string.Format("update RESIDENTIAL_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                helper.sqlExecute(sql2);
                                modifyResidentialCount++;
                            }
                            else if ("VEGETATION_LINE".Equals(baseTableName))
                            {
                                string sql1 = string.Format("INSERT INTO VEGETATION_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                helper.sqlExecute(sql1);
                                string sql2 = string.Format("update VEGETATION_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                helper.sqlExecute(sql2);
                                modifyVegetationCount++;
                            }
                            else if ("SOIL_LINE".Equals(baseTableName))
                            {
                                string sql1 = string.Format("INSERT INTO SOIL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0}.nationcode,{0}.nationelename,{1},{0}.versionid,{0}.starttime,'{2}',{0}.changeset,{0}.userid,{0}.username,{0}.fc,{0}.dsg,{0}.tags,{0}.trustvalue,{0}.userreputation,{0}.shape,{0}.source,2,3 from {0} where {0}.osmid={1})", baseTableName, endtime, baseosmid);
                                helper.sqlExecute(sql1);
                                string sql2 = string.Format("update SOIL_NEWLINE set matchid={0} where osmid={1}", matchid, baseosmid);
                                helper.sqlExecute(sql2);
                                modifySoilCount++;
                            }
                            string sql = string.Format("update {0} set updatestate=3,shape =sdo_geometry ('{1}',4326)) where osmid = {2} ", baseTableName, newGeomStr, baseosmid);
                            using (OracleCommand cmd = new OracleCommand(sql, con))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            con.Close();
                            Console.WriteLine("Objectid为：" + baseobjectid + "的{0}要素被局部更新", baseTableName);
                            trafficUpCount++;

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
        #endregion
        #region 求过某条线段端点的垂线与另一条线段是否存在交点
        ////知道一点和斜率求直线方程：y=k(x-a)+b//判断点是否在线的缓冲区范围内：ST_Contains（） 
        //////求过某条线段端点的垂线与另一条线段的交点
        //知道一点和斜率求直线方程：y=k(x-a)+b//判断点是否在线的缓冲区范围内：ST_Contains（）      
        /// <summary>
        /// 求过某条线段端点的垂线与另一条线段的交点,存在交点返回交点，否则返回N
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p_2"></param>
        /// <param name="p_3"></param>
        /// <param name="p_4"></param>
        /// <returns></returns>
        private string verticalIntersect(string pointsA2, string pointsA1, string pointsB1, string pointsB2)
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
                if (con.State == ConnectionState.Closed)
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
        #endregion
        #region 获得需插入的线数据
        /// <summary>
        /// 得到插入的线数据
        /// </summary>
        /// <param name="vector_A1x"></param>
        /// <param name="vector_A1y"></param>
        /// <param name="verticalX"></param>
        /// <param name="verticalY"></param>
        /// <param name="vector_B1x"></param>
        /// <param name="vector_B1y"></param>
        /// <param name="vector_B2x"></param>
        /// <param name="vector_B2y"></param>
        /// <returns></returns>
        private string getIntersectPoint(double ax1, double ay1, double ax2, double ay2, double bx1, double by1, double bx2, double by2)
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
        #endregion
        #region 计算两线对象角度相似度
        /// <summary>
        /// 通过坐标和方向计算两直线向量之间的角度
        /// </summary>
        /// <param name="startAx"></param>
        /// <param name="startAy"></param>
        /// <param name="endAx"></param>
        /// <param name="endAy"></param>
        /// <param name="startBx"></param>
        /// <param name="startBy"></param>
        /// <param name="endBx"></param>
        /// <param name="endBy"></param>
        /// <returns></returns>
        private double strokeLineAngleJudgeByCoordinate(double startAx, double startAy, double endAx, double endAy, double startBx, double startBy, double endBx, double endBy)
        {
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
        #endregion\

        #region 整体匹配更新
        /// <summary>
        /// 整体匹配-根据hausdorff距离和属性值进行匹配
        /// </summary>
        /// <param name="baseTable"></param>
        /// <param name="increosmid_V"></param>
        /// <param name="incregromStr_wkt"></param>
        /// <param name="increnationEleName"></param>
        /// <param name="constr"></param>
        /// <param name="baseTableName"></param>
        /// <returns></returns>
        private bool overallMatching(DataTable baseTable,string increosmid,string increosmid_V, string incregromStr_wkt, string increnationEleName, string constr, string baseTableName, string increchangeType)
        {
            Boolean matchSuccess = false;
            OracleDBHelper helper = new OracleDBHelper();
            try
            {
                int bufferDataTableCount = baseTable.Rows.Count;
                string foundObjectid = "",foundosmid="";
                double foundHausdorff = double.MaxValue;
                Boolean attributeIsSame = false;//记录上一对儿属性值是否相同
                for (int j = 0; j < bufferDataTableCount; j++)//循环遍历基态线对象缓冲区内对象
                {
                    string baseobjectid = baseTable.Rows[j]["objectid"].ToString();
                    string baseosmid = baseTable.Rows[j]["osmid"].ToString();
                    string basenationelename = baseTable.Rows[j]["nationelename"].ToString();
                    string basenationcode = baseTable.Rows[j]["nationcode"].ToString();
                    string basegeomStr_wkt = baseTable.Rows[j]["geometry_wkt"].ToString();
                    double hausdorff = hausdorffDistance(incregromStr_wkt, basegeomStr_wkt);

                    if (hausdorff <= hausdorffThreshold)//比阈值小，说明满足条件整体匹配成功，继续比较其他条件，如属性值，进入更新环节
                    {
                        if (increnationEleName.Equals(basenationelename) && !attributeIsSame)//第一次出现属性相同的情况
                        {
                            foundosmid = baseosmid;
                            foundObjectid = baseobjectid;
                            foundHausdorff = hausdorff;
                            attributeIsSame = true;
                        }
                        else if (increnationEleName.Equals(basenationelename) && attributeIsSame)//上一对属性值也相同
                        {
                            if (hausdorff < foundHausdorff)//且hausdorff值比上一对儿的还小，则将其作为最匹配对象
                            {
                                foundosmid = baseosmid;
                                foundObjectid = baseobjectid;
                                foundHausdorff = hausdorff;
                                attributeIsSame = true;
                            }
                        }
                        else if (!increnationEleName.Equals(basenationelename) && !attributeIsSame)//未出现属性值相同的情况
                        {
                            if (hausdorff < foundHausdorff)//选取hausdorff值最小的一个，将其作为最匹配对象
                            {
                                foundosmid = baseosmid;
                                foundObjectid = baseobjectid;
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
                        DateTime dt = DateTime.Now;
                        string endtime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        //如果匹配成功，此处的更新操作是将增量A的shape线串更新基态的shape。
                        if ("TRAFFIC_LINE".Equals(baseTableName))
                        {
                            //matchid++;
                            string sql1 = string.Format("INSERT INTO TRAFFIC_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0},'{1}',{2}.osmid,{2}.versionid,{2}.starttime,'{3}',{2}.changeset,{2}.userid,{2}.username,{2}.fc,{2}.dsg,{2}.tags,{2}.trustvalue,{2}.userreputation,{2}.shape,{2}.source,{4},3 from {2} where {2}.osmid={5})", basenationcode, basenationelename, baseTableName, endtime,matchid,baseosmid);
                            helper.sqlExecute(sql1);
                            //string sql2 = string.Format("update TRAFFIC_NEWLINE set matchid={0},shape =sdo_geometry ('{1}',4326) where osmid={2}", matchid, incregromStr_wkt, Int32.Parse(foundosmid));
                            //helper.sqlExecute(sql2);
                            modifyTrafficCount++; 
                        }
                        else if ("WATER_LINE".Equals(baseTableName))
                        {
                            string sql1 = string.Format("INSERT INTO WATER_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0},'{1}',{2}.osmid,{2}.versionid,{2}.starttime,'{3}',{2}.changeset,{2}.userid,{2}.username,{2}.fc,{2}.dsg,{2}.tags,{2}.trustvalue,{2}.userreputation,{2}.shape,{2}.source,{4},3 from {2} where {2}.osmid={5})", basenationcode, basenationelename, baseTableName, endtime,matchid,baseosmid);
                            helper.sqlExecute(sql1);
                            //string sql2 = string.Format("update WATER_NEWLINE set matchid={0},shape =sdo_geometry ('{1}',4326) where osmid={2}", matchid, incregromStr_wkt, Int32.Parse(foundosmid));
                            //helper.sqlExecute(sql2);
                            modifyWaterCount++;
                        }
                        else if ("RESIDENTIAL_LINE".Equals(baseTableName))
                        {
                            string sql1 = string.Format("INSERT INTO RESIDENTIAL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0},'{1}',{2}.osmid,{2}.versionid,{2}.starttime,'{2}',{3}.changeset,{2}.userid,{2}.username,{2}.fc,{2}.dsg,{3}.tags,{2}.trustvalue,{2}.userreputation,{2}.shape,{2}.source,{4},3 from {2} where {2}.osmid={5})", basenationcode, basenationelename, baseTableName, endtime,matchid,baseosmid);
                            helper.sqlExecute(sql1);
                            //string sql2 = string.Format("update RESIDENTIAL_NEWLINE set matchid={0},shape =sdo_geometry ('{1}',4326) where osmid={2}", matchid, incregromStr_wkt, Int32.Parse(foundosmid));
                            //helper.sqlExecute(sql2);
                            modifyResidentialCount++;
                        }
                        else if ("VEGETATION_LINE".Equals(baseTableName))
                        {
                            string sql1 = string.Format("INSERT INTO VEGETATION_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0},'{1}',{2}.osmid,{2}.versionid,{2}.starttime,'{3}',{2}.changeset,{2}.userid,{2}.username,{2}.fc,{2}.dsg,{2}.tags,{2}.trustvalue,{2}.userreputation,{2}.shape,{2}.source,{4},3 from {2} where {2}.osmid={5})", basenationcode, basenationelename, baseTableName, endtime,matchid,baseosmid);
                            helper.sqlExecute(sql1);
                            //string sql2 = string.Format("update VEGETATION_NEWLINE set matchid={0},shape =sdo_geometry ('{1}',4326) where osmid={2}", matchid, incregromStr_wkt, Int32.Parse(foundosmid));
                            //helper.sqlExecute(sql2);
                            modifyVegetationCount++;
                        }
                        else if ("SOIL_LINE".Equals(baseTableName))
                        {
                            string sql1 = string.Format("INSERT INTO SOIL_OLDLINE (nationcode,nationelename,osmid,versionid,starttime,endtime,changeset,userid,username,fc,dsg,tags,trustvalue,userreputation,shape,source,matchid,updatestate)(select {0},'{1}',{2}.osmid,{2}.versionid,{2}.starttime,'{3}',{2}.changeset,{2}.userid,{2}.username,{2}.fc,{2}.dsg,{2}.tags,{2}.trustvalue,{2}.userreputation,{2}.shape,{2}.source,{4},3 from {2} where {2}.osmid={5})", basenationcode, basenationelename, baseTableName, endtime,matchid,baseosmid);
                            helper.sqlExecute(sql1);
                            //string sql2 = string.Format("update SOIL_NEWLINE set matchid={0},shape =sdo_geometry ('{1}',4326) where osmid={2}", matchid, incregromStr_wkt, Int32.Parse(foundosmid));
                            //helper.sqlExecute(sql2);
                            modifySoilCount++; 
                        }
                        sql = string.Format("update {0} set updatestate=3,shape =sdo_geometry ('{1}',4326) where osmid={2}", baseTableName, incregromStr_wkt, Int32.Parse(foundosmid));
                        helper.sqlExecute(sql);
                        Console.WriteLine("Objectid为：" + foundObjectid + "的{0}要素被osmid_v:" + increosmid_V + "整体更新", baseTableName);
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
        #endregion
        #region 获得缓冲区内的所有线对象
        /// <summary>
        /// 通过缓冲区半径和对象geom得到缓冲区内的线要素对象
        /// </summary>
        /// <param name="baseTableName">基态线数据</param>
        /// <param name="increosmid">缓冲区内增量数据的osmid</param>
        /// <returns>缓冲区内对象数据表</returns>
        private DataTable GetDataByBuffer(string baseTableName, string increosmid)
        {
            DataTable bufferData = new DataTable();//读取某个线对象缓冲区内所有对象的数据集表
            OracleDBHelper helper = new OracleDBHelper();
            try
            {
                string sql = "";
                if ("TRAFFIC_LINE".Equals(baseTableName) || "WATER_LINE".Equals(baseTableName) || "RESIDENTIAL_LINE".Equals(baseTableName) || "VEGETATION_LINE".Equals(baseTableName) || "SOIL_LINE".Equals(baseTableName))
                {
                    //对osm数据源的处理
                    sql = string.Format("select objectid,osmid,versionid,nationelename,nationcode,sdo_geometry.get_wkt(shape)geometry_wkt from {0} where SDO_WITHIN_DISTANCE(shape,(select shape from TRAFFIC_NEWLINE where osmid={1} and rownum=1),'distance={2},unit=m')='TRUE'", baseTableName, increosmid,bufferRadius);
                    //sql = string.Format("select objectid,osmid,versionid,nationelename,nationcode,shape,sdo_geometry.get_wkt(shape)geometry_wkt from {0}where SDO_WITHIN_DISTANCE(shape,{1},'distance={2},unit=m')='TRUE'", baseTableName, increshape, bufferRadius);
                    bufferData.Columns.Add("objectid");
                    bufferData.Columns.Add("osmid");
                    bufferData.Columns.Add("versionid");
                    bufferData.Columns.Add("nationelename");
                    bufferData.Columns.Add("nationcode");
                    //bufferData.Columns.Add("shape");
                    bufferData.Columns.Add("geometry_wkt");
                }
                else
                {
                    //对专业矢量数据源的处理
                    sql = string.Format("select objectid,versionid,nationelename,nationcode,sdo_geometry.get_wkt(shape)geometry_wkt from {0} where SDO_WITHIN_DISTANCE(shape,(select shape from VEGETATION_NEWLINE where osmid={1} and rownum=1),'distance={2},unit=m')='TRUE'", baseTableName,bufferRadius);
                    //sql = string.Format("select objectid,osmid,versionid,nationelename,nationcode,shape,sdo_geometry.get_wkt(shape)geometry_wkt from {0}where SDO_WITHIN_DISTANCE(shape,{1},'distance={2},unit=m')='TRUE'", baseTableName, increshape, bufferRadius);
                    bufferData.Columns.Add("objectid");
                    //bufferData.Columns.Add("osmid");
                    bufferData.Columns.Add("versionid");
                    bufferData.Columns.Add("nationelename");
                    bufferData.Columns.Add("nationcode");
                    //bufferData.Columns.Add("shape");
                    bufferData.Columns.Add("geometry_wkt");
                }
                using (OracleDataReader rd = helper.queryReader(sql))
                {
                    while (rd.Read())
                    {
                        DataRow bufferDataRow = bufferData.NewRow();
                        if ("TRAFFIC_LINE".Equals(baseTableName) || "WATER_LINE".Equals(baseTableName) || "RESIDENTIAL_LINE".Equals(baseTableName) || "VEGETATION_LINE".Equals(baseTableName) || "SOIL_LINE".Equals(baseTableName))
                        {
                            bufferDataRow["objectid"] = rd["objectid"].ToString();
                            bufferDataRow["osmid"] = rd["osmid"].ToString();
                            bufferDataRow["versionid"] = rd["versionid"].ToString();
                            bufferDataRow["nationelename"] = rd["nationelename"].ToString();
                            bufferDataRow["nationcode"] = rd["nationcode"].ToString();
                            //bufferDataRow["shape"] = rd["shape"].ToString();
                            bufferDataRow["geometry_wkt"] = rd["geometry_wkt"].ToString();
                            bufferData.Rows.Add(bufferDataRow);
                        }
                        else
                        {
                            bufferDataRow["objectid"] = rd["objectid"].ToString();
                            bufferDataRow["versionid"] = rd["versionid"].ToString();
                            bufferDataRow["nationelename"] = rd["nationelename"].ToString();
                            bufferDataRow["nationcode"] = rd["nationcode"].ToString();
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
        #endregion
        #region 获得各要素全部增量数据
        /// <summary>
        /// 获取各个要素的全部增量数据
        /// </summary>
        /// <param name="baseTableName">增量数据表</param>
        /// <returns></returns>
        private DataTable GetAllData(string IncreTableName)
        {
            string sql = "";
            DataTable table = new DataTable();
            try
            {
                OracleDBHelper helper = new OracleDBHelper();
                if ("TRAFFIC_NEWLINE".Equals(IncreTableName) || "WATER_NEWLINE".Equals(IncreTableName) || "RESIDENTIAL_NEWLINE".Equals(IncreTableName) || "VEGETATION_NEWLINE".Equals(IncreTableName) || "SOIL_NEWLINE".Equals(IncreTableName))
                {
                    sql = string.Format("select objectid,osmid,versionid,nationelename,nationcode,sdo_geometry.get_wkt(shape)geometry_wkt,changetype from {0} order by osmid,versionid,objectid", IncreTableName);
                  
                    table.Columns.Add("objectid");
                    table.Columns.Add("osmid");
                    table.Columns.Add("versionid");
                    table.Columns.Add("nationelename");
                    table.Columns.Add("nationcode");
                    //table.Columns.Add("shape");
                    table.Columns.Add("geometry_wkt");
                    table.Columns.Add("changetype");
                }
                else
                {
                    //如果是专业矢量数据则以objectid+nationcode+geometry_wkt来唯一标识一个对象
                    sql = string.Format("select objectid,versionid,nationelename,nationcode,sdo_geometry.get_wkt(shape)geometry_wkt,changetype from {0} order by versionid,objectid", IncreTableName);

                    table.Columns.Add("objectid");
                    //table.Columns.Add("osmid");
                    table.Columns.Add("versionid");
                    table.Columns.Add("nationelename");
                    table.Columns.Add("nationcode");
                    //table.Columns.Add("shape");
                    table.Columns.Add("geometry_wkt");
                    table.Columns.Add("changetype");
                }
                using (OracleDataReader rd = helper.queryReader(sql))
                {
                    while (rd.Read())
                    {
                        DataRow tableRow = table.NewRow();
                        if ("TRAFFIC_NEWLINE".Equals(IncreTableName) || "WATER_NEWLINE".Equals(IncreTableName) || "RESIDENTIAL_NEWLINE".Equals(IncreTableName) || "VEGETATION_NEWLINE".Equals(IncreTableName) || "SOIL_NEWLINE".Equals(IncreTableName))
                        {
                            tableRow["objectid"] = rd["objectid"].ToString();
                            tableRow["osmid"] = rd["osmid"].ToString();
                            tableRow["versionid"] = rd["versionid"].ToString();
                            tableRow["nationelename"] = rd["nationelename"].ToString();
                            tableRow["nationcode"] = rd["nationcode"].ToString();
                            //tableRow["shape"] = rd["shape"].ToString();
                            tableRow["geometry_wkt"] = rd["geometry_wkt"].ToString();
                            tableRow["changetype"] = rd["changetype"].ToString();
                        }
                        else
                        {
                            //专业矢量数据
                            tableRow["objectid"] = rd["objectid"].ToString();
                            //tableRow["osmid"] = rd["osmid"].ToString();
                            tableRow["versionid"] = rd["versionid"].ToString();
                            tableRow["nationelename"] = rd["nationelename"].ToString();
                            tableRow["nationcode"] = rd["nationcode"].ToString();
                            //tableRow["shape"] = rd["shape"].ToString();
                            tableRow["geometry_wkt"] = rd["geometry_wkt"].ToString();
                            tableRow["changetype"] = rd["changetype"].ToString();
                        }
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
        #endregion

        #region 水系要素匹配更新
        /// <summary>
        /// 水系要素更新主函数
        /// </summary>
        /// <param name="baseTableName">水系要素基态数据表</param>
        /// <param name="IncreTableName">水系要素增量数据表</param>
        /// <param name="constr">数据库连接字符</param>
        public void WaterEleUpdate(string baseTableName, string IncreTableName, string constr)
        {
            try
            {
                this.UpdatepgBar.Value = 0;
                OracleDBHelper helper = new OracleDBHelper();
                DataTable increTable = GetAllData(IncreTableName);//获得交通要素增量数据
                int increDataCount = increTable.Rows.Count;//统计交通要素增量数据总数
                for (int i = 0; i < increDataCount; i++)
                {
                    this.UpdatepgBar.Maximum = increDataCount;//控制进度条的最大值
                    UpdatepgBar.Value = i + 1;//更新进度条的
                    string increobjectid = increTable.Rows[i]["objectid"].ToString();
                    string increosmid = increTable.Rows[i]["osmid"].ToString();
                    string increversionid = increTable.Rows[i]["versionid"].ToString();
                    string increosmid_V = increTable.Rows[i]["osmid"].ToString() + "_" + increTable.Rows[i]["versionid"].ToString();
                    string increnationEleName = increTable.Rows[i]["nationelename"].ToString();
                    string increnationcode = increTable.Rows[i]["nationcode"].ToString();
                    string incregromStr_wkt = increTable.Rows[i]["geometry_wkt"].ToString();
                    string increchangeType = increTable.Rows[i]["changetype"].ToString();
                    DataTable baseTable = GetDataByBuffer(baseTableName, increosmid);//在基态数据库中搜索增量数据对象缓冲区范围内的所有对象数据
                    if (increchangeType =="create")
                    {
                        CreateToBaseDB(IncreTableName,baseTableName,increosmid, increobjectid, increnationEleName, increnationcode);
                        createWaterCount++;
                    }
                  else if (increchangeType == "delete")
                   {
                        string sql = string.Format("update {0} set updatestate=1  where {0}.osmid={1}", baseTableName, increosmid);
                        helper.sqlExecute(sql);
                        InsertToHistoryDB(IncreTableName, baseTableName, increosmid, increobjectid, increnationEleName, increnationcode);
                        deleteWaterCount++;
                  }
                 else if (increchangeType == "modify")
                 {
                     if (!overallMatching(baseTable,increosmid,increosmid_V, incregromStr_wkt, increnationEleName, constr, baseTableName, increchangeType))//整体匹配没有成功，则进行局部匹配
                    {
                        if (!partialMatch(baseTable, increosmid_V, incregromStr_wkt, increnationEleName, constr, baseTableName))
                        { //如果局部匹配也没成功，则说明基态数据中不存在与该对象可以匹配的对象，将其保留                               
                            dataRetain(increosmid, increversionid, IncreTableName, baseTableName, constr);
                        }
                    } 
                }
             }
           }
          catch (Exception ex)
           {
                MessageBox.Show(ex.ToString());
           }
       }
        #endregion

        #region 居民地要素匹配更新
        /// <summary>
        /// 居民地要素更新主函数
        /// </summary>
        /// <param name="baseTableName">居民地要素基态数据表</param>
        /// <param name="IncreTableName">居民地要素增量数据表</param>
        /// <param name="constr">数据库连接字符</param>
        public void ResidentialEleUpdate(string baseTableName, string IncreTableName, string constr)
        {
            try
            {
                this.UpdatepgBar.Value = 0;
                OracleDBHelper helper = new OracleDBHelper();
                DataTable increTable = GetAllData(IncreTableName);//获得交通要素增量数据
                int increDataCount = increTable.Rows.Count;//统计交通要素增量数据总数
                for (int i = 0; i < increDataCount; i++)
                {
                    this.UpdatepgBar.Maximum = increDataCount;//控制进度条的最大值
                    UpdatepgBar.Value = i + 1;//更新进度条的值
                    string increobjectid = increTable.Rows[i]["objectid"].ToString();
                    string increosmid = increTable.Rows[i]["osmid"].ToString();
                    string increversionid = increTable.Rows[i]["versionid"].ToString(); ;
                    string increosmid_V = increTable.Rows[i]["osmid"].ToString() + "_" + increTable.Rows[i]["versionid"].ToString();
                    string increnationEleName = increTable.Rows[i]["nationelename"].ToString();
                    string increnationcode = increTable.Rows[i]["nationcode"].ToString();
                    string incregromStr_wkt = increTable.Rows[i]["geometry_wkt"].ToString();
                    string increchangeType = increTable.Rows[i]["changetype"].ToString();
                    DataTable baseTable = GetDataByBuffer(baseTableName, increosmid);//在基态数据库中搜索增量数据对象缓冲区范围内的所有对象数据
                    if (baseTable.Rows.Count > 0)
                    {
                         if (increchangeType =="create")
                        {
                            CreateToBaseDB(IncreTableName,baseTableName,increosmid, increobjectid, increnationEleName, increnationcode);
                            createResidentialCount++;
                        }
                         else if (increchangeType == "delete")
                        {
                            string sql = string.Format("update {0} set updatestate=1  where {0}.osmid={1}", baseTableName, increosmid);
                            helper.sqlExecute(sql);
                            InsertToHistoryDB(IncreTableName, baseTableName, increosmid, increobjectid, increnationEleName, increnationcode);
                            deleteResidentialCount++;
                        }
                         else if (increchangeType == "modify")
                        {
                            if (!overallMatching(baseTable,increosmid,increosmid_V, incregromStr_wkt, increnationEleName, constr, baseTableName, increchangeType))//整体匹配没有成功，则进行局部匹配
                            {
                                if (!partialMatch(baseTable, increosmid_V, incregromStr_wkt, increnationEleName, constr, baseTableName))
                                { //如果局部匹配也没成功，则说明基态数据中不存在与该对象可以匹配的对象，将其保留                               
                                    dataRetain(increosmid, increversionid, IncreTableName, baseTableName, constr);
                                }
                            }
                        }
                    }
                    else
                    { //说明缓冲区内没有找到可能与该对象相匹配的对象，则该对象保留
                        dataRetain(increosmid, increversionid,IncreTableName, baseTableName, constr);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region 植被要素匹配更新
        /// <summary>
        /// 植被要素更新主函数
        /// </summary>
        /// <param name="baseTableName">植被要素基态数据表</param>
        /// <param name="IncreTableName">植被要素增量数据表</param>
        /// <param name="constr">数据库连接字符</param>
        public void VegetationEleUpdate(string baseTableName, string IncreTableName, string constr)
        {
            try
            {
                this.UpdatepgBar.Value = 0;
                OracleDBHelper helper = new OracleDBHelper();
                DataTable increTable = GetAllData(IncreTableName);//获得交通要素增量数据
                int increDataCount = increTable.Rows.Count;//统计交通要素增量数据总数
                for (int i = 0; i < increDataCount; i++)
                {
                    this.UpdatepgBar.Maximum = increDataCount;//控制进度条的最大值
                    UpdatepgBar.Value = i + 1;//更新进度条的值
                    string increobjectid = increTable.Rows[i]["objectid"].ToString();
                    string increosmid = increTable.Rows[i]["osmid"].ToString();
                    string increversionid = increTable.Rows[i]["versionid"].ToString(); 
                    string increosmid_V = increTable.Rows[i]["osmid"].ToString() + "_" + increTable.Rows[i]["versionid"].ToString();
                    string increnationEleName = increTable.Rows[i]["nationelename"].ToString();
                    string increnationcode = increTable.Rows[i]["nationcode"].ToString();
                    string incregromStr_wkt = increTable.Rows[i]["geometry_wkt"].ToString();
                    string increchangeType = increTable.Rows[i]["changetype"].ToString();
                    DataTable baseTable = GetDataByBuffer(baseTableName, increosmid);//在基态数据库中搜索增量数据对象缓冲区范围内的所有对象数据
                    if (increchangeType =="create")
                     {
                        CreateToBaseDB(IncreTableName,baseTableName,increosmid, increobjectid, increnationEleName, increnationcode);
                        createVegetationCount++;
                     }
                    else if (increchangeType == "delete")
                    {
                        string sql = string.Format("update {0} set updatestate=1  where {0}.osmid={1}", baseTableName, increosmid);
                        helper.sqlExecute(sql);
                        InsertToHistoryDB(IncreTableName, baseTableName, increosmid, increobjectid, increnationEleName, increnationcode);
                        deleteResidentialCount++;
                    }
                    else if (increchangeType == "modify")
                    {
                        if (!overallMatching(baseTable,increosmid,increosmid_V, incregromStr_wkt, increnationEleName, constr, baseTableName, increchangeType))//整体匹配没有成功，则进行局部匹配
                        {
                            if (!partialMatch(baseTable, increosmid_V, incregromStr_wkt, increnationEleName, constr, baseTableName))
                            { //如果局部匹配也没成功，则说明基态数据中不存在与该对象可以匹配的对象，将其保留                               
                                dataRetain(increosmid, increversionid, IncreTableName, baseTableName, constr);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region 土质要素匹配更新
        /// <summary>
        /// 土质要素更新主函数
        /// </summary>
        /// <param name="baseTableName">土质要素基态数据表</param>
        /// <param name="IncreTableName">土质要素增量数据表</param>
        /// <param name="constr">数据库连接字符</param>
        public void SoilEleUpdate(string baseTableName, string IncreTableName, string constr)
        {
            try
            {
                this.UpdatepgBar.Value = 0;
                OracleDBHelper helper = new OracleDBHelper();
                DataTable increTable = GetAllData(IncreTableName);//获得交通要素增量数据
                int increDataCount = increTable.Rows.Count;//统计交通要素增量数据总数
                for (int i = 0; i < increDataCount; i++)
                {
                    this.UpdatepgBar.Maximum = increDataCount;//控制进度条的最大值
                    UpdatepgBar.Value = i + 1;//更新进度条的值
                    string increobjectid = increTable.Rows[i]["objectid"].ToString();
                    string increosmid = increTable.Rows[i]["osmid"].ToString();
                    string increversionid = increTable.Rows[i]["versionid"].ToString();
                    string increosmid_V = increTable.Rows[i]["osmid"].ToString() + "_" + increTable.Rows[i]["versionid"].ToString();
                    string increnationEleName = increTable.Rows[i]["nationelename"].ToString();
                    string increnationcode = increTable.Rows[i]["nationcode"].ToString();
                    string incregromStr_wkt = increTable.Rows[i]["geometry_wkt"].ToString();
                    string increchangeType = increTable.Rows[i]["changetype"].ToString();
                    DataTable baseTable = GetDataByBuffer(baseTableName, increosmid);//在基态数据库中搜索增量数据对象缓冲区范围内的所有对象数据
                    if (increchangeType == "create")
                    {
                        CreateToBaseDB(IncreTableName,baseTableName,increosmid, increobjectid, increnationEleName, increnationcode);
                        createSoilCount++;
                    }
                    else if (increchangeType == "delete")
                    {
                        string sql = string.Format("update {0} set updatestate=1  where {0}.osmid={1}", baseTableName, increosmid);
                        helper.sqlExecute(sql);
                        InsertToHistoryDB(IncreTableName, baseTableName, increosmid, increobjectid, increnationEleName, increnationcode);
                        deleteSoilCount++;
                    }
                    else if (increchangeType == "modify")
                    {
                        if (!overallMatching(baseTable,increosmid,increosmid_V, incregromStr_wkt, increnationEleName, constr, baseTableName, increchangeType))//整体匹配没有成功，则进行局部匹配
                        {
                            if (!partialMatch(baseTable, increosmid_V, incregromStr_wkt, increnationEleName, constr, baseTableName))
                            { //如果局部匹配也没成功，则说明基态数据中不存在与该对象可以匹配的对象，将其保留                               
                                dataRetain(increosmid, increversionid, IncreTableName, baseTableName, constr);
                            }
                        }
                    }
                }
          }
        catch (Exception ex)
         {
            MessageBox.Show(ex.ToString());
         }
      }
        #endregion


        private void ThreadFun()
        {
            //线程操作
            //入库完成之后，清空缓存的文件
            string path = @"..\..\..\testfile\";
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
        //需要启动线程的方法
        private void Fun1()
        {
            Thread m_thread = new Thread(ThreadFun);
            m_thread.Start();
        }

      


    }
}
