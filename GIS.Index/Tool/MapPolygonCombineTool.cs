using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GIS.Toplogical;
using GIS.Layer;
using GIS.Geometries;
using GIS.Increment;

using System.Windows.Forms;
using System.Drawing;
using GIS.Map;
using GIS.Utilities;
using GIS.TreeIndex.Tool;
using GIS.SpatialRelation;
using GIS.TreeIndex.GeometryComputation;
using System.IO;
using System.Threading;

namespace GIS.TreeIndex.Tool
{
    public class MapPolygonCombineTool:MapTool
    {
        public MapPolygonCombineTool(MapUI ui): base(ui)
        {

            if (m_MapUI.SltGeoSet.Count != 2)
            {
                //m_MapUI.OutPutTextInfo("提示：请选择一个图层内的两个或两个以上的多边形，再单击此功能\r\n");
                m_MapUI.OutPutTextInfo("提示：请选择一个图层内的两个多边形，再单击此功能\r\n");
                m_MapUI.ClearAllSlt();
                base.Cancel();
            }
            else
            {
                //改成两个多边形，可以使多多边形    20143.30 吴志强
                string str1 = GIS.Converters.WellKnownText.GeometryToWKT.Write(m_MapUI.SltGeoSet[0].Geometry);
                g1 = OSGeo.OGR.Geometry.CreateFromWkt(str1);
                string str2 = GIS.Converters.WellKnownText.GeometryToWKT.Write(m_MapUI.SltGeoSet[1].Geometry);
                g2 = OSGeo.OGR.Geometry.CreateFromWkt(str2);
                OSGeo.OGR.wkbGeometryType t1=g1.GetGeometryType();
                OSGeo.OGR.wkbGeometryType t2=g2.GetGeometryType();

                if ((t1 == OSGeo.OGR.wkbGeometryType.wkbPolygon || t1 == OSGeo.OGR.wkbGeometryType.wkbMultiPolygon)
                    && (t2 == OSGeo.OGR.wkbGeometryType.wkbPolygon || t2 == OSGeo.OGR.wkbGeometryType.wkbMultiPolygon))
                {
                    m_EditRows.AddRange(m_MapUI.SltGeoSet);

                    m_MapUI.OutPutTextInfo("提示：多面求并功能开始，单击左键求多边形的并，单击右键取消\r\n");
                }


                //for (int i = 0; i < m_MapUI.SltGeoSet.Count; i++)
                //{
                //    if (m_MapUI.SltGeoSet[i].Geometry is GeoPolygon)
                //    {
                //        GeoData.GeoDataRow editr = m_MapUI.SltGeoSet[i];

                //        //判断是否同一图层
                //        GeoVectorLayer layer1 = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)editr.Table) as GeoVectorLayer;
                //        if (m_EditRows.Count > 0)
                //        {
                //            GeoVectorLayer layer2 = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)m_EditRows[0].Table) as GeoVectorLayer;

                //            if (layer1.LayerName.Equals(layer2.LayerName))
                //            {
                //                plg1.Add((GeoPolygon)m_MapUI.SltGeoSet[i].Geometry);
                //                //plg1.Add(m_MapUI.SltGeoSet[i].Geometry);
                //                m_EditRows.Add(editr);
                //            }
                //        }
                //        else
                //        {
                //            plg1.Add((GeoPolygon)m_MapUI.SltGeoSet[i].Geometry);
                //            //plg1.Add(m_MapUI.SltGeoSet[i].Geometry);
                //            m_EditRows.Add(editr);
                //        }
                //    }
                //}


                //if (m_MapUI.SltGeoSet.Count == 2)
                //{
                //    m_MapUI.OutPutTextInfo("提示：多面求并功能开始，单击左键求多边形的并，单击右键取消\r\n");
                //}
            }
           

            //if (plg1.Count > 1)
            //{
            //    m_MapUI.OutPutTextInfo("提示：多面求并功能开始，单击左键求多边形的并，单击右键取消\r\n");
            //}
            //else
            //{
            //    m_MapUI.OutPutTextInfo("提示：请选择一个图层内的两个或两个以上的多边形，再单击此功能\r\n");
            //    m_MapUI.ClearAllSlt();
            //    base.Cancel();
            //}
        }

        private OSGeo.OGR.Geometry g1 = null;
        private OSGeo.OGR.Geometry g2 = null;

        //private List<GeoPolygon> plg1 = new List<GeoPolygon>();
        //private List<GeoPolygon> plg2 = new List<GeoPolygon>();
        private List<GeoData.GeoDataRow> m_EditRows = new List<GIS.GeoData.GeoDataRow>();

        public override void Cancel()
        {
            //plg1.Clear();
            //plg2.Clear();
            m_EditRows.Clear();
            base.Cancel();  
        }

        public override void Finish()
        {
            m_MapUI.OutPutTextInfo("提示：多面合并工具结束！\r\n");
        }

        int isfirst=0;

        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            isfirst++;
            if (isfirst == 1)
            {
                if (e.Button == MouseButtons.Left)
                {
                    //if (plg1.Count > 1)
                    if (m_EditRows.Count>1)
                    {

                        //GeometryComputation.PolygonCombinePolyg.PolygonCombine(plg1, plg2);

                        OSGeo.OGR.Geometry rg = g1.Union(g2);  //可以合成多多边形

                        //if (plg2.Count < 0)
                        //    return;

                        string rstr;
                        rg.ExportToWkt(out rstr);
                        GIS.Geometries.Geometry rrg = GIS.Converters.WellKnownText.GeometryFromWKT.Parse(rstr);

                        //操作回退
                        GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                        m_MapUI.m_OprtManager.AddOprt(oprts);
                        //操作回退

                        for (int i = 0; i < m_EditRows.Count; i++)
                        {
                            EditState state = m_EditRows[i].EditState;
                            //if (m_EditRows[i].EditState == EditState.Original)
                            //{
                                m_EditRows[i].EditState = EditState.Disappear;
                            //}
                            //else
                            //    m_EditRows[i].EditState = EditState.Invalid;
                            GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(m_EditRows[i], state, m_EditRows[i].EditState);
                            oprts.m_OldOperands.Add(oprtOld);
                        }

                        GeoVectorLayer layer = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)m_EditRows[0].Table) as GeoVectorLayer;
                        GeoData.GeoDataRow rowNew = layer.AddGeometry(rrg);

                        //List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
                        //for (int i = 0; i < plg2.Count; i++)
                        //{
                        //    GeoData.GeoDataRow rowNew = layer.AddGeometry(plg2[i]);

                        if (rowNew.Table.Columns.Contains("PlgAttr"))
                        {
                            rowNew["PlgAttr"] = m_EditRows[0]["PlgAttr"];
                        }

                        m_MapUI.InitialNewGeoFeature(rowNew);

                        //    m_list.Add(rowNew);
                        rowNew.EditState = EditState.Appear;

                        GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                        oprts.m_NewOperands.Add(oprtNew);
                        //}

                        m_MapUI.ClearAllSlt();

                    }
                    //m_MapUI.OutPutTextInfo(string.Format("{0}个多边形合并为", plg1.Count));
                    //m_MapUI.OutPutTextInfo(string.Format("{0}个多边形\r\n", plg2.Count));
                    m_MapUI.OutPutTextInfo("合并成功 \r\n");
                    Cancel();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    m_MapUI.OutPutTextInfo("提示：取消多面合并工具！\r\n");
                    Cancel();

                }
            }
            else
            {
                m_MapUI.ClearAllSlt();
                base.Cancel();
            }
        }

    }
}
