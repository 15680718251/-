using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
namespace GIS.TreeIndex.Tool
{
    public class MapMesureLengthTool:MapAddGeometryTool
    {
        public MapMesureLengthTool(MapUI ui)
            : base(ui)
        {
            m_MapUI.OutPutTextInfo("提示： 长度量算工具， 左键量算，右键退出。\r\n");
        }
 
        public override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            bool LineAngleSnapEnbalbe = m_MapUI.LineAngleSnapEnable;
            m_MapUI.LineAngleSnapEnable = false;
            base.OnMouseMove(sender, e);
            m_MapUI.LineAngleSnapEnable = LineAngleSnapEnbalbe;
            if (m_PtList.Count > 0)
            {
                GeoPoint SnapingPt = (m_MapUI.m_SnapPoint == null) ?
                    m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
                List<GeoPoint> pts = new List<GeoPoint>(m_PtList);
                pts.Add(SnapingPt);
                GeoLineString line = new GeoLineString(pts);
                string len = string.Format("当前长度为：    {0}米\r\n", line.Length);
                m_MapUI.OutPutTextInfo(len);
            }
        }
        public override void Finish()
        { 
            m_MapUI.RePaint();
            m_MapUI.OutPutTextInfo("提示：  退出长度量算工具\r\n");
        }
        public override void initial()
        {
            base.initial();

            m_MapUI.OutPutTextInfo("提示： 长度量算工具， 左键量算，右键退出。\r\n");
        }
    }
}
