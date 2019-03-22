using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using GIS.Geometries;
using GIS.UI.Forms;
using Oracle.ManagedDataAccess.Client;

namespace TrustValueAndReputation.historyToDatabase
{
    public class OsmDataRelation
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
        public List<uint> memberNode;
        public List<uint> memberWay;
        public List<uint> memberRelation;
        public List<string> nodeRole;
        public List<string> wayRole;
        public List<string> relationRole;

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
        /// 根据XML节点初始化relation对象
        /// </summary>
        /// <param name="xr"></param>
        /// <returns></returns>
        public bool InitialRelationFromXml(XmlTextReader xr,OracleConnection con)
        {
            new OsmDataRelation();
            tagkey = new List<string>();
            value = new List<string>();
            
            memberNode = new List<uint>();
            memberWay = new List<uint>();
            memberRelation = new List<uint>();
            nodeRole = new List<string>();
            wayRole = new List<string>();
            relationRole = new List<string>();
            List<uint> inners = new List<uint>();
            List<uint> outers = new List<uint>();
            try
            {
                id = uint.Parse(xr.GetAttribute("id"));
                version = int.Parse(xr.GetAttribute("version"));
                time = DateTime.Parse(xr.GetAttribute("timestamp"));
                uid = uint.Parse(xr.GetAttribute("uid"));
                user = xr.GetAttribute("user");
                changeset = xr.GetAttribute("changeset");
                visible = true;
                if (xr.IsEmptyElement == false)
                {
                    while (xr.Read())
                    {
                        if (xr.Name == "tag" && xr.NodeType == XmlNodeType.Element)
                        {
                            string k = xr.GetAttribute("k");
                            string v = xr.GetAttribute("v");
                            tagkey.Add(k);
                            value.Add(v);
                            if (OsmDataNode.FClass.Contains(k))
                            { fc = k; dsg = v; }
                            if (v.Length < 400)
                            {
                                if (k == "name")
                                { name = v; }
                                if (k == "name:zh")
                                { name_zh = v; }
                                if (k == "name:en")
                                { name_en = v; }
                            }
                            if (v == "multipolygon")
                            {
                                isPolygon = true;
                                continue;
                            }
                        }
                        else if (xr.Name == "member" && xr.NodeType == XmlNodeType.Element)
                        {
                            #region 依member属性值类型typec初始化relation中role和ref变量
                            switch (xr.GetAttribute("type"))
                            {
                                case "node":
                                    memberNode.Add(uint.Parse(xr.GetAttribute("ref")));
                                    nodeRole.Add(xr.GetAttribute("role"));
                                    break;
                                case "way":
                                    memberWay.Add(uint.Parse(xr.GetAttribute("ref")));
                                    wayRole.Add(xr.GetAttribute("role"));
                                    if(xr.GetAttribute("role")=="outer")
                                    {
                                        outers.Add(uint.Parse(xr.GetAttribute("ref")));
                                    }
                                    else if (xr.GetAttribute("role") == "inner")
                                    {
                                        inners.Add(uint.Parse(xr.GetAttribute("ref")));
                                    }
                                    break;
                                case "relation":
                                    memberRelation.Add(uint.Parse(xr.GetAttribute("ref")));
                                    relationRole.Add(xr.GetAttribute("role"));
                                    break;
                            }
                            #endregion
                            
                        }
                        else if (xr.Name == "relation" && xr.NodeType == XmlNodeType.EndElement)
                        {
                            break;
                        }
                    }
                    if(isPolygon)
                    {
                        shapewkt=SqlHelper_OSC.BuildRelation(outers,inners,con);
                    }
                }
                return true;
            }
            catch(Exception e)
            {
                
                return false;
            }
        }

