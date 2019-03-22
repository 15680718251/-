using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.Geometries
{
    public class GeoPoint3D:GeoPoint
    {
         public GeoPoint3D():this(0,0,0)
        {
        }
         public GeoPoint3D(double x, double y)
         {
             X = x;
             Y = y;
             m_Z = 0;
         }
         public GeoPoint3D(GeoPoint pt)
         {
             X = pt.X;
             Y = pt.Y;
             m_Z = 0;
         }  
        public GeoPoint3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            m_Z = z;
        }    

        private double m_Z=0;

        
        public double Z
        {
            get { return m_Z; }
            set { m_Z = value; }
        }
        public void SetXYZ(double x, double y, double z)
        {
            X = x;
            Y = y;
            m_Z = z;
        }

        public override bool IsEqual(Geometry geom)
        {
            GeoPoint3D pt = geom as GeoPoint3D;
            return pt != null && Math.Abs(pt.X - X) < Geometry.EPSIONAL && Math.Abs(pt.Y - Y) < Geometry.EPSIONAL && Math.Abs(pt.Z - Z) < Geometry.EPSIONAL;
        }
        public override Geometry Clone()
        {
            return new GeoPoint3D(X, Y,Z);
        }

        public override void WriteGeoInfo(System.IO.StreamWriter sw)
        {
            sw.WriteLine("GeoPoint3D");
            sw.WriteLine("{0:f5},{1:f5},{2:f5}", X, Y,m_Z);
        }
        public override void WriteToLQFile(System.IO.StreamWriter sw)
        {
            sw.WriteLine("GeoPoint3D");
            sw.WriteLine("{0:f5},{1:f5},{2:f5}", X, Y, m_Z); ;
        }
        public override void ReadFromLQFile(System.IO.StreamReader sr)
        {
            try
            {
                string temp = sr.ReadLine();
                string[] strArray = temp.Split(',');
                X = double.Parse(strArray[0].Trim());
                Y = double.Parse(strArray[1].Trim());
                m_Z = double.Parse(strArray[2].Trim());
            }
            catch
            {
                string temp = sr.ReadLine();
            }
        }

 
    }
}
