using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GIS.Geometries;
using GIS.Map;
using GIS.Layer;

namespace GIS.TreeIndex.Tool
{
    public class MapAddPolygonTool : MapAddGeometryTool
    {
        protected MapAddPolygonTool()
        {
        }
        public MapAddPolygonTool(MapUI ui)
            : base(ui)
        {
            m_MapUI.OutPutTextInfo("画面工具激活：指定起始点 \r\n");
        }
        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
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
                GeoVectorLayer actLayer = m_MapUI.GetActiveVectorLayer() as GeoVectorLayer;
                if (actLayer != null &&
                    (actLayer.VectorType == VectorLayerType.PolygonLayer
                    || actLayer.VectorType == VectorLayerType.MixLayer ||
                      actLayer.VectorType == VectorLayerType.DraftLayer))
                {
                    if (m_PtList.Count >= 3)
                    {
                        GeoLinearRing ring = new GeoLinearRing(m_PtList);
                        m_Geom = actLayer.AddGeometry(new GeoPolygon(ring));
                        m_MapUI.OutPutTextInfo("提示：  画面成功，退出面构建命令\r\n");
                        base.Finish();
                        m_MapUI.m_EditToolBack = m_MapUI.MapTool;
                        m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
                    }
                    else
                    {
                        m_MapUI.OutPutTextInfo("提示：  画面失败，退出画面命令\r\n");
                        m_MapUI.OutPutTextInfo("提示：  构面点数少于3个，无法完成面的绘制\r\n");
                    }
                }
                else
                {
                    MessageBox.Show("当前活动层不是面层，请指定目标图层");
                }
            }
            else
                base.Cancel();
        }
        public override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            GeoPoint SnapingPt = m_MapUI.TransFromMapToWorld(e.Location);
            if (m_CurEditType != EditType.Vertical)//如果是垂涎命令，则不捕捉端点和中点
                m_MapUI.MouseCatch(SnapingPt);
            else m_MapUI.m_SnapPoint = null;
           
            int nCount = m_PtList.Count;
            if (nCount > 0)
            {
                Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
                Graphics g = Graphics.FromImage(imgTemp);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                m_MapUI.RePaint(g);

         
                if (nCount > 1 && m_CurEditType != EditType.Vertical && m_MapUI.LineAngleSnapEnable)//// 在移动时 根据角度 捕捉 相应的点如果是垂涎命令，则不捕捉端点和中点
                    VerticalAndHorizontalRender(g, m_PtList[nCount - 2], m_PtList[nCount - 1], SnapingPt, 90, 3);

                Point[] pts = GetCurDrawPoints(e);
                if (pts == null)
                    return;

                Brush brush = new SolidBrush(Color.FromArgb(150, 255, 0, 0));
                Pen pen = new Pen(Color.Blue, 1f);                //画笔颜色
                g.FillPolygon(brush, pts);
                g.DrawLines(pen, pts);

                VerticalPtRender(g, SnapingPt);

                m_MapUI.Image.Dispose();
                m_MapUI.Image = imgTemp;

                brush.Dispose();
                pen.Dispose();
                g.Dispose();
                m_MapUI.BaseRefresh();
            }
            else
            {
                m_MapUI.RePaint();
            }
        }

        public override void initial()
        {
            base.initial();
            m_MapUI.OutPutTextInfo("画面工具激活：指定起始点 \r\n");
        }
    }
}
