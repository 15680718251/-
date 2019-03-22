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
    public class MapLineSplitTool:MapTool
    {
        public MapLineSplitTool(MapUI ui)
            : base(ui)
        {
            
            for (int i = 0; i < m_MapUI.SltGeoSet.Count; i++)
            {
                if (m_MapUI.SltGeoSet[i].Geometry is GeoLineString)
                {
                    GeoData.GeoDataRow editr = m_MapUI.SltGeoSet[i];
                    //判断是否同一图层
                    GeoVectorLayer layer1 = m_MapUI.GetLayerByTable
                        ((GeoData.GeoDataTable)editr.Table) as GeoVectorLayer;
                    if (m_EditRows.Count > 0)
                    {
                        GeoVectorLayer layer2 = m_MapUI.GetLayerByTable
                       ((GeoData.GeoDataTable)m_EditRows[0].Table) as GeoVectorLayer;
                        if (layer1.LayerName.Equals(layer2.LayerName))
                        {

                            pls1.Add((GeoLineString)m_MapUI.SltGeoSet[i].Geometry);
                            m_EditRows.Add(editr);
                        }
                        
                    }
                    else
                    {
                        pls1.Add((GeoLineString)m_MapUI.SltGeoSet[i].Geometry);
                        m_EditRows.Add(editr);
                    }

                }
            }


            if (pls1.Count > 0)
            {
                m_MapUI.OutPutTextInfo
                        ("提示：交点打断功能开始，单击左键打断，单击右键取消\r\n");
            }
            else
            {
                m_MapUI.OutPutTextInfo
                        ("提示：请选择一个图层内的一条或一条以上的线，再单击此功能\r\n");
                m_MapUI.ClearAllSlt();
                base.Cancel();

            }
            

        }

        private List<GeoLineString> pls1 = new List<GeoLineString>();
        private List<GeoLineString> pls2 = new List<GeoLineString>();
        private List<GeoData.GeoDataRow> m_EditRows = new List<GIS.GeoData.GeoDataRow>();


        public override void Cancel()
        {
            pls1.Clear();
            pls2.Clear();
            m_EditRows.Clear();
            base.Cancel();
           
        }

        public override void Finish()
        {
            m_MapUI.OutPutTextInfo("提示：交点打断工具结束！\r\n");
        }


        int isfirst=0;
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            isfirst++;
            if (isfirst == 1)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (pls1.Count > 0)
                    {

                        GeometryComputation.LinesSplitLines.SplitLines(pls1, pls2, false);
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


                     

                        GeoVectorLayer layer = m_MapUI.GetLayerByTable
                            ((GeoData.GeoDataTable)m_EditRows[0].Table) as GeoVectorLayer;

                        List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
                        for (int i = 0; i < pls2.Count; i++)
                        {
                            GeoData.GeoDataRow rowNew = layer.AddGeometry(pls2[i]);

                            m_MapUI.InitialNewGeoFeature(rowNew);
                            m_list.Add(rowNew);
                            rowNew.EditState = EditState.Appear;

                            GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                            oprts.m_NewOperands.Add(oprtNew);

                        }
                        m_MapUI.OutPutTextInfo(string.Format("{0}条线打断为", pls1.Count));
                        m_MapUI.OutPutTextInfo(string.Format("{0}条线\r\n", pls2.Count));

                        m_MapUI.ClearAllSlt();
                        base.Cancel();

                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        m_MapUI.OutPutTextInfo("提示：取消交点打断工具！\r\n");
                        m_MapUI.ClearAllSlt();
                        base.Cancel();

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
}
