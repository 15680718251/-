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
using GIS.Geometries;

namespace GIS.TreeIndex.Forms
{
    public partial class SymbolDesigner : Form
    {


        public SymbolDesigner(MapUI ui)
        {
            m_mapui = ui;
            InitializeComponent();
            m_cache4p_circle = new List<List<float>>();
            m_cache4p_arc = new List<List<float>>();
            m_cache4p_line = new List<List<float>>();
            m_currentpoints = new List<float>();

            m_cache4l_line = new List<List<float>>();
            m_cache4l_dashline = new List<List<float>>();
            m_cache4l_pointline = new List<List<float>>();

            m_cache4region = new List<List<float>>();
        }

        #region private members
        MapUI m_mapui; //便于绘画
        #endregion


        


        private void button10_Click(object sender, EventArgs e)
        {
           
            //预览
            Bitmap bmp = new Bitmap(this.previewbox.Width, this.previewbox.Height);
            
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.Default;

            Pen redpen = new Pen(Color.FromArgb(255, 200, 0, 0));
            
            float[] dashpp = { 6, 4 };
            redpen.DashPattern = dashpp;
            //redpen.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(redpen, new PointF(0, this.previewbox.Height / 2 ), new PointF(this.previewbox.Width , this.previewbox.Height / 2 ));
            g.DrawLine(redpen, new PointF(this.previewbox.Width / 2  , 0), new PointF(this.previewbox.Width / 2 , this.previewbox.Height ));
            redpen.Dispose();

            // Create font and brush.
            Font drawFont = new Font("Arial", 12, GraphicsUnit.Pixel);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            // Draw string to screen.
            g.DrawString("X", drawFont, drawBrush, new PointF(this.previewbox.Width - 25, this.previewbox.Height / 2));
            g.DrawString("Y", drawFont, drawBrush, new PointF(this.previewbox.Width / 2 + 3, 3));
 
            drawFont.Dispose();
            drawBrush.Dispose();


            if (点符号 == this.tabControl1.SelectedTab)
            {
                PointSymbol pointsym = new PointSymbol();

                for (int i = 0; i < this.m_cache4p_arc.Count; i++)
                {
                        Atom_Arc arc = new Atom_Arc(0, 0, m_cache4p_arc[i][0], m_cache4p_arc[i][1], m_cache4p_arc[i][2],
                        m_cache4p_arc[i][3], m_cache4p_arc[i][4], (int)m_cache4p_arc[i][5] == 1 ? true : false, m_cache4p_arc[i][6],
                        m_cache4p_arc[i][7], m_cache4p_arc[i][8], m_cache4p_arc[i][9], (int)m_cache4p_arc[i][10] == 1 ? true : false);

                    pointsym.AddAtom(arc);

                }

                for (int i = 0; i < this.m_cache4p_circle.Count; i++)
                {
                    Atom_Circle circle = new Atom_Circle(0, 0, m_cache4p_circle[i][0],
                        m_cache4p_circle[i][1], m_cache4p_circle[i][2], m_cache4p_circle[i][3], m_cache4p_circle[i][4], (int)m_cache4p_circle[i][5] == 1 ? true : false,
                        m_cache4p_circle[i][6], m_cache4p_circle[i][7]);

                    pointsym.AddAtom(circle);

                }

                for (int i = 0; i < this.m_cache4p_line.Count; i++)
                {
                    int ncount = m_cache4p_line[i].Count - 7;

                    PointF[] pts = new PointF[ncount / 2];

                    for (int j = 0, k = 7; k < m_cache4p_line[i].Count - 1; k +=2, j++)
                    {
                        pts[j].X = m_cache4p_line[i][k];
                        pts[j].Y = m_cache4p_line[i][k + 1];
                    }
                    
                    Atom_Line4p line = new Atom_Line4p(0, 0, m_cache4p_line[i][0], m_cache4p_line[i][1], m_cache4p_line[i][2], m_cache4p_line[i][3], m_cache4p_line[i][4],
                        (int)m_cache4p_line[i][5] == 1 ? true : false, m_cache4p_line[i][6], pts);

                    pointsym.AddAtom(line);

                }
                //DrawPtSym2Preview(g, pointsym, -1);
                GeoBound bound = new GeoBound(0, 0, this.previewbox.Width, this.previewbox.Height);
                GIS.Render.RenderAPI.DrawPointSymbol(g, pointsym, bound, m_scale, true, new GIS.Style.VectorStyle());


            }
            else if (线符号 == this.tabControl1.SelectedTab)
            {

                LineSymbol linesym = new LineSymbol();

                float len2presavex = previewbox.Width / 10;
                float len2presavey = previewbox.Height / 10;

                float Width2 = previewbox.Width / 2;
                float Height2 = previewbox.Height / 2;

                float x = Width2  - len2presavex;
                float y = Height2 - len2presavey;

                float xx = len2presavex ;
                float yy = len2presavey ;

                //PointF[] pts = { new PointF(-xx, yy), new PointF(xx, yy), new PointF(xx, -yy), new PointF(-xx - 20, -yy)
                //                  , new PointF(-xx - 20, yy) ,new PointF(0, y), new PointF(x, 0), new PointF(0, -y), new PointF(-x, 0)};

               // PointF[] pts = { new PointF(-xx, yy), new PointF(xx, yy) };

                PointF[] pts = { new PointF(-xx, yy), new PointF(xx, yy), new PointF(xx, -yy), new PointF(-xx, -yy), new PointF(-xx - 10, yy) };

                float[] verlen = new float[pts.Length - 1];
                float[] rad = new float[pts.Length - 1];
                float dy = 0f, dx = 0f;
                for (int i = 0; i < pts.Length - 1; i++)
                {
                    dy = pts[i + 1].Y - pts[i].Y;
                    dx = pts[i + 1].X - pts[i].X;

                    verlen[i] = System.Convert.ToSingle(Math.Sqrt(Math.Pow((double)(dx), 2) + Math.Pow((double)(dy), 2)));
                    rad[i] = (float)Math.Atan((double)(dy / dx));
                }


                for (int i = 0; i < this.m_cache4l_line.Count; i++)
                {
                    Atom_SolidLine solidline = new Atom_SolidLine(m_cache4l_line[i][0], m_cache4l_line[i][1], pts, verlen, rad);
                    
                    linesym.AddAtom(solidline);
                }


                for (int i = 0; i < this.m_cache4l_dashline.Count; i++)
                {
                    Atom_DashLine dashline = new Atom_DashLine(m_cache4l_dashline[i][0], m_cache4l_dashline[i][1], m_cache4l_dashline[i][2],
                       /* m_cache4l_dashline[i][3],*/ m_cache4l_dashline[i][4], m_cache4l_dashline[i][5], pts, verlen, rad);

                    linesym.AddAtom(dashline);
                }


                for (int i = 0; i < this.m_cache4l_pointline.Count; i++)
                {
                    int ptsymbolID = System.Convert.ToInt32(m_cache4l_pointline[i][0]);

                    if (ptsymbolID >= 0 && m_mapui.m_conv_gtr.Extract_PtElementbyID(ptsymbolID))//valid
                    {
                        PointSymbol ptsymbol = m_mapui.m_conv_gtr.generate_pointsymbol(new PointF(0, 0));
                        linesym.AddPointSymbol(ptsymbol, pts, m_cache4l_pointline[i][1],
                            m_cache4l_pointline[i][2], m_cache4l_pointline[i][3], verlen, rad);
                    }

                }

                //DrawLineSym2Preview(g, linesym, -1);
                GeoBound bound = new GeoBound(0, 0, this.previewbox.Width, this.previewbox.Height);
                GIS.Render.RenderAPI.DrawLineSymbol(g, linesym, bound, m_scale, true, new GIS.Style.VectorStyle());
             
            }
            else//面符号
            {
                float len2presavex = previewbox.Width / 10;
                float len2presavey = previewbox.Height / 10;

                float Width2 = previewbox.Width / 2;
                float Height2 = previewbox.Height / 2;

                float x = Width2  - len2presavex;
                float y = Height2 - len2presavey;

                RegionSymbol regionsym = new RegionSymbol();

                List<GeoPoint> list = new List<GeoPoint>();
                list.Add(new GeoPoint(x, y));
                list.Add(new GeoPoint(-x, y));
                list.Add(new GeoPoint(-x, -y));
                list.Add(new GeoPoint(x, -y));

                GeoLinearRing exring = new GeoLinearRing(list);
                PointSymbol testpt0 = new PointSymbol();
                //testpt.AddAtom(new Atom_Circle(0, 0, 0, 0, 0, 0, 0, false, 1, 3));
                //PointF[] rect =  {new PointF(1.5f, 1.5f), new PointF(1.5f, -1.5f)
                  //  , new PointF(-1.5f, -1.5f), new PointF(-1.5f, 1.5f), new PointF(1.5f, 1.5f)};

                PointF[] line1 = { new PointF(1.5f, 0), new PointF(1.5f, 3) };
                PointF[] line2 = { new PointF(-1.5f, 0), new PointF(-1.5f, 3) };

                //testpt.AddAtom(new Atom_Line4p(0, 0, 0, 0, 0, 0, 0, false, 1, rect));

                testpt0.AddAtom(new Atom_Line4p(0, 0, 0, -1.5f, 0, 0, 0, false, 0.5f, line1));
                testpt0.AddAtom(new Atom_Line4p(0, 0, 0, -1.5f, 0, 0, 0, false, 0.5f, line2));

                PointF[] line3 = { new PointF(1.5f, 1.5f), new PointF(-1.5f, -1.5f) };
                PointF[] line4 = { new PointF(1.5f, -1.5f), new PointF(-1.5f, 1.5f) };

                PointSymbol testpt1 = new PointSymbol();
                testpt1.AddAtom(new Atom_Line4p(0, 0, 0, 0, 0, 0, 0, false, 0.5f, line3));
                testpt1.AddAtom(new Atom_Line4p(0, 0, 0, 0, 0, 0, 0, false, 0.5f, line4));


                regionsym.add_fillsymbol(new List<GeoLinearRing>(), exring, exring.Bound, testpt0, testpt1, 20, 30/*, true, true*/);

                LineSymbol outline = new LineSymbol();
                PointF[] pt = {new PointF(x, y), new PointF(-x, y), new PointF(-x, -y), new PointF(x, -y), new PointF(x, y)};
                outline.Atom.Add(new Atom_SolidLine(1, pt));

                regionsym.appoint_outline(outline);


                GeoBound bound = new GeoBound(0, 0, this.previewbox.Width, this.previewbox.Height);
                //IntPtr hdc = GdiAPI.GDIAPI.GetDC(this.m_mapui.Handle);
                GIS.Render.RenderAPI.DrawRegionSymbol(g, regionsym, bound, 1, true, new GIS.Style.VectorStyle());
                //GdiAPI.GDIAPI.ReleaseDC(hdc);
            }

            previewbox.Image = bmp;
           
        }

        private void SymbolDesigner_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_mapui.m_conv_gtr.WriteSymbol2Disk();
        }

      

        private void cursymbol_Click(object sender, EventArgs e)
        {
            CurSymbolForm form = new CurSymbolForm(m_mapui, -1);
            form.ShowDialog();
        }

        private void scale_select_ValueChanged(object sender, EventArgs e)
        {
            m_scale = 1.0f + this.scale_select.Value;
            this.button10_Click(null, null);
        }

























    
    
    }
}
