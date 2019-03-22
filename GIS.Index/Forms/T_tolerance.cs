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
    public partial class T_tolerance : Form
    {
        public float m_t_tolerance = 0.15f;
        public T_tolerance()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            m_t_tolerance = System.Convert.ToSingle(T_num.Value);
            this.DialogResult = DialogResult.OK;
        }
    }
}
