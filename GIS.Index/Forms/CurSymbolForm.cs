using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.mm_Conv_Symbol;
using GIS.Geometries;
using System.Drawing.Drawing2D;

namespace GIS.TreeIndex.Forms
{
    public partial class CurSymbolForm : Form
    {
        public CurSymbolForm(MapUI ui, int selectedtype)
        {
            InitializeComponent();

            m_mapui = ui;
            Initial();

            switch (selectedtype)
            {
                case -1:   //all
                    this.comboBox_type.SelectedIndex = 0;
                    this.ok.Enabled = false;
                    //this.ok.Hide();
                    break;
                case 0:    //点图元
                    this.comboBox_type.SelectedIndex = 0;
                    this.comboBox_type.Enabled = false;
                    this.delete.Enabled = false;
                    this.Text = "点图元";
                    break;
                case 1:    //点符号
                    this.comboBox_type.SelectedIndex = 1;
                    this.comboBox_type.Enabled = false;
                    this.delete.Enabled = false;
                    this.Text = "点符号";
                    break;
                case 2:    //线图元
                    this.comboBox_type.SelectedIndex = 2;
                    this.comboBox_type.Enabled = false;
                    this.delete.Enabled = false;
                    this.Text = "线图元";
                    break;
                case 3:    //线符号
                    this.comboBox_type.SelectedIndex = 3;
                    this.comboBox_type.Enabled = false;
                    this.delete.Enabled = false;
                    this.Text = "线符号";
                    break;
                case 4:    //面符号
                    this.comboBox_type.SelectedIndex = 4;
                    this.comboBox_type.Enabled = false;
                    this.delete.Enabled = false;
                    this.Text = "面符号";
                    break;
                default:
                    break;
            }
            
        }

        private MapUI m_mapui;
        private int iconwidth = 74;
        private float m_scale = 4.0f;
        public int m_curindex = -1;
        public Image m_curimage = null;
        private ImageList m_ImageList;
        private ListViewItem[] m_items;


        private void Initial()
        {
            this.comboBox_type.Items.Add("点图元");    //0
            this.comboBox_type.Items.Add("点符号");    //1
            this.comboBox_type.Items.Add("线图元");    //2
            this.comboBox_type.Items.Add("线符号");    //3
            this.comboBox_type.Items.Add("面符号");    //4

            listView_cursymbol.OwnerDraw = false;
            listView_cursymbol.View = View.LargeIcon;

        }



