//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.IO;
//using System.Drawing;
//using System.Drawing.Imaging;
//using GIS.Geometries;
//using GIS.Utilities;
//using GIS.GeoData;
//using OSGeo.GDAL;

//namespace GIS.Layer
//{
//    /// <summary>
//    /// 用来控制红绿蓝三个波段
//    /// </summary>
//    public class colortype
//    {
//        public Band Red = null;
//        public Band Green = null;
//        public Band Blue = null;
//    }

//    public enum RasterType
//    {
//        Invalid,    //位置
//        RGB,        //RGB 三原色
//        Palette     //调色板
//    }
    
//    /// <summary>
//    /// 阳成飞修改 138行，修改金字塔建立函数，修改
//    /// </summary>
//    public class GeoRasterLayer : GeoLayer
//    {
//        [DllImport("gdi32.dll")]
//        private static extern long SetBitmapBits(IntPtr hbm, Int32 cb, IntPtr pvBits);

//        public Dataset m_GdalDataset;
//        private double[] m_GeoTransform = new double[6];
//        private double[] m_GeoTransform_preser = new double[6];//用来保存原始数据
//        public colortype bandcolors = new colortype();
//        private Size m_ImageSize;
//        private int m_BandCount;
//        /// <summary>
//        /// 当前金字塔层
//        /// </summary>
//        private int bandindex = -1;
//        private int iPixelSize = 3;
//        private RasterType m_RasterType;
//        private GeoRasterLayer()
//        {
//        }
//        public int GetRasterLayerCount()
//        {
//            return m_GdalDataset.RasterCount;
//        }
        
//        //图层类型
//        public override LAYERTYPE LayerType
//        {
//            get { return LAYERTYPE.RasterLayer; }
//        }
//        public override LAYERTYPE_DETAIL LayerTypeDetail
//        {
//            get { return LAYERTYPE_DETAIL.RasterLayer; }
//        }
        
//        public GeoRasterLayer(string strFilePathName): base(strFilePathName)
//        {
//            disposed = false;

//            //// 为了支持中文路径，请添加下面这句代码
//            //OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
//            //// 为了使属性表字段支持中文，请添加下面这句
//            //OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");

//            if ( System.IO.Path.GetExtension(strFilePathName).ToUpper()== ".HDR")
//            {
//                strFilePathName = strFilePathName.Remove(strFilePathName.Length-4);
//            }

//            Gdal.AllRegister();
//            try
//            {
//                m_GdalDataset = Gdal.Open(strFilePathName, Access.GA_ReadOnly);
//                m_GdalDataset.GetGeoTransform(m_GeoTransform_preser);
//                m_ImageSize = new Size(m_GdalDataset.RasterXSize, m_GdalDataset.RasterYSize);
//                LayerBound = GetBoundingBox();
//                m_BandCount = m_GdalDataset.RasterCount;

//                Driver hDriver = m_GdalDataset.GetDriver();
//                String pszDriver = hDriver.ShortName;

//                if (pszDriver.Trim() == "HFA" || pszDriver.Trim() == "PCIDSK" || pszDriver.Trim() == "ENVI" || pszDriver.Trim() == "GTiff")
//                {
//                    Gdal.Unlink(strFilePathName);
//                    m_GdalDataset.Dispose();
//                    m_GdalDataset = Gdal.Open(strFilePathName, Access.GA_ReadOnly);
//                    string ovrFilePath = PathName + ".ovr";
//                    BuildOverViewForEnviAndPCI();
//                }
//                else
//                {
//                    BuildOverView();
//                }

//                GetRasterType();

//            }
//            catch (Exception ex)
//            {
//                m_GdalDataset = null;
//                throw new Exception("Couldn't load " + strFilePathName + "\n\n" + ex.Message + ex.InnerException);
//            }
//        }

//        #region Disposers and finalizers

//        private bool disposed;

//        /// <summary>
//        /// Disposes the GdalRasterLayer and release the raster file
//        /// </summary>
//        public override void Dispose()
//        {
//            base.Dispose();
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
//        ~GeoRasterLayer()
//        {
//            Dispose(true);
//        }

//        #endregion

//        private bool BuildOverView()
//        {
//            string ovrFilePath = PathName + ".ovr";
//            if (!File.Exists(ovrFilePath))
//            {
//                string badFile = Path.GetDirectoryName(PathName) + "\\" + LayerName + ".aux";
//                if (File.Exists(badFile))
//                {
//                    File.Delete(badFile);
//                }

//                int iPixelNum = m_GdalDataset.RasterXSize * m_GdalDataset.RasterYSize;    //图像中的总像元个数
//                int iTopNum = 4096 * 16;                 //顶层金字塔大小，64*64，小于该数目时不再创建金字塔。因为不再需要。
//                int iCurNum = iPixelNum / 4;

//                int[] anLevels = new int[1023];
//                int nLevelCount = 0;                //金字塔级数

//                do    //计算金字塔级数，从第二级到顶层
//                {
//                    anLevels[nLevelCount] = (int)Math.Pow(2.0, nLevelCount + 2);
//                    nLevelCount++;
//                    iCurNum /= 4;
//                } 
//                while (iCurNum > iTopNum);

