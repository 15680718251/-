using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using GIS.Geometries;
using GIS.UI.Forms;
using GIS.UI.WellKnownText;
using Oracle.ManagedDataAccess.Client;
using GIS.UI.AdditionalTool;
using System.Data;

namespace TrustValueAndReputation.historyToDatabase
{
    public partial class Importhistorydata : Form
    {
        public Importhistorydata()
        {
            InitializeComponent();
            
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        private void creattable(OracleConnection con)
        {
            string sum = null;

            if (SqlHelper_OSC.CreateTable(con, "Osm\\CreateTablePoint"))
            { sum += "Point-create "; }
            if (SqlHelper_OSC.CreateTable(con, "Osm\\CreateTablePolyline"))
            { sum += "Polyline-create "; }
            if (SqlHelper_OSC.CreateTable(con, "Osm\\CreateTablePolygon"))
            { sum += "Polygon-create "; }
            //if (SqlHelper_OSC.CreateTable(con, "Osm\\CreateTablePolylinesubversion"))
            //{ sum += "Polylinesubversion-create "; }
            //if (SqlHelper_OSC.CreateTable(con, "Osm\\CreateTablePolygonsubversion"))
            //{ sum += "Polylgonsubversion-create "; }
        }

         
        //Dictionary<string, int> osmEleCnt = new System.Collections.Generic.Dictionary<string, int>();
        private static string[] FClass = { "aerialway","aeroway","amenity","barrier","boundary","building","craft","emergency",
                                             "geological","highway","historic","landuse","leisure","man_made","military",
                                         "natural","office","place","power","public transport","railway","route","shop","sport","tourism","waterway"};
        //string osmpath;
        string[] osmpaths;
        DateTime start;
        long nodeStatic, wayStatic, relationStatic = 0;
        string conString = null;
        List<Thread> threads = new List<Thread>();
        int fileNum = 0;


        private void open_Click_1(object sender, EventArgs e)
        {
            
            OpenFileDialog fd = new OpenFileDialog();
            fd.Title = "输入OSM文件";
            fd.Filter = "OSM文件|*.osm" + "|OSM-XML文件|*.xml";
            fd.Multiselect = true;
            if (fd.ShowDialog() == DialogResult.OK)
            {

                osmpaths = fd.FileNames;
                //osmpath = osmpaths[0];
                osmOpenTB.Enabled = false;
                string files = "";
                for (int i = 0; i < fd.FileNames.Length; i++)
                {
                    files += fd.FileNames[i];
                    if (i != fd.FileNames.Length - 1)
                    {
                        files += ";";
                    }

                }
                osmOpenTB.Text = files;
            }

        }

        //public static string connectionString = OSMDataBaseLinkForm.conOSMString;
        private void import_Click_1(object sender, EventArgs e)
        {
            //OracleDBHelper.ExecuteSql("drop table point");
            //OracleDBHelper.ExecuteSql("drop table polyline");
            //OracleDBHelper.ExecuteSql("drop table polygon");
            try
            {
                conString = OSMDataBaseLinkForm.conStringTemp;
                using (OracleConnection con = new OracleConnection(conString))
                {
                    con.Open();
                    creattable(con);

                    OracleDBHelper helper = new OracleDBHelper();
                    helper.createTriger("point");
                    helper.createTriger("polyline");
                    helper.createTriger("polygon");
                }
            }
            catch (Exception)
            {

                MessageBox.Show("数据库连接失败！请检查连接参数！");
                return;
            }
            show2label.Text = "正在导入数据库，请稍后。。。";
            this.import.Enabled = false;
            this.open.Enabled = false;

            start = DateTime.Now;
            timelabel.Text = start.ToLocalTime() + "/";
            string osmpath;
            for (int i = 0; i < osmpaths.Length; i++)
            {
                osmpath = osmpaths[i];
                StreamReader sr = new StreamReader(osmpath, Encoding.UTF8);
                XmlTextReader xr = new XmlTextReader(sr);
                xr.WhitespaceHandling = WhitespaceHandling.None;
                ParameterizedThreadStart pts = new ParameterizedThreadStart(parseXml);
                Thread thread = new Thread(pts);
                threads.Add(thread);
                thread.Start(xr);

                //ThreadPool.QueueUserWorkItem(new WaitCallback(parseXml),xr);

            }
            
            //MessageBox.Show("入库完成！");
        }
        
        private void parseXml(object obj)
        {
            try
            {

                XmlTextReader xr = obj as XmlTextReader;
                while (xr.Read())
                {
                    switch (xr.NodeType)
                    {
                        case XmlNodeType.Attribute:
                            break;
                        case XmlNodeType.CDATA:
                            break;
                        case XmlNodeType.Comment:
                            break;
                        case XmlNodeType.Document:
                            break;
                        case XmlNodeType.DocumentFragment:
                            break;
                        case XmlNodeType.DocumentType:
                            break;
                        case XmlNodeType.Element: processElement(xr);
                            break;
                        case XmlNodeType.EndElement:
                            break;
                        case XmlNodeType.EndEntity:
                            break;
                        case XmlNodeType.Entity:
                            break;
                        case XmlNodeType.EntityReference:
                            break;
                        case XmlNodeType.None:
                            break;
                        case XmlNodeType.Notation:
                            break;
                        case XmlNodeType.ProcessingInstruction:
                            break;
                        case XmlNodeType.SignificantWhitespace:
                            break;
                        case XmlNodeType.Text:
                            break;
                        case XmlNodeType.Whitespace:
                            break;
                        case XmlNodeType.XmlDeclaration:
                            break;
                        default:

                            break;
                    }

                }
            }

            catch (System.Exception ex)
            {

            }

           

            //OracleDBHelper.ExecuteSql("update point set objectid=rownum");
            //OracleDBHelper.ExecuteSql("update polyline set objectid=rownum");
            //OracleDBHelper.ExecuteSql("update polygon set objectid=rownum");
            setEndTime("polygon");
            setEndTime("polyline");
            setEndTime("point");
            Console.WriteLine("入库完成了！！！！！！！！！！！！！！！！！！！");
            show2label.Text = "入库完成了。。。";
            //this.BeginInvoke(new System.EventHandler(showlabel));
        }

        private void setEndTime(string tablename)
        {  
            string  sqlread = "select distinct osmid from "+tablename;
        using (OracleDataReader dr = OracleDBHelper.ExecuteReader(sqlread))
        {
            while (dr.Read())
            {
                sqlread = "select max(versionid) from " + tablename + "  where osmid=" + dr["osmid"];
                using (OracleDataReader dr1 = OracleDBHelper.ExecuteReader(sqlread))
                {
                    while (dr1.Read())
                    {
                        Console.WriteLine("最大值为："+dr1["max(versionid)"].ToString());
                        if (dr1["max(versionid)"].ToString() == "1")//如果某条数据的versionID最大值1
                        {
                            sqlread = "update " + tablename + " set endtime=-1 where osmid=" + dr["osmid"];
                            OracleDBHelper.ExecuteSql(sqlread);//将改数据的endtime设置为-1
                        }
                        else
                        {
                            for (int i = 1; i <= int.Parse(dr1["max(versionid)"].ToString()); i++)
                            {
                                sqlread = "select starttime from " + tablename + "  where osmid=" + dr["osmid"] + " and versionid=" + (i + 1).ToString();
                                using (OracleDataReader dr3 = OracleDBHelper.ExecuteReader(sqlread))
                                {
                                    while (dr3.Read())
                                    {
                                        sqlread = "update " + tablename + " set endtime='" + dr3["starttime"] + "' where osmid=" + dr["osmid"] + " and versionid=" + (i).ToString();
                                        OracleDBHelper.ExecuteSql(sqlread);
                                    }
                                }
                                if (i == int.Parse(dr1["max(versionid)"].ToString()))
                                {
                                    sqlread = "update " + tablename + " set endtime=-1 where osmid=" + dr["osmid"] + " and versionid=" + (i).ToString();
                                    OracleDBHelper.ExecuteSql(sqlread);//将改数据的endtime设置为-1
                                }
                            }
                        }
                        

                    }
                }
               
            }
        }
        
        }
        
        private void showlabel(object o, EventArgs e)
        {
            fileNum++;
            //this.show2label.Text = "完成文件数目（" + fileNum + "）/" + "总文件数目（" + osmpaths.Length + ")";
            this.show2label.Text="            入库完成！";
            TimeSpan span = DateTime.Now - start;
            string expens = span.Days.ToString() + "天" + span.Hours.ToString() + "小时" + span.Minutes.ToString() + "分钟";
            this.timelabel.Text ="开始于："+ timelabel.Text + "/历时：" + expens;
            //DateTime.Now.ToLocalTime() + 
        }
        private void updateLabel(object o, EventArgs e)
        {
            string number = (string)o;
            this.show2label.Text = this.show2label.Text + "\n" + number;
        }
        private void updateNodeLabel(object o, EventArgs e)
        {
            string number = (string)o;
            this.pointlabel.Text = number;
        }
        private void updateWayLabel(object o, EventArgs e)
        {
            string number = (string)o;
            this.polylinelabel.Text = number;
        }
        private void updateRelationLabel(object o, EventArgs e)
        {
            string number = (string)o;
            this.polygonlabel.Text = number;
        }
        private void processElement(XmlTextReader xr)
        {
            try
            {


                switch (xr.Name)
                {
                    case "osm": break;
                    case "bounds": break;
                    case "node": processNode(xr);
                        
                        break;
                    case "way": processWay(xr);
                       
                        break;
                    case "relation": processRelation(xr); break;
                    case "nd": processND(xr); break;
                    case "tag": processTag(xr); break;
                    case "member": processMember(xr);
                        
                        break;
                    default:
                        break;
                }
                //OracleDBHelper.ExecuteSql("update point set objectid=rownum");
                //OracleDBHelper.ExecuteSql("update polyline set objectid=rownum");
                //OracleDBHelper.ExecuteSql("update polygon set objectid=rownum");
            }
            catch (Exception e)
            {
                // append(e.Message);
                return;
            }

        }

        private void processMember(XmlTextReader xr)
        {
            throw new NotImplementedException();
        }

        private void processTag(XmlTextReader xr)
        {
            throw new NotImplementedException();
        }

        private void processND(XmlTextReader xr)
        {
            throw new NotImplementedException();
        }

        private void processRelation(XmlTextReader xr)
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
                    relationStatic++;
                    string showStr = relationStatic.ToString();
                    //this.BeginInvoke(new System.EventHandler(updateRelationLabel), showStr);
                }
                catch (System.Exception ex)
                {

                }
                //holdpart++;
                //if (holdpart > threshold)
                //{
                //    holdpart = 0;
                //    indiaBase.Dispose();
                //    indiaBase = new indiaEntities();
                //}

