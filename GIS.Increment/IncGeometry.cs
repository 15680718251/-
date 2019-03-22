using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using GIS.Layer;
using GIS.GeoData;
namespace GIS.Increment
{
    public class IncGeometry:IncBase
    {
        public IncGeometry(GeoDataRow changeBef, GeoDataRow changeAft)
            :base()
        {
            changeBef.EditState = EditState.GeometryBef;
            changeAft.EditState = EditState.GeometryAft;
            ChangeInfo info = new ChangeInfo();
            info.ChangeType = ChangeType.CHANGE_GEOMETRY;
            info.ChangeBefInfo = new List<GeoDataRow>();
            info.ChangeAftInfo = new List<GeoDataRow>();
            info.ChangeBefInfo.Add(changeBef);
            info.ChangeAftInfo.Add(changeAft);
            m_ChangeInfoList.Add(info);
        }
        public override void AddChangeInfoToList(List<ChangeInfo> list, GIS.Layer.VectorLayerType type)
        {
            for (int i = 0; i < m_ChangeInfoList.Count; i++)
            {
                ChangeInfo info = m_ChangeInfoList[i];
                Geometry geom = info.ChangeBefInfo[0].Geometry;
                if (ChangeInfoJudge(geom, type))
                {
                    list.Add(info);
                }
            }
        }
        public override void WriteAttributeInfo(System.IO.StreamWriter fw, GeoLayer layer)
        {
            for (int i = 0; i < m_ChangeInfoList.Count; i++)
            {
                ChangeInfo info = m_ChangeInfoList[i];
                ///////////变化前的属性
                GeoDataRow rowBef = info.ChangeBefInfo[0];
                GeoLayer lyr = ((GeoDataTable)rowBef.Table).BelongLayer;
                if (lyr != layer)
                    return;
                int colNums = rowBef.Table.Columns.Count;
                for (int j = 0; j < colNums; j++)
                {
                    fw.Write(rowBef[j].ToString());
                    if (j != colNums - 1)
                        fw.Write(",");
                }
                fw.Write("\r\n");

                ///////////变化后的属性
                GeoDataRow row = info.ChangeAftInfo[0];
                
                if (lyr != layer)
                    return;
                colNums = row.Table.Columns.Count;
                for (int j = 0; j < colNums; j++)
                {
                    fw.Write(row[j].ToString());
                    if (j != colNums - 1)
                        fw.Write(",");
                }
                fw.Write("\r\n");
            }
        }
      
        public override void WriteGeoInfo(System.IO.StreamWriter sw, GIS.Layer.VectorLayerType type, GIS.Layer.GeoLayer layer)
        {
            for (int i = 0; i < m_ChangeInfoList.Count; i++)
            {
                ChangeInfo info = m_ChangeInfoList[i];
                GeoDataRow rowAft = info.ChangeAftInfo[0];
                
                GeoLayer lyr = ((GeoDataTable)rowAft.Table).BelongLayer;
                if (!ChangeInfoJudge(rowAft.Geometry, type) || lyr != layer)
                    return;

                try
                {
                    sw.WriteLine("{0:d}", info.ChangeType);
                    GeoDataRow rowBef = info.ChangeBefInfo[0];//变化前的信息


                    sw.WriteLine(rowBef["FeatID"].ToString());
                    sw.WriteLine(rowBef["ClasID"].ToString());
                    sw.WriteLine(lyr.LayerName);
                    if (type == VectorLayerType.PolygonLayer)
                    {
                        sw.WriteLine(",");
                    }
                    rowBef.Geometry.WriteGeoInfo(sw);

                    sw.WriteLine(rowAft["FeatID"].ToString());
                    sw.WriteLine(rowAft["ClasID"].ToString());
                    sw.WriteLine(lyr.LayerName);
                    if (type == VectorLayerType.PolygonLayer)
                    {
                        sw.WriteLine(",");
                    }
                    rowAft.Geometry.WriteGeoInfo(sw);

                    sw.WriteLine("0");

                }

                catch (Exception e)
                {
                    string m = e.Message;
                }
              

            }
        }

        public override void AddGeomToList(List<GeoDataRow> list)
        {
            for (int i = 0; i < m_ChangeInfoList.Count; i++)
            {
                ChangeInfo info = m_ChangeInfoList[i];
                GeoDataRow row = info.ChangeAftInfo[0];
                list.Add(row);
            }
        }








        internal static void WriteGeoBefAftInfo(System.IO.StreamWriter sw, GeoVectorLayer lyr, GeoDataRow rowBef, GeoDataRow rowAft)
        {
            sw.WriteLine("{0:d}", ChangeType.CHANGE_GEOMETRY);
            sw.WriteLine(rowBef["FeatID"].ToString());
            sw.WriteLine(rowBef["ClasID"].ToString());
            sw.WriteLine(lyr.LayerName);


             int colNums = rowBef.Table.Columns.Count;
            //for (int j = 0; j < colNums; j++)
            //{
            //    sw.Write(rowBef[j].ToString());
            //    if (j != colNums - 1)
            //        sw.Write(",");
            //}
            //sw.Write("\r\n");

            //sw.WriteLine(rowAft["FeatID"].ToString());
            //sw.WriteLine(rowAft["ClasID"].ToString());
            //sw.WriteLine(lyr.LayerName);

            double areaChange = rowAft.Geometry.Area - rowBef.Geometry.Area;
            if (Math.Abs(areaChange) < Geometry.EPSIONAL)
                areaChange = 0;
            double lenChange = rowAft.Geometry.Length - rowBef.Geometry.Length;
            if (Math.Abs(lenChange) < Geometry.EPSIONAL)
                lenChange = 0;
            sw.WriteLine(areaChange);
            sw.WriteLine(lenChange);

            for (int j = 0; j < colNums; j++)
            {
                sw.Write(rowAft[j].ToString());
                if (j != colNums - 1)
                    sw.Write(",");
            }
            sw.Write("\r\n");

            rowAft.Geometry.WriteGeoInfo(sw);

            sw.WriteLine("0");
        }
        public static void WriteGeoChangedGeoInfo(System.IO.StreamWriter sw, GeoVectorLayer lyr, GeoDataRow row)
        {
            for (int i = 0; i < lyr.DataTable.Count; i++)
            {
                GeoData.GeoDataRow rowBef = lyr.DataTable[i];

                if (rowBef.EditState == EditState.GeometryBef
                    && rowBef["FeatID"].ToString() == row["FeatID"].ToString())
                {
                    if (!rowBef.Geometry.IsEqual(row.Geometry))
                    {
                        rowBef.EditState = EditState.GeometryBef;

                        WriteGeoBefAftInfo(sw, lyr, rowBef, row);
                        return;
                    }
                    else
                    {
                        rowBef.EditState = EditState.Original;
                        row.EditState = EditState.Invalid;
                        return;
                    }
                }
            }

            row.EditState = EditState.Appear;
            IncAppear.WriteGeoAppearInfo(sw, lyr, row);

        }
    }
}
