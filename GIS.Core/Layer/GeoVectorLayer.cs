using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using GIS.Geometries;
using GIS.Utilities;
using GIS.GeoData;
using GIS.Map;
using System.IO;
using OSGeo.OGR;

using System.Runtime.Serialization;
namespace GIS.Layer
{
    [Serializable]
    /// <summary>
    /// 矢量图层的样式
    /// </summary>
    public enum VectorLayerType
    {
        PointLayer = 1,              //点类
        LineLayer = 3,           //线层      
        PolygonLayer = 5,           //面
        MixLayer = 7,             //混合层 
        LabelLayer = 9,//注记 
        SurveyLayer = 100,//观测层
        DraftLayer = 200 //草图层
    }

    public class GeoVectorLayer:GeoLayer
    {
        public GeoVectorLayer(){}
        public GeoVectorLayer(string strFilePathName): base(strFilePathName){}

        #region PrivateMember

        private VectorLayerType m_VectorType;
        private Style.VectorStyle m_Style = new GIS.Style.VectorStyle();
        private GeoDataTable m_DataTable;          //数据表

        private string m_symbolfield = null;
        private Dictionary<string,Color> m_filesymbol=null;
        private bool m_bShowSymbol = false;
        //public OSGeo.OSR.SpatialReference sr = null;
        #endregion

        #region Properties

        public bool bShowSymbol
        {
            get { return m_bShowSymbol; }
            set { m_bShowSymbol = value; }
        }

        public int apha;

        public string SymbolField
        {
            get { return m_symbolfield; }
            set { m_symbolfield = value; }
        }

        public Dictionary<string, Color> FileSymbol
        {
            get { return m_filesymbol; }
            set { m_filesymbol = value; }
        }
 
        public Style.VectorStyle LayerStyle
        {
            get { return m_Style; }
            set { m_Style = value; }
        }
        public GeoDataTable DataTable
        {
            get { return m_DataTable; }
            set 
            {
                value.BelongLayer = this;
                m_DataTable = value; 
            }
        }

        public VectorLayerType VectorType
        {
            get { return m_VectorType; }
            set 
            {
                m_VectorType = value;
                switch (value)
                {
                    case VectorLayerType.PointLayer:
                        ClasID = 110000;
                        break;
                    case VectorLayerType.LineLayer:
                        ClasID = 430000;
                        break;
                    case VectorLayerType.PolygonLayer:
                        ClasID = 310000;
                        break;
                    case VectorLayerType.MixLayer:
                        ClasID = 888888;
                        break;
                    case VectorLayerType.LabelLayer:
                        ClasID = 999999;
                        break;
                }
            
            }
        }

        public override LAYERTYPE LayerType
        {
            get { return LAYERTYPE.VectorLayer; }
        }
        
        public override LAYERTYPE_DETAIL LayerTypeDetail
        {
            get { return (LAYERTYPE_DETAIL)((int)VectorType); }
        }

        #endregion
       
        #region Function

        public GeoDataRow AddGeometry(GIS.Geometries.Geometry geom)
        {
            GIS.GeoData.GeoDataRow row = DataTable.NewRow();//产生新记录
            DataTable.AddRow(row);                          //添加数据
            row[0] = DataTable.Count-1;         //FID赋值
            row.Geometry = geom;
            LayerBound = GetBoundingBox();           //重新计算边界矩形   
  
            return row;
        }

        public override GeoBound GetBoundingBox()
        {
            if (DataTable == null || DataTable.Count == 0)
                return null;

            GeoBound bound = null; //几何可能出现为空的情况，所以要判断
            for (int i = 0; i < DataTable.Count; ++i)
            {
                if (DataTable[i].Geometry == null)
                    continue;
                if (bound == null)
                {
                    bound = DataTable[i].Geometry.Bound.Clone();
                    continue;
                }
                bound.UnionBound(DataTable[i].Geometry.Bound);
            }
            return bound; 
        }
  
        #endregion

