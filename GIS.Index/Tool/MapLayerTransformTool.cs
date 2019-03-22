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
        public class MapLayerTransformTool : MapTool
        {
            public MapLayerTransformTool(MapUI ui) : base(ui)
            {
                List<GeoData.GeoDataRow> m_EditRowsPl = new List<GIS.GeoData.GeoDataRow>();
                List<GeoData.GeoDataRow> m_EditRowsPg = new List<GIS.GeoData.GeoDataRow>();
                List<GeoData.GeoDataRow> m_EditRowsPt = new List<GIS.GeoData.GeoDataRow>();
                //是否选择目标
                if (m_MapUI.SltGeoSet.Count == 0)
                {
                    m_MapUI.OutPutTextInfo
                            ("提示：您没有选择目标，请选择后单击此功能\r\n");
                }
                else
                {//选择的目标分为点、线、面存储
                    for(int i=0;i<m_MapUI.SltGeoSet.Count;i++)
                    {
                        if (m_MapUI.SltGeoSet[i].Geometry is GeoPolygon)
                        {
                            GeoData.GeoDataRow editr = m_MapUI.SltGeoSet[i];
                            m_EditRowsPg.Add(editr);
                        }
                        if (m_MapUI.SltGeoSet[i].Geometry is GeoLineString)
                        {
                            GeoData.GeoDataRow editr = m_MapUI.SltGeoSet[i];
                            m_EditRowsPl.Add(editr);
                        }
                        if (m_MapUI.SltGeoSet[i].Geometry is GeoPoint)
                        {
                            GeoData.GeoDataRow editr = m_MapUI.SltGeoSet[i];
                            m_EditRowsPt.Add(editr);
                        }
                    }
                    //点线面分别转换
                    if (m_MapUI.SltGeoSet[0].Geometry is GeoPolygon)
                    {
                        if (m_EditRowsPt.Count + m_EditRowsPl.Count > 0)
                        {
                            m_MapUI.OutPutTextInfo
                           ("提示：您选择的目标种类不一致，只转换其中的面目标\r\n");
                        }
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
                            if (frm.ShowDialog() == DialogResult.OK)
                            {
                                string str = frm.LayerName;

                                ///////////////操作回退
                                GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                                m_MapUI.m_OprtManager.AddOprt(oprts);
                                ///////////////操作回退

                                for (int i = 0; i < m_EditRowsPg.Count; i++)
                                {
                                    EditState state = m_EditRowsPg[i].EditState;
                                    //if (m_EditRows[i].EditState == EditState.Original)
                                    //{
                                    m_EditRowsPg[i].EditState = EditState.Disappear;
                                    //}
                                    //else
                                    //    m_EditRows[i].EditState = EditState.Invalid;
                                    GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand
                                        (m_EditRowsPg[i], state, m_EditRowsPg[i].EditState);
                                    oprts.m_OldOperands.Add(oprtOld);
                                }



                                GeoVectorLayer layer = m_MapUI.GetLayerByName(str) as GeoVectorLayer;

                                List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
                                for (int i = 0; i < m_EditRowsPg.Count; i++)
                                {
                                    GeoData.GeoDataRow rowNew = layer.AddGeometry(m_EditRowsPg[i].Geometry);

                                    m_MapUI.InitialNewGeoFeature(rowNew);
                                    m_list.Add(rowNew);
                                    rowNew.EditState = EditState.Appear;

                                    GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                                    oprts.m_NewOperands.Add(oprtNew);

                                }
                                //m_MapUI.ClearAllSlt();
                            }
                        }
                    }
                    if (m_MapUI.SltGeoSet[0].Geometry is GeoLineString)
                    {
                        if (m_EditRowsPt.Count + m_EditRowsPg.Count > 0)
                        {
                            m_MapUI.OutPutTextInfo
                           ("提示：您选择的目标种类不一致，只转换其中的线目标\r\n");
                        }
                        int plgcount = 0;
                        GIS.Map.LayerGroup lyg = m_MapUI.GetGroupByName(m_MapUI.ActiveLyrGroupName) as LayerGroup;
                        for (int i = 0; i < lyg.Counts; i++)
                        {
                            if (lyg[i].LayerTypeDetail == GIS.Layer.LAYERTYPE_DETAIL.LineLayer)
                                plgcount++;
                        }
                        if (plgcount == 0)
                        {
                            m_MapUI.OutPutTextInfo("提示：请加载线图层！");
                        }
                        else
                        {
                            GIS.TreeIndex.Forms.PolygonToLineFrm frm = new GIS.TreeIndex.Forms.PolygonToLineFrm(m_MapUI);
                            frm.ShowDialog();
                            string str = frm.LayerName;

                            ///////////////操作回退
                            GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                            m_MapUI.m_OprtManager.AddOprt(oprts);
                            ///////////////操作回退

                            for (int i = 0; i < m_EditRowsPl.Count; i++)
                            {
                                EditState state = m_EditRowsPl[i].EditState;
                                //if (m_EditRows[i].EditState == EditState.Original)
                                //{
                                m_EditRowsPl[i].EditState = EditState.Disappear;
                                //}
                                //else
                                //    m_EditRows[i].EditState = EditState.Invalid;
                                GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand
                                    (m_EditRowsPl[i], state, m_EditRowsPl[i].EditState);
                                oprts.m_OldOperands.Add(oprtOld);
                            }



                            GeoVectorLayer layer = m_MapUI.GetLayerByName(str) as GeoVectorLayer;

                            List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
                            for (int i = 0; i < m_EditRowsPl.Count; i++)
                            {
                                GeoData.GeoDataRow rowNew = layer.AddGeometry(m_EditRowsPl[i].Geometry);

                                m_MapUI.InitialNewGeoFeature(rowNew);
                                m_list.Add(rowNew);
                                rowNew.EditState = EditState.Appear;

                                GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                                oprts.m_NewOperands.Add(oprtNew);

                            }
                            //m_MapUI.ClearAllSlt();
                        }
                    }
                    if (m_MapUI.SltGeoSet[0].Geometry is GeoPoint)
                    {
                        if (m_EditRowsPg.Count + m_EditRowsPl.Count > 0)
                        {
                            m_MapUI.OutPutTextInfo
                           ("提示：您选择的目标种类不一致，只转换其中的点目标\r\n");
                        }
                        int plgcount = 0;
                        GIS.Map.LayerGroup lyg = m_MapUI.GetGroupByName(m_MapUI.ActiveLyrGroupName) as LayerGroup;
                        for (int i = 0; i < lyg.Counts; i++)
                        {
                            if (lyg[i].LayerTypeDetail == GIS.Layer.LAYERTYPE_DETAIL.PointLayer)
                                plgcount++;
                        }
                        if (plgcount == 0)
                        {
                            m_MapUI.OutPutTextInfo("提示：请加载点图层！");
                        }
                        else
                        {
                            TreeIndex.Forms.SelectPointLayerForm frm = new GIS.TreeIndex.Forms.SelectPointLayerForm(m_MapUI);
                            frm.ShowDialog();
                            string str = frm.LayerName;

                            ///////////////操作回退
                            GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                            m_MapUI.m_OprtManager.AddOprt(oprts);
                            ///////////////操作回退

                            for (int i = 0; i < m_EditRowsPt.Count; i++)
                            {
                                EditState state = m_EditRowsPt[i].EditState;
                                //if (m_EditRows[i].EditState == EditState.Original)
                                //{
                                m_EditRowsPt[i].EditState = EditState.Disappear;
                                //}
                                //else
                                //    m_EditRows[i].EditState = EditState.Invalid;
                                GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand
                                    (m_EditRowsPt[i], state, m_EditRowsPt[i].EditState);
                                oprts.m_OldOperands.Add(oprtOld);
                            }



                            GeoVectorLayer layer = m_MapUI.GetLayerByName(str) as GeoVectorLayer;

                            List<GeoData.GeoDataRow> m_list = new List<GIS.GeoData.GeoDataRow>();
                            for (int i = 0; i < m_EditRowsPt.Count; i++)
                            {
                                GeoData.GeoDataRow rowNew = layer.AddGeometry(m_EditRowsPt[i].Geometry);

                                m_MapUI.InitialNewGeoFeature(rowNew);
                                m_list.Add(rowNew);
                                rowNew.EditState = EditState.Appear;

                                GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                                oprts.m_NewOperands.Add(oprtNew);

                            }
                            //m_MapUI.ClearAllSlt();
                        }
                    }
                    m_MapUI.ClearAllSlt();
                }
            }
        }
}
