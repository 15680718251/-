using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using GIS.GeoData;
using System.IO;
using GIS.Layer;


namespace GIS.Increment
{

    public struct ChangeInfo
    {
        public List<GeoDataRow> ChangeAftInfo; //变化后信息
        public List<GeoDataRow> ChangeBefInfo; //变化前信息
        public ChangeType ChangeType;//变化类型
        
    }
    public class IncBase
    {
        public IncBase()
        {
        }

        protected List<ChangeInfo> m_ChangeInfoList = new List<ChangeInfo>();

        public  List<ChangeInfo> IncInfoList
        {
            get { return m_ChangeInfoList; }
            set { m_ChangeInfoList = value; }
        }
    
        //根据几何类型添加到链表LIST中
        public virtual void AddChangeInfoToList(List<ChangeInfo> list, GIS.Layer.VectorLayerType type)
        {
        }
        public bool ChangeInfoJudge(Geometry geom, GIS.Layer.VectorLayerType type)
        {
            if (type == GIS.Layer.VectorLayerType.PointLayer)
            {
                if (geom is GeoPoint || geom is GeoMultiPoint)
                {
                    return true;
                }
               
            }
            else if (type == GIS.Layer.VectorLayerType.LineLayer)
            {
                if (geom is GeoLineString || geom is GeoMultiLineString)
                {
                    return true;
                }
            }
            else if (type == GIS.Layer.VectorLayerType.PolygonLayer)
            {
                if (geom is GeoPolygon || geom is GeoMultiPolygon)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void WriteGeoInfo(StreamWriter sw, VectorLayerType type, GeoLayer layer)
        {
        }
        public virtual  void WriteAttributeInfo(StreamWriter fw, GeoLayer layer)
        {         
        }



        public virtual void AddGeomToList(List<GeoDataRow> list)
        {
            
        }
    }
}
