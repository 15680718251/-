using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OSGeo.GDAL;
using GIS.Geometries;

namespace GIS.RaToVe.ObjectVecRLE
{
    public partial class ObjectOrientedVec:IDisposable
    {
        private string strRasFilePathName;
        private string strExportfolderName;

        private Dataset m_GdalDataset;
        private double[] m_GeoTransform = new double[6];
        public Size m_ImageSize;
        private int m_BandCount;
        public List<List<Run>> m_ImageRun;
        public Bound ImageBound;   //栅格图像边界定义

        private Dictionary<string, string> dics = null;


        #region 需要矢量压缩
        public double ToleranceDis;
        private bool m_Compress;
        #endregion

        #region 需多边形高程范围
        private string strDEMFilePathName;
        private Dataset m_DEMGdalDataset;  //DEM数据与栅格图大小一致
        private bool m_UsingDEM;
        #endregion

        public class Bound
        {
            public int MinRow;
            public int MaxRow;
            public int MinColumn;
            public int MaxColumn;
        }

        //构造函数
        public ObjectOrientedVec(string strFilePathName1, string strFilePathName2, string strFilePathName3, double Distance, Dictionary<string, string> dics)
        {
            strRasFilePathName = strFilePathName1;
            strDEMFilePathName = strFilePathName2;
            strExportfolderName = strFilePathName3;
            this.dics = dics;

            if (Distance != -1)
            {
                ToleranceDis = Distance;
                m_Compress = true;
            }
            else
                m_Compress = false;
        }

        //public List<Run> this[int RoWRunIndex]
        //{
        //    get
        //    {
        //        return (List<Run>)List[RoWRunIndex];
        //    }
        //    set
        //    {
        //        List[RoWRunIndex] = value;
        //    }
        //}

        public void OpenRasterAndDEM()
        {
            Gdal.AllRegister();
            OSGeo.GDAL.Gdal.GetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");

            try
            {
                m_GdalDataset = Gdal.Open(strRasFilePathName, Access.GA_ReadOnly);
                m_ImageSize = new Size(m_GdalDataset.RasterXSize, m_GdalDataset.RasterYSize);
                //m_ImageSize = new Size(16, 26);
                m_BandCount = m_GdalDataset.RasterCount;
                m_GdalDataset.GetGeoTransform(m_GeoTransform);
            }
            catch (Exception ex)   //这里是只要上面try有空指针就，捕获错误的
            {
                m_GdalDataset = null;
                throw new Exception("Couldn't load " + strRasFilePathName + "\n\n" + ex.Message + ex.InnerException);
            }

            //一般是没有DEM
            if (strDEMFilePathName != null && strDEMFilePathName!="")
            {
                try
                {
                    m_DEMGdalDataset = Gdal.Open(strDEMFilePathName, Access.GA_ReadOnly);
                    m_UsingDEM = true;
                }
                catch (Exception ex)   //这里是只要上面有空指针就，捕获错误的
                {
                    m_DEMGdalDataset = null;
                    throw new Exception("Couldn't load " + strDEMFilePathName + "\n\n" + ex.Message + ex.InnerException);
                }
            }
            else
            {
                m_DEMGdalDataset = null;
                m_UsingDEM = false;
            }
        }

        #region Disposers and finalizers

        private bool disposed = false;

