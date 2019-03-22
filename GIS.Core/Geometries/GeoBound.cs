using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace GIS.Geometries
{
    /// <summary>
    /// 几何体的外包矩形
    /// </summary>
    [Serializable]
    public class GeoBound
    {
        public double[] max;
        public double[] min;

        public GeoBound() : this(0, 0, 0, 0) { }

        public GeoBound(double minX, double minY, double maxX, double maxY)
        {
            //m_LeftBottomPt = new GeoPoint(minX, minY);
            //m_RightUpPt = new GeoPoint(maxX, maxY);
            min = new double[2];
            max = new double[2];
            //CheckMinMax();
            set(minX, minY, maxX, maxY);
        }

        public GeoBound(string s)
        {
            //m_LeftBottomPt = new GeoPoint(minX, minY);
            //m_RightUpPt = new GeoPoint(maxX, maxY);
            if (!s.Trim().Equals(""))
            {
                string[] box = s.Split(',');
                //CheckMinMax();
                set(double.Parse(box[0]), double.Parse(box[1]), double.Parse(box[2]), double.Parse(box[3]));
            }
            
        }

        public GeoBound(GeoPoint lowerLeft, GeoPoint upperRight) : this(lowerLeft.X, lowerLeft.Y, upperRight.X, upperRight.Y) { }

        public GeoBound(double[] min, double[] max)
        {
            if (min.Length != 2 || max.Length != 2)
            {
                throw new Exception("Error in Rectangle constructor: " + "min and max arrays must be of length " + 2);
            }

            this.min = new double[2];
            this.max = new double[2];

            set(min, max);
        }

        //矩形的左下角坐标和右上角坐标
        //private GeoPoint m_LeftBottomPt;
        //private GeoPoint m_RightUpPt;

        internal void set(double x1, double y1, double x2, double y2)
        {
            min[0] = Math.Min(x1, x2);
            min[1] = Math.Min(y1, y2);
            max[0] = Math.Max(x1, x2);
            max[1] = Math.Max(y1, y2);
        }

        internal void set(double[] min, double[] max)
        {
            System.Array.Copy(min, 0, this.min, 0, 2);
            System.Array.Copy(max, 0, this.max, 0, 2);
        }

        #region 属性
        public bool IsEmpty
        {
            get
            {
                return (min[0] == 0 && min[1] == 0 && max[0] == 0 && max[1] == 0);
            }
        }
        public GeoPoint LeftBottomPt //边界矩形左下角坐标
        {
            get { return new GeoPoint(min); }
        }

        public GeoPoint RightUpPt //边界矩形右上角坐标
        {
            get { return new GeoPoint(max); }
        }

        public Double Left
        {
            get { return min[0]; }
            set { min[0] = value; }
        }
        public Double Bottom
        {
            get { return min[1]; }
            set { min[1] = value; }
        }
        public Double Top
        {
            get { return max[1]; }
            set { max[1] = value; }
        }
        public Double Right
        {
            get { return max[0]; }
            set { max[0] = value; }
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
        public void SetExtents(double l, double b, double r, double t)
        {
            set(l, b, r, t);
        }

        /// <summary>
        /// 通过一个外边界矩形来设置当前地理范围
        /// </summary>
        public void SetExtents(GeoBound bound)
        {
            set(bound.min, bound.max);
        }

        /// <summary>
        /// Initialize an <c>Envelope</c> for a region defined by maximum and minimum values.
        /// </summary>
        public void Init(double x1, double x2, double y1, double y2)
        {
            if (x1 < x2)
            {
                Left = x1;
                Right = x2;
            }
            else
            {
                Left = x2;
                Right = x1;
            }

            if (y1 < y2)
            {
                Bottom = y1;
                Top = y2;
            }
            else
            {
                Bottom = y2;
                Top = y1;
            }
        }

        /// <summary>
        /// 判断两个矩形是否相交
        /// </summary>
        public bool IsIntersectWith(GeoBound r)
        {
            for (int i = 0; i < 2; i++)
            {
                if (max[i] < r.min[i] || min[i] > r.max[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsPointIn(GeoPoint pt)
        {
            return !(pt.X + Geometry.EPSIONAL < Left || pt.X - Geometry.EPSIONAL > Right || pt.Y + Geometry.EPSIONAL < Bottom || pt.Y - Geometry.EPSIONAL > Top);
        }

        public bool Contains(GeoBound r)
        {
            for (int i = 0; i < 2; i++)
            {
                if (max[i] + Geometry.EPSIONAL < r.max[i] || min[i] - Geometry.EPSIONAL > r.min[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal bool edgeOverlaps(GeoBound r)//完全重叠，汪2017-8-10
        {
            for (int i = 0; i < 2; i++)
            {
                if (!(Math.Abs(min[i] - r.min[i]) < Geometry.EPSIONAL || Math.Abs(max[i] - r.max[i]) < Geometry.EPSIONAL))
                {
                    return false;
                }
            }
            return true;
        }
        //internal bool edgeOverlaps(GeoBound r)//原有
        //{
        //    for (int i = 0; i < 2; i++)
        //    {
        //        if (Math.Abs(min[i] - r.min[i]) < Geometry.EPSIONAL || Math.Abs(max[i] - r.max[i]) < Geometry.EPSIONAL)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        internal double area()
        {
            return (max[0] - min[0]) * (max[1] - min[1]);
        }

        internal double enlargement(GeoBound r)
        {
            double enlargedArea = (Math.Max(max[0], r.max[0]) - Math.Min(min[0], r.min[0])) *
                                 (Math.Max(max[1], r.max[1]) - Math.Min(min[1], r.min[1]));

            return enlargedArea - area();
        }

        internal double distance(GeoPoint p)
        {
            double distanceSquared = 0;
            for (int i = 0; i < 2; i++)
            {
                double greatestMin = Math.Max(min[i], p.coordinates[i]);
                double leastMax = Math.Min(max[i], p.coordinates[i]);
                if (greatestMin > leastMax)
                {
                    distanceSquared += ((greatestMin - leastMax) * (greatestMin - leastMax));
                }
            }
            return Math.Sqrt(distanceSquared);
        }

        internal double distance(GeoBound r)
        {
            double distanceSquared = 0;
            for (int i = 0; i < 2; i++)
            {
                double greatestMin = Math.Max(min[i], r.min[i]);
                double leastMax = Math.Min(max[i], r.max[i]);
                if (greatestMin > leastMax)
                {
                    distanceSquared += ((greatestMin - leastMax) * (greatestMin - leastMax));
                }
            }
            return Math.Sqrt(distanceSquared);
        }

        /// <summary>
        /// 矩形合并，不创建新的矩形。
        /// </summary>
        /// <param name="bound">被合并的矩形</param> 
        public void UnionBound(GeoBound bound)
        {
            if (bound == null)
                return;

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

        public GeoBound Join(GeoBound box)
        {
            if (box == null)
                return this.Clone();
            else
                return new GeoBound(Math.Min(this.LeftBottomPt.X, box.LeftBottomPt.X), Math.Min(this.LeftBottomPt.Y, box.LeftBottomPt.Y),
                                       Math.Max(this.RightUpPt.X, box.RightUpPt.X), Math.Max(this.RightUpPt.Y, box.RightUpPt.Y));
        }

        /// <summary>
        /// 合并点，不创建新的矩形
        /// </summary>
        public void UnionPoint(GeoPoint pt)
        {
            double x = pt.X;
            double y = pt.Y;
            if (x < Left) Left = x;
            if (x > Right) Right = x;
            if (y < Bottom) Bottom = y;
            if (y > Top) Top = y;
            return;
        }

        /// <summary>
        /// 计算边界矩形集的 边界矩形
        /// </summary>
        public static GeoBound UnionBounds(GeoBound[] bounds)
        {
            if (bounds == null || bounds.Length == 0)
                return null;
            GeoBound bound = bounds[0].Clone();
            for (int i = 1; i < bounds.Length; ++i)
            {
                bound.UnionBound(bounds[i]);
            }
            return bound;
        }

        /// <summary>
        /// 创建当前矩形的Copy
        /// </summary>
        public GeoBound Clone()
        {
            return new GeoBound(Left, Bottom, Right, Top);
        }

        /// <summary>
        /// 返回中心点，调用了点的操作运算重载
        /// </summary>
        public GeoPoint GetCentroid()
        {
            return (LeftBottomPt + RightUpPt) * .5f;
        }

        public uint LongestAxis
        {
            get
            {
                GIS.Geometries.GeoPoint boxdim = this.RightUpPt - this.LeftBottomPt;
                uint la = 0;
                double lav = 0; // 长轴
                for (uint ii = 0; ii < 2; ii++)
                {
                    if (boxdim[ii] > lav)
                    {
                        la = ii;
                        lav = boxdim[ii];
                    }
                }
                return la;
            }
        }

        /// <summary>
        /// 检测边界矩形的端点坐标是否正确
        /// </summary>
        //public bool CheckMinMax()
        //{
        //    bool bSwapped = false;
        //    if (m_LeftBottomPt.X > m_RightUpPt.X)
        //    {
        //        double tmp = m_LeftBottomPt.X;
        //        m_LeftBottomPt.X = m_RightUpPt.X;
        //        m_RightUpPt.X = tmp;
        //        bSwapped = true;
        //    }
        //    if (m_LeftBottomPt.Y > m_RightUpPt.Y)
        //    {
        //        double tmp = m_RightUpPt.Y;
        //        m_LeftBottomPt.Y = m_RightUpPt.Y;
        //        m_RightUpPt.Y = tmp;
        //        bSwapped = true;
        //    }
        //    return bSwapped;
        //}

        //GeoBound对象膨胀，左下角和右上角分别膨胀dx,dy，结果保存到CMapBound对象
        public GeoBound InflateBound(double dx, double dy)
        {
            return new GeoBound(Left - dx, Bottom - dy, Right + dx, Top + dy);
        }

        //CMapBound对象收缩，左下角和右上角分别收缩dx,dy，结果保存到CMapBound对象
        public GeoBound DeflateBound(double dx, double dy)
        {
            return new GeoBound(Left + dx, Bottom + dy, Right - dx, Top - dy);
        }

        //判断两个矩形是否相等
        public bool isEqual(GeoBound bound)
        {
            if (bound == null)
                return false;
            return LeftBottomPt.IsEqual(bound.LeftBottomPt) && RightUpPt.IsEqual(bound.RightUpPt);
        }
        #endregion

        #region 打印box，zh修改，2018年1月15日
        public string toString()
        {
            return Left + "," + Bottom + "," + Right + "," + Top;
        }
        #endregion
    }
}
