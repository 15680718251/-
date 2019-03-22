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
    public partial class SaveCurLineSymForm : Form
    {
        public SaveCurLineSymForm(MapUI ui)
        {
            InitializeComponent();
            m_ui = ui;
            Initial();
        }

        public string m_name = "Default";
        public int m_id = 999999;
        public bool m_blinesymchecked = false;
        public bool m_blineelechecked = false;
        private MapUI m_ui;

        private void Initial()
        {
            this.checkBox_linesym.Checked = true;
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

            if (this.checkBox_lineele.Checked)
            {
                for (int i = 0; i < m_ui.m_conv_gtr.lineelefeatid.Count; i++)
                {
                    if (id == m_ui.m_conv_gtr.lineelefeatid[i])
                    {
                        MessageBox.Show("此ID已存在，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            if (this.checkBox_linesym.Checked)
            {
                for (int i = 0; i < m_ui.m_conv_gtr.linefeatid.Count; i++)
                {
                    if (id == m_ui.m_conv_gtr.linefeatid[i])
                    {
                        MessageBox.Show("此ID已存在，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            m_id = id;
            m_name = name;

            m_blinesymchecked = this.checkBox_linesym.Checked;
            m_blineelechecked = this.checkBox_lineele.Checked;

            this.DialogResult = DialogResult.OK;
        }





    }
}
