using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using GIS.Utilities;

using System.Runtime.Serialization;
namespace GIS.Geometries
{
    [Serializable]
    public class GeoLabel :Geometry
    {
        public GeoLabel()
        {
        }
        public GeoLabel(ITransForm transform)
        {
            m_Transform = transform;
        }
        public GeoLabel(string text, GeoPoint startPt,GeoPoint endPt, double size,Color clr, string font,ITransForm transform )
        {
            m_TextSize = size;
            m_Text = text;
            m_StartPt = (GeoPoint)startPt.Clone();
            m_EndPt = (GeoPoint)endPt.Clone();
            m_Color = clr;
            m_FontName = font;
            m_Transform = transform;
        }

        #region PrivateMember

        private string m_Text = "陆琪我爱你";
        private GeoPoint m_StartPt;     
        private GeoPoint m_EndPt;    
        private double m_TextSize = 16;// 字符大小	
        private string m_FontName = "楷体_GB2312";//字体名称       
        private Color m_Color = Color.Red;    //字体颜色     
        private double m_Angle = 0;

        public double Angle
        {
            get { return m_Angle; }
            set { m_Angle = value; }
        }
        private GIS.Utilities.ITransForm m_Transform; // 用来做点选判断

        #endregion
 
        #region Properties

        public string FontName
        {
            get { return m_FontName; }
            set { m_FontName = value; }
        }
        public double TextSize
        {
            get { return m_TextSize; }
            set { m_TextSize = value; }
        }
        public Color Color
        {
            get { return m_Color; }
            set { m_Color = value; }
        }
        public GIS.Utilities.ITransForm Transform
        {
            get { return m_Transform; }
            set { m_Transform = value; }
        }

        public GeoPoint StartPt
        {
            get { return m_StartPt; }
            set { m_StartPt = value; }
        }
        public GeoPoint EndPt
        {
            get { return m_EndPt; }
            set { m_EndPt = value; }
        }

    
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }
      
        #endregion

        #region OverrideFunction
        public override bool IsEmpty()
        {
            return false;
        }
        public override bool IsSimple()
        {
            throw new NotImplementedException();
        }
       
        public override Geometry Clone()
        {
            return new GeoLabel(m_Text, StartPt,EndPt,m_TextSize,m_Color,m_FontName, Transform);
        }
        public override bool IsSelectByPt(GeoPoint pt)
        {
            Point sltPt = Transform.TransFromWorldToMap(pt);

            float fontsize = Transform.TransFromWorldToMap(this.TextSize);
            Font font = new Font(FontName, fontsize, GraphicsUnit.Pixel); //设置字体
            int fontheight = (int)font.GetHeight(); //字体的高度
            float width = Transform.TransFromWorldToMap(Length);
            Point startPt = Transform.TransFromWorldToMap(StartPt); //旋转中心，找到字符的原点 左下角
            startPt.Y -= fontheight;

            RectangleF rect = new RectangleF(startPt ,new SizeF( width, fontheight));
            Matrix mtx = new Matrix();
            mtx.RotateAt(RotateAngle,startPt);

            Region rgn = new Region(rect);          
            rgn.Transform(mtx);

            bool result = rgn.IsVisible(sltPt);

            rgn.Dispose();
            mtx.Dispose();
            return result;

        }
        public override GeoPoint MouseCatchPt(GeoPoint pt, MouseCatchType type )
        {
            if (type == MouseCatchType.Both || type == MouseCatchType.Vertex)
            {
                if (SpatialRelation.GeoAlgorithm.IsOnPointCoarse(pt, m_StartPt))
                {
                    return m_StartPt;
                }
                else if (SpatialRelation.GeoAlgorithm.IsOnPointCoarse(pt, m_EndPt))
                {
                    return m_EndPt;
                }
            }
            else
            {
                double x = (m_StartPt.X + m_EndPt.X) / 2;
                double y = (m_StartPt.Y + m_EndPt.Y) / 2;
                GeoPoint centPt = new GeoPoint(x, y);
                if (SpatialRelation.GeoAlgorithm.IsOnPointCoarse(centPt, pt))
                    return centPt;
            }
            return null;
        }
        public override GeoBound GetBoundingBox()
        {
            if (m_EndPt == null)
                return new GeoBound(StartPt, StartPt);
            return new GeoBound(StartPt, EndPt) ;
        }
        public override void WriteGeoInfo(System.IO.StreamWriter sw)
        {
            throw new NotImplementedException();
        }
        public override void ReadFromLQFile(System.IO.StreamReader sr)
        {
            string strTemp = sr.ReadLine();
            Text = strTemp.Trim();
            strTemp = sr.ReadLine();
            string[] strArray = strTemp.Split(',');
            m_TextSize = double.Parse(strArray[0].Trim());
            m_FontName = strArray[1].Trim();
            m_Color = Color.FromArgb(int.Parse(strArray[2].Trim()),
                                     int.Parse(strArray[3].Trim()),
                                     int.Parse(strArray[4].Trim()));
            strTemp = sr.ReadLine();
            strArray = strTemp.Split(',');
            m_StartPt = new GeoPoint(double.Parse(strArray[0].Trim()), double.Parse(strArray[1].Trim()));
            strTemp = sr.ReadLine();
            strArray = strTemp.Split(',');
            m_EndPt = new GeoPoint(double.Parse(strArray[0].Trim()), double.Parse(strArray[1].Trim()));
        }
        public override void WriteToLQFile(System.IO.StreamWriter sw)
        {
            sw.WriteLine("GeoLabel");
            sw.WriteLine(Text);
            sw.WriteLine("{0},{1},{2},{3},{4}",m_TextSize,m_FontName,m_Color.R,m_Color.G,m_Color.B);
            sw.WriteLine("{0:f5},{1:f5}", m_StartPt.X, m_StartPt.Y);
            sw.WriteLine("{0:f5},{1:f5}", m_EndPt.X, m_EndPt.Y);  
        }
        #endregion

        public override void Move(double deltaX, double deltaY)
        {
            m_StartPt.Move(deltaX,deltaY);
            if(m_EndPt!=null)
                m_EndPt.Move(deltaX, deltaY);
        }
        public override void RotateAt(double angle, GeoPoint basePt)
        {
            m_StartPt.RotateAt(angle, basePt);
            if(m_EndPt != null)
              m_EndPt.RotateAt(angle, basePt);
        }
        public override void SymmetryWithLine(GeoPoint ptStart, GeoPoint ptEnd)
        {
            m_StartPt.SymmetryWithLine(ptStart, ptEnd);
            m_EndPt.SymmetryWithLine(ptStart, ptEnd);
        }
        public float RotateAngle 
        { 
            get
            {
                 float angTemp = (float )GIS.SpatialRelation.GeoAlgorithm.CalcAzimuth(m_StartPt.X, m_StartPt.Y, m_EndPt.X, m_EndPt.Y);
                 return 360 -  angTemp * 180 / 3.14159f;
            }
        }
        public new double Length
        {
            get
            {
                double len = GIS.SpatialRelation.GeoAlgorithm.DistanceOfTwoPt(StartPt,EndPt);
                return Math.Sqrt(len);
            }
        }
        public override bool IsEqual(Geometry geom)
        {
            GeoLabel label = geom as GeoLabel;
            if (label == null)
                return false;
            if (label.m_EndPt.IsEqual(EndPt) && label.m_StartPt.IsEqual(StartPt)
                && label.Angle == Angle && label.TextSize == label.TextSize
                && label.Text == Text)
                return true;
            else return false;
        }
    }
}
