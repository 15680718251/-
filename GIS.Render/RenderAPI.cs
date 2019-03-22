using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GIS.Geometries;
using GIS.Utilities;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using GIS.mm_Conv_Symbol;
using GIS.GeoData;
using GIS.GdiAPI;

namespace GIS.Render
{
    public class RenderAPI
    {

        #region Vector

        /// <summary>
        /// 普通绘制点的样式
        /// </summary>
        /// <param name="g">传入的画布参数</param>
        /// <param name="pt">空间点坐标</param>
        /// <param name="style">绘制样式</param>
        /// <param name="map">传入地图控件参数，用来进行坐标转换</param>
        public static void DrawPoint(Graphics g, GeoPoint pt, Style.VectorStyle style, ITransForm map)
        {
            using (SolidBrush brush = new SolidBrush(style.FillColor))
            {
                PointF ptMap = map.TransFromWorldToMap(pt);
                RectangleF rectf = new RectangleF(ptMap.X - style.SymbolSize, ptMap.Y - style.SymbolSize, 2 * style.SymbolSize, 2 * style.SymbolSize);
                g.FillEllipse(brush, rectf);
                if (style.EnableOutLine)
                {
                    using (Pen outPen = new Pen(style.LineColor, style.LineSize))
                    {
                        g.DrawEllipse(outPen, rectf);
                    }
                }
            }
        }
        public static void DrawLineString(Graphics g, GeoLineString line, Style.VectorStyle style, ITransForm map)
        {
            if (line.NumPoints < 2)
            {
                return;
            }
            Point[] points = map.TransLineToMap(line);
            using (Pen linePen = new Pen(style.LineColor, style.LineSize))
            {
                g.DrawLines(linePen, points);
                //for (int i = 0; i < points.Length - 1; i ++)
                //{
                //    GDIAPI.MoveToEx(hdc, points[i].X, points[i].Y, (IntPtr)null);
                //    GDIAPI.LineTo(hdc, points[i + 1].X, points[i + 1].Y);
                //}

                if (style.IsSelected)
                {
                    SolidBrush brush = new SolidBrush(Style.VectorStyle.NodeBrushColor);
                    Pen pen = new Pen(Style.VectorStyle.NodePenColoe);
                    for (int i = 0; i < points.Length; i++)
                    {
                        g.FillRectangle(brush, points[i].X - 2.5f, points[i].Y - 2.5f, 5f, 5f);
                        g.DrawRectangle(pen, points[i].X - 3, points[i].Y - 3, 6, 6);
                    }
                    pen.Dispose();
                    brush.Dispose();
                }
            }
        }

        public static void DrawPolygonLineSring(Graphics g, GeoLineString line, Style.VectorStyle style ,ITransForm map)
        {
            if (line.NumPoints < 2)
            {
                return;
            }
            Point[] points = map.TransLineToMap(line);
            using (Pen linePen = new Pen(Color.FromArgb(255,style.FillColor), style.LineSize))
            {
                g.DrawLines(linePen, points);

                if (style.IsSelected)
                {
                    SolidBrush brush = new SolidBrush(Style.VectorStyle.NodeBrushColor);
                    Pen pen = new Pen(Style.VectorStyle.NodePenColoe);
                    for (int i = 0; i < points.Length; i++)
                    {
                        g.FillRectangle(brush, points[i].X - 2.5f, points[i].Y - 2.5f, 5f, 5f);
                        g.DrawRectangle(pen, points[i].X - 3, points[i].Y - 3, 6, 6);
                    }
                    pen.Dispose();
                    brush.Dispose();
                }
            }
        }

