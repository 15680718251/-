using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    public class GeoPolygon:GeoSurface
    {
        public GeoPolygon(GeoLinearRing exteriorRing, List<GeoLinearRing> interiorRings)
        {
            m_ExteriorRing = exteriorRing;
            m_InteriorRings = interiorRings;
        }
        public GeoPolygon(GeoLinearRing exteriorRing) : this(exteriorRing, new List<GeoLinearRing>())
        {
        }
        public GeoPolygon(): this(new GeoLinearRing(), new List<GeoLinearRing>())
        {
        }
 
        private GeoLinearRing m_ExteriorRing;        // Polygon的外边届 
        private List<GeoLinearRing> m_InteriorRings; //Polygon的内边界

        public override bool IsEmpty()
        {
            return (ExteriorRing == null) || (ExteriorRing.Vertices.Count == 0);
        }

        #region Properties
        public GeoLinearRing ExteriorRing
        {
            get { return m_ExteriorRing; }
            set { m_ExteriorRing = value; }
        }

        public List<GeoLinearRing> InteriorRings
        {
            get { return m_InteriorRings; }
            set { m_InteriorRings = value; }
        }
        public override double Area
        {
            get
            {
                double area = 0.0;
                area += ExteriorRing.Area;
                bool extIsClockwise = ExteriorRing.IsCCW();
                for (int i = 0; i < InteriorRings.Count; i++)
                    //opposite direction of exterior subtracts area
                    if (InteriorRings[i].IsCCW() != extIsClockwise)
                        area -= InteriorRings[i].Area;
                    else
                        area += InteriorRings[i].Area;
                return area;
            }
        }

        /// <summary>
        /// 返回多边形所有环的顶点总数。
        /// </summary>
        public int NumPoints
        {
            get
            {
                int nPointsCount = ExteriorRing.NumPoints;
                int nPartsNum = InteriorRings.Count;
                for (int i = 0; i < nPartsNum; ++i)
                {
                    nPointsCount += InteriorRings[i].NumPoints;
                }
                return nPointsCount;
            }
        }
        #endregion
        
        #region Function
        public override double Length
        {
            get
            {
                return ExteriorRing.Length;
            }
        }
        public override GeoBound GetBoundingBox()
        {
            if (m_ExteriorRing == null || m_ExteriorRing.Vertices.Count == 0)
                return null;

            GeoBound bound = m_ExteriorRing.GetBoundingBox();
            m_ExteriorRing.Bound = m_ExteriorRing.GetBoundingBox();           
            for (int i = 0; i < InteriorRings.Count; i++)
            {
                InteriorRings[i].Bound = InteriorRings[i].GetBoundingBox();
            }
            return bound;
        }
       

        public override bool IsSimple()
        {
            throw new NotImplementedException();
        }

        public override Geometry Clone()
        {
            GeoPolygon plg = new GeoPolygon();
            plg.ExteriorRing = m_ExteriorRing.Clone() as GeoLinearRing;
            for (int i = 0; i < InteriorRings.Count; ++i)
            {
                plg.InteriorRings.Add(m_InteriorRings[i].Clone() as GeoLinearRing);
            }
            return plg;
        }
        public override bool IsSelectByPt(GeoPoint pt)
        {
            if (GIS.SpatialRelation.GeoAlgorithm.IsInLinearRing(pt, this.ExteriorRing) > 0)
            {
                for (int i = 0; i < m_InteriorRings.Count; i++)
                {
                    if (GIS.SpatialRelation.GeoAlgorithm.IsInLinearRing(pt, m_InteriorRings[i]) > 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override GeoPoint MouseCatchPt(GeoPoint pt, MouseCatchType type )
        {            
            GeoPoint ptExt = m_ExteriorRing.MouseCatchPt(pt,type );
            if (ptExt != null)
                return ptExt;
            for (int i = 0; i < m_InteriorRings.Count; i++)
            {
                GeoPoint SnapPt = m_InteriorRings[i].MouseCatchPt(pt,type );
                if (SnapPt != null)
                { 
                    return SnapPt;
                }
            }
            return null;
        }
        public override void Move(double deltaX, double deltaY)
        {
            m_ExteriorRing.Move(deltaX, deltaY);
            for (int i = 0; i < InteriorRings.Count; i++)
            {
                InteriorRings[i].Move(deltaX, deltaY);
            }
        }
        public override void RotateAt(double angle, GeoPoint basePt)
        {
            m_ExteriorRing.RotateAt(angle, basePt);
            for (int i = 0; i < InteriorRings.Count; i++)
            {
                InteriorRings[i].RotateAt(angle, basePt);
            }
        }
        public override void SymmetryWithLine(GeoPoint ptStart, GeoPoint ptEnd)
        {
            m_ExteriorRing.SymmetryWithLine(ptStart, ptEnd);
            for (int i = 0; i < InteriorRings.Count; i++)
            {
                InteriorRings[i].SymmetryWithLine(ptStart, ptEnd);
            }
        }
        public override bool RemoveVertex(GeoPoint pt)
        {
            bool bok = false;
            if(m_ExteriorRing.RemoveVertex(pt))
                bok = true;
            for (int i = 0; i < InteriorRings.Count; i++)
            {
                if (InteriorRings[i].RemoveVertex(pt))
                    bok = true;
            }
            return bok;
        }
        public override void WriteToLQFile(System.IO.StreamWriter sw)
        {
            sw.WriteLine("GeoPolygon");
            int nPart = this.InteriorRings.Count + 1;
            sw.WriteLine(nPart);
            sw.WriteLine(ExteriorRing.NumPoints);//输出外边界点数
            for (int i = 0; i < ExteriorRing.NumPoints; i++)
            {
                sw.WriteLine("{0:f5},{1:f5}", ExteriorRing.Vertices[i].X, ExteriorRing.Vertices[i].Y);
            }

            for (int i = 0; i < nPart - 1; i++)
            {
                GeoLinearRing ring = m_InteriorRings[i];
                sw.WriteLine(ring.NumPoints);
                for (int j = 0; j < ring.NumPoints; j++)
                {
                    sw.WriteLine("{0:f5},{1:f5}", ring.Vertices[j].X, ring.Vertices[j].Y);
                }
            }
        }
        public override void ReadFromLQFile(System.IO.StreamReader sr)
        {
            string temp = sr.ReadLine();
           
            int nPart = int.Parse(temp.Trim());

            for (int i = 0; i < nPart; i++)
            {
                GeoLinearRing ring = new GeoLinearRing();
                ring.ReadFromLQFile(sr);
                if (i == 0)
                {
                    m_ExteriorRing = ring;
                }
                else
                    InteriorRings.Add(ring); 
            }
        }
        public override void WriteGeoInfo(System.IO.StreamWriter sw)
        {
            WriteToLQFile(sw);
        }
        public override bool IsEqual(Geometry geom)
        {
            GeoPolygon plg = geom as GeoPolygon;
            if (plg == null) return false;
            bool equal = plg.ExteriorRing.IsEqual(ExteriorRing);
            if (!equal)
                return false;
            for (int i = 0; i < InteriorRings.Count; i++)
            {
                if (!InteriorRings[i].IsEqual(plg.InteriorRings[i]))
                    return false;
            }
            return true;
        }
        #endregion

    }
}
