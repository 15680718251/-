using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System.Collections;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using Oracle.ManagedDataAccess.Client;
/*
 * 显示地图功能类
 * zh编写
 * 2018年9月19日
 */
namespace GIS.UI.AdditionalTool
{
    public class AddMap
    {
        private string tablename;//表名
        private string[] tablelist;        

        private AxMapControl axMapcontrol;
        public AxMapControl AxMapcontrol
        {
            get { return axMapcontrol; }
            set { axMapcontrol = value; }
        }
        private AxTOCControl axTOCControl;
        public AxTOCControl AxTOCControl
        {
            get { return axTOCControl; }
            set { axTOCControl = value; }
        }

        public AddMap()
        { 
            
        }
        public AddMap(AxMapControl axMapcontrol)
        {
            this.axMapcontrol = axMapcontrol;
        }
        public AddMap(AxMapControl axMapcontrol, AxTOCControl axTOCControl)
        {
            this.axMapcontrol = axMapcontrol;
            this.axTOCControl = axTOCControl;
        }
        
        //添加图层树的重载
        public bool showMap(string tablename, AxMapControl axMapcontrol, AxTOCControl axTOCControl)
        {
            if (tablename == null || tablename.Trim().Equals("")) return false;
            this.tablename = tablename;
            this.axMapcontrol = axMapcontrol;
            this.axTOCControl = axTOCControl;
            Thread t = new Thread(OSMtoShp);//新建一个线程来执行转shape操作，避免假死状态
            t.Start();//标记新线程的开始
            return true;
        }
        //批量添加图层树的重载
        public bool showMap(string[] tablelist)
        {
            if (tablelist == null) return false;
            string path = @"..\..\..\testfile\";
            List<string> list = new List<string>();
            for (int i = 0; i < tablelist.Length; i++)
            {
                list.Add(tablelist[i]);
            }
            OSMToShp2(list);//导出shp文件到文件夹
            ShowShpFile(path, list);//进行图层树显示
            ss();//进行颜色固定显示
            return true;
        }

        //批量显示图层,非树形
        public bool showMap(string[] tablelist, AxMapControl axMapcontrol)
        {
            if (tablelist == null) return false;
            this.tablelist = tablelist;
            this.axMapcontrol = axMapcontrol;
            Thread t = new Thread(OSMtoShpList);//新建一个线程来执行转shape操作，避免假死状态
            t.Start();//标记新线程的开始
            return true;
        }
        //显示图层,非树形
        public bool showMap(string tablename, AxMapControl axMapcontrol)
        {
            if (tablename == null || tablename.Trim().Equals("")) return false;
            this.tablename = tablename;
            this.axMapcontrol = axMapcontrol;
            Thread t = new Thread(OSMtoShp);//新建一个线程来执行转shape操作，避免假死状态
            t.Start();//标记新线程的开始
            return true;
        }
        


        /// <summary>
        /// 执行数据导出操作
        /// </summary>
        //导出为shp文件并加载进行图层树显示
        private void TransToShpAndShowMap(OracleDBHelper odb,string path,string tablename)
        {
            ShpHelper.GetShpFileByTableName(odb, tablename, path);//根据数据库表中数据生成shp文件，这是核心功能

            tablename = tablename + ".shp";
            //获取出库后的新文件存放的路径                
            string[] s = Directory.GetFiles(path);
            string[] filename = new string[s.Length];
            for (int j = 0; j < s.Length; j++)
            {
                filename[j] = System.IO.Path.GetFileName(s[j]);
            }
            //遍历选中的文件，与文件夹里面带shp后缀的比较，相同就输出显示
            bool sameFile = false;
            string findFileName = "";

            //找是否有相同名字的文件
            for (int j = 0; j < filename.Length; j++)
            {
                if (tablename == filename[j])
                {
                    //相同就输出显示
                    sameFile = true;
                    findFileName = filename[j];

                }
            }
            if (sameFile == true)
            {
                string findFile = findFileName.Substring(0, findFileName.IndexOf('.'));//去除文件名中.shp字符串,目的是为了根据这个名字操作后面的图层组名字
                //bool isFindSameLayerGroup = FindSameLyersByName(findFileName);//新建图层组
                if (findFile.Contains("RESIDENTIAL") || findFile.Contains("SOIL") || findFile.Contains("VEGETATION") || findFile.Contains("WATER")||findFile.Contains("TRAFFIC"))
                {//如果包含了四大要素的关键字，就准备新建图层组
                    if (!isLoadSameLayer(findFile))//如果没有加载相同的图层就加载，避免重复加载图层
                    {
                        IGroupLayer groupLayer = new GroupLayerClass();
                        string layerGroupName = "";//将英文要素名翻译成中文
                        if (!FindSameGroupLyers(findFile))//如果没有加载相同的图层组，避免重复加载图层组
                        {
                            axMapcontrol.Map.AddLayer(groupLayer);
                            //switch (findFile.Substring(1))//如果名字中包含关键字
                            //{
                            //    case "RESIDENTIAL": layerGroupName = "居民地";
                            //        break;
                            //    case "SOIL": layerGroupName = "土壤";
                            //        break;
                            //    case "VEGETATION": layerGroupName = "植被";
                            //        break;
                            //    case "WATER": layerGroupName = "水系";
                            //        break;
                            //    default:
                            //        break;
                            //}
                            groupLayer.Name = GetLayerGroupName(findFile, layerGroupName);

                            axMapcontrol.AddShapeFile(path, findFileName);//根据路径和名称添加图层
                            ILayer getLayer = GetLayersByName(findFile);//根据文件名获得图层栏中相应名字的图层
                            groupLayer.Add(getLayer);//在图层组中添加这个图层
                            //这里不用考虑是否需要通过改变其中某一个的图层名来进行删除，因为改变的话他们的图层名都会被改变
                            DeleteLyersByName(findFile); //删除单独的图层，留下图层组    
                        }
                        else//如果加载了相同的图层组，则在已有的图层组下面添加
                        {
                            IGroupLayer plGroup = GetGroupLayersByName(findFile);//通过图层名找到对应的图层组

                            axMapcontrol.AddShapeFile(path, findFileName);//根据路径和名称添加图层
                            ILayer getLayer = GetLayersByName(findFile);
                            plGroup.Add(getLayer);
                            DeleteLyersByName(findFile);
                        }

                    }
                }
                else
                {
                    axMapcontrol.AddShapeFile(path, findFileName);
                }

            }
            
        }

        private string GetLayerGroupName(string findFile, string layerGroupName)
        {
            if (findFile.Contains("RESIDENTIAL"))
            {
                layerGroupName = "居民地";
            }
            else if (findFile.Contains("SOIL"))
            {
                layerGroupName = "土壤";
            }
            else if (findFile.Contains("VEGETATION"))
            {
                layerGroupName = "植被";
            }
            else if (findFile.Contains("WATER"))
            {
                layerGroupName = "水系";
            }
            else if (findFile.Contains("TRAFFIC"))
            {
                layerGroupName = "交通";
            }
            return layerGroupName;
        }

        //根据图层名获取图层
        private ILayer GetLayersByName(string IN_Name)
        {
            IEnumLayer Temp_AllLayer = axMapcontrol.Map.Layers;
            ILayer Each_Layer = Temp_AllLayer.Next();
            while (Each_Layer != null)
            {
                if (Each_Layer.Name.Contains(IN_Name))
                    return Each_Layer;
                Each_Layer = Temp_AllLayer.Next();
            }
            return null;
        }

