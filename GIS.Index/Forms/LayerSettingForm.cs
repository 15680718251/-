using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.Layer;
using GIS.Geometries;
using GIS.Render;
namespace GIS.TreeIndex.Forms
{
    public partial class LayerSettingForm : Form
    {
        private LayerSettingForm()
        {
            InitializeComponent();
        }
        private MapUI m_MapUI;
        private GeoLayer layer;

        private float m_SymbolSize  ;    // 点符号的大小
        private float m_LineSize  ;   // 线符号宽度
        private Color m_OutLineColor ;//轮廓线的颜色
        private Color m_FillColor  ;  //填充色
        private int m_FillAlpha;    // 填充透明度 0-255之间

        private long m_ClasID;

        private double m_MaxVisible;
        private double m_MinVisible;
        public LayerSettingForm(MapUI ui,string strLayerName)
        {
            m_MapUI = ui; 
            InitializeComponent();
            InitialParameter(strLayerName);
            ReadClasID();
            SymbolRender();           
        }

        public void ReadClasID()
        {
            foreach (var item in MapUI.FeatureCodes)
            {
                string strTemp = string.Format("{0} {1}", item.Key.ToString(), item.Value);
                int index = comboBoxClasID.Items.Add(strTemp);
                if (item.Key % 100000 == 0)
                {
                    comboBox1.Items.Add(strTemp);
                }
                if (item.Key.ToString() == m_ClasID.ToString())
                {
                    comboBoxClasID.SelectedIndex = index;
                }
            }
            if (comboBoxClasID.SelectedIndex == -1)
            {
                comboBoxClasID.Text = m_ClasID.ToString();
            }
        }
        private void InitialParameter(string strLayerName)
        {
            layer = m_MapUI.GetLayerByName(strLayerName);
            this.Text = layer.LayerName + "层参数设置";
            if (layer.LayerBound != null)
            {
                labelX.Text = layer.LayerBound.LeftBottomPt.X.ToString("f4");
                labelY.Text = layer.LayerBound.LeftBottomPt.Y.ToString("f4");
                labelXLen.Text = layer.LayerBound.Width.ToString("f4");
                labelYLen.Text = layer.LayerBound.Height.ToString("f4");
            }
            if (layer is GeoVectorLayer)
            {
                GeoVectorLayer vlyr = layer as GeoVectorLayer;
                m_SymbolSize = vlyr.LayerStyle.SymbolSize;
                m_LineSize = vlyr.LayerStyle.LineSize;
                m_OutLineColor = vlyr.LayerStyle.LineColor;
                m_FillColor = vlyr.LayerStyle.FillColor;
                m_FillAlpha = vlyr.LayerStyle.FillColor.A;
                m_MaxVisible = vlyr.LayerStyle.MaxVisible;
                m_MinVisible = vlyr.LayerStyle.MinVisible;
                m_ClasID = vlyr.ClasID;
                buttonLineClr.BackColor = m_OutLineColor;
                buttonFillClr.BackColor = m_FillColor;
                numericUpDownPoint.Value = (decimal)m_SymbolSize  ;
                numericUpDownLine.Value =(decimal) m_LineSize;             
                trackBar1.Value = m_FillAlpha;
                if (vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
                {                    
                    numericUpDownPoint.Enabled = false;
                    buttonFillClr.Enabled = false;
                    trackBar1.Enabled = false;
                    labelLayerType.Text = "线图层";
             
                }
                else if (vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.PolygonLayer)
                {
                    numericUpDownPoint.Enabled = false;
                    labelLayerType.Text = "面图层";
                }
                else if(vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.PointLayer)
                {
                    labelLayerType.Text = "点图层";
                }
                else if (vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.MixLayer)
                {
                    labelLayerType.Text = "混合图层";
                }
                else if (vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.SurveyLayer)
                {
                    labelLayerType.Text = "观测点层";
                }
                else if (vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.DraftLayer)
                {
                    labelLayerType.Text = "草图层";
                }
                else if (vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.LabelLayer)
                {
                    labelLayerType.Text = "注记层";
                } 
                textBoxMaxVisible.Text = vlyr.LayerStyle.MaxVisible.ToString();
                textBoxMinVisible.Text = vlyr.LayerStyle.MinVisible.ToString();
            }
            if (layer is GeoRasterLayer)
            {
                GeoRasterLayer vlyr = layer as GeoRasterLayer;
                int j = vlyr.GetRasterLayerCount();
                for (int count = 0; count < vlyr.GetRasterLayerCount(); count++)
                {
                    comboBoxR.Items.Add("第" + (count + 1) + "波段");
                    comboBoxG.Items.Add("第" + (count + 1) + "波段");
                    comboBoxB.Items.Add("第" + (count + 1) + "波段");
                }
                comboBoxR.SelectedIndex = vlyr.bandcolors.Red.GetBand() - 1;
                comboBoxG.SelectedIndex = vlyr.bandcolors.Green.GetBand() - 1;
                comboBoxB.SelectedIndex = vlyr.bandcolors.Blue.GetBand() - 1;
            }
        }
        private void SymbolRender()
        {
            GeoVectorLayer vlyr = layer as GeoVectorLayer;
            if (vlyr == null)
                return;
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            Pen pen = new Pen(m_OutLineColor, m_LineSize);
            
            SolidBrush brush = new SolidBrush(Color.FromArgb(m_FillAlpha,m_FillColor));
            if (vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.PointLayer)
            {              
                RectangleF rectf = new RectangleF(pictureBox1.Width / 2 - m_SymbolSize, pictureBox1.Height / 2 - m_SymbolSize, 2 * m_SymbolSize, 2 * m_SymbolSize);
                g.FillEllipse(brush, rectf);
                g.DrawEllipse(pen, rectf);
            }
            else if (vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.LineLayer)
            {
                g.DrawLine(pen,10, pictureBox1.Height / 2, pictureBox1.Width-10, pictureBox1.Height / 2);           
            }
            else if (vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.PolygonLayer)
            {
                Point[] pts=  new Point[4];
                pts[0].X=10;
                pts[0].Y=10;
                pts[1].X = pictureBox1.Width-10;
                pts[1].Y = 10;
                pts[2].X= pictureBox1.Width-10;
                pts[2].Y = pictureBox1.Height -10;
                pts[3].X = 10;
                pts[3].Y = pictureBox1.Height-10;
                g.DrawPolygon(pen, pts);
                g.FillPolygon(brush, pts);                         
            }
            else if (vlyr.LayerTypeDetail == LAYERTYPE_DETAIL.MixLayer)
            {
                RectangleF rectf = new RectangleF(pictureBox1.Width / 2 - m_SymbolSize, pictureBox1.Height / 8 - m_SymbolSize, 2 * m_SymbolSize, 2 * m_SymbolSize);
                g.FillEllipse(brush, rectf);
                g.DrawEllipse(pen, rectf);

                g.DrawLine(pen, 10, pictureBox1.Height / 3, pictureBox1.Width - 10, pictureBox1.Height /3);           
              
                Point[] pts = new Point[4];
                pts[0].X = 10;
                pts[0].Y = pictureBox1.Height / 2;
                pts[1].X = pictureBox1.Width - 10;
                pts[1].Y = pictureBox1.Height / 2;

                pts[2].X = pictureBox1.Width - 10;
                pts[2].Y = pictureBox1.Height - 10;
                pts[3].X = 10;
                pts[3].Y = pictureBox1.Height - 10;
                g.DrawPolygon(pen, pts);
                g.FillPolygon(brush, pts); 
            }
            pictureBox1.Image = bmp;
            g.Dispose();
            pen.Dispose();
            brush.Dispose();
            pictureBox1.Refresh();
           
        }

        private void buttonLineClr_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                m_OutLineColor = dlg.Color;
                SymbolRender();
                buttonLineClr.BackColor = dlg.Color;
            }           
        }

