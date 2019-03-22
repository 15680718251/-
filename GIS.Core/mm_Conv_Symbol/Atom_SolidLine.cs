using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Drawing;

namespace GIS.mm_Conv_Symbol
{
    public class Atom_SolidLine : Atom_LineBase
    {

        #region private memebers
       
        private float m_line_width = 0.5f;
        private Color m_color = Color.Black;
        #endregion


        public Atom_SolidLine(float penwidth, float udoffset, PointF[] pts, float[] ver_len, float[] rad)
        {
            m_line_width = penwidth;

            PointF[] ppts =  generate_solidline(pts, udoffset, ver_len, rad);


            for (int i = 0; i < ppts.Length; i++)
            {
                m_pts.Add(ppts[i]);
            }

        }

        public Atom_SolidLine(float penwidth, PointF[] ppts)
        {
            m_line_width = penwidth;
            for (int i = 0; i < ppts.Length; i++)
            {
                m_pts.Add(ppts[i]);
            }
        }

        public PointF[] generate_solidline(PointF[] pts, float udoffset, float[] ver_len, float[] rad)
        {
            if (udoffset == 0f)
                return pts;

            //得实现平行线算法来控制udoffset

            PointF[] paral_line = SymbolAlgorithm.gen_paraline(udoffset, pts, ver_len, rad);
            return paral_line;

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
