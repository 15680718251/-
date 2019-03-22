using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GIS.Layer;
using GIS.Geometries;
using GIS.Map;
using GIS.SpatialRelation;
using GIS.TreeIndex.OprtRollBack;

namespace GIS.TreeIndex.Tool
{
    public class MapLineParallelTool:MapTool
    {
        public MapLineParallelTool(MapUI ui)
            : base(ui)
        {
            initial();
        }
        
        public override void initial()
        {
            if (m_MapUI.SltGeoSet.Count == 1 && m_MapUI.SltGeoSet[0].Geometry is GeoLineString)
            {
                m_MapUI.OutPutTextInfo("提示：画平行线功能开始，已选择目标线体，请指定定位点！\r\n");
            }
            else
            {
                m_MapUI.ClearAllSlt();
                m_MapUI.OutPutTextInfo("提示：画平行线功能开始，请选择目标线体！\r\n");
            }
        }
       
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (m_MapUI.SltGeoSet.Count == 1 && m_MapUI.SltGeoSet[0].Geometry is GeoLineString)
            {
                GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
                m_MapUI.MouseCatch(pt);
                m_MapUI.RePaint(); 
            }
        }
        public override void Finish()
        {
            m_MapUI.OutPutTextInfo("提示：平行线工具结束！\r\n");
        }
        private void GenerateParallelLine(GeoPoint pt)
        {
            GeoData.GeoDataRow row = m_MapUI.SltGeoSet[0];
            GeoLineString line = row.Geometry as GeoLineString;
            line.ClearRepeatPoints();
            GeoPoint ptVertical;
            int index;
            if (!GeoAlgorithm.VerticalPtofPtToLineString(pt, line, out ptVertical, out index))
                return;
 
            double xx1 = line.Vertices[index - 1].X;
            double yy1 = line.Vertices[index - 1].Y;
            double xx2 = line.Vertices[index].X;
            double yy2 = line.Vertices[index].Y;

            double a1, b1, c1;
            a1 = yy1 - yy2;
            b1 = xx2 - xx1;
            c1 = yy2 * xx1 - yy1 * xx2;

            double Dist = -(a1 * pt.X + b1 * pt.Y + c1) / Math.Sqrt(a1 * a1 + b1 * b1);
 

            List<GeoPoint> pts = GeoAlgorithm.CalcuParallel(line.Vertices, Dist);
            GeoPoint ptStart = GeoAlgorithm.CalcuLineLCommand(line.Vertices[1], line.Vertices[0], -Dist);

            pts.Insert(0, ptStart);
            GeoPoint ptEnd = GeoAlgorithm.CalcuLineLCommand(line.Vertices[line.NumPoints - 2], line.Vertices[line.NumPoints - 1], Dist);
            pts.Add(ptEnd);

            GeoVectorLayer lyr = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)row.Table) as GeoVectorLayer;

           GeoData.GeoDataRow newRow =  lyr.AddGeometry(new GeoLineString(pts));
           newRow.EditState = EditState.Appear;
            m_MapUI.BoundingBoxChangedBy(newRow);

            #region MyRegion
            Operand oprt = new Operand(newRow, EditState.Invalid, EditState.Appear);
            OperandList oprts = new OperandList();
            oprts.m_NewOperands.Add(oprt);
            m_MapUI.m_OprtManager.AddOprt(oprts);
            #endregion

            m_MapUI.Refresh();

        }
        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GeoPoint pt = (m_MapUI.m_SnapPoint != null) ?
                                m_MapUI.m_SnapPoint : m_MapUI.TransFromMapToWorld(e.Location);

                if (m_MapUI.SltGeoSet.Count == 1 && m_MapUI.SltGeoSet[0].Geometry is GeoLineString)
                {
                    GenerateParallelLine(pt);
                    base.Cancel();
                }
                else
                {
                    if (m_MapUI.SelectByPt(e.Location) != null)
                        m_MapUI.Refresh();
                    initial();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (m_MapUI.SltGeoSet.Count == 0)
                    base.Cancel();
            }
        }
    }
}