//                if (nLevelCount > 0 && m_GdalDataset.BuildOverviews("NEAREST", anLevels, new Gdal.GDALProgressFuncDelegate(ProgressFunc), "Sample Data") != (int)CPLErr.CE_None)
//                {
//                    Console.WriteLine("The BuildOverviews operation doesn't work");
//                    System.Environment.Exit(-1);
//                }
//                //int error = m_GdalDataset.BuildOverviews("NEAREST", new int[] { 2,4,8 });
//                return true;
//            }
//            return false;
//        }
//        /// <summary>
//        /// 为envi和edars格式的img创建内部金字塔
//        /// </summary>
//        /// <returns></returns>
//        private bool BuildOverViewForEnviAndPCI()
//        {
//            try
//            {
//                string ovrFilePath = PathName + ".ovr";
//                if (!File.Exists(ovrFilePath))
//                {
//                    string badFile = Path.GetDirectoryName(PathName) + "\\" + LayerName + ".aux";
//                    if (File.Exists(badFile))
//                    {
//                        File.Delete(badFile);
//                    }
//                    badFile = Path.GetDirectoryName(PathName) + "\\" + LayerName + ".rrd";
//                    if (File.Exists(badFile))
//                    {
//                        return true;
//                    }
//                    badFile = Path.GetDirectoryName(PathName) + "\\" + LayerName + ".aux.xml";
//                    if (File.Exists(badFile))
//                    {
//                        return true;
//                    }
//                    int iPixelNum = m_GdalDataset.RasterXSize * m_GdalDataset.RasterYSize;    //图像中的总像元个数
//                    int iTopNum = 4096 * 16;                 //顶层金字塔大小，64*64，小于该数目时不再创建金字塔。因为不再需要。
//                    int iCurNum = iPixelNum / 4;

//                    List<int> ai = new List<int>();
//                    int nLevelCount = 0;//金字塔级数
//                    do    //计算金字塔级数，从第二级到顶层
//                    {
//                        ai.Add((int)Math.Pow(2.0, nLevelCount + 2));
//                        nLevelCount++;
//                        iCurNum /= 4;
//                    }
//                    while (iCurNum > iTopNum);

//                    int[] kinlevels;
//                    kinlevels = new int[ai.Count];
//                    ai.CopyTo(kinlevels);
//                    if (nLevelCount > 0 && m_GdalDataset.BuildOverviews("NEAREST", kinlevels, new Gdal.GDALProgressFuncDelegate(ProgressFunc), "Sample Data") != (int)CPLErr.CE_None)
//                    {
//                        Console.WriteLine("The BuildOverviews operation doesn't work");
//                        System.Environment.Exit(-1);
//                    }
//                    //int error = m_GdalDataset.BuildOverviews("NEAREST", new int[] { 2,4,8 });
//                    return true;
//                }
//                return false;
//            }
//            catch (Exception ee)
//            {
//                return false;
//            }
//        }

//        /// <summary>
//        /// 阳成飞2013.4.14，若有需要，这里可以用来更新界面，显示建立进度
//        /// </summary>
//        /// <param name="Complete"></param>
//        public static int ProgressFunc(double Complete, IntPtr Message, IntPtr Data)
//        {
//            Console.Write("Processing ... " + Complete * 100 + "% Completed.");
//            if (Message != IntPtr.Zero)
//                Console.Write(" Message:" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Message));
//            if (Data != IntPtr.Zero)
//                Console.Write(" Data:" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Data));

//            Console.WriteLine("");
//            return 1;
//        }
        
//        private void GetRasterType()
//        {
//            bool green = false, red = false, blue = false, palette = false;
//            for (int i = 0; i < m_BandCount; i++)
//            {
//                ColorInterp ci = m_GdalDataset.GetRasterBand(i + 1).GetColorInterpretation();
//                if (ci == ColorInterp.GCI_Undefined)
//                {
//                    Band band = m_GdalDataset.GetRasterBand(i + 1);
//                    for (int iOver = 0; iOver < band.GetOverviewCount(); iOver++)
//                    {
//                        Band over = band.GetOverview(iOver);
//                        ci = over.GetRasterColorInterpretation();
//                        break;
//                    }
//                }

//                if (ci == ColorInterp.GCI_Undefined)
//                { 
//                    //如果遍历金字塔后仍然没有效果。则默认设置
//                    if (i == 0)
//                    {
//                        ci = ColorInterp.GCI_RedBand;
//                    }
//                    else if (i == 1)
//                    {
//                        ci = ColorInterp.GCI_GreenBand;
//                    }
//                    else if (i == 2)
//                    {
//                        ci = ColorInterp.GCI_BlueBand;
//                    }
//                }
//                switch (ci)
//                {
//                    case ColorInterp.GCI_RedBand:
//                        red = true;
//                        bandcolors.Red = m_GdalDataset.GetRasterBand(i + 1);
//                        break;
//                    case ColorInterp.GCI_GreenBand:
//                        green = true;
//                        bandcolors.Green = m_GdalDataset.GetRasterBand(i + 1);
//                        break;
//                    case ColorInterp.GCI_BlueBand:
//                        blue = true;
//                        bandcolors.Blue = m_GdalDataset.GetRasterBand(i + 1);
//                        break;
//                    case ColorInterp.GCI_PaletteIndex:
//                        palette = true;
//                        break;
//                }
//            }

//            if ((red == blue && blue == green && green == true)|| m_BandCount >= 3)
//            {
//                m_RasterType = RasterType.RGB;
//                if (!(red == blue && blue == green && green == true)) 
//                {
//                    bandcolors.Red = m_GdalDataset.GetRasterBand(1);
//                    bandcolors.Green = m_GdalDataset.GetRasterBand(2);
//                    bandcolors.Blue = m_GdalDataset.GetRasterBand(3);
//                }
//            }
//            else if (m_BandCount == 1 && palette == true)
//            {
//                m_RasterType = RasterType.Palette;
//            }
//            else
//            {
//                m_RasterType = RasterType.Invalid;

//            }
//        }
//        public override GeoBound GetBoundingBox()
//        {
//            if (m_GdalDataset != null)
//            {
//                double right = 0, left = 0, top = 0, bottom = 0;
//                double dblW, dblH;

//                string tfwFileName = Path.GetDirectoryName(PathName) + "\\" + LayerName + ".tfw";
//                if (File.Exists(tfwFileName))
//                {
//                    FileStream fs = new FileStream(tfwFileName, FileMode.Open, FileAccess.Read);
//                    StreamReader m_StreamReader = new StreamReader(fs);
//                    fs.Seek(0, SeekOrigin.Begin);
//                    int[] order = new[] { 1, 2, 4, 5, 0, 3 };
//                    double val = 0;
//                    string strLine = null;
//                    for (int i = 0; i < 6; i++)
//                    {
//                        strLine = m_StreamReader.ReadLine();
//                        if (double.TryParse(strLine, out val))
//                            m_GeoTransform[order[i]] = val;
//                        else
//                            throw new Exception("坐标数据不对");
//                    }
//                    m_StreamReader.Close();
//                    fs.Close();
//                }
//                else
//                {
//                    m_GdalDataset.GetGeoTransform(m_GeoTransform);
//                }
//                if (m_GeoTransform[0] == 0 && m_GeoTransform[3] == 0)
//                {
//                    m_GeoTransform = new[] { 999.5, 1, 0, 1000.5, 0, -1 };
//                }


