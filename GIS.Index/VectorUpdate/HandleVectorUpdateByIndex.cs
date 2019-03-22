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
using GIS.TreeIndex.Index;
using GIS.Utilities;
using GIS.Layer;
using GIS.Geometries;
using GIS.Toplogical;
using GIS.TreeIndex.GeometryComputation;
using GIS.GeoData;
using GIS.Converters.GeoConverter;
using GIS.SpatialRelation;
//oracle的引用
using Oracle.ManagedDataAccess.Client;

namespace GIS.TreeIndex.VectorUpdate
{
    /// <summary>
    /// 矢量更新，分为以下几个步骤
    /// 1，首先对增量图层进行碎多边形处理，剔除掉大部分碎多边形
    /// 2，对增量图层和处理后的碎多边形进行分割
    /// 3，对分割后的图层分别进行更新，最后合并结果图层
    /// </summary>
    public class HandleVectorUpdateByIndex
    {
        string baslyr;
        string inclyr;
        double filterArea;
        MapUI m_map;
        private static int sp = 0;


        //分割后的多边形文件路径
        List<string> srcfilenames = new List<string>();
        List<string> incfilenames = new List<string>();

        DebugLog log;
        frmVectorUpdateProgress m_frmProgressBar;

        public HandleVectorUpdateByIndex(string str1, string str2, double area, frmVectorUpdateProgress fm, MapUI map)
        {
            baslyr = str1;
            inclyr = str2;
            m_map = map;
            filterArea = area;
            m_frmProgressBar = fm;
            log = new DebugLog();
        }

        ~HandleVectorUpdateByIndex()
        {
            
        }

        public void DoWork()
        {
            log.WriteLog("------------------------------------------");
            Update();
        }

        private void Update() //对每个分割的小块进行更新
        {
            SetlbDescriptionText("正在进行矢量更新...");
            log.WriteLog("开始矢量更新：");
            try
            {
                #region 作废
                //int count = srcfilenames.Count;
                //ThreadPool.SetMaxThreads(10, 10);  //最大开启十个线程
                //ManualResetEvent[] _ManualEvents = new ManualResetEvent[count];
                //for (int i = 0; i < count; i++)
                //{
                //    _ManualEvents[i] = new ManualResetEvent(false);
                //    ThreadPool.QueueUserWorkItem(new WaitCallback(SubtractIncrementAndAddIncrement), new object[] { srcfilenames[i], incfilenames[i], _ManualEvents[i] });
                //}
                //WaitHandle.WaitAll(_ManualEvents);
                #endregion
                //wang
                SubtractIncrement(baslyr, inclyr);
                //SubtractIncrementDirectly(baslyr, inclyr);
             // SubtractIncrementAndAddIncrement( new object[] { baslyr, inclyr });
            }
            catch (Exception ex)
            {
                log.WriteLog(ex.ToString());
                throw new ApplicationException(ex.ToString());
            }
            SetlbDescriptionText("更新完成");
            OutPutText("更新完成");
            log.WriteLog("更新完成，任务结束");
        }

        #region 作废
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
                    log.WriteLog("处理 " + inclyr + "碎小多边形");
                    SetlbDescriptionText("处理 " + inclyr + "碎小多边形");
                    int count = RemoveSmallPolygon(inclyr, filterArea);
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
        #endregion

