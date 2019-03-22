using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GIS.Style
{
    public class VectorStyle:LayerStyle
    {        
        public VectorStyle()
        {
            Initial();         
        }

        public VectorStyle(float symbolsize,float linesize,Color outlineclr,Color fillclr,bool select)
        {
            m_FillColor = Color.FromArgb(150, fillclr);
            m_SymbolSize = symbolsize;
            m_LineSize = linesize;
            m_OutLineColor = outlineclr;
            m_bSelected = select; 
        }

        #region PrivateMember
    
        private float m_SymbolSize = 2;    // 点符号的大小

        private float m_LineSize = 0.5f;   // 线符号宽度

        private Color m_OutLineColor = Color.Blue;//轮廓线的颜色

        private Color m_FillColor = Color.Green;  //填充色

        private bool m_EnableOutLine = true;//是否显示轮廓线 

        private bool m_bSelected = true; //默认为 选中的样式 

        public  static Color m_NodeBrushColor = Color.Red; //选中时节点的颜色
        public static Color m_NodePenColoe = Color.Black;//选中时节点的外轮廓

        
        #endregion

        #region Properties

        public static Color NodePenColoe
        {
            get { return VectorStyle.m_NodePenColoe; }
            set { VectorStyle.m_NodePenColoe = value; }
        }
        public static Color NodeBrushColor
        {
            get { return m_NodeBrushColor; }
            set { m_NodeBrushColor = value; }
        }

        public bool IsSelected
        {
            get { return m_bSelected; }
            set { m_bSelected = value; }
        }
        public float SymbolSize
        {
            get { return m_SymbolSize; }
            set { m_SymbolSize = value; }
        }
        public float LineSize
        {
            get { return m_LineSize; }
            set { m_LineSize = value; }
        }
        public Color LineColor
        {
            get { return m_OutLineColor; }
            set { m_OutLineColor = value; }
        }
        public Color FillColor
        {
            get { return m_FillColor; }
            set { m_FillColor = value; }
        }
        public bool EnableOutLine
        {
            get { return m_EnableOutLine; }
            set { m_EnableOutLine = value; }
        } 
        #endregion
     
        private void Initial()
        {
            Random RandomNum_First = new Random(Guid.NewGuid().GetHashCode());
            Random RandomNum_Sencond = new Random(Guid.NewGuid().GetHashCode());

            //  为了在白色背景上显示，尽量生成深色
            int int_Red = RandomNum_First.Next(256);
            int int_Green = RandomNum_Sencond.Next(256);
            int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;

            m_OutLineColor = Color.FromArgb(255, int_Red, int_Green, int_Blue);
            m_FillColor = Color.FromArgb(150, int_Red, int_Green, int_Blue);

            m_bSelected = false;
        }
    }
}