//                dblW = m_ImageSize.Width;
//                dblH = m_ImageSize.Height;

//                left = m_GeoTransform[0];
//                right = m_GeoTransform[0] + dblW * m_GeoTransform[1];
//                top = m_GeoTransform[3];
//                bottom = m_GeoTransform[3] + dblH * m_GeoTransform[5];
//                return new GeoBound(left, bottom, right, top);
//            }

//            return null;
//        }
//        private struct CrossRegion
//        {
//            public int startX;   //起始点X在图像中的位置
//            public int startY;   //起始点Y在图像中的位置
//            public int OffSetX;  //在图像中X的偏移范围
//            public int OffSetY;  //在图像中Y的偏移范围
//            public int ScrXWidth;     //图像在屏幕上显示X范围的大小
//            public int ScrYHeight;     //图像在屏幕上显示Y范围的大小
//            public int ScrStartX;//图像在屏幕上的X坐标的起点，屏幕坐标
//            public int ScrStartY;//图像在屏幕上的Y坐标的起点，屏幕坐标
//        }
//        private bool GetCrossRegion(Size ScreenSize, ITransForm transform, ref CrossRegion region)
//        {
//            Rectangle rectScr = new Rectangle(0, 0, ScreenSize.Width, ScreenSize.Height); //屏幕的范围
//            Point lb = transform.TransFromWorldToMap(LayerBound.LeftBottomPt);
//            Point tr = transform.TransFromWorldToMap(LayerBound.RightUpPt);
//            Rectangle rectImage = new Rectangle(Math.Min(lb.X, tr.X), Math.Min(lb.Y, tr.Y), tr.X - lb.X, Math.Abs(tr.Y - lb.Y));

//            Rectangle rect = Rectangle.Intersect(rectScr, rectImage); //与屏幕相交的矩形范围
//            if (rect.IsEmpty)
//                return false;

//            GeoPoint ptlb = transform.TransFromMapToWorld(new Point(rect.Left, rect.Top));
//            GeoPoint ptrt = transform.TransFromMapToWorld(new Point(rect.Right, rect.Bottom));
//            int endx, endy;
//            region.startX = Math.Abs(Convert.ToInt32((ptlb.X - m_GeoTransform[0]) / m_GeoTransform[1]));
//            region.startY = Math.Abs(Convert.ToInt32((ptlb.Y - m_GeoTransform[3]) / m_GeoTransform[5]));

//            endx = Math.Abs(Convert.ToInt32((ptrt.X - m_GeoTransform[0]) / m_GeoTransform[1]));
//            endy = Math.Abs(Convert.ToInt32((ptrt.Y - m_GeoTransform[3]) / m_GeoTransform[5]));



//            region.ScrXWidth = Math.Abs(rect.Right - rect.Left);
//            region.ScrYHeight = Math.Abs(rect.Top - rect.Bottom);

//            region.ScrStartX = rect.Left;
//            region.ScrStartY = rect.Top;


//            region.OffSetX = Math.Abs(endx - region.startX);
//            region.OffSetY = Math.Abs(endy - region.startY);

//            if ((region.OffSetX + region.startX) > m_ImageSize.Width)
//                region.OffSetX = m_ImageSize.Width - region.startX;
//            if ((region.OffSetY + region.startY) > m_ImageSize.Height)
//                region.OffSetY = m_ImageSize.Height - region.startY;


//            return true;
//        }

//        private unsafe void PaletteImage(CrossRegion rgn, BitmapData bmpData)
//        {
//            Band band = m_GdalDataset.GetRasterBand(1);
//            ColorTable clrTable = band.GetColorTable();
//            if (clrTable == null)
//                return;
//            byte[] Buffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
//            band.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, Buffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);

//            int index = 0;
//            for (int y = 0; y < bmpData.Height; y++)
//            {
//                byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;
//                for (int x = 0; x < bmpData.Width; x++)
//                {
//                    Int32 offsetX = x * iPixelSize;
//                    ColorEntry clrEntry = clrTable.GetColorEntry(Buffer[index]);

//                    row[offsetX] = (byte)clrEntry.c3;
//                    row[offsetX + 1] = (byte)clrEntry.c2;
//                    row[offsetX + 2] = (byte)clrEntry.c1;
//                    index++;
//                }

//            }
//        }
//        //#region  gdal库在解析img图像波段颜色时有问题，所以在这里加以修正
//        //ColorInterp ci = m_GdalDataset.GetRasterBand(i + 1).GetColorInterpretation();
//        //if (ci == ColorInterp.GCI_Undefined) {
//        //    Band band1  =  m_GdalDataset.GetRasterBand(i + 1);
//        //    for (int iOver = 0; iOver <band1.GetOverviewCount(); iOver++)
//        //    {
//        //        Band over = band1.GetOverview(iOver);
//        //        ci=over.GetRasterColorInterpretation();
//        //        break;
//        //    }
//        //} 
//        //ColorInterp clrInterp = ci != ColorInterp.GCI_Undefined ? ci : band.GetColorInterpretation();
//        //#endregion

