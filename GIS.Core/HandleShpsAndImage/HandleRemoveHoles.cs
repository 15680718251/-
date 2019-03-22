using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using OSGeo.OSR;
using System.Windows.Forms;

namespace GIS.HandleShpsAndImage
{
    public class HandleRemoveHoles : IDisposable
    {
        OSGeo.OGR.Driver poDriver;
        public OSGeo.OGR.DataSource ds;
        OSGeo.OGR.Layer layer;
        public OSGeo.OSR.SpatialReference sr;

        string foleder;
        string tablename;
        string filename;
        double area;

        List<OSGeo.OGR.Feature> smallFeats = new List<OSGeo.OGR.Feature>();
        List<OSGeo.OGR.Geometry> smallGeos = new List<OSGeo.OGR.Geometry>();
        List<OSGeo.OGR.Geometry> bigGeos = new List<OSGeo.OGR.Geometry>();
        List<OSGeo.OGR.Feature> bigFeats = new List<OSGeo.OGR.Feature>();

        public HandleRemoveHoles(string path,double filterArea)
        {
            string pszDriverName = "ESRI Shapefile";
            OSGeo.OGR.Ogr.RegisterAll();

            poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);
            if (poDriver == null)
            {
                MessageBox.Show("Driver Error", "提示");
                return;
            }
            this.area = filterArea;
            foleder = System.IO.Path.GetDirectoryName(path);
            ds = poDriver.Open(foleder, 1);
            if (ds == null)
            {
                MessageBox.Show("DataSource Error", "提示");
                return;
            }

            tablename = System.IO.Path.GetFileNameWithoutExtension(path);
            layer = ds.GetLayerByName(tablename);
        }

        public void Work()
        {
            OSGeo.OGR.Feature pFeature;
            OSGeo.OGR.Geometry pGeometry;
            layer.ResetReading();
            while ((pFeature = layer.GetNextFeature()) != null)
            {
                pGeometry = pFeature.GetGeometryRef();
                double darea = pGeometry.GetArea();
                if (darea < area)
                {
                    smallFeats.Add(pFeature);
                    smallGeos.Add(pGeometry);
                }
            }

            int count = (int)layer.GetFeatureCount(1);

            for (int i = 0; i < count; i++)
            {
                OSGeo.OGR.Feature pFeat = layer.GetFeature(i);
                OSGeo.OGR.Geometry pGeom = pFeat.GetGeometryRef();

                wkbGeometryType wkbType = pGeom.GetGeometryType();
                if (wkbType == wkbGeometryType.wkbMultiPolygon)
                {
                    if (pGeom.GetArea() > 10000)
                    {
                        bigGeos.Add(pGeom);
                        bigFeats.Add(pFeat);
                    }
                    continue;
                }

                int RingsCount = pGeom.GetGeometryCount();
                if (RingsCount > 1)  //多个环
                {
                    OSGeo.OGR.Geometry nGeo = new OSGeo.OGR.Geometry(wkbGeometryType.wkbPolygon);

                    nGeo.AddGeometry(pGeom.GetGeometryRef(0));  //外环

                    for (int k = 1; k < RingsCount; k++)
                    {
                        OSGeo.OGR.Geometry inRing = pGeom.GetGeometryRef(k);
                        if (inRing.GetArea() > area)  //够大的内环
                        {
                            nGeo.AddGeometry(inRing);
                        }

                        #region
                        else  //当有一些小的内环舍弃的时候，需要相应的
                        {
                            OSGeo.OGR.Geometry gg = new OSGeo.OGR.Geometry(wkbGeometryType.wkbPolygon);
                            gg.AddGeometry(inRing);

                            for (int j = 0; j < smallFeats.Count; j++)
                            {
                                if (gg.Intersect(smallGeos[j]))
                                {
                                    smallGeos.RemoveAt(j);
                                    smallFeats.RemoveAt(j);
                                }
                            }
                        }
                        #endregion
                    }
                    pFeat.SetGeometry(nGeo);
                    bigFeats.Add(pFeat);
                    bigGeos.Add(nGeo);
                }
                else  //没有内环的情况
                {
                    if (pGeom.GetArea() > area)
                    {
                        bigFeats.Add(pFeat);
                        bigGeos.Add(pGeom);
                    }
                }
            }

            bigFeats.AddRange(smallFeats);
            OutputLayer(bigFeats);
        }

        private void OutputLayer(List<OSGeo.OGR.Feature> feats)
        {
            OSGeo.OGR.Layer newLayer = ds.CreateLayer(tablename, sr, wkbGeometryType.wkbPolygon, new string[] { });
            OSGeo.OGR.FieldDefn fld = new FieldDefn("PlgAttr", FieldType.OFTString);
            newLayer.CreateField(fld, 0);

            int count = feats.Count;
            for (int i = 0; i < count; i++)
            {
                newLayer.CreateFeature(feats[i]);
            }

            newLayer.Dispose();
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
                            layer.Dispose();
                            ds.Dispose();
                            poDriver.Dispose();

                            smallFeats.ForEach(delegate(OSGeo.OGR.Feature feat)
                            {
                                feat.Dispose();
                            });

                            bigFeats.ForEach(delegate(OSGeo.OGR.Feature feat)
                            {
                                feat.Dispose();
                            });

                            smallGeos.ForEach(delegate(OSGeo.OGR.Geometry g)
                            {
                                g.Dispose();
                            });

                            bigGeos.ForEach(delegate(OSGeo.OGR.Geometry g)
                            {
                                g.Dispose();
                            });
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
        ~HandleRemoveHoles()
        {
            Dispose(true);
        }
        #endregion
    }
}
