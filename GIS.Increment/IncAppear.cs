using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using GIS.GeoData;
using GIS.Layer;
namespace GIS.Increment
{
    public class IncAppear:IncBase
    {
        public IncAppear(GeoDataRow AppearRow)
            : base()
        {
            AppearRow.EditState = EditState.Appear;
            ChangeInfo info = new ChangeInfo();
            info.ChangeType = ChangeType.CHANGE_APPEAR;
            info.ChangeAftInfo = new List<GeoDataRow>();
            info.ChangeAftInfo.Add(AppearRow);
            m_ChangeInfoList.Add(info);
        }

        public override void AddChangeInfoToList(List<ChangeInfo> list, GIS.Layer.VectorLayerType type)
        {
            ChangeInfo info = m_ChangeInfoList[0];
            Geometry geom = info.ChangeAftInfo[0].Geometry;
            if(ChangeInfoJudge(geom,type))
            {
                list.Add(info);
            }
        }
        public override void WriteAttributeInfo(System.IO.StreamWriter fw, GeoLayer layer)
        { 
             ChangeInfo info = m_ChangeInfoList[0];
             GeoDataRow row = info.ChangeAftInfo[0];
             GeoLayer lyr = ((GeoDataTable)row.Table).BelongLayer;
             if (lyr != layer
                 || row.EditState == EditState.Invalid)
                 return;
             int colNums = row.Table.Columns.Count;
             for (int i = 0; i < colNums; i++)
             {
                 fw.Write(row[i].ToString());
                 if (i != colNums - 1)
                     fw.Write(",");
             }
             fw.Write("\r\n");
        }
        public override void WriteGeoInfo(System.IO.StreamWriter sw, VectorLayerType type, GeoLayer layer)
        {
            ChangeInfo info = m_ChangeInfoList[0];
            GeoDataRow row = info.ChangeAftInfo[0];
          
            GeoLayer lyr =  ((GeoDataTable)row.Table).BelongLayer;
            Geometry geom = row.Geometry;
            if (!ChangeInfoJudge(geom, type) 
                || lyr != layer
                || row.EditState == EditState.Invalid)
                return;
            try
            {
                sw.WriteLine("{0:d}", info.ChangeType);
              
                sw.WriteLine(row["FeatID"].ToString());
                sw.WriteLine(row["ClasID"].ToString());
                sw.WriteLine(lyr.LayerName);
                if (type == VectorLayerType.PolygonLayer)
                {
                    sw.WriteLine(",");
                }
                geom.WriteGeoInfo(sw);
                sw.WriteLine("0");

            }
            catch (Exception e)
            {
                string m = e.Message;
            }
            finally
            {
            }
        }
        public override void AddGeomToList(List<GeoDataRow> list)
        {
            ChangeInfo info = m_ChangeInfoList[0];
            GeoDataRow row = info.ChangeAftInfo[0];
            list.Add(row);
        }
        internal static void WriteGeoAppearInfo(System.IO.StreamWriter sw, GeoVectorLayer lyr, GeoDataRow row)
        {
            sw.WriteLine("{0:d}", ChangeType.CHANGE_APPEAR);
            sw.WriteLine(row["FeatID"].ToString());
            sw.WriteLine(row["ClasID"].ToString());
            sw.WriteLine(lyr.LayerName);
            sw.WriteLine(row.Geometry.Area);
            sw.WriteLine(row.Geometry.Length);

            int colNums = row.Table.Columns.Count;
            for (int j = 0; j < colNums; j++)
            {
                string test = row[j].ToString();
                sw.Write(test);
                if (j != colNums - 1)
                    sw.Write(",");
            }
            sw.Write("\r\n");
            row.Geometry.WriteGeoInfo(sw);
            sw.WriteLine("0");
        }
        public static void WriteAppearGeoInfo(System.IO.StreamWriter sw, GeoVectorLayer lyr, GeoDataRow row)
        {
            bool isAppear = true;
            for (int i = 0; i < lyr.DataTable.Count; i++)
            {
                GeoData.GeoDataRow rowBef = lyr.DataTable[i];

                if (rowBef["FeatID"].ToString() == row["FeatID"].ToString()
                    &&
                    (rowBef.EditState == EditState.GeometryBef ||
                     rowBef.EditState == EditState.AttributeBef ||
                     rowBef.EditState == EditState.Disappear))
                {
                    isAppear = false;
                    if (!rowBef.Geometry.IsEqual(row.Geometry))
                    {
                        rowBef.EditState = EditState.GeometryBef;
                        row.EditState = EditState.GeometryAft;
                        IncGeometry.WriteGeoBefAftInfo(sw, lyr, rowBef, row);
                    }
                    else
                    {
                        for (int j = 0; j < lyr.DataTable.Columns.Count; j++)
                        {
                            if (row[j].ToString() != rowBef[j].ToString())
                            {
                                rowBef.EditState = EditState.AttributeBef;
                                row.EditState = EditState.AttributeAft;
                                IncAttribute.WriteAttriBefAftInfo(sw, lyr, rowBef, row);
                                return;
                            }
                        }
                        row.EditState = EditState.Original;
                    }
                }
            }
            if (isAppear)
            {
                WriteGeoAppearInfo(sw, lyr, row);
            }
        }
     
    }
}
