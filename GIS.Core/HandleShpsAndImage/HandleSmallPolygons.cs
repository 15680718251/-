using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

using OSGeo.OGR;
using OSGeo.OSR;
using OSGeo.GDAL;

namespace GIS.HandleShpsAndImage
{
    public class HandleSmallPolygons
    {
        public OSGeo.OSR.SpatialReference sr;
        public string tablename;
        public string path;
        public OSGeo.OGR.DataSource ds;
        public double area;

        public HandleSmallPolygons(string path,double area)
        {
            this.area = area;
            this.path = path;

            string pszDriverName = "ESRI Shapefile";
            OSGeo.OGR.Ogr.RegisterAll();

            OSGeo.OGR.Driver poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);
            if (poDriver == null)
            {
                MessageBox.Show("Driver Error","提示");
                return;
            }

            ds = poDriver.Open(path, 0);
            if (ds == null)
            {
                MessageBox.Show("DataSource Error", "提示");
                return;
            }
        }

        ~HandleSmallPolygons()
        {
            ds.Dispose();
        }

        public int UninTouchSmallPolygon()
        {
            int layerCount = ds.GetLayerCount(); //表示有多少个图层  一般的只有一个 (一个图层包括很多特征物)
            OSGeo.OGR.Layer layer = ds.GetLayerByIndex(0);
            tablename = layer.GetName();
            sr = layer.GetSpatialRef();

            List<OSGeo.OGR.Feature> bigFeatures = new List<OSGeo.OGR.Feature>();
            List<OSGeo.OGR.Geometry> bigGeometrys = new List<OSGeo.OGR.Geometry>();

            List<OSGeo.OGR.Feature> smallFeatures = new List<OSGeo.OGR.Feature>();
            List<OSGeo.OGR.Geometry> smallGeometrys = new List<OSGeo.OGR.Geometry>();

            OSGeo.OGR.Feature pFeature;
            OSGeo.OGR.Geometry pGeometry;

            layer.ResetReading();
            while ((pFeature = layer.GetNextFeature()) != null)
            {
                pGeometry = pFeature.GetGeometryRef();
                double garea = pGeometry.GetArea();
                if (garea > area)
                {
                    bigFeatures.Add(pFeature);
                    bigGeometrys.Add(pGeometry);
                }
                else
                {
                    smallFeatures.Add(pFeature);
                    smallGeometrys.Add(pGeometry);
                }
            }

            int bigSize = bigGeometrys.Count;//大面积的多边形数量
            int smallSize = smallGeometrys.Count;//小面积的多边形数量

            for (int i = 0; i < smallSize; i++)
            {
                OSGeo.OGR.Geometry pSmallGeometry = smallGeometrys[i];

                for (int j = 0; j < bigSize; j++)
                {
                    OSGeo.OGR.Geometry pBigGeometry = bigGeometrys[j];

                    bool isEnvelopesIntersects = IsEnvelopesIntersect(pSmallGeometry, pBigGeometry);
                    if (!isEnvelopesIntersects)
                    {
                        continue;
                    }

                    bool isTouch = pSmallGeometry.Touches(pBigGeometry);
                    if (!isTouch)
                    {
                        continue;
                    }

                    OSGeo.OGR.Geometry pTempGeometry = pSmallGeometry.Union(pBigGeometry);
                    bigGeometrys[j] = pTempGeometry; //替换原来的几何对象

                    //OSGeo.OGR.Feature pTempFeature=layer.CreateFeature();
                    OSGeo.OGR.Feature pTempFeature = new Feature(layer.GetLayerDefn());
                    int nValue = bigFeatures[j].GetFieldAsInteger("PlgAttr");
                    pTempFeature.SetField("PlgAttr", nValue);
                    pTempFeature.SetGeometry(pTempGeometry);

                    bigFeatures[j] = pTempFeature;

                    break;
                }
            }

            OutputLayer(bigFeatures);//输出图层

            for (int i = 0; i < smallFeatures.Count; ++i)
            {
                smallFeatures[i].Dispose();
            }

            //return smallSize;

            //ds.ExecuteSQL("REPACK AOI2009_src", null, "");//很重要的一句，真正更新
            ////ds.ExecuteSQL("CREATE SPATIAL INDEX ON AOI_2009_cls", null, "");

            //ds.Dispose();
            layer.Dispose();
            return 0;
        }

        private void OutputLayer(List<OSGeo.OGR.Feature> bigFeatures)
        {
            string strDriverName = "ESRI Shapefile";
            OSGeo.OGR.Ogr.RegisterAll();

            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName(strDriverName);
            if (dr == null)
            {
                MessageBox.Show("Driver Error", "提示");
                return;
            }

            string layername = tablename + "_" + System.DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd-HH-mm-ss");

            path = System.IO.Path.GetDirectoryName(path);
            OSGeo.OGR.DataSource ds2 = dr.CreateDataSource(path, null);
            if (ds2 == null)
            {
                MessageBox.Show("DataSource Error", "提示");
                return;
            }

            OSGeo.OGR.Layer layer = ds2.CreateLayer(layername, sr, wkbGeometryType.wkbMultiPolygon, null);
            OSGeo.OGR.FieldDefn fld = new FieldDefn("PlgAttr", FieldType.OFTInteger);
            layer.CreateField(fld, 1);

            int count = bigFeatures.Count;
            for (int i = 0; i < count; i++)
            {
                int nValue = bigFeatures[i].GetFieldAsInteger("PlgAttr");
                OSGeo.OGR.Feature pFeature = new Feature(layer.GetLayerDefn());
                pFeature.SetField("PlgAttr", nValue);
                pFeature.SetGeometry(bigFeatures[i].GetGeometryRef());

                layer.CreateFeature(pFeature);
            }

            layer.Dispose();
            ds2.Dispose();
        }

        private bool IsEnvelopesIntersect(OSGeo.OGR.Geometry pGeo1, OSGeo.OGR.Geometry pGeo2)
        {
            OSGeo.OGR.Envelope env1 = new OSGeo.OGR.Envelope();
            OSGeo.OGR.Envelope env2 = new OSGeo.OGR.Envelope();
            pGeo1.GetEnvelope(env1);
            pGeo2.GetEnvelope(env2);

            return env1.MinX <= env2.MaxX && env1.MaxX >= env2.MinX && env1.MinY <= env2.MaxY && env1.MaxY >= env2.MinY;
        }
    }
}
