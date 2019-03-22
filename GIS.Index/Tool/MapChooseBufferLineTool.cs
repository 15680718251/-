using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GIS.TreeIndex;
using GIS.Buffer;
using GIS.Geometries;
using GIS.Map;
using GIS.Layer;
using GIS.TreeIndex.Forms;
using GIS.GeoData;

namespace GIS.TreeIndex.Tool
{
    public class MapChooseBufferTool : MapBufferTool
    {
        public MapChooseBufferTool(MapUI ui): base(ui)
        {
            m_MapUI.ClearAllSlt();
        }

        public override void OnMouseDown(object sender, MouseEventArgs e)
        {
            GeoLayer gl = m_MapUI.GetActiveVectorLayer();
            if (gl == null)
            {
                MessageBox.Show("当前没有活动图层", "提示");
                return;
            }
            if (gl.LayerTypeDetail == LAYERTYPE_DETAIL.PointLayer || gl.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer
                || gl.LayerTypeDetail == LAYERTYPE_DETAIL.PolygonLayer)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    GeoDataRow row = m_MapUI.SelectByPt(e.Location);
                    m_MapUI.Refresh();
                    m_MapUI.OutPutTextInfo(string.Format("提示：你已选了{0}个几何目标,右键结束选择!\r\n", m_MapUI.SltGeoSet.Count));
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    if (m_MapUI.SltGeoSet.Count == 1)  //选择多边形边界做缓冲区线的情况)
                    {
                        m_MapUI.OutPutTextInfo("选择多边形边界做缓冲区线成功：\r\n");
                        if (m_MapUI.SltGeoSet.Count == 1)  //选择多边形边界做缓冲区线的情况
                        {
                            srcGeometry = m_MapUI.SltGeoSet[0].Geometry;
                            if (srcGeometry is GeoPolygon || srcGeometry is GeoMultiPolygon)
                            {
                                BufferRemove(); //缓冲区剔除
                            }
                            else
                            {
                                MessageBox.Show("选择对象不是多边形", "提示");
                                return;
                            }
                        }
                        base.Cancel();  //取消捕获鼠标消息
                        //m_MapUI.ClearAllSlt();
                    }
                    else
                    {
                        MessageBox.Show("选择数量多于一个", "提示");
                        m_MapUI.OutPutTextInfo("选择多边形边界做缓冲区线失败！：\r\n");
                        base.Cancel();  //取消捕获鼠标消息
                    }
                }
            }
            else
            {
                MessageBox.Show("当前活动图层不符合", "提示");
                return;
            }
        }
    }
}