        public static void DrawSymbolPolygon(Graphics g, GeoPolygon polygon, Style.VectorStyle style, ITransForm map)
        {
            if (polygon.ExteriorRing.Vertices.Count < 4)
            {
                return;
            }

            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddPolygon(map.TransLineToMap(polygon.ExteriorRing));//渲染

            //增加内环(空洞)
            for (int i = 0; i < polygon.InteriorRings.Count; i++)
                gp.AddPolygon(map.TransLineToMap(polygon.InteriorRings[i]));//渲染

            SolidBrush brush = new SolidBrush(style.FillColor);
            g.FillPath(brush, gp);

            if (style.EnableOutLine)//如果需要绘制外边界
            {
                DrawPolygonLineSring(g, polygon.ExteriorRing, style, map);
                for (int i = 0; i < polygon.InteriorRings.Count; ++i)
                    DrawLineString(g, polygon.InteriorRings[i], style, map);
            }

        }

        public static void DrawPolygon(Graphics g, GeoPolygon polygon, Style.VectorStyle style, ITransForm map)
        {
            if (polygon.ExteriorRing.Vertices.Count < 4)
            {
                return;
            }

            //GraphicsPath path = new GraphicsPath();// 填充内部区域，去除岛屿
            //path.AddPolygon(map.TransLineToMap(polygon.ExteriorRing));

            //Region rgn = new Region(path);

            //for (int i = 0; i < polygon.InteriorRings.Count; ++i)
            //{
            //    Point[] pts = map.TransLineToMap(polygon.InteriorRings[i]);
            //    GraphicsPath pathExclude = new GraphicsPath();
            //    pathExclude.AddPolygon(pts);
            //    rgn.Exclude(pathExclude);
            //}

            //SolidBrush brush = new SolidBrush(style.FillColor);

            //g.FillRegion(brush, rgn);

            //brush.Dispose();
            //path.Dispose();


            #region 新问题修改

            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();

            gp.AddPolygon(map.TransLineToMap(polygon.ExteriorRing));//渲染

            //增加内环(空洞)
            for (int i = 0; i < polygon.InteriorRings.Count; i++)
                gp.AddPolygon(map.TransLineToMap(polygon.InteriorRings[i]));//渲染

            SolidBrush brush = new SolidBrush(style.FillColor);
            g.FillPath(brush, gp);

            #endregion



            if (style.EnableOutLine)//如果需要绘制外边界
            {
                DrawLineString(g, polygon.ExteriorRing, style, map);
                for (int i = 0; i < polygon.InteriorRings.Count; ++i)
                    DrawLineString(g, polygon.InteriorRings[i], style, map);
            }

        }
        public static void DrawMultiPoint(Graphics g, GeoMultiPoint points, Style.VectorStyle style, ITransForm map)
        {
            for (int i = 0; i < points.NumGeometries; i++)
            {
                DrawPoint(g, points[i], style, map);
            }
        }
        public static void DrawMultiLineString(Graphics g, GeoMultiLineString lines, Style.VectorStyle style, ITransForm map)
        {
            for (int i = 0; i < lines.NumGeometries; i++)
            {
                DrawLineString(g, lines[i], style, map);
            }
        }
        public static void DrawMultiPolygon(Graphics g, GeoMultiPolygon polygons, Style.VectorStyle style, ITransForm map)
        {
            for (int i = 0; i < polygons.NumGeometries; ++i)
            {
                DrawPolygon(g, polygons[i], style, map);
            }
        }

        public static void DrawSymbolMultiPolygon(Graphics g, GeoMultiPolygon polygons, Style.VectorStyle style, ITransForm map)
        {
            for (int i = 0; i < polygons.NumGeometries; ++i)
            {
                DrawSymbolPolygon(g, polygons[i], style, map);
            }
        }

        public static void DrawSymbol(Graphics g, GeoDataRow row, GeoBound bound, float scale, Style.VectorStyle style)
        {
            if (row.symbol is PointSymbol)
            {
                DrawPointSymbol(g, row.symbol as PointSymbol, bound, scale, false, style);
            }
            else if (row.symbol is LineSymbol)
            {
                DrawLineSymbol(g, row.symbol as LineSymbol, bound, scale, false, style);
            }
            else if (row.symbol is RegionSymbol)
            {
                DrawRegionSymbol(g, row.symbol as RegionSymbol, bound, scale, false, style);
            }
            #region ###
            //else if (geom is GeoMultiPoint)
            //{
            //    DrawMultiPoint(g, (GeoMultiPoint)geom, style, map);
            //}
            //else if (geom is GeoMultiLineString)
            //{
            //    DrawMultiLineString(g, (GeoMultiLineString)geom, style, map);
            //}
            //else if (geom is GeoMultiPolygon)
            //{
            //    DrawMultiPolygon(g, (GeoMultiPolygon)geom, style, map);
            //}
            //else if (geom is GeoLabel)
            //{
            //    DrawLabel(g, (GeoLabel)geom, style, map);
            //}
            #endregion
        }

