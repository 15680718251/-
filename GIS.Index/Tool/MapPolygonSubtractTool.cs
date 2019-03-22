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
    public class MapPolygonSubtractTool:MapTool
    {
        public MapPolygonSubtractTool(MapUI ui) : base(ui)
        {
            if (m_MapUI.SltGeoSet.Count == 0)
            {
                m_MapUI.OutPutTextInfo("提示：两面求差功能开始，请选择两个多边形后\r\n");
            }
            else
            {
                if (m_MapUI.SltGeoSet.Count == 2)
                {
                    GeoLayer l1 = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)m_MapUI.SltGeoSet[0].Table) as GeoLayer;
                    GeoLayer l2 = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)m_MapUI.SltGeoSet[1].Table) as GeoLayer;

                    if (l1.LayerName.Equals(l2.LayerName))
                    {
                        plg1.Add((GeoPolygon)m_MapUI.SltGeoSet[0].Geometry);
                        plg1.Add((GeoPolygon)m_MapUI.SltGeoSet[1].Geometry);
                        m_EditRows.Add((GeoData.GeoDataRow)m_MapUI.SltGeoSet[0]);
                        m_EditRows.Add((GeoData.GeoDataRow)m_MapUI.SltGeoSet[0]);
                        Cancel();
                        Finish();
                    }
                    else
                    {
                        m_MapUI.OutPutTextInfo("提示：请选择同一图层的两个多边形\r\n");
                        Finish();
                    }
                }
                else
                {
                    m_MapUI.OutPutTextInfo("提示：请选择两个多边形\r\n");
                    Finish();
                }
            }
        }

        private List<GeoPolygon> plg1 = new List<GeoPolygon>();
        private List<GeoPolygon> plg2 = new List<GeoPolygon>();
        private List<GeoData.GeoDataRow> m_EditRows = new List<GIS.GeoData.GeoDataRow>();
        
        public override void initial()
        {
            if (m_MapUI.SltGeoSet.Count == 0)
            {
                m_MapUI.OutPutTextInfo("提示：两面求差功能开始，请选择两个多边形后\r\n");
            }
            else
            {
                if (m_MapUI.SltGeoSet.Count == 2)
                {
                    GeoLayer l1 = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)m_MapUI.SltGeoSet[0].Table) as GeoLayer;
                    GeoLayer l2 = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)m_MapUI.SltGeoSet[1].Table) as GeoLayer;

                    if (l1.LayerName.Equals(l2.LayerName))
                    {
                        plg1.Add((GeoPolygon)m_MapUI.SltGeoSet[0].Geometry);
                        plg1.Add((GeoPolygon)m_MapUI.SltGeoSet[1].Geometry);
                        m_EditRows.Add((GeoData.GeoDataRow)m_MapUI.SltGeoSet[0]);
                        m_EditRows.Add((GeoData.GeoDataRow)m_MapUI.SltGeoSet[0]);
                        Cancel();
                    }
                    else
                        m_MapUI.OutPutTextInfo("提示：请选择同一图层的两个多边形\r\n");
                }
                else
                    m_MapUI.OutPutTextInfo("提示：请先选择两个多边形\r\n");

            }

        }

        public override void Cancel()
        {

            if (plg1.Count < 2)
            {
                m_MapUI.ClearAllSlt();
                base.Cancel();
                m_MapUI.OutPutTextInfo("提示：请选择两个多边形\r\n");
            }
            if (plg1.Count > 2)
            {
                m_MapUI.ClearAllSlt();

                base.Cancel();

                m_MapUI.OutPutTextInfo("提示：您选择的多边形超过了两个，请选择两个多边形\r\n");
            }
            if (plg1.Count == 2)
            {

                GeometryComputation.PolygonSubtractPolygon.PolygonSubtract(plg1[0], plg1[1], plg2);

                //string wkt1 = GIS.Converters.WellKnownText.GeometryToWKT.Write(plg1[0]);
                //string wkt2 = GIS.Converters.WellKnownText.GeometryToWKT.Write(plg1[1]);

                //OSGeo.OGR.Geometry g1 = OSGeo.OGR.Geometry.CreateFromWkt(wkt1);
                //OSGeo.OGR.Geometry g2 = OSGeo.OGR.Geometry.CreateFromWkt(wkt2);

                //if (!g1.Intersect(g2))
                //{
                //    m_MapUI.OutPutTextInfo("两个多边形不相交\r\n");
                //    //base.Cancel();
                //    return;
                //}

                //OSGeo.OGR.Geometry rg = g1.Difference(g2);


                if (plg2.Count < 0)
                    return;
                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                m_MapUI.m_OprtManager.AddOprt(oprts);
                ///////////////操作回退

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

                List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
                for (int i = 0; i < plg2.Count; i++)
                {
                    GeoData.GeoDataRow rowNew = layer.AddGeometry(plg2[i]);

                    if (rowNew.Table.Columns.Contains("PlgAttr"))
                    {
                        rowNew["PlgAttr"] = m_EditRows[0]["PlgAttr"];
                    }

                    m_MapUI.InitialNewGeoFeature(rowNew);
                    m_list.Add( rowNew);
                    rowNew.EditState = EditState.Appear;

                    GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                    oprts.m_NewOperands.Add(oprtNew);

                }
                m_MapUI.ClearAllSlt();

                base.Cancel();
            }
        }

        public override void Finish()
        {

            m_MapUI.OutPutTextInfo("根据选择先后顺序，面1差面2产生了");
            m_MapUI.OutPutTextInfo(string.Format("{0}个多边形\r\n", plg2.Count));

            m_MapUI.OutPutTextInfo("提示：两面求差工具结束！\r\n");

            plg1.Clear(); 
            plg2.Clear();
            m_EditRows.Clear();
        }

        #region 废弃
        //public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        if (plg1.Count < 2)
        //        {
        //            m_MapUI.OutPutTextInfo("提示：请先选择两个多边形");
        //        }
        //        else
        //        {
        //            m_MapUI.OutPutTextInfo("提示：请单击右键求差");
        //        }
                
        //    }
        //    else if (e.Button == MouseButtons.Right)
        //    {
        //        Cancel();
        //    }
        //}

        //private void SelectLine(System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (plg1.Count < 2)
        //    {

        //        GeoData.GeoDataRow row = m_MapUI.SelectByPt(e.Location, SelectType.Polygon);

        //        if (m_EditRows.Count == 0)
        //        {
        //            if (row!=null && row.Geometry is GeoPolygon)
        //            {
        //                plg1.Add((GeoPolygon)row.Geometry);
        //                m_EditRows.Add(row);
        //                m_MapUI.Refresh();
        //            }
        //            else
        //            {
        //                m_MapUI.OutPutTextInfo("提示：您选择的目标不是多边形，请选择多边形");
        //            }
        //        }
        //        else
        //        {
        //            GeoLayer l1 = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)row.Table) as GeoLayer;
        //            GeoLayer l2 = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)m_MapUI.SltGeoSet[0].Table) as GeoLayer;
        //            if (l1.LayerName.Equals(l2.LayerName))
        //            {
        //                plg1.Add((GeoPolygon)row.Geometry);
        //                m_EditRows.Add(row);
        //                m_MapUI.Refresh();
        //            }
        //            else
        //                m_MapUI.OutPutTextInfo("提示：您选择的多边形不在同一个图层中，请选择同一图层的多边形");
        //        }
        //    }            
        //}
        #endregion

    }
}
