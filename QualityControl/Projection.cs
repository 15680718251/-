using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Geometry;

namespace QualityControl
{ 
    public enum EnumStrip
    {
        Strip3=3,
        Strip6=6,
    }

    public class Projection
    {
            //this.a = 6378140;           //西安80椭球长半轴
            //this.b = 6356755.29;        //西安80椭球短半轴
            //this.f = 1 / 298.257;     //椭球扁率f
            //this.e = 0.081819221455523;       //第一偏心率
            //this.e2 = 6.69438499958795E-03;   //椭球第一偏心率平方 e2
            //this.e12 = 6.73950181947292E-03;  //椭球第二偏心率平方 e12
            //this.c = 6399596.65198801;         //极点子午圈曲率半径 c
            //this.p = 206264.806252992;      //弧度秒=180*3600/pi
            //this.pi = 3.1415926535;

            //this.a = 6378137;           //2000椭球长半轴
            //this.b = 6356752.3141;      //2000椭球短半轴
            //this.f = 1 / 298.257222101;     //椭球扁率f
            //this.e = 0.0818191910428;       //第一偏心率
            //this.e2 = 6.693421622966E-03;   //椭球第一偏心率平方 e2
            //this.e12 = 6.738525414683E-03;  //椭球第二偏心率平方 e12
            //this.c = 6399593.62586;         //极点子午圈曲率半径 c
            //this.p = 206264.806252992;      //弧度秒=180*3600/pi
            //this.pi = 3.1415926535;

        protected double a = 6378137;           //2000椭球长半轴
        protected double b = 6356752.3142451795;      //2000椭球短半轴
        protected double f = 1 / 298.257223563;     //椭球扁率f
        protected double e = 0.0818191908426215;       //第一偏心率
        protected double e2 = 0.00669437999013;   //椭球第一偏心率平方 e2
        protected double e12 = 0.0067394967422764;  //椭球第二偏心率平方 e12
        protected double c = 6399593.6257584931;         //极点子午圈曲率半径 c
        protected double p = 206264.806252992;      //弧度秒=180*3600/pi
        protected double pi = 3.1415926535898;
 
        //需要三个配置参数
        private bool m_IsBigNumber = true;
        private EnumStrip m_Strip = EnumStrip.Strip6;
        private double m_L0 = 112;
 
        public bool IsBigNumber
        {
            get
            {
                return m_IsBigNumber;
            }
            set
            {
                m_IsBigNumber = value;
            }
        }
        public EnumStrip Strip
        {
            get
            {
                return m_Strip;
            }
            set
            {
                m_Strip = value;
            }
        }
        public double L0
        {
            get
            {
                return m_L0;
            }
            set
            {
                m_L0 = value;
            }
        }
        protected double toNum(double num)
        {
            //return double.Parse(num.ToString());
            return (double)num;
        }
        protected double toDec(double num)
        {
            //return double.Parse(num.ToString());
            return (double)num;
        }
        protected double sin(double num)
        {
            return this.toDec(Math.Sin(this.toNum(num)));
        }
        protected double cos(double num)
        {
            return this.toDec(Math.Cos(this.toNum(num)));
        }
        protected double sqrt(double num)
        {
            return this.toDec(Math.Sqrt(this.toNum(num)));
        }
        protected double tan(double num)
        {
            return this.toDec(Math.Tan(this.toNum(num)));
        }
        public  double pow(double x, double y)
        {
            return toDec(Math.Pow(toNum(x), toNum(y)));
        }
 
