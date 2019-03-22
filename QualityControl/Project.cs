using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using GIS.Converters.WellKnownText;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;

namespace QualityControl
{
    public class Project
    {
        /// <summary>
        /// 地球半径
        /// </summary>
        const double EarthRadio = 6371.004;


        public void LonlatToXY(string geometry_wkt, int pcsType)
        {
            ISpatialReferenceFactory2 pSpaRefFactory = new SpatialReferenceEnvironmentClass();
            //创建地理坐标系对象
            IProjectedCoordinateSystem pNewGeoSys = pSpaRefFactory.CreateProjectedCoordinateSystem(pcsType);//4214代表Beijing1954

            IPoint point = GeometryFromWKT.Parse(geometry_wkt) as IPoint;
            point.Project(pNewGeoSys);
            ISpatialReference ss = point.SpatialReference;
            Console.WriteLine(ss);
        }

        //正算
        public static double[] gaussBLtoXY(double longitude, double latitude)
        {
            int ProjNo = 0;

            // 带宽  
            int ZoneWide = 6;

            double longitude1, latitude1, longitude0, X0, Y0, xval, yval;
            double a, f, e2, ee, NN, T, C, A, M, iPI;

            // 3.1415926535898/180.0;  
            iPI = 0.0174532925199433;

            // 54年北京坐标系参数  
            a = 6378137;
            f = 1 / 298.257223563;

            // 80年西安坐标系参数  
            // a=6378140.0;  
            // f=1/298.257;  

            ProjNo = (int)(longitude / ZoneWide);
            longitude0 = ProjNo * ZoneWide + ZoneWide / 2;
            longitude0 = longitude0 * iPI;

            // 经度转换为弧度  
            longitude1 = longitude * iPI;

            // 纬度转换为弧度  
            latitude1 = latitude * iPI;

            e2 = 2 * f - f * f;
            ee = e2 * (1.0 - e2);
            NN = a / Math.Sqrt(1.0 - e2 * Math.Sin(latitude1) * Math.Sin(latitude1));
            T = Math.Tan(latitude1) * Math.Tan(latitude1);
            C = ee * Math.Cos(latitude1) * Math.Cos(latitude1);
            A = (longitude1 - longitude0) * Math.Cos(latitude1);
            M = a * ((1 - e2 / 4 - 3 * e2 * e2 / 64 - 5 * e2 * e2 * e2 / 256) * latitude1 - (3 * e2 / 8 + 3 * e2 * e2 / 32 + 45 * e2 * e2 * e2 / 1024) * Math.Sin(2 * latitude1) + (15 * e2 * e2 / 256 + 45 * e2 * e2 * e2 / 1024) * Math.Sin(4 * latitude1) - (35 * e2 * e2 * e2 / 3072) * Math.Sin(6 * latitude1));
            xval = NN * (A + (1 - T + C) * A * A * A / 6 + (5 - 18 * T + T * T + 72 * C - 58 * ee) * A * A * A * A * A / 120);
            yval = M + NN * Math.Tan(latitude1) * (A * A / 2 + (5 - T + 9 * C + 4 * C * C) * A * A * A * A / 24 + (61 - 58 * T + T * T + 600 * C - 330 * ee) * A * A * A * A * A * A / 720);
            X0 = 1000000 * (ProjNo + 1) + 500000;
            Y0 = 0;
            xval = xval + X0;
            yval = yval + Y0;
            return new double[] { xval, yval };
        }


