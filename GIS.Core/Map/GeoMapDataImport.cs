using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Data;
using System.IO;
using GIS.Map;
using GIS.Layer;
using GIS.Geometries;
using GIS.GeoData;
using OSGeo.OGR;

using System.Windows.Forms;

namespace GIS.Map
{
    public partial class GeoMap
    {
        public delegate void LayerGroupIncreaseEventHandle(String strGroupName);
        public event LayerGroupIncreaseEventHandle LayerGroupIncrease;

        public delegate void LayerIncreaseEventHandle(string lyrName, LAYERTYPE_DETAIL type);
        public event LayerIncreaseEventHandle LayerIncrease;

        public delegate void LayerDecreaseEventHandle(string lyrName );
        public event LayerDecreaseEventHandle LayerDecrease;

        public delegate void LayerGroupDecreaseEventHandle(string lyrGroupName);
        public event LayerGroupDecreaseEventHandle LayerGroupDecrease;

        public bool AddGroup(String strGroupName)
        {
            if (GetGroupByName(strGroupName) != null)
                return false;
            LayerGroup group = new LayerGroup(strGroupName);
            m_LayerGroups.Add(group);
            m_ActiveLyrGroup = strGroupName;
            if (LayerGroupIncrease != null)
                LayerGroupIncrease(strGroupName);
            return true;
        }
        public void AddLayer(GeoLayer layer)
        {
            if (layer.LayerName != "草图层")
            {
                m_Layers.Add(layer);
                LayerGroup lyrGoup = m_LayerGroups[m_ActiveLyrGroupIndex];
                lyrGoup.Layers.Add(layer);
                lyrGoup.LayerGroupBound = lyrGoup.GetBoundingBox();
                SetActiveLayer(m_ActiveLyrGroup, layer.LayerName);
                if (LayerIncrease != null)
                {
                    LayerIncrease(layer.LayerName, layer.LayerTypeDetail);
                }
            }
            else
            {
                GeoLayer lyr = GetLayerByName("草图层");
                m_Layers.Add(layer);
                if (lyr == m_ActiveVectorLayer) //如果图层为当前活动图层
                {
                    m_ActiveVectorLayer = null;
                }
                if (lyr == m_ActiveLabelLayer)
                {
                    m_ActiveLabelLayer = null;
                }
                m_Layers.Remove(lyr);
                GetGroupByName("系统工作区").Layers.Remove(lyr);//删除图层 

                LayerGroup group = GetGroupByName("系统工作区");
                group.Layers.Add(layer);
                group.LayerGroupBound = group.GetBoundingBox();
            }
           
            m_MapBound = GetBoundingBox();//边界矩形重新计算，不能用MAPBOUND 因为会触发鹰眼事件
        }
        public bool RemoveLayerGroup(string strGroupName)
        {
            LayerGroup group = GetGroupByName(strGroupName);
            if (group != null)
            {
                int lyrCount = group.Counts;
                for (int i = lyrCount - 1; i >= 0; i--)
                {
                    RemoveLayer(strGroupName, group[i].LayerName);
                }
                m_LayerGroups.Remove(group);
                if (LayerGroupDecrease != null)
                    LayerGroupDecrease(strGroupName);
                return true;
            }
            return false;
        }
        public bool RemoveLayer(String strGroupName, String strLayerName)
        {
            GeoLayer lyr = GetLayerByName(strGroupName, strLayerName);

            if (lyr != null)
            {
                if (lyr == m_ActiveVectorLayer) //如果图层为当前活动图层
                {
                    m_ActiveVectorLayer = null;
                }
                if (lyr == m_ActiveLabelLayer)
                {
                    m_ActiveLabelLayer = null;
                }
                m_Layers.Remove(lyr);
                GetGroupByName(strGroupName).Layers.Remove(lyr);//删除图层
                GetGroupByName(strGroupName).LayerGroupBound = GetGroupByName(strGroupName).GetBoundingBox();
                MapBound = GetBoundingBox();
                if (LayerDecrease != null)
                {
                    LayerDecrease(strLayerName);
                }
              
                return true;
            }
            return false;
        }
 
        //加入文件的几何数据
        public void AddFiles(string[] strFileNames)
        {
            //#region 修改
            //Draftlayer.VectorType = VectorLayerType.MixLayer;
            //#endregion
            foreach (string strFileName in strFileNames)
            {
                AddFile(strFileName);
            }

            #region 修改
            //AddLayer(Draftlayer);
            //Draftlayer.DataTable.FillData = true;
            #endregion
        }