//        #region
//        /// <summary>
//        /// 目前只对RGB情况作了修改，如果其他情况也有错误，按照RGB的思路修改
//        /// </summary>
//        /// <param name="rgn"></param>
//        /// <param name="bmpData"></param>
//        //private unsafe void RGBImage(CrossRegion rgn , BitmapData bmpData)
//        //{
//        //    Band RedBand ;
//        //    Band GreenBand;
//        //    Band BlueBand;
//        //    byte[] RedBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
//        //    byte[] GreenBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
//        //    byte[] BlueBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
//        //    for(int i =0 ; i<3 ;i++)
//        //    {
//        //        Band band=null;
//        //        if (bandindex >= 0) {
//        //            band = m_GdalDataset.GetRasterBand(i + 1).GetOverview(bandindex);
//        //        }
//        //        else
//        //        {
//        //            band = m_GdalDataset.GetRasterBand(i + 1);
//        //        }
//        //        ColorInterp clrInterp = band.GetColorInterpretation();
//        //        if(clrInterp == ColorInterp.GCI_RedBand)
//        //        {
//        //            RedBand = band;
//        //            RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY,RedBuffer ,rgn.ScrXWidth, rgn.ScrYHeight,0,0);
//        //        }
//        //        else if (clrInterp == ColorInterp.GCI_GreenBand)
//        //        {
//        //            GreenBand = band;
//        //            GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//        //        }
//        //        else if (clrInterp == ColorInterp.GCI_BlueBand)
//        //        {
//        //            BlueBand = band;

//        //            BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//        //        }
//        //        else {
//        //            if (i == 0)
//        //            {
//        //                RedBand = band;
//        //                RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//        //            }
//        //            else if (i == 1)
//        //            {
//        //                GreenBand = band;
//        //                GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//        //            }
//        //            else if (i==2)
//        //            {
//        //                BlueBand = band;

//        //                BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//        //            }
//        //        }    

//        //    }

//        //    int indexBuffer = 0 ;
//        //    for (int y = 0; y < bmpData.Height; y++)
//        //    {
//        //        byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;
//        //        for (int x = 0; x < bmpData.Width; x++)
//        //        {
//        //            Int32 offsetX =  x * iPixelSize;
//        //            row[offsetX] = BlueBuffer[indexBuffer];
//        //            row[offsetX + 1] = GreenBuffer[indexBuffer];
//        //            row[offsetX + 2] = RedBuffer[indexBuffer];
//        //            indexBuffer++;
//        //        }

//        //    }

//        //}
//        #endregion
//        /// <summary>
//        /// 依据图层颜色设置来显示
//        /// </summary>
//        /// <param name="rgn"></param>
//        /// <param name="bmpData"></param>
        
//        private unsafe void RGBImage(CrossRegion rgn, BitmapData bmpData)
//        {
//            Band RedBand;
//            Band GreenBand;
//            Band BlueBand;

//            if (bandcolors.Red.DataType == DataType.GDT_Byte)
//            {
//                //如果是8位的
//                byte[] RedBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
//                byte[] GreenBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
//                byte[] BlueBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
//                if (bandindex >= 0)
//                {
//                    RedBand = bandcolors.Red.GetOverview(bandindex);
//                    RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                    GreenBand = bandcolors.Green.GetOverview(bandindex);
//                    GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                    BlueBand = bandcolors.Blue.GetOverview(bandindex);
//                    BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                }
//                else
//                {
//                    RedBand = bandcolors.Red;
//                    RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                    GreenBand = bandcolors.Green;
//                    GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                    BlueBand = bandcolors.Blue;
//                    BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                }
//                int indexBuffer = 0;
//                for (int y = 0; y < bmpData.Height; y++)
//                {
//                    byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;
//                    for (int x = 0; x < bmpData.Width; x++)
//                    {
//                        Int32 offsetX = x * iPixelSize;
//                        row[offsetX] = BlueBuffer[indexBuffer];
//                        row[offsetX + 1] = GreenBuffer[indexBuffer];
//                        row[offsetX + 2] = RedBuffer[indexBuffer];
//                        indexBuffer++;
//                    }

//                }
//            }
//            else if (bandcolors.Red.DataType == DataType.GDT_Int16)
//            {
//                short[] RedBuffer = new short[rgn.ScrXWidth * rgn.ScrYHeight];
//                short[] GreenBuffer = new short[rgn.ScrXWidth * rgn.ScrYHeight];
//                short[] BlueBuffer = new short[rgn.ScrXWidth * rgn.ScrYHeight];
//                if (bandindex >= 0)
//                {
//                    RedBand = bandcolors.Red.GetOverview(bandindex);
//                    RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                    GreenBand = bandcolors.Green.GetOverview(bandindex);
//                    GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                    BlueBand = bandcolors.Blue.GetOverview(bandindex);
//                    BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                }
//                else
//                {
//                    RedBand = bandcolors.Red;
//                    RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                    GreenBand = bandcolors.Green;
//                    GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                    BlueBand = bandcolors.Blue;
//                    BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
//                }
//                int indexBuffer = 0;
//                for (int y = 0; y < bmpData.Height; y++)
//                {
//                    byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;
//                    for (int x =0; x < bmpData.Width; x++)
//                    {
//                        try
//                        {
//                            Int32 offsetX = x * iPixelSize;
//                            ((int*)row)[0] = BlueBuffer[indexBuffer];
//                            row++;
//                            row++;
//                            ((int*)row)[0] = GreenBuffer[indexBuffer];
//                            row++;
//                            row++;
//                            ((int*)row)[0] = RedBuffer[indexBuffer];
//                            row++;
//                            row++;
//                            indexBuffer++;
//                        }
//                        catch (Exception ee) {
//                            return;
//                        }
//                    }
//                }

//            }
//        }

//        public void DrawImage(Graphics g, Size ScrSize, ITransForm transform)
//        {
//            CrossRegion rgn = new CrossRegion();

