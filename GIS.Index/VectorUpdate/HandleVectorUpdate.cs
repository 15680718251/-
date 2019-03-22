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

namespace GIS.TreeIndex.VectorUpdate
{
    /// <summary>
    /// 矢量更新，分为以下几个步骤
    /// 1，首先对增量图层进行碎多边形处理，剔除掉大部分碎多边形
    /// 2，对增量图层和处理后的碎多边形进行分割
    /// 3，对分割后的图层分别进行更新，最后合并结果图层
    /// </summary>
    public class HandleVectorUpdate
    {
        string srcFilename;
        string incrFilename;
        double filterArea;
        OSGeo.OGR.Driver poDriver;
        private static int sp = 0;
        //OSGeo.OGR.DataSource tmpDs; //读取Temp目录下的矢量文件
        int m; int n;
        List<OSGeo.OGR.Envelope> envs = new List<Envelope>();

        //分割后的多边形文件路径
        List<string> srcfilenames = new List<string>();
        List<string> incfilenames = new List<string>();

        DebugLog log;
        frmVectorUpdateProgress m_frmProgressBar;

        public HandleVectorUpdate(string str1,string str2,double area,frmVectorUpdateProgress fm)
        {
            srcFilename = str1;
            incrFilename = str2;
            filterArea = area;
            m_frmProgressBar = fm;
            log = new DebugLog();

            string pszDriverName = "ESRI Shapefile";
            OSGeo.OGR.Ogr.RegisterAll();
            poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);
            if (poDriver == null)
            {
                MessageBox.Show("Driver Error", "提示");
                return;
            }
        }

        ~HandleVectorUpdate()
        {
            poDriver.Dispose();
        }

        public void DoWork()
        {
            log.WriteLog("------------------------------------------");
            RemoveSmallPolygons();
            //SpiltVector();  //20    //wang

            //UnionSmallPolygon();  //30    //wang
            sp = 0;
            Update();  //20
            sp = 0;
            //UnionSmallPolygon();  //30
        }

