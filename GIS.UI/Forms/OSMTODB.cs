using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using GIS.UI.AdditionalTool;
using GIS.UI.Entity;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using System.Threading;
using System.Diagnostics;

namespace GIS.UI.Forms
{
    public partial class OSMTODB : Form
    {

        private string conStr = null;//数据库连接字符串
        private string osmFilePath = "";
        Dictionary<string, int> osmEleCnt = new System.Collections.Generic.Dictionary<string, int>();
        private string fileNames = "";
        long nodeNum = 0, wayNum = 0, relationNum = 0;
        List<Ppoint> pointList = null;
        DateTime insertDBeginDateTime;
        private static string[] FClass = { "aerialway","aeroway","amenity","barrier","boundary","building","craft","emergency",
                                             "geological","highway","historic","landuse","leisure","man_made","military",
                                         "natural","office","place","power","public transport","railway","route","shop","sport","tourism","waterway"};

        OracleDBHelper helper;

        //OracleConnection con = new OracleConnection();
        public OSMTODB()
        {
            InitializeComponent();
        }
        public OSMTODB(string conStr)
        {
            InitializeComponent();
            this.conStr = conStr;
            //helper = new OracleDBHelper(conStr);

        }
        /// <summary>
        /// 选择要入库的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void osmFilebtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog osmFileDialog = new OpenFileDialog();
            osmFileDialog.Title = "输入osm文件";
            osmFileDialog.Filter = "osm文件|*.osm|txt文件|*.txt|XML文件|*.xml";
            osmFileDialog.Multiselect = true;
            if (osmFileDialog.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < osmFileDialog.FileNames.Length; i++)
                {
                    //string currentStorage = "当前入库：" + osmFileDialog.FileNames[i] + "[" + (i + 1) + "/" + osmFileDialog.FileNames.Length + "]\r\n";
                    //txtOSMPath.Text = currentStorage;
                    fileNames = osmFileDialog.FileNames[i];
                    txtOSMPath.Text = fileNames;//zh
                }
                

            }          
             
        }
       

        private void Closebtn_Click(object sender, EventArgs e)
        {
            Fun1();
            this.Close();
            
        }

        #region 进度条和提示lable的设置和更新委托
        public delegate void SetState(int maxValue, string info);
        public void SetMyState(int maxValue, string info)
        {
            this.oscPgBar.Minimum = 0;
            this.oscPgBar.Maximum = maxValue;
            this.exeStarteTBox.Text = info;
        }
        public delegate void UpdateState(int value, string info);
        public void UpdateMyState(int value, string info)
        {
            this.oscPgBar.Value = value;
            this.exeStarteTBox.Text = this.exeStarteTBox.Text + info;
        }
        public void UpdateMyStateInitInfo(int value, string info)
        {
            this.oscPgBar.Value = value;
            this.exeStarteTBox.Text = info;
        }
        #endregion

        #region 入库代码

        public void startImportDataToDB()
        {
            if (fileNames.Length > 0)
            {
                helper = new OracleDBHelper();
                OracleConnection con = helper.getOracleConnection();
                helper.createTable();
                Stopwatch swatch = new Stopwatch();
                swatch.Start();
                importDataToDB();
                swatch.Stop();
                string time = swatch.Elapsed.ToString();
                this.Invoke(new UpdateState(UpdateMyStateInitInfo), 0, "入库完成!");
                Console.WriteLine("该基态入库共花费时间：" + "{0}", time);
                
            }
            else 
            {
                MessageBox.Show("请选择要入库的文件！"); 
            }

        }

        public void importDataToDB()
        {
            insertDBeginDateTime = DateTime.Now;
            Layers.GetOscEleCnt(fileNames, out osmEleCnt);
            StreamReader sr = new StreamReader(fileNames, Encoding.UTF8);
            XmlTextReader xr = new XmlTextReader(sr);
            xr.WhitespaceHandling = WhitespaceHandling.None;
            List<Ppoint> pointlist = new List<Ppoint>();
            List<Poly> waylist = new List<Poly>();
            List<Poly> arealist = new List<Poly>();
            List<Relation> relationlist = new List<Relation>();
            while (xr.Read())
            {
                switch (xr.NodeType)
                {
                    case XmlNodeType.Element:
                        try
                        {
                            switch (xr.Name)
                            {
                                case "node": processPoint(xr, pointlist); break;
                                case "way": processWay(xr,arealist, waylist); break;
                                case "relation":processRelation(xr,relationlist) ; break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            return;
                        }
                        break;
                    default:
                        break;

                }


            }
        }

        public void processPoint(XmlTextReader xr, List<Ppoint> pointList)
        {
            OracleDBHelper odh = new OracleDBHelper();
            Ppoint point = new Ppoint();
            string wkt = "";


            point.setOsmid(long.Parse(xr.GetAttribute("id")));
            point.setLat(double.Parse(xr.GetAttribute("lat") == null ? "" : xr.GetAttribute("lat")));
            point.setLon(double.Parse(xr.GetAttribute("lon") == null ? "" : xr.GetAttribute("lon")));
            point.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
            point.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
            point.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
            point.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
            //point.setUserid(int.Parse(source.GetAttribute("uid")));
            string username = xr.GetAttribute("user") == null ? "" : xr.GetAttribute("user");
            username = username.Replace("'", "");
            username = username.Replace("\r", "");
            if ("?var Arnfj?re Bjarmason".Equals(username))
            {

            }
            //username = "?var Arnfj?re Bjarmason";
            username = username.Replace("?", "");
            point.setUsername(username);
            //node数据入库
            if (xr.IsEmptyElement)
            {
                if (xr.HasAttributes)
                {
                    point.issimple = true;

                }
            }
            else
            {
                point.issimple = false;
                if (xr.NodeType == XmlNodeType.Element && xr.Name == "node")
                {


                    string pointTags = "";
                    while (xr.Read() && xr.Name == "tag")
                    {
                        string k = xr.GetAttribute("k");
                        string v = xr.GetAttribute("v");
                        if (FClass.Contains(k))
                        {
                            point.setFc(k);
                            point.setDsg(v);
                        }
                        pointTags += "key=" + k + ",value=" + v + "|";
                    }

                    pointTags = pointTags.Replace("'", "");
                    pointTags = pointTags.Replace("\r", "");
                    pointTags = pointTags.Replace("\n", "");
                    point.setTags(pointTags);


                }
             
            }
            wkt = "POINT(" + point.getLon() + " " + point.getLat() + ")";
            //wkt = "POINT(" + point.getLon() + " " + point.getLat() + ")";
            point.setWkt(wkt);
            pointList.Add(point);


            nodeNum++;
            string timeinfo = "";
            string info = "";
            if (nodeNum == 1)
            {
                info = string.Format("正在入库点数据...\r\n\r\n    正在计算所需时间...  \r\n\r\n");
                this.Invoke(new SetState(SetMyState), osmEleCnt["nodeCnt"], info);
            }
            else if (nodeNum % 100 == 0)
            {
                odh.pointToOracle("APOINT", pointList);
                //odh.insertPointDataBySql("apoint", pointList);
                pointList.Clear();
                info = string.Format("正在入库点数据...\r\n\r\n    进度：{0}/{1}   \r\n\r\n{2}", (int)nodeNum, osmEleCnt["nodeCnt"], timeinfo);
                this.Invoke(new UpdateState(UpdateMyStateInitInfo), (int)nodeNum, info);
            }
            else if (nodeNum == osmEleCnt["nodeCnt"])
            {
                odh.pointToOracle("APOINT", pointList);
                //odh.insertPointDataBySql("apoint", pointList);
                pointList.Clear();
                info = string.Format("正在入库点数据...\r\n\r\n    进度：{0}/{1}   \r\n\r\n{2}", (int)nodeNum, osmEleCnt["nodeCnt"], timeinfo);
                this.Invoke(new UpdateState(UpdateMyStateInitInfo), (int)nodeNum, info);
            }
        }

        public void processWay(XmlTextReader xr,List<Poly> arealist, List<Poly> waylist)
        {
            wayNum++;
            string timeInfo = "";
            string info = "";
            if (wayNum == 1)
            {
                info = string.Format("点数据入库完成。\r\n\r\n正在入库线数据...\r\n\r\n    正在计算所需时间...  \r\n\r\n");
                this.Invoke(new SetState(SetMyState), osmEleCnt["wayCnt"], info);
            }
            else if (wayNum % 200 == 0)
            {
                //timeInfo = TimeHelper.GetLeftTimeStr(insertDBeginDateTime, (int)wayStatic, osmEleCnt["wayCnt"]);
                info = string.Format("点数据入库完成。\r\n\r\n正在入库线数据...\r\n\r\n    进度：{0}/{1}   \r\n\r\n{2}", (int)wayNum, osmEleCnt["wayCnt"], timeInfo);
                this.Invoke(new UpdateState(UpdateMyStateInitInfo), (int)wayNum, info);
            }
            else if (wayNum == osmEleCnt["wayCnt"])
            {
                info = string.Format("点数据入库完成。\r\n\r\n线数据入库完成\r\n\r\n    进度：{0}/{1}  ", (int)wayNum, osmEleCnt["wayCnt"]);
                this.Invoke(new UpdateState(UpdateMyStateInitInfo), (int)wayNum, info);
            }

            OracleDBHelper odh = new OracleDBHelper();
            if (xr.NodeType == XmlNodeType.Element && xr.Name == "way")
            {

                //if (pointList.Count() > 0)
                //{
                //    odh.insertPointDataBySql("apoint", pointList);
                //    pointList.Clear();
                //}//将剩余不足100000的点数据继续入库。
                //if (xr.GetAttribute("id") == null)
                //{
                //    continue;
                //}
                Poly poly = new Poly();
                //poly.setObjectid(wayObjectid);
                poly.setOsmid(long.Parse(xr.GetAttribute("id")));
                poly.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
                poly.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
                poly.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
                poly.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
                poly.setUsername(xr.GetAttribute("user") == null ? "" : xr.GetAttribute("user").Replace("'", "”"));
                //poly.setUsername(source.GetAttribute("user").Replace("'", "”"));

                string pointsId = "";
                string nodeid = "";
                string wayTags = "";
                List<string> refs = new List<string>();
                while (xr.Read() && xr.Name == "nd"||xr.Name=="tag")
                {
                    if (xr.Name == "nd")
                    {
                        nodeid = xr.GetAttribute("ref");
                        refs.Add(nodeid);
                        pointsId += nodeid + ",";
                    }
                    if( xr.Name == "tag")
                    {
                        string k = xr.GetAttribute("k");
                        string v = xr.GetAttribute("v");
                        if (FClass.Contains(k))
                        {
                            poly.setFc(k);
                            poly.setDsg(v);
                        }
                        wayTags += "key=" + k + ",value=" + v + ";";
                        //wayTags += "key=" + source.GetAttribute("k").ToString() + ",value=" + source.GetAttribute("v").ToString().Replace(",", "") + ";";
                    }
                }
                wayTags = wayTags.Replace("'", "”");
                poly.setTags(wayTags);
                if (pointsId.Length > 0)
                {
                    pointsId = pointsId.Substring(0, pointsId.Length - 1);
                }
                else
                {

                }
                poly.setPointsId(pointsId);
                if (refs[0].Equals(refs[refs.Count - 1]) && refs.Count > 3)
                {
                    arealist.Add(poly);
                    //areaObjectid++;
                }
                else
                {
                    waylist.Add(poly);
                    //wayObjectid++;
                }
              

                if (waylist.Count() == 200)
                {
                    //odh.wayToOracle("aline", waylist, helper.getOracleConnection());
                    odh.insertPolyDataBySql("aline", waylist);
                    waylist.Clear();
                }
                else if (wayNum == osmEleCnt["wayCnt"] && waylist.Count > 0)
                {
                    //odh.wayToOracle("aline", waylist, helper.getOracleConnection());
                    odh.insertPolyDataBySql("aline", waylist);
                    waylist.Clear();
                }


                if (arealist.Count() == 200)
                {
                    //odh.wayToOracle("apoly", arealist, helper.getOracleConnection());
                    odh.insertPolyDataBySql("apoly", arealist);
                    arealist.Clear();
                }
                else if (wayNum == osmEleCnt["wayCnt"] && arealist.Count > 0)
                {
                    //odh.wayToOracle("apoly", arealist, helper.getOracleConnection());
                    odh.insertPolyDataBySql("apoly", arealist);
                    arealist.Clear();
                }
            }

        }

        /*relation入库处理  by dy 2018.7.5*/
        /// <summary>
        /// 处理relation节点
        /// </summary>
        /// <param name="xr"></param>
        public void processRelation(XmlTextReader xr, List<Relation> relationList)
        {
            if (xr.IsEmptyElement)
            {
                return;
            }
            else
            {
                if (!xr.HasAttributes)
                {
                    return;
                }

                try
                {
                    relationNum++;
                    string timeInfo = "";
                    string info = "";
                    if (relationNum == 1)
                    {
                        info = string.Format("点、线数据入库完成。\r\n\r\n正在入库关系数据...\r\n\r\n    正在计算所需时间...  \r\n\r\n");
                        this.Invoke(new SetState(SetMyState), osmEleCnt["relationCnt"], info);
                    }
                    else if (relationNum % 100 == 0)
                    {
                        info = string.Format("点、线数据入库完成。\r\n\r\n正在入库关系数据...\r\n\r\n    进度：{0}/{1}   \r\n\r\n{2}", (int)relationNum, osmEleCnt["relationCnt"], timeInfo);
                        this.Invoke(new UpdateState(UpdateMyStateInitInfo), (int)relationNum, info);
                    }
                    else if (relationNum == osmEleCnt["relationCnt"])
                    {
                        info = string.Format("点、线数据入库完成。\r\n\r\n关系数据入库完成\r\n\r\n    进度：{0}/{1}  ", (int)relationNum, osmEleCnt["relationCnt"]);
                        this.Invoke(new UpdateState(UpdateMyStateInitInfo), (int)relationNum, info);
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                Relation relation = initialRelation(xr);
                int memberNum = 0, tagNum = 0;
                StringBuilder tagSb = new StringBuilder();
                StringBuilder allTagSb = new StringBuilder();
                StringBuilder keyTagSb = new StringBuilder();

                List<string> outerrels = new List<string>();
                List<string> innerrels = new List<string>();
                List<string> wayrelation = new List<string>();
                if (xr.NodeType == XmlNodeType.Element && xr.Name == "relation")
                {
                    bool isPolygon = false;
                    string areaid = "";
                    string areaIds = "";
                    while (xr.Read() && xr.Name == "member")
                    {

                        //List<string> refs = new List<string>();
                        if (xr.Name == "member")
                        {
                            string type = xr.GetAttribute("type");
                            string role = xr.GetAttribute("role");
                            if (xr.GetAttribute("ref") == null)
                            {
                                continue;
                            }
                            memberNum++;
                            areaid = xr.GetAttribute("ref");
                            //refs.Add(areaid);
                            areaIds += areaid + ",";
                            if (role == "outer")
                            {
                                outerrels.Add(areaid);
                            }
                            if (role == "inner")
                            {
                                innerrels.Add(areaid);
                            }
                        }
                    }
                    if (areaIds.Length > 0)
                    {
                        areaIds = areaIds.Substring(0, areaIds.Length - 1);
                    }
                    relation.setId(areaIds);
                    string wayTags = "";
                    while (xr.Read() && xr.Name == "tag")
                    {
                        tagNum++;
                        string k = xr.GetAttribute("k");
                        string v = xr.GetAttribute("v");
                        if (FClass.Contains(k))
                        {
                            relation.setFc(k);
                            relation.setDsg(v);
                            continue;
                        }
                        if (v == "multipolygon")
                        {
                            isPolygon = true;
                            relationList.Add(relation);
                            continue;
                        }
                        wayTags += "key=" + k + ",value=" + v + ";";
                        relation.setTags(wayTags);
                       
                    }

                    if (isPolygon == true)
                    {
                        helper.insertRelationDataBySql("apoly", relationList);

                    }
                }
            }
        }
        /// <summary>
        /// 初始化relation节点 by dy2017.7.5
        /// </summary>
        /// <param name="xr"></param>
        /// <returns></returns>
        private Relation initialRelation(XmlTextReader xr)
        {
            Relation relation = new Relation();
            relation.setOsmid(long.Parse(xr.GetAttribute("id")));
            relation.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
            relation.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
            relation.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
            relation.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
            relation.setUsername(xr.GetAttribute("user") == null ? "" : xr.GetAttribute("user").Replace("'", "”"));
            //relation.setOsmid() = long.Parse(xr.GetAttribute("id"));
            //relation.changeset = xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset");
            //node.timestamp = xr.GetAttribute("timestamp") == null ? DateTime.Now : DateTime.Parse(xr.GetAttribute("timestamp"));
            //node.username = xr.GetAttribute("user") == null ? "" : xr.GetAttribute("user");
            //node.userid = int.Parse(xr.GetAttribute("userid") == null ? "" : xr.GetAttribute("userid"));
            //node.version = xr.GetAttribute("version") == null ? (short)0 : short.Parse(xr.GetAttribute("version"));
            //node.visible = xr.GetAttribute("visible") == null ? false : bool.Parse(xr.GetAttribute("visible"));
            return relation;
        }
        //private Relation initialRelation(XmlTextReader xr)
        //{
        //    Relation node = new Relation();
        //    node.osmid = long.Parse(xr.GetAttribute("id"));
        //    node.changeset = xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset");
        //    node.timestamp = xr.GetAttribute("timestamp") == null ? DateTime.Now : DateTime.Parse(xr.GetAttribute("timestamp"));
        //    node.username = xr.GetAttribute("user") == null ? "" : xr.GetAttribute("user");
        //    node.userid = int.Parse(xr.GetAttribute("userid") == null ? "" : xr.GetAttribute("userid"));
        //    node.version = xr.GetAttribute("version") == null ? (short)0 : short.Parse(xr.GetAttribute("version"));
        //    node.visible = xr.GetAttribute("visible") == null ? false : bool.Parse(xr.GetAttribute("visible"));
        //    return node;
        //}


        private void button2_Click(object sender, EventArgs e)
        {
            new System.Threading.Thread(new System.Threading.ThreadStart(startImportDataToDB)).Start();
            
           

        }
        private void ThreadFun()
        {
            //线程操作
            //入库完成之后，清空缓存的文件byZYH
            string path = @"..\..\..\testfile";
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
        //需要启动线程的方法
        private void Fun1()
        {
            Thread m_thread = new Thread(ThreadFun);
            m_thread.Start();
        }
        
        #endregion



        

     
        #region 废弃
        ///// <summary>
        ///// 创建osmpoint,osmline,osmarea三个数据表
        ///// </summary>
        ///// <returns></returns>
        //private bool createBaseTable()
        //{
        //    OracleDBHelper helper = new OracleDBHelper();

        //    if (helper.IsExistTable("APOINT") || helper.IsExistTable("ALINE") || helper.IsExistTable("APOLY") || helper.IsExistTable("DAUL"))//zh修改
        //    {
        //        helper.DropTable("apoint");
        //        helper.DropTable("aline");
        //        helper.DropTable("apoly");
        //        helper.DropTable("daul");
        //        if (helper.CreateTable("Osm\\CreateTableOsmPoint") && helper.CreateTable("Osm\\CreateTableOsmLine") && helper.CreateTable("Osm\\CreateTableOsmArea") && helper.CreateTable("Osm\\Daul"))//zh修改
        //        {
        //            return true;
        //        }
        //    }
        //    else
        //    {
        //        if (helper.CreateTable("Osm\\CreateTableOsmPoint") && helper.CreateTable("Osm\\CreateTableOsmLine") && helper.CreateTable("Osm\\CreateTableOsmArea") && helper.CreateTable("Osm\\Daul"))//zh修改
        //        {
        //            return true;
        //        }

        //    }
        //    return false;

        //}
        /// <summary>
        /// 创建osmpoint、osmline、osmarea三个数据表
        /// </summary>
        /// <returns></returns>
        //private bool createBaseTable()
        //{
        //    OracleDBHelper odh = new OracleDBHelper(conStr);
        //    if (odh.IsExistTable("osmpoint") || odh.IsExistTable("osmline") || odh.IsExistTable("osmarea") || odh.IsExistTable("osmmultiline"))
        //    {
        //        odh.DropTable("osmpoint");
        //        odh.DropTable("osmline");
        //        odh.DropTable("osmarea");
        //        odh.DropTable("osmmultiline");
        //        if (odh.CreateTable("Osm\\CreateTableOsmPoint") && odh.CreateTable("Osm\\CreateTableOsmLine") && odh.CreateTable("Osm\\CreateTableOsmArea") && odh.CreateTable("Osm\\CreateTableOsmMultiLine"))
        //        {
        //            return true;
        //        }
        //    }
        //    else
        //    {
        //        if (odh.CreateTable("Osm\\CreateTableOsmPoint") && odh.CreateTable("Osm\\CreateTableOsmLine") && odh.CreateTable("Osm\\CreateTableOsmArea") && odh.CreateTable("Osm\\CreateTableOsmMultiLine"))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        #endregion
    }
}
