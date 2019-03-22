using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    /// <summary>
    /// GeometryCollection是一个几何体的集合类，可以包含若干个几何类
    /// </summary>
    public class GeometryCollection : Geometry
    {
        private List<Geometry> m_Geometrys;

        public GeometryCollection()
        {
            m_Geometrys = new List<Geometry>();
        }

        public virtual Geometry this[int index]
        {
            get { return m_Geometrys[index]; }
        }

        public override bool IsEmpty()
        {
            if (m_Geometrys == null)
                return true;
            for (int i = 0; i < m_Geometrys.Count; i++)
                if (!m_Geometrys[i].IsEmpty())
                    return false;
            return true;
        }

        public override GeoBound GetBoundingBox()
        {
            if (this.m_Geometrys.Count == 0)
                return null;
            GeoBound bbox = m_Geometrys[0].GetBoundingBox();
            for (int i = 1; i < m_Geometrys.Count; i++)
                bbox.UnionBound(m_Geometrys[i].GetBoundingBox());
            return bbox;

        }
        public override bool IsSimple()
        {
            throw new NotImplementedException();
        }

        public virtual List<Geometry> Collection
        {
            get { return m_Geometrys; }
            set { m_Geometrys = value; }
        }
        public virtual int NumGeometries
        {
            get { return m_Geometrys.Count; }
        }
        public override Geometry Clone()
        {
            throw new ApplicationException("没有实现抽象几何集的CLONE定义");
        }

        public override bool IsSelectByPt(GeoPoint pt)
        {
            throw new NotImplementedException();
        }

        public override GeoPoint MouseCatchPt(GeoPoint pt, MouseCatchType type)
        {
            throw new NotImplementedException();
        }

        public override void WriteGeoInfo(System.IO.StreamWriter sw)
        {
            throw new NotImplementedException();
        }

        public override void WriteToLQFile(System.IO.StreamWriter sw)
        {
            throw new NotImplementedException();
        }

        public override void Move(double deltaX, double deltaY)
        {
            throw new NotImplementedException();
        }

        public override void RotateAt(double angle, GeoPoint basePt)
        {
            throw new NotImplementedException();
        }

        public override void SymmetryWithLine(GeoPoint ptStart, GeoPoint ptEnd)
        {
            throw new NotImplementedException();
        }

        public override bool IsEqual(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public override void ReadFromLQFile(System.IO.StreamReader sr)
        {
            throw new NotImplementedException();
        }
    }
}
