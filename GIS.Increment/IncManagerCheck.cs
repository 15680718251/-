using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using GIS.Map;
using GIS.Layer;
namespace GIS.Increment
{
    public partial class IncManager
    {
        public class CheckRow
        {
            public CheckRow(string type, Int64 clasID)
            {
                m_changetype = type;
                m_ClassID = clasID;
                m_rows = new List<GIS.GeoData.GeoDataRow>();
            }
            public void AddRow(GeoData.GeoDataRow row, double iArea, double iLen, double dArea, double dLen)
            {
                m_rows.Add(row);
                m_IncArea += iArea;
                m_IncLength += iLen;
                m_DecArea += dArea;
                m_DecLength += dLen;
            }
            public List<GeoData.GeoDataRow> m_rows;
            public string m_changetype;
            public Int64 m_ClassID;
            public double m_IncArea = 0;
            public double m_IncLength = 0;
            public double m_DecArea = 0;
            public double m_DecLength = 0;

        }
       public List<CheckRow> m_UpdateCheckDatas = new List<CheckRow>();

        private void AddCheckRow(string type, GeoData.GeoDataRow row, double area, double len)
        {
            CheckRow checkrow = ChangeTypeExist(row, type);
            if (checkrow == null)
            {
                Int64 clasID;
                if (row["ClasID"] == DBNull.Value)
                    clasID = 0;
                else
                    clasID = Int64.Parse(row["ClasID"].ToString());
                checkrow = new CheckRow(type, clasID);
                m_UpdateCheckDatas.Add(checkrow);
            }
            double a, b, c, d;
            a = b = c = d = 0;
            if (area > 0)
                a = area;
            else
                c = -area;
            if (len > 0)
                b = len;
            else
                d = -len;
            checkrow.AddRow(row, a, b, c, d);       
     
        }
        private CheckRow ChangeTypeExist(GeoData.GeoDataRow row,string type)
        {
            for (int i = 0; i < m_UpdateCheckDatas.Count; i++)
            {
                CheckRow crow = m_UpdateCheckDatas[i];
                if (row["ClasID"] == DBNull.Value)
                    continue;
                if ((row["ClasID"].ToString() == crow.m_ClassID.ToString())
                    && crow.m_changetype == type)
                {
                    return crow;
                }
            }
            return null;
        }


        private string GetChangeType(GeoData.GeoDataRow row)
        {
            EditState state = row.EditState;
            switch (state)
            {
                case EditState.Appear:
                    return "出现";
                case EditState.GeometryAft:
                    return "几何变化";
                case EditState.AttributeAft:
                    return "属性变化";
                default:
                    return "消失";
            }
        }
        public bool UpdateCheck()
        {
            if (m_Map == null || m_Map.MapBound == null)
                return false;
            if (!FeatIDCheckValid())
                return false;
            m_UpdateCheckDatas.Clear();
            for (int i = 0; i < m_Map.LayerCounts; i++)
            {
                GeoVectorLayer lyr = m_Map.GetLayerAt(i) as GeoVectorLayer;
                if (lyr == null || ((int)lyr.LayerTypeDetail >= 9))
                    continue;

                for (int j = 0; j < lyr.DataTable.Count; j++)
                {
                    GeoData.GeoDataRow row = lyr.DataTable[j];
                    if ((int)row.EditState > 0 && (int)row.EditState < 7)
                    {
                        if (row.EditState == EditState.AttributeAft)
                        {
                            CheckAttriChanged(lyr, row);
                        }
                        else if (row.EditState == EditState.Appear || row.EditState == EditState.GeometryAft)
                        {
                            CheckAppearChanged(lyr, row);
                        }
                    }
                }
                for (int j = 0; j < lyr.DataTable.Count; j++)
                {
                    GeoData.GeoDataRow row = lyr.DataTable[j];

                    if (row.EditState == EditState.Disappear)
                    {
                        AddCheckRow("消失", row, -row.Geometry.Area,-row.Geometry.Length);
                    }
                }         
            }
            return true;
        }

        private void CheckAppearChanged(GeoVectorLayer lyr, GIS.GeoData.GeoDataRow row)
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
                        AddCheckRow("几何变化", row, row.Geometry.Area - rowBef.Geometry.Area, row.Geometry.Length - rowBef.Geometry.Length);
                    }
                    else
                    {
                        for (int j = 0; j < lyr.DataTable.Columns.Count; j++)
                        {
                            if (row[j].ToString() != rowBef[j].ToString())
                            {
                                rowBef.EditState = EditState.AttributeBef;
                                row.EditState = EditState.AttributeAft;
                                AddCheckRow("属性变化", row, 0, 0);
                                return;
                            }
                        }
                        row.EditState = EditState.Original;
                    }
                }
            }
            if (isAppear)
            {
                AddCheckRow("出现", row, row.Geometry.Area, row.Geometry.Length);
            }
        }

        private void CheckAttriChanged(GeoVectorLayer lyr,GeoData.GeoDataRow row)
        {

            for (int i = 0; i < lyr.DataTable.Count; i++)
            {
                GeoData.GeoDataRow rowBef = lyr.DataTable[i];

                if (rowBef.EditState == EditState.AttributeBef
                    && rowBef["FeatID"].ToString() == row["FeatID"].ToString())
                {
                    System.Data.DataTable table = row.Table;
                    bool isSame = true;
                    for (int j = 1; j < table.Columns.Count - 1; j++)
                    {
                        if (table.Columns[j].ColumnName == "BeginTime")
                            continue;
                        if (rowBef[j].ToString() != rowBef[j].ToString())
                        {
                            isSame = false;
                            break;
                        }
                    }
                    if (isSame)
                    {
                        row.EditState = EditState.Invalid;
                        rowBef.EditState = EditState.Original;
                        return;
                    }

                    rowBef.EditState = EditState.AttributeBef;
                    AddCheckRow("属性变化", row, 0, 0);
                    return;
                }
            }

            row.EditState = EditState.Appear;
            AddCheckRow("出现", row, row.Geometry.Area,row.Geometry.Length);
        
        }


        public bool CheckIncFile(List<string> results)
        {
            if (m_UpdateCheckDatas.Count != results.Count)           
                return false;

            for (int i = 0; i < results.Count; i++)
            {
                string[] array = results[i].Split(',');
                Int64 clasID = Int64.Parse(array[2]);
                bool valid = false;
                for (int j = 0; j < m_UpdateCheckDatas.Count; j++)
                {
                    CheckRow row = m_UpdateCheckDatas[j];
                    if (row.m_ClassID == clasID &&
                        row.m_changetype == array[3] &&
                        row.m_IncArea.ToString("f3") == array[5] &&
                        row.m_IncLength.ToString("f3") == array[6] &&
                        row.m_DecArea.ToString("f3") == array[7] &&
                        row.m_DecLength.ToString("f3") == array[8])
                    {
                        valid = true;
                    }
                }
                if (!valid) 
                    return false;
            }
            return true;
        }
    }
}
