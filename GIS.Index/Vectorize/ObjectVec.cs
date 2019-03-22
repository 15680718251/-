using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using OSGeo.OGR;
using OSGeo.GDAL;
using GIS.Geometries;
using GIS.Utilities;

namespace GIS.TreeIndex.Vectorize
{
    public class ObjectVec : IDisposable
    {
        private string strRasPathName;
        private string strfolderName;
        private Dataset m_GdalDataset;
        private DebugLog log;
        private int m_RastersCount;   //影像个数
        private DoWorkEventArgs e;
        private BackgroundWorker bw;
        private frmVectorizeProgress m_frmProgressBar;
        private Dictionary<short, string> m_dics;

        private double[] m_GeoTransform = new double[6];
        public int PlgIndex; //用于内外环跟踪时，将多边形当前个数，赋予相应的游程标记

        public Size m_ImageSize;
        private int m_BandCount;
        public List<List<Run>> m_ImageRun;
        public Bound ImageBound;   //栅格图像边界定义
        private int totalCount;

        //构造函数
        public ObjectVec(string str1, string str2,Dictionary<short, string> dics,BackgroundWorker bw,frmVectorizeProgress fm,DoWorkEventArgs args,int n)
        {
            strRasPathName = str1;
            strfolderName = str2;
            log=new DebugLog();
            this.m_RastersCount = n;  //个数

            this.e = args;
            this.m_dics = dics;
            this.bw = bw;
            this.m_frmProgressBar = fm;
            this.lastMaxValue = m_frmProgressBar.progressBar1.Value;
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
                if (disposing)
                    if (m_GdalDataset != null)
                    {
                        try
                        {
                            m_ImageRun.Clear();
                            m_GdalDataset.Dispose();
                        }
                        finally
                        {
                            m_ImageRun = null;
                            m_GdalDataset = null;
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                disposed = true;
            }
        }
        ~ObjectVec()
        {
            Dispose(false);
        }
        #endregion

        private void OpenRaster()
        {
            //是否取消
            if (CancelOrNot())
            {
                return;
            }

            Gdal.AllRegister();
            try
            {
                log.WriteLog("打开影像："+strRasPathName);

                m_frmProgressBar.lblDescription.Invoke(
                    (MethodInvoker)delegate()
                    {
                        m_frmProgressBar.lblDescription.Text = "当前处理影像："+ System.IO.Path.GetFileName(strRasPathName);
                    }
                    );

                OutPutText("打开影像："+strRasPathName);

                m_GdalDataset = Gdal.Open(strRasPathName, Access.GA_ReadOnly);
                m_BandCount = m_GdalDataset.RasterCount;
                if (m_BandCount != 1)
                {
                    MessageBox.Show("影像:" + strRasPathName + "波段数不正确！", "提示");
                    log.WriteLog("影像:" + strRasPathName + "波段数不正确！");
                    return;
                }
                m_ImageSize = new Size(m_GdalDataset.RasterXSize, m_GdalDataset.RasterYSize);
                totalCount = m_GdalDataset.RasterXSize * m_GdalDataset.RasterYSize;  //总像素个数
                m_GdalDataset.GetGeoTransform(m_GeoTransform);
            }
            catch (Exception ex)   //这里是只要上面try有空指针就，捕获错误的
            {
                m_GdalDataset = null;
                //throw new Exception("Couldn't load " + strRasPathName + "\n\n" + ex.Message + ex.InnerException);
                MessageBox.Show(ex.Message,"警告");
                log.WriteLog("影像:"+strRasPathName+"打开失败！");
                return;
            }
        }

        private void TransformRasterToRLE()
        {

            //是否取消
            if (CancelOrNot())
            {
                return;
            }

            ImageBound = new Bound();
            ImageBound.MinRow = 0;
            ImageBound.MaxRow = m_ImageSize.Height;
            ImageBound.MinColumn = 0;
            ImageBound.MaxColumn = m_ImageSize.Width;

            m_ImageRun = new List<List<Run>>();
            Band band = m_GdalDataset.GetRasterBand(1); //单波段

            try
            {
                byte[] Buffer = new byte[totalCount];   //此处开辟连续的内存块，注意内存溢出的错误
                band.ReadRaster(0, 0, m_ImageSize.Width, m_ImageSize.Height, Buffer, m_ImageSize.Width, m_ImageSize.Height, 0, 0);

                OutPutText("开始扫描影像: "+strRasPathName);
                log.WriteLog("开始扫描影像: " + strRasPathName);
                for (int y = 0; y < m_GdalDataset.RasterYSize; y++)
                {
                    Run TempRun1 = new Run();
                    List<Run> m_RoWRun = new List<Run>();
                    TempRun1.Attr = Buffer[y * m_GdalDataset.RasterXSize];
                    TempRun1.Start = 0;
                    TempRun1.LeftFlag = -1;
                    TempRun1.RightFlag = -1;
                    m_RoWRun.Add(TempRun1);

                    for (int x = 0; x < m_GdalDataset.RasterXSize - 1; x++)
                    {
                        while (Buffer[x + y * m_GdalDataset.RasterXSize] != Buffer[x + 1 + y * m_GdalDataset.RasterXSize])
                        {
                            Run TempRun2 = new Run();
                            TempRun2.Attr = Buffer[x + 1 + y * m_GdalDataset.RasterXSize];
                            TempRun2.Start = x + 1;
                            TempRun2.LeftFlag = -1;
                            TempRun2.RightFlag = -1;
                            m_RoWRun.Add(TempRun2);
                            break;
                        }

                        //是否取消
                        if (CancelOrNot())
                        {
                            return;
                        }
                    }
                    m_ImageRun.Add(m_RoWRun);

                    int step = Convert.ToInt32((y * 1000) / (2 * m_RastersCount * m_GdalDataset.RasterYSize));
                    
                    SetProgressBar(step);
                }

                Buffer = null;  //批量处理时，利用释放内存
                OutPutText("扫描完成");
                log.WriteLog("影像: " + strRasPathName+"扫描完成！");
            }
            catch(Exception ex)
            {
                m_GdalDataset.Dispose(); 
                log.WriteLog("内存中连续的内存块不够，内存溢出！");
                m_frmProgressBar.Cancel = true;
                return;
            }
        }

        public void RaToVe()
        {
            OpenRaster();
            TransformRasterToRLE();  //扫描影像
            ExtractionVecPolygon(/*ref FileCount */); //提取多边形
        }

        OSGeo.OGR.Driver poDriver;
        DataSource poDS;
        OSGeo.OGR.Layer poLayer;
        private void ExtractionVecPolygon(/* ref int FileCount */)
        {

            if (CancelOrNot())
            {
                return;
            }

            OutPutText("开始提取多边形...");
            log.WriteLog("开始提取多边形...");

            try
            {
                TurnParam Turn = new TurnParam();
                PlgIndex = -1;         //多边形个数的索引号如果为0，代表还有一个多边形
                bool FileMove = false; //文件批量矢量化，是否内圈有超过3万个的，有则直接移走文件，删除已矢量化的部分文件

                #region 创建矢量文件
                string strFileName = System.IO.Path.GetFileNameWithoutExtension(strRasPathName) + "vec.shp";
                OSGeo.OGR.Ogr.RegisterAll();//注册所有驱动
                //OSGeo.GDAL.Gdal.GetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");

                string pszDriverName = "ESRI Shapefile";
                poDriver = Ogr.GetDriverByName(pszDriverName);//创建SHAPEFILE 文件驱动

                if (poDriver == null)
                {
                    log.WriteLog("创建矢量驱动失败");
                    return;
                }

                string SHPpathName = strfolderName + "\\" + strFileName;
                string lyrName = System.IO.Path.GetFileNameWithoutExtension(SHPpathName);
                poDS = poDriver.CreateDataSource(strfolderName, null);

                if (poDS == null)
                {
                    log.WriteLog("创建矢量文件：" + strFileName + "失败");
                    return;
                }

                wkbGeometryType gmttype = wkbGeometryType.wkbPolygon;
                string m_lyrName = lyrName;

                string strwkt = m_GdalDataset.GetProjectionRef();  //栅格数据的投影和坐标信息
                OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(strwkt);
                poLayer = poDS.CreateLayer(m_lyrName, srs, gmttype, null);
                OSGeo.OGR.FieldDefn fd = new FieldDefn("PlgAttr", FieldType.OFTString);
                poLayer.CreateField(fd, 0);

                OutPutText("创建矢量文件：" + strFileName);
                log.WriteLog("创建矢量文件：" + strFileName);
                #endregion

                #region 所有多边形提取和写入shape
                for (int i = m_ImageSize.Height - 1; i >= 0; i--)
                {
                    for (int j = 0; j < m_ImageRun[i].Count; j++)
                    {
                        short RingAttr = m_ImageRun[i][j].Attr;
                        if (!m_dics.ContainsKey(RingAttr))
                            continue;


                        #region 提取一个多边形  包括提取内环和外环
                        int Imin = -1, Imax = -1/*, Jmin = -1, Jmax = -1*/;
                        //记录当前多边形到达的最大行,用于限定内圈搜索的范围,但J不太方便记录,因为是游程
                        //最大行，就是起始行，最小行，只在Up和Down函数改变，但比较复杂
                        if (m_ImageRun[i][j].LeftFlag == -1)
                        {
                            OSGeo.OGR.Feature poFeature = new OSGeo.OGR.Feature(poLayer.GetLayerDefn());
                            OSGeo.OGR.Geometry ogrgtr = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);     //几何
                            OSGeo.OGR.Geometry ogrexter = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);//外环

                            # region 提取外环
                            Imin = i; //初始化
                            Imax = i; //当前多边形用到的最大行,因为外圈开始追踪时就是第一个起点

                            PlgIndex++;

                            m_ImageRun[i][j].LeftFlag = PlgIndex; //标记用于哪个多边形:起始游程的标记这是,然后

                            GeoPoint PtOut = new GeoPoint();
                            PtOut.X = m_ImageRun[i][j].Start;
                            PtOut.Y = m_ImageSize.Height - (i + 1);
                            Turn.SetField(DirectionTypes.DT_UP, i, j, (int)PtOut.X);
                            GeoTransformPt(PtOut);
                            ogrexter.AddPoint_2D(PtOut.X, PtOut.Y);  //写入外环

                            bool OneRingOutOver = false;
                            while (!OneRingOutOver)  //直到外环提取完成
                            {
                                if (Turn.Direction == DirectionTypes.DT_UP)
                                {
                                    OneRingOutOver = Up(ref ogrexter, ref Turn);

                                    if (Turn.ZeroRow < Imin) //当前0格点所在的行
                                        Imin = Turn.ZeroRow;
                                }
                                else if (Turn.Direction == DirectionTypes.DT_DOWN)
                                {
                                    OneRingOutOver = Down(ref ogrexter, ref Turn);
                                }
                                else if (Turn.Direction == DirectionTypes.DT_LEFT)
                                {
                                    OneRingOutOver = Left(ref ogrexter, ref Turn);
                                }
                                else if (Turn.Direction == DirectionTypes.DT_RIGHT)
                                {
                                    OneRingOutOver = Right(ref ogrexter, ref Turn);
                                }
                            }
                            ogrgtr.AddGeometry(ogrexter); //外圈建立完毕
                            # endregion

                            #region 所有内环提取
                            int Count = 0;
                            for (int i1 = Imax; i1 >= Imin; i1--)        //此处应优化：K>= 当前多边形最上方的栅格行号即可
                            {
                                for (int j1 = 0; j1 < m_ImageRun[i1].Count; j1++)
                                {
                                    //用m_ImageRun[i1][j1].LeftFlag == PlgIndex，而不是-1,是为了更精确和省时间，因为不用管LeftFlag不是-1，是其他值，然后继续判断RightFlag的情况了
                                    //这里应该用if而不是 while
                                    while (m_ImageRun[i1][j1].LeftFlag == PlgIndex && m_ImageRun[i1][j1].RightFlag == -1)
                                    {
                                        OSGeo.OGR.Geometry ogrinter = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);//多边形一个内圈创建

                                        m_ImageRun[i1][j1].RightFlag = PlgIndex; //标记用于哪个多边形

                                        GeoPoint PtIn = new GeoPoint();
                                        if ((j1 + 1) < m_ImageRun[i1].Count && (i1 + 1) < ImageBound.MaxRow) //判断是否出界
                                        {
                                            int ZeroStart = m_ImageRun[i1][j1 + 1].Start;  //和当前的3格点所在列一致
                                            int ZeroAttr = -1;
                                            int Zeroj = -1;

                                            #region 查找格点0所在行第几游程，以及其属性
                                            for (int k = 0; k < m_ImageRun[i1 + 1].Count - 1; k++)
                                            {
                                                if (ZeroStart >= m_ImageRun[i1 + 1][k].Start && ZeroStart < m_ImageRun[i1 + 1][k + 1].Start)
                                                {
                                                    ZeroAttr = m_ImageRun[i1 + 1][k].Attr;
                                                    Zeroj = k;
                                                    break;
                                                }
                                            }
                                            if (ZeroAttr == -1)
                                            {
                                                ZeroAttr = m_ImageRun[i1 + 1][m_ImageRun[i1 + 1].Count - 1].Attr;
                                                Zeroj = m_ImageRun[i1 + 1].Count - 1;
                                            }
                                            #endregion

                                            if (ZeroAttr != m_ImageRun[i][j].Attr)   //应该用i,j 没错
                                            {
                                                //MessageBox.Show("内圈属性与外圈属性不等，出错");
                                                log.WriteLog("内圈属性与外圈属性不等，出错");
                                                return;
                                            }
                                            PtIn.X = m_ImageRun[i1][j1 + 1].Start;  //判断，如果越界，则取最后一列值
                                            PtIn.Y = m_ImageSize.Height - (i1 + 1);

                                            #region shape结构内圈点加入
                                            GeoTransformPt(PtIn); //写入shape前要转换坐标系
                                            ogrinter.AddPoint_2D(PtIn.X, PtIn.Y);
                                            #endregion

                                            Turn.SetField(DirectionTypes.DT_RIGHT, i1 + 1, Zeroj, ZeroStart);

                                            bool OneRingInOver = false;

                                            while (!OneRingInOver)
                                            {
                                                if (Turn.Direction == DirectionTypes.DT_UP)
                                                {
                                                    OneRingInOver = Up(ref ogrinter, ref Turn);
                                                }
                                                else if (Turn.Direction == DirectionTypes.DT_DOWN)
                                                {
                                                    OneRingInOver = Down(ref ogrinter, ref Turn);
                                                }
                                                else if (Turn.Direction == DirectionTypes.DT_LEFT)
                                                {
                                                    OneRingInOver = Left(ref ogrinter, ref Turn);
                                                }
                                                else if (Turn.Direction == DirectionTypes.DT_RIGHT)
                                                {
                                                    OneRingInOver = Right(ref ogrinter, ref Turn);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            PtIn.X = ImageBound.MaxColumn; //算法问题：向右已没有任何游程，如何处理？-不可能，一定会有的，因为还没有标记，必定存在当前游程的内圈，否则在提取外圈时就会标记右标记
                                            //MessageBox.Show("内圈找到入口点，却向右到了边界，出错");
                                            log.WriteLog("内圈找到入口点，却向右到了边界，出错");
                                            return;               //这个是错误的，正确标记了游程，不应存在此问题，解决---
                                        }

                                        ogrgtr.AddGeometry(ogrinter);//内圈建立完毕
                                        Count++;   //输出有内圈的多边形
                                    }
                                }
                            }
                            #endregion

                            poFeature.SetGeometry(ogrgtr);
                            poFeature.SetField(0, m_dics[RingAttr]);
                            poLayer.CreateFeature(poFeature);
                            poFeature.Dispose();

                        }
                        # endregion

                        if (CancelOrNot())
                        {
                            return;
                        }
                    }

                    int step = Convert.ToInt32(((m_ImageSize.Height-i) * 1000) / (2 * m_RastersCount * m_ImageSize.Height));
                    SetProgressBar(step);
                }

                log.WriteLog("影像：" + System.IO.Path.GetFileName(strRasPathName) + "矢量化完成");
                log.WriteLog("------------------------------------------");
                OutPutText("影像：" + System.IO.Path.GetFileName(strRasPathName)+"矢量化完成");
                OutPutText("");
                #endregion
            }
            catch (Exception ex)
            {
                log.WriteLog(ex.ToString());
            }
            finally
            {
                poLayer.Dispose();
                poDS.Dispose();
                poDriver.Dispose();
            }
        }

