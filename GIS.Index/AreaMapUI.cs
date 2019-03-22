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
using System.Net.Sockets;
using System.Threading;

namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox //从PictureBox类派生下来的类代码文件显示的类型都是那种样子的  与控件关联的类代码
    {
       #region 统计每种类型剔除的数目和面积和变量定义
        public class TypeandArea
        {
            public int croplandCount_1, AreaSum1;
            public int foestCount_2, AreaSum2;
            public int grassCount_3, AreaSum3;
            public int waterCount_4, AreaSum4;
            public int urbanCount_5, AreaSum5;
            public int barelandCount_6, AreaSum6;
            public int shrubCount_7, AreaSum7;
        }
        public TypeandArea TypeArea = new TypeandArea();
        #endregion

        public List<GIS.GeoData.GeoDataRow> SelectByArea(double PlgAreaVal,string predicate)
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

            for (int k = 0; k < nNumsGeo; k++)
            {
                try
                {
                    GIS.GeoData.GeoDataRow row = table[k];   //找到的对象
                    if (row.Geometry == null || (int)row.EditState >= 6 ||
                        !row.Geometry.Bound.IsIntersectWith(m_ViewExtents)) //如果已经删除或者不在屏幕范围内则不进入判断
                        continue;

                    double PlgArea = row.Geometry.Area;
                    string PlgAttr = System.Convert.ToString(row["PlgAttr"]);

                    //if (PlgArea <= PlgAreaVal)
                    //{
                    //    //选中显示出来
                    //    m_Map.AddSltObj(row);
                    //    rows.Add(row);
                        

                        #region 统计每种类型剔除的数目和面积
                        //if (PlgAttr == 1)
                        //{
                        //    TypeArea.croplandCount_1++;
                        //    TypeArea.AreaSum1 += PlgArea;
                        //}
                        //else if (PlgAttr == 2)
                        //{
                        //    TypeArea.foestCount_2++;
                        //    TypeArea.AreaSum2 += PlgArea;
                        //}
                        //else if (PlgAttr == 3)
                        //{
                        //    TypeArea.grassCount_3++;
                        //    TypeArea.AreaSum3 += PlgArea;
                        //}
                        //else if (PlgAttr == 4)
                        //{
                        //    TypeArea.waterCount_4++;
                        //    TypeArea.AreaSum4 += PlgArea;
                        //}
                        //else if (PlgAttr == 5)
                        //{
                        //    TypeArea.urbanCount_5++;
                        //    TypeArea.AreaSum5 += PlgArea;
                        //}
                        //else if (PlgAttr == 6)
                        //{
                        //    TypeArea.barelandCount_6++;
                        //    TypeArea.AreaSum6 += PlgArea;
                        //}
                        //else if (PlgAttr == 7)
                        //{
                        //    TypeArea.shrubCount_7++;
                        //    TypeArea.AreaSum7 += PlgArea;
                        //}
                        #endregion
                    //}

                    if(predicate=="<")
                    {
                        if (PlgArea < PlgAreaVal)
                        {
                            m_Map.AddSltObj(row);
                            rows.Add(row);
                        }
                    }
                    else if (predicate == ">=")
                    {
                        if (PlgArea >= PlgAreaVal)
                        {
                            m_Map.AddSltObj(row);
                            rows.Add(row);
                        }
                    }
                    else if(predicate == "<=")
                    {
                        if (PlgArea <= PlgAreaVal)
                        {
                            m_Map.AddSltObj(row);
                            rows.Add(row);
                        }
                    }
                    else if(predicate == ">")
                    {
                        if (PlgArea > PlgAreaVal)
                        {
                            m_Map.AddSltObj(row);
                            rows.Add(row);
                        }
                    }
                    else if (predicate == "=")
                    {
                        if (PlgArea == PlgAreaVal)
                        {
                            m_Map.AddSltObj(row);
                            rows.Add(row);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());
                }
            }

            if (SltGeoSet.Count > 0 || SltLabelSet.Count > 0)
                OutPutTextInfo(string.Format(">>选中{0}个几何目标，{1}个文本注记\r\n", SltGeoSet.Count, SltLabelSet.Count));

            #region 输出每种类型剔除的数目和面积
            //OutPutTextInfo(string.Format(">>剔除croplandCount_1数为：{0}，面积和为：{1}\r\n", TypeArea.croplandCount_1, TypeArea.AreaSum1));
            //OutPutTextInfo(string.Format(">>剔除foestCount_2数为：{0}，面积和为：{1}\r\n", TypeArea.foestCount_2, TypeArea.AreaSum2));
            //OutPutTextInfo(string.Format(">>剔除grassCount_3数为：{0}，面积和为：{1}\r\n", TypeArea.grassCount_3, TypeArea.AreaSum3));
            //OutPutTextInfo(string.Format(">>剔除waterCount_4数为：{0}，面积和为：{1}\r\n", TypeArea.waterCount_4, TypeArea.AreaSum4));
            //OutPutTextInfo(string.Format(">>剔除urbanCount_5数为：{0}，面积和为：{1}\r\n", TypeArea.urbanCount_5, TypeArea.AreaSum5));
            //OutPutTextInfo(string.Format(">>剔除barelandCount_6数为：{0}，面积和为：{1}\r\n", TypeArea.barelandCount_6, TypeArea.AreaSum6));
            //OutPutTextInfo(string.Format(">>剔除shrubCount_7数为：{0}，面积和为：{1}\r\n", TypeArea.shrubCount_7, TypeArea.AreaSum7));
            #endregion
            ToolDeleteObj();  //直接调用删除选定对象工具
            return rows;
        }
    }
}