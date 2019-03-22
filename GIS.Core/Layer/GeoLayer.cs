using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using GIS.Geometries;
using GIS.GeoData;
using GIS.Utilities;
using System.Runtime.Serialization;
namespace GIS.Layer
{
    [Serializable]
    /// <summary>
    /// 图层的类型
    /// </summary>
    public enum LAYERTYPE
    {
        GeoLayer    = 0,              //图层基类
        VectorLayer = 1,           //矢量图层   
        RasterLayer = 11,           //栅格图层
        DemLayer    = 15           //DEM图层 
    }
    public enum LAYERTYPE_DETAIL
    {
        PointLayer = 1,              //点类
        LineLayer = 3,           //线层      
        PolygonLayer = 5,           //面
        MixLayer = 7,             //混合层 
        LabelLayer = 9,//注记
        RasterLayer = 11,      //栅格图层
        SurveyLayer = 100,//观测层
        DraftLayer = 200 //草图层
    }
    public abstract class GeoLayer:IDisposable
    {
        public GeoLayer()
        {
        }
        public GeoLayer(String strFilePathName)
        {
            PathName = strFilePathName;
        }

        #region PrivateMember
        private GeoBound m_LayerBound = null;      //图层的外边界矩形  

        private string m_LayerName = "未命名";     //图层名
        private string m_PathName = null;          //图层路径

        private bool m_bEnable = true;             //图层是否可见          
        
        private Int64 m_ClasID; //要素编码
    
        #endregion

        #region Properties   
        public Int64 ClasID
        {
            get { return m_ClasID; }
            set { m_ClasID = value; }
        }
        //路径名，同时设置图层名
        public string PathName
        {
            get { return m_PathName; }
            set 
            { 
                m_PathName = value;
                m_LayerName = Path.GetFileNameWithoutExtension(value);
            }
        }
        //图层类型
        public virtual LAYERTYPE LayerType
        {
            get { return LAYERTYPE.GeoLayer; }
        }
        public abstract LAYERTYPE_DETAIL LayerTypeDetail
        {
            get;
        }
        public GeoBound LayerBound
        {
            get
            {
                if (m_LayerBound == null|| m_LayerBound.IsEmpty)
                    m_LayerBound = GetBoundingBox();
                return m_LayerBound;
            }
            set { m_LayerBound = value; }
        }
        public string LayerName
        {
            get { return m_LayerName; }
            set { m_LayerName = value; }
        }
        public bool Enable
        {
            get { return m_bEnable; }
            set { m_bEnable = value; }
        }  
        #endregion


        #region IDisposable Members

        /// <summary>
        /// Disposes the node
        /// </summary>
        public virtual void Dispose()
        { 
            m_LayerBound = null;
        }

        #endregion


        #region MyFunction
        public abstract GeoBound GetBoundingBox();

       
        #endregion
    }
}
