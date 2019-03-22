using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Drawing;

namespace GIS.GdiAPI
{
    public class GDIAPI
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr windowhander);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr windowhander);

        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        //获取屏size.cx,size.cy

        public enum PenStyles
        {
            PS_SOLID = 0
            ,
            PS_DASH = 1
                ,
            PS_DOT = 2
                ,
            PS_DASHDOT = 3
                ,
            PS_DASHDOTDOT = 4
                ,
            PS_NULL = 5
                ,
            PS_INSIDEFRAME = 6
                ,
            PS_USERSTYLE = 7
                ,
            PS_ALTERNATE = 8
                ,
            PS_STYLE_MASK = 0x0000000F

                ,
            PS_ENDCAP_ROUND = 0x00000000
                ,
            PS_ENDCAP_SQUARE = 0x00000100
                ,
            PS_ENDCAP_FLAT = 0x00000200
                ,
            PS_ENDCAP_MASK = 0x00000F00
                ,
            PS_JOIN_ROUND = 0x00000000
                ,
            PS_JOIN_BEVEL = 0x00001000
                ,
            PS_JOIN_MITER = 0x00002000
                ,
            PS_JOIN_MASK = 0x0000F000

                ,
            PS_COSMETIC = 0x00000000
                ,
            PS_GEOMETRIC = 0x00010000
                , PS_TYPE_MASK = 0x000F0000
        }
        public enum drawingMode
        {
            R2_BLACK = 1   /*  0       */
            ,
            R2_NOTMERGEPEN = 2   /* DPon     */
                ,
            R2_MASKNOTPEN = 3   /* DPna     */
                ,
            R2_NOTCOPYPEN = 4   /* PN       */
                ,
            R2_MASKPENNOT = 5   /* PDna     */
                ,
            R2_NOT = 6   /* Dn       */
                ,
            R2_XORPEN = 7   /* DPx      */
                ,
            R2_NOTMASKPEN = 8   /* DPan     */
                ,
            R2_MASKPEN = 9   /* DPa      */
                ,
            R2_NOTXORPEN = 10  /* DPxn     */
                ,
            R2_NOP = 11  /* D        */
                ,
            R2_MERGENOTPEN = 12  /* DPno     */
                ,
            R2_COPYPEN = 13  /* P        */
                ,
            R2_MERGEPENNOT = 14  /* PDno     */
                ,
            R2_MERGEPEN = 15  /* DPo      */
                ,
            R2_WHITE = 16  /*  1       */
                , R2_LAST = 16
        }
        public static int RGB(int R, int G, int B)
        {
            return (R | (G << 8) | (B << 16));
        }

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        public static extern IntPtr CreatePen(PenStyles fnPenStyle, int nWidth, int crColor);

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(int crColor);

        //[System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]


        [System.Runtime.InteropServices.DllImport("gdi32")]
        public static extern IntPtr GetStockObject(int fnObject);

        [System.Runtime.InteropServices.DllImport("gdi32")]
        public static extern bool BeginPath(IntPtr hdc);
        [System.Runtime.InteropServices.DllImport("gdi32")]
        public static extern bool EndPath(IntPtr hdc);

        [System.Runtime.InteropServices.DllImport("gdi32")]
        public static extern bool FillPath(IntPtr hdc);

        [System.Runtime.InteropServices.DllImport("gdi32")]
        public static extern int SetBkMode(IntPtr hdc, int nBkMode);
        [System.Runtime.InteropServices.DllImport("gdi32")]
        public static extern IntPtr StrokePath(IntPtr hdc);
        [System.Runtime.InteropServices.DllImport("gdi32")]
        public static extern int TextOut(IntPtr hdc, int x, int y, string lpString, int nCount);

        [System.Runtime.InteropServices.DllImport("gdi32")]
        public static extern bool MoveToEx(IntPtr hdc, int x, int y, IntPtr lppoint);

        [System.Runtime.InteropServices.DllImport("gdi32")]
        public static extern bool LineTo(IntPtr hdc, int x, int y);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int abc);

        [DllImport("gdi32.dll")]
        public extern static IntPtr GetCurrentObject(IntPtr hdc, ushort objectType);
        [DllImport("user32.dll")]
        public extern static void ReleaseDC(IntPtr hdc);
        [DllImport("User32.dll")]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);
        /// <summary>
        /// CreateCompatibleDC
        /// </summary>
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [System.Runtime.InteropServices.DllImportAttribute("USER32.DLL")]
        //public static extern long FillRect(IntPtr hDc, Rectangle lpRect, IntPtr hBrush);
        public static extern int FillRect(IntPtr hDC, ref Rectangle rect, IntPtr hBrush);
        /// <summary>
        /// DeleteDC
        /// </summary>
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        /// <summary>
        /// SelectObject
        /// </summary>
        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr SelectObject(IntPtr hDC,
                                                  IntPtr hObject);

        /// <summary>
        /// DeleteObject
        /// </summary>
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// CreateCompatibleBitmap
        /// </summary>
        [DllImport("gdi32.dll",
                   ExactSpelling = true,
                   SetLastError = true)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hObject,
                                                           int width,
                                                           int height);

        /// <summary>
        ///
        /// </summary>
        /// <param name="hdcDest">目标设备的句柄</param>
        /// <param name="nXDest">目标对象的左上角的X坐标 </param>
        /// <param name="nYDest">目标对象的左上角的Y坐标 </param>
        /// <param name="nWidth">目标对象的矩形的宽度   </param>
        /// <param name="nHeight">目标对象的矩形的长度 </param>
        /// <param name="hdcSrc">源设备的句柄</param>
        /// <param name="nXSrc">源对象的左上角的X坐标 </param>
        /// <param name="nYSrc">源对象的左上角的X坐标 </param>
        /// <param name="dwRop">光栅的操作值 </param>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        public static extern bool BitBlt(
        IntPtr hdcDest,     //    目标设备的句柄  
        int nXDest,         //    目标对象的左上角的X坐标  
        int nYDest,         //    目标对象的左上角的Y坐标  
        int nWidth,         //    目标对象的矩形的宽度  
        int nHeight,        //    目标对象的矩形的长度  
        IntPtr hdcSrc,      //    源设备的句柄  
        int nXSrc,          //    源对象的左上角的X坐标  
        int nYSrc,          //    源对象的左上角的X坐标  
        TernaryRasterOperations dwRop //    光栅的操作值  
        );

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        public static extern bool StretchBlt(
        IntPtr hdcDest,     //    目标设备的句柄  
        int nXDest,         //    目标对象的左上角的X坐标  
        int nYDest,         //    目标对象的左上角的Y坐标  
        int nWidthdest,         //    目标对象的矩形的宽度  
        int nHeightdest,        //    目标对象的矩形的长度  
        IntPtr hdcSrc,      //    源设备的句柄  
        int nXSrc,          //    源对象的左上角的X坐标  
        int nYSrc,          //    源对象的左上角的X坐标  
        int nWidthSrc,    // width of source rectangle
        int nHeightSrc,   // height of source rectangle
        TernaryRasterOperations dwRop //    光栅的操作值  
        );

        /// <summary>
        ///
        /// </summary>
        /// <param name="lpszDriver">驱动名称</param>
        /// <param name="lpszDevice">设备名称</param>
        /// <param name="lpszOutput">无用，可以设定位"NULL"   </param>
        /// <param name="lpInitData">任意的打印机数据   </param>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        public static extern IntPtr CreateDC(
        string lpszDriver,    //    驱动名称  
        string lpszDevice,    //    设备名称  
        string lpszOutput,    //    无用，可以设定位"NULL"  
        IntPtr lpInitData    //    任意的打印机数据  
        );
        /// <summary>
        /// Enumeration for the raster operations used in BitBlt.
        /// In C++ these are actually #define. But to use these
        /// constants with C#, a new enumeration type is defined.
        /// </summary>
        public enum TernaryRasterOperations
        {
            SRCCOPY = 0x00CC0020, /* dest = source                   */
            SRCPAINT = 0x00EE0086, /* dest = source OR dest           */
            SRCAND = 0x008800C6, /* dest = source AND dest          */
            SRCINVERT = 0x00660046, /* dest = source XOR dest          */
            SRCERASE = 0x00440328, /* dest = source AND (NOT dest )   */
            NOTSRCCOPY = 0x00330008, /* dest = (NOT source)             */
            NOTSRCERASE = 0x001100A6, /* dest = (NOT src) AND (NOT dest) */
            MERGECOPY = 0x00C000CA, /* dest = (source AND pattern)     */
            MERGEPAINT = 0x00BB0226, /* dest = (NOT source) OR dest     */
            PATCOPY = 0x00F00021, /* dest = pattern                  */
            PATPAINT = 0x00FB0A09, /* dest = DPSnoo                   */
            PATINVERT = 0x005A0049, /* dest = pattern XOR dest         */
            DSTINVERT = 0x00550009, /* dest = (NOT dest)               */
            BLACKNESS = 0x00000042, /* dest = BLACK                    */
            WHITENESS = 0x00FF0062, /* dest = WHITE                    */
        };
    }
}
