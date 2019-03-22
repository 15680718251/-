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
    public partial class SaveCurPtSymForm : Form
    {
        public SaveCurPtSymForm(MapUI ui)
        {
            InitializeComponent();
            m_ui = ui;
            Initial();

        }

        public string m_name = "Default";
        public int m_id = 999999;
        public bool m_bptsymchecked = false;
        public bool m_bptelechecked = false;
        private MapUI m_ui;

        private void Initial()
        {
            this.checkBox_ptsym.Checked = true;
        }

        private void button_ok_Click(object sender, EventArgs e)
        {

            int id = System.Convert.ToInt32(this.id.Value);  
            string name = this.name.Text;

            if (id <= 0)
            {
                MessageBox.Show("id不对", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (name == "")
            {
                MessageBox.Show("请输入name", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this.checkBox_ptele.Checked)
            {
                for (int i = 0; i < m_ui.m_conv_gtr.ptelefeatid.Count; i++)
                {
                    if (id == m_ui.m_conv_gtr.ptelefeatid[i])
                    {
                        MessageBox.Show("此ID已存在，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            if (this.checkBox_ptsym.Checked)
            {
                for (int i = 0; i < m_ui.m_conv_gtr.pointfeatid.Count; i++)
                {
                    if (id == m_ui.m_conv_gtr.pointfeatid[i])
                    {
                        MessageBox.Show("此ID已存在，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }


            m_id = id;
            m_name = name;

            m_bptsymchecked = this.checkBox_ptsym.Checked;
            m_bptelechecked = this.checkBox_ptele.Checked;

            this.DialogResult = DialogResult.OK;
        }


    }
}