        //遍历判断图层中是否已经加载了相应的图层
        private bool isLoadSameLayer(string IN_Name)
        {
            ILayer pL = null;
            string name = "";
            if (axMapcontrol.LayerCount != 0)
            {
                for (int i = 0; i < axMapcontrol.LayerCount; i++)
                {
                    pL = axMapcontrol.get_Layer(i);//获取图层组和图层
                    if (pL is IGroupLayer)
                    {
                        ICompositeLayer pGroupLayer = pL as ICompositeLayer;
                        for (int j = 0; j < pGroupLayer.Count; j++)
                        {
                            name = pGroupLayer.get_Layer(j).Name;
                            if (IN_Name == name)
                                return true;
                        }
                    }
                    else//如果不是图层组
                    {
                        name = pL.Name;
                        if (IN_Name == name)
                            return true;
                    }
                }
            }
            return false;//默认没有加载相同的图层
        }

        //根据图层名删除图层
        private void DeleteLyersByName(string IN_Name)
        {
            ILayer pL = null;
            for (int i = 0; i < axMapcontrol.LayerCount; i++)
            {
                pL = axMapcontrol.get_Layer(i);
                if (pL as IGroupLayer == null)
                {
                    if (pL.Name.Contains(IN_Name))
                        axMapcontrol.Map.DeleteLayer(pL);
                }
            }
        }

        //根据图层名判断添加过相同的图层组没有
        private bool FindSameGroupLyers(string IN_Name)
        {
            //switch (IN_Name.Substring(1))//如果名字中包含关键字
            //{
            //    case "RESIDENTIAL": IN_Name = "居民地";
            //        break;
            //    case "SOIL": IN_Name = "土壤";
            //        break;
            //    case "VEGETATION": IN_Name = "植被";
            //        break;
            //    case "WATER": IN_Name = "水系";
            //        break;
            //    default:
            //        break;
            //}
            string grouplayerName = "";
            IN_Name = GetLayerGroupName(IN_Name, grouplayerName);
            if (axMapcontrol.LayerCount != 0)
            {
                for (int i = 0; i < axMapcontrol.LayerCount; i++)
                {
                    ILayer pL = axMapcontrol.get_Layer(i);//获取图层组和图层
                    if (pL is IGroupLayer)
                    {
                        if (pL.Name == IN_Name)
                        {
                            return true;
                        }
                    }
                }

            }
            return false;//默认没有加载相同的图层
        }

        //根据图层名返回图层组
        private IGroupLayer GetGroupLayersByName(string IN_Name)
        {
            ILayer pL = null;
            IGroupLayer plGroup = null;
            //switch (IN_Name.Substring(1))//如果名字中包含关键字
            //{
            //    case "RESIDENTIAL": IN_Name = "居民地";
            //        break;
            //    case "SOIL": IN_Name = "土壤";
            //        break;
            //    case "VEGETATION": IN_Name = "植被";
            //        break;
            //    case "WATER": IN_Name = "水系";
            //        break;
            //    default:
            //        break;
            //}
            string grouplayerName = "";
            IN_Name = GetLayerGroupName(IN_Name, grouplayerName);
            if (axMapcontrol.LayerCount != 0)
            {
                for (int i = 0; i < axMapcontrol.LayerCount; i++)
                {
                    pL = axMapcontrol.get_Layer(i);//获取图层组和图层
                    if (pL is IGroupLayer)
                    {
                        if (pL.Name == IN_Name)
                        {

                            plGroup = pL as IGroupLayer;
                            return plGroup;
                        }
                    }
                }

            }
            return null;
        }

        //优化OSMtoShp
        private void OSMtoShpPlus()
        {
            Control.CheckForIllegalCrossThreadCalls = false;//不对错误线程进行调用
            LoadTableNames(); //装载英文名对应的中文名                  
            string path = @"..\..\..\testfile";//获取文件路径
            //添加by DY
            string[] s1 = Directory.GetFiles(path);
            OracleDBHelper odb = new OracleDBHelper();//新建连接
            TransToShpAndShowMap(odb,path,tablename);
            axTOCControl.Update();//更新图层栏
        }

        private void OSMtoShp()
        {
            Control.CheckForIllegalCrossThreadCalls = false;//不对错误线程进行调用
            LoadTableNames(); //装载英文名对应的中文名                  
            string path = @"..\..\..\testfile";//获取文件路径
            //添加by DY
            string[] s1 = Directory.GetFiles(path);
            OracleDBHelper odb = new OracleDBHelper();//新建连接
            ShpHelper.GetShpFileByTableName(odb, tablename, path);//根据数据库表中数据生成shp文件，这是核心功能

            tablename = tablename + ".shp";
            //获取出库后的新文件存放的路径                
            string[] s = Directory.GetFiles(path);
            string[] filename = new string[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                filename[i] = System.IO.Path.GetFileName(s[i]);
            }
            //遍历选中的文件，与文件夹里面带shp后缀的比较，相同就输出显示
            bool sameFile = false;
            string findFileName = "";

            //找是否有相同名字的文件
            for (int j = 0; j < filename.Length; j++)
            {
                if (tablename == filename[j])
                {
                    //相同就输出显示
                    sameFile = true;
                    findFileName = filename[j];

                }
            }
            if (sameFile == true)
            {
                IWorkspaceFactory pwf = new ShapefileWorkspaceFactory();
                //关闭资源锁定  
                IWorkspaceFactoryLockControl ipWsFactoryLock = (IWorkspaceFactoryLockControl)pwf;
                if (ipWsFactoryLock.SchemaLockingEnabled)
                {
                    ipWsFactoryLock.DisableSchemaLocking();
                }
                axMapcontrol.AddShapeFile(path, findFileName);
            }

        }

        //优化OSMtoShpList
        private void OSMtoShpListPlus()
        { 
            Control.CheckForIllegalCrossThreadCalls = false;//不对错误线程进行调用
            LoadTableNames(); //装载英文名对应的中文名                  
            string path = @"..\..\..\testfile";//获取文件路径
            //添加by DY
            string[] s1 = Directory.GetFiles(path);
            OracleDBHelper odb = new OracleDBHelper();//新建连接
            for (int i = 0; i < tablelist.Length; i++)
            {
                tablename = tablelist[i];
                TransToShpAndShowMap(odb, path, tablename);
                
            }
            axTOCControl.Update();
        }

        private void OSMtoShpList()
        {
            Control.CheckForIllegalCrossThreadCalls = false;//不对错误线程进行调用
            LoadTableNames(); //装载英文名对应的中文名                  
            string path = @"..\..\..\testfile";//获取文件路径
            //添加by DY
            string[] s1 = Directory.GetFiles(path);
            OracleDBHelper odb = new OracleDBHelper();//新建连接
            for (int i = 0; i < tablelist.Length; i++)
            {
                tablename = tablelist[i];
                ShpHelper.GetShpFileByTableName(odb, tablename, path);//根据数据库表中数据生成shp文件，这是核心功能

                tablename = tablename + ".shp";
                //获取出库后的新文件存放的路径                
                string[] s = Directory.GetFiles(path);
                string[] filename = new string[s.Length];
                for (int j = 0; j < s.Length; j++)
                {
                    filename[j] = System.IO.Path.GetFileName(s[j]);
                }
                //遍历选中的文件，与文件夹里面带shp后缀的比较，相同就输出显示
                bool sameFile = false;
                string findFileName = "";

                //找是否有相同名字的文件
                for (int j = 0; j < filename.Length; j++)
                {
                    if (tablename == filename[j])
                    {
                        //相同就输出显示
                        sameFile = true;
                        findFileName = filename[j];

                    }
                }
                if (sameFile == true)
                {
                    IWorkspaceFactory pwf = new ShapefileWorkspaceFactory();
                    //关闭资源锁定  
                    IWorkspaceFactoryLockControl ipWsFactoryLock = (IWorkspaceFactoryLockControl)pwf;
                    if (ipWsFactoryLock.SchemaLockingEnabled)
                    {
                        ipWsFactoryLock.DisableSchemaLocking();
                    }
                    axMapcontrol.AddShapeFile(path, findFileName);
                }
            }

        }


