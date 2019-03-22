using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;

namespace GIS.UI.Forms
{
    public partial class AddText : Form
    {
        //地图数据
        private AxMapControl axMapControl1;
        private static string addText;//存储文本框中的内容
        public static string AddText1
        {
            get { return addText; }
            set { addText = value; }
        }
        public AddText(AxMapControl axMapControl)
        {
            if (axMapControl.LayerCount <= 0)
            {
                MessageBox.Show("请先添加图层");
            }
            else
            {
                InitializeComponent();
                this.axMapControl1 = axMapControl;
            }

        }


        //#region 文本符号化

        //int flag = 1;


        ///// <summary>
        ///// 右键时添加文本
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void mainMapControl_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        //{
        //    if (flag == 0)
        //    {
        //        //右键时添加对象，左键时返回
        //        if (e.button != 2)
        //        {
        //            return;
        //        }
        //        IPoint pPoint = new PointClass();
        //        pPoint = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
        //        //设置文本格式
        //        ITextSymbol pTextSymbol = new TextSymbolClass();
        //        StdFont myFont = new stdole.StdFontClass();
        //        myFont.Name = "宋体";
        //        myFont.Size = 24;
        //        pTextSymbol.Font = (IFontDisp)myFont;
        //        pTextSymbol.Angle = 0;
        //        pTextSymbol.RightToLeft = false;//文本由左向右排列
        //        pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVABaseline;//垂直方向基线对齐
        //        pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHAFull;//文本两端对齐
        //        pTextSymbol.Text = TextBox.Text;
        //        ITextElement pTextElement = new TextElementClass();
        //        pTextElement.Symbol = pTextSymbol;
        //        pTextElement.Text = pTextSymbol.Text;
        //        //获取一个图上坐标，且将文本添加到此位置           
        //        IElement pElement = (IElement)pTextElement;
        //        pElement.Geometry = pPoint;
        //        axMapControl1.ActiveView.GraphicsContainer.AddElement(pElement, 0);
        //        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, pElement, null);
        //    }
        //    else
        //    {
        //        return;
        //    }
        //}
        //#endregion

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (this.TextBox.Text.Trim() == "")
            {
                MessageBox.Show("请输入您想添加的标注！");
            }
            else
            {
                addText = this.TextBox.Text;
                this.Close();
            }
            //flag = 0;

        }

        private void btnCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
