using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.ADF;
namespace 全球典型要素数据库增量更新系统
{
    public partial class SetParameter : Form
    {
        List<IFeatureClass> _lstFeatCls = null;
        private IStyleGalleryItem pStyleGalleryItem;
        private ILegendClass pLegendClass;
        private ILayer pLayer;
        public ISymbol pSymbol;
        public Image pSymbolImage;
        string filepath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        OperateMap m_OperateMap = new OperateMap();
        bool contextMenuMoreSymbolInitiated = false;

        public SetParameter(ILegendClass tempLegendClass, ILayer tempLayer)
        {
            InitializeComponent();
            this.pLegendClass = tempLegendClass;
            this.pLayer = tempLayer;
        }
        public SetParameter()
        { }

        private IMap _map;
        public IMap Map
        {
            get { return _map; }
            set { _map = value; }
        }

        private void SymbologyCtr_OnStyleClassChanged(object sender, ESRI.ArcGIS.Controls.ISymbologyControlEvents_OnStyleClassChangedEvent e)
        {
            switch (((ISymbologyStyleClass)e.symbologyStyleClass).StyleClass)
            {
                case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                    this.lblSize.Visible = true;
                    this.nudSize.Visible = true;
                    this.lblWidth.Visible = false;
                    this.nudWidth.Visible = false;
                    this.lblOutlineColor.Visible = false;
                    this.btnOutlineColor.Visible = false;
                    break;
                case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                    this.lblSize.Visible = false;
                    this.nudSize.Visible = false;
                    this.lblWidth.Visible = true;
                    this.nudWidth.Visible = true;
                    this.lblOutlineColor.Visible = false;
                    this.btnOutlineColor.Visible = false;
                    break;
                case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                    this.lblSize.Visible = false;
                    this.nudSize.Visible = false;
                    this.lblWidth.Visible = true;
                    this.nudWidth.Visible = true;
                    this.lblOutlineColor.Visible = true;
                    this.btnOutlineColor.Visible = true;
                    break;
            }
        }

        private void SymbologyCtr_OnItemSelected(object sender, ESRI.ArcGIS.Controls.ISymbologyControlEvents_OnItemSelectedEvent e)
        {

            pStyleGalleryItem = (IStyleGalleryItem)e.styleGalleryItem;
            Color color;
            switch (this.SymbologyCtr.StyleClass)
            {
                //点符号
                case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                    color = this.ConvertIRgbColorToColor(((IMarkerSymbol)pStyleGalleryItem.Item).Color as IRgbColor);
                    //设置按钮背景色
                    this.btnColor.BackColor = color;
                    //设置点符号角度和大小初始值
                    //this.nudAngle.Value = (decimal)((IMarkerSymbol)this.pStyleGalleryItem.Item).Angle;
                    this.nudSize.Value = (decimal)((IMarkerSymbol)this.pStyleGalleryItem.Item).Size;
                    break;
                //线符号
                case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                    IRgbColor rgbColor = ((ILineSymbol)pStyleGalleryItem.Item).Color as IRgbColor;
                    if (rgbColor!=null)
                    {
                        Console.WriteLine(((ILineSymbol)pStyleGalleryItem.Item).Color);
                        color = this.ConvertIRgbColorToColor(((ILineSymbol)pStyleGalleryItem.Item).Color as IRgbColor);
                        //设置按钮背景色
                        this.btnColor.BackColor = color;
                        //设置线宽初始值
                        this.nudWidth.Value = (decimal)((ILineSymbol)this.pStyleGalleryItem.Item).Width;
                    }
                    
                    break;
                //面符号
                case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                    //如果面符号选中的颜色不为渐变色，则设置按钮背景颜色
                    if (((IFillSymbol)pStyleGalleryItem.Item).Color as IRgbColor != null)
                    {
                        color = this.ConvertIRgbColorToColor(((IFillSymbol)pStyleGalleryItem.Item).Color as IRgbColor);
                        //设置按钮背景色
                        this.btnColor.BackColor = color;
                    }
                    this.btnOutlineColor.BackColor =
                    this.ConvertIRgbColorToColor(((IFillSymbol)pStyleGalleryItem.Item).Outline.Color as IRgbColor);
                    //设置外框线宽度初始值
                    this.nudWidth.Value = (decimal)((IFillSymbol)this.pStyleGalleryItem.Item).Outline.Width;
                    break;
                default:
                    color = Color.Black;
                    break;
            }
            //预览符号
            this.PreviewImage();
        }