        //高斯反算方法  OK   (x,y)=>(B,L)
        //latitude纬度B longitude经度L
        public double[] GetBLFromXY(double x, double y)
        {
            double latitude;
            double longitude;
            //去掉大数和东移500公里
            double y1 = y - 500000.0;
            if (this.IsBigNumber == true)
            {
                y1 = y1 - (this.L0 / (int)this.Strip) * 1000000;
            }
            y = y1;
            double l0 = this.L0 * 3600;  //中央子午线转为秒值 如=105*3600
            //计算临时值
            //double e4 = e2 * e2;
            //double e6 = e4 * e2;
            //double e8 = e6 * e2;
            //double e10 = e8 * e2;
            //double e_12 = e10 * e2;
            double e4 = pow(e2, 2); // e2 * e2;
            double e6 = pow(e2, 3); //e4 * e2;
            double e8 = pow(e2, 4); //e6 * e2;
            double e10 = pow(e2, 5); //e8 * e2;
            double e_12 = pow(e2, 6); //e10 * e2;
            //
            double A1 = 1 + 3 * e2 / 4 + 45 * e4 / 64 + 175 * e6 / 256 + 11025 * e8 / 16384 + 43659 * e10 / 65536 + 693693 * e_12 / 1048576;
            double B1 = 3 * e2 / 8 + 15 * e4 / 32 + 525 * e6 / 1024 + 2205 * e8 / 4096 + 72765 * e10 / 131072 + 297297 * e_12 / 524288;
            double C1 = 15 * e4 / 256 + 105 * e6 / 1024 + 2205 * e8 / 16384 + 10395 * e10 / 65536 + 1486485 * e_12 / 8388608;
            double D1 = 35 * e6 / 3072 + 105 * e8 / 4096 + 10395 * e10 / 262144 + 55055 * e_12 / 1048576;
            double E1 = 315 * e8 / 131072 + 3465 * e10 / 524288 + 99099 * e_12 / 8388608;
            double F1 = 693 * e10 / 1310720 + 9009 * e_12 / 5242880;
            double G1 = 1001 * e_12 / 8388608;
            //求底点纬度值Bf
            double B0 = x / (a * (1 - e2) * A1);
            double Bf = 0.0;
            double FB = 0.0;
            double FB1 = 0.0;
            double a0 = a * (1 - e2);
            double delta = Math.Abs(Bf - B0);
            while (delta > 4.8E-11)   //0.000000000048
            {
                Bf = B0;
                FB = a0 * (A1 * Bf - B1 * sin(2 * Bf) + C1 * sin(4 * Bf) - D1 * sin(6 * Bf) + E1 * sin(8 * Bf) - F1 * sin(10 * Bf) + G1 * sin(12 * Bf));
                FB1 = a0 * (A1 - 2 * B1 * cos(2 * Bf) + 4 * C1 * cos(4 * Bf) - 6 * D1 * cos(6 * Bf) + 8 * E1 * cos(8 * Bf) - 10 * F1 * cos(10 * Bf) + 12 * G1 * cos(12 * Bf));
                B0 = Bf + (x - FB) / FB1;
                //
                delta = Math.Abs(Bf - B0);
            }
            //
            double sinBf = sin(Bf);
            double sinBf2 = sinBf * sinBf;
            double W = sqrt(1 - e2 * sinBf2);
            double W3 = W * W * W;
            double N = a / W;
            double t = tan(Bf);
            double t2 = t * t;
            double t4 = t2 * t2;
            double cosBf = cos(Bf);
            double cosBf2 = cosBf * cosBf;
            double yy = e12 * cosBf2;   //η2
            double Mf = a0 / W3;
            //
            double y_N = y / N;
            double y_N2 = y_N * y_N;
            double y_N4 = y_N2 * y_N2;
            //
            //计算经伟度值B,L
            double t_B = Bf*p  - (p * t / (2 * Mf) * y * y_N) * (1 - (5 + 3 * t2 + yy - 9 * yy * t2) * y_N2 + (61 + 90 * t2 + 45 * t4) * y_N4 / 360);
            double t_L = (p / cosBf) * y_N * (1 - (1 + 2 * t2 + yy) * y_N2 / 6 + (5 + 28 * t2 + 24 * t4 + 6 * yy + 8 * yy * t2) * y_N4 / 120);
            //
            longitude = t_L + l0;
            //
            latitude = t_B / 3600;   //转为度
            longitude = longitude / 3600;   //转为度
            double[] lonlat = { longitude, latitude };
            return lonlat;
            //--the--end--
        }
 
