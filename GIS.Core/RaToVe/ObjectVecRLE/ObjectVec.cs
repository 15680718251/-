using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using OSGeo.OGR;
using GIS.Geometries;

namespace GIS.RaToVe.ObjectVecRLE
{
    public partial class ObjectOrientedVec:IDisposable
    {
        //矢量坐标系：原点在图像左下角，X正向向右，Y正向向上,矢量化过程中采用为图像坐标系，然后再加上栅格原点：左下角的地理坐标偏移量
        
        #region 变量定义
        //public List<GeoPolygon> PlgList;
        public int PlgIndex;            //用于内，外圈跟踪时，将多边形当前个数，赋予相应的游程标记
        //public int ReConstructFlag;   //用于内，外圈跟踪时，赋予属于的多边形标示
        #endregion

        public void ExtractionVecPolygon(ref int FileCount )
        {
            try
            {
                TurnParam Turn = new TurnParam();
                PlgIndex = -1;         //多边形个数的索引号如果为0，代表还有一个多边形
                bool FileMove = false; //文件批量矢量化，是否内圈有超过3万个的，有则直接移走文件，删除已矢量化的部分文件

                #region 创建shapefile
                string strFileName = Path.GetFileNameWithoutExtension(strRasFilePathName) + "vec.shp";

                //CreatShapeFile(strExportfolderName, StrFileName);
                OSGeo.OGR.Ogr.RegisterAll();//注册所有驱动
                OSGeo.GDAL.Gdal.GetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");

                string pszDriverName = "ESRI Shapefile";
                Driver poDriver = Ogr.GetDriverByName(pszDriverName);//创建SHAPEFILE 文件驱动

                if (poDriver == null)
                    return;

                string SHPpathName = strExportfolderName + "\\" + strFileName;
                string lyrName = Path.GetFileNameWithoutExtension(SHPpathName);
                DataSource poDS = poDriver.CreateDataSource(strExportfolderName, null);

                if (poDS == null)
                    return;

                wkbGeometryType gmttype = wkbGeometryType.wkbPolygon;

                string m_lyrName = lyrName;

                //OSGeo.OSR.SpatialReference ss;   //读取shape中的投影和坐标信息
                //ss = orgLayer.GetSpatialRef();//orgLayer为OSGeo.OGR.layer类型实例
                //ss.ExportToWkt(out ppss);//将读取文件的shape投影信息输出到格式化字符串ppss
                //MessageBox.Show(ppss);

                //设定输出shape中的投影和坐标信息
                string strwkt = m_GdalDataset.GetProjectionRef();  //栅格数据的投影和坐标信息
                OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(strwkt);
                OSGeo.OGR.Layer poLayer = poDS.CreateLayer(m_lyrName, srs, gmttype, null);

                string[] AttrColumnName = new string[5] { "PlgID", "PlgAttr", "ElevMax", "ElevMin", "Area" };

                //字段
                for (int i = 0; i < AttrColumnName.Length; i++)
                {
                    string colName = AttrColumnName[i];
                    OSGeo.OGR.FieldDefn oField = null;
                    Type dType;

                    if (i == 0 || i == 2 || i == 3)
                        dType = typeof(Int32);          //多边形编号为整型
                    else if (i == 4)
                        dType = typeof(double);         //多边形面积为double
                    else
                        dType = typeof(string);

                    if (dType == typeof(Int32) || dType == typeof(Int64) || dType == typeof(Int16))
                    {
                        oField = new FieldDefn(colName, FieldType.OFTInteger);
                    }
                    else if (dType == typeof(double) || dType == typeof(float))
                    {
                        oField = new FieldDefn(colName, FieldType.OFTReal);
                    }
                    else if (dType == typeof(string))
                    {
                        oField = new FieldDefn(colName, FieldType.OFTString);
                    }
                    poLayer.CreateField(oField, i);
                }
                #endregion

                //#region 程序调试：写TXT
                //FileStream fs = new FileStream("G:\\ObjectVecTest.txt", FileMode.Create);
                //StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                //#endregion

                #region 所有多边形提取和写入shape
                for (int i = m_ImageSize.Height - 1; i >= 0; i--)
                {
                    for (int j = 0; j < m_ImageRun[i].Count; j++)
                    {
                        int RingAttr = -1, RingElevMax = 0, RingElevMin = 10000;

                        //控制不矢量化的像素
                        if (m_ImageRun[i][j].Attr == 0)   //属性为0的游程不跟踪形成多边形
                            continue;

                        #region 提取一个多边形
                        int Imin = -1, Imax = -1/*, Jmin = -1, Jmax = -1*/;
                        //记录当前多边形到达的最大行,用于限定内圈搜索的范围,但J不太方便记录,因为是游程
                        //最大行，就是起始行，最小行，只在Up和Down函数改变，但比较复杂
                        if (m_ImageRun[i][j].LeftFlag == -1)
                        {
                            OSGeo.OGR.Feature poFeature = new OSGeo.OGR.Feature(poLayer.GetLayerDefn());
                            OSGeo.OGR.Geometry ogrgtr = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);     //几何
                            OSGeo.OGR.Geometry ogrexter = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);//外环

                            # region 外圈提取
                            Imin = i; //初始化
                            Imax = i; //当前多边形用到的最大行,因为外圈开始追踪时就是第一个起点

                            PlgIndex++;

                            m_ImageRun[i][j].LeftFlag = PlgIndex; //标记用于哪个多边形:起始游程的标记这是,然后

                            RingAttr = m_ImageRun[i][j].Attr;
                            RingElevMax = m_ImageRun[i][j].ElevationMax;
                            RingElevMin = m_ImageRun[i][j].ElevationMin;

                            GeoPoint PtOut = new GeoPoint();
                            //List<GeoPoint> PtListOut = new List<GeoPoint>();
                            PtOut.X = m_ImageRun[i][j].Start;
                            PtOut.Y = m_ImageSize.Height - (i + 1);
                            //PtListOut.Add(PtOut);                //多边形第一点
                            Turn.SetField(DirectionTypes.DT_UP, i, j, (int)PtOut.X);

                            #region shape结构外圈点加入
                            GeoTransformPt(PtOut); //写入shape前要转换坐标系        
                            ogrexter.AddPoint_2D(PtOut.X, PtOut.Y);
                            #endregion

                            // Up函数和Right函数中分别得到外、内圈的第一个3格属性值
                            if (m_Compress)
                            {
                                #region 初始化3格点属性、初始点下标
                                int ThreeAttribute = -1;
                                #region 边界越界控制
                                if ((int)PtOut.X == ImageBound.MinColumn)
                                {                                             //列越界
                                    ThreeAttribute = -2;
                                }
                                #endregion
                                else //未越界情形，查询3格点属性
                                {
                                    if (ThreeAttribute != -2)
                                    {
                                        ThreeAttribute = m_ImageRun[i][j - 1].Attr;
                                    }
                                }
                                Turn.SetField2(ThreeAttribute, 0, RingAttr);
                                #endregion
                            }

                            bool OneRingOutOver = false;

                            
                            while (!OneRingOutOver)  //直到外环提取完成
                            {
                                if (Turn.Direction == DirectionTypes.DT_UP)
                                {
                                    int RunElevMin = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMin;
                                    int RunElevMax = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMax;

                                    if (RunElevMin < RingElevMin)
                                        RingElevMin = RunElevMin;
                                    if (RunElevMax > RingElevMax)
                                        RingElevMax = RunElevMax;

                                    OneRingOutOver = Up(ref ogrexter, ref Turn);

                                    if (Turn.ZeroRow < Imin) //当前0格点所在的行
                                        Imin = Turn.ZeroRow;
                                }
                                else if (Turn.Direction == DirectionTypes.DT_DOWN)
                                {
                                    int RunElevMin = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMin;
                                    int RunElevMax = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMax;

                                    if (RunElevMin < RingElevMin)
                                        RingElevMin = RunElevMin;
                                    if (RunElevMax > RingElevMax)
                                        RingElevMax = RunElevMax;

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

                            if (m_Compress)
                            {
                                if (Turn.NodeIndex != 0)
                                {
                                    #region 返回起点-最后一段的压缩
                                    {
                                        //DP算法调用
                                        //List<GeoPoint> RePtList = new List<GeoPoint>();
                                        //RePtList = DPReduction.DouglasPeuckerReduction(PtListOut, ToleranceDis, Turn.NodeIndex, PtListOut.Count - 1);
                                        //PtListOut.RemoveRange(Turn.NodeIndex, PtListOut.Count - Turn.NodeIndex);
                                        //PtListOut.AddRange(RePtList);
                                    }
                                    #endregion
                                }
                                else if (Turn.NodeIndex == 0)
                                {
                                    #region 全为坐标点形成的多边形的压缩
                                    ////有结点形成的多边形在上面4个函数中做了压缩
                                    //List<GeoPoint> RePtList = new List<GeoPoint>();
                                    //double MaxDistance = -1;
                                    //int IndexM = -1;
                                    //for (int Kk = 0; Kk < PtListOut.Count; Kk++) //为保证内外全重合情况，压缩结果相同，需要分为两段
                                    //{
                                    //    double Dis = PtListOut[0].DistanceTo(PtListOut[Kk]);
                                    //    if (Dis > MaxDistance)
                                    //    {
                                    //        MaxDistance = Dis;
                                    //        IndexM = Kk;
                                    //    }
                                    //}
                                    //RePtList = DPReduction.DouglasPeuckerReduction(PtListOut, ToleranceDis, IndexM, PtListOut.Count - 1);
                                    //PtListOut.RemoveRange(IndexM, PtListOut.Count - IndexM);
                                    //PtListOut.AddRange(RePtList);
                                    //RePtList.Clear();

                                    //RePtList = DPReduction.DouglasPeuckerReduction(PtListOut, ToleranceDis, 0, IndexM);
                                    //RePtList.RemoveAt(RePtList.Count - 1); //中间分隔点只存储一次
                                    //PtListOut.RemoveRange(0, IndexM);
                                    //PtListOut.InsertRange(0, RePtList);
                                    #endregion
                                }
                            }
                            //GeoLinearRing LinearRingOut = new GeoLinearRing(PtListOut);
                            //GeoPolygon Polygon = new GeoPolygon(LinearRingOut);
                            //Polygon.SetAttribute(RingAttr, RingElevMax, RingElevMin);
                            //double[] Attr = new double[3] { RingAttr, RingElevMax, RingElevMin };
                            #region shape结构外圈建立完毕
                            //double Areaex = ogrexter.GetArea();
                            //if (Areaex < 8200)
                            //    continue;                //外圈全部留下
                            //else
                            ogrgtr.AddGeometry(ogrexter);
                            #endregion
                            # endregion

                            #region 所有内圈提取
                            int Count = 0;
                            for (int i1 = Imax; i1 >= Imin; i1--)        //此处应优化：K>= 当前多边形最上方的栅格行号即可
                            {
                                for (int j1 = 0; j1 < m_ImageRun[i1].Count; j1++)
                                {
                                    //用m_ImageRun[i1][j1].LeftFlag == PlgIndex，而不是-1,是为了更精确和省时间，因为不用管LeftFlag不是-1，是其他值，然后继续判断RightFlag的情况了
                                    //这里应该用if而不是 while
                                    while (m_ImageRun[i1][j1].LeftFlag == PlgIndex && m_ImageRun[i1][j1].RightFlag == -1)
                                    {

                                        #region shape结构多边形一个内圈创建
                                        OSGeo.OGR.Geometry ogrinter = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                                        #endregion

                                        m_ImageRun[i1][j1].RightFlag = PlgIndex; //标记用于哪个多边形

                                        GeoPoint PtIn = new GeoPoint();
                                        //List<GeoPoint> PtListIn = new List<GeoPoint>();
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
                                                MessageBox.Show("内圈属性与外圈属性不等，出错");
                                                return;
                                            }
                                            PtIn.X = m_ImageRun[i1][j1 + 1].Start;  //判断，如果越界，则取最后一列值
                                            PtIn.Y = m_ImageSize.Height - (i1 + 1);
                                            //PtListIn.Add(PtIn);                //多边形第一点

                                            #region shape结构内圈点加入
                                            GeoTransformPt(PtIn); //写入shape前要转换坐标系
                                            ogrinter.AddPoint_2D(PtIn.X, PtIn.Y);
                                            #endregion

                                            Turn.SetField(DirectionTypes.DT_RIGHT, i1 + 1, Zeroj, ZeroStart);
                                            if (m_Compress)
                                            {
                                                #region 初始化3格点属性、初始点下标
                                                int ThreeAttribute = -1;
                                                #region 边界越界控制
                                                int Twoi = i1;
                                                if (Twoi < ImageBound.MinRow)    //即Twoi=ImageBound.MaxRow
                                                    ThreeAttribute = -2;
                                                #endregion
                                                else //未越界情形，查询3格点属性
                                                {
                                                    if (ThreeAttribute != -2)
                                                    {
                                                        #region 查找3格点属性
                                                        for (int k2 = 0; k2 < m_ImageRun[i1].Count - 1; k2++)
                                                        {
                                                            if (ZeroStart >= m_ImageRun[i1][k2].Start && ZeroStart < m_ImageRun[i1][k2 + 1].Start)
                                                            {
                                                                ThreeAttribute = m_ImageRun[i1][k2].Attr;
                                                                break;
                                                            }
                                                        }
                                                        if (ThreeAttribute == -1)
                                                        {
                                                            ThreeAttribute = m_ImageRun[i1][m_ImageRun[i1].Count - 1].Attr;
                                                        }
                                                        #endregion
                                                    }
                                                }
                                                Turn.SetField2(ThreeAttribute, 0, ZeroAttr);
                                                #endregion
                                            }
                                            bool OneRingInOver = false;
                                            while (!OneRingInOver)
                                            {
                                                if (Turn.Direction == DirectionTypes.DT_UP)
                                                {
                                                    int RunElevMin = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMin;
                                                    int RunElevMax = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMax;
                                                    if (RunElevMin < RingElevMin)
                                                        RingElevMin = RunElevMin;
                                                    if (RunElevMax > RingElevMax)
                                                        RingElevMax = RunElevMax;
                                                    OneRingInOver = Up(ref ogrinter, ref Turn);
                                                }
                                                else if (Turn.Direction == DirectionTypes.DT_DOWN)
                                                {
                                                    int RunElevMin = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMin;
                                                    int RunElevMax = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMax;
                                                    if (RunElevMin < RingElevMin)
                                                        RingElevMin = RunElevMin;
                                                    if (RunElevMax > RingElevMax)
                                                        RingElevMax = RunElevMax;
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
                                            if (m_Compress)
                                            {
                                                if (Turn.NodeIndex != 0)
                                                {
                                                    #region 返回起点-最后一段的压缩
                                                    {
                                                        //DP算法调用
                                                        //List<GeoPoint> RePtList = new List<GeoPoint>();
                                                        //RePtList = DPReduction.DouglasPeuckerReduction(PtListIn, ToleranceDis, Turn.NodeIndex, PtListIn.Count - 1);
                                                        //PtListIn.RemoveRange(Turn.NodeIndex, PtListIn.Count - Turn.NodeIndex);
                                                        //PtListIn.AddRange(RePtList);
                                                    }
                                                    #endregion
                                                }
                                                else if (Turn.NodeIndex == 0)
                                                {
                                                    #region 全为坐标点形成的多边形的压缩
                                                    //有结点形成的多边形在上面4个函数中做了压缩
                                                    //List<GeoPoint> RePtList = new List<GeoPoint>();
                                                    //double MaxDistance = -1;
                                                    //int IndexM = -1;
                                                    //for (int Kk = 0; Kk < PtListIn.Count; Kk++) //为保证内外全重合情况，压缩结果相同，需要分为两段
                                                    //{
                                                    //    double Dis = PtListIn[0].DistanceTo(PtListIn[Kk]);
                                                    //    if (Dis > MaxDistance)
                                                    //    {
                                                    //        MaxDistance = Dis;
                                                    //        IndexM = Kk;
                                                    //    }
                                                    //}
                                                    //RePtList = DPReduction.DouglasPeuckerReduction(PtListIn, ToleranceDis, IndexM, PtListIn.Count - 1);
                                                    //PtListIn.RemoveRange(IndexM, PtListIn.Count - IndexM);
                                                    //PtListIn.AddRange(RePtList);
                                                    //RePtList.Clear();

                                                    //RePtList = DPReduction.DouglasPeuckerReduction(PtListIn, ToleranceDis, 0, IndexM);
                                                    //RePtList.RemoveAt(RePtList.Count - 1); //中间分隔点只存储一次
                                                    //PtListIn.RemoveRange(0, IndexM); //因为前面已经移除掉下标为Indexm的点了，因此此时移除的会少一个点：本来该是0,Indexm+1
                                                    //PtListIn.InsertRange(0, RePtList);
                                                    #endregion
                                                }
                                            }
                                            //GeoLinearRing LinearRingIn = new GeoLinearRing(PtListIn);
                                            //Polygon.InteriorRings.Add(LinearRingIn);  //增加到内环中
                                        }
                                        else
                                        {
                                            PtIn.X = ImageBound.MaxColumn; //算法问题：向右已没有任何游程，如何处理？-不可能，一定会有的，因为还没有标记，必定存在当前游程的内圈，否则在提取外圈时就会标记右标记
                                            MessageBox.Show("内圈找到入口点，却向右到了边界，出错");
                                            return;               //这个是错误的，正确标记了游程，不应存在此问题，解决---
                                        }
                                        #region shape结构一个内圈建立完毕
                                        Double AreaIn = ogrinter.GetArea();
                                        //if (Math.Abs(AreaIn) < 8200)
                                        //  continue;             //小面积内圈直接不存储:外圈面积小于10个像素，内圈根本就不管了
                                        //else
                                        ogrgtr.AddGeometry(ogrinter);
                                        #endregion
                                        #region 程序调试：写TXT
                                        Count++;   //输出有内圈的多边形
                                        //sw.WriteLine("第\"{0}\"个多边形，第\"{1}\"内圈，面积为：\"{2}\"平方米", PlgIndex, Count, Math.Abs(AreaIn));//////////////////////////////
                                        #endregion
                                    }
                                    #region 跳过IMG文件中：超过3万个内圈的多边形|同时移动原始IMG文件到另外目录
                                    if (Count > 30000)
                                    {
                                        FileMove = true;
                                        PlgIndex--;
                                        break;
                                    }

                                    #endregion
                                }
                                #region 跳过IMG文件中：超过3万个内圈的多边形|同时移动原始IMG文件到另外目录
                                if (Count > 30000)
                                    break;
                                #endregion
                            }
                            #region 跳过IMG文件中：超过3万个内圈的多边形|同时移动原始IMG文件到另外目录
                            if (Count > 30000)
                                break;

                            #endregion
                            #endregion

                            #region 当前多边形写入shape
                            poFeature.SetGeometry(ogrgtr);

                            FieldType ftype = poFeature.GetFieldType(0);
                            if (ftype == OSGeo.OGR.FieldType.OFTInteger)
                                poFeature.SetField(0, PlgIndex);

                            //for (int m = 1; m < AttrColumnName.Length - 1; m++)
                            //{
                            ftype = poFeature.GetFieldType(1);

                            if (ftype == OSGeo.OGR.FieldType.OFTString)
                                poFeature.SetField(1, dics[System.Convert.ToString(RingAttr)]);
                                //poFeature.SetField(1, System.Convert.ToString(RingAttr));
                            //}
                            ftype = poFeature.GetFieldType(2);
                            if (ftype == OSGeo.OGR.FieldType.OFTInteger)
                                poFeature.SetField(2, RingElevMax);

                            ftype = poFeature.GetFieldType(3);
                            if (ftype == OSGeo.OGR.FieldType.OFTInteger)
                                poFeature.SetField(3, RingElevMin);

                            ftype = poFeature.GetFieldType(4);
                            if (ftype == OSGeo.OGR.FieldType.OFTReal)
                                poFeature.SetField(4, ogrgtr.GetArea());


                            poLayer.CreateFeature(poFeature);
                            poFeature.Dispose();
                            #endregion
                        }
                        # endregion

                    }

                    #region 跳过IMG文件中：超过3万个内圈的多边形|同时移动原始IMG文件到另外目录|删除已经完成矢量化的部分文件
                    try
                    {
                        if (FileMove)
                        {
                            //目录在程序里面全部都是“\\表示的，因为“\”表示的是转义字符”
                            m_GdalDataset.Dispose(); //释放栅格文件，否则一个线程在占用该文件，不可移动
                            string path = Path.GetDirectoryName(strRasFilePathName) + "\\Temp";
                            string pathFilename = path + "\\" + Path.GetFileName(strRasFilePathName);

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                                File.Move(strRasFilePathName, pathFilename);
                            }
                            else
                                File.Move(strRasFilePathName, pathFilename);

                            poDS.DeleteLayer(0); //直接使用SHP库的删除所有已生成shape相关文件函数
                            FileCount++;  //不可矢量化的文件数加1/////////////////////////////////////////////////////////////////////////
                            //File.Delete();
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("The process failed: {0}", e.ToString());
                    }
                    #endregion
                }

                #region 写shape结束
                poDS.Dispose();
                poDriver.Dispose();
                #endregion

                #region 程序调试：写TXT结束
                //sw.Close();
                //fs.Close();
                #endregion

                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            //throw new ApplicationException("没有异常产生，正常矢量化！");
        }
        
        public void TopologyReconstruct() //拓扑建立,已与多边形提取一起完成了
        {
            #region 测试GDAL库写Shape时对多边形类型的支持代码
            //////////////////////////////////////////
            //List<List<GeoPoint> >Listvertices = new List<List<GeoPoint>>();
            //for (int i = 0; i < 4; i++)
            //{
            //    List<GeoPoint> vertices = new List<GeoPoint>();
            //    GeoPoint vertice1 = new GeoPoint();
            //    GeoPoint vertice2 = new GeoPoint();
            //    GeoPoint vertice3 = new GeoPoint();
            //    GeoPoint vertice4 = new GeoPoint();
            //    vertice1.X = 0 + i * 10;
            //    vertice1.Y = 0 + i * 10;
            //    vertice2.X = 100 - i * 10;
            //    vertice2.Y = 0 + i * 10;
            //    vertice3.X = 100 - i * 10;
            //    vertice3.Y = 100 - i * 10;
            //    vertice4.X = 0 + i * 10;
            //    vertice4.Y = 100 - i * 10;
            //    vertices.Add(vertice1);
            //    vertices.Add(vertice2);
            //    vertices.Add(vertice3);
            //    vertices.Add(vertice4);
            //    Listvertices.Add(vertices);
            //}
            //GeoLinearRing ExteriorRings = new GeoLinearRing(Listvertices[0]);
            //for (int j = 1; j < 4; j++)
            //{
            //    GeoLinearRing Rings = new GeoLinearRing(Listvertices[j]);
            //    InteriorRings.Add(Rings);
            //}
            //GeoPolygon Plg = new GeoPolygon(ExteriorRings, InteriorRings);
            //PlgList.Add(Plg);
            /////////////////////////////////////////
            #endregion
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
            int ThreeAttr = -1;

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

            #region 获取3格点属性
            if (m_Compress)
            {
                if (OneStart == ImageBound.MinColumn)
                    ThreeAttr = -2;
                else if (ThreeAttr != -2)
                    ThreeAttr = m_ImageRun[i][j - 1].Attr;
            }
            #endregion
            //Point.X = PtList[PtList.Count - 1].X; //用上一点坐标表示，为加快算法执行，可直接用i,j表达
            //Point.Y = PtList[PtList.Count - 1].Y - 1; 
            GeoPoint Point = new GeoPoint();
            Point.X = OneStart;     //1格点栅格[I,J]  ——> 矢量[J, I+1]
            Point.Y = m_ImageSize.Height - (i - 1 + 1);        //加快算法：用OneStart而不是m_ImageRun[i][j].Start
            GeoTransformPt(Point); //写入shape前要转换坐标系 
            //:此处不转换，在判断是否回到起点时，将出错（起点已经转换了）,而且此处只需要一行代码，省的后面每个都要转换一次

            //判断的结点，是此步Point点的，前一点。压缩的是上一结点和此结点间的点。不连此步的Point点
            #region 判断3格点属性是否改变-确定是否为结点，决定调用否DP算法（需要用到2格点属性-复制了获取代码）
            if (m_Compress)
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
                if (ThreeAttr != Turn.ThreeAttr || (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr))
                {  //将1,3和2,0格点相同的类型点也要当做结点，否则将出现问题
                    //DP算法调用
                    //    if (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr)
                    //        PtList.Add(Point);   //11类型结点，是箭头尖上的点 还为存储；3格点属性改变情况的点，则已存储
                    //    if (PtList[PtList.Count - 1].IsEqual(PtList[Turn.NodeIndex]))
                    //    {
                    //        #region 起始和结束点都为同一11类型点形成的封闭型弧段，按坐标点形成的弧段压缩
                    //        List<GeoPoint> RePtList = new List<GeoPoint>();
                    //        double MaxDistance = -1;
                    //        int IndexM = -1;
                    //        for (int Kk = Turn.NodeIndex; Kk < PtList.Count; Kk++) //为保证内外全重合情况，压缩结果相同，需要分为两段
                    //        {
                    //            double Dis = PtList[Turn.NodeIndex].DistanceTo(PtList[Kk]);
                    //            if (Dis > MaxDistance)
                    //            {
                    //                MaxDistance = Dis;
                    //                IndexM = Kk;
                    //            }
                    //        }
                    //        RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, IndexM, PtList.Count - 1);
                    //        PtList.RemoveRange(IndexM, PtList.Count - IndexM);
                    //        PtList.AddRange(RePtList);
                    //        RePtList.Clear();

                    //        RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, IndexM);
                    //        RePtList.RemoveAt(RePtList.Count - 1); //中间分隔点只存储一次
                    //        PtList.RemoveRange(Turn.NodeIndex, IndexM - Turn.NodeIndex);
                    //        PtList.InsertRange(Turn.NodeIndex, RePtList);
                    //        Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr);
                    //        #endregion
                    //    }
                    //    else
                    //    {
                    //        List<GeoPoint> RePtList = new List<GeoPoint>();
                    //        RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, PtList.Count - 1);
                    //        PtList.RemoveRange(Turn.NodeIndex, PtList.Count - Turn.NodeIndex);
                    //        PtList.AddRange(RePtList);
                    //        Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr); //此步Point点的前一点。因为还未存此步Point点，则为PtList.Count - 1
                    //    }
                }
            }
            #endregion
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
            int ThreeAttr = -1;
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
            #region 获取3格点属性
            if (m_Compress)
            {
                if (OneStart == (ImageBound.MaxColumn - 1))
                    ThreeAttr = -2;
                else if (ThreeAttr != -2)
                    ThreeAttr = m_ImageRun[i][j + 1].Attr;
            }
            #endregion
            GeoPoint Point = new GeoPoint();
            Point.X = TwoStart;  //3格点栅格[I,J]  ——> 矢量[J, I+1]  3格点与2格点列值相同
            Point.Y = m_ImageSize.Height - (i + 1);
            GeoTransformPt(Point); //写入shape前要转换坐标系

            //判断的结点，是此步Point点的，前一点。压缩的是上一结点和此结点间的点。不连此步的Point点
            #region 判断3格点属性是否改变-确定是否为结点，决定调用否DP算法（需要用到2格点属性-复制了获取代码）
            if (m_Compress)
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
                if (ThreeAttr != Turn.ThreeAttr || (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr))
                {
                    //DP算法调用
                    //if (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr)
                    //    PtList.Add(Point);   //11类型结点，是箭头尖上的点 还为存储；3格点属性改变情况的点，则已存储
                    //if (PtList[PtList.Count - 1].IsEqual(PtList[Turn.NodeIndex]))
                    //{
                    //    #region 起始和结束点都为同一11类型点形成的封闭型弧段，按坐标点形成的弧段压缩
                    //    List<GeoPoint> RePtList = new List<GeoPoint>();
                    //    double MaxDistance = -1;
                    //    int IndexM = -1;
                    //    for (int Kk = Turn.NodeIndex; Kk < PtList.Count; Kk++) //为保证内外全重合情况，压缩结果相同，需要分为两段
                    //    {
                    //        double Dis = PtList[Turn.NodeIndex].DistanceTo(PtList[Kk]);
                    //        if (Dis > MaxDistance)
                    //        {
                    //            MaxDistance = Dis;
                    //            IndexM = Kk;
                    //        }
                    //    }
                    //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, IndexM, PtList.Count - 1);
                    //    PtList.RemoveRange(IndexM, PtList.Count - IndexM);
                    //    PtList.AddRange(RePtList);
                    //    RePtList.Clear();

                    //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, IndexM);
                    //    RePtList.RemoveAt(RePtList.Count - 1); //中间分隔点只存储一次
                    //    PtList.RemoveRange(Turn.NodeIndex, IndexM - Turn.NodeIndex); //上面已经移除了Indexm点了
                    //    PtList.InsertRange(Turn.NodeIndex, RePtList);
                    //    Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr); //此步Point点的前一点。因为还未存此步Point点，则为PtList.Count - 1
                    //    #endregion
                    //}
                    //else
                    //{
                    //    List<GeoPoint> RePtList = new List<GeoPoint>();
                    //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, PtList.Count - 1);
                    //    PtList.RemoveRange(Turn.NodeIndex, PtList.Count - Turn.NodeIndex);
                    //    PtList.AddRange(RePtList);
                    //    Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr); //此步Point点的前一点。因为还未存此步Point点，则为PtList.Count - 1
                    //}
                }
            }
            #endregion
            if (OneAttr != m_ImageRun[i][j].Attr)
            {
                #region 判断格点1与格点0属性异同关系
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
            int ThreeAttr = -1;  //以下两个变量用于矢量压缩
            int Threej = -1;
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
                ThreeAttr = -2;
            }
            else if (Twoi > ImageBound.MaxRow - 1 && OneStart >= ImageBound.MinColumn)
            {
                TwoAttr = -2;                    //行越界
                ThreeAttr = -2;
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
                if (m_Compress)
                {
                    if (ThreeAttr != -2)
                    {
                        #region 查找3格点属性
                        for (int k2 = 0; k2 < m_ImageRun[i + 1].Count - 1; k2++)
                        {
                            if (ZeroStart >= m_ImageRun[i + 1][k2].Start && ZeroStart < m_ImageRun[i + 1][k2 + 1].Start)
                            {
                                ThreeAttr = m_ImageRun[i + 1][k2].Attr;
                                Threej = k2;
                                break;
                            }
                        }
                        if (ThreeAttr == -1)
                        {
                            ThreeAttr = m_ImageRun[i + 1][m_ImageRun[i + 1].Count - 1].Attr;
                            Threej = m_ImageRun[i + 1].Count - 1;
                        }
                        #endregion
                    }
                    if (TwoAttr != -2)
                    {
                        #region 获取2格点属性
                        if (TwoStart >= m_ImageRun[i + 1][Threej].Start)
                        {
                            TwoAttr = m_ImageRun[i + 1][Threej].Attr;
                            Twoj = Threej;
                        }
                        else
                        {
                            TwoAttr = m_ImageRun[i + 1][Threej - 1].Attr;
                            Twoj = Threej - 1;
                        }
                        #endregion
                    }
                    //判断的结点，是此步Point点的，前一点。压缩的是上一结点和此结点间的点。不连此步的Point点
                    #region 判断3格点属性是否改变-确定是否为结点，决定调用否DP算法（需要用到2格点属性-复制了获取代码）
                    if (ThreeAttr != Turn.ThreeAttr || (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr))
                    {
                        //DP算法调用
                        //if (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr)
                        //    PtList.Add(Point);   //11类型结点，是箭头尖上的点 还为存储；3格点属性改变情况的点，则已存储
                        //if (PtList[PtList.Count - 1].IsEqual(PtList[Turn.NodeIndex]))
                        //{
                        //    #region 起始和结束点都为同一11类型点形成的封闭型弧段，按坐标点形成的弧段压缩
                        //    List<GeoPoint> RePtList = new List<GeoPoint>();
                        //    double MaxDistance = -1;
                        //    int IndexM = -1;
                        //    for (int Kk = Turn.NodeIndex; Kk < PtList.Count; Kk++) //为保证内外全重合情况，压缩结果相同，需要分为两段
                        //    {
                        //        double Dis = PtList[Turn.NodeIndex].DistanceTo(PtList[Kk]);
                        //        if (Dis > MaxDistance)
                        //        {
                        //            MaxDistance = Dis;
                        //            IndexM = Kk;
                        //        }
                        //    }
                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, IndexM, PtList.Count - 1);
                        //    PtList.RemoveRange(IndexM, PtList.Count - IndexM);
                        //    PtList.AddRange(RePtList);
                        //    RePtList.Clear();

                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, IndexM);
                        //    RePtList.RemoveAt(RePtList.Count - 1); //中间分隔点只存储一次
                        //    PtList.RemoveRange(Turn.NodeIndex, IndexM - Turn.NodeIndex);
                        //    PtList.InsertRange(Turn.NodeIndex, RePtList);
                        //    Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr);
                        //    #endregion
                        //}
                        //else
                        //{
                        //    List<GeoPoint> RePtList = new List<GeoPoint>();
                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, PtList.Count - 1);
                        //    PtList.RemoveRange(Turn.NodeIndex, PtList.Count - Turn.NodeIndex);
                        //    PtList.AddRange(RePtList);
                        //    Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr); //此步Point点的前一点。因为还未存此步Point点，则为PtList.Count - 1
                        //}
                    }
                    #endregion
                }
                #region 判断格点1与0号格点的属性异同关系
                m_ImageRun[i][j].LeftFlag = PlgIndex;
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
                if (m_Compress)
                {
                    if (ThreeAttr != -2)
                    {
                        #region 获取3格点属性
                        if ((Twoj + 1) < (m_ImageRun[i + 1].Count))
                        {
                            if (ZeroStart < m_ImageRun[i + 1][Twoj + 1].Start)
                                ThreeAttr = TwoAttr;
                            else
                                ThreeAttr = m_ImageRun[i + 1][Twoj + 1].Attr;
                        }
                        else
                            ThreeAttr = TwoAttr;
                        #endregion
                    }
                    //判断的结点，是此步Point点的，前一点。压缩的是上一结点和此结点间的点。不连此步的Point点
                    #region 判断3格点属性是否改变-确定是否为结点，决定调用否DP算法（需要用到2格点属性-复制了获取代码）
                    if (ThreeAttr != Turn.ThreeAttr || (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr))
                    {
                        //DP算法调用
                        //if (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr)
                        //    PtList.Add(Point);   //11类型结点，是箭头尖上的点 还为存储；3格点属性改变情况的点，则已存储
                        //if (PtList[PtList.Count - 1].IsEqual(PtList[Turn.NodeIndex]))
                        //{
                        //    #region 起始和结束点都为同一11类型点形成的封闭型弧段，按坐标点形成的弧段压缩
                        //    List<GeoPoint> RePtList = new List<GeoPoint>();
                        //    double MaxDistance = -1;
                        //    int IndexM = -1;
                        //    for (int Kk = Turn.NodeIndex; Kk < PtList.Count; Kk++) //为保证内外全重合情况，压缩结果相同，需要分为两段
                        //    {
                        //        double Dis = PtList[Turn.NodeIndex].DistanceTo(PtList[Kk]);
                        //        if (Dis > MaxDistance)
                        //        {
                        //            MaxDistance = Dis;
                        //            IndexM = Kk;
                        //        }
                        //    }
                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, IndexM, PtList.Count - 1);
                        //    PtList.RemoveRange(IndexM, PtList.Count - IndexM);
                        //    PtList.AddRange(RePtList);
                        //    RePtList.Clear();

                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, IndexM);
                        //    RePtList.RemoveAt(RePtList.Count - 1); //中间分隔点只存储一次
                        //    PtList.RemoveRange(Turn.NodeIndex, IndexM - Turn.NodeIndex);
                        //    PtList.InsertRange(Turn.NodeIndex, RePtList);
                        //    Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr);
                        //    #endregion
                        //}
                        //else
                        //{
                        //    List<GeoPoint> RePtList = new List<GeoPoint>();
                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, PtList.Count - 1);
                        //    PtList.RemoveRange(Turn.NodeIndex, PtList.Count - Turn.NodeIndex);
                        //    PtList.AddRange(RePtList);
                        //    Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr); //此步Point点的前一点。因为还未存此步Point点，则为PtList.Count - 1
                        //}
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
        /// </summary>
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
            int ThreeAttr = -1; //以下两个变量用于压缩
            int Threej = -1;
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
                ThreeAttr = -2;
            }
            else if (Twoi < ImageBound.MinRow && OneStart <= (ImageBound.MaxColumn - 1))
            {
                TwoAttr = -2;
                ThreeAttr = -2;
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
                if (m_Compress)
                {
                    if (ThreeAttr != -2)
                    {
                        #region 查找3格点属性
                        for (int k2 = 0; k2 < m_ImageRun[i - 1].Count - 1; k2++)
                        {
                            if (ZeroStart >= m_ImageRun[i - 1][k2].Start && ZeroStart < m_ImageRun[i - 1][k2 + 1].Start)
                            {
                                ThreeAttr = m_ImageRun[i - 1][k2].Attr;
                                Threej = k2;
                                break;
                            }
                        }
                        if (ThreeAttr == -1)
                        {
                            ThreeAttr = m_ImageRun[i - 1][m_ImageRun[i - 1].Count - 1].Attr;
                            Threej = m_ImageRun[i - 1].Count - 1;
                        }
                        #endregion
                    }
                    if (TwoAttr != -2)
                    {
                        #region 获取2格点属性
                        if ((Threej + 1) < (m_ImageRun[i - 1].Count))
                        {
                            if (TwoStart < m_ImageRun[i - 1][Threej + 1].Start)
                            {
                                TwoAttr = ThreeAttr;
                                Twoj = Threej;
                            }
                            else
                            {
                                TwoAttr = m_ImageRun[i - 1][Threej + 1].Attr;
                                Twoj = Threej + 1;
                            }
                        }
                        else
                        {
                            TwoAttr = ThreeAttr;
                            Twoj = Threej;
                        }
                        #endregion
                    }
                    //判断的结点，是此步Point点的，前一点。压缩的是上一结点和此结点间的点。不连此步的Point点
                    #region 判断3格点属性是否改变-确定是否为结点，决定调用否DP算法（需要用到2格点属性-复制了获取代码）
                    if (ThreeAttr != Turn.ThreeAttr || (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr))
                    {
                        //DP算法调用
                        //if (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr)
                        //    PtList.Add(Point);   //11类型结点，是箭头尖上的点 还为存储；3格点属性改变情况的点，则已存储
                        //if (PtList[PtList.Count - 1].IsEqual(PtList[Turn.NodeIndex]))
                        //{
                        //    #region 起始和结束点都为同一11类型点形成的封闭型弧段，按坐标点形成的弧段压缩
                        //    List<GeoPoint> RePtList = new List<GeoPoint>();
                        //    double MaxDistance = -1;
                        //    int IndexM = -1;
                        //    for (int Kk = Turn.NodeIndex; Kk < PtList.Count; Kk++) //为保证内外全重合情况，压缩结果相同，需要分为两段
                        //    {
                        //        double Dis = PtList[Turn.NodeIndex].DistanceTo(PtList[Kk]);
                        //        if (Dis > MaxDistance)
                        //        {
                        //            MaxDistance = Dis;
                        //            IndexM = Kk;
                        //        }
                        //    }
                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, IndexM, PtList.Count - 1);
                        //    PtList.RemoveRange(IndexM, PtList.Count - IndexM);
                        //    PtList.AddRange(RePtList);
                        //    RePtList.Clear();

                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, IndexM);
                        //    RePtList.RemoveAt(RePtList.Count - 1); //中间分隔点只存储一次
                        //    PtList.RemoveRange(Turn.NodeIndex, IndexM - Turn.NodeIndex);
                        //    PtList.InsertRange(Turn.NodeIndex, RePtList);
                        //    Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr);
                        //    #endregion
                        //}
                        //else
                        //{
                        //    List<GeoPoint> RePtList = new List<GeoPoint>();
                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, PtList.Count - 1);
                        //    PtList.RemoveRange(Turn.NodeIndex, PtList.Count - Turn.NodeIndex);
                        //    PtList.AddRange(RePtList);
                        //    Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr); //此步Point点的前一点。因为还未存此步Point点，则为PtList.Count - 1
                        //}
                    }
                    #endregion
                }
                #region 判断格点1与0号格点的属性异同关系
                m_ImageRun[i][j].RightFlag = PlgIndex;
                //if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
                //{
                //    if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                //        PtList.Add(Point);                    //存储坐标
                //    return true;
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
                if (m_Compress)
                {
                    if (ThreeAttr != -2)
                    {
                        #region 获取3格点属性
                        if (ZeroStart >= m_ImageRun[i - 1][Twoj].Start)
                            ThreeAttr = m_ImageRun[i - 1][Twoj].Attr;
                        else
                            ThreeAttr = m_ImageRun[i - 1][Twoj - 1].Attr;
                        #endregion
                    }
                    //判断的结点，是此步Point点的，前一点。压缩的是上一结点和此结点间的点。不连此步的Point点
                    #region 判断3格点属性是否改变-确定是否为结点，决定调用否DP算法（需要用到2格点属性-复制了获取代码）
                    if (ThreeAttr != Turn.ThreeAttr || (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr))
                    {
                        //DP算法调用
                        //if (ThreeAttr == OneAttr && TwoAttr == Turn.ZeroAttr)
                        //    PtList.Add(Point);   //11类型结点，是箭头尖上的点 还为存储；3格点属性改变情况的点，则已存储
                        //if (PtList[PtList.Count - 1].IsEqual(PtList[Turn.NodeIndex]))
                        //{
                        //    #region 起始和结束点都为同一11类型点形成的封闭型弧段，按坐标点形成的弧段压缩
                        //    List<GeoPoint> RePtList = new List<GeoPoint>();
                        //    double MaxDistance = -1;
                        //    int IndexM = -1;
                        //    for (int Kk = Turn.NodeIndex; Kk < PtList.Count; Kk++) //为保证内外全重合情况，压缩结果相同，需要分为两段
                        //    {
                        //        double Dis = PtList[Turn.NodeIndex].DistanceTo(PtList[Kk]);
                        //        if (Dis > MaxDistance)
                        //        {
                        //            MaxDistance = Dis;
                        //            IndexM = Kk;
                        //        }
                        //    }
                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, IndexM, PtList.Count - 1);
                        //    PtList.RemoveRange(IndexM, PtList.Count - IndexM);
                        //    PtList.AddRange(RePtList);
                        //    RePtList.Clear();

                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, IndexM);
                        //    RePtList.RemoveAt(RePtList.Count - 1); //中间分隔点只存储一次
                        //    PtList.RemoveRange(Turn.NodeIndex, IndexM - Turn.NodeIndex);
                        //    PtList.InsertRange(Turn.NodeIndex, RePtList);
                        //    Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr);
                        //    #endregion
                        //}
                        //else
                        //{
                        //    List<GeoPoint> RePtList = new List<GeoPoint>();
                        //    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, PtList.Count - 1);
                        //    PtList.RemoveRange(Turn.NodeIndex, PtList.Count - Turn.NodeIndex);
                        //    PtList.AddRange(RePtList);
                        //    Turn.SetField2(ThreeAttr, PtList.Count - 1, Turn.ZeroAttr); //此步Point点的前一点。因为还未存此步Point点，则为PtList.Count - 1
                        //}
                    }
                    #endregion
                    //判断3格点属性是否改变的代码，应放在上面的if条件外面，因为可能已确定为了-2,应把各种情况都得到判断
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
                    //if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
                    //{
                    //    if (!Point.IsEqual(PtList[PtList.Count - 1]))  //因为可能前面如果要压缩，11类型结点情况可能已经存储当前点
                    //        PtList.Add(Point);                    //存储坐标
                    //    return true;
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

        //#region  InUp InDown InLeft InRight函数-内圈
        //        /// </summary>
        //        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1]
        //        ///InUp函数：前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
        //        ///         与当前3格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）对应
        //        public bool InUp(ref List<GeoPoint> PtList, ref TurnParam Turn)
        //        {
        //            #region 格点的栅格坐标//用坐标表示
        //            //0格点：（i,PtList[PtList.Count-1].X-1）                    
        //            //1格点：（i-1,PtList[PtList.Count-1].X-1）
        //            //2格点：（i-1,PtList[PtList.Count-1].X）

        //            //int OneAttr = -1;
        //            //int TwoAttr = -1;
        //            //int OneStart = PtList[PtList.Count - 1].X - 1;
        //            //int TwoStart = PtList[PtList.Count - 1].X;
        //            #endregion
        //            #region 格点的栅格坐标-游程表示
        //            //0格点：（i,m_ImageRun[i][j+1].Start-1）   //用下一游程表示
        //            //1格点：（i-1,m_ImageRun[i][j+1].Start-1）
        //            //2格点：（i-1,m_ImageRun[i][j+1].Start）
        //            #endregion
        //            int i = Turn.ZeroRow;
        //            int j = Turn.ZeroRunColumn;
        //            int ZeroStart = Turn.ZeroStart;
        //            int OneAttr = -1;
        //            int TwoAttr = -1;
        //            int OneStart = ZeroStart;
        //            int TwoStart = OneStart + 1;
        //            int Onej = -1;
        //            int Twoj = -1;                          //1,2格点所在行游程中第几游程
        //            #region 边界越界控制
        //            int Onei = i - 1;
        //            int Twoi = i - 1;
        //            if (Onei < ImageBound.MinRow)    //即Onei=-1  行越界
        //            {
        //                OneAttr = -2;
        //                TwoAttr = -2;
        //            }
        //            else if (Onei >= ImageBound.MinRow && OneStart == (ImageBound.MaxColumn - 1))
        //            {                                             //列越界
        //                OneAttr = m_ImageRun[i - 1][m_ImageRun[i - 1].Count - 1].Attr;
        //                Onej = m_ImageRun[i - 1].Count - 1;
        //                TwoAttr = -2;
        //            }
        //            #endregion
        //            else
        //            {
        //                #region 查找格点1属性
        //                for (int k = 0; k < m_ImageRun[i - 1].Count - 1; k++)
        //                {
        //                    if (OneStart >= m_ImageRun[i - 1][k].Start && OneStart < m_ImageRun[i - 1][k + 1].Start)
        //                    {
        //                        OneAttr = m_ImageRun[i - 1][k].Attr;
        //                        Onej = k;
        //                        break;
        //                    }
        //                }
        //                if (OneAttr == -1)
        //                {
        //                    OneAttr = m_ImageRun[i - 1][m_ImageRun[i - 1].Count - 1].Attr;
        //                    Onej = m_ImageRun[i - 1].Count - 1;
        //                }
        //                #endregion
        //            }
        //            #region 判断格点1与格点0属性异同关系
        //            //Point.X = PtList[PtList.Count - 1].X; //用上一点坐标表示，为加快算法执行，可直接用i,j表达
        //            //Point.Y = PtList[PtList.Count - 1].Y - 1;
        //            //Point.X = PtList[PtList.Count - 1].X;     //用上一点坐标表示，或者是 用2格点栅格[I,J]  ——> 矢量[J, I+1]---加快算法速度
        //            //Point.Y = i - 1 + 1;
        //            GeoPoint Point = new GeoPoint();
        //            Point.X = TwoStart;     //用2格点栅格[I,J]  ——> 矢量[J, I+1]---加快算法速度
        //            Point.Y = m_ImageSize.Height - (i - 1 + 1);
        //            if (OneAttr != m_ImageRun[i][j].Attr)               //无需标记游程
        //            {
        //                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
        //                {
        //                    return true;
        //                }
        //                else
        //                {
        //                    PtList.Add(Point);                    //存储坐标
        //                    Turn.SetField(DirectionTypes.DT_LEFT, i, j, ZeroStart);
        //                    return false;
        //                }
        //            }
        //            #endregion
        //            else if (OneAttr == m_ImageRun[i][j].Attr)
        //            {
        //                if (TwoAttr != -2)
        //                {
        //                    #region 获取2格点属性
        //                    if ((Onej + 1) < (m_ImageRun[i - 1].Count))
        //                    {
        //                        if (TwoStart < m_ImageRun[i - 1][Onej + 1].Start)
        //                        {
        //                            TwoAttr = m_ImageRun[i - 1][Onej].Attr;
        //                            Twoj = Onej;
        //                        }
        //                        else
        //                        {
        //                            TwoAttr = m_ImageRun[i - 1][Onej + 1].Attr;
        //                            Twoj = Onej + 1;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        TwoAttr = m_ImageRun[i - 1][Onej].Attr;
        //                        Twoj = Onej;
        //                    }
        //                    #endregion
        //                }
        //                #region 判断格点2与0格点属性异同关系
        //                if (TwoAttr != m_ImageRun[i][j].Attr)
        //                {
        //                    m_ImageRun[i - 1][Onej].RightFlag = ReConstructFlag; //标记游程   应该加上如果已经为真，即抛出异常
        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
        //                    {
        //                        return true;
        //                    }
        //                    else                                             //不存储坐标
        //                    {
        //                        Turn.SetField(DirectionTypes.DT_UP, i - 1, Onej, OneStart);
        //                        return false;
        //                    }

        //                }
        //                else if (TwoAttr == m_ImageRun[i][j].Attr)
        //                {
        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //未返回起点
        //                    {
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        PtList.Add(Point);                       //存储坐标
        //                        Turn.SetField(DirectionTypes.DT_RIGHT, i - 1, Twoj, TwoStart);
        //                        return false;
        //                    }
        //                }
        //                #endregion
        //            }
        //            return false;
        //        }
        //        /// </summary>
        //        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1]
        //        ///InDown函数：前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
        //        ///         与当前0格点正上方格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）对应
        //        public bool InDown(ref List<GeoPoint> PtList, ref TurnParam Turn)
        //        {
        //            #region 格点的栅格坐标
        //            //0格点：（i,m_ImageRun[i][j].Start）
        //            //1格点：（i+1,m_ImageRun[i][j].Start）
        //            //2格点：（i+1,m_ImageRun[i][j].Start-1）
        //            #endregion
        //            int i = Turn.ZeroRow;
        //            int j = Turn.ZeroRunColumn;
        //            int ZeroStart = Turn.ZeroStart;
        //            int OneAttr = -1;
        //            int TwoAttr = -1;
        //            int OneStart = ZeroStart;
        //            int TwoStart = OneStart - 1;
        //            int Onej = -1;
        //            int Twoj = -1;                          //1,2格点所在行游程中第几游程 
        //            #region 边界越界控制
        //            int Onei = i + 1;
        //            int Twoi = i + 1;
        //            if (Onei > ImageBound.MaxRow - 1)    //即Onei=ImageBound.MaxRow
        //            {
        //                OneAttr = -2;
        //                TwoAttr = -2;
        //            }
        //            else if (Onei <= (ImageBound.MaxRow - 1) && OneStart == ImageBound.MinColumn)
        //            {
        //                OneAttr = m_ImageRun[i + 1][0].Attr;
        //                Onej = 0;
        //                TwoAttr = -2;
        //            }
        //            #endregion
        //            else
        //            {
        //                #region 查找格点1属性
        //                for (int k = 0; k < m_ImageRun[i + 1].Count - 1; k++)
        //                {
        //                    if (OneStart >= m_ImageRun[i + 1][k].Start && OneStart < m_ImageRun[i + 1][k + 1].Start)
        //                    {
        //                        OneAttr = m_ImageRun[i + 1][k].Attr;
        //                        Onej = k;
        //                        break;
        //                    }
        //                }
        //                if (OneAttr == -1)
        //                {
        //                    OneAttr = m_ImageRun[i + 1][m_ImageRun[i + 1].Count - 1].Attr;
        //                    Onej = m_ImageRun[i + 1].Count - 1;
        //                }
        //                #endregion
        //            }
        //            #region 判断格点1与格点0属性异同关系
        //            GeoPoint Point = new GeoPoint();
        //            Point.X = OneStart;  //0格点栅格[I,J]  ——> 矢量[J, I+1]
        //            Point.Y = m_ImageSize.Height - (i + 1);
        //            if (OneAttr != m_ImageRun[i][j].Attr)
        //            {
        //                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
        //                {
        //                    return true;
        //                }
        //                else
        //                {
        //                    PtList.Add(Point);                    //存储坐标
        //                    Turn.SetField(DirectionTypes.DT_RIGHT, i, j, ZeroStart);
        //                    return false;
        //                }
        //            }
        //            #endregion
        //            else if (OneAttr == m_ImageRun[i][j].Attr)
        //            {
        //                if (TwoAttr != -2)
        //                {
        //                    #region 获取2格点属性
        //                    if (TwoStart >= m_ImageRun[i + 1][Onej].Start)
        //                    {
        //                        TwoAttr = m_ImageRun[i + 1][Onej].Attr;
        //                        Twoj = Onej;
        //                    }
        //                    else
        //                    {
        //                        TwoAttr = m_ImageRun[i + 1][Onej - 1].Attr;
        //                        Twoj = Onej - 1;
        //                    }
        //                    #endregion
        //                }
        //                #region 判断格点2与0格点属性异同关系
        //                if (TwoAttr != m_ImageRun[i][j].Attr)
        //                {
        //                    m_ImageRun[i + 1][Onej].LeftFlag = ReConstructFlag;
        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
        //                    {
        //                        return true;
        //                    }
        //                    else                                             //不存储坐标
        //                    {
        //                        Turn.SetField(DirectionTypes.DT_DOWN, i + 1, Onej, OneStart);
        //                        return false;
        //                    }

        //                }
        //                else if (TwoAttr == m_ImageRun[i][j].Attr)
        //                {
        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //未返回起点
        //                    {
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        PtList.Add(Point);                       //存储坐标
        //                        Turn.SetField(DirectionTypes.DT_LEFT, i + 1, Twoj, TwoStart);
        //                        return false;
        //                    }
        //                }
        //                #endregion
        //            }
        //            return false;
        //        }
        //        /// </summary>
        //        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1]
        //        ///ChangeDirection:true：前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
        //        ///         与当前3格点右边格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）对应
        //        ///ChangeDirection:false--前一矢量点坐标[X坐标减1，Y坐标不变],（PtList[PtList.Count-1].X-1,PtList[PtList.Count-1].Y）
        //        ///          后与当前3格点右边一个栅格坐标对应（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X-1）    
        //        public bool InLeft(ref List<GeoPoint> PtList, ref TurnParam Turn)
        //        {
        //            #region 格点的栅格坐标
        //            //ChangeDirection:true
        //            //0格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-1）    
        //            //1格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-1-1）
        //            //2格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-1-1）
        //            //ChangeDirection:false
        //            //0格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-2）    
        //            //1格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-3）
        //            //2格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-3）
        //            #endregion
        //            int i = Turn.ZeroRow;
        //            int j = Turn.ZeroRunColumn;
        //            int ZeroStart = Turn.ZeroStart;
        //            int OneAttr = -1;
        //            int TwoAttr = -1;
        //            int OneStart = ZeroStart - 1;
        //            int TwoStart = OneStart;
        //            int Onej = -1;
        //            int Twoj = -1;                          //1,2格点所在行游程中第几游程            
        //            #region 边界越界控制
        //            int Onei = i;
        //            int Twoi = i - 1;
        //            if (Twoi < ImageBound.MinRow && OneStart >= ImageBound.MinColumn)    //即Twoi=-1
        //            {
        //                TwoAttr = -2;
        //            }
        //            else if (Twoi < ImageBound.MinRow && OneStart < ImageBound.MinColumn)
        //            {
        //                OneAttr = -2;
        //                TwoAttr = -2;
        //            }
        //            else if (Twoi >= ImageBound.MinRow && OneStart < ImageBound.MinColumn)
        //            {
        //                OneAttr = -2;
        //                TwoAttr = -2;
        //            }
        //            #endregion
        //            if (OneAttr != -2)
        //            {
        //                #region 获取1格点属性

        //                if (OneStart >= m_ImageRun[i][j].Start)
        //                {
        //                    OneAttr = m_ImageRun[i][j].Attr;
        //                    Onej = j;
        //                }
        //                else
        //                {
        //                    OneAttr = m_ImageRun[i][j - 1].Attr;
        //                    Onej = j - 1;
        //                }
        //                #endregion
        //            }
        //            #region 判断格点1与0号格点的属性异同关系
        //            GeoPoint Point = new GeoPoint();
        //            Point.X = TwoStart + 1;//3格点栅格[I,J]  ——> 矢量[J, I+1]
        //            Point.Y = m_ImageSize.Height - (i - 1 + 1);
        //            if (OneAttr != m_ImageRun[i][j].Attr)
        //            {
        //                m_ImageRun[i][j].LeftFlag = ReConstructFlag;
        //                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
        //                {
        //                    return true;
        //                }
        //                else
        //                {
        //                    PtList.Add(Point);
        //                    Turn.SetField(DirectionTypes.DT_DOWN, i, j, ZeroStart);
        //                    return false;
        //                }
        //            }
        //            #endregion
        //            else if (OneAttr == m_ImageRun[i][j].Attr)
        //            {
        //                if (TwoAttr != -2)
        //                {
        //                    #region 查找2格点属性
        //                    for (int k2 = 0; k2 < m_ImageRun[i - 1].Count - 1; k2++)
        //                    {
        //                        if (TwoStart >= m_ImageRun[i - 1][k2].Start && TwoStart < m_ImageRun[i - 1][k2 + 1].Start)
        //                        {
        //                            TwoAttr = m_ImageRun[i - 1][k2].Attr;
        //                            Twoj = k2;
        //                            break;
        //                        }
        //                    }
        //                    if (TwoAttr == -1)
        //                    {
        //                        TwoAttr = m_ImageRun[i - 1][m_ImageRun[i - 1].Count - 1].Attr;
        //                        Twoj = m_ImageRun[i - 1].Count - 1;
        //                    }
        //                    #endregion
        //                }
        //                #region 判断格点2与0格点属性异同关系
        //                if (TwoAttr != m_ImageRun[i][j].Attr)
        //                {
        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
        //                    {
        //                        return true;                                                //不存储坐标
        //                    }
        //                    else
        //                    {
        //                        Turn.SetField(DirectionTypes.DT_LEFT, i, Onej, OneStart);
        //                        return false;
        //                    }
        //                }
        //                else if (TwoAttr == m_ImageRun[i][j].Attr)
        //                {
        //                    m_ImageRun[i - 1][Twoj].RightFlag = ReConstructFlag;
        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
        //                    {
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        PtList.Add(Point);
        //                        Turn.SetField(DirectionTypes.DT_UP, i - 1, Twoj, TwoStart);
        //                        return false;
        //                    }
        //                }
        //                #endregion
        //            }
        //            return false;
        //        }
        //        /// </summary>
        //        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1]
        //        ///ChangeDirection：true 前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
        //        ///         与当前0格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）对应
        //        ///ChangeDirection:false 前一矢量点坐标[X坐标加1，Y坐标不变],（PtList[PtList.Count-1].X+1,PtList[PtList.Count-1].Y）
        //        ///         与当前0格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X+1）对应     
        //        public bool InRight(ref List<GeoPoint> PtList, ref TurnParam Turn)
        //        {
        //            #region 格点的栅格坐标
        //            //ChangeDirection：true
        //            //0格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X）    
        //            //1格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X+1）
        //            //2格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X+1）
        //            //ChangeDirection：false
        //            //0格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X+1）    
        //            //1格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X+2）
        //            //2格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X+2）
        //            #endregion
        //            int i = Turn.ZeroRow;
        //            int j = Turn.ZeroRunColumn;
        //            int ZeroStart = Turn.ZeroStart;
        //            int OneAttr = -1;
        //            int TwoAttr = -1;
        //            int OneStart = ZeroStart + 1;
        //            int TwoStart = OneStart;
        //            int Onej = -1;
        //            int Twoj = -1;
        //            #region 边界越界控制
        //            int Onei = i;
        //            int Twoi = i + 1;
        //            if (Twoi > ImageBound.MaxRow - 1 && OneStart <= (ImageBound.MaxColumn - 1))    //即Twoi=ImageBound.MaxRow
        //            {
        //                TwoAttr = -2;                    //此if条件在，最后一行出现，概率居中
        //            }
        //            else if (Twoi > ImageBound.MaxRow - 1 && OneStart > (ImageBound.MaxColumn - 1))
        //            {
        //                OneAttr = -2;                        //此if条件在，图像最角落才出现，概率低，放在后面
        //                TwoAttr = -2;
        //            }
        //            else if (Twoi <= (ImageBound.MaxRow - 1) && OneStart > (ImageBound.MaxColumn - 1))
        //            {
        //                OneAttr = -2;                         //此if条件在每行的末尾判断，出现概率高于第一个
        //                TwoAttr = -2;
        //            }
        //            #endregion                          //为了提高算法效率，可以调整下if条件顺序的
        //            if (OneAttr != -2)
        //            {
        //                #region 获取1格点属性
        //                if ((j + 1) < (m_ImageRun[i].Count))
        //                {
        //                    if (OneStart < m_ImageRun[i][j + 1].Start)
        //                    {
        //                        OneAttr = m_ImageRun[i][j].Attr;
        //                        Onej = j;
        //                    }
        //                    else
        //                    {
        //                        OneAttr = m_ImageRun[i][j + 1].Attr;
        //                        Onej = j + 1;
        //                    }
        //                }
        //                else
        //                {
        //                    OneAttr = m_ImageRun[i][j].Attr;
        //                    Onej = j;
        //                }
        //                #endregion
        //            }
        //            #region 判断格点1与0号格点的属性异同关系
        //            GeoPoint Point = new GeoPoint();
        //            Point.X = OneStart;     //1格点栅格[I,J]  ——> 矢量[J, I+1]
        //            Point.Y = m_ImageSize.Height - (i + 1);    //1格点（i，OneStart）
        //            if (OneAttr != m_ImageRun[i][j].Attr)
        //            {
        //                m_ImageRun[i][j].RightFlag = ReConstructFlag;
        //                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
        //                {
        //                    return true;
        //                }
        //                else
        //                {
        //                    PtList.Add(Point);
        //                    Turn.SetField(DirectionTypes.DT_UP, i, j, ZeroStart);
        //                    return false;
        //                }
        //            }
        //            #endregion
        //            else if (OneAttr == m_ImageRun[i][j].Attr)
        //            {
        //                if (TwoAttr != -2)
        //                {
        //                    #region 查找2格点属性
        //                    for (int k2 = 0; k2 < m_ImageRun[i + 1].Count - 1; k2++)
        //                    {
        //                        if (TwoStart >= m_ImageRun[i + 1][k2].Start && TwoStart < m_ImageRun[i + 1][k2 + 1].Start)
        //                        {
        //                            TwoAttr = m_ImageRun[i + 1][k2].Attr;
        //                            Twoj = k2;
        //                            break;
        //                        }
        //                    }
        //                    if (TwoAttr == -1)
        //                    {
        //                        TwoAttr = m_ImageRun[i + 1][m_ImageRun[i + 1].Count - 1].Attr;
        //                        Twoj = m_ImageRun[i + 1].Count - 1;
        //                    }
        //                    #endregion
        //                }
        //                #region 判断格点2与0格点属性异同关系
        //                if (TwoAttr != m_ImageRun[i][j].Attr)
        //                {
        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
        //                    {
        //                        return true;                                                //不存储坐标
        //                    }
        //                    else
        //                    {
        //                        Turn.SetField(DirectionTypes.DT_RIGHT, i, Onej, OneStart);
        //                        return false;
        //                    }
        //                }
        //                else if (TwoAttr == m_ImageRun[i][j].Attr)
        //                {
        //                    m_ImageRun[i + 1][Twoj].LeftFlag = ReConstructFlag;
        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
        //                    {
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        PtList.Add(Point);
        //                        Turn.SetField(DirectionTypes.DT_DOWN, i + 1, Twoj, TwoStart);
        //                        return false;
        //                    }
        //                }
        //                #endregion
        //            }
        //            return false;
        //        }
        //        #endregion  

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

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.Windows.Forms;
//using System.IO;
//using OSGeo.OGR;
//using GIS.Geometries;
//namespace GIS.RaToVe.ObjectVec
//{
//    public partial class ObjectOrientedVec : CollectionBase
//    {
//        //矢量坐标系：原点在图像左下角，X正向向右，Y正向向上,矢量化过程中采用为图像坐标系，然后再加上栅格原点：左下角的地理坐标偏移量
//        #region 变量定义
//        //public List<GeoPolygon> PlgList;
//        public int PlgIndex;   //用于内，外圈跟踪时，将多边形当前个数，赋予相应的栅格
//        #endregion
//        public enum DirectionTypes
//        {
//            DT_UP = 1,
//            DT_DOWN = 2,
//            DT_LEFT = 3,
//            DT_RIGHT = 4,
//            DT_OVER = 5
//        }
//        public class TurnParam
//        {
//            public DirectionTypes Direction;  //下一步转向哪个方向  
//            public int ZeroRow;       //下步0格点所处行、所处列          
//            //public int ZeroRunColumn;
//            public int ZeroStart;
//            public int ThreeAttr;    //前一步3格点属性         以下3个变量都用于压缩
//            public int NodeIndex;     //前一结点在多边形点数组中的下标号
//            public int ZeroAttr;    //当前0格点属性
//            public void SetField(DirectionTypes Dt, int Zr, /*int Zc,*/ int Zs)
//            {
//                Direction = Dt;
//                ZeroRow = Zr;
//                //ZeroRunColumn = Zc;
//                ZeroStart = Zs;
//            }
//            public void SetField2(int Ta, int Ni, int Za)
//            {
//                ThreeAttr = Ta;
//                NodeIndex = Ni;
//                ZeroAttr = Za;
//            }
//        }
//        public void ExtractionVecPolygon()
//        {
//            //PlgList = new List<GeoPolygon>();
//            PlgIndex = -1;  //多边形个数的索引号如果为0，代表还有一个多边形
//            TurnParam Turn = new TurnParam();

