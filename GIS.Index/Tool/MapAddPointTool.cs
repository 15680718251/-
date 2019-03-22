using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Geometries;
using GIS.Layer;
namespace GIS.TreeIndex.Tool
{
    public class MapAddPointTool : MapAddGeometryTool
    {
        protected MapAddPointTool() { }

        public MapAddPointTool(MapUI ui): base(ui)
        {
            m_MapUI.OutPutTextInfo("画点工具激活：指定坐标 \r\n");
        }

        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        { 
            if (e.Button == MouseButtons.Left)
            {
                GeoPoint pt = (m_MapUI.m_SnapPoint == null) ?
                m_MapUI.TransFromMapToWorld(e.Location) : m_MapUI.m_SnapPoint;
               

                GeoVectorLayer actLayer = m_MapUI.GetActiveVectorLayer() as GeoVectorLayer;
                if (actLayer != null &&(actLayer.VectorType == VectorLayerType.PointLayer
                    || actLayer.VectorType == VectorLayerType.MixLayer || actLayer.VectorType == VectorLayerType.DraftLayer))
                {
                    m_Geom = actLayer.AddGeometry(new GeoPoint(pt.X, pt.Y));
                    m_Geom.Geometry.Bound = m_Geom.Geometry.GetBoundingBox();
                    m_MapUI.OutPutTextInfo("画点完成！\r\n");
                    m_MapUI.m_EditToolBack = m_MapUI.MapTool;//画点完成 
                    m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);
                }
                else
                {
                    MessageBox.Show("当前活动层不是点层，请指定目标点层");
                }
            }             
        }
        public override void initial()
        {
            base.initial();
            m_MapUI.OutPutTextInfo("画点工具激活：指定坐标 \r\n");
        }
    }
}
