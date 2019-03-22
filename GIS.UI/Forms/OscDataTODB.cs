using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.UI.AdditionalTool;
using GIS.UI.Entity;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Diagnostics;

namespace GIS.UI.Forms
{
    public partial class OscDataTODB : Form
    {

        private string conStr = null;
        Dictionary<string, int> oscEleCnt = new System.Collections.Generic.Dictionary<string, int>();
        //private string oscFileNames = "";
        string[] oscFileNames = { null };
        long nodeNum = 0, wayNum = 0, relationNum = 0;
        List<OscDataNode> oscpointlist = new List<OscDataNode>();
        List<OscDataWay> oscwaylist = new List<OscDataWay>();
        List<OscDataRelation> relationlist = new List<OscDataRelation>();
        List<OscDataWay> oscarealist = new List<OscDataWay>();
        DateTime insertDBeginDateTime;
        private static string[] FClass = { "aerialway","area:highway","aeroway","amenity","barrier","boundary","building","craft","emergency",
                                             "geological","highway","historic","landuse","leisure","man_made","military","source",
                                         "natural","office","place","power","public transport","railway","route","shop","sport","tourism","waterway"};

        OracleDBHelper helper;
        public OscDataTODB()
        {
            InitializeComponent();
        }
        public OscDataTODB(string conStr)
        {
            InitializeComponent();
            this.conStr = conStr;
        }

        /// <summary>
        /// 选择osm增量入库文件  by zbl 2018.7.3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OscFilebtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog oscopenFileDlg = new OpenFileDialog();
        #region  废弃的打开文件代码
            //oscopenFileDlg.Title = "输入osc文件";
            //oscopenFileDlg.Filter = "增量osc文件|*.osc|txt文件|*.txt|XML文件|*.xml";
            //oscopenFileDlg.Multiselect = true;
            //if (oscopenFileDlg.ShowDialog() == DialogResult.OK)
            //{
            //    oscFileNames = oscopenFileDlg.FileNames[0];
            //    this.OscOpenTBox.Text = Path.GetDirectoryName(oscFileNames);
            //    for (int i = 0; i < oscopenFileDlg.FileNames.Length; i++)
            //    {
            //        oscFileNames = oscopenFileDlg.FileNames[i];
            //        this.OscOpenTBox.Text = oscFileNames;
            //    }
            //}
            //string messageText = null;
            //if (oscopenFileDlg.FileNames.Length > 0 && oscopenFileDlg.FileNames[0] != null)
            //{
            //    messageText = "输入文件" + oscopenFileDlg.FileNames.Length + "个\r\n";
            //    for (int i = 0; i < oscopenFileDlg.FileNames.Length; i++)
            //    {
            //        messageText += Path.GetFileName(oscopenFileDlg.FileNames[i]) + "\r\n";
            //    }
            //}
            //else
            //{
            //    messageText = "没有输入文件";
            //}
            //this.OSCexeStarteTBox.Text = messageText;
       #endregion
            oscopenFileDlg.Filter = "增量文件(*.osc)|*.osc";//by zbl 2018.8.1修改
            oscopenFileDlg.Multiselect = true;
            if (oscopenFileDlg.ShowDialog() == DialogResult.OK)
            {
                oscFileNames = oscopenFileDlg.FileNames;
                this.OSCexeStarteTBox .Text = Path.GetDirectoryName(oscFileNames[0]);
            }
            string messageText = null;
            if (oscFileNames.Length > 0 && oscFileNames[0] != null)
            {
                messageText = "输入文件" + oscFileNames.Length + "个\r\n";
                for (int i = 0; i < oscFileNames.Length; i++)
                {
                    messageText += Path.GetFileName(oscFileNames[i]) + "\r\n";
                }
            }
            else
            {
                messageText = "没有输入文件";
            }
            this.OSCexeStarteTBox .Text = messageText;

        }

        #region 进度条和提示lable的设置和更新委托
        public delegate void SetState(int maxValue, string info);
        public void SetMyState(int maxValue, string info)
        {
            this.OscPgBar.Minimum = 0;
            this.OscPgBar.Maximum = maxValue;
            this.OSCexeStarteTBox.Text = info;
        }
        public delegate void UpdateState(int value, string info);
        public void UpdateMyState(int value, string info)
        {
            this.OscPgBar.Value = value++;
            this.OSCexeStarteTBox.Text += info;
        }
        public void UpdateMyStateInitInfo(int value, string info)
        {
            this.OscPgBar.Value = value++;
            this.OSCexeStarteTBox.Text = info;
        }
        #endregion

