using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using GIS.GeoData;
using GIS.Layer;
namespace GIS.Increment
{
    public class IncDisappear:IncBase
    {
        public IncDisappear( GeoDataRow DisappearRow)
            :base()
        {
            DisappearRow.EditState = EditState.Disappear;
            ChangeInfo info = new ChangeInfo();
            info.ChangeType = ChangeType.CHANGE_DISAPPEAR;
            info.ChangeBefInfo = new List<GeoDataRow>();
            info.ChangeBefInfo.Add(DisappearRow);
            m_ChangeInfoList.Add(info);
        }
        public IncDisappear(List<GeoDataRow> DisappearRows)
            : base()
        {
            for (int i = 0; i < DisappearRows.Count; i++)
            {
                DisappearRows[i].EditState = EditState.Disappear;
                ChangeInfo info = new ChangeInfo(); 
                info.ChangeType = ChangeType.CHANGE_DISAPPEAR;
                info.ChangeBefInfo = new List<GeoDataRow>();
                info.ChangeBefInfo.Add(DisappearRows[i]);
                m_ChangeInfoList.Add(info);
            }             
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
                GeoDataRow row = info.ChangeBefInfo[0];
                GeoLayer lyr = ((GeoDataTable)row.Table).BelongLayer;
                if (lyr != layer)
                    return;
                int colNums = row.Table.Columns.Count;
                for (int j = 0; j < colNums; j++)
                {
                    fw.Write(row[j].ToString());
                    if (j != colNums - 1)
                        fw.Write(",");
                }
                fw.Write("\r\n");
            }
        }
        public static void WriteDisappearGeoInfo(System.IO.StreamWriter sw, GeoVectorLayer lyr, GeoDataRow row)
        {
            sw.WriteLine("{0:d}",ChangeType.CHANGE_DISAPPEAR);

            sw.WriteLine(row["FeatID"].ToString());
            sw.WriteLine(row["ClasID"].ToString());
            sw.WriteLine(lyr.LayerName);
            sw.WriteLine(row.Geometry.Area);
            sw.WriteLine(row.Geometry.Length);
            sw.WriteLine("0");
        }
        public override void WriteGeoInfo(System.IO.StreamWriter sw, GIS.Layer.VectorLayerType type, GIS.Layer.GeoLayer layer)
        {
            for (int i = 0; i < m_ChangeInfoList.Count; i++)
            {
                ChangeInfo info = m_ChangeInfoList[i];
                GeoDataRow row = info.ChangeBefInfo[0];
                GeoLayer lyr = ((GeoDataTable)row.Table).BelongLayer;
                Geometry geom = row.Geometry;
                if (!ChangeInfoJudge(geom, type) || lyr != layer)
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
              
            }
        }
    }
}
