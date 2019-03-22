using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using GIS.Geometries;
using GIS.UI.Forms;
using Oracle.ManagedDataAccess.Client;

namespace TrustValueAndReputation.historyToDatabase
{
    public enum NodeLayer
    {
        #region 国标
        //HYDP,
        //HFCP,
        //RESP,
        //RFCP,
        //LFCP,
        //PIPP,
        //BOUP,
        //BRGP,
        //TERP,
        //VEGP
        #endregion
        #region 数字周边方案
        WATP,
        RESP,
        TRAP,
        PIPP,
        BOUP,
        TERP
        #endregion
    }
    public enum WayLayer
    {
        #region 国标
        //HYDL,
        //HFCL,
        //RESL,
        //RFCL,
        //LRRL,
        //LRDL,
        //LFCL,
        //PIPL,
        //BOUL,
        //BRGL,
        //TERL,
        //VEGL
        #endregion
        #region 数字周边方案
        WATL,
        RESL,
        TRAL,
        PIPL,
        BOUL,
        BOUNATL,
        TERL,
        VEGL
        #endregion
    }
    public enum AreaLayer
    {
        #region 国标
        //HYDA,
        //HFCA, 
        //RESA,
        //RFCA,
        //BOUA,
        //BRGA,
        //TERA,
        //VEGA,
        //LFCA,
        //LRDA,
        //LRRA
        #endregion
        #region 数字周边方案
        WATA,
        RESA,
        BOUA,
        BOUNATA,
        TERA,
        VEGA
        #endregion
    }
    public partial class ImportOsc : Form
    {
        public static string osmPointTblName = "osmpoint";
        public static string osmLineTblName = "osmline";
        public static string osmAreaTblName = "osmarea";

        public static string oscPointTblName = "oscpoint";
        public static string oscLineTblName = "oscline";
        public static string oscAreaTblName = "oscarea";

