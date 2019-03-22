using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;
using GIS.Map;
using GIS.Layer;
using GIS.Geometries;
namespace GIS.Increment
{
    public partial class IncManager
    {

        public List<string> IncManagerfeatId = new List<string>();
        public List<string> IncManagerlyrName = new List<string>();

        public bool ReadChangeFile()
        { 
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "增量信息文件|*.inc";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                // ReadFileHeader(sr);
                ReadTableStruct(sr);
                ReadChangeInfo(sr);              
                sr.Close();
                fs.Close();
                return true;
            }
            return false;
        }
        private bool ReadChangeInfo(StreamReader sr)
        {
             sr.ReadLine();
             string strtemp = sr.ReadLine();
             while (strtemp != null)
             {
                 if (strtemp == "RecordEnd")
                 {
                     return true;
                 }
                 else
                 {
                     switch (strtemp)
                     {
                         case "1":
                             if (!ReadAppearFeature(sr))
                                 return false;
                             break;
                         case "2":
                             if (!ReadDisappearFeature(sr))
                                 return false;
                             break;
                         case "3":
                             if (!ReadGeomChangeFeature(sr))
                                 return false;
                             break; 
                         case "4":
                             if (!ReadAttriChangeFeature(sr))
                                 return false;
                             break;                             
                     }
                 }
                 sr.ReadLine();
                 strtemp = sr.ReadLine();
             }
             return false;
        }
        private Geometry ReadGeomInfo(StreamReader sr)
        {
            string strTemp = sr.ReadLine();
           
            Geometry geom = null;
            switch (strTemp)
            {
                case "GeoPoint":
                    geom = new GeoPoint();
                    break;
                case "GeoLineString":
                    geom = new GeoLineString();
                    break;
                case "GeoPolygon":
                    geom = new GeoPolygon();
                    break;
                case "GeoArc":
                    geom = new GeoArc();
                    break;
                case "GeoSpline":
                    geom = new GeoSpline();
                    break;
                case "GeoMultiPoint":
                    geom = new GeoMultiPoint();
                    break;
                case "GeoMultiLineString":
                    geom = new GeoMultiLineString();
                    break;
                case "GeoMultiPolygon":
                    geom = new GeoMultiPolygon();
                    break;
                case "GeoLabel":
                    geom = new GeoLabel();
                    break;
            }

            geom.ReadFromLQFile(sr);
            return geom;
             
        }
 
