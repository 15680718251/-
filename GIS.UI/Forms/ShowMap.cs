using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIS.UI.OSMModelTrans;
using GIS.UI.AdditionalTool;
using System.Threading;
using System.IO;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesGDB;
namespace GIS.UI.Forms
{
    public partial class ShowMap : Form
    {
            List<string> baseLayer = new List<string>();
            List<string> oscLayer = new List<string>();
            List<string> newLayer = new List<string>();
            List<string> pointLayer = new List<string>();
            List<string> lineLayer = new List<string>();
            List<string> areaLayer = new List<string>();
            List<string> osc_pointLayer = new List<string>();
            List<string> osc_lineLayer = new List<string>();
            List<string> osc_areaLayer = new List<string>();
            List<string> new_pointLayer = new List<string>();
            List<string> new_lineLayer = new List<string>();
            List<string> new_areaLayer = new List<string>();
            List<string> selecttables = new List<string>();//选中的表格
            private string conStr = "";
            string path = @"..\..\..\testfile";
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



            public ShowMap(string conStr, AxMapControl axMapcontrol, AxTOCControl axTOCControl)//构造函数，进行初始化操作，将连接数据库的字符串传过来
            {
                InitializeComponent();
                this.conStr = conStr;//接收传过来的链接参数
                this.axMapcontrol = axMapcontrol;
                this.axTOCControl = axTOCControl;
                List<string> tables = DataBaseProcess.TablesQuery();//调用静态类的获取数据库中数据表的方法
                if (tables.Count != 0)//判断表中是否有数据
                {
                    //for (int i = 0; i < tables.Count; i++)
                    //{
                    //    DataGridViewRow row = new DataGridViewRow();
                    //    dataGridView1.Rows.Add(row); //获取一个集合，该集合包含 DataGridView 控件中的所有行
                    //    dataGridView1.Rows[i].Cells[1].Value = tables[i];//将表名读取到视图中的每一行上
                    //}

                    int j = 0;
                    for (int i = 0; i < tables.Count; i++)//循环遍历
                    {
                        if (tables[i].Contains("RESIDENTIAL") ||tables[i].Contains("SOIL")  ||tables[i].Contains("VEGETATION") || tables[i].Contains("WATER") || tables[i].Contains("TRAFFIC"))
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            dataGridView1.Rows.Add(row); //获取一个集合，该集合包含 DataGridView 控件中的所有行
                            dataGridView1.Rows[j].Cells[1].Value = tables[i];//将表名读取到视图中的每一行上
                            j++;
                        }

                    }
                }
                else
                {
                    MessageBox.Show("数据库中不存在数据表！");
                    this.Close();
                }
                //对获取的表名进行点线面及基本图层的分类

            }
            
            private List<string> selecttable()
            {
                List<string> result = new List<string>();
                if (dataGridView1.Rows.Count >= 1)//如果每一行都有元素
                {

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)//遍历这一行
                    {
                        if ((bool)dataGridView1.Rows[i].Cells[0].EditedFormattedValue == true)//判断每一行元素的第一个值是否是格式化的值
                        
                        {
                            result.Add(dataGridView1.Rows[i].Cells[1].Value.ToString());//如果是的话就将每一行的第二个元素添加到结果集合中
                        }

                    }
                }

