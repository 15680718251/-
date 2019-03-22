using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using OSGeo.OSR;
using System.Windows.Forms;

namespace GIS.TreeIndex.VectorSmallPolygonsMerge
{
    public class HandleUnionSmallPolygons : IDisposable
    {
        string filename;
        double area;
        string tablename;

        OSGeo.OGR.Driver poDriver;
        public OSGeo.OGR.DataSource ds;
        public OSGeo.OSR.SpatialReference sr;
        OSGeo.OGR.Layer layer;
        string folder;

        List<OSGeo.OGR.Feature> smallFeats = new List<OSGeo.OGR.Feature>();
        List<OSGeo.OGR.Geometry> smallGeos = new List<OSGeo.OGR.Geometry>();
        List<OSGeo.OGR.Geometry> bigGeos = new List<OSGeo.OGR.Geometry>();
        List<OSGeo.OGR.Feature> bigFeats = new List<OSGeo.OGR.Feature>();

        public HandleUnionSmallPolygons(string name,double areafilter)
        {
            this.filename = name;
            this.area = areafilter;

            string pszDriverName = "ESRI Shapefile";
            OSGeo.OGR.Ogr.RegisterAll();
            poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);

            if (poDriver == null)
            {
                MessageBox.Show("Driver Error", "提示");
                return;
            }

            folder = System.IO.Path.GetDirectoryName(name);
            ds = poDriver.Open(folder, 1);
            tablename = System.IO.Path.GetFileNameWithoutExtension(name);
            layer = ds.GetLayerByName(tablename);
            sr = layer.GetSpatialRef();

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
                else
                {
                    bigFeats.Add(pFeature);
                    bigGeos.Add(pGeometry);
                }
            }

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
        ~HandleUnionSmallPolygons()
        {
            Dispose(true);
        }
        #endregion

        public void RemoveHoles()
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
        }

        public void Work()
        {
            int bigSize = bigGeos.Count;
            int smallSize = smallGeos.Count;

            List<OSGeo.OGR.Feature> noFeats = new List<Feature>();

            for (int i = 0; i < smallSize; i++)
            {
                //bool noFind = true;
                OSGeo.OGR.Geometry g1 = smallGeos[i];
                for (int j = 0; j < bigSize; j++)
                {
                    OSGeo.OGR.Geometry g2 = bigGeos[j];

                    bool b = IsEnvelopesIntersect(g1, g2);
                    if (!b)
                        continue;

                    //修改，不能相邻就直接合并，还需要判断相邻的情况 点相邻不能合并，边相邻则合并
                    if (g1.Disjoint(g2))
                        continue;

                    OSGeo.OGR.Geometry intersection_g = g1.Intersection(g2);
                    wkbGeometryType intersection_t=intersection_g.GetGeometryType();
                    if (intersection_t == wkbGeometryType.wkbPoint||intersection_t==wkbGeometryType.wkbMultiPoint)
                        continue;

                    //if (intersection_t == wkbGeometryType.wkbLineString || intersection_t == wkbGeometryType.wkbMultiLineString)
                    //{

                    //}

                    OSGeo.OGR.Geometry tg = g1.Union(g2);
                    bigGeos[j] = tg;
                    bigFeats[j].SetGeometryDirectly(tg);
                    //noFind = false;
                    break;
                }

                //if (noFind)
                //{
                //    noFeats.Add(smallFeats[i]);
                //}
            }

            //bigFeats.AddRange(noFeats);
            OutputLayer(bigFeats);
        }

        private bool IsEnvelopesIntersect(OSGeo.OGR.Geometry g1, OSGeo.OGR.Geometry g2)
        {
            OSGeo.OGR.Envelope env1 = new Envelope();
            OSGeo.OGR.Envelope env2 = new Envelope();
            g1.GetEnvelope(env1);
            g2.GetEnvelope(env2);

            return env1.MinX <= env2.MaxX && env1.MaxX >= env2.MinX && env1.MinY <= env2.MaxY && env1.MaxY >= env2.MinY;
        }

        private bool IsEnvelopesContains(OSGeo.OGR.Geometry g1, OSGeo.OGR.Geometry g2)
        {
            OSGeo.OGR.Envelope env1 = new Envelope();
            OSGeo.OGR.Envelope env2 = new Envelope();
            g1.GetEnvelope(env1);
            g2.GetEnvelope(env2);

            return env1.MinX <= env2.MinX && env1.MinY <= env2.MinY && env1.MaxX >= env2.MaxX && env1.MaxY >= env2.MinY;
        }

        private void OutputLayer(List<OSGeo.OGR.Feature> feats)
        {
            OSGeo.OGR.Layer newLayer = ds.CreateLayer(tablename, sr, wkbGeometryType.wkbPolygon, new string[] { });
            OSGeo.OGR.FieldDefn fld = new FieldDefn("PlgAttr", FieldType.OFTString);
            newLayer.CreateField(fld,0);

            int count=feats.Count;
            for (int i = 0; i < count; i++)
            {
                newLayer.CreateFeature(feats[i]);
            }

            newLayer.Dispose();
        }


    }
}
