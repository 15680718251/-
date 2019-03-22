using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GIS.Map;
using GIS.Geometries;
using GIS.Utilities;
using GIS.TreeIndex.Tool;
using GIS.Layer;
using System.IO;
using System.Threading;

namespace GIS.TreeIndex
{
    public partial class MapUI : PictureBox
    {
        private void ImportFeatureCodes()
        {
            DateTime time = DateTime.Today;           //初始化时间
            BeginTime = string.Format("{0}{1:d2}{2:d2}", time.Year.ToString(), int.Parse(time.Month.ToString()), int.Parse(time.Day.ToString()));

            m_FeatureCodes = new Dictionary<long, string>();
            string Path = Application.StartupPath + "\\FeatureCode.txt";
            try
            {
                FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, Encoding.Default);
                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] values = line.Split(' ');
                    long szCode = long.Parse(values[0]);
                    string szName = values[1];
                    m_FeatureCodes.Add(szCode, szName);
                    line = sr.ReadLine();
                }
                sr.Close();
                fs.Close();
            }
            catch
            {
                //MessageBox.Show("读取要素编码错误,请将要素编码文件放在程序运行文件夹中", "提示:");
            }
        }
    
        //向图层里导入属性数据
        private void FillAttribute()
        {
            //OutputTextEventHandler evt = new OutputTextEventHandler(OutputText);

            #region 修改
            //GeoLayer draftLayer = GetLayerByName("混合层");
            //GeoVectorLayer DraftLayer = draftLayer as GeoVectorLayer;
            //GeoData.GeoDataTable DraftTable = DraftLayer.DataTable;
            //int num = 0;
            //int num1 = 0;
            #endregion

            for (int j = 0; j < LayerCounts; j++)
            {
                GeoLayer layer = GetLayerAt(j);
                GeoVectorLayer vlyr = layer as GeoVectorLayer;
                if (vlyr != null&& !vlyr.DataTable.FillData &&vlyr.LayerName!="混合层")
                {
                    
                    GIS.GeoData.DataProviders.IProvider datasource = null;
                    GeoData.GeoDataTable table = vlyr.DataTable;
                    
                    try
                    {
                        string extension = Path.GetExtension(vlyr.PathName).ToLower();
                        if(extension ==".shp")
                          datasource = new GIS.GeoData.DataProviders.ShapeFile(vlyr.PathName);
                        else if(extension == ".evc")
                          datasource = new GIS.GeoData.DataProviders.LQFile(vlyr.PathName);
                        datasource.Open();
                        datasource.ExecuteAllAttributeQuery(table);
                        datasource.Close();
                        table.FillData = true;

                        #region 修改
                        //num1 = table.Count;
                        //for (int i = 0; i < table.Count; i++)
                        //{
                        //    DraftTable.Rows[num + i]["ClasID"] = table[i]["ClasID"];
                        //    DraftTable.Rows[num + i]["BeginTime"] = table[i]["BeginTime"];
                        //    DraftTable.Rows[num + i]["FeatID"] = table[i]["FeatID"];
                        //    DraftTable.Rows[num + i]["UserID"] = table[i]["UserID"];
                        //    DraftTable.Rows[num + i]["ChangeType"] = table[i]["ChangeType"];
                        //}
                        #endregion
                       
                        //UIEventArgs.OutPutEventArgs e1 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs(vlyr.LayerName + "属性数据导入成功..................................\r\n");
                        //this.Invoke(evt, null, e1);
                    }
                    catch 
                    {
                        if (datasource != null)
                        {
                            datasource.Close();
                        } 
                    } 
                }
                //num = num + num1;
            }
            //UIEventArgs.OutPutEventArgs e2 = new GIS.TreeIndex.UIEventArgs.OutPutEventArgs("属性数据导入完成..................................\r\n");
            //this.Invoke(evt, null, e2);
        }

        //向地图里添加多个文件数据，strFileNames为文件路径。
        public void AddFiles()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "shape文件|*.shp|E00文件|*.e00|EVC文件|*.EVC|CAD交换文件|*.dxf|影像文件|*.img;*.IMG;*.tif;*.TIF;*.jpg;*.JPEG;*.jpeg;*.JPEG;*.dem;*.adf;*.DEM;*.gif;*.GIF;*.bmp;*.BMP;*.png;*.PNG;*.jp2;*.JP2;*.j2k;*.J2K;*.hdr";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    m_Map.AddFiles(dlg.FileNames);
                   
                    OutPutTextInfo("开始导入属性数据，请稍等哦.......................\r\n");

                    //Thread thread = new Thread(new ThreadStart(FillAttribute));
                    //thread.IsBackground = true;
                    //thread.Start();

                    FillAttribute();

                    ZoomToFullExtent();
                    EagleMapRefresh(true);

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        //zh 向地图里添加多个文件数据，strFileNames为文件路径。 20180706
        public void AddFiles(String[] fileNames)
        {
                try
                {
                    m_Map.AddFiles(fileNames);
                    FillAttribute();
                    EagleMapRefresh(true);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
        }

        public void CheckIncInfo()
        {
            m_IncManager.UpdateCheck();
            TreeIndex.Forms.CheckIncInfoForm form = new GIS.TreeIndex.Forms.CheckIncInfoForm(this);
            form.ShowDialog();
        }

        public void WriteChangeInfoFile()
        {
            if (m_IncManager.WriteChangeFile(m_FeatureCodes))
            {

                OutPutTextInfo(">>增量文件输出成功！\r\n");

            }
        }

        public void ReadChangeInfoFile()
        {
            ClearAllSlt();
            if (m_IncManager.ReadChangeFile())
            {
                OutPutTextInfo(">>增量文件读取成功！\r\n");
                Refresh();
            }
        }

        public void VisualInfo()
        {
            m_IncManager.Visualable();
        }

        public void SaveLayerAs(string strLayerName)
        {
            GeoLayer lyr = GetLayerByName(strLayerName);
            if (lyr.LayerType == LAYERTYPE.VectorLayer)
            {
                GeoVectorLayer vlyr = lyr as GeoVectorLayer;
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "EVC文件|*.EVC|SHP文件|*.shp";
                dlg.FileName = lyr.LayerName; 
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string dir = Path.GetDirectoryName(dlg.FileName);
                    string lyrname = Path.GetFileNameWithoutExtension(dlg.FileName) + Path.GetExtension(dlg.FileName);
                    vlyr.SaveLayerAs(dir,lyrname);
                    MessageBox.Show("文件保存成功！", "提示");
                }
            }
        }
        public void SaveLQLayer(string strLayerName)
        {
            GeoLayer lyr = GetLayerByName(strLayerName);
            if (lyr.LayerType == LAYERTYPE.VectorLayer)
            {
                GeoVectorLayer vlyr = lyr as GeoVectorLayer;
                if (vlyr.PathName != null)
                {
                    string dir = Path.GetDirectoryName(vlyr.PathName);
                    string lyrName = vlyr.LayerName + ".EVC";
                    vlyr.SaveLayerAsLQFile(dir,lyrName);
                    MessageBox.Show("文件保存成功！", "提示");
                }
                else
                {
                    SaveLayerAs(strLayerName);
                }
            }
        }

        public void SaveShapeLayer(string strLayerName)
        {
            GeoLayer lyr = GetLayerByName(strLayerName);
            if (lyr.LayerType == LAYERTYPE.VectorLayer)
            {
                GeoVectorLayer vlyr = lyr as GeoVectorLayer;
                if (vlyr.PathName != null)
                {
                    string dir = Path.GetDirectoryName(vlyr.PathName);
                    string lyrName = vlyr.LayerName + ".shp";
                    vlyr.SaveLayerAsShapeFile( dir,lyrName);
                    MessageBox.Show("文件保存成功！", "提示");
                }
                else
                {
                    SaveLayerAs(strLayerName);
                }            
            }
        }

        public void SaveLayersAsLQFiles()
        {
            bool valid = true;
            for (int j = 0; j < LayerCounts; j++)
            {
                GeoVectorLayer lyr = GetLayerAt(j) as GeoVectorLayer;
                if (lyr != null)
                {
                    if (lyr.PathName == null)
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (!valid)
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string folder = dlg.SelectedPath;
                    for (int j = 0; j < LayerCounts; j++)
                    {
                        GeoVectorLayer lyr = GetLayerAt(j) as GeoVectorLayer;
                        lyr.PathName = folder + "\\" + lyr.LayerName + ".EVC";
                    }
                }
                else return;
            }
            for (int j = 0; j < LayerCounts; j++)
            {
                GeoVectorLayer lyr = GetLayerAt(j) as GeoVectorLayer;
                if (lyr != null)
                {
                    string dir = Path.GetDirectoryName(lyr.PathName);
                    string lyrName = lyr.LayerName + ".EVC";
                    lyr.SaveLayerAsLQFile(dir,lyrName);
                }
            }

            MessageBox.Show("文件已保存！", "提示"); 
        }
        
        public void SaveLayersAsLQFilesAs()
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string folderName = dlg.SelectedPath;

                for (int j = 0; j < LayerCounts; j++)
                {
                    GeoVectorLayer lyr = GetLayerAt(j) as GeoVectorLayer;
                    if (lyr != null)
                    {
                        string lyrname = lyr.LayerName+".EVC";
                        lyr.SaveLayerAsLQFile(folderName,lyrname);
                    }
                }

                MessageBox.Show("文件已保存！", "提示");
            }
         
        }
        public bool SaveLayersAsShapeFilesAs()
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
 
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                
                string folderName =  dlg.SelectedPath;
            

                   for (int j = 0; j <  LayerCounts ; j++)
                   {
                       GeoVectorLayer lyr =GetLayerAt(j) as GeoVectorLayer;
                       if (lyr != null)
                       {
                            string lyrname = lyr.LayerName+".shp";
                           lyr.SaveLayerAsShapeFile(folderName,lyrname);
                       }
                   }
               
               MessageBox.Show("文件已保存！", "提示");
            } 
            return true;
        }
        public bool SaveLayersAsShapeFiles()
        {
            bool valid = true;
            for (int j = 0; j < LayerCounts; j++)
            {
                GeoVectorLayer lyr = GetLayerAt(j) as GeoVectorLayer;
                if (lyr != null)
                {
                    if (lyr.PathName == null)
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (!valid)
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string folder = dlg.SelectedPath;
                    for (int j = 0; j < LayerCounts; j++)
                    {
                        GeoVectorLayer lyr = GetLayerAt(j) as GeoVectorLayer;
                        lyr.PathName = folder + "\\" + lyr.LayerName + ".shp";
                    }
                }
            }
            for (int j = 0; j < LayerCounts; j++)
            {
                GeoVectorLayer lyr = GetLayerAt(j) as GeoVectorLayer;
                if (lyr != null)
                {
                    string foldername = Path.GetDirectoryName(lyr.PathName);
                    string lyrname = lyr.LayerName + ".shp";
                    lyr.SaveLayerAsShapeFile(foldername,lyrname);
                }

            } 
            MessageBox.Show("文件已保存！", "提示");
            return true;
        }

        #region 观测值导入导出

        public void ImportSurveyPts()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "文本文件|*.txt";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
                StreamReader sw = new StreamReader(fs);
                GeoVectorLayer lyr = GetLayerByName("测点层") as GeoVectorLayer;
                try
                {
                    string data = sw.ReadLine();
                    while (data != null)
                    {
                        string[] datas = data.Split(',');
                        double x = double.Parse(datas[1].Trim());
                        double y = double.Parse(datas[2].Trim());
                        double z = double.Parse(datas[3].Trim());

                        GeoPoint3D pt = new GeoPoint3D(x, y,z);
                        lyr.AddGeometry(pt);
                        data = sw.ReadLine();
                    } 
                    ZoomToFullExtent();
                    OutPutTextInfo("提示：观测值导入成功。。。。。。。\r\n");
                }
                catch
                {
                    MessageBox.Show("文件格式不对");
                }
                finally
                {
                    sw.Close();
                    fs.Close();
                }
            
            }
        }
        public void ExportSurveyPts()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "文本文件|*.txt";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                GeoVectorLayer lyr = GetLayerByName("测点层") as GeoVectorLayer;
                for (int i = 0; i < lyr.DataTable.Count; i++)
                {
                     GeoPoint3D pt3D = lyr.DataTable[i].Geometry as GeoPoint3D;
                     if (pt3D != null)
                     {
                         sw.WriteLine(string.Format("{0},{1},{2},{3},{4}", i + 1, pt3D.X, pt3D.Y, pt3D.Z, 5));
                     }
                     else
                     {
                         GeoPoint pt = lyr.DataTable[i].Geometry as GeoPoint;
                         sw.WriteLine(string.Format("{0},{1},{2},{3},{4}", i + 1, pt.X, pt.Y, 999999.999, 5));
                     }
                }
                sw.Close();
                fs.Close();
                OutPutTextInfo("提示：观测值导出成功。。。。。。。\r\n");
              
            }
       
        } 
        #endregion
    }
}
