using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using GIS.Layer;

namespace GIS.Map
{
    public partial class GeoMap : ILayerControl, ILayerGroupControl
    {
        internal static System.Globalization.NumberFormatInfo numberFormat_zhCN = new System.Globalization.CultureInfo("zh-CN", false).NumberFormat;
        public GeoMap()
        {
            m_LayerGroups = new List<LayerGroup>();
            m_Layers = new List<GeoLayer>();
            m_aSltObjSet = new List<GIS.GeoData.GeoDataRow>();
            m_aSltLableSet = new List<GIS.GeoData.GeoDataRow>();
            initial();
        }

        public void initial()
        {
            LayerGroup group = new LayerGroup("系统工作区");
            m_LayerGroups.Add(group);
       
            GeoVectorLayer layer = new GeoVectorLayer( );
            layer.LayerName = "测点层";
          
            layer.DataTable = new GIS.GeoData.GeoDataTable();
            GeoData.GeoDataTable table = layer.DataTable;

            layer.VectorType = VectorLayerType.SurveyLayer;

            table.FillData = true; ///空图层，属性信息已经填充，因为是空的
            table.Columns.Add("FID", typeof(int));
            table.Columns.Add("FeatID", typeof(string));
            table.Columns.Add("ClasID", typeof(Int64));
            table.Columns.Add("BeginTime", typeof(string));
            table.Columns.Add("ChangeType", typeof(string));
            group.Layers.Add(layer);
            m_Layers.Add(layer);

            layer = new GeoVectorLayer();
            layer.LayerName = "草图层";
            layer.DataTable = new GIS.GeoData.GeoDataTable();
            table = layer.DataTable;
            
            layer.VectorType = VectorLayerType.DraftLayer;

            table.FillData = true; ///空图层，属性信息已经填充，因为是空的
            table.Columns.Add("FID", typeof(int));
            table.Columns.Add("FeatID", typeof(string));
            table.Columns.Add("ClasID", typeof(Int64));
            table.Columns.Add("BeginTime", typeof(string));
            table.Columns.Add("ChangeType", typeof(string));
            group.Layers.Add(layer);
            m_Layers.Add(layer);

            group = new LayerGroup("工作区1");
            m_LayerGroups.Add(group);
        }
    
        public void BoundingBoxChangedBy(GeoData.GeoDataRow row)
        {
            row.Geometry.Bound = row.Geometry.GetBoundingBox();

            GeoData.GeoDataTable table = row.Table as GeoData.GeoDataTable;
            GeoLayer lyr = GetLayerByTable(table);         
        
            lyr.LayerBound = lyr.GetBoundingBox();
            LayerGroup group = GetGroupByLayer(lyr);
            group.LayerGroupBound = group.GetBoundingBox();
            MapBound = GetBoundingBox();
        }

        public void BoundingBoxChangedBy(GeoLayer layer)
        {
           // lyr.LayerBound = lyr.GetBoundingBox();

        }
        public GeoBound GetBoundingBox()
        {
            if (m_LayerGroups == null || m_LayerGroups.Count == 0)
            {
                return null;
            }
            GeoBound bound = null;

            for (int i = 0; i < m_LayerGroups.Count; ++i)
            {
                if (bound == null )
                {
                    if( m_LayerGroups[i].LayerGroupBound != null)
                    {
                        bound = m_LayerGroups[i].LayerGroupBound.Clone(); ;
                    }                    
                    continue;
                }
                bound.UnionBound(m_LayerGroups[i].LayerGroupBound);
            }
            return bound;
        }


    }
}
