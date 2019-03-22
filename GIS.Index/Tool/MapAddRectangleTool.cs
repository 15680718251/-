using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Text;
using GIS.Geometries;
using GIS.Layer;
using GIS.SpatialRelation;
using GIS.TreeIndex.OprtRollBack;
namespace GIS.TreeIndex.Tool
{
    public class MapAddRectangleTool:MapTool
    {
        public MapAddRectangleTool(MapUI ui)
            : base(ui)
        {
            m_Cursor = Cursors.Cross;          
            initial();
        }
        private List<GeoPoint> m_PtList= new List<GeoPoint>();


        public override void initial()
        {
            m_PtList.Clear();
            m_MapUI.OutPutTextInfo("提示：三点画矩形功能开始，请输入矩形的三个点。\r\n");
        }
        public override void Cancel()
        {
            m_PtList.Clear();
            base.Cancel();
            m_MapUI.OutPutTextInfo("提示：退出三点矩形功能。\r\n");
        }
        public override void Finish()
        {
            if (m_PtList.Count == 3)
            {

                List<GeoPoint> rectList =GeoAlgorithm.ThreePointRect(m_PtList);
                if (rectList != null)
                {
                    m_MapUI.OutPutTextInfo("提示:矩形绘制成功，退出三点矩形功能！\r\n");
                    GeoLinearRing ring = new GeoLinearRing(rectList);
                    GeoVectorLayer layer = m_MapUI.GetActiveVectorLayer() as GeoVectorLayer;
                    if (layer != null)
                    {
                        GeoData.GeoDataRow row = layer.AddGeometry(ring);
                        m_MapUI.InitialNewGeoFeature(row);
                        row.EditState = EditState.Appear;

                        #region MyRegion
                        Operand oprt = new Operand(row, EditState.Invalid, EditState.Appear);
                        OperandList oprts = new OperandList();
                        oprts.m_NewOperands.Add(oprt);
                        m_MapUI.m_OprtManager.AddOprt(oprts);
                        #endregion

                        m_MapUI.BoundingBoxChangedBy(row);
                        m_MapUI.Refresh();


                    }
                }

                
            }
        }
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
             GeoPoint SnapingPt = m_MapUI.TransFromMapToWorld(e.Location);
             m_MapUI.MouseCatch(SnapingPt);
            if (m_PtList.Count > 0)
            {
                List<GeoPoint> m_ptTemp = new List<GeoPoint>();
                m_ptTemp.AddRange(m_PtList);
                m_ptTemp.Add(SnapingPt);
                Point[] pts = m_MapUI.TransLineToMap(m_ptTemp);
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
        }
        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GeoPoint pt = (m_MapUI.m_SnapPoint != null) ?
                    m_MapUI.m_SnapPoint : m_MapUI.TransFromMapToWorld(e.Location);
                if (m_PtList.Count < 2)
                {
                    m_PtList.Add(pt);
                }
                else if (m_PtList.Count == 2)
                {
                    m_PtList.Add(pt);
                    base.Cancel();
                    m_PtList.Clear();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                Cancel();
            }
        }
        
    }
}
