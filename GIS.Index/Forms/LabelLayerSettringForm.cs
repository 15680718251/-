using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using GIS.Layer;
namespace GIS.TreeIndex.Forms
{
    public partial class LabelLayerSettringForm : Form
    {
        public  class LabelLyrInfo
        {
            public string LayerName;
            public int Index  ;
            public string FontName = "未改变";
            public double FontSize = 0 ;
            public Color FontClr  =  Color.Transparent;
        }
        public LabelLayerSettringForm(MapUI ui)
        {
            InitializeComponent();
            m_MapUI = ui;
            LoadParameters();
        }
        private MapUI m_MapUI;
        private List<LabelLyrInfo> m_LabelInfoList = new List<LabelLyrInfo>();
   

        private void LoadParameters()
        {
            InstalledFontCollection MyFont = new InstalledFontCollection();
            FontFamily[] MyFontFamilies = MyFont.Families;
            int Count = MyFontFamilies.Length;
            for (int i = 0; i < Count; i++)
            {
                string fName = MyFontFamilies[i].Name;
                comboBoxFontName.Items.Add(fName);
            }
            List<MapUI.LayerInfo> m_LayerList = m_MapUI.GetAllLabelLayer();
            for (int i = 0; i < m_LayerList.Count; i++)
            {
                listBoxLabelLyrs.Items.Add(m_LayerList[i].LayerName);
                LabelLyrInfo info = new LabelLyrInfo();
                info.Index = i;
                info.LayerName = m_LayerList[i].LayerName;
                m_LabelInfoList.Add(info); 
            } 
        }
       
        private void listBoxLabelLyrs_SelectedIndexChanged(object sender, EventArgs e)
        {
           int index = listBoxLabelLyrs.SelectedIndex;
           comboBoxFontName.Text = m_LabelInfoList[index].FontName;
            numericUpDown1.Value = (decimal)m_LabelInfoList[index].FontSize ;
           buttonFontColor.BackColor = m_LabelInfoList[index].FontClr;
        }

        private void comboBoxFontName_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBoxLabelLyrs.SelectedIndex;
            if (index == -1)
            {
                MessageBox.Show("请选择注记层","提示");
                return;
            }
            m_LabelInfoList[index].FontName = comboBoxFontName.Text;
        }
   

        private void buttonFontColor_Click(object sender, EventArgs e)
        {
            int index = listBoxLabelLyrs.SelectedIndex;
            if (index == -1)
            {
                MessageBox.Show("请选择注记层", "提示");
                return;
            }
            ColorDialog dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                buttonFontColor.BackColor = dlg.Color;
               
                m_LabelInfoList[index].FontClr = dlg.Color;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int index = listBoxLabelLyrs.SelectedIndex; 
            if (index == -1)
            {
                MessageBox.Show("请选择注记层", "提示");
                return;
            }
            m_LabelInfoList[index].FontSize = (double)numericUpDown1.Value;
        }
        private bool LabelLyrEdit(LabelLyrInfo info)
        {
            if (info.FontName != "未改变" ||
                   info.FontSize != 0 ||
                   info.FontClr != Color.Transparent)
            {
                GeoVectorLayer lyr = m_MapUI.GetLayerByName(info.LayerName) as GeoVectorLayer;
                for (int i = 0; i < lyr.DataTable.Count; i++)
                {
                    GeoData.GeoDataRow row = lyr.DataTable[i];
                    Geometries.GeoLabel label = row.Geometry as Geometries.GeoLabel;
                    if (info.FontName != "未改变")
                    {
                        label.FontName = info.FontName;
                    }
                    if (info.FontSize != 0)
                    {
                        label.TextSize = info.FontSize;
                    }
                    if (info.FontClr != Color.Transparent)
                    {
                        label.Color = info.FontClr;
                    }

                }
                return true;
            }
            return false;
           
        }
        private void button2_Click(object sender, EventArgs e)
        {
            bool Changed = false;
            for (int i = 0; i < m_LabelInfoList.Count; i++)
            {
               LabelLyrInfo info = m_LabelInfoList[i];

               if (LabelLyrEdit(info))
                   Changed = true;
            }
            if (Changed)
                m_MapUI.Refresh();
        }
    }
}