        private void SymbologyCtr_OnDoubleClick(object sender, ESRI.ArcGIS.Controls.ISymbologyControlEvents_OnDoubleClickEvent e)
        {
            this.btnOK.PerformClick();
        }

        #region 将ArcGIS Engine中的IRgbColor接口转换至.NET中的Color结构
        /// <summary>
        /// 将ArcGIS Engine中的IRgbColor接口转换至.NET中的Color结构
        /// </summary>
        /// <param name="pRgbColor"></param>
        /// <returns></returns>
        public Color ConvertIRgbColorToColor(IRgbColor pRgbColor)
        {

            return ColorTranslator.FromOle(pRgbColor.RGB);

        }
        #region 把选中并设置好的符号在picturebox控件中预览
        /// <summary>
        /// 把选中并设置好的符号在picturebox控件中预览
        /// </summary>
        private void PreviewImage()
        {
            stdole.IPictureDisp picture = this.SymbologyCtr.GetStyleClass(this.SymbologyCtr.StyleClass).PreviewItem(pStyleGalleryItem, this.ptbPreview.Width, this.ptbPreview.Height);
            System.Drawing.Image image = System.Drawing.Image.FromHbitmap(new System.IntPtr(picture.Handle));
            this.ptbPreview.Image = image;
        }
        #endregion

        private void btnOK_Click(object sender, EventArgs e)
        {
            //取得选定的符号
            this.pSymbol = (ISymbol)pStyleGalleryItem.Item;
            //更新预览图像
            this.pSymbolImage = this.ptbPreview.Image;
            //关闭窗体
            this.Close();
        }
        #endregion

        private void btnCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region 将.NET中的Color结构转换至于ArcGIS Engine中的IColor接口
        /// <summary>
        /// 将.NET中的Color结构转换至于ArcGIS Engine中的IColor接口
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public IColor ConvertColorToIColor(Color color)
        {
            IColor pColor = new RgbColorClass();
            pColor.RGB = color.B * 65536 + color.G * 256 + color.R;
            return pColor;
        }
        #endregion

        private void btnColor_Click(object sender, EventArgs e)
        {

            //调用系统颜色对话框
            if (this.colorDialog.ShowDialog() == DialogResult.OK)
            {
                //将颜色按钮的背景颜色设置为用户选定的颜色
                this.btnColor.BackColor = this.colorDialog.Color;
                //设置符号颜色为用户选定的颜色
                switch (this.SymbologyCtr.StyleClass)
                {
                    //点符号
                    case esriSymbologyStyleClass.esriStyleClassMarkerSymbols:
                        ((IMarkerSymbol)this.pStyleGalleryItem.Item).Color = this.ConvertColorToIColor(this.colorDialog.Color);
                        break;
                    //线符号
                    case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                        ((ILineSymbol)this.pStyleGalleryItem.Item).Color = this.ConvertColorToIColor(this.colorDialog.Color);
                        break;
                    //面符号
                    case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                        ((IFillSymbol)this.pStyleGalleryItem.Item).Color = this.ConvertColorToIColor(this.colorDialog.Color);
                        break;
                }
                //更新符号预览
                this.PreviewImage();
            }
        }

        private void btnOutlineColor_Click(object sender, EventArgs e)
        {
            if (this.colorDialog.ShowDialog() == DialogResult.OK)
            {
                //取得面符号中的外框线符号
                ILineSymbol pLineSymbol = ((IFillSymbol)this.pStyleGalleryItem.Item).Outline;
                //设置外框线颜色
                pLineSymbol.Color = this.ConvertColorToIColor(this.colorDialog.Color);
                //重新设置面符号中的外框线符号
                ((IFillSymbol)this.pStyleGalleryItem.Item).Outline = pLineSymbol;
                //设置按钮背景颜色
                this.btnOutlineColor.BackColor = this.colorDialog.Color;
                //更新符号预览
                this.PreviewImage();
            }
        }

