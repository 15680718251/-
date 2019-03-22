/************************************************************
 *  文档说明：本文件是点缓冲区边界生成算法的C#实现。
 ************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.Buffer
{
    /// <summary>
    /// 点缓冲区边界生成算法
    /// </summary>
    public class PointBuffer
    {
        #region Public Members
        /// <summary>
        /// 用于近似表示点缓冲区边界的内接正多边形的边数N
        /// </summary>
        public static int N = 12;
        #endregion

        #region Public Static Methods
        /// <summary>
        /// 根据一个给定点的坐标，生成基于这个点的点缓冲区边界点坐标串(逆时针)
        /// </summary>
        /// <param name="center">一个给定点的坐标</param>
        /// <param name="radius">缓冲区的半径</param>
        /// <returns>点缓冲区边界点坐标串(逆时针)</returns>
        public static string GetBufferEdgeCoords(Coordinate center, double radius)
        {
            double alpha = 0.0;
            double gamma = (2 * Math.PI) / N;

            StringBuilder strCoords = new StringBuilder();
            double x = 0.0, y = 0.0;
            for (double phi = 0; phi <= (N - 1) * gamma; phi += gamma)
            {
                x = center.X + radius * Math.Cos(alpha + phi);
                y = center.Y + radius * Math.Sin(alpha + phi);
                if (strCoords.Length > 0) strCoords.Append(";");
                strCoords.Append(x.ToString() + "," + y.ToString());
            }
            return strCoords.ToString();
        }
        #endregion
    }
}