//            for (int i = m_ImageSize.Height - 1; i >= 0; i--)
//            {
//                for (int j = 0; j < m_ImageSize.Width-1; j++)
//                {
//                    int RingAttr = -1, RingElevMax = 0, RingElevMin = 10000;
//                    if (m_ImageRas[i][j].Attr == 0)   //属性为0的栅格不跟踪形成多边形
//                        continue;

//                    #region 提取一个多边形的外圈|所有内圈
//                    //外圈判断和提取
//                    if (m_ImageRas[i][j].Flag == -2)
//                    {
//                        List<GeoPolygon> SinglePlg = new List<GeoPolygon>();
//                        PlgIndex++;
//                        //ReConstructFlag = PlgList.Count;//当前多边形下标；因为在组建完多边形外环后才加入PlgList
//                        m_ImageRas[i][j].Flag = PlgIndex; //标记用于哪个多边形

//                        RingAttr = m_ImageRas[i][j].Attr;
//                        //RingElevMax = m_ImageRun[i][j].ElevationMax;
//                        //RingElevMin = m_ImageRun[i][j].ElevationMin;

//                        GeoPoint Pt = new GeoPoint();
//                        List<GeoPoint> PtList = new List<GeoPoint>();
//                        Pt.X = j;   //每个点坐标以当前0值栅格行列号得到
//                        Pt.Y = m_ImageSize.Height - (i + 1);
//                        PtList.Add(Pt);                //多边形第一点
//                        Turn.SetField(DirectionTypes.DT_UP, i, j);
//                        //Up函数和Right函数中分别得到外、内圈的第一个3格属性值

