using System.Text;
using GIS.Geometries;
using System.Drawing;
using System.Drawing.Drawing2D;
using System;

namespace GIS.mm_Conv_Symbol
{
    public class Atom_LineBase : Atom_Base
    {
        public Atom_LineBase()
        {

        }

        public virtual Atom_PointBase Clone()
        {
            return null;
        }

        //public bool line_line_intersectpoint(float x1, float y1, float x2, float y2,
        //                     float x3, float y3, float x4, float y4,
        //                     ref float xc, ref float yc)
        //{
        //    float a1, b1, c1, a2, b2, c2, d;
        //    xc = 1e20f;
        //    yc = 1e20f;
        //    a1 = y1 - y2;
        //    b1 = x2 - x1;
        //    c1 = y2 * x1 - y1 * x2;
        //    a2 = y3 - y4;
        //    b2 = x4 - x3;
        //    c2 = y4 * x3 - y3 * x4;
        //    d = a1 * b2 - a2 * b1;
        //    //d==0表示两直线平行
        //    if (Math.Abs(d) > 1e-5)
        //    {
        //        xc = (-c1 * b2 + c2 * b1) / d;
        //        yc = (-a1 * c2 + a2 * c1) / d;
        //        return true;
        //    }
        //    else
        //        return false;
        //}

        //public  PointF[] gen_paraline(float udoffset, PointF[] vertex, float[] ver_len, float[] rad)
        //{
        //    PointF[] paral_vertices = new PointF[vertex.Length];
        //    for (int i = 0; i < vertex.Length; i++)
        //    {
        //        paral_vertices[i].X = vertex[i].X;
        //        paral_vertices[i].Y = vertex[i].Y;
        //    }

        //    mm_matrix mtx = new mm_matrix();

        //    PointF[] tempvertex = new PointF[((vertex.Length - 1) * 2)];

        //    float base_x, base_y;
        //    float noffset, ndz;

        //    for (int i = 0, j = 0; i < vertex.Length - 1; i++, j = j + 2)
        //    {
        //        tempvertex[j].X = vertex[i].X;
        //        tempvertex[j].Y = vertex[i].Y;
        //        tempvertex[j + 1].X = vertex[i + 1].X;
        //        tempvertex[j + 1].Y = vertex[i + 1].Y;

        //        float dy = vertex[i + 1].Y - vertex[i].Y;
        //        float dx = vertex[i + 1].X - vertex[i].X;
        //        //float dz = System.Convert.ToSingle(Math.Sqrt(System.Convert.ToDouble((Math.Pow((double)dy, 2.0) + Math.Pow((double)dx, 2.0)))));
        //        //float bnn = dy / dx;
        //        //float rad_angle = System.Convert.ToSingle(Math.Atan(bnn));

        //        base_x = vertex[i].X;
        //        base_y = vertex[i].Y;

        //        mtx.reset();
        //        mtx.rotate_at(-rad[i], base_x, base_y);


        //        mtx.transform(ref tempvertex[j]);
        //        mtx.transform(ref tempvertex[j + 1]);

        //        noffset = udoffset;
        //        ndz = ver_len[i];

        //        if (dx < 0)
        //        {
        //            noffset = -noffset;
        //            //if (dy >= 0)
        //                ndz = -ndz;            
        //        }

        //        tempvertex[j].X = tempvertex[j].X;
        //        tempvertex[j].Y = tempvertex[j].Y + noffset;
        //        tempvertex[j + 1].X = tempvertex[j].X + ndz;
        //        tempvertex[j + 1].Y = tempvertex[j].Y;

        //        mtx.reset();
        //        mtx.rotate_at(rad[i], base_x, base_y);

        //        mtx.transform(ref tempvertex[j]);
        //        mtx.transform(ref tempvertex[j + 1]);
        //    }

        //    // 0,1---> 2,3; 2,3---> 4,5;
        //    for (int j = 0, index = 1; j < tempvertex.Length - 3; j = j + 2, index++)
        //    {
        //        float tmpx = 0, tmpy = 0;
        //        line_line_intersectpoint(tempvertex[j].X, tempvertex[j].Y, tempvertex[j + 1].X, tempvertex[j + 1].Y,
        //                                 tempvertex[j + 2].X, tempvertex[j + 2].Y, tempvertex[j + 3].X, tempvertex[j + 3].Y, ref tmpx, ref tmpy);

        //        paral_vertices[index].X = tmpx;
        //        paral_vertices[index].Y = tmpy;
        //    }
        //    paral_vertices[0].X = tempvertex[0].X;
        //    paral_vertices[0].Y = tempvertex[0].Y;
        //    paral_vertices[vertex.Length - 1].X = tempvertex[tempvertex.Length - 1].X;
        //    paral_vertices[vertex.Length - 1].Y = tempvertex[tempvertex.Length - 1].Y;


        //    return paral_vertices;

        //}
    }

}