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
        public List<GIS.GeoData.GeoDataRow> SelectByBuffer(OSGeo.OGR.Geometry  ring,BufferQueryForm.TPType TpType)
        {
            m_Map.ClearAllSelect();   //清除所有选中集合
            List<GIS.GeoData.GeoDataRow> rows = new List<GIS.GeoData.GeoDataRow>();

            GeoLayer layer = GetActiveVectorLayer();  //获取当前图层
            GeoData.GeoDataTable table = ((GeoVectorLayer)layer).DataTable;

            GeoVectorLayer actLayer = GetDraftMixLayer() as GeoVectorLayer;

            string rwkt;
            ring.ExportToWkt(out rwkt);
            GIS.Geometries.Geometry  gr = GIS.Converters.WellKnownText.GeometryFromWKT.Parse(rwkt);
            GeoData.GeoDataRow rowNew = actLayer.AddGeometry(gr);

            //m_Map.AddSltObj(rowNew);  //添加到选中集合中删掉

            int nNumsGeo = table.Count;
            for (int k = 0; k < nNumsGeo; k++)
            {
                GIS.GeoData.GeoDataRow row = table[k];   //找到的对象
                if (row.Geometry == null || (int)row.EditState >= 6 ||
                    !row.Geometry.Bound.IsIntersectWith(m_ViewExtents)) //如果已经删除或者不在屏幕范围内则不进入判断
                    continue;

                string str = GIS.Converters.WellKnownText.GeometryToWKT.Write(row.Geometry);
                OSGeo.OGR.Geometry tg = OSGeo.OGR.Geometry.CreateFromWkt(str);
                if (TpType == GIS.TreeIndex.Forms.BufferQueryForm.TPType.TpIntersect) //相交
                {
                    if (ring.Intersect(tg)) //两者相交
                    {
                        //选中显示出来
                        if (row.SelectState == false)
                        {
                            m_Map.AddSltObj(row);
                        }
                        else
                        {
                            m_Map.RemoveSltObj(row);
                        }
                        rows.Add(row);
                    }
                }
                else
                {
                    if (ring.Contains(tg))
                    {
                        if (row.SelectState == false)
                        {
                            m_Map.AddSltObj(row);         //添加对象
                        }
                        else
                        {
                            m_Map.RemoveSltObj(row);     //从选中集合中删除
                        }
                        rows.Add(row);
                    }
                }

                #region 废弃
                //TpRelateConstants relate = TpRelatemain.IsTpWith(ring, row.Geometry, false);

                //if (relate == TpRelateConstants.tpContains || relate == TpRelateConstants.tpCovers && TpType==GIS.UI.Forms.BufferQueryForm.TPType.TpContains) //处理在缓冲区内的
                //{
                //    //选中显示出来
                //    if (row.SelectState == false)
                //    {
                //        m_Map.AddSltObj(row);
                //    }
                //    else
                //    {
                //        m_Map.RemoveSltObj(row);
                //    }
                //    rows.Add(row);
                //}

                //if (relate == TpRelateConstants.tpIntersect && TpType == GIS.UI.Forms.BufferQueryForm.TPType.TpIntersect) //处理与缓冲区相交的
                //{
                //    List<GeoPolygon> plg1 = new List<GeoPolygon>();
                //    List<GeoPolygon> plg2 = new List<GeoPolygon>();

                //    if (row.SelectState == false)
                //    {
                //        m_Map.AddSltObj(row);         //添加对象
                //        plg1.Add(ring);
                //        plg1.Add((GeoPolygon)row.Geometry);
                //    }
                //    else
                //    {
                //        m_Map.RemoveSltObj(row);     //从选中集合中删除
                //    }
                //    rows.Add(row);
                //}
                #endregion
            }

            #region 已经废弃
            /*
            for (int j = 0; j < LayerCounts; j++)
            {
                GeoData.GeoDataTable table = null;
                GeoLayer layer = GetLayerAt(j);

                if (layer.LayerTypeDetail == LAYERTYPE_DETAIL.SurveyLayer)
                    continue;
                if (layer is GeoVectorLayer && layer.Enable)
                {
                    table = ((GeoVectorLayer)layer).DataTable;            //图层
                }
                else
                    continue;

                //将缓冲区多边形加入当前图层,否则无法执行,多边形求交和求差运算
                GeoVectorLayer layer1 = GetLayerByTable((GeoData.GeoDataTable)table) as GeoVectorLayer;

                GeoData.GeoDataRow rowNew = layer1.AddGeometry(ring);
                m_Map.AddSltObj(rowNew);  //添加到选中集合中删掉

                int nNumsGeo = table.Count;
                for (int k = 0; k < nNumsGeo; k++)
                {
                    GIS.GeoData.GeoDataRow row = table[k];   //找到的对象
                    if (row.Geometry == null || (int)row.EditState >= 6 ||
                        !row.Geometry.Bound.IsIntersectWith(m_ViewExtents)) //如果已经删除或者不在屏幕范围内则不进入判断
                        continue;

                    TpRelateConstants relate = TpRelatemain.IsTpWith(ring, row.Geometry, false);

                    if (relate == TpRelateConstants.tpContains || relate == TpRelateConstants.tpCovers && BufForm.TpType.TpContains.Checked) //处理在缓冲区内的
                    {
                        //选中显示出来
                        if (row.SelectState == false)
                        {
                            m_Map.AddSltObj(row);
                        }
                        else
                        {
                            m_Map.RemoveSltObj(row);
                        }
                        rows.Add(row);

                        if (BufForm.TpType.TpContains.Delete) //删除
                        {
                            ToolDeleteObj();  //直接调用删除选定对象工具
                            //若点了缓冲区工具，此时再点左键选了对象，再点右键还将激发delete工具
                            //直接用此工具，还可操作回退
                        }
                        else                 //改变编码   可增加将选中记录都显示在一个对话框上
                        {
                            Refresh();
                            row["PlgAttr"] = BufForm.TpType.TpContains.ChangeCode;
                        }
                    }

                    if (relate == TpRelateConstants.tpIntersect && BufForm.TpType.TpIntersect.Checked) //处理与缓冲区相交的
                    {
                        List<GeoPolygon> plg1 = new List<GeoPolygon>();
                        List<GeoPolygon> plg2 = new List<GeoPolygon>();
                        if (row.SelectState == false)
                        {
                            m_Map.AddSltObj(row);         //添加对象
                            plg1.Add(ring);
                            plg1.Add((GeoPolygon)row.Geometry);
                        }
                        else
                        {
                            m_Map.RemoveSltObj(row);     //从选中集合中删除
                        }
                        rows.Add(row);
                        if (plg1.Count > 1)
                            GeometryComputation.PolygonIntersectPolygon.PolygonsIntersect(plg1, plg2);//plg1两个多边形相交部分存在了plg2[0]中
                        if (plg2[0].Area > (BufForm.TpType.TpIntersect.AreaVal * row.Geometry.Area))
                        {
                            GeometryComputation.PolygonSubtractPolygon.PolygonSubtract(plg1[1], plg1[0], plg2);//plg1[1], plg1[0] 做差的结果存在了plg2[1]中
                            for (int i = 1; i < plg2.Count; i++)
                            {
                                GeoData.GeoDataRow rowTemp = layer1.AddGeometry(plg2[i]);
                                InitialNewGeoFeature(rowTemp);
                                rowTemp.EditState = EditState.Appear;
                                if (BufForm.TpType.TpContains.Delete) //删除
                                {
                                    rowTemp.EditState = EditState.Disappear;
                                }
                                else  //改变编码   可增加将选中记录都显示在一个对话框上
                                {
                                    Refresh();
                                    row["PlgAttr"] = BufForm.TpType.TpContains.ChangeCode;
                                }
                            }
                            row.EditState = EditState.Disappear;
                        }
                    }
                }
            }
            */
            #endregion

            Refresh();
            if (SltGeoSet.Count > 0 || SltLabelSet.Count > 0)
                OutPutTextInfo(string.Format(">>选中{0}个几何目标，{1}个文本注记\r\n", SltGeoSet.Count, SltLabelSet.Count));
            return rows;
        }
    }
}