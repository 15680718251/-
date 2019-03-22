using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Geometries;
using GIS.Layer;
namespace GIS.TreeIndex.Tool
{
    public class MapAddSplineTool:MapAddGeometryTool
    {
        public MapAddSplineTool(MapUI ui)
            : base(ui)
        {
            m_Cursor = Cursors.Cross;
            m_PtList = new List<GeoPoint>();
            m_PtTemp = new List<GeoPoint>(); 
            m_MapUI.OutPutTextInfo("样条曲线工具激活：请添加起始点, 按C闭合,U撤销\r\n");
        }
        protected new  List<GeoPoint> m_PtList;      //最终点
        protected new List<GeoPoint> m_PtTemp;      //临时骨架点

        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                               m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
                m_PtTemp.Add(pt.Clone() as GeoPoint);
            }

            else if (e.Button == MouseButtons.Right)
            {
                Cancel(); 
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
                    GeoSpline line = new GeoSpline(m_PtTemp);
                    m_Geom = actLayer.AddGeometry(line);
                    m_MapUI.OutPutTextInfo("提示：  画样条曲线成功，退出线构建命令\r\n");
                    //base.Finish();
                    m_MapUI.m_EditToolBack = m_MapUI.MapTool;
                    m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
                }
                else
                {
                    m_MapUI.OutPutTextInfo("提示：  画线失败，退出画线命令\r\n");
                    m_MapUI.OutPutTextInfo("提示：  构线点数少于3个，无法完成样条曲线的绘制\r\n");
                }
            }
            else
            {
                MessageBox.Show("当前活动层不是线层，请指定目标线层");
            }
        }

        public override void OnMouseMove(object sender, MouseEventArgs e)
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
                    List<GeoPoint> ptlist = GIS.SpatialRelation.GeoAlgorithm.CubicSpline(m_PtList);
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
        public override void OnKeyDown(KeyEventArgs e)
        {
             if (e.KeyCode == Keys.C)        //闭合命令
            {
                 int ptCounts = m_PtTemp.Count;
                 if (ptCounts < 3)
                {
                    m_MapUI.OutPutTextInfo("样条曲线点数少于3个，无法闭合：\r\n");
                }
                else
                {
                    if (!m_PtTemp[0].IsEqual(m_PtTemp[ptCounts - 1]))
                    {
                        m_PtTemp.Add((GeoPoint)m_PtTemp[0].Clone());
                    }
                    Cancel();             
                    m_MapUI.OutPutTextInfo("样条曲线闭合，画线工具结束：\r\n");
                    m_MapUI.Refresh();
                    m_MapUI.m_EditToolBack = m_MapUI.MapTool;
                    m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
                }
            }
             else if (e.KeyCode == Keys.U) //撤销
             {
                 if(m_PtTemp.Count>0)
                     m_PtTemp.RemoveAt(m_PtTemp.Count - 1);
             }
        }
     
        public override void initial()
        {
            m_Cursor = Cursors.Cross;
            m_PtTemp.Clear();
            m_MapUI.OutPutTextInfo("样条曲线工具激活：请添加起始点, 按C闭合,U撤销\r\n");
        }

       
    }
}