        //private void AddFile(string strFileName)
        //{
        //    try
        //    {
        //        string lyrName = Path.GetFileNameWithoutExtension(strFileName);
        //        if (lyrName != "草图层" && LayerExist(lyrName))
        //        {
        //            return;
        //        }
        //        string ExtName = Path.GetExtension(strFileName).ToLower();
        //        switch (ExtName)
        //        {
        //            case ".shp":
        //                AddShapeFile(strFileName);
        //                return;
        //            case ".lq":
        //                AddLQFile(strFileName);
        //                break;
        //            case ".dxf":
        //                AddDxfFile(strFileName);
        //                return;
                      
        //            default:
        //                AddRasterFile(strFileName);
        //                return;
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //}

        private void AddFile(string strFileName)
        {
            #region old
            string lyrName = Path.GetFileNameWithoutExtension(strFileName);
            if (lyrName != "草图层" && LayerExist(lyrName))
            {
                MessageBox.Show("已经存在同名图层","提示");
                return;
            }
            string ExtName = Path.GetExtension(strFileName).ToLower();
            switch (ExtName)
            {
                case ".shp":
                    AddShapeFile(strFileName);
                    //AddAttributeToDraftLayer(strFileName);
                    return;
                case ".evc":
                    AddLQFile(strFileName);
                    break;
                case ".dxf":
                    AddDxfFile(strFileName);
                    return;
                case ".e00":
                    //此处需用线程处理
                    AddOgrFile(strFileName);
                    return;
                default:
                    AddRasterFile(strFileName);
                    return;
            }
            #endregion
        }