        public static void DrawGeometry(Graphics g, Geometry geom, Style.VectorStyle style, ITransForm map)
        {
            if (geom is GeoPoint)
            {
                DrawPoint(g, (GeoPoint)geom, style, map);
            }
            else if (geom is GeoLineString)
            {
                DrawLineString(g, (GeoLineString)geom, style, map);
            }
            else if (geom is GeoPolygon)
            {
                DrawPolygon(g, (GeoPolygon)geom, style, map);
            }
            else if (geom is GeoMultiPoint)
            {
                DrawMultiPoint(g, (GeoMultiPoint)geom, style, map);
            }
            else if (geom is GeoMultiLineString)
            {
                DrawMultiLineString(g, (GeoMultiLineString)geom, style, map);
            }
            else if (geom is GeoMultiPolygon)
            {
                DrawMultiPolygon(g, (GeoMultiPolygon)geom, style, map);
            }
            else if (geom is GeoLabel)
            {
                DrawLabel(g, (GeoLabel)geom, style, map);
            }
        }

        public static void DrawSymbolGeometry(Graphics g, Geometry geom, Style.VectorStyle style, ITransForm map)
        {
            if (geom is GeoPoint)
            {
                DrawPoint(g, (GeoPoint)geom, style, map);
            }
            else if (geom is GeoLineString)
            {
                DrawLineString(g, (GeoLineString)geom, style, map);
            }
            else if (geom is GeoPolygon)
            {
                DrawSymbolPolygon(g, (GeoPolygon)geom, style, map);
            }
            else if (geom is GeoMultiPoint)
            {
                DrawMultiPoint(g, (GeoMultiPoint)geom, style, map);
            }
            else if (geom is GeoMultiLineString)
            {
                DrawMultiLineString(g, (GeoMultiLineString)geom, style, map);
            }
            else if (geom is GeoMultiPolygon)
            {
                DrawSymbolMultiPolygon(g, (GeoMultiPolygon)geom, style, map);
            }
            else if (geom is GeoLabel)
            {
                DrawLabel(g, (GeoLabel)geom, style, map);
            }
        }

        
        #endregion

        public static void DrawPointSymbol(Graphics g, PointSymbol ptsym, GeoBound bound, float scale, bool bicon, Style.VectorStyle style)
        {
            for (int i = 0; i < ptsym.Atom.Count; i++)
            {
                if (ptsym.Atom[i] is Atom_Circle)
                {
                    DrawCircle(g, ptsym.Atom[i] as Atom_Circle, bound, scale, bicon, style);
                }
                else if (ptsym.Atom[i] is Atom_Arc)
                {
                    DrawArc(g, ptsym.Atom[i] as Atom_Arc, bound, scale, bicon, style);
                }
                else
                {   // is AtomLine4p
                    DrawLine4p(g, ptsym.Atom[i] as Atom_Line4p, bound, scale, bicon, style);
                }
            }

        }

        public static void DrawLineSymbol(Graphics g, LineSymbol linesym, GeoBound bound, float scale, bool bicon, Style.VectorStyle style)
        {
            for (int i = 0; i < linesym.Atom.Count; i++)
            {
                if (linesym.Atom[i] is Atom_DashLine)
                {
                    DrawDashline(g, linesym.Atom[i] as Atom_DashLine, bound, scale, bicon, style);
                }
                else if (linesym.Atom[i] is Atom_SolidLine)
                {
                    Drawline4l(g, linesym.Atom[i] as Atom_SolidLine, bound, scale, bicon, style);
                }

            }

            for (int i = 0; i < linesym.point_symbol.Count; i++)
            {
                DrawPointSymbol(g, linesym.point_symbol[i], bound, scale, bicon, style);
            }

        }

