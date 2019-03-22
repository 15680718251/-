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

namespace GIS.UI.Forms
{
    public partial class OSMToShapeForm : Form
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

         
            public OSMToShapeForm(string conStr)//构造函数，进行初始化操作，将连接数据库的字符串传过来
            {
                InitializeComponent();
                this.conStr = conStr;//接收传过来的链接参数
                List<string> tables = DataBaseProcess.TablesQuery();//调用静态类的获取数据库中数据表的方法
                if (tables.Count != 0)//判断表中是否有数据
                {
                    for (int i = 0; i < tables.Count; i++)//循环遍历
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
                this.toolStripProgressBar1.Maximum = tableNames.Count;//表名的数量就是工具栏进度条的最大值
                string path = textBox1.Text.ToString().TrimEnd();//获取文件路径
                //添加by DY

                for (int i = 0; i < selecttables.Count; i++)//遍历选择的表名集合
                {
                    string tablename = selecttables[i];
                    this.lblShpTip.Text = "正在导出【" + tablename + "】表中的数据";
                    this.toolStripProgressBar1.Value = i + 1;
                    OracleDBHelper odb = new OracleDBHelper();//新建连接
                    ShpHelper.GetShpFileByTableName(odb, tablename, path);//根据数据库表中数据生成shp文件，这是核心功能

                }
                this.lblShpTip.Text = "导出完成！";
                this.toolStripProgressBar1.Value = 0;
                this.Enabled = true;

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
            else if (textBox1.Text == "")
            {
                MessageBox.Show("未选择存储路径！");
                return;
            }
            else//没有选择内容或者没有选择路径，那么导出按钮就无法下按
            {

                this.Enabled = false;


                Thread t = new Thread(OSMtoShp);//新建一个线程来执行转shape操作，避免假死状态
                t.Start();//标记新线程的开始


            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog form = new FolderBrowserDialog();
            form.ShowDialog();
            textBox1.Text = form.SelectedPath;
        }
    }
}