        private bool ReadAppearFeature(StreamReader sr)
        {
            try
            {
                string featID = sr.ReadLine();
                string clasID = sr.ReadLine();
                string lyrName = sr.ReadLine();
                string area = sr.ReadLine();
                string length = sr.ReadLine();
                string strAttri = sr.ReadLine();
                string[] items = strAttri.Split(',');
                GeoVectorLayer lyr = m_Map.GetLayerByName(lyrName) as GeoVectorLayer;             
                Geometry geom = ReadGeomInfo(sr);
                GeoData.GeoDataRow row = lyr.AddGeometry(geom);
                for (int i = 1; i < items.Length; i++)
                {
                    if (items[i].ToString() == "")
                        row[i] = DBNull.Value;
                    else
                        row[i] = items[i].ToString();
                }
                row.EditState = EditState.Appear;

                m_Map.AddSltObj(row);


                IncManagerfeatId.Add(featID);
                IncManagerlyrName.Add(lyrName);



                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool ReadDisappearFeature(StreamReader sr)
        {
            try
            {
                string featID = sr.ReadLine();
                string clasID = sr.ReadLine();
                string lyrName = sr.ReadLine();
                string area = sr.ReadLine();
                string length = sr.ReadLine();
                GeoVectorLayer lyr = m_Map.GetLayerByName(lyrName) as GeoVectorLayer;
                string filter = string.Format("FeatID = '{0}'", featID);
                DataRow[] rowsFind = lyr.DataTable.Select(filter);
                if (rowsFind.Length != 1)
                {
                    throw new Exception("没有找到啦，怎么办呀？");
                }
                GeoData.GeoDataRow row = rowsFind[0] as GeoData.GeoDataRow;
                row.EditState = EditState.Disappear;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private bool ReadAttriChangeFeature(StreamReader sr)
        {
            try
            {
                string featID = sr.ReadLine();
                string clasID = sr.ReadLine();
                string lyrName = sr.ReadLine();
                string area = sr.ReadLine();
                string length = sr.ReadLine();
                string strAttri = sr.ReadLine();
                string[] items = strAttri.Split(',');

                GeoVectorLayer lyr = m_Map.GetLayerByName(lyrName) as GeoVectorLayer;
                string filter = string.Format("FeatID = '{0}'", featID);
                DataRow[] rowsFind = lyr.DataTable.Select(filter);
                if (rowsFind.Length != 1)
                {
                    throw new Exception("没有找到啦，怎么办呀？");
                }
                GeoData.GeoDataRow rowBef = rowsFind[0] as GeoData.GeoDataRow;
                rowBef.EditState = EditState.AttributeBef;


                GeoData.GeoDataRow row = lyr.AddGeometry(rowBef.Geometry.Clone());
                for (int i = 1; i < items.Length; i++)
                {
                    if (items[i].ToString() == "")
                        row[i] = DBNull.Value;
                    else
                        row[i] = items[i].ToString();
                }
                row.EditState = EditState.AttributeAft;

                m_Map.AddSltObj(row);

                IncManagerfeatId.Add(featID);
                IncManagerlyrName.Add(lyrName);


                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ReadGeomChangeFeature(StreamReader sr)
        {
            try
            {
                string featID = sr.ReadLine();
                string clasID = sr.ReadLine();
                string lyrName = sr.ReadLine();
                string area = sr.ReadLine();
                string length = sr.ReadLine();
                string strAttri = sr.ReadLine();
                string[] items = strAttri.Split(',');
                GeoVectorLayer lyr = m_Map.GetLayerByName(lyrName) as GeoVectorLayer;
                string filter = string.Format("FeatID = '{0}'", featID);
                DataRow[] rowsFind = lyr.DataTable.Select(filter);
                if (rowsFind.Length != 1)
                {
                    throw new Exception("没有找到啦，怎么办呀？");
                }
                GeoData.GeoDataRow rowBef = rowsFind[0] as GeoData.GeoDataRow;
                rowBef.EditState = EditState.GeometryBef;

                Geometry geom = ReadGeomInfo(sr);
                GeoData.GeoDataRow row = lyr.AddGeometry(geom);
                for (int i = 1; i < items.Length; i++)
                {
                    if (items[i].ToString() == "")
                        row[i] = DBNull.Value;
                    else
                        row[i] = items[i].ToString();
                }
                row.EditState = EditState.GeometryAft;

                m_Map.AddSltObj(row);

                IncManagerfeatId.Add(featID);
                IncManagerlyrName.Add(lyrName);


                return true;
            }
            catch
            {
                return false;
            }
        }

     

  

 
        private List<GeoLayer> ReadTableStruct(StreamReader sw)
        {
            List<GeoLayer> layerList = new List<GeoLayer>();
            string strTemp;
            do
            {
                strTemp = sw.ReadLine();
            } while (strTemp != null && strTemp != "TableStructureBegin");

            if (strTemp != "TableStructureBegin")
                throw new Exception("错误的表结构");

            strTemp = sw.ReadLine();/////////////////////////////
            while (strTemp != null && strTemp != "TableStructureEnd")
            {
                string[] lyrInfo = strTemp.Split(',');
                string LayerName = lyrInfo[0];
                int FieldCount = int.Parse(lyrInfo[1]);
                LAYERTYPE_DETAIL type = (LAYERTYPE_DETAIL)(int.Parse(lyrInfo[2]));
                GeoLayer lyr = m_Map.GetLayerByName(LayerName);
                if (lyr == null)
                {
                    GeoData.GeoDataTable table = new GIS.GeoData.GeoDataTable();
                    for (int i = 0; i < FieldCount; i++)
                    {
                        strTemp = sw.ReadLine();//////////////////////////
                        string[] fieldInfo = strTemp.Split(',');
                        Type FieldType = null;
                        foreach (var item in m_DictDataType)
                        {
                            if (item.Value == fieldInfo[1])
                            {
                                FieldType = Type.GetType(item.Key);
                                break;
                            }
                        }
                        DataColumn column = new DataColumn(fieldInfo[0], FieldType);
                        table.Columns.Add(column);
                    }

                    GeoLayer layer = null;

                    layer = new GeoVectorLayer();
                    //((GeoVectorLayer)layer).VectorType = (VectorLayerType)((int)type);
                    ((GeoVectorLayer)layer).DataTable = table;

                    layer.LayerName = LayerName;
                    layerList.Add(layer);
                    m_Map.AddLayer(layer);
                    sw.ReadLine();


                }
                else
                {
                    for (int i = 0; i < FieldCount + 1; i++)
                    {
                        sw.ReadLine();
                    }
                }

                strTemp = sw.ReadLine();/////////////////////////////读取下一个的开头
            }


            return layerList;
        }
 
       

    }
}
