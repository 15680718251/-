using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.TreeIndex.UIEventArgs
{
    public class OutPutEventArgs
    {
        public OutPutEventArgs(string strText)
        {
            m_OutPutText = strText;
        }
        private string m_OutPutText;

        public string OutPutText
        {
            get { return m_OutPutText; }
            set { m_OutPutText = value; }
        }

    }
}