        private void RenderViewList()
        {
            switch (this.comboBox_type.SelectedIndex)
            {
                case 0:
                    //
                    //点图元
                    //
                    {
                        m_ImageList = new ImageList();
                        m_ImageList.ImageSize = new Size(iconwidth, iconwidth);
                        int count = m_mapui.m_conv_gtr.ptelename.Count;
                        m_items = new ListViewItem[count];
                        for (int i = 0; i < count; i++)
                        {
                            string str = m_mapui.m_conv_gtr.ptelename[i] + "\r\n" + m_mapui.m_conv_gtr.ptelefeatid[i];
                            m_items[i] = new ListViewItem(str, i);
                            Bitmap bmp = new Bitmap(iconwidth, iconwidth);
                            Graphics g = Graphics.FromImage(bmp);
                            //g.SmoothingMode = SmoothingMode.AntiAlias;
                          
                            DrawPtEleIcon(g, i);
                            m_ImageList.Images.Add(bmp);
                            g.Dispose();
                        }
                        listView_cursymbol.Items.Clear();
                        listView_cursymbol.Items.AddRange(m_items);
                        listView_cursymbol.LargeImageList = m_ImageList;
                        this.count.Text = System.Convert.ToString(count);
                        break;
                    }
                case 1:
                    //
                    //点符号
                    //
                    {
                        m_ImageList = new ImageList();
                        m_ImageList.ImageSize = new Size(iconwidth, iconwidth);
                        int count = m_mapui.m_conv_gtr.pointname.Count;
                        m_items = new ListViewItem[count];
                        for (int i = 0; i < count; i++)
                        {
                            string str = m_mapui.m_conv_gtr.pointname[i] + "\r\n" + m_mapui.m_conv_gtr.pointfeatid[i];
                            m_items[i] = new ListViewItem(str, i);

                            Bitmap bmp = new Bitmap(iconwidth, iconwidth);
                            Graphics g = Graphics.FromImage(bmp);
                            //g.SmoothingMode = SmoothingMode.AntiAlias;

                            DrawPointIcon(g, i);
                            m_ImageList.Images.Add(bmp);
                            g.Dispose();
                        }

                        listView_cursymbol.Items.Clear();
                        listView_cursymbol.Items.AddRange(m_items);
                        listView_cursymbol.LargeImageList = m_ImageList;
                        this.count.Text = System.Convert.ToString(count);
                        break;
                    }
                case 2:
                    //
                    //线图元
                    //
                    {
                        m_ImageList = new ImageList();
                        m_ImageList.ImageSize = new Size(iconwidth, iconwidth);
                        int count = m_mapui.m_conv_gtr.lineelename.Count;
                        m_items = new ListViewItem[count];
                        for (int i = 0; i < count; i++)
                        {
                            string str = m_mapui.m_conv_gtr.lineelename[i] + "\r\n" + m_mapui.m_conv_gtr.lineelefeatid[i];
                            m_items[i] = new ListViewItem(str, i);
                            Bitmap bmp = new Bitmap(iconwidth, iconwidth);
                            Graphics g = Graphics.FromImage(bmp);
                            //g.SmoothingMode = SmoothingMode.AntiAlias;

                            DrawLineEleIcon(g, i);
                            m_ImageList.Images.Add(bmp);
                            g.Dispose();
                        }
                        listView_cursymbol.Items.Clear();
                        listView_cursymbol.Items.AddRange(m_items);
                        listView_cursymbol.LargeImageList = m_ImageList;
                        this.count.Text = System.Convert.ToString(count);
                        break;
                    }
                case 3:
                    //
                    //线符号
                    //
                    {
                        m_ImageList = new ImageList();
                        m_ImageList.ImageSize = new Size(iconwidth, iconwidth);
                        int count = m_mapui.m_conv_gtr.linename.Count;
                        m_items = new ListViewItem[count];
                        for (int i = 0; i < count; i++)
                        {
                            string str = m_mapui.m_conv_gtr.linename[i] + "\r\n" + m_mapui.m_conv_gtr.linefeatid[i];
                            m_items[i] = new ListViewItem(str, i);
                            Bitmap bmp = new Bitmap(iconwidth, iconwidth);
                            Graphics g = Graphics.FromImage(bmp);
                            //g.SmoothingMode = SmoothingMode.AntiAlias;

                            DrawLineIcon(g, i);
                            m_ImageList.Images.Add(bmp);
                            g.Dispose();
                        }
                        listView_cursymbol.Items.Clear();
                        listView_cursymbol.Items.AddRange(m_items);
                        listView_cursymbol.LargeImageList = m_ImageList;
                        this.count.Text = System.Convert.ToString(count);
                        break;
                    }
                case 4:
                    //
                    //面符号
                    //
                    {
                        break;
                    }
                default:
                    break;
            }

        }

        private void DrawLineIcon(Graphics g, int index)
        {
            if (!m_mapui.m_conv_gtr.Extract_LineSymbol(index))
                return;
            DrawLineIcon(g);
        }

        private void DrawLineIcon(Graphics g)
        {
            PointF[] pts = { new PointF(-iconwidth / 2 + 2, 0), new PointF(iconwidth / 2 - 2, 0) };
            LineSymbol lineele = m_mapui.m_conv_gtr.generate_lineelement(pts);

            SolidBrush brush = new SolidBrush(Color.FromArgb(250, 0, 0, 0));
            Pen outlinepen = new Pen(Color.FromArgb(80, 0, 0, 0));
            Pen pen = new Pen(Color.FromArgb(255, 200, 0, 0));
            pen.DashPattern = new float[] { 4f, 2.5f };

            g.FillRectangle(brush, 1, 1, iconwidth - 2, iconwidth - 2);
            g.DrawRectangle(outlinepen, 0, 0, iconwidth - 1, iconwidth - 1);

            //DrawLine(g, lineele);
            GeoBound bound = new GeoBound(0, 0, iconwidth, iconwidth);
            GIS.Render.RenderAPI.DrawLineSymbol(g, lineele, bound, 1, true, new GIS.Style.VectorStyle());

            outlinepen.Dispose();
            brush.Dispose();
        }

