using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.UI.AdditionalTool;
using GIS.UI.ModelTranByOSM;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Forms;
using System.Data;


//**********by dy2018079 ***********
namespace GIS.UI.ModelTranByOSM
{
   public class TEModelTrans
    {
        public enum geoTypes { Area = 0, Way = 1, Node = 2 };
        string rTableName = "type_rule";
        string lTableName = "land_type";
        string tableName = "";
        OracleDBHelper helper = new OracleDBHelper();
       public TEModelTrans() { }
       public TEModelTrans(string rTableName,string tableName)
       {
           this.rTableName = rTableName;
           this.tableName = tableName;
       }
       //委托
       public delegate void dModelTransProgress(int layerIndex, int layerCount, string layerName, int maximum, int current);
       public event dModelTransProgress onModelTransProgress;

       public void start()
       {
           //模型转换主函数入口 
           string[] layerNames = helper.getColumnData(lTableName,"elementtype",false,"elementtypeid");
           TypicalElementTableHelper.CreateTables(tableName);
           switch (tableName)
           {
               /*分点线面进行相应的模型转换 by zbl 20181010*/
               case "APOLY": process(layerNames, geoTypes.Area); break;
               case "ALINE": ProcessLine(layerNames, geoTypes.Way); break; 
               case "APOINT": ProcessPoint(layerNames, geoTypes.Node); break; 
               case "OSCAREA": process(layerNames, geoTypes.Area); break;
               case "OSCLINE": ProcessLine(layerNames, geoTypes.Way); break; 
               case "OSCPOINT": ProcessPoint(layerNames, geoTypes.Node); break;
               default: break;
           }
  
       }

       private void process(string[] layerNames, geoTypes geoType)
       {
           for (int i = 0; i < layerNames.Count(); i++)
           {
               if (tableName == "APOLY")
               {
                   createIndex(layerNames[i],geoType);//创建索引
               }
               List<RuleItem> rules = getRules(rTableName,i+1,geoType);
               if (rules == null)
               { continue; }
               for (int j = 0; j < rules.Count; j++)
               {
                   onModelTransProgress(i, layerNames.Length, layerNames[i], rules.Count - 1, j);
                   insertData(helper, rules[j], layerNames[i], geoType,tableName);                   
               }
               //updateObjectId(tableName,layerNames[i]);
           }
           //helper.dispose();
           onModelTransProgress(layerNames.Length, layerNames.Length, layerNames[layerNames.Length - 1], 100, 0);        
       }

       #region 模型转换基态和增量线数据、点数据 by zbl 20181010
       /// <summary>
       /// 模型转换处理线数据 by zbl 20181010
       /// </summary>
       /// <param name="layerNames">线图层名</param>
       /// <param name="geoType">线几何</param>
       private void ProcessLine(string[] layerNames, geoTypes geoType)
       {
           for (int i = 0; i < layerNames.Count(); i++)
           {
               if (tableName == "ALINE")// 修改 by dy20181029
               {
                   //createLineIndex(layerNames[i]); //创建基态数据表索引
                   createIndex(layerNames[i], geoType);
               }
               else
               {
                   createNewLineIndex(layerNames[i]);//创建增量数据表索引 
               }
                           
               List<RuleItem> rules = getRules(rTableName,i+1,geoType);
               if (rules == null)
               { continue; }
               for (int j = 0; j < rules.Count; j++)
               {
                   onModelTransProgress(i, layerNames.Length, layerNames[i], rules.Count - 1, j);
                   insertData(helper, rules[j], layerNames[i], geoType, tableName);// 修改 by dy20181029
                   //insertLineData(helper, rules[j], layerNames[i], geoType,tableName);                   
               }
           }
           onModelTransProgress(layerNames.Length, layerNames.Length, layerNames[layerNames.Length - 1], 100, 0); 
           
       }
       /// <summary>
       /// 插入线要素数据到对应的数据表中
       /// </summary>
       /// <param name="odh">数据库帮助对象</param>
       /// <param name="ri">规则集对象</param>
       /// <param name="layerName">线图层名</param>
       /// <param name="geoType">线几何</param>
       /// <param name="tableName">数据表名</param>
       //private void insertLineData(OracleDBHelper odh, RuleItem ri, string layerName, geoTypes geoType, string tableName)
       //{
       //    string insertSql = "";
       //    if (geoType == geoTypes.Way )
       //    {
       //        if (tableName == "ALINE")
       //        {
       //            insertSql = string.Format("insert into {0}_LINE (nationcode,nationelename,osmid,versionid,starttime,changeset,userid,username,fc,dsg,tags,trustvalue,shape,source)(select type_rule.nationcode,type_rule.nationelename,aline.osmid,aline.versionid,aline.starttime,aline.changeset,aline.userid,aline.username,aline.fc,aline.dsg,aline.tags,aline.trustvalue,aline.shape,aline.source from type_rule,aline where type_rule.osm_key='{1}' and type_rule.osm_value='{2}' and aline.fc ='{1}' and aline.dsg='{2}' and type_rule.geometry='Way')", layerName, ri.Osm_key, ri.Osm_value);
       //        }
       //        else if (tableName == "OSCLINE")
       //        {
       //            insertSql = string.Format("insert into {0}_NEWLINE (nationcode,nationelename,osmid,versionid,starttime,changeset,userid,username,fc,dsg,tags,trustvalue,shape,changetype,source)(select type_rule.nationcode,type_rule.nationelename,oscline.osmid,oscline.versionid,oscline.starttime,oscline.changeset,oscline.userid,oscline.username,oscline.fc,oscline.dsg,oscline.tags,oscline.trustvalue,oscline.shape,oscline.changetype,oscline.source from type_rule,oscline where type_rule.osm_key=oscline.fc and type_rule.osm_value=oscline.dsg and oscline.fc ='{1}' and oscline.dsg='{2}'and type_rule.geometry='Way')", layerName, ri.Osm_key, ri.Osm_value);
       //        }
       //        else
       //        {
       //            MessageBox.Show("请选择正确的表名！");
       //        }
   
