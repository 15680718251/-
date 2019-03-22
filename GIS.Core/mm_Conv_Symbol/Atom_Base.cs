using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GIS.mm_Conv_Symbol
{
    public class Atom_Base
    {
        protected List<PointF> m_pts;

        public List<PointF> Vertices
        {
            get { return m_pts; }
            set { m_pts = value; }
        }

        public Atom_Base()
        {
            m_pts = new List<PointF>();
            m_Bound = new SymbolBound();
        }

        protected SymbolBound m_Bound;
        public SymbolBound Bound
        {
            get
            {
                //if (m_Bound == null)
                    m_Bound = GetBoundingBox();
                return m_Bound;
            }
            set
            {
                m_Bound = value;
            }
        }

        public virtual SymbolBound GetBoundingBox()
        {

            for (int i = 0; i < m_pts.Count; i++)
            {
                m_Bound.UnionPoint(m_pts[i]);
            }
            return m_Bound;

        }


        public virtual void Translate(float dx, float dy)
        {
            
            mm_matrix mtx = new mm_matrix();
            mtx.translate(dx, dy);

            PointF[] pts = m_pts.ToArray();

            mtx.transform(ref pts);

            m_pts.Clear();
            for (int i = 0; i < pts.Length; i++)
            {
                m_pts.Add(pts[i]);
            }

        }

        public virtual void Rotate(float rad)
        {
            mm_matrix mtx = new mm_matrix();
            mtx.rotate(rad);

            PointF[] pts = m_pts.ToArray();

            mtx.transform(ref pts);

            m_pts.Clear();
            for (int i = 0; i < pts.Length; i++)
            {
                m_pts.Add(pts[i]);
            }
        }

        public virtual void Rotate_At(float x, float y, float rad)
        {
            mm_matrix mtx = new mm_matrix();
            mtx.rotate_at(rad, x, y);

            PointF[] pts = m_pts.ToArray();

            mtx.transform(ref pts);

            m_pts.Clear();
            for (int i = 0; i < pts.Length; i++)
            {
                m_pts.Add(pts[i]);
            }
        }

        public void Flip_X()
        {
            mm_matrix mtx = new mm_matrix();
            mtx.flip_x();

            PointF[] pts = m_pts.ToArray();

            mtx.transform(ref pts);

            m_pts.Clear();
            for (int i = 0; i < pts.Length; i++)
            {
                m_pts.Add(pts[i]);
            }
        }

        public void Flip_Y()
        {
            mm_matrix mtx = new mm_matrix();
            mtx.flip_y();

            PointF[] pts = m_pts.ToArray();

            mtx.transform(ref pts);

            m_pts.Clear();
            for (int i = 0; i < pts.Length; i++)
            {
                m_pts.Add(pts[i]);
            }

        }

    }
}
