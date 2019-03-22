using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Geometries;
using System.Runtime.InteropServices;
using GIS.SpatialRelation;
//using System.Threading;
//using System.Timers;
namespace GIS.TreeIndex.Tool
{

    public partial class MapGenerateLineTool : MapTool
    {

        public Point[] GetCurDrawPoints(System.Windows.Forms.MouseEventArgs e)
        {
            GeoPoint LastPoint = m_MapUI.TransFromMapToWorld(e.Location);
            Point[] pts = null;
            switch (m_CurEditType)
            {
                case EditType.Turn:
                case EditType.Extend:                
                case EditType.AddOnePoint: //画单点的线 坐标串 就是这样的
                    {
                        pts = new Point[m_PtList.Count + 1];
                        m_MapUI.TransLineToMap(m_PtList).CopyTo(pts, 0);
                        pts[m_PtList.Count] = e.Location;
                        break;
                    }
                case EditType.Cross:
                    pts = new Point[m_PtList.Count ];
                    m_MapUI.TransLineToMap(m_PtList).CopyTo(pts, 0);                  
                    break;
                case EditType.ThreePointArc:
                    {
                        if (m_PtTemp.Count == 2)
                        {
                            List<GeoPoint> ArcPts = SpatialRelation.GeoAlgorithm.ThreePointsArc(m_PtTemp[0], m_PtTemp[1], LastPoint, ArcType.Arc);
                            if (ArcPts == null)
                            {
                                break;
                            }
                            pts = new Point[m_PtList.Count + ArcPts.Count];
                            m_MapUI.TransLineToMap(m_PtList).CopyTo(pts, 0);
                            m_MapUI.TransLineToMap(ArcPts).CopyTo(pts, m_PtList.Count);
                        }
                        else if (m_PtTemp.Count == 1)
                        {
                            pts = new Point[m_PtList.Count + 1];
                            m_MapUI.TransLineToMap(m_PtList).CopyTo(pts, 0);
                            pts[m_PtList.Count] = e.Location;
                        }
                        break;
                    }
                case EditType.Vertical:
                    {
                        if (m_PtList.Count > 0)
                        {
                            pts = new Point[m_PtList.Count + 1];
                            GeoPoint pt1 = m_PtList[m_PtList.Count - 1];
                            GeoPoint extPt = GeoAlgorithm.ExtentLine(pt1, LastPoint, 10000);
                            m_MapUI.TransLineToMap(m_PtList).CopyTo(pts, 0);
                            pts[m_PtList.Count] = m_MapUI.TransFromWorldToMap(extPt);
                        }
                        break;
                    }

            }
            return pts;
        }
        // GeoPoint StartPtBegin ,GeoPoint StartPtEnd为线段的起始和终点，ENDPT为捕捉点,ANGLE为捕捉的角度值,tolerance为捕捉的误差
        public void VerticalAndHorizontalRender(Graphics g, GeoPoint StartPtBegin, GeoPoint StartPtEnd, GeoPoint EndPt, double Angle,double Tolerance)
        {
            double angleArc = GeoAlgorithm.CalcAngle(StartPtBegin, StartPtEnd, EndPt);//计算角度值 
                           
            if (Math.Abs(angleArc % Angle - Angle / 2) >= Angle / 2 - Tolerance) //误差为2度
            {
               
                double snapAngle = (int)((angleArc + 4) / Angle) * Angle;
                snapAngle *= Math.PI/180;
                GeoPoint tmpPt = GIS.SpatialRelation.GeoAlgorithm.PointRotate(StartPtBegin, StartPtEnd, snapAngle);//将起点绕基点旋转后的点

                double dis = Math.Sqrt(GIS.SpatialRelation.GeoAlgorithm.DistanceOfTwoPt(StartPtEnd, EndPt));//终点到基点的距离
                GeoPoint ptEnd = GIS.SpatialRelation.GeoAlgorithm.ExtentLine(StartPtEnd, tmpPt, dis);//实际捕捉到的点
                GeoPoint ptFar = GIS.SpatialRelation.GeoAlgorithm.ExtentLine(StartPtEnd, tmpPt, dis + 1000);
                Point ptEndless = m_MapUI.TransFromWorldToMap(ptFar);  //无穷远的点
                Point ptBegin = m_MapUI.TransFromWorldToMap(StartPtEnd); //射线的起始点
                m_MapUI.m_SnapPoint = ptEnd;//设置捕捉点

                Pen pen = new Pen(Color.DarkRed,0.5f);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawLine(pen, ptBegin, ptEndless);
                pen.Dispose();
 
            }
        }
 
        public void VerticalPtRender(Graphics g, GeoPoint SnapingPt)
        {
            if (m_CurEditType == EditType.Vertical) //如果是捕捉垂足，则画出垂足
            {
                GeoPoint tempPt = CalVerticalPt(SnapingPt);
                if (tempPt != null)
                {
                    Pen pen = new Pen(Color.MediumVioletRed, 1f);                //画笔颜色  
                    Point ppt = m_MapUI.TransFromWorldToMap(tempPt);
                    g.DrawRectangle(pen, new Rectangle(ppt.X - m_MapUI.SnapPixels, ppt.Y - m_MapUI.SnapPixels, 2 * m_MapUI.SnapPixels, 2 * m_MapUI.SnapPixels));
                    pen.Dispose();
                }
            }
        }
        public virtual void NormalLineRender(Graphics g, MouseEventArgs e)
        {
            Point[] pts = GetCurDrawPoints(e);

            if (pts == null)
                return;
            Pen pen = new Pen(Color.MediumVioletRed, 1f);                //画笔颜色  
            g.DrawLines(pen, pts);
            pen.Dispose();
        }

        public override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            GeoPoint SnapingPt = m_MapUI.TransFromMapToWorld(e.Location);
            if (m_CurEditType != EditType.Vertical
                //&& m_CurEditType != EditType.Turn
                                                  )//如果是垂涎命令或者拐线命令，则不捕捉端点和中点
            {
                m_MapUI.MouseCatch(SnapingPt);
            }
            else
                m_MapUI.m_SnapPoint = null;
            int nCount = m_PtList.Count;
            if (nCount > 0)
            {
                try
                {
                    Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
                    Graphics g = Graphics.FromImage(imgTemp);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    m_MapUI.RePaint(g);

                    NormalLineRender(g, e);

                    if (m_PtList.Count > 1 && m_CurEditType != EditType.Vertical && m_MapUI.LineAngleSnapEnable)
                    {
                        VerticalAndHorizontalRender(g, m_PtList[nCount - 2], m_PtList[nCount - 1], SnapingPt, m_MapUI.SnapAngle, m_MapUI.AngleToler); // 在移动时 根据角度 捕捉 相应的点如果是垂涎命令，则不捕捉端点和中点

                    }

                    VerticalPtRender(g, SnapingPt);
                    TurnLineRender(g, SnapingPt);

                    m_MapUI.Image.Dispose();
                    m_MapUI.Image = imgTemp;
                    g.Dispose();
                    m_MapUI.BaseRefresh();
                }
                catch
                {
                }
            }
            else
            {
                m_MapUI.RePaint();
            }

          
        }



    }
}