        public static void DrawRegionSymbol(Graphics g, RegionSymbol regionsym, GeoBound bound, float scale, bool bicon, Style.VectorStyle style)
        {
            for (int i = 0; i < regionsym.fillsymbol.Count; i++)
            {
                DrawPointSymbol(g, regionsym.fillsymbol[i], bound, scale, bicon, style);
            }
            DrawLineSymbol(g, regionsym.outline, bound, scale, bicon, style);
        }

        private static void Drawline4l(Graphics g, Atom_SolidLine line, GeoBound bound, float scale, bool bicon, Style.VectorStyle style)
        {
            float cur_scale = scale;
            int ncount = line.Vertices.Count;
            PointF[] points = new PointF[ncount];
            if (bicon)
            {
                for (int i = 0; i < ncount; i++)
                {

                    points[i] = new PointF((float)bound.Width / 2 + line.Vertices[i].X * cur_scale, (float)bound.Height / 2 - line.Vertices[i].Y * cur_scale);
                }
            }
            else
            {
                ///to be continued
                points = TransLineToMapF(line.Vertices, bound, scale);
                cur_scale = 1 / scale;
            }
            Color clr = Color.FromArgb(255, 0, 255, 245);

            Pen linePen = new Pen(bicon ? clr : style.FillColor, line.line_width == 0.15f ? line.line_width : line.line_width * cur_scale);
            g.DrawLines(linePen, points);
            linePen.Dispose();

        }

        private static void DrawDashline(Graphics g, Atom_DashLine dashline, GeoBound bound, float scale, bool bicon, Style.VectorStyle style)
        {
            float cur_scale = scale;
            int ncount = dashline.Vertices.Count;
            PointF[] points = new PointF[ncount];
            if (bicon)
            {
                for (int i = 0; i < ncount; i++)
                {

                    points[i] = new PointF(((float)bound.Width / 2 + dashline.Vertices[i].X * cur_scale), ((float)bound.Height / 2 - dashline.Vertices[i].Y * cur_scale));

                }
            }
            else
            {
                points = TransLineToMapF(dashline.Vertices, bound, scale);
                cur_scale = 1 / scale;
            }

            Color clr = Color.FromArgb(255, 0, 255, 245);
            Pen linePen = new Pen(bicon ? clr : style.FillColor, dashline.line_width == 0.15f ? dashline.line_width : dashline.line_width * cur_scale);

            for (int i = 0; i < ncount - 1; i += 2)
            {
                g.DrawLine(linePen, points[i], points[i + 1]);

            }

            linePen.Dispose();
        }

        private static PointF[] TransLineToMapF(List<PointF> line, GeoBound bound, double PixelAspectRatio)
        {
            if (line == null)
                return null;
            PointF[] pts = new PointF[line.Count];
            for (int i = 0; i < line.Count; i++)
            {
                pts[i] = MapToScreenF(line[i], bound, PixelAspectRatio);
            }
            return pts;
        }
        private static Point[] TransLineToMap(List<PointF> line, GeoBound bound, double PixelAspectRatio)
        {
            if (line == null)
                return null;
            Point[] pts = new Point[line.Count];
            for (int i = 0; i < line.Count; i++)
            {
                pts[i] = MapToScreen(line[i], bound, PixelAspectRatio);
            }
            return pts;
        }

        public static Point MapToScreen(PointF pt, GeoBound bound, double PixelAspectRatio)
        {
            int x = (int)((pt.X - bound.Left) / PixelAspectRatio);
            int y = (int)((bound.Top - pt.Y) / PixelAspectRatio);
            return new Point(x, y);
        }

