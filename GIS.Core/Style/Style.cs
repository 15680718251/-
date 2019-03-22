using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.Style
{
    /// <summary>
    /// 绘制的样式的基类
    /// </summary>
    public class LayerStyle
    {
        private double m_minVisible;
        private double m_maxVisible; 

        public LayerStyle()
        {
            m_minVisible = 0;
            m_maxVisible = Double.MaxValue;
      
        }

        #region Property
        public double MinVisible
        {
            get { return m_minVisible; }
            set { m_minVisible = value; }
        }
        public double MaxVisible
        {
            get { return m_maxVisible; }
            set { m_maxVisible = value; }
        }
 
        #endregion
    }
}
