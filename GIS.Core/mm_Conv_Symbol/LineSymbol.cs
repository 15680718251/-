using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GIS.mm_Conv_Symbol
{
    public class LineSymbol:Symbol_Base
    {
        protected List<Atom_LineBase> m_atom;
        protected List<PointSymbol> m_ptsymbol;

        public LineSymbol()
        {
            m_atom = new List<Atom_LineBase>();
            m_ptsymbol = new List<PointSymbol>();
        }

        public void AddAtom(Atom_LineBase atom)
        {
            m_atom.Add(atom);
        }

        public override SymbolBound GetBoundingBox()
        {

            for (int i = 0; i < m_atom.Count; i++)
            {
                m_Bound.UnionBound(m_atom[i].Bound);
            }
            for (int i = 0; i < m_ptsymbol.Count; i++)
            {
                m_Bound.UnionBound(m_ptsymbol[i].Bound);
            }
            return m_Bound;
        }


        public List<PointSymbol> ptssymbol
        {
            get { return m_ptsymbol; }
            set { m_ptsymbol = value; }
        }

        public void AddPointSymbol(PointSymbol ptsym_o, PointF[] vertices_t, float udoffset_t, float lroffset, float gaplength,
            float[] ver_len_t/*, bool[] x_aorm, bool[] y_aorm*/,float[] rad_t)
        {
            //算法计算各点的数据

            //
            // 注意：pointsymbol的定位点统一于下中部或者正中;否则会出现绘制错误
            //

            //ptsym 是根据第一个点计算出来的点数据
            PointF[] vertices = new PointF[0];
            float[] ver_len = new float[0];

            if (udoffset_t == 0f)
            {
                vertices = vertices_t;
                ver_len = ver_len_t;
            }
            else
            {
               vertices = SymbolAlgorithm.gen_paraline(udoffset_t, vertices_t, ver_len_t, rad_t);
               ver_len = new float[vertices.Length - 1];
               float deltay = 0f, deltax = 0f;
               for (int i = 0; i < vertices.Length - 1; i++)
               {
                   deltay = vertices[i + 1].Y - vertices[i].Y;
                   deltax = vertices[i + 1].X - vertices[i].X;
                   ver_len[i] = System.Convert.ToSingle(Math.Sqrt(Math.Pow((double)(deltax), 2) + Math.Pow((double)(deltay), 2)));
               }

            }

            //float udoffset = 0f;

            float top = (float)ptsym_o.Bound.Top;
            float bottom = (float)ptsym_o.Bound.Bottom;

            bool bcenterorbottom = false;

            if (top - Math.Abs(bottom) == 0)
                bcenterorbottom = true;
            else
                bcenterorbottom = false;

            float r_lroffset = lroffset;
            ptsym_o.Translate(vertices[0].X, vertices[0].Y);

            PointSymbol ptsym_s = ptsym_o.Clone();

            //lroffset 必须大于0

            #region new code
            float dx = vertices[1].X - vertices[0].X;
            float dy = vertices[1].Y - vertices[0].Y;
            float dxx, dyy;

            float rad = (float)Math.Atan((double)(dy / dx));

            if (dx < 0)
            {
                if (!bcenterorbottom)   //bottom
                {
                    ptsym_s.Flip_X_Y(vertices[0].Y/* - udoffset / 2*/);
                    ptsym_s.Flip_Y_X(vertices[0].X);

                    //ptsym_s.Translate(0, -udoffset - top);
                    //ptsym_s.Flip_X_Y(vertices[0].Y);
                    //ptsym_s.Translate(0, -udoffset);
                }
                else
                {
                    //if (udoffset != 0)
                    //    ptsym_s.Translate(0, -udoffset);
                }
            }
            else
            {
                //ptsym_s.Translate(0, udoffset);
            }

            ptsym_s.Rotate_At(vertices[0].X, vertices[0].Y, rad);

            ComputeEachPart(ptsym_s, /*udoffset, */ref r_lroffset, vertices[0], vertices[1], gaplength, ver_len[0]/*, x_aorm[i], y_aorm[i]*/);

            for (int i = 1; i < vertices.Length - 1; i++)
            {
                dx = vertices[i + 1].X - vertices[i].X;
                dy = vertices[i + 1].Y - vertices[i].Y;
                rad = (float)Math.Atan((double)(dy / dx));

                dxx = vertices[i].X - vertices[0].X;
                dyy = vertices[i].Y - vertices[0].Y;

                ptsym_s = ptsym_o.Clone();
                ptsym_s.Translate(dxx, dyy);

                if (dx < 0)
                {
                    if (!bcenterorbottom)
                    {
                        ptsym_s.Flip_X_Y(vertices[i].Y/* - udoffset / 2*/);
                        ptsym_s.Flip_Y_X(vertices[i].X);
                        //ptsym_s.Translate(0, -udoffset - top);
                        //ptsym_s.Translate(0, -udoffset);
                        //float y = vertices[i].Y;
                    }
                    else
                    {
                        //if (udoffset != 0)
                        //    ptsym_s.Translate(0, -udoffset);
                    }

                }
                else
                {
                    //ptsym_s.Translate(0, udoffset);
                }
                ptsym_s.Rotate_At(vertices[i].X, vertices[i].Y, rad);

                ComputeEachPart(ptsym_s, /*udoffset, */ref r_lroffset, vertices[i], vertices[i + 1], gaplength, ver_len[i]/*, x_aorm[i], y_aorm[i]*/);

            }

            #endregion


        }


        private void ComputeEachPart(PointSymbol ptsym, /*float udoffset, */ref float lroffset, PointF startpt, PointF endpt,  
            float gaplength, float partlen/*, bool x_aorm, bool y_aorm */)
        {
            if (lroffset > partlen)
            {
                lroffset = lroffset - partlen;
                return;
            }

            //add the first point symbol
            float dx = endpt.X - startpt.X;
            float dy = endpt.Y - startpt.Y;
            
            
            float dx_lroffset = (dx * lroffset) / partlen;
            float dy_lroffset = (dy * lroffset) / partlen;
            float dx_gaplength = 0;
            float dy_gaplength = 0;

            
            PointSymbol firstpt = ptsym.Clone();
            firstpt.Translate(dx_lroffset, dy_lroffset);
            PointSymbol curpt = firstpt;

            m_ptsymbol.Add(firstpt);

            float len2cut =  (partlen - lroffset);

            int cutpart = (Int32) (len2cut / gaplength);

            if (cutpart >= 1)
            {
                //须检测partlen非0
                dx_gaplength = (dx * gaplength) / partlen;
                dy_gaplength = (dy * gaplength) / partlen;
            }

            for (int i = 0; i < cutpart; i++)
            {
                PointSymbol nextpt = curpt.Clone();
                nextpt.Translate(dx_gaplength, dy_gaplength);
                m_ptsymbol.Add(nextpt);

                curpt = nextpt;
            }

            float len2save = gaplength - (len2cut - (cutpart * gaplength));

            lroffset = len2save;

        }



        public List<PointSymbol> point_symbol
        {
            get { return m_ptsymbol; }
            set { m_ptsymbol = value; }
        }

        public List<Atom_LineBase> Atom
        {
            get { return m_atom; }
            set { m_atom = value; }
        }


    }
}
