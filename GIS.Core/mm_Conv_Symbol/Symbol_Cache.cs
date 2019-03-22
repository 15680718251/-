using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using GIS.Geometries;



//unused

namespace GIS.mm_Conv_Symbol
{
    public class Symbol_Cache
    {
        #region
        private List<Symbol_Base> m_symbol;
        #endregion
        public Symbol_Cache()
        {
            m_symbol = new List<Symbol_Base>();
        }

        public List<Symbol_Base> symbol_atom
        {
            get { return m_symbol; }
                set { m_symbol = value; }
        }

        public Symbol_Base this[int index]
        {

            get { return m_symbol[index]; }
        }

        public void Add(Symbol_Base gtr)
        {
            m_symbol.Add(gtr);
        }

        

    }
}
