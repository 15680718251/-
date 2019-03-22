using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GIS.mm_Conv_Symbol
{
    public class Atom_Arc : Atom_PointBase
    {

        #region private members
        private bool m_bFill = false;
        private bool m_bClosed = false;
        private Color m_color = Color.Black;
        private float m_line_width = 0.5f;

        #endregion

        public override Atom_PointBase Clone()
        {
            Atom_Arc arc = new Atom_Arc();

            for (int i = 0; i < m_pts.Count; i++)
            {
                arc.m_pts.Add(new PointF( m_pts[i].X, m_pts[i].Y));   
            }

            return arc;

        }

        private Atom_Arc()
        {

        }
        public Atom_Arc(float x, float y, float dx0, float dy0, float dx1, float dy1, float ang, bool bfill, float penwidth, float r, 
            float startangle, float endangle, bool bclosed)
        {
            //float x, float y, float dx0, float dy0, float dx1, float dy1,  
            //              float ang,  bool bfill, float penwidth, float r )
            m_bFill = bfill;
            m_bClosed = bclosed;
            m_bFill = bfill;
            m_line_width = penwidth;

            PointF[] pt = generate_arc(x, y, dx0, dy0, dx1, dy1, ang, r, startangle, endangle);

            for (int i = 0; i < pt.Length; i++)
            {
                m_pts.Add(pt[i]);
            }
            
        }



       

        public PointF[] generate_arc(float x, float y, float dx0, float dy0, float dx1, float dy1, float ang, float r,
            float startangle, float endangle)
        {
            //x, y 为实际圆心位置，r 为半径

            float delta_angle;
            if (endangle == 0)
                endangle = 360;

            if (startangle >= 180 && startangle <= 360 && endangle >= 0 && endangle <= 180)
                delta_angle = 360 - startangle + endangle;
            else
                delta_angle = endangle - startangle;

            int part;
            if (Math.Abs(delta_angle) < 60)
                part = 2;
            else
                part = (int)(Math.Abs(delta_angle) / 30 );
            part++;

            PointF[] pt = new PointF[part];

            mm_matrix mtx = new mm_matrix();

            float rad = (((float)Math.PI) / 180) * startangle;
            mtx.rotate(rad);

            pt[0] = new PointF(r, 0);
            mtx.transform(ref pt[0]);
            rad = (((float)Math.PI) / 180) * (Math.Abs(delta_angle) / (part - 1));

            for (int i = 1; i < part; i++)
            {
                pt[i] = new PointF(r, 0);
                mtx.rotate(rad);
                mtx.transform(ref pt[i]);
            }

            //------------------------------------------
            mtx.reset();
            mtx.translate(dx0, dy0);
            float tmp = (((float)Math.PI) / 180) * ang;
            mtx.rotate_at(tmp, dx1, dy1);

            
            mtx.translate(x, y);
            mtx.transform(ref pt);
            return pt;
        }

        public bool bFill
        {
            get { return m_bFill; }
            set { m_bFill = value; }
        }

        public Color clr
        {
            get { return m_color; }
            set { m_color = value; }
        }

        public float line_width
        {
            get { return m_line_width; }
            set { m_line_width = value; }
        }

        public bool bclosed
        {
            get { return m_bClosed; }
            set { m_bClosed = value; }
        }


    }
}
