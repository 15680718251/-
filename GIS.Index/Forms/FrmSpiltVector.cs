using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using OSGeo.OGR;
using OSGeo.OSR;

namespace GIS.TreeIndex.Forms
{
    public partial class FrmSpiltVector : Form
    {
        private string strPath;
        private int n;
        private int m;

        BackgroundWorker bw;
        FrmSpiltVectorProgress m_fmProgress;

        public FrmSpiltVector()
        {
            InitializeComponent();

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;

            bw.DoWork += new DoWorkEventHandler(SpiltWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "矢量文件|*.shp";
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

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.txtPath.Text == "")
            {
                MessageBox.Show("影像路径不能为空", "提示");
                return;
            }

            if (this.txtNO.Text.Trim() == "" || this.txtNO.Text.Trim() == "0" || this.txtNO2.Text.Trim() == "0" || this.txtNO2.Text.Trim() == "")
            {
                MessageBox.Show("切割方案不正确", "提示");
                return;
            }

            if (this.txtNO.Text.Trim() == "1" && this.txtNO2.Text.Trim() == "2")
            {
                MessageBox.Show("结果就是影像本身，无需切割", "提示");
                return;
            }

            n = Convert.ToInt32(this.txtNO.Text.Trim());
            m = Convert.ToInt32(this.txtNO2.Text.Trim());

            this.Hide();
            m_fmProgress = new FrmSpiltVectorProgress();
            bw.RunWorkerAsync();
            m_fmProgress.ShowDialog(this);
            this.Close();
        }

        private void VectorSpilt()
        {
            OSGeo.OGR.Driver poDriver;
            string pszDriverName = "ESRI Shapefile";
            OSGeo.OGR.Ogr.RegisterAll();
            poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);

            if (poDriver == null)
            {
                MessageBox.Show("Driver Error", "提示");
                return;
            }

            OSGeo.OGR.DataSource ds1 = poDriver.Open(strPath, 0); //底图

            if (ds1 == null)
            {
                MessageBox.Show("DataSource Error", "提示");
                return;
            }

            OSGeo.OGR.Layer srcLayer = ds1.GetLayerByIndex(0);
            OSGeo.OSR.SpatialReference sr = srcLayer.GetSpatialRef();

            OSGeo.OGR.Envelope srcEnvelope = new Envelope(); 
            srcLayer.GetExtent(srcEnvelope, 0);

            double x1 = srcEnvelope.MaxX - srcEnvelope.MinX;
            double y1 = srcEnvelope.MaxY - srcEnvelope.MinY;

            string folder = System.IO.Path.GetDirectoryName(strPath);
            string tmpFolder = folder + "\\SpiltVec";
            string srcfilename = System.IO.Path.GetFileNameWithoutExtension(strPath);

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

            int ccc = 0;
            List<OSGeo.OGR.Geometry> glists = new List<Geometry>();
            try
            {
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

                        //开始分割多边形
                        OSGeo.OGR.DataSource ds3 = poDriver.CreateDataSource(tmpFolder, null);
                        if (ds3 == null)
                        {
                            MessageBox.Show("DataSource Error", "提示");
                            return;
                        }

                        string srcName = srcfilename + "-" + (j + 1).ToString() + "-" + (i + 1).ToString();

                        m_fmProgress.lblDescription.Invoke(
                           (MethodInvoker)delegate()
                           {
                               m_fmProgress.lblDescription.Text = "正在生成:" + srcfilename;
                           }
                           );


                        OSGeo.OGR.Layer newSrclayer = ds3.CreateLayer(srcName, sr, wkbGeometryType.wkbMultiPolygon, null);
                        OSGeo.OGR.FieldDefn fld = new FieldDefn("PlgAttr", FieldType.OFTString);
                        newSrclayer.CreateField(fld, 0);

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

                                OSGeo.OGR.Geometry tg = g.Intersection(geo);
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

                        ds3.Dispose();

                        int step = 100 * (j * n + i + 1) / (m * n);
                        SetProgressBar(step);

                    }

                }

                m_fmProgress.lblDescription.Invoke(
                   (MethodInvoker)delegate()
                   {
                       m_fmProgress.lblDescription.Text = "执行完成";
                   }
                   );

            }
            catch (Exception ex)
            {
                srcLayer.Dispose();
                ds1.Dispose();

                MessageBox.Show("矢量图层分块错误", "提示");

                return;
            }

            srcLayer.Dispose();
            ds1.Dispose();
            poDriver.Dispose();
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

        private void txtNO2_KeyPress(object sender, KeyPressEventArgs e)
        {
            int ikc = (int)e.KeyChar;
            if ((!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), "[0-9]")) && ((int)e.KeyChar) != 8)
            {
                e.Handled = true;
                return;
            }
        }

        private void txtNO_KeyPress(object sender, KeyPressEventArgs e)
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

        private void SpiltWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                VectorSpilt();
            }
            catch (Exception ex)
            {
                throw (new ApplicationException(ex.ToString()));
            }
            finally
            {
                //m_fmProgress.Dispose();
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                return;
            }

            m_fmProgress.progressBar1.Value = 100; //maxvalue位1000
            m_fmProgress.btnOK.Visible = true;
            //mapUI.OutPutTextInfo("矢量更新化完成");
            m_fmProgress.Text = "执行完成";
            m_fmProgress.lblDescription.Text = "更新完成";

            if (m_fmProgress.checkBox1.Checked)
                m_fmProgress.Dispose();
        }

        private void SetProgressBar(int value)
        {
            m_fmProgress.progressBar1.Invoke(
           (MethodInvoker)delegate()
           {
               m_fmProgress.progressBar1.Value = value;
           }
           );
        }
    }
}
