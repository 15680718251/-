using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    public class GeoMultiPolygon:GeoMultiSurface
    {

        public GeoMultiPolygon()
        {
            m_Polygons = new List<GeoPolygon>();
        }

        private List<GeoPolygon> m_Polygons;

        public List<GeoPolygon> Polygons
        {
            get { return m_Polygons; }
            set { m_Polygons = value; }
        }
        public override double Area
        {
            get
            {
                double area = 0;
                for (int i = 0; i < NumGeometries; i++)
                    area += m_Polygons[i].Area;
                return area;
            }
        }
        public override double Length
        {
            get
            {
                double length = 0;
                for (int i = 0; i < NumGeometries; i++)
                    length += m_Polygons[i].Length;
                return length;
            }
        }
        public override int NumGeometries
        {
            get
            {
                return m_Polygons.Count;
            }
        }

        public override bool IsEmpty()
        {
            if (m_Polygons == null || m_Polygons.Count == 0)
                return true;
            for (int i = 0; i < m_Polygons.Count; i++)
                if (!m_Polygons[i].IsEmpty())
                    return false;
            return true;
        }

        public override bool IsEqual(GIS.Geometries.Geometry geom)
        {
            GeoMultiPolygon plgs = geom as GeoMultiPolygon;
            if (plgs == null) return false;
            for (int i = 0; i < plgs.NumGeometries; i++)
            {
                if (!plgs[i].IsEqual(Polygons[i]))
                    return false;
            }
            return true;
        }
        #region Function

        public new GeoPolygon this[int index]
        {
            get { return Polygons[index]; }
        }
        public override GeoBound GetBoundingBox()
        {
            if (Polygons == null || Polygons.Count == 0)
                return null;
            GeoBound bound = Polygons[0].GetBoundingBox();
            for (int i = 1; i < Polygons.Count; ++i)
                bound.UnionBound(Polygons[i].GetBoundingBox());
            return bound;
        }
        public override Geometry Clone()
        {
            GeoMultiPolygon geoms = new GeoMultiPolygon();
            for (int i = 0; i < Polygons.Count; i++)
            {
                geoms.Polygons.Add((GeoPolygon)Polygons[i].Clone());
            }
            return geoms;

        }
        public override bool IsSelectByPt(GeoPoint pt)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                if (m_Polygons[i].IsSelectByPt(pt))
                {
                    return true;
                }
            }
            return false;
        }
        public override GeoPoint MouseCatchPt(GeoPoint pt, MouseCatchType type )
        { 
            for (int i = 0; i < NumGeometries; i++)
            {
                GeoPoint SnapPt = m_Polygons[i].MouseCatchPt(pt,type );
                if (SnapPt != null)
                { 
                    return SnapPt;
                }
            }
            return null;
        }
        public override bool  RemoveVertex(GeoPoint pt)
        {
            bool bok = false;
            for (int i = 0; i < NumGeometries; i++)
            {
                if (m_Polygons[i].RemoveVertex(pt))
                    bok = true;
            }
            return bok;
        }
        public override void WriteGeoInfo(System.IO.StreamWriter sw)
        {
            WriteToLQFile(sw);
        }
        public override void ReadFromLQFile(System.IO.StreamReader sr)
        {
            string strTemp = sr.ReadLine(); 
            int plgParts = int.Parse(strTemp);
            for (int i = 0; i < plgParts; i++)
            {
                sr.ReadLine(); 
                GeoPolygon plg = new GeoPolygon();
                plg.ReadFromLQFile(sr);
                Polygons.Add(plg);
            }
        }
        public override void WriteToLQFile(System.IO.StreamWriter sw)
        {           
            sw.WriteLine("GeoMultiPolygon");
            sw.WriteLine(NumGeometries);//代表点个数
            for (int i = 0; i < NumGeometries; i++)
            {
                GeoPolygon plg = Polygons[i];
                plg.WriteGeoInfo(sw);
            }
        }
        public override void Move(double deltaX, double deltaY)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                Polygons[i].Move(deltaX, deltaY);
            }
        }
        public override void RotateAt(double angle, GeoPoint basePt)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                Polygons[i].RotateAt(angle, basePt);
            }
        }
        public override void SymmetryWithLine(GeoPoint ptStart, GeoPoint ptEnd)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                Polygons[i].SymmetryWithLine(ptStart,ptEnd);
            }
        }
        #endregion
    }
}
