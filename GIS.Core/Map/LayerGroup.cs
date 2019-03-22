using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Layer;
using GIS.Geometries;

namespace GIS.Map
{ 
    public class LayerGroup
    {
        public LayerGroup()
        {
            m_Layers = new List<GeoLayer>();
        }
        public LayerGroup(string name)
        {
            m_LayerGroupName = name;
            m_Layers = new List<GeoLayer>();
        }
        private string m_LayerGroupName = "未命名";      //分组图层名

        private List<GeoLayer> m_Layers;                 //图层组中的图层集

        private GeoBound m_LayerGroupBound;              // 图层组的外边界

        private bool m_bEnable = true;                          //图层组是否可见

        #region Properties
        public bool Enable
        {
            get { return m_bEnable; }
            set { m_bEnable = value; }
        }
        public int Counts
        {
            get { return m_Layers.Count; }
        }
        public string LayerGroupName
        {
            get { return m_LayerGroupName; }
            set { m_LayerGroupName = value; }
        }
        public List<GeoLayer> Layers
        {
            get { return m_Layers; }
            set { m_Layers = value; }
        }
        public GeoBound LayerGroupBound
        {
            get
            {
                if (m_LayerGroupBound == null)
                    m_LayerGroupBound = GetBoundingBox();
                return m_LayerGroupBound;
            }
            set
            {
                m_LayerGroupBound = value;
            }
        }
        
        #endregion

        #region Funtion
        public GeoLayer this[int index]
        {
            get
            {
                return m_Layers[index];
            }
        }
        /// <summary>
        /// 计算该图层组的外边界矩形
        /// </summary>
        public GeoBound GetBoundingBox()
        {
            if (m_Layers == null || m_Layers.Count == 0)
                return null;
            GeoBound bound = null;
            for (int i = 0; i < m_Layers.Count; ++i)
            {
                if (bound == null)
                {
                    if (m_Layers[i].LayerBound != null)
                    {
                        bound = m_Layers[i].LayerBound.Clone() ;
                    }
                    continue;
                }
                bound.UnionBound(m_Layers[i].LayerBound);
            }
            return bound;
        }

        public GeoLayer GetLayerByName(string strLayerName)
        {
            for (int i = 0; i < Counts; i++)
            {
                if (m_Layers[i].LayerName == strLayerName)
                    return m_Layers[i];
            }
            return null;
        }



        #endregion
    }
}
