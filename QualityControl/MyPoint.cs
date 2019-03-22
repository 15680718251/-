using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using GIS.UI.AdditionalTool;

namespace QualityControl
{
    public class MyPoint : IDisposable
    {
        public MyPoint()
        {
        }

       public MyPoint(OracleDBHelper conHelper)
       {
           this.ConHelper = conHelper;
       }
       private OracleDBHelper conHelper = new OracleDBHelper();
       public OracleDBHelper ConHelper
       {
           get { return conHelper; }
           set { conHelper = value; }
       }

        private double lon;
        public double Lon
        {
            get { return lon; }
            set { lon = value; }
        }


        private double lat;
        public double Lat
        {
            get { return lat; }
            set { lat = value; }
        }
        
       /// <summary>
       /// 根据经纬度计算两点之间的距离
       /// </summary>
       /// <param name="point1"></param>
       /// <param name="point2"></param>
       /// <returns></returns>
        public  double calculateDistance(MyPoint point1,MyPoint point2)
        {
            //POINT (-73.7747863 40.7860006)
            //sdo_geometry('{0}',4326)
            //OracleDBHelper conHelper = new OracleDBHelper();
            double distance = 0 ;
            string SQL = string.Format("select SDO_GEOM.SDO_DISTANCE (sdo_geometry('POINT ({0} {1})',4326),sdo_geometry('POINT ({2} {3})',4326),0.05) from  daul", point1.Lon, point1.Lat, point2.Lon.ToString().Replace("(",""), point2.Lat);
            using (OracleDataReader dr = conHelper.queryReader(SQL))
            {
                if(dr.Read())
                {
                    distance = Convert.ToDouble(dr[0]);
                }
                dr.Close();
            }
            GC.Collect();
            return distance;
        }
        /// <summary>
        /// 获得两个点的中点
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        public MyPoint getMidpointLonLat(MyPoint point1, MyPoint point2)
        {
            MyPoint newPoint = new MyPoint();
            newPoint.lon = (point1.lon + point2.lon) / 2;
            newPoint.lat = (point1.lat + point2.lat) / 2;
            return newPoint;
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
