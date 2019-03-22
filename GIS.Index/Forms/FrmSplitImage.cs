using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using OSGeo.GDAL;
using System.Text;
using System.Windows.Forms;
using GIS.Geometries;

namespace GIS.TreeIndex.Forms
{
    public partial class FrmSplitImage : Form
    {
        public FrmSplitImage()
        {
            InitializeComponent();
        }

        private string strPath;
        private double[] m_GeoTransform = new double[6];
        private int n;
        private int m;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "影像文件|*.img;*.IMG;*.tif;*.TIF;*.jpg;*.JPEG;*.jpeg;*.JPEG;*.dem;*.adf;*.DEM;*.gif;*.GIF;*.bmp;*.BMP;*.png;*.PNG;*.jp2;*.JP2;*.j2k;*.J2K";
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    strPath = dlg.FileName;
                    this.txtPath.Text = dlg.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //分割影像
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.txtPath.Text == "")
            {
                MessageBox.Show("影像路径不能为空","提示");
                return;
            }

            if (this.txtNO.Text.Trim() == "" || this.txtNO.Text.Trim() == "0" || this.txtNO2.Text.Trim() =="0"|| this.txtNO2.Text.Trim() == "")
            {
                MessageBox.Show("切割方案不正确","提示");
                return;
            }

            if (this.txtNO.Text.Trim() == "1" && this.txtNO2.Text.Trim() == "2")
            {
                MessageBox.Show("结果就是影像本身，无需切割","提示");
                return;
            }