//            #region 依据当前比例尺，重新设置像元大小，以及波段数
//            if (m_GdalDataset.GetRasterBand(1).GetOverviewCount() > 0)
//            {
//                m_GeoTransform = (double[])m_GeoTransform_preser.Clone();
//                double blc = transform.GetBlc();
//                double bandindexbefore = blc / m_GeoTransform_preser[1];
//                int bandindexmiddle = (int)(bandindexbefore / 4);
//                bandindex = -1;
//                if (bandindexmiddle >= m_GdalDataset.GetRasterBand(1).GetOverviewCount() - 1)
//                {
//                    bandindex = m_GdalDataset.GetRasterBand(1).GetOverviewCount() - 1;
//                    m_GeoTransform[1] = m_GeoTransform[1] * Math.Pow(2.0, bandindex + 2);
//                    m_GeoTransform[5] = m_GeoTransform[5] * Math.Pow(2.0, bandindex + 2);
//                }
//                else if (bandindexmiddle > 0 && bandindexmiddle < m_GdalDataset.GetRasterBand(1).GetOverviewCount())
//                {
//                    bandindex = bandindexmiddle - 1;
//                    m_GeoTransform[1] = m_GeoTransform[1] * Math.Pow(2.0, bandindex + 2);
//                    m_GeoTransform[5] = m_GeoTransform[5] * Math.Pow(2.0, bandindex + 2);

//                }
//            }
//            #endregion

//            if (!GetCrossRegion(ScrSize, transform, ref rgn))
//            {
//                return;
//            }

//            Bitmap bmp = null;
//            if (m_GdalDataset.GetRasterBand(1).DataType == DataType.GDT_Int16)
//            {
//                bmp = new Bitmap(rgn.ScrXWidth, rgn.ScrYHeight, PixelFormat.Format48bppRgb);
//            }
//            else if (m_GdalDataset.GetRasterBand(1).DataType == DataType.GDT_Byte)
//            {
//                bmp = new Bitmap(rgn.ScrXWidth, rgn.ScrYHeight, PixelFormat.Format24bppRgb);
//            }

//            BitmapData bmpData = bmp.LockBits( new Rectangle(0, 0, rgn.ScrXWidth, rgn.ScrYHeight), ImageLockMode.ReadWrite, bmp.PixelFormat);

//            try
//            {
//                switch (m_RasterType)
//                {
//                    case RasterType.RGB:
//                        RGBImage(rgn, bmpData);
//                        break;
//                    case RasterType.Palette:
//                        PaletteImage(rgn, bmpData);
//                        break;
//                    default:
//                        break;
//                }

//            }
//            catch
//            {
//                return;
//            }
//            finally
//            {
//                if (bmpData != null)
//                    bmp.UnlockBits(bmpData);
//            }

//            g.DrawImage(bmp, new Point(rgn.ScrStartX, rgn.ScrStartY));
//            bmp.Dispose();

//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using GIS.Geometries;
using GIS.Utilities;
using GIS.GeoData;
using OSGeo.GDAL;

namespace GIS.Layer
{
    /// <summary>
    /// 用来控制红绿蓝三个波段
    /// </summary>
    public class colortype
    {
        public Band Red = null;
        public Band Green = null;
        public Band Blue = null;
    }

    public enum RasterType
    {
        Invalid,     //位置
        RGB,         //RGB 三原色
        Palette,     //调色板
        GrayIndex   //灰度
    }

    public class GeoRasterLayer : GeoLayer
    {
        [DllImport("gdi32.dll")]
        private static extern long SetBitmapBits(IntPtr hbm, Int32 cb, IntPtr pvBits);

        public Dataset m_GdalDataset;
        private double[] m_GeoTransform = new double[6];
        private Size m_ImageSize;
        private int m_BandCount;
        private int bandindex = -1;
        private int iPixelSize = 3;
        private RasterType m_RasterType;
        private string rasterpath;
        public colortype bandcolors = new colortype();
        private GeoRasterLayer() { }

        public int GetRasterLayerCount()
        {
            return m_GdalDataset.RasterCount;
        }

        //图层类型
        public override LAYERTYPE LayerType
        {
            get { return LAYERTYPE.RasterLayer; }
        }

        public override LAYERTYPE_DETAIL LayerTypeDetail
        {
            get { return LAYERTYPE_DETAIL.RasterLayer; }
        }

        public GeoRasterLayer(string strFilePathName) : base(strFilePathName)
        {
            disposed = false;

            if (System.IO.Path.GetExtension(strFilePathName).ToUpper() == ".HDR")
            {
                strFilePathName = strFilePathName.Remove(strFilePathName.Length - 4);
            }
            rasterpath = strFilePathName;

            Gdal.AllRegister();
            try
            {
                m_GdalDataset = Gdal.OpenShared(strFilePathName, Access.GA_ReadOnly);
                m_ImageSize = new Size(m_GdalDataset.RasterXSize, m_GdalDataset.RasterYSize);
                LayerBound = GetBoundingBox();
                m_BandCount = m_GdalDataset.RasterCount;

                //BuildOverView();

                GetRasterType();
            }
            catch (Exception ex)
            {
                m_GdalDataset = null;
                throw new Exception("Couldn't load " + strFilePathName + "\n\n" + ex.Message + ex.InnerException);
            }
        }

        #region Disposers and finalizers

        private bool disposed;

        /// <summary>
        /// Disposes the GdalRasterLayer and release the raster file
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    if (m_GdalDataset != null)
                    {
                        try
                        {
                            m_GdalDataset.Dispose();
                        }
                        finally
                        {
                            m_GdalDataset = null;
                        }
                    }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~GeoRasterLayer()
        {
            Dispose(true);
        }

        #endregion

        private bool BuildOverView()
        {
            FileInfo fi = new FileInfo(rasterpath);
            double fSize = fi.Length / 1024 / 1024;
            if (fSize < 8)
                return false;

            string ext = System.IO.Path.GetExtension(rasterpath);
            if (ext != "")
            {
                rasterpath = rasterpath.Remove(rasterpath.LastIndexOf('.'));
            }
            string ovrFilePath = rasterpath + ".ovr";
            if (!File.Exists(ovrFilePath))
            {
                string badFile = Path.GetDirectoryName(rasterpath) + "\\" + LayerName + ".aux";
                if (File.Exists(badFile))
                {
                    File.Delete(badFile);
                }
                int error = m_GdalDataset.BuildOverviews("NEAREST", new int[] { 2, 4, 8 });
                return true;
            }
            return false;
        }