        private void buttonFillClr_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                m_FillColor = dlg.Color;
                SymbolRender();
                buttonFillClr.BackColor = dlg.Color;
            }
        }

        private void numericUpDownPoint_ValueChanged(object sender, EventArgs e)
        {

            m_SymbolSize = (float)numericUpDownPoint.Value;
            SymbolRender();
        }

        private void numericUpDownLine_ValueChanged(object sender, EventArgs e)
        {
            m_LineSize = (float)numericUpDownLine.Value;
            SymbolRender();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            m_FillAlpha = trackBar1.Value;
            SymbolRender();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (layer is GeoVectorLayer)
            {
                GeoVectorLayer vlyr = layer as GeoVectorLayer;
                vlyr.LayerStyle.SymbolSize = m_SymbolSize;    // 点符号的大小
                vlyr.LayerStyle.LineSize = m_LineSize;   // 线符号宽度
                vlyr.LayerStyle.LineColor = m_OutLineColor;//轮廓线的颜色
                vlyr.LayerStyle.FillColor = Color.FromArgb(m_FillAlpha, m_FillColor);  //填充色
                vlyr.LayerStyle.MinVisible = m_MinVisible;
                vlyr.LayerStyle.MaxVisible = m_MaxVisible;

                string[] strTemp =  comboBoxClasID.Text.Split(' ');
                long clasID;
                if(long.TryParse(strTemp[0], out clasID)) 
                   vlyr.ClasID = clasID;

                m_MapUI.Refresh();
            }

            //如果是栅格图层
            if (layer is GeoRasterLayer)
            {
                GeoRasterLayer vlyr = layer as GeoRasterLayer;
                vlyr.bandcolors.Red = vlyr.m_GdalDataset.GetRasterBand(comboBoxR.SelectedIndex + 1);
                vlyr.bandcolors.Green = vlyr.m_GdalDataset.GetRasterBand(comboBoxG.SelectedIndex + 1);
                vlyr.bandcolors.Blue = vlyr.m_GdalDataset.GetRasterBand(comboBoxB.SelectedIndex + 1);
                m_MapUI.Refresh();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            long ulCode = (comboBox1.SelectedIndex + 1) * 100000;
            comboBoxClasID.Items.Clear();
            foreach (var item in MapUI.FeatureCodes)
            {
                string strTemp = string.Format("{0} {1}", item.Key.ToString(), item.Value);

                if (item.Key >= ulCode && item.Key < ulCode + 100000)
                {
                    comboBoxClasID.Items.Add(strTemp);
                }
            }
            comboBoxClasID.SelectedIndex = 0;
        }
    }
}
