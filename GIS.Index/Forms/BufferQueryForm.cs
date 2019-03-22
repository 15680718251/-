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
    public partial class BufferQueryForm : Form
    {
        public enum TPType
        {
            TpContains = 0,
            TpIntersect = 1
        }

        public string distance;
        public TPType TpType;

        public BufferQueryForm()
        {
            InitializeComponent();
            this.buttonOK.Enabled = false;  //防止输入完信息前点此键
            this.textBox1.Tag = false;      //控件状态
            this.textBox1.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxEmpty_Validating);
            this.textBox1.TextChanged += new System.EventHandler(this.textBox_TextChanged);

            this.radioButton1.Checked = true;
            TpType = TPType.TpIntersect;
        }

        private void textBoxEmpty_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.Length == 0)
            {
                tb.BackColor = Color.Red;
                tb.Tag = false;
            }
            else
            {
                tb.BackColor = System.Drawing.SystemColors.Window;
                tb.Tag = true;
            }
            ValidateOK();
        }

        private void textBox_TextChanged(object sender, System.EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.Length == 0)
            {
                tb.BackColor = Color.Red;
                tb.Tag = false;
            }
            else
            {
                tb.Tag = true;
            }
            ValidateOK();
        }
        private void ValidateOK()
        {
            this.buttonOK.Enabled = (bool)(this.textBox1.Tag);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            distance = this.textBox1.Text;
            if (this.radioButton1.Checked)
            {
                TpType = TPType.TpIntersect;
            }
            else
            {
                TpType = TPType.TpContains;
            }
        }
    }
}