        /// <summary>
        /// Disposes the GdalRasterLayer and release the raster file
        /// </summary>
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
                        }
                    }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~ObjectOrientedVec()
        {
            Dispose(true);
        }

        #endregion

        public void TransformRasterToRLE()
        {
            ImageBound = new Bound();
            ImageBound.MinRow = 0;
            ImageBound.MaxRow = m_ImageSize.Height;
            ImageBound.MinColumn = 0;
            ImageBound.MaxColumn = m_ImageSize.Width;

            m_ImageRun = new List<List<Run>>();
            Band band = m_GdalDataset.GetRasterBand(1); //单波段

            byte[] Buffer = new byte[m_ImageSize.Width * m_ImageSize.Height];
            band.ReadRaster(0, 0, m_ImageSize.Width, m_ImageSize.Height, Buffer, m_ImageSize.Width, m_ImageSize.Height, 0, 0);

            Band bandDEM;
            short[] BufferDEM;  //不能用byte,否则大于0--255范围的高程值，全部被读为255了就

            if (m_UsingDEM)
            {
                bandDEM = m_DEMGdalDataset.GetRasterBand(1);
                BufferDEM = new short[m_ImageSize.Width * m_ImageSize.Height];
                bandDEM.ReadRaster(0, 0, m_ImageSize.Width, m_ImageSize.Height, BufferDEM, m_ImageSize.Width, m_ImageSize.Height, 0, 0);
            }
            else
                BufferDEM = new short[0];

            for (int y = 0; y < m_GdalDataset.RasterYSize; y++)
            {
                Run TempRun1 = new Run();
                List<Run> m_RoWRun = new List<Run>();
                TempRun1.Attr = Buffer[y * m_GdalDataset.RasterXSize];
                TempRun1.Start = 0;
                TempRun1.LeftFlag = -1;
                TempRun1.RightFlag = -1;
                TempRun1.ElevationMax = 0;           //如果不使用高程的矢量化，每个游程高程值都为0
                TempRun1.ElevationMin = 10000;
                //if (TempRun.Attr != 0)             //每行第一个0游程不进行存储
                //{
                m_RoWRun.Add(TempRun1);
                //}

                for (int x = 0; x < m_GdalDataset.RasterXSize - 1; x++)
                {
                    if (m_UsingDEM)           //游程高程最大和最小值获取
                    {
                        int Elevation = BufferDEM[x + y * m_GdalDataset.RasterXSize];
                        if (Elevation < m_RoWRun[m_RoWRun.Count - 1].ElevationMin)
                            m_RoWRun[m_RoWRun.Count - 1].ElevationMin = Elevation;
                        if (Elevation > m_RoWRun[m_RoWRun.Count - 1].ElevationMax)
                            m_RoWRun[m_RoWRun.Count - 1].ElevationMax = Elevation;
                    }

                    while (Buffer[x + y * m_GdalDataset.RasterXSize] != Buffer[x + 1 + y * m_GdalDataset.RasterXSize])
                    {
                        Run TempRun2 = new Run();
                        TempRun2.Attr = Buffer[x + 1 + y * m_GdalDataset.RasterXSize];
                        TempRun2.Start = x + 1;
                        TempRun2.LeftFlag = -1;
                        TempRun2.RightFlag = -1;
                        TempRun2.ElevationMax = 0;
                        TempRun2.ElevationMin = 10000;
                        m_RoWRun.Add(TempRun2);
                        break;
                    }
                }

                if (m_UsingDEM)   //每行最后一个格点游程高程最大和最小值获取
                {
                    int Elevation = BufferDEM[m_GdalDataset.RasterXSize - 1 + y * m_GdalDataset.RasterXSize];
                    if (Elevation < m_RoWRun[m_RoWRun.Count - 1].ElevationMin)
                        m_RoWRun[m_RoWRun.Count - 1].ElevationMin = Elevation;
                    if (Elevation > m_RoWRun[m_RoWRun.Count - 1].ElevationMax)
                        m_RoWRun[m_RoWRun.Count - 1].ElevationMax = Elevation;
                }

                m_ImageRun.Add(m_RoWRun);
            }
        }

        public void GeoTransform(GeoPolygon Plg)
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

        public void GeoTransformPt(GeoPoint Pt)
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

        public void RaToVe( ref int FileCount)
        {
            OpenRasterAndDEM();
            TransformRasterToRLE();
            ExtractionVecPolygon(ref FileCount );
        }
    }
}

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Drawing;
//using System.Drawing.Imaging;
//using OSGeo.GDAL;
//using GIS.Geometries;

//namespace GIS.RaToVe.ObjectVec
//{
//    public partial class ObjectOrientedVec : CollectionBase
//    {
//        private string strRasFilePathName;
//        private string strExportfolderName;
//        private Dataset m_GdalDataset;
//        private double[] m_GeoTransform = new double[6];
//        public Size m_ImageSize;
//        private int m_BandCount;
//        public List<List<Ras>> m_ImageRas;
//        public Bound ImageBound;   //栅格图像边界定义
//        #region 需要矢量压缩
//        public double ToleranceDis;
//        private bool m_Compress;
//        #endregion
//        #region 需多边形高程范围
//        private string strDEMFilePathName;
//        private Dataset m_DEMGdalDataset;  //DEM数据与栅格图大小一致
//        private bool m_UsingDEM;
//        #endregion
//        public class Bound
//        {
//            public int MinRow;
//            public int MaxRow;
//            public int MinColumn;
//            public int MaxColumn;
//        }
//        public ObjectOrientedVec(string strFilePathName1, string strFilePathName2, string strFilePathName3, double Distance)
//        {
//            strRasFilePathName = strFilePathName1;
//            strDEMFilePathName = strFilePathName2;
//            strExportfolderName = strFilePathName3;
//            if (Distance != -1)
//            {
//                ToleranceDis = Distance;
//                m_Compress = true;
//            }
//            else
//                m_Compress = false;
//        }
//        public List<Ras> this[int RoWRunIndex]
//        {
//            get
//            {
//                return (List<Ras>)List[RoWRunIndex];
//            }
//            set
//            {
//                List[RoWRunIndex] = value;
//            }
//        }

