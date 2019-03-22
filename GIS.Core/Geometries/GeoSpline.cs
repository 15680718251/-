using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.SpatialRelation;
using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    public class GeoSpline:GeoLineString
    {
        public GeoSpline():base()
        {
            m_PtSkeleton = new List<GeoPoint>();
        }

        public GeoSpline(List<GeoPoint> pts)
        {
            m_PtSkeleton = new List<GeoPoint>();
            m_PtSkeleton.AddRange( pts);
            Interpolation();
        }

        private List<GeoPoint> m_PtSkeleton;

        public List<GeoPoint> SkeletonPtList
        {
            get { return m_PtSkeleton; }
            set { m_PtSkeleton = value; }
        }
        public void Interpolation()
        {
            if (m_PtSkeleton.Count < 3)
                return;
            m_pts = GeoAlgorithm.CubicSpline(m_PtSkeleton);
        }
        public override void AddPoint(GeoPoint pt)
        {
            m_PtSkeleton.Add(pt);
            if (m_PtSkeleton.Count < 3)
                return;
            m_pts = GeoAlgorithm.CubicSpline(m_PtSkeleton);
        }
        public override Geometry Clone()
        {
            GeoSpline spline = new GeoSpline();

            for (int i = 0; i < SkeletonPtList.Count; i++)
            {
                spline.SkeletonPtList.Add(SkeletonPtList[i].Clone() as GeoPoint);
            }
            for (int i = 0; i < Vertices.Count; ++i)
            {
                spline.Vertices.Add((GeoPoint)Vertices[i].Clone());
            }
            return spline;    
      

        }
        public override void ReadFromLQFile(System.IO.StreamReader sr)
        {
            string strTemp = sr.ReadLine();
            int nPtSkeletonCount = int.Parse(strTemp);
            for (int i = 0; i < nPtSkeletonCount; i++)
            {
                GeoPoint pt = new GeoPoint();
                pt.ReadFromLQFile(sr);
                m_PtSkeleton.Add(pt);
            }
            strTemp = sr.ReadLine();
            int nPtCount = int.Parse(strTemp);
            for (int i = 0; i < nPtCount; i++)
            {
                GeoPoint pt = new GeoPoint();
                pt.ReadFromLQFile(sr);
                m_pts.Add(pt);
            }
        }
        public override void WriteToLQFile(System.IO.StreamWriter sw)
        {
            sw.WriteLine("GeoSpline");
            sw.WriteLine(m_PtSkeleton.Count);
            for (int i = 0; i < m_PtSkeleton.Count; i++)
            {
                sw.WriteLine("{0:f5},{1:f5}", m_PtSkeleton[i].X, m_PtSkeleton[i].Y);
            }
            sw.WriteLine(m_pts.Count);
            for (int i = 0; i < m_pts.Count; i++)
            {
                sw.WriteLine("{0:f5},{1:f5}", m_pts[i].X, m_pts[i].Y);
            }
        }
    }
}