       //    }
          
       //    odh.sqlExecuteUnClose(insertSql);
       //}
       /// <summary>
       /// 创建线要素增量数据表的shape字段索引
       /// </summary>
       /// <param name="layerName"></param>
       public void createNewLineIndex(string layerName)
       {
           string sqlInset = "";
           string sqlCreate = "";
           if (helper.IsExistSpatialTable(layerName + "_NEWLINE"))
           {
               sqlCreate = string.Format("create index idx_{0}_NEWLINE on {0}_NEWLINE(shape) indextype is mdsys.spatial_index", layerName);
           }
           else
           {
               sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('{0}_NEWLINE', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", layerName);
               helper.sqlExecuteUnClose(sqlInset);
               sqlCreate = string.Format("create index idx_{0}_NEWLINE on {0}_NEWLINE(shape) indextype is mdsys.spatial_index", layerName);
           }
           helper.sqlExecuteUnClose(sqlCreate);
       }
       /// <summary>
       /// 创建线要素基态数据表的shape字段索引
       /// </summary>
       /// <param name="layerName"></param>
       //public void createLineIndex(string layerName)
       //{
       //    string sqlInset = "";
       //    string sqlCreate = "";
       //    if (helper.IsExistSpatialTable(layerName + "_LINE"))
       //    {
       //        sqlCreate = string.Format("create index idx_{0}_LINE on {0}_LINE(shape) indextype is mdsys.spatial_index", layerName);
       //    }
       //    else
       //    {
       //        sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('{0}_LINE', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", layerName);
       //        helper.sqlExecuteUnClose(sqlInset);
       //        sqlCreate = string.Format("create index idx_{0}_LINE on {0}_LINE(shape) indextype is mdsys.spatial_index", layerName);
       //    }
       //    helper.sqlExecuteUnClose(sqlCreate);
       //}
       /// <summary>
       /// 模型转换处理线数据 by zbl 20181010
       /// </summary>
       /// <param name="layerNames">点图层名</param>
       /// <param name="geoType">点几何</param>
       private void ProcessPoint(string[] layerNames, geoTypes geoType)
       {
           for (int i = 0; i < layerNames.Count(); i++)
           {
               if (tableName == "OSCPOINT")
               {
                   //createPointIndex(layerNames[i]);//创建索引
                   createIndex(layerNames[i], geoType); // 修改 by dy20181029
               }
               List<RuleItem> rules = getRules(rTableName, i + 1, geoType);
               if (rules == null)
               { continue; }
               for (int j = 0; j < rules.Count; j++)
               {
                   onModelTransProgress(i, layerNames.Length, layerNames[i], rules.Count - 1, j);
                   insertData(helper, rules[j], layerNames[i], geoType, tableName); // 修改 by dy20181029
                   //insertPointData(helper, rules[j], layerNames[i], geoType, tableName);
               }
           }
           onModelTransProgress(layerNames.Length, layerNames.Length, layerNames[layerNames.Length - 1], 100, 0);

       }
       /// <summary>
       /// 插入点要素数据到对应的数据表中
       /// </summary>
       /// <param name="odh">数据库帮助对象</param>
       /// <param name="ri">规则集对象</param>
       /// <param name="layerName">点图层名</param>
       /// <param name="geoType">点几何</param>
       /// <param name="tableName">数据表名</param>
       //private void insertPointData(OracleDBHelper odh, RuleItem ri, string layerName, geoTypes geoType, string tableName)
       //{
       //    string insertSql = "";
       //    if (geoType == geoTypes.Node )
       //    {
       //        if (tableName == "APOINT")
       //        {
       //            insertSql = string.Format("insert into {0}_POINT(nationcode,nationelename,osmid,versionid,lat,lon,starttime,changeset,userid,username,fc,dsg,tags,shape,source)(select type_rule.nationcode,type_rule.nationelename,apoint.osmid,apoint.versionid,apoint.lat,apoint.lon,apoint.starttime,apoint.changeset,apoint.userid,apoint.username,apoint.fc,apoint.dsg,apoint.tags,apoint.shape,apoint.source from type_rule,apoint where type_rule.osm_key='{1}' and type_rule.osm_value='{2}' and apoint.fc ='{1}' and apoint.dsg='{2}' and type_rule.geometry='Node')", layerName, ri.Osm_key, ri.Osm_value);
       //        }
       //        else if (tableName == "OSCPOINT")
       //        {
       //            insertSql = string.Format("insert into {0}_NEWPOINT (nationcode,nationelename,osmid,versionid,lat,lon,starttime,changeset,userid,username,fc,dsg,tags,shape,changetype,source)(select type_rule.nationcode,type_rule.nationelename,oscpoint.osmid,oscpoint.versionid,oscpoint.lat,oscpoint.lon,oscpoint.starttime,oscpoint.changeset,oscpoint.userid,oscpoint.username,oscpoint.fc,oscpoint.dsg,oscpoint.tags,oscpoint.shape,oscpoint.changetype,oscpoint.source from type_rule,oscpoint where type_rule.osm_key=oscpoint.fc and type_rule.osm_value=oscpoint.dsg and oscpoint.fc ='{1}' and oscpoint.dsg='{2}' and type_rule.geometry='Node')", layerName, ri.Osm_key, ri.Osm_value);
       //        }
       //        else
       //        {
       //            MessageBox.Show("请选择正确的表名！");
       //        }

       //    }

       //    odh.sqlExecuteUnClose(insertSql);
       //}
       /// <summary>
       /// 创建点要素数据表的shape字段索引
       /// </summary>
       /// <param name="layerName"></param>
       //public void createPointIndex(string layerName)
       //{
       //    string sqlInset = "";
       //    string sqlCreate = "";
       //    if (helper.IsExistSpatialTable(layerName + "_NEWPOINT"))
       //    {
       //        sqlCreate = string.Format("create index idx_{0}_NEWPOINT on {0}_NEWPOINT(shape) indextype is mdsys.spatial_index", layerName);
       //    }
       //    else
       //    {
       //        sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('{0}_NEWPOINT', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", layerName);
       //        helper.sqlExecuteUnClose(sqlInset);
       //        sqlCreate = string.Format("create index idx_{0}_NEWPOINT on {0}_NEWPOINT(shape) indextype is mdsys.spatial_index", layerName);
       //    }
       //    helper.sqlExecuteUnClose(sqlCreate);
       //}
       #endregion

       private List<RuleItem> getRules(string tableName, int layerIndex, geoTypes geoType)
       {
           //int objectid = 1;
           string sql = "";
           if (geoType == geoTypes.Area)
           {
               sql = string.Format("select osm_key,osm_value,nationcode,nationelename,geometry from {0} where elementtype={1} and geometry='Area'", tableName, layerIndex);
           }
           else if (geoType == geoTypes.Way)
           {
               sql = string.Format("select osm_key,osm_value,nationcode,nationelename,geometry from {0} where elementtype={1} and geometry='Way'", tableName, layerIndex);
           }
           else
           {
               sql = string.Format("select osm_key,osm_value,nationcode,nationelename,geometry from {0} where elementtype={1} and geometry='Node'", tableName, layerIndex);
           }
           List<RuleItem> rList = null;
           using (OracleDataReader dr = helper.queryReader(sql))
           {
               try
               {
                   if (dr == null)
                   {
                       return null;
                   }
                   if (dr.HasRows)
                   {
                       rList = new List<RuleItem>();
                       while (dr.Read())
                       {
                           
                           RuleItem ri = new RuleItem(dr.GetValue(0).ToString(),dr.GetValue(1).ToString(),dr.GetValue(2).ToString(),dr.GetValue(3).ToString());
                           rList.Add(ri);
                           //objectid++;
                          
                       }
                       dr.Dispose();
                       dr.Close();
                       return rList;
                   }
               }
               finally { helper.close(); }
 
           }
           return null;
       }
       /// <summary>
       /// 将数据插入到不同的数据表
       /// </summary>
       /// <param name="odh"></param>
       /// <param name="ri"></param>
       /// <param name="layerName"></param>
       /// <param name="geoType"></param>
       /// <param name="tableName"></param>
       private void insertData(OracleDBHelper odh, RuleItem ri, string layerName, geoTypes geoType,string tableName)
       {
           //int objectid = 1;
           string insertSql = "";
           if (geoType == geoTypes.Area)
           {
               if (tableName == "APOLY")
               {
                   insertSql = string.Format("insert into {0}_AREA (nationcode,nationelename,osmid,versionid,starttime,changeset,userid,username,fc,dsg,tags,trustvalue,shape,source,pointsid)(select type_rule.nationcode,type_rule.nationelename,apoly.osmid,apoly.versionid,apoly.starttime,apoly.changeset,apoly.userid,apoly.username,apoly.fc,apoly.dsg,apoly.tags,apoly.trustvalue,apoly.shape,apoly.source,apoly.pointsid from type_rule,apoly where type_rule.osm_key='{1}' and type_rule.osm_value='{2}' and apoly.fc ='{1}' and apoly.dsg='{2}' and type_rule.geometry='Area')", layerName, ri.Osm_key, ri.Osm_value);
               }
               else if (tableName == "OSCAREA")
               {
                   insertSql = string.Format("insert into {0}_NEWAREA (nationcode,nationelename,osmid,versionid,starttime,changeset,userid,username,fc,dsg,tags,trustvalue,shape,changetype,source,pointsid)(select type_rule.nationcode,type_rule.nationelename,oscarea.osmid,oscarea.versionid,oscarea.starttime,oscarea.changeset,oscarea.userid,oscarea.username,oscarea.fc,oscarea.dsg,oscarea.tags,oscarea.trustvalue,oscarea.shape,oscarea.changetype,oscarea.source,oscarea.pointsid from type_rule,oscarea where type_rule.osm_key=oscarea.fc and type_rule.osm_value=oscarea.dsg and oscarea.fc ='{1}' and oscarea.dsg='{2}' and type_rule.geometry='Area')", layerName, ri.Osm_key, ri.Osm_value);
               }
               else
               {
                   MessageBox.Show("请选择正确的表名！");

               }
           }
           //insertSql = string.Format("insert into A{0} (nationcode,nationelename,osmid,version,timestamp,changeset,userid,username,fc,dsg,tags,trustvalue,pointsId,shape)(select nationcode,nationelename,osmid,version,timestamp,changeset,userid,username,fc,dsg,tags,trustvalue,pointsId,shape from apoly where fc='{3}'and dsg='{4}')", layerName, ri.NationCode, ri.NationEleName, ri.Osm_key, ri.Osm_value);

           else if (geoType == geoTypes.Way)
           {
               if (tableName == "ALINE")
               {
                   insertSql = string.Format("insert into {0}_LINE (nationcode,nationelename,osmid,versionid,starttime,changeset,userid,username,fc,dsg,tags,trustvalue,shape,source,pointsid)(select type_rule.nationcode,type_rule.nationelename,aline.osmid,aline.versionid,aline.starttime,aline.changeset,aline.userid,aline.username,aline.fc,aline.dsg,aline.tags,aline.trustvalue,aline.shape,aline.source,aline.pointsid from type_rule,aline where type_rule.osm_key='{1}' and type_rule.osm_value='{2}' and aline.fc ='{1}' and aline.dsg='{2}' and type_rule.geometry='Way')", layerName, ri.Osm_key, ri.Osm_value);
               }
               else if (tableName == "OSCLINE")
               {
                   insertSql = string.Format("insert into {0}_NEWLINE (nationcode,nationelename,osmid,versionid,starttime,changeset,userid,username,fc,dsg,tags,trustvalue,shape,changetype,source,pointsid)(select type_rule.nationcode,type_rule.nationelename,oscline.osmid,oscline.versionid,oscline.starttime,oscline.changeset,oscline.userid,oscline.username,oscline.fc,oscline.dsg,oscline.tags,oscline.trustvalue,oscline.shape,oscline.changetype,oscline.source,oscline.pointsid from type_rule,oscline where type_rule.osm_key=oscline.fc and type_rule.osm_value=oscline.dsg and oscline.fc ='{1}' and oscline.dsg='{2}'and type_rule.geometry='Way')", layerName, ri.Osm_key, ri.Osm_value);
               }
               else
               {
                   MessageBox.Show("请选择正确的表名！");
               }
           }
           else
           {
               if (tableName == "APOINT")
               {
                   insertSql = string.Format("insert into {0}_POINT(nationcode,nationelename,osmid,versionid,lat,lon,starttime,changeset,userid,username,fc,dsg,tags,shape,source)(select type_rule.nationcode,type_rule.nationelename,apoint.osmid,apoint.versionid,apoint.lat,apoint.lon,apoint.starttime,apoint.changeset,apoint.userid,apoint.username,apoint.fc,apoint.dsg,apoint.tags,apoint.shape,apoint.source from type_rule,apoint where type_rule.osm_key='{1}' and type_rule.osm_value='{2}' and apoint.fc ='{1}' and apoint.dsg='{2}' and type_rule.geometry='Node')", layerName, ri.Osm_key, ri.Osm_value);
               }
               else if (tableName == "OSCPOINT")
               {
                   insertSql = string.Format("insert into {0}_NEWPOINT (nationcode,nationelename,osmid,versionid,lat,lon,starttime,changeset,userid,username,fc,dsg,tags,shape,changetype,source)(select type_rule.nationcode,type_rule.nationelename,oscpoint.osmid,oscpoint.versionid,oscpoint.lat,oscpoint.lon,oscpoint.starttime,oscpoint.changeset,oscpoint.userid,oscpoint.username,oscpoint.fc,oscpoint.dsg,oscpoint.tags,oscpoint.shape,oscpoint.changetype,oscpoint.source from type_rule,oscpoint where type_rule.osm_key=oscpoint.fc and type_rule.osm_value=oscpoint.dsg and oscpoint.fc ='{1}' and oscpoint.dsg='{2}' and type_rule.geometry='Node')", layerName, ri.Osm_key, ri.Osm_value);
               }
               else
               {
                   MessageBox.Show("请选择正确的表名！");
               }
           }
           odh.sqlExecuteUnClose(insertSql);

       }

       //public void updateObjectId(string tableName,string layerName)
       //{
       //    string sql = "";
       //    string sql1 = "";
       //    if (tableName == "APOLY")
       //    {
       //        sql = string.Format("update {0}_AREA set objectId=rownum", layerName);
       //        sql1 = string.Format("alter table {0}_AREA add primary key(objectId)", layerName);           
       //    }
       //    else if (tableName == "OSCAREA")
       //    {
       //        sql = string.Format("update {0}_NEWAREA set objectId=rownum", layerName);
       //        sql1 = string.Format("alter table {0}_NEWAREA add primary key(objectId)", layerName); 
       //    }
       //    helper.sqlExecuteUnClose(sql);
       //    helper.sqlExecuteUnClose(sql1);
         
       //}
       /// <summary>
       /// 创建索引
       /// </summary>
       /// <param name="layerName"></param>
       /// <param name="geoType"></param>
       public void createIndex(string layerName,geoTypes geoType)
       {
           string sqlInset = "";
           string sqlCreate = "";
           if (geoType == geoTypes.Area)
           {
               if (helper.IsExistSpatialTable(layerName + "_AREA"))
               {
                   //sqlCreate = string.Format("create index idx_osc{0} on OSC{0}(shape) indextype is mdsys.spatial_index", layerName);
                   sqlCreate = string.Format("create index idx_{0}_AREA on {0}_AREA(shape) indextype is mdsys.spatial_index", layerName);
               }
               else
               {
                   //sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('OSC{0}', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", layerName);
                   sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('{0}_AREA', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", layerName);
                   helper.sqlExecuteUnClose(sqlInset);
                   //sqlCreate = string.Format("create index idx_osc{0} on OSC{0}(shape) indextype is mdsys.spatial_index", layerName);
                   sqlCreate = string.Format("create index idx_{0}_AREA on {0}_AREA(shape) indextype is mdsys.spatial_index", layerName);

               }
           }
           else if (geoType == geoTypes.Way)
           {
               if (helper.IsExistSpatialTable(layerName + "_LINE"))
               {
                   sqlCreate = string.Format("create index idx_{0}_LINE on {0}_LINE(shape) indextype is mdsys.spatial_index", layerName);
               }
               else
               {
                   sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('{0}_LINE', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", layerName);
                   helper.sqlExecuteUnClose(sqlInset);
                   sqlCreate = string.Format("create index idx_{0}_LINE on {0}_LINE(shape) indextype is mdsys.spatial_index", layerName);
               }
           }
           else {
               if (helper.IsExistSpatialTable(layerName + "_NEWPOINT"))
               {
                   sqlCreate = string.Format("create index idx_{0}_NEWPOINT on {0}_NEWPOINT(shape) indextype is mdsys.spatial_index", layerName);
               }
               else
               {
                   sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('{0}_NEWPOINT', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", layerName);
                   helper.sqlExecuteUnClose(sqlInset);
                   sqlCreate = string.Format("create index idx_{0}_NEWPOINT on {0}_NEWPOINT(shape) indextype is mdsys.spatial_index", layerName);
               }
           }
           helper.sqlExecuteUnClose(sqlCreate);
          
           
       }

       //public void createIndex(string tableName,string layerName)
       //{
       //    string sql = "select TABLE_NAME from USER_SDO_GEOM_METADATA";
       //    string sql1 = "";
       //    string sqlInset = "";
       //    string sqlCreate = "";
       //    List<string> tableNames = new List<string>();
       //    OracleConnection con = helper.getOracleConnection();
       //    using (OracleCommand cmd = new OracleCommand(sql, con))
       //    {
       //        if (con.State == ConnectionState.Closed)
       //        {
       //            con.Open();
       //        }
       //        using (OracleDataReader dr = cmd.ExecuteReader())
       //        {
       //            while (dr.Read())
       //            {
       //                tableNames.Add(dr[0].ToString());
       //            }
       //        }
       //        for (int i = 0; i < tableNames.Count; i++)
       //        {
       //            if (tableName == "APOLY")
       //            {
       //                sql1 = string.Format("delete from USER_SDO_GEOM_METADATA where TABLE_NAME='A{0}'", layerName);
       //            }
       //            else if (tableName == "OSCAREA")
       //            {
       //                sql1 = string.Format("delete from USER_SDO_GEOM_METADATA where TABLE_NAME='OSC{0}'", layerName);
       //            }
       //            else { continue; }              
       //            helper.sqlExecuteUnClose(sql1);
       //        }                 
       //    }
       //    if (tableName == "APOLY")
       //    {
       //        sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('A{0}', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", layerName);
       //        sqlCreate = string.Format("create index idx_a{0} on A{0}(shape) indextype is mdsys.spatial_index", layerName);
       //    }
       //    else if (tableName == "OSCAREA")
       //    {
       //        sqlInset = string.Format("INSERT INTO USER_SDO_GEOM_METADATA (TABLE_NAME, COLUMN_NAME, DIMINFO, SRID)VALUES ('OSC{0}', 'shape', MDSYS.SDO_DIM_ARRAY (MDSYS.SDO_DIM_ELEMENT('X', -5000000, -5000000, 0.000000050), MDSYS.SDO_DIM_ELEMENT('Y', -5000000, -5000000, 0.000000050) ),4326)", layerName);
       //        sqlCreate = string.Format("create index idx_osc{0} on OSC{0}(shape) indextype is mdsys.spatial_index", layerName);
       //    }
       //    else { MessageBox.Show("请选择正确的表"); }
 
       //    helper.sqlExecuteUnClose(sqlInset);
       //    helper.sqlExecuteUnClose(sqlCreate);

       //}
    }
}
