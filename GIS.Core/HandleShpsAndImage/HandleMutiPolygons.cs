using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using OSGeo.OSR;
using System.Windows.Forms;

namespace GIS.HandleShpsAndImage
{
    //多多边形化简，主要两个功能
    public class HandleMutiPolygons : IDisposable
    {
        OSGeo.OGR.Driver poDriver;
        public OSGeo.OGR.DataSource ds;
        string tablename;

        public HandleMutiPolygons(string path)
        {
            string pszDriverName = "ESRI Shapefile";
            OSGeo.OGR.Ogr.RegisterAll();

            poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);
            if (poDriver == null)
            {
                MessageBox.Show("Driver Error", "提示");
                return;
            }
            ds = poDriver.Open(path, 1);
            if (ds == null)
            {
                MessageBox.Show("DataSource Error", "提示");
                return;
            }
            tablename = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        #region Disposers and finalizers
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if(disposing)
                    if (poDriver != null || ds != null)
                    {
                        try
                        {
                            ds.Dispose();
                            poDriver.Dispose();

                            //layers.ForEach(delegate(OSGeo.OGR.Layer layer)
                            //{
                            //    layer.Dispose();
                            //});

                            //feats.ForEach(delegate(OSGeo.OGR.Feature feat)
                            //{
                            //    feat.Dispose();
                            //});
                        }
                        finally
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                disposed = true;
            }
        }
        ~HandleMutiPolygons()
        {
            Dispose(true);
        }
        #endregion

        public int HandleWork()
        {
            int count=0;

            OSGeo.OGR.Layer layer=ds.GetLayerByIndex(0);
            int featuresCount=(int)layer.GetFeatureCount(0);

            layer.ResetReading();

            //首先判断多多变形
            for( int index=0;index<featuresCount;index++)
            {
                OSGeo.OGR.Feature feature=layer.GetFeature(index);
                //if(feature==null)
                //{
                //    continue;
                //}

                OSGeo.OGR.Geometry geometry=feature.GetGeometryRef();
                if(geometry==null)
                {
                    feature.Dispose();
                    continue;
                }

                OSGeo.OGR.wkbGeometryType geoType=geometry.GetGeometryType();
                //string str1=geometry.GetGeometryName();
                //int ii=geometry.GetGeometryCount();
                
                //多多边形
                if (geoType == OSGeo.OGR.wkbGeometryType.wkbMultiPolygon)
                {
                    int geoCount = geometry.GetGeometryCount();
                    for (int i = 0; i < geoCount; i++)
                    {
                        OSGeo.OGR.Geometry subGeometry = geometry.GetGeometryRef(i);
                        OSGeo.OGR.wkbGeometryType subGeoType = subGeometry.GetGeometryType();
                        if (subGeoType == wkbGeometryType.wkbPolygon)
                        {
                            OSGeo.OGR.Feature newFeature = new OSGeo.OGR.Feature(layer.GetLayerDefn());
                            newFeature.SetField("PlgAttr", feature.GetFieldAsString("PlgAttr"));
                            newFeature.SetGeometry(subGeometry);
                            layer.CreateFeature(newFeature);
                            newFeature.Dispose();
                        }
                    }
                    layer.DeleteFeature(feature.GetFID());
                }
                feature.Dispose();
                count++;
            }
            ExecuteSQL();
            return count;
        }

        private void ExecuteSQL()
        {
            string sql = "REPACK " + tablename;
            ds.ExecuteSQL(sql,null,null);
        }
    }
}