        #region 保存文件
        public void SaveFileAsShapeFileOfType(string strFolderName, string strLayerName, VectorLayerType type, bool ismix)
        {
            

            OSGeo.OGR.Ogr.RegisterAll();//注册所有驱动

            // 为了支持中文路径，请添加下面这句代码
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            // 为了使属性表字段支持中文，请添加下面这句
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");

            string pszDriverName = "ESRI Shapefile";

            Driver poDriver = Ogr.GetDriverByName(pszDriverName);//创建SHAPEFILE 文件驱动
            if (poDriver == null)
                return;
            string pathName = strFolderName + "\\"+strLayerName;
            string lyrName = Path.GetFileNameWithoutExtension(pathName);
            DataSource poDS = poDriver.CreateDataSource(strFolderName, null);
            if (poDS == null)
                return;
            wkbGeometryType gmttype;
            string LayerType = null;
            switch (type)
            {
                case VectorLayerType.LineLayer:
                    LayerType = "(线)";
                    gmttype = wkbGeometryType.wkbLineString;
                    break;
                case VectorLayerType.PointLayer:
                    LayerType = "(点)";
                    gmttype = wkbGeometryType.wkbPoint;
                    break;
                case VectorLayerType.PolygonLayer:
                    LayerType = "(面)";
                    gmttype = wkbGeometryType.wkbPolygon;
                    break;
                default:
                    LayerType = "(未知)";
                    gmttype = wkbGeometryType.wkbNone;
                    break;
            }
            string m_lyrName = (ismix) ? lyrName + LayerType : lyrName;
            OSGeo.OGR.Layer poLayer = poDS.CreateLayer(m_lyrName, null, gmttype, null);

            for (int i = 1; i <= DataTable.Columns.Count - 1; i++)
            {
                string colName = DataTable.Columns[i].ColumnName;
                OSGeo.OGR.FieldDefn oField = null;
                Type dType = DataTable.Columns[i].DataType;
                if (dType == typeof(Int32) ||
                    dType == typeof(Int64) ||
                    dType == typeof(Int16))
                {

                    oField = new FieldDefn(colName, FieldType.OFTInteger);

                }
                else if (dType == typeof(double) ||
                    dType == typeof(float))
                {
                    oField = new FieldDefn(colName, FieldType.OFTReal);
                }
                else if (dType == typeof(string))
                {
                    oField = new FieldDefn(colName, FieldType.OFTString);
                }
                poLayer.CreateField(oField, i);
            }

            for (int i = 0; i < m_DataTable.Count; i++)
            {
                GeoDataRow row = m_DataTable[i];
                if ((int)row.EditState >= 6)
                    continue;
                OSGeo.OGR.Feature poFeature = new OSGeo.OGR.Feature(poLayer.GetLayerDefn());
                OSGeo.OGR.Geometry ogrgtr = null;
                GIS.Geometries.Geometry gtr = row.Geometry;
                if (gtr == null)
                    continue;

                string wkt = GIS.Converters.WellKnownText.GeometryToWKT.Write(gtr);
                ogrgtr = OSGeo.OGR.Geometry.CreateFromWkt(wkt);

                #region 废弃
                /*
                if (type == VectorLayerType.PointLayer)
                {
                    if (!(gtr is GeoPoint) && !(gtr is GeoMultiPoint))
                        continue;
                }
                else if (type == VectorLayerType.LineLayer)
                {
                    if (!(gtr is GeoLineString) && !(gtr is GeoMultiLineString))
                        continue;
                }
                else if (type == VectorLayerType.PolygonLayer)
                {
                    if (!(gtr is GeoPolygon) && !(gtr is GeoMultiPolygon))
                        continue;
                }
                if (gtr is GeoPoint)
                {
                    GeoPoint pt = (GeoPoint)gtr;
                    ogrgtr = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPoint);
                    ogrgtr.AddPoint_2D(pt.X, pt.Y);
                }
                else if (gtr is GeoLineString)
                {
                    GeoLineString pl = (GeoLineString)gtr;
                    ogrgtr = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLineString);
                    for (int j = 0; j < pl.Vertices.Count; j++)
                    {
                        ogrgtr.AddPoint_2D(pl.Vertices[j].X, pl.Vertices[j].Y);
                    }
                }
                else if (gtr is GeoPolygon)
                {
                    GeoPolygon plg = (GeoPolygon)gtr;
                    ogrgtr = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);
                    OSGeo.OGR.Geometry ogrexter = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                    for (int j = 0; j < plg.ExteriorRing.Vertices.Count; j++)
                    {
                        ogrexter.AddPoint_2D(plg.ExteriorRing.Vertices[j].X, plg.ExteriorRing.Vertices[j].Y);
                    }
                    ogrgtr.AddGeometry(ogrexter);

                    for (int j = 0; j < plg.InteriorRings.Count; j++)
                    {
                        OSGeo.OGR.Geometry ogrinter = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                        for (int k = 0; k < plg.InteriorRings[j].Vertices.Count; k++)
                        {
                            ogrinter.AddPoint_2D(plg.InteriorRings[j].Vertices[k].X, plg.InteriorRings[j].Vertices[k].Y);
                        }
                        ogrgtr.AddGeometry(ogrinter);
                    }
                }
                else if (gtr is GeoMultiPolygon)
                { 
                    GeoMultiPolygon plg=(GeoMultiPolygon)gtr;
                    ogrgtr = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbMultiPolygon);

                    for (int i = 0; i < plg.NumGeometries; i++)
                    {
                        
                    }

                }
                else if (gtr is GeoMultiLineString)
                {
                    GeoMultiLineString lines = (GeoMultiLineString)gtr;
                    ogrgtr = new OSGeo.OGR.Geometry(wkbGeometryType.wkbMultiLineString);

                    for (int l = 0; l < lines.NumGeometries; l++)
                    {
                        OSGeo.OGR.Geometry line = new OSGeo.OGR.Geometry(wkbGeometryType.wkbLineString);
                        for (int k = 0; k < lines.LineStrings[l].NumPoints; k++)
                        {
                            GeoPoint ptTemp = lines.LineStrings[l].Vertices[k];
                            line.AddPoint_2D(ptTemp.X, ptTemp.Y);
                        }
                        ogrgtr.AddGeometry(line);
                    }
                }
                else if (gtr is GeoMultiPoint)
                {
                    GeoMultiPoint points = gtr as GeoMultiPoint;
                    ogrgtr = new OSGeo.OGR.Geometry(wkbGeometryType.wkbMultiPoint);
                    for (int l = 0; l < points.NumGeometries; l++)
                    {
                        OSGeo.OGR.Geometry pt = new OSGeo.OGR.Geometry(wkbGeometryType.wkbPoint);
                        pt.AddPoint_2D(points[l].X, points[l].Y);
                        ogrgtr.AddGeometry(pt);
                    }
                }
                else
                    continue;
                 */
                #endregion

                poFeature.SetGeometry(ogrgtr);

                for (int m = 1; m <= m_DataTable.Columns.Count - 1; m++)
                {
                    FieldType ftype = poFeature.GetFieldType(m - 1);
                    if (ftype == OSGeo.OGR.FieldType.OFTInteger)
                    {
                        if (row[m] == DBNull.Value)
                            continue;
                        poFeature.SetField(m - 1, System.Convert.ToInt32(row[m]));
                    }
                    else if (ftype == OSGeo.OGR.FieldType.OFTReal)
                    {
                        if (row[m] == DBNull.Value)
                            continue;
                        poFeature.SetField(m - 1, System.Convert.ToDouble(row[m]));
                    }
                    else if (ftype == OSGeo.OGR.FieldType.OFTString)
                    {
                        if (row[m] == DBNull.Value)
                            continue;
                        poFeature.SetField(m - 1, System.Convert.ToString(row[m]));
                    }
                    else
                    {
                        if (row[m] == DBNull.Value)
                            continue;
                        poFeature.SetField(m - 1, System.Convert.ToString(row[m]));  //default to string??
                    }

                }
                poLayer.CreateFeature(poFeature);
                poFeature.Dispose();

            }

            poDS.Dispose();
            poDriver.Dispose();
        }

