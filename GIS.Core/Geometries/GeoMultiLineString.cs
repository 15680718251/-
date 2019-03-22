using System;
using System.Collections.Generic;
using System.Text;

using System.Collections;

using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    /// <summary>
    /// MultiLineString是一个存放若干个LineString的MultiCurve
    /// </summary>
    public class GeoMultiLineString:GeoMultiCurve
    {
        /// <summary>
        /// 折线集合
        /// </summary>
        private List<GeoLineString> m_LineStrings;

        public GeoMultiLineString()
        {
            m_LineStrings = new List<GeoLineString>();
        }

        public new GeoLineString this[int index]
        {
            get 
            {
                if (index >= 0 && index < m_LineStrings.Count)
                    return m_LineStrings[index];
                else
                    return null;
            }
        }
        public List<GeoLineString> LineStrings
        {
            get { return m_LineStrings; }
            set { m_LineStrings = value; }
        }
        public override bool IsEqual(GIS.Geometries.Geometry geom)
        {
            GeoMultiLineString lines = geom as GeoMultiLineString;
            if (lines == null) return false;
            for (int i = 0; i < lines.NumGeometries; i++)
            {
                if (!lines[i].IsEqual(LineStrings[i]))
                    return false;
            }
            return true;
        }

        public override bool IsEmpty()
        {
            if (m_LineStrings == null || m_LineStrings.Count == 0)
                return true;
            for (int i = 0; i < m_LineStrings.Count; i++)
                if (!m_LineStrings[i].IsEmpty())
                    return false;
            return true;
        }

        #region Properties
        public override int NumGeometries
        {
            get
            {
                return m_LineStrings.Count;
            }
        }
        public override double Length
        {
            get
            {
                double len = 0.0;
                for (int i = 0; i < LineStrings.Count; i++)
                    len += LineStrings[i].Length;
                return len;
            }
        } 
        #endregion

        #region Function
        public List<GeoLineString> Bomb()
        {
            List<GeoLineString> m_list = new List<GeoLineString>();
            for (int i = 0; i < NumGeometries; i++)
            {
                List<GeoLineString> lines = m_LineStrings[i].Bomb();
                m_list.AddRange(lines);
            }
            return m_list;
        }
        public override GeoBound GetBoundingBox()
        {
            if (LineStrings == null || LineStrings.Count == 0)
                return null;

            GeoBound bound = LineStrings[0].GetBoundingBox();
            LineStrings[0].Bound = LineStrings[0].GetBoundingBox(); 
            for (int i = 1; i < LineStrings.Count; ++i)
            {
                bound.UnionBound(LineStrings[i].GetBoundingBox());
                LineStrings[i].Bound = LineStrings[i].GetBoundingBox(); 
            }
            return bound;
        }
        public override Geometry Clone()
        {
            GeoMultiLineString mLine = new GeoMultiLineString();
            for (int i = 0; i < m_LineStrings.Count; i++)
            {
                mLine.LineStrings.Add((GeoLineString)LineStrings[i].Clone());
            }
            return mLine;
        }

        public override bool IsSelectByPt(GeoPoint pt)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                if (m_LineStrings[i].IsSelectByPt(pt))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 捕捉多线上的点
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override GeoPoint MouseCatchPt(GeoPoint pt, MouseCatchType type )
        { 
            for (int i = 0; i < NumGeometries; i++)
            {
                GeoPoint SnapPt = m_LineStrings[i].MouseCatchPt(pt,type );
                if (SnapPt != null)
                { 
                    return SnapPt;
                }
            }
            return null;            
        }
        public override void Move(double deltaX, double deltaY)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                LineStrings[i].Move(deltaX, deltaY);
            }
        }
        public override void RotateAt(double angle, GeoPoint basePt)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                LineStrings[i].RotateAt(angle,basePt);
            }
        }
        public override void SymmetryWithLine(GeoPoint ptStart, GeoPoint ptEnd)
        {
            for (int i = 0; i < NumGeometries; i++)
            {
                LineStrings[i].SymmetryWithLine(ptStart, ptEnd);
            }
        }
        public override bool RemoveVertex(GeoPoint pt)
        {
            bool bok = false;
            for (int i = 0; i < NumGeometries; i++)
            {
                if (LineStrings[i].RemoveVertex(pt))
                    bok = true;
            }
            return bok;
        }
        public override void ReadFromLQFile(System.IO.StreamReader sr)
        {
            string temp = sr.ReadLine();
          
            int nPart = int.Parse(temp.Trim());

            for (int i = 0; i < nPart; i++)
            { 
                GeoLineString line = new GeoLineString();
                line.ReadFromLQFile(sr);
                LineStrings.Add(line);
            }
        }
        public override void WriteToLQFile(System.IO.StreamWriter sw)
        {
            sw.WriteLine("GeoMultiLineString");
            sw.WriteLine(NumGeometries);//线的部分数
            for (int i = 0; i < NumGeometries; i++)
            { 
                GeoLineString line = this.LineStrings[i];
                sw.WriteLine(line.NumPoints);
                for (int j = 0; j < line.NumPoints; j++)
                {
                    sw.WriteLine("{0:f5},{1:f5}", line.Vertices[j].X, line.Vertices[j].Y);
                }
            }
        }
        public override void WriteGeoInfo(System.IO.StreamWriter sw)
        {
            WriteToLQFile(sw);
        }
        #endregion
    }
}