//        public void OpenRasterAndDEM()
//        {
//            disposed = false;
//            Gdal.AllRegister();
//            try
//            {
//                m_GdalDataset = Gdal.OpenShared(strRasFilePathName, Access.GA_ReadOnly);
//                m_ImageSize = new Size(m_GdalDataset.RasterXSize, m_GdalDataset.RasterYSize);
//                m_BandCount = m_GdalDataset.RasterCount;
//                m_GdalDataset.GetGeoTransform(m_GeoTransform);
//            }
//            catch (Exception ex)   //这里是只要上面try有空指针就，捕获错误的
//            {
//                m_GdalDataset = null;
//                throw new Exception("Couldn't load " + strRasFilePathName + "\n\n" + ex.Message + ex.InnerException);
//            }
//            if (strDEMFilePathName != null)
//            {
//                try
//                {
//                    m_DEMGdalDataset = Gdal.OpenShared(strDEMFilePathName, Access.GA_ReadOnly);
//                    m_UsingDEM = true;
//                }
//                catch (Exception ex)   //这里是只要上面有空指针就，捕获错误的
//                {
//                    m_DEMGdalDataset = null;
//                    throw new Exception("Couldn't load " + strDEMFilePathName + "\n\n" + ex.Message + ex.InnerException);
//                }
//            }
//            else
//            {
//                m_DEMGdalDataset = null;
//                m_UsingDEM = false;
//            }
//        }

//        #region Disposers and finalizers

//        private bool disposed;

//        /// <summary>
//        /// Disposes the GdalRasterLayer and release the raster file
//        /// </summary>
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        private void Dispose(bool disposing)
//        {
//            if (!disposed)
//            {
//                if (disposing)
//                    if (m_GdalDataset != null)
//                    {
//                        try
//                        {
//                            m_GdalDataset.Dispose();
//                        }
//                        finally
//                        {
//                            m_GdalDataset = null;
//                        }
//                    }
//                disposed = true;
//            }
//        }

//        /// <summary>
//        /// Finalizer
//        /// </summary>
//        ~RunLengthEncodingVec()
//        {
//            Dispose(true);
//        }

//        #endregion

//        public void TransformRasterToRas()  //将用GDAL读取的栅格数据，转换到自定义栅格结构：Ras来存储
//        {
//            ImageBound = new Bound();
//            ImageBound.MinRow = 0;
//            ImageBound.MaxRow = m_ImageSize.Height;
//            ImageBound.MinColumn = 0;
//            ImageBound.MaxColumn = m_ImageSize.Width;

//            m_ImageRas = new List<List<Ras>>();
//            Band band = m_GdalDataset.GetRasterBand(1);
//            byte[] Buffer = new byte[m_ImageSize.Width * m_ImageSize.Height];
//            band.ReadRaster(0, 0, m_ImageSize.Width, m_ImageSize.Height, Buffer, m_ImageSize.Width, m_ImageSize.Height, 0, 0);

//            for (int y = 0; y < m_GdalDataset.RasterYSize; y++)
//            {
//                List<Ras> m_RoWRas = new List<Ras>();
//                for (int x = 0; x < m_GdalDataset.RasterXSize - 1; x++)
//                {
//                  Ras TempRas1 = new Ras();
//                  TempRas1.Attr = Buffer[x +y * m_GdalDataset.RasterXSize];
//                  TempRas1.Flag = -2;  //初始化每个栅格标记位为-2
//                  m_RoWRas.Add(TempRas1);
//                }
//                m_ImageRas.Add(m_RoWRas);
//            }
//        }
//        public void GeoTransform()
//        {
//            //m_GeoTransform[0],m_GeoTransform[3]分别为栅格左上角的地理X,Y坐标: 转变为地理坐标,写到shape中时，计算面积才正确的
//            //则栅格左下角地理坐标,矢量坐标原点地理坐标为:
//            double XGeo = m_GeoTransform[0];
//            double YGeo = m_GeoTransform[3] + m_GeoTransform[5] * m_ImageSize.Height;
//            foreach (GeoPolygon Plg in PlgList)
//            {
//                for (int i = 0; i < Plg.ExteriorRing.Vertices.Count; i++)
//                {
//                    //矢量坐标点：将一个栅格格点30*30m，当做长度1*1m来表示
//                    //因此需乘以实际像元大小：m_GeoTransform[1],m_GeoTransform[5]，比30大一点点，若用准确的30,和栅格影像叠加会有偏差
//                    //m_GeoTransform[5]以栅格图像左上角为原点，向下为Y轴正向，矢量Y轴方向相反，需乘-1
//                    Plg.ExteriorRing.Vertices[i].X = Plg.ExteriorRing.Vertices[i].X * m_GeoTransform[1] + XGeo;
//                    Plg.ExteriorRing.Vertices[i].Y = Plg.ExteriorRing.Vertices[i].Y * (-1) * m_GeoTransform[5] + YGeo;
//                }
//                for (int i = 0; i < Plg.InteriorRings.Count; i++)
//                {
//                    for (int j = 0; j < Plg.InteriorRings[i].Vertices.Count; j++)
//                    {
//                        Plg.InteriorRings[i].Vertices[j].X = Plg.InteriorRings[i].Vertices[j].X * m_GeoTransform[1] + XGeo;
//                        Plg.InteriorRings[i].Vertices[j].Y = Plg.InteriorRings[i].Vertices[j].Y * (-1) * m_GeoTransform[5] + YGeo;
//                    }
//                }
//            }
//        }
//        public void RaToVe()
//        {
//            OpenRasterAndDEM();
//            TransformRasterToRas();
//            ExtractionVecPolygon();
//            GeoTransform();
//            ExportDataAsShape();
//        }
//    }
//}