        private void GetRasterType()
        {
            bool green = false, red = false, blue = false, palette = false, grayindex = false;
            for (int i = 0; i < m_BandCount; i++)
            {
                ColorInterp ci = m_GdalDataset.GetRasterBand(i + 1).GetColorInterpretation();
                if (ci == ColorInterp.GCI_Undefined)
                {
                    Band band = m_GdalDataset.GetRasterBand(i + 1);
                    int oc = band.GetOverviewCount();
                    for (int iOver = 0; iOver < oc; iOver++)
                    {
                        Band over = band.GetOverview(iOver);
                        ci = over.GetRasterColorInterpretation();

                        break;
                    }
                }

                if (ci == ColorInterp.GCI_Undefined)
                {
                    //如果遍历金字塔后仍然没有效果。则默认设置
                    if (i == 0)
                    {
                        ci = ColorInterp.GCI_RedBand;
                    }
                    else if (i == 1)
                    {
                        ci = ColorInterp.GCI_GreenBand;
                    }
                    else if (i == 2)
                    {
                        ci = ColorInterp.GCI_BlueBand;
                    }
                }

                switch (ci)
                {
                    case ColorInterp.GCI_RedBand:
                        red = true;
                        bandcolors.Red = m_GdalDataset.GetRasterBand(i + 1);
                        break;
                    case ColorInterp.GCI_GreenBand:
                        green = true;
                        bandcolors.Green = m_GdalDataset.GetRasterBand(i + 1);
                        break;
                    case ColorInterp.GCI_BlueBand:
                        blue = true;
                        bandcolors.Blue = m_GdalDataset.GetRasterBand(i + 1);
                        break;
                    case ColorInterp.GCI_PaletteIndex:
                        palette = true;
                        break;
                    case ColorInterp.GCI_GrayIndex:
                        grayindex = true;
                        break;
                }
            }

            if ((red == blue && blue == green && green == true) || m_BandCount >= 3)
            {
                m_RasterType = RasterType.RGB;
                if (!(red == blue && blue == green && green == true))
                {
                    bandcolors.Red = m_GdalDataset.GetRasterBand(1);
                    bandcolors.Green = m_GdalDataset.GetRasterBand(2);
                    bandcolors.Blue = m_GdalDataset.GetRasterBand(3);
                }
            }
            else if (m_BandCount == 1 && palette == true)
            {
                m_RasterType = RasterType.Palette;
            }
            else if (m_BandCount == 1 && grayindex == true)
            {
                m_RasterType = RasterType.GrayIndex;
            }
            else
            {
                m_RasterType = RasterType.Invalid;

            }
        }

        public override GeoBound GetBoundingBox()
        {
            if (m_GdalDataset != null)
            {
                double right = 0, left = 0, top = 0, bottom = 0;
                double dblW, dblH;

                string tfwFileName = Path.GetDirectoryName(PathName) + "\\" + LayerName + ".tfw";
                if (File.Exists(tfwFileName))
                {
                    FileStream fs = new FileStream(tfwFileName, FileMode.Open, FileAccess.Read);
                    StreamReader m_StreamReader = new StreamReader(fs);
                    fs.Seek(0, SeekOrigin.Begin);
                    int[] order = new[] { 1, 2, 4, 5, 0, 3 };
                    double val = 0;
                    string strLine = null;
                    for (int i = 0; i < 6; i++)
                    {
                        strLine = m_StreamReader.ReadLine();
                        if (double.TryParse(strLine, out val))
                            m_GeoTransform[order[i]] = val;
                        else
                            throw new Exception("坐标数据不对");
                    }
                    m_StreamReader.Close();
                    fs.Close();
                }
                else
                {
                    m_GdalDataset.GetGeoTransform(m_GeoTransform);
                }
                if (m_GeoTransform[0] == 0 && m_GeoTransform[3] == 0)
                {
                    m_GeoTransform = new[] { 999.5, 1, 0, 1000.5, 0, -1 };
                }


                dblW = m_ImageSize.Width;
                dblH = m_ImageSize.Height;

                left = m_GeoTransform[0];
                right = m_GeoTransform[0] + dblW * m_GeoTransform[1];
                top = m_GeoTransform[3];
                bottom = m_GeoTransform[3] + dblH * m_GeoTransform[5];
                return new GeoBound(left, bottom, right, top);
            }

            return null;
        }

        private struct CrossRegion
        {
            public int startX;   //起始点X在图像中的位置
            public int startY;   //起始点Y在图像中的位置
            public int OffSetX;  //在图像中X的偏移范围
            public int OffSetY;  //在图像中Y的偏移范围
            public int ScrXWidth;     //图像在屏幕上显示X范围的大小
            public int ScrYHeight;     //图像在屏幕上显示Y范围的大小
            public int ScrStartX;//图像在屏幕上的X坐标的起点，屏幕坐标
            public int ScrStartY;//图像在屏幕上的Y坐标的起点，屏幕坐标
        }