                RELATION relation = initialRelation(xr);
                int memberNum = 0, tagNum = 0;
                string tags = "";
                //注意，此处只把relation为多多变形的存入数据库
                List<string> outerrels = new List<string>();
                List<string> innerrels = new List<string>();
                bool isPolygon = false;
                while (xr.Read() && (xr.Name == "member" || xr.Name == "tag"))
                {
                    if (xr.Name == "member")
                    {
                        //RelationMember member = new RelationMember();
                        string type = xr.GetAttribute("type");
                        string role = xr.GetAttribute("role");
                        if (xr.GetAttribute("ref") == null)//有可能存在问题 
                        {
                            continue;
                        }
                        memberNum++;
                        string id = xr.GetAttribute("ref");
                        if (role == "outer")
                        {
                            outerrels.Add(id);
                        }
                        if (role == "inner")
                        {
                            innerrels.Add(id);
                        }


                    }
                    if (xr.Name == "tag")
                    {
                        tagNum++;
                        string k = xr.GetAttribute("k");
                        string v = xr.GetAttribute("v");
                        if (FClass.Contains(k))
                        {
                            relation.fc = k;
                            relation.dsg = v;
                            continue;
                        }
                        if (k == "name")
                        {
                            relation.name = v;
                            continue;
                        }
                        if (k == "name:zh")
                        {
                            relation.name_zh = v;
                            continue;
                        }
                        if (k == "name:en")
                        {
                            relation.name_en = v;
                            continue;
                        }
                        if (v == "multipolygon")
                        {
                            isPolygon = true;
                            continue;
                        }
                        string tag = k;
                        tag += "=";
                        tag += v;
                        tag += "&";
                        tags += tag;
                    }
                }
                if (isPolygon == true)
                {
                    if (tags != "")
                    {
                        tags = tags.Remove(tags.Length - 1);
                    }
                    relation.tags = tags;
                    if (tagNum > 0)
                    {
                        relation.issimple = false;
                    }
                    else
                    {
                        relation.issimple = true;
                    }
                    // Importoshhelper helper = new Importoshhelper(conString);
                    //Geometry relationGeo = helper.buildRelation(outerrels, innerrels);
                    //if (relationGeo == null)
                    //{
                    //    return;
                    //}
                    //string wkt = GeometryToWKT.Write(relationGeo);
                    //if (helper.relation2PostGIS(relation, wkt) < 1)
                    //{
                    //    // append("插入失败，relationid:" + relation.osmid + " lineNumber:" + xr.LineNumber);
                    //}



                }

            }
        }

        private RELATION initialRelation(XmlTextReader xr)
        {
            RELATION node = new RELATION();
            //int id =Int32.Parse(xr.GetAttribute("id"));
            node.osmid = xr.GetAttribute("id");
            node.changeset = xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset");
            node.timestamp = xr.GetAttribute("timestamp") == null ? DateTime.Now : DateTime.Parse(xr.GetAttribute("timestamp"));
            node.user = xr.GetAttribute("username") == null ? "" : xr.GetAttribute("username");
            node.uid = xr.GetAttribute("userid") == null ? "" : xr.GetAttribute("userid");
            node.version = xr.GetAttribute("version") == null ? (short)0 : short.Parse(xr.GetAttribute("version"));
            node.visible = xr.GetAttribute("visible") == null ? false : bool.Parse(xr.GetAttribute("visible"));
            
            return node;
        }

        private void processWay(XmlTextReader xr)
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
                //try
                //{
                //    wayStatic++;
                //    string showStr = wayStatic.ToString();
                //    this.BeginInvoke(new System.EventHandler(updateWayLabel), showStr);
                //}
                //catch (System.Exception ex)
                //{

                //}
                //holdpart++;
                //if (holdpart > threshold)
                //{
                //    holdpart = 0;
                //    indiaBase.Dispose();
                //    indiaBase = new indiaEntities();
                //}

                WAY way = initialWay(xr);
                int nodeNum = 0, tagNum = 0;
                string tags = "";
                List<string> rels = new List<string>();
                while (xr.Read() && (xr.Name == "nd" || xr.Name == "tag"))
                {
                    if (xr.Name == "nd")
                    {

                        if (xr.GetAttribute("ref") == null)
                        {
                            continue;
                        }
                        nodeNum++;
                        string id = xr.GetAttribute("ref");
                        rels.Add(id);                      


                    }
                    if (xr.Name == "tag")
                    {
                        tagNum++;
                        string k = xr.GetAttribute("k");
                        string v = xr.GetAttribute("v");
                        if (FClass.Contains(k))
                        {
                            way.fc = k;
                            way.dsg = v;
                            continue;
                        }
                        if (k == "name")
                        {
                            way.name = v;
                            continue;
                        }
                        if (k == "name:zh")
                        {
                            way.name_zh = v;
                            continue;
                        }
                        if (k == "name:en")
                        {
                            way.name_en = v;
                            continue;
                        }
                        string tag = k;
                        tag += "=";
                        tag += v;
                        tag += "&";
                        tags += tag;

                    }
                }
                //add pointid
                for (int i = 0; i < rels.Count; i++)
                {
                    way.pointids += rels[i] + ",";
                }
                
                if (tags != "")
                {
                    tags = tags.Remove(tags.Length - 1);
                }
                way.tags = tags;
                if (tagNum > 0)
                {
                    way.issimple = false;
                }
                else
                {
                    way.issimple = true;
                }
                Importoshhelper helper = new Importoshhelper(conString);
                helper = new Importoshhelper(conString);
                Geometry wayGeo = helper.buildway(rels);
                if (wayGeo == null)
                {
                    return;
                }
                string wkt = GeometryToWKT.Write(wayGeo);
                if (wayGeo is GeoLineString)
                {
                    if (helper.way2PostGIS(way, wkt, 0) < 1)
                    {
                        //append("插入失败，wayid:" + way.osmid + " lineNumber:" + xr.LineNumber);
                    }
                    try
                    {
                        wayStatic++;
                        string showStr = wayStatic.ToString();
                        this.BeginInvoke(new System.EventHandler(updateWayLabel), showStr);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                if (wayGeo is GeoPolygon)
                {
                    if (helper.way2PostGIS(way, wkt, 1) < 1)
                    {
                        //append("插入失败，wayid:" + way.osmid + " lineNumber:" + xr.LineNumber);
                    }
                    try
                    {
                        relationStatic++;
                        string showStr = relationStatic.ToString();
                        this.BeginInvoke(new System.EventHandler(updateRelationLabel), showStr);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

            }
        
}
        private WAY initialWay(XmlTextReader xr)
        {
            WAY node = new WAY();
            //int id =Int32.Parse(xr.GetAttribute("id"));
            node.osmid = xr.GetAttribute("id");
            node.changeset = xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset");
            node.timestamp = xr.GetAttribute("timestamp") == null ? DateTime.Now : DateTime.Parse(xr.GetAttribute("timestamp"));
            node.user = xr.GetAttribute("user") == null ? "" : xr.GetAttribute("user");
            node.uid = xr.GetAttribute("uid") == null ? "" : xr.GetAttribute("uid");
            //node.points = xr.GetAttribute("ref") == null ? "" : xr.GetAttribute("ref");
            node.version = xr.GetAttribute("version") == null ? (short)0 : short.Parse(xr.GetAttribute("version"));
            node.visible = xr.GetAttribute("visible") == null ? false : bool.Parse(xr.GetAttribute("visible"));
            return node;
        }

        private void processNode(XmlTextReader xr)
        {
            //try
            //{
            //    nodeStatic++;
            //    string showStr = nodeStatic.ToString();
            //    this.BeginInvoke(new System.EventHandler(updateNodeLabel), showStr);
            //}
            //catch (System.Exception ex)
            //{

            //}
            //holdpart++;
            //if (holdpart > threshold)
            //{
            //    holdpart = 0;
            //    indiaBase.Dispose();
            //    indiaBase = new indiaEntities();
            //}
            if (xr.IsEmptyElement)//如果node不含有其他子元素，说明该node为简单点
            {
                if (xr.HasAttributes)
                {
                    NODE node = initialNode(xr);
                    node.issimple = true;

                    string wkt = GeometryToWKT.Write(new GIS.Geometries.GeoPoint(node.lon, node.lat));
                    Importoshhelper helper = new Importoshhelper(conString);
                    if (helper.node2PostGIS(node, wkt) <= 0)
                    {
                        //append("插入失败，nodeid:" + node.osmid + " lineNumber:" + xr.LineNumber);
                    }
                    try
                    {
                        nodeStatic++;
                        string showStr = nodeStatic.ToString();
                        this.BeginInvoke(new System.EventHandler(updateNodeLabel), showStr);
                    }
                    catch (System.Exception ex)
                    {

                    }
                }
            }
            else
            {
                NODE node = initialNode(xr);
                node.issimple = false;
                string tags = "";
                while (xr.Read() && xr.Name == "tag")
                {
                    string k = xr.GetAttribute("k");
                    string v = xr.GetAttribute("v");
                    if (FClass.Contains(k))
                    {
                        node.fc = k;
                        node.dsg = v;
                        continue;
                    }
                    if (k == "name")
                    {
                        node.name = v;
                        continue;
                    }
                    if (k == "name:zh")
                    {
                        node.name_zh = v;
                        continue;
                    }
                    if (k == "name:en")
                    {
                        node.name_en = v;
                        continue;
                    }
                    string tag = k;
                    tag += "=";
                    tag += v;
                    tag += "&";
                    tags += tag;
                }
                if (tags != "")
                {
                    tags = tags.Remove(tags.Length - 1);
                }
                node.tags = tags;

                string wkt = GeometryToWKT.Write(new GIS.Geometries.GeoPoint(node.lon, node.lat));
                Importoshhelper helper = new Importoshhelper(conString);
                if (helper.node2PostGIS(node, wkt) <= 0)
                {
                    //append("插入失败，nodeid:"+node.osmid+" lineNumber:"+xr.LineNumber);
                }
                try
                {
                    nodeStatic++;
                    string showStr = nodeStatic.ToString();
                    this.BeginInvoke(new System.EventHandler(updateNodeLabel), showStr);
                }
                catch (System.Exception ex)
                {

                }

            }
            
        }

        private void append(string p)
        {
            var path = Application.StartupPath;
            //var lastChar=path.Substring()
            path = path + "\\" + "geonamesPostGISlog.txt";
            if (File.Exists(path))
            {
                StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8);
                sw.WriteLine(p);
                sw.Close();
            }
            else
            {
                FileStream fs = new FileStream(path, FileMode.CreateNew);
                //fs.Close();
                StreamWriter sw = new StreamWriter(fs);
                //该编码类型不会改变已有文件的编码类型
                sw.WriteLine(p);
                sw.Close();
                fs.Close();
            }


        }

        private static string showNode(NODE node)
        {
            string nodeStr = "";
            nodeStr += "id(20):" + node.osmid + "=" + node.osmid.Length;
            nodeStr += "\n";
            nodeStr += "username(50):" + node.user + "=" + node.user.Length;
            nodeStr += "\n";
            nodeStr += "userid(50):" + node.uid + "=" + node.uid.Length;
            nodeStr += "\n";
            nodeStr += "(notnull)lat:" + node.lat;
            nodeStr += "\n";
            nodeStr += "(notnull)lon:" + node.lon;
            nodeStr += "\n";
            nodeStr += "visible:" + node.visible;
            nodeStr += "\n";
            nodeStr += "version:" + node.version + "=";
            nodeStr += "\n";
            nodeStr += "changeset(20):" + node.changeset + "=" + node.changeset.Length;
            nodeStr += "\n";
            nodeStr += "(notnull)timestamp:" + node.timestamp.ToLocalTime();
            nodeStr += "\n";
            //nodeStr += "(notnull)issimple:" + node.issimple;
            //nodeStr += "\n";
            //nodeStr += "fc(20):" + (node.fc == null ? "" : node.fc) + "=" + (node.fc == null ? "" : node.fc.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "dsg(20):" + (node.dsg == null ? "" : node.dsg) + "=" + (node.dsg == null ? "" : node.dsg.Length.ToString());
            //nodeStr += "\n";
            nodeStr += "code(2):" + (node.code == null ? "" : node.code) + "=" + (node.code == null ? "" : node.code.Length.ToString());
            nodeStr += "\n";
            //nodeStr += "gbcode(20):" + (node.gbcode == null ? "" : node.gbcode) + "=" + (node.gbcode == null ? "" : node.gbcode.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "gbdes(20):" + (node.gbdes == null ? "" : node.gbdes) + "=" + (node.gbdes == null ? "" : node.gbdes.Length.ToString());
            //nodeStr += "\n";
            nodeStr += "tags(Max):" + (node.tags == null ? "" : node.tags) + "=" + (node.tags == null ? "" : node.tags.Length.ToString());
            nodeStr += "\n";
            //nodeStr += "bz(50):" + (node.bz == null ? "" : node.bz) + "=" + (node.bz == null ? "" : node.bz.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "name(200):" + (node.name == null ? "" : node.name) + "=" + (node.name == null ? "" : node.name.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "name_en(200):" + (node.name_en == null ? "" : node.name_en) + "=" + (node.name_en == null ? "" : node.name_en.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "name_zh(200):" + (node.name_zh == null ? "" : node.name_zh) + "=" + (node.name_zh == null ? "" : node.name_zh.Length.ToString());
            //nodeStr += "\n";
            return nodeStr;
            //MessageBox.Show(nodeStr);
        }
        private static string showRelation(RELATION node)
        {
            string nodeStr = "";
            nodeStr += "id(20):" + node.osmid + "=" + node.osmid.Length;
            nodeStr += "\n";
            nodeStr += "username(50):" + node.user + "=" + node.user.Length;
            nodeStr += "\n";
            nodeStr += "userid(50):" + node.uid + "=" + node.uid.Length;
            nodeStr += "\n";
            //nodeStr += "(notnull)lat:" + node.lat;
            //nodeStr += "\n";
            //nodeStr += "(notnull)lon:" + node.lon;
            //nodeStr += "\n";
            nodeStr += "visible:" + node.visible;
            nodeStr += "\n";
            nodeStr += "version:" + node.version + "=";
            nodeStr += "\n";
            nodeStr += "changeset(20):" + node.changeset + "=" + node.changeset.Length;
            nodeStr += "\n";
            nodeStr += "(notnull)timestamp:" + node.timestamp.ToLocalTime();
            nodeStr += "\n";
            //nodeStr += "(notnull)issimple:" + node.issimple;
            //nodeStr += "\n";
            //nodeStr += "fc(20):" + (node.fc == null ? "" : node.fc) + "=" + (node.fc == null ? "" : node.fc.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "dsg(20):" + (node.dsg == null ? "" : node.dsg) + "=" + (node.dsg == null ? "" : node.dsg.Length.ToString());
            //nodeStr += "\n";
            nodeStr += "code(2):" + (node.code == null ? "" : node.code) + "=" + (node.code == null ? "" : node.code.Length.ToString());
            nodeStr += "\n";
            //nodeStr += "gbcode(20):" + (node.gbcode == null ? "" : node.gbcode) + "=" + (node.gbcode == null ? "" : node.gbcode.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "gbdes(20):" + (node.gbdes == null ? "" : node.gbdes) + "=" + (node.gbdes == null ? "" : node.gbdes.Length.ToString());
            //nodeStr += "\n";
            nodeStr += "tags(Max):" + (node.tags == null ? "" : node.tags) + "=" + (node.tags == null ? "" : node.tags.Length.ToString());
            nodeStr += "\n";
            //nodeStr += "bz(50):" + (node.bz == null ? "" : node.bz) + "=" + (node.bz == null ? "" : node.bz.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "name(200):" + (node.name == null ? "" : node.name) + "=" + (node.name == null ? "" : node.name.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "name_en(200):" + (node.name_en == null ? "" : node.name_en) + "=" + (node.name_en == null ? "" : node.name_en.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "name_zh(200):" + (node.name_zh == null ? "" : node.name_zh) + "=" + (node.name_zh == null ? "" : node.name_zh.Length.ToString());
            //nodeStr += "\n";
            return nodeStr;
            //MessageBox.Show(nodeStr);
        }
        private static string showWay(WAY node)
        {
            string nodeStr = "";
            nodeStr += "osmid(20):" + node.osmid + "=" + node.osmid.Length;
            nodeStr += "\n";
            nodeStr += "user(50):" + node.user + "=" + node.user.Length;
            nodeStr += "\n";
            nodeStr += "uid(50):" + node.uid + "=" + node.uid.Length;
            nodeStr += "\n";
            //nodeStr += "(notnull)lat:" + node.lat;
            //nodeStr += "\n";
            //nodeStr += "(notnull)lon:" + node.lon;
            //nodeStr += "\n";
            nodeStr += "visible:" + node.visible;
            nodeStr += "\n";
            nodeStr += "version:" + node.version + "=";
            nodeStr += "\n";
            nodeStr += "changeset(20):" + node.changeset + "=" + node.changeset.Length;
            nodeStr += "\n";
            nodeStr += "(notnull)timestamp:" + node.timestamp.ToLocalTime();
            nodeStr += "\n";
            //nodeStr += "(notnull)issimple:" + node.issimple;
            //nodeStr += "\n";
            //nodeStr += "fc(20):" + (node.fc == null ? "" : node.fc) + "=" + (node.fc == null ? "" : node.fc.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "dsg(20):" + (node.dsg == null ? "" : node.dsg) + "=" + (node.dsg == null ? "" : node.dsg.Length.ToString());
            //nodeStr += "\n";
            nodeStr += "code(2):" + (node.code == null ? "" : node.code) + "=" + (node.code == null ? "" : node.code.Length.ToString());
            nodeStr += "\n";
            //nodeStr += "gbcode(20):" + (node.gbcode == null ? "" : node.gbcode) + "=" + (node.gbcode == null ? "" : node.gbcode.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "gbdes(20):" + (node.gbdes == null ? "" : node.gbdes) + "=" + (node.gbdes == null ? "" : node.gbdes.Length.ToString());
            //nodeStr += "\n";
            nodeStr += "tags(Max):" + (node.tags == null ? "" : node.tags) + "=" + (node.tags == null ? "" : node.tags.Length.ToString());
            nodeStr += "\n";
            //nodeStr += "bz(50):" + (node.bz == null ? "" : node.bz) + "=" + (node.bz == null ? "" : node.bz.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "name(200):" + (node.name == null ? "" : node.name) + "=" + (node.name == null ? "" : node.name.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "name_en(200):" + (node.name_en == null ? "" : node.name_en) + "=" + (node.name_en == null ? "" : node.name_en.Length.ToString());
            //nodeStr += "\n";
            //nodeStr += "name_zh(200):" + (node.name_zh == null ? "" : node.name_zh) + "=" + (node.name_zh == null ? "" : node.name_zh.Length.ToString());
            //nodeStr += "\n";
            return nodeStr;
            //MessageBox.Show(nodeStr);
        }
        /// <summary>
        /// 初始化node
        /// </summary>
        /// <param name="xr"></param>
        private static NODE initialNode(XmlTextReader xr)
        {
            NODE node = new NODE();
            //int id =Int32.Parse(xr.GetAttribute("id"));
            node.osmid = xr.GetAttribute("id");
            node.changeset = xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset");
            node.timestamp = xr.GetAttribute("timestamp") == null ? DateTime.Now : DateTime.Parse(xr.GetAttribute("timestamp"));
            node.user = xr.GetAttribute("user") == null ? "" : xr.GetAttribute("user");
            node.uid = xr.GetAttribute("uid") == null ? "" : xr.GetAttribute("uid");
            node.version = xr.GetAttribute("version") == null ? (short)0 : short.Parse(xr.GetAttribute("version"));
            node.visible = xr.GetAttribute("visible") == null ? false : bool.Parse(xr.GetAttribute("visible"));
            node.lon = double.Parse(xr.GetAttribute("lon"));
            node.lat = double.Parse(xr.GetAttribute("lat"));
            return node;
        }

        private void ImportData_FormClosing(object sender, FormClosingEventArgs e)
        {
            //indiaBase.Dispose();
            if (threads.Count > 0)
            {
                for (int i = 0; i < threads.Count; i++)
                {
                    threads[i].Abort();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.comboBox1.Text == "基态表")
            {
                //OracleDBHelper helper = new OracleDBHelper();
                //string sql = "insert into polygon (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from APOLY where pointsid is not null";
                //helper.sqlExecuteUnClose(sql);

                basicOrIncToHistory("APOLY", "polygon");
                Console.WriteLine("基态面插入历史面成功。。。。。。。。。。。。。。。。。。。。。。。");

                //      sql = "insert into POLYLINE (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from ALINE where pointsid is not null";
                //helper.sqlExecuteUnClose(sql);

                basicOrIncToHistory("ALINE", "POLYLINE");
                Console.WriteLine("基态线插入历史线成功。。。。。。。。。。。。。。。。。。。。。。");


                //sql = "insert into POINT (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape from APOINT";
                //helper.sqlExecuteUnClose(sql);
                //Console.WriteLine("基态点插入历史点成功");

                //basicToHistory();

                //string sqlInsert = "insert into polygon (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from apoly ";
                //baseToHistory("polygon", "apoly", sqlInsert);//基态面到历史面
                //sqlInsert = "insert into point (osmid,username,userid,lat,lon,versionid,starttime,changeset,fc,dsg,tags,shape) select osmid,username,userid,lat,lon,versionid,starttime,changeset,fc,dsg,tags,shape from apoint ";
                //baseToHistory("point", "apoint", sqlInsert);//基态点到历史点
                //sqlInsert = "insert into polyline (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from aline ";
                //baseToHistory("polyline", "aline", sqlInsert);//基态线到历史线
            }


            if (this.comboBox1.Text == "增量表")
            {
                //OracleDBHelper helper = new OracleDBHelper();
                //string sql = "insert into polygon (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from OSCAREA where pointsid is not null";
                //helper.sqlExecuteUnClose(sql);

                basicOrIncToHistory("OSCAREA", "polygon");
                Console.WriteLine("增量面插入历史面成功。。。。。。。。。。。。。。。。。。。。。");

                //      sql = "insert into POLYLINE (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from OSCLINE where pointsid is not null";
                //helper.sqlExecuteUnClose(sql);

                basicOrIncToHistory("OSCLINE", "POLYLINE");
                Console.WriteLine("增量线插入历史线成功。。。。。。。。。。。。。。。。。。。。。");


                //sql = "insert into POINT (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from OSCPOINT";
                //helper.sqlExecuteUnClose(sql);
                //Console.WriteLine("增量点插入历史点成功");

                //incToHistory();

                //string sqlInsert = "insert into polygon (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape, pointsid from oscarea ";
                //incToHistory("polygon", "oscarea", sqlInsert);//增量面到历史面
                //sqlInsert = "insert into point (osmid,username,userid,lat,lon,versionid,starttime,changeset,fc,dsg,tags,shape) select osmid,username,userid,lat,lon,versionid,starttime,changeset,fc,dsg,tags,shape from oscpoint ";
                //incToHistory("point", "oscpoint", sqlInsert);//增量点到历史点
                //sqlInsert = "insert into polyline (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from oscline ";
                //incToHistory("polyline", "oscline", sqlInsert);//增量线到历史线
            }

            if(this.comboBox1.Text ==null)
            {
                MessageBox.Show("请选择基态或增量");
            }

        }

        public void basicOrIncToHistory(string tableName, string hisTableName)
        {
            OracleDBHelper conHelper = new OracleDBHelper();

            string sql = string.Format("select osmid,versionid from {0}", tableName);
            using (OracleDataReader dr = conHelper.queryReader(sql))
            {
                while (dr.Read())
                {
                    sql = String.Format("select count(*) from {0}  where osmid={1} and versionid={2}", hisTableName, dr["osmid"], dr["versionid"]);
                    using (OracleDataReader dr1 = conHelper.queryReader(sql))
                    {
                        while (dr1.Read())
                        {
                            if (dr1["count(*)"].ToString() == "0")
                            {//如果没有相同osmid versionid 则直接插入
                                //OracleDBHelper helper = new OracleDBHelper();
                                sql = string.Format("insert into {0} (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from {1} where osmid = {2} and versionid = {3}", hisTableName, tableName, dr["osmid"], dr["versionid"]);
                                //sql = string.Format("update {0} set trustvalue= {1},userreputation={4} where osmid={2} and versionid={3}", hisTableName, dr["trustvalue"], dr["osmid"], dr["versionid"], dr["userreputation"]);
                                conHelper.sqlExecuteUnClose(sql);
                                Console.WriteLine("插入一行");
                            }

                        }
                    }
                }
            }
        }

        private void basicToHistory()
        {
            string[] shapeType = { "AREA", "ALINE", "APOINT" };
            foreach (string shape in shapeType)
            {
                string[] elementType = { "RESIDENTIAL_", "SOIL_", "TRAFFIC_", "VEGETATION_", "WATER_" };
                foreach (string element in elementType)
                {
                    string tableName = element + shape;
                    if (shape == "AREA")
                    {
                        string sql = "insert into polygon (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from " + tableName;
                        OracleDBHelper helper = new OracleDBHelper();
                        helper.sqlExecuteUnClose(sql);
                        Console.WriteLine(tableName + "插入polygon成功");
                    }
                    if (shape == "LINE")
                    {
                        string sql = "insert into POLYLINE (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from " + tableName;
                        OracleDBHelper helper = new OracleDBHelper();
                        helper.sqlExecuteUnClose(sql);
                        Console.WriteLine(tableName + "插入POLYLINE成功");
                    }
                    if (shape == "POINT")
                    {
                        string sql = "insert into POINT (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from " + tableName;
                        OracleDBHelper helper = new OracleDBHelper();
                        helper.sqlExecuteUnClose(sql);
                        Console.WriteLine(tableName + "插入POINT成功");
                    }

                }

            }
           
        }

        private void incToHistory()
        {
            string[] shapeType = { "AREA", "LINE", "POINT" };
            foreach (string shape in shapeType)
            {
                string[] elementType = { "RESIDENTIAL_NEW", "SOIL_NEW", "TRAFFIC_NEW", "VEGETATION_NEW", "WATER_NEW" };
                foreach (string element in elementType)
                {
                    string tableName = element + shape;
                    if (shape == "AREA")
                    {
                        string sql = "insert into polygon (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from " + tableName;
                        OracleDBHelper helper = new OracleDBHelper();
                        helper.sqlExecuteUnClose(sql);
                        Console.WriteLine(tableName + "插入polygon成功");
                    }
                    if (shape == "LINE")
                    {
                        string sql = "insert into POLYLINE (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from " + tableName;
                        OracleDBHelper helper = new OracleDBHelper();
                        helper.sqlExecuteUnClose(sql);
                        Console.WriteLine(tableName + "插入polygon成功");
                    }
                    if (shape == "POINT")
                    {
                        string sql = "insert into POINT (osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid) select osmid,username,userid,versionid,starttime,changeset,fc,dsg,tags,shape,pointsid from " + tableName;
                        OracleDBHelper helper = new OracleDBHelper();
                        helper.sqlExecuteUnClose(sql);
                        Console.WriteLine(tableName + "插入polygon成功");
                    }

                }

            }

        }

        //RESIDENTIAL_AREA
        //将基态数据或者增量数据复制历史数据库才可以计算基态或者增量数据的可信度
        private void baseToHistory(string osmTable, string oscTable, string sqlInsert)
        {

            string sqlread = "select osmid,versionid from " + oscTable;
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(sqlread))//读取基态或者增量数据表中的osmid
            {
                while (dr.Read())
                {
                    StringBuilder sqlInsert1 = new StringBuilder(sqlInsert);
                    sqlInsert1.Append(" where osmid=" + dr["osmid"]);
                    //sqlInsert=String.Format(sqlInsert+" where osmid=" + dr["osmid"]);
                    sqlread = String.Format("select count(*) from {0}  where osmid={1}", osmTable, dr["osmid"]);
                    using (OracleDataReader dr1 = OracleDBHelper.ExecuteReader(sqlread))
                    {
                        while (dr1.Read())
                        {
                            if (dr1["count(*)"].ToString() != "0")//判断历史数据表中是否有相同是osmid
                            {
                                //如果有相同的osmid 判断其versionID是否相同
                                sqlread = String.Format("select count(*) from {0}  where osmid={1} and versionid={2}", osmTable, dr["osmid"], dr["versionid"]);
                                using (OracleDataReader dr2 = OracleDBHelper.ExecuteReader(sqlread))
                                {
                                    while (dr2.Read())
                                    {
                                        if (dr2["count(*)"].ToString() == "0")
                                        {//如果versionid不同 则直接插入
                                            OracleDBHelper helper = new OracleDBHelper();
                                            helper.sqlExecuteUnClose(sqlInsert1.ToString());
                                            Console.WriteLine("插入一行");
                                        }
                                    }

                                }

                            }
                            else
                            { //如果没有相同的osmid 则直接插入
                                OracleDBHelper helper = new OracleDBHelper();
                                helper.sqlExecuteUnClose(sqlInsert1.ToString());
                                Console.WriteLine("插入一行");
                            }
                        }


                    }

                }

            }
        }

        private void incToHistory(string tablename1, string tablename2, string sqlInsert)
        {    //table1历史
            //table2基态或者增量
            //string sql = "";
            //string sql1 = "";
            string sqlread = "select osmid,versionid from " + tablename2;
            using (OracleDataReader dr = OracleDBHelper.ExecuteReader(sqlread))//读取基态或者增量数据表中的osmid
            {
                while (dr.Read())
                {
                    StringBuilder sqlInsert1 = new StringBuilder(sqlInsert);
                    sqlInsert1.Append(" where osmid=" + dr["osmid"]);
                    //sqlInsert=String.Format(sqlInsert+" where osmid=" + dr["osmid"]);
                    sqlread = String.Format("select count(*) from {0}  where osmid={1}", tablename1, dr["osmid"]);
                    using (OracleDataReader dr1 = OracleDBHelper.ExecuteReader(sqlread))
                    {
                        while (dr1.Read())
                        {
                            if (dr1["count(*)"].ToString() != "0")//判断历史数据表中是否有相同是osmid
                            {
                                //如果有相同的osmid 判断其versionID是否相同
                                sqlread = String.Format("select count(*) from {0}  where osmid={1} and versionid={2}", tablename1, dr["osmid"], dr["versionid"]);
                                using (OracleDataReader dr2 = OracleDBHelper.ExecuteReader(sqlread))
                                {
                                    while (dr2.Read())
                                    {
                                        if (dr2["count(*)"].ToString() == "0")
                                        {//如果versionid不同 则直接插入
                                            OracleDBHelper helper = new OracleDBHelper();
                                            helper.sqlExecuteUnClose(sqlInsert1.ToString());
                                            Console.WriteLine("插入一行");
                                        }
                                    }

                                }

                            }
                            else
                            { //如果没有相同的osmid 则直接插入
                                OracleDBHelper helper = new OracleDBHelper();
                                helper.sqlExecuteUnClose(sqlInsert1.ToString());
                                Console.WriteLine("插入一行");
                            }
                        }


                    }

                }

            }
        }
       

        private void Importhistorydata_Load(object sender, EventArgs e)
        {
            string[] TableNames = { "基态表", "增量表" };
            this.comboBox1.Items.Clear();
            if (TableNames != null && TableNames.Count() > 0)
            {
                foreach (string name in TableNames)
                {
                    comboBox1.Items.Add(name);
                }
            }
            else
            {
                MessageBox.Show("对不起，您连接的数据库中无基态数据表！！！");
            }

        }


       
        

       
    }
}