        private void DrawLineEleIcon(Graphics g, int index)
        {
            if (!m_mapui.m_conv_gtr.Extract_LineEleSymbol(index))
                return;

            DrawLineIcon(g);
            #region old 
            //PointF[] pts = {new PointF(- iconwidth /2 + 2, 0 ), new PointF(iconwidth /2 - 2, 0)};
            //LineSymbol lineele = m_mapui.m_conv_gtr.generate_lineelement(pts);

            //SolidBrush brush = new SolidBrush(Color.FromArgb(250, 0, 0, 0));
            //Pen outlinepen = new Pen(Color.FromArgb(100, 0, 255, 255));
            //Pen pen = new Pen(Color.FromArgb(255, 200, 0, 0));
            //pen.DashPattern = new float[] { 4f, 2.5f };

            //g.FillRectangle(brush, 1, 1, iconwidth - 1, iconwidth - 1);
            //g.DrawRectangle(outlinepen, 0, 0, iconwidth, iconwidth);

            ////DrawLine(g, lineele);
            //GeoBound bound = new GeoBound(0, 0, iconwidth, iconwidth);
            //GIS.Render.RenderAPI.DrawLineSymbol(g, lineele, bound, 1, true, null);

            //outlinepen.Dispose();
            //brush.Dispose();
            #endregion

        }

        private void DrawPtEleIcon(Graphics g, int index)
        {
            if (!m_mapui.m_conv_gtr.Extract_PtElement(index))
                return;
            DrawPointIcon(g);
            #region old
            //PointSymbol ptele = m_mapui.m_conv_gtr.generate_pointsymbol(new PointF(0, 0));

            //SolidBrush brush = new SolidBrush(Color.FromArgb(250, 0, 0, 0));
            //Pen outlinepen = new Pen(Color.FromArgb(100, 0, 255, 255));
            //Pen pen = new Pen(Color.FromArgb(255, 200, 0, 0));
            //pen.DashPattern = new float[] { 4f, 2.5f };

            //g.FillRectangle(brush, 1, 1, iconwidth - 1, iconwidth - 1);
            //g.DrawRectangle(outlinepen, 0, 0, iconwidth, iconwidth);

            //g.DrawLine(pen, new Point(iconwidth / 2, 0), new Point(iconwidth / 2, iconwidth));
            //g.DrawLine(pen, new Point(iconwidth, iconwidth / 2), new Point(0, iconwidth / 2));

            ////DrawPoint(g, ptele, -1);
            //GeoBound bound = new GeoBound(0, 0, iconwidth, iconwidth);
            //GIS.Render.RenderAPI.DrawPointSymbol(g, ptele, bound, m_scale, true, null);

            //outlinepen.Dispose();
            //brush.Dispose();
            #endregion
        }

        private void DrawPointIcon(Graphics g)
        {
            PointSymbol ptele = m_mapui.m_conv_gtr.generate_pointsymbol(new PointF(0, 0));

            SolidBrush brush = new SolidBrush(Color.FromArgb(250, 0, 0, 0));
            Pen outlinepen = new Pen(Color.FromArgb(80, 0, 0, 0));
            Pen pen = new Pen(Color.FromArgb(255, 200, 0, 0));
            pen.DashPattern = new float[] { 4f, 2.5f };

            g.FillRectangle(brush, 1, 1, iconwidth - 2, iconwidth - 2);
            g.DrawRectangle(outlinepen, 0, 0, iconwidth - 1, iconwidth - 1);

            g.DrawLine(pen, new Point(iconwidth / 2, 0), new Point(iconwidth / 2, iconwidth));
            g.DrawLine(pen, new Point(iconwidth, iconwidth / 2), new Point(0, iconwidth / 2));

            ////DrawPoint(g, ptele, -1);
            GeoBound bound = new GeoBound(0, 0, iconwidth, iconwidth);
            GIS.Render.RenderAPI.DrawPointSymbol(g, ptele, bound, m_scale, true, new GIS.Style.VectorStyle());

            outlinepen.Dispose();
            brush.Dispose();
        }

