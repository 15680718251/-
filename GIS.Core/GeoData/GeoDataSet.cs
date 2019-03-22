using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
namespace GIS.GeoData
{
    /// <summary>
    /// Repsents the corellection of tables for the FeatureDataSet.
    /// </summary>
    [Serializable()]
    public class GeoTableCollection : List<GeoDataTable>
    {
    }
    public class GeoDataSet:DataSet
    {
        private GeoTableCollection m_GeoTables;
        public new GeoTableCollection Tables
        {
            get { return m_GeoTables; }
        }
        public GeoDataSet()
        {
            InitClass();
        }
        /// <summary>
        /// 初始化表
        /// </summary>
        private void InitClass()
        {
            m_GeoTables = new GeoTableCollection();
        }
        public new GeoDataSet Clone()
        {
            GeoDataSet cln = (GeoDataSet)(base.Clone());
            return cln;
        }
    }
}