            try
            {

                n = Convert.ToInt32(this.txtNO.Text.Trim());
                m = Convert.ToInt32(this.txtNO2.Text.Trim());

                SpiltRaster();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"提示");
                return;
            }
        }

        //分割影像
        private void SpiltRaster()
        {
            Gdal.AllRegister();
            Dataset m_DataSet = Gdal.OpenShared(strPath, Access.GA_ReadOnly);

            if (m_DataSet == null)
            {
                MessageBox.Show("打开影像失败", "提示");
                return;
            }

            Driver d = m_DataSet.GetDriver();
            if (d == null)
            {
                MessageBox.Show("不能打开影像！");
                System.Environment.Exit(-1);
            }

            string strPro1 = m_DataSet.GetProjection();

            Band band = m_DataSet.GetRasterBand(1);//原始影像
            m_DataSet.GetGeoTransform(m_GeoTransform);
            ColorTable colormap = band.GetColorTable();
            int ii = colormap.GetCount();

            int x = band.XSize / n;
            int y = band.YSize / m;
            int x1 = band.XSize - x * (n - 1);
            int y1 = band.YSize - y * (m - 1);

            Driver dr = Gdal.GetDriverByName("HFA");

            for (int j = 0; j < m; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    int nx = x;
                    int ny = y;
                    //string str = strPath.Substring(0, strPath.LastIndexOf('.')) + "_" + (j + 1).ToString() + "_" + (i + 1).ToString() + ".img";

                    string str = strPath.Substring(0, strPath.LastIndexOf('.'))  + (j + 1).ToString()  + (i + 1).ToString() + ".img";

                    if (j == m - 1)
                    {
                         ny = y1;
                    }
                    if (i == n - 1)
                    {
                        nx = x1;
                    }

                    double[] transforms = GeoTransformRC(i * x, j * y);//得到相应的geotransform数组
                    dr.Create(str, nx, ny, 1, DataType.GDT_Byte, null);
                    OSGeo.GDAL.Dataset ds = Gdal.Open(str, Access.GA_Update);
                    ds.SetProjection(strPro1);
                    ds.SetGeoTransform(transforms);

                    byte[] buffer = new byte[nx * ny];
                    band.ReadRaster(i*x, j*y, nx, ny, buffer, nx, ny, 0, 0);//读取原影像中相应的值

                    Band b = ds.GetRasterBand(1);//被更新的波段
                    b.SetColorTable(colormap);
                    b.WriteRaster(0, 0, nx, ny, buffer, nx, ny, 0, 0);
                    b.Dispose();
                    buffer = null;
                    ds.Dispose();
                }
            }

            dr.Dispose();

            band.Dispose();
            m_DataSet.Dispose();

            MessageBox.Show("处理完成","提示");
            this.Close();
        }

        #region 已经废弃
        /*
        private void SpiltRasterBackUp()
        {
            Gdal.AllRegister();
            Dataset m_DataSet = Gdal.OpenShared(strPath, Access.GA_ReadOnly);

            Driver d = m_DataSet.GetDriver();
            if (d == null)
            {
                MessageBox.Show("不能打开影像！");
                System.Environment.Exit(-1);
            }

            //ImageSize = new Size() { Width = m_DataSet.RasterXSize, Height = m_DataSet.RasterYSize };
            //sx=m_DataSet.RasterXSize;
            //sy=m_DataSet.RasterYSize;
            
            string strPro1 = m_DataSet.GetProjection();
            string strPro2 = m_DataSet.GetProjectionRef();

            Band band = m_DataSet.GetRasterBand(1);//原始影像
            //double[] adfGeoTransform = new double[6];
            m_DataSet.GetGeoTransform(m_GeoTransform);
            ColorTable colormap= band.GetColorTable();
            int ii= colormap.GetCount();

            int x = band.XSize / 2;
            int y = band.YSize / 2;
            int x1 = band.XSize - x;
            int y1 = band.YSize - y;


            Driver dr = Gdal.GetDriverByName("HFA");

            string s1 = strPath.Substring(0, strPath.LastIndexOf('.')) + "01.img";
            string s2 = strPath.Substring(0, strPath.LastIndexOf('.')) + "02.img";
            string s3 = strPath.Substring(0, strPath.LastIndexOf('.')) + "03.img";
            string s4 = strPath.Substring(0, strPath.LastIndexOf('.')) + "04.img";

            dr.Create(s1, x, y, 1, DataType.GDT_Byte, null);
            dr.Create(s2, x1, y, 1, DataType.GDT_Byte, null);
            dr.Create(s3, x, y1, 1, DataType.GDT_Byte, null);
            dr.Create(s4, x1, y1, 1, DataType.GDT_Byte, null);

            Dataset ds1 = Gdal.Open(s1, Access.GA_Update);
            Dataset ds2 = Gdal.Open(s2, Access.GA_Update);//新创建影像
            Dataset ds3 = Gdal.Open(s3, Access.GA_Update);//新创建影像
            Dataset ds4 = Gdal.Open(s4, Access.GA_Update);//新创建影像

            ds1.SetProjection(strPro1);
            ds1.SetGeoTransform(m_GeoTransform);

            double[] transform;
            transform = GeoTransformRC(x,0);
            ds2.SetProjection(strPro1);
            ds2.SetGeoTransform(transform);

            transform = GeoTransformRC(0,y);
            ds3.SetProjection(strPro1);
            ds3.SetGeoTransform(transform);

            transform = GeoTransformRC(x,y);
            ds4.SetProjection(strPro1);
            ds4.SetGeoTransform(transform);


            //Size size = new Size() { Width = band.XSize, Height = band.YSize };
            Size size1 = new Size() { Width = x, Height = y };
            Size size2 = new Size() { Width = x1, Height = y };
            Size size3 = new Size() { Width = x, Height = y1 };
            Size size4 = new Size() { Width = x1, Height = y1 };


            //读取第一块
            byte[] Buffer1 = new byte[x * y];
            band.ReadRaster(0, 0, x, y, Buffer1, x, y, 0, 0);
            Band band1 = ds1.GetRasterBand(1);//被更新的波段
            band1.SetColorTable(colormap);
            //for (int i = 0; i < y; i++)
            //{
            //    for (int j = 0; j < x; j++)
            //    {
            //        byte[] b = new byte[1] { Buffer[j + i * sx] };
            //        band1.WriteRaster(j, i, 1, 1, b, 1, 1, 0, 0);
            //    }
            //}
            band1.WriteRaster(0,0,x,y,Buffer1,x,y,0,0);
            band1.Dispose();
            Buffer1 = null;

            byte[] Buffer2 = new byte[x1 * y];
            band.ReadRaster(x, 0, x1, y, Buffer2, x1, y, 0, 0);
            Band band2 = ds2.GetRasterBand(1);//被更新的波段
            band2.SetColorTable(colormap);
            //for (int i = 0; i < y; i++)
            //{
            //    for (int j = 0; j < x1; j++)
            //    {
            //        byte[] b = new byte[1] { Buffer[x+j + i * sx] };
            //        band2.WriteRaster(j, i, 1, 1, b, 1, 1, 0, 0);
            //    }
            //}
            band2.WriteRaster(0, 0, x1, y, Buffer2, x1, y, 0, 0);
            band2.Dispose();
            Buffer2 = null;


            byte[] Buffer3 = new byte[x * y1];
            band.ReadRaster(0, y, x, y1, Buffer3, x, y1, 0, 0);
            Band band3 = ds3.GetRasterBand(1);//被更新的波段
            band3.SetColorTable(colormap);
            //for (int i = 0; i < y1; i++)
            //{
            //    for (int j = 0; j < x; j++)
            //    {
            //        byte[] b = new byte[1] { Buffer[j + i * sx + y * sx] };
            //        band3.WriteRaster(j, i, 1, 1, b, 1, 1, 0, 0);
            //    }
            //}
            band3.WriteRaster(0, 0, x, y1, Buffer3, x, y1, 0, 0);
            band3.Dispose();
            Buffer3 = null;

            byte[] Buffer4 = new byte[x1 * y1];
            band.ReadRaster(x, y, x1, y1, Buffer4, x1, y1, 0, 0);
            Band band4 = ds4.GetRasterBand(1);//被更新的波段
            band4.SetColorTable(colormap);
            //for (int i = 0; i < y1; i++)
            //{
            //    for (int j = 0; j < x1; j++)
            //    {
            //        byte[] b = new byte[1] { Buffer[x + j + i * sx + y * sx] };
            //        band4.WriteRaster(j, i, 1, 1, b, 1, 1, 0, 0);
            //    }
            //}
            band4.WriteRaster(0, 0, x1, y1, Buffer4, x1, y1, 0, 0);
            band4.Dispose();
            Buffer4 = null;
            band.Dispose();
            
            m_DataSet.Dispose();
            ds1.Dispose();
            ds2.Dispose();
            ds3.Dispose();
            ds4.Dispose();

            MessageBox.Show("处理完成");
            this.Close();
        }
         */
       #endregion

        //只准输入数字
        private void txtNO_KeyPress(object sender, KeyPressEventArgs e)
        {
            int ikc = (int)e.KeyChar;
            if ((!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), "[0-9]")) && ((int)e.KeyChar) != 8)
            {
                e.Handled = true;
                return;
            }
        }

        private void txtNO2_KeyPress(object sender, KeyPressEventArgs e)
        {
            int ikc = (int)e.KeyChar;
            if ((!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), "[0-9]")) && ((int)e.KeyChar) != 8)
            {
                e.Handled = true;
                return;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public double[] GeoTransformRC(int row, int col)
        {
            //m_GeoTransform[0],m_GeoTransform[3]分别为栅格左上角的地理X,Y坐标: 转变为地理坐标,写到shape中时，计算面积才正确的
            //则栅格左下角地理坐标,矢量坐标原点地理坐标为:

            //例如:某图像上(P,L)点左上角的实际空间坐标为：
            //Xp = GeoTransform[0] + P * GeoTransform[1] + L * GeoTransform[2];
            //Yp = GeoTransform[3] + P * GeoTransform[4] + L * GeoTransform[5];

            double[] result = new double[6];
            result[0] = m_GeoTransform[0] + row * m_GeoTransform[1] + col * m_GeoTransform[2];
            result[1] = m_GeoTransform[1];
            result[2] = m_GeoTransform[2];
            result[3] = m_GeoTransform[3] + row * m_GeoTransform[4] + col * m_GeoTransform[5];
            result[4] = m_GeoTransform[4];
            result[5] = m_GeoTransform[5];

            //double XGeo = m_GeoTransform[0];
            //double YGeo = m_GeoTransform[3] + m_GeoTransform[5] * m_ImageSize.Height;

            ////矢量坐标点：将一个栅格格点30*30m，当做长度1*1m来表示
            ////因此需乘以实际像元大小：m_GeoTransform[1],m_GeoTransform[5]，比30大一点点，若用准确的30,和栅格影像叠加会有偏差
            ////m_GeoTransform[5]以栅格图像左上角为原点，向下为Y轴正向，矢量Y轴方向相反，需乘-1
            //Pt.X = Pt.X * m_GeoTransform[1] + XGeo;
            //Pt.Y = Pt.Y * (-1) * m_GeoTransform[5] + YGeo;

            return result;
        }
    }
}