        private void DrawPointIcon(Graphics g, int index)
        {
            if (!m_mapui.m_conv_gtr.Extract_PointSymbol(index))
                return;

            DrawPointIcon(g);
            #region old
            //PointSymbol ptele = m_mapui.m_conv_gtr.generate_pointsymbol(new PointF(0, 0));

            //SolidBrush brush = new SolidBrush(Color.FromArgb(250, 0, 0, 0));
            //Pen outlinepen = new Pen(Color.FromArgb(100, 0, 255, 255));
            //Pen pen = new Pen(Color.FromArgb(255, 200, 0, 0));
            //pen.DashPattern = new float[] { 4f, 2.5f };

            //g.FillRectangle(brush, 1, 1, iconwidth - 1, iconwidth - 1);
            //g.DrawRectangle(outlinepen, 0, 0, iconwidth, iconwidth);

            //g.DrawLine(pen, new Point(iconwidth / 2, 0), new Point(iconwidth / 2, iconwidth));
            //g.DrawLine(pen, new Point(iconwidth, iconwidth / 2), new Point(0, iconwidth / 2));

            ////DrawPoint(g, ptele, -1.0f);
            //GeoBound bound = new GeoBound(0, 0, iconwidth, iconwidth);
            //GIS.Render.RenderAPI.DrawPointSymbol(g, ptele, bound, m_scale, true, null);

            //outlinepen.Dispose();
            //brush.Dispose();
#endregion
        }

        //private void DrawPoint(Graphics g, PointSymbol ptsym, float scale)
        //{
        //    float cur_scale = (scale == -1.0f ? m_scale : scale);
        //    for (int i = 0; i < ptsym.Atom.Count; i++)
        //    {
        //        if (ptsym.Atom[i] is Atom_Circle)
        //        {
        //            DrawCircle2ListView(g, ptsym.Atom[i] as Atom_Circle, cur_scale);
        //        }
        //        else if (ptsym.Atom[i] is Atom_Arc)
        //        {
        //            DrawArc2ListView(g, ptsym.Atom[i] as Atom_Arc, cur_scale);
        //        }
        //        else
        //        {   // is AtomLine4p
        //            DrawLine4p2ListView(g, ptsym.Atom[i] as Atom_Line4p, cur_scale);
        //        }
        //    }
        //}

        //private void DrawLine(Graphics g, LineSymbol linesym)
        //{
        //    for (int i = 0; i < linesym.Atom.Count; i++)
        //    {
        //        if (linesym.Atom[i] is Atom_DashLine)
        //        {
        //            //DrawDashline2ListView(g, linesym.Atom[i] as Atom_DashLine);
        //        }
        //        else if (linesym.Atom[i] is Atom_SolidLine)
        //        {
        //            //Drawline4l2ListView(g, linesym.Atom[i] as Atom_SolidLine);
        //        }

        //    }

        //    for (int i = 0; i < linesym.point_symbol.Count; i++)
        //    {
        //        DrawPoint(g, linesym.point_symbol[i], 1.0f);
        //    }
        //}

        //private void DrawCircle2ListView(Graphics g, Atom_Circle circle, float scale)
        //{
        //    float cur_scale = (scale == -1.0f ? m_scale : scale);
        //    int ncount = circle.Vertices.Count;
        //    PointF[] points = new PointF[ncount];
        //    for (int i = 0; i < ncount; i++)
        //    {
        //        points[i] = new PointF(iconwidth / 2 + circle.Vertices[i].X * cur_scale, iconwidth / 2 - circle.Vertices[i].Y * cur_scale);

        //    }

        //    Pen linePen = new Pen(Color.FromArgb(255, 0, 255, 245), circle.line_width);

        //    if (circle.bFill)
        //    {
        //        GraphicsPath path_Circle = new GraphicsPath();
        //        path_Circle.AddLines(points);
        //        SolidBrush brushFill = new SolidBrush(Color.Red);
        //        g.FillPath(brushFill, path_Circle);
        //        brushFill.Dispose();
        //        path_Circle.Dispose();
        //    }
        //    g.DrawLines(linePen, points);

        //    linePen.Dispose();

        //}

        //private void DrawArc2ListView(Graphics g, Atom_Arc arc, float scale)
        //{
        //    float cur_scale = (scale == -1.0f ? m_scale : scale);
        //    int ncount = arc.Vertices.Count;
        //    PointF[] points = new PointF[ncount];
        //    for (int i = 0; i < ncount; i++)
        //    {
        //        points[i] = new PointF(iconwidth / 2 + arc.Vertices[i].X * cur_scale, iconwidth / 2 - arc.Vertices[i].Y * cur_scale);

        //    }

        //    Pen linePen = new Pen(Color.FromArgb(255, 0, 255, 245), arc.line_width);

