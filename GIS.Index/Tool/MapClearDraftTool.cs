using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.Tool
{
    public class MapClearDraftTool:MapTool
    {
        protected MapClearDraftTool()
        {
        }
        public MapClearDraftTool(MapUI ui)
            : base(ui)
        {
             m_Cursor = Cursors.Cross;
             initial();
        }
                
        private List<GeoData.GeoDataRow> m_EditRows = new List<GIS.GeoData.GeoDataRow>();

        public override void initial()
        {
            int nCount = m_MapUI.SltGeoSet.Count;

            if (nCount > 0)//如果已经有选中目标则直接删除
            {

                for (int i = 0; i < nCount; i++)
                {
                    m_EditRows.Add(m_MapUI.SltGeoSet[i]);
                }

                m_MapUI.DeleteSltObjSet(DeleteType.Geomtry);


                m_MapUI.Refresh();
                m_MapUI.OutPutTextInfo(string.Format("提示： 已经删除{0}个目标，右键结束，左键继续选择删除目标！\r\n", nCount));
            }
            else
                m_MapUI.OutPutTextInfo("提示：删除工具激活：点击左键选中目标，点击右键删除\r\n");
        }
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)   //如果是左键 就选择目标
            {
                if (m_MapUI.SelectByPt(e.Location, SelectType.Geomtry) != null)
                {
                    m_MapUI.Refresh();                   
                }
            }
            else if( e.Button == MouseButtons.Right)        //如果是右键则删除对象
            {
                if (m_MapUI.DeleteSltObjSet(DeleteType.Geomtry))              //如果有删除对象，则刷新图片
                {
                    m_MapUI.Refresh();
                    m_MapUI.OutPutTextInfo( "提示：删除1个目标！\r\n" );
                }
                else                                        //如果没有删除对象，则切换到上一个编辑工具
                {
                    m_MapUI.OutPutTextInfo("提示：删除工具退出\r\n");
                    base.Cancel();
                }
            }
        }
        public override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);
        }
        public override void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(sender, e);
        }
    }
}