//                        bool OnePlgOver = false;
//                        while (!OnePlgOver)
//                        {
//                            if (Turn.Direction == DirectionTypes.DT_UP)
//                            {
//                                OnePlgOver = Up(ref PtList, ref Turn);
//                            }
//                            else if (Turn.Direction == DirectionTypes.DT_DOWN)
//                            {
//                                OnePlgOver = Down(ref PtList, ref Turn);
//                            }
//                            else if (Turn.Direction == DirectionTypes.DT_LEFT)
//                            {
//                                OnePlgOver = Left(ref PtList, ref Turn);
//                            }
//                            else if (Turn.Direction == DirectionTypes.DT_RIGHT)
//                            {
//                                OnePlgOver = Right(ref PtList, ref Turn);
//                            }
//                        }

//                        GeoLinearRing LinearRing = new GeoLinearRing(PtList);
//                        GeoPolygon Plg = new GeoPolygon(LinearRing);
//                        Plg.SetAttribute(RingAttr, RingElevMax, RingElevMin);
//                        //PlgList.Add(Plg);
//                    }
//                    #endregion
//                    else if (m_ImageRun[i][j].LeftFlag != -1 && m_ImageRun[i][j].RightFlag == -1)
//                    {
//                        ReConstructFlag = m_ImageRun[i][j].LeftFlag;
//                        m_ImageRun[i][j].RightFlag = ReConstructFlag; //标记用于哪个多边形

