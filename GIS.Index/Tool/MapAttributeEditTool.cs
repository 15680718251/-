using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using GIS.GeoData;
using GIS.Geometries;
namespace GIS.TreeIndex.Tool
{
    public class MapAttributeEditTool:MapTool
    {
        public MapAttributeEditTool(MapUI ui)
            : base(ui)
        {
            initial();
        }
        private GeoDataRow m_EditingRow = null;
        public override void initial()
        {
            if (m_MapUI.SltGeoSet.Count == 1)
            {
                AttriEdit();
            }
            else
            { 
                m_MapUI.OutPutTextInfo("提示：属性查询修改工具激活，点击左键选中目标查看,右键结束！\r\n");
            }
        }
        private void AttriEdit()
        {
            if (m_EditingRow!= null)
            {
                TreeIndex.Forms.IncAttributeForm form = new GIS.TreeIndex.Forms.IncAttributeForm(m_EditingRow, m_MapUI);
                form.ShowDialog();

                m_MapUI.OutPutTextInfo("提示：左键选择属性修改目标,右键结束！\r\n");
 
            }
        }
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            { 
                GeoDataRow OrignRow = m_MapUI.SelectByPt(e.Location);
                if (OrignRow != null)
                {
                    m_EditingRow = OrignRow;
                    m_MapUI.AddSltObj(OrignRow);
                    m_MapUI.Refresh();
                    AttriEdit();
                }
                else 
                    m_EditingRow = null;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                m_MapUI.OutPutTextInfo("提示：属性修改工具结束！\r\n");
                base.Cancel();
            }
        }
         
    }
}