        public static string geonamesTblName = "label_geonames";
        public static string gnsTblName = "label_gns";


        
        public ImportOsc()
        {
            InitializeComponent();
            this.groupBox1.Text = "DB状态：" + con.State.ToString();
            Control.CheckForIllegalCrossThreadCalls = false;
            //server = OSMDataBaseLinkForm.Server0;
            //port = OSMDataBaseLinkForm.Port0;
            //dbname = OSMDataBaseLinkForm.Database0;
            //username = OSMDataBaseLinkForm.User0;
            //password = OSMDataBaseLinkForm.Password0;
            //string temp = String.Format("服务器: {0}\r\n端  口: {1}\r\n用户名: {2}\r\n密  码 ：{3}\r\n数据库: {4}\r\n",
            //server, port, username, "******", dbname);
            //this.showLbl.Text = temp;
            //rule_sql_text = OSMRuleLinkForm.conRuleString;
        }
        public static string[] FClass = { "aerialway","aeroway","amenity","barrier","boundary","building","craft","emergency",
"geological","highway","historic","landuse","leisure","man_made","military",
"natural","office","place","power","public transport","railway","route","shop","sport","tourism","waterway"};
        string[] oscFileNames = { null };
        public static  OracleConnection con = new OracleConnection();
        Dictionary<string, int> oscEleCnt = new Dictionary<string, int>();
        public static string conString = null;
        DateTime startTime = new DateTime();
        DateTime endTime = new DateTime();
        public string rule_sql_text = null;//= "Server=localhost;Port=5432;Database=OSMRule;User id=postgres;password=123;";
        public DateTime osm_end_time = new DateTime();
        public DateTime osc_begin_time = new DateTime();
        public DateTime osc_end_time = new DateTime();
        public bool selectOsc = false;
        public bool renewOsm = false;
        public string server = null;
        public string port = null;
        public string dbname = null;
        public string username = null;
        public string password = null;
        public string countryShape = null;
        private void ImportOsc_Load(object sender, EventArgs e)
        {
        }
        private void open_Click(object sender, EventArgs e)
        {
            #region 检查OSC中数据是不是具备完整性——way里面的node，都在node中找得到
            //string filepath = null;
            //if (this.openFileDlg.ShowDialog() == DialogResult.OK)
            //{
            //    filepath = this.openFileDlg.FileName;
            //}
            //checkOscFile(filepath);
            #endregion
        }
        private void 连接数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            conString = "Server=" + server + ";Port=" + port + ";Database="
            + dbname + ";User id=" + username + ";password=" + password + ";";
            try
            {
                con = new OracleConnection(conString);
                con.Open();
                this.exeStarteTBox.Text = ("数据库连接成功！");
            }
            catch (Exception)
            {
                this.exeStarteTBox.Text = ("数据库连接失败！请检查连接参数！");
                return;
            }
            this.groupBox1.Text = "DB状态：" + con.State.ToString();
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {
        }
        private void label4_Click(object sender, EventArgs e)
        {
        }
        private void 开始入库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmdStr = String.Format("");
            using ( OracleCommand cmd = new OracleCommand())
            {
            }
        }
        private void 创建增量数据表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                this.exeStarteTBox.Text = CreateOscTable(con);
            }
            else
            {
                this.exeStarteTBox.Text = "请确认数据库正确连接！";
            }
        }
        private void 删除增量数据表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                string message = null;
                if (SqlHelper_OSC.DropTable(con, "oscpoint") == -1)
                {
                    message += "已清除 POINT 表格\r\n";
                }
                if (SqlHelper_OSC.DropTable(con, "oscline") == -1)
                {
                    message += "已清除 LINE 表格\r\n";
                }
                if (SqlHelper_OSC.DropTable(con, "oscarea") == -1)
                {
                    message += "已清除 AREA 表格\r\n";
                }
                this.exeStarteTBox.Text = message;
            }
            else
            {
                this.exeStarteTBox.Text = "未连接数据库···";
            }
        }
        private void 开始增量入库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (oscFileNames.Length > 0 && oscFileNames[0] != null && con.State == ConnectionState.Open)
            {
                Thread storeOsc = new Thread(StoreOscFiles);
                storeOsc.Start();
            }
            else
            {
                this.exeStarteTBox.Text = "请确认数据库连接以及输入文件！";
            }
        }
        /// <summary>
        /// OSC文件按输入文件名一个个入库
        /// </summary>
        private void StoreOscFiles()
        {
            for (int i = 0; i < oscFileNames.Length; i++)
            {
                this.exeStarteTBox.Text = "进度:" + (i + 1) + "/" + oscFileNames.Length + "\r\n正在入库：" + Path.GetFileName(oscFileNames[i]);
                if (GetOscEleCnt(this.oscFileNames[i], out oscEleCnt))
                {
                    OscFileToPostSQL(i);
                }
            }
            this.exeStarteTBox.Text = "OSC文件入库完成";
        }
        private void 优化OSC数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                OptimizeOscData();
            }
            else
            {
                this.exeStarteTBox.Text = "请确认数据库连接！";
            }
        }
        /// <summary>
        /// 优化处理Osc数据
        /// </summary>
        private void OptimizeOscData()
        {
            List<uint> repEleIdLst = new List<uint>();
            //处理node
            repEleIdLst = SqlHelper_OSC.GetRepIds(OscNodeTblName, OscNodeTblIdname, con);
            this.exeStarteTBox.Text = "正在处理 Node 元素 待优化数据统计：" + repEleIdLst.Count();
            this.oscPgBar.Maximum = 100;
            if (repEleIdLst.Count > 0)
            {
                for (int i = 0; i < repEleIdLst.Count; i++)
                {
                    this.oscPgBar.Value = (i + 1) * 100 / repEleIdLst.Count;
                    if (OptimizeOscNodeById(repEleIdLst[i]))
                    {//当前ID node数据优化成功
                    }
                    else
                    { }
                }
            }
            //处理way
            this.exeStarteTBox.Text = "正在处理 Way 元素 待优化数据统计：" + repEleIdLst.Count();
            repEleIdLst = SqlHelper_OSC.GetRepIds(OscWayTblName, "wayid", con);
            if (repEleIdLst.Count > 0)
            {
                for (int i = 0; i < repEleIdLst.Count; i++)
                {
                    this.oscPgBar.Value = (i + 1) * 100 / repEleIdLst.Count;
                    if (OptimizeOscWayById(repEleIdLst[i]))
                    {//当前ID node数据优化成功
                    }
                    else
                    { }
                }
            }
            //处理relation
            repEleIdLst = SqlHelper_OSC.GetRepIds(OscRelaTblName, OscRelaTblIdname, con);
            this.exeStarteTBox.Text = "正在处理 Relation 元素 待优化数据统计：" + repEleIdLst.Count();
            if (repEleIdLst.Count > 0)
            {
                for (int i = 0; i < repEleIdLst.Count; i++)
                {
                    this.oscPgBar.Value = (i + 1) * 100 / repEleIdLst.Count;
                    if (OptimizeOscRelationById(repEleIdLst[i]))
                    {//当前ID node数据优化成功
                    }
                    else
                    { }
                }
            }
            this.exeStarteTBox.Text = "Osc数据优化完成";
            this.oscPgBar.Value = 0;
        }
        /// <summary>
        /// 优化数据库中OscRelation数据
        /// </summary>
        /// <param name=OscRelaTblIdname></param>
        /// <returns></returns>
        private bool OptimizeOscRelationById(uint rid)
        {
            try
            {
                List<OsmDataRelation> relationLst = SqlHelper_OSC.GetRelationsByID(rid, con);
                int last = relationLst.Count() - 1;
                if (relationLst[0].changeType == "create" && relationLst[last].changeType == "modify")
                {
                    SqlHelper_OSC.UpdateNodeChangeType(OscRelaTblName, relationLst[last].id, relationLst[last].version, con);
                }
                //三种可能性，不详细叙述。所有情况都要删除除最后一个版本的数据，统一在这一行删除,注意，保留最后一行数据
                for (int j = 0; j < relationLst.Count - 1; j++)
                {
                    SqlHelper_OSC.DeleteEleByIdAndVersion(OscRelaTblName, OscRelaTblIdname, relationLst[j].id, relationLst[j].version, con);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 优化数据库中OscWay数据
        /// </summary>
        /// <param name="wayid"></param>
        /// <returns></returns>
        private bool OptimizeOscWayById(uint wayid)
        {
            try
            {
                List<OsmDataWay> wayLst = SqlHelper_OSC.GetWaysByID(wayid, con);
                int last = wayLst.Count() - 1;
                if (wayLst[0].changeType == "create" && wayLst[last].changeType == "modify")
                {
                    SqlHelper_OSC.UpdateNodeChangeType(OscWayTblName, wayLst[last].id, wayLst[last].version, con);
                }
                //三种可能性，不详细叙述。所有情况都要删除除最后一个版本的数据，统一在这一行删除,注意，保留最后一行数据
                for (int j = 0; j < wayLst.Count - 1; j++)
                {
                    SqlHelper_OSC.DeleteEleByIdAndVersion(OscWayTblName, "wayid", wayLst[j].id, wayLst[j].version, con);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 优化数据库中OSCNode数据
        /// </summary>
        /// <param name=OscNodeTblIdname></param>
        /// <returns></returns>
        private bool OptimizeOscNodeById(uint nodeid)
        {
            try
            {
                List<OsmDataNode> nodeLst = SqlHelper_OSC.GetNodesByID(nodeid, con);
                int last = nodeLst.Count() - 1;
                if (nodeLst[0].changeType == "create" && nodeLst[last].changeType == "modify")
                {
                    SqlHelper_OSC.UpdateNodeChangeType(OscNodeTblName, nodeLst[last].id, nodeLst[last].version, con);
                }
                //三种可能性，不详细叙述。所有情况都要删除除最后一个版本的数据，统一在这一行删除,注意，保留最后一行数据
                for (int j = 0; j < nodeLst.Count - 1; j++)
                {
                    SqlHelper_OSC.DeleteEleByIdAndVersion(OscNodeTblName, OscNodeTblIdname, nodeLst[j].id, nodeLst[j].version, con);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void 更新OSM数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                Thread renewOsm = new Thread(RewOsmWithOsc);
                renewOsm.Start();
            }
            else
            {
                this.exeStarteTBox.Text = "请确认数据库连接！";
            }
        }
        private void RewOsmWithOsc()
        {
            //node
            List<uint> nodeids = SqlHelper_OSC.SelectIdsFromTable(OscNodeTblName, OscNodeTblIdname, con);
            this.exeStarteTBox.Text = "正在更新 Node " + "\r\n待更新数据统计:" + nodeids.Count;
            if (nodeids.Count > 0)
            {
                for (int i = 0; i < nodeids.Count; i++)
                {
                    this.oscPgBar.Value = (i + 1) * 100 / nodeids.Count;
                    RenewNode(nodeids[i]);
                }
            }
            else
            {
            }
            //way
            List<uint> wayids = SqlHelper_OSC.SelectIdsFromTable(OscWayTblName, "wayid", con);
            this.exeStarteTBox.Text = "正在更新 Way " + "\r\n待更新数据统计:" + wayids.Count;
            if (wayids.Count > 0)
            {
                for (int i = 0; i < wayids.Count; i++)
                {
                    this.oscPgBar.Value = (i + 1) * 100 / wayids.Count;
                    RenewWay(wayids[i]);
                }
            }
            else
            {
            }
            //Relation
            List<uint> rids = SqlHelper_OSC.SelectIdsFromTable(OscRelaTblName, OscRelaTblIdname, con);
            this.exeStarteTBox.Text = "正在更新 Relation " + "\r\n待更新数据统计:" + rids.Count;
            if (rids.Count > 0)
            {
                for (int i = 0; i < rids.Count; i++)
                {
                    this.oscPgBar.Value = (i + 1) * 100 / rids.Count;
                    RenewRelation(rids[i]);
                }
            }
            else
            {
            }
            this.exeStarteTBox.Text = "更新完毕!";
            this.oscPgBar.Value = 0;
        }
        private bool RenewRelation(uint rid)
        {
            try
            {
                OsmDataRelation relation = new OsmDataRelation();
                relation.InitialRelationFromNpgsqlReader(SqlHelper_OSC.GetElesByID(OscRelaTblName, OscRelaTblIdname, rid, con));
                bool isPolygon = false;
                List<string> outers = new List<string>();
                List<string> inners = new List<string>();
                if (relation.changeType == "create")
                {
                    RELATION rela = relation.ToRelation(out isPolygon, out outers, out inners);
                    SqlHelper_OSC.InsertRelationIntoOsm(conString, rela, outers, inners);
                }
                else if (relation.changeType == "modify")
                {
                    SqlHelper_OSC.DeleteOsmEleByID("relations", "osmid", rid, con);
                    RELATION rela = relation.ToRelation(out isPolygon, out outers, out inners);
                    SqlHelper_OSC.InsertRelationIntoOsm(conString, rela, outers, inners);
                }
                else if (relation.changeType == "delete")
                {
                    SqlHelper_OSC.DeleteOsmEleByID("relations", "osmid", rid, con);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool RenewWay(uint wayid)
        {
            try
            {
                OsmDataWay way = new OsmDataWay();
                way.InitialWayFromNpgsqlreader(SqlHelper_OSC.GetElesByID(OscWayTblName, "wayid", wayid, con));
                List<string> rels = new List<string>();
                if (way.changeType == "create")
                {
                    WAY osmway = way.ToWay(out rels);
                    SqlHelper_OSC.InsertWayIntoOsm(osmway, conString, rels);
                }
                else if (way.changeType == "modify")
                {
                    SqlHelper_OSC.DeleteOsmEleByID("ways", "osmid", wayid, con);
                    WAY osmway = way.ToWay(out rels);
                    SqlHelper_OSC.InsertWayIntoOsm(osmway, conString, rels);
                }
                else if (way.changeType == "delete")
                {
                    SqlHelper_OSC.DeleteOsmEleByID("ways", "osmid", wayid, con);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool RenewNode(uint nid)
        {
            try
            {
                OsmDataNode node = new OsmDataNode();
                node.InitialNodeFromNpgsqlreader(SqlHelper_OSC.GetElesByID(OscNodeTblName, OscNodeTblIdname, nid, con));
                if (node.changeType == "create")
                {
                    //SqlHelper_OSC.InsertNodeIntoOsm(node.ToNode(), conString);
                }
                else if (node.changeType == "modify")
                {
                    SqlHelper_OSC.DeleteOsmEleByID("nodes", "osmid", nid, con);
                    //SqlHelper_OSC.InsertNodeIntoOsm(node.ToNode(), conString);
                }
                else if (node.changeType == "delete")
                {
                    SqlHelper_OSC.DeleteOsmEleByID("nodes", "osmid", nid, con);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void OscFileToPostSQL(object i)
        {
            int Cnt = 0;
            string changeTpe = null;
            #region node_to_postreSql
            using (XmlTextReader oscNodeReader = new XmlTextReader(this.oscFileNames[int.Parse(i.ToString())]))
            {
                while (oscNodeReader.Read())
                {
                    if (oscNodeReader.Name == "delete" || oscNodeReader.Name == "modify" || oscNodeReader.Name == "create"
                    && oscNodeReader.NodeType == XmlNodeType.Element)
                    {
                        changeTpe = oscNodeReader.Name;
                        while (oscNodeReader.Read())
                        {
                            if (oscNodeReader.Name == "node" && oscNodeReader.NodeType == XmlNodeType.Element)
                            {
                                Cnt += 1;
                                OsmDataNode node = new OsmDataNode();
                                node.InitialNodeFromXml(oscNodeReader);
                                node.changeType = changeTpe;
                                if (node.changeType == "delete") { node.visible = false; }
                                SqlHelper_OSC.InsertOscNode(con, node);
                            }
                            else if (oscNodeReader.Name == changeTpe && oscNodeReader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                        }
                        //一个changeTpe内部数据读完统计一次数据，显示当前状态。
                        this.oscPgBar.Value = Cnt * 100 / oscEleCnt["eleCnt"];
                    }
                }
            }
            #endregion
            #region way_to_postgreSql
            using (XmlTextReader oscWayReader = new XmlTextReader(this.oscFileNames[int.Parse(i.ToString())]))
            {
                string text = null;
                while (oscWayReader.Read())
                {
                    if (oscWayReader.Name == "delete" || oscWayReader.Name == "modify" || oscWayReader.Name == "create"
                    && oscWayReader.NodeType == XmlNodeType.Element)
                    {
                        changeTpe = oscWayReader.Name;
                        while (oscWayReader.Read())
                        {
                            if (oscWayReader.Name == "way" && oscWayReader.NodeType == XmlNodeType.Element)
                            {
                                Cnt += 1;
                                OsmDataWay way = new OsmDataWay();
                                way.InitialWayFromXml(oscWayReader,con);
                                way.changeType = changeTpe;
                                if (way.changeType == "delete") { way.visible = false; }
                                SqlHelper_OSC.InsertOscWay(con, way);
                            }
                            else if (oscWayReader.Name == changeTpe && oscWayReader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                        }
                        //一个changeTpe内部数据读完统计一次数据，显示当前状态。
                        this.oscPgBar.Value = Cnt * 100 / oscEleCnt["eleCnt"];
                    }
                }
            }
            #endregion
            #region relation_to_postgreSql
            using (XmlTextReader oscRelReader = new XmlTextReader(this.oscFileNames[int.Parse(i.ToString())]))
            {
                while (oscRelReader.Read())
                {
                    if (oscRelReader.Name == "delete" || oscRelReader.Name == "modify" || oscRelReader.Name == "create"
                    && oscRelReader.NodeType == XmlNodeType.Element)
                    {
                        changeTpe = oscRelReader.Name;
                        while (oscRelReader.Read())
                        {
                            if (oscRelReader.Name == "relation" && oscRelReader.NodeType == XmlNodeType.Element)
                            {
                                Cnt += 1;
                                OsmDataRelation relation = new OsmDataRelation();
                                relation.InitialRelationFromXml(oscRelReader,con);
                                relation.changeType = changeTpe;
                                if (relation.changeType == "delete") { relation.visible = false; }
                                if (relation.isPolygon)
                                {
                                    int k = SqlHelper_OSC.InsertOscRelation(con, relation, OscRelaTblName);
                                }
                            }
                            else if (oscRelReader.Name == changeTpe && oscRelReader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                        }
                        //一个changeTpe内部数据读完统计一次数据，显示当前状态。
                        this.oscPgBar.Value = Cnt * 100 / oscEleCnt["eleCnt"];
                    }
                }
            }
            #endregion
            this.oscPgBar.Value = 0;
        }
        /// <summary>
        /// 预统计输入文件中node、way、Relation的节点数目
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="oscFileEleCnt"></param>
        /// <returns></returns>
        public static bool GetOscEleCnt(string fileName, out Dictionary<string, int> oscFileEleCnt)
        {
            oscFileEleCnt = new Dictionary<string, int>();
            try
            {
                using (XmlTextReader reader = new XmlTextReader(fileName))
                {
                    int nodeCnt = 0; int wayCnt = 0; int relationCnt = 0;
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
        private static void checkOscFile(string oscfilepath)
        {
            List<uint> nodeLst = new List<uint>();
            using (XmlTextReader xr = new XmlTextReader(oscfilepath))
            {
                while (xr.Read())
                {
                    if (xr.Name == "node" && xr.NodeType == XmlNodeType.Element)
                    {
                        nodeLst.Add(uint.Parse(xr.GetAttribute("id")));
                    }
                }
            }
            using (XmlTextReader xr = new XmlTextReader(oscfilepath))
            {
                while (xr.Read())
                {
                    if (xr.Name == "way" && xr.NodeType == XmlNodeType.Element)
                    {
                        while (xr.Read())
                        {
                            if (xr.Name == "nd")
                            {
                                uint nodeid = uint.Parse(xr.GetAttribute("ref"));
                                bool idexisted = false;
                                for (int i = 0; i < nodeLst.Count; i++)
                                {
                                    if (nodeid == nodeLst[i])
                                    {
                                        idexisted = true;
                                    }
                                }
                                if (!idexisted)
                                {
                                    MessageBox.Show(nodeid + "notexisted");
                                }
                            }
                            if (xr.Name == "way" && xr.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            MessageBox.Show("检查完毕");
        }
        private void 按时间提取增量ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.timeGBox.Visible = true;
        }
        private void 选择OSC文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDlg.ShowDialog() == DialogResult.OK)
            {
                oscFileNames = this.openFileDlg.FileNames;
                this.oscOpenTB.Text = Path.GetDirectoryName(oscFileNames[0]);
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
            this.exeStarteTBox.Text = messageText;
        }
        private void StartBtn_Click(object sender, EventArgs e)
        {

            if (con.State == ConnectionState.Open)
            {
                if (selectOsc == true)
                {
                    //按时间段选取增量
                    startTime = startTimePicker.Value;
                    endTime = endTimePicker.Value;
                    this.exeStarteTBox.Text = "开始时间：" + startTime.ToString() + "\r\n";
                    this.exeStarteTBox.Text += "结束时间：" + endTime.ToString() + "\r\n";
                    if (startTime.CompareTo(endTime) < 0)
                    {
                        Thread getOscData = new Thread(GetOscDataWithTime);
                        getOscData.Start();
                    }
                    else
                    { this.exeStarteTBox.Text += "时间选择出错\r\n"; }
                }
                else if (renewOsm == true)
                {
                    this.exeStarteTBox.Text += "查找符合时间区间的增量\r\n";
                    //首先清除查找痕迹
                    this.exeStarteTBox.Text += SetGbLyrInvalid();
                    this.exeStarteTBox.Text += "查找符合时间区间的增量\r\n";
                    this.exeStarteTBox.Text = SetGbLyrValidWithTime();
                    this.exeStarteTBox.Text += "优化整理增量...\r\n";
                    this.exeStarteTBox.Text = OptimizeGbData(con);
                    this.exeStarteTBox.Text += "将优化后的增量更新到新基态表中...\r\n";
                    this.exeStarteTBox.Text = RenewGbNewTbl();
                    this.exeStarteTBox.ScrollToCaret();
                    selectOsc = false;
                    renewOsm = false;
                }
            }
            else
            {
                this.exeStarteTBox.Text = "请先连接数据库，后执行提取数据操作";
            }
        }
        /// <summary>
        /// 按照时间筛选增量
        /// </summary>
        private void GetOscDataWithTime()
        {
            this.exeStarteTBox.Text += "正在筛选增量数据\r\n";
            this.exeStarteTBox.Text += "清除上次查找痕迹\r\n";
            //首先清除查找痕迹
            this.exeStarteTBox.Text += SetGbLyrInvalid();
            this.exeStarteTBox.Text += "挑选符合时间要求的数据\r\n";
            //然后按照时间查找
            this.exeStarteTBox.Text += SetGbLyrValidWithTime();
            //将重复出现的数据优化
            this.exeStarteTBox.Text += "优化重复出现的数据\r\n";
            this.exeStarteTBox.Text += OptimizeGbData(con);
            this.exeStarteTBox.Text += "增量筛选结束，点击 导出shp文件 生成shape文件\r\n";
            exeStarteTBox.Select(exeStarteTBox.TextLength, 0);
            this.exeStarteTBox.ScrollToCaret();
            selectOsc = false;
        }
        private string RenewGbNewTbl()
        {
            this.exeStarteTBox.Text += "点：\r\n";
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string osc_Tbl = "osc_" + layer.ToString().ToLower();
                string new_Tbl = "new_" + layer.ToString().ToLower();
                string sum = SqlHelper_OSC.FreshNewFromOsc(osc_Tbl, new_Tbl, con,"point");
                this.exeStarteTBox.Text += osc_Tbl + "->" + new_Tbl + ":" + sum + "\r\n";
            }

            this.exeStarteTBox.Text += "线：\r\n";
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string osc_Tbl = "osc_" + layer.ToString().ToLower();
                string new_Tbl = "new_" + layer.ToString().ToLower();
                string sum = SqlHelper_OSC.FreshNewFromOsc(osc_Tbl, new_Tbl, con,"line");
                this.exeStarteTBox.Text += osc_Tbl + "->" + new_Tbl + ":" + sum + "\r\n";
            }

            this.exeStarteTBox.Text += "面：\r\n";
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string osc_Tbl = "osc_" + layer.ToString().ToLower();
                string new_Tbl = "new_" + layer.ToString().ToLower();
                string sum = SqlHelper_OSC.FreshNewFromOsc(osc_Tbl, new_Tbl, con,"area");
                this.exeStarteTBox.Text += osc_Tbl + "->" + new_Tbl + ":" + sum + "\r\n";
            }
            return null;
        }
        /// <summary>
        /// 根据录入的全局变量时间范围将tblname中符合时间要求的数据挑选出来
        /// </summary>
        /// <param name="tblname"></param>
        /// <returns></returns>
        private int SetEleValidWithTime(string tblname)
        {
            using ( OracleDataReader nr = SqlHelper_OSC.GetOscData(tblname, con))
            {
                return SqlHelper_OSC.UpdateTblValidWithTime(tblname, nr, con, startTime, endTime);
            }
        }
        private void 增量转换图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void Osc_toGbTbl(List<string> gb_lyr, string sourceTbl)
        {
            for (int i = 0; i < gb_lyr.Count; i++)
            {
                List<string> osmkey = new List<string>();
                List<string> osmvalue = new List<string>();
                List<string>code= new List<string>();
                SqlHelper_OSC.GetRulesKVByLyrName(rule_sql_text, gb_lyr[i], out osmkey, out osmvalue, out code);
                string subCmdText = null;
                for (int j = 0; j < osmkey.Count - 1; j++)
                {
                    subCmdText += String.Format("fc = '{0}' and dsg = '{1}' and valid = 1 OR ", osmkey[j], osmvalue[j]);
                }
                subCmdText += String.Format("fc = '{0}' and dsg = '{1}' and valid = 1 ", osmkey[osmkey.Count - 1], osmvalue[osmkey.Count - 1]);
                string gbtbl_name = "osc_" + gb_lyr[i].ToLower();
                string cmdtext = String.Format("insert into {0} select * from {1} where {2}", gbtbl_name, sourceTbl, subCmdText);
                try
                {
                    using (OracleCommand cmd = new OracleCommand(cmdtext, con))
                    {
                        int num = cmd.ExecuteNonQuery();
                        this.exeStarteTBox.Text += gbtbl_name + ":" + num + "\r\n";
                    }
                }
                catch
                {
                }
            }
        }
        private void 清空增量数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// 创建OscNode OscWay OscRelation三个表格
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        private string CreateOscTable(OracleConnection con)
        {
            string finishSql = null;
            string unFinishSql = null;
            string path = "OsmSql\\OSC\\";
            if (SqlHelper_OSC.CreateOSCTable(con,path+"point.txt","oscpoint")==-1)
            {
                finishSql += "OSC_POINT ";
            }
            else
            {
                unFinishSql += "OSC_POINT ";
            }
            if (SqlHelper_OSC.CreateOSCTable(con, path + "line.txt", "oscline") == -1)
            {
                finishSql += "OSC_LINE ";
            }
            else
            {
                unFinishSql += "OSC_LINE ";
            }
            if (SqlHelper_OSC.CreateOSCTable(con, path + "area.txt", "oscarea") == -1)
            {
                finishSql += "OSC_AREA ";
            }
            else
            {
                unFinishSql += "OSC_AREA ";
            }
            string outString = String.Format("新建成功：{0}\r\n", finishSql);
            if (unFinishSql != null)
            {
                outString += String.Format("新建失败：{0}\r\n", unFinishSql);
            }
            return outString;
        }
        /// <summary>
        /// 创建OSC_GB数据表
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        private int CreateOscGbTbls(OracleConnection con)
        {
            int num = 0;
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string path = "OsmSql\\OSC\\point.txt";
                string tblname = "osc_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.CreateOSCTable(con, path, tblname) == -1)
                {
                    num += 1;
                }
            }
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string path = "OsmSql\\OSC\\line.txt";
                string tblname = "osc_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.CreateOSCTable(con, path, tblname) == -1)
                {
                    num += 1;
                }
            }
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string path = "OsmSql\\OSC\\area.txt";
                string tblname = "osc_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.CreateOSCTable(con, path, tblname) == -1)
                {
                    num += 1;
                }
            }
            return num;
        }
        private int DropOscGbTbls(OracleConnection con)
        {
            int num = 0;
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string tblname = "osc_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.DropTable(con, tblname) == -1)
                { num += 1; }
            }
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string tblname = "osc_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.DropTable(con, tblname) == -1)
                { num += 1; }
            }
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string tblname = "osc_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.DropTable(con, tblname) == -1)
                { num += 1; }
            }
            return num;
        }
        private void 增量处理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void 国标图层建表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                this.exeStarteTBox.Text = "正在尝试新建...";
                int num = CreateOscGbTbls(con);
                this.exeStarteTBox.Text = "新建数据表:" + num + "个";
            }
            else { this.exeStarteTBox.Text = "数据库未连接"; }
        }
        private void 国标图层删表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                this.exeStarteTBox.Text = "正在努力删除...";
                int num = DropOscGbTbls(con);
                this.exeStarteTBox.Text = "删除数据表:" + num + "个";
            }
            else { this.exeStarteTBox.Text = "数据库未连接"; }
        }
        //private void 引入数据ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (OSMRuleLinkForm.RuleLinkSuccess)
        //    {
        //        rule_sql_text = OSMRuleLinkForm.conRuleString;
        //        this.exeStarteTBox.Text = null;
        //        Thread trans = new Thread(OscTransfer);
        //        trans.Start();
        //    }
        //    else
        //    {
        //        MessageBox.Show("规则数据库连接问题");
        //    }
        //}
        private void OscTransfer()
        {
            List<string> node_lyr = new List<string>();
            List<string> line_lyr = new List<string>();
            List<string> area_lyr = new List<string>();
            this.exeStarteTBox.Text += "引入点数据开始>>\r\n";
            foreach (NodeLayer lyer in Enum.GetValues(typeof(NodeLayer)))
            {
                node_lyr.Add(lyer.ToString());
            }
            Osc_toGbTbl(node_lyr, "oscnode");
            this.exeStarteTBox.Text += "结束\r\n引入线数据开始>>\r\n";
            foreach (WayLayer lyer in Enum.GetValues(typeof(WayLayer)))
            {
                line_lyr.Add(lyer.ToString());
            }
            Osc_toGbTbl(line_lyr, "oscway");
            this.exeStarteTBox.Text += "结束\r\n引入面数据开始>>\r\n";
            foreach (AreaLayer lyer in Enum.GetValues(typeof(AreaLayer)))
            {
                area_lyr.Add(lyer.ToString());
            }
            Osc_toGbTbl(area_lyr, "oscrelation");
            this.exeStarteTBox.Text += "结束\r\n";
            exeStarteTBox.Select(exeStarteTBox.TextLength, 0);
            this.exeStarteTBox.ScrollToCaret();
        }
        private void 基态建表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string sum = null;
            if (con.State == ConnectionState.Open)
            {
                if (SqlHelper_OSC.CreateTable(con, "Osm\\CreateTableOsmNode"))
                { sum += "node-create "; }
                if (SqlHelper_OSC.CreateTable(con, "Osm\\CreateTableOsmWay"))
                { sum += "way-create "; }
                if (SqlHelper_OSC.CreateTable(con, "Osm\\CreateTableOsmRelation"))
                { sum += "relation-create "; }
                this.exeStarteTBox.Text = sum;
            }
            else
            {
                this.exeStarteTBox.Text = "数据库未连接";
            }
        }
        private void 基态删表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                //string sum = null;
                //if (SqlHelper_OSC.HandleTable(con, "Osm\\DropTableOsmNode"))
                //{ sum += "node-del"; }
                //if (SqlHelper_OSC.HandleTable(con, "Osm\\DropTableOsmWay"))
                //{sum += " way-del";}
                //if (SqlHelper_OSC.HandleTable(con, "Osm\\DropTableOsmRelation"))
                //{ sum += " relation-del"; }
                //this.exeStarteTBox.Text = sum;
                this.exeStarteTBox.Text = "不是不能删，只是怕误删，数据入库很耗时的，真要删除请手动删除。";
            }
            else
            {
                this.exeStarteTBox.Text = "数据库未连接";
            }
        }
        private void 导出ShapeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                folderBrowserDlg.Description = "程序将在选择目录下面分三个文件夹存放\r\n<新建> <修改> <删除> 三类数据";
                if (folderBrowserDlg.ShowDialog() == DialogResult.OK)
                {
                    string path = folderBrowserDlg.SelectedPath;
                    this.oscOpenTB.Text = path;
                    GenerateGbBatFile(path);
                    Process pros = new Process();
                    pros.StartInfo.FileName = "osc_shape.bat";
                    pros.Start();
                    pros.StartInfo.CreateNoWindow = true;
                    pros.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    pros.WaitForExit();
                    pros.Close();
                    File.Delete("osc_shape.bat");
                    this.exeStarteTBox.Text = "转换结束";
                    System.Diagnostics.Process.Start(path);
                }
                else { }
            }
            else
            {
                this.exeStarteTBox.Text = "未连接数据库...";
            }
        }
        private string GenerateGbBatFile(string path)
        {
            string root = path.Split(':')[0];
            string subpath = path.Substring(3);
            string batText = null;
            string createPath = subpath + "\\CREATE";
            string modifyPath = subpath + "\\MODIFY";
            string deletePath = subpath + "\\DELETE";
            #region CREATE
            batText += "cd\\\r\n";
            batText += root + ":\r\n";
            batText += String.Format("if exist {0} (echo 文件夹存在) else (md {1})\r\n", createPath, createPath);
            batText += String.Format("cd {0}\r\n", createPath);
            batText += "set pgclientencoding=utf8\r\n";
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string lyrname = "osc_" + layer.ToString().ToLower();
                batText += String.Format("pgsql2shp -f {0} -h {1} -u {2} -p {3} -P {4} {5} \"SELECT * FROM {6} where valid = 1 and changetype = 'create'or changetype = 'c_modify';\"\r\n",
                "c" + lyrname, server, username, port, password, dbname, lyrname);
            }
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string lyrname = "osc_" + layer.ToString().ToLower();
                batText += String.Format("pgsql2shp -f {0} -h {1} -u {2} -p {3} -P {4} {5} \"SELECT * FROM {6} where valid = 1 and changetype = 'create'or changetype = 'c_modify';\"\r\n",
                "c" + lyrname, server, username, port, password, dbname, lyrname);
            }
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string lyrname = "osc_" + layer.ToString().ToLower();
                batText += String.Format("pgsql2shp -f {0} -h {1} -u {2} -p {3} -P {4} {5} \"SELECT * FROM {6} where valid = 1 and changetype = 'create'or changetype = 'c_modify';\"\r\n",
                "c" + lyrname, server, username, port, password, dbname, lyrname);
            }
            #endregion
            #region MODIFY
            batText += "cd\\\r\n";
            batText += root + ":\r\n";
            batText += String.Format("if exist {0} (echo 文件夹存在) else (md {1})\r\n", modifyPath, modifyPath);
            batText += String.Format("cd {0}\r\n", modifyPath);
            batText += "set pgclientencoding=utf8\r\n";
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string lyrname = "osc_" + layer.ToString().ToLower();
                batText += String.Format("pgsql2shp -f {0} -h {1} -u {2} -p {3} -P {4} {5} \"SELECT * FROM {6} where valid = 1 and changetype = 'modify';\"\r\n",
                "m" + lyrname, server, username, port, password, dbname, lyrname);
            }
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string lyrname = "osc_" + layer.ToString().ToLower();
                batText += String.Format("pgsql2shp -f {0} -h {1} -u {2} -p {3} -P {4} {5} \"SELECT * FROM {6} where valid = 1 and changetype = 'modify';\"\r\n",
                "m" + lyrname, server, username, port, password, dbname, lyrname);
            }
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string lyrname = "osc_" + layer.ToString().ToLower();
                batText += String.Format("pgsql2shp -f {0} -h {1} -u {2} -p {3} -P {4} {5} \"SELECT * FROM {6} where valid = 1 and changetype = 'modify';\"\r\n",
                "m" + lyrname, server, username, port, password, dbname, lyrname);
            }
            #endregion
            #region DELETE
            batText += "cd\\\r\n";
            batText += root + ":\r\n";
            batText += String.Format("if exist {0} (echo 文件夹存在) else (md {1})\r\n", deletePath, deletePath);
            batText += String.Format("cd {0}\r\n", deletePath);
            batText += "set pgclientencoding=utf8\r\n";
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string lyrname = "osc_" + layer.ToString().ToLower();
                batText += String.Format("pgsql2shp -f {0} -h {1} -u {2} -p {3} -P {4} {5} \"SELECT * FROM {6} where valid = 1 changetype = 'delete';\"\r\n",
                "d" + lyrname, server, username, port, password, dbname, lyrname);
            }
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string lyrname = "osc_" + layer.ToString().ToLower();
                batText += String.Format("pgsql2shp -f {0} -h {1} -u {2} -p {3} -P {4} {5} \"SELECT * FROM {6} where valid = 1 and changetype = 'delete';\"\r\n",
                "d" + lyrname, server, username, port, password, dbname, lyrname);
            }
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string lyrname = "osc_" + layer.ToString().ToLower();
                batText += String.Format("pgsql2shp -f {0} -h {1} -u {2} -p {3} -P {4} {5} \"SELECT * FROM {6} where valid = 1 and changetype = 'delete';\"\r\n",
                "d" + lyrname, server, username, port, password, dbname, lyrname);
            }
            #endregion
            FileStream fs = new FileStream(Application.StartupPath + "\\osc_shape.bat", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("GB2312"));
            sw.Write(batText);
            sw.Close();
            fs.Close();
            return batText;
        }
        private void 清除查找痕迹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                string sum = null;
                sum += "node:" + SqlHelper_OSC.SetTblInvalid(con, "oscnode") + "\r\n";
                sum += "way :" + SqlHelper_OSC.SetTblInvalid(con, "oscway") + "\r\n";
                sum += "rela:" + SqlHelper_OSC.SetTblInvalid(con, "oscrelation") + "\r\n";
                this.exeStarteTBox.Text = sum;
            }
            else
            {
                this.exeStarteTBox.Text = "未连接数据库..";
            }
        }
        private void 清除查找痕迹ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                string sum = SetGbLyrInvalid();
                this.exeStarteTBox.Text = sum;
            }
            else
            {
                this.exeStarteTBox.Text = "未连接数据库..";
            }
        }
        private string SetGbLyrInvalid()
        {
            //将国标图层数据表置零
            string sum = null;
            sum += "<点层>\r\n";
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string tbl_name = "osc_" + layer.ToString().ToLower();
                sum += tbl_name + " : " + SqlHelper_OSC.SetTblInvalid(con, tbl_name) + "\r\n";
            }
            sum += "<线层>\r\n";
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string tbl_name = "osc_" + layer.ToString().ToLower();
                sum += tbl_name + " : " + SqlHelper_OSC.SetTblInvalid(con, tbl_name) + "\r\n";
            }
            sum += "<面层>\r\n";
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string tbl_name = "osc_" + layer.ToString().ToLower();
                sum += tbl_name + " : " + SqlHelper_OSC.SetTblInvalid(con, tbl_name) + "\r\n";
            }
            return sum;
        }
        /// <summary>
        /// 根据时间将各个图层的数据筛选出来，设置valid=1
        /// </summary>
        /// <returns></returns>
        private string SetGbLyrValidWithTime()
        {
            int cnt = 3;
            int _cnt = 0;
            string sum = null;
            sum += "<点层>\r\n";
            _cnt += 1;
            this.oscPgBar.Value = _cnt * 100 / cnt;
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string tbl_name = "osc_" + layer.ToString().ToLower();
                sum += tbl_name + " : " + SetEleValidWithTime(tbl_name) + "\r\n";
            }
            sum += "<线层>\r\n";
            _cnt += 1;
            this.oscPgBar.Value = _cnt * 100 / cnt;
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string tbl_name = "osc_" + layer.ToString().ToLower();
                sum += tbl_name + " : " + SetEleValidWithTime(tbl_name) + "\r\n";
            }
            sum += "<面层>\r\n";
            _cnt += 1;
            this.oscPgBar.Value = _cnt * 100 / cnt;
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string tbl_name = "osc_" + layer.ToString().ToLower();
                sum += tbl_name + " : " + SetEleValidWithTime(tbl_name) + "\r\n";
            }
            return sum;
            this.oscPgBar.Value = 0;
        }
        private string OptimizeGbData(OracleConnection con)
        {
            int cnt = 3;
            int _cnt = 0;
            string sum = null;
            sum += "<点层>\r\n";
            _cnt += 1;

            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string tbl_name = "osc_" + layer.ToString().ToLower();
                sum += tbl_name + " : " + SqlHelper_OSC.OptimizeTbl(tbl_name, con) + "\r\n";
            }
            sum += "<线层>\r\n";
            _cnt += 1;

            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string tbl_name = "osc_" + layer.ToString().ToLower();
                sum += tbl_name + " : " + SqlHelper_OSC.OptimizeTbl(tbl_name, con) + "\r\n";
            }
            sum += "<面层>\r\n";
            _cnt += 1;

            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string tbl_name = "osc_" + layer.ToString().ToLower();
                sum += tbl_name + " : " + SqlHelper_OSC.OptimizeTbl(tbl_name, con) + "\r\n";
            }

            return sum;
        }
        private void 按时间查找增量ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                //osm_end_time = SqlHelper_OSC.GetMaxTime("nodes", con).Date;
                osc_begin_time = GetGbMinTime("osc_", con);
                osc_end_time = GetGbMaxTime("osc_", con);
                if (osc_begin_time < osc_end_time)
                {
                    this.exeStarteTBox.Text = "增量开始时间： " + osc_begin_time.ToString() + "\r\n";
                    this.exeStarteTBox.Text += "增量结束时间： " + osc_end_time.ToString() + "\r\n\r\n";
                    this.timeGBox.Visible = true;
                    selectOsc = true;
                    this.startTimePicker.MaxDate = osc_end_time;
                    this.startTimePicker.MinDate = osc_begin_time;
                    this.startTimePicker.Value = osc_begin_time;
                    this.endTimePicker.MinDate = osc_begin_time;
                    this.endTimePicker.MaxDate = osc_end_time;
                    this.exeStarteTBox.Text += "请在选好时间后单击确定";
                }
                else
                { this.exeStarteTBox.Text = "osc数据表出问题了"; }
            }
            else
            {
                this.exeStarteTBox.Text = "未连接数据库";
            }
        }
        public string OscNodeTblName { get; set; }
        public string OscNodeTblIdname { get; set; }
        public string OscWayTblName { get; set; }
        public string OscRelaTblName { get; set; }
        public string OscRelaTblIdname { get; set; }
        private void 新建GB基态数据表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                this.exeStarteTBox.Text = "正在尝试新建...";
                int num = CreateOsmGbTbls(con);
                this.exeStarteTBox.Text = "新建数据表:" + num + "个";
            }
            else { this.exeStarteTBox.Text = "数据库未连接"; }
        }
        private int CreateOsmGbTbls(OracleConnection con)
        {
            int num = 0;
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string path = "OsmSql\\create_gbosm_new\\point.txt";
                string tblname = "new_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.CreateOSCTable(con, path, tblname) == -1)
                {
                    num += 1;
                }
            }
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string path = "OsmSql\\create_gbosm_new\\line.txt";
                string tblname = "new_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.CreateOSCTable(con, path, tblname) == -1)
                {
                    num += 1;
                }
            }
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string path = "OsmSql\\create_gbosm_new\\area.txt";
                string tblname = "new_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.CreateOSCTable(con, path, tblname) == -1)
                {
                    num += 1;
                }
            }
            return num;
        }
        private void 删除GB基态数据表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                this.exeStarteTBox.Text = "正在努力删除...";
                int num = DropOsmGbTbls(con);
                this.exeStarteTBox.Text = "删除数据表:" + num + "个";
            }
            else { this.exeStarteTBox.Text = "数据库未连接"; }
        }
        private int DropOsmGbTbls(OracleConnection con)
        {
            int num = 0;
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string tblname = "new_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.DropTable(con, tblname) == -1)
                { num += 1; }
            }
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string tblname = "new_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.DropTable(con, tblname) == -1)
                { num += 1; }
            }
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string tblname = "new_" + layer.ToString().ToLower();
                if (SqlHelper_OSC.DropTable(con, tblname) == -1)
                { num += 1; }
            }
            return num;
        }
        private void 引入当前GB基态数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                Thread copydata = new Thread(CopyGbData);
                copydata.Start();
            }
            else
            {
                this.exeStarteTBox.Text = "数据库未连接···";
            }
        }
        /// <summary>
        /// 从当前模型转换后的国标数据表中复制数据到更新后数据表，以用于数据表更新
        /// </summary>
        /// <param name="con"></param>
        private void CopyGbData()
        {
            this.exeStarteTBox.Text = null;
            int ndcnt = 10;
            int lineCnt = 12;
            int areaCnt = 8;
            int thisLyrCnt = 0;
            this.exeStarteTBox.Text += "点：\r\n";
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                thisLyrCnt += 1;
                this.oscPgBar.Value = thisLyrCnt * 100 / ndcnt;
                string sourceTbl = layer.ToString().ToLower();
                string targetTbl = "new_" + sourceTbl;
                int cnt = SqlHelper_OSC.CopyDataBetween(targetTbl, sourceTbl, con);
                this.exeStarteTBox.Text += sourceTbl + " --> " + targetTbl + " : " + cnt.ToString() + "\r\n";
            }
            thisLyrCnt = 0;
            this.exeStarteTBox.Text += "\r\n线：\r\n";
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                thisLyrCnt += 1;
                this.oscPgBar.Value = thisLyrCnt * 100 / lineCnt;
                string sourceTbl = layer.ToString().ToLower();
                string targetTbl = "new_" + sourceTbl;
                int cnt = SqlHelper_OSC.CopyDataBetween(targetTbl, sourceTbl, con);
                this.exeStarteTBox.Text += sourceTbl + " --> " + targetTbl + " : " + cnt.ToString() + "\r\n";
            }
            thisLyrCnt = 0;
            this.exeStarteTBox.Text += "\r\n面：\r\n";
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                thisLyrCnt += 1;
                this.oscPgBar.Value = thisLyrCnt * 100 / areaCnt;
                string sourceTbl = layer.ToString().ToLower();
                string targetTbl = "new_" + sourceTbl;
                int cnt = SqlHelper_OSC.CopyDataBetween(targetTbl, sourceTbl, con);
                this.exeStarteTBox.Text += sourceTbl + " --> " + targetTbl + " : " + cnt.ToString() + "\r\n";
            }
            this.oscPgBar.Value = 0;
        }
        private void 更新当前GB数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                startTime = GetGbMaxTime("new_", con);
                endTime = GetGbMaxTime("osc_", con);
                renewOsm = true;
                this.exeStarteTBox.Text = "基态数据结束时间：" + startTime.ToString() + "\r\n"
                + "增量数据结束时间：" + endTime.ToString() + "\r\n" + "点击确定，开始更新";
            }
            else
            {
                this.exeStarteTBox.Text = "数据库未连接···";
            }
        }
        /// <summary>
        /// 查询更新后国标图层最大时间
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        private DateTime GetGbMaxTime(string pre, OracleConnection con)
        {
            DateTime maxTime = new DateTime();
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string tblname = pre + layer.ToString().ToLower();
                DateTime timeTemp = new DateTime();
                if (SqlHelper_OSC.GetMaxTime(tblname, con, ref timeTemp))
                {
                    if (timeTemp.CompareTo(maxTime) > 0)
                    {
                        maxTime = timeTemp;
                    }
                }
            }
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string tblname = pre + layer.ToString().ToLower();
                DateTime timeTemp = new DateTime();
                if (SqlHelper_OSC.GetMaxTime(tblname, con, ref timeTemp))
                {
                    if (timeTemp.CompareTo(maxTime) > 0)
                    {
                        maxTime = timeTemp;
                    }
                }
            }
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string tblname = pre + layer.ToString().ToLower();
                DateTime timeTemp = new DateTime();
                if (SqlHelper_OSC.GetMaxTime(tblname, con, ref timeTemp))
                {
                    if (timeTemp.CompareTo(maxTime) > 0)
                    {
                        maxTime = timeTemp;
                    }
                }
            }
            return maxTime;
        }
        private DateTime GetGbMinTime(string pre, OracleConnection con)
        {
            DateTime minTime = DateTime.Now;
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string tblname = pre + layer.ToString().ToLower();
                DateTime timeTemp = new DateTime();
                if (SqlHelper_OSC.GetMinTime(tblname, con, ref timeTemp))
                {
                    if (timeTemp.CompareTo(minTime) < 0)
                    {
                        minTime = timeTemp;
                    }
                }
            }
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string tblname = pre + layer.ToString().ToLower();
                DateTime timeTemp = new DateTime();
                if (SqlHelper_OSC.GetMinTime(tblname, con, ref timeTemp))
                {
                    if (timeTemp.CompareTo(minTime) < 0)
                    {
                        minTime = timeTemp;
                    }
                }
            }
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string tblname = pre + layer.ToString().ToLower();
                DateTime timeTemp = new DateTime();
                if (SqlHelper_OSC.GetMinTime(tblname, con, ref timeTemp))
                {
                    if (timeTemp.CompareTo(minTime) < 0)
                    {
                        minTime = timeTemp;
                    }
                }
            }
            return minTime;
        }
        private void 入库测试ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmdtext = null;
            StreamReader myRead = new StreamReader("laos.sql");
            while (!myRead.EndOfStream)
            {
                cmdtext += myRead.ReadLine();
            }
            myRead.Close();
            myRead.Dispose();
            using (OracleCommand cmd = new OracleCommand(cmdtext, con))
            {
                int i = cmd.ExecuteNonQuery();
                MessageBox.Show(i + "完成");
            }
        }
        private void 录入文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDlg.ShowDialog() == DialogResult.OK)
            {
                oscFileNames = this.openFileDlg.FileNames;
                this.oscOpenTB.Text = Path.GetDirectoryName(oscFileNames[0]);
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
            this.exeStarteTBox.Text = messageText;
        }
        private void 开始筛选入库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (oscFileNames.Length > 0 && oscFileNames[0] != null && con.State == ConnectionState.Open && countryShape != null)
            {
                Thread storeOsc = new Thread(SelectOscEle);
                storeOsc.Start();
            }
            else
            {
                this.exeStarteTBox.Text = "请确认数据库连接以及输入文件、国家边界！";
            }
        }
        /// <summary>
        /// 全球增量中筛选限定范围OSC数据
        /// </summary>
        private void SelectOscEle()
        {
            string time_b = "开始时间：" + DateTime.Now.ToString() + "\r\n";
            for (int i = 0; i < oscFileNames.Length; i++)
            {
                this.exeStarteTBox.Text = time_b + "进度:" + (i + 1) + "/" + oscFileNames.Length + "\r\n正在筛选：" + Path.GetFileName(oscFileNames[i]);
                if (GetOscEleCnt(this.oscFileNames[i], out oscEleCnt))
                {
                    SelectOscEleToPostSQL(i);
                }
            }
            int nodeCnt = SqlHelper_OSC.GetValidCnt("oscnode", con);
            int wayCnt = SqlHelper_OSC.GetValidCnt("oscway", con);
            int relaCnt = SqlHelper_OSC.GetValidCnt("oscrelation", con);
            string cntText = "[点]:" + nodeCnt + "\r\n" + "[线]:" + wayCnt + "\r\n" + "[面]:" + relaCnt + "\r\n";
            string time_e = "结束时间：" + DateTime.Now.ToString() + "\r\n";
            this.exeStarteTBox.Text = "<--OSC文件筛选完成-->\r\n\r\n" + time_b + time_e + "\r\n入库统计：\r\n" + cntText;
        }
        /// <summary>
        /// 开始筛选单个文件
        /// </summary>
        /// <param name="i"></param>
        private void SelectOscEleToPostSQL(int i)
        {
            int Cnt = 0;
            string changeTpe = null;
            #region 筛选 node to_postreSql
            using (XmlTextReader oscNodeReader = new XmlTextReader(this.oscFileNames[int.Parse(i.ToString())]))
            {
                while (oscNodeReader.Read())
                {
                    if (oscNodeReader.Name == "delete" || oscNodeReader.Name == "modify" || oscNodeReader.Name == "create"
                    && oscNodeReader.NodeType == XmlNodeType.Element)
                    {
                        changeTpe = oscNodeReader.Name;
                        while (oscNodeReader.Read())
                        {
                            if (oscNodeReader.Name == "node" && oscNodeReader.NodeType == XmlNodeType.Element)
                            {
                                Cnt += 1;
                                OsmDataNode node = new OsmDataNode();
                                node.InitialNodeFromXml(oscNodeReader);
                                node.changeType = changeTpe;
                                if (node.changeType == "delete") { node.visible = false; }
                                //在此做条件筛选
                                if (SqlHelper_OSC.IsNodeInShape(node, countryShape, con))
                                {
                                    SqlHelper_OSC.InsertOscNode(con, node);
                                }
                            }
                            else if (oscNodeReader.Name == changeTpe && oscNodeReader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                        }
                        //一个changeTpe内部数据读完统计一次数据，显示当前状态。
                        this.oscPgBar.Value = Cnt * 100 / oscEleCnt["nodeCnt"];
                        this.exePgLbl.Text = "点：" + Cnt + "/" + oscEleCnt["nodeCnt"];
                    }
                }
            }
            #endregion
            #region 筛选 way_to_postgreSql
            using (XmlTextReader oscWayReader = new XmlTextReader(this.oscFileNames[int.Parse(i.ToString())]))
            {
                string text = null;
                Cnt = 0;
                while (oscWayReader.Read())
                {
                    if (oscWayReader.Name == "delete" || oscWayReader.Name == "modify" || oscWayReader.Name == "create"
                    && oscWayReader.NodeType == XmlNodeType.Element)
                    {
                        changeTpe = oscWayReader.Name;
                        while (oscWayReader.Read())
                        {
                            if (oscWayReader.Name == "way" && oscWayReader.NodeType == XmlNodeType.Element)
                            {
                                Cnt += 1;
                                OsmDataWay way = new OsmDataWay();
                                way.InitialWayFromXml(oscWayReader,con);
                                way.changeType = changeTpe;
                                if (way.changeType == "delete") { way.visible = false; }
                                if (SqlHelper_OSC.IsWayInShape(way, countryShape, con))
                                {
                                    SqlHelper_OSC.InsertOscWay(con, way);
                                }
                            }
                            else if (oscWayReader.Name == changeTpe && oscWayReader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                        }
                        //一个changeTpe内部数据读完统计一次数据，显示当前状态。
                        this.oscPgBar.Value = Cnt * 100 / oscEleCnt["wayCnt"];
                        this.exePgLbl.Text = "线：" + Cnt + "/" + oscEleCnt["wayCnt"];
                    }
                }
            }
            #endregion
            #region 筛选 relation_to_postgreSql
            using (XmlTextReader oscRelReader = new XmlTextReader(this.oscFileNames[int.Parse(i.ToString())]))
            {
                Cnt = 0;
                while (oscRelReader.Read())
                {
                    if (oscRelReader.Name == "delete" || oscRelReader.Name == "modify" || oscRelReader.Name == "create"
                    && oscRelReader.NodeType == XmlNodeType.Element)
                    {
                        changeTpe = oscRelReader.Name;
                        while (oscRelReader.Read())
                        {
                            if (oscRelReader.Name == "relation" && oscRelReader.NodeType == XmlNodeType.Element)
                            {
                                Cnt += 1;
                                OsmDataRelation relation = new OsmDataRelation();
                                relation.InitialRelationFromXml(oscRelReader,con);
                                relation.changeType = changeTpe;
                                if (relation.changeType == "delete") { relation.visible = false; }
                                if (relation.isPolygon)
                                {
                                    if (SqlHelper_OSC.IsRelationInShape(relation, countryShape, con))
                                    {
                                        int k = SqlHelper_OSC.InsertOscRelation(con, relation, OscRelaTblName);
                                    }
                                }
                            }
                            else if (oscRelReader.Name == changeTpe && oscRelReader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                        }
                        //一个changeTpe内部数据读完统计一次数据，显示当前状态。
                        this.oscPgBar.Value = Cnt * 100 / oscEleCnt["relationCnt"];
                        this.exePgLbl.Text = "面：" + Cnt + "/" + oscEleCnt["relationCnt"];
                    }
                }
            }
            #endregion
            this.oscPgBar.Value = 0;
            this.exePgLbl.Text = "操作进度";
        }

        private void 导入国家边界ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string DBname = "worldnations";
            string tblname = "Laos";
            string borderConStr = "Server=" + server + ";Port=" + port + ";Database="
            + DBname + ";User id=" + username + ";password=" + password + ";";
            string cmdText = String.Format("select asText(the_geom) from {0}", tblname);
            try
            {
                using (OracleConnection con_ = new OracleConnection(borderConStr))
                using (OracleCommand cmd = new OracleCommand(cmdText, con_))
                {
                    con_.Open();
                    using (OracleDataReader nr = cmd.ExecuteReader())
                    {
                        nr.Read();
                        this.countryShape = nr.GetValue(0).ToString();
                        this.exeStarteTBox.Text = "提取shqpe成功！\r\n" + countryShape;
                    }
                    con_.Close();
                }
            }
            catch { }
        }

        private void 数据统计ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dictionary<string, int> totalCnt = new System.Collections.Generic.Dictionary<string, int>();
            int t_create = 0;
            int t_modify = 0;
            int t_delete = 0;
            int n_c = 0;
            int n_m = 0;
            int n_d = 0;
            int w_c = 0;
            int w_m = 0;
            int w_d = 0;
            int r_c = 0;
            int r_m = 0;
            int r_d = 0;
            foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
            {
                string tblname = "osc_" + layer.ToString().ToLower();
                Dictionary<string, int> thisCnt = SqlHelper_OSC.GetChangeType(tblname, con);
                totalCnt.Add(tblname + "-create", thisCnt["create"]);
                totalCnt.Add(tblname + "-modify", thisCnt["modify"]);
                totalCnt.Add(tblname + "-delete", thisCnt["delete"]);
                n_c += thisCnt["create"];
                n_m += thisCnt["modify"];
                n_d += thisCnt["delete"];
            }
            foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
            {
                string tblname = "osc_" + layer.ToString().ToLower();
                Dictionary<string, int> thisCnt = SqlHelper_OSC.GetChangeType(tblname, con);
                totalCnt.Add(tblname + "-create", thisCnt["create"]);
                totalCnt.Add(tblname + "-modify", thisCnt["modify"]);
                totalCnt.Add(tblname + "-delete", thisCnt["delete"]);
                w_c += thisCnt["create"];
                w_m += thisCnt["modify"];
                w_d += thisCnt["delete"];
            }
            foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
            {
                string tblname = "osc_" + layer.ToString().ToLower();
                Dictionary<string, int> thisCnt = SqlHelper_OSC.GetChangeType(tblname, con);
                totalCnt.Add(tblname + "-create", thisCnt["create"]);
                totalCnt.Add(tblname + "-modify", thisCnt["modify"]);
                totalCnt.Add(tblname + "-delete", thisCnt["delete"]);
                r_c += thisCnt["create"];
                r_m += thisCnt["modify"];
                r_d += thisCnt["delete"];
            }
            totalCnt.Add("total-create", t_create);
            totalCnt.Add("total-modify", t_modify);
            totalCnt.Add("total-delete", t_delete);
            string message = null;
            foreach (var key in totalCnt.Keys)
            {
                message += key + " : " + totalCnt[key] + "\r\n";
            }
        }

        private void 配置文件转换ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDlg.ShowDialog() == DialogResult.OK)
            {
                string txtPath = openFileDlg.FileName;
                GenerateConfig("c", txtPath);
                GenerateConfig("m", txtPath);
                GenerateConfig("d", txtPath);
                MessageBox.Show("配置文件生成！");
            }
        }

        private static void GenerateConfig(string pre, string txtPath)
        {
            using (StreamReader sr = new StreamReader(txtPath, Encoding.Default))
            {
                string st = sr.ReadToEnd();
                foreach (NodeLayer layer in Enum.GetValues(typeof(NodeLayer)))
                {
                    string lyr = layer.ToString();
                    st = st.Replace(lyr, pre + "osc_" + lyr.ToLower());
                }
                foreach (WayLayer layer in Enum.GetValues(typeof(WayLayer)))
                {
                    string lyr = layer.ToString();
                    st = st.Replace(lyr, pre + "osc_" + lyr.ToLower());
                }
                foreach (AreaLayer layer in Enum.GetValues(typeof(AreaLayer)))
                {
                    string lyr = layer.ToString();
                    st = st.Replace(lyr, pre + "osc_" + lyr.ToLower());
                }
                string savepath = Path.GetDirectoryName(txtPath) + "\\" + pre + ".txt";
                using (StreamWriter wr = new StreamWriter(savepath))
                {
                    wr.Write(st);
                }
            }
        }
        ///// <summary>
        ///// 快捷连接设置操作，调用存储在文本里的数据库参数信息，连接数据库
        ///// </summary>
        ///// <returns>返回带连接结果的消息</returns>
        //public static string InitialDbLink()
        //{
        //    bool baseLink = false;
        //    bool ruleLink = false;
        //    try
        //    {
        //        OSMDataBaseLinkForm basefm = new OSMDataBaseLinkForm();
        //        using (OracleConnection con = new OracleConnection(OSMDataBaseLinkForm.conStringTemp))
        //        {
        //            con.Open();
        //            if (con.State == ConnectionState.Open)
        //            {
        //                OSMDataBaseLinkForm.OSMLinkSuccess = true;
        //                baseLink = true;
        //            }
        //            con.Close();
        //        }
        //        OSMRuleLinkForm rulefm = new OSMRuleLinkForm();
        //        using (OracleConnection con = new OracleConnection(OSMRuleLinkForm.conRuleStringTmp))
        //        {
        //            con.Open();
        //            if (con.State == ConnectionState.Open)
        //            {
        //                OSMRuleLinkForm.RuleLinkSuccess = true;
        //                ruleLink = true;
        //            }
        //            con.Close();
        //        }
        //    }
        //    catch
        //    {
        //    }
        //    string message=null;
        //    if (baseLink)
        //    {
        //        message += String.Format("OSM 数据库[ {0} ]\r\n----参数快捷设置成功\r\n", OSMDataBaseLinkForm.Database0);
        //    }
        //    else 
        //    {
        //        message += String.Format("OSM 数据库参数快捷设置失败\r\n");
        //    }
        //    if (ruleLink)
        //    {
        //        message += String.Format("规则 数据库[ {0} ]\r\n----参数快捷设置成功\r\n", OSMRuleLinkForm.DataBaseName);
        //    }
        //    else 
        //    {
        //        message += String.Format("规则 数据库参数快捷设置失败\r\n");
        //    }
        //    return message;
            
        //}

        private void 增量信誉度交互ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string saveFlName = null;
            saveFileDlg.Filter = "文本文件(*.txt)|*.txt|所有文件(*.*)|*.*";
            if (con.State == ConnectionState.Open)
            {
                if (saveFileDlg.ShowDialog() == DialogResult.OK)
                {
                    saveFlName = saveFileDlg.FileName;
                    if (GenerateNoRepuIdFile(saveFlName))
                    {
                        this.exeStarteTBox.Text = "ID文件输出目录：\r\n" + saveFlName;
                    }
                    else 
                    {
                        this.exeStarteTBox.Text = "生成文件报错！";
                    }
                }
                else
                {
                    this.exeStarteTBox.Text = "已取消生成ID文件...";
                }
            }
            else 
            {
                this.exeStarteTBox.Text = "没有连接数据库...";
            }
        }
        /// <summary>
        /// 生成需要写信誉度的数据id的交互文件
        /// </summary>
        /// <param name="saveFlName"></param>
        private bool GenerateNoRepuIdFile(string saveFlName)
        {
            try
            {
                List<string> lineids = SqlHelper_OSC.GetNoRepuIdsFromLine(ImportOsc.oscLineTblName, con);
                List<string> areaids = SqlHelper_OSC.GetNoRepuIdsFromArea(ImportOsc.oscAreaTblName, con);
                XmlDocument doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", "no");
                doc.AppendChild(dec);
                XmlElement root = doc.CreateElement("way_ids");
                doc.AppendChild(root);
                //if (lineids.Count > 0)
                //{
                //    for (int i = 0; i < lineids.Count; i++)
                //    {
                //        XmlElement id = doc.CreateElement("id");
                //        id.InnerText = lineids[i];
                //        root.AppendChild(id);
                //    }
                //}
                if (areaids.Count > 0)
                {
                    for (int i = 0; i < areaids.Count; i++)
                    {
                        XmlElement id = doc.CreateElement("id");
                        id.InnerText = areaids[i];
                        root.AppendChild(id);
                    }
                }
                doc.Save(saveFlName);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        private void StartBtn_Click_1(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                if (selectOsc == true)
                {
                    //按时间段选取增量
                    startTime = startTimePicker.Value;
                    endTime = endTimePicker.Value;
                    this.exeStarteTBox.Text = "开始时间：" + startTime.ToString() + "\r\n";
                    this.exeStarteTBox.Text += "结束时间：" + endTime.ToString() + "\r\n";
                    if (startTime.CompareTo(endTime) < 0)
                    {
                        Thread getOscData = new Thread(GetOscDataWithTime);
                        getOscData.Start();
                    }
                    else
                    { this.exeStarteTBox.Text += "时间选择出错\r\n"; }
                }
                else if (renewOsm == true)
                {
                    this.exeStarteTBox.Text += "查找符合时间区间的增量\r\n";
                    //首先清除查找痕迹
                    this.exeStarteTBox.Text += SetGbLyrInvalid();
                    this.exeStarteTBox.Text += "查找符合时间区间的增量\r\n";
                    this.exeStarteTBox.Text = SetGbLyrValidWithTime();
                    this.exeStarteTBox.Text += "优化整理增量...\r\n";
                    this.exeStarteTBox.Text = OptimizeGbData(con);
                    this.exeStarteTBox.Text += "将优化后的增量更新到新基态表中...\r\n";
                    this.exeStarteTBox.Text = RenewGbNewTbl();
                    this.exeStarteTBox.ScrollToCaret();
                    selectOsc = false;
                    renewOsm = false;
                }
            }
            else
            {
                this.exeStarteTBox.Text = "请先连接数据库，后执行提取数据操作";
            }
        }

        


    }
}
//countryCB.Items.Add("china-taiwan");
//countryCB.Items.Add("pakistan");
//countryCB.Items.Add("india");
//countryCB.Items.Add("vietnam");
//countryCB.Items.Add("azerbaijan");
//countryCB.Items.Add("bangladesh");
//countryCB.Items.Add("gcc-states");
//countryCB.Items.Add("indonesia");
//countryCB.Items.Add("iran");
//countryCB.Items.Add("iraq");
//countryCB.Items.Add("israel-and-palestine");
//countryCB.Items.Add("japan");
//countryCB.Items.Add("jordan");
//countryCB.Items.Add("kazakhstan");
//countryCB.Items.Add("kyrgyzstan");
//countryCB.Items.Add("lebanon");
//countryCB.Items.Add("malaysia-singapore-brunei");
//countryCB.Items.Add("mongolia");
//countryCB.Items.Add("philippines");
//countryCB.Items.Add("tajikistan");
//countryCB.Items.Add("thailand");
//countryCB.Items.Add("turkmenistan");
//countryCB.Items.Add("uzbekistan");

//africa
//asia
//australia-oceania
//central-america
//europe
//north-america
//south-america
 