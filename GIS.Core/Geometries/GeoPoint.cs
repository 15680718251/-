using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
namespace GIS.Geometries
{

    /// <summary>
    /// Point是个0维的几何类，在空间中只有一个位置。
    /// 一个点具有X,Y坐标
    /// </summary>
    [Serializable]
    public class GeoPoint : Geometry, IComparable<GeoPoint>
    {
        public GeoPoint(): this(0, 0)
        {
            _IsEmpty = true;
        }
        public GeoPoint(double x, double y)
        {
            coordinates = new double[2];
            coordinates[0] = x;
            coordinates[1] = y;
        }
        public GeoPoint(double[] m) : this(m[0], m[1]) { }

        //private double m_X;
        //private double m_Y;
        internal double[] coordinates;
        private bool _IsEmpty = false;
        protected bool SetIsEmpty
        {
            set { _IsEmpty = value; }
        }

        public double X
        {
            get { return coordinates[0]; }
            set { coordinates[0] = value; _IsEmpty = false; }
        }

        /// <summary>
        /// Gets or sets the Y coordinate of the point
        /// </summary>
        public double Y
        {
            get { return coordinates[1]; }
            set { coordinates[1] = value; _IsEmpty = false; }
        }

        #region Function
        /// <summary>
        /// 设置点的XY坐标
        /// </summary>
        public void SetXY(double x, double y)
        {
            coordinates[0] = x;
            coordinates[1] = y;
            _IsEmpty = false;
        }

        public GeoBound FromPoint(double d)
        {
            GeoBound bound = new GeoBound();
            bound.set(this.X - d, this.Y - d, this.X + d, this.Y + d);
            return bound;
        }

        /// <summary>
        /// 判断两点是否相等
        /// </summary>
        public override bool IsEqual(Geometry geom)
        {
            GeoPoint pt = geom as GeoPoint;
            return pt != null && Math.Abs(pt.X - X) < Geometry.EPSIONAL && Math.Abs(pt.Y - Y) < Geometry.EPSIONAL;
        }

        public virtual int NumOrdinates
        {
            get { return 2; }
        }

        public override bool IsEmpty()
        {
            return _IsEmpty;
        }

        /// <summary>
        /// 两点距离
        /// </summary>
        public double DistanceTo(GeoPoint pt)
        {
            return Math.Sqrt(Math.Pow(pt.X - X, 2) + Math.Pow(pt.Y - Y, 2));
        }
        /// <summary>
        /// 返回一个边界矩形。
        /// </summary>
        public override GeoBound GetBoundingBox()
        {
            return new GeoBound(coordinates[0], coordinates[1], coordinates[0], coordinates[1]);
        }
        public override bool IsSimple()
        {
            return true;
        }
        public override bool IsSelectByPt(GIS.Geometries.GeoPoint pt)
        {
            return GIS.SpatialRelation.GeoAlgorithm.IsOnPointCoarse(this, pt);
        }
        public override GeoPoint MouseCatchPt(GeoPoint pt, MouseCatchType type)
        {
            if (GIS.SpatialRelation.GeoAlgorithm.IsOnPointCoarse(this, pt))
                return this;
            else
                return null;
        }
        public override Geometry Clone()
        {
            return new GeoPoint(X, Y);
        }
        public override void Move(double deltaX, double deltaY)
        {
            coordinates[0] += deltaX;
            coordinates[1] += deltaY;
        }
        public override void RotateAt(double angle, GeoPoint basePt)
        {
            GeoPoint pt = SpatialRelation.GeoAlgorithm.PointRotate(this, basePt, angle);
            X = pt.X;
            Y = pt.Y;
        }
        public override void SymmetryWithLine(GeoPoint ptStart, GeoPoint ptEnd)
        {
            GeoPoint pt = SpatialRelation.GeoAlgorithm.SymmetryPtOfLine(this, ptStart, ptEnd);
            X = pt.X;
            Y = pt.Y;
        }
        public override void WriteGeoInfo(System.IO.StreamWriter sw)
        {
            sw.WriteLine("GeoPoint");
            sw.WriteLine("{0:f5},{1:f5}", coordinates[0], coordinates[1]);
        }
        public override void WriteToLQFile(System.IO.StreamWriter sw)
        {
            sw.WriteLine("GeoPoint");
            sw.WriteLine("{0:f5},{1:f5}", coordinates[0], coordinates[1]);
        }
        public override void ReadFromLQFile(System.IO.StreamReader sr)
        {
            try
            {
                string temp = sr.ReadLine();
                string[] strArray = temp.Split(',');
                coordinates[0] = double.Parse(strArray[0].Trim());
                coordinates[1] = double.Parse(strArray[1].Trim());
            }
            catch
            {
                string temp = sr.ReadLine();
            }
        }

        /// <summary>
        /// Returns part of coordinate. Index 0 = X, Index 1 = Y
        /// </summary>
        public double this[uint index]
        {
            get
            {
                if (index == 0)
                    return this.X;
                else if (index == 1)
                    return this.Y;
                else
                    throw new ApplicationException("点为空");
            }
            set
            {
                if (index == 0)
                    this.X = value;
                else if (index == 1)
                    this.Y = value;
            }
        }

        #endregion

        #region "IComparable 接口，实现点的排序"
        /// <summary>
        /// 实现IComparable接口，返回与other点的比较结果
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns> 0 为相等， 1为大于， -1为小于
        public virtual int CompareTo(GeoPoint other)
        {
            if (X < other.X || X == other.X && Y < other.Y)
                return -1;
            else if (X > other.X || X == other.X && Y > other.Y)
                return 1;
            else
                return 0;
        }

        #endregion


        #region "操作运算符重载"

        public static GeoPoint operator -(GeoPoint pt1, GeoPoint pt2)
        {
            return new GeoPoint(pt1.X - pt2.X, pt1.Y - pt2.Y);
        }
        public static GeoPoint operator +(GeoPoint pt1, GeoPoint pt2)
        {
            return new GeoPoint(pt1.X + pt2.X, pt1.Y + pt2.Y);
        }
        public static GeoPoint operator *(GeoPoint pt, double d)
        {
            return new GeoPoint(pt.X * d, pt.Y * d);
        }
        #endregion
    }
}