//                        GeoPoint Pt = new GeoPoint();
//                        List<GeoPoint> PtList = new List<GeoPoint>();
//                        if ((j + 1) < m_ImageRun[i].Count && (i + 1) <ImageBound.MaxRow) //判断是否出界
//                        {
//                            int ZeroStart = m_ImageRun[i][j + 1].Start;  //和当前的3格点所在列一致
//                            int ZeroAttr = -1;
//                            int Zeroj = -1;
//                            #region 查找格点0所在行第几游程，以及其属性
//                            for (int k = 0; k < m_ImageRun[i + 1].Count - 1; k++)
//                            {
//                                if (ZeroStart >= m_ImageRun[i + 1][k].Start && ZeroStart < m_ImageRun[i + 1][k + 1].Start)
//                                {
//                                    ZeroAttr = m_ImageRun[i + 1][k].Attr;
//                                    Zeroj = k;
//                                    break;
//                                }
//                            }
//                            if (ZeroAttr == -1)
//                            {
//                                ZeroAttr = m_ImageRun[i + 1][m_ImageRun[i + 1].Count - 1].Attr;
//                                Zeroj = m_ImageRun[i + 1].Count - 1;
//                            }
//                            #endregion
//                            if (ZeroAttr != m_ImageRun[i][j].Attr)
//                            {
//                                MessageBox.Show("内圈属性与外圈属性不等，出错");
//                                return;
//                            }
//                            Pt.X = m_ImageRun[i][j + 1].Start;  //判断，如果越界，则取最后一列值
//                            Pt.Y = m_ImageSize.Height - (i + 1);
//                            PtList.Add(Pt);                //多边形第一点
//                            Turn.SetField(DirectionTypes.DT_RIGHT, i + 1, Zeroj, ZeroStart);
//                            if (m_Compress)
//                            {
//                                #region 初始化3格点属性、初始点下标
//                                int ThreeAttribute = -1;
//                                #region 边界越界控制
//                                int Twoi = i;
//                                if (Twoi < ImageBound.MinRow)    //即Twoi=ImageBound.MaxRow
//                                    ThreeAttribute = -2;
//                                #endregion
//                                else //未越界情形，查询3格点属性
//                                {
//                                    if (ThreeAttribute != -2)
//                                    {
//                                        #region 查找3格点属性
//                                        for (int k2 = 0; k2 < m_ImageRun[i].Count - 1; k2++)
//                                        {
//                                            if (ZeroStart >= m_ImageRun[i][k2].Start && ZeroStart < m_ImageRun[i][k2 + 1].Start)
//                                            {
//                                                ThreeAttribute = m_ImageRun[i][k2].Attr;
//                                                break;
//                                            }
//                                        }
//                                        if (ThreeAttribute == -1)
//                                        {
//                                            ThreeAttribute = m_ImageRun[i][m_ImageRun[i].Count - 1].Attr;
//                                        }
//                                        #endregion
//                                    }
//                                }
//                                Turn.SetField2(ThreeAttribute, 0, ZeroAttr);
//                                #endregion
//                            }
//                            bool OnePlgOver = false;
//                            while (!OnePlgOver)
//                            {
//                                if (Turn.Direction == DirectionTypes.DT_UP)
//                                {
//                                    int RunElevMin = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMin;
//                                    int RunElevMax = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMax;
//                                    if (RunElevMin < RingElevMin)
//                                        RingElevMin = RunElevMin;
//                                    if (RunElevMax > RingElevMax)
//                                        RingElevMax = RunElevMax;
//                                    OnePlgOver = Up(ref PtList, ref Turn);
//                                }
//                                else if (Turn.Direction == DirectionTypes.DT_DOWN)
//                                {
//                                    int RunElevMin = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMin;
//                                    int RunElevMax = m_ImageRun[Turn.ZeroRow][Turn.ZeroRunColumn].ElevationMax;
//                                    if (RunElevMin < RingElevMin)
//                                        RingElevMin = RunElevMin;
//                                    if (RunElevMax > RingElevMax)
//                                        RingElevMax = RunElevMax;
//                                    OnePlgOver = Down(ref PtList, ref Turn);
//                                }
//                                else if (Turn.Direction == DirectionTypes.DT_LEFT)
//                                {
//                                    OnePlgOver = Left(ref PtList, ref Turn);
//                                }
//                                else if (Turn.Direction == DirectionTypes.DT_RIGHT)
//                                {
//                                    OnePlgOver = Right(ref PtList, ref Turn);
//                                }
//                            }
//                            if (m_Compress)
//                            {
//                                if (Turn.NodeIndex != 0)
//                                {
//                                    #region 返回起点-最后一段的压缩
//                                    {
//                                        //DP算法调用
//                                        List<GeoPoint> RePtList = new List<GeoPoint>();
//                                        RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, Turn.NodeIndex, PtList.Count - 1);
//                                        PtList.RemoveRange(Turn.NodeIndex, PtList.Count - Turn.NodeIndex);
//                                        PtList.AddRange(RePtList);
//                                    }
//                                    #endregion
//                                }
//                                else if (Turn.NodeIndex == 0)
//                                {
//                                    #region 全为坐标点形成的多边形的压缩
//                                    //有结点形成的多边形在上面4个函数中做了压缩
//                                    List<GeoPoint> RePtList = new List<GeoPoint>();
//                                    double MaxDistance = -1;
//                                    int IndexM = -1;
//                                    for (int Kk = 0; Kk < PtList.Count; Kk++) //为保证内外全重合情况，压缩结果相同，需要分为两段
//                                    {
//                                        double Dis = PtList[0].DistanceTo(PtList[Kk]);
//                                        if (Dis > MaxDistance)
//                                        {
//                                            MaxDistance = Dis;
//                                            IndexM = Kk;
//                                        }
//                                    }
//                                    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, IndexM, PtList.Count - 1);
//                                    PtList.RemoveRange(IndexM, PtList.Count - IndexM);
//                                    PtList.AddRange(RePtList);
//                                    RePtList.Clear();

