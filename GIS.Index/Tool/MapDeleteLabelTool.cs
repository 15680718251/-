using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.Tool
{
    public class MapDeleteLabelTool : MapTool
    {
        protected MapDeleteLabelTool()
        {
        }
        public MapDeleteLabelTool(MapUI ui)
            : base(ui)
        {
             m_Cursor = Cursors.Cross;
             initial();
        }
        public override void initial()
        {
            if (m_MapUI.SltLabelSet.Count > 0)//如果已经有选中目标则直接删除
            {
                m_MapUI.DeleteSltObjSet(DeleteType.Label);
                m_MapUI.Refresh(); 
                m_MapUI.OutPutTextInfo(string.Format("提示： 已经删除{0}个目标，右键结束，左键继续选择删除目标！\r\n", m_MapUI.SltGeoSet.Count));
            }
            else
                m_MapUI.OutPutTextInfo("提示：删除工具激活：点击左键选中目标，点击右键删除\r\n");
        }
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)   //如果是左键 就选择目标
            {
                if (m_MapUI.SelectByPt(e.Location, SelectType.Label) != null)
                {
                    m_MapUI.Refresh();                   
                }
            }
            else if( e.Button == MouseButtons.Right)        //如果是右键则删除对象
            {
                if (m_MapUI.DeleteSltObjSet(DeleteType.Label))              //如果有删除对象，则刷新图片
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
