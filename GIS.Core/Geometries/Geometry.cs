using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    /// <summary>
    /// 所有几何类的基类，是对所有几何的抽象
    /// 是个虚类，不可实例化
    /// </summary>
    public abstract class Geometry   
    {

        #region EPSIONAL
        //用于double计算的判断
        public static double m_Epsional = 0.005;//原为0.00000001，由于误差原因，多边形判断点在线上，顶点重合等问题时，不够准确，汪2017-8-8修改
                                                //并在多处比较时考虑误差即   + - Geometry.EPSIONAL
        public static double EPSIONAL
        {
            get { return m_Epsional; }
        } 
        #endregion
        /// <summary>
        /// 获得当前几何对象的边界矩形
        /// </summary>
        /// <returns></returns>

        #region PrivateMember
        private GeoBound m_Bound = null; 
        #endregion
        #region Properties

        public GeoBound Bound
        {
            get
            {
                if (m_Bound == null)
                    m_Bound = GetBoundingBox();
                return m_Bound;
            }
            set
            {
                m_Bound = value;
            }
        }
        
        #endregion
        #region Virtual Function

        public abstract GeoBound GetBoundingBox();
        /// <summary>
        /// 该方法返回几何对象的COPY，派生类必须使用" public new [derived_data_type] Clone()"
        /// </summary>
        /// <returns></returns>
        public abstract Geometry Clone();
        public virtual double Length
        {
            get
            {
                return 0;
            }
        }
        public virtual double Area
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        ///  Returns 'true' if this Geometry has no anomalous geometric points, such as self
        /// intersection or self tangency. The description of each instantiable geometric class will include the specific
        /// conditions that cause an instance of that class to be classified as not simple.
        /// </summary>
        public abstract bool IsSimple();
        public abstract bool IsEmpty();
        public abstract bool IsSelectByPt(GeoPoint pt);
        public abstract GeoPoint MouseCatchPt(GeoPoint pt,MouseCatchType type);
        public abstract void WriteGeoInfo(System.IO.StreamWriter sw);
        public abstract void WriteToLQFile(System.IO.StreamWriter sw);
        public abstract void Move(double deltaX, double deltaY);
        public abstract void RotateAt(double angle, GeoPoint basePt);
        public abstract void SymmetryWithLine(GeoPoint ptStart, GeoPoint ptEnd);
        public virtual bool RemoveVertex(GeoPoint pt)
        {
            return false;
        }
        #endregion

        public abstract bool IsEqual(Geometry geom);

        public abstract void ReadFromLQFile(System.IO.StreamReader sr);
       
    }
}
