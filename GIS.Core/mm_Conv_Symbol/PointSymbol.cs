using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GIS.mm_Conv_Symbol
{
    public class PointSymbol:Symbol_Base
    {

        protected List<Atom_PointBase> m_atom;

        public PointSymbol()
        {
            m_atom = new List<Atom_PointBase>();
           
        }

        public override SymbolBound GetBoundingBox()
        {

            for (int i = 0; i < m_atom.Count; i++)
            {
                m_Bound.UnionBound( m_atom[i].GetBoundingBox());
            }
            return m_Bound;
        }

        public PointSymbol(List<Atom_PointBase> list)
        {
            m_atom = list;
        }

        

        public List<Atom_PointBase> Atom
        {
            get { return m_atom; }
            set { m_atom = value; }
        }

        public void AddAtom(Atom_PointBase atom)
        {
            m_atom.Add(atom);
        }

        public PointSymbol Clone()
        {

            PointSymbol ptsym = new PointSymbol();
            for (int i = 0; i < m_atom.Count; i++)
            {
                ptsym.AddAtom(m_atom[i].Clone());
            }

            return ptsym;
        }

        //public PointSymbol Translate(float dx, float dy)
        //{
        //    PointSymbol ptsym = new PointSymbol();

        //    //mm_matrix mtx = new mm_matrix();
        //    //mtx.translate(dx, dy);

        //    for (int i = 0; i < ptsym.m_atom.Count; i++)
        //    {

                
        //    }


        //}

        public void Translate(float dx, float dy)
        {
            for (int i = 0; i < m_atom.Count; i++)
            {
                m_atom[i].Translate(dx, dy);
            }
        }

        public void Rotate(float rad)
        {
            for (int i = 0; i < m_atom.Count; i++)
            {
                m_atom[i].Rotate(rad);
            }
        }


        public void Rotate_At(float x, float y, float rad)
        {
            for (int i = 0; i < m_atom.Count; i++)
            {
                m_atom[i].Rotate_At(x, y, rad);
            }
        }

        public void Flip_X()
        {
            for (int i = 0; i < m_atom.Count; i++)
            {
                m_atom[i].Flip_X();
            }
        }

        public void Flip_X_Y(float y)
        {
            this.Translate(0, -y);
            //this.Flip_X();     //??????
            this.Flip_Y();
            this.Translate(0, y);
        }

        public void Flip_Y_X(float x)
        {
            this.Translate(-x, 0);
            //this.Flip_Y();    //????????
            this.Flip_X();
            this.Translate(x, 0);
        }

        public void Flip_Y()
        {
            for (int i = 0; i < m_atom.Count; i++)
            {
                m_atom[i].Flip_Y();
            }
        }

    }
}
