using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OSGeo.GDAL;

namespace GIS.TreeIndex.Forms
{
    public partial class frmChangeDetection : Form
    {
        public frmChangeDetection()
        {
            InitializeComponent();
        }

        string strInput1;
        string strInput2;
        string strOutput;
        private void btnInput1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "影像文件|*.img;*.IMG;*.tif;*.TIF;*.jpg;*.JPEG;*.jpeg;*.JPEG;*.dem;*.adf;*.DEM;*.gif;*.GIF;*.bmp;*.BMP;*.png;*.PNG;*.jp2;*.JP2;*.j2k;*.J2K";
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    this.txtInput1.Text = dlg.FileName;
                    this.strInput1 = dlg.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnInput2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "影像文件|*.img;*.IMG;*.tif;*.TIF;*.jpg;*.JPEG;*.jpeg;*.JPEG;*.dem;*.adf;*.DEM;*.gif;*.GIF;*.bmp;*.BMP;*.png;*.PNG;*.jp2;*.JP2;*.j2k;*.J2K";
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    this.txtInput2.Text = dlg.FileName;
                    this.strInput2 = dlg.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnOutput_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                sfd.Filter = "影像文件(*.img)|*.img";

                if (!sfd.FileName.EndsWith(".img", true, null))
                {
                    this.txtOutput.Text = sfd.FileName + ".img";
                    this.strOutput = sfd.FileName + ".img";
                }
                else
                {
                    this.txtOutput.Text = sfd.FileName;
                    this.strOutput = sfd.FileName;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.txtInput1.Text == "" || this.txtInput2.Text == "")
            {
                MessageBox.Show("请输入完整影像","提示");
                return;
            }
            if (this.txtOutput.Text == "")
            {
                MessageBox.Show("请指定输出影像路径","提示");
                return;
            }

            ChangeDetection();

            MessageBox.Show("处理完成","提示");
            this.Close();
        }

        private void ChangeDetection()
        {
            Gdal.AllRegister();
            Dataset ds1 = Gdal.Open(strInput1, Access.GA_ReadOnly);//被更新影像

            Driver d1 = ds1.GetDriver();
            if (d1 == null)
            {
                MessageBox.Show("不能打开影像！");
                System.Environment.Exit(-1);
            }

            Dataset ds2 = Gdal.Open(strInput2, Access.GA_ReadOnly);//变化影像
            Driver d2 = ds1.GetDriver();

            if (d2 == null)
            {
                MessageBox.Show("不能打开影像！");
                System.Environment.Exit(-1);
            }

            //更新的必须是分类后单波段的影像
            if (ds1.RasterCount != 1 || ds2.RasterCount != 1)
            {
                MessageBox.Show("必须都是分类后单波段影像", "提示");
                ds1.Dispose();
                ds2.Dispose();
                return;
            }

            Band band1 = ds1.GetRasterBand(1);//被更新的波段
            Band band2 = ds2.GetRasterBand(1);

            Size size1 = new Size(ds1.RasterXSize, ds1.RasterYSize);
            Size size2 = new Size(ds2.RasterXSize, ds2.RasterYSize);

            if (size1 != size2)
            {
                MessageBox.Show("两幅影像大小不一致");
                band1.Dispose();
                band2.Dispose();
                ds1.Dispose();
                ds2.Dispose();
                return;
            }

            FileStream fs = null;
            StreamWriter sw = null;
            try
            {

                //读取所有像素
                byte[] Buffer1 = new byte[size1.Width * size1.Height];
                band1.ReadRaster(0, 0, size1.Width, size1.Height, Buffer1, size1.Width, size1.Height, 0, 0);

                //变化影像
                byte[] Buffer2 = new byte[band2.XSize * band2.YSize];
                band2.ReadRaster(0, 0, band2.XSize, band2.YSize, Buffer2, band2.XSize, band2.YSize, 0, 0);


                double[] m_GeoTransform = new double[6];
                ds1.GetGeoTransform(m_GeoTransform);
                string strPro1 = ds1.GetProjection();

                Driver dr = Gdal.GetDriverByName("HFA");
                dr.Create(strOutput, size1.Width, size1.Height, 1, DataType.GDT_Byte, null);
                OSGeo.GDAL.Dataset ods = Gdal.Open(strOutput, Access.GA_Update);
                ods.SetProjection(strPro1);
                ods.SetGeoTransform(m_GeoTransform);
                Band b = ods.GetRasterBand(1);//被更新的波段

                //byte[] b1 = new byte[1] { 1 };

                //using (FileStream fileStream = File.Create(fileName))
                //{
                //    fileStream.W
                //    fileStream.Close();
                //}

                string fileName = System.IO.Path.GetDirectoryName(strInput1) + "\\变化说明_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fs);

                Dictionary<KeyValuePair<byte, byte>, byte> dic = new Dictionary<KeyValuePair<byte, byte>, byte>();

                byte value = 0;

                for (int i = 0; i < Buffer1.Length; i++)
                {
                    int k = i / size1.Width;
                    int h = i % size1.Width;

                    if (Buffer1[i] == Buffer2[i])
                        continue;
                    else if (Buffer1[i] != Buffer2[i])  //有变化
                    {
                        if (!dic.ContainsKey(new KeyValuePair<byte, byte>(Buffer1[i], Buffer2[i])))//没有存储此类变化
                        {
                            value++;
                            dic.Add(new KeyValuePair<byte, byte>(Buffer1[i], Buffer2[i]), value);

                        }
                        else
                        {
                            byte dd = dic[new KeyValuePair<byte, byte>(Buffer1[i], Buffer2[i])];
                            byte[] b1 = new byte[1] { dd };
                            b.WriteRaster(h, k, 1, 1, b1, 1, 1, 0, 0);
                        }
                    }
                }

                foreach (KeyValuePair<KeyValuePair<byte, byte>, byte> item in dic)
                {

                    sw.WriteLine(string.Format("类型:{0}变化为类型:{1}取值{2}", item.Key.Key.ToString(), item.Key.Value.ToString(),item.Value.ToString() ));
                }

                b.Dispose();
                ods.Dispose();

                band1.Dispose();
                band2.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                band1.Dispose();
                band2.Dispose();
                return;
            }
            finally
            {
                ds1.Dispose();
                ds2.Dispose();
                sw.Close();
                fs.Close();
            }

        }



        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       
    }
}