        private void SubtractIncrementDirectly(string baslyr, string inclyr)
        {
            //2017-8-8测试时间
            DateTime startTime = DateTime.Now;
            //2017-8-8测试时间
            GeoVectorLayer srcLayer = m_map.GetLayerByName(baslyr) as GeoVectorLayer;
            GeoVectorLayer incLayer = m_map.GetLayerByName(inclyr) as GeoVectorLayer;
            int srcFeatureCount = srcLayer.DataTable.Count;
            int incFeatureCount = incLayer.DataTable.Count;
            if (incFeatureCount == 0 || srcFeatureCount == 0)
            {
                return;
            }
            DescriptionIncrementNumber(incFeatureCount.ToString());//显示增量多边形数目
            for (int i = 0; i < incFeatureCount; i++)
            {
                RefreshUpdatingProcess((i + 1).ToString());//显示已更新增量多边形数目
                //if (i == 97)
                //    continue;
                RefreshSrcNumber(srcLayer.DataTable.Count.ToString());//显示目前的底图多边形数目
                OSGeo.OGR.Geometry ogr_incP = GeometryConverter.GeometricToOGR(incLayer.DataTable[i].Geometry as GeoPolygon);//增量多边形
                List<GeoObjects> gg = m_map.QuadIndex.Root.IntersectQueryIncludeFake(incLayer.DataTable[i].Geometry as GeoPolygon, m_map.QuadIndex.heurdata);
                for (int j = 0; j < gg.Count; j++)
                {
                    OSGeo.OGR.Geometry ogr_srcP = GeometryConverter.GeometricToOGR(gg[j].CurrentPolygon as Geometries.Geometry);//原多边形
                    OSGeo.OGR.Geometry ogr_resultP = ogr_srcP.Difference(ogr_incP);// 裁剪更新，获得结果多边形
                    if (ogr_resultP.GetGeometryType() == wkbGeometryType.wkbMultiPolygon)
                    {
                        for (int k = 0; k < ogr_resultP.GetGeometryCount(); k++)
                        {
                            //GeoDataRow r = gg[j].Row.Clone();
                            GeoDataRow r = srcLayer.DataTable.NewRow();
                            r["PlgID"] = r["FID"];
                            r.Geometry = GeometryConverter.OGRTOGeometric(ogr_resultP.GetGeometryRef(k));
                            srcLayer.DataTable.AddRow(r);
                            GeoObjects resultP = new GeoObjects(r);
                            m_map.QuadIndex.Insert(resultP);
                        }

                    }
                    else if (ogr_resultP.GetGeometryType() == wkbGeometryType.wkbPolygon)
                    {
                        //GeoDataRow r = gg[j].Row.Clone();
                        GeoDataRow r = srcLayer.DataTable.NewRow();
                        r["PlgID"] = r["FID"];
                        r.Geometry = GeometryConverter.OGRTOGeometric(ogr_resultP);
                        srcLayer.DataTable.AddRow(r);
                        GeoObjects resultP = new GeoObjects(r);
                        m_map.QuadIndex.Insert(resultP);
                    }
                    gg[j].Row.Geometry = null;
                }
                GC.Collect();
            }

            //2017-8-8测试时间
            DateTime stopTime = DateTime.Now;
            TimeSpan elapsedTime = stopTime - startTime;
            //2017-8-8测试时间
            OutPutText("Elapsed: " + elapsedTime);
            OutPutText("in hours: " + elapsedTime.TotalHours);
            OutPutText("in minutes: " + elapsedTime.TotalMinutes);
            OutPutText("in seconds: " + elapsedTime.TotalSeconds);
            //2017-8-8测试时间            
            for (int j = 0; j < srcLayer.DataTable.Count; j++)
            {
                if (srcLayer.DataTable[j].Geometry == null)
                {
                    srcLayer.DataTable.Rows.RemoveAt(j);
                    j--;
                }
            }



        }
        private void SubtractIncrement(string baslyr, string inclyr)
        {
            //2016-7-24测试时间
            DateTime startTime = DateTime.Now;
            //2016-7-24测试时间
            GeoVectorLayer srcLayer = m_map.GetLayerByName(baslyr) as GeoVectorLayer;
            GeoVectorLayer incLayer = m_map.GetLayerByName(inclyr) as GeoVectorLayer;
            GeoVectorLayer testLayer = m_map.GetLayerByName("test") as GeoVectorLayer;

            //test(srcLayer, incLayer);

            #region 查找与增量多边形有交的多边形，并更新
            if (m_map.QuadIndex == null)
            {
                MessageBox.Show("请先创建底图的索引！", "Tip");

                #region 读取数据库中的索引文件 zh修改 2018年3月5号
                string connectionString;
                connectionString = "User Id=system;Password=Zh522700;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.98)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=myoracle)))";
                OracleConnection myConnection = new OracleConnection(connectionString);
                myConnection.Open();

                //读取quadtreeindex表
                string queryString = "select * from quadtreeindex";
                OracleCommand command = new OracleCommand(queryString, myConnection);
                OracleDataReader myDataReader = command.ExecuteReader();
                string mroot = "";
                while (myDataReader.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了  
                {
                    mroot = myDataReader.GetOracleValue(0).ToString();
                }
                myDataReader.Close();
                MessageBox.Show("根节点编号：" + mroot, "调试");

                //从当前活动页面获取GeoObjects对象
                List<GeoObjects> geoObjects = new List<GeoObjects>();
                GeoVectorLayer actlyr = srcLayer;
                if ((actlyr == null) || (actlyr.LayerTypeDetail != LAYERTYPE_DETAIL.PolygonLayer))
                {
                    MessageBox.Show("请先设置面要素图层为活动图层！");
                    return;
                }
                int objNum = actlyr.DataTable.Count;
                for (int i = 0; i < objNum; i++)
                {
                    GeoDataRow r = actlyr.DataTable[i];
                    GeoObjects go = new GeoObjects(r);
                    geoObjects.Add(go);
                }

                //读取quadtreenode表，建立索引节点
                string query = String.Format("select * from quadtreenode where M_ID={0}", mroot);
                OracleCommand cmd = new OracleCommand(query, myConnection);
                QuadTreeNode root = new QuadTreeNode();
                OracleDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了  
                {
                    root.ID = int.Parse(mroot);
                    root.NodeType = int.Parse(dataReader.GetOracleValue(1).ToString());
                    root.Depth = int.Parse(dataReader.GetOracleValue(2).ToString());

                    string mbound = dataReader.GetOracleValue(8).ToString().Trim();
                    if (!mbound.Equals(""))
                    {
                        root.mBound = new GeoBound(mbound);
                    }
                    string obound = dataReader.GetOracleValue(9).ToString().Trim();
                    if (!mbound.Equals(""))
                    {
                        root.mBound = new GeoBound(obound);
                    }

                    List<GeoObjects> objects = new List<GeoObjects>();
                    root.m_objList = objects;
                    string oblist = dataReader.GetOracleValue(10).ToString().Trim();
                    string[] m_oblist = oblist.Split(',');
                    for (int i = 0; i < m_oblist.Length; i++)
                    {
                        //string myquery = String.Format("select * from geoobjects where PLGID={0}", m_oblist[i]);
                        //OracleCommand mycmd = new OracleCommand(query, myConnection);
                        //OracleDataReader mydataReader = mycmd.ExecuteReader();
                        //if (mydataReader.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了
                        //{
                        //    objects.Add(geoObjects[PlgIDToFID[int.Parse(m_oblist[i])]]);//读取m_oblist字段
                        //}

                        //objects.Add(geoObjects[PlgIDToFID[int.Parse(m_oblist[i])]]);//读取m_oblist字段
                    }

                    int childnw = int.Parse(dataReader.GetOracleValue(3).ToString());
                    int childne = int.Parse(dataReader.GetOracleValue(4).ToString());
                    int childsw = int.Parse(dataReader.GetOracleValue(5).ToString());
                    int childse = int.Parse(dataReader.GetOracleValue(6).ToString());
                    if (childnw != -1)
                    {
                        //CreateIndexTree(myConnection, root, childnw, 1, geoObjects, PlgIDToFID);//需先建立geoObjects对象
                    }
                    if (childne != -1)
                    {
                        //CreateIndexTree(myConnection, root, childne, 2, geoObjects, PlgIDToFID);//需先建立geoObjects对象
                    }
                    if (childsw != -1)
                    {
                        //CreateIndexTree(myConnection, root, childsw, 3, geoObjects, PlgIDToFID);//需先建立geoObjects对象
                    }
                    if (childse != -1)
                    {
                        //CreateIndexTree(myConnection, root, childse, 4, geoObjects, PlgIDToFID);//需先建立geoObjects对象
                    }
                }
                dataReader.Close();

                myConnection.Close();
                #endregion
                return;

            }
            int srcFeatureCount = srcLayer.DataTable.Count;
            int incFeatureCount = incLayer.DataTable.Count;
            if (incFeatureCount == 0 || srcFeatureCount == 0)
            {
                return;
            }
            //int testddd = 0;//调试
            //GeoObjects testg = m_map.QuadIndex.Root.QueryByID(382, ref testddd);
            //testddd = 6;
            List<GeoObjects> gg = null;
            DescriptionIncrementNumber(incFeatureCount.ToString());//显示增量多边形数目
            for (int i = 0; i < incFeatureCount; i++)
            {
                RefreshUpdatingProcess((i+1).ToString());//显示已更新增量多边形数目
                RefreshSrcNumber(srcLayer.DataTable.Count.ToString());//显示目前的底图多边形数目
//找与当前增量多边形相交并需要更新的多边形gg
                GeoObjects incObj = new GeoObjects(incLayer.DataTable[i]);//调试时用
                //if ((incObj.ID == 22308))// || incObj.ID == 30435 || incObj.ID == 30600))
                //{
                //    int testdata = 0;;
                //}
                gg = m_map.QuadIndex.Root.IntersectQuery(incLayer.DataTable[i].Geometry as GeoPolygon, m_map.QuadIndex.heurdata);
                //if (incObj.ID == 3474)
                //{
                //    int dataset = 0;
                ////    //if (testLayer != null)
                ////    //{
                ////    //    GeoDataRow r = testLayer.DataTable[0].Clone();
                ////    //    r.Geometry = gg[0].CurrentPolygon as Geometries.Geometry;
                ////    //    testLayer.DataTable.AddRow(r);
                ////    //    for (int ii = 1; ii < gg.Count; ii++)
                ////    //    {
                ////    //        r = testLayer.DataTable[0].Clone();
                ////    //        r.Geometry = gg[ii].CurrentPolygon as Geometries.Geometry;
                ////    //        testLayer.DataTable.AddRow(r);
                ////    //    }
                ////    //    return;
                ////    //}
                //}
                //注意：找到的这些gg对象即将进行裁剪并产生新的多边形，所以这些gg所涉及的包含关系都需要更新！！
                bool boolParentInGg = false;
                GeoObjects parentGO = GetParentGO(gg, ref boolParentInGg);
                GeoLinearRing RingInParentOfParentInGg = null;
                GeoLinearRing RingInParentOfParentNotInGg = null;
                OSGeo.OGR.Geometry ogr_incP = GeometryConverter.GeometricToOGR(incLayer.DataTable[i].Geometry as GeoPolygon);//增量多边形
         //逐个多边形进行增量更新(裁剪)
                for (int j = 0; j < gg.Count; j++)
                {
                    //GeoDataRow rr = gg[j].Row.Clone();
                    GeoDataRow rr = srcLayer.DataTable.NewRow();
                    rr["PlgID"] = rr["FID"];
                    //将当前多边形从其父多边形中删除
                    if (gg[j].ParentObj != null) 
                        gg[j].ParentObj.Childrens.Remove(gg[j]);
             //1)生成更新多边形newP（gg[j]中可能有很多内环，一般只有较少内环需要参与更新，更新多边形即为需要参与计算的环构成的多边形）
                    GeoPolygon newP = new GeoPolygon(gg[j].CurrentPolygon.ExteriorRing);
                    List<GeoLinearRing> RingInParents = new List<GeoLinearRing>();
                    for (int k = 0; k < gg.Count; k++)  //生成需要参与裁剪计算的多边形gg[j]的内环（其内环必为gg中的多边形的RingInParent）
                    {
                        if (gg[j].CurrentPolygon.InteriorRings.Count() == 0)//如果gg[j]没有内环
                            break;
                        if (j == k)
                            continue;
                        if (gg[j] == gg[k].ParentObj && gg[k].RingInParent != null) 
                        {
                            GeoPolygon RingPolygonOfChild = new GeoPolygon(gg[k].RingInParent);//RingInParent构成多边形（环多边形）
                            OSGeo.OGR.Geometry ogr_RingPolygonOfChild = GeometryConverter.GeometricToOGR(RingPolygonOfChild as Geometries.Geometry);
                            if (ogr_RingPolygonOfChild.Intersect(ogr_incP))//由于在查找时将包含和覆盖排除了，所以只要相交就意味着需要计算
                            {
                                //说明：下面代码避免生成多边形newP中的内环被重复加入的情况（多个内多边形共用RingInParent造成）
                                bool needAdd = true;
                                for (int kk = 0; kk < newP.InteriorRings.Count(); kk++)
                                {
                                    if (newP.InteriorRings[kk] == gg[k].RingInParent)
                                    {
                                        gg[k].RingInParent = null;
                                        needAdd = false;
                                    }
                                }
                                if (needAdd)
                                {
                                    newP.InteriorRings.Add(gg[k].RingInParent);
                                    RingInParents.Add(gg[k].RingInParent);//因为newP中的环在经过裁剪后就改变了，存储的目的是为了更新以该环为RingInParent的其他未参与计算子多边形新的RingInParent，如笔记Aa页的多边形E
                                    //可以优化，将此环作标记，在加入时判别
                                    gg[j].CurrentPolygon.InteriorRings.Remove(gg[k].RingInParent);//将对应的内环从原多边形中删除，留下的内环需要再加入新生成的多边形中
                                    gg[k].RingInParent = null;
                                }
                            }
                        }
                    }//生成newP
             //2)更新，生成更新后多边形结果resultPs（集合）（无不相交的环）
                    OSGeo.OGR.Geometry ogr_newP = GeometryConverter.GeometricToOGR(newP as Geometries.Geometry);
                    OSGeo.OGR.Geometry ogr_resultP = null;
                    try
                    {
                        ogr_resultP = ogr_newP.Difference(ogr_incP);// 裁剪更新，获得结果多边形
                    }
                    catch (Exception e)
                    {
                        //continue;
                        MessageBox.Show("In Difference:" + incObj.ID.ToString() + ":  " + e.Message.ToString());
                        if (testLayer != null)
                        {
                            GeoDataRow r = testLayer.DataTable.NewRow();
                            r.Geometry = newP as Geometries.Geometry;
                            testLayer.DataTable.AddRow(r);
                            //r = testLayer.DataTable[0].Clone();
                            //r.Geometry = incObj.CurrentPolygon as Geometries.Geometry;
                            //testLayer.DataTable.AddRow(r);
                        }
                        return;
                    }
                    List<GeoPolygon> resultPs = new List<GeoPolygon>();
                    if (ogr_resultP.GetGeometryType() == wkbGeometryType.wkbGeometryCollection)
                    {
                        OSGeo.OGR.Geometry temp = new OSGeo.OGR.Geometry(wkbGeometryType.wkbMultiPolygon);
                        for (int k = 0; k < ogr_resultP.GetGeometryCount(); k++)
                        {
                            if (ogr_resultP.GetGeometryRef(k).GetArea() < Geometries.Geometry.EPSIONAL)
                                continue;
                            temp.AddGeometry(ogr_resultP.GetGeometryRef(k));
                        }
                        for (int k = 0; k < temp.GetGeometryCount(); k++)
                        {
                            if (temp.GetGeometryRef(k).GetArea() < Geometries.Geometry.EPSIONAL)
                                continue;
                            resultPs.Add(GeometryConverter.OGRTOGeometric(temp.GetGeometryRef(k)) as GeoPolygon);
                        }
                    }
                    else if (ogr_resultP.GetGeometryType() == wkbGeometryType.wkbMultiPolygon)
                    {
                        for (int k = 0; k < ogr_resultP.GetGeometryCount(); k++)//将多多边形放入数组resultPs中
                        {
                            if (ogr_resultP.GetGeometryRef(k).GetArea() < Geometries.Geometry.EPSIONAL)
                                continue;
                            resultPs.Add(GeometryConverter.OGRTOGeometric(ogr_resultP.GetGeometryRef(k)) as GeoPolygon);
                        }

                    }
                    else if (ogr_resultP.GetGeometryType() == wkbGeometryType.wkbPolygon)//如果是单多边形，则更新原数据
                    {
                        if (ogr_resultP.GetArea() < Geometries.Geometry.EPSIONAL)
                            continue;
                        resultPs.Add(GeometryConverter.OGRTOGeometric(ogr_resultP) as GeoPolygon);
                    }
                    else//若裁剪结果为空，则将gg[j]置为空，注意更新完成后，应该扫描srcLayer.DataTable，将空行删除
                    {
                        //gg[j].Row.Geometry = null;
                        continue;
                    }
                    newP = null;
                    //将resultP中具有最大MBR的元素置换到末位，避免判断点在环内的开销
                    double length = 0;
                    int max = 0;
                    for (int k = 0; k < resultPs.Count; k++)
                    {
                        double len = (resultPs[k].Bound.LeftBottomPt.X - resultPs[k].Bound.RightUpPt.X) * (resultPs[k].Bound.LeftBottomPt.X - resultPs[k].Bound.RightUpPt.X) +
                            (resultPs[k].Bound.LeftBottomPt.Y - resultPs[k].Bound.RightUpPt.Y) * (resultPs[k].Bound.LeftBottomPt.Y - resultPs[k].Bound.RightUpPt.Y);//len = x * x + y * y
                        if (len > length)
                        {
                            length = len;
                            max = k;
                        }
                    }
                    if ((resultPs.Count > 1) && (max != resultPs.Count - 1))
                    {
                        GeoPolygon temp = resultPs[max];
                        resultPs[max] = resultPs[resultPs.Count - 1];
                        resultPs[resultPs.Count - 1] = temp;
                    }

            //3)逐个处理更新后多边形
                    //分为两种情形，一种为父多边形不在gg中，另一种为父多边形在gg中
                    if (!boolParentInGg)//父多边形不在gg中，此时ParentObj及RingInParent是原来存在，不参与更新
                    {
                        for (int k = 0; k < resultPs.Count; k++)
                        {
                            //生成新的结果对象，维护ParentObj及RingInParent
                            //GeoDataRow r = rr.Clone();
                            GeoDataRow r = srcLayer.DataTable.NewRow();
                            r["PlgID"] = r["FID"];
                            r.Geometry = resultPs[k] as Geometries.Geometry;
                            srcLayer.DataTable.AddRow(r);   //将resultPs[k]插入到数据表中，下面将对resultP进行维护parentObj、RingInParent、Childrens、内环
                            GeoObjects resultP = new GeoObjects(r); //resultP.CurrentPolygon即为resultPs[k]
                            resultP.ParentObj = gg[j].ParentObj;         //从原多边形处继承父多边形
                            resultP.RingInParent = gg[j].RingInParent;
                            if (gg[j].ParentObj != parentGO)//gg中多边形拥有共同外层父多边形，gg[j]为笔记Ab页二：图(2)中的E或F，此时它们的ParentObj及RingInParent与B、C相同
                            {
                                GeoObjects temp = gg[j];
                                while (temp.ParentObj != parentGO)
                                {
                                    temp = temp.ParentObj;
                                }
                                resultP.ParentObj = temp.ParentObj;
                                resultP.RingInParent = temp.RingInParent;
                            }
                            if (RingInParentOfParentNotInGg == null)
                                RingInParentOfParentNotInGg = resultP.RingInParent;
                            if (parentGO != null) 
                                parentGO.Childrens.Add(resultP);//将当前裁剪后结果多边形放入其父多边形的孩子结点数组中
                            //i)第一步处理子多边形
                            //处理更新前的多边形中未参与更新的子多边形（判断其属于更新后产生的哪个多边形）
                            //有两种未参与更新的子多边形情形：一种是其RingInParent未参与更新，这种多边形仍是子多边形，其RingInParent不变，需要找出归属更新后的哪个多边形
                            //另一种是其本身未参与更新，但其RingInParent参与了更新，如笔记Ab页二(2)多边形G，除了需要确定其ParentObj，还需要更新其RingInParent
                            List<GeoLinearRing> CommonRingInParent = new List<GeoLinearRing>();
                            for (int jj = 0; jj < gg[j].Childrens.Count(); jj++)
                            {
                                if (gg[j].Childrens[jj] == null)
                                {
                                    continue;
                                }
                                if (gg.Contains(gg[j].Childrens[jj])) //为了尽量少遍历gg[j]的所有孩子结点，在此删除包含在gg中的gg[j]的孩子结点
                                {
                                    gg[j].Childrens[jj] = null;
                                    //gg[j].Childrens.RemoveAt(jj);
                                    //jj--;
                                    continue;
                                }
                                bool isChild = true;

                                for (int kk = 0; kk < RingInParents.Count(); kk++)//RingInParents是参与更新的内环备份
                                {
                                    if (!RingInParents[kk].Bound.Contains(gg[j].Childrens[jj].RingInParent.Bound))//如果当前孩子结点的MBR不被RingInParents[kk]的MBR包含，则是孩子
                                    {
                                        continue;
                                    }
                                    if (GeoAlgorithm.IsInLinearRingWHS(gg[j].Childrens[jj].RingInParent.Vertices[0], RingInParents[kk]) >= 1)
                                    {
                                        isChild = false;
                                        break;
                                    }
                                    //GeoPolygon p = new GeoPolygon(RingInParents[kk]);
                                    //OSGeo.OGR.Geometry ogr_polygon = GeometryConverter.GeometricToOGR(p as Geometries.Geometry);
                                    //OSGeo.OGR.Geometry ogr_Point = GeometryConverter.GeometricToOGR(gg[j].Childrens[jj].RingInParent.Vertices[0] as Geometries.Geometry);
                                    //if (ogr_Point.Distance(ogr_polygon) < Geometries.Geometry.EPSIONAL)//关于G多边形的判断需再优化，如何尽快找出G
                                    //{
                                    //    isChild = false;
                                    //    break;
                                    //}
                                }
                                if (isChild)//判断是否为当前resultP的子多边形
                                {
                                    if (resultPs.Count == 1 || k == resultPs.Count - 1)//只用一个或最后一个
                                    {
                                        resultP.Childrens.Add(gg[j].Childrens[jj]);
                                        gg[j].Childrens[jj].ParentObj = resultP;
                                        gg[j].Childrens[jj] = null;
                                    }
                                    else
                                    {
                                        if (GeoAlgorithm.IsInLinearRingWHS(gg[j].Childrens[jj].RingInParent.Vertices[0], resultP.CurrentPolygon.ExteriorRing) >= 1)
                                        {
                                            resultP.Childrens.Add(gg[j].Childrens[jj]);
                                            gg[j].Childrens[jj].ParentObj = resultP;
                                            gg[j].Childrens[jj] = null;
                                        }
                                        //GeoPolygon p = new GeoPolygon(resultP.CurrentPolygon.ExteriorRing);
                                        //OSGeo.OGR.Geometry ogr_polygon = GeometryConverter.GeometricToOGR(p as Geometries.Geometry);
                                        //OSGeo.OGR.Geometry ogr_Point = GeometryConverter.GeometricToOGR(gg[j].Childrens[jj].RingInParent.Vertices[0] as Geometries.Geometry);
                                        //if (ogr_Point.Distance(ogr_polygon) < Geometries.Geometry.EPSIONAL)
                                        //{
                                        //    resultP.Childrens.Add(gg[j].Childrens[jj]);
                                        //    gg[j].Childrens[jj].ParentObj = resultP;
                                        //    gg[j].Childrens[jj] = null;
                                        //}
                                    }
                                }
                                else
                                {
                                    if (parentGO != null)
                                        parentGO.Childrens.Add(gg[j].Childrens[jj]);//如果当前子多边形的RingInParent参与了更新，则该多边形是ParentGO的子多边形
                                    //必存在与该多边形具有共同RingInParent的多边形参与了更新
                                    gg[j].Childrens[jj].ParentObj = parentGO;
                                    gg[j].Childrens[jj].RingInParent = RingInParentOfParentNotInGg;
                                    gg[j].Childrens[jj] = null;
                                }
                            }
                            //ii)第二步处理内环
                            for (int kk = 0; kk < gg[j].CurrentPolygon.InteriorRings.Count; kk++)  //将gg[j]中剩余内环加入到结果多边形中
                            {
                                if (gg[j].CurrentPolygon.InteriorRings[kk] == null)
                                    continue;
                                if (resultPs.Count == 1 || k == resultPs.Count - 1)//只用一个或最后一个
                                {
                                    resultP.CurrentPolygon.InteriorRings.Add(gg[j].CurrentPolygon.InteriorRings[kk]);
                                    gg[j].CurrentPolygon.InteriorRings[kk] = null;
                                    continue;
                                }
                                if (!resultP.CurrentPolygon.ExteriorRing.Bound.Contains(gg[j].CurrentPolygon.InteriorRings[kk].Bound))
                                    continue;
                                if (RingInParents.Contains(gg[j].CurrentPolygon.InteriorRings[kk]))
                                {
                                    gg[j].CurrentPolygon.InteriorRings[kk] = null;
                                    continue;
                                }
                                if (GeoAlgorithm.IsInLinearRingWHS(gg[j].CurrentPolygon.InteriorRings[kk].Vertices[0], resultP.CurrentPolygon.ExteriorRing) >= 1)
                                {
                                    resultP.CurrentPolygon.InteriorRings.Add(gg[j].CurrentPolygon.InteriorRings[kk]);
                                    gg[j].CurrentPolygon.InteriorRings[kk] = null;
                                }
                                //GeoPolygon p = new GeoPolygon(resultP.CurrentPolygon.ExteriorRing);
                                //OSGeo.OGR.Geometry ogr_polygon = GeometryConverter.GeometricToOGR(p as Geometries.Geometry);
                                //OSGeo.OGR.Geometry ogr_Point = GeometryConverter.GeometricToOGR(gg[j].CurrentPolygon.InteriorRings[kk].Vertices[0] as Geometries.Geometry);
                                //if (ogr_Point.Distance(ogr_polygon) < Geometries.Geometry.EPSIONAL)
                                //{
                                //    resultP.CurrentPolygon.InteriorRings.Add(gg[j].CurrentPolygon.InteriorRings[kk]);
                                //    gg[j].CurrentPolygon.InteriorRings[kk] = null;
                                //}
                            }
                            //将获得的多边形resultP插入到索引树中
                            m_map.QuadIndex.Insert(resultP);
                        }//endOfFor 更新后多边形resultPs
                    }
                    else//父多边形在gg中
                    {
                        //修正父多边形可能被更新为多个多边形，即resultPs有多个元素，设置两个变量：flagParentInGg标识父多边形真正在gg中（图边更新时，可能gg更新后多边形父多边形不在gg中），pk标识当flagParentInGg为真时，父多边形位置
                        bool flagParentInGg = false;
                        int pk = -1;//存储resultPs中父多边形的位置
                        if (j == 0)
                        {
                            for (int rk = 0; rk < resultPs.Count; rk++)
                            {
                                if (resultPs[rk].InteriorRings.Count > 0)
                                {
                                    flagParentInGg = true;
                                    pk = rk;
                                    break;
                                }
                            }
                            if (flagParentInGg)//先求出gg更新后多边形的父多边形及其RingInparent
                            {
                                //GeoDataRow r = rr.Clone();
                                GeoDataRow r = srcLayer.DataTable.NewRow();
                                r["PlgID"] = r["FID"];
                                r.Geometry = resultPs[pk] as Geometries.Geometry;
                                srcLayer.DataTable.AddRow(r);
                                GeoObjects resultP = new GeoObjects(r);
                                resultP.ParentObj = gg[j].ParentObj;         //从原多边形处继承父多边形
                                resultP.RingInParent = gg[j].RingInParent;
                                if (resultP.ParentObj != null)
                                    resultP.ParentObj.Childrens.Add(resultP);
                                parentGO = resultP;
                                RingInParentOfParentInGg = resultP.CurrentPolygon.InteriorRings[0];//此时若j==0则裁剪后只有一个内环
                                
                            }
                            else//图边情形
                            {
                                parentGO = null;
                                RingInParentOfParentInGg = null;
                            }
                        }
                        for (int k = 0; k < resultPs.Count; k++)
                        {
                            GeoObjects resultP = null;
                            if (j != 0 || (j == 0 && k != pk))
                            {
                                //生成新的结果对象
                                //GeoDataRow r = rr.Clone();
                                GeoDataRow r = srcLayer.DataTable.NewRow();
                                r["PlgID"] = r["FID"];
                                r.Geometry = resultPs[k] as Geometries.Geometry;
                                srcLayer.DataTable.AddRow(r);
                                resultP = new GeoObjects(r);
                                resultP.ParentObj = parentGO;         //从原多边形处继承父多边形
                                resultP.RingInParent = RingInParentOfParentInGg;
                                if (parentGO != null)
                                    parentGO.Childrens.Add(resultP);
                            }
                            else
                                resultP = parentGO;//当前多边形是父多边形k==pk

                            //i)第一步处理子多边形
                            List<GeoLinearRing> CommonRingInParent = new List<GeoLinearRing>();
                            for (int jj = 0; jj < gg[j].Childrens.Count(); jj++)
                            {
                                if (gg[j].Childrens[jj] == null)
                                {
                                    continue;
                                }
                                if (gg[j].Childrens[jj].RingInParent == null)//存在于gg中，其RingInParent已被置空
                                    continue;
                                bool isChild = true;
                                for (int kk = 0; kk < RingInParents.Count(); kk++)//RingInParents是参与更新的内环备份
                                {
                                    if (!RingInParents[kk].Bound.Contains(gg[j].Childrens[jj].RingInParent.Bound))//如果当前孩子结点的MBR与RingInParents[kk]的MBR不相交，则是孩子
                                    {
                                        continue;
                                    }
                                    if (GeoAlgorithm.IsInLinearRingWHS(gg[j].Childrens[jj].RingInParent.Vertices[0], RingInParents[kk]) >= 1)
                                    {
                                        isChild = false;
                                        break;
                                    }
                                    //GeoPolygon p = new GeoPolygon(RingInParents[kk]);                                    
                                    //OSGeo.OGR.Geometry ogr_Polygon = GeometryConverter.GeometricToOGR(p as Geometries.Geometry);
                                    //OSGeo.OGR.Geometry ogr_Point = GeometryConverter.GeometricToOGR(gg[j].Childrens[jj].RingInParent.Vertices[0] as Geometries.Geometry);
                                    //if (ogr_Point.Distance(ogr_Polygon) < Geometries.Geometry.EPSIONAL)//关于G多边形的判断需再优化，如何尽快找出G
                                    //{
                                    //    isChild = false;
                                    //    break;
                                    //}
                                }
                                if (isChild)//不能断定是当前resultP的孩子，也有可能是其他resultP的孩子！！！
                                {
                                    if (resultPs.Count == 1 || k == resultPs.Count - 1)//只有一个或最后一个
                                    {
                                        resultP.Childrens.Add(gg[j].Childrens[jj]);
                                        gg[j].Childrens[jj].ParentObj = resultP;
                                        gg[j].Childrens[jj] = null;
                                    }
                                    else 
                                    {
                                        if (GeoAlgorithm.IsInLinearRingWHS(gg[j].Childrens[jj].RingInParent.Vertices[0], resultP.CurrentPolygon.ExteriorRing) >= 1)
                                        {
                                            resultP.Childrens.Add(gg[j].Childrens[jj]);
                                            gg[j].Childrens[jj].ParentObj = resultP;
                                            gg[j].Childrens[jj] = null;
                                        }
                                        //GeoPolygon p = new GeoPolygon(resultP.CurrentPolygon.ExteriorRing);
                                        //OSGeo.OGR.Geometry ogr_Polygon = GeometryConverter.GeometricToOGR(p as Geometries.Geometry);
                                        //OSGeo.OGR.Geometry ogr_Point = GeometryConverter.GeometricToOGR(gg[j].Childrens[jj].RingInParent.Vertices[0] as Geometries.Geometry);
                                        //if (ogr_Point.Distance(ogr_Polygon) < Geometries.Geometry.EPSIONAL)
                                        //{
                                        //    resultP.Childrens.Add(gg[j].Childrens[jj]);
                                        //    gg[j].Childrens[jj].ParentObj = resultP;
                                        //    gg[j].Childrens[jj] = null;
                                        //}
                                    }
                                }
                                else
                                {
                                    if (parentGO != null)
                                        parentGO.Childrens.Add(gg[j].Childrens[jj]);//如果当前子多边形的RingInParent参与了更新，则该多边形是ParentGO的子多边形
                                    //必存在与该多边形具有共同RingInParent的多边形参与了更新
                                    gg[j].Childrens[jj].ParentObj = parentGO;
                                    gg[j].Childrens[jj].RingInParent = RingInParentOfParentInGg;
                                    gg[j].Childrens[jj] = null;
                                }
                            }
                            //ii)第二步处理内环
                            for (int kk = 0; kk < gg[j].CurrentPolygon.InteriorRings.Count; kk++)  //将gg[j]中剩余内环加入到结果多边形中
                            {
                                if (gg[j].CurrentPolygon.InteriorRings[kk] == null)
                                    continue;
                                if (resultPs.Count == 1 || k == resultPs.Count - 1)//只用一个或最后一个
                                {
                                    resultP.CurrentPolygon.InteriorRings.Add(gg[j].CurrentPolygon.InteriorRings[kk]);
                                    gg[j].CurrentPolygon.InteriorRings[kk] = null;
                                    continue;
                                }
                                if (!resultP.CurrentPolygon.ExteriorRing.Bound.Contains(gg[j].CurrentPolygon.InteriorRings[kk].Bound))
                                    continue;
                                if (RingInParents.Contains(gg[j].CurrentPolygon.InteriorRings[kk]))
                                {
                                    gg[j].CurrentPolygon.InteriorRings[kk] = null;
                                    continue;
                                }
                                if (GeoAlgorithm.IsInLinearRingWHS(gg[j].CurrentPolygon.InteriorRings[kk].Vertices[0], resultP.CurrentPolygon.ExteriorRing) >= 1)
                                {
                                    resultP.CurrentPolygon.InteriorRings.Add(gg[j].CurrentPolygon.InteriorRings[kk]);
                                    gg[j].CurrentPolygon.InteriorRings[kk] = null;
                                }
                                //GeoPolygon p = new GeoPolygon(resultP.CurrentPolygon.ExteriorRing);
                                //OSGeo.OGR.Geometry ogr_polygon = GeometryConverter.GeometricToOGR(p as Geometries.Geometry);
                                //OSGeo.OGR.Geometry ogr_Point = GeometryConverter.GeometricToOGR(gg[j].CurrentPolygon.InteriorRings[kk].Vertices[0] as Geometries.Geometry);
                                //if (ogr_Point.Distance(ogr_polygon) < Geometries.Geometry.EPSIONAL)
                                //{
                                //    resultP.CurrentPolygon.InteriorRings.Add(gg[j].CurrentPolygon.InteriorRings[kk]);
                                //    gg[j].CurrentPolygon.InteriorRings[kk] = null;
                                //}
                            }
                            //将获得的多边形resultP插入到索引树中
                            m_map.QuadIndex.Insert(resultP);
                        }//endOfFor 更新后多边形resultPs
                    }
                    if (resultPs.Count > 0)
                        resultPs.Clear();
                    if (RingInParents.Count() > 0)
                        RingInParents.Clear();//将存储的内环清空
                }//enfOfFor对gg逐个进行更新
                for (int jj = 0; jj < gg.Count; jj++)
                {
                    gg[jj].Dispose();
                }
                if (i % 100 == 0)
                    GC.Collect();
                //GeoDataRow ir = srcLayer.DataTable[srcLayer.DataTable.Count-1].Clone();
                GeoDataRow ir = srcLayer.DataTable.NewRow();
                ir.Geometry = incLayer.DataTable[i].Geometry;
                ir["PlgID"] = srcLayer.DataTable.Count;
                srcLayer.DataTable.AddRow(ir);   //将增量加入
                GeoObjects srcObj = new GeoObjects(ir);
                srcObj.ParentObj = parentGO;
                if(parentGO!=null)
                    parentGO.Childrens.Add(srcObj);
                if (boolParentInGg)
                    srcObj.RingInParent = RingInParentOfParentInGg;
                else
                    srcObj.RingInParent = RingInParentOfParentNotInGg;
                parentGO = null;
                RingInParentOfParentInGg = null;
                RingInParentOfParentNotInGg = null;
                m_map.QuadIndex.Insert(srcObj);
            }
            #endregion

            //DateTime startTimeMaintainPCn = DateTime.Now;
            //TimeSpan timen = new TimeSpan();
            //DateTime stopTimeMaintainPCn = DateTime.Now;
            //timen += stopTimeMaintainPCn - startTimeMaintainPCn;
            //MessageBox.Show("IntersectQuery  " + timen.ToString());

            #region 测试时间

            //2017-8-9测试时间
            DateTime stopTime = DateTime.Now;
            TimeSpan elapsedTime = stopTime - startTime;
            OutPutText("Elapsed: " + elapsedTime);
            OutPutText("in hours: " + elapsedTime.TotalHours);
            OutPutText("in minutes: " + elapsedTime.TotalMinutes);
            OutPutText("in seconds: " + elapsedTime.TotalSeconds);
            //2017-8-9测试时间            
            #endregion            

            incLayer.Dispose();
            srcLayer.Dispose();            
        }

        #region 创建索引树 zh编写 2018年3月8号
        public void CreateIndexTree(OracleConnection myConnection, QuadTreeNode root, int child, int f, List<GeoObjects> geoObjects, int[] PlgIDToFID)
        {
            //读取quadtreenode表，建立索引节点
            QuadTreeNode node = new QuadTreeNode();
            switch (f)
            { 
                case 1:
                    root.ChildNW = node;
                    node.PNode = root;
                    break;
                case 2:
                    root.ChildNE = node;
                    node.PNode = root;
                    break;
                case 3:
                    root.ChildSW = node;
                    node.PNode = root;
                    break;
                case 4:
                    root.ChildSE = node;
                    node.PNode = root;
                    break;
                default:
                    break;
            }

            string query = String.Format("select * from quadtreenode where M_ID={0}", child);
            OracleCommand cmd = new OracleCommand(query, myConnection);
            OracleDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了  
            {
                node.ID = child;
                node.NodeType = int.Parse(dataReader.GetOracleValue(1).ToString());
                node.Depth = int.Parse(dataReader.GetOracleValue(2).ToString());
                string mbound = dataReader.GetOracleValue(8).ToString().Trim();
                if (!mbound.Equals(""))
                {
                    node.mBound = new GeoBound(mbound);
                }
                string obound = dataReader.GetOracleValue(9).ToString().Trim();
                if (!mbound.Equals(""))
                {
                    node.mBound = new GeoBound(obound);
                }

                List<GeoObjects> objects=new List<GeoObjects>();
                node.m_objList = objects;
                string oblist = dataReader.GetOracleValue(10).ToString().Trim();
                string[] m_oblist = oblist.Split(',');
                for (int i = 0; i < m_oblist.Length; i++)
                {
                    //string myquery = String.Format("select * from geoobjects where PLGID={0}", m_oblist[i]);
                    //OracleCommand mycmd = new OracleCommand(query, myConnection);
                    //OracleDataReader mydataReader = mycmd.ExecuteReader();
                    //if (mydataReader.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了
                    //{
                    //    objects.Add(geoObjects[PlgIDToFID[int.Parse(m_oblist[i])]]);//读取m_oblist字段
                    //}
                    objects.Add(geoObjects[PlgIDToFID[int.Parse(m_oblist[i])]]);//读取m_oblist字段
                }

                int childnw = int.Parse(dataReader.GetOracleValue(3).ToString());
                int childne = int.Parse(dataReader.GetOracleValue(4).ToString());
                int childsw = int.Parse(dataReader.GetOracleValue(5).ToString());
                int childse = int.Parse(dataReader.GetOracleValue(6).ToString());
                if (childnw != -1)
                {
                    CreateIndexTree(myConnection, node, childnw, 1, geoObjects, PlgIDToFID);
                }
                if (childne != -1)
                {
                    CreateIndexTree(myConnection, node, childne, 2, geoObjects, PlgIDToFID);
                }
                if (childsw != -1)
                {
                    CreateIndexTree(myConnection, node, childsw, 3, geoObjects, PlgIDToFID);
                }
                if (childse != -1)
                {
                    CreateIndexTree(myConnection, node, childse, 4, geoObjects, PlgIDToFID);
                }
            }
        }
        #endregion







        /// <summary>
        /// gg为一组构成连续区域的多边形，由于这一片连续空间，所以这组多边形的父多边形
        /// 要么是其中一个多边形（这个多边形包含其他多边形），要么被一个更大的多边形包含（父多边形不在gg中）（或为空，即父多边形为制图区域）
        /// 算法说明：
        /// flg数组记录gg中多边形是父多边形是否中当前gg中，经过双重循环可得；
        /// 若flg中1的个数只有1个（设只有gg中的多边形i的父多边形不在gg中），说明gg中所有多边形被包含中多边形i中；
        /// 若flg中1的个数不只1个（设为i,j...），说明gg中多边形i,j,...有共同的父多边形，取i的父多边形即可
        /// </summary>
        /// <param name="gg"></param>
        /// <returns>父多边形</returns>
        private GeoObjects GetParentGO(List<GeoObjects> gg,ref bool boolParentInGg)
        {
            int n = gg.Count();
            int[] flg = new int[n];//标记数组，标记多边形的父多边形是否在当前gg中，如果在记为0，否则记1
            for (int i = 0; i < n; i++)
            {
                bool flag = false;
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                        continue;
                    if (gg[i].ParentObj == gg[j])
                    {
                        flag = true;
                        break;
                    }                    
                }
                if (flag)
                    flg[i] = 0;
                else
                    flg[i] = 1;

            }
            GeoObjects ParentGO = null;
            int pn = 0;//计父多边形不在gg中的多边形的个数
            int ip = 0;//若父多边形不在gg中的多边形只有一个，则记录下其位置
            for (int i = 0; i < n; i++)
            {
                if (flg[i] == 1)
                {
                    ParentGO = gg[i];
                    boolParentInGg = true;
                    ip = i;
                    pn++;
                }
                if (pn > 1)//父多边形不在gg中，是更大的外层多边形
                {
                    ParentGO = gg[i].ParentObj;
                    boolParentInGg = false;
                    break;
                }
            }
            if (boolParentInGg)//若父多边形不在gg中的多边形只有一个，则将其交换至第一个
            {
                GeoObjects temp = gg[ip];
                gg[ip] = gg[0];
                gg[0] = temp;
            }
            flg = null;
            return ParentGO;
        }
        /// <summary>
        /// 测试在table中删除一个对象，表索引的变化
        /// </summary>
        private void test(GeoVectorLayer srcLayer, GeoVectorLayer incLayer)
        {
            List<GeoPolygon> gout = new List<GeoPolygon>();
            int si = srcLayer.DataTable.Count;
            for (int i = 0; i < si; i++)//srcLayer.DataTable.Count
            {
                //if (i != 3)
                //    continue;
                List<GeoPolygon> gin = new List<GeoPolygon>();   
                gin.Add(srcLayer.DataTable[i].Geometry as GeoPolygon);
                for (int j = 0; j < incLayer.DataTable.Count; j++)
                {
                    GeoPolygon inc = incLayer.DataTable[j].Geometry as GeoPolygon;
                    int s = gin.Count;
                    for (int t = 0; t < s; t++)
                    {
                        bool bbb=GeometryConverter.GeometricToOGR(gin[t]).Intersect(GeometryConverter.GeometricToOGR(inc));
                        if(bbb)
                        {
                            PolygonSubtractPolygon.PolygonDifference(gin[t], inc, gout);
                            gin.RemoveAt(t);
                            int n = gout.Count;
                            for (int k = 0; k < n; k++)
                            {
                                gin.Add(gout[k]);
                            }
                            gout.Clear();
                        }
                    }
                }
                srcLayer.DataTable[i].Delete();
                i--; si--;//删除DataTable中元素，回退一个位置   //可以考虑在底图层从后向前操作
                int a=gin.Count;
                for (int k = 0; k < a; k++)
                {
                    GeoDataRow r = srcLayer.DataTable.NewRow();
                    r.Geometry = gin[k] as GIS.Geometries.Geometry;
                    srcLayer.DataTable.AddRow(r);
                }
                gin = null; 
            }
        }

