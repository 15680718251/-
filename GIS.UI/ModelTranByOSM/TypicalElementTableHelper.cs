using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.UI.AdditionalTool;
using Oracle.ManagedDataAccess.Client;

namespace GIS.UI.ModelTranByOSM
{
    //*************by dy20180711*********
   public class TypicalElementTableHelper
    {
       private static string Typical_Element_Table_Name = "land_type";
       private static string Typical_Element_Rule_Table_Name = "type_rule";
       
       /// <summary>
       /// 建表
       /// </summary>
       public static void CreateTables(string tablename)
        {
            OracleDBHelper helper = new OracleDBHelper();
            string[] tableNames = helper.getColumnData(Typical_Element_Table_Name, "elementtype", false);
            string[] ids = helper.getColumnData(Typical_Element_Table_Name, "elementtypeid", false);
            for ( int i = 0; i < tableNames.Length; i++)
            {
                string tableName = "";
                string createTableSql = "";
                if (tablename == "APOLY")
                {
                    tableName =(tableNames[i]+"_AREA").ToLower();//基态面要素数据表
                    createTableSql = "(objectId NUMBER(30)  primary key NOT NULL,nationcode NUMBER(30),nationelename VARCHAR2(50),osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,shape SDO_GEOMETRY,source NUMBER(30),matchid NUMBER(30),updatestate NUMBER(30),pointsid VARCHAR2(4000))";
                    helper.createTable(tableName, createTableSql);

                    if (i == 0 && (helper.IsExistTable("HISTORYTRAFFIC_AREA") || helper.IsExistTable("HISTORYWATER_AREA") || helper.IsExistTable("HISTORYRESIDENTIAL_AREA") || helper.IsExistTable("HISTORYVEGETATION_AREA") || helper.IsExistTable("HISTORYSOIL_AREA")))
                    {

                    }
                    else if (helper.IsExistTable("HISTORYTRAFFIC_AREA") == false || helper.IsExistTable("HISTORYWATER_AREA") == false || helper.IsExistTable("HISTORYRESIDENTIAL_AREA") == false || helper.IsExistTable("HISTORYVEGETATION_AREA") == false || helper.IsExistTable("HISTORYSOIL_AREA") == false)
                    {
                        tableName = ("HISTORY" + tableNames[i] + "_AREA").ToLower();//历史面要素数据表   by zbl 20181026
                        createTableSql = "(objectId NUMBER(30)  primary key NOT NULL,nationcode NUMBER(30),nationelename VARCHAR2(50),osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,shape SDO_GEOMETRY,source NUMBER(30),matchid NUMBER(30),updatestate NUMBER(30),pointsid VARCHAR2(4000))";
                        helper.createHistoryTable(tableName, createTableSql);
                    }
                }
                else if (tablename=="OSCAREA")
                {
                    tableName = (tableNames[i] + "_NEWAREA").ToLower();//增量面要素数据表
                    createTableSql = "(objectId NUMBER(30) primary key NOT NULL,nationcode NUMBER(30),nationelename VARCHAR2(50),osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,shape SDO_GEOMETRY,changetype VARCHAR2(50),source NUMBER(30),matchid NUMBER(30),pointsid VARCHAR2(4000))";
                    helper.createTable(tableName, createTableSql);
                }

                #region 创建模型转换后的基态和增量要素线数据表、点数据表 by zbl 20181010
                if (tablename == "ALINE")
                {
                    tableName = (tableNames[i] + "_LINE").ToLower();//基态线要素数据表
                    createTableSql = "(objectId NUMBER(30)  primary key NOT NULL,nationcode NUMBER(30),nationelename VARCHAR2(50),osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,shape SDO_GEOMETRY,source NUMBER(30),matchid NUMBER(30),updatestate NUMBER(30),pointsid VARCHAR2(4000))";
                    helper.createTable(tableName, createTableSql);

                    if (i == 0 && (helper.IsExistTable("TRAFFIC_HISTORYLINE") || helper.IsExistTable("WATER_HISTORYLINE") || helper.IsExistTable("RESIDENTIAL_HISTORYLINE") || helper.IsExistTable("VEGETATION_HISTORYLINE") || helper.IsExistTable("SOIL_HISTORYLINE")))
                    {

                    }
                    else if (helper.IsExistTable("TRAFFIC_HISTORYLINE") == false || helper.IsExistTable("WATER_HISTORYLINE") == false || helper.IsExistTable("RESIDENTIAL_HISTORYLINE") == false || helper.IsExistTable("VEGETATION_HISTORYLINE") == false || helper.IsExistTable("SOIL_HISTORYLINE") == false)
                    {
                        tableName = (tableNames[i] + "_HISTORYLINE").ToLower();//历史线要素数据表 by zbl 20181026
                        createTableSql = "(objectId NUMBER(30)  primary key NOT NULL,nationcode NUMBER(30),nationelename VARCHAR2(50),osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,shape SDO_GEOMETRY,source NUMBER(30),matchid NUMBER(30),updatestate NUMBER(30),pointsid VARCHAR2(4000))";
                        helper.createHistoryTable(tableName, createTableSql);
                    }
                }
                else if (tablename == "OSCLINE")
                {
                    tableName = (tableNames[i] + "_NEWLINE").ToLower();//增量线要素数据表
                    createTableSql = "(objectId NUMBER(30) primary key NOT NULL,nationcode NUMBER(30),nationelename VARCHAR2(50),osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,shape SDO_GEOMETRY,changetype VARCHAR2(50),source NUMBER(30),matchid NUMBER(30),pointsid VARCHAR2(4000))";
                    helper.createTable(tableName, createTableSql);
                }

                if (tablename == "APOINT")
                { 
                    tableName = (tableNames[i] + "_POINT").ToLower();//基态点要素数据表
                    createTableSql = "(objectid NUMBER(30) primary key NOT NULL,nationcode NUMBER(30),nationelename VARCHAR2(50),osmid NUMBER(30),lat FLOAT,lon FLOAT,versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),shape SDO_GEOMETRY,source NUMBER(4),matchid NUMBER(30),updatestate NUMBER(30))";
                    helper.createTable(tableName, createTableSql);
                    if (i == 0 && (helper.IsExistTable("TRAFFIC_HISTORYPOINT") || helper.IsExistTable("WATER_HISTORYPOINT") || helper.IsExistTable("RESIDENTIAL_HISTORYPOINT") || helper.IsExistTable("VEGETATION_HISTORYPOINT") || helper.IsExistTable("SOIL_HISTORYPOINT")))
                    {

                    }
                    else if (helper.IsExistTable("TRAFFIC_HISTORYPOINT") == false || helper.IsExistTable("WATER_HISTORYPOINT") == false || helper.IsExistTable("RESIDENTIAL_HISTORYPOINT") == false || helper.IsExistTable("VEGETATION_HISTORYPOINT") == false || helper.IsExistTable("SOIL_HISTORYPOINT") == false)
                    {
                        tableName = (tableNames[i] + "_HISTORYPOINT").ToLower();//历史点要素数据表 by zbl 20181026
                        createTableSql = "(objectId NUMBER(30)  primary key NOT NULL,nationcode NUMBER(30),nationelename VARCHAR2(50),osmid NUMBER(30),versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),trustvalue FLOAT,userreputation FLOAT,shape SDO_GEOMETRY,source NUMBER(30),matchid NUMBER(30),updatestate NUMBER(30))";
                        helper.createHistoryTable(tableName, createTableSql);
                    }
                }
                else if (tablename == "OSCPOINT")
                {
                    tableName = (tableNames[i] + "_NEWPOINT").ToLower();//增量点要素数据表
                    createTableSql = "(objectid NUMBER(30) primary key NOT NULL,nationcode NUMBER(30),nationelename VARCHAR2(50),osmid NUMBER(30),lat FLOAT,lon FLOAT,versionid NUMBER(10),starttime VARCHAR2(50),endtime VARCHAR2(50),changeset VARCHAR2(500),userid NUMBER(30),username VARCHAR2(30),fc VARCHAR2(50),dsg VARCHAR2(50),tags VARCHAR2(2000),shape SDO_GEOMETRY,changetype VARCHAR2(50),source NUMBER(30),matchid NUMBER(30))";
                    helper.createTable(tableName, createTableSql);
                }
                #endregion

            }

        }
/// <summary>
/// 删除数据库中所有表格
/// </summary>
/// <param name="conStr"></param>
       public static void deleteAllTable( )
       {
           OracleDBHelper odh = new OracleDBHelper();
           string[] layerNames = odh.getColumnData(Typical_Element_Table_Name,"name",false,"id");
           for (int i = 0; i < layerNames.Length; i++)
           {
               string layerName = layerNames[i].ToLower();
               string sql = string.Format("drop table {0}_AREA;", layerName);
               if (odh.IsExistTable( layerName+"_AREA"))
               {
                   odh.sqlExecuteUnClose(sql);
               }
               if (odh.IsExistTable(layerName + "_NEWAREA"))
               {
                   sql = string.Format("drop table {0}_NEWAREA;", layerName);
                   odh.sqlExecuteUnClose(sql);
               }
               if (odh.IsExistTable(layerName+"_LINE"))//by zbl 20181010修改
               {
                   sql = string.Format("drop table {0}_LINE;", layerName);
                   odh.sqlExecuteUnClose(sql);
               }
               if (odh.IsExistTable(layerName + "_NEWLINE"))
               {
                   sql = string.Format("drop table {0}_NEWLINE;", layerName);
                   odh.sqlExecuteUnClose(sql);
               }
               if (odh.IsExistTable( layerName+"_POINT"))//by zbl 20181010修改
               {
                   sql = string.Format("drop table {0}_POINT;", layerName);
                   odh.sqlExecuteUnClose(sql);
               }
               if (odh.IsExistTable(layerName + "_NEWPOINT"))
               {
                   sql = string.Format("drop table {0}_NEWPOINT;", layerName);
                   odh.sqlExecuteUnClose(sql);
               }
           }
           odh.dispose();
       }

  
    }
}
