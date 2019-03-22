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
    public partial class AttributeQueryForm : Form
    {
        public bool Cancel = false;
        public int ElevaValue;   //高程
        public string PlgValue; //多边形类型
        public string newPlgValue;
        public TPType tpType;     //操作类型
        public string predicate;

        public enum TPType
        {
            Delete=0,
            ChangeType=1
        }

        public AttributeQueryForm()
        {
            InitializeComponent();

            #region comboBox1 添加内容
            this.comboBox1.Items.Add(">");
            this.comboBox1.Items.Add(">=");
            this.comboBox1.Items.Add("<");
            this.comboBox1.Items.Add("<=");
            this.comboBox1.Items.Add("=");
            this.comboBox1.SelectedIndex = 0;    //设置默认选中第一项
            #endregion

            tpType = TPType.Delete;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text.Trim() == ""||this.textBox2.Text.Trim()=="")
            {
                MessageBox.Show("输入条件不合适", "提示");
                Cancel = true;
                return;
            }

            try
            {
                ElevaValue = Convert.ToInt32(this.textBox2.Text.Trim());
                PlgValue = textBox3.Text.Trim();
                newPlgValue = this.textBox1.Text.Trim();
                predicate = this.comboBox1.Text.Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "提示");
                Cancel = true;
                return;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                groupBox3.Enabled = true;
                tpType = TPType.ChangeType;
            }
            else
            {
                groupBox3.Enabled = false;
                tpType = TPType.Delete;
            }
        }
    }
}
