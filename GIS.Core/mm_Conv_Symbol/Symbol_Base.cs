using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.Geometries;


namespace GIS.mm_Conv_Symbol
{
    public  class Symbol_Base
    {

//none...............
        protected SymbolBound m_Bound;
        public Symbol_Base()
        {
            m_Bound = new SymbolBound();
        }

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
            return null;
        }


    }
}
