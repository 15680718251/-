using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using GISEditor.EditorTool.BasicClass;
using GISEditor.EditorTool.Tool;
using GISEditor.EditTool.Tool;
using GISEditor.EditTool.Command;
using GISEditor.EditorTool.Command;
using ESRI.ArcGIS.DataSourcesFile;
using System.IO;
using GIS.UI;
using GIS.UI.Forms;
using Mono.Security;
using GIS.TreeIndex;
using QualityControl;
using FeatureMatchUpdate.Forms;
using QualityControl.Forms;
using TrustValueAndReputation.historyToDatabase;
using TrustValueAndReputation;
using GIS.UI.AdditionalTool;
using stdole;

namespace 全球典型要素数据库增量更新系统
{
    public partial class MainForm : Form
    {
        #region 变量定义
        private ISpatialReference spatialReference;
        private OperateMap m_OperateMap = null;
        private IEngineEditLayers m_EngineEditLayers = null;
        //地图导出窗体
        string pMouseOperate;                             //鼠标操作事件
        private FormExportMap frmExpMap = null;           //地图导出窗体
        private string sMapUnits = "未知单位";             //地图单位变量
        private object missing = Type.Missing;

        //TOC菜单
        private ESRI.ArcGIS.Geometry.Point pMoveLayerPoint = new ESRI.ArcGIS.Geometry.Point();  //鼠标在TOC中左键按下时点的位置
        public IFeatureLayer pTocFeatureLayer = null;            //鼠标点击的要素图层
        private FormAtrribute frmAttribute = null;        //图层属性窗体
        private TopoErrorAtrribute topoErrorAttribute = null;        //拓扑冲突属性窗体
        private ILayer pMoveLayer;                        //需要调整显示顺序的图层
        private int toIndex;                              //存放拖动图层移动到的索引号  
        IFeature pFeature1;                                //鼠标点击的要素对象
        private Identifier IdentifierForm = null;         //Identifier窗体    
        //鹰眼同步
        private bool bCanDrag;              //鹰眼地图上的矩形框可移动的标志
        private IPoint pMoveRectPoint;      //记录在移动鹰眼地图上的矩形框时鼠标的位置
        private IEnvelope pEnv;              //记录数据视图的extent

        private IScreenDisplay iScreenDisplay;
        private IGeometry iGeometry;

        private FormMeasureResult frmMeasureResult = null;   //量算结果窗体
        private INewLineFeedback pNewLineFeedback;           //追踪线对象
        private INewPolygonFeedback pNewPolygonFeedback;     //追踪面对象
        private IPointCollection pAreaPointCol = new MultipointClass();  //面积量算时画的点进行存储；  
        private IPoint pPointPt = null;                      //鼠标点击点
        private IPoint pMovePt = null;                       //鼠标移动时的当前点
        private double dSegmentLength = 0;                   //片段距离
        private double dToltalLength = 0;                    //量测总长度

        //chp06
        private string sMxdPath = Application.StartupPath;
        private IMap pMap = null;

        private IActiveView pActiveView = null;//视图
        private List<ILayer> plstLayers = null;//当前窗体加载的图层集合
        //private IFeatureLayer pCurrentLyr = null;//当前编辑图层
        private IEngineEditor pEngineEditor = null;//功能是启动或者停止一个编辑流程 在应用engioneditor前 首先要实例化 IEngineEditor pEngineEditor = new EngionEditorClass();
        private IEngineEditTask pEngineEditTask = null;//编辑任务
        private IEngineEditLayers pEngineEditLayers = null;//编辑图层
        //chp05
        private IStyleGalleryItem pStyleGalleryItem;
        //add by zyh
        //TOCControl中Map菜单
        private IToolbarMenu m_MenuMap = null;
        //TOCControl中图层菜单
        private IToolbarMenu m_MenuLayer = null;

        private AnnotationDisplay AnnotationDisplay = null;  // 注记
        private AddText Addtext = null;                      //增加注记

        MoveFeatureToolClass m_moveTool = new MoveFeatureToolClass();
        CreateFeatureToolClass m_CreateFeatTool = new CreateFeatureToolClass();
        SelectFeatureToolClass m_SelectFeatTool = new SelectFeatureToolClass();
        CopyFeatureToolClass CopyFeature = new CopyFeatureToolClass();
        DelFeatureCommandClass m_delFeatCom = new DelFeatureCommandClass();
        MoveVertexToolClass m_MoveVertexTool = new MoveVertexToolClass();
        
        //ICommand m_moveTool = new MoveFeatureToolClass();
    //    struct AFX_OLDNONCLIENTMETRICS
    //    {
    //        uint cbSize;
    //        int iBorderWidth;
    //        int iScrollWidth;
    //        int iScrollHeight;
    //        int iCaptionWidth;
    //        int iCaptionHeight;
    //        DevExpress.Utils.Drawing.Helpers.NativeMethods.LOGFONT lfCaptionFont;
    //        int iSmCaptionWidth;
    //        int iSmCaptionHeight;
    //        DevExpress.Utils.Drawing.Helpers.NativeMethods.LOGFONT lfSmCaptionFont;
    //        int iMenuWidth;
    //        int iMenuHeight;
    //        DevExpress.Utils.Drawing.Helpers.NativeMethods.LOGFONT lfMenuFont;
    //        DevExpress.Utils.Drawing.Helpers.NativeMethods.LOGFONT lfStatusFont;
    //        DevExpress.Utils.Drawing.Helpers.NativeMethods.LOGFONT lfMessageFont;
    //    };
    //    const UINT cbProperSize = sizeof(AFX_OLDNONCLIENTMETRICS);/*(_AfxGetComCtlVersion() < MAKELONG(1, 6)) 
    //? sizeof(AFX_OLDNONCLIENTMETRICS) : sizeof(NONCLIENTMETRICS);*/  

        #endregion

        #region 初始化

        private void Form1_Load(object sender, EventArgs e)
        {
            axMapControl1.Map.Name = "图层";
            axTOCControl1.SetBuddyControl(axMapControl1);
            axTOCControl1.LabelEdit = esriTOCControlEdit.esriTOCControlManual;
            axMapControl1.AutoMouseWheel = true;
        }

        public MainForm()
        {
            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);
            InitializeComponent();
            EagleEyeMapControl.Extent = axMapControl1.FullExtent;
            pEnv = EagleEyeMapControl.Extent;
            DrawRectangle(pEnv);
            //InitObject();

            pEngineEditor = new EngineEditor();
            pEngineEditLayers = pEngineEditor as IEngineEditLayers;
            pMap = axMapControl1.Map;
            //窗体设置

            //窗体大小不可变
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            //固定splitContainer1的哪个Panel
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            //设置splitContainer1不可拖动
            splitContainer1.IsSplitterFixed = true;
            m_OperateMap = new OperateMap();


        }
        #endregion

        #region 封装的方法

