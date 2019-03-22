using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GIS.Geometries;
using GIS.GeoData;
namespace GIS.TreeIndex.Tool
{
    public class MapPointSelectTool:MapTool
    {
        public MapPointSelectTool(MapUI ui): base(ui)
        {
            m_MapUI.OutPutTextInfo("提示：现在开始进行点择功能\r\n");
      
        }
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                GeoDataRow row = m_MapUI.SelectByPt(e.Location);
                m_MapUI.Refresh();
                m_MapUI.OutPutTextInfo(string.Format("提示：已经选中{0}个几何目标,{1}个文本注记!\r\n", m_MapUI.SltGeoSet.Count,m_MapUI.SltLabelSet.Count));
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                m_MapUI.OutPutTextInfo("提示：点选功能结束！\r\n");
                base.Cancel();
            }
        }
    }
}
