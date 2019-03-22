using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.UI.AdditionalTool;

namespace QualityControl
{
    public partial class QualityControlForm : Form
    {
        public QualityControlForm()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AttributeAccuracy attAcc=new AttributeAccuracy();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PositionAccuracy posAcc = new PositionAccuracy();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IncrementalInformation form = new IncrementalInformation();
            form.ShowDialog();

        }


    }
}
