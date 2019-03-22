using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GIS.mm_Conv_Symbol
{

    public class SymbolBound
    {
        public SymbolBound():this(0,0,0,0)
        {
            m_bfirst = true;
        }
        public SymbolBound(double minX, double minY, double maxX, double maxY)
        {
            m_LeftBottomPt = new GeoPoint(minX, minY);
            m_RightUpPt = new GeoPoint(maxX, maxY);
            CheckMinMax();
            m_bfirst = false;
        }

        //public SymbolBound(GeoPoint lowerLeft, GeoPoint upperRight)
        //    : this(lowerLeft.X, lowerLeft.Y, upperRight.X, upperRight.Y)
        //{
        //}

        //矩形的左下角坐标和右上角坐标
        private GeoPoint m_LeftBottomPt;
        private GeoPoint m_RightUpPt;
        private bool m_bfirst = true;

        #region 属性
        public bool IsEmpty
        {
            get
            {
                return (m_LeftBottomPt.X == 0 && m_LeftBottomPt.Y == 0
                    && m_RightUpPt.X == 0 && m_RightUpPt.Y == 0);
            }
        }
        //边界矩形左下角坐标
        public GeoPoint LeftBottomPt
        {
            get { return m_LeftBottomPt; }
            set { m_LeftBottomPt = value; }
        }
        //边界矩形右上角坐标
        public GeoPoint RightUpPt
        {
            get { return m_RightUpPt; }
            set { m_RightUpPt = value; }
        }
        public Double Left
        {
            get { return m_LeftBottomPt.X; }
            set { m_LeftBottomPt.X = value; }
        }
        public Double Bottom
        {
            get { return m_LeftBottomPt.Y; }
            set { m_LeftBottomPt.Y = value; }
        }
        public Double Top
        {
            get { return m_RightUpPt.Y; }
            set { m_RightUpPt.Y = value; }
        }
        public Double Right
        {
            get { return m_RightUpPt.X; }
            set { m_RightUpPt.X = value;}
        }
        public Double Width
        {
            get { return Right - Left; }
        }

        public Double Height
        {
            get { return Top - Bottom; }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 重新设置边界矩形的左下右上坐标
        /// </summary>
        /// <param name="l">左下角坐标</param> 
        /// <param name="b"></param>
        /// <param name="r">右上角坐标</param>
        /// <param name="t"></param>
        public void SetExtents(double l,double b,double r,double t)
        {
            m_LeftBottomPt.SetXY(l, b);
            m_RightUpPt.SetXY(r, t);
        }
        /// <summary>
        /// 通过一个外边界矩形来设置当前地理范围
        /// </summary>
        /// <param name="bound"></param>
        public void SetExtents(SymbolBound bound)
        {
            SetExtents(bound.Left, bound.Bottom, bound.Right, bound.Top);
        }
        /// <summary>
        /// 判断两个矩形是否相交
        /// </summary>
        /// <param name="bound">输入矩形</param>
        /// <returns></returns>
        public bool IsIntersectWith(SymbolBound bound)
        {
            return !(bound.Left > this.Right ||
                     bound.Right < this.Left ||
                     bound.Top < this.Bottom ||
                     bound.Bottom > this.Top);
        }

        public bool IsPointIn(GeoPoint pt)
        {
            return !(pt.X < Left || pt.X > Right || pt.Y < Bottom || pt.Y > Top); 
        }
        /// <summary>
        /// 矩形合并，不创建新的矩形。
        /// </summary>
        /// <param name="bound">被合并的矩形</param> 
        public void UnionBound(SymbolBound bound)
        {
            if(bound == null)
                return;
            if (m_bfirst)
            {
                this.m_LeftBottomPt = new GeoPoint(bound.Left, bound.Bottom);
                this.m_RightUpPt = new GeoPoint(bound.Right, bound.Top);
                m_bfirst = false;
                return;
            }

            if (bound.Left < Left)
                Left = bound.Left;
            if (bound.Right > Right)
                Right = bound.Right;
            if (bound.Top > Top)
                Top = bound.Top;
            if (bound.Bottom < Bottom)
                Bottom = bound.Bottom;
            return;
        }

        /// <summary>
        /// 合并点，不创建新的矩形
        /// </summary>
        /// <param name="pt"></param>
        public void UnionPoint(GeoPoint pt)
        {
            if (m_bfirst)
            {
                this.m_LeftBottomPt = pt.Clone() as GeoPoint;
                this.m_RightUpPt = pt.Clone() as GeoPoint;
                m_bfirst = false;
                return;
            }
            double x = pt.X;
            double y = pt.Y;
            if (x < Left) Left = x;
            if (x > Right) Right = x;
            if (y < Bottom) Bottom = y;
            if (y > Top) Top = y;   
            return;
        }

        public void UnionPoint(PointF pt)
        {
            if (m_bfirst)
            {
                this.m_LeftBottomPt = new GeoPoint((double)pt.X, (double)pt.Y);
                this.m_RightUpPt = new GeoPoint((double)pt.X, (double)pt.Y);
                m_bfirst = false;
                return;
            }
            float x = pt.X;
            float y = pt.Y;
            if (x < Left) Left = (double)x;
            if (x > Right) Right = (double)x;
            if (y < Bottom) Bottom = (double)y;
            if (y > Top) Top = (double)y;
            return;
        }
        /// <summary>
        /// 计算边界矩形集的 边界矩形
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        //public static SymbolBound UnionBounds(SymbolBound[] bounds)
        //{
        //    if (bounds == null || bounds.Length == 0)
        //        return null;
        //    SymbolBound bound = bounds[0].Clone();
        //    for (int i = 1; i < bounds.Length; ++i)  
        //    {
        //        bound.UnionBound(bounds[i]);
        //    }
        //    return bound;
        //}
        
        /// <summary>
        /// 创建当前矩形的Copy
        /// </summary>
        /// <returns></returns>
        public SymbolBound Clone()
        {
            SymbolBound bound = new SymbolBound(Left, Bottom, Right, Top);
            bound.m_bfirst = this.m_bfirst;
            return bound;

        }

        /// <summary>
        /// 返回中心点，调用了点的操作运算重载
        /// </summary>
        /// <returns></returns>
        public GeoPoint GetCentroid()
        {
            return (LeftBottomPt+RightUpPt) * .5f;
        }

        /// <summary>
        /// 检测边界矩形的端点坐标是否正确
        /// </summary>
        /// <returns></returns>
        public bool CheckMinMax()
        {
            bool bSwapped = false;
            if (m_LeftBottomPt.X > m_RightUpPt.X)
            {
                double tmp = m_LeftBottomPt.X;
                m_LeftBottomPt.X = m_RightUpPt.X;
                m_RightUpPt.X = tmp;
                bSwapped = true;
            }
            if (m_LeftBottomPt.Y > m_RightUpPt.Y)
            {
                double tmp = m_RightUpPt.Y;
                m_LeftBottomPt.Y = m_RightUpPt.Y;
                m_RightUpPt.Y = tmp;
                bSwapped = true;
            }
            return bSwapped;
        }

        //SymbolBound对象膨胀，左下角和右上角分别膨胀dx,dy，结果保存到CMapBound对象
        public SymbolBound InflateBound(double dx, double dy)
        {
            return new SymbolBound(Left - dx, Bottom - dy, Right + dx, Top + dy);
        }

        //CMapBound对象收缩，左下角和右上角分别收缩dx,dy，结果保存到CMapBound对象
        public SymbolBound DeflateBound(double dx, double dy)
        {
            return new SymbolBound(Left + dx, Bottom + dy, Right - dx, Top - dy);
        }
        //判断两个矩形是否相等
        public bool isEqual(SymbolBound bound)
        {
            if (bound == null)
                return false;
            return LeftBottomPt.IsEqual(bound.LeftBottomPt) && RightUpPt.IsEqual(bound.RightUpPt);
        }
        #endregion
       
       
    }
}