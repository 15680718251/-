using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.Geometries;
using GIS.Map;
using GIS.Layer;
using GIS.TreeIndex.OprtRollBack;
namespace GIS.TreeIndex.Tool
{
    public class MapAddGeometryTool : MapGenerateLineTool
    {
        public MapAddGeometryTool()
        {
        }
        public MapAddGeometryTool(MapUI ui) :base(ui)
        {
           
        }
        protected GeoData.GeoDataRow m_Geom = null;                //需要生成的线
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                base.Cancel();
                return;
            }
            else
            {
                base.OnMouseDown(sender, e);
            }
        } 
        public override void Finish()
        {
            if (m_Geom != null)
            {
                GeoVectorLayer actLayer = m_MapUI.GetActiveVectorLayer() as GeoVectorLayer;
                if (actLayer != null)
                {
                    m_MapUI.InitialNewGeoFeature(m_Geom);
                    m_Geom.EditState = EditState.Appear;

                    #region MyRegion
                    Operand oprt = new Operand(m_Geom, EditState.Invalid, EditState.Appear);
                    OperandList oprts = new OperandList();
                    oprts.m_NewOperands.Add(oprt);
                    m_MapUI.m_OprtManager.AddOprt(oprts);
                    #endregion

                    m_MapUI.BoundingBoxChangedBy(m_Geom);//重新计算边界矩形
                    m_MapUI.Refresh();
                    m_MapUI.EagleMapRefresh(true);
                }
            }
            else
            {
                m_MapUI.OutPutTextInfo("提示：当前几何编辑功能尚未结束，请切换到编辑功能，点击右键保存编辑结果，否则结果不保存！\r\n");
            }
        }
    }
}
