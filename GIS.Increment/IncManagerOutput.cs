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
        public bool WriteChangeFile(Dictionary<long, string> ftrCodes)
        {
            UpdateCheck();

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "增量信息文件|*.inc";            
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);

                string checkFileName = Path.GetDirectoryName(dlg.FileName) +"\\"+ Path.GetFileNameWithoutExtension(dlg.FileName) + "检核报表.txt";
                FileStream fs_check = new FileStream(checkFileName, FileMode.Create, FileAccess.Write);
                
                StreamWriter sw = new StreamWriter(fs);
                StreamWriter sw_check = new StreamWriter(fs_check);
                WriteFileHeader(sw);
                WriteTableStruct(sw);
                sw.WriteLine("RecordBegin");
                WriteGeomInfo(sw);
                sw.WriteLine("RecordEnd");
                sw.Close();
                fs.Close();

                WriteCheckInfo(sw_check, ftrCodes);
                sw_check.Close();
                fs_check.Close();

                return true;
            }
            return false;
        }

        private void WriteCheckInfo(StreamWriter sw, Dictionary<long, string> ftrCodes)
        {
            sw.WriteLine("编号   要素类型     要素编码     变化类型     变化个数     新增面积     新增长度     减少面积     减少长度");
            for (int i = 0; i < m_UpdateCheckDatas.Count; i++)
            {
                GIS.Increment.IncManager.CheckRow row = m_UpdateCheckDatas[i];
                Int64 clasid = row.m_ClassID;

                sw.Write((i + 1).ToString().PadRight(7,  ' '));
                string typeName ="";
                if (clasid != 0)
                {
                    ftrCodes.TryGetValue(clasid, out typeName);
                    //typeName = ftrCodes[clasid];
                    if (typeName == null)
                        typeName = "未知类型";
                }
                sw.Write(typeName.PadRight(12, ' ')); 
                sw.Write(clasid.ToString().PadRight(13,  ' ')); 

                sw.Write(row.m_changetype.PadRight(10, ' '));

                sw.Write(row.m_rows.Count.ToString().PadRight(10, ' '));

                sw.Write(row.m_IncArea.ToString("f3").PadRight(15, ' ')); 
                sw.Write(row.m_IncLength.ToString("f3").PadRight(15,  ' '));
                sw.Write(row.m_DecArea.ToString("f3").PadRight(15, ' '));
                sw.Write(row.m_DecLength.ToString("f3").PadRight(15, ' ')+"\r\n"); 
                
            }
        }
        public bool FeatIDCheckValid()
        {
            bool checkpass = true;
            m_Map.ClearAllSelect();
            for (int i = 0; i < m_Map.LayerCounts; i++)
            {
                GeoVectorLayer lyr = m_Map.GetLayerAt(i) as GeoVectorLayer;
                if (lyr == null) 
                    continue;
                if ((int)lyr.LayerTypeDetail > 7)
                    continue;
                for (int j = 0; j < lyr.DataTable.Count-1; j++)
                {
                    GeoData.GeoDataRow rowFirst = lyr.DataTable[j];

                    if ((int)rowFirst.EditState > 3 || rowFirst.EditState == EditState.Original)
                        continue;
                    for (int k = j+1; k < lyr.DataTable.Count; k++)
                    {
                        GeoData.GeoDataRow rowSecond = lyr.DataTable[k];
                        if ((int)rowSecond.EditState > 3)
                            continue;
                        if (rowFirst["FeatID"].ToString() == rowSecond["FeatID"].ToString())
                        {
                            checkpass = false;
                            m_Map.AddSltObj(rowFirst);
                            m_Map.AddSltObj(rowSecond);
                        }
                    }
                }
            }
            if (!checkpass)
                MessageBox.Show("FeatID存在冲突！请修改后完成增量信息输出！");
            return checkpass;
        }
        private void WriteFileHeader(StreamWriter fw)
        {
            fw.WriteLine("HeadBegin");
            fw.WriteLine("DataMark:CNSDTF_VCT CHANGE-ONLY");
            fw.WriteLine("Version:2.0");
            fw.WriteLine("Unit:M");
            fw.WriteLine("Dim:2");
            fw.WriteLine("Topo:0");
            fw.WriteLine("Coordinate:G");
            fw.WriteLine("Projection:Unknow");
            fw.WriteLine("Spheroid:Unknow");
            fw.WriteLine("Parameters:Unknow");
            fw.WriteLine("MinX:{0}\r\nMinY:{1}\r\nMaxX:{2}\r\nMaxY:{3}",
                m_Map.MapBound.Left, m_Map.MapBound.Bottom, m_Map.MapBound.Right, m_Map.MapBound.Top);
            fw.WriteLine("MinZ:-999999.9999\r\nMaxZ:99999999.9999");
            fw.WriteLine("ScaleM:500");
            DateTime time = DateTime.Today;
            fw.WriteLine("Date:{0}{1:d2}{2:d2}", time.Year.ToString(), int.Parse(time.Month.ToString()), int.Parse(time.Day.ToString()));
            fw.WriteLine("Separator:,");
            fw.WriteLine("HeadEnd");
        }
        private void WriteTableStruct(StreamWriter fw)
        {
            fw.WriteLine("FeatureCodeBegin");
            fw.WriteLine("FeatureCodeEnd");
            fw.WriteLine("TableStructureBegin"); 
            for (int j = 0; j < m_Map.LayerCounts; j++)
            {
                GeoVectorLayer layer = m_Map.GetLayerAt(j) as GeoVectorLayer;
                if (layer == null)
                    continue;
                fw.WriteLine("{0},{1},{2:d}", layer.LayerName, layer.DataTable.Columns.Count, layer.LayerTypeDetail);
                for (int k = 0; k < layer.DataTable.Columns.Count; k++)
                {
                    string colName = layer.DataTable.Columns[k].ColumnName;
                    Type type = layer.DataTable.Columns[k].DataType;
                    fw.WriteLine("{0},{1}", colName, m_DictDataType[type.ToString()]);
                }
                fw.WriteLine("TableEnd");
            }

            fw.WriteLine("TableStructureEnd");

        }
        private void WriteGeomInfo(StreamWriter fw)
        {
            for (int i = 0; i < m_Map.LayerCounts; i++)
            {
                GeoVectorLayer lyr = m_Map.GetLayerAt(i) as GeoVectorLayer;
                if (lyr == null || ((int)lyr.LayerTypeDetail >= 9))
                    continue;


                WriteLayerGeomInfo(lyr, fw);

            }
        }
        private void WriteLayerGeomInfo(GeoVectorLayer lyr, StreamWriter sw)
        {
            for (int i = 0; i < lyr.DataTable.Count; i++)
            {
                GeoData.GeoDataRow row = lyr.DataTable[i];
                if ((int)row.EditState > 0 && (int)row.EditState < 7)
                { 
                    if (row.EditState == EditState.AttributeAft)
                    {
                        IncAttribute.WriteAttriChangedGeoInfo(sw, lyr, row);
                    }
                    else if (row.EditState == EditState.Appear|| row.EditState == EditState.GeometryAft)
                    {
                        IncAppear.WriteAppearGeoInfo(sw, lyr, row);
                    }
                }
            }
            for (int i = 0; i < lyr.DataTable.Count; i++)
            {
                GeoData.GeoDataRow row = lyr.DataTable[i];

                if (row.EditState == EditState.Disappear)
                {                     
                    IncDisappear.WriteDisappearGeoInfo(sw, lyr, row); 
                } 
            }
        }
    }
}
