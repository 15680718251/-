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
    public partial class InputXYForm : Form
    {
        public InputXYForm()
        {
            InitializeComponent();
        }
        private double m_X;
        private double m_Y;

        public double X
        {
            get { return m_X; }
            set { m_X = value; }
        }
        public double Y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            double x,y;
            if(double.TryParse(textBoxX.Text,out x) && double.TryParse(textBoxY.Text,out y))
            {
                m_X = x;
                m_Y = y;
            }
        }
    }
}