        /////<summary>
        /////测试从DataTable中删除一行，测试在指定位置插入一行private void test(GeoVectorLayer srcLayer)
        /////</summary>
        //private void test(GeoVectorLayer srcLayer)
        //{
        //    string s1 = srcLayer.DataTable.Rows[1]["FID"].ToString();
        //    string s2 = srcLayer.DataTable.Rows[1]["PlgAttr"].ToString();
        //    string s3 = srcLayer.DataTable.Rows[1]["PlgID"].ToString();            
        //    GeoData.GeoDataRow r = srcLayer.DataTable.Rows[1] as GeoData.GeoDataRow;
        //    srcLayer.DataTable.Rows[1].Delete();
        //    srcLayer.DataTable.AcceptChanges();
        //    srcLayer.DataTable.Rows.InsertAt(r, 2);
        //    srcLayer.DataTable.Rows[2]["FID"] = s1 as Object;
        //    srcLayer.DataTable.Rows[2]["PlgAttr"] = s2 as Object;
        //    srcLayer.DataTable.Rows[2]["PlgID"] = s3 as Object;
        //}

        //private void ProcessGeometryToArray(OSGeo.OGR.Geometry gg, List<OSGeo.OGR.Geometry> plgArr)
        //{
        //    if (gg.GetArea() < 10.0)
        //        return;
        //    if (gg.GetGeometryType() == wkbGeometryType.wkbGeometryCollection)  //如果是集合，则转多多边形
        //    {
        //        OSGeo.OGR.Geometry temp = new Geometry(wkbGeometryType.wkbMultiPolygon);
        //        int n = gg.GetGeometryCount();
        //        for (int k = 0; k < n; k++)
        //        {
        //            OSGeo.OGR.Geometry gt = new Geometry(wkbGeometryType.wkbPolygon);
        //            gt = gg.GetGeometryRef(k);
        //            if (gt.GetArea() > 10.0)
        //            {
        //                temp.AddGeometry(gt);
        //            }
        //        }
        //        gg = temp;
        //    }
        //    int plgCount = gg.GetGeometryCount();
        //    for (int i = 0; i < plgCount; i++)
        //    {
        //        OSGeo.OGR.Geometry tmp = new Geometry(wkbGeometryType.wkbPolygon);
        //        tmp = gg.GetGeometryRef(i);
        //        if (tmp.GetArea() > 10.0)
        //            plgArr.Add(tmp);
        //    }
        //}


