using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using System.IO;
using System.Diagnostics;
using GIS.UI.AdditionalTool;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using GIS.UI.Entity;

using Oracle.ManagedDataAccess.Client;

//zh编写
//shp数据入库界面设计
namespace GIS.UI.Forms
{
    public partial class SHPToDataBase : Form
    {
        public SHPToDataBase()
        {
            InitializeComponent();
            OracleDBHelper db = new OracleDBHelper();
            this.dataBasePara = db.conString;
        }

        public SHPToDataBase( string datapara)
        {
            InitializeComponent();
            this.dataBasePara = datapara;
        }

        public SHPToDataBase(AxMapControl axMapControl1)
        {
            InitializeComponent();
            this.axMapControl1 = axMapControl1;
        }
        //取消
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #region 定义变量
        string pFilePath = null;
        string pFileName = null;
        string[] pFullPath = null;
        string dataBasePara = null;
        AxMapControl axMapControl1;
        List<Poly> polyList = null;//线面属性集合
        List<Ppoint> pointList = null;//点属性集合
        string tablename = "";//shp数据入库的表名
        #endregion

        //浏览
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog pOpenFileDialog = new OpenFileDialog();
            pOpenFileDialog.CheckFileExists = true;
            pOpenFileDialog.Title = "打开Shp数据";
            pOpenFileDialog.Filter = "Shape文件（*.shp）|*.shp";
            pOpenFileDialog.Multiselect = false;

