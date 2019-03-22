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
    public partial class AreaQueryForm : Form
    {
        public double AreaValue;
        public bool Cancel = false;
        public string sel = string.Empty;
        public AreaQueryForm()
        {
            InitializeComponent();
            #region comboBox1 添加内容
            this.comboBox1.Items.Add("<");
            this.comboBox1.Items.Add(">=");
            this.comboBox1.Items.Add(">");
            this.comboBox1.Items.Add("<=");
            this.comboBox1.Items.Add("=");
            this.comboBox1.SelectedIndex = 0;    //设置默认选中第一项
            #endregion
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.comboBox1.Text.Trim() == "" || this.textBox2.Text.Trim() == "")
            {
                MessageBox.Show("请选择合适的条件", "提示");
                Cancel = true;
                return;
            }

            try
            {
                AreaValue = Convert.ToDouble(this.textBox2.Text.Trim());
                sel = this.comboBox1.Text.Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"提示");
                Cancel = true;
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

    }
}
