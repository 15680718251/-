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
    public partial class InputTextBox : Form
    {
        private string m_TextValue="";
        private Type m_Type;

        //#region 2012.09.26添加属性
        //public Type Type
        //{
        //    get { return m_Type; }
        //}
        //#endregion


        public string TextValue
        {
            get { return m_TextValue; }
            set { m_TextValue = value; }
        }
        public InputTextBox()
        {
            InitializeComponent();
        }
        public InputTextBox(Type type)
        {
            InitializeComponent();
            m_Type = type;
        }

        #region 暂时解决，可以想想更好的解决办法
        private void button1_Click(object sender, EventArgs e)
        {
           
            //TextValue = textBox1.Text;
            if (textBox1.Text == "")
            { 
                DialogResult dialogResult=MessageBox.Show("未输入数据","注意",MessageBoxButtons.OK);
                if(dialogResult==DialogResult.OK||dialogResult==DialogResult.Cancel)
                {
                    textBox1.Text = "-10000";
                    TextValue = textBox1.Text;
                    this.button1_Click(sender,e);
                }
               
            }
            else
            {
                if (TextValue == "-10000")
                {                    
                    TextValue = textBox1.Text;
                }
                else
                {
                    TextValue = textBox1.Text;
                }
            
            }


        }
        #endregion


        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (m_Type == typeof(int)
             || m_Type == typeof(Int64)
             || m_Type == typeof(Int16)             
            )
            {
                if (
                    (e.KeyChar < (char)47|| e.KeyChar > (char)58)                
                    &&(e.KeyChar !=(char)8)
                    )
                    e.Handled = true;
            }
            else if (m_Type == typeof(double) || m_Type == typeof(float))
            {
                if (
                    (e.KeyChar < (char)47 || e.KeyChar > (char)58)
                    &&(e.KeyChar !=(char)46)
                    && (e.KeyChar != (char)8)
                    )
                    e.Handled = true;
            }
        }
    }
}
