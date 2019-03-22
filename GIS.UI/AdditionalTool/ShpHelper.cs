using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Layer;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using GIS.Geometries;
using GIS.UI.WellKnownText;
using System.Data;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using GIS.UI.Entity;
using System.Collections.Generic;

namespace GIS.UI.AdditionalTool
{
    class ShpHelper
    {

        private AxMapControl axMapControl1;
        public static string GetShpFileByTableName(OracleDBHelper odb, string tableName, string savePath)
        {
            return GetShpFileByTableName(odb, tableName, savePath, null, null);
        }
        public static string GetShpFileByTableName(OracleDBHelper odb, string tableName, string savePath, string condition)
        {
            return GetShpFileByTableName(odb, tableName, savePath, condition, null);
        }
        public static string GetShpFileByTableName(OracleDBHelper odb, string tableName, string savePath, string condition, string fileName)
        {
            if (!odb.IsExistTable(tableName))//数据库中不存在表名
            {
                return string.Format("数据库不存在{0}!", tableName);
            }
            //string shpFieldName = GetGeometryFieldName(odb, tableName);//获取几何字段名
            //if (shpFieldName == null)//如果没有shp字段名
            //{
            //    return string.Format("表格{0}不存在geometry字段!", tableName);//返回不存在相应的类型
            //}
            string shpFieldName = "SHAPE";
            VectorLayerType vt = GetGeometryType(odb, tableName, shpFieldName);//根据几何字段返回相应的矢量图层信息
            string layerName = tableName;//将表名赋值给图层名
            if (fileName != null)
            {//如果文件名不为空
                layerName = fileName;//将文件名赋值给图层名
            }
            string pre = string.Concat(savePath, "\\", layerName);//将保存路径和文件名连接起来
            string aft = ".shp";
            if (File.Exists(pre + aft))//判断是否存在组合之后的路径，如果存在
            {
                try//存在就删除相应的文件
                {
                    //File.Delete(pre + aft);
                    //aft = ".shx";
                    //File.Delete(pre + aft);
                    //aft = ".dbf";
                    //File.Delete(pre + aft);
                    layerName = layerName + "1";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }
            GeoVectorLayer layer = CreateLayer(layerName, savePath, vt);//新建一个几何矢量图层
            GeoData.GeoDataTable table = layer.DataTable;//将这个图层的数据表赋值个几何数据表
            List<String> fields = new List<string>();

            InitTableHeader(table, fields, odb, tableName);//初始化表头

            InputData(table, fields, odb, tableName, shpFieldName, condition);//放入数据
            layer.SaveLayerAsShapeFile(layer.PathName, layer.LayerName);//将图层转换为shp格式
            return "true";
        }

        //获取几何字段名，判断是否是几何字段，是的话就返回几何字段
        private static string GetGeometryFieldName(OracleDBHelper odb, string tableName)
        {
            //输入命令行
            string sql = string.Format("SELECT a.attname as name,pg_type.typname as typename FROM pg_class as c,pg_attribute as a inner join pg_type on pg_type.oid = a.atttypid where c.relname = '{0}' and a.attrelid = c.oid and a.attnum>0", tableName);
            string geometryFieldName = null;

            using (OracleDataReader rd = odb.queryReader(sql))//根据命令行读取数据库里面的内容
            {
                if (rd == null || !rd.HasRows)//如果读出来没有内容
                {
                    return null;//返回没有读取到值
                }
                while (rd.Read())//如果能读取到值
                {
                    string fieldName = rd["name"].ToString();//转换字段名
                    string type = rd["typename"].ToString();//转换类型
                    switch (type)
                    {
                        case "geometry": geometryFieldName = fieldName; break;//如果类型是几何类型，那么将字段名赋值给几何字段名
                        default: break;
                    }

                }
            }
            return geometryFieldName;
        }
        //根据传进来的几何字段名返回相应的矢量图层
        private static VectorLayerType GetGeometryType(OracleDBHelper odb, string tableName, string geometryFileName)
        {
            //string sql = string.Format("SELECT ST_GeometryType({0}) geomtype from {1} where {0} is not null limit 1;", geometryFileName, tableName);//编写命令语句            
            //using (OracleDataReader rd = odb.queryReader(sql))//数据库执行命令语句
            //{
            //    while (rd.Read())//读取到有值
            //    {
            //        string type = rd["geomtype"].ToString();//将几何类型的返回
            //        switch (type)//根据点线面等结合类型返回相应的矢量图层
            //        {
            //            case "ST_Point": return VectorLayerType.PointLayer;
            //            case "ST_MultiLineString"://zh添加
            //            case "ST_LineString": return VectorLayerType.LineLayer;
            //            case "ST_Polygon": return VectorLayerType.PolygonLayer;
            //            default: return VectorLayerType.MixLayer;
            //        }

            //    }
            //}

            string sql = string.Format("select sdo_geometry.get_wkt({0}) geom_as_text from {1}", "SHAPE", tableName);
            using (OracleDataReader rd = odb.queryReader(sql))
            {
                while (rd.Read())
                {
                    string geomStr = rd["geom_as_text"].ToString();
                    int index = geomStr.IndexOf("(");
                    string geomStr2 = geomStr.Substring(0, index - 1);
                    //string geomStr2 = "POLYGON";
                    switch (geomStr2)
                    {
                        case "MULTIPOLYGON": return VectorLayerType.PolygonLayer;
                            break;
                        case "POLYGON": return VectorLayerType.PolygonLayer;
                            break;
                        case "LINESTRING": return VectorLayerType.LineLayer;
                            break;    
                        case "POINT": return VectorLayerType.PointLayer;
                            break;
                        default: return VectorLayerType.MixLayer;
                    }
                }
            }
            switch (tableName)
            {
                case "APOLY": return VectorLayerType.PolygonLayer;
                case "ALINE": return VectorLayerType.LineLayer;
                case "APOINT": return VectorLayerType.PointLayer;
                default: return VectorLayerType.MixLayer;
            }
        }
        //传入图层名，路径和矢量图层信息，根据路径新建一个几何矢量图层，并附上名字和图层类型，将其返回
        public static GeoVectorLayer CreateLayer(string layerName, string path, VectorLayerType vt)
        {

            GeoVectorLayer ptLayer = new GeoVectorLayer(path);//根据路径创建一个几何矢量图层
            ptLayer.LayerName = layerName;//图层名赋值给几何矢量图层
            ptLayer.DataTable = new GIS.GeoData.GeoDataTable();//新建一个几何数据表
            GeoData.GeoDataTable table = ptLayer.DataTable;
            ptLayer.VectorType = vt;//几何矢量图层的矢量类型为传进来的类型
            table.FillData = true; ///空图层，属性信息已经填充，因为是空的
            return ptLayer;//返回这个图层
        }
        //初始化表头
        private static void InitTableHeader(GeoData.GeoDataTable table, List<string> fieldList, OracleDBHelper odb, string tableName)
        {
            //table.Columns.Add("FID", typeof(Int64));//为表的列增添信息
            //table.Columns.Add("TIMESTAMPS", typeof(string)); 
            //fieldList.Add("TIMESTAMPS");
            //新建输入语句

            string sql = string.Format("SELECT  column_name names FROM user_tab_columns WHERE table_name = '{0}'", tableName);
            //执行输入的语句
            using (OracleDataReader rd = odb.queryReader(sql))
            {
                if (rd == null || !rd.HasRows)
                {
                    return;
                }
                table.Columns.Add("FID", typeof(Int64));//为表的列增添信息               
                while (rd.Read())//读取表的信息
                {
                    string fieldName = rd["names"].ToString();
                    //string type = rd["typename"].ToString();
                    //int maxLen = -1;

                    //if ("character varying".Equals(type))
                    //{
                    //    string maxLenStr = rd["maxlen"].ToString();
                    //    if (!string.IsNullOrEmpty(maxLenStr))
                    //    {
                    //        maxLen = int.Parse(maxLenStr);
                    //    }

                    //}
                    if ("SHAPE".Equals(fieldName))
                    {
                        continue;
                    }
                    else
                    {
                        table.Columns.Add(fieldName, typeof(string)); fieldList.Add(fieldName);
                    }
                    //switch (fieldName)
                    //{
                    //    case "OSMID": table.Columns.Add(fieldName, typeof(string)); fieldList.Add(fieldName); break;
                    //    case "VERSIONID": table.Columns.Add(fieldName, typeof(string)); fieldList.Add(fieldName); break;
                    //    case "CHANGESET": table.Columns.Add(fieldName, typeof(string)); fieldList.Add(fieldName); break;
                    //    case "TIMESTAMPS": table.Columns.Add(fieldName, typeof(string)); fieldList.Add(fieldName); break;
                    //    case "USERID": table.Columns.Add(fieldName, typeof(string)); fieldList.Add(fieldName); break;//DY添加
                    //    case "USERNAME": table.Columns.Add(fieldName, typeof(string)); fieldList.Add(fieldName); break;
                    //    case "FC": table.Columns.Add(fieldName, typeof(string)); fieldList.Add(fieldName); break;
                    //    case "DSG": table.Columns.Add(fieldName, typeof(string)); fieldList.Add(fieldName); break;
                    //    case "TAGS": table.Columns.Add(fieldName, typeof(string)); fieldList.Add(fieldName); break;
                    //    default: break;
                    //}

                }
            }

        }
        private static void InputData(GeoData.GeoDataTable table, List<string> fields, OracleDBHelper odb, string tableName, string shpFieldName, string condition)
        {
            string sql1 = string.Format("SELECT  column_name names FROM user_tab_columns WHERE table_name = '{0}'", tableName);
            //执行输入的语句
            string sqlName = "";
            using (OracleDataReader rd = odb.queryReader(sql1))
            {
                if (rd == null || !rd.HasRows)
                {
                    return;
                }

                while (rd.Read())//读取表的信息
                {

                    string fieldName = rd["names"].ToString();
                    if (fieldName == "SHAPE")
                    { }
                    else
                    {
                        sqlName = sqlName + fieldName + ",";
                    }
                    
                }
                sqlName = sqlName.Substring(0, sqlName.LastIndexOf(","));
            }

            //table.Columns.Add("TIMESTAMPS", typeof(string));
            //tableName = tableName.ToLower();
            //string sql = string.Format("select sdo_geometry.get_wkt({0}) geom_as_text,OSMID,VERSIONID,CHANGESET,TIMESTAMPS,USERID,USERNAME,FC,DSG,TAGS,POINTSID from {1}", shpFieldName, tableName);
            string sql = string.Format("select sdo_geometry.get_wkt({0}) geom_as_text,{1} from {2}", shpFieldName,sqlName,tableName);
            //string sql = string.Format("select sdo_geometry.get_wkt({0}) geom_as_text,OSMID,VERSIONID,TIMESTAMPS,CHANGESET,USERID,USERNAME,FC,DSG,TAGS,TRUSTVALUE,POINTSID,POLYTYPE from {1}", shpFieldName, tableName);
            using (OracleDataReader rd = odb.queryReader(sql))
            {
                if (rd == null || !rd.HasRows)
                {
                    return;
                }
                int k = 1;
                while (rd.Read())
                {
                    GeoData.GeoDataRow dataRow = table.NewRow();
                    dataRow["FID"] = k;
                    k++;
                    //zh修改
                    for (int i = 0; i < fields.Count; i++)
                    {
                        int maxLen = table.Columns[fields[i]].MaxLength;
                        string str = rd[fields[i]].ToString();
                        if (maxLen != -1 && maxLen < str.Length)
                        {
                            dataRow[fields[i]] = str.Substring(0, maxLen);
                        }
                        else
                        {
                            dataRow[fields[i]] = str;
                        }
                        str = null;

                    }
                    //结束修改
                    try
                    {
                        string geomStr = rd["geom_as_text"].ToString();
                        Geometry geom = GeometryFromWKT.Parse(geomStr);
                        dataRow.Geometry = geom;
                        table.AddRow(dataRow);
                        geom = null;
                        geomStr = null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {
                        dataRow = null;
                    }
                }
            }

        }

        #region shp数据辅助功能 zh修改 0929 
        List<string> list = new List<string>();//wkt的集合
        public ShpHelper()
        {
        }
        public ShpHelper(AxMapControl axMapControl1)
        {
            this.axMapControl1 = axMapControl1;
        }

        //将shp数据加入地图控件
        public void addShpToAxmap(string[] pFullPath)
        {
            for (int i = 0; i < pFullPath.Length; i++)
            {
                if (pFullPath[i] == "") return;

                addShpToAxmap(pFullPath[i]);
            }
        }

        //将shp数据加入地图控件
        public void addShpToAxmap(string pFullPath)
        {
                if (pFullPath == "") return;

                int pIndex = pFullPath.LastIndexOf("\\");
                string pFilePath = pFullPath.Substring(0, pIndex); //文件路径
                string pFileName = pFullPath.Substring(pIndex + 1); //文件名
                axMapControl1.AddShapeFile(pFilePath, pFileName);
                list = ShptoWkt(pFullPath);
        }

        //获取shp数据的属性表
        public DataTable getShpAttributes(int j)
        {
            IFeatureLayer _curFeatureLayer = axMapControl1.get_Layer(j) as IFeatureLayer;
            if (_curFeatureLayer == null) return null;

            IFeature pFeature = null;
            DataTable pFeatDT = new DataTable(); //创建数据表
            DataRow pDataRow = null; //数据表行变量
            DataColumn pDataCol = null; //数据表列变量
            IField pField = null;
            for (int i = 0; i < _curFeatureLayer.FeatureClass.Fields.FieldCount; i++)
            {
                pDataCol = new DataColumn();
                pField = _curFeatureLayer.FeatureClass.Fields.get_Field(i);
                pDataCol.ColumnName = pField.AliasName; //获取字段名作为列标题
                pDataCol.DataType = Type.GetType("System.Object");//定义列字段类型
                pFeatDT.Columns.Add(pDataCol); //在数据表中添加字段信息
            }

            IFeatureCursor pFeatureCursor = _curFeatureLayer.Search(null, true);

            pFeature = pFeatureCursor.NextFeature();



            while (pFeature != null)
            {
                //try { 
                //pDataRow = pFeatDT.NewRow();
                pDataRow = pFeatDT.NewRow();
                //获取字段属性
                for (int k = 0; k < pFeatDT.Columns.Count; k++)
                {
                    pDataRow[k] = pFeature.get_Value(k);
                }

                pFeatDT.Rows.Add(pDataRow); //在数据表中添加字段属性信息
                pFeature = pFeatureCursor.NextFeature();

                //}

                //catch(Exception e)
                //{

                //}
            }
            //释放指针
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
            return pFeatDT;
        }


        //将线、面DataTable转换成list
        public List<Poly> datatableToPolyList(DataTable dt,int dataType)
        {
            List<Poly> polyList = new List<Poly>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Poly poly = new Poly();
                //poly.setObjectid(wayObjectid);

                poly.setWkt(list[i]);
                if (dt.Columns.Contains("osmid") &&! dt.Rows[i]["osmid"].ToString().Trim().Equals("")) poly.setOsmid(long.Parse(dt.Rows[i]["osmid"].ToString()));
                if (dt.Columns.Contains("versionid") && !dt.Rows[i]["versionid"].ToString().Trim().Equals("")) poly.setVersion(int.Parse(dt.Rows[i]["versionid"].ToString()));
                if (dt.Columns.Contains("userreputation") && !dt.Rows[i]["userreputation"].ToString().Trim().Equals("")) poly.setUserreputation(long.Parse(dt.Rows[i]["userreputation"].ToString()));
                if (dt.Columns.Contains("trustvalue") && !dt.Rows[i]["trustvalue"].ToString().Trim().Equals("")) poly.setTrustvalue(double.Parse(dt.Rows[i]["trustvalue"].ToString()));
                if (dt.Columns.Contains("matchid") && !dt.Rows[i]["matchid"].ToString().Trim().Equals("")) poly.setMatchid(int.Parse(dt.Rows[i]["matchid"].ToString()));

                if (dt.Columns.Contains("userid") && !dt.Rows[i]["userid"].ToString().Trim().Equals("")) poly.setUserid(int.Parse(dt.Rows[i]["userid"].ToString()));
                if (dt.Columns.Contains("username")) poly.setUsername(dt.Rows[i]["username"].ToString());
                if (dt.Columns.Contains("Tags")) poly.setTags(dt.Rows[i]["Tags"].ToString());
                if (dt.Columns.Contains("fc")) poly.setFc(dt.Rows[i]["fc"].ToString());
                if (dt.Columns.Contains("dsg")) poly.setDsg(dt.Rows[i]["dsg"].ToString());
                if (dt.Columns.Contains("starttime")) poly.setStartTime(dt.Rows[i]["starttime"].ToString());
                if (dt.Columns.Contains("endtime")) poly.setEndTime(dt.Rows[i]["endtime"].ToString());
                if (dt.Columns.Contains("source")) poly.setSource(int.Parse(dt.Rows[i]["source"].ToString()));
                if (dt.Columns.Contains("NationEleN")) poly.setNationelename(dt.Rows[i]["NationEleN"].ToString());
                if (dt.Columns.Contains("NationCode")) poly.setNationcode(dt.Rows[i]["NationCode"].ToString());
                if (dt.Columns.Contains("changeset")) poly.setChangeset(dt.Rows[i]["changeset"].ToString());
                if (dt.Columns.Contains("pointsid")) poly.setPointsId(dt.Rows[i]["pointsid"].ToString());
                if (dataType == 1)
                {
                    if (dt.Columns.Contains("changetype")) poly.setChangetype(dt.Rows[i]["changetype"].ToString());
                }
                polyList.Add(poly);

            }
            return polyList;
        }

        //将点DataTable转换成list
        public List<Ppoint> datatableToPointList(DataTable dt,int dataType)
        {
            List<Ppoint> pointList = new List<Ppoint>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Ppoint point = new Ppoint();
                //poly.setObjectid(wayObjectid);

                point.setWkt(list[i]);
                if (dt.Columns.Contains("osmid") && !dt.Rows[i]["osmid"].ToString().Trim().Equals("")) point.setOsmid(long.Parse(dt.Rows[i]["osmid"].ToString()));
                if (dt.Columns.Contains("lat") && !dt.Rows[i]["lat"].ToString().Trim().Equals("")) point.setLat(double.Parse(dt.Rows[i]["lat"].ToString()));
                if (dt.Columns.Contains("lon") && !dt.Rows[i]["lon"].ToString().Trim().Equals("")) point.setLon(double.Parse(dt.Rows[i]["lon"].ToString()));
                if (dt.Columns.Contains("versionid") && !dt.Rows[i]["versionid"].ToString().Trim().Equals("")) point.setVersion(int.Parse(dt.Rows[i]["versionid"].ToString()));
                if (dt.Columns.Contains("matchid") && !dt.Rows[i]["matchid"].ToString().Trim().Equals("")) point.setMatchid(int.Parse(dt.Rows[i]["matchid"].ToString()));

                if (dt.Columns.Contains("userid") && !dt.Rows[i]["userid"].ToString().Trim().Equals("")) point.setUserid(int.Parse(dt.Rows[i]["userid"].ToString()));
                if (dt.Columns.Contains("username")) point.setUsername(dt.Rows[i]["username"].ToString());
                if (dt.Columns.Contains("Tags")) point.setTags(dt.Rows[i]["Tags"].ToString());
                if (dt.Columns.Contains("fc")) point.setFc(dt.Rows[i]["fc"].ToString());
                if (dt.Columns.Contains("dsg")) point.setDsg(dt.Rows[i]["dsg"].ToString());
                if (dt.Columns.Contains("starttime")) point.setStartTime(dt.Rows[i]["starttime"].ToString());
                if (dt.Columns.Contains("endtime")) point.setEndTime(dt.Rows[i]["endtime"].ToString());
                if (dt.Columns.Contains("source")) point.setSource(int.Parse(dt.Rows[i]["source"].ToString()));
                if (dt.Columns.Contains("changeset")) point.setChangeset(dt.Rows[i]["changeset"].ToString());
                if (dataType == 1)
                {
                    if (dt.Columns.Contains("nationelename")) point.setNationelename(dt.Rows[i]["nationelename"].ToString());
                    if (dt.Columns.Contains("nationcode")) point.setNationelename(dt.Rows[i]["nationcode"].ToString());
                    if (dt.Columns.Contains("changetype")) point.setChangetype(dt.Rows[i]["changetype"].ToString());
                }
                pointList.Add(point);

            }
            return pointList;
        }