        #region 作废
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
        #endregion

        private bool EnvelopeIntersets(OSGeo.OGR.Geometry g1, OSGeo.OGR.Geometry g2)
        {
            OSGeo.OGR.Envelope env1 = new Envelope();
            OSGeo.OGR.Envelope env2 = new Envelope();

            g1.GetEnvelope(env1);
            g2.GetEnvelope(env2);

            return env1.MinX < env2.MaxX && env1.MaxX > env2.MinX &&
                   env1.MinY < env2.MaxY && env1.MaxY > env2.MinY;
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
        private void DescriptionIncrementNumber(string text)
        {
             m_frmProgressBar.incrNumber.Invoke(
                (MethodInvoker)delegate()
                {
                    m_frmProgressBar.incrNumber.Text = text;
                }
                );
        }
        private void RefreshUpdatingProcess(string text)
        {
            m_frmProgressBar.procNumber.Invoke(
                (MethodInvoker)delegate()
                {
                    m_frmProgressBar.procNumber.Text = text;
                    m_frmProgressBar.procNumber.Refresh();
                }
                );
        }
        private void RefreshSrcNumber(string text)
        {
            m_frmProgressBar.srcNumber.Invoke(
                (MethodInvoker)delegate()
                {
                    m_frmProgressBar.srcNumber.Text = text;
                    m_frmProgressBar.srcNumber.Refresh();
                }
                );
        }
    }
}
