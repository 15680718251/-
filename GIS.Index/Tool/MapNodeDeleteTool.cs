using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.GeoData;
using System.Windows.Forms;
using System.Drawing;
using GIS.Geometries;
using GIS.Layer;
namespace GIS.TreeIndex.Tool
{
    public class MapNodeDeleteTool:MapTool
    {
        public MapNodeDeleteTool(MapUI ui)
            : base(ui)
        {
            initial();
        }
        private void OnLeftButtonDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
            GeoDataRow RowCatched = null;
            int sltCount = m_MapUI.SltGeoSet.Count;
            if (sltCount == 0)
            {
                if (m_MapUI.SelectByPt(e.Location, SelectType.Geomtry) != null)
                {
                    m_MapUI.Refresh();
                }
                return;
            }
            else
            {
                RowCatched = m_MapUI.MouseCatchInSltGeoSet(pt, MouseCatchType.Vertex);
            }
            if (RowCatched == null)
            {
                if (m_MapUI.SelectByPt(e.Location, SelectType.Geomtry) != null)
                {
                    m_MapUI.Refresh();
                }

                else
                {
                    m_MapUI.OutPutTextInfo(" 没有选中节点，请重新选择\r\n");
                }
                return;
            }
            else
            {
                GeoData.GeoDataRow rowNew = RowCatched.Clone();
                if (rowNew.Geometry.RemoveVertex(m_MapUI.m_SnapPoint))
                {
                    EditState state = RowCatched.EditState;
                    ((GeoDataTable)RowCatched.Table).AddRow(rowNew); //保存的数据不添加到图层中，由增量管理器管理
                    if (RowCatched.EditState == EditState.Original)
                    {
                        RowCatched.EditState = EditState.GeometryBef;
                        rowNew.EditState = EditState.GeometryAft;
                    }
                    else
                    {
                       RowCatched.EditState = EditState.Invalid;
                       rowNew.EditState = state;
                    }
                    ///////////////操作回退
                    GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                    m_MapUI.m_OprtManager.AddOprt(oprts);

                    GIS.TreeIndex.OprtRollBack.Operand oprtNew = new GIS.TreeIndex.OprtRollBack.Operand(rowNew, EditState.Invalid, rowNew.EditState);
                    oprts.m_NewOperands.Add(oprtNew);

                    GIS.TreeIndex.OprtRollBack.Operand oprtOld = new GIS.TreeIndex.OprtRollBack.Operand(RowCatched, state, RowCatched.EditState);
                    oprts.m_OldOperands.Add(oprtOld);
                    ///////////////操作回退

                    m_MapUI.OutPutTextInfo("提示：删除节点成功！\r\n");
                    m_MapUI.Refresh();
                }
                else
                {
                    m_MapUI.OutPutTextInfo("提示：删除节点失败，请确认节点后再删除！\r\n");
                }
            }   
        }

        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                OnLeftButtonDown(sender,e);
            else
                Cancel();
        }
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
          GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
          GeoDataRow  RowCatched = m_MapUI.MouseCatchInSltGeoSet(pt, MouseCatchType.Vertex);
          if (RowCatched != null)
          {
              Image imgTemp = new Bitmap(m_MapUI.Width, m_MapUI.Height);
              Graphics g = Graphics.FromImage(imgTemp);
              m_MapUI.RePaint(g);
              
              m_MapUI.Image.Dispose();
              m_MapUI.Image = imgTemp;
              g.Dispose();
              m_MapUI.BaseRefresh();   
          }
        }
        public override void initial()
        {
            m_Cursor = Cursors.Cross;
            m_MapUI.OutPutTextInfo("节点删除工具激活：请选中目标节点，左键删除\r\n");
         }

        public override void Finish()
        {
            m_MapUI.OutPutTextInfo("提示：删除节点工具结束！\r\n");
        }

    }
}
