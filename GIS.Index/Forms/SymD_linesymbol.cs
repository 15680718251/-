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
        private List<List<float>> m_cache4l_line;
        private List<List<float>> m_cache4l_dashline;
        private List<List<float>> m_cache4l_pointline;
        private int m_curindex_ls = -1;
        private string m_curptelename_ls = "";
        private Int32 m_curpteleid_ls = 666666;
        #endregion

        private void add_ptline_Click(object sender, EventArgs e)
        {
            // ptelementid,udoffset,lroffset,gaglen;
            float gaplen = System.Convert.ToSingle(gaplen_ptline.Value);
            if (m_curindex_ls == -1)
            {
                MessageBox.Show("请选择点图元!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (gaplen <= 0f)
            {
                MessageBox.Show("绘制间隔须大于0！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<float> ptline_list = new List<float>();

            ptline_list.Add((float)m_curpteleid_ls);   //add ptelement id
            ptline_list.Add(System.Convert.ToSingle(udoffset_ptline.Value));
            ptline_list.Add(System.Convert.ToSingle(lroffset_ptline.Value));
            ptline_list.Add(gaplen);

            m_cache4l_pointline.Add(ptline_list);

            string str2show = m_curpteleid_ls + "," + udoffset_ptline.Value + "," +
                lroffset_ptline.Value + "," + gaplen_ptline.Value;


            this.listBox_ptline.Items.Add(str2show);

            listBox_ptline.SelectedIndex = listBox_ptline.Items.Count - 1;
        }




        private void add_dashline_Click(object sender, EventArgs e)
        {

            float d0 = System.Convert.ToSingle(dash0_dashline.Value);
            float d1 = System.Convert.ToSingle(dash1_dashline.Value);

            if (d0 <= 0.01 || d1 <= 0.01)
            {
                MessageBox.Show("Dash模板参数需大于0！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<float> dashline_list = new List<float>();

            dashline_list.Add(System.Convert.ToSingle(penwidth_dashline.Value));
            dashline_list.Add(System.Convert.ToSingle(udoffset_dashline.Value));
            dashline_list.Add(System.Convert.ToSingle(lroffset_dashline.Value));
            dashline_list.Add(System.Convert.ToSingle(color_dashline.Value));

            dashline_list.Add(System.Convert.ToSingle(dash0_dashline.Value));
            dashline_list.Add(System.Convert.ToSingle(dash1_dashline.Value));


            m_cache4l_dashline.Add(dashline_list);

            string str2show = penwidth_dashline.Value + "," + udoffset_dashline.Value
                + "," + lroffset_dashline.Value + "," + color_dashline.Value
                + "," + dash0_dashline.Value + "," + dash1_dashline.Value;

            this.listBox_dashline.Items.Add(str2show);

            listBox_dashline.SelectedIndex = listBox_dashline.Items.Count - 1;
        }



        private void add_line_Click(object sender, EventArgs e)
        {
            List<float> line_list = new List<float>();

            line_list.Add(System.Convert.ToSingle(penwidth_line4l.Value));
            line_list.Add(System.Convert.ToSingle(udoffset_line4l.Value));
            //line_list.Add(System.Convert.ToSingle(color_line4l.Value));

            m_cache4l_line.Add(line_list);

            string str2show = penwidth_line4l.Value + "," + udoffset_line4l.Value;

            this.listBox_line.Items.Add(str2show);

            listBox_line.SelectedIndex = listBox_line.Items.Count - 1;

        }

        private void delete_dashline_Click(object sender, EventArgs e)
        {
            int index = this.listBox_dashline.SelectedIndex;
            if (index == -1)
                return;

            if (listBox_dashline.Items.Count > 1)
            {
                if (listBox_dashline.SelectedIndex - 1 >= 0)
                    listBox_dashline.SelectedIndex -= 1;
                else
                    listBox_dashline.SelectedIndex += 1;
            }
            else
                listBox_dashline.SelectedIndex = -1;

            m_cache4l_dashline.RemoveAt(index);
            listBox_dashline.Items.RemoveAt(index);
        }

        private void delete_ptline_Click(object sender, EventArgs e)
        {
            int index = this.listBox_ptline.SelectedIndex;
            if (index == -1)
                return;

            if (listBox_ptline.Items.Count > 1)
            {
                if (listBox_ptline.SelectedIndex - 1 >= 0)
                    listBox_ptline.SelectedIndex -= 1;
                else
                    listBox_ptline.SelectedIndex += 1;
            }
            else
                listBox_ptline.SelectedIndex = -1;

            m_cache4l_pointline.RemoveAt(index);
            listBox_ptline.Items.RemoveAt(index);

        }

        private void delete_line_Click(object sender, EventArgs e)
        {
            int index = this.listBox_line.SelectedIndex;
            if (index == -1)
                return;

            if (listBox_line.Items.Count > 1)
            {
                if (listBox_line.SelectedIndex - 1 >= 0)
                    listBox_line.SelectedIndex -= 1;
                else
                    listBox_line.SelectedIndex += 1;
            }
            else
                listBox_line.SelectedIndex = -1;

            m_cache4l_line.RemoveAt(index);
            listBox_line.Items.RemoveAt(index);
        }

        private void listBox_ptline_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_ptline.SelectedIndex == -1)
                return;

            int index = this.listBox_ptline.SelectedIndex;


            udoffset_ptline.Value = System.Convert.ToDecimal(m_cache4l_pointline[index][1]);
            lroffset_ptline.Value = System.Convert.ToDecimal(m_cache4l_pointline[index][2]);
            gaplen_ptline.Value = System.Convert.ToDecimal(m_cache4l_pointline[index][3]);


        }

        private void listBox_line_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_line.SelectedIndex == -1)
                return;

            int index = this.listBox_line.SelectedIndex;

            penwidth_line4l.Value = System.Convert.ToDecimal(m_cache4l_line[index][0]);
            udoffset_line4l.Value = System.Convert.ToDecimal(m_cache4l_line[index][1]);


        }

        private void listBox_dashline_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_dashline.SelectedIndex == -1)
                return;

            int index = this.listBox_dashline.SelectedIndex;

            penwidth_dashline.Value = System.Convert.ToDecimal(m_cache4l_dashline[index][0]);
            udoffset_dashline.Value = System.Convert.ToDecimal(m_cache4l_dashline[index][1]);
            lroffset_dashline.Value = System.Convert.ToDecimal(m_cache4l_dashline[index][2]);
            color_dashline.Value = System.Convert.ToDecimal(m_cache4l_dashline[index][3]);

            dash0_dashline.Value = System.Convert.ToDecimal(m_cache4l_dashline[index][4]);
            dash1_dashline.Value = System.Convert.ToDecimal(m_cache4l_dashline[index][5]);


        }



        //#region Draw Atom_line
        //private void DrawLineSym2Preview(Graphics g, LineSymbol linesym, float scale)
        //{
        //    float cur_scale = (scale == -1.0f ? m_scale : scale);
        //    for (int i = 0; i < linesym.Atom.Count; i++)
        //    {
        //        if (linesym.Atom[i] is Atom_DashLine)
        //        {
        //            DrawDashline2Preview(g, linesym.Atom[i] as Atom_DashLine, cur_scale);
        //        }
        //        else if (linesym.Atom[i] is Atom_SolidLine)
        //        {
        //            Drawline4l2Preview(g, linesym.Atom[i] as Atom_SolidLine, cur_scale);
        //        }

        //    }

        //    for (int i = 0; i < linesym.point_symbol.Count; i++)
        //    {
        //        DrawPtSym2Preview(g, linesym.point_symbol[i], cur_scale);
        //    }


        //}


        //private void Drawline4l2Preview(Graphics g, Atom_SolidLine line, float scale)
        //{
        //    int ncount = line.Vertices.Count;
        //    PointF[] points = new PointF[ncount];
        //    for (int i = 0; i < ncount; i++)
        //    {
        //        points[i] = new PointF((float)this.previewbox.Width / 2 + line.Vertices[i].X * scale, (float)this.previewbox.Height / 2 - line.Vertices[i].Y * scale);

        //    }

        //    Color clr = Color.FromArgb(255, 0, 255, 245);

        //    Pen linePen = new Pen(clr, line.line_width == 0.15f ? line.line_width : line.line_width * scale);
        //    g.DrawLines(linePen, points);

        //    linePen.Dispose();

        //}

        //private void DrawDashline2Preview(Graphics g, Atom_DashLine dashline, float scale)
        //{
        //    int ncount = dashline.Vertices.Count;
        //    PointF[] points = new PointF[ncount];
        //    for (int i = 0; i < ncount; i++)
        //    {
        //        points[i] = new PointF(((float)this.previewbox.Width / 2 + dashline.Vertices[i].X * scale), ((float)this.previewbox.Height / 2 - dashline.Vertices[i].Y*scale));

        //    }

        //    Color clr = Color.FromArgb(255, 0, 255, 245);
        //    Pen linePen = new Pen(clr, dashline.line_width == 0.15f ? dashline.line_width : dashline.line_width * scale);

        //    for (int i = 0; i < ncount - 1; i += 2)
        //    {
        //        g.DrawLine(linePen, points[i], points[i + 1]);
        //    }

        //    linePen.Dispose();
        //}



        //#endregion



        private void savecurlinesym_Click(object sender, EventArgs e)
        {
            SaveCurLineSymForm form = new SaveCurLineSymForm(m_mapui);
            if (form.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string packstr = "";
            bool bexist = false;

            for (int i = 0; i < m_cache4l_line.Count; i++)
            {
                packstr += "L,";
                int j = 0;
                for (; j < m_cache4l_line[i].Count - 1; j++)
                {
                    packstr += m_cache4l_line[i][j] + ",";
                }
                packstr += m_cache4l_line[i][j] + ";";
                bexist = true;
            }

            for (int i = 0; i < m_cache4l_dashline.Count; i++)
            {
                packstr += "D,";
                int j = 0;
                for (; j < m_cache4l_dashline[i].Count - 1; j++)
                {
                    packstr += m_cache4l_dashline[i][j] + ",";
                }
                packstr += m_cache4l_dashline[i][j] + ";";
                bexist = true;
            }

            for (int i = 0; i < m_cache4l_pointline.Count; i++)
            {
                packstr += "P,";
                int j = 0;
                for (; j < m_cache4l_pointline[i].Count - 1; j++)
                {
                    packstr += m_cache4l_pointline[i][j] + ",";
                }
                packstr += m_cache4l_pointline[i][j] + ";";
                bexist = true;
            }

            if (bexist)
            {
                if (form.m_blinesymchecked)
                {
                    m_mapui.m_conv_gtr.linefeatid.Add(form.m_id);
                    m_mapui.m_conv_gtr.linename.Add(form.m_name);
                    m_mapui.m_conv_gtr.linepara.Add(packstr);
                }
                if (form.m_blineelechecked)
                {
                    m_mapui.m_conv_gtr.lineelefeatid.Add(form.m_id);
                    m_mapui.m_conv_gtr.lineelename.Add(form.m_name);
                    m_mapui.m_conv_gtr.lineelepara.Add(packstr);
                }

            }

        }


        private void choosecurptsym_Click(object sender, EventArgs e)
        {
            //CurPtElementForm form = new CurPtElementForm(m_mapui);
            CurSymbolForm form = new CurSymbolForm(m_mapui, 0);

            DialogResult result = form.ShowDialog();

            if (result == DialogResult.OK)
            {
                //this.selectedptele.Image = form.m_curimage;

                Image img = new Bitmap(this.selectedptele.Width, this.selectedptele.Height);
                Graphics g = Graphics.FromImage(img);
                g.DrawImage(form.m_curimage, new Rectangle(0, 0, this.selectedptele.Width, this.selectedptele.Height),
                    new Rectangle(0, 0, form.m_curimage.Width, form.m_curimage.Height), GraphicsUnit.Pixel);
                this.selectedptele.Image = img;

                m_curindex_ls = form.m_curindex;
                m_curptelename_ls = m_mapui.m_conv_gtr.ptelename[m_curindex_ls];
                m_curpteleid_ls = m_mapui.m_conv_gtr.ptelefeatid[m_curindex_ls];
            }
            else if (result == DialogResult.Cancel)
            {
                this.selectedptele.Image = null;
                m_curindex_ls = form.m_curindex;
                m_curptelename_ls = "";
                m_curpteleid_ls = 666666;
            }

        }

      



    }
}
