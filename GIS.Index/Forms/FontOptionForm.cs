using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.Forms
{
    public partial class FontOptionForm : Form
    {
        public static Color FontColor = Color.GreenYellow;
        public static double FontSize = 16.0;
        public static string FontName = "楷体_GB2312";
        public FontOptionForm(string fontName,double textSize,Color clr)
        {
            InitializeComponent();
            InstalledFontCollection MyFont = new InstalledFontCollection();
            FontFamily[] MyFontFamilies = MyFont.Families;
            int Count = MyFontFamilies.Length;
            for (int i = 0; i < Count; i++)
            {
                string fName = MyFontFamilies[i].Name;
                comboBox1.Items.Add(fName);
            }
            FontName = fontName;
            FontSize = textSize;
            FontColor = clr;
            comboBox1.Text = fontName;
            textBox1.Text = textSize.ToString() ;
            button1.BackColor = clr;

        }
  
 
        private void button2_Click(object sender, EventArgs e)
        {
           double size;
           if (double.TryParse(textBox1.Text, out   size))
           {
               FontSize = size;
           }
           FontName = comboBox1.Text;
        }
       

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                FontColor = colorDialog1.Color;
                button1.BackColor = FontColor;
            }
        }
    }
}