        //高斯正算方法 (B,L)=>(x,y)
        ////latitude纬度B longitude经度L
        public  double[] GetXYFromBL(double latitude, double longitude)
        {
            double x;
            double y;
            //计算临时值
            double e4 = pow(e2, 2); // e2 * e2;
            double e6 = pow(e2, 3); //e4 * e2;
            double e8 = pow(e2, 4); //e6 * e2;
            double e10 = pow(e2, 5); //e8 * e2;
            double e_12 = pow(e2, 6); //e10 * e2;
            //
            double A1 = 1 + 3 * e2 / 4 + 45 * e4 / 64 + 175 * e6 / 256 + 11025 * e8 / 16384 + 43659 * e10 / 65536 + 693693 * e_12 / 1048576;
            double B1 = 3 * e2 / 8 + 15 * e4 / 32 + 525 * e6 / 1024 + 2205 * e8 / 4096 + 72765 * e10 / 131072 + 297297 * e_12 / 524288;
            double C1 = 15 * e4 / 256 + 105 * e6 / 1024 + 2205 * e8 / 16384 + 10395 * e10 / 65536 + 1486485 * e_12 / 8388608;
            double D1 = 35 * e6 / 3072 + 105 * e8 / 4096 + 10395 * e10 / 262144 + 55055 * e_12 / 1048576;
            double E1 = 315 * e8 / 131072 + 3465 * e10 / 524288 + 99099 * e_12 / 8388608;
            double F1 = 693 * e10 / 1310720 + 9009 * e_12 / 5242880;
            double G1 = 1001 * e_12 / 8388608;
            //
            double l0 = this.L0 * 3600;  //中央子午线 度转为秒值 如=105*3600
            double LL = longitude * 3600;                   //转为秒值
            //
            double t_B = latitude * this.pi / 180;     //转为弧度值  b
            double t_L = (LL - l0) / p;          //转为秒值    l
            double L2 = pow(t_L, 2);// t_L * t_L;
            double L4 = pow(t_L, 4);// L2 * L2;
            //
            double sinB = sin(t_B);
            double sinB2 = sinB * sinB;
            double W = sqrt(1 - e2 * sinB2);
            //double W3 = pow(W, 3);// W * W * W;
            double N = a / W;
            double t = tan(t_B);
            double t2 = t * t;
            double t4 = t2 * t2;
            double cosB = cos(t_B);
            double cosB2 = cosB * cosB;
            double cosB4 = cosB2 * cosB2;
            double y2 = e12 * cosB2;   //η2
            double y4 = y2 * y2;
            //
            double l_p = t_L;  //t_L/p;  //上面t_L已经除了p值,这里就不再除p值
            double l2_p2 = L2;   //l2/(p*p);
            double l4_p4 = L4;   //l4/(p*p*p*p);
            //
            double a0 = a * (1 - e2);
            //计算子午弧长公式xx
            double xx = a0 * (A1 * t_B - B1 * sin(2 * t_B) + C1 * sin(4 * t_B) - D1 * sin(6 * t_B) + E1 * sin(8 * t_B) - F1 * sin(10 * t_B) + G1 * sin(12 * t_B));
            //计度平面坐标值x,y
            x = xx + N * t * cosB2 * l2_p2 * (0.5 + (5 - t2 + 9 * y2 + 4 * y4) * cosB2 * l2_p2 / 24 + (61 - 58 * t2 + t4) * cosB4 * l4_p4 / 720);
            y = N * cosB * l_p * (1 + (1 - t2 + y2) * cosB2 * l2_p2 / 6 + (5 - 18 * t2 + t4 + 14 * y2 - 58 * y2 * t2) * cosB4 * l4_p4 / 120);
            //
            if (IsBigNumber == true)  //转为高斯投影是大数投影吗？即Zone 35带数  （小数投影为CM_105E)
            {
                y = y + (this.L0 / (int)this.Strip) * 1000000;
            }
            y = y + 500000.0;
            double[] xy = {x,y };
            return xy;
            //--the--end--
        }





    }
}
