using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.SpatialRelation;
using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    public class GeoLineString:GeoCurve
    {
        public GeoLineString()
        {
            m_pts = new List<GeoPoint>();
        }
        public GeoLineString(List<GeoPoint> pts)
        {
            m_pts = pts;
        }
        protected List<GeoPoint> m_pts;

        public List<GeoPoint> Vertices
        {
            get { return m_pts; }
            set { m_pts = value; }
        }

        public override bool IsEmpty()
        {
            return m_pts == null || m_pts.Count == 0;
        }

        #region Function
        public virtual void AddPoint(GeoPoint pt)
        {
            Vertices.Add(pt);
        }
        public List<GeoLineString> Bomb()
        {
            List<GeoLineString> m_list = new List<GeoLineString>();
            if (NumPoints >= 3)
            {
                for (int i = 0; i < NumPoints - 1; i++)
                {
                    GeoLineString newLine = new GeoLineString();
                    newLine.Vertices.Add(Vertices[i].Clone() as GeoPoint);
                    newLine.Vertices.Add(Vertices[i + 1].Clone() as GeoPoint);

                    m_list.Add(newLine);
                }
            }
            return m_list;
        }
        //返回折线的边界矩形
        public override GeoBound GetBoundingBox()
        {
            if (Vertices == null || Vertices.Count == 0)
                return null;

            GeoBound bound = new GeoBound(Vertices[0], Vertices[0]);
            for (int i = 1; i < Vertices.Count; ++i)
            {
                bound.UnionPoint(Vertices[i]);
            }
            return bound;
        }
        //返回直线的第一个端点
        public override GeoPoint StartPoint
        {
            get
            {
                if (m_pts.Count == 0)
                {
                    throw new ApplicationException("错误：没有起点");
                }
                else
                {
                    return m_pts[0];
                }
            }
        }
        //返回直线的最后一个端点
        public override GeoPoint EndPoint
        {
            get
            {
                if (m_pts.Count == 0)
                {
                    return null;
                }
                else
                {
                    return m_pts[m_pts.Count - 1];
                }
            }
        }
        //计算直线长度
        public override double Length
        {
            get
            {
                double sum = 0;
                for (int i = 0; i < m_pts.Count - 1; ++i)
                {
                    sum += m_pts[i].DistanceTo(m_pts[i + 1]);
                }
                return sum;
            }
        }
        public override double Area
        {
            get
            {
                if (Vertices.Count < 3 || !IsClosed)
                    return 0;
                double sum = 0;

                for (int i = 0; i < Vertices.Count - 1; i++)
                {
                    GeoPoint pt = Vertices[i];
                    GeoPoint pt2 = Vertices[i + 1];
                    sum += (pt.Y + pt2.Y) * (pt.X - pt2.X);
                }
                return Math.Abs(sum / 2);
            }
        }
        public virtual int NumPoints
        {
            get { return Vertices.Count; }
        }
        public override bool IsRing
        {
            get { return IsClosed && IsSimple(); }
        }

        public override bool IsSimple()
        {
            List<GeoPoint> verts = new List<GeoPoint>();
            for (int i = 0; i < NumPoints; ++i)
            {
                if (!verts.Exists(delegate(GeoPoint pt) { return pt.IsEqual(Vertices[i]); }))
                    verts.Add(Vertices[i]);
            }
            return (verts.Count == Vertices.Count - (IsClosed ? 1 : 0));
        }
        /// <summary>
        ///  复制线
        /// </summary>
        /// <returns>geometry的复制</returns>
        public override Geometry Clone()
        {
            GeoLineString line = new GeoLineString();
            for (int i = 0; i < Vertices.Count; ++i)
            {
                line.Vertices.Add((GeoPoint)Vertices[i].Clone());
            }
            return line;
        }
        /// <summary>
        /// 判断是否点选中
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override bool IsSelectByPt(GeoPoint pt)
        { 
            for (int i = 0; i < NumPoints - 1; i++)
            {
                if(GIS.SpatialRelation.GeoAlgorithm.IsOnLineCoarse(pt,Vertices[i],Vertices[i+1]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断是否鼠标捕捉到该线上的点
        /// </summary>
        /// <param name="pt"></param>
        /// <returns>返回捕捉到的点</returns>
        public override GeoPoint MouseCatchPt(GeoPoint pt, MouseCatchType type )
        {
        
            for (int i = 0; i < NumPoints; i++)
            {
                if (type == MouseCatchType.Vertex || type == MouseCatchType.Both)
                {
                    if (GeoAlgorithm.IsOnPointCoarse(Vertices[i], pt))
                        return Vertices[i];
                }
                if (type == MouseCatchType.Center || type == MouseCatchType.Both)
                {
                    if (i == NumPoints - 1)
                        return null;
                    double x = (Vertices[i].X + Vertices[i + 1].X) / 2;
                    double y = (Vertices[i].Y + Vertices[i + 1].Y) / 2;
                    GeoPoint centPt = new GeoPoint(x, y);
                    if (GeoAlgorithm.IsOnPointCoarse(pt, centPt))
                    {
                        return centPt;
                    }
                }
            }
            return null;
        }
        public override void WriteGeoInfo(System.IO.StreamWriter sw)
        {
            WriteToLQFile(sw);
        }
        public override void ReadFromLQFile(System.IO.StreamReader sr)
        {
            string temp = sr.ReadLine();
         
            int nPointCount = int.Parse(temp.Trim());
            for (int i = 0; i < nPointCount; i++)
            { 
                GeoPoint pt = new GeoPoint();
                pt.ReadFromLQFile(sr);
                AddPoint(pt); 
            } 
        }
        public override void WriteToLQFile(System.IO.StreamWriter sw)
        {
            sw.WriteLine("GeoLineString");
            sw.WriteLine(NumPoints);
            for (int i = 0; i < NumPoints; i++)
            {
                sw.WriteLine("{0:f5},{1:f5}", m_pts[i].X, m_pts[i].Y);
            }
        }

        public void ClearRepeatPoints()
        {
            for (int i = 1; i < NumPoints; ++i)
            {
                if (Vertices[i - 1].IsEqual( Vertices[i]))
                {
                    Vertices.RemoveAt(i);
                    --i;
                }
            }
        }
        public override void Move(double deltaX, double deltaY)
        {
            for (int i = 0; i < NumPoints; i++)
            {
                m_pts[i].Move(deltaX, deltaY);
            }
        }
        public override void RotateAt(double angle, GeoPoint basePt)
        {
            for (int i = 0; i < NumPoints; i++)
            {
                m_pts[i].RotateAt(angle, basePt);
            }    
        }
        public override void SymmetryWithLine(GeoPoint ptStart, GeoPoint ptEnd)
        {
            for (int i = 0; i < NumPoints; i++)
            {
                m_pts[i].SymmetryWithLine(ptStart, ptEnd);
            }
        }
        public override bool RemoveVertex(GeoPoint pt)
        {
            if (m_pts.Count <= 2)
                return false;

            bool bok = false;
            for (int i = NumPoints - 1; i >= 0; i--)
            {
                if (m_pts[i].IsEqual(pt))
                {
                    m_pts.RemoveAt(i);
                    bok = true;
                    if (m_pts.Count == 2)
                        return bok;
                }
            }
            return bok;

        }
        public override bool IsEqual(Geometry geom)
        {
            GeoLineString line = geom as GeoLineString;
      
            if (line == null || line.NumPoints != NumPoints)
                return false;
            for (int i = 0; i < NumPoints; i++)
            {
                if (!line.Vertices[i].IsEqual(Vertices[i]))
                    return false;
            }
            return true;
        }
        public bool IsEqualW(Geometry geom)//起点可以不同
        {
            GeoLineString line = geom as GeoLineString;

            if (line == null || line.NumPoints != NumPoints)
                return false;
            int k = 0;
            bool flag = false;
            for (int i = 0; i < NumPoints; i++)
            {
                if (Vertices[0].IsEqual(line.Vertices[i]))
                {
                    if(line.Vertices[i].IsEqual(line.Vertices[(i+1)%NumPoints]))
                    {
                        k = (i + 1) % NumPoints;
                    }
                    else
                        k = i;
                    flag = true;
                    break;
                }
            }
            if (!flag)
                return false;
            for (int i = 0; i < NumPoints; i++)
            {
                if (!Vertices[i].IsEqual(line.Vertices[(i + k) % NumPoints]))
                    return false;
            }
            return true;
        }
        #endregion


    }
}
