using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GIS.Geometries;
namespace GIS.TreeIndex.Tool
{
    public class MapRegionSelectTool:MapTool
    {
        public enum SelectType
        {
            RectSelect,
            RegionSelect
        }
        public MapRegionSelectTool(MapUI ui, SelectType type)
            : base(ui)
        {
            m_MapUI.OutPutTextInfo("提示：现在开始进行区域选择功能\r\n");
            m_SelectType = type;
        }
        private SelectType m_SelectType;
        private List<GeoPoint> m_List= new  List<GeoPoint>();
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
                m_List.Add(pt);
                if (m_SelectType == SelectType.RectSelect)
                {
                    if (m_List.Count == 2)
                    {
                        double xMin = Math.Min(m_List[0].X, m_List[1].X);
                        double yMin = Math.Min(m_List[0].Y, m_List[1].Y);
                        double xMax = Math.Max(m_List[0].X, m_List[1].X);
                        double yMax = Math.Max(m_List[0].Y, m_List[1].Y);
                        m_List.Clear();
                        List<GeoPoint> ptList = new List<GeoPoint>();
                        ptList.Add(new GeoPoint(xMin, yMin));
                        ptList.Add(new GeoPoint(xMax, yMin));
                        ptList.Add(new GeoPoint(xMax, yMax));
                        ptList.Add(new GeoPoint(xMin, yMax));
                        GeoLinearRing ring = new GeoLinearRing(ptList);
                        List<GeoData.GeoDataRow> rows = m_MapUI.SelectByRegion(new GeoPolygon(ring));
                        if (rows.Count > 0)
                            m_MapUI.Refresh();

                    }

                }
                else if (m_SelectType == SelectType.RegionSelect)
                {
                    if (m_List.Count > 3)
                    {
                        if (SpatialRelation.GeoAlgorithm.IsOnPointCoarse(m_List[0], m_List[m_List.Count - 1]))
                        {
                            GeoLinearRing ring = new GeoLinearRing(m_List);
                            List<GeoData.GeoDataRow> rows = m_MapUI.SelectByRegion(new GeoPolygon(ring));
                            m_List.Clear();
                            if (rows.Count > 0)
                            {
                                m_MapUI.Refresh();
                            }
                        }
                    }
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                m_MapUI.OutPutTextInfo("提示：多选功能结束！\r\n");
                base.Cancel();
            }

        }
        public override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location); 
            if (m_SelectType == SelectType.RectSelect)
            {
                if (m_List.Count == 1)
                {
                    Point ptFirst =m_MapUI.TransFromWorldToMap(m_List[0]);
                    Image _imgTemp = new Bitmap(m_MapUI.Size.Width, m_MapUI.Size.Height);
                    Graphics g = Graphics.FromImage(_imgTemp);
                    g.Clear(Color.Transparent);
                    m_MapUI.RePaint(g);
                    Pen pen = new Pen(Color.RoyalBlue, 1);
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                    int xMin = Math.Min(e.X, ptFirst.X);
                    int yMin = Math.Min(e.Y, ptFirst.Y);
                    g.DrawRectangle(pen, xMin, yMin, Math.Abs(e.X - ptFirst.X), Math.Abs(e.Y - ptFirst.Y));
                    g.Dispose();
                    pen.Dispose();
                    m_MapUI.Image = _imgTemp;
                    m_MapUI.BaseRefresh();
                }
            }
            else if (m_SelectType == SelectType.RegionSelect)
            {
                if (m_List.Count >= 1)
                {
                    List<GeoPoint> pts = new List<GeoPoint>();
                    pts.AddRange(m_List);
                    pts.Add(pt);
                    Point[] drawPts = m_MapUI.TransLineToMap(pts);

                    Image _imgTemp = new Bitmap(m_MapUI.Size.Width, m_MapUI.Size.Height);
                    Graphics g = Graphics.FromImage(_imgTemp);
                    g.Clear(Color.Transparent);
                    m_MapUI.RePaint(g);
                    Pen pen = new Pen(Color.RoyalBlue, 1);
                    
                    if (SpatialRelation.GeoAlgorithm.IsOnPointCoarse(pt, m_List[0]))
                    {
                        Point ptStart = m_MapUI.TransFromWorldToMap(m_List[0]);
                        g.DrawEllipse(pen, new Rectangle(ptStart.X - m_MapUI.SnapPixels, ptStart.Y - m_MapUI.SnapPixels, 2 * m_MapUI.SnapPixels, 2 * m_MapUI.SnapPixels));

                    } 
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                    g.DrawLines(pen, drawPts);
                    g.Dispose();
                    pen.Dispose();
                    m_MapUI.Image = _imgTemp;
                    m_MapUI.BaseRefresh();
                }
            }
        }
    }
}
