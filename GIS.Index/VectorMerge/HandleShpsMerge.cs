using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;
using OSGeo.OSR;
using System.Windows.Forms;

namespace GIS.TreeIndex.VectorMerge
{
    public class HandleShpsMerge:IDisposable
    {
        OSGeo.OGR.Driver poDriver;
        public OSGeo.OGR.DataSource ds;
        public OSGeo.OSR.SpatialReference sr;

        string[] strs;
        List<string> names;
        List<OSGeo.OGR.Layer> layers;
        //List<OSGeo.OGR.Envelope> envs;
        //OSGeo.OGR.FeatureDefn fdn;

        string folder;
        frmVectorMergeProgress m_frmProgressBar;

        List<OSGeo.OGR.Feature> feats = new List<Feature>();

        public HandleShpsMerge(string[] paths, frmVectorMergeProgress frm)
        {
            strs = paths;
            names = new List<string>();
            layers = new List<OSGeo.OGR.Layer>();
            //envs = new List<Envelope>();
            m_frmProgressBar = frm;

            string pszDriverName = "ESRI Shapefile";
            OSGeo.OGR.Ogr.RegisterAll();
            poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);
            
            if (poDriver == null)
            {
                MessageBox.Show("Driver Error", "提示");
                return;
            }

            folder = System.IO.Path.GetDirectoryName(strs[0]);
            ds = poDriver.Open(folder, 1);

            //int lc = ds.GetLayerCount();

            for (int i = 0; i < strs.Length; i++)
            {
                names.Add(System.IO.Path.GetFileNameWithoutExtension(strs[i]));
                layers.Add(ds.GetLayerByName(names[i]));

                //OSGeo.OGR.Envelope env = new Envelope();
                //layers[i].GetExtent(env, 0);
                //envs.Add(env);
            }

            sr = layers[0].GetSpatialRef();
            //fdn = layers[0].GetLayerDefn();

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

                            layers.ForEach(delegate(OSGeo.OGR.Layer layer)
                            {
                                layer.Dispose();
                            });

