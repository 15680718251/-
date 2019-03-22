using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using GIS.UI.Forms;
using Oracle.ManagedDataAccess.Client;

namespace TrustValueAndReputation.historyToDatabase
{
    public class OsmDataWay
    {
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
        public bool isSimple;
        public List<uint> refNodeId;
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
        public bool isPolygon;
        
        /// <summary>
        /// 根据XML节点初始化way元素
        /// </summary>
        /// <param name="xr">XML节点</param>
        /// <returns>返回初始化结果：成功true 失败false</returns>
        public bool InitialWayFromXml(XmlTextReader xr,OracleConnection  con)
        {
            new OsmDataWay();
            refNodeId = new List<uint>();
            tagkey = new List<string>();
            value = new List<string>();
            try
            {
                id = uint.Parse(xr.GetAttribute("id"));
               
                version = int.Parse(xr.GetAttribute("version"));
                time = DateTime.Parse(xr.GetAttribute("timestamp"));
                uid = uint.Parse(xr.GetAttribute("uid"));
                user = xr.GetAttribute("user");
                changeset = xr.GetAttribute("changeset");
                visible = true;
                #region 解析tag 赋值fc dsg relnode等
                if (xr.IsEmptyElement == false)
                {
                    isSimple = false;
                    while (xr.Read())
                    {
                        if (xr.Name == "nd")
                        {
                            refNodeId.Add(uint.Parse(xr.GetAttribute("ref")));
                        }
                        else if (xr.Name == "tag")
                        {
                            string k = xr.GetAttribute("k");
                            string v = xr.GetAttribute("v");
                            tagkey.Add(k);
                            value.Add(v);
                            if (OsmDataNode.FClass.Contains(k))
                            {   fc = k;dsg = v;   }
                            if (v.Length < 400)
                            {
                                if (k == "name")
                                { name = v; }
                                if (k == "name:zh")
                                { name_zh = v; }
                                if (k == "name:en")
                                { name_en = v; }
                            }
                        }
                        else if (xr.Name == "way" && xr.NodeType == XmlNodeType.EndElement)
                        {
                            break;
                        }
                    }
                }
                #endregion
                shapewkt = SqlHelper_OSC.BuildWay(refNodeId, con,out isPolygon);
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

        public bool InitialWayFromNpgsqlreader(OracleDataReader nr)
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
                        TextToNodeIds(nr.GetValue(6).ToString());
                        XmlToTags(nr.GetValue(7).ToString());
                        this.changeType = nr.GetValue(8).ToString();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        public string TagsToXml()
        {
            if (tagkey.Count > 0)
            {
                using (MemoryStream fileStream = new MemoryStream())
                {
                    XmlTextWriter textWriter = new XmlTextWriter(fileStream, Encoding.UTF8);
                    textWriter.WriteStartDocument();
                    textWriter.WriteStartElement("waytags");
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
        /// <summary>
        /// 将存在数据库里面的XML格式的tag文件，解析到way元素tagList变量中
        /// </summary>
        /// <param name="tagText"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 将way元素中涉及到的NodeId用字符串方式保存并返回
        /// </summary>
        /// <returns></returns>
        public string NodeIdsToText()
        {
            if (refNodeId.Count > 0)
            {
                string nodeIdText = null;
                for (int i = 0; i < refNodeId.Count; i++)
                {
                    nodeIdText += refNodeId[i].ToString() + ";";
                }
                return nodeIdText;
            }
            else 
            {
                return null;
            }
        }
        /// <summary>
        /// 将数据库中存储的refNodeIds字符串变量解析，赋值到way元素refnodeid对应的List中。
        /// </summary>
        /// <param name="nodeidsText"></param>
        /// <returns></returns>
        public bool TextToNodeIds(string nodeidsText)
        {
            try
            {
                string[] idText = nodeidsText.Split(';');
                for (int i = 0; i < idText.Length; i++)
                {
                    uint temp=0;
                    if(uint.TryParse(idText[i],out temp))
                    {
                        refNodeId.Add(temp);
                    }
                }
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public WAY ToWay(out List<string> rels)
        {
            WAY way = new WAY();
            //int id =Int32.Parse(xr.GetAttribute("id"));
            way.osmid = id.ToString();
            way.changeset = changeset;
            way.timestamp = time;
            way.user = user;
            way.uid = uid.ToString();
            way.version = (short)version;
            way.visible = true;

            string tags = "";
            rels = new List<string>();
            if (refNodeId.Count > 0)
            {
                for (int i = 0; i < refNodeId.Count; i++)
                {
                    rels.Add(refNodeId[i].ToString());
                }
            }

            if (tagkey.Count > 0)
            {
                way.issimple = false;
                for (int i = 0; i < tagkey.Count; i++)
                {
                    string k = tagkey[i];
                    string v = value[i];
                    if (ImportOsc.FClass.Contains(k))
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
                if (tags != "")
                {
                    tags = tags.Remove(tags.Length - 1);
                }
                way.tags = tags;
            }
            else
            {
                way.issimple = true;
            }
            return way;


        }



    }
}