        //将shape字段转成wkt
        public List<string> ShptoWkt(string path)
        {
            List<string> list = new List<string>();
            string wkt = "";

            OSGeo.OGR.Ogr.RegisterAll();
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");

            if (dr == null)
            {
                return list;
            }

            OSGeo.OGR.DataSource ds = dr.Open(path, 0);
            int layerCount = ds.GetLayerCount();

            OSGeo.OGR.Layer layer = ds.GetLayerByIndex(0);

            //投影信息
            //OSGeo.OSR.SpatialReference coord = layer.GetSpatialRef();
            //string coordString;
            //coord.ExportToWkt(out coordString);

            OSGeo.OGR.Feature feat;
            //string contentString = "";
            //读取shp文件
            while ((feat = layer.GetNextFeature()) != null)
            {
                OSGeo.OGR.Geometry geometry = feat.GetGeometryRef();
                OSGeo.OGR.wkbGeometryType goetype = geometry.GetGeometryType();
                geometry.ExportToWkt(out wkt);
                list.Add(wkt);
            }
            return list;
        }
        #endregion
        #region 去除NATIONCODE字段，BY YG
        public List<Poly> datatabletoPolyList(DataTable dt, int dataType)
        {
            List<Poly> polyList = new List<Poly>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Poly poly = new Poly();
                //poly.setObjectid(wayObjectid);

                poly.setWkt(list[i]);
                //if (dt.Columns.Contains("Shape")) poly.setShape((dt.Rows[i]["Shape"].ToString()));
                if (dt.Columns.Contains("username")) poly.setUsername(dt.Rows[i]["username"].ToString());
                //if (dt.Columns.Contains("userid")) poly.setUserid(int.Parse(dt.Rows[i]["userid"].ToString()));
                if (dt.Columns.Contains("Tags")) poly.setTags(dt.Rows[i]["Tags"].ToString());
                if (dt.Columns.Contains("fc")) poly.setFc(dt.Rows[i]["fc"].ToString());
                if (dt.Columns.Contains("dsg")) poly.setDsg(dt.Rows[i]["dsg"].ToString());
                if (dt.Columns.Contains("starttime")) poly.setStartTime(dt.Rows[i]["starttime"].ToString());
                if (dt.Columns.Contains("endtime")) poly.setStartTime(dt.Rows[i]["endtime"].ToString());
                //if (dt.Columns.Contains("source")) poly.setSource(int.Parse(dt.Rows[i]["source"].ToString()));
                if (dt.Columns.Contains("NationEleN")) poly.setNationelename(dt.Rows[i]["NationEleN"].ToString());
                //if (dt.Columns.Contains("NationCode")) poly.setNationcode(int.Parse(dt.Rows[i]["NationCode"].ToString()));
                if (dataType == 1)
                {
                    if (dt.Columns.Contains("changetype")) poly.setChangetype(dt.Rows[i]["changetype"].ToString());
                }
                polyList.Add(poly);

            }
            return polyList;
        }
        #endregion

    }
}