        private void AddDxfFile(string strFileName)
        {

            GIS.GeoData.DataProviders.DxfFile dxf = new GIS.GeoData.DataProviders.DxfFile(strFileName);
            for (int i = 0; i < dxf.LayerList.Count; i++)
            {
                try
                {
                    GeoLayer lyr = dxf.LayerList[i];
                    lyr.LayerBound = lyr.GetBoundingBox();
                    AddLayer(lyr);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
         
        }
        //添加LQ矢量数据
        private void AddLQFile(string FilePath)
        {
            GIS.GeoData.DataProviders.IProvider datasource = null;
            GeoData.GeoDataTable table = null;
            try
            {
                datasource = new GIS.GeoData.DataProviders.LQFile(FilePath);
                datasource.Open();
                table = datasource.ExecuteAllGeomQuery();
                datasource.Close();
            }

            catch (Exception e)
            {
                if (datasource != null)
                {
                     datasource.Close();
                }
                throw new ApplicationException(e.Message);
            }

            GeoVectorLayer lyr = new GeoVectorLayer(FilePath);
            lyr.DataTable = table;
            lyr.VectorType = (VectorLayerType)datasource.GetLayerType();
            lyr.LayerBound = datasource.GetExtents();
            lyr.LayerStyle = datasource.GetLayerStype();
            AddLayer(lyr);
        }

        //添加SHP矢量数据
        //public  GeoVectorLayer Draftlayer = new GeoVectorLayer("混合层");
        //private bool bStartDraftlayer = false;
        //private int CurrentDraftNum = 0;
        private void AddShapeFile(string FilePath)
        {
            GIS.GeoData.DataProviders.IProvider datasource = null;
            GeoData.GeoDataTable table = null;
          
            try
            {
                datasource = new GIS.GeoData.DataProviders.ShapeFile(FilePath);
                datasource.Open();
                table = datasource.ExecuteAllGeomQuery();
                //datasource.ExecuteAllAttributeQuery(table);
                datasource.Close();
            }
            catch (Exception e)
            {
                if (datasource != null)
                {
                    datasource.Close();
                }
                throw new ApplicationException(e.Message);
            }

            GeoVectorLayer lyr = new GeoVectorLayer(FilePath);
            lyr.DataTable = table;  

            lyr.VectorType = (VectorLayerType)datasource.GetLayerType();
            lyr.LayerBound = datasource.GetExtents();
            AddLayer(lyr);

            AddSymbolFile(lyr,FilePath);

            #region 20121011修改
            //if (lyr.VectorType == VectorLayerType.PolygonLayer || lyr.VectorType == VectorLayerType.LabelLayer)
            //{
            //    lyr.LayerBound = datasource.GetExtents();
            //    AddLayer(lyr);
            //}
            

            //if (bStartDraftlayer == false)
            //{
            //    Draftlayer.DataTable = lyr.DataTable.Clone();
            //    for (int i = 0; i < lyr.DataTable.Count;i++ )
            //    {
            //            Draftlayer.AddGeometry(lyr.DataTable[i].Geometry);
                    
            //            Draftlayer.DataTable.Rows[CurrentDraftNum + i]["ClasID"] = table[i]["ClasID"];
            //            Draftlayer.DataTable.Rows[CurrentDraftNum + i]["BeginTime"] = table[i]["BeginTime"];
            //            Draftlayer.DataTable.Rows[CurrentDraftNum + i]["FeatID"] = table[i]["FeatID"];
            //            Draftlayer.DataTable.Rows[CurrentDraftNum + i]["UserID"] = table[i]["UserID"];
            //            Draftlayer.DataTable.Rows[CurrentDraftNum + i]["ChangeType"] = table[i]["ChangeType"];
                   
                     
            //    }
            //    CurrentDraftNum = CurrentDraftNum + table.Count;
            //    bStartDraftlayer = true;
            //}
            //else
            //{
            //    for (int i = 0; i < lyr.DataTable.Count; i++)
            //    {
            //        Draftlayer.AddGeometry(lyr.DataTable[i].Geometry);
            //        Draftlayer.DataTable.Rows[CurrentDraftNum + i]["ClasID"] = table[i]["ClasID"];
            //        Draftlayer.DataTable.Rows[CurrentDraftNum + i]["BeginTime"] = table[i]["BeginTime"];
            //        Draftlayer.DataTable.Rows[CurrentDraftNum + i]["FeatID"] = table[i]["FeatID"];
            //        Draftlayer.DataTable.Rows[CurrentDraftNum + i]["UserID"] = table[i]["UserID"];
            //        Draftlayer.DataTable.Rows[CurrentDraftNum + i]["ChangeType"] = table[i]["ChangeType"];
                   
            //    }
            //    CurrentDraftNum = CurrentDraftNum + table.Count;
            //}
            #endregion
        }

        private void AddSymbolFile(GeoVectorLayer lry,string FilePath)
        {
            string str = FilePath.Substring(0, FilePath.Length - 3) + "sym";
            
            // 如果存在存在符号文件
            if (File.Exists(str))
            {
                Dictionary<string, System.Drawing.Color> dic = new Dictionary<string, System.Drawing.Color>();

                FileStream fs = new FileStream(str, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string head = sr.ReadLine();
                string line = sr.ReadLine();
                int apha = 0;
                while (line != null)
                {
                    string[] values = line.Split(':');
                    string szName = values[0];
                    int a = int.Parse(values[1]);
                    int r = int.Parse(values[2]);
                    int g = int.Parse(values[3]);
                    int b = int.Parse(values[4]);
                    apha = a;
                    Color color = Color.FromArgb(a, r, g, b);
                    dic.Add(szName,color);
                    line = sr.ReadLine();
                }
                lry.FileSymbol = dic;
                lry.SymbolField = head;
                lry.bShowSymbol = true;
                lry.apha = apha;
                sr.Close();
                fs.Close();
            }
        }
       
        //添加栅格图层
        private void AddRasterFile(string FilePath)
        {
            GeoRasterLayer lyr = null;
            try
            {
                lyr = new GeoRasterLayer(FilePath);
            }
            catch (Exception e)
            {
                lyr.Dispose();
                throw new ApplicationException(e.Message);
            }
            AddLayer(lyr);
        }

        public void AddOgrFile(string strFileName)
        {
            OSGeo.OGR.Ogr.RegisterAll();//注册所有驱动

            // 为了支持中文路径，请添加下面这句代码
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            // 为了使属性表字段支持中文，请添加下面这句
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");

            OSGeo.OGR.DataSource poDS = OSGeo.OGR.Ogr.Open(strFileName, 0);
            if (poDS == null)
            {
                return;
            }

            GeoVectorLayer lyr = new GeoVectorLayer(strFileName);

            lyr.VectorType = VectorLayerType.MixLayer;

            GeoDataTable datatable = new GeoDataTable();

            OSGeo.OGR.Layer poLayer = poDS.GetLayerByIndex(0);


            FeatureDefn FtrDefn = poLayer.GetLayerDefn();


            int FieldCount = FtrDefn.GetFieldCount();
            bool bHasClasID = false;
            bool bHasFeatID = false;
            bool bHasBeginTime = false;
            datatable.Columns.Add("FID", typeof(int));
            for (int j = 0; j < FieldCount; j++)
            {
                FieldDefn FieldDefn = FtrDefn.GetFieldDefn(j);
                string FieldName = FieldDefn.GetName();
                string TypeName = FieldDefn.GetTypeName();
                Type type = null;
                switch (TypeName)
                {
                    case "Integer":
                        type = typeof(int);
                        break;
                    case "String":
                        type = typeof(string);
                        break;
                    case "Real":
                        type = typeof(double);
                        break;
                }
                DataColumn column = new DataColumn(FieldName, type);
                datatable.Columns.Add(column);
                if (FieldName == "ClasID") //************************/判断 3个关键字段是否存在
                    bHasClasID = true;
                else if (FieldName == "FeatID")
                    bHasFeatID = true;
                else if (FieldName == "BeginTime")
                    bHasBeginTime = true;
            }

            if (!bHasFeatID)
            {
                datatable.Columns.Add("FeatID", typeof(string));
            }
            if (!bHasClasID)
            {
                datatable.Columns.Add("ClasID", typeof(Int64));
            }
            if (!bHasBeginTime)
            {
                datatable.Columns.Add("BeginTime", typeof(string));
            }
            datatable.Columns.Add("ChangeType", typeof(string));
             

            OSGeo.OGR.Feature poFeature;
            poLayer.ResetReading();
            int fid = 0;
            while ((poFeature = poLayer.GetNextFeature()) != null)
            {
                OSGeo.OGR.FeatureDefn poFDefn = poLayer.GetLayerDefn();
                int iField;
                GeoDataRow row = datatable.NewRow();
                row[0] = fid++;
                #region attribute
                for (iField = 0; iField < poFDefn.GetFieldCount(); iField++)
                {
                    OSGeo.OGR.FieldDefn poFieldDefn = poFDefn.GetFieldDefn(iField);

                    string strname = poFieldDefn.GetName();
                    string item = null;
                    if (poFieldDefn.GetFieldType() == FieldType.OFTInteger)
                    {
                        item = poFeature.GetFieldAsInteger(iField).ToString();
                    }
                    else if (poFieldDefn.GetFieldType() == FieldType.OFTReal)
                    {
                        item = poFeature.GetFieldAsDouble(iField).ToString();
                    }
                    else if (poFieldDefn.GetFieldType() == FieldType.OFTString)
                    {
                        item = poFeature.GetFieldAsString(iField);
                    }
                    else
                    {
                        item = poFeature.GetFieldAsString(iField);
                    }
                    row[iField+1] = item;
                    if (!bHasBeginTime)
                    {
                        DateTime time = DateTime.Today;
                        string time1 = string.Format("{0}{1:d2}{2:d2}", time.Year.ToString(), int.Parse(time.Month.ToString()), int.Parse(time.Day.ToString()));
                        row["BeginTime"] = time1;
                    }
                    if (!bHasClasID)
                    {
                        row["ClasID"] = 0;
                    }
                    if (!bHasFeatID)
                    {
                        row["FeatID"] = System.Guid.NewGuid().ToString();
                    }
                    row["ChangeType"] = EditState.Original;
                }
                #endregion

                OSGeo.OGR.Geometry poGeometry;
                poGeometry = poFeature.GetGeometryRef();
                if (poGeometry == null)
                {
                    poFeature.Dispose();
                    return;
                }
                wkbGeometryType type = poGeometry.GetGeometryType();
                switch (type)
                {
                    case wkbGeometryType.wkbPoint:
                        GeoPoint newpt = new GeoPoint(poGeometry.GetX(0), poGeometry.GetY(0));
                        row.Geometry = newpt;
                        break;
                    case wkbGeometryType.wkbPoint25D:
                        GeoPoint newpt1 = new GeoPoint(poGeometry.GetX(0), poGeometry.GetY(0));
                        row.Geometry = newpt1;
                        break;

                    case wkbGeometryType.wkbLineString:
                        GeoLineString newlinestring = new GeoLineString();
                        int ptcount = poGeometry.GetPointCount();
                        for (int i = 0; i < ptcount; i++)
                        {
                            newlinestring.AddPoint(new GeoPoint(poGeometry.GetX(i), poGeometry.GetY(i)));
                        }
                        row.Geometry = newlinestring;
                        break;
                    case wkbGeometryType.wkbPolygon:
                        GeoPolygon plg = new GeoPolygon();
                        GeoLinearRing outring = new GeoLinearRing();
                        OSGeo.OGR.Geometry poutring = poGeometry.GetGeometryRef(0);

                        for (int i = 0; i < poutring.GetPointCount(); i++)
                        {
                            outring.AddPoint(new GeoPoint(poutring.GetX(i), poutring.GetY(i)));
                        }
                        int ringcount = poGeometry.GetGeometryCount();
                        plg.ExteriorRing = outring;

                        for (int i = 1; i < ringcount; i++)
                        {
                            GeoLinearRing inring = new GeoLinearRing();
                            OSGeo.OGR.Geometry pinring = poGeometry.GetGeometryRef(i);
                            for (int j = 0; j < pinring.GetPointCount(); j++)
                            {
                                inring.AddPoint(new GeoPoint(pinring.GetX(j), pinring.GetY(j)));
                            }
                            plg.InteriorRings.Add(inring);
                        }
                        row.Geometry = plg;
                        break;
                    default:
                        break;

                }

                datatable.AddRow(row);
                poFeature.Dispose();

            }
            lyr.DataTable = datatable;
            lyr.DataTable.FillData = true;
            AddLayer(lyr);

            poDS.Dispose();
 
        }
    }
}
