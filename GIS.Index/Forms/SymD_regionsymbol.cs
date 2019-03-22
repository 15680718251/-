using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.mm_Conv_Symbol;
using System.Collections;
using System.Drawing.Drawing2D;

namespace GIS.TreeIndex.Forms
{

    public partial class SymbolDesigner : Form
    {
        #region private members
        //
        //第一个点符号
        //
        private int m_curindex_rs0 = -1;
        private string m_curptelename_rs0 = "";
        private Int32 m_curpteleid_rs0 = 666666;
        //
        //第二个点符号
        //
        private int m_curindex_rs1 = -1;
        private string m_curptelename_rs1 = "";
        private Int32 m_curpteleid_rs1 = 666666;

        //
        //填充边线
        //
        private int m_curindex_rsol = -1;
        private string m_curolelename = "";
        private Int32 m_curoleleid = 666666;

        private List<List<float>> m_cache4region;

        #endregion

        private void chosfillptsym1_Click(object sender, EventArgs e)
        {
            CurSymbolForm form = new CurSymbolForm(m_mapui, 0);

            DialogResult result = form.ShowDialog();

            if (result == DialogResult.OK)
            {
                //this.pictureBox_fillpt.Image = form.m_curimage;

                Image img = new Bitmap(this.pictureBox_fillpt1.Width, this.pictureBox_fillpt1.Height);
                Graphics g = Graphics.FromImage(img);
                g.DrawImage(form.m_curimage, new Rectangle(0, 0, this.pictureBox_fillpt1.Width, this.pictureBox_fillpt1.Height),
                    new Rectangle(0, 0, form.m_curimage.Width, form.m_curimage.Height), GraphicsUnit.Pixel);
                this.pictureBox_fillpt1.Image = img;

                m_curindex_rs0 = form.m_curindex;
                m_curptelename_rs0 = m_mapui.m_conv_gtr.ptelename[m_curindex_rs0];
                m_curpteleid_rs0 = m_mapui.m_conv_gtr.ptelefeatid[m_curindex_rs0];
            }
            else if (result == DialogResult.Cancel)
            {
                this.pictureBox_fillpt1.Image = null;
                m_curindex_rs0 = form.m_curindex;
                m_curptelename_rs0 = "";
                m_curpteleid_rs0 = 666666;
            }

        }

        private void chosfillptsym2_Click(object sender, EventArgs e)
        {
            CurSymbolForm form = new CurSymbolForm(m_mapui, 0);

            DialogResult result = form.ShowDialog();

            if (result == DialogResult.OK)
            {
                Image img = new Bitmap(this.pictureBox_fillpt2.Width, this.pictureBox_fillpt2.Height);
                Graphics g = Graphics.FromImage(img);
                g.DrawImage(form.m_curimage, new Rectangle(0, 0, this.pictureBox_fillpt2.Width, this.pictureBox_fillpt2.Height),
                    new Rectangle(0, 0, form.m_curimage.Width, form.m_curimage.Height), GraphicsUnit.Pixel);
                this.pictureBox_fillpt2.Image = img;

                m_curindex_rs1 = form.m_curindex;
                m_curptelename_rs1 = m_mapui.m_conv_gtr.ptelename[m_curindex_rs1];
                m_curpteleid_rs1 = m_mapui.m_conv_gtr.ptelefeatid[m_curindex_rs1];
            }
            else if (result == DialogResult.Cancel)
            {
                this.pictureBox_fillpt2.Image = null;
                m_curindex_rs1 = form.m_curindex;
                m_curptelename_rs1 = "";
                m_curpteleid_rs1 = 666666;
            }

        }