        private void btnMoreSymbols_Click(object sender, EventArgs e)
        {
            if (this.contextMenuMoreSymbolInitiated == false)
            {
                string path = m_OperateMap.getPath(filepath) + "\\Symbol\\Styles";
                //取得菜单项数量
                string[] styleNames = System.IO.Directory.GetFiles(path, "*.ServerStyle");
                ToolStripMenuItem[] symbolContextMenuItem = new ToolStripMenuItem[styleNames.Length + 1];
                //循环添加其它符号菜单项到菜单
                for (int i = 0; i < styleNames.Length; i++)
                {
                    symbolContextMenuItem[i] = new ToolStripMenuItem();
                    symbolContextMenuItem[i].CheckOnClick = true;
                    symbolContextMenuItem[i].Text = System.IO.Path.GetFileNameWithoutExtension(styleNames[i]);
                    if (symbolContextMenuItem[i].Text == "ESRI")
                    {
                        symbolContextMenuItem[i].Checked = true;
                    }
                    symbolContextMenuItem[i].Name = styleNames[i];
                }
                //添加“更多符号”菜单项到菜单最后一项
                symbolContextMenuItem[styleNames.Length] = new ToolStripMenuItem();
                symbolContextMenuItem[styleNames.Length].Text = "添加符号";
                symbolContextMenuItem[styleNames.Length].Name = "AddMoreSymbol";
                //添加所有的菜单项到菜单
                this.contextMenuStripMoreSymbol.Items.AddRange(symbolContextMenuItem);
                this.contextMenuMoreSymbolInitiated = true;
            }
            //显示菜单
            this.contextMenuStripMoreSymbol.Show(this.btnMoreSymbols.Location);
        }


        #region 更多符号”按钮弹出的菜单项单击事件
        /// <summary>
        /// “更多符号”按钮弹出的菜单项单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStripMoreSymbol_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem pToolStripMenuItem = (ToolStripMenuItem)e.ClickedItem;
            //如果单击的是“添加符号”
            if (pToolStripMenuItem.Name == "AddMoreSymbol")
            {
                //弹出打开文件对话框
                if (this.openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //导入style file到SymbologyControl 
                    this.SymbologyCtr.LoadStyleFile(this.openFileDialog.FileName);
                    //刷新axSymbologyControl控件
                    this.SymbologyCtr.Refresh();
                }
            }
            else//如果是其它选项
            {
                if (pToolStripMenuItem.Checked == false)
                {
                    this.SymbologyCtr.LoadStyleFile(pToolStripMenuItem.Name);
                    this.SymbologyCtr.Refresh();
                }
                else
                {
                    this.SymbologyCtr.RemoveFile(pToolStripMenuItem.Name);
                    this.SymbologyCtr.Refresh();
                }
            }
        }
        #endregion

        private void nudSize_ValueChanged(object sender, EventArgs e)
        {
            ((IMarkerSymbol)this.pStyleGalleryItem.Item).Size = (double)this.nudSize.Value;
            this.PreviewImage();
        }

        private void nudWidth_ValueChanged(object sender, EventArgs e)
        {
            switch (this.SymbologyCtr.StyleClass)
            {
                case esriSymbologyStyleClass.esriStyleClassLineSymbols:
                    ((ILineSymbol)this.pStyleGalleryItem.Item).Width = Convert.ToDouble(this.nudWidth.Value);
                    break;
                case esriSymbologyStyleClass.esriStyleClassFillSymbols:
                    //取得面符号的轮廓线符号
                    ILineSymbol pLineSymbol = ((IFillSymbol)this.pStyleGalleryItem.Item).Outline;
                    pLineSymbol.Width = Convert.ToDouble(this.nudWidth.Value);
                    ((IFillSymbol)this.pStyleGalleryItem.Item).Outline = pLineSymbol;
                    break;
            }
            this.PreviewImage();
        }

