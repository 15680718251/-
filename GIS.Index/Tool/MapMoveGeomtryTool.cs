using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GIS.Geometries;
using GIS.Layer;
namespace GIS.TreeIndex.Tool
{
    public class MapMoveGeomtryTool:MapTool
    {
        public MapMoveGeomtryTool(MapUI ui)
            : base(ui)
        {
            m_EditingRowsBackUp = new List<GIS.GeoData.GeoDataRow>();
            initial();
        }
        protected GeoPoint m_PointBefore; //移动前的点
        protected GeoPoint m_PointOrign;
        protected MoveState m_MoveState = MoveState.State_Select;
        protected List<GeoData.GeoDataRow> m_EditingRowsBackUp;//节点移动的目标记录备份，用作增量信息前对象

        protected enum MoveState
        {
            State_Select, //选择截断
            State_PositionFirst, //定位第一点
            State_PositionSecond //定位第二点
        }
        public override void Cancel()
        {
            if (m_MoveState == MoveState.State_Select)  //如果已经再拖动 ，就取消
            {
                if (m_MapUI.SltGeoSet.Count == 0)
                {
                    base.Cancel();
                    return;
                }
                m_MoveState = MoveState.State_PositionFirst;
                m_MapUI.OutPutTextInfo("提示：请选择移动的基点！\r\n");
            }
            else if (m_MoveState == MoveState.State_PositionFirst)
            {
                m_MoveState = MoveState.State_Select;
                m_MapUI.OutPutTextInfo("提示：请继续选择需要移动的对象！\r\n");
            }
            else if (m_MoveState == MoveState.State_PositionSecond)
            {
                m_MoveState = MoveState.State_PositionFirst;
                m_MapUI.RePaint();
                m_MapUI.OutPutTextInfo("提示：请重新选择移动的基点！\r\n");
            }
        }
        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_MoveState == MoveState.State_Select)
                {
                    SelectGeometries(sender, e);
                }
                else if (m_MoveState == MoveState.State_PositionFirst)
                {
                    PositionFirst(sender, e);
                }
                else if (m_MoveState == MoveState.State_PositionSecond)
                {
                    PositionSecond(sender, e);             
                }
            }
            else if (e.Button == MouseButtons.Right) //鼠标的右键取消命令
            {
                Cancel();
            }
        }

        protected virtual void PositionSecond(object sender, MouseEventArgs e)
        {
            GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                          m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
            double deltaX = pt.X - m_PointOrign.X;
            double deltaY = pt.Y - m_PointOrign.Y;

            ///////////////操作回退
            GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
            m_MapUI.m_OprtManager.AddOprt(oprts);
            ///////////////操作回退

            for (int i = 0; i < m_MapUI.SltGeoSet.Count; i++)
            {
                GeoData.GeoDataRow row = m_MapUI.SltGeoSet[i];
                GIS.GeoData.GeoDataRow NewRow = row.Clone();
                ((GIS.GeoData.GeoDataTable)row.Table).AddRow(NewRow);
                EditState state = row.EditState;

                if (row.EditState == EditState.Original)
                {
                    row.EditState = EditState.GeometryBef;
                    NewRow.EditState = EditState.GeometryAft;
                }
                else
                {
                    row.EditState = EditState.Invalid;
                    NewRow.EditState = state;
                }

                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(NewRow, EditState.Invalid, NewRow.EditState);
                oprts.m_NewOperands.Add(oprtNew);

                GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(row, state, row.EditState);
                oprts.m_OldOperands.Add(oprtOld);
                ///////////////操作回退

                NewRow.Geometry.Move(deltaX, deltaY);

            }
            m_MapUI.Refresh();
            base.Cancel();
        }

        protected void PositionFirst(object sender, MouseEventArgs e)
        {
            GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                           m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
            m_PointBefore = pt;
            m_PointOrign = pt;
            m_EditingRowsBackUp.Clear();
            for (int i = 0; i < m_MapUI.SltGeoSet.Count; i++)
            {
                m_EditingRowsBackUp.Add(m_MapUI.SltGeoSet[i].Clone());
            }
            m_MoveState = MoveState.State_PositionSecond;
            m_MapUI.OutPutTextInfo("提示： 开始平移，左键点击确认平移位置。\r\n");
        }

        protected void SelectGeometries(object sender, MouseEventArgs e)
        { 
            if (m_MapUI.SelectByPt(e.Location, SelectType.Geomtry) != null)
            {
                m_MapUI.Refresh();
            }
            int sltCount = m_MapUI.SltGeoSet.Count;
            m_MapUI.OutPutTextInfo(string.Format("提示： 选中 {0} 个目标,点击右键选择平移基点开始平移\r\n",sltCount));
            return;
        }
        public override void initial()
        {
            if (m_MapUI.SltGeoSet.Count == 0)
            {
                m_MoveState = MoveState.State_Select;
                m_MapUI.OutPutTextInfo("提示：平移工具开始，请先选择目标后，点左键选择平移基点开始平移！\r\n");
            }
            else
            {
                m_MoveState = MoveState.State_PositionFirst;
                m_MapUI.OutPutTextInfo("提示： 请选择平移基点开始平移！\r\n");
            }
           
        }
        public override void Finish()
        {
            for(int i =0;i<m_MapUI.SltGeoSet.Count;i++)
            {
                GeoData.GeoDataRow row = m_MapUI.SltGeoSet[i];
                m_MapUI.BoundingBoxChangedBy(row);
            }
            m_MapUI.OutPutTextInfo("提示：平移工具结束！\r\n");
        }

        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (m_MoveState > MoveState.State_Select)
            {
                GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
                m_MapUI.MouseCatch(pt);
                Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
                Graphics g = Graphics.FromImage(imgTemp);
                m_MapUI.RePaint(g);

                if (m_MoveState == MoveState.State_PositionSecond)
                {
                    double deltaX = pt.X - m_PointBefore.X;
                    double deltaY = pt.Y - m_PointBefore.Y;
                    m_PointBefore = pt;

                    Style.VectorStyle style = new Style.VectorStyle(2, 2, Color.Orchid, Color.Orchid, false);
                    for (int i = 0; i < m_EditingRowsBackUp.Count; i++)
                    {
                        m_EditingRowsBackUp[i].Geometry.Move(deltaX, deltaY);
                        GIS.Render.RenderAPI.DrawGeometry(g, m_EditingRowsBackUp[i].Geometry, style, m_MapUI);
                    }

                }
                m_MapUI.Image.Dispose();
                m_MapUI.Image = imgTemp;
                g.Dispose();
                m_MapUI.BaseRefresh();
            }
        }
  
    }
}
