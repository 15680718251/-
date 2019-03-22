using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Text;
using GIS.Geometries;
using GIS.Layer;
using GIS.SpatialRelation;

namespace GIS.TreeIndex.Tool
{
    public class MapAddArcTool:MapAddGeometryTool
    {

        public MapAddArcTool(MapUI ui,ArcType type)
            : base(ui)
        {
            m_Type = type; 
            m_PtList = new List<GeoPoint>();
            m_PtTemp = new List<GeoPoint>(); 
            initial();
        }
        private ArcType m_Type; 
        protected new List<GeoPoint> m_PtList;      //最终点
        protected new List<GeoPoint> m_PtTemp;      //临时骨架点
        public override void initial()
        {
            m_Cursor = Cursors.Cross;
            m_PtTemp.Clear();
            switch (m_Type)
            {
                case ArcType.Arc:
                    m_MapUI.OutPutTextInfo("提示：三点画弧工具启动，请添加目标弧上的三个点\r\n");
                    break;
                case ArcType.Circle:
                    m_MapUI.OutPutTextInfo("提示：三点画圆工具启动：请添加三个目标圆上的点\r\n");
                    break;
            }
        }
        public override void Cancel()
        {
            GeoVectorLayer actLayer = m_MapUI.GetActiveVectorLayer() as GeoVectorLayer;
            if (actLayer != null &&
                (actLayer.VectorType == VectorLayerType.LineLayer
                || actLayer.VectorType == VectorLayerType.MixLayer ||
                  actLayer.VectorType == VectorLayerType.DraftLayer))
            {
                if (m_PtTemp.Count >= 3)
                {
                    GeoArc arc = new GeoArc(m_PtTemp, m_Type);
                    m_Geom = actLayer.AddGeometry(arc);
                    m_MapUI.OutPutTextInfo("提示：  三点画弧成功，退出线构建命令\r\n");
               
                }
                else
                {
                    m_MapUI.OutPutTextInfo("提示：  画线失败，退出画线命令\r\n");
                    m_MapUI.OutPutTextInfo("提示：  构线点数少于3个，无法完成样三点弧的绘制\r\n");
                }
                m_MapUI.m_EditToolBack = m_MapUI.MapTool;
                m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
            }
            else
            {
                MessageBox.Show("当前活动层不是线或层，请指定目标线层");
            }
        }
    
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                               m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
                m_PtTemp.Add(pt.Clone() as GeoPoint);
                if (m_PtTemp.Count == 3)
                {
                    Cancel();

                }
            }

            else if (e.Button == MouseButtons.Right)
            {
                base.Cancel();
                
            }
        }
        public override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            GeoPoint SnapingPt = m_MapUI.TransFromMapToWorld(e.Location);
            m_MapUI.MouseCatch(SnapingPt);
            int nCount = m_PtTemp.Count;
            Point[] pts = null;
            if (nCount > 0)
            {
                m_PtList.Clear();
                m_PtList.AddRange(m_PtTemp);
                m_PtList.Add(SnapingPt);
                if (nCount == 1)
                {
                    pts = m_MapUI.TransLineToMap(m_PtList);
                }
                else
                {
                    List<GeoPoint> ptlist = GIS.SpatialRelation.GeoAlgorithm.ThreePointsArc(m_PtList[0], m_PtList[1], m_PtList[2], m_Type);
                    if (ptlist == null)
                        return;
                    pts = m_MapUI.TransLineToMap(ptlist);
                }

                if (pts == null)
                {
                    return;
                }
                Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
                Graphics g = Graphics.FromImage(imgTemp);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                m_MapUI.RePaint(g);
                Pen pen = new Pen(Color.MediumVioletRed, 1f);                //画笔颜色
                g.DrawLines(pen, pts);
                m_MapUI.Image.Dispose();
                m_MapUI.Image = imgTemp;
      
                pen.Dispose();
                g.Dispose();
                m_MapUI.BaseRefresh();
            }

            else
            {
                m_MapUI.RePaint();
            }
        }
   
    }
}