        private bool GetCrossRegion(Size ScreenSize, ITransForm transform, ref CrossRegion region)
        {
            Rectangle rectScr = new Rectangle(0, 0, ScreenSize.Width, ScreenSize.Height); //屏幕的范围
            Point lb = transform.TransFromWorldToMap(LayerBound.LeftBottomPt);
            Point tr = transform.TransFromWorldToMap(LayerBound.RightUpPt);
            Rectangle rectImage = new Rectangle(Math.Min(lb.X, tr.X), Math.Min(lb.Y, tr.Y), tr.X - lb.X, Math.Abs(tr.Y - lb.Y));

            Rectangle rect = Rectangle.Intersect(rectScr, rectImage); //与屏幕相交的矩形范围
            if (rect.IsEmpty)
                return false;

            GeoPoint ptlb = transform.TransFromMapToWorld(new Point(rect.Left, rect.Top));
            GeoPoint ptrt = transform.TransFromMapToWorld(new Point(rect.Right, rect.Bottom));
            int endx, endy;
            region.startX = Math.Abs(Convert.ToInt32((ptlb.X - m_GeoTransform[0]) / m_GeoTransform[1]));
            region.startY = Math.Abs(Convert.ToInt32((ptlb.Y - m_GeoTransform[3]) / m_GeoTransform[5]));

            endx = Math.Abs(Convert.ToInt32((ptrt.X - m_GeoTransform[0]) / m_GeoTransform[1]));
            endy = Math.Abs(Convert.ToInt32((ptrt.Y - m_GeoTransform[3]) / m_GeoTransform[5]));



            region.ScrXWidth = Math.Abs(rect.Right - rect.Left);
            region.ScrYHeight = Math.Abs(rect.Top - rect.Bottom);

            region.ScrStartX = rect.Left;
            region.ScrStartY = rect.Top;


            region.OffSetX = Math.Abs(endx - region.startX);
            region.OffSetY = Math.Abs(endy - region.startY);

            if ((region.OffSetX + region.startX) > m_ImageSize.Width)
                region.OffSetX = m_ImageSize.Width - region.startX;
            if ((region.OffSetY + region.startY) > m_ImageSize.Height)
                region.OffSetY = m_ImageSize.Height - region.startY;


            return true;
        }

        private unsafe void PaletteImage(CrossRegion rgn, BitmapData bmpData)
        {
            Band band = m_GdalDataset.GetRasterBand(1);
            ColorTable clrTable = band.GetColorTable();

            if (clrTable == null)
                return;

            byte[] Buffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
            band.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, Buffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);

            int index = 0;
            for (int y = 0; y < bmpData.Height; y++)
            {
                byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;
                for (int x = 0; x < bmpData.Width; x++)
                {
                    Int32 offsetX = x * iPixelSize;
                    ColorEntry clrEntry = clrTable.GetColorEntry(Buffer[index]);

                    row[offsetX] = (byte)clrEntry.c3;
                    row[offsetX + 1] = (byte)clrEntry.c2;
                    row[offsetX + 2] = (byte)clrEntry.c1;
                    index++;
                }

            }
        }

        //private unsafe void RGBImage(CrossRegion rgn, BitmapData bmpData)
        //{
        //    Band RedBand;
        //    Band GreenBand;
        //    Band BlueBand;
        //    byte[] RedBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
        //    byte[] GreenBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
        //    byte[] BlueBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];

        //    for (int i = 0; i < 3; i++)
        //    {
        //        Band band = m_GdalDataset.GetRasterBand(i + 1);
        //        ColorInterp clrInterp = band.GetColorInterpretation();
        //        if (clrInterp == ColorInterp.GCI_RedBand)
        //        {
        //            RedBand = band;
        //            RedBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
        //            RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
        //        }
        //        else if (clrInterp == ColorInterp.GCI_GreenBand)
        //        {
        //            GreenBand = band;
        //            GreenBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
        //            GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
        //        }
        //        else if (clrInterp == ColorInterp.GCI_BlueBand)
        //        {
        //            BlueBand = band;

        //            BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
        //        }

        //    }

        //    int indexBuffer = 0;
        //    for (int y = 0; y < bmpData.Height; y++)
        //    {
        //        byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;
        //        for (int x = 0; x < bmpData.Width; x++)
        //        {
        //            Int32 offsetX = x * iPixelSize;
        //            row[offsetX] = BlueBuffer[indexBuffer];
        //            row[offsetX + 1] = GreenBuffer[indexBuffer];
        //            row[offsetX + 2] = RedBuffer[indexBuffer];
        //            indexBuffer++;
        //        }

        //    }

        //}

        private unsafe void RGBImage(CrossRegion rgn, BitmapData bmpData)
        {
            Band RedBand;
            Band GreenBand;
            Band BlueBand;

            if (bandcolors.Red.DataType == DataType.GDT_Byte)
            {
                //如果是8位的
                byte[] RedBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
                byte[] GreenBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
                byte[] BlueBuffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];

                if (bandindex >= 0)
                {
                    RedBand = bandcolors.Red.GetOverview(bandindex);
                    RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    GreenBand = bandcolors.Green.GetOverview(bandindex);
                    GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    BlueBand = bandcolors.Blue.GetOverview(bandindex);
                    BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                }
                else
                {
                    RedBand = bandcolors.Red;
                    double []redMM=new double[2];
                    RedBand.ComputeRasterMinMax(redMM,1);
                    RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    for (int i = 0; i < RedBuffer.Length; i++)
                    {
                        RedBuffer[i] = Convert.ToByte((RedBuffer[i] - redMM[0]) * 255 / (redMM[1] - redMM[0]));
                    }

                    GreenBand = bandcolors.Green;
                    double[] GreenMM = new double[2];
                    GreenBand.ComputeRasterMinMax(GreenMM, 1);
                    GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    for (int i = 0; i < GreenBuffer.Length; i++)
                    {
                        GreenBuffer[i] = Convert.ToByte((GreenBuffer[i] - GreenMM[0]) * 255 / (GreenMM[1] - GreenMM[0]));
                    }

                    BlueBand = bandcolors.Blue;
                    double[] BlueMM = new double[2];
                    BlueBand.ComputeRasterMinMax(BlueMM, 1);
                    BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    for (int i = 0; i < BlueBuffer.Length; i++)
                    {
                        BlueBuffer[i] = Convert.ToByte((BlueBuffer[i] - BlueMM[0]) * 255 / (BlueMM[1] - BlueMM[0]));
                    }
                }
                int indexBuffer = 0;
                for (int y = 0; y < bmpData.Height; y++)
                {
                    byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;
                    for (int x = 0; x < bmpData.Width; x++)
                    {
                        Int32 offsetX = x * iPixelSize;
                        row[offsetX] = BlueBuffer[indexBuffer];
                        row[offsetX + 1] = GreenBuffer[indexBuffer];
                        row[offsetX + 2] = RedBuffer[indexBuffer];
                        indexBuffer++;
                    }

                }
            }
            else if (bandcolors.Red.DataType == DataType.GDT_Int16)
            {
                short[] RedBuffer = new short[rgn.ScrXWidth * rgn.ScrYHeight];
                short[] GreenBuffer = new short[rgn.ScrXWidth * rgn.ScrYHeight];
                short[] BlueBuffer = new short[rgn.ScrXWidth * rgn.ScrYHeight];
                if (bandindex >= 0)
                {
                    RedBand = bandcolors.Red.GetOverview(bandindex);
                    RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    GreenBand = bandcolors.Green.GetOverview(bandindex);
                    GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    BlueBand = bandcolors.Blue.GetOverview(bandindex);
                    BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                }
                else
                {
                    RedBand = bandcolors.Red;
                    RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    GreenBand = bandcolors.Green;
                    GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    BlueBand = bandcolors.Blue;
                    BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                }
                int indexBuffer = 0;

                for (int y = 0; y < bmpData.Height; y++)
                {
                    byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;
                    for (int x = 0; x < bmpData.Width; x++)
                    {
                        try
                        {
                            Int32 offsetX = x * iPixelSize;
                            ((int*)row)[0] = BlueBuffer[indexBuffer];
                            row++;
                            row++;
                            ((int*)row)[0] = GreenBuffer[indexBuffer];
                            row++;
                            row++;
                            ((int*)row)[0] = RedBuffer[indexBuffer];
                            row++;
                            row++;
                            indexBuffer++;
                        }
                        catch (Exception ee)
                        {
                            System.Windows.Forms.MessageBox.Show(ee.Message, "提示");
                            return;
                        }
                    }
                }

            }
            else if (bandcolors.Red.DataType == DataType.GDT_Int32)
            {
                int[] RedBuffer = new int[rgn.ScrXWidth * rgn.ScrYHeight];
                int[] GreenBuffer = new int[rgn.ScrXWidth * rgn.ScrYHeight];
                int[] BlueBuffer = new int[rgn.ScrXWidth * rgn.ScrYHeight];
                if (bandindex >= 0)
                {
                    RedBand = bandcolors.Red.GetOverview(bandindex);
                    RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    GreenBand = bandcolors.Green.GetOverview(bandindex);
                    GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    BlueBand = bandcolors.Blue.GetOverview(bandindex);
                    BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                }
                else
                {
                    RedBand = bandcolors.Red;
                    RedBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, RedBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    GreenBand = bandcolors.Green;
                    GreenBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, GreenBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                    BlueBand = bandcolors.Blue;
                    BlueBand.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, BlueBuffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);
                }
                int indexBuffer = 0;

