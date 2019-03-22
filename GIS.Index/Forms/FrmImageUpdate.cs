using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using OSGeo.GDAL;
using System.Text;
using System.Windows.Forms;

namespace GIS.TreeIndex.Forms
{
    public partial class FrmImageUpdate : Form
    {
        string strRasterOriginal;
        string strRasterChange;

        public FrmImageUpdate()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "影像文件|*.img;*.IMG;*.tif;*.TIF;*.jpg;*.JPEG;*.jpeg;*.JPEG;*.dem;*.adf;*.DEM;*.gif;*.GIF;*.bmp;*.BMP;*.png;*.PNG;*.jp2;*.JP2;*.j2k;*.J2K;*.hdr";
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    strRasterOriginal = dlg.FileName;
                    this.textBox1.Text = dlg.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "影像文件|*.img;*.IMG;*.tif;*.TIF;*.jpg;*.JPEG;*.jpeg;*.JPEG;*.dem;*.adf;*.DEM;*.gif;*.GIF;*.bmp;*.BMP;*.png;*.PNG;*.jp2;*.JP2;*.j2k;*.J2K;*.hdr";
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    strRasterChange = dlg.FileName;
                    this.textBox2.Text = dlg.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.textBox1.Text == "" || this.textBox2.Text == "")
            {
                MessageBox.Show("请设置完整影像路径！");
                return;
            }
            UpDateRaster();
        }

        private void UpDateRaster()
        {
            Gdal.AllRegister();
            Dataset OrgDataSet = Gdal.Open(strRasterOriginal, Access.GA_Update);//被更新影像

            Driver d1 = OrgDataSet.GetDriver();
            if (d1 == null)
            {
               MessageBox.Show("不能打开影像！");
                System.Environment.Exit(-1);
            }

            Dataset IncDataSet = Gdal.Open(strRasterChange, Access.GA_ReadOnly);//变化影像
            Driver d2 = OrgDataSet.GetDriver();

            if (d2 == null)
            {
                MessageBox.Show("不能打开影像！");
                System.Environment.Exit(-1);
            }

            //更新的必须是分类后单波段的影像
            if (OrgDataSet.RasterCount != 1 || IncDataSet.RasterCount != 1)
            {
                MessageBox.Show("必须都是分类后单波段影像","提示");
                OrgDataSet.Dispose();
                IncDataSet.Dispose();
                return;
            }

            Band band1 = OrgDataSet.GetRasterBand(1);//被更新的波段
            Band band2 = IncDataSet.GetRasterBand(1);
           
            Size size1 = new Size(OrgDataSet.RasterXSize, OrgDataSet.RasterYSize);
            Size size2 = new Size(IncDataSet.RasterXSize,IncDataSet.RasterYSize);

            if (size1 != size2)
            {
                MessageBox.Show("两幅影像大小不一致");
                band1.Dispose();
                band2.Dispose();
                OrgDataSet.Dispose();
                IncDataSet.Dispose();
                return;
            }

            double[] OrgGeoTransform = new double[6];
            OrgDataSet.GetGeoTransform(OrgGeoTransform);

            double[] IncGeoTransform = new double[6];
            OrgDataSet.GetGeoTransform(IncGeoTransform);

            //读取所有像素
            byte[] Buffer = new byte[size1.Width * size1.Height];
            band1.ReadRaster(0, 0, size1.Width, size1.Height, Buffer, size1.Width, size1.Height, 0, 0);

            //变化影像
            byte[] Buf = new byte[band2.XSize * band2.YSize];
            band2.ReadRaster(0, 0, band2.XSize, band2.YSize, Buf, band2.XSize, band2.YSize, 0, 0);

            byte[] b1 = new byte[1] { 1 };
            byte[] b2 = new byte[1] { 2 };
            byte[] b3 = new byte[1] { 3 };
            byte[] b4 = new byte[1] { 4 };
            byte[] b5 = new byte[1] { 5 };
            byte[] b6 = new byte[1] { 6 };
            byte[] b7 = new byte[1] { 7 };

            //目前只支持这8个分类
            for (int i = 0; i < Buffer.Length; i++)
            {
                int k = i / size1.Width;
                int h = i % size1.Width;

                if (Buf[i] == 1)
                {
                    band1.WriteRaster(h, k, 1, 1, b1, 1, 1, 0, 0);
                }
                if (Buf[i] == 2)
                {
                    band1.WriteRaster(h, k, 1, 1, b2, 1, 1, 0, 0);
                }
                if (Buf[i] == 3)
                {
                    band1.WriteRaster(h, k, 1, 1, b3, 1, 1, 0, 0);
                }
                if (Buf[i] == 4)
                {
                    band1.WriteRaster(h, k, 1, 1, b4, 1, 1, 0, 0);
                }
                if (Buf[i] == 5)
                {
                    band1.WriteRaster(h, k, 1, 1, b5, 1, 1, 0, 0);
                }
                if (Buf[i] == 6)
                {
                    band1.WriteRaster(h, k, 1, 1, b6, 1, 1, 0, 0);
                }
                if (Buf[i] == 7)
                {
                    band1.WriteRaster(h, k, 1, 1, b7, 1, 1, 0, 0);
                }
            }

            band1.Dispose();
            band2.Dispose();
            OrgDataSet.Dispose();
            IncDataSet.Dispose();

            MessageBox.Show("处理完成");
            this.Close();
        }
    
    }
}
