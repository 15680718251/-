using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using GIS.Map;
using GIS.Geometries;
using GIS.Layer;
using GIS.Toplogical;
using GIS.Utilities;
using GIS.TreeIndex.Forms;
using GIS.TreeIndex.OprtRollBack;
using System.Threading;

namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox //从PictureBox类派生下来的类代码文件显示的类型都是那种样子的  与控件关联的类代码
    {
        public List<GIS.GeoData.GeoDataRow> SelectByAttribute(int ElevValue, string PlgValue, string newPlgValue, AttributeQueryForm.TPType tpType, string predicate)
        {
            List<GIS.GeoData.GeoDataRow> rows = new List<GIS.GeoData.GeoDataRow>();
            m_Map.ClearAllSelect();   //清除所有选中集合

            GeoLayer lyr = GetActiveVectorLayer();
            if (lyr == null)
            {
                MessageBox.Show("当前没有活动图层","提示");
                return null;
            }
            if (lyr.LayerTypeDetail != LAYERTYPE_DETAIL.PolygonLayer)
            {
                MessageBox.Show("当前活动图层不是面图层，无法完成操作","提示");
                return null; ;
            }

            GeoData.GeoDataTable table = ((GeoVectorLayer)lyr).DataTable;
            int nNumsGeo = table.Count;

            if (table.Columns.Contains("ElevMax") && table.Columns.Contains("ElevMin"))
            {
                for (int k = 0; k < nNumsGeo; k++)
                {
                    GIS.GeoData.GeoDataRow row = table[k];   //找到的对象
                    if (row.Geometry == null || (int)row.EditState >= 6 ||
                        !row.Geometry.Bound.IsIntersectWith(m_ViewExtents)) //如果已经删除或者不在屏幕范围内则不进入判断
                        continue;

                    string PlgAttr = System.Convert.ToString(row["PlgAttr"]);
                    int PlgElevMax = System.Convert.ToInt32(row["ElevMax"]);
                    int PlgElevMin = System.Convert.ToInt32(row["ElevMin"]);


                    if (tpType == AttributeQueryForm.TPType.Delete)  //删除
                    {
                        if(PlgValue!="")
                        {
                            if (predicate == "<")
                            {

                                if ((PlgAttr == PlgValue) && (PlgElevMax < ElevValue))
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                }
                            }
                            else if (predicate == ">=")
                            {
                                if ((PlgAttr == PlgValue) && (PlgElevMin >= ElevValue))
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                }
                            }
                            else if (predicate == "<=")
                            {
                                if ((PlgAttr == PlgValue) && (PlgElevMax <= ElevValue))
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                }
                            }
                            else if (predicate == ">")
                            {
                                if ((PlgAttr == PlgValue) && (PlgElevMin > ElevValue))
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                }
                            }
                            else if (predicate == "=")
                            {
                                if ((PlgAttr == PlgValue) && (ElevValue == PlgElevMax))
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                }
                            }
                        }
                        else
                        {
                            if (predicate == "<")
                            {

                                if (PlgElevMax < ElevValue)
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                }
                            }
                            else if (predicate == ">=")
                            {
                                if (PlgElevMin >= ElevValue)
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                }
                            }
                            else if (predicate == "<=")
                            {
                                if (PlgElevMax <= ElevValue)
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                }
                            }
                            else if (predicate == ">")
                            {
                                if (PlgElevMin > ElevValue)
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                }
                            }
                            else if (predicate == "=")
                            {
                                if (ElevValue == PlgElevMax)
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                }
                            }
                        }
                    }
                    else  //改变多边形类型
                    { 
                        if(PlgValue!="")
                        {
                            if (predicate == "<")
                            {

                                if ((PlgAttr == PlgValue) && (PlgElevMax < ElevValue))
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                    row["PlgAttr"] = newPlgValue;
                                }
                            }
                            else if (predicate == ">=")
                            {
                                if ((PlgAttr == PlgValue) && (PlgElevMin >= ElevValue))
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                    row["PlgAttr"] = newPlgValue;
                                }
                            }
                            else if (predicate == "<=")
                            {
                                if ((PlgAttr == PlgValue) && (PlgElevMax <= ElevValue))
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                    row["PlgAttr"] = newPlgValue;
                                }
                            }
                            else if (predicate == ">")
                            {
                                if ((PlgAttr == PlgValue) && (PlgElevMin > ElevValue))
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                    row["PlgAttr"] = newPlgValue;
                                }
                            }
                            else if (predicate == "=")
                            {
                                if ((PlgAttr == PlgValue) && (ElevValue == PlgElevMax))
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                    row["PlgAttr"] = newPlgValue;
                                }
                            }
                        }
                        else
                        {
                            if (predicate == "<")
                            {

                                if (PlgElevMax < ElevValue)
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                    row["PlgAttr"] = newPlgValue;
                                }
                            }
                            else if (predicate == ">=")
                            {
                                if (PlgElevMin >= ElevValue)
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                    row["PlgAttr"] = newPlgValue;
                                }
                            }
                            else if (predicate == "<=")
                            {
                                if (PlgElevMax <= ElevValue)
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                    row["PlgAttr"] = newPlgValue;
                                }
                            }
                            else if (predicate == ">")
                            {
                                if (PlgElevMin > ElevValue)
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                    row["PlgAttr"] = newPlgValue;
                                }
                            }
                            else if (predicate == "=")
                            {
                                if (ElevValue == PlgElevMax)
                                {
                                    m_Map.AddSltObj(row);
                                    rows.Add(row);
                                    row["PlgAttr"] = newPlgValue;
                                }
                            }
                        }
                    }

                }
            }
            else
            {
                MessageBox.Show("当前活动图层不存在高程字段","提示");
                return null;
            }

            if (SltGeoSet.Count > 0 || SltLabelSet.Count > 0)
                OutPutTextInfo(string.Format(">>选中{0}个几何目标，{1}个文本注记\r\n", SltGeoSet.Count, SltLabelSet.Count));

            return rows;
        }
    }
}