//                                    RePtList = DPReduction.DouglasPeuckerReduction(PtList, ToleranceDis, 0, IndexM);
//                                    RePtList.RemoveAt(RePtList.Count - 1); //中间分隔点只存储一次
//                                    PtList.RemoveRange(0, IndexM); //因为前面已经移除掉下标为Indexm的点了，因此此时移除的会少一个点：本来该是0,Indexm+1
//                                    PtList.InsertRange(0, RePtList);
//                                    #endregion
//                                }
//                            }
//                            GeoLinearRing LinearRing = new GeoLinearRing(PtList);
//                            PlgList[ReConstructFlag].InteriorRings.Add(LinearRing);  //增加到内环中
//                        }
//                        else
//                        {
//                            Pt.X = ImageBound.MaxColumn; //算法问题：向右已没有任何游程，如何处理？-
//                            MessageBox.Show("内圈找到入口点，却向右到了边界，出错");
//                            return;               //这个是错误的，正确标记了游程，不应存在此问题，解决---
//                        }
//                    }
//                }
//            }
//        }
//        public void TopologyReconstruct() //拓扑建立,已与多边形提取一起完成了
//        {
//            #region 测试GDAL库写Shape时对多边形类型的支持代码
//            //////////////////////////////////////////
//            //List<List<GeoPoint> >Listvertices = new List<List<GeoPoint>>();
//            //for (int i = 0; i < 4; i++)
//            //{
//            //    List<GeoPoint> vertices = new List<GeoPoint>();
//            //    GeoPoint vertice1 = new GeoPoint();
//            //    GeoPoint vertice2 = new GeoPoint();
//            //    GeoPoint vertice3 = new GeoPoint();
//            //    GeoPoint vertice4 = new GeoPoint();
//            //    vertice1.X = 0 + i * 10;
//            //    vertice1.Y = 0 + i * 10;
//            //    vertice2.X = 100 - i * 10;
//            //    vertice2.Y = 0 + i * 10;
//            //    vertice3.X = 100 - i * 10;
//            //    vertice3.Y = 100 - i * 10;
//            //    vertice4.X = 0 + i * 10;
//            //    vertice4.Y = 100 - i * 10;
//            //    vertices.Add(vertice1);
//            //    vertices.Add(vertice2);
//            //    vertices.Add(vertice3);
//            //    vertices.Add(vertice4);
//            //    Listvertices.Add(vertices);
//            //}
//            //GeoLinearRing ExteriorRings = new GeoLinearRing(Listvertices[0]);
//            //for (int j = 1; j < 4; j++)
//            //{
//            //    GeoLinearRing Rings = new GeoLinearRing(Listvertices[j]);
//            //    InteriorRings.Add(Rings);
//            //}
//            //GeoPolygon Plg = new GeoPolygon(ExteriorRings, InteriorRings);
//            //PlgList.Add(Plg);
//            /////////////////////////////////////////
//            #endregion
//        }

