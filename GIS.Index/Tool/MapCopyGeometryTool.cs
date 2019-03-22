using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GIS.Geometries;
using GIS.Layer;
namespace GIS.TreeIndex.Tool
{
    public class MapCopyGeometryTool:MapMoveGeomtryTool
    {
        public MapCopyGeometryTool(MapUI ui)
            : base(ui)
        {
            m_List = new List<GIS.GeoData.GeoDataRow>();
           
        }
        private List<GeoData.GeoDataRow> m_List;
        protected  override void PositionSecond(object sender, MouseEventArgs e)
        {
            GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                          m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
            double deltaX = pt.X - m_PointOrign.X;
            double deltaY = pt.Y - m_PointOrign.Y;
            m_List.Clear();

            ///////////////操作回退
            GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();       
            m_MapUI.m_OprtManager.AddOprt(oprts);
            ///////////////操作回退

            for (int i = 0; i < m_MapUI.SltGeoSet.Count; i++)
            {
                GeoData.GeoDataRow row = m_MapUI.SltGeoSet[i];
                GeoLayer layer = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)row.Table);
                GeoVectorLayer vlyr = layer as GeoVectorLayer;
                if (vlyr != null)
                {
                    GeoData.GeoDataRow newRow = vlyr.AddGeometry(row.Geometry.Clone());
                    m_MapUI.InitialCopyGeoFeature(row, newRow);
                    newRow.EditState = EditState.Appear;
                    newRow.Geometry.Move(deltaX, deltaY);

                    ///////////////操作回退
                    GIS.TreeIndex.OprtRollBack.Operand oprt = new GIS.TreeIndex.OprtRollBack.Operand(newRow, EditState.Invalid, EditState.Appear);
                    oprts.m_NewOperands.Add(oprt);

                    ///////////////操作回退

                    m_List.Add(newRow);
                }               
            }
            m_MapUI.ClearAllSlt();
            m_MapUI.m_EditToolBack = m_MapUI.MapTool;
            m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);     
        }
        public override void Finish()
        {
            for (int i = 0; i <m_List.Count; i++)
            {
                GeoData.GeoDataRow row = m_List[i];
                m_MapUI.BoundingBoxChangedBy(row);
            }
            m_MapUI.OutPutTextInfo("提示：复制工具结束！\r\n");
        }

        public override void initial()
        {
            if (m_MapUI.SltGeoSet.Count == 0)
            {
                m_MoveState = MoveState.State_Select;
                m_MapUI.OutPutTextInfo("提示：复制工具开始，请先选择目标后，点左键选择复制基点开始平移！\r\n");
            }
            else
            {
                m_MoveState = MoveState.State_PositionFirst;
                m_MapUI.OutPutTextInfo("提示： 请选择平移基点开始复制！\r\n");
            }

        }
    }
}
