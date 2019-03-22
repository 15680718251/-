using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.VectorUpdate
{
    public partial class frmVectorUpdateProgress : Form
    {

        public frmVectorUpdateProgress()
        {
            InitializeComponent();
            this.Height = 170;
            this.btnDetail.Text = "<< 详情";
        }
        
        private void btnDetail_Click(object sender, EventArgs e)
        {
            if (this.btnDetail.Text == "<< 详情")
            {
                this.Height = 353;
                this.btnDetail.Text = "<< 简要";
            }
            else
            {
                this.Height = 170;
                this.btnDetail.Text = "<< 详情";
            }
        }

        private void frmVectorizeProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }


        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            this.txtDescription.Select(this.txtDescription.TextLength,0);
            this.txtDescription.ScrollToCaret();
        }

        //执行完成时，
        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

    }
}
