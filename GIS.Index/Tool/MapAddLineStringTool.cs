using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.Geometries;
using GIS.Map;
using GIS.Layer;
using GIS.TreeIndex.Forms;
namespace GIS.TreeIndex.Tool
{
    public class MapAddLineStringTool :MapAddGeometryTool
    {
        protected MapAddLineStringTool()
        {
        }
        public MapAddLineStringTool(MapUI ui) : base(ui)
        {
            m_MapUI.OutPutTextInfo("画线工具激活：指定起始点 \r\n");
        }
        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                base.OnMouseDown(sender, e);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Cancel(); 
            } 
        }

        public override void Cancel()
        {
            if (m_CurEditType == EditType.AddOnePoint)
            {
                GeoVectorLayer actLayer = m_MapUI.GetActiveVectorLayer() as GeoVectorLayer;
                if (actLayer != null &&
                    (actLayer.VectorType == VectorLayerType.LineLayer ||
                      actLayer.VectorType == VectorLayerType.MixLayer ||
                      actLayer.VectorType == VectorLayerType.DraftLayer))
                {
                    if (m_PtList.Count >= 2)
                    {
                        m_Geom = actLayer.AddGeometry(new GeoLineString(m_PtList));
                        m_MapUI.OutPutTextInfo("提示：  画线成功，退出线构建命令\r\n");
                        m_MapUI.m_EditToolBack = m_MapUI.MapTool;
                        m_MapUI.MapTool = new MapMoveNodeTool(m_MapUI);

                    }
                    else
                    {
                        m_MapUI.OutPutTextInfo("提示：  画线失败，退出画线命令\r\n");
                        m_MapUI.OutPutTextInfo("提示：  构线点数少于2个，无法完成线的绘制\r\n");
                    }
                }
                else
                {
                    MessageBox.Show("当前活动层不是线层，请指定目标线层");
                }
            }
            else
                base.Cancel();
        }
    
        public override void initial()
        {
            base.initial();
            m_MapUI.OutPutTextInfo("画线工具激活：指定起始点 \r\n");
        }
 
    }
}