//        #region  Up Down Left Right函数
//        /// <summary>
//        /// Up:向上跟踪一步：步长为1个像素
//        /// </summary>
//        ///params：组成多边形的点集；0格点所在游程行数i, 所在行中的第j游程,所在的栅格列数
//        public bool Up(ref List<GeoPoint> PtList, ref TurnParam Turn)
//        {
//            #region 格点的栅格坐标
//            //0格点：（i,m_ImageRun[i][j].Start）
//            //1格点：（i-1,m_ImageRun[i][j].Start）
//            //2格点：（i-1,m_ImageRun[i][j].Start-1）
//            //4像素窗口中{右下角栅格行列号}与{该栅格左下角点矢量坐标对应关系}：
//            //          栅格坐标[I,J]  ——>   矢量点[J, I+1]
//            #endregion
//            int i = Turn.ZeroRow;
//            //int j = Turn.ZeroRunColumn;
//            int ZeroStart = Turn.ZeroStart;
//            int OneAttr = -1;
//            int TwoAttr = -1;
//            //int ThreeAttr = -1;
//            int OneStart = ZeroStart;
//            int TwoStart = OneStart - 1;
//            //int Onej = -1;
//            //int Twoj = -1;   //1,2格点所在行游程中第几游程
//            #region 1、2格点边界越界控制
//            int Onei = i - 1;
//            int Twoi = i - 1;
//            if (Onei < ImageBound.MinRow)    //即Onei=-1  行越界
//            {
//                OneAttr = -2;
//                TwoAttr = -2;
//            }
//            else if (Onei >= ImageBound.MinRow && OneStart == ImageBound.MinColumn)
//            {                                             //列越界
//                OneAttr = m_ImageRas[i - 1][0].Attr;
//                //Onej = 0;
//                TwoAttr = -2;
//            }
//            #endregion
//            else //未越界情形，查询格点1和2属性，以及3属性--越界与未越界转向关系一起判断
//            {
//                #region 格点1属性
//                OneAttr = m_ImageRas[i - 1][ZeroStart].Attr;
//                #endregion
//            }

//            GeoPoint Point = new GeoPoint();
//            Point.X = OneStart;     //1格点栅格[I,J]  ——> 矢量[J, I+1]
//            Point.Y = m_ImageSize.Height - (i - 1 + 1);        //加快算法：用OneStart而不是m_ImageRun[i][j].Start

//            if (OneAttr != m_ImageRas[i][ZeroStart].Attr)               //无需标记游程
//            {
//                #region 判断格点1与格点0属性异同关系
//                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//                {
//                    PtList.Add(Point);                    //存储坐标
//                    return true;
//                }
//                else
//                {
//                    PtList.Add(Point);                    //存储坐标
//                    Turn.SetField(DirectionTypes.DT_RIGHT, i, ZeroStart); //向右
//                    return false;
//                }
//                #endregion
//            }
//            else if (OneAttr == m_ImageRas[i][ZeroStart].Attr)
//            {
//                if (TwoAttr != -2)
//                {
//                    #region 获取2格点属性
//                    //if (TwoStart >= m_ImageRun[i - 1][Onej].Start)
//                    //{
//                    TwoAttr = m_ImageRas[i - 1][TwoStart].Attr;
//                    //    Twoj = Onej;
//                    //}
//                    //else
//                    //{
//                    //    TwoAttr = m_ImageRun[i - 1][Onej - 1].Attr;
//                    //    Twoj = Onej - 1;
//                    //}
//                    #endregion
//                }
//                #region 判断格点2与0格点属性异同关系
//                if (TwoAttr != m_ImageRas[i][ZeroStart].Attr)
//                {
//                    //m_ImageRun[i - 1][Onej].LeftFlag = ReConstructFlag; //标记游程   应该加上如果已经为真，即抛出异常
//                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        return true;
//                    }
//                    else                                             //不存储坐标
//                    {
//                        Turn.SetField(DirectionTypes.DT_UP, i - 1, OneStart);//向上
//                        return false;
//                    }
//                }
//                else if (TwoAttr == m_ImageRas[i][ZeroStart].Attr)
//                {
//                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //未返回起点
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        return true;
//                    }
//                    else
//                    {

//                        PtList.Add(Point);                    //存储坐标
//                        Turn.SetField(DirectionTypes.DT_LEFT, i - 1,TwoStart);//向左
//                        return false;
//                    }
//                }
//                #endregion
//            }
//            return false;
//        }
//        /// <summary>
//        /// 下一游程起始点来确定栅格点行列号
//        /// </summary>
//        public bool Down(ref List<GeoPoint> PtList, ref TurnParam Turn)
//        {
//            #region 格点的栅格坐标
//            //0格点：（i,m_ImageRun[i][j+1].Start-1）
//            //1格点：（i+1,m_ImageRun[i][j+1].Start-1）
//            //2格点：（i+1,m_ImageRun[i][j+1].Start）
//            #endregion
//            int i = Turn.ZeroRow;
//            //int j = Turn.ZeroRunColumn;
//            int ZeroStart = Turn.ZeroStart;
//            int OneAttr = -1;
//            int TwoAttr = -1;
//            //int ThreeAttr = -1;
//            int OneStart = ZeroStart;
//            int TwoStart = OneStart + 1;
//            //int Onej = -1;
//            //int Twoj = -1;                          //1,2格点所在行游程中第几游程
//            #region 1、2边界越界控制
//            int Onei = i + 1;
//            int Twoi = i + 1;
//            if (Onei > ImageBound.MaxRow - 1)    //即Onei=ImageBound.MaxRow
//            {
//                OneAttr = -2;
//                TwoAttr = -2;
//            }
//            else if (Onei <= (ImageBound.MaxRow - 1) && OneStart == (ImageBound.MaxColumn - 1))
//            {
//                OneAttr = m_ImageRas[i + 1][OneStart].Attr;
//                //Onej = m_ImageRun[i + 1].Count - 1;
//                TwoAttr = -2;
//            }
//            #endregion
//            else  //未越界情形，查询格点1和2属性--越界与未越界转向关系一起判断
//            {
//                #region 查找格点1属性
//                OneAttr = m_ImageRas[i + 1][OneStart].Attr;
//                #endregion
//            }

//            GeoPoint Point = new GeoPoint();
//            Point.X = TwoStart;  //3格点栅格[I,J]  ——> 矢量[J, I+1]  3格点与2格点列值相同
//            Point.Y = m_ImageSize.Height - (i + 1);
//            //判断的结点，是此步Point点的，前一点。压缩的是上一结点和此结点间的点。不连此步的Point点

//            if (OneAttr != m_ImageRas[i][ZeroStart].Attr)
//            {
//                #region 判断格点1与格点0属性异同关系
//                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//                {
//                    PtList.Add(Point);                    //存储坐标
//                    return true;
//                }
//                else
//                {
//                    PtList.Add(Point);                    //存储坐标
//                    Turn.SetField(DirectionTypes.DT_LEFT, i,ZeroStart);//向左
//                    return false;
//                }
//                #endregion
//            }
//            else if (OneAttr == m_ImageRas[i][ZeroStart].Attr)
//            {
//                if (TwoAttr != -2)
//                {
//                    #region 获取2格点属性
//                    TwoAttr = m_ImageRas[i + 1][TwoStart].Attr;
//                    #endregion
//                }
//                #region 判断格点2与0格点属性异同关系
//                if (TwoAttr != m_ImageRas[i][ZeroStart].Attr)
//                {
//                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        return true;
//                    }
//                    else                                             //不存储坐标
//                    {
//                        Turn.SetField(DirectionTypes.DT_DOWN, i + 1, OneStart);
//                        return false;
//                    }
//                }
//                else if (TwoAttr == m_ImageRas[i][ZeroStart].Attr)
//                {
//                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //未返回起点
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        return true;
//                    }
//                    else
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        Turn.SetField(DirectionTypes.DT_RIGHT, i + 1,TwoStart);
//                        return false;
//                    }
//                }
//                #endregion
//            }
//            return false;
//        }
//        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1] 4像素右下角栅格与该栅格左小角点坐标对应
//        ///ChangeDirection:true时-- 前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
//        ///         与当前0格点右边一个栅格坐标对应（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）
//        ///ChangeDirection:false--前一矢量点坐标[X坐标减1，Y坐标不变],（PtList[PtList.Count-1].X-1,PtList[PtList.Count-1].Y）
//        ///          后与当前0格点右边一个栅格坐标对应（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X-1）     
//        public bool Left(ref List<GeoPoint> PtList, ref TurnParam Turn)
//        {
//            #region 格点的栅格坐标
//            //changeDirection:true
//            //0格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-1）    
//            //1格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-1-1）
//            //2格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-1-1）
//            //changedirection:false
//            //0格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-2）    
//            //1格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-3）
//            //2格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-3）
//            #endregion
//            int i = Turn.ZeroRow;
//            //int j = Turn.ZeroRunColumn;
//            int ZeroStart = Turn.ZeroStart;
//            int OneAttr = -1;
//            int TwoAttr = -1;
//            //int ThreeAttr = -1;  //以下两个变量用于矢量压缩
//            //int Threej = -1;
//            int OneStart = ZeroStart - 1;
//            int TwoStart = OneStart;
//            //int Onej = -1;
//            //int Twoj = -1;                          //1,2格点所在行游程中第几游程
//            #region 1、2、3边界越界控制
//            int Onei = i;
//            int Twoi = i + 1;
//            if (Twoi > ImageBound.MaxRow - 1 && OneStart < ImageBound.MinColumn)
//            {
//                OneAttr = -2;                      //行、列越界
//                TwoAttr = -2;                      //即Twoi=ImageBound.MaxRow  OneStart=-1
//                ThreeAttr = -2;
//            }
//            else if (Twoi > ImageBound.MaxRow - 1 && OneStart >= ImageBound.MinColumn)
//            {
//                TwoAttr = -2;                    //行越界
//                ThreeAttr = -2;
//            }
//            else if (Twoi <= (ImageBound.MaxRow - 1) && OneStart < ImageBound.MinColumn)
//            {
//                OneAttr = -2;                     //列越界
//                TwoAttr = -2;
//            }
//            #endregion
//            if (OneAttr != -2)
//            {
//                #region 获取1格点属性
//                //if (OneStart >= m_ImageRun[i][j].Start)
//                //{
//                    OneAttr = m_ImageRas[i][OneStart].Attr;
//                    //Onej = j;
//                //}
//                //else
//                //{
//                //    OneAttr = m_ImageRun[i][j - 1].Attr;  //j-1的一定存在了，已排除边界越界
//                //    Onej = j - 1;
//                //}
//                #endregion
//            }
//            GeoPoint Point = new GeoPoint();
//            Point.X = OneStart + 1;//0格点栅格[I,J]  ——> 矢量[J, I+1]
//            Point.Y = m_ImageSize.Height - (i + 1);
//            if (OneAttr != m_ImageRas[i][ZeroStart].Attr)
//            {
//                #region 判断格点1与0号格点的属性异同关系
//                //m_ImageRun[i][j].LeftFlag = ReConstructFlag;
//                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
//                {
//                    PtList.Add(Point);                    //存储坐标
//                    return true;
//                }
//                else
//                {
//                    PtList.Add(Point);                    //存储坐标
//                    Turn.SetField(DirectionTypes.DT_UP, i, ZeroStart);
//                    return false;
//                }
//                #endregion
//            }
//            else if (OneAttr == m_ImageRas[i][ZeroStart].Attr)
//            {
//                if (TwoAttr != -2)
//                {
//                    #region 查找2格点属性
//                    //for (int k2 = 0; k2 < m_ImageRun[i + 1].Count - 1; k2++)
//                    //{
//                    //    if (TwoStart >= m_ImageRun[i + 1][k2].Start && TwoStart < m_ImageRun[i + 1][k2 + 1].Start)
//                    //    {
//                    TwoAttr = m_ImageRas[i + 1][TwoStart].Attr;
//                    //        Twoj = k2;
//                    //        break;
//                    //    }
//                    //}
//                    //if (TwoAttr == -1)
//                    //{
//                    //    TwoAttr = m_ImageRun[i + 1][m_ImageRun[i + 1].Count - 1].Attr;
//                    //    Twoj = m_ImageRun[i + 1].Count - 1;
//                    //}
//                    #endregion
//                }

