using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    public class GeoMultiPoint:GeometryCollection
    {
        private List<GeoPoint> m_Points;

        public List<GeoPoint> Points
        {
            get { return m_Points; }
            set { m_Points = value; }
        }
        public GeoMultiPoint()
        {
            m_Points = new List<GeoPoint>();
        }
        public override int NumGeometries
        {
            get
            {
                return m_Points.Count;
            }
        }
        public new GeoPoint this[int index]
        {
            get { return m_Points[index]; }
        }

        public override bool IsEmpty()
        {
            return (m_Points != null && m_Points.Count == 0);
        }

        public override GeoBound GetBoundingBox()
        {
            if (Points == null || Points.Count == 0)
                return null;
            GeoBound bound = new GeoBound(Points[0], Points[0] );
            Points[0].Bound = Points[0].GetBoundingBox();
            for (int i = 1; i < Points.Count; ++i)
            {
                bound.UnionPoint(Points[i]);
                Points[i].Bound = Points[i].GetBoundingBox();
            }
            return bound;
        }
        public override Geometry Clone()
        {
            GeoMultiPoint geoms = new GeoMultiPoint();
            for (int i = 0; i < Points.Count; i++)
                geoms.Points.Add((GeoPoint)Points[i].Clone());
            return geoms;
        }

        public override bool IsSelectByPt(GeoPoint pt)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                if (m_Points[i].IsSelectByPt(pt))
                {
                    return true;
                }
            }
            return false;
        }
        public override void Move(double deltaX, double deltaY)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                Points[i].Move(deltaX, deltaY);
            }
        }
        public override void RotateAt(double angle, GeoPoint basePt)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                Points[i].RotateAt(angle, basePt);
            }
        }
        public override void SymmetryWithLine(GeoPoint ptStart, GeoPoint ptEnd)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                Points[i].SymmetryWithLine(ptStart, ptEnd);
            }
        }
        public override GeoPoint MouseCatchPt(GeoPoint pt, MouseCatchType type )
        { 
            for (int i = 0; i < NumGeometries; i++)
            {
                GeoPoint SnapPt = m_Points[i].MouseCatchPt(pt,type );
                if (SnapPt != null)
                { 
                    return SnapPt;
                }
            }
            return null;
        }
        public override void ReadFromLQFile(System.IO.StreamReader sr)
        {
            string temp = sr.ReadLine(); 
         
            int nPointCount = int.Parse(temp.Trim());
            for (int i = 0; i < nPointCount; i++)
            {
                GeoPoint pt = new GeoPoint();
                pt.ReadFromLQFile(sr);
                Points.Add(pt); 
            }
            
        }
        public override void WriteToLQFile(System.IO.StreamWriter sw)
        {
            sw.WriteLine("GeoMultiPoint");
            sw.WriteLine(NumGeometries);//代表点个数
            for (int i = 0; i < NumGeometries; i++)
            {
                GeoPoint pt = this.Points[i];
                sw.WriteLine("{0:f5},{1:f5}", pt.X, pt.Y);
            }
        }
        public override void WriteGeoInfo(System.IO.StreamWriter sw)
        {
            WriteToLQFile(sw);
        }
        public override bool IsEqual(GIS.Geometries.Geometry geom)
        {
            GeoMultiPoint pts = geom as GeoMultiPoint;
            if (pts == null) return false;
            for (int i = 0; i < pts.NumGeometries; i++)
            {
                if (!pts[i].IsEqual(Points[i]))
                    return false;
            }
            return true;
        }
    }
}
