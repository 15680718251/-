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
    public partial class LineToPolygonForm : Form
    {
        public LineToPolygonForm()
        {
            InitializeComponent();
        }
        private string m_LayerName;

        public string LayerName
        {
            get { return m_LayerName; }            
        }

        public  LineToPolygonForm(MapUI  ui)
        {
            InitializeComponent();
            for(int i =0;i<ui.GroupCounts;i++)
            {
                GIS.Map.LayerGroup group = ui.GetGroupByIndex(i);
                for(int j =0;j<group.Counts;j++)
                {
                    GIS.Layer.GeoLayer layer = group[j];
                    if (layer.LayerTypeDetail == GIS.Layer.LAYERTYPE_DETAIL.PolygonLayer)
                    {
                        comboBox1.Items.Add(layer.LayerName);
                    }
                }
            }
            if(comboBox1.Items.Count>0)
                comboBox1.SelectedIndex = 0;
             
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_LayerName = comboBox1.Text;
        }
    }
}
