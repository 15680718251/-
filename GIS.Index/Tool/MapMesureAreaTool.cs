using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Geometries;
namespace GIS.TreeIndex.Tool
{
    public class MapMesureAreaTool:MapAddPolygonTool
    {
        public MapMesureAreaTool(MapUI ui)
            : base(ui)
        {
            m_MapUI.OutPutTextInfo("提示： 面积量算工具， 左键量算，右键退出。\r\n");
        }

        public override void Finish()
        { 
            m_MapUI.RePaint();

            m_MapUI.OutPutTextInfo("提示：  退出面积量算工具\r\n");
        }

        public override void initial()
        {
            base.initial();

            m_MapUI.OutPutTextInfo("提示：面积量算工具， 左键量算，右键退出。\r\n");
        }
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            bool LineAngleSnapEnbalbe= m_MapUI.LineAngleSnapEnable;
            m_MapUI.LineAngleSnapEnable = false;
            base.OnMouseMove(sender, e);
            m_MapUI.LineAngleSnapEnable = LineAngleSnapEnbalbe;
            if (m_PtList.Count > 1)
            {
                GeoPoint SnapingPt = (m_MapUI.m_SnapPoint == null) ?
                       m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
               
                List<GeoPoint> pts = new List<GeoPoint>(m_PtList);
                pts.Add(SnapingPt);
                GeoLinearRing ring = new GeoLinearRing(pts);
                string area = string.Format("当前面积为：    {0}米\r\n",ring.Area );
                m_MapUI.OutPutTextInfo(area);
            }
        }
      
    }
}
