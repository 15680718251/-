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
    public class MapPolygonIntersectTool:MapTool
    {
        public MapPolygonIntersectTool(MapUI ui) : base(ui)
        {
            for (int i = 0; i < m_MapUI.SltGeoSet.Count; i++)
            {
                if (m_MapUI.SltGeoSet[i].Geometry is GeoPolygon)
                {
                    GeoData.GeoDataRow editr = m_MapUI.SltGeoSet[i];
                    //判断是否同一图层
                    GeoVectorLayer layer1 = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)editr.Table) as GeoVectorLayer;
                    if (m_EditRows.Count > 0)
                    {
                        GeoVectorLayer layer2 = m_MapUI.GetLayerByTable
                       ((GeoData.GeoDataTable)m_EditRows[0].Table) as GeoVectorLayer;
                        if (layer1.LayerName.Equals(layer2.LayerName))
                        {

                            plg1.Add((GeoPolygon)m_MapUI.SltGeoSet[i].Geometry);
                            m_EditRows.Add(editr);
                        }
                        
                    }
                    else
                    {
                        plg1.Add((GeoPolygon)m_MapUI.SltGeoSet[i].Geometry);
                        m_EditRows.Add(editr);
                    }

                }
            }


            if (plg1.Count > 1)
            {
                m_MapUI.OutPutTextInfo
                        ("提示：多面求交功能开始，单击左键求多边形的交，单击右键取消\r\n");
            }
            else
            {
                m_MapUI.OutPutTextInfo("提示：请选择一个图层内的两个或两个以上的多边形，再单击此功能\r\n");
                m_MapUI.ClearAllSlt();
                base.Cancel();

            }
            

        }

        private List<GeoPolygon> plg1 = new List<GeoPolygon>();
        private List<GeoPolygon> plg2 = new List<GeoPolygon>();
        private List<GeoData.GeoDataRow> m_EditRows = new List<GIS.GeoData.GeoDataRow>();

        public override void Cancel()
        {
            plg1.Clear();
            plg2.Clear();
            m_EditRows.Clear();
            base.Cancel();
        }

        public override void Finish()
        {
            m_MapUI.OutPutTextInfo("提示：多面求交工具结束！\r\n");
        }


        int isfirst=0;
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            isfirst++;
            if(isfirst==1)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (plg1.Count > 1)
                    {

                        GeometryComputation.PolygonIntersectPolygon.PolygonsIntersect(plg1, plg2);
                        if (plg2.Count < 0)
                            return;
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

                        List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
                        for (int i = 0; i < plg2.Count; i++)
                        {
                            GeoData.GeoDataRow rowNew = layer.AddGeometry(plg2[i]);

                            m_MapUI.InitialNewGeoFeature(rowNew);

                            if (rowNew.Table.Columns.Contains("PlgAttr"))
                            {
                                rowNew["PlgAttr"] = m_EditRows[0]["PlgAttr"];
                            }

                            m_list.Add(rowNew);
                            rowNew.EditState = EditState.Appear;

                            GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                            oprts.m_NewOperands.Add(oprtNew);

                        }
                        m_MapUI.ClearAllSlt();

                    }
                    m_MapUI.OutPutTextInfo(string.Format("{0}个多边形交为", plg1.Count));
                    m_MapUI.OutPutTextInfo(string.Format("{0}个多边形\r\n", plg2.Count));

                    Cancel();
                    
                }
                else if (e.Button == MouseButtons.Right)
                {
                    m_MapUI.OutPutTextInfo("提示：取消多面求交工具！\r\n");
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
