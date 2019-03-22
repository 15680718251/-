using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GIS.UI.Entity
{
   public  class Layers
    {

       //osmTralCreateTableSqlPath
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
           catch (Exception ex)
           {
               Console.WriteLine(ex.ToString());
               return false;
           }
       }
    }
}
