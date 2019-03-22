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
    public class MapCreatPolygonTool:MapTool
    {

        public MapCreatPolygonTool(MapUI ui)
            : base(ui)
        {
            
            
            
            for (int i = 0; i < m_MapUI.SltGeoSet.Count; i++)
            {
                
                    if (m_MapUI.SltGeoSet[i].Geometry is GeoLineString)
                    {
                        GeoData.GeoDataRow editr = m_MapUI.SltGeoSet[i];
                        pls.Add((GeoLineString)m_MapUI.SltGeoSet[i].Geometry);
                        m_EditRows.Add(editr);

                    }
            }


            if (pls.Count > 0)
            {
                m_MapUI.OutPutTextInfo
                        ("提示：线构多变形功能开始，单击左键构建多变性，单击右键取消\r\n");
            }
            else
            {
                m_MapUI.OutPutTextInfo
                        ("提示：请选择一条或一条以上的线，再单击此功能\r\n");
                Finish();
                m_MapUI.ClearAllSlt();
                base.Cancel();
            }
            
        }
        

         private List<GeoLineString> pls = new List<GeoLineString>();
        private List<GeoPolygon> plg = new List<GeoPolygon>();
        private List<GeoData.GeoDataRow> m_EditRows = new List<GIS.GeoData.GeoDataRow>();


        public override void Cancel()
        {
            plg.Clear();
            
            pls.Clear();
 
            m_EditRows.Clear();
            m_MapUI.ClearAllSlt();
            base.Cancel();
           
        }

        public override void Finish()
        {
            m_MapUI.OutPutTextInfo("提示：线构多边形工具结束！\r\n");
        }


        int isfirst = 0;
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            isfirst++;
            if (isfirst == 1)
            {

                if (e.Button == MouseButtons.Left)
                {
                    if (pls.Count > 0)
                    {

                        int plgcount = 0;
                        GIS.Map.LayerGroup lyg = m_MapUI.GetGroupByName(m_MapUI.ActiveLyrGroupName) as LayerGroup;
                        for (int i = 0; i < lyg.Counts; i++)
                        {
                            if (lyg[i].LayerTypeDetail == GIS.Layer.LAYERTYPE_DETAIL.PolygonLayer)
                                plgcount++;
                        }


                        if (plgcount == 0)
                        {
                            m_MapUI.OutPutTextInfo("提示：请加载面图层！");
                        }

                        else
                        {
                            TreeIndex.Forms.LineToPolygonForm frm = new GIS.TreeIndex.Forms.LineToPolygonForm(m_MapUI);

                            //
                            //Application.Run(frm);
                            //frm.MdiParent = ;
                            frm.ShowDialog();
                            string str = frm.LayerName;

                            GeometryComputation.CreatePolygon.CreatePolylineToPolygon(pls, plg);

                            ///////////////操作回退
                            GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                            m_MapUI.m_OprtManager.AddOprt(oprts);
                            ///////////////操作回退


                            GeoVectorLayer layer = m_MapUI.GetLayerByName(str) as GeoVectorLayer;

                            List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
                            for (int i = 0; i < plg.Count; i++)
                            {
                                GeoData.GeoDataRow rowNew = layer.AddGeometry(plg[i]);

                                m_MapUI.InitialNewGeoFeature(rowNew);
                                m_list.Add(rowNew);
                                rowNew.EditState = EditState.Appear;

                                GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                                oprts.m_NewOperands.Add(oprtNew);

                            }

                            m_MapUI.OutPutTextInfo(string.Format("{0}个线生成", pls.Count));
                            m_MapUI.OutPutTextInfo(string.Format("{0}个多边形\r\n", plg.Count));


                            m_MapUI.ClearAllSlt();

                            base.Cancel();
                        }
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        m_MapUI.ClearAllSlt();
                        base.Cancel();


                        m_MapUI.OutPutTextInfo("提示：取消构建多边形！\r\n");
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
}
