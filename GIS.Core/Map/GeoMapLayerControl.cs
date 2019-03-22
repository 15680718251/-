using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Map;
using GIS.Layer;
namespace GIS.Map
{
    public partial class GeoMap :ILayerControl,ILayerGroupControl
    {
        private List<GeoLayer> m_Layers;   
        private List<LayerGroup> m_LayerGroups;
        private string m_ActiveLyrGroup = "工作区1";    //当前活动图层组名称
        private GeoLayer m_ActiveVectorLayer = null;    //活动矢量层14
        private GeoLayer m_ActiveLabelLayer  = null;    //活动注记层
         
        #region Properties
        
        public int LayerCounts
        {
            get
            {
                return m_Layers.Count;
            }
        }
        public int GroupCounts
        {
            get
            {
                return m_LayerGroups.Count;
            }
        }       
     
        /// <summary>
        /// 通过当前活动组的名称，找到当前工作区索引
        /// </summary>
        private int m_ActiveLyrGroupIndex
        {
            get
            {
                for (int i = 0; i < GroupCounts; i++)
                {
                    if (m_LayerGroups[i].LayerGroupName == ActiveLyrGroup)
                    {
                        return i;
                    }
                }
                return 0;
            }
        }
        /// <summary>
        /// 获得当前活动工作组的名称
        /// </summary>
        public string ActiveLyrGroup
        {
            get { return m_ActiveLyrGroup; }
            set { m_ActiveLyrGroup = value; }
        } 
        #endregion

        #region ILayerGroupControl
        /// <summary>
        /// 重新命名工作区
        /// </summary>
        /// <param name="strOldName">工作区老的名字</param>
        /// <param name="strNewName">不用说也知道，新的名字</param>
        /// <returns></returns>
        public bool LayerGroupReName(string strOldName, string strNewName)
        {
            LayerGroup group = GetGroupByName(strOldName);
            if (group == null)
                return false;

            for (int i = 0; i < GroupCounts; ++i)
            {
                if (m_LayerGroups[i].LayerGroupName == strNewName && m_LayerGroups[i] != group)
                {
                    return false;
                }
            }
            group.LayerGroupName = strNewName;
            return true;
        }
        public bool LayerGroupReName(int index, string strNewName)
        {
            LayerGroup group = GetGroupAt(index);
            if (group != null)
               return false;

            for (int i = 0; i < GroupCounts; ++i)
            {
                if (m_LayerGroups[i].LayerGroupName == strNewName && m_LayerGroups[i] != group)
                {
                    return false;
                }
            }
            group.LayerGroupName = strNewName;
            return true;

        }
 
        public LayerGroup GetGroupAt(int index)
        {
            if (index < 0 || index > GroupCounts)
                return null;

             return m_LayerGroups[index]; 
        }

        public LayerGroup GetGroupByLayer(GeoLayer lyr)
        {
            for (int i = 0; i < GroupCounts; i++)
            {
                LayerGroup group = m_LayerGroups[i];
                for (int j = 0; j < group.Counts; j++)
                {
                    if (group[j] == lyr)
                        return group;
                }
             }
            return null;
        }

        public LayerGroup GetGroupByName(string strGroupName)
        {
            for (int i = 0; i < GroupCounts; i++)
            {
                if (m_LayerGroups[i].LayerGroupName == strGroupName)
                {
                    return m_LayerGroups[i];
                }
            }
            return null;
        }
        public bool SetGroupEnable(string strGroupName, bool bEnable)
        {
            LayerGroup group = GetGroupByName(strGroupName);
            if (group != null)
            {               
                for (int i = 0; i < group.Counts; ++i)
                {
                    group[i].Enable = bEnable;
                }
                return group.Counts>0;
            }
            return false;
        } 
        #endregion

        #region ILayerControl
   
        public GeoLayer GetActiveVectorLayer()
        {
            return m_ActiveVectorLayer;
        }

        public GeoLayer GetActiveLabelLayer()
        {
            return m_ActiveLabelLayer;
        }

        public GeoLayer GetDraftMixLayer()
        {
            LayerGroup group = GetGroupByName("系统工作区");
            if (group == null)
                return null;
            for (int i = 0; i < group.Counts; i++)
            {
                GeoLayer layer = group[i];
                if (layer.LayerName == "草图层")
                {
                    return layer;
                }
            }
            return null;  
        }

        /// <summary>
        /// 设置活动图层
        /// </summary>
        /// <returns></returns>
        public bool SetActiveLayer(String strGroupName, String strLayerName)
        { 
            GeoLayer lyr = GetLayerByName(strLayerName);
            if (strLayerName == "草图层"
                ||lyr.LayerTypeDetail == LAYERTYPE_DETAIL.MixLayer)
            {
                m_ActiveLabelLayer = lyr;
                m_ActiveVectorLayer = lyr;
            }
            else if (lyr.LayerTypeDetail ==  LAYERTYPE_DETAIL.LabelLayer)
                m_ActiveLabelLayer = lyr;
            else if (lyr.LayerType == LAYERTYPE.VectorLayer)
                m_ActiveVectorLayer = lyr;
            
            return lyr != null;
        }

        public bool LayerExist(string strLayerName)
        {
            for (int i = 0; i < m_Layers.Count; i++)
            {
                if (m_Layers[i].LayerName == strLayerName)
                    return true;
            }
            return false; 
        }
        public bool SetLayerEnable(String strGroupName, string strLayerName, bool bIsEnable)
        {
            LayerGroup group = GetGroupByName(strGroupName);
            if (group != null)
            {
                GeoLayer lyr = group.GetLayerByName(strLayerName);
                if (lyr != null)
                {
                    lyr.Enable = bIsEnable;
                    return true;
                }
                return false;
            }
            return false;
        }
        public GeoLayer GetLayerByName(String strLayerName)
        {
            for (int i = 0; i < m_Layers.Count; i++)
            {
                if (m_Layers[i].LayerName == strLayerName)
                    return m_Layers[i];
            }
            return null;
        }
        public GeoLayer GetLayerAt(int index)
        {
            if (index < 0 || index > m_Layers.Count - 1)
                return null;
            else
                return m_Layers[index];
        }
        public GeoLayer GetLayerByName(String strGroupName, String strLayerName)
        {            
            LayerGroup group = GetGroupByName(strGroupName);
            if (group == null)
                return null;
            for (int i = 0; i < group.Counts; i++)
            {
                GeoLayer layer = group[i];
                if (layer.LayerName == strLayerName)
                {
                    return layer;
                }
            }
            return null;               
        }
        public GeoLayer GetLayerByTable(GeoData.GeoDataTable table)
        {
            if (table == null)
            {
                return null;
            }
            for (int i = 0; i < m_Layers.Count; i++)
            {
               GeoLayer layer = m_Layers[i];
               if (layer.LayerType == LAYERTYPE.VectorLayer)
               {
                   GeoVectorLayer vlyr = layer as GeoVectorLayer;
                   if (vlyr.DataTable == table)
                   {
                       return vlyr;
                   }
               } 
            }
            return null;   
        }
       

       
        #endregion
    }
}