//                #region 判断格点2与0格点属性异同关系
//                if (TwoAttr != m_ImageRas[i][ZeroStart].Attr)
//                {
//                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        return true;                                                //不存储坐标
//                    }
//                    else
//                    {
//                        Turn.SetField(DirectionTypes.DT_LEFT, i, OneStart);
//                        return false;
//                    }
//                }
//                else if (TwoAttr == m_ImageRas[i][ZeroStart].Attr)
//                {
//                    //m_ImageRun[i + 1][Twoj].RightFlag = ReConstructFlag;
//                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        return true;
//                    }
//                    else
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        Turn.SetField(DirectionTypes.DT_DOWN, i + 1,TwoStart);
//                        return false;
//                    }
//                }
//                #endregion
//            }
//            return false;
//        }
//        /// </summary>
//        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1] 4像素右下角栅格与该栅格左小角点坐标对应
//        ///ChangeDirection:true：前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
//        ///         与当前3格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）对应
//        ///ChangeDirection:false前一矢量点坐标[X坐标加1，Y坐标不变],（PtList[PtList.Count-1].X+1,PtList[PtList.Count-1].Y）
//        ///         与当前3格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X+1）对应
//        public bool Right(ref List<GeoPoint> PtList, ref TurnParam Turn)
//        {
//            #region 格点的栅格坐标
//            //changeDireciton:true
//            //0格点：（PtList[PtList.Count-1].Y,PtList[PtList.Count-1].X）    
//            //1格点：（PtList[PtList.Count-1].Y,PtList[PtList.Count-1].X+1）
//            //2格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X+1）
//            //changeDirection:false
//            //0格点：（PtList[PtList.Count-1].Y,PtList[PtList.Count-1].X+1）    
//            //1格点：（PtList[PtList.Count-1].Y,PtList[PtList.Count-1].X+2）
//            //2格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X+2）
//            #endregion
//            int i = Turn.ZeroRow;
//            //int j = Turn.ZeroRunColumn;
//            int ZeroStart = Turn.ZeroStart;
//            int OneAttr = -1;
//            int TwoAttr = -1;
//            //int ThreeAttr = -1; //以下两个变量用于压缩
//            //int Threej = -1;
//            int OneStart = ZeroStart + 1;
//            int TwoStart = OneStart;
//            //int Onej = -1;
//            //int Twoj = -1;                          //1,2格点所在行游程中第几游程            
//            #region 边界越界控制
//            int Onei = i;
//            int Twoi = i - 1;
//            if (Twoi < ImageBound.MinRow && OneStart > (ImageBound.MaxColumn - 1))    //即Twoi=ImageBound.MaxRow
//            {
//                OneAttr = -2;
//                TwoAttr = -2;
//                //ThreeAttr = -2;
//            }
//            else if (Twoi < ImageBound.MinRow && OneStart <= (ImageBound.MaxColumn - 1))
//            {
//                TwoAttr = -2;
//                //ThreeAttr = -2;
//            }
//            else if (Twoi >= ImageBound.MinRow && OneStart > (ImageBound.MaxColumn - 1))
//            {
//                OneAttr = -2;
//                TwoAttr = -2;
//            }
//            #endregion
//            if (OneAttr != -2)
//            {
//                #region 获取1格点属性
//                //if ((j + 1) < (m_ImageRun[i].Count))
//                //{
//                //    if (OneStart < m_ImageRun[i][j + 1].Start)
//                //    {
//                        OneAttr = m_ImageRas[i][OneStart].Attr;
//                        //Onej = j;
//                    //}
//                    //else
//                    //{
//                    //    OneAttr = m_ImageRun[i][j + 1].Attr;
//                    //    Onej = j + 1;
//                    //}
//                //}
//                //else
//                //{
//                //    OneAttr = m_ImageRun[i][j].Attr;
//                //    Onej = j;
//                //}
//                #endregion
//            }
//            GeoPoint Point = new GeoPoint();
//            Point.X = TwoStart;     //2格点栅格[I,J]  ——> 矢量[J, I+1]
//            Point.Y = m_ImageSize.Height - (i - 1 + 1);        //2格点（i-1，TwoStart）
//            if (OneAttr != m_ImageRas[i][ZeroStart].Attr)
//            {
//                #region 判断格点1与0号格点的属性异同关系
//                //m_ImageRun[i][j].RightFlag = ReConstructFlag;
//                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
//                {
//                    PtList.Add(Point);                    //存储坐标
//                    return true;
//                }
//                else
//                {
//                    PtList.Add(Point);                    //存储坐标
//                    Turn.SetField(DirectionTypes.DT_DOWN, i, ZeroStart);
//                    return false;
//                }
//                #endregion
//            }
//            else if (OneAttr == m_ImageRas[i][ZeroStart].Attr)
//            {
//                if (TwoAttr != -2)
//                {
//                    #region 查找2格点属性
//                    //for (int k2 = 0; k2 < m_ImageRun[i - 1].Count - 1; k2++)
//                    //{
//                    //    if (TwoStart >= m_ImageRun[i - 1][k2].Start && TwoStart < m_ImageRun[i - 1][k2 + 1].Start)
//                    //    {
//                    TwoAttr = m_ImageRas[i - 1][TwoStart].Attr;
//                    //        Twoj = k2;
//                    //        break;
//                    //    }
//                    //}
//                    //if (TwoAttr == -1)
//                    //{
//                    //    TwoAttr = m_ImageRun[i - 1][m_ImageRun[i - 1].Count - 1].Attr;
//                    //    Twoj = m_ImageRun[i - 1].Count - 1;
//                    //}
//                    #endregion
//                }

//                #region 判断格点2与0格点属性异同关系
//                if (TwoAttr != m_ImageRas[i][ZeroStart].Attr)
//                {
//                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        return true;                                                //不存储坐标
//                    }
//                    else
//                    {
//                        Turn.SetField(DirectionTypes.DT_RIGHT, i,OneStart);
//                        return false;
//                    }
//                }
//                else if (TwoAttr == m_ImageRas[i][ZeroStart].Attr)
//                {
//                    //m_ImageRun[i - 1][Twoj].LeftFlag = ReConstructFlag;
//                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        return true;
//                    }
//                    else
//                    {
//                        PtList.Add(Point);                    //存储坐标
//                        Turn.SetField(DirectionTypes.DT_UP, i - 1, TwoStart);
//                        return false;
//                    }
//                }
//                #endregion
//            }
//            return false;
//        }
//        #endregion
//        //#region  InUp InDown InLeft InRight函数-内圈
//        //        /// </summary>
//        //        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1]
//        //        ///InUp函数：前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
//        //        ///         与当前3格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）对应
//        //        public bool InUp(ref List<GeoPoint> PtList, ref TurnParam Turn)
//        //        {
//        //            #region 格点的栅格坐标//用坐标表示
//        //            //0格点：（i,PtList[PtList.Count-1].X-1）                    
//        //            //1格点：（i-1,PtList[PtList.Count-1].X-1）
//        //            //2格点：（i-1,PtList[PtList.Count-1].X）

//        //            //int OneAttr = -1;
//        //            //int TwoAttr = -1;
//        //            //int OneStart = PtList[PtList.Count - 1].X - 1;
//        //            //int TwoStart = PtList[PtList.Count - 1].X;
//        //            #endregion
//        //            #region 格点的栅格坐标-游程表示
//        //            //0格点：（i,m_ImageRun[i][j+1].Start-1）   //用下一游程表示
//        //            //1格点：（i-1,m_ImageRun[i][j+1].Start-1）
//        //            //2格点：（i-1,m_ImageRun[i][j+1].Start）
//        //            #endregion
//        //            int i = Turn.ZeroRow;
//        //            int j = Turn.ZeroRunColumn;
//        //            int ZeroStart = Turn.ZeroStart;
//        //            int OneAttr = -1;
//        //            int TwoAttr = -1;
//        //            int OneStart = ZeroStart;
//        //            int TwoStart = OneStart + 1;
//        //            int Onej = -1;
//        //            int Twoj = -1;                          //1,2格点所在行游程中第几游程
//        //            #region 边界越界控制
//        //            int Onei = i - 1;
//        //            int Twoi = i - 1;
//        //            if (Onei < ImageBound.MinRow)    //即Onei=-1  行越界
//        //            {
//        //                OneAttr = -2;
//        //                TwoAttr = -2;
//        //            }
//        //            else if (Onei >= ImageBound.MinRow && OneStart == (ImageBound.MaxColumn - 1))
//        //            {                                             //列越界
//        //                OneAttr = m_ImageRun[i - 1][m_ImageRun[i - 1].Count - 1].Attr;
//        //                Onej = m_ImageRun[i - 1].Count - 1;
//        //                TwoAttr = -2;
//        //            }
//        //            #endregion
//        //            else
//        //            {
//        //                #region 查找格点1属性
//        //                for (int k = 0; k < m_ImageRun[i - 1].Count - 1; k++)
//        //                {
//        //                    if (OneStart >= m_ImageRun[i - 1][k].Start && OneStart < m_ImageRun[i - 1][k + 1].Start)
//        //                    {
//        //                        OneAttr = m_ImageRun[i - 1][k].Attr;
//        //                        Onej = k;
//        //                        break;
//        //                    }
//        //                }
//        //                if (OneAttr == -1)
//        //                {
//        //                    OneAttr = m_ImageRun[i - 1][m_ImageRun[i - 1].Count - 1].Attr;
//        //                    Onej = m_ImageRun[i - 1].Count - 1;
//        //                }
//        //                #endregion
//        //            }
//        //            #region 判断格点1与格点0属性异同关系
//        //            //Point.X = PtList[PtList.Count - 1].X; //用上一点坐标表示，为加快算法执行，可直接用i,j表达
//        //            //Point.Y = PtList[PtList.Count - 1].Y - 1;
//        //            //Point.X = PtList[PtList.Count - 1].X;     //用上一点坐标表示，或者是 用2格点栅格[I,J]  ——> 矢量[J, I+1]---加快算法速度
//        //            //Point.Y = i - 1 + 1;
//        //            GeoPoint Point = new GeoPoint();
//        //            Point.X = TwoStart;     //用2格点栅格[I,J]  ——> 矢量[J, I+1]---加快算法速度
//        //            Point.Y = m_ImageSize.Height - (i - 1 + 1);
//        //            if (OneAttr != m_ImageRun[i][j].Attr)               //无需标记游程
//        //            {
//        //                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//        //                {
//        //                    return true;
//        //                }
//        //                else
//        //                {
//        //                    PtList.Add(Point);                    //存储坐标
//        //                    Turn.SetField(DirectionTypes.DT_LEFT, i, j, ZeroStart);
//        //                    return false;
//        //                }
//        //            }
//        //            #endregion
//        //            else if (OneAttr == m_ImageRun[i][j].Attr)
//        //            {
//        //                if (TwoAttr != -2)
//        //                {
//        //                    #region 获取2格点属性
//        //                    if ((Onej + 1) < (m_ImageRun[i - 1].Count))
//        //                    {
//        //                        if (TwoStart < m_ImageRun[i - 1][Onej + 1].Start)
//        //                        {
//        //                            TwoAttr = m_ImageRun[i - 1][Onej].Attr;
//        //                            Twoj = Onej;
//        //                        }
//        //                        else
//        //                        {
//        //                            TwoAttr = m_ImageRun[i - 1][Onej + 1].Attr;
//        //                            Twoj = Onej + 1;
//        //                        }
//        //                    }
//        //                    else
//        //                    {
//        //                        TwoAttr = m_ImageRun[i - 1][Onej].Attr;
//        //                        Twoj = Onej;
//        //                    }
//        //                    #endregion
//        //                }
//        //                #region 判断格点2与0格点属性异同关系
//        //                if (TwoAttr != m_ImageRun[i][j].Attr)
//        //                {
//        //                    m_ImageRun[i - 1][Onej].RightFlag = ReConstructFlag; //标记游程   应该加上如果已经为真，即抛出异常
//        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//        //                    {
//        //                        return true;
//        //                    }
//        //                    else                                             //不存储坐标
//        //                    {
//        //                        Turn.SetField(DirectionTypes.DT_UP, i - 1, Onej, OneStart);
//        //                        return false;
//        //                    }

