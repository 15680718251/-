/*******************************************************
 * 文档说明：BufferConsoleApplication中常用的通用数学函数. by zbl 2018.7.11
 ******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.OSMIncrement.Buffer
{
    /// <summary>
    /// 常用的通用数学函数
    /// </summary>
    public class MathTool
    {
        /// <summary>
        /// 获取由两个点所形成的向量的象限角度:指以顺时针方向，从直线到x正方向的夹角
        /// </summary>
        /// <param name="preCoord">第一个点的坐标</param>
        /// <param name="nextCoord">第二个点的坐标</param>
        /// <returns></returns>
        public static double GetQuadrantAngle(Coordinate preCoord, Coordinate nextCoord)
        {
            return GetQuadrantAngle(nextCoord.X - preCoord.X, nextCoord.Y - preCoord.Y);
        }
        /// <summary>
        /// 由增量X和增量Y所形成的向量的象限角度
        /// </summary>
        /// <param name="x">增量X</param>
        /// <param name="y">增量Y</param>
        /// <returns>象限角</returns>
        public static double GetQuadrantAngle(double x, double y)
        {
            double theta = Math.Atan(y / x);
            if (x > 0 && y > 0) return theta;
            if (x > 0 && y < 0) return Math.PI * 2 + theta;
            if (x < 0 && y > 0) return theta + Math.PI;
            if (x < 0 && y < 0) return theta + Math.PI;
            return theta;
        }
        /// <summary>
        /// 获取由相邻的三个点所形成的两个向量之间的夹角  （0到PI弧度）
        /// </summary>
        /// <param name="preCoord"></param>
        /// <param name="midCoord"></param>
        /// <param name="nextCoord"></param>
        /// <returns></returns>
        public static double GetIncludedAngel(Coordinate preCoord, Coordinate midCoord, Coordinate nextCoord)
        {
            double innerProduct = (midCoord.X - preCoord.X) * (nextCoord.X - midCoord.X) + (midCoord.Y - preCoord.Y) * (nextCoord.Y - midCoord.Y);
            double mode1 = Math.Sqrt(Math.Pow((midCoord.X - preCoord.X), 2.0) + Math.Pow((midCoord.Y - preCoord.Y), 2.0));
            double mode2 = Math.Sqrt(Math.Pow((nextCoord.X - midCoord.X), 2.0) + Math.Pow((nextCoord.Y - midCoord.Y), 2.0));
            return Math.Acos(innerProduct / (mode1 * mode2));
        }
        /// <summary>
        /// 获取由两个点所形成的向量的模(长度)
        /// </summary>
        /// <param name="preCoord">第一个点</param>
        /// <param name="nextCoord">第二个点</param>
        /// <returns>由两个点所形成的向量的模(长度)</returns>
        public static double GetDistance(Coordinate preCoord, Coordinate nextCoord)
        {
            return Math.Sqrt(Math.Pow((nextCoord.X - preCoord.X), 2) + Math.Pow((nextCoord.Y - preCoord.Y), 2));
        }
    }
}