                            feats.ForEach(delegate(OSGeo.OGR.Feature feat)
                            {
                                feat.Dispose();
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
        ~HandleShpsMerge()
        {
            Dispose(true);
        }
        #endregion


        public void Merge()
        {
            int count=layers.Count;
            for (int i = 0; i < count; i++)
            {
                OSGeo.OGR.Layer tlayer = layers[i];
                OSGeo.OGR.Feature feat;
                while (null != (feat = tlayer.GetNextFeature()))
                {
                    feats.Add(feat);
                }

                int step = Convert.ToInt32(i*50/count);
            }

            SetProgressBar(50);

            OutputLayer(feats);

            #region 废弃

            //OSGeo.OGR.Envelope env1 = envs[0];

            //string str=names[names.Count-1];
            //string[] strs = str.Substring(str.Length - 3).Split('_');
            //int m = Convert.ToInt32(strs[0]);
            //int n = Convert.ToInt32(strs[1]);

            //List<List<OSGeo.OGR.Feature>> allFeatures = new List<List<Feature>>();
            //for (int k = 0; k < m * n; k++)
            //{
            //    allFeatures.Add(new List<Feature>());
            //}
           

            #region 第一步，不与边界相接触的多边形直接添加进来
            /*
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    OSGeo.OGR.Layer tlayer = layers[t];
                    OSGeo.OGR.Feature feat;
                    while (null != (feat = tlayer.GetNextFeature()))
                    {
                        feats.Add(feat);

                        OSGeo.OGR.Geometry tg = feat.GetGeometryRef();
                        strLists.Add(feat.GetFieldAsString("PlgAttr"));
                        OSGeo.OGR.Envelope tenv = new Envelope();
                        tg.GetEnvelope(tenv);
                        if (i == 0&&j==0)
                        {
                            if (tenv.MaxX<envs[t].MaxX&&tenv.MinY>envs[t].MinY||tg.GetArea()<100000)
                                layer.CreateFeature(feat);
                            else
                            {
                                //feats.Add(feat);
                                allFeatures[i * n + j].Add(feat);
                                count++;
                            }
                            continue;
                        }
                        else if (i==0&&j>0&&j<n-1)
                        {
                            if (tenv.MaxX < envs[t].MaxX && tenv.MinY > envs[t].MinY && tenv.MinX > envs[t].MinX || tg.GetArea() < 100000)
                                layer.CreateFeature(feat);
                            else
                            {
                                //feats.Add(feat);
                                allFeatures[i * n + j].Add(feat);
                                count++;
                            }
                            continue;
                        }
                        else if (i == 0 && j == n - 1)
                        {
                            if (tenv.MinY > envs[t].MinY && tenv.MinX > envs[t].MinX || tg.GetArea() < 100000)
                                layer.CreateFeature(feat);
                            else
                            {
                                //feats.Add(feat);
                                allFeatures[i * n + j].Add(feat);
                                count++;
                            }
                            continue;
                        }
                        else if (i > 0 && i < m - 1 && j > 0 && j < n - 1)
                        {
                            if (EnvelopeCompare(tenv, envs[t]) || tg.GetArea() < 100000)
                               layer.CreateFeature(feat);
                            else
                            {
                                //feats.Add(feat);
                                allFeatures[i * n + j].Add(feat);
                                count++;
                            }
                            continue;
                        }
                        else if (i > 0 && i < m - 1 && j == 0)
                        {
                            if (tenv.MinY > envs[t].MinY && tenv.MaxY < envs[t].MaxY && tenv.MaxX < envs[t].MaxX || tg.GetArea() < 100000)
                                layer.CreateFeature(feat);
                            else
                            {
                                //feats.Add(feat);
                                allFeatures[i * n + j].Add(feat);
                                count++;
                            }
                            continue;
                        }
                        else if (i == m - 1 && j == 0)
                        {
                            if (tenv.MaxY < envs[t].MaxY && tenv.MaxX < envs[t].MaxX || tg.GetArea() < 100000)
                                layer.CreateFeature(feat);
                            else
                            {
                                //feats.Add(feat);
                                allFeatures[i * n + j].Add(feat);
                                count++;
                            }
                            continue;
                        }
                        else if (i == m - 1 && j > 0 && j < n - 1)
                        {
                            if (tenv.MaxX < envs[t].MaxX && tenv.MaxY < envs[t].MaxY && tenv.MinX > envs[t].MinX || tg.GetArea() < 100000)
                                layer.CreateFeature(feat);
                            else
                            {
                                //feats.Add(feat);
                                allFeatures[i * n + j].Add(feat);
                                count++;
                            }
                            continue;
                        }
                        else if (i == m - 1 && j == n - 1)
                        {
                            if (tenv.MaxY < envs[t].MaxY && tenv.MinX > envs[t].MinX || tg.GetArea() < 100000)
                                layer.CreateFeature(feat);
                            else
                            {
                                //feats.Add(feat);
                                allFeatures[i * n + j].Add(feat);
                                count++;
                            }
                            continue;
                        }
                        else if (i > 0 && i < m - 1 && j == n - 1)
                        {
                            if (tenv.MinY > envs[t].MinY && tenv.MaxY < envs[t].MaxY && tenv.MinX > envs[t].MinX || tg.GetArea() < 100000)
                                layer.CreateFeature(feat);
                            else
                            {
                                //feats.Add(feat);
                                allFeatures[i * n + j].Add(feat);
                                count++;
                            }
                            continue;
                        }
                    }
                    t++;
                }
            }
             */
            #endregion

            //HashSet<string> hs = new HashSet<string>(strLists);
            //strLists.Clear();
            //strLists.AddRange(hs);

            //List<OSGeo.OGR.Feature> ls = MergeAll(allFeatures, strLists);

            #endregion
        }

        private OSGeo.OGR.Layer CopyLayer(OSGeo.OGR.Layer layer)
        {
            OSGeo.OGR.Layer newLayer;
            string layername = "UnionResult_" + System.DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss");
            newLayer = ds.CopyLayer(layer, layername, new string[] { });
            return newLayer;
        }

        private OSGeo.OGR.Layer CreateLayer()
        {
            string layername = "UnionResult_" + System.DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd-HH-mm-ss");
            OSGeo.OGR.Layer layer = ds.CreateLayer(layername, sr, wkbGeometryType.wkbMultiPolygon, null);
            OSGeo.OGR.FieldDefn fld = new FieldDefn("PlgAttr", FieldType.OFTString);
            layer.CreateField(fld, 0);
            return layer;
        }

        //输出
        private void OutputLayer(List<OSGeo.OGR.Feature> tfeats)
        {
            string layername = "UnionResult_" + System.DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss");
            string path = System.IO.Path.GetDirectoryName(strs[0]);

            OSGeo.OGR.Layer layer = ds.CreateLayer(layername, sr, wkbGeometryType.wkbMultiPolygon, null);
            OSGeo.OGR.FieldDefn fld = new FieldDefn("PlgAttr", FieldType.OFTString);
            layer.CreateField(fld, 0);

            int a = 0;
            int count = tfeats.Count;
            for (int i = 0; i < count; i++)
            {
                layer.CreateFeature(tfeats[i]);
                int step = Convert.ToInt32(a*50/count);
                SetProgressBar(step+50);
            }
            layer.Dispose();
        }

        private void SetProgressBar(int value)
        {
            m_frmProgressBar.progressBar1.Invoke(
           (MethodInvoker)delegate()
           {
               m_frmProgressBar.progressBar1.Value = value;
           }
           );
        }


      #region 废弃

        /*
      //env1是否完全被env2包含，注意是完全包含，不包括相接触
      private bool EnvelopeCompare(OSGeo.OGR.Envelope env1, OSGeo.OGR.Envelope env2)
      {
          return env1.MinX > env2.MinX && env1.MinY > env2.MinY &&
                 env1.MaxX < env2.MaxX && env1.MaxY < env2.MaxY;
      }

      //判断env1和env2相交
      private bool EnvelopeIntersets(OSGeo.OGR.Envelope env1, OSGeo.OGR.Envelope env2)
      {
          return env1.MinX <= env2.MaxX && env1.MaxX >= env2.MinX &&
                 env1.MinY <= env2.MaxY && env1.MaxY >= env2.MinY;
      }

      //主要判断两个矩形的公共边界，并返回公共边
      private OSGeo.OGR.Geometry EnvelopeTouch(OSGeo.OGR.Envelope env1, OSGeo.OGR.Envelope env2)
      {
          OSGeo.OGR.Geometry rg = new Geometry(wkbGeometryType.wkbLineString);

          if (env1.MaxX == env2.MinX && env1.MaxY == env2.MaxY && env1.MinY == env2.MinY)
          {
              rg.AddPoint_2D(env1.MaxX, env1.MaxY);
              rg.AddPoint_2D(env1.MaxX, env1.MinY);
          }
          else if (env1.MinX == env2.MaxX && env1.MaxY == env2.MaxY && env1.MinY == env2.MinY)
          {
              rg.AddPoint_2D(env1.MinX, env1.MaxY);
              rg.AddPoint_2D(env1.MinX, env1.MinY);
          }
          else if (env1.MinY == env2.MaxY && env1.MaxX == env2.MaxX && env1.MinX == env2.MinX)
          {
              rg.AddPoint_2D(env1.MinX, env1.MinY);
              rg.AddPoint_2D(env1.MaxX, env1.MinY);
          }
          else if (env1.MaxY == env2.MinY && env1.MaxX == env2.MaxX && env1.MinX == env2.MinX)
          {
              rg.AddPoint_2D(env1.MinX, env1.MaxY);
              rg.AddPoint_2D(env1.MaxX, env1.MaxY);
          }
          return rg;
      }

      private void TT(List<OSGeo.OGR.Feature> feats, List<string> strs)
      {
          Dictionary<string, List<OSGeo.OGR.Feature>> dics = new Dictionary<string, List<Feature>>();

          int count = strs.Count;
          for (int k = 0; k < count; k++)
          {
              dics.Add(strs[k], new List<Feature>());
          }


      }

      private List<OSGeo.OGR.Feature> MergeAll(List<List<OSGeo.OGR.Feature>> allfeats, List<string> strs)
      {
          List<OSGeo.OGR.Feature> rfeats = new List<Feature>(); //需要返回的要素

          Dictionary<string, List<OSGeo.OGR.Feature>> dics = new Dictionary<string, List<Feature>>();

          int count = strs.Count;
          for (int k = 0; k < count; k++)
          {
              dics.Add(strs[k], new List<Feature>());
          }

          //for (int j = 0; j < allfeats.Count; j++)
          //{
          //    List<OSGeo.OGR.Feature> tfeats = allfeats[j];

          //} 

          rfeats = MergeTwoBlock(allfeats[0], allfeats[1], envs[0], envs[1], strs);

          return rfeats;
      }

      private List<OSGeo.OGR.Feature> MergeTwoBlock(List<List<OSGeo.OGR.Feature>> allfeats, List<string> strs)
      {
          List<OSGeo.OGR.Feature> rfeats = new List<Feature>(); //需要返回的要素

          Dictionary<string, List<OSGeo.OGR.Feature>> dics = new Dictionary<string, List<Feature>>();

          int count = strs.Count;
          for (int k = 0; k < count; k++)
          {
              dics.Add(strs[k], new List<Feature>());
          }

          for (int j = 0; j < allfeats.Count; j++)
          {
              List<OSGeo.OGR.Feature> tfeats = allfeats[j];

          }

          return rfeats;
      }

      //没有办法而为之
      private List<OSGeo.OGR.Feature> MergeTwoBlock(List<OSGeo.OGR.Feature> feats1,List<OSGeo.OGR.Feature> feats2,OSGeo.OGR.Envelope env1, OSGeo.OGR.Envelope env2,List<string> strs)
      {

          List<OSGeo.OGR.Feature> rfeats = new List<Feature>(); //需要返回的要素

          //目的是得到两个图块中每种类型待合并的要素
          Dictionary<string, List<OSGeo.OGR.Feature>> dics1 = new Dictionary<string, List<Feature>>();  
          Dictionary<string, List<OSGeo.OGR.Feature>> dics2 = new Dictionary<string, List<Feature>>();

          int count=strs.Count;
          for (int k = 0; k < count; k++)
          {
              dics1.Add(strs[k], new List<Feature>());
              dics2.Add(strs[k], new List<Feature>());
          }

          #region 挑选和边界接触的要素
          OSGeo.OGR.Geometry gline = EnvelopeTouch(env1, env2); //公共边界
          if (gline.IsEmpty() && gline.IsValid())
          {
              return null;
          }
          else
          {
              for (int i = 0; i < feats1.Count; i++)
              {
                  OSGeo.OGR.Geometry tg1 = feats1[i].GetGeometryRef();
                  if (tg1.Disjoint(gline))
                  {
                      rfeats.Add(feats1[i]);
                      continue;
                  }
                  string str = feats1[i].GetFieldAsString("PlgAttr");
                  dics1[str].Add(feats1[i]);
                  //newFeats1.Add(feats1[i]);
              }

              for (int j = 0; j < feats2.Count; j++)
              {
                  OSGeo.OGR.Geometry tg2 = feats2[j].GetGeometryRef();
                  if (tg2.Disjoint(gline))
                  {
                      rfeats.Add(feats2[j]);
                      continue;
                  }
                  string str = feats2[j].GetFieldAsString("PlgAttr");
                  //dics2[str].Add(feats2[j]);
                  dics2[str].Add(feats2[j]);
                  //newFeats2.Add(feats2[j]);
              }
          }
          #endregion

          //已经都是相同类型，只需要比较是否相接处
          for (int k = 0; k < count; k++)
          {
              List<OSGeo.OGR.Feature> feat1=dics1[strs[k]];
              List<OSGeo.OGR.Feature> feat2=dics2[strs[k]];

              int m = feat1.Count;
              int n = feat2.Count;

              if (m == 0 || n == 0)
              {
                  rfeats.AddRange(feat1);
                  rfeats.AddRange(feat2);
                  continue;
              }               

              for (int i = 0; i < feat1.Count; i++)
              {
                  bool b = false;
                  OSGeo.OGR.Geometry g1 = feat1[0].GetGeometryRef(); //永远都是取第一个

                  for (int j = 0; j < n; j++)
                  {
                      OSGeo.OGR.Geometry g2 = feat2[j].GetGeometryRef();

                      if (g1.Disjoint(g2)) //两个面相离
                      {
                          continue;
                      }

                      OSGeo.OGR.Geometry rg = g1.Intersection(g2);
                      wkbGeometryType rtype = rg.GetGeometryType();
                      if (rtype == wkbGeometryType.wkbLineString || rtype == wkbGeometryType.wkbMultiLineString) //两个面相交
                      {
                          b = true;
                          OSGeo.OGR.Geometry g = g1.Union(g2);
                          feat2[j].SetGeometry(g);
                          break;
                      }

                  }

                  if (b)
                  {
                      rfeats.Add(feat1[0]);
                  }

                  feat1.RemoveAt(0); //移除当前要素
              }

              rfeats.AddRange(feat2);
                
          }

           #region 复杂
          for (int i = 0; i < count; i++)
          {
              List<OSGeo.OGR.Feature> dfeats1=dics1[strs[i]];
              List<OSGeo.OGR.Feature> dfeats2 = dics2[strs[i]];

              int j = dfeats1.Count;

              for (j = 0; j < dfeats1.Count; j++)
              {
                  OSGeo.OGR.Geometry g1 = dfeats1[j].GetGeometryRef();

                  bool rb = false; //如果碰到有相接触，并且进行过合并就改为true，若为false，则直接添加到返回集合
                  OSGeo.OGR.Geometry g = null;

                  for (int k = 0; k < dfeats2.Count; k++)
                  {
                      OSGeo.OGR.Geometry g2 = dfeats2[k].GetGeometryRef();
                      if (g1.Disjoint(g2)) //两个面相离
                      {
                          continue;
                      }

                      OSGeo.OGR.Geometry rg = g1.Intersection(g2);
                      wkbGeometryType rtype = rg.GetGeometryType();
                      if (rtype == wkbGeometryType.wkbLineString || rtype == wkbGeometryType.wkbMultiLineString) //两个面相交
                      {
                          rb = true;
                          g = g1.Union(g2);

                          g1 = g;  //这个放在这里非常消耗时间，但是不放在这里又有问题

                          //dfeats2[k].SetGeometry(g);  //合并后的几何对象，赋值到内循环数组中，自己思考原因

                          dfeats1[j].SetGeometry(g);
                          dfeats2.RemoveAt(k);
                          k = k - 1;
                      }
                  }

                  //if (!rb)
                  //{
                  //    rfeats.Add(dfeats1[j]);
                  //}

                  rfeats.Add(dfeats1[j]);
              }

              rfeats.AddRange(dfeats2);
          }
              #endregion

              #region 一起合并，已废弃
          for (int i = 0; i < count; i++)
          {
              List<OSGeo.OGR.Feature> dfeats1 = dics1[strs[i]];
              OSGeo.OGR.Geometry rg = new Geometry(wkbGeometryType.wkbMultiPolygon);

              int n = dfeats1.Count;
              if (n == 0)
                  continue;

              for (int j = 0; j < n; j++)
              {
                  OSGeo.OGR.Geometry g1 = dfeats1[j].GetGeometryRef();
                  rg = rg.Union(g1);
              }

              OSGeo.OGR.Feature feat = new Feature(fdn);
              feat.SetField("PlgAttr", strs[i]);
              feat.SetGeometry(rg);
                
              rfeats.Add(feat);
          }
              #endregion

              return rfeats;
      }

      private List<OSGeo.OGR.Feature> MergeTwoBlock(List<OSGeo.OGR.Feature> feats1, List<OSGeo.OGR.Feature> feats2, OSGeo.OGR.Envelope env1, OSGeo.OGR.Envelope env2)
      {
          List<OSGeo.OGR.Feature> newFeats1 = new List<Feature>();
          List<OSGeo.OGR.Feature> newFeats2 = new List<Feature>();

          List<OSGeo.OGR.Feature> rfeats = new List<Feature>(); //需要返回的要素

          #region 挑选和边界接触的要素
          OSGeo.OGR.Geometry gline = EnvelopeTouch(env1, env2);
          if (gline.IsEmpty() && gline.IsValid())
          {
              return null;
          }
          else
          {
              for (int i = 0; i < feats1.Count; i++)
              {
                  OSGeo.OGR.Geometry tg1 = feats1[i].GetGeometryRef();
                  if (tg1.Disjoint(gline))
                  {
                      rfeats.Add(feats1[i]);
                      continue;
                  }
                  newFeats1.Add(feats1[i]);
              }

              for (int j = 0; j < feats2.Count; j++)
              {
                  OSGeo.OGR.Geometry tg2 = feats2[j].GetGeometryRef();
                  if (tg2.Disjoint(gline))
                  {
                      rfeats.Add(feats2[j]);
                      continue;
                  }
                  newFeats2.Add(feats2[j]);
              }
          }
          #endregion


          int n=newFeats1.Count;
          for (int i = 0; i < n; i++)  //外循环中把不进行合并的直接添加到返回集合中
          {
              string value1 = newFeats1[i].GetFieldAsString("PlgAttr");
              OSGeo.OGR.Geometry g1 = newFeats1[i].GetGeometryRef();

              bool rb = false; //如果碰到有相接触，并且进行过合并就改为true，若为false，则直接添加到返回集合
              OSGeo.OGR.Geometry g = null;
              for (int j = 0; j < newFeats2.Count; j++)
              {
                  string value2 = newFeats2[j].GetFieldAsString("PlgAttr");
                  if (value1 != value2)
                  {
                      //rfeats.Add(feats2[j]);
                      continue;
                  }

                  OSGeo.OGR.Geometry g2 = newFeats2[j].GetGeometryRef();
                  if (g1.Disjoint(g2)) //两个面相离
                  {
                      continue;
                  }

                  OSGeo.OGR.Geometry rg = g1.Intersection(g2);
                  wkbGeometryType rtype = rg.GetGeometryType();
                  if (rtype == wkbGeometryType.wkbLineString || rtype == wkbGeometryType.wkbMultiLineString) //两个面相交
                  {
                      rb = true;
                      g = g1.Union(g2);

                      //g1 = g;  //这个放在这里非常消耗时间，但是不放在这里又有问题

                      newFeats2[j].SetGeometry(g);  //合并后的几何对象，赋值到内循环数组中，自己思考原因
                  }
                  else
                  {
                      continue;
                  }

              }

              if (!rb)
              {
                  rfeats.Add(newFeats1[i]);
              }
          }

          rfeats.AddRange(newFeats2);

          return rfeats;

      }

      //合并两个图块
      private void MergeTwoBlock(List<OSGeo.OGR.Feature> feats1,OSGeo.OGR.Envelope env1,List<OSGeo.OGR.Feature> feats2,OSGeo.OGR.Envelope env2)
      {
          OSGeo.OGR.Geometry gline = EnvelopeTouch(env1, env2);
          if (gline.IsEmpty() && gline.IsValid())
          {
              return ;
          }
          else
          {
              List<OSGeo.OGR.Feature> rfeats = new List<Feature>();

              for (int i = 0; i < feats1.Count; i++)
              {
                  rfeats.Add(feats1[i]);

                  string value1 = feats1[i].GetFieldAsString("PlgAttr");

                  OSGeo.OGR.Geometry tg1 = feats1[i].GetGeometryRef();
                  OSGeo.OGR.Envelope tenv1 = new Envelope();
                  tg1.GetEnvelope(tenv1);

                  for (int j = 0; j < feats2.Count; j++)
                  {
                      string value2 = feats2[j].GetFieldAsString("PlgAttr");
                      if (value1 != value2)
                      {
                          rfeats.Add(feats2[j]);
                          continue;
                      }

                      OSGeo.OGR.Geometry tg2 = feats2[j].GetGeometryRef();
                      OSGeo.OGR.Envelope tenv2 = new Envelope();
                      tg2.GetEnvelope(tenv2);

                      if (!EnvelopeIntersets(tenv1, tenv2))
                      {
                          rfeats.Add(feats2[j]);
                      }
                      else
                      {
                          OSGeo.OGR.Geometry rg = tg1.Intersection(tg2);
                          wkbGeometryType rtype = rg.GetGeometryType();
                          if (rtype == wkbGeometryType.wkbLineString || rtype == wkbGeometryType.wkbMultiLineString)
                          {
                              OSGeo.OGR.Geometry g = tg1.Union(tg2);
                              feats1[i].SetGeometry(g);
                          }
                      }
                  }
              }
          }
      }

      /// <summary>
      /// 拼接两个矢量图层
      /// </summary>
      private void Merge(OSGeo.OGR.Layer layer1, OSGeo.OGR.Layer layer2, OSGeo.OGR.Geometry line)
      {
          string name = layer1.GetName();
          OSGeo.OGR.Envelope env1 = new Envelope();
          layer1.GetExtent(env1, 0);
          //OSGeo.OGR.Envelope env2 = new Envelope();
          //layer2.GetExtent(env2, 0);

          //OSGeo.OGR.Geometry g = new OSGeo.OGR.Geometry(wkbGeometryType.wkbLineString);
          //g.AddPoint_2D(env1.MaxX, env1.MaxY);
          //g.AddPoint_2D(env1.MaxX, env1.MinY);

          layer1.ResetReading();
          layer1.SetSpatialFilter(line);

          List<OSGeo.OGR.Feature> features = new List<Feature>();
          int count = 0;
          OSGeo.OGR.Feature feature = layer1.GetNextFeature();
          while (feature != null)
          {
              count++;
              int id = feature.GetFID();
              features.Add(feature);
              feature = layer1.GetNextFeature();
          }

          int a = 0;
          int k = layer2.GetFeatureCount(0);
          layer2.ResetReading();
          for (int i = 0; i < k; i++)  //遍历新添加的图层，不相接触的直接添加，相接处的进一步判断处理
          {
              OSGeo.OGR.Feature feat = layer2.GetFeature(i);
              OSGeo.OGR.Geometry tg = feat.GetGeometryRef();
              if (tg == null)
                  continue;
              if (tg.GetArea() == 0)
                  continue;

              OSGeo.OGR.Envelope env = new Envelope();
              tg.GetEnvelope(env);
              if (env.MinX > env1.MaxX)  //肯定不相接触
              {
                  layer1.CreateFeature(feat);
              }
              else  //应该基本都是相接处的多边形，
              {
                  a++;
                  string value = feat.GetFieldAsString("PlgAttr");
                  int n = features.Count;
                  for (int j = 0; j < n; j++)
                  {
                      OSGeo.OGR.Feature oldFeature = features[j];
                      string nvalue = oldFeature.GetFieldAsString("PlgAttr");
                      if (value != nvalue)
                          continue;

                      OSGeo.OGR.Geometry og = oldFeature.GetGeometryRef();
                      if (og == null)
                      {
                          continue;
                      }

                      OSGeo.OGR.Geometry rg = tg.Intersection(og);
                      if (rg == null || rg.GetGeometryType() == wkbGeometryType.wkbPoint||rg.GetGeometryType()==wkbGeometryType.wkbMultiPoint)
                      {
                          continue;
                      }

                      if (rg.GetGeometryType() == wkbGeometryType.wkbLineString || rg.GetGeometryType() == wkbGeometryType.wkbMultiLineString) //表示此时需要合并过去
                      {
                          int fid = oldFeature.GetFID();
                          OSGeo.OGR.Geometry sg = og.Union(tg);
                          oldFeature.SetGeometry(sg);
                          layer1.SetFeature(oldFeature);
                      }
                  }
              }
          }
          ds.ExecuteSQL("REPACK "+name, null, null);
      }
       */

#endregion
    }
}