            if (pOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
               pFullPath = pOpenFileDialog.FileNames;
                string files = "";
                for (int i = 0; i < pFullPath.Length; i++)
                {
                    if (pFullPath[i] == "") return;
                    int pIndex = pFullPath[i].LastIndexOf("\\");
                    pFileName = pFullPath[i].Substring(pIndex + 1); //文件名
                    pIndex = pFileName.LastIndexOf(".");
                    pFileName = pFileName.Substring(0, pIndex); //文件名

                    pIndex = pFullPath[i].LastIndexOf(".");
                    pFilePath = pFullPath[i].Substring(0, pIndex); //文件路径

                    files += pFullPath[i];
                    textBox1.Text = files;
                }
            }
            
        }
        //导入
        private void button2_Click(object sender, EventArgs e)
        {
            //复制shp2sdo插件
            //string sourcePath = @"shp2sdo.exe";
            //string targetPath = pFilePath + "\\shp2sdo.exe";
            //bool isrewrite = true; // true=覆盖已存在的同名文件,false则反之
            //File.Copy(sourcePath, targetPath, isrewrite);

            #region 直接调用cmd命令 zh 2018.6.21 废弃
//            try
//            {
//                //写入sql
//                string content = String.Format(@"create index idx_{0} on {1}(shape) indextype is mdsys.spatial_index;
//EXECUTE SDO_MIGRATE.TO_CURRENT('{2}','shape');
//commit;"
//     , tableName.Text, tableName.Text
//     , tableName.Text);

//                writeSqlFile(content);
//                //调用cmd命令
//                Process p = new Process();//创建进程对象 
//                p.StartInfo.FileName = "cmd.exe";//设定需要执行的命令   
//                p.StartInfo.UseShellExecute = false;//不使用系统外壳程序启动 
//                p.StartInfo.RedirectStandardInput = true;//可以重定向输入  
//                p.StartInfo.RedirectStandardOutput = true;
//                p.StartInfo.RedirectStandardError = true;
//                p.StartInfo.CreateNoWindow = true;//不创建窗口 

//                p.Start();
//                string cmd1 = String.Format("shp2sdo {0} {1} -i OBJECTID -g shape", pFilePath, tableName.Text);
//                p.StandardInput.WriteLine(cmd1);
//                string cmd2 = String.Format("sqlplus {0} @{1}.sql &exit", dataBasePara, tableName.Text);
//                p.StandardInput.WriteLine(cmd2);
//                //string cmd3 = String.Format("@{0}.sql", tableName.Text);
//                //p.StandardInput.WriteLine(cmd3);
//                p.StandardInput.WriteLine("exit");

//                //输出
//                string outStr = p.StandardOutput.ReadToEnd();
//                tbShow.Text = outStr;

//                p.Start();
//                string cmd4 = String.Format("sqlldr {0} {1}.ctl", dataBasePara, tableName.Text);
//                p.StandardInput.WriteLine(cmd4);
//                string cmd5 = String.Format("sqlplus {0} @shp.sql &exit", dataBasePara);
//                p.StandardInput.WriteLine(cmd5);
//                //string cmd6 = String.Format("create index idx_{0} on {1}(shape) indextype is mdsys.spatial_index;", tableName.Text, tableName.Text);
//                //p.StandardInput.WriteLine(cmd6);
//                //string cmd7 = String.Format("EXECUTE SDO_MIGRATE.TO_CURRENT('{0}','shape');", tableName.Text);
//                //p.StandardInput.WriteLine(cmd7);
//                p.StandardInput.WriteLine("exit");

//                //输出
//                outStr += p.StandardOutput.ReadToEnd();
//                tbShow.Text = outStr;
//                //等待程序执行完退出进程
//                p.WaitForExit();
//                p.Close();
//            }
//            catch (Exception exp)
//            {
//                tbShow.Text = exp.ToString();
//            }
         #endregion 
            

            #region 生成bat执行 zh2018.6.21 废弃
//            try
//            {
//                //写入bat
//                string content = String.Format(@"shp2sdo {0} {1} -i OBJECTID -g shape 
//sqlplus {2}
//@{3}.sql
//exit
//sqlldr {4} {5}.ctl
//sqlplus {6}
//create index idx_{7} on {8}(shape) indextype is mdsys.spatial_index;
//EXECUTE SDO_MIGRATE.TO_CURRENT('{9}','shape');
//exit"
//     , pFilePath, tableName.Text
//     ,dataBasePara
//     , tableName.Text
//     , dataBasePara, tableName.Text
//     ,dataBasePara
//     , tableName.Text, tableName.Text
//     , tableName.Text);

//                writeBATFile(content);
//                Process p = new Process();
//                FileInfo file = new FileInfo("osm.bat");
//                p.StartInfo.WorkingDirectory = file.Directory.FullName;
//                p.StartInfo.FileName = "osm.bat";
//                p.StartInfo.CreateNoWindow = false;//不创建窗口 
//                p.StartInfo.RedirectStandardOutput = true;
//                p.StartInfo.RedirectStandardError = true;
//                p.StartInfo.RedirectStandardInput = true;//可以重定向输入 
//                p.Start();

//                //输出
//                string outStr = p.StandardOutput.ReadToEnd();
//                tbShow.Text = outStr;
//                p.WaitForExit();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
//            }
            #endregion

            #region 读取属性表导入数据 zh 0929
            //获取单选按钮的值
            int dataType = 0;//0代表基态，1代表增量
            int topoType = 0;//0代表点，1代表线，2代表面
            if (this.rb_jitai.Checked == false) dataType = 1;
            if (this.rb_line.Checked == true) topoType = 1;
            if (this.rb_poly.Checked == true) topoType = 2;
            //
            //判断表名是否为空
            tablename = this.tb_name.Text.ToString();
            if (tablename.Equals(""))
            {
                MessageBox.Show("请输入要入库的表名");
                return;
            }
            //判断数据库中是否含有该表名，若没有则新建
            OracleDBHelper helper = new OracleDBHelper();
            if (!helper.IsExistTable(tablename))//判断当前数据库中是否存在该表
            {
                if (dataType == 0)
                {
                    if (topoType == 0) helper.createPointTable(tablename);
                    else helper.createPolyTable(tablename);
                }
                else
                {
                    if (topoType == 0) helper.createZPointTable(tablename);
                    else helper.createZPolyTable(tablename);
                }
            }
            //将shp数据显示到地图控件上，并获取其属性信息
            ShpHelper shphelper = new ShpHelper(axMapControl1);
            shphelper.addShpToAxmap(pFullPath[0]);
            int index = axMapControl1.LayerCount;
            DataTable dt = shphelper.getShpAttributes(index-1);
            
            //将属性datatable转成List
            if (topoType == 0) pointList = shphelper.datatableToPointList(dt,dataType);
            else polyList = shphelper.datatableToPolyList(dt,dataType);
            insertShpToOracle(dataType,topoType);
            #endregion
        }

        //将DataTable数据导入oracle
        public void insertShpToOracle(int dataType,int topoTpye)
        {
            if (dataType == 0)//基态
            {
                switch (topoTpye)
                {
                    case 0://目标为点
                        new System.Threading.Thread(new System.Threading.ThreadStart(processJPoint)).Start();
                        break;
                    case 1://目标为线
                        new System.Threading.Thread(new System.Threading.ThreadStart(processJLine)).Start();
                        break;
                    case 2://目标为面
                        new System.Threading.Thread(new System.Threading.ThreadStart(processJArea)).Start();
                        break;
                }
            }
            else
            {
                switch (topoTpye)
                {
                    case 0://目标为增量点
                        new System.Threading.Thread(new System.Threading.ThreadStart(processZPoint)).Start();
                        break;
                    case 1://目标为增量线
                        new System.Threading.Thread(new System.Threading.ThreadStart(processZLine)).Start();
                        break;
                    case 2://目标为增量面
                        new System.Threading.Thread(new System.Threading.ThreadStart(processZArea)).Start();
                        break;
                }
            }
        }