        public static PointF MapToScreenF(PointF pt, GeoBound bound, double PixelAspectRatio)
        {
            float x = (float)((pt.X - bound.Left) / PixelAspectRatio);
            float y = (float)((bound.Top - pt.Y) / PixelAspectRatio);
            return new PointF(x, y);
        }

        private static void DrawCircle(Graphics g, Atom_Circle circle, GeoBound bound, float scale, bool bicon, Style.VectorStyle style)
        {
            int ncount = circle.Vertices.Count;
            PointF[] points = new PointF[ncount];
            if (bicon)
            {
                for (int i = 0; i < ncount; i++)
                {
                    points[i] = new PointF((float)bound.Width / 2 + circle.Vertices[i].X * scale, (float)bound.Height / 2 - circle.Vertices[i].Y * scale);
                }
            }
            else
            {
                ///to be continued
                points = TransLineToMapF(circle.Vertices, bound, scale);
            }
            Color clr = Color.FromArgb(255, 0, 255, 245);
            Pen linePen = new Pen(bicon ? clr : style.FillColor, circle.line_width == 0.15f ? circle.line_width : circle.line_width * scale);

            if (circle.bpoint && ncount == 1)
            {
                g.DrawEllipse(linePen, new RectangleF(points[0].X - 0.15f, points[0].Y - 0.15f, 0.3f, 0.3f));
                return;
            }

            if (circle.bFill)
            {
                GraphicsPath path_Circle = new GraphicsPath();
                path_Circle.AddLines(points);
                SolidBrush brushFill = new SolidBrush(bicon ? clr : style.FillColor);
                g.FillPath(brushFill, path_Circle);
                brushFill.Dispose();
                path_Circle.Dispose();
                //GDIAPI.BeginPath(hdc);
                //for (int i = 0; i < points.Length - 1; i++)
                //{
                //    GDIAPI.MoveToEx(hdc, (int)points[i].X, (int)points[i].Y, (IntPtr)null);
                //    GDIAPI.LineTo(hdc, (int)points[i + 1].X, (int)points[i + 1].Y);
                //}
                //GDIAPI.EndPath(hdc);
                //IntPtr brushfill = GDIAPI.CreateSolidBrush(bicon ? GDIAPI.RGB(0, 0, 0) : GDIAPI.RGB(0, 0, 0));
                //GDIAPI.SelectObject(hdc, brushfill);
                //GDIAPI.FillPath(hdc);
                //GDIAPI.DeleteObject(brushfill);
            }
            g.DrawLines(linePen, points);

            //for (int i = 0; i < points.Length - 1; i++)
            //{
            //    GDIAPI.MoveToEx(hdc, (int)points[i].X, (int)points[i].Y, (IntPtr)null);
            //    GDIAPI.LineTo(hdc, (int)points[i + 1].X, (int)points[i + 1].Y);
            //}

            linePen.Dispose();
        }

        private static void DrawArc(Graphics g, Atom_Arc arc, GeoBound bound, float scale, bool bicon, Style.VectorStyle style)
        {
            int ncount = arc.Vertices.Count;
            PointF[] points = new PointF[ncount];
            if (bicon)
            {
                for (int i = 0; i < ncount; i++)
                {

                    points[i] = new PointF((float)bound.Width / 2 + arc.Vertices[i].X * scale, (float)bound.Height / 2 - arc.Vertices[i].Y * scale);

                }
            }
            else
            {
                ///to be continued
                points = TransLineToMapF(arc.Vertices, bound, scale);
            }
            Color clr = Color.FromArgb(255, 0, 255, 245);
            Pen linePen = new Pen(bicon ? clr : style.FillColor, arc.line_width == 0.15f ? arc.line_width : arc.line_width * scale);

            if (arc.bFill)
            {
                GraphicsPath path_arc = new GraphicsPath();
                path_arc.AddLines(points);
                SolidBrush brushFill = new SolidBrush(bicon ? clr : style.FillColor);
                g.FillPath(brushFill, path_arc);
                brushFill.Dispose();
                path_arc.Dispose();
            }

            g.DrawLines(linePen, points);
            //for (int i = 0; i < points.Length - 1; i++)
            //{
            //    GDIAPI.MoveToEx(hdc, (int)points[i].X, (int)points[i].Y, (IntPtr)null);
            //    GDIAPI.LineTo(hdc, (int)points[i + 1].X, (int)points[i + 1].Y);
            //}
            linePen.Dispose();
        }

