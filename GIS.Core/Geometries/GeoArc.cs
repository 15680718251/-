using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using GIS.SpatialRelation;
using GIS.Geometries;
using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    public class GeoArc:GeoLineString
    {
        public GeoArc()
            : base()
        {
            m_PtSkeleton = new List<GeoPoint>();
        }
        public GeoArc(List<GeoPoint> pts,ArcType type)
        {
            if (pts.Count != 3)
                throw new Exception("不是三个点构成弧");
            m_Type = type;
            m_PtSkeleton = new List<GeoPoint>();
            m_PtSkeleton.AddRange( pts);
            Interpolation();
        }


        private List<GeoPoint> m_PtSkeleton;
        private ArcType m_Type;

        public ArcType ArcType
        {
            get { return m_Type; }
            set { m_Type = value; }
        }
        public List<GeoPoint> SkeletonPtList
        {
            get { return m_PtSkeleton; }
            set { m_PtSkeleton = value; }
        }

        public void Interpolation()
        {
            if (m_PtSkeleton.Count!=3)
                return;
            m_pts = GeoAlgorithm.ThreePointsArc(m_PtSkeleton[0],m_PtSkeleton[1],m_PtSkeleton[2],m_Type);
        }
        public override void AddPoint(GeoPoint pt)
        {
            if (m_PtSkeleton.Count < 3)
            {
                m_PtSkeleton.Add(pt);
                if (m_PtSkeleton.Count == 3)
                    Interpolation();
                
            }
        }
        public override Geometry Clone()
        {
            GeoArc arc = new GeoArc();
             
            for (int i = 0; i < SkeletonPtList.Count; i++)
            {
                arc.SkeletonPtList.Add(SkeletonPtList[i].Clone() as GeoPoint);
            }
            for (int i = 0; i < Vertices.Count; ++i)
            {
                arc.Vertices.Add((GeoPoint)Vertices[i].Clone());
            }
            return arc;
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
            sw.WriteLine("GeoArc");
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
