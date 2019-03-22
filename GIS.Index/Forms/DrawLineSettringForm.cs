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
    public partial class DrawLineSettringForm : Form
    {
        public DrawLineSettringForm()
        {
            InitializeComponent();
        }
        public DrawLineSettringForm(MapUI ui)
        {
            InitializeComponent();
            m_MapUI = ui;
            checkBoxSnap.Checked = ui.LineAngleSnapEnable;

            numericUpDownSnapAngle.Value = (decimal)(ui.SnapAngle);
            numericUpDownAngleToler.Value = (decimal)(ui.AngleToler);
            numericUpDownVertexToler.Value = (decimal)(ui.SnapPixels);
            if (!checkBoxSnap.Checked)
            {
                numericUpDownSnapAngle.Enabled = false;
                numericUpDownVertexToler.Enabled = false;
                numericUpDownAngleToler.Enabled = false;
            }
        }
        private MapUI m_MapUI;
       
        private void button1_Click(object sender, EventArgs e)
        {
            m_MapUI.SnapAngle = (double)numericUpDownSnapAngle.Value;
            m_MapUI.SnapPixels = (int)numericUpDownVertexToler.Value;
            m_MapUI.AngleToler = (double)numericUpDownAngleToler.Value;
            m_MapUI.LineAngleSnapEnable = checkBoxSnap.Checked;
        }

        private void checkBoxSnap_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBoxSnap.Checked)
            {
                numericUpDownSnapAngle.Enabled = false;
                numericUpDownVertexToler.Enabled = false;
                numericUpDownAngleToler.Enabled = false;
            }
            else
            {
                numericUpDownSnapAngle.Enabled = true;
                numericUpDownVertexToler.Enabled = true;
                numericUpDownAngleToler.Enabled = true;
            }
        }
    }
}