                return result;//返回结果集合

            }
    

            /// <summary>
            /// 执行数据导出操作
            /// </summary>
            private void OSMtoShp()
            {
                Control.CheckForIllegalCrossThreadCalls = false;//不对错误线程进行调用
                LoadTableNames(); //装载英文名对应的中文名                  
                string path = @"..\..\..\testfile";//获取文件路径
                //添加by DY
                string[] s1 = Directory.GetFiles(path);
                
                for (int i = 0; i < selecttables.Count; i++)//遍历选择的表名集合
                {
                    bool findFile = false;
                    //遍历文件夹，找看是否存在相应的文件
                    for (int j = 0; j < s1.Length; j++)
                    {   
                        if(s1[j].Contains(selecttables[i]))
                            findFile=true;                       
                    }
                    if (findFile == false)
                    {
                        string tablename = selecttables[i];
                        OracleDBHelper odb = new OracleDBHelper();//新建连接
                        ShpHelper.GetShpFileByTableName(odb, tablename, path);//根据数据库表中数据生成shp文件，这是核心功能
                        
                    }                                       
                }                
                this.Enabled = true;

               

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
                    for (int i = 0; i < axMapcontrol.LayerCount;i++ )
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
                    if (pL as IGroupLayer==null)
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
                string grouplayerName="";
                IN_Name=GetLayerGroupName(IN_Name,grouplayerName);
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
                IGroupLayer plGroup=null;
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
            public Dictionary<string, string> tableNames = new Dictionary<string, string>();//新建一个字典集合
            /// <summary>
            /// 对应tables中文名称查找英文名称
            /// </summary>
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

            private void button1_Click(object sender, EventArgs e)
            {
             
                selecttables = selecttable();//获取选择的要导出的表名
                if (selecttables.Count == 0)
                {
                    MessageBox.Show("未选择数据库中的表！");
                    return;
                }               
                else//没有选择内容或者没有选择路径，那么导出按钮就无法下按
                {

                    this.Enabled = false;


                    //Thread t = new Thread(OSMtoShp);//新建一个线程来执行转shape操作，避免假死状态
                    //t.Start();//标记新线程的开始
                    //OSMtoShp();
                    AddMap addmap = new AddMap(axMapcontrol,axTOCControl);//将两个控件传过去
                    addmap.OSMToShp2(selecttables);//导出shp文件到文件夹
                    addmap.ShowShpFile(path, selecttables);//进行图层树显示                    
                    this.Enabled = true;//使按钮能够点击
                    addmap.ss();//进行颜色固定显示
                    MessageBox.Show("显示成功");


                }
            }

            private void radioButton1_CheckedChanged(object sender, EventArgs e)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)//遍历这一行
                {
                    dataGridView1.Rows[i].Cells[0].Value = "true";
                }

            }

            private void radioButton2_CheckedChanged(object sender, EventArgs e)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)//遍历这一行
                {
                    dataGridView1.Rows[i].Cells[0].Value = "false";
                }
            }

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

            private void checkBox1_CheckedChanged(object sender, EventArgs e)
            {
                List<string> tables = DataBaseProcess.TablesQuery();//调用静态类的获取数据库中数据表的方法
                if (checkBox1.Checked==true)
                {
                int m = 0;
                while (m >= 0)
                {
                    m = dataGridView1.RowCount - 1;
                    dataGridView1.Rows.Remove(dataGridView1.Rows[m]);
                    m--;
                }
                    
                    if (tables.Count != 0)//判断表中是否有数据
                    {
                        for (int i = 0; i < tables.Count; i++)
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            dataGridView1.Rows.Add(row); //获取一个集合，该集合包含 DataGridView 控件中的所有行
                            dataGridView1.Rows[i].Cells[1].Value = tables[i];//将表名读取到视图中的每一行上
                        }

                       
                    }
                    else
                    {
                        MessageBox.Show("数据库中不存在数据表！");
                        this.Close();
                    }
                    //对获取的表名进行点线面及基本图层的分类
                }
                else//如果不显示所有的表，则还是默认显示要素表
                {
                    int m = 0;
                    while (m>=0)
                    {
                        m = dataGridView1.RowCount - 1;
                        dataGridView1.Rows.Remove(dataGridView1.Rows[m]);
                        m--;
                    }
                    dataGridView1.DataSource = null;
                    int j = 0;
                    for (int i = 0; i < tables.Count; i++)//循环遍历
                    {
                        if (tables[i].Contains("RESIDENTIAL") || tables[i].Contains("SOIL") || tables[i].Contains("VEGETATION") || tables[i].Contains("WATER") || tables[i].Contains("TRAFFIC"))
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            dataGridView1.Rows.Add(row); //获取一个集合，该集合包含 DataGridView 控件中的所有行
                            dataGridView1.Rows[j].Cells[1].Value = tables[i];//将表名读取到视图中的每一行上
                            j++;
                        }

                    }
                }

            }
               
            
    }
}
