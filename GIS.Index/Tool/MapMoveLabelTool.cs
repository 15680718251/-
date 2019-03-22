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
    public class MapMoveLabelTool :MapTool
    {
        public MapMoveLabelTool(MapUI ui)
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
                if (m_MapUI.SltLabelSet.Count == 0)
                {
                    base.Cancel();
                    return;
                }
                m_MoveState = MoveState.State_PositionSecond;
                m_MapUI.OutPutTextInfo("提示： 开始注记平移，左键点击确认平移位置。\r\n");
            }
            else if (m_MoveState == MoveState.State_PositionSecond)
            {
                m_MoveState = MoveState.State_Select;
                m_MapUI.OutPutTextInfo("提示：请继续选择需要移动的对象！\r\n");
            }
    
        }
        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_MoveState == MoveState.State_Select)
                {
                    SelectLabels(sender, e);
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
            GeoPoint pt =  m_MapUI.TransFromMapToWorld(e.Location)  ;
            double deltaX = pt.X - m_PointOrign.X;
            double deltaY = pt.Y - m_PointOrign.Y;

            for (int i = 0; i < m_MapUI.SltLabelSet.Count; i++)
            {
                m_MapUI.SltLabelSet[i].Geometry.Move(deltaX, deltaY);
            }
            m_MapUI.Refresh();
            base.Cancel();
        }

        

        protected void SelectLabels(object sender, MouseEventArgs e)
        {
            if (m_MapUI.SelectByPt(e.Location, SelectType.Label) != null)
            {
                m_MapUI.Refresh();
            }
            int sltCount = m_MapUI.SltLabelSet.Count;
            m_MapUI.OutPutTextInfo(string.Format("提示： 选中 {0} 个目标,点击右键开始平移\r\n", sltCount));
            GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
            m_PointBefore = pt;
            m_PointOrign = pt;
            m_EditingRowsBackUp.Clear();
            for (int i = 0; i < m_MapUI.SltLabelSet.Count; i++)
            {
                m_EditingRowsBackUp.Add(m_MapUI.SltLabelSet[i].Clone());
            }
 
        }
        public override void initial()
        { 
                m_MoveState = MoveState.State_Select;
                m_MapUI.OutPutTextInfo("提示：注记平移工具开始，请先选择目标后，点右键开始平移！\r\n");
            

        }
        public override void Finish()
        {
            for (int i = 0; i < m_MapUI.SltLabelSet.Count; i++)
            {
                GeoData.GeoDataRow row = m_MapUI.SltLabelSet[i];
                m_MapUI.BoundingBoxChangedBy(row);
            }
            m_MapUI.OutPutTextInfo("提示：注记平移工具结束！\r\n");
        }

        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (m_MoveState > MoveState.State_Select)
            {
                GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
                
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
                        GIS.Render.RenderAPI.DrawLabel(g, (GeoLabel)m_EditingRowsBackUp[i].Geometry, Color.Red, m_MapUI);
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