        private static void DrawLine4p(Graphics g, Atom_Line4p line, GeoBound bound, float scale, bool bicon, Style.VectorStyle style)
        {
            int ncount = line.Vertices.Count;
            PointF[] points = new PointF[ncount];
            if (bicon)
            {
                for (int i = 0; i < ncount; i++)
                {

                    points[i] = new PointF((float)bound.Width / 2 + line.Vertices[i].X * scale, (float)bound.Height / 2 - line.Vertices[i].Y * scale);

                }
            }
            else
            {
                ///to be continued
                points = TransLineToMapF(line.Vertices, bound, scale);
            }
            Color clr = Color.FromArgb(255, 0, 255, 245);
            Pen linePen = new Pen(bicon ? clr : style.FillColor, line.line_width == 0.15f ? line.line_width : line.line_width * scale);

            if (line.bfill)
            {
                GraphicsPath path_line = new GraphicsPath();
                path_line.AddLines(points);
                SolidBrush brushFill = new SolidBrush(bicon ? clr : style.FillColor);
                g.FillPath(brushFill, path_line);
                brushFill.Dispose();
                path_line.Dispose();
            }

            g.DrawLines(linePen, points);
            //for (int i = 0; i < points.Length - 1; i++)
            //{
            //    GDIAPI.MoveToEx(hdc, (int)points[i].X, (int)points[i].Y, (IntPtr)null);
            //    GDIAPI.LineTo(hdc, (int)points[i + 1].X, (int)points[i + 1].Y);
            //}

            linePen.Dispose();
        }

        private static void DrawLabel(Graphics g, GeoLabel lable, GIS.Style.VectorStyle style, ITransForm map)
        {
            Color clr = lable.Color; 
            DrawLabel(g, lable, clr, map);
        } 

        public static void DrawLabel(Graphics g, GeoLabel lable, Color  clr, ITransForm map)
        {

            using (SolidBrush brush = new SolidBrush(clr))
            {

                lable.Transform = map;
                float fontsize = (float)Math.Ceiling(map.TransFromWorldToMap(lable.TextSize));  //字体的像素大小，通过实地大小转换 

                Font font = new Font(lable.FontName, fontsize, GraphicsUnit.Pixel); //设置字体

                float fontheight = font.GetHeight(g); //字体的高度  
                if (lable.EndPt == null)
                {
                    SizeF size = g.MeasureString(lable.Text, font);
                    double width = map.TransFromMapToWorld(size.Width);
                    GeoPoint NoRotatePt = new GeoPoint(lable.StartPt.X + width, lable.StartPt.Y);
                    GeoPoint EndPt = SpatialRelation.GeoAlgorithm.PointRotate(NoRotatePt, lable.StartPt, lable.Angle);
                    lable.EndPt = EndPt;
                }
                float fLength = map.TransFromWorldToMap(lable.Length);//字符串的实际总长度

                float fRotateAngle = lable.RotateAngle;    //旋转角度
                float fInterval = (float)fLength / (lable.Text.Length);     //间隔              


                PointF StartPt = map.TransFromWorldToMapF(lable.StartPt);  //旋转中心，找到字符的原点 左下角
                StartPt.Y -= fontheight;

                Matrix mtx = new Matrix();
                mtx.RotateAt(fRotateAngle, StartPt);//矩阵旋转
                g.Transform = mtx;
                try
                {
                    for (int k = 0; k < lable.Text.Length; k++)
                    {
                        string onechar = lable.Text[k].ToString();
                        g.DrawString(onechar, font, brush, StartPt);
                        StartPt.X += fInterval;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                finally
                {
                    g.ResetTransform();

                    font.Dispose();
                    mtx.Dispose();
                }
            }
        }
    }
}
