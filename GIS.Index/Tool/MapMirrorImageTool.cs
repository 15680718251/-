using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GIS.Layer;
using GIS.Geometries;
using GIS.Map;
using GIS.SpatialRelation;


namespace GIS.TreeIndex.Tool
{
    public class MapMirrorImageTool:MapTool
    {
        public MapMirrorImageTool(MapUI ui)
            : base(ui)
        {           
            initial();
        }
        private enum MirrorState
        {
            State_SelectObject,//选择对象
            State_SelectBaseLine//选择基线
        }
        private MirrorState m_MirrorState = MirrorState.State_SelectObject;
      
        public override void Cancel()
        {
            if (m_MirrorState == MirrorState.State_SelectObject)
            {
                if (m_MapUI.SltGeoSet.Count > 0)
                {
                    m_MirrorState = MirrorState.State_SelectBaseLine;
                    
                    m_MapUI.OutPutTextInfo( "提示： 左键选择基线！\r\n" );
                }
            }
        }
        public override void Finish()
        {
            m_MapUI.OutPutTextInfo("提示：镜像工具结束！\r\n");
        }
        public override void initial()
        {
            m_MirrorState = MirrorState.State_SelectObject;
            m_MapUI.OutPutTextInfo("提示：镜像工具开始，请先选择目标！\r\n");
        }

        protected void SelectGeometries(  MouseEventArgs e)
        {
            if (m_MapUI.SelectByPt(e.Location, SelectType.Geomtry) != null)
            {
                m_MapUI.Refresh();
            }
            int sltCount = m_MapUI.SltGeoSet.Count;
            m_MapUI.OutPutTextInfo(string.Format("提示： 选中 {0} 个目标,点击右键选择基线！\r\n", sltCount));
            return;
        }

        private void MakeMirror(MouseEventArgs e)
        {
            GeoData.GeoDataRow row = m_MapUI.SelectByPt(e.Location, SelectType.Geomtry);           
            if(row==null) 
                return;
            if (row.Geometry is GeoLineString)
            {
                GeoPoint pt = m_MapUI.TransFromMapToWorld(e.Location);
                GeoPoint ptVertical;
                int ptIndex;
                GeoLineString line = GeoAlgorithm.VerticalPtOfPtToGeometry(pt, row.Geometry, out ptVertical, out ptIndex);
                GeoPoint ptStart = line.Vertices[ptIndex - 1];
                GeoPoint ptEnd = line.Vertices[ptIndex];
                m_MapUI.RemoveSltObj(row);
                ///////////////操作回退
                GIS.TreeIndex.OprtRollBack.OperandList oprts = new GIS.TreeIndex.OprtRollBack.OperandList();
                m_MapUI.m_OprtManager.AddOprt(oprts);
                ///////////////操作回退
                for (int i = 0; i < m_MapUI.SltGeoSet.Count; i++)
                {
                    GeoData.GeoDataRow rowEdit = m_MapUI.SltGeoSet[i];
                    GeoVectorLayer layer = m_MapUI.GetLayerByTable((GeoData.GeoDataTable)rowEdit.Table) as GeoVectorLayer;
                    Geometry geom = m_MapUI.SltGeoSet[i].Geometry.Clone();
                    geom.SymmetryWithLine(ptStart, ptEnd);
                    GeoData.GeoDataRow NewRow = layer.AddGeometry(geom);
                    m_MapUI.InitialNewGeoFeature(NewRow);
                    NewRow.EditState = EditState.Appear;
                    ///////////////操作回退
                    GIS.TreeIndex.OprtRollBack.Operand oprt = new GIS.TreeIndex.OprtRollBack.Operand(NewRow, EditState.Invalid, EditState.Appear);
                    oprts.m_NewOperands.Add(oprt);

                    ///////////////操作回退
                }             
                base.Cancel();
            }
            else
            {
                m_MapUI.OutPutTextInfo("提示：您选中的不是线目标，不能做镜像\r\n");
                m_MapUI.RemoveSltObj(row);                
            }
            m_MapUI.Refresh();
        }
        public override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_MirrorState ==  MirrorState.State_SelectObject)
                {
                    SelectGeometries(e);
                }
                else if (m_MirrorState == MirrorState.State_SelectBaseLine)
                {
                    MakeMirror(e);
                }                
            }
            else if (e.Button == MouseButtons.Right)
            {
                Cancel();
            }
        }
    }
}
