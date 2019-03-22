using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.Vectorize
{
    public partial class frmVectorizeProgress : Form
    {
        private bool m_Cancel = false;
        public bool Cancel
        {
            get { return m_Cancel; }
            set { m_Cancel = value; }
        }

        public frmVectorizeProgress()
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
            m_Cancel = true;
            e.Cancel = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.btnCancel.Text == "取消")
            {
                m_Cancel = true;
                this.lblDescription.Text = "正在取消...";
            }
            else if (this.btnCancel.Text == "确定")
            {
                this.Dispose();//this.Close();不起作用，由于FormClosing事件的影响
            }
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            this.txtDescription.Select(this.txtDescription.TextLength,0);
            this.txtDescription.ScrollToCaret();
        }
    }
}
