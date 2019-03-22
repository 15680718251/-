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
        private List<List<float>> m_cache4p_circle;
        private List<List<float>> m_cache4p_arc;
        private List<List<float>> m_cache4p_line;
        private List<float> m_currentpoints;
        private float m_scale = 1.0f;

        #endregion


        //#region Draw Point Atom

        //private void DrawPtSym2Preview(Graphics g, PointSymbol ptsym,  float scale)
        //{
        //    float cur_scale = (scale == -1.0f ? m_scale : scale);
        //    for (int i = 0; i < ptsym.Atom.Count; i++)
        //    {
        //        if (ptsym.Atom[i] is Atom_Circle)
        //        {
        //            DrawCircle2Preview(g, ptsym.Atom[i] as Atom_Circle, cur_scale);
        //        }
        //        else if (ptsym.Atom[i] is Atom_Arc)
        //        {
        //            DrawArc2Preview(g, ptsym.Atom[i] as Atom_Arc, cur_scale);
        //        }
        //        else
        //        {   // is AtomLine4p
        //            DrawLine4p2Preview(g, ptsym.Atom[i] as Atom_Line4p, cur_scale);
        //        }

        //    }
        //}

        //private void DrawCircle2Preview(Graphics g, Atom_Circle circle, float cur_scale)
        //{
        //    int ncount = circle.Vertices.Count;
        //    PointF[] points = new PointF[ncount];
        //    for (int i = 0; i < ncount; i++)
        //    {
        //        points[i] = new PointF((float)this.previewbox.Width / 2 + circle.Vertices[i].X * cur_scale, (float)this.previewbox.Height / 2 - circle.Vertices[i].Y * cur_scale);

        //    }

        //    Color clr = Color.FromArgb(255, 0, 255, 245);
        //    Pen linePen = new Pen(clr, circle.line_width == 0.15f ? circle.line_width : circle.line_width * cur_scale);

        //    if (circle.bpoint && ncount == 1)
        //    {
        //        g.DrawEllipse(linePen, new RectangleF(points[0].X - 0.15f, points[0].Y - 0.15f, 0.3f, 0.3f));
        //        return;
        //    }
            

        //    if (circle.bFill)
        //    {
        //        GraphicsPath path_Circle = new GraphicsPath();
        //        path_Circle.AddLines(points);
        //        SolidBrush brushFill = new SolidBrush(clr);
        //        g.FillPath(brushFill, path_Circle);
        //        brushFill.Dispose();
        //        path_Circle.Dispose();
        //    }
        //    g.DrawLines(linePen, points);


        //    linePen.Dispose();

        //}

        //private void DrawArc2Preview(Graphics g, Atom_Arc arc, float cur_scale)
        //{

        //    int ncount = arc.Vertices.Count;
        //    PointF[] points = new PointF[ncount];
        //    for (int i = 0; i < ncount; i++)
        //    {
        //        points[i] = new PointF((float)this.previewbox.Width / 2 + arc.Vertices[i].X * cur_scale, (float)this.previewbox.Height / 2 - arc.Vertices[i].Y * cur_scale);

        //    }
        //    Color clr = Color.FromArgb(255, 0, 255, 245);
        //    Pen linePen = new Pen(arc.clr, arc.line_width == 0.15f ? arc.line_width : arc.line_width * cur_scale);

        //    if (arc.bFill)
        //    {
        //        GraphicsPath path_arc = new GraphicsPath();
        //        path_arc.AddLines(points);
        //        SolidBrush brushFill = new SolidBrush(clr);
        //        g.FillPath(brushFill, path_arc);
        //        brushFill.Dispose();
        //        path_arc.Dispose();
        //    }

        //    g.DrawLines(linePen, points);

        //    linePen.Dispose();

        //}

        //private void DrawLine4p2Preview(Graphics g, Atom_Line4p line, float cur_scale)
        //{

        //    int ncount = line.Vertices.Count;
        //    PointF[] points = new PointF[ncount];
        //    for (int i = 0; i < ncount; i++)
        //    {
        //        points[i] = new PointF((float)this.previewbox.Width / 2 + line.Vertices[i].X * cur_scale, (float)this.previewbox.Height / 2 - line.Vertices[i].Y * cur_scale);

        //    }
        //    Color clr = Color.FromArgb(255, 0, 255, 245);
        //    Pen linePen = new Pen(clr, line.line_width == 0.15f ? line.line_width : line.line_width * cur_scale);

        //    if (line.bfill)
        //    {
        //        GraphicsPath path_line = new GraphicsPath();
        //        path_line.AddLines(points);
        //        SolidBrush brushFill = new SolidBrush(clr);
        //        g.FillPath(brushFill, path_line);
        //        brushFill.Dispose();
        //        path_line.Dispose();
        //    }

        //    g.DrawLines(linePen, points);

        //    linePen.Dispose();
        //}

        //#endregion

        #region click event
        private void arc_add_Click(object sender, EventArgs e)
        {

            float r = System.Convert.ToSingle(r_arc.Value);
            if (r <= 0)
            {
                MessageBox.Show("圆弧半径必须大于0!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // float x, float y, float dx0, float dy0,  float dx1, float dy1, float ang, bool bfill, float penwidth,  float r, 
            // float startangle, float endangle, bool bclosed)
            List<float> arc_list = new List<float>();

            arc_list.Add(System.Convert.ToSingle(dx0_arc.Value));
            arc_list.Add(System.Convert.ToSingle(dy0_arc.Value));

            arc_list.Add(System.Convert.ToSingle(dx1_arc.Value));
            arc_list.Add(System.Convert.ToSingle(dy1_arc.Value));

            arc_list.Add(System.Convert.ToSingle(angle_arc.Value));
            arc_list.Add(System.Convert.ToSingle(bfill_arc.Value));
            arc_list.Add(System.Convert.ToSingle(penwidth_arc.Value));
            arc_list.Add(r);

            arc_list.Add(System.Convert.ToSingle(startangle_arc.Value));
            arc_list.Add(System.Convert.ToSingle(endangle_arc.Value));
            arc_list.Add(System.Convert.ToSingle(bclosed_arc.Value));

            m_cache4p_arc.Add(arc_list);

            string str2show = dx0_arc.Value + "," + dy0_arc.Value + "," + dx1_arc.Value + "," + dy1_arc.Value + "," +
                angle_arc.Value + "," + bfill_arc.Value + "," + penwidth_arc.Value + "," + r_arc.Value + "," + startangle_arc.Value + "," +
                endangle_arc.Value + "," + bclosed_arc.Value;

            this.listBox_arc.Items.Add(str2show);

            listBox_arc.SelectedIndex = listBox_arc.Items.Count - 1;

        }


        private void add_circle_Click(object sender, EventArgs e)
        {
            //float x, float y, float dx0, float dy0, float dx1, float dy1,  
            //float ang,  bool bfill, float penwidth, float r )
            float r = System.Convert.ToSingle(r_circle.Value);
            if (r <= 0)
            {
                MessageBox.Show("圆半径必须大于0!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }


            List<float> circle_list = new List<float>();

            circle_list.Add(System.Convert.ToSingle(dx0_circle.Value));
            circle_list.Add(System.Convert.ToSingle(dy0_circle.Value));


            circle_list.Add(System.Convert.ToSingle(dx1_circle.Value));
            circle_list.Add(System.Convert.ToSingle(dy1_circle.Value));


            circle_list.Add(System.Convert.ToSingle(angle_circle.Value));
            circle_list.Add(System.Convert.ToSingle(bfill_circle.Value));
            circle_list.Add(System.Convert.ToSingle(penwidth_circle.Value));
            circle_list.Add(r);


            m_cache4p_circle.Add(circle_list);


            string str2show = dx0_circle.Value + "," + dy0_circle.Value + "," + dx1_circle.Value + "," + dy1_circle.Value + "," +
                angle_circle.Value + "," + bfill_circle.Value + "," + penwidth_circle.Value + "," + r_circle.Value;
            this.listBox_circle.Items.Add(str2show);

            listBox_circle.SelectedIndex = listBox_circle.Items.Count - 1;

        }

        private void arc_delete_Click(object sender, EventArgs e)
        {
            int index = this.listBox_arc.SelectedIndex;
            if (index == -1)
                return;

            if (listBox_arc.Items.Count > 1)
            {
                if (listBox_arc.SelectedIndex - 1 >= 0)
                    listBox_arc.SelectedIndex -= 1;
                else
                    listBox_arc.SelectedIndex += 1;
            }
            else
                listBox_arc.SelectedIndex = -1;

            m_cache4p_arc.RemoveAt(index);
            listBox_arc.Items.RemoveAt(index);
        }


        private void delete_circle_Click(object sender, EventArgs e)
        {
            int index = this.listBox_circle.SelectedIndex;
            if (index == -1)
                return;

            if (listBox_circle.Items.Count > 1)
            {
                if (listBox_circle.SelectedIndex - 1 >= 0)
                    listBox_circle.SelectedIndex -= 1;
                else
                    listBox_circle.SelectedIndex += 1;
            }
            else
                listBox_circle.SelectedIndex = -1;

            m_cache4p_circle.RemoveAt(index);
            listBox_circle.Items.RemoveAt(index);

        }

        private void listBox_circle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_circle.SelectedIndex == -1)
                return;

            int index = this.listBox_circle.SelectedIndex;

            dx0_circle.Value = System.Convert.ToDecimal(m_cache4p_circle[index][0]);
            dy0_circle.Value = System.Convert.ToDecimal(m_cache4p_circle[index][1]);

            dx1_circle.Value = System.Convert.ToDecimal(m_cache4p_circle[index][2]);
            dy1_circle.Value = System.Convert.ToDecimal(m_cache4p_circle[index][3]);

            angle_circle.Value = System.Convert.ToDecimal(m_cache4p_circle[index][4]);
            bfill_circle.Value = System.Convert.ToDecimal(m_cache4p_circle[index][5]);

            penwidth_circle.Value = System.Convert.ToDecimal(m_cache4p_circle[index][6]);

            r_circle.Value = System.Convert.ToDecimal(m_cache4p_circle[index][7]);


        }


        private void listBox_arc_SelectedIndexChanged(object sender, EventArgs e)
        {

            // float x, float y, float dx0, float dy0,  float dx1, float dy1, float ang, bool bfill, float penwidth,  float r, 
            // float startangle, float endangle, bool bclosed)

            int k = listBox_arc.SelectedIndex;
            if (listBox_arc.SelectedIndex == -1)
                return;

            int index = this.listBox_arc.SelectedIndex;

            dx0_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][0]);
            dy0_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][1]);

            dx1_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][2]);
            dy1_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][3]);

            angle_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][4]);
            bfill_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][5]);

            penwidth_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][6]);

            r_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][7]);

            startangle_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][8]);
            endangle_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][9]);

            bclosed_arc.Value = System.Convert.ToDecimal(m_cache4p_arc[index][10]);

        }

        private void add2currentpt_Click(object sender, EventArgs e)
        {
            string currentpt2show = currentpoint4p.Text;

            m_currentpoints.Add(System.Convert.ToSingle(cur_x.Value));
            m_currentpoints.Add(System.Convert.ToSingle(cur_y.Value));


            currentpt2show += "(" + System.Convert.ToString(cur_x.Value) + "," + System.Convert.ToString(cur_y.Value) + ")";

            currentpoint4p.Text = currentpt2show;


        }

        private void clear4p_Click(object sender, EventArgs e)
        {

            currentpoint4p.Clear();
            m_currentpoints.Clear();
        }

        private void add_line4p_Click(object sender, EventArgs e)
        {
            //float x, float y, float dx0, float dy0, float dx1, float dy1, 
            //float angle,  bool bfill, float penwidth, List<float> list)

            if (m_currentpoints.Count < 2)
            {
                MessageBox.Show("点个数至少为2!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;

            }

            List<float> list = new List<float>();

            list.Add(System.Convert.ToSingle(dx0_line4p.Value));
            list.Add(System.Convert.ToSingle(dy0_line4p.Value));
            list.Add(System.Convert.ToSingle(dx1_line4p.Value));
            list.Add(System.Convert.ToSingle(dy1_line4p.Value));

            list.Add(System.Convert.ToSingle(angle_line4p.Value));

            list.Add(System.Convert.ToSingle(bfill_line4p.Value));
            list.Add(System.Convert.ToSingle(penwidth_line4p.Value));


            string str2show;
            str2show = dx0_line4p.Value + "," + dy0_line4p.Value + "," + dx1_line4p.Value + "," + dy1_line4p.Value + "," + angle_line4p.Value + "," +
                bfill_line4p.Value + "," + penwidth_line4p.Value;


            for (int i = 0; i < m_currentpoints.Count; i++)
            {
                list.Add(m_currentpoints[i]);
                str2show += "," + m_currentpoints[i];
            }

            m_cache4p_line.Add(list);
            listBox_line4p.Items.Add(str2show);

            listBox_line4p.SelectedIndex = listBox_line4p.Items.Count - 1;

        }

        private void delete_line4p_Click(object sender, EventArgs e)
        {
            int index = this.listBox_line4p.SelectedIndex;
            if (index == -1)
                return;

            if (listBox_line4p.Items.Count > 1)
            {
                if (listBox_line4p.SelectedIndex - 1 >= 0)
                    listBox_line4p.SelectedIndex -= 1;
                else
                    listBox_line4p.SelectedIndex += 1;
            }
            else
                listBox_line4p.SelectedIndex = -1;

            m_cache4p_line.RemoveAt(index);
            listBox_line4p.Items.RemoveAt(index);

        }

        private void listBox_line4p_SelectedIndexChanged(object sender, EventArgs e)
        {

            //float dx0, float dy0, float dx1, float dy1, 
            //float angle,  bool bfill, float penwidth, List<float> list)

            int k = listBox_line4p.SelectedIndex;
            if (listBox_line4p.SelectedIndex == -1)
                return;

            int index = this.listBox_line4p.SelectedIndex;

            dx0_line4p.Value = System.Convert.ToDecimal(m_cache4p_line[index][0]);
            dy0_line4p.Value = System.Convert.ToDecimal(m_cache4p_line[index][1]);
            dx1_line4p.Value = System.Convert.ToDecimal(m_cache4p_line[index][2]);
            dy1_line4p.Value = System.Convert.ToDecimal(m_cache4p_line[index][3]);
            angle_line4p.Value = System.Convert.ToDecimal(m_cache4p_line[index][4]);
            bfill_line4p.Value = System.Convert.ToDecimal(m_cache4p_line[index][5]);
            penwidth_line4p.Value = System.Convert.ToDecimal(m_cache4p_line[index][6]);


            m_currentpoints.Clear();
            currentpoint4p.Clear();
            string str2show = currentpoint4p.Text;
            for (int i = 7; i < m_cache4p_line[index].Count - 1; i += 2)
            {
                str2show = str2show + "(" + m_cache4p_line[index][i] + "," + m_cache4p_line[index][i + 1] + ")";
                m_currentpoints.Add(m_cache4p_line[index][i]);
                m_currentpoints.Add(m_cache4p_line[index][i + 1]);
            }
            currentpoint4p.Text = str2show;




        }

        private void replace_line4p_Click(object sender, EventArgs e)
        {
            int index = listBox_line4p.SelectedIndex;
            if (listBox_line4p.SelectedIndex == -1)
                return;
            m_cache4p_line[index].Clear();


            m_cache4p_line[index].Add(System.Convert.ToSingle(dx0_line4p.Value));
            m_cache4p_line[index].Add(System.Convert.ToSingle(dy0_line4p.Value));
            m_cache4p_line[index].Add(System.Convert.ToSingle(dx1_line4p.Value));
            m_cache4p_line[index].Add(System.Convert.ToSingle(dy1_line4p.Value));

            m_cache4p_line[index].Add(System.Convert.ToSingle(angle_line4p.Value));

            m_cache4p_line[index].Add(System.Convert.ToSingle(bfill_line4p.Value));
            m_cache4p_line[index].Add(System.Convert.ToSingle(penwidth_line4p.Value));


            string str2show;
            str2show = dx0_line4p.Value + "," + dy0_line4p.Value + "," + dx1_line4p.Value + "," + dy1_line4p.Value + "," + angle_line4p.Value + "," +
                bfill_line4p.Value + "," + penwidth_line4p.Value;


            for (int i = 0; i < m_currentpoints.Count; i++)
            {
                m_cache4p_line[index].Add(m_currentpoints[i]);
                str2show += "," + m_currentpoints[i];
            }

            listBox_line4p.Items[index] = str2show;

        }

        #endregion
        private void savecurptsym_Click(object sender, EventArgs e)
        {

            SaveCurPtSymForm form = new SaveCurPtSymForm(m_mapui);
            if (form.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            //string savetype = form.m_savetype;

            string packstr = "";
            bool bexist = false;

            for (int i = 0; i < m_cache4p_arc.Count; i++)
            {
                packstr += "A,";
                int j = 0;
                for (; j < m_cache4p_arc[i].Count - 1; j++)
                {
                    packstr += m_cache4p_arc[i][j] + ",";
                }
                packstr += m_cache4p_arc[i][j] + ";";
                bexist = true;
            }

            for (int i = 0; i < m_cache4p_circle.Count; i++)
            {
                packstr += "C,";
                int j = 0;
                for (; j < m_cache4p_circle[i].Count - 1; j++)
                {
                    packstr += m_cache4p_circle[i][j] + ",";
                }
                packstr += m_cache4p_circle[i][j] + ";";
                bexist = true;
            }

            for (int i = 0; i < m_cache4p_line.Count; i++)
            {
                packstr += "L,";
                int j = 0;
                for (; j < m_cache4p_line[i].Count - 1; j++)
                {
                    packstr += m_cache4p_line[i][j] + ",";
                }
                packstr += m_cache4p_line[i][j] + ";";
                bexist = true;
            }

            if (bexist)
            {
                if (form.m_bptsymchecked)
                {
                    m_mapui.m_conv_gtr.pointfeatid.Add(form.m_id);
                    m_mapui.m_conv_gtr.pointname.Add(form.m_name);

                    m_mapui.m_conv_gtr.pointpara.Add(packstr);
                }
                if (form.m_bptelechecked)
                {
                    m_mapui.m_conv_gtr.ptelefeatid.Add(form.m_id);
                    m_mapui.m_conv_gtr.ptelename.Add(form.m_name);
                    m_mapui.m_conv_gtr.ptelepara.Add(packstr);
                }

            }


        }

       

       

    }
}
