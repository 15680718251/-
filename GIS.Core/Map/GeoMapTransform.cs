using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using GIS.Layer;

namespace GIS.Map
{
    public partial class GeoMap
    {

        #region 地图坐标转换
       
        private bool bShowAsPaper = false;//是否显示成图纸坐标
        private double m_MapScale = 1000/1000;//前面的代表地图的比例尺，不是缩放比例尺,,,,,单位毫米
        private GeoBound m_MapBound;      //当前的地图范围。可以是图纸坐标范围，也可以是地理坐标范围
        private GeoBound m_MapGeoBound;   //当前的地理范围。做备份用

        #region MapBoundChangeEvent
        public delegate void MapBoundChangeEventHandler(bool refresh);
        public event MapBoundChangeEventHandler MapBoundChange; 
        #endregion

        public GeoBound MapBound
        {
            get
            {
                if (m_MapBound == null)
                    m_MapBound = GetBoundingBox();
                return m_MapBound;
            }
            set
            {
                if (m_MapBound == null && value == null)
                    return;
                if ((m_MapBound == null && value != null)|| !m_MapBound.isEqual(value))//如果边界矩形不相等，则赋值，并触发委托
                {
                    m_MapBound = value;
                    if (MapBoundChange != null)
                    {
                    }
                }
            }
        } 
        //设置地图比例尺
        public double MapScale
        {
            get { return m_MapScale; }
            set { m_MapScale =  value/1000; }
        }
        //是否显示为图纸坐标
        public bool ShowAsPaper
        {
            get { return bShowAsPaper; }
            set 
            {
                if (bShowAsPaper != value)
                {
                    bShowAsPaper = value;
                    if (bShowAsPaper == true)
                    {
                        TransFromWorldToPaper();
                    }
                    else
                    {
                        TransFromPaperToWorld();
                    }
                }
            }
        }

        //将图层里的所有地理数据转为图纸坐标
        public void TransFromWorldToPaper()
        {
            for (int i = 0; i < GroupCounts; i++)
            {
                LayerGroup group = m_LayerGroups[i];
                if (group.Counts > 0)
                {
                    //将图层组里的所有地理数据转为图纸坐标
                    for (int j = 0; j < group.Counts; j++)
                    {
                        GeoLayer lyr = group.Layers[j];
                        GIS.Utilities.Transform.WorldToPaper(lyr,m_MapBound,m_MapScale);
                        lyr.LayerBound = lyr.GetBoundingBox();
                    } 
                    group.LayerGroupBound = group.GetBoundingBox();
                }
            }
            m_MapGeoBound = MapBound;
            MapBound = GetBoundingBox();
        }

        //将图层里的的所有图纸坐标数据转为地理数据
        public void TransFromPaperToWorld()
        {
            for (int i = 0; i < GroupCounts; i++)
            {
                LayerGroup group = m_LayerGroups[i];
                if (group.Counts > 0)
                {
                    //将图层组里的所有地理数据转为图纸坐标
                    for (int j = 0; j < group.Counts; j++)
                    {
                        GeoLayer lyr = group.Layers[j];
                        GIS.Utilities.Transform.PaperToWorld(lyr, m_MapGeoBound, m_MapScale);
                        lyr.LayerBound = lyr.GetBoundingBox();
                    }
                    group.LayerGroupBound = group.GetBoundingBox();
                }
            } 
            MapBound = GetBoundingBox();
            m_MapGeoBound = null;
        }

        #endregion
    }
}
