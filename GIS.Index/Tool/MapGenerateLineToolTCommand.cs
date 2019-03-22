using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Geometries;
using System.Runtime.InteropServices;
using GIS.SpatialRelation;
using GIS.Toplogical;
using GIS.Layer;
using GIS.Map;

namespace GIS.TreeIndex.Tool
{

    public partial class MapGenerateLineTool : MapTool
    {
        private List<double> m_TDistList = new List<double>();
        public GeoPoint[] GetTurnRoundPts(GeoPoint pt)
        {
            if (m_TDistList.Count != 2)
                return null;

            double dist1 = m_TDistList[0];
            double dist2 = m_TDistList[1];
            GeoPoint[] resultPts = new GeoPoint[2];
            GeoPoint StartPt = m_PtList[m_PtList.Count - 2];
            GeoPoint CenterPt = m_PtList[m_PtList.Count - 1];
            double angle = GeoAlgorithm.CalcAngle(StartPt, CenterPt, pt);
            if (angle > 180)
                dist1 *= -1;
            GeoPoint ptFirst = GeoAlgorithm.CalcuLineLCommand(StartPt, CenterPt, dist1);
            resultPts[0] = ptFirst;

            angle = GeoAlgorithm.CalcAngle(CenterPt, ptFirst, pt);
            if (angle > 180)
                dist2 *= -1;
            GeoPoint ptSecond = GeoAlgorithm.CalcuLineLCommand(CenterPt, ptFirst, dist2);
            resultPts[1] = ptSecond;
            return resultPts;
        }
        public void GenerateTurnCommand(GeoPoint pt)
        {
            GeoPoint[] pts = null;
            if (m_TDistList.Count == 0)
            {
                GeoPoint pt1 = m_PtList[m_PtList.Count - 2];
                GeoPoint pt2 = m_PtList[m_PtList.Count - 1];
                GeoPoint ptL = GeoAlgorithm.CalcuLineTCommand(pt1, pt2, pt);
                if (ptL == null)
                    return;
                double len1 = pt2.DistanceTo(ptL);
                double len2 = ptL.DistanceTo(pt);
                m_MapUI.OutPutTextInfo(string.Format("确定横向距离{0},纵向距离{1} ！\r\n", len1, len2));
                m_TDistList.Add(len1);
                m_TDistList.Add(len2);
                pts = new GeoPoint[2];
                pts[0] = ptL; 
                pts[1] = pt;

            }
            else
            {
                 pts = GetTurnRoundPts(pt);
                if (pts == null)
                    return; 
            }
            for (int i = 0; i < pts.Length; i++)
            {
                m_PtList.Add(pts[i]);
            }    
            RecDataRollBack(m_PtList.Count - 2, 2);
        }
        public void TurnLineRender(Graphics g, GeoPoint pt)
        {
            if (m_TDistList.Count != 2)
                return;

            GeoPoint[] ots = GetTurnRoundPts(pt);
            Point pt0 = m_MapUI.TransFromWorldToMap(m_PtList[m_PtList.Count - 1]);
            Point pt1 = m_MapUI.TransFromWorldToMap(ots[0]);
            Point pt2 = m_MapUI.TransFromWorldToMap(ots[1]);
            Pen pen = new Pen(Color.DarkBlue);
            Point[] pts = new Point[3];
            pts[0] = pt0;
            pts[1] = pt1;
            pts[2] = pt2;
            g.DrawLines(pen, pts);

        }
    }
}
