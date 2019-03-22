using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;
using GIS.Utilities;
using System.Collections;



namespace GIS.mm_Conv_Symbol
{
    public class Atom_Line4p : Atom_PointBase
    {
        #region private memebers
        private bool m_bFill = false;
        private float m_line_width = 0.5f;
        private Color m_color = Color.Black;
        #endregion

        public Atom_Line4p()
        {

        }
        public Atom_Line4p(float x, float y, float dx0, float dy0, float dx1, float dy1, 
            float angle,  bool bfill, float penwidth, PointF[] pts)
        {

            m_bFill = bfill;
            m_line_width = penwidth;


            generate_line4p(x, y, dx0, dy0, dx1, dy1, angle, pts);

            for (int i = 0; i < pts.Length; i++)
            {
                m_pts.Add(pts[i]);
            }
        }

        public Atom_Line4p(float xx, float yy, List<float> list)
        {
            float x = xx;
            float y = yy;
            float dx0 = list[0];
            float dy0 = list[1];
            float dx1 = list[2];
            float dy1 = list[3];
            float angle = list[4];
            m_bFill = list[5] == 1 ? true : false;
            m_line_width = list[6];

            PointF[] pts = new PointF[(list.Count - 7)/2];


            for (int j = 0, k = 7; j < (list.Count - 7) / 2; j++, k += 2)
            {
                pts[j].X = list[k];
                pts[j].Y = list[k + 1];
            }

            generate_line4p(x, y, dx0, dy0, dx1, dy1, angle, pts);


            for (int i = 0; i < pts.Length; i++)
            {
                m_pts.Add(pts[i]);
            }

        }

        private void generate_line4p(float x, float y, float dx0, float dy0, float dx1, float dy1,
            float angle,  PointF[] pts)
        {
            
            mm_matrix mtx = new mm_matrix();
            //handle dx0, dy0, handle rotate
            mtx.translate(dx0, dy0);
            float tmp = (((float)Math.PI) / 180) * angle;
            mtx.rotate_at(tmp, dx1, dy1);

            mtx.translate(x, y);
            mtx.transform(ref pts);
            
        }

        public override Atom_PointBase Clone()
        {
            Atom_Line4p line4p = new Atom_Line4p();

            for (int i = 0; i < m_pts.Count; i++)
            {
                line4p.m_pts.Add(new PointF(m_pts[i].X, m_pts[i].Y));
            }

            return line4p;

        }


        private void Add(PointF pt)
        {

            this.m_pts.Add(pt);
        }

        public float line_width
        {
            get { return m_line_width; }
            set { m_line_width = value; }
        }

        public bool bfill
        {
            get { return m_bFill; }
            set { m_bFill = value; }
        }

        public Color clr
        {
            get { return m_color; }
            set { m_color = value; }
        }


    }
}