        #region  Up Down Left Right函数

        /// <summary>
        /// Up:向上跟踪一步：步长为1个像素
        /// </summary>
        ///params：组成多边形的点集；0格点所在游程行数i, 所在行中的第j游程,所在的栅格列数
        public bool Up(ref OSGeo.OGR.Geometry ogrter, ref TurnParam Turn)
        {
            #region 格点的栅格坐标
            //0格点：（i,m_ImageRun[i][j].Start）
            //1格点：（i-1,m_ImageRun[i][j].Start）
            //2格点：（i-1,m_ImageRun[i][j].Start-1）
            //4像素窗口中{右下角栅格行列号}与{该栅格左下角点矢量坐标对应关系}：
            //          栅格坐标[I,J]  ——>   矢量点[J, I+1]
            #endregion

            int i = Turn.ZeroRow;
            int j = Turn.ZeroRunColumn;

            int OneAttr = -1;
            int TwoAttr = -1;

            int ZeroStart = Turn.ZeroStart;
            int OneStart = ZeroStart;
            int TwoStart = OneStart - 1;

            int Onej = -1;
            int Twoj = -1;   //1,2格点所在行游程中第几游程

            #region 1、2格点边界越界控制
            int Onei = i - 1;
            int Twoi = i - 1;

            if (Onei < ImageBound.MinRow)    //即Onei=-1  行越界
            {
                OneAttr = -2;
                TwoAttr = -2;
            }
            else if (Onei >= ImageBound.MinRow && OneStart == ImageBound.MinColumn)
            {                                             //列越界
                OneAttr = m_ImageRun[i - 1][0].Attr;
                Onej = 0;
                TwoAttr = -2;
            }
            #endregion
            else //未越界情形，查询格点1和2属性，以及3属性--越界与未越界转向关系一起判断
            {
                #region 查找格点1属性
                for (int k = 0; k < m_ImageRun[i - 1].Count - 1; k++)
                {
                    if (OneStart >= m_ImageRun[i - 1][k].Start && OneStart < m_ImageRun[i - 1][k + 1].Start)
                    {
                        OneAttr = m_ImageRun[i - 1][k].Attr;
                        Onej = k;
                        break;
                    }
                }
                if (OneAttr == -1)
                {
                    OneAttr = m_ImageRun[i - 1][m_ImageRun[i - 1].Count - 1].Attr;
                    Onej = m_ImageRun[i - 1].Count - 1;
                }
                #endregion
            }

            //Point.X = PtList[PtList.Count - 1].X; //用上一点坐标表示，为加快算法执行，可直接用i,j表达
            //Point.Y = PtList[PtList.Count - 1].Y - 1; 
            GeoPoint Point = new GeoPoint();
            Point.X = OneStart;     //1格点栅格[I,J]  ——> 矢量[J, I+1]
            Point.Y = m_ImageSize.Height - (i - 1 + 1);    //加快算法：用OneStart而不是m_ImageRun[i][j].Start
            GeoTransformPt(Point); //写入shape前要转换坐标系 
            //:此处不转换，在判断是否回到起点时，将出错（起点已经转换了）,而且此处只需要一行代码，省的后面每个都要转换一次

            if (OneAttr != m_ImageRun[i][j].Attr)  //无需标记游程
            {
                #region 判断格点1与格点0属性异同关系
                if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                {
                    //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                    //PtList.Add(Point);                    //存储坐标
                    #region shape结构外圈点加入
                    ogrter.AddPoint_2D(Point.X, Point.Y);
                    #endregion
                    return true;
                }
                else
                {
                    //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                    //    PtList.Add(Point);                    //存储坐标
                    #region shape结构外圈点加入
                    ogrter.AddPoint_2D(Point.X, Point.Y);
                    #endregion
                    Turn.SetField(DirectionTypes.DT_RIGHT, i, j, ZeroStart); //向右
                    return false;
                }
                #endregion
            }
            else if (OneAttr == m_ImageRun[i][j].Attr)
            {
                if (TwoAttr != -2)
                {
                    #region 获取2格点属性
                    if (TwoStart >= m_ImageRun[i - 1][Onej].Start)
                    {
                        TwoAttr = m_ImageRun[i - 1][Onej].Attr;
                        Twoj = Onej;
                    }
                    else
                    {
                        TwoAttr = m_ImageRun[i - 1][Onej - 1].Attr;
                        Twoj = Onej - 1;
                    }
                    #endregion
                }

                #region 判断格点2与0格点属性异同关系
                if (TwoAttr != m_ImageRun[i][j].Attr)
                {
                    m_ImageRun[i - 1][Onej].LeftFlag = PlgIndex; //标记游程   应该加上如果已经为真，即抛出异常
                    //m_ImageRun[i - 1][Onej].SetLeftFlag(PlgIndex);

                    if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        return true;
                    }
                    else                                             //不存储坐标
                    {
                        Turn.SetField(DirectionTypes.DT_UP, i - 1, Onej, OneStart);//向上
                        return false;
                    }
                }
                else if (TwoAttr == m_ImageRun[i][j].Attr)
                {
                    if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        return true;
                    }
                    else
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        Turn.SetField(DirectionTypes.DT_LEFT, i - 1, Twoj, TwoStart);//向左
                        return false;
                    }
                }
                #endregion
            }
            return false;
        }

        /// <summary>
        /// 下一游程起始点来确定栅格点行列号
        /// </summary>
        public bool Down(ref OSGeo.OGR.Geometry ogrter, ref TurnParam Turn)
        {
            #region 格点的栅格坐标
            //0格点：（i,m_ImageRun[i][j+1].Start-1）
            //1格点：（i+1,m_ImageRun[i][j+1].Start-1）
            //2格点：（i+1,m_ImageRun[i][j+1].Start）
            #endregion
            
            int i = Turn.ZeroRow;
            int j = Turn.ZeroRunColumn;
            int ZeroStart = Turn.ZeroStart;
            int OneAttr = -1;
            int TwoAttr = -1;
            int OneStart = ZeroStart;
            int TwoStart = OneStart + 1;
            int Onej = -1;
            int Twoj = -1;                          //1,2格点所在行游程中第几游程

            #region 1、2边界越界控制
            int Onei = i + 1;
            int Twoi = i + 1;
            if (Onei > ImageBound.MaxRow - 1)    //即Onei=ImageBound.MaxRow
            {
                OneAttr = -2;
                TwoAttr = -2;
            }
            else if (Onei <= (ImageBound.MaxRow - 1) && OneStart == (ImageBound.MaxColumn - 1))
            {
                OneAttr = m_ImageRun[i + 1][m_ImageRun[i + 1].Count - 1].Attr;
                Onej = m_ImageRun[i + 1].Count - 1;
                TwoAttr = -2;
            }
            #endregion
            else  //未越界情形，查询格点1和2属性--越界与未越界转向关系一起判断
            {
                #region 查找格点1属性
                for (int k = 0; k < m_ImageRun[i + 1].Count - 1; k++)
                {
                    if (OneStart >= m_ImageRun[i + 1][k].Start && OneStart < m_ImageRun[i + 1][k + 1].Start)
                    {
                        OneAttr = m_ImageRun[i + 1][k].Attr;
                        Onej = k;
                        break;
                    }
                }
                if (OneAttr == -1)
                {
                    OneAttr = m_ImageRun[i + 1][m_ImageRun[i + 1].Count - 1].Attr;
                    Onej = m_ImageRun[i + 1].Count - 1;
                }
                #endregion
            }

            GeoPoint Point = new GeoPoint();
            Point.X = TwoStart;  //3格点栅格[I,J]  ——> 矢量[J, I+1]  3格点与2格点列值相同
            Point.Y = m_ImageSize.Height - (i + 1);
            GeoTransformPt(Point); //写入shape前要转换坐标系

            
            if (OneAttr != m_ImageRun[i][j].Attr)
            {
                #region 判断格点1与格点0属性异同关系
                if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                {
                    //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                    //    PtList.Add(Point);                         //存储坐标
                    #region shape结构外圈点加入
                    ogrter.AddPoint_2D(Point.X, Point.Y);
                    #endregion

                    return true;
                }
                else
                {
                    //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                    //    PtList.Add(Point);                         //存储坐标
                    #region shape结构外圈点加入
                    ogrter.AddPoint_2D(Point.X, Point.Y);
                    #endregion
                    Turn.SetField(DirectionTypes.DT_LEFT, i, j, ZeroStart);//向左
                    return false;
                }
                #endregion
            }
            else if (OneAttr == m_ImageRun[i][j].Attr)
            {
                if (TwoAttr != -2)
                {
                    #region 获取2格点属性
                    if ((Onej + 1) < (m_ImageRun[i + 1].Count))
                    {
                        if (TwoStart < m_ImageRun[i + 1][Onej + 1].Start)
                        {
                            TwoAttr = OneAttr;
                            Twoj = Onej;
                        }
                        else
                        {
                            TwoAttr = m_ImageRun[i + 1][Onej + 1].Attr;
                            Twoj = Onej + 1;
                        }
                    }
                    else
                    {
                        TwoAttr = OneAttr;
                        Twoj = Onej;
                    }
                    #endregion
                }

                #region 判断格点2与0格点属性异同关系
                if (TwoAttr != m_ImageRun[i][j].Attr)
                {
                    m_ImageRun[i + 1][Onej].RightFlag = PlgIndex;
                    //m_ImageRun[i + 1][Onej].SetRightFlag(PlgIndex);

                    if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        return true;
                    }
                    else                                             //不存储坐标
                    {
                        Turn.SetField(DirectionTypes.DT_DOWN, i + 1, Onej, OneStart);
                        return false;
                    }
                }
                else if (TwoAttr == m_ImageRun[i][j].Attr)
                {
                    if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        return true;
                    }
                    else
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        Turn.SetField(DirectionTypes.DT_RIGHT, i + 1, Twoj, TwoStart);
                        return false;
                    }
                }
                #endregion
            }
            return false;
        }

        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1] 4像素右下角栅格与该栅格左小角点坐标对应
        ///ChangeDirection:true时-- 前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
        ///         与当前0格点右边一个栅格坐标对应（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）
        ///ChangeDirection:false--前一矢量点坐标[X坐标减1，Y坐标不变],（PtList[PtList.Count-1].X-1,PtList[PtList.Count-1].Y）
        ///          后与当前0格点右边一个栅格坐标对应（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X-1）    
        public bool Left(ref OSGeo.OGR.Geometry ogrter, ref TurnParam Turn)
        {
            #region 格点的栅格坐标
            //changeDirection:true
            //0格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-1）    
            //1格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-1-1）
            //2格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-1-1）
            //changedirection:false
            //0格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-2）    
            //1格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-3）
            //2格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-3）
            #endregion
            int i = Turn.ZeroRow;
            int j = Turn.ZeroRunColumn;
            int ZeroStart = Turn.ZeroStart;
            int OneAttr = -1;
            int TwoAttr = -1;
            int OneStart = ZeroStart - 1;
            int TwoStart = OneStart;
            int Onej = -1;
            int Twoj = -1;                          //1,2格点所在行游程中第几游程

            #region 1、2、3边界越界控制
            int Onei = i;
            int Twoi = i + 1;
            if (Twoi > ImageBound.MaxRow - 1 && OneStart < ImageBound.MinColumn)
            {
                OneAttr = -2;                      //行、列越界
                TwoAttr = -2;                      //即Twoi=ImageBound.MaxRow  OneStart=-1
            }
            else if (Twoi > ImageBound.MaxRow - 1 && OneStart >= ImageBound.MinColumn)
            {
                TwoAttr = -2;                    //行越界
            }
            else if (Twoi <= (ImageBound.MaxRow - 1) && OneStart < ImageBound.MinColumn)
            {
                OneAttr = -2;                     //列越界
                TwoAttr = -2;
            }
            #endregion
            if (OneAttr != -2)
            {
                #region 获取1格点属性
                if (OneStart >= m_ImageRun[i][j].Start)
                {
                    OneAttr = m_ImageRun[i][j].Attr;
                    Onej = j;
                }
                else
                {
                    OneAttr = m_ImageRun[i][j - 1].Attr;  //j-1的一定存在了，已排除边界越界
                    Onej = j - 1;
                }
                #endregion
            }
            GeoPoint Point = new GeoPoint();
            Point.X = OneStart + 1;//0格点栅格[I,J]  ——> 矢量[J, I+1]
            Point.Y = m_ImageSize.Height - (i + 1);

            GeoTransformPt(Point); //写入shape前要转换坐标系
            if (OneAttr != m_ImageRun[i][j].Attr)
            {

                #region 判断格点1与0号格点的属性异同关系
                m_ImageRun[i][j].LeftFlag = PlgIndex;
                //m_ImageRun[i][j].SetLeftFlag(PlgIndex);

                if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                {
                    //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                    //    PtList.Add(Point);                    //存储坐标
                    #region shape结构外圈点加入
                    ogrter.AddPoint_2D(Point.X, Point.Y);
                    #endregion
                    return true;
                }
                else
                {
                    //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                    //    PtList.Add(Point);                    //存储坐标
                    #region shape结构外圈点加入
                    ogrter.AddPoint_2D(Point.X, Point.Y);
                    #endregion
                    Turn.SetField(DirectionTypes.DT_UP, i, j, ZeroStart);
                    return false;
                }
                #endregion
            }
            else if (OneAttr == m_ImageRun[i][j].Attr)
            {
                if (TwoAttr != -2)
                {
                    #region 查找2格点属性
                    for (int k2 = 0; k2 < m_ImageRun[i + 1].Count - 1; k2++)
                    {
                        if (TwoStart >= m_ImageRun[i + 1][k2].Start && TwoStart < m_ImageRun[i + 1][k2 + 1].Start)
                        {
                            TwoAttr = m_ImageRun[i + 1][k2].Attr;
                            Twoj = k2;
                            break;
                        }
                    }
                    if (TwoAttr == -1)
                    {
                        TwoAttr = m_ImageRun[i + 1][m_ImageRun[i + 1].Count - 1].Attr;
                        Twoj = m_ImageRun[i + 1].Count - 1;
                    }
                    #endregion
                }
                #region 判断格点2与0格点属性异同关系
                if (TwoAttr != m_ImageRun[i][j].Attr)
                {
                    if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        return true;
                    }
                    else
                    {
                        Turn.SetField(DirectionTypes.DT_LEFT, i, Onej, OneStart);
                        return false;
                    }
                }
                else if (TwoAttr == m_ImageRun[i][j].Attr)
                {
                    m_ImageRun[i + 1][Twoj].RightFlag = PlgIndex;
                    //m_ImageRun[i + 1][Twoj].SetRightFlag(PlgIndex);

                    if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        return true;
                    }
                    else
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        Turn.SetField(DirectionTypes.DT_DOWN, i + 1, Twoj, TwoStart);
                        return false;
                    }
                }
                #endregion
            }
            return false;
        }

        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1] 4像素右下角栅格与该栅格左小角点坐标对应
        ///ChangeDirection:true：前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
        ///         与当前3格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）对应
        ///ChangeDirection:false前一矢量点坐标[X坐标加1，Y坐标不变],（PtList[PtList.Count-1].X+1,PtList[PtList.Count-1].Y）
        ///         与当前3格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X+1）对应
        public bool Right(ref OSGeo.OGR.Geometry ogrter, ref TurnParam Turn)
        {
            #region 格点的栅格坐标
            //changeDireciton:true
            //0格点：（PtList[PtList.Count-1].Y,PtList[PtList.Count-1].X）    
            //1格点：（PtList[PtList.Count-1].Y,PtList[PtList.Count-1].X+1）
            //2格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X+1）
            //changeDirection:false
            //0格点：（PtList[PtList.Count-1].Y,PtList[PtList.Count-1].X+1）    
            //1格点：（PtList[PtList.Count-1].Y,PtList[PtList.Count-1].X+2）
            //2格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X+2）
            #endregion
            int i = Turn.ZeroRow;
            int j = Turn.ZeroRunColumn;
            int ZeroStart = Turn.ZeroStart;
            int OneAttr = -1;
            int TwoAttr = -1;

            int OneStart = ZeroStart + 1;
            int TwoStart = OneStart;
            int Onej = -1;
            int Twoj = -1;                          //1,2格点所在行游程中第几游程   
         
            #region 边界越界控制
            int Onei = i;
            int Twoi = i - 1;
            if (Twoi < ImageBound.MinRow && OneStart > (ImageBound.MaxColumn - 1))    //即Twoi=ImageBound.MaxRow
            {
                OneAttr = -2;
                TwoAttr = -2;
            }
            else if (Twoi < ImageBound.MinRow && OneStart <= (ImageBound.MaxColumn - 1))
            {
                TwoAttr = -2;
            }
            else if (Twoi >= ImageBound.MinRow && OneStart > (ImageBound.MaxColumn - 1))
            {
                OneAttr = -2;
                TwoAttr = -2;
            }
            #endregion

            if (OneAttr != -2)
            {
                #region 获取1格点属性
                if ((j + 1) < (m_ImageRun[i].Count))
                {
                    if (OneStart < m_ImageRun[i][j + 1].Start)
                    {
                        OneAttr = m_ImageRun[i][j].Attr;
                        Onej = j;
                    }
                    else
                    {
                        OneAttr = m_ImageRun[i][j + 1].Attr;
                        Onej = j + 1;
                    }
                }
                else
                {
                    OneAttr = m_ImageRun[i][j].Attr;
                    Onej = j;
                }
                #endregion
            }
            GeoPoint Point = new GeoPoint();
            Point.X = TwoStart;     //2格点栅格[I,J]  ——> 矢量[J, I+1]
            Point.Y = m_ImageSize.Height - (i - 1 + 1);        //2格点（i-1，TwoStart）
            GeoTransformPt(Point); //写入shape前要转换坐标系
            if (OneAttr != m_ImageRun[i][j].Attr)
            {

                #region 判断格点1与0号格点的属性异同关系
                m_ImageRun[i][j].RightFlag = PlgIndex;
                //m_ImageRun[i][j].SetRightFlag(PlgIndex);



                if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                {
                    //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                    //    PtList.Add(Point);                    //存储坐标
                    #region shape结构外圈点加入
                    ogrter.AddPoint_2D(Point.X, Point.Y);
                    #endregion
                    return true;
                }
                else
                {
                    //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                    //    PtList.Add(Point);                    //存储坐标
                    #region shape结构外圈点加入
                    ogrter.AddPoint_2D(Point.X, Point.Y);
                    #endregion
                    Turn.SetField(DirectionTypes.DT_DOWN, i, j, ZeroStart);
                    return false;
                }
                #endregion
            }
            else if (OneAttr == m_ImageRun[i][j].Attr)
            {
                if (TwoAttr != -2)
                {
                    #region 查找2格点属性
                    for (int k2 = 0; k2 < m_ImageRun[i - 1].Count - 1; k2++)
                    {
                        if (TwoStart >= m_ImageRun[i - 1][k2].Start && TwoStart < m_ImageRun[i - 1][k2 + 1].Start)
                        {
                            TwoAttr = m_ImageRun[i - 1][k2].Attr;
                            Twoj = k2;
                            break;
                        }
                    }
                    if (TwoAttr == -1)
                    {
                        TwoAttr = m_ImageRun[i - 1][m_ImageRun[i - 1].Count - 1].Attr;
                        Twoj = m_ImageRun[i - 1].Count - 1;
                    }
                    #endregion
                }

                #region 判断格点2与0格点属性异同关系
                if (TwoAttr != m_ImageRun[i][j].Attr)
                {
                    //if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
                    //{
                    //    if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                    //        PtList.Add(Point);                    //存储坐标
                    //    return true;                                                //不存储坐标
                    //}
                    if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        return true;
                    }
                    else
                    {
                        Turn.SetField(DirectionTypes.DT_RIGHT, i, Onej, OneStart);
                        return false;
                    }
                }
                else if (TwoAttr == m_ImageRun[i][j].Attr)
                {
                    m_ImageRun[i - 1][Twoj].LeftFlag = PlgIndex;
                    //m_ImageRun[i - 1][Twoj].SetLeftFlag(PlgIndex);

                    if (Point.X == ogrter.GetX(0) && Point.Y == ogrter.GetY(0))  //返回起点
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        return true;
                    }
                    else
                    {
                        //if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                        //    PtList.Add(Point);                    //存储坐标
                        #region shape结构外圈点加入
                        ogrter.AddPoint_2D(Point.X, Point.Y);
                        #endregion
                        Turn.SetField(DirectionTypes.DT_UP, i - 1, Twoj, TwoStart);
                        return false;
                    }
                }
                #endregion
            }
            return false;
        }
        #endregion

        private void GeoTransform(GeoPolygon Plg)
        {
            //m_GeoTransform[0],m_GeoTransform[3]分别为栅格左上角的地理X,Y坐标: 转变为地理坐标,写到shape中时，计算面积才正确的
            //则栅格左下角地理坐标,矢量坐标原点地理坐标为:
            double XGeo = m_GeoTransform[0];
            double YGeo = m_GeoTransform[3] + m_GeoTransform[5] * m_ImageSize.Height;

            for (int i = 0; i < Plg.ExteriorRing.Vertices.Count; i++)
            {
                //矢量坐标点：将一个栅格格点30*30m，当做长度1*1m来表示
                //因此需乘以实际像元大小：m_GeoTransform[1],m_GeoTransform[5]，比30大一点点，若用准确的30,和栅格影像叠加会有偏差
                //m_GeoTransform[5]以栅格图像左上角为原点，向下为Y轴正向，矢量Y轴方向相反，需乘-1
                Plg.ExteriorRing.Vertices[i].X = Plg.ExteriorRing.Vertices[i].X * m_GeoTransform[1] + XGeo;
                Plg.ExteriorRing.Vertices[i].Y = Plg.ExteriorRing.Vertices[i].Y * (-1) * m_GeoTransform[5] + YGeo;
            }
            for (int i = 0; i < Plg.InteriorRings.Count; i++)
            {
                for (int j = 0; j < Plg.InteriorRings[i].Vertices.Count; j++)
                {
                    Plg.InteriorRings[i].Vertices[j].X = Plg.InteriorRings[i].Vertices[j].X * m_GeoTransform[1] + XGeo;
                    Plg.InteriorRings[i].Vertices[j].Y = Plg.InteriorRings[i].Vertices[j].Y * (-1) * m_GeoTransform[5] + YGeo;
                }
            }
        }

        private void GeoTransformPt(GeoPoint Pt)
        {
            //m_GeoTransform[0],m_GeoTransform[3]分别为栅格左上角的地理X,Y坐标: 转变为地理坐标,写到shape中时，计算面积才正确的
            //则栅格左下角地理坐标,矢量坐标原点地理坐标为:
            double XGeo = m_GeoTransform[0];
            double YGeo = m_GeoTransform[3] + m_GeoTransform[5] * m_ImageSize.Height;

            //矢量坐标点：将一个栅格格点30*30m，当做长度1*1m来表示
            //因此需乘以实际像元大小：m_GeoTransform[1],m_GeoTransform[5]，比30大一点点，若用准确的30,和栅格影像叠加会有偏差
            //m_GeoTransform[5]以栅格图像左上角为原点，向下为Y轴正向，矢量Y轴方向相反，需乘-1
            Pt.X = Pt.X * m_GeoTransform[1] + XGeo;
            Pt.Y = Pt.Y * (-1) * m_GeoTransform[5] + YGeo;
        }

        //判断是否取消
        private bool CancelOrNot()
        {
            if (m_frmProgressBar.Cancel)
            {
                e.Cancel = true;
                log.WriteLog("已经取消了矢量化");
                return true;
            }
            return false;
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
    }

    public struct Bound
    {
        public int MinRow;
        public int MaxRow;
        public int MinColumn;
        public int MaxColumn;
    }

    public enum DirectionTypes
    {
        DT_UP = 1,
        DT_DOWN = 2,
        DT_LEFT = 3,
        DT_RIGHT = 4,
        DT_OVER = 5
    }

    public class TurnParam
    {
        public DirectionTypes Direction;  //下一步转向哪个方向  
        public int ZeroRow;               //下步0格点所处行、所在行的第几游程、所处列          
        public int ZeroRunColumn;
        public int ZeroStart;
        public int ThreeAttr;     //前一步3格点属性         以下3个变量都用于压缩
        public int NodeIndex;     //前一结点在多边形点数组中的下标号
        public int ZeroAttr;      //当前0格点属性

        public void SetField(DirectionTypes Dt, int Zr, int Zc, int Zs)
        {
            Direction = Dt;
            ZeroRow = Zr;
            ZeroRunColumn = Zc;
            ZeroStart = Zs;
        }
        public void SetField2(int Ta, int Ni, int Za)
        {
            ThreeAttr = Ta;
            NodeIndex = Ni;
            ZeroAttr = Za;
        }
    }
}
