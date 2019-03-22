using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Drawing;


namespace GIS.mm_Conv_Symbol
{
    public class Atom_DashLine : Atom_LineBase
    {
        #region private memebers
        private float m_line_width = 0.5f;
        private Color m_color = Color.Black;
        #endregion


        public Atom_DashLine(float penwidth, float udoffset, float lroffset, /*bool color,*/ float dash0, float dash1
            , PointF[] vertices, float[] ver_len, float[] rad)
        {
            m_line_width = penwidth;
            //m_color = (color == 1? Color.Black:Color.White);

            generate_dashline(udoffset, lroffset, dash0, dash1, vertices, ver_len, rad);

        }

        private void generate_dashline(float udoffset, float lroffset, float dash0, float dash1, PointF[] vertices, 
            float[] ver_len, float[] rad)
        {
            if (Math.Abs(udoffset) > 0.00001f)
            {
                PointF[] parall_line = SymbolAlgorithm.gen_paraline(udoffset, vertices, ver_len, rad);

                float[] verlen_t = new float[vertices.Length - 1];

                float dy = 0f, dx = 0f;
                for (int i = 0; i < vertices.Length - 1; i++)
                {
                    dy = vertices[i + 1].Y - vertices[i].Y;
                    dx = vertices[i + 1].X - vertices[i].X;

                    verlen_t[i] = System.Convert.ToSingle(Math.Sqrt(Math.Pow((double)(dx), 2) + Math.Pow((double)(dy), 2)));
                }


                float r_lroffset = lroffset;
                for (int i = 0; i < vertices.Length - 1; i++)
                {
                    compute_dashline(ref r_lroffset, dash0, dash1, parall_line[i], parall_line[i + 1],
                        verlen_t[i], rad[i]);
                }
            }
            else
            {
                float r_lroffset = lroffset;
                for (int i = 0; i < vertices.Length - 1; i++)
                {
                    compute_dashline(ref r_lroffset, dash0, dash1, vertices[i], vertices[i + 1],
                        ver_len[i], rad[i]);
                }
            }


        }

        private void compute_dashline(ref float lroffset, float dash0, float dash1,
                                      PointF vertex0, PointF vertex1, float ver_len, float rad)
        {

            PointF startpt = new PointF(vertex0.X, vertex0.Y);
            PointF endpt   = new PointF(vertex1.X, vertex1.Y);

            float partlen = ver_len - lroffset;
            if (partlen < 0f)
            {
                lroffset = -partlen;

                return;
            }

            float dashlen = dash1 + dash0;

            int cutpart = (int)(partlen / dashlen);

            float dx = vertex1.X - vertex0.X;
            float dy = vertex1.Y - vertex0.Y;

            
            float lroffset_dx = (lroffset * dx) / ver_len;
            float lroffset_dy = (lroffset * dy) / ver_len;
            float dash0_dx    =  (dash0 * dx) / ver_len;
            float dash0_dy    = (dash0 * dy) / ver_len;
            //float dash1_dx    = (Math.Abs(dy) < 0.05f ? dash1 : (dash1 * dx) / ver_len);
            float dash1_dx = (dash1 * dx) / ver_len;
            float dash1_dy    = (dash1 * dy) / ver_len;


            //add start pt
            if (lroffset > 0)
            {
                m_pts.Add(new PointF(startpt.X + lroffset_dx, startpt.Y + lroffset_dy));
            }
            else
                m_pts.Add(startpt);
            //end add start pt

            if (cutpart < 1)
            {

                float savelen = partlen - dash0;
                if (savelen > 0)
                {
                    m_pts.Add(new PointF(startpt.X + lroffset_dx + dash0_dx, startpt.Y + lroffset_dy + dash0_dy));
                    lroffset = dash1 - savelen;
                }
                else
                {
                    m_pts.Add(new PointF(startpt.X + dx, startpt.Y + dy));

                    lroffset = -partlen;
                }

            }
            else
            {
                m_pts.Add(new PointF(startpt.X + dash0_dx + lroffset_dx, startpt.Y + dash0_dy + lroffset_dy));

                for (int i = 1; i < cutpart; i++)
                {
                    m_pts.Add(new PointF(m_pts[m_pts.Count - 1].X + dash1_dx, m_pts[m_pts.Count - 1].Y + dash1_dy));
                    m_pts.Add(new PointF(m_pts[m_pts.Count - 1].X + dash0_dx, m_pts[m_pts.Count - 1].Y + dash0_dy));
                }

                float lastlen = partlen - (cutpart * dashlen);

                float savelen = lastlen - dash0;

                m_pts.Add(new PointF(m_pts[m_pts.Count - 1].X  + dash1_dx, m_pts[m_pts.Count - 1].Y  + dash1_dy));
                if (savelen > 0)
                {
                    m_pts.Add(new PointF(m_pts[m_pts.Count - 1].X + dash0_dx, m_pts[m_pts.Count - 1].Y + dash0_dy));
                    lroffset = dash1 - savelen;
                }
                else
                {

                    m_pts.Add(new PointF(endpt.X, endpt.Y));
                    lroffset = -lastlen;
                }

            }

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



    }
}
