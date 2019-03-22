using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using GIS.Geometries;
using GIS.UI.Forms;
using GIS.UI.WellKnownText;
using Oracle.ManagedDataAccess.Client;

namespace TrustValueAndReputation.historyToDatabase
{
     public class OsmDataNode
    {
        public static string[] FClass = { "aerialway","aeroway","amenity",
                                            "barrier","boundary","building",
                                            "craft","emergency","geological",
                                            "highway","historic","landuse",
                                            "leisure","man_made","military",
                                         "natural","office","place",
                                         "power","public transport","railway",
                                         "route","shop","sport","tourism",
                                         "waterway"
                                        };
        //string osmpath;

        #region 字段、属性定义
        public uint id;
        public uint uid;
        public string user;
        public int version;
        public string changeset;
        public bool visible;
        public DateTime time;
        public List<string> tagkey;
        public List<string> value;
        public string changeType;
        public double lat;
        public double lon;
        public bool isSimple;
        public string fc;
        public string dsg;
        public string code;
        public string gbcode;
        public string gbdes;
        public string bz;
        public string name;
        public string name_en;
        public string name_zh;
        public string shapewkt;
        #endregion
        /// <summary>
        /// 根据XmlTextReader初始化OsmNode
        /// </summary>
        /// <param name="xr">XmlTextReader变量</param>
        /// <returns></returns>
        public bool InitialNodeFromXml(XmlTextReader xr)
        {
            new OsmDataNode();
            tagkey = new List<string>();
            value = new List<string>();
            try
            {
                id = uint.Parse(xr.GetAttribute("id"));
                version = int.Parse(xr.GetAttribute("version") == null ? "0" : xr.GetAttribute("version"));
                time = xr.GetAttribute("timestamp") == null ? DateTime.Parse("1970-01-01 00:01:01") : DateTime.Parse(xr.GetAttribute("timestamp"));
                uid = uint.Parse(xr.GetAttribute("uid") == null ? "0" : xr.GetAttribute("uid"));
                user = xr.GetAttribute("user") == null ? "" : xr.GetAttribute("user");
                changeset = xr.GetAttribute("changeset") == null ? "" : xr.GetAttribute("changeset");
                lat = double.Parse(xr.GetAttribute("lat"));
                lon = double.Parse(xr.GetAttribute("lon"));
                visible = true;
                shapewkt = GeometryToWKT.Write(new GIS.Geometries.GeoPoint(lon, lat));
                #region 提取 tag 并且在tag里面提取fc dsg name name_en name_zh
                if (xr.IsEmptyElement == false)
                {
                    isSimple = false;
                    while(xr.Read())
                    {
                        if (xr.NodeType == XmlNodeType.Element && xr.Name == "tag")
                        {
                            string k = xr.GetAttribute("k");
                            string v = xr.GetAttribute("v");
                            tagkey.Add(k);
                            value.Add(v);
                            if (FClass.Contains(k))
                            {
                                fc = k;
                                dsg = v;
                            }
                            if (v.Length < 400)
                            {
                                if (k == "name")
                                {
                                    name = v;
                                }
                                if (k == "name:zh")
                                {
                                    name_zh = v;
                                }
                                if (k == "name:en")
                                {
                                    name_en = v;
                                }
                            }
                        }
                        else if (xr.Name == "node" && xr.NodeType == XmlNodeType.EndElement)
                        {
                            break;
                        }
                    }
                }
                else 
                {
                    isSimple = true;
                }

                #endregion
                return true;
            }
            catch 
            {
                return false;
            }
        }
        /// <summary>
        /// 将tag标记用XML格式文档保存
        /// </summary>
        /// <returns></returns>
        public bool InitialNodeFromNpgsqlreader(OracleDataReader nr)
        {
            try
            {
                if (nr.HasRows)
                {
                    while (nr.Read())
                    {
                        this.id = uint.Parse(nr.GetValue(0).ToString());
                        this.user = nr.GetValue(1).ToString();
                        this.uid = uint.Parse(nr.GetValue(2).ToString());
                        this.changeset = nr.GetValue(3).ToString();
                        this.version = int.Parse(nr.GetValue(4).ToString());
                        this.time = DateTime.Parse(nr.GetValue(5).ToString());
                        this.lon = double.Parse(nr.GetValue(6).ToString());
                        this.lat = double.Parse(nr.GetValue(7).ToString());
                        XmlToTags(nr.GetValue(8).ToString());
                        this.changeType = nr.GetValue(9).ToString();
                    }
                }
                return true;
            }
            catch 
            {
                return false;
            }

        }

        public string tagsToXml()
        {
            if (tagkey.Count > 0)
            {
                using (MemoryStream fileStream = new MemoryStream())
                {
                    XmlTextWriter textWriter = new XmlTextWriter(fileStream, Encoding.UTF8);
                    textWriter.WriteStartDocument();
                    textWriter.WriteStartElement("nodetags");
                    for (int i = 0; i < this.tagkey.Count; i++)
                    {
                        textWriter.WriteStartElement("tag");
                        textWriter.WriteAttributeString("k", tagkey[i]);
                        textWriter.WriteAttributeString("v", value[i]);
                        textWriter.WriteEndElement();
                    }
                    textWriter.WriteEndElement();
                    textWriter.WriteEndDocument();
                    textWriter.Close();
                    byte[] data = fileStream.ToArray();
                    return Encoding.Default.GetString(data);
                }
            }
            else 
            {
                return null;
            }
            
 
        }
        public bool XmlToTags(string tagText)
        {
            try
            {
                using (XmlTextReader xtr = new XmlTextReader(tagText, XmlNodeType.Element, null))
                {
                    while (xtr.Read())
                    {
                        if (xtr.Name == "tag" && xtr.NodeType == XmlNodeType.Element)
                        {
                            tagkey.Add(xtr.GetAttribute("k"));
                            value.Add(xtr.GetAttribute("v"));
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public NODE ToNode()
        {
            NODE node = new NODE();
            node.osmid = id.ToString();
            node.changeset = changeset;
            node.timestamp = time;
            node.user = user;
            node.uid = uid.ToString();
            node.version = (short)version;
            node.visible = true;
            node.lon = lon;
            node.lat = lat;
            if (tagkey.Count > 0)
            {
                node.issimple = false;
                for (int i = 0; i < tagkey.Count; i++)
                {
                    string k = tagkey[i];
                    string v = value[i];
                    if (ImportOsc.FClass.Contains(k))
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
                    node.tags += tag;
                }
                if (node.tags != "")
                {
                    node.tags = node.tags.Remove(node.tags.Length - 1);
                }

            }
            else 
            {
                node.issimple = true;
            }
            return node;
        }

    }
}
