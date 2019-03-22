using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.Forms
{
    public partial class PolygonToLineFrm : Form
    {
        public PolygonToLineFrm()
        {
            InitializeComponent();
        }


        private string m_LayerName="";

        public string LayerName
        {
            get { return m_LayerName; }            
        }

        public PolygonToLineFrm(MapUI ui)
        {
            InitializeComponent();
            for(int i =0;i<ui.GroupCounts;i++)
            {
                GIS.Map.LayerGroup group = ui.GetGroupByIndex(i);
                for(int j =0;j<group.Counts;j++)
                {
                    GIS.Layer.GeoLayer layer = group[j];
                    if (layer.LayerTypeDetail == GIS.Layer.LAYERTYPE_DETAIL.LineLayer)
                    {
                        comboBox1.Items.Add(layer.LayerName);
                    }
                }
            }
            if(comboBox1.Items.Count>0)
                comboBox1.SelectedIndex = 0;
             
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            m_LayerName = comboBox1.Text;
        }

      
    }
}
