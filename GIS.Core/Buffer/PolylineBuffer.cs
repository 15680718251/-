/***********************************************************************
 *  文档说明：线缓冲区边界生成算法
 **********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.Buffer
{
    /// <summary>
    /// 线缓冲区边界生成算法
    /// </summary>
    public class PolylineBuffer
    {
        /// <summary>
        /// 根据给定的一系列有顺序的坐标，右侧缓冲区边界坐标：圆弧拟合处为逆时针
        /// </summary>
        /// <param name="strPolyLineCoords">一系列有顺序的坐标</param>
        /// <param name="radius">缓冲区半径</param>
        /// <returns>缓冲区的边界坐标</returns>
        public static string GetBufferEdgeCoords(string strPolyLineCoords, double radius)
        {
            //参数处理
            if (strPolyLineCoords.Trim().Length < 1)
                MessageBox.Show("缓冲区输入坐标串为空");

            string[] strCoords = strPolyLineCoords.Split(new char[] { ';' });
            List<Coordinate> coords = new List<Coordinate>();
            foreach (string coord in strCoords)
            {
                coords.Add(new Coordinate(coord));
            }

            //分别生成右侧和左侧的缓冲区边界点坐标串
            string rightBufferCoords = GetRightBufferEdgeCoords(coords, radius);
            coords.Reverse();
            string leftBufferCoords = GetRightBufferEdgeCoords(coords, radius);
            return rightBufferCoords + ";" + leftBufferCoords;
        }

        #region Private Methods
        /// <summary>
        /// 根据给定的一系列有顺序的坐标，生成轴线右侧的缓冲区边界点
        /// </summary>
        /// <param name="coords">一系列有顺序的坐标</param>
        /// <param name="radius">缓冲区半径</param>
        /// <returns>缓冲区的边界坐标</returns>
        private static string GetRightBufferEdgeCoords(IList<Coordinate> coords, double radius)
        {
            //参数处理
            if (coords.Count < 1) return "";
            else if (coords.Count < 2) return PointBuffer.GetBufferEdgeCoords(coords[0], radius);

            //计算时所需变量
            double alpha = 0.0;//向量绕起始点沿顺时针方向旋转到X轴正半轴所扫过的角度
            double delta = 0.0;//前后线段所形成的向量之间的夹角
            double l = 0.0;//前后线段所形成的向量的叉积

            //辅助变量
            StringBuilder strCoords = new StringBuilder();
            double startRadian = 0.0;
            double endRadian = 0.0;
            double beta = 0.0;
            double x = 0.0, y = 0.0;

            //第一节点的缓冲区
            {
                alpha = MathTool.GetQuadrantAngle(coords[0], coords[1]);
                startRadian = alpha + Math.PI;
                endRadian = alpha + (3 * Math.PI) / 2;
                strCoords.Append(GetBufferCoordsByRadian(coords[0], startRadian, endRadian, radius));
            }

            //中间节点
            for (int i = 1; i < coords.Count - 1; i++)
            {
                alpha = MathTool.GetQuadrantAngle(coords[i], coords[i + 1]);
                delta = MathTool.GetIncludedAngel(coords[i - 1], coords[i], coords[i + 1]);
                l = GetVectorProduct(coords[i - 1], coords[i], coords[i + 1]);
                if (l > 0)
                {
                    startRadian = alpha + (3 * Math.PI) / 2 - delta;
                    endRadian = alpha + (3 * Math.PI) / 2;
                    if (strCoords.Length > 0) strCoords.Append(";");
                    strCoords.Append(GetBufferCoordsByRadian(coords[i], startRadian, endRadian, radius));
                }
                else if (l < 0)
                {
                    beta = alpha - (Math.PI - delta) / 2;
                    x = coords[i].X + radius * Math.Cos(beta);
                    y = coords[i].Y + radius * Math.Sin(beta);
                    if (strCoords.Length > 0) strCoords.Append(";");
                    strCoords.Append(x.ToString() + "," + y.ToString());
                }
            }

            //最后一个点
            {
                alpha = MathTool.GetQuadrantAngle(coords[coords.Count - 1], coords[coords.Count - 2]);
                startRadian = alpha + (Math.PI) / 2;
                endRadian = alpha + Math.PI;
                if (strCoords.Length > 0) strCoords.Append(";");
                strCoords.Append(GetBufferCoordsByRadian(coords[coords.Count - 1], startRadian, endRadian, radius));
            }

            return strCoords.ToString();
        }
        /// <summary>
        /// 获取指定弧度范围之间的缓冲区圆弧拟合边界点
        /// </summary>
        /// <param name="center">指定拟合圆弧的原点</param>
        /// <param name="startRadian">开始弧度</param>
        /// <param name="endRadian">结束弧度</param>
        /// <param name="radius">缓冲区半径</param>
        /// <returns>缓冲区的边界坐标</returns>
        private static string GetBufferCoordsByRadian(Coordinate center, double startRadian, double endRadian, double radius)
        {
            //double gamma = Math.PI / 6;
            double gamma = (endRadian - startRadian) / 5;
            StringBuilder strCoords = new StringBuilder();
            double x = 0.0, y = 0.0;
            for (double phi = startRadian; phi <= endRadian + 0.000000000000001; phi += gamma)
            {
                x = center.X + radius * Math.Cos(phi);
                y = center.Y + radius * Math.Sin(phi);
                if (strCoords.Length > 0) strCoords.Append(";");
                strCoords.Append(x.ToString() + "," + y.ToString());
            }
            return strCoords.ToString();
        }
        /// <summary>
        /// 获取相邻三个点所形成的两个向量的交叉乘积
        /// </summary>
        /// <param name="preCoord">第一个节点坐标</param>
        /// <param name="midCoord">第二个节点坐标</param>
        /// <param name="nextCoord">第三个节点坐标</param>
        /// <returns>相邻三个点所形成的两个向量的交叉乘积</returns>
        private static double GetVectorProduct(Coordinate preCoord, Coordinate midCoord, Coordinate nextCoord)
        {
            return (midCoord.X - preCoord.X) * (nextCoord.Y - midCoord.Y) - (nextCoord.X - midCoord.X) * (midCoord.Y - preCoord.Y);
        }
        #endregion
    }
}