        private void SetFeatureClassStyle(esriSymbologyStyleClass symbologyStyleClass)
        {
            this.SymbologyCtr.StyleClass = symbologyStyleClass;
            ISymbologyStyleClass pSymbologyStyleClass = this.SymbologyCtr.GetStyleClass(symbologyStyleClass);
            if (this.pLegendClass != null)
            {
                IStyleGalleryItem currentStyleGalleryItem = new ServerStyleGalleryItem();
                currentStyleGalleryItem.Name = "当前符号";
                currentStyleGalleryItem.Item = pLegendClass.Symbol;
                pSymbologyStyleClass.AddItem(currentStyleGalleryItem, 0);
                this.pStyleGalleryItem = currentStyleGalleryItem;
            }
            pSymbologyStyleClass.SelectItem(0);
        }

        private void SetParameter_Load(object sender, EventArgs e)
        {
          
            //string path = Application.StartupPath + "\\ESRI.ServerStyle";

            string path = m_OperateMap.getPath(filepath) + @".\Symbol\ESRI.ServerStyle";

            this.SymbologyCtr.LoadStyleFile(path);  // 根据ESRI.ServerStyle的路径，将其载入SymbologyControl控件中        
            //确定图层的类型(点线面),设置好SymbologyControl的StyleClass,设置好各控件的可见性(visible) 
            IGeoFeatureLayer pGeoFeatureLayer = (IGeoFeatureLayer)pLayer;
            switch (((IFeatureLayer)pLayer).FeatureClass.ShapeType)
            {
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                    this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassMarkerSymbols);
                    //this.lblAngle.Visible = true;
                    //this.nudAngle.Visible = true;
                    this.lblSize.Visible = true;
                    this.nudSize.Visible = true;
                    this.lblWidth.Visible = false;
                    this.nudWidth.Visible = false;
                    this.lblOutlineColor.Visible = false;
                    this.btnOutlineColor.Visible = false;
                    break;
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                    this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassLineSymbols);
                    //this.lblAngle.Visible = false;
                    //this.nudAngle.Visible = false;
                    this.lblSize.Visible = false;
                    this.nudSize.Visible = false;
                    this.lblWidth.Visible = true;
                    this.nudWidth.Visible = true;
                    this.lblOutlineColor.Visible = false;
                    this.btnOutlineColor.Visible = false;
                    break;
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                    this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassFillSymbols);
                    //this.lblAngle.Visible = false;
                    //this.nudAngle.Visible = false;
                    this.lblSize.Visible = false;
                    this.nudSize.Visible = false;
                    this.lblWidth.Visible = true;
                    this.nudWidth.Visible = true;
                    this.lblOutlineColor.Visible = true;
                    this.btnOutlineColor.Visible = true;
                    break;
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryMultiPatch:
                    this.SetFeatureClassStyle(esriSymbologyStyleClass.esriStyleClassFillSymbols);
                    //this.lblAngle.Visible = false;
                    //this.nudAngle.Visible = false;
                    this.lblSize.Visible = false;
                    this.nudSize.Visible = false;
                    this.lblWidth.Visible = true;
                    this.nudWidth.Visible = true;
                    this.lblOutlineColor.Visible = true;
                    this.btnOutlineColor.Visible = true;
                    break;
                default:
                    this.Close();
                    break;
            }
           
            
        }

        public void getFields(ILayer Layer)
        {
            cmbSelField.Items.Clear();
            cmbSelField.Text = "";
            IField pField = null;
            IFeatureLayer featurelayer = Layer as IFeatureLayer;
            IFeatureClass pFeatCls = featurelayer.FeatureClass;

            for (int i = 0; i < pFeatCls.Fields.FieldCount; i++)
            {
                pField = pFeatCls.Fields.get_Field(i);
                if (pField.Type == esriFieldType.esriFieldTypeDouble ||
                    pField.Type == esriFieldType.esriFieldTypeInteger ||
                    pField.Type == esriFieldType.esriFieldTypeSingle ||
                    pField.Type == esriFieldType.esriFieldTypeSmallInteger ||
                    pField.Type == esriFieldType.esriFieldTypeString ||
                     pField.Type == esriFieldType.esriFieldTypeOID)
                {
                    if (!cmbSelField.Items.Contains(pField.Name))
                    {
                        cmbSelField.Items.Add(pField.Name);
                    }
                }
            }
        }
        //获取需要显示的字段
        public IField getField(ILayer Layer)
        {
            //cmbSelField.Items.Clear();
            //cmbSelField.Text = "";
            IField pField = null;
            IField qField = null;
            IFeatureLayer featurelayer = Layer as IFeatureLayer;
            IFeatureClass pFeatCls = featurelayer.FeatureClass;
            for (int i = 0; i < pFeatCls.Fields.FieldCount; i++)
            {
                pField = pFeatCls.Fields.get_Field(i);
                if (pField.Type == esriFieldType.esriFieldTypeDouble ||
                    pField.Type == esriFieldType.esriFieldTypeInteger ||
                    pField.Type == esriFieldType.esriFieldTypeSingle ||
                    pField.Type == esriFieldType.esriFieldTypeSmallInteger ||
                    pField.Type == esriFieldType.esriFieldTypeString ||
                     pField.Type == esriFieldType.esriFieldTypeOID)
                {
                    if (pField.Name == (cmbSelField.SelectedItem).ToString())
                    {
                        qField = pField;
                        break;
                    }
                }
            }
            return qField;
        }
        private void contextMenuStripMoreSymbol_ItemClicked_1(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem pToolStripMenuItem = (ToolStripMenuItem)e.ClickedItem;
            //如果单击的是“添加符号”
            if (pToolStripMenuItem.Name == "AddMoreSymbol")
            {
                //弹出打开文件对话框
                if (this.openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //导入style file到SymbologyControl 
                    this.SymbologyCtr.LoadStyleFile(this.openFileDialog.FileName);
                    //刷新axSymbologyControl控件
                    this.SymbologyCtr.Refresh();
                }
            }
            else//如果是其它选项
            {
                if (pToolStripMenuItem.Checked == false)
                {
                    this.SymbologyCtr.LoadStyleFile(pToolStripMenuItem.Name);
                    this.SymbologyCtr.Refresh();
                }
                else
                {
                    this.SymbologyCtr.RemoveFile(pToolStripMenuItem.Name);
                    this.SymbologyCtr.Refresh();
                }
            }
        }


        public ITextSymbol ziti()
        {
            ITextSymbol pTextSymbol = new TextSymbolClass();
            //object obj = ESRI.ArcGIS.ADF.COMSupport.OLE.GetIFontDispFromFont(this.fontDialog1.Font);
            //pTextSymbol.Font = obj as stdole.IFontDisp;
            //pTextSymbol.Color = this.ConvertColorToIColor(this.colorDialog.Color);
            //pTextSymbol.Font = (ESRI.ArcGIS.ADF.COMSupport.OLE.GetIFontDispFromFont(this.fontDialog1.Font)) as stdole.IFontDisp;
            //pTextSymbol.Font = this.fontDialog1.Font;
            //pTextSymbol.Color = this.colorDialog.Color;
            
            return pTextSymbol;
        }
         public void Annotation(IFeatureLayer pFeatLyr, string sFieldName)
        {
            try
            {
                IGeoFeatureLayer pGeoFeatLyer = pFeatLyr as IGeoFeatureLayer;
                IAnnotateLayerPropertiesCollection pAnnoProps = pGeoFeatLyer.AnnotationProperties;
                pAnnoProps.Clear();
                //设置标注记体格式                                                                     
                ITextSymbol pTextSymbol = new TextSymbolClass();
                object obj= ESRI.ArcGIS.ADF.COMSupport.OLE.GetIFontDispFromFont(this.button3.Font);
                pTextSymbol.Font = obj as stdole.IFontDisp;
                pTextSymbol.Color = this.ConvertColorToIColor(this.fontColor.BackColor);
                //设置注记放置格式
                ILineLabelPosition pPosition = new LineLabelPositionClass();
                pPosition.Parallel = false;
                pPosition.Perpendicular = true;
                ILineLabelPlacementPriorities pPlacement = new LineLabelPlacementPrioritiesClass();
                IBasicOverposterLayerProperties pBasic = new BasicOverposterLayerPropertiesClass();
                pBasic.FeatureType = esriBasicOverposterFeatureType.esriOverposterPolyline;
                pBasic.LineLabelPlacementPriorities = pPlacement;//设置标注文本摆设路径权重
                pBasic.LineLabelPosition = pPosition;//控制文本的排放位置
                ILabelEngineLayerProperties pLableEngine = new LabelEngineLayerPropertiesClass();
                pLableEngine.Symbol = pTextSymbol;
                pLableEngine.BasicOverposterLayerProperties = pBasic;//设置标注文本的放置方式，以及处理文字间冲突的处理方式等
                pLableEngine.Expression = "[" + sFieldName + "]";//输入VBScript或JavaScript语言，设置要标注的字段
                IAnnotateLayerProperties pAnnoLayerProps = pLableEngine as IAnnotateLayerProperties;
                pAnnoProps.Add(pAnnoLayerProps);
                pGeoFeatLyer.DisplayAnnotation = true;

                //axMapControl.Refresh(esriViewDrawPhase.esriViewBackground, null, null);
                //mainMapControl.Update();
            }
            catch (Exception ex)
            {

            }
        }
         //设置字体
         IFontAttribute fontAttribute;
         private void button3_Click_1(object sender, EventArgs e)
         {
             if (fontDialog1.ShowDialog() == DialogResult.OK)
             {
                 
             }
         }
        //字体颜色
        private void button3_Click(object sender, EventArgs e)
        {
            if (this.colorDialog.ShowDialog() == DialogResult.OK)
            {
               
            }
        }
        //确定
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //取消
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //董海峰。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。
        public void getPointSymbol()
        {
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            //创建RgbColorClass对象为pSimpleMarkerSymbol设置颜色

            IRgbColor pRgbColor = new RgbColorClass();

            pRgbColor.Red = 255;
            pSimpleMarkerSymbol.Color = pRgbColor as IColor;

            //设置pSimpleMarkerSymbol对象的符号类型，选择钻石
            pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;

            //设置pSimpleMarkerSymbol对象大小，设置为５
            pSimpleMarkerSymbol.Size = 5;

            //显示外框线
            pSimpleMarkerSymbol.Outline = true;

            //为外框线设置颜色

            IRgbColor pLineRgbColor = new RgbColorClass();
            pLineRgbColor.Green = 255;
            pSimpleMarkerSymbol.OutlineColor = pLineRgbColor as IColor;
            //设置外框线的宽度

            pSimpleMarkerSymbol.OutlineSize = 1;
            ISymbol symbol = pSimpleMarkerSymbol as ISymbol;
            //this.pSymbol = symbol;
        }

        public void getLineSymbol()
        {
            IArrowMarkerSymbol pArrowMarker = new ArrowMarkerSymbolClass();

            RgbColor pRgbColor = new RgbColorClass();
            pRgbColor.Red = 255;
            pArrowMarker.Color = pRgbColor;
            pArrowMarker.Length = 10;
            pArrowMarker.Width = 8;
            pArrowMarker.Style = esriArrowMarkerStyle.esriAMSPlain;

            IMarkerLineSymbol pMarkerLine = new MarkerLineSymbolClass();
            pMarkerLine.MarkerSymbol = pArrowMarker;

            IRgbColor pLineColor = new RgbColorClass();
            pLineColor.Blue = 255;
            pMarkerLine.Color = pLineColor;
            ISymbol symbol = pMarkerLine as ISymbol;
            //this.pSymbol = symbol;
        }

        public void getAreaSymbol()
        {
            IColor pLineColor = new RgbColorClass();
            ICartographicLineSymbol pCartoLineSymbol = new CartographicLineSymbolClass();

            pCartoLineSymbol.Width = 2;

            pCartoLineSymbol.Color = pLineColor;

            //创建一个填充符号

            ISimpleFillSymbol pSmplFillSymbol = new SimpleFillSymbol();

            //设置填充符号的属性

            IColor pRgbClr = new RgbColorClass();

            IFillSymbol pFillSymbol = pSmplFillSymbol;

            pFillSymbol.Color = pRgbClr;

            pFillSymbol.Outline = pCartoLineSymbol;
            ISymbol symbol = pFillSymbol as ISymbol;
            //this.pSymbol = symbol;

        }
        public void ss(AxMapControl mainMapControl, AxTOCControl mainTOCControl)
        {
            ILayer pLayer = new FeatureLayerClass();          
            pLayer = mainMapControl.get_Layer(0);
            IGeoFeatureLayer pGeoFeatLyr = pLayer as IGeoFeatureLayer;
            //设置面填充符号           
            ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
            //pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSVertical;//设置面填充为垂直线填充
            pSimpleFillSymbol.Color = GetRgbColor(155, 144, 144);
            //更改符号样式
            ISimpleRenderer pSimpleRenderer = new SimpleRendererClass();
            pSimpleRenderer.Symbol = pSimpleFillSymbol as ISymbol;
            pGeoFeatLyr.Renderer = pSimpleRenderer as IFeatureRenderer;
            mainMapControl.Refresh();
            mainTOCControl.Update();
        
        }

        public IRgbColor GetRgbColor(int intR, int intG, int intB)
        {
            IRgbColor pRgbColor = null;
            if (intR < 0 || intR > 255 || intG < 0 || intG > 255 || intB < 0 || intB > 255)
            {
                return pRgbColor;
            }
            pRgbColor = new RgbColorClass();
            pRgbColor.Red = intR;
            pRgbColor.Green = intG;
            pRgbColor.Blue = intB;
            return pRgbColor;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            IFeatureLayer featureLayer = new FeatureLayerClass();
            IGeoFeatureLayer geoFeatureLayer = (IGeoFeatureLayer)featureLayer;
            ISimpleRenderer simpleRenderer = (ISimpleRenderer)geoFeatureLayer.Renderer;
            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            //IRgbColor pRgbColor = new RgbColor();
            //pRgbColor = GetRgbColor(225, 100, 100);
            pFillSymbol.Color = ConvertColorToIColor(Color.Blue);
            simpleRenderer.Symbol = (ISymbol)pFillSymbol;
            //axMapControl1.ActiveView.Refresh();

            //getPointSymbol();
            //getLineSymbol();
            getAreaSymbol();
            
            //IFeatureSelection pFtSelection = pFeatureLayer as IFeatureSelection;
            //pFtSelection.SetSelectionSymbol = true;
            //pFtSelection.SelectionSymbol = (ISymbol)ipSimpleFillSymbol;        
            ////只用前五行，可以直接将选中的面要素的颜色全部修改成红色，也就是填充颜色
            //IRgbColor pRgbColor = new RgbColor(); ;
            //pRgbColor.Red = 255;
            //pRgbColor.Green = 0;
            //pRgbColor.Blue = 0;
            //IFeatureSelection pFtSelection = pFeatureLayer as IFeatureSelection;
            ////符号边线颜色 ，下边这五行设定选中面的边线，但是需要配合后边的代码执行
            //IRgbColor pLineColor = new RgbColor();
            //pLineColor.Red = 255;
            //ILineSymbol ilSymbl = new SimpleLineSymbolClass();
            //ilSymbl.Color = pLineColor;
            //ilSymbl.Width = 3;

            ////定义选中要素的符号为红色，这部分的作用并没有搞清楚，随后可能还需要研究 
            //ISimpleFillSymbol ipSimpleFillSymbol = new SimpleFillSymbol();
            //ipSimpleFillSymbol.Outline = ilSymbl;
            //RgbColor pFillColor = new RgbColor();
            //pFillColor.Green = 60;
            //ipSimpleFillSymbol.Color = pFillColor;
            //ipSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSNull;//设置成填充效果为空，也就是不填充，有很多填充效果，这是个枚举
            ////ipSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSForwardDiagonal;

            ////选取要素集 ，这里需要把pFeatureLayer 转换成IFeatureSelection 
            //IFeatureSelection pFtSelection = pFeatureLayer as IFeatureSelection;
            //pFtSelection.SetSelectionSymbol = true;
            //pFtSelection.SelectionSymbol = (ISymbol)ipSimpleFillSymbol;        
        }

       





    }
}