//        //                }
//        //                else if (TwoAttr == m_ImageRun[i][j].Attr)
//        //                {
//        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //未返回起点
//        //                    {
//        //                        return true;
//        //                    }
//        //                    else
//        //                    {
//        //                        PtList.Add(Point);                       //存储坐标
//        //                        Turn.SetField(DirectionTypes.DT_RIGHT, i - 1, Twoj, TwoStart);
//        //                        return false;
//        //                    }
//        //                }
//        //                #endregion
//        //            }
//        //            return false;
//        //        }
//        //        /// </summary>
//        //        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1]
//        //        ///InDown函数：前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
//        //        ///         与当前0格点正上方格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）对应
//        //        public bool InDown(ref List<GeoPoint> PtList, ref TurnParam Turn)
//        //        {
//        //            #region 格点的栅格坐标
//        //            //0格点：（i,m_ImageRun[i][j].Start）
//        //            //1格点：（i+1,m_ImageRun[i][j].Start）
//        //            //2格点：（i+1,m_ImageRun[i][j].Start-1）
//        //            #endregion
//        //            int i = Turn.ZeroRow;
//        //            int j = Turn.ZeroRunColumn;
//        //            int ZeroStart = Turn.ZeroStart;
//        //            int OneAttr = -1;
//        //            int TwoAttr = -1;
//        //            int OneStart = ZeroStart;
//        //            int TwoStart = OneStart - 1;
//        //            int Onej = -1;
//        //            int Twoj = -1;                          //1,2格点所在行游程中第几游程 
//        //            #region 边界越界控制
//        //            int Onei = i + 1;
//        //            int Twoi = i + 1;
//        //            if (Onei > ImageBound.MaxRow - 1)    //即Onei=ImageBound.MaxRow
//        //            {
//        //                OneAttr = -2;
//        //                TwoAttr = -2;
//        //            }
//        //            else if (Onei <= (ImageBound.MaxRow - 1) && OneStart == ImageBound.MinColumn)
//        //            {
//        //                OneAttr = m_ImageRun[i + 1][0].Attr;
//        //                Onej = 0;
//        //                TwoAttr = -2;
//        //            }
//        //            #endregion
//        //            else
//        //            {
//        //                #region 查找格点1属性
//        //                for (int k = 0; k < m_ImageRun[i + 1].Count - 1; k++)
//        //                {
//        //                    if (OneStart >= m_ImageRun[i + 1][k].Start && OneStart < m_ImageRun[i + 1][k + 1].Start)
//        //                    {
//        //                        OneAttr = m_ImageRun[i + 1][k].Attr;
//        //                        Onej = k;
//        //                        break;
//        //                    }
//        //                }
//        //                if (OneAttr == -1)
//        //                {
//        //                    OneAttr = m_ImageRun[i + 1][m_ImageRun[i + 1].Count - 1].Attr;
//        //                    Onej = m_ImageRun[i + 1].Count - 1;
//        //                }
//        //                #endregion
//        //            }
//        //            #region 判断格点1与格点0属性异同关系
//        //            GeoPoint Point = new GeoPoint();
//        //            Point.X = OneStart;  //0格点栅格[I,J]  ——> 矢量[J, I+1]
//        //            Point.Y = m_ImageSize.Height - (i + 1);
//        //            if (OneAttr != m_ImageRun[i][j].Attr)
//        //            {
//        //                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//        //                {
//        //                    return true;
//        //                }
//        //                else
//        //                {
//        //                    PtList.Add(Point);                    //存储坐标
//        //                    Turn.SetField(DirectionTypes.DT_RIGHT, i, j, ZeroStart);
//        //                    return false;
//        //                }
//        //            }
//        //            #endregion
//        //            else if (OneAttr == m_ImageRun[i][j].Attr)
//        //            {
//        //                if (TwoAttr != -2)
//        //                {
//        //                    #region 获取2格点属性
//        //                    if (TwoStart >= m_ImageRun[i + 1][Onej].Start)
//        //                    {
//        //                        TwoAttr = m_ImageRun[i + 1][Onej].Attr;
//        //                        Twoj = Onej;
//        //                    }
//        //                    else
//        //                    {
//        //                        TwoAttr = m_ImageRun[i + 1][Onej - 1].Attr;
//        //                        Twoj = Onej - 1;
//        //                    }
//        //                    #endregion
//        //                }
//        //                #region 判断格点2与0格点属性异同关系
//        //                if (TwoAttr != m_ImageRun[i][j].Attr)
//        //                {
//        //                    m_ImageRun[i + 1][Onej].LeftFlag = ReConstructFlag;
//        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//        //                    {
//        //                        return true;
//        //                    }
//        //                    else                                             //不存储坐标
//        //                    {
//        //                        Turn.SetField(DirectionTypes.DT_DOWN, i + 1, Onej, OneStart);
//        //                        return false;
//        //                    }

//        //                }
//        //                else if (TwoAttr == m_ImageRun[i][j].Attr)
//        //                {
//        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //未返回起点
//        //                    {
//        //                        return true;
//        //                    }
//        //                    else
//        //                    {
//        //                        PtList.Add(Point);                       //存储坐标
//        //                        Turn.SetField(DirectionTypes.DT_LEFT, i + 1, Twoj, TwoStart);
//        //                        return false;
//        //                    }
//        //                }
//        //                #endregion
//        //            }
//        //            return false;
//        //        }
//        //        /// </summary>
//        //        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1]
//        //        ///ChangeDirection:true：前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
//        //        ///         与当前3格点右边格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）对应
//        //        ///ChangeDirection:false--前一矢量点坐标[X坐标减1，Y坐标不变],（PtList[PtList.Count-1].X-1,PtList[PtList.Count-1].Y）
//        //        ///          后与当前3格点右边一个栅格坐标对应（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X-1）    
//        //        public bool InLeft(ref List<GeoPoint> PtList, ref TurnParam Turn)
//        //        {
//        //            #region 格点的栅格坐标
//        //            //ChangeDirection:true
//        //            //0格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-1）    
//        //            //1格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-1-1）
//        //            //2格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-1-1）
//        //            //ChangeDirection:false
//        //            //0格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-2）    
//        //            //1格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X-3）
//        //            //2格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X-3）
//        //            #endregion
//        //            int i = Turn.ZeroRow;
//        //            int j = Turn.ZeroRunColumn;
//        //            int ZeroStart = Turn.ZeroStart;
//        //            int OneAttr = -1;
//        //            int TwoAttr = -1;
//        //            int OneStart = ZeroStart - 1;
//        //            int TwoStart = OneStart;
//        //            int Onej = -1;
//        //            int Twoj = -1;                          //1,2格点所在行游程中第几游程            
//        //            #region 边界越界控制
//        //            int Onei = i;
//        //            int Twoi = i - 1;
//        //            if (Twoi < ImageBound.MinRow && OneStart >= ImageBound.MinColumn)    //即Twoi=-1
//        //            {
//        //                TwoAttr = -2;
//        //            }
//        //            else if (Twoi < ImageBound.MinRow && OneStart < ImageBound.MinColumn)
//        //            {
//        //                OneAttr = -2;
//        //                TwoAttr = -2;
//        //            }
//        //            else if (Twoi >= ImageBound.MinRow && OneStart < ImageBound.MinColumn)
//        //            {
//        //                OneAttr = -2;
//        //                TwoAttr = -2;
//        //            }
//        //            #endregion
//        //            if (OneAttr != -2)
//        //            {
//        //                #region 获取1格点属性

//        //                if (OneStart >= m_ImageRun[i][j].Start)
//        //                {
//        //                    OneAttr = m_ImageRun[i][j].Attr;
//        //                    Onej = j;
//        //                }
//        //                else
//        //                {
//        //                    OneAttr = m_ImageRun[i][j - 1].Attr;
//        //                    Onej = j - 1;
//        //                }
//        //                #endregion
//        //            }
//        //            #region 判断格点1与0号格点的属性异同关系
//        //            GeoPoint Point = new GeoPoint();
//        //            Point.X = TwoStart + 1;//3格点栅格[I,J]  ——> 矢量[J, I+1]
//        //            Point.Y = m_ImageSize.Height - (i - 1 + 1);
//        //            if (OneAttr != m_ImageRun[i][j].Attr)
//        //            {
//        //                m_ImageRun[i][j].LeftFlag = ReConstructFlag;
//        //                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
//        //                {
//        //                    return true;
//        //                }
//        //                else
//        //                {
//        //                    PtList.Add(Point);
//        //                    Turn.SetField(DirectionTypes.DT_DOWN, i, j, ZeroStart);
//        //                    return false;
//        //                }
//        //            }
//        //            #endregion
//        //            else if (OneAttr == m_ImageRun[i][j].Attr)
//        //            {
//        //                if (TwoAttr != -2)
//        //                {
//        //                    #region 查找2格点属性
//        //                    for (int k2 = 0; k2 < m_ImageRun[i - 1].Count - 1; k2++)
//        //                    {
//        //                        if (TwoStart >= m_ImageRun[i - 1][k2].Start && TwoStart < m_ImageRun[i - 1][k2 + 1].Start)
//        //                        {
//        //                            TwoAttr = m_ImageRun[i - 1][k2].Attr;
//        //                            Twoj = k2;
//        //                            break;
//        //                        }
//        //                    }
//        //                    if (TwoAttr == -1)
//        //                    {
//        //                        TwoAttr = m_ImageRun[i - 1][m_ImageRun[i - 1].Count - 1].Attr;
//        //                        Twoj = m_ImageRun[i - 1].Count - 1;
//        //                    }
//        //                    #endregion
//        //                }
//        //                #region 判断格点2与0格点属性异同关系
//        //                if (TwoAttr != m_ImageRun[i][j].Attr)
//        //                {
//        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//        //                    {
//        //                        return true;                                                //不存储坐标
//        //                    }
//        //                    else
//        //                    {
//        //                        Turn.SetField(DirectionTypes.DT_LEFT, i, Onej, OneStart);
//        //                        return false;
//        //                    }
//        //                }
//        //                else if (TwoAttr == m_ImageRun[i][j].Attr)
//        //                {
//        //                    m_ImageRun[i - 1][Twoj].RightFlag = ReConstructFlag;
//        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
//        //                    {
//        //                        return true;
//        //                    }
//        //                    else
//        //                    {
//        //                        PtList.Add(Point);
//        //                        Turn.SetField(DirectionTypes.DT_UP, i - 1, Twoj, TwoStart);
//        //                        return false;
//        //                    }
//        //                }
//        //                #endregion
//        //            }
//        //            return false;
//        //        }
//        //        /// </summary>
//        //        ///格点的:栅格坐标[I,J]  ——>   矢量点[J, I+1]
//        //        ///ChangeDirection：true 前一矢量点坐标（PtList[PtList.Count-1].X,PtList[PtList.Count-1].Y）
//        //        ///         与当前0格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X）对应
//        //        ///ChangeDirection:false 前一矢量点坐标[X坐标加1，Y坐标不变],（PtList[PtList.Count-1].X+1,PtList[PtList.Count-1].Y）
//        //        ///         与当前0格点栅格坐标（PtList[PtList.Count-1].Y-1，PtList[PtList.Count-1].X+1）对应     
//        //        public bool InRight(ref List<GeoPoint> PtList, ref TurnParam Turn)
//        //        {
//        //            #region 格点的栅格坐标
//        //            //ChangeDirection：true
//        //            //0格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X）    
//        //            //1格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X+1）
//        //            //2格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X+1）
//        //            //ChangeDirection：false
//        //            //0格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X+1）    
//        //            //1格点：（PtList[PtList.Count-1].Y-1,PtList[PtList.Count-1].X+2）
//        //            //2格点：（PtList[PtList.Count-1].Y-1+1,PtList[PtList.Count-1].X+2）
//        //            #endregion
//        //            int i = Turn.ZeroRow;
//        //            int j = Turn.ZeroRunColumn;
//        //            int ZeroStart = Turn.ZeroStart;
//        //            int OneAttr = -1;
//        //            int TwoAttr = -1;
//        //            int OneStart = ZeroStart + 1;
//        //            int TwoStart = OneStart;
//        //            int Onej = -1;
//        //            int Twoj = -1;
//        //            #region 边界越界控制
//        //            int Onei = i;
//        //            int Twoi = i + 1;
//        //            if (Twoi > ImageBound.MaxRow - 1 && OneStart <= (ImageBound.MaxColumn - 1))    //即Twoi=ImageBound.MaxRow
//        //            {
//        //                TwoAttr = -2;                    //此if条件在，最后一行出现，概率居中
//        //            }
//        //            else if (Twoi > ImageBound.MaxRow - 1 && OneStart > (ImageBound.MaxColumn - 1))
//        //            {
//        //                OneAttr = -2;                        //此if条件在，图像最角落才出现，概率低，放在后面
//        //                TwoAttr = -2;
//        //            }
//        //            else if (Twoi <= (ImageBound.MaxRow - 1) && OneStart > (ImageBound.MaxColumn - 1))
//        //            {
//        //                OneAttr = -2;                         //此if条件在每行的末尾判断，出现概率高于第一个
//        //                TwoAttr = -2;
//        //            }
//        //            #endregion                          //为了提高算法效率，可以调整下if条件顺序的
//        //            if (OneAttr != -2)
//        //            {
//        //                #region 获取1格点属性
//        //                if ((j + 1) < (m_ImageRun[i].Count))
//        //                {
//        //                    if (OneStart < m_ImageRun[i][j + 1].Start)
//        //                    {
//        //                        OneAttr = m_ImageRun[i][j].Attr;
//        //                        Onej = j;
//        //                    }
//        //                    else
//        //                    {
//        //                        OneAttr = m_ImageRun[i][j + 1].Attr;
//        //                        Onej = j + 1;
//        //                    }
//        //                }
//        //                else
//        //                {
//        //                    OneAttr = m_ImageRun[i][j].Attr;
//        //                    Onej = j;
//        //                }
//        //                #endregion
//        //            }
//        //            #region 判断格点1与0号格点的属性异同关系
//        //            GeoPoint Point = new GeoPoint();
//        //            Point.X = OneStart;     //1格点栅格[I,J]  ——> 矢量[J, I+1]
//        //            Point.Y = m_ImageSize.Height - (i + 1);    //1格点（i，OneStart）
//        //            if (OneAttr != m_ImageRun[i][j].Attr)
//        //            {
//        //                m_ImageRun[i][j].RightFlag = ReConstructFlag;
//        //                if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
//        //                {
//        //                    return true;
//        //                }
//        //                else
//        //                {
//        //                    PtList.Add(Point);
//        //                    Turn.SetField(DirectionTypes.DT_UP, i, j, ZeroStart);
//        //                    return false;
//        //                }
//        //            }
//        //            #endregion
//        //            else if (OneAttr == m_ImageRun[i][j].Attr)
//        //            {
//        //                if (TwoAttr != -2)
//        //                {
//        //                    #region 查找2格点属性
//        //                    for (int k2 = 0; k2 < m_ImageRun[i + 1].Count - 1; k2++)
//        //                    {
//        //                        if (TwoStart >= m_ImageRun[i + 1][k2].Start && TwoStart < m_ImageRun[i + 1][k2 + 1].Start)
//        //                        {
//        //                            TwoAttr = m_ImageRun[i + 1][k2].Attr;
//        //                            Twoj = k2;
//        //                            break;
//        //                        }
//        //                    }
//        //                    if (TwoAttr == -1)
//        //                    {
//        //                        TwoAttr = m_ImageRun[i + 1][m_ImageRun[i + 1].Count - 1].Attr;
//        //                        Twoj = m_ImageRun[i + 1].Count - 1;
//        //                    }
//        //                    #endregion
//        //                }
//        //                #region 判断格点2与0格点属性异同关系
//        //                if (TwoAttr != m_ImageRun[i][j].Attr)
//        //                {
//        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)  //返回起点
//        //                    {
//        //                        return true;                                                //不存储坐标
//        //                    }
//        //                    else
//        //                    {
//        //                        Turn.SetField(DirectionTypes.DT_RIGHT, i, Onej, OneStart);
//        //                        return false;
//        //                    }
//        //                }
//        //                else if (TwoAttr == m_ImageRun[i][j].Attr)
//        //                {
//        //                    m_ImageRun[i + 1][Twoj].LeftFlag = ReConstructFlag;
//        //                    if (Point.X == PtList[0].X && Point.Y == PtList[0].Y)
//        //                    {
//        //                        return true;
//        //                    }
//        //                    else
//        //                    {
//        //                        PtList.Add(Point);
//        //                        Turn.SetField(DirectionTypes.DT_DOWN, i + 1, Twoj, TwoStart);
//        //                        return false;
//        //                    }
//        //                }
//        //                #endregion
//        //            }
//        //            return false;
//        //        }
//        //        #endregion  
//    }
//}