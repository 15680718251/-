using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Windows.Forms;

namespace GIS.Toplogical
{
    public static class TpRelatemain
    {
        public static TpRelateConstants IsTpWith(Geometry geom1,Geometry geom2, bool bReport)
        {
            if( geom1 is GeoPoint)
            {
                if(geom2 is GeoPoint)
                {
                    return TpPointtoPoint.IsTpWith((GeoPoint)geom1, (GeoPoint)geom2, bReport);
                }
                else if (geom2 is GeoLineString)
                {
                    return TpPointtoLines.IsTpWith((GeoPoint)geom1, (GeoLineString)geom2, bReport);///点、单线
                }
                else if (geom2 is GeoMultiLineString)
                {
                   return  TpPointtoLines.IsTpWith((GeoPoint)geom1, (GeoMultiLineString)geom2, true);///点、多线
                }
                else if (geom2 is GeoPolygon)
                {
                    return TpPointtoPolygon.IsTpWith((GeoPoint)geom1, (GeoPolygon)geom2, bReport);///点、面
                }
                else
                {
                    return TpRelateConstants.tpUnknow;
                    
                }
            }
            else if (geom1 is GeoLineString)
            {
                if (geom2 is GeoPoint)
                {
                    return TpPointtoLines.IsTpWith((GeoLineString)geom1, (GeoPoint)geom2, bReport);///单线、点
                }
                else if (geom2 is GeoLineString)
                {
                    return TpLinetoLine.IsTpWith((GeoLineString)geom1, (GeoLineString)geom2, bReport);
                    ///单线、单线
                }
                else if (geom2 is GeoPolygon)
                {
                    return TpLinetoPolygon.IsTpWith((GeoLineString)geom1, (GeoPolygon)geom2, bReport);///单线、面
                }
                else
                {
                  //  MessageBox.Show("对不起，选择对象类型错误，待扩充", "GIS");
                    return TpRelateConstants.tpUnknow;
                }
            }
            else if (geom1 is GeoPolygon)
            {
                if (geom2 is GeoPoint)
                {
                    return TpPointtoPolygon.IsTpWith((GeoPolygon)geom1, (GeoPoint)geom2, bReport);///面、点
                }
                else if (geom2 is GeoLineString)
                {
                    return TpLinetoPolygon.IsTpWith((GeoPolygon)geom1, (GeoLineString)geom2, bReport);///面、单线
                }
                else if (geom2 is GeoPolygon)
                {
                    return TpPolygontoPolygon.IsTpWith((GeoPolygon)geom1, (GeoPolygon)geom2, bReport);///面、面
                }
                else
                {
                  //  MessageBox.Show("对不起，选择对象类型错误，待扩充", "GIS");
                    return TpRelateConstants.tpUnknow;                 
                }
            }
            else
            {
              //  MessageBox.Show("对不起，选择对象类型错误，待扩充", "GIS");
                return TpRelateConstants.tpUnknow;
            }
            
        }
    }
}
