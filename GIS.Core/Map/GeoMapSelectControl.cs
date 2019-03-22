using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Collections.ObjectModel;
using GIS.GeoData.DataProviders;
using GIS.Layer;
using System.IO;
using GIS.Geometries;
using GIS.GeoData;

namespace GIS.Map
{
    public partial class GeoMap : ILayerControl, ILayerGroupControl
    {
        private List<GeoData.GeoDataRow> m_aSltObjSet;//选中几何对象的集合
        private List<GeoData.GeoDataRow> m_aSltLableSet;//选中的注记对象

        /// <summary>
        /// 选中的注记对象
        /// </summary>
        public List<GeoData.GeoDataRow> SltLableSet
        {
            get { return m_aSltLableSet; }
            set { m_aSltLableSet = value; }
        }

        /// <summary>
        /// 选中的几何对象
        /// </summary>
        public List<GeoData.GeoDataRow> SltGeoSet
        {
            get { return m_aSltObjSet; }
        }

        #region Function
        //
        //注释：
        // 清空选中对象集合中的所有元素
        public void ClearAllSelect()
        {
            for (int i = 0; i < m_aSltObjSet.Count; ++i)
            {
                GeoData.GeoDataRow dr = m_aSltObjSet[i];
                dr.SelectState = false;
            }
            m_aSltObjSet.Clear();
            for (int i = 0; i < m_aSltLableSet.Count; ++i)
            {
                GeoData.GeoDataRow dr = m_aSltLableSet[i];
                dr.SelectState = false;
            }
            m_aSltLableSet.Clear();
        }
        //
        //注释：
        // 添加选中元素到集合中
        public bool  AddSltObj(GeoData.GeoDataRow row)
        {
            row.SelectState = true;
         
            if (row.Geometry is GeoLabel)
            {
                SltLableSet.Add(row);
            }
            else
            {
               SltGeoSet.Add(row);
            } 
            return true;
        }
        //
        //注释：
        // 将选中元素从集合中删除
        public bool RemoveSltObj(GeoData.GeoDataRow row)
        {
            row.SelectState = false;

            if (row.Geometry is GeoLabel)
            {
                for (int i = 0; i < SltLableSet.Count; ++i)
                {
                    GeoData.GeoDataRow rowExist = SltLableSet[i];
                    if (rowExist == row)
                    { 
                        SltLableSet.RemoveAt(i);
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < SltGeoSet.Count; ++i)
                {
                    GeoData.GeoDataRow rowExist = m_aSltObjSet[i];
                    if (rowExist == row)
                    { 
                        m_aSltObjSet.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }
       
        #endregion

    }
}