        //分割矢量图层,分割增量图层时
        private void SpiltVector()
        {
            OSGeo.OGR.DataSource ds1 = poDriver.Open(srcFilename, 0); //底图
            OSGeo.OGR.DataSource ds2 = poDriver.Open(incrFilename, 0); //增量

            if (ds2 == null || ds1 == null)
            {
                MessageBox.Show("DataSource Error", "提示");
                return;
            }

            OSGeo.OGR.Layer srcLayer = ds1.GetLayerByIndex(0);
            OSGeo.OGR.Layer incLayer = ds2.GetLayerByIndex(0);
            OSGeo.OSR.SpatialReference sr = srcLayer.GetSpatialRef();

            OSGeo.OGR.Envelope srcEnvelope = new Envelope(); srcLayer.GetExtent(srcEnvelope, 0);
            OSGeo.OGR.Envelope incEnvelope = new Envelope(); incLayer.GetExtent(incEnvelope, 0);

            double x1 = srcEnvelope.MaxX - srcEnvelope.MinX;
            double y1 = srcEnvelope.MaxY - srcEnvelope.MinY;

            double x2 = incEnvelope.MaxX - incEnvelope.MinX;
            double y2 = incEnvelope.MaxY - incEnvelope.MinY;

            //应该是在一定范围
            //if (srcEnvelope.MinX > incEnvelope.MinX || srcEnvelope.MaxX < incEnvelope.MaxX || srcEnvelope.MinY > incEnvelope.MinY || srcEnvelope.MaxY < incEnvelope.MaxY)
            //{
            //    MessageBox.Show("增量图层和原始图层范围不符合更新要求","提示");
            //    return;
            //}

            //int am = Convert.ToInt32(y1/10000.0);
            //int an= Convert.ToInt32(y1 / 10000.0);

            m = Convert.ToInt32(System.Math.Ceiling(y1 / 5000.0));
            n = Convert.ToInt32(System.Math.Ceiling(y1 / 5000.0));

            //if (y1<5000  && x1<5000)
            //{
            //    OutPutText("输入多边形大小合适，无需分块");
            //    log.WriteLog("输入多边形无需分块，直接开始更新...");

            //    srcfilenames.Add(srcFilename);
            //    incfilenames.Add(incrFilename);

            //    srcLayer.Dispose();
            //    incLayer.Dispose();

            //    ds1.Dispose();
            //    ds2.Dispose();

            //    SetProgressBar(20);
            //    return;
            //}



            string folder = System.IO.Path.GetDirectoryName(srcFilename);
            string tmpFolder = folder + "\\Temp";
            string srcfilename = System.IO.Path.GetFileNameWithoutExtension(srcFilename);
            string incfilename=System.IO.Path.GetFileNameWithoutExtension(incrFilename);

            if (!Directory.Exists(tmpFolder))//判断文件夹是否已经存在
            {
                Directory.CreateDirectory(tmpFolder);//创建文件夹用来存放中间文件
            }
            

            double xStep = 0.0;
            double yStep = 0.0;

            if (m == 0)
            {
                yStep = y1 / 2;
            }
            else if (n == 0)
            {
                xStep = x1 / 2;
            }
            else
            {
                xStep = x1 / n;
                yStep = y1 / m;
            }

            SetlbDescriptionText("正在进行多边形分块...");
            OutPutText("开始分割多边形....");
            log.WriteLog("开始对底图和增量图层进行分块:");
            int ccc = 0;
            List<OSGeo.OGR.Geometry> glists = new List<Geometry>();
            try
            {
                int ncount = 0;
                for (int j = 0; j < m; j++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        OSGeo.OGR.Geometry geo = new Geometry(wkbGeometryType.wkbPolygon);
                        OSGeo.OGR.Geometry ogrexter = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);//外环
                        OSGeo.OGR.Envelope env = new Envelope();

                        #region 分割过程
                        if (j == m - 1 && i < n - 1)
                        {
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MaxY - j * yStep);
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + (i + 1) * xStep, srcEnvelope.MaxY - j * yStep);

                            ogrexter.AddPoint_2D(srcEnvelope.MinX + (i + 1) * xStep, srcEnvelope.MinY);
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MinY);

                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MaxY - j * yStep);

                        }
                        else if (j < m - 1 && i == n - 1)
                        {
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MaxY - j * yStep);

                            ogrexter.AddPoint_2D(srcEnvelope.MaxX, srcEnvelope.MaxY - j * yStep);
                            ogrexter.AddPoint_2D(srcEnvelope.MaxX, srcEnvelope.MaxY - (j + 1) * yStep);

                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MaxY - (j + 1) * yStep);
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MaxY - j * yStep);

                        }
                        else if (j == m - 1 && i == n - 1)   //右下边一个矩形
                        {
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MaxY - j * yStep);
                            ogrexter.AddPoint_2D(srcEnvelope.MaxX, srcEnvelope.MaxY - j * yStep);
                            ogrexter.AddPoint_2D(srcEnvelope.MaxX, srcEnvelope.MinY);
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MinY);
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MaxY - j * yStep);
                        }
                        else
                        {
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MaxY - j * yStep);
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + (i + 1) * xStep, srcEnvelope.MaxY - j * yStep);
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + (i + 1) * xStep, srcEnvelope.MaxY - (j + 1) * yStep);
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MaxY - (j + 1) * yStep);
                            ogrexter.AddPoint_2D(srcEnvelope.MinX + i * xStep, srcEnvelope.MaxY - j * yStep);
                        }
                        #endregion

                        geo.AddGeometry(ogrexter);
                        geo.GetEnvelope(env); //作为矩形的边框
                        envs.Add(env);

                        //开始分割多边形
                        OSGeo.OGR.DataSource ds3 = poDriver.CreateDataSource(tmpFolder, null);
                        if (ds3 == null)
                        {
                            MessageBox.Show("DataSource Error", "提示");
                            return;
                        }

                        string srcName = srcfilename + "-" + (j + 1).ToString() + "-" + (i + 1).ToString();
                        string incName = incfilename + "-" + (j + 1).ToString() + "-" + (i + 1).ToString();
                        srcfilenames.Add(tmpFolder + "\\" + srcName + ".shp");
                        incfilenames.Add(tmpFolder + "\\" + incName + ".shp");

                        OSGeo.OGR.Layer newSrclayer = ds3.CreateLayer(srcName, sr, wkbGeometryType.wkbMultiPolygon, null);
                        OSGeo.OGR.Layer newIncLayer = ds3.CreateLayer(incName, sr, wkbGeometryType.wkbMultiPolygon, null);
                        OSGeo.OGR.FieldDefn fld = new FieldDefn("PlgAttr", FieldType.OFTString);
                        newSrclayer.CreateField(fld, 0);
                        newIncLayer.CreateField(fld, 0);

                        #region 分割底图
                        srcLayer.ResetReading();  //底图
                        srcLayer.SetSpatialFilterRect(env.MinX, env.MinY, env.MaxX, env.MaxY);
                        OSGeo.OGR.Feature feat;
                        while (null != (feat = srcLayer.GetNextFeature()))
                        {
                            OSGeo.OGR.Geometry g = feat.GetGeometryRef();

                            if (EnvelopeContains(env, g))  //矩形里面直接全部接受
                            {
                                newSrclayer.CreateFeature(feat);
                            }
                            else if (g.Intersect(geo))
                            {
                                OSGeo.OGR.Feature poFeature = new OSGeo.OGR.Feature(srcLayer.GetLayerDefn());

                                //OSGeo.OGR.Geometry tg = g.Intersection(geo);
                                OSGeo.OGR.Geometry tg = g.Difference(g.Difference(geo));//wang
                                if (tg.GetGeometryType() == wkbGeometryType.wkbPolygon || tg.GetGeometryType() == wkbGeometryType.wkbMultiPolygon || tg.GetArea() > 0)
                                {
                                    //经调试:相交运算后出现wkbGeometryCollection类型
                                    if (tg.GetGeometryType() == wkbGeometryType.wkbGeometryCollection)
                                    {
                                        OSGeo.OGR.Geometry gg = new Geometry(wkbGeometryType.wkbMultiPolygon);
                                        int a = tg.GetGeometryCount();
                                        for (int k = 0; k < a; k++)
                                        {
                                            OSGeo.OGR.Geometry gt = tg.GetGeometryRef(k);
                                            if (gt.GetArea() > 0)
                                            {
                                                gg.AddGeometry(gt);
                                            }
                                        }
                                        poFeature.SetGeometry(gg);
                                        poFeature.SetField("PlgAttr", feat.GetFieldAsString("PlgAttr"));
                                        newSrclayer.CreateFeature(poFeature);

                                        continue;
                                    }

                                    if (tg.GetArea() > 0)
                                    {
                                        poFeature.SetGeometry(tg);
                                        poFeature.SetField("PlgAttr", feat.GetFieldAsString("PlgAttr"));
                                        newSrclayer.CreateFeature(poFeature);
                                    }
                                }
                            }
                            else
                            {
                                ccc++;
                            }
                        }
                        //ds3.ExecuteSQL("CREATE SPATIAL INDEX ON "+srcName, null, null);
                        newSrclayer.Dispose();
                        #endregion

                        OutPutText("成功生成分块多边形：" + srcName);
                        log.WriteLog("成功生成分块多边形：" + srcName);

                        #region 分割增量图
                        incLayer.ResetReading();  //底图
                        incLayer.SetSpatialFilterRect(env.MinX, env.MinY, env.MaxX, env.MaxY);
                        OSGeo.OGR.Feature feature;
                        while (null != (feature = incLayer.GetNextFeature()))
                        {
                            OSGeo.OGR.Geometry g = feature.GetGeometryRef();

                            if (EnvelopeContains(env, g))  //矩形里面直接全部接受
                            {
                                newIncLayer.CreateFeature(feature);
                            }
                            else if (g.Intersect(geo))
                            {
                                //OSGeo.OGR.Geometry tg = g.Intersection(geo);
                                OSGeo.OGR.Geometry tg = g.Difference(g.Difference(geo));      //wang
                                if (tg.GetGeometryType() == wkbGeometryType.wkbPolygon || tg.GetGeometryType() == wkbGeometryType.wkbMultiPolygon || tg.GetArea() > 0)
                                {
                                    OSGeo.OGR.Feature poFeature = new OSGeo.OGR.Feature(incLayer.GetLayerDefn());
                                    poFeature.SetGeometry(tg);
                                    poFeature.SetField("PlgAttr", feature.GetFieldAsString("PlgAttr"));
                                    newIncLayer.CreateFeature(poFeature);
                                }
                            }
                            else
                            {
                                ccc++;
                            }
                        }
                        //ds3.ExecuteSQL("CREATE SPATIAL INDEX ON " + incName, null, null);
                        newIncLayer.Dispose();
                        #endregion

                        OutPutText("成功生成分块多边形：" + incName);
                        log.WriteLog("成功生成分块多边形：" + incName);

                        ds3.Dispose();

                        ncount++;
                        if (m != 0 && n != 0)
                        {
                            int step = Convert.ToInt32(ncount*20 / (m * n));
                            SetProgressBar(step);
                        }

                        if (m==0)
                        {
                            int step = Convert.ToInt32(((i + 1)*20) / n);
                            SetProgressBar(step);
                        }

                    }
                    if (n == 0)
                    {
                        int setp = Convert.ToInt32((j + 1) * 20 / m);
                        SetProgressBar(setp);
                    }
                    
                }

                SetlbDescriptionText("多边形分块完成");
            }
            catch (Exception ex)
            {
                srcLayer.Dispose();
                incLayer.Dispose();

                ds1.Dispose();
                ds2.Dispose();
                log.WriteLog(ex.ToString());

                MessageBox.Show("矢量图层分块错误","提示");
                
                return;
            }



            srcLayer.Dispose();
            incLayer.Dispose();

            ds1.Dispose();
            ds2.Dispose();

            OutPutText("多边形分块完成");
            log.WriteLog("多边形分块完成");
        }

        private void Update() //对每个分割的小块进行更新
        {
            #region 用多线程代替
            //try
            //{
            //    SetlbDescriptionText("正在进行矢量更新...");
            //    log.WriteLog("开始矢量更新：");
            //    int count = srcfilenames.Count;
            //    for (int i = 0; i < count; i++)
            //    {
            //        SubtractIncrementAndAddIncrement(srcfilenames[i], incfilenames[i]);

            //        int step = Convert.ToInt32((i + 1) * 50 / count);
            //        SetProgressBar(step);

            //        OutPutText("分块图层：" + srcfilenames[i] + "更新完成");
            //        log.WriteLog("分块图层：" + srcfilenames[i] + "更新完成");
            //    }
            //    SetlbDescriptionText("更新完成");
            //    OutPutText("更新完成");
            //    log.WriteLog("更新完成，任务结束");
            //}
            //catch (Exception ex)
            //{
            //    log.WriteLog(ex.ToString());
            //    return;
            //}
            #endregion

            SetlbDescriptionText("正在进行矢量更新...");
            log.WriteLog("开始矢量更新：");

            try
            {
                //int count = srcfilenames.Count;
                //ThreadPool.SetMaxThreads(10, 10);  //最大开启十个线程
                //ManualResetEvent[] _ManualEvents = new ManualResetEvent[count];
                //for (int i = 0; i < count; i++)
                //{
                //    _ManualEvents[i] = new ManualResetEvent(false);
                //    ThreadPool.QueueUserWorkItem(new WaitCallback(SubtractIncrementAndAddIncrement), new object[] { srcfilenames[i], incfilenames[i], _ManualEvents[i] });
                //}
                //WaitHandle.WaitAll(_ManualEvents);



                //wang

                SubtractIncrementAndAddIncrement( new object[] { srcFilename, incrFilename });



            }
            catch(Exception ex)
            {
                log.WriteLog(ex.ToString());
                throw new ApplicationException(ex.ToString());
            }

             SetlbDescriptionText("更新完成");
             OutPutText("更新完成");
             log.WriteLog("更新完成，任务结束");
        }

        private void UnionSmallPolygon()
        {
            SetlbDescriptionText("正在处理相邻小多边形...");
            log.WriteLog("开始处理相邻小多边形：");

             int count = srcfilenames.Count;

             try
             {

                 ThreadPool.SetMaxThreads(10, 10);  //最大开启十个线程
                 ManualResetEvent[] _ManualEvents = new ManualResetEvent[count];
                 for (int i = 0; i < count; i++)
                 {
                     _ManualEvents[i] = new ManualResetEvent(false);
                     ThreadPool.QueueUserWorkItem(new WaitCallback(UnionTouchSmallPolygon), new object[] { srcfilenames[i], filterArea, _ManualEvents[i] });
                 }

                 WaitHandle.WaitAll(_ManualEvents);
             }
             catch (Exception ex)
             {
                 log.WriteLog(ex.ToString());
                 throw new ApplicationException(ex.ToString());
             }

            SetlbDescriptionText("合并相邻小多边形完成");
            OutPutText("处理相邻小多边形完成完成");
            log.WriteLog("处理相邻小多边形完成");
        }

        [DllImport("IncrementUpdateDll_haiou.dll")]
        public static extern int RemoveSmallPolygon(string filename, double filterArea);
        private void RemoveSmallPolygons()
        {
            log.WriteLog("进入碎小多边形处理.....");
            if (filterArea > 0)
            {
                try
                {
                    log.WriteLog("处理 " + incrFilename + "碎小多边形");
                    SetlbDescriptionText("处理 " + incrFilename + "碎小多边形");
                    int count = RemoveSmallPolygon(incrFilename, filterArea);
                    log.WriteLog("成功处理完" + count.ToString() + "个碎小多边形");
                    SetlbDescriptionText("碎小多边形完成");
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString(),"提示");
                    log.WriteLog(ex.ToString());
                    throw new ApplicationException(ex.ToString());
                    //return;
                }
            }
            log.WriteLog("碎小多边形处理完成！");
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

        private void SubtractIncrement(string srcFileName, string incFileName)
        {
            //2016-7-24测试时间
            DateTime startTime = DateTime.Now;

            //2016-7-24测试时间


            OSGeo.OGR.DataSource ds1 = poDriver.Open(srcFileName, 1); //底图
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
            
                        
            //wang
            int srcFeatureCount = (int)srcLayer.GetFeatureCount(0);
            bool redo = true;
            int intersectNum = 0;
            int numberSrc = 0;//求与增量多边形相交的原目标数量


            //define some variables for calculating the quantities of all types of parcels     2015-11-23
            int forestNum = 0;  //森林
            int waterNum = 0;   //水体
            int shrubNum = 0;   //灌木
            int plowlandNum = 0;    //耕地
            int barelandNum = 0;    //裸地
            int grasslandNum = 0;   //草地
            int artificialNum = 0;  //人造覆盖
            int irrigableNum = 0;   //水浇地

            while (redo)
            {
                redo = false;
                //srcFeatureCount = srcLayer.GetFeatureCount(1);
                //srcLayer.ResetReading();
                for (int i = 0; i < srcFeatureCount; i++)           //对每个原目标（2009年底图）
                {
                    OSGeo.OGR.Feature psrcFeature = srcLayer.GetFeature(i);
                    OSGeo.OGR.Geometry psrcGeometry = psrcFeature.GetGeometryRef();

                    //string ss;
                    //psrcGeometry.ExportToWkt(out ss);

                    //MessageBox.Show(ss);
                    //Geometry geom = Geometry.CreateFromWkt("POINT(47.0 19.2)");
                    

                    incLayer.ResetReading();
                    incLayer.SetSpatialFilter(psrcGeometry);   //wang :set current geometry of base map as the filter
                    OSGeo.OGR.Feature pincFeature;
                    bool deleteFlg = false;
                    bool skip = false;

                    while (null != (pincFeature = incLayer.GetNextFeature()))    //wang:  traverse all of increment object in the filter. (nested loop----the reason of poor performance. )
                    {
                        OSGeo.OGR.Geometry pincGeometry = pincFeature.GetGeometryRef();

                        OSGeo.OGR.Geometry AIntersectB = psrcGeometry.Intersection(pincGeometry);
                        if (AIntersectB.GetArea() <= 10.0)
                        {
                            continue;
                        }
                        //else
                        //{
                        //    numberSrc++;//求与增量多边形相交的原目标数量
                        //    break;
                        //}
                        intersectNum++;

                        
                        OSGeo.OGR.Geometry ADiffB = psrcGeometry.Difference(pincGeometry);            //A/B
                        OSGeo.OGR.Geometry BDiffA = pincGeometry.Difference(psrcGeometry);            //B/A
                       // OSGeo.OGR.Geometry AsymDiffB = psrcGeometry.SymmetricDifference(pincGeometry);//A B 的对称差
                        List<OSGeo.OGR.Geometry> arrADiffB = new List<OSGeo.OGR.Geometry>();
                        List<OSGeo.OGR.Geometry> arrBDiffA = new List<OSGeo.OGR.Geometry>();
                        List<OSGeo.OGR.Geometry> arrAIntersectB = new List<OSGeo.OGR.Geometry>();
                        ProcessGeometryToArray(ADiffB, arrADiffB);//A\B结果数组
                        ProcessGeometryToArray(BDiffA, arrBDiffA);//B\A结果数组
                        ProcessGeometryToArray(AIntersectB, arrAIntersectB);//A B 的交 结果数组

                        if (arrADiffB.Count() == 0 && arrBDiffA.Count() == 0)
                        {
                            //string testAttr = pincFeature.GetFieldAsString("PlgAttr");
                            //if (testAttr == "森林")
                            //    forestNum++;
                            //else if (testAttr == "水体")
                            //    waterNum++;
                            //else if (testAttr == "灌木")
                            //    shrubNum++;
                            //else if (testAttr == "耕地")
                            //    plowlandNum++;
                            //else if (testAttr == "裸地")
                            //    barelandNum++;
                            //else if (testAttr == "草地")
                            //    grasslandNum++;
                            //else if (testAttr == "人造覆盖")
                            //    artificialNum++;
                            //else if (testAttr == "水浇地")
                            //{
                            //    int aaaa = srcLayer.GetFeature(i).GetFID();
                            //    irrigableNum++;
                            //}
                            //     A equal B    rule1;
                        }
                        if (arrAIntersectB.Count() == 1 && arrADiffB.Count() == 1 && arrBDiffA.Count() == 0)
                        {
                            //string testAttr = pincFeature.GetFieldAsString("PlgAttr");
                            //if (testAttr == "森林")
                            //    forestNum++;
                            //else if (testAttr == "水体")
                            //    waterNum++;
                            //else if (testAttr == "灌木")
                            //    shrubNum++;
                            //else if (testAttr == "耕地")
                            //    plowlandNum++;
                            //else if (testAttr == "裸地")
                            //    barelandNum++;
                            //else if (testAttr == "草地")
                            //    grasslandNum++;
                            //else if (testAttr == "人造覆盖")
                            //    artificialNum++;
                            //else if (testAttr == "水浇地")
                            //{
                            //    int aaaa = srcLayer.GetFeature(i).GetFID();
                            //    irrigableNum++;
                            //}
                            //A contain B 或  A cover B 用A\B为单个多边形      rule2;
                        }
                        if (arrAIntersectB.Count() == 1 && arrADiffB.Count() > 1 && arrBDiffA.Count() == 0)
                        {
                            //string testAttr = pincFeature.GetFieldAsString("PlgAttr");
                            //if (testAttr == "森林")
                            //    forestNum++;
                            //else if (testAttr == "水体")
                            //    waterNum++;
                            //else if (testAttr == "灌木")
                            //    shrubNum++;
                            //else if (testAttr == "耕地")
                            //    plowlandNum++;
                            //else if (testAttr == "裸地")
                            //    barelandNum++;
                            //else if (testAttr == "草地")
                            //    grasslandNum++;
                            //else if (testAttr == "人造覆盖")
                            //    artificialNum++;
                            //else if (testAttr == "水浇地")
                            //{
                            //    int aaaa = srcLayer.GetFeature(i).GetFID();
                            //    irrigableNum++;
                            //}
                            //A cover B  且A\B结果为多个多边形     rule3;
                        }
                        if (arrAIntersectB.Count() == 1 && arrADiffB.Count() == 0 && arrBDiffA.Count() > 0)
                        {
                            //string testAttr = pincFeature.GetFieldAsString("PlgAttr");
                            //if (testAttr == "森林")
                            //    forestNum++;
                            //else if (testAttr == "水体")
                            //    waterNum++;
                            //else if (testAttr == "灌木")
                            //    shrubNum++;
                            //else if (testAttr == "耕地")
                            //    plowlandNum++;
                            //else if (testAttr == "裸地")
                            //    barelandNum++;
                            //else if (testAttr == "草地")
                            //    grasslandNum++;
                            //else if (testAttr == "人造覆盖")
                            //    artificialNum++;
                            //else if (testAttr == "水浇地")
                            //{
                            //    int aaaa = srcLayer.GetFeature(i).GetFID();
                            //    irrigableNum++;
                            //}
                            //B contain A 或 B cover A         rule4;
                        }
                        if (arrAIntersectB.Count() == 1 && arrADiffB.Count() == 1 && arrBDiffA.Count() > 0)
                        {
                            //string testAttr = pincFeature.GetFieldAsString("PlgAttr");
                            //if (testAttr == "森林")
                            //    forestNum++;
                            //else if (testAttr == "水体")
                            //    waterNum++;
                            //else if (testAttr == "灌木")
                            //    shrubNum++;
                            //else if (testAttr == "耕地")
                            //    plowlandNum++;
                            //else if (testAttr == "裸地")
                            //    barelandNum++;
                            //else if (testAttr == "草地")
                            //    grasslandNum++;
                            //else if (testAttr == "人造覆盖")
                            //    artificialNum++;
                            //else if (testAttr == "水浇地")
                            //{
                            //    int aaaa = srcLayer.GetFeature(i).GetFID();
                            //    irrigableNum++;
                            //}
                            //A、B重叠于A的一端      rule5
                        }
                        if (arrAIntersectB.Count() == 1 && arrADiffB.Count() > 1 && arrBDiffA.Count() > 0)
                        {
                            //string testAttr = pincFeature.GetFieldAsString("PlgAttr");
                            //if (testAttr == "森林")
                            //    forestNum++;
                            //else if (testAttr == "水体")
                            //    waterNum++;
                            //else if (testAttr == "灌木")
                            //    shrubNum++;
                            //else if (testAttr == "耕地")
                            //    plowlandNum++;
                            //else if (testAttr == "裸地")
                            //    barelandNum++;
                            //else if (testAttr == "草地")
                            //    grasslandNum++;
                            //else if (testAttr == "人造覆盖")
                            //    artificialNum++;
                            //else if (testAttr == "水浇地")
                            //{
                            //    int aaaa = srcLayer.GetFeature(i).GetFID();
                            //    irrigableNum++;
                            //}
                            //A、B重叠于A的中间部分    rule6
                        }
                        if (arrAIntersectB.Count() > 1)
                        {
                            if (arrADiffB.Count() == 1)
                            {
                            //    string testAttr = pincFeature.GetFieldAsString("PlgAttr");
                            //    if (testAttr == "森林")
                            //        forestNum++;
                            //    else if (testAttr == "水体")
                            //        waterNum++;
                            //    else if (testAttr == "灌木")
                            //        shrubNum++;
                            //    else if (testAttr == "耕地")
                            //        plowlandNum++;
                            //    else if (testAttr == "裸地")
                            //        barelandNum++;
                            //    else if (testAttr == "草地")
                            //        grasslandNum++;
                            //    else if (testAttr == "人造覆盖")
                            //        artificialNum++;
                            //    else if (testAttr == "水浇地")
                            //    {
                            //        int aaaa = srcLayer.GetFeature(i).GetFID();
                            //        irrigableNum++;
                            //    }
                                //A交B 有多个交 AND    A\B为单个多边形       rule7
                            }
                            else if (arrADiffB.Count() > arrAIntersectB.Count())
                            {
                                //string testAttr = pincFeature.GetFieldAsString("PlgAttr");
                                //if (testAttr == "森林")
                                //    forestNum++;
                                //else if (testAttr == "水体")
                                //    waterNum++;
                                //else if (testAttr == "灌木")
                                //    shrubNum++;
                                //else if (testAttr == "耕地")
                                //    plowlandNum++;
                                //else if (testAttr == "裸地")
                                //    barelandNum++;
                                //else if (testAttr == "草地")
                                //    grasslandNum++;
                                //else if (testAttr == "人造覆盖")
                                //    artificialNum++;
                                //else if (testAttr == "水浇地")
                                //{
                                //    int aaaa = srcLayer.GetFeature(i).GetFID();
                                //    irrigableNum++;
                                //}
                                //A交B 有多个交 AND    A\B数大于交的数量      rule8 
                            }
                            else
                            {
                                //string testAttr = pincFeature.GetFieldAsString("PlgAttr");
                                //if (testAttr == "森林")
                                //    forestNum++;
                                //else if (testAttr == "水体")
                                //    waterNum++;
                                //else if (testAttr == "灌木")
                                //    shrubNum++;
                                //else if (testAttr == "耕地")
                                //    plowlandNum++;
                                //else if (testAttr == "裸地")
                                //    barelandNum++;
                                //else if (testAttr == "草地")
                                //    grasslandNum++;
                                //else if (testAttr == "人造覆盖")
                                //    artificialNum++;
                                //else if (testAttr == "水浇地")
                                //{
                                //    int aaaa = srcLayer.GetFeature(i).GetFID();
                                //    irrigableNum++;
                                //}
                                //A交B 有多个交 AND    A\B数不大于交的数量       rule9
                            }
                        }


                        psrcGeometry = psrcGeometry.Difference(pincGeometry);
                        deleteFlg = true;
                    }//EndWhile

                    if (deleteFlg)
                    {
                        string value = psrcFeature.GetFieldAsString("PlgAttr");
                        srcLayer.DeleteFeature(psrcFeature.GetFID());

                        if (psrcGeometry.GetArea() > 100.0)
                        {
                            OSGeo.OGR.Feature ptmpFeature = new OSGeo.OGR.Feature(srcLayer.GetLayerDefn());
                            ptmpFeature.SetField("PlgAttr", value);
                            ptmpFeature.SetGeometry(psrcGeometry);
                            srcLayer.CreateFeature(ptmpFeature);
                        }
                    } //EndIf

                }//EndFor
            }//EndWhile


            //2016-7-24测试时间
            DateTime stopTime = DateTime.Now;
            TimeSpan elapsedTime = stopTime - startTime;

            //2016-7-24测试时间


            OutPutText("共有" + numberSrc + "个原目标与增量目标相交");
            OutPutText("原目标与增量目标共有" + intersectNum + "次相交");
            OutPutText("规则处理森林的个数为：" + forestNum);
            OutPutText("规则处理水体的个数为：" + waterNum);
            OutPutText("规则处理灌木的个数为：" + shrubNum);
            OutPutText("规则处理耕地的个数为：" + plowlandNum);
            OutPutText("规则处理裸地的个数为：" + barelandNum);
            OutPutText("规则处理草地的个数为：" + grasslandNum);
            OutPutText("规则处理人造覆盖的个数为：" + artificialNum);
            OutPutText("规则处理水浇地的个数为：" + irrigableNum);


            //2016-7-24测试时间
            OutPutText("Elapsed: " + elapsedTime);
            OutPutText("in hours: " + elapsedTime.TotalHours);
            OutPutText("in minutes: " + elapsedTime.TotalMinutes);
            OutPutText("in seconds: " + elapsedTime.TotalSeconds);
            OutPutText("in milliseconds: " + elapsedTime.TotalMilliseconds);
            //2016-7-24测试时间


            ds1.ExecuteSQL("REPACK "+strName, null, null);

            //log.WriteLog(System.DateTime.Now.ToString());

            incLayer.Dispose();
            srcLayer.Dispose();
            ds2.Dispose();
            ds1.Dispose();
        }

        private void ProcessGeometryToArray(OSGeo.OGR.Geometry gg, List<OSGeo.OGR.Geometry> plgArr)
        {
            if (gg.GetArea() < 10.0)
                return;
            if (gg.GetGeometryType() == wkbGeometryType.wkbGeometryCollection)  //如果是集合，则转多多边形
            {
                OSGeo.OGR.Geometry temp = new Geometry(wkbGeometryType.wkbMultiPolygon);
                int n = gg.GetGeometryCount();
                for (int k = 0; k < n; k++)
                {
                    OSGeo.OGR.Geometry gt = new Geometry(wkbGeometryType.wkbPolygon);
                    gt = gg.GetGeometryRef(k);
                    if (gt.GetArea() > 10.0)
                    {
                        temp.AddGeometry(gt);
                    }
                }
                gg = temp;
            }
            int plgCount = gg.GetGeometryCount();
            for (int i = 0; i < plgCount; i++)
            {
                OSGeo.OGR.Geometry tmp = new Geometry(wkbGeometryType.wkbPolygon);
                tmp = gg.GetGeometryRef(i);
                if (tmp.GetArea() > 10.0)
                    plgArr.Add(tmp);
            }
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

            srcLayer.ResetReading();
            int incFeatureCount = (int)incLayer.GetFeatureCount(0);
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

        private void SubtractIncrementAndAddIncrement(object parameter)
        {
            //object[] parameters = (object[])parameter;

            //string srcFileName = (string)parameters[0];
            //string incFileName = (string)parameters[1];
            //ManualResetEvent manualResetEvent = (ManualResetEvent)parameters[2];



            //SubtractIncrement(srcFileName, incFileName);
            //AddIncrement(srcFileName, incFileName);

            //OutPutText("分块图层：" + srcFileName + "更新完成");
            //log.WriteLog("分块图层：" + srcFileName + "更新完成");

            //manualResetEvent.Set();

            //sp++;

            //int step = Convert.ToInt32(sp * 20 / srcfilenames.Count);
            //SetProgressBar(step);








            //wang
            object[] parameters = (object[])parameter;

            string srcFileName = (string)parameters[0];
            string incFileName = (string)parameters[1];


            SubtractIncrement(srcFileName, incFileName);
            //AddIncrement(srcFileName, incFileName);

            OutPutText(srcFileName + "更新完成");
            log.WriteLog(srcFileName + "更新完成");


        }

        //单独处理
        private void UnionTouchSmallPolygon(object parameter)
        {
            object[] parameters = (object[])parameter;
            string filename = (string)parameters[0];
            double area = (double)parameters[1];
            ManualResetEvent manualResetEvent = (ManualResetEvent)parameters[2];
            string folder = System.IO.Path.GetDirectoryName(filename);

            #region Old
            /*
            OSGeo.OGR.DataSource ds = poDriver.Open(filename, 1);

            OSGeo.OGR.Layer layer = ds.GetLayerByIndex(0);
            OSGeo.OSR.SpatialReference sr = layer.GetSpatialRef();

            List<OSGeo.OGR.Feature> bigFeats = new List<Feature>();
            List<OSGeo.OGR.Geometry> bigGeoms = new List<Geometry>();

            List<OSGeo.OGR.Feature> smallFeats=new List<Feature>();
            List<OSGeo.OGR.Geometry> smallGeoms = new List<Geometry>();

            OSGeo.OGR.Feature pFeat=null;
            OSGeo.OGR.Geometry pGeom=null;
            layer.ResetReading();
            while((pFeat=layer.GetNextFeature())!=null)
            {
                pGeom=pFeat.GetGeometryRef();
                if (pGeom.GetArea() < area)
                {
                    smallFeats.Add(pFeat);
                    smallGeoms.Add(pGeom);
                }
                else
                {
                    bigFeats.Add(pFeat);
                    bigGeoms.Add(pGeom);
                }
            }

            layer.Dispose();
            ds.Dispose();

            int bigCount = bigFeats.Count;
            int smallCount = smallFeats.Count;

            List<OSGeo.OGR.Feature> noFeats = new List<Feature>();

            for (int i = 0; i < smallCount; i++)
            {
                bool noFind = true;
                OSGeo.OGR.Geometry g1=smallGeoms[i];
                for (int j = 0; j < bigCount; j++)
                {
                    OSGeo.OGR.Geometry g2=bigGeoms[j];

                    bool b=EnvelopeIntersets(g1,g2);
                    if (!b)
                        continue;

                    if (g1.Disjoint(g2))
                        continue;

                    OSGeo.OGR.Geometry tg=g1.Union(g2);
                    bigGeoms[j] = tg;
                    bigFeats[j].SetGeometry(tg);
                    noFind = false;
                    break;
                }

                if (noFind)
                {
                    noFeats.Add(smallFeats[i]);
                }
            }

            bigFeats.AddRange(noFeats);
            */
            #endregion

            //下面为新修改，保证及时对内存的释放
            GIS.TreeIndex.VectorSmallPolygonsMerge.HandleUnionSmallPolygons h = new GIS.TreeIndex.VectorSmallPolygonsMerge.HandleUnionSmallPolygons(filename, area);
            h.RemoveHoles();
            h.Work();
            h.Dispose();

            //OutputLayer(bigFeats, filename,sr);

            OutPutText(filename + "将相邻的小多边形处理完成");
            log.WriteLog(filename + "将相邻的小多边形处理完成");

            //ds.Dispose();

            manualResetEvent.Set();

            sp++;

            int step = Convert.ToInt32(sp * 30 / srcfilenames.Count);
            SetProgressBar(step);

        }

        private bool EnvelopeIntersets(OSGeo.OGR.Geometry g1, OSGeo.OGR.Geometry g2)
        {
            OSGeo.OGR.Envelope env1 = new Envelope();
            OSGeo.OGR.Envelope env2 = new Envelope();

            g1.GetEnvelope(env1);
            g2.GetEnvelope(env2);

            return env1.MinX < env2.MaxX && env1.MaxX > env2.MinX &&
                   env1.MinY < env2.MaxY && env1.MaxY > env2.MinY;
        }

        private void OutputLayer(List<OSGeo.OGR.Feature> tfeats,string filename,OSGeo.OSR.SpatialReference sr)
        {   
            string name = System.IO.Path.GetFileNameWithoutExtension(filename);
            string folder = System.IO.Path.GetDirectoryName(filename);
            OSGeo.OGR.DataSource ds = poDriver.Open(folder,1);

            OSGeo.OGR.Layer layer = ds.CreateLayer(name, sr, wkbGeometryType.wkbMultiPolygon, null);
            OSGeo.OGR.FieldDefn fld = new FieldDefn("PlgAttr", FieldType.OFTString);
            layer.CreateField(fld, 0);

            int count = tfeats.Count;
            for (int i = 0; i < count; i++)
            {
                layer.CreateFeature(tfeats[i]);
            }
            layer.Dispose();
            ds.Dispose();
        }

        //在进度条界面上输出矢量化过程
        private void OutPutText(string str)
        {
            m_frmProgressBar.txtDescription.Invoke(
            (MethodInvoker)delegate()
            {
                m_frmProgressBar.txtDescription.Text += str + "\r\n";
            }
            );
        }

        //进度条显示，不是一个完整的
        int lastValue = 0;
        int lastMaxValue;
        private void SetProgressBar(int value)
        {
            if (value == lastValue)
            {
                return;
            }
            else if (value > lastValue)
            {
                lastValue = value;
                m_frmProgressBar.progressBar1.Invoke(
               (MethodInvoker)delegate()
               {
                   m_frmProgressBar.progressBar1.Value = lastMaxValue + value;
               }
               );
            }
            else
            {
                lastValue = value;
                lastMaxValue = m_frmProgressBar.progressBar1.Value; //这里取得阶段性最大值
                if (value == 0)
                    return;
                else
                {
                    m_frmProgressBar.progressBar1.Invoke(
                    (MethodInvoker)delegate()
                    {
                        m_frmProgressBar.progressBar1.Value = lastMaxValue + value;
                    }
                    );
                }
            }
        }

        private void SetlbDescriptionText(string text)
        {
            m_frmProgressBar.lblDescription.Invoke(
                (MethodInvoker)delegate()
                {
                    m_frmProgressBar.lblDescription.Text = text;
                }
                );
        }

    }
}
