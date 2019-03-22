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
    public class MapRotateLabelTool : MapTool
    {
        public MapRotateLabelTool(MapUI ui)
            : base(ui)
        {
            m_EditingRowsBackUp = new List<GIS.GeoData.GeoDataRow>();
            initial();
        }
        private double m_Angle = 0;
        private GeoPoint m_PointBefore;
        private GeoPoint m_PointOrign;
        private RotateState m_RotateState = RotateState.State_Select;
        private List<GeoData.GeoDataRow> m_EditingRowsBackUp;//节点移动的目标记录备份，用作增量信息前对象

        private enum RotateState
        {
            State_Select,         //选择截断
            State_PositionFirst,  //定位第一点
            State_PositionSecond, //定位第二点
            State_Rotate          //旋转，找第三点
        }
        public override void Cancel()
        {
            if (m_RotateState == RotateState.State_Select)  //如果已经再拖动 ，就取消
            {
                if (m_MapUI.SltLabelSet.Count == 0)
                {
                    base.Cancel();
                    return;
                }
                m_RotateState = RotateState.State_PositionFirst;
                m_MapUI.OutPutTextInfo("提示：请选择旋转的基点！\r\n");
            }
            else if (m_RotateState == RotateState.State_PositionFirst)
            {
                m_RotateState = RotateState.State_Select;
                m_MapUI.OutPutTextInfo("提示：请继续选择需要旋转的对象！\r\n");
            }
            else if (m_RotateState == RotateState.State_PositionSecond)
            {
                m_RotateState = RotateState.State_PositionFirst;
                m_MapUI.OutPutTextInfo("提示：请重新选择旋转的基点！\r\n");
            }
            else if (m_RotateState == RotateState.State_Rotate)
            {
                m_Angle = 0;
                m_RotateState = RotateState.State_PositionSecond;
                m_EditingRowsBackUp.Clear();
                for (int i = 0; i < m_MapUI.SltLabelSet.Count; i++)
                {
                    m_EditingRowsBackUp.Add(m_MapUI.SltLabelSet[i].Clone());
                }
                m_MapUI.RePaint();
                m_MapUI.OutPutTextInfo("提示：请重新开始旋转！\r\n");
            }
        }
        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_RotateState == RotateState.State_Select)
                {
                    SelectLabels(sender, e);
                }
                else if (m_RotateState == RotateState.State_PositionFirst)
                {
                    PositionFirst(sender, e);
                }
                else if (m_RotateState == RotateState.State_PositionSecond)
                {
                    PositionSecond(sender, e);
                }
                else if (m_RotateState == RotateState.State_Rotate)
                {
                    PositionFinish(sender, e);
                }
            }
            else if (e.Button == MouseButtons.Right) //鼠标的右键取消命令
            {
                Cancel();
            }
        }
        private void PositionFinish(object sender, MouseEventArgs e)
        {
            m_RotateState = RotateState.State_Select;
            for (int i = 0; i < m_MapUI.SltLabelSet.Count; i++)
            {
                m_MapUI.SltLabelSet[i].Geometry.RotateAt(m_Angle, m_PointOrign);
            }
            m_MapUI.Refresh();
            base.Cancel();
        }
        private void PositionSecond(object sender, MouseEventArgs e)
        {
            GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                          m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
            m_PointBefore = pt;
            m_RotateState = RotateState.State_Rotate;
        }

        private void PositionFirst(object sender, MouseEventArgs e)
        {
            GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                           m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
            m_PointOrign = pt;
            m_EditingRowsBackUp.Clear();
            for (int i = 0; i < m_MapUI.SltLabelSet.Count; i++)
            {
                m_EditingRowsBackUp.Add(m_MapUI.SltLabelSet[i].Clone());
            }
            m_RotateState = RotateState.State_PositionSecond;
            m_MapUI.OutPutTextInfo("提示：左键点击确认开始的平移位置。\r\n");
        }

        private void SelectLabels(object sender, MouseEventArgs e)
        {
            if (m_MapUI.SelectByPt(e.Location, SelectType.Label) != null)
            {
                m_MapUI.Refresh();
            }
            int sltCount = m_MapUI.SltLabelSet.Count;
            m_MapUI.OutPutTextInfo(string.Format("提示： 选中 {0} 个目标!\r\n", sltCount));
            return;
        }
        public override void initial()
        {
            if (m_MapUI.SltLabelSet.Count == 0)
            {
                m_RotateState = RotateState.State_Select;
                m_MapUI.OutPutTextInfo("提示：旋转工具开始，请先选择目标后，点左键选择旋转基点开始平移！\r\n");
            }
            else
            {
                m_RotateState = RotateState.State_PositionFirst;
                m_MapUI.OutPutTextInfo("提示： 请选择旋转基点开始平移！\r\n");
            }

            m_Angle = 0;
        }
        public override void Finish()
        {
            for (int i = 0; i < m_MapUI.SltLabelSet.Count; i++)
            {
                GeoData.GeoDataRow row = m_MapUI.SltLabelSet[i];
                m_MapUI.BoundingBoxChangedBy(row);
            }
            m_MapUI.OutPutTextInfo("提示：旋转工具结束！\r\n");
        }

        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (m_RotateState > RotateState.State_Select)
            {
                GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
                m_MapUI.MouseCatch(pt);
                Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
                Graphics g = Graphics.FromImage(imgTemp);
                m_MapUI.RePaint(g);

                if (m_RotateState == RotateState.State_Rotate)
                {
                    double angle2 = SpatialRelation.GeoAlgorithm.CalcAzimuth(m_PointOrign.X, m_PointOrign.Y, pt.X, pt.Y);

                    double angle1 = SpatialRelation.GeoAlgorithm.CalcAzimuth(m_PointOrign.X, m_PointOrign.Y, m_PointBefore.X, m_PointBefore.Y);

                    double deltaAngle = angle2 - angle1;
                    m_Angle += deltaAngle;


                    m_PointBefore = pt;

                    Style.VectorStyle style = new Style.VectorStyle(2, 2, Color.Orchid, Color.Orchid, false);
                    for (int i = 0; i < m_EditingRowsBackUp.Count; i++)
                    {
                        m_EditingRowsBackUp[i].Geometry.RotateAt(deltaAngle, m_PointOrign);
                        GIS.Render.RenderAPI.DrawLabel(g, (GeoLabel)m_EditingRowsBackUp[i].Geometry, Color.Blue, m_MapUI);
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