        private void chosoutline_Click(object sender, EventArgs e)
        {
            CurSymbolForm form = new CurSymbolForm(m_mapui, 2);

            DialogResult result = form.ShowDialog();

            if (result == DialogResult.OK)
            {

                Image img = new Bitmap(this.pictureBox_outline.Width, this.pictureBox_outline.Height);
                Graphics g = Graphics.FromImage(img);
                g.DrawImage(form.m_curimage, new Rectangle(0, 0, this.pictureBox_outline.Width, this.pictureBox_outline.Height),
                    new Rectangle(0, 0, form.m_curimage.Width, form.m_curimage.Height), GraphicsUnit.Pixel);
                this.pictureBox_outline.Image = img;

                m_curindex_rsol = form.m_curindex;
                m_curolelename = m_mapui.m_conv_gtr.lineelename[m_curindex_rsol];
                m_curoleleid = m_mapui.m_conv_gtr.lineelefeatid[m_curindex_rsol];
            }
            else if (result == DialogResult.Cancel)
            {
                this.pictureBox_outline.Image = null;
                m_curindex_rsol = form.m_curindex;
                m_curolelename = "";
                m_curoleleid = 666666;
            }

        }

        private void add_region_Click(object sender, EventArgs e)
        {
            float udgap = System.Convert.ToSingle(this.udgap_region.Value);
            float lrgap = System.Convert.ToSingle(this.lrgap_region.Value);

            #region check the consistency
            if (udgap <= 0f)
            {
                MessageBox.Show("上下绘制间隔须大于0!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (lrgap <= 0f)
            {

                MessageBox.Show("左右绘制间隔须大于0!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (m_curindex_rs0 == -1)
            {
                MessageBox.Show("请选择填充点图元1!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //if (m_curindex_rs1 == -1)
            //{
            //    MessageBox.Show("请选择填充点图元2!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            if (m_curindex_rsol == -1)
            {
                MessageBox.Show("请选择填充边线!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            #endregion

            List<float> region_list = new List<float>();

            region_list.Add((float)m_curoleleid);       //outline

            if (m_curindex_rs0 != -1)
                region_list.Add((float)m_curpteleid_rs0);  //add ptelement id0

            if (m_curindex_rs1 != -1)
                region_list.Add((float)m_curpteleid_rs1);   //add ptelement id0

            region_list.Add(System.Convert.ToSingle(this.udgap_region.Value));
            region_list.Add(System.Convert.ToSingle(this.lrgap_region.Value));


            m_cache4region.Add(region_list);


            string fillpt1 = "";
            string fillpt2 = "";
            if (m_curindex_rs0 != -1)
                fillpt1 = m_curpteleid_rs0 + ",";
            if (m_curindex_rs1 != -1)
                fillpt2 = m_curpteleid_rs1 + ",";

            string str2show = m_curoleleid + "," + fillpt1 + fillpt2 + udgap_region.Value + "," + lrgap_region.Value;


            this.listBox_region.Items.Add(str2show);

            listBox_region.SelectedIndex = listBox_region.Items.Count - 1;

        }

        private void delete_region_Click(object sender, EventArgs e)
        {
            int index = this.listBox_region.SelectedIndex;
            if (index == -1)
                return;

            if (listBox_region.Items.Count > 1)
            {
                if (listBox_region.SelectedIndex - 1 >= 0)
                    listBox_region.SelectedIndex -= 1;
                else
                    listBox_region.SelectedIndex += 1;
            }
            else
                listBox_region.SelectedIndex = -1;

            m_cache4region.RemoveAt(index);
            listBox_region.Items.RemoveAt(index);
        }

        private void listBox_region_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_region.SelectedIndex == -1)
                return;

            int index = this.listBox_region.SelectedIndex;

            udgap_region.Value = System.Convert.ToDecimal(m_cache4region[index][m_cache4region[index].Count - 2]);
            lrgap_region.Value = System.Convert.ToDecimal(m_cache4region[index][m_cache4region[index].Count - 1]);
        }


        private void savecurregionsym_Click(object sender, EventArgs e)
        {
            MessageBox.Show("");
        }


    }
}