        //    if (arc.bFill)
        //    {
        //        GraphicsPath path_arc = new GraphicsPath();
        //        path_arc.AddLines(points);
        //        SolidBrush brushFill = new SolidBrush(Color.Red);
        //        g.FillPath(brushFill, path_arc);
        //        brushFill.Dispose();
        //        path_arc.Dispose();
        //    }

        //    g.DrawLines(linePen, points);

        //    linePen.Dispose();

        //}

        //private void DrawLine4p2ListView(Graphics g, Atom_Line4p line, float scale)
        //{
        //    float cur_scale = (scale == -1.0f ? m_scale : scale);
        //    int ncount = line.Vertices.Count;
        //    PointF[] points = new PointF[ncount];
        //    for (int i = 0; i < ncount; i++)
        //    {
        //        points[i] = new PointF(iconwidth / 2 + line.Vertices[i].X * cur_scale, iconwidth / 2 - line.Vertices[i].Y * cur_scale);

        //    }

        //    Pen linePen = new Pen(Color.FromArgb(255, 0, 255, 245), line.line_width);

        //    if (line.bfill)
        //    {
        //        GraphicsPath path_line = new GraphicsPath();
        //        path_line.AddLines(points);
        //        SolidBrush brushFill = new SolidBrush(Color.Red);
        //        g.FillPath(brushFill, path_line);
        //        brushFill.Dispose();
        //        path_line.Dispose();
        //    }

        //    g.DrawLines(linePen, points);

        //    linePen.Dispose();
        //}

        private void comboBox_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            RenderViewList();
        }

        private void delete_Click(object sender, EventArgs e)
        {
            MessageBox.Show("还在弄，别急！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            m_curindex = -1;
            m_curimage = null;
            this.DialogResult = DialogResult.Cancel;
        }

        private void ok_Click(object sender, EventArgs e)
        {
            if (this.listView_cursymbol.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择一项！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            m_curindex = this.listView_cursymbol.SelectedItems[0].Index;
            m_curimage = (Image)listView_cursymbol.LargeImageList.Images[m_curindex].Clone();
            this.DialogResult = DialogResult.OK;
        }

        private void search_Click(object sender, EventArgs e)
        {
            int id = System.Convert.ToInt32(this.searchid.Value);

            bool bfind = false;
            int i = 0;
            switch (this.comboBox_type.SelectedIndex)
            {
                case 0:  //点图元
                    bfind = false;
                    i = 0;
                    for (; i < m_mapui.m_conv_gtr.ptelefeatid.Count; i++)
                    {
                        if (m_mapui.m_conv_gtr.ptelefeatid[i] == id)
                        {
                            bfind = true;
                            break;
                        }
                    }
                    if (bfind)
                    {
                        this.listView_cursymbol.Items[i].Selected = true;
                        this.listView_cursymbol.Select();
                    }
                    else
                        MessageBox.Show("未找到!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 1:  //点符号
                    bfind = false;
                    i = 0;
                    for (; i < m_mapui.m_conv_gtr.pointfeatid.Count; i++)
                    {
                        if (m_mapui.m_conv_gtr.pointfeatid[i] == id)
                        {
                            bfind = true;
                            break;
                        }
                    }
                    if (bfind)
                    {
                        this.listView_cursymbol.Items[i].Selected = true;
                        this.listView_cursymbol.Select();
                    }
                    else
                        MessageBox.Show("未找到!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 2:  //线图元
                    bfind = false;
                    i = 0;
                    for (; i < m_mapui.m_conv_gtr.lineelefeatid.Count; i++)
                    {
                        if (m_mapui.m_conv_gtr.lineelefeatid[i] == id)
                        {
                            bfind = true;
                            break;
                        }
                    }
                    if (bfind)
                    {
                        this.listView_cursymbol.Items[i].Selected = true;
                        this.listView_cursymbol.Select();
                    }
                    else
                        MessageBox.Show("未找到!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 3:  //线符号
                    bfind = false;
                    i = 0;
                    for (; i < m_mapui.m_conv_gtr.linefeatid.Count; i++)
                    {
                        if (m_mapui.m_conv_gtr.linefeatid[i] == id)
                        {
                            bfind = true;
                            break;
                        }
                    }
                    if (bfind)
                    {
                        this.listView_cursymbol.Items[i].Selected = true;
                        this.listView_cursymbol.Select();
                    }
                    else
                        MessageBox.Show("未找到!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 4:  //面符号 
                    break;
                default:
                    break;
            }
        }



    }
}