        //反算
        public static double[] gaussXYtoBL(double X, double Y)
        {
            int ProjNo;
            int ZoneWide; // //带宽  
            double[] output = new double[2];
            double longitude1, latitude1, longitude0, X0, Y0, xval, yval;// latitude0,  
            double e1, e2, f, a, ee, NN, T, C, M, D, R, u, fai, iPI;
            iPI = 0.0174532925199433; // //3.1415926535898/180.0;  
            a = 6378137;
            f = 1 / 298.257223563;
            //a = 6378140.0;  
            //f = 1 / 298.257; // 80年西安坐标系参数  


            ZoneWide = 6; // //6度带宽  
            ProjNo = (int)(X / 1000000); // 查找带号  
            longitude0 = (ProjNo - 1) * ZoneWide + ZoneWide / 2;
            longitude0 = longitude0 * iPI; // 中央经线  

            X0 = ProjNo * 1000000 + 500000;
            Y0 = 0;
            xval = X - X0;
            yval = Y - Y0; // 带内大地坐标  
            e2 = 2 * f - f * f;
            e1 = (1.0 - Math.Sqrt(1 - e2)) / (1.0 + Math.Sqrt(1 - e2));
            ee = e2 / (1 - e2);
            M = yval;
            u = M / (a * (1 - e2 / 4 - 3 * e2 * e2 / 64 - 5 * e2 * e2 * e2 / 256));
            fai = u + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * u) + (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * u) + (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * u) + (1097 * e1 * e1 * e1 * e1 / 512) * Math.Sin(8 * u);
            C = ee * Math.Cos(fai) * Math.Cos(fai);
            T = Math.Tan(fai) * Math.Tan(fai);
            NN = a / Math.Sqrt(1.0 - e2 * Math.Sin(fai) * Math.Sin(fai));
            R = a * (1 - e2) / Math.Sqrt((1 - e2 * Math.Sin(fai) * Math.Sin(fai)) * (1 - e2 * Math.Sin(fai) * Math.Sin(fai)) * (1 - e2 * Math.Sin(fai) * Math.Sin(fai)));
            D = xval / NN;
            // 计算经度(Longitude) 纬度(Latitude)  
            longitude1 = longitude0 + (D - (1 + 2 * T + C) * D * D * D / 6 + (5 - 2 * C + 28 * T - 3 * C * C + 8 * ee + 24 * T * T) * D * D * D * D * D / 120) / Math.Cos(fai);
            latitude1 = fai - (NN * Math.Tan(fai) / R) * (D * D / 2 - (5 + 3 * T + 10 * C - 4 * C * C - 9 * ee) * D * D * D * D / 24 + (61 + 90 * T + 298 * C + 45 * T * T - 256 * ee - 3 * C * C) * D * D * D * D * D * D / 720);
            // 转换为度 DD  
            output[0] = longitude1 / iPI;
            output[1] = latitude1 / iPI;
            return output;
        }

        
        /// <summary>
        /// 计算平面位置AB两点的距离
        /// </summary>
        /// <param name="lonA"></param>
        /// <param name="latA"></param>
        /// <param name="lonB"></param>
        /// <param name="latB"></param>
        /// <returns>单位米</returns>
        public double computeDistanceByxy(double startX, double startY, double endX, double endY)
        {
            double lonA = startX * Math.PI / 180;
            double latA = startY * Math.PI / 180;
            double lonB = endX * Math.PI / 180;
            double latB = endY * Math.PI / 180;
            double distanceAB;
            distanceAB = EarthRadio * Math.Acos(Math.Cos(latA) * Math.Cos(latB) * Math.Cos(lonB - lonA) + Math.Sin(latA) * Math.Sin(latB));
            return distanceAB * 1000;

        }

        /// <summary>
        /// 计算球面位置AB两点的距离
        /// </summary>
        /// <param name="lonA"></param>
        /// <param name="latA"></param>
        /// <param name="lonB"></param>
        /// <param name="latB"></param>
        /// <returns>单位米</returns>
        public double computeDistanceBylatlon(double latA, double lonA, double latB, double lonB)
        {
            double distanceAB;
            distanceAB = EarthRadio * Math.Acos(Math.Cos(latA) * Math.Cos(latB) * Math.Cos(lonB - lonA) + Math.Sin(latA) * Math.Sin(latB));
            return distanceAB * 1000;
        }


    }
}
