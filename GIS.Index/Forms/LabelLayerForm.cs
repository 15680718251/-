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
    public partial class LabelLayerForm : Form
    {
        private LabelLayerForm()
        {
            InitializeComponent();
        }
        public LabelLayerForm(MapUI ui)
        {
            InitializeComponent();
            m_MapUI = ui;
            m_LayerList = ui.GetAllLabelLayer();

            for (int i = 0; i < m_LayerList.Count; i++)
            {
                comboBox1.Items.Add(m_LayerList[i].LayerName);
            }
            
 
            comboBox1.Text = ui.GetActiveLabelLayer().LayerName;
            GroupLabel.Text = ui.GetGroupByLayer(ui.GetActiveLabelLayer()).LayerGroupName;
        }
        private MapUI m_MapUI;
        private List<MapUI.LayerInfo> m_LayerList;

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GroupLabel.Text = m_LayerList[comboBox1.SelectedIndex].GroupName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_MapUI.SetActiveLayer(GroupLabel.Text, comboBox1.Text);
        }
      
    }
}
