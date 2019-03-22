using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GIS.Layer;
using GIS.Geometries;
using GIS.Map;
using GIS.SpatialRelation;

namespace GIS.TreeIndex.Tool
{
    public class MapSpiltLineTool:MapTool
    {
        public MapSpiltLineTool(MapUI ui)
            : base(ui)
        {
            initial();
        }
        private enum SpiltState
        {
            State_Select, //选择线的状态
            State_FirstSpiltPt, //打断第一个点
            State_SecondSpiltPt //打断第二个点
        }
        private SpiltState m_SpiltState = SpiltState.State_Select;
        private GeoData.GeoDataRow m_EditingRow;
        private GeoLineString m_SnapLine = null;

        private int ptIndex1 = -1;
        private GeoPoint spiltPt1 = null;

        private int ptIndex2 = -1;
        private GeoPoint spiltPt2 = null;

        private int ptIndex = -1;


        public override void initial()
        {
            if (m_MapUI.SltGeoSet.Count !=1 )
            {
                m_SpiltState = SpiltState.State_Select;
                m_MapUI.OutPutTextInfo("提示：打断线工具开始，请先选择目标后，点左键选择打断点！\r\n");
            }
            else if (!(m_MapUI.SltGeoSet[0].Geometry is GeoLineString))
            {
                m_MapUI.ClearAllSlt();
                m_SpiltState = SpiltState.State_Select;
                m_MapUI.OutPutTextInfo("提示：打断线工具开始，当前选择目标不是单线，请选择目标直线！\r\n");
            }
            else
            {
                m_SpiltState = SpiltState.State_FirstSpiltPt;
                m_EditingRow = m_MapUI.SltGeoSet[0];
                m_MapUI.OutPutTextInfo("提示：打断线工具开始，请左键点击打断线的第一个点后，按右键结束，左键选择第二个点！\r\n");
            }
            
        }
        public override void Cancel()
        {
            if (m_SpiltState == SpiltState.State_Select)
            {
                m_MapUI.ClearAllSlt();
                base.Cancel();
            }
            if (m_SpiltState == SpiltState.State_SecondSpiltPt)
            {
                if (ptIndex1 != -1 && m_SnapLine != null)
                {
                    ///////////////操作回退
                    GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                    m_MapUI.m_OprtManager.AddOprt(oprts);
                    ///////////////操作回退

                    GeoLineString newLine1 = new GeoLineString();
                    for (int i = 0; i < ptIndex1; i++)
                    {
                        newLine1.Vertices.Add(m_SnapLine.Vertices[i]);
                    }
                    newLine1.Vertices.Add(spiltPt1.Clone() as GeoPoint);

                    GeoLineString newLine2 = new GeoLineString();
                    newLine2.AddPoint(spiltPt1.Clone() as GeoPoint);
                    for (int i = ptIndex1; i < m_SnapLine.Vertices.Count; i++)
                    {
                        newLine2.Vertices.Add(m_SnapLine.Vertices[i]);
                    }
                    GeoVectorLayer layer = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)m_EditingRow.Table) as GeoVectorLayer;

                    GeoData.GeoDataRow newRow1 = layer.AddGeometry(newLine1);
                  
                    GeoData.GeoDataRow newRow2 = layer.AddGeometry(newLine2);

                    EditState state = m_EditingRow.EditState;
                    for (int i = 1; i < m_EditingRow.ItemArray.Length; i++)
                    {
                        newRow1[i] = m_EditingRow[i].ToString();
                        newRow2[i] = m_EditingRow[i].ToString();
                    }
                    if (m_EditingRow.EditState == EditState.Original)
                    {
                        m_EditingRow.EditState = EditState.Disappear;
                    }
                    else
                        m_EditingRow.EditState = EditState.Invalid;
                    newRow1.EditState = EditState.Appear;
                    newRow2.EditState = EditState.Appear;

                    ///////////////操作回退
                    GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(newRow1, EditState.Invalid, newRow1.EditState);
                    oprts.m_NewOperands.Add(oprtNew);
                    GIS.TreeIndex.OprtRollBack.Operand oprtNew1 = new GIS.TreeIndex.OprtRollBack.Operand(newRow2, EditState.Invalid, newRow2.EditState);
                    oprts.m_NewOperands.Add(oprtNew1);

                    GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(m_EditingRow, state, m_EditingRow.EditState);
                    oprts.m_OldOperands.Add(oprtOld);
                    ///////////////操作回退

                    m_MapUI.ClearAllSlt();
                    base.Cancel();
                }
            }
        }
        public override void Finish()
        { 
            m_MapUI.OutPutTextInfo("提示：打断线工具结束！\r\n");
        }
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_SpiltState == SpiltState.State_Select)
                {
                    SelectLine(e);
                }
                else if (m_SpiltState == SpiltState.State_FirstSpiltPt)
                {
                    SpiltFirstPoint(e);
                }
                else if (m_SpiltState == SpiltState.State_SecondSpiltPt)
                {
                    SpiltSecondPoint(e);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                Cancel();
            }
        }

        private void SpiltSecondPoint(MouseEventArgs e)
        {
            if (ptIndex != -1 && m_SnapLine != null && m_MapUI.m_SnapPoint != null)
            {  
                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                m_MapUI.m_OprtManager.AddOprt(oprts); 
                EditState state = m_EditingRow.EditState;
                ///////////////操作回退

                ptIndex2 = ptIndex;
                spiltPt2 = m_MapUI.m_SnapPoint; 

                if (ptIndex1 == ptIndex2)
                {
                    double dist1 = spiltPt1.DistanceTo(m_SnapLine.Vertices[ptIndex1-1]);
                    double dist2 = spiltPt2.DistanceTo(m_SnapLine.Vertices[ptIndex1-1]);
                    GeoPoint ptTemp = null;
                    if (dist2 < dist1)
                    {
                        ptTemp = spiltPt1;
                        spiltPt1 = spiltPt2;
                        spiltPt2 = ptTemp;
                    }
                }
                else if (ptIndex1 > ptIndex2)
                {
                    GeoPoint ptTemp = spiltPt1;
                    spiltPt1 = spiltPt2;
                    spiltPt2 = ptTemp;
                }
                GeoLineString newLine1 = new GeoLineString();
                for (int i = 0; i < Math.Min(ptIndex1, ptIndex2); i++)
                {
                    newLine1.Vertices.Add(m_SnapLine.Vertices[i]);
                }
                newLine1.Vertices.Add(spiltPt1); 

                GeoLineString newLine2 = new GeoLineString();
                newLine2.Vertices.Add(spiltPt2);
                for (int i = Math.Max(ptIndex1, ptIndex2) ; i < m_SnapLine.Vertices.Count; i++)
                {
                    newLine2.Vertices.Add(m_SnapLine.Vertices[i]);
                }

                GeoVectorLayer layer = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)m_EditingRow.Table) as GeoVectorLayer;
                GeoData.GeoDataRow newRow1 = layer.AddGeometry(newLine1);
                GeoData.GeoDataRow newRow2 = layer.AddGeometry(newLine2);

                for (int i = 1; i < m_EditingRow.ItemArray.Length; i++)
                {
                    newRow1[i] = m_EditingRow[i].ToString();
                    newRow2[i] = m_EditingRow[i].ToString();
                }
                if (m_EditingRow.EditState == EditState.Original)
                {
                    m_EditingRow.EditState = EditState.Disappear;
                }
                else
                    m_EditingRow.EditState = EditState.Invalid;
                newRow1.EditState = EditState.Appear;
                newRow2.EditState = EditState.Appear;
                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(newRow1, EditState.Invalid, newRow1.EditState);
                oprts.m_NewOperands.Add(oprtNew);
                GIS.TreeIndex.OprtRollBack.Operand oprtNew1 = new GIS.TreeIndex.OprtRollBack.Operand(newRow2, EditState.Invalid, newRow2.EditState);
                oprts.m_NewOperands.Add(oprtNew1);

                GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(m_EditingRow, state, m_EditingRow.EditState);
                oprts.m_OldOperands.Add(oprtOld);
                ///////////////操作回退
                m_MapUI.ClearAllSlt();
                base.Cancel();
            }
        }

        private void SpiltFirstPoint(MouseEventArgs e)
        {
            if (ptIndex != -1 && m_SnapLine != null && m_MapUI.m_SnapPoint != null)
            {
                ptIndex1 = ptIndex;
                spiltPt1 = m_MapUI.m_SnapPoint;
                m_SpiltState = SpiltState.State_SecondSpiltPt;
                m_MapUI.OutPutTextInfo("提示： 按右键直接打断，左键选择第二个点后打断！\r\n");
            }
        }
        private void SelectLine(System.Windows.Forms.MouseEventArgs e)
        { 
            GeoData.GeoDataRow row = m_MapUI.SelectByPt(e.Location, SelectType.Line);
            if (row != null && row.Geometry is GeoLineString)
            {
                m_EditingRow = row;
                m_SpiltState = SpiltState.State_FirstSpiltPt;
                m_MapUI.OutPutTextInfo("提示：请左键点击打断线的第一个点后，按右键结束，左键选择第二个点！\r\n");
                m_MapUI.Refresh();
            }
            else
            {
                m_MapUI.ClearAllSlt();
                m_MapUI.OutPutTextInfo("提示：打断线工具开始，当前选择目标不是单线，请选择目标直线！\r\n");
            }
       
        }
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if ((int)m_SpiltState >= (int)SpiltState.State_FirstSpiltPt)
            {
                GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);

                GeoPoint ptVertical;
                GeoLineString line = GeoAlgorithm.VerticalPtOfPtToGeometry(pt, m_EditingRow.Geometry, out ptVertical, out ptIndex);

                if (line != null &&
                    GeoAlgorithm.IsOnPointCoarse(ptVertical, pt))
                {
                    m_SnapLine = line;
                    m_MapUI.m_SnapPoint = ptVertical;
                    Bitmap bmp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
                    Graphics g = Graphics.FromImage(bmp);
                    m_MapUI.RePaint(g);
                    Point ppt = m_MapUI.TransFromWorldToMap(ptVertical);
                    Pen pen = new Pen(Color.Red, 0.5f);
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawEllipse(pen, new Rectangle(ppt.X - 3, ppt.Y - 3, 6, 6));
                    g.DrawRectangle(pen, new Rectangle(ppt.X - m_MapUI.SnapPixels, ppt.Y - m_MapUI.SnapPixels, 2 * m_MapUI.SnapPixels, 2 * m_MapUI.SnapPixels));
                    m_MapUI.Image = bmp;

                    m_MapUI.BaseRefresh();
                    g.Dispose();
                }
                else
                { 
                    ptIndex = -1;
                }
            }
        }
    }
}
