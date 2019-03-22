using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.GeoData;
using GIS.Geometries;
using GIS.Layer;
namespace GIS.TreeIndex.Tool
{
    public  class MapLineContinueTool:MapGenerateLineTool
    {
        public MapLineContinueTool(MapUI ui)
            : base(ui)
        {
            m_MapUI.OutPutTextInfo("直线延长工具激活：选择直线端点 \r\n");
        }
        private GeoLineString m_Line = null;        //捕捉到的线 
        private bool m_bSnapedVertex=false;         //是否捕捉到端点
        private bool m_bSnapedBegin = false;        //捕捉到起点
        private GeoDataRow m_EditingRowNew;      //线延长的目标记录的目标记录备份，用作增量信息前对象
        private GeoDataRow m_EditingRow;               //线延长的目标记录
        private void SnapVertex(MouseEventArgs e)
        {
            GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
            m_EditingRow = m_MapUI.SelectByPt(e.Location, SelectType.Line);
            if (m_EditingRow != null)
            {
                m_EditingRowNew = m_EditingRow.Clone();
                m_Line = m_EditingRowNew.Geometry as GeoLineString;
                if (m_Line == null)
                    return;
                if (SnapBeginPoint(pt, m_Line))
                {
                    m_Line.Vertices.Reverse();
                    m_bSnapedBegin = true;
                }
                base.m_PtList = m_Line.Vertices;         //将线的坐标传到画线工具
                m_bSnapedVertex = true;                 //是否捕捉到端点
                m_MapUI.OutPutTextInfo(strLevelOne);    //提示输出
                base.m_KeyType = KeyType.ALL;         //接受键盘消息
               
            }
        }
        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!m_bSnapedVertex)
                {
                    SnapVertex(e);
                }
                else
                    base.OnMouseDown(sender, e);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Cancel();
            }           
        }
        public override void Cancel()
        {
            if (m_CurEditType == EditType.AddOnePoint)
            {
                m_MapUI.OutPutTextInfo("提示：  退线段延长命令\r\n");
                m_bSnapedVertex = false;
                if (m_Line != null)
                {
                    if (m_bSnapedBegin)
                    {
                        m_Line.Vertices.Reverse();
                        m_bSnapedBegin = false;
                    }
                    if (m_EditingRowNew != null)
                    {
                        EditState state = m_EditingRow.EditState;
                        ((GeoDataTable)m_EditingRow.Table).AddRow(m_EditingRowNew); //保存的数据不添加到图层中，由增量管理器管理
                        if (m_EditingRow.EditState == EditState.Original)//几何变化的增量信息
                        {
                            m_EditingRowNew.EditState = EditState.GeometryAft;
                            m_EditingRow.EditState = EditState.GeometryBef;
                            m_EditingRow["BeginTime"] = MapUI.BeginTime;
                        }
                        else
                        {
                            m_EditingRowNew.EditState = state;
                            m_EditingRow.EditState = EditState.Invalid;
                        }
                        ///////////////操作回退
                        GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                        m_MapUI.m_OprtManager.AddOprt(oprts);

                        GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(m_EditingRowNew, EditState.Invalid, m_EditingRowNew.EditState);
                        oprts.m_NewOperands.Add(oprtNew);

                        GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(m_EditingRow, state, m_EditingRow.EditState);
                        oprts.m_OldOperands.Add(oprtOld);
                        ///////////////操作回退

                    }
                    m_MapUI.BoundingBoxChangedBy(m_EditingRow);//重新计算边界矩形
                    m_MapUI.Refresh();
                    m_MapUI.m_EditToolBack = m_MapUI.MapTool;
                    m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
                    m_Line = null;
                }
            }
            else
                base.Cancel();
        }
 
        /// <summary>
        /// 初始化
        /// </summary>
        public override void initial()
        {
            m_MapUI.OutPutTextInfo("直线延长工具激活：选择直线端点 \r\n");
            base.initial();
            m_EditingRowNew = null;          
            m_Line = null;
        }
        //捕捉的是起点还是终点
        private bool SnapBeginPoint(GeoPoint pt , GeoLineString line)
        {
            double dis2begin = GIS.SpatialRelation.GeoAlgorithm.DistanceOfTwoPt(pt, line.StartPoint);
            double dis2end = GIS.SpatialRelation.GeoAlgorithm.DistanceOfTwoPt(pt, line.EndPoint);
            return dis2end > dis2begin;
     
        }
    }
}
