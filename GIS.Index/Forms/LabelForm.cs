using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GIS.Layer;
using GIS.TreeIndex;
using GIS.GeoData;
using GIS.Geometries;
namespace GIS.TreeIndex.Forms
{
    public partial class LabelForm : Form
    {
        public LabelForm()
        {
            InitializeComponent();
        }
        public LabelForm(MapUI  ui)
        {
            InitializeComponent();
            MapUI = ui;

            #region initialform
            GeoLayer lyr = ui.GetActiveLabelLayer();
            if (lyr != null)
                lyrSltButton.Text = ui.GetActiveLabelLayer().LayerName;
            else
                lyrSltButton.Text = "创建注记层";
            textBox.Text = LabelText;
            if (!IsNorth)
                radioButton2.Checked = true;
            #endregion
        }

        public LabelForm(MapUI ui, GeoDataRow labelRow)
        {
            InitializeComponent();
            MapUI = ui;
           
            GeoLabel label = labelRow.Geometry as GeoLabel;
            if (label.StartPt.Y != label.EndPt.Y)
            {
                radioButton2.Checked = true;
            }
            LabelText = label.Text;
            TextSize = label.TextSize;
            FontName = label.FontName;
            Color = label.Color; 
            textBox.Text = LabelText;

        }

        private MapUI m_MapUI;
        public static string LabelText = "砖"; //文本内容
        public static bool IsNorth = true;  //字体方向
        public static double TextSize = 16.0;//字体大小
        public static string FontName = "楷体_GB2312";//字体名字
        public static Color Color = Color.Red;
        public bool LayerExits;
       
        public MapUI MapUI
        {
            get { return m_MapUI; }
            set { m_MapUI = value; }
        }

        private void buttonYes_Click(object sender, EventArgs e)
        {
            LabelText = textBox.Text;
            IsNorth = radioButton1.Checked;
            if (lyrSltButton.Text == "创建注记层")
            {

                GeoLayer lyr = m_MapUI.CreateLayer(LAYERTYPE_DETAIL.LabelLayer);
                if (lyr != null)
                {
                    lyrSltButton.Text = Path.GetFileNameWithoutExtension(lyr.LayerName);
                    LayerExits = true;
                }
                else
                {
                    LayerExits = false;
                }
            }
            else LayerExits = true;
        }

        private void lyrSltButton_Click(object sender, EventArgs e)
        {
            if (lyrSltButton.Text == "创建注记层")
            {
                
                  GeoLayer lyr=  m_MapUI.CreateLayer(LAYERTYPE_DETAIL.LabelLayer);
                  if (lyr != null)
                      lyrSltButton.Text = Path.GetFileNameWithoutExtension(lyr.LayerName);
                
            }
            else
            {
                LabelLayerForm form = new LabelLayerForm(m_MapUI);
                form.ShowDialog();//设置当前活动注记层              
            }
        }

        private void fontStyleButton_Click(object sender, EventArgs e)
        {
            FontOptionForm form = new FontOptionForm(FontName,TextSize,Color);
            if (form.ShowDialog() == DialogResult.OK)
            {
                TextSize = FontOptionForm.FontSize;
                FontName = FontOptionForm.FontName;
                Color = FontOptionForm.FontColor;
            }
            
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 1)
            {
                textBox.Text = this.listView1.SelectedItems[0].Text.ToString();
            }             
        }


 
    }
}
