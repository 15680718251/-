using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace GIS.mm_Conv_Symbol
{
    //created by jq  2011.1.26

    //---------------------------------------------------------------------
    // [x, y, 1] *  sx   shy  0    default :     1   0   0
    //              shx  sy   0                     0   1   0
    //              tx   ty   1                     0   0   1
    //----------------------------------------------------------------------
    //mm_matrix使用笛卡尔坐标系进行计算

    public class mm_matrix
    {
        private float sx, shy, shx, sy, tx, ty;

        public mm_matrix()
        {//默认构造函数
            sx = 1.0f;
            shy = 0f;
            shx = 0f;
            sy = 1.0f;
            tx = 0f;
            ty = 0f;
        }

        public mm_matrix(float[] mtx)
        { // 用一数组初始化matrix
            sx = mtx[0];
            shy = mtx[1];
            shx = mtx[2];
            sy = mtx[3];
            tx = mtx[4];
            ty = mtx[5];
        }


        public mm_matrix loadfrom(float[] mtx)
        {
            sx = mtx[0];
            shy = mtx[1];
            shx = mtx[2];
            sy = mtx[3];
            tx = mtx[4];
            ty = mtx[5];
            return this;
        }

        public mm_matrix storeto(float[] mtx)
        {
            mtx[0] = sx;
            mtx[1] = shy;
            mtx[2] = shx;
            mtx[3] = sy;
            mtx[4] = tx;
            mtx[5] = ty;
            return this;
        }

        public mm_matrix reset()
        {
            sx = sy = 1.0f;
            shy = shx = tx = ty = 0f;
            return this;
        }

        public mm_matrix translate(float x, float y)
        {
            tx += x;
            ty += y;
            return this;
        }

        public mm_matrix rotate(float a)
        {   //逆时针
            //angle: a
            float ca = System.Convert.ToSingle(Math.Cos(a));
            float sa = System.Convert.ToSingle(Math.Sin(a));
            float t0 = sx * ca - shy * sa;
            float t2 = shx * ca - sy * sa;
            float t4 = tx * ca - ty * sa;
            shy = sx * sa + shy * ca;
            sy = shx * sa + sy * ca;
            ty = tx * sa + ty * ca;
            sx = t0;
            shx = t2;
            tx = t4;
            return this;
        }

        public mm_matrix rotate_at(float a,  float x,  float y)
        {
            //逆时针
            //rotate_at pt(x, y)
            translate(-x, -y);
            rotate(a);
            translate(x, y);
            return this;


        }

        public void transform(ref float x, ref float y)
        {
            float tmp = x;
            x = tmp * sx + y * shx + tx;
            y = tmp * shy + y * sy + ty;
        }

        public void transform(ref PointF pt)
        {
            float tmp = pt.X;
            pt.X = tmp * sx + pt.Y * shx + tx;
            pt.Y = tmp * shy + pt.Y * sy + ty;
        }

        public void transform(ref PointF[] pt)
        {
            for (int i = 0; i < pt.Length; i++)
            {
                transform(ref pt[i]);
            }
        }



        public void transform_2x2(ref float x, ref float y)
        {
            float tmp = x;
            x = tmp * sx + y * shx;
            y = tmp * shy + y * sy;
        }

        public mm_matrix scale(float x, float y)
        {
            sx *= x;
            shx *= x;
            tx *= x;
            shy *= y;
            sy *= y;
            ty *= y;
            return this;
        }

        public mm_matrix scale(float m)
        {
            sx *= m;
            shx *= m;
            tx *= m;
            shy *= m;
            sy *= m;
            ty *= m;
            return this;
        }

        public mm_matrix multiply(mm_matrix mtx, bool bappend)
        {
            // this*mtx 
            if (bappend)
            {
                float t0 = sx * mtx.sx + shy * mtx.shx;
                float t2 = shx * mtx.sx + sy * mtx.shx;
                float t4 = tx * mtx.sx + ty * mtx.shx + mtx.tx;
                shy = sx * mtx.shy + shy * mtx.sy;
                sy = shx * mtx.shy + sy * mtx.sy;
                ty = tx * mtx.shy + ty * mtx.sy + mtx.ty;
                sx = t0;
                shx = t2;
                tx = t4;
            }
            else
            //mtx*this
            {
                float t0 = sx * mtx.sx + shx * mtx.shy;
                float t2 = sx * mtx.shx + shx * mtx.sy;
                float t4 = sx * mtx.tx + shx * mtx.ty + tx;
                shy = shy * mtx.sx + sy * mtx.shy;
                sy = shy * mtx.shx + sy * mtx.sy;
                ty = shy * mtx.tx + sy * mtx.ty + ty;
                sx = t0;
                shx = t2;
                tx = t4;

            }
            return this;
        }

        public mm_matrix flip_x()
        {
            sx = -sx;
            shy = -shy;
            tx = -tx;
            return this;
        }

        public mm_matrix flip_y()
        {
            shx = -shx;
            sy = -sy;
            ty = -ty;
            return this;
        }

        public double rotation()
        {
            float x1 = 0f;
            float y1 = 0f;
            float x2 = 1.0f;
            float y2 = 0f;
            transform(ref x1, ref y1);
            transform(ref x2, ref y2);
            return Math.Atan2(y2 - y1, x2 - x1);
        }

        public void translation(ref float dx, ref float dy)
        {
            dx = tx;
            dy = ty;
        }

        public void scaling(float x, float y)
        {
            //    float x1 = 0f;
            //float y1 = 0f;
            //float x2 = 1.0f;
            //float y2 = 1.0f;
            //mm_matrix t = new mm_matrix(this);
            //t *= trans_affine_rotation(-rotation());
            //t.transform(&x1, &y1);
            //t.transform(&x2, &y2);
            //*x = x2 - x1;
            //*y = y2 - y1;
        }








    }
}

