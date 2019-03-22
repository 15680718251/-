using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.TreeIndex.UIEventArgs
{
    public class EagleMapEventArgs
    {
       public EagleMapEventArgs(Geometries.GeoBound bound,bool bRefreshAll)
        {
            m_ViewExtent= bound.Clone();
            m_bNeedRefreshAll = bRefreshAll;
        }
        private Geometries.GeoBound m_ViewExtent;

        private bool m_bNeedRefreshAll ;
        /// <summary>
        /// 刷新部分还是全部
        /// </summary>
        public bool  NeedRefreshAll
        {
            get { return m_bNeedRefreshAll; }
            
        }
        /// <summary>
        /// 视图范围大小
        /// </summary>
        public Geometries.GeoBound ViewExtent
        {
            get { return m_ViewExtent; }
            set { m_ViewExtent = value; }
        }
    }
}