        public void SaveLayerAsShapeFile(string strFolderName, string strLayerName)
        {
            if (VectorType == VectorLayerType.LabelLayer)
                return;
            if (m_DataTable.Count == 0 || !m_DataTable.FillData)
                return;
            if (VectorType != VectorLayerType.MixLayer && VectorType != VectorLayerType.DraftLayer)
            {
                SaveFileAsShapeFileOfType(strFolderName, strLayerName, VectorType, false);
            }
            else
            {
                SaveFileAsShapeFileOfType(strFolderName, strLayerName,VectorLayerType.PointLayer, true);
                SaveFileAsShapeFileOfType(strFolderName, strLayerName, VectorLayerType.LineLayer, true);
                SaveFileAsShapeFileOfType(strFolderName, strLayerName, VectorLayerType.PolygonLayer, true);
            }
        }
        
        public void SaveLayerAs(string strFolderName, string strLayerName)
        {
            string path = strFolderName + "\\" + strLayerName;
            string extName = Path.GetExtension(path);
            if (extName == ".shp")
            {
                SaveLayerAsShapeFile(strFolderName, strLayerName);
            }
            else if (extName == ".LQ")
            {
                SaveLayerAsLQFile(strFolderName, strLayerName);
            }
        }

        public void SaveLayerAsLQFile(string strFolderName,string strLayerName)
        {
            if (m_DataTable.Count == 0 || !m_DataTable.FillData)
                return; 
       
            string pathName = strFolderName + "\\" + strLayerName ;
            FileStream fs = new System.IO.FileStream(pathName, FileMode.Create, FileAccess.Write);
            StreamWriter fw = new System.IO.StreamWriter(fs, Encoding.Default);
            fw.WriteLine("HeadBegin");
            fw.WriteLine("Version:1.0");
            fw.WriteLine("Unit:M");
            fw.WriteLine("MinX:{0}\r\nMinY:{1}\r\nMaxX:{2}\r\nMaxY:{3}",
                LayerBound.Left, LayerBound.Bottom, LayerBound.Right, LayerBound.Top);
            fw.WriteLine("MinZ:-999999.9999\r\nMaxZ:99999999.9999");
            fw.WriteLine("ScaleM:500");
            DateTime time = DateTime.Today;
            fw.WriteLine("Date:{0}{1:d2}{2:d2}", time.Year.ToString(), int.Parse(time.Month.ToString()), int.Parse(time.Day.ToString()));
            fw.WriteLine("Separator:,");
            fw.WriteLine("HeadEnd");


            fw.WriteLine("TableStructureBegin");
            fw.WriteLine("{0},{1},{2:d}", LayerName, DataTable.Columns.Count, LayerTypeDetail);
            fw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                ClasID,m_Style.SymbolSize,m_Style.LineSize,
                m_Style.LineColor.A, m_Style.LineColor.R, m_Style.LineColor.G, m_Style.LineColor.B,
                m_Style.FillColor.A, m_Style.FillColor.R, m_Style.FillColor.G, m_Style.FillColor.B);
            int nColumnCount = DataTable.Columns.Count;
            for (int k = 0; k < nColumnCount; k++)
            {
                string colName = DataTable.Columns[k].ColumnName;
                Type type = DataTable.Columns[k].DataType; 
                fw.WriteLine("{0},{1}", colName, type.ToString());
            } 
            fw.WriteLine("TableStructureEnd"); 
            fw.WriteLine("GeometryBegin");
            int nValidGeomCount = 0;
            for (int i = 0; i < DataTable.Count; i++)
            {
                GeoDataRow row = DataTable[i];

                if ((row.EditState == EditState.Invalid)||row.Geometry == null)
                {
                    continue;
                }
                try
                {
                    row.Geometry.WriteToLQFile(fw);
                    nValidGeomCount++;
                }
                catch
                {
                }
            }
            fw.WriteLine("GeometryEnd");

            fw.WriteLine("AttributeBegin");
            int nValidAttriCount = 0;
            for (int i = 0; i < DataTable.Count; i++)
            {
                GeoDataRow row = DataTable[i];
                if (row.EditState == EditState.Invalid ||row.Geometry == null)
                {
                    continue;
                }
                try
                {
                    for (int k = 0; k < nColumnCount; k++)
                    {
                        fw.Write(row[k]);
                        if (k < nColumnCount - 1)
                        {
                            fw.Write(",");
                        }
                    }
                    fw.Write("\r\n");
                    nValidAttriCount++;
                }
                catch
                {
                }
            }
            fw.WriteLine("AttributeEnd");
            fw.WriteLine("{0},{1}", nValidGeomCount, nValidAttriCount);
            fw.Close();
            fs.Close();
        } 
        #endregion

    }
}