        #region 废弃
        public void writeBATFile(string fileContent)
        {
            string filePath = "osm.bat";
            System.IO.FileStream fs1 = new System.IO.FileStream(filePath, FileMode.Create, FileAccess.Write);//创建写入文件
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(fileContent);//开始写入值
            sw.Close();
            fs1.Close();
        }

        public void writeSqlFile(string fileContent)
        {
            string filePath = "shp.sql";
            System.IO.FileStream fs1 = new System.IO.FileStream(filePath, FileMode.Create, FileAccess.Write);//创建写入文件
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(fileContent);//开始写入值
            sw.Close();
            fs1.Close();
        }

        IFeatureWorkspace featureWorkspace;
        IFeatureDataset featureDataset;
       


        public void FeatureClassToFeatureClass(IDataset pDataSet, string strFeatFileDir, string strFeatFileName, string strOutName, bool isWorkspace)
        {
            try
            {
                IWorkspaceFactory pWSF = new ShapefileWorkspaceFactoryClass();
                IFeatureWorkspace pFeatureWK = (IFeatureWorkspace)pWSF.OpenFromFile(strFeatFileDir, 0);
                IFeatureClass pInFeatureClass = pFeatureWK.OpenFeatureClass(strFeatFileName);
                if (pInFeatureClass == null || pDataSet == null)
                {
                    MessageBox.Show("创建失败");
                    return;
                }
                IFeatureClassName pInFeatureclassName;
                IDataset pIndataset = (IDataset)pInFeatureClass;
                pInFeatureclassName = (IFeatureClassName)pIndataset.FullName;
                //如果名称已存在
                IWorkspace2 pWS2 = null;
                if (isWorkspace)
                    pWS2 = pDataSet as IWorkspace2;
                else
                    pWS2 = pDataSet.Workspace as IWorkspace2;
                if (pWS2.get_NameExists(esriDatasetType.esriDTFeatureClass, strOutName))
                {
                    DialogResult result;
                    result = System.Windows.Forms.MessageBox.Show(null, "矢量文件名  " + strOutName + "  在数据库中已存在!" + "/r是否覆盖?", "相同文件名", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    //覆盖原矢量要素
                    if (result == DialogResult.Yes)
                    {
                        IFeatureWorkspace pFWS = (IFeatureWorkspace)pWS2;
                        IDataset pDataset = pFWS.OpenFeatureClass(strOutName) as IDataset;
                        pDataset.Delete();
                        pDataset = null;
                    }
                }
                IFields pInFields, pOutFields;
                IFieldChecker pFieldChecker = new FieldCheckerClass();
                IEnumFieldError pError;
                pInFields = pInFeatureClass.Fields;
                pFieldChecker.Validate(pInFields, out pError, out pOutFields);
                IField geoField = null;
                for (int i = 0; i < pOutFields.FieldCount; i++)
                {
                    IField pField = pOutFields.get_Field(i);
                    if (pField.Type == esriFieldType.esriFieldTypeOID)
                    {
                        IFieldEdit pFieldEdit = (IFieldEdit)pField;
                        pFieldEdit.Name_2 = pField.AliasName;
                    }
                    if (pField.Type == esriFieldType.esriFieldTypeGeometry)
                    {
                        geoField = pField;
                        break;
                    }
                }
                IGeometryDef geometryDef = geoField.GeometryDef;
                IFeatureDataConverter one2another = new FeatureDataConverterClass();
                IFeatureClassName pOutFeatureClassName = new FeatureClassNameClass();
                IDatasetName pOutDatasetName = (IDatasetName)pOutFeatureClassName;
                if (isWorkspace)
                    pOutDatasetName.WorkspaceName = (IWorkspaceName)pDataSet.FullName;
                else
                    pOutDatasetName.WorkspaceName = (IWorkspaceName)((IDataset)pDataSet.Workspace).FullName;
                pOutDatasetName.Name = strOutName;
                if (isWorkspace)
                {
                    one2another.ConvertFeatureClass(pInFeatureclassName, null, null, pOutFeatureClassName, geometryDef, pOutFields, "", 1000, 0);
                }
                else
                {
                    IFeatureDataset pFeatDS = (IFeatureDataset)pDataSet;
                    IFeatureDatasetName pOutFeatDSName = pFeatDS.FullName as IFeatureDatasetName;
                    one2another.ConvertFeatureClass(pInFeatureclassName, null, pOutFeatDSName, pOutFeatureClassName, geometryDef, pOutFields, "", 1000, 0);
                    pOutFeatDSName = null;
                    pFeatDS = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        #endregion


        #region 进度条和提示lable的设置和更新委托
        public delegate void SetState(int maxValue, string info);
        public void SetMyState(int maxValue, string info)
        {
            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = maxValue;
            this.tbShow.Text = info;
        }
        public delegate void UpdateState(int value, string info);
        public void UpdateMyState(int value, string info)
        {
            this.progressBar1.Value = value;
            this.tbShow.Text = this.tbShow.Text + info;
        }
        public void UpdateMyStateInitInfo(int value, string info)
        {
            this.progressBar1.Value = value;
            this.tbShow.Text = info;
        }
        #endregion


        #region 入库线程
        //线程，基态shp点入库
        public void processJPoint()
        {
            string info = "";
            info = string.Format("正在计算所需时间...  \r\n\r\n");
            this.Invoke(new SetState(SetMyState), 2, info);
            info = string.Format("正在入库shp数据...\r\n\r\n    进度：{0}/{1}   \r\n\r\n", 0, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 1, info);

            OracleDBHelper odh = new OracleDBHelper();
            odh.insertPointDataByList(tablename, pointList);

            info = string.Format("shp数据入库完成\r\n\r\n    进度：{0}/{1}  ", 1, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 2, info);
        }

        //线程，基态shp线入库
        public void processJLine()
        {
            string info = "";
            info = string.Format("正在计算所需时间...  \r\n\r\n");
            this.Invoke(new SetState(SetMyState), 2, info);
            info = string.Format("正在入库shp数据...\r\n\r\n    进度：{0}/{1}   \r\n\r\n", 0, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 1, info);
            
            OracleDBHelper odh = new OracleDBHelper();
            odh.insertPolyDataByList(tablename, polyList);

            info = string.Format("shp数据入库完成\r\n\r\n    进度：{0}/{1}  ", 1, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 2, info);
        }

        //线程，基态shp面入库
        public void processJArea()
        {
            string info = "";
            info = string.Format("正在计算所需时间...  \r\n\r\n");
            this.Invoke(new SetState(SetMyState), 2, info);
            info = string.Format("正在入库shp数据...\r\n\r\n    进度：{0}/{1}   \r\n\r\n", 0, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 1, info);

            OracleDBHelper odh = new OracleDBHelper();
            odh.insertPolyDataByList(tablename, polyList);

            info = string.Format("shp数据入库完成\r\n\r\n    进度：{0}/{1}  ", 1, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 2, info);
        }

        //线程，增量shp点入库
        public void processZPoint()
        {
            string info = "";
            info = string.Format("正在计算所需时间...  \r\n\r\n");
            this.Invoke(new SetState(SetMyState), 2, info);
            info = string.Format("正在入库shp数据...\r\n\r\n    进度：{0}/{1}   \r\n\r\n", 0, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 1, info);

            OracleDBHelper odh = new OracleDBHelper();
            odh.insertZPointDataByList(tablename, pointList);

            info = string.Format("shp数据入库完成\r\n\r\n    进度：{0}/{1}  ", 1, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 2, info);
        }

        //线程，增量shp线入库
        public void processZLine()
        {
            string info = "";
            info = string.Format("正在计算所需时间...  \r\n\r\n");
            this.Invoke(new SetState(SetMyState), 2, info);
            info = string.Format("正在入库shp数据...\r\n\r\n    进度：{0}/{1}   \r\n\r\n", 0, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 1, info);

            OracleDBHelper odh = new OracleDBHelper();
            odh.insertZPolyDataByList(tablename, polyList);

            info = string.Format("shp数据入库完成\r\n\r\n    进度：{0}/{1}  ", 1, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 2, info);
        }

        //线程，增量shp面入库
        public void processZArea()
        {
            string info = "";
            info = string.Format("正在计算所需时间...  \r\n\r\n");
            this.Invoke(new SetState(SetMyState), 2, info);
            info = string.Format("正在入库shp数据...\r\n\r\n    进度：{0}/{1}   \r\n\r\n", 0, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 1, info);

            OracleDBHelper odh = new OracleDBHelper();
            odh.insertZPolyDataByList(tablename, polyList);

            info = string.Format("shp数据入库完成\r\n\r\n    进度：{0}/{1}  ", 1, 1);
            this.Invoke(new UpdateState(UpdateMyStateInitInfo), 2, info);
        }
        #endregion

    }
}
