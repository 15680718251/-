using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.Index
{
    public partial class FrmCreateIndex : Form
    {
        public FrmCreateIndex()
        {
            InitializeComponent();
        }
        public FrmCreateIndex(string idxHeight,string idxNdNum,string ndAvgNum,string crtIdxTime)
        {
            InitializeComponent();
            this.IdxHeight.Text = idxHeight;
            this.IdxNdNum.Text = idxNdNum;
            this.NdAvgNum.Text = ndAvgNum;
            this.CrtIdxTime.Text = crtIdxTime;
        }
        private void IDOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