        #region 加载工作空间里面的要素和栅格数据
        private void AddAllDataset(IWorkspace pWorkspace, AxMapControl mapControl)
        {
            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            //将Enum数据集中的数据一个个读到DataSet中
            IDataset pDataset = pEnumDataset.Next();
            //判断数据集是否有数据
            while (pDataset != null)
            {
                if (pDataset is IFeatureDataset)  //要素数据集
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;
                    IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(pDataset.Name);
                    IEnumDataset pEnumDataset1 = pFeatureDataset.Subsets;
                    pEnumDataset1.Reset();
                    IGroupLayer pGroupLayer = new GroupLayerClass();
                    pGroupLayer.Name = pFeatureDataset.Name;
                    IDataset pDataset1 = pEnumDataset1.Next();
                    while (pDataset1 != null)
                    {
                        if (pDataset1 is IFeatureClass)  //要素类
                        {
                            IFeatureLayer pFeatureLayer = new FeatureLayerClass();
                            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(pDataset1.Name);
                            if (pFeatureLayer.FeatureClass != null)
                            {
                                pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;
                                pGroupLayer.Add(pFeatureLayer);
                                mapControl.Map.AddLayer(pFeatureLayer);
                            }
                        }
                        pDataset1 = pEnumDataset1.Next();
                    }
                }
                else if (pDataset is IFeatureClass) //要素类
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;
                    IFeatureLayer pFeatureLayer = new FeatureLayerClass();
                    pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(pDataset.Name);

                    pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;
                    mapControl.Map.AddLayer(pFeatureLayer);
                }
                else if (pDataset is IRasterDataset) //栅格数据集
                {
                    IRasterWorkspaceEx pRasterWorkspace = (IRasterWorkspaceEx)pWorkspace;
                    IRasterDataset pRasterDataset = pRasterWorkspace.OpenRasterDataset(pDataset.Name);
                    //影像金字塔判断与创建
                    IRasterPyramid3 pRasPyrmid;
                    pRasPyrmid = pRasterDataset as IRasterPyramid3;
                    if (pRasPyrmid != null)
                    {
                        if (!(pRasPyrmid.Present))
                        {
                            pRasPyrmid.Create(); //创建金字塔
                        }
                    }
                    IRasterLayer pRasterLayer = new RasterLayerClass();
                    pRasterLayer.CreateFromDataset(pRasterDataset);
                    ILayer pLayer = pRasterLayer as ILayer;
                    mapControl.AddLayer(pLayer, 0);
                }
                pDataset = pEnumDataset.Next();
            }

            mapControl.ActiveView.Refresh();
            //同步鹰眼
            SynchronizeEagleEye();
        }
        #endregion

        #region 清楚数据
        private void ClearAllData()
        {
            if (axMapControl1.Map != null && axMapControl1.Map.LayerCount > 0)
            {
                //新建axMapControl1中Map
                IMap dataMap = new MapClass();
                dataMap.Name = "Map";
                axMapControl1.DocumentFilename = string.Empty;
                axMapControl1.Map = dataMap;

                //新建EagleEyeMapControl中Map
                IMap eagleEyeMap = new MapClass();
                eagleEyeMap.Name = "eagleEyeMap";
                EagleEyeMapControl.DocumentFilename = string.Empty;
                EagleEyeMapControl.Map = eagleEyeMap;
            }
        }
        #endregion

        #region  获取RGB颜色
        private IRgbColor GetRgbColor(int intR, int intG, int intB)
        {
            IRgbColor pRgbColor = null;
            if (intR < 0 || intR > 255 || intG < 0 || intG > 255 || intB < 0 || intB > 255)
            {
                return pRgbColor;
            }
            pRgbColor = new RgbColorClass();
            pRgbColor.Red = intR;
            pRgbColor.Green = intG;
            pRgbColor.Blue = intB;
            return pRgbColor;
        }
        #endregion

        #region 获取地图单位
        private string GetMapUnit(esriUnits _esriMapUnit)
        {
            string sMapUnits = string.Empty;
            switch (_esriMapUnit)
            {
                case esriUnits.esriCentimeters:
                    sMapUnits = "厘米";
                    break;
                case esriUnits.esriDecimalDegrees:
                    sMapUnits = "十进制";
                    break;
                case esriUnits.esriDecimeters:
                    sMapUnits = "分米";
                    break;
                case esriUnits.esriFeet:
                    sMapUnits = "尺";
                    break;
                case esriUnits.esriInches:
                    sMapUnits = "英寸";
                    break;
                case esriUnits.esriKilometers:
                    sMapUnits = "千米";
                    break;
                case esriUnits.esriMeters:
                    sMapUnits = "米";
                    break;
                case esriUnits.esriMiles:
                    sMapUnits = "英里";
                    break;
                case esriUnits.esriMillimeters:
                    sMapUnits = "毫米";
                    break;
                case esriUnits.esriNauticalMiles:
                    sMapUnits = "海里";
                    break;
                case esriUnits.esriPoints:
                    sMapUnits = "点";
                    break;
                case esriUnits.esriUnitsLast:
                    sMapUnits = "UnitsLast";
                    break;
                case esriUnits.esriUnknownUnits:
                    sMapUnits = "未知单位";
                    break;
                case esriUnits.esriYards:
                    sMapUnits = "码";
                    break;
                default:
                    break;
            }
            return sMapUnits;
        }
        #endregion

        #region 绘制多边形
        public IPolygon DrawPolygon(AxMapControl mapCtrl)
        {
            IGeometry pGeometry = null;
            if (mapCtrl == null) return null;
            IRubberBand rb = new RubberPolygonClass();
            pGeometry = rb.TrackNew(mapCtrl.ActiveView.ScreenDisplay, null);
            return pGeometry as IPolygon;
        }
        #endregion



        #endregion

        #region 文件
        #region 打开文件
      
        #region 加载Shape文件
        private void 打开Shapefile数据_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                OpenFileDialog pOpenFileDialog = new OpenFileDialog();
                pOpenFileDialog.CheckFileExists = true;
                pOpenFileDialog.Title = "打开Shape文件";
                pOpenFileDialog.Filter = "Shape文件（*.shp）|*.shp";
                pOpenFileDialog.Multiselect = true;
                pOpenFileDialog.ShowDialog();

                string[] pFullPaths = pOpenFileDialog.FileNames;
                if (pFullPaths == null) return;

                IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
                IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(pFullPaths[0].Substring(0, pFullPaths[0].LastIndexOf("\\")), 0);

                for (int i = 0; i < pFullPaths.Length; i++)
                {
                    int pIndex = pFullPaths[i].LastIndexOf("\\");
                    string pFileName = pFullPaths[i].Substring(pIndex + 1); //文件名
                    IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(pFileName);
                    IFeatureLayer pFeatureLayer = new FeatureLayer();

                    pFeatureLayer.FeatureClass = pFeatureClass;
                    pFeatureLayer.Name = pFeatureClass.AliasName;

                    //设置投影坐标
                    pFeatureLayer.SpatialReference = setSpatialReference(4508);

                    axMapControl1.Map.AddLayer(pFeatureLayer);
                }

                #region GDAL加载数据，用于构建索引 20180706
                //zh 20180706
                MainMap = new MapUI();
                MainMap.AddFiles(pFullPaths);
                #endregion


                pActiveView = pMap as IActiveView;
                pEngineEditor = new EngineEditorClass();
                MapManager.EngineEditor = pEngineEditor;
                pEngineEditTask = pEngineEditor as IEngineEditTask;
                pEngineEditLayers = pEngineEditor as IEngineEditLayers;
                //pEngineEditLayers.SetTargetLayer(pCurrentLyr, 0);


                plstLayers = GetLayers(pMap);


                axMapControl1.ActiveView.Refresh();
                //同步鹰眼
                SynchronizeEagleEye();
            }
            catch (Exception ex)
            {
                MessageBox.Show("图层加载失败！" + ex.Message);
            }

            //try
            //{
            //    OpenFileDialog pOpenFileDialog = new OpenFileDialog();//实例化打开对话框
            //    pOpenFileDialog.CheckFileExists = true;               //检查文件是否存在
            //    pOpenFileDialog.Multiselect = true;                   //允许多选
            //    pOpenFileDialog.Title = "加载地图数据";               //打开对话框的名字
            //    pOpenFileDialog.Filter = "Shp文件(*.shp)|*.shp";      //文件过滤器，打开文件的类型
            //    pOpenFileDialog.ShowDialog();                         //显示对话框
            //    string[] pFullPaths = pOpenFileDialog.FileNames;      //数组存储路径（包含文件名）
            //    if (pFullPaths == null) return;                       //未选择return

            //    //实例化工作空间
            //    IWorkspaceFactory pWsF = new ShapefileWorkspaceFactory();
            //    IFeatureWorkspace pFW = (IFeatureWorkspace)pWsF.OpenFromFile(pFullPaths[0].Substring(0, pFullPaths[0].LastIndexOf("\\")), 0);
            //    //pFullPaths[0].Substring(0,pFullPaths[0].LastIndexOf("\\"))=pFilePath，不包含文件名

            //    //循环，打开每个文件
            //    for (int i = 0; i < pFullPaths.Length; i++)
            //    {
            //        //获取每一个文件的文件名
            //        string pFullPath = pFullPaths[i].ToString();
            //        int pIndex = pFullPath.LastIndexOf("\\");
            //        string pFileName = pFullPath.Substring(pIndex + 1);

            //        //打开文件并添加到map中
            //        IFeatureClass pFeaC = pFW.OpenFeatureClass(pFileName);
            //        IFeatureLayer pFeaL = new FeatureLayer();
            //        pFeaL.FeatureClass = pFeaC;
            //        pFeaL.Name = pFeaC.AliasName;
            //        pFeaL.SpatialReference = setSpatialReference(4508);
            //        axMapControl1.Map.AddLayer(pFeaL);
            //    }

            //    #region GDAL加载数据，用于构建索引 20180706
            //    //zh 20180706
            //    MainMap = new MapUI();
            //    MainMap.AddFiles(pFullPaths);
            //    #endregion


            //    //pActiveView = pMap as IActiveView;
            //    //pEngineEditor = new EngineEditorClass();
            //    //MapManager.EngineEditor = pEngineEditor;
            //    //pEngineEditTask = pEngineEditor as IEngineEditTask;
            //    //pEngineEditLayers = pEngineEditor as IEngineEditLayers;
            //    ////pEngineEditLayers.SetTargetLayer(pCurrentLyr, 0);


            //    plstLayers = GetLayers(pMap);


            //    axMapControl1.ActiveView.Refresh();
            //    //同步鹰眼
            //    SynchronizeEagleEye();

            //    axMapControl1.Refresh();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}

        }
        #endregion

        #region 加载栅格文件
        private void 打开Raster数据_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenFileDialog pOpenFileDialog = new OpenFileDialog();
            pOpenFileDialog.CheckFileExists = true;
            pOpenFileDialog.Title = "打开Raster文件";
            pOpenFileDialog.Filter = "栅格文件 (*.*)|*.bmp;*.tif;*.jpg;*.img;*.png|(*.bmp)|*.bmp|(*.tif)|*.tif|(*.jpg)|*.jpg|(*.img)|*.img|(*.png)|*.png";
            pOpenFileDialog.ShowDialog();

            string pRasterFileName = pOpenFileDialog.FileName;
            if (pRasterFileName == "")
            {
                return;
            }

            string pPath = System.IO.Path.GetDirectoryName(pRasterFileName);
            string pFileName = System.IO.Path.GetFileName(pRasterFileName);

            IWorkspaceFactory pWorkspaceFactory = new RasterWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(pPath, 0);
            IRasterWorkspace pRasterWorkspace = pWorkspace as IRasterWorkspace;
            IRasterDataset pRasterDataset = pRasterWorkspace.OpenRasterDataset(pFileName);
            //影像金字塔判断与创建
            IRasterPyramid3 pRasPyrmid;
            pRasPyrmid = pRasterDataset as IRasterPyramid3;
            if (pRasPyrmid != null)
            {
                if (!(pRasPyrmid.Present))
                {
                    pRasPyrmid.Create(); //创建金字塔
                }
            }
            IRaster pRaster;
            pRaster = pRasterDataset.CreateDefaultRaster();
            IRasterLayer pRasterLayer;
            pRasterLayer = new RasterLayerClass();
            pRasterLayer.CreateFromRaster(pRaster);
            pRasterLayer.SpatialReference = setSpatialReference(4508);
            ILayer pLayer = pRasterLayer as ILayer;
            axMapControl1.AddLayer(pLayer, 0);
        }
        #endregion

        /// <summary>
        /// 设置加载栅格个矢量数据时的投影坐标
        /// </summary>
        /// <param name="wkid">投影坐标的wkid</param>
        /// <returns></returns>
        public ISpatialReference setSpatialReference(int wkid)
        {
            ISpatialReferenceFactory pSpatialReferFac = new SpatialReferenceEnvironmentClass();
            ISpatialReference pSpatialRefer = pSpatialReferFac.CreateProjectedCoordinateSystem(wkid);
            return pSpatialRefer;
        }
             
        #endregion
        #region 保存
        private void 保存_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                SaveEditCommandClass m_saveEditCom = new SaveEditCommandClass();
                m_saveEditCom.OnCreate(axMapControl1.Object);
                m_saveEditCom.OnClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            ////try
            ////{
            //string sMxdFileName = axMapControl1.DocumentFilename;
            //    IMapDocument pMapDocument = new MapDocumentClass();
            //    //if (sMxdFileName != null && axMapControl1.CheckMxFile(sMxdFileName))
            //    //{
            //        if (pMapDocument.get_IsReadOnly(sMxdFileName))
            //        {
            //            MessageBox.Show("本地图文档是只读的，不能保存!");
            //            pMapDocument.Close();
            //            return;
            //        }
            //    //}
            //    else
            //    {
            //        SaveFileDialog pSaveFileDialog = new SaveFileDialog();
            //        pSaveFileDialog.Title = "请选择保存路径";
            //        pSaveFileDialog.OverwritePrompt = true;
            //        pSaveFileDialog.Filter = "ShapeFile文档（*.mxd）|*.mxd";
            //        pSaveFileDialog.RestoreDirectory = true;
            //        pSaveFileDialog.FileName = axMapControl1.DocumentFilename;
            //        if (pSaveFileDialog.ShowDialog() == DialogResult.OK)
            //        {
            //            pMapDocument.Save(true, true);
            //            pMapDocument.Close();
            //            MessageBox.Show("保存地图文档成功!");
            //            //sMxdFileName = pSaveFileDialog.FileName;
            //        }
            //        else
            //        {
            //            return;
            //        }
            //    }

            //    //pMapDocument.New(sMxdFileName);
            //    //pMapDocument.ReplaceContents(axMapControl1.Map as IMxdContents);
            //    //pMapDocument.Save(pMapDocument.UsesRelativePaths, true);
            //    //pMapDocument.Close();
            //    //MessageBox.Show("保存地图文档成功!");
            ////}
            ////catch (Exception ex)
            ////{
            ////    MessageBox.Show(ex.Message);
            ////}

            //ICommand command = new ControlsSaveAsDocCommandClass();
            //command.OnCreate(axMapControl1.Object);
            //command.OnClick();
        }
        #endregion

        #region 另存为
        private void 另存为_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            //string[] pFullPath = pOpenFileDialog.FileNames;
            //for (int i = 0; i < pFullPath.Length; i++)
            //{
            //    if (pFullPath[i] == "") return;

            //    int pIndex = pFullPath[i].LastIndexOf("\\");
            //    string pFilePath = pFullPath[i].Substring(0, pIndex); //文件路径
            //    string pFileName = pFullPath[i].Substring(pIndex + 1); //文件名

            //axMapControl1.AddShapeFile(pFilePath, pFileName);
            //}

            try
            {
                SaveFileDialog pSaveFileDialog = new SaveFileDialog();
                pSaveFileDialog.Title = "另存为";
                pSaveFileDialog.OverwritePrompt = true;
                pSaveFileDialog.Filter = "ShapeFile文档（*.shp）|*.shp";
                pSaveFileDialog.RestoreDirectory = true;
                pSaveFileDialog.DefaultExt = ".shp";

                string sFileName = pSaveFileDialog.FileName;
                int index = sFileName.LastIndexOf("\\");
                string sFilePath = sFileName.Substring(0, index);
                plstLayers = GetLayers(pMap);

                if (pSaveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < plstLayers.Count; i++)
                    {

                    }
                    //IMapDocument pMapDocument = new MapDocumentClass();
                    //for (int i = 0; i < sFilePath.Length; i++)
                    //{
                    //pMapDocument.SaveAs(sFilePath[i], true, true);
                    //}
                    //pMapDocument.New(sFilePath);
                    //pMapDocument.ReplaceContents(axMapControl1.Map as IMxdContents);

                    //pMapDocument.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 退出系统
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion
        #endregion

        #region AxTOCControl的右键菜单
        // 查看属性表
        private void 属性表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (frmAttribute == null || frmAttribute.IsDisposed)
            {
                frmAttribute = new FormAtrribute(axMapControl1);
            }
            frmAttribute.CurFeatureLayer = pTocFeatureLayer;
            frmAttribute.InitUI();
            frmAttribute.ShowDialog();
        }

        //缩放至图层
        private void 缩放到图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pTocFeatureLayer == null) return;
            (axMapControl1.Map as IActiveView).Extent = pTocFeatureLayer.AreaOfInterest;
            (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            SynchronizeEagleEye();
        }

        //移除图层
        private void 移除图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pTocFeatureLayer == null) return;
                DialogResult result = MessageBox.Show("是否删除[" + pTocFeatureLayer.Name + "]图层", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                
                if (result == DialogResult.OK)
                {                    
                        axMapControl1.Map.DeleteLayer(pTocFeatureLayer);
                }
                ILayer pL = null;
                for (int i = 0; i < axMapControl1.LayerCount; i++)
                {
                    pL = axMapControl1.get_Layer(i);
                    if (pL is IGroupLayer)
                    {
                        ICompositeLayer pGroupLayer = pL as ICompositeLayer;
                        if (pGroupLayer.Count==0)
                            axMapControl1.Map.DeleteLayer(pL);
                    }
                }
                ClearPCLayerLock();
                SynchronizeEagleEye();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
         }
        private void 移除所有图层ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否删除所有图层", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                axMapControl1.Map.ClearLayers();
                ClearPCLayerLock();
                axTOCControl1.Update();//更新图层栏图表信息
                SynchronizeEagleEye();
            }
        }      
        //移除图层后消除文件夹中的pc锁，也就是完全移除了图层
        public void ClearPCLayerLock()
        {
            IWorkspaceFactory pwf = new ShapefileWorkspaceFactory();
            //关闭资源锁定  
            IWorkspaceFactoryLockControl ipWsFactoryLock = (IWorkspaceFactoryLockControl)pwf;
            if (ipWsFactoryLock.SchemaLockingEnabled)
            {
                ipWsFactoryLock.DisableSchemaLocking();
            }

            axMapControl1.ActiveView.Refresh();
        }
        //图层可选
        private void btnLayerSel_Click(object sender, EventArgs e)
        {
            pTocFeatureLayer.Selectable = true;
            btnLayerSel.Enabled = !btnLayerSel.Enabled;
        }

        //图层不可选
        private void btnLayerUnSel_Click(object sender, EventArgs e)
        {
            pTocFeatureLayer.Selectable = false;
            btnLayerUnSel.Enabled = !btnLayerUnSel.Enabled;
        }

        private void 新建图层组ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IGroupLayer groupLayer = new GroupLayerClass();
            pMap.AddLayer(groupLayer);
            groupLayer.Add(pTocFeatureLayer);
            groupLayer.Name = "新建图层组";
            //axMapControl1.Map.DeleteLayer(pTocFeatureLayer);
            axTOCControl1.Update();                      
        }
      

        //设为活动图层
        private void 设为活动图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pTocFeatureLayer == null) return;
                DialogResult result = MessageBox.Show("是否将[" + pTocFeatureLayer.Name + "]图层设为活动图层", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    pEngineEditLayers.SetTargetLayer(pTocFeatureLayer, 0);
                    //zh 20180706 GDAL设置活动图层
                    if (MainMap!=null)
                    {
                        MainMap.SetActiveLayer("", pTocFeatureLayer.Name);
                    }
                    
                }
                axMapControl1.ActiveView.Refresh();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void axTOCControl1_OnDoubleClick(object sender, ITOCControlEvents_OnDoubleClickEvent e)
        {
            esriTOCControlItem itemType = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap basicMap = null;
            ILayer layer = null;
            object unk = null;
            object data = null;
            IField field = null;
            axTOCControl1.HitTest(e.x, e.y, ref itemType, ref basicMap, ref layer, ref unk, ref data);

            if (e.button == 1)
            {
                if (itemType == esriTOCControlItem.esriTOCControlItemLegendClass)
                {
                    //取得图例
                    ILegendClass pLegendClass = ((ILegendGroup)unk).get_Class((int)data);
                    //创建符号选择器SymbolSelector实例
                    SetParameter SymbolSelectorFrm = new SetParameter(pLegendClass, layer);
                    SymbolSelectorFrm.getFields(layer);
                    DialogResult dialogResult = SymbolSelectorFrm.ShowDialog();
                    if (dialogResult == DialogResult.OK)
                    {
                        //局部更新主Map控件
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                        //设置新的符号
                        pLegendClass.Symbol = SymbolSelectorFrm.pSymbol;
                        //更新主Map控件和图层控件
                        this.axMapControl1.ActiveView.Refresh();
                        this.axMapControl1.Refresh();
                    }
                    if (dialogResult == DialogResult.Yes)
                    {
                        //局部更新主Map控件
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

                        field = SymbolSelectorFrm.getField(layer);
                        string fieldname = null;
                        fieldname = field.Name;
                        IFeatureLayer featurelayer = layer as IFeatureLayer;
                        TextElementLabel(featurelayer, fieldname);
                        //更新主Map控件和图层控件
                        this.axMapControl1.ActiveView.Refresh();
                        this.axMapControl1.Refresh();

                    }


                }
            }


        }
        private void 设置图层参数ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //SetParameter form = new SetParameter();
            //form.Show();

            esriTOCControlItem itemType = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap basicMap = null;
            ILayer layer = pTocFeatureLayer as ILayer;
            object unk = null;
            object data = null;
            IField field = null;
            //axTOCControl1.HitTest(e.x, e.y, ref itemType, ref basicMap, ref layer, ref unk, ref data);

            //if (e.button == 1)
            {
                if (itemType == esriTOCControlItem.esriTOCControlItemLegendClass)
                {
                    //取得图例
                    ILegendClass pLegendClass = ((ILegendGroup)unk).get_Class((int)data);
                    //创建符号选择器SymbolSelector实例
                    SetParameter SymbolSelectorFrm = new SetParameter(pLegendClass, layer);
                    SymbolSelectorFrm.getFields(layer);
                    DialogResult dialogResult = SymbolSelectorFrm.ShowDialog();
                    if (dialogResult == DialogResult.OK)
                    {
                       // 局部更新主Map控件
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                       // 设置新的符号
                        pLegendClass.Symbol = SymbolSelectorFrm.pSymbol;
                       // 更新主Map控件和图层控件
                        this.axMapControl1.ActiveView.Refresh();
                        this.axMapControl1.Refresh();
                    }
                    if (dialogResult == DialogResult.Yes)
                    {
                        //局部更新主Map控件
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

                        field = SymbolSelectorFrm.getField(layer);
                        string fieldname = null;
                        fieldname = field.Name;
                        IFeatureLayer featurelayer = layer as IFeatureLayer;
                        TextElementLabel(featurelayer, fieldname);
                       // 更新主Map控件和图层控件
                        this.axMapControl1.ActiveView.Refresh();
                        this.axMapControl1.Refresh();
                    }
                }
            }
        }

        private void 拓扑冲突属性表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (topoErrorAttribute == null || topoErrorAttribute.IsDisposed)
            {
                topoErrorAttribute = new TopoErrorAtrribute(axMapControl1);
            }
            topoErrorAttribute.CurFeatureLayer = pTocFeatureLayer;
            topoErrorAttribute.InitUI();
            topoErrorAttribute.Show();
        }
        #endregion

        #region 图层控制
        // TOCControl的OnMouseDown事件
        private void axTOCControl1_OnMouseDown(object sender, ESRI.ArcGIS.Controls.ITOCControlEvents_OnMouseDownEvent e)
        {

            if (e.button == 2)
            {
                esriTOCControlItem pItem = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap pMap = null;
                ILayer pLayer = null;
                object unk = null;
                object data = null;
                axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pMap, ref pLayer, ref unk, ref data);
                pTocFeatureLayer = pLayer as IFeatureLayer;
                
                if (pItem == esriTOCControlItem.esriTOCControlItemLayer && pTocFeatureLayer != null)
                {
                    btnLayerSel.Enabled = !pTocFeatureLayer.Selectable;
                    btnLayerUnSel.Enabled = pTocFeatureLayer.Selectable;
                    contextMenuStrip1.Show(Control.MousePosition);
                }
                if (pItem == esriTOCControlItem.esriTOCControlItemMap) 
                {
                    //btnLayerSel.Enabled = !pTocFeatureLayer.Selectable;
                    //btnLayerUnSel.Enabled = pTocFeatureLayer.Selectable;
                    contextMenuStrip2.Show(Control.MousePosition);
                }

            }
            if (e.button == 1)
            {
                esriTOCControlItem pItem = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap pMap = null; object unk = null;
                object data = null; ILayer pLayer = null;
                axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pMap, ref pLayer, ref unk, ref data);
                if (pLayer == null) return;
                pMoveLayerPoint.PutCoords(e.x, e.y);//错误信息未将对象引用设置到对象的实例。
                if (pItem == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    if (pLayer is IAnnotationSublayer)
                    {
                        return;
                    }
                    else
                    {
                        pMoveLayer = pLayer;
                    }
                }
            }
        }
        // TOCControl的OnMouseUp事件
        private void axTOCControl1_OnMouseUp(object sender, ITOCControlEvents_OnMouseUpEvent e)
        {
            try
            {
                if (e.button == 1 && pMoveLayer != null && pMoveLayerPoint.Y != e.y)
                {
                    esriTOCControlItem pItem = esriTOCControlItem.esriTOCControlItemNone;
                    IBasicMap pBasicMap = null; object unk = null;
                    object data = null; ILayer pLayer = null;
                    axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pBasicMap, ref pLayer, ref unk, ref data);
                    //IMap pMap = axMapControl1.ActiveView.FocusMap;
                    if (pItem == esriTOCControlItem.esriTOCControlItemLayer || pLayer != null)
                    {
                        if (pMoveLayer != pLayer)
                        {
                            ILayer pTempLayer;
                            //获得鼠标弹起时所在图层的索引号
                            for (int i = 0; i < pMap.LayerCount; i++)
                            {
                                pTempLayer = pMap.get_Layer(i);
                                if (pTempLayer == pLayer)
                                {
                                    toIndex = i;
                                }
                            }
                        }
                    }
                    //移动到最前面
                    else if (pItem == esriTOCControlItem.esriTOCControlItemMap)
                    {
                        toIndex = 0;
                    }
                    //移动到最后面
                    else if (pItem == esriTOCControlItem.esriTOCControlItemNone)
                    {
                        toIndex = pMap.LayerCount - 1;
                    }
                    pMap.MoveLayer(pMoveLayer, toIndex);
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 鹰眼的实现及同步
        //  地图被替换时触发
        private void axMapControl1_OnMapReplaced_1(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            SynchronizeEagleEye();
        }
        //鹰眼的实现
        private void SynchronizeEagleEye()
        {
            if (EagleEyeMapControl.LayerCount > 0)
            {
                EagleEyeMapControl.ClearLayers();
            }
            //设置鹰眼和主地图的坐标系统一致
            EagleEyeMapControl.SpatialReference = axMapControl1.SpatialReference;
            for (int i = axMapControl1.LayerCount - 1; i >= 0; i--)
            {
                //使鹰眼视图与数据视图的图层上下顺序保持一致
                ILayer pLayer = axMapControl1.get_Layer(i);
                if (pLayer is IGroupLayer || pLayer is ICompositeLayer)
                {
                    ICompositeLayer pCompositeLayer = (ICompositeLayer)pLayer;
                    for (int j = pCompositeLayer.Count - 1; j >= 0; j--)
                    {
                        ILayer pSubLayer = pCompositeLayer.get_Layer(j);
                        IFeatureLayer pFeatureLayer = pSubLayer as IFeatureLayer;
                        if (pFeatureLayer != null)
                        {
                            //由于鹰眼地图较小，所以过滤点图层不添加
                            if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint
                                && pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryMultipoint)
                            {
                                EagleEyeMapControl.AddLayer(pLayer);
                            }
                        }
                    }
                }
                else
                {
                    IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
                    if (pFeatureLayer != null)
                    {
                        if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint
                            && pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryMultipoint)
                        {
                            EagleEyeMapControl.AddLayer(pLayer);
                        }
                    }
                }
                //设置鹰眼地图全图显示  
                EagleEyeMapControl.Extent = axMapControl1.FullExtent;
                pEnv = axMapControl1.Extent as IEnvelope;
                DrawRectangle(pEnv);
                EagleEyeMapControl.ActiveView.Refresh();
            }
        }
        // 判断鼠标点击点的位置
        private void EagleEyeMapControl_OnMouseDown_1(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (EagleEyeMapControl.Map.LayerCount > 0)
            {
                //按下鼠标左键移动矩形框
                if (e.button == 1)
                {
                    //如果指针落在鹰眼的矩形框中，标记可移动
                    if (e.mapX > pEnv.XMin && e.mapY > pEnv.YMin && e.mapX < pEnv.XMax && e.mapY < pEnv.YMax)
                    {
                        bCanDrag = true;
                    }
                    pMoveRectPoint = new PointClass();

                    pMoveRectPoint.PutCoords(e.mapX, e.mapY);  //记录点击的第一个点的坐标
                }
                //按下鼠标右键绘制矩形框
                else if (e.button == 2)
                {
                    IEnvelope pEnvelope = EagleEyeMapControl.TrackRectangle();

                    IPoint pTempPoint = new PointClass();
                    pTempPoint.PutCoords(pEnvelope.XMin + pEnvelope.Width / 2, pEnvelope.YMin + pEnvelope.Height / 2);
                    axMapControl1.Extent = pEnvelope;
                    //矩形框的高宽和数据试图的高宽不一定成正比，这里做一个中心调整
                    axMapControl1.CenterAt(pTempPoint);
                }
            }
        }
        //移动矩形框
        private void EagleEyeMapControl_OnMouseMove_1(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            if (e.mapX > pEnv.XMin && e.mapY > pEnv.YMin && e.mapX < pEnv.XMax && e.mapY < pEnv.YMax)
            {
                //如果鼠标移动到矩形框中，鼠标换成小手，表示可以拖动
                EagleEyeMapControl.MousePointer = esriControlsMousePointer.esriPointerHand;
                if (e.button == 2)  //如果在内部按下鼠标右键，将鼠标演示设置为默认样式
                {
                    EagleEyeMapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
                }
            }
            else
            {
                //在其他位置将鼠标设为默认的样式
                EagleEyeMapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
            }

            if (bCanDrag)
            {
                double Dx, Dy;  //记录鼠标移动的距离
                Dx = e.mapX - pMoveRectPoint.X;
                Dy = e.mapY - pMoveRectPoint.Y;
                pEnv.Offset(Dx, Dy); //根据偏移量更改 pEnv 位置
                pMoveRectPoint.PutCoords(e.mapX, e.mapY);
                DrawRectangle(pEnv);
                axMapControl1.Extent = pEnv;
            }
        }
        // 判断鼠标是否是点击
        private void EagleEyeMapControl_OnMouseUp_1(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
            if (e.button == 1 && pMoveRectPoint != null)
            {
                if (e.mapX == pMoveRectPoint.X && e.mapY == pMoveRectPoint.Y)
                {
                    axMapControl1.CenterAt(pMoveRectPoint);
                }
                bCanDrag = false;
            }
        }
        //绘制矩形框
        private void axMapControl1_OnExtentUpdated_1(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            //得到当前视图范围
            pEnv = (IEnvelope)e.newEnvelope;
            DrawRectangle(pEnv);
        }

        //在鹰眼地图上面画矩形框
        private void DrawRectangle(IEnvelope pEnvelope)
        {
            //在绘制前，清除鹰眼中之前绘制的矩形框
            IGraphicsContainer pGraphicsContainer = EagleEyeMapControl.Map as IGraphicsContainer;
            IActiveView pActiveView = pGraphicsContainer as IActiveView;
            pGraphicsContainer.DeleteAllElements();
            //得到当前视图范围
            IRectangleElement pRectangleElement = new RectangleElementClass();
            IElement pElement = pRectangleElement as IElement;
            pElement.Geometry = pEnvelope;
            //设置矩形框（实质为中间透明度面）
            IRgbColor pColor = new RgbColorClass();
            pColor = GetRgbColor(255, 0, 0);
            pColor.Transparency = 255;
            ILineSymbol pOutLine = new SimpleLineSymbolClass();
            pOutLine.Width = 1;
            pOutLine.Color = pColor;
            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pColor = new RgbColorClass();
            pColor.Transparency = 0;
            pFillSymbol.Color = pColor;
            pFillSymbol.Outline = pOutLine;
            //向鹰眼中添加矩形框
            IFillShapeElement pFillShapeElement = pElement as IFillShapeElement;
            pFillShapeElement.Symbol = pFillSymbol;
            pGraphicsContainer.AddElement((IElement)pFillShapeElement, 0);
            //刷新
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        #endregion

        #region 布局视图与数据视图的同步

        // 把数据视图中的数据复制到布局视图中
        private void CopyToPageLayout()
        {
            IObjectCopy pObjectCopy = new ObjectCopyClass();
            object copyFromMap = axMapControl1.Map;
            
            object copiedMap = pObjectCopy.Copy(copyFromMap);//复制地图到copiedMap中
            object copyToMap = axPageLayoutControl.ActiveView.FocusMap;
            pObjectCopy.Overwrite(copiedMap, ref copyToMap); //复制地图
            axPageLayoutControl.ActiveView.Refresh();
        }
        // 地图被重绘时触发
        private void axMapControl1_OnAfterScreenDraw_1(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            //IActiveView pActiveView = (IActiveView)axPageLayoutControl.ActiveView.FocusMap;
            //IDisplayTransformation displayTransformation = pActiveView.ScreenDisplay.DisplayTransformation;
            //displayTransformation.VisibleBounds = axMapControl1.Extent;
            //axPageLayoutControl.ActiveView.Refresh();
            //CopyToPageLayout();
        }
        #endregion

        #region 地图导出

        #region 全域导出
        private void 全域导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string pMouseOperate = null;
            axMapControl1.CurrentTool = null;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            pMouseOperate = "ExportRegion";
        }
        #endregion

        #region 区域导出

        private void 区域导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (frmExpMap == null || frmExpMap.IsDisposed)
            {
                frmExpMap = new FormExportMap(axMapControl1);
            }
            frmExpMap.IsRegion = false;
            frmExpMap.GetGeometry = axMapControl1.ActiveView.Extent;
            frmExpMap.Show();
            frmExpMap.Activate();
        }
        #endregion

        #endregion

        #region axMapControl1的鼠标事件
        private void axMapControl1_OnMouseDown_1(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            //屏幕坐标点转化为地图坐标点
            pPointPt = (axMapControl1.Map as IActiveView).ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
            if (e.button == 1)
            {
                IActiveView pActiveView = axMapControl1.ActiveView;
                IEnvelope pEnvelope = new EnvelopeClass();  

                switch (pMouseOperate)
                {
                    #region 新增注记
                    case "addText":
                        IPoint pPoint = new PointClass();
                        pPoint = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
                        //设置文本格式
                        ITextSymbol pTextSymbol = new TextSymbolClass();
                        StdFont myFont = new stdole.StdFontClass();
                        myFont.Name = "宋体";
                        myFont.Size = 24;
                        pTextSymbol.Font = (IFontDisp)myFont;
                        pTextSymbol.Angle = 0;
                        pTextSymbol.RightToLeft = false;//文本由左向右排列
                        pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVABaseline;//垂直方向基线对齐
                        pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHAFull;//文本两端对齐
                        pTextSymbol.Text = AddText.AddText1;
                        ITextElement pTextElement = new TextElementClass();
                        pTextElement.Symbol = pTextSymbol;
                        pTextElement.Text = pTextSymbol.Text;
                        //获取一个图上坐标，且将文本添加到此位置           
                        IElement pElement = (IElement)pTextElement;
                        pElement.Geometry = pPoint;
                        axMapControl1.ActiveView.GraphicsContainer.AddElement(pElement, 0);
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, pElement, null);
                        break;
                    #endregion
                    #region 拉框放大

                    case "ZoomIn":
                        pEnvelope = axMapControl1.TrackRectangle();
                        //如果拉框范围为空则返回
                        if (pEnvelope == null || pEnvelope.IsEmpty || pEnvelope.Height == 0 || pEnvelope.Width == 0)
                        {
                            return;
                        }
                        //如果有拉框范围，则放大到拉框范围
                        pActiveView.Extent = pEnvelope;
                        pActiveView.Refresh();
                        break;

                    #endregion

                    #region 拉框缩小

                    case "ZoomOut":
                        pEnvelope = axMapControl1.TrackRectangle();

                        //如果拉框范围为空则退出
                        if (pEnvelope == null || pEnvelope.IsEmpty || pEnvelope.Height == 0 || pEnvelope.Width == 0)
                        {
                            return;
                        }
                        //如果有拉框范围，则以拉框范围为中心，缩小倍数为：当前视图范围/拉框范围
                        else
                        {
                            double dWidth = pActiveView.Extent.Width * pActiveView.Extent.Width / pEnvelope.Width;
                            double dHeight = pActiveView.Extent.Height * pActiveView.Extent.Height / pEnvelope.Height;
                            double dXmin = pActiveView.Extent.XMin -
                                           ((pEnvelope.XMin - pActiveView.Extent.XMin) * pActiveView.Extent.Width /
                                            pEnvelope.Width);
                            double dYmin = pActiveView.Extent.YMin -
                                           ((pEnvelope.YMin - pActiveView.Extent.YMin) * pActiveView.Extent.Height /
                                            pEnvelope.Height);
                            double dXmax = dXmin + dWidth;
                            double dYmax = dYmin + dHeight;
                            pEnvelope.PutCoords(dXmin, dYmin, dXmax, dYmax);
                        }
                        pActiveView.Extent = pEnvelope;
                        pActiveView.Refresh();
                        break;
                    #endregion

                    #region 漫游

                    case "Pan":
                        axMapControl1.Pan();
                        break;

                    #endregion

                    #region 区域导出
                    case "ExportRegion":
                        //删除视图中数据
                        axMapControl1.ActiveView.GraphicsContainer.DeleteAllElements();
                        axMapControl1.ActiveView.Refresh();
                        IPolygon pPolygon = DrawPolygon(axMapControl1);
                        if (pPolygon == null) return;
                        ExportMap.AddElement(pPolygon, axMapControl1.ActiveView);
                        if (frmExpMap == null || frmExpMap.IsDisposed)
                        {
                            frmExpMap = new FormExportMap(axMapControl1);
                        }
                        frmExpMap.IsRegion = true;
                        frmExpMap.GetGeometry = pPolygon as IGeometry;
                        frmExpMap.Show();
                        frmExpMap.Activate();
                        break;
                    #endregion



                    #region 点选选择要素
                    case "PointSelectFeature":
                        //axMapControl1.MousePointer=esriControlsMousePointer.esriPointerCrosshair;
                        ESRI.ArcGIS.Geometry.Point point = new ESRI.ArcGIS.Geometry.PointClass();
                        point.X = e.mapX;
                        point.Y = e.mapY;
                        IGeometry geometry= point as IGeometry;
                        axMapControl1.Map.SelectByShape(geometry, null, false);
                        axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);

                        //IGeometry pGeoPoint = axMapControl1.TrackCircle();
                        //axMapControl1.Map.SelectByShape(pGeoPoint, null, false);
                        //axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        //axMapControl1.CurrentTool = null;
                       
                        //二师兄写的    
                        //SelectFeatureToolClass s = new SelectFeatureToolClass();
                        //    s.OnCreate(axMapControl1.Object);
                        //    s.OnClick();
                        //    s.OnMouseDown(1, 1, e.x, e.y);

                        //可执行最终代码
                        //ICommand m_SelTool = new SelectFeatureToolClass();
                        //m_SelTool.OnCreate(axMapControl1.Object);
                        //m_SelTool.OnClick();
                        //axMapControl1.CurrentTool = m_SelTool as ITool;
                        //axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
                        break;
                    #endregion

                    #region 点击查看属性
                    case "PointReturnFeatureAttribute":
                         IGeometry pGeoPoint1 = axMapControl1.TrackCircle();
                         //IFeatureLayer featurelayer = axMapControl1.get_Layer(0) as IFeatureLayer;
                        axMapControl1.Map.SelectByShape(pGeoPoint1, null, true);
                        axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        IEnumFeature pEnumFeature1 = axMapControl1.Map.FeatureSelection as IEnumFeature;
                        pFeature1 = pEnumFeature1.Next();
                        axMapControl1.CurrentTool = null;
                        if (IdentifierForm == null || IdentifierForm.IsDisposed)
                        {
                            IdentifierForm = new Identifier( axMapControl1);
                        }
                        IdentifierForm.PFeature = pFeature1;
                        if (pFeature1 == null)
                        {
                            MessageBox.Show("您没有选中要素");
                            return;
                        }
                        else
                        {
                            IdentifierForm.InitUI();
                            IdentifierForm.ShowDialog();
                        }
                        
                        break;
                    #endregion

                    #region 线选选择要素
                    case "LineSelectFeature":
                        IGeometry pGeoLine = axMapControl1.TrackLine();
                        axMapControl1.Map.SelectByShape(pGeoLine, null, false);
                        axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        axMapControl1.CurrentTool = null;
                        break;
                    #endregion

                    #region 框选选择要素
                    case "RectSelectFeature":
                        IEnvelope pEnv = axMapControl1.TrackRectangle();
                        IGeometry pGeoRect = pEnv as IGeometry;
                        axMapControl1.Map.SelectByShape(pGeoRect, null, false);
                        axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        axMapControl1.CurrentTool = null;
                        break;

                    #endregion

                    #region 任意多边形选择要素
                    case "polygonSelectFeature":
                        IGeometry pGeopolygon = axMapControl1.TrackPolygon();
                        axMapControl1.Map.SelectByShape(pGeopolygon, null, false);
                        axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        axMapControl1.CurrentTool = null;
                        break;
                    #endregion

                    #region 清除选择
                    case "CancelSelectFeature":
                        axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
                        axMapControl1.Map.ClearSelection();
                        axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        axMapControl1.CurrentTool = null;
                        break;
                    #endregion

                    #region 全选
                    case "SelectAllFeature":
                        axMapControl1.Map.SelectByShape(null, null, false);
                        axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        IGeometry pgeometry = axMapControl1.TrackCancel as IGeometry;
                        axMapControl1.Map.SelectByShape(pgeometry, null, false);
                        axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        axMapControl1.CurrentTool = null;
                        break;
                    #endregion

                    #region 反选
                    case "ReverseSelectFeature":
                        IFeatureLayer pFeatLyr = m_EngineEditLayers.TargetLayer;
                        IFeatureSelection FeatureSelection = pFeatLyr as IFeatureSelection;
                        FeatureSelection.SelectFeatures(null, esriSelectionResultEnum.esriSelectionResultNew, false);
                        IQueryFilter QueryFilter = new QueryFilterClass();
                        QueryFilter.WhereClause = "OBJECTID >= 2 AND OBJECTID <= 5";
                        FeatureSelection.SelectFeatures(QueryFilter, esriSelectionResultEnum.esriSelectionResultSubtract, false);
                        axMapControl1.Refresh();
                        break;
                    #endregion

                    #region 移动要素
                    case "MoveFeature":
                        try
                        {
                            m_moveTool.OnCreate(axMapControl1.Object);
                            m_moveTool.OnClick();
                            axMapControl1.CurrentTool = m_moveTool as ITool;
                            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        break;
                    #endregion

                    #region 新建点
                    case "NewPoint":
                        try
                        {
                            m_CreateFeatTool.OnCreate(axMapControl1.Object);
                            m_CreateFeatTool.OnClick();
                            axMapControl1.CurrentTool = m_CreateFeatTool as ITool;
                            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                            m_CreateFeatTool.OnMouseDown(1, 1, e.x, e.y);


                            //IPoint pt = axMapControl1.ToMapPoint(e.x, e.y);
                            //IMarkerElement pMarkerElement = new MarkerElementClass();
                            //IElement pElement = pMarkerElement as IElement;
                            //pElement.Geometry = pt;
                            //IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
                            //pGraphicsContainer.AddElement(pElement, 0);
                            //pActiveView.Refresh();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        break;
                    #endregion

                    #region 线操作
                    #region 任意线绘制
                    case "NewLine":
                        try
                        {
                            m_CreateFeatTool.OnCreate(axMapControl1.Object);
                            m_CreateFeatTool.OnClick();
                            axMapControl1.CurrentTool = m_CreateFeatTool as ITool;
                            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                            m_CreateFeatTool.OnMouseDown(1, 1, e.x, e.y);

                            //IGeometry polyline = axMapControl1.TrackLine();
                            //ILineElement pLineElement = new LineElementClass();
                            //IElement pElement = pLineElement as IElement;
                            //pElement.Geometry = polyline;
                            //IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
                            //pGraphicsContainer.AddElement(pElement, 0);
                            //pActiveView.Refresh();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        break;
                    #endregion
                    #endregion

                    #region 面操作
                    #region 面绘制
                    case "Newpolygon":
                        try
                        {
                            m_CreateFeatTool.OnCreate(axMapControl1.Object);
                            m_CreateFeatTool.OnClick();
                            axMapControl1.CurrentTool = m_CreateFeatTool as ITool;
                            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                            m_CreateFeatTool.OnMouseDown(1, 1, e.x, e.y);

                            //IGeometry Polygon = axMapControl1.TrackPolygon();
                            //IPolygonElement PolygonElement = new PolygonElementClass();
                            //IElement pElement = PolygonElement as IElement;
                            //pElement.Geometry = Polygon;
                            //IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
                            //pGraphicsContainer.AddElement(pElement, 0);
                            //pActiveView.Refresh();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        break;
                    #endregion
                    #endregion

                    #region 复制
                    case "FeatureCopy":
                        try
                        {

                            IFeature pfeature = null;
                            IGeometry pge = pfeature.ShapeCopy;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        break;
                    #endregion

                    #region 长度量测
                    case "MeasureLength":
                        pPointPt = (axMapControl1.Map as IActiveView).ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);//屏幕坐标点转化为地图坐标点
                        // 判断追踪线对象是否为空，若是则实例化并设置当前鼠标点为起始点
                        if (pNewLineFeedback == null)
                        {
                            // 实例化追踪线对象
                            pNewLineFeedback = new NewLineFeedbackClass();
                            pNewLineFeedback.Display = (axMapControl1.Map as IActiveView).ScreenDisplay;//将追踪的线对象在地图上显示出来
                            //设置起点，开始绘制动态线
                            pNewLineFeedback.Start(pPointPt);
                            dToltalLength = 0;
                        }
                        else//如果追踪线对象不为空，则添加当前鼠标点
                        {
                            pNewLineFeedback.AddPoint(pPointPt);
                        }
                        //pGeometry = m_Pointpt;
                        if (dSegmentLength != 0)
                        {
                            dToltalLength = dToltalLength + dSegmentLength;
                        }
                        break;
                    #endregion

                    #region 属性编辑
                    case "AttributeEdit":
                        EditAtrributeToolClass m_AtrributeCom = new EditAtrributeToolClass();
                        m_AtrributeCom.OnCreate(axMapControl1.Object);
                        m_AtrributeCom.OnClick();
                        axMapControl1.CurrentTool = m_AtrributeCom as ITool;
                        axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
                        m_AtrributeCom.OnMouseDown(1, 1, e.x, e.y);
                        break;
                    #endregion

                    #region 面积量算
                    case "MeasureArea":
                        pPointPt = (axMapControl1.Map as IActiveView).ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);//屏幕坐标点转化为地图坐标点
                        if (pNewPolygonFeedback == null)
                        {
                            //实例化追踪面对象
                            pNewPolygonFeedback = new NewPolygonFeedback();
                            pNewPolygonFeedback.Display = (axMapControl1.Map as IActiveView).ScreenDisplay;

                            pAreaPointCol.RemovePoints(0, pAreaPointCol.PointCount);
                            //开始绘制多边形
                            pNewPolygonFeedback.Start(pPointPt);
                            pAreaPointCol.AddPoint(pPointPt, ref missing, ref missing);//存储点
                        }
                        else
                        {
                            pNewPolygonFeedback.AddPoint(pPointPt);
                            pAreaPointCol.AddPoint(pPointPt, ref missing, ref missing);
                        }
                        break;
                    #endregion

                }
            }
        }

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            sMapUnits = GetMapUnit(axMapControl1.Map.MapUnits);
            barCoorTxt.Text = String.Format("当前坐标：X = {0:#.###} Y = {1:#.###} {2}", e.mapX, e.mapY, sMapUnits);
            pMovePt = (axMapControl1.Map as IActiveView).ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);

            #region 长度量算
            if (pMouseOperate == "MeasureLength")
            {
                if (pNewLineFeedback != null)
                {
                    pNewLineFeedback.MoveTo(pMovePt);
                }
                double deltaX = 0; //两点之间X差值
                double deltaY = 0; //两点之间Y差值

                if ((pPointPt != null) && (pNewLineFeedback != null))
                {
                    deltaX = pMovePt.X - pPointPt.X;
                    deltaY = pMovePt.Y - pPointPt.Y;
                    dSegmentLength = Math.Round(Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY)), 3);
                    dToltalLength = dToltalLength + dSegmentLength;
                    if (frmMeasureResult != null)
                    {
                        frmMeasureResult.lblMeasureResult.Text = String.Format(
                            "当前线段长度：{0:.###}{1};\r\n总长度为: {2:.###}{1}",
                            dSegmentLength, sMapUnits, dToltalLength);
                        dToltalLength = dToltalLength - dSegmentLength; //鼠标移动到新点重新开始计算
                    }
                    frmMeasureResult.frmClosed += new FormMeasureResult.FormClosedEventHandler(frmMeasureResult_frmColsed);
                }
            }
            #endregion

            #region 面积量算
            if (pMouseOperate == "MeasureArea")
            {
                if (pNewPolygonFeedback != null)
                {
                    pNewPolygonFeedback.MoveTo(pMovePt);
                }

                IPointCollection pPointCol = new Polygon();
                IPolygon pPolygon = new PolygonClass();
                IGeometry pGeo = null;

                ITopologicalOperator pTopo = null;
                for (int i = 0; i <= pAreaPointCol.PointCount - 1; i++)//点击存储的点，添加到多边形
                {
                    pPointCol.AddPoint(pAreaPointCol.get_Point(i), ref missing, ref missing);
                }
                pPointCol.AddPoint(pMovePt, ref missing, ref missing);//滑动存储的点

                if (pPointCol.PointCount < 3) return;
                pPolygon = pPointCol as IPolygon;

                if ((pPolygon != null))
                {
                    pPolygon.Close();
                    pGeo = pPolygon as IGeometry;
                    pTopo = pGeo as ITopologicalOperator;//为成员提供基于现有几何结构之间的拓扑关系构造新的几何构型的成员的访问
                    //使几何图形的拓扑正确
                    pTopo.Simplify();
                    pGeo.Project(axMapControl1.Map.SpatialReference);
                    IArea pArea = pGeo as IArea;

                    frmMeasureResult.lblMeasureResult.Text = String.Format(
                        "总面积为：{0:.####}平方{1};\r\n总长度为：{2:.####}{1}",
                        pArea.Area, sMapUnits, pPolygon.Length);
                    pPolygon = null;
                }
            }

            #endregion

        }

        private void axMapControl1_OnDoubleClick(object sender, IMapControlEvents2_OnDoubleClickEvent e)
        {
            #region 长度量算
            if (pMouseOperate == "MeasureLength")
            {
                if (frmMeasureResult != null)
                {
                    frmMeasureResult.lblMeasureResult.Text = "线段总长度为：" + dToltalLength + sMapUnits;
                }
                if (pNewLineFeedback != null)
                {
                    pNewLineFeedback.Stop();
                    pNewLineFeedback = null;
                    //清空所画的线对象
                    (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
                }
                dToltalLength = 0;
                dSegmentLength = 0;
            }
            #endregion

            #region 面积量算
            if (pMouseOperate == "MeasureArea")
            {
                if (pNewPolygonFeedback != null)
                {
                    pNewPolygonFeedback.Stop();
                    pNewPolygonFeedback = null;
                    //清空所画的线对象
                    (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
                }
                pAreaPointCol.RemovePoints(0, pAreaPointCol.PointCount); //清空点集中所有点
            }
            #endregion

        }
        #endregion

        #region 视图

        private void 漫游_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "Pan";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPan;
        }
        private void 拉框放大_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "ZoomIn";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerZoomIn;
        }
        private void 拉框缩小_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            axMapControl1.CurrentTool = null;
            pMouseOperate = "ZoomOut";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerZoomOut;
        }
        private void 逐级放大_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            IEnvelope pEnvelope;
            pEnvelope = axMapControl1.Extent;
            pEnvelope.Expand(0.5, 0.5, true);     //这里设置放大为2倍，可以根据需要具体设置
            axMapControl1.Extent = pEnvelope;
            axMapControl1.ActiveView.Refresh();
        }
        private void 逐级缩小_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            IActiveView pActiveView = axMapControl1.ActiveView;//获取当前试图
            IPoint centerPoint = new PointClass();//定义中心点
            centerPoint.PutCoords((pActiveView.Extent.XMin + pActiveView.Extent.XMax) / 2, (pActiveView.Extent.YMax + pActiveView.Extent.YMin) / 2);//中心点位置
            IEnvelope envlope = pActiveView.Extent;
            envlope.Expand(1.5, 1.5, true);       //和放大的区别在于Expand函数的参数不同
            pActiveView.Extent.CenterAt(centerPoint);
            pActiveView.Extent = envlope;
            pActiveView.Refresh();
        }
        private void 全图_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.axMapControl1.Extent = this.axMapControl1.FullExtent;
        }

        IExtentStack pExtentStack;

        private void 前一视图_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            pExtentStack = axMapControl1.ActiveView.ExtentStack;
            //判断是否可以回到前一视图，第一个视图没有前一视图
            if (pExtentStack.CanUndo())
            {
                pExtentStack.Undo();
                //btnForWardView.Enabled = true;
                if (!pExtentStack.CanUndo())
                {
                    //btnFrontView.Enabled = false;
                }
            }
            axMapControl1.ActiveView.Refresh();
        }

        private void 后一视图_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            pExtentStack = axMapControl1.ActiveView.ExtentStack;
            //判断是否可以回到后一视图，最后一个视图没有后一视图
            if (pExtentStack.CanRedo())
            {
                pExtentStack.Redo();
                //btnFrontView.Enabled = true;
                if (!pExtentStack.CanRedo())
                {
                    //btnForWardView.Enabled = false;
                }
            }
            axMapControl1.ActiveView.Refresh();
        }
        #endregion

        #region 量算
        private void 长度量算_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "MeasureLength";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            if (frmMeasureResult == null || frmMeasureResult.IsDisposed)
            {
                frmMeasureResult = new FormMeasureResult();
                frmMeasureResult.frmClosed += new FormMeasureResult.FormClosedEventHandler(frmMeasureResult_frmColsed);
                frmMeasureResult.lblMeasureResult.Text = "";
                frmMeasureResult.Text = "距离量测";
                frmMeasureResult.Show();
            }
            else
            {
                frmMeasureResult.Activate();
            }
        }

        #region 面积量算
        private void 面积量算_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "MeasureArea";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;//将鼠标指针形式设置为十字丝形式
            if (frmMeasureResult == null || frmMeasureResult.IsDisposed)//如果两侧结果窗体为空并且已经被释放
            {
                frmMeasureResult = new FormMeasureResult();
                frmMeasureResult.frmClosed += new FormMeasureResult.FormClosedEventHandler(frmMeasureResult_frmColsed);
                frmMeasureResult.lblMeasureResult.Text = "";
                frmMeasureResult.Text = "面积量测";
                frmMeasureResult.Show();
            }
            else
            {
                frmMeasureResult.Activate();
            }
        }
        #endregion

        #region 测量结果窗口关闭响应事件
        private void frmMeasureResult_frmColsed()
        {
            //清空线对象
            if (pNewLineFeedback != null)
            {
                pNewLineFeedback.Stop();
                pNewLineFeedback = null;
            }
            //清空面对象
            if (pNewPolygonFeedback != null)
            {
                pNewPolygonFeedback.Stop();
                pNewPolygonFeedback = null;
                pAreaPointCol.RemovePoints(0, pAreaPointCol.PointCount); //清空点集中所有点
            }
            //清空量算画的线、面对象
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
            //结束量算功能
            pMouseOperate = string.Empty;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
        }
        #endregion

        #endregion

        #region 获取当前地图文档所有地图集合
        public static List<ILayer> GetLayers(IMap pMap)
        {
            ILayer pLyr = null;
            List<ILayer> pLstLayers = null;
            try
            {
                pLstLayers = new List<ILayer>();
                for (int i = 0; i < pMap.LayerCount; i++)
                {
                    pLyr = pMap.get_Layer(i);
                    if (!pLstLayers.Contains(pLyr))
                    {
                        pLstLayers.Add(pLyr);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return pLstLayers;
        }
        #endregion

        #region 工具
        private void 点选_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "PointSelectFeature";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void 线选_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "LineSelectFeature";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void 框选_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "RectSelectFeature";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void 任意多边形选择_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "polygonSelectFeature";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void 除所选_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
                axMapControl1.Map.ClearSelection();
                axMapControl1.Refresh();

                //要不要用mapcontrol的mousedown事件
                //axMapControl1.CurrentTool = null;
                //pMouseOperate = "CancelSelectFeature";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // 读取工作空间下的所有要素
        public List<Feature> ReadFeatureClassList(IWorkspace pWs)
        {
            //定义接收所有featureclass的数组
            List<Feature> list_sdefeatures = new List<Feature>();
            //打开要素集
            IFeatureWorkspace pFeatureWorkspace = pWs as IFeatureWorkspace;
            IFeatureClass esriFeatureClass = null;
            //遍历要素集Dataset下的featureclass
            IEnumDataset penumDatasets = pWs.get_Datasets(esriDatasetType.esriDTFeatureDataset);
            penumDatasets.Reset();
            IDataset pesriDataset = penumDatasets.Next();
            while (pesriDataset != null)
            {
                if (pesriDataset is IFeatureDataset)
                {
                    // try to find class in dataset
                    try
                    {
                        IFeatureClassContainer featContainer = (IFeatureClassContainer)pesriDataset;
                        IEnumFeatureClass pEnumFC = featContainer.Classes;
                        esriFeatureClass = pEnumFC.Next();
                        while (esriFeatureClass != null)
                        {
                            IDataset pDS = esriFeatureClass as IDataset;
                            IFeatureClass featureclass = pDS as IFeatureClass;
                            IFeature feature = featureclass as IFeature;
                            //list_sdefeatures.Add(feature);
                            esriFeatureClass = pEnumFC.Next();
                        }
                    }
                    catch { }
                    // try another dataset                        
                }
                pesriDataset = penumDatasets.Next();
            }

            //遍历工作空间下的featureclass
            IEnumDataset enumDatasets = pWs.get_Datasets(esriDatasetType.esriDTFeatureClass) as IEnumDataset;
            enumDatasets.Reset();
            IDataset esriDataset = enumDatasets.Next() as IDataset;
            while (esriDataset is IFeatureClass)
            {
                IFeatureLayer pFeatureLayer = new FeatureLayerClass();
                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(esriDataset.Name);

                //获取面要素
                if (pFeatureLayer.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                {
                    //list_sdefeatures.Add(esriDataset.Name.ToString());
                }
                //获取线要素
                if (pFeatureLayer.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                {
                    //list_sdefeatures.Add(esriDataset.Name.ToString());
                }
                //获取点要素
                if (pFeatureLayer.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint)
                {
                    //list_sdefeatures.Add(esriDataset.Name.ToString());
                }
                esriDataset = enumDatasets.Next();
            }
            return list_sdefeatures;

        }

        //private List<IFeature> currentFeatures = null;//当前选择的要素

        private void 全选_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //try
            //{
            IMap pmap = axMapControl1.Map;
            plstLayers = GetLayers(pmap);

            axMapControl1.CurrentTool = null;
            for (int i = 0; i < plstLayers.Count; i++)
            {
                IFeatureLayer pFeatureLayer = (plstLayers[i]) as IFeatureLayer;
                IFeatureClass pFeaterClass = pFeatureLayer.FeatureClass;
                string where = "";
                IQueryFilter filter = new QueryFilterClass();
                filter.WhereClause = where;
                IFeatureCursor pFeatcursor = pFeaterClass.Search(filter, false);
                IFeature pFeature = pFeatcursor.NextFeature();
                while (pFeature != null)
                {
                    pFeature = pFeatcursor.NextFeature();
                    IFeatureSelection featureselection = pFeatureLayer as IFeatureSelection;
                    //axMapControl1.Map.SelectFeature(pFeatureLayer, pFeature);
                    featureselection.SelectFeatures(null, esriSelectionResultEnum.esriSelectionResultNew, false);
                }


            }

            //pMouseOperate = "SelectAllFeature";
            //PageLayout pagelayout = new PageLayout();
            //IGraphicsContainer GraphicsContainer = new ;
            //axMapControl1.Map.SelectByShape(null, null, false);
            //axMapControl1.Map.SelectFeature(pFeatureLayer, pFeature);
            axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        private void 反选_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "ReverseSelectFeature";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void 主视图刷新_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            axMapControl1.Refresh();
            EagleEyeMapControl.Refresh();
        }

        private void 鹰眼刷新_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            EagleEyeMapControl.Refresh();
        }

        #endregion

        #region 几何编辑

        #region 开始编辑
        private void 开始编辑_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                IMap pmap = axMapControl1.Map ;
                plstLayers=GetLayers(pmap);
                if (pTocFeatureLayer == null)
                {
                    MessageBox.Show("请设置活动图层！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (plstLayers == null || plstLayers.Count == 0)
                {
                    MessageBox.Show("请加载编辑图层！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                //pMap.ClearSelection();
                pActiveView = pMap as IActiveView;
                pActiveView.Refresh();
                //InitComboBox(plstLayers);
                //ChangeButtonState(true);
                pEngineEditLayers.SetTargetLayer(pTocFeatureLayer, 0);//设置要编辑的图层
                //如果编辑已经开始，则直接退出
                if (pEngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing)
                    return;
                //如果图层为空，则直接退出


                if (pTocFeatureLayer == null)
                    return;
                //获取当前编辑图层工作空间
                IDataset pDataSet = pTocFeatureLayer.FeatureClass as IDataset;
                IWorkspace pWs = pDataSet.Workspace;
                //设置编辑模式，如果是ArcSDE，则采用版本模式
                if (pWs.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
                {
                    pEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeVersioned;
                }
                else
                {
                    pEngineEditor.EditSessionMode = esriEngineEditSessionMode.esriEngineEditSessionModeNonVersioned;
                }
                //设置编辑任务
                pEngineEditTask = pEngineEditor.GetTaskByUniqueName("ControlToolsEditing_CreateNewFeatureTask");
                pEngineEditor.CurrentTask = pEngineEditTask;//设置编辑任务
                pEngineEditor.EnableUndoRedo(true);
                pEngineEditor.StartEditing(pWs, pMap);
                MapManager.EngineEditor = pEngineEditor;
                MessageBox.Show("请开始编辑");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        #endregion

        #region 新建点
        private void 新建点_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "NewPoint";
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 线操作
        private void 任意线绘制_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "NewLine";
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 面操作
        private void 面绘制_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "Newpolygon";
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 节点编辑

        private void 移动节点_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ICommand m_MoveVertexTool = new MoveFeatureToolClass();
                m_MoveVertexTool.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = m_MoveVertexTool as ITool;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void 添加节点_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ICommand m_CreateFeatTool = new CreateFeatureToolClass();
                m_CreateFeatTool.OnCreate(axMapControl1.Object);
                m_CreateFeatTool.OnClick();
                axMapControl1.CurrentTool = m_CreateFeatTool as ITool;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void 删除节点_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
                ICommand m_delFeatCom = new DelFeatureCommandClass();
                m_delFeatCom.OnCreate(axMapControl1.Object);
                m_delFeatCom.OnClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 删除
        private void 删除编辑_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
            m_delFeatCom.OnCreate(axMapControl1.Object);
            m_delFeatCom.OnClick();
        }
        #endregion

        #region 移动
        private void 移动_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "MoveFeature";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 撤销编辑
        IOperationStack operationStack = new ControlsOperationStackClass();
        private void 撤销_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (operationStack.UndoOperation != null)
                {
                    operationStack.Undo();//调用AE自带重做指令  
                }
                else
                {
                    MessageBox.Show("没有可撤消的操作！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "无可撤消操作");
            }
        }
        #endregion

        #region 恢复
        private void 恢复编辑_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (operationStack.RedoOperation == null)
                {
                    MessageBox.Show("没有可重做的操作！");
                }
                else
                {
                    operationStack.Redo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "无可重做操作");
            }
        }
        #endregion

        #endregion

        #region 操作函数

        #region 获取路径
        public string getPath(string path)
        {
            int t;
            for (t = 0; t < path.Length; t++)
            {
                if (path.Substring(t, 6) == "空间数据编辑")
                {
                    break;
                }
            }
            string name = path.Substring(0, t + 6);
            return name;
        }
        #endregion

        #endregion

        //private void 缓冲区分析ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    ICommand pCommand = new ToolBufferAnalysis();
        //    pCommand.OnCreate(this.axMapControl1.Object);
        //    this.axMapControl1.CurrentTool = pCommand as ITool;
        //    pCommand = null;
        //}

   
        private void 复制_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "FeatureCopy";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void 属性编辑_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ICommand m_AtrributeCom = new EditAtrributeToolClass();
                m_AtrributeCom.OnCreate(axMapControl1.Object);
                m_AtrributeCom.OnClick();
                axMapControl1.CurrentTool = m_AtrributeCom as ITool;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

      
        #region
        private void 点ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        #endregion

        private void OSM数据库连接_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OSMDataBaseLinkForm form = new OSMDataBaseLinkForm();
            form.ShowDialog();
        }

        private void 基态数据入库_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            OSMTODB form = new OSMTODB();
            form.ShowDialog();
        }

        private void TextElementLabel(IFeatureLayer pFeatLyr, string sFieldName)
        {
            //IMap pMap = axMapControl1.Map;
            //获得图层所有要素
            IFeatureClass pFeatureClass = pFeatLyr.FeatureClass;
            IFeatureCursor pFeatCursor = pFeatureClass.Search(null, true);
            IFeature pFeature = pFeatCursor.NextFeature();
            while (pFeature != null)
            {
                IFields pFields = pFeature.Fields;
                //找出标注字段的索引号
                int index = pFields.FindField(sFieldName);
                //得到要素的Envelope
                IEnvelope pEnve = pFeature.Extent;
                IPoint pPoint = new PointClass();
                pPoint.PutCoords(pEnve.XMin + pEnve.Width / 2, pEnve.YMin + pEnve.Height / 2);
                //新建字体对象
                stdole.IFontDisp pFont;
                pFont = new stdole.StdFontClass() as stdole.IFontDisp;
                pFont.Name = "arial";

                //产生一个文本符号
                ITextSymbol pTextSymbol = new TextSymbolClass();
                //设置文本符号的大小
                //pTextSymbol.Size = 200;
                //pTextSymbol.Font = pFont;
                //pTextSymbol.Color = m_OperateMap.GetRgbColor(255, 0, 0);
                SetParameter setParametr = new SetParameter();
                pTextSymbol = setParametr.ziti();

                //产生一个文本对象
                ITextElement pTextElement = new TextElementClass();
                pTextElement.Text = pFeature.get_Value(index).ToString();
                pTextElement.ScaleText = true;
                pTextElement.Symbol = pTextSymbol;
                IElement pElement = pTextElement as IElement;
                pElement.Geometry = pPoint;
                IActiveView pActiveView = pMap as IActiveView;
                IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
                //添加元素
                pGraphicsContainer.AddElement(pElement, 0);
                pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                pPoint = null;
                pElement = null;
                pFeature = pFeatCursor.NextFeature();
            }


        }

        private void 点击查看属性_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //Form1 form = new Form1();
            //form.ShowDialog();
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "PointReturnFeatureAttribute";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void shp数据入库_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            SHPToDataBase form = new SHPToDataBase(axMapControl1);
            form.ShowDialog();
        }

        private void 增量数据入库_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            OscDataTODB formTemp = new OscDataTODB();
            formTemp.StartPosition = FormStartPosition.CenterParent;
            formTemp.ShowDialog();
        }

        private void excel数据入库_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            ExcelToOSM eto = new ExcelToOSM();
            eto.StartPosition = FormStartPosition.CenterParent;
            eto.ShowDialog();
        }

        private void 模型转换_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            ModelTrans mt = new ModelTrans();
            mt.StartPosition = FormStartPosition.CenterParent;
            mt.ShowDialog();
        }

        private void 索引建立_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            //20180712 zh修改
            if (MainMap == null)
            {
                MessageBox.Show("尚未选择面数据！");
                return;
            }
            MainMap.CreateQuadTreeIndex();
        }

        private void 专业数据处理_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess)
            {
                //ProfessionDataProcess dataHandle = new ProfessionDataProcess();
                LineFeatureExtraction dataHandle = new LineFeatureExtraction();
                dataHandle.StartPosition = FormStartPosition.CenterParent;
                dataHandle.ShowDialog();
            }
            else
            {
                MessageBox.Show("数据库未连接，请您先连接数据库，谢谢！");
            }
        }

        private void 属性控制_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            if (OSMDataBaseLinkForm.OSMLinkSuccess)
            {
                AttributeAccuracy form = new AttributeAccuracy();
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("数据库未连接！");
            }
        }


        private void 面要素匹配更新_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess)
            {
                AreaElementUpdate areaUpdate = new AreaElementUpdate(axMapControl1,axTOCControl1);
                areaUpdate.StartPosition = FormStartPosition.CenterParent;
                areaUpdate.ShowDialog();
                //flashDisplay();
                //AddMap addmap = new AddMap(axMapControl1, axTOCControl1);
                //addmap.flashDisplay(pMap);
            }
            else
            {
                MessageBox.Show("数据库未连接！");
            }
        }
        
        private void 全球典型要素匹配更新_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
           if (OSMDataBaseLinkForm.OSMLinkSuccess)
             {
                //LineElementMatchUpdate professionnal = new LineElementMatchUpdate("",axMapControl1,axTOCControl1 );
                //professionnal.StartPosition = FormStartPosition.CenterParent;
                //professionnal.ShowDialog();
                AddMap addMap=new AddMap();
                ElementMatchUpdate professionnal = new ElementMatchUpdate("", axMapControl1, axTOCControl1);
                professionnal.StartPosition = FormStartPosition.CenterParent;
                professionnal.ShowDialog();
                addMap.FlashElement(axMapControl1);
             }
          else
            {
              MessageBox.Show("数据库未连接，请您先连接数据库，谢谢！");
            }
        }
            

        private void 快照查询_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            hist form = new hist();
             
            form.ShowDialog();
        }

        private void 增量数据处理_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            IncrementDataDispose formTemp = new IncrementDataDispose();
            formTemp.StartPosition = FormStartPosition.CenterParent;
            formTemp.ShowDialog();
        }
       

        private void 显示_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //如果还没有连数据库，那么需要提醒用户还没有连接数据库
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            string path = @"../../../testfile";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            ShowMap form = new ShowMap(OSMDataBaseLinkForm.conStringTemp,axMapControl1,axTOCControl1);//将该控件传过去，并且创建一个showmap对象
            //MapTest form = new MapTest();

            form.ShowDialog();
        }

        private void 拓扑控制_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //如果还没有连数据库，那么需要提醒用户还没有连接数据库
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            TopoControlForm form = new TopoControlForm(axMapControl1, axTOCControl1);//将该控件传过去，并且创建一个showmap对象
            //MapTest form = new MapTest();

            form.Show();
        }

       

        private void 位置改进精度_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            //位置精度
            OracleDBHelper ConHelper = new OracleDBHelper();
            PositionAccuracy form = new PositionAccuracy(axMapControl1, axTOCControl1, ConHelper);
            form.ShowDialog();
           
        }

        private void 高精度数据利用率_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            UtilizationRatio form = new UtilizationRatio();
            form.ShowDialog();
        }

        private void 影像抽检_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            ImageComparison form = new ImageComparison();
            form.ShowDialog();
           
        }

        private void 信誉度计算_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            //信誉度计算
            FindSonVersion form = new FindSonVersion();
            form.ShowDialog();
        }

        private void 历史_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            //历史数据入库
            Importhistorydata form = new Importhistorydata();
            form.ShowDialog();
           
        }

        private void 增量信息_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
            //增量信息汇总表
            IncrementalInformation form = new IncrementalInformation();
            form.ShowDialog();
        }
        private void 数据库导出_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (OSMDataBaseLinkForm.OSMLinkSuccess)//如果连接成功
            {
                OSMToShapeForm form = new OSMToShapeForm(OSMDataBaseLinkForm.conStringTemp);//把这个连接成功的参数数据传过去
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("OSM数据库尚未连接！");
            }
        }

        private void 回滚_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DateTime currentTime = System.DateTime.Now;
            MessageBox.Show(currentTime.ToString());
            if (OSMDataBaseLinkForm.OSMLinkSuccess == false)
            {
                MessageBox.Show("尚未连接数据库！");
                return;
            }
           if (RollBack.CalculateThreshold() == 0)
            {
                MessageBox.Show("请先评价数据质量！");
                return;
            }
            DialogResult result=MessageBox.Show("当前数据总体质量为" + RollBack .CalculateThreshold()+ "是否回滚？", "是否取消更新", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                RollBack.rollback("polygon", "2018-10-10 15:55:00");
                //RollBack.rollback("ARESIDENTIAL", "2018-10-10 15:55:00");
                //RollBack.rollback("ASOIL", "2018-10-10 15:55:00");
                //RollBack.rollback("ATRAFFIC", "2018-10-10 15:55:00");
                //RollBack.rollback("AVEGETATION", "2018-10-10 15:55:00");
                //RollBack.rollback("AWATER", "2018-10-10 15:55:00");
            }
            else
            {
                return;
            }

        }

        private void 唯一值符号化_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (frmUniqueValueRen == null || frmUniqueValueRen.IsDisposed)
                {
                    frmUniqueValueRen = new frmUniqueValueRender();
                    frmUniqueValueRen.UniqueValueRender += new frmUniqueValueRender.UniqueValueRenderEventHandler(frmUniqueValueRen_UniqueValueRender);
                }
                frmUniqueValueRen.Map = axMapControl1.Map;
                frmUniqueValueRen.InitUI();
                frmUniqueValueRen.ShowDialog();
            }
            catch (Exception ex)
            {

            }

            
        }


        #region 唯一值单字段

        private frmUniqueValueRender frmUniqueValueRen = null;
        void frmUniqueValueRen_UniqueValueRender(string sFeatClsName, string sFieldName)
        {
            m_OperateMap = new OperateMap();
            IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName2(axMapControl1.Map, sFeatClsName);
            UniqueValueRenderer(pFeatLyr, sFieldName);

        }
        /// <summary>
        /// 唯一值符号化
        /// </summary>
        /// <param name="pFeatLyr">渲染图层</param>
        /// <param name="sFieldName">渲染字段</param>
        private void UniqueValueRenderer(IFeatureLayer pFeatLyr, string sFieldName)
        {
            try
            {
                IGeoFeatureLayer pGeoFeatLyr = pFeatLyr as IGeoFeatureLayer;
                ITable pTable = pFeatLyr as ITable;
                IUniqueValueRenderer pUniqueValueRender = new UniqueValueRendererClass();

                int intFieldNumber = pTable.FindField(sFieldName);
                pUniqueValueRender.FieldCount = 1;//设置唯一值符号化的关键字段为一个
                pUniqueValueRender.set_Field(0, sFieldName);//设置唯一值符号化的第一个关键字段

                IRandomColorRamp pRandColorRamp = new RandomColorRampClass();
                pRandColorRamp.StartHue = 0;
                pRandColorRamp.MinValue = 0;
                pRandColorRamp.MinSaturation = 15;
                pRandColorRamp.EndHue = 360;
                pRandColorRamp.MaxValue = 100;
                pRandColorRamp.MaxSaturation = 30;
                //根据渲染字段的值的个数，设置一组随机颜色，如某一字段有5个值，则创建5个随机颜色与之匹配
                IQueryFilter pQueryFilter = new QueryFilterClass();
                pRandColorRamp.Size = pFeatLyr.FeatureClass.FeatureCount(pQueryFilter);
                bool bSuccess = false;
                pRandColorRamp.CreateRamp(out bSuccess);

                IEnumColors pEnumRamp = pRandColorRamp.Colors;
                IColor pNextUniqueColor = null;
                //查询字段的值
                pQueryFilter = new QueryFilterClass();
                pQueryFilter.AddField(sFieldName);
                ICursor pCursor = pTable.Search(pQueryFilter, true);
                IRow pNextRow = pCursor.NextRow();
                object codeValue = null;
                IRowBuffer pNextRowBuffer = null;


                while (pNextRow != null)
                {
                    pNextRowBuffer = pNextRow as IRowBuffer;
                    codeValue = pNextRowBuffer.get_Value(intFieldNumber);//获取渲染字段的每一个值

                    pNextUniqueColor = pEnumRamp.Next();
                    if (pNextUniqueColor == null)
                    {
                        pEnumRamp.Reset();
                        pNextUniqueColor = pEnumRamp.Next();
                    }
                    IFillSymbol pFillSymbol = null;
                    ILineSymbol pLineSymbol;
                    IMarkerSymbol pMarkerSymbol;
                    switch (pGeoFeatLyr.FeatureClass.ShapeType)
                    {
                        case esriGeometryType.esriGeometryPolygon:
                            {
                                pFillSymbol = new SimpleFillSymbolClass();
                                pFillSymbol.Color = pNextUniqueColor;
                                pUniqueValueRender.AddValue(codeValue.ToString(), "", pFillSymbol as ISymbol);//添加渲染字段的值和渲染样式
                                pNextRow = pCursor.NextRow();
                                break;
                            }
                        case esriGeometryType.esriGeometryPolyline:
                            {
                                pLineSymbol = new SimpleLineSymbolClass();
                                pLineSymbol.Color = pNextUniqueColor;
                                pUniqueValueRender.AddValue(codeValue.ToString(), "", pLineSymbol as ISymbol);//添加渲染字段的值和渲染样式
                                pNextRow = pCursor.NextRow();
                                break;
                            }
                        case esriGeometryType.esriGeometryPoint:
                            {
                                pMarkerSymbol = new SimpleMarkerSymbolClass();
                                pMarkerSymbol.Color = pNextUniqueColor;
                                pUniqueValueRender.AddValue(codeValue.ToString(), "", pMarkerSymbol as ISymbol);//添加渲染字段的值和渲染样式
                                pNextRow = pCursor.NextRow();
                                break;
                            }
                    }
                }
                pGeoFeatLyr.Renderer = pUniqueValueRender as IFeatureRenderer;
                axMapControl1.Refresh();
                axTOCControl1.Update();
            }
            catch (Exception ex)
            {

            }

        }
        #endregion

        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            SynchronizeEagleEye();
        }

        private void axTOCControl1_OnEndLabelEdit(object sender, ITOCControlEvents_OnEndLabelEditEvent e)
        {
            if(e.newLabel.Trim()=="")
            {
                e.canEdit = false;
            }
        }

        #region  典型要素更新部分闪烁标识 by ZBL 20181124

        //要素更新部分闪烁标识按钮点击事件
        private void UpdateFlashbBItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (axMapControl1.LayerCount > 0)
            {
                AddMap addMap=new AddMap ();
                addMap.FlashElement(axMapControl1 );
            }
            else
            {
                MessageBox.Show("OSM数据库尚未连接，请先连接数据库！");
            }
        }
        #endregion

        private void barButton增量数据整合_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //CombineOSC combineOSC = new CombineOSC();
            //combineOSC.ShowDialog();
            CombineOSC combineOSC = new CombineOSC();
            combineOSC.ShowDialog();
            //combineOSC.Combine();

        }

        private void barButton创建面_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.CurrentTool = null;
                pMouseOperate = "NewLine";
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void barButton删除线_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
                ICommand m_delFeatCom = new DelFeatureCommandClass();
                m_delFeatCom.OnCreate(axMapControl1.Object);
                m_delFeatCom.OnClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void barButton移动线节点_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ICommand m_MoveVertexTool = new MoveVertexToolClass();
                m_MoveVertexTool.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = m_MoveVertexTool as ITool;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void barButton删除面_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
                ICommand m_delFeatCom = new DelFeatureCommandClass();
                m_delFeatCom.OnCreate(axMapControl1.Object);
                m_delFeatCom.OnClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void barButton删除线节点_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ICommand m_DelVertexTool = new DelVertexToolClass();
                m_DelVertexTool.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = m_DelVertexTool as ITool;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void barButton删除面节点_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ICommand m_DelVertexTool = new DelVertexToolClass();
                m_DelVertexTool.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = m_DelVertexTool as ITool;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void barButton移动面节点_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ICommand m_MoveVertexTool = new MoveVertexToolClass();
                m_MoveVertexTool.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = m_MoveVertexTool as ITool;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void barButton移动线_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ICommand m_MoveVertexTool = new MoveFeatureToolClass();
                m_MoveVertexTool.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = m_MoveVertexTool as ITool;
                       
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void barButton移动面_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ICommand m_MoveVertexTool = new MoveFeatureToolClass();
                m_MoveVertexTool.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = m_MoveVertexTool as ITool;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Annotation注记显示
        private void 添加注记_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (AnnotationDisplay == null || AnnotationDisplay.IsDisposed)
                {
                    AnnotationDisplay = new AnnotationDisplay();
                    AnnotationDisplay.Annotation += new AnnotationDisplay.AnnotationEventHandler(AnnotationDisplay_Annotation);
                }
                AnnotationDisplay.Map = axMapControl1.Map;
                AnnotationDisplay.InitUI();
                AnnotationDisplay.ShowDialog();
            }
            catch (Exception ex)
            {
            }
        }
        //注记
        void AnnotationDisplay_Annotation(string sFeatClsName, string sFieldName)
        {
            IFeatureLayer pFeatLyr = m_OperateMap.GetFeatLyrByName(axMapControl1.Map, sFeatClsName);
            Annotation(pFeatLyr, sFieldName);
        }
        private void Annotation(IFeatureLayer pFeatLyr, string sFieldName)
        {
            try
            {
                IGeoFeatureLayer pGeoFeatLyer = pFeatLyr as IGeoFeatureLayer;
                IAnnotateLayerPropertiesCollection pAnnoProps = pGeoFeatLyer.AnnotationProperties;
                pAnnoProps.Clear();
                //设置标注记体格式                                                                     
                ITextSymbol pTextSymbol = new TextSymbolClass();
                stdole.StdFont pFont = new stdole.StdFontClass();
                pFont.Name = "verdana";
                pFont.Size = 10;
                pTextSymbol.Font = pFont as stdole.IFontDisp;
                //设置注记放置格式
                ILineLabelPosition pPosition = new LineLabelPositionClass();
                pPosition.Parallel = false;
                pPosition.Perpendicular = true;
                ILineLabelPlacementPriorities pPlacement = new LineLabelPlacementPrioritiesClass();
                IBasicOverposterLayerProperties pBasic = new BasicOverposterLayerPropertiesClass();
                pBasic.FeatureType = esriBasicOverposterFeatureType.esriOverposterPolyline;
                pBasic.LineLabelPlacementPriorities = pPlacement;//设置标注文本摆设路径权重
                pBasic.LineLabelPosition = pPosition;//控制文本的排放位置
                ILabelEngineLayerProperties pLableEngine = new LabelEngineLayerPropertiesClass();
                pLableEngine.Symbol = pTextSymbol;
                pLableEngine.BasicOverposterLayerProperties = pBasic;//设置标注文本的放置方式，以及处理文字间冲突的处理方式等
                pLableEngine.Expression = "[" + sFieldName + "]";//输入VBScript或JavaScript语言，设置要标注的字段
                IAnnotateLayerProperties pAnnoLayerProps = pLableEngine as IAnnotateLayerProperties;
                pAnnoProps.Add(pAnnoLayerProps);
                pGeoFeatLyer.DisplayAnnotation = true;
                axMapControl1.Refresh(esriViewDrawPhase.esriViewBackground, null, null);
                axMapControl1.Update();
            }
            catch (Exception ex)
            {


            }

        }
        #endregion

        private void 新增注记_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            AddText addText = new AddText(this.axMapControl1);
            addText.Show();
            //调用onmousedown事件
            axMapControl1.CurrentTool = null;
            pMouseOperate = "addText";
        }

        private void 保存编辑_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ICommand m_saveEditCom = new SaveEditCommandClass();
                m_saveEditCom.OnCreate(axMapControl1.Object);
                m_saveEditCom.OnClick();
            }
            catch (Exception ex)
            {
            }
        }

        private void 结束编辑_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (pTocFeatureLayer == null)
            {
                MessageBox.Show("请设置活动图层！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SaveEdit endedit = new SaveEdit(pTocFeatureLayer.Name, axMapControl1);
            endedit.ShowDialog();

        }

        private void barButtonItem15_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //txt增量入库
            if (OSMDataBaseLinkForm.OSMLinkSuccess)
            {
                txt2DB form = new txt2DB();
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("数据库未连接！");
            }
        }





    }
}