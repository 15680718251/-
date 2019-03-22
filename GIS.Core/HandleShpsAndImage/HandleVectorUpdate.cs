using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading; 
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OSGeo.OGR;
using OSGeo.OSR;
using GIS.Utilities;

namespace GIS.HandleShpsAndImage
{
    public class HandleVectorUpdate
    {
        string srcFilename;
        string incrFilename;
        string outFilename;
        double filterArea;
        OSGeo.OGR.Driver poDriver;
 
        public HandleVectorUpdate(string str1,string str2,string str3,double area)
        {
            srcFilename = str1;
            incrFilename = str2;
            outFilename = str3;  //复制到了这里之后就直接在这上面操作
            filterArea = area;

            string pszDriverName = "ESRI Shapefile";
            OSGeo.OGR.Ogr.RegisterAll();
            poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);
            if (poDriver == null)
            {
                MessageBox.Show("Driver Error", "提示");
                return;
            }

            //把原始图层的数据拷贝成新生成的图层 shp dbf shx prj
            System.IO.File.Delete(str3);
            System.IO.File.Copy(str1,str3);  //shp

            System.IO.File.Delete(str3.Substring(0, str3.Length - 3) + "dbf");
            System.IO.File.Copy(str1.Substring(0,str1.Length-3)+"dbf",str3.Substring(0,str3.Length-3)+"dbf");

            System.IO.File.Delete(str3.Substring(0, str3.Length - 3) + "shx");
            System.IO.File.Copy(str1.Substring(0, str1.Length - 3) + "shx", str3.Substring(0, str3.Length - 3) + "shx");

            if (System.IO.File.Exists(str1.Substring(0, str1.Length - 3) + "prj"))
            {
                System.IO.File.Delete(str3.Substring(0, str3.Length - 3) + "prj");
                System.IO.File.Copy(str1.Substring(0, str1.Length - 3) + "prj", str3.Substring(0, str3.Length - 3) + "prj");
            }

        }

        ~HandleVectorUpdate()
        {
            poDriver.Dispose();
        }

        public void DoWork()
        {
            SubtractIncrementAndAddIncrement(outFilename, incrFilename);
        }

        private bool EnvelopeContains(OSGeo.OGR.Envelope env1, OSGeo.OGR.Geometry g)
        {
            OSGeo.OGR.Envelope env2 = new Envelope();
            g.GetEnvelope(env2);

            if (env1.MinX < env2.MinX && env1.MinY < env2.MinY && env1.MaxX > env2.MaxX && env1.MaxY > env2.MaxY)
            {
                return true;
            }
            else
                return false;
        }

        //不需要新建什么图
        private void SubtractIncrement(string srcFileName, string incFileName)
        {
            OSGeo.OGR.DataSource ds1 = poDriver.Open(srcFileName, 1); //底图 其实是拷贝的图层
            OSGeo.OGR.DataSource ds2 = poDriver.Open(incFileName, 1); //增量

            string strName = System.IO.Path.GetFileNameWithoutExtension(srcFileName);
            if (ds2 == null || ds1 == null)
            {
                MessageBox.Show("DataSource Error", "提示");
                return;
            }
            OSGeo.OGR.Layer srcLayer = ds1.GetLayerByIndex(0);
            OSGeo.OGR.Layer incLayer = ds2.GetLayerByIndex(0);
            

            int incFeatureCount = (int)incLayer.GetFeatureCount(0);
            if (incFeatureCount == 0)
            {
                return;
            }

            //log.WriteLog(System.DateTime.Now.ToString());

            srcLayer.ResetReading();
            int srcFeatureCount = (int)srcLayer.GetFeatureCount(0);
            for (int i = 0; i < srcFeatureCount; i++)
            {
                OSGeo.OGR.Feature psrcFeature = srcLayer.GetFeature(i);
                OSGeo.OGR.Geometry psrcGeometry = psrcFeature.GetGeometryRef();

                incLayer.ResetReading();
                incLayer.SetSpatialFilter(psrcGeometry);
                OSGeo.OGR.Feature pincFeature;
                bool deleteFlg = false;
                while (null != (pincFeature = incLayer.GetNextFeature()))
                {
                    OSGeo.OGR.Geometry pincGeometry = pincFeature.GetGeometryRef();

                    bool flag = psrcGeometry.Intersect(pincGeometry);
                    if (!flag)
                    {
                        continue;
                    }

                    psrcGeometry = psrcGeometry.Difference(pincGeometry);
                    deleteFlg = true;
                }

                if (deleteFlg)
                {
                    string value = psrcFeature.GetFieldAsString("PlgAttr");
                    srcLayer.DeleteFeature(psrcFeature.GetFID());

                    if (psrcGeometry.GetArea() > 0)
                    {
                        OSGeo.OGR.Feature ptmpFeature = new OSGeo.OGR.Feature(srcLayer.GetLayerDefn());
                        ptmpFeature.SetField("PlgAttr", value);
                        ptmpFeature.SetGeometry(psrcGeometry);
                        srcLayer.CreateFeature(ptmpFeature);
                    }
                }
            }

            //srcLayer.SyncToDisk();
            ds1.ExecuteSQL("REPACK " + strName, null, null);

            //log.WriteLog(System.DateTime.Now.ToString());

            incLayer.Dispose();
            srcLayer.Dispose();
            ds2.Dispose();
            ds1.Dispose();
        }

        private void AddIncrement(string srcFileName, string incFileName)
        {
            OSGeo.OGR.DataSource ds1 = poDriver.Open(srcFileName, 1); //底图
            OSGeo.OGR.DataSource ds2 = poDriver.Open(incFileName, 0); //增量

            //string strName = System.IO.Path.GetFileNameWithoutExtension(srcFileName);

            if (ds2 == null || ds1 == null)
            {
                MessageBox.Show("DataSource Error", "提示");
                return;
            }

            OSGeo.OGR.Layer srcLayer = ds1.GetLayerByIndex(0);
            OSGeo.OGR.Layer incLayer = ds2.GetLayerByIndex(0);

            //srcLayer.ResetReading();
            int incFeatureCount = (int)incLayer.GetFeatureCount(1);
            //int srcFeatureCount = srcLayer.GetFeatureCount(1);
            for (int i = 0; i < incFeatureCount; i++)
            {
                OSGeo.OGR.Feature feat = incLayer.GetFeature(i);
                srcLayer.CreateFeature(feat);
            }

            incLayer.Dispose();
            srcLayer.Dispose();
            ds2.Dispose();
            ds1.Dispose();
        }

        private void SubtractIncrementAndAddIncrement(string srcFileName, string incFileName)
        {
            SubtractIncrement(srcFileName, incFileName);
            AddIncrement(srcFileName, incFileName);
        }

    }
}
