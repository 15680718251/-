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
    public class MapPolygontoLineTool:MapTool
    {


        public MapPolygontoLineTool(MapUI ui)
            : base(ui)
        {
            
            
            for (int i = 0; i < m_MapUI.SltGeoSet.Count; i++)
            {
                
                if (m_MapUI.SltGeoSet[i].Geometry is GeoPolygon)
                {
                    GeoData.GeoDataRow editr = m_MapUI.SltGeoSet[i];
                    //判断是否同一图层

                        plg1.Add((GeoPolygon)m_MapUI.SltGeoSet[i].Geometry);
                        m_EditRows.Add(editr);
                    

                }
            }


            if (plg1.Count > 0)
            {
                m_MapUI.OutPutTextInfo
                        ("提示：多变形边界转线功能开始，单击左键分割多边形，单击右键取消\r\n");
            }
            else
            {
                m_MapUI.OutPutTextInfo
                        ("提示：请选择一个或一个以上的多边形，再单击此功能\r\n");
                m_MapUI.ClearAllSlt();
                base.Cancel();
            }
            
        }
        

        private List<GeoPolygon> plg1 = new List<GeoPolygon>();
        private List<GeoLineString> pls = new List<GeoLineString>();
        private List<GeoData.GeoDataRow> m_EditRows = new List<GIS.GeoData.GeoDataRow>();


        public override void Cancel()
        {
            plg1.Clear();
            
            pls.Clear();
 
            m_EditRows.Clear();
            m_MapUI.ClearAllSlt();
            base.Cancel();
           
        }

        public override void Finish()
        {
            m_MapUI.OutPutTextInfo("提示：多边形边界专线工具结束！\r\n");
        }


        int isfirst = 0;
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            isfirst++;
            if (isfirst == 1)
            {

                if (e.Button == MouseButtons.Left)
                {
                    if (plg1.Count > 0)
                    {

                        int plgcount = 0;
                        GIS.Map.LayerGroup lyg = m_MapUI.GetGroupByName(m_MapUI.ActiveLyrGroupName) as LayerGroup;
                        for (int i = 0; i < lyg.Counts; i++)
                        {
                            if (lyg[i].LayerTypeDetail == GIS.Layer.LAYERTYPE_DETAIL.LineLayer)
                                plgcount++;
                        }


                        if (plgcount == 0)
                        {
                            m_MapUI.OutPutTextInfo("提示：请加载面线层！");
                        }

                        else
                        {

                            PolygonToLine.polygontoline(plg1, pls);
                            ///////////////操作回退
                            GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                            m_MapUI.m_OprtManager.AddOprt(oprts);
                            ///////////////操作回退
                            GIS.TreeIndex.Forms.PolygonToLineFrm frm = new GIS.TreeIndex.Forms.PolygonToLineFrm(m_MapUI);
                            frm.ShowDialog();
                            string str = frm.LayerName;


                            GeoVectorLayer layer = m_MapUI.GetLayerByName(str) as GeoVectorLayer;

                            List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
                            for (int i = 0; i < pls.Count; i++)
                            {
                                GeoData.GeoDataRow rowNew = layer.AddGeometry(pls[i]);

                                m_MapUI.InitialNewGeoFeature(rowNew);
                                m_list.Add(rowNew);
                                rowNew.EditState = EditState.Appear;

                                GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                                oprts.m_NewOperands.Add(oprtNew);

                            }

                            m_MapUI.OutPutTextInfo(string.Format("{0}个多边形的边界生成", plg1.Count));
                            m_MapUI.OutPutTextInfo(string.Format("{0}条线\r\n", pls.Count));


                            m_MapUI.ClearAllSlt();

                            base.Cancel();
                        }
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        m_MapUI.ClearAllSlt();
                        base.Cancel();


                        m_MapUI.OutPutTextInfo("提示：取消分割多边形！\r\n");
                    }
                }

            }
            else
            {
                m_MapUI.ClearAllSlt();
                base.Cancel();

            }



        }    
            
            
            
      
        
    }


    public class PolygonToLine
    {
        public static void polygontoline(List<GeoPolygon> plg, List<GeoLineString> pls)
        {
            pls.Clear();

            List<GeoLineString> pls1 = new List<GeoLineString>();
            for (int i = 0; i < plg.Count; i++)
            {

                GeoLineString pl = new GeoLineString(plg[i].ExteriorRing.Vertices);

                pls1.Add(pl);

                for (int j = 0; j < plg[i].InteriorRings.Count; j++)
                {
                    GeoLineString pll = new GeoLineString(plg[i].InteriorRings[j].Vertices);

                    pls1.Add(pll);
                }

            }

            List<GeoLineString> pls2 = new List<GeoLineString>();
            GeometryComputation.LinesSplitLines.SplitLines(pls1, pls2, false);
            GeometryComputation.LinkLine.LinkLines1(pls2, pls);


        }
    }

}
