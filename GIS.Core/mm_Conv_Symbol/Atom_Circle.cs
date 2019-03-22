using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;
using GIS.Utilities;

namespace GIS.mm_Conv_Symbol
{
    public class Atom_Circle : Atom_PointBase
    {
        #region private members
        private bool m_bFill = false;
        private Color m_color = Color.Black ;
        private float m_line_width = 0.5f;
        private bool m_bpoint = false;
        #endregion

        public override Atom_PointBase Clone()
        {
            List<PointF> list2copy = new List<PointF>();
            for (int i = 0; i < this.m_pts.Count; i++)
            {
                list2copy.Add(new PointF(m_pts[i].X, m_pts[i].Y));
            }
            Atom_Circle circle = new Atom_Circle(list2copy);
            circle.m_bFill = this.m_bFill;
            circle.m_color = this.m_color;
            circle.m_line_width = this.m_line_width;
            circle.bpoint = this.bpoint;
            return circle;
        }

        private Atom_Circle(List<PointF> list)
        {
            this.m_pts = list;
        }

        //circle C,X,Y,a,b,angle,filled,penwidth,r,填充模式
        public Atom_Circle(float x, float y, float dx0, float dy0, float dx1, float dy1,  
                           float ang,  bool bfill, float penwidth, float r )
        {
 
            m_bFill = bfill;
            m_line_width = penwidth;

            if (r == 0.15f)
            {
                m_bpoint = true;
                m_pts.Add(new PointF(x + dx0, y + dy0));
                return;
            }

            PointF[] pt = generate_circle(x, y, r, dx0, dy0, dx1, dy1, ang);

            for (int i = 0; i < pt.Length; i++)
            {
                m_pts.Add(pt[i]);
            }


        }



        public PointF[] generate_circle(float x, float y, float r, float dx0,
                                        float dy0, float dx1, float dy1, float ang)
        {
            //x, y 为实际圆心位置，r 为半径
            //dx0, dy0为代表图素定位点与符号定位点的偏移量
            //dx1, dy1代表图素旋转点与符号定位点的偏移量,一般均采用0值，即与符号定位点一致

            mm_matrix mtx = new mm_matrix();
            int part = 8;
            PointF[] pt = new PointF[part + 1];
            float angle = 360 / part;
            float arc = (((float)Math.PI) / 180) * angle;
            pt[0] = new PointF(r, 0);

            for (int i = 1; i < part; i++)
            {
                pt[i] = new PointF(r, 0);
                mtx.rotate(arc);
                mtx.transform(ref pt[i]);
            }
            pt[part] = pt[0];

            //handle dx0, dy0, handle rotate
            mtx.reset();
            mtx.translate(dx0, dy0);
            float tmp = (((float)Math.PI) / 180) * ang;
            mtx.rotate_at(tmp, dx1, dy1);

            mtx.translate(x, y);
            mtx.transform(ref pt);
            return pt;
        }

        public bool bpoint
        {
            get { return m_bpoint; }
            set { m_bpoint = value; }
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




        

    }
}
