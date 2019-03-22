using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GIS.Geometries;
namespace GIS.Utilities
{
     public interface ITransForm
    {  
        // 将地理坐标转换为屏幕坐标
        /// </summary>
        /// <param name="pt">地理坐标点PT</param>
        /// <returns>返回屏幕坐标</returns>
        Point TransFromWorldToMap(GeoPoint pt);
        PointF TransFromWorldToMapF(GeoPoint pt);
        /// <summary>
        /// 给其它函数在需要的时候获取比例尺，2013.4.14日，阳成飞
        /// </summary>
        /// <returns></returns>
        double GetBlc();
        /// <summary>
        /// 将屏幕坐标转换成地理坐标
        /// </summary>
        /// <param name="pt">屏幕坐标PT</param>
        /// <returns>地理坐标</returns>
        GeoPoint TransFromMapToWorld(Point pt);
        GeoPoint TransFromMapToWorldF(PointF pt);
        /// <summary>
        /// 将屏幕大小转换为地理坐标形式的矩形
        /// </summary>
        /// <param name="size">屏幕大小</param>
        /// <returns></returns>
        GeoBound TransFromMapToWorld(Rectangle rect);
     
        /// <summary>
        /// 将一条线的地理坐标转为屏幕坐标数组
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
       Point[] TransLineToMap(GeoLineString line);
       PointF[] TransLineToMapF(GeoLineString line);
         /// <summary>
         /// 将矩形范围转成屏幕坐标
         /// </summary>
         /// <param name="bound"></param>
         /// <returns></returns>
       Rectangle TransFromWorldToMap(GeoBound bound);
         /// <summary>
         /// 将地图上的距离转换为实地距离
         /// </summary>
         /// <param name="len"></param>
         /// <returns></returns>
       double TransFromMapToWorld(int len);
       double TransFromMapToWorld(float len);
         /// <summary>
         /// 将实地距离转换为地图距离
         /// </summary>
         /// <param name="len"></param>
         /// <returns></returns>
       float TransFromWorldToMap(double len);
    }
}