        private void 开始入库Btn_Click(object sender, EventArgs e)
        {
            helper = new OracleDBHelper();//by zbl 20180801 修改
            OracleConnection con = helper.getOracleConnection();//连接数据库实例
            helper.createOscTable();//创建增量数据表
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            if (oscFileNames.Length > 0 && oscFileNames[0] != null && con.State == ConnectionState.Open)
            {
                Control.CheckForIllegalCrossThreadCalls = false;//处理不能跨进程操作问题 by zbl 20180802
                
                Thread storeOsc = new Thread(StoreOscFiles);//多进程读取文件入库
                storeOsc.Start();
            }
            else
            {
                this.OSCexeStarteTBox.Text = "请确认数据库是否已连接以及输入的增量数据文件！";
            }
        }
        /// <summary>
        /// 增量数据入库进度条控制
        /// </summary>
        private void StoreOscFiles()
        {
            Stopwatch swatch = new Stopwatch();
            swatch.Start();
            for (int i = 0; i < oscFileNames.Length; i++)
            {
                int nMaxValue = 0;
                this.OSCexeStarteTBox.Text = "进度：" + (i + 1) + "/" + oscFileNames.Length + "\r\n正在入库：" + Path.GetFileName(this.oscFileNames[i]);
                nMaxValue = oscFileNames.Length;//by zbl 20180802 修改进度条
                OscPgBar.Maximum = nMaxValue;//设置进度条的最大值
                if (GetOscEleCnt(this.oscFileNames[i], out oscEleCnt))
                {
                    importOSCDataToDB(i);//增量数据入库函数
                    OscPgBar.Value++;// by zbl 20180802 修改，更新进度条状态
                }
            }
            swatch.Stop();
            string time = swatch.Elapsed.ToString();
            this.OSCexeStarteTBox .Text = "OSC文件入库已完成！";
            Console.WriteLine("该增量数据入库共花费时间：" + "{0}", time);
            AddMap.Fun1();

            }
        