        public bool InitialRelationFromNpgsqlReader(OracleDataReader nr)
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
                        DBXmlToMember(nr.GetValue(6).ToString());
                        XmlToTags(nr.GetValue(7).ToString());
                        this.changeType = nr.GetValue(3).ToString();
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
        /// 将tag标记用XML格式文档保存
        /// </summary>
        /// <returns></returns>
        public string TagsToXml()
        {
            if (tagkey.Count > 0)
            {
                using (MemoryStream fileStream = new MemoryStream())
                {
                    XmlTextWriter textWriter = new XmlTextWriter(fileStream, Encoding.UTF8);
                    textWriter.WriteStartDocument();
                    textWriter.WriteStartElement("relationtags");
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
        /// <summary>
        /// 将way元素中涉及到的NodeId用字符串方式保存并返回
        /// </summary>
        /// <returns></returns>
        public string MembersToXml()
        {
            if (memberNode.Count() + memberWay.Count() + memberRelation .Count() > 0)
            {
               
                using (MemoryStream fileStream = new MemoryStream())
                {
                    XmlTextWriter textWriter = new XmlTextWriter(fileStream, Encoding.UTF8);
                    textWriter.WriteStartDocument();
                    textWriter.WriteStartElement("members");
                    if (memberNode.Count > 0)
                    {
                        for (int i = 0; i < this.memberNode.Count; i++)
                        {
                            textWriter.WriteStartElement("member");
                            textWriter.WriteAttributeString("type", "node");
                            textWriter.WriteAttributeString("ref", memberNode[i].ToString());
                            textWriter.WriteAttributeString("role", nodeRole[i]);
                            textWriter.WriteEndElement();
                        }
                    }
                    else if (memberWay.Count > 0)
                    {
                        for (int i = 0; i < this.memberWay.Count; i++)
                        {
                            textWriter.WriteStartElement("member");
                            textWriter.WriteAttributeString("type", "way");
                            textWriter.WriteAttributeString("ref", memberWay[i].ToString());
                            textWriter.WriteAttributeString("role", wayRole[i]);
                        }
                    }
                    else if (memberRelation.Count > 0)
                    {
                        for (int i = 0; i < this.memberRelation.Count; i++)
                        {
                            textWriter.WriteStartElement("member");
                            textWriter.WriteAttributeString("type", "relation");
                            textWriter.WriteAttributeString("ref", memberRelation[i].ToString());
                            textWriter.WriteAttributeString("role", relationRole[i]);
                        }
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
        public bool DBXmlToMember(string xmlText)
        {
            try
            {
                XmlTextReader reader = new XmlTextReader(xmlText, XmlNodeType.Element, null);
                while (reader.Read())
                {
                    if (reader.Name == "member" && reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.GetAttribute("type"))
                        {
                            case "node":
                                memberNode.Add(uint.Parse(reader.GetAttribute("ref")));
                                nodeRole.Add(reader.GetAttribute("role"));
                                break;
                            case "way":
                                memberWay.Add(uint.Parse(reader.GetAttribute("ref")));
                                wayRole.Add(reader.GetAttribute("role"));
                                break;
                            case "relation":
                                memberRelation.Add(uint.Parse(reader.GetAttribute("ref")));
                                relationRole.Add(reader.GetAttribute("role"));
                                break;
                            default: break;
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

        public RELATION ToRelation(out bool isPolygon,out List<string> outers, out List<string> inners) 
        {

            RELATION rela = new RELATION();
            //int id =Int32.Parse(xr.GetAttribute("id"));
            rela.osmid = id.ToString();
            rela.changeset = changeset;
            rela.timestamp = time;
            rela.user = user;
            rela.uid = uid.ToString();
            rela.version = (short)version;
            rela.visible = true;

            outers = new List<string>();
            inners = new List<string>();
           isPolygon = false;
            string tags = null; ;
            if(tagkey.Count>0)
            {
                rela.issimple = false;
                for (int i = 0; i < tagkey.Count; i++)
                {
                    string k = tagkey[i];
                    string v = value[i];
                    if (ImportOsc.FClass.Contains(k))
                    {
                        rela.fc = k;
                        rela.dsg = v;
                        continue;
                    }
                    if (k == "name")
                    {
                        rela.name = v;
                        continue;
                    }
                    if (k == "name:zh")
                    {
                        rela.name_zh = v;
                        continue;
                    }
                    if (k == "name:en")
                    {
                        rela.name_en = v;
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
            else
            {
                rela.issimple = true;
            }
            if (memberWay.Count > 0)
            {
                for (int i = 0; i < memberWay.Count; i++)
                {
                    if (wayRole[i] == "inner")
                    {
                        inners.Add(wayRole[i]);
                    }
                    else if (wayRole[i] == "outer")
                    {
                        outers.Add(wayRole[i]);
                    }
                }
            }
            return rela;

        }
       
    }
}