                for (int y = 0; y < bmpData.Height; y++)
                {
                    byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;
                    for (int x = 0; x < bmpData.Width; x++)
                    {
                        //try
                        //{
                        //    Int32 offsetX = x * iPixelSize;
                        //    ((int*)row)[0] = BlueBuffer[indexBuffer];
                        //    row++;
                        //    row++;
                        //    ((int*)row)[0] = GreenBuffer[indexBuffer];
                        //    row++;
                        //    row++;
                        //    ((int*)row)[0] = RedBuffer[indexBuffer];
                        //    row++;
                        //    row++;
                        //    indexBuffer++;
                        //}
                        //catch (Exception ee)
                        //{
                        //    System.Windows.Forms.MessageBox.Show(ee.Message, "提示");
                        //    return;
                        //}

                        Int32 offsetX = x * iPixelSize;
                        ((int*)row)[0] = BlueBuffer[indexBuffer];
                        row++;
                        ((int*)row)[0] = GreenBuffer[indexBuffer];
                        row++;
                        ((int*)row)[0] = RedBuffer[indexBuffer];
                        row++;
                        indexBuffer++;
                    }
                }
            }
        }

        private unsafe void GrayImage(CrossRegion rgn, BitmapData bmpData)
        {
            Band band = m_GdalDataset.GetRasterBand(1);
            ColorTable clrTable = band.GetColorTable();

            //if (clrTable == null)
            //    return;

            byte[] Buffer = new byte[rgn.ScrXWidth * rgn.ScrYHeight];
            band.ReadRaster(rgn.startX, rgn.startY, rgn.OffSetX, rgn.OffSetY, Buffer, rgn.ScrXWidth, rgn.ScrYHeight, 0, 0);

            int index = 0;
            for (int y = 0; y < bmpData.Height; y++)
            {
                byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;
                for (int x = 0; x < bmpData.Width; x++)
                {
                    Int32 offsetX = x * iPixelSize;
                    //ColorEntry clrEntry = clrTable.GetColorEntry(Buffer[index]);
                    //row[offsetX] = (byte)clrEntry.c3;
                    //row[offsetX + 1] = (byte)clrEntry.c2;
                    //row[offsetX + 2] = (byte)clrEntry.c1;

                    row[offsetX] = Buffer[index];
                    row[offsetX + 1] = Buffer[index];
                    row[offsetX + 2] = Buffer[index];
                    index++;
                }

            }
        }

        public void DrawImage(Graphics g, Size ScrSize, ITransForm transform)
        {
            CrossRegion rgn = new CrossRegion();
            if (!GetCrossRegion(ScrSize, transform, ref rgn))
            {
                return;
            }
            Bitmap bmp = new Bitmap(rgn.ScrXWidth, rgn.ScrYHeight, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, rgn.ScrXWidth, rgn.ScrYHeight), ImageLockMode.ReadWrite, bmp.PixelFormat);
            try
            {
                switch (m_RasterType)
                {
                    case RasterType.RGB:
                        RGBImage(rgn, bmpData);
                        break;
                    case RasterType.Palette:
                        PaletteImage(rgn, bmpData);
                        break;
                    case RasterType.GrayIndex:
                        GrayImage(rgn, bmpData);
                        break;
                    default:
                        break;
                }
            }
            catch
            {
                return;
            }
            finally
            {
                if (bmpData != null)
                    bmp.UnlockBits(bmpData);
            }

            g.DrawImage(bmp, new Point(rgn.ScrStartX, rgn.ScrStartY));
            bmp.Dispose();

        }

    }
}