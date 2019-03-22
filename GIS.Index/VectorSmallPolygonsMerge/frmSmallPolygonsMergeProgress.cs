using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.VectorSmallPolygonsMerge
{
    public partial class frmSmallPolygonsMergeProgress : Form
    {

        public frmSmallPolygonsMergeProgress()
        {
            InitializeComponent();
            this.Height = 170;
        }

        private void frmVectorizeProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        //执行完成时，
        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