        public Dictionary<string, string> tableNames = new Dictionary<string, string>();//新建一个字典集合

        public void LoadTableNames()
        {
            this.tableNames = new Dictionary<string, string>();//新建一个字典集合
            string[] enLayerName = { "WATP", "RESP", "TRAP", "PIPP", "BOUP", "TERP", "WATL", "RESL", "TRAL", "PIPL", "BOUL", "BOUNATL", "TERL", "VEGL", "WATA", "RESA", "BOUA", "BOUNATA", "TERA", "VEGA" };
            string[] chLayerName = { "(水系)", "(居民地及设施)", "(交通)", "(管线)", "(行政境界)", "(地貌)", "(水系)", "(居民地及设施)", "(交通)", "(管线)", "(行政境界)", "(区域境界)", "(地貌)", "(植被与土质)", "(水系)", "(居民地及设施)", "(行政境界)", "(区域境界)", "(地貌)", "(植被与土质)" };
            for (int i = 0; i < enLayerName.Length; i++)
            {
                tableNames.Add(enLayerName[i], chLayerName[i]);
                tableNames.Add("OSC_" + enLayerName[i], chLayerName[i]);
                tableNames.Add("NEW_" + enLayerName[i], chLayerName[i]);
            }
        }

        #region 闪烁查询要素接口 by YG 20181125
        /// <summary>
        /// 对应tables中文名称查找英文名称
        /// </summary>
        /// <summary>
        /// 闪烁查询要素
        /// </summary>
        /// <param name="axMapControl">查询结果显示在axMapControl上</param>
        /// <param name="key">根据key值进行查询</param>
        /// <param name="Operator">查询运算符</param>
        /// <param name="value"> 属性string值</param>
        public void Flash(AxMapControl axmapcontrol, string key, string Operator, string value)
        {
            value = string.Format("'{0}'", value);
            IFeatureLayer pFeatureLayer = axmapcontrol.get_Layer(0) as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            IQueryFilter pQFilter = new QueryFilterClass();
            pQFilter.WhereClause = key + Operator + value;
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(pQFilter, false);
            IFeature pFeature = pFeatureCursor.NextFeature();
            IArray pArray = new ArrayClass();
            while (pFeature != null)
            {
                pArray.Add(pFeature.ShapeCopy);
                pFeature = pFeatureCursor.NextFeature();
            }
            HookHelperClass pHook = new HookHelperClass();
            pHook.Hook = axmapcontrol.Object;
            IHookActions pHookActions = pHook as IHookActions;
            pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsPan);
            Application.DoEvents();
            pHook.ActiveView.ScreenDisplay.UpdateWindow();
            for (int i = 0; i < 5; i++)
            {
                pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsFlash);
                System.Threading.Thread.Sleep(300);
            }
        }
        public void Flash(AxMapControl axmapcontrol, IFeatureLayer pFLayer, string key, string Operator, string value)
        {
            value = string.Format("'{0}'", value);
            //pFLayer = axmapcontrol.get_Layer(0) as IFeatureLayer;
            IFeatureClass pFeatureClass = pFLayer.FeatureClass;
            IQueryFilter pQFilter = new QueryFilterClass();
            pQFilter.WhereClause = key + Operator + value;
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(pQFilter, false);
            IFeature pFeature = pFeatureCursor.NextFeature();
            IArray pArray = new ArrayClass();
            while (pFeature != null)
            {
                pArray.Add(pFeature.ShapeCopy);
                pFeature = pFeatureCursor.NextFeature();
            }
            HookHelperClass pHook = new HookHelperClass();
            pHook.Hook = axmapcontrol.Object;
            IHookActions pHookActions = pHook as IHookActions;
            pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsPan);
            Application.DoEvents();
            pHook.ActiveView.ScreenDisplay.UpdateWindow();
            for (int i = 0; i < 5; i++)
            {
                pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsFlash);
                System.Threading.Thread.Sleep(300);
            }
        }

        public void Flash(AxMapControl axmapcontrol, string key, string Operator, ArrayList value)
        {
            string value0 = string.Format("'{0}'", (string)value[0]);
            string sql = key + Operator + value0;
            IFeatureLayer pFeatureLayer = axmapcontrol.get_Layer(0) as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            IQueryFilter pQFilter = new QueryFilterClass();
            for (int i = 1; i < value.Count; i++)
            {
                value0 = string.Format("'{0}'", (string)value[i]);
                sql = sql + " or " + (key + Operator + value0);
            }
            pQFilter.WhereClause = sql;
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(pQFilter, false);
            IFeature pFeature = pFeatureCursor.NextFeature();
            IArray pArray = new ArrayClass();
            while (pFeature != null)
            {
                pArray.Add(pFeature.ShapeCopy);
                pFeature = pFeatureCursor.NextFeature();
            }
            HookHelperClass pHook = new HookHelperClass();
            pHook.Hook = axmapcontrol.Object;
            IHookActions pHookActions = pHook as IHookActions;
            pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsPan);
            Application.DoEvents();
            pHook.ActiveView.ScreenDisplay.UpdateWindow();
            for (int i = 0; i < 5; i++)
            {
                pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsFlash);
                System.Threading.Thread.Sleep(300);
            }
        }

        public void Flash(AxMapControl axmapcontrol, IFeatureLayer pFeatureLayer, string key, string Operator, ArrayList value)
        {
            string value0 = string.Format("'{0}'", (string)value[0]);
            string sql = key + Operator + value0;
            //IFeatureLayer pFeatureLayer = axmapcontrol.get_Layer(0) as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            IQueryFilter pQFilter = new QueryFilterClass();
            for (int i = 1; i < value.Count; i++)
            {
                value0 = string.Format("'{0}'", (string)value[i]);
                sql = sql + " or " + (key + Operator + value0);
            }
            pQFilter.WhereClause = sql;
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(pQFilter, false);
            IFeature pFeature = pFeatureCursor.NextFeature();
            IArray pArray = new ArrayClass();
            while (pFeature != null)
            {
                pArray.Add(pFeature.ShapeCopy);
                pFeature = pFeatureCursor.NextFeature();
            }
            HookHelperClass pHook = new HookHelperClass();
            pHook.Hook = axmapcontrol.Object;
            IHookActions pHookActions = pHook as IHookActions;
            pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsPan);
            Application.DoEvents();
            pHook.ActiveView.ScreenDisplay.UpdateWindow();
            for (int i = 0; i < 10; i++)
            {
                pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsFlash);
                System.Threading.Thread.Sleep(300);
            }
        }
        public void Flash(AxMapControl axmapcontrol, string key, string Operator, string value, string LayerName)
        {
            IMap pmap = axmapcontrol.Map;
            int index = 0;
            for (int i = 0; i < pmap.LayerCount; i++)
            {
                if (pmap.get_Layer(i).Name == LayerName)
                {
                    index = i;
                    break;
                }
            }
            value = string.Format("'{0}'", value);
            IFeatureLayer pFeatureLayer = axmapcontrol.get_Layer(index) as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            IQueryFilter pQFilter = new QueryFilterClass();
            pQFilter.WhereClause = key + Operator + value;
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(pQFilter, false);
            IFeature pFeature = pFeatureCursor.NextFeature();
            IArray pArray = new ArrayClass();
            while (pFeature != null)
            {
                pArray.Add(pFeature.ShapeCopy);
                pFeature = pFeatureCursor.NextFeature();
            }
            HookHelperClass pHook = new HookHelperClass();
            pHook.Hook = axmapcontrol.Object;
            IHookActions pHookActions = pHook as IHookActions;
            pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsPan);
            Application.DoEvents();
            pHook.ActiveView.ScreenDisplay.UpdateWindow();
            for (int i = 0; i < 5; i++)
            {
                pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsFlash);
                System.Threading.Thread.Sleep(300);
            }
        }

        ///// <summary>
        ///// 闪烁显示接口 by dy20181126
        ///// </summary>
        ///// <param name="sql"></param>
        ///// <param name="IN_LayerName"></param>
        //public  void FlashShow(string sql, string IN_LayerName)
        //{
        //    IMap pMap = axMapcontrol.Map;
        //    //ILayer pLayer = null;
        //    int index = 0;
        //    for (int i = 0; i < pMap.LayerCount; i++)
        //    {
        //        if (pMap.get_Layer(i).Name == IN_LayerName)
        //        //pLayer = pMap.get_Layer(i);
        //        {
        //            index = i;
        //            break;//找到所需图层的索引即跳出循环
        //        }
        //    }
        //    string time1 = string.Format("'{0}'", sql);
        //    IFeatureLayer pFeatureLayer = this.axMapcontrol.Map.get_Layer(index) as IFeatureLayer;
        //    IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
        //    IQueryFilter pQFilter = new QueryFilterClass();
        //    //pQFilter.WhereClause = time1 + "\"UPDATESTATE\"" + " = " + "'3'" + " " + "OR" + " " + "\"UPDATESTATE\"" + " = " + "'2'" + " " + "OR" + " " + "\"UPDATESTATE\"" + " = " + "'1'";
        //    pQFilter.WhereClause = "UPDATESTATE" + " = " + "'3'" + " " + " OR " + " " + "UPDATESTATE" + " = " + "'2'" + " " + " OR " + " " + "UPDATESTATE" + " = " + "'1'"; 
        //    //pQFilter.WhereClause = time1;
        //    IFeatureCursor pFeatureCursor = pFeatureClass.Search(pQFilter, false);
        //    IFeature pFeature = pFeatureCursor.NextFeature();
        //    IArray pArray = new ArrayClass();
        //    while (pFeature != null)
        //    {
        //        pArray.Add(pFeature.ShapeCopy);
        //        pFeature = pFeatureCursor.NextFeature();
        //    }
        //    HookHelperClass pHook = new HookHelperClass();
        //    pHook.Hook = axMapcontrol.Object;
        //    IHookActions pHookActions = pHook as IHookActions;
        //    pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsPan);
        //    Application.DoEvents();
        //    pHook.ActiveView.ScreenDisplay.UpdateWindow();
        //    for (int j = 0; j < 30; j++)
        //    {
        //        pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsFlash);
        //        System.Threading.Thread.Sleep(300);
        //    }
        //}
        /// <summary>
        /// 闪烁显示接口 by dy20181127
        /// </summary>
        //public void flashDisplay(IMap pMap)
        //{
        //     pMap = axMapcontrol.Map;
        //     List<string> layerNames = new List<string>();
        //     OperateMap player = new OperateMap();
        //     ArrayList countlist =new ArrayList() { "1","2","3"};
        //    //AddMap addmap = new AddMap();
        //    for (int i = 0; i < pMap.LayerCount; i++)
        //    {
        //        string namelist = this.axMapcontrol.Map.get_Layer(i).Name;
        //        layerNames.Add(namelist);
        //    }
        //    if (layerNames.Count != 0)
        //    {
        //        for (int j = 0; j < layerNames.Count; j++)
        //        {
        //            string sql = string.Format("select * from {0} where ", layerNames[j]);
        //            IFeatureLayer layer = player.GetFeatLyrByName(axMapcontrol.Map, layerNames[j]);
        //            Flash(axMapcontrol, layer, "'UPDATESTATE'", "=", countlist);
        //            //FlashShow(sql, layerNames[j]);
        //            //FlashLine(sql, layerNames[j]);
        //        }
        //    }
        //    else { MessageBox.Show("OSM数据库尚未连接，请先连接数据库！"); }
        //}
        #endregion

        #region 闪烁显示接口 by zbl 20181126

        //得到更新后updatestate不为null的数据的objectid值
        public ArrayList getUpdateAfterObjectId(string tableName)
        {
            ArrayList objectid = new ArrayList();
            OracleDBHelper helper = new OracleDBHelper();
            string sql = string.Format("select objectid from {0} where updatestate is not NULL", tableName);
            using (OracleDataReader dr = helper.queryReader(sql))
            {
                while (dr.Read())
                {
                    objectid.Add(dr[0].ToString());
                }
            }
            return objectid;

        }

        //闪烁显示主方法
        public void FlashLine(string sql, string IN_LayerName, AxMapControl axMapcontrol,ArrayList value)
        {
            IMap pMap = axMapcontrol.Map;
            ILayer pFeaturelayer = null;
            IFeatureClass pFeatureClass = null;
            int index = 0;
            for (int i = 0; i < axMapcontrol.LayerCount; i++)
            {
                pFeaturelayer = axMapcontrol.get_Layer(i);//获取图层组和图层
                if (pFeaturelayer is IGroupLayer)//判断是不是图层组
                {
                    ICompositeLayer pGroupLayer = pFeaturelayer as ICompositeLayer;//将图层组转换为子图层
                    for (int j = 0; j < pGroupLayer.Count; j++)
                    {
                        IGeoFeatureLayer pGeoFeatLyr = pGroupLayer.get_Layer(j) as IGeoFeatureLayer;
                        if (pGroupLayer.get_Layer(j).Name == IN_LayerName)
                        {
                            index = j;
                            IFeatureLayer pFeatureLayer = pGroupLayer.get_Layer(index) as IFeatureLayer;
                            pFeatureClass = pFeatureLayer.FeatureClass;
                            break;//找到所需图层的索引即跳出循环
                        }
                        continue;
                    }
                }
                else
                {
                    IGeoFeatureLayer pGeoFeatLyr = axMapcontrol.get_Layer(i) as IGeoFeatureLayer;//将图层转换为几何图层
                    if (pMap.get_Layer(i).Name == IN_LayerName)
                    {
                        index = i;
                        IFeatureLayer pFeatureLayer = axMapcontrol.Map.get_Layer(index) as IFeatureLayer;
                        pFeatureClass = pFeatureLayer.FeatureClass;
                        break;//找到所需图层的索引即跳出循环
                    }
                }
                continue;
            }

            string value0 = string.Format("'{0}'", (string)value[0]);
            sql = "objectid" + "=" + value0;
            string time1 = string.Format("'{0}'", sql);
            IQueryFilter pQFilter = new QueryFilterClass();
            //pQFilter.WhereClause = time1 + "'UPDATESTATE'" + " = " + "'3'" + " " + "OR" + " " + "'UPDATESTATE'" + " = " + "'2'" + " " + "OR" + " " + "'UPDATESTATE'" + " = " + "'1'";
            for (int i = 1; i < value.Count; i++)
            {
                value0 = string.Format("'{0}'", (string)value[i]);
                sql = sql + " or " + ("objectid" + "=" + value0);
            }
            pQFilter.WhereClause = sql;
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(pQFilter, false);
            IFeature pFeature = pFeatureCursor.NextFeature();
            IArray pArray = new ArrayClass();
            while (pFeature != null)
            {
                pArray.Add(pFeature.ShapeCopy);
                pFeature = pFeatureCursor.NextFeature();
            }
            HookHelperClass pHook = new HookHelperClass();
            pHook.Hook = axMapcontrol.Object;
            IHookActions pHookActions = pHook as IHookActions;
            pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsPan);
            Application.DoEvents();
            pHook.ActiveView.ScreenDisplay.UpdateWindow();
            for (int j = 0; j < 5; j++)
            {
                pHookActions.DoActionOnMultiple(pArray, esriHookActions.esriHookActionsFlash);
                System.Threading.Thread.Sleep(300);
            }
        }
        //图层树形加载显示的更新要素闪烁方法调用接口
        public void FlashElement(AxMapControl axMapcontrol)
        {
            ILayer pFeaturelayer = null;
            List<string> layerNames = new List<string>();
            ArrayList countlist = new ArrayList();
            
            for (int i = 0; i < axMapcontrol.LayerCount; i++)
            {
                pFeaturelayer = axMapcontrol.get_Layer(i);//获取图层组和图层
                if (pFeaturelayer is IGroupLayer)//判断是不是图层组
                {
                    ICompositeLayer pGroupLayer = pFeaturelayer as ICompositeLayer;//将图层组转换为子图层
                    for (int j = 0; j < pGroupLayer.Count; j++)
                    {
                        IGeoFeatureLayer pGeoFeatLyr = pGroupLayer.get_Layer(j) as IGeoFeatureLayer;//将子图层转换为几何图层
                        string namelist = pGroupLayer.get_Layer(j).Name;
                        layerNames.Add(namelist);

                    }
                    continue;
                }
                else
                {   //不是图层组的加载显示
                    IGeoFeatureLayer pGeoFeatLyr = axMapcontrol.get_Layer(i) as IGeoFeatureLayer;//将图层转换为几何图层
                    string namelist = axMapcontrol.get_Layer(i).Name;
                    layerNames.Add(namelist);
                }
            }
            if (layerNames.Count != 0)
            {
                for (int m = 0; m < layerNames.Count; m++)
                {
                    string sql1 = string.Format("select * from {0} where ", layerNames[m]);
                    countlist = getUpdateAfterObjectId(layerNames[m]);
                    if (countlist.Count > 0)
                    {
                        OperateMap player = new OperateMap();
                        IFeatureLayer pFeatLyr = player.GetFeatLyrByName(axMapcontrol.Map, layerNames[m]);
                        showByAttibute(pFeatLyr, "objectid", "=", countlist, 255, 0, 0);
                        FlashLine(sql1, layerNames[m], axMapcontrol, countlist);
                    }
                    else
                    { continue; }
                }
            }
            else
            {
                MessageBox.Show("数据库尚未连接或未加载图层，请先连接数据库后加载图层！");
            }
           
        }
        #endregion



        #region 高亮显示查询要素接口 by DHF
        /// <summary>
        /// 高亮显示
        /// </summary>
        /// <param name="axMapControl">查询结果显示在axMapControl上</param>
        /// <param name="key">根据key值进行查询</param>
        /// <param name="Operator">查询运算符</param>
        /// <param name="value"> 属性string值</param>
        public void showByAttibute(AxMapControl axMapControl, string key, string Operator, string value)
        {
            axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            IQueryFilter pQuery = new QueryFilterClass();
            value = string.Format("'{0}'", value);
            //col = string.Format("\"{0}\"", this.dataGridAttribute.Columns[7].Name);
            //val = string.Format("'{0}'", this.dataGridAttribute.CurrentCell.OwningRow.Cells[7].Value.ToString());
            ILayer pLayer = axMapControl.get_Layer(0); //pLayer 已经获取到
            Console.WriteLine(pLayer.GetType());
            IFeatureLayer pFLayer = pLayer as IFeatureLayer; //转换失败 pFLayer为空

            //int index=pFLayer.FeatureClass.Fields.FindField("OSMID");

            //string startTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(index));
            //string endTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(8));
            //设置高亮要素的查询条件
            pQuery.WhereClause = key + Operator + value;
            //+"and" + time + "<" + endTime;
            //"Name" = '行政区E'
            //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
            if (pFLayer == null) return;
            IFeatureSelection pFeatSelection;

            pFeatSelection = pFLayer as IFeatureSelection;
            pFeatSelection.Clear();
            pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);

            axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }
        //默认高亮显示
        public void showByAttibute(IFeatureLayer pFLayer, string key, string Operator, string value)
        {
            IQueryFilter pQuery = new QueryFilterClass();
            value = string.Format("'{0}'", value);

            //int index=pFLayer.FeatureClass.Fields.FindField("OSMID");

            //string startTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(index));
            //string endTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(8));
            //设置高亮要素的查询条件
            pQuery.WhereClause = key + Operator + value;
            //+"and" + time + "<" + endTime;
            //"Name" = '行政区E'
            //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
            if (pFLayer == null) return;
            IFeatureSelection pFeatSelection;

            pFeatSelection = pFLayer as IFeatureSelection;
            pFeatSelection.Clear();
            pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);

            //axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }
        //自定义颜色高亮显示
        public void showByAttibute(IFeatureLayer pFLayer, string key, string Operator, string value,int R,int G,int B)
        {
            IQueryFilter pQuery = new QueryFilterClass();
            value = string.Format("'{0}'", value);

            //int index=pFLayer.FeatureClass.Fields.FindField("OSMID");

            //string startTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(index));
            //string endTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(8));
            //设置高亮要素的查询条件
            pQuery.WhereClause = key + Operator + value;
            //+"and" + time + "<" + endTime;
            //"Name" = '行政区E'
            //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
            if (pFLayer == null) return;
            IFeatureSelection pFeatSelection;

            pFeatSelection = pFLayer as IFeatureSelection;
            pFeatSelection.Clear();
            pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);

            //将选中的要素设置为自定义色
            ESRI.ArcGIS.Display.IRgbColor pRgbColor = new RgbColor();
            pRgbColor.Red = R;
            pRgbColor.Green = G;
            pRgbColor.Blue = B;
            pFeatSelection.SelectionColor = pRgbColor;

            //axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        /// <summary>
        /// 高亮显示
        /// </summary>
        /// <param name="axMapControl">查询结果显示在axMapControl上</param>
        /// <param name="key">根据key值进行查询</param>
        /// <param name="Operator">查询运算符</param>
        /// <param name="value"> 属性int单值值</param>
        public void showByAttibute(AxMapControl axMapControl, string key, string Operator, int value)
        {
            axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            IQueryFilter pQuery = new QueryFilterClass();
            //value = string.Format("'{0}'", value);
            //col = string.Format("\"{0}\"", this.dataGridAttribute.Columns[7].Name);
            //val = string.Format("'{0}'", this.dataGridAttribute.CurrentCell.OwningRow.Cells[7].Value.ToString());
            ILayer pLayer = axMapControl.get_Layer(0); //pLayer 已经获取到
            Console.WriteLine(pLayer.GetType());
            IFeatureLayer pFLayer = pLayer as IFeatureLayer; //转换失败 pFLayer为空

            //int index=pFLayer.FeatureClass.Fields.FindField("OSMID");

            //string startTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(index));
            //string endTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(8));
            //设置高亮要素的查询条件
            pQuery.WhereClause = key + Operator + value;
            //+"and" + time + "<" + endTime;
            //"Name" = '行政区E'
            //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
            if (pFLayer == null) return;
            IFeatureSelection pFeatSelection;

            pFeatSelection = pFLayer as IFeatureSelection;
            pFeatSelection.Clear();
            pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);

            axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }
        /// <summary>
        /// 高亮显示
        /// </summary>
        /// <param name="axMapControl">查询结果显示在axMapControl上</param>
        /// <param name="key">根据key值进行查询</param>
        /// <param name="Operator">查询运算符</param>
        /// <param name="value"> 属性数组值</param>
        public void showByAttibute(AxMapControl axMapControl, string key, string Operator, ArrayList value)
        {
            axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            IQueryFilter pQuery = new QueryFilterClass();
           
            ILayer pLayer = axMapControl.get_Layer(0); //pLayer 已经获取到
            Console.WriteLine(pLayer.GetType());
            IFeatureLayer pFLayer = pLayer as IFeatureLayer; //转换失败 pFLayer为空

            //设置高亮要素的查询条件
            string value0 = string.Format("'{0}'", (string)value[0]);
            string sql = key + Operator + value0;
            for (int i = 1; i < value.Count; i++)
            {
                value0 = string.Format("'{0}'", (string)value[i]);
                sql = sql + " or " + (key + Operator + value0);
            }

            pQuery.WhereClause = sql;
            //+"and" + time + "<" + endTime;
            //"Name" = '行政区E'
            //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
            if (pFLayer == null) return;
            IFeatureSelection pFeatSelection;

            pFeatSelection = pFLayer as IFeatureSelection;
            pFeatSelection.Clear();
            pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);

            axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        //高亮显示默认方式
        public void showByAttibute(IFeatureLayer pFLayer, string key, string Operator, ArrayList value)
        {
            if (value.Count == 0) return;
            IQueryFilter pQuery = new QueryFilterClass();

            //int index=pFLayer.FeatureClass.Fields.FindField("OSMID");

            //string startTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(index));
            //string endTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(8));
            //设置高亮要素的查询条件
            string value0 = string.Format("'{0}'", (string)value[0]);
            string sql = key + Operator + value0;
            for (int i = 1; i < value.Count; i++)
            {
                value0 = string.Format("'{0}'", (string)value[i]);
                sql = sql + " or " + (key + Operator + value0);
            }

            pQuery.WhereClause = sql;
            //+"and" + time + "<" + endTime;
            //"Name" = '行政区E'
            //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
            if (pFLayer == null) return;
            IFeatureSelection pFeatSelection;

            pFeatSelection = pFLayer as IFeatureSelection;
            pFeatSelection.Clear();
            pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);

            //axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        //自定义颜色高亮显示
        public void showByAttibute(IFeatureLayer pFLayer, string key, string Operator, ArrayList value,int R,int G,int B)
        {
            if (value.Count == 0) return;
            for (int i = 0; i < 3; i++)
            {
                if (pFLayer == null) Thread.Sleep(5*1000);
            }
            if (pFLayer == null) return;
            IQueryFilter pQuery = new QueryFilterClass();

            //int index=pFLayer.FeatureClass.Fields.FindField("OSMID");

            //string startTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(index));
            //string endTime = string.Format("'{0}'", pFLayer.FeatureClass.Fields.get_Field(8));
            //设置高亮要素的查询条件
            string value0 = string.Format("'{0}'", (string)value[0]);
            string sql = key + Operator + value0;
            for (int i = 1; i < value.Count; i++)
            {
                value0 = string.Format("'{0}'", (string)value[i]);
                sql = sql + " or " + (key + Operator + value0);
            }

            pQuery.WhereClause = sql;
            //+"and" + time + "<" + endTime;
            //"Name" = '行政区E'
            //IFeatureLayer pFeatureLayer = _curFeatureLayer as IFeatureLayer;
            if (pFLayer == null) return;
            IFeatureSelection pFeatSelection;

            pFeatSelection = pFLayer as IFeatureSelection;
            pFeatSelection.Clear();
            pFeatSelection.SelectFeatures(pQuery, esriSelectionResultEnum.esriSelectionResultNew, false);

            //将选中的要素设置为自定义色
            ESRI.ArcGIS.Display.IRgbColor pRgbColor = new RgbColor();
            pRgbColor.Red = R;
            pRgbColor.Green = G;
            pRgbColor.Blue = B;
            pFeatSelection.SelectionColor = pRgbColor;

            //axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }
        #endregion

        //进行固定颜色显示的接口
        public void ss()
        {
            ILayer pFeaturelayer = null;
            for (int i = 0; i < axMapcontrol.LayerCount; i++)
            {
                pFeaturelayer = axMapcontrol.get_Layer(i);//获取图层组和图层
                if (pFeaturelayer is IGroupLayer)//判断是不是图层组
                {
                    ICompositeLayer pGroupLayer = pFeaturelayer as ICompositeLayer;//将图层组转换为子图层
                    for (int j = 0; j < pGroupLayer.Count; j++)
                    {                       
                        IGeoFeatureLayer pGeoFeatLyr = pGroupLayer.get_Layer(j) as IGeoFeatureLayer;//将子图层转换为几何图层
                        //判断图层对象的类型，按照点线面来分
                        switch (pGeoFeatLyr.FeatureClass.ShapeType)
                        {
                            case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryLine: LineReFill(pGeoFeatLyr);
                                break;                          
                            case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                                break;
                            case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon: PolygonReFill(pGeoFeatLyr);
                                break;
                            case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline: LineReFill(pGeoFeatLyr);
                                break;                            
                            default:
                                break;
                        }
                        
                    }
                }
                else
                {
                    IGeoFeatureLayer pGeoFeatLyr = axMapcontrol.get_Layer(i) as IGeoFeatureLayer;//将图层转换为几何图层
                    //判断图层对象的类型，按照点线面来分
                    switch (pGeoFeatLyr.FeatureClass.ShapeType)
                    {
                        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryLine: LineReFill(pGeoFeatLyr);
                            break;
                        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                            break;
                        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon: PolygonReFill(pGeoFeatLyr);
                            break;
                        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline: LineReFill(pGeoFeatLyr);
                            break;
                        default:
                            break;
                    }           
                }
            }

           
            axMapcontrol.Extent=axMapcontrol.FullExtent;
            axMapcontrol.Refresh();
            axTOCControl.Update();

        }

        #region 固定点线面要素的显示颜色方法函数 by ZYH
        //固定面的显示颜色
        public void PolygonReFill(IGeoFeatureLayer pGeoFeatLyr)
        {
            List<IRgbColor> result = ReadColorFile();
            //设置面填充符号           
            ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
            if (pGeoFeatLyr.Name.Contains("RESIDENTIAL"))
            {
                pSimpleFillSymbol.Color = result[0];
            
            }
            else if (pGeoFeatLyr.Name.Contains("SOIL"))
            {
                pSimpleFillSymbol.Color = result[3];
            }
            else if (pGeoFeatLyr.Name.Contains("VEGETATION"))
            {
                pSimpleFillSymbol.Color = result[2];
            }
            else if (pGeoFeatLyr.Name.Contains("WATER"))
            {
                pSimpleFillSymbol.Color = result[4];
            }
            else if (pGeoFeatLyr.Name.Contains("TRAFFIC"))
            {
                pSimpleFillSymbol.Color = result[1];
            }
            else
            {
                Random r = new Random();
                int r1=r.Next(0, 256);
                int r2 = r.Next(0, 256);
                int r3 = r.Next(0, 256);
                pSimpleFillSymbol.Color = GetRgbColor(r1, r2, r3);
            }

            //更改符号样式
            ISimpleRenderer pSimpleRenderer = new SimpleRendererClass();
            pSimpleRenderer.Symbol = pSimpleFillSymbol as ISymbol;
            pGeoFeatLyr.Renderer = pSimpleRenderer as IFeatureRenderer;
        }

        //固定线的显示颜色
        public void LineReFill(IGeoFeatureLayer pGeoFeatLyr)
        {
            List<IRgbColor> result = ReadColorFile();
            //设置面填充符号           
            ISimpleLineSymbol pSimpleLineSymbol = new SimpleLineSymbolClass();
            if (pGeoFeatLyr.Name.Contains("RESIDENTIAL"))
            {
                pSimpleLineSymbol.Color = result[5];

            }
            else if (pGeoFeatLyr.Name.Contains("SOIL"))
            {
                pSimpleLineSymbol.Color = result[8];
            }
            else if (pGeoFeatLyr.Name.Contains("VEGETATION"))
            {
                pSimpleLineSymbol.Color = result[7];
            }
            else if (pGeoFeatLyr.Name.Contains("WATER"))
            {
                pSimpleLineSymbol.Color = result[9];
            }
            else if (pGeoFeatLyr.Name.Contains("TRAFFIC"))
            {
                pSimpleLineSymbol.Color = result[6];
            }
            else
            {
                Random r = new Random();
                int r1 = r.Next(0, 256);
                int r2 = r.Next(0, 256);
                int r3 = r.Next(0, 256);
                pSimpleLineSymbol.Color = GetRgbColor(r1, r2, r3);
            }
            //更改符号样式
            ISimpleRenderer pSimpleRenderer = new SimpleRendererClass();
            pSimpleRenderer.Symbol = pSimpleLineSymbol as ISymbol;
            pGeoFeatLyr.Renderer = pSimpleRenderer as IFeatureRenderer;
        }

        //固定点的显示颜色
        public void PointReFill(IGeoFeatureLayer pGeoFeatLyr)
        {
            //设置面填充符号           
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            if (pGeoFeatLyr.Name.Contains("RESIDENTIAL"))
            {
                pSimpleMarkerSymbol.Color = GetRgbColor(155, 111, 100);

            }
            else if (pGeoFeatLyr.Name.Contains("SOIL"))
            {
                pSimpleMarkerSymbol.Color = GetRgbColor(155, 100, 1);
            }
            else if (pGeoFeatLyr.Name.Contains("VEGETATION"))
            {
                pSimpleMarkerSymbol.Color = GetRgbColor(55, 100, 1);
            }
            else if (pGeoFeatLyr.Name.Contains("WATER"))
            {
                pSimpleMarkerSymbol.Color = GetRgbColor(5, 10, 1);
            }
            else if (pGeoFeatLyr.Name.Contains("TRAFFIC"))
            {
                pSimpleMarkerSymbol.Color = GetRgbColor(15, 10, 100);
            }
            else
            {
                Random r = new Random();
                int r1 = r.Next(0, 256);
                int r2 = r.Next(0, 256);
                int r3 = r.Next(0, 256);
                pSimpleMarkerSymbol.Color = GetRgbColor(r1, r2, r3);
            }

            //更改符号样式
            ISimpleRenderer pSimpleRenderer = new SimpleRendererClass();
            pSimpleRenderer.Symbol = pSimpleMarkerSymbol as ISymbol;
            pGeoFeatLyr.Renderer = pSimpleRenderer as IFeatureRenderer;
        }
        #endregion

        //获取颜色的方法
        public IRgbColor GetRgbColor(int intR, int intG, int intB)
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

        //新版树形显示，减少了循环次数。进行树形显示的接口
        public void ShowShpFile(string path,List<string> selecttables)
        {
            //获取出库后的新文件存放的路径                
            string[] s = Directory.GetFiles(path);//获取路径下的全部文件（全路径）
            string[] filename = new string[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                filename[i] = System.IO.Path.GetFileName(s[i]);//获取文件的文件名和后缀名
            }

            //遍历选中的文件，与文件夹里面带shp后缀的比较，相同就输出显示
            string findFileName = "";//接收找到的相同的文件名，包含了后缀
            for (int i = 0; i < selecttables.Count; i++)//遍历选择的表名集合
            {
                bool sameFile = false;
                string tablename = selecttables[i] + ".shp";
                //找是否有相同名字的文件
                for (int j = 0; j < filename.Length; j++)
                {
                    if (tablename == filename[j])
                    {
                        //相同就输出显示
                        sameFile = true;
                        findFileName = filename[j];

                    }
                }
                if (sameFile == true)
                {
                    string findFile = findFileName.Substring(0, findFileName.IndexOf('.'));//去除文件名中.shp字符串,目的是为了根据这个名字操作后面的图层组名字
                    if (findFile.Contains("RESIDENTIAL") || findFile.Contains("SOIL") || findFile.Contains("VEGETATION") || findFile.Contains("WATER") || findFile.Contains("TRAFFIC"))
                    {
                        //判断选择的表是否包含了要素关键字，如果包含了就添加图层组，如果没有就直接添加图层
                        if (!isLoadSameLayer(findFile))//如果没有加载相同的图层就加载
                        {
                            IGroupLayer groupLayer = new GroupLayerClass();
                            string layerGroupName = "";//将英文要素名翻译成中文
                            if (!FindSameGroupLyers(findFile))//如果没有加载相同的图层组
                            {
                                axMapcontrol.Map.AddLayer(groupLayer);                                
                                groupLayer.Name = GetLayerGroupName(findFile, layerGroupName);
                                ILayer pLayer = AddShp2Map(path, findFileName);
                                groupLayer.Add(pLayer);
                                //UniqueValueRender(pLayer as IFeatureLayer, "OBJECTID");
                            }
                            else//如果加载了相同的图层组，则在已有的图层组下面添加
                            {
                                IGroupLayer plGroup = GetGroupLayersByName(findFile);//找到的图层组对象
                                ILayer pLayer = AddShp2Map(path, findFileName);//获取要显示的图层对象
                                plGroup.Add(pLayer);//在图层组下面加载显示
                               
                            }
                             
                        }
                    }
                    else
                    {
                        axMapcontrol.AddShapeFile(path, findFileName);
                    }
                }
            }
            
        }

        //获取shp文件的图层对象
        public ILayer AddShp2Map(string path, string shpfileName)
        { 
            IWorkspaceFactory workspaceFC = new ShapefileWorkspaceFactory();
            IWorkspace workSpace = workspaceFC.OpenFromFile(path, 0);
            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace) workSpace;
            IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(shpfileName);
            IFeatureLayer pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureClass;
            pFeatureLayer.Name = pFeatureClass.AliasName;
            return pFeatureLayer;
        }

        /// <summary>
        /// 将选中的表导出到文件夹，这是导出shp文件的主接口
        /// </summary>
        /// <param name="selecttables">选择导出的数据表</param>
        public void OSMToShp2(List<string> selecttables)
        {
            Control.CheckForIllegalCrossThreadCalls = false;//不对错误线程进行调用
            LoadTableNames(); //装载英文名对应的中文名                  
            string path = @"..\..\..\testfile";//获取文件路径
            //添加by DY
            string[] s1 = Directory.GetFiles(path);
            List<string> s2 = new List<string>();
            for (int i = 0; i < s1.Length; i++)
            {
                if (!s2.Contains(Path.GetFileNameWithoutExtension(s1[i])))
                s2.Add(Path.GetFileNameWithoutExtension(s1[i]));
            }

            for (int i = 0; i < selecttables.Count; i++)//遍历选择的表名集合
            {
                bool findFile = false;
                //遍历文件夹，找看是否存在相应的文件
                for (int j = 0; j < s2.Count; j++)
                {
                    if (s2[j].Equals(selecttables[i]))
                        findFile = true;
                }
                if (findFile == false)
                {
                    string tablename = selecttables[i];
                    OracleDBHelper odb = new OracleDBHelper();//新建连接
                    ShpHelper.GetShpFileByTableName(odb, tablename, path);//根据数据库表中数据生成shp文件，这是核心功能

                }
            }
            
        }

        //测试接口：唯一值符号化进行渲染
        public void UniqueValueRender(IFeatureLayer pFeatLyr, string sFieldName)
        {
            IGeoFeatureLayer pGeoFeatureLayer = pFeatLyr as IGeoFeatureLayer;
            ITable pTable = pFeatLyr as ITable;
            IUniqueValueRenderer pUniqueValueRender = new UniqueValueRenderer();
            int intFieldNumber = pTable.FindField(sFieldName);//根据字段查找属性表中字段的位置
            pUniqueValueRender.FieldCount = 1;//设置符号化关键字段的个数
            pUniqueValueRender.set_Field(0, sFieldName);//设置第一个渲染的字段
            IRandomColorRamp pRandomColorRamp = new RandomColorRampClass();//定义随机颜色对象
            pRandomColorRamp.StartHue = 0;
            pRandomColorRamp.MinValue = 0;
            pRandomColorRamp.MinSaturation = 15;
            pRandomColorRamp.EndHue = 360;
            pRandomColorRamp.MaxValue = 100;
            pRandomColorRamp.MaxSaturation = 30;
            IQueryFilter pQueryFilter = new QueryFilterClass();
            pRandomColorRamp.Size = pFeatLyr.FeatureClass.FeatureCount(pQueryFilter);//根据筛选条件获得图层的个数，作为颜色的个数
            bool bSuccess = false;
            pRandomColorRamp.CreateRamp(out bSuccess);
            IEnumColors pEnumRamp = pRandomColorRamp.Colors;
            IColor pNextUniqueColor = null;
            //查询字段的值
            pQueryFilter = new QueryFilterClass();
            pQueryFilter.AddField(sFieldName);//将查询字段赋值给查询条件
            ICursor pCursor = pTable.Search(pQueryFilter, true);//在属性表中根据筛选条件来找值
            IRow pNextRow = pCursor.NextRow();//遍历下一行
            object codeValue = null;
            IRowBuffer pNextRowBuffer = null;//每一行的缓冲区
            while (pNextRow!=null)
            {
                pNextRowBuffer = pNextRow as IRowBuffer;//讲不为空的行存储到缓冲区中
                codeValue = pNextRowBuffer.get_Value(intFieldNumber);//根据字段的位置获取相应的字段值
                pNextUniqueColor = pEnumRamp.Next();//将随机生成的颜色赋值给颜色对象
                if (pNextUniqueColor==null)
                {
                    pEnumRamp.Reset();//如果颜色对象没有值的话就重新生成颜色
                    pNextUniqueColor = pEnumRamp.Next();
                }
                IFillSymbol pFillSymbol = null;
                ILineSymbol pLineSymbol;
                IMarkerSymbol pMarkerSymbol;
                switch (pGeoFeatureLayer.FeatureClass.ShapeType)
                {                                   
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                        pMarkerSymbol = new SimpleMarkerSymbolClass();
                        pMarkerSymbol.Color = pNextUniqueColor;//将前面存储的颜色赋值给面颜色
                        pUniqueValueRender.AddValue(codeValue.ToString(), "", pMarkerSymbol as ISymbol);
                        pNextRow = pCursor.NextRow();//换下一个行
                        break;
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                         pFillSymbol = new SimpleFillSymbolClass();
                        pFillSymbol.Color = pNextUniqueColor;//将前面存储的颜色赋值给面颜色
                        pUniqueValueRender.AddValue(codeValue.ToString(),"",pFillSymbol as ISymbol);
                        pNextRow = pCursor.NextRow();//换下一个行
                        break;
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                        pLineSymbol = new SimpleLineSymbolClass();
                        pLineSymbol.Color = pNextUniqueColor;//将前面存储的颜色赋值给面颜色
                        pUniqueValueRender.AddValue(codeValue.ToString(), "", pLineSymbol as ISymbol);
                        pNextRow = pCursor.NextRow();//换下一个行
                        break;                   
                    default:
                        break;
                }
            }
            pGeoFeatureLayer.Renderer = pUniqueValueRender as IFeatureRenderer;
            axMapcontrol.Refresh();
            axTOCControl.Update();
        }

        //读取外部储存颜色的文件，用以渲染
        public List<IRgbColor> ReadColorFile()
        {
            string filePath = @"..\..\..\fileImport\colorSeting.txt";
            //using (FileStream fs=new FileStream(filePath,FileMode.Open,FileAccess.Read))
            //{
            //    byte[] buffer = new byte[1024 * 1024 * 5];
            //    int r = fs.Read(buffer,0,buffer.Length);
            //    fileText = Encoding.Default.GetString(buffer,0,r);
            //}
            string[]contents=File.ReadAllLines(filePath,Encoding.Default);//读取外部文件所有行的信息
            List<IRgbColor> a = new List<IRgbColor>();//定义返回的颜色集合
            for (int i = 0; i < contents.Length; i++)
            {
                //遍历每一行的信息
                string s = contents[i];
                if (i!=0&&i!=6)//如果是没有带颜色设置的那两行就不执行
                {
                    int r = s.IndexOf(':');
                    s=s.Substring(r+1);//截取冒号以后的内容
                    string []s2=s.Split(',');//截取逗号分隔开的数字
                    List<int> result = new List<int>();//定义集合，接受字符串转为int型的内容
                    for (int j = 0; j < s2.Length; j++)
                    {
                        //将转换后结果装到数组之中
                        int m = int.Parse(s2[j]);
                        result.Add(m);
                    }
                    int a1 = result[0];
                    int a2 = result[1];
                    int a3 = result[2];
                    a.Add(GetRgbColor(a1, a2, a3));
                    
                }
            }
            return a;

        }

        public void ThreadFun()
        {
            //线程操作
            //入库完成之后，清空缓存的文件byZYH
            string path = @"..\..\..\testfile";
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
        //新版删除缓存文件里面的值
        public static void DeleteFolder()
        {
            string dir = @"..\..\..\testfile";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);//直接删除其中的文件  
                }
                //else
                //{
                //    DirectoryInfo d1 = new DirectoryInfo(d);
                //    if (d1.GetFiles().Length != 0)
                //    {
                //        DeleteFolder(d1.FullName);////递归删除子文件夹
                //    }
                //    Directory.Delete(d);
                //}
            }
        } 
        //需要启动线程的方法
        
        public static void Fun1()
        {
            Thread m_thread = new Thread(DeleteFolder);
            m_thread.Start();
        }

    }
}