        /// <summary>
        /// 统计增量文件点线面数据的总数量
        /// </summary>
        /// <param name="fileName">增量文件名</param>
        /// <param name="oscFileEleCnt"></param>
        /// <returns></returns>
        public static bool GetOscEleCnt(string fileName, out Dictionary<string, int> oscFileEleCnt)
        {
            oscFileEleCnt = new Dictionary<string, int>();
            try
            {
                using (XmlTextReader reader = new XmlTextReader(fileName))//XML文件格式读取函数
                {
                    int nodeCnt = 0; int wayCnt = 0; int relationCnt = 0;//点线面数据量统计字段
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "node": nodeCnt += 1; break;
                                case "way": wayCnt += 1; break;
                                case "relation": relationCnt += 1; break;
                                default: break;
                            }
                        }
                    }
                    oscFileEleCnt.Add("nodeCnt", nodeCnt);
                    oscFileEleCnt.Add("wayCnt", wayCnt);
                    oscFileEleCnt.Add("relationCnt", relationCnt);
                    oscFileEleCnt.Add("eleCnt", nodeCnt + relationCnt + wayCnt);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// OSM增量数据入库主函数入口
        /// </summary>
        public void importOSCDataToDB(object i)
        {
            insertDBeginDateTime = DateTime.Now;
            Layers.GetOscEleCnt(oscFileNames[int.Parse(i.ToString())], out oscEleCnt);
            //StreamReader sr = new StreamReader(oscFileNames[int.Parse(i.ToString())], Encoding.UTF8);
            //XmlTextReader xr = new XmlTextReader(sr);
            
            using (XmlTextReader xr = new XmlTextReader(this.oscFileNames[int.Parse(i.ToString())]))
            {
                xr.WhitespaceHandling = WhitespaceHandling.None;
                while (xr.Read())
                {
                    try
                    {
                        switch (xr.Name)
                        {
                            case "modify": processOscModify(xr); break;
                            case "delete": processOscDelete(xr); break;
                            case "create": processOscCreate(xr); break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 处理osm增量变化类型为“modify”类型的OSM增量数据 。 by zbl 20180709修改
        /// </summary>
        /// <param name="xr"></param>
        public void processOscModify(XmlTextReader xr)
        {
            string changType1;
            changType1 = "modify";
            //int Cnt = 0;
            OracleDBHelper odh = new OracleDBHelper();
            OscDataNode Node = new OscDataNode();
            OscDataWay way = new OscDataWay();
            OscDataRelation relation = new OscDataRelation();
            while (xr.Read())
            {
                if ((xr.Name == "modify" || xr.Name == "create" || xr.Name == "delete") && xr.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
                /*changeType=="modify"时对点、线、面入库的处理*/
                #region 点增量数据入库处理OscDataNode_TO_OracleDB   by zbl 20180705
                if (xr.Name == "node" && xr.NodeType == XmlNodeType.Element)//判断xml文件第三级节点类型
                {
                    //Cnt += 1;
                    string wkt = "";

                    Node.setOscid(long.Parse(xr.GetAttribute("id")));
                    Node.setLat(double.Parse(xr.GetAttribute("lat") == null ? "-1" : xr.GetAttribute("lat")));
                    Node.setLon(double.Parse(xr.GetAttribute("lon") == null ? "-1" : xr.GetAttribute("lon")));
                    Node.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
                    Node.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
                    Node.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
                    Node.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
                    Node.setChangetype(changType1);
                    string username = xr.GetAttribute("user") == null ? "" : xr.GetAttribute("user");
                    username = username.Replace("'", "");
                    username = username.Replace("\r", "");
                    if ("?var Arnfj?re Bjarmason".Equals(username))
                    {
                    }
                    username = username.Replace("?", "");
                    Node.setUsername(username);
                    // 判断OSM增量node类型是否为简单点数据，值为false进行读取tag标签属性。
                    if (xr.IsEmptyElement)
                    {
                        if (xr.HasAttributes)
                        {
                            Node.issimple = true;
                        }
                    }
                    else
                    {
                        Node.issimple = false;
                        if (xr.NodeType == XmlNodeType.Element && xr.Name == "node")
                        {
                            string pointTags = "";
                            while (xr.Read() && xr.Name == "tag")
                            {
                                string k = xr.GetAttribute("k");
                                string v = xr.GetAttribute("v");
                                if (FClass.Contains(k))
                                {
                                    Node.setFc(k);
                                    Node.setDsg(v);
                                }
                                pointTags += "key=" + k + ",value=" + v + "||";
                            }
                            pointTags = pointTags.Replace("'", "");
                            pointTags = pointTags.Replace("\r", "");
                            pointTags = pointTags.Replace("\n", "");
                            Node.setTags(pointTags);
                        }
                    }
                    wkt = "POINT(" + Node.getLon() + " " + Node.getLat() + ")";
                    Node.setWkt(wkt);
                    oscpointlist.Add(Node);
                    nodeNum++;
                    if (nodeNum > 0)
                    {
                        //odh.OscpointToOracle("OSCPOINT", oscpointlist);
                        odh.insertoscPointDataBySql("OSCPOINT", oscpointlist);
                        oscpointlist.Clear();                    
                    }
                    else if (nodeNum % 50 == 0)
                    {
                        //odh.OscpointToOracle("OSCPOINT", oscpointlist);
                        odh.insertoscPointDataBySql("OSCPOINT", oscpointlist);
                        oscpointlist.Clear();
                    }
                    else if (nodeNum == oscEleCnt["nodeCnt"])
                    {
                        //odh.OscpointToOracle("OSCPOINT", oscpointlist);
                        odh.insertoscPointDataBySql("OSCPOINT", oscpointlist);
                        oscpointlist.Clear();
                    }
                }
                #endregion
                #region 线面增量数据入库处理OscDataWay_TO_OracleDB   by zbl 20180705
                else if (xr.Name == "way" && xr.NodeType == XmlNodeType.Element)//OscDataWay_TO_OracleDB   by zbl 2018.7.5
                {
                    //Cnt += 1;
                    //way.setChangetype(changType1);
                    //processOscWay(xr, oscarealist, oscwaylist);
                    wayNum++;
                    if (wayNum == 1)
                    {
                    }
                    else if (wayNum % 200 == 0)
                    {
                    }
                    else if (wayNum == oscEleCnt["wayCnt"])
                    {
                    }
                    if (xr.Name == "way" && xr.NodeType == XmlNodeType.Element)
                    {
                        way.setOscid(long.Parse(xr.GetAttribute("id")));
                        way.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
                        way.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
                        way.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
                        way.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
                        way.setChangetype(changType1);
                        way.setUsername(xr.GetAttribute("user") == null ? "-1" : xr.GetAttribute("user").Replace("'", "”"));
                        string pointsId = "";
                        string nodeid = "";
                        string wayTags = "";
                        List<string> refs = new List<string>();
                        while (xr.Read() && xr.Name == "nd" || xr.Name == "tag")
                        {
                            if (xr.Name == "nd")
                            {
                                nodeid = xr.GetAttribute("ref");
                                refs.Add(nodeid);
                                pointsId += nodeid + ",";
                            }
                            if (xr.Name == "tag")
                            {
                                string k = xr.GetAttribute("k");
                                string v = xr.GetAttribute("v");
                                if (FClass.Contains(k))
                                {
                                    way.setFc(k);
                                    way.setDsg(v);
                                }
                                wayTags += "key=" + k + ",value=" + v + ";";
                            }
                        }
                        wayTags = wayTags.Replace("'", "”");
                        way.setTags(wayTags);
                        if (pointsId.Length > 0)
                        {
                            pointsId = pointsId.Substring(0, pointsId.Length - 1);
                        }
                        else
                        {
                        }
                        way.setPointsld(pointsId);
                        if (refs.Count == 0)
                        {
                            oscwaylist.Add(way);
                        }
                        else
                        {
                            if (refs[0].Equals(refs[refs.Count - 1]) && refs.Count > 3)
                            {
                                oscarealist.Add(way);
                            }
                            else
                            {
                                oscwaylist.Add(way);
                            }
                        }
                        if (oscarealist.Count() >0 )
                        {
                            odh.insertoscWayDataBySql("OSCAREA", oscarealist);
                            oscarealist.Clear();
                        }
                        else if (oscarealist.Count() == 200)//BY zbl 20180925修改 由’>‘改为‘==’。
                        {
                            odh.insertoscWayDataBySql("OSCAREA", oscarealist);
                            oscarealist.Clear();
                        }
                        if (oscwaylist.Count() > 0)
                        {
                            odh.insertoscWayDataBySql("OSCLINE", oscwaylist);
                            oscwaylist.Clear();
                        }
                        else if (oscwaylist.Count == 200)//BY zbl 20180925修改 由’>‘改为‘==’。
                        {
                            odh.insertoscWayDataBySql("OSCLINE", oscwaylist);
                            oscwaylist.Clear();
                        }
                    }
                }
                #endregion
                #region 关系数据入库处理OscDataRelation_TO_OracleDB   by zbl 2018.7.5
                else if (xr.Name == "relation" && xr.NodeType == XmlNodeType.Element)
                {
                    //Cnt += 1;
                    if (xr.IsEmptyElement) { return; }
                    else
                    {
                        if (!xr.HasAttributes)
                        {
                            return;
                        }
                        try
                        {
                            relationNum++;
                            if (relationNum == 1)
                            {
                            }
                            else if (relationNum % 100 == 0)
                            {
                            }
                            else if (relationNum == oscEleCnt["relationCnt"])
                            {
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                        relation.setOscid(long.Parse(xr.GetAttribute("id")));
                        relation.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
                        relation.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
                        relation.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
                        relation.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
                        relation.setUsername(xr.GetAttribute("user") == null ? "-1" : xr.GetAttribute("user").Replace("'", "”"));
                        relation.setChangeType(changType1);

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
                                    relationlist.Add(relation);
                                    continue;
                                }
                                wayTags += "key=" + k + ",value=" + v + ";";
                                relation.setTags(wayTags);
                            }
                            if (isPolygon == true)
                            {
                                helper.insertoscRelationDataBySql("OSCAREA", relationlist);
                            }
                        }
                    }
                }
            }
            //this.OscPgBar.Value = Cnt * 100 / oscEleCnt["eleCnt"];
                #endregion 
        }

        /// <summary>
        /// 处理osm增量变化类型为“delete”类型的osc数据  by zbl 2018.7.9修改
        /// </summary>
        /// <param name="xr"></param>
        public void processOscDelete(XmlTextReader  xr) 
        {
            string changType2 ;
            changType2 = "delete";
            OracleDBHelper odh = new OracleDBHelper();
            OscDataNode Node = new OscDataNode();
            OscDataWay way = new OscDataWay();
            OscDataRelation relation = new OscDataRelation();
            //int Cnt = 0;
            while (xr.Read())
            {
                if ((xr.Name == "modify" || xr.Name == "create" || xr.Name == "delete") && xr.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
                /*changeType=="delete"时对点、线、面入库的处理*/
                #region 点增量数据入库处理OscDataNode_TO_OracleDB   by zbl 2018.7.5
                if (xr.Name == "node" && xr.NodeType == XmlNodeType.Element)//OscDataNode_TO_OracleDB   by zbl 2018.7.5
                {
                    //Cnt += 1;
                    string wkt = "";

                    Node.setOscid(long.Parse(xr.GetAttribute("id")));
                    Node.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
                    Node.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
                    Node.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
                    Node.setChangetype(changType2);
                    Node.setLat(double.Parse(xr.GetAttribute("lat") == null ? "-1" : xr.GetAttribute("lat")));
                    Node.setLon(double.Parse(xr.GetAttribute("lon") == null ? "-1" : xr.GetAttribute("lon")));
                    Node.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
                    //Node.setLat (double.Parse (lat));
                    //Node.setLon(double.Parse (lon));
                    //Node.setUserid(int.Parse(uid));
                    string username = xr.GetAttribute("user") == null ? "" : xr.GetAttribute("user");
                    username = username.Replace("'", "");
                    username = username.Replace("\r", "");
                    if ("?var Arnfj?re Bjarmason".Equals(username))
                    {

                    }
                    username = username.Replace("?", "");
                    Node.setUsername(username);
                    // 增量node数据入库
                    if (xr.IsEmptyElement)
                    {
                        if (xr.HasAttributes)
                        {
                            Node.issimple = true;
                        }
                    }
                    else
                    {
                        Node.issimple = false;
                        if (xr.NodeType == XmlNodeType.Element && xr.Name == "node")
                        {
                            string pointTags = "";
                            while (xr.Read() && xr.Name == "tag")
                            {
                                string k = xr.GetAttribute("k");
                                string v = xr.GetAttribute("v");
                                if (FClass.Contains(k))
                                {
                                    Node.setFc(k);
                                    Node.setDsg(v);
                                }
                                pointTags += "key=" + k + ",value=" + v + "|";
                            }
                            pointTags = pointTags.Replace("'", "");
                            pointTags = pointTags.Replace("\r", "");
                            pointTags = pointTags.Replace("\n", "");
                            Node.setTags(pointTags);
                        }
                    }
                    wkt = "POINT(" + Node.getLon() + " " + Node.getLat() + ")";
                    Node.setWkt(wkt);
                    oscpointlist.Add(Node);
                    nodeNum++;             
                    if (nodeNum>0)
                    {
                        //odh.OscpointToOracle("OSCPOINT", oscpointlist);
                        odh.insertoscPointDataBySql("OSCPOINT", oscpointlist);
                        oscpointlist.Clear();
                    }
                    else if (nodeNum % 50 == 0)
                    {
                        //odh.OscpointToOracle("OSCPOINT", oscpointlist);
                        odh.insertoscPointDataBySql("OSCPOINT", oscpointlist);
                        oscpointlist.Clear();
                    }
                    else if (nodeNum == oscEleCnt["nodeCnt"])
                    {
                        //odh.OscpointToOracle("OSCPOINT", oscpointlist);
                        odh.insertoscPointDataBySql("OSCPOINT", oscpointlist);
                        oscpointlist.Clear();
                    }
                }
                #endregion
                #region 增量线面数据入库处理OscDataWay_TO_OracleDB   by zbl 2018.7.5
                else if (xr.Name == "way" && xr.NodeType == XmlNodeType.Element)
                {
                    //Cnt += 1;
                    //processOscWay(xr,oscarealist, oscwaylist);
                    //way.setChangetype(changType2);
                    wayNum++;
                    if (wayNum == 1)
                    {
                    }
                    else if (wayNum % 200 == 0)
                    {
                    }
                    else if (wayNum == oscEleCnt["wayCnt"])
                    {
                    }

                    if (xr.Name == "way" && xr.NodeType == XmlNodeType.Element)//OscDataWay_TO_OracleDB   by zbl 2018.7.5
                    {
                        way.setOscid(long.Parse(xr.GetAttribute("id")));
                        way.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
                        way.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
                        way.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
                        way.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
                        way.setChangetype(changType2);
                        way.setUsername(xr.GetAttribute("user") == null ? "-1" : xr.GetAttribute("user").Replace("'", "”"));
                        string pointsId = "";
                        string nodeid = "";
                        string wayTags = "";
                        List<string> refs = new List<string>();
                        while (xr.Read() && xr.Name == "nd" || xr.Name == "tag")
                        {
                            if (xr.Name == "nd")
                            {
                                nodeid = xr.GetAttribute("ref");
                                refs.Add(nodeid);
                                pointsId += nodeid + ",";
                            }

                            if (xr.Name == "tag")
                            {
                                string k = xr.GetAttribute("k");
                                string v = xr.GetAttribute("v");
                                if (FClass.Contains(k))
                                {
                                    way.setFc(k);
                                    way.setDsg(v);
                                }
                                wayTags += "key=" + k + ",value=" + v + ";";
                            }
                        }
                        wayTags = wayTags.Replace("'", "”");
                        way.setTags(wayTags);
                        if (pointsId.Length > 0)
                        {
                            pointsId = pointsId.Substring(0, pointsId.Length - 1);
                        }
                        else
                        {
                        }
                        way.setPointsld(pointsId);
                        if (refs.Count == 0)
                        { 
                            oscwaylist.Add(way); 
                        }
                        else
                        {
                            if (refs[0].Equals(refs[refs.Count - 1]) && refs.Count > 3)
                            {
                                oscarealist.Add(way);
                            }
                            else
                            {
                                oscwaylist.Add(way);
                            }
                        }
                        if (oscarealist.Count() > 0)
                        {
                            odh.insertoscWayDataBySql("OSCAREA", oscarealist);
                            oscarealist.Clear();
                        }
                        else if (oscarealist.Count() > 200)//20180925修改
                        {
                            odh.insertoscWayDataBySql("OSCAREA", oscarealist);
                            oscarealist.Clear();
                        }
                        if (oscwaylist.Count() > 0)
                        {
                            odh.insertoscWayDataBySql("OSCLINE", oscwaylist);
                            oscwaylist.Clear();
                        }
                        else if (oscwaylist.Count > 200)//20180925修改
                        {
                            odh.insertoscWayDataBySql("OSCLINE", oscwaylist);
                            oscwaylist.Clear();
                        }
                    }
                }
       #endregion
                #region 关系数据增量入库处理 OscDataRelation_TO_OracleDB   by zbl 2018.7.5
                else if (xr.Name == "relation" && xr.NodeType == XmlNodeType.Element)
                {
                    //Cnt += 1;
                    if (xr.IsEmptyElement) { return; }
                    else
                    {
                        if (!xr.HasAttributes)
                        {
                            return;
                        }

                        try
                        {
                            relationNum++;
                            if (relationNum == 1)
                            {
                            }
                            else if (relationNum % 100 == 0)
                            {
                            }
                            else if (relationNum == oscEleCnt["relationCnt"])
                            {
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        relation.setOscid(long.Parse(xr.GetAttribute("id")));
                        relation.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
                        relation.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
                        relation.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
                        relation.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
                        relation.setUsername(xr.GetAttribute("user") == null ? "-1" : xr.GetAttribute("user").Replace("'", "”"));
                        relation.setChangeType(changType2);

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
                                    relationlist.Add(relation);
                                    continue;
                                }
                                wayTags += "key=" + k + ",value=" + v + ";";
                                relation.setTags(wayTags);
                            }

                            if (isPolygon == true)
                            {
                                helper.insertoscRelationDataBySql("OSCAREA", relationlist);
                            }
                        }
                    }
                 }
              }
            //this.OscPgBar.Value = Cnt * 100 / oscEleCnt["eleCnt"];
#endregion
        }

        /// <summary>
        /// 处理osm增量变化类型为“create”类型的osc数据  by zbl 2018.7.9修改
        /// </summary>
        /// <param name="xr"></param>
        public void processOscCreate(XmlTextReader  xr)
        {
            string changType3;
            changType3 = "create";
            OracleDBHelper odh = new OracleDBHelper();
            OscDataNode Node = new OscDataNode();
            OscDataWay way = new OscDataWay();
            OscDataRelation relation = new OscDataRelation();
            //int Cnt = 0;
            while (xr.Read())
            {
                if ((xr.Name == "modify" || xr.Name == "create" || xr.Name == "delete") && xr.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
                /*changeType=="create"时对点、线、面入库的处理*/
                 #region 增量点数据入库处理OscDataNode_TO_OracleDB   by zbl 2018.7.5
                if (xr.Name == "node" && xr.NodeType == XmlNodeType.Element)//OscDataNode_TO_OracleDB   by zbl 2018.7.5
                {
                    //Cnt += 1;
                    string wkt = "";

                    Node.setOscid(long.Parse(xr.GetAttribute("id")));
                    Node.setLat(double.Parse(xr.GetAttribute("lat") == null ? "-1" : xr.GetAttribute("lat")));
                    Node.setLon(double.Parse(xr.GetAttribute("lon") == null ? "-1" : xr.GetAttribute("lon")));
                    Node.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
                    Node.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
                    Node.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
                    Node.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
                    Node.setChangetype(changType3);
                    string username = xr.GetAttribute("user") == null ? "-1" : xr.GetAttribute("user");
                    username = username.Replace("'", "");
                    username = username.Replace("\r", "");
                    if ("?var Arnfj?re Bjarmason".Equals(username))
                    {

                    }
                    username = username.Replace("?", "");
                    Node.setUsername(username);
                    // 增量node数据入库
                    if (xr.IsEmptyElement)
                    {
                        if (xr.HasAttributes)
                        {
                            Node.issimple = true;
                        }
                    }
                    else
                    {
                        Node.issimple = false;
                        if (xr.NodeType == XmlNodeType.Element && xr.Name == "node")
                        {
                            string pointTags = "";
                            while (xr.Read() && xr.Name == "tag")
                            {
                                string k = xr.GetAttribute("k");
                                string v = xr.GetAttribute("v");
                                if (FClass.Contains(k))
                                {
                                    Node.setFc(k);
                                    Node.setDsg(v);
                                }
                                pointTags += "key=" + k + ",value=" + v + "|";
                            }

                            pointTags = pointTags.Replace("'", "");
                            pointTags = pointTags.Replace("\r", "");
                            pointTags = pointTags.Replace("\n", "");
                            Node.setTags(pointTags);

                        }
                    }
                    wkt = "POINT(" + Node.getLon() + " " + Node.getLat() + ")";
                    Node.setWkt(wkt);
                    oscpointlist.Add(Node);
                    nodeNum++;
                    if (nodeNum >0)
                    {
                        //odh.OscpointToOracle("OSCPOINT", oscpointlist);
                        odh.insertoscPointDataBySql("OSCPOINT", oscpointlist);
                        oscpointlist.Clear();
                    }
                    else if (nodeNum % 50 == 0)
                    {
                        //odh.OscpointToOracle("OSCPOINT", oscpointlist);
                        odh.insertoscPointDataBySql("OSCPOINT", oscpointlist);
                        oscpointlist.Clear();
                    }
                    else if (nodeNum == oscEleCnt["nodeCnt"])
                    {
                        //odh.OscpointToOracle("OSCPOINT", oscpointlist);
                        odh.insertoscPointDataBySql("OSCPOINT", oscpointlist);
                        oscpointlist.Clear();
                    }
                }
#endregion
                 #region 增量线面数据入库处理 OscDataWay_TO_OracleDB   by zbl 2018.7.5
                else if (xr.Name == "way" && xr.NodeType == XmlNodeType.Element)//OscDataWay_TO_OracleDB   by zbl 2018.7.5
                {
                    //Cnt += 1;       
                    //way.setChangetype(changType3);
                    wayNum++;
                    if (wayNum == 1)
                    {
                    }
                    else if (wayNum % 200 == 0)
                    {
                    }
                    else if (wayNum == oscEleCnt["wayCnt"])
                    {
                    }

                    if (xr.Name == "way" && xr.NodeType == XmlNodeType.Element)
                    {
                        way.setOscid(long.Parse(xr.GetAttribute("id")));
                        way.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
                        way.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
                        way.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
                        way.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
                        way.setChangetype(changType3);
                        way.setUsername(xr.GetAttribute("user") == null ? "-1" : xr.GetAttribute("user").Replace("'", "”"));
                        string pointsId = "";
                        string nodeid = "";
                        string wayTags = "";
                        List<string> refs = new List<string>();
                        while (xr.Read() && xr.Name == "nd" || xr.Name == "tag")
                        {
                            if (xr.Name == "nd")
                            {
                                nodeid = xr.GetAttribute("ref");
                                refs.Add(nodeid);
                                pointsId += nodeid + ",";
                            }
                            if (xr.Name == "tag")
                            {
                                string k = xr.GetAttribute("k");
                                string v = xr.GetAttribute("v");
                                if (FClass.Contains(k))
                                {
                                    way.setFc(k);
                                    way.setDsg(v);
                                }
                                wayTags += "key=" + k + ",value=" + v + ";";
                            }
                        }
                        wayTags = wayTags.Replace("'", "”");
                        way.setTags(wayTags);
                        if (pointsId.Length > 0)
                        {
                            pointsId = pointsId.Substring(0, pointsId.Length - 1);
                        }
                        else
                        {
                        }
                        way.setPointsld(pointsId);
                        if (refs.Count == 0)
                        {
                            oscwaylist.Add(way);
                        }
                        else
                        {
                            if (refs[0].Equals(refs[refs.Count - 1]) && refs.Count > 3)
                            {
                                oscarealist.Add(way);
                            }
                            else
                            {
                                oscwaylist.Add(way);
                            }
                        }
                        if (oscarealist.Count() > 0)
                        {
                            odh.insertoscWayDataBySql("OSCAREA", oscarealist);
                            oscarealist.Clear();
                        }
                        else if (oscarealist.Count() >200)//20180925修改
                        {
                            odh.insertoscWayDataBySql("OSCAREA", oscarealist);
                            oscarealist.Clear();
                        }
                        if (oscwaylist.Count() > 0)
                        {
                            odh.insertoscWayDataBySql("OSCLINE", oscwaylist);
                            oscwaylist.Clear();
                        }
                        else if (oscwaylist.Count > 200)//20180925修改
                        {
                            odh.insertoscWayDataBySql("OSCLINE", oscwaylist);
                            oscwaylist.Clear();
                        }
                    }
                }
                #endregion
                 #region 增量关系数据入库处理OscDataRelation_TO_OracleDB   by zbl 2018.7.5
                else if (xr.Name == "relation" && xr.NodeType == XmlNodeType.Element)
                {
                    //Cnt += 1;
                    if (xr.IsEmptyElement) { return; }
                    else
                    {
                        if (!xr.HasAttributes)
                        {
                            return;
                        }

                        try
                        {
                            relationNum++;
                            if (relationNum == 1)
                            {
                            }
                            else if (relationNum % 100 == 0)
                            {
                            }
                            else if (relationNum == oscEleCnt["relationCnt"])
                            {
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        relation.setOscid(long.Parse(xr.GetAttribute("id")));
                        relation.setVersion(int.Parse(xr.GetAttribute("version") == null ? "-1" : xr.GetAttribute("version")));
                        relation.setStartTime(xr.GetAttribute("timestamp") == null ? "" : xr.GetAttribute("timestamp"));
                        relation.setChangeset(xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset"));
                        relation.setUserid(int.Parse(xr.GetAttribute("uid") == null ? "-1" : xr.GetAttribute("uid")));
                        relation.setUsername(xr.GetAttribute("user") == null ? "-1" : xr.GetAttribute("user").Replace("'", "”"));
                        relation.setChangeType(changType3);

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
                                    relationlist.Add(relation);
                                    continue;
                                }
                                wayTags += "key=" + k + ",value=" + v + ";";
                                relation.setTags(wayTags);
                            }
                            if (isPolygon == true)
                            {
                                helper.insertoscRelationDataBySql("OSCAREA", relationlist);

                            }
                        }
                    }
                }
            }
            //this.OscPgBar.Value = Cnt * 100 / oscEleCnt["eleCnt"];
                #endregion
        }

        private void 清空增量数据表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //无用的功能按钮，可删除。
        }
        private void 新建增量数据表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            helper = new OracleDBHelper();
            OracleConnection con = helper.getOracleConnection();
            con.Open();
            //if (con.State == ConnectionState.Open)
            //{
            helper.createOscTable(); 
            this.OSCexeStarteTBox.Text = "OSM增量数据表\r\n OSCPOINT\r OSCLINE\r OSCAREA\r\n已创建成功！"; 
            //}
            //else
            //{
            //    this.OSCexeStarteTBox.Text = "新建数据表 操作未能进行 \r\n\n >>>> 请先连接数据库！";
            //}
        }
        private void 删除增量数据表ToolStripMenuItem_Click(object sender, EventArgs e)//BY zbl 20180925修改
        {
            helper = new OracleDBHelper();
            OracleConnection con = helper.getOracleConnection();
            if (helper.IsExistTable("OSCAREA"))//判断当前数据库中是否存在增量线数据表
            {
                //string sql1 = string.Format("delete from USER_SDO_GEOM_METADATA where TABLE_NAME='OSCAREA'");//删除元数据视图中的数据表
                //helper.sqlExecuteUnClose(sql1);
                //sql1 = string.Format("drop index idx_OSCAREA");//删除数据表中的索引
                //helper.sqlExecuteUnClose(sql1);
                string sql = string.Format("drop table OSCAREA");//若数据表存在将其删除
                helper.sqlExecuteUnClose(sql);
                string sql1 = string.Format("drop trigger TG_OSCAREA");
                helper.sqlExecuteUnClose(sql1);
            }
            if (helper.IsExistTable("OSCLINE"))//判断当前数据库中是否存在增量线数据表
            {
                //string sql1 = string.Format("delete from USER_SDO_GEOM_METADATA where TABLE_NAME='OSCLINE'");//删除元数据视图中的数据表
                //helper.sqlExecuteUnClose(sql1);
                //sql1 = string.Format("drop index idx_OSCLINE");//删除数据表中的索引
                //helper.sqlExecuteUnClose(sql1);
                string sql = string.Format("drop table OSCLINE");//若数据表存在将其删除
                helper.sqlExecuteUnClose(sql);
                string sql1 = string.Format("drop trigger TG_OSCLINE");
                helper.sqlExecuteUnClose(sql1);
            }
            if (helper.IsExistTable("OSCPOINT"))//判断当前数据库中是否存在增量线数据表
            {
                //string sql1 = string.Format("delete from USER_SDO_GEOM_METADATA where TABLE_NAME='OSCPOINT'");//删除元数据视图中的数据表
                //helper.sqlExecuteUnClose(sql1);
                //sql1 = string.Format("drop index idx_OSCPOINT");//删除数据表中的索引
                //helper.sqlExecuteUnClose(sql1);
                string sql = string.Format("drop table OSCPOINT");//若数据表存在将其删除
                helper.sqlExecuteUnClose(sql);
                string sql1 = string.Format("drop trigger TG_OSCPOINT");
                helper.sqlExecuteUnClose(sql1);
            }
            this.OSCexeStarteTBox.Text = " 增量数据表\r OSCPOINT\r OSCLINE\r OSCAREA\r\n已删除！\n\n请选择增量数据入库";
     
        }
        private void ExitBtn_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
