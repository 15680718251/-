using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using GIS.Map;
using GIS.Layer;
namespace GIS.Increment
{
    public partial class IncManager
    {
        public IncManager(Map.GeoMap map) 
        {
            m_IncArray = new List<IncBase>();            
            m_Map = map;
            m_DictDataType = new Dictionary<string, string>();
            m_DictDataType.Add(typeof(int).ToString(), "integer");
            m_DictDataType.Add(typeof(Int64).ToString(), "long");
            m_DictDataType.Add(typeof(float).ToString(), "float");
            m_DictDataType.Add(typeof(double).ToString(), "double");
            m_DictDataType.Add(typeof(string).ToString(), "char");
            m_DictDataType.Add(typeof(bool).ToString(), "bool");
            m_DictDataType.Add(typeof(Int16).ToString(), "short");
           
        }
        private Dictionary<string, string> m_DictDataType;
        private List<IncBase> m_IncArray;
        private GIS.Map.GeoMap m_Map;


        #region Function
        public void AddIncInfo(IncBase info)
        {
            m_IncArray.Add(info);
        }
        public void RemoveIncInfo(IncBase info)
        {
            m_IncArray.Remove(info);
        }
        public List<GeoData.GeoDataRow> GetGeomForRender()
        {
            List<GeoData.GeoDataRow> list = new List<GIS.GeoData.GeoDataRow>();
            for (int i = 0; i < m_IncArray.Count; i++)
            {
                IncBase incInfo = m_IncArray[i];
                incInfo.AddGeomToList(list);
            }
            return list;
        }
        public List<ChangeInfo> GetIncInfoByGeoType(VectorLayerType type)
        {
            List<ChangeInfo> m_list = new List<ChangeInfo>();
            for (int i = 0; i < m_IncArray.Count; i++)
            {
                IncBase incInfo = m_IncArray[i];
                incInfo.AddChangeInfoToList(m_list, type);
            }
            return m_list;
        } 
        #endregion





    }
}
